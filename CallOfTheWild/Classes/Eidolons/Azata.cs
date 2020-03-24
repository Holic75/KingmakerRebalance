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
        static void fillAzataProgression()
        {
            var base_evolutions = Helpers.CreateFeature("AzataEidolonBaseEvolutionsFeature",
                                                        "",
                                                        "",
                                                        "",
                                                        null,
                                                        FeatureGroup.None,
                                                        Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.resistance[2])
                                                        );
            base_evolutions.HideInCharacterSheetAndLevelUp = true;
            var feature1 = Helpers.CreateFeature("AzataEidolonLevel1Feature",
                                                  "Base Evolutions",
                                                  "At 1st level, azata eidolons gain the resistance (electricity) evolution and the 4-point weapon training evolution (proficiency in martial weapons).",
                                                  "",
                                                  azata_eidolon.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.weapon_training[0]),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.weapon_training[1]),
                                                  Helpers.CreateAddFeatureOnClassLevel(base_evolutions, 16, Summoner.getSummonerArray(), before: true)
                                                  );

            var feature4 = Helpers.CreateFeature("AzataEidolonLevel4Feature",
                                                  "Resistance",
                                                  "At 4th level, azata eidolons gain cold resistance 10 and fire resistance 10.",
                                                  "",
                                                  Helpers.GetIcon("21ffef7791ce73f468b6fca4d9371e8b"), //resist energy
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("AzataEidolonLevel4AddFeature",
                                                                                Common.createEnergyDR(10, DamageEnergyType.Cold),
                                                                                Common.createEnergyDR(10, DamageEnergyType.Fire))
                                                  );

            var feature8 = Helpers.CreateFeature("AzataEidolonLevel8Feature",
                                                  "Wings",
                                                  "At 8th level, azata eidolons grow large, feathery wings, gaining the flight evolution.",
                                                  "",
                                                  Evolutions.flight.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.flight)
                                                  );

            var feature12 = Helpers.CreateFeature("AzataEidolonLevel12Feature",
                                                  "Damage Reduction",
                                                  "At 12th level, azata eidolons gain DR 5/evil. They also gain immunity to petrification.",
                                                  "",
                                                  Helpers.GetIcon("9e1ad5d6f87d19e4d8883d63a6e35568"), //mage armor
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("AzataEidolonLevel12AddFeature",
                                                                                Common.createContextAlignmentDR(Helpers.CreateContextValue(AbilityRankType.Default), DamageAlignment.Evil),
                                                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureList,
                                                                                                                progression: ContextRankProgression.MultiplyByModifier,
                                                                                                                stepLevel: 10,
                                                                                                                min: 5,
                                                                                                                featureList: new BlueprintFeature[] { Evolutions.damage_reduction }
                                                                                                                ),
                                                                                Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Petrified),
                                                                                Common.createBuffDescriptorImmunity(SpellDescriptor.Petrified),
                                                                                Helpers.Create<RecalculateOnFactsChange>(r => r.CheckedFacts = new BlueprintUnitFact[] { Evolutions.damage_reduction }))
                                                  );


            var feature16 = Helpers.CreateFeatureSelection("AzataEidolonLevel16Feature",
                                                          "Immunity",
                                                          "At 16th level, azata eidolons lose the resistance (electricity) evolution, and instead gain the immunity (electricity) evolution. They also gain the ability increase evolution, applied to an ability score of the summoner’s choice.",
                                                          "",
                                                          Helpers.GetIcon("d2f116cfe05fcdd4a94e80143b67046f"), //protection from energy,
                                                          FeatureGroup.None,
                                                          Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.immunity[2])
                                                          );
            feature16.AllFeatures = Evolutions.getPermanenetEvolutions(e => Evolutions.ability_increase.Any(ai => ai[0] == e));



            var icon = library.Get<BlueprintAbility>("3e4ab69ada402d145a5e0ad3ad4b8564").Icon; //mirror image

            var invisibility_buff = library.Get<BlueprintBuff>("e6b35473a237a6045969253beb09777c");
            var ghost_fx = library.Get<BlueprintBuff>("20f79fea035330b479fc899fa201d232");

            var incorporeal = library.Get<BlueprintFeature>("c4a7f98d743bc784c9d4cf2105852c39");

            var buff = Helpers.CreateBuff("AzataFeature20Buff",
                                           "Energy Form",
                                           "At 20th level, an azata eidolon gains the ability to switch between its normal form and an energy form as a standard action. In its energy form, an azata eidolon is incorporeal and doubles its fly speed, but it can’t make natural or manufactured weapon attacks; it can, however, activate any spell-like ability evolutions it possesses.",
                                           "",
                                           icon,
                                           ghost_fx.FxOnStart,
                                           Helpers.CreateAddFacts(incorporeal),
                                           Helpers.Create<BuffMovementSpeed>(b => { b.Descriptor = ModifierDescriptor.UntypedStackable; b.Value = 100; b.MultiplierCap = 2; b.CappedOnMultiplier = true; }),
                                           Helpers.Create<AddCondition>(a => a.Condition = Kingmaker.UnitLogic.UnitCondition.CanNotAttack)
                                           );

            var ability = Helpers.CreateActivatableAbility("AzataFeature20ToggleAbility",
                                                           buff.Name,
                                                           buff.Description,
                                                           "",
                                                           buff.Icon,
                                                           buff,
                                                           AbilityActivationType.Immediately,
                                                           CommandType.Standard,
                                                           null
                                                           );

            var feature20 = Helpers.CreateFeature("AzataEidolonLevel20Feature",
                                                  "Energy Form",
                                                  "At 20th level, an azata eidolon gains the ability to switch between its normal form and an energy form as a standard action. In its energy form, an azata eidolon is incorporeal and doubles its fly speed, but it can’t make natural or manufactured weapon attacks; it can, however, activate any spell-like ability evolutions it possesses.",
                                                  "",
                                                  buff.Icon,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("AzataEidolonLevel20AddFeature",
                                                                                Helpers.CreateAddFact(ability))                                              
                                                  );

            azata_eidolon.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, feature1),
                                                           Helpers.LevelEntry(4, feature4),
                                                           Helpers.LevelEntry(8, feature8),
                                                           Helpers.LevelEntry(12, feature12),
                                                           Helpers.LevelEntry(16, feature16),
                                                           Helpers.LevelEntry(20, feature20)
                                                           };
            azata_eidolon.UIGroups = Helpers.CreateUIGroups(feature1, feature4, feature8, feature12, feature16, feature20);
        }
    }
}
