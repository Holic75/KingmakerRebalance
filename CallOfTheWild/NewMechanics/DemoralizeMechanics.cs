using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.DemoralizeMechanics
{
    class UnitPartDemoralizeBonus : AdditiveUnitPart
    {
        public int getBonus()
        {
            int bonus = 0;
            foreach (var b in buffs)
            {
                b.CallComponents<DemoralizeBonus>(d => bonus += d.getValue());
            }
            return bonus;
        }
    }


    public class DemoralizeBonus : OwnedGameLogicComponent<UnitDescriptor>
    {
        public ContextValue value;
        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartDemoralizeBonus>().addBuff(this.Fact);

        }

        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartDemoralizeBonus>().removeBuff(this.Fact);
        }

        public int getValue()
        {
            return value.Calculate(this.Fact.MaybeContext);
        }
    }

    class UnitPartActionOnDemoralize: AdditiveUnitPart
    {
        public void runActions(MechanicsContext context, UnitEntityData target, int dc, int check_result)
        {
            foreach (var b in buffs)
            {
                b.CallComponents<ActionOnDemoralize>(c => c.runActions(context, target, dc, check_result));
            }
        }
    }


    abstract class ActionOnDemoralize: OwnedGameLogicComponent<UnitDescriptor>
    {
        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartActionOnDemoralize>().addBuff(this.Fact);

        }

        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartActionOnDemoralize>().removeBuff(this.Fact);
        }

        abstract public void runActions(MechanicsContext context, UnitEntityData target, int dc, int check_result);
    }


    class RunActionsOnDemoralize : ActionOnDemoralize
    {
        public ActionList actions;
        
        public override void runActions(MechanicsContext context, UnitEntityData target, int dc, int check_result)
        {
            actions.Run();
        }
    }


    class ScopedDemoralizeActions : BlueprintComponent
    {
        public ActionList actions;
    }


    class IntimidateSkillUnlock : ActionOnDemoralize
    {
        public int min_value = 5;
        public int upgrade_value = 15;
        public int dc_bypass = 10;
        public BlueprintBuff frightened_buff = Main.library.Get<BlueprintBuff>("f08a7239aa961f34c8301518e71d4cdf");


        public override void runActions(MechanicsContext context, UnitEntityData target, int dc, int check_result)
        {
            if (check_result <= dc +dc_bypass)
            {
                return;
            }
            var intimidate_value = context.MaybeCaster.Stats.GetStat<ModifiableValueSkill>(StatType.SkillPersuasion).BaseValue;
            if (intimidate_value < min_value)
            {
                return;
            }

            var spell_descriptors_to_add = new SpellDescriptor[]
            {
                SpellDescriptor.MindAffecting,
                SpellDescriptor.Fear
            };

            for (int i = 0; i < spell_descriptors_to_add.Length; i++)
            {
                if ((context.SpellDescriptor & spell_descriptors_to_add[i]) > 0)
                {
                    spell_descriptors_to_add[i] = SpellDescriptor.None;
                }
                else
                {
                    context.AddSpellDescriptor(spell_descriptors_to_add[i]);
                }
            }

            RuleSavingThrow saving_throw = new RuleSavingThrow(target, SavingThrowType.Will, 10 + intimidate_value);
            context.TriggerRule(saving_throw);

            foreach (var sd in spell_descriptors_to_add)
            {
                if (sd != SpellDescriptor.None)
                {
                    context.RemoveSpellDescriptor(sd);
                }
            }
            if (saving_throw.IsPassed)
            {
                return;
            }         

            int duration = intimidate_value < upgrade_value ? 1 : RulebookEvent.Dice.D(new DiceFormula(1, DiceType.D4));
            target.Descriptor.AddBuff(frightened_buff, context, new TimeSpan?(duration.Rounds().Seconds));
        }
    }


    [Harmony12.HarmonyPatch(typeof(Demoralize))]
    [Harmony12.HarmonyPatch("RunAction", Harmony12.MethodType.Normal)]
    class Demoralize_RunAction_Patch
    {
        static bool Prefix(Demoralize __instance)
        {
            var tr = Harmony12.Traverse.Create(__instance);
            var target = tr.Property("Target").GetValue<TargetWrapper>();
            MechanicsContext.Data data = ElementsContext.GetData<MechanicsContext.Data>();
            MechanicsContext mechanicsContext = (data != null) ? data.Context : null;
            UnitEntityData unitEntityData = (mechanicsContext != null) ? mechanicsContext.MaybeCaster : null;
            if (unitEntityData == null || !target.IsUnit)
            {
                UberDebug.LogError(__instance, "Unable to apply buff: no context found", Array.Empty<object>());
                return false;
            }
            int num = 10 + target.Unit.Descriptor.Progression.CharacterLevel + target.Unit.Stats.Wisdom.Bonus;
            ModifiableValue.Modifier modifier = null;
            try
            {
                if (__instance.DazzlingDisplay && unitEntityData.Descriptor.State.Features.SwordlordWeaponProwess)
                {
                    int num2 = 0;
                    foreach (var feature in unitEntityData.Descriptor.Progression.Features)
                    {
                        var param = feature.Param;
                        bool flag;
                        if (param == null)
                        {
                            flag = false;
                        }
                        else
                        {
                            WeaponCategory? weaponCategory = param.WeaponCategory;
                            WeaponCategory weaponCategory2 = WeaponCategory.DuelingSword;
                            flag = (weaponCategory.GetValueOrDefault() == weaponCategory2 & weaponCategory != null);
                        }
                        if (flag)
                        {
                            num2++;
                        }
                    }
                    modifier = unitEntityData.Stats.CheckIntimidate.AddModifier(num2, null, ModifierDescriptor.None);
                }
                var ruleSkillCheck = new RuleSkillCheck(unitEntityData, StatType.CheckIntimidate, num);
                var demoralize_bonus_part = mechanicsContext.MaybeCaster.Get<UnitPartDemoralizeBonus>();
                if (demoralize_bonus_part != null)
                {
                    ruleSkillCheck.Bonus.AddModifier(demoralize_bonus_part.getBonus(), null, ModifierDescriptor.UntypedStackable);
                }
               
                ruleSkillCheck = mechanicsContext.TriggerRule<RuleSkillCheck>(ruleSkillCheck);
                if (ruleSkillCheck.IsPassed)
                {
                    int num3 = 1 + (ruleSkillCheck.RollResult - num) / 5 + (unitEntityData.Descriptor.State.Features.FrighteningThug ? 1 : 0);
                    if (unitEntityData.Descriptor.State.Features.FrighteningThug && num3 >= 4)
                    {
                        target.Unit.Descriptor.AddBuff(__instance.GreaterBuff, mechanicsContext, new TimeSpan?(1.Rounds().Seconds));
                    }
                    Buff buff = target.Unit.Descriptor.AddBuff(__instance.Buff, mechanicsContext, new TimeSpan?(num3.Rounds().Seconds));
                    if (unitEntityData.Descriptor.HasFact(__instance.ShatterConfidenceFeature) && buff != null)
                    {
                        Buff fact = target.Unit.Descriptor.AddBuff(__instance.ShatterConfidenceBuff, mechanicsContext, new TimeSpan?(num3.Rounds().Seconds));
                        buff.StoreFact(fact);
                    }

                    unitEntityData.Get<UnitPartActionOnDemoralize>()?.runActions(mechanicsContext, target.Unit, num, ruleSkillCheck.RollResult);
                    var scoped_actions = (mechanicsContext?.SourceAbility?.GetComponents<ScopedDemoralizeActions>()).EmptyIfNull();
                    foreach (var s in scoped_actions)
                    {
                        s.actions.Run();
                    }
                }
            }
            finally
            {
                if (modifier != null)
                {
                    modifier.Remove();
                }
            }
            return false;
        }
    }
}
