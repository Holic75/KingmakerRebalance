using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.SpellFailureMechanics
{
    class ItemUseFailure: OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCastSpell>, IRulebookHandler<RuleCastSpell>, IInitiatorRulebookSubscriber
    {
        public int chance;

        public void OnEventAboutToTrigger(RuleCastSpell evt)
        {
            //Main.logger.Log("Checking " + evt.Spell.Name);
            if ((evt.Spell?.SourceItemUsableBlueprint == null) || (evt.Spell.StickyTouch != null))
                return;                   
            evt.SpellFailureChance = Math.Max(evt.SpellFailureChance, this.chance);
            //Main.logger.Log("Ok " + evt.SpellFailureChance.ToString());
        }

        public void OnEventDidTrigger(RuleCastSpell evt)
        {

        }
    }


    class SpellFailureChance : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCastSpell>, IRulebookHandler<RuleCastSpell>, IInitiatorRulebookSubscriber
    {
        public int chance;

        public void OnEventAboutToTrigger(RuleCastSpell evt)
        {
            if (!evt.Spell.Blueprint.IsSpell || (evt.Spell.StickyTouch != null))
                return;
            evt.SpellFailureChance = Math.Max(evt.SpellFailureChance, this.chance);
        }

        public void OnEventDidTrigger(RuleCastSpell evt)
        {

        }
    }
}
