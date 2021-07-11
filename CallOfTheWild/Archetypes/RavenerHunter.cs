using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild.Archetypes
{
    public class RavenerHunter
    {
        static public BlueprintArchetype archetype;
        static public BlueprintFeatureSelection charged_by_nature;
        static public BlueprintFeatureSelection revelation_selection;
        static public BlueprintFeatureSelection holy_magic;
        static public BlueprintFeature demon_hunter;
        static public BlueprintSpellbook spellbook;

        static LibraryScriptableObject library => Main.library;

        internal static void create()
        {
            var inquisitor_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "RavenerHunterArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Demon Hunter");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "The natural enemies of demoniacs and other cultists of fiendish forces, demon hunters are inquisitors who dedicate their lives to eradicating demonkind.");
            });
            Helpers.SetField(archetype, "m_ParentClass", inquisitor_class);
            library.AddAsset(archetype, "");

            var deity = library.Get<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4");
            var domain = library.Get<BlueprintFeatureSelection>("48525e5da45c9c243a343fc6545dbdb9");
            var teamwork_feat = library.Get<BlueprintFeature>("d87e2f6a9278ac04caeb0f93eff95fcb");
            var solo_tactics = library.Get<BlueprintFeature>("5602845cd22683840a6f28ec46331051");
            var solo_tactics_ravener = library.CopyAndAdd(solo_tactics, "SoloTacticsRavenerHunter", "");
            solo_tactics_ravener.SetDescription("A demon hunter gains solo tactics at 6th level instead of 3rd level.");
            createHolyMagic();
            createDemonHunter();
            createChargedByNatureAndRevelations();

            archetype.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, domain),
                                                          Helpers.LevelEntry(3, teamwork_feat, solo_tactics),
                                                       };
            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, holy_magic, charged_by_nature, revelation_selection),
                                                       Helpers.LevelEntry(3, demon_hunter),
                                                       Helpers.LevelEntry(6, solo_tactics_ravener),
                                                       Helpers.LevelEntry(8, revelation_selection),
                                                    };

            archetype.ReplaceSpellbook = spellbook;
            archetype.AddComponent(Common.createPrerequisiteAlignment((~Kingmaker.UnitLogic.Alignments.AlignmentMaskType.Evil) & Kingmaker.UnitLogic.Alignments.AlignmentMaskType.Any));

            inquisitor_class.Progression.UIGroups = inquisitor_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(revelation_selection, demon_hunter, solo_tactics_ravener));
            inquisitor_class.Progression.UIDeterminatorsGroup = inquisitor_class.Progression.UIDeterminatorsGroup.AddToArray(holy_magic, charged_by_nature);
            inquisitor_class.Archetypes = inquisitor_class.Archetypes.AddToArray(archetype);

            var mt_progression = library.Get<BlueprintProgression>("d21a104c204ed7348a51405e68387013");
            mt_progression.AddComponent(Common.prerequisiteNoArchetype(archetype));
            Common.addMTDivineSpellbookProgression(archetype.GetParentClass(), spellbook, "MysticTheurgeRavenerHunterProgression", 
                                                   Common.createPrerequisiteArchetypeLevel(archetype, 1),
                                                   Common.createPrerequisiteClassSpellLevel(inquisitor_class, 2)
                                                   );
        }


        static void createChargedByNatureAndRevelations()
        {
            charged_by_nature = Helpers.CreateFeatureSelection("ChargedByNatureSelection",
                                                  "Charged By Nature",
                                                  "Rather than having a deity patron, a demon hunter is charged by spirits to eradicate evil wherever it appears. At 1st level, a demon hunter chooses an oracle mystery from the following list: ancestor, battle, flame, life, nature, time, wave or wind. She gains one revelation from her chosen mystery. She must meet the revelation’s prerequisites, using her inquisitor level as her effective oracle level to determine the revelation’s effects, and she never qualifies for the Extra Revelation feat. The demon hunter gains a second revelation from her chosen mystery at 8th level.",
                                                  "",
                                                  null,
                                                  FeatureGroup.Domain);

            revelation_selection = Helpers.CreateFeatureSelection("RavenerHunterRevelationSelection",
                                                                  "Revelation",
                                                                  charged_by_nature.Description,
                                                                  "",
                                                                  null,
                                                                  FeatureGroup.None);
        }

        static void createDemonHunter()
        {
            demon_hunter = Helpers.CreateFeature("RavenHunterDemonHunter",
                                                 "Demon Hunter",
                                                 "At 3rd level demon hunter receives +2 moral bonus on attack rolls and caster level checks to overcome spell resistance against chaotic evil outsiders.",
                                                 "",
                                                 Helpers.GetIcon("ce0ece459ebed9941bb096f559f36fa8"),
                                                 FeatureGroup.None,
                                                 Helpers.Create<NewMechanics.AttackBonusAgainstFactAndAlignment>(a =>
                                                                                                                 {
                                                                                                                     a.alignment = Alignment.ChaoticEvil;
                                                                                                                     a.CheckedFact = Common.outsider;
                                                                                                                     a.AttackBonus = 0;
                                                                                                                     a.Bonus = 2;
                                                                                                                     a.Descriptor = ModifierDescriptor.Morale;
                                                                                                                 }),
                                                 Helpers.Create<NewMechanics.SpellPenetrationBonusAgainstFactAndAlignment>(a =>
                                                                                                  {
                                                                                                      a.alignment = Alignment.ChaoticEvil;
                                                                                                      a.fact = Common.outsider;
                                                                                                      a.value = 2;
                                                                                                  })
                                                 );
        }


        static void createHolyMagic()
        {
            var cleric_spell_list = library.Get<BlueprintSpellList>("8443ce803d2d31347897a3d85cc32f53");
            var inquisitor_spell_list = archetype.GetParentClass().Spellbook.SpellList;

            var ravener_spell_list = Common.combineSpellLists("ReavenerHunterSpellList",
                                                               (spell, spell_list, lvl) =>
                                                               {
                                                                   if ((spell.SpellDescriptor & (SpellDescriptor.Evil | SpellDescriptor.Law | SpellDescriptor.Chaos)) != 0)
                                                                   {
                                                                       return false;
                                                                   }
                                                                   if (lvl > 6)
                                                                   {
                                                                       return false;
                                                                   }
                                                                   if (spell_list == cleric_spell_list && (spell.SpellDescriptor & SpellDescriptor.Good) == 0)
                                                                   {
                                                                       return false;
                                                                   }
                                                                   return true;
                                                               },
                                                               inquisitor_spell_list, cleric_spell_list
                                                               );
            spellbook = library.CopyAndAdd(archetype.GetParentClass().Spellbook, "RavenerHunterSpellbook", "");
            spellbook.SpellList = ravener_spell_list;

            var feature = Helpers.CreateFeature("RavenerHunterHolyMagic",
                                               "Holy Magic",
                                               "A demon hunter must be non-evil. She adds all spells of 6th-level and lower on the cleric spell list with the good descriptor to her inquisitor spell list as inquisitor spells of the same level. If a spell appears on both the cleric and inquisitor spell lists, the demon hunter uses the lower of the two spell levels listed for the spell. She cannot cast a spell with the chaotic, evil, or lawful descriptors, even from spell trigger or spell completion items.",
                                               "",
                                               Helpers.GetIcon("808ab74c12df8784ab4eeaf6a107dbea"), //protection from evil
                                               FeatureGroup.None,
                                               Common.createPrerequisiteAlignment(AlignmentMaskType.Good | AlignmentMaskType.ChaoticNeutral | AlignmentMaskType.TrueNeutral | AlignmentMaskType.LawfulNeutral)
                                               );
            holy_magic = Common.featureToSelection(feature);
            holy_magic.Obligatory = true;
        }
    }

}