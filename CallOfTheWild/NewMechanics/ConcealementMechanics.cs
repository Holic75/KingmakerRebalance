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
         
            Concealment a = Concealment.None;   

            foreach (UnitPartConcealment.ConcealmentEntry concealment in unitPartOutgoingConcealment.m_Concealments)
            {
                var ignore_fog_concealement_part = initiator.Get<UnitPartIgnoreFogConcealement>();
                if (concealment.Descriptor == ConcealmentDescriptor.Fog && ignore_fog_concealement_part != null && ignore_fog_concealement_part.active())
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
            UnitPartConcealment unitPartConcealment1 = initiator.Get<UnitPartConcealment>();
            UnitPartConcealment unitPartConcealment2 = target.Get<UnitPartConcealment>();
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
            if (a < Concealment.Total && m_Concealments != null)
            {
                foreach (UnitPartConcealment.ConcealmentEntry concealment in m_Concealments)
                {
                    if (concealment.Descriptor == ConcealmentDescriptor.Fog && ignore_fog_concealement_part != null && ignore_fog_concealement_part.active())
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
            AbilityData_GetApproachDistance_Patch.Postfix(__instance, null, ref __result);
        }
    }

    [Harmony12.HarmonyPatch(typeof(AbilityData), "GetApproachDistance")]
    static class AbilityData_GetApproachDistance_Patch
    {
        internal static void Postfix(AbilityData __instance, UnitEntityData target, ref float __result)
        {
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





}
