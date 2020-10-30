using Kingmaker;
using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CallOfTheWild.DeadTargetMechanics
{
    [AllowedOn(typeof(BlueprintAbility))]
    public class AbilityTargetCanBeAnimated : BlueprintComponent, IAbilityTargetChecker
    {
        public Size max_size = Size.Colossal;
        public bool CanTarget(UnitEntityData caster, TargetWrapper t)
        {
            UnitEntityData unit = t.Unit;
            if (unit == null)
            {
                return false;
            }

            if (unit.Descriptor.OriginalSize > max_size)
            {
                return false;
            }

            if (unit != null && !unit.IsPlayerFaction && (unit.Descriptor.State.IsDead || unit.Descriptor.State.HasCondition(UnitCondition.DeathDoor)) && !unit.Descriptor.State.HasCondition(UnitCondition.Petrified))
            {
                bool can_raise = !unit.Descriptor.HasFact(Common.aberration)
                                 && !unit.Descriptor.HasFact(Common.undead)
                                 && !unit.Descriptor.HasFact(Common.vermin)
                                 && !unit.Descriptor.HasFact(Common.elemental)
                                 && !unit.Descriptor.HasFact(Common.construct)
                                 && !unit.Descriptor.HasFact(Common.plant);
                return can_raise && !unit.Descriptor.HasFact(Common.no_animate_feature);
            }

            return false;
        }
    }


    [AllowedOn(typeof(BlueprintAbility))]
    public class ContextConditionCanBeAnimated : ContextCondition
    {
        public Size max_size = Size.Colossal;

        protected override string GetConditionCaption()
        {
            return string.Empty;
        }

        protected override bool CheckCondition()
        {
            UnitEntityData unit = this.Target.Unit;
            if (unit == null)
            {
                return false;
            }

            if (unit.Descriptor.OriginalSize > max_size)
            {
                return false;
            }

            if (unit != null && !unit.IsPlayerFaction && (unit.Descriptor.State.IsDead || unit.Descriptor.State.HasCondition(UnitCondition.DeathDoor)) && !unit.Descriptor.State.HasCondition(UnitCondition.Petrified))
            {
                bool can_raise = !unit.Descriptor.HasFact(Common.aberration)
                                 && !unit.Descriptor.HasFact(Common.undead)
                                 && !unit.Descriptor.HasFact(Common.vermin)
                                 && !unit.Descriptor.HasFact(Common.elemental)
                                 && !unit.Descriptor.HasFact(Common.construct)
                                 && !unit.Descriptor.HasFact(Common.plant);
                return can_raise && !unit.Descriptor.HasFact(Common.no_animate_feature);
            }

            return false;
        }
    }



    public class ContextActionAnimateDead : ContextAction
    {
        public BlueprintUnit Blueprint;
        public BlueprintSummonPool SummonPool;
        public ContextDurationValue DurationValue;
        public bool DoNotLinkToCaster;
        public bool adapt_size = true;
        public bool do_not_link_to_caster = false;
        public ActionList AfterSpawn;
        public int str_bonus = 0;
        public int dex_bonus = 0;
        public bool transfer_equipment = false;
        public int hd_cl_multiplier = 2;
        public int max_hd_cl_multiplier = 4;

        static BlueprintCharacterClass[] racial_classes = new BlueprintCharacterClass[]
        {
            Main.library.Get<BlueprintCharacterClass>("4cd1757a0eea7694ba5c933729a53920"), //animal
            Main.library.Get<BlueprintCharacterClass>("6ab4526f94d2e3e439af0599a29b6675"), //humanoid
            Main.library.Get<BlueprintCharacterClass>("f2e6e760ead99fb48ade27c7e9d4ac94"), //fey
            Main.library.Get<BlueprintCharacterClass>("b9e97f47cb86f2d45a0784a096ff8037"), //magical beast
            Main.library.Get<BlueprintCharacterClass>("8a3c86893f383214da070e9c84c1e95b"), //monstrous humanoid
            Main.library.Get<BlueprintCharacterClass>("9a20b40b57f4e684fa20d17c0edfd5ba"), //nymph
            Main.library.Get<BlueprintCharacterClass>("92ab5f2fe00631b44810deffcc1a97fd"), //outsider
            Main.library.Get<BlueprintCharacterClass>("01a754e7c1b7c5946ba895a5ff0faffc"), //dragon
        };

        public override string GetCaption()
        {
            return string.Format("Animate {0} x {1} for {2}", (object)this.Blueprint.name, 1, (object)this.DurationValue);
        }

        public override void RunAction()
        {
            UnitEntityData caster = this.Context.MaybeCaster;
            if (caster == null)
            {
                UberDebug.LogError((UnityEngine.Object)this, (object)"Caster is missing", (object[])Array.Empty<object>());
            }
            var unit = this.Target.Unit;
            if (unit == null || !unit.Descriptor.State.IsDead || unit.Descriptor.State.HasCondition(UnitCondition.Petrified))
            {
                return;
            }

            Rounds duration = this.DurationValue.Calculate(this.Context);
        
            int level = Math.Max(Math.Min(getRacialHD(this.Target.Unit.Descriptor), 20), Blueprint.GetComponent<AddClassLevels>().Levels);
            int max_lvl = this.Context.Params.CasterLevel * hd_cl_multiplier * (this.Context.MaybeCaster.Descriptor.HasFact(ChannelEnergyEngine.desecrate_buff) ? 2 : 1);
            if (level > max_lvl)
            {
                Common.AddBattleLogMessage($"{unit.CharacterName} corpse HD ({level}) is beyond your {caster.CharacterName}'s to animate ({max_lvl}).");
                return;
            }

            //Main.logger.Log("Animate Dead: Remaining HD limit: " + getUsedHD(this.Context, SummonPool).ToString() + "/" + (this.Context.Params.CasterLevel * max_hd_cl_multiplier).ToString());
            int max_total_hd = this.Context.Params.CasterLevel * max_hd_cl_multiplier;
            int used_hd = getUsedHD(this.Context, SummonPool);
            int remaining_hd = max_total_hd - used_hd;
            if ( level > remaining_hd)
            {
                Common.AddBattleLogMessage($"{unit.CharacterName} corpse HD ({level}) does not fit into {caster.CharacterName}'s animate dead HD limit ({remaining_hd}/{max_total_hd})");
                return;
            }
            Vector3 clampedPosition = ObstacleAnalyzer.GetNearestNode(this.Target.Point).clampedPosition;
            UnitEntityView unitEntityView = this.Blueprint.Prefab.Load(false);

            var target_size = unit.Descriptor.OriginalSize;

            float radius = !((UnityEngine.Object)unitEntityView != (UnityEngine.Object)null) ? 0.5f : unitEntityView.Corpulence;
            FreePlaceSelector.PlaceSpawnPlaces(1, radius, clampedPosition);

            Vector3 relaxedPosition = FreePlaceSelector.GetRelaxedPosition(0, true);
            UnitEntityData animated_unit = this.Context.TriggerRule<RuleSummonUnit>(new RuleSummonUnit(caster, this.Blueprint, relaxedPosition, duration, 0)
            {
                Context = this.Context,
                DoNotLinkToCaster = this.do_not_link_to_caster
            }).SummonedUnit;
            if (this.SummonPool != null)
            {
                GameHelper.RegisterUnitInSummonPool(this.SummonPool, animated_unit);
            }

            var level_up_component = animated_unit.Blueprint.GetComponent<AddClassLevels>();

            int current_level = level_up_component.Levels;

            animated_unit.Stats.Strength.BaseValue = unit.Stats.Strength.BaseValue + str_bonus;
            animated_unit.Stats.Dexterity.BaseValue = unit.Stats.Dexterity.BaseValue + dex_bonus;

            if (adapt_size)
            {
                animated_unit.Descriptor.AddFact(Common.size_override_facts[target_size], null, null);
            }

            if (current_level < level)
            { 
                level_up_component.LevelUp(animated_unit.Descriptor, level - current_level);
            }

            if (transfer_equipment && unit.Body.HandsAreEnabled)
            {   
                List<ItemSlot> list1 = unit.Body.EquipmentSlots.ToList<ItemSlot>();
                List<ItemSlot> list2 = animated_unit.Body.EquipmentSlots.ToList<ItemSlot>();
                for (int index = 0; index < list1.Count && index < list2.Count; ++index)
                {
                    ItemEntity maybeItem = list1[index].MaybeItem;
                    if (maybeItem != null)
                        animated_unit.Body.TryInsertItem(maybeItem.Blueprint, list2[index]);
                }
                animated_unit.Body.CurrentHandEquipmentSetIndex = unit.Body.CurrentHandEquipmentSetIndex;
            }
            animated_unit.Descriptor.State.AddCondition(UnitCondition.Unlootable, (Kingmaker.UnitLogic.Buffs.Buff)null);


            using (this.Context.GetDataScope(animated_unit))
            {
                this.AfterSpawn.Run();
            }

            unit.Descriptor.AddFact(Common.no_animate_feature);

            animated_unit.Descriptor.CustomName = animated_unit.Descriptor.CharacterName + $" ({level} HD)";
            EventBus.RaiseEvent<IUnitNameHandler>((Action<IUnitNameHandler>)(h => h.OnUnitNameChanged(animated_unit)));
        }


        int getRacialHD(UnitDescriptor unit_descriptor)
        {
            var hd = 0;
            foreach (var rc in racial_classes)
            {
                hd += unit_descriptor.Progression.GetClassLevel(rc);
            }
            return hd;
        }

        int getUsedHD(MechanicsContext context, BlueprintSummonPool summon_pool)
        {           
            ISummonPool pool = Game.Instance.SummonPools.GetPool(this.SummonPool);
            if (pool == null)
            {
                return 0;
            }

            int used_hd = 0;
            foreach (UnitEntityData unit in pool.Units)
            {
                if (unit.Get<UnitPartSummonedMonster>().Summoner == context.MaybeCaster)
                {
                    used_hd += unit.Descriptor.Progression.CharacterLevel;
                }
            }

            return used_hd;
        }
    }

    [AllowedOn(typeof(BlueprintAbility))]
    public class AbilityTargetRecentlyDead : BlueprintComponent, IAbilityTargetChecker
    {
        public BlueprintBuff RecentlyDeadBuff;

        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            bool flag1 = target.Unit != null && ((target.Unit.Descriptor.State.IsDead || target.Unit.Descriptor.State.HasCondition(UnitCondition.DeathDoor)) && target.Unit.Descriptor.HasFact((BlueprintUnitFact)this.RecentlyDeadBuff)) && !target.Unit.Descriptor.State.HasCondition(UnitCondition.Petrified);
            return flag1;
        }
    }



    [Harmony12.HarmonyPatch(typeof(BlueprintAbility))]
    [Harmony12.HarmonyPatch("CanCastToDeadTarget", Harmony12.MethodType.Getter)]
    class BlueprintAbility__CanCastToDeadTarget__Patch
    {
        static void Postfix(BlueprintAbility __instance, ref bool __result)
        {
            Main.TraceLog();
            __result = __result || __instance.GetComponent<AbilityTargetRecentlyDead>() != null || __instance.GetComponent<AbilityTargetCanBeAnimated>();
        }
    }
}
