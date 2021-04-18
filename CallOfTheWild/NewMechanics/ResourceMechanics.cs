using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.ResourceMechanics
{

    public class UnitPartConnectResource: AdditiveUnitPart
    {
        public BlueprintAbilityResource[] getConnectedResource(BlueprintAbilityResource base_resource)
        {
            foreach (var b in buffs)
            {
                var r = b.Blueprint.GetComponent<ConnectResource>();
                if (r == null || r.base_resource != base_resource)
                {
                    continue;
                }
                return r.connected_resources;
            }

            return null;
        }
    }


    public class ConnectResource: OwnedGameLogicComponent<UnitDescriptor>
    {
        public BlueprintAbilityResource base_resource;
        public BlueprintAbilityResource[] connected_resources;

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartConnectResource>().addBuff(this.Fact);
        }

        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartConnectResource>().removeBuff(this.Fact);
        }
    }


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
                unit.Descriptor.Resources.Restore(Resource);
            }

        }
    }


    public class RestoreResourceAmountEqualToRemainingGroupSize : ContextAction
    {
        public ActivatableAbilityGroup group;
        public BlueprintAbilityResource resource;

        public override string GetCaption()
        {
            return "Restore resource equal to remaining group size";
        }

        public override void RunAction()
        {
            var unit = this.Target?.Unit;
            if (unit == null)
            {
                UberDebug.LogError("Target is missing");
                return;
            }

            int amount = getRemainingGroupSize(unit.Descriptor);
            unit.Descriptor.Resources.Restore(resource, amount);
        }

        private int getRemainingGroupSize(UnitDescriptor unit)
        {
            int remaining_group_size = unit.Ensure<UnitPartActivatableAbility>().GetGroupSize(this.group);

            foreach (var a in unit.ActivatableAbilities)
            {
                if (a.Blueprint.Group == group && a.IsOn)
                {
                    remaining_group_size -= a.Blueprint.WeightInGroup;
                }
            }
            return remaining_group_size;
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


    public class ContextActionSpendResourceFromCaster : ContextAction
    {
        public BlueprintAbilityResource resource;
        public int amount = 1;
        public BlueprintUnitFact[] cost_reducing_facts = new BlueprintUnitFact[0];


        public override void RunAction()
        {
            int need_resource = amount;

            var caster = this.Context.MaybeCaster?.Descriptor;

            foreach (var f in cost_reducing_facts)
            {
                if (caster.HasFact(f))
                {
                    need_resource--;
                }
            }

            if (need_resource < 0)
            {
                need_resource = 0;
            }

            if (this.resource == null || caster.Resources.GetResourceAmount(this.resource) < need_resource)
            {
                return;
            }

            caster.Resources.Spend(this.resource, need_resource);
        }


        public override string GetCaption()
        {
            return $"Spend {resource.name} ({amount})";
        }
    }


    public class RestrictionHasEnoughResource : ActivatableAbilityRestriction
    {
        public BlueprintAbilityResource resource;
        public int amount = 1;
        

        public override bool IsAvailable()
        {           
            return this.Owner.Resources.GetResourceAmount(resource) >= amount;
        }
    }


    public class ContextConditionTargetHasEnoughResource : ContextCondition
    {
        public BlueprintAbilityResource resource;
        public int amount = 1;
        public bool on_caster = false;

        protected override string GetConditionCaption()
        {
            return string.Empty;
        }

        protected override bool CheckCondition()
        {
            var unit = on_caster ? this.Context.MaybeCaster : this.Target?.Unit;
            if (unit == null)
            {
                return false;
            }
            return unit.Descriptor.Resources.HasEnoughResource(resource, amount);
        }
    }


    [AllowMultipleComponents]
    public class ContextIncreaseResourceAmount : OwnedGameLogicComponent<UnitDescriptor>, IResourceAmountBonusHandler, IUnitSubscriber
    {
        public ContextValue Value;
        public BlueprintAbilityResource Resource;

        public void CalculateMaxResourceAmount(BlueprintAbilityResource resource, ref int bonus)
        {
            if (!this.Fact.Active || (resource != this.Resource))
                return;
            bonus += this.Value.Calculate(this.Fact.MaybeContext);
        }
    }


    public class FakeResourceAmountFullRestore : BlueprintComponent
    {
        public BlueprintAbilityResource fake_resource;
    }

    public class MinResourceAmount : BlueprintComponent
    {
        public int value;
    }


    public class ResourceCostFromBuffRank: BlueprintComponent, IAbilityResourceCostCalculator
    {
        public int base_value = 1;
        public BlueprintBuff buff;

        public int Calculate(AbilityData ability)
        {
            var fact = ability.Caster.Buffs.GetFact(buff);
            if (fact == null)
            {
                return base_value;
            }

            return base_value + fact.GetRank();
        }
    }


    [Harmony12.HarmonyPatch(typeof(UnitAbilityResourceCollection))]
    [Harmony12.HarmonyPatch("Restore", Harmony12.MethodType.Normal)]
    [Harmony12.HarmonyPatch(new Type[] { typeof(BlueprintScriptableObject), typeof(int), typeof(bool) })]
    class UnitAbilityResourceCollection__Restore__Patch
    {
        static bool Prefix(UnitAbilityResourceCollection __instance, BlueprintScriptableObject blueprint, int amount, bool restoreFull, UnitDescriptor ___m_Owner)
        {
            Main.TraceLog();
            UnitAbilityResource resource = Harmony12.Traverse.Create(__instance).Method("GetResource", blueprint).GetValue<UnitAbilityResource>();
            if (resource == null)
            {
                return true;
            }

            var fake_resource = resource.Blueprint?.GetComponent<FakeResourceAmountFullRestore>()?.fake_resource;
            if (fake_resource == null || !restoreFull)
            {
                return true;
            }
            else
            {
                int maxAmount = fake_resource.GetMaxAmount(___m_Owner);
                resource.Amount = maxAmount;
                return false;
            }
        }
    }


    [Harmony12.HarmonyPatch(typeof(BlueprintAbilityResource))]
    [Harmony12.HarmonyPatch("GetMaxAmount", Harmony12.MethodType.Normal)]
    class BlueprintAbilityResource__GetMaxAmount__Patch
    {
        static void Postfix(BlueprintAbilityResource __instance, UnitDescriptor unit, ref int __result)
        {
            Main.TraceLog();
            var min_resource_component = __instance.GetComponent<MinResourceAmount>();

            if (min_resource_component == null)
            {
                return;
            }

            if (__result < min_resource_component.value)
            {
                __result = min_resource_component.value;
            }
        }
    }


    [Harmony12.HarmonyPatch(typeof(UnitAbilityResourceCollection))]
    [Harmony12.HarmonyPatch("Spend", Harmony12.MethodType.Normal)]
    class UnitAbilityResourceCollection__Spend__Patch
    {
        static void Postfix(UnitAbilityResourceCollection __instance, BlueprintScriptableObject blueprint, int amount)
        {
            var owner = Helpers.GetField<UnitDescriptor>(__instance, "m_Owner");
            var connected_resources = owner?.Get<UnitPartConnectResource>()?.getConnectedResource(blueprint as BlueprintAbilityResource);
            if (connected_resources == null || connected_resources.Length == 0)
            {
                return;
            }

            connected_resources = connected_resources.OrderByDescending(c => __instance.GetResourceAmount(c)).ToArray();
            var max_resource_id = 0;            
            while (amount > 0)
            {
                var next_resource_id = (max_resource_id + 1) % connected_resources.Length;
                if (__instance.GetResourceAmount(connected_resources[next_resource_id]) > __instance.GetResourceAmount(connected_resources[max_resource_id]))
                {
                    max_resource_id = next_resource_id;
                }
                __instance.Spend(connected_resources[max_resource_id], 1);
                amount--;
            }
        }
    }


    [Harmony12.HarmonyPatch(typeof(UnitAbilityResourceCollection))]
    [Harmony12.HarmonyPatch("HasEnoughResource", Harmony12.MethodType.Normal)]
    class UnitAbilityResourceCollection__HasEnoughResource__Patch
    {
        static bool Prefix(UnitAbilityResourceCollection __instance, BlueprintScriptableObject blueprint, int amount, ref bool __result)
        {
            __result = __instance.GetResourceAmount(blueprint) >= amount;
            return false;
        }
    }


    [Harmony12.HarmonyPatch(typeof(UnitAbilityResourceCollection))]
    [Harmony12.HarmonyPatch("GetResourceAmount", Harmony12.MethodType.Normal)]
    class UnitAbilityResourceCollection__GetResourceAmount__Patch
    {
        static bool Prefix(UnitAbilityResourceCollection __instance, BlueprintScriptableObject blueprint, ref int __result)
        {
            var owner = Helpers.GetField<UnitDescriptor>(__instance, "m_Owner");
            var connected_resources = owner?.Get<UnitPartConnectResource>()?.getConnectedResource(blueprint as BlueprintAbilityResource);
            if (connected_resources == null)
            {
                return true;
            }

            int val = 0;
            UnitAbilityResource resource = Harmony12.Traverse.Create(__instance).Method("GetResource", blueprint).GetValue<UnitAbilityResource>();
            if (resource != null)
                val = resource.Amount;

            __result = 0;
            foreach (var cr in connected_resources)
            {
                __result += __instance.GetResourceAmount(cr);
            }

            __result = Math.Min(__result, val);
            return false;
        }
    }
}
