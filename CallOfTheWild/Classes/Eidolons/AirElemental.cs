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
        static void createQuadrupedAirElementalUnit()
        {
            var fx_buff = Helpers.CreateBuff("QuadrupedAirElementalEidolonFxBuff",
                                             "",
                                             "",
                                             "",
                                             null,
                                             Common.createPrefabLink("6035a889bae45f242908569a7bc25c93"));
            fx_buff.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);
            var fx_buff2 = Helpers.CreateBuff("QuadrupedAirElementalEidolonFx2Buff",
                                 "",
                                 "",
                                 "",
                                 null,
                                 Common.createPrefabLink("e013bfb804dc9744e8d127634d71e13e"));
            fx_buff2.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);

            var fx_feature = Helpers.CreateFeature("QuadrupedAirElementalEidolonFxFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Common.createAuraFeatureComponent(fx_buff),
                                                   Common.createAuraFeatureComponent(fx_buff2),
                                                   Helpers.Create<SizeMechanics.PermanentSizeOverride>(p => p.size = Size.Medium));
            fx_feature.HideInCharacterSheetAndLevelUp = true;
            fx_feature.HideInUI = true;

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var air_elemental = library.Get<BlueprintUnit>("f739047597b7a2849b14def122e1ee0d");
            var air_elemental_unit = library.CopyAndAdd<BlueprintUnit>("54cf380dee486ff42b803174d1b9da1b", "QuadrupedAirElementalEidolonUnit", "");
            air_elemental_unit.Color = air_elemental.Color;

            air_elemental_unit.Visual = air_elemental.Visual;
            air_elemental_unit.LocalizedName = air_elemental_unit.LocalizedName.CreateCopy();
            air_elemental_unit.LocalizedName.String = Helpers.CreateString(air_elemental_unit.name + ".Name", "Elemental Eidolon (Air)");

            air_elemental_unit.Strength = 14;
            air_elemental_unit.Dexterity = 14;
            air_elemental_unit.Constitution = 13;
            air_elemental_unit.Intelligence = 7;
            air_elemental_unit.Wisdom = 10;
            air_elemental_unit.Charisma = 11;
            air_elemental_unit.Speed = 40.Feet();
            air_elemental_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2, fx_feature }; // { natural_armor2, fx_feature };
            air_elemental_unit.Body = air_elemental_unit.Body.CloneObject();
            air_elemental_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            air_elemental_unit.Body.PrimaryHand = library.Get<BlueprintItemWeapon>("a000716f88c969c499a535dadcf09286"); //bite 1d6
            air_elemental_unit.Body.SecondaryHand = null;
            air_elemental_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            air_elemental_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[] { quadruped_archetype };
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillPerception, StatType.SkillStealth, StatType.SkillLoreReligion };
                a.DoNotApplyAutomatically = true;
                a.Selections = new SelectionEntry[0];
            });
            air_elemental_unit.AddComponents(Helpers.Create<EidolonComponent>());


            Helpers.SetField(air_elemental_unit, "m_Portrait", Helpers.createPortrait("EidolonQuadrupedAirElementalProtrait", "AirElementalQuadruped", ""));

            air_quadruped_eidolon = Helpers.CreateProgression("QuadrupedAirElementalEidolonProgression",
                                                        "Elemental Eidolon (Air, Quadruped)",
                                                        "Pulled in from one of the four elemental planes, these eidolons are linked to one of the four elements: air, earth, fire, or water. Generally, an elemental eidolon appears as a creature made entirely of one element, but there is some variation. Elemental eidolons are decidedly moderate in their views and actions. They tend to avoid the conflicts of others when they can and seek to maintain balance. The only exception is when facing off against emissaries of their opposing elements, which they hate utterly.",
                                                        "",
                                                        Helpers.GetIcon("650f8c91aaa5b114db83f541addd66d6"), //summon elemental
                                                        FeatureGroup.AnimalCompanion,
                                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                                        );
            air_quadruped_eidolon.IsClassFeature = true;
            air_quadruped_eidolon.ReapplyOnLevelUp = true;
            air_quadruped_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            air_quadruped_eidolon.AddComponent(Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.ChaoticNeutral
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.LawfulNeutral
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralEvil
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralGood
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.TrueNeutral));
            air_quadruped_eidolon.ReplaceComponent<AddPet>(a => a.Pet = air_elemental_unit);
            Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(air_quadruped_eidolon);
        }


        static void createAirElementalUnit()
        {
            var fx_buff = Helpers.CreateBuff("AirElementalEidolonFxBuff",
                                             "",
                                             "",
                                             "",
                                             null,
                                             Common.createPrefabLink("6035a889bae45f242908569a7bc25c93"));
            fx_buff.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);
            var fx_buff2 = Helpers.CreateBuff("AirElementalEidolonFx2Buff",
                                 "",
                                 "",
                                 "",
                                 null,
                                 Common.createPrefabLink("e013bfb804dc9744e8d127634d71e13e"));
            fx_buff2.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);

            var fx_feature = Helpers.CreateFeature("AirElementalEidolonFxFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Helpers.Create<UnitViewMechanics.ReplaceUnitView>(r => r.prefab = Common.createUnitViewLink(/*"9269eeeee9f80b4498a3e5bcce90e77a"*/"f5e1fc6f049cd55478fd31ace4d35ca1")),
                                                   Common.createAuraFeatureComponent(fx_buff),
                                                   Common.createAuraFeatureComponent(fx_buff2));
            fx_feature.HideInCharacterSheetAndLevelUp = true;
            fx_feature.HideInUI = true;

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var air_elemental = library.Get<BlueprintUnit>("f739047597b7a2849b14def122e1ee0d");
            var air_elemental_unit = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "AirElementalEidolonUnit", "");
            air_elemental_unit.Color = air_elemental.Color;

            air_elemental_unit.Visual = air_elemental.Visual;
            air_elemental_unit.LocalizedName = air_elemental_unit.LocalizedName.CreateCopy();
            air_elemental_unit.LocalizedName.String = Helpers.CreateString(air_elemental_unit.name + ".Name", "Elemental Eidolon (Air)");

            air_elemental_unit.Strength = 16;
            air_elemental_unit.Dexterity = 12;
            air_elemental_unit.Constitution = 13;
            air_elemental_unit.Intelligence = 7;
            air_elemental_unit.Wisdom = 10;
            air_elemental_unit.Charisma = 11;
            air_elemental_unit.Speed = 30.Feet();
            air_elemental_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2, fx_feature }; // { natural_armor2, fx_feature };
            air_elemental_unit.Body = air_elemental_unit.Body.CloneObject();
            air_elemental_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            air_elemental_unit.Body.PrimaryHand = null;
            air_elemental_unit.Body.SecondaryHand = null;
            air_elemental_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            air_elemental_unit.Gender = Gender.Female;
            air_elemental_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[0];
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillPerception, StatType.SkillLoreReligion, StatType.SkillStealth };
                a.DoNotApplyAutomatically = true;
                a.Selections = new SelectionEntry[0];
            });
            air_elemental_unit.AddComponents(Helpers.Create<EidolonComponent>());


            Helpers.SetField(air_elemental_unit, "m_Portrait", Helpers.createPortrait("EidolonAirElementalProtrait", "AirElemental", ""));

            air_elemental_eidolon = Helpers.CreateProgression("AirElementalEidolonProgression",
                                                        "Elemental Eidolon (Air)",
                                                        "Pulled in from one of the four elemental planes, these eidolons are linked to one of the four elements: air, earth, fire, or water. Generally, an elemental eidolon appears as a creature made entirely of one element, but there is some variation. Elemental eidolons are decidedly moderate in their views and actions. They tend to avoid the conflicts of others when they can and seek to maintain balance. The only exception is when facing off against emissaries of their opposing elements, which they hate utterly.",
                                                        "",
                                                        Helpers.GetIcon("650f8c91aaa5b114db83f541addd66d6"), //summon elemental
                                                        FeatureGroup.AnimalCompanion,
                                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                                        );
            air_elemental_eidolon.IsClassFeature = true;
            air_elemental_eidolon.ReapplyOnLevelUp = true;
            air_elemental_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            air_elemental_eidolon.AddComponent(Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.ChaoticNeutral
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.LawfulNeutral
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralEvil
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralGood
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.TrueNeutral));
            air_elemental_eidolon.ReplaceComponent<AddPet>(a => a.Pet = air_elemental_unit);
            Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(air_elemental_eidolon);
        }


        static void fillAirElementalProgression()
        {
            var feature1 = Helpers.CreateFeature("AirElementalEidolonLevel1Feature",
                                                  "Base Evolutions",
                                                  "At 1st level, air elemental eidolons gain immunity to paralysis and sleep and the slam and immunity (electricity) evolutions.",
                                                  "",
                                                  air_elemental_eidolon.Icon,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("AirElementalEidolonLevel1AddFeature",
                                                                                Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Paralysis | SpellDescriptor.Sleep),
                                                                                Common.createBuffDescriptorImmunity(SpellDescriptor.Paralysis | SpellDescriptor.Sleep),
                                                                                Common.createAddConditionImmunity(UnitCondition.Paralyzed),
                                                                                Common.createAddConditionImmunity(UnitCondition.Sleeping)
                                                                                ),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.slam_biped),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.immunity[2])
                                                  );

            var feature1q = Helpers.CreateFeature("QuadrupedAirElementalEidolonLevel1Feature",
                                      "Base Evolutions",
                                      "At 1st level, air elemental eidolons gain immunity to paralysis and sleep and the bite and immunity (electricity) evolutions.",
                                      "",
                                      air_elemental_eidolon.Icon,
                                      FeatureGroup.None,
                                      addTransferableFeatToEidolon("QuadrupedAirElementalEidolonLevel1AddFeature",
                                                                    Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Paralysis | SpellDescriptor.Sleep),
                                                                    Common.createBuffDescriptorImmunity(SpellDescriptor.Paralysis | SpellDescriptor.Sleep),
                                                                    Common.createAddConditionImmunity(UnitCondition.Paralyzed),
                                                                    Common.createAddConditionImmunity(UnitCondition.Sleeping)
                                                                    ),
                                      Helpers.Create<EvolutionMechanics.AddFakeEvolution>(a => a.Feature = Evolutions.bite),
                                      Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.immunity[2])
                                      );

            var feature4 = Helpers.CreateFeature("AirElementalEidolonLevel4Feature",
                                                  "Evolution Pool Increase",
                                                  "At 4th level, all elemental eidolons add 1 point to their evolution pools.",
                                                  "",
                                                  null,
                                                  FeatureGroup.None,
                                                  Helpers.Create<EvolutionMechanics.IncreaseEvolutionPool>(i => i.amount = 1)
                                                  );

            var feature8 = Helpers.CreateFeature("AirElementalEidolonLevel8Feature",
                                                  "Flight",
                                                  "At 8th level, air elemental eidolons gain flight evolution.",
                                                  "",
                                                  Evolutions.flight.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.flight)
                                                  );

            var feature12 = Helpers.CreateFeature("AirElementalEidolonLevel12Feature",
                                                  "Immunity",
                                                  "At 12th level, all elemental eidolons gain immunity to bleed, poison, and stun. In addition, they can no longer be flanked.",
                                                  "",
                                                  Helpers.GetIcon("21ffef7791ce73f468b6fca4d9371e8b"), //resist energy,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("AirElementalEidolonLevel12AddFeature",
                                                                                Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Poison | SpellDescriptor.Bleed | SpellDescriptor.Stun),
                                                                                Common.createBuffDescriptorImmunity(SpellDescriptor.Poison | SpellDescriptor.Bleed | SpellDescriptor.Stun),
                                                                                Common.createAddConditionImmunity(UnitCondition.Stunned),
                                                                                Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.CannotBeFlanked)
                                                                                )
                                                  );

            var feature16 = Helpers.CreateFeature("AirElementalEidolonLevel16Feature",
                                                  "Immunity II",
                                                  "At 16th level, all elemental eidolons gain immunity to critical hits and do not take additional damage from precision-based attacks, such as sneak attack.",
                                                  "",
                                                  Helpers.GetIcon("d2f116cfe05fcdd4a94e80143b67046f"), //protection from energy,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("AirElementalEidolonLevel16AddFeature",
                                                                                Helpers.Create<AddImmunityToPrecisionDamage>(),
                                                                                Helpers.Create<AddImmunityToCriticalHits>()
                                                                                )
                                                  );

            var feature20 = Helpers.CreateFeature("AirElementalEidolonLevel20Feature",
                                                  "Whirlwind",
                                                  "At 20th level, air elemental eidolons gain the whirlwind ability as large air elementals.",
                                                  "",
                                                  Helpers.GetIcon("2c746396b6ce0ab47821ad960ec44df6"), //whirlwind
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("AirElementalEidolonLevel20AddFeature",
                                                                                Helpers.CreateAddFact(library.Get<BlueprintActivatableAbility>("2c746396b6ce0ab47821ad960ec44df6"))
                                                                                )
                                                  ); 

            air_elemental_eidolon.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, feature1),
                                                           Helpers.LevelEntry(4, feature4),
                                                           Helpers.LevelEntry(8, feature8),
                                                           Helpers.LevelEntry(12, feature12),
                                                           Helpers.LevelEntry(16, feature16),
                                                           Helpers.LevelEntry(20, feature20)
                                                           };
            air_elemental_eidolon.UIGroups = Helpers.CreateUIGroups(feature1, feature4, feature8, feature12, feature16, feature20);

            air_quadruped_eidolon.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, feature1q),
                                                           Helpers.LevelEntry(4, feature4),
                                                           Helpers.LevelEntry(8, feature8),
                                                           Helpers.LevelEntry(12, feature12),
                                                           Helpers.LevelEntry(16, feature16),
                                                           Helpers.LevelEntry(20, feature20)
                                                           };
            air_quadruped_eidolon.UIGroups = Helpers.CreateUIGroups(feature1, feature4, feature8, feature12, feature16, feature20);
        }
    }
}
