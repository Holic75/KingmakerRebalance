using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{

        public enum ActivatableAbilityGroupExtension
        {
            None,
            AnimalFocus,
            SacredWeaponEnchantment,
            SacredArmorEnchantment,
            MetaRage,
        }

        public static class Extensions
        {
            public static ActivatableAbilityGroup ToActivatableAbilityGroup(this ActivatableAbilityGroupExtension group)
            {
                int value = EnumUtils.GetMaxValue<ActivatableAbilityGroup>() + (int)group;
                return (ActivatableAbilityGroup)value;
            }
        }

        /*[Harmony12.HarmonyPatch(typeof(UnitPartActivatableAbility))]
        [Harmony12.HarmonyPatch("PersistentGroupsSizeIncreases", Harmony12.MethodType.Setter)]
        class UnitPartActivatableAbility__PersistentGroupsSizeIncreases__Setter__Patch
        {
            static void Postfix(UnitPartActivatableAbility __instance, int[] value)
            {
                var tr = Harmony12.Traverse.Create(__instance);
                int[] current_group_sizes = tr.Field("m_GroupsSizeIncreases").GetValue<int[]>();
                if (value.Length > current_group_sizes.Length)
                {
                    tr.Field("m_GroupsSizeIncreases").SetValue(value);
                    Main.logger.Log(tr.Field("m_GroupsSizeIncreases").GetValue<int[]>().Length.ToString());
                }

            }
        }*/


        [Harmony12.HarmonyPatch(typeof(UnitPartActivatableAbility))]
        [Harmony12.HarmonyPatch("ctor", Harmony12.MethodType.Constructor)]
        class UnitPartActivatableAbility__Constructor__Patch
        {
            static void increaseGroupSizeIfNeeded(UnitPartActivatableAbility part)
            {
                var tr = Harmony12.Traverse.Create(part);
                int max_length = EnumUtils.GetMaxValue<ActivatableAbilityGroupExtension>() + EnumUtils.GetMaxValue<ActivatableAbilityGroup>();

                int[] current_group_sizes = tr.Field("m_GroupsSizeIncreases").GetValue<int[]>();
                if (current_group_sizes.Length < max_length)
                {
                    current_group_sizes = current_group_sizes.AddToArray(Enumerable.Repeat<int>(0, max_length - current_group_sizes.Length).ToArray());
                    tr.Field("m_GroupsSizeIncreases").SetValue(current_group_sizes);
                }
            }

            static void Postfix(UnitPartActivatableAbility __instance)
            {
                var tr = Harmony12.Traverse.Create(__instance);
                increaseGroupSizeIfNeeded(__instance);
                //Main.logger.Log("UnitPartActivatableAbility Constructor Patch, array size increased : " + tr.Field("m_GroupsSizeIncreases").GetValue<int[]>().Length.ToString());
            }
        }
}
