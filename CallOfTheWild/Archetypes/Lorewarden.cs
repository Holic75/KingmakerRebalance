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
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Shields;
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
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild.Archetypes
{
    class LoreWarden
    {
        static public BlueprintArchetype archetype;

        static public BlueprintFeature scholastic;
        static public BlueprintFeature expertise;
        static public BlueprintFeatureSelection sword_secret;
        static public BlueprintFeature exploit_weakness;
        static public BlueprintFeature hairs_breadth;
        static public BlueprintFeature know_the_enemy;
        static public BlueprintFeature maneuver_training;
        static public BlueprintFeature swift_assesment;


        static BlueprintCharacterClass fighter_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");

        static LibraryScriptableObject library => Main.library;

        static public void create()
        {
            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "LoreWardenArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Lore Warden");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Quick wits and deceptive techniques can often succeed where brute force might not. A lore warden is the consummate warrior-scholar of the Explorer’s Society, outsmarting her competition even when locking blades with powerful foes. Outside of Combat, a lore warden’s extensive education also helps her document the past and survive great danger.");
            });
            Helpers.SetField(archetype, "m_ParentClass", fighter_class);
            library.AddAsset(archetype, "");

            createScholastic();
            createExpertise();            
            createSwordSecret();
            
            var fighter_proficiency = library.Get<BlueprintFeature>("a23591cc77086494ba20880f87e73970");
            var bravery = library.Get<BlueprintFeature>("f6388946f9f472f4585591b80e9f2452");
            var armor_training = library.Get<BlueprintFeature>("3c380607706f209499d951b29d3c44f3");
            var armor_mastery = library.Get<BlueprintFeature>("ae177f17cfb45264291d4d7c2cb64671");
            var fighter_feat = library.Get<BlueprintFeatureSelection>("41c8486641f7d6d4283ca9dae4147a9f");

            var advanced_armor_training = AdvancedFighterOptions.advanced_armor_training ?? armor_training;

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, fighter_proficiency),
                                                          Helpers.LevelEntry(2, bravery, fighter_feat),
                                                          Helpers.LevelEntry(3, armor_training),
                                                          Helpers.LevelEntry(6, bravery),
                                                          Helpers.LevelEntry(7, advanced_armor_training),
                                                          Helpers.LevelEntry(10, bravery),
                                                          Helpers.LevelEntry(11, advanced_armor_training),
                                                          Helpers.LevelEntry(14, bravery),
                                                          Helpers.LevelEntry(15, advanced_armor_training),
                                                          Helpers.LevelEntry(18, bravery),
                                                          Helpers.LevelEntry(19, armor_mastery)
                                                       };

            archetype.AddFeatures = new LevelEntry[] {  Helpers.LevelEntry(1, scholastic),
                                                          Helpers.LevelEntry(2, expertise),
                                                          Helpers.LevelEntry(3, sword_secret),
                                                          Helpers.LevelEntry(7, sword_secret),
                                                          Helpers.LevelEntry(11, sword_secret),
                                                          Helpers.LevelEntry(15, sword_secret),
                                                          Helpers.LevelEntry(19, sword_secret)
                                                       };
            archetype.ReplaceClassSkills = true;
            archetype.ClassSkills = fighter_class.ClassSkills.AddToArray(StatType.SkillLoreReligion, StatType.SkillKnowledgeArcana);
            archetype.AddSkillPoints = 1;
            archetype.ReplaceStartingEquipment = true;
            archetype.StartingItems = new Kingmaker.Blueprints.Items.BlueprintItem[]
            {
                library.Get<BlueprintItemWeapon>("2fff2921851568a4d80ed52f76cccdb6"), //great sword
                library.Get<BlueprintItemWeapon>("201f6150321e09048bd59e9b7f558cb0"), //longbow 
                library.Get<BlueprintItemArmor>("c65f6fc979d5556489b20e478189cbdd"), //chain short
                library.Get<BlueprintItemEquipmentUsable>("d52566ae8cbe8dc4dae977ef51c27d91"), //potion of cure light wounds
                library.Get<BlueprintItemEquipmentUsable>("d52566ae8cbe8dc4dae977ef51c27d91"), //potion of cure light wounds
                library.Get<BlueprintItemEquipmentUsable>("d52566ae8cbe8dc4dae977ef51c27d91") //potion of cure light wounds
            };

            fighter_class.Progression.UIDeterminatorsGroup = fighter_class.Progression.UIDeterminatorsGroup.AddToArray(scholastic);
            fighter_class.Progression.UIGroups = fighter_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(expertise, sword_secret));
            fighter_class.Archetypes = fighter_class.Archetypes.AddToArray(archetype);
        }


        static void createScholastic()
        {
            scholastic = Helpers.CreateFeature("ScholasticLoreWardenFeature",
                                               "Scholastic",
                                               "A lore warden gains 1 additional skill rank each level. This ability replaces the fighter’s proficiency with medium armor, heavy armor, and shields.",
                                               "",
                                               Helpers.GetIcon("d316d3d94d20c674db2c24d7de96f6a7"),
                                               FeatureGroup.None,
                                               Helpers.CreateAddFacts(library.Get<BlueprintFeature>("6d3728d4e9c9898458fe5e9532951132"), //light armor
                                                                      library.Get<BlueprintFeature>("e70ecf1ed95ca2f40b754f1adb22bbdd"), //simple weapon proficiency
                                                                      library.Get<BlueprintFeature>("203992ef5b35c864390b4e4a1e200629") //martial weapon proficiency
                                                                      )
                                              );
        }


        static void createExpertise()
        {
            var combat_expertise = library.Get<BlueprintFeature>("4c44724ffa8844f4d9bedb5bb27d144a");
            expertise = Helpers.CreateFeature("ExpertiseLoreWardenFeature",
                                               "Expertise",
                                               "At 2nd level, a lore warden gains Combat Expertise as a bonus feat, even if he would not normally qualify for this feat.",
                                               "",
                                               combat_expertise.Icon,
                                               FeatureGroup.None,
                                               Helpers.CreateAddFact(combat_expertise)
                                              );
        }


        static void createSwordSecret()
        {       
            sword_secret = Helpers.CreateFeatureSelection("SwordsSecretLoreWardenFeature",
                                                           "Swords Secret",
                                                           "A lore warden learns specialized techniques that help her to quickly analyze and defeat her foes. At 3rd level, a lore warden gains one swords secret, and she gains an additional swords secret for every 4 fighter levels gained after 3rd. Except where noted, a lore warden cannot select the same swords secret more than once.",
                                                           "",
                                                           null,
                                                           FeatureGroup.None
                                                          );
            createExploitWeakness();
            createHairsBreadth();
            createKnowTheEnemyAndSwiftAssesment();
            createManeuverTraining();

            sword_secret.AllFeatures = new BlueprintFeature[] { exploit_weakness, hairs_breadth, maneuver_training, know_the_enemy, swift_assesment };
        }


        static void createKnowTheEnemyAndSwiftAssesment()
        {
            var icon = Helpers.GetIcon("ee0b69e90bac14446a4cf9a050f87f2e"); //true seeing

            var buff = Helpers.CreateBuff("KnowThyEnemyBuff",
                                          "Know Thy Enemy",
                                          "When the lore warden succeeds at a Knowledge check to identify a creature’s abilities and weaknesses, she can also use a standard action to grant herself a +2 insight bonus on all attack and weapon damage rolls made against that enemy. This bonus lasts for a number of rounds equal to half her class level (minimum 2 rounds), or until the lore warden uses this ability against a different creature. At 11th level, she also gains a +2 bonus to her AC against the creature when using this ability. At 19th level, the insight bonus increases to +3.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.Create<NewMechanics.ACBonusAgainstTargetIfHasFact>(a =>
                                                                                                       {
                                                                                                           a.CheckCaster = true;
                                                                                                           a.Descriptor = ModifierDescriptor.Insight;
                                                                                                           a.Value = Helpers.CreateContextValue(AbilityRankType.StatBonus);
                                                                                                       }
                                                                                                      ),
                                          Helpers.Create<AttackBonusAgainstTarget>(a =>
                                                                                 {
                                                                                     a.CheckCaster = true;
                                                                                     a.Value = Helpers.CreateContextValue(AbilityRankType.DamageBonus);
                                                                                 }),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { fighter_class },
                                                                          progression: ContextRankProgression.Custom, type: AbilityRankType.DamageBonus,
                                                                          customProgression: new (int, int)[] { (18, 2), (20, 3) }),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { fighter_class },
                                                                          type: AbilityRankType.StatBonus,
                                                                          progression: ContextRankProgression.Custom,
                                                                          customProgression: new (int, int)[] { (10, 0), (18, 2), (20, 3) })
                                         );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);

            var ability = Helpers.CreateAbility("KnowThyEnemyAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Extraordinary,
                                                CommandType.Standard,
                                                AbilityRange.Medium,
                                                "1 round/2 levels",
                                                "",
                                                Helpers.CreateRunActions(Helpers.Create<NewMechanics.MonsterLore.ContextMonsterLoreCheck>(c =>
                                                                                                                                        {
                                                                                                                                            c.value = 0;
                                                                                                                                            c.action_on_success = Helpers.CreateActionList(apply_buff);
                                                                                                                                        })
                                                                        ),
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] {fighter_class},
                                                                                progression: ContextRankProgression.Div2, min: 2),
                                                Helpers.Create<NewMechanics.MonsterLore.AbilityTargetCanBeInspected>(a => a.allow_reinspect = true)
                                                );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(works_on_allies: true);

            know_the_enemy = Common.AbilityToFeature(ability, false);


            var swift_ability = library.CopyAndAdd(ability, "Swift" + ability.name, "");
            swift_ability.ActionType = CommandType.Swift;
            swift_ability.SetName(buff.Name + " (Swift)");

            swift_assesment = Helpers.CreateFeature("SwiftAssesmentLoreWardenFeature",
                                                    "Swift Assessment",
                                                    "The lore warden can now use her know thy enemy swords secret as a move action. At 15th level, she can use this ability as a swift action. She must have the know thy enemy swords secret before choosing this swords secret.",
                                                    "",
                                                    null,
                                                    FeatureGroup.None,
                                                    Helpers.Create<TurnActionMechanics.MoveActionAbilityUse>(m => m.abilities = new BlueprintAbility[] { ability }),
                                                    Helpers.CreateAddFeatureOnClassLevel(Common.AbilityToFeature(swift_ability), 15, new BlueprintCharacterClass[] { fighter_class }),
                                                    Helpers.PrerequisiteFeature(know_the_enemy)
                                                    );
        }


        static void createHairsBreadth()
        {
            hairs_breadth = Helpers.CreateFeature("HairsBreadthLoreWardenFeature",
                                         "Hair’s Breadth",
                                         "Lore Warden adds one-third her mobility skill as an AC bonus against crtical hits.",
                                         "",
                                         Helpers.GetIcon("4be5757b85af47545a5789f1d03abda9"),
                                         FeatureGroup.None,
                                         Helpers.Create<CriticalConfirmationACBonus>(c => c.Value = Helpers.CreateContextValue(AbilityRankType.Default)),
                                         Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, stat: StatType.SkillMobility,
                                                                         progression: ContextRankProgression.DivStep, stepLevel: 3),
                                         Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.SkillMobility)
                                         );
        }


        static void createManeuverTraining()
        {
            maneuver_training = Helpers.CreateFeature("ManeuverTrainingLoreWardenFeature",
                                         "Maneuver Training",
                                         "At 3rd level, a lore warden gains a +1 bonus on combat maneuver checks and a +1 bonus to her CMD. At 7th level and every 4 levels thereafter these bonuses increase by 1.",
                                         "",
                                         Helpers.GetIcon("4be5757b85af47545a5789f1d03abda9"),
                                         FeatureGroup.None,
                                         Helpers.CreateAddContextStatBonus(StatType.AdditionalCMB, ModifierDescriptor.UntypedStackable),
                                         Helpers.CreateAddContextStatBonus(StatType.AdditionalCMD, ModifierDescriptor.UntypedStackable),
                                         Helpers.Create<CombatManeuverMechanics.ContextManeuverDefenceBonus>(c => c.Bonus = Helpers.CreateContextValue(AbilityRankType.Default)),
                                         Helpers.Create<CombatManeuverMechanics.CombatManeuverBonus>(c => c.Value = Helpers.CreateContextValue(AbilityRankType.Default)),
                                         Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { fighter_class },
                                                                         progression: ContextRankProgression.DelayedStartPlusDivStep, stepLevel: 4, startLevel: 3)
                                         );
            maneuver_training.ReapplyOnLevelUp = true;
        }


        static void createExploitWeakness()
        {
            exploit_weakness = Helpers.CreateFeature("ExploitWeaknessLoreWardenFeature",
                                                     "Exploit Weakness",
                                                     "The lore warden adds one-third her class level on attack rolls to confirm critical hits. At 11th level, whenever she confirms a critical hit, her weapon attacks ignore the first 5 points of damage reduction or hardness the target has until the end of her next turn. At 19th level, the lore warden can automatically confirm a critical hit once per round when she threatens a critical hit.",
                                                     "",
                                                     NewSpells.deadly_juggernaut.Icon,
                                                     FeatureGroup.None,
                                                     Helpers.Create<CriticalConfirmationBonus>(c => c.Value = Helpers.CreateContextValue(AbilityRankType.Default)),
                                                     Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { fighter_class },
                                                                                     progression: ContextRankProgression.DivStep, stepLevel: 3)
                                                     );

            var buff = Helpers.CreateBuff("ExploitWeakness11LoreWardenBuff",
                                                   "Exploit Weakness: Reduced DR",
                                                   exploit_weakness.Description,
                                                   "",
                                                   exploit_weakness.Icon,
                                                   null,
                                                   Helpers.Create<NewMechanics.ReduceDRFromCaster>(r => r.reduction_reduction = 5)
                                                   );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1), dispellable: false);
            var feature11 = Helpers.CreateFeature("ExploitWeakness11LoreWardenFeature",
                                                     "Exploit Weakness",
                                                     "",
                                                     "",
                                                     null,
                                                     FeatureGroup.None,
                                                     Common.createAddInitiatorAttackRollTrigger2(Helpers.CreateActionList(apply_buff), critical_hit: true)
                                                     );
            feature11.HideInCharacterSheetAndLevelUp = true;

            var cooldown_buff = Helpers.CreateBuff("ExploitWeakness19LoreWardenCooldownBuff",
                                       "Exploit Weakness: Critical Hit Automatic Confirmation Cooldown",
                                       exploit_weakness.Description,
                                       "",
                                       exploit_weakness.Icon,
                                       null
                                       );
            cooldown_buff.Stacking = StackingType.Replace;
            var apply_cooldown_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1), dispellable: false);
            var apply_cooldown = Helpers.CreateConditional(Common.createContextConditionHasFact(cooldown_buff), null, apply_cooldown_buff);
            var feature19 = Helpers.CreateFeature("ExploitWeakness19LoreWardenFeature",
                                                    "Exploit Weakness",
                                                    "",
                                                    "",
                                                    null,
                                                    FeatureGroup.None,
                                                    Common.createAddInitiatorAttackRollTrigger2(Helpers.CreateActionList(apply_cooldown), critical_hit: true, on_initiator: true),
                                                    Helpers.Create<NewMechanics.CritAutoconfirmIfHasNoFact>(c => c.fact = cooldown_buff)
                                                    );
            feature19.HideInCharacterSheetAndLevelUp = true;

            exploit_weakness.AddComponents(Helpers.CreateAddFeatureOnClassLevel(feature11, 11, new BlueprintCharacterClass[] { fighter_class }),
                                           Helpers.CreateAddFeatureOnClassLevel(feature19, 19, new BlueprintCharacterClass[] { fighter_class })
                                           );
            exploit_weakness.ReapplyOnLevelUp = true;
        }
    }
}
