using Kingmaker.Items;
using Kingmaker.View.Equipment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.SharedHandItems
{
    //allow summoned weapons to equip as free action
    /*[Harmony12.HarmonyPatch(typeof(UnitViewHandsEquipment))]
    [Harmony12.HarmonyPatch("HandleEquipmentSetChanged", Harmony12.MethodType.Normal)]
    class UnitViewHandsEquipment_HandleEquipmentSlotUpdated_Patch
    {
        static int[] secondary_hand_share_slots = new [] {1, 0, -1, -1};
        static int[] primary_hand_share_slots = new int[] {2, -1, 1, -1 };
        static bool Prefix(UnitViewHandsEquipment __instance)
        {
            var tr = Harmony12.Traverse.Create(__instance);

            if (!tr.Property("Active").GetValue<bool>())
            {
                return true;
            }

            //does not work on polymorphed units and pets
            if (__instance.Owner.Descriptor.IsPet || __instance.Owner.Body.IsPolymorphed)
            {
                return true;
            }
            WeaponSet active_set = tr.Field("m_ActiveSet").GetValue<WeaponSet>();
            WeaponSet new_set = __instance.GetSelectedWeaponSet();

            int current_slot = __instance.Owner.Body.CurrentHandEquipmentSetIndex;
            int paired_slot = secondary_hand_share_slots[current_slot];
            if (paired_slot != -1
                && __instance.Owner.Body.HandsEquipmentSets[paired_slot].SecondaryHand.HasItem
                && __instance.Owner.Body.HandsEquipmentSets[paired_slot].SecondaryHand.CanRemoveItem()
                && !__instance.Owner.Body.HandsEquipmentSets[current_slot].SecondaryHand.HasItem
                && __instance.Owner.Body.HandsEquipmentSets[current_slot].SecondaryHand.CanInsertItem(__instance.Owner.Body.HandsEquipmentSets[paired_slot].SecondaryHand.MaybeItem)
                )
            {
                var item = __instance.Owner.Body.HandsEquipmentSets[paired_slot].SecondaryHand.MaybeItem;
                __instance.Owner.Body.HandsEquipmentSets[current_slot].SecondaryHand.InsertItem(item);
                __instance.Owner.Body.HandsEquipmentSets[paired_slot].SecondaryHand.RemoveItem();
                __instance.Owner.View.HandleEquipmentSlotUpdated(__instance.Owner.Body.HandsEquipmentSets[current_slot].SecondaryHand, null);
                __instance.Owner.View.HandleEquipmentSlotUpdated(__instance.Owner.Body.HandsEquipmentSets[paired_slot].SecondaryHand, item);
            }


            paired_slot = primary_hand_share_slots[current_slot];
            if (paired_slot != -1
                && __instance.Owner.Body.HandsEquipmentSets[paired_slot].PrimaryHand.HasItem
                && __instance.Owner.Body.HandsEquipmentSets[paired_slot].PrimaryHand.CanRemoveItem()
                && !__instance.Owner.Body.HandsEquipmentSets[current_slot].PrimaryHand.HasItem
                && __instance.Owner.Body.HandsEquipmentSets[current_slot].PrimaryHand.CanInsertItem(__instance.Owner.Body.HandsEquipmentSets[paired_slot].PrimaryHand.MaybeItem)
                )
            { 
                var item = __instance.Owner.Body.HandsEquipmentSets[paired_slot].PrimaryHand.MaybeItem;
                __instance.Owner.Body.HandsEquipmentSets[current_slot].PrimaryHand.InsertItem(item);
                __instance.Owner.Body.HandsEquipmentSets[paired_slot].PrimaryHand.RemoveItem();
                __instance.Owner.View.HandleEquipmentSlotUpdated(__instance.Owner.Body.HandsEquipmentSets[current_slot].PrimaryHand, null);
                __instance.Owner.View.HandleEquipmentSlotUpdated(__instance.Owner.Body.HandsEquipmentSets[paired_slot].PrimaryHand, item);
            }

            return true;
        }
    }*/
}
