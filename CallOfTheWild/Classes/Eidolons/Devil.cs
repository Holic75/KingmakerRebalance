using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
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
        static void createDevilUnit()
        {
            var fx_feature = Helpers.CreateFeature("DevilEidolonFxFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Helpers.Create<UnitViewMechanics.ReplaceUnitView>(r => r.prefab = Common.createUnitViewLink("c78e19a2f6fa01343b4a188aacf38e50")) //devil apostate
                                                                                                                                                                                    //Helpers.Create<SizeMechanics.PermanentSizeOverride>(p => p.size = Size.Medium)
                                                   );
            fx_feature.HideInCharacterSheetAndLevelUp = true;
            fx_feature.HideInUI = true;

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var devil = library.Get<BlueprintUnit>("07c5044acbd443b468b6badd778f8cad");
            var devil_unit = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "DevilEidolonUnit", "");
            devil_unit.Color = devil.Color;

            devil_unit.Visual = devil.Visual;
            devil_unit.LocalizedName = devil_unit.LocalizedName.CreateCopy();
            devil_unit.LocalizedName.String = Helpers.CreateString(devil_unit.name + ".Name", "Devil Eidolon");

            devil_unit.Alignment = Alignment.LawfulEvil;
            devil_unit.Strength = 16;
            devil_unit.Dexterity = 12;
            devil_unit.Constitution = 13;
            devil_unit.Intelligence = 7;
            devil_unit.Wisdom = 10;
            devil_unit.Charisma = 11;
            devil_unit.Speed = 30.Feet();
            devil_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2, fx_feature }; // { natural_armor2, fx_feature };
            devil_unit.Body = devil_unit.Body.CloneObject();
            devil_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            devil_unit.Body.PrimaryHand = null;
            devil_unit.Body.SecondaryHand = null;
            devil_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            //devil_unit.Size = Size.Large;
            devil_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[0];
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillAthletics, StatType.SkillMobility, StatType.SkillStealth };
                a.Selections = new SelectionEntry[0];
            });
            devil_unit.AddComponents(Helpers.Create<EidolonComponent>());


            Helpers.SetField(devil_unit, "m_Portrait", Helpers.createPortrait("EidolonDevilProtrait", "Devil", ""));
            devil_eidolon = Helpers.CreateProgression("DevilEidolonProgression",
                                                        "Devil Eidolon",
                                                        "Corruptors, tempters, and despoilers, devil eidolons often serve their summoners obediently and efficiently, all in a long-term attempt to damn the summoner’s soul to the deepest depths of Hell. While some types of devils have truly unusual forms, devil eidolons have found that the more traditional bipedal form allows them to build up a strong rapport with their summoners—and consequently to corrupt them—more easily than if they possessed a more monstrous appearance.",
                                                        "",
                                                        Helpers.GetIcon("e76a774cacfb092498177e6ca706064d"), //infernal bloodline
                                                        FeatureGroup.AnimalCompanion,
                                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                                        );
            devil_eidolon.IsClassFeature = true;
            devil_eidolon.ReapplyOnLevelUp = true;
            devil_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            devil_eidolon.AddComponent(Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.LawfulEvil | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralEvil | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.LawfulNeutral));
            devil_eidolon.ReplaceComponent<AddPet>(a => a.Pet = devil_unit);

            Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(devil_eidolon);


            var infernal_unit = library.CopyAndAdd<BlueprintUnit>(devil_unit, "InfernalEidolonUnit", "");
            infernal_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[] { infernal_archetype };
            });
            infernal_eidolon = library.CopyAndAdd<BlueprintProgression>(devil_eidolon, "InfernalEidolonProgression", "");
            infernal_eidolon.SetNameDescription("Infernal Binding",
                                                "A devil binder must select an eidolon of the devil subtype. The devil binder’s eidolon never increases its maximum number of attacks, and its base attack bonus is equal to half its Hit Dice. At 4th level and every 4 levels thereafter, the eidolon’s Charisma score increases by 2.");
            infernal_eidolon.ReplaceComponent<AddPet>(a => a.Pet = infernal_unit);
            infernal_eidolon.ReplaceComponent<PrerequisiteAlignment>(p => p.Alignment = Kingmaker.UnitLogic.Alignments.AlignmentMaskType.LawfulEvil | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.LawfulNeutral);
        }


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
                                                  "Starting at 1st level, devil eidolons gain the claws evolution, resistance (fire) evolution and the skilled (Persuasion) evolution. They also gain a +4 bonus on saving throws against poison.",
                                                  "",
                                                  devil_eidolon.Icon,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("DevilEidolonLevel1AddFeature",
                                                                                Common.createContextSavingThrowBonusAgainstDescriptor(4, ModifierDescriptor.UntypedStackable, SpellDescriptor.Poison)
                                                                                ),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.claws),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.skilled[1]),
                                                  Helpers.CreateAddFeatureOnClassLevel(base_evolutions, 32, Summoner.getSummonerArray(), before: true)
                                                  );

            var feature4 = Helpers.CreateFeature("DevilEidolonLevel4Feature",
                                                  "Resistance",
                                                  "At 4th level, devil eidolons gain acid resistance 10 and cold resistance 10.",
                                                  "",
                                                  Helpers.GetIcon("21ffef7791ce73f468b6fca4d9371e8b"), //resist energy
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("DevilEidolonLevel4AddFeature",
                                                                                Common.createEnergyDR(10, DamageEnergyType.Acid),
                                                                                Common.createEnergyDR(10, DamageEnergyType.Cold)
                                                                                )
                                                  );

            var feature8 = Helpers.CreateFeature("DevilEidolonLevel8Feature",
                                                  "Poison Immunity",
                                                  "At 8th level, devil eidolons gain immunity to poison.",
                                                  "",
                                                  Helpers.GetIcon("b48b4c5ffb4eab0469feba27fc86a023"), //delay poison
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("DevilEidolonLevel8AddFeature",
                                                                                Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Poison),
                                                                                Common.createBuffDescriptorImmunity(SpellDescriptor.Poison)
                                                                                )
                                                  );

            var feature12 = Helpers.CreateFeature("DevilEidolonLevel12Feature",
                                                  "Damage Reduction",
                                                  "At 12th level, devil eidolons gain DR 5/good.",
                                                  "",
                                                  Helpers.GetIcon("9e1ad5d6f87d19e4d8883d63a6e35568"), //mage armor
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("DevilEidolonLevel12AddFeature",
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

            var feature16 = Helpers.CreateFeature("DevilEidolonLevel16Feature",
                                                  "Fire Immunity",
                                                  "At 16th level, devil eidolons lose the resistance (fire) evolution, and instead gain the immunity (fire) evolution.",
                                                  "",
                                                  Helpers.GetIcon("d2f116cfe05fcdd4a94e80143b67046f"), //protection from energy,
                                                  FeatureGroup.None,
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.immunity[3])
                                                  );

            var regeneration_buff = Helpers.CreateBuff("DevilEidolonRegenerationBuff",
                                                       "Regeneration",
                                                       "At 20th level, devil eidolons gain regeneration 5 (good weapons, good spells). They are still banished to Hell as normal for eidolons if they take enough damage.",
                                                       "",
                                                       Helpers.GetIcon("b6afdc50876e08149b1f9fdcdb2a308c"), //fiend of the pit = rage,
                                                       null,
                                                       Helpers.Create<AddEffectRegeneration>(a => { a.Heal = 5; a.CancelDamageAlignmentTypes = new DamageAlignment[] { DamageAlignment.Good }; })
                                                       );
            var feature20 = Helpers.CreateFeature("DevilEidolonLevel20Feature",
                                                  regeneration_buff.Name,
                                                  regeneration_buff.Description,
                                                  "",
                                                  regeneration_buff.Icon,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("DevilEidolonLevel20AddFeature",
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
            addLesserEidolon(devil_eidolon);
        }
    }
}
