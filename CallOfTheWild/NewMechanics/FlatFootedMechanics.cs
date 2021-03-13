using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.FlatFootedMechanics
{

    public class UnitPartNotFlatFootedBeforeInitiative : CallOfTheWild.AdditiveUnitPart
    {
        public bool active()
        {
            return !this.buffs.Empty();
        }
    }


    [Harmony12.HarmonyPatch(typeof(RuleCheckTargetFlatFooted))]
    [Harmony12.HarmonyPatch("OnTrigger", Harmony12.MethodType.Normal)]
    class RuleCheckTargetFlatFooted__OnTrigger__Patch
    {
        static bool Prefix(RuleCheckTargetFlatFooted __instance)
        {
            bool ignore_initiative = (__instance.Target.Get<UnitPartNotFlatFootedBeforeInitiative>()?.active()).GetValueOrDefault();
            bool is_flat_footed = __instance.ForceFlatFooted 
                                      || !__instance.Target.CombatState.CanActInCombat && !__instance.IgnoreVisibility && !ignore_initiative
                                      || (__instance.Target.Descriptor.State.IsHelpless 
                                      || __instance.Target.Descriptor.State.HasCondition(UnitCondition.Stunned) 
                                      || !__instance.Target.Memory.Contains(__instance.Initiator) && !__instance.IgnoreVisibility) 
                                      || (UnitPartConcealment.Calculate(__instance.Target, __instance.Initiator, false) == Concealment.Total && !__instance.IgnoreConcealment 
                                          || __instance.Target.Descriptor.State.HasCondition(UnitCondition.LoseDexterityToAC)) 
                                      || ((__instance.Target.Descriptor.State.HasCondition(UnitCondition.Shaken) 
                                          || __instance.Target.Descriptor.State.HasCondition(UnitCondition.Frightened)) 
                                          && (bool)__instance.Initiator.Descriptor.State.Features.ShatterDefenses);

            //TODO: fix shatter defenses not to apply on first hit
            var tr = Harmony12.Traverse.Create(__instance);
            tr.Property("IsFlatFooted").SetValue(is_flat_footed);
            return false;
        }
    }


    [AllowedOn(typeof(BlueprintBuff))]
    public class FlatFootedAgainstCaster : BuffLogic, ITargetRulebookHandler<RuleCheckTargetFlatFooted>, ITargetRulebookHandler<RuleAttackRoll>
    {
        public bool remove_after_attack;
        public BlueprintUnitFact ranged_allowed_fact;

        public void OnEventAboutToTrigger(RuleCheckTargetFlatFooted evt)
        {
            var attack = Rulebook.CurrentContext.AllEvents.LastOfType<RuleAttackRoll>();

            if (attack == null)
            {
                return;
            }
            bool allowed = attack.Weapon.Blueprint.IsMelee || (evt.Initiator != null && ranged_allowed_fact != null && evt.Initiator.Descriptor.HasFact(ranged_allowed_fact));
            if (allowed && evt.Initiator == this.Context?.MaybeCaster)
            {
                evt.ForceFlatFooted = true;
            }
        }


        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {

        }

        public void OnEventDidTrigger(RuleCheckTargetFlatFooted evt)
        {
        }

        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
            if (!remove_after_attack)
            {
                return;
            }

            if (evt.Initiator == this.Fact.MaybeContext?.MaybeCaster)
            {
                this.Buff.Remove();
            }
        }
    }


    [AllowedOn(typeof(BlueprintBuff))]
    public class FlatFootedAgainstAttacType : BuffLogic, ITargetRulebookHandler<RuleCheckTargetFlatFooted>, ITargetRulebookHandler<RuleAttackRoll>
    {
        public bool remove_after_attack;
        public AttackType[] allowed_attack_types;

        public void OnEventAboutToTrigger(RuleCheckTargetFlatFooted evt)
        {
            var attack = Rulebook.CurrentContext.AllEvents.LastOfType<RuleAttackRoll>();

            if (attack == null)
            {
                return;
            }
            bool allowed = allowed_attack_types.Empty() || allowed_attack_types.Contains(attack.Weapon.Blueprint.AttackType);
            if (allowed)
            {
                evt.ForceFlatFooted = true;
            }
        }

        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {

        }

        public void OnEventDidTrigger(RuleCheckTargetFlatFooted evt)
        {
        }

        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
            if (!remove_after_attack)
            {
                return;
            }

            if (evt.Initiator == this.Fact.MaybeContext?.MaybeCaster)
            {
                this.Buff.Remove();
            }
        }
    }

    [AllowedOn(typeof(BlueprintUnitFact))]
    public class TargetFlatfooted : RuleTargetLogicComponent<RuleCheckTargetFlatFooted>
    {
        public override void OnEventAboutToTrigger(RuleCheckTargetFlatFooted evt)
        {
            evt.ForceFlatFooted = true;
        }

        public override void OnEventDidTrigger(RuleCheckTargetFlatFooted evt)
        {
        }
    }


    public class NotFlatFootedBeforeInitiative : OwnedGameLogicComponent<UnitDescriptor>
    {
        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartNotFlatFootedBeforeInitiative>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartNotFlatFootedBeforeInitiative>().removeBuff(this.Fact);
        }
    }
}
