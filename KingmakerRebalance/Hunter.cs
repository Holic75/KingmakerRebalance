using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;



namespace KingmakerRebalance
{
    public class Hunter
    {
        static internal readonly Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup AnimalFocusGroup = Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.Judgment;
        internal static BlueprintCharacterClass hunter_class;
        static internal LibraryScriptableObject library => Main.library;
        static internal BlueprintFeature animal_focus;
        static internal BlueprintFeature animal_focus_ac;
        static internal BlueprintFeature animal_focus_additional_use;
        static internal BlueprintFeature animal_focus_additional_use_ac;
        static internal BlueprintFeatureSelection hunter_animal_companion;
        static internal BlueprintFeatureSelection precise_companion;
        static internal BlueprintFeature hunter_teamwork_feat;
        static internal BlueprintFeature hunter_tactics;
        static internal BlueprintFeature hunter_woodland_stride;
        static internal BlueprintArchetype divine_hunter_archetype;
        static internal BlueprintFeatureSelection hunter_otherwordly_companion;

        static internal BlueprintFeature ac_smite_good_feature;
        static internal BlueprintAbility ac_smite_good_ability;

        static internal BlueprintFeature ac_smite_evil_feature;
        static internal BlueprintAbility ac_smite_evil_ability;

        internal static void createHunterClass()
        {
            var sacred_huntsmaster_archetype = ResourcesLibrary.TryGetBlueprint<BlueprintArchetype>("46eb929c8b6d7164188eb4d9bcd0a012");
            var ranger_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("cda0615668a6df14eb36ba19ee881af6");
            var druid_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");
            var inquisitor_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce");

            hunter_class = Helpers.Create<BlueprintCharacterClass>();
            hunter_class.name = "HunterClass";
            library.AddAsset(hunter_class, "32486dcfda61462fbfd66b5644786b39");
            createAnimalFocusFeat();
            hunter_class.LocalizedName = Helpers.CreateString("Hunter.Name", "Hunter");
            hunter_class.LocalizedDescription = Helpers.CreateString("Hunter.Description",
                "Hunters are warriors of the wilds that have forged close bonds with trusted animal companions.They focus their tactics on fighting alongside their companion animals as a formidable team of two.Able to cast a wide variety of nature spells and take on the abilities and attributes of beasts, hunters magically improve both themselves and their animal companions.\n"
                + "Role: Hunters can adapt their tactics to many kinds of opponents, and cherish their highly trained animal companions.As a team, the hunter and her companion can react to danger with incredible speed, making them excellent scouts, explorers, and saboteurs."
                );
            hunter_class.m_Icon = ranger_class.Icon;
            hunter_class.SkillPoints = ranger_class.SkillPoints;
            hunter_class.HitDie = DiceType.D8;
            hunter_class.BaseAttackBonus = druid_class.BaseAttackBonus;
            hunter_class.FortitudeSave = ranger_class.FortitudeSave;
            hunter_class.ReflexSave = ranger_class.FortitudeSave;
            hunter_class.WillSave = ranger_class.WillSave;
            hunter_class.Spellbook = createHunterSpellbook();
            hunter_class.ClassSkills = ranger_class.ClassSkills;
            hunter_class.IsDivineCaster = true;
            hunter_class.IsArcaneCaster = false;
            hunter_class.StartingGold = ranger_class.StartingGold;
            hunter_class.PrimaryColor = ranger_class.PrimaryColor;
            hunter_class.SecondaryColor = ranger_class.SecondaryColor;
            hunter_class.RecommendedAttributes = ranger_class.RecommendedAttributes;
            hunter_class.NotRecommendedAttributes = ranger_class.NotRecommendedAttributes;
            hunter_class.EquipmentEntities = ranger_class.EquipmentEntities;
            hunter_class.MaleEquipmentEntities = ranger_class.MaleEquipmentEntities;
            hunter_class.FemaleEquipmentEntities = ranger_class.FemaleEquipmentEntities;
            hunter_class.ComponentsArray = ranger_class.ComponentsArray;
            hunter_class.StartingItems = ranger_class.StartingItems;
            hunter_class.Progression = createHunterProgression();
            hunter_class.Archetypes = new BlueprintArchetype[0]; //Divine hunter, Forester (replace favored terrain attack bonus with racial enemies), Feykiller
            Helpers.RegisterClass(hunter_class);
        }


