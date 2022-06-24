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
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Items.Equipment;
using Newtonsoft.Json;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.PubSubSystem;

namespace CallOfTheWild
{
    public class Brawler
    {
        static internal LibraryScriptableObject library => Main.library;
        static internal bool test_mode = false;
        static public BlueprintCharacterClass brawler_class;
        static public BlueprintProgression brawler_progression;

        static public BlueprintFeatureSelection combat_feat;
        static public BlueprintFeature martial_training;
        static public BlueprintFeature brawlers_cunning;
        static public BlueprintFeature unarmed_strike;
        static public BlueprintFeature brawlers_flurry;
        static public BlueprintFeature brawlers_flurry11;
        static public BlueprintFeature ac_bonus;
        static public BlueprintFeature brawler_proficiencies;
        static public BlueprintFeatureSelection[] maneuver_training = new BlueprintFeatureSelection[5];
        static public BlueprintAbilityResource knockout_resource;
        static public BlueprintFeature knockout;
        static public BlueprintFeature brawlers_strike_magic;
        static public BlueprintFeature brawlers_strike_cold_iron_and_silver;
        static public BlueprintFeatureSelection brawlers_strike_alignment;
        static public BlueprintFeature brawlers_strike_adamantine;
        static public BlueprintFeature close_weapon_mastery;
        static public BlueprintFeature awesome_blow;
        static public BlueprintAbility awesome_blow_ability;
        static public BlueprintFeature awesome_blow_improved;
        static public BlueprintFeature perfect_warrior;

        static public BlueprintArchetype exemplar;
        static public BlueprintFeature call_to_arms;
        static public BlueprintFeature inspire_courage;
        static public BlueprintFeature inspire_greatness;
        static public BlueprintFeature inspire_heroics;
        static public BlueprintFeature field_instruction;
        static public BlueprintAbility field_instruction_ability;
        static public BlueprintFeature performance_resource_feature;
        static public BlueprintFeature inspiring_prowess;

        static public BlueprintArchetype mutagenic_mauler;
        static public BlueprintFeature mutagen;
        static public BlueprintFeature mutagen_damage_bonus;
        static public BlueprintFeatureSelection discovery;
        static public BlueprintFeature greater_mutagen;
        static public BlueprintFeature beastmorph_speed;
        static public BlueprintFeature beastmorph_blindsense;

        static public BlueprintArchetype wild_child;
        static public BlueprintFeatureSelection animal_companion;
        static public BlueprintFeatureSelection hunter_trick_selection;

        static public BlueprintArchetype snakebite_striker;
        static public BlueprintFeature[] snake_feint = new BlueprintFeature[3];
        static public BlueprintFeature   opportunist;

        static public BlueprintArchetype steel_breaker;
        static public BlueprintFeature exploit_weakness;
        static public BlueprintFeature sunder_training;
        static public BlueprintFeature disarm_training;

        static public BlueprintArchetype venomfist;
        static public BlueprintFeature[] venomous_strike = new BlueprintFeature[5];


        internal static void createBrawlerClass()
        {
            Main.logger.Log("Brawler class test mode: " + test_mode.ToString());
            var monk_class = library.Get<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");
            var fighter_class = library.Get<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");

            brawler_class = Helpers.Create<BlueprintCharacterClass>();
            brawler_class.name = "BrawlerClass";
            library.AddAsset(brawler_class, "");

            brawler_class.LocalizedName = Helpers.CreateString("Brawler.Name", "Brawler");
            brawler_class.LocalizedDescription = Helpers.CreateString("Brawler.Description",
                                                                         "Deadly even with nothing in her hands, a brawler eschews using the fighter’s heavy armor and the monk’s mysticism, focusing instead on perfecting many styles of brutal unarmed combat. Versatile, agile, and able to adapt to most enemy attacks, a brawler’s body is a powerful weapon.\n"
                                                                         + "Role: Brawlers are maneuverable and well suited for creating flanking situations or dealing with lightly armored enemies, as well as quickly adapting to a rapidly changing battlefield."
                                                                         );
            brawler_class.m_Icon = monk_class.Icon;
            brawler_class.SkillPoints = monk_class.SkillPoints;
            brawler_class.HitDie = DiceType.D10;
            brawler_class.BaseAttackBonus = monk_class.BaseAttackBonus;
            brawler_class.FortitudeSave = monk_class.FortitudeSave;
            brawler_class.ReflexSave = monk_class.ReflexSave;
            brawler_class.WillSave = monk_class.WillSave;
            brawler_class.Spellbook = null;
            brawler_class.ClassSkills = monk_class.ClassSkills.RemoveFromArray(StatType.SkillLoreReligion).RemoveFromArray(StatType.SkillStealth);
            brawler_class.IsDivineCaster = true;
            brawler_class.IsArcaneCaster = false;
            brawler_class.StartingGold = monk_class.StartingGold;
            brawler_class.PrimaryColor = fighter_class.PrimaryColor;
            brawler_class.SecondaryColor = fighter_class.SecondaryColor;
            brawler_class.RecommendedAttributes = new StatType[] {StatType.Strength, StatType.Dexterity, StatType.Constitution };
            brawler_class.NotRecommendedAttributes = monk_class.NotRecommendedAttributes;
            brawler_class.EquipmentEntities = monk_class.EquipmentEntities;
            brawler_class.MaleEquipmentEntities = monk_class.MaleEquipmentEntities;
            brawler_class.FemaleEquipmentEntities = monk_class.FemaleEquipmentEntities;
            brawler_class.ComponentsArray = monk_class.ComponentsArray.ToArray().RemoveFromArray(monk_class.GetComponent<PrerequisiteAlignment>());
            brawler_class.StartingItems = new Kingmaker.Blueprints.Items.BlueprintItem[]
            {
                library.Get<BlueprintItemWeapon>("43ff56218554d8547840e7659816db5e"), //punching dagger
                library.Get<BlueprintItemShield>("f4cef3ba1a15b0f4fa7fd66b602ff32b"), //heavy shield
                library.Get<BlueprintItemWeapon>("ada85dae8d12eda4bbe6747bb8b5883c"), //quarterstaff
                library.Get<BlueprintItemArmor>("afbe88d27a0eb544583e00fa78ffb2c7"), //studded leather
                library.Get<BlueprintItemEquipmentUsable>("d52566ae8cbe8dc4dae977ef51c27d91"), //cure light wounds potion
                library.Get<BlueprintItemEquipmentUsable>("d52566ae8cbe8dc4dae977ef51c27d91"), //cure light wounds potion
            };
            createBrawlerProgression();
            brawler_class.Progression = brawler_progression;

            createExemplar();
            createMutagenicMauler();
            createWildChild();
            createSnakebiteStriker();
            createSteelBreaker();
            createVenomfist();
            brawler_class.Archetypes = new BlueprintArchetype[] {exemplar, mutagenic_mauler, wild_child, snakebite_striker, steel_breaker, venomfist };
            Helpers.RegisterClass(brawler_class);
        }


