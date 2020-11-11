using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.InitiativeMechanics
{
    public class CanActInSurpriseRoundLogic : OwnedGameLogicComponent<UnitDescriptor>, IUnitInitiativeHandler
    {
        public void HandleUnitRollsInitiative(RuleInitiativeRoll rule)
        {
            if (rule.Initiator.Descriptor != Owner) return;

            // Are there other units not waiting on inititative?
            foreach (var unit in Game.Instance.State.Units.InCombat().CombatStates())
            {
                if (unit.IsWaitingInitiative) continue;

                // Another unit isn't waiting, but we just rolled, so it must be the surprise round.
                // Let us act too.
                //Log.Write($"Can act in surprise round: {Owner.CharacterName}, because other unit could: {unit.Unit.Descriptor.CharacterName}");
                unit.Cooldown.Initiative = 0;
            }
        }
    }


    public class ActionInSurpriseRound : OwnedGameLogicComponent<UnitDescriptor>, IUnitInitiativeHandler
    {
        public ActionList actions;
        public void HandleUnitRollsInitiative(RuleInitiativeRoll rule)
        {
            if (rule.Initiator.Descriptor != Owner) return;

            // Are there other units not waiting on inititative?
            foreach (var unit in Game.Instance.State.Units.InCombat().CombatStates())
            {
                if (unit.IsWaitingInitiative) continue;
                //we are in surprise round
                (this.Fact as IFactContextOwner).RunActionInContext(actions, this.Owner.Unit);
                return;
            }
        }
    }
}
