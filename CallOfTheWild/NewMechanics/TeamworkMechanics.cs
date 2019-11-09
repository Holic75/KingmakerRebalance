using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.TeamworkMechanics
{
    [AllowedOn(typeof(BlueprintUnitFact))]
    [AllowMultipleComponents]
    public class CoordinatedShotAttackBonus : RuleInitiatorLogicComponent<RuleCalculateAttackBonus>
    {
        public int AttackBonus = 1;
        public int AdditionalFlankBonus = 1;
        public BlueprintUnitFact CoordinatedShotFact;


        public override void OnEventAboutToTrigger(RuleCalculateAttackBonus evt)
        {
            if (!evt.Weapon.Blueprint.IsRanged)
                return;

            int bonus = AttackBonus + (evt.Target.CombatState.IsFlanked ? 0 : AdditionalFlankBonus);
            if (this.Owner.State.Features.SoloTactics)
            {
                evt.AddBonus(bonus, this.Fact);
                return;
            }

            foreach (UnitEntityData unitEntityData in evt.Target.CombatState.EngagedBy)
            {
                if (unitEntityData.Descriptor.HasFact(this.CoordinatedShotFact) && unitEntityData != this.Owner.Unit)
                {
                    evt.AddBonus(bonus, this.Fact);
                    return;
                }
            }
        }

        public override void OnEventDidTrigger(RuleCalculateAttackBonus evt)
        {
        }
    }

    [AllowedOn(typeof(BlueprintUnitFact))]
    [AllowMultipleComponents]
    public class TeamworkACBonus : RuleTargetLogicComponent<RuleCalculateAC>
    {
        public BlueprintUnitFact teamwork_fact;
        public float Radius;
        public int value_per_unit = 1;
        public ModifierDescriptor descriptor;

        public override void OnEventAboutToTrigger(RuleCalculateAC evt)
        {
            int bonus = 0;
            foreach (UnitEntityData unitEntityData in GameHelper.GetTargetsAround(this.Owner.Unit.Position, this.Radius, true, false))
            {
                if ((unitEntityData.Descriptor.HasFact(this.teamwork_fact) || this.Owner.State.Features.SoloTactics) && unitEntityData != this.Owner.Unit && !unitEntityData.IsEnemy(this.Owner.Unit))
                {
                    bonus += value_per_unit;
                }
            }

            if (bonus == 0)
            {
                return;
            }
            evt.AddBonus(bonus, this.Fact);
           // evt.AddTemporaryModifier(evt.Target.Stats.AC.AddModifier(bonus, (GameLogicComponent)this, this.descriptor));
        }

        public override void OnEventDidTrigger(RuleCalculateAC evt)
        {
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    [AllowMultipleComponents]
    public class AlliedSpellcasterSameSpellBonus : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>, IGlobalRulebookHandler<RuleSpellResistanceCheck>, IConcentrationBonusProvider,  IRulebookHandler<RuleSpellResistanceCheck>, IGlobalRulebookSubscriber
    {
        public BlueprintUnitFact AlliedSpellcasterFact;
        public int Radius;

        public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            var spell = evt.Spell;
            foreach (UnitEntityData unitEntityData in GameHelper.GetTargetsAround(this.Owner.Unit.Position, (float)this.Radius, true, false))
            {
                if ((unitEntityData.Descriptor.HasFact(this.AlliedSpellcasterFact) || (bool)this.Owner.State.Features.SoloTactics) && unitEntityData != this.Owner.Unit && !unitEntityData.IsEnemy(this.Owner.Unit)
                     && hasSpell(unitEntityData.Descriptor, evt.Spell))
                {
                    evt.AddBonusCasterLevel(1);
                    evt.AddBonusConcentration(2);
                    break;
                }
            }
        }


        public void OnEventAboutToTrigger(RuleSpellResistanceCheck evt)
        {
            foreach (UnitEntityData unitEntityData in GameHelper.GetTargetsAround(this.Owner.Unit.Position, (float)this.Radius, true, false))
            {
                if ((unitEntityData.Descriptor.HasFact(this.AlliedSpellcasterFact) || (bool)this.Owner.State.Features.SoloTactics) && unitEntityData != this.Owner.Unit && !unitEntityData.IsEnemy(this.Owner.Unit)
                     && hasSpell(unitEntityData.Descriptor, evt.Ability))
                {
                    evt.AdditionalSpellPenetration += 2;
                    break;
                }
            }
        }

        public void OnEventDidTrigger(RuleSpellResistanceCheck evt)
        {
        }

        public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }

        public int GetStaticConcentrationBonus()
        {
            return 0;
        }

        private bool hasSpell (UnitDescriptor unit, BlueprintAbility spell)
        {
            var duplicates = SpellDuplicates.getDuplicates(spell);
            foreach (var sb in unit.Spellbooks)
            {
                foreach (var d in duplicates)
                {
                    if (sb.CanSpend(d))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }


    public class ContextActionOnUnitsWithinRadius : ContextAction
    {
        public ActionList actions;
        public Feet Radius;
        public bool include_dead = false;

        public override string GetCaption()
        {
            return string.Empty;
        }

        public override void RunAction()
        {
            if (actions == null || this.Target?.Unit == null)
            {
                return;
            }


            foreach (UnitEntityData unitEntityData in GameHelper.GetTargetsAround(this.Target.Unit.Position, this.Radius.Meters, true, include_dead))
            {
                using (this.Context.GetDataScope((TargetWrapper)unitEntityData))
                {
                    this.actions.Run();
                }

            }
        }
    }


    public class ContextConditionHasSoloTactics : ContextCondition
    {
        protected override string GetConditionCaption()
        {
            return string.Empty;
        }

        protected override bool CheckCondition()
        {
            var unit = this.Target?.Unit;
            if (unit == null)
            {
                return false;
            }

            //Main.logger.Log($"{(bool)unit.Descriptor.State.Features.SoloTactics} " + unit.CharacterName);
            return (bool)unit.Descriptor.State.Features.SoloTactics;
        }
    }


    public class ContextConditionAllyOrCasterWithSoloTacticsSurroundedByAllies : ContextCondition
    {
        public float radius;

        protected override string GetConditionCaption()
        {
            return string.Empty;
        }

        protected override bool CheckCondition()
        {
            var target = this.Target?.Unit;
            var caster = this.Context?.MaybeCaster;

            if (target == null || caster == null)
            {
                return false;
            }

            if (!target.IsAlly(caster))
            {
                return false;
            }

            if (target != caster)
            {
                return true;
            }

            if (!(bool)caster.Descriptor.State.Features.SoloTactics)
            {
                return false;
            }


            foreach (UnitEntityData unitEntityData in GameHelper.GetTargetsAround(this.Target.Unit.Position, this.radius, true, false))
            {
                if (unitEntityData.IsAlly(this.Context.MaybeCaster) && unitEntityData != caster)
                {
                    return true;
                }
            }
            return false;
        }
    }

}
