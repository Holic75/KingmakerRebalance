using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.Visual.HitSystem;
using System;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Newtonsoft.Json;
using Kingmaker.Utility;
using Kingmaker.UI.GenericSlot;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.EntitySystem.Entities;
using System.Collections.Generic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.Blueprints.Validation;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.ElementsSystem;
using Kingmaker.Controllers;
using Kingmaker;
using static Kingmaker.UnitLogic.Abilities.Components.AbilityCustomMeleeAttack;
using Kingmaker.UnitLogic.Mechanics.ContextData;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.EntitySystem;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.EntitySystem.Persistence.Versioning;
using JetBrains.Annotations;
using Kingmaker.Enums.Damage;
using Kingmaker.Inspect;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;

namespace CallOfTheWild.NewMechanics.EnchantmentMechanics
{
    public class BuffRemainingGroupsSizeEnchantPrimaryHandWeapon : BuffLogic
    {
        public ActivatableAbilityGroup group;
        public BlueprintWeaponEnchantment[] enchantments;
        public BlueprintWeaponType[] allowed_types;
        public bool lock_slot = false;
        public bool only_non_magical = false;
        [JsonProperty]
        private ItemEnchantment m_Enchantment;
        [JsonProperty]
        private ItemEntityWeapon m_Weapon;
        [JsonProperty]
        private bool m_unlock;


        private int getRemainingGroupSize()
        {
            int remaining_group_size = Context.MaybeCaster.Ensure<UnitPartActivatableAbility>().GetGroupSize(this.group);

            foreach (var a in Owner.ActivatableAbilities)
            {
                if (a.Blueprint.Group == group && a.IsOn)
                {
                    remaining_group_size -= a.Blueprint.WeightInGroup;
                }
            }
            return remaining_group_size;
        }

        public override void OnFactActivate()
        {
            m_unlock = false;
            var unit = this.Owner;
            if (unit == null) return;

            var weapon = unit.Body.PrimaryHand.HasWeapon ? unit.Body.PrimaryHand.MaybeWeapon : unit.Body.EmptyHandWeapon;
            if (weapon == null)
            {
                return;
            }

            if (!allowed_types.Empty() && !allowed_types.Contains(weapon.Blueprint.Type))
            {
                return;
            }
            int bonus = getRemainingGroupSize() - 1;
            if (bonus < 0)
            {
                return;
            }

            if (bonus >= enchantments.Length)
            {
                bonus = enchantments.Length - 1;
            }


            if (weapon.Enchantments.HasFact(enchantments[bonus]))
            {
                return;
            }

            if (weapon.EnchantmentValue != 0 && only_non_magical)
            {
                return;
            }
            m_Enchantment = weapon.AddEnchantment(enchantments[bonus], Context, new Rounds?());



            if (lock_slot && !weapon.IsNonRemovable)
            {
                weapon.IsNonRemovable = true;
                m_unlock = true;
            }
            //m_Enchantment.RemoveOnUnequipItem = remove_on_unequip;
            m_Weapon = weapon;
        }

        public override void OnFactDeactivate()
        {
            if (this.m_Weapon == null)
                return;
            //m_Weapon.IsNonRemovable = false;
            if (m_unlock)
            {
                m_Weapon.IsNonRemovable = false;
            }
            if (this.m_Enchantment == null)
                return;
            this.m_Enchantment.Owner?.RemoveEnchantment(this.m_Enchantment);
        }
    }


    public class BuffContextEnchantPrimaryHandWeapon : BuffLogic
    {
        public BlueprintWeaponEnchantment[] enchantments;
        public ContextValue value;
        public BlueprintWeaponType[] allowed_types;
        public bool lock_slot = false;
        public bool only_non_magical = false;
        [JsonProperty]
        private ItemEnchantment m_Enchantment;
        [JsonProperty]
        private ItemEntityWeapon m_Weapon;
        [JsonProperty]
        private bool m_unlock;


