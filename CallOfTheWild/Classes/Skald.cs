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
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.ElementsSystem;
using static Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility;
using Kingmaker.ResourceLinks;
using Kingmaker.Blueprints.Root;

namespace CallOfTheWild
{
   
    public class Skald
    {
        static internal bool test_mode = false;
        static LibraryScriptableObject library => Main.library;

        static public BlueprintCharacterClass skald_class;
        static public BlueprintProgression skald_progression;

        static public BlueprintFeature skald_proficiencies;
        static public BlueprintFeature damage_reduction;
        static public BlueprintFeature uncanny_dodge;
        static public BlueprintFeature improved_uncanny_dodge;
        static public BlueprintFeature fast_movement;
        static public BlueprintFeatureSelection versatile_performance;
        static public BlueprintFeature well_versed;
        static public BlueprintAbilityResource performance_resource;
        static public BlueprintFeature give_performance_resource;
        static public BlueprintFeature fake_barbarian;

        static public BlueprintBuff no_spell_casting_buff;
        static public BlueprintFeature skald_vigor_feat;
        static public BlueprintFeature greater_skald_vigor_feat;
        static public BlueprintBuff skald_vigor_buff;

        static public BlueprintFeature inspired_rage_feature;
        static public BlueprintActivatableAbility inspired_rage;
        static public BlueprintBuff inspired_rage_effect_buff;
        static public BlueprintFeature master_skald;
        static public BlueprintBuff master_skald_buff;
        static public BlueprintFeatureSelection skald_rage_powers;
        static public BlueprintFeature song_of_marching;
        static public BlueprintFeature song_of_strength;
        static public BlueprintFeature dirge_of_doom;
        static public BlueprintFeature song_of_the_fallen;
        static public BlueprintFeature lore_master;
        static public BlueprintFeature spell_kenning;
        static public BlueprintFeature spell_kenning_extra_use;
        static public BlueprintFeature bardic_knowledge;
        static public BlueprintFeature bardic_performance_move;
        static public BlueprintFeature bardic_performance_swift;

        static public BlueprintFeatureSelection extra_rage_power;


        static public BlueprintArchetype urban_skald_archetype;
        static public BlueprintFeature urban_skald_proficiencies;
        static public BlueprintFeature controlled_rage_feature;
        static public BlueprintBuff controlled_rage_str_buff;
        static public BlueprintBuff controlled_rage_con_buff;
        static public BlueprintBuff controlled_rage_dex_buff;
        static public BlueprintFeature infuriating_mockery;
        static public BlueprintFeature back_of_the_crowd;

        static public BlueprintArchetype herald_of_the_horn_archetype;
        static public BlueprintFeature bonded_object_feature;
        static public BlueprintFeature rousing_retort_feature;
        static public BlueprintFeature horn_call_feature;
        static public BlueprintFeature crumbling_blast_feature;

        static public BlueprintArchetype war_drummer_archetype;
        static public BlueprintFeature war_drummer_proficiencies;
        static public BlueprintFeature fearsome_mien_feature;
        static public BlueprintFeature deadly_rythm_feature;
        static public BlueprintFeatureSelection club_improved_critical_selection;

        static public BlueprintArchetype court_poet;
        static public BlueprintFeature insightful_contemplation;
        static public BlueprintBuff insightful_contemplation_buff;
        static public BlueprintFeature song_of_inspiration;
        static public BlueprintFeature handling_the_crowd;

        static public BlueprintArchetype sunsinger;
        static public BlueprintFeature pillar_of_light;
        static public BlueprintFeature channel_solar_energy;
        static public BlueprintFeature sunsinger_extra_channel;

        static public BlueprintArchetype red_tongue;
        static public BlueprintFeature[] seed_of_discord;
        static public BlueprintFeature rile;
        static public BlueprintFeatureSelection rogue_talents;
        static public BlueprintFeatureSelection great_orator;


        internal static void createSkaldClass()
        {
            Main.logger.Log("Skald class test mode: " + test_mode.ToString());
            var bard_class = library.TryGet<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");
            var barbarian_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f7d7eb166b3dd594fb330d085df41853");

            skald_class = Helpers.Create<BlueprintCharacterClass>();
            skald_class.name = "SkaldClass";
            library.AddAsset(skald_class, "");

            skald_class.LocalizedName = Helpers.CreateString("Skald.Name", "Skald");
            skald_class.LocalizedDescription = Helpers.CreateString("Skald.Description",
                                                                         "Skalds are poets, historians, and keepers of lore who use their gifts for oration and song to inspire allies into a frenzied rage. They balance a violent spirit with the veneer of civilization, recording events such as heroic battles and the deeds of great leaders, enhancing these stories in the retelling to earn bloodier victories in combat. A skald’s poetry is nuanced and often has multiple overlapping meanings, and he applies similar talents to emulate magic from other spellcasters.\n"
                                                                         + "Role: A skald inspires his allies, and often presses forward to fight enemies in melee. Outside of combat, he’s useful as a healer and scholar, less versatile but more durable than a bard."
                                                                         );
            skald_class.m_Icon = bard_class.Icon;
            skald_class.SkillPoints = barbarian_class.SkillPoints;
            skald_class.HitDie = DiceType.D8;
            skald_class.BaseAttackBonus = bard_class.BaseAttackBonus;
            skald_class.FortitudeSave = barbarian_class.FortitudeSave;
            skald_class.ReflexSave = barbarian_class.ReflexSave;
            skald_class.WillSave = bard_class.WillSave;
            skald_class.Spellbook = createSkaldSpellbook();
            skald_class.ClassSkills = new StatType[] {StatType.SkillAthletics, StatType.SkillMobility,
                                                      StatType.SkillKnowledgeArcana, StatType.SkillKnowledgeWorld, StatType.SkillLoreNature, StatType.SkillLoreReligion,
                                                      StatType.SkillPerception, StatType.SkillPersuasion, StatType.SkillUseMagicDevice}; //everything except stealth and trickety
            skald_class.IsDivineCaster = false;
            skald_class.IsArcaneCaster = true;
            skald_class.StartingGold = bard_class.StartingGold;
            skald_class.PrimaryColor = bard_class.PrimaryColor;
            skald_class.SecondaryColor = bard_class.SecondaryColor;
            skald_class.RecommendedAttributes = new StatType[] { StatType.Strength, StatType.Charisma, StatType.Constitution };
            skald_class.NotRecommendedAttributes = new StatType[0];
            skald_class.EquipmentEntities = bard_class.EquipmentEntities;
            skald_class.MaleEquipmentEntities = bard_class.MaleEquipmentEntities;
            skald_class.FemaleEquipmentEntities = bard_class.FemaleEquipmentEntities;
            skald_class.ComponentsArray =  bard_class.ComponentsArray;
            skald_class.StartingItems = barbarian_class.StartingItems;
            createSkaldsVigor();
            createSkaldProgression();
            skald_class.Progression = skald_progression;
            createUrbanSkaldArchetype();
            createHeraldOfTheHorn();
            createWarDrummer();
            createCourtPoet();
            createSunSinger();
            createRedTongue();
            skald_class.Archetypes = new BlueprintArchetype[] {urban_skald_archetype, herald_of_the_horn_archetype, war_drummer_archetype, court_poet, sunsinger, red_tongue};
            Helpers.RegisterClass(skald_class);
            addToPrestigeClasses(); //to at, mt, ek, dd
            fixExtraRagePower();
        }


        static void createSkaldsVigor()
        {
            skald_vigor_feat = Helpers.CreateFeature("SkaldsVigorFeature",
                                                     "Skald's Vigor",
                                                     "While maintaining a raging song, you gain fast healing equal to the Strength bonus your song provides, starting in the round after you begin the song.",
                                                     "",
                                                     LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Skalds_Vigor.png"),
                                                     FeatureGroup.Feat,
                                                     Helpers.PrerequisiteClassLevel(skald_class, 1)
                                                     );

            greater_skald_vigor_feat = Helpers.CreateFeature("GreaterSkaldsVigorFeature",
                                         "Greater Skald's Vigor",
                                         "Your allies share in the fast healing granted by your Skald’s Vigor, starting in the round when you begin your performance. They must be able to hear the performance.",
                                         "",
                                         LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Skalds_Vigor_Greater.png"),
                                         FeatureGroup.Feat,
                                         Helpers.PrerequisiteClassLevel(skald_class, 10),
                                         Helpers.PrerequisiteFeature(skald_vigor_feat)
                                         );

            library.AddFeats(skald_vigor_feat, greater_skald_vigor_feat);

            var stat_context_rank_config = Helpers.CreateContextRankConfig(type: AbilityRankType.StatBonus,
                                                               baseValueType: ContextRankBaseValueType.ClassLevel,
                                                               classes: getSkaldArray(),
                                                               progression: ContextRankProgression.OnePlusDivStep,
                                                               stepLevel: 8);

            skald_vigor_buff = Helpers.CreateBuff("SkaldVigorBuff",
                                                  "",
                                                  "",
                                                  "",
                                                  null,
                                                  null,
                                                  Common.createAddContextEffectFastHealing(Helpers.CreateContextValue(AbilityRankType.StatBonus), 2),
                                                  stat_context_rank_config);
            skald_vigor_buff.SetBuffFlags(BuffFlags.HiddenInUi);
        }


        static void addToPrestigeClasses()
        {
            Common.addReplaceSpellbook(Common.EldritchKnightSpellbookSelection, skald_class.Spellbook, "EldritchKnightSkald",
                                       Common.createPrerequisiteClassSpellLevel(skald_class, 3));
            Common.addReplaceSpellbook(Common.ArcaneTricksterSelection, skald_class.Spellbook, "ArcaneTricksterSkald",
                                        Common.createPrerequisiteClassSpellLevel(skald_class, 2));
            Common.addReplaceSpellbook(Common.MysticTheurgeArcaneSpellbookSelection, skald_class.Spellbook, "MysticTheurgeSkald",
                                        Common.createPrerequisiteClassSpellLevel(skald_class, 2));
            Common.addReplaceSpellbook(Common.DragonDiscipleSpellbookSelection, skald_class.Spellbook, "DragonDiscipleSkald",
                                      Common.createPrerequisiteClassSpellLevel(skald_class, 1));
        }


        static void fixExtraRagePower()
        {
            extra_rage_power = library.CopyAndAdd<BlueprintFeatureSelection>("0c7f01fbbe687bb4baff8195cb02fe6a", "SkaldExtraRagePower", "");
            extra_rage_power.ReplaceComponent<PrerequisiteFeature>(r => r.Feature = skald_rage_powers);

            extra_rage_power.SetNameDescription("Extra Rage Power (Skald)",
                                                "You gain one additional rage power, which can not be shared with raging song. You must meet all of the prerequisites for this rage power. This feat can be taken multiple times."
                                                );

            extra_rage_power.AllFeatures = new BlueprintFeature[] {library.Get<BlueprintFeature>("efb97e482f53f064dab85a9eeaf01085"), //guarded stance
                                                                   library.Get<BlueprintFeature>("efd53fe2887c3a54d86b99f4bba61dd6"), //protect vitals
                                                                   library.Get<BlueprintFeature>("6a9319f42f742024680af38af54f5d6f"), //reflexive dodge
                                                                   library.Get<BlueprintFeature>("c841ffa13d39ce442a408f57feb3cb8e"), //deadly accuracy feature
                                                                   library.Get<BlueprintFeature>("bb79cb9706379934c9460e46fe8cd04e"), //lethal accuracy
                                                                   library.Get<BlueprintFeature>("e4450dd9c06dc034fb7c0c08abcc202b"), //lethal stance
                                                                   library.Get<BlueprintFeature>("32c4d277007aed74c905779cd04a6fed"), //inspire ferocity
                                                                   library.Get<BlueprintFeature>("cb502c65dab407b4e928f5d8355cafc9"), //reckless stance
                                                                   library.Get<BlueprintFeature>("cb502c65dab407b4e928f5d8355cafc9"), //renewed vigor
                                                                   NewRagePowers.terrifying_howl_feature,
                                                                   NewRagePowers.taunting_stance,
                                                                   NewRagePowers.greater_atavism_totem,
                                                                   NewRagePowers.sharpened_accuracy,
                                                                   NewRagePowers.clear_mind
                                                                  };

            library.AddFeats(extra_rage_power);
            
        }

