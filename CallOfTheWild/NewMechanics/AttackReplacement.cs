using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.AttackReplacementMechanics
{

    public class FullAttackWatcherUnitPart : UnitPart
    {
        public bool is_full_attack = false;
    }


    [Harmony12.HarmonyPatch(typeof(UnitAttack))]
    [Harmony12.HarmonyPatch("CreateFullAttack", Harmony12.MethodType.Normal)]
    class UnitAttack__CreateFullAttack__Patch
    {
        static bool Prefix(UnitAttack __instance)
        {
            Main.TraceLog();
            __instance.Executor.Descriptor.Ensure<FullAttackWatcherUnitPart>().is_full_attack = true;
            return true;
        }

        static void Postfix(UnitAttack __instance)
        {
            Main.TraceLog();
            __instance.Executor.Descriptor.Ensure<FullAttackWatcherUnitPart>().is_full_attack = false;
        }
    }




    [AllowMultipleComponents]
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class ReplaceAttackWithActionOnFullAttack : RuleInitiatorLogicComponent<RuleAttackRoll>, IInitiatorRulebookHandler<RuleCalculateAttacksCount>
    {
        private bool valid = false;
        public ActionList action;

        public override void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (evt.RuleAttackWithWeapon == null || !evt.RuleAttackWithWeapon.IsFirstAttack
                || !evt.RuleAttackWithWeapon.IsFullAttack || !this.valid)
            {
                return;
            }

            (this.Fact as IFactContextOwner)?.RunActionInContext(this.action, (TargetWrapper)evt.Target);
        }

        public void OnEventAboutToTrigger(RuleCalculateAttacksCount evt)
        {

        }

        public override void OnEventDidTrigger(RuleAttackRoll evt)
        {
            this.valid = false;
        }

        public void OnEventDidTrigger(RuleCalculateAttacksCount evt)
        {
            this.valid = false;
            if (!evt.Initiator.Ensure<FullAttackWatcherUnitPart>().is_full_attack)
            {
                return;
            }

            int num_attacks = evt.PrimaryHand.MainAttacks + evt.PrimaryHand.PenalizedAttacks + evt.SecondaryHand.MainAttacks + evt.SecondaryHand.PenalizedAttacks;

            if (num_attacks <= 1 || evt.PrimaryHand.MainAttacks <= 0)
            {
                return;
            }

            evt.PrimaryHand.AdditionalAttacks--;
            this.valid = true;
        }
    }






}
