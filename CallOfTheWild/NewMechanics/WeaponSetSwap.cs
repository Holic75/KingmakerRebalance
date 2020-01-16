using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View.Equipment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.NewMechanics
{
    internal class WeaponSetSwapPatch
    {

        internal static void Run()
        {
            var AnimateEquipmentChangeInCombat = typeof(UnitViewHandsEquipment).GetNestedTypes(Harmony12.AccessTools.all).First(x => x.Name.Contains("AnimateEquipmentChangeInCombat"));
            var original = Harmony12.AccessTools.Method(AnimateEquipmentChangeInCombat, "MoveNext");
            var transpiler = Harmony12.AccessTools.Method(typeof(WeaponSetSwapPatch), nameof(AnimateEquipmentChangeInCombat_Transpiler));
            try
            {
               Main.harmony.Patch(original, null, null, new Harmony12.HarmonyMethod(transpiler));
            }
            catch (Exception ex)
            {
                Main.logger.Log(ex.ToString());
            }
        }

        public static IEnumerable<Harmony12.CodeInstruction> AnimateEquipmentChangeInCombat_Transpiler(IEnumerable<Harmony12.CodeInstruction> instructions)
        {
            List<Harmony12.CodeInstruction> codes = new List<Harmony12.CodeInstruction>();
            try
            {
                codes = instructions.ToList();
                var check_cooldown_idx = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("HasCooldownForCommand"));
                codes[check_cooldown_idx - 1] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call, new Func<Kingmaker.Controllers.Combat.UnitCombatState, bool>(cannotChangeEquipment).Method);
                codes[check_cooldown_idx] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Nop); //to keep same length
                //codes.RemoveAt(check_cooldown_idx);
                //codes[check_cooldown_idx - 1].opcode = System.Reflection.Emit.OpCodes.Ldc_I4_3; //replace with move action

                var set_cooldown_idx = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("set_StandardAction"));
                codes[set_cooldown_idx - 2] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call, new Action<Kingmaker.Controllers.Combat.UnitCombatState>(consumeMoveAction).Method);
                codes[set_cooldown_idx - 1] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Nop);
                codes[set_cooldown_idx] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Nop);
                codes.InsertRange(set_cooldown_idx, new Harmony12.CodeInstruction[]{
                                                                                       new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Nop),
                                                                                       new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Nop),
                                                                                       new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Nop),
                                                                                       new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Nop),
                                                                                       new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Nop),
                                                                                       new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Nop),
                                                                                       new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Nop),
                                                                                       new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Nop)
                                                                                   }); // to keep the same size

                //codes[set_cooldown_idx].operand = typeof(Kingmaker.Controllers.Combat.UnitCombatState.Cooldowns).GetMethod("set_MoveAction");
                //codes[set_cooldown_idx - 1].operand = 3.0f;
            }
            catch (Exception ex)
            {
                Main.logger.Log(ex.ToString());
            }

            return codes.AsEnumerable();
        }


        static bool cannotChangeEquipment(Kingmaker.Controllers.Combat.UnitCombatState combat_state)
        {
            return (combat_state.HasCooldownForCommand(UnitCommand.CommandType.Standard) && combat_state.HasCooldownForCommand(UnitCommand.CommandType.Move));
        }

        static void consumeMoveAction(Kingmaker.Controllers.Combat.UnitCombatState combat_state)
        {
            if (combat_state.Cooldown.MoveAction > 0.0f)
            {
                combat_state.Cooldown.StandardAction = 6.0f;
                combat_state.Cooldown.MoveAction = 6.0f;
            }
            else
            {
                combat_state.Cooldown.MoveAction = 3.0f;
            }
        }

    }
}