        public override void OnFactActivate()
        {
            m_unlock = false;
            var unit = this.Owner;
            if (unit == null) return;

            var weapon = unit.Body.PrimaryHand.HasWeapon ? unit.Body.PrimaryHand.MaybeWeapon : unit.Body.EmptyHandWeapon;
            if (weapon == null)
            {
                return;
            }

            if (!allowed_types.Empty() && !allowed_types.Contains(weapon.Blueprint.Type))
            {
                return;
            }

            int bonus = value.Calculate(Context) - 1;
            if (bonus < 0)
            {
                bonus = 0;
            }
            if (bonus >= enchantments.Length)
            {
                bonus = enchantments.Length - 1;
            }

            if (weapon.Enchantments.HasFact(enchantments[bonus]))
            {
                return;
            }

            if (weapon.EnchantmentValue != 0 && only_non_magical)
            {
                return;
            }
            m_Enchantment = weapon.AddEnchantment(enchantments[bonus], Context, new Rounds?());

            if (lock_slot && !weapon.IsNonRemovable)
            {
                weapon.IsNonRemovable = true;
                m_unlock = true;
            }
            //m_Enchantment.RemoveOnUnequipItem = remove_on_unequip;
            m_Weapon = weapon;
        }

        public override void OnFactDeactivate()
        {
            if (this.m_Weapon == null)
                return;
            //m_Weapon.IsNonRemovable = false;
            if (m_unlock)
            {
                m_Weapon.IsNonRemovable = false;
            }
            if (this.m_Enchantment == null)
                return;
            this.m_Enchantment.Owner?.RemoveEnchantment(this.m_Enchantment);
        }
    }



    public class BuffContextEnchantPrimaryHandWeaponIfHasMetamagic : BuffLogic
    {
        public BlueprintWeaponEnchantment enchantment;
        public Metamagic metamagic;
        public BlueprintWeaponType[] allowed_types;
        public bool lock_slot = false;
        public bool only_non_magical = false;
        [JsonProperty]
        private ItemEnchantment m_Enchantment;
        [JsonProperty]
        private ItemEntityWeapon m_Weapon;
        [JsonProperty]
        private bool m_unlock;


        public override void OnFactActivate()
        {
            m_unlock = false;
            var unit = this.Owner;
            if (unit == null) return;

            var weapon = unit.Body.PrimaryHand.HasWeapon ? unit.Body.PrimaryHand.MaybeWeapon : unit.Body.EmptyHandWeapon;
            if (weapon == null)
            {
                return;
            }

            if (!allowed_types.Empty() && !allowed_types.Contains(weapon.Blueprint.Type))
            {
                return;
            }

            if (!Context.HasMetamagic(metamagic))
            {
                return;
            }

            if (weapon.Enchantments.HasFact(enchantment))
            {
                return;
            }

            /*var fact = weapon.Enchantments.Find(x => x.Blueprint == enchantment);
            if (fact != null)
            {
                weapon.RemoveEnchantment(fact);
            }*/

            if (weapon.EnchantmentValue != 0 && only_non_magical)
            {
                return;
            }

            m_Enchantment = weapon.AddEnchantment(enchantment, Context, new Rounds?());

            if (lock_slot && !weapon.IsNonRemovable)
            {
                weapon.IsNonRemovable = true;
                m_unlock = true;
            }
            //m_Enchantment.RemoveOnUnequipItem = remove_on_unequip;
            m_Weapon = weapon;
        }

        public override void OnFactDeactivate()
        {
            if (this.m_Weapon == null)
                return;
            //m_Weapon.IsNonRemovable = false;
            if (m_unlock)
            {
                m_Weapon.IsNonRemovable = false;
            }
            if (this.m_Enchantment == null)
                return;
            this.m_Enchantment.Owner?.RemoveEnchantment(this.m_Enchantment);
        }
    }


    public class BuffContextEnchantShield : BuffLogic
    {
        public BlueprintArmorEnchantment[] enchantments;
        public ContextValue value;
        public bool lock_slot = false;
        [JsonProperty]
        private bool m_unlock;
        [JsonProperty]
        private ItemEnchantment m_Enchantment;
        [JsonProperty]
        private ItemEntityShield m_Shield;

