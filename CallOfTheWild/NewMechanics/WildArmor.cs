using Kingmaker.Blueprints;
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

namespace CallOfTheWild.WildArmorMechanics
{
    public class UnitPartWildArmor : UnitPart
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
        static public void Postfix(UnitBody __instance)
        {
            if (__instance.Owner.Ensure<UnitPartWildArmor>().active())
            {
                __instance.Armor.ReleaseDeactivateFlag();
            }
        }
    }


    [Harmony12.HarmonyPatch(typeof(UnitBody))]
    [Harmony12.HarmonyPatch("CancelPolymorphEffect", Harmony12.MethodType.Normal)]
    class Patch_UnitBody_CancelPolymorphEffect
    {
        static public void Prefix(UnitBody __instance)
        {
            if (__instance.Armor.Active)
            {
                __instance.Armor.RetainDeactivateFlag();
            }
        }
    }
}
