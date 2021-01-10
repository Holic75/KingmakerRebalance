using Kingmaker.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.FlankingMechanics
{
    public class UnitPartAlwaysFlanked : AdditiveUnitPart
    {
        public bool active()
        {
            return !this.buffs.Empty();
        }
    }


    public class AlwaysFlanked : OwnedGameLogicComponent<UnitDescriptor>
    {
        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartAlwaysFlanked>().addBuff(this.Fact);
        }

        public override void OnFactDeactivate()
        {
            this.Owner.Get<UnitPartAlwaysFlanked>()?.removeBuff(this.Fact);
        }
    }


    [Harmony12.HarmonyPatch(typeof(UnitCombatState))]
    [Harmony12.HarmonyPatch("IsFlanked", Harmony12.MethodType.Getter)]
    class UnitCombatState__IsFlanked__Patch
    {
        static bool Prefix(UnitCombatState __instance, ref bool __result)
        {
            if ((__instance.Unit.Get<UnitPartAlwaysFlanked>()?.active()).GetValueOrDefault()
                 && !__instance.Unit.Descriptor.State.Features.CannotBeFlanked)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
