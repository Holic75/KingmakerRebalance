using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Combat;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.AooMechanics
{
    public class UnitPartAooAgainstAllies : AdditiveUnitPart
    {
        public bool active()
        {
            return !buffs.Empty();
        }
    }

    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AooAgainstAllies : OwnedGameLogicComponent<UnitDescriptor>
    {
        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartAooAgainstAllies>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartAooAgainstAllies>().removeBuff(this.Fact);
        }
    }



    public class UnitPartSpecialAttackOfOportunity : AdditiveUnitPart
    {
        public bool canMakeAoo(UnitEntityData enemy)
        {
            foreach (var b in buffs)
            {
                bool can_make_aoo = false;
                b.CallComponents<IAooSpecial>(c => can_make_aoo = c.canMakeAoo(enemy));
                if (can_make_aoo)
                {
                    return true;
                }
            }

            return false;
        }
    }


    interface IAooSpecial
    {
        bool canMakeAoo(UnitEntityData enemy);
    }



    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AooWithRangedWeapon : OwnedGameLogicComponent<UnitDescriptor>, IAooSpecial, IUnitSubscriber
    {
        public BlueprintParametrizedFeature required_feature;
        public WeaponCategory[] weapon_categories;
        public Feet max_range = 3.Feet();

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartSpecialAttackOfOportunity>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartSpecialAttackOfOportunity>().removeBuff(this.Fact);
        }

        public bool canMakeAoo(UnitEntityData enemy)
        {
            var weapon = this.Owner.Body?.PrimaryHand?.MaybeWeapon;
            if (weapon == null)
            {
                return false;
            }

            if (!weapon.Blueprint.IsRanged)
            {
                return false;
            }

            if (!weapon_categories.Contains(weapon.Blueprint.Category))
            {
                return false;
            }

            if (required_feature != null
                && !this.Owner.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => p.Blueprint == required_feature).Any(p => p.Param == weapon.Blueprint.Category))
            {
                return false;
            }
            if (enemy == null)
            {//to allow checks
                return true;
            }
            return this.Owner.Unit.DistanceTo(enemy) < (double)this.Owner.Unit.View.Corpulence + (double)enemy.View.Corpulence + (double)max_range.Meters && this.Owner.Unit.HasLOS(enemy);
        }
    }




    [AllowMultipleComponents]
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class DoNotProvokeAooOnAoo : RuleInitiatorLogicComponent<RuleAttackRoll>, IRulebookHandler<RuleAttackRoll>, IInitiatorRulebookSubscriber
    {
        public WeaponCategory[] weapon_categories;

        public override void OnEventAboutToTrigger(RuleAttackRoll evt)
        {

            var weapon_attack = evt.RuleAttackWithWeapon;
            if (weapon_attack != null && !weapon_attack.IsAttackOfOpportunity)
            {
                return;
            }
            if (!weapon_categories.Contains(evt.Weapon.Blueprint.Category))
                return;
            evt.DoNotProvokeAttacksOfOpportunity = true;
        }

        public override void OnEventDidTrigger(RuleAttackRoll evt)
        {
        }
    }


    [AllowMultipleComponents]
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AttackDamageBonusOnAoo : RuleInitiatorLogicComponent<RuleAttackRoll>, IRulebookHandler<RuleAttackRoll>, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, IInitiatorRulebookSubscriber
    {
        public WeaponCategory[] weapon_categories;
        public ContextValue value;
        public bool only_ranged = true;

        public override void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            var weapon_attack = evt.RuleAttackWithWeapon;
            if (weapon_attack == null || !check(weapon_attack))
            {
                return;
            }
           
            var bonus = value.Calculate(this.Fact.MaybeContext);
            evt.AddTemporaryModifier(evt.Target.Stats.AdditionalAttackBonus.AddModifier(bonus, (GameLogicComponent)this, ModifierDescriptor.UntypedStackable));
        }

        private bool check(RuleAttackWithWeapon evt)
        {
            if (!evt.IsAttackOfOpportunity)
            {
                return false;
            }

            if (!weapon_categories.Contains(evt.Weapon.Blueprint.Category))
                return false;

            if (!evt.Weapon.Blueprint.IsRanged)
            {
                return false;
            }

            return true;
        }

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
            var weapon_attack = evt.AttackRoll?.RuleAttackWithWeapon;
            if (weapon_attack == null || !check(weapon_attack))
            {
                return;
            }
            var bonus = value.Calculate(this.Fact.MaybeContext);

            evt.DamageBundle.WeaponDamage?.AddBonus(bonus);
        }

        public override void OnEventDidTrigger(RuleAttackRoll evt)
        {
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            
        }
    }


    [Harmony12.HarmonyPatch(typeof(UnitEngagementExtension))]
    [Harmony12.HarmonyPatch("IsEngage", Harmony12.MethodType.Normal)]
    class Patch_UnitEngagementExtension__IsEngage__Patch
    {
        static bool Prefix(UnitEntityData unit, UnitEntityData enemy, ref bool __result)
        {
            __result = false;
            if (!unit.Descriptor.State.CanAct)
            {               
                return false;
            }
            WeaponSlot threatHand = unit.GetThreatHand();
            if (threatHand == null || !unit.IsReach(enemy, threatHand))
            {
                var aoo_part = unit.Get<UnitPartSpecialAttackOfOportunity>();

                if (aoo_part == null || !aoo_part.canMakeAoo(enemy))
                {
                    return false;
                }
            }
            __result = UnitPartConcealment.Calculate(unit, enemy, false) != Concealment.Total;
            return false;
        }
    }


    [Harmony12.HarmonyPatch(typeof(UnitCombatEngagementController))]
    [Harmony12.HarmonyPatch("TickUnit", Harmony12.MethodType.Normal)]
    class Patch_UnitCombatEngagementControllern__TickUnit
    {
        static bool Prefix(UnitCombatEngagementController __instance, UnitEntityData unit)
        {
            if ((unit.Get<UnitPartAooAgainstAllies>()?.active()).GetValueOrDefault())
            {
                foreach (UnitGroupMemory.UnitInfo enemy in unit.Memory.UnitsList)
                {
                    UnitEntityData unit1 = enemy.Unit;
                    if (unit1.Descriptor.State.IsConscious && unit.IsEngage(unit1) && unit != unit1)
                        unit.CombatState.Engage(unit1);
                }
            }
            return true;
        }
    }

    //units with allied aoo should not really engage
    [Harmony12.HarmonyPatch(typeof(UnitCombatState))]
    [Harmony12.HarmonyPatch("EngagedBy", Harmony12.MethodType.Getter)]
    class Patch_UnitCombatState_EngagedBy
    {
        static bool Prefix(UnitCombatState __instance, Dictionary<UnitEntityData, TimeSpan> ___m_EngagedBy, ref Dictionary<UnitEntityData, TimeSpan>.KeyCollection __result)
        {
            __result = ___m_EngagedBy.Where(kv => !kv.Key.IsAlly(__instance.Unit)).ToDictionary(d => d.Key, d => d.Value).Keys;
            return false;
        }
    }



    [Harmony12.HarmonyPatch(typeof(UnitCombatState))]
    [Harmony12.HarmonyPatch("AttackOfOpportunity", Harmony12.MethodType.Normal)]
    class Patch_UnitCombatState_AttackOfOpportunity_Transpiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var check_threat_hand = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Call && x.operand.ToString().Contains("GetThreatHand"));

            codes[check_threat_hand] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_1);
            codes.Insert(check_threat_hand + 1, new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                           new Func<UnitEntityData, UnitEntityData, WeaponSlot>(canMakeAoo).Method
                                                                           )
                        );
            return codes.AsEnumerable();
        }


        static public WeaponSlot canMakeAoo(UnitEntityData unit, UnitEntityData enemy)
        {
            if (unit == null || enemy == null)
            {
                return null;
            }
            if (unit.GetThreatHand() != null)
            {
                return unit.GetThreatHand();
            }

            var aoo_part = unit.Get<UnitPartSpecialAttackOfOportunity>();
            if (aoo_part != null)
            {
                return unit.Body?.PrimaryHand;
            }

            return null;
        }
    }



    [Harmony12.HarmonyPatch(typeof(UnitAttackOfOpportunity))]
    [Harmony12.HarmonyPatch("Init", Harmony12.MethodType.Normal)]
    class UnitAttackOfOpportunity_Init_Transpiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var check_threat_hand = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Call && x.operand.ToString().Contains("GetThreatHand"));

            codes[check_threat_hand] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                           new Func<UnitEntityData, WeaponSlot>(getThreatHand2).Method
                                                                           );
            return codes.AsEnumerable();
        }


        static public WeaponSlot getThreatHand2(UnitEntityData unit)
        {
            if (unit == null)
            {
                return null;
            }
            if (unit.GetThreatHand() != null)
            {
                return unit.GetThreatHand();
            }

            var aoo_part = unit.Get<UnitPartSpecialAttackOfOportunity>();
            if (aoo_part != null)
            {
                return unit.Body?.PrimaryHand;
            }

            return null;
        }
    }


}
