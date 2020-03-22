using Harmony12;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.NewMechanics
{

    //make dialog skill checks account for temporary skill bonuses if possible
    [Harmony12.HarmonyPatch(typeof(RulePartySkillCheck))]
    [Harmony12.HarmonyPatch("Calculate", Harmony12.MethodType.Normal)]
    class Patch_RulePartySkillCheck_Calculate_Transpiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var check_d20_index = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("D20"));

            codes[check_d20_index] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                   new Func<RuleSkillCheck, int>(getD20Value).Method);
            return codes.AsEnumerable();
        }


        static private int getD20Value(RuleSkillCheck evt)
        {
            if (evt.IsTriggererd)
            {
                return evt.D20 + evt.Bonus;
            }
            else
            {
                return evt.D20;
            }
        }
    }
}
