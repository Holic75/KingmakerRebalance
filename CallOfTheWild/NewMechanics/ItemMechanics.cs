using Kingmaker.Blueprints;
using Kingmaker.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.ItemMechanics
{
    class ForcePrimary: BlueprintComponent
    {
    }



    [Harmony12.HarmonyPatch(typeof(ItemEntityWeapon))]
    [Harmony12.HarmonyPatch("IsSecondary", Harmony12.MethodType.Getter)]
    class ItemEntityWeapon__IsSecondary__Patch
    {
        static void Postfix(ItemEntityWeapon __instance, ref bool __result)
        {
            if (__result)
            {
                __result = __instance?.Blueprint.GetComponent<ForcePrimary>() == null;
            }
        }
    }
}
