using Kingmaker.Blueprints;
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
                Main.TraceLog();
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
            static EquipSlotBase.SlotType[] allowed_slots_serpentine = new EquipSlotBase.SlotType[] { EquipSlotBase.SlotType.Belt, EquipSlotBase.SlotType.Neck, EquipSlotBase.SlotType.Wrist, EquipSlotBase.SlotType.Shoulders, EquipSlotBase.SlotType.Head, EquipSlotBase.SlotType.Ring1, EquipSlotBase.SlotType.Ring2, EquipSlotBase.SlotType.Gloves,
                                                                                                  EquipSlotBase.SlotType.QuickSlot1,  EquipSlotBase.SlotType.QuickSlot2, EquipSlotBase.SlotType.QuickSlot3, EquipSlotBase.SlotType.QuickSlot4, EquipSlotBase.SlotType.QuickSlot5};
            static EquipSlotBase.SlotType[] allowed_slots_animal = new EquipSlotBase.SlotType[] { EquipSlotBase.SlotType.Belt, EquipSlotBase.SlotType.Neck, EquipSlotBase.SlotType.Wrist, EquipSlotBase.SlotType.Shoulders, EquipSlotBase.SlotType.Head };
            static EquipSlotBase.SlotType[] allowed_slots_eidolon = new EquipSlotBase.SlotType[] { EquipSlotBase.SlotType.PrimaryHand, EquipSlotBase.SlotType.SecondaryHand,
                                                                                                   EquipSlotBase.SlotType.Head, EquipSlotBase.SlotType.Feet, EquipSlotBase.SlotType.Ring1, EquipSlotBase.SlotType.Ring2, EquipSlotBase.SlotType.Gloves, EquipSlotBase.SlotType.Belt, EquipSlotBase.SlotType.Neck, EquipSlotBase.SlotType.Wrist, EquipSlotBase.SlotType.Shoulders,
                                                                                                   EquipSlotBase.SlotType.QuickSlot1,  EquipSlotBase.SlotType.QuickSlot2, EquipSlotBase.SlotType.QuickSlot3, EquipSlotBase.SlotType.QuickSlot4, EquipSlotBase.SlotType.QuickSlot5};
            static EquipSlotBase.SlotType[] allowed_slots_phantom = new EquipSlotBase.SlotType[] { EquipSlotBase.SlotType.PrimaryHand, EquipSlotBase.SlotType.SecondaryHand,
                                                                                                   EquipSlotBase.SlotType.Head, EquipSlotBase.SlotType.Feet, EquipSlotBase.SlotType.Ring1, EquipSlotBase.SlotType.Ring2, EquipSlotBase.SlotType.Gloves, EquipSlotBase.SlotType.Belt, EquipSlotBase.SlotType.Neck, EquipSlotBase.SlotType.Wrist, EquipSlotBase.SlotType.Shoulders,
                                                                                                   EquipSlotBase.SlotType.QuickSlot1,  EquipSlotBase.SlotType.QuickSlot2, EquipSlotBase.SlotType.QuickSlot3, EquipSlotBase.SlotType.QuickSlot4, EquipSlotBase.SlotType.QuickSlot5};


            static bool Prefix(CharDollBase __instance, UnitEntityData player)
            {
                Main.TraceLog();
                var tr = Harmony12.Traverse.Create(__instance);
                tr.Property("CurrentUnit").SetValue(player);
                if (__instance.CurrentUnit == null)
                    return false;

                bool is_phantom = player.Descriptor.Progression.GetClassLevel(Phantom.phantom_class) > 0;
                bool is_eidolon = player.Descriptor.Progression.GetClassLevel(Eidolon.eidolon_class) > 0;
                bool is_serpentine = player.Descriptor.Progression.IsArchetype(Eidolon.serpentine_archetype);
                bool is_biped = (is_eidolon
                                 && !player.Descriptor.Progression.IsArchetype(Eidolon.quadruped_archetype)
                                 && !is_serpentine)
                                 || is_phantom;
                is_biped = is_biped || player.Blueprint.GetComponent<Eidolon.CorpseCompanionComponent>() != null;
                EquipSlotBase.SlotType[] allowed_slots;
                if (is_serpentine)
                {
                    allowed_slots = allowed_slots_serpentine;
                }
                else if (is_phantom)
                {
                    allowed_slots = allowed_slots_phantom;
                }
                else if (!is_biped)
                {
                    allowed_slots = allowed_slots_animal;
                }
                else
                {
                    allowed_slots = allowed_slots_eidolon;
                }

                bool no_slot_limitation = player.Descriptor.Progression.IsArchetype(Archetypes.UndeadLord.corpse_companion_archetype);

                foreach (EquipSlotBase slot in tr.Property("SlotList").GetValue<List<EquipSlotBase>>())
                {
                    slot.Clear();
                    slot.SetupInfo(player.Body);
                    if (player.Descriptor.IsPet && !no_slot_limitation)
                    {
                        if (!allowed_slots.Contains(slot.Type))
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
                        else
                        {
                            if (slot.Slot.Lock)
                            {
                                slot.Slot.Lock.Release();
                            }
                        }
                    }                                      
                }
                var room = tr.Property("Room").GetValue<DollRoom>();
                if (room != null)
                    room.SetupInfo(player);

                var doll_weapon_sets = Helpers.GetField<DollWeaponSets>(__instance, "m_DollWeaponSets");
                if (doll_weapon_sets == null)
                {
                    return false;
                }

                if (player.Descriptor.IsPet && (!is_biped))
                {//remove additional weapon sets
                    for (int i = 1; i < player.Body.HandsEquipmentSets.Count; i++)
                    {
                        if (player.Body.HandsEquipmentSets[i].PrimaryHand?.MaybeItem != player.Body.HandsEquipmentSets[0].PrimaryHand?.MaybeItem
                            || player.Body.HandsEquipmentSets[i].SecondaryHand?.MaybeItem != player.Body.HandsEquipmentSets[0].SecondaryHand?.MaybeItem)
                        {
                            player.Body.HandsEquipmentSets[i] = player.Body.HandsEquipmentSets[0].CloneObject();
                        }

                    }
                }

                doll_weapon_sets.SetupInfo(player);

                return false;
            }
        }
    }
}