        public override void OnFactActivate()
        {
            m_unlock = false;
            var unit = this.Owner;
            if (unit == null) return;

            var shield = unit.Body.SecondaryHand.MaybeShield;
            if (shield == null)
            {
                return;
            }

            int bonus = value.Calculate(Context) - 1;
            if (bonus < 0)
            {
                bonus = 0;
            }
            if (bonus >= enchantments.Length)
            {
                bonus = enchantments.Length - 1;
            }

            if (shield.ArmorComponent.Enchantments.HasFact(enchantments[bonus]))
            {
                return;
            }

            m_Enchantment = shield.ArmorComponent.AddEnchantment(enchantments[bonus], Context, new Rounds?());
            shield.ArmorComponent.RecalculateStats();
            m_Shield = shield;
            if (lock_slot && !shield.IsNonRemovable)
            {
                shield.IsNonRemovable = true;
                m_unlock = true;
            }
        }

        public override void OnFactDeactivate()
        {
            if (this.m_Enchantment == null)
                return;
            this.m_Enchantment.Owner?.RemoveEnchantment(this.m_Enchantment);
            if (m_Shield != null)
            {
                m_Shield.ArmorComponent.RecalculateStats();
            }
            else
            {
                return;
            }
            if (m_unlock)
            {
                m_Shield.IsNonRemovable = false;
            }
        }
    }


    public class BuffRemainingGroupSizetEnchantArmor : BuffLogic
    {
        public BlueprintArmorEnchantment[] enchantments;
        public ActivatableAbilityGroup group;
        public bool shift_with_current_enchantment = true;
        public ContextValue value;
        public bool lock_slot = false;
        public bool only_non_magical = false;
        [JsonProperty]
        private bool m_unlock;
        [JsonProperty]
        private ItemEnchantment m_Enchantment;
        [JsonProperty]
        private ItemEntityArmor m_Armor;


        private int getRemainingGroupSize()
        {
            int remaining_group_size = Context.MaybeCaster.Ensure<UnitPartActivatableAbility>().GetGroupSize(this.group);

            foreach (var a in Owner.ActivatableAbilities)
            {
                if (a.Blueprint.Group == group && a.IsOn)
                {
                    remaining_group_size -= a.Blueprint.WeightInGroup;
                }
            }
            return remaining_group_size;
        }

        public override void OnFactActivate()
        {
            m_unlock = false;
            var unit = this.Owner;
            if (unit == null) return;

            var armor = unit.Body.Armor.MaybeArmor;
            if (armor == null) return;

            int bonus = getRemainingGroupSize() - 1;
            if (bonus < 0)
            {
                return;
            }

            if (shift_with_current_enchantment)
            {
                bonus += GameHelper.GetItemEnhancementBonus(armor);
            }

            if (bonus >= enchantments.Length)
            {
                bonus = enchantments.Length - 1;
            }

            if (armor.Enchantments.HasFact(enchantments[bonus]))
            {
                return;
            }


            m_Enchantment = armor.AddEnchantment(enchantments[bonus], Context, new Rounds?());

            armor.RecalculateStats();
            m_Armor = armor;
            if (lock_slot && !armor.IsNonRemovable)
            {
                armor.IsNonRemovable = true;
                m_unlock = true;
            }
        }

        public override void OnFactDeactivate()
        {
            if (this.m_Enchantment == null)
                return;
            this.m_Enchantment.Owner?.RemoveEnchantment(this.m_Enchantment);
            if (m_Armor != null)
            {
                m_Armor.RecalculateStats();
            }
            else
            {
                return;
            }
            if (m_unlock)
            {
                m_Armor.IsNonRemovable = false;
            }
        }
    }


    public class BuffContextEnchantArmor : BuffLogic
    {
        public BlueprintArmorEnchantment[] enchantments;
        public ContextValue value;
        public bool lock_slot = false;
        public bool only_non_magical = false;
        [JsonProperty]
        private bool m_unlock;
        [JsonProperty]
        private ItemEnchantment m_Enchantment;
        [JsonProperty]
        private ItemEntityArmor m_Armor;

        public override void OnFactActivate()
        {
            m_unlock = false;
            var unit = this.Owner;
            if (unit == null) return;

            var armor = unit.Body.Armor.MaybeArmor;
            if (armor == null) return;

            int bonus = value.Calculate(Context) - 1;
            if (bonus < 0)
            {
                bonus = 0;
            }
            if (bonus >= enchantments.Length)
            {
                bonus = enchantments.Length - 1;
            }

            if (armor.Enchantments.HasFact(enchantments[bonus]))
            {
                return;
            }

            m_Enchantment = armor.AddEnchantment(enchantments[bonus], Context, new Rounds?());

            armor.RecalculateStats();
            m_Armor = armor;
            if (lock_slot && !armor.IsNonRemovable)
            {
                armor.IsNonRemovable = true;
                m_unlock = true;
            }
        }

