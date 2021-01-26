using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Abilities.Components.AbilityCustomMeleeAttack;

namespace CallOfTheWild.VitalStrikeMechanics
{

    public class UnitPartVitalStrikeScalingDamageBonus : AdditiveUnitPart
    {
        public int getDamageBonus()
        {
            var bonus = 0;
            foreach (var b in buffs)
            {
                int new_bonus = 0;

                b.CallComponents<VitalStrikeScalingDamage>(v => v.getValue(out new_bonus));
                bonus += new_bonus;
            }

            return bonus;
        }
    }


    public class UnitPartVitalStrikeCriticalConfirmationBonus : AdditiveUnitPart
    {
        public int getBonus()
        {
            var bonus = 0;
            foreach (var b in buffs)
            {
                int new_bonus = 0;

                b.CallComponents<VitalStrikeCriticalConfirmationBonus>(v => v.getValue(out new_bonus));
                bonus += new_bonus;
            }

            return bonus;
        }
    }

    [AllowedOn(typeof(BlueprintUnitFact))]
    [AllowMultipleComponents]
    public class VitalStrikeScalingDamage : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {
        public ContextValue Value;
        public int multiplier = 1;

        private MechanicsContext Context
        {
            get
            {
                return this.Fact.MaybeContext;
            }
        }


        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartVitalStrikeScalingDamageBonus>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartVitalStrikeScalingDamageBonus>().removeBuff(this.Fact);
        }


        public void getValue(out int val)
        {
            val = Value.Calculate(this.Context) * multiplier;
        }
    }



    [AllowedOn(typeof(BlueprintUnitFact))]
    [AllowMultipleComponents]
    public class VitalStrikeCriticalConfirmationBonus : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {
        public ContextValue Value;
        public int multiplier = 1;

        private MechanicsContext Context
        {
            get
            {
                return this.Fact.MaybeContext;
            }
        }


        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartVitalStrikeCriticalConfirmationBonus>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartVitalStrikeCriticalConfirmationBonus>().removeBuff(this.Fact);
        }


        public void getValue(out int val)
        {
            val = Value.Calculate(this.Context) * multiplier;
        }
    }




    [Harmony12.HarmonyPatch(typeof(VitalStrike))]
    [Harmony12.HarmonyPatch("OnEventDidTrigger", Harmony12.MethodType.Normal)]
    class VitalStrike__OnEventDidTrigger__Patch
    {
        static bool Prefix(VitalStrike __instance, RuleCalculateWeaponStats evt, ref int ___m_DamageMod)
        {
            Main.TraceLog();
            int dmg_mod = ___m_DamageMod;
            if (Main.settings.balance_fixes)
            {
                var bab = evt.Initiator.Stats.BaseAttackBonus.ModifiedValue;
                dmg_mod = 2 + Math.Min(2, (Math.Max(bab - 6, 0) / 5));
            }
            //allow it to work with elemental damage (?)
            DamageDescription damageDescription = evt.DamageDescription.FirstItem<DamageDescription>();

            if (damageDescription == null)
                return false;

            int bonus = evt.Initiator.Ensure<UnitPartVitalStrikeScalingDamageBonus>().getDamageBonus();

            bonus *= (dmg_mod - 1);
            damageDescription.Bonus += bonus;
            //make vital strike damage not multipliable on critical hit
            var vital_strike_damage = new DamageDescription();
            vital_strike_damage.TypeDescription = damageDescription.TypeDescription;
            vital_strike_damage.Dice = new DiceFormula(damageDescription.Dice.Rolls * (dmg_mod - 1), damageDescription.Dice.Dice);
            if (evt.DamageDescription.Count() <= 1)
            {
                evt.DamageDescription.Add(vital_strike_damage);
            }
            else
            {
                evt.DamageDescription.Insert(1, vital_strike_damage);
            }
            //damageDescription.Dice = new DiceFormula(damageDescription.Dice.Rolls * ___m_DamageMod, damageDescription.Dice.Dice);
            return false;
        }
    }

    //fix vital strike to be recognized by overhand chop and greater weapon of the chosen
    [Harmony12.HarmonyPatch(typeof(VitalStrike))]
    [Harmony12.HarmonyPatch("OnEventAboutToTrigger", Harmony12.MethodType.Normal)]
    class VitalStrike__OnEventAboutToTrigger__Patch
    {
        static void Postfix(VitalStrike __instance, RuleCalculateWeaponStats evt, ref int ___m_DamageMod)
        {
            Main.TraceLog();
            if (evt.AttackWithWeapon != null)
            {
                evt.AttackWithWeapon.IsFirstAttack = true;
                evt.AttackWithWeapon.AttacksCount = 1;
            }
        }
    }

    //fix vital strike to work on ranged weapons
    internal class VitalStrikeRangedAttackPatch
    {

        internal static void Run()
        {
            var Deliver = typeof(AbilityCustomMeleeAttack).GetNestedTypes(Harmony12.AccessTools.all).First(x => x.Name.Contains("Deliver"));
            var original = Harmony12.AccessTools.Method(Deliver, "MoveNext");
            var transpiler = Harmony12.AccessTools.Method(typeof(VitalStrikeRangedAttackPatch), nameof(Deliver_Transpiler));
            try
            {
                Main.harmony.Patch(original, null, null, new Harmony12.HarmonyMethod(transpiler));
            }
            catch (Exception ex)
            {
                Main.logger.Log(ex.ToString());
            }
        }

        public static IEnumerable<Harmony12.CodeInstruction> Deliver_Transpiler(IEnumerable<Harmony12.CodeInstruction> instructions)
        {
            List<Harmony12.CodeInstruction> codes = new List<Harmony12.CodeInstruction>();
            try
            {
                codes = instructions.ToList();
                var load_this = codes.Find(x => x.opcode == System.Reflection.Emit.OpCodes.Ldfld && x.operand.ToString().Contains("this"));
                var find_threat_hand = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Call && x.operand.ToString().Contains("GetThreatHand"));
                codes[find_threat_hand] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call, new Func<UnitEntityData, AbilityCustomMeleeAttack, WeaponSlot>(getWeaponSlot).Method);
                codes.InsertRange(find_threat_hand, new Harmony12.CodeInstruction[] { new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_0), load_this });
            }
            catch (Exception ex)
            {
                Main.logger.Log(ex.ToString());
            }

            return codes.AsEnumerable();
        }


        static WeaponSlot getWeaponSlot(UnitEntityData attacker, AbilityCustomMeleeAttack custom_ability)
        {
            return !custom_ability.IsVitalStrike ? attacker.GetThreatHand() : attacker.Body.PrimaryHand;
        }
    }

}
