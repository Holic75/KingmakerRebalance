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
    public class Seeker
    {
        static BlueprintArchetype archetype;
        static Dictionary<BlueprintProgression, BlueprintProgression> normal_seeker_map = new Dictionary<BlueprintProgression, BlueprintProgression>();
        static BlueprintFeature tinkering;
        static BlueprintFeature seeker_lore;
        static BlueprintFeature seeker_magic;

        static LibraryScriptableObject library => Main.library;


        public static void create()
        {
            var sorcerer = library.Get<BlueprintCharacterClass>("b3a505fb61437dc4097f43c3f8f9a4cf");


            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "SeekerSorcererArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Seeker");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Sorcerers gain their magical powers through strange and mysterious ways. While most might be content with their strange powers, some adventure far and wide in search of a greater understanding of the genesis and history of their eldritch talents. These spellcasters are known as seekers, after their obsession with researching ancient texts and obscure ruins for any clues they can find about their heritage and histories.");
            });
            Helpers.SetField(archetype, "m_ParentClass", sorcerer);
            library.AddAsset(archetype, "");

            var bonus_feat = library.Get<BlueprintFeatureSelection>("d6dd06f454b34014ab0903cb1ed2ade3");
            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, bonus_feat), //original bloodline and bonus feat
                                                       };

            createTinkering();
            createSeekerLore();
            createSeekerMagic();
            createSeekerBloodlines();


            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, tinkering),
                                                       Helpers.LevelEntry(3, seeker_lore),
                                                       Helpers.LevelEntry(15, seeker_magic),};

            sorcerer.Archetypes = sorcerer.Archetypes.AddToArray(archetype);
            sorcerer.Progression.UIGroups = sorcerer.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(tinkering, seeker_lore, seeker_magic));

            //fix dragon disciple
            fixDragonDisciple();
            //var dragon_disicple = library.Get<BlueprintCharacterClass>("72051275b1dbb2d42ba9118237794f7c");
            //dragon_disicple.AddComponent(Common.prerequisiteNoArchetype(sorcerer, archetype));

        }


        static void fixDragonDisciple()
        {
            var dragon_disciple = library.Get<BlueprintCharacterClass>("72051275b1dbb2d42ba9118237794f7c");
            dragon_disciple.AddComponent(Common.prerequisiteNoArchetype(archetype.GetParentClass(), archetype));
        }


        static void createTinkering()
        {
            tinkering = library.CopyAndAdd<BlueprintFeature>("dbb6b3bffe6db3547b31c3711653838e", "SeekerSorcererTinkeringFeature", "");
            tinkering.SetNameDescription("Tinkering",
                                         "Seekers often look to ancient devices, old tomes, and strange magical items in order to learn more about their oracle mysteries. As a result of this curiosity and thanks to an innate knack at deciphering the strange and weird, a seeker gains Trickery as a class skill. In addition, at 1st level, a seeker adds half his oracle level on Perception checks made to locate traps and on all Trickery skill checks (minimum +1). A seeker can use Disable Device to disarm magical traps. If the seeker also possesses levels in rogue or another class that provides the trapfinding ability, those levels stack with his oracle levels for determining his overall bonus on these skill checks.\n"
                                         + "This ability replaces all of the bonus class skills he would otherwise normally gain from his mystery.\n"
                                         );
            tinkering.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", new BlueprintCharacterClass[] { archetype.GetParentClass() }));
            tinkering.AddComponent(Helpers.Create<AddClassSkill>(a => a.Skill = StatType.SkillThievery));
        }


        static void createSeekerLore()
        {
            seeker_lore = Helpers.CreateFeature("SorcererSeekerLoreFeature",
                                                "Seeker Lore",
                                                "By 3rd level, a seeker has already learned much about his mystery, and is more comfortable using the bonus spells gained by that mystery. He gains a +4 bonus on all concentration checks and on caster level checks made to overcome spell resistance.",
                                                "",
                                                null,
                                                FeatureGroup.None);
        }

        static void createSeekerMagic()
        {
            seeker_magic = Helpers.CreateFeature("SorcererSeekerMagicFeature",
                                                 "Seeker Magic",
                                                 "At 15th level, a seeker becomes skilled at modifying his mystery spells with metamagic. When a seeker applies a metamagic feat to any bonus spells granted by his mystery, he reduces the metamagic feat’s spell level adjustment by 1. Thus, applying a Metamagic feat like Still Spell to a spell does not change its effective spell level at all, while applying Quicken Spell only increases the spell’s effective spell level by 3 instead of by 4. This reduction to the spell level adjustment for Metamagic feats does not stack with similar reductions from other abilities.",
                                                 "",
                                                 Helpers.GetIcon("3524a71d57d99bb4b835ad20582cf613"),
                                                 FeatureGroup.None);
        }

        static void createSeekerBloodlines()
        {
            var dragon_disciple = library.Get<BlueprintCharacterClass>("72051275b1dbb2d42ba9118237794f7c");
            var eldritch_scion = library.Get<BlueprintArchetype>("d078b2ef073f2814c9e338a789d97b73");
            var bloodlines = library.Get<BlueprintFeatureSelection>("24bef8d1bee12274686f6da6ccbc8914");


            foreach (var b in bloodlines.AllFeatures.ToArray())
            {
                var bp = b as BlueprintProgression;
                var seeker_bloodline = library.CopyAndAdd<BlueprintProgression>(bp, "Seeker" + bp.name, Helpers.MergeIds("2e9cb641e8e54d7e85739d66861de858", bp.AssetGuid));
                var level_entries = new List<LevelEntry>();
                var spells = new List<BlueprintAbility>();

                foreach (var le in bp.LevelEntries)
                {
                    if (le.Level == 3 || le.Level == 15)
                    {
                        level_entries.Add(Helpers.LevelEntry(le.Level, le.Features.Where(f => f.name.Contains("SpellLevel")).FirstOrDefault()));
                    }
                    else
                    {
                        level_entries.Add(le);
                    }
                    var spell = le.Features.Where(f => f.name.Contains("SpellLevel")).FirstOrDefault()?.GetComponent<AddKnownSpell>().Spell;
                    if (spell != null)
                    {
                        spells.Add(spell);
                    }
                }

                seeker_bloodline.LevelEntries = level_entries.ToArray();
                

                var feature = Helpers.CreateFeature(bp.name + "SorcererSeekerLoreFeature",
                                    "",
                                    "",
                                    Helpers.MergeIds("fc4f484c9b0441dc8d57fef0ca46656a", bp.AssetGuid),
                                    null,
                                    FeatureGroup.None,
                                    Helpers.Create<NewMechanics.CasterLevelChecksBonusForSpecifiedSpells>(c => { c.value = 4; c.spells = spells.ToArray(); })
                                    );
                feature.HideInCharacterSheetAndLevelUp = true;
                seeker_lore.AddComponent(Common.createAddFeatureIfHasFact(seeker_bloodline, feature));

                var feature2 = Helpers.CreateFeature(bp.name + "SeekerMagicFeature",
                                                    "",
                                                    "",
                                                    Helpers.MergeIds("8ced1f34b4294d42ac0efd57bdd051d7", bp.AssetGuid),
                                                    null,
                                                    FeatureGroup.None,
                                                    Helpers.Create<NewMechanics.MetamagicMechanics.ReduceMetamagicCostForSpecifiedSpells>(r => { r.reduction = 1; r.spells = spells.ToArray(); })
                                                    );
                feature2.HideInCharacterSheetAndLevelUp = true;
                seeker_magic.AddComponent(Common.createAddFeatureIfHasFact(seeker_bloodline, feature2));

                normal_seeker_map.Add(bp, seeker_bloodline);
                bloodlines.AllFeatures = bloodlines.AllFeatures.AddToArray(seeker_bloodline);
                bp.AddComponent(Common.prerequisiteNoArchetype(archetype.GetParentClass(), archetype));
                seeker_bloodline.AddComponent(Common.createPrerequisiteArchetypeLevel(archetype.GetParentClass(), archetype, 1));
                seeker_bloodline.AddComponent(Common.prerequisiteNoArchetype(eldritch_scion.GetParentClass(), eldritch_scion));
                seeker_bloodline.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = dragon_disciple));

                b.AddComponent(Helpers.Create<NewMechanics.FeatureReplacement>(f => f.replacement_feature = seeker_bloodline));
            }
        }

    }
}
