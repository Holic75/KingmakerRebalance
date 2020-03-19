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
        static void fillDevilProgression()
        {
            var base_evolutions = Helpers.CreateFeature("DevilEidolonBaseEvolutionsFeature",
                                                        "",
                                                        "",
                                                        "",
                                                        null,
                                                        FeatureGroup.None,
                                                        Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.resistance[3])
                                                        );
            base_evolutions.HideInCharacterSheetAndLevelUp = true;
            var feature1 = Helpers.CreateFeature("DevilEidolonLevel1Feature",
                                                  "Base Evolutions",
                                                  "Starting at 1st level, devil eidolons gain the claws evolution, resistance (fire) evolution and the skilled (Persuation) evolution. They also gain a +4 bonus on saving throws against poison.",
                                                  "",
                                                  devil_eidolon.Icon,
                                                  FeatureGroup.None,
                                                  Common.createAddFeatComponentsToAnimalCompanion("DevilEidolonLevel1AddFeature",
                                                                                                  Common.createContextSavingThrowBonusAgainstDescriptor(4, ModifierDescriptor.Other, SpellDescriptor.Poison)
                                                                                                  ),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.claws_biped),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.skilled[1]),
                                                  Helpers.CreateAddFeatureOnClassLevel(base_evolutions, 16, Summoner.getSummonerArray(), before: true)
                                                  );

            var feature4 = Helpers.CreateFeature("DevilEidolonLevel4Feature",
                                                  "Resistance",
                                                  "At 4th level, devil eidolons gain acid resistance 10 and cold resistance 10.",
                                                  "",
                                                  Helpers.GetIcon("21ffef7791ce73f468b6fca4d9371e8b"), //resist energy
                                                  FeatureGroup.None,
                                                  Common.createAddFeatComponentsToAnimalCompanion("DevilEidolonLevel4AddFeature",
                                                                                                  Common.createEnergyDR(10, DamageEnergyType.Acid),
                                                                                                  Common.createEnergyDR(10, DamageEnergyType.Cold)
                                                                                                  )
                                                  );

            var feature8 = Helpers.CreateFeature("DevilEidolonLevel8Feature",
                                                  "Poison Immunity",
                                                  "At 8th level, devil eidolons gain the skilled (Diplomacy) evolution and gain immunity to poison.",
                                                  "",
                                                  Helpers.GetIcon("b48b4c5ffb4eab0469feba27fc86a023"), //delay poison
                                                  FeatureGroup.None,
                                                  Common.createAddFeatComponentsToAnimalCompanion("DevilEidolonLevel8AddFeature",
                                                                                                  Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Poison),
                                                                                                  Common.createBuffDescriptorImmunity(SpellDescriptor.Poison)
                                                                                                  )
                                                  );

            var feature12 = Helpers.CreateFeature("DevilEidolonLevel12Feature",
                                                  "Damage Reduction",
                                                  "At 12th level, devil eidolons gain DR 5/good.",
                                                  "",
                                                  Helpers.GetIcon("21ffef7791ce73f468b6fca4d9371e8b"), //resist energy,
                                                  FeatureGroup.None,
                                                  Common.createAddFeatComponentsToAnimalCompanion("DevilEidolonLevel12AddFeature",
                                                                                                  Common.createContextAlignmentDR(Helpers.CreateContextValue(AbilityRankType.Default), DamageAlignment.Good),
                                                                                                  Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureList,
                                                                                                                                  progression: ContextRankProgression.MultiplyByModifier,
                                                                                                                                  stepLevel: 10,
                                                                                                                                  min: 5,
                                                                                                                                  featureList: new BlueprintFeature[] { Evolutions.damage_reduction }
                                                                                                                                  )
                                                                                                  )
                                                  );

            var feature16 = Helpers.CreateFeature("DevilEidolonLevel16Feature",
                                                  "Fire Immunity",
                                                  "At 16th level, devil eidolons lose the resistance (fire) evolution, and instead gain the immunity (fire) evolution.",
                                                  "",
                                                  Helpers.GetIcon("d2f116cfe05fcdd4a94e80143b67046f"), //protection from energy,
                                                  FeatureGroup.None,
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.immunity[3])
                                                  );

            var regeneration_buff = Helpers.CreateBuff("DevilEidolonRegenerationBuff",
                                                       "",
                                                       "",
                                                       "",
                                                       null,
                                                       null,
                                                       Helpers.Create<AddEffectRegeneration>(a => { a.Heal = 5; a.CancelDamageAlignmentTypes = new DamageAlignment[] { DamageAlignment.Good }; })
                                                       );
            var feature20 = Helpers.CreateFeature("DevilEidolonLevel20Feature",
                                                  "Regeneration",
                                                  "At 20th level, devil eidolons gain regeneration 5 (good weapons, good spells). They are still banished to Hell as normal for eidolons if they take enough damage.",
                                                  "",
                                                  Helpers.GetIcon("b6afdc50876e08149b1f9fdcdb2a308c"), //fiend of the pit = rage
                                                  FeatureGroup.None,
                                                  Common.createAddFeatComponentsToAnimalCompanion("DevilEidolonLevel20AddFeature",
                                                                                                  Common.createAuraFeatureComponent(regeneration_buff)
                                                                                                  )
                                                  );

            devil_eidolon.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, feature1),
                                                           Helpers.LevelEntry(4, feature4),
                                                           Helpers.LevelEntry(8, feature8),
                                                           Helpers.LevelEntry(12, feature12),
                                                           Helpers.LevelEntry(16, feature16),
                                                           Helpers.LevelEntry(20, feature20)
                                                           };
            devil_eidolon.UIGroups = Helpers.CreateUIGroups(feature1, feature4, feature8, feature12, feature16, feature20);

            infernal_eidolon.LevelEntries = devil_eidolon.LevelEntries;
            infernal_eidolon.UIGroups = devil_eidolon.UIGroups;
        }
    }
}
