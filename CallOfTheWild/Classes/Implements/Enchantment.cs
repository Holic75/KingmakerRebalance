using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Conditions;

namespace CallOfTheWild
{
    public partial class ImplementsEngine
    {
        public BlueprintFeature createCloudMind()
        {
            var daze_buff = library.Get<BlueprintBuff>("9934fedff1b14994ea90205d189c8759");
            var stagger_buff = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");

            var apply_effect = Helpers.CreateConditional(Helpers.Create<ContextConditionHitDice>(c => { c.HitDice = 0; c.AddSharedValue = true; c.SharedValue = AbilitySharedValue.StatBonus; }),
                                                         Common.createContextActionApplyBuff(daze_buff, Helpers.CreateContextDuration(1)),
                                                         Common.createContextActionApplyBuff(stagger_buff, Helpers.CreateContextDuration(1))
                                                         );

            var ability = Helpers.CreateAbility(prefix + "CloudMindAbility",
                                                "Cloud Mind",
                                                "As a standard action, you can expend 1 point of mental focus to cloud the mind of one foe within 30 feet. That foe is dazed for 1 round if the number of Hit Dice it possesses is less than or equal to or your occultist level. If it has more Hit Dice than your occultist level, it is staggered for 1 round instead. The foe can attempt a Will saving throw to negate the effect.",
                                                "",
                                                daze_buff.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Medium,
                                                Helpers.oneRoundDuration,
                                                Helpers.willNegates,
                                                Helpers.CreateRunActions(SavingThrowType.Will, Helpers.CreateConditionalSaved(null, apply_effect)),
                                                createClassScalingConfig(),
                                                Helpers.CreateCalculateSharedValue(Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilityRankType.Default)), AbilitySharedValue.StatBonus),
                                                Helpers.CreateSpellComponent(SpellSchool.Enchantment),
                                                Common.createAbilitySpawnFx("28b3cd92c1fdc194d9ee1e378c23be6b", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                resource.CreateResourceLogic(),
                                                createDCScaling(),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting)
                                                );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            addFocusInvestmentCheck(ability, SpellSchool.Enchantment);
            return Common.AbilityToFeature(ability, false);
        }


        public BlueprintFeature createBindingPattern()
        {
            var paralyze_buff = library.Get<BlueprintBuff>("2cfcce5b62d3e6d4082ec31b58468cc8");
            var stagger_buff = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");

            var apply_paralysis = Common.createContextActionApplySpellBuff(paralyze_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)));
            var apply_stagger = Common.createContextActionApplySpellBuff(stagger_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)));
            var ability = Helpers.CreateAbility(prefix + "BindingPatternAbility",
                                                "Binding Pattern",
                                                "As a standard action, you can expend 1 point of mental focus to create a binding pattern of psychic energy that causes a living creature to become paralyzed. The target must be within 30 feet of you, and it can attempt a Will saving throw to negate the effect. If it fails the save, it is paralyzed for 1 round for every 2 occultist levels you possess. At the end of each of its turns, it can attempt another Will save to end the paralyzed effect and instead be staggered for the remaining duration. This is a mind-affecting compulsion effect. You must be at least 7th level to select this focus power.",
                                                "",
                                                Helpers.GetIcon("41e8a952da7a5c247b3ec1c2dbb73018"), //hold person
                                                AbilityType.SpellLike,
                                                CommandType.Standard,
                                                AbilityRange.Medium,
                                                Helpers.oneRoundDuration,
                                                Helpers.willNegates,
                                                Helpers.CreateRunActions(SavingThrowType.Will, Helpers.CreateConditionalSaved(new GameAction[0],
                                                                                                                              new GameAction[] {apply_paralysis, apply_stagger })),
                                                createClassScalingConfig(ContextRankProgression.Div2, min: 1),
                                                Helpers.CreateSpellComponent(SpellSchool.Enchantment),
                                                resource.CreateResourceLogic(),
                                                createDCScaling(),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Compulsion | SpellDescriptor.Paralysis)
                                                );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            addFocusInvestmentCheck(ability, SpellSchool.Enchantment);
            var feature =  Common.AbilityToFeature(ability, false);
            addMinLevelPrerequisite(feature, 7);
            return feature;
        }


        public BlueprintFeature createObey()
        {
            var ability = Common.convertToSpellLikeVariants(NewSpells.command, prefix, classes, stat, resource, archetypes: getArchetypeArray());

            ability.SetNameDescription("Obey",
                                       "As a standard action, you can issue a command to one living creature by expending 1 point of mental focus.\n"
                                       + "This functions as command.The target must be within 30 feet and capable of understanding your order.The target can attempt a Will save to negate this effect.If the creature is the same creature type as you, it takes a –2 penalty on this saving throw."
                                       );

            string[] names = { "Halt", "Fall", "Run" };
            for (int  i = 0; i < names.Length; i++)
            {
                ability.Variants[i].SetName("Obey (" + names[i] + ")");
            }

            var feature = Common.AbilityToFeature(ability, false);
            feature.AddComponent(Helpers.Create<RaceMechanics.SpellsDCBonusAgainstSameRace>(s =>
            {
                s.value = 2;
                s.spells = new BlueprintAbility[] { ability };
                s.descriptor = ModifierDescriptor.UntypedStackable;
            }));
            addFocusInvestmentCheck(ability, SpellSchool.Enchantment);
            return feature;
        }


        public BlueprintFeature createInspiredAssault()
        {
            var heroism = library.Get<BlueprintAbility>("5ab0d42fb68c9e34abae4921822b9d63");

            var buff = Helpers.CreateBuff(prefix + "InspiredAssaultBuff",
                                          "Inspired Assault",
                                          "As a standard action, you can inspire a living creature with a touch by expending 1 point of mental focus. The creature receives a morale bonus on attack rolls, damage rolls and savingthrows equal to 1 + 1 for every 6 occultist levels you possess(to a maximum bonus of + 4 at 18th level). This bonus lasts for 1 minute per occultist level.",
                                          "",
                                          heroism.Icon,
                                          null,
                                          Helpers.CreateAddContextStatBonus(StatType.AdditionalAttackBonus, ModifierDescriptor.Morale),
                                          Helpers.CreateAddContextStatBonus(StatType.AdditionalDamage, ModifierDescriptor.Morale),
                                          Helpers.CreateAddContextStatBonus(StatType.SaveFortitude, ModifierDescriptor.Morale),
                                          Helpers.CreateAddContextStatBonus(StatType.SaveReflex, ModifierDescriptor.Morale),
                                          Helpers.CreateAddContextStatBonus(StatType.SaveWill, ModifierDescriptor.Morale),
                                          createClassScalingConfig(ContextRankProgression.OnePlusDivStep, stepLevel: 6)
                                          );

            var ability = Helpers.CreateAbility(prefix + "InspiredAssaultAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.SpellLike,
                                                CommandType.Standard,
                                                AbilityRange.Touch,
                                                Helpers.minutesPerLevelDuration,
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionApplySpellBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes))),
                                                createClassScalingConfig(),
                                                Helpers.CreateSpellComponent(SpellSchool.Enchantment),
                                                resource.CreateResourceLogic(),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Compulsion),
                                                heroism.GetComponent<AbilitySpawnFx>()
                                                );
            ability.setMiscAbilityParametersTouchFriendly();
            addFocusInvestmentCheck(ability, SpellSchool.Enchantment);
            var feature = Common.AbilityToFeature(ability, false);
            return feature;
        }


        public BlueprintBuff createGloriousPresence()
        {
            var property = ImplementMechanics.InvestedImplementFocusAmountProperty.createProperty(prefix + "GloriousPresenceProperty", "",
                                                                                                  Helpers.CreateContextValue(AbilityRankType.StatBonus),
                                                                                                  SpellSchool.Enchantment);
            var buff = Helpers.CreateBuff(prefix + "GloriousPresenceBuff",
                                          "Glorious Presence",
                                          "The implement invokes the presence of those who have worn it in the past. The implement’s wearer gains a +1 competence bonus on all Charisma-based skill checks and ability checks for every 2 points of mental focus invested in the implement, to a maximum bonus of 1 + 1 for every 4 occultist levels you possess.",
                                          "",
                                          Helpers.GetIcon("90e59f4a4ada87243b7b3535a06d0638"), //bless
                                          null,
                                          Helpers.CreateAddContextStatBonus(StatType.SkillPersuasion, ModifierDescriptor.Competence),
                                          Helpers.CreateAddContextStatBonus(StatType.SkillUseMagicDevice, ModifierDescriptor.Competence),
                                          Common.createAbilityScoreCheckBonus(Helpers.CreateContextValue(AbilityRankType.Default), ModifierDescriptor.Competence, StatType.Charisma),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, ContextRankProgression.DivStep, stepLevel: 2,
                                                                          customProperty: property),
                                          createClassScalingConfig(ContextRankProgression.StartPlusDivStep, type: AbilityRankType.StatBonus, startLevel: -2, stepLevel: 2)//1 + (lvl + 2)/2 = 2 + lvl/2
                                          );
            return buff;
        }
    }
}
