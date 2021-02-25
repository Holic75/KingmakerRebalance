using JetBrains.Annotations;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.ConcealementMechanics
{

    public class UnitPartISpecificConcealment : AdditiveUnitPart
    {
        public UnitPartConcealment.ConcealmentEntry[] GetConcealments(UnitEntityData attacker)
        {
            var concealments = new List<UnitPartConcealment.ConcealmentEntry>();
            foreach (var b in buffs)
            {
                bool works_on = false;

                b.CallComponents<SpecificConcealementBase>(s => works_on = s.worksAgainst(attacker));

                if (works_on)
                {
                    b.CallComponents<SpecificConcealementBase>(s => concealments.Add(s.CreateConcealmentEntry()));
                }
            }

            return concealments.ToArray();
        }
    }


    public abstract class SpecificConcealementBase : BuffLogic
    {
        public ConcealmentDescriptor Descriptor;
        public Concealment Concealment;
        public bool CheckWeaponRangeType;
        [ShowIf("CheckWeaponRangeType")]
        public AttackTypeAttackBonus.WeaponRangeType RangeType;
        public bool CheckDistance;
        [ShowIf("CheckDistance")]
        public Feet DistanceGreater;
        public bool OnlyForAttacks;

        public abstract bool worksAgainst(UnitEntityData attacker);

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartISpecificConcealment>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartISpecificConcealment>().removeBuff(this.Fact);
        }

        public UnitPartConcealment.ConcealmentEntry CreateConcealmentEntry()
        {
            UnitPartConcealment.ConcealmentEntry concealmentEntry = new UnitPartConcealment.ConcealmentEntry()
            {
                Concealment = this.Concealment,
                Descriptor = this.Descriptor
            };
            if (this.CheckDistance)
                concealmentEntry.DistanceGreater = this.DistanceGreater;
            if (this.CheckWeaponRangeType)
                concealmentEntry.RangeType = new AttackTypeAttackBonus.WeaponRangeType?(this.RangeType);
            concealmentEntry.OnlyForAttacks = this.OnlyForAttacks;
            return concealmentEntry;
        }
    }



    [AllowedOn(typeof(BlueprintUnitFact))]
    public class IgnoreCasterConcealemnt : SpecificConcealementBase
    {
        public override bool worksAgainst(UnitEntityData attacker)
        {
            return (this.Context.MaybeCaster != attacker);
        }
    }

    public class UnitPartIgnoreFogConcealement : AdditiveUnitPart
    {
        public bool active()
        {
            return !buffs.Empty();
        }
    }




    public class UnitPartVisibilityLimit : AdditiveUnitPart
    {
        public bool active()
        {
            return !buffs.Empty();
        }


        public float getMaxDistance()
        {
            float max_distance = 1000;

            foreach (var b in buffs)
            {
                max_distance = Math.Min(max_distance, b.Blueprint.GetComponent<SetVisibilityLimit>().visibility_limit.Meters);
            }

            return max_distance;
        }
    }


    public class UnitPartOutgoingConcealment : UnitPart
    {
        [CanBeNull]
        [JsonProperty]
        private List<UnitPartConcealment.ConcealmentEntry> m_Concealments;


        public void AddConcealment(UnitPartConcealment.ConcealmentEntry entry)
        {
            if (this.m_Concealments == null)
                this.m_Concealments = new List<UnitPartConcealment.ConcealmentEntry>();
            this.m_Concealments.Add(entry);
        }


        public void RemoveConcealement(UnitPartConcealment.ConcealmentEntry entry)
        {
            if (this.m_Concealments == null)
                return;
            foreach (UnitPartConcealment.ConcealmentEntry concealment in this.m_Concealments)
            {
                if (concealment.Descriptor == entry.Descriptor && concealment.Concealment == entry.Concealment)
                {
                    AttackTypeAttackBonus.WeaponRangeType? rangeType1 = concealment.RangeType;
                    int valueOrDefault1 = (int)rangeType1.GetValueOrDefault();
                    AttackTypeAttackBonus.WeaponRangeType? rangeType2 = entry.RangeType;
                    int valueOrDefault2 = (int)rangeType2.GetValueOrDefault();
                    if ((valueOrDefault1 != valueOrDefault2 ? 0 : (rangeType1.HasValue == rangeType2.HasValue ? 1 : 0)) != 0 && concealment.DistanceGreater == entry.DistanceGreater)
                    {
                        this.m_Concealments.Remove(concealment);
                        if (this.m_Concealments.Count > 0)
                            break;
                        this.m_Concealments = (List<UnitPartConcealment.ConcealmentEntry>)null;
                        break;
                    }
                }
            }
        }


        public static Concealment Calculate([NotNull] UnitEntityData initiator, [NotNull] UnitEntityData target, bool attack = false)
        {
            UnitPartConcealment unitPartConcealment = initiator.Get<UnitPartConcealment>();
            UnitPartOutgoingConcealment unitPartOutgoingConcealment = initiator.Get<UnitPartOutgoingConcealment>();
            if (unitPartOutgoingConcealment?.m_Concealments == null)
            {
                return Concealment.None; //no concelement update
            }

            bool has_true_seeing = initiator.Descriptor.State.HasCondition(UnitCondition.TrueSeeing);
            Concealment a = Concealment.None;
            var ignore_fog_concealement_part = initiator.Get<UnitPartIgnoreFogConcealement>();

            foreach (UnitPartConcealment.ConcealmentEntry concealment in unitPartOutgoingConcealment.m_Concealments)
            {              
                if (concealment.Descriptor == ConcealmentDescriptor.Fog && ignore_fog_concealement_part != null && ignore_fog_concealement_part.active())
                {
                    continue;
                }

                if (concealment.Descriptor != ConcealmentDescriptor.Fog && concealment.Descriptor != ConcealmentDescriptor.InitiatorIsBlind && has_true_seeing)
                {
                    continue;
                }

                if (!concealment.OnlyForAttacks || attack)
                {
                    if (concealment.DistanceGreater > 0.Feet())
                    {
                        float num1 = initiator.DistanceTo(target);
                        float num2 = initiator.View.Corpulence + target.View.Corpulence;
                        if ((double)num1 <= (double)concealment.DistanceGreater.Meters + (double)num2)
                            continue;
                    }
                    if (concealment.RangeType.HasValue)
                    {
                        RuleAttackRoll ruleAttackRoll = Rulebook.CurrentContext.LastEvent<RuleAttackRoll>();
                        ItemEntityWeapon itemEntityWeapon = ruleAttackRoll == null ? initiator.GetFirstWeapon() : ruleAttackRoll.Weapon;
                        if (itemEntityWeapon == null || !AttackTypeAttackBonus.CheckRangeType(itemEntityWeapon.Blueprint, concealment.RangeType.Value))
                            continue;
                    }
                    a = UnitPartOutgoingConcealment.Max(a, concealment.Concealment);
                }
            }
            return a;
        }

        public static Concealment Max(Concealment a, Concealment b)
        {
            if (a > b)
                return a;
            return b;
        }
    }


    /*[Harmony12.HarmonyPatch(typeof(UnitPartConcealment))]
    [Harmony12.HarmonyPatch("Calculate", Harmony12.MethodType.Normal)]
    class UnitPartConcealment__Calculate__Patch
    {
        static void Postfix([NotNull] UnitEntityData initiator, [NotNull] UnitEntityData target, bool attack, ref Concealment __result)
        {
            __result = UnitPartOutgoingConcealment.Max(UnitPartOutgoingConcealment.Calculate(initiator, target, attack), __result);
        }
    }*/


    [Harmony12.HarmonyPatch(typeof(UnitPartConcealment))]
    [Harmony12.HarmonyPatch("Calculate", Harmony12.MethodType.Normal)]
    class Patch_UnitPartConcealment
    {
        public static bool Prefix(UnitEntityData initiator, UnitEntityData target, bool attack, ref Concealment __result)
        {
            Main.TraceLog();
            UnitPartConcealment unitPartConcealment1 = initiator.Get<UnitPartConcealment>();
            UnitPartConcealment unitPartConcealment2 = target.Get<UnitPartConcealment>();

            bool has_true_seeing = initiator.Descriptor.State.HasCondition(UnitCondition.TrueSeeing);
            if (unitPartConcealment1 != null && unitPartConcealment1.IgnoreAll)
            {
                __result = Concealment.None;
                return false;
            }
            List<Feet> m_BlindsightRanges = Harmony12.Traverse.Create(unitPartConcealment1).Field("m_BlindsightRanges").GetValue<List<Feet>>();
            if (m_BlindsightRanges != null)
            {
                Feet feet = 0.Feet();
                foreach (Feet blindsightRange in m_BlindsightRanges)
                {
                    if (feet < blindsightRange)
                        feet = blindsightRange;
                }
                float num = initiator.View.Corpulence + target.View.Corpulence;
                if ((double)initiator.DistanceTo(target) - (double)num <= (double)feet.Meters)
                {
                    __result = Concealment.None;
                    return false;
                }
            }
            Concealment a = Concealment.None;
            if (!initiator.Descriptor.IsSeeInvisibility && target.Descriptor.State.HasCondition(UnitCondition.Invisible))
                a = Concealment.Total;

            var ignore_fog_concealement_part = initiator.Get<UnitPartIgnoreFogConcealement>();
            List<UnitPartConcealment.ConcealmentEntry> m_Concealments = Harmony12.Traverse.Create(unitPartConcealment2).Field("m_Concealments").GetValue<List<UnitPartConcealment.ConcealmentEntry>>();

            var all_concealements = m_Concealments?.ToArray() ?? new UnitPartConcealment.ConcealmentEntry[0];
            var specific_concealment_part = target.Get<UnitPartISpecificConcealment>();
            if (specific_concealment_part != null)
            {
                all_concealements = all_concealements.AddToArray(specific_concealment_part.GetConcealments(initiator));
            }

            if (a < Concealment.Total && !all_concealements.Empty())
            {
                foreach (UnitPartConcealment.ConcealmentEntry concealment in all_concealements)
                {
                    if (concealment.Descriptor == ConcealmentDescriptor.Fog && ignore_fog_concealement_part != null && ignore_fog_concealement_part.active())
                    {
                        continue;
                    }

                    if (concealment.Descriptor != ConcealmentDescriptor.Fog && concealment.Descriptor != ConcealmentDescriptor.InitiatorIsBlind && has_true_seeing)
                    {
                        continue;
                    }

                    if (!concealment.OnlyForAttacks || attack)
                    {
                        if (concealment.DistanceGreater > 0.Feet())
                        {
                            float num1 = initiator.DistanceTo(target);
                            float num2 = initiator.View.Corpulence + target.View.Corpulence;
                            if ((double)num1 <= (double)concealment.DistanceGreater.Meters + (double)num2)
                                continue;
                        }
                        if (concealment.RangeType.HasValue)
                        {
                            RuleAttackRoll ruleAttackRoll = Rulebook.CurrentContext.LastEvent<RuleAttackRoll>();
                            ItemEntityWeapon itemEntityWeapon = ruleAttackRoll == null ? initiator.GetFirstWeapon() : ruleAttackRoll.Weapon;
                            if (itemEntityWeapon == null || !AttackTypeAttackBonus.CheckRangeType(itemEntityWeapon.Blueprint, concealment.RangeType.Value))
                                continue;
                        }
                        a = a > concealment.Concealment ? a : concealment.Concealment;
                    }
                }
            }
            if (unitPartConcealment2 != null && unitPartConcealment2.Disable)
                a = Concealment.None;
            if (initiator.Descriptor.State.HasCondition(UnitCondition.Blindness))
                a = Concealment.Total;
            if (initiator.Descriptor.State.HasCondition(UnitCondition.PartialConcealmentOnAttacks))
                a = Concealment.Partial;
            if (a == Concealment.None && (ignore_fog_concealement_part == null || !ignore_fog_concealement_part.active()) 
                  && Game.Instance.Player.Weather.ActualWeather >= BlueprintRoot.Instance.WeatherSettings.ConcealmentBeginsOn)
            {
                RuleAttackRoll ruleAttackRoll = Rulebook.CurrentContext.LastEvent<RuleAttackRoll>();
                ItemEntityWeapon itemEntityWeapon = ruleAttackRoll == null ? initiator.GetFirstWeapon() : ruleAttackRoll.Weapon;
                if (itemEntityWeapon != null && AttackTypeAttackBonus.CheckRangeType(itemEntityWeapon.Blueprint, AttackTypeAttackBonus.WeaponRangeType.Ranged))
                    a = Concealment.Partial;
            }

            a = UnitPartOutgoingConcealment.Max(UnitPartOutgoingConcealment.Calculate(initiator, target, attack), a);
            if (unitPartConcealment1 != null && unitPartConcealment1.IgnorePartial && a == Concealment.Partial)
                a = Concealment.None;
            if (unitPartConcealment1 != null && unitPartConcealment1.TreatTotalAsPartial && a == Concealment.Total)
                a = Concealment.Partial;
            __result = a;
            return false;
        }
    }



    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AddOutgoingConcealment : OwnedGameLogicComponent<UnitDescriptor>
    {
        public ConcealmentDescriptor Descriptor;
        public Concealment Concealment;
        public bool CheckWeaponRangeType;
        [ShowIf("CheckWeaponRangeType")]
        public AttackTypeAttackBonus.WeaponRangeType RangeType;
        public bool CheckDistance;
        [ShowIf("CheckDistance")]
        public Feet DistanceGreater;
        public bool OnlyForAttacks;

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartOutgoingConcealment>().AddConcealment(this.CreateConcealmentEntry());
        }

        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartOutgoingConcealment>().RemoveConcealement(this.CreateConcealmentEntry());
        }

        private UnitPartConcealment.ConcealmentEntry CreateConcealmentEntry()
        {
            UnitPartConcealment.ConcealmentEntry concealmentEntry = new UnitPartConcealment.ConcealmentEntry()
            {
                Concealment = this.Concealment,
                Descriptor = this.Descriptor
            };
            if (this.CheckDistance)
                concealmentEntry.DistanceGreater = this.DistanceGreater;
            if (this.CheckWeaponRangeType)
                concealmentEntry.RangeType = new AttackTypeAttackBonus.WeaponRangeType?(this.RangeType);
            concealmentEntry.OnlyForAttacks = this.OnlyForAttacks;
            return concealmentEntry;
        }
    }



    [AllowedOn(typeof(BlueprintUnitFact))]
    public class IgnoreFogConcelement : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {
        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartIgnoreFogConcealement>().addBuff(this.Fact);
        }

        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartIgnoreFogConcealement>().removeBuff(this.Fact);
        }
    }


    [ComponentName("Attack Dice Reroll")]
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class ReduceConcelemntByOneStep : RuleInitiatorLogicComponent<RuleAttackRoll>
    {
        private RuleAttackRoll m_Attack;

        public override void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (this.m_Attack != null)
                return;
            evt.Initiator.Ensure<UnitPartConcealment>().IgnorePartial = true;
            evt.Initiator.Ensure<UnitPartConcealment>().TreatTotalAsPartial = true;
            this.m_Attack = evt;
        }

        public override void OnEventDidTrigger(RuleAttackRoll evt)
        {
            if (this.m_Attack == null)
                return;
            evt.Initiator.Ensure<UnitPartConcealment>().IgnorePartial = false;
            evt.Initiator.Ensure<UnitPartConcealment>().TreatTotalAsPartial = false;
            this.m_Attack = (RuleAttackRoll)null;
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class SetVisibilityLimit : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {
        public Feet visibility_limit;
        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartVisibilityLimit>().addBuff(this.Fact);
        }

        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartVisibilityLimit>().removeBuff(this.Fact);
        }
    }



    [Harmony12.HarmonyPatch(typeof(AbilityData), "GetVisualDistance")]
    static class AbilityData_GetVisualDistance_Patch
    {
        static void Postfix(AbilityData __instance, ref float __result)
        {
            Main.TraceLog();
            AbilityData_GetApproachDistance_Patch.Postfix(__instance, null, ref __result);
        }
    }

    [Harmony12.HarmonyPatch(typeof(AbilityData), "GetApproachDistance")]
    static class AbilityData_GetApproachDistance_Patch
    {
        internal static void Postfix(AbilityData __instance, UnitEntityData target, ref float __result)
        {
            Main.TraceLog();
            try
            {
                var caster = __instance.Caster;
                var part = caster.Get<UnitPartVisibilityLimit>();
                if (part != null && part.active())
                {
                    __result = Math.Min(part.getMaxDistance(), __result);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }

    //allow character with blindsense to ignore mirror image if target is within blindsight range
    [Harmony12.HarmonyPatch(typeof(RuleAttackRoll))]
    [Harmony12.HarmonyPatch("OnTrigger", Harmony12.MethodType.Normal)]
    class RuleAttackRoll__OnTrigger__MirrorImageBlinsightFix
    {
        static IEnumerable<Harmony12.CodeInstruction> Transpiler(IEnumerable<Harmony12.CodeInstruction> instructions)
        {
            List<Harmony12.CodeInstruction> codes = new List<Harmony12.CodeInstruction>();
            try
            {
                codes = instructions.ToList();
                var immune_to_visual_idx = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("IsImmuneToVisualEffects"));
                codes[immune_to_visual_idx - 1] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_0); //add this == RuleAttackRoll (instead of Initiator.getDescriptor)
                codes[immune_to_visual_idx] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                           new Func<UnitEntityData, RuleAttackRoll, bool>(ignoreMirrorImage).Method
                                                                           );
            }
            catch (Exception ex)
            {
                Main.logger.Log(ex.ToString());
            }

            return codes.AsEnumerable();
        }


        internal static bool ignoreMirrorImage(UnitEntityData initiator, RuleAttackRoll rule_attack_roll)
        {
            var target = rule_attack_roll.Target;

            if (initiator.Descriptor.IsImmuneToVisualEffects)
            {
                return true;
            }

            var unit_part_concealement = initiator.Get<UnitPartConcealment>();
            if (unit_part_concealement == null)
            {
                return false;
            }

            List<Feet> m_BlindsightRanges = Harmony12.Traverse.Create(unit_part_concealement).Field("m_BlindsightRanges").GetValue<List<Feet>>();
            if (m_BlindsightRanges != null)
            {
                Feet feet = 0.Feet();
                foreach (Feet blindsightRange in m_BlindsightRanges)
                {
                    if (feet < blindsightRange)
                        feet = blindsightRange;
                }
                float num = initiator.View.Corpulence + target.View.Corpulence;
                if ((double)initiator.DistanceTo(target) - (double)num <= (double)feet.Meters)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
