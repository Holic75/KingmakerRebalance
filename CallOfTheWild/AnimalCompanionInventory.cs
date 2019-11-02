using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.GenericSlot;
using Kingmaker.UI.Group;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UI.Tooltip;
using Kingmaker.UnitLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.EncumbranceHelper;

namespace CallOfTheWild
{
    class AnimalCompanionInventory
    {
       [Harmony12.HarmonyPatch(typeof(Inventory))]
        [Harmony12.HarmonyPatch("SetCharacter", Harmony12.MethodType.Normal)]
        class Inventory__SetCharacter__Patch
        {
            static bool Prefix(Inventory __instance)
            {
                UnitEntityData currentCharacter = GroupController.Instance.GetCurrentCharacter();
                __instance.Placeholder.gameObject.SetActive(currentCharacter.Body.IsPolymorphed);
                __instance.Sheet.SetCharacter(currentCharacter.Descriptor);
                __instance.VisualSettings.Initialize(currentCharacter);
                if (__instance.Doll.gameObject.activeSelf)
                {
                    __instance.Doll.SetupInfo(currentCharacter);
                }
                __instance.Stash.SetupCanEquipLayer();
                var tr = Harmony12.Traverse.Create(__instance);
                tr.Method("UpdatePlayerMoneyAndInventoryWeight").GetValue();
                tr.Field("UnitCapacity").SetValue(EncumbranceHelper.GetCarryingCapacity(currentCharacter.Descriptor));
                __instance.CharacterEncumbrance.Initialize(tr.Field("UnitCapacity").GetValue<CarryingCapacity>(), true);
                __instance.CharacterEncumbrance.SetTooltipData((object)currentCharacter, TooltipType.EncumbranceCharacter);

                return false;
            }
        }


        [Harmony12.HarmonyPatch(typeof(CharDollBase))]
        [Harmony12.HarmonyPatch("SetupInfo", Harmony12.MethodType.Normal)]
        class CharDollBase__SetupInfo__Patch
        {

            static EquipSlotBase.SlotType[] allowed_slots = new EquipSlotBase.SlotType[] { EquipSlotBase.SlotType.Belt, EquipSlotBase.SlotType.Neck, EquipSlotBase.SlotType.Wrist, EquipSlotBase.SlotType.Shoulders };
            static bool Prefix(CharDollBase __instance, UnitEntityData player)
            {
                var tr = Harmony12.Traverse.Create(__instance);
                tr.Property("CurrentUnit").SetValue(player);
                if (__instance.CurrentUnit == null)
                    return false;


                foreach (EquipSlotBase slot in tr.Property("SlotList").GetValue<List<EquipSlotBase>>())
                {
                    slot.Clear();
                    slot.SetupInfo(player.Body);
                    if (player.Descriptor.IsPet && !allowed_slots.Contains(slot.Type))
                    {
                        if (!slot.Slot.Lock)
                        {
                            slot.Slot.Lock.Retain();
                        }
                        if (!slot.HasItem)
                        {
                            slot.ItemImage.color = new UnityEngine.Color(1.0f, 1.0f, 1.0f, 0.3f);
                        }
                    }
                }
                if (tr.Property("Room").GetValue() != null)
                    tr.Property("Room").Method("SetupInfo", player).GetValue();
                if (tr.Property("DollWeaponSets").GetValue() == null)
                {
                    return false;
                }

                if (player.Descriptor.IsPet)
                {//remove additional weapon sets
                    for (int i = 1; i < player.Body.HandsEquipmentSets.Count; i++)
                    {
                        player.Body.HandsEquipmentSets[i] = player.Body.HandsEquipmentSets[0];
                    }
                }

                tr.Field("m_DollWeaponSets").Method("SetupInfo", player.Body).GetValue();

                return false;
            }
        }
    }
}
