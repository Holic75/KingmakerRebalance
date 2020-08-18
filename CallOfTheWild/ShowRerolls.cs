using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    /*[Harmony12.HarmonyPatch(typeof(RuleRollD20))]
    [Harmony12.HarmonyPatch("Roll", Harmony12.MethodType.Normal)]
    class RuleRollD20__Roll__Patch
    {
        static bool Prefix(RuleRollD20 __instance, ref int? ___m_PreRolledResult, ref int __result)
        {
            
            int? preRolledResult = ___m_PreRolledResult;
            int val1 = !preRolledResult.HasValue ? RulebookEvent.Dice.D20 : preRolledResult.Value;
            for (int rerollsAmount = __instance.RerollsAmount; rerollsAmount > 0; --rerollsAmount)
            {
                int old_value = val1;
                int new_value = RulebookEvent.Dice.D20;
                val1 = !__instance.TakeBest ? Math.Min(val1, new_value) : Math.Max(val1, new_value);
                Common.AddBattleLogMessage(__instance.Initiator.CharacterName + " rerolls: " + $"({old_value}  >>  {new_value}, retains {(__instance.TakeBest ? "best" : "worst")})");
            }
            __result = val1;
            return false;
        }
    }*/

}
