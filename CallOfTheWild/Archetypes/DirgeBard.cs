using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker;
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
    public class DirgeBard
    {
        static public BlueprintArchetype archetype;
        static public BlueprintFeature dance_of_the_dead;

        static public BlueprintFeature haunted_eyes;
        static public BlueprintFeatureSelection secrets_of_the_grave;
        static public BlueprintFeature haunting_refrain;

        static LibraryScriptableObject library => Main.library;

        internal static void create()
        {
            var bard_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "DirgeBardArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Dirge Bard");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "A composer of sonorous laments for the dead and elaborate requiems for those lost yet long remembered, dirge bards master musical tools and tropes that must appeal to the ears and hearts of both the living and the dead.");
            });
            Helpers.SetField(archetype, "m_ParentClass", bard_class);
            library.AddAsset(archetype, "");

            var well_versed = library.Get<BlueprintFeature>("8f4060852a4c8604290037365155662f");
            var versatile_performance = library.Get<BlueprintFeatureSelection>("94e2cd84bf3a8e04f8609fe502892f4f");
            var jack_of_all_trades = library.Get<BlueprintFeature>("21fbafd5dc42d4d488c4d6caed46bc99");

            createDanceOfTheDead();
            createHauntingRefrain();
            createSecretsOfTheGrave();
            createHauntedEyes();

            archetype.RemoveFeatures = new LevelEntry[] { 
                                                          Helpers.LevelEntry(2, well_versed, versatile_performance), 
                                                          Helpers.LevelEntry(6, versatile_performance), 
                                                          Helpers.LevelEntry(10, versatile_performance, jack_of_all_trades), 
                                                          Helpers.LevelEntry(14, versatile_performance),
                                                          Helpers.LevelEntry(18, versatile_performance), 
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(2, secrets_of_the_grave),
                                                       Helpers.LevelEntry(5, haunting_refrain),
                                                       Helpers.LevelEntry(6, secrets_of_the_grave),
                                                       Helpers.LevelEntry(10, secrets_of_the_grave, dance_of_the_dead),
                                                       Helpers.LevelEntry(14, secrets_of_the_grave),
                                                       Helpers.LevelEntry(18, secrets_of_the_grave)
                                                     };

            bard_class.Progression.UIGroups = bard_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(haunted_eyes, haunting_refrain, dance_of_the_dead));
            bard_class.Archetypes = bard_class.Archetypes.AddToArray(archetype);
        }


        static void createDanceOfTheDead()
        {
            var bard_resource = library.Get<BlueprintAbilityResource>("e190ba276831b5c4fa28737e5e49e6a6");
            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("4a15b95f8e173dc4fb56924fe5598dcf", "DanceOfTheDeadAreaEffect", ""); //dirge of doom

            var dance_of_the_dead_mark = Helpers.CreateBuff("DanceOfTheDeadMarkBuff",
                                                            "",
                                                            "",
                                                            "",
                                                            null,
                                                            null);
            dance_of_the_dead_mark.SetBuffFlags(BuffFlags.HiddenInUi);

            var apply_mark = Common.createContextActionApplyBuff(dance_of_the_dead_mark, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);
            var animate_action = NewSpells.animate_dead_lesser.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as DeadTargetMechanics.ContextActionAnimateDead;

            animate_action = animate_action.CreateCopy(a => a.AfterSpawn = Helpers.CreateActionList(a.AfterSpawn.Actions.AddToArray(apply_mark)));
            area.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.CreateAreaEffectRunAction(
                                                  round:  Helpers.CreateConditional(Helpers.Create<DeadTargetMechanics.ContextConditionCanBeAnimated>(), animate_action),
                                                  unitEnter: Helpers.CreateConditional(Helpers.Create<DeadTargetMechanics.ContextConditionCanBeAnimated>(), animate_action),
                                                  unitExit: Helpers.CreateConditional(Common.createContextConditionHasBuffFromCaster(dance_of_the_dead_mark), Common.createContextActionRemoveBuff(Game.Instance.BlueprintRoot.SystemMechanics.SummonedUnitBuff))
                                                  ),
                Common.createContextCalculateAbilityParamsBasedOnClasses(new BlueprintCharacterClass[]{archetype.GetParentClass()}, StatType.Charisma)
            };

            var buff = Helpers.CreateBuff("DanceOfTheDeadBuff",
                                          "Dance of the Dead",
                                          "At 10th level, a dirge bard can use his bardic performance to cause dead bones or bodies to rise up and move or fight at his command. This ability functions like animate dead, but the created skeletons or zombies remain fully animate only as long as the dirge bard continues the performance. Once it stops, any created undead collapse into carrion. Bodies or bones cannot be animated more than once using this ability.",
                                          "",
                                          NewSpells.animate_dead_lesser.Icon,
                                          Common.createPrefabLink("39da71647ad4747468d41920d0edd721"),
                                          Common.createAddAreaEffect(area)
                                          );

            var toggle = Helpers.CreateActivatableAbility("DanceOfTheDeadToggleAbility",
                                                          buff.Name,
                                                          buff.Description,
                                                          "",
                                                          buff.Icon,
                                                          buff,
                                                          AbilityActivationType.Immediately,
                                                          CommandType.Standard,
                                                          null,
                                                          bard_resource.CreateActivatableResourceLogic(ResourceSpendType.NewRound)
                                                          );
            toggle.Group = ActivatableAbilityGroup.BardicPerformance;
            toggle.WeightInGroup = 1;

            dance_of_the_dead = Common.ActivatableAbilityToFeature(toggle, false);
        }

        static void createHauntingRefrain()
        {
            haunting_refrain = Helpers.CreateFeature("HauntingRefrainFeature",
                                                     "Haunting Refrain",
                                                     "At 5th level, a dirge bard is able to stir primal terrors in the hearts of listeners. He receives a bonus to Intimidate checks equal to half his bard level. In addition, saving throws against any fear effect he creates are made with a –2 penalty, and this penalty increases by –1 every 5 levels beyond 5th.",
                                                     "",
                                                     Helpers.GetIcon("d2aeac47450c76347aebbc02e4f463e0"), //fear spell
                                                     FeatureGroup.None,
                                                     Helpers.CreateAddContextStatBonus(StatType.CheckIntimidate, ModifierDescriptor.UntypedStackable),
                                                     Helpers.Create<NewMechanics.ContextIncreaseDescriptorSpellsDC>(i =>
                                                                                                                     {
                                                                                                                         i.only_spells = false;
                                                                                                                         i.Value = Helpers.CreateContextValue(AbilityRankType.StatBonus);
                                                                                                                         i.Descriptor = SpellDescriptor.Fear | SpellDescriptor.Shaken;
                                                                                                                     }
                                                                                                                     ),
                                                     Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] {archetype.GetParentClass()},
                                                                                     progression: ContextRankProgression.Div2),
                                                     Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { archetype.GetParentClass() },
                                                                                     progression: ContextRankProgression.DivStep, type: AbilityRankType.StatBonus,  stepLevel: 5) 
                                                     );
            haunting_refrain.ReapplyOnLevelUp = true;
        }


        static void createSecretsOfTheGrave()
        {
            var bloodline_undead_arcana = library.Get<BlueprintFeature>("1a5e7191279e7cd479b17a6ca438498c");

            var wizard = library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
            var bard = archetype.GetParentClass();
            var spell_list = Common.combineSpellLists("DirgeBardBonusNecromancySpellsList", Witch.witch_class.Spellbook.SpellList, wizard.Spellbook.SpellList, bard.Spellbook.SpellList);
            Common.filterSpellList(spell_list, s => s.School == SpellSchool.Necromancy);

            var spell_selection = library.CopyAndAdd<BlueprintParametrizedFeature>("4a2e8388c2f0dd3478811d9c947bebfb", "DirgeBardExtraNecromancySpell", "");
            spell_selection.SetNameDescriptionIcon("Secrets of the Grave",
                                                   "At 2nd level, a dirge bard gains a bonus equal to half his bard level on Knowledge (religion) checks made to identify undead creatures and their abilities. A dirge bard may use mind-affecting spells to affect undead as if they were living creatures, even if they are mindless. In addition, he may add one necromancy spell from the spell list of any arcane spellcasting class to his list of spells known at 2nd level and every four levels thereafter.",
                                                   Helpers.GetIcon("a9bb3dcb2e8d44a49ac36c393c114bd9")
                                                   );

            spell_selection.ComponentsArray = new BlueprintComponent[] { Helpers.CreateLearnSpell(spell_list, archetype.GetParentClass()) };
            spell_selection.BlueprintParameterVariants = Common.getSpellsFromSpellList(spell_list);
            spell_selection.SpellList = spell_list;
            spell_selection.SpellcasterClass = archetype.GetParentClass();

            secrets_of_the_grave = Common.featureToSelection(spell_selection);
            secrets_of_the_grave.AddComponent(Helpers.CreateAddFact(bloodline_undead_arcana));
        }


        static void createHauntedEyes()
        {
            haunted_eyes = Helpers.CreateFeature("HauntedEyesFeautre",
                                                 "Haunted Eyes",
                                                 "At 2nd level, a dirge bard gains a +4 bonus on saves against fear, energy drain, death effects, and necromantic effects.",
                                                 "",
                                                 Helpers.GetIcon("582009cf6013790469d6e98e5210477a"), //eyebite
                                                 FeatureGroup.None,
                                                 Helpers.Create<NewMechanics.SavingThrowBonusAgainstSchoolOrDescriptor>(s =>
                                                                                                         {
                                                                                                             s.School = SpellSchool.Necromancy;
                                                                                                             s.ModifierDescriptor = ModifierDescriptor.UntypedStackable;
                                                                                                             s.SpellDescriptor = SpellDescriptor.Fear | SpellDescriptor.Shaken | SpellDescriptor.Death;
                                                                                                             s.Value = 4;
                                                                                                         })
                                                 );
        }
    }
}