        static void createForbidSpellCastingBuff()
        {
            no_spell_casting_buff = Helpers.CreateBuff("SkaldForbidSpellCastingBuff",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   null,
                                                   Helpers.Create<ForbidSpellCasting>()
                                                   );
            no_spell_casting_buff.SetBuffFlags(BuffFlags.HiddenInUi);
        }

        static BlueprintCharacterClass[] getSkaldArray()
        {
            return new BlueprintCharacterClass[] { skald_class };
        }


        static void createSkaldProgression()
        {
            createFakeBarbarian();
            createForbidSpellCastingBuff();
            createVersatilePerformance();
            createWellVersed();
            createPerfromanceResource();
            createMasterSkald();
            createInspiredRage();
            createSkaldRagePowersFeature();
            createSkaldDamageReduction();
            createSongOfMarching();
            createSongOfStrength();
            createDirgeOfDoom();
            createSongOfTheFallen();
            createLoreMaster();
            createSpellKenning();


            skald_progression = Helpers.CreateProgression("SkaldProgression",
                           skald_class.Name,
                           skald_class.Description,
                           "",
                           skald_class.Icon,
                           FeatureGroup.None);
            skald_progression.Classes = getSkaldArray();

            skald_proficiencies = library.CopyAndAdd<BlueprintFeature>("acc15a2d19f13864e8cce3ba133a1979", //barbarian proficiencies
                                                                            "SkaldProficiencies",
                                                                            "");
            skald_proficiencies.AddComponent(Common.createArcaneArmorProficiency(Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.Buckler,
                                                                                      Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.LightShield,
                                                                                      Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.HeavyShield,
                                                                                      Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.TowerShield,
                                                                                      Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.Light,
                                                                                      Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.Medium)
                                                                                      );
            skald_proficiencies.SetName("Skald Proficiencies");
            skald_proficiencies.SetDescription("A skald is proficient with all simple and martial weapons, light and medium armor, and shields (except tower shields). A skald can cast skald spells while wearing light or medium armor and even using a shield without incurring the normal arcane spell failure chance. ");

            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");
            fast_movement = library.Get<BlueprintFeature>("d294a5dddd0120046aae7d4eb6cbc4fc");
            uncanny_dodge = library.Get<BlueprintFeature>("3c08d842e802c3e4eb19d15496145709");
            improved_uncanny_dodge = library.Get<BlueprintFeature>("485a18c05792521459c7d06c63128c79");

            var cantrips = library.CopyAndAdd<BlueprintFeature>("4f422e8490ec7d94592a8069cce47f98", //bard cantrips
                                                                     "SkaldCantripsFeature",
                                                                     ""
                                                                     );
            cantrips.SetDescription("Skalds learn a number of cantrips, or 0-level spells. These spells are cast like any other spell, but they do not consume any slots and may be used again.");
            cantrips.ReplaceComponent<BindAbilitiesToClass>(c => { c.CharacterClass = skald_class; });
            cantrips.ReplaceComponent<LearnSpells>(c => { c.CharacterClass = skald_class; });


            bardic_knowledge = library.CopyAndAdd<BlueprintFeature>("65cff8410a336654486c98fd3bacd8c5", "SkaldKnowledge", "");
            bardic_knowledge.SetDescription("A skald adds half his class level (minimum 1) to all Knowledge and Lore skill checks and may make all Knowledge and Lore skill checks untrained.");
            bardic_knowledge.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", getSkaldArray()));

            bardic_performance_move = library.CopyAndAdd<BlueprintFeature>("36931765983e96d4bb07ce7844cd897e", "SkaldMovePerformance", "");
            bardic_performance_move.SetName("Skald performance (Move Action)");
            bardic_performance_move.SetDescription("At 7th level, a skald can start a bardic performance as a move action instead of a standard action.");
            bardic_performance_swift = library.CopyAndAdd<BlueprintFeature>("fd4ec50bc895a614194df6b9232004b9", "SkaldSwiftPerformance", "");
            bardic_performance_swift.SetName("Skald performance (Swift Action)");
            bardic_performance_swift.SetDescription("At 13th level, a skald can start a bardic performance as a swift action.");

            skald_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, skald_proficiencies, fast_movement, detect_magic, cantrips, fake_barbarian,
                                                                                        give_performance_resource, inspired_rage_feature, bardic_knowledge,
                                                                                        library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                        library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")),  // touch calculate feature};
                                                                    Helpers.LevelEntry(2, versatile_performance, well_versed),
                                                                    Helpers.LevelEntry(3, skald_rage_powers, song_of_marching),
                                                                    Helpers.LevelEntry(4, uncanny_dodge),
                                                                    Helpers.LevelEntry(5, spell_kenning),
                                                                    Helpers.LevelEntry(6, skald_rage_powers, song_of_strength),
                                                                    Helpers.LevelEntry(7, versatile_performance, bardic_performance_move),
                                                                    Helpers.LevelEntry(8, improved_uncanny_dodge),
                                                                    Helpers.LevelEntry(9, skald_rage_powers, damage_reduction), 
                                                                    Helpers.LevelEntry(10, dirge_of_doom),
                                                                    Helpers.LevelEntry(11, spell_kenning_extra_use),
                                                                    Helpers.LevelEntry(12, versatile_performance, skald_rage_powers),
                                                                    Helpers.LevelEntry(13, bardic_performance_swift),
                                                                    Helpers.LevelEntry(14, damage_reduction, song_of_the_fallen), 
                                                                    Helpers.LevelEntry(15, skald_rage_powers),
                                                                    Helpers.LevelEntry(16),
                                                                    Helpers.LevelEntry(17, versatile_performance, spell_kenning_extra_use),
                                                                    Helpers.LevelEntry(18, skald_rage_powers), 
                                                                    Helpers.LevelEntry(19, damage_reduction),
                                                                    Helpers.LevelEntry(20, master_skald)
                                                                    };

