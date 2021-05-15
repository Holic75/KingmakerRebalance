using Kingmaker;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
