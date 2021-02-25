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
        static void createInevitableUnit()
        {
            var fx_feature = Helpers.CreateFeature("InevitableEidolonFxFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Helpers.Create<UnitViewMechanics.ReplaceUnitView>(r => r.prefab = Common.createUnitViewLink("c9f3318f6aa6a3a4a9ce476989a07df5")), //adamantine golem
                                                   Helpers.Create<SizeMechanics.PermanentSizeOverride>(p => p.size = Size.Medium));
            fx_feature.HideInCharacterSheetAndLevelUp = true;
            fx_feature.HideInUI = true;

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var axiomite = library.Get<BlueprintUnit>("a97cc6e80fe9a454db9c0fb519fa4087");
            var inevitable_unit = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "InevitableEidolonUnit", "");
            inevitable_unit.Color = axiomite.Color;

            inevitable_unit.Visual = axiomite.Visual;
            inevitable_unit.LocalizedName = inevitable_unit.LocalizedName.CreateCopy();
            inevitable_unit.LocalizedName.String = Helpers.CreateString(inevitable_unit.name + ".Name", "Inevitable Eidolon");

            inevitable_unit.Alignment = Alignment.LawfulNeutral;
            inevitable_unit.Strength = 16;
            inevitable_unit.Dexterity = 12;
            inevitable_unit.Constitution = 13;
            inevitable_unit.Intelligence = 7;
            inevitable_unit.Wisdom = 10;
            inevitable_unit.Charisma = 11;
            inevitable_unit.Speed = 30.Feet();
            inevitable_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2, fx_feature }; // { natural_armor2, fx_feature };
            inevitable_unit.Body = inevitable_unit.Body.CloneObject();
            inevitable_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            inevitable_unit.Body.PrimaryHand = null;
            inevitable_unit.Body.SecondaryHand = null;
            inevitable_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            inevitable_unit.Body.AdditionalSecondaryLimbs = new BlueprintItemWeapon[0];
            inevitable_unit.Size = Size.Large;
            inevitable_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[0];
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillAthletics, StatType.SkillMobility, StatType.SkillStealth };
                a.Selections = new SelectionEntry[0];
            });
            inevitable_unit.AddComponents(Helpers.Create<EidolonComponent>(),
                                          Helpers.Create<UnitViewMechanics.ChangeUnitScaleForInventory>(c => c.scale_factor = 1.51f));



            Helpers.SetField(inevitable_unit, "m_Portrait", Helpers.createPortrait("EidolonInevitableProtrait", "Golem", ""));
            inevitable_eidolon = Helpers.CreateProgression("InevitableEidolonProgression",
                                        "Inevitable Eidolon",
                                        "Implacable and ceaseless in their fight against chaos and those who break natural laws, inevitables make loyal, if literal-minded, companions for lawful summoners. Summoners of inevitables generally get along well with axiomites, who share their understanding of the process of forging and modifying an inevitable. Inevitable eidolons appear as a mixture between clockwork constructs and idealized humanoid statues.",
                                        "",
                                        Helpers.GetIcon("c66e86905f7606c4eaa5c774f0357b2b"), //stone_skin
                                        FeatureGroup.AnimalCompanion,
                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                        );
            inevitable_eidolon.IsClassFeature = true;
            inevitable_eidolon.ReapplyOnLevelUp = true;
            inevitable_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            inevitable_eidolon.AddComponent(Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.Lawful | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.TrueNeutral));
            inevitable_eidolon.ReplaceComponent<AddPet>(a => a.Pet = inevitable_unit);

            Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(inevitable_eidolon);
            addLesserEidolon(inevitable_eidolon);


            var construct_unit = library.CopyAndAdd(inevitable_unit, "ConstructEidolonUnit", "");
            construct_unit.LocalizedName = construct_unit.LocalizedName.CreateCopy();
            construct_unit.LocalizedName.String = Helpers.CreateString(construct_unit.name + ".Name", "Construct Eidolon");
            construct_unit.Constitution = 10;

            construct_eidolon = library.CopyAndAdd(inevitable_eidolon, "ConstructEidolonProgression", "");
            construct_eidolon.ReplaceComponent<AddPet>(a => a.Pet = construct_unit);
            construct_eidolon.SetNameDescription("Construct Eidolon",
                                                 "A construct caller must select the inevitable subtype for her eidolon. A construct eidolon functions as an inevitable eidolon except as noted here. A construct eidolon has no Constitution score and gains bonus hit points appropriate for a construct of its size.\n"
                                                 + "At 12th level, the construct eidolon gains DR 5/adamantine instead of DR 5/chaotic.");

            construct_eidolon.AddComponent(addTransferableFeatToEidolon("ConstructEidolonHealthFeature", 
                                                                        Helpers.Create<ConstructHealth>(),
                                                                        Helpers.Create<SizeMechanics.CorrectSizeHp>()));


        }


        static void fillInevitableProgression()
        {
            var feature1 = Helpers.CreateFeature("InevitableEidolonLevel1Feature",
                                                  "Base Evolutions",
                                                  "At 1st level, inevitable eidolons count as both constructs and outsiders for the purpose of effects such as the bane weapon special ability and the favored enemy class feature. They gain the slam evolution and a +4 bonus on saving throws against death effects, disease, necromancy effects, paralysis, poison, sleep, and stun.",
                                                  "",
                                                  inevitable_eidolon.Icon,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("InevitableEidolonLevel1AddFeature",
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
                                                  addTransferableFeatToEidolon("InevitableEidolonLevel4AddFeature",
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
                                                  addTransferableFeatToEidolon("InevitableEidolonLevel8AddFeature",
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
                                                  addTransferableFeatToEidolon("InevitableEidolonLevel12AddFeature",
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

            var feature12b = Helpers.CreateFeature("ConstructEidolonLevel12Feature",
                                                  "Damage Reduction",
                                                  "At 12th level, construct eidolons gain DR 5/adamantine. They also gain immunity to sleep.",
                                                  "",
                                                  Helpers.GetIcon("9e1ad5d6f87d19e4d8883d63a6e35568"), //mage armor
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("ConstructEidolonLevel12AddFeature",
                                                                                Common.createMaterialDR(Helpers.CreateContextValue(AbilityRankType.Default), PhysicalDamageMaterial.Adamantite),
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
                                                  addTransferableFeatToEidolon("InevitableEidolonLevel16AddFeature",
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
                                                  addTransferableFeatToEidolon("InevitableEidolonLevel20AddFeature",
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
            setLesserEidolonProgression(inevitable_eidolon);


            construct_eidolon.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, feature1),
                                                           Helpers.LevelEntry(4, feature4),
                                                           Helpers.LevelEntry(8, feature8),
                                                           Helpers.LevelEntry(12, feature12b),
                                                           Helpers.LevelEntry(16, feature16),
                                                           Helpers.LevelEntry(20, feature20)
                                                           };
            construct_eidolon.UIGroups = Helpers.CreateUIGroups(feature1, feature4, feature8, feature12b, feature16, feature20);

            inevitable_eidolon.AddComponent(Helpers.Create<NewMechanics.FeatureReplacement>(f => f.replacement_feature = construct_eidolon));
        }
    }
}
