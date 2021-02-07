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
        static public BlueprintFeature extend_potion;
        static public BlueprintFeature enhance_potion;

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


        static public BlueprintArchetype psychic_detective;
        static public BlueprintFeature psychic_spellcasting;
        static public BlueprintSpellbook psychic_detective_spellbook;
        static public BlueprintFeature psychic_meddler;
        static public BlueprintFeatureSelection phrenic_dabbler;
        static public BlueprintFeature extra_phrenic_pool;
        static public BlueprintFeatureSelection extra_phrenic_amplification;
        static public BlueprintAbilityResource phrenic_pool_resource;

        static public BlueprintFeature focused_force;
        static public BlueprintFeature ongoing_defense;
        static public BlueprintFeature biokinetic_healing;
        static public BlueprintFeature conjured_armor;
        static public BlueprintFeature defensive_prognostication;
        static public BlueprintFeature minds_eye;
        static public BlueprintFeature overpowering_mind;
        static public BlueprintFeature will_of_the_dead;
        static public BlueprintFeature relentness_casting;
        static public BlueprintFeature undercast_surge;
        static public BlueprintFeature psychofeedback;

        static public BlueprintFeature center_self;

        static public BlueprintArchetype cryptid_schoolar;
        static public BlueprintFeature intuitive_monster_lore;
        static public BlueprintFeature opportune_advice;
        static public BlueprintFeature knowledgeable_strike;


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
            investigator_class.ClassSkills = new StatType[] {StatType.SkillAthletics,  StatType.SkillMobility, StatType.SkillStealth, StatType.SkillThievery,
                                                      StatType.SkillKnowledgeArcana, StatType.SkillKnowledgeWorld, StatType.SkillLoreNature, StatType.SkillLoreReligion,
                                                      StatType.SkillPerception, StatType.SkillPersuasion, StatType.SkillUseMagicDevice}; 
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
            createJinyiwei();
            createPsychicDetective();
            createCryptidSchoolar();

            investigator_class.Archetypes = new BlueprintArchetype[] {empiricist_archetype, questioner_archetype, jinyiwei_archetype, psychic_detective, cryptid_schoolar};
            Helpers.RegisterClass(investigator_class);
            addToPrestigeClasses(); 
            createFeats();
        }

        static void addToPrestigeClasses()
        {
            Common.addReplaceSpellbook(Common.EldritchKnightSpellbookSelection, questioner_spellbook, "EldritchKnightQuestioner",
                                      Common.createPrerequisiteArchetypeLevel(investigator_class, questioner_archetype, 1),
                                      Common.createPrerequisiteClassSpellLevel(investigator_class, 3));
            Common.addReplaceSpellbook(Common.ArcaneTricksterSelection, questioner_spellbook, "ArcaneTricksterQuestioner",
                                        Common.createPrerequisiteArchetypeLevel(investigator_class, questioner_archetype, 1),
                                        Common.createPrerequisiteClassSpellLevel(investigator_class, 2));
            Common.addReplaceSpellbook(Common.MysticTheurgeArcaneSpellbookSelection, questioner_spellbook, "MysticTheurgeQuestioner",
                                        Common.createPrerequisiteArchetypeLevel(investigator_class, questioner_archetype, 1),
                                        Common.createPrerequisiteClassSpellLevel(investigator_class, 2));
            /*Common.addReplaceSpellbook(Common.MysticTheurgeDivineSpellbookSelection, jinyiwei_spellbook, "MysticTheurgeJinyiwei",
                                        Common.createPrerequisiteArchetypeLevel(investigator_class, jinyiwei_archetype, 2));*/
            Common.addMTDivineSpellbookProgression(investigator_class, jinyiwei_spellbook, "MysticTheurgeJinyiweiProgression", 
                                                    Common.createPrerequisiteArchetypeLevel(jinyiwei_archetype, 1),
                                                    Common.createPrerequisiteClassSpellLevel(investigator_class, 2)
                                                    );
            Common.addReplaceSpellbook(Common.DragonDiscipleSpellbookSelection, questioner_spellbook, "DragonDiscipleQuestioner",
                                       Common.createPrerequisiteArchetypeLevel(investigator_class, questioner_archetype, 1));
        }


        static void createCryptidSchoolar()
        {
            cryptid_schoolar = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "CryptidSchoolarArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Cryptid Scholar");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Cryptid scholars research monsters that lurk secretly at the edge of civilization, developing a deep expertise regarding their anatomy, habits, and ecology. Although competent as lone monster hunters, cryptid scholars excel when they have associates with whom to share their insights.");
            });
            Helpers.SetField(cryptid_schoolar, "m_ParentClass", investigator_class);
            library.AddAsset(cryptid_schoolar, "");

            createIntuitiveMonsterLore();
            createOpportuneAdviceAndKnowledgeableStrike();

            cryptid_schoolar.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(2, poison_resistance),
                                                                   Helpers.LevelEntry(4, studied_combat, studied_strike),
                                                                   Helpers.LevelEntry(11, poison_immunity),
                                                                };
            cryptid_schoolar.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, intuitive_monster_lore),
                                                               Helpers.LevelEntry(4, opportune_advice, knowledgeable_strike)
                                                             };
           

            quick_study.AddComponent(Common.prerequisiteNoArchetype(cryptid_schoolar));
            prolonged_study.AddComponent(Common.prerequisiteNoArchetype(cryptid_schoolar));
            sapping_offensive.AddComponent(Common.prerequisiteNoArchetype(cryptid_schoolar));
            sickening_offensive.AddComponent(Common.prerequisiteNoArchetype(cryptid_schoolar));
            blinding_strike.AddComponent(Common.prerequisiteNoArchetype(cryptid_schoolar));
            confusing_strike.AddComponent(Common.prerequisiteNoArchetype(cryptid_schoolar));
            studied_defense.AddComponent(Common.prerequisiteNoArchetype(cryptid_schoolar));
            toppling_strike.AddComponent(Common.prerequisiteNoArchetype(cryptid_schoolar));
            ranged_study.AddComponent(Common.prerequisiteNoArchetype(cryptid_schoolar));

            investigator_class.Progression.UIGroups[3].Features.Add(knowledgeable_strike);
        }


        static void createIntuitiveMonsterLore()
        {
            intuitive_monster_lore = Helpers.CreateFeature("IntuitiveLoreCryptidSchoolarFeature",
                                                           "Intuitive Lore",
                                                           "A cryptid scholar adds both his Intelligence and Wisdom modifiers on Knowledge and Lore skill checks.",
                                                           "",
                                                           null,
                                                           FeatureGroup.None,
                                                           Helpers.CreateAddContextStatBonus(StatType.SkillLoreNature, ModifierDescriptor.UntypedStackable),
                                                           Helpers.CreateAddContextStatBonus(StatType.SkillLoreReligion, ModifierDescriptor.UntypedStackable),
                                                           Helpers.CreateAddContextStatBonus(StatType.SkillKnowledgeArcana, ModifierDescriptor.UntypedStackable, rankType: AbilityRankType.StatBonus),
                                                           Helpers.CreateAddContextStatBonus(StatType.SkillKnowledgeWorld, ModifierDescriptor.UntypedStackable, rankType: AbilityRankType.StatBonus),
                                                           Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, stat: StatType.Intelligence, min: 0),
                                                           Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, stat: StatType.Wisdom, min: 0, type: AbilityRankType.StatBonus),
                                                           Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Intelligence),
                                                           Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Wisdom)
                                                           );
        }


        static void createOpportuneAdviceAndKnowledgeableStrike()
        {
            var favored_enemies = new (BlueprintFeature, string)[]
            {
                (library.Get<BlueprintFeature>("7081934ab5f8573429dbd26522adcc39"), "Aberration"),
                (library.Get<BlueprintFeature>("1ef8d7ab3ca4795498ff446cb027e2f3"), "Animal"),
                (library.Get<BlueprintFeature>("6ea5a4a19ccb81a498e18a229cc5038a"), "Construct"),
                (library.Get<BlueprintFeature>("918555c021b3a2944beed35df53b4c56"), "Dragon"),
                (library.Get<BlueprintFeature>("be3d454ea70a8bb468b0a8a087e7e65b"), "Fey"),
                (library.Get<BlueprintFeature>("bd59614d30bcadd46bd56aabe0de819f"), "Giant Humanoid"),
                (library.Get<BlueprintFeature>("c03dc59e2ce30484b82e7aead6d10471"), "Goblin"),
                (library.Get<BlueprintFeature>("f807fac786faa86438428c79f5629654"), "Magical Beast"),
                (library.Get<BlueprintFeature>("0fd21e10dff071e4580ef9f30a0334df"), "Monstrous Humanoid"),
                (library.Get<BlueprintFeature>("f643b38acc23e8e42a3ed577daeb6949"), "Outsider"),
                (library.Get<BlueprintFeature>("4ae78c44858bc1942934efe7c149d039"), "Plant"),
                (library.Get<BlueprintFeature>("3b0f03e24ad0b7243b5973c3d8ab7af5"), "Reptilian"),
                (library.Get<BlueprintFeature>("5941963eae3e9864d91044ba771f2cc2"), "Undead"),
                (library.Get<BlueprintFeature>("f6dac9009747b91408644fa834dd0d99"), "Vermin"),

                /*(library.Get<BlueprintFeature>("7ed4cbc226539d6419d6c60320bfd244"), "Elf"),
                (library.Get<BlueprintFeature>("8bf97ce6a52d7274197a6c1911378b27"), "Dwarf"),
                (library.Get<BlueprintFeature>("04314be4be2b7f444883ca8caf78a8a8"), "Gnome"),
                (library.Get<BlueprintFeature>("8a18b8eec185cd24eb55c3457a7f69b1"), "Halfling"),
                (library.Get<BlueprintFeature>("7283344b0309d8e4cb77eb22f1e7c57a"), "Human"),*/
            };

            var icon = Helpers.GetIcon("183d5bb91dea3a1489a6db6c9cb64445"); //shield of faith
            var icon_opportune_strike = Helpers.GetIcon("2c38da66e5a599347ac95b3294acbe00"); //true strike
            var buffs = new List<BlueprintBuff>();
            var opportune_strike_buffs = new List<BlueprintBuff>();
            var cooldowns = new List<BlueprintBuff>();
            var abilities = new List<BlueprintAbility>();

            foreach (var fe in favored_enemies)
            {
                var remove_self = Helpers.CreateActionList(Helpers.Create<ContextActionRemoveSelf>());
                var opportune_advice_cooldown = Helpers.CreateBuff(fe.Item2.Replace(" ", "") + "InvestigatorOpportuneAdviceCooldownBuff",
                                                                  "Opportune Advice Cooldown: " + fe.Item2,
                                                                  "At 4th level, when the cryptid scholar succeeds at a Knowledge check to identify a monster’s special powers or vulnerabilities, he can take a move action to share his insights with his allies. Allies within 30 feet who can hear the cryptid scholar gain a +1 insight bonus to their ACs and on saving throws against extraordinary, supernatural, and spell-like abilities used by creatures of the same type and all the same subtypes as the monster identified. This bonus lasts for a number of rounds equal to the cryptid scholar’s Intelligence modifier (minimum 1) or until he uses knowledgeable strike (see below), whichever comes first. This bonus increases by 1 at 8th level and every 4 investigator levels thereafter (to a maximum of +5 at 20th level). A creature cannot benefit from opportune advice regarding more than one specific kind of monster at a time.\n"
                                                                  + "Once the cryptid scholar has used this ability to provide a bonus against a specific kind of monster in the current encounter, he can’t grant a bonus against that same kind of monster again for 24 hours, unless he expends one use of inspiration when taking a move action to use this ability.",
                                                                  "",
                                                                  fe.Item1.Icon,
                                                                  null,
                                                                  Helpers.Create<CombatStateTrigger>(c => { c.CombatEndActions = remove_self; c.CombatStartActions = remove_self; }));
                opportune_advice_cooldown.SetBuffFlags(BuffFlags.RemoveOnRest);

                var opportune_advice_buff = Helpers.CreateBuff(fe.Item2.Replace(" ", "") + "InvestigatorOpportuneAdviceBuff",
                                                             "Opportune Advice: " + fe.Item2,
                                                             opportune_advice_cooldown.Description,
                                                             "",
                                                             fe.Item1.Icon,
                                                             null,
                                                             Helpers.Create<NewMechanics.ContextACBonusAgainstFactOwner>(a =>
                                                                                                                         {
                                                                                                                             a.CheckedFact = fe.Item1.GetComponent<AddFavoredEnemy>().CheckedFacts[0];
                                                                                                                             a.Descriptor = ModifierDescriptor.Insight;
                                                                                                                             a.Bonus = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                                                                         }
                                                                                                                         ),
                                                             Helpers.Create<NewMechanics.ContextSavingThrowBonusAgainstFact>(a =>
                                                                                                                            {
                                                                                                                                a.CheckedFact = fe.Item1.GetComponent<AddFavoredEnemy>().CheckedFacts[0];
                                                                                                                                a.Descriptor = ModifierDescriptor.Insight;
                                                                                                                                a.Bonus = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                                                                            }
                                                                                                                         ),
                                                             Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getInvestigatorArray(),
                                                                                         progression: ContextRankProgression.DivStep, stepLevel: 4)
                                                             );
                buffs.Add(opportune_advice_buff);
                cooldowns.Add(opportune_advice_cooldown);



                var opportune_strike_buff = Helpers.CreateBuff(fe.Item2.Replace(" ", "") + "InvestigatorOpportuneStrikeBuff",
                                                              "Opportune Strike: " + fe.Item2,
                                                              "At 4th level, the cryptid scholar can direct allies to exploit a monster’s weaknesses. If the cryptid scholar ends his opportune advice early as a move action, each ally within 30 feet who can hear the cryptid scholar deals additional damage on its next successful unarmed, natural, or weapon attack against that specific kind of monster within 1 round. The additional damage is 1d6 at 4th level and increases by 1d6 for every 4 investigator levels thereafter (to a maximum of 5d6 at 20th level). The damage of knowledgeable strike is precision damage and is not multiplied on a critical hit; creatures that are immune to sneak attacks are also immune to knowledgeable strike. Ranged attacks gain this additional damage only against a target within 30 feet. The ally must be able to see the target well enough to pick out a vital spot and must be able to reach such a spot. Knowledgeable strike cannot be used against a creature with concealment.",
                                                              "",
                                                              icon_opportune_strike,
                                                              null,
                                                              Helpers.Create<NewMechanics.DamageBonusPrecisionAgainstFactOwner>(a =>
                                                              {
                                                                  a.bonus = Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.Default), 0);
                                                                  a.attack_types = new AttackType[] { AttackType.Melee, AttackType.Touch, AttackType.Ranged, AttackType.RangedTouch };
                                                                  a.only_from_caster = false;
                                                                  a.checked_fact  = fe.Item1.GetComponent<AddFavoredEnemy>().CheckedFacts[0];
                                                                  a.remove_after_damage = true;
                                                              }),
                                                              Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getInvestigatorArray(),
                                                                                                         progression: ContextRankProgression.DivStep,
                                                                                                         stepLevel: 4)
                                                              );
                var remove_ooportune_strike = Common.createContextActionRemoveBuff(opportune_strike_buff);



                opportune_strike_buffs.Add(opportune_strike_buff);
            }


            for (int i = 0; i < buffs.Count; i++)
            {
                var apply_cooldown = Common.createContextActionApplyBuffToCaster(cooldowns[i], Helpers.CreateContextDuration(1, DurationRate.Days), dispellable: false);
                var apply_buff = Common.createContextActionApplyBuff(buffs[i], Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus)),
                                                                                                      dispellable: false);

                var ability = Helpers.CreateAbility(favored_enemies[i].Item2.Replace(" ", "") + "OpportuneAdviceInvestigatorAbility",
                                                   buffs[i].Name,
                                                   buffs[i].Description,
                                                   "",
                                                   favored_enemies[i].Item1.Icon,
                                                   AbilityType.Extraordinary,
                                                   CommandType.Move,
                                                   AbilityRange.Personal,
                                                   "1 round/Intelligence modifier",
                                                   "",
                                                   Helpers.CreateRunActions(Helpers.Create<NewMechanics.ContextActionRemoveBuffs>(b => b.Buffs = buffs.ToArray()), apply_buff),
                                                   Common.createAbilityCasterHasNoFacts(cooldowns[i]),
                                                   Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus,
                                                                                 stat: StatType.Intelligence),
                                                   Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(apply_cooldown)),
                                                   Helpers.CreateAbilityTargetsAround(30.Feet(), TargetType.Ally)
                                                   );
                ability.setMiscAbilityParametersSelfOnly();
                abilities.Add(ability);
            }

            var wrapper = Common.createVariantWrapper("OpportuneAdviceInvestigatorAbility", "", abilities.ToArray());
            wrapper.SetNameDescriptionIcon("Opportune Advice", buffs[0].Description, icon);


            var reset_cooldown = Helpers.CreateAbility("ResetCooldownOpportuneAdviceInvestigatorAbility",
                                                       "Opportune Advice: Reset Cooldown",
                                                       buffs[0].Description,
                                                       "",
                                                       icon,
                                                       AbilityType.Extraordinary,
                                                       CommandType.Free,
                                                       AbilityRange.Personal,
                                                       "",
                                                       "",
                                                       Helpers.CreateRunActions(Helpers.Create<NewMechanics.ContextActionRemoveBuffs>(b => b.Buffs = cooldowns.ToArray())),
                                                       inspiration_resource.CreateResourceLogic()
                                                       );
            reset_cooldown.setMiscAbilityParametersSelfOnly();

            opportune_advice = Common.AbilityToFeature(wrapper, false);
            opportune_advice.AddComponent(Helpers.CreateAddFact(reset_cooldown));



            var knowledgeable_strike_ability = Helpers.CreateAbility("KnowledgeableStrikeInvestigatorAbility",
                                           "Knowledgeable Strike",
                                           opportune_strike_buffs[0].Description,
                                           "",
                                           icon_opportune_strike,
                                           AbilityType.Extraordinary,
                                           CommandType.Move,
                                           AbilityRange.Personal,
                                           "",
                                           "",
                                           Common.createAbilityCasterHasFacts(buffs.ToArray()),
                                           Helpers.CreateAbilityTargetsAround(30.Feet(), TargetType.Ally)
                                           );

            var actions = new List<GameAction>();
            for (int i = 0; i < buffs.Count; i++)
            {
                var apply_oportune_strike = Common.createContextActionApplyBuff(opportune_strike_buffs[i], Helpers.CreateContextDuration(1), dispellable: false);
                var action = Helpers.CreateConditional(new Condition[] { Common.createContextConditionHasBuffFromCaster(buffs[i]) },
                                                       new GameAction[] { Common.createContextActionRemoveBuffFromCaster(buffs[i]), apply_oportune_strike }
                                                       );
                actions.Add(action);
            }

            knowledgeable_strike_ability.AddComponent(Helpers.CreateRunActions(actions.ToArray()));
            knowledgeable_strike = Common.AbilityToFeature(knowledgeable_strike_ability, false);


        }

        static void createPsychicDetective()
        {
            psychic_detective = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "PsychicDetectiveArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Psychic Detective");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "A psychic detective supplements her keen insight with occult skill to unravel mysteries both ordinary and supernatural.");
            });
            Helpers.SetField(psychic_detective, "m_ParentClass", investigator_class);
            library.AddAsset(psychic_detective, "");

            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");
            createPsychicSpellCasting();
            createPhrenicDabbler();
            createPsychicDiscoveries();

            psychic_meddler = Helpers.CreateFeature("PsychicMeddlerFeature",
                                                        "Psychic Meddler",
                                                        "At 2nd level, a psychic detective receives a +1 bonus on saves against mind-affecting spells and spell-like abilities. This bonus increases by 1 at 5th level and every 3 levels thereafter, to a maximum of +6 at 17th level.",
                                                        "",
                                                        Helpers.GetIcon("eabf94e4edc6e714cabd96aa69f8b207"), //mind fog
                                                        FeatureGroup.None,
                                                        Common.createContextSavingThrowBonusAgainstDescriptor(Helpers.CreateContextValue(AbilityRankType.Default), ModifierDescriptor.UntypedStackable, SpellDescriptor.MindAffecting),
                                                        Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getInvestigatorArray(), progression: ContextRankProgression.StartPlusDivStep,
                                                                                        startLevel: 2, stepLevel: 3)
                                                        );

            psychic_detective.RemoveFeatures = new LevelEntry[] {
                                                                   Helpers.LevelEntry(2, poison_resistance),
                                                                   Helpers.LevelEntry(3, investigator_talent_selection),
                                                                   Helpers.LevelEntry(11, poison_immunity),
                                                                };
            psychic_detective.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, detect_magic, psychic_spellcasting),
                                                               Helpers.LevelEntry(2, psychic_meddler),
                                                               Helpers.LevelEntry(3, phrenic_dabbler)
                                                             };

            psychic_detective.ReplaceSpellbook = psychic_detective_spellbook;
            investigator_class.Progression.UIDeterminatorsGroup = investigator_class.Progression.UIDeterminatorsGroup.AddToArray(detect_magic, psychic_spellcasting);
            investigator_class.Progression.UIGroups = investigator_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(psychic_meddler, phrenic_dabbler));

            psychic_detective.ReplaceClassSkills = true;
            psychic_detective.ClassSkills = new StatType[] {StatType.SkillStealth, StatType.SkillThievery,
                                                      StatType.SkillKnowledgeArcana, StatType.SkillKnowledgeWorld, StatType.SkillLoreNature, StatType.SkillLoreReligion,
                                                      StatType.SkillPerception, StatType.SkillPersuasion, StatType.SkillUseMagicDevice};

            infusion.AddComponent(Common.prerequisiteNoArchetype(investigator_class, psychic_detective));
            mutagen.AddComponent(Common.prerequisiteNoArchetype(investigator_class, psychic_detective));
            enhance_potion.AddComponent(Common.prerequisiteNoArchetype(investigator_class, psychic_detective));
            extend_potion.AddComponent(Common.prerequisiteNoArchetype(investigator_class, psychic_detective));
        }


        static void createPsychicDiscoveries()
        {
            extra_phrenic_pool = Helpers.CreateFeature("PsychicDetectiveExtraPhrenicPoolFeature",
                                                       "Expanded Phrenic Pool",
                                                       "Your phrenic pool total increases by 2 points.",
                                                       "",
                                                       Helpers.GetIcon("42f96fc8d6c80784194262e51b0a1d25"), //extra arcana
                                                       FeatureGroup.None,
                                                       Helpers.Create<IncreaseResourceAmount>(i => { i.Resource = phrenic_pool_resource; i.Value = 2; }),
                                                       Common.createPrerequisiteArchetypeLevel(investigator_class, psychic_detective, 3)
                                                       );

            extra_phrenic_amplification = Helpers.CreateFeatureSelection("PsychicDetectiveExtraPhrenicAmplificationFeatureSelection",
                                           "Extra Phrenic Amplification (Psychic Detective)",
                                           "You gain one additional phrenic amplification.",
                                           "",
                                           null,
                                           FeatureGroup.None,
                                           Common.createPrerequisiteArchetypeLevel(investigator_class, psychic_detective, 3)
                                           );
            extra_phrenic_amplification.AllFeatures = phrenic_dabbler.AllFeatures;
            extra_phrenic_amplification.AddComponent(Helpers.PrerequisiteNoFeature(extra_phrenic_amplification));

            investigator_talent_selection.AllFeatures = investigator_talent_selection.AllFeatures.AddToArray(extra_phrenic_pool, extra_phrenic_amplification);
        }


        static void createPhrenicDabbler()
        {
            phrenic_pool_resource = Helpers.CreateAbilityResource("PsychicDetectivePhrenicPoolResource", "", "", "", null);
            phrenic_pool_resource.SetIncreasedByLevelStartPlusDivStep(0, 2, 1, 2, 1, 0, 0.0f, getInvestigatorArray());

            phrenic_dabbler = Helpers.CreateFeatureSelection("PhrenicDabblerFeature",
                                                           "Phrenic Dabbler",
                                                           "At 3rd level, a psychic detective gains a small pool of phrenic points equal to 1/2 her psychic detective level, as well as one phrenic amplification, as the psychic class feature. This does not allow the psychic detective to qualify for the Extra Amplification feat.",
                                                           "",
                                                           null,
                                                           FeatureGroup.None,
                                                           Helpers.CreateAddAbilityResource(phrenic_pool_resource)
                                                           );
            var phrenic_amplifications_engine = new PhrenicAmplificationsEngine(phrenic_pool_resource, psychic_detective_spellbook, investigator_class, "PsychicDetective");

            focused_force = phrenic_amplifications_engine.createFocusedForce();
            biokinetic_healing = phrenic_amplifications_engine.createBiokineticHealing();
            conjured_armor = phrenic_amplifications_engine.createConjuredArmor();
            defensive_prognostication = phrenic_amplifications_engine.createDefensivePrognostication();
            minds_eye = phrenic_amplifications_engine.createMindsEye();
            overpowering_mind = phrenic_amplifications_engine.createOverpoweringMind();
            will_of_the_dead = phrenic_amplifications_engine.createWillOfTheDead();
            ongoing_defense = phrenic_amplifications_engine.createOngoingDefense();
            relentness_casting = phrenic_amplifications_engine.createRelentnessCasting();
            undercast_surge = phrenic_amplifications_engine.createUndercastSurge();
            psychofeedback = phrenic_amplifications_engine.createPsychofeedback();
            phrenic_dabbler.AllFeatures = new BlueprintFeature[]
            {
                biokinetic_healing,
                conjured_armor,
                defensive_prognostication,
                focused_force,
                minds_eye,
                overpowering_mind,
                will_of_the_dead,
                ongoing_defense,
                relentness_casting,
                undercast_surge,
                psychofeedback
            };

        }

        static void createPsychicSpellCasting()
        {
            psychic_spellcasting = Helpers.CreateFeature("PsychichDetectiveSpellCasting",
                                                         "Psychic Magic",
                                                         "A psychic detective casts psychic spells drawn from the psychic class spell list and augmented by a select set of additional spells specified below. Only spells from the psychic class spell list of 6th level or lower and psychic detective spells are considered to be part of the psychic detective’s spell list. If a spell appears on both the psychic detective and psychic class spell lists, the psychic detective uses the spell level from the psychic detective spell list. She can cast any spell she knows without preparing it ahead of time. To learn or cast a spell, a psychic detective must have an Intelligence score equal to at least 10 + the spell’s level. The saving throw DC against a psychic detective’s spell is 10 + the spell’s level + the psychic detective’s Intelligence modifier.\n"
                                                         + "Like other spellcasters, a psychic detective can cast only a certain number of spells of each spell level per day. She knows the same number of spells and receives the same number of spells slots per day as a bard of her investigator level, and knows and uses 0-level knacks as a bard uses cantrips. In addition, she receives bonus spells per day if she has a high Intelligence score.\n"
                                                         + "Additional Psychic detective spells: Find Traps (1st), Banishment (6th).\n",
                                                         "",
                                                         null,
                                                         FeatureGroup.None);

            var bard_class = library.TryGet<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");

            psychic_detective_spellbook = Helpers.Create<BlueprintSpellbook>();
            psychic_detective_spellbook.name = "PsychicDetectiveSpellbook";
            library.AddAsset(psychic_detective_spellbook, "");
            psychic_detective_spellbook.Name = psychic_detective.LocalizedName;
            psychic_detective_spellbook.SpellsPerDay = bard_class.Spellbook.SpellsPerDay;
            psychic_detective_spellbook.SpellsKnown = bard_class.Spellbook.SpellsKnown;
            psychic_detective_spellbook.Spontaneous = true;
            psychic_detective_spellbook.IsArcane = false;
            psychic_detective_spellbook.AllSpellsKnown = false;
            psychic_detective_spellbook.CanCopyScrolls = false;
            psychic_detective_spellbook.CastingAttribute = StatType.Intelligence;
            psychic_detective_spellbook.CharacterClass = investigator_class;
            psychic_detective_spellbook.CasterLevelModifier = 0;
            psychic_detective_spellbook.CantripsType = CantripsType.Orisions;
            psychic_detective_spellbook.SpellsPerLevel = bard_class.Spellbook.SpellsPerLevel;

            psychic_detective_spellbook.SpellList = Helpers.Create<BlueprintSpellList>();
            psychic_detective_spellbook.SpellList.name = "PsychicDetectiveSpellList";
            library.AddAsset(psychic_detective_spellbook.SpellList, "");
            psychic_detective_spellbook.SpellList.SpellsByLevel = new SpellLevelList[10];
            for (int i = 0; i < psychic_detective_spellbook.SpellList.SpellsByLevel.Length; i++)
            {
                psychic_detective_spellbook.SpellList.SpellsByLevel[i] = new SpellLevelList(i);

            }

            Common.SpellId[] spells = new Common.SpellId[]
            {
                new Common.SpellId( "55f14bc84d7c85446b07a1b5dd6b2b4c", 0), //daze
                new Common.SpellId( "f0f8e5b9808f44e4eadd22b138131d52", 0), //flare
                new Common.SpellId( "95f206566c5261c42aa5b3e7e0d1e36c", 0), //mage light
                new Common.SpellId( "7bc8e27cba24f0e43ae64ed201ad5785", 0), //resistance
                new Common.SpellId( "d3a852385ba4cd740992d1970170301a", 0), //virtue

                new Common.SpellId( NewSpells.burst_of_adrenaline.AssetGuid, 1),
                new Common.SpellId( NewSpells.burst_of_insight.AssetGuid, 1),
                new Common.SpellId( "bd81a3931aa285a4f9844585b5d97e51", 1), //cause fear
                new Common.SpellId( "91da41b9793a4624797921f221db653c", 1), //color spray
                new Common.SpellId( NewSpells.command.AssetGuid, 1),
                new Common.SpellId( "8e7cfa5f213a90549aadd18f8f6f4664", 1), //ear-piercing scream
                new Common.SpellId( "c60969e7f264e6d4b84a1499fdcf9039", 1), //enlarge person
                new Common.SpellId( "4f8181e7a7f1d904fbaea64220e83379", 1), //expeditious retreat
                new Common.SpellId( "4709274b2080b6444a3c11c6ebbe2404", 1), //find traps
                new Common.SpellId( "39a602aa80cc96f4597778b6d4d49c0a", 1), //flare burst
                new Common.SpellId( Witch.hermean_potential.AssetGuid, 1),
                new Common.SpellId( "88367310478c10b47903463c5d0152b0", 1), //hypnotism
                new Common.SpellId( Witch.ill_omen.AssetGuid, 1),
                new Common.SpellId( NewSpells.invigorate.AssetGuid, 1),
                new Common.SpellId( NewSpells.long_arm.AssetGuid, 1),                
                new Common.SpellId( "9e1ad5d6f87d19e4d8883d63a6e35568", 1), //mage armor
                new Common.SpellId( "4ac47ddb9fa1eaf43a1b6809980cfbd2", 1), //magic missile
                new Common.SpellId( NewSpells.mind_thrust[0].AssetGuid, 1),
                new Common.SpellId( "4e0e9aba6447d514f88eff1464cc4763", 1), //reduce person
                new Common.SpellId( "55a037e514c0ee14a8e3ed14b47061de", 1), //remove fear
                new Common.SpellId( "ef768022b0785eb43a18969903c537c4", 1), //shield
                new Common.SpellId( "bb7ecad2d3d2c8247a38f44855c99061", 1), //sleep
                new Common.SpellId( "a5ec7892fb1c2f74598b3a82f3fd679f", 1), //stunning barrier
                new Common.SpellId( "8fd74eddd9b6c224693d9ab241f25e84", 1), //summon monster 1
                new Common.SpellId( "2c38da66e5a599347ac95b3294acbe00", 1), //true strike
                new Common.SpellId( "f001c73999fb5a543a199f890108d936", 1), //vanish

                new Common.SpellId( "a900628aea19aa74aad0ece0e65d091a", 2), //bear's endurance
                new Common.SpellId( "46fd02ad56c35224c9c91c88cd457791", 2), //blindness
                new Common.SpellId( NewSpells.blood_armor.AssetGuid, 2),
                new Common.SpellId( "14ec7a4e52e90fa47a4c8d63c69fd5c1", 2), //blur
                new Common.SpellId( NewSpells.bone_fists.AssetGuid, 2),
                new Common.SpellId( "4c3d08935262b6544ae97599b3a9556d", 2), //bull's strength
                new Common.SpellId( "de7a025d48ad5da4991e7d3c682cf69d", 2), //cat's grace
                new Common.SpellId( "446f7bf201dc1934f96ac0a26e324803", 2), //eagle's splendor
                new Common.SpellId( "e1291272c8f48c14ab212a599ad17aac", 2), //effortless armor
                new Common.SpellId( "7a5b5bf845779a941a67251539545762", 2), //false life
                new Common.SpellId( NewSpells.force_sword.AssetGuid, 2),
                new Common.SpellId( "ae4d3ad6a8fda1542acf2e9bbc13d113", 2), //fox cunning
                new Common.SpellId( "fd4d9fd7f87575d47aafe2a64a6e2d8d", 2), //hideous laughter
                new Common.SpellId( "c7104f7526c4c524f91474614054547e", 2), //hold person
                new Common.SpellId( "41bab342089c0254ca222eb918e98cd4", 2), //hold animal
                new Common.SpellId( NewSpells.howling_agony.AssetGuid, 2),
                new Common.SpellId( NewSpells.hypnotic_pattern.AssetGuid, 2),
                new Common.SpellId( NewSpells.inflict_pain.AssetGuid, 2),
                new Common.SpellId( "89940cde01689fb46946b2f8cd7b66b7", 2), //invisibility
                new Common.SpellId( NewSpells.mental_barrier[0].AssetGuid, 2),
                new Common.SpellId( NewSpells.mind_thrust[1].AssetGuid, 2),
                new Common.SpellId( "3e4ab69ada402d145a5e0ad3ad4b8564", 2), //mirror image
                new Common.SpellId( NewSpells.pain_strike.AssetGuid, 2),
                new Common.SpellId( "c28de1f98a3f432448e52e5d47c73208", 2), //protection from arrows
                new Common.SpellId( "21ffef7791ce73f468b6fca4d9371e8b", 2), //resist energy
                new Common.SpellId( "08cb5f4c3b2695e44971bf5c45205df0", 2), //scare
                new Common.SpellId( NewSpells.savage_maw.AssetGuid, 2),
                new Common.SpellId( NewSpells.silence.AssetGuid, 2),
                new Common.SpellId( "f0455c9295b53904f9e02fc571dd2ce1", 2), //owl's wisdom
                new Common.SpellId( "30e5dc243f937fc4b95d2f8f4e1b7ff3", 2), //see invisibility
                new Common.SpellId( "970c6db48ff0c6f43afc9dbb48780d03", 2), //summon small elemental
                new Common.SpellId( "1724061e89c667045a6891179ee2e8e7", 2), //summon monster 2
                new Common.SpellId( NewSpells.telekinetic_strikes.AssetGuid, 2),
                new Common.SpellId( NewSpells.thought_shield[0].AssetGuid, 2),
                new Common.SpellId( NewSpells.warding_weapon.AssetGuid, 2),

                new Common.SpellId( NewSpells.babble.AssetGuid, 3),
                new Common.SpellId( "0a2f7c6aa81bc6548ac7780d8b70bcbc", 3), //battering blast (it seems it should be on the list since all force spells are there)
                new Common.SpellId( NewSpells.countless_eyes.AssetGuid, 3),
                new Common.SpellId( NewSpells.daze_mass.AssetGuid, 3),
                new Common.SpellId( "7658b74f626c56a49939d9c20580885e", 3), //deep slumber
                new Common.SpellId( "92681f181b507b34ea87018e8f7a528a", 3), //dispel magic
                new Common.SpellId( "903092f6488f9ce45a80943923576ab3", 3), //displacement
                new Common.SpellId( NewSpells.fly.AssetGuid, 3),
                new Common.SpellId( "486eaff58293f6441a5c2759c4872f98", 3), //haste
                new Common.SpellId( "5ab0d42fb68c9e34abae4921822b9d63", 3), //heroism
                new Common.SpellId( NewSpells.locate_weakness.AssetGuid, 3),
                new Common.SpellId( NewSpells.mental_barrier[1].AssetGuid, 3),
                new Common.SpellId( NewSpells.mind_thrust[2].AssetGuid, 3),
                new Common.SpellId( "96c9d98b6a9a7c249b6c4572e4977157", 3), //protection from arrows communal
                new Common.SpellId( "d2f116cfe05fcdd4a94e80143b67046f", 3), //protection from energy
                new Common.SpellId( "97b991256e43bb140b263c326f690ce2", 3), //rage
                new Common.SpellId( "7bb0c402f7f789d4d9fae8ca87b4c7e2", 3), //resist energy communal
                new Common.SpellId( NewSpells.resinous_skin.AssetGuid, 3),
                new Common.SpellId( NewSpells.sands_of_time.AssetGuid, 3),
                new Common.SpellId( NewSpells.shadow_enchantment.AssetGuid, 3),
                new Common.SpellId( "f492622e473d34747806bdb39356eb89", 3), //slow
                new Common.SpellId( NewSpells.stunning_barrier_greater.AssetGuid, 3),
                new Common.SpellId( NewSpells.synaptic_pulse.AssetGuid, 3),
                new Common.SpellId( NewSpells.synesthesia.AssetGuid, 3),
                new Common.SpellId( NewSpells.thought_shield[1].AssetGuid, 3),
                new Common.SpellId( SpiritualWeapons.twilight_knife.AssetGuid, 3),
                new Common.SpellId( "8a28a811ca5d20d49a863e832c31cce1", 3), //vampyric touch
                new Common.SpellId( NewSpells.wall_of_nausea.AssetGuid, 3),
                new Common.SpellId( "5d61dde0020bbf54ba1521f7ca0229dc", 3), //summon monster 3

                new Common.SpellId( NewSpells.aura_of_doom.AssetGuid, 4),
                new Common.SpellId( "7792da00c85b9e042a0fdfc2b66ec9a8", 4), //break enchantment
                new Common.SpellId( "cf6c901fb7acc904e85c63b342e9c949", 4), //confusion
                new Common.SpellId( "4baf4109145de4345861fe0f2209d903", 4), //crushing despair
                new Common.SpellId( NewSpells.debilitating_portent.AssetGuid, 4),
                new Common.SpellId( "4a648b57935a59547b7a2ee86fb4f26a", 4), //dimensions door
                new Common.SpellId( "754c478a2aa9bb54d809e648c3f7ac0e", 4), //dominate animal
                new Common.SpellId( "66dc49bf154863148bd217287079245e", 4), //enlarge person mass
                new Common.SpellId( "dc6af3b4fd149f841912d8a3ce0983de", 4), //false life, greater
                new Common.SpellId( "d2aeac47450c76347aebbc02e4f463e0", 4), //fear
                new Common.SpellId( NewSpells.fleshworm_infestation.AssetGuid, 4),
                new Common.SpellId( "0087fc2d64b6095478bc7b8d7d512caf", 4), //freedom of movement
                new Common.SpellId( NewSpells.globe_of_invulnerability_lesser.AssetGuid, 4),
                new Common.SpellId( NewSpells.intellect_fortress.AssetGuid, 4),
                new Common.SpellId( NewSpells.invigorate_mass.AssetGuid, 4),
                new Common.SpellId( "ecaa0def35b38f949bd1976a6c9539e0", 4), //invisibility greater
                new Common.SpellId( NewSpells.mental_barrier[2].AssetGuid, 4),
                new Common.SpellId( NewSpells.mind_thrust[3].AssetGuid, 4),
                new Common.SpellId( "dd2918e4a77c50044acba1ac93494c36", 4), //overwhelming grief
                new Common.SpellId( NewSpells.pain_strike_mass.AssetGuid, 4),
                new Common.SpellId( "6717dbaef00c0eb4897a1c908a75dfe5", 4), //phantasmal killer
                new Common.SpellId( "76a629d019275b94184a1a8733cac45e", 4), //protection from energy communal
                new Common.SpellId( "4b8265132f9c8174f87ce7fa6d0fe47b", 4), //rainbow pattern
                new Common.SpellId( "2427f2e3ca22ae54ea7337bbab555b16", 4), //reduce person mass  
                new Common.SpellId( "f09453607e683784c8fca646eec49162", 4), //shout
                new Common.SpellId( "c66e86905f7606c4eaa5c774f0357b2b", 4), //stoneskin
                new Common.SpellId( "e42b1dbff4262c6469a9ff0a6ce730e3", 4), //summon medium elemental
                new Common.SpellId( "7ed74a3ec8c458d4fb50b192fd7be6ef", 4), //summon monster 4
                new Common.SpellId( NewSpells.thought_shield[2].AssetGuid, 4),
                new Common.SpellId( NewSpells.wall_of_blindness.AssetGuid, 4),

                new Common.SpellId( NewSpells.burst_of_force.AssetGuid, 5),
                new Common.SpellId( NewSpells.command_greater.AssetGuid, 5),
                new Common.SpellId( "95f7cdcec94e293489a85afdf5af1fd7", 5), //dismissal
                new Common.SpellId( "d7cbd2004ce66a042aeab2e95a3c5c61", 5), //dominate person
                new Common.SpellId( "20b548bf09bb3ea4bafea78dcb4f3db6", 5), //echolocation
                new Common.SpellId( "444eed6e26f773a40ab6e4d160c67faa", 5), //feeblemind
                new Common.SpellId( "41e8a952da7a5c247b3ec1c2dbb73018", 5), //hold monster
                new Common.SpellId( "eabf94e4edc6e714cabd96aa69f8b207", 5), //mind fog
                new Common.SpellId( NewSpells.mind_thrust[4].AssetGuid, 5),
                new Common.SpellId( NewSpells.overland_flight.AssetGuid, 5),
                new Common.SpellId( "12fb4a4c22549c74d949e2916a2f0b6a", 5), //phantasmal web
                new Common.SpellId( "d316d3d94d20c674db2c24d7de96f6a7", 5), //serenity
                new Common.SpellId( "d38aaf487e29c3d43a3bffa4a4a55f8f", 5), //song of discord
                new Common.SpellId( "0a5ddfbcfb3989543ac7c936fc256889", 5), //spell resistance
                new Common.SpellId( "7c5d556b9a5883048bf030e20daebe31", 5), //stoneskin communal
                new Common.SpellId( "89404dd71edc1aa42962824b44156fe5", 5), //summon large elemental
                new Common.SpellId( "630c8b85d9f07a64f917d79cb5905741", 5), //summon monster 5
                new Common.SpellId( NewSpells.mental_barrier[3].AssetGuid, 5),
                new Common.SpellId( NewSpells.psychic_crush[0].AssetGuid, 5),
                new Common.SpellId( "4cf3d0fae3239ec478f51e86f49161cb", 5), //true seeing
                new Common.SpellId( NewSpells.suffocation.AssetGuid, 5),
                new Common.SpellId( NewSpells.synapse_overload.AssetGuid, 5),
                new Common.SpellId( NewSpells.synaptic_pulse_greater.AssetGuid, 5),
                new Common.SpellId( "8878d0c46dfbd564e9d5756349d5e439", 5), //waves of fatigue
                
                new Common.SpellId( "d42c6d3f29e07b6409d670792d72bc82", 6), //banshee blast ? (probably should be since wail of banshee is also on the list)
                new Common.SpellId( "d361391f645db984bbf58907711a146a", 6), //banishment
                new Common.SpellId( "f6bcea6db14f0814d99b54856e918b92", 6), //bears endurance mass
                new Common.SpellId( "36c8971e91f1745418cc3ffdfac17b74", 6), //blade barrier
                new Common.SpellId( "6a234c6dcde7ae94e94e9c36fd1163a7", 6), //bull strength mass
                new Common.SpellId( "1f6c94d56f178b84ead4c02f1b1e1c48", 6), //cat grace mass
                new Common.SpellId( "7f71a70d822af94458dc1a235507e972", 6), //cloak of dreams
                new Common.SpellId( NewSpells.contingency.AssetGuid, 6),
                new Common.SpellId( NewSpells.curse_major.AssetGuid, 6),
                new Common.SpellId( "f0f761b808dc4b149b08eaf44b99f633", 6), //dispel magic, greater
                new Common.SpellId( "4aa7942c3e62a164387a73184bca3fc1", 6), //disintegrate
                new Common.SpellId( "2caa607eadda4ab44934c5c9875e01bc", 6), //eagles splendor mass
                new Common.SpellId( NewSpells.fluid_form.AssetGuid, 6),
                new Common.SpellId( "2b24159ad9907a8499c2313ba9c0f615", 6), //fox cunning mass
                new Common.SpellId( NewSpells.globe_of_invulnerability.AssetGuid, 6),
                new Common.SpellId( "e15e5e7045fda2244b98c8f010adfe31", 6), //heroism greater
                new Common.SpellId( NewSpells.inflict_pain_mass.AssetGuid, 6),
                new Common.SpellId( "15a04c40f84545949abeedef7279751a", 6), //joyful rapture
                new Common.SpellId( NewSpells.mental_barrier[4].AssetGuid, 6),
                new Common.SpellId( NewSpells.mind_thrust[5].AssetGuid, 6),
                new Common.SpellId( "9f5ada581af3db4419b54db77f44e430", 6), //owls wisdom mass    
                new Common.SpellId( "07d577a74441a3a44890e3006efcf604", 6), //primal regression
                new Common.SpellId( NewSpells.psychic_crush[1].AssetGuid, 6),
                new Common.SpellId( NewSpells.psychic_surgery.AssetGuid, 6),
                new Common.SpellId( NewSpells.shadow_enchantment_greater.AssetGuid, 6),
                new Common.SpellId( "766ec978fa993034f86a372c8eb1fc10", 6), //summon huge elemental
                new Common.SpellId( "e740afbab0147944dab35d83faa0ae1c", 6), //summon monster 6
                new Common.SpellId( "27203d62eb3d4184c9aced94f22e1806", 6), //transformation 
                
            };

            foreach (var spell_id in spells)
            {
                var spell = library.Get<BlueprintAbility>(spell_id.guid);
                spell.AddToSpellList(psychic_detective_spellbook.SpellList, spell_id.level);
            }

            psychic_detective_spellbook.AddComponent(Helpers.Create<SpellbookMechanics.PsychicSpellbook>());

            var center_self_buff = Helpers.CreateBuff("CenterSelfBuff",
                                                  "Center Self",
                                                  "A psychic spellcaster casting a spell with a thought component can take a move action before beginning to cast the spell to center herself; she can then use the normal concentration DC instead of the increased DC.",
                                                  "",
                                                  Helpers.GetIcon("00369aa0a76141a479382e360b1f3dd7"),
                                                  null,
                                                  Helpers.Create<SpellFailureMechanics.CenterSelf>()
                                                  );
            center_self_buff.Stacking = StackingType.Replace;
            var center_self_ability = Helpers.CreateAbility("CenterSelfAbility",
                                                            center_self_buff.Name,
                                                            center_self_buff.Description,
                                                            "",
                                                            center_self_buff.Icon,
                                                            AbilityType.Special,
                                                            CommandType.Move,
                                                            AbilityRange.Personal,
                                                            Helpers.oneRoundDuration,
                                                            "",
                                                            Helpers.CreateRunActions(Common.createContextActionApplyBuff(center_self_buff, Helpers.CreateContextDuration(1), dispellable: false))
                                                            );
            center_self_ability.setMiscAbilityParametersSelfOnly();
            center_self = Common.AbilityToFeature(center_self_ability);

            psychic_spellcasting.AddComponents(Helpers.Create<SpellFailureMechanics.PsychicSpellbook>(p => p.spellbook = psychic_detective_spellbook),
                                               Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.NaturalSpell));
            psychic_spellcasting.AddComponent(Helpers.Create<SpellbookMechanics.AddUndercastSpells>(p => p.spellbook = psychic_detective_spellbook));
            psychic_spellcasting.AddComponent(Helpers.CreateAddFact(center_self));
            psychic_spellcasting.AddComponents(Common.createCantrips(investigator_class, StatType.Intelligence, psychic_detective_spellbook.SpellList.SpellsByLevel[0].Spells.ToArray()));
            psychic_spellcasting.AddComponents(Helpers.CreateAddFacts(psychic_detective_spellbook.SpellList.SpellsByLevel[0].Spells.ToArray()));
        }


        static void createJinyiwei()
        {
            jinyiwei_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "JinyiweiArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Jinyiwei");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Jinyiwei seek out and eliminate corruption wherever they find it, claiming they act under the celestial government’s mandate.");
            });
            Helpers.SetField(jinyiwei_archetype, "m_ParentClass", investigator_class);
            library.AddAsset(jinyiwei_archetype, "");

            jinyiwei_archetype.ChangeCasterType = true;
            jinyiwei_archetype.IsDivineCaster = true;
            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");
            createDivineInspiration();
            createCelestialInsight();

            var suspicious_mind = Helpers.CreateFeature("JinyweiSuspiciousMindFeature",
                                                        "Suspicious Mind",
                                                        "A jinyiwei adds half her level (minimum 1) to all her Perception checks.",
                                                        "",
                                                        Helpers.GetIcon("c927a8b0cd3f5174f8c0b67cdbfde539"),
                                                        FeatureGroup.None,
                                                        Helpers.CreateAddContextStatBonus(StatType.SkillPerception, ModifierDescriptor.None),
                                                        Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getInvestigatorArray(), progression: ContextRankProgression.Div2, min: 1)
                                                        );

            jinyiwei_archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, inspiration, trapfinding),
                                                                   Helpers.LevelEntry(3, trap_sense),
                                                                   Helpers.LevelEntry(6, trap_sense),
                                                                   Helpers.LevelEntry(9, trap_sense),
                                                                   Helpers.LevelEntry(12, trap_sense),
                                                                   Helpers.LevelEntry(15, trap_sense),
                                                                   Helpers.LevelEntry(18, trap_sense) };
            jinyiwei_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, detect_magic, divine_inspiration, suspicious_mind),
                                                                  Helpers.LevelEntry(3, celestial_insight) };

            investigator_progression.UIDeterminatorsGroup = investigator_progression.UIDeterminatorsGroup.AddToArray(suspicious_mind);
            jinyiwei_archetype.ReplaceSpellbook = jinyiwei_spellbook;
            investigator_class.Progression.UIDeterminatorsGroup = investigator_class.Progression.UIDeterminatorsGroup.AddToArray(detect_magic, divine_inspiration);
            infusion.AddComponent(Common.prerequisiteNoArchetype(investigator_class, jinyiwei_archetype));
            mutagen.AddComponent(Common.prerequisiteNoArchetype(investigator_class, jinyiwei_archetype));
            enhance_potion.AddComponent(Common.prerequisiteNoArchetype(investigator_class, jinyiwei_archetype));
            extend_potion.AddComponent(Common.prerequisiteNoArchetype(investigator_class, jinyiwei_archetype));
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
            jinyiwei_spellbook.Name = Helpers.CreateString("JynyiweySpellbook.Name", "Jinyiwey");
        }


        static void createCelestialInsight()
        {
            celestial_insight = Helpers.CreateFeature("CelestialInsightJynyiweyFeature",
                                                      "Celestial Insight",
                                                      "At 3rd level, a jinyiwei learns to see through the types of magic that often lead others astray. She gains a +1 competence bonus on saving throws to resist enchantment and illusion effects. At 6th level and every 3 levels thereafter, these bonuses increase by 1 (to a maximum of +6 at 18th level).",
                                                      "",
                                                      Helpers.GetIcon("b1c7576bd06812b42bda3f09ab202f14"),
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
            questioner_archetype.ChangeCasterType = true;
            questioner_archetype.IsArcaneCaster = true;

            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");
            createInspirationForSubterfuge();
            createQuestionerSpellcasting();
            createKnowItAll();

            questioner_archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, inspiration), Helpers.LevelEntry(2, poison_resistance), Helpers.LevelEntry(11, poison_immunity) };
            questioner_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, detect_magic, questioner_spellcasting, inspiration_for_subterfuge),
                                                                  Helpers.LevelEntry(2, know_it_all) };

            questioner_archetype.ReplaceSpellbook = questioner_spellbook;
            investigator_class.Progression.UIGroups[3].Features.Add(know_it_all);
            investigator_class.Progression.UIDeterminatorsGroup = investigator_class.Progression.UIDeterminatorsGroup.AddToArray(detect_magic, questioner_spellcasting, inspiration_for_subterfuge);
            infusion.AddComponent(Common.prerequisiteNoArchetype(investigator_class, questioner_archetype));
            mutagen.AddComponent(Common.prerequisiteNoArchetype(investigator_class, questioner_archetype));
            enhance_potion.AddComponent(Common.prerequisiteNoArchetype(investigator_class, questioner_archetype));
            extend_potion.AddComponent(Common.prerequisiteNoArchetype(investigator_class, questioner_archetype));
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
                                                Helpers.CreateAddContextStatBonus(StatType.SkillKnowledgeArcana, ModifierDescriptor.UntypedStackable),
                                                Helpers.CreateAddContextStatBonus(StatType.SkillKnowledgeWorld, ModifierDescriptor.UntypedStackable),
                                                Helpers.CreateAddContextStatBonus(StatType.SkillLoreNature, ModifierDescriptor.UntypedStackable),
                                                Helpers.CreateAddContextStatBonus(StatType.SkillLoreReligion, ModifierDescriptor.UntypedStackable),
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
                                                   
                                                    Helpers.Create<ShadowSpells.SavingthrowBonusAgainstDisbelief>(s =>
                                                                                                                        {
                                                                                                                            s.value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                                                                            s.save_type = SavingThrowType.Will;
                                                                                                                            s.school = SpellSchool.Illusion;
                                                                                                                            s.descriptor = ModifierDescriptor.Insight;
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


            extend_potion = library.Get<BlueprintFeature>("1db8e126b77e71d4d89dcfa50fe87e93");
            enhance_potion = library.CopyAndAdd<BlueprintFeature>("2673ccdd6df742d42a8c94977c76a984", "InvestigatorEnhancePotionFeature", "");
            enhance_potion.ReplaceComponent<EnhancePotion>(e => e.SpecializedClass = investigator_class);




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
                extend_potion,
                enhance_potion,
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
                Common.createAddInitiatorAttackRollTrigger2(Helpers.CreateActionList(checked_action),
                                                                 check_weapon_range_type: true),
                 Common.createAddInitiatorAttackRollTrigger2(Helpers.CreateActionList(checked_action_ranged),
                                                                  check_weapon_range_type: true,
                                                                  range_type: AttackTypeAttackBonus.WeaponRangeType.Ranged)
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
                Common.createAddInitiatorAttackRollTrigger2(Helpers.CreateActionList(checked_action),
                                                                 check_weapon_range_type: true),
                 Common.createAddInitiatorAttackRollTrigger2(Helpers.CreateActionList(checked_action_ranged),
                                                                  check_weapon_range_type: true,
                                                                  range_type: AttackTypeAttackBonus.WeaponRangeType.Ranged)
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
                Common.createAddInitiatorAttackRollTrigger2(Helpers.CreateActionList(checked_action),
                                                                 check_weapon_range_type: true),
                 Common.createAddInitiatorAttackRollTrigger2(Helpers.CreateActionList(checked_action_ranged),
                                                                  check_weapon_range_type: true,
                                                                  range_type: AttackTypeAttackBonus.WeaponRangeType.Ranged)
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
                                          Common.createAddInitiatorAttackRollTrigger2(Helpers.CreateActionList(apply_slow_reactions),
                                                                                           check_weapon_range_type: true),
                                          Common.createAddInitiatorAttackRollTrigger2(Helpers.CreateActionList(ranged_apply_slow_reactions),
                                                                                           check_weapon_range_type: true,
                                                                                           range_type: AttackTypeAttackBonus.WeaponRangeType.Ranged)
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
                                          Common.createAddInitiatorAttackRollTrigger2(Helpers.CreateActionList(apply_sickened),
                                                                                           check_weapon_range_type: true),
                                          Common.createAddInitiatorAttackRollTrigger2(Helpers.CreateActionList(ranged_apply_sickened),
                                                                                           check_weapon_range_type: true,
                                                                                           range_type: AttackTypeAttackBonus.WeaponRangeType.Ranged)
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
                              + "An investigator has the ability to augment skill checks and ability checks through his brilliant inspiration. The investigator has an inspiration pool equal to 1/2 his investigator level + his Intelligence modifier (minimum 1). An investigator’s inspiration pool refreshes each day, typically after he gets a restful night’s sleep. As a free action, he can expend one use of inspiration from his pool to add 1d6 to the result of that check, including any on which he takes 10 or 20. This choice is made after the check is rolled and before the results are revealed. An investigator can only use inspiration once per check or roll. The investigator can use inspiration on any Knowledge skill checks without expending a use of inspiration.\n"
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
                                                                         a.stats = new StatType[] { StatType.Strength, StatType.Dexterity, StatType.Constitution, StatType.Intelligence, StatType.Wisdom, StatType.Charisma };
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
                                                     "Inspiration: Persuasion",
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
            trapfinding.SetDescription("An investigator adds 1/2 her level on Perception and Trickery checks.");
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
                                                      Common.createContextSavingThrowBonusAgainstDescriptor(Helpers.CreateContextValue(AbilityRankType.Default), ModifierDescriptor.UntypedStackable, SpellDescriptor.Poison),
                                                      Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getInvestigatorArray(),
                                                                                      progression: ContextRankProgression.StartPlusDoubleDivStep,
                                                                                      startLevel: 2, stepLevel: 3)
                                                     );
        }


        static void createStudiedCombat()
        {
            ranged_study = Helpers.CreateFeature("RangedStudyFeature",
                                               "Ranged Study",
                                               "You gain the bonuses for studied combat with ranged weapons and can use studied strike with ranged weapons as long as the target of your studied strike is within 30 feet of you.\n"
                                               + "Normal: You gain the bonuses for studied combat and can use studied strike only with melee weapons.",
                                               "",
                                               null,
                                               FeatureGroup.Feat);
            library.AddFeats(ranged_study);
            createStudiedDefense();
            var icon = Helpers.GetIcon("b96d810ceb1708b4e895b695ddbb1813");

            var studied_target_cooldown = Helpers.CreateBuff("InvestigatorStudiedTargetCooldownBuff",
                                                             "Studied Target Cooldown",
                                                              "With a keen eye and calculating mind, an investigator can assess the mettle of his opponent to take advantage of gaps in talent and training. At 4th level, an investigator can use a move action to study a single enemy that he can see. Upon doing so, he adds 1/2 his investigator level as an insight bonus on melee attack rolls and as a bonus on damage rolls against the creature. This effect lasts for a number of rounds equal to his Intelligence modifier (minimum 1) or until he deals damage with a studied strike, whichever comes first. The bonus on damage rolls is precision damage, and is not multiplied on a critical hit.\n"
                                                              + "An investigator can only have one target of studied combat at a time, and once a creature has become the target of an investigator’s studied combat, he cannot become the target of the same investigator’s studied combat again for 24 hours unless the investigator expends one use of inspiration when taking the move action to use this ability.",
                                                              "",
                                                              icon,
                                                              null);
            studied_target_cooldown.SetBuffFlags(BuffFlags.RemoveOnRest);
            var apply_cooldown = Common.createContextActionApplyBuff(studied_target_cooldown, Helpers.CreateContextDuration(1, DurationRate.Days), dispellable: false);
            var studied_attack_buff = Helpers.CreateBuff("InvestigatorStudiedTargetAttackBuff",
                                                             "Studied Target",
                                                             studied_target_cooldown.Description,
                                                             "",
                                                             icon,
                                                             null
                                                             );
            var studied_defense_buff = Helpers.CreateBuff("InvestigatorStudiedDefenseBuff",
                                                         "Studied Defense Target",
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
                                       Common.createPrerequisiteParametrizedFeatureWeapon(weapon_focus, WeaponCategory.Javelin, any: true),
                                       Common.createPrerequisiteParametrizedFeatureWeapon(weapon_focus, WeaponCategory.HeavyCrossbow, any: true),
                                       Common.createPrerequisiteParametrizedFeatureWeapon(weapon_focus, WeaponCategory.HeavyRepeatingCrossbow, any: true),
                                       Common.createPrerequisiteParametrizedFeatureWeapon(weapon_focus, WeaponCategory.LightRepeatingCrossbow, any: true),
                                       Common.createPrerequisiteParametrizedFeatureWeapon(weapon_focus, WeaponCategory.HandCrossbow, any: true),
                                       Common.createPrerequisiteParametrizedFeatureWeapon(weapon_focus, WeaponCategory.Shuriken, any: true),
                                       Common.createPrerequisiteParametrizedFeatureWeapon(weapon_focus, WeaponCategory.ThrowingAxe, any: true),
                                       Common.createPrerequisiteParametrizedFeatureWeapon(weapon_focus, WeaponCategory.Dart, any: true),
                                       Common.createPrerequisiteParametrizedFeatureWeapon(weapon_focus, WeaponCategory.Ray, any: true),
                                       Common.createPrerequisiteParametrizedFeatureWeapon(weapon_focus, WeaponCategory.KineticBlast, any: true)
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
            var remove_studied_strike = Common.createContextActionRemoveBuffFromCaster(studied_target_buff);
            var remove_studied_strike_ranged = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(ranged_study), remove_studied_strike);
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
                                          Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(remove_studied_strike),
                                                                                           check_weapon_range_type: true, wait_for_attack_to_resolve: true),
                                          Common.createAddInitiatorCalculateDamageAfterAttckRollTrigger(Helpers.CreateActionList(remove_studied_strike_ranged),
                                                                                                        check_weapon_range_type: true,
                                                                                                        range_type: AttackTypeAttackBonus.WeaponRangeType.Ranged),
                                          /*(Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(remove_studied_strike_ranged),
                                                                                           check_weapon_range_type: true, wait_for_attack_to_resolve: true, 
                                                                                           range_type: AttackTypeAttackBonus.WeaponRangeType.Ranged),*/
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
