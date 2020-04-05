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
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Newtonsoft.Json;
using Kingmaker.Utility;
using Kingmaker.UI.GenericSlot;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.EntitySystem.Entities;
using System.Collections.Generic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.Blueprints.Validation;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.ElementsSystem;
using Kingmaker.Controllers;
using Kingmaker;
using static Kingmaker.UnitLogic.Abilities.Components.AbilityCustomMeleeAttack;
using Kingmaker.UnitLogic.Mechanics.ContextData;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.EntitySystem;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.EntitySystem.Persistence.Versioning;
using JetBrains.Annotations;
using Kingmaker.Enums.Damage;
using Kingmaker.Inspect;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Blueprints.Area;
using Kingmaker.Items.Slots;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.AreaLogic.Cutscenes.Commands;
using Pathfinding;
using Kingmaker.Controllers.Combat;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using System.Text;
using Kingmaker.Blueprints.Root.Strings;

namespace CallOfTheWild.TImeStopMechanics
{

    public class TimeStoppedUnitPart : AdditiveUnitPart
    {

        public bool active()
        {
            return !buffs.Empty();
        }
    }


        public class EraseFromTime : BuffLogic,
                                 IInitiatorRulebookHandler<RuleSavingThrow>,
                                 IInitiatorRulebookHandler<RuleDealDamage>,
                                 IInitiatorRulebookHandler<RuleDrainEnergy>,
                                 IInitiatorRulebookHandler<RuleDealStatDamage>
    {
        public bool make_invisible;
        [JsonProperty]
        private Vector3 original_scale;
        [JsonProperty]
        private int was_immune_to_paralysis_count = 0;

        static readonly FastGetter getNextTickTime = Helpers.CreateGetter<Buff>("NextTickTime");
        static readonly FastSetter setNextTickTime = Helpers.CreateSetter<Buff>("NextTickTime");

        public override void OnTurnOn()
        {
            this.Owner.State.IsUntargetable.Retain();
            //this.Owner.Unit.PreventDirectControl.Retain();
            this.Owner.State.AddCondition(UnitCondition.CantAct, (Buff)null);
            this.Owner.State.AddCondition(UnitCondition.CantMove, (Buff)null);
            foreach (var buff in Owner.Buffs.Enumerable)
            {
                if (buff == Buff) continue;
                var endTime = buff.EndTime;
                if (endTime != TimeSpan.MaxValue) buff.EndTime = endTime + Buff.TimeLeft;
                var nextTick = (TimeSpan)getNextTickTime(buff);
                if (nextTick != TimeSpan.MaxValue) setNextTickTime(buff, nextTick + Buff.TimeLeft);
            }

            if (make_invisible)
            {
                original_scale = Owner.Unit.View.transform.localScale;
                Owner.Unit.View.transform.localScale = new Vector3(0, 0, 0);
            }

            while (this.Owner.Unit.Descriptor.State.HasConditionImmunity(UnitCondition.Paralyzed))
            {
                this.Owner.Unit.Descriptor.State.RemoveConditionImmunity(UnitCondition.Paralyzed);
                was_immune_to_paralysis_count++;
            }
            this.Owner.Unit.Descriptor.State.AddCondition(UnitCondition.Paralyzed);
            this.Owner.Ensure<TimeStoppedUnitPart>().addBuff(this.Fact);
        }

        public override void OnTurnOff()
        {
            this.Owner.State.IsUntargetable.Release();
            this.Owner.Unit.PreventDirectControl.Release();
            this.Owner.State.RemoveCondition(UnitCondition.CantAct);
            this.Owner.State.RemoveCondition(UnitCondition.CantMove);
            if (make_invisible)
            {
                Owner.Unit.View.transform.localScale = original_scale;
            }

            this.Owner.Unit.Descriptor.State.RemoveCondition(UnitCondition.Paralyzed);
            for (int i = 0; i < was_immune_to_paralysis_count; i++)
            {
                this.Owner.Unit.Descriptor.State.AddConditionImmunity(UnitCondition.Paralyzed);
            }
            this.Owner.Ensure<TimeStoppedUnitPart>().removeBuff(this.Fact);
        }

        public void OnEventAboutToTrigger(RuleSavingThrow evt)
        {
            if (evt.Initiator != Owner.Unit) return;
            evt.AutoPass = true;
        }
        public void OnEventDidTrigger(RuleSavingThrow evt) { }

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
            if (evt.Initiator != Owner.Unit) return;
            foreach (var dmg in evt.DamageBundle)
            {
                dmg.Immune = true;
            }
        }
        public void OnEventDidTrigger(RuleDealDamage evt) { }

        public void OnEventAboutToTrigger(RuleDealStatDamage evt)
        {
            if (evt.Initiator != Owner.Unit) return;
            evt.Immune = true;
        }
        public void OnEventDidTrigger(RuleDealStatDamage evt) { }

        public void OnEventAboutToTrigger(RuleDrainEnergy evt)
        {
            if (evt.Initiator != Owner.Unit) return;
            evt.TargetIsImmune = true;
        }

        public void OnEventDidTrigger(RuleDrainEnergy evt) { }
    }
}
