using CallOfTheWild.HealingMechanics;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.UndeadMechanics
{
    public class UnitPartConsiderUndeadForHealing : AdditiveUnitPart
    {
        public bool active()
        {
            return !buffs.Empty();
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class ConsiderUndeadForHealing : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartConsiderUndeadForHealing>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartConsiderUndeadForHealing>().removeBuff(this.Fact);
        }
    }


    public class ContextConditionConsideredAsUndeadForenergy : ContextCondition
    {
        protected override string GetConditionCaption()
        {
            return string.Empty;
        }

        protected override bool CheckCondition()
        {
            return this.Target.Unit.Descriptor.IsUndead;
        }
    }


    [Harmony12.HarmonyPatch(typeof(UnitDescriptor), "IsUndead", Harmony12.MethodType.Getter)]
    public class UnitDescriptor_IsUndead_Patch
    {
        static void Postfix(UnitDescriptor __instance, ref bool __result)
        {
            try
            {
                if (!__result) __result = __instance.Ensure< UnitPartConsiderUndeadForHealing>().active();
            }
            catch (Exception ex)
            {
                Main.logger.LogException(ex);
            }
        }
    }
}
