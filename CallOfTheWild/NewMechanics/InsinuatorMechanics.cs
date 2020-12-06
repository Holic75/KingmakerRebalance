using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Mechanics.Conditions;
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
            return !buffs.Empty();
        }

        public AlignmentMaskType getAlignment()
        {
            if (!active())
            {
                return AlignmentMaskType.None;
            }

            return buffs[0].Blueprint.GetComponent<InsinuatorOutsiderAlignment>().alignment;
        }

        public AlignmentMaskType getSharedAlignment()
        {
            if (!active())
            {
                return AlignmentMaskType.None;
            }

            return buffs[0].Blueprint.GetComponent<InsinuatorOutsiderAlignment>().shared_alignment;
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



    public class ContextConditionNonOutsiderAlignment : ContextCondition
    {
        protected override string GetConditionCaption()
        {
            return string.Format("Check non-outsider alignment");
        }

        protected override bool CheckCondition()
        {
            UnitEntityData unit = this.Target.Unit;
            if (unit == null)
            {
                return false;
            }

            var unit_part = this.Context.MaybeCaster?.Get<UnitPartInsinuatorOutsiderAlignment>();
            if (unit_part == null || !unit_part.active())
            {
                return false;
            }

            return (unit.Descriptor.Alignment.Value.ToMask() & unit_part.getSharedAlignment()) == 0U;
        }
    }


    [AllowedOn(typeof(BlueprintAbility))]
    [AllowMultipleComponents]
    public class AbilityCasterInvokedOutsider : BlueprintComponent, IAbilityCasterChecker
    {
        public bool CorrectCaster(UnitEntityData caster)
        {
            return (caster?.Get<UnitPartInsinuatorOutsiderAlignment>()?.active()).GetValueOrDefault();
        }

        public string GetReason()
        {
            return "Requires invocation of an outsider";
        }
    }



    public class InsinuatorOutsiderAlignment : OwnedGameLogicComponent<UnitDescriptor>
    {
        public AlignmentMaskType alignment;
        public AlignmentMaskType shared_alignment;

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
    public class AbilityCasterMaybeInvokedOutsider : BlueprintComponent, IAbilityCasterChecker
    {
        public bool CorrectCaster(UnitEntityData caster)
        {
            if (!caster.Descriptor.Progression.IsArchetype(Antipaladin.insinuator))
            {
                return true;
            }

            var unit_part = caster.Get<UnitPartInsinuatorOutsiderAlignment>();
            if (unit_part == null || !unit_part.active())
            {
                return false;
            }
            return true;
        }

        public string GetReason()
        {
            return "Requires invocation of an outsider";
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
            return (unit_part.getAlignment() & (AlignmentMaskType.LawfulNeutral | AlignmentMaskType.TrueNeutral | AlignmentMaskType.ChaoticNeutral)) > 0;
        }

        public string GetReason()
        {
            return "Requires invocation of neutral outsider";
        }
    }
}
