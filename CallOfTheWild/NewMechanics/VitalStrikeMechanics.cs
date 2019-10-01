using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
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

    public class UnitPartVitalStrikeScalingDamageBonus : UnitPart
    {
        [JsonProperty]
        private List<Fact> buffs = new List<Fact>();

        public void addBuff(Fact buff)
        {
            buffs.Add(buff);
        }


        public void removeBuff(Fact buff)
        {
            buffs.Remove(buff);
        }


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


        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartVitalStrikeScalingDamageBonus>().addBuff(this.Fact);
        }


        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartVitalStrikeScalingDamageBonus>().removeBuff(this.Fact);
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
            //allow it to work with elemental damage (?)
            DamageDescription damageDescription = evt.DamageDescription.FirstItem<DamageDescription>();

            if (damageDescription == null)
                return false;

            int bonus = evt.Initiator.Ensure<UnitPartVitalStrikeScalingDamageBonus>().getDamageBonus();

            bonus *= (___m_DamageMod - 1);
            if (bonus <= 0)
            {
                return false;
            }
            damageDescription.Bonus += bonus;
            damageDescription.Dice = new DiceFormula(damageDescription.Dice.Rolls * ___m_DamageMod, damageDescription.Dice.Dice);
            return false;
        }
    }

    //fix vital strike to be recognized by overhand chop and greater weapon of the chosen
    [Harmony12.HarmonyPatch(typeof(VitalStrike))]
    [Harmony12.HarmonyPatch("OnEventAboutToTrigger", Harmony12.MethodType.Normal)]
    class VitalStrike__OnEventAboutToTrigger__Patch
    {
        static void Postfix(VitalStrike __instance, RuleCalculateWeaponStats evt)
        {
            if (evt.AttackWithWeapon != null)
            {
                evt.AttackWithWeapon.IsFirstAttack = true;
                evt.AttackWithWeapon.AttacksCount = 1;
            }
        }
    }



}
