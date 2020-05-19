using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.HoldingItemsMechanics
{
    [Harmony12.HarmonyPatch(typeof(ItemEntityWeapon))]
    [Harmony12.HarmonyPatch("HoldInTwoHands", Harmony12.MethodType.Getter)]
    class ItemEntityWeapon__HoldInTwoHands__Patch
    {

        static void Postfix(ItemEntityWeapon __instance, ref bool __result)
        {
            bool spell_combat = false;
            UnitPartMagus unit_part_magus = __instance.Wielder?.Get<UnitPartMagus>();
            if ((bool)(unit_part_magus) && unit_part_magus.SpellCombat.Active)
            {
                spell_combat = true;
            }

            var unit_part = __instance.Owner?.Get<UnitPartCanHold2hWeaponIn1h>();

            if (unit_part == null)
            {
                return;
            }

            if (__instance.Blueprint.IsTwoHanded
                || (__instance.Blueprint.IsOneHandedWhichCanBeUsedWithTwoHands && __result == false && !spell_combat))
            {
                __result = unit_part.canBeUsedAs2h(__instance);
            }
        }
    }


    [Harmony12.HarmonyPatch(typeof(HandSlot))]
    [Harmony12.HarmonyPatch("OnItemInserted", Harmony12.MethodType.Normal)]
    class HandSlot__OnItemInserted__Patch
    {

        static bool Prefix(HandSlot __instance)
        {
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
                HandSlot holdingSlot = weapon.HoldingSlot as HandSlot;
                if (holdingSlot != null)
                    return !holdingSlot.PairSlot.HasItem;
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

            var shield = (evt.Weapon?.HoldingSlot as HandSlot).PairSlot.MaybeShield;
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

    internal class Helpers
    {
        internal static bool hasFreeHand(HandSlot hand_slot)
        {
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
        static void Postfix(UnitPartMagus __instance, UnitDescriptor unit, ref bool __result)
        {
            if (__result == false)
            {
                __result = unit.Body.SecondaryHand.HasItem && Helpers.hasFreeHand(unit.Body.SecondaryHand);
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
            if (!Helpers.hasShield2(__instance.Owner.Body.SecondaryHand) && (!__instance.Owner.Body.Armor.HasArmor || !__instance.Owner.Body.Armor.Armor.Blueprint.IsArmor))
            {
                __instance.AddFact();
            }
            else
                __instance.RemoveFact();

            return false;
        }

        /*static IEnumerable<Harmony12.CodeInstruction> Transpiler(IEnumerable<Harmony12.CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var check_shield = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("HasShield"));

            codes[check_shield] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call, new Func<HandSlot, bool>(Helpers.hasShield2).Method);
            return codes.AsEnumerable();
        }*/
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


    [Harmony12.HarmonyPatch(typeof(ItemEntityWeapon))]
    [Harmony12.HarmonyPatch("GetAnimationStyle", Harmony12.MethodType.Normal)]
    class ItemEntityWeapon__GetAnimationStyle__Patch
    {
        static void Postfix(ItemEntityWeapon __instance, bool forDollRoom, ref WeaponAnimationStyle __result)
        {
            if ((__instance.Blueprint.IsTwoHanded && !__instance.HoldInTwoHands) //2h that is held as 1h
                || ((__instance.Blueprint.IsTwoHanded || __instance.Blueprint.IsOneHandedWhichCanBeUsedWithTwoHands) && forDollRoom)) // make weapon look 1h in the doll room to see the shield if possible
            {
                if ((__instance?.HoldingSlot as HandSlot).PairSlot?.MaybeShield == null)
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


}
