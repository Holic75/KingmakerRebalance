using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Newtonsoft.Json;
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
    [Harmony12.HarmonyPatch("LaunchProjectiles", Harmony12.MethodType.Normal)]
    public class RuleAttackWithWeapon_LaunchProjectiles_Patch
    {
        static bool Prefix(RuleAttackWithWeapon __instance, BlueprintProjectile[] projectiles)
        {
            var tr = Harmony12.Traverse.Create(__instance);
            foreach (BlueprintProjectile projectile in projectiles)
            {
                if (projectile != null)
                {
                    tr.Method("LaunchProjectile", projectile, true).GetValue();
                    if (__instance.Weapon.Blueprint.Type.FighterGroup == WeaponFighterGroup.Bows && __instance.Initiator.Descriptor.State.Features.Manyshot
                        && (__instance.IsFirstAttack && __instance.IsFullAttack && !__instance.IsAttackOfOpportunity)
                        && (__instance.Initiator.Get<UnitPartManyshotNotAvailable>() == null || !__instance.Initiator.Get<UnitPartManyshotNotAvailable>().active())
                        )
                    {
                        tr.Method("LaunchProjectile", projectile, false).GetValue();
                    }
                }
            }

            return false;
        }
    }


    [Harmony12.HarmonyPatch(typeof(AddParametrizedFeatures))]
    [Harmony12.HarmonyPatch("OnFactActivate", Harmony12.MethodType.Normal)]
    class Patch_AddParametrizedFeatures_OnFactActivate_Transpiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var add_fact_idx = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Call && x.operand.ToString().Contains("AddFact"));

            codes[add_fact_idx] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                           new Func<UnitDescriptor, BlueprintUnitFact, MechanicsContext, FeatureParam, Kingmaker.UnitLogic.Feature>(maybeAddFact).Method
                                                                           );


            return codes.AsEnumerable();
        }


        static private Kingmaker.UnitLogic.Feature maybeAddFact(UnitDescriptor descriptor,  BlueprintUnitFact feature, MechanicsContext context, FeatureParam param)
        {
            if (descriptor.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => p.Blueprint == feature).Any(a => a.Param == param))
            {
                return null;
            }
            return descriptor.AddFact<Kingmaker.UnitLogic.Feature>(feature, context, param);
        }
    }


    /*[AllowedOn(typeof(BlueprintUnitFact))]
    [AllowedOn(typeof(BlueprintUnit))]
    public class AddParametrizedFeatureWithCheck : OwnedGameLogicComponent<UnitDescriptor>
    {
        public BlueprintParametrizedFeature feature;
        public FeatureParam param;
        [JsonProperty]
        private Feature m_AppliedFeature = null;

        public override void OnFactActivate()
        {
            if (Owner.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => p.Blueprint == feature).Any(a => a.Param == param))
            {
                return;
            }
            m_AppliedFeature = this.Owner.AddFact<Kingmaker.UnitLogic.Feature>(feature, (MechanicsContext)null, param);
        }

        public override void OnFactDeactivate()
        {
            if (m_AppliedFeature != null)
            {
                this.Owner.RemoveFact(m_AppliedFeature);
            }
            m_AppliedFeature = null;
        }
    }*/
}
