using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
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
        public bool active(UnitEntityData attacker)
        {
            bool res = false;
            foreach (var b in buffs)
            {
                b.CallComponents<IAlwaysFlanked>(a => res = a.worksFor(attacker));
                if (res)
                {
                    break;
                }
            }
            return res;
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public abstract class IAlwaysFlanked : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {
        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartAlwaysFlanked>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartAlwaysFlanked>().removeBuff(this.Fact);
        }

        public abstract bool worksFor(UnitEntityData attacker);
    }


    public class AlwaysFlanked : IAlwaysFlanked
    {
        public override bool worksFor(UnitEntityData attacker)
        {
            return true;
        }
    }


    public class AlwaysFlankedIfEngagedByCaster : IAlwaysFlanked
    {
        public override bool worksFor(UnitEntityData attacker)
        {
            return (this.Fact.MaybeContext?.MaybeCaster?.CombatState.IsEngage(this.Owner.Unit)).GetValueOrDefault();
        }
    }


    [Harmony12.HarmonyPatch(typeof(UnitCombatState))]
    [Harmony12.HarmonyPatch("IsFlanked", Harmony12.MethodType.Getter)]
    class UnitCombatState__IsFlanked__Patch
    {
        static bool Prefix(UnitCombatState __instance, ref bool __result)
        {
            if ((__instance.Unit.Get<UnitPartAlwaysFlanked>()?.active(null)).GetValueOrDefault()
                 && !__instance.Unit.Descriptor.State.Features.CannotBeFlanked)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
