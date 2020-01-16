using CallOfTheWild.HealingMechanics;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using Newtonsoft.Json;
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


    public class ContextConditionHasNegativeEnergyAffinity : ContextCondition
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



    [AllowMultipleComponents]
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AddFeatureIfHasNegativeEnergyAffinity : OwnedGameLogicComponent<UnitDescriptor>, IUnitGainLevelHandler, IGlobalSubscriber
    {
        public bool inverted = false;
        public BlueprintFeature Feature;
        [JsonProperty]
        private Fact m_AppliedFact;

        public override void OnFactActivate()
        {
            this.Apply();
        }

        public override void OnFactDeactivate()
        {
            this.Owner.RemoveFact(this.m_AppliedFact);
            this.m_AppliedFact = (Fact)null;
        }

        public void HandleUnitGainLevel(UnitDescriptor unit, BlueprintCharacterClass @class)
        {
            this.Apply();
        }

        private void Apply()
        {
            if (this.m_AppliedFact != null)
                return;

            if (inverted != this.Owner.Unit.Descriptor.IsUndead)
            {
                this.m_AppliedFact = this.Owner.AddFact(this.Feature, (MechanicsContext)null, (FeatureParam)null);
            }
        }
    }


    [Harmony12.HarmonyPatch(typeof(UnitDescriptor), "IsUndead", Harmony12.MethodType.Getter)]
    public class UnitDescriptor_IsUndead_Patch
    {
        static void Postfix(UnitDescriptor __instance, ref bool __result)
        {
            try
            {
                if (!__result) __result = __instance.Ensure<UnitPartConsiderUndeadForHealing>().active();
            }
            catch (Exception ex)
            {
                Main.logger.LogException(ex);
            }
        }
    }
}
