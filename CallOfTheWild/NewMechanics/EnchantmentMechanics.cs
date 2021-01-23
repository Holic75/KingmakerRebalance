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
using Kingmaker.View.Equipment;
using Kingmaker.Items.Slots;
using Kingmaker.Blueprints.Items.Equipment;

namespace CallOfTheWild.NewMechanics.EnchantmentMechanics
{

    class UnitPartMenacing : AdditiveUnitPart
    {
        public bool isActive()
        {
            return !buffs.Empty();
        }
    }


    public class Menaced : OwnedGameLogicComponent<UnitDescriptor>
    {
        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartMenacing>().addBuff(this.Fact);

        }

        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartMenacing>().removeBuff(this.Fact);
        }
    }

    [Harmony12.HarmonyPatch(typeof(RuleCalculateAttackBonus))]
    [Harmony12.HarmonyPatch("OnTrigger", Harmony12.MethodType.Normal)]
    class RuleCalculateAttackBonus__OnTrigger__MenacingPostfix
    {
        static void Postfix(RuleCalculateAttackBonus __instance, RulebookEventContext context)
        {
            if (__instance.FlankingBonus == 0)
            {
                return;
            }
            if (!(__instance.Target.Get<UnitPartMenacing>()?.isActive()).GetValueOrDefault())
            {
                return;
            }
            __instance.FlankingBonus += 2;

            var tr = Harmony12.Traverse.Create(__instance);
            tr.Property("Result").SetValue(__instance.Result + 2);
        }
    }




    class WeaponEnchantmentPropertyGetter : PropertyValueGetter
    {
        internal static readonly Lazy<BlueprintUnitProperty> Blueprint = new Lazy<BlueprintUnitProperty>(() =>
        {
            var p = CallOfTheWild.Helpers.Create<BlueprintUnitProperty>();
            p.name = "WeaponEnchantmentCustomProperty";
            Main.library.AddAsset(p, "804bbccf9985428ea25e01f17e8a5239");
            p.SetComponents(CallOfTheWild.Helpers.Create<WeaponEnchantmentPropertyGetter>());
            return p;
        });

        public override int GetInt(UnitEntityData unit)
        {
            int value = 0;
            var primary = unit.Body?.PrimaryHand?.MaybeWeapon;
            var secondary = unit.Body?.PrimaryHand?.MaybeWeapon;
            if (primary != null)
            {
                value = Math.Max(value, GameHelper.GetItemEnhancementBonus(primary));
            }
            if (secondary != null)
            {
                value = Math.Max(value, GameHelper.GetItemEnhancementBonus(secondary));
            }
            
            return value;
        }
    }



    [AllowedOn(typeof(BlueprintUnitFact))]
    [AllowMultipleComponents]
    public class RecalculateOnEquipmentChange : OwnedGameLogicComponent<UnitDescriptor>, IUnitActiveEquipmentSetHandler, IUnitEquipmentHandler, IGlobalSubscriber
    {
        public void HandleUnitChangeActiveEquipmentSet(UnitDescriptor unit)
        {
            this.Fact.Recalculate();
        }

        public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
        {
            this.Fact.Recalculate();
        }
    }

    public class StaticWeaponEnhancementBonus : WeaponEnchantmentLogic, IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookHandler<RuleCalculateDamage>, IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>, IRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookSubscriber, IRulebookHandler<RuleCalculateAttackBonusWithoutTarget>
    {
        [JsonProperty]
        private int added_bonus = 0;

        public int EnhancementBonus;

        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (evt.Weapon != this.Owner)
                return;
       
            int bonus = EnhancementBonus - evt.Enhancement;
            added_bonus = bonus > 0 ? bonus : 0;

            evt.AddBonusDamage(added_bonus);
            evt.Enhancement += added_bonus;
            evt.EnhancementTotal += added_bonus;
        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt)
        {
        }

        public void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
            if (evt.Weapon != this.Owner)
                return;
            evt.AddBonus(added_bonus, this.Fact);
        }

        public void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
        }

        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {
            /*ItemEntityWeapon weapon = evt.DamageBundle.Weapon;
            if (weapon != this.Owner)
                return;

            foreach (BaseDamage base_dmg in evt.DamageBundle)
            {
                var physical_dmg = (base_dmg as PhysicalDamage);
                if (physical_dmg == null)
                {
                    continue;
                }
                physical_dmg.Enchantment += added_bonus;
                physical_dmg.EnchantmentTotal += added_bonus;
            }*/
        }

        public void OnEventDidTrigger(RuleCalculateDamage evt)
        {

        }
    }


    public class StaticEquipmentWeaponTypeEnhancement : EquipmentEnchantmentLogic, IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookHandler<RuleCalculateDamage>, IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>, IRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookSubscriber, IRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateAttackBonusWithoutTarget>
    {
        public int Enhancement;
        public bool AllNaturalAndUnarmed;
        [HideIf("AllNaturalAndUnarmed")]
        public WeaponCategory Category;
        [JsonProperty]
        private int added_bonus = 0;

        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (!this.CheckWeapon(evt.Weapon))
                return;

            int bonus = Enhancement - evt.Enhancement;
            added_bonus = bonus > 0 ? bonus : 0;

            evt.AddBonusDamage(added_bonus);
            evt.Enhancement += added_bonus;
            evt.EnhancementTotal += added_bonus;
        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt)
        {
        }

        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {
            /*ItemEntityWeapon weapon = evt.DamageBundle.Weapon;
            if (!CheckWeapon(weapon))
            {
                return;
            }

            foreach (BaseDamage base_dmg in evt.DamageBundle)
            {
                var physical_dmg = (base_dmg as PhysicalDamage);
                if (physical_dmg == null)
                {
                    continue;
                }
                physical_dmg.EnchantmentTotal += added_bonus;
            }*/
        }

        public void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
            if (evt.Weapon == null || !this.CheckWeapon(evt.Weapon))
                return;
            evt.AddBonus(added_bonus, this.Fact);
        }

        public void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
        }

        public void OnEventDidTrigger(RuleCalculateDamage evt)
        {
        }

        public bool CheckWeapon(ItemEntityWeapon weapon)
        {
            if (!this.AllNaturalAndUnarmed || !weapon.Blueprint.IsNatural && !weapon.Blueprint.IsUnarmed)
                return weapon.Blueprint.Category == this.Category;
            return true;
        }
    }


    public class EquipmentWeaponTypeEnhancement : EquipmentEnchantmentLogic, IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookHandler<RuleCalculateDamage>, IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>, IRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookSubscriber, IRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateAttackBonusWithoutTarget>
    {
        public int Enhancement;
        public bool AllNaturalAndUnarmed;
        [HideIf("AllNaturalAndUnarmed")]
        public WeaponCategory Category;

        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (!this.CheckWeapon(evt.Weapon))
                return;

            evt.AddBonusDamage(Enhancement);
            evt.Enhancement += Enhancement;
            evt.EnhancementTotal += Enhancement;
        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt)
        {
        }

        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {
            /*ItemEntityWeapon weapon = evt.DamageBundle.Weapon;
            if (!CheckWeapon(weapon))
            {
                return;
            }

            foreach (BaseDamage base_dmg in evt.DamageBundle)
            {
                var physical_dmg = (base_dmg as PhysicalDamage);
                if (physical_dmg == null)
                {
                    continue;
                }
                physical_dmg.EnchantmentTotal += Enhancement;
            }*/
        }

        public void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
            if (evt.Weapon == null || !this.CheckWeapon(evt.Weapon))
                return;
            evt.AddBonus(Enhancement, this.Fact);
        }

        public void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
        }

        public void OnEventDidTrigger(RuleCalculateDamage evt)
        {
        }

        public bool CheckWeapon(ItemEntityWeapon weapon)
        {
            if (!this.AllNaturalAndUnarmed || !weapon.Blueprint.IsNatural && !weapon.Blueprint.IsUnarmed)
                return weapon.Blueprint.Category == this.Category;
            return true;
        }
    }



    public class BuffRemainingGroupsSizeEnchantPrimaryHandWeapon : BuffLogic
    {
        public ActivatableAbilityGroup group;
        public BlueprintWeaponEnchantment[] enchantments;
        public BlueprintWeaponType[] allowed_types;
        public bool lock_slot = false;
        public bool only_non_magical = false;
        public bool in_off_hand = false;
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

            ItemEntityWeapon weapon = null;
            if (!in_off_hand)
            {
                weapon = unit.Body.PrimaryHand.HasWeapon ? unit.Body.PrimaryHand.MaybeWeapon : unit.Body.EmptyHandWeapon;
            }
            else
            {
                weapon = unit.Body.SecondaryHand.HasWeapon ? unit.Body.SecondaryHand.MaybeWeapon : null;
            }
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
        public bool in_off_hand = false;
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

            ItemEntityWeapon weapon = null;
            if (!in_off_hand)
            {
                weapon = unit.Body.PrimaryHand.HasWeapon ? unit.Body.PrimaryHand.MaybeWeapon : unit.Body.EmptyHandWeapon;
            }
            else
            {
                weapon = unit.Body.SecondaryHand.HasWeapon ? unit.Body.SecondaryHand.MaybeWeapon : null;
            }
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


    public class BuffContextEnhancePrimaryHandWeaponUpToValue : BuffLogic
    {
        public ContextValue value;
        public BlueprintWeaponType[] allowed_types;
        public bool lock_slot = false;
        public bool only_non_magical = false;
        public bool in_off_hand = false;
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
            ItemEntityWeapon weapon = null;
            if (!in_off_hand)
            {
                weapon = unit.Body.PrimaryHand.HasWeapon ? unit.Body.PrimaryHand.MaybeWeapon : unit.Body.EmptyHandWeapon;
            }
            else
            {
                weapon = unit.Body.SecondaryHand.HasWeapon ? unit.Body.SecondaryHand.MaybeWeapon : null;
            }
            if (weapon == null)
            {
                return;
            }

            if (!allowed_types.Empty() && !allowed_types.Contains(weapon.Blueprint.Type))
            {
                return;
            }

            int bonus = value.Calculate(Context) - 1;// - GameHelper.GetItemEnhancementBonus(weapon);
            if (bonus < 0)
            {
                return;
            }
            if (bonus >= 5)
            {
                bonus = 4;
            }

            m_Enchantment = weapon.AddEnchantment(WeaponEnchantments.static_enchants[bonus], Context, new Rounds?());

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
        public bool in_off_hand = false;
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

            ItemEntityWeapon weapon = null;
            if (!in_off_hand)
            {
                weapon = unit.Body.PrimaryHand.HasWeapon ? unit.Body.PrimaryHand.MaybeWeapon : unit.Body.EmptyHandWeapon;
            }
            else
            {
                weapon = unit.Body.SecondaryHand.HasWeapon ? unit.Body.SecondaryHand.MaybeWeapon : null;
            }
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


    public class WeaponDamageBonus : WeaponEnchantmentLogic, IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookSubscriber
    {
        public int damage;
        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (evt.Weapon != this.Owner)
                return;
            evt.AddBonusDamage(damage);
        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt)
        {
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
            if (this.Owner.Wielder == null || evt.Weapon != this.Owner)
                return;
            evt.AttackType = AttackType.Touch;
        }

        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
        }
    }


    [ComponentName("no damage scaling")]
    public class NoDamageScalingEnchant : WeaponEnchantmentLogic, IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookSubscriber
    {
        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (this.Owner.Wielder == null || evt.Weapon != this.Owner)
                return;
            evt.DoNotScaleDamage = true;
        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt)
        {
        }
    }


    [ComponentName("do not provoke aoo")]
    public class DoNotProvokeAooEnchant : WeaponEnchantmentLogic, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleAttackRoll>, IRulebookHandler<RuleAttackRoll>
    {
        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (this.Owner.Wielder == null || evt.Weapon != this.Owner)
                return;
            evt.DoNotProvokeAttacksOfOpportunity = true;
        }

        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
        }
    }


    public class ActionOnAttackWithEnchantedWeapon : WeaponEnchantmentLogic, IInitiatorRulebookHandler<RuleAttackWithWeapon>, IInitiatorRulebookHandler<RuleAttackWithWeaponResolve>, IRulebookHandler<RuleAttackWithWeapon>, IInitiatorRulebookSubscriber, IRulebookHandler<RuleAttackWithWeaponResolve>
    {
        public ActionList ActionsOnSelf = null;
        public ActionList ActionsOnTarget = null;
        public bool only_on_hit = false;

        public void OnEventAboutToTrigger(RuleAttackWithWeaponResolve evt)
        {

        }

        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
        {

        }

        public void OnEventDidTrigger(RuleAttackWithWeaponResolve evt)
        {
            if (this.Owner.Wielder == null || evt.AttackWithWeapon.Weapon != this.Owner)
                return;
            if (!evt.AttackRoll.IsHit && only_on_hit)
            {
                return;
            }
            TryRunActions(evt.AttackWithWeapon);
        }

        public void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
        }

        private void TryRunActions(RuleAttackWithWeapon evt)
        {
            using (new ContextAttackData(evt.AttackRoll, (Projectile)null))
            {
                if (ActionsOnSelf != null)
                {
                    (this.Fact as IFactContextOwner)?.RunActionInContext(this.ActionsOnSelf, (TargetWrapper)evt.Initiator);
                }
                if (ActionsOnTarget != null)
                {
                    (this.Fact as IFactContextOwner)?.RunActionInContext(this.ActionsOnTarget, (TargetWrapper)evt.Target);
                }
            }
        }
    }


    [ComponentName("ranged touch attack")]
    public class RangedTouchEnchant : WeaponEnchantmentLogic, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleAttackRoll>, IRulebookHandler<RuleAttackRoll>
    {
        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (this.Owner.Wielder == null || evt.Weapon != this.Owner)
                return;
            evt.AttackType = AttackType.RangedTouch;
        }

        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
        }
    }


    [ComponentName("touch attack")]
    public class TouchEnchant : WeaponEnchantmentLogic, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleAttackRoll>, IRulebookHandler<RuleAttackRoll>
    {
        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (this.Owner.Wielder == null || evt.Weapon != this.Owner)
                return;
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
            if (this.Owner.Wielder == null || evt.Reason.Item != this.Owner)
                return;

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


    [ComponentName("change weapon damage")]
    public class AddWeaponDamageMaterial : WeaponEnchantmentLogic, IInitiatorRulebookHandler<RulePrepareDamage>, IRulebookHandler<RulePrepareDamage>, IInitiatorRulebookSubscriber
    {
        public PhysicalDamageMaterial material = PhysicalDamageMaterial.Adamantite;

        public void OnEventAboutToTrigger(RulePrepareDamage evt)
        {
            ItemEntityWeapon weapon = evt.DamageBundle.Weapon;
            if (weapon != this.Owner)
            {
                return;
            }

            foreach (BaseDamage baseDamage in evt.DamageBundle)
            {
                var physical_dmg = baseDamage as PhysicalDamage;
                physical_dmg?.AddMaterial(material);
            }
        }

        public void OnEventDidTrigger(RulePrepareDamage evt)
        {

            
        }
    }


    [AllowedOn(typeof(BlueprintBuff))]
    public class CreateWeapon : BuffLogic
    {
        public BlueprintItemWeapon weapon;
        public bool disable_aoo = false;
        public bool create_in_offhand = false;
        [JsonProperty]
        private ItemEntityWeapon m_Applied;

        private int off_hand_unlock;
        private int main_hand_unlock;

        public override void OnFactActivate()
        {
            base.OnFactActivate();
            this.m_Applied = this.weapon.CreateEntity<ItemEntityWeapon>();
            this.m_Applied.MakeNotLootable();
            maybeUnlockSlots();
            if (!canInsert(this.m_Applied))
            {
                this.m_Applied = null;
                this.Buff.Remove();
            }
            else
            {
                if (disable_aoo)
                {
                    this.Owner.State.AddCondition(UnitCondition.DisableAttacksOfOpportunity, (Buff)null);
                }

                
                if (!create_in_offhand)
                {
                    ItemsCollection.DoWithoutEvents((Action)(() => this.Owner.Body.PrimaryHand.InsertItem((ItemEntity)this.m_Applied)));
                    this.Owner.Body.PrimaryHand.Lock.Retain();
                }
                else
                {
                    ItemsCollection.DoWithoutEvents((Action)(() => this.Owner.Body.SecondaryHand.InsertItem((ItemEntity)this.m_Applied)));
                    this.Owner.Body.SecondaryHand.Lock.Retain();
                }
            }

            maybeRelockSlots();
        }

        bool canInsert(ItemEntityWeapon weapon)
        {
            if (create_in_offhand)
            {
                return this.Owner.Body.SecondaryHand.CanInsertItem(weapon);
            }
            else
            {
                return this.Owner.Body.PrimaryHand.CanInsertItem(weapon);
            }
        }

        public override void OnFactDeactivate()
        {
            base.OnFactDeactivate();
            if (this.m_Applied == null)
                return;

            if (disable_aoo)
            {
                this.Owner.State.RemoveCondition(UnitCondition.DisableAttacksOfOpportunity);
            }
            this.m_Applied.HoldingSlot?.Lock.Release();
            this.m_Applied.HoldingSlot?.RemoveItem();
            ItemsCollection.DoWithoutEvents((Action)(() => this.m_Applied.Collection?.Remove((ItemEntity)this.m_Applied)));
            this.m_Applied = (ItemEntityWeapon)null;
        }


        private void maybeUnlockSlots()
        {
            while (this.Owner.Body.SecondaryHand.Lock.Count > 0)
            {
                this.Owner.Body.SecondaryHand.Lock.Release();
                off_hand_unlock++;
            }
            while (this.Owner.Body.PrimaryHand.Lock.Count > 0)
            {
                this.Owner.Body.PrimaryHand.Lock.Release();
                main_hand_unlock++;
            }
        }


        private void maybeRelockSlots()
        {
            while (off_hand_unlock > 0)
            {
                off_hand_unlock--;
                this.Owner.Body.SecondaryHand.Lock.Retain();
            }
            while (main_hand_unlock > 0)
            {
                main_hand_unlock--;
                this.Owner.Body.PrimaryHand.Lock.Retain();
            }
        }
    }


    [ComponentName("Persistent weapon enchantment")]
    [AllowedOn(typeof(BlueprintUnitFact))]
    [AllowMultipleComponents]
    public class PersistentWeaponEnchantment : OwnedGameLogicComponent<UnitDescriptor>, IUnitActiveEquipmentSetHandler, IUnitEquipmentHandler, IGlobalSubscriber
    {
        public BlueprintWeaponEnchantment enchant;
        public bool primary_hand = true;
        public bool secondary_hand = true;
        public bool only_melee = false;

        [JsonProperty]
        private ItemEnchantment m_PrimaryHandEnchantment = null;
        [JsonProperty]
        private ItemEnchantment m_SecondaryHandEnchantment = null;
        [JsonProperty]
        private ItemEntityWeapon m_PriamryHandWeapon;
        [JsonProperty]
        private ItemEntityWeapon m_SecondaryHandWeapon;


        public override void OnTurnOn()
        {
            this.checkWeapons();
        }

        public override void OnTurnOff()
        {
            this.deactivateEnchants();
        }

        public void HandleUnitChangeActiveEquipmentSet(UnitDescriptor unit)
        {
            this.checkWeapons();
        }

        private void checkWeapons()
        {
            deactivateEnchants();
            var primary_hand_weapon = this.Owner?.Body?.PrimaryHand?.MaybeWeapon;
            var secondary_hand_weapon = this.Owner?.Body?.SecondaryHand?.MaybeWeapon;

            if (primary_hand && primary_hand_weapon != null && !primary_hand_weapon.Enchantments.HasFact(enchant) 
                && (primary_hand_weapon.Blueprint.IsMelee || !only_melee))
            {
                m_PrimaryHandEnchantment = primary_hand_weapon.AddEnchantment(enchant, this.Fact.MaybeContext, new Rounds?());
                m_PrimaryHandEnchantment.RemoveOnUnequipItem = true;
                m_PriamryHandWeapon = primary_hand_weapon;
            }
            if (secondary_hand && secondary_hand_weapon != null && !secondary_hand_weapon.Enchantments.HasFact(enchant)
                && (secondary_hand_weapon.Blueprint.IsMelee || !only_melee))
            {
                m_SecondaryHandEnchantment = secondary_hand_weapon.AddEnchantment(enchant, this.Fact.MaybeContext, new Rounds?());
                m_SecondaryHandEnchantment.RemoveOnUnequipItem = true;
                m_SecondaryHandWeapon = secondary_hand_weapon;
            }
        }

        private void deactivateEnchants()
        {
            if (m_PrimaryHandEnchantment != null && m_PriamryHandWeapon != null)
            {
                m_PrimaryHandEnchantment.Owner?.RemoveEnchantment(this.m_PrimaryHandEnchantment);
                m_PrimaryHandEnchantment = null;
                m_PriamryHandWeapon = null;
            }

            if (m_SecondaryHandEnchantment != null && m_SecondaryHandWeapon != null)
            {
                m_SecondaryHandEnchantment.Owner?.RemoveEnchantment(this.m_SecondaryHandEnchantment);
                m_SecondaryHandEnchantment = null;
                m_SecondaryHandWeapon = null;
            }
        }

        public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
        {
            if (slot.Owner != this.Owner)
                return;
            this.checkWeapons();
        }
    }



    [ComponentName("transfer enchants to polymorph")]
    [AllowedOn(typeof(BlueprintUnitFact))]
    [AllowMultipleComponents]
    public class TransferPrimaryHandWeaponEnchantsToPolymorph : OwnedGameLogicComponent<UnitDescriptor>, WildArmorMechanics.IPolymorphOnHandler,
                                                                  /*IUnitActiveEquipmentSetHandler, IUnitEquipmentHandler*/ IGlobalSubscriber
    {
        public enum TransferType
        {
            Any,
            Only,
            Except
        }

        public bool transfer_enhancement = false;

        public BlueprintWeaponEnchantment[] enchants = new BlueprintWeaponEnchantment[0];
        public TransferType transfer_type;

        [JsonProperty]
        private List<ItemEnchantment> m_enchants = new List<ItemEnchantment>();


        public override void OnTurnOn()
        {
            this.checkWeapons();
        }

        public override void OnTurnOff()
        {
            this.deactivateEnchants();
        }

        /*public void HandleUnitChangeActiveEquipmentSet(UnitDescriptor unit)
        {
            this.checkWeapons();
        }*/

        private void checkWeapons()
        {
            deactivateEnchants();
            if (!this.Owner.Unit.Body.IsPolymorphed)
            {
                return;
            }

            var primary_hand_weapon = this.Owner?.Body?.CurrentHandsEquipmentSet?.PrimaryHand?.MaybeItem as ItemEntityWeapon;
            
            if (primary_hand_weapon == null || !primary_hand_weapon.Blueprint.IsMelee || Helpers.isSummoned(primary_hand_weapon))
            {
                return;
            }

            var primary_hand_enchants = primary_hand_weapon.Enchantments;
            if (primary_hand_enchants == null)
            {
                return;
            }

            List<ItemEntityWeapon> weapons = new List<ItemEntityWeapon>();

            var primary_weapon = this.Owner?.Body?.PrimaryHand?.MaybeWeapon;
            var secondary_weapon = this.Owner?.Body?.SecondaryHand?.MaybeWeapon;

            if (primary_weapon!= null && this.Owner.Body.PrimaryHand.HasWeapon)
            {
                weapons.Add(primary_weapon);
            }


            if (secondary_weapon != null && Owner.Body.SecondaryHand.HasWeapon)
            {
                weapons.Add(secondary_weapon);
            }

            foreach (var limb in this.Owner.Body.AdditionalLimbs)
            {
                var weapon = limb?.Item as ItemEntityWeapon;
                if (weapon != null)
                {
                    weapons.Add(weapon);
                }
            }


            foreach (var e in primary_hand_enchants)
            {
                var blueprint = e.Blueprint;
                if (transfer_type == TransferType.Any
                    || (transfer_type == TransferType.Only && enchants.Contains(blueprint))
                    || (transfer_type == TransferType.Except && !enchants.Contains(blueprint)))
                {
                    foreach (var w in weapons)
                    {
                        if (w.Enchantments.HasFact(blueprint))
                        {
                            continue;
                        }
                        var new_enchant = w.AddEnchantment(blueprint, this.Fact.MaybeContext, new Rounds?());
                        new_enchant.RemoveOnUnequipItem = true;
                        //Main.logger.Log("Added " + new_enchant.Blueprint.name + "to " + w.Blueprint.Name);
                        m_enchants.Add(new_enchant);
                    }
                }
            }

            if (transfer_enhancement)
            {
                var enchancement_bonus = GameHelper.GetItemEnhancementBonus(primary_hand_weapon);
                
                if (enchancement_bonus <= 0)
                {
                    return;
                }
                else
                {
                    enchancement_bonus = Math.Min(4, enchancement_bonus - 1);
                }

                foreach (var w in weapons)
                {
                    var new_enchant = w.AddEnchantment(WeaponEnchantments.static_enchants[enchancement_bonus], this.Fact.MaybeContext, new Rounds?());
                    new_enchant.RemoveOnUnequipItem = true;
                    m_enchants.Add(new_enchant);
                }
            }
        }

        private void deactivateEnchants()
        {
            foreach (var e in m_enchants)
            {
                e.Owner?.RemoveEnchantment(e);
            }
            m_enchants = new List<ItemEnchantment>();
        }

        public void polymorphOn(UnitDescriptor owner)
        {
           if (owner != this.Owner)
           {
                return;
           }
           this.checkWeapons();
        }
    }


    [ComponentName("remap armor enchants to specific weapon")]
    [AllowedOn(typeof(BlueprintUnitFact))]
    [AllowMultipleComponents]
    public class RemapBodyArmorEnchantsToSpecificWeapon : OwnedGameLogicComponent<UnitDescriptor>, IUnitActiveEquipmentSetHandler, IUnitEquipmentHandler, IGlobalSubscriber
    {
        public Dictionary<BlueprintArmorEnchantment, BlueprintWeaponEnchantment> enchantment_map = new Dictionary<BlueprintArmorEnchantment, BlueprintWeaponEnchantment>();
        public BlueprintItemWeapon target_weapon;
        public bool transfer_enhancement = true;

        [JsonProperty]
        private List<ItemEnchantment> m_enchants = new List<ItemEnchantment>();


        public override void OnTurnOn()
        {
            this.checkEquipment();
        }

        public override void OnTurnOff()
        {
            this.deactivateEnchants();
        }

        public void HandleUnitChangeActiveEquipmentSet(UnitDescriptor unit)
        {
            this.checkEquipment();
        }

        private void checkEquipment()
        {
            deactivateEnchants();
            if (this.Owner.Unit.Body.IsPolymorphed)
            {
                return;
            }

            var armor_enchants = this.Owner.Body.Armor?.MaybeArmor?.Enchantments;
            if (armor_enchants == null)
            {
                return;
            }

            var primary_hand_weapon = this.Owner?.Body?.PrimaryHand?.MaybeItem as ItemEntityWeapon;
            var secondary_hand_weapon = this.Owner?.Body?.SecondaryHand?.MaybeItem as ItemEntityWeapon;

            List<ItemEntityWeapon> weapons = new List<ItemEntityWeapon>();
            if (primary_hand_weapon?.Blueprint == target_weapon)
            {
                weapons.Add(primary_hand_weapon);
            }

            if (secondary_hand_weapon?.Blueprint == target_weapon)
            {
                weapons.Add(secondary_hand_weapon);
            }


            foreach (var limb in this.Owner.Body.AdditionalLimbs)
            {
                var weapon = limb?.Item as ItemEntityWeapon;
                if (weapon?.Blueprint == target_weapon)
                {
                    weapons.Add(weapon);
                }
            }

            if (weapons.Empty())
            {
                return;
            }


            foreach (var e in armor_enchants)
            {
                var blueprint = e.Blueprint as BlueprintArmorEnchantment;
                if (blueprint == null)
                {
                    continue;
                }
                if (enchantment_map.ContainsKey(blueprint))
                {
                    foreach (var w in weapons)
                    {
                        if (w.Enchantments.HasFact(blueprint))
                        {
                            continue;
                        }
                        var new_enchant = w.AddEnchantment(enchantment_map[blueprint], this.Fact.MaybeContext, new Rounds?());
                        new_enchant.RemoveOnUnequipItem = true;
                        m_enchants.Add(new_enchant);
                    }
                }
            }

            if (transfer_enhancement)
            {
                var enchancement_bonus = Math.Min(5, GameHelper.GetItemEnhancementBonus(this.Owner.Body.Armor.Armor));

                if (enchancement_bonus <= 0)
                {
                    return;
                }

                foreach (var w in weapons)
                {
                    var new_enchant = w.AddEnchantment(WeaponEnchantments.static_enchants[enchancement_bonus - 1], this.Fact.MaybeContext, new Rounds?());
                    new_enchant.RemoveOnUnequipItem = true;
                    m_enchants.Add(new_enchant);
                }
            }
        }

        private void deactivateEnchants()
        {
            foreach (var e in m_enchants)
            {
                e.Owner?.RemoveEnchantment(e);
            }
            m_enchants = new List<ItemEnchantment>();
        }

        public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
        {
            if (slot.Owner != this.Owner)
                return;
            this.checkEquipment();
        }
    }


    public class ActionOnDamageDealtWithDCFromSpecifiedBuff : WeaponEnchantmentLogic, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, IInitiatorRulebookSubscriber
    {
        public BlueprintBuff context_buff;
        public ActionList action;
        public int min_dmg = 1;
        public bool only_critical;

        public SavingThrowType save_type = SavingThrowType.Unknown;
        public DamageEnergyType energy_descriptor;

        public bool use_damage_energy_type = false;

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
            if (evt.Reason.Item != this.Owner)
            {
                return;
            }

            Helpers.runActionOnDamageDealt(evt, action, min_dmg, only_critical, save_type, context_buff, energy_descriptor, use_damage_energy_type);
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
        }
    }


    public class ApplyBuffDamageDealtWithDCFromSpecifiedBuff : WeaponEnchantmentLogic, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, IInitiatorRulebookSubscriber
    {
        public BlueprintBuff context_buff;
        public int min_dmg = 1;
        public bool only_critical;

        public SavingThrowType save_type = SavingThrowType.Unknown;
        public DamageEnergyType energy_descriptor;

        public bool use_damage_energy_type = false;
        public BlueprintBuff effect_buff;

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            if (evt.Reason.Item != this.Owner)
            {
                return;
            }

            var context_params = evt.Initiator?.Buffs?.GetBuff(context_buff)?.MaybeContext?.Params;
            if (context_params == null)
            {
                return;
            }

            
            var action = Common.createContextActionApplyBuff(effect_buff, CallOfTheWild.Helpers.CreateContextDuration(context_params.SpellLevel), is_from_spell: true);
            Helpers.runActionOnDamageDealt(evt, CallOfTheWild.Helpers.CreateActionList(action), min_dmg, only_critical, save_type, context_buff, energy_descriptor, use_damage_energy_type);
            
        }

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
        }
    }

    public class ReplaceEnergyDamage : WeaponEnchantmentLogic, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, IInitiatorRulebookSubscriber
    {

        public DamageEnergyType energy_descriptor;


        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
            if (evt.Reason.Item != this.Owner)
            {
                return;
            }

            foreach (var dmg in evt.DamageBundle)
            {
                (dmg as EnergyDamage)?.ReplaceEnergy(energy_descriptor);
            }
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
        }
    }


    public class WeaponDamageAgainstFact : WeaponEnchantmentLogic, IInitiatorRulebookHandler<RulePrepareDamage>, IRulebookHandler<RulePrepareDamage>, IInitiatorRulebookSubscriber
    {
        public ContextDiceValue Value;
        public DamageEnergyType DamageType;
        public BlueprintUnitFact checked_fact;

        public void OnEventAboutToTrigger(RulePrepareDamage evt)
        {
            var weapon = evt.ParentRule?.AttackRoll?.Weapon;
            if (weapon != this.Owner)
                return;
            if (!evt.Target.Descriptor.HasFact(checked_fact))
                return;
            int rollsCount = this.Value.DiceCountValue.Calculate(this.Context);
            int bonusDamage = this.Value.BonusValue.Calculate(this.Context);
            EnergyDamage energyDamage = new EnergyDamage(new DiceFormula(rollsCount, this.Value.DiceType), this.DamageType);
            energyDamage.AddBonusTargetRelated(bonusDamage);
            evt.DamageBundle.Add((BaseDamage)energyDamage);
        }

        public void OnEventDidTrigger(RulePrepareDamage evt)
        {
        }
    }


    public class WeaponSourceBuff: BlueprintComponent
    {
        public BlueprintBuff buff;
    }

    //allow summoned weapons to equip as free action
    [Harmony12.HarmonyPatch(typeof(UnitViewHandsEquipment))]
    [Harmony12.HarmonyPatch("HandleEquipmentSlotUpdated", Harmony12.MethodType.Normal)]
    class UnitViewHandsEquipment_HandleEquipmentSlotUpdated_Patch
    {
        static bool Prefix(UnitViewHandsEquipment __instance, HandSlot slot, ItemEntity previousItem)
        {
            Main.TraceLog();
            var tr = Harmony12.Traverse.Create(__instance);

            if (!tr.Property("Active").GetValue<bool>() || tr.Method("GetSlotData", slot).GetValue<UnitViewHandSlotData>() == null)
            {
                return true;
            }


            if (Helpers.isSummoned(slot.MaybeWeapon) || Helpers.isSummoned(previousItem)
                 && __instance.InCombat && (__instance.Owner.Descriptor.State.CanAct || __instance.IsDollRoom) && slot.Active)
            {
                tr.Method("ChangeEquipmentWithoutAnimation").GetValue();
                return false;
            }

            return true;
        }

    }


    [ComponentName("Heartseeker")]
    [AllowedOn(typeof(BlueprintWeaponEnchantment))]
    public class HeartSeekerEnchantment : GameLogicComponent, IInitiatorRulebookHandler<RuleAttackRoll>, IRulebookHandler<RuleAttackRoll>, IInitiatorRulebookSubscriber
    {
        private RuleAttackRoll m_Attack;

        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            m_Attack = null;
            ItemEntity owner = (this.Fact as ItemEnchantment)?.Owner;
            ItemEntityWeapon weapon = (evt.Reason.Rule as RuleAttackWithWeapon)?.Weapon;
            if (owner != weapon)
            {
                return;
            }

            var target = evt.Target;
            if (target == null)
            {
                return;
            }

            if (!evt.Weapon.Blueprint.IsRanged || this.m_Attack != null)
                return;

            if (target.Descriptor.HasFact(Common.elemental) 
                || target.Descriptor.HasFact(Common.aberration)
                || target.Descriptor.HasFact(Common.undead)
                || target.Descriptor.HasFact(Common.plant))
            {
                return;
            }

            evt.Initiator.Ensure<UnitPartConcealment>().IgnoreAll = true;
            this.m_Attack = evt;
        }

        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
            if (m_Attack == null)
                return;
            evt.Initiator.Ensure<UnitPartConcealment>().IgnoreAll = false;
            m_Attack = null;
        }
    }


    //allow summoned weapons to be equippable without proficiency
    [Harmony12.HarmonyPatch(typeof(ItemEntityWeapon))]
    [Harmony12.HarmonyPatch("CanBeEquippedInternal", Harmony12.MethodType.Normal)]
    class ItemEntityWeapon_CanBeEquippedInternal_Patch
    {
        static void Postfix(ItemEntityWeapon __instance, UnitDescriptor owner, ref bool __result)
        {
            Main.TraceLog();
            if (__result)
            {
                return;
            }

            bool? nullable = (__instance.Blueprint as BlueprintItemEquipment)?.CanBeEquippedBy(owner);
            if (nullable.HasValue && nullable.Value)
            {
                __result = Helpers.isSummoned(__instance);
            }
        }

    }

    static public class Helpers
    {
        static public bool isSummoned(ItemEntity item)
        {
            if (item?.EnchantmentsCollection == null)
            {
                return false;
            }

            return item.EnchantmentsCollection.HasFact(WeaponEnchantments.summoned_weapon_enchant);
        }


        public static void runActionOnDamageDealt(RuleDealDamage evt, ActionList action, int min_dmg = 1, bool only_critical = false, SavingThrowType save_type = SavingThrowType.Unknown,
                                          BlueprintBuff context_buff = null, DamageEnergyType energy_descriptor = DamageEnergyType.Acid, bool use_damage_energy_type = false)
        {


            if (only_critical && (evt.AttackRoll == null || !evt.AttackRoll.IsCriticalConfirmed))
            {
                return;
            }


            var target = evt.Target;
            if (target == null)
            {
                return;
            }

            if (evt.Damage <= min_dmg)
            {
                return;
            }

            if (use_damage_energy_type)
            {
                bool damage_found = false;
                foreach (var dmg in evt.DamageBundle)
                {
                    var energy_damage = (dmg as EnergyDamage);

                    if (energy_damage != null && energy_damage.EnergyType == energy_descriptor)
                    {
                        damage_found = true;
                        break;
                    }
                }

                if (!damage_found)
                {
                    return;
                }
            }

            if (save_type != SavingThrowType.Unknown)
            {
                var context_params = evt.Initiator.Buffs?.GetBuff(context_buff)?.MaybeContext?.Params;
                if (context_params == null)
                {
                    return;
                }

                var dc = context_params.DC;
                var rule_saving_throw = new RuleSavingThrow(target, save_type, dc);
                Rulebook.Trigger(rule_saving_throw);

                if (rule_saving_throw.IsPassed)
                {
                    return;
                }
            }

            var context_fact = evt.Initiator.Buffs?.GetBuff(context_buff);

            (context_fact as IFactContextOwner)?.RunActionInContext(action, target);
        }
    }


}