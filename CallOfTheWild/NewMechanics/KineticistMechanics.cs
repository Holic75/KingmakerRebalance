using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.KineticistMechanics
{
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class DecreaseWildTalentCostWithActionOnBurn : OwnedGameLogicComponent<UnitDescriptor>, IKineticistCalculateAbilityCostHandler, IKinetecistAcceptBurnHandler, IGlobalSubscriber, IUnitSubscriber
    {
        public int value = 1;
        public ActionList actions;

        private MechanicsContext Context
        {
            get
            {
                return this.Fact.MaybeContext;
            }
        }

        public void HandleKineticistCalculateAbilityCost(UnitDescriptor caster, BlueprintAbility abilityBlueprint, ref KineticistAbilityBurnCost cost)
        {
            if (caster != this.Owner)
                return;

            var burn_cost = abilityBlueprint.GetComponent<AbilityKineticist>();
            if (burn_cost != null && burn_cost.WildTalentBurnCost > 0)
            {
                //to make it work for wild talents
                cost.Decrease(1, KineticistBurnType.WildTalent);
            }
            else
            {
                cost.IncreaseGatherPower(value);
            }
        }

        public void HandleKineticistAcceptBurn(UnitPartKineticist kinetecist, int burn, AbilityData ability)
        {
           
            if (actions != null)
            {
                (this.Fact as IFactContextOwner)?.RunActionInContext(this.actions, kinetecist.Owner.Unit);
            }
        }
    }
}
