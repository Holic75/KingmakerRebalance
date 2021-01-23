using CallOfTheWild.NewMechanics;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    partial class MysteryEngine
    {
        static BlueprintBuff spirirt_boost_hp_buff;
        public BlueprintFeature createChannel(string name_prefix, string display_name, string description)
        {
            var resource = Helpers.CreateAbilityResource(name_prefix + "Resource", "", "", "", null);
            resource.SetIncreasedByStat(1, stat);

            var positive_energy_feature = library.Get<BlueprintFeature>("a79013ff4bcd4864cb669622a29ddafb");
            var context_rank_config = createClassScalingConfig(progression: ContextRankProgression.StartPlusDivStep,
                                                                                  type: AbilityRankType.Default, startLevel: 1, stepLevel: 2);
            var dc_scaling = Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(classes, getArchetypeArray(), stat);
            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                   display_name,
                                                   description,
                                                   "",
                                                   positive_energy_feature.Icon,
                                                   FeatureGroup.None);

            var heal_living = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHeal,
                                                                      name_prefix + "ChannelEnergyHealLiving",
                                                                      "",
                                                                      "",
                                                                      "",
                                                                      context_rank_config,
                                                                      dc_scaling,
                                                                      Helpers.CreateResourceLogic(resource));
            var harm_undead = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHarm,
                                                                        name_prefix + "ChannelEnergyHarmUndead",
                                                                        "",
                                                                        "",
                                                                        "",
                                                                        context_rank_config,
                                                                        dc_scaling,
                                                                        Helpers.CreateResourceLogic(resource));

            var heal_living_base = Common.createVariantWrapper(name_prefix + "PositiveHealBase", "", heal_living);
            var harm_undead_base = Common.createVariantWrapper(name_prefix + "PositiveHarmBase", "", harm_undead);

            ChannelEnergyEngine.storeChannel(heal_living, feature, ChannelEnergyEngine.ChannelType.PositiveHeal);
            ChannelEnergyEngine.storeChannel(harm_undead, feature, ChannelEnergyEngine.ChannelType.PositiveHarm);

            feature.AddComponent(Helpers.CreateAddFacts(heal_living_base, harm_undead_base));
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));
            var extra_channel = ChannelEnergyEngine.createExtraChannelFeat(heal_living, feature, name_prefix + "ExtraChannelDeature", $"Extra Channel ({classes[0].Name})", "");
            return feature;
        }


        //combat healer is the same as in battle mystery


        public BlueprintFeature createEnergyBody(string name_prefix, string display_name, string description)
        {
            var resource = Helpers.CreateAbilityResource(name_prefix + "Resource", "", "", "", null);
            resource.SetIncreasedByLevel(0, 1, classes, getArchetypeArray());

            var icon = library.Get<BlueprintAbility>("1bc83efec9f8c4b42a46162d72cbf494").Icon; //burst of glory


            var heal_living = Helpers.CreateConditional(new Condition[] { Helpers.Create<UndeadMechanics.ContextConditionHasNegativeEnergyAffinity>(),
                                                                          Helpers.Create<ContextConditionIsAlly>()},
                                                       new GameAction[0],
                                                       new GameAction[] {Helpers.Create<ContextActionSpawnFx>(c => c.PrefabLink = Common.createPrefabLink("61602c5b0ac793d489c008e9cb58f631")),
                                                                         Common.createContextActionHealTarget(Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 1, Helpers.CreateContextValue(AbilityRankType.Default)))
                                                                        }
                                             );
            var aura_of_healing = library.Get<BlueprintAbilityAreaEffect>("be47154a20220f64f9bea767587e700a");

            var area_effect = library.CopyAndAdd(aura_of_healing, $"{name_prefix}Area", "");
            area_effect.Size = 5.Feet();
            area_effect.SetComponents(createClassScalingConfig(),
                                      Helpers.CreateAreaEffectRunAction(round: heal_living)
                                      );
            var undead_damage = Helpers.CreateConditional(Helpers.Create<UndeadMechanics.ContextConditionHasNegativeEnergyAffinity>(),
                                                          Helpers.CreateActionDealDamage(DamageEnergyType.PositiveEnergy,
                                                                                         Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 1, Helpers.CreateContextValue(AbilityRankType.Default)),
                                                                                         IgnoreCritical: true
                                                                                         )
                                                         );
            var buff = Helpers.CreateBuff(name_prefix + "Buff",
                                          display_name,
                                          description,
                                          "",
                                          icon,
                                          Common.createPrefabLink("f5eaec10b715dbb46a78890db41fa6a0"), //fiery body
                                          Helpers.CreateAddFact(Common.elemental),
                                          Common.createAddTargetAttackWithWeaponTrigger(action_attacker: Helpers.CreateActionList(undead_damage)),
                                          createClassScalingConfig(),
                                          Common.createAddAreaEffect(area_effect)
                                          );

            var ability = Helpers.CreateActivatableAbility(name_prefix + "ActivatableAbility",
                                                           display_name,
                                                           description,
                                                           "",
                                                           icon,
                                                           buff,
                                                           Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
                                                           CommandType.Standard,
                                                           null,
                                                           Helpers.CreateActivatableResourceLogic(resource, ResourceSpendType.NewRound)
                                                           );
            var feature = Common.ActivatableAbilityToFeature(ability, false);
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));

            return feature;
        }


        public BlueprintFeature createEnchancedCures(string name_prefix, string display_name, string description)
        {
            var icon = library.Get<BlueprintAbility>("0d657aa811b310e4bbd8586e60156a2d").Icon;

            var healing_spells = new BlueprintAbility[]
            {   //cure
                library.Get<BlueprintAbility>("47808d23c67033d4bbab86a1070fd62f"),
                library.Get<BlueprintAbility>("1c1ebf5370939a9418da93176cc44cd9"),
                library.Get<BlueprintAbility>("6e81a6679a0889a429dec9cedcf3729c"),
                library.Get<BlueprintAbility>("0d657aa811b310e4bbd8586e60156a2d"),
                library.Get<BlueprintAbility>("5d3d689392e4ff740a761ef346815074"),
                library.Get<BlueprintAbility>("571221cc141bc21449ae96b3944652aa"),
                library.Get<BlueprintAbility>("0cea35de4d553cc439ae80b3a8724397"),
                library.Get<BlueprintAbility>("1f173a16120359e41a20fc75bb53d449")
            };

            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  display_name,
                                                  description,
                                                  "",
                                                  icon,
                                                  FeatureGroup.None,
                                                  Helpers.Create<HealingMechanics.ExtendHpBonusToCasterLevel>(e => e.spells = healing_spells));
            return feature;
        }


        public BlueprintFeature createHealingHands(string name_prefix, string display_name, string description)
        {
            var icon = library.Get<BlueprintAbility>("f6f95242abdfac346befd6f4f6222140").Icon;
            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                           display_name,
                                                           description,
                                                           "",
                                                           icon,
                                                           FeatureGroup.None,
                                                           Helpers.CreateAddStatBonus(StatType.SkillLoreReligion, 3, ModifierDescriptor.UntypedStackable)
                                                           );
            return feature; 
        }


        public BlueprintFeature createLifeLink(string name_prefix, string display_name, string description)
        {
            var clw = library.Get<BlueprintAbility>("47808d23c67033d4bbab86a1070fd62f");
            var resource = Helpers.CreateAbilityResource(name_prefix + "Resource", "", "", "", null);
            resource.SetIncreasedByLevel(0, 1, classes, getArchetypeArray());

            var spawn_fx = Helpers.Create<ContextActionSpawnFx>(c => c.PrefabLink = clw.GetComponent<AbilitySpawnFx>().PrefabLink);
            var transfer_damage = Helpers.Create<HealingMechanics.ContextActionTransferDamageToCaster>(c => c.Value = 5);
            var new_round_action = Helpers.CreateConditional(Helpers.Create<ContextConditionDistanceToTarget>(c => c.DistanceGreater = Common.medium_range_ft.Feet()),
                                                                                                              Helpers.Create<ContextActionRemoveSelf>(),
                                                                                                              Helpers.CreateConditional(Helpers.Create<ContextConditionHasDamage>(),
                                                                                                                                        new GameAction[] { spawn_fx, transfer_damage }
                                                                                                                                        )
                                                            );
            var buff = Helpers.CreateBuff(name_prefix + "Buff",
                                          display_name,
                                          description,
                                          "",
                                          clw.Icon,
                                          null,
                                          Helpers.CreateAddFactContextActions(deactivated: Common.createContextActionOnContextCaster(Helpers.Create<ResourceMechanics.ContextRestoreResource>(c => c.Resource = resource)),
                                                                              newRound: new_round_action)
                                         );

            buff.SetBuffFlags(BuffFlags.RemoveOnRest);
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);
            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                           display_name,
                                                           description,
                                                           "",
                                                           buff.Icon,
                                                           AbilityType.Supernatural,
                                                           Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                           AbilityRange.Close,
                                                           "Permanent",
                                                           "",
                                                           Helpers.CreateRunActions(apply_buff),
                                                           Helpers.Create<NewMechanics.AbilityTargetHasBuffFromCaster>(a => { a.Buffs = new BlueprintBuff[] { buff }; a.not = true; }),
                                                           Helpers.CreateResourceLogic(resource)
                                                           );
            ability.setMiscAbilityParametersSingleTargetRangedFriendly();
            var dismiss = Helpers.CreateAbility(name_prefix + "DismissAbility",
                                                           "Dismiss: " + display_name,
                                                           description,
                                                           "",
                                                           buff.Icon,
                                                           AbilityType.Supernatural,
                                                           Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                           AbilityRange.Medium,
                                                           "",
                                                           "",
                                                           Helpers.CreateRunActions(Common.createContextActionRemoveBuffFromCaster(buff)),
                                                           Helpers.Create<NewMechanics.AbilityTargetHasBuffFromCaster>(a => a.Buffs = new BlueprintBuff[] { buff })
                                                           );
            dismiss.setMiscAbilityParametersSingleTargetRangedFriendly();
            var feature = Common.AbilityToFeature(ability, hide: false);
            feature.AddComponent(Helpers.CreateAddFact(dismiss));
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));

            return feature;
        }


        public BlueprintFeature createSafeCuring(string name_prefix, string display_name, string description)
        {
            return Helpers.CreateFeature(name_prefix + "Feature",
                                         display_name,
                                         description,
                                         "",
                                         Helpers.GetIcon("7aa83ee3526a946419561d8d1aa09e75"), // arcane combat casting
                                         FeatureGroup.None,
                                         ImmuneToAttackOfOpportunityForSpells.Create(SpellDescriptor.RestoreHP)
                                         );
        }


        public BlueprintFeature createSpiritBoost(string name_prefix, string display_name, string description)
        {
            if (spirirt_boost_hp_buff == null)
            {
                spirirt_boost_hp_buff = library.CopyAndAdd<BlueprintBuff>("4814db563c105e64d948161162715661", "LifeMysterySpiritBoostTemporaryHpBuff", "");
                spirirt_boost_hp_buff.SetNameDescriptionIcon("", "", Helpers.GetIcon("7792da00c85b9e042a0fdfc2b66ec9a8")); //break enchantment
                spirirt_boost_hp_buff.RemoveComponents<ContextCalculateSharedValue>();
                //spirirt_boost_hp_buff.ReplaceComponent<TemporaryHitPointsFromAbilityValue>(t => t.Value = Helpers.CreateContextValue((AbilitySharedValue)100));
                /*spirirt_boost_hp_buff.ComponentsArray = new BlueprintComponent[]
                {
                    Helpers.Create<HealingMechanics.TemporaryHpBonusInternal>(h => h.RemoveWhenHitPointsEnd = true)
                }*/;             
            }

            return Helpers.CreateFeature(name_prefix + "Feature",
                                         display_name,
                                         description,
                                         "",
                                         Helpers.GetIcon("7792da00c85b9e042a0fdfc2b66ec9a8"), // break enchantment
                                         FeatureGroup.None,
                                         Helpers.Create<HealingMechanics.HealingWithOverflowToTemporaryHp>(h =>
                                                                                                          { h.temporary_hp_buff = spirirt_boost_hp_buff;
                                                                                                              h.duration = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                                                              h.max_hp = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                                                              h.shared_value_type = AbilitySharedValue.Heal;
                                                                                                           }
                                                                                                          ),
                                         createClassScalingConfig()
                                         );
        }


    }
}
