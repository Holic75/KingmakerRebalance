using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.ResourceMechanics
{
    public class ContextRestoreResource : ContextAction
    {
        public BlueprintAbilityResource Resource;
        public bool full = false;
        public ContextValue amount = 1;

        public override string GetCaption() => "Restore resourse";

        public override void RunAction()
        {
            var unit = this.Target?.Unit;
            if (unit == null)
            {
                UberDebug.LogError("Target is missing");
                return;
            }

            if (!full)
            {
                unit.Descriptor.Resources.Restore(Resource, amount.Calculate(this.Context));
            }
            else
            {
                unit.Descriptor.Resources.Restore(Resource, unit.Descriptor.Resources.GetResourceAmount(Resource));
            }

        }
    }


    [AllowedOn(typeof(BlueprintAbility))]
    public class ResourceIsFullChecker : BlueprintComponent, IAbilityCasterChecker
    {
        public BlueprintAbilityResource Resource;
        public bool Not;

        public static ResourceIsFullChecker Create(BlueprintAbilityResource resource, bool not)
        {
            var r = Helpers.Create<ResourceIsFullChecker>();
            r.Resource = resource;
            r.Not = not;
            return r;
        }

        public bool CorrectCaster(UnitEntityData caster)
        {
            var available = caster.Descriptor.Resources.GetResourceAmount(Resource);
            var max = Resource.GetMaxAmount(caster.Descriptor);
            return (available == max) != Not;
        }

        public string GetReason() => $"{Resource.Name} is not active on any targets.";
    }

}
