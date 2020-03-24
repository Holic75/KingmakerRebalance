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
        static void fillAngelProgression()
        {
            var base_evolutions = Helpers.CreateFeature("AngelEidolonBaseEvolutionsFeature",
                                                        "",
                                                        "",
                                                        "",
                                                        null,
                                                        FeatureGroup.None,
                                                        Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.resistance[0]),
                                                        Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.resistance[1])
                                                        );
            base_evolutions.HideInCharacterSheetAndLevelUp = true;
            var feature1 = Helpers.CreateFeature("AngelEidolonLevel1Feature",
                                                  "Base Evolutions",
                                                  "At 1st level, angel eidolons gain the slam, resistance (acid) and resistance (cold) evolutions. They also gain a +4 bonus on saving throws against poison.",
                                                  "",
                                                  angel_eidolon.Icon,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("AngelEidolonLevel1AddFeature",
                                                                               Common.createContextSavingThrowBonusAgainstDescriptor(4, ModifierDescriptor.UntypedStackable, SpellDescriptor.Poison)),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.slam_biped),
                                                  Helpers.CreateAddFeatureOnClassLevel(base_evolutions, 16, Summoner.getSummonerArray(), before: true)
                                                  );

            var feature4 = Helpers.CreateFeature("AngelEidolonLevel4Feature",
                                                  "Resistance",
                                                  "At 4th level, angel eidolons gain electricity resistance 10 and fire resistance 10.",
                                                  "",
                                                  Helpers.GetIcon("21ffef7791ce73f468b6fca4d9371e8b"), //resist energy
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("AngelEidolonLevel4AddFeature",
                                                                                Common.createEnergyDR(10, DamageEnergyType.Electricity),
                                                                                Common.createEnergyDR(10, DamageEnergyType.Fire))
                                                  );

            var feature8 = Helpers.CreateFeature("AngelEidolonLevel8Feature",
                                                  "Wings",
                                                  "At 8th level, angel eidolons grow large, feathery wings, gaining the flight evolution.",
                                                  "",
                                                  Evolutions.flight.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.flight)
                                                  );

            var feature12 = Helpers.CreateFeature("AngelEidolonLevel12Feature",
                                                  "Damage Reduction",
                                                  "At 12th level, angel eidolons gain DR 5/evil. They also gain immunity to petrification.",
                                                  "",
                                                  Helpers.GetIcon("9e1ad5d6f87d19e4d8883d63a6e35568"), //mage armor
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("AngelEidolonLevel12AddFeature",
                                                                                Common.createContextAlignmentDR(Helpers.CreateContextValue(AbilityRankType.Default), DamageAlignment.Evil),
                                                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureList,
                                                                                                                progression: ContextRankProgression.MultiplyByModifier,
                                                                                                                stepLevel: 10,
                                                                                                                min: 5,
                                                                                                                featureList: new BlueprintFeature[] {Evolutions.damage_reduction}
                                                                                                                ),                                                                                                                                
                                                                                Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Petrified),
                                                                                Common.createBuffDescriptorImmunity(SpellDescriptor.Petrified),
                                                                                Helpers.Create<RecalculateOnFactsChange> (r => r.CheckedFacts = new BlueprintUnitFact[] { Evolutions.damage_reduction })
                                                                                )
                                                  );

            var feature16 = Helpers.CreateFeature("AngelEidolonLevel16Feature",
                                                  "Immunity",
                                                  "At 16th level, angel eidolons lose the resistance (acid) and resistance (cold) evolutions, and instead gain the immunity (acid) and immunity (cold) evolutions.",
                                                  "",
                                                  Helpers.GetIcon("d2f116cfe05fcdd4a94e80143b67046f"), //protection from energy,
                                                  FeatureGroup.None,
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.immunity[0]),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.immunity[1])
                                                  );

            var protective_aura_toggle = library.Get<BlueprintActivatableAbility>("ab6a40148ed6fbc4ab72406327ac7e5e");
            var feature20 = Helpers.CreateFeature("AngelEidolonLevel20Feature",
                                                  "Protective Aura",
                                                  "At 20th level, angel eidolons gain the protective aura ability.",
                                                  "",
                                                  protective_aura_toggle.Icon,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("AngelEidolonLevel16AddFeature",
                                                                               Helpers.CreateAddFact(protective_aura_toggle))                                                
                                                  );

            angel_eidolon.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, feature1),
                                                           Helpers.LevelEntry(4, feature4),
                                                           Helpers.LevelEntry(8, feature8),
                                                           Helpers.LevelEntry(12, feature12),
                                                           Helpers.LevelEntry(16, feature16),
                                                           Helpers.LevelEntry(20, feature20)
                                                           };
            angel_eidolon.UIGroups = Helpers.CreateUIGroups(feature1, feature4, feature8, feature12, feature16, feature20);
        }
    }
}
