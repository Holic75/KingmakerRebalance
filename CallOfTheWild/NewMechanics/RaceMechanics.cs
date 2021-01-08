using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.RaceMechanics
{
    public class PrerequisiteRace : Prerequisite
    {
        public BlueprintRace race;

        public override bool Check(
          FeatureSelectionState selectionState,
          UnitDescriptor unit,
          LevelUpState state)
        {
            return race == unit.Progression.Race;
        }

        public override string GetUIText()
        {
            return race.Name;
        }
    }



    public class SpellsDCBonusAgainstSameRace : OwnedGameLogicComponent<UnitDescriptor>, MetamagicFeats.IRuleSavingThrowTriggered
    {
        public ContextValue value;
        public ModifierDescriptor descriptor;
        public BlueprintAbility[] spells;

        public void ruleSavingThrowBeforeTrigger(RuleSavingThrow evt)
        {
            var context = evt.Reason?.Context;
            if (context == null)
            {
                return;
            }

            var caster = context.MaybeCaster;
            if (caster == null)
            {
                return;
            }

            if (caster != this.Owner.Unit)
            {
                return;
            }

            if (evt.Initiator == null)
            {
                return;
            }


            if (caster.Descriptor.Progression?.Race != evt.Initiator.Descriptor.Progression?.Race)
            {
                return;
            }

            if (spells.Contains(context.SourceAbility) || spells.Contains(context.SourceAbility?.Parent))
            {
                var bonus = -value.Calculate(this.Fact.MaybeContext);
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveWill.AddModifier(bonus, (GameLogicComponent)this, this.descriptor));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveReflex.AddModifier(bonus, (GameLogicComponent)this, this.descriptor));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveFortitude.AddModifier(bonus, (GameLogicComponent)this, this.descriptor));
            }
        }

        public void ruleSavingThrowTriggered(RuleSavingThrow evt)
        {

        }
    }

}
