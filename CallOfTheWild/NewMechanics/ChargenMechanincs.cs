using Harmony12;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.ChargenMechanics
{
    //mercs will also receive 25 pts
    [Harmony12.HarmonyPatch(typeof(LevelUpState), Harmony12.MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(UnitDescriptor), typeof(LevelUpState.CharBuildMode) })]
    class Patch_LevelUpState_ctor_Transpiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var check_custom_companion = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Call && x.operand.ToString().Contains("IsCustomCompanion"));
            codes[check_custom_companion] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call, new Func<UnitDescriptor, bool>(isCustomCompanionAlwaysFalse).Method);
            return codes.AsEnumerable();
        }


        static public bool isCustomCompanionAlwaysFalse(UnitDescriptor unit)
        {
            return false;
        }
    }
}
