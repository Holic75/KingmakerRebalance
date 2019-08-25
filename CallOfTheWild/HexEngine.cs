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
    class HexEngine
    {
        static LibraryScriptableObject library => Main.library;
        static internal bool test_mode = false;

       
        static public BlueprintAbility hex_vulnerability_spell;
        static public BlueprintFeature accursed_hex_feat;
        static BlueprintBuff hex_vulnerability_buff;
        static BlueprintBuff accursed_hex_buff;

        static public BlueprintFeature split_hex_feat;
        static BlueprintBuff immune_to_split_hex_buff;

        static public BlueprintFeature amplified_hex_feat;
        static BlueprintBuff amplified_hex_buff;
        static private BlueprintAbility[] amplified_hex_sacrifice_spells = new BlueprintAbility[10];

        private BlueprintCharacterClass[] hex_classes;
        private StatType hex_stat;
        private List<BlueprintBuff> cackle_buffs = new List<BlueprintBuff>();


        public static void Initialize()
        {
            createHexVulnerabilitySpellAndAccursedHexFeat();
            createAmplifiedHex();
            createSplitHex();
            Main.logger.Log("Hex Engine test mode: " + test_mode.ToString());
        }

        public HexEngine(BlueprintCharacterClass[] scaling_classes, StatType scaling_stat)
        {
            hex_classes = scaling_classes;
            hex_stat = scaling_stat;
            foreach (var c in hex_classes)
            {
                amplified_hex_feat.AddComponent(Helpers.PrerequisiteClassLevel(c, 1, true));
                amplified_hex_feat.AddComponent(Common.createSpontaneousSpellConversion(c, amplified_hex_sacrifice_spells));
                accursed_hex_feat.AddComponent(Helpers.PrerequisiteClassLevel(c, 1, true));

            }
            //manually add hex_vilneraility to all necessary spellbooks
            //and create scroll once
        }


        void addMajorHexPrerequisite(BlueprintFeature hex_feature)
        {
            foreach (var c in hex_classes)
            {
               hex_feature.AddComponent(Helpers.PrerequisiteClassLevel(c, 10, true));
            }
        }


        void addGrandHexPrerequisite(BlueprintFeature hex_feature)
        {
            foreach (var c in hex_classes)
            {
                hex_feature.AddComponent(Helpers.PrerequisiteClassLevel(c, 18, true));
            }
        }


        void addWitchHexCooldownScaling(BlueprintAbility ability, BlueprintBuff hex_cooldown, bool allow_hex_vulnerability = true)
        {
            var cooldown_action = Helpers.Create<Kingmaker.UnitLogic.Mechanics.Actions.ContextActionApplyBuff>();
            cooldown_action.Buff = hex_cooldown;
            cooldown_action.AsChild = true;
            cooldown_action.IsNotDispelable = true;
            //cooldown_action.IsFromSpell = true;
            var duration = Helpers.CreateContextValue(AbilityRankType.Default);
            duration.ValueType = ContextValueType.Simple;
            duration.Value = 1;
            cooldown_action.DurationValue = Helpers.CreateContextDuration(bonus: duration,
                                                                            rate: DurationRate.Days);
            cooldown_action.IsNotDispelable = true;

            bool has_action = (ability.GetComponents<AbilityEffectRunAction>().Count() != 0);
            if (!has_action)
            {
                Main.logger.Log("Warning: no action on " + ability.name + " while trying to create hex cooldown");
                var action = Helpers.Create<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>();
                action.addAction(cooldown_action);
                ability.AddComponent(action);
            }
            else
            {
                ability.ReplaceComponent<AbilityEffectRunAction>(ability.GetComponent<AbilityEffectRunAction>().CreateCopy());
                ability.GetComponent<AbilityEffectRunAction>().addAction(cooldown_action);
            }


            if (allow_hex_vulnerability)
            {
                //if enemy and caster has accursed hex, apply hex_vulnerability for 1 round
                var accursed_hex_personalized = library.CopyAndAdd<BlueprintBuff>(accursed_hex_buff.AssetGuid, ability.name + "AccursedHexBuff", "");
                accursed_hex_personalized.SetName(accursed_hex_buff.Name + " : " + ability.Name);
                var test_condition = new Condition[] { Helpers.CreateConditionCasterHasFact(accursed_hex_feat), Common.createContextConditionHasBuffFromCaster(accursed_hex_personalized, true) };
                var release_condtion = test_condition.AddToArray(Helpers.Create<ContextConditionIsEnemy>());
                var accursed_hex_action = Common.createContextActionApplyBuff(accursed_hex_personalized, Helpers.CreateContextDuration(),
                                                                              dispellable: false, duration_seconds: 9); //set duration to 9 seconds to simulate "until end of your turn"
                var accursed_hex_conditional = Helpers.CreateConditional(test_mode ? test_condition : release_condtion,
                                                              Helpers.CreateConditionalSaved(accursed_hex_action,
                                                              Common.createContextActionRemoveBuffFromCaster(accursed_hex_personalized)),
                                                              Common.createContextActionRemoveBuffFromCaster(accursed_hex_personalized)
                                                            );
                var found_save = false;
                var run_actions = ability.GetComponent<AbilityEffectRunAction>().Actions.Actions;
                for (int i = 0; i < run_actions.Length; i++)
                {
                    var a = run_actions[i];
                    if (a is ContextActionSavingThrow)
                    {
                        var new_action = (a as ContextActionSavingThrow).CreateCopy();
                        new_action.Actions = Helpers.CreateActionList(new_action.Actions.Actions.AddToArray(accursed_hex_conditional));
                        run_actions[i] = new_action;
                        found_save = true;
                        break;
                    }
                }
                
                if (!found_save)
                {
                    run_actions = run_actions.AddToArray(accursed_hex_conditional);
                }

                ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(run_actions));

                var target_checker = Common.createAbilityTargetHasNoFactUnlessBuffsFromCaster(new BlueprintBuff[] { hex_cooldown },
                                                                          new BlueprintBuff[] { accursed_hex_personalized, hex_vulnerability_buff }//allow to to reapply if has hex vulnerability
                                                                          );
                ability.AddComponent(target_checker);
            }



            var scaling = Common.createContextCalculateAbilityParamsBasedOnClasses(hex_classes, hex_stat);
            ability.AddComponent(scaling);
            var spell_list_components = ability.GetComponents<Kingmaker.Blueprints.Classes.Spells.SpellListComponent>().ToArray();
            foreach (var c in spell_list_components)
            {
                ability.RemoveComponent(c);
            }
            ability.Type = AbilityType.Supernatural;
            ability.SpellResistance = false;
            var spell_components = ability.GetComponents<Kingmaker.Blueprints.Classes.Spells.SpellComponent>().ToArray();
            foreach (var s in spell_components)
            {
                ability.RemoveComponent(s);
            }
            ability.RemoveComponent(ability.GetComponent<Kingmaker.Blueprints.Classes.Spells.ChirurgeonSpell>());
            ability.RemoveComponent(ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityUseOnRest>());
            //ability.AvailableMetamagic = 0;
        }


        BlueprintBuff addWitchHexCooldownScaling(BlueprintAbility ability, string buff_guid, string name = "", bool allow_hex_vulnerability = true)
        {
            var hex_cooldown = Helpers.CreateBuff(ability.name + "CooldownBuff",
                                                                     name == "" ? "Cooldown " + ability.Name : name,
                                                                     ability.Description,
                                                                     buff_guid,
                                                                     ability.Icon,
                                                                     null);
            hex_cooldown.SetBuffFlags(BuffFlags.RemoveOnRest | BuffFlags.StayOnDeath);
            hex_cooldown.Frequency = DurationRate.Rounds;
            hex_cooldown.Stacking = StackingType.Replace;
            addWitchHexCooldownScaling(ability, hex_cooldown, allow_hex_vulnerability);
            return hex_cooldown;
        }



        public BlueprintFeature createHealing(string name_prefix, string display_name, string description, string abil1_guid, string abil2_guid, 
                                              string feature1_guid, string feature2_guid, string feature_guid, string cooldown_guid)
        {
            var healing = createHealingHex(name_prefix + "Hex", display_name,
                                        description,
                                        "47808d23c67033d4bbab86a1070fd62f", //cure light wounds
                                        "1c1ebf5370939a9418da93176cc44cd9", //cure moderate wounds
                                        abil1_guid,
                                        abil2_guid,
                                        feature1_guid,
                                        feature2_guid,
                                        feature_guid,
                                        cooldown_guid,
                                        5);
            return healing;
        }


        public BlueprintFeature createBeastOfIllOmen(string name_prefix, string display_name, string description, string ability_guid, string feature_guid, string cooldown_guid)
        {
            var doom_spell = library.Get<BlueprintAbility>("fbdd8c455ac4cde4a9a3e18c84af9485");
            var hex_ability = library.CopyAndAdd<BlueprintAbility>("8bc64d869456b004b9db255cdd1ea734", name_prefix + "HexAbility", ability_guid);
            hex_ability.SetName(display_name);
            hex_ability.SetDescription(description);
            hex_ability.Range = AbilityRange.Medium;
            hex_ability.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Point;
            hex_ability.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionPoint;
            hex_ability.RemoveComponent(hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityTargetsAround>());
            hex_ability.RemoveComponent(hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.Base.AbilitySpawnFx>());
            hex_ability.RemoveComponent(hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.Base.AbilitySpawnFx>());
            hex_ability.AddComponent(doom_spell.GetComponent<Kingmaker.UnitLogic.Abilities.Components.Base.AbilitySpawnFx>());

            var hex_cooldown = addWitchHexCooldownScaling(hex_ability, cooldown_guid);
            var beast_of_ill_omen = Helpers.CreateFeature(name_prefix + "Feature",
                                                      hex_ability.Name,
                                                      hex_ability.Description,
                                                      feature_guid,
                                                      hex_ability.Icon,
                                                      FeatureGroup.None,
                                                      Helpers.CreateAddFact(hex_ability));
            beast_of_ill_omen.Ranks = 1;
            addToAmplifyHex(hex_ability);
            addToSplitHex(hex_ability, true);
            return beast_of_ill_omen;
        }


        public BlueprintFeature createSlumber(string name_prefix, string display_name, string description, string ability_guid, string feature_guid, string cooldown_guid)
        {
            var sleep_spell = library.Get<BlueprintAbility>("bb7ecad2d3d2c8247a38f44855c99061");
            var dominate_spell = library.Get<BlueprintAbility>("3c17035ec4717674cae2e841a190e757");
            var sleep_buff = library.Get<BlueprintBuff>("c9937d7846aa9ae46991e9f298be644a");
            var hex_ability = Helpers.CreateAbility(name_prefix + "HexAbility",
                                                    display_name,
                                                    description,
                                                    ability_guid,
                                                    sleep_buff.Icon,
                                                    AbilityType.Supernatural,
                                                    CommandType.Standard,
                                                    AbilityRange.Close,
                                                    sleep_spell.LocalizedDuration,
                                                    sleep_spell.LocalizedSavingThrow);
            hex_ability.CanTargetPoint = false;
            hex_ability.CanTargetEnemies = true;
            hex_ability.CanTargetFriends = test_mode;
            hex_ability.CanTargetSelf = test_mode;
            hex_ability.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Point;
            hex_ability.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionPoint;
            hex_ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            var target_checker = Helpers.Create<Kingmaker.UnitLogic.Abilities.Components.TargetCheckers.AbilityTargetHasFact>();
            target_checker.CheckedFacts = new BlueprintUnitFact[] { library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f"), //construct
                                                                    library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33") //undead
                                                                  };
            target_checker.Inverted = true;
            hex_ability.AddComponent(target_checker);
            hex_ability.AddComponent(dominate_spell.GetComponent<Kingmaker.UnitLogic.Abilities.Components.Base.AbilitySpawnFx>());
           
            var action = (Common.createContextActionSavingThrow(SavingThrowType.Will, 
                          Helpers.CreateActionList(Common.createContextSavedApplyBuff(sleep_buff, DurationRate.Rounds, is_from_spell: true, is_dispellable: false))));
            hex_ability.AddComponent(Helpers.CreateRunActions(action));
            hex_ability.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: hex_classes));
            hex_ability.AddComponent(sleep_spell.GetComponent<Kingmaker.Blueprints.Classes.Spells.SpellDescriptorComponent>());
            var hex_cooldown = addWitchHexCooldownScaling(hex_ability, cooldown_guid);
            var slumber_hex = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                      hex_ability.Name,
                                                      hex_ability.Description,
                                                      feature_guid,
                                                      hex_ability.Icon,
                                                      FeatureGroup.None,
                                                      Helpers.CreateAddFact(hex_ability));
            slumber_hex.Ranks = 1;
            addToAmplifyHex(hex_ability);
            addToSplitHex(hex_ability, true);
            return slumber_hex;
        }


        public BlueprintFeature createMisfortune(string name_prefix, string display_name, string description, string ability_guid, string buff_guid, string feature_guid, string cooldown_guid)
        {
            var doom_spell = library.Get<BlueprintAbility>("fbdd8c455ac4cde4a9a3e18c84af9485");
            var hex_ability = library.CopyAndAdd<BlueprintAbility>("ca1a4cd28737ae544a0a7e5415c79d9b", name_prefix + "HexAbility", ability_guid); //touch of chaos as base

            hex_ability.SetName(display_name);
            hex_ability.LocalizedDuration = Helpers.CreateString(name_prefix + "HexAbility.Duration", "Variable");
            hex_ability.SetDescription(description);
            hex_ability.Range = AbilityRange.Close;
            hex_ability.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Point;
            hex_ability.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionPoint;
            hex_ability.RemoveComponent(hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityDeliverTouch>());
            hex_ability.RemoveComponent(hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityResourceLogic>());
 
            var hex_buff = library.CopyAndAdd<BlueprintBuff>("96bbd279e0bed0f4fb208a1761f566b5", "Witch" + name_prefix + "HexBuff", buff_guid);
            hex_buff.SetName(hex_ability.Name);
            hex_buff.SetDescription(hex_ability.Description);
            cackle_buffs.Add(hex_buff);
            var action = Common.createContextActionSavingThrow(SavingThrowType.Will, 
                                                               Helpers.CreateActionList(Common.createContextSavedApplyBuff(hex_buff, DurationRate.Rounds, AbilityRankType.DamageBonus, is_dispellable: false)));

            hex_ability.ReplaceComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>(Helpers.CreateRunActions(action));
            hex_ability.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.OnePlusDivStep,
                                                                     type: AbilityRankType.DamageBonus,
                                                                     min: 1,
                                                                     startLevel: 0,
                                                                     stepLevel: 8,
                                                                     classes: hex_classes));
            hex_ability.AddComponent(doom_spell.GetComponent<Kingmaker.UnitLogic.Abilities.Components.Base.AbilitySpawnFx>());
            var hex_cooldown = addWitchHexCooldownScaling(hex_ability, cooldown_guid);
            var misfortune_hex = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                      hex_ability.Name,
                                                      hex_ability.Description,
                                                      feature_guid,
                                                      hex_ability.Icon,
                                                      FeatureGroup.None,
                                                      Helpers.CreateAddFact(hex_ability));
            misfortune_hex.Ranks = 1;
            addToAmplifyHex(hex_ability);
            addToSplitHex(hex_ability, true);
            return misfortune_hex;
        }


        public BlueprintFeature createFortuneHex(string name_prefix, string display_name, string description, string ability_guid, string buff_guid, string feature_guid, string cooldown_guid)
        {
            var hex_ability = library.CopyAndAdd<BlueprintAbility>("9af0b584f6f754045a0a79293d100ab3", name_prefix + "HexAbility", ability_guid); //bit of luck
            hex_ability.SetName(display_name);
            hex_ability.LocalizedDuration = Helpers.CreateString(name_prefix + "HexAbility.Duration", "Variable");
            hex_ability.SetDescription(description);
            hex_ability.RemoveComponent(hex_ability.GetComponent<Kingmaker.Designers.Mechanics.Facts.ReplaceAbilitiesStat>());
            hex_ability.RemoveComponent(hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityResourceLogic>());
            hex_ability.Range = AbilityRange.Close;
            var action = Helpers.Create<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>();
            var apply_buff = (Kingmaker.UnitLogic.Mechanics.Actions.ContextActionApplyBuff)hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>().Actions.Actions[0].CreateCopy();
            apply_buff.Buff = library.CopyAndAdd<BlueprintBuff>(apply_buff.Buff, "Witch" + name_prefix + "HexBuff", buff_guid);
            apply_buff.IsNotDispelable = true;
            cackle_buffs.Add(apply_buff.Buff);

            var bonus_value = Helpers.CreateContextValue(AbilityRankType.DamageBonus);
            bonus_value.Value = 1;
            bonus_value.ValueType = ContextValueType.Rank;
            apply_buff.DurationValue = Helpers.CreateContextDuration(bonus: bonus_value);
            apply_buff.DurationValue.Rate = DurationRate.Rounds;
            action.Actions = Helpers.CreateActionList(hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>().Actions.Actions[0].CreateCopy());
            action.Actions.Actions = new Kingmaker.ElementsSystem.GameAction[] { apply_buff };
            hex_ability.RemoveComponent(hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>());
            hex_ability.AddComponent(action);
            var context_rank = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.OnePlusDivStep,
                                                              type: AbilityRankType.DamageBonus, min: 1, max: 3, startLevel: 0, stepLevel: 8,
                                            classes: hex_classes);
            hex_ability.AddComponent(context_rank);
            var hex_cooldown = addWitchHexCooldownScaling(hex_ability, cooldown_guid);
            var fortune_hex = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                      hex_ability.Name,
                                                      hex_ability.Description,
                                                      feature_guid,
                                                      hex_ability.Icon,
                                                      FeatureGroup.None,
                                                      Helpers.CreateAddFact(hex_ability));
            fortune_hex.Ranks = 1;
            addToSplitHex(hex_ability);
            return fortune_hex;
        }


        public BlueprintFeature createIceplantHex(string name_prefix, string display_name, string description, string feature_guid)
        {
            var frigid_touch = library.Get<BlueprintAbility>("c83447189aabc72489164dfc246f3a36");
            var iceplant_hex = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  display_name,
                                                  description,
                                                  feature_guid,
                                                  frigid_touch.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddStatBonus(StatType.AC, 2, ModifierDescriptor.NaturalArmor)
                                                  );
            iceplant_hex.Ranks = 1;
            return iceplant_hex;
        }


        public BlueprintFeature createMurksightHex(string name_prefix, string display_name, string description, string feature_guid)
        {
            var remove_blindness = library.Get<BlueprintAbility>("c927a8b0cd3f5174f8c0b67cdbfde539");
            var murksight_hex = Helpers.CreateFeature(name_prefix + "HexFeature",
                                  display_name,
                                  description,
                                  feature_guid,
                                   remove_blindness.Icon,
                                   FeatureGroup.None,
                                   Common.createBlindsight(15));
            murksight_hex.Ranks = 1;
            return murksight_hex;
        }


        public BlueprintFeature createAmeliorating(string name_prefix, string display_name, string description, string abil1_guid, string abil2_guid,
                                                   string feature_guid, string buff1_guid, string buff2_guid, string cooldown_guid)
        {
            var hex_ability1 = library.CopyAndAdd<BlueprintAbility>("f6f95242abdfac346befd6f4f6222140", name_prefix + "HexImmAbility", abil1_guid);
            hex_ability1.LocalizedDuration = Helpers.minutesPerLevelDuration;
            hex_ability1.SetName($"{display_name}: Suppress Condition");
            hex_ability1.SetDescription(description);
            hex_ability1.RemoveComponent(hex_ability1.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>());
            hex_ability1.Range = AbilityRange.Touch;
            hex_ability1.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Touch;
            hex_ability1.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionOmni;
            var hex_ability2 = library.CopyAndAdd<BlueprintAbility>("414d6c8fd5fc46c5a83b596a9bcf3322", name_prefix + "HexSaveAbility", abil2_guid);
            var hex_ability1_buff = Helpers.CreateBuff(name_prefix + "HexImmBuff", hex_ability1.Name, hex_ability1.Description, buff1_guid,
                                                       hex_ability1.Icon, null,
                                                       Common.createAddConditionImmunity(UnitCondition.Sickened),
                                                       Common.createAddConditionImmunity(UnitCondition.Dazzled),
                                                       Common.createAddConditionImmunity(UnitCondition.Fatigued),
                                                       Common.createAddConditionImmunity(UnitCondition.Shaken)
                                                       );
            var action = Helpers.Create<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>();
            var bonus_value = Helpers.CreateContextValue(AbilityRankType.Default);
            bonus_value.ValueType = ContextValueType.Rank;
            var duration_value = Helpers.CreateContextDuration(bonus: bonus_value, rate: DurationRate.Minutes);
            action.Actions = Helpers.CreateActionList(Helpers.CreateApplyBuff(hex_ability1_buff, duration_value, true, dispellable: false));
            hex_ability1.AddComponent(action);

            hex_ability2.SetName($"{display_name}: Saving Throws Bonus");
            hex_ability2.LocalizedDuration = Helpers.CreateString(name_prefix + "HexSaveAbility.Duration", "24 hours");
            var hex_ability2_buff = Helpers.CreateBuff(name_prefix + "HexSaveBuff", hex_ability2.Name, hex_ability2.Description, buff2_guid,
                                                       hex_ability2.Icon, null,
                                                       Common.createSavingThrowBonusAgainstDescriptor(4, ModifierDescriptor.Circumstance,
                                                                SpellDescriptor.Fatigue | SpellDescriptor.Shaken | SpellDescriptor.Sickened | SpellDescriptor.Blindness)
                                                      );
            hex_ability2_buff.SetBuffFlags(BuffFlags.RemoveOnRest);
            var action2 = Helpers.Create<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>();
            var bonus_value2 = Helpers.CreateContextValue(AbilityRankType.DamageBonus);
            bonus_value2.ValueType = ContextValueType.Simple;
            bonus_value2.Value = 1;
            var duration_value2 = Helpers.CreateContextDuration(bonus: bonus_value2, rate: DurationRate.Days);
            action2.Actions = Helpers.CreateActionList(Helpers.CreateApplyBuff(hex_ability2_buff, duration_value2, true, dispellable: false));
            hex_ability2.AddComponent(action2);
            var hex_cooldown = addWitchHexCooldownScaling(hex_ability1, "b58d93d94e834ada903a91d8cf46d650", $"Cooldown {display_name}");
            addWitchHexCooldownScaling(hex_ability2, hex_cooldown);

            var ameliorating = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                      display_name,
                                                      hex_ability1.Description,
                                                      feature_guid,
                                                      hex_ability1.Icon,
                                                      FeatureGroup.None,
                                                      Helpers.CreateAddFacts(hex_ability1, hex_ability2));
            ameliorating.Ranks = 1;
            addToSplitHex(hex_ability1);
            addToSplitHex(hex_ability2);
            return ameliorating;
        }


        public BlueprintFeature createEvilEye(string name_prefix, string display_name, string description, string abil_guid, string abil1_guid, string abil2_guid, string abil3_guid,
                                              string feature_guid, string buff1_guid, string buff2_guid, string buff3_guid)
        {
            var eyebyte = library.Get<BlueprintAbility>("3167d30dd3c622c46b0c0cb242061642");

            var context_value = Helpers.CreateContextValue(AbilityRankType.StatBonus);
            var context_rank_config = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.StartPlusDivStep,
                                            type: AbilityRankType.StatBonus, startLevel: 0, stepLevel: 8, min: 1, max: 2, classes: hex_classes);

            StatType[] stats = new StatType[] { StatType.AC, StatType.AdditionalAttackBonus, StatType.SaveFortitude, StatType.SaveReflex, StatType.SaveWill };
            AddContextStatBonus[] penalties = new AddContextStatBonus[stats.Length];
            for (int i = 0; i < stats.Length; i++)
            {
                penalties[i] = Helpers.CreateAddContextStatBonus(stats[i], ModifierDescriptor.None, ContextValueType.Rank, AbilityRankType.StatBonus);
                penalties[i].Multiplier = -2;
            }

            BlueprintAbility[] evil_eye_variants = new BlueprintAbility[3];
            evil_eye_variants[0] = createEvilEyeComponent(name_prefix + "ACHex", $"{display_name} (AC Penalty)", description, abil1_guid, buff1_guid,
                                                     eyebyte.Icon, eyebyte.GetComponent<AbilitySpawnFx>().PrefabLink, penalties[0], context_rank_config);
            evil_eye_variants[1] = createEvilEyeComponent(name_prefix + "AttackHex", $"{display_name} (Attack Rolls Penalty)", description, abil2_guid, buff2_guid,
                                                     eyebyte.Icon, eyebyte.GetComponent<AbilitySpawnFx>().PrefabLink, penalties[1], context_rank_config);
            evil_eye_variants[2] = createEvilEyeComponent(name_prefix + "SavesHex", $"{display_name} (Saving Throws Penalty)", description, abil3_guid, buff3_guid,
                                                     eyebyte.Icon, eyebyte.GetComponent<AbilitySpawnFx>().PrefabLink, penalties[2], penalties[3], penalties[4], context_rank_config);
            var evil_eye_ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                         display_name,
                                                         description,
                                                         abil_guid,
                                                         eyebyte.Icon,
                                                         evil_eye_variants[0].Type,
                                                         evil_eye_variants[0].ActionType,
                                                         evil_eye_variants[0].Range,
                                                         evil_eye_variants[0].LocalizedDuration,
                                                         evil_eye_variants[0].LocalizedSavingThrow);
            evil_eye_ability.AddComponent(evil_eye_ability.CreateAbilityVariants(evil_eye_variants));

            //remove separate abilities
            Action<UnitDescriptor> save_game_fix = delegate (UnitDescriptor unit)
            {
                foreach (var ee in evil_eye_variants)
                {
                    unit.RemoveFact(ee);
                }
            };
            SaveGameFix.save_game_actions.Add(save_game_fix);

            var evil_eye = Helpers.CreateFeature(name_prefix + "HexFeature",
                                          display_name,
                                          description,
                                          feature_guid,
                                          eyebyte.Icon,
                                          FeatureGroup.None,
                                          Helpers.CreateAddFact(evil_eye_ability)
                                          );
            evil_eye.Ranks = 1;
            return evil_eye;
        }


        BlueprintAbility createEvilEyeComponent(string name, string display_name, string description, string guid, string buff_guid, UnityEngine.Sprite icon, PrefabLink prefab,
                                                        params BlueprintComponent[] components)
        {
            var buff = Helpers.CreateBuff(name + "Buff", display_name, description, buff_guid, icon, prefab, components);
            buff.Stacking = StackingType.Prolong;
            cackle_buffs.Add(buff);

            var ability = Helpers.CreateAbility(name + "Ability",
                                                display_name,
                                                description,
                                                guid,
                                                icon,
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Close,
                                                "Variable",
                                                "Will special");
            ability.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Point;
            ability.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionPoint;
            ability.CanTargetEnemies = true;
            ability.CanTargetFriends = test_mode;
            ability.CanTargetSelf = test_mode;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.EffectOnAlly = AbilityEffectOnUnit.Harmful;
           
            
            var action_save = Common.createContextSavedApplyBuff(buff, DurationRate.Rounds, AbilityRankType.DamageBonus, is_dispellable: false);
            var buff_save = Helpers.Create<Kingmaker.UnitLogic.Mechanics.Actions.ContextActionApplyBuff>();
            buff_save.IsFromSpell = true;
            buff_save.Buff = buff;
            var bonus_value = Helpers.CreateContextValue(AbilityRankType.Default);
            bonus_value.ValueType = ContextValueType.Simple;
            bonus_value.Value = 1;
            buff_save.DurationValue = Helpers.CreateContextDuration(bonus: bonus_value,
                                                                           rate: DurationRate.Rounds);
            action_save.Succeed = Helpers.CreateActionList(buff_save);
            var action = Helpers.CreateActionList(Common.createContextActionRemoveBuff(buff),
                                                  Common.createContextActionSavingThrow(SavingThrowType.Will, Helpers.CreateActionList(action_save))
                                                  );
            ability.AddComponent(Helpers.CreateRunActions(action.Actions));

            var context_rank_config = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, stat: hex_stat, progression: ContextRankProgression.StartPlusDivStep,
                                                    min: 3, startLevel: -2, stepLevel: 1, type: AbilityRankType.DamageBonus);

            ability.AddComponent(context_rank_config);
            ability.Type = AbilityType.Supernatural;
            ability.SpellResistance = false;

            ability.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting));
            var scaling = Common.createContextCalculateAbilityParamsBasedOnClasses(hex_classes, hex_stat);
            ability.AddComponent(scaling);
            var eyebyte = library.Get<BlueprintAbility>("582009cf6013790469d6e98e5210477a");
            ability.AddComponent(eyebyte.GetComponent<Kingmaker.UnitLogic.Abilities.Components.Base.AbilitySpawnFx>());
            addToAmplifyHex(ability);
            addToSplitHex(ability, true);
            return ability;
        }


        public BlueprintFeature createSummerHeat(string name_prefix, string display_name, string description, string ability_guid, string buff1_guid, string buff2_guid, 
                                                 string feature_guid, string cooldown_guid)
        {
            var fatigued_buff = library.Get<BlueprintBuff>("e6f2fc5d73d88064583cb828801212f4");
            var exhausted_buff = library.Get<BlueprintBuff>("46d1b9cc3d0fd36469a471b047d773a2");

            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Fire,
                                                     Helpers.CreateContextDiceValue(DiceType.Zero, bonus: Helpers.CreateContextValue(AbilityRankType.DamageBonus)),
                                                     halfIfSaved: true
                                                     );
            var hex_ability = library.CopyAndAdd<BlueprintAbility>("f2f1efac32ea2884e84ecaf14657298b", name_prefix + "Hex", ability_guid);//bonshatter
            hex_ability.SetIcon(fatigued_buff.Icon);
            hex_ability.ComponentsArray = new BlueprintComponent[] { hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.Base.AbilitySpawnFx>() };
            hex_ability.CanTargetFriends = test_mode;
            hex_ability.CanTargetSelf = test_mode;
            hex_ability.SetName(display_name);
            hex_ability.SetDescription(description);



            var context_saved = Helpers.Create<Kingmaker.UnitLogic.Mechanics.Actions.ContextActionConditionalSaved>();
            context_saved.Failed = Helpers.CreateActionList(Helpers.CreateConditional(Helpers.CreateConditionHasFact(fatigued_buff),
                                                                                    Common.createContextActionApplyBuff(exhausted_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false),
                                                                                    Common.createContextActionApplyBuff(fatigued_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false)
                                                                                    )
                                                            );
            var effect = Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(dmg, context_saved));

            hex_ability.AddComponent(Helpers.CreateRunActions(effect));
            hex_ability.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Fatigue));
            hex_ability.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, type: AbilityRankType.DamageBonus,
                                                                      classes: hex_classes)
                                   );

            addWitchHexCooldownScaling(hex_ability, cooldown_guid);
            var summer_heat = Helpers.CreateFeature(name_prefix + "HexFeature",
                              hex_ability.Name,
                              hex_ability.Description,
                              feature_guid,
                              hex_ability.Icon,
                              FeatureGroup.None,
                              Helpers.CreateAddFact(hex_ability)
                              );
            summer_heat.Ranks = 1;
            addToAmplifyHex(hex_ability);
            addToSplitHex(hex_ability, true);
            return summer_heat;
        }


        public BlueprintFeature createMajorHealing(string name_prefix, string display_name, string description, string abil1_guid, string abil2_guid,
                                              string feature1_guid, string feature2_guid, string feature_guid, string cooldown_guid)
        {
            var major_healing = createHealingHex(name_prefix + "Hex", display_name,
                                             description,
                                             "6e81a6679a0889a429dec9cedcf3729c", //cure serious wounds
                                             "0d657aa811b310e4bbd8586e60156a2d", //cure critical wounds
                                             abil1_guid,
                                             abil2_guid,
                                             feature1_guid,
                                             feature2_guid,
                                             feature_guid,
                                             cooldown_guid,
                                             15);

            addMajorHexPrerequisite(major_healing);
            return major_healing;
        }


        BlueprintFeature createHealingHex(string name, string display_name, string description, string heal1_guid, string heal2_guid, string ability1_guid, string ability2_guid,
                                                    string feature1_guid, string feature2_guid, string feature_guid, string cooldown_guid, int update_level)
        {
            var heal1_hex_ability = library.CopyAndAdd<BlueprintAbility>(heal1_guid, name + "1Ability", ability1_guid);
            heal1_hex_ability.SetName(display_name);
            heal1_hex_ability.SetDescription(description);

            heal1_hex_ability.ReplaceComponent<Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig>(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                                                                                       classes: hex_classes,
                                                                                                                                                       max: update_level));
            var hex_cooldown = addWitchHexCooldownScaling(heal1_hex_ability, cooldown_guid);


            var heal2_hex_ability = library.CopyAndAdd<BlueprintAbility>(heal2_guid, name + "2Ability", ability2_guid);
            heal2_hex_ability.SetName(heal1_hex_ability.Name);
            heal2_hex_ability.SetDescription(heal1_hex_ability.Description);


            heal2_hex_ability.ReplaceComponent<Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig>(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                                                                                       classes: hex_classes,
                                                                                                                                                       max: update_level + 5));
            addWitchHexCooldownScaling(heal2_hex_ability, hex_cooldown);

            var healing_hex1_feature = Helpers.CreateFeature(name + "1Feature", heal1_hex_ability.Name, heal1_hex_ability.Description,
                                                             feature1_guid,
                                                             heal1_hex_ability.Icon,
                                                             FeatureGroup.None,
                                                             Helpers.CreateAddFact(heal1_hex_ability));
            healing_hex1_feature.HideInCharacterSheetAndLevelUp = true;
            var healing_hex2_feature = Helpers.CreateFeature(name + "2Feature", heal1_hex_ability.Name, heal1_hex_ability.Description,
                                                 feature2_guid,
                                                 heal1_hex_ability.Icon,
                                                 FeatureGroup.None,
                                                 Helpers.CreateAddFact(heal2_hex_ability));
            healing_hex2_feature.HideInCharacterSheetAndLevelUp = true;
            var healing = Helpers.CreateFeature(name + "Feature",
                                                heal1_hex_ability.Name,
                                                heal1_hex_ability.Description,
                                                feature_guid,
                                                heal1_hex_ability.Icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddFeatureOnClassLevel(healing_hex1_feature, update_level, hex_classes, new BlueprintArchetype[0], true),
                                                Helpers.CreateAddFeatureOnClassLevel(healing_hex2_feature, update_level, hex_classes, new BlueprintArchetype[0], false)
                                                );
            healing.Ranks = 1;
            addToAmplifyHex(heal1_hex_ability);
            addToAmplifyHex(heal2_hex_ability);
            if (update_level < 10)
            {
                addToSplitHex(heal1_hex_ability, true);
                addToSplitHex(heal2_hex_ability, true);
            };
            return healing;
        }


        public BlueprintFeature createMajorAmeliorating(string name_prefix, string display_name, string description, string abil1_guid, string abil2_guid,
                                                        string feature_guid, string buff2_guid, string cooldown_guid)
        {
            var hex_ability1 = library.CopyAndAdd<BlueprintAbility>("4093d5a0eb5cae94e909eb1e0e1a6b36", name_prefix + "HexRemoveAbility", abil1_guid);
            hex_ability1.SetName($"{display_name}: Remove Condition");
            hex_ability1.SetDescription(description);

            var action1 = (Kingmaker.UnitLogic.Mechanics.Actions.ContextActionDispelMagic)hex_ability1.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>().Actions.Actions[0].CreateCopy();
            action1.Descriptor = SpellDescriptor.Blindness | SpellDescriptor.Curse | SpellDescriptor.Poison | SpellDescriptor.Disease;
            hex_ability1.RemoveComponent(hex_ability1.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>());
            var run_action = Helpers.Create<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>();
            run_action.Actions = Helpers.CreateActionList(action1);
            hex_ability1.AddComponent(run_action);

            var hex_ability2 = library.CopyAndAdd<BlueprintAbility>("4093d5a0eb5cae94e909eb1e0e1a6b36", name_prefix + "HexSaveAbility", abil2_guid);


            hex_ability2.SetName($"{display_name}: Saving Throws Bonus");
            hex_ability2.LocalizedDuration = Helpers.CreateString(name_prefix + "HexAbility.Duration", "24 hours");
            var hex_ability2_buff = Helpers.CreateBuff(name_prefix + "HexSaveBuff", hex_ability2.Name, hex_ability2.Description, buff2_guid,
                                                       hex_ability2.Icon, null,
                                                       Common.createSavingThrowBonusAgainstDescriptor(4, ModifierDescriptor.Circumstance,
                                                                SpellDescriptor.Blindness | SpellDescriptor.Curse | SpellDescriptor.Disease | SpellDescriptor.Poison)
                                                      );
            hex_ability2_buff.SetBuffFlags(BuffFlags.RemoveOnRest);
            var action2 = Helpers.Create<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>();
            var bonus_value2 = Helpers.CreateContextValue(AbilityRankType.DamageBonus);
            bonus_value2.ValueType = ContextValueType.Simple;
            bonus_value2.Value = 1;
            var duration_value2 = Helpers.CreateContextDuration(bonus: bonus_value2, rate: DurationRate.Days);
            action2.Actions = Helpers.CreateActionList(Helpers.CreateApplyBuff(hex_ability2_buff, duration_value2, true, dispellable: false));
            hex_ability2.RemoveComponent(hex_ability1.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>());
            hex_ability2.AddComponent(action2);
            var hex_cooldown = addWitchHexCooldownScaling(hex_ability1, cooldown_guid, $"Cooldown {display_name}");
            addWitchHexCooldownScaling(hex_ability2, hex_cooldown);

            var major_ameliorating = Helpers.CreateFeature("Witch" + name_prefix + "HexFeature",
                                                      display_name,
                                                      hex_ability1.Description,
                                                      feature_guid,
                                                      hex_ability1.Icon,
                                                      FeatureGroup.None,
                                                      Helpers.CreateAddFacts(hex_ability1, hex_ability2));
            major_ameliorating.Ranks = 1;
            addMajorHexPrerequisite(major_ameliorating);
            return major_ameliorating;
        }


        public BlueprintFeature createAnimalSkin(string name_prefix, string display_name, string description, string ability1_guid, string ability2_guid, string ability3_guid, string feature_guid)
        {
            var hex_abilities = new BlueprintAbility[]{library.CopyAndAdd<BlueprintAbility>(Wildshape.bear_form_spell, name_prefix + "HexAbility", ability1_guid),
                                                       library.CopyAndAdd<BlueprintAbility>(Wildshape.dire_wolf_form_spell, name_prefix + "HexAbility2", ability2_guid),
                                                       library.CopyAndAdd<BlueprintAbility>(Wildshape.leopard_form_spell, name_prefix + "HexAbility3", ability3_guid)};
            foreach (var h in hex_abilities)
            {
                h.Type = AbilityType.Supernatural;
                h.SetName(display_name + $": {h.Name}");
                var spell_list_components = h.GetComponents<Kingmaker.Blueprints.Classes.Spells.SpellListComponent>().ToArray();
                foreach (var c in spell_list_components)
                {
                    h.RemoveComponent(c);
                }
                h.RemoveComponents<SpellDescriptorComponent>();
                h.RemoveComponent(h.GetComponent<Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig>());
                h.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: hex_classes));
            }

            var animal_skin = Helpers.CreateFeature(name_prefix + "Feature",
                                                  display_name,
                                                  description,
                                                  feature_guid,
                                                  hex_abilities[0].Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFacts(hex_abilities));
            animal_skin.Ranks = 1;
            addMajorHexPrerequisite(animal_skin);
            return animal_skin;
        }


        public BlueprintFeature createAgony(string name_prefix, string display_name, string description, string ability_guid, string buff_guid,
                                                 string feature_guid, string cooldown_guid)
        {
            var hex_ability = library.CopyAndAdd<BlueprintAbility>("68a9e6d7256f1354289a39003a46d826", name_prefix + "HexAbility", ability_guid);//stinking cloud
            hex_ability.SetName(display_name);
            hex_ability.SetDescription(description);
            hex_ability.RemoveComponent(hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>());
            var hex_buff = library.CopyAndAdd<BlueprintBuff>("956331dba5125ef48afe41875a00ca0e", name_prefix + "HexBuff", buff_guid); //nauseted
            hex_buff.RemoveComponent(hex_buff.GetComponent<Kingmaker.UnitLogic.FactLogic.AddCondition>());
            hex_buff.AddComponent(Common.createBuffStatusCondition(UnitCondition.Nauseated, SavingThrowType.Fortitude));
            cackle_buffs.Add(hex_buff);
            var action = Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(Common.createContextSavedApplyBuff(hex_buff, DurationRate.Rounds, is_dispellable: false)));

            hex_ability.AddComponent(Helpers.CreateRunActions(action));
            hex_ability.CanTargetFriends = test_mode;
            hex_ability.CanTargetSelf = test_mode;
            hex_ability.CanTargetPoint = false;
            hex_ability.CanTargetEnemies = true;
            hex_ability.RemoveComponent(hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityAoERadius>());
            hex_ability.RemoveComponent(hex_ability.GetComponent<ContextRankConfig>());
            hex_ability.ReplaceComponent<SpellDescriptorComponent>(Helpers.CreateSpellDescriptor(SpellDescriptor.Nauseated));

            addWitchHexCooldownScaling(hex_ability, cooldown_guid);

            var agony = Helpers.CreateFeature(name_prefix + "HexFeature",
                                              hex_ability.Name,
                                              hex_ability.Description,
                                              feature_guid,
                                              hex_ability.Icon,
                                              FeatureGroup.None,
                                              Helpers.CreateAddFact(hex_ability));
            agony.Ranks = 1;
            addMajorHexPrerequisite(agony);
            addToAmplifyHex(hex_ability);
            return agony;
        }


        public BlueprintFeature createBeastGift(string name_prefix, string display_name, string description, string ability_guid, string buff_guid,
                                                 string feature_guid, string cooldown_guid)
        {
            var animal_fury = library.Get<BlueprintFeature>("25954b1652bebc2409f9cb9d5728bceb");
            var buff = Helpers.CreateBuff(name_prefix + "Buff", display_name,
                                          description,
                                          buff_guid,
                                          animal_fury.Icon,
                                          null,
                                          Common.createAddSecondaryAttacks(library.Get<Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon>("61bc14eca5f8c1040900215000cfc218")) //bite 1d8
                                          );

            var hex_ability = library.CopyAndAdd<BlueprintAbility>("403cf599412299a4f9d5d925c7b9fb33", name_prefix + "HexAbility", ability_guid); //magic fang
            hex_ability.RemoveComponent(hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>());
            hex_ability.RemoveComponent(hex_ability.GetComponent<ContextRankConfig>());
            var action = Helpers.Create<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>();
            var bonus = Helpers.CreateContextValue(AbilityRankType.Default);
            var duration = Helpers.CreateContextDuration(bonus, DurationRate.Minutes);
            action.Actions = Helpers.CreateActionList(Common.createContextActionApplyBuff(buff, duration, dispellable: false));
            hex_ability.AddComponent(action);
            hex_ability.SetIcon(buff.Icon);
            hex_ability.SetName(buff.Name);
            hex_ability.SetDescription(buff.Description);
            addWitchHexCooldownScaling(hex_ability, cooldown_guid);

            var beast_gift = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  hex_ability.Name,
                                                  hex_ability.Description,
                                                  feature_guid,
                                                  hex_ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(hex_ability));
            beast_gift.Ranks = 1;
            addMajorHexPrerequisite(beast_gift);
            return beast_gift;
        }


        public BlueprintFeature createHarrowingCurse(string name_prefix, string display_name, string description, string ability_guid,
                                                      string feature_guid, string cooldown_guid)
        {
            var hex_ability = library.CopyAndAdd<BlueprintAbility>("69851cc3b821c2d479ac1f2d86e8ffa5", name_prefix + "Hex", ability_guid);
            BlueprintBuff[] curses = new BlueprintBuff[] {library.Get<BlueprintBuff>("caae9592917719a41b601b678a8e6ddf"),
                                                              library.Get<BlueprintBuff>("c092750ba895e014cb24a25e2e8274a7"),
                                                              library.Get<BlueprintBuff>("7fbb7799e8684434e80487cef9cc7f09"),
                                                              library.Get<BlueprintBuff>("de92c96c86cb2cd4c8eb8e2881b84d99")};
            hex_ability.SetName(display_name);
            hex_ability.SetDescription(description);
            hex_ability.RemoveComponent(hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>());

            Kingmaker.ElementsSystem.ActionList[] curse_actions = new Kingmaker.ElementsSystem.ActionList[curses.Length];
            for (int i = 0; i < curses.Length; i++)
            {
                curse_actions[i] = Helpers.CreateActionList(Common.createContextActionApplyBuff(curses[i], Helpers.CreateContextDuration(), is_permanent: true, dispellable: false));
            }
            var random_curse = Common.createContextActionRandomize(curse_actions);

            var action_saved = Helpers.Create<Kingmaker.UnitLogic.Mechanics.Actions.ContextActionConditionalSaved>();
            action_saved.Failed = Helpers.CreateActionList(random_curse);
            var effect = Common.createContextActionSavingThrow(SavingThrowType.Will, Helpers.CreateActionList(action_saved));
            
            hex_ability.AddComponent(Helpers.CreateRunActions(effect));
            addWitchHexCooldownScaling(hex_ability, cooldown_guid);

            var harrowing_curse = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                      hex_ability.Name,
                                                      hex_ability.Description,
                                                      feature_guid,
                                                      hex_ability.Icon,
                                                      FeatureGroup.None,
                                                      Helpers.CreateAddFact(hex_ability));
            harrowing_curse.Ranks = 1;
            addMajorHexPrerequisite(harrowing_curse);
            addToAmplifyHex(hex_ability);
            return harrowing_curse;
        }


        public BlueprintFeature createRetribution(string name_prefix, string display_name, string description, string ability_guid, string buff_guid,
                                                      string feature_guid)
        {
            var resounding_blow = library.Get<BlueprintAbility>("9047cb1797639924487ec0ad566a3fea");
            var serenity = library.Get<BlueprintAbility>("d316d3d94d20c674db2c24d7de96f6a7");

            var reflect_damage = Helpers.Create<NewMechanics.ReflectDamage>();
            reflect_damage.reflection_coefficient = 0.5f;
            reflect_damage.reflect_melee_weapon = true;

            var buff = Helpers.CreateBuff(name_prefix + "HexBuff",
                                          display_name,
                                          description,
                                          buff_guid,
                                          resounding_blow.Icon,
                                          null,
                                          reflect_damage);
            //buff.Stacking = StackingType.Replace;

            var apply_buff = Common.createContextSavedApplyBuff(buff,
                                                            Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.DamageBonus), rate: DurationRate.Rounds),
                                                            is_dispellable: false
                                                            );
            var action = Common.createContextActionSavingThrow(SavingThrowType.Will, Helpers.CreateActionList(apply_buff));
            var ability = Helpers.CreateAbility(name_prefix + "HexAbility",
                                                buff.Name,
                                                buff.Description,
                                                ability_guid,
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Medium,
                                                "Variable",
                                                "Will negates",
                                                Helpers.CreateRunActions(action),
                                                serenity.GetComponent<AbilitySpawnFx>(),
                                                Common.createContextCalculateAbilityParamsBasedOnClasses(hex_classes, hex_stat),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus,
                                                                                stat: StatType.Intelligence,
                                                                                progression: ContextRankProgression.AsIs,
                                                                                type: AbilityRankType.DamageBonus, min: 1)
                                                );
            ability.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Directional;
            ability.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionDirectional;
            ability.CanTargetEnemies = true;
            ability.CanTargetFriends = test_mode;
            ability.CanTargetSelf = test_mode;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.EffectOnAlly = AbilityEffectOnUnit.Harmful;

            var retribution = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                buff.Name,
                                                buff.Description,
                                                feature_guid,
                                                buff.Icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddFact(ability)
                                                );
            retribution.Ranks = 1;
            addMajorHexPrerequisite(retribution);
            addToAmplifyHex(ability);
            return retribution;
        }


        public BlueprintFeature createIceTomb(string name_prefix, string display_name, string description, string ability_guid, string buff_guid,
                                               string feature_guid, string cooldown_guid)
        {
            var hex_ability = library.CopyAndAdd<BlueprintAbility>("65e8d23aef5e7784dbeb27b1fca40931", name_prefix + "HexAbility", ability_guid);
            hex_ability.SetName(display_name);
            hex_ability.SetDescription(description);
            hex_ability.RemoveComponent(hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>());
            hex_ability.Range = AbilityRange.Close;
            hex_ability.CanTargetFriends = test_mode;
            hex_ability.CanTargetSelf = test_mode;
            var damage_value = new ContextValue();
            damage_value.ValueType = ContextValueType.Simple;
            damage_value.Value = 3;
            var damage_action = Helpers.CreateActionDealDamage(DamageEnergyType.Cold, Helpers.CreateContextDiceValue(DiceType.D8, damage_value), halfIfSaved: true);

            var sleep_buff = library.Get<BlueprintBuff>("c9937d7846aa9ae46991e9f298be644a");
            var ice_tomb_buff = library.CopyAndAdd<BlueprintBuff>("6f0e450771cc7d446aea798e1fef1c7a", name_prefix + "HexBuff", buff_guid);//icy prison buff
            ice_tomb_buff.RemoveComponent(ice_tomb_buff.GetComponent<Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig>());
            ice_tomb_buff.RemoveComponent(ice_tomb_buff.GetComponent<Kingmaker.UnitLogic.Mechanics.Components.AddFactContextActions>());
            ice_tomb_buff.SetName(hex_ability.Name);
            ice_tomb_buff.SetDescription(hex_ability.Description);
            ice_tomb_buff.AddComponent(Common.createBuffStatusCondition(UnitCondition.Sleeping, save_each_round: false));
            ice_tomb_buff.AddComponent(Common.createBuffStatusCondition(UnitCondition.Paralyzed, save_each_round: false));

            var staggered_buff = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");

            var damage_trigger = Helpers.Create<Kingmaker.UnitLogic.Mechanics.Components.AddIncomingDamageTrigger>();
            damage_trigger.Actions = Helpers.CreateActionList(Helpers.Create<Kingmaker.UnitLogic.Mechanics.Actions.ContextActionRemoveSelf>(),
                                                              Common.createContextActionApplyBuff(staggered_buff, 
                                                                                                  Helpers.CreateContextDuration(diceType: DiceType.D4, diceCount: Common.createSimpleContextValue(1)))
                                                              );
            ice_tomb_buff.AddComponent(damage_trigger); //remove buff on damage, and add stagger

            var action_buff = Helpers.Create<ContextActionConditionalSaved>();
            action_buff.Failed = Helpers.CreateActionList(Common.createContextActionApplyBuff(ice_tomb_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false));
          
            var effect = Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(damage_action, action_buff));
            hex_ability.AddComponent(Helpers.CreateRunActions(effect));
            addWitchHexCooldownScaling(hex_ability, cooldown_guid);
            var ice_tomb = Helpers.CreateFeature(name_prefix + "HexFeature",
                                          hex_ability.Name,
                                          hex_ability.Description,
                                          feature_guid,
                                          hex_ability.Icon,
                                          FeatureGroup.None,
                                          Helpers.CreateAddFact(hex_ability));
            ice_tomb.Ranks = 1;
            addMajorHexPrerequisite(ice_tomb);
            addToAmplifyHex(hex_ability);
            return ice_tomb;
        }


        public BlueprintFeature createRegenerativeSinew(string name_prefix, string display_name, string description, string ability1_guid, string ability2_guid, string buff_guid,
                                                        string feature_guid, string cooldown_guid)
        {
            var fast_healing_buff = library.CopyAndAdd<BlueprintBuff>("37a5e51e9e3a23049a77ba70b4e7b2d2", name_prefix + "FHHexBuff", buff_guid); //fs5
            fast_healing_buff.SetDescription(description);
            fast_healing_buff.SetName($"{display_name}: Fast Healing");

            var fast_healing_ability = library.CopyAndAdd<BlueprintAbility>("f2115ac1148256b4ba20788f7e966830", name_prefix + "FHHexAbility", ability1_guid); //restoration
            fast_healing_ability.RemoveComponent(fast_healing_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>());
            fast_healing_ability.RemoveComponent(fast_healing_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityUseOnRest>());

            var action = Helpers.Create<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>();
            var bonus = Helpers.CreateContextValue(AbilityRankType.DamageBonus);
            var duration = Helpers.CreateContextDuration(bonus, DurationRate.Rounds);
            action.Actions = Helpers.CreateActionList(Common.createContextActionApplyBuff(fast_healing_buff, duration, dispellable: false));
            fast_healing_ability.AddComponent(action);
            fast_healing_buff.SetIcon(fast_healing_ability.Icon);
            fast_healing_ability.SetName(fast_healing_buff.Name);
            fast_healing_ability.SetDescription(fast_healing_buff.Description);
            fast_healing_ability.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: hex_classes, progression: ContextRankProgression.Div2,
                                                                              type: AbilityRankType.DamageBonus));
            fast_healing_ability.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            fast_healing_ability.ActionType = CommandType.Standard;
            Helpers.SetField(fast_healing_ability, "m_IsFullRoundAction", false);
            var hex_cooldown = addWitchHexCooldownScaling(fast_healing_ability, cooldown_guid, $"Cooldown {display_name}");

            var restoration_ability = library.CopyAndAdd<BlueprintAbility>("f2115ac1148256b4ba20788f7e966830", name_prefix + "RestorationHexAbility", ability2_guid); //restoration
            var restoration_action = restoration_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>();
            var restoration_component = ((Kingmaker.UnitLogic.Mechanics.Actions.ContextActionHealStatDamage)restoration_action.Actions.Actions[0]).CreateCopy();
            restoration_component.HealDrain = false;
            restoration_ability.RemoveComponent(restoration_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>());
            restoration_ability.RemoveComponent(restoration_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityUseOnRest>());
            var action2 = Helpers.Create<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>();
            action2.Actions = Helpers.CreateActionList(restoration_component);
            restoration_ability.AddComponent(action2);

            restoration_ability.SetDescription(fast_healing_ability.Description);
            restoration_ability.SetName($"{display_name}: Restoration");
            restoration_ability.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            restoration_ability.ActionType = CommandType.Standard;
            Helpers.SetField(restoration_ability, "m_IsFullRoundAction", false);
            addWitchHexCooldownScaling(restoration_ability, hex_cooldown);

            var regenerative_sinew = Helpers.CreateFeature(name_prefix + "HexFeature",
                                          display_name,
                                          fast_healing_ability.Description,
                                          feature_guid,
                                          fast_healing_ability.Icon,
                                          FeatureGroup.None,
                                          Helpers.CreateAddFacts(fast_healing_ability, restoration_ability));
            regenerative_sinew.Ranks = 1;
            addMajorHexPrerequisite(regenerative_sinew);
            return regenerative_sinew;
        }


        public BlueprintFeature createAnimalServant(string name_prefix, string display_name, string description, string ability_guid, string buff_guid,
                                               string feature_guid, string cooldown_guid)
        {
            var hex_ability = library.CopyAndAdd<BlueprintAbility>("d7cbd2004ce66a042aeab2e95a3c5c61", name_prefix + "HexAbility", ability_guid);//dominate  person
            hex_ability.CanTargetFriends = test_mode;
            hex_ability.CanTargetSelf = test_mode;
            hex_ability.ActionType = CommandType.Standard;
            Helpers.SetField(hex_ability, "m_IsFullRoundAction", false);
            hex_ability.SetName(display_name);
            hex_ability.SetDescription(description);
            var dominate_person_buff = library.Get<BlueprintBuff>("c0f4e1c24c9cd334ca988ed1bd9d201f");
            var hex_buff = library.CopyAndAdd<BlueprintBuff>(Wildshape.bear_form.AssetGuid, name_prefix + "HexBuff", buff_guid);
            //library.CopyAndAdd<BlueprintBuff>("200bd9b179ee660489fe88663115bcbc", "WitchAnimalServantHexBuff", "32b4b11964724f59a9034e61014dbb3c"); //beast_shape2;
            hex_buff.SetDescription(hex_ability.Description);
            hex_buff.SetName(hex_ability.Name);
            hex_buff.SetIcon(hex_ability.Icon);

            var polymorph_component = hex_buff.GetComponent<Kingmaker.UnitLogic.Buffs.Polymorph>().CreateCopy();
            polymorph_component.Facts = polymorph_component.Facts.RemoveFromArray(Wildshape.turn_back);
            hex_buff.ReplaceComponent<Kingmaker.UnitLogic.Buffs.Polymorph>(polymorph_component);
            hex_buff.AddComponent(dominate_person_buff.GetComponent<Kingmaker.UnitLogic.FactLogic.ChangeFaction>());
            hex_buff.ReplaceComponent<Kingmaker.Blueprints.Classes.Spells.SpellDescriptorComponent>(Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Compulsion | SpellDescriptor.Polymorph));
            hex_buff.SetBuffFlags(hex_buff.GetBuffFlags() | BuffFlags.RemoveOnRest);
            var action = Common.createContextActionSavingThrow(SavingThrowType.Will, 
                                                               Helpers.CreateActionList(Common.createContextSavedApplyBuff(hex_buff, Helpers.CreateContextDuration(), is_permanent: true, is_dispellable: false)));
            hex_ability.ReplaceComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>(Helpers.CreateRunActions(action));


            addWitchHexCooldownScaling(hex_ability, cooldown_guid);
            var animal_servant = Helpers.CreateFeature(name_prefix + "HexFeature",
                              hex_ability.Name,
                              hex_ability.Description,
                              feature_guid,
                              hex_ability.Icon,
                              FeatureGroup.None,
                              Helpers.CreateAddFact(hex_ability));
            animal_servant.Ranks = 1;
            addGrandHexPrerequisite(animal_servant);
            addToAmplifyHex(hex_ability);
            return animal_servant;
        }


        public BlueprintFeature createDeathCurse(string name_prefix, string display_name, string description, string ability_guid, string buff_guid,
                                               string feature_guid, string cooldown_guid)
        {
            var hex_buff = library.CopyAndAdd<BlueprintBuff>("e6f2fc5d73d88064583cb828801212f4", name_prefix + "HexBuff", buff_guid); //fatigue buff
            var exhausted_buff = library.Get<BlueprintBuff>("46d1b9cc3d0fd36469a471b047d773a2");
            var death_effect = Helpers.Create<Kingmaker.UnitLogic.Mechanics.Actions.ContextActionSavingThrow>();
            death_effect.Type = SavingThrowType.Fortitude;
            var death_effect_conditional = Helpers.Create<Kingmaker.UnitLogic.Mechanics.Actions.ContextActionConditionalSaved>();
            death_effect_conditional.Failed = Helpers.CreateActionList(Helpers.Create<Kingmaker.UnitLogic.Mechanics.Actions.ContextActionKillTarget>());

            var damage = Helpers.CreateContextDiceValue(DiceType.D6, Common.createSimpleContextValue(4), Helpers.CreateContextValue(AbilityRankType.DamageBonus));
            death_effect_conditional.Succeed = Helpers.CreateActionList(Helpers.CreateActionDealDamage(DamageEnergyType.NegativeEnergy, damage));
            death_effect.Actions = Helpers.CreateActionList(death_effect_conditional, 
                                                            Helpers.Create<Kingmaker.UnitLogic.Mechanics.Actions.ContextActionRemoveSelf>());

            var round3_death = Helpers.CreateConditional(Common.createBuffConditionCheckRoundNumber(3), death_effect);
            var round2_exhausted = Helpers.CreateConditional(Common.createBuffConditionCheckRoundNumber(2),
                                                             Common.createContextActionApplyBuff(exhausted_buff, Helpers.CreateContextDuration(), is_permanent: true),
                                                             round3_death);
            hex_buff.AddComponent(Helpers.CreateAddFactContextActions(newRound: round2_exhausted));

            var hex_ability = library.CopyAndAdd<BlueprintAbility>("6f1dcf6cfa92d1948a740195707c0dbe", name_prefix + "HexAbility", ability_guid); //finger of death
            hex_ability.SetName(display_name);
            hex_ability.SetDescription(description);
            hex_ability.RemoveComponent(hex_ability.GetComponent<Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig>());
            hex_ability.RemoveComponent(hex_ability.GetComponent<Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig>());
            hex_buff.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.AsIs,
                                                                                                     classes: hex_classes, type: AbilityRankType.DamageBonus)); //for damage on save

            var action = Common.createContextActionSavingThrow(SavingThrowType.Will,
                                                               Helpers.CreateActionList(Common.createContextSavedApplyBuff(hex_buff, Helpers.CreateContextDuration(), is_permanent: true, is_dispellable: false))
                                                               );
            hex_ability.ReplaceComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>(Helpers.CreateRunActions(action));
            hex_ability.ReplaceComponent<Kingmaker.Blueprints.Classes.Spells.SpellDescriptorComponent>(Helpers.CreateSpellDescriptor(SpellDescriptor.Death));
            addWitchHexCooldownScaling(hex_ability, cooldown_guid);
            var target_checker = Helpers.Create<Kingmaker.UnitLogic.Abilities.Components.TargetCheckers.AbilityTargetHasFact>();
            target_checker.CheckedFacts = new BlueprintUnitFact[] { library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f"), //construct
                                                                    library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33") //undead
                                                                  };
            target_checker.Inverted = true;
            hex_ability.AddComponent(target_checker);
            hex_buff.SetIcon(hex_ability.Icon);
            hex_buff.SetDescription(hex_ability.Description);
            hex_buff.SetName(hex_ability.Name);

            hex_ability.CanTargetFriends = test_mode;
            hex_ability.ActionType = CommandType.Standard;
            Helpers.SetField(hex_ability, "m_IsFullRoundAction", false);
            var death_curse = Helpers.CreateFeature(name_prefix + "HexFeature",
                              hex_ability.Name,
                              hex_ability.Description,
                              feature_guid,
                              hex_ability.Icon,
                              FeatureGroup.None,
                              Helpers.CreateAddFact(hex_ability));
            death_curse.Ranks = 1;
            addGrandHexPrerequisite(death_curse);
            addToAmplifyHex(hex_ability);
            return death_curse;
        }


        public BlueprintFeature createLayToRest(string name_prefix, string display_name, string description, string ability_guid,
                                               string feature_guid, string cooldown_guid)
        {
            var hex_ability = library.CopyAndAdd<BlueprintAbility>("a9a52760290591844a96d0109e30e04d", name_prefix + "HexAbility", ability_guid);
            hex_ability.SetName(display_name);
            hex_ability.SetDescription(description);
            hex_ability.CanTargetPoint = false;
            hex_ability.CanTargetSelf = false;
            hex_ability.CanTargetFriends = false;
            hex_ability.Range = AbilityRange.Close;
            hex_ability.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            hex_ability.RemoveComponent(hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityTargetsAround>());
            var target_checker = Helpers.Create<Kingmaker.UnitLogic.Abilities.Components.TargetCheckers.AbilityTargetHasFact>();
            target_checker.CheckedFacts = new BlueprintUnitFact[] { library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33") };//undead
            hex_ability.AddComponent(target_checker);
            var destruction = library.Get<BlueprintAbility>("3b646e1db3403b940bf620e01d2ce0c7");
            hex_ability.ReplaceComponent<Kingmaker.UnitLogic.Abilities.Components.Base.AbilitySpawnFx>(destruction.GetComponent<Kingmaker.UnitLogic.Abilities.Components.Base.AbilitySpawnFx>());
            addWitchHexCooldownScaling(hex_ability, cooldown_guid);

            var lay_to_rest = Helpers.CreateFeature(name_prefix + "HexFeature",
                              hex_ability.Name,
                              hex_ability.Description,
                              feature_guid,
                              hex_ability.Icon,
                              FeatureGroup.None,
                              Helpers.CreateAddFact(hex_ability));
            lay_to_rest.Ranks = 1;
            addGrandHexPrerequisite(lay_to_rest);
            addToAmplifyHex(hex_ability);
            return lay_to_rest;
        }


        public BlueprintFeature createLifeGiver(string name_prefix, string display_name, string description, string ability_guid,
                                               string feature_guid, string resource_guid)
        {
            var hex_ability = library.CopyAndAdd<BlueprintAbility>("80a1a388ee938aa4e90d427ce9a7a3e9", name_prefix + "HexAbility", ability_guid);
            hex_ability.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            hex_ability.SetName(display_name);
            hex_ability.SetDescription(description);
            var hex_resource = Helpers.CreateAbilityResource(name_prefix + "HexResource", "", "", resource_guid, null);
            hex_resource.SetFixedResource(1);
            hex_ability.AddComponent(Helpers.CreateResourceLogic(hex_resource));

            hex_ability.Type = AbilityType.Supernatural;
            hex_ability.SpellResistance = false;
            var spell_components = hex_ability.GetComponents<Kingmaker.Blueprints.Classes.Spells.SpellComponent>().ToArray();
            foreach (var s in spell_components)
            {
                hex_ability.RemoveComponent(s);
            }

            var life_giver = Helpers.CreateFeature(name_prefix + "HexFeature",
                              hex_ability.Name,
                              hex_ability.Description,
                              feature_guid,
                              hex_ability.Icon,
                              FeatureGroup.None,
                              Helpers.CreateAddFact(hex_ability),
                              Helpers.CreateAddAbilityResource(hex_resource));
            life_giver.Ranks = 1;
            addGrandHexPrerequisite(life_giver);
            return life_giver;
        }


        public BlueprintFeature createEternalSlumber(string name_prefix, string display_name, string description, string ability_guid, string buff_guid,
                                               string feature_guid, string cooldown_guid)
        {
            var touch_of_fatigue_spell = library.Get<BlueprintAbility>("5bf3315ce1ed4d94e8805706820ef64d");
            var sleep_spell = library.Get<BlueprintAbility>("bb7ecad2d3d2c8247a38f44855c99061");
            var dominate_spell = library.Get<BlueprintAbility>("3c17035ec4717674cae2e841a190e757");
            var hex_buff = library.CopyAndAdd<BlueprintBuff>("c9937d7846aa9ae46991e9f298be644a", name_prefix + "HexBuff", buff_guid);
            hex_buff.SetIcon(touch_of_fatigue_spell.Icon);
            hex_buff.RemoveComponent(hex_buff.GetComponent<Kingmaker.UnitLogic.Mechanics.Components.AddIncomingDamageTrigger>());
            var hex_ability = Helpers.CreateAbility(name_prefix + "HexAbility",
                                                    display_name,
                                                    description,
                                                    ability_guid,
                                                    hex_buff.Icon,
                                                    AbilityType.Supernatural,
                                                    CommandType.Standard,
                                                    AbilityRange.Close,
                                                    "Permanent",
                                                    sleep_spell.LocalizedSavingThrow);

            hex_ability.Range = AbilityRange.Touch;
            hex_ability.CanTargetPoint = false;
            hex_ability.CanTargetEnemies = true;
            hex_ability.CanTargetFriends = test_mode;
            hex_ability.CanTargetSelf = test_mode;
            hex_ability.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Point;
            hex_ability.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionPoint;
            hex_ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;

            var target_checker = Helpers.Create<Kingmaker.UnitLogic.Abilities.Components.TargetCheckers.AbilityTargetHasFact>();
            target_checker.CheckedFacts = new BlueprintUnitFact[] { library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f"), //construct
                                                                    library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33") //undead
                                                                  };
            target_checker.Inverted = true;
            hex_ability.AddComponent(target_checker);
            hex_ability.AddComponent(dominate_spell.GetComponent<Kingmaker.UnitLogic.Abilities.Components.Base.AbilitySpawnFx>());
       
            var action = Common.createContextActionSavingThrow(SavingThrowType.Will,
                                                               Helpers.CreateActionList(Common.createContextSavedApplyBuff(hex_buff, DurationRate.Rounds, is_permanent: true, is_dispellable: false))
                                                               );

            hex_ability.AddComponent(Helpers.CreateRunActions(action));
            hex_ability.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: hex_classes));
            var touch = Helpers.Create<Kingmaker.UnitLogic.Abilities.Components.AbilityDeliverTouch>();
            touch.TouchWeapon = library.Get<Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon>("bb337517547de1a4189518d404ec49d4");
            hex_ability.AddComponent(touch);
            hex_ability.AddComponent(sleep_spell.GetComponent<Kingmaker.Blueprints.Classes.Spells.SpellDescriptorComponent>());
            var hex_cooldown = addWitchHexCooldownScaling(hex_ability, cooldown_guid);
            var eternal_slumber = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                      hex_ability.Name,
                                                      hex_ability.Description,
                                                      feature_guid,
                                                      hex_ability.Icon,
                                                      FeatureGroup.None,
                                                      Helpers.CreateAddFact(hex_ability));
            eternal_slumber.Ranks = 1;
            addGrandHexPrerequisite(eternal_slumber);
            addToAmplifyHex(hex_ability);
            return eternal_slumber;
        }


        public BlueprintFeature createCackle(string name_prefix, string display_name, string description, string toggle_ability_guid, string ability_guid, string buff_guid,
                                             string feature_guid, string toggle_ability_name)
        {
            var area_effect = Helpers.Create<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbilityAreaEffect>();
            area_effect.name = name_prefix + "HexAura";
            area_effect.AffectEnemies = true;
            area_effect.Size = 30.Feet();
            area_effect.Shape = AreaEffectShape.Cylinder;

            List<GameAction> actions = new List<GameAction>();
            ContextDurationValue duration = Helpers.CreateContextDuration(bonus: Common.createSimpleContextValue(1), rate: DurationRate.Rounds);
            foreach (var b in cackle_buffs)
            {
                b.Stacking = StackingType.Summ;
                var c = Helpers.CreateConditional(Helpers.CreateConditionHasBuffFromCaster(b), ifTrue: Common.createContextActionApplyBuff(b, duration, dispellable: false));
                actions.Add(c);
            }

            area_effect.AddComponent(Helpers.CreateAreaEffectRunAction(round: actions.ToArray()));
            area_effect.Fx = new Kingmaker.ResourceLinks.PrefabLink();
            library.AddAsset(area_effect, "");

            var energy_drain = library.Get<BlueprintAbility>("37302f72b06ced1408bf5bb965766d46");
            var dirge_of_doom = library.Get<BlueprintBuff>("83eab9b139717ad478d84bbf48ab457f");
            var cackle_buff = Helpers.CreateBuff(name_prefix + "AuraBuff",
                                                              display_name,
                                                              description,
                                                              buff_guid,
                                                              energy_drain.Icon,
                                                              dirge_of_doom.FxOnStart,
                                                              Common.createAddAreaEffect(area_effect),
                                                              Common.createAddCondition(UnitCondition.Staggered)
                                                              );


            var cackle_activatable_ability = Helpers.CreateActivatableAbility(toggle_ability_name,
                                                            cackle_buff.Name,
                                                            cackle_buff.Description,
                                                            toggle_ability_guid,
                                                            energy_drain.Icon,
                                                            cackle_buff,
                                                            AbilityActivationType.Immediately,
                                                            CommandType.Free,
                                                            null);

            var bane = library.Get<BlueprintAbility>("8bc64d869456b004b9db255cdd1ea734");
            var cackle_ability = Helpers.CreateAbility(name_prefix+ "Ability",
                                                           $"{display_name} (Move Action)",
                                                           cackle_activatable_ability.Description,
                                                           ability_guid,
                                                           cackle_activatable_ability.Icon,
                                                           AbilityType.Supernatural,
                                                           CommandType.Move,
                                                           AbilityRange.Personal,
                                                           Helpers.oneRoundDuration,
                                                           Helpers.savingThrowNone,
                                                           Helpers.CreateRunActions(actions.ToArray()),
                                                           Helpers.CreateAbilityTargetsAround(30.Feet(), TargetType.Any, spreadSpeed: 15.Feet()),
                                                           bane.GetComponents<AbilitySpawnFx>().ElementAt(1),
                                                           Common.createAbilityCasterHasNoFacts(cackle_buff)
                                                         );


            var cackle = Helpers.CreateFeature(name_prefix + "HexFeature",
                                          cackle_activatable_ability.Name,
                                          cackle_activatable_ability.Description,
                                          feature_guid,
                                          cackle_ability.Icon,
                                          FeatureGroup.None,
                                          Helpers.CreateAddFact(cackle_activatable_ability),
                                          Helpers.CreateAddFact(cackle_ability)
                                          );
            return cackle;
        }


        static void createHexVulnerabilitySpellAndAccursedHexFeat()
        {
            var hold_person_buff = library.Get<BlueprintBuff>("11cb2fe4fe9c44b448cfe1788ae1ab59");
            var hold_person_spell = library.Get<BlueprintAbility>("c7104f7526c4c524f91474614054547e");

            hex_vulnerability_buff = Helpers.CreateBuff("HexVulnerabilityBuff",
                                                   "Hex Vulnerability",
                                                   "The targeted creature becomes susceptible to a repeat use of your harmful hexes, even if you could not otherwise target that creature with a particular hex for a certain time period.For example, normally after you target a creature with a charm hex, you cannot target it again for 1 day.But after casting this spell on a creature, you could try the charm hex repeatedly as long as the spell persists. The end of this spell has no effect on any active or ongoing hex on a creature.For example, if the creature failed its save against a second use of your charm hex, it remains charmed for the normal duration, even if the spell expires before the hex does.\n"
                                                   + "Each subsequent casting of this spell on a target within a 24 - hour period gives the target a + 4 bonus on its save against the spell.",
                                                   "",
                                                   hold_person_buff.Icon,
                                                   hold_person_buff.FxOnStart
                                                  );
            hex_vulnerability_buff.Stacking = StackingType.Stack;
            var cooldown_buff = Helpers.CreateBuff("HexVulnerabilityCooldownBuff",
                                        "Hex Vulnerability Saving Throw Increase",
                                        hex_vulnerability_buff.Description,
                                        "",
                                        hold_person_buff.Icon,
                                        null
                                       );
            cooldown_buff.SetBuffFlags(BuffFlags.RemoveOnRest | BuffFlags.StayOnDeath);

            var failed_save_actions = new GameAction[] {Common.createContextActionRemoveBuffFromCaster(hex_vulnerability_buff),
                                                        Common.createContextActionApplyBuff(hex_vulnerability_buff,
                                                                                            Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default))
                                                                                           )
                                                       };
            var effect = Common.createContextActionSavingThrow(SavingThrowType.Will,
                                                               Helpers.CreateActionList(Helpers.CreateConditionalSaved(new GameAction[0], failed_save_actions))
                                                               );

            var cooldown = Common.createContextActionApplyBuff(cooldown_buff,
                                                                Helpers.CreateContextDuration(bonus: Common.createSimpleContextValue(24), rate: DurationRate.Hours),
                                                                is_from_spell: true);
            var run_action = Helpers.CreateRunActions(effect, cooldown);
            
            hex_vulnerability_spell = Helpers.CreateAbility("HexVulnerabilityAbility",
                                                hex_vulnerability_buff.Name,
                                                hex_vulnerability_buff.Description,
                                                "",
                                                hex_vulnerability_buff.Icon,
                                                AbilityType.Spell,
                                                CommandType.Standard,
                                                AbilityRange.Close,
                                                Helpers.roundsPerLevelDuration,
                                                "Will Negates",
                                                run_action
                                                );
            cooldown_buff.AddComponent(Common.createSavingThrowBonusAgainstSpecificSpells(4, ModifierDescriptor.UntypedStackable, hex_vulnerability_spell));

            cooldown_buff.Stacking = StackingType.Stack;


            hex_vulnerability_spell.Animation = hold_person_spell.Animation;
            hex_vulnerability_spell.AnimationStyle = hold_person_spell.AnimationStyle;
            hex_vulnerability_spell.CanTargetSelf = false;
            hex_vulnerability_spell.CanTargetEnemies = true;
            hex_vulnerability_spell.CanTargetFriends = test_mode;
            hex_vulnerability_spell.EffectOnAlly = test_mode ? AbilityEffectOnUnit.Harmful : AbilityEffectOnUnit.None;
            hex_vulnerability_spell.SpellResistance = true;
            hex_vulnerability_spell.AvailableMetamagic = Kingmaker.UnitLogic.Abilities.Metamagic.Heighten | Kingmaker.UnitLogic.Abilities.Metamagic.Quicken
                                                        | Kingmaker.UnitLogic.Abilities.Metamagic.Reach | Kingmaker.UnitLogic.Abilities.Metamagic.Extend;
            hex_vulnerability_spell.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            hex_vulnerability_spell.AddComponent(Helpers.CreateSpellComponent(SpellSchool.Necromancy));

            if (!test_mode)
            {
                hex_vulnerability_spell.AddComponent(Common.createAbilityTargetIsPartyMember(false));
            }

            accursed_hex_buff = library.CopyAndAdd<BlueprintBuff>(hex_vulnerability_buff.AssetGuid, "AccursedHexBuff", "");
            accursed_hex_buff.SetName("Accursed Hex");
            accursed_hex_buff.SetDescription("You can make a second attempt at failed hexes.\n"
                                             + "Benefit: When you target a creature with a hex that cannot target the same creature more than once per day, and that creature succeeds at its saving throw against the hex’s effect, you can target the creature with the same hex a second time before the end of your next turn.If the second attempt fails, you can make no further attempts to target that creature with the same hex for 1 day.\n"
                                             + "Normal: You can only target a creature with these hexes once per day.");
            accursed_hex_buff.SetIcon(LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Hex_Accursed.png"));
            accursed_hex_buff.Stacking = StackingType.Ignore;

            accursed_hex_feat = Helpers.CreateFeature("AccursedHexFeature",
                                                      accursed_hex_buff.Name,
                                                      accursed_hex_buff.Description,
                                                      "",
                                                      accursed_hex_buff.Icon,
                                                      FeatureGroup.Feat);
            library.AddFeats(accursed_hex_feat);
        }


        static void createAmplifiedHex()
        {
            amplified_hex_feat = Helpers.CreateFeature("AmplifiedHexFeature",
                                                       "Amplified Hex",
                                                       "You can augment the power of a hex by expending a spell slot or prepared spell of at least 1st level. Each additional time you use this ability in the same day, it requires a prepared spell or spell slot 1 level higher (a 2nd - level spell the second time, a 3rdlevel spell the third time, and so on). When you amplify a hex, its DC increases by 1.",
                                                       "",
                                                       LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Hex_Amplified.png"),
                                                       FeatureGroup.Feat);
           // var circle_of_death = library.Get<BlueprintAbility>("a89dcbbab8f40e44e920cc60636097cf"); //circle of death
            amplified_hex_buff = Helpers.CreateBuff("AmplifiedHexBuff",
                                                     amplified_hex_feat.Name,
                                                     amplified_hex_feat.Description,
                                                     "",
                                                     amplified_hex_feat.Icon,//circle_of_death.Icon,
                                                     null,
                                                     Helpers.Create<NewMechanics.IncreaseSpecifiedSpellsDC>(c =>
                                                     {
                                                         c.BonusDC = 1;
                                                         c.spells = new BlueprintAbility[0];
                                                     })
                                                     );
            amplified_hex_buff.SetBuffFlags(BuffFlags.RemoveOnRest);


            amplified_hex_feat.AddComponent(Helpers.Create<NewMechanics.AbilityUsedTrigger>(a =>
                {
                    a.Actions = Helpers.CreateActionList(Common.createContextActionRemoveBuffFromCaster(amplified_hex_buff, 3));
                })
            );


            amplified_hex_sacrifice_spells[0] = null;

            var action_apply_buff = Common.createContextActionApplyBuff(amplified_hex_buff,
                                                                        Helpers.CreateContextDuration(),
                                                                        is_permanent: true, dispellable: false);

            for (int i = 1; i < amplified_hex_sacrifice_spells.Length; i++)
            {
                var cooldown_buff = Helpers.CreateBuff($"AmplifiedHex{i}CooldownBuff",
                                                       "",
                                                       "",
                                                       "",
                                                       null,
                                                       null);
                cooldown_buff.SetBuffFlags(BuffFlags.RemoveOnRest | BuffFlags.HiddenInUi);
                var action_apply_cooldown_buff = Common.createContextActionApplyBuff(cooldown_buff,
                                                                                     Helpers.CreateContextDuration(),
                                                                                     is_permanent: true, dispellable: false);
                amplified_hex_sacrifice_spells[i] = Helpers.CreateAbility($"AmplifiedHex{i}Ability",
                                                            $"Amplified Hex {Common.roman_id[i]}",
                                                            amplified_hex_buff.Description,
                                                            "",
                                                            amplified_hex_buff.Icon,
                                                            AbilityType.Special,
                                                            CommandType.Free,
                                                            AbilityRange.Personal,
                                                            "",
                                                            "",
                                                            Helpers.CreateRunActions(action_apply_buff, action_apply_cooldown_buff),
                                                            Common.createAbilityCasterHasNoFacts(cooldown_buff, amplified_hex_buff));
            }

            library.AddFeats(amplified_hex_feat);
        }


        void addToAmplifyHex(BlueprintAbility hex)
        {
            var c = amplified_hex_buff.GetComponent<NewMechanics.IncreaseSpecifiedSpellsDC>();
            c.spells = c.spells.AddToArray(hex);

            var cast_trigger = amplified_hex_feat.GetComponent<NewMechanics.AbilityUsedTrigger>();
            cast_trigger.Spells = cast_trigger.Spells.AddToArray(hex);
        }


        static void createSplitHex()
        {
            split_hex_feat = Helpers.CreateFeature("SplitHexFeature",
                                                       "Split Hex",
                                                       "When you use one of your hexes (not a major hex or a grand hex) that targets a single creature, you can apply the same hex to another creature as a free action.",
                                                       "",
                                                       LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Hex_Split.png"),
                                                       FeatureGroup.Feat);

            //var evocation = library.Get<BlueprintFeature>("c46512b796216b64899f26301241e4e6"); //school evocation
            immune_to_split_hex_buff = Helpers.CreateBuff("ImmuneToSplitHexBuff",
                                                            split_hex_feat.Name,
                                                            split_hex_feat.Description,
                                                            "",
                                                            split_hex_feat.Icon,//evocation.Icon,
                                                            null
                                                            );
            immune_to_split_hex_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            library.AddFeats(split_hex_feat);
        }


        BlueprintAbility addToSplitHex(BlueprintAbility hex, bool amplify = false)
        {
            var split_hex = library.CopyAndAdd<BlueprintAbility>(hex.AssetGuid, "SplitHex" + hex.name, "");
            split_hex.SetName("Split Hex : " + hex.Name);
            split_hex.SetIcon(immune_to_split_hex_buff.Icon);
            split_hex.ActionType = CommandType.Free;

            var split_hex_buff = Helpers.CreateBuff("SplitHex" + hex.name + "Buff",
                                                split_hex.Name,
                                                split_hex_feat.Description,
                                                "",
                                                split_hex.Icon,
                                                null,
                                                Helpers.CreateAddFact(split_hex)
                                                );

            split_hex.AddComponent(Common.createAbilityTargetHasNoFactUnlessBuffsFromCaster(new BlueprintBuff[]{immune_to_split_hex_buff}));

            var apply_caster_buff = Common.createContextActionApplyBuff(split_hex_buff,
                                                                        Helpers.CreateContextDuration(Common.createSimpleContextValue(1)),
                                                                        dispellable: false,
                                                                        duration_seconds: 3);

            var apply_target_buff = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(split_hex_feat),
                                                              Common.createContextActionApplyBuff(immune_to_split_hex_buff, 
                                                                                                  Helpers.CreateContextDuration(Common.createSimpleContextValue(1)), dispellable: false)
                                                              );
            var apply_caster_buff2 = Common.createContextActionRemoveBuffFromCaster(split_hex_buff);

            var run_action_original = hex.GetComponent<AbilityEffectRunAction>().CreateCopy();
            run_action_original.addAction(apply_target_buff);
            hex.ReplaceComponent<AbilityEffectRunAction>(run_action_original);

            var spell_trigger_original = Helpers.Create<NewMechanics.AbilityUsedTrigger>();
            spell_trigger_original.Spells = new BlueprintAbility[] { hex };
            spell_trigger_original.Actions = Helpers.CreateActionList(apply_caster_buff);

            var spell_trigger_split = Helpers.Create<NewMechanics.AbilityUsedTrigger>();
            spell_trigger_split.Spells = new BlueprintAbility[] { split_hex };
            spell_trigger_split.Actions = Helpers.CreateActionList(apply_caster_buff2);

            split_hex_feat.AddComponents(spell_trigger_original, spell_trigger_split);


            if (amplify)
            {
                var c = amplified_hex_buff.GetComponent<NewMechanics.IncreaseSpecifiedSpellsDC>();
                c.spells = c.spells.AddToArray(split_hex);
            }

            return split_hex;
        }


    }
}
