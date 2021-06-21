using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Common;
using Kingmaker.UI.ServiceWindow.CharacterScreen;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.Class.Kineticist.ActivatableAbility;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View.Animation;
using Kingmaker.View.Equipment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurnBased.Controllers;

namespace CallOfTheWild.HoldingItemsMechanics
{
    [Harmony12.HarmonyPatch(typeof(ItemEntityWeapon))]
    [Harmony12.HarmonyPatch("HoldInTwoHands", Harmony12.MethodType.Getter)]
    class ItemEntityWeapon__HoldInTwoHands__Patch
    {
        static void Postfix(ItemEntityWeapon __instance, ref bool __result)
        {
            Main.TraceLog();
            bool spell_combat = false;
            UnitPartMagus unit_part_magus = __instance.Wielder?.Get<UnitPartMagus>();
            if ((bool)(unit_part_magus) && unit_part_magus.SpellCombat.Active)
            {
                spell_combat = true;
            }

            if (__instance.Blueprint.IsTwoHanded
                || (__instance.Blueprint.IsOneHandedWhichCanBeUsedWithTwoHands && __result == false))
            {
                var unit_part = __instance.Wielder?.Get<UnitPartCanHold2hWeaponIn1h>();
                if (!spell_combat)
                {//check if we can hold the 2h weapon in 1h
                    if (unit_part == null)
                    {
                        return;
                    }

                    __result = unit_part.canBeUsedAs2h(__instance);
                }
                else
                {
                    if (unit_part != null && unit_part.canBeUsedOn(__instance))
                    {//weapon is being held as one-handed
                        __result = false;
                        return;
                    }
                    //normally we can not 2h with spell combat, so we check only magus specific feature that would allow us
                    var use_spell_combat_part = __instance.Wielder?.Get<UnitPartCanUseSpellCombat>();
                    if (use_spell_combat_part == null)
                    {
                        return;
                    }

                    var pair_slot = (__instance.HoldingSlot as HandSlot)?.PairSlot;
                    __result = use_spell_combat_part.canBeUsedOn(__instance.HoldingSlot as HandSlot, pair_slot, true);
                }
            }
        }
    }


    [Harmony12.HarmonyPatch(typeof(HandSlot))]
    [Harmony12.HarmonyPatch("OnItemInserted", Harmony12.MethodType.Normal)]
    class HandSlot__OnItemInserted__Patch
    {
        static bool Prefix(HandSlot __instance)
        {
            Main.TraceLog();
            var unit_part = __instance.Owner?.Get<UnitPartCanHold2hWeaponIn1h>();

            if (unit_part == null)
            {
                return true;
            }

            if (!__instance.IsPrimaryHand)
            {
                HandSlot primaryHand = __instance.HandsEquipmentSet.PrimaryHand;
                ItemEntityWeapon maybeItem = primaryHand.MaybeItem as ItemEntityWeapon;
                if (maybeItem != null && ((maybeItem.Blueprint.IsTwoHanded && !unit_part.canBeUsedOn(maybeItem)) || maybeItem.Blueprint.Double))
                {
                    primaryHand.RemoveItem();
                }
            }
            ItemEntityWeapon maybeItem1 = __instance.MaybeItem as ItemEntityWeapon;
            if (maybeItem1 != null && ((maybeItem1.Blueprint.IsTwoHanded && !unit_part.canBeUsedOn(maybeItem1)) || maybeItem1.Blueprint.Double))
            {
                if (__instance.IsPrimaryHand)
                {
                    __instance.PairSlot.RemoveItem();
                }
                else
                {
                    __instance.RemoveItem();
                    __instance.PairSlot.InsertItem((ItemEntity)maybeItem1);
                }
            }
            __instance.IsDirty = true;

            return false;
        }
    }



    public class UnitPartCanHold2hWeaponIn1h : AdditiveUnitPart
    {
        public bool canBeUsedOn(ItemEntityWeapon weapon)
        {
            if (buffs.Empty())
            {
                return false;
            }

            foreach (var b in buffs)
            {
                bool result = false;
                b.CallComponents<CanUse2hWeaponAs1hBase>(c => result = c.canBeUsedOn(weapon));
                if (result)
                {
                    return true;
                }
            }

            return false;
        }


        public bool canBeUsedAs2h(ItemEntityWeapon weapon)
        {
            bool can_use_at_all = false;
            foreach (var b in buffs)
            {
                bool can_use = false;
                bool can_2h = false;
                b.CallComponents<CanUse2hWeaponAs1hBase>(c => { can_2h = c.canBeUsedAs2h(weapon); can_use = c.canBeUsedOn(weapon); });
                if (can_use && can_2h)
                {
                    return true;
                }
                can_use_at_all = can_use_at_all || can_use;
            }

            if (!can_use_at_all)
            {
                HandSlot pair_slot = (weapon?.HoldingSlot as HandSlot)?.PairSlot;
                if (pair_slot != null)
                    return !pair_slot.HasItem;
                return false;
            }
            else
            {
                return false;
            }
        }
    }


    public abstract class CanUse2hWeaponAs1hBase : OwnedGameLogicComponent<UnitDescriptor>
    {
        abstract public bool canBeUsedOn(ItemEntityWeapon weapon);

