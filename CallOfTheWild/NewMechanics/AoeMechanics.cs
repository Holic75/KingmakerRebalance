using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CallOfTheWild.AoeMechanics
{
    public class AbilityRectangularAoeVisualizer : BlueprintComponent
    { 
        public Feet length;
        public Feet width;
        public TargetType target_type;


        public bool wouldTarget(AbilityData ability, Vector3 target_pos, UnitEntityData unit)
        {
            float y_meters = width.Meters * 0.5f + unit.Corpulence;
            float x_meters = length.Meters * 0.5f + unit.Corpulence;
            UnitEntityData caster = ability.Caster.Unit;

            if (!caster.IsEnemy(unit) && target_type == TargetType.Enemy)
            {
                return false;
            }

            if (!caster.IsAlly(unit) && target_type == TargetType.Ally)
            {
                return false;
            }

            Vector2 y_normal = (target_pos - caster.Position).To2D().normalized;
            Vector3 start_position = caster.Position;

            Vector2 x_normal = new Vector2(-y_normal.y, y_normal.x);

            return Math.Abs(Vector2.Dot((unit.Position - target_pos).To2D(), y_normal)) < y_meters  && Math.Abs(Vector2.Dot((unit.Position - target_pos).To2D(), x_normal)) < x_meters;
        }
    }


    [Harmony12.HarmonyPatch(typeof(AbilityData))]
    [Harmony12.HarmonyPatch("WouldCurrentlyTarget", Harmony12.MethodType.Normal)]
    class AbilityData__WouldCurrentlyTarget__Patch
    {
        static void Postfix(AbilityData __instance, Vector3 targetPos, UnitEntityData unit, ref bool __result)
        {         
            if (!__result)
            {
                return;
            }

            var rectangle_visualzier = __instance.Blueprint.GetComponent<AbilityRectangularAoeVisualizer>();
            if (rectangle_visualzier != null)
            {
                __result = rectangle_visualzier.wouldTarget(__instance, targetPos, unit);
            }
        }
    }


    
}
