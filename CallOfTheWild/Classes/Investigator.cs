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
    class Investigator
    {
        static LibraryScriptableObject library => Main.library;
        internal static bool test_mode = false;
        static public BlueprintCharacterClass investigator_class;
        static public BlueprintProgression investigator_progression;

        static public BlueprintFeature investigator_proficiencies;
        static public BlueprintFeature trapfinding;
        static public BlueprintFeature inspiration;
        static public BlueprintAbilityResource inspiration_resource;
        static public BlueprintBuff ability_checks_inspiration_buff;
        static public BlueprintBuff attack_rolls_inspiration_buff;
        static public BlueprintBuff saving_throws_inspiration_buff;
        static public Dictionary<StatType, BlueprintBuff> skill_inspiration_buffs = new Dictionary<StatType, BlueprintBuff>();
        static public BlueprintBuff persuation_inspiration_buff;

        static public BlueprintFeature poison_resistance;
        static public BlueprintFeature poison_immunity;
        static public BlueprintFeature trap_sense;
        static public BlueprintFeatureSelection investigator_talent_selection;
        static public BlueprintFeature studied_combat;
        static public BlueprintBuff studied_target_buff;
        static public BlueprintAbility studied_combat_ability;
        static public BlueprintAbility studied_combat_ability_ignore_cooldown;
        static public BlueprintFeature studied_strike;
        static public BlueprintBuff studied_strike_buff;
        static public BlueprintFeature true_inspiration;

        internal static void createinvestigatorClass()
        {
            Main.logger.Log("Investigator class test mode: " + test_mode.ToString());
            var rogue_class = library.TryGet<BlueprintCharacterClass>("299aa766dee3cbf4790da4efb8c72484");
            var bard_class = library.TryGet<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");

            investigator_class = Helpers.Create<BlueprintCharacterClass>();
            investigator_class.name = "InvestigatorClass";
            library.AddAsset(investigator_class, "");

            investigator_class.LocalizedName = Helpers.CreateString("Investigator.Name", "Investigator");
            investigator_class.LocalizedDescription = Helpers.CreateString("Investigator.Description",
                                                                         "Whether on the trail of a fugitive, a long - lost treasure trove, or a criminal mastermind, investigators are motivated by an intense curiosity about the world and use knowledge of it as a weapon. Mixing gumption and learnedness into a personal alchemy of daring, investigators are full of surprises.Observing the world around them, they gain valuable knowledge about the situation they’re in, process that information using inspiration and deduction, and cut to the quick of the matter in unexpected ways. Investigators are always evaluating situations they encounter, sizing up potential foes, and looking out for secret dangers, all while using their vast knowledge and powers of perception to find solutions to the most perplexing problems.\n"
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
            investigator_class.ClassSkills = new StatType[] {StatType.SkillMobility, StatType.SkillStealth, 
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

            investigator_class.Archetypes = new BlueprintArchetype[] { }; //empiricist, questioner
            Helpers.RegisterClass(investigator_class);
            //addToPrestigeClasses(); 
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
            //createInvestigatorTalents();
            createTrapSense();
            createPoisonResistanceAndImmunity();
            createStudiedCombat();
            createStudiedStrike();
        }


        static void createInspiration()
        {
            inspiration_resource = Helpers.CreateAbilityResource("InvestigatorInspirationResource", "", "", "", null);
            inspiration_resource.SetIncreasedByLevel(0, 1, getInvestigatorArray()); //more inspiration uses since it is difficult to control when to make rerolls
            inspiration_resource.SetIncreasedByStat(0, StatType.Intelligence);

            List<BlueprintBuff> inspiration_buffs = new List<BlueprintBuff>();
            var description = "An investigator is beyond knowledgeable and skilled—he also possesses keen powers of observation and deduction that far surpass the abilities of others. An investigator typically uses these powers to aid in their investigations, but can also use these flashes of inspiration in other situations.\n"
                              + "An investigator has the ability to augment skill checks and ability checks through his brilliant inspiration. The investigator has an inspiration pool equal to his investigator level + his Intelligence modifier (minimum 1). An investigator’s inspiration pool refreshes each day, typically after he gets a restful night’s sleep. As a free action, he can expend one use of inspiration from his pool to add 1d6 to the result of that check, including any on which he takes 10 or 20. This choice is made after the check is rolled and before the results are revealed. An investigator can only use inspiration once per check or roll. The investigator can use inspiration on any Knowledge skill checks without expending a use of inspiration.\n"
                              + "Inspiration can also be used on attack rolls and saving throws, at the cost of expending two uses of inspiration each time from the investigator’s pool.";

            inspiration = Helpers.CreateFeature("InspirationFeature",
                                                "Inspiration",
                                                description,
                                                "",
                                                Helpers.GetIcon("4ebaf39efb8ffb64baf92784808dc49c"),
                                                FeatureGroup.None,
                                                inspiration_resource.CreateAddAbilityResource()
                                                );
            DiceType[] dice_type = new DiceType[] { DiceType.D6, DiceType.D8 };
            var dice_count_context = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureListRanks,
                                                                     featureList: new BlueprintFeature[] { inspiration });
            var dice_type_context = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureListRanks,
                                                                    type: AbilityRankType.StatBonus,
                                                                    featureList: new BlueprintFeature[] { inspiration });

            inspiration.AddComponent(Helpers.Create<NewMechanics.AddRandomBonusOnSkillCheckAndConsumeResource>(a =>
               {
                   a.stats = new StatType[] { StatType.SkillKnowledgeArcana, StatType.SkillKnowledgeWorld, StatType.SkillLoreNature, StatType.SkillLoreReligion };
                   a.resource = null;
                   a.dices = dice_type;
                   a.dice_count = Helpers.CreateContextValue(AbilityRankType.Default);
                   a.dice_type = Helpers.CreateContextValue(AbilityRankType.StatBonus);
               }));
            inspiration.AddComponents(dice_count_context, dice_type_context);


            ability_checks_inspiration_buff = Helpers.CreateBuff("InvestigatorInspirationAbilityChecksBuff",
                                                                 "Inspiration: Ability Checks",
                                                                 description,
                                                                 "",
                                                                 Helpers.GetIcon("c7773d1b408fea24dbbb0f7bf3eb864e"), //transmutation strength
                                                                 null,
                                                                 Helpers.Create<NewMechanics.AddRandomBonusOnSkillCheckAndConsumeResource>(a =>
                                                                 {
                                                                     a.stats = new StatType[] { StatType.Strength, StatType.Dexterity, StatType.Constitution,
                                                                                                StatType.Intelligence, StatType.Wisdom, StatType.Charisma };
                                                                     a.resource = inspiration_resource;
                                                                     a.amount = 1;
                                                                     a.dices = dice_type;
                                                                     a.dice_count = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                     a.dice_type = Helpers.CreateContextValue(AbilityRankType.StatBonus);
                                                                 }));
            inspiration_buffs.Add(ability_checks_inspiration_buff);

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
                                              Helpers.Create<NewMechanics.AddRandomBonusOnSkillCheckAndConsumeResource>(a =>
                                                {
                                                    a.stats = new StatType[] { s.Item1 };
                                                    a.resource = inspiration_resource;
                                                    a.amount = 1;
                                                    a.dices = dice_type;
                                                    a.dice_count = Helpers.CreateContextValue(AbilityRankType.Default);
                                                    a.dice_type = Helpers.CreateContextValue(AbilityRankType.StatBonus);
                                                })
                                                );
                skill_inspiration_buffs.Add(s.Item1, buff);
                inspiration_buffs.Add(buff);
            }


            persuation_inspiration_buff = Helpers.CreateBuff("InvestigatorInspirationPersuationBuff",
                                                     "Inspiration: Persuation",
                                                     description,
                                                     "",
                                                     Helpers.GetIcon("1621be43793c5bb43be55493e9c45924"), //transmutation strength
                                                     null
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
                                                        })
                                                      );
            }
            inspiration_buffs.Add(persuation_inspiration_buff);

            attack_rolls_inspiration_buff = Helpers.CreateBuff("InvestigatorInspirationAttackRollsBuff",
                                                                 "Inspiration: Attack Rolls",
                                                                 description,
                                                                 "",
                                                                 Helpers.GetIcon("90e54424d682d104ab36436bd527af09"), //weapon finesse
                                                                 null,
                                                                 Helpers.Create<NewMechanics.AddRandomBonusOnAttackRollAndConsumeResource>(a =>
                                                                 {
                                                                     a.resource = inspiration_resource;
                                                                     a.amount = 2;
                                                                     a.dices = dice_type;
                                                                     a.dice_count = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                     a.dice_type = Helpers.CreateContextValue(AbilityRankType.StatBonus);
                                                                 }));
            inspiration_buffs.Add(attack_rolls_inspiration_buff);

            saving_throws_inspiration_buff = Helpers.CreateBuff("InvestigatorInspirationSavingThrowsBuff",
                                                     "Inspiration: Saving Throws",
                                                     description,
                                                     "",
                                                     Helpers.GetIcon("175d1577bb6c9a04baf88eec99c66334"), //iron will
                                                     null,
                                                     Helpers.Create<NewMechanics.AddRandomBonusOnSavingThrowAndConsumeResource>(a =>
                                                     {
                                                         a.resource = inspiration_resource;
                                                         a.amount = 2;
                                                         a.dices = dice_type;
                                                         a.dice_count = Helpers.CreateContextValue(AbilityRankType.Default);
                                                         a.dice_type = Helpers.CreateContextValue(AbilityRankType.StatBonus);
                                                     }));
            inspiration_buffs.Add(saving_throws_inspiration_buff);

            foreach (var b in inspiration_buffs)
            {
                var toggle = Helpers.CreateActivatableAbility(b.name + "ToggleAbility",
                                                              b.Name,
                                                              b.Description,
                                                              "",
                                                              b.Icon,
                                                              b,
                                                              AbilityActivationType.Immediately,
                                                              CommandType.Free,
                                                              null,
                                                              Helpers.CreateActivatableResourceLogic(inspiration_resource, ResourceSpendType.Never)
                                                              );
                toggle.DeactivateImmediately = true;
                inspiration.AddComponent(Helpers.CreateAddFact(toggle));
            }
        }


        static void createInvestigatorProficiencies()
        {
            investigator_proficiencies = library.CopyAndAdd<BlueprintFeature>("33e2a7e4ad9daa54eaf808e1483bb43c", "InvestigatorProficiencies", "");
            investigator_proficiencies.SetNameDescription("Investigator Proficiencies",
                                                          "Ivestigators are proficient with all simple weapons, plus the hand crossbow, rapier, sap, short sword, and shortbow. They are proficient with light armor, but not with shields."
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
            var icon = Helpers.GetIcon("b96d810ceb1708b4e895b695ddbb1813");

            var studied_target_cooldown = Helpers.CreateBuff("InvestigatorStudiedTargetCooldownBuff",
                                                             "Investigator Studied Target Cooldown",
                                                              "With a keen eye and calculating mind, an investigator can assess the mettle of his opponent to take advantage of gaps in talent and training. At 4th level, an investigator can use a move action to study a single enemy that he can see. Upon doing so, he adds 1/2 his investigator level as an insight bonus on melee attack rolls and as a bonus on damage rolls against the creature. This effect lasts for a number of rounds equal to his Intelligence modifier (minimum 1) or until he deals damage with a studied strike, whichever comes first. The bonus on damage rolls is precision damage, and is not multiplied on a critical hit.\n"
                                                              + "An investigator can only have one target of studied combat at a time, and once a creature has become the target of an investigator’s studied combat, he cannot become the target of the same investigator’s studied combat again for 24 hours unless the investigator expends one use of inspiration when taking the move action to use this ability.",
                                                              "",
                                                              icon,
                                                              null);
            var apply_cooldown = Common.createContextActionApplyBuff(studied_target_cooldown, Helpers.CreateContextDuration(1, DurationRate.Days), dispellable: false);
            studied_target_buff = Helpers.CreateBuff("InvestigatorStudiedTargetBuff",
                                                     "Investigator Studied Target",
                                                     studied_target_cooldown.Description,
                                                     "",
                                                     icon,
                                                     null,
                                                     Helpers.CreateAddFactContextActions(deactivated: apply_cooldown),
                                                     Helpers.Create<UniqueBuff>()
                                                     );
            var apply_studied = Common.createContextActionApplyBuff(studied_target_cooldown, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);
            studied_combat_ability = Helpers.CreateAbility("StudiedCombatInvestigatorAbility",
                                                           "Studied Combat (Ignore Cooldown)",
                                                           studied_target_buff.Description,
                                                           "",
                                                           icon,
                                                           AbilityType.Extraordinary,
                                                           CommandType.Standard,
                                                           AbilityRange.Medium,
                                                           "1 round/Intelligence modifier",
                                                           "",
                                                           Helpers.CreateRunActions(apply_studied),
                                                           Common.createAbilityTargetHasFact(true, studied_target_cooldown)
                                                           );
            studied_combat_ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);

            studied_combat_ability_ignore_cooldown = library.CopyAndAdd<BlueprintAbility>(studied_combat_ability.AssetGuid, "StudiedCombatIgnoreCooldownInvestigatorAbility", "");
            studied_combat_ability_ignore_cooldown.AddComponent(inspiration_resource.CreateResourceLogic());


            studied_combat = Common.AbilityToFeature(studied_combat_ability, false);
            studied_combat.AddComponent(Helpers.CreateAddFact(studied_combat_ability_ignore_cooldown));
            studied_combat.AddComponents(Helpers.Create<NewMechanics.AttackBonusAgainstFactsOwner>(a =>
                                                                                                    {
                                                                                                        a.Bonus = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                                                        a.attack_types = new AttackType[] { AttackType.Melee, AttackType.Touch };
                                                                                                        a.Descriptor = ModifierDescriptor.Insight;
                                                                                                        a.only_from_caster = true;
                                                                                                        a.CheckedFacts = new BlueprintUnitFact[] { studied_target_buff };
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
                                          Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(Common.createContextActionRemoveBuffFromCaster(studied_target_buff)),
                                                                                           check_weapon_range_type: true, wait_for_attack_to_resolve: true),
                                          Helpers.Create<NewMechanics.DamageBonusPrecisionAgainstFactOwner>(a =>
                                                                                  {
                                                                                      a.bonus = Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.Default), 0);
                                                                                      a.attack_types = new AttackType[] { AttackType.Melee, AttackType.Touch };
                                                                                      a.only_from_caster = true;
                                                                                      a.checked_fact = studied_target_buff;
                                                                                  }),
                                          Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getInvestigatorArray(),
                                                                                     progression: ContextRankProgression.StartPlusDivStep, 
                                                                                     startLevel: 4, stepLevel: 2)
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
