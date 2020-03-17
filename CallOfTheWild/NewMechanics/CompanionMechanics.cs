using Kingmaker.Blueprints;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.CompanionMechanics
{
    public class TransferDamageAfterThresholdToPet : RuleTargetLogicComponent<RuleDealDamage>
    {
        public int threshold = 1;
        public override void OnEventAboutToTrigger(RuleDealDamage evt)
        {
        }

        public override void OnEventDidTrigger(RuleDealDamage evt)
        {
            if (evt.Target.Damage < 0)
            {
                return;
            }
            var pet = evt?.Target?.Descriptor?.Pet;
            if (pet == null)
            {
                return;
            }

            if (evt.Target.HPLeft <=0)
            {
                return;
            }

            if (pet.Descriptor.State.IsDead)
            {
                return;
            }

            if (evt.Target.HPLeft + evt.Target.Damage > 0)
            {
                return;
            }

            int max_can_transfer = pet.HPLeft + pet.Stats.Constitution - 1;
            int max_need_transfer = (evt.Target.Damage - evt.Target.HPLeft) + threshold;

            int transfer_damage = Math.Min(max_can_transfer, max_need_transfer);
            transfer_damage = Math.Min(transfer_damage, evt.Target.Damage);
            if (transfer_damage <=0 )
            {
                return;
            }
            evt.Target.Damage -= transfer_damage;
            var damage_bundle = new DamageBundle(new DirectDamage(new DiceFormula(transfer_damage, DiceType.One), 0));
            var rule = this.Fact.MaybeContext.TriggerRule(new RuleDealDamage(evt.Target, pet, damage_bundle));

        }
    }


    public class TransferDamageToMaster : RuleTargetLogicComponent<RuleDealDamage>
    {
        public override void OnEventAboutToTrigger(RuleDealDamage evt)
        {
        }

        public override void OnEventDidTrigger(RuleDealDamage evt)
        {
            var master = evt?.Target?.Descriptor?.Master.Value;
            if (master == null)
            {
                return;
            }


            if (master.Descriptor.State.IsDead)
            {
                return;
            }

            if (evt.Target.HPLeft + evt.Target.Damage > 0)
            {
                return;
            }

            int max_can_transfer = master.HPLeft - 1;
            int max_need_transfer = (evt.Target.Damage - evt.Target.HPLeft) - evt.Target.Stats.Constitution + 1;

            int transfer_damage = Math.Min(max_can_transfer, max_need_transfer);
            transfer_damage = Math.Min(transfer_damage, evt.Target.Damage);
            if (transfer_damage <= 0)
            {
                return;
            }

            evt.Target.Damage -= transfer_damage;
            var damage_bundle = new DamageBundle(new DirectDamage(new DiceFormula(transfer_damage, DiceType.One), 0));
            var rule = this.Fact.MaybeContext.TriggerRule(new RuleDealDamage(evt.Target, master, damage_bundle));
        }
    }
}
