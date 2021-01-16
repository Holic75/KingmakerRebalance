﻿using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Items;
using Kingmaker.Items.Slots;

namespace CallOfTheWild.WildArmorMechanics
{
    public class UnitPartWildArmor : AdditiveUnitPart
    {
        public bool active()
        {
            return !buffs.Empty();
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class WildArmorLogic : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {
        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartWildArmor>().addBuff(this.Fact);
        }


        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartWildArmor>().removeBuff(this.Fact);
        }
    }


    [Harmony12.HarmonyPatch(typeof(UnitBody))]
    [Harmony12.HarmonyPatch("ApplyPolymorphEffect", Harmony12.MethodType.Normal)]
    class Patch_UnitBody_ApplyPolymorphEffect
    {
        static public BlueprintItem thundering_claw = Main.library.Get<BlueprintItem>("e5b46c4b36c2ca74d8a30f68a93bc77c");
        static public void Postfix(UnitBody __instance)
        {
            Main.TraceLog();
            if (__instance.Owner.Ensure<UnitPartWildArmor>().active())
            {
                if (__instance.Armor.HasArmor && hasWildEnchant(__instance.Armor.Armor))
                {
                    __instance.Armor.ReleaseDeactivateFlag();
                }
            }
            var primary_hand = __instance.CurrentHandsEquipmentSet?.PrimaryHand;
            var secondary_hand = __instance.CurrentHandsEquipmentSet?.SecondaryHand;
            if (primary_hand != null
                && (!primary_hand.HasShield || !hasWildEnchant(primary_hand.Shield?.ArmorComponent) || !__instance.Owner.Ensure<UnitPartWildArmor>().active())
                && (primary_hand.MaybeItem?.Blueprint != thundering_claw))
            {
                primary_hand.RetainDeactivateFlag();
            }
            if (secondary_hand != null
                && (!secondary_hand.HasShield || !hasWildEnchant(secondary_hand.Shield?.ArmorComponent) || !__instance.Owner.Ensure<UnitPartWildArmor>().active())
                && (secondary_hand.MaybeItem?.Blueprint != thundering_claw))
            {
                secondary_hand.RetainDeactivateFlag();
            }
            EventBus.RaiseEvent<IPolymorphOnHandler>((Action<IPolymorphOnHandler>)(h => h.polymorphOn(__instance.Owner)));
        }


        static public bool hasWildEnchant(ItemEntityArmor armor)
        {
            if (armor == null)
            {
                return false;
            }
            if (armor.EnchantmentsCollection == null)
            {
                return false;
            }

            return armor.EnchantmentsCollection.HasFact(Wildshape.wild_armor_enchant);
        }
    }



    public interface IPolymorphOnHandler : IGlobalSubscriber
    {
        void polymorphOn(UnitDescriptor unit);
    }


    [Harmony12.HarmonyPatch(typeof(UnitBody))]
    [Harmony12.HarmonyPatch("CancelPolymorphEffect", Harmony12.MethodType.Normal)]
    class Patch_UnitBody_CancelPolymorphEffect
    {
        static public BlueprintItem thundering_claw = Main.library.Get<BlueprintItem>("e5b46c4b36c2ca74d8a30f68a93bc77c");
        static public bool Prefix(UnitBody __instance)
        {
            Main.TraceLog();
            if (__instance.Owner.Ensure<UnitPartWildArmor>().active())
            {
                if (__instance.Armor.HasArmor && Patch_UnitBody_ApplyPolymorphEffect.hasWildEnchant(__instance.Armor.Armor))
                {
                    __instance.Armor.RetainDeactivateFlag();
                }
            }

            return true;
        }


        static public void Postfix(UnitBody __instance)
        {
            var primary_hand = __instance.CurrentHandsEquipmentSet?.PrimaryHand;
            var secondary_hand = __instance.CurrentHandsEquipmentSet?.SecondaryHand;
            if (primary_hand != null
                && (!primary_hand.HasShield || !Patch_UnitBody_ApplyPolymorphEffect.hasWildEnchant(primary_hand.Shield?.ArmorComponent) || !__instance.Owner.Ensure<UnitPartWildArmor>().active())
                && (primary_hand.MaybeItem?.Blueprint != thundering_claw)
                && primary_hand.Disabled)//?????
            {
                primary_hand.ReleaseDeactivateFlag();
            }
            if (secondary_hand != null
                && (!secondary_hand.HasShield || !Patch_UnitBody_ApplyPolymorphEffect.hasWildEnchant(secondary_hand.Shield?.ArmorComponent) || !__instance.Owner.Ensure<UnitPartWildArmor>().active())
                && (secondary_hand.MaybeItem?.Blueprint != thundering_claw)
                && secondary_hand.Disabled)//????
            {
                secondary_hand.ReleaseDeactivateFlag();
            }

        }
    }

    //fix lock of items slots of polymorphed character on save game load
    [Harmony12.HarmonyPatch(typeof(UnitBody))]
    [Harmony12.HarmonyPatch("PostLoad", Harmony12.MethodType.Normal)]
    class Patch_UnitBody_Postload
    {
        static public void Postfix(UnitBody __instance)
        {
            Main.TraceLog();
            if (__instance.IsPolymorphed)
            {
                Harmony12.Traverse.Create(__instance).Method("SetPolymorphSlotsLock", false).GetValue();
            }
        }
    }
}