        public override void OnFactDeactivate()
        {
            if (this.m_Enchantment == null)
                return;
            this.m_Enchantment.Owner?.RemoveEnchantment(this.m_Enchantment);
            if (m_Armor != null)
            {
                m_Armor.RecalculateStats();
            }
            else
            {
                return;
            }
            if (m_unlock)
            {
                m_Armor.IsNonRemovable = false;
            }
        }
    }

    [ComponentName("Remove Weapon Damage Stat")]
    public class Immaterial : WeaponEnchantmentLogic, IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleAttackRoll>, IRulebookHandler<RuleAttackRoll>
    {
        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (this.Owner.Wielder == null || evt.Weapon != this.Owner)
                return;
            Harmony12.Traverse.Create(evt).Property("DamageBonusStat").SetValue(new StatType?());
            evt.DoNotScaleDamage = true;
            //Helpers.SetField(evt, "DamageBonusStat", new StatType?());
        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt)
        {
        }


        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {

            evt.AttackType = AttackType.Touch;
        }

        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
        }
    }



    [ComponentName("Metamagic effect on weapon damage")]
    public class WeaponMetamagicDamage : WeaponEnchantmentLogic, IInitiatorRulebookHandler<RulePrepareDamage>, IRulebookHandler<RulePrepareDamage>, IInitiatorRulebookSubscriber
    {
        public bool maximize = false;
        public bool empower = false;

        public void OnEventAboutToTrigger(RulePrepareDamage evt)
        {

            if (evt.DamageBundle.Count() == 0 || evt.DamageBundle.Weapon != this.Owner)
            {
                return;
            }

            if (empower)
            {
                evt.DamageBundle.First().EmpowerBonus = 1.5f;
            }

            if (maximize)
            {
                evt.DamageBundle.First().Maximized = true;
            }

        }

        public void OnEventDidTrigger(RulePrepareDamage evt)
        {
        }
    }


    [ComponentName("Weapon Attack Stat Replacement")]
    public class WeaponAttackStatReplacement : WeaponEnchantmentLogic, IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>, IRulebookHandler<RuleCalculateAttackBonusWithoutTarget>, IInitiatorRulebookSubscriber
    {
        public StatType Stat;

        public void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
            if (evt.Weapon != this.Owner)
            {
                return;
            }
            evt.AttackBonusStat = Stat;
        }

        public void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
        }
    }


    [ComponentName("Weapon Damage Stat Replacement")]
    public class WeaponDamageStatReplacement : WeaponEnchantmentLogic, IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookSubscriber
    {
        public StatType Stat;

        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (this.Owner.Wielder == null || evt.Weapon != this.Owner)
                return;
            evt.OverrideDamageBonusStat(this.Stat);
        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt)
        {
        }
    }

    [ComponentName("Ignore Damage Reduction if target has fact")]
    public class WeaponIgnoreDRIfTargetHasFact : WeaponEnchantmentLogic, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, IInitiatorRulebookSubscriber
    {
        public BlueprintUnitFact fact;

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
            if (evt.Reason.Item != this.Owner)
            {
                return;
            }

            if (evt.Target.Descriptor.HasFact(fact))
            {
                evt.IgnoreDamageReduction = true;
            }
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
        }
    }


    [ComponentName("change weapon damage")]
    public class WeaponDamageChange : WeaponEnchantmentLogic, IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookSubscriber
    {
        public DiceFormula dice_formula;
        public int bonus_damage;
        public DamageTypeDescription damage_type_description = null;

        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (evt.Weapon != this.Owner)
            {
                return;
            }
            evt.WeaponDamageDiceOverride = dice_formula;
            evt.AddBonusDamage(bonus_damage);
        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt)
        {
            if (evt.Weapon != this.Owner)
            {
                return;
            }
            if (damage_type_description != null && evt.DamageDescription.Count() > 0)
            {
                evt.DamageDescription[0].TypeDescription = damage_type_description;
            }
        }
    }
}
