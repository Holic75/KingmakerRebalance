using Kingmaker.Blueprints;
using Kingmaker.Controllers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.NonOffensiveActionMechanics
{
    class NonOffensiveAbility: BlueprintComponent
    {
    }


    [Harmony12.HarmonyPatch(typeof(OffensiveActionsController))]
    [Harmony12.HarmonyPatch("OnTryToApplyAbilityEffect", Harmony12.MethodType.Normal)]
    class RulePrepareDamage_OnTrigger
    {
        //static public Dictionary<(MechanicsContext, UnitEntityData), bool> spell_target_map = new Dictionary<(MechanicsContext, UnitEntityData), bool>();
        static bool Prefix(OffensiveActionsController __instance, AbilityExecutionContext context, TargetWrapper target)
        {
            if (context?.SourceAbility?.GetComponent<NonOffensiveAbility>() != null)
            {
                return false;
            }
            return true;
        }
    }
}
