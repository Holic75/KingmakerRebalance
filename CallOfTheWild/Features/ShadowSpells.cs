using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.ShadowSpells
{
    public class UnitPartDisbelief : UnitPart, IUnitCombatHandler, IGlobalSubscriber
    {
        public Dictionary<MechanicsContext, bool> disbelief_contexts = new Dictionary<MechanicsContext, bool>();

        public void HandleUnitJoinCombat(UnitEntityData unit)
        {
            /*if (unit.Descriptor == this.Owner)
            {
                disbelief_contexts.Clear();
            }*/
        }

        public void HandleUnitLeaveCombat(UnitEntityData unit)
        {
            if (unit.Descriptor == this.Owner)
            {
                Main.logger.Log("Clearing disbelief part for " + this.Owner.CharacterName);
                disbelief_contexts.Clear();
            }
        }
    }

    class DisbeliefSpell : BlueprintComponent
    {

    }


    public class ResilentSpells : OwnedGameLogicComponent<UnitDescriptor>, MetamagicFeats.IRuleSavingThrowTriggered
    {
        public SpellSchool school = SpellSchool.Illusion;
        public SavingThrowType save_type = SavingThrowType.Will;

        public void ruleSavingThrowTriggered(RuleSavingThrow evt)
        {
            var context = evt.Reason?.Context;
            if (context == null)
            {
                return;
            }

            var caster = context.MaybeCaster;
            if (caster == null)
            {
                return;
            }

            if (caster != this.Owner.Unit)
            {
                return;
            }

            if (context.SpellSchool == school
                 && (save_type == SavingThrowType.Unknown || evt.Type == save_type)
                 && (context?.SourceAbility.GetComponent<DisbeliefSpell>() != null
                     || (context.SpellDescriptor.Intersects((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow)
                         && !evt.Initiator.Ensure<UnitPartDisbelief>().disbelief_contexts.ContainsKey(context)
                         )
                    )
                )
            {
                var cl_check = RulebookEvent.Dice.D(new DiceFormula(1, DiceType.D20)) + evt.Reason.Context.Params.CasterLevel;
                if (cl_check > evt.DifficultyClass)
                {
                    Common.AddBattleLogMessage("Changing spell DC for " + evt.Initiator.CharacterName + $" form {evt.DifficultyClass} to {cl_check} due to {this.Fact.Name}");
                    Helpers.SetField(evt, "DifficultyClass", cl_check);
                }
            }
        }
    }

    namespace Patches
    {
        [Harmony12.HarmonyPatch(typeof(RuleSpellResistanceCheck))]
        [Harmony12.HarmonyPatch("HasResistanceRoll", Harmony12.MethodType.Getter)]
        class Patch_RuleSpellResistanceCheck_HasResistanceRoll_Prefix
        {
            //static public Dictionary<(MechanicsContext, UnitEntityData), bool> passed_disbilief_spell_target_map = new Dictionary<(MechanicsContext, UnitEntityData), bool>();
            static public bool Prefix(RuleSpellResistanceCheck __instance, ref bool __result)
            {
                if (__instance.Target == null || __instance.Context == null)
                {
                    return true;
                }
                if (__instance.Context.SpellDescriptor.Intersects((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow) 
                     &&  !__instance.Target.Ensure<UnitPartDisbelief>().disbelief_contexts.ContainsKey(__instance.Context))
                {
                    RuleSavingThrow ruleSavingThrow = __instance.Context.TriggerRule<RuleSavingThrow>(new RuleSavingThrow(__instance.Target, SavingThrowType.Will, __instance.Context.Params.DC));
                    __instance.Target.Ensure<UnitPartDisbelief>().disbelief_contexts[__instance.Context] = ruleSavingThrow.IsPassed;
                }
                return true;
            }
        }


        [Harmony12.HarmonyPatch(typeof(AreaEffectEntityData))]
        [Harmony12.HarmonyPatch("TryOvercomeSpellResistance", Harmony12.MethodType.Normal)]
        class Patch_AreaEffectEntityData_TryOvercomeSpellResistance_Prefix
        {
            static public bool Prefix(AreaEffectEntityData __instance, UnitEntityData unit, ref bool __result)
            {
                if (unit == null || __instance.Context == null)
                {
                    return true;
                }

                if (__instance.Context.SpellDescriptor.Intersects((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow)
                     && !unit.Ensure<UnitPartDisbelief>().disbelief_contexts.ContainsKey(__instance.Context))
                {
                    RuleSavingThrow ruleSavingThrow = __instance.Context.TriggerRule<RuleSavingThrow>(new RuleSavingThrow(unit, SavingThrowType.Will, __instance.Context.Params.DC));
                    unit.Ensure<UnitPartDisbelief>().disbelief_contexts[__instance.Context] = ruleSavingThrow.IsPassed;
                }
                return true;
            }
        }


        /*[Harmony12.HarmonyPatch(typeof(UnitEntityData))]
        [Harmony12.HarmonyPatch("LeaveCombat", Harmony12.MethodType.Normal)]
        class UnitEntityData__LeaveCombat__Patch
        {
            static void Postfix(UnitEntityData __instance)
            {
                var keys = Patch_RuleSpellResistanceCheck_HasResistanceRoll_Prefix.passed_disbilief_spell_target_map.Keys.ToArray();
                foreach (var k in keys)
                {
                    if (k.Item1?.MaybeCaster == __instance
                        || k.Item2 == __instance)
                    {
                        Patch_RuleSpellResistanceCheck_HasResistanceRoll_Prefix.passed_disbilief_spell_target_map.Remove(k);
                    }
                }
            }
        }*/

        //check damage
        [Harmony12.HarmonyPatch(typeof(RuleDealDamage))]
        [Harmony12.HarmonyPatch("OnTrigger", Harmony12.MethodType.Normal)]
        class Patch_RuleDealDamage_OnTrigger_Prefix
        {
            static public bool Prefix(RuleDealDamage __instance, RulebookEventContext context)
            {
                var context2 = __instance.Reason.Context;
                if (context2 == null || __instance.Target == null)
                {
                    return true;
                }

                if (!__instance.Target.Ensure<UnitPartDisbelief>().disbelief_contexts.ContainsKey(context2))
                {
                    return true;
                }

                if (__instance.Target.Ensure<UnitPartDisbelief>().disbelief_contexts[context2])
                {
                    if (context2.SpellDescriptor.Intersects((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow20))
                    {
                        __instance.ReducedBecauseOfShadowEvocation = true;
                    }
                    else if(context2.SpellDescriptor.Intersects((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow60))
                    {
                        __instance.ReducedBecauseOfShadowEvocationGreater = true;
                    }
                }

                return true;
            }
        }

        //check if buffs will be applied
        [Harmony12.HarmonyPatch(typeof(RuleApplyBuff))]
        [Harmony12.HarmonyPatch("OnTrigger", Harmony12.MethodType.Normal)]
        class Patch_RuleApplyBuff_OnTrigger_Prefix
        {
            static public bool Prefix(RuleApplyBuff __instance, RulebookEventContext context)
            {
                var context2 = __instance.Reason.Context;
                if (context2 == null || __instance.Initiator == null)
                {
                    return true;
                }

                if (!__instance.Initiator.Ensure<UnitPartDisbelief>().disbelief_contexts.ContainsKey(context2))
                {
                    return true;
                }

                if (__instance.Initiator.Ensure<UnitPartDisbelief>().disbelief_contexts[context2])
                {
                    if (context2.SpellDescriptor.Intersects((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow20))
                    {
                        if (RulebookEvent.Dice.D(new DiceFormula(1, DiceType.D100)) > 20)
                        {
                            __instance.CanApply = false;
                            Common.AddBattleLogMessage(__instance.Initiator.CharacterName + " avoids " + context2.SourceAbility.Name + " effect due to disbelief");
                            return false;
                        }
                    }
                    else if (context2.SpellDescriptor.Intersects((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow60))
                    {
                        if (RulebookEvent.Dice.D(new DiceFormula(1, DiceType.D100)) > 60)
                        {
                            __instance.CanApply = false;
                            Common.AddBattleLogMessage(__instance.Initiator.CharacterName + " avoids " + context2.SourceAbility.Name + " effect due to disbelief");
                            return false;
                        }
                    }
                }

                return true;
            }
        }
    }
}
