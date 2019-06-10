using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.Visual.HitSystem;
using System;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers;
using Kingmaker.UnitLogic.Mechanics;

namespace KingmakerRebalance
{
    [ComponentName("Increase spell descriptor DC for 1d6 damage")]
    [AllowedOn(typeof(Kingmaker.Blueprints.Facts.BlueprintUnitFact))]
    public class MetaRage : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
    {
        public Metamagic metamagic;
        public BlueprintAbilityResource resource;

        public MetaRage(Metamagic metamagic_to_apply, BlueprintAbilityResource resource_to_use)
        {
            metamagic = metamagic_to_apply;
            resource = resource_to_use;
        }
        public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            bool is_metamagic_available = ((evt.Spell.AvailableMetamagic & metamagic) != 0);
            if (evt.Spell == null || evt.Spellbook == null || evt.Spell.Type != AbilityType.Spell || !is_metamagic_available)
            {
                return;
            }

            int resource_needed = 2 * (evt.Spellbook.GetSpellLevel(evt.Spell) + MetamagicHelper.DefaultCost(metamagic));
            Main.logger.Log(resource_needed.ToString());
            if (this.resource == null || this.Owner.Resources.GetResourceAmount((BlueprintScriptableObject)this.resource) < resource_needed)
            {
                return;
            }

            evt.AddMetamagic(this.metamagic);
            this.Owner.Resources.Spend((BlueprintScriptableObject)this.resource, resource_needed);
        }

        public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }
    }


    [ComponentName("Increase spell descriptor DC for 1d6 damage")]
    [AllowedOn(typeof(Kingmaker.Blueprints.Facts.BlueprintUnitFact))]
    public class RageCasting : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
    {
        public int BonusDC;
        private int actual_dc;

        public RageCasting(int bonus_dc)
        {
            BonusDC = bonus_dc;
            actual_dc = 0;
        }
        public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            actual_dc = 0;
            bool no_save = evt.Spell.EffectOnEnemy != AbilityEffectOnUnit.Harmful; //TODO: properly check for saving throw
            if (evt.Spell == null || evt.Spellbook == null || evt.Spell.Type != AbilityType.Spell || no_save)
            {
                return;
            }
            actual_dc = Mathf.Min(evt.Spellbook.GetSpellLevel(evt.Spell), BonusDC);
            evt.AddBonusDC(actual_dc);
        }

        public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
            if (actual_dc == 0)
            {
                return;
            }
            RuleDealDamage evt_dmg = new RuleDealDamage(this.Owner.Unit, this.Owner.Unit, new DamageBundle(new BaseDamage[1]
            {
                (BaseDamage) new EnergyDamage(new DiceFormula(actual_dc, DiceType.D6), Kingmaker.Enums.Damage.DamageEnergyType.Holy)
            }));
            evt_dmg.Reason = (RuleReason)this.Fact;
            //temporary remove temp hp
            var temp_hd_modifiers = this.Owner.Stats.TemporaryHitPoints.Modifiers.ToArray();
            foreach (var m in temp_hd_modifiers)
            {
                this.Owner.Stats.TemporaryHitPoints.RemoveModifier(m);
            }
            this.Owner.Stats.TemporaryHitPoints.UpdateValue();
            Rulebook.Trigger<RuleDealDamage>(evt_dmg);
            if (this.Owner.HPLeft <= 0)
            { //do not give hp back if owner is unconscious
                return;
            }
            foreach (var m in temp_hd_modifiers)
            {
                this.Owner.Stats.TemporaryHitPoints.AddModifier(m.ModValue, m.Source, m.SourceComponent, m.ModDescriptor);
            }
            this.Owner.Stats.TemporaryHitPoints.UpdateValue();
        }
    }


    [AllowedOn(typeof(BlueprintBuff))]
    [ComponentName("Buffs/AddEffect/ContextFastHealing")]
    public class AddContextEffectFastHealing : BuffLogic, ITickEachRound, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ITargetRulebookSubscriber
    {
        public int Multiplier = 1;
        public ContextValue Value;

        public void OnNewRound()
        {
            int heal_amount = this.Value.Calculate(this.Context);
            if (this.Owner.State.IsDead || this.Owner.Damage <= 0)
            {
                return;
            }
            GameHelper.HealDamage(this.Owner.Unit, this.Owner.Unit, heal_amount * Multiplier);
        }

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
        }
    }
}
