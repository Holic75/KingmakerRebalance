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
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
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
        static void fillAgathionProgression()
        {
            var base_evolutions = Helpers.CreateFeature("AgathionEidolonBaseEvolutionsFeature",
                                                        "",
                                                        "",
                                                        "",
                                                        null,
                                                        FeatureGroup.None,
                                                        Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.resistance[2])
                                                        );
            base_evolutions.HideInCharacterSheetAndLevelUp = true;
            var feature1 = Helpers.CreateFeature("AgathionEidolonLevel1Feature",
                                                  "Base Evolutions",
                                                  "At 1st level, agathion eidolons gain the resistance (electricity) and the bite evolutions. They also gain a +4 bonus on saving throws against poison and petrification.",
                                                  "",
                                                  agathion_eidolon.Icon,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("AgathionEidolonLevel1AddFeature",
                                                                               Common.createContextSavingThrowBonusAgainstDescriptor(4, ModifierDescriptor.UntypedStackable, SpellDescriptor.Poison | SpellDescriptor.Petrified)),
                                                  Helpers.CreateAddFeatureOnClassLevel(base_evolutions, 16, Summoner.getSummonerArray(), before: true),
                                                  Helpers.Create<EvolutionMechanics.AddFakeEvolution>(a => a.Feature = Evolutions.bite)
                                                  );

            var feature4 = Helpers.CreateFeature("AgathionEidolonLevel4Feature",
                                                  "Resistance",
                                                  "At 4th level, agathion eidolons gain cold resistance 10 and sonic resistance 10.",
                                                  "",
                                                  Helpers.GetIcon("21ffef7791ce73f468b6fca4d9371e8b"), //resist energy
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("AgathionEidolonLevel4AddFeature",
                                                                                Common.createEnergyDR(10, DamageEnergyType.Cold),
                                                                                Common.createEnergyDR(10, DamageEnergyType.Sonic))
                                                  );

            var loh_resource = Helpers.CreateAbilityResource("AgathionLayOnHandsResource", "", "", "", null);
            loh_resource.SetIncreasedByLevelStartPlusDivStep(0, 2, 1, 2, 1, 0, 0.0f, new BlueprintCharacterClass[] { Eidolon.eidolon_class, Summoner.summoner_class });
            loh_resource.SetIncreasedByStat(0, StatType.Charisma);

            var loh_self = library.CopyAndAdd<BlueprintAbility>("8d6073201e5395d458b8251386d72df1", "AgathionLayOnHandsSelfAbility", "");
            loh_self.RemoveComponents<AbilityCasterAlignment>();
            loh_self.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", new BlueprintCharacterClass[] { Eidolon.eidolon_class, Summoner.summoner_class }));
            loh_self.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = loh_resource);
            var loh = library.CopyAndAdd<BlueprintAbility>("caae1dc6fcf7b37408686971ee27db13", "AgathionLayOnHandsAbility", "");
            loh.RemoveComponents<AbilityCasterAlignment>();
            loh.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", new BlueprintCharacterClass[] { Eidolon.eidolon_class }));
            loh.RemoveComponents<AbilityResourceLogic>();
            var loh_cast = Helpers.CreateTouchSpellCast(loh);
            loh_cast.AddComponent(loh_resource.CreateResourceLogic());

            var feature8 = Helpers.CreateFeature("AgathionEidolonLevel8Feature",
                                                  "Lay on Hands",
                                                  "At 8th level, agathion eidolons gain lay on hands as paladins with levels equal to their Hit Dice.",
                                                  "",
                                                  loh.Icon,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("AgathionEidolonLevel8AddFeature",
                                                                                Helpers.CreateAddFacts(loh_self, loh_cast),
                                                                                loh_resource.CreateAddAbilityResource()
                                                                                )
                                                  );


            var agathion_dr = Helpers.CreateFeature("AgathionEidolonDRFeature",
                                                  "Damage Reduction",
                                                  "At 12th level, agathion eidolons gain DR 5/evil. They also gain immunity to petrification.",
                                                   "",
                                                   Helpers.GetIcon("9e1ad5d6f87d19e4d8883d63a6e35568"), //mage armor
                                                   FeatureGroup.None,
                                                   Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Petrified),
                                                   Common.createBuffDescriptorImmunity(SpellDescriptor.Petrified),
                                                   Common.createContextAlignmentDR(Helpers.CreateContextValue(AbilityRankType.Default), DamageAlignment.Evil)
                                                   );
            var agathion_extra_dr = Helpers.CreateFeature("AgathionEidolonExtraDrFeature", "", "", "", null, FeatureGroup.None);
            agathion_extra_dr.HideInCharacterSheetAndLevelUp = true;
            agathion_dr.AddComponents(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureList,
                                                                progression: ContextRankProgression.MultiplyByModifier,
                                                                stepLevel: 5,
                                                                min: 5,
                                                                featureList: new BlueprintFeature[] { Evolutions.damage_reduction, agathion_dr, agathion_extra_dr }
                                                                ),
                                         Helpers.Create<RecalculateOnFactsChange>(r => r.CheckedFacts = new BlueprintUnitFact[] { Evolutions.damage_reduction, agathion_extra_dr })
                                         );

            transferable_abilities.Add(agathion_dr);
            var feature12 = Helpers.CreateFeature("AgathionEidolonLevel12Feature",
                                                  agathion_dr.Name,
                                                  agathion_dr.Description,
                                                  "",
                                                  agathion_dr.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.Create<AddFeatureToCompanion>(a => a.Feature = agathion_dr)
                                                  );


            var feature16 = Helpers.CreateFeature("AgathionEidolonLevel16Feature",
                                                    "Immunity",
                                                    "At 16th level, agathion eidolons lose the resistance (electricity) evolution, and instead gain the immunity (electricity) evolution.",
                                                    "",
                                                    Helpers.GetIcon("d2f116cfe05fcdd4a94e80143b67046f"), //protection from energy,
                                                    FeatureGroup.None,
                                                    Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.immunity[2])
                                                    );
            transferable_abilities.Add(agathion_extra_dr);
            var feature20 = Helpers.CreateFeature("AgathionEidolonLevel20Feature",
                                                  "Damage Reduction II",
                                                  "At 20th level, agathion eidolons increase their damage reduction to DR 10/evil.",
                                                  "",
                                                  feature12.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.Create<AddFeatureToCompanion>(a => a.Feature = agathion_extra_dr)
                                                  );

            agathion_eidolon.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, feature1),
                                                           Helpers.LevelEntry(4, feature4),
                                                           Helpers.LevelEntry(8, feature8),
                                                           Helpers.LevelEntry(12, feature12),
                                                           Helpers.LevelEntry(16, feature16),
                                                           Helpers.LevelEntry(20, feature20)
                                                           };
            agathion_eidolon.UIGroups = Helpers.CreateUIGroups(feature1, feature4, feature8, feature12, feature16, feature20);
        }
    }
}

