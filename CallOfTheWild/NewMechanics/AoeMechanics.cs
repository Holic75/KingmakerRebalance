﻿using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Clicks;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.AbilityTarget;
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
            Main.TraceLog();
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


    [HarmonyPatch(typeof(AbilityAoERange), "CanEnable")]
    static class AbilityAoERange_CanEnable_Patch
    {
        static void Postfix(AbilityAoERange __instance, ref bool __result, Ability ___Ability)
        {
            Main.TraceLog();
            //Disable AbilityAoERange for wall spells
            if (___Ability != null && ___Ability.Blueprint.GetComponent<AbilityRectangularAoeVisualizer>() != null)
            {
                __result = false;
            }
        }
    }
    public class AbilityWallRange : Kingmaker.UI.AbilityTarget.AbilityRange
    {
        bool Initalized;
        public void Awake()
        {
            Range = new GameObject("DummyWallRange");
            Range.transform.SetParent(transform);
        }
        protected override bool CanEnable()
        {
            return base.CanEnable() && Ability.Blueprint.GetComponent<AbilityRectangularAoeVisualizer>() != null;
        }
        public void InitWall()
        {
            if (!Initalized)
            {
                var abilityTargetSelect = Game.Instance.IsControllerMouse ?
                    Game.Instance.UI.Common?.transform.Find("CombatCursorPC")?.gameObject :
                    Game.Instance.UI.Common?.transform.Find("Console_TargetSelect")?.gameObject;
                var abilityLineRange = abilityTargetSelect?.GetComponent<AbilityLineRange>()?.Range;
                if (abilityLineRange == null) throw new System.Exception("Could not find AbilityLineRange");
                var lineRangeVisual = abilityLineRange.transform.Find("Pivot/Line")?.gameObject;
                if (abilityLineRange == null) throw new System.Exception("Could not find LineRangeVisual");
                Range = GameObject.Instantiate(lineRangeVisual);
                Range.name = "WallRange";
                GameObject.DontDestroyOnLoad(Range);
                Initalized = true;
            }
        }
        protected override void SetFirstSpecs()
        {
            try
            {
                InitWall();
                Transform transform = Range.transform;
                var wallSize = Ability.Blueprint.GetComponent<AbilityRectangularAoeVisualizer>();
                transform.localScale = new Vector3(wallSize.width.Meters, transform.localScale.y, wallSize.length.Meters);
            }
            catch (Exception ex)
            {
                Main.logger.Error(ex.ToString());
            }
        }
        protected override void SetRangeToWorldPosition()
        {
            try
            {
                InitWall();
                PointerController clickEventsController = Game.Instance.ClickEventsController;
                TargetWrapper target = Game.Instance.SelectedAbilityHandler.GetTarget(clickEventsController.PointerOn, clickEventsController.WorldPosition, Ability);
                Vector3 mousePosition = (!(target != null)) ? clickEventsController.WorldPosition : target.Point;
                Vector3 casterPosition = Ability.Caster.Unit.View.transform.position;
                Vector3 normalized = (mousePosition - casterPosition).normalized;
                Vector2 vector = normalized.To2D();
                Vector3 eulerAngles = Range.transform.eulerAngles;
                eulerAngles.y = Mathf.Atan2(vector.x, vector.y) * 57.29578f + 90f;
                Range.transform.eulerAngles = eulerAngles;
                Range.transform.position = mousePosition;
                EventBus.RaiseEvent<IShowAoEAffectedUIHandler>(h =>
                {
                    h.HandleAoEMove(mousePosition, Ability);
                });
            }
            catch (Exception ex)
            {
                Main.logger.Error(ex.ToString());
            }
        }

        public static void load()
        {
            var wallRangeContainer = new GameObject("WallRangeHolder");
            GameObject.DontDestroyOnLoad(wallRangeContainer);
            wallRangeContainer.AddComponent<AbilityWallRange>();
        }
    }



}
