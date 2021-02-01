using Kingmaker.Blueprints;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.RerollsMechanics
{ 
    class ExtraTakeBestRerollUnitPart : AdditiveUnitPart
    {
        public bool active()
        {
            return !this.buffs.Empty();
        }
    }

    class IgnoreTakeWorstRerollsUnitPart : AdditiveUnitPart
    {
        public bool active()
        {
            return !this.buffs.Empty();
        }
    }


    class ExtraGoodRerollOnTakeWorstUnitPart : AdditiveUnitPart
    {
        public bool active()
        {
            return !this.buffs.Empty();
        }
    }


    public class ExtraTakeBestReroll : OwnedGameLogicComponent<UnitDescriptor>
    {

        public override void OnFactActivate()
        {
            this.Owner.Ensure<ExtraTakeBestRerollUnitPart>().addBuff(this.Fact);

        }

        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<ExtraTakeBestRerollUnitPart>().removeBuff(this.Fact);
        }
    }


    public class IgnoreTakeWorstRerolls : OwnedGameLogicComponent<UnitDescriptor>
    {

        public override void OnFactActivate()
        {
            this.Owner.Ensure<IgnoreTakeWorstRerollsUnitPart>().addBuff(this.Fact);

        }

        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<IgnoreTakeWorstRerollsUnitPart>().removeBuff(this.Fact);
        }
    }


    public class ExtraGoodRerollOnTakeWorst : OwnedGameLogicComponent<UnitDescriptor>
    {

        public override void OnFactActivate()
        {
            this.Owner.Ensure<ExtraGoodRerollOnTakeWorstUnitPart>().addBuff(this.Fact);

        }

        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<ExtraGoodRerollOnTakeWorstUnitPart>().removeBuff(this.Fact);
        }
    }



    [Harmony12.HarmonyPatch(typeof(RuleRollD20))]
    [Harmony12.HarmonyPatch("Roll", Harmony12.MethodType.Normal)]
    class RuleRollD20__Roll__Patch
    {
        static bool Prefix(RuleRollD20 __instance, ref int? ___m_PreRolledResult, ref List<int> ___m_RollHistory, ref int __result)
        {         
            int? preRolledResult = ___m_PreRolledResult;
            int val1 = !preRolledResult.HasValue ? RulebookEvent.Dice.D(__instance.DiceFormula) : preRolledResult.Value;

            int rerollsAmount = __instance.RerollsAmount;

            if (__instance.TakeBest && rerollsAmount > 0 && (__instance.Initiator.Get<ExtraTakeBestRerollUnitPart>()?.active()).GetValueOrDefault())
            {
                rerollsAmount++;
            }

            if (!__instance.TakeBest && rerollsAmount > 0 && (__instance.Initiator.Get<IgnoreTakeWorstRerollsUnitPart>()?.active()).GetValueOrDefault())
            {
                rerollsAmount = 0;
            }

            ___m_RollHistory = new List<int>() {val1};
            for (; rerollsAmount > 0; --rerollsAmount)
            {
                int old_value = val1;
                int new_value = RulebookEvent.Dice.D(__instance.DiceFormula);
                ___m_RollHistory.Add(new_value);
                val1 = !__instance.TakeBest ? Math.Min(val1, new_value) : Math.Max(val1, new_value);
                Common.AddBattleLogMessage(__instance.Initiator.CharacterName + " rerolls: " + $"({old_value}  >>  {new_value}, retains {(__instance.TakeBest ? "best" : "worst")})");
            }

            if (!__instance.TakeBest && ___m_RollHistory.Count > 1 && (__instance.Initiator.Get<ExtraGoodRerollOnTakeWorstUnitPart>()?.active()).GetValueOrDefault())
            {
                int old_value = val1;
                int new_value = RulebookEvent.Dice.D(__instance.DiceFormula);
                ___m_RollHistory.Add(new_value);
                ___m_RollHistory.Sort();
                int retained_value = ___m_RollHistory[1];
                val1 = retained_value;
                Common.AddBattleLogMessage(__instance.Initiator.CharacterName + " rerolls: " + $"({old_value}  >>  {new_value}, retains {retained_value})");
            }

            __result = val1;
            return false;
        }
    }

}
