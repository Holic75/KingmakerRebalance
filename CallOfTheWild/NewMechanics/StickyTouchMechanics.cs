using Kingmaker.Blueprints;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.StickyTouchMechnics
{
    public class UnitPartTouchMultipleCharges : UnitPart
    {
        private int num_charges = 0;

        public void init(int charges)
        {
            num_charges = charges;
        }

        public void consume()
        {
            num_charges--;
        }

        public bool hasCharges()
        {
            return num_charges > 0;
        }

        public override void OnRemove()
        {
            num_charges = 0;
        }

        public int getNumCharges()
        {
            return num_charges;
        }
    }


    public class AbilityEffectStickyTouchMultiple : BlueprintComponent
    {
        public ContextValue num_charges;
    }


    [Harmony12.HarmonyPatch(typeof(TouchSpellsController))]
    [Harmony12.HarmonyPatch("OnAbilityEffectApplied", Harmony12.MethodType.Normal)]
    class TouchSpellsController__OnAbilityEffectApplied__Patch
    {
        static bool Prefix(TouchSpellsController __instance, AbilityExecutionContext context)
        {
            UnitEntityData maybeCaster = context.MaybeCaster;
            if (maybeCaster == null)
                return false;
            UnitPartTouch unitPartTouch = maybeCaster.Get<UnitPartTouch>();
            if (!(bool)(unitPartTouch) || (unitPartTouch.Ability.Data != context.Ability))
                return false;

            var charges_part = maybeCaster.Get<UnitPartTouchMultipleCharges>();

            if (charges_part != null && charges_part.hasCharges())
            {
                charges_part.consume();
                if (!charges_part.hasCharges())
                {
                    maybeCaster.Remove<UnitPartTouch>();
                }
            }
            else
            {
                maybeCaster.Remove<UnitPartTouch>();
            }
            if (maybeCaster.IsAutoUseAbility(context.Ability))
                return false;
            maybeCaster.CombatState.ManualTarget = null;
            return false;
        }
    }

    [Harmony12.HarmonyPatch(typeof(UnitPartTouch))]
    [Harmony12.HarmonyPatch("OnRemove", Harmony12.MethodType.Normal)]
    class UnitPartTouch__onRemove__Patch
    {
        static void Postfix(UnitPartTouch __instance)
        {
            var charges_part = __instance.Owner.Get<UnitPartTouchMultipleCharges>();
            if (charges_part == null)
            {
                return;
            }
            __instance.Owner.Remove<UnitPartTouchMultipleCharges>();
        }
    }


    [Harmony12.HarmonyPatch(typeof(UnitPartTouch))]
    [Harmony12.HarmonyPatch("Init", Harmony12.MethodType.Normal)]
    class UnitPartTouch__Init__Patch
    {
        static void Postfix(UnitPartTouch __instance, BlueprintAbility ability, AbilityData source, AbilityExecutionContext context)
        {
            var multiple_charges = source.Blueprint.GetComponent<AbilityEffectStickyTouchMultiple>();
            if (multiple_charges != null)
            {
                int charges = multiple_charges.num_charges.Calculate(context);
                __instance.Owner.Ensure<UnitPartTouchMultipleCharges>().init(charges);
            }
        }
    }

    //fix spellstrike to work on spells with multiple variants
    [Harmony12.HarmonyPatch(typeof(UnitPartMagus))]
    [Harmony12.HarmonyPatch("IsSpellFromMagusSpellList", Harmony12.MethodType.Normal)]
    class UnitPartMagus__IsSpellFromMagusSpellList__Init__Patch
    {
        static void Postfix(UnitPartMagus __instance, AbilityData spell, ref bool __result)
        {
            var spell1 = spell.StickyTouch != null ? spell.StickyTouch : spell;
            if (__result == false && spell1?.Blueprint?.Parent != null)
            {
                var blueprint = spell1.Blueprint.Parent;
                __result = blueprint.IsInSpellList(__instance.Spellbook.Blueprint.SpellList) || __instance.Spellbook.IsKnown(blueprint);
            }
        }
    }

}
