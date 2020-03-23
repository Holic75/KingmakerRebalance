using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.DeadTargetMechanics
{
    [AllowedOn(typeof(BlueprintAbility))]
    public class AbilityTargetRecentlyDead : BlueprintComponent, IAbilityTargetChecker
    {
        public BlueprintBuff RecentlyDeadBuff;

        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            bool flag1 = target.Unit != null && ((target.Unit.Descriptor.State.IsDead || target.Unit.Descriptor.State.HasCondition(UnitCondition.DeathDoor)) && target.Unit.Descriptor.HasFact((BlueprintUnitFact)this.RecentlyDeadBuff)) && !target.Unit.Descriptor.State.HasCondition(UnitCondition.Petrified);
            return flag1;
        }
    }



    [Harmony12.HarmonyPatch(typeof(BlueprintAbility))]
    [Harmony12.HarmonyPatch("CanCastToDeadTarget", Harmony12.MethodType.Getter)]
    class BlueprintAbility__CanCastToDeadTarget__Patch
    {
        static void Postfix(BlueprintAbility __instance, ref bool __result)
        {
            __result = __result || __instance.GetComponent<AbilityTargetRecentlyDead>() != null;
        }
    }
}