        static void createVenomfist()
        {
            venomfist = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "VenomfistBrawler";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Venomfist");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Thanks to alchemical experiments and rigorous study of venomous creatures, a venomfist has toxic unarmed strikes.");
            });
            Helpers.SetField(venomfist, "m_ParentClass", brawler_class);
            library.AddAsset(venomfist, "");

            createVenomousStrike();

            venomfist.RemoveFeatures = new LevelEntry[]
            {
                Helpers.LevelEntry(1, unarmed_strike),
                Helpers.LevelEntry(4, knockout),
                Helpers.LevelEntry(5, close_weapon_mastery),
            };

            venomfist.AddFeatures = new LevelEntry[] {Helpers.LevelEntry(1, venomous_strike[0]),
                                                              Helpers.LevelEntry(4, venomous_strike[1]),
                                                              Helpers.LevelEntry(5, venomous_strike[2]),
                                                              Helpers.LevelEntry(10, venomous_strike[3]),
                                                              Helpers.LevelEntry(16, venomous_strike[4]),
                                                         };

            brawler_progression.UIGroups = brawler_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(venomous_strike));
        }


        static void createVenomousStrike()
        {
            var fatigued = library.Get<BlueprintBuff>("e6f2fc5d73d88064583cb828801212f4");
            var shaken = library.Get<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220");
            var sickened = library.Get<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323");
            var blinded = library.Get<BlueprintBuff>("0ec36e7596a4928489d2049e1e1c76a7");
            var exhausted = library.Get<BlueprintBuff>("46d1b9cc3d0fd36469a471b047d773a2");
            var staggered = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");
            var dazed = Common.dazed_non_mind_affecting;
            var stunned = library.Get<BlueprintBuff>("09d39b38bb7c6014394b6daced9bacd3");

            var poison_stats = new StatType[] { StatType.Unknown, StatType.Constitution, StatType.Dexterity, StatType.Strength };
            
            List<BlueprintAbility> secondary_effect_toggle = new List<BlueprintAbility>();
            List<BlueprintAbility> poison_damage_type_toggle = new List<BlueprintAbility>();

            var secondary_effect_buff = Helpers.CreateBuff("VenomfistSecondaryEffectBuff",
                                                           "",
                                                           "",
                                                           "",
                                                           null,
                                                           null,
                                                           Helpers.CreateSpellDescriptor(SpellDescriptor.Poison));
            secondary_effect_buff.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.Harmful);


            var immune_to_secondary_condition_buff = Helpers.CreateBuff("ImmuneToSecondaryVenomfistEffect",
                                                            "Secondary Venom Fist Effect Immunity",
                                                            "The creature is immune to the secondary effects of the venomfist’s poison for 24 hours.",
                                                            "",
                                                            Helpers.GetIcon("b48b4c5ffb4eab0469feba27fc86a023"), //delay poison
                                                            null
                                                            );
            immune_to_secondary_condition_buff.SetBuffFlags(BuffFlags.RemoveOnRest);

            var apply_secondary_effect = Helpers.CreateConditional(Common.createContextConditionHasFact(immune_to_secondary_condition_buff, has: false),
                                                                   Common.createContextActionApplyChildBuff(secondary_effect_buff)
                                                                  );

            var apply_immunity_to_secondary_effect = Helpers.CreateConditional(Common.createContextConditionHasFact(immune_to_secondary_condition_buff, has: false),
                                                                               Common.createContextActionApplyBuff(immune_to_secondary_condition_buff, 
                                                                                                                    Helpers.CreateContextDuration(1, DurationRate.Days),
                                                                                                                    dispellable: false)
                                                                               );

            DiceFormula[] diceFormulas = new DiceFormula[] {new DiceFormula(1, DiceType.D3),
                                                            new DiceFormula(1, DiceType.D4),
                                                            new DiceFormula(1, DiceType.D6),
                                                            new DiceFormula(1, DiceType.D8),
                                                            new DiceFormula(1, DiceType.D10),
                                                            new DiceFormula(2, DiceType.D6)};

            venomous_strike[0] = Helpers.CreateFeature("VenomousStrike1Feature",
                                                       "Venomous Strike",
                                                       "A venomfist’s unarmed strikes deal damage as a creature two size categories smaller (1d3 at first level for Medium venomfists). If she hits with her first unarmed strike in a round, the target must succeed at a Fortitude saving throw (DC = 10 + half the venomfist’s brawler level + her Constitution modifier) or take an additional amount of damage equal to the venomfist’s Constitution modifier. The venomfist is immune to this toxin.",
                                                       "",
                                                       Helpers.GetIcon("c7773d1b408fea24dbbb0f7bf3eb864e"), //physical enchantment strength
                                                       FeatureGroup.None,
                                                       unarmed_strike.GetComponent<NewMechanics.ContextWeaponDamageDiceReplacementWeaponCategory>().CreateCopy(c =>
                                                       {
                                                           c.dice_formulas = diceFormulas;
                                                       }
                                                       ),
                                                       unarmed_strike.GetComponent<ContextRankConfig>(),
                                                       Helpers.CreateSpellDescriptor(SpellDescriptor.Poison)
                                                       );

            venomous_strike[1] = Helpers.CreateFeature("VenomousStrike4Feature",
                                                       "Venomous Strike",
                                                       "At 4th level, a target that fails this save must succeed at a second saving throw 1 round later or take the same amount of damage again. This effect repeats as long as the target continues to fail its saving throws, to a maximum number of rounds equal to 1 plus 1 additional round for every 4 brawler levels the venomfist has. Unlike other poisons, multiple doses of a venomfist’s poison never stack; the more recent poison effect replaces the older one.",
                                                       "",
                                                       Helpers.GetIcon("c7773d1b408fea24dbbb0f7bf3eb864e"), //physical enchantment strength
                                                       FeatureGroup.None
                                                       );

            venomous_strike[2] = Helpers.CreateFeature("VenomousStrike5Feature",
                                                       "Venomous Strike",
                                                       "At 5th level, after the venomfist gets 8 hours of rest, she can choose a secondary effect for her venom to impose. She can choose fatigued, shaken, or sickened. A creature that fails its saving throw against her venom also gains the chosen condition until it succeeds at a save against the venom or until the venom’s duration ends. Once a creature succeeds at its save against the poison, it becomes immune to the secondary condition for 24 hours, but the attack still deals the extra damage.",
                                                       "",
                                                       fatigued.Icon,
                                                       FeatureGroup.None
                                                       );

            venomous_strike[3] = Helpers.CreateFeature("VenomousStrike10Feature",
                                                       "Venomous Strike",
                                                       "At 10th level, when the venomfist chooses the condition her venom imposes, she can also cause her venom to deal ability score damage each round instead of hit point damage. She chooses Strength, Dexterity, or Constitution, and her venom deals 1d3 points of ability score damage each round. In addition, she adds blinded, exhausted, and staggered to the list of secondary effects she can choose for her venom.",
                                                       "",
                                                       exhausted.Icon,
                                                       FeatureGroup.None
                                                       );

            venomous_strike[4] = Helpers.CreateFeature("VenomousStrike16Feature",
                                                       "Venomous Strike",
                                                       "At 16th level, the venomfist’s venom is particularly potent. If it fails the initial save, the target must succeed at two consecutive saves before being cured of the venom, though if the first save is successful, the secondary effect ends and the creature is immune to the secondary effects of the venomfist’s poison for 24 hours. In addition, the venomfist adds dazed and stunned to the list of secondary effects she can choose for her venom.",
                                                       "",
                                                       dazed.Icon,
                                                       FeatureGroup.None
                                                       );

            var secondary_effect_buffs = new Dictionary<BlueprintBuff, BlueprintFeature>
            {
                {fatigued, venomous_strike[2]},
                {shaken, venomous_strike[2] },
                {sickened, venomous_strike[2] },
                {blinded, venomous_strike[3] },
                {exhausted, venomous_strike[3] },
                {staggered, venomous_strike[3] },
                {dazed, venomous_strike[4] },
                {stunned, venomous_strike[4] }
            };

            var remove_stat_buffs = Helpers.Create<NewMechanics.ContextActionRemoveBuffs>(c => c.Buffs = new BlueprintBuff[0]);
            var stat_buff_resource = Helpers.CreateAbilityResource("VenomousStrikeStatBuffResource", "", "", "", null);
            stat_buff_resource.SetFixedResource(1);

            venomous_strike[0].AddComponents(Common.createContextCalculateAbilityParamsBasedOnClass(brawler_class, StatType.Constitution),
                                             Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Constitution),
                                             Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                                                                             classes: getBrawlerArray(),
                                                                             progression: ContextRankProgression.OnePlusDivStep,
                                                                             stepLevel: 4,
                                                                             type: AbilityRankType.DamageDice
                                                                             )
                                             );
            foreach (var s in poison_stats)
            {
                string stat_text = s == StatType.Unknown ? "HP" : s.ToString();
                var buff = Helpers.CreateBuff(stat_text + "VenomousStrikeEffectBuff",
                                              "Venomous Strike Effect: " + stat_text + " Damage",
                                              venomous_strike[0].Description,
                                              "",
                                              venomous_strike[0].Icon,
                                              null,
                                              Helpers.Create<PoisonMechanics.BuffPoisonDamage>(p =>
                                              {
                                                  if (s == StatType.Unknown)
                                                  {
                                                      p.ContextValue = Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilityRankType.DamageBonus));
                                                  }
                                                  else
                                                  {
                                                      p.ContextValue = Helpers.CreateContextDiceValue(DiceType.D3, 1, 0);
                                                  }
                                                  p.Stat = s;
                                                  p.SaveType = SavingThrowType.Fortitude;
                                                  p.SuccesfullSaves = Helpers.CreateContextValue(AbilityRankType.StatBonus);
                                                  p.Ticks = Helpers.CreateContextValue(AbilityRankType.DamageDice);
                                                  p.on_successful_save_action = Helpers.CreateActionList(Common.createContextActionRemoveBuffFromCaster(secondary_effect_buff),
                                                                                                         apply_immunity_to_secondary_effect);
                                              }),
                                              Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureList, type: AbilityRankType.StatBonus,
                                                                              progression: ContextRankProgression.BonusValue, stepLevel: 1, featureList: new BlueprintFeature[] { venomous_strike[4] }),
                                              Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, type: AbilityRankType.DamageBonus,
                                                                              stat: StatType.Constitution, min: 0),
                                              Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getBrawlerArray(), type: AbilityRankType.DamageDice,
                                                                              progression: ContextRankProgression.OnePlusDivStep, stepLevel: 4),
                                              Helpers.CreateAddFactContextActions(activated: new GameAction[] { apply_secondary_effect}),
                                              Helpers.CreateSpellDescriptor(SpellDescriptor.Poison)
                                              );
                buff.Stacking = StackingType.Replace;

                var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.DamageDice)), dispellable: false);
                var apply_buff_saved = Helpers.CreateActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateConditionalSaved(apply_immunity_to_secondary_effect, apply_buff));
                var owner_buff = Helpers.CreateBuff(stat_text + "VenomousStrikeBuff",
                                                    buff.Name,
                                                    venomous_strike[0].Description,
                                                    "",
                                                    venomous_strike[0].Icon,
                                                    null
                                                    );

                


                if (s != StatType.Unknown)
                {
                    var check_apply_buff_saved = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(owner_buff), apply_buff_saved);
                    venomous_strike[0].AddComponent(Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(Helpers.CreateActionList(check_apply_buff_saved),
                                                                                                 only_first_hit: true));
                }
                else
                {
                    var check_apply_buff_saved = Helpers.CreateConditional(Helpers.CreateConditionsCheckerOr(Common.createContextConditionCasterHasFact(owner_buff), 
                                                                                                             Common.createContextConditionCasterHasFact(venomous_strike[3], has: false)
                                                                                                             ),
                                                                           apply_buff_saved);
                    venomous_strike[0].AddComponent(Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(Helpers.CreateActionList(check_apply_buff_saved),
                                                                                                 only_first_hit: true));
                }
                owner_buff.SetBuffFlags(BuffFlags.StayOnDeath);
                remove_stat_buffs.Buffs = remove_stat_buffs.Buffs.AddToArray(owner_buff);

                var owner_ability = Helpers.CreateAbility(stat_text + "VenomousStrikeAbility",
                                                          owner_buff.Name,
                                                          owner_buff.Description,
                                                          "",
                                                          owner_buff.Icon,
                                                          AbilityType.Extraordinary,
                                                          CommandType.Standard,
                                                          AbilityRange.Personal,
                                                          "",
                                                          "",
                                                          Helpers.CreateRunActions(remove_stat_buffs,
                                                                                   Common.createContextActionApplyBuff(owner_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true)),
                                                          stat_buff_resource.CreateResourceLogic()
                                                          );
                Common.setAsFullRoundAction(owner_ability);
                owner_ability.setMiscAbilityParametersSelfOnly();
                poison_damage_type_toggle.Add(owner_ability);
            }

            var remove_secondary_effect_buffs = Helpers.Create<NewMechanics.ContextActionRemoveBuffs>(c => c.Buffs = new BlueprintBuff[0]);
            var secondary_effect_resource = Helpers.CreateAbilityResource("VenomousStrikeSecondaryEffectResource", "", "", "", null);
            secondary_effect_resource.SetFixedResource(1);

            foreach (var b in secondary_effect_buffs)
            {
                var owner_buff = Helpers.CreateBuff("VenomousStrike" + b.Key.name,
                                                    "Venomous Strike: " + b.Key.Name,
                                                    venomous_strike[2].Description,
                                                    "",
                                                    b.Key.Icon,
                                                    null
                                                    );
                owner_buff.SetBuffFlags(BuffFlags.StayOnDeath);
                remove_secondary_effect_buffs.Buffs = remove_secondary_effect_buffs.Buffs.AddToArray(owner_buff);

                var owner_ability = Helpers.CreateAbility(b.Key.name + "VenomousStrikeAbility",
                                                          owner_buff.Name,
                                                          owner_buff.Description,
                                                          "",
                                                          owner_buff.Icon,
                                                          AbilityType.Extraordinary,
                                                          CommandType.Standard,
                                                          AbilityRange.Personal,
                                                          "",
                                                          "",
                                                          Helpers.CreateRunActions(remove_secondary_effect_buffs,
                                                                                   Common.createContextActionApplyBuff(owner_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true)),
                                                          secondary_effect_resource.CreateResourceLogic(),
                                                          Common.createContextCalculateAbilityParamsBasedOnClass(brawler_class, StatType.Constitution),
                                                          Common.createAbilityShowIfCasterHasFact(b.Value)
                                                          );
                Common.setAsFullRoundAction(owner_ability);
                owner_ability.setMiscAbilityParametersSelfOnly();
                secondary_effect_toggle.Add(owner_ability);

                Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuffNoRemove(secondary_effect_buff,
                                                                                          Helpers.CreateConditional(Common.createContextConditionCasterHasFact(owner_buff),
                                                                                                                    Common.createContextActionApplyChildBuff(b.Key)
                                                                                                                   )
                                                                                         );
            }

            var secondary_effects_wrapper = Common.createVariantWrapper("VenomousStrikeSecondaryEffectBaseAbility", "", secondary_effect_toggle.ToArray());
            secondary_effects_wrapper.SetNameDescriptionIcon("Venomous Strike Secondary Effect",
                                                             venomous_strike[2].Description,
                                                             Helpers.GetIcon("d797007a142a6c0409a74b064065a15e") //poison
                                                             );

            venomous_strike[2].AddComponents(Helpers.CreateAddFact(secondary_effects_wrapper),
                                            secondary_effect_resource.CreateAddAbilityResource()
                                            );

            var stat_damage_wrapper = Common.createVariantWrapper("VenomousStrikeStatDamageBase", "", poison_damage_type_toggle.ToArray());
            stat_damage_wrapper.SetNameDescriptionIcon("Venomous Strike: Stat Damage",
                                                       "At 10th level, when the venomfist chooses the condition her venom imposes, she can also cause her venom to deal ability score damage each round instead of hit point damage. She chooses Strength, Dexterity, or Constitution, and her venom deals 1d3 points of ability score damage each round.",
                                                       Helpers.GetIcon("fd101fbc4aacf5d48b76a65e3aa5db6d")
                                                       );

            venomous_strike[3].AddComponents(Helpers.CreateAddFact(stat_damage_wrapper),
                                            stat_buff_resource.CreateAddAbilityResource());
        }


        static void createSteelBreaker()
        {
            steel_breaker = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "SteelBreakerBrawler";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Steel-Breaker");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "The steel-breaker studies destruction and practices it as an art form. She knows every defense has a breaking point, and can shatter those defenses with carefully planned strikes.");
            });
            Helpers.SetField(steel_breaker, "m_ParentClass", brawler_class);
            library.AddAsset(steel_breaker, "");

            createExploitWeakness();
            createSunderAndDisarmTraining();

            steel_breaker.RemoveFeatures = new LevelEntry[]
            {
                Helpers.LevelEntry(3, maneuver_training[0]),
                Helpers.LevelEntry(5, brawlers_strike_magic),
                Helpers.LevelEntry(7, maneuver_training[1]),
                Helpers.LevelEntry(9, brawlers_strike_cold_iron_and_silver),
                Helpers.LevelEntry(11, maneuver_training[2]),
                Helpers.LevelEntry(12, brawlers_strike_alignment),
                Helpers.LevelEntry(15, maneuver_training[3]),
                Helpers.LevelEntry(17, brawlers_strike_adamantine),
                Helpers.LevelEntry(19, maneuver_training[4]),
            };

            steel_breaker.AddFeatures = new LevelEntry[] {Helpers.LevelEntry(3, sunder_training),
                                                              Helpers.LevelEntry(5, exploit_weakness),
                                                              Helpers.LevelEntry(7, disarm_training),
                                                         };

            brawler_progression.UIGroups = brawler_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(sunder_training, exploit_weakness, disarm_training));
        }


        static void createExploitWeakness()
        {
            var description = "At 5th level, as a swift action a steel-breaker can observe a creature or object to find its weak point by succeeding at a Wisdom check, adding her brawler level against a DC of 10 + target’s HD. If it succeeds, the steel-breaker gains a +2 bonus on attack rolls until the end of her turn, and any attacks she makes until the end of her turn ignore the creature's  DR.\n"
                               + "A steel-breaker can instead use this ability as a swift action to analyze the movements and expressions of one creature within 30 feet, granting a bonus on Reflex saving throws, as well as a dodge bonus to AC against that opponent equal to 1/2 her brawler level until the start of her next turn.";

            var attack_buff = Helpers.CreateBuff("SteelBreakerExplotWeaknessAttackBuff",
                                                 "Exploit Weakness: Attack Bonus",
                                                 description,
                                                 "",
                                                 Helpers.GetIcon("2c38da66e5a599347ac95b3294acbe00"), //true strike
                                                 null,
                                                 Helpers.Create<IgnoreTargetDR>(i => i.CheckCaster = true),
                                                 Helpers.Create<AttackBonusAgainstTarget>(a => { a.CheckCaster = true; a.Value = 2; })
                                                 );

            var defense_buff = Helpers.CreateBuff("SteelBreakerExplotWeaknessDefenseBuff",
                                     "Exploit Weakness: Defense Bonus",
                                     description,
                                     "",
                                     Helpers.GetIcon("9e1ad5d6f87d19e4d8883d63a6e35568"), //mage armor
                                     null,
                                     Helpers.Create<ACBonusAgainstTarget>(i => { i.CheckCaster = true; i.Value = Helpers.CreateContextValue(AbilityRankType.Default); i.Descriptor = ModifierDescriptor.Dodge; }),
                                     Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getBrawlerArray(), progression: ContextRankProgression.Div2)
                                     );

            exploit_weakness = Helpers.CreateFeature("SteelBreakerExploitWeaknessFeature",
                                                     "Exploit Weakness",
                                                     description,
                                                     "",
                                                     attack_buff.Icon,
                                                     FeatureGroup.None,
                                                     Helpers.Create<NewMechanics.SavingThrowBonusAgainstFactFromCaster>(a =>
                                                     {
                                                         a.Value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                         a.Descriptor = ModifierDescriptor.UntypedStackable;
                                                         a.reflex = true;
                                                         a.fortitude = false;
                                                         a.will = false;
                                                         a.CheckedFact = defense_buff;
                                                     }),
                                                     Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getBrawlerArray(), progression: ContextRankProgression.Div2)
                                                     );

            var buffs = new BlueprintBuff[] { attack_buff, defense_buff };

            foreach (var b in buffs)
            {
                b.Stacking = StackingType.Stack;
                var apply_buff = Common.createContextActionApplyBuff(b, Helpers.CreateContextDuration(1), dispellable: false);
                var check = Helpers.Create<SkillMechanics.ContextActionCasterSkillCheck>(c =>
                {
                    c.Stat = StatType.Wisdom;
                    c.Success = Helpers.CreateActionList(apply_buff);
                    c.bonus = Helpers.CreateContextValue(AbilityRankType.StatBonus);
                });
                var ability = Helpers.CreateAbility(b.name + "Ability",
                                                    b.Name,
                                                    b.Description,
                                                    "",
                                                    b.Icon,
                                                    AbilityType.Extraordinary,
                                                    CommandType.Swift,
                                                    AbilityRange.Close,
                                                    Helpers.roundsPerLevelDuration,
                                                    "",
                                                    Helpers.CreateRunActions(check),
                                                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getBrawlerArray(),
                                                                                    type: AbilityRankType.StatBonus),
                                                    Common.createAbilitySpawnFx("8de64fbe047abc243a9b4715f643739f", position_anchor: AbilitySpawnFxAnchor.None, orientation_anchor: AbilitySpawnFxAnchor.None)
                                                    );
                ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);
                exploit_weakness.AddComponent(Helpers.CreateAddFact(ability));
            }
        }


        static void createSunderAndDisarmTraining()
        {
            sunder_training = library.CopyAndAdd(maneuver_training[0].AllFeatures[3], "SteelBreakerSunderTraining", "");
            sunder_training.SetNameDescription("Sunder Training",
                                               "At 3rd level, a steel-breaker receives additional training in sunder combat maneuvers. She gains a +2 bonus when attempting a sunder combat maneuver checks and a +2 bonus to her CMD when defending against this maneuver.\n"
                                               + "At 7th, 11th, 15th, and 19th levels, these bonuses increase by 1.");
            sunder_training.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_StartLevel", -1));


            disarm_training = library.CopyAndAdd(maneuver_training[0].AllFeatures[1], "SteelBreakerDisarmTraining", "");
            disarm_training.SetNameDescription("Disarm Training",
                                               "At 7th level, a steel-breaker receives additional training in disarm combat maneuvers. She gains a +2 bonus when attempting a disarm combat maneuver checks and a +2 bonus to her CMD when defending against this maneuver.\n"
                                               + "At 11th, 15th, and 19th levels, these bonuses increase by 1.");
        }


        static void createSnakebiteStriker()
        {
            snakebite_striker = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "SnakebiteStriker";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Snakebite Striker");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "With her lightning quickness and guile, a snakebite striker keeps her foes’ attention focused on her, because any one of her feints might be an actual attack. By giving up some of a brawler’s versatility, she increases her damage potential and exposes opponents to deadly and unexpected strikes.");
            });
            Helpers.SetField(snakebite_striker, "m_ParentClass", brawler_class);
            library.AddAsset(snakebite_striker, "");

            createSnakeFeint();
            createOpportunist();

            var sneak_attack = library.Get<BlueprintFeature>("9b9eac6709e1c084cb18c3a366e0ec87");
            snakebite_striker.RemoveFeatures = new LevelEntry[]
            {
                Helpers.LevelEntry(2, combat_feat),
                Helpers.LevelEntry(3, maneuver_training[0]),
                Helpers.LevelEntry(6, combat_feat),
                Helpers.LevelEntry(7, maneuver_training[1]),
                Helpers.LevelEntry(10, combat_feat),
                Helpers.LevelEntry(11, maneuver_training[2]),
                Helpers.LevelEntry(14, combat_feat),
                Helpers.LevelEntry(15, maneuver_training[3]),
                Helpers.LevelEntry(18, combat_feat),
                Helpers.LevelEntry(19, maneuver_training[4]),
            };

            snakebite_striker.AddFeatures = new LevelEntry[] {Helpers.LevelEntry(2, sneak_attack),
                                                              Helpers.LevelEntry(3, snake_feint[0]),
                                                              Helpers.LevelEntry(6, sneak_attack),
                                                              Helpers.LevelEntry(7, snake_feint[1]),
                                                              Helpers.LevelEntry(10, sneak_attack),
                                                              Helpers.LevelEntry(11, opportunist),
                                                              Helpers.LevelEntry(14, sneak_attack),
                                                              Helpers.LevelEntry(15, snake_feint[2]),
                                                              Helpers.LevelEntry(18, sneak_attack),
                                                             };

            brawler_progression.UIGroups = brawler_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(sneak_attack));
            brawler_progression.UIGroups = brawler_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(snake_feint.AddToArray(opportunist)));

            snakebite_striker.ReplaceClassSkills = true;
            snakebite_striker.ClassSkills = brawler_class.ClassSkills.AddToArray(StatType.SkillStealth);
        }


        static void createSnakeFeint()
        {
            snake_feint[0] = Helpers.CreateFeature("SnakeFeint3Feature",
                                                   "Snake Feint",
                                                   "At 3rd level, a snakebite striker receives Improved Feint feat, even if he doesn’t meet the prerequisites.",
                                                   "",
                                                   null,//NewFeats.improved_feint.Icon,
                                                   FeatureGroup.None
                                                   //Helpers.CreateAddFact(NewFeats.improved_feint) will be added in newfeats
                                                   );

            snake_feint[1] = Helpers.CreateFeature("SnakeFeint11Feature",
                                                   "Snake Feint",
                                                   "At 11th level, a snakebite striker always flanks an enemy independently of his position, as long as at least one other ally threatens the same enemy.",
                                                   "",
                                                   Helpers.GetIcon("422dab7309e1ad343935f33a4d6e9f11"),
                                                   FeatureGroup.None
                                                   );
            snake_feint[1].HideInCharacterSheetAndLevelUp = true;
            snake_feint[1].HideInUI = true;


            var buff2 = Helpers.CreateBuff("SnakeFeint19Buff",
                                           "",
                                           "",
                                           "",
                                           null,
                                           null,
                                           Helpers.Create<FlankingMechanics.AlwaysFlankedIfEngagedByCaster>()
                                           );
            buff2.SetBuffFlags(BuffFlags.HiddenInUi);
            snake_feint[2] = Helpers.CreateFeature("SnakeFeint19Feature",
                                                   "Snake Feint",
                                                   "At 19th level, enemies threatened by snakebite striker are always considered to be flanked.",
                                                   "",
                                                   Helpers.GetIcon("422dab7309e1ad343935f33a4d6e9f11"),
                                                   FeatureGroup.None,
                                                   Common.createAuraEffectFeatureComponentCustom(buff2, 15.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>()))
                                                   );
        }


        static void createOpportunist()
        {
            opportunist = library.CopyAndAdd<BlueprintFeature>("5bb6dc5ce00550441880a6ff8ad4c968", "SnakebiteStrikerOpportunist", "");
            opportunist.SetDescription(" At 11th level, once per round the snakebite striker can make an attack of opportunity against an opponent who has just been struck for damage in melee by another character. This attack counts as an attack of opportunity for that round. She cannot use this ability more than once per round, even if she has the Combat Reflexes feat or a similar ability. At 19th level, she can use this ability twice per round.");

            opportunist.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.Create<AooMechanics.OpportunistMultipleAttacks>(o => o.num_extra_attacks = Helpers.CreateContextValue(AbilityRankType.Default)),
                Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getBrawlerArray(),
                                                progression: ContextRankProgression.DelayedStartPlusDivStep,
                                                startLevel: 19,
                                                stepLevel: 100)
            };
        }


        static void createWildChild()
        {
            wild_child = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "WildChild";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Wild Child");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "The wild child works with his sworn animal friend to conquer the challenges that lay before them. This kinship could come from being lost in the wilderness and raised by animals or growing up with an exotic pet.");
            });
            Helpers.SetField(wild_child, "m_ParentClass", brawler_class);
            library.AddAsset(wild_child, "");

            createAnimalCompanion();
            createHunterTricks();

            wild_child.RemoveFeatures = new LevelEntry[]
            {
                Helpers.LevelEntry(1, combat_feat),
                Helpers.LevelEntry(4, combat_feat),
                Helpers.LevelEntry(6, combat_feat),
                Helpers.LevelEntry(10, combat_feat),
                Helpers.LevelEntry(12, combat_feat),
                Helpers.LevelEntry(16, combat_feat),
                Helpers.LevelEntry(18, combat_feat),
            };

            wild_child.AddFeatures = new LevelEntry[] {Helpers.LevelEntry(1, animal_companion),
                                                             Helpers.LevelEntry(6, hunter_trick_selection),
                                                             Helpers.LevelEntry(12, hunter_trick_selection),
                                                             Helpers.LevelEntry(18, hunter_trick_selection),
                                                            };

            brawler_progression.UIDeterminatorsGroup = brawler_progression.UIDeterminatorsGroup.AddToArray(animal_companion);
            brawler_progression.UIGroups = brawler_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(hunter_trick_selection));
            wild_child.ReplaceClassSkills = true;
            wild_child.ClassSkills = brawler_class.ClassSkills.AddToArray(StatType.SkillLoreNature);
        }


        static void createHunterTricks()
        {
            hunter_trick_selection = library.CopyAndAdd(Hunter.trick_selection, "WildChildTrickSelection", "");
            hunter_trick_selection.SetNameDescription("Wild Tricks",
                                                      "The wild child has learned a number of tricks to aid his allies and his animal companion, as well as to hinder his opponents.\n"
                                                      + "At 6th level and every 6 levels thereafter, the wild child and his companion learn one hunter’s trick. Wild child cannot choose any tricks that rely on ranged attacks. They can use these tricks a number of times per day equal to 1/2 their brawler level or animal companion HD + their Constitution modifier. This ability otherwise follows the rules of the hunter’s tricks ability, including all action costs.");
            ClassToProgression.addClassToResource(brawler_class, new BlueprintArchetype[] { wild_child }, Hunter.trick_resource, library.Get<BlueprintCharacterClass>("4cd1757a0eea7694ba5c933729a53920"));
            hunter_trick_selection.ComponentsArray = new BlueprintComponent[]
            {
                Hunter.trick_resource.CreateAddAbilityResource(),
                Helpers.Create<ResourceMechanics.ContextIncreaseResourceAmount>(r => {r.Resource = Hunter.trick_resource; r.Value = Helpers.CreateContextValue(AbilityRankType.Default); }),
                Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, stat: StatType.Constitution),
                Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Constitution),
                Common.createAddFeatComponentsToAnimalCompanion("WildCHildAnimalCompanionTrickResource",
                                                Helpers.CreateAddAbilityResource(Hunter.trick_resource),
                                                Helpers.Create<ResourceMechanics.ContextIncreaseResourceAmount>(r =>
                                                {
                                                    r.Resource = Hunter.trick_resource;
                                                    r.Value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                }
                                                ),
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, stat: StatType.Constitution),
                                                Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Constitution)
                                                )
            };
            for (int i = 0; i < hunter_trick_selection.AllFeatures.Length; i++)
            {
                hunter_trick_selection.AllFeatures[i] = library.CopyAndAdd(hunter_trick_selection.AllFeatures[i], "WildChild" + hunter_trick_selection.AllFeatures[i].name, "");
                hunter_trick_selection.AllFeatures[i].AddComponents(hunter_trick_selection.AllFeatures[i].GetComponent<AddFeatureToCompanion>().Feature.ComponentsArray);
            }
        }


        static void createAnimalCompanion()
        {
            var animal_companion_progression = library.CopyAndAdd<BlueprintProgression>("924fb4b659dcb4f4f906404ba694b690",
                                                                                      "WildChildAnimalCompanionProgression",
                                                                                      "");
            animal_companion_progression.Classes = new BlueprintCharacterClass[] { brawler_class };

            animal_companion = library.CopyAndAdd<BlueprintFeatureSelection>("2995b36659b9ad3408fd26f137ee2c67",
                                                                                            "AnimalCompanionSelectionWildCHild",
                                                                                            "");
            animal_companion.SetDescription("At 1st level, a wild child forms a bond with a loyal companion that accompanies the wild child on his adventures. A wild child can begin play with any of the animals available to a druid. The wild child uses his brawler level as his effective druid level for determining the abilities of his animal companion.");
            var add_progression = Helpers.Create<AddFeatureOnApply>();
            add_progression.Feature = animal_companion_progression;
            animal_companion.ComponentsArray[0] = add_progression;
        }


        static void createMutagenicMauler()
        {
            mutagenic_mauler = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "MutagenicMauler";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Mutagenic Mauler");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Not content with perfecting her body with natural methods, a mutagenic mauler resorts to alchemy to unlock the primal beast within.");
            });
            Helpers.SetField(mutagenic_mauler, "m_ParentClass", brawler_class);
            library.AddAsset(mutagenic_mauler, "");

            createMutagen();
            createBeastmorph();

            mutagenic_mauler.RemoveFeatures = new LevelEntry[]
            {
                Helpers.LevelEntry(1, combat_feat),
                Helpers.LevelEntry(4, ac_bonus),
                Helpers.LevelEntry(6, combat_feat),
                Helpers.LevelEntry(10, combat_feat),
                Helpers.LevelEntry(12, combat_feat),
            };

            mutagenic_mauler.AddFeatures = new LevelEntry[] {Helpers.LevelEntry(1, mutagen),
                                                     Helpers.LevelEntry(4, beastmorph_speed),
                                                     Helpers.LevelEntry(6, mutagen_damage_bonus),
                                                     Helpers.LevelEntry(10, discovery),
                                                     Helpers.LevelEntry(12, greater_mutagen),
                                                     Helpers.LevelEntry(13, beastmorph_blindsense),
                                                    };

            brawler_progression.UIGroups = brawler_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(mutagen, beastmorph_speed, mutagen_damage_bonus, discovery, greater_mutagen, beastmorph_blindsense)
                                                                                   );
        }


        static void createBeastmorph()
        {
            beastmorph_speed = Helpers.CreateFeature("MutagenicMaulerBeastmorphSpeedBonusFeature",
                                                     "Beastmorph: Speed Bonus",
                                                     "Starting at 4th level, a mutagenic mauler gains additional abilities when using her mutagen. At 4th level, she gains a +10 enhancement bonus to her base speed. At 13th level, the enhancement bonus to her base speed increases to +15 feet. At 18th level, the enhancement bonus to her base speed increases to +20 feet.",
                                                     "",
                                                     Helpers.GetIcon("4f8181e7a7f1d904fbaea64220e83379"), //expeditious retreat
                                                     FeatureGroup.None
                                                     );

            var beastmorph_speed_buff = Helpers.CreateBuff("MutagenicMaulerBeastmorphSpeedBonusBuff",
                                         beastmorph_speed.Name,
                                         beastmorph_speed.Description,
                                         "",
                                         beastmorph_speed.Icon,
                                         null,
                                         Helpers.CreateAddContextStatBonus(StatType.Speed, ModifierDescriptor.Enhancement),
                                         Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.Custom,
                                                                         classes: getBrawlerArray(),
                                                                         customProgression: new (int, int)[] { (12, 10), (17, 15), (20, 20) }
                                                                         )
                                         );


            beastmorph_blindsense = Helpers.CreateFeature("MutagenicMaulerBeastmorphBlindsenseFeature",
                                         "Beastmorph: Blindsense",
                                         "At 13th level, a mutagenic mauler gains blindsense ability within 30 feet, when using her mutagen.",
                                         "",
                                         Helpers.GetIcon("b3da3fbee6a751d4197e446c7e852bcb"), //true seeing
                                         FeatureGroup.None
                                         );

            var beastmorph_blindsense_buff = Helpers.CreateBuff("MutagenicMaulerBeastmorphBlindsenseBuff",
                                         beastmorph_blindsense.Name,
                                         beastmorph_blindsense.Description,
                                         "",
                                         beastmorph_blindsense.Icon,
                                         null,
                                         Common.createBlindsense(30)
                                         );

            var mutagens = new BlueprintFeature[]
            {
                library.Get<BlueprintFeature>("cee8f65448ce71c4b8b8ca13751dd8ea"), //mutagen
                library.Get<BlueprintFeature>("76c61966afdd82048911f3d63c6fe0bc"), //greater mutagen
                library.Get<BlueprintFeature>("6f5cb651e26bd97428523061b07ffc85"), //grand mutagen

            };

            foreach (var m in mutagens)
            {
                var comp = m.GetComponent<AddFacts>();

                foreach (var f in comp.Facts)
                {
                    var buff = Common.extractActions<ContextActionApplyBuff>((f as BlueprintAbility).GetComponent<AbilityEffectRunAction>().Actions.Actions)[0].Buff;

                    Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(buff, beastmorph_speed_buff, beastmorph_speed);
                    Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(buff, beastmorph_blindsense_buff, beastmorph_blindsense);
                }
            }

        }


        static void createMutagen()
        {
            var alchemist_mutagen = library.Get<BlueprintFeature>("cee8f65448ce71c4b8b8ca13751dd8ea");
            mutagen = library.CopyAndAdd(alchemist_mutagen, "MutagenicMaulerMutagen", "");
            mutagen.GetComponent<AddFacts>().DoNotRestoreMissingFacts = true;
            foreach (var c in mutagen.GetComponents<SpellLevelByClassLevel>().ToArray())
            {
                mutagen.ReplaceComponent(c, c.CreateCopy(cc => cc.Class = brawler_class));
            }
            alchemist_mutagen.AddComponent(Helpers.Create<NewMechanics.FeatureReplacement>(f => f.replacement_feature = mutagen));
            mutagen.SetDescription("At 1st level, a mutagenic mauler discovers how to create a mutagen that she can imbibe in order to heighten her physical prowess, though at the cost of her personality. This functions as an alchemist’s mutagen and uses the brawler’s class level as her alchemist level for this ability (alchemist levels stack with brawler levels for determining the effect of this ability). A mutagenic mauler counts as an alchemist for the purpose of imbibing a mutagen prepared by someone else.");
            var mutagen_damage_buff = Helpers.CreateBuff("MutagenincMaulerDamageBonusBuff",
                                                         "Mutagen Damage Bonus",
                                                         "At 6th level, a mutagenic mauler gains a +2 bonus on damage rolls when she attacks in melee while in her mutagenic form. This bonus increases to +3 at 11th level, and to +4 at 16th level.",
                                                         "",
                                                         Helpers.GetIcon("85067a04a97416949b5d1dbf986d93f3"), //stone fist
                                                         null,
                                                         Helpers.Create<WeaponAttackTypeDamageBonus>(w =>
                                                         {
                                                             w.Value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                             w.Type = AttackTypeAttackBonus.WeaponRangeType.Melee;
                                                             w.Descriptor = ModifierDescriptor.UntypedStackable;
                                                             w.AttackBonus = 1;
                                                         }
                                                         ),
                                                         Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getBrawlerArray(),
                                                                                         progression: ContextRankProgression.StartPlusDivStep,
                                                                                         startLevel: 1, stepLevel: 5, max: 4)
                                                       );

            mutagen_damage_bonus = Helpers.CreateFeature("MutagenicMaulerDamageBonusFeature",
                                                         mutagen_damage_buff.Name,
                                                         mutagen_damage_buff.Description,
                                                         "",
                                                         mutagen_damage_buff.Icon,
                                                         FeatureGroup.None);

            var mutagens = new BlueprintFeature[]
            {
                library.Get<BlueprintFeature>("cee8f65448ce71c4b8b8ca13751dd8ea"), //mutagen
                library.Get<BlueprintFeature>("76c61966afdd82048911f3d63c6fe0bc"), //greater mutagen
                library.Get<BlueprintFeature>("6f5cb651e26bd97428523061b07ffc85"), //grand mutagen

            };

            foreach (var m in mutagens)
            {
                var comp = m.GetComponent<AddFacts>();

                foreach (var f in comp.Facts)
                {
                    var buff = Common.extractActions<ContextActionApplyBuff>((f as BlueprintAbility).GetComponent<AbilityEffectRunAction>().Actions.Actions)[0].Buff;

                    Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(buff, mutagen_damage_buff, mutagen_damage_bonus);
                }
            }

            discovery = library.CopyAndAdd<BlueprintFeatureSelection>("cd86c437488386f438dcc9ae727ea2a6", "MutagenicMaulerDiscovery", "");
            discovery.SetDescription("At 10th level, a mutagenic mauler learns one of the following alchemist discoveries: feral mutagen, preserve organs, spontaneous healing.");
            discovery.AllFeatures = new BlueprintFeature[]
            {
                library.Get<BlueprintFeature>("fd5f7b37ab4301c48a88cc196ee5f0ce"), //feral mutagen
                library.Get<BlueprintFeature>("76b4bb8e54f3f5c418f421684c76ef4e"), //preserve organs
                library.Get<BlueprintFeature>("2bc1ee626a69667469ab5c1698b99956"), //spontaneous healing
            };

            var spontaneous_healing_resource = library.Get<BlueprintAbilityResource>("0b417a7292b2e924782ef2aab9451816");
            ClassToProgression.addClassToResource(brawler_class, new BlueprintArchetype[] { mutagenic_mauler }, spontaneous_healing_resource, library.Get<BlueprintCharacterClass>("0937bec61c0dabc468428f496580c721"));

            var alchemist_greater_mutagen = library.Get<BlueprintFeature>("76c61966afdd82048911f3d63c6fe0bc");
            greater_mutagen = library.CopyAndAdd(alchemist_greater_mutagen, "MutagenicMaulerGreaterMutagen", "");
            greater_mutagen.RemoveComponents<PrerequisiteClassLevel>();
            greater_mutagen.SetDescription("At 12th level, the mutagenic mauler learns the greater mutagen discovery.");
        
            foreach (var c in greater_mutagen.GetComponents<SpellLevelByClassLevel>().ToArray())
            {
                greater_mutagen.ReplaceComponent(c, c.CreateCopy(cc => cc.Class = brawler_class));
            }
            alchemist_greater_mutagen.AddComponent(Helpers.Create<NewMechanics.FeatureReplacement>(f => f.replacement_feature = greater_mutagen));
        }


        static void createExemplar()
        {
            exemplar = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "ExemplarArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Exemplar");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "A versatile soldier who inspires her companions with her fighting prowess, an exemplar is at home on the front lines of battles anywhere.");
            });
            Helpers.SetField(exemplar, "m_ParentClass", brawler_class);
            library.AddAsset(exemplar, "");

            var improved_unarmed_strike = library.Get<BlueprintFeature>("7812ad3672a4b9a4fb894ea402095167");

            createCallToArms();
            createInspiringProwess();
            createFieldInstruction();

            exemplar.RemoveFeatures = new LevelEntry[]
            {
                Helpers.LevelEntry(1, unarmed_strike, improved_unarmed_strike),
                Helpers.LevelEntry(3, maneuver_training[0]),
                Helpers.LevelEntry(4, ac_bonus),
                Helpers.LevelEntry(5, brawlers_strike_magic, close_weapon_mastery),
                Helpers.LevelEntry(7, maneuver_training[1]),
                Helpers.LevelEntry(9, brawlers_strike_cold_iron_and_silver),
                Helpers.LevelEntry(11, maneuver_training[2]),
                Helpers.LevelEntry(12, brawlers_strike_alignment),
                Helpers.LevelEntry(15, maneuver_training[3]),
                Helpers.LevelEntry(17, brawlers_strike_adamantine),
                Helpers.LevelEntry(19, maneuver_training[4]),
                Helpers.LevelEntry(20, perfect_warrior),
            };

            exemplar.AddFeatures = new LevelEntry[] {Helpers.LevelEntry(1, call_to_arms),
                                                     Helpers.LevelEntry(3, inspiring_prowess, inspire_courage, performance_resource_feature),
                                                     Helpers.LevelEntry(5, field_instruction),
                                                     Helpers.LevelEntry(11, inspire_greatness),
                                                     Helpers.LevelEntry(15, inspire_heroics)
                                                    };

            brawler_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { brawler_proficiencies, unarmed_strike, improved_unarmed_strike, martial_training };
            brawler_progression.UIGroups = brawler_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(call_to_arms, inspiring_prowess, field_instruction),
                                                                                   Helpers.CreateUIGroup(inspire_courage, inspire_greatness, inspire_heroics)
                                                                                   );
            exemplar.OverrideAttributeRecommendations = true;
            exemplar.RecommendedAttributes = brawler_class.RecommendedAttributes.AddToArray(StatType.Charisma);
        }


        static void createInspiringProwess()
        {
            var resource = library.Get<BlueprintAbilityResource>("e190ba276831b5c4fa28737e5e49e6a6");
            //ClassToProgression.addClassToResource(brawler_class, new BlueprintArchetype[] { exemplar }, resource, library.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f"));

            var fake_bard_class = library.CopyAndAdd<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f", "FakeBardExemplarClas", "");

            var inspire_courage_ability = library.CopyAndAdd<BlueprintActivatableAbility>("70274c5aa9124424c984217b62dabee8", "ExemplarInspireCourageToggleAbility", "");
            inspire_courage_ability.SetDescription("A 3rd level exemplar can use his inspiring prowess to inspire courage in his allies (including himself), bolstering them against fear and improving their combat abilities. To be affected, an ally must be able to perceive the exemplars's performance. An affected ally receives a +1 morale bonus on saving throws against charm and fear effects and a +1 competence bonus on attack and weapon damage rolls. At 7th level, and every six exemplar levels thereafter, this bonus increases by +1, to a maximum of +4 at 19th level.");
            ClassToProgression.addClassToBuff(fake_bard_class, new BlueprintArchetype[] { }, inspire_courage_ability.Buff, library.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f"));

            var inspire_greatness_ability = library.CopyAndAdd<BlueprintActivatableAbility>("be36959e44ac33641ba9e0204f3d227b", "ExemplarInspireGreatnessToggleAbility", "");
            inspire_greatness_ability.SetDescription("An exemplar of 11th level or higher can use his inspiring prowess to inspire greatness in all allies within 30 feet, granting extra fighting capability. A creature inspired with greatness gains 2 bonus Hit Dice (d10s), the commensurate number of temporary hit points (apply the target's Constitution modifier, if any, to these bonus Hit Dice), a +2 competence bonus on attack rolls, and a +1 competence bonus on Fortitude saves.");

            var inspire_heroics_ability = library.CopyAndAdd<BlueprintActivatableAbility>("a4ce06371f09f504fa86fcf6d0e021e4", "ExemplarInspireHeroicsToggleAbility", "");
            inspire_heroics_ability.SetDescription("An exemplar of 15th level or higher can inspire tremendous heroism in all allies within 30 feet. Inspired creatures gain a +4 morale bonus on saving throws and a +4 dodge bonus to AC. The effect lasts for as long as the targets are able to witness the performance.");

     
            inspire_courage = Common.ActivatableAbilityToFeature(inspire_courage_ability, false);
            inspire_heroics = Common.ActivatableAbilityToFeature(inspire_heroics_ability, false);
            inspire_greatness = Common.ActivatableAbilityToFeature(inspire_greatness_ability, false);

            var performance_resource = library.Get<BlueprintAbilityResource>("e190ba276831b5c4fa28737e5e49e6a6");
            performance_resource_feature = library.Get<BlueprintFeature>("b92bfc201c6a79e49afd0b5cfbfc269f");
            performance_resource_feature.AddComponent(Helpers.Create<NewMechanics.IncreaseResourcesByClassWithArchetype>(i => 
                                                        {
                                                            i.Resource = performance_resource;
                                                            i.CharacterClass = brawler_class;
                                                            i.Archetype = exemplar;
                                                            i.base_value = -2;
                                                        }));

            inspiring_prowess = Helpers.CreateFeature("ExemplarInspiringProwessFeature",
                                                      "Inspiring Prowess",
                                                      "At 3rd level, an exemplar gains the ability to use certain bardic performances. She can use this ability for a number of rounds per day equal to 3 + her Charisma modifier; this increases by 1 round per brawler level thereafter. The exemplar’s effective bard level for this ability is equal to her brawler level – 2. At 3rd level, the exemplar can use inspire courage. At 11th level, the exemplar can use inspire greatness. At 15th level, the exemplar can use inspire heroics. Instead of the Perform skill, she activates this ability with impressive flourishes and displays of martial talent (this uses visual components).",
                                                      "",
                                                      null,
                                                      FeatureGroup.None,
                                                      Helpers.Create<FakeClassLevelMechanics.AddFakeClassLevel>(a => { a.fake_class = fake_bard_class; a.value = Helpers.CreateContextValue(AbilityRankType.Default); }),
                                                      Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.BonusValue, classes: getBrawlerArray(),
                                                                                      stepLevel: -2
                                                                                     )
                                                     );
            inspiring_prowess.ReapplyOnLevelUp = true;
        }


        static void createCallToArms()
        {
            var resource = Helpers.CreateAbilityResource("ExemplarCallToArmsResource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(3, 2, 1, 2, 1, 0, 0.0f, getBrawlerArray());

            var buff = Helpers.CreateBuff("CallToArmsBuff",
                                          "Call to Arms",
                                          "At 1st level, an exemplar rouse her allies into action. All allies within 30 feet are no longer flat-footed, even if they are surprised. Using this ability is a move action. This ability can be used 3 times per day + 1 more time per 2 exemplar levels. At 6th level, the exemplar can use it as a swift action instead. At 10th level, she can use it as a free action.",
                                          "",
                                          Helpers.GetIcon("76f8f23f6502def4dbefedffdc4d4c43"),
                                          null,
                                          Helpers.Create<FlatFootedIgnore>(f => f.Type = FlatFootedIgnoreType.UncannyDodge)
                                          );

            var ability = Helpers.CreateAbility("CallToArmsAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Extraordinary,
                                                CommandType.Move,
                                                AbilityRange.Personal,
                                                Helpers.oneRoundDuration,
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1), dispellable: false)),
                                                Common.createAbilitySpawnFx("8de64fbe047abc243a9b4715f643739f", anchor: AbilitySpawnFxAnchor.SelectedTarget, position_anchor: AbilitySpawnFxAnchor.None, orientation_anchor: AbilitySpawnFxAnchor.None),
                                                Helpers.CreateAbilityTargetsAround(30.Feet(), TargetType.Ally),
                                                resource.CreateResourceLogic()
                                                );
            ability.setMiscAbilityParametersSelfOnly();

            call_to_arms = Common.AbilityToFeature(ability, false);
            call_to_arms.AddComponent(resource.CreateAddAbilityResource());

            var feature_move = Helpers.CreateFeature("CallToArmsMoveFeature",
                                                     "",
                                                     "",
                                                     "",
                                                     null,
                                                     FeatureGroup.None,
                                                     Helpers.Create<TurnActionMechanics.UseAbilitiesAsSwiftAction>(m => m.abilities = new BlueprintAbility[] { ability })
                                                     );

            feature_move.HideInCharacterSheetAndLevelUp = true;
            feature_move.HideInUI = true;
            var feature_swift = library.CopyAndAdd(feature_move, "CallToArmsSwiftFeature", "");
            feature_swift.ReplaceComponent<TurnActionMechanics.UseAbilitiesAsSwiftAction>(Helpers.Create<TurnActionMechanics.UseAbilitiesAsFreeAction>(m => m.abilities = new BlueprintAbility[] { ability }));


            call_to_arms.AddComponents(Helpers.CreateAddFeatureOnClassLevel(feature_move, 6, getBrawlerArray()),
                                       Helpers.CreateAddFeatureOnClassLevel(feature_swift, 10, getBrawlerArray())
                                       );
        }


        static void createFieldInstruction()
        {
            field_instruction_ability = library.CopyAndAdd<BlueprintAbility>("00af3b5f43aa7ae4c87bcfe4e129f6e8", "BrawlerFieldInstructionAbility", ""); //vanguard tactician
            field_instruction_ability.SetName("Field Instruction");
            field_instruction_ability.SetDescription("At 5th level, as a standard action an exemplar can grant a teamwork feat to all allies within 30 feet who can see and hear her. This teamwork feat must be one the exemplar knows or has gained with the martial flexibility ability. Allies retain the use of this teamwork feat for 3 rounds + 1 round for every 2 brawler levels. If the granted teamwork feat is one gained from martial flexibility, this duration ends immediately if the exemplar loses access to that feat. Allies don’t need to meet the prerequisites of this teamwork feat. The exemplar can use this ability once per day at 5th level, plus one additional time per day at 9th, 12th, and 17th level.");

            var tactician_resource = Helpers.CreateAbilityResource("FieldInstrucitonResource", "", "", "", null);
            tactician_resource.name = "BrawlerTacticianResource";
            tactician_resource.SetFixedResource(1);

            field_instruction_ability.ReplaceComponent<AbilityResourceLogic>(Helpers.CreateResourceLogic(tactician_resource));

            var abilities = field_instruction_ability.Variants;
            var new_abilities = new List<BlueprintAbility>();

            foreach (var a in abilities)
            {
                var new_ability = library.CopyAndAdd(a, a.name.Replace("Vanguard", "Exemplar"), "");
                new_ability.ReplaceComponent<AbilityResourceLogic>(Helpers.CreateResourceLogic(tactician_resource));
                new_ability.Parent = field_instruction_ability;
                new_ability.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                                    progression: ContextRankProgression.DivStep,
                                                                                                    stepLevel: 2,
                                                                                                    classes: new BlueprintCharacterClass[] { brawler_class }
                                                                                                    )
                                                                    );

                var buff = Common.extractActions<ContextActionApplyBuff>(new_ability.GetComponent<AbilityEffectRunAction>().Actions.Actions).FirstOrDefault().Buff;
                var teamwork_feat_name = buff.GetComponent<AddFactsFromCaster>().Facts[0].Name;
                new_ability.SetName("Field Instruction — " + teamwork_feat_name);
                new_abilities.Add(new_ability);
                //change buffs to pick name from parent ability
                Common.extractActions<ContextActionApplyBuff>(a.GetComponent<AbilityEffectRunAction>().Actions.Actions).FirstOrDefault().Buff.SetName("");
            }

            field_instruction_ability.ReplaceComponent<AbilityVariants>(a => a.Variants = new_abilities.ToArray());

            field_instruction = Helpers.CreateFeature("FieldInstructionFeature",
                                                      field_instruction_ability.Name,
                                                      field_instruction_ability.Description,
                                                     "",
                                                     field_instruction_ability.Icon,
                                                     FeatureGroup.None,
                                                     Helpers.CreateAddFact(field_instruction_ability),
                                                     Helpers.CreateAddAbilityResource(tactician_resource),
                                                     Helpers.Create<ResourceMechanics.ContextIncreaseResourceAmount>(c =>
                                                     {
                                                         c.Value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                         c.Resource = tactician_resource;
                                                     }),
                                                     Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.Custom,
                                                                                     classes: getBrawlerArray(),
                                                                                     customProgression: new (int, int)[] { (8, 0), (11, 1), (16, 2), (20, 3) }
                                                                                     )
                                                    );
            field_instruction.ReapplyOnLevelUp = true;
        }


        public static BlueprintCharacterClass[] getBrawlerArray()
        {
            return new BlueprintCharacterClass[] { brawler_class };
        }


        static void createBrawlerProgression()
        {
            var monk_class = library.Get<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");
            var fighter_class = library.Get<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");
            brawler_proficiencies = library.CopyAndAdd<BlueprintFeature>("8c971173613282844888dc20d572cfc9", "BrawlerProficiencies", ""); //cleric proficiencies
            brawler_proficiencies.ReplaceComponent<AddFacts>(a => a.Facts = a.Facts.RemoveFromArray(library.Get<BlueprintFeature>("46f4fb320f35704488ba3d513397789d")));
            brawler_proficiencies.AddComponent(Common.createAddWeaponProficiencies(WeaponCategory.Handaxe, WeaponCategory.PunchingDagger,
                                                                                   WeaponCategory.SpikedHeavyShield, WeaponCategory.SpikedLightShield,
                                                                                   WeaponCategory.WeaponLightShield, WeaponCategory.WeaponHeavyShield)
                                                                                   );
            brawler_proficiencies.SetNameDescriptionIcon("Brawler Proficiencies",
                                                         "A brawler is proficient with all simple weapons plus the handaxe, short sword, and weapons from the close fighter weapon group. She is proficient with light armor and shields (except tower shields).",
                                                         null);

            var fist1d6_monk = library.Get<BlueprintFeature>("c3fbeb2ffebaaa64aa38ce7a0bb18fb0");         
            unarmed_strike = library.CopyAndAdd(fist1d6_monk, "BrawlerUnarmedStrikeFeature", "");
            //we add class to progression after to prevent monk from increasing unarmed damage using brawler class levels for brawler archetypes that remove unarmed strike feature
            ClassToProgression.addClassToFeat(brawler_class, new BlueprintArchetype[] { }, ClassToProgression.DomainSpellsType.NoSpells, unarmed_strike, monk_class);
            unarmed_strike.SetDescription("At 1st level, a brawler gains Improved Unarmed Strike as a bonus feat. The damage dealt by a Medium brawler's unarmed strike increases with level: 1d6 at levels 1–3, 1d8 at levels 4–7, 1d10 at levels 8–11, 2d6 at levels 12–15, 2d8 at levels 16–19, and 2d10 at level 20.\nIf the sacred fist is Small, his unarmed strike damage increases as follows: 1d4 at levels 1–3, 1d6 at levels 4–7, 1d8 at levels 8–11, 1d10 at levels 12–15, 2d6 at levels 16–19, and 2d8 at level 20.\nIf the brawler is Large, his unarmed strike damage increases as follows: 1d8 at levels 1–3, 2d6 at levels 4–7, 2d8 at levels 8–11, 3d6 at levels 12–15, 3d8 at levels 16–19, and 4d8 at level 20.");
            var improved_unarmed_strike = library.Get<BlueprintFeature>("7812ad3672a4b9a4fb894ea402095167");

            combat_feat = library.CopyAndAdd<BlueprintFeatureSelection>("41c8486641f7d6d4283ca9dae4147a9f", "BrawlerCombatFeat", "");
            combat_feat.SetDescription("At 1st level, and at every even level thereafter, a brawler gains a bonus feat in addition to those gained from normal advancement (meaning that the brawler gains a feat at every level). These bonus feats must be selected from those listed as Combat Feats.");

            createMartialTraining();
            createBrawlersCunning();
            createBrawlersFlurry();
            createPerfectWarrior();
            createManeuverTraining();
            createACBonus();
            createKnockout();
            createBrawlersStrike();
            createCloseWeaponMastery();
            createAwesomeBlow();

            brawler_progression = Helpers.CreateProgression("BrawlerProgression",
                                                              brawler_class.Name,
                                                              brawler_class.Description,
                                                              "",
                                                              brawler_class.Icon,
                                                              FeatureGroup.None);
            brawler_progression.Classes = getBrawlerArray();




            brawler_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, brawler_proficiencies, brawlers_cunning, unarmed_strike, improved_unarmed_strike,
                                                                                       martial_training, combat_feat,
                                                                                       library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                       library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")), // touch calculate feature                                                                                      
                                                                    Helpers.LevelEntry(2, brawlers_flurry, combat_feat),
                                                                    Helpers.LevelEntry(3, maneuver_training[0]),
                                                                    Helpers.LevelEntry(4, ac_bonus, knockout, combat_feat),
                                                                    Helpers.LevelEntry(5, brawlers_strike_magic, close_weapon_mastery),
                                                                    Helpers.LevelEntry(6, combat_feat),
                                                                    Helpers.LevelEntry(7, maneuver_training[1]),
                                                                    Helpers.LevelEntry(8, combat_feat),
                                                                    Helpers.LevelEntry(9, brawlers_strike_cold_iron_and_silver),
                                                                    Helpers.LevelEntry(10, combat_feat),
                                                                    Helpers.LevelEntry(11, maneuver_training[2], brawlers_flurry11),
                                                                    Helpers.LevelEntry(12, combat_feat, brawlers_strike_alignment),
                                                                    Helpers.LevelEntry(13),
                                                                    Helpers.LevelEntry(14, combat_feat),
                                                                    Helpers.LevelEntry(15, maneuver_training[3]),
                                                                    Helpers.LevelEntry(16, combat_feat, awesome_blow),
                                                                    Helpers.LevelEntry(17, brawlers_strike_adamantine),
                                                                    Helpers.LevelEntry(18, combat_feat),
                                                                    Helpers.LevelEntry(19, maneuver_training[4]),
                                                                    Helpers.LevelEntry(20, combat_feat, awesome_blow_improved, perfect_warrior)
                                                                    };

            brawler_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { brawler_proficiencies, unarmed_strike, improved_unarmed_strike, martial_training };
            brawler_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(brawlers_flurry, brawlers_flurry11),
                                                                Helpers.CreateUIGroup(combat_feat),
                                                                Helpers.CreateUIGroup(maneuver_training),
                                                                Helpers.CreateUIGroup(brawlers_strike_magic, brawlers_strike_cold_iron_and_silver, brawlers_strike_alignment, brawlers_strike_adamantine),
                                                                Helpers.CreateUIGroup(brawlers_cunning, ac_bonus, close_weapon_mastery, perfect_warrior),
                                                                Helpers.CreateUIGroup(knockout, awesome_blow, awesome_blow_improved)
                                                           };
        }


        static void createAwesomeBlow()
        {
            var text_entry = new Kingmaker.Blueprints.Root.Strings.CombatManeuverString.MyEntry();
            text_entry.Value = CombatManeuverTypeExtender.AwesomeBlow.ToCombatManeuverType();
            text_entry.Text = Helpers.CreateString("AwesomeBlow.String", "Combat Maneuver: Awesome Blow");
            BlueprintRoot.Instance.LocalizedTexts.CombatManeuver.Entries = BlueprintRoot.Instance.LocalizedTexts.CombatManeuver.Entries.AddToArray(text_entry);

            awesome_blow_improved = Helpers.CreateFeature("AwesomeBlowImprovedFeature",
                                                          "Improved Awesome Blow",
                                                          "At 20th level, the brawler can use her awesome blow ability on creatures of any size.",
                                                          "",
                                                          Helpers.GetIcon("c5a39c8f1a2d6824ca565e6c1e4075a5"), //pummeling charge
                                                          FeatureGroup.None
                                                          );

            var ability = library.CopyAndAdd<BlueprintAbility>("6fd05c4ecfebd6f4d873325de442fc17", "AwesomeBlowAction", "");
            ability.SetNameDescriptionIcon("Awesome Blow",
                                           "At 16th level, the brawler can as a standard action perform an awesome blow combat maneuver against a corporeal creature of her size or smaller. If the combat maneuver check succeeds, the opponent takes damage as if the brawler hit it with the close weapon she is wielding or an unarmed strike, it is knocked flying 10 feet in a direction of attack.",
                                           Helpers.GetIcon("bdf58317985383540920c723db07aa3b") //pummeling bully
                                           );

            ability.ReplaceComponent<AbilityEffectRunAction>(a =>
            {
                a.Actions = Helpers.CreateActionList(Helpers.Create<ContextActionCombatManeuver>(c =>
                {
                    c.Type = CombatManeuverTypeExtender.AwesomeBlow.ToCombatManeuverType();
                    c.OnSuccess = Helpers.CreateActionList();
                })
                );
            });

            ability.AddComponents(Helpers.Create<CombatManeuverMechanics.AbilityTargetNotBiggerUnlesFact>(a => a.fact = awesome_blow_improved),
                                  Helpers.Create<NewMechanics.AbilityCasterMainWeaponGroupCheck>(a =>
                                  {
                                      a.groups = new WeaponFighterGroup[] { WeaponFighterGroup.Close };
                                      a.extra_categories = new WeaponCategory[] { WeaponCategory.UnarmedStrike };
                                  }
                                  ),
                                  Common.createAbilityTargetHasFact(inverted: true, Common.incorporeal)
                                  );

            awesome_blow = Common.AbilityToFeature(ability, false);
            awesome_blow.AddComponent(Helpers.Create<ManeuverTrigger>(m =>
                {
                    m.OnlySuccess = true;
                    m.ManeuverType = CombatManeuverTypeExtender.AwesomeBlow.ToCombatManeuverType();
                    m.Action = Helpers.CreateActionList(Helpers.Create<ContextActionDealWeaponDamage>(),
                                                        Helpers.Create<CombatManeuverMechanics.ContextActionForceMove>(f => f.distance_dice = Helpers.CreateContextDiceValue(DiceType.Zero, 0, 10))
                                                        );
                }
                )
            );

            awesome_blow_ability = ability;
        }


        static void createCloseWeaponMastery()
        {
            DiceFormula[] diceFormulas = new DiceFormula[] {new DiceFormula(1, DiceType.D4),
                                                            new DiceFormula(1, DiceType.D6),
                                                            new DiceFormula(1, DiceType.D8),
                                                            new DiceFormula(1, DiceType.D10),
                                                            new DiceFormula(2, DiceType.D6),
                                                            new DiceFormula(2, DiceType.D8)};

            close_weapon_mastery = Helpers.CreateFeature("BrawlerCloseWeaponMasteryFeature",
                                              "Close Weapon Mastery",
                                              "At 5th level, a brawler’s damage with close weapons increases. When wielding a close weapon, she uses the unarmed strike damage of a brawler 4 levels lower instead of the base damage for that weapon (for example, a 5th-level Medium brawler wielding a punching dagger deals 1d6 points of damage instead of the weapon’s normal 1d4). If the weapon normally deals more damage than this, its damage is unchanged. This ability does not affect any other aspect of the weapon. The brawler can decide to use the weapon’s base damage instead of her adjusted unarmed strike damage—this must be declared before the attack roll is made.",
                                              "",
                                              Helpers.GetIcon("121811173a614534e8720d7550aae253"), //shield bash
                                              FeatureGroup.None,
                                              Helpers.Create<NewMechanics.ContextWeaponDamageDiceReplacementWeaponCategory>(c =>
                                              {
                                                  c.dice_formulas = diceFormulas;
                                                  c.value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                  c.categories = new WeaponCategory[] {WeaponCategory.SpikedHeavyShield, WeaponCategory.SpikedLightShield,
                                                                                       WeaponCategory.WeaponLightShield, WeaponCategory.WeaponHeavyShield,
                                                                                       WeaponCategory.PunchingDagger};
                                              }),
                                              Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                type: AbilityRankType.Default,
                                                                                progression: ContextRankProgression.StartPlusDivStep,
                                                                                startLevel: 4,
                                                                                stepLevel: 4,
                                                                                classes: getBrawlerArray())
                                              );
        }

        static void createBrawlersStrike()
        {
            brawlers_strike_magic = library.CopyAndAdd<BlueprintFeature>("1188005ee3160f84f8bed8b19a7d46cf", "BrawlerKiStrikeMagic", "");
            brawlers_strike_magic.SetNameDescription("Brawler's Strike —  Magic",
                                                     "At 5th level, a brawler’s unarmed strikes are treated as magic weapons for the purpose of overcoming damage reduction");

            brawlers_strike_cold_iron_and_silver = library.CopyAndAdd<BlueprintFeature>("7b657938fde78b14cae10fc0d3dcb991", "BrawlerStrikeColdIronSilver", "");
            brawlers_strike_cold_iron_and_silver.SetNameDescription("Brawler's Strike —  Cold Iron and Silver",
                                                                "At 9th level, the brawler's unarmed attacks are treated as cold iron and silver for the purpose of overcoming damage reduction.");
            var damage_alignments = new DamageAlignment[]
            {
                DamageAlignment.Lawful,
                DamageAlignment.Chaotic,
                DamageAlignment.Good,
                DamageAlignment.Evil
            };

            var alignment = new AlignmentMaskType[]
            {
               AlignmentMaskType.Chaotic,
               AlignmentMaskType.Lawful,
               AlignmentMaskType.Evil,
               AlignmentMaskType.Good
            };
          
            brawlers_strike_alignment = Helpers.CreateFeatureSelection("BrawlersStrikeAlignmentFeatureSelection",
                                                                        "Aligned Brawler's Strike",
                                                                        "At 12th level, the brawler chooses one alignment component: chaotic, evil, good, or lawful; her unarmed strikes also count as this alignment for the purpose of overcoming damage reduction. (This alignment component cannot be the opposite of the brawler’s actual alignment, such as a good brawler choosing evil strikes.)",
                                                                        "",
                                                                        null,
                                                                        FeatureGroup.None
                                                                        );

            for (int i = 0; i < damage_alignments.Length; i++)
            {
                var strike = library.CopyAndAdd<BlueprintFeature>("34439e527a8f5fb4588024e71960dd42", "BrawlersStrike" + damage_alignments[i].ToString(), "");
                strike.SetNameDescription($"Brawler's Strike — {damage_alignments[i].ToString()}",
                                          brawlers_strike_alignment.Description);
                strike.AddComponent(Common.createPrerequisiteAlignment(~alignment[i]));

                brawlers_strike_alignment.AllFeatures = brawlers_strike_alignment.AllFeatures.AddToArray(strike);
            }

            brawlers_strike_adamantine = library.CopyAndAdd<BlueprintFeature>("ddc10a3463bd4d54dbcbe993655cf64e", "BrawlersStrikeAdamantine", "");
            brawlers_strike_adamantine.SetNameDescription("Brawler's Strike — Adamantine",
                                                          "At 17th level, the brawler's unarmed attacks are treated as adamantine weapons for the purpose of overcoming damage reduction and bypassing hardness.");
        }


        static void createKnockout()
        {
            knockout_resource = Helpers.CreateAbilityResource("BrawlerKnockoutResource", "", "", "", null);
            knockout_resource.SetIncreasedByLevelStartPlusDivStep(1, 10, 1, 6, 1, 0, 0.0f, getBrawlerArray());

            var effect_buff = Helpers.CreateBuff("BrawlerKnockoutBuff",
                                          "Knocked out",
                                          "At 4th level, once per day a brawler can unleash a devastating attack that can instantly knock a target unconscious. She must announce this intent before making her attack roll. If the brawler hits and the target takes damage from the blow, the target must succeed at a Fortitude saving throw (DC = 10 + 1/2 the brawler’s level + the higher of the brawler’s Strength or Dexterity modifier) or fall unconscious for 1d6 rounds. Each round on its turn, the unconscious target may attempt a new saving throw to end the effect as a full-round action that does not provoke attacks of opportunity. Creatures immune to critical hits or nonlethal damage are immune to this ability. At 10th level, the brawler may use this ability twice per day; at 16th level, she may use it three times per day.",
                                          "",
                                          Helpers.GetIcon("247a4068296e8be42890143f451b4b45"),//basic feat
                                          null,
                                          Common.createBuffStatusCondition(UnitCondition.Unconscious, SavingThrowType.Fortitude)
                                          );

            var apply_buff = Common.createContextActionApplyBuff(effect_buff, Helpers.CreateContextDuration(0, DurationRate.Rounds, DiceType.D6, 1), dispellable: false);
            var effect = Helpers.CreateActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateConditionalSaved(null, apply_buff));
            var on_hit = Helpers.CreateConditional(Common.createContextConditionHasFacts(all: false, Common.undead, Common.construct, Common.elemental, Common.aberration),
                                                   null,
                                                   effect);
            var buff = Helpers.CreateBuff("BrawlerKnockoutOwnerBuff",
                                          "Knockout",
                                          effect_buff.Description,
                                          "",
                                          effect_buff.Icon,
                                          null
                                          );

            var physical_stat_property = NewMechanics.HighestStatPropertyGetter.createProperty("BrawlerStrOrDexProperty", "", StatType.Strength, StatType.Dexterity);

            buff.AddComponents(Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(effect, Common.createContextActionOnContextCaster(Common.createContextActionRemoveBuff(buff))),
                                                                               wait_for_attack_to_resolve: true),
                             Common.createContextCalculateAbilityParamsBasedOnClassesWithProperty(getBrawlerArray(), physical_stat_property)
                             );

            var ability = Helpers.CreateAbility("BrawlerKnockoutAbility",
                                                 buff.Name,
                                                 buff.Description,
                                                 "",
                                                 buff.Icon,
                                                 AbilityType.Extraordinary,
                                                 CommandType.Free,
                                                 AbilityRange.Personal,
                                                 "",
                                                 "",
                                                 Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false)),
                                                 Common.createAbilityCasterHasNoFacts(buff),
                                                 knockout_resource.CreateResourceLogic(),
                                                 Common.createContextCalculateAbilityParamsBasedOnClassesWithProperty(getBrawlerArray(), physical_stat_property)
                                                 );

            knockout = Common.AbilityToFeature(ability, false);
            knockout.AddComponent(knockout_resource.CreateAddAbilityResource());
        }


        static void createPerfectWarrior()
        {
            perfect_warrior = Helpers.CreateFeature("PerfectWarriorBrawlerFeature",
                                                    "Perfect Warrior",
                                                    "At 20th level, the brawler has reached the highest levels of her art. The brawler’s maneuver training increases by 2 and her dodge bonus to AC improves by 2.",
                                                    "",
                                                    Helpers.GetIcon("2a6a2f8e492ab174eb3f01acf5b7c90a"), //defensive stance
                                                    FeatureGroup.None
                                                    );
        }


        static void createACBonus()
        {
            var ac_bonus_feature = Helpers.CreateFeature("BrawlerACBonusFeature",
                                                         "AC Bonus",
                                                         "At 4th level, when a brawler wears light or no armor, she gains a +1 dodge bonus to AC and CMD. This bonus increases by 1 at 9th, 13th, and 18th levels.\n"
                                                         + "These bonuses to AC apply against touch attacks. She loses these bonuses while immobilized or helpless, wearing medium or heavy armor, or carrying a medium or heavy load.",
                                                         "",
                                                         Helpers.GetIcon("97e216dbb46ae3c4faef90cf6bbe6fd5"), //dodge
                                                         FeatureGroup.None,
                                                         Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.Dodge),
                                                         Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.Dodge, rankType: AbilityRankType.StatBonus),
                                                         Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                     classes: getBrawlerArray(),
                                                                                     progression: ContextRankProgression.Custom,
                                                                                     customProgression: new (int, int)[] { (8, 1), (12, 2), (17, 3), (20, 4) }),
                                                         Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureList,
                                                                                     type: AbilityRankType.StatBonus,
                                                                                     featureList: new BlueprintFeature[] { perfect_warrior, perfect_warrior }
                                                                                     )
                                                         );

            ac_bonus = Helpers.CreateFeature("BrawlerACBonusUnlock",
                                             ac_bonus_feature.Name,
                                             ac_bonus_feature.Description,
                                             "",
                                             ac_bonus_feature.Icon,
                                             FeatureGroup.None,
                                             Helpers.Create<WeaponTrainingMechanics.AddFeatureOnArmor>(a =>
                                             {
                                                 a.feature = ac_bonus_feature;
                                                 a.required_armor = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.Light, ArmorProficiencyGroup.None };
                                             })
                                             );

        }


        static void createManeuverTraining()
        {
            var maneuvers = new CombatManeuver[][] { new CombatManeuver[] { CombatManeuver.BullRush },
                                                     new CombatManeuver[] {CombatManeuver.Disarm },
                                                     new CombatManeuver[] {CombatManeuver.Trip },
                                                     new CombatManeuver[] {CombatManeuver.SunderArmor },
                                                     new CombatManeuver[] {CombatManeuver.DirtyTrickBlind, CombatManeuver.DirtyTrickEntangle, CombatManeuver.DirtyTrickSickened },
                                                      new CombatManeuver[] {CombatManeuverTypeExtender.AwesomeBlow.ToCombatManeuverType() },
                                                   };

            var names = new string[] { "Bull Rush", "Disarm", "Trip", "Sunder", "Dirty Trick", "Awesome Blow" };
            var icons = new UnityEngine.Sprite[]
            {
                library.Get<BlueprintFeature>("b3614622866fe7046b787a548bbd7f59").Icon,
                library.Get<BlueprintFeature>("25bc9c439ac44fd44ac3b1e58890916f").Icon,
                library.Get<BlueprintFeature>("0f15c6f70d8fb2b49aa6cc24239cc5fa").Icon,
                library.Get<BlueprintFeature>("9719015edcbf142409592e2cbaab7fe1").Icon,
                library.Get<BlueprintFeature>("ed699d64870044b43bb5a7fbe3f29494").Icon,
                library.Get<BlueprintFeature>("bdf58317985383540920c723db07aa3b").Icon, //pummeling bully
            };

            for (int i = 0; i < maneuver_training.Length; i++)
            {
                maneuver_training[i] = Helpers.CreateFeatureSelection($"ManeuverTraining{i + 1}FeatureSelection",
                                                                   "Maneuver Training ",
                                                                   "At 3rd level, a brawler can select one combat maneuver to receive additional training. She gains a +1 bonus on combat maneuver checks when performing that combat maneuver and a +1 bonus to her CMD when defending against that maneuver.\n"
                                                                   + "At 7th level and every 4 levels thereafter, the brawler becomes further trained in another combat maneuver, gaining the above +1 bonus combat maneuver checks and to CMD. In addition, the bonuses granted by all previous maneuver training increase by 1 each.",
                                                                   "",
                                                                   null,
                                                                   FeatureGroup.None);
                maneuver_training[i].AllFeatures = new BlueprintFeature[0];

                int lvl = 3 + i * 4;
                for (int j = 0; j < maneuvers.Length; j++)
                {
                    var feat = Helpers.CreateFeature($"ManeuverTraining{i+1}" + maneuvers[j][0].ToString() + "Feature",
                                                     maneuver_training[i].Name + ": " + names[j] + $" ({lvl}{Common.getNumExtension(lvl)} level)",
                                                     maneuver_training[i].Description,
                                                     "",
                                                     icons[j],
                                                     FeatureGroup.None,
                                                     Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                     classes: getBrawlerArray(),
                                                                                     progression: ContextRankProgression.StartPlusDivStep,
                                                                                     startLevel: lvl,
                                                                                     stepLevel: 4),
                                                     Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureList,
                                                                                     type: AbilityRankType.StatBonus,
                                                                                     featureList: new BlueprintFeature[] {perfect_warrior, perfect_warrior}
                                                                                     )
                                                     );

                    foreach (var maneuver in maneuvers[j])
                    {
                        feat.AddComponents(Helpers.Create<CombatManeuverMechanics.SpecificCombatManeuverBonus>(s => { s.maneuver_type = maneuver; s.Value = Helpers.CreateContextValue(AbilityRankType.Default); }),
                                           Common.createContextManeuverDefenseBonus(maneuver, Helpers.CreateContextValue(AbilityRankType.Default)),
                                           Helpers.Create<CombatManeuverMechanics.SpecificCombatManeuverBonus>(s => { s.maneuver_type = maneuver; s.Value = Helpers.CreateContextValue(AbilityRankType.StatBonus); }),
                                           Common.createContextManeuverDefenseBonus(maneuver, Helpers.CreateContextValue(AbilityRankType.StatBonus))
                                           );
                    }

                    if (j + 1 != maneuvers.Length)
                    {
                        for (int k = 0; k < i; k++)
                        {
                            feat.AddComponent(Helpers.PrerequisiteNoFeature(maneuver_training[k].AllFeatures[j]));
                        }
                    }
                    if (j + 1 != maneuvers.Length || i + 1 == maneuver_training.Length)
                    {
                        maneuver_training[i].AllFeatures = maneuver_training[i].AllFeatures.AddToArray(feat);
                    }
                }
            }
        }


        static void createBrawlersFlurry()
        {
            var flurry1_monk = library.Get<BlueprintFeature>("332362f3bd39ebe46a740a36960fdcb4");
            var flurry11_monk = library.Get<BlueprintFeature>("de25523acc24b1448aa90f74d6512a08");
            flurry1_monk.Ranks++;
            flurry11_monk.Ranks++;


            brawlers_flurry = Helpers.CreateFeature("BrawlersFlurryUnlock",
                                                    "Brawler’s Flurry",
                                                    "At 2nd level, a brawler can make a flurry of blows as a full attack. When making a flurry of blows, the brawler can make one additional attack at his highest base attack bonus. This additional attack stacks with the bonus attacks from haste and other similar effects. When using this ability, the brawler can make these attacks with any combination of his unarmed strikes, weapons from the close fighter weapon group, or weapons with the monk special weapon quality. He takes no penalty for using multiple weapons when making a flurry of blows, but he does not gain any additional attacks beyond what's already granted by the flurry for doing so. (He can still gain additional attacks from a high base attack bonus, from this ability, and from haste and similar effects).\nAt 11th level, a brawler can make an additional attack at his highest base attack bonus whenever he makes a flurry of blows. This stacks with the first attack from this ability and additional attacks from haste and similar effects.",
                                                    "",
                                                    null,
                                                    FeatureGroup.None,
                                                    Helpers.Create<FeralCombatTraining.SpecificWeaponGroupOrFeralCombatFeatureUnlock>(s =>
                                                    {
                                                        s.NewFact = flurry1_monk;
                                                        s.weapon_groups = new WeaponFighterGroup[] { WeaponFighterGroup.Monk, WeaponFighterGroup.Close };
                                                    })
                                                    );

            brawlers_flurry11 = library.CopyAndAdd(brawlers_flurry, "BrawlersFlurry11Unlock", "");
            brawlers_flurry11.ReplaceComponent<FeralCombatTraining.SpecificWeaponGroupOrFeralCombatFeatureUnlock>(f => f.NewFact = flurry11_monk);


            var buff = Helpers.CreateBuff("BrawlerTwoWeaponFlurryEnabledBuff",
                                          "Brawler’s Flurry: Use Off-Hand attacks",
                                          brawlers_flurry.Description,
                                          "",
                                          Helpers.GetIcon("ac8aaf29054f5b74eb18f2af950e752d"), //twf
                                          null,
                                          Helpers.Create<ActivateBrawlerTwoWeaponFlurry>()
                                          );

            var toggle = Common.buffToToggle(buff, CommandType.Free, false);

            brawlers_flurry.AddComponents(Helpers.Create<AddBrawlerTwoWeaponFlurryPart>(a => a.groups = new WeaponFighterGroup[] { WeaponFighterGroup.Monk, WeaponFighterGroup.Close }),
                              Helpers.Create<BrawlerTwoWeaponFlurryExtraAttack>(),
                              Helpers.CreateAddFact(toggle)
                             );
            brawlers_flurry11.AddComponents(Helpers.Create<BrawlerTwoWeaponFlurryExtraAttack>());

            brawlers_flurry.IsClassFeature = true;
            brawlers_flurry11.IsClassFeature = true;

            var pummeling_style = library.Get<BlueprintFeature>("c36562b8e7ae12d408487ba8b532d966");
            pummeling_style.AddComponent(Helpers.PrerequisiteFeature(brawlers_flurry, any: true));
        }

        static void createBrawlersCunning()
        {
            brawlers_cunning = Helpers.CreateFeature("BrawlersCunningFeature",
                                                     "Brawler’s Cunning",
                                                     "If the brawler’s Intelligence score is less than 13, it counts as 13 for the purpose of meeting the prerequisites of combat feats.",
                                                     "",
                                                     Helpers.GetIcon("ae4d3ad6a8fda1542acf2e9bbc13d113"), //foxs cunning
                                                     FeatureGroup.None,
                                                     Helpers.Create<ReplaceStatForPrerequisites>(r => { r.OldStat = StatType.Intelligence; r.SpecificNumber = 13; r.Policy = ReplaceStatForPrerequisites.StatReplacementPolicy.SpecificNumber; })
                                                     );
        }


        static void createMartialTraining()
        {
            var monk_class = library.Get<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");
            var fighter_class = library.Get<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");

            martial_training = Helpers.CreateFeature("BrawlerMartialTrainingFeat",
                                         "Martial Training",
                                         "At 1st level, a brawler counts her total brawler levels as both fighter levels and monk levels for the purpose of qualifying for feats. She also counts as both a fighter and a monk for feats and magic items that have different effects based on whether the character has levels in those classes (such as Stunning Fist and a monk’s robe). This ability does not automatically grant feats normally granted to fighters and monks based on class level, namely Stunning Fist.",
                                         "",
                                         null,
                                         FeatureGroup.None,
                                         Common.createClassLevelsForPrerequisites(monk_class, brawler_class),
                                         Common.createClassLevelsForPrerequisites(fighter_class, brawler_class)
                                         );

            var stunning_fist_resource = library.Get<BlueprintAbilityResource>("d2bae584db4bf4f4f86dd9d15ae56558");
            ClassToProgression.addClassToResource(brawler_class, new BlueprintArchetype[0], stunning_fist_resource, monk_class);
        }


        public class UnitPartBrawler : UnitPart
        {
            [JsonProperty]
            private bool active;
            [JsonProperty]
            private int extra_attacks = 0;
            [JsonProperty]
            private WeaponFighterGroup[] weapon_groups;

            public void initialize(params WeaponFighterGroup[] brawler_weapon_groups)
            {
                weapon_groups = brawler_weapon_groups;
            }

            public bool isActive()
            {
                return active;
            }

            public void activate()
            {
                active = true;
                EventBus.RaiseEvent<IUnitActiveEquipmentSetHandler>((Action<IUnitActiveEquipmentSetHandler>)(h => h.HandleUnitChangeActiveEquipmentSet(this.Owner)));
            }

            public void deactivate()
            {
                active = false;
                EventBus.RaiseEvent<IUnitActiveEquipmentSetHandler>((Action<IUnitActiveEquipmentSetHandler>)(h => h.HandleUnitChangeActiveEquipmentSet(this.Owner)));
            }

            public void increaseExtraAttacks()
            {
                extra_attacks++;
            }

            public void decreaseExtraAttacks()
            {
                extra_attacks--;
            }

            public int getNumExtraAttacks()
            {
                return extra_attacks;
            }


            public bool checkTwoWeapponFlurry()
            {
                if (!isActive())
                {
                    return false;
                }

                if (!Owner.Body.PrimaryHand.HasWeapon || !Owner.Body.SecondaryHand.HasWeapon)
                {
                    return false;
                }

                var weapon1 = Owner.Body.PrimaryHand.MaybeWeapon;
                var weapon2 = Owner.Body.SecondaryHand.MaybeWeapon;

                return (weapon_groups.Contains(weapon1.Blueprint.FighterGroup)
                      || FeralCombatTraining.checkHasFeralCombat(this.Owner.Unit, weapon1, allow_crusaders_flurry: true))
                    && (weapon_groups.Contains(weapon2.Blueprint.FighterGroup)
                        || FeralCombatTraining.checkHasFeralCombat(this.Owner.Unit, weapon2, allow_crusaders_flurry: true));
            }
        }



        //fix twf to work correctly with and brawlers flurry
        [Harmony12.HarmonyPriority(Harmony12.Priority.First)]
        [Harmony12.HarmonyPatch(typeof(TwoWeaponFightingAttacks))]
        [Harmony12.HarmonyPatch("OnEventAboutToTrigger", Harmony12.MethodType.Normal)]
        [Harmony12.HarmonyPatch(new Type[] { typeof(RuleCalculateAttacksCount) })]
        class TwoWeaponFightingAttacks__OnEventAboutToTrigger__Patch
        {
            static bool Prefix(TwoWeaponFightingAttacks __instance, RuleCalculateAttacksCount evt)
            {
                var maybeWeapon1 = evt.Initiator.Body.PrimaryHand?.MaybeWeapon;
                var maybeWeapon2 = evt.Initiator.Body.SecondaryHand?.MaybeWeapon;
                if (!evt.Initiator.Body.PrimaryHand.HasWeapon
                    || !evt.Initiator.Body.SecondaryHand.HasWeapon
                       || (maybeWeapon1.Blueprint.IsNatural && (!maybeWeapon1.Blueprint.IsUnarmed || HoldingItemsMechanics.Aux.isMainHandUnarmedAndCanBeIgnored(maybeWeapon1.Blueprint, evt.Initiator.Descriptor)))
                       || (maybeWeapon2.Blueprint.IsNatural && (!maybeWeapon2.Blueprint.IsUnarmed || HoldingItemsMechanics.Aux.isOffHandUnarmedAndCanBeIgnored(maybeWeapon2.Blueprint, evt.Initiator.Descriptor)))
                    )
                    return false;

                var brawler_part = evt.Initiator.Get<Brawler.UnitPartBrawler>();
                if ((brawler_part?.checkTwoWeapponFlurry()).GetValueOrDefault())
                {
                    for (int i = 1; i < brawler_part.getNumExtraAttacks(); i++)
                    {
                        ++evt.SecondaryHand.MainAttacks;
                    }
                }
                else if (__instance.Fact.GetRank() > 1)
                {
                    for (int i = 2; i < __instance.Fact.GetRank(); i++)
                    {
                        ++evt.SecondaryHand.PenalizedAttacks;
                    }
                }

                return false;
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class AddBrawlerTwoWeaponFlurryPart : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
        {
            public WeaponFighterGroup[] groups;

            public override void OnFactActivate()
            {
                this.Owner.Ensure<UnitPartBrawler>().initialize(groups);
            }


            public override void OnFactDeactivate()
            {
                this.Owner.Remove<UnitPartBrawler>();
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class BrawlerTwoWeaponFlurryExtraAttack : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
        {
            public override void OnFactActivate()
            {
                this.Owner.Get<UnitPartBrawler>()?.increaseExtraAttacks();
            }


            public override void OnFactDeactivate()
            {
                this.Owner.Get<UnitPartBrawler>()?.decreaseExtraAttacks();
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ActivateBrawlerTwoWeaponFlurry : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
        {

            public override void OnTurnOn()
            {
                this.Owner.Get<UnitPartBrawler>()?.activate();
            }


            public override void OnTurnOff()
            {
                this.Owner.Get<UnitPartBrawler>()?.deactivate();
            }
        }
    }
}
