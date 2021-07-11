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
using Kingmaker.Blueprints.Items.Shields;
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
    public class UntamedRager
    {
        static public BlueprintArchetype archetype;

        static public BlueprintFeature despicable_tactics;
        static public BlueprintFeature deplorable_tactics;
        static public BlueprintFeature feral_appearance;
        static public BlueprintFeature dishonorable;

        static LibraryScriptableObject library => Main.library;

        static public void create()
        {
            var barbarian_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f7d7eb166b3dd594fb330d085df41853");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "UntamedRagerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Untamed Rager");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "There are no rules in the wild. Some barbarians enter combat with only victory in mind and do anything in their power to achieve it.");
            });
            Helpers.SetField(archetype, "m_ParentClass", barbarian_class);
            library.AddAsset(archetype, "");

            createDirtyTrickTactics();
            createFeralAppearance();
            createDishonorable();

            var uncanny_dodge = library.Get<BlueprintFeature>("3c08d842e802c3e4eb19d15496145709");
            var improved_uncanny_dodge = library.Get<BlueprintFeature>("485a18c05792521459c7d06c63128c79");
            var danger_sense = library.Get<BlueprintFeature>("fdd591c1fbf1c0b41a359d59756f2888");
            var damage_reduction = library.Get<BlueprintFeature>("cffb5cddefab30140ac133699d52a8f8");
            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(2, uncanny_dodge),
                                                          Helpers.LevelEntry(3, danger_sense),
                                                          Helpers.LevelEntry(5, improved_uncanny_dodge),
                                                          Helpers.LevelEntry(6, danger_sense),
                                                          Helpers.LevelEntry(7, damage_reduction),
                                                          Helpers.LevelEntry(9, danger_sense),
                                                          Helpers.LevelEntry(10, damage_reduction),
                                                          Helpers.LevelEntry(12, danger_sense),
                                                          Helpers.LevelEntry(13, damage_reduction),
                                                          Helpers.LevelEntry(15, danger_sense),
                                                          Helpers.LevelEntry(16, damage_reduction),
                                                          Helpers.LevelEntry(18, danger_sense),
                                                          Helpers.LevelEntry(19, damage_reduction),
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(2, despicable_tactics),
                                                          Helpers.LevelEntry(3, feral_appearance),
                                                          Helpers.LevelEntry(5, deplorable_tactics),
                                                          Helpers.LevelEntry(6, feral_appearance),
                                                          Helpers.LevelEntry(7, dishonorable),
                                                          Helpers.LevelEntry(9, feral_appearance),
                                                          Helpers.LevelEntry(10, dishonorable),
                                                          Helpers.LevelEntry(12, feral_appearance),
                                                          Helpers.LevelEntry(13, dishonorable),
                                                          Helpers.LevelEntry(15, feral_appearance),
                                                          Helpers.LevelEntry(16, dishonorable),
                                                          Helpers.LevelEntry(18, feral_appearance),
                                                          Helpers.LevelEntry(19, dishonorable),
                                                       };

            barbarian_class.Progression.UIGroups = barbarian_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(feral_appearance));
            barbarian_class.Progression.UIGroups = barbarian_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(dishonorable));
            barbarian_class.Progression.UIGroups = barbarian_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(despicable_tactics, deplorable_tactics));
            barbarian_class.Archetypes = barbarian_class.Archetypes.AddToArray(archetype);
        }


        static void createDirtyTrickTactics()
        {
            var improved_dirty_trick = library.Get<BlueprintFeature>("ed699d64870044b43bb5a7fbe3f29494");
            var greater_dirty_trick = library.Get<BlueprintFeature>("52c6b07a68940af41b270b3710682dc7");
            despicable_tactics = Helpers.CreateFeature("DespicableTacticsFeature",
                                                       "Despicable Tactics",
                                                       "At 2nd level, the untamed rager gains Dirty Trick as a bonus feat.",
                                                       "",
                                                       improved_dirty_trick.Icon,
                                                       FeatureGroup.None,
                                                       Helpers.CreateAddFact(improved_dirty_trick)
                                                       );

            deplorable_tactics = Helpers.CreateFeature("DeplorableTacticsFeature",
                                           "Deplorable Tactics",
                                           "At 5th level, the untamed rager gains Greater Dirty Trick as a bonus feat.",
                                           "",
                                           greater_dirty_trick.Icon,
                                           FeatureGroup.None,
                                           Helpers.CreateAddFact(greater_dirty_trick)
                                           );
        }


        static void createFeralAppearance()
        {
            feral_appearance = Helpers.CreateFeature("FeralAppearanceUntamedRagerFeature",
                                                     "Feral Appearance",
                                                     "At 3rd level, the untamed rager gains a +1 bonus on Intimidate checks. This bonus increases by 1 every 3 barbarian levels thereafter.",
                                                     "",
                                                     Helpers.GetIcon("d2aeac47450c76347aebbc02e4f463e0"), //fear
                                                     FeatureGroup.None,
                                                     Helpers.CreateAddContextStatBonus(StatType.CheckIntimidate, ModifierDescriptor.UntypedStackable)
                                                     );
            feral_appearance.Ranks = 6;
            feral_appearance.ReapplyOnLevelUp = true;
            feral_appearance.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, feature: feral_appearance));
        }


        static void createDishonorable()
        {
            ContextValue value = Helpers.CreateContextValue(AbilityRankType.Default);
            dishonorable = Helpers.CreateFeature("DishonorableUntamedRagerFeature",
                                                    "Dishonorable",
                                                    "At 7th level and every 3 barbarian levels thereafter, the untamed rager gains a +1 bonus on combat maneuver checks when performing dirty tricks and to her CMD to resist others’ dirty tricks.",
                                                    "",
                                                    Helpers.GetIcon("95851f6e85fe87d4190675db0419d112"), //grease
                                                    FeatureGroup.None,
                                                    Helpers.Create<NewMechanics.ContextCombatManeuverBonus>(a => { a.Bonus = value; a.Type = Kingmaker.RuleSystem.Rules.CombatManeuver.DirtyTrickBlind; }),
                                                    Helpers.Create<NewMechanics.ContextCombatManeuverBonus>(a => { a.Bonus = value; a.Type = Kingmaker.RuleSystem.Rules.CombatManeuver.DirtyTrickEntangle; }),
                                                    Helpers.Create<NewMechanics.ContextCombatManeuverBonus>(a => { a.Bonus = value; a.Type = Kingmaker.RuleSystem.Rules.CombatManeuver.DirtyTrickSickened; }),
                                                    Helpers.Create<NewMechanics.ContextManeuverDefenceBonus>(a => { a.Bonus = value; a.Type = Kingmaker.RuleSystem.Rules.CombatManeuver.DirtyTrickBlind; }),
                                                    Helpers.Create<NewMechanics.ContextManeuverDefenceBonus>(a => { a.Bonus = value; a.Type = Kingmaker.RuleSystem.Rules.CombatManeuver.DirtyTrickEntangle; }),
                                                    Helpers.Create<NewMechanics.ContextManeuverDefenceBonus>(a => { a.Bonus = value; a.Type = Kingmaker.RuleSystem.Rules.CombatManeuver.DirtyTrickSickened; })
                                                    );
            dishonorable.Ranks = 5;
            dishonorable.ReapplyOnLevelUp = true;
            dishonorable.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, feature: dishonorable));
        }



    }
}
