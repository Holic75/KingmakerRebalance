using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.FavoredEnemyMechanics
{
    [AllowMultipleComponents]
    [ComponentName("Armor check penalty increase")]
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class ACBonusAgainstFavoredEnemy : OwnedGameLogicComponent<UnitDescriptor>, ITargetRulebookHandler<RuleAttackRoll>, IRulebookHandler<RuleAttackRoll>, ITargetRulebookSubscriber
    {
        public ModifierDescriptor descriptor;
        public ContextValue value;

        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            var unit_part_fe = this.Owner.Get<UnitPartFavoredEnemy>();
            if (unit_part_fe == null)
            {
                return;
            }

            foreach (var fe in unit_part_fe.Entries)
            {
                if ((fe.CheckedFeatures).Any<BlueprintUnitFact>((Func<BlueprintUnitFact, bool>)(p => evt.Initiator.Descriptor.HasFact(p))))
                {
                    evt.AddTemporaryModifier(evt.Target.Stats.AC.AddModifier(this.value.Calculate(this.Fact.MaybeContext) * this.Fact.GetRank(), (GameLogicComponent)this, this.descriptor));
                    break;
                }
            }
        }

        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
        }
    }
}
