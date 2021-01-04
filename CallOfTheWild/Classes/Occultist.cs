using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
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
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Buffs.Conditions;
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
    public class Occultist
    {
        static LibraryScriptableObject library => Main.library;
        internal static bool test_mode = false;
        static public BlueprintCharacterClass occultist_class;
        static public BlueprintProgression occultist_progression;

        static public BlueprintFeature occultist_proficiencies;
        static public BlueprintFeature occultist_knacks;
        static public BlueprintFeature occultist_spellcasting;
        static public BlueprintFeature mental_focus;
        static public BlueprintFeature magic_item_skill;


        static public Dictionary<SpellSchool, BlueprintAbilityResource> mental_focus_resource = new Dictionary<SpellSchool, BlueprintAbilityResource>();
        static public Dictionary<SpellSchool, BlueprintAbility> invest_focus_abilities = new Dictionary<SpellSchool, BlueprintAbility>();
        static public BlueprintFeatureSelection implement_mastery;
        static public Dictionary<SpellSchool, ImplementsEngine> implement_factories = new Dictionary<SpellSchool, ImplementsEngine>();
        static public Dictionary<SpellSchool, BlueprintFeature> base_implements = new Dictionary<SpellSchool, BlueprintFeature>();
        static public Dictionary<SpellSchool, BlueprintFeature> second_implements = new Dictionary<SpellSchool, BlueprintFeature>();
        static public Dictionary<SpellSchool, BlueprintFeatureSelection> new_spells_selection = new Dictionary<SpellSchool, BlueprintFeatureSelection>();
        static public Dictionary<SpellSchool, BlueprintCharacterClass> implement_mastery_classes = new Dictionary<SpellSchool, BlueprintCharacterClass>();

        static public BlueprintFeatureSelection focus_power_selection;
        static public BlueprintFeatureSelection first_implement_selection;
        static public BlueprintFeatureSelection implement_selection;
        static public BlueprintFeature repower_construct; //1 round -> 2 rounds -> 1 minute
        static public Dictionary<SpellSchool, UnityEngine.Sprite> implement_icons = new Dictionary<SpellSchool, UnityEngine.Sprite>();

        static public BlueprintBuff locked_focus_buff;

        internal static void createOccultistClass()
        {
            Main.logger.Log("Occultist class test mode: " + test_mode.ToString());
            var alchemsit_class = library.TryGet<BlueprintCharacterClass>("0937bec61c0dabc468428f496580c721");
            var bard_class = library.TryGet<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");

            occultist_class = Helpers.Create<BlueprintCharacterClass>();
            occultist_class.name = "OccultistClass";
            library.AddAsset(occultist_class, "");

            occultist_class.LocalizedName = Helpers.CreateString("Occultist.Name", "Occultist");
            occultist_class.LocalizedDescription = Helpers.CreateString("Occultist.Description",
                                                                         "The occultist focuses on the world around him, grounded in the powers that flow throughout his environment. He studies the magic that infuses everything, from psychic resonances left in everyday items to powerful incantations that fuel the mightiest spells.\n"
                                                                         + "The occultist channels his psychic might through implements—items that allow him to focus his power and produce incredible effects. For him, implements are more than simple tools. They are a repository of history and a tie to the events of the past. The occultist uses these implements to influence and change the present, adding his legend to theirs. Though some of these implements might be magic items in their own right, most of them are merely of historical or personal significance to the occultist.The occultist channels his psychic might through implements—items that allow him to focus his power and produce incredible effects. For him, implements are more than simple tools. They are a repository of history and a tie to the events of the past. The occultist uses these implements to influence and change the present, adding his legend to theirs. Though some of these implements might be magic items in their own right, most of them are merely of historical or personal significance to the occultist.\n"
                                                                         + "Role: Occultists are always eager to travel in the company of adventurers, explorers, and archaeologists, as those three groups of people have a knack for finding items with rich histories and great significance."
                                                                         );
            occultist_class.m_Icon = alchemsit_class.Icon;
            occultist_class.SkillPoints = alchemsit_class.SkillPoints;
            occultist_class.HitDie = DiceType.D8;
            occultist_class.BaseAttackBonus = alchemsit_class.BaseAttackBonus;
            occultist_class.FortitudeSave = alchemsit_class.FortitudeSave;
            occultist_class.ReflexSave = alchemsit_class.WillSave;
            occultist_class.WillSave = alchemsit_class.FortitudeSave;
            occultist_class.Spellbook = createOccultistSpellbook();
            occultist_class.ClassSkills = new StatType[] { StatType.SkillStealth, StatType.SkillThievery,
                                                      StatType.SkillKnowledgeArcana, StatType.SkillKnowledgeWorld, StatType.SkillLoreReligion,
                                                      StatType.SkillPerception, StatType.SkillPersuasion, StatType.SkillUseMagicDevice};
            occultist_class.IsDivineCaster = false;
            occultist_class.IsArcaneCaster = false;
            occultist_class.StartingGold = alchemsit_class.StartingGold;
            occultist_class.PrimaryColor = alchemsit_class.PrimaryColor;
            occultist_class.SecondaryColor = alchemsit_class.SecondaryColor;
            occultist_class.RecommendedAttributes = new StatType[] { StatType.Intelligence };
            occultist_class.NotRecommendedAttributes = new StatType[0];
            occultist_class.EquipmentEntities = alchemsit_class.EquipmentEntities;
            occultist_class.MaleEquipmentEntities = alchemsit_class.MaleEquipmentEntities;
            occultist_class.FemaleEquipmentEntities = alchemsit_class.FemaleEquipmentEntities;
            occultist_class.ComponentsArray = alchemsit_class.ComponentsArray;
            occultist_class.StartingItems = new BlueprintItem[]
            {
                library.Get<BlueprintItemArmor>("d7963e1fcf260c148877afd3252dbc91"), //scalemail
                library.Get<BlueprintItemWeapon>("6fd0a849531617844b195f452661b2cd"), //longsword
                library.Get<BlueprintItemShield>("f4cef3ba1a15b0f4fa7fd66b602ff32b"), //shield
                library.Get<BlueprintItemWeapon>("201f6150321e09048bd59e9b7f558cb0"), //longbow
                library.Get<BlueprintItemEquipmentUsable>("d52566ae8cbe8dc4dae977ef51c27d91"), //potion of cure light wounds
                library.Get<BlueprintItemEquipmentUsable>("f79c3fd5012a3534c8ab36dc18e85fb1"), //sleep
                library.Get<BlueprintItemEquipmentUsable>("fe244c39bdd5cb64eae65af23c6759de") //cause fear
            };

            createOccultistProgression();
            occultist_class.Progression = occultist_progression;
            occultist_class.Archetypes = new BlueprintArchetype[] { };//battle host, silksworn, reliquarian, haunt collector, panoply savant? necrocultist?
            Helpers.RegisterClass(occultist_class);
        }


        static public BlueprintCharacterClass[] getOccultistArray()
        {
            return new BlueprintCharacterClass[] { occultist_class };
        }

        static void createOccultistProgression()
        {
            /*abjuration: warding talisman -> mind barrier x
                aegis (as swift action for 2 pts ?) 1
                energy shield 3
                globe of negation ?
                unravelling lvl 5
            conjuration: casting focus -> servitor x
                flesh mend 3
                psychic fog 1
                purge corruption 5
                side step 7
            divination: third eye -> sudden insight x
                danger sight 1
                mind eye 5 TODO 
                in accordance with prophecy 9
                ?
            enchantment: glorious presence -> cloud mind x
                binding pattern 7
                inspired assault 1
                obey 1
                ?
            evocation: intense focus -> energy ray x
                energy blast: ((1d6 +2)/2 levels) 5
                energy ward ?
                light matrix ?
                radiance 1
                wall of power 9
            illusion: distortion (1% or miss chance on any attack, ignored with true sight) -> color beam (as blinding ray) x           
                shadow beast 9
                unseen - greater invisibility for 1 minute 7
                bedeveling aura (1 round/2 levels) 9
                terror  1
            necromancy: necromantic focus - > mind fear x
                flesh rot 3
                necromantic servant 3
                soulbound puppet 1
                pain wave 7
                spirit shroud 3
            transmuation: physical enchancement -> legacy weapon x
                mind over gravity (fly + 30 speed bonus) 7
                philosopher's touch 1
                quickness (haste + 1/6 levels) 5
                size alteration (enlarge reduce without limitation) 1
                sudden speed  1

            trappings of the warrior: martial skill -> combat feat (?) 
                counterstrike
                warrior's resilence (?)
                shield ally
            mage's paraphernalia: scholarly knowledge -> inspiration (convert any spell to wizard divination/evocation/necromancy spell of that level for 1 pt)
                metamagic knowledge
                metamagic master
                spell power
            */

            //repower construct to replace binding circles



            createKnacks();
            createProficiencies();
            createMagicItemSkill();
            createMentalFocus();
            createImplements();
            createFocusPowers();
            //createRepowerConstruct();
            createImplementMastery();


            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");

            occultist_progression = Helpers.CreateProgression("OccultistProgression",
                                                              occultist_class.Name,
                                                              occultist_class.Description,
                                                              "",
                                                              occultist_class.Icon,
                                                              FeatureGroup.None);
            occultist_progression.Classes = getOccultistArray();

            occultist_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, occultist_proficiencies, occultist_spellcasting, detect_magic,
                                                                                         occultist_knacks, first_implement_selection, implement_selection,
                                                                                         mental_focus, focus_power_selection,
                                                                                         library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                         library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")), // touch calculate feature                                                                                      
                                                                    Helpers.LevelEntry(2, implement_selection, magic_item_skill),
                                                                    Helpers.LevelEntry(3, focus_power_selection),
                                                                    Helpers.LevelEntry(4),
                                                                    Helpers.LevelEntry(5, focus_power_selection),
                                                                    Helpers.LevelEntry(6, implement_selection),
                                                                    Helpers.LevelEntry(7, focus_power_selection),
                                                                    Helpers.LevelEntry(8/*, repower_construct*/),
                                                                    Helpers.LevelEntry(9, focus_power_selection),
                                                                    Helpers.LevelEntry(10, implement_selection),
                                                                    Helpers.LevelEntry(11, focus_power_selection),
                                                                    Helpers.LevelEntry(12),
                                                                    Helpers.LevelEntry(13, focus_power_selection),
                                                                    Helpers.LevelEntry(14, implement_selection),
                                                                    Helpers.LevelEntry(15, focus_power_selection),
                                                                    Helpers.LevelEntry(16),
                                                                    Helpers.LevelEntry(17, focus_power_selection),
                                                                    Helpers.LevelEntry(18, implement_selection),
                                                                    Helpers.LevelEntry(19, focus_power_selection),
                                                                    Helpers.LevelEntry(20, implement_mastery)
                                                                    };

            occultist_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] {occultist_proficiencies, occultist_spellcasting, detect_magic,
                                                                                     occultist_knacks, mental_focus};
            occultist_progression.UIGroups = new UIGroup[] {Helpers.CreateUIGroup(first_implement_selection, implement_selection),
                                                            Helpers.CreateUIGroup(focus_power_selection),
                                                            Helpers.CreateUIGroup(magic_item_skill, /*repower_construct,*/ implement_mastery)
                                                           };
        }


        static void createImplementMastery()
        {
            implement_mastery = Helpers.CreateFeatureSelection("ImplementMasterySelection",
                                              "Implement Mastery",
                                              "At 20th level, an occultist learns to master one of his implements. He selects one implement school. Whenever he uses a focus power from an implement of that school, the DC to resist any of the effects increases by 2 and he treats his occultist level as 4 higher when determining the effects and duration of that power. In addition, the occultist gains 4 extra points of mental focus, but these points must always be invested in implements of the mastered school. He can’t save these points or expend them for any ability other than the focus powers of those implements.",
                                              "",
                                              LoadIcons.Image2Sprite.Create(@"AbilityIcons/MagicalSupremacy.png"),
                                              FeatureGroup.None
                                              );

            foreach (var kv in implement_mastery_classes)
            {
                var feature = Helpers.CreateFeature(kv.Key.ToString() + "ImplementMasteryFeature",
                                                    kv.Key.ToString() + " " + implement_mastery.Name,
                                                    implement_mastery.Description,
                                                    "",
                                                    implement_icons[kv.Key],
                                                    FeatureGroup.None,
                                                    Helpers.Create<ImplementMechanics.BonusInvestedFocusPoints>(b => { b.school = kv.Key; b.value = 4; b.resource = mental_focus_resource[kv.Key]; }),
                                                    Helpers.Create<FakeClassLevelMechanics.AddFakeClassLevel>(a => { a.fake_class = kv.Value; a.value = 4; })
                                                    );
                implement_mastery.AllFeatures = implement_mastery.AllFeatures.AddToArray(feature);
            }
        }


        static void createFocusPowers()
        {
            focus_power_selection = Helpers.CreateFeatureSelection("FocusPowerSelection",
                                                          "Focus Powers",
                                                          "At 1st level, an occultist learns the base focus power from both of his two implement schools and can select one more focus power from the list of those available to him through those schools.\n"
                                                          + "Whenever the occultist learns a new implement school, he gains the base power of that school. In addition, at 3rd level and every 2 levels thereafter, he learns a new focus power selected from the options granted by all of the implement schools he knows. The occultist can use focus powers only by expending mental focus.\n"
                                                          + "Unless otherwise noted, the DC for any saving throw against a focus power equals 10 + 1/2 the occultist’s level + the occultist’s Intelligence modifier. The occultist can’t select a focus power more than once. Some focus powers require him to reach a specific occultist level before he can choose them.",
                                                          "",
                                                          null,
                                                          FeatureGroup.None
                                                          );

            Dictionary<SpellSchool, BlueprintFeature[]> implement_powers = new Dictionary<SpellSchool, BlueprintFeature[]>()
            {
                {SpellSchool.Abjuration, new BlueprintFeature[]{implement_factories[SpellSchool.Abjuration].createAegis(),
                                                                implement_factories[SpellSchool.Abjuration].createEnergyShield(),
                                                                implement_factories[SpellSchool.Abjuration].createUnraveling()
                                                                //globe of negation
                                                               }
                },
                {SpellSchool.Conjuration, new BlueprintFeature[]{implement_factories[SpellSchool.Conjuration].createFleshMend(),
                                                                 implement_factories[SpellSchool.Conjuration].createPsychicFog(),
                                                                 implement_factories[SpellSchool.Conjuration].createPurgeCorruption(),
                                                                 implement_factories[SpellSchool.Conjuration].createSideStep()
                                                                }
                },
                {SpellSchool.Divination, new BlueprintFeature[]{implement_factories[SpellSchool.Divination].createDangerSight(),
                                                                implement_factories[SpellSchool.Divination].createInAccordanceWithProphecy(),
                                                                //mind eye
                                                               }
                },
                {SpellSchool.Enchantment, new BlueprintFeature[]{implement_factories[SpellSchool.Enchantment].createBindingPattern(),
                                                                implement_factories[SpellSchool.Enchantment].createInspiredAssault(),
                                                                implement_factories[SpellSchool.Enchantment].createObey(),
                                                                }
                },
                {SpellSchool.Evocation, new BlueprintFeature[]{implement_factories[SpellSchool.Evocation].createEnergyBlast(),
                                                                implement_factories[SpellSchool.Evocation].createRadiance(),
                                                                implement_factories[SpellSchool.Evocation].createWallOfPower(),
                                                                //light matrix
                                                                }
                },
                {SpellSchool.Illusion, new BlueprintFeature[]{implement_factories[SpellSchool.Illusion].createBedevelingAura(),
                                                                implement_factories[SpellSchool.Illusion].createShadowBeast(),
                                                                implement_factories[SpellSchool.Illusion].createTerror(),
                                                                implement_factories[SpellSchool.Illusion].createUnseen(),
                                                                }
                },
                {SpellSchool.Necromancy, new BlueprintFeature[]{implement_factories[SpellSchool.Necromancy].createFleshRot(),
                                                                implement_factories[SpellSchool.Necromancy].createNecromanticServant(),
                                                                implement_factories[SpellSchool.Necromancy].createPainWave(),
                                                                implement_factories[SpellSchool.Necromancy].createSoulboundPuppet(),
                                                                implement_factories[SpellSchool.Necromancy].createSpiritShroud(),
                                                                }
                },
                {SpellSchool.Transmutation, new BlueprintFeature[]{implement_factories[SpellSchool.Transmutation].createMindOverGravity(),
                                                                implement_factories[SpellSchool.Transmutation].createPhilosophersTouch(),
                                                                implement_factories[SpellSchool.Transmutation].createQuickness(),
                                                                implement_factories[SpellSchool.Transmutation].createSizeAlteration(),
                                                                implement_factories[SpellSchool.Transmutation].createSuddenSpeed(),
                                                                }
                },
            };

            foreach (var kv in implement_powers)
            {
                foreach (var f in kv.Value)
                {
                    f.AddComponent(Helpers.PrerequisiteFeature(base_implements[kv.Key]));
                    focus_power_selection.AllFeatures = focus_power_selection.AllFeatures.AddToArray(f);
                }
            }
        }


        static void createImplements()
        {
            first_implement_selection = Helpers.CreateFeatureSelection("BaseImplementSelection",
                                                                      "Implements",
                                                                      "At 1st level, an occultist learns to use two implement schools. At 2nd level and every 4 occultist levels thereafter, the occultist learns to use one additional implement school, to a maximum of seven schools at 18th level. Each implement school adds up to 6 spells of any level of that school of magic to the occultist’s spell list.\n"
                                                                      + "Each implement schools is represented by a small list of objects. Every day, the occultist selects one item from that school’s list to be his implement for the day for each implement school he knows. The occultist needs only one such item to cast spells of the corresponding school, unless he selected that implement schools multiple times, in which case he needs one item for each set of spells gained from that school. Implements don’t need to be magic items, and non-magical implements don’t take up a magic item slot even if they’re worn. Implements that are not magic items are often of some historical value or of personal significance to the occultist, such as the finger bone of a saint, the broken scepter of a long-dead king, the skull of a mentor’s familiar, or the glass eye of an uncanny ancestor.\n"
                                                                      + "Whenever an occultist casts a spell, he must have the corresponding implement in his possession and present the implement to the target or toward the area of effect.\n"
                                                                      + "Each implement schools also grants a base focus power. This power is added to the list of focus powers possessed by the occultist (see Mental Focus below). In addition, each implement schools grants access to a number of other focus powers that the occultist can select from using his mental focus class feature.",
                                                                      "",
                                                                      null,
                                                                      FeatureGroup.None
                                                                      );
         
            //initialize implement engines
            var schools = new SpellSchool[] { SpellSchool.Abjuration, SpellSchool.Conjuration, SpellSchool.Divination, SpellSchool.Enchantment,
                                              SpellSchool.Evocation, SpellSchool.Illusion, SpellSchool.Necromancy, SpellSchool.Transmutation };

            foreach (var s in schools)
            {
                implement_mastery_classes[s] = library.CopyAndAdd(occultist_class, s.ToString() + occultist_class.name, "");
                implement_mastery_classes[s].AddComponent(Helpers.Create<FakeClassLevelMechanics.FakeClass>());
                implement_factories[s] = new ImplementsEngine("Occultist", mental_focus_resource[s], 
                                                              getOccultistArray().AddToArray(implement_mastery_classes[s]),
                                                              StatType.Intelligence);
            }

            Dictionary<SpellSchool, (string flavor, BlueprintFeature base_power, BlueprintBuff[] resonant_power_buffs)> implement_data 
                = new Dictionary<SpellSchool, (string, BlueprintFeature, BlueprintBuff[])>
            {
                {SpellSchool.Abjuration, ("Abjuration implements are objects associated with protection and wards." ,
                                           implement_factories[SpellSchool.Abjuration].createMindBarrier(),
                                           new BlueprintBuff[]{implement_factories[SpellSchool.Abjuration].createWardingTalisman()}
                                           )
                },
                {SpellSchool.Conjuration, ("Implements used in conjuration allow the occultist to perform magic that transports or calls creatures.",
                                           implement_factories[SpellSchool.Conjuration].createServitor(),
                                           new BlueprintBuff[]{implement_factories[SpellSchool.Conjuration].createCastingFocus()}
                                           )
                },
                {SpellSchool.Divination, ("Implements of the divination school grant powers related to foresight and remote viewing.",
                                           implement_factories[SpellSchool.Divination].createSuddenInsight(),
                                           new BlueprintBuff[]{implement_factories[SpellSchool.Divination].createThirdEye()}
                                           )
                },
                {SpellSchool.Enchantment, ("Enchantment implements allow the occultist to befuddle the mind and charm his foes.",
                                           implement_factories[SpellSchool.Enchantment].createCloudMind(),
                                           new BlueprintBuff[]{implement_factories[SpellSchool.Enchantment].createGloriousPresence()}
                                           )
                },
                {SpellSchool.Evocation, ("Implements focused on evocation grant the ability to create and direct energy to protect and to destroy.",
                                           implement_factories[SpellSchool.Evocation].createEnergyRay(),
                                           new BlueprintBuff[]{implement_factories[SpellSchool.Evocation].createIntenseFocus()}
                                           )
                },
                {SpellSchool.Illusion, ("Illusion implements allow the occultist to distort the senses and cloak creatures from sight.",
                                           implement_factories[SpellSchool.Illusion].createColorBeam(),
                                           new BlueprintBuff[]{implement_factories[SpellSchool.Illusion].createDistortion()}
                                           )
                },
                {SpellSchool.Necromancy, ("Implements that draw power from necromancy can control undead and harm the living.",
                                           implement_factories[SpellSchool.Necromancy].createMindFear(),
                                           new BlueprintBuff[]{implement_factories[SpellSchool.Necromancy].createNecromanticFocus()}
                                           )
                },
                {SpellSchool.Transmutation,("Transmutation implements can alter the properties of both objects and creatures.",
                                           implement_factories[SpellSchool.Transmutation].createLegacyWeapon(),
                                           implement_factories[SpellSchool.Transmutation].createPhysicalEnhancement()
                                           )
                }
            };

            foreach (var s in schools)
            {
                bool starts_with_vowel = "aeiou".IndexOf(s.ToString().ToLower()) >= 0;
                var data = implement_data[s];
                var description = data.flavor + "\n"
                                 + $"Resonant Power: Each time the occultist invests mental focus into a{(starts_with_vowel ? "n" : "")} {s.ToString()} implement, the implement grants the following resonant power. The implement’s bearer gains the benefits of this power until the occultist refreshes his focus.\n"
                                 + (data.resonant_power_buffs.Length > 1 ? "Physical Enhancement" : data.resonant_power_buffs[0].Name) + ": " + data.resonant_power_buffs[0].Description + "\n"
                                 + $"Base Focus Power: All occultists who learn to use {s.ToString()} implements gain the following focus power.\n"
                                 + data.base_power.Name + ": " + data.base_power.Description;

                base_implements[s] = Helpers.CreateFeature(s.ToString() + "ImplementFeature",
                                                           s.ToString() + " Implement",
                                                           description,
                                                           "",
                                                           implement_icons[s],
                                                           FeatureGroup.Domain
                                                           );
                second_implements[s] = library.CopyAndAdd(base_implements[s], s.ToString() + "SecondImplementFeature", "");
                second_implements[s].SetName(base_implements[s].Name + " (Extra Spells)");
                base_implements[s].AddComponent(Helpers.CreateAddFact(data.base_power));
                second_implements[s].AddComponent(Helpers.PrerequisiteFeature(base_implements[s]));

                invest_focus_abilities[s].AddComponent(Helpers.Create<AbilityShowIfCasterHasFact>(a => a.UnitFact = base_implements[s]));

                if (s != SpellSchool.Transmutation)
                {
                    Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuffNoRemove(locked_focus_buff,
                                                                                              Helpers.CreateConditional(Helpers.Create<ImplementMechanics.ContextConditionInvestedFocusAmount>(c =>
                                                                                              {
                                                                                                  c.schools = new SpellSchool[] { s };
                                                                                                  c.amount = 1;
                                                                                                  c.locked_focus = false;
                                                                                              }
                                                                                              ),
                                                                                              Common.createContextActionApplyChildBuff(data.resonant_power_buffs[0])
                                                                                              )
                                                                                              );
                }
                else
                {
                    var toggles = new List<BlueprintActivatableAbility>();
                    foreach (var b in data.resonant_power_buffs)
                    {
                        var toggle_buff = library.CopyAndAdd(b, "Toggle" + b.name, "");
                        toggle_buff.SetBuffFlags(toggle_buff.GetBuffFlags() | BuffFlags.HiddenInUi);
                        toggle_buff.ComponentsArray = new BlueprintComponent[0];
                        var toggle = Common.buffToToggle(toggle_buff, CommandType.Free, true, Helpers.Create<ImplementMechanics.RestrictionInvestedFocus>(r => { r.amount = 3; r.school = s; r.locked_focus = false; }));
                        toggles.Add(toggle);
                        toggle.Group = ActivatableAbilityGroupExtension.PhysicalEnhancementResonantPower.ToActivatableAbilityGroup();
                        Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(locked_focus_buff, b, toggle_buff);
                    }
                    base_implements[s].AddComponent(Helpers.CreateAddFacts(toggles.ToArray()));
                }
                //var spell_selection = createSpellSelection(s);
                var spells_to_pick = createCreateSpellSelectionArrays(s);
                var spell_list = Common.combineSpellLists(s.ToString() + "OccultistSpellList",
                                          (spell, spelllist, lvl) =>
                                          {
                                              return spell.School == s;
                                          },
                                          occultist_class.Spellbook.SpellList);
                for (int i = 0; i < 6; i++)
                {
                    if (spells_to_pick[i].Item1)
                    {
                        base_implements[s].AddComponent(Helpers.Create<NewMechanics.addSpellChoice>(a => 
                        {
                            a.spell_book = occultist_class.Spellbook;
                            a.spell_level = i + 1;
                            a.spell_list = spell_list;
                        })
                        );
                    }
                    if (spells_to_pick[i].Item2)
                    {
                        second_implements[s].AddComponent(Helpers.Create<NewMechanics.addSpellChoice>(a =>
                        {
                            a.spell_book = occultist_class.Spellbook;
                            a.spell_level = i + 1;
                            a.spell_list = spell_list;
                        })
                        );
                    }
                }

                base_implements[s].AddComponents(Helpers.Create<ImplementMechanics.IncreaseResourceAmountBasedOnInvestedFocus>(r =>
                                                 {
                                                    r.resource = mental_focus_resource[s];
                                                    r.school = s;
                                                 })
                                                );
            }

            first_implement_selection.AllFeatures = base_implements.Values.ToArray().AddToArray(second_implements.Values.ToArray());
            implement_selection = library.CopyAndAdd(first_implement_selection, "ImplementSelection", "");
        }


        static (bool, bool)[] createCreateSpellSelectionArrays(SpellSchool school)
        {
            var num_spells_to_select = new (bool, bool)[6];
            for (int i = 0; i < 6; i ++)
            {
                var num_spells = occultist_class.Spellbook.SpellList.SpellsByLevel[i + 1].Spells.Count(s => s.School == school);
                num_spells_to_select[i] = (num_spells > 0, num_spells > 1);
            }

            return num_spells_to_select;
        }



        static BlueprintFeatureSelection createSpellSelection(SpellSchool school)
        {
            var spell_list = Common.combineSpellLists(school.ToString() + "OccultistSpellList",
                                                      (spell, spelllist, lvl) =>
                                                      {
                                                          return spell.School == school;
                                                      },
                                                      occultist_class.Spellbook.SpellList);

            BlueprintParametrizedFeature[] learn_spells = new BlueprintParametrizedFeature[6];
            for (int i = 1; i <= 6; i++)
            {
                var learn_spell = library.CopyAndAdd<BlueprintParametrizedFeature>("bcd757ac2aeef3c49b77e5af4e510956", $"Occultist{school.ToString()}Spells{i}ParametrizedFeature", "");
                learn_spell.SpellLevel = i;
                learn_spell.SpecificSpellLevel = true;
                learn_spell.SpellLevelPenalty = 0;
                learn_spell.SpellcasterClass = occultist_class;
                learn_spell.SpellList = spell_list;
                learn_spell.ReplaceComponent<LearnSpellParametrized>(l => { l.SpellList = spell_list; l.SpecificSpellLevel = true; l.SpellLevel = i; l.SpellcasterClass = occultist_class; });
                learn_spell.SetName(Helpers.CreateString($"{learn_spell.name}.Name", $"Learn {school.ToString()} Spell " + $"(level {i})"));
                learn_spell.SetDescription("The occultist’s selection of spells is limited. For each implement school he learns to use, he can add one spell of each level he can cast to his list of spells known, chosen from that school’s spell list. If he selects the same implement school multiple times, he adds one spell of each level from that school’s list for each time he has selected that school.");
                learn_spell.SetIcon(null);
                learn_spells[i] = learn_spell;
            }

            var spell_selection = Helpers.CreateFeatureSelection("OccultistSpellSelection",
                                                                 $"Learn {school.ToString()} Spell",
                                                                 learn_spells[0].Description,
                                                                 "",
                                                                 null,
                                                                 FeatureGroup.None);
            spell_selection.AllFeatures = learn_spells;
            return spell_selection;
        }


        static void createMentalFocus()
        {
            implement_icons.Add(SpellSchool.Abjuration, Helpers.GetIcon("c451fde0aec46454091b70384ea91989"));
            implement_icons.Add(SpellSchool.Conjuration, Helpers.GetIcon("567801abe990faf4080df566fadcd038"));
            implement_icons.Add(SpellSchool.Divination, Helpers.GetIcon("d7d18ce5c24bd324d96173fdc3309646"));
            implement_icons.Add(SpellSchool.Enchantment, Helpers.GetIcon("252363458703f144788af49ef04d0803"));
            implement_icons.Add(SpellSchool.Evocation, Helpers.GetIcon("f8019b7724d72a241a97157bc37f1c3b"));
            implement_icons.Add(SpellSchool.Illusion, Helpers.GetIcon("24d5402c0c1de48468b563f6174c6256"));
            implement_icons.Add(SpellSchool.Necromancy, Helpers.GetIcon("e9450978cc9feeb468fb8ee3a90607e3"));
            implement_icons.Add(SpellSchool.Transmutation, Helpers.GetIcon("b6a604dab356ac34788abf4ad79449ec"));
            implement_icons.Add(SpellSchool.Universalist, Helpers.GetIcon("0933849149cfc9244ac05d6a5b57fd80"));
            implement_icons.Add(SpellSchool.None, LoadIcons.Image2Sprite.Create(@"AbilityIcons/SchoolNone.png"));

            mental_focus = Helpers.CreateFeature("MentalFocusResource",
                                                 "Mental Focus",
                                                 "An occultist can invest a portion of his mental focus into his chosen implements for the day, allowing him to utilize a variety of abilities depending on the implements and the amount of mental focus invested in them.\n"
                                                 + "An occultist has a number of points of mental focus equal to his occultist level + his Intelligence modifier; these points refresh each day. He can divide this mental focus between his implements in any way he desires.\n"
                                                 + "Once mental focus is invested inside an implement, the implement gains the resonant power of its implement school, and the occultist can expend the mental focus stored in the implement to activate the associated focus powers he knows. If a resonant power grants a bonus that varies based on the amount of mental focus invested in the implement, the bonus is determined when the focus is invested, and is not reduced or altered by expending the mental focus invested in the item.\n"
                                                 + "The occultist refreshes his mental focus once each day after receiving at least 8 hours of sleep.\n"
                                                 + "The occultist can choose to save generic mental focus inside his own body instead of investing all of it, but expending this focus comes at a higher cost.\n"
                                                 + "Occultist can expend an amount of generic focus to restore equal amount of an appropriate implement focus that he has already spent.",
                                                 "",
                                                 implement_icons[SpellSchool.Universalist],
                                                 FeatureGroup.None,
                                                 Helpers.Create<ImplementMechanics.AddImplements>()
                                                 );

            locked_focus_buff = Helpers.CreateBuff("LockedFocusBuff",
                                                   "Mental Focus Invested",
                                                   "You have invested mental focus into your implements.",
                                                   "",
                                                   implement_icons[SpellSchool.Universalist],
                                                   null,
                                                   Helpers.CreateAddFactContextActions(deactivated: Helpers.Create<ImplementMechanics.ContextActionUnlockFocus>())
                                                   );
            locked_focus_buff.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.RemoveOnRest | BuffFlags.StayOnDeath);

            var lock_focus_ability = Helpers.CreateAbility("LockFocusAbility",
                                                           "Lock Invested Mental Focus",
                                                           "Lock invested mental focus until you rest.",
                                                           "",
                                                           implement_icons[SpellSchool.Universalist],
                                                           AbilityType.Special,
                                                           CommandType.Standard,
                                                           AbilityRange.Personal,
                                                           "",
                                                           "",
                                                           Helpers.CreateRunActions(Helpers.Create<ImplementMechanics.ContextActionLockFocus>(),
                                                                                    Common.createContextActionApplyBuff(locked_focus_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true)
                                                                                    ),
                                                           Helpers.Create<ImplementMechanics.AbilityCasterFocusLocked>(a => a.not = true)
                                                           );
            lock_focus_ability.setMiscAbilityParametersSelfOnly();
            Common.setAsFullRoundAction(lock_focus_ability);

            var reset_focus_ability = Helpers.CreateAbility("ResetFocusAbility",
                                               "Reset Invested Mental Focus",
                                               "Reset mental focus you invested into your implements and redistribute it anew.",
                                               "",
                                               implement_icons[SpellSchool.None],
                                               AbilityType.Special,
                                               CommandType.Free,
                                               AbilityRange.Personal,
                                               "",
                                               "",
                                               Helpers.CreateRunActions(Helpers.Create<ImplementMechanics.ContextActionResetFocus>()),
                                               Helpers.Create<ImplementMechanics.AbilityCasterFocusLocked>(a => a.not = true)
                                               );
            reset_focus_ability.setMiscAbilityParametersSelfOnly();

            List<BlueprintAbility> abilities = new List<BlueprintAbility>();
            abilities.Add(lock_focus_ability);
            abilities.Add(reset_focus_ability);

            foreach (SpellSchool school  in Enum.GetValues(typeof(SpellSchool)))
            {
                if (school == SpellSchool.None)
                {
                    continue;
                }

                var resource = Helpers.CreateAbilityResource(school.ToString() + "MentalFocusResource", "", "", "", null);
                if (school == SpellSchool.Universalist)
                {
                    resource.SetIncreasedByLevel(0, 1, getOccultistArray());
                    resource.SetIncreasedByStat(0, StatType.Intelligence);
                }
                else
                {
                    resource.SetFixedResource(0);
                }
                mental_focus_resource[school] = resource;
                mental_focus.AddComponent(resource.CreateAddAbilityResource());
                var reset_action = reset_focus_ability.GetComponent<AbilityEffectRunAction>().Actions;
                reset_action.Actions = reset_action.Actions.AddToArray(Helpers.Create<ResourceMechanics.ContextRestoreResource>(c => { c.full = true; c.Resource = resource; }));

                if (school == SpellSchool.Universalist)
                {
                    continue;
                }

                var invest_focus_ability = Helpers.CreateAbility(school.ToString() + "InvestFocusAbility",
                                                                 "Invest Mental Focus: " + school.ToString(),
                                                                 "Invest mental focus into specified implement.",
                                                                 "",
                                                                 implement_icons[school],
                                                                 AbilityType.Special,
                                                                 CommandType.Free,
                                                                 AbilityRange.Personal,
                                                                 "",
                                                                 "",
                                                                 Helpers.CreateRunActions(Helpers.Create<ImplementMechanics.ContextActionInvestFocus>(c => { c.school = school; c.resource = mental_focus_resource[school]; })),
                                                                 resource.CreateResourceLogic()
                                                                 );
                invest_focus_ability.setMiscAbilityParametersSelfOnly();
                abilities.Add(invest_focus_ability);
                invest_focus_abilities[school] = invest_focus_ability;
            }

            foreach (var ability in abilities)
            {
                ability.MaybeReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = mental_focus_resource[SpellSchool.Universalist]);
            }

            var wrapper = Common.createVariantWrapper("MentalFocusAbilityBase", "", abilities.ToArray());
            wrapper.SetNameDescriptionIcon(mental_focus);

            mental_focus.AddComponent(Helpers.CreateAddFact(wrapper));
            mental_focus.AddComponents(Helpers.Create<ImplementMechanics.IncreaseResourceAmountBasedOnInvestedFocus>(r =>
                                        {
                                            r.resource = mental_focus_resource[SpellSchool.Universalist];
                                            r.school = SpellSchool.Universalist;
                                        })
                                        );
        }


        static void createMagicItemSkill()
        {
            magic_item_skill = Helpers.CreateFeature("MagicItemSkillFeature",
                                                     "Magic Item Skill",
                                                     "At 2nd level, an occultist’s knowledge of magic items grants him a bonus when attempting to use them. He gains a bonus on all Use Magic Device checks equal to 1/2 his occultist level.",
                                                     "",
                                                     Helpers.GetIcon("f43ffc8e3f8ad8a43be2d44ad6e27914"),
                                                     FeatureGroup.None,
                                                     Helpers.CreateAddContextStatBonus(StatType.SkillUseMagicDevice, ModifierDescriptor.UntypedStackable),
                                                     Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getOccultistArray(), progression: ContextRankProgression.Div2)
                                                     );
            magic_item_skill.ReapplyOnLevelUp = true;
        }

        static void createProficiencies()
        {
            occultist_proficiencies = library.CopyAndAdd<BlueprintFeature>("c5e479367d07d62428f2fe92f39c0341", "OccultistProficiencies", "");
            occultist_proficiencies.SetNameDescription("Occultist Proficiencies",
                                                       "An occultist is proficient with all simple and martial weapons, light armor, medium armor, and shields (except tower shields)."
                                                       );
        }


        static void createKnacks()
        {
            occultist_knacks = Common.createCantrips("OccultistKnacksFeature",
                                                     "Knacks",
                                                     "An occultist knows a number of knacks, or 0-level psychic spells. These spells are cast like any other spell, but they can be cast any number of times per day.",
                                                     Helpers.GetIcon("55f14bc84d7c85446b07a1b5dd6b2b4c"),
                                                     "",
                                                     occultist_class, 
                                                     StatType.Intelligence, 
                                                     occultist_class.Spellbook.SpellList.SpellsByLevel[0].Spells.ToArray()
                                                     );
        }


        static BlueprintSpellbook createOccultistSpellbook()
        {
            var alchemist_spellbook = ResourcesLibrary.TryGetBlueprint<BlueprintSpellbook>("027d37761f3804042afa96fe3e9086cc");
            var occultist_spellbook = Helpers.Create<BlueprintSpellbook>();
            occultist_spellbook.name = "OccultistSpellbook";
            library.AddAsset(occultist_spellbook, "");
            occultist_spellbook.Name = occultist_class.LocalizedName;
            occultist_spellbook.SpellsPerDay = alchemist_spellbook.SpellsPerDay;
            occultist_spellbook.SpellsKnown = alchemist_spellbook.SpellsKnown;
            occultist_spellbook.Spontaneous = true;
            occultist_spellbook.IsArcane = false;
            occultist_spellbook.AllSpellsKnown = false;
            occultist_spellbook.CanCopyScrolls = false;
            occultist_spellbook.CastingAttribute = StatType.Intelligence;
            occultist_spellbook.CharacterClass = occultist_class;
            occultist_spellbook.CasterLevelModifier = 0;
            occultist_spellbook.CantripsType = CantripsType.Cantrips;
            occultist_spellbook.SpellsPerLevel = 0;

            occultist_spellbook.SpellList = Helpers.Create<BlueprintSpellList>();
            occultist_spellbook.SpellList.name = "OccultistSpellList";
            library.AddAsset(occultist_spellbook.SpellList, "");
            occultist_spellbook.SpellList.SpellsByLevel = new SpellLevelList[10];
            for (int i = 0; i < occultist_spellbook.SpellList.SpellsByLevel.Length; i++)
            {
                occultist_spellbook.SpellList.SpellsByLevel[i] = new SpellLevelList(i);
            }

            Common.SpellId[] spells = new Common.SpellId[]
            {
                new Common.SpellId( "55f14bc84d7c85446b07a1b5dd6b2b4c", 0), //daze
                new Common.SpellId( "f0f8e5b9808f44e4eadd22b138131d52", 0), //flare
                new Common.SpellId( "c3a8f31778c3980498d8f00c980be5f5", 0), //guidance
                new Common.SpellId( "95f206566c5261c42aa5b3e7e0d1e36c", 0), //mage light
                new Common.SpellId( "7bc8e27cba24f0e43ae64ed201ad5785", 0), //resistance
                new Common.SpellId( "5bf3315ce1ed4d94e8805706820ef64d", 0), //touch of fatigue

                new Common.SpellId( "4783c3709a74a794dbe7c8e7e0b1b038", 1),//burning hands                
                new Common.SpellId( "bd81a3931aa285a4f9844585b5d97e51", 1), //cause fear
                new Common.SpellId( NewSpells.command.AssetGuid, 1),
                new Common.SpellId( "5590652e1c2225c4ca30c4a699ab3649", 1), //cure light wounds
                new Common.SpellId( "c60969e7f264e6d4b84a1499fdcf9039", 1), //enlarge person
                new Common.SpellId( "4f8181e7a7f1d904fbaea64220e83379", 1), //expeditious retreat
                new Common.SpellId( "3e9d1119d43d07c4c8ba9ebfd1671952", 1), //gravity bow
                new Common.SpellId( "fd4d9fd7f87575d47aafe2a64a6e2d8d", 1), //hypnotism
                new Common.SpellId( "e5af3674bb241f14b9a9f6b0c7dc3d27", 1), //inflict light wounds
                new Common.SpellId( "779179912e6c6fe458fa4cfb90d96e10", 1), //lead blades
                new Common.SpellId( "9e1ad5d6f87d19e4d8883d63a6e35568", 1), //mage armor
                new Common.SpellId( NewSpells.magic_weapon.AssetGuid, 1),
                new Common.SpellId( "4e0e9aba6447d514f88eff1464cc4763", 1), //reduce person
                new Common.SpellId( "ef768022b0785eb43a18969903c537c4", 1), //shield
                new Common.SpellId( "ab395d2335d3f384e99dddee8562978f", 1), //shocking grasp
                new Common.SpellId( "bb7ecad2d3d2c8247a38f44855c99061", 1), //sleep
                new Common.SpellId( "f001c73999fb5a543a199f890108d936", 1), //vanish

                new Common.SpellId( NewSpells.animate_dead_lesser.AssetGuid, 2),
                //alied cloak
                new Common.SpellId( "14ec7a4e52e90fa47a4c8d63c69fd5c1", 2), //blur
                new Common.SpellId( NewSpells.blade_tutor.AssetGuid, 2), //should probablyalso be there due to flavor
                new Common.SpellId( "6b90c773a6543dc49b2505858ce33db5", 2), //cure moderate wounds
                new Common.SpellId( "b48b4c5ffb4eab0469feba27fc86a023", 2), //delay poison
                new Common.SpellId( "e1291272c8f48c14ab212a599ad17aac", 2), //effortless armor
                new Common.SpellId( "7a5b5bf845779a941a67251539545762", 2), //false life
                new Common.SpellId( "4709274b2080b6444a3c11c6ebbe2404", 2), //find traps
                new Common.SpellId( NewSpells.force_sword.AssetGuid, 2),
                //frost fall
                new Common.SpellId( NewSpells.ghostbane_dirge.AssetGuid, 2),
                new Common.SpellId( "ce7dad2b25acf85429b6c9550787b2d9", 2), //glitterdust
                new Common.SpellId( "65f0b63c45ea82a4f8b8325768a3832d", 2), //inflict moderate wounds
                new Common.SpellId( NewSpells.inflict_pain.AssetGuid, 2),
                new Common.SpellId( "89940cde01689fb46946b2f8cd7b66b7", 2), //invisibility
                new Common.SpellId( "3e4ab69ada402d145a5e0ad3ad4b8564", 3), //mirror image
                new Common.SpellId( "dee3074b2fbfb064b80b973f9b56319e", 2), //pernicious poison
                new Common.SpellId( "21ffef7791ce73f468b6fca4d9371e8b", 2), //resist energy
                new Common.SpellId( "30e5dc243f937fc4b95d2f8f4e1b7ff3", 2), //see invisibility
                new Common.SpellId( "08cb5f4c3b2695e44971bf5c45205df0", 2), //scare
                new Common.SpellId( "c3893092a333b93499fd0a21845aa265", 2), //sound burst
                //tactical acumen

                new Common.SpellId( "4b76d32feb089ad4499c3a1ce8e1ac27", 3), //animate dead
                new Common.SpellId( "989ab5c44240907489aba0a8568d0603", 3), //bestow curse
                new Common.SpellId( "2a9ef0e0b5822a24d88b16673a267456", 3), //call lightning
                new Common.SpellId( NewSpells.cloak_of_winds.AssetGuid, 3),
                new Common.SpellId( NewSpells.countless_eyes.AssetGuid, 3),
                new Common.SpellId( "3361c5df793b4c8448756146a88026ad", 3), //cure serious wounds
                new Common.SpellId( "7658b74f626c56a49939d9c20580885e", 3), //deep slumber
                new Common.SpellId( "92681f181b507b34ea87018e8f7a528a", 3), //dispel magic
                new Common.SpellId( "903092f6488f9ce45a80943923576ab3", 3), //displacement
                new Common.SpellId( "2d81362af43aeac4387a3d4fced489c3", 3), //fireball
                new Common.SpellId( NewSpells.flame_arrow.AssetGuid, 3),
                new Common.SpellId( NewSpells.fly.AssetGuid, 3),
                new Common.SpellId( "486eaff58293f6441a5c2759c4872f98", 3), //haste
                new Common.SpellId( "c7104f7526c4c524f91474614054547e", 3), //hold person
                new Common.SpellId( "bd5da98859cf2b3418f6d68ea66cabbe", 3), //inflict serious wounds
                new Common.SpellId( NewSpells.keen_edge.AssetGuid, 3),
                new Common.SpellId( "d2cff9243a7ee804cb6d5be47af30c73", 3), //lightning bolt
                new Common.SpellId( "2d4263d80f5136b4296d6eb43a221d7d", 3), //magic vestment
                new Common.SpellId( NewSpells.magic_weapon_greater.AssetGuid, 3),
                new Common.SpellId( "d2f116cfe05fcdd4a94e80143b67046f", 3), //protection from energy
                new Common.SpellId( "7bb0c402f7f789d4d9fae8ca87b4c7e2", 3), //resist energy communal
                new Common.SpellId( NewSpells.sands_of_time.AssetGuid, 3),
                new Common.SpellId( "f492622e473d34747806bdb39356eb89", 3), //slow
                new Common.SpellId( SpiritualWeapons.twilight_knife.AssetGuid, 3),


                new Common.SpellId( NewSpells.air_walk.AssetGuid, 4),
                new Common.SpellId( "7792da00c85b9e042a0fdfc2b66ec9a8", 4), //break enchantment
                new Common.SpellId( "cf6c901fb7acc904e85c63b342e9c949", 4), //confusion
                new Common.SpellId( "f72f8f03bf0136c4180cd1d70eb773a5", 4), //controlled blast fireball
                new Common.SpellId( "41c9016596fe1de4faf67425ed691203", 4), //cure critical wounds
                new Common.SpellId( NewSpells.daze_mass.AssetGuid, 4),
                new Common.SpellId( "e9cc9378fd6841f48ad59384e79e9953", 4), //death ward
                new Common.SpellId( "4a648b57935a59547b7a2ee86fb4f26a", 4), //dimensions door
                new Common.SpellId( "95f7cdcec94e293489a85afdf5af1fd7", 4), //dismissal
                new Common.SpellId( "20b548bf09bb3ea4bafea78dcb4f3db6", 4), //echolocation
                //etheric shards
                new Common.SpellId( "dc6af3b4fd149f841912d8a3ce0983de", 4), //false life greater
                new Common.SpellId( "d2aeac47450c76347aebbc02e4f463e0", 4), //fear
                new Common.SpellId( NewSpells.fire_shield.AssetGuid, 4),
                new Common.SpellId( "0087fc2d64b6095478bc7b8d7d512caf", 4), //freedom of movement
                new Common.SpellId( "41e8a952da7a5c247b3ec1c2dbb73018", 4), //hold monster
                new Common.SpellId( "fcb028205a71ee64d98175ff39a0abf9", 4), //ice storm
                new Common.SpellId( "651110ed4f117a948b41c05c5c7624c0", 4), //inflcit critical wounds
                new Common.SpellId( "ecaa0def35b38f949bd1976a6c9539e0", 4), //invisibility greater
                new Common.SpellId( "2a6eda8ef30379142a4b75448fb214a3", 4), //poison
                new Common.SpellId( NewSpells.rigor_mortis.AssetGuid, 4),
                new Common.SpellId( NewSpells.river_of_wind.AssetGuid, 4),
                new Common.SpellId( NewSpells.spirit_bound_blade.AssetGuid, 4),
                new Common.SpellId( "f09453607e683784c8fca646eec49162", 4), //shout
                new Common.SpellId( "c66e86905f7606c4eaa5c774f0357b2b", 4), //stoneskin
                new Common.SpellId( NewSpells.wall_of_fire.AssetGuid, 4),


                new Common.SpellId( NewSpells.air_walk_communal.AssetGuid, 5),
                new Common.SpellId( "d5a36a7ee8177be4f848b953d1c53c84", 5), //call lightning storm
                new Common.SpellId( NewSpells.command_greater.AssetGuid, 5),
                new Common.SpellId( "e7c530f8137630f4d9d7ee1aa7b1edc0", 5), //cone of cold     
                new Common.SpellId( "5d3d689392e4ff740a761ef346815074", 5), //cure light wounds mass
                new Common.SpellId( NewSpells.curse_major.AssetGuid, 5),
                new Common.SpellId( "f0f761b808dc4b149b08eaf44b99f633", 5), //dispel magic greater               
                new Common.SpellId( "d7cbd2004ce66a042aeab2e95a3c5c61", 5), //dominate person
                new Common.SpellId( "ebade19998e1f8542a1b55bd4da766b3", 5), //fire snake
                new Common.SpellId( NewSpells.ghostbane_dirge_mass.AssetGuid, 5),
                new Common.SpellId( "9da37873d79ef0a468f969e4e5116ad2", 5), //inflcit light wounds mass
                new Common.SpellId( NewSpells.inflict_pain_mass.AssetGuid, 5),
                new Common.SpellId( "eabf94e4edc6e714cabd96aa69f8b207", 5), //mind fog
                new Common.SpellId( NewSpells.overland_flight.AssetGuid, 5),
                new Common.SpellId( NewSpells.particulate_form.AssetGuid, 5),
                new Common.SpellId( "0a5ddfbcfb3989543ac7c936fc256889", 5), //spell resistance
                new Common.SpellId( "7c5d556b9a5883048bf030e20daebe31", 5), //stoneskin mass
                new Common.SpellId( NewSpells.suffocation.AssetGuid, 5),
                new Common.SpellId( "4cf3d0fae3239ec478f51e86f49161cb", 5), //true seeing

                new Common.SpellId( "36c8971e91f1745418cc3ffdfac17b74", 6), //blade barrier
                new Common.SpellId( "645558d63604747428d55f0dd3a4cb58", 6), //chain lightning
                new Common.SpellId( "7f71a70d822af94458dc1a235507e972", 6), //cloak of dreams
                new Common.SpellId( "5ef85d426783a5347b420546f91a677b", 6), //cold ice strike
                new Common.SpellId( NewSpells.contingency.AssetGuid, 6),
                new Common.SpellId( NewSpells.control_construct.AssetGuid, 6),
                new Common.SpellId( "571221cc141bc21449ae96b3944652aa", 6), //cure moderate wounds
                new Common.SpellId( "4aa7942c3e62a164387a73184bca3fc1", 6), //disintegrate
                new Common.SpellId( NewSpells.freezing_sphere.AssetGuid, 6),
                new Common.SpellId( "cc09224ecc9af79449816c45bc5be218", 6), //harm
                new Common.SpellId( "5da172c4c89f9eb4cbb614f3a67357d3", 6), //heal
                new Common.SpellId( "03944622fbe04824684ec29ff2cec6a7", 6), //inflict moderate wounds mass
                new Common.SpellId( "0e67fa8f011662c43934d486acc50253", 6), //prediction of failure
                new Common.SpellId( "093ed1d67a539ad4c939d9d05cfe192c", 6), //sirocco
                new Common.SpellId( "27203d62eb3d4184c9aced94f22e1806", 6), //transformation
                new Common.SpellId( "474ed0aa656cc38499cc9a073d113716", 6), //umbral strike

 
            };

            foreach (var spell_id in spells)
            {
                var spell = library.Get<BlueprintAbility>(spell_id.guid);
                spell.AddToSpellList(occultist_spellbook.SpellList, spell_id.level);
            }

            occultist_spellbook.AddComponent(Helpers.Create<SpellbookMechanics.PsychicSpellbook>());

            occultist_spellcasting = Helpers.CreateFeature("OccultistSpellCasting",
                                             "Occultist Spellcasting",
                                             "An occultist casts psychic spells drawn from the occultist spell list, limited by the implement groups he knows.\n"
                                             + "He can cast any spell he knows without preparing it ahead of time. Every occultist spell has an implement component. To learn or cast a spell, an occultist must have an Intelligence score equal to at least 10 + the spell level. The Difficulty Class for a saving throw against an occultist’s spell equals 10 + the spell level + the occultist’s Intelligence modifier.\n"
                                             + "An occultist can cast only a certain number of spells of each spell level per day. In addition, he gains bonus spells per day if he has a high Intelligence score.\n"
                                             + "The occultist’s selection of spells is limited. For each implement school he learns to use, he can add one spell of each level he can cast to his list of spells known, chosen from that school’s spell list. If he selects the same implement school multiple times, he adds one spell of each level from that school’s list for each time he has selected that school.\n"
                                             + "An occultist need not prepare his spells in advance. He can cast any spell he knows at any time, assuming he has not yet used up his allotment of spells per day for the spell’s level.",
                                             "",
                                             null,
                                             FeatureGroup.None);

            occultist_spellcasting.AddComponents(Helpers.Create<SpellFailureMechanics.PsychicSpellbook>(p => p.spellbook = occultist_spellbook),
                                                 Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.NaturalSpell));
            occultist_spellcasting.AddComponent(Helpers.Create<SpellbookMechanics.AddUndercastSpells>(p => p.spellbook = occultist_spellbook));
            occultist_spellcasting.AddComponent(Helpers.CreateAddFact(Investigator.center_self));
            occultist_spellcasting.AddComponents(Helpers.CreateAddFacts(occultist_spellbook.SpellList.SpellsByLevel[0].Spells.ToArray()));

            return occultist_spellbook;
        }
    }
}
