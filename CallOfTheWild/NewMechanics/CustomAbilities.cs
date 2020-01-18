using Kingmaker.Designers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.NewMechanics.CustomAbilities
{
    public class AbilityCustomDimensionDoorOnTargetWithBuffFromCaster : AbilityCustomDimensionDoor
    {
        public BlueprintBuff buff;

        protected override List<UnitEntityData> GetTargets(UnitEntityData caster)
        {
            List<UnitEntityData> units_to_move = new List<UnitEntityData>();

            
            if (this.Radius > 0.Feet())
            {
                var units = GameHelper.GetTargetsAround(caster.Position, this.Radius.Meters, false, false);
                foreach (var u in units)
                {
                    if (u.Buffs.Enumerable.Any(b => b.Blueprint == buff && b.MaybeContext.MaybeCaster == caster))
                    {
                        units_to_move.Add(u);
                    }
                }
            }
            return units_to_move;
        }
    }
}
