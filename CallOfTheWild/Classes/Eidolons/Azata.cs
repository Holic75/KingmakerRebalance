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
        static void createAzataUnit()
        {
            var fx_feature = Helpers.CreateFeature("AzataEidolonFxFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Helpers.Create<UnitViewMechanics.ReplaceUnitView>(r => r.prefab = Common.createUnitViewLink("255c1c746b1c31b40b16add1bb6b783e")));
            fx_feature.HideInCharacterSheetAndLevelUp = true;
            fx_feature.HideInUI = true;

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var azata = library.Get<BlueprintUnit>("d6fdf2d1776817b4bab5d4a43d9ea708");
            var azata_unit = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "AzataEidolonUnit", "");
            azata_unit.Color = azata.Color;
            azata_unit.Visual = azata.Visual;
            azata_unit.LocalizedName = azata.LocalizedName.CreateCopy();
            azata_unit.LocalizedName.String = Helpers.CreateString(azata_unit.name + ".Name", "Azata Eidolon");

            azata_unit.Alignment = Alignment.ChaoticGood;
            azata_unit.Strength = 16;
            azata_unit.Dexterity = 12;
            azata_unit.Constitution = 13;
            azata_unit.Intelligence = 7;
            azata_unit.Wisdom = 10;
            azata_unit.Charisma = 11;
            azata_unit.Speed = 30.Feet();
            azata_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2, fx_feature }; // { natural_armor2, fx_feature };
            azata_unit.Body = azata_unit.Body.CloneObject();
            azata_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            azata_unit.Body.PrimaryHand = null;
            azata_unit.Body.SecondaryHand = null;
            azata_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            azata_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[0];
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillPerception, StatType.SkillLoreReligion, StatType.SkillStealth };
                a.Selections = new SelectionEntry[0];
            });
            azata_unit.AddComponents(Helpers.Create<EidolonComponent>());


            Helpers.SetField(azata_unit, "m_Portrait", Helpers.createPortrait("EidolonAzataProtrait", "Azata", ""));
            azata_eidolon = Helpers.CreateProgression("AzataEidolonProgression",
                                        "Azata Eidolon",
                                        "Embodiments of the untamable beauty and noble passion of Elysium, azata eidolons have wild and beautiful features. They often take graceful forms reminiscent of elves or fey, but they occasionally appear like lillends, with serpentine tails. Azata eidolons are flighty and independent, and they often have their own ideas about how to defeat evil or have a good time. Thus, an azata eidolon is likely to balk if its summoner commands it to perform offensive or nefarious actions. On the other hand, an azata eidolon in sync with its summoner is a passionate and devoted companion.",
                                        "",
                                        Helpers.GetIcon("90810e5cf53bf854293cbd5ea1066252"), //righteous might
                                        FeatureGroup.AnimalCompanion,
                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                        );
            azata_eidolon.IsClassFeature = true;
            azata_eidolon.ReapplyOnLevelUp = true;
            azata_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            azata_eidolon.AddComponent(Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.ChaoticGood | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.ChaoticNeutral | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralGood));
            azata_eidolon.ReplaceComponent<AddPet>(a => a.Pet = azata_unit);

            Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(azata_eidolon);
        }


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
                                                  Helpers.CreateAddFeatureOnClassLevel(base_evolutions, 32, Summoner.getSummonerArray(), before: true)
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
            addLesserEidolon(azata_eidolon);
        }
    }
}
