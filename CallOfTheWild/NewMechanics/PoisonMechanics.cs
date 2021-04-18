using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.PoisonMechanics
{
    [AllowedOn(typeof(BlueprintBuff))]
    [AllowMultipleComponents]
    public class BuffPoisonDamage : BuffLogic, ITickEachRound
    {
        public StatType Stat = StatType.Unknown;//will deal hp damage by default
        public ContextValue Ticks;
        public ContextValue SuccesfullSaves;
        public SavingThrowType SaveType;
        public  ContextDiceValue ContextValue;
        public ActionList on_successful_save_action = Helpers.CreateActionList();
        [JsonProperty]
        private int m_TicksPassed;
        [JsonProperty]
        private int m_SavesSucceeded;
        [JsonProperty]
        private int m_BonusDC;
        [JsonProperty]
        private int m_Ticks;
        private int m_SuccesfullSaves;


        public void OnNewRound()
        {
            if (this.m_Ticks > this.m_TicksPassed && this.m_SuccesfullSaves > this.m_SavesSucceeded)
            {
                if (this.SaveType != SavingThrowType.Unknown)
                {
                    RuleSavingThrow ruleSavingThrow = new RuleSavingThrow(this.Owner.Unit, this.SaveType, this.Context.Params.DC + this.m_BonusDC);
                    ruleSavingThrow.Reason = (RuleReason)this.Fact;
                    RuleSavingThrow evt = ruleSavingThrow;
                    Game.Instance.Rulebook.TriggerEvent<RuleSavingThrow>(evt);
                    if (!evt.IsPassed)
                    {
                        triggerEffect();
                    }
                    if (evt.IsPassed)
                    {
                        (this.Buff as IFactContextOwner).RunActionInContext(on_successful_save_action, this.Owner.Unit);
                        ++this.m_SavesSucceeded;
                        if (this.m_SavesSucceeded >= this.m_SuccesfullSaves)
                        {
                            this.Buff.Remove();
                        }
                    }
                }
                ++this.m_TicksPassed;
            }
            else
                this.Buff.Remove();
        }


        private void triggerEffect()
        {
            int bonus = this.ContextValue.Calculate(this.Context);
            if (Stat != StatType.Unknown)
            {
                ModifiableValue stat = this.Owner.Stats.GetStat(this.Stat);
                RuleDealStatDamage rule = new RuleDealStatDamage(this.Owner.Unit, this.Owner.Unit, this.Stat, DiceFormula.Zero, bonus);
                rule.Reason = (RuleReason)this.Fact;
                this.Context.TriggerRule<RuleDealStatDamage>(rule);
            }
            else
            {
                var hp_damage = new DamageBundle(new BaseDamage[1]
                                                {
                                                    (BaseDamage) new DirectDamage(new DiceFormula(0, DiceType.Zero), bonus)
                                                }
                                                );
                RuleDealDamage rule = new RuleDealDamage(this.Owner.Unit, this.Owner.Unit, hp_damage);
                rule.Reason = (RuleReason)this.Fact;
                this.Context.TriggerRule<RuleDealDamage>(rule);
            }
        }

        public override void OnFactActivate()
        {
            m_Ticks = Ticks.Calculate(this.Fact.MaybeContext);
            m_SuccesfullSaves = SuccesfullSaves.Calculate(this.Fact.MaybeContext);
            triggerEffect();
            ++this.m_TicksPassed;
        }

        public void GetStacked()
        {
            this.m_TicksPassed -= this.m_Ticks / 2;
            this.m_SavesSucceeded = Math.Max(0, this.m_SavesSucceeded - 1);
            this.m_BonusDC += 2;
        }
    }
}
