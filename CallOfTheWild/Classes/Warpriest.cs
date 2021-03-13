using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    public class Warpriest
    {
        static LibraryScriptableObject library => Main.library;
        internal static bool test_mode = false;
        static public BlueprintCharacterClass warpriest_class;
        static public BlueprintProgression warpriest_progression;
        static public BlueprintFeatureSelection fighter_feat;
        static public BlueprintFeatureSelection weapon_focus_selection;

        static public BlueprintFeature warpriest_fighter_feat_prerequisite_replacement;
        static public BlueprintFeature warpriest_orisons;
        static public BlueprintFeature warpriest_proficiencies;

        static public BlueprintFeature warpriest_sacred_armor;
        static public BlueprintBuff sacred_armor_enhancement_buff;
        static public BlueprintFeature warpriest_sacred_armor2;
        static public BlueprintFeature warpriest_sacred_armor3;
        static public BlueprintFeature warpriest_sacred_armor4;
        static public BlueprintFeature warpriest_sacred_armor5;
        static public BlueprintAbilityResource sacred_armor_resource;

        static public BlueprintFeature warpriest_sacred_weapon_damage;
        static public BlueprintFeature warpriest_sacred_weapon_enhancement;
        static public BlueprintBuff sacred_weapon_enhancement_buff;
        static public BlueprintFeature warpriest_sacred_weapon_enhancement2;
        static public BlueprintFeature warpriest_sacred_weapon_enhancement3;
        static public BlueprintFeature warpriest_sacred_weapon_enhancement4;
        static public BlueprintFeature warpriest_sacred_weapon_enhancement5;
        static public BlueprintAbilityResource sacred_weapon_resource;

        static public BlueprintFeatureSelection warpriest_deity_selection;
        static public BlueprintFeature warpriest_spontaneous_heal;
        static public BlueprintFeature warpriest_spontaneous_harm;
        static public BlueprintFeatureSelection warpriest_energy_selection;
        static public BlueprintAbilityResource warpriest_fervor_resource;
        static public BlueprintAbilityResource warpriest_extra_channel_resource;
        static public BlueprintFeature extra_channel;
        static public BlueprintFeature warpriest_fervor;
        static public BlueprintFeature fervor_positive;
        static public BlueprintFeature fervor_negative;
        static public BlueprintFeature fervor_fcb;
        static public BlueprintFeature channel_positive_energy;
        static public BlueprintFeature channel_negative_energy;
        static public BlueprintFeature warpriest_channel_energy;
        static public ActivatableAbilityGroup sacred_weapon_enchancement_group = ActivatableAbilityGroupExtension.SacredWeaponEnchantment.ToActivatableAbilityGroup();//ActivatableAbilityGroup.TrueMagus;
        static public ActivatableAbilityGroup sacred_armor_enchancement_group = ActivatableAbilityGroupExtension.SacredArmorEnchantment.ToActivatableAbilityGroup();//ActivatableAbilityGroup.ArcaneWeaponProperty;

        static public BlueprintBuff warpriest_aspect_of_war_buff;
        static public BlueprintFeature warpriest_aspect_of_war;
        static public BlueprintAbilityResource warpriest_aspect_of_war_resource;
        static public BlueprintFeatureSelection warpriest_blessings;
        static public BlueprintAbilityResource warpriest_blessing_resource;
        static public BlueprintFeature add_warpriest_blessing_resource;

        static public ActivatableAbilityGroup confusion_control_group = ActivatableAbilityGroup.DivineWeaponProperty;
        static public BlueprintBuff warpriest_blessing_special_sancturay_buff;
        static BlueprintAbility heal_living;
        static BlueprintAbility harm_undead;
        static BlueprintAbility heal_living_extra;
        static BlueprintAbility harm_undead_extra;
        static BlueprintFeature remove_armor_speed_penalty_feature;


        static public BlueprintArchetype sacred_fist_archetype;
        static public BlueprintFeature sacred_fist_ki_pool;
        static public BlueprintFeatureSelection sacred_fist_syle_feat_selection;
        static public BlueprintFeature sacred_fist_proficiencies;
        static public BlueprintFeature sacred_fist_fake_monk_level;
        static public BlueprintFeature blessed_fortitude;
        static public BlueprintFeature miraculous_fortitude;
        static public BlueprintFeature flurry2_unlock, flurry15_unlock;
        static public BlueprintFeatureSelection sacred_fist_no_monk_check;

        static public BlueprintArchetype cult_leader_archetype;
        static public BlueprintArchetype champion_of_the_faith_archetype;
        static public Dictionary<string, BlueprintFeature> blessings_map = new Dictionary<string, BlueprintFeature>();
        static public BlueprintFeatureSelection quicken_blessing;
        static public BlueprintActivatableAbility quicken_blesing_ability;
        static BlueprintFeature quicken_blessing_feature;
        static Dictionary<string, BlueprintBuff> processed_quicken_ability_guid_buff_map = new Dictionary<string, BlueprintBuff>();
        static Dictionary<string, BlueprintFeature> quicken_blessing_selections = new Dictionary<string, BlueprintFeature>();

        static public BlueprintArchetype feral_champion;
        static public BlueprintFeature sacred_claws;
        static public BlueprintFeatureSelection feral_blessing;
        static public BlueprintFeature[] wild_shape;
        static public BlueprintAbilityResource wildshape_resource;
        static public BlueprintFeature extra_wildshape;

        static public BlueprintArchetype arsenal_chaplain;
        static public BlueprintProgression arsenal_chaplain_war_blessing;
        static public BlueprintFeature arsenal_chaplain_weapon_training;
        static public BlueprintFeature[] arsenal_chaplain_war_blessing_updates = new BlueprintFeature[3];
        static public BlueprintFeature aid_another_boost;

        static public BlueprintArchetype divine_commander;
        static public BlueprintFeatureSelection animal_companion;
        static public BlueprintFeatureSelection battle_tactician;
        static public BlueprintFeature blessed_companion;
        static public BlueprintFeatureSelection greater_battle_tactician;
        static public BlueprintFeatureSelection master_battle_tactician;

        internal static void createWarpriestClass()
        {
            Main.logger.Log("Warpriest class test mode: " + test_mode.ToString());
            var cleric_class = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var paladin_class = library.Get<BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec");
            warpriest_class = Helpers.Create<BlueprintCharacterClass>();
            warpriest_class.name = "WarpriestClass";
            library.AddAsset(warpriest_class, "");


            warpriest_class.LocalizedName = Helpers.CreateString("Warpriest.Name", "Warpriest");
            warpriest_class.LocalizedDescription = Helpers.CreateString("Warpriest.Description",
                                                                        "Capable of calling upon the power of the gods in the form of blessings and spells, warpriests blend divine magic with martial skill.They are unflinching bastions of their faith, shouting gospel as they pummel foes into submission, and never shy away from a challenge to their beliefs.While clerics might be subtle and use diplomacy to accomplish their aims, warpriests aren’t above using violence whenever the situation warrants it. In many faiths, warpriests form the core of the church’s martial forces—reclaiming lost relics, rescuing captured clergy, and defending the church’s tenets from all challenges.\n"
                                                                        + "Role: Warpriests can serve as capable healers or spellcasters, calling upon their divine powers from the center of the fight, where their armor and martial skills are put to the test.\n"
                                                                        + "Alignment: A warpriest’s alignment must be within one step of his deity’s, along either the law/ chaos axis or the good/ evil axis."
                                                                        );

            warpriest_class.m_Icon = cleric_class.Icon;
            warpriest_class.SkillPoints = cleric_class.SkillPoints;
            warpriest_class.HitDie = DiceType.D8;
            warpriest_class.BaseAttackBonus = cleric_class.BaseAttackBonus;
            warpriest_class.FortitudeSave = cleric_class.FortitudeSave;
            warpriest_class.ReflexSave = cleric_class.ReflexSave;
            warpriest_class.WillSave = cleric_class.WillSave;
            warpriest_class.Spellbook = createWarpriestSpellbook();
            warpriest_class.ClassSkills = new StatType[] { StatType.SkillKnowledgeArcana, StatType.SkillPersuasion, StatType.SkillLoreReligion, StatType.SkillAthletics, StatType.SkillLoreNature };
            warpriest_class.IsDivineCaster = true;
            warpriest_class.IsArcaneCaster = false;
            warpriest_class.StartingGold = paladin_class.StartingGold;
            warpriest_class.PrimaryColor = paladin_class.PrimaryColor;
            warpriest_class.SecondaryColor = paladin_class.SecondaryColor;
            warpriest_class.RecommendedAttributes = cleric_class.RecommendedAttributes;
            warpriest_class.NotRecommendedAttributes = cleric_class.NotRecommendedAttributes;
            warpriest_class.EquipmentEntities = paladin_class.EquipmentEntities;
            warpriest_class.MaleEquipmentEntities = paladin_class.MaleEquipmentEntities;
            warpriest_class.FemaleEquipmentEntities = paladin_class.FemaleEquipmentEntities;
            warpriest_class.ComponentsArray = cleric_class.ComponentsArray;
            warpriest_class.StartingItems = paladin_class.StartingItems;

            createWarpriestProgression();
            warpriest_class.Progression = warpriest_progression;
            createSacredFist();
            createChampionOfTheFaith();
            createCultLeader();
            createFeralChampion();
            createArsenalChaplain();
            createDivineCommander();
            warpriest_class.Archetypes = new BlueprintArchetype[] { sacred_fist_archetype, cult_leader_archetype, champion_of_the_faith_archetype, feral_champion, arsenal_chaplain, divine_commander }; 
            Helpers.RegisterClass(warpriest_class);

            Common.addMTDivineSpellbookProgression(warpriest_class, warpriest_class.Spellbook, "MysticTheurgeWarpriest",
                                                     Common.createPrerequisiteClassSpellLevel(warpriest_class, 2));
        }

        static BlueprintCharacterClass[] getWarpriestArray()
        {
            return new BlueprintCharacterClass[] { warpriest_class };
        }


        static BlueprintCharacterClass[] getBlessingUsersArray()
        {
            return new BlueprintCharacterClass[] { warpriest_class, Archetypes.DivineTracker.archetype.GetParentClass() };
        }


        static BlueprintArchetype[] getBlessingUsersArchetypesArray()
        {
            return new BlueprintArchetype[] { Archetypes.DivineTracker.archetype };
        }


        static BlueprintSpellbook createWarpriestSpellbook()
        {
            var cleric_class = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var magus_class = library.Get<BlueprintCharacterClass>("45a4607686d96a1498891b3286121780");
            var warpriest_spellbook = Helpers.Create<BlueprintSpellbook>();
            warpriest_spellbook.Name = warpriest_class.LocalizedName;
            warpriest_spellbook.name = "WarpriestSpellbook";
            library.AddAsset(warpriest_spellbook, "");
            warpriest_spellbook.Name = warpriest_class.LocalizedName;
            warpriest_spellbook.SpellsPerDay = magus_class.Spellbook.SpellsPerDay;
            warpriest_spellbook.SpellsKnown = magus_class.Spellbook.SpellsKnown;
            warpriest_spellbook.Spontaneous = false;
            warpriest_spellbook.IsArcane = false;
            warpriest_spellbook.AllSpellsKnown = true;
            warpriest_spellbook.CanCopyScrolls = false;
            warpriest_spellbook.CastingAttribute = StatType.Wisdom;
            warpriest_spellbook.CharacterClass = warpriest_class;
            warpriest_spellbook.CasterLevelModifier = 0;
            warpriest_spellbook.CantripsType = CantripsType.Orisions;
            warpriest_spellbook.SpellsPerLevel = cleric_class.Spellbook.SpellsPerLevel;
            warpriest_spellbook.SpellList = Common.createSpellList("WarpriestSpellList", "", cleric_class.Spellbook.SpellList, 6);
            return warpriest_spellbook;
        }


        static void createWarpriestProgression()
        {
            createWarpriestProficiencies();
            createWarpriestFighterFeatPrerequisiteReplacement();
            createWarpriestOrisions();
            createDeitySelection();
            createSacredWeaponDamage();
            createSacredWeaponEnhancement();
            createSpontaneousConversion();
            createFervor();
            createChannelEnergy();
            createSacredArmor();
            createAspectOfWar();
            createWarpriestBlessings();
            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");
            fighter_feat = library.CopyAndAdd<BlueprintFeatureSelection>("41c8486641f7d6d4283ca9dae4147a9f", "WarpriestBonusFeat", "");
            fighter_feat.SetDescription("At 3rd level and every 3 levels thereafter, a warpriest gains a bonus feat in addition to those gained from normal advancement. These bonus feats must be selected from those listed as combat feats.");

            var weapon_focus = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");
            weapon_focus_selection = Helpers.CreateFeatureSelection("WarpriestWeaponFocusSelection",
                                                                        "Focus Weapon",
                                                                        "At 1st level, a warpriest receives Weapon Focus as a bonus feat (she can choose any weapon, not just his deity’s favored weapon).",
                                                                        "",
                                                                        weapon_focus.Icon,
                                                                        FeatureGroup.None);
            weapon_focus_selection.IgnorePrerequisites = true;
            weapon_focus_selection.AllFeatures = new BlueprintFeature[] { weapon_focus };

            warpriest_progression = Helpers.CreateProgression("WarpriestProgression",
                                                               warpriest_class.Name,
                                                               warpriest_class.Description,
                                                               "",
                                                               warpriest_class.Icon,
                                                               FeatureGroup.None);
            warpriest_progression.Classes = getWarpriestArray();

            warpriest_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, warpriest_proficiencies, detect_magic, warpriest_orisons,
                                                                                        warpriest_fighter_feat_prerequisite_replacement,
                                                                                        warpriest_deity_selection,
                                                                                        warpriest_energy_selection,
                                                                                        weapon_focus_selection,
                                                                                        warpriest_sacred_weapon_damage,
                                                                                        add_warpriest_blessing_resource,
                                                                                        warpriest_blessings,
                                                                                        warpriest_blessings,
                                                                                        library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                        library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")),  // touch calculate feature};
                                                                    Helpers.LevelEntry(2, warpriest_fervor),
                                                                    Helpers.LevelEntry(3, fighter_feat),
                                                                    Helpers.LevelEntry(4, warpriest_channel_energy, warpriest_sacred_weapon_enhancement),
                                                                    Helpers.LevelEntry(5),
                                                                    Helpers.LevelEntry(6, fighter_feat),
                                                                    Helpers.LevelEntry(7, warpriest_sacred_armor),
                                                                    Helpers.LevelEntry(8, warpriest_sacred_weapon_enhancement2),
                                                                    Helpers.LevelEntry(9, fighter_feat),
                                                                    Helpers.LevelEntry(10, warpriest_sacred_armor2),
                                                                    Helpers.LevelEntry(11),
                                                                    Helpers.LevelEntry(12, fighter_feat, warpriest_sacred_weapon_enhancement3),
                                                                    Helpers.LevelEntry(13, warpriest_sacred_armor3),
                                                                    Helpers.LevelEntry(14),
                                                                    Helpers.LevelEntry(15, fighter_feat),
                                                                    Helpers.LevelEntry(16, warpriest_sacred_weapon_enhancement4, warpriest_sacred_armor4),
                                                                    Helpers.LevelEntry(17),
                                                                    Helpers.LevelEntry(18, fighter_feat),
                                                                    Helpers.LevelEntry(19, warpriest_sacred_armor5),
                                                                    Helpers.LevelEntry(20, warpriest_sacred_weapon_enhancement5, warpriest_aspect_of_war)
                                                                    };

            warpriest_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] {warpriest_proficiencies, detect_magic, warpriest_orisons,
                                                                                        warpriest_fighter_feat_prerequisite_replacement,
                                                                                        warpriest_deity_selection, warpriest_blessings, warpriest_blessings};
            warpriest_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(weapon_focus_selection, fighter_feat, fighter_feat, fighter_feat, fighter_feat, fighter_feat, fighter_feat),
                                                         Helpers.CreateUIGroup(warpriest_energy_selection, warpriest_fervor, warpriest_channel_energy, warpriest_aspect_of_war),
                                                         Helpers.CreateUIGroup(warpriest_sacred_weapon_damage, warpriest_sacred_weapon_enhancement, warpriest_sacred_weapon_enhancement2,
                                                                               warpriest_sacred_weapon_enhancement3, warpriest_sacred_weapon_enhancement4, warpriest_sacred_weapon_enhancement5),
                                                         Helpers.CreateUIGroup(warpriest_sacred_armor, warpriest_sacred_armor2, warpriest_sacred_armor3,
                                                                               warpriest_sacred_armor4, warpriest_sacred_armor5)
                                                        };
        }


        static void createWarpriestProficiencies()
        {
            warpriest_proficiencies = library.CopyAndAdd<BlueprintFeature>("b10ff88c03308b649b50c31611c2fefb", "WarpriestProficiencies", "");
            warpriest_proficiencies.SetName("Warpriest Proficiencies");
            warpriest_proficiencies.SetDescription("A warpriest is proficient with all simple and martial weapons, as well as the favored weapon of his deity, and with all armor (heavy, light, and medium) and shields (except tower shields). If the warpriest worships a deity with unarmed strike as its favored weapon, the warpriest gains Improved Unarmed Strike as a bonus feat.");
        }


        static void createWarpriestOrisions()
        {
            warpriest_orisons = library.CopyAndAdd<BlueprintFeature>(
                 "e62f392949c24eb4b8fb2bc9db4345e3", // cleric orisions
                 "WarpriestOrisonsFeature",
                 "");
            warpriest_orisons.SetDescription("Warpriests learn a number of orisons, or 0-level spells. These spells are cast like any other spell, but they do not consume any slots and may be used again.");
            warpriest_orisons.ReplaceComponent<BindAbilitiesToClass>(c => c.CharacterClass = warpriest_class);
        }


        static void createWarpriestFighterFeatPrerequisiteReplacement()
        {
            var fighter_class = library.Get<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");
            var fighter_training = library.Get<BlueprintFeature>("2b636b9e8dd7df94cbd372c52237eebf");
            warpriest_fighter_feat_prerequisite_replacement = Helpers.CreateFeature("WarpriestFighterFeatPrerequisiteReplacement",
                                                                                    "Fighter Training",
                                                                                    "The warpriest treats his warpriest level as his base attack bonus (in addition to base attack bonuses gained from other classes and Hit Dice) for the purpose of qualifying for feats.\n"
                                                                                    + "He can furthermore select feats that have a minimum number of fighter levels as a prerequisite, treating his warpriest level as his fighter level.",
                                                                                    "",
                                                                                    fighter_training.Icon,
                                                                                    FeatureGroup.None,
                                                                                    Common.createClassLevelsForPrerequisites(fighter_class, warpriest_class),
                                                                                    Common.createReplace34BabWithClassLevel(warpriest_class)
                                                                                    );
        }



        static void createDeitySelection()
        {
            warpriest_deity_selection = library.Get<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4");

            foreach (var f in warpriest_deity_selection.AllFeatures)
            {
                var forbid_spellbook = f.GetComponent<ForbidSpellbookOnAlignmentDeviation>();
                if (forbid_spellbook != null)
                {
                    forbid_spellbook.Spellbooks = forbid_spellbook.Spellbooks.AddToArray(warpriest_class.Spellbook);
                }

                var add_equipment = f.GetComponent<AddStartingEquipment>();
                if (add_equipment != null)
                {
                    add_equipment.RestrictedByClass = add_equipment.RestrictedByClass.AddToArray(warpriest_class);
                }

                var add_proficiencies = f.GetComponents<AddFeatureOnClassLevel>();
                foreach (var a in add_proficiencies)
                {
                     a.AdditionalClasses = a.AdditionalClasses.AddToArray(warpriest_class);
                }
            }
        }


        static void createSpontaneousConversion()
        {
            var channel_positive_allowed = library.Get<BlueprintFeature>("8c769102f3996684fb6e09a2c4e7e5b9");
            var channel_negative_allowed = library.Get<BlueprintFeature>("dab5255d809f77c4395afc2b713e9cd6");

            warpriest_spontaneous_heal = library.CopyAndAdd<BlueprintFeature>("5e4620cea099c9345a9207c11d7bc916", "WarpriestSpontaneousHealFeature", "");
            warpriest_spontaneous_heal.RemoveComponents<Prerequisite>();
            warpriest_spontaneous_heal.SetDescription("A good warpriest (or a neutral warpriest of a good deity) can channel stored spell energy into healing spells that he did not prepare ahead of time. The warpriest can expend any prepared spell that isn’t an orison to cast any cure spell of the same spell level (a cure spell is any spell with \"cure\" in its name).");
            warpriest_spontaneous_heal.ReplaceComponent<SpontaneousSpellConversion>(c => c.CharacterClass = warpriest_class);
            warpriest_spontaneous_heal.AddComponent(Helpers.PrerequisiteFeature(channel_positive_allowed));

            warpriest_spontaneous_harm = library.CopyAndAdd<BlueprintFeature>("5ba6b9cc18acafd45b6293d1e03221ac", "WarpriestSpontaneousHarmFeature", "");
            warpriest_spontaneous_harm.SetDescription("An evil warpriest (or a neutral warpriest of an evil deity) can channel stored spell energy into wounding spells that she did not prepare ahead of time. The warpriest can \"lose\" any prepared spell that is not an orison in order to cast any inflict spell of the same spell level (an inlict spell is any spell with \"inflict\" in its name).");
            warpriest_spontaneous_harm.ReplaceComponent<SpontaneousSpellConversion>(c => c.CharacterClass = warpriest_class);
            warpriest_spontaneous_harm.AddComponent(Helpers.PrerequisiteFeature(channel_negative_allowed));

            warpriest_energy_selection = Helpers.CreateFeatureSelection("WarpriestEnergySelection",
                                                                      "Spontaneous Spell Conversion",
                                                                      "A good warpriest (or a neutral warpriest of a good deity) can channel stored spell energy into healing spells that he did not prepare ahead of time. The warpriest can expend any prepared spell that isn’t an orison to cast any cure spell of the same spell level. A cure spell is any spell with \"cure\" in its name.\n"
                                                                      + "An evil warpriest (or a neutral warpriest of an evil deity) can’t convert spells to cure spells, but can convert them to inflict spells.An inflict spell is any spell with \"inflict\" in its name.\n"
                                                                      + "A warpriest that is neither good nor evil and whose deity is neither good nor evil chooses whether he can convert spells into either cure spells or inflict spells.Once this choice is made, it cannot be changed. This choice also determines whether the warpriest channels positive or negative energy.",
                                                                      "",
                                                                      null,
                                                                      FeatureGroup.None);
            warpriest_energy_selection.AllFeatures = new BlueprintFeature[] { warpriest_spontaneous_heal, warpriest_spontaneous_harm };
        }


        static void createFervor()
        {
            warpriest_fervor_resource = Helpers.CreateAbilityResource("WarpriestFervorResource", "", "", "", null);
            warpriest_fervor_resource.SetIncreasedByLevelStartPlusDivStep(0, 2, 1, 2, 1, 0, 0.0f, getWarpriestArray());
            warpriest_fervor_resource.SetIncreasedByStat(0, StatType.Wisdom);

            warpriest_extra_channel_resource = Helpers.CreateAbilityResource("WarpriestExtraChannelResource", "", "", "", null);
            warpriest_extra_channel_resource.SetFixedResource(0);


            var dispel_magic = library.Get<BlueprintAbility>("92681f181b507b34ea87018e8f7a528a");

            var cast_only_on_self = Common.createContextActionApplyBuff(SharedSpells.can_only_target_self_buff, Helpers.CreateContextDuration(), is_child: true, dispellable: false, is_permanent: true);
            var fervor_swift_cast_buff = Helpers.CreateBuff("WarpriestFervorSwiftCastBuff",
                                                            "Fervor (Quicken Personal Spell)",
                                                            "As a swift action, a warpriest can expend one use of this ability to cast any one single target warpriest spell he has prepared with a casting time of 1 round or shorter. When cast in this way, the spell can target only the warpriest, even if it could normally affect other targets. Spells cast in this way ignore somatic components and do not provoke attacks of opportunity. The warpriest does not need to have a free hand to cast a spell in this way.",
                                                            "",
                                                            dispel_magic.Icon,
                                                            null,
                                                            Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicOnPersonalSpell>(m =>
                                                                                                                   {
                                                                                                                       m.amount = 1;
                                                                                                                       m.resource = warpriest_fervor_resource;
                                                                                                                       m.Metamagic = Metamagic.Quicken;
                                                                                                                       m.spellbook = warpriest_class.Spellbook;
                                                                                                                   }),
                                                            Helpers.CreateAddFactContextActions(cast_only_on_self)
                                                            );
            var fervor_swift_cast_ability = Helpers.CreateActivatableAbility("WarpriestFervorSwiftCastActivatableAbility",
                                                                             fervor_swift_cast_buff.Name,
                                                                             fervor_swift_cast_buff.Description,
                                                                             "",
                                                                             fervor_swift_cast_buff.Icon,
                                                                             fervor_swift_cast_buff,
                                                                             AbilityActivationType.Immediately,
                                                                             CommandType.Free,
                                                                             null,
                                                                             Helpers.CreateActivatableResourceLogic(warpriest_fervor_resource, ResourceSpendType.Never)
                                                                             );
            fervor_swift_cast_ability.DeactivateImmediately = true;

            var cure_light_wounds = library.Get<BlueprintAbility>("47808d23c67033d4bbab86a1070fd62f");
            var inflict_light_wounds = library.Get<BlueprintAbility>("244a214d3b0188e4eb43d3a72108b67b");
            var construct_type = library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f");
            var undead_type = library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");

            fervor_fcb = Helpers.CreateFeature("WarpriestFervorFavoredClassBonusFeature",
                                               "Fervor Heal/Damage Bonus",
                                               "Add 1/2 point to the amount of damage dealt or healed by the warpriest’s fervor ability.",
                                               "4a68bcfeca4140c8827621a8f219c5a8",
                                               cure_light_wounds.Icon,
                                               FeatureGroup.None);
            fervor_fcb.Ranks = 10;


            var dice = Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.DamageDice), Helpers.CreateContextValue(AbilityRankType.DamageBonus));
            var heal_action = Common.createContextActionHealTarget(dice);
            var damage_undead_action = Helpers.CreateActionDealDamage(DamageEnergyType.PositiveEnergy, dice);
            var damage_living_action = Helpers.CreateActionDealDamage(DamageEnergyType.NegativeEnergy, dice);

            var context_rank_config = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.StartPlusDivStep,
                                                                      type: AbilityRankType.DamageDice, classes: getWarpriestArray(), startLevel: 2, stepLevel: 3);
            var context_rank_config_fcb = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureRank, feature: fervor_fcb, type: AbilityRankType.DamageBonus);

            var fervor_positive_ability_others = Helpers.CreateAbility("WarpriestFervorPositiveOthersTouchAbility",
                                                                        "Fervor (Positive Energy) Others",
                                                                        $"At 2nd level, a warpriest can draw upon the power of his faith to heal wounds or harm foes. This ability can be used a number of times per day equal to 1/2 his warpriest level + his Wisdom modifier. By expending one use of this ability, a good warpriest (or one who worships a good deity) can touch a creature to heal it of 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage, plus an additional 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage for every 3 warpriest levels he possesses above 2nd (to a maximum of 7d{BalanceFixes.getDamageDieString(DiceType.D6)} at 20th level). Using this ability is a standard action (unless the warpriest targets himself, in which case it’s a swift action). Alternatively, the warpriest can use this ability to harm an undead creature, dealing the same amount of damage he would otherwise heal with a melee touch attack. Using fervor in this way is a standard action that provokes an attack of opportunity. Undead do not receive a saving throw against this damage. This counts as positive energy.",
                                                                        "",
                                                                        cure_light_wounds.Icon,
                                                                        AbilityType.Supernatural,
                                                                        CommandType.Standard,
                                                                        AbilityRange.Touch,
                                                                        "",
                                                                        Helpers.savingThrowNone,
                                                                        Helpers.CreateRunActions(Helpers.CreateConditional(Helpers.Create<UndeadMechanics.ContextConditionHasNegativeEnergyAffinity>(),
                                                                                                                           damage_undead_action,
                                                                                                                           heal_action)),
                                                                        cure_light_wounds.GetComponent<SpellDescriptorComponent>(),
                                                                        cure_light_wounds.GetComponent<AbilityDeliverTouch>(),
                                                                        cure_light_wounds.GetComponent<AbilitySpawnFx>(),
                                                                        context_rank_config,
                                                                        context_rank_config_fcb
                                                                        );
            fervor_positive_ability_others.CanTargetFriends = true;
            fervor_positive_ability_others.CanTargetEnemies = true;
            fervor_positive_ability_others.CanTargetSelf = false;
            fervor_positive_ability_others.CanTargetPoint = false;
            fervor_positive_ability_others.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            fervor_positive_ability_others.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            fervor_positive_ability_others.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Touch;
            fervor_positive_ability_others.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionOmni;

            var fervor_positive_ability_others_sticky = Helpers.CreateTouchSpellCast(fervor_positive_ability_others, "WarpriestFervorPositiveOthersAbility", "");
            fervor_positive_ability_others_sticky.AddComponents(Common.createAbilityTargetHasFact(true, construct_type),
                                                                Helpers.CreateResourceLogic(warpriest_fervor_resource),
                                                                Helpers.Create<AbilityUseOnRest>(c => c.Type = AbilityUseOnRestType.HealDamage));

            var fervor_positive_ability_self = library.CopyAndAdd<BlueprintAbility>(fervor_positive_ability_others, "WarpriestFervorPositiveSelfAbility", "");
            fervor_positive_ability_self.SetName("Fervor (Positive Energy) Self");
            fervor_positive_ability_self.Range = AbilityRange.Personal;
            fervor_positive_ability_self.CanTargetSelf = true;
            fervor_positive_ability_self.CanTargetFriends = false;
            fervor_positive_ability_self.CanTargetEnemies = false;
            fervor_positive_ability_self.ActionType = CommandType.Swift;
            fervor_positive_ability_self.AddComponent(Helpers.CreateResourceLogic(warpriest_fervor_resource));


            fervor_positive = Helpers.CreateFeature("WarpriestFervorPositive",
                                                    "Fervor (Positive Energy)",
                                                    fervor_positive_ability_others.Description + "\n" + fervor_swift_cast_ability.Description,
                                                    "",
                                                    fervor_positive_ability_others.Icon,
                                                    FeatureGroup.None,
                                                    Helpers.CreateAddFacts(fervor_positive_ability_self, fervor_positive_ability_others_sticky, fervor_swift_cast_ability)
                                                    );

            var deaths_embrace_living = library.Get<BlueprintFeature>("fd7c08ccd3c7773458eb9613db3e93ad");
            var fervor_negative_ability_others = Helpers.CreateAbility("WarpriestFervorNegativeTouchOthersAbility",
                                                            "Fervor (Negative Energy) Others",
                                                            $"At 2nd level, a warpriest can draw upon the power of his faith to heal wounds or harm foes. This ability can be used a number of times per day equal to 1/2 his warpriest level + his Wisdom modifier. By expending one use of this ability, an evil warpriest (or one who worships an evil deity) can touch a living creature and deal to it 1d{BalanceFixes.getDamageDieString(DiceType.D6)}  points of damage, plus an additional 1d{BalanceFixes.getDamageDieString(DiceType.D6)}  points of damage for every 3 warpriest levels he possesses above 2nd (to a maximum of 7d{BalanceFixes.getDamageDieString(DiceType.D6)}  at 20th level). Using this ability is a standard action (unless the warpriest targets himself, in which case it’s a swift action). Alternatively, the warpriest can use this ability to heal an undead creature for same amount of damage he would otherwise deal with a melee touch attack. Using fervor in this way is a standard action that provokes an attack of opportunity. Living creatures do not receive a saving throw against this damage. This counts as negative energy.",
                                                            "",
                                                            inflict_light_wounds.Icon,
                                                            AbilityType.Supernatural,
                                                            CommandType.Standard,
                                                            AbilityRange.Touch,
                                                            "",
                                                            Helpers.savingThrowNone,
                                                            Helpers.CreateRunActions(Helpers.CreateConditional(Helpers.CreateConditionsCheckerOr(Helpers.Create<UndeadMechanics.ContextConditionHasNegativeEnergyAffinity>(),
                                                                                                                                                 Common.createContextConditionHasFact(deaths_embrace_living)
                                                                                                                                                ),
                                                                                                               heal_action,
                                                                                                               damage_living_action)),
                                                            inflict_light_wounds.GetComponent<AbilityDeliverTouch>(),
                                                            inflict_light_wounds.GetComponent<AbilitySpawnFx>(),
                                                            context_rank_config,
                                                            context_rank_config_fcb
                                                            );
            fervor_negative_ability_others.CanTargetFriends = true;
            fervor_negative_ability_others.CanTargetEnemies = true;
            fervor_negative_ability_others.CanTargetSelf = false;
            fervor_negative_ability_others.CanTargetPoint = false;
            fervor_negative_ability_others.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            fervor_negative_ability_others.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            fervor_negative_ability_others.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Touch;
            fervor_negative_ability_others.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionOmni;

            var fervor_negative_ability_others_sticky = Helpers.CreateTouchSpellCast(fervor_negative_ability_others, "WarpriestFervorNegativeOthersAbility", "");
            fervor_negative_ability_others_sticky.AddComponents(Common.createAbilityTargetHasFact(true, construct_type),
                                                                Helpers.CreateResourceLogic(warpriest_fervor_resource),
                                                                Helpers.Create<AbilityUseOnRest>(c => c.Type = AbilityUseOnRestType.HealDamage));

            var fervor_negative_ability_self = library.CopyAndAdd<BlueprintAbility>(fervor_negative_ability_others, "WarpriestFervorNegativeSelfAbility", "");
            fervor_negative_ability_self.SetName("Fervor (Negative Energy) Self");
            fervor_negative_ability_self.Range = AbilityRange.Personal;
            fervor_negative_ability_self.CanTargetSelf = true;
            fervor_negative_ability_self.CanTargetFriends = false;
            fervor_negative_ability_self.CanTargetEnemies = false;
            fervor_negative_ability_self.ActionType = CommandType.Swift;
            fervor_negative_ability_self.AddComponent(Helpers.CreateResourceLogic(warpriest_fervor_resource));

            fervor_negative = Helpers.CreateFeature("WarpriestFervorNegative",
                                                    "Fervor (Negative Energy)",
                                                    fervor_negative_ability_others.Description + "\n" + fervor_swift_cast_ability.Description,
                                                    "",
                                                    fervor_negative_ability_others.Icon,
                                                    FeatureGroup.None,
                                                    Helpers.CreateAddFacts(fervor_negative_ability_self, fervor_negative_ability_others_sticky, fervor_swift_cast_ability)
                                                    );

            warpriest_fervor = Helpers.CreateFeature("WarpriestFervorFeature",
                                                     "Fervor",
                                                     $"At 2nd level, a warpriest can draw upon the power of his faith to heal wounds or harm foes. This ability can be used a number of times per day equal to 1/2 his warpriest level + his Wisdom modifier. By expending one use of this ability, a good warpriest (or one who worships a good deity) can touch a creature to heal it of 1d{BalanceFixes.getDamageDieString(DiceType.D6)}  points of damage, plus an additional 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage for every 3 warpriest levels he possesses above 2nd (to a maximum of 7d{BalanceFixes.getDamageDieString(DiceType.D6)}  at 20th level). Using this ability is a standard action (unless the warpriest targets himself, in which case it’s a swift action). Alternatively, the warpriest can use this ability to harm an undead creature, dealing the same amount of damage he would otherwise heal with a melee touch attack. Using fervor in this way is a standard action that provokes an attack of opportunity. Undead do not receive a saving throw against this damage. This counts as positive energy.\n"
                                                     + "An evil warpriest(or one who worships an evil deity) can use this ability to instead deal damage to living creatures with a melee touch attack and heal undead creatures with a touch.This counts as negative energy.\n"
                                                     + "A neutral warpriest who worships a neutral deity(or one who is not devoted to a particular deity) uses this ability as a good warpriest if he chose to spontaneously cast cure spells or as an evil warpriest if he chose to spontaneously cast inflict spells.\n"
                                                     + "As a swift action, a warpriest can expend one use of this ability to cast any one single target warpriest spell he has prepared with a casting time of 1 round or shorter. When cast in this way, the spell can target only the warpriest, even if it could normally affect other targets. Spells cast in this way ignore somatic components and do not provoke attacks of opportunity. The warpriest does not need to have a free hand to cast a spell in this way.",
                                                     "",
                                                     null,
                                                     FeatureGroup.None,
                                                     Common.createAddFeatureIfHasFact(warpriest_spontaneous_heal, fervor_positive),
                                                     Common.createAddFeatureIfHasFact(warpriest_spontaneous_harm, fervor_negative),
                                                     Helpers.CreateAddAbilityResource(warpriest_fervor_resource),
                                                     Helpers.CreateAddAbilityResource(warpriest_extra_channel_resource)
                                                     );
        }



        static void createChannelEnergy()
        {
            var positive_energy_feature = library.Get<BlueprintFeature>("a79013ff4bcd4864cb669622a29ddafb");
            var negative_energy_feature = library.Get<BlueprintFeature>("3adb2c906e031ee41a01bfc1d5fb7eea");
            var context_rank_config = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.StartPlusDivStep,
                                                                                  type: AbilityRankType.Default, classes: getWarpriestArray(), startLevel: 2, stepLevel: 3);
            var dc_scaling = Common.createContextCalculateAbilityParamsBasedOnClasses(getWarpriestArray(), StatType.Wisdom);
            channel_positive_energy = Helpers.CreateFeature("WarpriestChannelPositiveEnergyFeature",
                                                                "Channel Positive Energy",
                                                                "A good warpriest (or a neutral warpriest who worships a good deity) channels positive energy and can choose to deal damage to undead creatures or to heal living creatures.\n"
                                                                + "Channeling energy causes a burst that either heals all living creatures or damages all undead creatures in a 30-foot radius centered on the warpriest. The amount of damage dealt or healed is equal to that of fervor ability. Creatures that take damage from channeled energy receive a Will save to halve the damage. The DC of this save is equal to 10 + 1/2 the warpriest's level + the warpriest's Wisdom modifier. Creatures healed by channel energy cannot exceed their maximum hit point total—all excess healing is lost. Channeling positive energy consumes two uses of fervor ability. This is a standard action that does not provoke an attack of opportunity.",
                                                                "",
                                                                positive_energy_feature.Icon,
                                                                FeatureGroup.None);

            heal_living = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHeal,
                                                                      "WarpriestChannelEnergyHealLiving",
                                                                      "",
                                                                      "Channeling positive energy causes a burst that heals all living creatures in a 30 - foot radius centered on the warpriest. The amount of damage healed is equal to that of fervor ability.",
                                                                      "",
                                                                      context_rank_config,
                                                                      dc_scaling,
                                                                      Helpers.CreateResourceLogic(warpriest_fervor_resource, true, 2));
            harm_undead = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHarm,
                                                          "WarpriestChannelEnergyHarmUndead",
                                                          "",
                                                          "Channeling energy causes a burst that damages all undead creatures in a 30 - foot radius centered on the warpriest. The amount of damage dealt is equal to that of fervor ability. Creatures that take damage from channeled energy receive a Will save to halve the damage. The DC of this save is equal to 10 + 1/2 the warpriest's level + the warpriest's Wisdom modifier.",
                                                          "",
                                                          context_rank_config,
                                                          dc_scaling,
                                                          Helpers.CreateResourceLogic(warpriest_fervor_resource, true, 2));
            heal_living_extra = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHeal,
                                                                      "WarpriestChannelEnergyHealLivingExtra",
                                                                      heal_living.Name + " (Extra)",
                                                                      heal_living.Description,
                                                                      "",
                                                                      context_rank_config,
                                                                      dc_scaling,
                                                                      Helpers.CreateResourceLogic(warpriest_extra_channel_resource, true, 1));

            harm_undead_extra = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHarm,
                                                          "WarpriestChannelEnergyHarmUndeadExtra",
                                                          harm_undead.Name + " (Extra)",
                                                          harm_undead.Description,
                                                          "",
                                                          context_rank_config,
                                                          dc_scaling,
                                                          Helpers.CreateResourceLogic(warpriest_extra_channel_resource, true, 1));

            var heal_living_base = Common.createVariantWrapper("WarpriestPositiveHealBase", "", heal_living, heal_living_extra);
            var harm_undead_base = Common.createVariantWrapper("WarpriestPositiveHarmBase", "", harm_undead, harm_undead_extra);

            ChannelEnergyEngine.storeChannel(heal_living, channel_positive_energy, ChannelEnergyEngine.ChannelType.PositiveHeal);
            ChannelEnergyEngine.storeChannel(heal_living_extra, channel_positive_energy, ChannelEnergyEngine.ChannelType.PositiveHeal);
            ChannelEnergyEngine.storeChannel(harm_undead, channel_positive_energy, ChannelEnergyEngine.ChannelType.PositiveHarm);
            ChannelEnergyEngine.storeChannel(harm_undead_extra, channel_positive_energy, ChannelEnergyEngine.ChannelType.PositiveHarm);

            channel_positive_energy.AddComponent(Helpers.CreateAddFacts(heal_living_base, harm_undead_base));

            channel_negative_energy = Helpers.CreateFeature("WarpriestChannelNegativeEnergyFeature",
                                                    "Channel Negative Energy",
                                                    "An evil warpriest (or a neutral warpriest who worships an evil deity) channels negative energy and can choose to deal damage to living creatures or to heal undead creatures.\n"
                                                    + "Channeling energy causes a burst that either heals all undead creatures or damages all living creatures in a 30-foot radius centered on the warpriest. The amount of damage dealt or healed is equal to that of fervor ability. Creatures that take damage from channeled energy receive a Will save to halve the damage. The DC of this save is equal to 10 + 1/2 the warpriest's level + the warpriest's Wisdom modifier. Creatures healed by channel energy cannot exceed their maximum hit point total—all excess healing is lost. Channeling negative energy consumes two uses of fervor ability. This is a standard action that does not provoke an attack of opportunity.",
                                                    "",
                                                    negative_energy_feature.Icon,
                                                    FeatureGroup.None);

            var harm_living = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.NegativeHarm,
                                                          "WarpriestChannelEnergyHarmLiving",
                                                          "",
                                                          "Channeling energy causes a burst that damages all living creatures in a 30 - foot radius centered on the warpriest. The amount of damage dealt is equal to that of fervor ability. Creatures that take damage from channeled energy receive a Will save to halve the damage. The DC of this save is equal to 10 + 1/2 the warpriest's level + the warpriest's Wisdom modifier. Channeling negative energy consumes two uses of fervor ability. This is a standard action that does not provoke an attack of opportunity.",
                                                          "",
                                                          context_rank_config,
                                                          dc_scaling,
                                                          Helpers.CreateResourceLogic(warpriest_fervor_resource, true, 2));

            var heal_undead = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.NegativeHeal,
                                              "WarpriestChannelEnergyHealUndead",
                                              "",
                                              "Channeling positive energy causes a burst that heals all undead creatures in a 30 - foot radius centered on the warpriest. The amount of damage healed is equal to that of fervor ability.",
                                              "",
                                              context_rank_config,
                                              dc_scaling,
                                              Helpers.CreateResourceLogic(warpriest_fervor_resource, true, 2));


            var harm_living_extra = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.NegativeHarm,
                                                          "WarpriestChannelEnergyHarmLivingExtra",
                                                          harm_living.Name + " (Extra)",
                                                          harm_living.Description,
                                                          "",
                                                          context_rank_config,
                                                          dc_scaling,
                                                          Helpers.CreateResourceLogic(warpriest_extra_channel_resource, true, 1));

            var heal_undead_extra = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.NegativeHeal,
                                                          "WarpriestChannelEnergyHealUndeadExtra",
                                                          heal_undead.Name + " (Extra)",
                                                          heal_undead.Description,
                                                          "",
                                                          context_rank_config,
                                                          dc_scaling,
                                                          Helpers.CreateResourceLogic(warpriest_extra_channel_resource, true, 1));

            var harm_living_base = Common.createVariantWrapper("WarpriestNegativeHarmBase", "", harm_living, harm_living_extra);
            var heal_undead_base = Common.createVariantWrapper("WarpriestNegativeHealBase", "", heal_undead, heal_undead_extra);

            ChannelEnergyEngine.storeChannel(harm_living, channel_negative_energy, ChannelEnergyEngine.ChannelType.NegativeHarm);
            ChannelEnergyEngine.storeChannel(harm_living_extra, channel_negative_energy, ChannelEnergyEngine.ChannelType.NegativeHarm);
            ChannelEnergyEngine.storeChannel(heal_undead, channel_negative_energy, ChannelEnergyEngine.ChannelType.NegativeHeal);
            ChannelEnergyEngine.storeChannel(heal_undead_extra, channel_negative_energy, ChannelEnergyEngine.ChannelType.NegativeHeal);

            channel_negative_energy.AddComponent(Helpers.CreateAddFacts(harm_living_base, heal_undead_base));
            warpriest_channel_energy = Helpers.CreateFeature("WarpriestChannelEnergyFeature",
                                                             "Channel energy",
                                                             "Starting at 4th level, a warpriest can release a wave of energy by channeling the power of his faith through his holy (or unholy) symbol. This energy can be used to deal or heal damage, depending on the type of energy channeled and the creatures targeted. Using this ability is a standard action that expends two uses of his fervor ability and doesn’t provoke an attack of opportunity. The warpriest must present a holy (or unholy) symbol to use this ability. A good warpriest (or one who worships a good deity) channels positive energy and can choose to heal living creatures or to deal damage to undead creatures. An evil warpriest (or one who worships an evil deity) channels negative energy and can choose to deal damage to living creatures or heal undead creatures. A neutral warpriest who worships a neutral deity (or one who is not devoted to a particular deity) channels positive energy if he chose to spontaneously cast cure spells or negative energy if he chose to spontaneously cast inflict spells.\n"
                                                             + "Channeling energy causes a burst that affects all creatures of one type (either undead or living) in a 30 - foot radius centered on the warpriest. The amount of damage dealt or healed is equal to the amount listed in the fervor ability. Creatures that take damage from channeled energy must succeed at a Will saving throw to halve the damage. The save DC is 10 + 1/2 the warpriest’s level + the warpriest’s Wisdom modifier. Creatures healed by channeled energy cannot exceed their maximum hit point total—all excess healing is lost.A warpriest can choose whether or not to include himself in this effect.",
                                                             "",
                                                             null,
                                                             FeatureGroup.None,
                                                             Common.createAddFeatureIfHasFact(warpriest_spontaneous_heal, channel_positive_energy),
                                                             Common.createAddFeatureIfHasFact(warpriest_spontaneous_harm, channel_negative_energy)
                                                             );
            extra_channel = ChannelEnergyEngine.createExtraChannelFeat(heal_living_extra, warpriest_channel_energy, "ExtraChannelWarpriest", "Extra Channel (Warpriest)", "");
        }


        static void createSacredWeaponDamage()
        {
            var bless_weapon = library.Get<BlueprintAbility>("831e942864e924846a30d2e0678e438b");
            var weapon_focus = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");
            DiceFormula[] diceFormulas = new DiceFormula[] {new DiceFormula(1, DiceType.D6),
                                                            new DiceFormula(1, DiceType.D8),
                                                            new DiceFormula(1, DiceType.D10),
                                                            new DiceFormula(2, DiceType.D6),
                                                            new DiceFormula(2, DiceType.D8)};
            warpriest_sacred_weapon_damage = Helpers.CreateFeature("WarpriestSacredWeaponDamage",
                                                                   "Sacred Weapon",
                                                                   "At 1st level, weapons wielded by a warpriest are charged with the power of his faith. In addition to the favored weapon of his deity, the warpriest can designate a weapon as a sacred weapon by selecting that weapon with the Weapon Focus feat; if he has multiple Weapon Focus feats, this ability applies to all of them. Whenever the warpriest hits with his sacred weapon, the weapon damage is based on his level and not the weapon type. If the weapon’s base damage exceeds the sacred weapon damage, its damage is unchanged. This increase in damage does not affect any other aspect of the weapon, and doesn’t apply to alchemical items, bombs, or other weapons that only deal energy damage.\n"
                                                                   + "The damage dealt by medium warpriest with her sacred weapon is 1d6 at levels 1-4, 1d8 at levels 5-9, 1d10 at levels 10 - 14, 2d6 at levels 15-19 and finally 2d8 at level 20.",
                                                                   "",
                                                                   bless_weapon.Icon,
                                                                   FeatureGroup.None,
                                                                   Common.createContextWeaponDamageDiceReplacement(new BlueprintParametrizedFeature[] { weapon_focus, NewFeats.deity_favored_weapon },
                                                                                                                   Helpers.CreateContextValue(AbilityRankType.Default),
                                                                                                                   diceFormulas),
                                                                   Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                                   type: AbilityRankType.Default,
                                                                                                   progression: ContextRankProgression.DivStep,
                                                                                                   stepLevel: 5,
                                                                                                   classes: getWarpriestArray())
                                                                  );
        }


        static void createSacredWeaponEnhancement()
        {
            var weapon_focus = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");
            var divine_weapon = library.Get<BlueprintAbility>("7ff088ab58c69854b82ea95c2b0e35b4");
            var enchants = WeaponEnchantments.temporary_enchants;

            var enhancement_buff = Helpers.CreateBuff("WarpriestSacredWeaponEnchancementBaseBuff",
                                         "",
                                         "",
                                         "",
                                         null,
                                         null,
                                         Common.createBuffRemainingGroupsSizeEnchantPrimaryHandWeapon(sacred_weapon_enchancement_group,
                                                                                                      false, true,
                                                                                                      enchants
                                                                                                      )
                                         );
            sacred_weapon_enhancement_buff = Helpers.CreateBuff("WarpriestSacredWeaponEnchancementSwitchBuff",
                                                                 "Sacred Weapon Enhancement",
                                                                 "At 4th level, the warpriest gains the ability to enhance one of his sacred weapons with divine power as a swift action. This power grants the weapon a +1 enhancement bonus. For every 4 levels beyond 4th, this bonus increases by 1 (to a maximum of +5 at 20th level). The warpriest can use this ability a number of rounds per day equal to his warpriest level, but these rounds need not be consecutive.\n"
                                                                 + "These bonuses stack with any existing bonuses the weapon might have, to a maximum of + 5. The warpriest can enhance a weapon with any of the following weapon special abilities: brilliant energy, disruption, flaming, frost, keen, and shock. In addition, if the warpriest is chaotic, he can add anarchic. If he is evil, he can add unholy. If he is good, he can add holy. If he is lawful, he can add axiomatic. If he is neutral, he can add ghost touch. Adding any of these special abilities replaces an amount of bonus equal to the special ability’s base cost. Duplicate abilities do not stack.",
                                                                 "",
                                                                 divine_weapon.Icon,
                                                                 null,
                                                                 Helpers.CreateAddFactContextActions(activated: Common.createContextActionApplyBuff(enhancement_buff, Helpers.CreateContextDuration(),
                                                                                                                is_child: true, is_permanent: true, dispellable: false)
                                                                                                     )
                                                                 );
            sacred_weapon_enhancement_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var brilliant_energy = createSacredEnhancementFeature("WarpriestSacredWeaponEnchancementBrilliantEnergy",
                                                                        "Sacred Weapon - Brilliant Energy",
                                                                        "A warpriest can add the brilliant energy property to her sacred weapon, but this consumes 4 points of enhancement bonus granted to this weapon.\nA brilliant energy weapon ignores nonliving matter.Armor and shield bonuses to AC(including any enhancement bonuses to that armor) do not count against it because the weapon passes through armor. (Dexterity, deflection, dodge, natural armor, and other such bonuses still apply.) A brilliant energy weapon cannot harm undead, constructs, or objects.",
                                                                        library.Get<BlueprintActivatableAbility>("f1eec5cc68099384cbfc6964049b24fa").Icon,
                                                                        sacred_weapon_enhancement_buff,
                                                                        library.Get<BlueprintWeaponEnchantment>("66e9e299c9002ea4bb65b6f300e43770"),
                                                                        4, sacred_weapon_enchancement_group);

            var flaming = createSacredEnhancementFeature("WarpriestSacredWeaponEnchancementFlaming",
                                                                "Sacred Weapon - Flaming",
                                                                "A warpriest can add the flaming property to her sacred weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA flaming weapon is sheathed in fire that deals an extra 1d6 points of fire damage on a successful hit. The fire does not harm the wielder.",
                                                                library.Get<BlueprintActivatableAbility>("7902941ef70a0dc44bcfc174d6193386").Icon,
                                                                sacred_weapon_enhancement_buff,
                                                                library.Get<BlueprintWeaponEnchantment>("30f90becaaac51f41bf56641966c4121"),
                                                                1, sacred_weapon_enchancement_group);

            var frost = createSacredEnhancementFeature("WarpriestSacredWeaponEnchancementFrost",
                                                            "Sacred Weapon - Frost",
                                                            "A warpriest can add the frost property to her sacred weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA frost weapon is sheathed in a terrible, icy cold that deals an extra 1d6 points of cold damage on a successful hit. The cold does not harm the wielder.",
                                                            library.Get<BlueprintActivatableAbility>("b338e43a8f81a2f43a73a4ae676353a5").Icon,
                                                            sacred_weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("421e54078b7719d40915ce0672511d0b"),
                                                            1, sacred_weapon_enchancement_group);

            var shock = createSacredEnhancementFeature("WarpriestSacredWeaponEnchancementShock",
                                                            "Sacred Weapon - Shock",
                                                            "A warpriest can add the shock property to her sacred weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA shock weapon is sheathed in crackling electricity that deals an extra 1d6 points of electricity damage on a successful hit. The electricity does not harm the wielder.",
                                                            library.Get<BlueprintActivatableAbility>("a3a9e9a2f909cd74e9aee7788a7ec0c6").Icon,
                                                            sacred_weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("7bda5277d36ad114f9f9fd21d0dab658"),
                                                            1, sacred_weapon_enchancement_group);

            var ghost_touch = createSacredEnhancementFeature("WarpriestSacredWeaponEnchancementGhostTouch",
                                                                    "Sacred Weapon - Ghost Touch",
                                                                    "A warpriest can add the ghost touch property to her sacred weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA ghost touch weapon deals damage normally against incorporeal creatures, regardless of its bonus. An incorporeal creature's 50% reduction in damage from corporeal sources does not apply to attacks made against it with ghost touch weapons.",
                                                                    library.Get<BlueprintActivatableAbility>("688d42200cbb2334c8e27191c123d18f").Icon,
                                                                    sacred_weapon_enhancement_buff,
                                                                    library.Get<BlueprintWeaponEnchantment>("47857e1a5a3ec1a46adf6491b1423b4f"),
                                                                    1, sacred_weapon_enchancement_group,
                                                                    AlignmentMaskType.TrueNeutral);

            var keen = createSacredEnhancementFeature("WarpriestSacredWeaponEnchancementKeen",
                                                            "Sacred Weapon - Keen",
                                                            "A warpriest can add the keen property to her sacred weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nThe keen property doubles the threat range of a weapon. This benefit doesn't stack with any other effects that expand the threat range of a weapon (such as the keen edge spell or the Improved Critical feat).",
                                                            library.Get<BlueprintActivatableAbility>("27d76f1afda08a64d897cc81201b5218").Icon,
                                                            sacred_weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("102a9c8c9b7a75e4fb5844e79deaf4c0"),
                                                            1, sacred_weapon_enchancement_group);

            var disruption = createSacredEnhancementFeature("WarpriestSacredWeaponEnchancementDisruption",
                                                                    "Sacred Weapon - Disruption",
                                                                    "A warpriest can add the disruption property to her sacred weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nA disruption weapon is the bane of all undead. Any undead creature struck in combat must succeed on a DC 14 Will save or be destroyed. A disruption weapon must be a bludgeoning melee weapon.",
                                                                    library.Get<BlueprintActivatableAbility>("27d76f1afda08a64d897cc81201b5218").Icon,
                                                                    sacred_weapon_enhancement_buff,
                                                                    library.Get<BlueprintWeaponEnchantment>("0f20d79b7049c0f4ca54ca3d1ea44baa"),
                                                                    2, sacred_weapon_enchancement_group);

            var holy = createSacredEnhancementFeature("WarpriestSacredWeaponEnchancementHoly",
                                                            "Sacred Weapon - Holy",
                                                            "A warpriest can add the holy property to her sacred weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nA holy weapon is imbued with holy power. This power makes the weapon good-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of evil alignment.",
                                                            library.Get<BlueprintActivatableAbility>("ce0ece459ebed9941bb096f559f36fa8").Icon,
                                                            sacred_weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("28a9964d81fedae44bae3ca45710c140"),
                                                            2, sacred_weapon_enchancement_group,
                                                            AlignmentMaskType.Good);

            var unholy = createSacredEnhancementFeature("WarpriestSacredWeaponEnchancementUnholy",
                                                            "Sacred Weapon - Unholy",
                                                            "A warpriest can add the unholy property to her sacred weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn unholy weapon is imbued with unholy power. This power makes the weapon evil-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of good alignment.",
                                                            library.Get<BlueprintActivatableAbility>("561803a819460f34ea1fe079edabecce").Icon,
                                                            sacred_weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("d05753b8df780fc4bb55b318f06af453"),
                                                            2, sacred_weapon_enchancement_group,
                                                            AlignmentMaskType.Evil);

            var axiomatic = createSacredEnhancementFeature("WarpriestSacredWeaponEnchancementAxiomatic",
                                                            "Sacred Weapon - Axiomatic",
                                                            "A warpriest can add the axiomatic property to her sacred weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn axiomatic weapon is infused with lawful power. It makes the weapon lawful-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against chaotic creatures.",
                                                            library.Get<BlueprintActivatableAbility>("d76e8a80ab14ac942b6a9b8aaa5860b1").Icon,
                                                            sacred_weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("0ca43051edefcad4b9b2240aa36dc8d4"),
                                                            2, sacred_weapon_enchancement_group,
                                                            AlignmentMaskType.Lawful);

            var anarchic = createSacredEnhancementFeature("WarpriestSacredWeaponEnchancementAnarchic",
                                                "Sacred Weapon - Anarchic",
                                                "A warpriest can add the anarchic property to her sacred weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn anarchic weapon is infused with the power of chaos. It makes the weapon chaotic-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of lawful alignment.",
                                                library.Get<BlueprintActivatableAbility>("8ed07b0cc56223c46953348f849f3309").Icon,
                                                sacred_weapon_enhancement_buff,
                                                library.Get<BlueprintWeaponEnchantment>("57315bc1e1f62a741be0efde688087e9"),
                                                2, sacred_weapon_enchancement_group,
                                                AlignmentMaskType.Chaotic);

            sacred_weapon_resource = Helpers.CreateAbilityResource("WarpriestSacredWeaponResource", "", "", "", null);
            sacred_weapon_resource.SetIncreasedByLevel(0, 1, getWarpriestArray());

            var restriction = Helpers.Create<NewMechanics.ActivatableAbilityMainWeaponHasParametrizedFeatureRestriction>(c => c.features = new BlueprintParametrizedFeature[] { weapon_focus, NewFeats.deity_favored_weapon });
            var sacred_weapon_ability = Helpers.CreateActivatableAbility("WarpriestSacredWeaponEnchantmentToggleAbility",
                                                                         sacred_weapon_enhancement_buff.Name,
                                                                         sacred_weapon_enhancement_buff.Description,
                                                                         "",
                                                                         sacred_weapon_enhancement_buff.Icon,
                                                                         sacred_weapon_enhancement_buff,
                                                                         AbilityActivationType.WithUnitCommand,
                                                                         CommandType.Swift,
                                                                         null,
                                                                         Helpers.CreateActivatableResourceLogic(sacred_weapon_resource, ResourceSpendType.NewRound),
                                                                         restriction
                                                                         );

            if (!test_mode)
            {
                sacred_weapon_ability.DeactivateIfCombatEnded = true;
                //sacred_weapon_ability.AddComponent(Common.createActivatableAbilityUnitCommand(CommandType.Swift));
            }

            warpriest_sacred_weapon_enhancement = Helpers.CreateFeature("WarpriestSacredWeaponEnchancementFeature",
                                                                        "Sacred Weapon +1",
                                                                        sacred_weapon_ability.Description,
                                                                        "",
                                                                        sacred_weapon_ability.Icon,
                                                                        FeatureGroup.None,
                                                                        Helpers.CreateAddAbilityResource(sacred_weapon_resource),
                                                                        Helpers.CreateAddFacts(sacred_weapon_ability, flaming, frost, shock, ghost_touch, keen)
                                                                        );

            warpriest_sacred_weapon_enhancement2 = Helpers.CreateFeature("WarpriestSacredWeaponEnchancement2Feature",
                                                            "Sacred Weapon +2",
                                                            sacred_weapon_ability.Description,
                                                            "",
                                                            sacred_weapon_ability.Icon,
                                                            FeatureGroup.None,
                                                            Common.createIncreaseActivatableAbilityGroupSize(sacred_weapon_enchancement_group),
                                                            Helpers.CreateAddFacts(disruption, holy, unholy, axiomatic, anarchic)
                                                            );

            warpriest_sacred_weapon_enhancement3 = Helpers.CreateFeature("WarpriestSacredWeaponEnchancement3Feature",
                                                                            "Sacred Weapon +3",
                                                                            sacred_weapon_ability.Description,
                                                                            "",
                                                                            sacred_weapon_ability.Icon,
                                                                            FeatureGroup.None,
                                                                            Common.createIncreaseActivatableAbilityGroupSize(sacred_weapon_enchancement_group)
                                                                            );

            warpriest_sacred_weapon_enhancement4 = Helpers.CreateFeature("WarpriestSacredWeaponEnchancement4Feature",
                                                                            "Sacred Weapon +4",
                                                                            sacred_weapon_ability.Description,
                                                                            "",
                                                                            sacred_weapon_ability.Icon,
                                                                            FeatureGroup.None,
                                                                            Common.createIncreaseActivatableAbilityGroupSize(sacred_weapon_enchancement_group),
                                                                            Helpers.CreateAddFact(brilliant_energy)
                                                                            );

            warpriest_sacred_weapon_enhancement5 = Helpers.CreateFeature("WarpriestSacredWeaponEnchancement5Feature",
                                                                            "Sacred Weapon +5",
                                                                            sacred_weapon_ability.Description,
                                                                            "",
                                                                            sacred_weapon_ability.Icon,
                                                                            FeatureGroup.None,
                                                                            Common.createIncreaseActivatableAbilityGroupSize(sacred_weapon_enchancement_group)
                                                                            );
        }


        static BlueprintActivatableAbility createSacredEnhancementFeature(string name_prefix, string display_name, string description, UnityEngine.Sprite icon, BlueprintBuff sacred_buff,
                                                                           BlueprintItemEnchantment enchantment, int group_size, ActivatableAbilityGroup group,
                                                                           AlignmentMaskType alignment = AlignmentMaskType.Any)
        {
            return Common.createEnchantmentAbility(name_prefix, display_name, description, icon, sacred_buff, enchantment, group_size, group, alignment);
        }


        static void createSacredArmor()
        {
            var mage_armor = library.Get<BlueprintAbility>("9e1ad5d6f87d19e4d8883d63a6e35568");
            var enchants = ArmorEnchantments.temporary_armor_enchantments;
            var enhancement_buff = Helpers.CreateBuff("WarpriestSacredArmorEnchancementBaseBuff",
                                         "",
                                         "",
                                         "",
                                         null,
                                         null,
                                         Common.createBuffRemainingGroupSizetEnchantArmor(sacred_armor_enchancement_group,
                                                                                                      false, true,
                                                                                                      enchants
                                                                                                      )
                                         );
            enhancement_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            sacred_armor_enhancement_buff = Helpers.CreateBuff("WarpriestSacredArmorEnhancementSwitchBuff",
                                                                 "Sacred Armor",
                                                                 "At 7th level, the warpriest gains the ability to enhance his armor with divine power as a swift action. This power grants the armor a +1 enhancement bonus. For every 3 levels beyond 7th, this bonus increases by 1 (to a maximum of +5 at 19th level). The warpriest can use this ability a number of minutes per day equal to his warpriest level. This duration must be used in 1-minute increments, but they don’t need to be consecutive.\n"
                                                                 + "These bonuses stack with any existing bonuses the armor might have, to a maximum of +5. The warpriest can enhance armor any of the following armor special abilities: energy resistance (normal, improved, and greater), fortification (heavy, light, or moderate), and spell resistance (13, 17, 21, and 25). Adding any of these special abilities replaces an amount of bonus equal to the special ability’s base cost. Duplicate abilities do not stack.",
                                                                 "",
                                                                 mage_armor.Icon,
                                                                 null,
                                                                 Helpers.CreateAddFactContextActions(activated: Common.createContextActionApplyBuff(enhancement_buff, Helpers.CreateContextDuration(),
                                                                                                                is_child: true, dispellable: false, is_permanent: true)
                                                                                                     )
                                                                 );
            //energy resistance - normal (10 +2), improved (20 +4), greater (+5)  - fire, cold, shock, acid = 12 //pick existing
            //fortification - light (25% +1), medium(50% +3), heavy (75% +5)
            //spell resistance (+13 +2), (+17 +3), (+21 +4), (+25, +5)

            var sr_icon = library.Get<BlueprintAbility>("0a5ddfbcfb3989543ac7c936fc256889").Icon;
            var fortification_icon = library.Get<BlueprintAbility>("c66e86905f7606c4eaa5c774f0357b2b").Icon; //stoneskin
            Dictionary<DamageEnergyType, UnityEngine.Sprite> energy_resistance_icons = new Dictionary<DamageEnergyType, UnityEngine.Sprite>();
            energy_resistance_icons.Add(DamageEnergyType.Fire, library.Get<BlueprintAbility>("ddfb4ac970225f34dbff98a10a4a8844").Icon);
            energy_resistance_icons.Add(DamageEnergyType.Cold, library.Get<BlueprintAbility>("5368cecec375e1845ae07f48cdc09dd1").Icon);
            energy_resistance_icons.Add(DamageEnergyType.Electricity, library.Get<BlueprintAbility>("90987584f54ab7a459c56c2d2f22cee2").Icon);
            energy_resistance_icons.Add(DamageEnergyType.Acid, library.Get<BlueprintAbility>("fedc77de9b7aad54ebcc43b4daf8decd").Icon);

            List<BlueprintActivatableAbility>[] enchant_abilities = new List<BlueprintActivatableAbility>[5];

            for (int i = 0; i < enchant_abilities.Length; i++)
            {
                enchant_abilities[i] = new List<BlueprintActivatableAbility>();
            }

            foreach (var e in ArmorEnchantments.spell_resistance_enchantments)
            {
                int cost = e.EnchantmentCost;
                var ability = createSacredEnhancementFeature("WarpriestSacredArmor" + e.name, "Sacred Armor - " + e.Name, e.Description + $"\nThis consumes {cost} point(s) of armor enhancement bonus.", sr_icon, sacred_armor_enhancement_buff, e, cost, sacred_armor_enchancement_group);
                enchant_abilities[cost - 1].Add(ability);
            }


            foreach (var e in ArmorEnchantments.fortification_enchantments)
            {
                int cost = e.EnchantmentCost;
                var ability = createSacredEnhancementFeature("WarpriestSacredArmor" + e.name, "Sacred Armor - " + e.Name, e.Description + $"\nThis consumes {cost} point(s) of armor enhancement bonus.", fortification_icon, sacred_armor_enhancement_buff, e, cost, sacred_armor_enchancement_group);
                enchant_abilities[cost - 1].Add(ability);
            }

            foreach (var item in ArmorEnchantments.energy_resistance_enchantments)
            {
                foreach (var e in item.Value)
                {
                    int cost = e.EnchantmentCost;
                    var ability = createSacredEnhancementFeature("WarpriestSacredArmor" + e.name, "Sacred Armor - " + e.Name, e.Description + $"\nThis consumes {cost} point(s) of armor enhancement bonus.", energy_resistance_icons[item.Key], sacred_armor_enhancement_buff, e, cost, sacred_armor_enchancement_group);
                    enchant_abilities[cost - 1].Add(ability);
                }
            }


            sacred_armor_resource = Helpers.CreateAbilityResource("WarpriestSacredArmorResource", "", "", "", null);
            sacred_armor_resource.SetIncreasedByLevel(0, 1, getWarpriestArray());

            var apply_buff = Common.createContextActionApplyBuff(sacred_armor_enhancement_buff,
                                                                 Helpers.CreateContextDuration(Common.createSimpleContextValue(1), rate: DurationRate.Minutes),
                                                                 dispellable: false);
            var sacred_armor_ability = Helpers.CreateAbility("WarpriestSacredArmorAbility",
                                                                         sacred_armor_enhancement_buff.Name,
                                                                         sacred_armor_enhancement_buff.Description,
                                                                         "",
                                                                         sacred_armor_enhancement_buff.Icon,
                                                                         AbilityType.Supernatural,
                                                                         CommandType.Swift,
                                                                         AbilityRange.Personal,
                                                                         "1 minute",
                                                                         Helpers.savingThrowNone,
                                                                         mage_armor.GetComponent<AbilitySpawnFx>(),
                                                                         Helpers.CreateRunActions(apply_buff),
                                                                         Helpers.CreateResourceLogic(sacred_armor_resource)
                                                                         );


            warpriest_sacred_armor = Helpers.CreateFeature("WarpriestSacredArmorFeature",
                                                                        "Sacred Armor +1",
                                                                        sacred_armor_ability.Description,
                                                                        "",
                                                                        sacred_armor_ability.Icon,
                                                                        FeatureGroup.None,
                                                                        Helpers.CreateAddAbilityResource(sacred_armor_resource),
                                                                        Helpers.CreateAddFact(sacred_armor_ability),
                                                                        Helpers.CreateAddFacts(enchant_abilities[0].ToArray())
                                                                        );

            warpriest_sacred_armor2 = Helpers.CreateFeature("WarpriestSacredArmor2Feature",
                                                            "Sacred Armor +2",
                                                            sacred_armor_ability.Description,
                                                            "",
                                                            sacred_armor_ability.Icon,
                                                            FeatureGroup.None,
                                                            Common.createIncreaseActivatableAbilityGroupSize(sacred_armor_enchancement_group),
                                                            Helpers.CreateAddFacts(enchant_abilities[1].ToArray())
                                                            );

            warpriest_sacred_armor3 = Helpers.CreateFeature("WarpriestSacredArmor3Feature",
                                                 "Sacred Armor +3",
                                                 sacred_armor_ability.Description,
                                                 "",
                                                 sacred_armor_ability.Icon,
                                                 FeatureGroup.None,
                                                 Common.createIncreaseActivatableAbilityGroupSize(sacred_armor_enchancement_group),
                                                 Helpers.CreateAddFacts(enchant_abilities[2].ToArray())
                                                 );

            warpriest_sacred_armor4 = Helpers.CreateFeature("WarpriestSacredArmor4Feature",
                                                "Sacred Armor +4",
                                                sacred_armor_ability.Description,
                                                "",
                                                sacred_armor_ability.Icon,
                                                FeatureGroup.None,
                                                Common.createIncreaseActivatableAbilityGroupSize(sacred_armor_enchancement_group),
                                                Helpers.CreateAddFacts(enchant_abilities[3].ToArray())
                                                );

            warpriest_sacred_armor5 = Helpers.CreateFeature("WarpriestSacredArmor5Feature",
                                                "Sacred Armor +5",
                                                sacred_armor_ability.Description,
                                                "",
                                                sacred_armor_ability.Icon,
                                                FeatureGroup.None,
                                                Common.createIncreaseActivatableAbilityGroupSize(sacred_armor_enchancement_group),
                                                Helpers.CreateAddFacts(enchant_abilities[4].ToArray())
                                                );
        }


        static void createAspectOfWar()
        {
            var rage = library.Get<BlueprintAbility>("97b991256e43bb140b263c326f690ce2");
            warpriest_aspect_of_war_resource = Helpers.CreateAbilityResource("WarpriestAspectOfWarResource", "", "", "", null);
            warpriest_aspect_of_war_resource.SetFixedResource(1);
            remove_armor_speed_penalty_feature = Helpers.CreateFeature("RemoveArmorSpeedPenaltyFeature",
                                                                           "Remove Armor Penalty",
                                                                           "",
                                                                           "",
                                                                           null,
                                                                           FeatureGroup.None,
                                                                           Helpers.Create<ArmorSpeedPenaltyRemoval>()
                                                                           );
            remove_armor_speed_penalty_feature.Ranks = 2;
            //remove_armor_speed_penalty_feature.HideInCharacterSheetAndLevelUp = true;

            var transformation = library.Get<BlueprintAbility>("27203d62eb3d4184c9aced94f22e1806");
            var transformation_buff = library.Get<BlueprintBuff>("287682389d2011b41b5a65195d9cbc84");
            warpriest_aspect_of_war_buff = Helpers.CreateBuff("WarpriestAspectOfWarBuff",
                                          "Aspect of War",
                                          "At 20th level, the warpriest can channel an aspect of war, growing in power and martial ability. Once per day as a swift action, a warpriest can treat his level as his base attack bonus, gains DR 10/—, and can move at his full speed regardless of the armor he is wearing or his encumbrance. In addition, the blessings he calls upon don’t count against his daily limit during this time. This ability lasts for 1 minute.",
                                          "",
                                          rage.Icon,
                                          transformation_buff.FxOnStart,
                                          Common.createPhysicalDR(10),
                                          Helpers.Create<RaiseBAB>(c => c.TargetValue = Common.createSimpleContextValue(20)),
                                          Helpers.CreateAddFact(remove_armor_speed_penalty_feature),
                                          Helpers.CreateAddFact(remove_armor_speed_penalty_feature),
                                          Helpers.Create<EncumbranceMechanics.IgnoreEncumbrence>()
                                          );

            var apply_buff = Common.createContextActionApplyBuff(warpriest_aspect_of_war_buff,
                                                                 Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes),
                                                                 true, false, false, false);

            var warpriest_aspect_of_war_ability = Helpers.CreateAbility("WarpriestAspectOfWarFeature",
                                                                    warpriest_aspect_of_war_buff.Name,
                                                                    warpriest_aspect_of_war_buff.Description,
                                                                    "",
                                                                    warpriest_aspect_of_war_buff.Icon,
                                                                    AbilityType.Supernatural,
                                                                    CommandType.Swift,
                                                                    AbilityRange.Personal,
                                                                    "1 minute",
                                                                    Helpers.savingThrowNone,
                                                                    Helpers.CreateRunActions(apply_buff),
                                                                    transformation.GetComponent<AbilitySpawnFx>(),
                                                                    Helpers.CreateResourceLogic(warpriest_aspect_of_war_resource)
                                                                    );
            warpriest_aspect_of_war_ability.CanTargetSelf = true;
            warpriest_aspect_of_war_ability.Animation = transformation.Animation;
            warpriest_aspect_of_war_ability.AnimationStyle = transformation.AnimationStyle;

            warpriest_aspect_of_war = Common.AbilityToFeature(warpriest_aspect_of_war_ability, false);
            warpriest_aspect_of_war.AddComponent(Helpers.CreateAddAbilityResource(warpriest_aspect_of_war_resource));

        }


        static void createWarpriestBlessings()
        {
            warpriest_blessings = Helpers.CreateFeatureSelection("WarpriestBlessingsSelection",
                                                                 "Blessing",
                                                                 "A warpriest can select any two blessings granted by his deity. Deities grant blessings of the same name as the domains they grant.\n"
                                                                 + "Each blessing grants a minor power at 1st level and a major power at 10th level. A warpriest can call upon the power of his blessings a number of times per day (in any combination) equal to 3 + 1/2 his warpriest level (to a maximum of 13 times per day at 20th level). Each time he calls upon any one of his blessings, it counts against his daily limit.The save DC for these blessings is equal to 10 + 1/2 the warpriest’s level + the warpriest’s Wisdom modifier.",
                                                                 "",
                                                                 null,
                                                                 FeatureGroup.None);
            warpriest_blessing_resource = Helpers.CreateAbilityResource("WarpriestBlessingsResource", "", "", "", null);
            warpriest_blessing_resource.SetIncreasedByLevelStartPlusDivStep(3, 2, 1, 2, 1, 0, 0.0f, getBlessingUsersArray(), new BlueprintArchetype[] {Archetypes.DivineTracker.archetype });
            add_warpriest_blessing_resource = Helpers.CreateFeature("AddWarpriestBlessingsResource",
                                                                    "",
                                                                    "",
                                                                    "",
                                                                    null,
                                                                    FeatureGroup.None,
                                                                    Helpers.CreateAddAbilityResource(warpriest_blessing_resource)
                                                                    );
            add_warpriest_blessing_resource.HideInCharacterSheetAndLevelUp = true;
            add_warpriest_blessing_resource.HideInUI = true;

            Archetypes.DivineTracker.archetype.AddFeatures[1].Features.Add(add_warpriest_blessing_resource);

            createQuickenBlessing();

            createAirBlessing();
            createAnimalBlessing();
            createArtificeBlessing();
            createChaosBlessing();
            createCharmBlessing();
            createCommunityBlessing();
            createDarknessBlessing();
            createCreateDeathBlessing();
            createDestructionBlessing();
            createEarthBlessing();
            createEvilBlessing();
            createFireBlessing();
            createGloryBlessing();
            createGoodBlessing();
            createHealingBlessing();
            createKnowledgeBlessing();
            createLawBlessing();
            createLiberationBlessing();
            createLuckBlessing();
            createMadnessBlessing();
            createMagicBlessing();
            createNobilityBlessing();
            createPlantBlessing();
            createProtectionBlessing();
            createReposeBlessing();
            createRuneBlessing();
            createStrengthBlessing();
            createSunBlessing();
            createTravelBlessing();
            createTrickeryBlessing();
            createWarBlessing();
            createWaterBlessing();
            createWeatherBlessing();
        }


        static void createQuickenBlessing()
        {
            
            quicken_blessing = Helpers.CreateFeatureSelection("QuickenBlessingFeatureSelection",
                                                              "Quicken Blessing",
                                                              "Choose one of your blessings that normally requires a standard action to use. You can expend two of your daily uses of blessings to deliver that blessing (regardless of whether it’s a minor or major effect) as a swift action instead.\n"
                                                              + "Special: You can take this feat multiple times. Each time you do, you choose a different blessing.",
                                                              "",
                                                              library.Get<BlueprintFeature>("ef7ece7bb5bb66a41b256976b27f424e").Icon,
                                                              FeatureGroup.Feat,
                                                              Helpers.PrerequisiteClassLevel(warpriest_class, 10, any: true),
                                                              Common.createPrerequisiteArchetypeLevel(Archetypes.DivineTracker.archetype.GetParentClass(), Archetypes.DivineTracker.archetype, 13, any: true)
                                                              );
            var quicken_blesing_buff = Helpers.CreateBuff("QuickenBlessingBuff",
                                                          quicken_blessing.Name,
                                                          quicken_blessing.Description,
                                                          "",
                                                          quicken_blessing.Icon,
                                                          null);

            quicken_blesing_ability = Helpers.CreateActivatableAbility("QuickenBlessingToggleAbility",
                                                                       quicken_blessing.Name,
                                                                       quicken_blessing.Description,
                                                                       "",
                                                                       quicken_blessing.Icon,
                                                                       quicken_blesing_buff,
                                                                       AbilityActivationType.Immediately,
                                                                       CommandType.Free,
                                                                       null);
            quicken_blesing_ability.DeactivateImmediately = true;


            quicken_blessing_feature = Common.ActivatableAbilityToFeature(quicken_blesing_ability);
            library.AddFeats(quicken_blessing);
        }


        static void addBlessing(string name_prefix, string Name, BlueprintAbility minor_blessing, BlueprintAbility major_blessing, string allowed_key)
        {
            addBlessing(name_prefix, Name, Common.AbilityToFeature(minor_blessing, false), Common.AbilityToFeature(major_blessing, false), allowed_key);
        }


        static void addBlessing(string name_prefix, string Name, BlueprintFeature minor_blessing, BlueprintFeature major_blessing, string allowed_key)
        {
            var allowed_blessings = Helpers.CreateFeature(name_prefix + "AllowedFeature",
                                                          "",
                                                          "",
                                                          "",
                                                          null,
                                                          FeatureGroup.None);
            allowed_blessings.HideInCharacterSheetAndLevelUp = true;

            foreach (var a in warpriest_deity_selection.AllFeatures)
            {
                var add_facts = a.GetComponent<AddFacts>();
                if (add_facts == null)
                {
                    continue;
                }
                var facts = add_facts.Facts;
                foreach (var f in facts)
                {
                    if (f.AssetGuid == allowed_key)
                    {
                        add_facts.Facts = add_facts.Facts.AddToArray(allowed_blessings);
                    }
                }
            }

            var progression = Helpers.CreateProgression(name_prefix + "Progression",
                                                        Name + " Blessing",
                                                        minor_blessing.Name + " (minor): " + minor_blessing.Description + "\n"
                                                        + major_blessing.Name + " (major): " + major_blessing.Description,
                                                        "",
                                                        null,
                                                        FeatureGroup.Domain,
                                                        Helpers.PrerequisiteFeature(allowed_blessings));
            progression.Classes = getWarpriestArray();
            progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, minor_blessing),
                                                         Helpers.LevelEntry(10, major_blessing)
                                                        };
            progression.UIGroups = new UIGroup[] { Helpers.CreateUIGroup(minor_blessing, major_blessing) };


            var divine_tracker_progression = Helpers.CreateProgression(name_prefix + "DivineTrackerProgression",
                                            Name + " Blessing",
                                            minor_blessing.Name + " (minor): " + minor_blessing.Description + "\n"
                                            + major_blessing.Name + " (major): " + major_blessing.Description,
                                            "",
                                            null,
                                            FeatureGroup.Domain,
                                            Helpers.PrerequisiteFeature(allowed_blessings));
            divine_tracker_progression.Classes = new BlueprintCharacterClass[] { getBlessingUsersArray()[1] };
            divine_tracker_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(4, minor_blessing),
                                                         Helpers.LevelEntry(13, major_blessing)
                                                        };
            divine_tracker_progression.UIGroups = new UIGroup[] { Helpers.CreateUIGroup(minor_blessing, major_blessing) };

            Archetypes.DivineTracker.blessings.AllFeatures = Archetypes.DivineTracker.blessings.AllFeatures.AddToArray(divine_tracker_progression);
            warpriest_blessings.AllFeatures = warpriest_blessings.AllFeatures.AddToArray(progression);

            blessings_map.Add(Name, progression);
            if (quicken_blessing_selections.ContainsKey(Name))
            {
                quicken_blessing_selections[Name].AddComponents(Helpers.Create<PrerequisiteMechanics.CompoundPrerequisite>(c =>
                                                                {
                                                                    c.prerequisite1 = Helpers.PrerequisiteClassLevel(warpriest_class, 10);
                                                                    c.prerequisite2 = Helpers.PrerequisiteFeature(progression);
                                                                    c.Group = Prerequisite.GroupType.Any;
                                                                }
                                                                ),
                                                                Helpers.Create<PrerequisiteMechanics.CompoundPrerequisite>(c =>
                                                                {
                                                                    c.prerequisite1 = Common.createPrerequisiteArchetypeLevel(Archetypes.DivineTracker.archetype.GetParentClass(), Archetypes.DivineTracker.archetype, 13);
                                                                    c.prerequisite2 = Helpers.PrerequisiteFeature(divine_tracker_progression);
                                                                    c.Group = Prerequisite.GroupType.Any;
                                                                }
                                                                )
                                                                );
            }
        }


        static void addBlessingResourceLogic(string blessing_name, BlueprintAbility blessing, int amount = 1, bool quicken = false, BlueprintAbility parent = null)
        {
            
            blessing.AddComponent(Helpers.CreateResourceLogic(warpriest_blessing_resource, amount: amount, cost_is_custom: true));

            if (!quicken)
            {
                blessing.AddComponent(Helpers.Create<NewMechanics.ResourseCostCalculatorWithDecreasingFacts>(r => r.cost_reducing_facts = Enumerable.Repeat<BlueprintFact>(warpriest_aspect_of_war_buff, amount).ToArray()));
            }
            else
            {
                var base_ability = parent ?? blessing;

                BlueprintBuff quicken_buff = null;
                if (!processed_quicken_ability_guid_buff_map.TryGetValue(base_ability.AssetGuid, out quicken_buff))
                {
                    quicken_buff = Helpers.CreateBuff(base_ability.name + "QuickenBuff",
                                                          "",
                                                          "",
                                                          "",
                                                          null,
                                                          null,
                                                          Helpers.Create<TurnActionMechanics.UseAbilitiesAsSwiftAction>(u => u.abilities = new BlueprintAbility[] { base_ability })
                                                          );
                    quicken_buff.SetBuffFlags(BuffFlags.HiddenInUi);

                    if (!quicken_blessing_selections.ContainsKey(blessing_name))
                    {
                        var quicken_feature = Helpers.CreateFeature(blessing_name + "QuickenFeature",
                                                                    "Quicken Blessing: " + blessing_name,
                                                                    quicken_blessing_feature.Description,
                                                                    "",
                                                                    base_ability.Icon,
                                                                    FeatureGroup.Feat,
                                                                    Common.createAddFeatureIfHasFact(quicken_blessing_feature, quicken_blessing_feature, not: true)
                                                                    );
                        quicken_blessing_selections[blessing_name] = quicken_feature;
                        quicken_blessing.AllFeatures = quicken_blessing.AllFeatures.AddToArray(quicken_feature);
                    }
                    
                    Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(quicken_blesing_ability.Buff, quicken_buff, quicken_blessing_selections[blessing_name]);
                    processed_quicken_ability_guid_buff_map.Add(base_ability.AssetGuid, quicken_buff);
                }
                
                blessing.AddComponent(Helpers.Create<NewMechanics.ResourseCostCalculatorWithDecreasingFacts>(r =>
                                                                                                                {
                                                                                                                    r.cost_reducing_facts = Enumerable.Repeat<BlueprintFact>(warpriest_aspect_of_war_buff, amount *2).ToArray();
                                                                                                                    r.cost_increasing_facts = Enumerable.Repeat<BlueprintFact>(quicken_buff, amount).ToArray();
                                                                                                                })
                                      );
            }
        }


        static void createAirBlessing()
        {
            var hurricane_bow = library.Get<BlueprintAbility>("3e9d1119d43d07c4c8ba9ebfd1671952");
            var hurricane_bow_buff = library.Get<BlueprintBuff>("002c51d933574824c8ef2b04c9d09ff5");

            var minor_buff = Helpers.CreateBuff("WarpriestAirBlessingMinorBuff",
                                                "Zephyr's Gift",
                                                "At 1st level, you can touch any one ranged weapon and enhance it with the quality of air. For 1 minute, any attacks made with the weapon receive + 1 bonus on attack and damage rolls. In addition, making a ranged attack with this weapon doesn’t provoke an attack of opportunity.",
                                                "",
                                                hurricane_bow.Icon,
                                                hurricane_bow_buff.FxOnStart,
                                                Common.createWeaponGroupAttackBonus(1, ModifierDescriptor.UntypedStackable, Kingmaker.Blueprints.Items.Weapons.WeaponFighterGroup.Bows),
                                                Common.createWeaponGroupAttackBonus(1, ModifierDescriptor.UntypedStackable, Kingmaker.Blueprints.Items.Weapons.WeaponFighterGroup.Crossbows),
                                                Common.createWeaponGroupAttackBonus(1, ModifierDescriptor.UntypedStackable, Kingmaker.Blueprints.Items.Weapons.WeaponFighterGroup.Thrown),
                                                Helpers.Create<NewMechanics.SpecifiedWeaponCategoryIgnoreAoo>(s => s.WeaponGroups = new WeaponFighterGroup[] { WeaponFighterGroup.Bows,
                                                                                                                                                               WeaponFighterGroup.Crossbows,
                                                                                                                                                               WeaponFighterGroup.Thrown})
                                                );

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestAirBlessingMinorAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      Helpers.savingThrowNone,
                                                      hurricane_bow.GetComponent<AbilitySpawnFx>(),
                                                      Helpers.CreateRunActions(apply_minor_buff)
                                                      );

            minor_ability.setMiscAbilityParametersTouchFriendly(works_on_self: true);
            addBlessingResourceLogic("Air", minor_ability, quicken: true);

            var charge_buff = library.Get<BlueprintBuff>("f36da144a379d534cad8e21667079066");
            var wings_demon = library.Get<BlueprintBuff>("3c958be25ab34dc448569331488bee27");
            var wings_dragon = library.Get<BlueprintBuff>("5a791c1b0bacee3459d7f5137fa0bd5f");
            var wings_angel = library.Get<BlueprintBuff>("d596694ff285f3f429528547f441b1c0");


            //var damage_action = Helpers.CreateActionDealDamage(DamageEnergyType.Electricity, Helpers.CreateContextDiceValue(DiceType.Zero, bonus: Helpers.CreateContextValue(AbilityRankType.DamageBonus)));
            var major_buff = Helpers.CreateBuff("WarpriestAirBlessingMajorBuff",
                                                "Soaring Assault",
                                                "At 10th level, you can touch an ally and give her the gift of flight for 1 minute. The ally gains immunity to ground based abilities and difficult terrain, as well as receives +3 dodge bonus against melee attacks. Whenever the ally succeeds at a charge attack, that attack deals an amount of additional electricity damage equal to your level.",
                                                "",
                                                wings_angel.Icon,
                                                null,
                                                //Helpers.Create<NewMechanics.AddInitiatorAttackWithWeaponTriggerOnCharge>(a => a.Action = Helpers.CreateActionList(damage_action)),
                                                Common.createAddWeaponEnergyDamageDiceBuffIfHasFact(Helpers.CreateContextDiceValue(DiceType.Zero, bonus: Helpers.CreateContextValue(AbilityRankType.DamageBonus)),
                                                                                                    DamageEnergyType.Electricity,
                                                                                                    charge_buff,
                                                                                                    AttackType.Melee, AttackType.Touch),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.SummClassLevelWithArchetype, classes: getBlessingUsersArray(), archetype: Archetypes.DivineTracker.archetype,
                                                                                type: AbilityRankType.DamageBonus, progression: ContextRankProgression.AsIs)
                                                );

            var apply_major_buff = Common.createContextActionApplyBuff(major_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var apply_wings_angel = Common.createContextActionApplyBuff(wings_angel, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var apply_wings_demon = Common.createContextActionApplyBuff(wings_demon, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var apply_wings_dragon = Common.createContextActionApplyBuff(wings_dragon, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var major_ability = Helpers.CreateAbility("WarpriestAirBlessingMajorAbility",
                                                      major_buff.Name,
                                                      major_buff.Description,
                                                      "",
                                                      major_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      Helpers.savingThrowNone,
                                                      Helpers.CreateRunActions(apply_major_buff, Helpers.CreateConditional(Helpers.CreateContextConditionAlignment(AlignmentComponent.Good, check_caster: true),
                                                                                                                           apply_wings_angel,
                                                                                                                           Helpers.CreateConditional(Helpers.CreateContextConditionAlignment(AlignmentComponent.Evil, check_caster: true),
                                                                                                                                                                                             apply_wings_demon,
                                                                                                                                                                                             apply_wings_dragon)
                                                                                                                           )
                                                                              )
                                                      );
            major_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic("Air", major_ability, quicken: true);

            addBlessing("WarpriestBlessingAir", "Air", minor_ability, major_ability, "6e5f4ff5a7010754ca78708ce1a9b233");
        }


        static void createAnimalBlessing()
        {
            var minor_buff = library.CopyAndAdd<BlueprintBuff>("a67b51a8074ae47438280be0a87b01b6", "WarpriestAnimalMinorBuff", ""); //animal fury
            minor_buff.SetBuffFlags(0);
            var animal_fury = library.Get<BlueprintFeature>("25954b1652bebc2409f9cb9d5728bceb");
            var magic_fang = library.Get<BlueprintAbility>("403cf599412299a4f9d5d925c7b9fb33");
            minor_buff.AddComponent(animal_fury.GetComponent<AddCalculatedWeapon>());
            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestAnimalBlessingMinorAbility",
                                                      "Animal Fury",
                                                      "At 1st level, you can touch one ally and grant it feral features. The ally gains 1 secondary bite attack that deals 1d8 points of damage if the ally is Medium or 1d6 if it’s Small. This effect lasts for 1 minute.",
                                                      "",
                                                      animal_fury.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      Helpers.savingThrowNone,
                                                      magic_fang.GetComponent<AbilitySpawnFx>(),
                                                      Helpers.CreateRunActions(apply_minor_buff)
                                                      );

            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic("Animal", minor_ability, quicken: true);

            string[] nature_ally_guids = new string[] {"28ea1b2e0c4a9094da208b4c186f5e4f", "060afb9e13d8a3547ad0dd20c407c0a5",
                                                      "6d8d59aa38713be4fa3be76c19107cc0", "8d3d5b62878d5b24391c1d7834d0d706", "f6751c3b22dbd884093e350a37420368" };

            var summon_na8 = library.Get<BlueprintAbility>("8d3d5b62878d5b24391c1d7834d0d706");

            List<ActionList> summon_actions = new List<ActionList>();
            foreach (var s in nature_ally_guids)
            {
                summon_actions.Add(library.Get<BlueprintAbility>(s).GetComponent<AbilityEffectRunAction>().Actions);
            }

            var major_ability = library.CopyAndAdd<BlueprintAbility>(summon_na8.AssetGuid, "WarpirestAnimalBlessingMajorAbility", "");
            Common.unsetAsFullRoundAction(major_ability);
            major_ability.SetName("Battle Companion (Animal)");
            major_ability.SetDescription("At 10th level, you can summon a battle companion. This ability functions as summon nature’s ally V with a duration of 1 minute. This ability can summon only one creature, regardless of the list used. For every 2 levels beyond 10th, the level of the summon nature’s ally spell increases by 1 (to a maximum of summon nature’s ally IX at 18th level).");
            major_ability.RemoveComponents<SpellComponent>();
            major_ability.Type = AbilityType.Supernatural;
            var action = Helpers.CreateRunActions(Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus), summon_actions.ToArray()));
            major_ability.ReplaceComponent<AbilityEffectRunAction>(action);
            major_ability.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(min: 10, max: 10));
            major_ability.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.SummClassLevelWithArchetype, classes: getBlessingUsersArray(), archetype: Archetypes.DivineTracker.archetype,
                                                                       progression: ContextRankProgression.DelayedStartPlusDivStep,
                                                                       startLevel: 10, stepLevel: 2, max: 5, type: AbilityRankType.StatBonus
                                                                      )
                                      );
            major_ability.LocalizedDuration = Helpers.oneMinuteDuration;
            major_ability.Parent = null;
            addBlessingResourceLogic("Animal", major_ability, quicken: true);
            addBlessing("WarpriestBlessingAnimal", "Animal", minor_ability, major_ability, "9f05f9da2ea5ae44eac47d407a0000e5");
        }


        static void createArtificeBlessing()
        {
            var resounding_blow_buff = library.Get<BlueprintBuff>("06173a778d7067a439acffe9004916e9");
            var construct_type = library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f");

            var enchant = Common.createWeaponEnchantment("WarpriestArtificeBlessingMinorEnchant",
                                                         "Crafter’s Wrath",
                                                         "Whenever this weapon deals damage to constructs, it bypasses damage reduction.",
                                                         "",
                                                         "",
                                                         "",
                                                         0,
                                                         null,
                                                         Helpers.Create<NewMechanics.EnchantmentMechanics.WeaponIgnoreDRIfTargetHasFact>(c => c.fact = construct_type)
                                                         );

            var minor_buff = Helpers.CreateBuff("WarpriestArtificeBlessingMinorBuff",
                                                enchant.Name,
                                                "At 1st level, you can touch one melee weapon and grant it greater power to harm and destroy crafted objects. For 1 minute, whenever this weapon deals damage to constructs, it bypasses damage reduction.",
                                                "",
                                                resounding_blow_buff.Icon,
                                                resounding_blow_buff.FxOnStart,
                                                Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), false, true, new BlueprintWeaponType[0], enchant)
                                                );

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestArtificeBlessingMinorAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      Helpers.savingThrowNone,
                                                      Helpers.CreateRunActions(apply_minor_buff)
                                                      );

            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic("Artifice", minor_ability, quicken: true);

            var spell_combat = library.Get<BlueprintFeature>("2464ba53317c7fc4d88f383fac2b45f9");
            var major_feature = Helpers.CreateFeature("WarpriestArtificeBlessingMajorFeature",
                                                "Spell Storing",
                                                "At 10th level, you can cast a single target non-personal spell of 3rd level or lower into a weapon that can be released on target upon successful attack.",
                                                "",
                                                spell_combat.Icon,
                                                FeatureGroup.None,
                                                Helpers.Create<SpellManipulationMechanics.FactStoreSpell>());

            var release_buff = Helpers.CreateBuff("WarpriestArtificeBlessingMajorToggleBuff",
                                                  major_feature.Name + ": Release",
                                                  major_feature.Description,
                                                  "",
                                                  major_feature.Icon,
                                                  null,
                                                  Helpers.Create<SpellManipulationMechanics.AddStoredSpellToCaption>(a => a.store_fact = major_feature));

            var major_activatable_ability = Helpers.CreateActivatableAbility("WarpriestArtificeBlessingMajorToggleAbility",
                                                                             major_feature.Name + ": Release",
                                                                             major_feature.Description,
                                                                             "",
                                                                             major_feature.Icon,
                                                                             release_buff,
                                                                             AbilityActivationType.Immediately,
                                                                             CommandType.Free,
                                                                             null,
                                                                             Helpers.Create<SpellManipulationMechanics.ActivatableAbilitySpellStoredInFactRestriction>(a => a.fact = major_feature));
            major_activatable_ability.DeactivateImmediately = true;

            var release_action = Helpers.Create<SpellManipulationMechanics.ReleaseSpellStoredInSpecifiedBuff>(r => r.fact = major_feature);
            var release_on_condition = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(release_buff), release_action);
            major_feature.AddComponent(Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(release_on_condition)));
            major_feature.AddComponent(Helpers.CreateAddFact(major_activatable_ability));

            int max_variants = 10; //due to ui limitation
            Predicate<AbilityData> check_slot_predicate = delegate (AbilityData spell)
            {
                return spell.SpellLevel <= 3
                        && (spell.Spellbook?.Blueprint == warpriest_class.Spellbook || spell.Spellbook?.Blueprint == Archetypes.DivineTracker.archetype.GetParentClass().Spellbook)
                        && spell.Blueprint.EffectOnEnemy == AbilityEffectOnUnit.Harmful
                        && spell.Blueprint.Range != AbilityRange.Personal
                        && spell.Blueprint.CanTargetEnemies
                        && !spell.Blueprint.CanTargetPoint
                        && !spell.Blueprint.IsFullRoundAction
                        && (!spell.Blueprint.HasVariants || spell.Variants.Count < max_variants)
                        && !spell.Blueprint.HasAreaEffect()
                        && (!spell.RequireMaterialComponent || spell.HasEnoughMaterialComponent);
            };

            for (int i = 0; i < max_variants; i++)
            {
                var major_ability = Helpers.CreateAbility($"WarpriestArtificeBlessingMajor{i + 1}Ability",
                                                          major_feature.Name,
                                                          major_feature.Description,
                                                          "",
                                                          major_feature.Icon,
                                                          AbilityType.Supernatural,
                                                          CommandType.Standard,
                                                          AbilityRange.Personal,
                                                          "",
                                                          "",
                                                          Helpers.Create<SpellManipulationMechanics.AbilityStoreSpellInFact>(s => { s.fact = major_feature; s.check_slot_predicate = check_slot_predicate; s.variant = i; })
                                                          );
                major_ability.setMiscAbilityParametersSelfOnly();
                addBlessingResourceLogic("Artifice", major_ability, quicken: true);
                major_feature.AddComponent(Helpers.CreateAddFact(major_ability));
            }
          
            addBlessing("WarpriestBlessingArtifice", "Artifice", Common.AbilityToFeature(minor_ability, false), major_feature, "9656b1c7214180f4b9a6ab56f83b92fb");
        }


        static void createChaosBlessing()
        {
            var sneak_attack = library.Get<BlueprintFeature>("df4f34f7cac73ab40986bc33f87b1a3c");
            var bless_weapon = library.Get<BlueprintAbility>("831e942864e924846a30d2e0678e438b");
            var weapon_bond = library.Get<BlueprintAbility>("7ff088ab58c69854b82ea95c2b0e35b4");
            var anarchic_enchantment = library.Get<BlueprintWeaponEnchantment>("57315bc1e1f62a741be0efde688087e9");

            var enchantment = Common.createWeaponEnchantment("WarpriestChaosMinorBlessingWeaponEcnchantment",
                                                             "Anarchic Strike",
                                                             "This weapon glows yellow or purple and deals an additional 1d6 points of damage against lawful creatures. It is also treated as chaotic for the purposes of overcoming damage reduction.",
                                                             "",
                                                             "",
                                                             "",
                                                             0,
                                                             anarchic_enchantment.WeaponFxPrefab,
                                                             Common.createWeaponDamageAgainstAlignment(DamageEnergyType.Unholy, DamageAlignment.Chaotic, AlignmentComponent.Lawful,
                                                                                                       Helpers.CreateContextDiceValue(DiceType.D6, Common.createSimpleContextValue(1)))
                                                             );

            var minor_buff = Helpers.CreateBuff("WarpriestChaosMinorBuff",
                                                enchantment.Name,
                                                "At 1st level, you can touch one weapon and grant it a chaotic blessing. For 1 minute, this weapon glows yellow or purple and deals an additional 1d6 points of damage against lawful creatures. During this time, it’s treated as chaotic for the purposes of overcoming damage reduction.",
                                                "",
                                                sneak_attack.Icon,
                                                null,
                                                Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), false, true, enchantment));

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestChaosBlessingMinorAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      Helpers.savingThrowNone,
                                                      weapon_bond.GetComponent<AbilitySpawnFx>(),
                                                      Helpers.CreateRunActions(apply_minor_buff)
                                                      );

            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic("Chaos", minor_ability, quicken: true);

            string[] summon_monster_guids = new string[] {"efa433a38e9c7c14bb4e780f8a3fe559", "0964bf88b582bed41b74e79596c4f6d9", "02de4dd8add69aa42a3d1330b573e2ab",
                                                      "2920d48574933c24391fbb9e18f87bf5", "eb6df7ddfc0669d4fb3fc9af4bd34bca", "e96593e67d206ab49ad1b567327d1e75" };

            var summon_m9 = library.Get<BlueprintAbility>("e96593e67d206ab49ad1b567327d1e75");

            List<ActionList> summon_actions = new List<ActionList>();
            foreach (var s in summon_monster_guids)
            {
                summon_actions.Add(library.Get<BlueprintAbility>(s).GetComponent<AbilityEffectRunAction>().Actions);
            }

            var major_ability = library.CopyAndAdd<BlueprintAbility>(summon_m9.AssetGuid, "WarpirestChaosBlessingMajorAbility", "");
            Common.unsetAsFullRoundAction(major_ability);
            major_ability.SetName("Battle Companion (Chaos)");
            major_ability.SetDescription("At 10th level, you can summon a battle companion. This ability functions as summon monster IV with a duration of 1 minute. This ability can summon only one creature, regardless of the list used. For every 2 levels beyond 10th, the level of the summon monster spell increases by 1 (to a maximum of summon monster IX at 20th level).");
            major_ability.RemoveComponents<SpellComponent>();
            major_ability.Type = AbilityType.Supernatural;
            var action = Helpers.CreateRunActions(Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus), summon_actions.ToArray()));
            major_ability.ReplaceComponent<AbilityEffectRunAction>(action);
            major_ability.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(min: 10, max: 10));
            major_ability.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.SummClassLevelWithArchetype, classes: getBlessingUsersArray(), archetype: Archetypes.DivineTracker.archetype,
                                                                       progression: ContextRankProgression.DelayedStartPlusDivStep,
                                                                       startLevel: 10, stepLevel: 2, max: 6, type: AbilityRankType.StatBonus
                                                                      )
                                      );
            major_ability.LocalizedDuration = Helpers.oneMinuteDuration;
            major_ability.Parent = null;
            addBlessingResourceLogic("Chaos", major_ability, quicken: true);
            addBlessing("WarpriestBlessingChaos", "Chaos", minor_ability, major_ability, "8c7d778bc39fec642befc1435b00f613");
        }


        static void createCharmBlessing()
        {
            var sancturay_logic = Helpers.Create<SanctuaryMechanics.Sanctuary>(c =>
            {
                c.save_type = SavingThrowType.Will;
                c.offensive_action_effect = SanctuaryMechanics.OffensiveActionEffect.REMOVE_FROM_TARGET;
            });
            warpriest_blessing_special_sancturay_buff = library.CopyAndAdd<BlueprintBuff>("525f980cb29bc2240b93e953974cb325", "WarpriestSpecialSanctuaryBuff", "");//invisibility
            warpriest_blessing_special_sancturay_buff.ComponentsArray = new BlueprintComponent[] { sancturay_logic, Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting) };

            var lesser_restoration = library.Get<BlueprintAbility>("e84fc922ccf952943b5240293669b171");
            var apply_minor_buff = Common.createContextActionApplyBuff(warpriest_blessing_special_sancturay_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestCharmMinorBlessingAbility",
                                                      "Charming Presence",
                                                      "At 1st level, you can touch an ally and grant an entrancing blessing. For 1 minute, the ally becomes mesmerizing to her opponents, filling them with either abject admiration or paralyzing fear. This effect functions as sanctuary, except if the ally attacks an opponent, the effect ends with respect to only that opponent. This is a mind-affecting effect.",
                                                      "",
                                                      lesser_restoration.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      Helpers.willNegates,
                                                      Helpers.CreateRunActions(apply_minor_buff),
                                                      Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(getBlessingUsersArray(), getBlessingUsersArchetypesArray(), StatType.Wisdom));
            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic("Charm", minor_ability, quicken: true);


            var swift_command = library.CopyAndAdd<BlueprintAbility>(NewSpells.command.AssetGuid, "WarpriestCharmDomainCommandAbility", "");
            swift_command.ActionType = CommandType.Swift;
            swift_command.Type = AbilityType.SpellLike;
            swift_command.RemoveComponents<SpellComponent>();
            var dc_replace = Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(getBlessingUsersArray(), getBlessingUsersArchetypesArray(), StatType.Wisdom);
            swift_command.ReplaceComponent<AbilityVariants>(a => a.Variants = Common.CreateAbilityVariantsReplace(swift_command, "WarpriestCharmDomain",
                                                                                                                  (v, vv) =>
                                                                                                                  {
                                                                                                                      v.Type = swift_command.Type;
                                                                                                                      v.ActionType = swift_command.ActionType;
                                                                                                                      v.RemoveComponents<SpellComponent>();
                                                                                                                      v.AddComponent(dc_replace);
                                                                                                                  },
                                                                                                                  false,
                                                                                                                  a.Variants));
            var clock_of_dreams_buff = library.Get<BlueprintBuff>("2e4b85213927f0a4ea2198e0f2a6028b");
            var major_buff = Helpers.CreateBuff("WarpriestCharmMajorBlessingBuff",
                                                "Dominance Aura",
                                                "At 10th level, you can surround yourself with a tangible aura of majesty for 1 minute. While this aura is active, once per round as a swift action you can issue a command (as the command spell) to one creature within 30 feet; the creature must succeed at a Will saving throw or submit for 1 round.",
                                                "",
                                                swift_command.Icon,
                                                clock_of_dreams_buff.FxOnStart,
                                                Helpers.CreateAddFact(swift_command)
                                                );
            var apply_major_buff = Common.createContextActionApplyBuff(major_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var major_ability = Helpers.CreateAbility("WarpriestCharmMajorBlessingAbility",
                                                      major_buff.Name,
                                                      major_buff.Description,
                                                      "",
                                                      major_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Personal,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_major_buff));
            major_ability.setMiscAbilityParametersSelfOnly();
            addBlessingResourceLogic("Charm", major_ability, quicken: true);
            addBlessing("WarpriestBlessingCharm", "Charm", minor_ability, major_ability, "f1ceba79ee123cc479cece27bc994ff2");
        }


        static void createCommunityBlessing()
        {
            var remove_fear_buff = library.Get<BlueprintBuff>("c5c86809a1c834e42a2eb33133e90a28");
            var eccli_blessing = library.Get<BlueprintAbility>("db0f61cd985ca09498cafde9a4b27a16");

            var aid_another_boost = Helpers.CreateFeature("AidAnotherBoostFeature", "", "", "", null, FeatureGroup.None);
            aid_another_boost.HideInCharacterSheetAndLevelUp = true;

            Helpers.SetField(Rebalance.aid_another_config, "m_FeatureList",
                             Helpers.GetField<BlueprintFeature[]>(Rebalance.aid_another_config, "m_FeatureList").AddToArray(aid_another_boost, aid_another_boost));
            var minor_buff = Helpers.CreateBuff("WarpriestCommunityMinorBlessingBuff",
                                                "Communal Aid",
                                                "At 1st level, you can touch an ally and grant it the blessing of community. For the next minute, whenever that ally uses the aid another action, the bonus granted increases to +4. You can instead use this ability on yourself as a swift action.",
                                                "",
                                                eccli_blessing.Icon,
                                                remove_fear_buff.FxOnStart,
                                                Helpers.CreateAddFacts(aid_another_boost)
                                                );

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);


            var minor_ability_base = Helpers.CreateAbility("WarpriestCommunityMinorBlessingBaseAbility",
                                          minor_buff.Name,
                                          minor_buff.Description,
                                          "",
                                          eccli_blessing.Icon,
                                          AbilityType.Supernatural,
                                          CommandType.Standard,
                                          AbilityRange.Touch,
                                          Helpers.oneMinuteDuration,
                                          Helpers.willNegates
                                          );
            minor_ability_base.setMiscAbilityParametersTouchFriendly();

            var minor_ability = Helpers.CreateAbility("WarpriestCommunityMinorBlessingAbility",
                                                      minor_buff.Name + " (Others)",
                                                      minor_buff.Description,
                                                      "",
                                                      eccli_blessing.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      Helpers.willNegates,
                                                      Helpers.CreateRunActions(apply_minor_buff));
            minor_ability.setMiscAbilityParametersTouchFriendly(works_on_self: false);
            addBlessingResourceLogic("Community", minor_ability, quicken: true, parent: minor_ability_base);

            var minor_ability2 = library.CopyAndAdd<BlueprintAbility>(minor_ability.AssetGuid, "WarpriestCommunityMinorBlessingSelfAbility", "");
            minor_ability2.CanTargetFriends = false;
            minor_ability2.CanTargetSelf = true;
            minor_ability2.ActionType = CommandType.Swift;
            minor_ability2.SetName(minor_buff.Name + " (Self)");
            addBlessingResourceLogic("Community", minor_ability2, quicken: true, parent: minor_ability_base);
            minor_ability_base.AddComponent(Helpers.CreateAbilityVariants(minor_ability_base, minor_ability, minor_ability2));

            var true_strike = library.Get<BlueprintAbility>("2c38da66e5a599347ac95b3294acbe00");

            var target_melee_buff = Helpers.CreateBuff("WarpriestCommunityMajorBlessingMeleeTargetBuff",
                                                       "",
                                                       "",
                                                       "",
                                                       null,
                                                       null);
            target_melee_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var target_ranged_buff = library.CopyAndAdd<BlueprintBuff>(target_melee_buff.AssetGuid, "WarpriestCommunityMajorBlessingRangedTargetBuff", "");
            var target_melee_buff2 = library.CopyAndAdd<BlueprintBuff>(target_melee_buff.AssetGuid, "WarpriestCommunityMajorBlessingMeleeTarget2Buff", "");
            var target_ranged_buff2 = library.CopyAndAdd<BlueprintBuff>(target_melee_buff.AssetGuid, "WarpriestCommunityMajorBlessingRangedTarget2Buff", "");

            var apply_melee_buff1 = Common.createContextActionApplyBuff(target_melee_buff,
                                                             Helpers.CreateContextDuration(Common.createSimpleContextValue(1)),
                                                             dispellable: false);
            var apply_melee_buff2 = Common.createContextActionApplyBuff(target_melee_buff2,
                                                 Helpers.CreateContextDuration(Common.createSimpleContextValue(1)),
                                                 dispellable: false);
            var apply_ranged_buff1 = Common.createContextActionApplyBuff(target_ranged_buff,
                                                 Helpers.CreateContextDuration(Common.createSimpleContextValue(1)),
                                                 dispellable: false);
            var apply_ranged_buff2 = Common.createContextActionApplyBuff(target_ranged_buff2,
                                                 Helpers.CreateContextDuration(Common.createSimpleContextValue(1)),
                                                 dispellable: false);

            var ally_buff = Helpers.CreateBuff("WarpriestCommunityMajorBlessingAllyBuff",
                                                     "",
                                                     "",
                                                     "",
                                                     null,
                                                     null,
                                                     Helpers.Create<NewMechanics.AttackBonusAgainstFactsOwner>(a =>
                                                                                                              {
                                                                                                                  a.Bonus = Common.createSimpleContextValue(2);
                                                                                                                  a.attack_types = new AttackType[] { AttackType.Melee, AttackType.Touch };
                                                                                                                  a.Descriptor = ModifierDescriptor.Insight;
                                                                                                                  a.only_from_caster = true;
                                                                                                                  a.CheckedFacts = new BlueprintUnitFact[] { target_melee_buff, target_melee_buff2 };
                                                                                                              }),
                                                      Helpers.Create<NewMechanics.AttackBonusAgainstFactsOwner>(a =>
                                                                                                          {
                                                                                                              a.Bonus = Common.createSimpleContextValue(2);
                                                                                                              a.attack_types = new AttackType[] { AttackType.Ranged, AttackType.RangedTouch };
                                                                                                              a.Descriptor = ModifierDescriptor.Insight;
                                                                                                              a.only_from_caster = true;
                                                                                                              a.CheckedFacts = new BlueprintUnitFact[] { target_ranged_buff, target_ranged_buff2 };
                                                                                                          })
                                                      );
            ally_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var area_effect = library.CopyAndAdd<BlueprintAbilityAreaEffect>("8c5617838c1195443b584e005d0913eb", "WarpriestCommunityMajorBlessingArea", "");
            area_effect.ReplaceComponent<AbilityAreaEffectBuff>(a => a.Buff = ally_buff);
            var major_buff = Helpers.CreateBuff("WarpriestCommunityMajorBlessingCasterBuff",
                                                 "Fight as One",
                                                 "At 10th level, you can rally your allies to fight together. For 1 minute, whenever you make a successful melee or ranged attack against a foe, allies within 10 feet of you gain a +2 insight bonus on attacks of the same type you made against that foe—melee attacks if you made a melee attack, or ranged attacks if you made a ranged attack. If you score a critical hit, this bonus increases to +4 until the start of your next turn.",
                                                 "",
                                                 true_strike.Icon,
                                                 null,
                                                 Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(apply_melee_buff1),
                                                                                                  check_weapon_range_type: true, range_type: AttackTypeAttackBonus.WeaponRangeType.Melee),
                                                 Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(apply_melee_buff2), critical_hit: true,
                                                                                                  check_weapon_range_type: true, range_type: AttackTypeAttackBonus.WeaponRangeType.Melee),
                                                 Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(apply_ranged_buff1),
                                                                                                  check_weapon_range_type: true, range_type: AttackTypeAttackBonus.WeaponRangeType.Ranged),
                                                 Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(apply_ranged_buff2), critical_hit: true,
                                                                                                  check_weapon_range_type: true, range_type: AttackTypeAttackBonus.WeaponRangeType.Ranged),
                                                 Common.createAddAreaEffect(area_effect)
                                                 );

            var apply_major_buff = Common.createContextActionApplyBuff(major_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var major_ability = Helpers.CreateAbility("WarpriestCommunityMajorBlessingAbility",
                                                      major_buff.Name,
                                                      major_buff.Description,
                                                      "",
                                                      major_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Personal,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_major_buff),
                                                      true_strike.GetComponent<AbilitySpawnFx>()
                                                      );
            major_ability.setMiscAbilityParametersSelfOnly();
            addBlessingResourceLogic("Community", major_ability, quicken: true);


            addBlessing("WarpriestBlessingCommunity", "Community",
                        Helpers.CreateFeature("WarpriesCommunityBlessingMinorFeature",
                                              minor_buff.Name,
                                              minor_buff.Description,
                                              "",
                                              minor_buff.Icon,
                                              FeatureGroup.None,
                                              Helpers.CreateAddFact(minor_ability_base)
                                              ),
                        Common.AbilityToFeature(major_ability, false),
                        "c87004460f3328c408d22c5ead05291f");
        }


        static void createDarknessBlessing()
        {
            var blur_buff = library.Get<BlueprintBuff>("dd3ad347240624d46a11a092b4dd4674"); //blur

            var apply_minor_buff = Common.createContextActionApplyBuff(blur_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestDarknessMinorBlessingAbility",
                                                      "Enshrouding Darkness",
                                                      "At 1st level, you can touch an ally and bestow a darkness blessing. For 1 minute, the ally becomes enshrouded in shadows while in combat, granting it concealment (20%). Creatures that are normally able to see in supernatural darkness ignore this concealment.",
                                                      "",
                                                      blur_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_minor_buff));
            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic("Darkness", minor_ability, quicken: true);


            var blindness_buff = library.Get<BlueprintBuff>("187f88d96a0ef464280706b63635f2af");
            var blindness_spell = library.Get<BlueprintAbility>("46fd02ad56c35224c9c91c88cd457791");

            var apply_major_buff = Common.createContextSavedApplyBuff(blindness_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), is_dispellable: true);
            var apply_major_buff_save = Common.createContextActionSavingThrow(SavingThrowType.Will, Helpers.CreateActionList(apply_major_buff));
            var major_ability = Helpers.CreateAbility("WarpriestDarknessMajorBlessingAbility",
                                                      "Darkened Vision",
                                                      "At 10th level, you can place a shroud of darkness around the eyes of one foe within 30 feet. The target must succeed at a Will saving throw or be blinded for 1 minute.",
                                                      "",
                                                      blindness_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Close,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_major_buff_save),
                                                      blindness_spell.GetComponent<AbilityTargetHasFact>(),
                                                      Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(getBlessingUsersArray(), getBlessingUsersArchetypesArray(), StatType.Wisdom)
                                                      );
            major_ability.setMiscAbilityParametersSingleTargetRangedHarmful(test_mode);
            addBlessingResourceLogic("Darkness", major_ability, quicken: true);

            addBlessing("WarpriestBlessingDarkness", "Darkness", minor_ability, major_ability, "6d8e7accdd882e949a63021af5cde4b8");
        }


        static void createCreateDeathBlessing()
        {
            var undead_bloodline_progression = library.Get<BlueprintProgression>("a1a8bf61cadaa4143b2d4966f2d1142e");
            var vampiric_touch = library.Get<BlueprintAbility>("6cbb040023868574b992677885390f92");
            var false_life = library.Get<BlueprintAbility>("7a5b5bf845779a941a67251539545762");
            var minor_buff = Helpers.CreateBuff("WarpriestDeathMinorBlessingBuff",
                                                "From the Grave",
                                                "At 1st level, you can take on a corpse-like visage for 1 minute, making you more intimidating and giving you undead-like protection from harm. You gain a +4 bonus on Intimidate checks, as well as a +2 profane bonus on saving throws against disease, mind-affecting effects, paralysis, poison, and stun.",
                                                "",
                                                undead_bloodline_progression.Icon,
                                                null,
                                                Helpers.CreateAddStatBonus(StatType.CheckIntimidate, 4, ModifierDescriptor.Profane),
                                                Common.createSavingThrowBonusAgainstDescriptor(2, ModifierDescriptor.Profane, SpellDescriptor.Disease | SpellDescriptor.MindAffecting | SpellDescriptor.Paralysis | SpellDescriptor.Poison | SpellDescriptor.Stun)
                                                );

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestDeathMinorBlessingAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Personal,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_minor_buff),
                                                      false_life.GetComponent<AbilitySpawnFx>());

            minor_ability.setMiscAbilityParametersSelfOnly();
            addBlessingResourceLogic("Death", minor_ability, quicken: true);


            var undead = library.Get<BlueprintUnitFact>("734a29b693e9ec346ba2951b27987e33");
            var construct = library.Get<BlueprintUnitFact>("fd389783027d63343b4a5634bd81645f");

            var energy_drain = Helpers.CreateActionEnergyDrain(Helpers.CreateContextDiceValue(DiceType.Zero, bonus: Common.createSimpleContextValue(1)),
                                                               Helpers.CreateContextDuration(bonus: Common.createSimpleContextValue(1), rate: DurationRate.Minutes),
                                                               Kingmaker.RuleSystem.Rules.EnergyDrainType.Temporary);
            var effect_action = Helpers.CreateConditional(new Condition[] { Helpers.CreateConditionHasFact(undead, not: true), Helpers.CreateConditionHasFact(construct, not: true) },
                                                                    energy_drain);
            var projectile_action = vampiric_touch.GetComponent<AbilityEffectRunAction>().Actions.Actions.Last();

            var ability = Helpers.CreateAbility("WarpriestDeathMajorBlessingBaseAbility",
                                                "Death’s Touch",
                                                "At 10th level, you can make a melee touch attack against an opponent to deliver grim suffering. If you succeed, you inflict 1 temporary negative level on the target for 1 minute. Alternatively, you can activate this ability as a swift action upon hitting an opponent with a melee attack. These temporary negative levels stack. You gain no benefit from imposing these negative levels (such as the temporary hit points undead gain from enervation).",
                                                "",
                                                vampiric_touch.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Touch,
                                                Helpers.oneMinuteDuration,
                                                Helpers.savingThrowNone,
                                                vampiric_touch.GetComponent<AbilityDeliverTouch>(),
                                                Helpers.CreateRunActions(effect_action, projectile_action),
                                                vampiric_touch.GetComponent<AbilityTargetHasFact>()
                                                );

            ability.setMiscAbilityParametersTouchHarmful(works_on_allies: true);
            var major_ability_touch = Helpers.CreateAbility("WarpriestDeathMajorBlessingTouchAbility",
                                                            ability.Name,
                                                            ability.Description,
                                                            "",
                                                            ability.Icon,
                                                            AbilityType.Supernatural,
                                                            CommandType.Standard,
                                                            AbilityRange.Touch,
                                                            "",
                                                            Helpers.savingThrowNone,
                                                            Helpers.CreateStickyTouch(ability)
                                                            );

            major_ability_touch.setMiscAbilityParametersTouchHarmful(works_on_allies: true);
            addBlessingResourceLogic("Death", major_ability_touch, quicken: true);

            var on_hit_action = Helpers.CreateActionList(effect_action,
                                                         projectile_action);
            var spend_resource = Common.createContextActionSpendResource(warpriest_blessing_resource, 1, warpriest_aspect_of_war_buff);
            var on_hit_buff = Helpers.CreateBuff("WarpriestDeathMajorOnHitBuff",
                                                 major_ability_touch.Name,
                                                 major_ability_touch.Description,
                                                 "",
                                                 major_ability_touch.Icon,
                                                 null,
                                                 Common.createAddInitiatorAttackWithWeaponTrigger(on_hit_action, check_weapon_range_type: true, only_first_hit: true),
                                                 Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(spend_resource), on_initiator: true,
                                                                                                  check_weapon_range_type: true, only_first_hit: true)
                                                 );
            var major_activatable_ability = Helpers.CreateActivatableAbility("WarpriestDeathMajorBlessingOnHitActivatable",
                                                                             major_ability_touch.Name + " (On Hit)",
                                                                             major_ability_touch.Description,
                                                                             "",
                                                                             major_ability_touch.Icon,
                                                                             on_hit_buff,
                                                                             AbilityActivationType.Immediately,
                                                                             CommandType.Free,
                                                                             null,
                                                                             Helpers.CreateActivatableResourceLogic(warpriest_blessing_resource, ResourceSpendType.Never)
                                                                             );
            if (!test_mode)
            {
                major_activatable_ability.AddComponent(Common.createActivatableAbilityUnitCommand(CommandType.Swift));
            }
            major_ability_touch.AddComponent(Common.createAbilityCasterHasNoFacts(on_hit_buff));

            addBlessing("WarpriestBlessingDeath", "Death",
                        Common.AbilityToFeature(minor_ability, false),
                        Helpers.CreateFeature("WarpriestDeathMajorBlessingFeature",
                                              major_ability_touch.Name,
                                              major_ability_touch.Description,
                                              "",
                                              major_ability_touch.Icon,
                                              FeatureGroup.None,
                                              Helpers.CreateAddFacts(major_ability_touch, major_activatable_ability)
                                              ),
                         "a099afe1b0b32554199b230699a69525");
        }


        static void createDestructionBlessing()
        {
            var resounding_blow_buff = library.Get<BlueprintBuff>("06173a778d7067a439acffe9004916e9");
            var minor_buff = Helpers.CreateBuff("WarpriestDestructionMinorBlessingBuff",
                                    "Destructive Attacks",
                                    "At 1st level, you can touch an ally and bless it with the power of destruction. For 1 minute, the ally gains a morale bonus on weapon damage rolls equal to half your level (minimum 1).",
                                    "",
                                    resounding_blow_buff.Icon,
                                    resounding_blow_buff.FxOnStart,
                                    Helpers.CreateAddContextStatBonus(StatType.AdditionalDamage, ModifierDescriptor.Morale, rankType: AbilityRankType.DamageBonus),
                                    Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.SummClassLevelWithArchetype, classes: getBlessingUsersArray(), archetype: Archetypes.DivineTracker.archetype,
                                                                    type: AbilityRankType.DamageBonus, progression: ContextRankProgression.Div2, min: 1)
                                    );

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestDestructionMinorBlessingAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_minor_buff)
                                                      );

            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic("Destruction", minor_ability, quicken: true);

            var defensive_stance_buff = library.Get<BlueprintBuff>("3dccdf27a8209af478ac71cded18a271");
            var major_buff = Helpers.CreateBuff("WarpriestDestructionMajorBlessingBuff",
                                                "Heart of Carnage",
                                                "At 10th level, you can touch an ally and bless it with even greater destructive power. For 1 minute, the ally gains a +4 insight bonus on attack rolls made to confirm critical hits and has a 50% chance to treat any critical hit or sneak attack against it as a normal hit.",
                                                "",
                                                defensive_stance_buff.Icon,
                                                defensive_stance_buff.FxOnStart,
                                                Common.createCriticalConfirmationBonus(4),
                                                Common.createAddFortification(50)
                                                );

            var apply_major_buff = Common.createContextActionApplyBuff(major_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var major_ability = Helpers.CreateAbility("WarpriestDestructionMajorBlessingAbility",
                                                      major_buff.Name,
                                                      major_buff.Description,
                                                      "",
                                                      major_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_major_buff)
                                                      );

            major_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic("Destruction", major_ability, quicken: true);

            addBlessing("WarpriestBlessingDestruction", "Destruction", minor_ability, major_ability, "6832681c9a91bf946a1d9da28c5be4b4");
        }


        static void createEarthBlessing()
        {
            var weapon_bond = library.Get<BlueprintAbility>("7ff088ab58c69854b82ea95c2b0e35b4");
            var bless_weapon = library.Get<BlueprintAbility>("831e942864e924846a30d2e0678e438b");
            var acid_enchantment = library.Get<BlueprintWeaponEnchantment>("633b38ff1d11de64a91d490c683ab1c8");
            var acid_ray = library.Get<BlueprintAbility>("435222be97067a447b2b40d3c58a058e");

            var enchantment = Common.createWeaponEnchantment("WarpriestEarthMinorBlessingWeaponEcnchantment",
                                                             "Acid Strike",
                                                             "This weapon emits acrid fumes that deal an additional 1d4 points of acid damage with each strike.",
                                                             "",
                                                             "",
                                                             "",
                                                             0,
                                                             acid_enchantment.WeaponFxPrefab,
                                                             Common.weaponEnergyDamageDice(DamageEnergyType.Acid, new DiceFormula(1, DiceType.D4))
                                                             );
            var minor_buff = Helpers.CreateBuff("WarpriestEarthMinorBlessingBuff",
                                     enchantment.Name,
                                     "At 1st level, you can touch one weapon and enhance it with acidic potency. For 1 minute, this weapon emits acrid fumes that deal an additional 1d4 points of acid damage with each strike.",
                                     "",
                                     acid_ray.Icon,
                                     null,
                                     Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), false, true, enchantment)
                                    );

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestEarthMinorBlessingAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_minor_buff),
                                                      weapon_bond.GetComponent<AbilitySpawnFx>()
                                                      );

            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic("Earth", minor_ability, quicken: true);

            var stoneskin_buff = library.Get<BlueprintBuff>("7aeaf147211349b40bb55c57fec8e28d");
            var major_buff = Helpers.CreateBuff("WarpriestEarthMajorBlessingBuff",
                        "Armor of Earth",
                        "At 10th level, you can touch an ally and harden its armor or clothing. For 1 minute, the ally gains DR 1/—. For every 2 levels beyond 10th, this DR increases by 1 (to a maximum of DR 5/— at 18th level). This doesn’t stack with any other damage resistance or reduction.",
                        "",
                        stoneskin_buff.Icon,
                        stoneskin_buff.FxOnStart,
                        Common.createContextPhysicalDR(Helpers.CreateContextValue(AbilityRankType.StatBonus)),
                        Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.SummClassLevelWithArchetype, classes: getBlessingUsersArray(), archetype: Archetypes.DivineTracker.archetype,
                                                        type: AbilityRankType.StatBonus, progression: ContextRankProgression.StartPlusDivStep, startLevel: 10, stepLevel: 2, max: 5)
                        );

            var apply_major_buff = Common.createContextActionApplyBuff(major_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var major_ability = Helpers.CreateAbility("WarpriestEarthMajorBlessingAbility",
                                                      major_buff.Name,
                                                      major_buff.Description,
                                                      "",
                                                      major_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_major_buff)
                                                      );

            major_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic("Earth", major_ability, quicken: true);

            addBlessing("WarpriestBlessingEarth", "Earth", minor_ability, major_ability, "5ca99a6ae118feb449dbbd165a8fe7c4");
        }


        static void createEvilBlessing()
        {
            var unholy_weapon = library.Get<BlueprintActivatableAbility>("561803a819460f34ea1fe079edabecce");
            var weapon_bond = library.Get<BlueprintAbility>("7ff088ab58c69854b82ea95c2b0e35b4");
            var unholy_enchantment = library.Get<BlueprintWeaponEnchantment>("d05753b8df780fc4bb55b318f06af453");

            var enchantment = Common.createWeaponEnchantment("WarpriestEvilMinorBlessingWeaponEcnchantment",
                                                             "Unholy Strike",
                                                             "This weapon takes on a black, orange, or violet cast and deals an additional 1d6 points of damage against good creatures. During this time, it’s treated as evil for the purposes of overcoming damage reduction.",
                                                             "",
                                                             "",
                                                             "",
                                                             0,
                                                             unholy_enchantment.WeaponFxPrefab,
                                                             Common.createWeaponDamageAgainstAlignment(DamageEnergyType.Unholy, DamageAlignment.Evil, AlignmentComponent.Good,
                                                                                                       Helpers.CreateContextDiceValue(DiceType.D6, Common.createSimpleContextValue(1)))
                                                             );

            var minor_buff = Helpers.CreateBuff("WarpriestEvilMinorBuff",
                                                enchantment.Name,
                                                "At 1st level, you can touch one weapon and give it an evil blessing. For 1 minute, this weapon takes on a black, orange, or violet cast and deals an additional 1d6 points of damage against good creatures. During this time, it’s treated as evil for the purposes of overcoming damage reduction.",
                                                "",
                                                unholy_weapon.Icon,
                                                null,
                                                Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), false, true, enchantment));

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestEvilBlessingMinorAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      Helpers.savingThrowNone,
                                                      weapon_bond.GetComponent<AbilitySpawnFx>(),
                                                      Helpers.CreateRunActions(apply_minor_buff)
                                                      );

            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic("Evil", minor_ability, quicken: true);

            string[] summon_monster_guids = new string[] {"efa433a38e9c7c14bb4e780f8a3fe559", "0964bf88b582bed41b74e79596c4f6d9", "02de4dd8add69aa42a3d1330b573e2ab",
                                                      "2920d48574933c24391fbb9e18f87bf5", "eb6df7ddfc0669d4fb3fc9af4bd34bca", "e96593e67d206ab49ad1b567327d1e75" };

            var summon_m9 = library.Get<BlueprintAbility>("e96593e67d206ab49ad1b567327d1e75");

            List<ActionList> summon_actions = new List<ActionList>();
            foreach (var s in summon_monster_guids)
            {
                summon_actions.Add(library.Get<BlueprintAbility>(s).GetComponent<AbilityEffectRunAction>().Actions);
            }

            var major_ability = library.CopyAndAdd<BlueprintAbility>(summon_m9.AssetGuid, "WarpirestEvilBlessingMajorAbility", "");
            Common.unsetAsFullRoundAction(major_ability);
            major_ability.SetName("Battle Companion (Evil)");
            major_ability.SetDescription("At 10th level, you can summon a battle companion. This ability functions as summon monster IV with a duration of 1 minute. This ability can summon only one creature, regardless of the list used. For every 2 levels beyond 10th, the level of the summon monster spell increases by 1 (to a maximum of summon monster IX at 20th level).");
            major_ability.RemoveComponents<SpellComponent>();
            major_ability.Type = AbilityType.Supernatural;
            var action = Helpers.CreateRunActions(Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus), summon_actions.ToArray()));
            major_ability.ReplaceComponent<AbilityEffectRunAction>(action);
            major_ability.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(min: 10, max: 10));
            major_ability.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.SummClassLevelWithArchetype, classes: getBlessingUsersArray(), archetype: Archetypes.DivineTracker.archetype,
                                                                       progression: ContextRankProgression.DelayedStartPlusDivStep,
                                                                       startLevel: 10, stepLevel: 2, max: 6, type: AbilityRankType.StatBonus
                                                                      )
                                      );
            major_ability.LocalizedDuration = Helpers.oneMinuteDuration;
            major_ability.Parent = null;
            addBlessingResourceLogic("Evil", major_ability, quicken: true);
            addBlessing("WarpriestBlessingEvil", "Evil", minor_ability, major_ability, "351235ac5fc2b7e47801f63d117b656c");
        }


        static void createFireBlessing()
        {
            var weapon_bond = library.Get<BlueprintAbility>("7ff088ab58c69854b82ea95c2b0e35b4");
            var flaming_enchantment = library.Get<BlueprintWeaponEnchantment>("30f90becaaac51f41bf56641966c4121");
            var bless_weapon = library.Get<BlueprintAbility>("831e942864e924846a30d2e0678e438b");

            var enchantment = Common.createWeaponEnchantment("WarpriestFireMinorBlessingWeaponEcnchantment",
                                                             "Fire Strike",
                                                             "This weapon glows red-hot and deals an additional 1d4 points of fire damage with each hit.",
                                                             "",
                                                             "",
                                                             "",
                                                             0,
                                                             flaming_enchantment.WeaponFxPrefab,
                                                             Common.weaponEnergyDamageDice(DamageEnergyType.Fire, new DiceFormula(1, DiceType.D4))
                                                             );
            var minor_buff = Helpers.CreateBuff("WarpriestFireMinorBlessingBuff",
                                     enchantment.Name,
                                     "At 1st level, you can touch one weapon and enhance it with the grandeur of fire. For 1 minute, this weapon glows red-hot and deals an additional 1d4 points of fire damage with each hit.",
                                     "",
                                     bless_weapon.Icon,
                                     null,
                                     Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), false, true, enchantment)
                                    );

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestFireMinorBlessingAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_minor_buff),
                                                      weapon_bond.GetComponent<AbilitySpawnFx>()
                                                      );

            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic("Fire", minor_ability, quicken: true);

            var warm_shield = NewSpells.fire_shield_variants[DamageEnergyType.Fire];
            var major_buff = library.CopyAndAdd<BlueprintBuff>(NewSpells.fire_shield_buffs[DamageEnergyType.Fire], "WarpriestFireMajorBlessingBuff", "");
            major_buff.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.SummClassLevelWithArchetype, classes: getBlessingUsersArray(), archetype: Archetypes.DivineTracker.archetype,
                                                                                           max: 15, type: AbilityRankType.DamageBonus
                                                                                           )
                                                          );
            major_buff.SetName("Armor of Flame");
            major_buff.SetDescription("At 10th level, you can touch an ally to wreath it in flames. This works as fire shield (warm shield only) with a duration of 1 minute.");

            var apply_major_buff = Common.createContextActionApplyBuff(major_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var major_ability = Helpers.CreateAbility("WarpriestFireMajorBlessingAbility",
                                                      major_buff.Name,
                                                      major_buff.Description,
                                                      "",
                                                      major_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_major_buff),
                                                      warm_shield.GetComponent<SpellDescriptorComponent>(),
                                                      warm_shield.GetComponent<AbilitySpawnFx>()
                                                      );

            major_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic("Fire", major_ability, quicken: true);

            addBlessing("WarpriestBlessingFire", "Fire", minor_ability, major_ability, "8d4e9731082008640b28417f577f5f31");
        }


        static void createGloryBlessing()
        {
            var lesser_restoration = library.Get<BlueprintAbility>("e84fc922ccf952943b5240293669b171");
            var apply_minor_buff = Common.createContextActionApplyBuff(warpriest_blessing_special_sancturay_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestGloryMinorBlessingAbility",
                                                      "Glorious Presence",
                                                      "At 1st level, you can touch an ally and grant it a glorious blessing. For 1 minute, the ally becomes mesmerizing to her foes. This functions as sanctuary, except if the ally attacks an opponent, this effect ends with respect to only that opponent. This is a mind-affecting effect.",
                                                      "",
                                                      lesser_restoration.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      Helpers.willNegates,
                                                      Helpers.CreateRunActions(apply_minor_buff),
                                                      Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(getBlessingUsersArray(), getBlessingUsersArchetypesArray(), StatType.Wisdom));
            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic("Glory", minor_ability, quicken: true);

            var heroism = library.Get<BlueprintAbility>("5ab0d42fb68c9e34abae4921822b9d63");
            var cornugon_smash = library.Get<BlueprintFeature>("ceea53555d83f2547ae5fc47e0399e14");
            var demoralize_action = ((Conditional)cornugon_smash.GetComponent<AddInitiatorAttackWithWeaponTrigger>().Action.Actions[0]).IfTrue.Actions[0];
            var spend_resource = Common.createContextActionSpendResource(warpriest_blessing_resource, 1, warpriest_aspect_of_war_buff);
            var demoralize_on_damage = Helpers.Create<AddOutgoingDamageTrigger>(a =>
                                                                                {
                                                                                    a.Actions = Helpers.CreateActionList(demoralize_action);
                                                                                    a.NotZeroDamage = true;
                                                                                });


            var major_buff = Helpers.CreateBuff("WarpriestGloryMajorOnHitBuff",
                                                "Demoralizing Glory",
                                                "At 10th level, by spending a swift action, for 1 round when you successfully damage an opponent with an attack or a spell, you can attempt to demoralize that opponent with the Intimidate skill using your ranks in Intimidate or your warpriest level, whichever is higher.",
                                                "",
                                                heroism.Icon,
                                                null,
                                                demoralize_on_damage,
                                                Helpers.CreateAddFactContextActions(newRound: spend_resource));

            major_buff.AddComponent(Helpers.Create<NewMechanics.ReplaceSkillRankWithClassLevel>(r =>
                                                                                                {
                                                                                                    r.character_class = warpriest_class;
                                                                                                    r.skill = StatType.CheckIntimidate;
                                                                                                    r.reason = major_buff;
                                                                                                })
                                   );

            var major_activatable_ability = Helpers.CreateActivatableAbility("WarpriestGloryMajorBlessingActivatable",
                                                                             major_buff.Name,
                                                                             major_buff.Description,
                                                                             "",
                                                                             major_buff.Icon,
                                                                             major_buff,
                                                                             AbilityActivationType.Immediately,
                                                                             CommandType.Free,
                                                                             null,
                                                                             Helpers.CreateActivatableResourceLogic(warpriest_blessing_resource, ResourceSpendType.Never)
                                                                             );
            if (!test_mode)
            {
                Helpers.SetField(major_activatable_ability, "m_ActivateWithUnitCommand", CommandType.Swift);
                major_activatable_ability.DeactivateIfCombatEnded = true;
                major_activatable_ability.AddComponent(Common.createActivatableAbilityUnitCommand(CommandType.Swift));
            }
            addBlessing("WarpriestBlessingGlory", "Glory",
                        Common.AbilityToFeature(minor_ability, false),
                        Common.ActivatableAbilityToFeature(major_activatable_ability, false),
                        "2418251fa9c8ada4bbfbaaf5c90ac200"
                        );
        }


        static void createGoodBlessing()
        {
            var weapon_bond = library.Get<BlueprintAbility>("7ff088ab58c69854b82ea95c2b0e35b4");
            var holy_weapon = library.Get<BlueprintActivatableAbility>("ce0ece459ebed9941bb096f559f36fa8");
            var bless_weapon = library.Get<BlueprintAbility>("831e942864e924846a30d2e0678e438b");
            var holy_enchantment = library.Get<BlueprintWeaponEnchantment>("28a9964d81fedae44bae3ca45710c140");

            var enchantment = Common.createWeaponEnchantment("WarpriestGoodMinorBlessingWeaponEcnchantment",
                                                             "Holy Strike",
                                                             "This weapon glows green, white, or yellow-gold and deals an additional 1d6 points of damage against evil creatures. During this time, it’s treated as good for the purposes of overcoming damage reduction.",
                                                             "",
                                                             "",
                                                             "",
                                                             0,
                                                             holy_enchantment.WeaponFxPrefab,
                                                             Common.createWeaponDamageAgainstAlignment(DamageEnergyType.Holy, DamageAlignment.Good, AlignmentComponent.Evil,
                                                                                                       Helpers.CreateContextDiceValue(DiceType.D6, Common.createSimpleContextValue(1)))
                                                             );

            var minor_buff = Helpers.CreateBuff("WarpriestGoodMinorBuff",
                                                enchantment.Name,
                                                "At 1st level, you can touch one weapon and bless it with the power of purity and goodness. For 1 minute, this weapon glows green, white, or yellow-gold and deals an additional 1d6 points of damage against evil creatures. During this time, it’s treated as good for the purposes of overcoming damage reduction.",
                                                "",
                                                holy_weapon.Icon,
                                                null,
                                                Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), false, true, enchantment));

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestGoodBlessingMinorAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      Helpers.savingThrowNone,
                                                      weapon_bond.GetComponent<AbilitySpawnFx>(),
                                                      Helpers.CreateRunActions(apply_minor_buff)
                                                      );

            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic("Good", minor_ability, quicken: true);

            string[] summon_monster_guids = new string[] {"efa433a38e9c7c14bb4e780f8a3fe559", "0964bf88b582bed41b74e79596c4f6d9", "02de4dd8add69aa42a3d1330b573e2ab",
                                                      "2920d48574933c24391fbb9e18f87bf5", "eb6df7ddfc0669d4fb3fc9af4bd34bca", "e96593e67d206ab49ad1b567327d1e75" };

            var summon_m9 = library.Get<BlueprintAbility>("e96593e67d206ab49ad1b567327d1e75");

            List<ActionList> summon_actions = new List<ActionList>();
            foreach (var s in summon_monster_guids)
            {
                summon_actions.Add(library.Get<BlueprintAbility>(s).GetComponent<AbilityEffectRunAction>().Actions);
            }

            var major_ability = library.CopyAndAdd<BlueprintAbility>(summon_m9.AssetGuid, "WarpirestGoodBlessingMajorAbility", "");
            Common.unsetAsFullRoundAction(major_ability);
            major_ability.SetName("Battle Companion (Good)");
            major_ability.SetDescription("At 10th level, you can summon a battle companion. This ability functions as summon monster IV with a duration of 1 minute. This ability can summon only one creature, regardless of the list used. For every 2 levels beyond 10th, the level of the summon monster spell increases by 1 (to a maximum of summon monster IX at 20th level).");
            major_ability.RemoveComponents<SpellComponent>();
            major_ability.Type = AbilityType.Supernatural;
            var action = Helpers.CreateRunActions(Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus), summon_actions.ToArray()));
            major_ability.ReplaceComponent<AbilityEffectRunAction>(action);
            major_ability.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(min: 10, max: 10));
            major_ability.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.SummClassLevelWithArchetype, classes: getBlessingUsersArray(), archetype: Archetypes.DivineTracker.archetype,
                                                                       progression: ContextRankProgression.DelayedStartPlusDivStep,
                                                                       startLevel: 10, stepLevel: 2, max: 6, type: AbilityRankType.StatBonus
                                                                      )
                                      );
            major_ability.LocalizedDuration = Helpers.oneMinuteDuration;
            major_ability.Parent = null;
            addBlessingResourceLogic("Good", major_ability, quicken: true);
            addBlessing("WarpriestBlessingGood", "Good", minor_ability, major_ability, "882521af8012fc749930b03dc18a69de");
        }


        static void createHealingBlessing()
        {
            var healing_domain_feature = library.Get<BlueprintFeature>("b9ea4eb16ded8b146868540e711f81c8");
            var touch_of_good = library.Get<BlueprintAbility>("18f734e40dd7966438ab32086c3574e1");
            var healing_metamagic = Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicOnSpellDescriptor>(m =>
                                                                                            {
                                                                                                m.amount = 1;
                                                                                                m.Metamagic = Kingmaker.UnitLogic.Abilities.Metamagic.Empower;
                                                                                                m.resource = warpriest_blessing_resource;
                                                                                                m.spell_descriptor = SpellDescriptor.RestoreHP | SpellDescriptor.Cure;
                                                                                                m.cost_reducing_facts = new BlueprintUnitFact[] { warpriest_aspect_of_war_buff };
                                                                                            });

            var minor_buff = Helpers.CreateBuff("WarpriestHealingMinorBuff",
                                    "Powerful Healer",
                                    "At 1st level, you can add power to a cure spell as you cast it. As a swift action, you can treat any cure spell as if it were empowered (as the Empower Spell feat), causing it to heal 50% more damage (or deal 50% more damage if used against undead). This ability doesn’t stack with itself or the Empower Spell feat.",
                                     "",
                                     healing_domain_feature.Icon,
                                     null,
                                     healing_metamagic);

            var minor_activatable_ability = Helpers.CreateActivatableAbility("WarpriestHealingMinorBlessingActivatableAbility",
                                                                             minor_buff.Name,
                                                                             minor_buff.Description,
                                                                             "",
                                                                             minor_buff.Icon,
                                                                             minor_buff,
                                                                             AbilityActivationType.Immediately,
                                                                             CommandType.Free,
                                                                             null,
                                                                             Helpers.CreateActivatableResourceLogic(warpriest_blessing_resource, ResourceSpendType.Never)
                                                                             );
            minor_activatable_ability.DeactivateImmediately = true;
            if (!test_mode)
            {
                Helpers.SetField(minor_activatable_ability, "m_ActivateWithUnitCommand", CommandType.Swift);
                minor_activatable_ability.AddComponent(Common.createActivatableAbilityUnitCommand(CommandType.Swift));
            }
            minor_activatable_ability.DeactivateImmediately = true;

            var major_buff = Helpers.CreateBuff("WarpriestHealingMajorBuff",
                                                "Fast Healing",
                                                "At 10th level, you can touch an ally and grant it fast healing 3 for 1 minute.",
                                                 "",
                                                 touch_of_good.Icon,
                                                 null,
                                                 Helpers.Create<AddEffectFastHealing>(a => a.Heal = 3)
                                                 );

            var apply_major_buff = Common.createContextActionApplyBuff(major_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var major_ability = Helpers.CreateAbility("WarpriestHealingMajorBlessingAbility",
                                                      major_buff.Name,
                                                      major_buff.Description,
                                                      "",
                                                      touch_of_good.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_major_buff),
                                                      touch_of_good.GetComponent<AbilitySpawnFx>());
            major_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic("Healing", major_ability, quicken: true);

            addBlessing("WarpriestBlessingHealing", "Healing",
                        Common.ActivatableAbilityToFeature(minor_activatable_ability, false),
                        Common.AbilityToFeature(major_ability, false),
                        "73ae164c388990c43ade94cfe8ed5755"
                        );
        }


        static void createKnowledgeBlessing()
        {
            var aid = library.Get<BlueprintAbility>("03a9630394d10164a9410882d31572f0");
            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");

            var lore_action = Helpers.Create<NewMechanics.MonsterLore.ContextMonsterLoreCheckUsingClassAndStat>(c =>
                                                                                                               {
                                                                                                                   c.bonus = 15;
                                                                                                                   c.value = Helpers.CreateContextValue(AbilitySharedValue.StatBonus);
                                                                                                               }
                                                                                                               );

            var minor_ability_resolve = Helpers.CreateAbility("WarpriestKnowledgeMinorBlessingResolveAbility",
                                                              "Lore Keeper",
                                                              "At 1st level, you can touch a creature to learn about its abilities and weaknesses. With a successful touch attack, you gain information as if your result on the appropriate Knowledge skill check were equal to 15 + your warpriest level + your Wisdom modifier.",
                                                              "",
                                                              aid.Icon,
                                                              AbilityType.Supernatural,
                                                              CommandType.Standard,
                                                              AbilityRange.Touch,
                                                              "",
                                                              Helpers.savingThrowNone,
                                                              Helpers.CreateDeliverTouch(),
                                                              Helpers.CreateRunActions(lore_action),
                                                              Helpers.CreateCalculateSharedValue(Helpers.CreateContextDiceValue(DiceType.One, Helpers.CreateContextValue(AbilityRankType.StatBonus),
                                                                                                 Helpers.CreateContextValue(AbilityRankType.Default)), AbilitySharedValue.StatBonus),
                                                              Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, stat: StatType.Wisdom, type: AbilityRankType.StatBonus),
                                                              Helpers.CreateContextRankConfig(ContextRankBaseValueType.SummClassLevelWithArchetype, classes: getBlessingUsersArray(),
                                                                                              archetype: Archetypes.DivineTracker.archetype),
                                                              Helpers.Create<NewMechanics.MonsterLore.AbilityTargetCanBeInspected>(),
                                                              aid.GetComponent<AbilitySpawnFx>()
                                                              );
            minor_ability_resolve.setMiscAbilityParametersTouchHarmful();

            var minor_ability = Helpers.CreateAbility("WarpriestKnowledgeMinorBlessingAbility",
                                                            minor_ability_resolve.Name,
                                                            minor_ability_resolve.Description,
                                                            "",
                                                            minor_ability_resolve.Icon,
                                                            AbilityType.Supernatural,
                                                            CommandType.Standard,
                                                            AbilityRange.Touch,
                                                            "",
                                                            Helpers.savingThrowNone,
                                                            Helpers.CreateStickyTouch(minor_ability_resolve),
                                                            Helpers.Create<NewMechanics.MonsterLore.AbilityTargetCanBeInspected>(),
                                                            aid.GetComponent<AbilitySpawnFx>()
                                                           );
            minor_ability.setMiscAbilityParametersTouchHarmful();
            addBlessingResourceLogic("Knowledge", minor_ability, quicken: true);
            var major_target_buff = Helpers.CreateBuff("WarpriestKnowledgBlessingeMajorTargetBuff",
                                                "Monster Lore",
                                                "At 10th level, you can as a swift action gain a +2 insight bonus on attacks, saving throws, as well as to your AC against the creature that was previously successfully inspected by you or your allies. This effect lasts for 1 minute.",
                                                "",
                                                detect_magic.Icon,
                                                null,
                                                Helpers.Create<AttackBonusAgainstTarget>(c => { c.Value = Common.createSimpleContextValue(2); c.CheckCaster = true; }),
                                                Helpers.Create<ACBonusAgainstTarget>(c => { c.Value = Common.createSimpleContextValue(2); c.CheckCaster = true; c.Descriptor = ModifierDescriptor.Insight; })
                                                );

            var major_caster_buff = Helpers.CreateBuff("WarpriestKnowledgBlessingeMajorCasterBuff",
                                    "",
                                    "",
                                    "",
                                    detect_magic.Icon,
                                    null,
                                    Helpers.Create<NewMechanics.SavingThrowBonusAgainstFactFromCaster>(c =>
                                                                                                       {
                                                                                                           c.Value = Common.createSimpleContextValue(2);
                                                                                                           c.CheckedFact = major_target_buff;
                                                                                                           c.Descriptor = ModifierDescriptor.Insight;
                                                                                                       })
                                    );
            major_caster_buff.Stacking = StackingType.Stack;
            major_caster_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var apply_target_buff = Common.createContextActionApplyBuff(major_target_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: true);
            var apply_caster_buff = Common.createContextActionApplyBuff(major_caster_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: true);
            var major_ability = Helpers.CreateAbility("WarpriestKnowledgeBlessingMajorAbility",
                                                      major_target_buff.Name,
                                                      major_target_buff.Description,
                                                      "",
                                                      major_target_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Swift,
                                                      AbilityRange.Close,
                                                      Helpers.oneMinuteDuration,
                                                      Helpers.savingThrowNone,
                                                      aid.GetComponent<AbilitySpawnFx>(),
                                                      Helpers.CreateRunActions(apply_target_buff, Helpers.Create<ContextActionOnContextCaster>(c => c.Actions = Helpers.CreateActionList(apply_caster_buff))),
                                                      Helpers.Create<NewMechanics.MonsterLore.AbilityTargetInspected>()
                                                      );
            major_ability.setMiscAbilityParametersTouchHarmful();
            addBlessingResourceLogic("Knowledge", major_ability);
            addBlessing("WarpriestBlessingKnowledge", "Knowledge", minor_ability, major_ability, "443d44b3e0ea84046a9bf304c82a0425");
        }


        static void createLawBlessing()
        {
            var weapon_bond = library.Get<BlueprintAbility>("7ff088ab58c69854b82ea95c2b0e35b4");
            var axiomatic_weapon = library.Get<BlueprintActivatableAbility>("d76e8a80ab14ac942b6a9b8aaa5860b1");
            var bless_weapon = library.Get<BlueprintAbility>("831e942864e924846a30d2e0678e438b");
            var axiomatic_enchantment = library.Get<BlueprintWeaponEnchantment>("0ca43051edefcad4b9b2240aa36dc8d4");

            var enchantment = Common.createWeaponEnchantment("WarpriestLawMinorBlessingWeaponEcnchantment",
                                                             "Axiomatic Strike",
                                                             "This weapon glows blue, pale yellow, or white and deals an additional 1d6 points of damage against chaotic creatures. During this time, it’s treated as lawful for the purposes of overcoming damage reduction.",
                                                             "",
                                                             "",
                                                             "",
                                                             0,
                                                             axiomatic_enchantment.WeaponFxPrefab,
                                                             Common.createWeaponDamageAgainstAlignment(DamageEnergyType.Holy, DamageAlignment.Lawful, AlignmentComponent.Chaotic,
                                                                                                       Helpers.CreateContextDiceValue(DiceType.D6, Common.createSimpleContextValue(1)))
                                                             );

            var minor_buff = Helpers.CreateBuff("WarpriestLawMinorBuff",
                                                enchantment.Name,
                                                "At 1st level, you can touch one weapon and enhance it with the essence of law. For 1 minute, this weapon glows blue, pale yellow, or white and deals an additional 1d6 points of damage against chaotic creatures. During this time, it’s treated as lawful for the purposes of overcoming damage reduction.",
                                                "",
                                                axiomatic_weapon.Icon,
                                                null,
                                                Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), false, true, enchantment));

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestLawBlessingMinorAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      Helpers.savingThrowNone,
                                                      weapon_bond.GetComponent<AbilitySpawnFx>(),
                                                      Helpers.CreateRunActions(apply_minor_buff)
                                                      );

            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic("Law", minor_ability, quicken: true);

            string[] summon_monster_guids = new string[] {"efa433a38e9c7c14bb4e780f8a3fe559", "0964bf88b582bed41b74e79596c4f6d9", "02de4dd8add69aa42a3d1330b573e2ab",
                                                      "2920d48574933c24391fbb9e18f87bf5", "eb6df7ddfc0669d4fb3fc9af4bd34bca", "e96593e67d206ab49ad1b567327d1e75" };

            var summon_m9 = library.Get<BlueprintAbility>("e96593e67d206ab49ad1b567327d1e75");

            List<ActionList> summon_actions = new List<ActionList>();
            foreach (var s in summon_monster_guids)
            {
                summon_actions.Add(library.Get<BlueprintAbility>(s).GetComponent<AbilityEffectRunAction>().Actions);
            }

            var major_ability = library.CopyAndAdd<BlueprintAbility>(summon_m9.AssetGuid, "WarpirestLawBlessingMajorAbility", "");
            Common.unsetAsFullRoundAction(major_ability);
            major_ability.SetName("Battle Companion (Law)");
            major_ability.SetDescription("At 10th level, you can summon a battle companion. This ability functions as summon monster IV with a duration of 1 minute. This ability can summon only one creature, regardless of the list used. For every 2 levels beyond 10th, the level of the summon monster spell increases by 1 (to a maximum of summon monster IX at 20th level).");
            major_ability.RemoveComponents<SpellComponent>();
            major_ability.Type = AbilityType.Supernatural;
            var action = Helpers.CreateRunActions(Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus), summon_actions.ToArray()));
            major_ability.ReplaceComponent<AbilityEffectRunAction>(action);
            major_ability.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(min: 10, max: 10));
            major_ability.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.SummClassLevelWithArchetype, classes: getBlessingUsersArray(), archetype: Archetypes.DivineTracker.archetype, 
                                                                       progression: ContextRankProgression.DelayedStartPlusDivStep,
                                                                       startLevel: 10, stepLevel: 2, max: 6, type: AbilityRankType.StatBonus
                                                                      )
                                      );
            major_ability.LocalizedDuration = Helpers.oneMinuteDuration;
            major_ability.Parent = null;
            addBlessingResourceLogic("Law", major_ability, quicken: true);
            addBlessing("WarpriestBlessingLaw", "Law", minor_ability, major_ability, "092714336606cfc45a37d2ab39fabfa8");
        }


        static void createLiberationBlessing()
        {
            var freedom_of_movement_buff = library.Get<BlueprintBuff>("1533e782fca42b84ea370fc1dcbf4fc1");
            var buff = library.CopyAndAdd<BlueprintBuff>("60906dd9e4ddec14c8ac9a0f4e47f54c", "WarpriestLiberationBlessingBuff", ""); //freedom of movement no strings
            var spend_resource = Common.createContextActionSpendResource(warpriest_blessing_resource, 1, warpriest_aspect_of_war_buff);
            var spend_resource_caster = Helpers.CreateConditional(new Condition[] { Common.createContextConditionIsCaster(), Common.createContextConditionCasterHasFact(warpriest_aspect_of_war_buff, false) },
                                                                  spend_resource);
            buff.AddComponent(Helpers.CreateAddFactContextActions(newRound: spend_resource_caster));
            buff.FxOnStart = freedom_of_movement_buff.FxOnStart;
            buff.SetIcon(null);

            var minor_activatable_ability = Helpers.CreateActivatableAbility("WarpriestLiberationBlessingMinorActivatableAbility",
                                                                            "Liberation",
                                                                            "At 1st level, for 1 round as a swift action, you can ignore impediments to your mobility and effects that cause paralysis (as freedom of movement).",
                                                                             "",
                                                                             freedom_of_movement_buff.Icon,
                                                                             buff,
                                                                             AbilityActivationType.Immediately,
                                                                             CommandType.Free,
                                                                             null,
                                                                             Helpers.CreateActivatableResourceLogic(warpriest_blessing_resource, ResourceSpendType.Never)
                                                                             );
            if (!test_mode)
            {
                Helpers.SetField(minor_activatable_ability, "m_ActivateWithUnitCommand", CommandType.Swift);
                minor_activatable_ability.AddComponent(Common.createActivatableAbilityUnitCommand(CommandType.Swift));
            }

            var major_area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("cd23b709497500142b59802d7bc85edc", "WarpriestLiberationMajorBlessingArea", "");
            major_area.ReplaceComponent<AbilityAreaEffectBuff>(c => c.Buff = buff);

            var major_buff = library.CopyAndAdd<BlueprintBuff>("aa561f70d2260524e82c794d6140677c", "WarpriestMajorBlessingBuff", "");
            major_buff.SetName("Freedom’s Shout");
            major_buff.SetDescription("At 10th level, as a swift action you can emit a 30-foot aura that affects all allies with the liberation blessing");
            major_buff.ReplaceComponent<AddAreaEffect>(a => a.AreaEffect = major_area);
            var major_activatable_ability = Helpers.CreateActivatableAbility("WarpriestLiberationBlessingMajorActivatableAbility",
                                                                             major_buff.Name,
                                                                             major_buff.Description,
                                                                             "",
                                                                             major_buff.Icon,
                                                                             major_buff,
                                                                             AbilityActivationType.Immediately,
                                                                             CommandType.Free,
                                                                             null,
                                                                             Helpers.CreateActivatableResourceLogic(warpriest_blessing_resource, ResourceSpendType.Never)
                                                                             );
            if (!test_mode)
            {
                Helpers.SetField(major_activatable_ability, "m_ActivateWithUnitCommand", CommandType.Swift);
                major_activatable_ability.AddComponent(Common.createActivatableAbilityUnitCommand(CommandType.Swift));
            }

            addBlessing("WarpriestBlessingLiberation", "Liberation",
                        Common.ActivatableAbilityToFeature(minor_activatable_ability, false),
                        Common.ActivatableAbilityToFeature(major_activatable_ability, false),
                        "801ca88338451a546bca2ee59da87c53"
                        );
        }


        static void createLuckBlessing()
        {
            var minor_ability = library.CopyAndAdd<BlueprintAbility>("9af0b584f6f754045a0a79293d100ab3", "WarpriestLuckBlessingMinorAbility", "");
            minor_ability.RemoveComponents<ReplaceAbilitiesStat>();
            minor_ability.RemoveComponents<AbilityResourceLogic>();
            minor_ability.SetName("Lucky Presence");
            minor_ability.SetDescription("You can touch a willing creature as a standard action, giving it a bit of luck. For the next round, any time the target rolls a d20, he may roll twice and take the more favorable result.");
            addBlessingResourceLogic("Luck", minor_ability, quicken: true);

            var major_ability = library.CopyAndAdd<BlueprintAbility>("0e0668a703fbfcf499d9aa9d918b71ea", "WarpriestLuckBlessingMajorAbility", ""); //divine fortune
            major_ability.SetDescription("At 10th level, you can call on your deity to give you unnatural luck. This ability functions like Lucky Presence, but it affects you and lasts for a number of rounds equal to 1/2 your warpriest level.");
            major_ability.ReplaceComponent<ContextRankConfig>(c =>
                                                              {
                                                                  Helpers.SetField(c, "m_Class", getBlessingUsersArray());
                                                                  Helpers.SetField(c, "Archetype", Archetypes.DivineTracker.archetype);
                                                                  Helpers.SetField(c, "m_BaseValueType", ContextRankBaseValueType.SummClassLevelWithArchetype);
                                                              }
                                                              );
            major_ability.RemoveComponents<AbilityResourceLogic>();
            addBlessingResourceLogic("Luck", major_ability, quicken: true);

            addBlessing("WarpriestBlessingLuck", "Luck", minor_ability, major_ability, "d4e192475bb1a1045859c7664addd461");
        }


        static void createMadnessBlessing()
        {
            var minor_buff = library.CopyAndAdd<BlueprintBuff>("886c7407dc629dc499b9f1465ff382df", "WarpriestMadnessBlessingMinorBuff", ""); //confusion
            var confusion = library.Get<BlueprintAbility>("cf6c901fb7acc904e85c63b342e9c949");
            minor_buff.SetName("Madness Supremacy");
            minor_buff.SetDescription("At 1st level, as a swift action you can target a creature within 30 feet that has frightened or paralyzed condition. That condition is suspended for 1 round, and the chosen creature gains the confused condition instead. The confused creature rerolls any result other than “attack self” or “attack nearest creature.” The round spent confused counts toward the duration of the suspended effect. At the end of the confused round, the suspended condition resumes.");
            minor_buff.AddComponent(Common.createAddConditionImmunity(UnitCondition.Frightened));
            minor_buff.AddComponent(Common.createAddConditionImmunity(UnitCondition.Paralyzed));
            minor_buff.AddComponent(Helpers.Create<ConfusionControl.ControlConfusionBuff>(c => c.allowed_states = new ConfusionState[] { ConfusionState.AttackNearest, ConfusionState.SelfHarm }));

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Rounds), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestMadnessBlessingMinorAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Swift,
                                                      AbilityRange.Close,
                                                      Helpers.oneRoundDuration,
                                                      Helpers.savingThrowNone,
                                                      Helpers.CreateRunActions(apply_minor_buff),
                                                      confusion.GetComponent<AbilitySpawnFx>(),
                                                      confusion.GetComponent<SpellDescriptorComponent>(),
                                                      Common.createAbilityTargetCompositeOr(false, Common.createAbilityTargetHasCondition(UnitCondition.Paralyzed),
                                                                                                   Common.createAbilityTargetHasCondition(UnitCondition.Frightened)
                                                                                            )
                                                     );
            minor_ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            addBlessingResourceLogic("Madness", minor_ability);
            ConfusionState[] confusion_states = { ConfusionState.AttackNearest, ConfusionState.SelfHarm, ConfusionState.ActNormally, ConfusionState.DoNothing };
            string[] names = new string[] { "Attack Nearest", "Attack Self", "Act Normally", "Do Nothing" };

            BlueprintActivatableAbility[] major_abilities = new BlueprintActivatableAbility[confusion_states.Length];
            var harm = library.Get<BlueprintAbility>("137af566f68fd9b428e2e12da43c1482");
            var spend_resource = Common.createContextActionSpendResource(warpriest_blessing_resource, 1, warpriest_aspect_of_war_buff);
            var spend_resource_caster = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(warpriest_aspect_of_war_buff, false),
                                                                  spend_resource);

            for (int i = 0; i < confusion_states.Length; i++)
            {

                var buff = Helpers.CreateBuff($"WarpriestMadnessBlessingMajor{i + 1}Buff",
                                              $"Control Madness ({names[i]})",
                                              "At 10th level, as a swift action you can choose one behavior for all confused creatures within 30 feet to exhibit (as if all creatures rolled the same result). This effect lasts for 1 round. You can use this ability even while you are confused.",
                                              "",
                                              harm.Icon,
                                              null,
                                              Helpers.Create<ConfusionControl.ControlConfusionBuff>(c => c.allowed_states = new ConfusionState[] { confusion_states[i] })
                                              );


                var area_effect = library.CopyAndAdd<BlueprintAbilityAreaEffect>("4a15b95f8e173dc4fb56924fe5598dcf", $"WarpriestMadnessBlessingMajor{i + 1}Area", "");//dirge of doom area
                area_effect.ReplaceComponent<AbilityAreaEffectBuff>(a =>
                                                                    {
                                                                        a.Buff = buff;
                                                                        a.Condition = Helpers.CreateConditionsCheckerOr();
                                                                    }
                                                                    );
                var caster_buff = Helpers.CreateBuff($"WarpriestMadnessBlessingMajor{i + 1}CasterBuff",
                                                     buff.Name,
                                                     buff.Description,
                                                     "",
                                                     buff.Icon,
                                                     null,
                                                     Common.createAddAreaEffect(area_effect),
                                                     Helpers.CreateAddFactContextActions(newRound: spend_resource_caster)
                                                     );
                caster_buff.SetBuffFlags(BuffFlags.HiddenInUi);
                major_abilities[i] = Helpers.CreateActivatableAbility($"WarpriestMadnessBlessingMajor{i + 1}Ability",
                                                                     buff.Name,
                                                                     buff.Description,
                                                                     "",
                                                                     buff.Icon,
                                                                     caster_buff,
                                                                     AbilityActivationType.Immediately,
                                                                     CommandType.Free,
                                                                     null,
                                                                     Helpers.CreateActivatableResourceLogic(warpriest_blessing_resource, ResourceSpendType.Never)
                                                                     );
                major_abilities[i].Group = confusion_control_group;
                if (!test_mode)
                {
                    Helpers.SetField(major_abilities[i], "m_ActivateWithUnitCommand", CommandType.Swift);
                    major_abilities[i].AddComponent(Common.createActivatableAbilityUnitCommand(CommandType.Swift));
                }
            }


            addBlessing("WarpriestBlessingMadness", "Madness",
                        Common.AbilityToFeature(minor_ability, false),
                        Helpers.CreateFeature("WarpriestBlessingMadness",
                                              "Control Madness",
                                              major_abilities[0].Description,
                                              "",
                                              major_abilities[0].Icon,
                                              FeatureGroup.None,
                                              Helpers.CreateAddFacts(major_abilities)
                                              ),
                        "c346bcc77a6613040b3aa915b1ceddec");
        }


        static void createMagicBlessing()
        {
            var minor_ability = library.CopyAndAdd<BlueprintAbility>("8e40da3ef31245d468de08394504920b", "WarpriestMagicBlessingMinorAbility", "");
            minor_ability.SetDescription("At 1st level, you can cause your melee weapon to fly from your grasp and strike an opponent, then instantly return to you. You can make a single attack using a melee weapon at a range of 30 feet. This attack is treated as a ranged attack with a thrown weapon, except that you add your Wisdom modifier to the attack roll instead of your Dexterity modifier (you still add your Strength modifier to the damage roll as normal). This ability cannot be used to perform a combat maneuver.");

            minor_ability.RemoveComponents<AbilityResourceLogic>();
            addBlessingResourceLogic("Magic", minor_ability, quicken: true);

            BlueprintFeature[] blessed_magic_features = new BlueprintFeature[3];
            string name = "Blessed Magic";
            string description = "At 10th level, you can restore used warpriest spell slot that is at least 3 spell levels lower than the highest warpriest spell level you can cast (2 levels lower for divine tracker).";
            var bond_object = library.Get<BlueprintAbility>("e5dcf71e02e08fc448d9745653845df1");
            for (int i = 0; i < blessed_magic_features.Length; i++)
            {
                var ability = library.CopyAndAdd<BlueprintAbility>(bond_object.AssetGuid, $"WarpriestMagicBlessingMajor{i + 1}Ability", "");
                ability.SetName(name);
                ability.SetDescription(description);
                ability.RemoveComponents<AbilityResourceLogic>();
                ability.ReplaceComponent<AbilityRestoreSpellSlot>(a => { a.AnySpellLevel = false; a.SpellLevel = i + 1; });
                addBlessingResourceLogic("Magic", ability);
                var add_ability = Common.AbilityToFeature(ability);

                var feature = Helpers.CreateFeature($"WarpriestMagicBlessingMajor{i + 1}Feature",
                                                    "",
                                                    "",
                                                    "",
                                                    null,
                                                    FeatureGroup.None,
                                                    Helpers.CreateAddFeatureOnClassLevel(add_ability, 10 + i * 3, getWarpriestArray(), null)
                                                    );
                blessed_magic_features[i] = add_ability;
            }

            var major_feature = Helpers.CreateFeature($"WarpriestMagicBlessingMajorFeature",
                                                    name,
                                                    description,
                                                    "",
                                                    bond_object.Icon,
                                                    FeatureGroup.None,
                                                    Helpers.Create<LevelUpMechanics.AddFeatureOnClassLevelRange>(a =>
                                                                                                                 { a.Feature = blessed_magic_features[0];
                                                                                                                   a.classes = getBlessingUsersArray();
                                                                                                                   a.archetypes = getBlessingUsersArchetypesArray();
                                                                                                                   a.min_level = 10;
                                                                                                                   a.max_level = 12;
                                                                                                                 }
                                                                                                                 ),
                                                    Helpers.Create<LevelUpMechanics.AddFeatureOnClassLevelRange>(a =>
                                                                                                                {
                                                                                                                    a.Feature = blessed_magic_features[1];
                                                                                                                    a.classes = getWarpriestArray();
                                                                                                                    a.min_level = 13;
                                                                                                                    a.max_level = 15;
                                                                                                                }
                                                                                                                ),
                                                    Helpers.Create<LevelUpMechanics.AddFeatureOnClassLevelRange>(a =>
                                                                                                                {
                                                                                                                    a.Feature = blessed_magic_features[1];
                                                                                                                    a.classes = getBlessingUsersArray().Skip(1).ToArray();
                                                                                                                    a.archetypes = getBlessingUsersArchetypesArray();
                                                                                                                    a.min_level = 13;
                                                                                                                }
                                                                                                                ),
                                                    Helpers.Create<LevelUpMechanics.AddFeatureOnClassLevelRange>(a =>
                                                                                                                {
                                                                                                                    a.Feature = blessed_magic_features[2];
                                                                                                                    a.classes = getWarpriestArray();
                                                                                                                    a.min_level = 16;
                                                                                                                }
                                                                                                                )
                                                    );
            addBlessing("WarpriestBlessingMagic", "Magic", Common.AbilityToFeature(minor_ability, false), major_feature, "08a5686378a87b64399d329ba4ef71b8");
        }


        static void createNobilityBlessing()
        {
            var heroism = library.Get<BlueprintAbility>("5ab0d42fb68c9e34abae4921822b9d63");
            var minor_buff = library.Get<BlueprintBuff>("87ab2fed7feaaff47b62a3320a57ad8d"); //heroism

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestNobilityMinorBlessing",
                                                      "Inspiring Word",
                                                      "At 1st level, you can speak a few words to a creature within 30 feet that fill them with inspiration. You can grant that creature a +2 morale bonus on attack rolls, ability checks, skill checks, and saving throws. This effect lasts for 1 minute.",
                                                      "",
                                                      heroism.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Close,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_minor_buff),
                                                      heroism.GetComponent<AbilitySpawnFx>()
                                                      );
            minor_ability.setMiscAbilityParametersSingleTargetRangedFriendly(works_on_self: true);
            addBlessingResourceLogic("Nobility", minor_ability, quicken: true);

            var true_strike = library.Get<BlueprintAbility>("2c38da66e5a599347ac95b3294acbe00");

            var target_buff = Helpers.CreateBuff("WarpriestNobilityMajorBlessingTargetBuff",
                                                       "",
                                                       "",
                                                       "",
                                                       null,
                                                       null);
            var apply_buff = Common.createContextActionApplyBuff(target_buff,
                                                             Helpers.CreateContextDuration(Common.createSimpleContextValue(1)),
                                                             dispellable: false);

            var ally_buff = Helpers.CreateBuff("WarpriestNobilityMajorBlessingAllyBuff",
                                                     "",
                                                     "",
                                                     "",
                                                     null,
                                                     null,
                                                     Helpers.Create<NewMechanics.AttackBonusAgainstFactsOwner>(a =>
                                                     {
                                                         a.Bonus = Common.createSimpleContextValue(4);
                                                         a.attack_types = new AttackType[] { AttackType.Melee, AttackType.Touch };
                                                         a.Descriptor = ModifierDescriptor.Morale;
                                                         a.only_from_caster = true;
                                                         a.CheckedFacts = new BlueprintUnitFact[] { target_buff };
                                                     })
                                                     );
            ally_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var area_effect = library.CopyAndAdd<BlueprintAbilityAreaEffect>("8c5617838c1195443b584e005d0913eb", "WarpriestNobilityMajorBlessingArea", "");
            area_effect.ReplaceComponent<AbilityAreaEffectBuff>(a => a.Buff = ally_buff);
            var major_buff = Helpers.CreateBuff("WarpriestNobilityMajorBlessingCasterBuff",
                                                 "Lead by Example",
                                                 "At 10th level, you can inspire your allies to follow your lead. For one minute if you successfully hit an opponent, all allies within 30 feet will receive +4 morale bonus on attack rolls against that opponent.",
                                                 "",
                                                 true_strike.Icon,
                                                 null,
                                                 Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(apply_buff)),
                                                 Common.createAddAreaEffect(area_effect)
                                                 );

            var apply_major_buff = Common.createContextActionApplyBuff(major_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var major_ability = Helpers.CreateAbility("WarpriestNobilityMajorBlessingAbility",
                                                      major_buff.Name,
                                                      major_buff.Description,
                                                      "",
                                                      major_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Personal,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_major_buff),
                                                      true_strike.GetComponent<AbilitySpawnFx>()
                                                      );
            major_ability.setMiscAbilityParametersSelfOnly();
            addBlessingResourceLogic("Nobility", major_ability, quicken: true);

            addBlessing("WarpriestBlessingNobility", "Nobility", minor_ability, major_ability, "e0471d01e73254a4ca23278705b75e57");
        }


        static void createPlantBlessing()
        {
            var entangle_buff = library.Get<BlueprintBuff>("f7f6330726121cf4b90a6086b05d2e38");

            var spend_resource = Common.createContextActionSpendResource(warpriest_blessing_resource, 1, warpriest_aspect_of_war_buff);
            var apply_entangle_buff_saved = Common.createContextSavedApplyBuff(entangle_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Rounds), is_dispellable: false);

            var on_hit_action = Helpers.CreateActionList(spend_resource,
                                                         Common.createContextActionSavingThrow(SavingThrowType.Reflex, Helpers.CreateActionList(apply_entangle_buff_saved)));
            var minor_buff = Helpers.CreateBuff("WarpriestPlantMinorBlessingBuff",
                                                "Creeping Vines",
                                                "At 1st level, upon hitting with a melee attack, as a swift action you cause the creature you hit to sprout entangling vines that attempt to hold it in place, entangling it for 1 round (Reflex negates).",
                                                "",
                                                entangle_buff.Icon,
                                                null,
                                                Common.createAddInitiatorAttackWithWeaponTrigger(on_hit_action, check_weapon_range_type: true, only_first_hit: true),
                                                Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(getBlessingUsersArray(), getBlessingUsersArchetypesArray(), StatType.Wisdom)
                                                );

            var minor_activatable_ability = Helpers.CreateActivatableAbility("WarpriestPlantMinorBlessingActivatable",
                                                                 minor_buff.Name,
                                                                 minor_buff.Description,
                                                                 "",
                                                                 minor_buff.Icon,
                                                                 minor_buff,
                                                                 AbilityActivationType.Immediately,
                                                                 CommandType.Free,
                                                                 null,
                                                                 Helpers.CreateActivatableResourceLogic(warpriest_blessing_resource, ResourceSpendType.Never)
                                                                 );
            if (!test_mode)
            {
                Helpers.SetField(minor_activatable_ability, "m_ActivateWithUnitCommand", CommandType.Swift);
                minor_activatable_ability.AddComponent(Common.createActivatableAbilityUnitCommand(CommandType.Swift));
            }

            string[] nature_ally_guids = new string[] {"28ea1b2e0c4a9094da208b4c186f5e4f", "060afb9e13d8a3547ad0dd20c407c0a5",
                                                      "6d8d59aa38713be4fa3be76c19107cc0", "8d3d5b62878d5b24391c1d7834d0d706", "f6751c3b22dbd884093e350a37420368" };

            var summon_na8 = library.Get<BlueprintAbility>("8d3d5b62878d5b24391c1d7834d0d706");

            List<ActionList> summon_actions = new List<ActionList>();
            foreach (var s in nature_ally_guids)
            {
                summon_actions.Add(library.Get<BlueprintAbility>(s).GetComponent<AbilityEffectRunAction>().Actions);
            }

            var major_ability = library.CopyAndAdd<BlueprintAbility>(summon_na8.AssetGuid, "WarpirestPlantBlessingMajorAbility", "");
            Common.unsetAsFullRoundAction(major_ability);
            major_ability.SetName("Battle Companion (Plant)");
            major_ability.SetDescription("At 10th level, you can summon a battle companion. This ability functions as summon nature’s ally V with a duration of 1 minute. This ability can summon only one creature, regardless of the list used. For every 2 levels beyond 10th, the level of the summon nature’s ally spell increases by 1 (to a maximum of summon nature’s ally IX at 18th level).");
            major_ability.RemoveComponents<SpellComponent>();
            major_ability.Type = AbilityType.Supernatural;
            var action = Helpers.CreateRunActions(Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus), summon_actions.ToArray()));
            major_ability.ReplaceComponent<AbilityEffectRunAction>(action);
            major_ability.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(min: 10, max: 10));
            major_ability.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.SummClassLevelWithArchetype, classes: getBlessingUsersArray(), archetype: Archetypes.DivineTracker.archetype,
                                                                       progression: ContextRankProgression.DelayedStartPlusDivStep,
                                                                       startLevel: 10, stepLevel: 2, max: 5, type: AbilityRankType.StatBonus
                                                                      )
                                      );
            major_ability.LocalizedDuration = Helpers.oneMinuteDuration;
            major_ability.Parent = null;
            addBlessingResourceLogic("Plant", major_ability, quicken: true);
            addBlessing("WarpriestBlessingPalnt", "Plant", Common.ActivatableAbilityToFeature(minor_activatable_ability, false),
                                                            Common.AbilityToFeature(major_ability, false), "0e03c2a03222b0b42acf96096b286327");
        }


        static void createProtectionBlessing()
        {
            var protective_ward = library.Get<BlueprintFeature>("e2e9d41bfa7aa364592b9d57dd74c9db");
            var shiled = library.Get<BlueprintAbility>("ef768022b0785eb43a18969903c537c4");
            var minor_buff = Helpers.CreateBuff("WarpriestProtectionBlessingMinorBuff",
                                                "Increased Defense",
                                                "At 1st level, you can gain a +1 sacred bonus on saving throws and a +1 sacred bonus to AC for 1 minute. The bonus increases to +2 at 10th level and +3 at 20th level.",
                                                "",
                                                protective_ward.Icon,
                                                null,
                                                Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.Sacred, ContextValueType.Rank, AbilityRankType.StatBonus),
                                                Helpers.CreateAddContextStatBonus(StatType.SaveFortitude, ModifierDescriptor.Sacred, ContextValueType.Rank, AbilityRankType.StatBonus),
                                                Helpers.CreateAddContextStatBonus(StatType.SaveReflex, ModifierDescriptor.Sacred, ContextValueType.Rank, AbilityRankType.StatBonus),
                                                Helpers.CreateAddContextStatBonus(StatType.SaveWill, ModifierDescriptor.Sacred, ContextValueType.Rank, AbilityRankType.StatBonus),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.SummClassLevelWithArchetype, progression: ContextRankProgression.OnePlusDivStep,
                                                                                classes: getBlessingUsersArray(), archetype: Archetypes.DivineTracker.archetype, stepLevel: 10, type: AbilityRankType.StatBonus)
                                                );

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestProtectionBlessingMinorAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Personal,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      shiled.GetComponent<AbilitySpawnFx>(),
                                                      Helpers.CreateRunActions(apply_minor_buff)
                                                      );
            minor_ability.setMiscAbilityParametersSelfOnly();
            addBlessingResourceLogic("Protection", minor_ability, quicken: true);

            var protection_from_energy_communal = library.Get<BlueprintAbility>("76a629d019275b94184a1a8733cac45e");
            var protection_from_electricity_communal = library.Get<BlueprintAbility>("f10cd112b876a6f449d52dee0a57e602");

            var major_buff = Helpers.CreateBuff("WarpriestProtectionBlessingMajorBuff",
                                                 "Aura of Protection",
                                                 "At 10th level, you can emit a 30-foot aura of protection for 1 minute. You and your allies within this aura gain resistance 10 against acid, cold, electricity, fire, and sonic. At 15th level, the energy resistance increases to 20.",
                                                 "",
                                                 protection_from_energy_communal.Icon,
                                                 null,
                                                 Common.createEnergyDRContextRank(DamageEnergyType.Acid, AbilityRankType.StatBonus, 10),
                                                 Common.createEnergyDRContextRank(DamageEnergyType.Cold, AbilityRankType.StatBonus, 10),
                                                 Common.createEnergyDRContextRank(DamageEnergyType.Electricity, AbilityRankType.StatBonus, 10),
                                                 Common.createEnergyDRContextRank(DamageEnergyType.Fire, AbilityRankType.StatBonus, 10),
                                                 Common.createEnergyDRContextRank(DamageEnergyType.Sonic, AbilityRankType.StatBonus, 10),
                                                 Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.SummClassLevelWithArchetype, progression: ContextRankProgression.OnePlusDivStep,
                                                                                classes: getBlessingUsersArray(), archetype: Archetypes.DivineTracker.archetype, stepLevel: 15, type: AbilityRankType.StatBonus)
                                                );


            var apply_major_buff = Common.createContextActionApplyBuff(major_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var major_ability = Helpers.CreateAbility("WarpriestProtectionBlessingMajorAbility",
                                                      major_buff.Name,
                                                      major_buff.Description,
                                                      "",
                                                      major_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Personal,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_major_buff),
                                                      protection_from_electricity_communal.GetComponent<AbilitySpawnFx>(),
                                                      Helpers.CreateAbilityTargetsAround(30.Feet(), TargetType.Ally)
                                                      );

            major_ability.setMiscAbilityParametersSelfOnly();
            major_ability.CanTargetFriends = true;
            addBlessingResourceLogic("Protection", major_ability, quicken: true);

            addBlessing("WarpriestBlessingProtection", "Protection", minor_ability, major_ability, "d4ce7592bd12d63439907ad64e986e59");
        }


        static void createReposeBlessing()
        {
            var staggered_buff = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");
            var sleep_buff = library.Get<BlueprintBuff>("c9937d7846aa9ae46991e9f298be644a");
            var undead_type = library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");

            var effect = Helpers.CreateConditional(Common.createContextConditionHasFact(undead_type),
                                                   Common.createContextActionApplyBuff(staggered_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false),
                                                   Helpers.CreateConditional(Common.createContextConditionHasFact(staggered_buff),
                                                                             Common.createContextActionApplyBuff(sleep_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1)), dispellable: false),
                                                                             Common.createContextActionApplyBuff(staggered_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1)), dispellable: false)
                                                                             )
                                                  );

            var minor_ability_touch = library.CopyAndAdd<BlueprintAbility>("30dfb2e83f9de7246ad6cb44e36f2b4d", "WarpriestReposeBlessingMinorTouch", "");
            minor_ability_touch.Type = AbilityType.Supernatural;
            minor_ability_touch.RemoveComponents<AbilityResourceLogic>();
            minor_ability_touch.RemoveComponents<SpellComponent>();
            minor_ability_touch.ReplaceComponent<AbilityEffectRunAction>(c => c.Actions = Helpers.CreateActionList(effect));
            minor_ability_touch.SetDescription("At 1st level, you can fill a living creature with lethargy by hitting it with a melee touch attack, causing it to become staggered for 1 round. If the target is already staggered, it falls asleep for 1 round instead. An undead creature that’s touched is staggered for a number of rounds equal to your Wisdom modifier (minimum 1).");
            minor_ability_touch.SpellResistance = false;
            var minor_ability = Helpers.CreateAbility("WarpriestReposeBlessingMinorAbility",
                                                      minor_ability_touch.Name,
                                                      minor_ability_touch.Description,
                                                      "",
                                                      minor_ability_touch.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      "Variable",
                                                      Helpers.savingThrowNone,
                                                      Helpers.CreateStickyTouch(minor_ability_touch)
                                                      );
            minor_ability.setMiscAbilityParametersTouchHarmful();
            minor_ability.SpellResistance = false;
            addBlessingResourceLogic("Repose", minor_ability, quicken: true);


            var major_ability = library.CopyAndAdd<BlueprintAbility>(harm_undead.AssetGuid, "WarpriestReposeBlessingMajorAbility", "");
            major_ability.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_StepLevel", 6));
            major_ability.RemoveComponents<AbilityResourceLogic>();
            major_ability.ActionType = CommandType.Swift;
            major_ability.SetName("Back to the Grave");
            major_ability.SetDescription("At 10th level, when using channel energy to heal living creatures, you can take a swift action on that same turn to also deal damage to undead creatures (as your channel energy ability). Undead take an amount of damage equal to half the amount healed, and can attempt the normal saving throw to halve this damage.");
            addBlessingResourceLogic("Repose", major_ability);

            var major_ability_base = Common.createVariantWrapper("WarpriestReposeBlessingMajorBaseAbility", "", major_ability);

            var major_feature = Helpers.CreateFeature("WarpriestReposeBlessingMajorFeature",
                                                       major_ability.Name,
                                                       major_ability.Description,
                                                       "",
                                                       major_ability.Icon,
                                                       FeatureGroup.None,
                                                       Helpers.CreateAddFact(major_ability_base)
                                                       );


            var major_buff = Helpers.CreateBuff("WarpriestReposeBlessingMajorBuff",
                                                major_ability.Name,
                                                major_ability.Description,
                                                "",
                                                major_ability.Icon,
                                                null
                                                );

            var remove_buff = Helpers.Create<ContextActionOnContextCaster>(c =>
            {
                c.Actions = Helpers.CreateActionList(Common.createContextActionRemoveBuff(major_buff));
            }
            );
            major_ability.ReplaceComponent<AbilityEffectRunAction>(c => c.Actions = Helpers.CreateActionList(c.Actions.Actions.AddToArray(remove_buff)));
            major_ability.AddComponent(Common.createAbilityCasterHasFacts(major_buff));

            var caster_action = Helpers.Create<ContextActionOnContextCaster>(c =>
            {
                var apply_buff = Common.createContextActionApplyBuff(major_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1)), dispellable: false);

                c.Actions = Helpers.CreateActionList(Helpers.CreateConditional(Common.createContextConditionCasterHasFact(major_feature), apply_buff));
            }
            );
            heal_living.ReplaceComponent<AbilityEffectRunAction>(c => c.Actions = Helpers.CreateActionList(c.Actions.Actions.AddToArray(caster_action)));
            heal_living_extra.ReplaceComponent<AbilityEffectRunAction>(c => c.Actions = Helpers.CreateActionList(c.Actions.Actions.AddToArray(caster_action)));

            ChannelEnergyEngine.storeChannel(major_ability, major_feature, ChannelEnergyEngine.ChannelType.PositiveHarm | ChannelEnergyEngine.ChannelType.BackToTheGrave);
            ChannelEnergyEngine.updateItemsForChannelDerivative(harm_undead, major_ability);

            addBlessing("WarpriestBlessingRepose", "Repose", Common.AbilityToFeature(minor_ability, false), major_feature, "076ba1e3a05fac146acfc956a9f41e95");
        }

        static void createRuneBlessing()
        {
            BlueprintAbility[] runes = library.Get<BlueprintAbility>("56ad05dedfd9df84996f62108125eed5").GetComponent<AbilityVariants>().Variants; //from rune domain

            string description = "At 1st level, you can create a blast rune in any adjacent square. Any creature entering this square takes an amount of damage equal to 1d6 + 1/2 your warpriest level. This rune deals either acid, cold, electricity, or fire damage, designated when you create the rune. The rune is invisible, and lasts a number of rounds equal to your warpriest level or until discharged.";
            if (Main.settings.balance_fixes)
            {
                description = $"At 1st level, you can create a blast rune in any adjacent square. Any creature entering this square takes an amount of damage equal to 1d{BalanceFixes.getDamageDieString(DiceType.D6)} + 1d{BalanceFixes.getDamageDieString(DiceType.D6)} per two warpriest levels beyond first. This rune deals either acid, cold, electricity, or fire damage, designated when you create the rune. The rune is invisible, and lasts a number of rounds equal to your warpriest level or until discharged.";
            }
            var minor_ability = Helpers.CreateAbility("WarpriestRuneBlessingMinorAbility",
                                          "Blast Rune",
                                          description,
                                          "",
                                          library.Get<BlueprintAbility>("56ad05dedfd9df84996f62108125eed5").Icon, //rune domain base ability
                                          AbilityType.Supernatural,
                                          CommandType.Standard,
                                          AbilityRange.Close,
                                          "",
                                          Helpers.savingThrowNone);
            minor_ability.setMiscAbilityParametersRangedDirectional();
            List<BlueprintAbilityAreaEffect> areas = new List<BlueprintAbilityAreaEffect>();
            for (int i = 0; i < runes.Length; i++)
            {
                var rune = library.CopyAndAdd<BlueprintAbility>(runes[i].AssetGuid, $"WarpriestRuneBlessingMinor{i + 1}Ability", "");
                rune.SpellResistance = false;
                rune.Type = AbilityType.Supernatural;
                rune.RemoveComponents<AbilityResourceLogic>();
                var area_action = (rune.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionSpawnAreaEffect);
                var area = library.CopyAndAdd(area_action.AreaEffect, $"WarpriestRuneBlessingMinor{i + 1}Area", "");
                areas.Add(area);
                foreach (var c in rune.GetComponents<ContextRankConfig>().ToArray())
                {
                    var new_c = c.CreateCopy();
                    Helpers.SetField(new_c, "m_Class", getBlessingUsersArray());
                    Helpers.SetField(new_c, "m_BaseValueType", ContextRankBaseValueType.SummClassLevelWithArchetype);
                    Helpers.SetField(new_c, "Archetype", Archetypes.DivineTracker.archetype);
                    rune.ReplaceComponent(c, new_c);
                }
                rune.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(area_action.CreateCopy(a => a.AreaEffect = area)));
                area.RemoveComponents<ContextRankConfig>();
                area.AddComponent(rune.GetComponents<ContextRankConfig>().First());
                
                addBlessingResourceLogic("Rune", rune, quicken: true, parent: minor_ability);
                rune.SetDescription(description);
                runes[i] = rune;
            }

            for (int i = 0; i < runes.Length; i++)
            {
                foreach (var a in areas)
                {
                    library.Get<BlueprintAbility>("56ad05dedfd9df84996f62108125eed5").GetComponent<AbilityVariants>().Variants[i].AddComponent(Helpers.Create<NewMechanics.AbilityTargetPointDoesNotContainAreaEffect>(atp => atp.area_effect = a));
                    runes[i].AddComponent(Helpers.Create<NewMechanics.AbilityTargetPointDoesNotContainAreaEffect>(atp => atp.area_effect = a));
                }
            }

            minor_ability.AddComponent(minor_ability.CreateAbilityVariants(runes));

            var spell_combat = library.Get<BlueprintFeature>("2464ba53317c7fc4d88f383fac2b45f9");
            var major_feature = Helpers.CreateFeature("WarpriestRuneBlessingMajorFeature",
                                                "Spell Storing",
                                                "At 10th level, you can cast a single target non-personal spell of 3rd level or lower into a weapon that can be released on target upon successful attack.",
                                                "",
                                                spell_combat.Icon,
                                                FeatureGroup.None,
                                                Helpers.Create<SpellManipulationMechanics.FactStoreSpell>());

            var release_buff = Helpers.CreateBuff("WarpriestRuneBlessingMajorToggleBuff",
                                                  major_feature.Name + ": Release",
                                                  major_feature.Description,
                                                  "",
                                                  major_feature.Icon,
                                                  null,
                                                  Helpers.Create<SpellManipulationMechanics.AddStoredSpellToCaption>(a => a.store_fact = major_feature));

            var major_activatable_ability = Helpers.CreateActivatableAbility("WarpriestRuneBlessingMajorToggleAbility",
                                                                             major_feature.Name + ": Release",
                                                                             major_feature.Description,
                                                                             "",
                                                                             major_feature.Icon,
                                                                             release_buff,
                                                                             AbilityActivationType.Immediately,
                                                                             CommandType.Free,
                                                                             null,
                                                                             Helpers.Create<SpellManipulationMechanics.ActivatableAbilitySpellStoredInFactRestriction>(a => a.fact = major_feature));
            major_activatable_ability.DeactivateImmediately = true;

            var release_action = Helpers.Create<SpellManipulationMechanics.ReleaseSpellStoredInSpecifiedBuff>(r => r.fact = major_feature);
            var release_on_condition = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(release_buff), release_action);
            major_feature.AddComponent(Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(release_on_condition)));
            major_feature.AddComponent(Helpers.CreateAddFact(major_activatable_ability));

            int max_variants = 10; //due to ui limitation
            Predicate<AbilityData> check_slot_predicate = delegate (AbilityData spell)
            {
                return spell.SpellLevel <= 3
                        && (spell.Spellbook?.Blueprint == warpriest_class.Spellbook || spell.Spellbook?.Blueprint == Archetypes.DivineTracker.archetype.GetParentClass().Spellbook)
                        && spell.Blueprint.EffectOnEnemy == AbilityEffectOnUnit.Harmful
                        && spell.Blueprint.Range != AbilityRange.Personal
                        && spell.Blueprint.CanTargetEnemies
                        && !spell.Blueprint.CanTargetPoint
                        && !spell.Blueprint.IsFullRoundAction
                        && (!spell.Blueprint.HasVariants || spell.Variants.Count < max_variants)
                        && !spell.Blueprint.HasAreaEffect()
                        && (!spell.RequireMaterialComponent || spell.HasEnoughMaterialComponent);
            };

            for (int i = 0; i < max_variants; i++)
            {
                var major_ability = Helpers.CreateAbility($"WarpriestRuneBlessingMajor{i + 1}Ability",
                                                          major_feature.Name,
                                                          major_feature.Description,
                                                          "",
                                                          major_feature.Icon,
                                                          AbilityType.Supernatural,
                                                          CommandType.Standard,
                                                          AbilityRange.Personal,
                                                          "",
                                                          "",
                                                          Helpers.Create<SpellManipulationMechanics.AbilityStoreSpellInFact>(s => { s.fact = major_feature; s.check_slot_predicate = check_slot_predicate; s.variant = i; })
                                                          );
                major_ability.setMiscAbilityParametersSelfOnly();
                addBlessingResourceLogic("Rune", major_ability, quicken: true);
                major_feature.AddComponent(Helpers.CreateAddFact(major_ability));
            }

            addBlessing("WarpriestBlessingRune", "Rune", Common.AbilityToFeature(minor_ability, false), major_feature, "77637f81d6aa33b4f82873d7934e8c4b");
        }


        static void createStrengthBlessing()
        {
            var minor_buff = library.CopyAndAdd<BlueprintBuff>("94dfcf5f3a72ce8478c8de5db69e752b", "WarpriestStrengthBlessingMinorBuff", "");
            minor_buff.AddComponent(Common.createAbilityScoreCheckBonus(Helpers.CreateContextValue(AbilityRankType.Default), ModifierDescriptor.Enhancement, StatType.Strength));
            minor_buff.AddComponent(Helpers.CreateAddContextStatBonus(StatType.AdditionalCMB, ModifierDescriptor.Enhancement));

            minor_buff.ReplaceComponent<ContextRankConfig>(c => {
                                                                  Helpers.SetField(c, "m_Class", getBlessingUsersArray());
                                                                  Helpers.SetField(c, "Archetype", Archetypes.DivineTracker.archetype);
                                                                  Helpers.SetField(c, "m_BaseValueType", ContextRankBaseValueType.SummClassLevelWithArchetype);
                                                                }
                                                                );

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Rounds), dispellable: false);

            var minor_ability = Helpers.CreateAbility("WarpriestStrengthBlessingMinorAbility",
                                                      "Strength Surge",
                                                      "At 1st level, as a swift action you can focus your own strength. You gain an enhancement bonus equal to 1/2 your warpriest level (minimum +1) on melee attack rolls, combat maneuver checks that rely on Strength, Strength-based skills, and Strength checks for 1 round.",
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Swift,
                                                      AbilityRange.Personal,
                                                      Helpers.oneRoundDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_minor_buff)
                                                      );
            minor_ability.setMiscAbilityParametersSelfOnly();
            addBlessingResourceLogic("Strength", minor_ability);

            var greater_strength_feature = library.Get<BlueprintFeature>("3298fd30e221ef74189a06acbf376d29");
            var bull_strength = library.Get<BlueprintAbility>("4c3d08935262b6544ae97599b3a9556d");
            var major_buff = Helpers.CreateBuff("WarpriestStrengthBlessingMajorBuff",
                                                "Strength of Will",
                                                "At 10th level, as a swift action you can ignore the movement penalties caused by wearing medium or heavy armor or by carrying a medium or heavy load. This effect lasts for 1 minute. During this time, you can add your Strength modifier on saving throws against effects that would cause you to become entangled, staggered, or paralyzed.",
                                                "",
                                                greater_strength_feature.Icon,
                                                null,
                                                Helpers.CreateAddFact(remove_armor_speed_penalty_feature),
                                                Helpers.CreateAddFact(remove_armor_speed_penalty_feature),
                                                Common.createContextSavingThrowBonusAgainstDescriptor(Helpers.CreateContextValue(AbilityRankType.StatBonus),
                                                                                                      ModifierDescriptor.UntypedStackable,
                                                                                                      SpellDescriptor.Paralysis | SpellDescriptor.Staggered | SpellDescriptor.MovementImpairing),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, type: AbilityRankType.StatBonus,
                                                                                stat: StatType.Strength, min: 0)
                                                );

            var apply_major_buff = Common.createContextActionApplyBuff(major_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);

            var major_ability = Helpers.CreateAbility("WarpriestStrengthBlessingMajorAbility",
                                                      major_buff.Name,
                                                      major_buff.Description,
                                                      "",
                                                      greater_strength_feature.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Swift,
                                                      AbilityRange.Personal,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_major_buff),
                                                      bull_strength.GetComponent<AbilitySpawnFx>()
                                                      );

            major_ability.setMiscAbilityParametersSelfOnly();
            addBlessingResourceLogic("Strength", major_ability);

            addBlessing("WarpriestBlessingStrength", "Strength", minor_ability, major_ability, "58d2867520de17247ac6988a31f9e397");
        }


        static void createSunBlessing()
        {
            var blindness_buff = library.Get<BlueprintBuff>("187f88d96a0ef464280706b63635f2af");
            var dazzled_buff = library.Get<BlueprintBuff>("df6d1025da07524429afbae248845ecc");

            var apply_blindess_buff = Common.createContextActionApplyBuff(blindness_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Rounds), dispellable: false);
            var apply_dazzzled_buff = Common.createContextActionApplyBuff(dazzled_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Rounds), dispellable: false);

            var effect = Common.createContextActionSavingThrow(SavingThrowType.Reflex,
                                                               Helpers.CreateActionList(Helpers.CreateConditionalSaved(apply_dazzzled_buff, apply_blindess_buff))
                                                               );
            var flare = library.Get<BlueprintAbility>("f0f8e5b9808f44e4eadd22b138131d52");

            var minor_ability = Helpers.CreateAbility("WarpriestSunBlessingMinorAbility",
                                                      "Blinding Strike",
                                                      "At 1st level, you can create a flash of sunlight in the eyes of one of your opponents. The target is blinded for 1 round. If it succeeds at a Reflex saving throw, it’s instead dazzled for 1 round. This is a light effect. Sightless creatures are unaffected by this ability.",
                                                      "",
                                                      flare.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Close,
                                                      Helpers.oneRoundDuration,
                                                      "Reflex partial",
                                                      Helpers.CreateRunActions(effect),
                                                      flare.GetComponent<AbilitySpawnFx>(),
                                                      Helpers.CreateSpellDescriptor(SpellDescriptor.Blindness | SpellDescriptor.SightBased),
                                                      Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(getBlessingUsersArray(), getBlessingUsersArchetypesArray(), StatType.Wisdom)
                                                      );

            addBlessingResourceLogic("Sun", minor_ability);
            minor_ability.setMiscAbilityParametersSingleTargetRangedHarmful(test_mode);
            minor_ability.SpellResistance = false;

            var weapon_bond = library.Get<BlueprintAbility>("7ff088ab58c69854b82ea95c2b0e35b4");
            var bless_weapon = library.Get<BlueprintAbility>("831e942864e924846a30d2e0678e438b");
            var bane_undead = library.Get<BlueprintWeaponEnchantment>("eebb4d3f20b8caa43af1fed8f2773328");
            var flaming = library.Get<BlueprintWeaponEnchantment>("30f90becaaac51f41bf56641966c4121");

            BlueprintWeaponEnchantment[][] enchant_lists = new BlueprintWeaponEnchantment[][] { new BlueprintWeaponEnchantment[1]{flaming},
                                                                                              new BlueprintWeaponEnchantment[1]{bane_undead},
                                                                                              new BlueprintWeaponEnchantment[2]{flaming, bane_undead},
                                                                                            };
            List<BlueprintAbility> major_abilities = new List<BlueprintAbility>();
            string name = "Cleansing Fire";
            string description = "At 10th level, you can touch a weapon and grant it either the flaming or undead–bane weapon special ability for 1 minute. If you spend two uses of your blessing when activating this ability, the weapon can have both weapon special abilities.";

            var major_ability = Helpers.CreateAbility("WarpriestSunBlessingMajorAbility",
                                                      name,
                                                      description,
                                                      "",
                                                      bless_weapon.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      "");
            major_ability.setMiscAbilityParametersTouchFriendly();
            for (int i = 0; i < enchant_lists.Length; i++)
            {
                var el = enchant_lists[i];
                string postfix = el.Length == 1 ? el[0].Name : el[0].Name + ", " + el[1].Name;
                var buff = Helpers.CreateBuff($"WarpriestSunBlessingMajor{i + 1}Buff",
                                              $"{name} ({postfix})",
                                              description,
                                              "",
                                              bless_weapon.Icon,
                                              null);
                for (int j = 0; j < el.Length; j++)
                {
                    buff.AddComponent(Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), false, true, el[j]));
                }

                var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
                var ability = Helpers.CreateAbility($"WarpriestSunBlessingMajor{i + 1}Ability",
                                                   buff.Name,
                                                   buff.Description,
                                                   "",
                                                   buff.Icon,
                                                   AbilityType.Supernatural,
                                                   CommandType.Standard,
                                                   AbilityRange.Touch,
                                                   Helpers.oneMinuteDuration,
                                                   "",
                                                   Helpers.CreateRunActions(apply_buff),
                                                   weapon_bond.GetComponent<AbilitySpawnFx>()
                                                   );
                addBlessingResourceLogic("Sun", ability, el.Length, quicken: true, parent: major_ability);
                ability.setMiscAbilityParametersTouchFriendly();
                major_abilities.Add(ability);
            }

            major_ability.AddComponent(Helpers.CreateAbilityVariants(major_ability, major_abilities.ToArray()));
            addBlessing("WarpriestBlessingSun", "Sun",
                         Common.AbilityToFeature(minor_ability, false),
                         Helpers.CreateFeature("WarpriestBlessingSunMajorFeature",
                                               name,
                                               description,
                                               "",
                                               bless_weapon.Icon,
                                               FeatureGroup.None,
                                               Helpers.CreateAddFact(major_ability)
                                               ),
                        "e28412c548ff21a49ac5b8b792b0aa9b");
        }


        static void createTravelBlessing()
        {
            var minor_ability = library.CopyAndAdd<BlueprintAbility>("ce5ec0a87ad5c4746bdd4e9a1552b397", "WarpriestTravelBlessingMinorAbility", "");//from travel domain
            minor_ability.SetDescription("At 1st level, as a swift action you gain increased mobility. For 1 round, you ignore all difficult terrain (including magical terrain) and take no penalties for moving through it.");
            minor_ability.RemoveComponents<AbilityResourceLogic>();
            minor_ability.Type = AbilityType.Supernatural;
            minor_ability.ActionType = CommandType.Swift;
            addBlessingResourceLogic("Travel", minor_ability);

            var name = "Dimensional Hop";
            var description = "At 10th level, you can teleport up to 25 feet as a move action. You can increase this distance by expending another use of your blessing—each use spent grants an additional 20 feet. You must have line of sight to your destination. This teleportation doesn’t provoke attacks of opportunity. You can bring other willing creatures with you, but each such creature requires expending one additional use of your blessing.";

            var major_ability_self = library.CopyAndAdd<BlueprintAbility>("a9b8be9b87865744382f7c64e599aeb2", "WarpriestTravelBlessingMajor1Ability", "");
            major_ability_self.SetName(name);
            major_ability_self.SetDescription(description);
            major_ability_self.RemoveComponents<SpellComponent>();
            major_ability_self.RemoveComponents<SpellListComponent>();
            major_ability_self.Type = AbilityType.Supernatural;
            major_ability_self.ActionType = CommandType.Move;
            major_ability_self.Range = AbilityRange.Close;
            addBlessingResourceLogic("Travel", major_ability_self, 1);


            var major_ability_mass = library.CopyAndAdd<BlueprintAbility>("5bdc37e4acfa209408334326076a43bc", "WarpriestTravelBlessingMajor2Ability", "");
            major_ability_mass.SetName(name + " - Mass");
            major_ability_mass.SetDescription(description);
            major_ability_mass.RemoveComponents<SpellComponent>();
            major_ability_mass.RemoveComponents<SpellListComponent>();
            major_ability_mass.Type = AbilityType.Supernatural;
            major_ability_mass.ActionType = CommandType.Move;
            major_ability_mass.Range = AbilityRange.Close;
            addBlessingResourceLogic("Travel", major_ability_mass, 2);


            addBlessing("WarpriestBlessingTraverl", "Travel",
                        Common.AbilityToFeature(minor_ability, false),
                        Helpers.CreateFeature("WarpriestBlessingTravelMajorFeature",
                                                name,
                                                description,
                                                "",
                                                major_ability_self.Icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddFacts(major_ability_self, major_ability_mass)
                                                ),
                        "c008853fe044bd442ae8bd22260592b7"
                        );
        }


        static void createTrickeryBlessing()
        {
            var minor_ability = library.CopyAndAdd<BlueprintAbility>("ee7eb5b9c644a0347b36eec653d3dfcb", "WarpriestTrickeryBlessingMinorAbility", "");
            minor_ability.SetDescription("At 1st level, as a move action you can create an illusory double of yourself. This double functions as a single mirror image, and lasts for a number of rounds equal to your warpriest level, or until the illusory duplicate is dispelled or destroyed. You can have no more than one double at a time. The double created by this ability doesn’t stack with the additional images from the mirror image spell.");
            minor_ability.RemoveComponents<AbilityResourceLogic>();
            minor_ability.Type = AbilityType.Supernatural;
            minor_ability.ReplaceComponent<ContextRankConfig>(c => {
                                                                        Helpers.SetField(c, "m_Class", getBlessingUsersArray());
                                                                        Helpers.SetField(c, "Archetype", Archetypes.DivineTracker.archetype);
                                                                        Helpers.SetField(c, "m_BaseValueType", ContextRankBaseValueType.SummClassLevelWithArchetype);
                                                                    }
                                                             );

            addBlessingResourceLogic("Trickery", minor_ability);

            var major_buff = library.CopyAndAdd<BlueprintBuff>("e6b35473a237a6045969253beb09777c", "WarpriestTrickeryBlessingMajorBuff", "");
            var spend_resource = Common.createContextActionSpendResource(warpriest_blessing_resource, 1, warpriest_aspect_of_war_buff);
            major_buff.AddComponent(Helpers.CreateAddFactContextActions(newRound: spend_resource));

            major_buff.SetDescription("At 10th level, as a swift action you can become invisible for 1 round (as greater invisibility)");
            major_buff.SetName("Greater Invisibility");

            var major_activatable_ability = Helpers.CreateActivatableAbility("WarpriestTrickeryBlessingMajorActivatableAbility",
                                                                 major_buff.Name,
                                                                 major_buff.Description,
                                                                 "",
                                                                 major_buff.Icon,
                                                                 major_buff,
                                                                 AbilityActivationType.Immediately,
                                                                 CommandType.Free,
                                                                 null,
                                                                 Helpers.CreateActivatableResourceLogic(warpriest_blessing_resource, ResourceSpendType.Never));
            if (!test_mode)
            {
                Helpers.SetField(major_activatable_ability, "m_ActivateWithUnitCommand", CommandType.Swift);
                major_activatable_ability.AddComponent(Common.createActivatableAbilityUnitCommand(CommandType.Swift));
            }

            addBlessing("WarpriestBlessingTrickery", "Trickery", Common.AbilityToFeature(minor_ability, false), Common.ActivatableAbilityToFeature(major_activatable_ability, false), "eaa368e08628a8641b16cd41cbd2cb33");
        }


        static void createWarBlessing()
        {
            for (int i = 0; i < 3; i++)
            {
                arsenal_chaplain_war_blessing_updates[i] = Helpers.CreateFeature($"ArsenalChaplainWarBlessingUpdate{i}Feature",
                                                                                  "",
                                                                                  "",
                                                                                  "",
                                                                                  null,
                                                                                  FeatureGroup.None);
                arsenal_chaplain_war_blessing_updates[i].HideInCharacterSheetAndLevelUp = true;
                arsenal_chaplain_war_blessing_updates[i].HideInUI = true;
            }

            AddStatBonus[][] boni = new AddStatBonus[][] {new AddStatBonus[]{Helpers.CreateAddStatBonus(StatType.Speed, 10, ModifierDescriptor.UntypedStackable)},
                                                          new AddStatBonus[]{Helpers.CreateAddStatBonus(StatType.AC, 1, ModifierDescriptor.Dodge) },
                                                          new AddStatBonus[]{Helpers.CreateAddStatBonus(StatType.AdditionalAttackBonus, 1, ModifierDescriptor.Insight) },
                                                           new AddStatBonus[]{Helpers.CreateAddStatBonus(StatType.SaveFortitude, 1, ModifierDescriptor.Luck),
                                                                              Helpers.CreateAddStatBonus(StatType.SaveReflex, 1, ModifierDescriptor.Luck),
                                                                              Helpers.CreateAddStatBonus(StatType.SaveWill, 1, ModifierDescriptor.Luck)
                                                                            }
                                                     };
            string[] postfix = new string[] { "Speed", "AC", "Attack Bonus", "Saving Throws" };
            List<BlueprintAbility> minor_variants = new List<BlueprintAbility>();
            List<BlueprintBuff> minor_buffs = new List<BlueprintBuff>();

            var minor_name = "War Mind";
            var minor_description = "At 1st level, you can touch an ally and grant it a tactical advantage for 1 minute. The ally gets one of the following bonuses: +10 feet to base land speed, +1 dodge bonus to AC, +1 insight bonus on attack rolls, or a +1 luck bonus on saving throws.";

            var mind_blank_buff = library.Get<BlueprintBuff>("35f3724d4e8877845af488d167cb8a89");

            var minor_ability = Helpers.CreateAbility("WarpriestWarBlessinMinorAbility",
                        minor_name,
                        minor_description,
                        "",
                        mind_blank_buff.Icon,
                        AbilityType.Supernatural,
                        CommandType.Standard,
                        AbilityRange.Touch,
                        Helpers.oneMinuteDuration,
                        ""
                        );
            minor_ability.setMiscAbilityParametersTouchFriendly();

            for (int i = 0; i < boni.Length; i++)
            {
                var buff = Helpers.CreateBuff($"WarpriestWarBlessingMinor{i}Buff",
                                              minor_name + $" ({postfix[i]})",
                                              minor_description,
                                              "",
                                              mind_blank_buff.Icon,
                                              mind_blank_buff.FxOnStart,
                                              boni[i]);

                var apply_buff = Common.createContextActionApplyBuff(buff,
                                                     Helpers.CreateContextDuration(Common.createSimpleContextValue(1), rate: DurationRate.Minutes),
                                                     dispellable: false);
                var ability = Helpers.CreateAbility($"WarpriestWarBlessinMinor{i}Ability",
                                                    buff.Name,
                                                    buff.Description,
                                                    "",
                                                    buff.Icon,
                                                    AbilityType.Supernatural,
                                                    CommandType.Standard,
                                                    AbilityRange.Touch,
                                                    Helpers.oneMinuteDuration,
                                                    "",
                                                    Helpers.CreateRunActions(apply_buff)
                                                    );
                ability.setMiscAbilityParametersTouchFriendly();
                
                minor_buffs.Add(buff);

                minor_variants.Add(ability);

                var ability_ranged = library.CopyAndAdd(ability, $"WarpriestWarBlessinMinor{i}RangedAbility", "");
                ability_ranged.SetName(ability.Name + " (Ranged)");
                ability_ranged.Range = AbilityRange.Close;
                ability_ranged.setMiscAbilityParametersSingleTargetRangedFriendly();
                ability_ranged.AddComponents(Helpers.Create<AbilityShowIfCasterHasFact>(a => a.UnitFact = arsenal_chaplain_war_blessing_updates[0]),
                                             Helpers.Create<NewMechanics.AbilityShowIfCasterHasNoFact>(a => a.UnitFact = arsenal_chaplain_war_blessing_updates[2]));
                addBlessingResourceLogic("War", ability_ranged, amount: 2, quicken: true, parent: minor_ability);


                var ability_mass = library.CopyAndAdd(ability, $"WarpriestWarBlessinMinor{i}MassNearAbility", "");
                ability_mass.SetName(ability.Name + " (Mass)");
                ability_mass.Range = AbilityRange.Personal;
                ability_mass.setMiscAbilityParametersSelfOnly();
                ability_mass.AddComponents(Helpers.CreateAbilityTargetsAround(7.Feet(), TargetType.Ally),
                                           Helpers.Create<AbilityShowIfCasterHasFact>(a => a.UnitFact = arsenal_chaplain_war_blessing_updates[1])
                                           );
                addBlessingResourceLogic("War", ability_mass, amount: 1, quicken: true, parent: minor_ability);


                var ability_mass_ranged = library.CopyAndAdd(ability, $"WarpriestWarBlessinMinor{i}MassRangedAbility", "");
                ability_mass_ranged.SetName(ability.Name + " (Mass Ranged)");
                ability_mass_ranged.Range = AbilityRange.Personal;
                ability_mass_ranged.setMiscAbilityParametersSelfOnly();
                ability_mass_ranged.AddComponents(Helpers.CreateAbilityTargetsAround(30.Feet(), TargetType.Ally),
                                                  Helpers.Create<AbilityShowIfCasterHasFact>(a => a.UnitFact = arsenal_chaplain_war_blessing_updates[2])
                                                  );
                addBlessingResourceLogic("War", ability_mass_ranged, amount: 2, quicken: true, parent: minor_ability);

                minor_variants.Add(ability_ranged);
                minor_variants.Add(ability_mass);
                minor_variants.Add(ability_mass_ranged);

                ability.AddComponent(Helpers.Create<NewMechanics.AbilityShowIfCasterHasNoFact>(a => a.UnitFact = arsenal_chaplain_war_blessing_updates[1]));
                addBlessingResourceLogic("War", ability, quicken: true, parent: minor_ability);
            }

            //remove other buffs
            foreach (var b in minor_buffs)
            {
                List<GameAction> remove_buffs = new List<GameAction>();
                foreach (var b2 in minor_buffs)
                {
                    if (b2 != b)
                    {
                        remove_buffs.Add(Common.createContextActionRemoveBuff(b2));
                    }
                    b.AddComponent(Helpers.CreateAddFactContextActions(remove_buffs.ToArray()));
                    ;
                }
            }

            //addBlessingResourceLogic(minor_ability);
            minor_ability.AddComponent(minor_ability.CreateAbilityVariants(minor_variants.ToArray()));

            var vicious_enchantment = library.Get<BlueprintWeaponEnchantment>("a1455a289da208144981e4b1ef92cc56");
            var resounding_blow_buff = library.Get<BlueprintBuff>("06173a778d7067a439acffe9004916e9");


            var major_buff = Helpers.CreateBuff("WarpriestWarMajorBlessingBuff",
                                     "Battle Lust",
                                     "At 10th level, you can touch an ally and grant it a thirst for battle. All of the ally’s melee attacks are treated as if they had the vicious weapon special ability. In addition, the ally receives a +4 insight bonus on attack rolls made to confirm critical hits and fast healing 1. These benefits last for 1 minute.",
                                     "",
                                     resounding_blow_buff.Icon,
                                     resounding_blow_buff.FxOnStart,
                                     Helpers.Create<CriticalConfirmationBonus>(c =>
                                                                                 {
                                                                                     c.Bonus = 4;
                                                                                     c.CheckWeaponRangeType = true;
                                                                                     c.Type = AttackTypeAttackBonus.WeaponRangeType.Melee;
                                                                                 }
                                                                                 ),
                                      Helpers.Create<AddEffectFastHealing>(c => c.Heal = 1)
                                    );

            foreach (var a in vicious_enchantment.GetComponents<AddInitiatorAttackRollTrigger>())
            {
                major_buff.AddComponent(Common.createAddInitiatorAttackWithWeaponTrigger(a.Action, check_weapon_range_type: true, on_initiator: a.OnOwner));
            }

            var apply_major_buff = Common.createContextActionApplyBuff(major_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);

            var major_ability_touch = Helpers.CreateAbility("WarpriestWarMajorBlessingTouchAbility",
                                          major_buff.Name,
                                          major_buff.Description,
                                          "",
                                          major_buff.Icon,
                                          AbilityType.Supernatural,
                                          CommandType.Standard,
                                          AbilityRange.Touch,
                                          Helpers.oneMinuteDuration,
                                          "",
                                          Helpers.CreateRunActions(apply_major_buff)
                                          );
            major_ability_touch.setMiscAbilityParametersTouchFriendly();
            
            var major_ability_ranged = library.CopyAndAdd(major_ability_touch, "WarpriestWarMajorBlessingRangedAbility", "");
            major_ability_ranged.SetName(major_ability_touch.Name + " (Ranged)");
            major_ability_ranged.Range = AbilityRange.Close;
            major_ability_ranged.setMiscAbilityParametersSingleTargetRangedFriendly();
            major_ability_ranged.AddComponents(Helpers.Create<AbilityShowIfCasterHasFact>(a => a.UnitFact = arsenal_chaplain_war_blessing_updates[0]),
                                               Helpers.Create<NewMechanics.AbilityShowIfCasterHasNoFact>(a => a.UnitFact = arsenal_chaplain_war_blessing_updates[2]));

            var major_ability_mass = library.CopyAndAdd(major_ability_touch, "WarpriestWarMajorBlessingMassAbility", "");
            major_ability_mass.SetName(major_ability_touch.Name + " (Mass)");
            major_ability_mass.Range = AbilityRange.Personal;
            major_ability_mass.setMiscAbilityParametersSelfOnly();
            major_ability_mass.AddComponents(Helpers.CreateAbilityTargetsAround(7.Feet(), TargetType.Ally),
                                             Helpers.Create<AbilityShowIfCasterHasFact>(a => a.UnitFact = arsenal_chaplain_war_blessing_updates[1]));

            var major_ability_mass_ranged = library.CopyAndAdd(major_ability_touch, "WarpriestWarMajorBlessingMassRangedAbility", "");
            major_ability_mass_ranged.SetName(major_ability_touch.Name + " (Mass Ranged)");
            major_ability_mass_ranged.Range = AbilityRange.Personal;
            major_ability_mass_ranged.setMiscAbilityParametersSelfOnly();
            major_ability_mass_ranged.AddComponents(Helpers.CreateAbilityTargetsAround(30.Feet(), TargetType.Ally),
                                             Helpers.Create<AbilityShowIfCasterHasFact>(a => a.UnitFact = arsenal_chaplain_war_blessing_updates[2]));

            major_ability_touch.AddComponent(Helpers.Create<NewMechanics.AbilityShowIfCasterHasNoFact>(a => a.UnitFact = arsenal_chaplain_war_blessing_updates[1]));
            var major_ability = Helpers.CreateAbility("WarpriestWarMajorBlessingAbility",
                                                      major_buff.Name,
                                                      major_buff.Description,
                                                      "",
                                                      major_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      ""
                                                      );
            major_ability.setMiscAbilityParametersTouchFriendly();
            major_ability.AddComponent(major_ability.CreateAbilityVariants(major_ability_touch, major_ability_ranged, major_ability_mass, major_ability_mass_ranged));
            addBlessingResourceLogic("War", major_ability_touch, quicken: true, parent: major_ability);
            addBlessingResourceLogic("War", major_ability_ranged, amount: 2, quicken: true, parent: major_ability);
            addBlessingResourceLogic("War", major_ability_mass, quicken: true, parent: major_ability);
            addBlessingResourceLogic("War", major_ability_mass_ranged, amount: 2, quicken: true, parent: major_ability);

            addBlessing("WarpriestBlessingWar", "War", minor_ability, major_ability, "3795653d6d3b291418164b27be88cb43");
        }


        static void createWaterBlessing()
        {
            var weapon_bond = library.Get<BlueprintAbility>("7ff088ab58c69854b82ea95c2b0e35b4");
            var cold_ray = library.Get<BlueprintAbility>("7ef096fdc8394e149a9e8dced7576fee");
            var frost_enchantment = library.Get<BlueprintWeaponEnchantment>("421e54078b7719d40915ce0672511d0b");
            var bless_weapon = library.Get<BlueprintAbility>("831e942864e924846a30d2e0678e438b");

            var enchantment = Common.createWeaponEnchantment("WarpriestWaterMinorBlessingWeaponEcnchantment",
                                                             "Ice Strike",
                                                             "This weapon  glows with a blue-white chill and deals an additional 1d4 points of cold damage with each strike.",
                                                             "",
                                                             "",
                                                             "",
                                                             0,
                                                             frost_enchantment.WeaponFxPrefab,
                                                             Common.weaponEnergyDamageDice(DamageEnergyType.Cold, new DiceFormula(1, DiceType.D4))
                                                             );
            var minor_buff = Helpers.CreateBuff("WarpriestWaterMinorBlessingBuff",
                                     enchantment.Name,
                                     "At 1st level, you can touch one weapon and enhance it with the power of water. For 1 minute, this weapon glows with a blue-white chill and deals an additional 1d4 points of cold damage with each strike.",
                                     "",
                                     cold_ray.Icon,
                                     null,
                                     Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), false, true, enchantment)
                                    );

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestWaterMinorBlessingAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_minor_buff),
                                                      weapon_bond.GetComponent<AbilitySpawnFx>()
                                                      );

            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic("Water", minor_ability, quicken: true);

            var chill_shield = NewSpells.fire_shield_variants[DamageEnergyType.Cold];
            var major_buff = library.CopyAndAdd<BlueprintBuff>(NewSpells.fire_shield_buffs[DamageEnergyType.Cold], "WarpriestWaterMajorBlessingBuff", "");
            major_buff.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.SummClassLevelWithArchetype, classes: getBlessingUsersArray(),
                                                                                           archetype: Archetypes.DivineTracker.archetype,
                                                                                           max: 15, type: AbilityRankType.DamageBonus
                                                                                           )
                                                          );
            major_buff.SetName("Armor of Ice");
            major_buff.SetDescription("At 10th level, you can touch any one ally and wreath it in freezing mist. This works as fire shield (chill shield only) with a duration 1 minute.");

            var apply_major_buff = Common.createContextActionApplyBuff(major_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var major_ability = Helpers.CreateAbility("WarpriestWaterMajorBlessingAbility",
                                                      major_buff.Name,
                                                      major_buff.Description,
                                                      "",
                                                      major_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_major_buff),
                                                      chill_shield.GetComponent<SpellDescriptorComponent>(),
                                                      chill_shield.GetComponent<AbilitySpawnFx>()
                                                      );

            major_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic("Water", major_ability, quicken: true);

            addBlessing("WarpriestBlessingWater", "Water", minor_ability, major_ability, "8f49469c40e2c6e4db61296558e08966");
        }


        static void createWeatherBlessing()
        {
            var weapon_bond = library.Get<BlueprintAbility>("7ff088ab58c69854b82ea95c2b0e35b4");
            var lightning = library.Get<BlueprintAbility>("d2cff9243a7ee804cb6d5be47af30c73");
            var shock_enchantment = library.Get<BlueprintWeaponEnchantment>("7bda5277d36ad114f9f9fd21d0dab658");
            var bless_weapon = library.Get<BlueprintAbility>("831e942864e924846a30d2e0678e438b");

            var enchantment = Common.createWeaponEnchantment("WarpriestWeatherBlessingWeaponEcnchantment",
                                                             "Storm Strike",
                                                             "this weapon glows with blue or yellow sparks and deals an additional 1d4 points of electricity damage with each hit.",
                                                             "",
                                                             "",
                                                             "",
                                                             0,
                                                             shock_enchantment.WeaponFxPrefab,
                                                             Common.weaponEnergyDamageDice(DamageEnergyType.Electricity, new DiceFormula(1, DiceType.D4))
                                                             );
            var minor_buff = Helpers.CreateBuff("WarpriestWeatherMinorBlessingBuff",
                                     enchantment.Name,
                                     "At 1st level, you can touch one weapon and grant it a blessing of stormy weather. For 1 minute, this weapon glows with blue or yellow sparks and deals an additional 1d4 points of electricity damage with each hit.",
                                     "",
                                     lightning.Icon,
                                     null,
                                     Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), false, true, enchantment)
                                    );

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestWeatherMinorBlessingAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_minor_buff),
                                                      weapon_bond.GetComponent<AbilitySpawnFx>()
                                                      );

            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic("Weather", minor_ability, quicken: true);

            var blur_buff = library.Get<BlueprintBuff>("dd3ad347240624d46a11a092b4dd4674");

            var major_buff = Helpers.CreateBuff("WarpriestWeatherBlessingMajorBuff",
                                                "Wind Barrier",
                                                "At 10th level, you can create a barrier of fast winds around yourself for 1 minute. It gives you complete immunity to ranged attacks.",
                                                "",
                                                blur_buff.Icon,
                                                blur_buff.FxOnStart,
                                                Helpers.Create<NewMechanics.WeaponAttackAutoMiss>(w => w.attack_types = new AttackType[] { AttackType.Ranged, AttackType.RangedTouch })
                                                );
            var apply_major_buff = Common.createContextActionApplyBuff(major_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var major_ability = Helpers.CreateAbility("WarpriestWeatherMajorBlessingAbility",
                                                      major_buff.Name,
                                                      major_buff.Description,
                                                      "",
                                                      major_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Personal,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_major_buff)
                                                      );

            major_ability.setMiscAbilityParametersSelfOnly();
            addBlessingResourceLogic("Weather", major_ability, quicken: true);

            addBlessing("WarpriestBlessingWeather", "Weather", minor_ability, major_ability, "9dfdfd4904e98fa48b80c8f63ec2cf11");
        }



        static void createSacredFist()
        {
            sacred_fist_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "SacredFistArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Sacred Fist");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Unlike many warpriests, sacred fists leave behind armor and shield and instead rely on their fists and whatever protection their deity bestows on them.");
            });
            Helpers.SetField(sacred_fist_archetype, "m_ParentClass", warpriest_class);
            library.AddAsset(sacred_fist_archetype, "");


            var monk = library.Get<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");
            createSacredFistProficiencies();
            createSacredFistFakeMonkLevels();
            //unarmed damage, 1d6 already includes full progression if we used condenseMonkUnarmedDamage
            var fist1d6_monk = library.Get<BlueprintFeature>("c3fbeb2ffebaaa64aa38ce7a0bb18fb0");
            ClassToProgression.addClassToFeat(warpriest_class, new BlueprintArchetype[] { sacred_fist_archetype }, ClassToProgression.DomainSpellsType.NoSpells, fist1d6_monk, monk);
            var fist1d6 = library.CopyAndAdd(fist1d6_monk, "WarpriestSacredFistUnarmed1d6Feature", "");
            fist1d6.SetDescription("At 1st level, a sacred fist gains Improved Unarmed Strike as a bonus feat. The damage dealt by a Medium sacred fist's unarmed strike increases with level: 1d6 at levels 1–3, 1d8 at levels 4–7, 1d10 at levels 8–11, 2d6 at levels 12–15, 2d8 at levels 16–19, and 2d10 at level 20.\nIf the sacred fist is Small, his unarmed strike damage increases as follows: 1d4 at levels 1–3, 1d6 at levels 4–7, 1d8 at levels 8–11, 1d10 at levels 12–15, 2d6 at levels 16–19, and 2d8 at level 20.\nIf the sacred fist is Large, his unarmed strike damage increases as follows: 1d8 at levels 1–3, 2d6 at levels 4–7, 2d8 at levels 8–11, 3d6 at levels 12–15, 3d8 at levels 16–19, and 4d8 at level 20.");
            
            /*var fist1d8 = library.Get<BlueprintFeature>("8267a0695a4df3f4ca508499e6164b98");
            var fist1d10 = library.Get<BlueprintFeature>("f790a36b5d6f85a45a41244f50b947ca");
            var fist2d6 = library.Get<BlueprintFeature>("b3889f445dbe42948b8bb1ba02e6d949");
            var fist2d8 = library.Get<BlueprintFeature>("078636a2ce835e44394bb49a930da230");
            var fist2d10 = library.Get<BlueprintFeature>("df38e56fa8b3f0f469d55f9aa26b3f5c");*/

            var ac_bonus_old = library.CopyAndAdd<BlueprintFeature>("e241bdfd6333b9843a7bfd674d607ac4", "ACBonusSacredFistACBonusFeature", "");
            ac_bonus_old.SetDescription("When unarmored and unencumbered, the sacred fist adds his Wisdom bonus (if any) to his AC and CMD. In addition, a sacred fist gains a +1 bonus to AC and CMD at 4th level. This bonus increases by 1 for every four sacred fist levels thereafter, up to a maximum of +5 at 20th level.");
            ac_bonus_old.ComponentsArray = new BlueprintComponent[0];
            var ac_bonus = library.Get<BlueprintFeature>("e241bdfd6333b9843a7bfd674d607ac4");
            ac_bonus.Ranks++;
            foreach (var c in ac_bonus.GetComponents<ContextRankConfig>().ToArray())
            {
                if (c.IsBasedOnClassLevel)
                {
                    ClassToProgression.addClassToContextRankConfig(warpriest_class, new BlueprintArchetype[] { sacred_fist_archetype }, c, "SacredFist", monk);
                    /*var new_c = c.CreateCopy();
                    Helpers.SetField(new_c, "m_Class", getWarpriestArray());
                    ac_bonus.ReplaceComponent(c, new_c);
                    break;*/
                }
                if (c.IsBasedOnCustomProperty) //for balance fixes (class level limiter on wisdom)
                {
                    var property = Helpers.GetField<BlueprintUnitProperty>(c, "m_CustomProperty");
                    var cfg = property.GetComponent<NewMechanics.ContextValueWithLimitProperty>().max_value;
                    ClassToProgression.addClassToContextRankConfig(warpriest_class, new BlueprintArchetype[] { sacred_fist_archetype }, cfg, "SacredFist", monk);
                }
            }

            var unlock_ac_bonus = Common.createMonkFeatureUnlock(ac_bonus_old, false);
            unlock_ac_bonus.ReplaceComponent<MonkNoArmorFeatureUnlock>(m => m.NewFact = ac_bonus);
            unlock_ac_bonus.SetDescription($"When unarmored and unencumbered, the sacred fist adds his Wisdom bonus (if any{(Main.settings.balance_fixes_monk_ac ? ", up to his sacred fist level" : "")}) to his AC and CMD. In addition, a sacred fist gains a +1 bonus to AC and CMD at 4th level. This bonus increases by 1 for every four sacred fist levels thereafter, up to a maximum of +5 at 20th level.");
            var flurry2 = library.CopyAndAdd<BlueprintFeature>("332362f3bd39ebe46a740a36960fdcb4", "WarpriestSacredFistFlurryOfBlows1Feature", "");
            flurry2.SetDescription("At 2nd level, a sacred fist can make a flurry of blows as a full attack. When making a flurry of blows, the sacred fist can make one additional attack at his highest base attack bonus. This additional attack stacks with the bonus attacks from haste and other similar effects. When using this ability, the sacred fist can make these attacks with any combination of his unarmed strikes and weapons that have the monk special weapon quality. He takes no penalty for using multiple weapons when making a flurry of blows, but he does not gain any additional attacks beyond what's already granted by the flurry for doing so. (He can still gain additional attacks from a high base attack bonus, from this ability, and from haste and similar effects).\nAt 15th level, a sacred fist can make an additional attack at his highest base attack bonus whenever he makes a flurry of blows. This stacks with the first attack from this ability and additional attacks from haste and similar effects.");
            var flurry15 = library.CopyAndAdd<BlueprintFeature>("de25523acc24b1448aa90f74d6512a08", "WarpriestSacredFistFlurryOfBlows2Feature", "");
            var flurry1_monk = library.Get<BlueprintFeature>("332362f3bd39ebe46a740a36960fdcb4");
            var flurry10_monk = library.Get<BlueprintFeature>("de25523acc24b1448aa90f74d6512a08");
            flurry10_monk.SetDescription("");
            flurry1_monk.SetDescription("");
            flurry1_monk.Ranks++;
            flurry10_monk.Ranks++;

            flurry2_unlock = Common.createMonkFeatureUnlock(flurry2, true);
            flurry15_unlock = Common.createMonkFeatureUnlock(flurry15, true);
            flurry15_unlock.HideInCharacterSheetAndLevelUp = true;
            flurry15_unlock.HideInUI = true;
            flurry2_unlock.GetComponent<MonkNoArmorAndMonkWeaponFeatureUnlock>().NewFact = flurry1_monk;
            flurry15_unlock.GetComponent<MonkNoArmorAndMonkWeaponFeatureUnlock>().NewFact = flurry10_monk;

            createSacredFistKiPowers();

            var ki_strike_cold_iron_silver = library.CopyAndAdd<BlueprintFeature>("7b657938fde78b14cae10fc0d3dcb991", "WarpriestSacredFistKiStrikeColdIronSilver", "");
            ki_strike_cold_iron_silver.SetDescription("At 10th level, the sacred fist's unarmed attacks are treated as cold iron and silver for the purpose of overcoming damage reduction.");

            var ki_strike_lawful = library.CopyAndAdd<BlueprintFeature>("34439e527a8f5fb4588024e71960dd42", "WarpriestSacredFistKiStrikeLawful", "");
            ki_strike_lawful.SetDescription("At 13th level, the sacred fist's unarmed attacks are treated as lawful weapons for the purpose of overcoming damage reduction.");
            var ki_strike_chaotic = library.CopyAndAdd<BlueprintFeature>("34439e527a8f5fb4588024e71960dd42", "WarpriestSacredFistKiStrikeChaotic", "");
            ki_strike_chaotic.SetNameDescription("Ki Strike — Chaotic", "At 13th level, the sacred fist's unarmed attacks are treated as chaotic weapons for the purpose of overcoming damage reduction.");
            ki_strike_chaotic.ReplaceComponent<AddOutgoingPhysicalDamageProperty>(a => a.Alignment = DamageAlignment.Chaotic);
            var ki_strike_good = library.CopyAndAdd<BlueprintFeature>("34439e527a8f5fb4588024e71960dd42", "WarpriestSacredFistKiStrikeGood", "");
            ki_strike_good.SetNameDescription("Ki Strike — Good", "At 13th level, the sacred fist's unarmed attacks are treated as good weapons for the purpose of overcoming damage reduction.");
            ki_strike_good.ReplaceComponent<AddOutgoingPhysicalDamageProperty>(a => a.Alignment = DamageAlignment.Good);
            var ki_strike_evil = library.CopyAndAdd<BlueprintFeature>("34439e527a8f5fb4588024e71960dd42", "WarpriestSacredFistKiStrikeEvil", "");
            ki_strike_evil.SetNameDescription("Ki Strike — Evil", "At 13th level, the sacred fist's unarmed attacks are treated as evil weapons for the purpose of overcoming damage reduction.");
            ki_strike_evil.ReplaceComponent<AddOutgoingPhysicalDamageProperty>(a => a.Alignment = DamageAlignment.Evil);

            var ki_strikes_alignment = new BlueprintFeature[] { ki_strike_lawful, ki_strike_chaotic, ki_strike_good, ki_strike_evil };
            var deity_alignment = new BlueprintFeature[]{ library.Get<BlueprintFeature>("092714336606cfc45a37d2ab39fabfa8"), //law domain allowed
                                                          library.Get<BlueprintFeature>("8c7d778bc39fec642befc1435b00f613"), //chaos domain allowed
                                                          library.Get<BlueprintFeature>("882521af8012fc749930b03dc18a69de"), //good domain allowed
                                                          library.Get<BlueprintFeature>("351235ac5fc2b7e47801f63d117b656c") //evil domain allowed
                                                         };

            for (int i = 0; i < ki_strikes_alignment.Length; i++)
            {
                ki_strikes_alignment[i].AddComponents(Helpers.PrerequisiteFeature(deity_alignment[i], any: true),
                                                    Helpers.Create<PrerequisiteMechanics.PrerequisiteNoFeatures>(a => { a.Features = deity_alignment; a.Group = Prerequisite.GroupType.Any; })
                                                    );
            }

            var ki_strike_alignment = Helpers.CreateFeatureSelection("KiStrikeAlignmentSacredFistFeatureSelection",
                                                            "Aligned Ki Strike",
                                                            "At 13th level, the sacred fist's unarmed attacks are treated as weapons having one component of his Deity alignment for the purpose of overcoming damage reduction. If sacred fist's Deity is neutral, he can select any alignment component.",
                                                            "",
                                                            null,
                                                            FeatureGroup.None
                                                            );
            ki_strike_alignment.AllFeatures = ki_strikes_alignment;
                                                            


            var ki_strike_adamantine = library.CopyAndAdd<BlueprintFeature>("ddc10a3463bd4d54dbcbe993655cf64e", "WarpriestSacredFistKiStrikeAdamantine", "");
            ki_strike_adamantine.SetDescription("At 19th level, the sacred fist's unarmed attacks are treated as adamantine weapons for the purpose of overcoming damage reduction and bypassing hardness.");

            createSacredFistStyleFeatsSelection();
            createSacredFistFortitudeEvasion();
            createSacredFistNoMonkCheck();

            var improved_unarmed_strike = library.Get<BlueprintFeature>("7812ad3672a4b9a4fb894ea402095167");


            sacred_fist_archetype.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, warpriest_proficiencies,
                                                                                        warpriest_fighter_feat_prerequisite_replacement,
                                                                                        weapon_focus_selection,
                                                                                        warpriest_sacred_weapon_damage),
                                                                    Helpers.LevelEntry(3, fighter_feat),
                                                                    Helpers.LevelEntry(4, warpriest_sacred_weapon_enhancement),
                                                                    Helpers.LevelEntry(6, fighter_feat),
                                                                    Helpers.LevelEntry(7, warpriest_sacred_armor),
                                                                    Helpers.LevelEntry(8, warpriest_sacred_weapon_enhancement2),
                                                                    Helpers.LevelEntry(9, fighter_feat),
                                                                    Helpers.LevelEntry(10, warpriest_sacred_armor2),
                                                                    Helpers.LevelEntry(12, fighter_feat, warpriest_sacred_weapon_enhancement3),
                                                                    Helpers.LevelEntry(13, warpriest_sacred_armor3),
                                                                    Helpers.LevelEntry(14),
                                                                    Helpers.LevelEntry(16, warpriest_sacred_weapon_enhancement4, warpriest_sacred_armor4),
                                                                    Helpers.LevelEntry(17),
                                                                    Helpers.LevelEntry(18, fighter_feat),
                                                                    Helpers.LevelEntry(19, warpriest_sacred_armor5),
                                                                    Helpers.LevelEntry(20, warpriest_sacred_weapon_enhancement5)
                                                                    };

            sacred_fist_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, sacred_fist_proficiencies, improved_unarmed_strike, unlock_ac_bonus, sacred_fist_fake_monk_level,
                                                                                      fist1d6),
                                                                  Helpers.LevelEntry(2, flurry2_unlock),
                                                                  Helpers.LevelEntry(3, blessed_fortitude),
                                                                  //Helpers.LevelEntry(4, fist1d8),
                                                                  Helpers.LevelEntry(6, sacred_fist_syle_feat_selection),
                                                                  Helpers.LevelEntry(7, sacred_fist_ki_pool),
                                                                  //Helpers.LevelEntry(8, fist1d10),
                                                                  Helpers.LevelEntry(9, miraculous_fortitude),
                                                                  Helpers.LevelEntry(10, ki_strike_cold_iron_silver),
                                                                  Helpers.LevelEntry(12, sacred_fist_syle_feat_selection/*, fist2d6*/),
                                                                  Helpers.LevelEntry(13, ki_strike_alignment),
                                                                  Helpers.LevelEntry(15, flurry15_unlock),
                                                                  //Helpers.LevelEntry(16, fist2d8),
                                                                  Helpers.LevelEntry(18, sacred_fist_syle_feat_selection),
                                                                  Helpers.LevelEntry(19, ki_strike_adamantine),
                                                                  //Helpers.LevelEntry(20, fist2d10),
                                                                };

            warpriest_progression.UIGroups[0].Features.Add(improved_unarmed_strike);
            warpriest_progression.UIGroups[0].Features.Add(sacred_fist_syle_feat_selection);
            warpriest_progression.UIGroups[0].Features.Add(sacred_fist_syle_feat_selection);
            warpriest_progression.UIGroups[0].Features.Add(sacred_fist_syle_feat_selection);

            warpriest_progression.UIGroups[2].Features.Add(fist1d6);
            warpriest_progression.UIGroups[2].Features.Add(flurry2_unlock);
            warpriest_progression.UIGroups[2].Features.Add(sacred_fist_ki_pool);
            warpriest_progression.UIGroups[2].Features.Add(ki_strike_cold_iron_silver);
            warpriest_progression.UIGroups[2].Features.Add(ki_strike_alignment);
            warpriest_progression.UIGroups[2].Features.Add(ki_strike_adamantine);

            warpriest_progression.UIGroups[3].Features.Add(unlock_ac_bonus);
            warpriest_progression.UIGroups[3].Features.Add(blessed_fortitude);
            warpriest_progression.UIGroups[3].Features.Add(miraculous_fortitude);

            warpriest_progression.UIDeterminatorsGroup = warpriest_progression.UIDeterminatorsGroup.AddToArray(sacred_fist_proficiencies, sacred_fist_fake_monk_level);

            sacred_fist_archetype.ReplaceClassSkills = true;
            sacred_fist_archetype.ClassSkills = warpriest_class.ClassSkills.AddToArray(StatType.SkillMobility, StatType.SkillStealth, StatType.SkillPerception, StatType.SkillKnowledgeWorld);

            var monk_class = library.Get<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");
            //monk_class.AddComponent(Common.prerequisiteNoArchetype(warpriest_class, sacred_fist_archetype));
            sacred_fist_archetype.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(c => c.CharacterClass = monk_class));
            sacred_fist_archetype.ReplaceStartingEquipment = true;
            sacred_fist_archetype.StartingItems = monk_class.StartingItems;
            sacred_fist_archetype.StartingGold = monk_class.StartingGold;
        }

        static void createSacredFistNoMonkCheck()
        {
            var monk_class = library.Get<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");
            sacred_fist_no_monk_check = Helpers.CreateFeatureSelection("SacredFistNoMonkCheckSelection",
                                                                       "No Monk Levels Check",
                                                                       "Sacred fist can not have monk levels.",
                                                                       "",
                                                                       null,
                                                                       FeatureGroup.None);
            sacred_fist_no_monk_check.HideInCharacterSheetAndLevelUp = true;
            var no_monk_feature = Helpers.CreateFeature("SacredFistNoMonkLevelsFeature",
                                  "No Monk Levels",
                                  sacred_fist_no_monk_check.Description,
                                  "",
                                  null,
                                  FeatureGroup.None,
                                  Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = monk_class)
                                  );
            no_monk_feature.HideInUI = true;
            sacred_fist_no_monk_check.AllFeatures = new BlueprintFeature[] { no_monk_feature };
            sacred_fist_no_monk_check.Obligatory = true;
        }

        static void createSacredFistProficiencies()
        {
            sacred_fist_proficiencies = library.CopyAndAdd<BlueprintFeature>("c7d6f5244c617734a8a76b6785a752b4", "SacredFistProficiencies", "");
            sacred_fist_proficiencies.SetName("Sacred Fist Proficiencies");
            sacred_fist_proficiencies.SetDescription("Sacred fists are proficient with the club, crossbow (light or heavy), dagger, handaxe, javelin, kama, nunchaku, quarterstaff, sai, shortspear, short sword, shuriken, siangham, sling, and spear. Sacred fists are not proficient with any armor or shields. When wearing armor, using a shield, or carrying a medium or heavy load, a sacred fist loses his AC bonus and flurry of blows.");
        }

        static void createSacredFistFakeMonkLevels()
        {
            var monk_class = library.Get<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");
            var stone_fist = library.Get<BlueprintAbility>("85067a04a97416949b5d1dbf986d93f3");

            sacred_fist_fake_monk_level = Helpers.CreateFeature("WarpriestMonkTraining",
                                                                "Monk training",
                                                                "Sacred fist uses his warpriest levels as monk levels for the purposes of meeting the feat’s prerequisites.",
                                                                "",
                                                                stone_fist.Icon,
                                                                FeatureGroup.None,
                                                                Common.createClassLevelsForPrerequisites(monk_class, warpriest_class)
                                                                );
        }


        static void createSacredFistKiPowers()
        {
            var ki_resource = Helpers.CreateAbilityResource("WarpriestSacredFistKiResource", "", "", "", null);
            ki_resource.SetIncreasedByLevelStartPlusDivStep(1, 5, 0, 2, 1, 0, 0.0f, getWarpriestArray());
            ki_resource.SetIncreasedByStat(1, StatType.Wisdom);

            var extra_attack_ability = library.CopyAndAdd<BlueprintAbility>("7f6ea312f5dad364fa4a896d7db39fdd", "WarpriestSacredFistKiExtraAttackAbility", "");
            extra_attack_ability.SetDescription("By spending 1 point from his ki pool as a swift action, a sacred fist can make one additional unarmed strike at his highest attack bonus when making a flurry of blows attack.This bonus attack stacks with all bonus attacks gained from flurry of blows, as well as those from haste and similar effects.");
            extra_attack_ability.ReplaceComponent<AbilityResourceLogic>(c => c.RequiredResource = ki_resource);
            library.Get<BlueprintBuff>("cadf8a5c42002494cabfc6c1196b514a").SetDescription("By spending 1 point from his ki pool as a swift action, a character can make one additional unarmed strike at his highest attack bonus when making a flurry of blows attack. This bonus attack stacks with all bonus attacks gained from flurry of blows, as well as those from haste and similar effects.");

            var mage_armor = library.Get<BlueprintAbility>("9e1ad5d6f87d19e4d8883d63a6e35568");
            var ki_armor_buff = Helpers.CreateBuff("WarpriestSacredFistKiArmorBuff",
                                                   "Ki Armor",
                                                   "A sacred fist can as a swift action spend 1 point from his ki pool to grant himself a +1 insight bonus to his AC for 1 minute. This insight bonus increases by 1 for every 3 levels above 7th (to a maximum of +5 at 19th level).",
                                                   "",
                                                   mage_armor.Icon,
                                                   null,
                                                   Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.Insight, ContextValueType.Rank, AbilityRankType.StatBonus),
                                                   Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getWarpriestArray(),
                                                                                   type: AbilityRankType.StatBonus, progression: ContextRankProgression.StartPlusDivStep,
                                                                                   startLevel: 7, stepLevel: 3)
                                                   );

            var apply_buff = Common.createContextActionApplyBuff(ki_armor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);

            var ki_armor = Helpers.CreateAbility("WarpriestSacredFistKiArmor",
                                                 ki_armor_buff.Name,
                                                 ki_armor_buff.Description,
                                                 "",
                                                 ki_armor_buff.Icon,
                                                 AbilityType.Supernatural,
                                                 CommandType.Swift,
                                                 AbilityRange.Personal,
                                                 Helpers.oneMinuteDuration,
                                                 "",
                                                 Helpers.CreateRunActions(apply_buff),
                                                 mage_armor.GetComponent<AbilitySpawnFx>(),
                                                 Helpers.CreateResourceLogic(ki_resource)
                                                 );

            var ki_strike_magic = library.CopyAndAdd<BlueprintFeature>("1188005ee3160f84f8bed8b19a7d46cf", "WarpriestSacredFistKiStrikeMagic", "");
            ki_strike_magic.SetDescription("At 7th level, ki strike allows the sacred fist's unarmed attacks to be treated as magic weapons for the purpose of overcoming damage reduction.");

            sacred_fist_ki_pool = Helpers.CreateFeature("WarpriestSacredFistKiPowerFeature",
                                                        "Ki Pool",
                                                        "At 7th level, a sacred fist gains a pool of ki points, supernatural energy he can use to accomplish amazing feats. The number of points in a sacred's ki pool is equal to 1/2 his sacred fist level + his Wisdom modifier - 3. As long as he has at least 1 point in his ki pool, he can make a ki strike.\n"
                                                        + "At 7th level, ki strike allows his unarmed attacks to be treated as magic weapons for the purpose of overcoming damage reduction.\n"
                                                        + "At 10th level, his unarmed attacks are also treated as cold iron and silver for the purpose of overcoming damage reduction.\n"
                                                        + "At 13th level, his unarmed attacks are also treated as having one of of his Deity alignment components for the purpose of overcoming damage reduction.\n"
                                                        + "At 19th level, his unarmed attacks are treated as adamantine weapons for the purpose of overcoming damage reduction and bypassing hardness.\n"
                                                        + "By spending 1 point from his ki pool as a swift action, a sacred fist can make one additional unarmed strike at his highest attack bonus when making a flurry of blows attack. This bonus attack stacks with all bonus attacks gained from flurry of blows, as well as those from haste and similar effects.\n"
                                                        + "Additionally, the sacred fist can as a swift action spend 1 point from his ki pool to grant himself a +1 insight bonus to his AC for 1 minute. This insight bonus increases by 1 for every 3 levels above 7th (to a maximum of +5 at 19th level)\n"
                                                        + "The ki pool is replenished each morning after 8 hours of rest or meditation; these hours do not need to be consecutive.",
                                                        "",
                                                        null,
                                                        FeatureGroup.None,
                                                        Helpers.CreateAddAbilityResource(ki_resource),
                                                        Helpers.CreateAddFacts(ki_armor, extra_attack_ability, ki_strike_magic)
                                                        );
        }


        static void createSacredFistStyleFeatsSelection()
        {
            BlueprintFeature[] style_feats = new BlueprintFeature[]{
                                                                        library.Get<BlueprintFeature>("0c17102f650d9044290922b0fad9132f"),//crane style
                                                                        library.Get<BlueprintFeature>("af0aae1b973114f47a19ea532237b5fc"),//crane wing
                                                                        library.Get<BlueprintFeature>("59eb2a5507975244c893402d582bf77b"),//crane riposte

                                                                        library.Get<BlueprintFeature>("c36562b8e7ae12d408487ba8b532d966"),//pummeling style
                                                                        library.Get<BlueprintFeature>("bdf58317985383540920c723db07aa3b"),//pummeling bully
                                                                        library.Get<BlueprintFeature>("c5a39c8f1a2d6824ca565e6c1e4075a5"),//pummeling charge

                                                                        library.Get<BlueprintFeature>("87ec6541cddfa394ab540dd13399d319"),//dragon style
                                                                        library.Get<BlueprintFeature>("2a681cb9fcaab664286cb36fff761245"),//dragon ferocity
                                                                        library.Get<BlueprintFeature>("3fca938ad6a5b8348a8523794127c5bc"),//dragon roar
                                                                    };

            //set pumeling style to work with warpriest flurry
            library.Get<BlueprintFeature>("c36562b8e7ae12d408487ba8b532d966").AddComponent(Helpers.PrerequisiteFeature(flurry2_unlock, true));

            sacred_fist_syle_feat_selection = Helpers.CreateFeatureSelection("WarpriestSacredFist",
                                                                            "Bonus Style Feat",
                                                                            "At 6th level, the sacred fist gains a style feat as a bonus feat. The sacred fist must meet the style feat’s prerequisites. At 12th and 18th levels, a sacred fist gains either another style feat or a feat that requires a style feat as a prerequisite.",
                                                                            "",
                                                                            null,
                                                                            FeatureGroup.None
                                                                            );
            sacred_fist_syle_feat_selection.AllFeatures = style_feats;
            //sacred_fist_syle_feat_selection.Features = style_feats;
        }


        static void createSacredFistFortitudeEvasion()
        {
            var defensive_stance = library.Get<BlueprintFeature>("2a6a2f8e492ab174eb3f01acf5b7c90a");
            blessed_fortitude = Helpers.CreateFeature("WarpriestSacredFistBlessedFortitudeFeature",
                                                      "Blessed Fortitude",
                                                      "At 3rd level, a sacred fist can avoid even magical and unusual attacks with help from his deity. If he succeeds at a Fortitude saving throw against an attack that has a reduced effect on a successful save, he instead avoids the effect entirely. A helpless sacred fist does not gain the benefit of the blessed fortitude ability.",
                                                      "",
                                                      defensive_stance.Icon,
                                                      FeatureGroup.None,
                                                      Helpers.Create<Evasion>(e => e.SavingThrow = SavingThrowType.Fortitude)
                                                      );

            miraculous_fortitude = Helpers.CreateFeature("WarpriestSacredFistMiraculousFortitudeFeature",
                                          "Miraculous Fortitude",
                                          "At 9th level, the sacred fist’s blessed fortitude ability improves. He still takes no damage or negative effect when he succeeds at a Fortitude save, but now when he fails a Fortitude saving throw against a spell or effect that deals damage (including ability damage and drain), he takes only half the amount of damage. A helpless sacred fist does not gain the benefit of miraculous fortitude.",
                                          "",
                                          defensive_stance.Icon,
                                          FeatureGroup.None,
                                          Helpers.Create<ImprovedEvasion>(e => e.SavingThrow = SavingThrowType.Fortitude)
                                          );
        }


        static void createCultLeader()
        {
            //proficiencies
            var cult_leader_proficiencies = library.CopyAndAdd<BlueprintFeature>("33e2a7e4ad9daa54eaf808e1483bb43c", "CultLeaderProficiencies", "");
            cult_leader_proficiencies.SetName("Cult Leader Proficiencies");
            cult_leader_proficiencies.SetDescription("Cult leaders are proficient with all simple weapons, plus the hand crossbow, rapier, sap, shortbow, and short sword, as well as the favored weapon of their deity. They are proficient with light armor and light shields.");
            cult_leader_proficiencies.ReplaceComponent<AddProficiencies>(a =>
                                                                            {
                                                                                a.ArmorProficiencies = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.Buckler, ArmorProficiencyGroup.LightShield };
                                                                                //a.WeaponProficiencies = a.WeaponProficiencies.AddToArray(WeaponCategory.SpikedLightShield, WeaponCategory.WeaponLightShield);
                                                                            }
                                                                        );
            var skill_focus_persuation = library.Get<BlueprintFeature>("1621be43793c5bb43be55493e9c45924");

            var well_hidden = library.CopyAndAdd<BlueprintFeature>("610652378253d3845bb70f005c084daa", "CultLeaderWellHidden", "");
            well_hidden.ReplaceComponent<AddStatBonus>(c => { c.Descriptor = ModifierDescriptor.UntypedStackable; c.Value = 2; });
            well_hidden.SetName("Well Hidden");
            well_hidden.SetDescription("A cult leader gains a +2 bonus on Stealth checks.");
            well_hidden.Groups = new FeatureGroup[] { FeatureGroup.None};

            var sneak_attack = library.Get<BlueprintFeature>("9b9eac6709e1c084cb18c3a366e0ec87");

            var hide_in_plain_sight_ability = library.CopyAndAdd<BlueprintAbility>("b26a123a009d4a141ac9c19355913285", "CultLeaderHideInPlainSight", "");
            hide_in_plain_sight_ability.RemoveComponents<AbilityCasterIsOnFavoredTerrain>();
            hide_in_plain_sight_ability.SetName("Hide in Plain Sight");
            hide_in_plain_sight_ability.Type = AbilityType.Supernatural;
            hide_in_plain_sight_ability.SetDescription("At 12th level, a cult leader can use the Stealth skill even while being observed. As long as he is within 10 feet of an area of dim light, a cult leader can hide himself from view in the open without anything to actually hide behind. He cannot, however, hide in his own shadow.");
            var hide_in_plain_sight = Common.AbilityToFeature(hide_in_plain_sight_ability, false);


            cult_leader_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "CultLeaderArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Cult Leader");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Referred to as fanatics, lunatics, or obsessives, cultists see themselves as genuine devotees of their deity. And the hierarchs of those devotees, the cult leaders, are the most fanatical of them all. Cult leaders are known for turning reasonable hearts toward corrupted teachings and striking at those that get in the way of their agenda.");
            });
            Helpers.SetField(cult_leader_archetype, "m_ParentClass", warpriest_class);
            library.AddAsset(cult_leader_archetype, "");


            cult_leader_archetype.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, warpriest_proficiencies,
                                                                                        warpriest_fighter_feat_prerequisite_replacement),
                                                                    Helpers.LevelEntry(3, fighter_feat),
                                                                    Helpers.LevelEntry(4, warpriest_channel_energy),
                                                                    Helpers.LevelEntry(6, fighter_feat),
                                                                    Helpers.LevelEntry(9, fighter_feat),
                                                                    Helpers.LevelEntry(12, fighter_feat),
                                                                    Helpers.LevelEntry(15, fighter_feat),
                                                                    Helpers.LevelEntry(18, fighter_feat),
                                                                    };
            var rogue_talent = library.Get<BlueprintFeature>("c074a5d615200494b8f2a9c845799d93");
            cult_leader_archetype.AddFeatures = new LevelEntry[]{ Helpers.LevelEntry(1, cult_leader_proficiencies, well_hidden),
                                                                  Helpers.LevelEntry(3, sneak_attack),
                                                                  Helpers.LevelEntry(4, skill_focus_persuation),
                                                                  Helpers.LevelEntry(6, sneak_attack, rogue_talent),
                                                                  Helpers.LevelEntry(9, sneak_attack),
                                                                  Helpers.LevelEntry(12, hide_in_plain_sight, sneak_attack),
                                                                  Helpers.LevelEntry(15, sneak_attack),
                                                                  Helpers.LevelEntry(18, sneak_attack)
                                                                 };

            warpriest_progression.UIGroups[0].Features.Add(rogue_talent);
            warpriest_progression.UIGroups = warpriest_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(sneak_attack));

            UIGroup misc_group = Helpers.CreateUIGroup(well_hidden, skill_focus_persuation, hide_in_plain_sight);
            warpriest_progression.UIGroups = warpriest_progression.UIGroups.AddToArray(misc_group);

            warpriest_progression.UIDeterminatorsGroup = warpriest_progression.UIDeterminatorsGroup.AddToArray(cult_leader_proficiencies);

            cult_leader_archetype.ReplaceClassSkills = true;
            cult_leader_archetype.ClassSkills = warpriest_class.ClassSkills.AddToArray(StatType.SkillMobility, StatType.SkillStealth, StatType.SkillThievery, StatType.SkillPerception, StatType.SkillKnowledgeWorld);
            cult_leader_archetype.AddSkillPoints = 1;
        }


        static void createChampionOfTheFaith()
        {
            string[] alignment_names = new string[] { "Evil", "Good", "Chaotic", "Lawful" };
            string[] smite_names = new string[] { "Good", "Evil", "Law", "Chaos" };
            AlignmentComponent[] smite_alignment = new AlignmentComponent[] { AlignmentComponent.Good, AlignmentComponent.Evil, AlignmentComponent.Lawful, AlignmentComponent.Chaotic };
            AlignmentMaskType[] opposed_alignments = new AlignmentMaskType[] { AlignmentMaskType.Good, AlignmentMaskType.Evil, AlignmentMaskType.Lawful, AlignmentMaskType.Chaotic };

            BlueprintFeature[] alignment_features = new BlueprintFeature[]
            {
                library.Get<BlueprintFeature>("882521af8012fc749930b03dc18a69de"), //good
                library.Get<BlueprintFeature>("351235ac5fc2b7e47801f63d117b656c"), //evil
                library.Get<BlueprintFeature>("092714336606cfc45a37d2ab39fabfa8"), //law
                library.Get<BlueprintFeature>("8c7d778bc39fec642befc1435b00f613") //chaos
            };

            DamageAlignment[] damage_alignments = new DamageAlignment[] { DamageAlignment.Evil, DamageAlignment.Good, DamageAlignment.Chaotic, DamageAlignment.Lawful };

            BlueprintWeaponEnchantment[] enchants = new BlueprintWeaponEnchantment[]
            {
                library.Get<BlueprintWeaponEnchantment>("d05753b8df780fc4bb55b318f06af453"), //unholy
                library.Get<BlueprintWeaponEnchantment>("28a9964d81fedae44bae3ca45710c140"), //holy
                library.Get<BlueprintWeaponEnchantment>("57315bc1e1f62a741be0efde688087e9"), //chaotic
                library.Get<BlueprintWeaponEnchantment>("0ca43051edefcad4b9b2240aa36dc8d4") //axiomatic
            };

            UnityEngine.Sprite[] enchant_icons = new UnityEngine.Sprite[]
            {
                library.Get<BlueprintActivatableAbility>("561803a819460f34ea1fe079edabecce").Icon,//unholy
                library.Get<BlueprintActivatableAbility>("ce0ece459ebed9941bb096f559f36fa8").Icon,//holy
                library.Get<BlueprintActivatableAbility>("8ed07b0cc56223c46953348f849f3309").Icon,//chaotic
                library.Get<BlueprintActivatableAbility>("d76e8a80ab14ac942b6a9b8aaa5860b1").Icon,//axiomatic
            };

            UnityEngine.Sprite[] smite_icons = new UnityEngine.Sprite[]
            {
                    LoadIcons.Image2Sprite.Create(@"AbilityIcons/SmiteGood.png"),
                    library.Get<BlueprintAbility>("7bb9eb2042e67bf489ccd1374423cdec").Icon,//smite evil
                    LoadIcons.Image2Sprite.Create(@"AbilityIcons/SmiteNature.png"),
                    LoadIcons.Image2Sprite.Create(@"AbilityIcons/SmiteImpudence.png"),
            };

            BlueprintFeatureSelection chosen_alignment = Helpers.CreateFeatureSelection("ChosenAlignmentChampionOfFaithSelection",
                                                                                        "Chosen Alignment",
                                                                                        "At 1st level, a champion of the faith must select one of the following as his chosen alignment: chaos, evil, good, or law. This choice must be one of the alignments shared by the champion of the faith and his deity. Champions of the faith who are neutral with no other alignment components (or whose deity is) can choose any of the above alignments for this purpose. Additionally, a champion of the faith must select the blessing corresponding to his chosen alignment, even if it’s not on his deity’s list of domains.\n"
                                                                                        + "His chosen alignment’s opposite is referred to as his opposed alignment. Good and evil oppose one another, just as law and chaos oppose one another.\n"
                                                                                        + "As she gains levels, champion of the faith receives an array of abilities particularly suited to fight enemies of her opposed alignment.",
                                                                                        "",
                                                                                        null,
                                                                                        FeatureGroup.None);

            for (int i = 0; i < alignment_names.Length; i++)
            {
                var progression = createChosenAlignmentProgression(alignment_names[i], smite_names[i], opposed_alignments[i], alignment_features[i],
                                                                    damage_alignments[i], enchants[i], smite_alignment[i], enchant_icons[i], smite_icons[i]);
                chosen_alignment.AllFeatures = chosen_alignment.AllFeatures.AddToArray(progression);
            }


            champion_of_the_faith_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "ChampionOfTheFaithArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Champion of the Faith");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Champions of the faith are crusaders who use the power of their divine patron to annihilate the faith’s enemies.");
            });
            Helpers.SetField(champion_of_the_faith_archetype, "m_ParentClass", warpriest_class);
            library.AddAsset(champion_of_the_faith_archetype, "");


            champion_of_the_faith_archetype.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(3, fighter_feat),
                                                                    Helpers.LevelEntry(4, warpriest_sacred_weapon_enhancement, warpriest_channel_energy),
                                                                    Helpers.LevelEntry(8, warpriest_sacred_weapon_enhancement2),
                                                                    Helpers.LevelEntry(12, warpriest_sacred_weapon_enhancement3),
                                                                    Helpers.LevelEntry(16, warpriest_sacred_weapon_enhancement4),
                                                                    Helpers.LevelEntry(20, warpriest_sacred_weapon_enhancement5)
                                                                    };

            champion_of_the_faith_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, chosen_alignment) };
            warpriest_progression.UIDeterminatorsGroup = warpriest_progression.UIDeterminatorsGroup.AddToArray(chosen_alignment);
        }

        static BlueprintProgression createChosenAlignmentProgression(string alignment_name, string smite_name, AlignmentMaskType opposed_alignment, BlueprintFeature opposed_diety_feature,
                                                                      DamageAlignment damage_alignment, BlueprintWeaponEnchantment weapon_enchantment, AlignmentComponent smite_alignment,
                                                                      UnityEngine.Sprite enchant_icon, UnityEngine.Sprite smite_icon)
        {
            var true_seeing_buff = library.Get<BlueprintBuff>("09b4b69169304474296484c74aa12027");
            var detect_alignment_buff = Helpers.CreateBuff("ChampionOfTheFaithDetect" + smite_name + "Buff",
                                                           "Detected " + smite_name,
                                                           "At 3rd level, a champion of the faith can detect his opposed alignment. As a move action, the champion of the faith can focus on a single item or creature within 60 feet and determine whether it possesses the opposed alignment.",
                                                           "",
                                                           true_seeing_buff.Icon,
                                                           true_seeing_buff.FxOnStart);
            detect_alignment_buff.SetBuffFlags(BuffFlags.RemoveOnRest);
            var apply_detect_alignment_buff = Helpers.CreateConditional(Helpers.CreateContextConditionAlignment(smite_alignment),
                                                                       Common.createContextActionApplyBuff(detect_alignment_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true)
                                                                       );

            var detect_alignment_ability = Helpers.CreateAbility("ChampionOfTheFaithDetect" + smite_name + "Ability",
                                                                 "Detect " + smite_name,
                                                                 detect_alignment_buff.Description,
                                                                 "",
                                                                 detect_alignment_buff.Icon,
                                                                 AbilityType.SpellLike,
                                                                 CommandType.Move,
                                                                 AbilityRange.Medium,
                                                                 "",
                                                                 Helpers.savingThrowNone,
                                                                 Helpers.CreateRunActions(apply_detect_alignment_buff)
                                                                 );
            detect_alignment_ability.SpellResistance = false;
            detect_alignment_ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            var detect_alignment_feature = Common.AbilityToFeature(detect_alignment_ability, false);


            var weapon_focus = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");
            var aligned_weapon = Helpers.CreateFeature(damage_alignment.ToString() + "SacredWeaponFeature",
                                                       damage_alignment.ToString() + " Sacred Weapon",
                                                       "At 4th level, any sacred weapon wielded by a champion of the faith counts as having his chosen alignment for the purposes of overcoming damage reduction.",
                                                       "",
                                                       null,
                                                       FeatureGroup.None,
                                                       Helpers.Create<NewMechanics.AddOutgoingPhysicalDamageAlignmentIfParametrizedFeature>(c =>
                                                                                                                                            {
                                                                                                                                                c.required_parametrized_feature = weapon_focus;
                                                                                                                                                c.damage_alignment = damage_alignment;
                                                                                                                                            })
                                                       );

            var weapon_enchant_resource = Helpers.CreateAbilityResource(alignment_name + "ChampionOfTheFaithEnchantResource", "", "", "", null);
            weapon_enchant_resource.SetIncreasedByLevelStartPlusDivStep(1, 16, 1, 4, 1, 0, 0.0f, getWarpriestArray());

            var weapon_bond = library.Get<BlueprintAbility>("7ff088ab58c69854b82ea95c2b0e35b4");
            var enchant_weapon_buff = Helpers.CreateBuff(alignment_name + "ChampionOfTheFaithWeaponEnchantBuff",
                                                    weapon_enchantment.Name + " Weapon",
                                                    "At 12th level, once per day as a swift action, a champion of the faith can enhance any one sacred weapon with a weapon special ability based on his chosen alignment (anarchic for chaos, unholy for evil, holy for good, and axiomatic for law). This effect lasts for 1 minute. He can use this ability one additional time per day at 16th and 20th levels.",
                                                    "",
                                                    enchant_icon,
                                                    null,
                                                    Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), false, true, weapon_enchantment)
                                                   );

            var apply_enchant = Common.createContextActionApplyBuff(enchant_weapon_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var enchant_weapon_ability = Helpers.CreateAbility(alignment_name + "ChampionOfTheFaithWeaponEnchantAbility",
                                                               enchant_weapon_buff.Name,
                                                               enchant_weapon_buff.Description,
                                                               "",
                                                               enchant_weapon_buff.Icon,
                                                               AbilityType.Supernatural,
                                                               CommandType.Swift,
                                                               AbilityRange.Personal,
                                                               Helpers.oneMinuteDuration,
                                                               "",
                                                               Helpers.CreateRunActions(apply_enchant),
                                                               weapon_bond.GetComponent<AbilitySpawnFx>(),
                                                               Helpers.CreateResourceLogic(weapon_enchant_resource),
                                                               Helpers.Create<NewMechanics.AbilityCasterMainWeaponCheckHasParametrizedFeature>(c => c.feature = weapon_focus)
                                                               );
            enchant_weapon_ability.setMiscAbilityParametersSelfOnly();
            var enchant_weapon_feature = Common.AbilityToFeature(enchant_weapon_ability, false);
            enchant_weapon_feature.AddComponent(Helpers.CreateAddAbilityResource(weapon_enchant_resource));

            var smite_feature = Common.createSmite("ChampionOfTheFaithSmite" + smite_name,
                               "Smite " + smite_name,
                               "At 4th level, a champion of the faith can focus his powers against his chosen foes. As a swift action, the champion of the faith chooses one target within sight to smite. If this target is of his opposed alignment, the champion of the faith adds his Charisma bonus (if any) to his attack rolls and adds his warpriest level to all damage rolls made against the target of his smite. Smite attacks automatically bypass any DR the target possesses.\n"
                               + "In addition, while smite is in effect, the champion of the faith gains a deflection bonus equal to his Charisma modifier (if any) to his AC against attacks made by the target of the smite. If the smite targets a creature that’s not of the champion of the faith’s opposed alignment, the smite is wasted with no effect.\n"
                               + "The smite effect remains until the target of the smite is dead or the next time the champion of the faith regains spells. The champion of the faith can use this ability once per day, plus one additional time per day for every 4 levels beyond 4th (to a maximum of five times per day at 20th level). Using this ability consumes two uses of his fervor ability.",
                               "",
                               "",
                               smite_icon,
                               getWarpriestArray(),
                               smite_alignment);
            var smite_resource = Helpers.CreateAbilityResource(alignment_name + "ChampionOfTheFaithSmiteResource", "", "", "", null);
            smite_resource.SetIncreasedByLevelStartPlusDivStep(1, 8, 1, 4, 1, 0, 0.0f, getWarpriestArray());

            var smite_ability = smite_feature.GetComponent<AddFacts>().Facts.First() as BlueprintAbility;
            smite_feature.ReplaceComponent<AddAbilityResources>(c => c.Resource = smite_resource);
            smite_ability.ReplaceComponent<AbilityResourceLogic>(c => c.RequiredResource = smite_resource);
            var spend_fervor_resource = Helpers.Create <ContextActionOnContextCaster>(c => c.Actions = Helpers.CreateActionList(Common.createContextActionSpendResource(warpriest_fervor_resource, 2)));
            smite_ability.ReplaceComponent<AbilityEffectRunAction>(c => c.Actions = Helpers.CreateActionList(c.Actions.Actions.AddToArray(spend_fervor_resource)));
            smite_ability.AddComponent(Helpers.Create<NewMechanics.AbilityCasterHasResource>(c =>
                                                                                            {
                                                                                                c.resource = warpriest_fervor_resource;
                                                                                                c.amount = 2;
                                                                                            }
                                                                                            )
                                      );

            var progression = Helpers.CreateProgression(alignment_name + "AlignmentChampionOfTheFaithProgression",
                                                        "Chosen Alignment: " + alignment_name,
                                                        "At 1st level, a champion of the faith must select one of the following as his chosen alignment: chaos, evil, good, or law. This choice must be one of the alignments shared by the champion of the faith and his deity. Champions of the faith who are neutral with no other alignment components (or whose deity is) can choose any of the above alignments for this purpose. Additionally, a champion of the faith must select the blessing corresponding to his chosen alignment, even if it’s not on his deity’s list of domains.\n"
                                                        + "His chosen alignment’s opposite is referred to as his opposed alignment. Good and evil oppose one another, just as law and chaos oppose one another.",
                                                        "",
                                                        null,
                                                        FeatureGroup.None,
                                                        Helpers.PrerequisiteNoFeature(opposed_diety_feature),
                                                        Common.createPrerequisiteAlignment((~opposed_alignment) & AlignmentMaskType.Any)
                                                        );

            progression.LevelEntries = new LevelEntry[] { Helpers.LevelEntry(3, detect_alignment_feature),
                                                          Helpers.LevelEntry(4, aligned_weapon, smite_feature),
                                                          Helpers.LevelEntry(12, enchant_weapon_feature)
                                                        };
            progression.Classes = getWarpriestArray();
            progression.UIGroups = new UIGroup[] {Helpers.CreateUIGroup(detect_alignment_feature, smite_feature),
                                                  Helpers.CreateUIGroup(aligned_weapon, enchant_weapon_feature)
                                                 };

            return progression;
        }



        static void createFeralChampion()
        {
           
            feral_champion = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "FeralChampionArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Feral Champion");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "When a warpriest devotes himself to a god of the natural world, he is sometimes blessed with supernatural powers that allow him to evoke animalistic power and fury.");
            });
            Helpers.SetField(feral_champion, "m_ParentClass", warpriest_class);
            library.AddAsset(feral_champion, "");


            feral_champion.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, warpriest_blessings, warpriest_blessings),
                                                                    Helpers.LevelEntry(7, warpriest_sacred_armor),
                                                                    Helpers.LevelEntry(10, warpriest_sacred_armor2),
                                                                    Helpers.LevelEntry(13, warpriest_sacred_armor3),
                                                                    Helpers.LevelEntry(16, warpriest_sacred_armor4),
                                                                    Helpers.LevelEntry(19, warpriest_sacred_armor5)
                                                                    };
            createFeralBlessing();
            createSacredClaws();
            createWildShape();

            var wildshape_progression = Wildshape.addWildshapeProgression("FeralChampionWildshapeProgression",
                                                                          new BlueprintCharacterClass[] { warpriest_class },
                                                                          new BlueprintArchetype[0],
                                                                          new LevelEntry[]
                                                                          {
                                                                             Helpers.LevelEntry(7, wild_shape[0], wild_shape[1]), //wolf, leopard
                                                                             Helpers.LevelEntry(9, wild_shape[2], wild_shape[3]), //bear, dire wolf
                                                                             Helpers.LevelEntry(11, wild_shape[4], wild_shape[5], extra_wildshape), //smilodon, mastodon
                                                                             Helpers.LevelEntry(15, extra_wildshape),
                                                                             Helpers.LevelEntry(19, extra_wildshape)
                                                                          }
                                                                          );

            feral_champion.AddFeatures = new LevelEntry[]{ Helpers.LevelEntry(1, feral_blessing, sacred_claws, wildshape_progression)
                                                         };


            warpriest_progression.UIDeterminatorsGroup = warpriest_progression.UIDeterminatorsGroup.AddToArray(feral_blessing);
            warpriest_progression.UIGroups = warpriest_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(sacred_claws, wild_shape[0], wild_shape[2], wild_shape[4]));
            warpriest_progression.UIGroups = warpriest_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(wild_shape[1], wild_shape[3], wild_shape[5]));
        }


        static void createWildShape()
        {
            var druid = library.Get<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");
            //fix scaling
            foreach (var s in Wildshape.animal_wildshapes)
            {
                ClassToProgression.addClassToAbility(warpriest_class, new BlueprintArchetype[] { feral_champion }, s, druid);
                /*s.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(ContextRankBaseValueType.SummClassLevelWithArchetype,
                                                                                       classes: new BlueprintCharacterClass[] { druid, warpriest_class },
                                                                                       archetype: feral_champion)
                                                     );*/                    
            }

            wildshape_resource = library.Get<BlueprintAbilityResource>("ae6af4d58b70a754d868324d1a05eda4");
            extra_wildshape = library.Get<BlueprintFeature>("f78260b9a089ccc44b55f0fed08b1752");
                                                               
            string description = "At 7th level, a feral champion gains wild shape, as the druid ability of the same name, and treats his warpriest level – 3 as his effective druid level for the purposes of this ability. However, a feral champion does not gain the ability to take on elemental or plant forms with wild shape. A feral champion can use wild shape once per day at 7th level and one additional time per day every 4 levels thereafter, for a total of four times per day at 19th level.";            
            wild_shape = new BlueprintFeature[Wildshape.animal_wildshapes.Count];

            for (int i = 0; i < wild_shape.Length; i++)
            {

                wild_shape[i] = Helpers.CreateFeature("FeralChampion" + Wildshape.animal_wildshapes[i].name + "Feature",
                                                      Wildshape.animal_wildshapes[i].Name,
                                                      description,
                                                      "",
                                                      Wildshape.animal_wildshapes[i].Icon,
                                                      FeatureGroup.None,
                                                      Helpers.CreateAddFact(Wildshape.animal_wildshapes[i])
                                                      );
                if (i == 0)
                {
                    wild_shape[i].AddComponents(wildshape_resource.CreateAddAbilityResource(),
                                                Helpers.CreateAddFact(Wildshape.first_wildshape_form)
                                                );
                }
            }
        }


        static void createFeralBlessing()
        {
            var nature_blessing = blessings_map["Animal"];
            feral_blessing = Helpers.CreateFeatureSelection("FeralBlessingFeralChampionFeatureSelection",
                                                            "Feral Blessing",
                                                            "A feral champion must take the Animal blessing and does not gain a second blessing.",
                                                            "",
                                                            Helpers.GetIcon("0fd00984a2c0e0a429cf1a911b4ec5ca"),
                                                            FeatureGroup.Domain);
            feral_blessing.AllFeatures = new BlueprintFeature[] { nature_blessing };




            foreach (var d in warpriest_deity_selection.AllFeatures)
            {
                var add_facts = d.GetComponent<AddFacts>();
                if (add_facts?.Facts?.FirstOrDefault(a => a.AssetGuid == "9f05f9da2ea5ae44eac47d407a0000e5") == null) //animal blessing
                {
                    d.AddComponent(Common.prerequisiteNoArchetype(warpriest_class, feral_champion));
                }
            }
        }


        static void createSacredClaws()
        {
            sacred_claws = Helpers.CreateFeature("SacredClawsFeralChampionFeature",
                                                "Sacred Claws",
                                                "Rather than empowering a physical weapon, a feral champion grows claws as primary natural weapons on each hand. These claws deal damage as a warpriest’s sacred weapon and can be enhanced as such.",
                                                "",
                                                Helpers.GetIcon("f68af48f9ebf32549b5f9fdc4edfd475"),
                                                FeatureGroup.None,
                                                Common.createAddParametrizedFeatures(NewFeats.deity_favored_weapon, WeaponCategory.Claw)
                                                );
            var buff = Helpers.CreateBuff("SacredClawsFeralChampionBuff",
                                            sacred_claws.Name,
                                            sacred_claws.Description,
                                            "",
                                            sacred_claws.Icon,
                                            null,
                                            Common.createEmptyHandWeaponOverride(library.Get<BlueprintItemWeapon>("289c13ba102d0df43862a488dad8a5d5"))//claws 1d4
                                            );
            var toggle = Helpers.CreateActivatableAbility("SacredClawsFeralChampionToggle",
                                                          sacred_claws.name,
                                                          sacred_claws.Description,
                                                          "",
                                                          sacred_claws.Icon,
                                                          buff,
                                                          AbilityActivationType.Immediately,
                                                          CommandType.Free,
                                                          null
                                                          );
            sacred_claws.AddComponent(Helpers.CreateAddFact(toggle));
        }


        static void createDivineCommander()
        {

            divine_commander = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "DivineCommanderArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Divine Commander");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Some warpriests are called to lead great armies and face legions of foes. These divine commanders live for war and fight for glory. Their hearts quicken at battle cries, and they charge forth with their deity’s symbol held high. These leaders of armies do so to promote the agenda of their faith, and lead armies of devoted followers willing to give their lives for the cause.");
            });
            Helpers.SetField(divine_commander, "m_ParentClass", warpriest_class);
            library.AddAsset(divine_commander, "");

            divine_commander.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, add_warpriest_blessing_resource, warpriest_blessings, warpriest_blessings),
                                                                    Helpers.LevelEntry(3, fighter_feat),
                                                                    Helpers.LevelEntry(6, fighter_feat),
                                                                    Helpers.LevelEntry(12, fighter_feat),
                                                                    Helpers.LevelEntry(18, fighter_feat)
                                                                    };
            createAnimalCompanionAndBlessedCompanion();
            createTactician();

            divine_commander.AddFeatures = new LevelEntry[]{ Helpers.LevelEntry(1, animal_companion),
                                                           Helpers.LevelEntry(3, battle_tactician),
                                                           Helpers.LevelEntry(6, blessed_companion),
                                                           Helpers.LevelEntry(12, greater_battle_tactician),
                                                           Helpers.LevelEntry(18, master_battle_tactician),
                                                          };


            warpriest_progression.UIDeterminatorsGroup = warpriest_progression.UIDeterminatorsGroup.AddToArray(animal_companion);
            warpriest_progression.UIGroups = warpriest_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(battle_tactician, blessed_companion, greater_battle_tactician, master_battle_tactician));
        }


        static void createTactician()
        {
            var resource = Helpers.CreateAbilityResource("DivineCommanderTacticianResource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 9, 1, 6, 1, 0, 0.0f, getWarpriestArray());
            var ability = library.CopyAndAdd<BlueprintAbility>("f1c8ec6179505714083ed9bd47599268", "DivineCommanderBattleTactician", "");
            ability.SetNameDescription("Battle Tactician",
                                       "At 3rd level, a divine commander gains a teamwork feat as a bonus feat. She must meet the prerequisites for this feat. As a standard action, the divine commander can grant any teamwork feat to all allies within 30 feet who can see and hear her. Allies retain the use of this bonus feat for 4 rounds, plus 1 round for every 2 levels beyond 3rd that the divine commander possesses. Allies do not need to meet the prerequisites of this bonus feat. The divine commander can use this ability once per day at 3rd level, plus one additional time per day at 9th and 15th levels.");

            ability.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", getWarpriestArray()));
            ability.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = resource);

            battle_tactician = Helpers.CreateFeatureSelection("DivineCommanderBattleTacticianFeatureSelection",
                                                               ability.Name,
                                                               ability.Description,
                                                               "",
                                                               ability.Icon,
                                                               FeatureGroup.None,
                                                               Helpers.CreateAddFact(ability),
                                                               resource.CreateAddAbilityResource()
                                                               );
            var teamwork_feats = library.Get<BlueprintBuff>("a603a90d24a636c41910b3868f434447").GetComponent<TeamworkMechanics.AddFactsFromCasterIfHasBuffs>().facts.Cast<BlueprintFeature>().ToArray();

            foreach (var tf in teamwork_feats)
            {
                var add_comp = tf.GetComponent<AddFeatureIfHasFact>().CreateCopy(a => a.CheckedFact = battle_tactician);
                tf.AddComponent(add_comp);
            }

            battle_tactician.AllFeatures = library.Get<BlueprintFeatureSelection>("d87e2f6a9278ac04caeb0f93eff95fcb").AllFeatures;

            var comp = library.Get<BlueprintFeature>("4ca47c023f1c158428bd55deb44c735f").GetComponent<AutoMetamagic>().CreateCopy(a => a.Abilities = new BlueprintAbility[] { ability }.ToList());
            greater_battle_tactician = Helpers.CreateFeatureSelection("DivineCommanderGreaterBattleTacticianFeatureSelection",
                                           "Greater Battle Tactician",
                                           "At 12th level, the divine commander gains an additional teamwork feat as a bonus feat. She must meet the prerequisites for this feat. Additionally, using the battle tactician ability is now a swift action.",
                                           "",
                                           ability.Icon,
                                           FeatureGroup.None,
                                           comp
                                           );
            greater_battle_tactician.AllFeatures = battle_tactician.AllFeatures;


            master_battle_tactician = Helpers.CreateFeatureSelection("DivineCommanderMasterBattleTacticianFeatureSelection",
                                                               "Master Battle Tactician",
                                                               "At 18th level, the divine commander receives an additional teamwork feat as a bonus feat. He must meet the prerequisites for this feat. Whenever the divine commander uses the battle tactician ability, he grants any two teamwork feats that he knows.",
                                                               "",
                                                               ability.Icon,
                                                               FeatureGroup.None,
                                                               Common.createIncreaseActivatableAbilityGroupSize(ActivatableAbilityGroupExtension.TacticianTeamworkFeatShare.ToActivatableAbilityGroup())
                                                               );
            master_battle_tactician.AllFeatures = battle_tactician.AllFeatures;
        }


        static void createAnimalCompanionAndBlessedCompanion()
        {
            var animal_companion_progression = library.CopyAndAdd<BlueprintProgression>("924fb4b659dcb4f4f906404ba694b690",
                                                                                      "WarpriestAnimalCompanionProgression",
                                                                                      "");
            animal_companion_progression.Classes = getWarpriestArray();
            animal_companion = library.CopyAndAdd<BlueprintFeatureSelection>("2995b36659b9ad3408fd26f137ee2c67",
                                                                                            "AnimalCompanionSelectionWarpriest",
                                                                                            "");
            animal_companion.SetDescription("At 1st level, a divine commander gains the service of a loyal and trusty animal companion. This ability works as the druid class feature of the same name, using his warpriest level as his druid level.");
            var add_progression = Helpers.Create<AddFeatureOnApply>();
            add_progression.Feature = animal_companion_progression;
            animal_companion.ComponentsArray[0] = add_progression;

            blessed_companion = library.CopyAndAdd(Hunter.hunter_otherwordly_companion, "DivineCommanderBlessedCompanion", "");
            blessed_companion.SetNameDescription("Blessed Companion",
                                                 "At 6th level, a divine commander’s animal companion becomes a creature blessed by his deity. The divine commander’s mount gains either the celestial, entropic, fiendish, or resolute template, matching the alignment of the warpriest’s deity (celestial for good, entropic for chaotic, fiendish for evil, and resolute for lawful). If the deity matches more than one alignment, the divine commander can select which of the two templates the mount receives. Once the type of template is selected, it cannot be changed.\n If the divine commander’s deity is neutral with no other alignment components, divine commander can select any template.");
        }


        static void createArsenalChaplain()
        {

            arsenal_chaplain = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "ArsenalChaplainArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Arsenal Chaplain");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Arsenal chaplains are warpriests trained in the militant aspects of their gods.");
            });
            Helpers.SetField(arsenal_chaplain, "m_ParentClass", warpriest_class);
            library.AddAsset(arsenal_chaplain, "");

            arsenal_chaplain.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, warpriest_blessings, warpriest_blessings, warpriest_sacred_weapon_damage),
                                                                    Helpers.LevelEntry(4, warpriest_channel_energy, warpriest_sacred_weapon_enhancement),
                                                                    Helpers.LevelEntry(7, warpriest_sacred_armor),
                                                                    Helpers.LevelEntry(8, warpriest_sacred_weapon_enhancement2),
                                                                    Helpers.LevelEntry(10, warpriest_sacred_armor2),
                                                                    Helpers.LevelEntry(12, warpriest_sacred_weapon_enhancement3),
                                                                    Helpers.LevelEntry(13, warpriest_sacred_armor3),
                                                                    Helpers.LevelEntry(16, warpriest_sacred_armor4, warpriest_sacred_weapon_enhancement4),
                                                                    Helpers.LevelEntry(19, warpriest_sacred_armor5),
                                                                    Helpers.LevelEntry(20, warpriest_sacred_weapon_enhancement5),
                                                                    };
            createArsenalChaplainWarBlessing();
            createArsenalChaplainWeaponTraining();

            arsenal_chaplain.AddFeatures = new LevelEntry[]{ Helpers.LevelEntry(1, arsenal_chaplain_war_blessing, blessings_map["War"]),
                                                           Helpers.LevelEntry(5, arsenal_chaplain_weapon_training),
                                                           Helpers.LevelEntry(9, arsenal_chaplain_weapon_training),
                                                           Helpers.LevelEntry(13, arsenal_chaplain_weapon_training),
                                                           Helpers.LevelEntry(17, arsenal_chaplain_weapon_training),
                                                          };


            warpriest_progression.UIDeterminatorsGroup = warpriest_progression.UIDeterminatorsGroup.AddToArray(arsenal_chaplain_war_blessing, blessings_map["War"]);
        }


        static void createArsenalChaplainWarBlessing()
        {
            arsenal_chaplain_war_blessing = Helpers.CreateProgression("ArsenalChaplainWarBlessingFeature",
                                                                  "War Blessing",
                                                                  "An arsenal chaplain must choose War as his blessing, and can do so even if it is a domain not normally granted by his deity. He does not receive a second blessing.\n"
                                                                  + "At 7th level, an arsenal chaplain gains Quicken Blessing (War) as a bonus feat even if he does not meet the prerequisites.\n"
                                                                  + "At 13th level, an arsenal chaplain can use the War blessing on an ally within close range by spending an additional use of the blessing ability.\n"
                                                                  + "At 16th level, an arsenal chaplain can use the War blessing on all adjacent allies with a single use of the blessing ability.\n"
                                                                  + "At 19th level, an arsenal chaplain can use the War blessing on all allies within 30 feet by spending an additional use of the blessing ability.",
                                                                  "",
                                                                  Helpers.GetIcon("beffc11890fb54a48b855ef14f0a284e"),
                                                                  FeatureGroup.None
                                                                  );
            var quicken_war = Common.featureToFeature(quicken_blessing_selections["War"], false);
            quicken_war.SetNameDescriptionIcon(arsenal_chaplain_war_blessing);
            arsenal_chaplain_war_blessing.LevelEntries = new LevelEntry[]
            {
                Helpers.LevelEntry(7, quicken_war),
                Helpers.LevelEntry(13, (arsenal_chaplain_war_blessing_updates[0])),
                Helpers.LevelEntry(16, (arsenal_chaplain_war_blessing_updates[1])),
                Helpers.LevelEntry(19, (arsenal_chaplain_war_blessing_updates[2]))
            };
            arsenal_chaplain_war_blessing.Classes = getWarpriestArray();

            for (int i = 0; i < arsenal_chaplain_war_blessing_updates.Length; i++)
            {
                arsenal_chaplain_war_blessing_updates[i].HideInUI = false;
                arsenal_chaplain_war_blessing_updates[i].HideInCharacterSheetAndLevelUp = false;
                arsenal_chaplain_war_blessing_updates[i].SetNameDescriptionIcon(arsenal_chaplain_war_blessing);
            }
            arsenal_chaplain_war_blessing.UIGroups = Helpers.CreateUIGroups(arsenal_chaplain_war_blessing_updates.AddToArray(quicken_war));
        }


        static void createArsenalChaplainWeaponTraining()
        {
            var weapon_focus = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");
            arsenal_chaplain_weapon_training = Helpers.CreateFeature("ArsenalChaplainWeaponTrainingFeature",
                                                                  "Sacred Weapon Training",
                                                                  "At 5th level, an arsenal chaplain gains weapon training as per the fighter class feature, but the benefits of this weapon training apply only to the his sacred weapons (weapons with which the warpriest has taken Weapon Focus).",
                                                                  "",
                                                                  Helpers.GetIcon("b8cecf4e5e464ad41b79d5b42b76b399"),
                                                                  FeatureGroup.WeaponTraining,
                                                                  Helpers.Create<WeaponTraining>(),
                                                                  Helpers.Create<NewMechanics.WeaponTrainingIfHasParametrizedFeatures>(w => w.required_parametrized_features = new BlueprintParametrizedFeature[] { weapon_focus, NewFeats.deity_favored_weapon } )
                                                                  );
            arsenal_chaplain_weapon_training.Ranks = 10;
        }



    }
}
