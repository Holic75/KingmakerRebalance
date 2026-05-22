using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Common;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UI.Tooltip;
using Kingmaker.UnitLogic;
using Kingmaker.Visual.CharacterSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace CallOfTheWild.ArmorFix
{
    class Helpers
    {

        public static bool isBodyArmor(BlueprintItemArmor armor)
        {
            return armor.ProficiencyGroup != ArmorProficiencyGroup.Buckler
                   && armor.ProficiencyGroup != ArmorProficiencyGroup.HeavyShield
                   && armor.ProficiencyGroup != ArmorProficiencyGroup.LightShield
                   && armor.ProficiencyGroup != ArmorProficiencyGroup.TowerShield;
        }
    }

    [Harmony12.HarmonyPatch(typeof(ItemEntityArmor))]
    [Harmony12.HarmonyPatch("CanBeEquippedInternal", Harmony12.MethodType.Normal)]
    class ItemEntityArmor__CanBeEquippedInternal__Patch
    {
        static bool Prefix(ItemEntityArmor __instance, UnitDescriptor owner, ref bool __result)
        {
            BlueprintItemEquipment blueprint = __instance.Blueprint as BlueprintItemEquipment;
            if (blueprint == null || !Helpers.isBodyArmor(__instance.Blueprint))
                return true;
            __result =  blueprint.CanBeEquippedBy(owner);
            return false;
        }
    }

    // highlight in grey armors that can be equipped but the character does not have proficiency with
    [Harmony12.HarmonyPatch(typeof(ItemSlot))]
    [Harmony12.HarmonyPatch("SetupEquipPossibility")]
    class ItemSlot__SetupEquipPossibility__Patch
    {
        static void Postfix(ItemSlot __instance)
        {
            var can_equip = CallOfTheWild.Helpers.GetField<Transform>(__instance, "m_CanEquip");
            if (can_equip == null)
            {
                return;
            }
            can_equip.GetComponent<UnityEngine.UI.Image>().color = new Color(0.9f, 0.3f, 0.1f, 0.4f);
            if (!__instance.HasItem)
            {
                can_equip.gameObject.SetActive(false);
                return;
            }
            var character = UIUtility.GetCurrentCharacter()?.Descriptor;
            if (character == null)
            {
                can_equip.gameObject.SetActive(false);
                return;
            }
            var item = __instance.Item as ItemEntityArmor;
            if (item == null || item.Blueprint == null || !Helpers.isBodyArmor(item.Blueprint)) return;
            if (item.CanBeEquippedBy(character) && !character.Proficiencies.Contains(item.Blueprint.ProficiencyGroup))
            {
                can_equip.GetComponent<UnityEngine.UI.Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.4f);
                can_equip.gameObject.SetActive(true);
            }
        }
    }

    // update 'Can be equipped' text in the armor tooltip, in case of missing proficiency
    class UIUtilityTexts__GetCanDoText__Patch
    {
        internal static void Run()
        {
            var original = Harmony12.AccessTools.Method(typeof(UIUtilityTexts), "GetCanDoText");
            var patch = Harmony12.AccessTools.Method(typeof(UIUtilityTexts__GetCanDoText__Patch), "Postfix");
            Main.harmony.Patch(original, postfix: new Harmony12.HarmonyMethod(patch));
        }

        static void Postfix(TooltipData data, ref string __result)
        {
            var item = data.Item as ItemEntityArmor;
            var character = UIUtility.GetCurrentCharacter();
            if (item == null || character == null)
            {
                return;
            }
            if (item.Owner != null)
            {
                return;
            }
            if (item.Blueprint == null || !Helpers.isBodyArmor(item.Blueprint)) return;

            if (item.CanBeEquippedBy(character.Descriptor) && !character.Descriptor.Proficiencies.Contains(item.Blueprint.ProficiencyGroup))
            {
                __result = Harmony12.Traverse.Create(typeof(UIUtilityTexts)).Method("GetEquipPosibilityString", "Can be equipped, but armor check penalty will be applied to attack rolls", BlueprintRoot.Instance.UIRoot.TooltipColors.Default).GetValue<string>();
            }
        }
    }


    [Harmony12.HarmonyPatch(typeof(ItemEntityArmor))]
    [Harmony12.HarmonyPatch("RecalculateStats", Harmony12.MethodType.Normal)]
    class ItemEntityArmor__RecalculateStats__Patch
    {
        static void Postfix(ItemEntityArmor __instance)
        {
            if (__instance?.Wielder == null || __instance?.Owner == null || __instance?.Blueprint == null)
            {
                return;
            }

            if (__instance.Owner.Proficiencies.Contains(__instance.Blueprint.ProficiencyGroup) 
                || !Game.Instance.Player.PartyCharacters.Any(c => c.Value == __instance?.Wielder?.Unit || c.Value?.Descriptor?.Pet == __instance?.Wielder?.Unit)) 
                //a lot of units in the  game do not have required proficiencies, so we will consider only party members and pets
            {
                return;
            }

            var tr = Harmony12.Traverse.Create(__instance);
            int penalty = Rulebook.Trigger<RuleCalculateArmorCheckPenalty>(new RuleCalculateArmorCheckPenalty(__instance.Wielder.Unit, __instance)).Penalty;
            if (penalty < 0)
            {
                tr.Method("AddModifier", (ModifiableValue)__instance.Wielder.Stats.AdditionalAttackBonus, penalty).GetValue();
            }
        }
    }




}
