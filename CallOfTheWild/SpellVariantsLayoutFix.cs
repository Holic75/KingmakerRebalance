using JetBrains.Annotations;
using Kingmaker;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Persistence.Scenes;
using Kingmaker.UI;
using Kingmaker.UI.ActionBar;
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
                    //Main.logger.Log("No Transform");
                    return;
                }
                var subgroup = t.Find("SubGroup");
                if (subgroup == null)
                {
                    //Main.logger.Log("No subgroup");
                    return;
                }

                //Main.logger.Log("Slot ok");
                var horizontalLayout = subgroup.GetComponent<HorizontalLayoutGroupWorkaround>();
                if (horizontalLayout) UnityEngine.Object.DestroyImmediate(horizontalLayout);

                if (subgroup.GetComponent<GridLayoutGroupWorkaround>() == null)
                {
                    var grid = subgroup.gameObject.AddComponent<GridLayoutGroupWorkaround>();

                }
            }

        }
                /* [Harmony12.HarmonyPatch(typeof(HUDLayout))]
                 [Harmony12.HarmonyPatch("Initialize", Harmony12.MethodType.Normal)]
                 class HUDLayout__Initialize__Patch
                 {
                     static void Postfix(HUDLayout __instance)
                     {
                         string[] slot_object_names = new string[]
                         {
                             "/StaticCanvas/HUDLayout/ActionBar2/Slots",
                             "/StaticCanvas/HUDLayout/ActionBarAdditional/Slots",
                             "/StaticCanvas/HUDLayout/ActionBar2/Groups/Ability/Slots",
                             "/StaticCanvas/HUDLayout/ActionBar2/Groups/Spells/Slots",

                         };

                         foreach (var object_name in slot_object_names)
                         {
                             var slots = GameObject.Find(object_name);
                             if (slots == null)
                             {
                                 continue;
                             }
                             Main.logger.Log("Fixing Slots: " + object_name);
                             foreach (var t in slots.transform)
                             {

                                 Main.logger.Log("Checking: " + t.ToString());
                                 var subgroup = (t as Transform).Find("SubGroup");
                                 if (!subgroup)
                                 {
                                     continue;
                                 }
                                 Main.logger.Log("Found subgroup: " + object_name);
                                 var horizontalLayout = subgroup.GetComponent<HorizontalLayoutGroupWorkaround>();
                                 if (horizontalLayout) UnityEngine.Object.DestroyImmediate(horizontalLayout);

                                 if (subgroup.GetComponent<GridLayoutGroupWorkaround>() == null)
                                 {
                                     var grid = subgroup.gameObject.AddComponent<GridLayoutGroupWorkaround>();

                                 }
                             }
                             Main.logger.Log("Fixed Slots: " + object_name);
                         }
                     }
                 }*/
            }
}
