using Kingmaker.Blueprints;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.ConcentrationMechanics
{
    class UnitPartDoNoProvekeAooOnSpellCast : AdditiveUnitPart
    {
        public bool active()
        {
            return !buffs.Empty();
        }
    }

    public class DoNotProvokeAooOnSpellCast : OwnedGameLogicComponent<UnitDescriptor>
    {
        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartDoNoProvekeAooOnSpellCast>().addBuff(this.Fact);

        }

        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartDoNoProvekeAooOnSpellCast>().removeBuff(this.Fact);
        }
    }

    [Harmony12.HarmonyPatch(typeof(UnitUseAbility))]
    [Harmony12.HarmonyPatch("TryCastingDefensively", Harmony12.MethodType.Normal)]
    class UnitUseAbility__TryCastingDefensively__Patch
    {
        static bool Prefix(UnitUseAbility __instance, ref bool __result)
        {
            if ((__instance?.Spell?.Caster?.Get<UnitPartDoNoProvekeAooOnSpellCast>()?.active()).GetValueOrDefault())
            {
                __result = true;
                return false;
            }

            return true;
        }
    }



    //fix concentration checks for kineticist
    [Harmony12.HarmonyPatch(typeof(RuleCheckConcentration))]
    [Harmony12.HarmonyPatch("OnTrigger", Harmony12.MethodType.Normal)]
    class RuleCheckConcentration__OnTrigger__Patch
    {
        static bool Prefix(RuleCheckConcentration __instance, RulebookEventContext context)
        {
            if (__instance.Spell.Blueprint.GetComponent<AbilityKineticist>() == null)
            {
                return true;
            }
            var kineticist_part = __instance.Initiator.Get<UnitPartKineticist>();
            if (kineticist_part == null)
            {
                return true;
            }

            var tr = Harmony12.Traverse.Create(__instance);
            var rule = Rulebook.Trigger<RuleCalculateAbilityParams>(new RuleCalculateAbilityParams(__instance.Initiator, __instance.Spell));
            var ability_params = rule.Result;

            tr.Property("DC").SetValue(__instance.Damage == null ? 15 + ability_params.SpellLevel : 10 + ability_params.SpellLevel + __instance.Damage.Damage / 2);

            var bonus_concentration = Helpers.GetField<int>(rule, "m_BonusConcentration");
            tr.Property("Concentration").SetValue(bonus_concentration + kineticist_part.ClassLevel + kineticist_part.MainStatBonus);
            tr.Property("ResultRollRaw").SetValue(RulebookEvent.Dice.D20);
            return false;
        }
    }


    //fix concentration checks for kineticist
    [Harmony12.HarmonyPatch(typeof(RuleCheckCastingDefensively))]
    [Harmony12.HarmonyPatch("OnTrigger", Harmony12.MethodType.Normal)]
    class RuleCheckCastingDefensively__OnTrigger__Patch
    {
        static bool Prefix(RuleCheckCastingDefensively __instance, RulebookEventContext context)
        {
            if (__instance.Spell.Blueprint.GetComponent<AbilityKineticist>() == null)
            {
                return true;
            }
            var kineticist_part = __instance.Initiator.Get<UnitPartKineticist>();
            if (kineticist_part == null)
            {
                return true;
            }

            var tr = Harmony12.Traverse.Create(__instance);
            var rule = Rulebook.Trigger<RuleCalculateAbilityParams>(new RuleCalculateAbilityParams(__instance.Initiator, __instance.Spell));
            var ability_params = rule.Result;

            tr.Property("DC").SetValue(15 + ability_params.SpellLevel * 2);

            var bonus_concentration = Helpers.GetField<int>(rule, "m_BonusConcentration");
            tr.Property("Concentration").SetValue(bonus_concentration + kineticist_part.ClassLevel + kineticist_part.MainStatBonus);
            tr.Property("ResultRollRaw").SetValue(RulebookEvent.Dice.D20);
            return false;
        }
    }
}
