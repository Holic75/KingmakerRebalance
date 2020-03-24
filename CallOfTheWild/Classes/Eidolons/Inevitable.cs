using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.CharacterSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    public partial class Eidolon
    {
        static void fillInevitableProgression()
        {
            var feature1 = Helpers.CreateFeature("InevitableEidolonLevel1Feature",
                                                  "Base Evolutions",
                                                  "At 1st level, inevitable eidolons count as both constructs and outsiders for the purpose of effects such as the bane weapon special ability and the favored enemy class feature. They gain the slam evolution and a +4 bonus on saving throws against death effects, disease, necromancy effects, paralysis, poison, sleep, and stun.",
                                                  "",
                                                  inevitable_eidolon.Icon,
                                                  FeatureGroup.None,
                                                  Common.createAddFeatComponentsToAnimalCompanion("InevitableEidolonLevel1AddFeature",
                                                                                                  Common.createContextSavingThrowBonusAgainstDescriptor(4, ModifierDescriptor.Size, SpellDescriptor.Poison | SpellDescriptor.Death 
                                                                                                                                                                                     | SpellDescriptor.Disease | SpellDescriptor.Paralysis
                                                                                                                                                                                     | SpellDescriptor.Sleep | SpellDescriptor.Stun)
                                                                                                 ),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.slam_biped)
                                                  );

            var feature4 = Helpers.CreateFeature("InevitableEidolonLevel4Feature",
                                                  "Immunity",
                                                  "At 4th level, inevitable eidolons gain a +4 bonus on saving throws against mind-affecting effects and immunity to nonlethal damage, fatigue, and exhaustion.",
                                                  "",
                                                  Helpers.GetIcon("2a6a2f8e492ab174eb3f01acf5b7c90a"), //defensive stance
                                                  FeatureGroup.None,
                                                  Common.createAddFeatComponentsToAnimalCompanion("InevitableEidolonLevel4AddFeature",
                                                                                                  Common.createContextSavingThrowBonusAgainstDescriptor(4, ModifierDescriptor.UntypedStackable, SpellDescriptor.MindAffecting),
                                                                                                  Common.createAddConditionImmunity(UnitCondition.Fatigued),
                                                                                                  Common.createAddConditionImmunity(UnitCondition.Exhausted),
                                                                                                  Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Fatigue | SpellDescriptor.Exhausted),
                                                                                                  Common.createBuffDescriptorImmunity(SpellDescriptor.Exhausted | SpellDescriptor.Fatigue)
                                                                                                  )
                                                  );

            var feature8 = Helpers.CreateFeature("InevitableEidolonLevel8Feature",
                                                  "Immunity II",
                                                  "At 8th level, inevitable eidolons gain immunity to death effects, disease, and poison.",
                                                  "",
                                                  Helpers.GetIcon("0413915f355a38146bc6ad40cdf27b3f"), //death ward
                                                  FeatureGroup.None,
                                                  Common.createAddFeatComponentsToAnimalCompanion("InevitableEidolonLevel8AddFeature",
                                                                                                  Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Death | SpellDescriptor.Disease | SpellDescriptor.Poison),
                                                                                                  Common.createBuffDescriptorImmunity(SpellDescriptor.Death | SpellDescriptor.Disease | SpellDescriptor.Poison)
                                                                                                  )
                                                  );

            var feature12 = Helpers.CreateFeature("InevitableEidolonLevel12Feature",
                                                  "Damage Reduction",
                                                  "At 12th level, inevitable eidolons gain DR 5/chaotic. They also gain immunity to sleep.",
                                                  "",
                                                  Helpers.GetIcon("9e1ad5d6f87d19e4d8883d63a6e35568"), //mage armor
                                                  FeatureGroup.None,
                                                  Common.createAddFeatComponentsToAnimalCompanion("InevitableEidolonLevel12AddFeature",
                                                                                                  Common.createContextAlignmentDR(Helpers.CreateContextValue(AbilityRankType.Default), DamageAlignment.Chaotic),
                                                                                                  Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureList,
                                                                                                                                  progression: ContextRankProgression.MultiplyByModifier,
                                                                                                                                  stepLevel: 10,
                                                                                                                                  min: 5,
                                                                                                                                  featureList: new BlueprintFeature[] { Evolutions.damage_reduction }
                                                                                                                                  ),
                                                                                                  Common.createBuffDescriptorImmunity(SpellDescriptor.Sleep),
                                                                                                  Helpers.Create<RecalculateOnFactsChange>(r => r.CheckedFacts = new BlueprintUnitFact[] { Evolutions.damage_reduction })
                                                                                                  )
                                                  );

            var feature16 = Helpers.CreateFeature("InevitableEidolonLevel16Feature",
                                                  "Immunity III",
                                                  "At 16th level, inevitable eidolons gain immunity to ability damage, ability drain and energy drain.",
                                                  "",
                                                  Helpers.GetIcon("d2f116cfe05fcdd4a94e80143b67046f"), //protection from energy,
                                                  FeatureGroup.None,
                                                  Common.createAddFeatComponentsToAnimalCompanion("InevitableEidolonLevel16AddFeature",
                                                                                                  Helpers.Create<AddImmunityToAbilityScoreDamage>(a => a.Drain = true),
                                                                                                  Helpers.Create<AddImmunityToEnergyDrain>()
                                                                                                  )
                                                  );

            var feature20 = Helpers.CreateFeature("InevitableEidolonLevel20Feature",
                                                  "Immunity IV",
                                                  "At 20th level, inevitable eidolons gain immunity to paralysis, stun, and any effect that requires a Fortitude save",
                                                  "",
                                                  Helpers.GetIcon("0a5ddfbcfb3989543ac7c936fc256889"), //spell resistance
                                                  FeatureGroup.None,
                                                  Common.createAddFeatComponentsToAnimalCompanion("InevitableEidolonLevel20AddFeature",
                                                                                                  Common.createBuffDescriptorImmunity(SpellDescriptor.Paralysis | SpellDescriptor.Stun),
                                                                                                  Common.createAddConditionImmunity(UnitCondition.Paralyzed),
                                                                                                  Common.createAddConditionImmunity(UnitCondition.Stunned),
                                                                                                  Helpers.CreateAddStatBonus(StatType.SaveFortitude, 100, ModifierDescriptor.UntypedStackable),
                                                                                                  Helpers.Create<Evasion>(e => e.SavingThrow = SavingThrowType.Fortitude)
                                                                                                  )
                                                  );

            inevitable_eidolon.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, feature1),
                                                           Helpers.LevelEntry(4, feature4),
                                                           Helpers.LevelEntry(8, feature8),
                                                           Helpers.LevelEntry(12, feature12),
                                                           Helpers.LevelEntry(16, feature16),
                                                           Helpers.LevelEntry(20, feature20)
                                                           };
            inevitable_eidolon.UIGroups = Helpers.CreateUIGroups(feature1, feature4, feature8, feature12, feature16, feature20);
        }
    }
}
