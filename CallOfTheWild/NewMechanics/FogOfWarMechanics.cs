﻿using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Selection;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CallOfTheWild.FogOfWarMechanics
{
    public class UnitPartFogOfWarRevealer : AdditiveUnitPart
    {
        public bool active()
        {
            return !buffs.Empty();
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class FogOfWarRevealer : OwnedGameLogicComponent<UnitDescriptor>
    {
        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartFogOfWarRevealer>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartFogOfWarRevealer>().removeBuff(this.Fact);
        }
    }

    [Harmony12.HarmonyPatch(typeof(FogOfWarController))]
    [Harmony12.HarmonyPatch("Tick", Harmony12.MethodType.Normal)]
    public class FogOfWarController_Tick_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var get_party_idx = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("Party"));

            codes[get_party_idx] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                    new Func<List<UnitEntityData>>(getPartyRevealars).Method
                                                                    );

            return codes.AsEnumerable();
        }


        internal static List<UnitEntityData> getPartyRevealars()
        {
            var party = Game.Instance.Player.Party.ToList();
            var revealers = Game.Instance.State.Units.Where(u => (u.Descriptor.Get<UnitPartFogOfWarRevealer>()?.active()).GetValueOrDefault());
            party.AddRange(revealers);

            return party;
        }
    }



    [Harmony12.HarmonyPatch(typeof(UnitEntityData))]
    [Harmony12.HarmonyPatch("IsDirectlyControllable", Harmony12.MethodType.Getter)]
    public class UnitEntityData_IsDirectlyControllable_Patch
    {
        static void Postfix(UnitEntityData __instance, ref bool __result)
        {
            __result = __result || (__instance?.Get<UnitPartFogOfWarRevealer>()?.active()).GetValueOrDefault();
        }
    }



    [Harmony12.HarmonyPatch(typeof(SelectionManager))]
    [Harmony12.HarmonyPatch("GetNearestSelectedUnit", Harmony12.MethodType.Normal)]
    public class SelectionManager_GetNearestSelectedUnit_Patch
    {
        static bool Prefix(SelectionManager __instance, Vector3 point, ref UnitEntityData __result)
        {
            if (__instance.SelectedUnits.Count > 1)
                __result = __instance.SelectedUnits.Where<UnitEntityData>((Func<UnitEntityData, bool>)(u =>!(u.Get<UnitPartFogOfWarRevealer>()?.active()).GetValueOrDefault())).OrderBy<UnitEntityData, float>((Func<UnitEntityData, float>)(u => u.DistanceTo(point))).FirstOrDefault<UnitEntityData>();
            else
                __result = __instance.SelectedUnits.FirstOrDefault<UnitEntityData>(u => !(u.Get<UnitPartFogOfWarRevealer>()?.active()).GetValueOrDefault());

            return false;
        }
    }

}
