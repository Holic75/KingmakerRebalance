using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.OutgoingConcealementMechanics
{
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

            
            if (unitPartConcealment != null)
            {
                List<Feet> m_BlindsightRanges = Harmony12.Traverse.Create(unitPartConcealment).Field("m_BlindsightRanges").GetValue<List<Feet>>();
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
                        return Concealment.None;
                }
            }

            Concealment a = Concealment.None;   

            foreach (UnitPartConcealment.ConcealmentEntry concealment in unitPartOutgoingConcealment.m_Concealments)
            {
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


    [Harmony12.HarmonyPatch(typeof(UnitPartConcealment))]
    [Harmony12.HarmonyPatch("Calculate", Harmony12.MethodType.Normal)]
    class UnitPartConcealment__Calculate__Patch
    {
        static void Postfix([NotNull] UnitEntityData initiator, [NotNull] UnitEntityData target, bool attack, ref Concealment __result)
        {
            __result = UnitPartOutgoingConcealment.Max(UnitPartOutgoingConcealment.Calculate(initiator, target, attack), __result);
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



}
