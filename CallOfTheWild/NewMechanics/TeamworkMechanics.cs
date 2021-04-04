using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using Newtonsoft.Json;
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

            int bonus = AttackBonus + (!evt.Target.CombatState.IsFlanked ? 0 : AdditionalFlankBonus);
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
    public class AlliedSpellcasterSameSpellBonus : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>, IGlobalRulebookHandler<RuleSpellResistanceCheck>, IConcentrationBonusProvider, IRulebookHandler<RuleSpellResistanceCheck>, IGlobalRulebookSubscriber
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

        private bool hasSpell(UnitDescriptor unit, BlueprintAbility spell)
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


    public class ContextActionOnUnitsEngagingTarget : ContextAction
    {
        public ActionList actions;
        public bool ignore_caster;

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


            foreach (UnitEntityData unitEntityData in this.Target.Unit.CombatState.EngagedBy)
            {
                if (unitEntityData == this.Context.MaybeCaster && ignore_caster)
                {
                    continue;
                }
                using (this.Context.GetDataScope((TargetWrapper)unitEntityData))
                {
                    this.actions.Run();
                }
            }
        }
    }


    public class ContextActionOnUnitsWithinRadius : ContextAction
    {
        public ActionList actions;
        public Feet Radius;
        public bool include_dead = false;
        public bool ignore_target;
        public bool around_caster;

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

            var target = around_caster ? this.Context.MaybeCaster : this.Target.Unit;

            foreach (UnitEntityData unitEntityData in GameHelper.GetTargetsAround(target.Position, target.Corpulence + this.Radius.Meters, true, include_dead))
            {
                if (unitEntityData == target && ignore_target)
                {
                    continue;
                }
                using (this.Context.GetDataScope((TargetWrapper)unitEntityData))
                {
                    this.actions.Run();
                }

            }
        }
    }


    public class ContextConditionCasterHasSoloTactics : ContextCondition
    {
        protected override string GetConditionCaption()
        {
            return string.Empty;
        }

        protected override bool CheckCondition()
        {
            var unit = this.Context?.MaybeCaster;
            if (unit == null)
            {
                return false;
            }

            //Main.logger.Log($"{(bool)unit.Descriptor.State.Features.SoloTactics} " + unit.CharacterName);
            return (bool)unit.Descriptor.State.Features.SoloTactics;
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


    [AllowedOn(typeof(BlueprintUnitFact))]
    [AllowMultipleComponents]
    public class ProvokeAttackFromFactOwners : ContextAction
    {
        public BlueprintUnitFact fact;
        public bool except_caster;

        public override string GetCaption()
        {
            return string.Empty;
        }

        public override void RunAction()
        {
            foreach (UnitEntityData attacker in this.Target.Unit.CombatState.EngagedBy)
            {
                if (attacker.Descriptor.HasFact(this.fact) && (attacker != this.Context.MaybeCaster || !except_caster))
                {
                    Kingmaker.Game.Instance.CombatEngagementController.ForceAttackOfOpportunity(attacker, this.Target.Unit);
                }
            }
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    [AllowMultipleComponents]
    public class ProvokeRangedAttackFromFactOwners : ContextAction
    {
        public BlueprintUnitFact fact;
        public BlueprintUnitFact no_fact;
        public Feet distance;
        public bool require_swift_action;
        public bool allow_engaged;
        public bool except_caster;


        public override string GetCaption()
        {
            return string.Empty;
        }

        public override void RunAction()
        {
            foreach (UnitEntityData attacker in GameHelper.GetTargetsAround(this.Target.Unit.Position, distance.Meters, false, false))
            {
                if (attacker.Descriptor.HasFact(this.fact) 
                    && attacker.Descriptor.State.CanAct
                    && (no_fact == null || !attacker.Descriptor.HasFact(no_fact))
                    && (!attacker.CombatState.IsEngage(attacker) || allow_engaged) && attacker.CombatState.CanAttackOfOpportunity
                    && attacker.CanAttack(this.Target.Unit) && (attacker?.Body.PrimaryHand?.MaybeWeapon?.Blueprint?.IsRanged).GetValueOrDefault()
                    && (attacker.CombatState.Cooldown.SwiftAction == 0.0f || !require_swift_action)
                    && (attacker != Context?.MaybeCaster || !except_caster) 
                    )
                {
                    if (require_swift_action)
                    {
                        attacker.CombatState.Cooldown.SwiftAction += 6.0f;
                    }
                    RuleAttackWithWeapon attackWithWeapon = new RuleAttackWithWeapon(attacker, this.Target.Unit, attacker?.Body.PrimaryHand?.MaybeWeapon, 0);
                    attackWithWeapon.Reason = (RuleReason)this.Context;
                    RuleAttackWithWeapon rule = attackWithWeapon;
                    this.Context.TriggerRule<RuleAttackWithWeapon>(rule);
                }
            }
        }
    }


    [AllowMultipleComponents]
    [AllowedOn(typeof(BlueprintAbility))]
    public class AbilityTargetHasFactOrCasterHasSoloTactics : BlueprintComponent, IAbilityTargetChecker
    {
        public BlueprintUnitFact fact;

        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            UnitEntityData unit = target.Unit;
            return ((bool)caster.Descriptor.State.Features.SoloTactics) || unit.Descriptor.HasFact(fact);
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AddFactsFromCasterIfHasBuffs : BuffLogic, IUnitReapplyFeaturesOnLevelUpHandler, IUnitSubscriber
    {
        [JsonProperty]
        private List<Fact> m_AppliedFacts = new List<Fact>();
        public List<BlueprintUnitFact> facts = new List<BlueprintUnitFact>();
        public List<BlueprintUnitFact> prerequsites = new List<BlueprintUnitFact>();

        public void HandleUnitReapplyFeaturesOnLevelUp()
        {
            this.OnRecalculate();
        }

        public override void OnFactActivate()
        {
            UnitEntityData maybeCaster = this.Fact.MaybeContext?.MaybeCaster;
            if (maybeCaster == null)
                return;
            if (this.Owner == maybeCaster?.Descriptor)
            {
                return;
            }

            for (int i = 0; i < facts.Count; i++)
            {
                if (maybeCaster.Descriptor.HasFact(facts[i]) && maybeCaster.Descriptor.HasFact(prerequsites[i]) && !this.Owner.HasFact(facts[i]))
                {
                    this.m_AppliedFacts.Add(this.Owner.Logic.AddFact((BlueprintFact)facts[i], this.Context));
                }
            }
        }

        public override void OnFactDeactivate()
        {
            this.m_AppliedFacts.ForEach(new Action<Fact>(((FactCollection)this.Owner.Logic).RemoveFact));
        }
    }


    [Harmony12.HarmonyPatch(typeof(AddFactsFromCaster))]
    [Harmony12.HarmonyPatch("OnFactActivate", Harmony12.MethodType.Normal)]
    class AddFactsFromCaster__OnFactActivate__Patch
    {
        static bool Prefix(AddFactsFromCaster __instance, ref List<Fact> ___m_AppliedFacts)
        {
            UnitEntityData maybeCaster = __instance.Fact.MaybeContext?.MaybeCaster;
            if (maybeCaster == null)
                return false;
            foreach (BlueprintUnitFact fact in __instance.Facts)
            {
                if (maybeCaster.Descriptor.HasFact(fact) && !__instance.Owner.HasFact(fact))
                    ___m_AppliedFacts.Add(__instance.Owner.Logic.AddFact((BlueprintFact)fact, __instance.Context));
            }
            return false;
        }
    }
}
