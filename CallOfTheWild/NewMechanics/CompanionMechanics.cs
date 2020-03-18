using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Newtonsoft.Json;
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

            if (evt.Target.HPLeft <= 0)
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
            if (transfer_damage <= 0)
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



    [AllowedOn(typeof(BlueprintUnitFact))]
    public class GrabFeaturesFromCompanion : OwnedGameLogicComponent<UnitDescriptor>
    {
        public BlueprintFeature[] Features;
        [JsonProperty]
        private List<Fact> m_AppliedFacts = new List<Fact>();


        public override void OnTurnOn()
        {
            this.m_AppliedFacts.Clear();
            base.OnTurnOn();
            UnitEntityData pet = this.Owner.Pet;
            if (pet == null)
                return;

            foreach (BlueprintFeature feature in this.Features)
            {
                if (pet.Descriptor.Progression.Features.HasFact((BlueprintFact)feature) && !this.Owner.HasFact((BlueprintUnitFact)feature))
                {
                    var added_fact = this.Owner.Progression.Features.AddFact((BlueprintFact)feature, null);
                    if (added_fact != null)
                    {
                        m_AppliedFacts.Add(added_fact);
                    }
                }
            }
        }

        public override void OnTurnOff()
        {
            base.OnTurnOff();
            this.m_AppliedFacts.ForEach(new Action<Fact>((this.Owner).RemoveFact));
            this.m_AppliedFacts.Clear();
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class SetPhysicalStatsToAnimalCompanionStats : OwnedGameLogicComponent<UnitDescriptor>
    {
        [JsonProperty]
        private readonly List<ModifiableValue.Modifier> m_AppliedModifiers = new List<ModifiableValue.Modifier>();

        public override void OnFactActivate()
        {
            UnitEntityData pet = this.Owner.Pet;
            if (pet == null)
                return;

            var stats = new StatType[] { StatType.Strength, StatType.Dexterity, StatType.Constitution };
            m_AppliedModifiers.Clear();
            foreach (var s in stats)
            {
                int bonus = pet.Stats.GetStat(s).BaseValue + pet.Stats.GetStat(s).GetDescriptorBonus(ModifierDescriptor.Racial);
                bonus -= this.Owner.Stats.GetStat(s).BaseValue + this.Owner.Stats.GetStat(s).GetDescriptorBonus(ModifierDescriptor.Racial);
                this.m_AppliedModifiers.Add(this.Owner.Stats.Strength.AddModifier(bonus, (GameLogicComponent)this, ModifierDescriptor.Other));
            }
        }


        public override void OnFactDeactivate()
        {
            this.m_AppliedModifiers.ForEach((Action<ModifiableValue.Modifier>)(m => m.Remove()));
            m_AppliedModifiers.Clear();
        }
    }

}
