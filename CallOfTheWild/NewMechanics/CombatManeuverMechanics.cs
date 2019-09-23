using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
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
    public class UnitPartFakeSizeBonus : UnitPart
    {
        [JsonProperty]
        private List<Fact> buffs = new List<Fact>();

        public void addBuff(Fact buff)
        {
            buffs.Add(buff);
        }


        public void removeBuff(Fact buff)
        {
            buffs.Remove(buff);
        }


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

        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartFakeSizeBonus>().addBuff(this.Fact);
        }


        public override void OnFactDeactivate()
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
}
