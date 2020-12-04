using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.InsinuatorMechanics
{
    class UnitPartInsinuatorOutsiderAlignment : AdditiveUnitPart
    {
        public bool active()
        {
            return buffs.Empty();
        }


        public AlignmentMaskType getAlignment()
        {
            if (!active())
            {
                return AlignmentMaskType.None;
            }

            return buffs[0].Blueprint.GetComponent<InsinuatorOutsiderAlignment>().alignment;
        }

        public AlignmentMaskType getOppositeAlignment()
        {
            if (!active())
            {
                return AlignmentMaskType.None;
            }

            return (~buffs[0].Blueprint.GetComponent<InsinuatorOutsiderAlignment>().alignment) & AlignmentMaskType.Any;
        }
    }





    public class InsinuatorOutsiderAlignment : OwnedGameLogicComponent<UnitDescriptor>
    {
        public AlignmentMaskType alignment;

        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartInsinuatorOutsiderAlignment>().addBuff(this.Fact);

        }

        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartInsinuatorOutsiderAlignment>().removeBuff(this.Fact);
        }
    }

    [AllowedOn(typeof(BlueprintAbility))]
    [AllowMultipleComponents]
    public class AbilityCasterInsinuatorAlignmentNeutral : BlueprintComponent, IAbilityCasterChecker
    {
        public bool CorrectCaster(UnitEntityData caster)
        {
            var unit_part = caster.Get<UnitPartInsinuatorOutsiderAlignment>();
            if (unit_part == null || !unit_part.active())
            {
                return false;
            }
            return (unit_part.getAlignment() & (AlignmentMaskType.Good | AlignmentMaskType.Evil)) == 0;
        }

        public string GetReason()
        {
            return "Requires invocation of neutral outsider";
        }
    }
}
