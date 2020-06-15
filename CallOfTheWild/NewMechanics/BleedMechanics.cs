using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Units;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.BleedMechanics
{
    public class UnitPartBleed : UnitPart
    {
        [JsonProperty]
        public int bleed_bonus = 0;

        [JsonProperty]
        private Fact bleed_buff;

        public bool addBuff(Fact fact)
        {
            int new_bleed_amount = 0;
            fact?.CallComponents<BleedBuff>(c => new_bleed_amount = c.getAverageBleedAmount());

            int old_bleed_amount = 0;

            bleed_buff?.CallComponents<BleedBuff>(c => old_bleed_amount = c.getAverageBleedAmount());


            if (old_bleed_amount < new_bleed_amount)
            {
                //(bleed_buff as Buff)?.Remove();
                bleed_buff = fact;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool isCurrentBleedBuff(Fact fact)
        {
            return bleed_buff == fact;
        }

        public void removeBuff(Fact fact)
        {
            if (bleed_buff == fact)
            {
                bleed_buff = null;
            }
        }
    }


    public class IncreaseBleed : OwnedGameLogicComponent<UnitDescriptor>
    {
        public int amount;

        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartBleed>().bleed_bonus++;

        }

        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartBleed>().bleed_bonus--;
        }
    }


    public class BleedBuff: BuffLogic, ITickEachRound
    {
        public ContextDiceValue dice_value;

        public int getBleedAmount()
        {
            return dice_value.Calculate(this.Fact.MaybeContext);
        }

        public int getAverageBleedAmount()
        {
            var dice_fomula = new DiceFormula(dice_value.DiceCountValue.Calculate(this.Fact.MaybeContext), dice_value.DiceType);
            var bonus = dice_value.BonusValue.Calculate(this.Fact.MaybeContext);
            int bleed = dice_fomula.MinValue(0) + dice_fomula.MaxValue(0) + bonus*2;
            return bleed;
        }

        public override void OnFactActivate()
        {         
            if (!this.Owner.Ensure<UnitPartBleed>().addBuff(this.Fact))
            {
               // this.Buff.Remove();
            }
        }

        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartBleed>().removeBuff(this.Fact);
        }

        public void OnNewRound()
        {
            if (!this.Owner.Ensure<UnitPartBleed>().isCurrentBleedBuff(this.Fact))
            {
                return;
            }

            var dices = new DiceFormula(this.dice_value.DiceCountValue.Calculate(this.Context), this.dice_value.DiceType);
            var bonus = this.dice_value.BonusValue.Calculate(this.Context) + this.Owner.Ensure<UnitPartBleed>().bleed_bonus;

            var base_dmg = (BaseDamage)new DirectDamage(dices, bonus);
            base_dmg.IgnoreReduction = true;

            RuleDealDamage evt_dmg = new RuleDealDamage(this.Context.MaybeCaster, this.Owner.Unit, new DamageBundle(new BaseDamage[] { base_dmg }));
            evt_dmg.SourceAbility = Context?.SourceAbility;
            evt_dmg.Reason = this.Fact;
            Rulebook.Trigger<RuleDealDamage>(evt_dmg);
        }
    }




}
