using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UI.Group;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.EncumbranceHelper;

namespace CallOfTheWild.MechanicsFixes
{
    public class NaturalArmorFix
    {
        public const ModifierDescriptor NATURAL_ARMOR_BONUS = (ModifierDescriptor)45;

        static internal void run()
        {
            fixDescriptorComparer();
            fixFeatures();
        }
        static void fixFeatures()
        {
            var lizard_familiar = Main.library.Get<BlueprintFeature>("61aeb92c176193e48b0c9c50294ab290");
            lizard_familiar.GetComponents<AddStatBonus>().Where(c => c.Descriptor == ModifierDescriptor.NaturalArmor).First().Descriptor = NaturalArmorFix.NATURAL_ARMOR_BONUS;

            // fix bloodlines
            var nat_ac_bonuses = Main.library.GetAllBlueprints().Where
                (b => 
                    (b is BlueprintFeature) 
                    && b.GetComponents<AddStatBonus>().Any(c => c.Descriptor == ModifierDescriptor.NaturalArmor || c.Descriptor == ModifierDescriptor.NaturalArmorEnhancement)
                    && b.name.StartsWith("Bloodline")
                ).Cast<BlueprintFeature>().ToArray();

            foreach (var nat_ac_bonus in nat_ac_bonuses)
            {
                foreach (var c in nat_ac_bonus.GetComponents<AddStatBonus>().Where(c => c.Descriptor == ModifierDescriptor.NaturalArmor || c.Descriptor == ModifierDescriptor.NaturalArmorEnhancement)) {
                    c.Descriptor = NATURAL_ARMOR_BONUS;
                }
                // Main.logger.Log($"Fixed natural armor bonus descriptor for  {nat_ac_bonus.name}");
            }
        }

        static void fixDescriptorComparer()
        {
            ModifierDescriptorComparer.SortedValues = ModifierDescriptorComparer.SortedValues.AddToArray(NaturalArmorFix.NATURAL_ARMOR_BONUS);
            var m_order = Helpers.GetField<int[]>(ModifierDescriptorComparer.Instance, "m_Order").AddToArray(-1);
            m_order[m_order.Count() - 2] = (int)NaturalArmorFix.NATURAL_ARMOR_BONUS + 1;
            Helpers.SetField(ModifierDescriptorComparer.Instance, "m_Order", m_order);

            /*Main.logger.Log($"Lengths: {ModifierDescriptorComparer.SortedValues.Length} / {m_order.Count()}");
            foreach (var sv in ModifierDescriptorComparer.SortedValues)
            {
                Main.logger.Log($"sv {sv}: order {m_order[(int)sv]}");
            }*/
        }
    }


    [Harmony12.HarmonyPatch(typeof(AbilityModifiersStrings))]
    [Harmony12.HarmonyPatch("GetName", Harmony12.MethodType.Normal)]
    class AbilityModifiersStrings__GetName__Patch
    {
        static bool Prefix(AbilityModifiersStrings __instance, ModifierDescriptor descriptor, ref string __result)
        {
            if (descriptor == NaturalArmorFix.NATURAL_ARMOR_BONUS)
            {
                __result = "Natural armor bonus";
                return false;
            }
            return true;
        }
    }

    [Harmony12.HarmonyPatch(typeof(AbilityModifiersStrings))]
    [Harmony12.HarmonyPatch("GetDescription", Harmony12.MethodType.Normal)]
    class AbilityModifiersStrings__GetDescription__Patch
    {
        static bool Prefix(AbilityModifiersStrings __instance, ModifierDescriptor descriptor, ref string __result)
        {
            if (descriptor == NaturalArmorFix.NATURAL_ARMOR_BONUS)
            {
                __result = "Natural armor bonus";
                return false;
            }
            return true;
        }
    }


    [Harmony12.HarmonyPatch(typeof(ModifiableValueArmorClass))]
    [Harmony12.HarmonyPatch("IsArmor", Harmony12.MethodType.Normal)]
    class ModifiableValueArmorClass__IsArmor__Patch
    {
        static void Postfix(ModifiableValue.Modifier m, ref bool __result)
        {
            if (m.ModDescriptor == NaturalArmorFix.NATURAL_ARMOR_BONUS)
            {
                __result = true;
            }
        }
    }


    // make polymorph apply natural armor bonus descriptor instead of natural armor one
    [Harmony12.HarmonyPatch(typeof(Polymorph))]
    [Harmony12.HarmonyPatch("OnFactActivate", Harmony12.MethodType.Normal)]
    class Polymorph_OnFactActivate__Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var apply_natural_armor = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Ldc_I4_S && x.operand.ToString() == "16");

            codes[apply_natural_armor].operand = (sbyte)NaturalArmorFix.NATURAL_ARMOR_BONUS;

            return codes.AsEnumerable();
        }
    }
}
