using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Designers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Visual.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace CallOfTheWild.NewMechanics.CustomAbilities
{
    public class AbilityCustomDimensionDoorOnTargetWithBuffFromCaster : AbilityCustomDimensionDoor
    {
        public BlueprintBuff buff;

        public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
        {
            UnitEntityData caster = context.Caster;
            List<UnitEntityData> targets = this.GetTargets(caster);
            Vector3 tp = target.Point;

            Vector3[] points = new Vector3[targets.Count];
            float[] radiuses = new float[targets.Count];
            for (int i = 0; i < targets.Count; i++)
            {
                points[i] = tp;
                radiuses[i] = targets[i].View.Corpulence;
            }
            FreePlaceSelector.RelaxPoints(points, radiuses, targets.Count, null);
            Vector3 targetPoint = points[0];
            GameObject portalFrom = FxHelper.SpawnFxOnUnit(this.PortalFromPrefab, caster.View, null, default(Vector3));
            GameObject portalTo = FxHelper.SpawnFxOnUnit(this.PortalToPrefab, caster.View, null, default(Vector3));
            if (portalTo != null)
            {
                portalTo.transform.position = targetPoint;
            }
            Transform portalFromBone = (!(portalFrom != null)) ? null : portalFrom.transform.FindChildRecursive(this.PortalBone);
            Vector3 portalFromPoint = (!(portalFromBone != null)) ? caster.Position : portalFromBone.transform.position;
            Transform portalToBone = (!(portalTo != null)) ? null : portalTo.transform.FindChildRecursive(this.PortalBone);
            Vector3 portalToPoint = (!(portalToBone != null)) ? targetPoint : portalToBone.transform.position;
            TimeSpan startTime = Game.Instance.TimeController.GameTime;
            bool casterTeleported = false;
            for (int j = 0; j < targets.Count; j++)
            {
                UnitEntityData t = targets[j];
                t.Wake(10f);
                Vector3 teleportPosition = points[j];
                GameObject prefab = (j != 0) ? this.SideDisappearFx : this.CasterDisappearFx;
                GameObject appearFx = (j != 0) ? this.SideAppearFx : this.CasterAppearFx;
                BlueprintProjectile blueprint = (j != 0) ? this.SideDisappearProjectile : this.CasterDisappearProjectile;
                BlueprintProjectile appearProjectile = (j != 0) ? this.SideAppearProjectile : this.CasterAppearProjectile;
                FxHelper.SpawnFxOnUnit(prefab, t.View, null, default(Vector3));
                GameHelper.LaunchProjectile(t, portalFromPoint, blueprint, delegate (Projectile dp)
                {
                    t.CombatState.PreventAttacksOfOpporunityNextFrame = true;
                    t.Position = teleportPosition;
                    t.View.StopMoving();
                    Game.Instance.ProjectileController.Launch(t, t, appearProjectile, portalToPoint, delegate (Projectile ap)
                    {
                        if (this.LookAtTarget)
                        {
                            t.ForceLookAt(target.Point);
                        }
                        FxHelper.SpawnFxOnUnit(appearFx, t.View, null, default(Vector3));
                        casterTeleported |= (caster == t);
                    });
                });
            }
            while (!casterTeleported && Game.Instance.TimeController.GameTime - startTime < 2.Seconds())
            {
                yield return null;
            }
            if (casterTeleported)
            {
                yield return new AbilityDeliveryTarget(target);
            }
            yield break;
        }

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


    public class AbilityCustomMoveCompanionToTarget : AbilityCustomDimensionDoor
    {
        public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
        {
            UnitEntityData caster = context.Caster;
            List<UnitEntityData> targets = this.GetTargets(caster);
            Vector3 tp = target.Point;
            if (target.Unit != null)
            {
                float d = caster.View.Corpulence + target.Unit.View.Corpulence + GameConsts.MinWeaponRange.Meters;
                tp += Quaternion.Euler(0f, target.Unit.Orientation, 0f) * Vector3.forward * d;
            }
            Vector3[] points = new Vector3[targets.Count];
            float[] radiuses = new float[targets.Count];
            for (int i = 0; i < targets.Count; i++)
            {
                points[i] = tp;
                radiuses[i] = targets[i].View.Corpulence;
            }
            FreePlaceSelector.RelaxPoints(points, radiuses, targets.Count, null);
            Vector3 targetPoint = points[0];
            GameObject portalFrom = FxHelper.SpawnFxOnUnit(this.PortalFromPrefab, caster.View, null, default(Vector3));
            GameObject portalTo = FxHelper.SpawnFxOnUnit(this.PortalToPrefab, caster.View, null, default(Vector3));
            if (portalTo != null)
            {
                portalTo.transform.position = targetPoint;
            }
            Transform portalFromBone = (!(portalFrom != null)) ? null : portalFrom.transform.FindChildRecursive(this.PortalBone);
            Vector3 portalFromPoint = (!(portalFromBone != null)) ? caster.Position : portalFromBone.transform.position;
            Transform portalToBone = (!(portalTo != null)) ? null : portalTo.transform.FindChildRecursive(this.PortalBone);
            Vector3 portalToPoint = (!(portalToBone != null)) ? targetPoint : portalToBone.transform.position;
            TimeSpan startTime = Game.Instance.TimeController.GameTime;
            bool casterTeleported = false;
            for (int j = 0; j < targets.Count; j++)
            {
                UnitEntityData t = targets[j];
                t.Wake(10f);
                Vector3 teleportPosition = points[j];
                GameObject prefab = (j != 0) ? this.SideDisappearFx : this.CasterDisappearFx;
                GameObject appearFx = (j != 0) ? this.SideAppearFx : this.CasterAppearFx;
                BlueprintProjectile blueprint = (j != 0) ? this.SideDisappearProjectile : this.CasterDisappearProjectile;
                BlueprintProjectile appearProjectile = (j != 0) ? this.SideAppearProjectile : this.CasterAppearProjectile;
                FxHelper.SpawnFxOnUnit(prefab, t.View, null, default(Vector3));
                GameHelper.LaunchProjectile(t, portalFromPoint, blueprint, delegate (Projectile dp)
                {
                    t.CombatState.PreventAttacksOfOpporunityNextFrame = true;
                    t.Position = teleportPosition;
                    t.View.StopMoving();
                    Game.Instance.ProjectileController.Launch(t, t, appearProjectile, portalToPoint, delegate (Projectile ap)
                    {
                        if (this.LookAtTarget)
                        {
                            t.ForceLookAt(target.Point);
                        }
                        FxHelper.SpawnFxOnUnit(appearFx, t.View, null, default(Vector3));
                        casterTeleported |= (caster == t);
                    });
                });
            }
            while (!casterTeleported && Game.Instance.TimeController.GameTime - startTime < 2.Seconds())
            {
                yield return null;
            }
            if (casterTeleported)
            {
                yield return new AbilityDeliveryTarget(target);
            }
            yield break;
        }

        protected override List<UnitEntityData> GetTargets(UnitEntityData caster)
        {
            List<UnitEntityData> units_to_move = new List<UnitEntityData>();

            var pet = caster?.Descriptor?.Pet;

            if (pet != null)
            {
                units_to_move.Add(pet);
            }
            return units_to_move;
        }
    }
}
