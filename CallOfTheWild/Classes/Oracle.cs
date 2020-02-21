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
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    class Oracle
    {
        static LibraryScriptableObject library => Main.library;
        internal static bool test_mode = false;
        static public BlueprintCharacterClass oracle_class;
        static public BlueprintProgression oracle_progression;
        static public BlueprintFeature oracle_proficiencies;
        static public BlueprintFeature oracle_orisons;
        static public BlueprintFeatureSelection oracle_curses;
        static public BlueprintFeatureSelection oracle_mysteries;
        static public BlueprintFeatureSelection revelation_selection;
        static public BlueprintFeatureSelection cure_inflict_spells_selection;

        static public BlueprintProgression clouded_vision;
        static public BlueprintProgression blackened;
        static public BlueprintProgression deaf;
        static public BlueprintProgression lame;
        static public BlueprintProgression wasting;
        static public BlueprintProgression pranked;
        static public BlueprintProgression plagued;
        static public BlueprintProgression vampirism;
        static public BlueprintProgression lich;
        static public BlueprintProgression wolf_scarred_face;

        static MysteryEngine mystery_engine;

        static public BlueprintProgression time_mystery;
        static public BlueprintProgression ancestor_mystery;


        static BlueprintCharacterClass[] getOracleArray()
        {
            return new BlueprintCharacterClass[] { oracle_class };
        }


        internal static void createOracleClass()
        {
            Main.logger.Log("Oracle class test mode: " + test_mode.ToString());
            var cleric_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var druid_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");

            oracle_class = Helpers.Create<BlueprintCharacterClass>();
            oracle_class.name = "OracleClass";
            library.AddAsset(oracle_class, "32c02466b2364c8a906e6e4761175099");


            oracle_class.LocalizedName = Helpers.CreateString("Oracle.Name", "Oracle");
            oracle_class.LocalizedDescription = Helpers.CreateString("Oracle.Description",
                "Although the gods work through many agents, perhaps none is more mysterious than the oracle. These divine vessels are granted power without their choice, selected by providence to wield powers that even they do not fully understand. Unlike a cleric, who draws her magic through devotion to a deity, oracles garner strength and power from many sources, namely those patron deities who support their ideals. Instead of worshiping a single source, oracles tend to venerate all of the gods that share their beliefs. While some see the powers of the oracle as a gift, others view them as a curse, changing the life of the chosen in unforeseen ways.\n"
                + "Role: Oracles do not usually associate with any one church or temple, instead preferring to strike out on their own, or with a small group of like-minded individuals. Oracles typically use their spells and revelations to further their understanding of their mystery, be it through fighting mighty battles or tending to the poor and sick."
                );
            oracle_class.m_Icon = oracle_class.Icon;
            oracle_class.SkillPoints = druid_class.SkillPoints;
            oracle_class.HitDie = DiceType.D8;
            oracle_class.BaseAttackBonus = cleric_class.BaseAttackBonus;
            oracle_class.FortitudeSave = cleric_class.ReflexSave;
            oracle_class.ReflexSave = cleric_class.ReflexSave;
            oracle_class.WillSave = cleric_class.WillSave;
            oracle_class.Spellbook = createOracleSpellbook();
            oracle_class.ClassSkills = new StatType[] { StatType.SkillKnowledgeArcana, StatType.SkillLoreReligion, StatType.SkillPersuasion, StatType.SkillKnowledgeWorld };
            oracle_class.IsDivineCaster = true;
            oracle_class.IsArcaneCaster = false;
            oracle_class.StartingGold = cleric_class.StartingGold;
            oracle_class.PrimaryColor = cleric_class.PrimaryColor;
            oracle_class.SecondaryColor = cleric_class.SecondaryColor;
            oracle_class.RecommendedAttributes = new StatType[] { StatType.Charisma};
            oracle_class.NotRecommendedAttributes = new StatType[0];
            oracle_class.EquipmentEntities = cleric_class.EquipmentEntities;
            oracle_class.MaleEquipmentEntities = cleric_class.MaleEquipmentEntities;
            oracle_class.FemaleEquipmentEntities = cleric_class.FemaleEquipmentEntities;
            oracle_class.ComponentsArray = new BlueprintComponent[] { cleric_class.ComponentsArray[0] };
            oracle_class.StartingItems = cleric_class.StartingItems;

            createOracleProgression();
            oracle_class.Progression = oracle_progression;
            //createSeeker();
            //createSpiritGuide();
            //createWarsighted();
            oracle_class.Archetypes = new BlueprintArchetype[] { };
            Helpers.RegisterClass(oracle_class);
            //createExtraRevelationFeat();

            Common.addMTDivineSpellbookProgression(oracle_class, oracle_class.Spellbook, "MysticTheurgeOracle",
                                                     Common.createPrerequisiteClassSpellLevel(oracle_class, 2));
        }


        static BlueprintSpellbook createOracleSpellbook()
        {
            var cleric_class = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var sorcerer_class = library.Get<BlueprintCharacterClass>("b3a505fb61437dc4097f43c3f8f9a4cf");
            var oracle_spellbook = Helpers.Create<BlueprintSpellbook>();
            oracle_spellbook.Name = oracle_class.LocalizedName;
            oracle_spellbook.name = "OracleSpellbook";
            library.AddAsset(oracle_spellbook, "");
            oracle_spellbook.Name = oracle_class.LocalizedName;
            oracle_spellbook.SpellsPerDay = sorcerer_class.Spellbook.SpellsPerDay;
            oracle_spellbook.SpellsKnown = sorcerer_class.Spellbook.SpellsKnown;
            oracle_spellbook.Spontaneous = true;
            oracle_spellbook.IsArcane = false;
            oracle_spellbook.AllSpellsKnown = false;
            oracle_spellbook.CanCopyScrolls = false;
            oracle_spellbook.CastingAttribute = StatType.Charisma;
            oracle_spellbook.CharacterClass = oracle_class;
            oracle_spellbook.CasterLevelModifier = 0;
            oracle_spellbook.CantripsType = CantripsType.Orisions;
            oracle_spellbook.SpellsPerLevel = cleric_class.Spellbook.SpellsPerLevel;
            oracle_spellbook.SpellList = Common.createSpellList("OracleSpellList", "", cleric_class.Spellbook.SpellList, 9);
            return oracle_spellbook;
        }


        static void createOracleProgression()
        {
            createOracleOrisons();
            createOracleProficiencies();
            createCureInflictSpells();
            createCurses();
            createMysteries();
            
            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");

            oracle_progression = Helpers.CreateProgression("OracleProgression",
                                                               oracle_class.Name,
                                                               oracle_class.Description,
                                                               "",
                                                               oracle_class.Icon,
                                                               FeatureGroup.None);
            oracle_progression.Classes = getOracleArray();

            oracle_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, oracle_proficiencies, detect_magic, oracle_orisons,
                                                                                        oracle_curses,
                                                                                        oracle_mysteries,
                                                                                        cure_inflict_spells_selection,
                                                                                        revelation_selection,
                                                                                        library.Get<BlueprintFeature>("0aeba56961779e54a8a0f6dedef081ee"), //inside the storm
                                                                                        library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), //ray calculate feature
                                                                                        library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")), //touch calculate feature};
                                                                    Helpers.LevelEntry(2),
                                                                    Helpers.LevelEntry(3, revelation_selection),
                                                                    Helpers.LevelEntry(4),
                                                                    Helpers.LevelEntry(5),
                                                                    Helpers.LevelEntry(6),
                                                                    Helpers.LevelEntry(7, revelation_selection),
                                                                    Helpers.LevelEntry(8),
                                                                    Helpers.LevelEntry(9),
                                                                    Helpers.LevelEntry(10),
                                                                    Helpers.LevelEntry(11, revelation_selection),
                                                                    Helpers.LevelEntry(12),
                                                                    Helpers.LevelEntry(13),
                                                                    Helpers.LevelEntry(14),
                                                                    Helpers.LevelEntry(15, revelation_selection),
                                                                    Helpers.LevelEntry(16),
                                                                    Helpers.LevelEntry(17),
                                                                    Helpers.LevelEntry(18),
                                                                    Helpers.LevelEntry(19, revelation_selection),
                                                                    Helpers.LevelEntry(20)
                                                                    };

            oracle_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { oracle_proficiencies, detect_magic, oracle_orisons, oracle_curses, cure_inflict_spells_selection, oracle_mysteries};

            oracle_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(revelation_selection)};
        }


        static void createMysteries()
        {
            mystery_engine = new MysteryEngine(getOracleArray(), StatType.Charisma);

            revelation_selection = Helpers.CreateFeatureSelection("OracleRevelationSelection",
                                                                  "Revelation",
                                                                  "At 1st level, 3rd level, and every four levels thereafter (7th, 11th, and so on), an oracle uncovers a new secret about her mystery that grants her powers and abilities. The oracle must select a revelation from the list of revelations available to her mystery (see FAQ at right). If a revelation is chosen at a later level, the oracle gains all of the abilities and bonuses granted by that revelation based on her current level. Unless otherwise noted, activating the power of a revelation is a standard action.\n"
                                                                  + "Unless otherwise noted, the DC to save against these revelations is equal to 10 + 1/2 the oracle’s level + the oracle’s Charisma modifier.",
                                                                  "",
                                                                  null,
                                                                  FeatureGroup.None);


            createTimeMystery();
            createAncestorsMystery();


            revelation_selection.AllFeatures = new BlueprintFeature[] { time_mystery, ancestor_mystery };
        }


        static void createTimeMystery()
        {
            var spells = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("14c90900b690cac429b229efdf416127"), //longstrider
                library.Get<BlueprintAbility>("464a7193519429f48b4d190acb753cf0"), //grace
                NewSpells.sands_of_time,
                NewSpells.threefold_aspect,
                library.Get<BlueprintAbility>("41e8a952da7a5c247b3ec1c2dbb73018"), //hold monster
                NewSpells.contingency,
                library.Get<BlueprintAbility>("4aa7942c3e62a164387a73184bca3fc1"), //disintegrate
                NewSpells.temporal_stasis,
                NewSpells.time_stop,
            };

            var aging_touch = mystery_engine.createAgingTouch("AgingTouchOracleRevelation",
                                                              "Aging Touch",
                                                              "Your touch ages living creatures and objects. As a melee touch attack, you can deal 1 point of Strength damage for every two oracle levels you possess to living creatures. Against constructs, you can deal 1d6 points of damage per oracle level. You can use this ability once per day, plus one additional time per day for every five oracle levels you possess.");
            var rewind_time = mystery_engine.createRewindTime("RewindTimeOracleRevelation",
                                                              "Rewind Time",
                                                              "Once per day as a swift action, you can reroll any one failed d20 in the next round. At 11th level, and every four levels thereafter, you can use this ability an additional time per day. You must be at least 7th level to select this revelation.");          
            var speed_or_slow_time = mystery_engine.createSpeedOrSlowTime("SpeedOrSlowTimeOracleRevelation",
                                                                          "Speed or Slow Time",
                                                                          "As a standard action, you can speed up or slow down time, as either the haste or slow spell. You can use this ability once per day, plus one additional time per day at 12th level and 17th level. You must be at least 7th level before selecting this revelation.");                     
            var temporal_celerity = mystery_engine.createTemporalCelerity("TemporalCelerityOracleRevelation",
                                                                          "Temporal Celerity",
                                                                          "Whenever you roll for initiative, you can roll twice and take either result. At 7th level, you can always act in the surprise round, but if you fail to notice the ambush, you act last, regardless of your initiative result (you act in the normal order in following rounds). At 11th level, you can roll for initiative three times and take any one of the results.");
            var time_flicker = mystery_engine.createTimeFlicker("TimeFlickerOracleRevelation",
                                                                "Time Flicker",
                                                                "As a standard action, you can flicker in and out of time, gaining concealment (as the blur spell). You can use this ability for 1 minute per oracle level that you possess per day. This duration does not need to be consecutive, but it must be spent in 1-minute increments. At 7th level, each time you activate this ability, you can treat it as the displacement spell, though each round spent this way counts as 1 minute of your normal time flicker duration. You must be at least 3rd level to select this revelation.");
            var time_hop = mystery_engine.createTimeHop("TimeHopOracleRevelation",
                                                        "Time Hop",
                                                        "As a move action, you can teleport up to 50 feet per 3 oracle levels, as the dimension door spell. This movement does not provoke attacks of opportunity. You must have line of sight to your destination to use this ability. You can bring other willing creatures with you, but you must expend 2 uses of this ability. You must be at least 7th level before selecting this revelation.");
            var time_sight = mystery_engine.createTimeSight("TimeSightOracleRevelation",
                                                            "Time Sight",
                                                            "You can peer through the mists of time to see things as they truly are, as if using the true seeing spell.\n" +
                                                            "At 18th level, this functions like foresight. You can use this ability for a number of minutes per day equal to your oracle level, but these minutes do not need to be consecutive. You must be at least 11th level to select this revelation.");
            var erase_from_time = mystery_engine.createEraseFromTime("EraseFromTimeOracleRevelation",
                                                                     "Erase From Time",
                                                                     "As a melee touch attack, you can temporarily remove a creature from time altogether. The target creature must make a Fortitude save or vanish completely for a number of rounds equal to 1/2 your oracle level (minimum 1 round). No magic or divinations can detect the creature during this time, as it exists outside of time and space—in effect, the creature ceases to exist for the duration of this ability. At the end of the duration, the creature reappears unharmed in the space it last occupied (or the nearest possible space, if the original space is now occupied). You can use this ability once per day, plus one additional time per day at 11th level.");

            var time_stop = Common.convertToSpellLike(NewSpells.time_stop, "OracleTimeMystery", getOracleArray(), StatType.Charisma);
            var final_revelation = Helpers.CreateFeature("FinalRevelationTimeMystery",
                                                         "Final Revelation",
                                                         "Upon reaching 20th level, you become a true master of time and stop aging. You receive +2 competence bonus to your mental stats. In addition, you can cast time stop once per day as a spell-like ability.",
                                                         "",
                                                         null,
                                                         FeatureGroup.None,
                                                         Helpers.CreateAddStatBonus(StatType.Wisdom, 2, ModifierDescriptor.Competence),
                                                         Helpers.CreateAddStatBonus(StatType.Intelligence, 2, ModifierDescriptor.Competence),
                                                         Helpers.CreateAddStatBonus(StatType.Charisma, 2, ModifierDescriptor.Competence),
                                                         Helpers.CreateAddFact(time_stop),
                                                         Helpers.CreateAddAbilityResource(time_stop.GetComponent<AbilityResourceLogic>().RequiredResource)
                                                         );

            time_mystery = createMystery("Time", "Time", time_stop.Icon, final_revelation,
                                         new StatType[] { StatType.SkillMobility, StatType.SkillPerception, StatType.SkillUseMagicDevice },
                                         spells,
                                         aging_touch, rewind_time, speed_or_slow_time, temporal_celerity,
                                         time_flicker, time_hop, time_sight, erase_from_time
                                         );
        }


        static void createAncestorsMystery()
        {
            var spells = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("2c38da66e5a599347ac95b3294acbe00"), //true strike
                NewSpells.force_sword,
                library.Get<BlueprintAbility>("5ab0d42fb68c9e34abae4921822b9d63"), //heroism
                library.Get<BlueprintAbility>("6717dbaef00c0eb4897a1c908a75dfe5"), //phantasmal killer
                library.Get<BlueprintAbility>("90810e5cf53bf854293cbd5ea1066252"), //righteous might
                library.Get<BlueprintAbility>("e15e5e7045fda2244b98c8f010adfe31"), //heroism greater
                library.Get<BlueprintAbility>("98310a099009bbd4dbdf66bcef58b4cd"), //mass invisibility
                library.Get<BlueprintAbility>("0e67fa8f011662c43934d486acc50253"), //prediction of failure
                library.Get<BlueprintAbility>("43740dab07286fe4aa00a6ee104ce7c1"), //heroic invocation
            };


            var blood_of_heroes = mystery_engine.createBloodOfHeroes("BloodOfHeroesOracleRevelation",
                                                         "Blood of Heroes",
                                                         "As a move action, you can call upon your ancestors to grant you extra bravery in battle. You gain a +1 morale bonus on attack rolls, damage rolls, and Will saves against fear for a number of rounds equal to your Charisma bonus. At 7th level, this bonus increases to +2, and at 14th level this bonus increases to +3. You can use this ability once per day, plus one additional time per day at 5th level, and every five levels thereafter.");
            var phantom_touch = mystery_engine.createPhantomTouch("PhantomTouchOracleRevelation",
                                                                  "Phantom Touch",
                                                                  "As a standard action, you can perform a melee touch attack that causes a living creature to become shaken. This ability lasts for a number of rounds equal to 1/2 your oracle level (minimum 1 round). You can use this ability a number of times per day equal to 3 + your Charismae modifier.");
            var sacred_council = mystery_engine.createSacredCouncil("SacredCouncilOracleRevelation",
                                                                    "Sacred Council",
                                                                     "As a move action, you can call upon your ancestors to provide council. This advice grants you a +2 bonus on any one kind of d20 rolls. This effect lasts for 1 round. You can use this ability a number of times per day equal to your Charisma bonus.");
            var spirit_of_the_warrior = mystery_engine.createSpiritOfTheWarrior("SpiritOfTheWarriorOracleRevelation",
                                                                                "Spirit of the Warrior",
                                                                                "You can summon the spirit of a great warrior ancestor and allow it to possess you, becoming a mighty warrior yourself. You gain a +4 enhancement bonus to Strength, Dexterity, and Constitution, and a +4 natural armor bonus to AC. Your base attack bonus while possessed equals your oracle level (which may give you additional attacks), all weapons you are holding receive keen enchantment. You can use this ability for 1 round for every 2 oracle levels you possess. This duration does not need to be consecutive, but it must be spent in 1-round increments. You must be at least 11th level to select this revelation.");
           
            var spirit_shield = mystery_engine.createSpiritShield("SpiritShieldOracleRevelation",
                                                                  "Spirit Shield",
                                                                  "You can call upon the spirits of your ancestors to form a shield around you that blocks incoming attacks and grants you a +4 armor bonus. At 7th level, and every four levels thereafter, this bonus increases by +2. At 13th level, this shield causes arrows, rays, and other ranged attacks requiring an attack roll against you to have a 50% miss chance. You can use this shield for 1 hour per day per oracle level. This duration does not need to be consecutive, but it must be spent in 1-hour increments.");
            var storm_of_souls = mystery_engine.createStormOfSouls("StormOfSoulsOracleRevelation",
                                                                   "Storm of Souls",
                                                                   "You can summon the spirits of your ancestors to attack in a ghostly barrage—their fury creates physical wounds on creatures in the area. The storm has a range of 100 feet and is a 20-foot-radius burst. Objects and creatures in the area take 1d8 hit points of damage for every two oracle levels you possess. Undead creatures in the area take 1d8 points of damage for every oracle level you possess. A successful Fortitude save reduces the damage to half. You must be at least 7th level to select this revelation. You can use this ability once per day, plus one additional time per day at 11th level and every four levels thereafter.");
            var spirit_walk = mystery_engine.createSpiritWalk("SpiritWalkOracleRevelation",
                                                                   "Spirit Walk",
                                                                   "You can become incorporeal and invisible. You can take no action other than to move while in this form. You remain in this form for a number of rounds equal to twice your oracle level, but these rounds need not be consecutive. You must be at least 11th level to select this revelation.");           
            var ancestral_weapon = mystery_engine.createAncestralWeapon("AncestralWeaponOracleRevelation",
                                                                     "Ancestral Wepon",
                                                                     "The weapon you hold gains ghost touch weapon property. You can use this ability for a number of minutes per day equal to your oracle level. This duration does not need to be consecutive, but it must be used in 1-minute increments. The weapon loses ghost touch property if it leaves your grasp.");

            var heroic_invocation = Common.convertToSpellLike(library.Get<BlueprintAbility>("43740dab07286fe4aa00a6ee104ce7c1"), "OracleAncestorMystery", getOracleArray(), StatType.Charisma);
            var final_revelation = Helpers.CreateFeature("FinalRevelationAncestorMystery",
                                                         "Final Revelation",
                                                         "Upon reaching 20th level, you become one with the spirits of your ancestors. You gain a bonus on Will saving throws equal to your Charisma modifier, blindsense out to a range of 60 feet, and a +4 bonus on your caster level for all divination spells. You can cast heroic invocation as a spell-like ability once per day.",
                                                         "",
                                                         null,
                                                         FeatureGroup.None,
                                                         Helpers.CreateAddContextStatBonus(StatType.SaveWill, ModifierDescriptor.Other),
                                                         Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Charisma),
                                                         Helpers.Create<IncreaseSpellSchoolCasterLevel>(i => { i.School = SpellSchool.Divination; i.BonusLevel = 4; }),
                                                         Helpers.CreateAddFact(heroic_invocation),
                                                         Helpers.CreateAddAbilityResource(heroic_invocation.GetComponent<AbilityResourceLogic>().RequiredResource)
                                                         );
            ancestor_mystery = createMystery("Ancestor", "Ancestor", library.Get<BlueprintAbility>("6717dbaef00c0eb4897a1c908a75dfe5").Icon, final_revelation,
                             new StatType[] { StatType.SkillLoreNature },
                             spells,
                             blood_of_heroes, phantom_touch, sacred_council, spirit_of_the_warrior,
                             spirit_shield , storm_of_souls, spirit_walk, ancestral_weapon
                             );
        }


        static BlueprintProgression createMystery(string name, string display_name, UnityEngine.Sprite icon, BlueprintFeature final_revelation, StatType[] class_skills, BlueprintAbility[] spells, params BlueprintFeature[] revelations)
        {
            string description = $"An oracle with the {display_name.ToLower()} mystery adds ";

            for (int i = 0; i < class_skills.Length; i++)
            {
                description += LocalizedTexts.Instance.Stats.GetText(class_skills[i]);
                if (i + 2 < class_skills.Length)
                {
                    description += ", ";
                }
                else if (i + 1 < class_skills.Length)
                {
                    description += " and ";
                }       
            }

            description += " to her list of class skills.";

            var mystery = Helpers.CreateProgression(name + "Progression",
                                                    display_name,
                                                    description,
                                                    "",
                                                    icon,
                                                    FeatureGroup.Domain);
            foreach (var s in class_skills)
            {
                mystery.AddComponent(Helpers.Create<AddClassSkill>(a => a.Skill = s));
            }

            mystery.Classes = getOracleArray();
            mystery.LevelEntries = new LevelEntry[spells.Length + 1];
            mystery.UIGroups = Helpers.CreateUIGroups();
            for (int i = 0; i < spells.Length; i++)
            {
                var feat = Helpers.CreateFeature(name + spells[i].name,
                                                 spells[i].Name,
                                                 "At 2nd level, and every two levels thereafter, an oracle learns an additional spell derived from her mystery.\n"
                                                 + spells[i].Name + ": " + spells[i].Description,
                                                 "",
                                                 spells[i].Icon,
                                                 FeatureGroup.None,
                                                 spells[i].CreateAddKnownSpell(oracle_class, i + 1)
                                                 );
                mystery.LevelEntries[i] = Helpers.LevelEntry(2 * (i + 1), feat);
                mystery.UIGroups[0].Features.Add(feat);
            }

            mystery.LevelEntries[spells.Length] = Helpers.LevelEntry(20, final_revelation);
            mystery.UIGroups[0].Features.Add(final_revelation);

            var mystery_revelation_selection = Helpers.CreateFeatureSelection(name + "RevelationSelection",
                                                                              display_name,
                                                                              revelation_selection.Description,
                                                                              "",
                                                                              icon,
                                                                              FeatureGroup.None,
                                                                              Helpers.PrerequisiteFeature(mystery));

            mystery_revelation_selection.AllFeatures = revelations;

            revelation_selection.AllFeatures = revelation_selection.AllFeatures.AddToArray(mystery_revelation_selection);

            return mystery;
        }


        static void createCurses()
        {
            createCloudedVision();
            createBlackened();
            createDeaf();
            createLame();
            createWasting();
            createPranked();
            createPlagued();
            createWolfScarredFace();
            createLich();
            createVampirism();

            oracle_curses = Helpers.CreateFeatureSelection("OracleCurseSelection",
                                                           "Oracle's Curse",
                                                           "Each oracle is cursed, but this curse comes with a benefit as well as a hindrance. This choice is made at 1st level, and once made, it cannot be changed. The oracle’s curse cannot be removed or dispelled without the aid of a deity. An oracle’s curse is based on her oracle level.",
                                                           "",
                                                           null,
                                                           FeatureGroup.None);

            oracle_curses.AllFeatures = new BlueprintFeature[] { clouded_vision, blackened, deaf, lame, wasting, pranked, plagued, wolf_scarred_face, lich, vampirism };
        }


        static void createVampirism()
        {
            var vampiric_touch = library.Get<BlueprintAbility>("8a28a811ca5d20d49a863e832c31cce1");
            var vampiric_shadow_shield = library.Get<BlueprintAbility>("a34921035f2a6714e9be5ca76c5e34b5");
            var curse = Helpers.CreateFeature("OracleCurseVampirism",
                                              "Vampirism",
                                              "You take damage from positive energy and heal from negative energy as if you were undead.",
                                              "",
                                              NewSpells.savage_maw.Icon,
                                              FeatureGroup.None,
                                              Helpers.Create<UndeadMechanics.ConsiderUndeadForHealing>(),
                                              Helpers.Create<AddEnergyImmunity>(a => a.Type = DamageEnergyType.NegativeEnergy)
                                              );

            var curse5 = Helpers.CreateFeature("OracleCurseVampirism",
                                               "ChannelResistance",
                                               "At 5th level, you gain channel resistance +4.",
                                               "",
                                               Helpers.GetIcon("89df18039ef22174b81052e2e419c728"),
                                               FeatureGroup.None,
                                               Helpers.Create<SavingThrowBonusAgainstSpecificSpells>(c =>
                                                                                                        {
                                                                                                            c.Spells = new BlueprintAbility[0];
                                                                                                            c.Value = 4;
                                                                                                            c.BypassFeatures = new BlueprintFeature[] { library.Get<BlueprintFeature>("3d8e38c9ed54931469281ab0cec506e9") }; //sun domain
                                                                                                        }
                                                                                                    )
                                               );
            ChannelEnergyEngine.addChannelResitance(curse5);

            var curse10 = Helpers.CreateFeature("OracleCurse10Vampirism",
                                                "Vampirism",
                                                "At 10th level, you add vampiric touch to your list of 3rd-level oracle spells known and vampiric shadow shield to your list of 5th-level oracle spells known",
                                                "",
                                                vampiric_touch.Icon,
                                                FeatureGroup.None,
                                                vampiric_touch.CreateAddKnownSpell(oracle_class, 2),
                                                vampiric_shadow_shield.CreateAddKnownSpell(oracle_class, 5)
                                                );

            var curse15 = Helpers.CreateFeature("OracleCurse15Vampirism",
                                                "Damage Reduction",
                                                "At 15th level, you gain damage reduction 5/magic.",
                                                "",
                                                vampiric_shadow_shield.Icon,
                                                FeatureGroup.None,
                                                Common.createMagicDR(5)
                                                );

            vampirism = createOracleCurseProgression("OracleVampirismCurseProgression", "Vampirism",
                                                    "You crave the taste of fresh, warm blood.",
                                                    curse, curse5, curse10, curse15);
        }

        static void createLich()
        {
            var false_life = library.Get<BlueprintAbility>("7a5b5bf845779a941a67251539545762");
            var wail_of_banshee = library.Get<BlueprintAbility>("b24583190f36a8442b212e45226c54fc");
            var curse = Helpers.CreateFeature("OracleCurseLich",
                                              "Lich",
                                              "You have (unknowingly) fulfilled most (but not all) of the ritualistic components to achieve lichdom. You have yet to turn into an undead creature, but you are close. You take damage from positive energy and heal from negative energy as if you were undead.",
                                              "",
                                              Helpers.GetIcon("a1a8bf61cadaa4143b2d4966f2d1142e"), // undead bloodline
                                              FeatureGroup.None,
                                              Helpers.Create<UndeadMechanics.ConsiderUndeadForHealing>(),
                                              Helpers.Create<AddEnergyImmunity>(a => a.Type = DamageEnergyType.NegativeEnergy)
                                              );

            var curse5 = Helpers.CreateFeature("OracleCurseLich",
                                               "Ghoul Touch",
                                               "At 5th level, you add ghoul touch to your list of 2nd-level oracle spells known.",
                                               "",
                                               NewSpells.ghoul_touch.Icon,
                                               FeatureGroup.None,
                                               NewSpells.ghoul_touch.CreateAddKnownSpell(oracle_class, 2));


            var curse10 = Helpers.CreateFeature("OracleCurse10Lich",
                                                "Lich",
                                                "At 10th level, you add false life to your list of 2nd-level oracle spells known and wail of banshee to your list of 5th-level oracle spells known.",
                                                "",
                                                wail_of_banshee.Icon,
                                                FeatureGroup.None,
                                                false_life.CreateAddKnownSpell(oracle_class, 2),
                                                wail_of_banshee.CreateAddKnownSpell(oracle_class, 5)
                                                );

            var curse15 = Helpers.CreateFeature("OracleCurse15Lich",
                                                "Immunity To Death Effects",
                                                "At 15th level, you are immune to death effects.",
                                                "",
                                                Helpers.GetIcon("0413915f355a38146bc6ad40cdf27b3f"), // death ward
                                                FeatureGroup.None,
                                                Common.createBuffDescriptorImmunity(SpellDescriptor.Death),
                                                SpellDescriptor.Death.CreateBuffImmunity());

            lich = createOracleCurseProgression("OracleLichCurseProgression", "Lich",
                                                    "Every living spellcaster hides a secret in their flesh—a unique, personalized set of conditions that, when all are fulfilled in the correct order, can trigger the transformation into a lich. Normally, one must expend years and tens of thousands of gold pieces to research this deeply personalized method of attaining immortality. Yet, in a rare few cases, chance and ill fortune can conspire against an unsuspecting spellcaster.",
                                                    curse, curse5, curse10, curse15);
        }


        static void createWolfScarredFace()
        {
            var magic_fang = library.Get<BlueprintAbility>("403cf599412299a4f9d5d925c7b9fb33");
            var magic_fang_greater = library.Get<BlueprintAbility>("f1100650705a69c4384d3edd88ba0f52");

            var bite1d4 = Helpers.CreateFeature("OracleWolfScarred1d4BiteFeature",
                                    "",
                                    "",
                                    "",
                                    null,
                                    FeatureGroup.None,
                                    Common.createAddSecondaryAttacks(library.Get<BlueprintItemWeapon>("35dfad6517f401145af54111be04d6cf"))
                                    );
            bite1d4.HideInUI = true;

            var bite1d6 = Helpers.CreateFeature("OracleWolfScarred1d6BiteFeature",
                                                "",
                                                "",
                                                "",
                                                null,
                                                FeatureGroup.None,
                                                Common.createAddSecondaryAttacks(library.Get<BlueprintItemWeapon>("a000716f88c969c499a535dadcf09286"))
                                                );
            bite1d6.HideInUI = true;
            var bite1d8 = Helpers.CreateFeature("OracleWolfScarred1d8BiteFeature",
                                    "",
                                    "",
                                    "",
                                    null,
                                    FeatureGroup.None,
                                    Common.createAddSecondaryAttacks(library.Get<BlueprintItemWeapon>("61bc14eca5f8c1040900215000cfc218"))
                                    );
            bite1d8.HideInUI = true;

            var curse = Helpers.CreateFeature("OracleCurseWolfScarredFace",
                                              "Wolf-scarred Face",
                                              "You have a severe speech impediment, and any spells you cast have a 20% chance of failing, wasting your action but not expending the spell. You gain a natural bite attack that deals 1d4 points of damage if you are a Medium creature or 1d3 points of damage if you are Small.",
                                              "",
                                              Helpers.GetIcon("de7a025d48ad5da4991e7d3c682cf69d"), // cats grace
                                              FeatureGroup.None,
                                              Helpers.Create<SpellFailureMechanics.SpellFailureChance>(s => s.chance = 20),
                                              Helpers.CreateAddFeatureOnClassLevel(bite1d4, 5, getOracleArray(), before: true)
                                              );

            var curse5 = Helpers.CreateFeature("OracleCurse5WolfScarredFace",
                                               "Wolf-scarred Face",
                                               "At 5th level, you add magic fang to your list of known spells and your bite damage increases to 1d6.",
                                               "",
                                               magic_fang.Icon,
                                               FeatureGroup.None,
                                               magic_fang.CreateAddKnownSpell(oracle_class, 1),
                                               Helpers.CreateAddFeatureOnClassLevel(bite1d6, 10, getOracleArray(), before: true)
                                               );

            var curse10 = Helpers.CreateFeature("OracleCurse10WolfScarredFace",
                                                "Wolf-scarred Face",
                                                "At 10th level, the damage dealt by your bite attack increases to 1d8.",
                                                "",
                                                Helpers.GetIcon("75de4ded3e731dc4f84d978fe947dc67"), // acid maw
                                                FeatureGroup.None,
                                                Helpers.CreateAddFeatureOnClassLevel(bite1d8, 15, getOracleArray(), before: true)
                                                );

            var curse15 = Helpers.CreateFeature("OracleCurse15WolfScarredFace",
                                                "Wolf-scarred Face",
                                                "At 15th level, you add greater magic fang to your list of known spells and the damage dealt by your bite attack increases to 2d6.",
                                                "",
                                                magic_fang_greater.Icon,
                                                FeatureGroup.None,
                                                magic_fang_greater.CreateAddKnownSpell(oracle_class, 3),
                                                Common.createAddSecondaryAttacks(library.Get<BlueprintItemWeapon>("2abc1dc6172759c42971bd04b8c115cb")) //bite 2d6
                                                );

            wolf_scarred_face = createOracleCurseProgression("OracleWolfScarredFaceCurseProgression", "Wolf-scarred Face",
                                                            "Your face is deformed, as though you were born with a wolf’s muzzle instead of an ordinary nose and jaw. Many mistake you for a werewolf, and in areas plagued by lycanthropes, you must take pains to hide your face.",
                                                            curse, curse5, curse10, curse15);
        }

        static void createPlagued()
        {
            var pox_pustules = library.Get<BlueprintAbility>("bc153808ef4884a4594bc9bec2299b69");
            var curse = Helpers.CreateFeature("OracleCursePlagued",
                                  "Plagued",
                                  "You take a –1 penalty on all saving throws against disease effects, but you are immune to the sickened condition.",
                                  "",
                                  Helpers.GetIcon("4e42460798665fd4cb9173ffa7ada323"), // sickened
                                  FeatureGroup.None,
                                  Helpers.Create<SavingThrowBonusAgainstDescriptor>(s => { s.Bonus = -1; s.ModifierDescriptor = ModifierDescriptor.Other; s.SpellDescriptor = SpellDescriptor.Disease; })
                                  );

            var curse5 = Helpers.CreateFeature("OracleCurse5Plagued",
                                               "Pox Pustules",
                                               "At 5th level, you add pox pustules to your list of 2nd-level oracle spells known.",
                                               "",
                                               pox_pustules.Icon,
                                               FeatureGroup.None,
                                               pox_pustules.CreateAddKnownSpell(oracle_class, 2));

            var curse10 = Helpers.CreateFeature("OracleCurse10Plagued",
                                                "Plagued",
                                                "At 10th level, increase the save DC of any disease effect you create by +2.",
                                                "",
                                                Helpers.GetIcon("82a5b848c05e3f342b893dedb1f9b446"), // plague storm
                                                FeatureGroup.None,
                                                Helpers.Create<NewMechanics.ContextIncreaseDescriptorSpellsDC>(i => { i.Value = 2; i.Descriptor = SpellDescriptor.Disease; })
                                                );

            var curse15 = Helpers.CreateFeature("OracleCurse15Plagued",
                                                "Immune to Disease",
                                                "At 15th level, you are immune to the effects of disease.",
                                                "",
                                                Helpers.GetIcon("4093d5a0eb5cae94e909eb1e0e1a6b36"), // remove disease
                                                FeatureGroup.None,
                                                Common.createBuffDescriptorImmunity(SpellDescriptor.Disease),
                                                SpellDescriptor.Disease.CreateBuffImmunity());

            plagued = createOracleCurseProgression("OraclePlaguedCurseProgression", "Plagued",
                                                    "You suffer from minor ailments and sicknesses. While you struggle to resist new diseases, you have grown accustomed to the many inconveniences of sickness.",
                                                    curse, curse5, curse10, curse15);
        }

        static void createPranked()
        {
            var faerie_fire = library.Get<BlueprintAbility>("4d9bf81b7939b304185d58a09960f589");
            var vanish = library.Get<BlueprintAbility>("f001c73999fb5a543a199f890108d936");
            var glitterdust = library.Get<BlueprintAbility>("ce7dad2b25acf85429b6c9550787b2d9");
            var hideous_laughter = library.Get<BlueprintAbility>("fd4d9fd7f87575d47aafe2a64a6e2d8d");
            var confusion = library.Get<BlueprintAbility>("886c7407dc629dc499b9f1465ff382df");
            var mass_invisibility = library.Get<BlueprintAbility>("98310a099009bbd4dbdf66bcef58b4cd");
            var curse = Helpers.CreateFeature("OracleCursePranked",
                                              "Pranked",
                                              "You take a –4 penalty on initiative checks. Furthermore, whenever you attempt to cast a spell using an item, there’s a 25% chance that you fail. You add faerie fire and vanish to your list of spells known.",
                                              "",
                                              hideous_laughter.Icon,
                                              FeatureGroup.None,
                                              Helpers.CreateAddStatBonus(StatType.Initiative, -4, ModifierDescriptor.Penalty),
                                              Helpers.Create<SpellFailureMechanics.ItemUseFailure>(s => s.chance = 25),
                                              faerie_fire.CreateAddKnownSpell(oracle_class, 1),
                                              vanish.CreateAddKnownSpell(oracle_class, 1)
                                              );

            var curse5 = Helpers.CreateFeature("OracleCurse5Pranked",
                                               "Pranked",
                                               "At 5th level, you add glitterdust and minor image to your list of spells known.",
                                               "",
                                               glitterdust.Icon,
                                               FeatureGroup.None,
                                               glitterdust.CreateAddKnownSpell(oracle_class, 2),
                                               hideous_laughter.CreateAddKnownSpell(oracle_class, 2)
                                               );

            var curse10 = Helpers.CreateFeature("OracleCurse10Pranked",
                                                "Pranked",
                                                "At 10th level, you add confusion to your list of spells known as a 5th-level spell.",
                                                "",
                                                confusion.Icon,
                                                FeatureGroup.None,
                                                confusion.CreateAddKnownSpell(oracle_class, 5));

            var curse15 = Helpers.CreateFeature("OracleCurse15Pranked",
                                                "Pranked",
                                                "At 15th level, you add invisibility, mass to your list of spells known.",
                                                "",
                                                mass_invisibility.Icon,
                                                FeatureGroup.None,
                                                mass_invisibility.CreateAddKnownSpell(oracle_class, 7));

            pranked = createOracleCurseProgression("OraclePrankedCurseProgression", "Pranked",
                                                    "Capricious fey constantly bedevil you, playing pranks on you such as tying your shoelaces together, hiding your gear, making inappropriate noises or smells at formal events, and mimicking your voice to tell embarrassing lies.",
                                                    curse, curse5, curse10, curse15);
        }


        static void createWasting()
        {
            var curse = Helpers.CreateFeature("OracleCurseWasting", 
                                              "Wasting",
                                              "You take a –4 penalty on Charisma-based skill checks, except for Intimidate. You gain a +4 competence bonus on saves made against disease.",
                                              "",
                                              Helpers.GetIcon("4e42460798665fd4cb9173ffa7ada323"), // sickened
                                              FeatureGroup.None,
                                              Helpers.CreateAddStatBonus(StatType.CheckDiplomacy, -4, ModifierDescriptor.Penalty),
                                              Helpers.CreateAddStatBonus(StatType.CheckBluff, -4, ModifierDescriptor.Penalty),
                                              Helpers.CreateAddStatBonus(StatType.SkillUseMagicDevice, -4, ModifierDescriptor.Penalty),
                                              Helpers.Create<SavingThrowBonusAgainstDescriptor>(s =>
                                                                                                {
                                                                                                    s.Value = 4;
                                                                                                    s.SpellDescriptor = SpellDescriptor.Disease;
                                                                                                    s.ModifierDescriptor = ModifierDescriptor.Competence;
                                                                                                }
                                                                                                )
                                              );

            var curse5 = Helpers.CreateFeature("OracleCurse5Wasting", 
                                               "Immune to Sickened",
                                               "At 5th level, you are immune to the sickened condition (but not nauseated).",
                                               "",
                                               Helpers.GetIcon("7ee2ef06226a4884f80b7647a2aa2dee"), // mercy sickened
                                               FeatureGroup.None,
                                               UnitCondition.Sickened.CreateImmunity(),
                                               SpellDescriptor.Sickened.CreateBuffImmunity());

            var curse10 = Helpers.CreateFeature("OracleCurse10Wasting", 
                                                "Immune to Disease",
                                                "At 10th level, you gain immunity to disease.",
                                                "",
                                                Helpers.GetIcon("3990a92ce97efa3439e55c160412ce14"), // mercy diseased
                                                FeatureGroup.None,
                                                SpellDescriptor.Disease.CreateSpellImmunity(),
                                                SpellDescriptor.Disease.CreateBuffImmunity());

            var curse15 = Helpers.CreateFeature("OracleCurse15Wasting",
                                                "Immune to Nauseated",
                                                "At 15th level, you are immune to the nauseated condition.",
                                                "",
                                                Helpers.GetIcon("a0cacf71d872d2a42ae3deb6bf977962"), // mercy nauseated
                                                FeatureGroup.None,
                                                UnitCondition.Nauseated.CreateImmunity(),
                                                SpellDescriptor.Nauseated.CreateBuffImmunity());

            wasting = createOracleCurseProgression("OracleWastingCurseProgression", "Wasting",
                                                    "Your body is slowly rotting away.",
                                                    curse, curse5, curse10, curse15);
        }


        static void createLame()
        {
            var curse = Helpers.CreateFeature("OracleCurseLame",
                                              "Lame",
                                              "One of your legs is permanently wounded, reducing your base land speed by 10 feet if your base speed is 30 feet or more. If your base speed is less than 30 feet, your speed is reduced by 5 feet. Your speed is never reduced due to encumbrance.",
                                              "",
                                              Helpers.GetIcon("f492622e473d34747806bdb39356eb89"), // slow
                                              FeatureGroup.None,
                                              Helpers.Create<EncumbranceMechanics.IgnoreEncumbrence>(),
                                              Helpers.Create<NewMechanics.AddSpeedBonusBasedOnRaceSize>(a => { a.small_race_speed_bonus = -5; a.normal_race_speed_bonus = -10; })
                                              );

            var curse5 = Helpers.CreateFeature("OracleCurse5Lame", 
                                               "Immune to Fatigue",
                                               "At 5th level, you are immune to the fatigued condition (but not exhaustion).",
                                               "",
                                               Helpers.GetIcon("e5aa306af9b91974a9b2f2cbe702f562"), // mercy fatigue
                                               FeatureGroup.None,
                                               UnitCondition.Fatigued.CreateImmunity(),
                                               SpellDescriptor.Fatigue.CreateBuffImmunity());

            var curse10 = Helpers.CreateFeature("OracleCurse10Lame", 
                                                "Effortless Armor",
                                                "At 10th level, your speed is never reduced by armor.",
                                                "",
                                                Helpers.GetIcon("e1291272c8f48c14ab212a599ad17aac"), // effortless armor
                                                FeatureGroup.None,
                                                // Conceptually similar to ArmorSpeedPenaltyRemoval, but doesn't need 2 ranks in the feat to work.
                                                AddMechanicsFeature.MechanicsFeatureType.ImmunToMediumArmorSpeedPenalty.CreateAddMechanics(),
                                                AddMechanicsFeature.MechanicsFeatureType.ImmunToArmorSpeedPenalty.CreateAddMechanics());

            var curse15 = Helpers.CreateFeature("OracleCurse15Lame", "Immune to Exhausted",
                                                "At 15th level, you are immune to the exhausted condition.",
                                                "be45e9251c134ac9baee97e1e3ffc30a",
                                                Helpers.GetIcon("25641bda25467224e930e8c70eaf9a83"), // mercy exhausted
                                                FeatureGroup.None,
                                                UnitCondition.Exhausted.CreateImmunity(),
                                                SpellDescriptor.Exhausted.CreateBuffImmunity());

            lame = createOracleCurseProgression("OracleLameCurseProgression", "Lame",
                                                "",
                                                curse, curse5, curse10, curse15);
        }


        static void createBlackened()
        {
            var burning_hands = library.Get<BlueprintAbility>("4783c3709a74a794dbe7c8e7e0b1b038");
            var scorching_ray = library.Get<BlueprintAbility>("cdb106d53c65bbc4086183d54c3b97c7");
            var burning_arc = library.Get<BlueprintAbility>("eaac3d36e0336cb479209a6f65e25e7c");
            var firebrand = library.Get<BlueprintAbility>("98734a2665c18cd4db71878b0532024a");
            var curse = Helpers.CreateFeature("OracleCurseBlackened",
                                              "Blackened",
                                              "You take a –4 penalty on weapon attack rolls, but you add burning hands to your list of spells known.",
                                              "",
                                              burning_hands.Icon,
                                              FeatureGroup.None,
                                              Helpers.CreateAddContextStatBonus(StatType.AdditionalAttackBonus, ModifierDescriptor.Other),
                                              Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CasterLevel, classes: getOracleArray(),
                                                                              progression: ContextRankProgression.Custom,
                                                                              customProgression: new (int, int)[] { (9, -4), (20, -2) }
                                                                              )
                                              );

            var curse5 = Helpers.CreateFeature("OracleCurse5Blackened", 
                                               curse.Name,
                                               "At 5th level, add scorching ray and burning arc to your list of spells known.",
                                               "",
                                               scorching_ray.Icon,
                                               FeatureGroup.None,
                                               scorching_ray.CreateAddKnownSpell(oracle_class, 2),
                                               burning_hands.CreateAddKnownSpell(oracle_class, 2)
                                               );

            var curse10 = Helpers.CreateFeature("OracleCurse10Blackened",
                                                curse.Name,
                                                "At 10th level, add wall of fire to your list of spells known and your penalty on weapon attack rolls is reduced to –2.",
                                                "",
                                                NewSpells.wall_of_fire.Icon,
                                                FeatureGroup.None,
                                                NewSpells.wall_of_fire.CreateAddKnownSpell(oracle_class, 4)
                                                );

            var curse15 = Helpers.CreateFeature("OracleCurse15Blackened", 
                                                curse.Name,
                                                "At 15th level, add delayed blast fireball to your list of spells known.",
                                                "",
                                                firebrand.Icon,
                                                FeatureGroup.None,
                                                firebrand.CreateAddKnownSpell(oracle_class, 7));

            blackened = createOracleCurseProgression("OracleBlackenedCurseProgression", "Blackened", 
                                                     "Your hands and forearms are shriveled and blackened, as if you had plunged your arms into a blazing fire, and your thin, papery skin is sensitive to the touch.",
                                                     curse, curse5, curse10, curse15);
        }


        static void createDeaf()
        {
            var curse = Helpers.CreateFeature("OracleCurseDeaf",
                                              "Deaf",
                                              "You cannot hear and suffer all of the usual penalties for being deafened: -4 penalty on initiative and -4 perception. You cast all of your spells as if they were modified by the Silent Spell feat. This does not increase their level or casting time.",
                                              "",
                                              Helpers.GetIcon("c3893092a333b93499fd0a21845aa265"),
                                              FeatureGroup.None,
                                              Helpers.CreateAddContextStatBonus(StatType.Initiative, ModifierDescriptor.Other),
                                              Helpers.CreateAddStatBonus(StatType.SkillPerception, -4, ModifierDescriptor.Other),
                                              Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CasterLevel, classes: getOracleArray(),
                                                                              progression: ContextRankProgression.Custom,
                                                                              customProgression: new (int, int)[] { (4, -4), (9, -2), (20, 0) }
                                                                              ),
                                              Helpers.Create<SpecificBuffImmunity>(s => s.Buff = Common.deafened)
                                              );


            var curse5 = Helpers.CreateFeature("OracleCurse5Deaf", 
                                               curse.Name,
                                               "At 5th level, you no longer receive a penalty on Perception checks, and the initiative penalty for being deaf is reduced to –2.",
                                               "",
                                               curse.Icon, FeatureGroup.None);

            var curse10 = Helpers.CreateFeature("OracleCurse10Deaf",
                                                curse.Name,
                                                "At 10th level, you receive a +3 competence bonus on Perception checks, and you do not suffer any penalty on initiative checks due to being deaf.",
                                                "",
                                                Helpers.GetIcon("c927a8b0cd3f5174f8c0b67cdbfde539"), // remove blindness
                                                FeatureGroup.None,
                                                Helpers.CreateAddStatBonus(StatType.SkillPerception, 3, ModifierDescriptor.Competence)
                                                );

            var curse15 = Helpers.CreateFeature("OracleCurse15Deaf",
                                                "Tremorsense",
                                                "At 15th level, you gain tremorsense out to a range of 30 feet.",
                                                "",
                                                Helpers.GetIcon("30e5dc243f937fc4b95d2f8f4e1b7ff3"), // see invisible
                                                FeatureGroup.None,
                                                Helpers.Create<Blindsense>(b => b.Range = 30.Feet()));
            deaf = createOracleCurseProgression("OracleDeafCurseProgression", "Deaf", "", curse, curse5, curse10, curse15);
        }


        static void createCloudedVision()
        {
            var curse = Helpers.CreateProgression("OracleCurseCloudedVision",
                                                  "Clouded Vision",
                                                  "Your eyes are obscured, making it difficult for you to see.\n"+
                                                  "You cannot see anything beyond 20 feet. Targets beyond this range have concealment, and you cannot target any point past that range.",
                                                  "",
                                                  Helpers.GetIcon("46fd02ad56c35224c9c91c88cd457791"), // blindness
                                                  FeatureGroup.None,
                                                  Helpers.Create<ConcealementMechanics.SetVisibilityLimit>(s => s.visibility_limit = 20.Feet()),
                                                  Helpers.Create<ConcealementMechanics.AddOutgoingConcealment>(a => 
                                                                                                              { a.CheckDistance = true;
                                                                                                                a.Descriptor = ConcealmentDescriptor.InitiatorIsBlind;
                                                                                                                a.DistanceGreater = 20.Feet();
                                                                                                                a.Concealment = Concealment.Total;
                                                                                                              }
                                                                                                              )
                                                  );
            var curse5 = Helpers.CreateProgression("OracleCurse5CloudedVision",
                                      curse.Name,
                                      "At 5th level, your vision distance increases to 30 feet.",
                                      "",
                                      curse.Icon,
                                      FeatureGroup.None,
                                      Common.createRemoveFeatureOnApply(curse),
                                      Helpers.Create<ConcealementMechanics.SetVisibilityLimit>(s => s.visibility_limit = 30.Feet()),
                                      Helpers.Create<ConcealementMechanics.AddOutgoingConcealment>(a =>
                                                                                                  {
                                                                                                      a.CheckDistance = true;
                                                                                                      a.Descriptor = ConcealmentDescriptor.InitiatorIsBlind;
                                                                                                      a.DistanceGreater = 30.Feet();
                                                                                                      a.Concealment = Concealment.Total;
                                                                                                  }
                                                                                                  )
                                      );
            var curse10 = Helpers.CreateFeature("OracleCurse10CloudedVision", 
                                                "Blindsense",
                                                "At 10th level, you gain blindsense out to a range of 30 feet.",
                                                "",
                                                Helpers.GetIcon("30e5dc243f937fc4b95d2f8f4e1b7ff3"), // see invisible
                                                FeatureGroup.None,
                                                Helpers.Create<Blindsense>(b => b.Range = 30.Feet()));
            var curse15 = Helpers.CreateFeature("OracleCurse15CloudedVision", 
                                                "Blindsight",
                                                "At 15th level, you gain blindsight out to a range of 15 feet.",
                                                "",
                                                Helpers.GetIcon("4cf3d0fae3239ec478f51e86f49161cb"), // true seeing
                                                FeatureGroup.None,
                                                Helpers.Create<Blindsense>(b => { b.Range = 15.Feet(); b.Blindsight = true; }));
            clouded_vision = createOracleCurseProgression("OracleCloudedVisionCurseProgression", "Clouded Vision", "Your eyes are obscured, making it difficult for you to see.",
                                                          curse, curse5, curse10, curse15);                
        }


        static BlueprintProgression createOracleCurseProgression(string name, string display_name, string base_description, params BlueprintFeature[] features)
        {
            var curse_description = base_description;
            foreach (var f in features)
            {
                curse_description += "\n";
                curse_description += f.Description;
            }
            var curse = Helpers.CreateProgression(name,
                                                  display_name,
                                                  curse_description,
                                                  "",
                                                  features[0].Icon,
                                                  FeatureGroup.None);

            curse.LevelEntries = new LevelEntry[] { Helpers.LevelEntry(1, features[0]), Helpers.LevelEntry(5, features[1]), Helpers.LevelEntry(10, features[2]), Helpers.LevelEntry(15, features[3]) };
            curse.UIGroups = Helpers.CreateUIGroups(features);
            curse.Classes = getOracleArray();
            return curse;
        }


        static void createCureInflictSpells()
        {
            var cure_spells = new BlueprintAbility[]
                {
                library.Get<BlueprintAbility>("5590652e1c2225c4ca30c4a699ab3649"),
                library.Get<BlueprintAbility>("6b90c773a6543dc49b2505858ce33db5"),
                library.Get<BlueprintAbility>("3361c5df793b4c8448756146a88026ad"),
                library.Get<BlueprintAbility>("41c9016596fe1de4faf67425ed691203"),
                library.Get<BlueprintAbility>("5d3d689392e4ff740a761ef346815074"),
                library.Get<BlueprintAbility>("571221cc141bc21449ae96b3944652aa"),
                library.Get<BlueprintAbility>("0cea35de4d553cc439ae80b3a8724397"),
                library.Get<BlueprintAbility>("1f173a16120359e41a20fc75bb53d449"),
                };
            var inflict_spells = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("e5af3674bb241f14b9a9f6b0c7dc3d27"),
                library.Get<BlueprintAbility>("65f0b63c45ea82a4f8b8325768a3832d"),
                library.Get<BlueprintAbility>("bd5da98859cf2b3418f6d68ea66cabbe"),
                library.Get<BlueprintAbility>("651110ed4f117a948b41c05c5c7624c0"),
                library.Get<BlueprintAbility>("9da37873d79ef0a468f969e4e5116ad2"),
                library.Get<BlueprintAbility>("03944622fbe04824684ec29ff2cec6a7"),
                library.Get<BlueprintAbility>("820170444d4d2a14abc480fcbdb49535"),
                library.Get<BlueprintAbility>("5ee395a2423808c4baf342a4f8395b19"),
            };


            BlueprintComponent[] add_cure_spells = new BlueprintComponent[cure_spells.Length];
            
            for (int i = 0; i < cure_spells.Length; i++)
            {
                add_cure_spells[i] = Helpers.CreateAddKnownSpell(cure_spells[i], oracle_class, i + 1);
            }
            var free_cure_spells = Helpers.CreateFeature("OracleFreeCureSpells",
                                                                "Cure Spells",
                                                                "In addition to the spells gained by oracles as they gain levels, each oracle also adds all of either the cure spells or the inflict spells to her list of spells known (cure spells include all spells with “cure” in the name, inflict spells include all spells with “inflict” in the name). These spells are added as soon as the oracle is capable of casting them. This choice is made when the oracle gains her first level and cannot be changed.",
                                                                "",
                                                                cure_spells[0].Icon,
                                                                FeatureGroup.None,
                                                                add_cure_spells);
            for (int i = 0; i < cure_spells.Length; i++)
            {
                cure_spells[i].AddRecommendNoFeature(free_cure_spells);
            }

            BlueprintComponent[] add_inflict_spells = new BlueprintComponent[inflict_spells.Length];

            for (int i = 0; i < inflict_spells.Length; i++)
            {
                add_inflict_spells[i] = Helpers.CreateAddKnownSpell(inflict_spells[i], oracle_class, i + 1);
            }
            var free_inflict_spells = Helpers.CreateFeature("OracleFreeInflictSpells",
                                                                "Inflict Spells",
                                                                "In addition to the spells gained by oracles as they gain levels, each oracle also adds all of either the cure spells or the inflict spells to her list of spells known (cure spells include all spells with “cure” in the name, inflict spells include all spells with “inflict” in the name). These spells are added as soon as the oracle is capable of casting them. This choice is made when the oracle gains her first level and cannot be changed.",
                                                                "",
                                                                inflict_spells[0].Icon,
                                                                FeatureGroup.None,
                                                                add_inflict_spells);
            for (int i = 0; i < inflict_spells.Length; i++)
            {
                inflict_spells[i].AddRecommendNoFeature(free_inflict_spells);
            }


            cure_inflict_spells_selection = Helpers.CreateFeatureSelection("OracleBonusSpellsFeature",
                                                                           "Bonus Spells",
                                                                           free_cure_spells.Description,
                                                                           "",
                                                                           null,
                                                                           FeatureGroup.None);
            cure_inflict_spells_selection.AllFeatures = new BlueprintFeature[] { free_cure_spells, free_inflict_spells };
        }


        static void createOracleOrisons()
        {
            oracle_orisons = library.CopyAndAdd<BlueprintFeature>(
                 "e62f392949c24eb4b8fb2bc9db4345e3", // cleric orisions
                 "OracleOrisonsFeature",
                 "");
            oracle_orisons.SetDescription("Oracles learn a number of orisons, or 0-level spells. These spells are cast like any other spell, but they do not consume any slots and may be used again.");
            oracle_orisons.ReplaceComponent<BindAbilitiesToClass>(c => c.CharacterClass = oracle_class);
        }


        static void createOracleProficiencies()
        {
            oracle_proficiencies = library.CopyAndAdd<BlueprintFeature>("8c971173613282844888dc20d572cfc9", //cleric proficiencies
                                                                "OracleProficiencies",
                                                                "");
            oracle_proficiencies.SetName("Oracle Proficiencies");
            oracle_proficiencies.SetDescription("Oracles are proficient with all simple weapons, light armor, medium armor, and shields (except tower shields). Some oracle revelations grant additional proficiencies.");
        }
    }
}
