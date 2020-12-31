using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace CallOfTheWild
{
    public partial class ImplementsEngine
    {
        BlueprintFeature createEnergyRay()
        {
            var rays = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("435222be97067a447b2b40d3c58a058e"), //acid
                library.Get<BlueprintAbility>("7ef096fdc8394e149a9e8dced7576fee"), //cold
                library.Get<BlueprintAbility>("96ca3143601d6b242802655336620d91"), //electricity
                library.Get<BlueprintAbility>("cdb106d53c65bbc4086183d54c3b97c7") //fire
            };

            var names = new string[] { "Acid", "Cold", "Electricity", "Fire" };

            var abilities = new BlueprintAbility[rays.Length];

            for (int i = 0; i < abilities.Length; i++)
            {
                var dmg_type = (rays[i].GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionDealDamage).DamageType.Energy;
                var ability = Helpers.CreateAbility(prefix + names[i] + "EnergyRayAbility",
                                                    "Energy Ray",
                                                    "As a standard action that provokes attacks of opportunity, you can expend 1 point of mental focus to unleash a ray of pure energy as a ranged touch attack. This ray has a range of 25 feet. The ray deals an amount of energy damage equal to 1d6 points + 1d6 points for every 2 occultist levels you possess beyond 1st (2d6 at 3rd level, 3d6 at 5th, and so on, to a maximum of 10d6 at 19th level). When you unleash an energy ray, you must decide what type of damage it deals (acid, cold, electricity, or fire).",
                                                    "",
                                                    rays[i].Icon,
                                                    AbilityType.SpellLike,
                                                    CommandType.Standard,
                                                    AbilityRange.Close,
                                                    "",
                                                    "",
                                                    Helpers.CreateRunActions(Helpers.CreateActionDealDamage(dmg_type, Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.Default), 0))),
                                                    rays[i].GetComponent<AbilityDeliverProjectile>().CreateCopy(a => { a.Projectiles = new BlueprintProjectile[] { a.Projectiles[0] }; a.UseMaxProjectilesCount = false; }),
                                                    Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                    rays[i].GetComponent<SpellDescriptorComponent>(),
                                                    createClassScalingConfig(ContextRankProgression.OnePlusDivStep),
                                                    resource.CreateResourceLogic()
                                                    );
                ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);
                abilities[i] = ability;
            }

            var wrapper = Common.createVariantWrapper(prefix + "EnergyRayAbilityBase", "", abilities);

            return Common.AbilityToFeature(wrapper, false);
        }
    }
}
