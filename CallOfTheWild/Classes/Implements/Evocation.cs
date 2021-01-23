using Kingmaker.Blueprints;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

namespace CallOfTheWild
{
    public partial class ImplementsEngine
    {
        public BlueprintFeature createEnergyRay()
        {
            var rays = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("435222be97067a447b2b40d3c58a058e"), //acid
                library.Get<BlueprintAbility>("7ef096fdc8394e149a9e8dced7576fee"), //cold
                library.Get<BlueprintAbility>("96ca3143601d6b242802655336620d91"), //electricity
                library.Get<BlueprintAbility>("cdb106d53c65bbc4086183d54c3b97c7") //fire
            };

            var names = new string[] { "Acid", "Cold", "Electricity", "Fire" };

            var abilities = new BlueprintAbility[rays.Length];

            for (int i = 0; i < abilities.Length; i++)
            {
                var dmg_type = (rays[i].GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionDealDamage).DamageType.Energy;
                var ability = Helpers.CreateAbility(prefix + names[i] + "EnergyRayAbility",
                                                    "Energy Ray: " + names[i],
                                                    $"As a standard action that provokes attacks of opportunity, you can expend 1 point of mental focus to unleash a ray of pure energy as a ranged touch attack. This ray has a range of 25 feet. The ray deals an amount of energy damage equal to 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points + 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points for every 2 occultist levels you possess beyond 1st (2d{BalanceFixes.getDamageDieString(DiceType.D6)} at 3rd level, 3d{BalanceFixes.getDamageDieString(DiceType.D6)} at 5th, and so on, to a maximum of 10d{BalanceFixes.getDamageDie(DiceType.D6)} at 19th level). When you unleash an energy ray, you must decide what type of damage it deals (acid, cold, electricity, or fire).",
                                                    "",
                                                    rays[i].Icon,
                                                    AbilityType.SpellLike,
                                                    CommandType.Standard,
                                                    AbilityRange.Close,
                                                    "",
                                                    "",
                                                    Helpers.CreateRunActions(Helpers.CreateActionDealDamage(dmg_type, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.Default), 0))),
                                                    rays[i].GetComponent<AbilityDeliverProjectile>().CreateCopy(a => { a.Projectiles = new BlueprintProjectile[] { a.Projectiles[0] }; a.UseMaxProjectilesCount = false; }),
                                                    Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                    rays[i].GetComponent<SpellDescriptorComponent>(),
                                                    createClassScalingConfig(ContextRankProgression.StartPlusDivStep, stepLevel: 2, startLevel: 1),
                                                    resource.CreateResourceLogic()
                                                    );
                ability.SpellResistance = true;
                ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);
                abilities[i] = ability;
            }

            var wrapper = Common.createVariantWrapper(prefix + "EnergyRayAbilityBase", "", abilities);
            wrapper.SetName("Energy Ray");
            addFocusInvestmentCheck(wrapper, SpellSchool.Evocation);
            return Common.AbilityToFeature(wrapper, false);
        }


        public BlueprintFeature createEnergyBlast()
        {
            var fx_id = new string[]
            {
                "2e4bb367a72490b46944654f321b91a4", //acid
                "070bd772af57d3c46ae89218032dad80", //cold 
                "448260fa84101684ca32750574089663", //electricity
                "c152c5cb0af124a40bc94087f9e2bb29", //fire 
            };

            var energy = new DamageEnergyType[]
            {
                DamageEnergyType.Acid, DamageEnergyType.Cold, DamageEnergyType.Electricity, DamageEnergyType.Fire
            };

            var descriptors = new SpellDescriptor[]
            {
                SpellDescriptor.Acid, SpellDescriptor.Cold, SpellDescriptor.Electricity, SpellDescriptor.Fire
            };

            var icons = new UnityEngine.Sprite[]
            {
                LoadIcons.Image2Sprite.Create(@"AbilityIcons/AcidBall.png"),
                Helpers.GetIcon("9f10909f0be1f5141bf1c102041f93d9"), //snowball
                NewSpells.aggressive_thundercloud.Icon,
                Helpers.GetIcon("2d81362af43aeac4387a3d4fced489c3"), //fireball
            };

            var names = new string[] { "Acid", "Cold", "Electricity", "Fire" };

            var abilities = new BlueprintAbility[names.Length];

            for (int i = 0; i < abilities.Length; i++)
            {
                var dmg = Helpers.CreateActionDealDamage(energy[i], 
                                                         Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D4), 
                                                                                        Helpers.CreateContextValue(AbilityRankType.Default),
                                                                                        0),
                                                         isAoE: true, halfIfSaved: true);

                var ability = Helpers.CreateAbility(prefix + names[i] + "EnergyBlastAbility",
                                                    "Energy Blast: " + names[i],
                                                    $"As a standard action that provokes attacks of opportunity, you can expend 2 points of mental focus to unleash a blast of energy. This blast has a range of 60 feet, and deals 1d{BalanceFixes.getDamageDieString(DiceType.D4)} points of energy damage per occultist level. The blast deals damage to each creature in a 20-foot-radius burst, but each affected creature can attempt a Reflex save to halve the damage. When you unleash an energy blast, you must decide what type of damage it deals (acid, cold, electricity, or fire). You must be at least 5th level to select this focus power.",
                                                    "",
                                                    icons[i],
                                                    AbilityType.SpellLike,
                                                    CommandType.Standard,
                                                    AbilityRange.Medium,
                                                    "",
                                                    "",
                                                    Helpers.CreateRunActions(SavingThrowType.Reflex ,dmg),
                                                    Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                    Helpers.CreateSpellDescriptor(descriptors[i]),
                                                    createDCScaling(),
                                                    createClassScalingConfig(),
                                                    resource.CreateResourceLogic(amount: 2),
                                                    Common.createAbilitySpawnFx(fx_id[i], anchor: AbilitySpawnFxAnchor.ClickedTarget),
                                                    Helpers.CreateAbilityTargetsAround(20.Feet(), TargetType.Any)
                                                    );
                ability.SpellResistance = true;
                ability.setMiscAbilityParametersRangedDirectional();
                abilities[i] = ability;
            }

            var wrapper = Common.createVariantWrapper(prefix + "EnergyBlastAbilityBase", "", abilities);
            wrapper.SetName("Energy Blast");
            addFocusInvestmentCheck(wrapper, SpellSchool.Evocation);
            var feature = Common.AbilityToFeature(wrapper, false);
            addMinLevelPrerequisite(feature, 5);
            return feature;
        }


        public BlueprintFeature createWallOfPower()
        {
            var areas = new BlueprintAbilityAreaEffect[]
            {
                library.Get<BlueprintAbilityAreaEffect>("2a9cebe780b6130428f3bf4b18270021"), //acid
                library.Get<BlueprintAbilityAreaEffect>("608d84e25f42d6044ba9b96d9f60722a"), //cold
                library.Get<BlueprintAbilityAreaEffect>("2175d68215aa61644ad1d877d4915ece"), //electricity
                library.Get<BlueprintAbilityAreaEffect>("ac8737ccddaf2f948adf796b5e74eee7") //fire
            };

            var icons = new UnityEngine.Sprite[]
            {
                Helpers.GetIcon("1e418794638cf95409f6e33c8c3dbe1a"), //acid wall
                Helpers.GetIcon("e377feb2ecec95e478e0565da621ea55"), //cold wall
                Helpers.GetIcon("8ba05ef69b06ea04c9430427a95685f6"), //elec wall
                Helpers.GetIcon("77d255c06e4c6a745b807400793cf7b1"), //fire wall
            };

            var energy = new DamageEnergyType[]
            {
                DamageEnergyType.Acid, DamageEnergyType.Cold, DamageEnergyType.Electricity, DamageEnergyType.Fire
            };

            var descriptors = new SpellDescriptor[]
            {
                SpellDescriptor.Acid, SpellDescriptor.Cold, SpellDescriptor.Electricity, SpellDescriptor.Fire
            };

            var names = new string[] { "Acid", "Cold", "Electricity", "Fire" };

            var abilities = new BlueprintAbility[areas.Length];


            for (int i = 0; i < abilities.Length; i++)
            {
                var dmg = Helpers.CreateActionDealDamage(energy[i],
                                                         Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6),
                                                                                        2,
                                                                                        Helpers.CreateContextValue(AbilityRankType.Default)),
                                                         isAoE: true);

                var dmg_action = Helpers.CreateActionList(dmg);
                var area = library.CopyAndAdd(areas[i], prefix + names[i] + "WallOfPowerArea", "");
                area.ComponentsArray = new BlueprintComponent[]
                {
                    Helpers.Create<NewMechanics.AbilityAreaEffectRunActionWithFirstRound>(a =>
                    {
                        a.Round = dmg_action;
                        a.UnitEnter = dmg_action;
                    }),
                    createClassScalingConfig()
                };
                area.SpellResistance = true;
                var ability = Helpers.CreateAbility(prefix + names[i] + "WallOfPowerAbility",
                                                    "Wall of Power: " + names[i],
                                                    $"By expending 1 point of mental focus as a standard action, you can create a wall of pure energy with a length of up to 5 feet per occultist level you possess. This wall is 10 feet high and 1 foot thick. It doesn’t block passage, line of sight, or line of effect, but does deal damage to anyone passing through it. The wall deals 2d{BalanceFixes.getDamageDieString(DiceType.D6)} points of energy damage + 1 point of energy damage per occultist level you possess. You must select acid, cold, electricity, or fire when you create the wall to determine the type of damage it deals. The wall lasts for 1 round per occultist level you possess. You must be at least 9th level to select this focus power.",
                                                    "",
                                                    icons[i],
                                                    AbilityType.SpellLike,
                                                    CommandType.Standard,
                                                    AbilityRange.Medium,
                                                    "",
                                                    "",
                                                    Helpers.CreateRunActions(Common.createContextActionSpawnAreaEffect(area, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)))),
                                                    Helpers.CreateSpellDescriptor(descriptors[i]),
                                                    Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                    createClassScalingConfig(),
                                                    resource.CreateResourceLogic()
                                                    );

                ability.setMiscAbilityParametersRangedDirectional();
                abilities[i] = ability;
            }

            var wrapper = Common.createVariantWrapper(prefix + "WallOfPowerAbilityBase", "", abilities);
            wrapper.SetName("Wall of Power");
            addFocusInvestmentCheck(wrapper, SpellSchool.Evocation);
            var feature = Common.AbilityToFeature(wrapper, false);
            addMinLevelPrerequisite(feature, 9);
            return feature;
        }


        public BlueprintFeature createRadiance()
        {
            var icon = library.Get<BlueprintBuff>("50d0501ad05f15d498c3aa8d602af273").Icon; //shocking weapon
            var faerie_fire_buff = library.Get<BlueprintBuff>("cc383a9eaae4d2b45a925d442b367b54");

            var on_hit_buff = Helpers.CreateBuff(prefix + "RadianceEffectBuff",
                                                  "Radiance Effect",
                                                  "As a standard action, you can touch a weapon to cause it to glow with light by expending 1 point of mental focus. The weapon produces light like a torch. For the purpose of darkness spells and abilities, this effect counts as a light spell of level 0, plus 1 additional spell level for every 2 occultist levels you possess. This light lasts for 1 minute per occultist level you possess. In addition, whenever the weapon scores a critical hit against a foe, the wielder can choose to end the effect, causing the foe to be surrounded by the radiance for 1d4 rounds. While illuminated in this way, the target can’t benefit from concealment or invisibility, nor can it attempt Stealth checks to avoid being seen. All attack rolls made against an illuminated foe receive a +2 circumstance bonus.",
                                                  "",
                                                  faerie_fire_buff.Icon,
                                                  null,
                                                  faerie_fire_buff.ComponentsArray
                                                  );
            on_hit_buff.AddComponents(Helpers.Create<AttackBonusAgainstTarget>(a => { a.Value = 2; a.CheckCaster = true; a.CheckCasterFriend = true; }));

            var apply_effect = Common.createContextActionApplyBuff(on_hit_buff, Helpers.CreateContextDuration(0, DurationRate.Rounds, DiceType.D4, 1));
            var active_buff = Helpers.CreateBuff(prefix + "ActiveRadianceBuff",
                                      on_hit_buff.Name,
                                      on_hit_buff.Description,
                                      "",
                                      icon,
                                      null
                                      );
            var toggle = Common.buffToToggle(active_buff, CommandType.Free, true);

            var buff = Helpers.CreateBuff(prefix + "RadianceBuff",
                                          "Radiance",
                                          active_buff.Description,
                                          "",
                                          active_buff.Icon,
                                          null,
                                          Helpers.CreateAddFact(toggle),
                                          Helpers.Create<NewMechanics.EnchantmentMechanics.PersistentWeaponEnchantment>(p => 
                                          {
                                              p.enchant = WeaponEnchantments.dazzling_blade_fx_enchant;
                                              p.secondary_hand = false;
                                              p.only_melee = true;
                                          })
                                          );

            var on_hit_action = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(buff),
                                                          new GameAction[] {apply_effect, Common.createContextActionOnContextCaster(Common.createContextActionRemoveBuff(buff)) }
                                                          );
            active_buff.AddComponent(Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(on_hit_action), critical_hit: true, check_weapon_range_type: true));

            var ability = Helpers.CreateAbility(prefix + "RadianceAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.SpellLike,
                                                CommandType.Standard,
                                                AbilityRange.Touch,
                                                Helpers.minutesPerLevelDuration,
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionApplySpellBuff(buff,
                                                                                                                  Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes))
                                                                        ),
                                                Helpers.Create<AbilityTargetHasMeleeWeaponInPrimaryHand>(),
                                                Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                createClassScalingConfig(),
                                                resource.CreateResourceLogic()
                                                );
            ability.setMiscAbilityParametersTouchFriendly();
            addFocusInvestmentCheck(ability, SpellSchool.Evocation);
            return Common.AbilityToFeature(ability, false);
        }


        public BlueprintBuff createIntenseFocus()
        {
            var property = ImplementMechanics.InvestedImplementFocusAmountProperty.createProperty(prefix + "IntenseFocusProperty", "",
                                                                                                  createClassScalingConfig(ContextRankProgression.BonusValue, type: AbilityRankType.StatBonus, stepLevel: 2),//2 + lvl => 1 + lvl/2
                                                                                                  false,
                                                                                                  SpellSchool.Evocation);
            var buff = Helpers.CreateBuff(prefix + "IntenseFocusBuff",
                                          "Intense Focus",
                                          "The implement channels and enhances the effects of damaging evocations. A spellcaster who bears the implement can add the implement as an additional focus component for any of his damaging evocation spells or focus powers. If he does so, the spell or focus power deals 1 additional point of damage of the same type to each creature for every 2 points of mental focus invested in the implement, to a maximum of 1 + 1 for every 2 occultist levels you possess.",
                                          "",
                                          Helpers.GetIcon("104a9f275539abf44b594e9e36f71694"), //gather power high
                                          null,
                                          Helpers.Create<NewMechanics.ContextValueIntenseSpells>(c => c.value = Helpers.CreateContextValue(AbilityRankType.Default)),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, ContextRankProgression.DivStep, stepLevel: 2,
                                                                          customProperty: property)
                                          
                                          );
            return buff;
        }
    }
}
