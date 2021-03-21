using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Validation;
using Kingmaker.ElementsSystem;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.KineticistMechanics
{
    public class UnitPartEnergizeWeapon : UnitPart
    {
        private Fact fact = null;

        public bool active()
        {
            return fact != null;
        }

        public BlueprintBuff getActivationBuff()
        {
            return fact?.Blueprint.GetComponent<KineticistEnergizeWeapon>()?.activation_buff;
        }

        public BlueprintAbility getAttackAbility() 
        {
            return fact?.Blueprint.GetComponent<KineticistEnergizeWeapon>()?.blast_ability;
        }


        public bool worksOn(ItemEntityWeapon weapon)
        {
            bool res = false;
            fact?.CallComponents<KineticistEnergizeWeapon>(c => res = c.worksOn(weapon));

            return res;
        }


        public AbilityData getAttackAbilityData()
        {
            var blueprint = getAttackAbility();
            if (blueprint == null)
            {
                return null;
            }

            Ability ability = this.Owner.GetFact(blueprint) as Ability ?? this.Owner.GetFact(blueprint.Parent) as Ability;
            if (ability == null)
            {
                return null;
            }
            var data = ability.Data;
            if (data.Blueprint != blueprint)
            {
                data = new AbilityData(blueprint, this.Owner)
                {
                    ConvertedFrom = data
                };
            }
            return data;
        }

        public void deactivate()
        {
            this.Owner.Buffs.RemoveFact(getActivationBuff());            
        }

        public bool isActivated()
        {
            return this.Owner.Buffs.HasFact(getActivationBuff());
        }

        public BlueprintAbility getActivationAbility()
        {
            return fact?.Blueprint.GetComponent<KineticistEnergizeWeapon>()?.activation_ability;
        }


        public bool isActivatingNow()
        {
            var ability_to_activate = getAttackAbility();
            foreach (UnitCommand command in this.Owner.Unit.Commands.Raw)
            {
                if (command == null || !command.IsRunning)
                {
                    continue;
                }

                if ((command as UnitUseAbility)?.Spell.Blueprint == ability_to_activate)
                {
                    return true;
                }
            }

            return false;
        }


        public bool tryActivate()
        {
            if (!active() || isActivated())
            {
                return false;
            }

            this.Owner.Buffs.AddBuff(getActivationBuff(), this.Owner.Unit, new TimeSpan?(2.Rounds().Seconds), AbilityParams.Empty);
            return true;
        }

        public void clear()
        {
            deactivate();
            fact = null;
        }

        public void set(Fact new_fact)
        {
            clear();
            fact = new_fact;
        }
    }

    [AllowedOn(typeof(BlueprintUnitFact))]
    public class DecreaseWildTalentCostWithActionOnBurn : OwnedGameLogicComponent<UnitDescriptor>, IKineticistCalculateAbilityCostHandler, IKinetecistAcceptBurnHandler, IGlobalSubscriber, IUnitSubscriber
    {
        public int value = 1;
        public ActionList actions;

        private MechanicsContext Context
        {
            get
            {
                return this.Fact.MaybeContext;
            }
        }

        public void HandleKineticistCalculateAbilityCost(UnitDescriptor caster, BlueprintAbility abilityBlueprint, ref KineticistAbilityBurnCost cost)
        {
            if (caster != this.Owner)
                return;

            var burn_cost = abilityBlueprint.GetComponent<AbilityKineticist>();
            if (burn_cost != null && burn_cost.WildTalentBurnCost > 0)
            {
                //to make it work for wild talents
                cost.Decrease(1, KineticistBurnType.WildTalent);
            }
            else
            {
                cost.IncreaseGatherPower(value);
            }
        }

        public void HandleKineticistAcceptBurn(UnitPartKineticist kinetecist, int burn, AbilityData ability)
        {
            if (actions != null)
            {
                (this.Fact as IFactContextOwner)?.RunActionInContext(this.actions, kinetecist.Owner.Unit);
            }
        }
    }

    public class NoElementalOverflow: BlueprintComponent
    {

    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class DecreaseWildTalentCostForSpecificTalents : OwnedGameLogicComponent<UnitDescriptor>, IKineticistCalculateAbilityCostHandler, IKinetecistAcceptBurnHandler, IGlobalSubscriber, IUnitSubscriber
    {
        public int value = 1;
        public BlueprintAbility[] abilities;
        public ActionList actions;

        private MechanicsContext Context
        {
            get
            {
                return this.Fact.MaybeContext;
            }
        }

        public void HandleKineticistCalculateAbilityCost(UnitDescriptor caster, BlueprintAbility abilityBlueprint, ref KineticistAbilityBurnCost cost)
        {
            if (caster != this.Owner)
                return;

            var burn_cost = abilityBlueprint.GetComponent<AbilityKineticist>();
            if (burn_cost != null && burn_cost.WildTalentBurnCost > 0 && abilities.Contains(abilityBlueprint))
            {
                cost.Decrease(1, KineticistBurnType.WildTalent);
            }
        }

        public void HandleKineticistAcceptBurn(UnitPartKineticist kinetecist, int burn, AbilityData ability)
        {

            if (actions != null && abilities.Contains(ability.Blueprint))
            {
                (this.Fact as IFactContextOwner)?.RunActionInContext(this.actions, kinetecist.Owner.Unit);
            }
        }
    }

    //component used to indicate that cost should be calculated in specific way
    public class KineticistEnergizeWeapon : BuffLogic
    {
        public BlueprintAbility activation_ability;
        public BlueprintAbility blast_ability;
        public BlueprintBuff activation_buff;

        public bool kinetic_fist = false;

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartEnergizeWeapon>().set(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartEnergizeWeapon>().clear();
        }


        public bool worksOn(ItemEntityWeapon weapon)
        {
            if (weapon == null)
            {
                return false;
            }

            if (kinetic_fist)
            {
                return weapon.Blueprint.IsNatural || weapon.Blueprint.IsUnarmed;
            }
            else
            {
                return weapon == this.Owner.Body?.PrimaryHand?.MaybeWeapon;
            }
        }
    }


    [Harmony12.HarmonyPatch(typeof(MechanicActionBarSlotActivableAbility))]
    [Harmony12.HarmonyPatch("GetResource", Harmony12.MethodType.Normal)]
    class MechanicActionBarSlotActivableAbility__GetResource__Patch
    {
        static bool Prefix(MechanicActionBarSlotActivableAbility __instance, ref int __result)
        {
            var component1 = __instance.ActivatableAbility.Blueprint.Buff.GetComponent<KineticistEnergizeWeapon>();
            if (component1 == null)
            {
                return true;
            }
            __result = component1.activation_ability.GetComponent<AbilityKineticist>().CalculateBurnCost(__instance.ActivatableAbility.Owner, component1.activation_ability).Total;
            return false;
        }
    }


    [Harmony12.HarmonyPatch(typeof(MechanicActionBarSlotActivableAbility))]
    [Harmony12.HarmonyPatch("IsDisabled", Harmony12.MethodType.Normal)]
    class MechanicActionBarSlotActivableAbility__IsDisabled__Patch
    {
        static bool Prefix(MechanicActionBarSlotActivableAbility __instance, int resourceCount, ref bool __result)
        {
            var component1 = __instance.ActivatableAbility.Blueprint.Buff.GetComponent<KineticistEnergizeWeapon>();
            if (component1 != null)
            {
                __result = !__instance.ActivatableAbility.Owner.State.IsConscious;
                return false;
            }
            return true;
        }
    }
}



