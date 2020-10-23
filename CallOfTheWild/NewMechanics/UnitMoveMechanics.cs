using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CallOfTheWild.UnitMoveMechanics
{
    public interface IUnitMovedHandler : IGlobalSubscriber
    {
        void onUnitMoved(UnitEntityData unit);
    }

    [Harmony12.HarmonyPatch(typeof(UnitEntityData))]
    [Harmony12.HarmonyPatch("Position", Harmony12.MethodType.Setter)]
    class Patch_UnitEntityData_Position_Postfix
    {
        static public void Postfix(UnitEntityData __instance)
        {
            EventBus.RaiseEvent<IUnitMovedHandler>((Action<IUnitMovedHandler>)(h => h.onUnitMoved(__instance)));
        }
    }

    class ActionOnUnitMoved : OwnedGameLogicComponent<UnitDescriptor>, IUnitMovedHandler, IGlobalSubscriber
    {
        public float min_distance_moved = 5.Feet().Meters;
        public ActionList actions;
        private float moved_distance = 0.0f;

        public void onUnitMoved(UnitEntityData unit)
        {
            if (unit?.Descriptor != this.Owner)
            {
                return;
            }

            moved_distance += (unit.Position - unit.PreviousPosition).magnitude;

            if (moved_distance >= min_distance_moved)
            {
                (this.Fact as IFactContextOwner).RunActionInContext(actions, this.Owner.Unit);
                while (moved_distance >= min_distance_moved)
                {
                    moved_distance -= min_distance_moved;
                }
            }
        }
    }
}
