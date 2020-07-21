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
        static void createDemonUnit()
        {
            var fx_feature = Helpers.CreateFeature("DemonEidolonFxFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Helpers.Create<UnitViewMechanics.ReplaceUnitView>(r => r.prefab = Common.createUnitViewLink(/*"58db978ff8529e1418f12e402e11c1f8"*/"f271f20076d660c4a9eeb1992f8b96e0")) //kanerah                                                   
                                                   );
            fx_feature.HideInCharacterSheetAndLevelUp = true;
            fx_feature.HideInUI = true;

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var erinyes = library.Get<BlueprintUnit>("306448df57ed0c44ea380bc41d7a0740");
            var demon_unit = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "DemonEidolonUnit", "");
            demon_unit.Color = erinyes.Color;

            demon_unit.Visual = erinyes.Visual;
            demon_unit.LocalizedName = demon_unit.LocalizedName.CreateCopy();
            demon_unit.LocalizedName.String = Helpers.CreateString(demon_unit.name + ".Name", "Demon Eidolon");

            demon_unit.Alignment = Alignment.ChaoticEvil;
            demon_unit.Strength = 16;
            demon_unit.Dexterity = 12;
            demon_unit.Constitution = 13;
            demon_unit.Intelligence = 7;
            demon_unit.Wisdom = 10;
            demon_unit.Charisma = 11;
            demon_unit.Speed = 30.Feet();
            demon_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2, fx_feature }; // { natural_armor2, fx_feature };
            demon_unit.Body = demon_unit.Body.CloneObject();
            demon_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            demon_unit.Body.PrimaryHand = null;
            demon_unit.Body.SecondaryHand = null;
            demon_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            demon_unit.Body.AdditionalSecondaryLimbs = new BlueprintItemWeapon[0];
            demon_unit.Gender = Gender.Female;
            demon_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[0];
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillAthletics, StatType.SkillMobility, StatType.SkillStealth };
                a.Selections = new SelectionEntry[0];
            });
            demon_unit.AddComponents(Helpers.Create<EidolonComponent>());

            Helpers.SetField(demon_unit, "m_Portrait", Helpers.createPortrait("EidolonDemonProtrait", "Demon", ""));
            demon_eidolon = Helpers.CreateProgression("DemonEidolonProgression",
                                                        "Demon Eidolon",
                                                        "Raw destruction given material substance, demon eidolons form out of the Abyss’s stew of soul energy, leading some scholars to speculate that the summoner’s arts are related to the magical tampering that gave rise to the first demons. Demon eidolons revel in causing destruction and inflicting suffering, and they will do so for their summoners without question, taking pleasure in whatever havoc they can create. For a demon eidolon, the means justify the ends.",
                                                        "",
                                                        Helpers.GetIcon("d3a4cb7be97a6694290f0dcfbd147113"), //abyssal progression
                                                        FeatureGroup.AnimalCompanion,
                                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                                        );
            demon_eidolon.IsClassFeature = true;
            demon_eidolon.ReapplyOnLevelUp = true;
            demon_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            demon_eidolon.AddComponent(Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.ChaoticEvil | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.ChaoticNeutral | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralEvil));
            demon_eidolon.ReplaceComponent<AddPet>(a => a.Pet = demon_unit);

            Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(demon_eidolon);
            addLesserEidolon(demon_eidolon);
        }


        static void fillDemonProgression()
        {
            var base_evolutions = Helpers.CreateFeature("DemonEidolonBaseEvolutionsFeature",
                                                        "",
                                                        "",
                                                        "",
                                                        null,
                                                        FeatureGroup.None,
                                                        Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.resistance[2]),
                                                        Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.resistance[3])
                                                        );
            base_evolutions.HideInCharacterSheetAndLevelUp = true;
            var feature1 = Helpers.CreateFeature("DemonEidolonLevel1Feature",
                                                  "Base Evolutions",
                                                  "Starting at 1st level, demon eidolons gain the claws, resistance (electricity) and resistance (fire) evolutions as well as a +4 bonus on saving throws against poison.",
                                                  "",
                                                  demon_eidolon.Icon,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("DemonEidolonLevel1AddFeature",
                                                                                Common.createContextSavingThrowBonusAgainstDescriptor(4, ModifierDescriptor.UntypedStackable, SpellDescriptor.Poison)
                                                                                ),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.claws),
                                                  Helpers.CreateAddFeatureOnClassLevel(base_evolutions, 32, Summoner.getSummonerArray(), before: true)
                                                  );

            var feature4 = Helpers.CreateFeature("DemonEidolonLevel4Feature",
                                                  "Resistance",
                                                  "At 4th level, demon eidolons gain acid resistance 10 and cold resistance 10.",
                                                  "",
                                                  Helpers.GetIcon("21ffef7791ce73f468b6fca4d9371e8b"), //resist energy
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("DemonEidolonLevel4AddFeature",
                                                                                Common.createEnergyDR(10, DamageEnergyType.Acid),
                                                                                Common.createEnergyDR(10, DamageEnergyType.Cold)
                                                                                )
                                                  );

            var feature8 = Helpers.CreateFeature("DemonEidolonLevel8Feature",
                                                  "Poison Immunity",
                                                  "At 8th level, demon eidolons lose the +4 bonus on saving throws against poison and gain immunity to poison. They also add 1 point to their evolution pools.",
                                                  "",
                                                  Helpers.GetIcon("b48b4c5ffb4eab0469feba27fc86a023"), //delay poison
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("DemonEidolonLevel8AddFeature",
                                                                                Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Poison),
                                                                                Common.createBuffDescriptorImmunity(SpellDescriptor.Poison)
                                                                                ),
                                                  Helpers.Create<EvolutionMechanics.IncreaseEvolutionPool>(i => i.amount = 1)
                                                  );

            var feature12 = Helpers.CreateFeatureSelection("DemonEidolonLevel12Feature",
                                                          "Damage Reduction",
                                                          "At 12th level, demon eidolons gain DR 5/good. They also gain the ability increase evolution in an ability score of the summoner’s choice.",
                                                          "",
                                                          Helpers.GetIcon("9e1ad5d6f87d19e4d8883d63a6e35568"), //mage armor
                                                          FeatureGroup.None,
                                                          addTransferableFeatToEidolon("DemonEidolonLevel12AddFeature",
                                                                                        Common.createContextAlignmentDR(Helpers.CreateContextValue(AbilityRankType.Default), DamageAlignment.Good),
                                                                                        Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureList,
                                                                                                                        progression: ContextRankProgression.MultiplyByModifier,
                                                                                                                        stepLevel: 10,
                                                                                                                        min: 5,
                                                                                                                        featureList: new BlueprintFeature[] { Evolutions.damage_reduction }
                                                                                                                        ),
                                                                                        Helpers.Create<RecalculateOnFactsChange>(r => r.CheckedFacts = new BlueprintUnitFact[] { Evolutions.damage_reduction })
                                                                                        )
                                                          );
            feature12.AllFeatures = Evolutions.getPermanenetEvolutions(e => Evolutions.ability_increase.Any(ai => ai[0] == e));

            var feature16 = Helpers.CreateFeature("DemonEidolonLevel16Feature",
                                                  "Electricity Immunity",
                                                  "At 16th level, demon eidolons lose the resistance (electricity) evolution, and instead gain the immunity (electricity) evolution.",
                                                  "",
                                                  Helpers.GetIcon("d2f116cfe05fcdd4a94e80143b67046f"), //protection from energy,
                                                  FeatureGroup.None,
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.immunity[2])
                                                  );

            var feature20 = Helpers.CreateFeature("DemonEidolonLevel20Feature",
                                                  "True Seeing",
                                                  "At 20th level, demon eidolons gain true seeing as a constant ability.",
                                                  "",
                                                  Helpers.GetIcon("b3da3fbee6a751d4197e446c7e852bcb"), //true seeing
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("DemonEidolonLevel20AddFeature",
                                                                                Helpers.Create<AddCondition>(a => a.Condition = UnitCondition.TrueSeeing)
                                                                                )
                                                  );

            demon_eidolon.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, feature1),
                                                           Helpers.LevelEntry(4, feature4),
                                                           Helpers.LevelEntry(8, feature8),
                                                           Helpers.LevelEntry(12, feature12),
                                                           Helpers.LevelEntry(16, feature16),
                                                           Helpers.LevelEntry(20, feature20)
                                                           };
            demon_eidolon.UIGroups = Helpers.CreateUIGroups(feature1, feature4, feature8, feature12, feature16, feature20);
            setLesserEidolonProgression(demon_eidolon);
        }
    }
}
