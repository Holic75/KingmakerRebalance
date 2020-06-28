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
        static void createAngelUnit()
        {
            var fx_buff = Helpers.CreateBuff("AngelEidolonFxBuff",
                                 "",
                                 "",
                                 "",
                                 null,
                                 Common.createPrefabLink("20832f2c72b574d4cb42ee82fc244d78")); //aasimar halo
            fx_buff.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);
            var fx_feature = Helpers.CreateFeature("AngelEidolonFxFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Helpers.Create<UnitViewMechanics.ReplaceUnitView>(r => r.prefab = Common.createUnitViewLink("db4e061c2d26e01408a264cc7c569daf")), //ghaele
                                                   Common.createAuraFeatureComponent(fx_buff));
            fx_feature.HideInCharacterSheetAndLevelUp = true;
            fx_feature.HideInUI = true;

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var azata = library.Get<BlueprintUnit>("bc8ca1437c0f48948b317b7e64febf0d");
            var angel_unit = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "AngelEidolonUnit", "");
            angel_unit.Color = azata.Color;

            angel_unit.Visual = azata.Visual;
            angel_unit.LocalizedName = azata.LocalizedName.CreateCopy();
            angel_unit.LocalizedName.String = Helpers.CreateString(angel_unit.name + ".Name", "Angel Eidolon");

            angel_unit.Alignment = Alignment.LawfulGood;
            angel_unit.Strength = 16;
            angel_unit.Dexterity = 12;
            angel_unit.Constitution = 13;
            angel_unit.Intelligence = 7;
            angel_unit.Wisdom = 10;
            angel_unit.Charisma = 11;
            angel_unit.Speed = 30.Feet();
            angel_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2, fx_feature }; // { natural_armor2, fx_feature };
            angel_unit.Body = angel_unit.Body.CloneObject();
            angel_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            angel_unit.Body.PrimaryHand = null;
            angel_unit.Body.SecondaryHand = null;
            angel_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            angel_unit.Gender = Gender.Female;
            angel_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[0];
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillAthletics, StatType.SkillMobility, StatType.SkillStealth };
                a.Selections = new SelectionEntry[0];
            });
            angel_unit.AddComponents(Helpers.Create<EidolonComponent>());

            Helpers.SetField(angel_unit, "m_Portrait", Helpers.createPortrait("EidolonAngelProtrait", "Angel", ""));
            angel_eidolon = Helpers.CreateProgression("AngelEidolonProgression",
                                                        "Angel Eidolon",
                                                        "Hailing from the higher planes, angel eidolons are creatures of exquisite beauty. They usually appear in idealized humanoid forms, with smooth skin, shining hair, and bright eyes. Angel eidolons are impeccably honorable, trustworthy, and diplomatic, but they do not shy away from confrontation when facing off against evil and its minions.",
                                                        "",
                                                        Helpers.GetIcon("75a10d5a635986641bfbcceceec87217"), //angelic aspect
                                                        FeatureGroup.AnimalCompanion,
                                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                                        );
            angel_eidolon.IsClassFeature = true;
            angel_eidolon.ReapplyOnLevelUp = true;
            angel_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            angel_eidolon.AddComponent(Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.LawfulGood | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.LawfulNeutral | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralGood));
            angel_eidolon.ReplaceComponent<AddPet>(a => a.Pet = angel_unit);

            Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(angel_eidolon);
            
        }


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
                                                  Helpers.CreateAddFeatureOnClassLevel(base_evolutions, 32, Summoner.getSummonerArray(), before: true)
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
            addLesserEidolon(angel_eidolon);
        }
    }
}