            skald_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { skald_proficiencies, detect_magic, cantrips, bardic_knowledge };
            skald_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(versatile_performance, versatile_performance, versatile_performance, versatile_performance),
                                                         Helpers.CreateUIGroup(fast_movement, uncanny_dodge, improved_uncanny_dodge),
                                                         Helpers.CreateUIGroup(inspired_rage_feature, song_of_marching, song_of_strength, dirge_of_doom, song_of_the_fallen, master_skald, 
                                                                               bardic_performance_move, bardic_performance_swift),
                                                         Helpers.CreateUIGroup(well_versed, lore_master, spell_kenning, spell_kenning_extra_use, spell_kenning_extra_use)
                                                        };
        }


        static void createFakeBarbarian()
        {    //consider skald levels as that of barbarian
            var barbarian_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f7d7eb166b3dd594fb330d085df41853");
            fake_barbarian = Helpers.CreateFeature("SkaldFakeBarbarian",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Common.createClassLevelsForPrerequisites(barbarian_class, skald_class)
                                                   );
            fake_barbarian.HideInUI = true;
            fake_barbarian.HideInCharacterSheetAndLevelUp = true;

        }


        static void createWarDrummer()
        {
            war_drummer_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "WarDrummerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "War Drummer");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "These fierce drummers are equally adept at tapping out a driving rhythm and rapping enemies upside the head with the same massive clubs they use to beat the crude hide-covered drums they carry into battle.");
            });
            Helpers.SetField(war_drummer_archetype, "m_ParentClass", skald_class);
            library.AddAsset(war_drummer_archetype, "");
            war_drummer_archetype.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, bardic_knowledge, skald_proficiencies),
                                                                          Helpers.LevelEntry(7, versatile_performance)
                                                                        };
            createWarDrummerProficiencies();
            createFearsomeMien();
            createDeadlyRythm();
            createClubImprovedCriticalSelection();

            war_drummer_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, war_drummer_proficiencies, fearsome_mien_feature),
                                                                  Helpers.LevelEntry(3, deadly_rythm_feature),
                                                                  Helpers.LevelEntry(6, club_improved_critical_selection)
                                                                };


            skald_progression.UIGroups[0].Features.Add(war_drummer_proficiencies);
            skald_progression.UIGroups[0].Features.Add(club_improved_critical_selection);
            skald_progression.UIGroups[0].Features.Add(deadly_rythm_feature);

            skald_progression.UIDeterminatorsGroup = skald_progression.UIDeterminatorsGroup.AddToArray(war_drummer_proficiencies, fearsome_mien_feature);
        }


        static void createWarDrummerProficiencies()
        {
            war_drummer_proficiencies = library.CopyAndAdd<BlueprintFeature>(skald_proficiencies.AssetGuid, "SkaldWarDrummerProficiencies", "");
            war_drummer_proficiencies.ReplaceComponent<AddFacts>(c => c.Facts = c.Facts.RemoveFromArray(library.Get<BlueprintFeature>("203992ef5b35c864390b4e4a1e200629"))); //martial weapons
            war_drummer_proficiencies.AddComponent(Common.createAddWeaponProficiencies(WeaponCategory.Greatclub));
            war_drummer_proficiencies.SetName("War Drummer Proficiencies");
            war_drummer_proficiencies.SetDescription("A war drummer is proficient with all simple weapons and the greatclub.");
        }


        static void createFearsomeMien()
        {
            var confusion = library.Get<BlueprintAbility>("cf6c901fb7acc904e85c63b342e9c949");
            fearsome_mien_feature = library.CopyAndAdd<BlueprintFeature>(bardic_knowledge.AssetGuid, "SkaldWarDrummerFearsomeMien", "");
            fearsome_mien_feature.RemoveComponents<AddContextStatBonus>();
            fearsome_mien_feature.AddComponent(Helpers.CreateAddContextStatBonus(StatType.CheckBluff, ModifierDescriptor.UntypedStackable));
            fearsome_mien_feature.AddComponent(Helpers.CreateAddContextStatBonus(StatType.CheckIntimidate, ModifierDescriptor.UntypedStackable));
            fearsome_mien_feature.SetName("Fearsome Mien");
            fearsome_mien_feature.SetDescription("A war drummer adds 1/2 his class level (minimum 1) to all Intimidate and Bluff skill checks.");
            fearsome_mien_feature.SetIcon(confusion.Icon);
        }


        static void createDeadlyRythm()
        {
            deadly_rythm_feature = Helpers.CreateFeature("SkaldWarDrummerDeadlyRythmFeature",
                                                     "Deadly Rhythm",
                                                     " At 3rd level while under effect of inspired rage, the war drummer gains a +1 bonus on damage rolls for attacks made with clubs or greatclubs. At 7th level and every 4 levels thereafter, this bonus increases by 1 (to a maximum of +5)",
                                                     "",
                                                     null,
                                                     FeatureGroup.None
                                                     );

            var club = library.Get<BlueprintWeaponType>("26aa0672af2c7d84ba93bec37758c712");
            var great_club = library.Get<BlueprintWeaponType>("1b8c24cd1f9358c48839bb39266468c3");
            var deadly_rythm_buff = Helpers.CreateBuff("SkaldWarDrummerDeadlyRythmBuff",
                                                       deadly_rythm_feature.Name,
                                                       deadly_rythm_feature.Description,
                                                       "",
                                                       null,
                                                       null,
                                                       Common.createContextWeaponTypeDamageBonus(Helpers.CreateContextValue(AbilityRankType.Default), club, great_club),
                                                       Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getSkaldArray(),
                                                                                     type: AbilityRankType.Default, progression: ContextRankProgression.StartPlusDivStep,
                                                                                     startLevel: 3, stepLevel: 4)
                                                      );
            deadly_rythm_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var action = Helpers.CreateConditional(new Condition[] { Helpers.CreateConditionHasFact(deadly_rythm_feature), Common.createContextConditionIsCaster()}, 
                                                   Common.createContextActionApplyBuff(deadly_rythm_buff, Helpers.CreateContextDuration(), false, true, true));
            Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuff(inspired_rage_effect_buff, action);
        }


        static void createClubImprovedCriticalSelection()
        {
            var improved_critical = library.Get<BlueprintParametrizedFeature>("f4201c85a991369408740c6888362e20");
            var great_club_improved_critical_f = library.Get<BlueprintFeature>("9d0ffb9b55cb7324e99129a37168ea20");
            var club_improved_critical_f = library.Get<BlueprintFeature>("2447a3b507c32144bbe7fa6197a53e21");
            var great_club_improved_critical = Helpers.CreateFeature("WarDrummerImprovedCriticalGreatClubFeature",
                                                                      great_club_improved_critical_f.Name,
                                                                      great_club_improved_critical_f.Description,
                                                                      "",
                                                                      great_club_improved_critical_f.Icon,
                                                                      FeatureGroup.None,
                                                                      Common.createAddParametrizedFeatures(improved_critical, WeaponCategory.Greatclub)
                                                                      );
            great_club_improved_critical.HideInCharacterSheetAndLevelUp = true;
            var club_improved_critical = Helpers.CreateFeature("WarDrummerImprovedCriticalClubFeature",
                                                          club_improved_critical_f.Name,
                                                          club_improved_critical_f.Description,
                                                          "",
                                                          club_improved_critical_f.Icon,
                                                          FeatureGroup.None,
                                                          Common.createAddParametrizedFeatures(improved_critical, WeaponCategory.Club)
                                                          );
            club_improved_critical.HideInCharacterSheetAndLevelUp = true;

            //var great_club_improved_critical = library.Get<BlueprintFeature>("9d0ffb9b55cb7324e99129a37168ea20");
            //var club_improved_critical = library.Get<BlueprintFeature>("2447a3b507c32144bbe7fa6197a53e21");
            club_improved_critical_selection = Helpers.CreateFeatureSelection("SkaldWarDrummerImprovedCriticalSelection",
                                                                               "Improved Critical (Club or Greatclub)",
                                                                               "At 6th level, the war drummer gains Improved Critical with the club or the greatclub as a bonus feat.",
                                                                               "",
                                                                               improved_critical.Icon,
                                                                               FeatureGroup.None
                                                                               );
            club_improved_critical_selection.IgnorePrerequisites = true;
            club_improved_critical_selection.AllFeatures = new BlueprintFeature[] { great_club_improved_critical, club_improved_critical };
            club_improved_critical_selection.Features = club_improved_critical_selection.AllFeatures;
        }



        static void createHeraldOfTheHorn()
        {
            herald_of_the_horn_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "HeraldOfTheHornArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Herald of the Horn");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Even the loudest voice can often times be drowned out by the din of battle. Whether with the polished metal trumpet of a standing army or the crude curved animal horn of savage raiders, a herald of the horn sounds his raging song with thunderous blasts, which can bolster allies or shatter castle walls.");
            });
            Helpers.SetField(herald_of_the_horn_archetype, "m_ParentClass", skald_class);
            library.AddAsset(herald_of_the_horn_archetype, "");
            herald_of_the_horn_archetype.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(5, spell_kenning),
                                                                          Helpers.LevelEntry(7, versatile_performance),
                                                                          Helpers.LevelEntry(11, spell_kenning_extra_use),
                                                                          Helpers.LevelEntry(17, spell_kenning_extra_use),
                                                                        };
            createArcaneBond();
            createRousingRetort();
            createHornCall();
            createCrumblingBlast();

            herald_of_the_horn_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, bonded_object_feature),
                                                                  Helpers.LevelEntry(5, rousing_retort_feature),
                                                                  Helpers.LevelEntry(7, horn_call_feature),
                                                                  Helpers.LevelEntry(11, crumbling_blast_feature)
                                                                };


            skald_progression.UIGroups[2].Features.Add(rousing_retort_feature);
            skald_progression.UIGroups[3].Features.Add(bonded_object_feature);
            skald_progression.UIGroups[3].Features.Add(horn_call_feature);
            skald_progression.UIGroups[3].Features.Add(crumbling_blast_feature);
        }





        static void createArcaneBond()
        {
            var arcane_bond_ability = library.CopyAndAdd<BlueprintAbility>("e5dcf71e02e08fc448d9745653845df1", "SkaldHeraldOfTheHornBondedObjectAbility", "");
            arcane_bond_ability.SetDescription("A bonded object can be used once per day to restore any one spell that the Herald of the Horn had prepared for this day.");
            var resource = arcane_bond_ability.GetComponent<AbilityResourceLogic>().RequiredResource;
            arcane_bond_ability.ReplaceComponent<AbilityRestoreSpellSlot>(Helpers.Create<AbilityRestoreSpontaneousSpell>(a => a.SpellLevel = 10));

            bonded_object_feature = Helpers.CreateFeature("SkaldHeraldOfTheHornBondedObjectFeature",
                                                     arcane_bond_ability.Name,
                                                     arcane_bond_ability.Description,
                                                     "",
                                                     arcane_bond_ability.Icon,
                                                     FeatureGroup.None,
                                                     Helpers.CreateAddFact(arcane_bond_ability),
                                                     Helpers.CreateAddAbilityResource(resource)
                                                     );


        }


        static void createRousingRetort()
        {

            var break_enchantment = library.Get<BlueprintAbility>("7792da00c85b9e042a0fdfc2b66ec9a8");
            var rousing_retort = library.CopyAndAdd<BlueprintAbility>("55a037e514c0ee14a8e3ed14b47061de", "SkaldHeraldOfTheHornRousingRetortAbility", "");
            rousing_retort.SetIcon(break_enchantment.Icon);
            rousing_retort.SetName("Rousing Retort");
            rousing_retort.SetDescription("At 5th level, a herald of the horn can use raging song to free allies from enchantment effects and fear. He can expend 4 rounds of that ability to attempt a caster level check to remove any of such effects.");


            var action = Helpers.CreateRunActions(Common.createContextActionDispelMagic(SpellDescriptor.None,
                                                                                         new SpellSchool[] { SpellSchool.Enchantment },
                                                                                         Kingmaker.RuleSystem.Rules.RuleDispelMagic.CheckType.CasterLevel),
                                                  Common.createContextActionDispelMagic(SpellDescriptor.Fear,
                                                                                         new SpellSchool[0] ,
                                                                                         Kingmaker.RuleSystem.Rules.RuleDispelMagic.CheckType.CasterLevel)
                                     );
            rousing_retort.ReplaceComponent<AbilityEffectRunAction>(action);
            //rousing_retort.ReplaceComponent<AbilityEffectRunAction>(break_enchantment.GetComponent< AbilityEffectRunAction>());
            rousing_retort.RemoveComponents<SpellListComponent>();
            rousing_retort.RemoveComponents<SpellComponent>();
            rousing_retort.Range = AbilityRange.Personal;
            rousing_retort.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Omni;
            rousing_retort.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionOmni;
            rousing_retort.Type = AbilityType.Supernatural;
            rousing_retort.AddComponent(Helpers.CreateResourceLogic(performance_resource, true, 4));
            rousing_retort.LocalizedDuration = library.Get<BlueprintAbility>("2d81362af43aeac4387a3d4fced489c3").LocalizedDuration; //from firebal (instant)

            rousing_retort_feature = Helpers.CreateFeature("SkaldHeraldOfTheHornRousingRetortFeature",
                                                     rousing_retort.Name,
                                                     rousing_retort.Description,
                                                     "",
                                                     rousing_retort.Icon,
                                                     FeatureGroup.None,
                                                     Helpers.CreateAddFact(rousing_retort)
                                                     );
        }


        static void createHornCall()
        {
            var sound_burst = library.Get<BlueprintAbility>("c3893092a333b93499fd0a21845aa265");
            var increase_dc = Helpers.Create<NewMechanics.ContextIncreaseDescriptorSpellsDC>();
            increase_dc.Descriptor = SpellDescriptor.Sonic;
            increase_dc.Value = Helpers.CreateContextValue(AbilityRankType.Default);

            horn_call_feature = Helpers.CreateFeature("SkaldHeraldOfTheHornCallFeature",
                                                     "Horn Call",
                                                     "At 7th level, a herald’s horn enhances his sonic spells. If a skald spell with the sonic descriptor is cast using the horn, its DC increases by 1. These DCs increase by an additional 1 at 13th and 19th levels.",
                                                     "",
                                                     sound_burst.Icon,
                                                     FeatureGroup.None,
                                                     increase_dc,
                                                     Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getSkaldArray(),
                                                                                     type: AbilityRankType.Default, progression: ContextRankProgression.StartPlusDivStep,
                                                                                     startLevel: 7, stepLevel: 6)
                                                     );
        }


        static void createCrumblingBlast()
        {
            var shout_resource = Helpers.CreateAbilityResource("SkaldHeraldOfTheHornShoutResource", "", "", "", null);
            shout_resource.SetIncreasedByLevelStartPlusDivStep(1, 11, 0, 6, 1, 0, 0, getSkaldArray());
            var shout = library.CopyAndAdd<BlueprintAbility>("f09453607e683784c8fca646eec49162", "SkaldHeraldOfTheHornShoutAbility", "");
            shout.Type = AbilityType.Supernatural;
            shout.RemoveComponents<SpellComponent>();
            shout.RemoveComponents<SpellListComponent>();
            shout.AddComponent(Helpers.CreateResourceLogic(shout_resource));
            shout.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClass(skald_class, StatType.Charisma));

            var greater_shout = library.CopyAndAdd<BlueprintAbility>("fd0d3840c48cafb44bb29e8eb74df204", "SkaldHeraldOfTheHornGreaterShoutAbility", "");
            greater_shout.Type = AbilityType.Supernatural;
            greater_shout.RemoveComponents<SpellComponent>();
            greater_shout.RemoveComponents<SpellListComponent>();
            greater_shout.AddComponent(Helpers.CreateResourceLogic(shout_resource));
            greater_shout.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClass(skald_class, StatType.Charisma));

            crumbling_blast_feature = Helpers.CreateFeature("SkaldHeraldOfTheHornCrumblingBlastFeature",
                                                     "Crumbling Blast",
                                                     "At 11th level, a herald of the horn can use his horn to create a devastating shock wave of energy. Once per day, he can sound a note on the horn that functions like a Shout spell (DC = 10 + 1/2 the herald of the horn’s level + his Charisma bonus). At 17th level, the herald of the horn can use this ability twice per day and it functions like a Shout, Greater.",
                                                     "",
                                                     shout.Icon,
                                                     FeatureGroup.None,
                                                     Helpers.CreateAddAbilityResource(shout_resource),
                                                     Helpers.CreateAddFeatureOnClassLevel(Common.AbilityToFeature(shout), 17, getSkaldArray(),
                                                                                          new BlueprintArchetype[] { herald_of_the_horn_archetype }, true),
                                                     Helpers.CreateAddFeatureOnClassLevel(Common.AbilityToFeature(greater_shout), 17, getSkaldArray(),
                                                                                          new BlueprintArchetype[] { herald_of_the_horn_archetype }, false)
                                                     );
        }



        static void createSunSinger()
        {
            sunsinger = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "SunsingerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Sunsinger");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Qadiran sunsingers are particularly religious skalds of Sarenrae who call down their goddess’s glory to fill soldiers with fire.");
            });
            Helpers.SetField(sunsinger, "m_ParentClass", skald_class);
            library.AddAsset(sunsinger, "");
            sunsinger.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(3, song_of_marching),
                                                        Helpers.LevelEntry(5, spell_kenning),
                                                        Helpers.LevelEntry(11, spell_kenning_extra_use),
                                                        Helpers.LevelEntry(17, spell_kenning_extra_use),
                                                    };
            var deity_selection = library.CopyAndAdd<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4", "SunsingerDeitySelection", "");
            deity_selection.AllFeatures = new BlueprintFeature[] { library.Get<BlueprintFeature>("c1c4f7f64842e7e48849e5e67be11a1b") };//sarenrae
            deity_selection.SetNameDescription("Sarenrae","A sunsinger skald must be lawful good, neutral good, chaotic good or neutral, and must worship Sarenrae.");
            deity_selection.RemoveComponents<NoSelectionIfAlreadyHasFeature>();
            createPillarOfLight();
            createChannelSolarEnergy();

            sunsinger.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, deity_selection),
                                                                  Helpers.LevelEntry(5, channel_solar_energy),
                                                                  Helpers.LevelEntry(6, pillar_of_light),
                                                                };

            skald_progression.UIDeterminatorsGroup = skald_progression.UIDeterminatorsGroup.AddToArray(deity_selection);

            skald_progression.UIGroups = skald_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(channel_solar_energy, pillar_of_light));
        }

        static void createPillarOfLight()
        {
            var fascinate_ability = library.CopyAndAdd<BlueprintActivatableAbility>("993908ad3fb81f34ba0ed168b7c61f58", "SunsingerFascinateToggleAbility", "");
            fascinate_ability.SetDescription("At 6th level, aa sunsinger skald can use her raging song to call upon her goddess to imbue her with glory and make all who see it pay heed. A great beam of sunlight shines down upon the skald, casting bright light in a 30-foot radius, and allows the skald’s raging song to function as the fascinate bardic performance.");

            var fasciante_area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("a4fc1c0798359974e99e1d790935501d", "SunsingerFascianteArea", "");
            fasciante_area.Fx = Common.createPrefabLink("79cd602c3311fda459f1e7c62d7ec9a1"); //sund domain aura
            fasciante_area.ReplaceComponent<ContextCalculateAbilityParamsBasedOnClass>(c => c.CharacterClass = skald_class);
            var fascinate_buff = library.CopyAndAdd<BlueprintBuff>("555930f121b364a4e82670b433028728", "SunsingertFascianteBuff", "");
            fascinate_buff.SetDescription(fascinate_ability.Description);
            fascinate_buff.ReplaceComponent<AddAreaEffect>(a => a.AreaEffect = fasciante_area);
            fascinate_ability.Buff = fascinate_buff;
            pillar_of_light = Common.ActivatableAbilityToFeature(fascinate_ability, false);
        }


        static void createChannelSolarEnergy()
        {
            var resource = Helpers.CreateAbilityResource("SunsingerChannelEnergyResource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 11, 1, 6, 1, 0, 0.0f, getSkaldArray());

            var positive_energy_feature = library.Get<BlueprintFeature>("a79013ff4bcd4864cb669622a29ddafb");
           
            var context_rank_config = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.OnePlusDiv2,
                                                                                  type: AbilityRankType.Default, classes: getSkaldArray());
            var dc_scaling = Common.createContextCalculateAbilityParamsBasedOnClasses(getSkaldArray(), StatType.Charisma);
            var channel_positive_energy = Helpers.CreateFeature("SunsingerChannelPositiveEnergyFeature",
                                                                "Channel Solar Energy",
                                                                "At 5th level, a sunsinger skald can channel energy as a cleric once per day to heal wounds or harm undead like a good-aligned cleric. When she does so, she fills the area affected by the channeled energy with light, and can outline creatures in the area of effect as per faerie fire for a number of rounds equal to half her skald level. The sunsinger uses her skald level as her effective cleric level when channeling positive energy.",
                                                                "",
                                                                positive_energy_feature.Icon,
                                                                FeatureGroup.None,
                                                                Helpers.CreateAddAbilityResource(resource));

            var heal_living = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHeal,
                                                                      "SunsingerChannelEnergyHealLiving",
                                                                      "Channel Solar Energy — Heal Living",
                                                                      "Channeling positive energy causes a burst that heals all living creatures in a 30-foot radius centered on the sunsinger. The amount of damage healed is equal to 1d6 points of damage plus 1d6 points of damage for every two sunsinger levels beyond 1st (2d6 at 3rd, 3d6 at 5th, and so on).",
                                                                      "",
                                                                      context_rank_config,
                                                                      dc_scaling,
                                                                      Helpers.CreateResourceLogic(resource));

            var faerie_fire_buff = library.Get<BlueprintBuff>("cc383a9eaae4d2b45a925d442b367b54");
            var apply_faerie_fire = Common.createContextActionApplyBuff(faerie_fire_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Rounds), dispellable: false);
            var apply_effect = Helpers.CreateConditional(Helpers.Create<ContextConditionIsEnemy>(), apply_faerie_fire);

            var harm_undead = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHarm,
                                                          "SunsingerChannelEnergyHarmUndead",
                                                          "Channel Solar Energy — Harm Undead",
                                                          "Channeling positive energy causes a burst that damages all undead creatures in a 30-foot radius centered on the sunsinger. The amount of damage inflicted is equal to 1d6 points of damage plus 1d6 points of damage for every two sunsinger levels beyond 1st (2d6 at 3rd, 3d6 at 5th, and so on). Creatures that take damage from channeled energy receive a Will save to halve the damage. The DC of this save is equal to 10 + 1/2 the cleric's level + the cleric's Charisma modifier.",
                                                          "",
                                                          context_rank_config,
                                                          dc_scaling,
                                                          Helpers.CreateResourceLogic(resource));

            heal_living.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(a.Actions.Actions.AddToArray(apply_effect)));
            harm_undead.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(a.Actions.Actions.AddToArray(apply_effect)));

            var heal_living_base = Common.createVariantWrapper("SunsingerPositiveHealBase", "", heal_living);
            var harm_undead_base = Common.createVariantWrapper("SunsingerPositiveHarmBase", "", harm_undead);

            ChannelEnergyEngine.storeChannel(heal_living, channel_positive_energy, ChannelEnergyEngine.ChannelType.PositiveHeal);
            ChannelEnergyEngine.storeChannel(harm_undead, channel_positive_energy, ChannelEnergyEngine.ChannelType.PositiveHarm);

            channel_positive_energy.AddComponent(Helpers.CreateAddFacts(heal_living_base, harm_undead_base));
            channel_solar_energy = channel_positive_energy;
          
            sunsinger_extra_channel = ChannelEnergyEngine.createExtraChannelFeat(heal_living, channel_positive_energy, "ExtraChannelSunsinger", "Extra Channel (Sunsinger)", "");
        }


        static void createRedTongue()
        {
            red_tongue  = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "RedTongueArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Red Tongue");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Hot-blooded firebrands—referred to as red tongues in polite company—dominate by swaying emotions in the moment and wielding magic in the shadows.");
            });
            Helpers.SetField(red_tongue, "m_ParentClass", skald_class);
            library.AddAsset(red_tongue, "");
            red_tongue.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, bardic_knowledge),
                                                        Helpers.LevelEntry(2, versatile_performance),
                                                        Helpers.LevelEntry(7, versatile_performance),
                                                        Helpers.LevelEntry(12, versatile_performance),
                                                        Helpers.LevelEntry(17, versatile_performance),
                                                    };

            rile = Helpers.CreateFeature("RileFeature",
                                         "Rile",
                                         "The red tongue is particularly skilled at provoking others to action, even when ignorant on a subject. He adds 1/2 his skald level on Bluff checks and on Intimidate checks.",
                                         "",
                                         Helpers.GetIcon("76f8f23f6502def4dbefedffdc4d4c43"), //freeboter bane - command
                                         FeatureGroup.None,
                                         Helpers.CreateAddContextStatBonus(StatType.CheckBluff, ModifierDescriptor.UntypedStackable),
                                         Helpers.CreateAddContextStatBonus(StatType.CheckIntimidate, ModifierDescriptor.UntypedStackable),
                                         Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getSkaldArray(), progression: ContextRankProgression.Div2)
                                         );

            //great orator is only a place holder, it will be filled in verstile performance section
            great_orator = Helpers.CreateFeatureSelection("GreatOratorFeatureSelection",
                                                          "Great Orator",
                                                          $"Red tongue must select {LocalizedTexts.Instance.Stats.GetText(StatType.SkillPersuasion)} as his versatile performance choice at 2nd level.",
                                                          "",
                                                          null,
                                                          FeatureGroup.None
                                                          );

            rogue_talents = library.CopyAndAdd<BlueprintFeatureSelection>("94e2cd84bf3a8e04f8609fe502892f4f", "RedTongueRogueTalentSelection", "");

            var bonus_spells = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("fbdd8c455ac4cde4a9a3e18c84af9485"), //doom
                library.Get<BlueprintAbility>("ce4c4e52c53473549ae033e2bb44b51a"), //castigate
                library.Get<BlueprintAbility>("d2aeac47450c76347aebbc02e4f463e0"), //fear
                library.Get<BlueprintAbility>("d7cbd2004ce66a042aeab2e95a3c5c61"), //dominate
                NewSpells.command_greater,
                library.Get<BlueprintAbility>("2caa607eadda4ab44934c5c9875e01bc"), //doom
            };

            seed_of_discord = new BlueprintFeature[bonus_spells.Length];
            for (int i = 0; i < bonus_spells.Length; i++)
            {
                seed_of_discord[i] = Helpers.CreateFeature($"SeedOfDiscord{i}Feature",
                                    "Seed of Discord: " + bonus_spells[i].Name,
                                    "The fiery outlook of the red tongue imparts instinctual knowledge to invest arcane energy into his proclamations and denouncements. The red tongue gains the following bonus spells known as he reaches the appropriate level to cast each spell: doom (1st), castigate (2nd), fear (3rd), dominate person (4th), greater command (5th), mass eagle’s splendor (6th).\n"
                                    + bonus_spells[i].Name + ": " + bonus_spells[i].Description,
                                    Helpers.MergeIds("21ec54143ca945bab34520456bc7e5b5", bonus_spells[i].AssetGuid),
                                    bonus_spells[i].Icon,
                                    FeatureGroup.None,
                                    Helpers.CreateAddKnownSpell(bonus_spells[i], skald_class, i + 1)
                                    );
            }

            
                                                          
            red_tongue.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, rile, seed_of_discord[0]),
                                                        Helpers.LevelEntry(2, great_orator),
                                                        Helpers.LevelEntry(4, seed_of_discord[1]),
                                                        Helpers.LevelEntry(7, rogue_talents, seed_of_discord[2]),
                                                        Helpers.LevelEntry(10, seed_of_discord[3]),
                                                        Helpers.LevelEntry(12, rogue_talents),
                                                        Helpers.LevelEntry(13, seed_of_discord[4]),
                                                        Helpers.LevelEntry(16, seed_of_discord[5]),
                                                        Helpers.LevelEntry(19, rogue_talents)
                                                        };

            skald_progression.UIGroups = skald_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(rile, great_orator, rogue_talents, rogue_talents, rogue_talents));
            skald_progression.UIGroups = skald_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(seed_of_discord));
        }

        static void createCourtPoet()
        {
            court_poet = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "CourtPoetArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Court Poet");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Many courts are places of artistic refinement, attracting those performers who wish to revel in an aristocratic art scene. Such artists may aim to become a darling of the court, focusing on the aesthetic requirements of a particular tradition as well as learning details about that court’s history and culture. Court poets elevate the skald’s love of history and poetry to an aristocratic ideal, captivating courts with complicated poetic traditions and inspiring others with their craft. Some court poets go on to create their own works, weaving their magic and force of personality into their unique performances.");
            });
            Helpers.SetField(court_poet, "m_ParentClass", skald_class);
            library.AddAsset(court_poet, "");
            court_poet.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, inspired_rage_feature),
                                                                          Helpers.LevelEntry(2, well_versed),
                                                                          Helpers.LevelEntry(6, song_of_strength)
                                                                        };

            createInsightfulContemplation();
            createSongOfInspiration();
            createHandlingTheCrowd();

            court_poet.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, insightful_contemplation),
                                                                  Helpers.LevelEntry(2, handling_the_crowd),
                                                                  Helpers.LevelEntry(6, song_of_inspiration),
                                                                };

            skald_progression.UIGroups[1].Features.Add(handling_the_crowd);
            skald_progression.UIGroups[2].Features.Add(insightful_contemplation);
            skald_progression.UIGroups[2].Features.Add(song_of_inspiration);
        }


        static void createHandlingTheCrowd()
        {
            var crowd_ac_bonus = Common.createCrowdACBonus(2, 1);

            var mirror_image = library.Get<BlueprintAbility>("3e4ab69ada402d145a5e0ad3ad4b8564");
            handling_the_crowd = Helpers.CreateFeature("SkaldCourtPoetHandlingTheCrowdFeature",
                                                      "Handling The Crowd",
                                                      "At 2nd level, a court poet gains a +1 bonus to AC and on Perform checks when adjacent to two or more creatures. In addition she gains a bonus equal to 1/2 her skald level on Diplomacy checks.",
                                                      "",
                                                      mirror_image.Icon,
                                                      FeatureGroup.None,
                                                      crowd_ac_bonus,
                                                      Helpers.CreateAddContextStatBonus(StatType.CheckDiplomacy, ModifierDescriptor.UntypedStackable),
                                                      Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getSkaldArray(), progression: ContextRankProgression.Div2)
                                                      );
        }


        static internal void createInsightfulContemplation()
        {
            var ac_penalty = Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.UntypedStackable, ContextValueType.Rank, AbilityRankType.DamageBonus);
            var will_bonus = Helpers.CreateAddContextStatBonus(StatType.SaveWill, ModifierDescriptor.Morale, ContextValueType.Rank, AbilityRankType.Default);
            var ac_context_rank_config = Helpers.CreateContextRankConfig(type: AbilityRankType.DamageBonus,
                                                               baseValueType: ContextRankBaseValueType.FeatureListRanks,
                                                               progression: ContextRankProgression.BonusValue,
                                                               stepLevel: -1,
                                                               featureList: new BlueprintFeature[] { master_skald }
                                                               );
            var will_context_rank_config = Helpers.CreateContextRankConfig(type: AbilityRankType.Default,
                                                                           baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                           classes: getSkaldArray(),
                                                                           progression: ContextRankProgression.OnePlusDivStep,
                                                                           stepLevel: 4);
            var int_bonus = Helpers.CreateAddContextStatBonus(StatType.Intelligence, ModifierDescriptor.Morale, ContextValueType.Rank, AbilityRankType.StatBonus, 2);
            var cha_bonus = Helpers.CreateAddContextStatBonus(StatType.Charisma, ModifierDescriptor.Morale, ContextValueType.Rank, AbilityRankType.StatBonus, 2);


            var stat_context_rank_config = Helpers.CreateContextRankConfig(type: AbilityRankType.StatBonus,
                                                                           baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                           classes: getSkaldArray(),
                                                                           progression: ContextRankProgression.OnePlusDivStep,
                                                                           stepLevel: 8);

            var insightful_contemplation_debuff = Helpers.CreateBuff("InsightfulContemplationDebuff",
                                             "",
                                             "",
                                             "",
                                             null,
                                             null,
                                             Helpers.CreateAddStatBonus(StatType.SkillAthletics, -20, ModifierDescriptor.UntypedStackable),
                                             Common.createAbilityScoreCheckBonus(-20, ModifierDescriptor.UntypedStackable, StatType.Constitution)
                                             );
            insightful_contemplation_debuff.SetBuffFlags(BuffFlags.HiddenInUi);

            insightful_contemplation_buff = createRagingSongEffectBuffForbidSpellCasting("SkaldCourtPoetInsigtfulContemplationBuff", false, insightful_contemplation_debuff,
                                                                                                                                     int_bonus, cha_bonus, will_bonus, ac_penalty,
                                                                                                                                     stat_context_rank_config,
                                                                                                                                     will_context_rank_config, ac_context_rank_config);



            insightful_contemplation = Helpers.CreateFeature("SkalCourtPoetInsightfulContemplationFeature",
                                              "Insightful Contemplation",
                                              "At 1st level, affected allies gain a + 2 morale bonus to Intelligence and Charisma and a + 1 morale bonus on Will saving throws, but they also take a –1 penalty to AC. While under the effects of insightful contemplation, allies other than the court poet can’t use any Strength - based skills or make any physical effort that requires a Constitution check. At 4th level and every 4 skald levels thereafter, the song’s bonus on Will saves increases by 1; the penalty to AC doesn’t change. At 8th and 16th levels, the song’s bonuses to Intelligence and Charisma increase by 2. (Unlike the barbarian’s rage ability, those affected are not fatigued after the song ends.)",
                                              "",
                                              Helpers.GetIcon("6d3fcfab6d935754c918eb0e004b5ef7"), //inspire competence
                                              FeatureGroup.None
                                              );
            insightful_contemplation_buff.SetNameDescriptionIcon(insightful_contemplation.Name, insightful_contemplation.Description, insightful_contemplation.Icon);
            var inspire_courage = library.Get<BlueprintActivatableAbility>("5250fe10c377fdb49be449dfe050ba70");
            var rage_ability = Common.convertPerformance(inspire_courage, insightful_contemplation_buff, "SkaldCourtPoetInsightfulContemplationAbility");
            insightful_contemplation.AddComponent(Helpers.CreateAddFact(rage_ability));
        }


        static void createSongOfInspiration()
        {
            var buff = library.CopyAndAdd<BlueprintBuff>("1fa5f733fa1d77743bf54f5f3da5a6b1", "SkaldSongOfInspirationEffectBuff", "");
            buff.SetName("Song of Inspiration");
            buff.SetDescription("At 6th level, a court poet can use raging song to inspire her allies to greater mental clarity. Once each round while the court poet uses this performance, allies within 30 feet who can hear her can add 1/2 the court poet’s skald level to a single Wisdom check or Wisdom-based skill check.");
            buff.SetIcon(Helpers.GetIcon("4ebaf39efb8ffb64baf92784808dc49c"));
            buff.ComponentsArray = new BlueprintComponent[]{Helpers.CreateAddContextStatBonus(StatType.SkillPerception, ModifierDescriptor.UntypedStackable,
                                                                                                               ContextValueType.Rank, AbilityRankType.Default),
                                                            Helpers.CreateAddContextStatBonus(StatType.SkillLoreNature, ModifierDescriptor.UntypedStackable,
                                                                                                               ContextValueType.Rank, AbilityRankType.Default),
                                                            Helpers.CreateAddContextStatBonus(StatType.SkillLoreReligion, ModifierDescriptor.UntypedStackable,
                                                                                                               ContextValueType.Rank, AbilityRankType.Default),
                                                                             Common.createAbilityScoreCheckBonus(Helpers.CreateContextValue(AbilityRankType.Default),
                                                                                                                 ModifierDescriptor.UntypedStackable, StatType.Wisdom),
                                                                              Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                                             progression: ContextRankProgression.Div2,
                                                                                                             type: AbilityRankType.Default,
                                                                                                             classes: getSkaldArray()
                                                                                                             )
                                                                             };
            var ability = Common.convertPerformance(library.Get<BlueprintActivatableAbility>("430ab3bb57f2cfc46b7b3a68afd4f74e"), buff, "SkaldSongOfInspiration");
            ability.DeactivateIfCombatEnded = !test_mode;

            song_of_inspiration = Helpers.CreateFeature("SkaldSongOfInspirationFeature",
                                                     buff.Name,
                                                     buff.Description,
                                                     "",
                                                     buff.Icon,
                                                     FeatureGroup.None,
                                                     Helpers.CreateAddFact(ability)
                                                     );
        }



        static void createUrbanSkaldArchetype()
        {
            urban_skald_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "UrbanSkaldArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Urban Skald");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "The urban skald finds that challenging and mocking foes is sometimes more effective than inspiring uncontrolled rage in a city.");
            });
            Helpers.SetField(urban_skald_archetype, "m_ParentClass", skald_class);
            library.AddAsset(urban_skald_archetype, "");
            urban_skald_archetype.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, inspired_rage_feature, skald_proficiencies),
                                                                          Helpers.LevelEntry(3, song_of_marching),
                                                                          Helpers.LevelEntry(9, damage_reduction),
                                                                          Helpers.LevelEntry(10, dirge_of_doom),
                                                                          Helpers.LevelEntry(14, damage_reduction),
                                                                          Helpers.LevelEntry(19, damage_reduction)
                                                                        };
            urban_skald_proficiencies = library.CopyAndAdd<BlueprintFeature>("fa3d3b2211a51994785d85e753f612d3", //bard proficiencies
                                                                            "SkaldUrbanSkaldProficiencies",
                                                                            "");
            urban_skald_proficiencies.RemoveComponents<AddProficiencies>();
            urban_skald_proficiencies.ReplaceComponent<AddFacts>(c => c.Facts = c.Facts.AddToArray(library.Get<BlueprintFeature>("203992ef5b35c864390b4e4a1e200629"))); //add martial weapons

            urban_skald_proficiencies.SetName("Urban Skald Proficiencies");
            urban_skald_proficiencies.SetDescription("An urban skald is not proficient with medium armor.");

            createControlledRage();
            createInfuriatingMockery();
            createBackOfTheCrowd();

            urban_skald_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, controlled_rage_feature, urban_skald_proficiencies),
                                                                  Helpers.LevelEntry(3, back_of_the_crowd),
                                                                  Helpers.LevelEntry(10, infuriating_mockery),
                                                                };
            skald_progression.UIDeterminatorsGroup = skald_progression.UIDeterminatorsGroup.AddToArray(urban_skald_proficiencies);
            var bard_class = library.TryGet<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");
            urban_skald_archetype.ReplaceStartingEquipment = true;
            urban_skald_archetype.StartingItems = bard_class.StartingItems;

            skald_progression.UIGroups[1].Features.Add(back_of_the_crowd);
            skald_progression.UIGroups[2].Features.Add(controlled_rage_feature);
            skald_progression.UIGroups[2].Features.Add(infuriating_mockery);
        }


        static internal void createControlledRage()
        {
            var str_bonus = Helpers.CreateAddContextStatBonus(StatType.Strength, ModifierDescriptor.Morale, ContextValueType.Rank, AbilityRankType.StatBonus, 2);
            var con_bonus = Helpers.CreateAddContextStatBonus(StatType.Constitution, ModifierDescriptor.Morale, ContextValueType.Rank, AbilityRankType.StatBonus, 2);
            var dex_bonus = Helpers.CreateAddContextStatBonus(StatType.Dexterity, ModifierDescriptor.Morale, ContextValueType.Rank, AbilityRankType.StatBonus, 2);
           
            var stat_context_rank_config = Helpers.CreateContextRankConfig(type: AbilityRankType.StatBonus,
                                                                           baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                           classes: getSkaldArray(),
                                                                           progression: ContextRankProgression.OnePlusDivStep,
                                                                           stepLevel: 8);


            controlled_rage_dex_buff = createRagingSongEffectBuff("SkaldUrbanSkaldControlledRageDexBuff", null, dex_bonus, stat_context_rank_config);
            controlled_rage_str_buff = createRagingSongEffectBuff("SkaldUrbanSkaldControlledRageStrBuff", null, str_bonus, stat_context_rank_config);
            controlled_rage_con_buff = createRagingSongEffectBuff("SkaldUrbanSkaldControlledRageConBuff", null, con_bonus, stat_context_rank_config);

            var transmuter_con = library.Get<BlueprintActivatableAbility>("99cf556b967c2074ca284e127d815711");
            var transmuter_str = library.Get<BlueprintActivatableAbility>("c7773d1b408fea24dbbb0f7bf3eb864e");
            var transmuter_dex = library.Get<BlueprintActivatableAbility>("3553bda4d6dfe6344ad89b25f7be939a");

            (BlueprintBuff, string, UnityEngine.Sprite)[] rages = new (BlueprintBuff, string, UnityEngine.Sprite)[]
            {
                (controlled_rage_str_buff, "Strength", transmuter_str.Icon),
                (controlled_rage_dex_buff, "Dexterity", transmuter_dex.Icon),
                (controlled_rage_con_buff, "Constitution", transmuter_con.Icon),
            };

            controlled_rage_feature = Helpers.CreateFeature("SkaldUrbanSkaldControlledRageFeature",
                                              "Controlled Inspired Rage",
                                              "When the urban skald inspires rage, he does not grant the normal benefits.\n"
                                              + "Instead, he can apply a + 2 morale bonus to his allies’ Strength, Dexterity, or Constitution. This bonus increases to + 4 at 8th level and to + 6 at 16th level. The choice applies to all affected allies. The controlled inspired rage grants no bonus on Will saves, imposes no penalties to AC.",
                                              "",
                                              transmuter_str.Icon,
                                              FeatureGroup.None
                                              );
            var inspire_courage = library.Get<BlueprintActivatableAbility>("5250fe10c377fdb49be449dfe050ba70");

            foreach (var r in rages)
            {
                r.Item1.SetIcon(r.Item3);
                r.Item1.SetName($"Controlled Inspired Rage ({r.Item2})");
                r.Item1.SetDescription($"At 1st level, affected allies gain a +2 morale bonus to {r.Item2}. While under the effects of inspired rage, allies other than the skald cannot use any ability that requires patience or concentration. At 8th and 16th levels, the song’s bonus to {r.Item2} increases by 2. (Unlike the barbarian’s rage ability, those affected are not fatigued after the song ends.)");
                var rage_ability = Common.convertPerformance(inspire_courage, r.Item1, $"SkaldUrbanSkaldControlledRage{r.Item2}Ability");
                controlled_rage_feature.AddComponent(Helpers.CreateAddFact(rage_ability));
            }
        }


        static void createInfuriatingMockery()
        {
            var hideous_laughter = library.Get<BlueprintBuff>("4b1f07a71a982824988d7f48cd49f3f8");
            var buff = Helpers.CreateBuff("SkaldUrbanSkaldInfuriatingMockeryEffectBuff",
                                          "Infuriating Mockery",
                                          "At 10th level, the urban skald can inspire reckless fury in all foes within 30 feet. If they fail a Will saving throw, they take a –2 penalty to AC and on attack rolls, cannot use any Intelligence-, Dexterity-, or Charisma-based skills, and must succeed at a concentration check to cast spells (DC = 15 + spell level) for as long as they remain in range of the skald and the performance is maintained",
                                          "",
                                          hideous_laughter.Icon,
                                          hideous_laughter.FxOnStart,
                                          Helpers.CreateAddStatBonus(StatType.AC, -2, ModifierDescriptor.UntypedStackable),
                                          Helpers.CreateAddStatBonus(StatType.AdditionalAttackBonus, -2, ModifierDescriptor.UntypedStackable),
                                          Common.createAddCondition(UnitCondition.SpellCastingIsDifficult)
                                          );


            var area_effect = library.CopyAndAdd<BlueprintAbilityAreaEffect>("55c526a79761a3c48a3cc974a09bfef7", "SkaldUrbanSkaldInfuriatingMockeryArea", "");//frightening tune area
            var apply_buff = Common.createContextSavedApplyBuff(buff, Helpers.CreateContextDuration(), is_permanent: true);
            var area_action = Helpers.CreateAreaEffectRunAction(Helpers.CreateConditional(Helpers.Create<ContextConditionIsEnemy>(),
                                                                                          Common.createContextActionSavingThrow(SavingThrowType.Will, Helpers.CreateActionList(apply_buff))
                                                                                         ),
                                                                Helpers.CreateConditional(Helpers.Create<ContextConditionIsEnemy>(),
                                                                                          Common.createContextActionRemoveBuff(buff)
                                                                                         )
                                                               );

            area_effect.ReplaceComponent<AbilityAreaEffectRunAction>(area_action);
            var frightening_tune = library.Get<BlueprintActivatableAbility>("ad8a93dfa2db7ac4e85133b5e4f14a5f");
            var ability = Common.convertPerformance(frightening_tune, area_effect, "SkaldUrbanSkaldInfuriatingMockery", buff.Icon, buff.Name, buff.Description);
            ability.DeactivateIfCombatEnded = !test_mode;

            infuriating_mockery = Helpers.CreateFeature("SkaldUrbanSkaldInfuriatingMockeryFeature",
                                                     ability.Name,
                                                     ability.Description,
                                                     "",
                                                     ability.Icon,
                                                     FeatureGroup.None,
                                                     Helpers.CreateAddFact(ability)
                                                     );
        }


        static void createBackOfTheCrowd()
        {
            var crowd_ac_bonus = Common.createCrowdAlliesACBonus(2, Helpers.CreateContextValue(AbilityRankType.Default));
            var context_rank_config = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.StartPlusDivStep,
                                                                      classes: getSkaldArray(), startLevel: 3, stepLevel: 6, type: AbilityRankType.Default);

            var mirror_image = library.Get<BlueprintAbility>("3e4ab69ada402d145a5e0ad3ad4b8564");
            back_of_the_crowd = Helpers.CreateFeature("SkaldUrbanSkaldBackOfTheCrowd",
                                                      "Back of the Crowd",
                                                      "At 3rd level, an urban skald has learned to maximize the defensive benefit of being near allies. He gains a +1 dodge bonus to AC when adjacent to 2 or more allies. This bonus increases to +2 at 9th level and to +3 at 15th level.",
                                                      "",
                                                      mirror_image.Icon,
                                                      FeatureGroup.None,
                                                      crowd_ac_bonus,
                                                      context_rank_config
                                                      );
        }


        static void createSpellKenning()
        {
            var resource = Helpers.CreateAbilityResource("SkaldSpellKenningResource", "", "", "", null);
            resource.SetFixedResource(1);
            var cleric_class = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var wizard_class = library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");

            var classes = new (BlueprintCharacterClass, UnityEngine.Sprite)[] { (skald_class, library.Get<BlueprintFeature>("c459c8200e666ef4c990873d3e501b91").Icon), //transmutation
                                                                                (cleric_class, library.Get<BlueprintFeature>("30f20e6f850519b48aa59e8c0ff66ae9").Icon), //abjuration
                                                                                (wizard_class, library.Get<BlueprintFeature>("c46512b796216b64899f26301241e4e6").Icon)}; //evocation 
            const int max_level = 6;
            const int min_level = 1;
            const int max_variants_in_list = 100;
            const int max_spell_holders = 10;
            List<BlueprintComponent> spell_kenning_base = new List<BlueprintComponent>();

            var icon = library.Get<BlueprintAbility>("da2447792e4ced74c80f35e29eb7a9e8").Icon; //secret of s
            for (int i = min_level; i <= max_level; i++)
            {
                foreach (var c in classes)
                {
                    var spell_kenning_class_c = Helpers.CreateAbility($"SkaldSpellKenning{i}Base{c.Item1.name}PrototypeAbility",
                             $"Spell Kenning {Common.roman_id[i]}",
                             $"This ability allows to cast any {c.Item1.Name} spell of level {i}.",
                             "",
                             c.Item2,
                             AbilityType.Spell,
                             CommandType.Standard,
                             AbilityRange.Close,
                             "",
                             "");

                    var variant_spells = Common.CreateAbilityVariantsReplace(spell_kenning_class_c, "SkaldSpellKenning" + c.Item1.name,
                                                                                    (s, v) => {
                                                                                        s.ActionType = CommandType.Standard;
                                                                                        Helpers.SetField(s, "m_IsFullRoundAction", true);
                                                                                        if (s.GetComponent<AbilityResourceLogic>() == null)
                                                                                        {
                                                                                            s.AddComponent(Helpers.CreateResourceLogic(resource));
                                                                                        }
                                                                                    },
                                                                                    true,
                                                                                  c.Item1.Spellbook.SpellList.SpellsByLevel[i].Spells.Where(s => !NewSpells.getShadowSpells().Contains(s)).ToArray()
                                                                                  );

                    for (int j = 0; j < max_variants_in_list * max_spell_holders; j+= max_variants_in_list)
                    {
                        int id = j / max_variants_in_list + 1;
                        var spell_kenning_class = Helpers.CreateAbility($"SkaldSpellKenning{i}Base{c.Item1.name}{id}Ability",
                                                     spell_kenning_class_c.Name + $" ({c.Item1.Name}" + ((id == 1) ? ")" : $", {id})"),
                                                     spell_kenning_class_c.Description,
                                                     "",
                                                     c.Item2,
                                                     AbilityType.Spell,
                                                     CommandType.Standard,
                                                     AbilityRange.Personal,
                                                     "",
                                                     "");
                        Helpers.SetField(spell_kenning_class, "m_IsFullRoundAction", true);
                        if (j >= variant_spells.Length - 1)
                        {//we intentionally generate more holders to accomodate for additional spells
                         //and remove the ones that are not necessary
                            Action<UnitDescriptor> save_game_fix = delegate (UnitDescriptor unit)
                            {
                                foreach (var sb in unit.Spellbooks)
                                {
                                    sb.RemoveSpell(spell_kenning_class);
                                }
                            };
                            SaveGameFix.save_game_actions.Add(save_game_fix);
                            continue;
                        }
                        int num_spells = j + max_variants_in_list > variant_spells.Length - 1 ? variant_spells.Length - j : max_variants_in_list;
                        var variants_to_add = variant_spells.Skip(j).Take(num_spells).ToArray();
                        spell_kenning_class.AddComponent(Helpers.CreateAbilityVariants(spell_kenning_class, variants_to_add));
                        spell_kenning_base.Add(Helpers.CreateAddKnownSpell(spell_kenning_class, skald_class, i));
                    }         
                }
            }


            spell_kenning = Helpers.CreateFeature("SkaldSpellKenningFeature",
                                      "Spell Kenning",
                                      "At 5th level, a skald is learned in the magic of other spellcasters, and can use his own magic to duplicate those classes’ spells. Once per day, a skald can cast any spell on the bard, cleric, or sorcerer/wizard spell list as if it were one of his skald spells known, expending a skald spell slot of the same spell level to cast the desired spell. Casting a spell with spell kenning always has a minimum casting time of 1 full round, regardless of the casting time of the spell.\n"
                                       + "At 11th level, a skald can use this ability twice per day. At 17th level, he can use this ability three times per day.",
                                      "",
                                      icon,
                                      FeatureGroup.None,
                                      spell_kenning_base.ToArray().AddToArray(Helpers.CreateAddAbilityResource(resource))
                                      );

            spell_kenning_extra_use = Helpers.CreateFeature("SkaldSpellKenningExtraUseFeature",
                          "Spell Kenning Extra Use",
                          spell_kenning.Description,
                          "",
                          icon,
                          FeatureGroup.None,
                          Helpers.CreateIncreaseResourceAmount(resource, 1)
                          );
            spell_kenning_extra_use.Ranks = 5;
        }


        static void createLoreMaster()
        {
            lore_master = Helpers.CreateFeature("SkaldLoreMasterFeature",
                                                "Lore Master",
                                                "At 7th level skald receives + 1 bonus to all lore and knowledge skills, this bonus increases by one every 6 levels thereafter.",
                                                "",
                                                null,
                                                FeatureGroup.None,
                                                Helpers.CreateAddContextStatBonus(StatType.SkillKnowledgeArcana, ModifierDescriptor.UntypedStackable, ContextValueType.Rank),
                                                Helpers.CreateAddContextStatBonus(StatType.SkillKnowledgeWorld, ModifierDescriptor.UntypedStackable, ContextValueType.Rank),
                                                Helpers.CreateAddContextStatBonus(StatType.SkillLoreNature, ModifierDescriptor.UntypedStackable, ContextValueType.Rank),
                                                Helpers.CreateAddContextStatBonus(StatType.SkillLoreReligion, ModifierDescriptor.UntypedStackable, ContextValueType.Rank),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getSkaldArray(),
                                                                                progression: ContextRankProgression.StartPlusDivStep,
                                                                                startLevel: 7, stepLevel: 6)
                                               );
        }


        static void createSongOfTheFallen()
        {
            var song_of_the_fallen_ability = library.CopyAndAdd<BlueprintAbility>("80a1a388ee938aa4e90d427ce9a7a3e9", "SkaldSongOfTheFallen", "");
            song_of_the_fallen_ability.MaterialComponent = new MaterialComponentData();
            song_of_the_fallen_ability.SetName("Song of the fallen");
            song_of_the_fallen_ability.SetDescription("At 14th level, a skald can temporarily revive dead allies to continue fighting, with the same limitations as raise dead. The skald selects a dead ally within 30 feet and expends 6 rounds of raging song to bring that ally back to life for number of rounds equal to half her skald level. The revived ally is alive but staggered.");
            song_of_the_fallen_ability.Type = AbilityType.Supernatural;
            song_of_the_fallen_ability.Range = AbilityRange.Close;
            var staggered_buff = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");
            var buff = Helpers.CreateBuff("SkaldSongOfTheFallenEffectBuff",
                              song_of_the_fallen_ability.Name,
                              song_of_the_fallen_ability.Description,
                              "",
                              song_of_the_fallen_ability.Icon,
                              null
                              );
            song_of_the_fallen_ability.AddComponent(Common.createAbilityCasterHasNoFacts(NewSpells.silence_buff));
            buff.SetBuffFlags(BuffFlags.StayOnDeath);
            
            buff.AddComponent(Helpers.CreateAddFactContextActions(newRound: Common.createContextActionApplyBuff(staggered_buff, Helpers.CreateContextDuration(), is_child: true, dispellable: false, is_permanent: true),
                                                                  deactivated:Helpers.Create<Kingmaker.UnitLogic.Mechanics.Actions.ContextActionKillTarget>()
                                                                  )
                            );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);


            song_of_the_fallen_ability.ReplaceComponent<AbilityEffectRunAction>(c => c.Actions = Helpers.CreateActionList(c.Actions.Actions.AddToArray(apply_buff)));
            song_of_the_fallen_ability.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getSkaldArray(),
                                                                                    progression: ContextRankProgression.Div2, type: AbilityRankType.Default)
                                                                                    );
            song_of_the_fallen_ability.AddComponent(Helpers.CreateResourceLogic(performance_resource, true, 6));


            song_of_the_fallen = Helpers.CreateFeature("SkaldSongOfTheFallenFeature",
                                                     song_of_the_fallen_ability.Name,
                                                     song_of_the_fallen_ability.Description,
                                                     "",
                                                     song_of_the_fallen_ability.Icon,
                                                     FeatureGroup.None,
                                                     Helpers.CreateAddFact(song_of_the_fallen_ability)
                                                     );
        }


        static void createDirgeOfDoom()
        {
            var dirge_of_doom_ability = library.CopyAndAdd<BlueprintActivatableAbility>("d99d63f84e180d44e8f92b9a832c609d","SkaldDirgeOfDoomToggleAbility", "");
            dirge_of_doom_ability.SetDescription("At 10th level, a skald can create a sense of growing dread in his enemies, causing them to become shaken. This only affects enemies that are within 30 feet and able to hear the skald’s performance. The effect persists for as long as the enemy is within 30 feet and the skald continues his performance. This cannot cause a creature to become frightened or panicked, even if the targets are already shaken from another effect. This is a sonic mind-affecting fear effect, and relies on audible components.");
            dirge_of_doom = Helpers.CreateFeature("SkaldDirgeOfDoomFeature",
                                                  dirge_of_doom_ability.Name,
                                                  dirge_of_doom_ability.Description,
                                                  "",
                                                  dirge_of_doom_ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(dirge_of_doom_ability)
                                                  );
        }


        static void createSongOfStrength()
        {
            var strength_surge = library.Get<BlueprintAbility>("1d6364123e1f6a04c88313d83d3b70ee");
            var song_of_strength_buff = library.CopyAndAdd<BlueprintBuff>("1fa5f733fa1d77743bf54f5f3da5a6b1", "SkaldSongOfStrengthEffectBuff", "");
            song_of_strength_buff.SetName("Song of Strength");
            song_of_strength_buff.SetDescription("At 6th level, a skald can use raging song to inspire his allies to superhuman feats of strength. Once each round while the skald uses this performance, allies within 30 feet who can hear the skald may add 1/2 the skald’s level to a Strength check or Strength-based skill check.");
            song_of_strength_buff.SetIcon(strength_surge.Icon);
            song_of_strength_buff.ComponentsArray = new BlueprintComponent[]{Helpers.CreateAddContextStatBonus(StatType.SkillAthletics, ModifierDescriptor.UntypedStackable,
                                                                                                               ContextValueType.Rank, AbilityRankType.Default),
                                                                             Common.createAbilityScoreCheckBonus(Helpers.CreateContextValue(AbilityRankType.Default),
                                                                                                                 ModifierDescriptor.UntypedStackable, StatType.Strength),
                                                                              Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                                             progression: ContextRankProgression.Div2,
                                                                                                             type: AbilityRankType.Default,
                                                                                                             classes: getSkaldArray()
                                                                                                             )
                                                                             };
            var song_of_strength_ability = Common.convertPerformance(library.Get<BlueprintActivatableAbility>("430ab3bb57f2cfc46b7b3a68afd4f74e"), song_of_strength_buff, "SkaldSongOfStrength");
            song_of_strength_ability.DeactivateIfCombatEnded = !test_mode;

            song_of_strength = Helpers.CreateFeature("SkaldSongOfStrengthFeature",
                                                     song_of_strength_buff.Name,
                                                     song_of_strength_buff.Description,
                                                     "",
                                                     song_of_strength_buff.Icon,
                                                     FeatureGroup.None,
                                                     Helpers.CreateAddFact(song_of_strength_ability)
                                                     );
        }


        static void createSongOfMarching()
        {
            var longstrider = library.Get<BlueprintAbility>("14c90900b690cac429b229efdf416127");
            var song_of_marching_ability = library.CopyAndAdd<BlueprintAbility>("9b7fa6cadc0349449829873c63cc5b0b", "SkaldSongOfMarchingAbility", "");
            song_of_marching_ability.SetName("Song of Marching");
            song_of_marching_ability.SetDescription("At 3rd level, a skald can use raging song to inspire his allies to move faster without suffering from fatigue. By expending 4 rounds of raging song, the skald invigorates all allies, who may hustle (move 2 times faster on the global map) for the next 4 hours; this movement counts as a walk (not a hustle) for the purpose of accruing nonlethal damage and fatigue.") ;
            song_of_marching_ability.SetIcon(longstrider.Icon);
            song_of_marching_ability.ReplaceComponent<AbilityResourceLogic>(c => c.Amount = 4);
            song_of_marching_ability.ReplaceComponent<AbilitySpawnFx>(longstrider.GetComponent<AbilitySpawnFx>());

            var song_of_marching_buff = Helpers.CreateBuff("SkaldSongOfMarchingBuff",
                                                           song_of_marching_ability.Name,
                                                           song_of_marching_ability.Description,
                                                           "",
                                                           song_of_marching_ability.Icon,
                                                           null,
                                                           Helpers.Create<TravelMap.OverrideGlobalMapTravelMultiplier>(o => o.travel_map_multiplier = 2)
                                                           );
            song_of_marching_buff.Stacking = StackingType.Stack;
            song_of_marching_buff.SetBuffFlags(BuffFlags.RemoveOnRest);
            var apply_buff = Common.createContextActionApplyBuff(song_of_marching_buff,
                                                                 Helpers.CreateContextDuration(Common.createSimpleContextValue(4), DurationRate.Hours),
                                                                 dispellable: false);
            song_of_marching_ability.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(apply_buff));
            song_of_marching_ability.RemoveComponents<ContextRankConfig>();
            song_of_marching_ability.AddComponent(Common.createAbilityCasterHasNoFacts(NewSpells.silence_buff));

            song_of_marching = Helpers.CreateFeature("SkaldSongOfMarchingFeature",
                                                     song_of_marching_ability.Name,
                                                     song_of_marching_ability.Description,
                                                      "",
                                                      song_of_marching_ability.Icon,
                                                      FeatureGroup.None,
                                                      Helpers.CreateAddFact(song_of_marching_ability)
                                                     );
        }


        static void createSkaldRagePowersFeature()
        {
            var barbarian_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f7d7eb166b3dd594fb330d085df41853");
            skald_rage_powers = library.CopyAndAdd<BlueprintFeatureSelection>("28710502f46848d48b3f0d6132817c4e", "SkaldRagePowerSelection", "");
            skald_rage_powers.SetDescription("At 3rd level and every 3 levels thereafter, a skald learns a rage power that affects the skald and any allies under the influence of his inspired rage. This cannot be a rage power that requires the creature to spend a standard action or rounds of rage to activate it, nor can it be a stance power.\n"
                                             + "When starting an inspired rage, the skald adds rage powers (if any) to the song, and all affected allies gain the benefit of these rage powers, using the skald’s level as their effective barbarian level. The skald uses his skald level as his barbarian level for the purpose of selecting rage powers that require a minimum barbarian level.") ;
            skald_rage_powers.AddComponent(Common.createAddFeatureIfHasFact(fake_barbarian, fake_barbarian, true)); //for save compatibility

            //fix rage buffs to work with skald
            BlueprintBuff[] buffs_to_fix = new BlueprintBuff[]{library.Get<BlueprintBuff>("ec7db4946877f73439c4ee661f645452"), //beast totem ac buff
                                                               library.Get<BlueprintBuff>("3858dd3e9a94f0b41abdc58387d68ccf"), //guarded stance
                                                               library.Get<BlueprintBuff>("5b5e566167a3f2746b7d3a26bc8a50a6"), //guarded stance protect vitals
                                                               library.Get<BlueprintBuff>("b209f567dc78a1440aad52d4138c5f27"), //reflexive dodge
                                                               library.Get<BlueprintBuff>("0c6e198a78210954c9fe245a26b0c315"), //deadly accuracy
                                                               library.Get<BlueprintBuff>("9ec69854596674a4ba40802e6337894d"), //inspire ferocity buff
                                                               library.Get<BlueprintBuff>("c6271b3183c48d54b8defd272bea0665"), //lethal stance
                                                               library.Get<BlueprintBuff>("a8a733d2605c66548b652f312ea4dbf3"), //reckless stance
                                                               NewRagePowers.greater_celestial_totem_buff,
                                                               NewRagePowers.superstition_buff,
                                                               NewRagePowers.ghost_rager_buff,
                                                               NewRagePowers.witch_hunter_buff,
                                                              };
            buffs_to_fix = buffs_to_fix.AddToArray(NewRagePowers.energy_resistance_buff);

            foreach (var b in buffs_to_fix)
            {
                var c = b.GetComponent<ContextRankConfig>();
                BlueprintCharacterClass[] classes = Helpers.GetField<BlueprintCharacterClass[]>(c, "m_Class");
                classes = classes.AddToArray(skald_class);
                Helpers.SetField(c, "m_Class", classes);
                //replaceContextConditionHasFactToContextConditionCasterHasFact(b);
            }
            var renewed_vigor = library.Get<BlueprintAbility>("5a25185dbf75a954580a1248dc694cfc");
            var context_rank_configs = renewed_vigor.GetComponents<ContextRankConfig>();
            foreach (var config in context_rank_configs)
            {
                var t = Helpers.GetField<ContextRankBaseValueType>(config, "m_BaseValueType");
                if (t == ContextRankBaseValueType.ClassLevel)
                {
                    BlueprintCharacterClass[] classes = Helpers.GetField<BlueprintCharacterClass[]>(config, "m_Class");
                    classes = classes.AddToArray(skald_class);
                    Helpers.SetField(config, "m_Class", classes);
                }
            }

            var howl_scaling = NewRagePowers.terrifying_howl_ability.GetComponent<NewMechanics.ContextCalculateAbilityParamsBasedOnClasses>();
            howl_scaling.CharacterClasses = howl_scaling.CharacterClasses.AddToArray(skald_class);
        }


        static void createSkaldDamageReduction()
        {
            var damage_reduction_barbarian = library.Get<BlueprintFeature>("cffb5cddefab30140ac133699d52a8f8");
            damage_reduction = library.CopyAndAdd<BlueprintFeature>("cffb5cddefab30140ac133699d52a8f8", "SkaldDamageReduction", "");
            damage_reduction.SetDescription("At 9th level, a skald gains damage reduction. Subtract 1 from the damage the skald takes each time he is dealt damage from a weapon or a natural attack.At 14th and 19th levels, this damage reduction increases by 1. Damage Reduction can reduce damage to 0, but not below 0.Additionally, the skald grants this DR to all allies affected by his inspired rage.");

            var c = damage_reduction.GetComponent<ContextRankConfig>();
            BlueprintFeature[] feats = Helpers.GetField<BlueprintFeature[]>(c, "m_FeatureList");
            feats = feats.AddToArray(damage_reduction);
            Helpers.SetField(c, "m_FeatureList", feats);
            
            //allow to dr share feats (only from skald)
            var dr_share_buff = Helpers.CreateBuff("SkaldSharedDr",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          damage_reduction.GetComponent<AddDamageResistancePhysical>(),
                                          Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureListRanks,
                                                                          featureList: new BlueprintFeature[]{ damage_reduction})
                                          );
            dr_share_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var share_dr_condition = Helpers.CreateConditional(new Condition[] { Common.createContextConditionIsCaster(not: true) },
                                                             Common.createContextActionApplyBuff(dr_share_buff, Helpers.CreateContextDuration(), false, true, true),
                                                             null
                                                            );

            Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuff(inspired_rage_effect_buff, share_dr_condition);
        }



        static void createVersatilePerformance()
        {
            versatile_performance = library.CopyAndAdd<BlueprintFeatureSelection>("94e2cd84bf3a8e04f8609fe502892f4f", "SkaldVersatilePerformance", "");
            versatile_performance.SetName("Skald Talent");
            versatile_performance.SetDescription("As a skald gains experience, she learns a number of talents that aid her and confound her foes. At 2nd level, a skald gains a rogue talent, as the rogue class feature of the same name. At 6th level and every 4 levels thereafter, the skald gains an additional rogue talent. A skald cannot select a rogue talent that modifies the sneak attack ability.");
        }

        static void createWellVersed()
        {
            well_versed = library.CopyAndAdd<BlueprintFeature>("8f4060852a4c8604290037365155662f", "SkaldWelLVersed", "");
            well_versed.SetDescription("At 2nd level, the skald becomes resistant to the bardic performance of others, and to sonic effects in general. The bard gains a +4 bonus on saving throws made against bardic performance, sonic, and language-dependent effects.");
        }


        static void createPerfromanceResource()
        {
            performance_resource = library.Get<BlueprintAbilityResource>("e190ba276831b5c4fa28737e5e49e6a6");
            var amount = Helpers.GetField(performance_resource, "m_MaxAmount");
            BlueprintCharacterClass[] classes = (BlueprintCharacterClass[])Helpers.GetField(amount, "Class");
            classes = classes.AddToArray(skald_class);
            Helpers.SetField(amount, "Class", classes);
            Helpers.SetField(performance_resource, "m_MaxAmount", amount);

            give_performance_resource = library.CopyAndAdd<BlueprintFeature>("b92bfc201c6a79e49afd0b5cfbfc269f", "SkaldPerformanceResourceFact", "");
            give_performance_resource.ReplaceComponent<IncreaseResourcesByClass>(c => { c.CharacterClass = skald_class;});

            //extra performance feat is fixed automatically
        }


        static BlueprintSpellbook createSkaldSpellbook()
        {
            var bard_class = library.TryGet<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");
            var skald_spellbook = Helpers.Create<BlueprintSpellbook>();
            skald_spellbook.name = "SkaldSpellbook";
            library.AddAsset(skald_spellbook, "");
            skald_spellbook.Name = skald_class.LocalizedName;
            skald_spellbook.SpellsPerDay = bard_class.Spellbook.SpellsPerDay;

            skald_spellbook.SpellsKnown = bard_class.Spellbook.SpellsKnown;
            skald_spellbook.Spontaneous = true;
            skald_spellbook.IsArcane = true;
            skald_spellbook.AllSpellsKnown = false;
            skald_spellbook.CanCopyScrolls = false;
            skald_spellbook.CastingAttribute = StatType.Charisma;
            skald_spellbook.CharacterClass = skald_class;
            skald_spellbook.CasterLevelModifier = 0;
            skald_spellbook.CantripsType = CantripsType.Cantrips;
            skald_spellbook.SpellsPerLevel = 0;
            skald_spellbook.SpellList = bard_class.Spellbook.SpellList;
           
            return skald_spellbook;
        }


        static void createMasterSkald()
        {
            var inspire_greatness = library.Get<BlueprintFeature>("9ae0f32c72f8df84dab023d1b34641dc");
            master_skald = Helpers.CreateFeature("SkaldMasterSkald",
                                                 "Master Skald",
                                                 "At 20th level, a skald’s inspired rage no longer gives allies a penalty to AC, nor limits what skills or abilities they can use. Finally, when making a full attack, affected allies may make an additional attack each round (as if using a haste effect).",
                                                 "",
                                                 inspire_greatness.Icon,
                                                 FeatureGroup.None);

            master_skald_buff = Helpers.CreateBuff("SkaldMasterSkaldBuff",
                                                   master_skald.Name,
                                                   master_skald.Description,
                                                   "",
                                                   master_skald.Icon,
                                                   null,
                                                   Common.createBuffExtraAttack(1, true)
                                                   );
        }


        static void createInspiredRage()
        {
            var ac_penalty = Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.UntypedStackable, ContextValueType.Rank, AbilityRankType.DamageBonus);
            var will_bonus = Helpers.CreateAddContextStatBonus(StatType.SaveWill, ModifierDescriptor.Morale, ContextValueType.Rank, AbilityRankType.Default);
            var str_bonus = Helpers.CreateAddContextStatBonus(StatType.Strength, ModifierDescriptor.Morale, ContextValueType.Rank, AbilityRankType.StatBonus, 2);
            var con_bonus = Helpers.CreateAddContextStatBonus(StatType.Constitution, ModifierDescriptor.Morale, ContextValueType.Rank, AbilityRankType.StatBonus, 2);
            var ac_context_rank_config = Helpers.CreateContextRankConfig(type: AbilityRankType.DamageBonus,
                                                               baseValueType: ContextRankBaseValueType.FeatureListRanks,
                                                               progression: ContextRankProgression.BonusValue,
                                                               stepLevel: -1,
                                                               featureList: new BlueprintFeature[] { master_skald }
                                                               );
            var will_context_rank_config = Helpers.CreateContextRankConfig(type: AbilityRankType.Default,
                                                                           baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                           classes: getSkaldArray(),
                                                                           progression: ContextRankProgression.OnePlusDivStep,
                                                                           stepLevel: 4);
            var stat_context_rank_config = Helpers.CreateContextRankConfig(type: AbilityRankType.StatBonus,
                                                                           baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                           classes: getSkaldArray(),
                                                                           progression: ContextRankProgression.OnePlusDivStep,
                                                                           stepLevel: 8);


            var inspire_rage_debuff = Helpers.CreateBuff("InspiredRageDebuff",
                                                         "",
                                                         "",
                                                         "",
                                                         null,
                                                         null,
                                                         Helpers.CreateAddStatBonus(StatType.CheckDiplomacy, -20, ModifierDescriptor.UntypedStackable),
                                                         Helpers.CreateAddStatBonus(StatType.CheckBluff, -20, ModifierDescriptor.UntypedStackable),
                                                         Helpers.CreateAddStatBonus(StatType.SkillThievery, -20, ModifierDescriptor.UntypedStackable),
                                                         Helpers.CreateAddStatBonus(StatType.SkillStealth, -20, ModifierDescriptor.UntypedStackable),
                                                         Helpers.CreateAddStatBonus(StatType.SkillUseMagicDevice, -20, ModifierDescriptor.UntypedStackable),
                                                         Helpers.CreateAddStatBonus(StatType.SkillKnowledgeArcana, -20, ModifierDescriptor.UntypedStackable),
                                                         Helpers.CreateAddStatBonus(StatType.SkillKnowledgeWorld, -20, ModifierDescriptor.UntypedStackable)
                                                         );
            inspire_rage_debuff.SetBuffFlags(BuffFlags.HiddenInUi);

            inspired_rage_effect_buff = createRagingSongEffectBuff("SkaldInspiredRageEffectBuff", inspire_rage_debuff,
                                                                      ac_penalty, will_bonus, str_bonus, con_bonus, ac_context_rank_config, will_context_rank_config, stat_context_rank_config);
            var inspire_courage = library.Get<BlueprintActivatableAbility>("5250fe10c377fdb49be449dfe050ba70");

            inspired_rage_effect_buff.SetIcon(inspire_courage.Icon);
            inspired_rage_effect_buff.SetName("Inspired Rage");
            inspired_rage_effect_buff.SetDescription("At 1st level, affected allies gain a +2 morale bonus to Strength and Constitution and a +1 morale bonus on Will saving throws, but also take a –1 penalty to AC. While under the effects of inspired rage, allies other than the skald cannot use any Charisma-, Dexterity-, or Intelligence-based skills (except Mobility and Intimidate). While under the effects of inspired rage, allies other than the skald cannot use any ability that requires patience or concentration. At 4th level and every 4 levels thereafter, the song’s bonuses on Will saves increase by 1; the penalty to AC doesn’t change. At 8th and 16th levels, the song’s bonuses to Strength and Constitution increase by 2. Unlike the barbarian’s rage ability, those affected are not fatigued after the song ends. A skald is trained to use music, oration, and similar performances to inspire his allies to feats of strength and ferocity. At 1st level, a skald can use this ability for a number of rounds per day equal to 4 + his Charisma modifier. For each level thereafter, he can use raging song for 2 additional rounds per day.");
            inspired_rage = Common.convertPerformance(inspire_courage, inspired_rage_effect_buff, "SkaldInspiredRage");

            inspired_rage_feature = Helpers.CreateFeature("SkaldInspiredRageFeature",
                                                          inspired_rage.Name,
                                                          inspired_rage.Description,
                                                          "",
                                                          inspired_rage.Icon,
                                                          FeatureGroup.None,
                                                          Helpers.CreateAddFact(inspired_rage)
                                                          );
        }


        static internal BlueprintBuff createRagingSongEffectBuffForbidSpellCasting(string name, bool forbid_spellcasting,
                                                                                   BlueprintBuff allies_debuff, params BlueprintComponent[] components)
        {
            var raging_song_effect_buff = library.CopyAndAdd<BlueprintBuff>("da8ce41ac3cd74742b80984ccc3c9613", name, "");//standard rage buff
            //in AddFactContextActions in Activated we will need to replace all ContextConditionHasFact with ContextConditionCasterHasFact
            //moreover if fact is a switch buff, then in its AddFactContextActions we will also need to add  inspired rage buff to StandardRage
            //remove all logic in NewRound (since we do not need to count number of rage rounds for after rage fatigue)
            //in Deactivated remove Conditional (to apply fatigue)
            //also add skald to all contexts
            var standard_rage_buff = library.Get<BlueprintBuff>("da8ce41ac3cd74742b80984ccc3c9613");
            //replaceContextConditionHasFactToContextConditionCasterHasFact(raging_song_effect_buff, standard_rage_buff, raging_song_effect_buff, "Skald");

            var component = raging_song_effect_buff.GetComponent<AddFactContextActions>().CreateCopy();
            component.NewRound = Helpers.CreateActionList();

            var deactivate_actions = component.Deactivated.Actions;
            component.Deactivated = Helpers.CreateActionList(deactivate_actions.RemoveFromArrayByType<Kingmaker.ElementsSystem.GameAction, Conditional>());
            raging_song_effect_buff.ReplaceComponent<AddFactContextActions>(component);

            //clear everything
            raging_song_effect_buff.RemoveComponents<TemporaryHitPointsPerLevel>();
            raging_song_effect_buff.RemoveComponents<AttackTypeAttackBonus>();
            raging_song_effect_buff.RemoveComponents<WeaponGroupDamageBonus>();
            raging_song_effect_buff.RemoveComponents<SpellDescriptorComponent>();
            raging_song_effect_buff.RemoveComponents<WeaponAttackTypeDamageBonus>();
            raging_song_effect_buff.RemoveComponents<ContextCalculateSharedValue>();
            raging_song_effect_buff.RemoveComponents<AddContextStatBonus>();
            raging_song_effect_buff.RemoveComponents<ContextRankConfig>();

            raging_song_effect_buff.AddComponents(components);
            raging_song_effect_buff.RemoveComponents<ForbidSpellCasting>();
            raging_song_effect_buff.RemoveComponents<NewMechanics.ForbidSpellCastingUnlessHasClass>();

            var actions = new List<GameAction>();
            if (forbid_spellcasting)
            {
                actions.Add(Common.createContextActionApplyBuff(no_spell_casting_buff, Helpers.CreateContextDuration(), false, true, true, false));
            }
            if (allies_debuff != null)
            {
                actions.Add(Common.createContextActionApplyBuff(allies_debuff, Helpers.CreateContextDuration(), false, true, true, false));
            }
            var forbid_condition = Helpers.CreateConditional(new Condition[] { Common.createContextConditionIsCaster(not: true), Common.createContextConditionCasterHasFact(master_skald, has: false) },
                                                             actions.ToArray(),
                                                             null
                                                            );
            var master_skald_condition = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(master_skald, has: true),
                                                 Common.createContextActionApplyBuff(master_skald_buff, Helpers.CreateContextDuration(), false, true, true, false),
                                                 null
                                                );

            Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuff(raging_song_effect_buff, forbid_condition);

            Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuff(raging_song_effect_buff, master_skald_condition);

            var skald_vigor_condition1 = Helpers.CreateConditional(new Condition[]{Common.createContextConditionCasterHasFact(skald_vigor_feat, has: true),
                                                                                  Common.createContextConditionCasterHasFact(greater_skald_vigor_feat, has: false),
                                                                                  Common.createContextConditionIsCaster() },
                                                                  Common.createContextActionApplyBuff(skald_vigor_buff, Helpers.CreateContextDuration(), false, true, true, false),
                                                                  null
                                                                  );
            var skald_vigor_condition2 = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(greater_skald_vigor_feat, has: true),
                                                                   Common.createContextActionApplyBuff(skald_vigor_buff, Helpers.CreateContextDuration(), false, true, true, false),
                                                                   null
                                                                   );
            Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuff(raging_song_effect_buff, skald_vigor_condition1);
            Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuff(raging_song_effect_buff, skald_vigor_condition2);
            raging_song_effect_buff.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Emotion));
            return raging_song_effect_buff;
        }


        static internal BlueprintBuff createRagingSongEffectBuff(string name, BlueprintBuff allies_debuff, params BlueprintComponent[] components)
        {
            return createRagingSongEffectBuffForbidSpellCasting(name, true, allies_debuff, components);
        }




    }
}
