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
            if ((evt.Spell?.SourceItemUsableBlueprint == null) || (evt.Spell.StickyTouch != null))
                return;
            evt.SpellFailureChance = Math.Max(evt.SpellFailureChance, this.chance);
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
