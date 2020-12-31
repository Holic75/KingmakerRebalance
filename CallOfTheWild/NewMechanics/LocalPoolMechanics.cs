using Kingmaker.Blueprints;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.LocalPoolMechanics
{
    [AllowedOn(typeof(BlueprintBuff))]
    public class BuffLocalPool : BuffLogic
    {
        public ContextValue value;
        public bool remove_when_empty = true;
        public int min_value;
        [JsonProperty]
        private int remaining_value;

        public override void OnFactActivate()
        {
            remaining_value = value.Calculate(this.Fact.MaybeContext);
        }

        public override void OnFactDeactivate()
        {

        }

        public int getRemainingPool()
        {
            return remaining_value;
        }


        public void updatePool(int val)
        {
            remaining_value += val;
            if (remaining_value < min_value)
            {
                remaining_value = min_value;
            }

            if (remaining_value <= 0 && remove_when_empty)
            {
                this.Buff.Remove();
            }
        }
    }



    [AllowedOn(typeof(BlueprintBuff))]
    [AllowMultipleComponents]
    public class AddDamageProtectionEnergyFromLocalPool : AddDamageResistanceBase
    {
        public DamageEnergyType Type;

        public override int GetValue()
        {
            int pool = 0;

            this.Fact.CallComponents<BuffLocalPool>(c => pool = c.getRemainingPool());
            return pool;
        }


        public override void OnSpendPool(int damage)
        {
            this.Fact.CallComponents<BuffLocalPool>(c => c.updatePool(-Math.Min(c.getRemainingPool(), damage)));
        }

        public override bool Bypassed(BaseDamage damage, ItemEntityWeapon weapon)
        {
            EnergyDamage energyDamage = damage as EnergyDamage;
            if (energyDamage != null)
                return energyDamage.EnergyType != this.Type;
            return true;
        }
    }
}
