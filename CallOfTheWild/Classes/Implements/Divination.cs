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

namespace CallOfTheWild
{
    public partial class ImplementsEngine
    {
        public BlueprintFeature createSuddenInsight()
        {
            var guidance_buff = library.Get<BlueprintBuff>("ec931b882e806ce42906597e5585c13f");

            var buff = Helpers.CreateBuff(prefix + "SuddenInsightBuff",
                                          "Sudden Insight",
                                          "As a swift action, you can expend 1 point of mental focus to gain an insight into your immediate future.\n"
                                          + "You gain an insight bonus on that roll equal to 1/2 your occultist level (minimum + 1) on your next ability check, attack roll, or skill check. If it’s not used by the end of your turn, the insight fades and you gain no benefit.",
                                          "",
                                          guidance_buff.Icon,
                                          null,
                                          Helpers.CreateAddContextStatBonus(StatType.AdditionalAttackBonus, ModifierDescriptor.Insight),
                                          Common.createAbilityScoreCheckBonus(Helpers.CreateContextValue(AbilityRankType.Default), ModifierDescriptor.Insight),
                                          Helpers.Create<BuffAllSkillsBonus>(b => { b.Value = 1; b.Descriptor = ModifierDescriptor.Insight; b.Multiplier = Helpers.CreateContextValue(AbilityRankType.Default); }),
                                          createClassScalingConfig(progression: ContextRankProgression.Div2, min: 1)
                                          );

            var remove_self = Helpers.CreateActionList(Common.createContextActionRemoveBuff(buff));
            buff.AddComponents(guidance_buff.GetComponent<AddInitiatorAttackRollTrigger>().CreateCopy(a => a.Action = remove_self),
                               guidance_buff.GetComponent<AddInitiatorPartySkillRollTrigger>().CreateCopy(a => a.Action = remove_self),
                               guidance_buff.GetComponent<AddInitiatorSkillRollTrigger>().CreateCopy(a => a.Action = remove_self)
                              );

            var ability = Helpers.CreateAbility(prefix + "SuddenInsightAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.SpellLike,
                                                CommandType.Swift,
                                                AbilityRange.Personal,
                                                Helpers.oneRoundDuration,
                                                "",
                                                Helpers.CreateSpellComponent(SpellSchool.Divination),
                                                Helpers.CreateRunActions(Common.createContextActionApplySpellBuff(buff, Helpers.CreateContextDuration(1))),
                                                Common.createAbilitySpawnFx("c388856d0e8855f429a83ccba67944ba", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                resource.CreateResourceLogic()
                                                );

            ability.setMiscAbilityParametersSelfOnly();
            addFocusInvestmentCheck(ability, SpellSchool.Divination);

            return Common.AbilityToFeature(ability, false);
        }


        public BlueprintFeature createDangerSight()
        {
            var guidance_buff = library.Get<BlueprintBuff>("ec931b882e806ce42906597e5585c13f");

            var buff = Helpers.CreateBuff(prefix + "DangerSightBuff",
                                          "Danger Sight",
                                          "As a swift action, you can protect yourself from harm by expending 1 point of mental focus.  Doing so grants you an insight bonus to your AC or on your saving throw equal to 1/2 your occultist level. This bonus applies only to the next attack against you or saving throw you attempt, and if not applied within one minute, the protection fades and you gain no benefit. You must be at least 3rd level to select this focus power.",
                                          "",
                                          NewSpells.countless_eyes.Icon,
                                          null,
                                          Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.Insight),
                                          Helpers.CreateAddContextStatBonus(StatType.SaveFortitude, ModifierDescriptor.Insight),
                                          Helpers.CreateAddContextStatBonus(StatType.SaveReflex, ModifierDescriptor.Insight),
                                          Helpers.CreateAddContextStatBonus(StatType.SaveWill, ModifierDescriptor.Insight),
                                          Common.createAbilityScoreCheckBonus(Helpers.CreateContextValue(AbilityRankType.Default), ModifierDescriptor.Insight),
                                          createClassScalingConfig(progression: ContextRankProgression.Div2, min: 1)
                                          );

            var remove_self = Helpers.CreateActionList(Common.createContextActionRemoveBuff(buff));
            buff.AddComponents(guidance_buff.GetComponent<AddInitiatorSavingThrowTrigger>().CreateCopy(a => a.Action = remove_self),
                               Helpers.Create<AddTargetAttackRollTrigger>(a => { a.OnlyHit = false; a.ActionsOnAttacker = Helpers.CreateActionList(); a.ActionOnSelf = remove_self; })
                              );

            var ability = Helpers.CreateAbility(prefix + "DangerSightAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.SpellLike,
                                                CommandType.Swift,
                                                AbilityRange.Personal,
                                                Helpers.oneMinuteDuration,
                                                "",
                                                Helpers.CreateSpellComponent(SpellSchool.Divination),
                                                Helpers.CreateRunActions(Common.createContextActionApplySpellBuff(buff, Helpers.CreateContextDuration(1, DurationRate.Minutes))),
                                                Common.createAbilitySpawnFx("c388856d0e8855f429a83ccba67944ba", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                resource.CreateResourceLogic()
                                                );

            ability.setMiscAbilityParametersSelfOnly();
            addFocusInvestmentCheck(ability, SpellSchool.Divination);
            var feature =  Common.AbilityToFeature(ability, false);
            addMinLevelPrerequisite(feature, 3);
            return feature;
        }


        public BlueprintFeature createInAccordanceWithProphecy()
        {
            var buff = Helpers.CreateBuff(prefix + "InAccordanceWithProphecyBuff",
                                                 "In Accordance with the Prophecy",
                                                 "When you cast a spell, you can expend 1 point of mental focus and publicly declare that your next spell is guided by prophecy. When you do, the spell you cast has a 20% chance of fizzling (1–20 on a d%). If the spell does not fail, treat the spell as if it had been modified by the Empower Spell feat, even if you do not have that feat. At 12th level, the chance that the spell fizzles is reduced to 15% (1–15 on a d%). At 16th level, the chance is reduced to 10% (1–10 on a d%).\n"
                                                 + "You must be at least 9th level to select this focus power.",
                                                 "",
                                                 Helpers.GetIcon("ef16771cb05d1344989519e87f25b3c5"), //divine power
                                                 null,
                                                 Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicOnSpellDescriptor>(a => { a.Metamagic = Metamagic.Empower; a.resource = resource; a.amount = 1; }),
                                                 Helpers.Create<SpellFailureMechanics.SpellFailureChance>(s => s.chance = Helpers.CreateContextValue(AbilityRankType.Default)),
                                                 createClassScalingConfig( progression: ContextRankProgression.Custom,
                                                                                 customProgression: new (int, int)[] { (11, 20), (15, 15), (20, 10) })
                                                 );

            var ability = Helpers.CreateActivatableAbility(prefix + "InAccordanceWithProphecyAbility",
                                                        buff.Name,
                                                        buff.Description,
                                                        "",
                                                        buff.Icon,
                                                        buff,
                                                        AbilityActivationType.Immediately,
                                                        CommandType.Free,
                                                        null,
                                                        resource.CreateActivatableResourceLogic(ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                        );
            ability.DeactivateImmediately = true;
            addFocusInvestmentCheck(ability, SpellSchool.Divination);
            var feature = Common.ActivatableAbilityToFeature(ability, false);
            addMinLevelPrerequisite(feature, 9);

            return feature;
        }

        public BlueprintBuff createThirdEye()
        {
            var property = ImplementMechanics.InvestedImplementFocusAmountProperty.createProperty(prefix + "ThirdEyeProperty", "",
                                                                                                  Helpers.CreateContextValue(AbilityRankType.StatBonus),
                                                                                                  SpellSchool.Divination);

            var property_bonus = ImplementMechanics.InvestedImplementFocusAmountProperty.createProperty(prefix + "ThirdEyeBonusProperty", "",
                                                                                      Helpers.CreateContextValue(AbilityRankType.DamageBonus),
                                                                                      SpellSchool.Divination);

            var buff7 =  library.CopyAndAdd<BlueprintBuff>("0adecbf63b614e846bfe15c33f34507e", prefix + "Thirdeye7Buff", "");
            buff7.SetBuffFlags(BuffFlags.StayOnDeath | BuffFlags.HiddenInUi);
            var buff13 = library.CopyAndAdd<BlueprintBuff>(buff7, prefix + "Thirdeye13Buff", "");
            buff13.ComponentsArray = new BlueprintComponent[] { Common.createBlindsense(60) };
            var buff19 = library.CopyAndAdd<BlueprintBuff>(buff7, prefix + "Thirdeye19Buff", "");
            buff19.ComponentsArray = new BlueprintComponent[] { Common.createBlindsight(30),
                                                                Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.GazeAttack),
                                                                Common.createBuffDescriptorImmunity(SpellDescriptor.GazeAttack),
                                                              };

            var buff = Helpers.CreateBuff(prefix + "ThirdEyeBuff",
                                          "Third Eye",
                                          "The implement allows its bearer to notice that which can’t easily be seen. The implement grants a +1 insight bonus on Perception checks per 2 points of mental focus stored in it, to a maximum bonus equal to the occultist’s level. If the occultist is 7th level or higher and stores at least 9 points of mental focus in it, the implement also grants the effects of see invisibility. If the occultist is 13th level or higher and stores at least 12 points of mental focus in it, the implement also grants blindsense 60 feet. If the occultist is 19th level or higher and stores at least 15 points of mental focus in it, the implement also grants blindsight 30 feet.",
                                          "",
                                          Helpers.GetIcon("b3da3fbee6a751d4197e446c7e852bcb"), //true seeing
                                          null,
                                          Helpers.CreateAddContextStatBonus(StatType.SkillPerception, ModifierDescriptor.Insight),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, ContextRankProgression.DivStep, stepLevel: 2,
                                                                          customProperty: property),
                                          createClassScalingConfig(ContextRankProgression.MultiplyByModifier, type: AbilityRankType.StatBonus, stepLevel: 2),//2 * lvl
                                          Helpers.CreateAddFactContextActions(activated: Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.DamageDice),
                                                                                                                                        Common.createContextActionApplyChildBuff(buff7),
                                                                                                                                        Common.createContextActionApplyChildBuff(buff13),
                                                                                                                                        Common.createContextActionApplyChildBuff(buff19)
                                                                                                                                        )
                                                                             ),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, ContextRankProgression.DelayedStartPlusDivStep, startLevel: 9, stepLevel: 3,
                                                                          customProperty: property_bonus, type: AbilityRankType.DamageDice),
                                          createClassScalingConfig(ContextRankProgression.Custom, type: AbilityRankType.DamageBonus,
                                                                   customProgression: new (int, int)[] {(6, 0), (12, 9), (18, 12)  })
                                          );
            return buff;
        }
    }
}

