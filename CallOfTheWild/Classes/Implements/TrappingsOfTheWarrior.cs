using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    public partial class ImplementsEngine
    {
        public BlueprintFeature createCounterStrike()
        {
            var effect_buff = Helpers.CreateBuff(prefix + "CounterstrikeDamageBuff",
                                                 "",
                                                 "",
                                                 "",
                                                 null,
                                                 null,
                                                 Helpers.Create<NewMechanics.AttackOfOpportunityDamgeBonus>(a => { a.value = Helpers.CreateContextValue(AbilityRankType.Default); a.descriptor = ModifierDescriptor.UntypedStackable; }),
                                                 createClassScalingConfig(progression: ContextRankProgression.DivStep, stepLevel: 3, min: 3),
                                                 Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(Helpers.Create<ContextActionRemoveSelf>()), only_hit: false,
                                                                                                  on_initiator: true, wait_for_attack_to_resolve: true)
                                                 );
            var apply_effect = Common.createContextActionApplyBuff(effect_buff, Helpers.CreateContextDuration(1));

            var buff = Helpers.CreateBuff(prefix + "CounterstrikeBuff",
                                                 "Counterstrike",
                                                 "As an immediate action when you are damaged by a melee attack, you can expend 1 point of mental focus to immediately make a single attack at your highest base attack bonus against the creature that hit you, provided that you threaten that creature. If the attack hits, you gain a bonus on the damage roll equal to 1/3 your occultist level (rounded down, minimum +1). This attack counts as an attack of opportunity.",
                                                 "",
                                                 Helpers.GetIcon("36c8971e91f1745418cc3ffdfac17b74"), //blade barrier
                                                 null,
                                                 Helpers.Create<AooMechanics.ApplyActionToCasterAndMakeAttackOfOpportunityOnAttack>(a =>
                                                 {
                                                     a.consume_swift_action = true;
                                                     a.required_resource = resource;
                                                     a.resource_amount = 1;
                                                     a.actions = Helpers.CreateActionList(apply_effect);
                                                 }
                                                 )
                                                 );

            var toggle = Common.buffToToggle(buff, CommandType.Free, true, resource.CreateActivatableResourceLogic(ActivatableAbilityResourceLogic.ResourceSpendType.Never));

            addFocusInvestmentCheck(toggle, SpellSchool.Transmutation, SpellSchool.Abjuration);
            var feature = Common.ActivatableAbilityToFeature(toggle, false);

            return feature;
        }


        public BlueprintFeature createCombatTrick(BlueprintAbilityResource reduced_resource)
        {
            var feature = Helpers.CreateFeatureSelection(prefix + "CombatTrickFeature",
                                                        "Combat Trick",
                                                        "You receive one combat feat.",
                                                        "",
                                                        null,
                                                        FeatureGroup.None
                                                        );
            feature.AddComponent(Helpers.PrerequisiteNoFeature(feature));
            feature.AllFeatures = library.Get<BlueprintFeatureSelection>("41c8486641f7d6d4283ca9dae4147a9f").AllFeatures;

            return feature;
        }


        public BlueprintBuff createMartialSkill()
        {
            var property = ImplementMechanics.InvestedImplementFocusAmountProperty.createProperty(prefix + "TrappingOfTheWarriorFocusProperty", "",
                                                                                                  createClassScalingConfig(ContextRankProgression.BonusValue, stepLevel: 3),
                                                                                                  false,
                                                                                                  SpellSchool.Abjuration, SpellSchool.Transmutation);
            var buff = Helpers.CreateBuff(prefix + "TrappingOfTheWarriorFocusBuff",
                                          "Martial Skill",
                                          "When wielding the weapon used as the panoply’s associated implement, you treat your base attack bonus as though it were 1 point higher for every 4 points of total mental focus invested in all of the associated implements, to a maximum base attack bonus equal to your occultist level. This increase can grant you additional attacks when using the full attack action (for example, a 11th-level occultist with 11 points of mental focus invested among the associated implements would be treated as having a base attack bonus of +11, with iterative attacks at a base attack bonus of +6 and +1).",
                                          "",
                                          Helpers.GetIcon("9d5d2d3ffdd73c648af3eb3e585b1113"),
                                          null,
                                          Helpers.CreateAddContextStatBonus(StatType.BaseAttackBonus, ModifierDescriptor.UntypedStackable),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, ContextRankProgression.DivStep, stepLevel: 4,
                                                                          customProperty: property)
                                          );
            return buff;
        }
    }
}
