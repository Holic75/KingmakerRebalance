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
        public BlueprintFeature createGuardianAura()
        {
            var buff = Helpers.CreateBuff(prefix + "GuardianAuraBuff",
                                                 "Guardian Aura",
                                                 "As a full-round action, you can expend 3 points of mental focus to emit a 20-foot-radius aura of protection that lasts for 1 minute. You and your allies within the aura gain a sacred bonus to AC equal to 1/4 of your occultist level (minimum +1). Your speed is reduced by half while this power is active.",
                                                 "",
                                                 Helpers.GetIcon("183d5bb91dea3a1489a6db6c9cb64445"), //shield of faith
                                                 null,
                                                 Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.Sacred),
                                                 createClassScalingConfig(progression: ContextRankProgression.DivStep, stepLevel: 4, min: 1)
                                                 );
            var caster_buff = Common.createBuffAreaEffect(buff, 20.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsAlly>()));

            caster_buff.AddComponent(Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.Slowed));
            caster_buff.SetNameDescriptionIcon(buff);
            caster_buff.GetComponent<AddAreaEffect>().AreaEffect.Fx = Common.createPrefabLink("bbd6decdae32bce41ae8f06c6c5eb893");
            var apply_buff = Common.createContextActionApplySpellBuff(caster_buff, Helpers.CreateContextDuration(1, DurationRate.Minutes));
            caster_buff.SetBuffFlags(0);

            var ability = Helpers.CreateAbility(prefix + "GuardianAuraAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.SpellLike,
                                                CommandType.Standard,
                                                AbilityRange.Personal,
                                                Helpers.oneMinuteDuration,
                                                "",
                                                Helpers.CreateRunActions(apply_buff),
                                                resource.CreateResourceLogic(amount: 3)
                                                );
            ability.setMiscAbilityParametersSelfOnly();
            Common.setAsFullRoundAction(ability);

            addFocusInvestmentCheck(ability, SpellSchool.Conjuration, SpellSchool.Abjuration);
            var feature = Common.AbilityToFeature(ability, false);

            return feature;
        }


        public BlueprintFeature createRestoringTouch()
        {
            var action = Helpers.Create<ContextActionHealStatDamage>(c =>
            {
                c.HealDrain = false;
                c.Value = Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilityRankType.Default));
            }
            );
            var action_drain = action.CreateCopy(c => c.HealDrain = true);

            var ability = Helpers.CreateAbility(prefix + "RestoratingTouchAbility",
                                                "Restorating Touch",
                                                "As a standard action, you can expend 2 points of mental focus to cure a living creature you touch of temporary ability damage. You cure 1 point of temporary ability damage to a single ability score for every 3 occultist levels you have. At 10th level, you can expend an additional point of mental focus to instead restore the same amount of permanent ability drain.",
                                                "",
                                                Helpers.GetIcon("f2115ac1148256b4ba20788f7e966830"),
                                                AbilityType.SpellLike,
                                                CommandType.Standard,
                                                AbilityRange.Touch,
                                                "",
                                                "",
                                                Helpers.CreateRunActions(action),
                                                Common.createAbilitySpawnFx("621885df4b6add9489c8edd14b844ad6"),
                                                resource.CreateResourceLogic(amount: 2),
                                                createClassScalingConfig(ContextRankProgression.DivStep, stepLevel: 3, min: 1)
                                                );
            ability.setMiscAbilityParametersTouchFriendly();

            var ability_drain = library.CopyAndAdd(ability, prefix + "RestoratingTouchDrainAbility", "");
            ability_drain.SetName("Restorating Touch: (Heal Ability Drain)");
            ability_drain.ReplaceComponent<AbilityResourceLogic>(a => a.Amount = 3);
            ability_drain.ReplaceComponent<AbilityEffectRunAction>(a => { a.Actions = Helpers.CreateActionList(action_drain); });
            ability_drain.AddComponent(Helpers.Create<NewMechanics.AbilityShowIfHasClassLevels>(a => a.character_classes = classes));

            addFocusInvestmentCheck(ability, SpellSchool.Conjuration, SpellSchool.Abjuration);
            addFocusInvestmentCheck(ability_drain, SpellSchool.Conjuration, SpellSchool.Abjuration);
            var wrapper = Common.createVariantWrapper(prefix + "RestoratingTouchAbilityBase", "", ability, ability_drain);

            var feature = Common.AbilityToFeature(wrapper, false);

            return feature;
        }


        public BlueprintBuff createFontOfHealing()
        {
            var healing_spells = new BlueprintAbility[]
            {   //cure
                library.Get<BlueprintAbility>("47808d23c67033d4bbab86a1070fd62f"),
                library.Get<BlueprintAbility>("1c1ebf5370939a9418da93176cc44cd9"),
                library.Get<BlueprintAbility>("6e81a6679a0889a429dec9cedcf3729c"),
                library.Get<BlueprintAbility>("0d657aa811b310e4bbd8586e60156a2d"),
                library.Get<BlueprintAbility>("5d3d689392e4ff740a761ef346815074"),
                library.Get<BlueprintAbility>("571221cc141bc21449ae96b3944652aa"),
                library.Get<BlueprintAbility>("0cea35de4d553cc439ae80b3a8724397"),
                library.Get<BlueprintAbility>("1f173a16120359e41a20fc75bb53d449")
            };
            var property = ImplementMechanics.InvestedImplementFocusAmountProperty.createProperty(prefix + "SaintsHolyRegaliaFocusProperty", "",
                                                                                                  createClassScalingConfig(ContextRankProgression.AsIs),
                                                                                                  false,
                                                                                                  SpellSchool.Conjuration, SpellSchool.Abjuration);
            var buff = Helpers.CreateBuff(prefix + "SaintsHolyRegaliaFocusBuff",
                                          "Font of Healing",
                                          "The panoply grants you mastery over healing magic. When you cast a cure spell (a spell with the “cure” in its name), you restore 1 additional hit point for every 2 points of total mental focus invested in all of the associated implements, to a maximum bonus equal to half the occultist’s level. This doesn’t apply to damage dealt to undead with the spell.",
                                          "",
                                          Helpers.GetIcon("41c9016596fe1de4faf67425ed691203"), //cure critical wounds
                                          null,
                                          Helpers.Create<HealingMechanics.BonusHealing>(b =>
                                          {
                                              b.value = Helpers.CreateContextValue(AbilityRankType.Default);
                                              b.spells = healing_spells;
                                          }),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, ContextRankProgression.DivStep, stepLevel: 2,
                                                                          customProperty: property)
                                          );
            return buff;
        }
    }
}
