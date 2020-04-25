using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.FeatureMechanics
{

    public class UnitPartManyshotNotAvailable : AdditiveUnitPart
    {
        public bool active() { return !buffs.Empty(); }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class DeactivateManyshot : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartManyshotNotAvailable>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartManyshotNotAvailable>().removeBuff(this.Fact);
        }
    }


    [Harmony12.HarmonyPatch(typeof(RuleAttackWithWeapon))]
    [Harmony12.HarmonyPatch("LaunchProjectile", Harmony12.MethodType.Normal)]
    public class RuleAttackWithWeapon_LaunchProjectile_Patch
    {
        static bool Prefix(RuleAttackWithWeapon __instance, BlueprintProjectile projectile, bool first)
        {
            if ((!first) && __instance.Weapon.Blueprint.Type.FighterGroup == WeaponFighterGroup.Bows && (bool)__instance.Initiator.Descriptor.State.Features.Manyshot && (__instance.IsFirstAttack && __instance.IsFullAttack))
            {
                if (__instance.Initiator.Get<UnitPartManyshotNotAvailable>() != null && __instance.Initiator.Get<UnitPartManyshotNotAvailable>().active())
                {
                    return false;
                }
            }

            return true;
        }
    }

}
