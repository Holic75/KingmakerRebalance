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
        static void createSerpentineWaterElementalUnit()
        {
            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var elemental = library.Get<BlueprintUnit>("9922c4c5d1ec4cf409cf3b4742c90b51");
            var water_elemental = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "SerpentineWaterElementalEidolonUnit", "");

            water_elemental.Color = elemental.Color;
            water_elemental.Visual = elemental.Visual;

            water_elemental.LocalizedName = water_elemental.LocalizedName.CreateCopy();
            water_elemental.LocalizedName.String = Helpers.CreateString(water_elemental.name + ".Name", "Elemental Eidolon (Water)");

            water_elemental.Prefab = elemental.Prefab;
            water_elemental.Alignment = Alignment.TrueNeutral;
            water_elemental.Strength = 12;
            water_elemental.Dexterity = 16;
            water_elemental.Constitution = 13;
            water_elemental.Intelligence = 7;
            water_elemental.Wisdom = 10;
            water_elemental.Charisma = 11;
            water_elemental.Speed = 20.Feet();
            water_elemental.AddFacts = new BlueprintUnitFact[] { natural_armor2}; // { natural_armor2, fx_feature };
            water_elemental.Body = water_elemental.Body.CloneObject();
            water_elemental.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            water_elemental.Body.PrimaryHand = library.Get<BlueprintItemWeapon>("a000716f88c969c499a535dadcf09286"); //bite 1d6
            water_elemental.Body.SecondaryHand = null;
            water_elemental.Body.AdditionalSecondaryLimbs = new BlueprintItemWeapon[] { library.Get<BlueprintItemWeapon>("b21cd5b03fbb0f542815580e66f85915") }; //tail 1d6
            water_elemental.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            water_elemental.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[] { serpentine_archetype };
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillPerception, StatType.SkillStealth, StatType.SkillLoreReligion };
                a.Selections = new SelectionEntry[0];
            });
            water_elemental.AddComponents(Helpers.Create<EidolonComponent>());
            Helpers.SetField(water_elemental, "m_Portrait", Helpers.createPortrait("EidolonSerpentineWaterElementalProtrait", "WaterElementalSerpentine", ""));


            water_serpentine_eidolon = Helpers.CreateProgression("SerpentineWaterElementalEidolonProgression",
                                                        "Elemental Eidolon (Water, Serpentine)",
                                                        "Pulled in from one of the four elemental planes, these eidolons are linked to one of the four elements: air, earth, fire, or water. Generally, an elemental eidolon appears as a creature made entirely of one element, but there is some variation. Elemental eidolons are decidedly moderate in their views and actions. They tend to avoid the conflicts of others when they can and seek to maintain balance. The only exception is when facing off against emissaries of their opposing elements, which they hate utterly.",
                                                        "",
                                                        Helpers.GetIcon("650f8c91aaa5b114db83f541addd66d6"), //summon elemental
                                                        FeatureGroup.AnimalCompanion,
                                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                                       );
            water_serpentine_eidolon.IsClassFeature = true;
            water_serpentine_eidolon.ReapplyOnLevelUp = true;
            water_serpentine_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            water_serpentine_eidolon.AddComponent(Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.ChaoticNeutral
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.LawfulNeutral
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralEvil
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralGood
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.TrueNeutral));
            water_serpentine_eidolon.ReplaceComponent<AddPet>(a => a.Pet = water_elemental);

            Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(water_serpentine_eidolon);
        }


        static void createWaterElementalUnit()
        {
            var fx_buff = Helpers.CreateBuff("WaterElementalEidolonFxBuff",
                                             "",
                                             "",
                                             "",
                                             null,
                                             Common.createPrefabLink("191b45b04a55aef4fa8b0d63992dbb16"));
            fx_buff.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);

            var water_fx_feature = Helpers.CreateFeature("WaterElementalEidolonFxFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Helpers.Create<UnitViewMechanics.ReplaceUnitView>(r => r.prefab = Common.createUnitViewLink("9269eeeee9f80b4498a3e5bcce90e77a"/*"f5e1fc6f049cd55478fd31ace4d35ca1"*/)),
                                                   Common.createAuraFeatureComponent(fx_buff));
            water_fx_feature.HideInCharacterSheetAndLevelUp = true;
            water_fx_feature.HideInUI = true;

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var water_elemental = library.Get<BlueprintUnit>("9922c4c5d1ec4cf409cf3b4742c90b51");
            var water_elemental_unit = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "WaterElementalEidolonUnit", "");
            water_elemental_unit.Color = water_elemental.Color;

            water_elemental_unit.Visual = water_elemental.Visual;
            water_elemental_unit.LocalizedName = water_elemental_unit.LocalizedName.CreateCopy();
            water_elemental_unit.LocalizedName.String = Helpers.CreateString(water_elemental_unit.name + ".Name", "Elemental Eidolon (Water)");

            water_elemental_unit.Strength = 16;
            water_elemental_unit.Dexterity = 12;
            water_elemental_unit.Constitution = 13;
            water_elemental_unit.Intelligence = 7;
            water_elemental_unit.Wisdom = 10;
            water_elemental_unit.Charisma = 11;
            water_elemental_unit.Speed = 30.Feet();
            water_elemental_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2, water_fx_feature }; // { natural_armor2, fx_feature };
            water_elemental_unit.Body = water_elemental_unit.Body.CloneObject();
            water_elemental_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            water_elemental_unit.Body.PrimaryHand = null;
            water_elemental_unit.Body.SecondaryHand = null;
            water_elemental_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            water_elemental_unit.Gender = Gender.Female;
            water_elemental_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[0];
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillPerception, StatType.SkillLoreReligion, StatType.SkillStealth };
                a.DoNotApplyAutomatically = true;
                a.Selections = new SelectionEntry[0];
            });
            water_elemental_unit.AddComponents(Helpers.Create<EidolonComponent>());


            Helpers.SetField(water_elemental_unit, "m_Portrait", Helpers.createPortrait("EidolonWaterElementalProtrait", "WaterElemental", ""));

            water_elemental_eidolon = Helpers.CreateProgression("WaterElementalEidolonProgression",
                                                        "Elemental Eidolon (Water)",
                                                        "Pulled in from one of the four elemental planes, these eidolons are linked to one of the four elements: air, earth, fire, or water. Generally, an elemental eidolon appears as a creature made entirely of one element, but there is some variation. Elemental eidolons are decidedly moderate in their views and actions. They tend to avoid the conflicts of others when they can and seek to maintain balance. The only exception is when facing off against emissaries of their opposing elements, which they hate utterly.",
                                                        "",
                                                        Helpers.GetIcon("650f8c91aaa5b114db83f541addd66d6"), //summon elemental
                                                        FeatureGroup.AnimalCompanion,
                                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                                        );
            water_elemental_eidolon.IsClassFeature = true;
            water_elemental_eidolon.ReapplyOnLevelUp = true;
            water_elemental_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            water_elemental_eidolon.AddComponent(Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.ChaoticNeutral
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.LawfulNeutral
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralEvil
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralGood
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.TrueNeutral));
            water_elemental_eidolon.ReplaceComponent<AddPet>(a => a.Pet = water_elemental_unit);
            Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(water_elemental_eidolon);
        }


        static void fillWaterElementalProgression()
        {
            var feature1s = Helpers.CreateFeature("SerpentineWaterElementalEidolonLevel1Feature",
                                      "Base Evolutions",
                                      "At 1st level, water elemental eidolons gain immunity to paralysis and sleep and the bite, tail slap and immunity (cold) evolutions.",
                                      "",
                                      water_elemental_eidolon.Icon,
                                      FeatureGroup.None,
                                      addTransferableFeatToEidolon("SerpentineWaterElementalEidolonLevel1AddFeature",
                                                                    Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Paralysis | SpellDescriptor.Sleep),
                                                                    Common.createBuffDescriptorImmunity(SpellDescriptor.Paralysis | SpellDescriptor.Sleep),
                                                                    Common.createAddConditionImmunity(UnitCondition.Paralyzed),
                                                                    Common.createAddConditionImmunity(UnitCondition.Sleeping)
                                                                    ),
                                      Helpers.Create<EvolutionMechanics.AddFakeEvolution>(a => a.Feature = Evolutions.bite),
                                      Helpers.Create<EvolutionMechanics.AddFakeEvolution>(a => a.Feature = Evolutions.tail_slap),
                                      Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.immunity[1])
                                      );

            var feature1 = Helpers.CreateFeature("WaterElementalEidolonLevel1Feature",
                                                  "Base Evolutions",
                                                  "At 1st level, water elemental eidolons gain immunity to paralysis and sleep and the slam and immunity (cold) evolutions.",
                                                  "",
                                                  water_elemental_eidolon.Icon,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("WaterElementalEidolonLevel1AddFeature",
                                                                                Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Paralysis | SpellDescriptor.Sleep),
                                                                                Common.createBuffDescriptorImmunity(SpellDescriptor.Paralysis | SpellDescriptor.Sleep),
                                                                                Common.createAddConditionImmunity(UnitCondition.Paralyzed),
                                                                                Common.createAddConditionImmunity(UnitCondition.Sleeping)
                                                                                ),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.slam_biped),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.immunity[1])
                                                  );

            var feature4 = Helpers.CreateFeature("WaterElementalEidolonLevel4Feature",
                                                  "Evolution Pool Increase",
                                                  "At 4th level, all elemental eidolons add 1 point to their evolution pools.",
                                                  "",
                                                  null,
                                                  FeatureGroup.None,
                                                  Helpers.Create<EvolutionMechanics.IncreaseEvolutionPool>(i => i.amount = 1)
                                                  );

            var feature8 = Helpers.CreateFeature("WaterElementalEidolonLevel8Feature",
                                                  "Combat Maneuver Immunity",
                                                  "At 8th level, water elemental eidolons gain immunity to combat maneuvers.",
                                                  "",
                                                  Helpers.GetIcon("737ef897849327b45b88b83a797918c8"), //freedom of movement
                                                  FeatureGroup.None,
                                                  Helpers.Create<AddFeatureToCompanion>(a => a.Feature = library.Get<BlueprintFeature>("737ef897849327b45b88b83a797918c8"))
                                                  );

            var feature12 = Helpers.CreateFeature("WaterElementalEidolonLevel12Feature",
                                                  "Immunity",
                                                  "At 12th level, all elemental eidolons gain immunity to bleed, poison, and stun. In addition, they can no longer be flanked.",
                                                  "",
                                                  Helpers.GetIcon("21ffef7791ce73f468b6fca4d9371e8b"), //resist energy,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("WaterElementalEidolonLevel12AddFeature",
                                                                                Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Poison | SpellDescriptor.Bleed | SpellDescriptor.Stun),
                                                                                Common.createBuffDescriptorImmunity(SpellDescriptor.Poison | SpellDescriptor.Bleed | SpellDescriptor.Stun),
                                                                                Common.createAddConditionImmunity(UnitCondition.Stunned),
                                                                                Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.CannotBeFlanked)
                                                                                )
                                                  );

            var feature16 = Helpers.CreateFeature("WaterElementalEidolonLevel16Feature",
                                                  "Immunity II",
                                                  "At 16th level, all elemental eidolons gain immunity to critical hits and do not take additional damage from precision-based attacks, such as sneak attack.",
                                                  "",
                                                  Helpers.GetIcon("d2f116cfe05fcdd4a94e80143b67046f"), //protection from energy,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("WaterElementalEidolonLevel16AddFeature",
                                                                                Helpers.Create<AddImmunityToPrecisionDamage>(),
                                                                                Helpers.Create<AddImmunityToCriticalHits>()
                                                                                )
                                                  );

            var wildshape_water_elemental_freeze = library.CopyAndAdd<BlueprintFeature>("83427fd78a6e91847a3bb419076b7705", "WaterElementalEidolonLevel20FreezeFeature", "");
            wildshape_water_elemental_freeze.ReplaceComponent<ContextCalculateAbilityParamsBasedOnClass>(Common.createContextCalculateAbilityParamsBasedOnClasses(new BlueprintCharacterClass[] { Eidolon.eidolon_class, Summoner.summoner_class }, StatType.Constitution));
            transferable_abilities.Add(wildshape_water_elemental_freeze);
            var feature20 = Helpers.CreateFeature("WaterElementalEidolonLevel20Feature",
                                                  "Freeze",
                                                  "At 20th level, water elemental eidolons gain the freeze ability as large water elementals.",
                                                  "",
                                                  wildshape_water_elemental_freeze.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.Create<AddFeatureToCompanion>(a => a.Feature = wildshape_water_elemental_freeze)
                                                  );

            water_elemental_eidolon.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, feature1),
                                                           Helpers.LevelEntry(4, feature4),
                                                           Helpers.LevelEntry(8, feature8),
                                                           Helpers.LevelEntry(12, feature12),
                                                           Helpers.LevelEntry(16, feature16),
                                                           Helpers.LevelEntry(20, feature20)
                                                           };
            water_elemental_eidolon.UIGroups = Helpers.CreateUIGroups(feature1, feature4, feature8, feature12, feature16, feature20);


            water_serpentine_eidolon.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, feature1s),
                                                           Helpers.LevelEntry(4, feature4),
                                                           Helpers.LevelEntry(8, feature8),
                                                           Helpers.LevelEntry(12, feature12),
                                                           Helpers.LevelEntry(16, feature16),
                                                           Helpers.LevelEntry(20, feature20)
                                                           };
            water_serpentine_eidolon.UIGroups = Helpers.CreateUIGroups(feature1s, feature4, feature8, feature12, feature16, feature20);
            addLesserEidolon(water_elemental_eidolon);
            addLesserEidolon(water_serpentine_eidolon);
        }
    }
}