        static void createDivineHunterArchetype()
        {
            divine_hunter_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "DivineHunterHunterArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Divine Hunter");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "While most hunters heed the call of nature and fight to protect its bounty, some are inspired to serve a higher power. These divine hunters use faith to aid them in their struggles, and their faith infuses their animal companions, making these companions champions of their deities.\n"
                                                              + "Alignment: A divine hunter’s alignment must be within one step of her deity’s, along either the law / chaos axis or the good / evil axis. A divine hunter can otherwise be of any alignment.");

            });
            Helpers.SetField(divine_hunter_archetype, "m_ParentClass", hunter_class);
            library.AddAsset(divine_hunter_archetype, "bd650995013f4cb2b98b014b0639a46c");

            divine_hunter_archetype.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(3, hunter_tactics, hunter_teamwork_feat),
                                                                        Helpers.LevelEntry(6, hunter_teamwork_feat),
                                                                        Helpers.LevelEntry(9, hunter_teamwork_feat),
                                                                        Helpers.LevelEntry(12, hunter_teamwork_feat),
                                                                        Helpers.LevelEntry(15, hunter_teamwork_feat),
                                                                        Helpers.LevelEntry(18, hunter_teamwork_feat)
                                                                       };
            var diety_selection = library.Get<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4");
            var domain_selection = library.Get<BlueprintFeatureSelection>("48525e5da45c9c243a343fc6545dbdb9");
            //add divine_hunter to all domains
            //this will make hunter receive all domain bonuses starting from level 1 which will be a bit stronger than pnp version, but way simpler to implement
            Common.addClassToDomains(hunter_class, new BlueprintArchetype[] { divine_hunter_archetype }, Common.DomainSpellsType.NormalList, domain_selection);
            createOtherWordlyCompanion();

            divine_hunter_archetype.ClassSkills = hunter_class.ClassSkills.AddToArray(StatType.SkillLoreReligion);
            divine_hunter_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, diety_selection, domain_selection),
                                                                     Helpers.LevelEntry(3, hunter_otherwordly_companion)
                                                                   };


        }


        static void createOtherWordlyCompanion()
        {
            createSmiteGoodEvilAC();

            var celestial_bloodline = library.Get<Kingmaker.Blueprints.Classes.BlueprintProgression>("aa79c65fa0e11464d9d100b038c50796");
            var abbysal_bloodline = library.Get<Kingmaker.Blueprints.Classes.BlueprintProgression>("d3a4cb7be97a6694290f0dcfbd147113");

            var demonic_might = library.Get<Kingmaker.Blueprints.Classes.BlueprintFeature>("5c1c2ed7fe5f99649ab00605610b775b");
            var damage_resistance = library.Get<Kingmaker.Blueprints.Classes.BlueprintFeature>("8cbf303d479cf0d42a8e36092c76fa7c");
            var aura_of_heaven = library.Get<Kingmaker.Blueprints.Classes.BlueprintFeature>("2768c719ee7338c49932358c2c581bba");

            var ac_dr5_evil = Helpers.CreateFeature("AnimalCompanionDR5EvilFeature",
                                                "Celestial Damage Reduction",
                                                "Animal Companion gains damage reduction 5/Evil at level 5. It increases to damage reduction 10/Evil at level 11.",
                                                "368bc4311f7f4ba9af3752ff4418d0a8",
                                                aura_of_heaven.Icon,
                                                FeatureGroup.None,
                                                Common.createAlignmentDR(5, DamageAlignment.Evil)
                                                );

            var ac_dr10_evil = Helpers.CreateFeature("AnimalCompanionDR10EvilFeature",
                                    ac_dr5_evil.Name,
                                    ac_dr5_evil.Description,
                                    "025575448afb4d30b10015b1208938aa",
                                    aura_of_heaven.Icon,
                                    FeatureGroup.None,
                                    Common.createAlignmentDR(10, DamageAlignment.Evil)
                                    );
            ac_dr10_evil.HideInUI = true;
            ac_dr10_evil.HideInCharacterSheetAndLevelUp = true;


            var ac_dr5_good = Helpers.CreateFeature("AnimalCompanionDR5EvilFeature",
                                               "Fiendish Damage Reduction",
                                               "Animal Companion gains damage reduction 5/Good at level 5. It increases to damage reduction 10/Good at level 11.",
                                               "a203d617f8d547459e1f25790f886b6e",
                                               aura_of_heaven.Icon,
                                               FeatureGroup.None,
                                               Common.createAlignmentDR(5, DamageAlignment.Good)
                                               );

            var ac_dr10_good = Helpers.CreateFeature("AnimalCompanionDR10EvilFeature",
                                    ac_dr5_good.Name,
                                    ac_dr5_good.Description,
                                    "",
                                    aura_of_heaven.Icon,
                                    FeatureGroup.None,
                                    Common.createAlignmentDR(10, DamageAlignment.Good)
                                    );
            ac_dr10_good.HideInUI = true;
            ac_dr10_good.HideInCharacterSheetAndLevelUp = true;

            var ac_resist_cae5 = Helpers.CreateFeature("AnimalCompanionCelestialResist5Feature",
                        "Celestial Resistance",
                        "Animal commanpanion gains reist acid 5, resist cold 5 and resist electricity 5. At 5th level these resistances increase to 10, at 11th level to 15",
                        "46a19a521e0d40f792d8b4f64931be8a",
                        damage_resistance.Icon,
                        FeatureGroup.None,
                        Common.createEnergyDR(5, DamageEnergyType.Acid),
                        Common.createEnergyDR(5, DamageEnergyType.Cold),
                        Common.createEnergyDR(5, DamageEnergyType.Electricity)
                        );
            var ac_resist_cae10 = Helpers.CreateFeature("AnimalCompanionCelestialResist10Feature",
                        ac_resist_cae5.Name,
                        ac_resist_cae5.Description,
                        "53c2ebe9de684d8290386f3f67fef90b",
                        damage_resistance.Icon,
                        FeatureGroup.None,
                        Common.createEnergyDR(10, DamageEnergyType.Acid),
                        Common.createEnergyDR(10, DamageEnergyType.Cold),
                        Common.createEnergyDR(10, DamageEnergyType.Electricity)
                        );
            ac_resist_cae10.HideInUI = true;
            ac_resist_cae10.HideInCharacterSheetAndLevelUp = true;
            var ac_resist_cae15 = Helpers.CreateFeature("AnimalCompanionCelestialResist15Feature",
                    ac_resist_cae5.Name,
                    ac_resist_cae5.Description,
                    "3fe9c4c62055440f8491d3ce139011fe",
                    damage_resistance.Icon,
                    FeatureGroup.None,
                    Common.createEnergyDR(15, DamageEnergyType.Acid),
                    Common.createEnergyDR(15, DamageEnergyType.Cold),
                    Common.createEnergyDR(15, DamageEnergyType.Electricity)
                    );
            ac_resist_cae15.HideInUI = true;
            ac_resist_cae15.HideInCharacterSheetAndLevelUp = true;


            var ac_resist_cf5 = Helpers.CreateFeature("AnimalCompanionCelestialResist5Feature",
                        "Fiendish Resistance",
                        "Animal commanpanion gains reist resist cold 5 and resist fire 5. At 5th level these resistances increase to 10, at 11th level to 15",
                        "4170f7f5874a4e45bc7050a53727452f",
                        damage_resistance.Icon,
                        FeatureGroup.None,
                        Common.createEnergyDR(5, DamageEnergyType.Fire),
                        Common.createEnergyDR(5, DamageEnergyType.Cold)
                        );
            var ac_resist_cf10 = Helpers.CreateFeature("AnimalCompanionCelestialResist10Feature",
                        ac_resist_cf5.Name,
                        ac_resist_cf5.Description,
                        "1ae4fad1bff64c03b76979a896adf250",
                        damage_resistance.Icon,
                        FeatureGroup.None,
                        Common.createEnergyDR(10, DamageEnergyType.Fire),
                        Common.createEnergyDR(10, DamageEnergyType.Cold)
                        );
            ac_resist_cf10.HideInUI = true;
            ac_resist_cf10.HideInCharacterSheetAndLevelUp = true;
            var ac_resist_cf15 = Helpers.CreateFeature("AnimalCompanionCelestialResist15Feature",
                        ac_resist_cf5.Name,
                        ac_resist_cf5.Description,
                        "9f646d3d19d246c09988d2a0df2e4d92",
                        damage_resistance.Icon,
                        FeatureGroup.None,
                        Common.createEnergyDR(15, DamageEnergyType.Fire),
                        Common.createEnergyDR(15, DamageEnergyType.Cold)
                        );
            ac_resist_cf15.HideInUI = true;
            ac_resist_cf15.HideInCharacterSheetAndLevelUp = true;


            var ac_spell_resistance = Common.createSpellResistance("AnimalCompanionSpellResistanceFeature",
                                                               "Spell Resistance",
                                                               "Animal Companion gains spell resistance equal to its level + 6.",
                                                               "0e7481a8ceb041129a692bf59f24d057",
                                                               library.Get<BlueprintCharacterClass>("4cd1757a0eea7694ba5c933729a53920"),
                                                               6);
            var animal_companion_array = new BlueprintCharacterClass[] { library.Get<BlueprintCharacterClass>("4cd1757a0eea7694ba5c933729a53920") };
            var celestial_progression = Helpers.CreateProgression("CelestialCompanionProgression",
                                                  "Celestial Companion",
                                                  "Celestial creatures dwell in the higher planes, but can be summoned using spells such as summon monster and planar ally. Celestial creatures may use Smite Evil once per day, gain energy resistance 5 to acid, cold and fire, which increases to 10 at level 5 and to 15 at level 11. They also gain spell resistance equal to their level + 6. Starting from level 5 they also gain damage reduction 5/Evil which further increases to  10/Evil at level 11.",
                                                  "69f0d7d1077f492f8237952f8219a270",
                                                  celestial_bloodline.Icon,
                                                  FeatureGroup.None);
            celestial_progression.Classes = animal_companion_array;
            celestial_progression.LevelEntries = new LevelEntry[] { Helpers.LevelEntry(3, ac_smite_evil_feature, ac_spell_resistance, ac_resist_cae5),
                                                                    Helpers.LevelEntry(5, ac_resist_cae10, ac_dr5_evil),
                                                                    Helpers.LevelEntry(11, ac_resist_cae15, ac_dr10_evil)
                                                                  };
            var fiendish_progression = Helpers.CreateProgression("FiendishCompanionProgression",
                                                  "Fiendish Companion",
                                                  "Creatures with the fiendish template live in the Lower Planes, such as the Abyss and Hell, but can be summoned using spells such as summon monster and planar ally. Fiendish creatures may use Smite Good once per day, gain energy resistance 5 to cold and fire, which increases to 10 at level 5 and to 15 at level 11. They also gain spell resistance equal to their level + 6. Starting from level 5 they also gain damage reduction 5/Good which further increases to  10/Good at level 11.",
                                                  "3e33af2ab5974859bdaa92c32987b3e0",
                                                  abbysal_bloodline.Icon,
                                                  FeatureGroup.None);
            fiendish_progression.LevelEntries = new LevelEntry[] { Helpers.LevelEntry(3, ac_smite_good_feature, ac_spell_resistance, ac_resist_cf5),
                                                                    Helpers.LevelEntry(5, ac_resist_cf10, ac_dr5_good),
                                                                    Helpers.LevelEntry(11, ac_resist_cf15, ac_dr10_good)
                                                                  };

            hunter_otherwordly_companion = Helpers.CreateFeatureSelection("AnimalCompanionTemplateSelection",
                                           "Outwordly Companion",
                                           "At 3rd level, a hunter’s companion takes on otherworldly features. If the divine hunter is good (or worships a good deity), the animal companion gains the celestial template. If the hunter is evil (or worships an evil deity), the animal companion gains the fiendish template. If the hunter is neutral and worships a neutral deity, she must choose either the celestial or fiendish template; once this choice is made, it cannot be changed.",
                                           "1936995e234b4d2e8dbddc935e731254",
                                           null,
                                           FeatureGroup.None);
            var channel_positive_allowed = library.Get<Kingmaker.Blueprints.Classes.BlueprintFeature>("8c769102f3996684fb6e09a2c4e7e5b9");
            var channel_negative_allowed = library.Get<Kingmaker.Blueprints.Classes.BlueprintFeature>("dab5255d809f77c4395afc2b713e9cd6");

            hunter_otherwordly_companion.AllFeatures = new BlueprintFeature[] {Helpers.CreateFeature("CelestialCompanionTemplateFeature",
                                                                                          celestial_progression.Name,
                                                                                          celestial_progression.Description,
                                                                                          "4eff84c8f4a740b28f18587cdeb0c41d",
                                                                                          celestial_bloodline.Icon,
                                                                                          FeatureGroup.None,
                                                                                          createAddFeatToAnimalCompanion(celestial_progression),
                                                                                          Helpers.PrerequisiteFeature(channel_positive_allowed)
                                                                                          ),
                                                                                          Helpers.CreateFeature("FiendishCompanionTemplateFeature",
                                                                                          fiendish_progression.Name,
                                                                                          fiendish_progression.Description,
                                                                                          "76784350237247aab40ebdcc6107794d",
                                                                                          abbysal_bloodline.Icon,
                                                                                          FeatureGroup.None,
                                                                                          createAddFeatToAnimalCompanion(fiendish_progression),
                                                                                          Helpers.PrerequisiteFeature(channel_negative_allowed)
                                                                                          )
                                                                                };
        }


        static void createSmiteGoodEvilAC()
        {
            var animal_companion_array = new BlueprintCharacterClass[] {library.Get<BlueprintCharacterClass>("4cd1757a0eea7694ba5c933729a53920") };

            ac_smite_good_ability = library.CopyAndAdd<BlueprintAbility>("7bb9eb2042e67bf489ccd1374423cdec", "SmiteGoodACAbility", "eeb6e25da78e4ac99f73024eaf54718e");
            ac_smite_good_feature = library.CopyAndAdd<BlueprintFeature>("3a6db57fce75b0244a6a5819528ddf26", "SmiteGoodACFeature", "250a6fed6c9a4de1b8483aae07728c62");
            var smite_good_resource = library.CopyAndAdd<BlueprintAbilityResource>("b4274c5bb0bf2ad4190eb7c44859048b", "SmiteGoodResource", "0686f01667e24299834545aa98b29b6e");

            ac_smite_good_feature.GetComponent<Kingmaker.UnitLogic.FactLogic.AddFacts>().Facts[0] = ac_smite_good_ability;
            ac_smite_good_feature.GetComponent<Kingmaker.Designers.Mechanics.Facts.AddAbilityResources>().Resource = smite_good_resource;
            ac_smite_good_feature.SetName("Smite Good");
            ac_smite_good_feature.SetDescription("A character can call out to the powers of evil to aid her in her struggle against good. As a swift action, the character chooses one target within sight to smite. If this target is good, the character adds her Cha bonus (if any) to her attack rolls and adds her class level to all damage rolls made against the target of her smite, smite evil attacks automatically bypass any DR the creature might possess.\nIn addition, while smite good is in effect, the character gains a deflection bonus equal to her Charisma modifier (if any) to her AC against attacks made by the target of the smite. If the character targets a creature that is not good, the smite is wasted with no effect.\nThe smite good lasts until the target dies or the character selects a new target.");

            ac_smite_good_ability.SetName(ac_smite_good_feature.Name);
            ac_smite_good_ability.SetDescription(ac_smite_good_feature.Description);
            ac_smite_good_ability.RemoveComponent(ac_smite_good_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.CasterCheckers.AbilityCasterAlignment>());
            var context_rank_config = ac_smite_good_ability.GetComponent<Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig>();
            Helpers.SetField(context_rank_config, "m_Class", animal_companion_array);
            ac_smite_good_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityResourceLogic>().RequiredResource = smite_good_resource;
            var smite_good_actions = ac_smite_good_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>().Actions;
            var condition = (Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional)smite_good_actions.Actions[0];
            ((Kingmaker.UnitLogic.Mechanics.Conditions.ContextConditionAlignment)condition.ConditionsChecker.Conditions[0]).Alignment = AlignmentComponent.Good;


            ac_smite_evil_ability = library.CopyAndAdd<BlueprintAbility>("7bb9eb2042e67bf489ccd1374423cdec", "SmiteEvilACAbility", "40c15480d05e4e7e8242237caeecf909");
            ac_smite_evil_feature = library.CopyAndAdd<BlueprintFeature>("3a6db57fce75b0244a6a5819528ddf26", "SmiteEvilACFeature", "fbf3dd7b8043491194142634e0344258");
            ac_smite_evil_feature.SetDescription("A character can call out to the powers of good to aid her in her struggle against evil. As a swift action, the character chooses one target within sight to smite. If this target is evil, the character adds her Charisma bonus (if any) to her attack rolls and adds her character level to all damage rolls made against the target of her smite, smite evil attacks automatically bypass any DR the creature might possess.\nIn addition, while smite evil is in effect, the character gains a deflection bonus equal to her Charisma bonus (if any) to her AC against attacks made by the target of the smite. If the character targets a creature that is not evil, the smite is wasted with no effect. The smite evil lasts until the target dies or the character selects a new target.");
            ac_smite_evil_ability.SetDescription(ac_smite_evil_feature.Description);
            ac_smite_evil_ability.RemoveComponent(ac_smite_evil_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.CasterCheckers.AbilityCasterAlignment>());
            var context_rank_config2 = ac_smite_evil_ability.GetComponent<Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig>();
            Helpers.SetField(context_rank_config2, "m_Class", animal_companion_array);
        }


        static BlueprintSpellbook createHunterSpellbook()
        {
            var ranger_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("cda0615668a6df14eb36ba19ee881af6");
            var druid_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");
            var inquisitor_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce");
            //will not work properly with mystic theurge
            var hunter_spellbook = Helpers.Create<BlueprintSpellbook>();
            hunter_spellbook.name = "HunterSpellbook";
            library.AddAsset(hunter_spellbook, "e1e06d8905884a1faaadd77a3cb5f87a");
            hunter_spellbook.Name = hunter_class.LocalizedName;
            hunter_spellbook.SpellsPerDay = inquisitor_class.Spellbook.SpellsPerDay;
            hunter_spellbook.SpellsKnown = inquisitor_class.Spellbook.SpellsKnown;
            hunter_spellbook.Spontaneous = true;
            hunter_spellbook.IsArcane = false;
            hunter_spellbook.AllSpellsKnown = false;
            hunter_spellbook.CanCopyScrolls = false;
            hunter_spellbook.CastingAttribute = StatType.Wisdom;
            hunter_spellbook.CharacterClass = hunter_class;
            hunter_spellbook.CasterLevelModifier = 0;
            hunter_spellbook.CantripsType = CantripsType.Orisions;
            //hunter knows all spells of ranger and 1-6 level spells of druid
            hunter_spellbook.SpellList = Helpers.Create<BlueprintSpellList>();
            hunter_spellbook.SpellList.name = "HunterSpellList";
            library.AddAsset(hunter_spellbook.SpellList, "b161506e0b8f4116806a243f6838ae01");
            hunter_spellbook.SpellList.SpellsByLevel = new SpellLevelList[10];
            for (int i = 0; i < hunter_spellbook.SpellList.SpellsByLevel.Length; i++)
            {
                hunter_spellbook.SpellList.SpellsByLevel[i] = new SpellLevelList(i);
               
            }
            hunter_spellbook.SpellList.SpellsByLevel[0].SpellLevel = 0;
            /* hunter_spellbook.SpellList = library.CopyAndAdd<BlueprintSpellList>("29f3c338532390546bc5347826a655c4", //ranger spelllist
                                                                              "HunterSpellList",
                                                                              "3f0cbe75afe142478facc64fef816b28");*/
            //add ranger spells      
            foreach (var spell_level_list in ranger_class.Spellbook.SpellList.SpellsByLevel)
            {
                int sp_level = spell_level_list.SpellLevel;              
                foreach (var spell in spell_level_list.Spells)
                {
                    if (!spell.IsInSpellList(hunter_spellbook.SpellList))
                    {
                        ExtensionMethods.AddToSpellList(spell, hunter_spellbook.SpellList, sp_level);
                    }
                }
            }
            //add druid spells      
            foreach (var spell_level_list in druid_class.Spellbook.SpellList.SpellsByLevel)
            {
                int sp_level = spell_level_list.SpellLevel;
                if (sp_level > 6)
                {
                    continue;
                }
                foreach (var spell in spell_level_list.Spells)
                {
                    if (!spell.IsInSpellList(hunter_spellbook.SpellList))
                    {
                        ExtensionMethods.AddToSpellList(spell, hunter_spellbook.SpellList, sp_level);
                    }
                }
            }
           
            return hunter_spellbook;
        }


        static BlueprintProgression createHunterProgression()
        {
            var hunter_progression = Helpers.CreateProgression("HunterProgression",
                                                   hunter_class.Name,
                                                   hunter_class.Description,
                                                   "110347af180c477982894a74885466a4",
                                                   hunter_class.Icon,
                                                   FeatureGroup.None);
            hunter_progression.Classes = new BlueprintCharacterClass[1] { hunter_class };

            var hunter_orisions = library.CopyAndAdd<BlueprintFeature>("f2ed91cc202bd344691eef91eb6d5d1a",
                                                                       "HunterOrisionsFeature",
                                                                       "5838ceedcf344dfe8d3e0538c67a7884");
            hunter_orisions.SetDescription("Hunters learn a number of orisions, or 0 - level spells. These spells are cast like any other spell, but they do not consume any slots and may be used again.");
            hunter_orisions.SetComponents(hunter_orisions.ComponentsArray.Select(c =>
            {
                var bind = c as BindAbilitiesToClass;
                if (bind == null) return c;
                bind = UnityEngine.Object.Instantiate(bind);
                bind.CharacterClass = hunter_class;
                bind.Stat = StatType.Wisdom;
                return bind;
            }));

            var hunter_proficiencies = library.CopyAndAdd<BlueprintFeature>("c5e479367d07d62428f2fe92f39c0341",
                                                                            "HunterProficiencies",
                                                                            "e92350a79aa84304a4d2837e4a248537");
            hunter_proficiencies.SetName("Hunter Proficiencies");
            hunter_proficiencies.SetDescription("A hunter is proficient with all simple and martial weapons and with light armor, medium armor, and shields (except tower shields)");

            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");

            hunter_tactics = library.CopyAndAdd<BlueprintFeature>("e1f437048db80164792155102375b62c",
                                                                      "HunterTactics",
                                                                      "3bd652e743f346ccbc25e298954e9805");
            hunter_tactics.SetDescription("At 3rd level, a hunter automatically grants her teamwork feats to her animal companion. The companion doesn't need to meet the prerequisites of these teamwork feats.");

            hunter_teamwork_feat = library.CopyAndAdd<BlueprintFeatureSelection>("d87e2f6a9278ac04caeb0f93eff95fcb",
                                                                              "HunterTeamworkFeat",
                                                                              "93a91b20e57845b19804de9c57e28bb3");
            hunter_teamwork_feat.SetDescription("At 3rd level, and every three levels thereafter, the hunter gains a bonus feat in addition to those gained from normal advancement. These bonus feats must be selected from those listed as teamwork feats. The inquisitor must meet the prerequisites of the selected bonus feat.");
 
            var bonus_hunter_spells = createFreeSummonNatureAllySpells();
            hunter_animal_companion = createHunterAnimalCompanion();

            hunter_woodland_stride = library.CopyAndAdd<BlueprintFeature>("11f4072ea766a5840a46e6660894527d",
                                                                         "HunterWooldlandStride",
                                                                         "07f67ae4a1614ca6b0d09df6a317630c");
            hunter_woodland_stride.SetDescription("At 5th level, you can move through any sort difficult terrain at your normal speed and without taking damage or suffering any other impairment.");

            var entries = new List<LevelEntry>();
            entries.Add(Helpers.LevelEntry(1, hunter_proficiencies, hunter_orisions, detect_magic, bonus_hunter_spells, hunter_animal_companion, animal_focus, animal_focus_ac,
                                                           library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                           library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")  // touch calculate feature
                                                           )) ;
            hunter_orisions.GetComponent<BindAbilitiesToClass>().CharacterClass = hunter_class;
            var learn_orisions = new Kingmaker.UnitLogic.FactLogic.LearnSpells();
            learn_orisions.CharacterClass = hunter_class;
            learn_orisions.Spells = hunter_orisions.GetComponent<BindAbilitiesToClass>().Abilites;
            hunter_orisions.AddComponent(learn_orisions);

            createPreciseCompanion();
            entries.Add(Helpers.LevelEntry(2, precise_companion));
            entries.Add(Helpers.LevelEntry(3, hunter_tactics, hunter_teamwork_feat));
            entries.Add(Helpers.LevelEntry(5, hunter_woodland_stride));
            entries.Add(Helpers.LevelEntry(6, hunter_teamwork_feat));
            entries.Add(Helpers.LevelEntry(8, animal_focus_additional_use, animal_focus_additional_use_ac));
            entries.Add(Helpers.LevelEntry(9, hunter_teamwork_feat));
            entries.Add(Helpers.LevelEntry(12, hunter_teamwork_feat));
            entries.Add(Helpers.LevelEntry(15, hunter_teamwork_feat));
            entries.Add(Helpers.LevelEntry(18, hunter_teamwork_feat));
            entries.Add(Helpers.LevelEntry(20, animal_focus_additional_use, animal_focus_additional_use_ac));
            hunter_progression.UIGroups = new UIGroup[4] { Helpers.CreateUIGroups(precise_companion, hunter_teamwork_feat, hunter_teamwork_feat, hunter_teamwork_feat, 
                                                                                    hunter_teamwork_feat, hunter_teamwork_feat, hunter_teamwork_feat)[0],
                                                          Helpers.CreateUIGroups(animal_focus, animal_focus_additional_use, animal_focus_additional_use)[0],
                                                          Helpers.CreateUIGroups(animal_focus_ac, animal_focus_additional_use_ac, animal_focus_additional_use_ac)[0],
                                                          Helpers.CreateUIGroups(bonus_hunter_spells, hunter_tactics, hunter_woodland_stride)[0] };
            hunter_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { hunter_animal_companion, hunter_proficiencies, hunter_orisions, detect_magic };
            hunter_progression.LevelEntries = entries.ToArray();

            return hunter_progression;
        }


        static void createPreciseCompanion()
        {
            precise_companion = library.CopyAndAdd<BlueprintFeatureSelection>("d87e2f6a9278ac04caeb0f93eff95fcb",
                                                      "HunterPreciseCompanion",
                                                      "7904ae839f71415abc930d4afb37f006");
            precise_companion.SetName("Precise Companion");
            precise_companion.SetDescription("At 2nd level, a hunter chooses either Precise Shot or Outflank as a bonus feat. She does not need to meet the prerequisites for this feat. If she chooses Outflank, she automatically grants this feat to her animal companion as well.");

            var outflank = library.TryGet<Kingmaker.Blueprints.Classes.BlueprintFeature>("8f3d1e6b4be006f4d896081f2f889665");
            var precise_companion_outflank = Helpers.CreateFeature("PreciseCompanionOutflankFeature",
                                                                   outflank.Name,
                                                                   outflank.Description,
                                                                   "789857381caf4d87bb41df1af721a078",
                                                                   outflank.Icon,
                                                                   FeatureGroup.None,
                                                                   Helpers.CreateAddFact(outflank),
                                                                   createAddFeatToAnimalCompanion(outflank));
            precise_companion_outflank.HideInCharacterSheetAndLevelUp = true;
            precise_companion_outflank.HideInUI = true;

            precise_companion.AllFeatures = new BlueprintFeature[2] {library.TryGet< Kingmaker.Blueprints.Classes.BlueprintFeature>("422dab7309e1ad343935f33a4d6e9f11"), //precise shot
                                                                           precise_companion_outflank };//outflank
            precise_companion.Features = precise_companion.AllFeatures;
            precise_companion.IgnorePrerequisites = true;
        }


        static BlueprintFeature createFreeSummonNatureAllySpells()
        {
            BlueprintAbility[] summon_nature_ally    = new BlueprintAbility[6]{library.TryGet<BlueprintAbility>("c6147854641924442a3bb736080cfeb6"),
                                                                             library.TryGet<BlueprintAbility>("298148133cdc3fd42889b99c82711986"),
                                                                             library.TryGet<BlueprintAbility>("fdcf7e57ec44f704591f11b45f4acf61"),
                                                                             library.TryGet<BlueprintAbility>("c83db50513abdf74ca103651931fac4b"),
                                                                             library.TryGet<BlueprintAbility>("8f98a22f35ca6684a983363d32e51bfe"),
                                                                             library.TryGet<BlueprintAbility>("55bbce9b3e76d4a4a8c8e0698d29002c")
                                                                            };

            BlueprintComponent[] add_summon_nature_ally = new BlueprintComponent[summon_nature_ally.Length];

            for (int i = 0; i <add_summon_nature_ally.Length; i++)
            {
                add_summon_nature_ally[i] = Helpers.CreateAddKnownSpell(summon_nature_ally[i], hunter_class, i + 1);           
            }


            var free_summon_nature_ally = Helpers.CreateFeature("HunterFreeSummonNatureAllyFeat",
                                                                "Bonus Hunter Spells",
                                                                "In addition to the spells gained by hunters as they gain levels, each hunter also automatically adds all summon nature’s ally spells to her list of spells known. These spells are added as soon as the hunter is capable of casting them.",
                                                                "6fdb0275f34a4a0184a215fed24cb1cd",
                                                                summon_nature_ally[0].Icon,
                                                                FeatureGroup.None,
                                                                add_summon_nature_ally);

            for (int i = 0; i < add_summon_nature_ally.Length; i++)
            {
                summon_nature_ally[i].AddRecommendNoFeature(free_summon_nature_ally);
            }

            return free_summon_nature_ally;
        }


        static Kingmaker.Blueprints.Classes.Selection.BlueprintFeatureSelection createHunterAnimalCompanion()
        {
            var animal_companion_progression = library.CopyAndAdd<BlueprintProgression>("924fb4b659dcb4f4f906404ba694b690",
                                                                          "HunterAnimalCompanionProgression",
                                                                          "d96334e7101f4dc5b1f5666c52bba0a6");
            animal_companion_progression.Classes = new BlueprintCharacterClass[1] { hunter_class};

            var animal_companion_selection = library.CopyAndAdd<BlueprintFeatureSelection>("2995b36659b9ad3408fd26f137ee2c67",
                                                                                            "AnimalCompanionSelectionHunter",
                                                                                            "cf9f8d9910db4beba174f4e2b7c1bb2a");
            var add_progression = new AddFeatureOnApply();
            add_progression.Feature = animal_companion_progression;
            animal_companion_selection.ComponentsArray[0] = add_progression;


            return animal_companion_selection;
        }


        static void createAnimalFocusFeat()
        {
            var wildshape_wolf = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Classes.BlueprintFeature>("19bb148cb92db224abb431642d10efeb");
            var acid_maw = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("75de4ded3e731dc4f84d978fe947dc67");
            var animal_focus_additional_use_component = new Kingmaker.UnitLogic.FactLogic.IncreaseActivatableAbilityGroupSize();
            animal_focus_additional_use_component.Group = AnimalFocusGroup;

            animal_focus_additional_use = Helpers.CreateFeature("AdditionalAnimalFocusFeature",
                                                             "Additional Animal Focus",
                                                             "The character can apply additional animal focus to herself.",
                                                             "896f036314b049bfa723b74b0e509a89",
                                                             wildshape_wolf.Icon,
                                                             FeatureGroup.None,
                                                             animal_focus_additional_use_component
                                                            );
            animal_focus_additional_use.Ranks = 1;

            animal_focus = createAnimalFocus();

            animal_focus_ac = Helpers.CreateFeature("AnimalFocusAc",
                                                        "Animal Focus (Animal Companion)",
                                                        "The character can apply animal focus to her animal companion.",
                                                        "5eea1e98d11c4acbafc1f9b4abf6cae6",
                                                        acid_maw.Icon,
                                                        FeatureGroup.None,
                                                        createAddFeatToAnimalCompanion(animal_focus)
                                                        );
            animal_focus_additional_use_ac = Helpers.CreateFeature("AdditonalAnimalFocusAc",
                                                        "Additional Animal Focus (Animal Companion)",
                                                        "The character can apply one more animal focus to her animal companion.",
                                                        "06bd293935354563be67cb5d2679a9bf",
                                                        acid_maw.Icon,
                                                        FeatureGroup.None,
                                                        createAddFeatToAnimalCompanion(animal_focus_additional_use)
                                                        );
        }


        internal static void addAnimalFocusSH()
        {
            Kingmaker.Blueprints.Classes.LevelEntry initial_entry = new Kingmaker.Blueprints.Classes.LevelEntry();
            Kingmaker.Blueprints.Classes.LevelEntry add_animal_focus_use_entry = new Kingmaker.Blueprints.Classes.LevelEntry();
           
            initial_entry.Level = 4;
            initial_entry.Features.Add(animal_focus);
            initial_entry.Features.Add(animal_focus_ac);
            add_animal_focus_use_entry.Level = 17;
            add_animal_focus_use_entry.Features.Add(animal_focus_additional_use);
            add_animal_focus_use_entry.Features.Add(animal_focus_additional_use_ac);

            var sacred_huntsmaster_archetype = ResourcesLibrary.TryGetBlueprint<BlueprintArchetype>("46eb929c8b6d7164188eb4d9bcd0a012");
            var additional_level_entry_features = new LevelEntry[2] { initial_entry, add_animal_focus_use_entry };
            var new_features = additional_level_entry_features.Concat(sacred_huntsmaster_archetype.AddFeatures).ToArray();
            Array.Sort(new_features, (x, y) => x.Level.CompareTo(y.Level));
            sacred_huntsmaster_archetype.AddFeatures = new_features;

            var inquisitor_progression = ResourcesLibrary.TryGetBlueprint<BlueprintProgression>("4e945c2fe5e252f4ea61eee7fb560017");
            Kingmaker.Blueprints.Classes.UIGroup animal_focus_ui_group = new Kingmaker.Blueprints.Classes.UIGroup();
            animal_focus_ui_group.Features.Add(animal_focus);
            animal_focus_ui_group.Features.Add(animal_focus_additional_use);
            inquisitor_progression.UIGroups = inquisitor_progression.UIGroups.AddToArray(animal_focus_ui_group);

            Kingmaker.Blueprints.Classes.UIGroup animal_focus_ac_ui_group = new Kingmaker.Blueprints.Classes.UIGroup();
            animal_focus_ac_ui_group.Features.Add(animal_focus_ac);
            animal_focus_ac_ui_group.Features.Add(animal_focus_additional_use_ac);
            inquisitor_progression.UIGroups = inquisitor_progression.UIGroups.AddToArray(animal_focus_ac_ui_group);

            //remove racial enemies on sacred huntsmaster
            var racial_enemy_feat = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("16cc2c937ea8d714193017780e7d4fc6");
            var racial_enemy_rankup_feat = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("c1be13839472aad46b152cf10cf46179");
            foreach (var lvl_entry in sacred_huntsmaster_archetype.AddFeatures)
            {
                lvl_entry.Features.Remove(racial_enemy_feat);
                lvl_entry.Features.Remove(racial_enemy_rankup_feat);
            }
        }


        static Kingmaker.Designers.Mechanics.Facts.AddFeatureToCompanion createAddFeatToAnimalCompanion(BlueprintFeature feat)
        {
            var add_feat_ac = new Kingmaker.Designers.Mechanics.Facts.AddFeatureToCompanion();
            add_feat_ac.Feature = feat;
            return add_feat_ac;
        }


        static BlueprintFeature createAnimalFocus()
        {
            var inquistor_class = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Classes.BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce");
            var animal_class = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Classes.BlueprintCharacterClass>("4cd1757a0eea7694ba5c933729a53920");
            var sacred_huntsmaster_archetype = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Classes.BlueprintArchetype>("46eb929c8b6d7164188eb4d9bcd0a012");
            var bull_strength = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("4c3d08935262b6544ae97599b3a9556d");
            var bear_endurance = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("a900628aea19aa74aad0ece0e65d091a");
            var cat_grace = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("de7a025d48ad5da4991e7d3c682cf69d");
            var aspect_falcon = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("7bdb6a9fb6b37614e96f155748ae50c6");
            var owl_wisdom = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("f0455c9295b53904f9e02fc571dd2ce1");
            var heroism = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("5ab0d42fb68c9e34abae4921822b9d63"); //for monkey
            var feather_step = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("f3c0b267dd17a2a45a40805e31fe3cd1"); //for frog
            var longstrider  = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("14c90900b690cac429b229efdf416127"); //for stag
            var summon_monster1 = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("8fd74eddd9b6c224693d9ab241f25e84"); //for mouse
            (int, int)[] progressionStat = new (int, int)[3]{(7,2), (14, 4), (20, 6)};
            (int, int)[] progressionSkill = new (int, int)[3]{(7,4), (14, 6), (20, 8)};
            (int, int)[] progressionSpeed = new (int, int)[3]{(7,5), (14, 10), (20, 20)};
            BlueprintCharacterClass[] allowed_classes = new BlueprintCharacterClass[3]{inquistor_class, hunter_class, animal_class};

            var mouse_focus = createMouseFocus(summon_monster1.Icon, allowed_classes, sacred_huntsmaster_archetype, 12);
            BlueprintComponent animal_foci = KingmakerRebalance.Helpers.CreateAddFacts(createScaledFocus("BullFocus",
                                                                                            "Animal Focus: Bull",
                                                                                            "The character gains a +2 enhancement bonus to Strength. This bonus increases to +4 at 8th level and +6 at 15th level",
                                                                                            "1fa6cfa7421b4b60b41b2f055363ebe5",
                                                                                            "3230b40a4c314fe6be77e4b07af49a4a",
                                                                                            bull_strength.Icon,
                                                                                            Kingmaker.Enums.ModifierDescriptor.Enhancement,
                                                                                            Kingmaker.EntitySystem.Stats.StatType.Strength,
                                                                                            progressionStat,
                                                                                            allowed_classes,
                                                                                            sacred_huntsmaster_archetype),
                                                                         createScaledFocus("BearFocus",
                                                                                            "Animal Fcous: Bear",
                                                                                            "The character gains a +2 enhancement bonus to Constitution. This bonus increases to +4 at 8th level and +6 at 15th level.",
                                                                                            "dd96c3c629e94c17830a4d8f0fcdc08f",
                                                                                            "de5113284399417e94a0e5c15cca5872",
                                                                                            bear_endurance.Icon,
                                                                                            Kingmaker.Enums.ModifierDescriptor.Enhancement,
                                                                                            Kingmaker.EntitySystem.Stats.StatType.Constitution,
                                                                                            progressionStat, 
                                                                                            allowed_classes,
                                                                                            sacred_huntsmaster_archetype),
                                                                         createScaledFocus("TigerFocus",
                                                                                            "Animal Focus: Tiger",
                                                                                            "The character gains a +2 enhancement bonus to Dexterity. This bonus increases to +4 at 8th level and +6 at 15th level.",
                                                                                            "a175ea28855547c6a5ac9c4ee8bd6429",
                                                                                            "7e9910e8cd394f5398f3d6b36885c26a",
                                                                                            cat_grace.Icon,
                                                                                            Kingmaker.Enums.ModifierDescriptor.Enhancement,
                                                                                            Kingmaker.EntitySystem.Stats.StatType.Dexterity,
                                                                                            progressionStat, 
                                                                                            allowed_classes,
                                                                                            sacred_huntsmaster_archetype),
                                                                         createScaledFocus("FalconFocus",
                                                                                            "Animal Focus: Falcon",
                                                                                            "The character gains a +4 competence bonus on Perception checks. This bonus increases to +6 at 8th level and +8 at 15th level.",
                                                                                            "7fc508da39444660a47225979635d904",
                                                                                            "1a73b65baa024ce18943977284033df4",
                                                                                            aspect_falcon.Icon,
                                                                                            Kingmaker.Enums.ModifierDescriptor.Competence,
                                                                                            Kingmaker.EntitySystem.Stats.StatType.SkillPerception,
                                                                                            progressionSkill, 
                                                                                            allowed_classes,
                                                                                            sacred_huntsmaster_archetype),
                                                                         createScaledFocus("MonkeyFocus",
                                                                                            "Animal Focus: Monkey",
                                                                                            "The character gains a +4 competence bonus on Athletics checks. This bonus increases to +6 at 8th level and +8 at 15th level.",
                                                                                            "54ab00a5b0e54b0dbd727c352bf61a19",
                                                                                            "0da42655cad24cb2bce785b13bb93e09",
                                                                                            heroism.Icon,
                                                                                            Kingmaker.Enums.ModifierDescriptor.Competence,
                                                                                            Kingmaker.EntitySystem.Stats.StatType.SkillAthletics,
                                                                                            progressionSkill, 
                                                                                            allowed_classes,
                                                                                            sacred_huntsmaster_archetype),
                                                                        createScaledFocus("OwlFocus",
                                                                                            "Animal Focus: Owl",
                                                                                            "The character gains a +4 competence bonus on Stealth checks. This bonus increases to +6 at 8th level and +8 at 15th level.",
                                                                                            "4fad7e82e03c4ece81aa94e246f3f1c1",
                                                                                            "c02ae54193b044539db3236e4ef99139",
                                                                                            owl_wisdom.Icon,
                                                                                            Kingmaker.Enums.ModifierDescriptor.Competence,
                                                                                            Kingmaker.EntitySystem.Stats.StatType.SkillStealth,
                                                                                            progressionSkill, 
                                                                                            allowed_classes,
                                                                                            sacred_huntsmaster_archetype),
                                                                        createScaledFocus("FrogFocus",
                                                                                            "Animal Focus: Frog",
                                                                                            "The character gains a +4 competence bonus on Mobility checks. This bonus increases to +6 at 8th level and +8 at 15th level.",
                                                                                            "a5e28c9f687849988f6c8d8586b9ed3f",
                                                                                            "2248cfbd1c1349d68095431244428843",
                                                                                            feather_step.Icon,
                                                                                            Kingmaker.Enums.ModifierDescriptor.Competence,
                                                                                            Kingmaker.EntitySystem.Stats.StatType.SkillMobility,
                                                                                            progressionSkill, 
                                                                                            allowed_classes,
                                                                                            sacred_huntsmaster_archetype),
                                                                       createScaledFocus("StagFocus",
                                                                                            "Animal Focus: Stag",
                                                                                            "The character gains a 5-foot enhancement bonus to its base land speed. This bonus increases to 10 feet at 8th level and 20 feet at 15th level.",
                                                                                            "14c371bccb2240c18565d34fd210ff83",
                                                                                            "9e7bcb3eb48c4b67993365a599156077",
                                                                                            longstrider.Icon,
                                                                                            Kingmaker.Enums.ModifierDescriptor.Enhancement,
                                                                                            Kingmaker.EntitySystem.Stats.StatType.Speed,
                                                                                            progressionSpeed, 
                                                                                            allowed_classes,
                                                                                            sacred_huntsmaster_archetype)
                                                                         );

            var wildshape_wolf = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Classes.BlueprintFeature>("19bb148cb92db224abb431642d10efeb");
            var feat = KingmakerRebalance.Helpers.CreateFeature("AnimalFocusFeature",
                                                            "Animal Focus",
                                                            "Character can take the Focus of an animal as a swift action. She must select one type of animal to emulate, gaining a bonus or special ability based on the type of animal emulated and her level.",
                                                            "5a05ef60442c4fd38c418d4d190cb250",
                                                            wildshape_wolf.Icon,
                                                            FeatureGroup.None,
                                                            animal_foci, mouse_focus[0], mouse_focus[1]);
            return feat;
        }


        static Kingmaker.Designers.Mechanics.Facts.AddFeatureOnClassLevel[] createMouseFocus(UnityEngine.Sprite icon, BlueprintCharacterClass[] allowed_classes,
                                                                                                      BlueprintArchetype archetype, int update_lvl)
        {

            var mouse_focus1 = createToggleFocus("MouseFocus1",
                                            "Mouse Focus",
                                            "The creature gains evasion, as the rogue class feature. At 12th level, this increases to improved evasion, as the rogue advanced talent.",
                                            "35e970a820614e1e92d8f616f70a6785",
                                            "9a1f080c968a4bd0ada30bf35ee7aa34",
                                            icon,
                                            new Kingmaker.Designers.Mechanics.Facts.Evasion());
            var mouse_focus2 = createToggleFocus("MouseFocus2",
                                           "Mouse Focus",
                                           "The creature gains evasion, as the rogue class feature. At 12th level, this increases to improved evasion, as the rogue advanced talent.",
                                           "8bd28d45179441a2962a1acc3ed679d2",
                                           "b7977b0f52094ce4be1aaf113200ca1f",
                                           icon,
                                           new Kingmaker.Designers.Mechanics.Facts.Evasion(), new Kingmaker.Designers.Mechanics.Facts.ImprovedEvasion());

            var mouse_focus1f = Helpers.CreateFeature("MouseFocus1Feature",
                                                       "",
                                                       "708f7602863f4dda88d6dff8d9579d42",
                                                       "0ed62ee6b78e411e87c931fb6939fede",
                                                       icon,
                                                       FeatureGroup.None,
                                                       KingmakerRebalance.Helpers.CreateAddFact(mouse_focus1));
            mouse_focus1f.HideInUI = true;
            mouse_focus1f.HideNotAvailibleInUI = true;
            var mouse_focus2f = Helpers.CreateFeature("MouseFocus2Feature",
                                           "",
                                           "78b653737fa74c72be9c5933139376ad",
                                           "ee6fe29a2f7c4d4ebaad71c630ca4061",
                                           icon,
                                           FeatureGroup.None,
                                           KingmakerRebalance.Helpers.CreateAddFact(mouse_focus2));
            mouse_focus2f.HideInUI = true;
            mouse_focus2f.HideInCharacterSheetAndLevelUp = true;

            Kingmaker.Designers.Mechanics.Facts.AddFeatureOnClassLevel[] mouse_focus = new Kingmaker.Designers.Mechanics.Facts.AddFeatureOnClassLevel[2] {
                                                Helpers.CreateAddFeatureOnClassLevel(mouse_focus1f, update_lvl, allowed_classes, new BlueprintArchetype[1] { archetype }, true),
                                                Helpers.CreateAddFeatureOnClassLevel(mouse_focus2f, update_lvl, allowed_classes, new BlueprintArchetype[1] { archetype }, false)
                                                };

            return mouse_focus;
        }


        static Kingmaker.UnitLogic.ActivatableAbilities.BlueprintActivatableAbility createScaledFocus(string name, string display_name, string description,
                                                                                                      string buff_guid, string ability_guid,
                                                                                                      UnityEngine.Sprite icon,
                                                                                                      Kingmaker.Enums.ModifierDescriptor descriptor,
                                                                                                      Kingmaker.EntitySystem.Stats.StatType stat_type,
                                                                                                      (int, int)[] progression,
                                                                                                      BlueprintCharacterClass[] allowed_classes,
                                                                                                      BlueprintArchetype archetype)
        {
            BlueprintComponent[] components = new BlueprintComponent[2]{ KingmakerRebalance.Helpers.CreateContextRankConfig(ContextRankBaseValueType.MaxClassLevelWithArchetype, 
                                                                                                                      ContextRankProgression.Custom,
                                                                                                                      classes: allowed_classes,
                                                                                                                      archetype: archetype,
                                                                                                                      customProgression: progression),
                                                                       KingmakerRebalance.Helpers.CreateAddContextStatBonus(stat_type, 
                                                                                                                        descriptor,
                                                                                                                        Kingmaker.UnitLogic.Mechanics.ContextValueType.Rank)
                                                                     };
            var focus = createToggleFocus(name,
                                          display_name,
                                          description,
                                          buff_guid,
                                          ability_guid,
                                          icon,
                                          components);
            return focus;
        }

 

        static Kingmaker.UnitLogic.ActivatableAbilities.BlueprintActivatableAbility createToggleFocus(string name, string display_name, string description, 
                                                                                                      string buff_guid, string ability_guid,
                                                                                                      UnityEngine.Sprite icon, params BlueprintComponent[] components)
        {
            var buff = KingmakerRebalance.Helpers.CreateBuff(name + "Buff",
                                                         display_name,
                                                         description,
                                                         buff_guid,
                                                         icon,
                                                         null,
                                                         components);

            var Focus = KingmakerRebalance.Helpers.CreateActivatableAbility(name,
                                                                         display_name,
                                                                         description,
                                                                         ability_guid,
                                                                         icon,
                                                                         buff,
                                                                         Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.Immediately,
                                                                         Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift,
                                                                         null);
            Focus.Group = AnimalFocusGroup;
            Focus.WeightInGroup = 1;
            Focus.DeactivateImmediately = true;
            return Focus;
        }



    }
}
