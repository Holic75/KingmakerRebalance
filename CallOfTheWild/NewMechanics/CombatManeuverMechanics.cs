using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.ContextData;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.CombatManeuverMechanics
{
    public class UnitPartFakeSizeBonus : AdditiveUnitPart
    {
        public Size getEffectiveSize()
        {
            var unit_size = this.Owner.State.Size;
            if (buffs.Empty())
            {
                return unit_size;
            }

            int bonus = buffs[0].Blueprint.GetComponent<FakeSizeBonus>().bonus;
            for  (int i = 1; i < buffs.Count; i++)
            {
                var c = buffs[i].Blueprint.GetComponent<FakeSizeBonus>();

                if (c.bonus > bonus)
                {
                    bonus = c.bonus;
                }
            }

            return unit_size.Shift(bonus);
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class FakeSizeBonus : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber, IInitiatorRulebookHandler<RuleCalculateBaseCMB>, IInitiatorRulebookHandler<RuleCalculateBaseCMD>
    {
        public int bonus;

        public void OnEventAboutToTrigger(RuleCalculateBaseCMB evt)
        {
            var old_size = this.Owner.State.Size;
            var new_size = old_size.Shift(bonus);
            if (old_size == new_size)
            {
                return;
            }
            
            evt.AddBonus(new_size.GetModifiers().CMDAndCMD - old_size.GetModifiers().CMDAndCMD + new_size.GetModifiers().AttackAndAC - old_size.GetModifiers().AttackAndAC, this.Fact);
        }

        public void OnEventAboutToTrigger(RuleCalculateBaseCMD evt)
        {
            var old_size = this.Owner.State.Size;
            var new_size = old_size.Shift(bonus);
            if (old_size == new_size)
            {
                return;
            }

            evt.AddBonus(new_size.GetModifiers().CMDAndCMD - old_size.GetModifiers().CMDAndCMD + new_size.GetModifiers().AttackAndAC - old_size.GetModifiers().AttackAndAC, this.Fact);
        }

        public void OnEventDidTrigger(RuleCalculateBaseCMB evt)
        {
        }

        public void OnEventDidTrigger(RuleCalculateBaseCMD evt)
        {
        }

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartFakeSizeBonus>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartFakeSizeBonus>().removeBuff(this.Fact);
        }
    }


    public class ContextConditionCasterSizeGreater : ContextCondition
    {
        public int size_delta;

        protected override string GetConditionCaption()
        {
            return string.Empty;
        }

        protected override bool CheckCondition()
        {
            if (Target == null)
            {
                return false;
            }

            return (int)this.Context.MaybeCaster.Ensure<UnitPartFakeSizeBonus>().getEffectiveSize() - (int)this.Target.Unit.Ensure<UnitPartFakeSizeBonus>().getEffectiveSize() >= size_delta;
        }
    }


    public class ContextConditionTargetSizeLessOrEqual : ContextCondition
    {
        public Size target_size;

        protected override string GetConditionCaption()
        {
            return string.Empty;
        }

        protected override bool CheckCondition()
        {
            if (Target == null)
            {
                return false;
            }

            return this.Target.Unit.Ensure<UnitPartFakeSizeBonus>().getEffectiveSize() <= target_size;
        }
    }


    public class ContextConditionCasterBuffRankLess : ContextCondition
    {
        public BlueprintBuff buff;
        public int rank;

        protected override string GetConditionCaption()
        {
            return string.Empty;
        }

        protected override bool CheckCondition()
        {
            var caster = this.Context.MaybeCaster;
            if (caster == null)
            {
                return false;
            }
            return (caster.Buffs.Enumerable.Where(b => b.Blueprint == buff).Count() < rank);
        }
    }


    public class ContextActionSpitOut : ContextAction
    {

        public override string GetCaption()
        {
            return "Spit out";
        }

        public override void RunAction()
        {
            var caster = this.Context.MaybeCaster;

            if (caster == null)
            {
                return;
            };
            caster.Ensure<UnitPartSwallowWhole>().SpitOut(true);
        }
    }


    public class ContextActionForceMove : ContextAction
    {
        public bool provoke_aoo = false;
        public ContextDiceValue distance_dice;

        public override string GetCaption()
        {
            return "Force Move";
        }

        public override void RunAction()
        {
            var caster = this.Context.MaybeCaster;

            var unit = this.Target.Unit;

            if (unit == null || caster == null)
            {
                return;
            }
            var distance = distance_dice.Calculate(this.Context);
            unit.Ensure<UnitPartForceMove>().Push((unit.Position - caster.Position).normalized, distance.Feet().Meters, provoke_aoo);
        }
    }



    [AllowedOn(typeof(BlueprintUnitFact))]
    public class CombatManeuverBonus : RuleInitiatorLogicComponent<RuleCalculateCMB>
    {
        public ContextValue Value;

        private MechanicsContext Context
        {
            get
            {
                MechanicsContext context = (this.Fact as Buff)?.Context;
                if (context != null)
                    return context;
                return (this.Fact as Feature)?.Context;
            }
        }

        public override void OnEventAboutToTrigger(RuleCalculateCMB evt)
        {
            evt.AddBonus(this.Value.Calculate(this.Context), this.Fact);
        }

        public override void OnEventDidTrigger(RuleCalculateCMB evt)
        {
        }
    }



    [ComponentName("Maneuver Defence Bonus")]
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class ContextManeuverDefenceBonus : RuleTargetLogicComponent<RuleCalculateCMB>
    {
        public ContextValue Bonus;

        public override void OnEventAboutToTrigger(RuleCalculateCMB evt)
        {
            evt.AddBonus(this.Bonus.Calculate(this.Fact.MaybeContext), this.Fact);
        }

        public override void OnEventDidTrigger(RuleCalculateCMB evt)
        {
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class SpecificCombatManeuverBonusUnlessHasFacts : RuleInitiatorLogicComponent<RuleCalculateCMB>
    {
        public ContextValue Value;
        public CombatManeuver maneuver_type;
        public static List<BlueprintUnitFact> facts = new List<BlueprintUnitFact>();

        private MechanicsContext Context
        {
            get
            {
                MechanicsContext context = (this.Fact as Buff)?.Context;
                if (context != null)
                    return context;
                return (this.Fact as Feature)?.Context;
            }
        }

        public override void OnEventAboutToTrigger(RuleCalculateCMB evt)
        {
            if (evt.Type == maneuver_type)
            {
                foreach (var f in facts)
                {
                    if (evt.Initiator.Descriptor.HasFact(f))
                    {
                        return;
                    }
                }
                evt.AddBonus(this.Value.Calculate(this.Context), this.Fact);
            }
        }

        public override void OnEventDidTrigger(RuleCalculateCMB evt)
        {
        }
    }


    public class ApplyBuffOnHit : GameLogicComponent, ITargetRulebookHandler<RuleAttackWithWeapon>, IRulebookHandler<RuleAttackWithWeapon>, IInitiatorRulebookSubscriber
    {
        public bool apply_to_ranged = false;
        public bool apply_to_natural = false;

        public ActionList main_hand_action = null;
        public ActionList off_hand_action = null;
        public ActionList other_action = null;


        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
        {
            
        }

        public void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {

            if (evt.Weapon.Blueprint.IsRanged && !apply_to_ranged)
            {
                 return;
            }

            if ((evt.Weapon.Blueprint.IsNatural || evt.Weapon.Blueprint.IsUnarmed) && !apply_to_natural)
            {
                return;
            }

            if (evt.Weapon.HoldingSlot == evt.Initiator.Body.PrimaryHand)
            {
                if (main_hand_action != null)
                {
                    using (new ContextAttackData(evt.AttackRoll, (Projectile)null))
                        (this.Fact as IFactContextOwner)?.RunActionInContext(this.main_hand_action, (TargetWrapper)evt.Initiator);
                }
            }
            else if (evt.Weapon.HoldingSlot == evt.Initiator.Body.SecondaryHand)
            {
                if (off_hand_action != null)
                {
                    using (new ContextAttackData(evt.AttackRoll, (Projectile)null))
                        (this.Fact as IFactContextOwner)?.RunActionInContext(this.off_hand_action, (TargetWrapper)evt.Initiator);
                }
            }
            else
            {
                if (other_action != null)
                {
                    using (new ContextAttackData(evt.AttackRoll, (Projectile)null))
                        (this.Fact as IFactContextOwner)?.RunActionInContext(this.other_action, (TargetWrapper)evt.Initiator);
                }
            }
        }
    }


    public class ContextActionBreakFreeFromSpellGrapple : ContextAction
    {
        public ActionList Success;
        public ActionList Failure;

        public override void RunAction()
        {
            if (this.Target.Unit == null)
            {
                UberDebug.LogError((object)"Target unit is missing", (object[])Array.Empty<object>());
            }
            else
            {

                if (tryBreakFree(this.Target.Unit, this.Context.MaybeCaster, this.Context))
                    this.Success.Run();
                else
                    this.Failure.Run();
            }
        }

        private static bool tryBreakFree(UnitEntityData target, UnitEntityData grappler, MechanicsContext context)
        {
            int dc = 0;
            if (context != null)
            {
                dc = context.Params.DC;
            }
            RuleCalculateCMB rule_calcualte_cmb1 = new RuleCalculateCMB(target, grappler, CombatManeuver.Grapple);
            RuleCalculateCMB rule_calcualte_cmb2 = context?.TriggerRule<RuleCalculateCMB>(rule_calcualte_cmb1) ?? Rulebook.Trigger<RuleCalculateCMB>(rule_calcualte_cmb1);
            if (Math.Max((int)(target.Stats.SkillMobility), (int)(target.Stats.SkillAthletics)) < rule_calcualte_cmb2.Result)
            {             
                RuleSkillCheck rule_skill_check = new RuleSkillCheck(target, StatType.AdditionalCMB, dc);
                rule_skill_check.Bonus.AddModifier(rule_calcualte_cmb2.Result - target.Stats.AdditionalCMB.ModifiedValue, null, ModifierDescriptor.UntypedStackable);
                return (context?.TriggerRule<RuleSkillCheck>(rule_skill_check) ?? Rulebook.Trigger<RuleSkillCheck>(rule_skill_check)).IsPassed;
            }
            else
            {
                StatType stat = (int)(target.Stats.SkillMobility) > (int)(target.Stats.SkillAthletics) ? StatType.SkillMobility : StatType.SkillAthletics;
                RuleSkillCheck rule_skill_check = new RuleSkillCheck(target, stat, dc);
                return (context?.TriggerRule<RuleSkillCheck>(rule_skill_check) ?? Rulebook.Trigger<RuleSkillCheck>(rule_skill_check)).IsPassed;
            }
        }

        public override string GetCaption()
        {
            return "Check to break free from entangle or grapple";
        }
    }
}
