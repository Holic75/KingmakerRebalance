using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.AffectedBySpellMechanics
{

    public class UnitPartAffectedBySpell : UnitPart, IUnitCombatHandler, IGlobalSubscriber
    {
        public HashSet<MechanicsContext> contexts = new HashSet<MechanicsContext>();

        public void HandleUnitJoinCombat(UnitEntityData unit)
        {

        }

        public void HandleUnitLeaveCombat(UnitEntityData unit)
        {
            if (unit.Descriptor == this.Owner)
            {
                Main.logger.Log("Clearing affected by spells part for " + this.Owner.CharacterName);
                contexts.Clear();
            }
        }
    }

    //fix sneak attack to trigger only on first ray (generally one sneak attack per context)
    [Harmony12.HarmonyPatch(typeof(RulePrepareDamage))]
    [Harmony12.HarmonyPatch("OnTrigger", Harmony12.MethodType.Normal)]
    class RulePrepareDamage_OnTrigger
    {
        //static public Dictionary<(MechanicsContext, UnitEntityData), bool> spell_target_map = new Dictionary<(MechanicsContext, UnitEntityData), bool>();
        static bool Prefix(RulePrepareDamage __instance, RulebookEventContext context)
        {
            if (!Main.settings.one_sneak_attack_per_target_per_spell)
            {
                return true;
            }
            AbilityData ability = __instance.ParentRule?.Reason?.Ability;

            if (ability == null || __instance.Target == null)
            {
                return true;
            }

            var context2 = __instance.ParentRule?.Reason?.Context;
            if (context2 == null)
            {
                return true;
            }

            var unit_part_affected_by_spell = __instance.Target.Ensure<UnitPartAffectedBySpell>();
            if (!unit_part_affected_by_spell.contexts.Contains(context2))
            {
                unit_part_affected_by_spell.contexts.Add(context2);
                return true;
            }

            __instance.IsSurpriseSpell = false;
            __instance.ParentRule.AttackRoll?.UseSneakAttack();
            return true;
        }
    }

    /*[Harmony12.HarmonyPatch(typeof(UnitEntityData))]
    [Harmony12.HarmonyPatch("LeaveCombat", Harmony12.MethodType.Normal)]
    class UnitEntityData__LeaveCombat__Patch
    {
        static void Postfix(UnitEntityData __instance)
        {
            var keys = RulePrepareDamage_OnTrigger.spell_target_map.Keys.ToArray();
            foreach (var k in keys)
            {
                if (k.Item1?.MaybeCaster == __instance
                    || k.Item2 == __instance)
                {
                    RulePrepareDamage_OnTrigger.spell_target_map.Remove(k);
                }
            }
        }
    }*/
}
