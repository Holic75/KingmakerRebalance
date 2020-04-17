using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

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
    public class GraceParametrized : ParametrizedFeatureComponent, IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookSubscriber
    {
        static BlueprintParametrizedFeature slashing_grace = Main.library.Get<BlueprintParametrizedFeature>("697d64669eb2c0543abb9c9b07998a38");
        static BlueprintParametrizedFeature fencing_grace = Main.library.Get<BlueprintParametrizedFeature>("47b352ea0f73c354aba777945760b441");
        static BlueprintWeaponEnchantment agile = Main.library.Get<BlueprintWeaponEnchantment>("a36ad92c51789b44fa8a1c5c116a1328");
        static BlueprintFeature deft_grace = Main.library.Get<BlueprintFeature>("b63a316cb172c7b4e906a318a0621c2c");

        public ContextValue value;

        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            var weapon = evt.Weapon;
            if (weapon == null)
            {
                return;
            }

            if (weapon.Blueprint.Category != this.Param)
            {
                return;
            }

            if (weapon.Blueprint.Category == WeaponCategory.DuelingSword && this.Owner.Progression.Features.HasFact(deft_grace))
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

            if (this.Owner.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => p.Blueprint == slashing_grace).Any(p => p.Param == weapon.Blueprint.Category))
            {
                return;
            }

            if (this.Owner.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => p.Blueprint == fencing_grace).Any(p => p.Param == weapon.Blueprint.Category))
            {
                return;
            }

            if (weapon.EnchantmentsCollection != null && weapon.EnchantmentsCollection.HasFact(agile))
            {
                return;
            }

            ModifiableValueAttributeStat stat1 = this.Owner.Stats.GetStat(evt.Weapon.Blueprint.AttackBonusStat) as ModifiableValueAttributeStat;
            ModifiableValueAttributeStat stat2 = this.Owner.Stats.GetStat(StatType.Dexterity) as ModifiableValueAttributeStat;

            if (stat2 <= stat1)
            {
                return;
            }


            evt.AddBonusDamage(value.Calculate(this.Fact.MaybeContext));
        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt)
        {
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class TrainedGrace : RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
    {
        static BlueprintParametrizedFeature slashing_grace = Main.library.Get<BlueprintParametrizedFeature>("697d64669eb2c0543abb9c9b07998a38");
        static BlueprintParametrizedFeature fencing_grace = Main.library.Get<BlueprintParametrizedFeature>("47b352ea0f73c354aba777945760b441");
        static BlueprintWeaponEnchantment agile = Main.library.Get<BlueprintWeaponEnchantment>("a36ad92c51789b44fa8a1c5c116a1328");
        static BlueprintFeature deft_grace = Main.library.Get<BlueprintFeature>("b63a316cb172c7b4e906a318a0621c2c");

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

            if (weapon.Blueprint.Category == WeaponCategory.DuelingSword && this.Owner.Progression.Features.HasFact(deft_grace))
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

            if (this.Owner.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => p.Blueprint == slashing_grace).Any(p => p.Param == weapon.Blueprint.Category))
            {
                return;
            }

            if (this.Owner.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => p.Blueprint == fencing_grace).Any(p => p.Param == weapon.Blueprint.Category))
            {
                return;
            }

            if (weapon.EnchantmentsCollection != null && weapon.EnchantmentsCollection.HasFact(agile))
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


    [AllowMultipleComponents]
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AddFeatureOnArmor : OwnedGameLogicComponent<UnitDescriptor>, IUnitActiveEquipmentSetHandler, IUnitEquipmentHandler, IGlobalSubscriber
    {
        public BlueprintUnitFact feature;
        [JsonProperty]
        private Fact m_AppliedFact;

        public ArmorProficiencyGroup[] required_armor;

        public override void OnFactActivate()
        {
            this.Apply();
        }

        public override void OnFactDeactivate()
        {
            this.Owner.RemoveFact(this.m_AppliedFact);
            this.m_AppliedFact = null;
        }

        public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
        {
            if (slot.Owner != this.Owner)
                return;
            this.Apply();
        }

        public void HandleUnitChangeActiveEquipmentSet(UnitDescriptor unit)
        {
            this.Apply();
        }

        private void Apply()
        {
            OnFactDeactivate();
            if (Owner.Body.IsPolymorphed)
            {
                return;
            }

            bool armor_ok = false;
            var body_armor = this.Owner.Body?.Armor?.MaybeArmor;
            armor_ok = body_armor != null && required_armor.Contains(body_armor.Blueprint.ProficiencyGroup);

            if (!armor_ok)
            {
                var shield = this.Owner.Body?.SecondaryHand?.MaybeShield?.ArmorComponent;
                armor_ok = shield != null && required_armor.Contains(shield.Blueprint.ProficiencyGroup);
            }

            if (this.m_AppliedFact != null || !armor_ok)
            {
                return;
            }
            this.m_AppliedFact = this.Owner.AddFact(this.feature, null, null);
        }
    }


    [AllowMultipleComponents]
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AddFeatureOnWeaponTraining : OwnedGameLogicComponent<UnitDescriptor>, IUnitActiveEquipmentSetHandler, IUnitEquipmentHandler, IGlobalSubscriber
    {
        public BlueprintUnitFact feature;
        [JsonProperty]
        private Fact m_AppliedFact;

        public override void OnFactActivate()
        {
            this.Apply();
        }

        public override void OnFactDeactivate()
        {
            this.Owner.RemoveFact(this.m_AppliedFact);
            this.m_AppliedFact = null;
        }

        public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
        {
            if (slot.Owner != this.Owner)
                return;
            this.Apply();
        }

        public void HandleUnitChangeActiveEquipmentSet(UnitDescriptor unit)
        {
            this.Apply();
        }

        private void Apply()
        {
            OnFactDeactivate();
            var weapon_training_part = this.Owner.Get<UnitPartWeaponTraining>();
            if (weapon_training_part == null)
            {
                return;
            }

            if (this.m_AppliedFact != null || weapon_training_part.GetMaxWeaponRank() <= 0)
            {
                return;
            }
            this.m_AppliedFact = this.Owner.AddFact(this.feature, null, null);
        }
    }
}
