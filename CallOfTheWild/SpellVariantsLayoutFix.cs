using JetBrains.Annotations;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Persistence.Scenes;
using Kingmaker.UI;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CallOfTheWild
{
    namespace SpellVariantsLayoutFix
    {

        [Harmony12.HarmonyPatch(typeof(ActionBarGroupSlot))]
        [Harmony12.HarmonyPatch("Initialize", Harmony12.MethodType.Normal)]
        class HUDLayout__Initialize__Patch
        {
            static void Postfix(ActionBarGroupSlot __instance)
            {
                var t = __instance.transform;
                if (t == null)
                {
                    return;
                }
                var subgroup = t.Find("SubGroup");
                if (subgroup == null)
                {
                    return;
                }

                var horizontalLayout = subgroup.GetComponent<HorizontalLayoutGroupWorkaround>();
                if (horizontalLayout) UnityEngine.Object.DestroyImmediate(horizontalLayout);

                if (subgroup.GetComponent<GridLayoutGroupWorkaround>() == null)
                {
                    var grid = subgroup.gameObject.AddComponent<GridLayoutGroupWorkaround>();
                }
            }

        }

        //fix resource count for converted abilities
        [Harmony12.HarmonyPatch(typeof(AbilityData))]
        [Harmony12.HarmonyPatch("GetAvailableForCastCount", Harmony12.MethodType.Normal)]
        class MechanicActionBarSlotAbility__GetResource__Patch
        {
            static void Postfix(AbilityData __instance, ref int __result)
            {
                if (__result == -1 && __instance.ConvertedFrom != null)
                {
                    if (__instance.Blueprint != null)
                    {
                        AbilityResourceLogic abilityResourceLogic = __instance.Blueprint.GetComponents<AbilityResourceLogic>().FirstOrDefault<AbilityResourceLogic>();
                        BlueprintAbilityResource blueprintAbilityResource = (abilityResourceLogic != null && abilityResourceLogic.IsSpendResource) ? abilityResourceLogic.RequiredResource : null;
                        if (blueprintAbilityResource != null)
                            __result =  __instance.Fact.Owner.Resources.GetResourceAmount(blueprintAbilityResource) / abilityResourceLogic.CalculateCost(__instance);
                    }
                }
            }
        }

    }
}
