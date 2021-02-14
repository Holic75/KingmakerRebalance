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
    public class Swashbuckler
    {
        static public BlueprintArchetype archetype;

        static public BlueprintFeature martial_training;
        static public BlueprintFeature daring;
        static public BlueprintFeatureSelection swashbucler_combat_trick;

        static LibraryScriptableObject library => Main.library;


        static BlueprintCharacterClass[] getRogueArray()
        {
            var rogue_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("299aa766dee3cbf4790da4efb8c72484");
            return new BlueprintCharacterClass[] { rogue_class };
        }

        static public void create()
        {
            var rogue_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("299aa766dee3cbf4790da4efb8c72484");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "SwashbucklerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Swashbuckler");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "A paragon of mobile swordplay, the swashbuckler is a rogue who focuses almost exclusively on honing her skill at arms and perfecting daring acrobatic moves and elaborate flourishes that border on performance.");
            });
            Helpers.SetField(archetype, "m_ParentClass", rogue_class);
            library.AddAsset(archetype, "");


            var trapfinding = library.Get<BlueprintFeature>("dbb6b3bffe6db3547b31c3711653838e");
            var danger_sense = library.Get<BlueprintFeature>("0bcbe9e450b0e7b428f08f66c53c5136");


            createMartialTraining();
            createDaring();

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, trapfinding),
                                                          Helpers.LevelEntry(3, danger_sense),
                                                          Helpers.LevelEntry(6, danger_sense),
                                                          Helpers.LevelEntry(9, danger_sense),
                                                          Helpers.LevelEntry(12, danger_sense),
                                                          Helpers.LevelEntry(15, danger_sense),
                                                          Helpers.LevelEntry(18, danger_sense),
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, martial_training),
                                                       Helpers.LevelEntry(3, daring),
                                                       Helpers.LevelEntry(6, daring),
                                                       Helpers.LevelEntry(9, daring),
                                                       Helpers.LevelEntry(12, daring),
                                                       Helpers.LevelEntry(15, daring),
                                                       Helpers.LevelEntry(18, daring),
                                                       };

            rogue_class.Progression.UIGroups = rogue_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(martial_training, daring));
            rogue_class.Archetypes = rogue_class.Archetypes.AddToArray(archetype);
        }

        static void createMartialTraining()
        {
            var combat_trick = library.Get<BlueprintFeatureSelection>("c5158a6622d0b694a99efb1d0025d2c1");
            var martial_proficiency = library.Get<BlueprintFeature>("203992ef5b35c864390b4e4a1e200629");
            martial_training = library.CopyAndAdd<BlueprintFeature>("203992ef5b35c864390b4e4a1e200629", "SwashbucklerRogueMartialTraining", "");
            martial_training.SetNameDescription("Martial Training",
                                                "At 1st level, the swashbuckler receives proficiency with all martial weapons. In addition, she may take the combat trick rogue talent up to two times.");
            martial_training.ComponentsArray = new BlueprintComponent[] { Helpers.CreateAddFact(martial_proficiency)};
            swashbucler_combat_trick = library.CopyAndAdd<BlueprintFeatureSelection>("c5158a6622d0b694a99efb1d0025d2c1", "SwashbucklerRogueCombatTrickFeatureSelection", "");
            swashbucler_combat_trick.ReplaceComponent<PrerequisiteNoFeature>(p => p.Feature = swashbucler_combat_trick);
            swashbucler_combat_trick.AddComponents(Helpers.PrerequisiteFeature(martial_training));
            swashbucler_combat_trick.AddComponents(Helpers.PrerequisiteFeature(combat_trick));
            RogueTalents.addToTalentSelection(swashbucler_combat_trick, false, false);
        }


        static void createDaring()
        {
            daring = Helpers.CreateFeature("SwshbucklerRogueDaringFeature",
                                            "Daring",
                                            "At 3rd level, a swashbuckler gains a +1 morale bonus on Acrobatics checks and saving throws against fear. This bonus increases by +1 for every 3 levels beyond 3rd. ",
                                            "",
                                            Helpers.GetIcon("4be5757b85af47545a5789f1d03abda9"), //mobility ability
                                            FeatureGroup.None,
                                            Helpers.CreateAddContextStatBonus(StatType.SkillMobility, ModifierDescriptor.Morale),
                                            Common.createContextSavingThrowBonusAgainstDescriptor(Helpers.CreateContextValue(AbilityRankType.Default), ModifierDescriptor.Morale, SpellDescriptor.Fear)
                                            );
            daring.Ranks = 6;
            daring.ReapplyOnLevelUp = true;
            daring.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, feature: daring));
        }
    }

}
