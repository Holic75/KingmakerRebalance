using Kingmaker.Blueprints;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.FlatFootedMechanics
{

    public class UnitPartNotFlatFootedBeforeInitiative : CallOfTheWild.AdditiveUnitPart
    {
        public bool active()
        {
            return !this.buffs.Empty();
        }
    }


    [Harmony12.HarmonyPatch(typeof(RuleCheckTargetFlatFooted))]
    [Harmony12.HarmonyPatch("OnTrigger", Harmony12.MethodType.Normal)]
    class RuleCheckTargetFlatFooted__OnTrigger__Patch
    {
        static bool Prefix(RuleCheckTargetFlatFooted __instance)
        {
            bool ignore_initiative = (__instance.Target.Get<UnitPartNotFlatFootedBeforeInitiative>()?.active()).GetValueOrDefault();
            bool is_flat_footed = __instance.ForceFlatFooted 
                                      || !__instance.Target.CombatState.CanActInCombat && !__instance.IgnoreVisibility && !ignore_initiative
                                      || (__instance.Target.Descriptor.State.IsHelpless 
                                      || __instance.Target.Descriptor.State.HasCondition(UnitCondition.Stunned) 
                                      || !__instance.Target.Memory.Contains(__instance.Initiator) && !__instance.IgnoreVisibility) 
                                      || (UnitPartConcealment.Calculate(__instance.Target, __instance.Initiator, false) == Concealment.Total && !__instance.IgnoreConcealment 
                                          || __instance.Target.Descriptor.State.HasCondition(UnitCondition.LoseDexterityToAC)) 
                                      || ((__instance.Target.Descriptor.State.HasCondition(UnitCondition.Shaken) 
                                          || __instance.Target.Descriptor.State.HasCondition(UnitCondition.Frightened)) 
                                          && (bool)__instance.Initiator.Descriptor.State.Features.ShatterDefenses);

            //TODO: fix shatter defenses not to apply on first hit
            var tr = Harmony12.Traverse.Create(__instance);
            tr.Property("IsFlatFooted").SetValue(is_flat_footed);
            return false;
        }
    }


    public class NotFlatFootedBeforeInitiative : OwnedGameLogicComponent<UnitDescriptor>
    {
        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartNotFlatFootedBeforeInitiative>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartNotFlatFootedBeforeInitiative>().removeBuff(this.Fact);
        }
    }
}
