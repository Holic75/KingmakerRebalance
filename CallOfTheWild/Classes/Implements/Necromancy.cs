using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    public partial class ImplementsEngine
    {
        public BlueprintFeature createMindFear()
        {
            var frightened_buff = library.Get<BlueprintBuff>("f08a7239aa961f34c8301518e71d4cdf");
            var shaken_buff = library.Get<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220");

            var apply_effect = Helpers.CreateConditional(Helpers.Create<ContextConditionHitDice>(c => { c.HitDice = 0; c.AddSharedValue = true; c.SharedValue = AbilitySharedValue.StatBonus; }),
                                                         Common.createContextActionApplyBuff(frightened_buff, Helpers.CreateContextDuration(0, DurationRate.Rounds, DiceType.D4, 1)),
                                                         Common.createContextActionApplyBuff(shaken_buff, Helpers.CreateContextDuration(0, DurationRate.Rounds, DiceType.D4, 1))
                                                         );

            var ability = Helpers.CreateAbility(prefix + "MindFearAbility",
                                                "Mind Fear",
                                                "As a standard action, you can expend 1 point of mental focus to cause a living creature to succumb to fear.\n"
                                                + "The target must be within 30 feet of you, and it can attempt a Will saving throw to negate the effect. If the target fails the save and has a number of Hit Dice less than or equal to yours, it is frightened for 1d4 rounds. If the target fails the saving throw and has a number of Hit Dice greater than yours, it is instead shaken for 1d4 rounds. This is a mind-affecting fear effect.",
                                                "",
                                                Helpers.GetIcon("bd81a3931aa285a4f9844585b5d97e51"), //cause fear
                                                AbilityType.SpellLike,
                                                CommandType.Standard,
                                                AbilityRange.Close,
                                                "1d4 rounds",
                                                Helpers.willNegates,
                                                Helpers.CreateRunActions(SavingThrowType.Will, Helpers.CreateConditionalSaved(null, apply_effect)),
                                                createClassScalingConfig(),
                                                createDCScaling(),
                                                Helpers.CreateCalculateSharedValue(Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilityRankType.Default)), AbilitySharedValue.StatBonus),
                                                Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.Fear | SpellDescriptor.Shaken | SpellDescriptor.Emotion | SpellDescriptor.NegativeEmotion | SpellDescriptor.MindAffecting),
                                                Common.createAbilitySpawnFx("cbfe312cb8e63e240a859efaad8e467c", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                Common.createAbilityTargetHasFact(true, Common.undead, Common.construct),
                                                resource.CreateResourceLogic()
                                                );
            ability.SpellResistance = true;
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            addFocusInvestmentCheck(ability, SpellSchool.Necromancy);
            return Common.AbilityToFeature(ability, false);
        }


        public BlueprintFeature createFleshRot()
        {
            var icon = NewSpells.ghoul_touch.Icon;
            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Unholy, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D8), Helpers.CreateContextValue(AbilityRankType.DamageDice), Helpers.CreateContextValue(AbilityRankType.DamageBonus)));
            var ability = Helpers.CreateAbility(prefix + "FleshRotAbility",
                                                "Flesh Rot",
                                                $"As a standard action, you can make a melee touch attack and expend 1 point of mental focus to cause the flesh of a living creature to rot and wither. If the attack hits, the target takes 1d{BalanceFixes.getDamageDieString(DiceType.D8)} points of damage + 1 point per occultist level you possess. For every 4 occultist levels you possess beyond 3rd, the target takes an additional 1d{BalanceFixes.getDamageDieString(DiceType.D8)} points of damage (to a maximum of 5d{BalanceFixes.getDamageDieString(DiceType.D8)} at 19th level). If you miss with the melee touch attack, this power is wasted with no effect. You must be at least 3rd level to select this focus power.",
                                                "",
                                                icon,
                                                AbilityType.SpellLike,
                                                CommandType.Standard,
                                                AbilityRange.Touch,
                                                "",
                                                "",
                                                Helpers.CreateRunActions(Helpers.CreateConditional(Common.createContextConditionHasFacts(false, Common.undead, Common.elemental, Common.construct), 
                                                                                                   null,
                                                                                                   dmg)
                                                                        ),
                                                createClassScalingConfig(type: AbilityRankType.DamageBonus),
                                                createClassScalingConfig(type: AbilityRankType.DamageDice, progression: ContextRankProgression.StartPlusDivStep, startLevel: 3, stepLevel: 4),
                                                Common.createAbilitySpawnFx("9a38d742801be084d89bd34318c600e8", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                Common.createAbilityTargetHasFact(true, Common.undead, Common.construct, Common.elemental),
                                                Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                Helpers.CreateDeliverTouch()
                                                );
            ability.setMiscAbilityParametersTouchHarmful();
            ability.SpellResistance = true;
            var cast_ability = Helpers.CreateTouchSpellCast(ability, resource);
            cast_ability.AddComponent(Common.createAbilityTargetHasFact(true, Common.undead, Common.construct, Common.elemental));
            var feature = Common.AbilityToFeature(cast_ability, false);
            addMinLevelPrerequisite(feature, 3);
            addFocusInvestmentCheck(cast_ability, SpellSchool.Necromancy);
            return feature;
        }


        public BlueprintFeature createPainWave()
        {
            var sickened = library.Get<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323");
            var apply_sickened = Common.createContextActionApplySpellBuff(sickened, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)));
            var apply_sickened1 = Common.createContextActionApplySpellBuff(sickened, Helpers.CreateContextDuration(1));

            var effect = Helpers.CreateConditionalSaved(apply_sickened1, apply_sickened);
            var ability = Helpers.CreateAbility(prefix + "PainWaveAbility",
                                     "Pain Wave",
                                     "As a standard action, you can expend 1 point of mental focus to unleash a wave of pain. This wave hits all creatures other than you in a 20-foot-radius burst centered on a point that you designate within medium range. All living creatures in this area are wracked with pain, gaining the sickened condition for 1 round per occultist level you possess. Affected creatures can attempt a Will save to reduce the duration to just 1 round. This is a mind-affecting pain effect. You must be at least 7th level to select this focus power.",
                                     "",
                                     sickened.Icon,
                                     AbilityType.Supernatural,
                                     CommandType.Standard,
                                     AbilityRange.Medium,
                                     "",
                                     "Will partial",
                                     createClassScalingConfig(),                                   
                                     Helpers.CreateRunActions(SavingThrowType.Will, effect),
                                     Helpers.CreateAbilityTargetsAround(20.Feet(), Kingmaker.UnitLogic.Abilities.Components.TargetType.Any),
                                     Common.createAbilitySpawnFx("872f843a900d8f442896e5fdae6d44d1", anchor: AbilitySpawnFxAnchor.ClickedTarget),
                                     Helpers.CreateResourceLogic(resource),
                                     createDCScaling(),
                                     Helpers.CreateSpellDescriptor(SpellDescriptor.Death | SpellDescriptor.Sickened)
                                    );
            ability.setMiscAbilityParametersRangedDirectional();
            addFocusInvestmentCheck(ability, SpellSchool.Necromancy);
            var feature = Common.AbilityToFeature(ability, false);
            addMinLevelPrerequisite(feature, 7);
            return feature;
        }


        public BlueprintFeature createSoulboundPuppet()
        {
            var display_name = "Soulbound Puppet";
            var description = "As a full-round action, you can expend 1 point of mental focus to create a soulbound puppet from a bone, doll, or skull. If you use a bone or a skull, your power builds a Tiny or Small flesh puppet around it that vaguely resembles the original creature from which the bones were taken. If the implement is a doll, the doll comes to life. Treat this as a familiar, using your occultist level as your wizard level to determine its powers and abilities. You can have no more than one soulbound puppet active at any given time.\n"
                              + "The soulbound puppet remains animated for 10 minutes per occultist level you possess.";
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/SoulboundPuppet.png");
            var familiar_selection = library.Get<BlueprintFeatureSelection>("363cab72f77c47745bf3a8807074d183");
            var abilities = new List<BlueprintAbility>();
            var buffs = new List<BlueprintBuff>();

            var remove_buffs = Helpers.Create<NewMechanics.ContextActionRemoveBuffs>(b => b.Buffs = new BlueprintBuff[0]);
            
            foreach (var f in familiar_selection.AllFeatures)
            {
                var buff = Helpers.CreateBuff(prefix + f.name + "SoulboundPuppetBuff",
                                                 display_name + ": " + f.Name,
                                                 description + "\n" + f.Description,
                                                 "",
                                                 icon,
                                                 null,
                                                 Helpers.CreateAddFact(f)
                                                 );
                remove_buffs.Buffs = remove_buffs.Buffs.AddToArray(buff);

                var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.TenMinutes), dispellable: false);
                var ability = Helpers.CreateAbility(prefix + f.name + "SoulboundPuppetAbility",
                                                   buff.Name,
                                                   buff.Description,
                                                   "",
                                                   buff.Icon,
                                                   AbilityType.Supernatural,
                                                   CommandType.Standard,
                                                   AbilityRange.Personal,
                                                   Helpers.tenMinPerLevelDuration,
                                                   "",
                                                   Helpers.CreateRunActions(apply_buff),
                                                   createClassScalingConfig(),
                                                   Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(remove_buffs)),
                                                   Common.createAbilityCasterHasNoFacts(familiar_selection.AllFeatures),
                                                   resource.CreateResourceLogic()
                                                   );
                Common.setAsFullRoundAction(ability);
                ability.setMiscAbilityParametersSelfOnly();
                abilities.Add(ability);
            }

            var wrapper = Common.createVariantWrapper(prefix + "SoulboundPuppetAbilityBase", "", abilities.ToArray());
            wrapper.SetNameDescription(display_name, description);
            addFocusInvestmentCheck(wrapper, SpellSchool.Necromancy);
            return Common.AbilityToFeature(wrapper);
        }


        public BlueprintFeature createSpiritShroud()
        {
            var buff = library.CopyAndAdd<BlueprintBuff>("0fdb3cca6744fd94b9436459e6d9b947", prefix + "SpiritShroudBuff", "");
            buff.RemoveComponents<ContextRankConfig>();
            buff.AddComponents(createClassScalingConfig(),
                               createClassScalingConfig(ContextRankProgression.OnePlusDivStep, AbilityRankType.StatBonus, stepLevel: 4),
                               Common.createContextSavingThrowBonusAgainstDescriptor(Helpers.CreateContextValue(AbilityRankType.StatBonus), ModifierDescriptor.Resistance, SpellDescriptor.Death | SpellDescriptor.Fear)
                               );
            buff.ReplaceComponent<ContextCalculateSharedValue>(c => c.Value = Helpers.CreateContextDiceValue(DiceType.D6, 1, Helpers.CreateContextValue(AbilityRankType.Default)));

            var ability = Helpers.CreateAbility(prefix + "SpiritShroudAbility",
                                                "Spirit Shroud",
                                                "As a standard action, you can expend 1 point of mental focus to surround yourself with a shroud of spirit energy. You gain a number of temporary hit points equal to 1d6 + your occultist level. This shroud lasts for 1 minute per occultist level or until the temporary hit points are expended, Whichever comes first. These temporary hit points stack with those from other sources, but not with those gained through multiple uses of this ability. As long as the shroud remains, you also gain a resistance bonus on all saving throws against death effects, fear effects, and any spells or effects that bestow negative levels or deal negative energy damage (if the spells or effects allow a save). This bonus is equal to 1 + 1 for every 4 occultist levels you possess. You must be at least 3rd level to select this focus power.",
                                                "",
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Personal,
                                                Helpers.minutesPerLevelDuration,
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), dispellable: false)),
                                                Common.createAbilitySpawnFx("e93261ee4c3ea474e923f6a645a3384f", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                createClassScalingConfig(),
                                                resource.CreateResourceLogic()
                                                );
            addFocusInvestmentCheck(ability, SpellSchool.Necromancy);
            var feature = Common.AbilityToFeature(ability, false);
            addMinLevelPrerequisite(feature, 3);
            return feature;
        }


        public BlueprintFeature createNecromanticServant()
        {
            var dispaly_name = "Necromantic Servant";
            var description = "As a standard action, you can expend 1 point of mental focus to raise a single dead creature as a skeleton to serve you for 10 minutes per occultist level you possess or until it is destroyed, whichever comes first. This servant has a number of HD equal to your occultist level. It also gains a bonus on damage rolls equal to 1/2 your occultist level. At 9th level, you can give the servant the bloody simple template. At 13th level, you can use this ability to animate all the dead bodies in a 10-foot burst. You can have a maximum number of servants in existence equal to 1/2 your occultist level. At 17th level, the servants gain all your teamwork feats.\n"
                + "You must be at least 3rd level to select this focus power.";
            var tactical_leader_feat_share_buff = library.Get<BlueprintBuff>("a603a90d24a636c41910b3868f434447");
            tactical_leader_feat_share_buff.SetNameDescription("", ""); //remove description to pick it up from parent spell

            var bloody_template_buff = Helpers.CreateBuff(prefix + "NecromanticServantBloodyTemplateBuff",
                                                       "Bloody Skeleton",
                                                       "Bloody Skeletons have fast healing equal to 1/2 their HD and receive +4 bonus to their Charisma.",
                                                       "",
                                                       null,
                                                       null,
                                                       Helpers.CreateAddStatBonus(StatType.Charisma, 4, ModifierDescriptor.Feat),
                                                       Common.createAddContextEffectFastHealing(Helpers.CreateContextValue(AbilityRankType.Default)),
                                                       createClassScalingConfig(ContextRankProgression.Div2)
                                                       );

            var damage_bonus_buff = Helpers.CreateBuff(prefix + "NecromanticServantDamageBuff",
                                                       "",
                                                       "",
                                                       "",
                                                       null,
                                                       null,
                                                       Helpers.CreateAddContextStatBonus(StatType.AdditionalDamage, ModifierDescriptor.UntypedStackable),
                                                       createClassScalingConfig(ContextRankProgression.Div2)
                                                       );

            var apply_teamwork_feats = Common.createContextActionApplyBuff(tactical_leader_feat_share_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);
            var apply_bloody_template = Common.createContextActionApplyBuff(bloody_template_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);
            var apply_damage_bonus = Common.createContextActionApplyBuff(damage_bonus_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);

            var extra_actions = Common.createRunActionsDependingOnContextValueIgnoreNegative(Helpers.CreateContextValue(AbilityRankType.DamageDice),
                                                                                             Helpers.CreateActionList(apply_bloody_template),
                                                                                             Helpers.CreateActionList(apply_bloody_template, apply_teamwork_feats)
                                                                                             );


            var summon_pool = library.CopyAndAdd<BlueprintSummonPool>("490248a826bbf904e852f5e3afa6d138", prefix + "NecromanticServant", "");
            var ability_single = library.CopyAndAdd(NewSpells.animate_dead_lesser, prefix + "NecromanticServantSingleAbility", "");
            ability_single.Type = AbilityType.SpellLike;
            ability_single.MaterialComponent = library.Get<BlueprintAbility>("2d81362af43aeac4387a3d4fced489c3").MaterialComponent;
            ability_single.ReplaceComponent<AbilityEffectRunAction>(a =>
            {
                a.Actions = Helpers.CreateActionList(Common.replaceActions<DeadTargetMechanics.ContextActionAnimateDead>(a.Actions.Actions, r =>
                {
                    var animate_fixed_hd = Helpers.Create<DeadTargetMechanics.ContextActionAnimateDeadFixedHD>();
                    animate_fixed_hd.adapt_size = true;
                    animate_fixed_hd.AfterSpawn = Helpers.CreateActionList(r.AfterSpawn.Actions.AddToArray(apply_damage_bonus, extra_actions));
                    animate_fixed_hd.Blueprint = r.Blueprint;
                    animate_fixed_hd.dex_bonus = r.dex_bonus;
                    animate_fixed_hd.SummonPool = summon_pool;
                    animate_fixed_hd.str_bonus = r.str_bonus;
                    animate_fixed_hd.transfer_equipment = r.transfer_equipment;
                    animate_fixed_hd.hd = Helpers.CreateContextValue(AbilityRankType.Default);
                    animate_fixed_hd.max_units = Helpers.CreateContextValue(AbilityRankType.DamageDiceAlternative);
                    animate_fixed_hd.DurationValue = r.DurationValue;
                    return animate_fixed_hd;
                }));
            });
            ability_single.RemoveComponents<ContextRankConfig>();
            ability_single.RemoveComponents<SpellListComponent>();
            ability_single.AddComponents(resource.CreateResourceLogic(),
                                         createClassScalingConfig(),
                                         createClassScalingConfig(ContextRankProgression.Div2, AbilityRankType.DamageDiceAlternative),
                                         createClassScalingConfig(ContextRankProgression.DelayedStartPlusDivStep, AbilityRankType.DamageDice, 
                                                                  startLevel: 9, stepLevel: 8)
                                         );
            ability_single.ReplaceComponent<DeadTargetMechanics.AbilityTargetCanBeAnimated>(a => a.max_size = Size.Colossal);
            ability_single.SetNameDescription(dispaly_name, description);

            var ability_multiple = library.CopyAndAdd(ability_single, prefix + "NecromanticServantMultileAbility", "");
            ability_multiple.setMiscAbilityParametersRangedDirectional();
            ability_multiple.RemoveComponents<DeadTargetMechanics.AbilityTargetCanBeAnimated>();
            ability_multiple.AddComponent(Helpers.CreateAbilityTargetsAround(10.Feet(), TargetType.Any, includeDead: true));

            var feature_single = Common.AbilityToFeature(ability_single);
            var feature_multiple = Common.AbilityToFeature(ability_multiple);
            addFocusInvestmentCheck(ability_single, SpellSchool.Necromancy);
            addFocusInvestmentCheck(ability_multiple, SpellSchool.Necromancy);
            var feature = Helpers.CreateFeature(prefix + "NecromanticServantFeature",
                                                dispaly_name,
                                                description,
                                                "",
                                                ability_single.Icon,
                                                FeatureGroup.None,
                                                createAddFeatureInLevelRange(feature_single, 0, 12),
                                                createAddFeatureInLevelRange(feature_multiple, 13, 100)
                                                );
            addMinLevelPrerequisite(feature, 3);
            return feature;
        }


        public BlueprintBuff createNecromanticFocus()
        {
            var property = ImplementMechanics.InvestedImplementFocusAmountProperty.createProperty(prefix + "NecromanticFocusProperty", "",
                                                                                                  createClassScalingConfig(ContextRankProgression.MultiplyByModifier, type: AbilityRankType.StatBonus, startLevel: 2),
                                                                                                  false,
                                                                                                  SpellSchool.Necromancy);
            var buff = Helpers.CreateBuff(prefix + "NecromanticFocusBuff",
                                          "Necromantic Focus",
                                          "The implement grants its possessor greater power over the undead. Whoever possesses the implement can control an additional 2 Hit Dice of undead for every point of mental focus invested in the item (to a maximum number of Hit Dice equal to 4 × your occultist level).\n"
                                          + "Undead creatures take a –1 penalty on saving throws against spells cast by the bearer of this implement for every 4 points of mental focus invested in this implement.",
                                          "",
                                          NewSpells.control_undead.Icon,
                                          null,
                                          Helpers.Create<NewMechanics.SpellsDCBonusAgainstFact>(s =>
                                          {
                                              s.value = Helpers.CreateContextValue(AbilityRankType.Default);
                                              s.descriptor = ModifierDescriptor.UntypedStackable;
                                              s.fact = Common.undead;
                                          }
                                          ),
                                          Helpers.Create<DeadTargetMechanics.IncreaseMaxUndeadHD>(i => i.value = Helpers.CreateContextValue(AbilityRankType.DamageBonus)),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, ContextRankProgression.DivStep, stepLevel: 4,
                                                                          customProperty: property),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, ContextRankProgression.MultiplyByModifier, type: AbilityRankType.DamageBonus,
                                                                          customProperty: property, stepLevel: 2)                                          
                                          );
            return buff;
        }


        public BlueprintFeature createLifeDrain(BlueprintAbilityResource initial_resource)
        {
            var enervation = library.Get<BlueprintAbility>("f34fb78eaaec141469079af124bcfa0f");

            var actions = new ActionList[4];

            for (int i = 0; i < actions.Length; i++)
            {
                var level_drain = Helpers.CreateActionEnergyDrain(Helpers.CreateContextDiceValue(DiceType.Zero, 0, i + 1),
                                                                  Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes),
                                                                  Kingmaker.RuleSystem.Rules.EnergyDrainType.Temporary
                                                                  );

                var recover_hp = Common.createContextActionOnContextCaster(Common.createContextActionHealTarget(Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilityRankType.Default))));

                var effect = Helpers.CreateConditional(Common.createContextConditionHasFacts(false, Common.undead, Common.elemental, Common.construct),
                                                       new GameAction[0],
                                                       new GameAction[] { level_drain}.AddToArray(Enumerable.Repeat(recover_hp, i + 1))
                                                       );
                actions[i] = Helpers.CreateActionList(effect);
            }

            var ability = Helpers.CreateAbility(prefix + "LifeDrainAbility",
                                                "Life Drain",
                                                "At 8th level, a necroccultist can expend 1 point of mental focus from a necromancy implement to unleash a life-draining ray as a ranged touch attack that causes the target to accrue 1d4 negative levels. The ray has a medium range. The negative levels last a number of minutes equal to the necroccultist’s occultist level.\n"
                                                + "For each negative level this attack gives a target, the necroccultist regains a number of hit points equal to his occultist level. The necroccultist can use this ability once per day at 8th level, plus one additional time per day for every 3 occultist levels thereafter.",
                                                "",
                                                enervation.Icon,
                                                AbilityType.SpellLike,
                                                CommandType.Standard,
                                                AbilityRange.Medium,
                                                "",
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionRandomize(actions)),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.Death),
                                                Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                enervation.GetComponent<AbilityDeliverProjectile>(),
                                                createClassScalingConfig(),
                                                createDCScaling(),
                                                initial_resource.CreateResourceLogic()
                                                );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            ability.SpellResistance = true;

            addFocusInvestmentCheck(ability, SpellSchool.Necromancy);

            return Common.AbilityToFeature(ability, false);
        }


        public BlueprintFeature createGhostlyHorde(BlueprintAbilityResource initial_resource)
        {
            var display_name = "Ghostly Horde";
            var description = "At 5th level, a necroccultist can spend 1 point of mental focus from a necromancy implement to summon a mob of ghostly spirits to harass his enemies.\n"
                               + "Summoning the ghostly horde is a standard action that doesn’t provoke attacks of opportunity. The ghostly horde appears within 60 feet of the necroccultist, fills an area 20 feet in diameter, and always hovers a few inches off the ground (thus ignoring any difficult terrain). The necroccultist can command the ghostly horde to move up to 30 feet each round as a move action, though the farthest edge of the ghostly horde can never be more than 60 feet from the necroccultist.\n"
                               + $"Any creature caught inside the ghostly horde takes 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage per 2 occultist levels at the start of its turn(up to a maximum of 10d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage at 20th level). A successful Fortitude save (DC = 10 + 1/2 the necroccultist’s occultist level +the necroccultist’s Intelligence modifier) halves this damage.This damage is not negative energy—it manifests in the form of physical wounds and aches as if from supernatural aging.Nonliving creatures and creatures immune to magical aging are immune to this damage, but otherwise the damage bypasses all forms of damage reduction. The ghostly horde remains for 1 round, though at the beginning of his turn, the necroccultist can expend 1 point of mental focus as a swift action to extend the duration for 1 additional round. The necroccultist can use this ability once per day at 5th level, plus one additional time per day for every 5 occultist levels thereafter.";
            var icon = library.Get<BlueprintAbility>("b24583190f36a8442b212e45226c54fc").Icon; //wail of banshee

            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("6fa0adacca8d00f4aaba1e8df77a318f", prefix + "GhostlyHordeArea", "");


            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Unholy, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.Default)), halfIfSaved: true, isAoE: true);
            

            var actions = Helpers.CreateActionList(Helpers.CreateConditional(Common.createContextConditionHasFacts(false, Common.undead, Common.elemental, Common.construct),
                                                                             null,
                                                                             Helpers.CreateActionSavingThrow(SavingThrowType.Fortitude, dmg))
                                                  );

            area.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.Create<NewMechanics.AbilityAreaEffectRunActionWithFirstRound>(a =>
                {
                    a.UnitEnter = Helpers.CreateActionList();
                    a.Round = actions;
                    a.FirstRound = Helpers.CreateActionList();
                }),
                Helpers.CreateSpellDescriptor(SpellDescriptor.Death),
                createDCScaling(),
                createClassScalingConfig()
            };

            var caster_buff = Helpers.CreateBuff(prefix + "GhostlyHordeCasterBuff",
                                                  display_name + " (Caster)",
                                                  description,
                                                  "",
                                                  icon,
                                                  null
                                                  );
            caster_buff.AddComponent(Helpers.CreateAddFactContextActions(deactivated: Helpers.Create<NewMechanics.RemoveUniqueArea>(a => a.feature = caster_buff)));
            area.AddComponent(Helpers.Create<UniqueAreaEffect>(u => u.Feature = caster_buff));
            caster_buff.Stacking = StackingType.Summ;

            var spawn_area = Common.createContextActionSpawnAreaEffect(area, Helpers.CreateContextDuration(100));


            var move_ability = Helpers.CreateAbility(prefix + "GhostlyHordeMoveAbility",
                                                     display_name,
                                                     description,
                                                     "",
                                                     icon,
                                                     AbilityType.Supernatural,
                                                     UnitCommand.CommandType.Move,
                                                     AbilityRange.Medium,
                                                     "",
                                                     "Fortitude half",
                                                     Helpers.CreateRunActions(spawn_area),
                                                     Common.createAbilityAoERadius(20.Feet(), TargetType.Any),
                                                     Helpers.CreateSpellDescriptor(SpellDescriptor.Death),
                                                     Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                     Common.createAbilityCasterHasFacts(caster_buff)
                                                     );

            move_ability.setMiscAbilityParametersRangedDirectional();
          
            //caster_buff.AddComponent(Helpers.CreateAddFact(move_ability));
            caster_buff.AddComponent(Helpers.Create<ReplaceAbilityParamsWithContext>(r => r.Ability = move_ability));


            var apply_caster_buff = Common.createContextActionApplyBuffToCaster(caster_buff, Helpers.CreateContextDuration(1));
            var apply_caster_buff_init = Common.createContextActionApplyBuffToCaster(caster_buff, Helpers.CreateContextDuration(), duration_seconds: 9);

            var ability = Helpers.CreateAbility(prefix + "GhostlyHordeAbility",
                                                 display_name,
                                                 description,
                                                 "",
                                                 icon,
                                                 AbilityType.Supernatural,
                                                 UnitCommand.CommandType.Standard,
                                                 AbilityRange.Medium,
                                                 Helpers.oneRoundDuration,
                                                 "Fortitude half",
                                                 Helpers.CreateRunActions(spawn_area),
                                                 createDCScaling(),
                                                 Common.createAbilityAoERadius(20.Feet(), TargetType.Any),
                                                 Helpers.CreateSpellDescriptor(SpellDescriptor.Death),
                                                 Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                 Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(apply_caster_buff_init)),
                                                 initial_resource.CreateResourceLogic()
                                                 );
            ability.setMiscAbilityParametersRangedDirectional();

            var prolong_ability = Helpers.CreateAbility(prefix + "GhostlyHordeProlongAbility",
                                                 display_name + " Prolongation",
                                                 description,
                                                 "",
                                                 icon,
                                                 AbilityType.Spell,
                                                 UnitCommand.CommandType.Swift,
                                                 AbilityRange.Personal,
                                                 Helpers.oneRoundDuration,
                                                 "",
                                                 Helpers.CreateRunActions(apply_caster_buff),
                                                 resource.CreateResourceLogic(),
                                                 Common.createAbilityCasterHasFacts(caster_buff)
                                                 );

            prolong_ability.setMiscAbilityParametersSelfOnly();

            addFocusInvestmentCheck(ability, SpellSchool.Necromancy);
           

            var wrapper = Common.createVariantWrapper(prefix + "GhostlyHordeBaseAbility", "", ability, move_ability, prolong_ability);
            var feature = Common.AbilityToFeature(wrapper, false);
            return feature;
        }
    }
}
