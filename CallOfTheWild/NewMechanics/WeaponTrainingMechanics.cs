using JetBrains.Annotations;
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
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
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
        static BlueprintFeature leopard_frace = Main.library.Get<BlueprintFeature>("b8c98af302ee334499d30a926306327d");

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

            if (weapon.Blueprint.Category == WeaponCategory.Scimitar && this.Owner.Progression.Features.HasFact(NewFeats.dervish_dance))
            {
                return;
            }


            if ((weapon.Blueprint.Category == WeaponCategory.Bite || weapon.Blueprint.Category == WeaponCategory.Claw) && this.Owner.Progression.Features.HasFact(leopard_frace))
            {
                return;
            }

            if (!evt.DamageBonusStat.HasValue || evt.DamageBonusStat != StatType.Strength)
            {
                return;
            }

            /*if (!evt.Weapon.Blueprint.Category.HasSubCategory(WeaponSubCategory.Finessable) && !this.Owner.HasFact(AdvancedFighterOptions.fighters_finesse))
            {
                return;
            }*/
            var has_free_secondary_hand = HoldingItemsMechanics.Helpers.hasFreeHandOrBuckler(evt.Initiator.Body?.SecondaryHand);
            if (AdvancedFighterOptions.category_finesse_training_map.ContainsKey(evt.Weapon.Blueprint.Category)
                && this.Owner.HasFact(AdvancedFighterOptions.category_finesse_training_map[evt.Weapon.Blueprint.Category]))
            {
                return;
            }

            if (this.Owner.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => p.Blueprint == slashing_grace).Any(p => p.Param == weapon.Blueprint.Category) 
                && has_free_secondary_hand)
            {
                return;
            }

            if (this.Owner.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => p.Blueprint == fencing_grace).Any(p => p.Param == weapon.Blueprint.Category) 
                && has_free_secondary_hand)
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

            if (weapon.Blueprint.Category == WeaponCategory.Scimitar && this.Owner.Progression.Features.HasFact(NewFeats.dervish_dance))
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

            var has_free_secondary_hand = HoldingItemsMechanics.Helpers.hasFreeHandOrBuckler(evt.Initiator.Body?.SecondaryHand);
            if (this.Owner.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => p.Blueprint == slashing_grace).Any(p => p.Param == weapon.Blueprint.Category)
                && has_free_secondary_hand)
            {
                return;
            }

            if (this.Owner.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => p.Blueprint == fencing_grace).Any(p => p.Param == weapon.Blueprint.Category)
                && has_free_secondary_hand)
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
    public class AddFeatureOnWeaponCategory : OwnedGameLogicComponent<UnitDescriptor>, IUnitActiveEquipmentSetHandler, IUnitEquipmentHandler, IGlobalSubscriber
    {
        public BlueprintUnitFact feature;
        [JsonProperty]
        private Fact m_AppliedFact;

        public WeaponCategory[] required_categories;

        public override void OnFactActivate()
        {
            this.Apply();
        }

        public override void OnFactDeactivate()
        {
            if (m_AppliedFact != null)
            {
                this.Owner.RemoveFact(this.m_AppliedFact);
            }
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

            bool weapon_ok = checkWeapon(this.Owner.Body?.PrimaryHand?.MaybeWeapon) || checkWeapon(this.Owner.Body?.PrimaryHand?.MaybeWeapon);


            if (this.m_AppliedFact != null || !weapon_ok)
            {
                return;
            }
            this.m_AppliedFact = this.Owner.AddFact(this.feature, null, null);
        }


        private bool checkWeapon([CanBeNull] ItemEntityWeapon weapon)
        {
            if (weapon == null)
            {
                return false;
            }

            return required_categories.Contains(weapon.Blueprint.Category);
        }
    }



    [AllowMultipleComponents]
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AddFeatureOnArmor : OwnedGameLogicComponent<UnitDescriptor>, IUnitActiveEquipmentSetHandler, IUnitEquipmentHandler, IGlobalSubscriber
    {
        public BlueprintUnitFact feature;
        [JsonProperty]
        private Fact m_AppliedFact;

        public ArmorProficiencyGroup[] required_armor = new ArmorProficiencyGroup[0];
        public ArmorProficiencyGroup[] forbidden_armor = new ArmorProficiencyGroup[0];

        public override void OnFactActivate()
        {
            this.Apply();
        }

        public override void OnFactDeactivate()
        {
            if (m_AppliedFact != null)
            {
                this.Owner.RemoveFact(this.m_AppliedFact);
            }
            this.m_AppliedFact = null;
        }

        public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
        {
            if (slot.Owner != this.Owner)
                return;

            var armor_slot = Owner.Body?.Armor;
            var shield_slot = Owner.Body?.SecondaryHand;

            if ((armor_slot != null && slot == armor_slot)
                || (shield_slot != null && slot == shield_slot))
            {
                this.Apply();
            }
                         
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
            armor_ok = armor_ok || (body_armor == null && required_armor.Contains(ArmorProficiencyGroup.None));
            if (!armor_ok)
            {
                var shield = this.Owner.Body?.SecondaryHand?.MaybeShield?.ArmorComponent;
                armor_ok = shield != null && required_armor.Contains(shield.Blueprint.ProficiencyGroup);
            }

            if (!armor_ok)
            {
                return;
            }

            bool has_forbidden_armor = false;
            has_forbidden_armor = body_armor != null && forbidden_armor.Contains(body_armor.Blueprint.ProficiencyGroup);
            has_forbidden_armor = has_forbidden_armor || (body_armor == null && forbidden_armor.Contains(ArmorProficiencyGroup.None));
            if (!has_forbidden_armor)
            {
                if (!HoldingItemsMechanics.Helpers.hasFreeHandOrBuckler(this.Owner.Body?.SecondaryHand))
                {
                    var shield = this.Owner.Body?.SecondaryHand?.MaybeShield?.ArmorComponent;
                    has_forbidden_armor = shield != null && forbidden_armor.Contains(shield.Blueprint.ProficiencyGroup);
                }
            }

            if (has_forbidden_armor)
            {
                return;
            }


            if (this.m_AppliedFact != null)
            {
                return;
            }
            /*if (Owner.HasFact(this.feature))
            {
                return;
            }*/
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



    [ComponentName("Weapon group damage bonus")]
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class ShareWeaponGroupAttackDamageBonus : RuleInitiatorLogicComponent<RuleCalculateWeaponStats>, IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>
    {
        public float multiplier = 0.5f;

        private MechanicsContext Context
        {
            get
            {
                MechanicsContext context = (this.Fact as Buff)?.Context;
                if (context != null)
                    return context;
                return (this.Fact as Feature)?.Context;
            }
        }


        int getBonus(ItemEntityWeapon weapon)
        {
            var num = this.Context.MaybeCaster?.Get<UnitPartWeaponTraining>()?.GetWeaponRank(weapon);
            var caster_bonus = (int)(num.GetValueOrDefault() * multiplier);

            var num2 = this.Owner?.Get<UnitPartWeaponTraining>()?.GetWeaponRank(weapon);
            var wielder_bonus = num2.GetValueOrDefault();

            return caster_bonus - wielder_bonus;
        }

        public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {

            int bonus = getBonus(evt.Weapon);
            if (bonus > 0)
            {
                evt.AddTemporaryModifier(evt.Initiator.Stats.AdditionalDamage.AddModifier(bonus, (GameLogicComponent)this, ModifierDescriptor.UntypedStackable));
            }

        }

        public override void OnEventDidTrigger(RuleCalculateWeaponStats evt)
        {
        }

        public void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
            int bonus = getBonus(evt.Weapon);
            if (bonus > 0)
            {
                evt.AddBonus(bonus, this.Fact);
            }
        }

        public void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
        }
    }

    //allow 2h weapon training and sacred weapon training to be recognized by advanced weapon training
    [Harmony12.HarmonyPatch(typeof(UnitPartWeaponTraining))]
    [Harmony12.HarmonyPatch("GetWeaponRank", Harmony12.MethodType.Normal)]
    [Harmony12.HarmonyPatch(new Type[] { typeof(ItemEntityWeapon) })]
    class Patch_UnitPartWeaponTraining_GetWeaponRank
    {
        static BlueprintFeature two_handed_weapon_training = Main.library.Get<BlueprintFeature>("88da2a5dfc505054f933bb81014e864f");
        static BlueprintParametrizedFeature weapon_focus = Main.library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");

        static public void Postfix(UnitPartWeaponTraining __instance, ItemEntityWeapon weapon, ref int __result)
        {
            Main.TraceLog();
            if (weapon == null)
            {
                return;
            }
            //allow weapon training, arsenal chaplain weapon training and two-handed fighter weapon training to stack 
            if (weapon.Blueprint.IsTwoHanded && weapon.Blueprint.IsMelee)
            {
                __result += (__instance.Owner.GetFact(two_handed_weapon_training)?.GetRank()).GetValueOrDefault();   
            }

            if (checkFeature(__instance.Owner, weapon.Blueprint.Category, weapon_focus, NewFeats.deity_favored_weapon))
            {
                __result += (__instance.Owner.GetFact(Warpriest.arsenal_chaplain_weapon_training)?.GetRank()).GetValueOrDefault();           
            }
        }


        static bool checkFeature(UnitDescriptor unit, WeaponCategory category, params BlueprintParametrizedFeature[] required_parametrized_features)
        {
            if (required_parametrized_features.Empty())
            {
                return true;
            }
            foreach (var f in required_parametrized_features)
            {
                if (unit.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => p.Blueprint == f).Any(p => p.Param == category))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
