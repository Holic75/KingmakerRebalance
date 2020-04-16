using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;

namespace CallOfTheWild.WeaponTrainingMechanics
{
    class WeaponTrainingPropertyGetter : PropertyValueGetter
    {
        internal static readonly Lazy<BlueprintUnitProperty> Blueprint = new Lazy<BlueprintUnitProperty>(() =>
        {
            var p = Helpers.Create<BlueprintUnitProperty>();
            p.name = "WeaponTrainingCustomProperty";
            Main.library.AddAsset(p, "442d938de06b432f9f0d9d76ed1b08dd");
            p.SetComponents(Helpers.Create<WeaponTrainingPropertyGetter>());
            return p;
        });

        public override int GetInt(UnitEntityData unit)
        {
            int? maxWeaponRank = unit.Get<UnitPartWeaponTraining>()?.GetMaxWeaponRank();
            int num = !maxWeaponRank.HasValue ? 0 : maxWeaponRank.Value;
            return num;
        }
    }


    [ComponentName("Replace attack stat")]
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AttackStatReplacementIfWeaponTraining : RuleInitiatorLogicComponent<RuleCalculateAttackBonusWithoutTarget>
    {
        public StatType repalcement_stat;

        public override void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
            var weapon_training = this.Owner.Get<UnitPartWeaponTraining>();
            if (weapon_training == null)
            {
                return;
            }

            if (weapon_training.GetWeaponRank(evt.Weapon) <= 0)
            {
                return;
            }

            ModifiableValueAttributeStat stat1 = this.Owner.Stats.GetStat(evt.AttackBonusStat) as ModifiableValueAttributeStat;
            ModifiableValueAttributeStat stat2 = this.Owner.Stats.GetStat(this.repalcement_stat) as ModifiableValueAttributeStat;
            bool flag = stat2 != null && stat1 != null && stat2.Bonus > stat1.Bonus;
            if (!flag)
                return;
            evt.AttackBonusStat = this.repalcement_stat;
        }

        public override void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class WeaponCategoryGrace : RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
    {
        public WeaponFighterGroup group;
        public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            var weapon = evt.Weapon;
            if (weapon == null)
            {
                return;
            }

            if (evt.Weapon.Blueprint.FighterGroup != group)
            {
                return;

            }
            if (!evt.DamageBonusStat.HasValue || evt.DamageBonusStat != StatType.Strength)
            {
                return;
            }

            var training_part = this.Owner.Get<UnitPartWeaponTraining>();
            if (training_part == null)
            {
                return;
            }

            int value = training_part.GetWeaponRank(evt.Weapon);
            evt.AddBonusDamage(value);
        }

        public override void OnEventDidTrigger(RuleCalculateWeaponStats evt)
        {
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class TrainedGrace : RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
    {
        
        public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            var weapon = evt.Weapon;
            if (weapon == null)
            {
                return;
            }

            if (!evt.DamageBonusStat.HasValue || evt.DamageBonusStat != StatType.Strength)
            {
                return;
            }

            if (!evt.Weapon.Blueprint.Category.HasSubCategory(WeaponSubCategory.Finessable) && !this.Owner.HasFact(AdvancedFighterOptions.fighters_finesse))
            {
                return;
            }

            if (AdvancedFighterOptions.category_finesse_training_map.ContainsKey(evt.Weapon.Blueprint.Category) 
                && this.Owner.HasFact(AdvancedFighterOptions.category_finesse_training_map[evt.Weapon.Blueprint.Category]))
            {
                return;
            }

            ModifiableValueAttributeStat stat1 = this.Owner.Stats.GetStat(evt.Weapon.Blueprint.AttackBonusStat) as ModifiableValueAttributeStat;
            ModifiableValueAttributeStat stat2 = this.Owner.Stats.GetStat(StatType.Dexterity) as ModifiableValueAttributeStat;

            if (stat2 <= stat1)
            {
                return;
            }

            var training_part = this.Owner.Get<UnitPartWeaponTraining>();
            if (training_part == null)
            {
                return;
            }

            int value = training_part.GetWeaponRank(evt.Weapon);
            evt.AddBonusDamage(value);
        }

        public override void OnEventDidTrigger(RuleCalculateWeaponStats evt)
        {
        }
    }
}