        abstract public bool canBeUsedAs2h(ItemEntityWeapon weapon);

        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartCanHold2hWeaponIn1h>().addBuff(this.Fact);
        }

        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartCanHold2hWeaponIn1h>().removeBuff(this.Fact);
        }
    }


    public class UnitPartUnarmedTwf : AdditiveUnitPart
    {
        public bool active()
        {
            return !buffs.Empty();
        }
    }

    public class UnarmedTwf : OwnedGameLogicComponent<UnitDescriptor>
    {
        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartUnarmedTwf>().addBuff(this.Fact);
            EventBus.RaiseEvent<IUnitActiveEquipmentSetHandler>((Action<IUnitActiveEquipmentSetHandler>)(h => h.HandleUnitChangeActiveEquipmentSet(this.Owner)));
        }


        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartUnarmedTwf>().removeBuff(this.Fact);
            EventBus.RaiseEvent<IUnitActiveEquipmentSetHandler>((Action<IUnitActiveEquipmentSetHandler>)(h => h.HandleUnitChangeActiveEquipmentSet(this.Owner)));
        }
    }


    public class UnitPartCanUseSpellCombat : AdditiveUnitPart
    {
        public bool canBeUsedOn(HandSlot primary_hand, HandSlot secondary_hand, bool use_two_handed)
        {
            if (buffs.Empty())
            {
                return false;
            }

            foreach (var b in buffs)
            {
                bool result = false;
                b.CallComponents<CanUseSpellCombatBase>(c => result = c.canBeUsedOn(primary_hand, secondary_hand, use_two_handed));
                if (result)
                {
                    return true;
                }
            }

            return false;
        }
    }


    public abstract class CanUseSpellCombatBase : OwnedGameLogicComponent<UnitDescriptor>
    {
        abstract public bool canBeUsedOn(HandSlot primary_hand_slot, HandSlot secondary_hand_slot, bool use_two_handed);

        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartCanUseSpellCombat>().addBuff(this.Fact);
        }


        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartCanUseSpellCombat>().removeBuff(this.Fact);
        }
    }





    public class UseSpellCombatWithShield : CanUseSpellCombatBase
    {
        public override bool canBeUsedOn(HandSlot primary_hand_slot, HandSlot secondary_hand_slot, bool use_two_handed)
        {
            if (use_two_handed)
            {
                return false;
            }

            var shield = secondary_hand_slot?.MaybeShield;

            return shield != null;
        }

        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartCanUseSpellCombat>().addBuff(this.Fact);
        }


        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartCanUseSpellCombat>().removeBuff(this.Fact);
        }
    }


    public class UseSpellCombatWith2ManifestedWeapons : CanUseSpellCombatBase
    {
        public BlueprintWeaponEnchantment required_enchant;
        public override bool canBeUsedOn(HandSlot primary_hand_slot, HandSlot secondary_hand_slot, bool use_two_handed)
        {
            if (use_two_handed)
            {
                return false;
            }

            var weapon = primary_hand_slot?.MaybeWeapon;
            var weapon2 = secondary_hand_slot?.MaybeWeapon;
            if (weapon == null || weapon.EnchantmentsCollection == null)
            {
                return false;
            }

            if (weapon.Blueprint.Double && weapon.EnchantmentsCollection.HasFact(required_enchant))
            {
                return true;
            }

            if (weapon2 == null || weapon2.EnchantmentsCollection == null)
            {
                return false;
            }

            return weapon.EnchantmentsCollection.HasFact(required_enchant) && weapon2.EnchantmentsCollection.HasFact(required_enchant);
        }

        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartCanUseSpellCombat>().addBuff(this.Fact);
        }


        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartCanUseSpellCombat>().removeBuff(this.Fact);
        }
    }


    public class UseSpellCombatWith2hManifestedWeapon : CanUseSpellCombatBase
    {
        public BlueprintWeaponEnchantment required_enchant;
        public override bool canBeUsedOn(HandSlot primary_hand_slot, HandSlot secondary_hand_slot, bool use_two_handed)
        {
            var weapon = primary_hand_slot?.MaybeWeapon;
            if (weapon == null || weapon.EnchantmentsCollection == null)
            {
                return false;
            }

            if (secondary_hand_slot != null && !Helpers.hasFreeHand(secondary_hand_slot))
            {
                return false;
            }

            return weapon.EnchantmentsCollection.HasFact(required_enchant);
        }

        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartCanUseSpellCombat>().addBuff(this.Fact);
        }


        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartCanUseSpellCombat>().removeBuff(this.Fact);
        }
    }




    [AllowMultipleComponents]
    public class ShieldBrace : CanUse2hWeaponAs1hBase, IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>
    {
        public override bool canBeUsedAs2h(ItemEntityWeapon weapon)
        {
            return false;
        }

        public override bool canBeUsedOn(ItemEntityWeapon weapon)
        {
            if (!weapon.Blueprint.IsTwoHanded)
            {
                return false;
            }
            HandSlot holding_slot = weapon?.HoldingSlot as HandSlot;
            if (holding_slot == null)
            {
                return false;
            }

            if (!holding_slot.PairSlot.HasItem)
            {
                return false;
            }

            if (holding_slot.PairSlot.MaybeShield == null)
            {
                return false;
            }

            var shield_proficiency = holding_slot.PairSlot.MaybeShield.ArmorComponent.Blueprint.ProficiencyGroup;

            if (shield_proficiency == Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.Buckler)
            {
                return false;
            }

            return weapon.Blueprint.FighterGroup == WeaponFighterGroup.Spears || weapon.Blueprint.FighterGroup == WeaponFighterGroup.Polearms;
        }

        public void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
            if (evt.Weapon == null)
            {
                return;
            }

            if (!canBeUsedOn(evt.Weapon))
            {
                return;
            }

            var shield = (evt.Weapon?.HoldingSlot as HandSlot)?.PairSlot.MaybeShield;
            int penalty = Rulebook.Trigger<RuleCalculateArmorCheckPenalty>(new RuleCalculateArmorCheckPenalty(evt.Initiator, shield.ArmorComponent)).Penalty;


            evt.AddBonus(penalty, this.Fact);
        }

        public void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {

        }
    }


    [AllowMultipleComponents]
    public class UnhinderingShield : CanUse2hWeaponAs1hBase
    {
        public override bool canBeUsedAs2h(ItemEntityWeapon weapon)
        {
            return true;
        }


        public override bool canBeUsedOn(ItemEntityWeapon weapon)
        {
            HandSlot holding_slot = weapon?.HoldingSlot as HandSlot;
            if (holding_slot == null)
            {
                return false;
            }

            if (!holding_slot.PairSlot.HasItem)
            {
                return false;
            }

            if (holding_slot.PairSlot.MaybeShield == null)
            {
                return false;
            }

            return holding_slot.PairSlot.MaybeShield.ArmorComponent.Blueprint.ProficiencyGroup == Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.Buckler;
        }
    }


    [AllowMultipleComponents]
    public class CanHoldIn1Hand : CanUse2hWeaponAs1hBase
    {
        public WeaponCategory category;
        public BlueprintWeaponType[] except_types = new BlueprintWeaponType[0];
        public bool require_full_proficiency = false;
        public override bool canBeUsedAs2h(ItemEntityWeapon weapon)
        {
            HandSlot holding_slot = weapon?.HoldingSlot as HandSlot;
            if (holding_slot == null)
            {
                return false;
            }

            return Helpers.hasFreeHand(holding_slot.PairSlot);
        }


        public override bool canBeUsedOn(ItemEntityWeapon weapon)
        {
            if (require_full_proficiency &&
                !(weapon.Owner?.Get<WeaponsFix.UnitPartFullProficiency>()?.hasFullProficiency(category)).GetValueOrDefault())
            {
                return false;
            }
            
            return (weapon.Blueprint?.Category).GetValueOrDefault() == category 
                    && !except_types.Contains(weapon?.Blueprint?.Type);
        }
    }

    internal class Helpers
    {
        internal static bool hasFreeHand(HandSlot hand_slot)
        {
            if (hand_slot == null)
            {
                return true;
            }
            if (!hand_slot.HasItem)
            {
                return true;
            }

            var shield = hand_slot.MaybeShield;
            if (shield == null)
            {
                return false;
            }
            return shield.ArmorComponent.Blueprint.ProficiencyGroup == Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.Buckler && hand_slot.Owner.HasFact(NewFeats.unhindering_shield);
        }


        internal static bool hasFreeHandOrBuckler(HandSlot hand_slot)
        {
            if (hand_slot == null)
            {
                return true;
            }
            if (!hand_slot.HasItem)
            {
                return true;
            }

            var shield = hand_slot.MaybeShield;
            if (shield == null)
            {
                return false;
            }
            return shield.ArmorComponent.Blueprint.ProficiencyGroup == Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.Buckler;
        }


        internal static bool hasFreeHandOr1hWeapon(HandSlot hand_slot)
        {
            if (hasFreeHand(hand_slot))
            {
                return true;
            }

            var weapon = hand_slot.Weapon;
            if (weapon == null)
            {
                return true;
            }
            return !weapon.Blueprint.IsTwoHanded && (!weapon.Blueprint.Double);
        }


        internal static bool has2hWeapon(HandSlot hand_slot)
        {
            if (hand_slot == null)
            {
                return false;
            }
            if (!hand_slot.HasItem)
            {
                return false;
            }

            var weapon = hand_slot.MaybeWeapon;
            if (weapon == null)
            {
                return false;
            }

            if (weapon.Blueprint.IsTwoHanded)
            {
                return true;
            }

            return weapon.Blueprint.Double;
        }

        internal static bool hasShield2(HandSlot hand_slot)
        {
            if (!hand_slot.HasItem)
            {
                return false;
            }

            var shield = hand_slot.MaybeShield;
            if (shield == null)
            {
                return false;
            }

            return !(shield.ArmorComponent.Blueprint.ProficiencyGroup == Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.Buckler && hand_slot.Owner.HasFact(NewFeats.unhindering_shield));
        }
    }



    [Harmony12.HarmonyPatch(typeof(UnitPartMagus))]
    [Harmony12.HarmonyPatch("HasOneHandedMeleeWeaponAndFreehand", Harmony12.MethodType.Normal)]
    class UnitPartMagus__HasOneHandedMeleeWeaponAndFreehand__Patch
    {
        static void Postfix(UnitDescriptor unit, ref bool __result)
        {
            if (__result == false && unit.Body.PrimaryHand.MaybeWeapon != null)
            {
                __result = !unit.Body.PrimaryHand.MaybeWeapon.HoldInTwoHands && Helpers.hasFreeHand(unit.Body.SecondaryHand);
            }
            var use_spell_combat_part = unit.Get<UnitPartCanUseSpellCombat>();
            if (__result == false && use_spell_combat_part != null)
            {
                __result = use_spell_combat_part.canBeUsedOn(unit.Body.PrimaryHand, unit.Body.SecondaryHand, false);
            }
        }
    }


    [Harmony12.HarmonyPatch(typeof(MonkNoArmorAndMonkWeaponFeatureUnlock))]
    [Harmony12.HarmonyPatch("CheckEligibility", Harmony12.MethodType.Normal)]
    class MonkNoArmorAndMonkWeaponFeatureUnlock__CheckEligibility__Patch
    {
        static IEnumerable<Harmony12.CodeInstruction> Transpiler(IEnumerable<Harmony12.CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var check_shield = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("HasShield"));

            codes[check_shield] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call, new Func<HandSlot, bool>(Helpers.hasShield2).Method);
            return codes.AsEnumerable();
        }
    }


    [Harmony12.HarmonyPatch(typeof(MonkNoArmorFeatureUnlock))]
    [Harmony12.HarmonyPatch("CheckEligibility", Harmony12.MethodType.Normal)]
    class MonkNoArmorFeatureUnlock__CheckEligibility__Patch
    {
        static bool Prefix(MonkNoArmorFeatureUnlock __instance)
        {
            Main.TraceLog();
            if (!Helpers.hasShield2(__instance.Owner.Body.SecondaryHand) && (!__instance.Owner.Body.Armor.HasArmor || !__instance.Owner.Body.Armor.Armor.Blueprint.IsArmor))
            {
                __instance.AddFact();
            }
            else
                __instance.RemoveFact();

            return false;
        }
    }


    [Harmony12.HarmonyPatch(typeof(CannyDefensePermanent))]
    [Harmony12.HarmonyPatch("CheckArmor", Harmony12.MethodType.Normal)]
    class CannyDefensePermanent__CheckArmor__Patch
    {
        static IEnumerable<Harmony12.CodeInstruction> Transpiler(IEnumerable<Harmony12.CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var check_shield = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("HasShield"));

            codes[check_shield] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call, new Func<HandSlot, bool>(Helpers.hasShield2).Method);
            return codes.AsEnumerable();
        }
    }


    [Harmony12.HarmonyPatch(typeof(ACBonusAgainstAttacks))]
    [Harmony12.HarmonyPatch("Check", Harmony12.MethodType.Normal)]
    class ACBonusAgainstAttacks__Check__Patch
    {
        static IEnumerable<Harmony12.CodeInstruction> Transpiler(IEnumerable<Harmony12.CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var check_shield = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("HasShield"));

            codes[check_shield] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call, new Func<HandSlot, bool>(Helpers.hasShield2).Method);
            return codes.AsEnumerable();
        }
    }


    [Harmony12.HarmonyPatch(typeof(DuelistPreciseStrike))]
    [Harmony12.HarmonyPatch("OnEventAboutToTrigger", Harmony12.MethodType.Normal)]
    class DuelistPreciseStrike__OnEventAboutToTrigger__Patch
    {
        static IEnumerable<Harmony12.CodeInstruction> Transpiler(IEnumerable<Harmony12.CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var check_shield = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("HasShield"));

            codes[check_shield] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call, new Func<HandSlot, bool>(Helpers.hasShield2).Method);
            return codes.AsEnumerable();
        }
    }


    [Harmony12.HarmonyPatch(typeof(DeflectArrows))]
    [Harmony12.HarmonyPatch("CheckRestriction", Harmony12.MethodType.Normal)]
    class DeflectArrows__CheckRestriction__Patch
    {
        static IEnumerable<Harmony12.CodeInstruction> Transpiler(IEnumerable<Harmony12.CodeInstruction> instructions)
        {
            var codes = instructions.ToList();

            var check_shield = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("HasShield"));
            codes[check_shield] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call, new Func<HandSlot, bool>(Helpers.hasShield2).Method);
            check_shield = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("HasShield"));
            codes[check_shield] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call, new Func<HandSlot, bool>(Helpers.hasShield2).Method);
            return codes.AsEnumerable();
        }
    }


    [Harmony12.HarmonyPatch(typeof(RestrictionCanGatherPower))]
    [Harmony12.HarmonyPatch("IsAvailable", Harmony12.MethodType.Normal)]
    class RestrictionCanGatherPower__IsAvailable__Patch
    {
        static bool Prefix(RestrictionCanGatherPower __instance, ref bool __result)
        {
            Main.TraceLog();
            __result = getResult(__instance);
            return false;
        }


        static bool getResult(RestrictionCanGatherPower __instance)
        {
            UnitPartKineticist unitPartKineticist = __instance.Owner.Get<UnitPartKineticist>();
            if (unitPartKineticist == null)
                return false;
            UnitBody body = __instance.Owner.Body;
            if (body.IsPolymorphed)
                return false;
            ItemEntity maybeItem1 = body.PrimaryHand.MaybeItem;
            WeaponCategory? category = (maybeItem1 as ItemEntityWeapon)?.Blueprint.Category;
            bool flag = category.GetValueOrDefault() == WeaponCategory.KineticBlast && category.HasValue;
            if (maybeItem1 != null && !flag)
                return false;

            ItemEntity maybeItem2 = body.SecondaryHand.MaybeItem;
            if (maybeItem2 == null)
                return true;
            ArmorProficiencyGroup? proficiencyGroup = body.SecondaryHand.MaybeShield?.Blueprint.Type.ProficiencyGroup;

            if (proficiencyGroup.HasValue && (proficiencyGroup.GetValueOrDefault() != ArmorProficiencyGroup.TowerShield))
                return unitPartKineticist.CanGatherPowerWithShield || Helpers.hasFreeHand(body.SecondaryHand);
            return false;
        }
    }



    [Harmony12.HarmonyPatch(typeof(ItemEntityWeapon))]
    [Harmony12.HarmonyPatch("GetAnimationStyle", Harmony12.MethodType.Normal)]
    class ItemEntityWeapon__GetAnimationStyle__Patch
    {
        static void Postfix(ItemEntityWeapon __instance, bool forDollRoom, ref WeaponAnimationStyle __result)
        {
            Main.TraceLog();
            if (__instance == null)
            {
                return;
            }
            if ((__instance.Blueprint.IsTwoHanded && !__instance.HoldInTwoHands) //2h that is held as 1h
                || ((__instance.Blueprint.IsTwoHanded || __instance.Blueprint.IsOneHandedWhichCanBeUsedWithTwoHands) && forDollRoom)) // make weapon look 1h in the doll room to see the shield if possible
            {
                var pair_slot = (__instance.HoldingSlot as HandSlot)?.PairSlot;
                if (!(pair_slot?.HasItem).GetValueOrDefault())
                {
                    return;
                }
               
                switch (__result)
                {
                    case WeaponAnimationStyle.AxeTwoHanded:
                        __result = WeaponAnimationStyle.SlashingOneHanded;
                        break;
                    case WeaponAnimationStyle.PiercingTwoHanded:
                        __result = WeaponAnimationStyle.PiercingOneHanded;
                        break;
                    case WeaponAnimationStyle.SlashingTwoHanded:
                        __result = WeaponAnimationStyle.SlashingOneHanded;
                        break;
                    default:
                        break;
                }
            }
        }
    }

    //EmptyHandWeaponOverrideNotification
    [Harmony12.HarmonyPatch(typeof(UnitBody))]
    [Harmony12.HarmonyPatch("SetEmptyHandWeapon", Harmony12.MethodType.Normal)]
    public class UnitBody__SetEmptyHandWeapon__Patch
    {
        public static bool no_animation_action = false;
        //signal weapon set change to notify that weapons could have been changed
        static void Postfix(UnitBody __instance, BlueprintItemWeapon weapon)
        {
            no_animation_action = true;
            EventBus.RaiseEvent<IUnitActiveEquipmentSetHandler>((Action<IUnitActiveEquipmentSetHandler>)(h => h.HandleUnitChangeActiveEquipmentSet(__instance.Owner)));
            no_animation_action = false;
        }
    }


    //Shield bash activated notification
    [Harmony12.HarmonyPatch(typeof(AddMechanicsFeature))]
    [Harmony12.HarmonyPatch("OnTurnOn", Harmony12.MethodType.Normal)]
    class AddMechanicsFeature__OnTurnOn__Patch
    {
        public static bool no_animation_action = false;
        //signal weapon set change to notify that weapons could have been changed
        static void Postfix(AddMechanicsFeature __instance)
        {
            var feature = CallOfTheWild.Helpers.GetField<AddMechanicsFeature.MechanicsFeatureType>(__instance, "m_Feature");
            if (feature == AddMechanicsFeature.MechanicsFeatureType.ShieldBash)
            {
                no_animation_action = true;
                EventBus.RaiseEvent<IUnitActiveEquipmentSetHandler>((Action<IUnitActiveEquipmentSetHandler>)(h => h.HandleUnitChangeActiveEquipmentSet(__instance.Owner)));
                no_animation_action = false;
            }
        }
    }
    [Harmony12.HarmonyPatch(typeof(AddMechanicsFeature))]
    [Harmony12.HarmonyPatch("OnTurnOff", Harmony12.MethodType.Normal)]
    class AddMechanicsFeature__OnTurnOff__Patch
    {
        public static bool no_animation_action = false;
        //signal weapon set change to notify that weapons could have been changed
        static void Postfix(AddMechanicsFeature __instance)
        {
            var feature = CallOfTheWild.Helpers.GetField<AddMechanicsFeature.MechanicsFeatureType>(__instance, "m_Feature");
            if (feature == AddMechanicsFeature.MechanicsFeatureType.ShieldBash)
            {
                no_animation_action = true;
                EventBus.RaiseEvent<IUnitActiveEquipmentSetHandler>((Action<IUnitActiveEquipmentSetHandler>)(h => h.HandleUnitChangeActiveEquipmentSet(__instance.Owner)));
                no_animation_action = false;
            }
        }
    }

    //make signal from created weapons no longer consume move action
    [Harmony12.HarmonyPatch(typeof(UnitViewHandsEquipment))]
    [Harmony12.HarmonyPatch("HandleEquipmentSetChanged", Harmony12.MethodType.Normal)]
    class UnitViewHandsEquipment_HandleEquipmentSetChanged_Patch
    {
        static bool Prefix(UnitViewHandsEquipment __instance)
        {
            Main.TraceLog();
            var tr = Harmony12.Traverse.Create(__instance);

            if (UnitBody__SetEmptyHandWeapon__Patch.no_animation_action)
            {
                //Main.logger.Log("Skipping Weapon change animation");
                //tr.Method("UpdateActiveWeaponSetImmediately").GetValue();
                return false;
            }

            return true;
        }
    }


    //allow to remove summoned weapons during combat
    [Harmony12.HarmonyPatch(typeof(WeaponSlot))]
    [Harmony12.HarmonyPatch("CanRemoveItem", Harmony12.MethodType.Normal)]
    class WeaponSlot_CanRemoveItem_Patch
    {
        static void Postfix(WeaponSlot __instance, ref bool __result)
        {
            Main.TraceLog();
            if (!__result)
            {
                if (__instance.MaybeItem != null)
                {
                    if (NewMechanics.EnchantmentMechanics.Helpers.isSummoned(__instance.MaybeItem) 
                        || isExtraLimb(__instance.Owner?.Unit, __instance.MaybeItem)
                        )
                    {
                        __result = canRemoveBase(__instance);
                    }
                }
            }
        }


        static bool isExtraLimb(UnitEntityData unit, ItemEntity item)
        {
            if (unit?.Body?.NotPolymorphedAdditionalLimbs == null)
            {
                return false;
            }

            return unit.Body.NotPolymorphedAdditionalLimbs.Any(l => l.MaybeItem == item);
        }

        static bool canRemoveBase(WeaponSlot slot)
        {
            if (ItemSlot.IgnoreLock)
                return true;
            if (slot.Lock)
                return false;
            ItemEntity itemEntity = slot.MaybeItem;
            return (itemEntity != null ? (itemEntity.IsNonRemovable ? 1 : 0) : 0) == 0;
        }
    }


    //make signal from created weapons no longer consume move action
    [Harmony12.HarmonyPatch(typeof(TurnController))]
    [Harmony12.HarmonyPatch("HandleUnitChangeActiveEquipmentSet", Harmony12.MethodType.Normal)]
    class TurnController_HandleEquipmentSetChanged_Patch
    {
        static bool Prefix(TurnController __instance, UnitDescriptor unit)
        {
            Main.TraceLog();
            if (UnitBody__SetEmptyHandWeapon__Patch.no_animation_action)
            {
                return false;
            }

            return true;
        }
    }

    [Harmony12.HarmonyPatch(typeof(UnitBody))]
    [Harmony12.HarmonyPatch("RemoveEmptyHandWeapon", Harmony12.MethodType.Normal)]
    class UnitBody__RemoveEmptyHandWeaponEmptyHandWeapon__Patch
    {
        //signal weapon set change to notify that weapons could have been changed
        static void Postfix(UnitBody __instance, BlueprintItemWeapon weapon)
        {
            Main.TraceLog();
            UnitBody__SetEmptyHandWeapon__Patch.no_animation_action = true;
            EventBus.RaiseEvent<IUnitActiveEquipmentSetHandler>((Action<IUnitActiveEquipmentSetHandler>)(h => h.HandleUnitChangeActiveEquipmentSet(__instance.Owner)));
            UnitBody__SetEmptyHandWeapon__Patch.no_animation_action = false;
        }
    }




    public class UnitPartConsiderAsLightWeapon : AdditiveUnitPart
    {
        public bool canBeUsedOn(ItemEntityWeapon weapon)
        {
            if (buffs.Empty())
            {
                return false;
            }

            foreach (var b in buffs)
            {
                bool result = false;
                b.CallComponents<ConsiderAsLightWeaponBase>(c => result = c.canBeUsedOn(weapon));
                if (result)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public abstract class ConsiderAsLightWeaponBase : OwnedGameLogicComponent<UnitDescriptor>
    {
        abstract public bool canBeUsedOn(ItemEntityWeapon weapon);

        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartConsiderAsLightWeapon>().addBuff(this.Fact);
        }


        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartConsiderAsLightWeapon>().removeBuff(this.Fact);
        }
    }

    public class ConsiderWeaponCategoriesAsLightWeapon : ConsiderAsLightWeaponBase
    {
        public WeaponCategory[] categories;

        public override void OnFactActivate()
        {
            base.OnFactActivate();
            foreach (var c in categories)
            {
                this.Owner.Ensure<DamageGracePart>().AddEntry(c, this.Fact);
            }
        }

        public override void OnFactDeactivate()
        {
            base.OnFactDeactivate();
            foreach (var c in categories)
            {
                this.Owner.Ensure<DamageGracePart>().RemoveEntry(this.Fact);
            }
        }

        public override bool canBeUsedOn(ItemEntityWeapon weapon)
        {
            if (weapon == null)
            {
                return false;
            }

            return categories.Contains(weapon.Blueprint.Category);
        }
    }


    [Harmony12.HarmonyPatch(typeof(CharSAttack))]
    [Harmony12.HarmonyPatch("SetupMainAttacks", Harmony12.MethodType.Normal)]
    class CharSAttack__SetupMainAttacks__Patch
    {
        static IEnumerable<Harmony12.CodeInstruction> Transpiler(IEnumerable<Harmony12.CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var check_is_unarmed = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("IsUnarmed")); //checking is unarmed on weapon on primary hand

            codes[check_is_unarmed] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_1);
            codes.Insert(check_is_unarmed + 1, new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call, new Func<BlueprintItemWeapon, UnitDescriptor, bool>(considerUnarmedAndIgnore).Method));

            check_is_unarmed = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("IsUnarmed")); //checking is unarmed on weapon on secondary hand
            codes[check_is_unarmed] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_1);
            codes.Insert(check_is_unarmed + 1, new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call, new Func<BlueprintItemWeapon, UnitDescriptor, bool>(considerUnarmedAndIgnoreOffHand).Method));
            return codes.AsEnumerable();
        }


        private static bool considerUnarmedAndIgnore(BlueprintItemWeapon weapon, UnitDescriptor unit)
        {
            return Aux.isMainHandUnarmedAndCanBeIgnored(weapon, unit);
        }

        private static bool considerUnarmedAndIgnoreOffHand(BlueprintItemWeapon weapon, UnitDescriptor unit)
        {
            return Aux.isOffHandUnarmedAndCanBeIgnored(weapon, unit);
        }
    }


    [Harmony12.HarmonyPatch(typeof(RuleCalculateAttacksCount))]
    [Harmony12.HarmonyPatch("OnTrigger", Harmony12.MethodType.Normal)]
    class RuleCalculateAttacksCount__OnTrigger__Patch
    {
        /*static IEnumerable<Harmony12.CodeInstruction> Transpiler(IEnumerable<Harmony12.CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var check_is_unarmed = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("IsUnarmed")); //checking is unarmed on weapon on primary hand

            codes[check_is_unarmed] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_0);
            codes.Insert(check_is_unarmed + 1, new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call, new Func<BlueprintItemWeapon, RuleCalculateAttacksCount, bool>(considerUnarmedAndIgnore).Method));

            check_is_unarmed = codes.IndexOf(codes.FindAll(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("IsUnarmed"))[2]);
            codes[check_is_unarmed] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_0);
            codes.Insert(check_is_unarmed + 1, new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call, new Func<BlueprintItemWeapon, RuleCalculateAttacksCount, bool>(considerUnarmedAndIgnoreOffHand).Method));
            return codes.AsEnumerable();
        }*/

        static bool Prefix(RuleCalculateAttacksCount __instance, RulebookEventContext context)
        {
            int bab = (int)__instance.Initiator.Stats.BaseAttackBonus;
            int num_penalized_attacks = Math.Min(Math.Max(0, bab / 5 - (bab % 5 == 0 ? 1 : 0)), 3);
            HandSlot primary_hand = __instance.Initiator.Body.PrimaryHand;
            HandSlot secondary_hand = __instance.Initiator.Body.SecondaryHand;
            BlueprintItemWeapon blueprint1 = primary_hand.MaybeWeapon?.Blueprint;
            BlueprintItemWeapon blueprint2 = secondary_hand.MaybeShield != null ? ((bool)__instance.Initiator.Descriptor.State.Features.ShieldBash ? secondary_hand.MaybeShield.WeaponComponent?.Blueprint : null) : secondary_hand.MaybeWeapon?.Blueprint;
            int num = primary_hand.MaybeWeapon == null ? 0 : (primary_hand.MaybeWeapon.HoldInTwoHands ? 1 : 0);
            if ((secondary_hand.MaybeWeapon?.HoldInTwoHands).GetValueOrDefault() == false 
                && (blueprint1 != null) 
                && (!considerUnarmedAndIgnore(blueprint1, __instance) || blueprint2 == null || blueprint2.IsUnarmed)
                )
            {
                ++__instance.PrimaryHand.AdditionalAttacks;
                if (!blueprint1.IsNatural || (bool)__instance.Initiator.Descriptor.State.Features.IterativeNaturalAttacks || __instance.ForceIterativeNaturealAttacks || blueprint1.IsUnarmed)
                    __instance.PrimaryHand.PenalizedAttacks += Math.Max(0, num_penalized_attacks);
            }
            if ((secondary_hand.MaybeWeapon?.HoldInTwoHands).GetValueOrDefault() == true 
                || (blueprint2 == null) 
                || considerUnarmedAndIgnoreOffHand(blueprint2, __instance) && (blueprint1 != null)
                || (primary_hand.MaybeWeapon?.HoldInTwoHands).GetValueOrDefault() == true
                )
                return false;
            ++__instance.SecondaryHand.AdditionalAttacks;
            if (blueprint1 == null || !considerUnarmedAndIgnore(blueprint1, __instance)
                && (blueprint1 == null || !blueprint1.IsNatural || !(bool)__instance.Initiator.Descriptor.State.Features.IterativeNaturalAttacks && !__instance.ForceIterativeNaturealAttacks)
                || (blueprint2.IsUnarmed)
                )
                return false;
            __instance.SecondaryHand.PenalizedAttacks += Math.Max(0, num_penalized_attacks);
            return false;
        }


        private static bool considerUnarmedAndIgnore(BlueprintItemWeapon weapon, RuleCalculateAttacksCount evt)
        {
            return Aux.isMainHandUnarmedAndCanBeIgnored(weapon, evt.Initiator.Descriptor);
        }


        private static bool considerUnarmedAndIgnoreOffHand(BlueprintItemWeapon weapon, RuleCalculateAttacksCount evt)
        {
            return Aux.isOffHandUnarmedAndCanBeIgnored(weapon, evt.Initiator.Descriptor);
        }
    }

    [Harmony12.HarmonyPatch(typeof(UnitAttack))]
    [Harmony12.HarmonyPatch("TriggerAttackRule", Harmony12.MethodType.Normal)]
    class UnitAttack__TriggerAttackRule__Patch
    {
        static bool Prefix(UnitAttack __instance, AttackHandInfo attack)
        {
            TwoWeaponFightingDamagePenalty__OnEventAboutToTrigger__Patch.is_off_hand = (attack?.Hand != null && attack?.Hand == __instance.Executor?.Body?.SecondaryHand);
            return true;
        }

        static void Postfix(UnitAttack __instance, AttackHandInfo attack)
        {
            TwoWeaponFightingDamagePenalty__OnEventAboutToTrigger__Patch.is_off_hand = false;
        }
    }


    [Harmony12.HarmonyPatch(typeof(UIUtilityItem))]
    [Harmony12.HarmonyPatch("GetAttackParametersEntity", Harmony12.MethodType.Normal)]
    class UIUtilityItem__GetAttackParametersEntity__Patch
    {
        static bool Prefix(ItemEntityWeapon weaponEntity, UnitDescriptor unit, bool primaryHand)
        {
            TwoWeaponFightingDamagePenalty__OnEventAboutToTrigger__Patch.is_off_hand = !primaryHand;
            return true;
        }

        static void Postfix(ItemEntityWeapon weaponEntity, UnitDescriptor unit, bool primaryHand)
        {
            TwoWeaponFightingDamagePenalty__OnEventAboutToTrigger__Patch.is_off_hand = false;
        }
    }


    [Harmony12.HarmonyPatch(typeof(TwoWeaponFightingDamagePenalty))]
    [Harmony12.HarmonyPatch("OnEventAboutToTrigger", Harmony12.MethodType.Normal)]
    class TwoWeaponFightingDamagePenalty__OnEventAboutToTrigger__Patch
    {
        public static bool is_off_hand = false;
        static bool Prefix(TwoWeaponFightingDamagePenalty __instance, RuleCalculateWeaponStats evt)
        {
            ItemEntityWeapon maybeWeapon1 = evt.Initiator.Body.PrimaryHand.MaybeWeapon;
            ItemEntityWeapon maybeWeapon2 = evt.Initiator.Body.SecondaryHand.MaybeWeapon;

            var brawler_part = evt.Initiator?.Get<Brawler.UnitPartBrawler>();
            if ((brawler_part?.checkTwoWeapponFlurry()).GetValueOrDefault())
            {
                is_off_hand = false;
                return false;
            }

            if (evt.Weapon == null
               || maybeWeapon1 == null
               || maybeWeapon2 == null
               || (maybeWeapon1.Blueprint.IsNatural && (!maybeWeapon1.Blueprint.IsUnarmed || Aux.isMainHandUnarmedAndCanBeIgnored(maybeWeapon1.Blueprint, evt.Initiator.Descriptor)))
               || (maybeWeapon2.Blueprint.IsNatural && (!maybeWeapon2.Blueprint.IsUnarmed || Aux.isOffHandUnarmedAndCanBeIgnored(maybeWeapon2.Blueprint, evt.Initiator.Descriptor)))
               || maybeWeapon2 != evt.Weapon
               || (bool)evt.Initiator.Descriptor.State.Features.DoubleSlice
               || ((evt.Weapon?.Blueprint.IsUnarmed).GetValueOrDefault() && !is_off_hand)
               )
            {
                is_off_hand = false;
                return false;
            }
            evt.SecondaryWeapon = true;
            is_off_hand = false;
            return false;
        }
    }

    static public class Aux
    {
        public static bool isMainHandUnarmedAndCanBeIgnored(BlueprintItemWeapon weapon, UnitDescriptor unit)
        {
            return weapon.IsUnarmed && !(bool)unit.State.Features.ImprovedUnarmedStrike;
        }

        public static bool isOffHandUnarmedAndCanBeIgnored(BlueprintItemWeapon weapon, UnitDescriptor unit)
        {
            return weapon.IsUnarmed && !((bool)unit.State.Features.ImprovedUnarmedStrike && (unit.Get<UnitPartUnarmedTwf>()?.active()).GetValueOrDefault());
        }
    }



}
