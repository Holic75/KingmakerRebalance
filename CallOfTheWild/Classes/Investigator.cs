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
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Root;
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
    public class Investigator
    {
        static LibraryScriptableObject library => Main.library;
        internal static bool test_mode = false;
        static public BlueprintCharacterClass investigator_class;
        static public BlueprintProgression investigator_progression;

        static public BlueprintFeature investigator_proficiencies;
        static public BlueprintFeature trapfinding;
        static public BlueprintFeature inspiration;
        static public BlueprintFeature inspiration_base, true_inspiration_base;
        static public BlueprintAbilityResource inspiration_resource;
        static Dictionary<string, BlueprintActivatableAbility> inspiration_ability_features = new Dictionary<string, BlueprintActivatableAbility>();
        static Dictionary<string, BlueprintFeature> inspiration_features = new Dictionary<string, BlueprintFeature>();

        static public BlueprintFeature poison_resistance;
        static public BlueprintFeature poison_immunity;
        static public BlueprintFeature trap_sense;
        static public BlueprintFeatureSelection investigator_talent_selection;
        static public BlueprintFeature studied_combat;
        static public BlueprintBuff studied_target_buff;
        static public BlueprintAbility studied_combat_base_ability;
        static public BlueprintAbility studied_combat_ability;
        static public BlueprintAbility studied_combat_ability_ignore_cooldown;
        static public BlueprintFeature studied_strike;
        static public BlueprintBuff studied_strike_buff;

        static public BlueprintFeature true_inspiration;
        static public BlueprintFeature infusion;
        static public BlueprintFeature mutagen;
        static public BlueprintFeature amazing_inspiration;
        static public BlueprintFeature blinding_strike;
        static public BlueprintFeature confusing_strike;
        static public BlueprintFeature combat_inspiration;
        static public BlueprintFeature device_talent;
        static public BlueprintFeature expanded_inspiration;
        static public BlueprintParametrizedFeature greater_combat_inspiration;
        static public BlueprintFeature quick_study;
        static public BlueprintFeature sapping_offensive;
        static public BlueprintFeature sickening_offensive;
        static public BlueprintFeature studied_defense;
        static BlueprintBuff studied_defense_toggle_buff;
        static public BlueprintFeature tenacious_inspiration;
        static public BlueprintFeature toppling_strike;
        static public BlueprintFeature underworld_inspiration;
        static public BlueprintFeature prolonged_study;

        static public BlueprintFeature ranged_study;
        static public BlueprintFeature extra_investigator_talent;
        static public BlueprintFeature extra_inspiration;

        static public BlueprintArchetype empiricist_archetype;
        static public BlueprintFeature ceaseless_observation, unfailing_logic, master_intellect;
        static public BlueprintArchetype questioner_archetype;
        static public BlueprintSpellbook questioner_spellbook;
        static public BlueprintFeature inspiration_for_subterfuge, questioner_spellcasting, know_it_all;
        static public BlueprintArchetype jinyiwei_archetype;
        static public BlueprintFeature divine_inspiration, celestial_insight;
        static public BlueprintSpellbook jinyiwei_spellbook;



        internal static void createInvestigatorClass()
        {
            Main.logger.Log("Investigator class test mode: " + test_mode.ToString());
            var rogue_class = library.TryGet<BlueprintCharacterClass>("299aa766dee3cbf4790da4efb8c72484");
            var bard_class = library.TryGet<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");

            investigator_class = Helpers.Create<BlueprintCharacterClass>();
            investigator_class.name = "InvestigatorClass";
            library.AddAsset(investigator_class, "");

            investigator_class.LocalizedName = Helpers.CreateString("Investigator.Name", "Investigator");
            investigator_class.LocalizedDescription = Helpers.CreateString("Investigator.Description",
                                                                         "Whether on the trail of a fugitive, a long - lost treasure trove, or a criminal mastermind, investigators are motivated by an intense curiosity about the world and use knowledge of it as a weapon. Mixing gumption and learnedness into a personal alchemy of daring, investigators are full of surprises. Observing the world around them, they gain valuable knowledge about the situation they’re in, process that information using inspiration and deduction, and cut to the quick of the matter in unexpected ways. Investigators are always evaluating situations they encounter, sizing up potential foes, and looking out for secret dangers, all while using their vast knowledge and powers of perception to find solutions to the most perplexing problems.\n"
                                                                         + "Role: Investigators live to solve mysteries and find inventive ways to get out of jams. They serve as advisors and support for their adventuring parties, but can take center stage when knowledge and cunning are needed. No slouches in battle, they know how to make surprise attacks and use inspiration to bring those attacks home."
                                                                         );
            investigator_class.m_Icon = rogue_class.Icon;
            investigator_class.SkillPoints = bard_class.SkillPoints;
            investigator_class.HitDie = DiceType.D8;
            investigator_class.BaseAttackBonus = rogue_class.BaseAttackBonus;
            investigator_class.FortitudeSave = bard_class.FortitudeSave;
            investigator_class.ReflexSave = bard_class.ReflexSave;
            investigator_class.WillSave = bard_class.WillSave;
            investigator_class.Spellbook = createInvestigatorSpellbook();
            investigator_class.ClassSkills = new StatType[] {StatType.SkillMobility, StatType.SkillStealth, StatType.SkillThievery,
                                                      StatType.SkillKnowledgeArcana, StatType.SkillKnowledgeWorld, StatType.SkillLoreNature, StatType.SkillLoreReligion,
                                                      StatType.SkillPerception, StatType.SkillPersuasion, StatType.SkillUseMagicDevice}; //everything except stealth and trickety
            investigator_class.IsDivineCaster = false;
            investigator_class.IsArcaneCaster = false;
            investigator_class.StartingGold = rogue_class.StartingGold;
            investigator_class.PrimaryColor = rogue_class.PrimaryColor;
            investigator_class.SecondaryColor = rogue_class.SecondaryColor;
            investigator_class.RecommendedAttributes = new StatType[] { StatType.Dexterity, StatType.Intelligence};
            investigator_class.NotRecommendedAttributes = new StatType[0];
            investigator_class.EquipmentEntities = rogue_class.EquipmentEntities;
            investigator_class.MaleEquipmentEntities = rogue_class.MaleEquipmentEntities;
            investigator_class.FemaleEquipmentEntities = rogue_class.FemaleEquipmentEntities;
            investigator_class.ComponentsArray = rogue_class.ComponentsArray;
            investigator_class.StartingItems = rogue_class.StartingItems;
           
            createInvestigatorProgression();
            investigator_class.Progression = investigator_progression;
            createEmpiricistArchetype();
            createQuestioner();
            createJynyiwei();

            investigator_class.Archetypes = new BlueprintArchetype[] {empiricist_archetype, questioner_archetype, jinyiwei_archetype};
            Helpers.RegisterClass(investigator_class);
            addToPrestigeClasses(); 
            createFeats();
        }

        static void addToPrestigeClasses()
        {
            Common.addReplaceSpellbook(Common.EldritchKnightSpellbookSelection, questioner_spellbook, "EldritchKnightQuestioner",
                                      Common.createPrerequisiteArchetypeLevel(investigator_class, questioner_archetype, 3));
            Common.addReplaceSpellbook(Common.ArcaneTricksterSelection, questioner_spellbook, "ArcaneTricksterQuestioner",
                                        Common.createPrerequisiteArchetypeLevel(investigator_class, questioner_archetype, 2));
            Common.addReplaceSpellbook(Common.MysticTheurgeArcaneSpellbookSelection, questioner_spellbook, "MysticTheurgeQuestioner",
                                        Common.createPrerequisiteArchetypeLevel(investigator_class, questioner_archetype, 2));
            Common.addReplaceSpellbook(Common.MysticTheurgeDivineSpellbookSelection, jinyiwei_spellbook, "MysticTheurgeJinyiwei",
                                        Common.createPrerequisiteArchetypeLevel(investigator_class, jinyiwei_archetype, 2));
            Common.addReplaceSpellbook(Common.DragonDiscipleSpellbookSelection, questioner_spellbook, "DragonDiscipleQuestioner",
                                       Common.createPrerequisiteArchetypeLevel(investigator_class, questioner_archetype, 1));
        }


        static void createJynyiwei()
        {
            jinyiwei_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "JinyiweiArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Jinyiwei");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Jinyiwei seek out and eliminate corruption wherever they find it, claiming they act under the celestial government’s mandate.");
            });
            Helpers.SetField(jinyiwei_archetype, "m_ParentClass", investigator_class);
            library.AddAsset(jinyiwei_archetype, "");

            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");
            createDivineInspiration();
            createCelestialInsight();

            jinyiwei_archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, inspiration),
                                                                   Helpers.LevelEntry(3, trap_sense),
                                                                   Helpers.LevelEntry(6, trap_sense),
                                                                   Helpers.LevelEntry(9, trap_sense),
                                                                   Helpers.LevelEntry(12, trap_sense),
                                                                   Helpers.LevelEntry(15, trap_sense),
                                                                   Helpers.LevelEntry(18, trap_sense) };
            jinyiwei_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, detect_magic, divine_inspiration),
                                                                  Helpers.LevelEntry(3, celestial_insight) };

            jinyiwei_archetype.ReplaceSpellbook = jinyiwei_spellbook;
            investigator_class.Progression.UIDeterminatorsGroup = investigator_class.Progression.UIDeterminatorsGroup.AddToArray(detect_magic, divine_inspiration);
            infusion.AddComponent(Common.prerequisiteNoArchetype(investigator_class, jinyiwei_archetype));
            mutagen.AddComponent(Common.prerequisiteNoArchetype(investigator_class, jinyiwei_archetype));
        }


        static void createDivineInspiration()
        {
            divine_inspiration = library.CopyAndAdd<BlueprintFeature>(inspiration, "DivineInspirationJinyiweiFeature", "");
            divine_inspiration.SetNameDescriptionIcon("Divine Inspiration",
                                                      "A jinyiwei follows a mandate to combat corruption in the mortal world. A jinyiwei adds her Wisdom modifier to her inspiration pool, rather than her Intelligence modifier. Additionally, rather than dabbling in the arcane arts of alchemy, a jinyiwei is empowered by the forces of celestial bureaucracy. She casts spells as an inquisitor of the same level.",
                                                      Helpers.GetIcon("a5e23522eda32dc45801e32c05dc9f96")//good hope
                                                      );
            divine_inspiration.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(type: AbilityRankType.ProjectilesCount ,baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Wisdom, min: 1));
            divine_inspiration.ReplaceComponent<RecalculateOnStatChange>(r => r.Stat = StatType.Wisdom);
            divine_inspiration.AddComponents(library.Get<BlueprintFeature>("4f898e6a004b2a84686c1fbd0ffe950e").ComponentsArray);//cantrips
            divine_inspiration.ReplaceComponent<LearnSpells>(l => l.CharacterClass = investigator_class);
            divine_inspiration.ReplaceComponent<BindAbilitiesToClass>(b => b.CharacterClass = investigator_class);
            jinyiwei_spellbook = library.CopyAndAdd<BlueprintSpellbook>("57fab75111f377248810ece84193a5a5", "JynyiweySpellbook", "");
            jinyiwei_spellbook.Name = Helpers.CreateString("JynyiweySpellbook.Name", "Jynyiwey");
        }


        static void createCelestialInsight()
        {
            celestial_insight = Helpers.CreateFeature("CelestialInsightJynyiweyFeature",
                                                      "Celestial Insight",
                                                      "At 3rd level, a jinyiwei learns to see through the types of magic that often lead others astray. She gains a +1 competence bonus on saving throws to resist enchantment and illusion effects. At 6th level and every 3 levels thereafter, these bonuses increase by 1 (to a maximum of +6 at 18th level).",
                                                      "",
                                                      Helpers.GetIcon("75a10d5a635986641bfbcceceec87217"),
                                                      FeatureGroup.None,
                                                      Helpers.Create<SavingThrowBonusAgainstSchoolAbilityValue>(s =>
                                                                                                                  {
                                                                                                                      s.ModifierDescriptor = ModifierDescriptor.Competence;
                                                                                                                      s.Value = 0;
                                                                                                                      s.Bonus = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                                                                      s.School = SpellSchool.Enchantment;
                                                                                                                  }
                                                                                                                ),
                                                       Helpers.Create<SavingThrowBonusAgainstSchoolAbilityValue>(s =>
                                                                                                                {
                                                                                                                    s.ModifierDescriptor = ModifierDescriptor.Competence;
                                                                                                                    s.Value = 0;
                                                                                                                    s.Bonus = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                                                                    s.School = SpellSchool.Illusion;
                                                                                                                }
                                                                                                                ),
                                                       Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getInvestigatorArray(),
                                                                                       progression: ContextRankProgression.DivStep, stepLevel: 3)
                                                       );
        }

        static void createQuestioner()
        {
            questioner_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "QuestionerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Questioner");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Dabblers in arcane magic and masters of stealth and guile, questioners are investigators who often find themselves mucking about in cases for less-than-savory clientele or that require an extra bit of subtlety.");
            });
            Helpers.SetField(questioner_archetype, "m_ParentClass", investigator_class);
            library.AddAsset(questioner_archetype, "");

            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");
            createInspirationForSubterfuge();
            createQuestionerSpellcasting();
            createKnowItAll();

            questioner_archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, inspiration)};
            questioner_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, detect_magic, questioner_spellcasting, inspiration_for_subterfuge),
                                                                  Helpers.LevelEntry(2, know_it_all) };

            questioner_archetype.ReplaceSpellbook = questioner_spellbook;
            investigator_class.Progression.UIGroups[3].Features.Add(know_it_all);
            investigator_class.Progression.UIDeterminatorsGroup = investigator_class.Progression.UIDeterminatorsGroup.AddToArray(detect_magic, questioner_spellcasting, inspiration_for_subterfuge);
            infusion.AddComponent(Common.prerequisiteNoArchetype(investigator_class, questioner_archetype));
            mutagen.AddComponent(Common.prerequisiteNoArchetype(investigator_class, questioner_archetype));
        }


        static void createInspirationForSubterfuge()
        {
            inspiration_for_subterfuge = library.CopyAndAdd<BlueprintFeature>(inspiration, "InspirationForSubterfugeFeature", "");
            inspiration_for_subterfuge.SetNameDescriptionIcon("Inspiration for Subterfuge",
                                                              "A questioner can use inspiration on any Knowledge or Stealth checks he attempts without expending a use of inspiration, provided he’s trained in that skill.",
                                                              Helpers.GetIcon("c7e1d5ef809325943af97f093e149c4f")
                                                              );

            var stealt_ability = inspiration_for_subterfuge.GetComponents<AddFacts>().Where(a => a.Facts.Contains(inspiration_ability_features["SkillStealth"])).First();
            inspiration_for_subterfuge.RemoveComponent(stealt_ability);
            inspiration_for_subterfuge.AddComponent(Helpers.CreateAddFact(inspiration_features["SkillStealth"]));
        }


        static void createQuestionerSpellcasting()
        {
            questioner_spellbook = library.CopyAndAdd<BlueprintSpellbook>("bc04fc157a8801d41b877ad0d9af03dd", "QuestionerSpellbook", "");
            questioner_spellbook.Name = Helpers.CreateString("QuestionerSpellbook.Name", "Questioner");
            questioner_spellbook.CastingAttribute = StatType.Intelligence;

            questioner_spellcasting = Helpers.CreateFeature("QuestionerSpellCasting",
                                                            "Questioner Spellcasting",
                                                            "A questioner casts arcane spells drawn from the bard spell list. He can cast any spell he knows without preparing it ahead of time. To learn or cast a spell, a questioner must have an Intelligence score equal to at least 10 + the spell’s level. The saving throw DC against a questioner’s spell is equal to 10 + the spell’s level + the questioner’s Intelligence modifier.\n"
                                                            + "Like other spellcasters, a questioner can cast only a certain number of spells of each spell level per day. He knows the same number of spells and receives the same number of spell slots per day as a bard of his investigator level, including for cantrips. In addition, he receives bonus spells per day if he has a high Intelligence score.\n"
                                                            + "A questioner need not prepare his spells in advance. He can cast any bard spell he knows at any time, assuming he has not yet used up his allotment of spells per day for the spell’s level.\n"
                                                            + "A questioner can cast bard spells while wearing light armor and using a shield without incurring the normal arcane spell failure chance. Like any other arcane spellcaster, a questioner wearing medium or heavy armor incurs a chance of arcane spell failure. A multiclass questioner still incurs the normal arcane spell failure chance for arcane spells received from other classes.\n"
                                                            + "A questioner can notselect infusion or mutagen investigator talents.",
                                                            "",
                                                            Helpers.GetIcon("55edf82380a1c8540af6c6037d34f322"),
                                                            FeatureGroup.None,
                                                            Common.createArcaneArmorProficiency(ArmorProficiencyGroup.Light,
                                                                                                ArmorProficiencyGroup.Buckler,
                                                                                                ArmorProficiencyGroup.LightShield,
                                                                                                ArmorProficiencyGroup.HeavyShield,
                                                                                                ArmorProficiencyGroup.TowerShield)
                                                            );
            questioner_spellcasting.AddComponents(library.Get<BlueprintFeature>("4f422e8490ec7d94592a8069cce47f98").ComponentsArray);//cantrips
            questioner_spellcasting.ReplaceComponent<LearnSpells>(l => l.CharacterClass = investigator_class);
            questioner_spellcasting.ReplaceComponent<BindAbilitiesToClass>(b => { b.CharacterClass = investigator_class; b.Stat = StatType.Intelligence; });
        }


        static void createKnowItAll()
        {
            know_it_all = Helpers.CreateFeature("KnowItAllQuestionerFeature",
                                                "Know It All",
                                                "At 2nd level, a questioner receives a +1 bonus on skill checks for all Knowledge skills. This bonus increases by 1 at 5th level and every 3 investigator levels thereafter, to a maximum of +6 at 17th level",
                                                "",
                                                Helpers.GetIcon("65cff8410a336654486c98fd3bacd8c5"), //owls wisdom
                                                FeatureGroup.None,
                                                Helpers.CreateAddContextStatBonus(StatType.SkillKnowledgeArcana, ModifierDescriptor.Other),
                                                Helpers.CreateAddContextStatBonus(StatType.SkillKnowledgeWorld, ModifierDescriptor.Other),
                                                Helpers.CreateAddContextStatBonus(StatType.SkillLoreNature, ModifierDescriptor.Other),
                                                Helpers.CreateAddContextStatBonus(StatType.SkillLoreReligion, ModifierDescriptor.Other),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getInvestigatorArray(),
                                                                                progression: ContextRankProgression.StartPlusDivStep,
                                                                                startLevel: 2, stepLevel: 3)
                                                );
        }


        static void createEmpiricistArchetype()
        {
            empiricist_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "EmpiricistArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Empiricist");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Champions of deductive reasoning and logical insight, empiricists put their faith in facts, data, confirmed observations, and consistently repeatable experiments—these things are their currency of truth.");
            });
            Helpers.SetField(empiricist_archetype, "m_ParentClass", investigator_class);
            library.AddAsset(empiricist_archetype, "");

            createCeaselessObservation();
            createUnfailingLogic();
            createMasterIntellect();

            empiricist_archetype.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(2, poison_resistance),
                                                                    Helpers.LevelEntry(11, poison_immunity),
                                                                    Helpers.LevelEntry(20, true_inspiration)
                                                                   };
            empiricist_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(2, ceaseless_observation),
                                                                  Helpers.LevelEntry(4, unfailing_logic),
                                                                  Helpers.LevelEntry(20, master_intellect) };

            investigator_class.Progression.UIGroups = investigator_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(ceaseless_observation, unfailing_logic, master_intellect));
        }


        static void createCeaselessObservation()
        {
            ceaseless_observation = Helpers.CreateFeature("CeaselessObservationInvestigator",
                                                          "Ceaseless Observation",
                                                          "An empiricist’s ability to notice the minutiae of almost everything that happens around him allows him to make shrewd and insightful calculations about people and even inanimate objects. At 2nd level, an empiricist uses his Intelligence modifier instead of the skill’s typical ability for all Trickery, Perception, and Use Magic Device checks.",
                                                          "",
                                                          Helpers.GetIcon("b3da3fbee6a751d4197e446c7e852bcb"),
                                                          FeatureGroup.None,
                                                          Helpers.Create<StatReplacementMechanics.ReplaceBaseStatForStatTypeLogic>(s =>
                                                          {
                                                              s.StatTypeToReplaceBastStatFor = StatType.SkillThievery;
                                                              s.NewBaseStatType = StatType.Intelligence;
                                                          }),
                                                          Helpers.Create<StatReplacementMechanics.ReplaceBaseStatForStatTypeLogic>(s =>
                                                          {
                                                              s.StatTypeToReplaceBastStatFor = StatType.SkillPerception;
                                                              s.NewBaseStatType = StatType.Intelligence;
                                                          }),
                                                          Helpers.Create<StatReplacementMechanics.ReplaceBaseStatForStatTypeLogic>(s =>
                                                          {
                                                              s.StatTypeToReplaceBastStatFor = StatType.SkillUseMagicDevice;
                                                              s.NewBaseStatType = StatType.Intelligence;
                                                          }),
                                                          Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Charisma),
                                                          Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Dexterity),
                                                          Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Wisdom),
                                                          Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Intelligence)
                                                         );
        }


        static void createUnfailingLogic()
        {
            unfailing_logic = Helpers.CreateFeature("UnfailingLogicInvestigatorFeature",
                                                    "Unfailing Logic",
                                                    "An empiricist’s grasp of facts and data teaches him to anchor himself in reality, granting resistance to even the most potent illusions. At 4th level, an empiricist gains a +2 insight bonus on all Will saving throws against illusion spells or spell-like abilities that allow a save to disbelieve their effects. At 8th level, the empiricist’s insight bonus increases to +4. At 16th level, he gains immunity to all illusion spells and spell-like abilities that allow a save to disbelieve the effects.",
                                                    "",
                                                    Helpers.GetIcon("1bb08308c9f6a5e4697887cd438b7221"), //judgement protection
                                                    FeatureGroup.None,
                                                    Helpers.Create<NewMechanics.SpecificSavingThrowBonusAgainstSchool>(s =>
                                                                                                                        {
                                                                                                                            s.Value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                                                                            s.type = SavingThrowType.Will;
                                                                                                                            s.School = SpellSchool.Illusion;
                                                                                                                            s.ModifierDescriptor = ModifierDescriptor.Insight;
                                                                                                                        }
                                                                                                                      ),
                                                    Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getInvestigatorArray(),
                                                                                    progression: ContextRankProgression.Custom,
                                                                                    customProgression: new (int, int)[] {(7, 2),
                                                                                                                         (15, 4),
                                                                                                                         (20, 100)}
                                                                                    )
                                                    );
        }


        static void createMasterIntellect()
        {
            master_intellect = Helpers.CreateFeature("MasterIntellectInvestigatorFeature",
                                                     "Master Intellect",
                                                     "At 20th level, an empiricist’s powers of reason and deduction become almost superhuman, and he is able to use them in nearly all aspects of life. At 20th level, an empiricist can use inspiration on all skills (even ones he is not trained in) and all ability checks without spending inspiration.",
                                                     "",
                                                     Helpers.GetIcon("ae4d3ad6a8fda1542acf2e9bbc13d113"),
                                                     FeatureGroup.None,
                                                     Helpers.CreateAddFact(true_inspiration_base)
                                                     );
        }


        static void createFeats()
        {
            extra_investigator_talent = library.CopyAndAdd<BlueprintFeatureSelection>(investigator_talent_selection, "ExtraInvestigatorTalent", "");
            extra_investigator_talent.SetNameDescription("Extra Investigator Talent",
                                                          " You gain one additional investigator talent. You must meet the prerequisites for this investigator talent.\n"
                                                          + "Special: You can take this feat multiple times. Each time you do, you gain another investigator talent.");
            extra_investigator_talent.AddComponent(Helpers.PrerequisiteFeature(investigator_talent_selection));
            extra_investigator_talent.Groups = new FeatureGroup[] { FeatureGroup.Feat };


            extra_inspiration = Helpers.CreateFeature("ExtraInspirationFeature",
                                                      "Extra Inspiration",
                                                      "You gain three extra use per day of inspiration in your inspiration pool.\n"
                                                      + "Special: If you have levels in the investigator class, you can take this feat multiple times. Each time you do, you gain three extra uses of inspiration per day.",
                                                      "",
                                                      inspiration.Icon,
                                                      FeatureGroup.Feat,
                                                      Helpers.Create<IncreaseResourceAmount>(i => { i.Resource = inspiration_resource; i.Value = 3; }),
                                                      Helpers.PrerequisiteFeature(inspiration_base)
                                                      );
            extra_inspiration.Ranks = 10;
            library.AddFeats(extra_investigator_talent, extra_inspiration);
        }


        static BlueprintCharacterClass[] getInvestigatorArray()
        {
            return new BlueprintCharacterClass[] { investigator_class };
        }


        static BlueprintSpellbook createInvestigatorSpellbook()
        {
            var spellbook = library.CopyAndAdd<BlueprintSpellbook>("027d37761f3804042afa96fe3e9086cc", "InvestigatorSpellbook", "");
            spellbook.Name = Helpers.CreateString("InvestigatorSpellbook.Name", "Investigator");

            return spellbook;
        }


        static void createInvestigatorProgression()
        {
            createInvestigatorProficiencies();
            createTrapfinding();
            createInspiration();            
            createTrapSense();
            createPoisonResistanceAndImmunity();
            createStudiedCombat();
            createStudiedStrike();
            createInvestigatorTalents();

            investigator_progression = Helpers.CreateProgression("InvestigatorProgression",
                                                                   investigator_class.Name,
                                                                   investigator_class.Description,
                                                                   "",
                                                                   investigator_class.Icon,
                                                                   FeatureGroup.None);
            investigator_progression.Classes = getInvestigatorArray();

            investigator_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, investigator_proficiencies, trapfinding, inspiration,
                                                                                        library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                        library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")),  // touch calculate feature};
                                                                    Helpers.LevelEntry(2, poison_resistance),
                                                                    Helpers.LevelEntry(3, trap_sense, investigator_talent_selection),
                                                                    Helpers.LevelEntry(4, studied_combat, studied_strike),
                                                                    Helpers.LevelEntry(5, investigator_talent_selection),
                                                                    Helpers.LevelEntry(6, trap_sense),
                                                                    Helpers.LevelEntry(7, investigator_talent_selection),
                                                                    Helpers.LevelEntry(8),
                                                                    Helpers.LevelEntry(9, investigator_talent_selection, trap_sense),
                                                                    Helpers.LevelEntry(10),
                                                                    Helpers.LevelEntry(11, investigator_talent_selection, poison_immunity),
                                                                    Helpers.LevelEntry(12, trap_sense),
                                                                    Helpers.LevelEntry(13, investigator_talent_selection),
                                                                    Helpers.LevelEntry(14),
                                                                    Helpers.LevelEntry(15, investigator_talent_selection, trap_sense),
                                                                    Helpers.LevelEntry(16),
                                                                    Helpers.LevelEntry(17, investigator_talent_selection),
                                                                    Helpers.LevelEntry(18, trap_sense),
                                                                    Helpers.LevelEntry(19, investigator_talent_selection),
                                                                    Helpers.LevelEntry(20, true_inspiration)
                                                                    };

            investigator_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { investigator_proficiencies, trapfinding, inspiration };
            investigator_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(investigator_talent_selection),
                                                                Helpers.CreateUIGroup(trap_sense),
                                                                Helpers.CreateUIGroup(poison_resistance, poison_immunity),
                                                                Helpers.CreateUIGroup(studied_combat, true_inspiration)
                                                               };
        }


        static void createInvestigatorTalents()
        {
            investigator_talent_selection = Helpers.CreateFeatureSelection("InvestigatorTalentsFeatureSelection",
                                                                           "Investigator Talents",
                                                                           "At 3rd level and every 2 levels thereafter, an investigator gains an investigator talent. Except where otherwise noted, each investigator talent can only be selected once.\n"
                                                                           + "Some investigator talents add effects to an investigator’s studied combat or studied strike. Only one of these talents can be applied to an individual attack",
                                                                           "",
                                                                           null,
                                                                           FeatureGroup.None);
            infusion = library.Get<BlueprintFeature>("57d5077b301ade749b840b0ea9230bb9");
            mutagen = library.CopyAndAdd<BlueprintFeature>("cee8f65448ce71c4b8b8ca13751dd8ea", "InvestigatorMutagen", "");
            foreach (var c in mutagen.GetComponents<SpellLevelByClassLevel>())
            {
                var new_c = c.CreateCopy();
                new_c.Class = investigator_class;
                mutagen.ReplaceComponent(c, new_c);
            }

            createQuickAndProlongedStudy();
            createSappingOffensive();
            createSickeningOffensive();
            createTopplingStrike();
            createBlindingStrike();
            createConfusingStrike();



            investigator_talent_selection.AllFeatures = new BlueprintFeature[]
            {
                amazing_inspiration,
                combat_inspiration,
                device_talent,
                expanded_inspiration,
                greater_combat_inspiration,
                studied_defense,
                tenacious_inspiration,
                underworld_inspiration,
                library.Get<BlueprintFeature>("68a23a419b330de45b4c3789649b5b41"), //canny observer
                library.Get<BlueprintFeature>("97a6aa2b64dd21a4fac67658a91067d7"), //fast stealth
                library.Get<BlueprintFeature>("6087e0c9801b5eb48bf48d6e75116aad"), //iron guts
                library.Get<BlueprintFeature>("4cd06a915daa74f4094952f2b3314b3b"), //terrain mastery
                infusion,
                mutagen,
                quick_study,
                prolonged_study,
                sapping_offensive,
                sickening_offensive,
                toppling_strike,
                blinding_strike,
                confusing_strike
            };                                                                
        }


        static void createConfusingStrike()
        {
            var buff = Helpers.CreateBuff("InvestigatorConfusingStrikeBuff",
                                         "Confusing Strike",
                                         "When the investigator deals damage with studied strike, the opponent must succeed at a Fortitude saving throw or become confused for 1d4+1 rounds. A successful saving throw reduces the duration to 1 round. The DC for this Fortitude save is equal to 10 + 1/2 the investigator’s level + his Intelligence modifier. Constructs, mindless creatures, oozes, plants, undead, incorporeal creatures, and creatures immune to critical hits are not affected by this ability.",
                                         "",
                                         Helpers.GetIcon("886c7407dc629dc499b9f1465ff382df"), //confused
                                         null);
            var activatable_ability = Helpers.CreateActivatableAbility("InvestigatorConfusingStrikeToggleAbility",
                                                                       buff.Name,
                                                                       buff.Description,
                                                                       "",
                                                                       buff.Icon,
                                                                       buff,
                                                                       AbilityActivationType.Immediately,
                                                                       CommandType.Free,
                                                                       null);
            activatable_ability.Group = ActivatableAbilityGroupExtension.InvestigatorStudiedStrike.ToActivatableAbilityGroup();
            activatable_ability.DeactivateImmediately = true;

            confusing_strike = Common.ActivatableAbilityToFeature(activatable_ability, false);
            confusing_strike.AddComponent(Helpers.PrerequisiteClassLevel(investigator_class, 19));

            var confused = library.Get<BlueprintBuff>("886c7407dc629dc499b9f1465ff382df");

            var apply_confused = Common.createContextActionApplyBuff(confused, Helpers.CreateContextDuration(1, DurationRate.Rounds, DiceType.D4, 1), dispellable: false);
            var action = Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(Helpers.CreateConditionalSaved(null, apply_confused)));
            var checked_action = Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Common.createContextConditionCasterHasFact(buff),
                                                                                              Common.createContextConditionHasBuffFromCaster(studied_target_buff)),
                                                                                              action);
            var checked_action_ranged = Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Common.createContextConditionCasterHasFact(buff),
                                                                                                     Common.createContextConditionCasterHasFact(ranged_study),
                                                                                                     Common.createContextConditionHasBuffFromCaster(studied_target_buff)),
                                                                  action);
            var extra_components = new BlueprintComponent[]
            {
                Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(checked_action),
                                                                 check_weapon_range_type: true, wait_for_attack_to_resolve: true),
                 Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(checked_action_ranged),
                                                                  check_weapon_range_type: true,
                                                                  range_type: AttackTypeAttackBonus.WeaponRangeType.Ranged, wait_for_attack_to_resolve: true)
            };

            studied_strike_buff.ComponentsArray = extra_components.AddToArray(studied_strike_buff.ComponentsArray);
        }


        static void createBlindingStrike()
        {
            var buff = Helpers.CreateBuff("InvestigatorBlindingStrikeBuff",
                                         "Blinding Strike",
                                         "When the investigator deals damage with studied strike, the opponent must succeed at a Fortitude saving throw or be permanently blinded. A successful saving throw reduces this to dazzled for 1d4 rounds. The DC for this Fortitude save is equal to 10 + 1/2 the investigator’s level + his Intelligence modifier. This talent has no effect on creatures that do not rely on eyes for sight or creatures with more than two eyes (although multiple critical hits might cause blindness, at the GM’s discretion). Blindness can be cured by heal, regeneration, remove blindness/deafness, or similar abilities.",
                                         "",
                                         Helpers.GetIcon("46fd02ad56c35224c9c91c88cd457791"), //blindness
                                         null);
            var activatable_ability = Helpers.CreateActivatableAbility("InvestigatorBlindingStrikeToggleAbility",
                                                                       buff.Name,
                                                                       buff.Description,
                                                                       "",
                                                                       buff.Icon,
                                                                       buff,
                                                                       AbilityActivationType.Immediately,
                                                                       CommandType.Free,
                                                                       null);
            activatable_ability.Group = ActivatableAbilityGroupExtension.InvestigatorStudiedStrike.ToActivatableAbilityGroup();
            activatable_ability.DeactivateImmediately = true;

            blinding_strike = Common.ActivatableAbilityToFeature(activatable_ability, false);
            blinding_strike.AddComponent(Helpers.PrerequisiteClassLevel(investigator_class, 17));

            var blind = library.Get<BlueprintBuff>("187f88d96a0ef464280706b63635f2af");
            var dazzled = library.Get<BlueprintBuff>("df6d1025da07524429afbae248845ecc");

            var apply_blind = Common.createContextActionApplyBuff(blind, Helpers.CreateContextDuration(), is_permanent: true);
            var apply_dazzled = Common.createContextActionApplyBuff(dazzled, Helpers.CreateContextDuration(0, DurationRate.Rounds, DiceType.D4, 1), dispellable: false);
            var action = Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(Helpers.CreateConditionalSaved(apply_dazzled, apply_blind)));
            var checked_action = Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Common.createContextConditionCasterHasFact(buff),
                                                                                              Common.createContextConditionHasBuffFromCaster(studied_target_buff)),
                                                                                              action);
            var checked_action_ranged = Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Common.createContextConditionCasterHasFact(buff),
                                                                                                     Common.createContextConditionCasterHasFact(ranged_study),
                                                                                                     Common.createContextConditionHasBuffFromCaster(studied_target_buff)),
                                                                  action);
            var extra_components = new BlueprintComponent[]
            {
                Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(checked_action),
                                                                 check_weapon_range_type: true, wait_for_attack_to_resolve: true),
                 Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(checked_action_ranged),
                                                                  check_weapon_range_type: true,
                                                                  range_type: AttackTypeAttackBonus.WeaponRangeType.Ranged, wait_for_attack_to_resolve: true)
            };

            studied_strike_buff.ComponentsArray = extra_components.AddToArray(studied_strike_buff.ComponentsArray);
        }


        static void createTopplingStrike()
        {           
            var buff = Helpers.CreateBuff("InvestigatorTopplingStrikeBuff",
                                         "Toppling Strike",
                                         "When the investigator deals damage with studied strike, he can perform a trip combat maneuver as a free action against the creature damaged by studied strike.",
                                         "",
                                         Helpers.GetIcon("95851f6e85fe87d4190675db0419d112"),
                                         null);
            var activatable_ability = Helpers.CreateActivatableAbility("InvestigatorTopplingStrikeToggleAbility",
                                                                       buff.Name,
                                                                       buff.Description,
                                                                       "",
                                                                       buff.Icon,
                                                                       buff,
                                                                       AbilityActivationType.Immediately,
                                                                       CommandType.Free,
                                                                       null);
            activatable_ability.Group = ActivatableAbilityGroupExtension.InvestigatorStudiedStrike.ToActivatableAbilityGroup();
            activatable_ability.DeactivateImmediately = true;

            toppling_strike = Common.ActivatableAbilityToFeature(activatable_ability, false);
            toppling_strike.AddComponent(Helpers.PrerequisiteClassLevel(investigator_class, 9));

            var action = Helpers.Create<ContextActionCombatManeuver>(c => { c.Type = Kingmaker.RuleSystem.Rules.CombatManeuver.Trip; c.OnSuccess = Helpers.CreateActionList(); });
            var checked_action = Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Common.createContextConditionCasterHasFact(buff), 
                                                                                              Common.createContextConditionHasBuffFromCaster(studied_target_buff)),
                                                                                              action);
            var checked_action_ranged = Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Common.createContextConditionCasterHasFact(buff),
                                                                                                     Common.createContextConditionCasterHasFact(ranged_study),
                                                                                                     Common.createContextConditionHasBuffFromCaster(studied_target_buff)),
                                                                  action);
            var extra_components = new BlueprintComponent[]
            {
                Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(checked_action),
                                                                 check_weapon_range_type: true, wait_for_attack_to_resolve: true),
                 Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(checked_action_ranged),
                                                                  check_weapon_range_type: true,
                                                                  range_type: AttackTypeAttackBonus.WeaponRangeType.Ranged, wait_for_attack_to_resolve: true)
            };

            studied_strike_buff.ComponentsArray = extra_components.AddToArray(studied_strike_buff.ComponentsArray);
        }


        static void createSappingOffensive()
        {
            var slow_reactions_buff = library.Get<BlueprintBuff>("d6b572cac9e3ad142a13dfc4c658ef10");
            var icon = Helpers.GetIcon("ce72662a812b1f242849417b2c784b5e"); //confounding blades
            var apply_slow_reactions = Helpers.CreateConditional(Common.createContextConditionHasBuffFromCaster(studied_target_buff),
                                                           Common.createContextActionApplyBuff(slow_reactions_buff, Helpers.CreateContextDuration(1), dispellable: false)
                                                           );
            var ranged_apply_slow_reactions = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(ranged_study), apply_slow_reactions);
            var buff = Helpers.CreateBuff("InvestigatorSappingOffensiveBuff",
                                          "Sapping Offensive",
                                          "When the investigator damages a studied target, that creature cannot make attacks of opportunity for 1 round.",
                                          "",
                                          icon,
                                          null,
                                          Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(apply_slow_reactions),
                                                                                           check_weapon_range_type: true, wait_for_attack_to_resolve: true),
                                          Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(ranged_apply_slow_reactions),
                                                                                           check_weapon_range_type: true,
                                                                                           range_type: AttackTypeAttackBonus.WeaponRangeType.Ranged, wait_for_attack_to_resolve: true)
                                          );
            var toggle = Helpers.CreateActivatableAbility("InvestigatorSappingOffensiveToggleAbility",
                                                          buff.Name,
                                                          buff.Description,
                                                          "",
                                                          buff.Icon,
                                                          buff,
                                                          AbilityActivationType.Immediately,
                                                          CommandType.Free,
                                                          null);
            toggle.Group = ActivatableAbilityGroupExtension.InvestigatorStudiedCombat.ToActivatableAbilityGroup();
            sapping_offensive = Common.ActivatableAbilityToFeature(toggle, false);
            sapping_offensive.AddComponent(Helpers.PrerequisiteClassLevel(investigator_class, 5));
        }


        static void createSickeningOffensive()
        {
            var sickened = library.Get<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323");
            var icon = Helpers.GetIcon("4e42460798665fd4cb9173ffa7ada323"); //sickened
            var apply_sickened = Helpers.CreateConditional(Common.createContextConditionHasFact(studied_target_buff),
                                                           Common.createContextActionApplyBuff(sickened, Helpers.CreateContextDuration(1), dispellable: false)
                                                           );
            var ranged_apply_sickened = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(ranged_study), apply_sickened);
            var buff = Helpers.CreateBuff("InvestigatorSickeningOffensiveBuff",
                                          "Sickening Offensive",
                                          "When the investigator damages a studied target, that creature is also sickened for 1 round. ",
                                          "",
                                          icon,
                                          null,
                                          Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(apply_sickened),
                                                                                           check_weapon_range_type: true, wait_for_attack_to_resolve: true),
                                          Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(ranged_apply_sickened),
                                                                                           check_weapon_range_type: true,
                                                                                           range_type: AttackTypeAttackBonus.WeaponRangeType.Ranged, wait_for_attack_to_resolve: true)
                                          );
            var toggle = Helpers.CreateActivatableAbility("InvestigatorSickeningOffensiveToggleAbility",
                                                          buff.Name,
                                                          buff.Description,
                                                          "",
                                                          buff.Icon,
                                                          buff,
                                                          AbilityActivationType.Immediately,
                                                          CommandType.Free,
                                                          null);
            toggle.Group = ActivatableAbilityGroupExtension.InvestigatorStudiedCombat.ToActivatableAbilityGroup();
            sickening_offensive = Common.ActivatableAbilityToFeature(toggle, false);
            sickening_offensive.AddComponent(Helpers.PrerequisiteClassLevel(investigator_class, 7));
        }


        static void createQuickAndProlongedStudy()
        {
            var icon = Helpers.GetIcon("b3da3fbee6a751d4197e446c7e852bcb"); //true seeing
            var quick_study_ability = library.CopyAndAdd<BlueprintAbility>(studied_combat_ability, "QuickStudyStudiedCombatAbility", "");
            quick_study_ability.ActionType = CommandType.Swift;
            quick_study_ability.SetName("Quick " + studied_combat_ability.Name);
            quick_study_ability.SetIcon(icon);

            var quick_study_ignore_cooldown = library.CopyAndAdd<BlueprintAbility>(studied_combat_ability_ignore_cooldown, "QuickStudyStudiedCombatIgnoreCooldownAbility", "");
            quick_study_ignore_cooldown.ActionType = CommandType.Swift;
            quick_study_ignore_cooldown.SetName("Quick " + studied_combat_ability.Name);
            quick_study_ignore_cooldown.SetIcon(icon);

            quick_study = Helpers.CreateFeature("QuickStudyInvestigatorTalentFeature",
                                                "Quick Study",
                                                "An investigator can use his studied combat ability as swift action instead of a move action.",
                                                "",
                                                icon,
                                                FeatureGroup.None);
            quick_study_ability.AddComponent(Common.createAbilityShowIfCasterHasFact(quick_study));
            quick_study_ignore_cooldown.AddComponent(Common.createAbilityShowIfCasterHasFact(quick_study));

            var variants_component = studied_combat_base_ability.GetComponent<AbilityVariants>();
            variants_component.Variants = variants_component.Variants.AddToArray(quick_study_ability, quick_study_ignore_cooldown);

            quick_study.AddComponent(Helpers.PrerequisiteFeature(studied_combat));


            prolonged_study = Helpers.CreateFeature("ProlongedStudyInvestigatorTalentFeature",
                                                    "Prolonged Study",
                                                    "The investigator can study his opponents for long periods of time. The effects of his studied combat ability last for a number of rounds equal to twice his Intelligence modifier (minimum 2) or until he deals damage with a studied strike, whichever comes first.",
                                                    "",
                                                    null,
                                                    FeatureGroup.None,
                                                    Helpers.PrerequisiteClassLevel(investigator_class, 13),
                                                    Helpers.Create<AutoMetamagic>(a =>
                                                                                    {
                                                                                        a.Metamagic = Metamagic.Extend;
                                                                                        Helpers.SetField(a, "m_AllowedAbilities", 3);
                                                                                        a.Abilities = new List<BlueprintAbility>();
                                                                                        a.Abilities.Add(studied_combat_ability);
                                                                                        a.Abilities.Add(studied_combat_ability_ignore_cooldown);
                                                                                        a.Abilities.Add(quick_study_ability);
                                                                                        a.Abilities.Add(quick_study_ignore_cooldown);
                                                                                    }
                                                                                  )
                                                    );
        }


        static void createInspiration()
        {
            inspiration_resource = Helpers.CreateAbilityResource("InvestigatorInspirationResource", "", "", "", null);
            inspiration_resource.SetIncreasedByLevelStartPlusDivStep(0, 2, 1, 2, 1, 0, 0.0f, getInvestigatorArray());
            //inspiration_resource.SetIncreasedByStat(0, StatType.Intelligence);

            inspiration_base = Helpers.CreateFeature("InvestigatorInspirationBase",
                                                     "Inspiration",
                                                     "",
                                                     "",
                                                     null,
                                                     FeatureGroup.None);
            inspiration_base.HideInCharacterSheetAndLevelUp = true;
            true_inspiration_base = Helpers.CreateFeature("InvestigatorTrueInspirationBase",
                                                             "",
                                                             "",
                                                             "",
                                                             null,
                                                             FeatureGroup.None);
            true_inspiration_base.HideInCharacterSheetAndLevelUp = true;

            Dictionary<string, BlueprintBuff> inspiration_buffs = new Dictionary<string, BlueprintBuff>();
            var description = "An investigator is beyond knowledgeable and skilled—he also possesses keen powers of observation and deduction that far surpass the abilities of others. An investigator typically uses these powers to aid in their investigations, but can also use these flashes of inspiration in other situations.\n"
                              + "An investigator has the ability to augment skill checks and ability checks through his brilliant inspiration. The investigator has an inspiration pool equal to his investigator level + his Intelligence modifier (minimum 1). An investigator’s inspiration pool refreshes each day, typically after he gets a restful night’s sleep. As a free action, he can expend one use of inspiration from his pool to add 1d6 to the result of that check, including any on which he takes 10 or 20. This choice is made after the check is rolled and before the results are revealed. An investigator can only use inspiration once per check or roll. The investigator can use inspiration on any Knowledge skill checks without expending a use of inspiration.\n"
                              + "Inspiration can also be used on attack rolls and saving throws, at the cost of expending two uses of inspiration each time from the investigator’s pool.";

            inspiration = Helpers.CreateFeature("InspirationFeature",
                                                "Inspiration",
                                                description,
                                                "",
                                                Helpers.GetIcon("4ebaf39efb8ffb64baf92784808dc49c"),
                                                FeatureGroup.None,
                                                Helpers.CreateAddFact(inspiration_base),
                                                inspiration_resource.CreateAddAbilityResource(),
                                                Helpers.Create<ResourceMechanics.ContextIncreaseResourceAmount>(c =>
                                                                                                                {
                                                                                                                    c.Value = Helpers.CreateContextValue(AbilityRankType.ProjectilesCount);
                                                                                                                    c.Resource = inspiration_resource;
                                                                                                                }
                                                                                                                ),
                                                Helpers.CreateContextRankConfig(type: AbilityRankType.ProjectilesCount, baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Intelligence, min: 1),
                                                Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Intelligence)
                                                );
            createTrueInspiration();
            createTenaciousInspiration();
            createAmazingInspiration();
            createCombatInspiration();
            createExpandedInspirtation();
            createGreaterCombatInspiration();
            createUnderworldInspiration();
            createDeviceTalent();

            DiceType[] dice_type = new DiceType[] { DiceType.D6, DiceType.D8 };
            var dice_count_context = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureListRanks,
                                                                     featureList: new BlueprintFeature[] { inspiration_base, true_inspiration });
            var dice_type_context = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureListRanks,
                                                                    type: AbilityRankType.StatBonus,
                                                                    featureList: new BlueprintFeature[] { inspiration_base, amazing_inspiration });

            inspiration.AddComponent(Helpers.Create<NewMechanics.AddRandomBonusOnSkillCheckAndConsumeResource>(a =>
               {
                   a.stats = new StatType[] { StatType.SkillKnowledgeArcana, StatType.SkillKnowledgeWorld, StatType.SkillLoreNature, StatType.SkillLoreReligion };
                   a.resource = null;
                   a.dices = dice_type;
                   a.dice_count = Helpers.CreateContextValue(AbilityRankType.Default);
                   a.dice_type = Helpers.CreateContextValue(AbilityRankType.StatBonus);
               }));
            inspiration.AddComponents(dice_count_context, dice_type_context);


            var ability_checks_inspiration_buff = Helpers.CreateBuff("InvestigatorInspirationAbilityChecksBuff",
                                                                     "Inspiration: Ability Checks",
                                                                     description,
                                                                     "",
                                                                     Helpers.GetIcon("c7773d1b408fea24dbbb0f7bf3eb864e"), //transmutation strength
                                                                     null,
                                                                     dice_count_context,
                                                                     dice_type_context,
                                                                     Helpers.Create<NewMechanics.AddRandomBonusOnSkillCheckAndConsumeResource>(a =>
                                                                     {
                                                                         a.stats = new StatType[] { StatType.Strength, StatType.Dexterity, StatType.Constitution,                                                                                                    StatType.Intelligence, StatType.Wisdom, StatType.Charisma };
                                                                         a.resource = inspiration_resource;
                                                                         a.amount = 1;
                                                                         a.dices = dice_type;
                                                                         a.dice_count = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                         a.dice_type = Helpers.CreateContextValue(AbilityRankType.StatBonus);
                                                                         a.allow_reroll_fact = tenacious_inspiration;
                                                                         a.cost_reducing_facts = new BlueprintUnitFact[] { true_inspiration_base };
                                                                     }));
            inspiration_buffs.Add("AbilityChecks", ability_checks_inspiration_buff);

            //will use skill_foci icons
            var single_skills = new (StatType, string, string)[] { (StatType.SkillAthletics, "Athletics", "9db907332bdaec1468cff3a99efef5b4"),
                                                                   (StatType.SkillMobility, "Mobility", "52dd89af385466c499338b7297896ded"),
                                                                   (StatType.SkillStealth, "Stealth", "3a8d34905eae4a74892aae37df3352b9"),
                                                                   (StatType.SkillThievery, "Trickery", "7feda1b98f0c169418aa9af78a85953b"),
                                                                   (StatType.SkillPerception, "Perception", "f74c6bdf5c5f5374fb9302ecdc1f7d64"),
                                                                   (StatType.SkillUseMagicDevice, "Use Magic Device", "f43ffc8e3f8ad8a43be2d44ad6e27914") };

            foreach (var s in single_skills)
            {
                var buff = Helpers.CreateBuff($"InvestigatorInspiration{s.Item1.ToString()}Buff",
                                              $"Inspiration: {s.Item2}",
                                              description,
                                              "",
                                              Helpers.GetIcon(s.Item3),
                                              null,
                                              dice_count_context,
                                              dice_type_context,
                                              Helpers.Create<NewMechanics.AddRandomBonusOnSkillCheckAndConsumeResource>(a =>
                                                {
                                                    a.stats = new StatType[] { s.Item1 };
                                                    a.resource = inspiration_resource;
                                                    a.amount = 1;
                                                    a.dices = dice_type;
                                                    a.dice_count = Helpers.CreateContextValue(AbilityRankType.Default);
                                                    a.dice_type = Helpers.CreateContextValue(AbilityRankType.StatBonus);
                                                    if (s.Item1 == StatType.SkillUseMagicDevice)
                                                    {
                                                        a.cost_reducing_facts = new BlueprintUnitFact[] { true_inspiration_base, device_talent };
                                                    }
                                                    else if (s.Item1 == StatType.SkillPerception)
                                                    {
                                                        a.cost_reducing_facts = new BlueprintUnitFact[] { true_inspiration_base, expanded_inspiration };
                                                    }
                                                    else if (s.Item1 == StatType.SkillThievery)
                                                    {
                                                        a.cost_reducing_facts = new BlueprintUnitFact[] { true_inspiration_base, underworld_inspiration };
                                                    }
                                                    a.allow_reroll_fact = tenacious_inspiration;
                                                })
                                                );
                inspiration_buffs.Add(s.Item1.ToString(), buff);
            }


            var persuation_inspiration_buff = Helpers.CreateBuff("InvestigatorInspirationPersuationBuff",
                                                     "Inspiration: Persuation",
                                                     description,
                                                     "",
                                                     Helpers.GetIcon("1621be43793c5bb43be55493e9c45924"), //transmutation strength
                                                     null,
                                                     dice_count_context,
                                                     dice_type_context
                                                     );
            var persuation_checks = new StatType[] { StatType.CheckDiplomacy, StatType.CheckIntimidate, StatType.CheckBluff };
            foreach (var pc in persuation_checks)
            {
                persuation_inspiration_buff.AddComponent(Helpers.Create<NewMechanics.AddRandomBonusOnSkillCheckAndConsumeResource>(a =>
                                                        {
                                                            a.stats = new StatType[] { pc };
                                                            a.resource = inspiration_resource;
                                                            a.amount = 1;
                                                            a.dices = dice_type;
                                                            a.dice_count = Helpers.CreateContextValue(AbilityRankType.Default);
                                                            a.dice_type = Helpers.CreateContextValue(AbilityRankType.StatBonus);
                                                            if (pc == StatType.CheckDiplomacy)
                                                            {
                                                                a.cost_reducing_facts = new BlueprintUnitFact[] { true_inspiration_base, expanded_inspiration };
                                                            }
                                                            else 
                                                            {
                                                                a.cost_reducing_facts = new BlueprintUnitFact[] { true_inspiration_base, underworld_inspiration };
                                                            }
                                                            a.allow_reroll_fact = tenacious_inspiration;
                                                        })
                                                      );
            }
            inspiration_buffs.Add("SkillPersuation", persuation_inspiration_buff);

            var attack_rolls_inspiration_buff = Helpers.CreateBuff("InvestigatorInspirationAttackRollsBuff",
                                                                 "Inspiration: Attack Rolls",
                                                                 description,
                                                                 "",
                                                                 Helpers.GetIcon("90e54424d682d104ab36436bd527af09"), //weapon finesse
                                                                 null,
                                                                 dice_count_context,
                                                                 dice_type_context,
                                                                 Helpers.Create<NewMechanics.AddRandomBonusOnAttackRollAndConsumeResource>(a =>
                                                                 {
                                                                     a.resource = inspiration_resource;
                                                                     a.amount = 2;
                                                                     a.dices = dice_type;
                                                                     a.dice_count = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                     a.dice_type = Helpers.CreateContextValue(AbilityRankType.StatBonus);
                                                                     a.cost_reducing_facts = new BlueprintUnitFact[] { combat_inspiration };
                                                                     a.parametrized_feature = greater_combat_inspiration;
                                                                     a.allow_reroll_fact = tenacious_inspiration;
                                                                 }));
            inspiration_buffs.Add("AttackRolls", attack_rolls_inspiration_buff);

            var saving_throws_inspiration_buff = Helpers.CreateBuff("InvestigatorInspirationSavingThrowsBuff",
                                                                     "Inspiration: Saving Throws",
                                                                     description,
                                                                     "",
                                                                     Helpers.GetIcon("175d1577bb6c9a04baf88eec99c66334"), //iron will
                                                                     null,
                                                                     dice_count_context,
                                                                     dice_type_context,
                                                                     Helpers.Create<NewMechanics.AddRandomBonusOnSavingThrowAndConsumeResource>(a =>
                                                                     {
                                                                         a.resource = inspiration_resource;
                                                                         a.amount = 2;
                                                                         a.dices = dice_type;
                                                                         a.dice_count = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                         a.dice_type = Helpers.CreateContextValue(AbilityRankType.StatBonus);
                                                                         a.cost_reducing_facts = new BlueprintUnitFact[] { combat_inspiration };
                                                                         a.allow_reroll_fact = tenacious_inspiration;
                                                                     }));
            inspiration_buffs.Add("SavingThrows", saving_throws_inspiration_buff);

            foreach (var b in inspiration_buffs)
            {
                var toggle = Helpers.CreateActivatableAbility(b.Value.name + "ToggleAbility",
                                                              b.Value.Name,
                                                              b.Value.Description,
                                                              "",
                                                              b.Value.Icon,
                                                              b.Value,
                                                              AbilityActivationType.Immediately,
                                                              CommandType.Free,
                                                              null,
                                                              Helpers.CreateActivatableResourceLogic(inspiration_resource, ResourceSpendType.Never)
                                                              );
                toggle.DeactivateImmediately = true;
                
                var feature = Common.buffToFeature(b.Value);
                foreach (var c in feature.GetComponents<NewMechanics.AddRandomBonusOnSkillCheckAndConsumeResource>().ToArray())
                {
                    var new_c = c.CreateCopy();
                    new_c.resource = null;
                    new_c.amount = 0;
                    feature.ReplaceComponent(c, new_c);
                }
                inspiration_features.Add(b.Key, feature);
                var ability_feature = Common.ActivatableAbilityToFeature(toggle);
                inspiration_ability_features.Add(b.Key, toggle);
                inspiration.AddComponent(Helpers.CreateAddFactNoRestore(toggle));
            }

            regularizeInspirationTalents();
        }


        static void regularizeInspirationTalents()
        {
            var remove_persuation_inspiration_ability = Helpers.CreateFeature("RemovePersuationInspirationAbilityFeature",
                                      "",
                                      "",
                                      "",
                                      null,
                                      FeatureGroup.None,
                                      Common.createRemoveFeatureOnApply(inspiration_ability_features["SkillPersuation"]),
                                      Helpers.CreateAddFact(inspiration_features["SkillPersuation"])
                                      );

            remove_persuation_inspiration_ability.HideInCharacterSheetAndLevelUp = true;
            Main.logger.Log("Perception");
            expanded_inspiration.AddComponents(Common.createRemoveFeatureOnApply(inspiration_ability_features["SkillPerception"]),
                                               Helpers.CreateAddFact(inspiration_features["SkillPerception"]),
                                               Common.createAddFeatureIfHasFact(underworld_inspiration, remove_persuation_inspiration_ability));
            device_talent.AddComponents(Common.createRemoveFeatureOnApply(inspiration_ability_features["SkillUseMagicDevice"]),
                                        Helpers.CreateAddFact(inspiration_features["SkillUseMagicDevice"]));
            underworld_inspiration.AddComponents(Common.createRemoveFeatureOnApply(inspiration_ability_features["SkillThievery"]),
                                                 Helpers.CreateAddFact(inspiration_features["SkillThievery"]),
                                                 Common.createAddFeatureIfHasFact(expanded_inspiration, remove_persuation_inspiration_ability));

            var replace_features = new string[]
            {
                "AbilityChecks",
                "SkillAthletics",
                "SkillMobility",
                "SkillThievery",
                "SkillPerception",
                "SkillUseMagicDevice",
                "SkillPersuation"
            };

            foreach (var rp in replace_features)
            {
                true_inspiration.AddComponents(Common.createRemoveFeatureOnApply(inspiration_ability_features[rp]),
                                               Helpers.CreateAddFact(inspiration_features[rp]));
            }
        }

        static void createExpandedInspirtation()
        {
            var icon = Helpers.GetIcon("f74c6bdf5c5f5374fb9302ecdc1f7d64"); // skill focus perception
            expanded_inspiration = Helpers.CreateFeature("ExapndedInspirtaionInvestigatorFeature",
                                              "Expanded Inspiration",
                                              "An investigator can use his inspiration ability when attempting Diplomacy or Perception checks without expending uses of inspiration.",
                                              "",
                                              icon,
                                              FeatureGroup.None
                                              );                                                         
        }


        static void createDeviceTalent()
        {
            var icon = Helpers.GetIcon("f43ffc8e3f8ad8a43be2d44ad6e27914"); // skill focus umd
            device_talent = Helpers.CreateFeature("DeviceTalentInvestigatorFeature",
                                              "Device Talent",
                                              "If the investigator is trained in Use Magic Device, he can use the inspiration ability with that skill without expending uses of inspiration.",
                                              "",
                                              icon,
                                              FeatureGroup.None
                                              );
        }


        static void createUnderworldInspiration()
        {
            var icon = Helpers.GetIcon("7feda1b98f0c169418aa9af78a85953b"); // skill focus trickery
            underworld_inspiration = Helpers.CreateFeature("UnderworldInspirtaionInvestigatorFeature",
                                              "Underworld Inspiration",
                                              "An investigator can use his inspiration on Bluff, Trickery, or Intimidate checks without expending uses of inspiration",
                                              "",
                                              icon,
                                              FeatureGroup.None
                                              );
        }


        static void createCombatInspiration()
        {
            var icon = Helpers.GetIcon("90e54424d682d104ab36436bd527af09"); // weapon finesse
            combat_inspiration = Helpers.CreateFeature("CombatInspirtaionInvestigatorFeature",
                                              "Combat Inspiration",
                                              "When an investigator uses inspiration on an attack roll or saving throw, he expends one use of inspiration instead of two.",
                                              "",
                                              icon,
                                              FeatureGroup.None,
                                              Helpers.PrerequisiteClassLevel(investigator_class, 9)
                                              );
        }


        static void createGreaterCombatInspiration()
        {
            var icon = Helpers.GetIcon("90e54424d682d104ab36436bd527af09"); // weapon finesse
            greater_combat_inspiration = library.CopyAndAdd<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e", "GreaterCombatInspirationParametrizedFeature", "");
            greater_combat_inspiration.SetNameDescription("Greater Combat Inspiration",
                                                          "Choose a single weapon type (such as sword cane or short sword). As long as the investigator has at least 1 inspiration point in his inspiration pool, he no longer has to expend a use of inspiration to use that ability with attacks made with this weapon.");
            greater_combat_inspiration.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.PrerequisiteClassLevel(investigator_class, 19),
                Helpers.PrerequisiteFeature(combat_inspiration)
            };
            greater_combat_inspiration.Groups = new FeatureGroup[] { FeatureGroup.None };
        }


        static void createAmazingInspiration()
        {
            var icon = Helpers.GetIcon("f2115ac1148256b4ba20788f7e966830"); // restoration
            amazing_inspiration = Helpers.CreateFeature("AmazingInspirtaionInvestigatorFeature",
                                              "Amazing Inspiration",
                                              "When using inspiration, the investigator rolls a d8 instead of a d6. At 20th level, the investigator rolls 2d8 and adds both dice to the result.",
                                              "",
                                              icon,
                                              FeatureGroup.None,
                                              Helpers.PrerequisiteClassLevel(investigator_class, 7)
                                              );
        }

        static void createTenaciousInspiration()
        {            
            tenacious_inspiration = Helpers.CreateFeature("TenaciousInspirtaionInvestigatorFeature",
                                                          "Tenacious Inspiration",
                                                          "When an investigator rolls his inspiration die, he can roll an additional inspiration die and take the higher result.",
                                                          "",
                                                          null,
                                                          FeatureGroup.None,
                                                          Helpers.PrerequisiteClassLevel(investigator_class, 13)
                                                          );
        }


        static void createTrueInspiration()
        {
            var icon = Helpers.GetIcon("fafd77c6bfa85c04ba31fdc1c962c914"); //greater restoration
            true_inspiration = Helpers.CreateFeature("TrueInspirtaionInvestigatorFeature",
                                                          "True Inspiration",
                                                          "At 20th level, an investigator can use inspiration on all skill checks and all ability checks without spending inspiration.\n"
                                                          + "In addition, whenever he expends inspiration on an ability check, attack roll, saving throw, or skill check, he adds 2d6 rather than 1d6 to the result. Some talents can affect this. If using the amazing inspiration investigator talent, he rolls 2d8 instead. If using this with tenacious inspiration, underworld inspiration, or a similar talent, he rolls two sets of inspiration dice and uses the higher of the two results.",
                                                          "",
                                                          icon,
                                                          FeatureGroup.None,
                                                          Helpers.CreateAddFact(true_inspiration_base)
                                                          );
    }


        static void createInvestigatorProficiencies()
        {
            investigator_proficiencies = library.CopyAndAdd<BlueprintFeature>("33e2a7e4ad9daa54eaf808e1483bb43c", "InvestigatorProficiencies", "");
            investigator_proficiencies.SetNameDescription("Investigator Proficiencies",
                                                          "Investigators are proficient with all simple weapons, plus the hand crossbow, rapier, sap, short sword, and shortbow. They are proficient with light armor, but not with shields."
                                                          );
        }


        static void createTrapfinding()
        {
            trapfinding = library.CopyAndAdd<BlueprintFeature>("dbb6b3bffe6db3547b31c3711653838e", "InvestigatorTrapfinding", "");
            trapfinding.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", getInvestigatorArray()));
            trapfinding.SetDescription("An investigator adds 1/2 her level on Perception checks.");
        }


        static void createTrapSense()
        {
            trap_sense = library.CopyAndAdd<BlueprintFeature>("0bcbe9e450b0e7b428f08f66c53c5136", "IvestigatorTrapSense", "");
            trap_sense.SetDescription("At 3rd level, an investigator gains an intuitive sense that alerts him to danger from traps, granting him a +1 bonus on Reflex saving throws to avoid traps and a +1 dodge bonus to AC against attacks by traps. At 6th level and every 3 levels thereafter, these bonuses increase by 1 (to a maximum of +6 at 18th level).");

        }


        static void createPoisonResistanceAndImmunity()
        {
            poison_immunity = library.CopyAndAdd<BlueprintFeature>("202af59b918143a4ab7c33d72c8eb6d5", "PoisonImmunityInvestigator", "");
            poison_immunity.SetDescription("At 11th level, the investigator becomes completely immune to poison.");

            poison_resistance = Helpers.CreateFeature("PoisonResistanceInvestigator",
                                                      "Poison Resistance",
                                                      "At 2nd level, an investigator gains a +2 bonus on all saving throws against poison. This bonus increases to +4 at 5th level, and to +6 at 8th level.",
                                                      "",
                                                      null,
                                                      FeatureGroup.None,
                                                      Common.createContextSavingThrowBonusAgainstDescriptor(Helpers.CreateContextValue(AbilityRankType.Default), ModifierDescriptor.Other, SpellDescriptor.Poison),
                                                      Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getInvestigatorArray(),
                                                                                      progression: ContextRankProgression.StartPlusDoubleDivStep,
                                                                                      startLevel: 2, stepLevel: 3)
                                                     );
        }


        static void createStudiedCombat()
        {
            ranged_study = Helpers.CreateFeature("RangedStudyFeature",
                                               "Ranged Study",
                                               "You gain the bonuses for studied combat with ranged weapons and can use studied strike with ranged_weapons as long as the target of your studied strike is within 30 feet of you.\n"
                                               + "Normal: You gain the bonuses for studied combat and can use studied strike only with melee weapons.",
                                               "",
                                               null,
                                               FeatureGroup.Feat);
            library.AddFeats(ranged_study);
            createStudiedDefense();
            var icon = Helpers.GetIcon("b96d810ceb1708b4e895b695ddbb1813");

            var studied_target_cooldown = Helpers.CreateBuff("InvestigatorStudiedTargetCooldownBuff",
                                                             "Investigator Studied Target Cooldown",
                                                              "With a keen eye and calculating mind, an investigator can assess the mettle of his opponent to take advantage of gaps in talent and training. At 4th level, an investigator can use a move action to study a single enemy that he can see. Upon doing so, he adds 1/2 his investigator level as an insight bonus on melee attack rolls and as a bonus on damage rolls against the creature. This effect lasts for a number of rounds equal to his Intelligence modifier (minimum 1) or until he deals damage with a studied strike, whichever comes first. The bonus on damage rolls is precision damage, and is not multiplied on a critical hit.\n"
                                                              + "An investigator can only have one target of studied combat at a time, and once a creature has become the target of an investigator’s studied combat, he cannot become the target of the same investigator’s studied combat again for 24 hours unless the investigator expends one use of inspiration when taking the move action to use this ability.",
                                                              "",
                                                              icon,
                                                              null);
            studied_target_cooldown.SetBuffFlags(BuffFlags.RemoveOnRest);
            var apply_cooldown = Common.createContextActionApplyBuff(studied_target_cooldown, Helpers.CreateContextDuration(1, DurationRate.Days), dispellable: false);
            var studied_attack_buff = Helpers.CreateBuff("InvestigatorStudiedTargetAttackBuff",
                                                             "Investigator Studied Target",
                                                             studied_target_cooldown.Description,
                                                             "",
                                                             icon,
                                                             null
                                                             );
            var studied_defense_buff = Helpers.CreateBuff("InvestigatorStudiedDefenseBuff",
                                                         "Investigator Studied Defense Target",
                                                         studied_defense.Description,
                                                         "",
                                                         icon,
                                                         null,
                                                         Helpers.Create<NewMechanics.ACBonusAgainstTargetIfHasFact>(a =>
                                                                                                                     {
                                                                                                                         a.CheckCaster = true;
                                                                                                                         a.Descriptor = ModifierDescriptor.Insight;
                                                                                                                         a.Value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                                                                     }
                                                                                                                     ),
                                                         Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getInvestigatorArray(),
                                                                                     progression: ContextRankProgression.Div2)
                                                         );
            var apply_attack = Common.createContextActionApplyBuff(studied_attack_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)),
                                                                                                                      is_child: true, dispellable: false);
            var apply_defense = Common.createContextActionApplyBuff(studied_defense_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)),
                                                                                                                        is_child: true, dispellable: false);
            studied_target_buff = Helpers.CreateBuff("InvestigatorStudiedTargetBuff",
                                                         "",
                                                         "",
                                                         "",
                                                         icon,
                                                         null,
                                                         Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus,
                                                                                         stat: StatType.Intelligence),
                                                         Helpers.CreateAddFactContextActions(activated: Helpers.CreateConditional(Common.createContextConditionCasterHasFact(studied_defense_toggle_buff),
                                                                                                                                  apply_defense,
                                                                                                                                  apply_attack)
                                                                                            ),
                                                         Helpers.Create<UniqueBuff>()
                                                         );
            studied_target_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var apply_studied = Common.createContextActionApplyBuff(studied_target_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);
            studied_combat_ability = Helpers.CreateAbility("StudiedCombatInvestigatorAbility",
                                                           "Studied Combat",
                                                           studied_attack_buff.Description,
                                                           "",
                                                           icon,
                                                           AbilityType.Extraordinary,
                                                           CommandType.Move,
                                                           AbilityRange.Close,
                                                           "1 round/Intelligence modifier",
                                                           "",
                                                           Helpers.CreateRunActions(apply_studied, apply_cooldown),
                                                           Common.createAbilityTargetHasFact(true, studied_target_cooldown),
                                                           Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus,
                                                                                         stat: StatType.Intelligence)
                                                           );
            studied_combat_ability.AvailableMetamagic = Metamagic.Extend;
            studied_combat_ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);

            studied_combat_ability_ignore_cooldown = library.CopyAndAdd<BlueprintAbility>(studied_combat_ability.AssetGuid, "StudiedCombatIgnoreCooldownInvestigatorAbility", "");
            studied_combat_ability_ignore_cooldown.AddComponent(inspiration_resource.CreateResourceLogic());
            studied_combat_ability_ignore_cooldown.SetName("Studied Combat (Ignore Cooldown)");
            studied_combat_ability_ignore_cooldown.ReplaceComponent<AbilityTargetHasFact>(a => a.Inverted = false);

            studied_combat_base_ability = Common.createVariantWrapper("InvestigatorStudiedCombatBaseAbility", "", studied_combat_ability, studied_combat_ability_ignore_cooldown);
            studied_combat = Common.AbilityToFeature(studied_combat_base_ability, false);
            studied_combat.AddComponents(Helpers.Create<NewMechanics.AttackBonusAgainstFactsOwner>(a =>
                                                                                                    {
                                                                                                        a.Bonus = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                                                        a.attack_types = new AttackType[] { AttackType.Melee, AttackType.Touch };
                                                                                                        a.Descriptor = ModifierDescriptor.Insight;
                                                                                                        a.only_from_caster = true;
                                                                                                        a.CheckedFacts = new BlueprintUnitFact[] { studied_attack_buff };
                                                                                                    }),
                                        Helpers.Create<NewMechanics.DamageBonusPrecisionAgainstFactOwner>(a =>
                                                                                                    {
                                                                                                        a.bonus = Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilityRankType.Default));
                                                                                                        a.attack_types = new AttackType[] { AttackType.Melee, AttackType.Touch };
                                                                                                        a.only_from_caster = true;
                                                                                                        a.checked_fact = studied_target_buff;
                                                                                                    }),
                                        Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getInvestigatorArray(),
                                                                                     progression: ContextRankProgression.Div2)
                                        );

            var weapon_focus = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");
            ranged_study.AddComponents(Helpers.PrerequisiteFeature(studied_combat),
                                       Common.createPrerequisiteParametrizedFeatureWeapon(weapon_focus, WeaponCategory.Longbow, any: true),
                                       Common.createPrerequisiteParametrizedFeatureWeapon(weapon_focus, WeaponCategory.Shortbow, any: true),
                                       Common.createPrerequisiteParametrizedFeatureWeapon(weapon_focus, WeaponCategory.LightCrossbow, any: true),
                                       Common.createPrerequisiteParametrizedFeatureWeapon(weapon_focus, WeaponCategory.HeavyCrossbow, any: true),
                                       Common.createPrerequisiteParametrizedFeatureWeapon(weapon_focus, WeaponCategory.ThrowingAxe, any: true),
                                       Common.createPrerequisiteParametrizedFeatureWeapon(weapon_focus, WeaponCategory.Dart, any: true)
                                       );
            ranged_study.AddComponents(Helpers.Create<NewMechanics.AttackBonusAgainstFactsOwner>(a =>
                                                                                                {
                                                                                                    a.Bonus = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                                                    a.attack_types = new AttackType[] { AttackType.Ranged, AttackType.RangedTouch };
                                                                                                    a.Descriptor = ModifierDescriptor.Insight;
                                                                                                    a.only_from_caster = true;
                                                                                                    a.CheckedFacts = new BlueprintUnitFact[] { studied_attack_buff };
                                                                                                }),
                                                                                                Helpers.Create<NewMechanics.DamageBonusPrecisionAgainstFactOwner>(a =>
                                                                                                {
                                                                                                    a.bonus = Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilityRankType.Default));
                                                                                                    a.attack_types = new AttackType[] { AttackType.Ranged, AttackType.RangedTouch };
                                                                                                    a.only_from_caster = true;
                                                                                                    a.checked_fact = studied_target_buff;
                                                                                                }),
                                                                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getInvestigatorArray(),
                                                                                                                                             progression: ContextRankProgression.Div2)
                                                                                                );
        }


        static void createStudiedDefense()
        {
            studied_defense_toggle_buff = Helpers.CreateBuff("StudiedDefenseToggleBuff",
                                                             "Studied Defense",
                                                             "When an investigator with this talent uses his studied combat ability, he can chose to apply that ability’s insight bonus to his AC against attacks made by the target of his studied combat instead of to attack rolls against the target of his studied combat. (The insight bonus on damage rolls remains.) He must choose which type of bonus he gains when using studied combat, and it cannot be changed until he uses studied combat again.",
                                                             "",
                                                             Helpers.GetIcon("62888999171921e4dafb46de83f4d67d"), //shield of dawn
                                                             null);
            var studied_defense_toggle = Helpers.CreateActivatableAbility("StudiedDefenseToggleAbility",
                                                                          studied_defense_toggle_buff.Name,
                                                                          studied_defense_toggle_buff.Description,
                                                                          "",
                                                                          studied_defense_toggle_buff.Icon,
                                                                          studied_defense_toggle_buff,
                                                                          AbilityActivationType.Immediately,
                                                                          CommandType.Free,
                                                                          null);
            studied_defense_toggle.DeactivateImmediately = true;

            studied_defense = Common.ActivatableAbilityToFeature(studied_defense_toggle, false);
            studied_defense.AddComponent(Helpers.PrerequisiteCharacterLevel(9, investigator_class));
        }


        static void createStudiedStrike()
        {
            var icon = Helpers.GetIcon("9b9eac6709e1c084cb18c3a366e0ec87");
            studied_strike_buff = Helpers.CreateBuff("InvestigatorStudiedStrikeBuff",
                                          "Studied Strike",
                                          "At 4th level, an investigator can choose to make a studied strike against the target of his studied combat as a free action, upon successfully hitting his studied target with a melee attack, to deal additional damage. The damage is 1d6 at 4th level, and increases by 1d6 for every 2 levels thereafter (to a maximum of 9d6 at 20th level). The damage of studied strike is precision damage and is not multiplied on a critical hit; creatures that are immune to sneak attacks are also immune to studied strike.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.Create<NewMechanics.DamageBonusPrecisionAgainstFactOwner>(a =>
                                                                                  {
                                                                                      a.bonus = Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.Default), 0);
                                                                                      a.attack_types = new AttackType[] { AttackType.Melee, AttackType.Touch };
                                                                                      a.only_from_caster = true;
                                                                                      a.checked_fact = studied_target_buff;
                                                                                  }),
                                          Helpers.Create<NewMechanics.DamageBonusPrecisionAgainstFactOwner>(a =>
                                                                                    {
                                                                                        a.bonus = Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.Default), 0);
                                                                                        a.attack_types = new AttackType[] { AttackType.Ranged, AttackType.RangedTouch };
                                                                                        a.only_from_caster = true;
                                                                                        a.checked_fact = studied_target_buff;
                                                                                        a.attacker_fact = ranged_study;
                                                                                    }),
                                          Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getInvestigatorArray(),
                                                                                     progression: ContextRankProgression.StartPlusDivStep, 
                                                                                     startLevel: 4, stepLevel: 2),
                                          Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(Common.createContextActionRemoveBuffFromCaster(studied_target_buff)),
                                                                                           check_weapon_range_type: true, wait_for_attack_to_resolve: true),
                                          Common.createContextCalculateAbilityParamsBasedOnClass(investigator_class, StatType.Intelligence)
                                          );
            var toggle = Helpers.CreateActivatableAbility("InvestigatorStudiedStrikeToggleAbility",
                                                          studied_strike_buff.Name,
                                                          studied_strike_buff.Description,
                                                          "",
                                                          studied_strike_buff.Icon,
                                                          studied_strike_buff,
                                                          AbilityActivationType.Immediately,
                                                          CommandType.Free,
                                                          null);
            toggle.DeactivateImmediately = true;

            studied_strike = Common.ActivatableAbilityToFeature(toggle, false);

        }
    }
}
