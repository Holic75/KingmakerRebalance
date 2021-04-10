using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild.TurnActionMechanics
{
    public class UnitPartFamiliarFreeItemUse : AdditiveUnitPart
    {
        private bool used_this_round = false;
        private bool in_use = false;

        public void prepare()
        {
            in_use = true;
        }

        public void use()
        {
            used_this_round = true;
        }

        public void unprepare()
        {
            in_use = false;
        }

        public void reset()
        {
            used_this_round = false;
        }

        public bool isActive()
        {
            return !this.buffs.Empty() && !used_this_round && in_use;
        }


        public bool canBeUsed()
        {
            return !this.buffs.Empty() && !used_this_round;
        }
    }


    [Harmony12.HarmonyPatch(typeof(BlueprintItemEquipmentUsable))]
    [Harmony12.HarmonyPatch("IsUnitNeedUMDForUse", Harmony12.MethodType.Normal)]
    class BlueprintItemEquipmentUsable_IsUnitNeedUMDForUse__Patch
    {
        static void Postfix(BlueprintItemEquipmentUsable __instance, UnitDescriptor unit,  ref bool __result)
        {
            if (__result == true)
            {
                return;
            }
            if (__instance.RequireUMDIfCasterHasNoSpellInSpellList)
            {
                var unit_part = unit.Get<UnitPartFamiliarFreeItemUse>();
                if (unit_part == null || !unit_part.isActive())
                {
                    return;
                }
                __result = true;
            }
        }
    }


    public class UnitPartFreeAbilityUse : AdditiveUnitPartWithCheckLock
    {
        public bool canBeUsedOnAbility(AbilityData ability, CommandType actual_action_type)
        {
            if (buffs.Empty())
            {
                return false;
            }

            foreach (var b in buffs)
            {
                bool result = check<FreeActionAbilityUseBase>(b, c => c.canUseOnAbility(ability, actual_action_type));
                if (!result)
                {
                    var sticky_touch = ability?.StickyTouch;
                    if (sticky_touch != null)
                    {
                        result = check<FreeActionAbilityUseBase>(b, c => c.canUseOnAbility(sticky_touch, actual_action_type));
                    }
                }
                if (result)
                {
                    return true;
                }
            }

            return false;
        }
    }



    public abstract class FreeActionAbilityUseBase: BuffLogic
    {
        public abstract bool canUseOnAbility(AbilityData ability, CommandType actual_action_type);

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartFreeAbilityUse>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartFreeAbilityUse>().removeBuff(this.Fact);
        }
    }


    public class UnitPartSwiftAbilityUse : AdditiveUnitPartWithCheckLock
    {
        public bool canBeUsedOnAbility(AbilityData ability, CommandType actual_action_type)
        {
            if (buffs.Empty())
            {
                return false;
            }

            foreach (var b in buffs)
            {
                bool result = check<SwiftActionAbilityUseBase>(b, c => c.canUseOnAbility(ability, actual_action_type));
                if (!result)
                {
                    var sticky_touch = ability?.StickyTouch;
                    if (sticky_touch != null)
                    {
                        result = check<SwiftActionAbilityUseBase>(b, c => c.canUseOnAbility(sticky_touch, actual_action_type));
                    }
                }
                if (result)
                {
                    return true;
                }
            }

            return false;
        }
    }


    public class UnitPartMoveAbilityUse : AdditiveUnitPartWithCheckLock
    {
        public bool canBeUsedOnAbility(AbilityData ability, CommandType actual_action_type)
        {
            if (buffs.Empty())
            {
                return false;
            }

            foreach (var b in buffs)
            {
                bool result = check<MoveActionAbilityUseBase>(b, c => c.canUseOnAbility(ability, actual_action_type));
                if (!result)
                {
                    var sticky_touch = ability?.StickyTouch;
                    if (sticky_touch != null)
                    {
                        result = check<MoveActionAbilityUseBase>(b, c => c.canUseOnAbility(sticky_touch, actual_action_type));
                    }
                }
                if (result)
                {
                    return true;
                }
            }

            return false;
        }
    }


    public class UnitPartStandardActionAbilityUse : AdditiveUnitPartWithCheckLock
    {
        public bool canBeUsedOnAbility(AbilityData ability, CommandType actual_action_type)
        {
            if (buffs.Empty())
            {
                return false;
            }

            foreach (var b in buffs)
            {
                bool result = check<StandardActionAbilityUseBase>(b, c => c.canUseOnAbility(ability, actual_action_type));
                if (!result)
                {
                    var sticky_touch = ability?.StickyTouch;
                    if (sticky_touch != null)
                    {
                        result = check<StandardActionAbilityUseBase>(b, c => c.canUseOnAbility(sticky_touch, actual_action_type));
                    }
                }
                if (result)
                {
                    return true;
                }
            }

            return false;
        }
    }


    public abstract class MoveActionAbilityUseBase : BuffLogic
    {
        public abstract bool canUseOnAbility(AbilityData ability, CommandType actual_action_type);

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartMoveAbilityUse>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartMoveAbilityUse>().removeBuff(this.Fact);
        }
    }





    public abstract class StandardActionAbilityUseBase : BuffLogic
    {
        public abstract bool canUseOnAbility(AbilityData ability, CommandType actual_action_type);

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartStandardActionAbilityUse>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartStandardActionAbilityUse>().removeBuff(this.Fact);
        }
    }



    public abstract class SwiftActionAbilityUseBase : BuffLogic
    {
        public abstract bool canUseOnAbility(AbilityData ability, CommandType actual_action_type);

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartSwiftAbilityUse>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartSwiftAbilityUse>().removeBuff(this.Fact);
        }
    }


    public class UseAbilitiesWithSameOrLowerActionCostForFree : FreeActionAbilityUseBase, IUnitSubscriber, IInitiatorRulebookHandler<RuleCastSpell>
    {
        public int num_uses;
        public BlueprintAbility[] abilities;
        public int[] groups;
        [JsonProperty]
        private CommandType command_type;
        [JsonProperty]
        private bool is_full_round;

        [JsonProperty]
        private int remaining_uses;
        [JsonProperty]
        private bool[] used_groups = null;

        public override bool canUseOnAbility(AbilityData ability, CommandType actual_action_type)
        {
            if (remaining_uses <= 0)
            {
                return false;
            }

            if (remaining_uses == num_uses)
            {
                return false;
            }

            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i] == ability?.Blueprint)
                {
                    if (used_groups[groups[i]])
                    {
                        return false;
                    }
                    if (num_uses == remaining_uses)
                    {
                        return true;
                    }
                    else
                    {
                        return Aux.isNotSlower(ability.Blueprint.ActionType, ability.Blueprint.IsFullRoundAction, command_type, is_full_round);
                    }                   
                }
            }
            return false;
        }

        public override void OnTurnOn()
        {
            base.OnTurnOn();
            if (used_groups == null)
            {
                used_groups = new bool[groups.Max() + 1];
            }
            remaining_uses = num_uses;
        }


        public void OnEventAboutToTrigger(RuleCastSpell evt)
        {
        }

        public void OnEventDidTrigger(RuleCastSpell evt)
        {  
            if (remaining_uses <= 0)
            {
                return;
            }
            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i] == evt.Spell?.Blueprint && !used_groups[groups[i]])
                {
                    if (remaining_uses == num_uses)
                    {
                        command_type = evt.Spell.Blueprint.ActionType;
                        is_full_round = evt.Spell.Blueprint.IsFullRoundAction;
                    }
                    else if (!Aux.isNotSlower(evt.Spell.Blueprint.ActionType, evt.Spell.Blueprint.IsFullRoundAction, command_type, is_full_round))
                    {
                        return;
                    }
                    used_groups[groups[i]] = true;
                    remaining_uses--;
                    if (remaining_uses == 0)
                    {
                        this.Buff.Remove();
                    }
                    return;
                }
            }
        }
    }


    public class IterativeAttacksWithAbilities : FreeActionAbilityUseBase, IInitiatorRulebookHandler<RuleAttackRoll>, IGlobalSubscriber, IRulebookHandler<RuleAttackRoll>, IInitiatorRulebookSubscriber
    {
        private int m_MainAttacksCount;
        private int m_PenalizedAttacksCount;
        private int m_AttackPenalty;

        public BlueprintAbility[] abilities;

        public override bool canUseOnAbility(AbilityData ability, CommandType actual_action_type)
        {
            return abilities.Contains(ability.Blueprint);
        }

        public override void OnTurnOn()
        {
            base.OnTurnOn();

            RuleCalculateAttacksCount calculateAttacksCount = Rulebook.Trigger<RuleCalculateAttacksCount>(new RuleCalculateAttacksCount(this.Owner.Unit)
            {
                ForceIterativeNaturealAttacks = true
            });
            this.m_MainAttacksCount = calculateAttacksCount.PrimaryHand.MainAttacks;
            this.m_PenalizedAttacksCount = calculateAttacksCount.PrimaryHand.PenalizedAttacks;
            this.m_AttackPenalty = 0;
        }


        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (!(evt.Reason.Ability != (AbilityData)null) || !((IList<BlueprintAbility>)this.abilities).HasItem<BlueprintAbility>(evt.Reason.Ability.Blueprint))
                return;
                 
            if (this.m_MainAttacksCount < 1)
                this.m_AttackPenalty += 5;
            if (this.m_MainAttacksCount > 0)
                --this.m_MainAttacksCount;
            else if (this.m_PenalizedAttacksCount > 0)
                --this.m_PenalizedAttacksCount;

            evt.SetAttackBonusPenalty(this.m_AttackPenalty);
           
            if (this.m_MainAttacksCount <= 0 && this.m_PenalizedAttacksCount <= 0)
                this.Buff.Remove();
        }

        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
        }
    }

    public class SpellSynthesis : FreeActionAbilityUseBase, IUnitSubscriber, IInitiatorRulebookHandler<RuleCastSpell>
    {
        public bool is_arcane;
        public CommandType action_type;
        public ContextValue max_spell_level;
        public bool is_full_round;

        public override bool canUseOnAbility(AbilityData ability, CommandType actual_action_type)
        {
            if (ability == null)
            {
                return false;
            }

            if (ability.Spellbook == null)
            {
                return false;
            }

            if (actual_action_type != action_type)
            {
                return false;
            }

            if (actual_action_type == CommandType.Standard)
            {
                bool need_full_round = ability.RequireFullRoundAction;
                if (is_full_round != need_full_round)
                {
                    return false;
                }
            }

            if (ability.SpellLevel > max_spell_level.Calculate(this.Fact.MaybeContext))
            {
                return false;
            }

            if (ability.Spellbook.Blueprint.IsArcane != is_arcane)
            {
                return false;
            }
            return true;
        }

        public void OnEventAboutToTrigger(RuleCastSpell evt)
        {

        }

        public void OnEventDidTrigger(RuleCastSpell evt)
        {
            if (canUseOnAbility(evt.Spell, CommandType.Free) || canUseOnAbility(evt.Spell, action_type))
            {
                this.Buff.Remove();
            }
        }
    }



    public class FreeTouchOrPersonalSpellUseFromSpellbook : FreeActionAbilityUseBase, IUnitSubscriber, IInitiatorRulebookHandler<RuleCastSpell>
    {
        public BlueprintSpellbook allowed_spellbook;
        public ContextValue max_spell_level;
        public BlueprintBuff control_buff;

        public override bool canUseOnAbility(AbilityData ability, CommandType actual_action_type)
        {
            if (control_buff != null && !ability.Caster.Buffs.HasFact(control_buff))
            {
                return false;
            }

            if (ability == null)
            {
                return false;
            }

            if (ability.Spellbook == null)
            {
                return false;
            }

            if (ability.SpellLevel > max_spell_level.Calculate(this.Fact.MaybeContext))
            {
                return false;
            }

            if (ability.Spellbook.Blueprint != allowed_spellbook)
            {
                return false;
            }


            if (!Common.isPersonalSpell(ability))
            {
                return false;
            }

            if (!(ability.Blueprint.Range == AbilityRange.Personal || ability.Blueprint.Range == AbilityRange.Touch))
            {
                return false;
            }
            return true;
        }

        public void OnEventAboutToTrigger(RuleCastSpell evt)
        {

        }

        public void OnEventDidTrigger(RuleCastSpell evt)
        {
            if (canUseOnAbility(evt.Spell, CommandType.Free))
            {
                if (control_buff != null)
                {
                    this.Owner?.Buffs?.GetBuff(control_buff)?.Remove();
                }
            }
        }
    }



    public class FamiliarFreeItemUse : FreeActionAbilityUseBase, IUnitSubscriber, IInitiatorRulebookHandler<RuleCastSpell>, ITickEachRound, IGlobalSubscriber
    {
        public override void OnTurnOn()
        {
            base.OnTurnOn();
            this.Owner.Ensure<UnitPartFamiliarFreeItemUse>().addBuff(this.Fact);
        }

        public override void OnTurnOff()
        {
            base.OnTurnOff();
            this.Owner.Ensure<UnitPartFamiliarFreeItemUse>().removeBuff(this.Fact);
        }

        public override bool canUseOnAbility(AbilityData ability, CommandType actual_action_type)
        {
            if (ability == null)
            {
                return false;
            }
            var unit_part = ability.Caster.Get<UnitPartFamiliarFreeItemUse>();
            if (unit_part == null)
            {
                return false;
            }

            if (!unit_part.isActive())
            {
                return false;
            }

            if (ability.SourceItem == null || !(ability.SourceItem.Blueprint is BlueprintItemEquipmentUsable))
            {
                return false;
            }


            var item = ability.SourceItem.Blueprint as BlueprintItemEquipmentUsable;

            if (item.RequireUMDIfCasterHasNoSpellInSpellList && !ability.Caster.HasUMDSkill)
            {
                return false;
            }
            return true;
        }

        public void OnNewRound()
        {
            this.Owner.Ensure<UnitPartFamiliarFreeItemUse>().reset();
        }

        public void OnEventAboutToTrigger(RuleCastSpell evt)
        {

        }

        public void OnEventDidTrigger(RuleCastSpell evt)
        {
            if (canUseOnAbility(evt.Spell, CommandType.Free))
            {
                this.Owner.Ensure<UnitPartFamiliarFreeItemUse>().use();

                var ability = this.Owner.ActivatableAbilities.Enumerable
                              .Where(a => a.IsOn && a.Blueprint.Group == ActivatableAbilityGroupExtension.Familiar.ToActivatableAbilityGroup()).FirstOrDefault();
                if (ability != null)
                {
                    ability.IsOn = false;
                }
                /*var buff = ability?.Blueprint?.Buff;
                if (buff != null)
                {
                    this.Owner.Buffs?.GetBuff(buff)?.Remove();
                }*/
            }
        }
    }


    public class RestrictionCanUseFamiliarFreeCast : ActivatableAbilityRestriction
    {
        public bool Not;

        public override bool IsAvailable()
        {
            return this.Owner.Ensure<UnitPartFamiliarFreeItemUse>().canBeUsed() != Not;
        }
    }

    public class ActivateFamiliarFreeCast : OwnedGameLogicComponent<UnitDescriptor>
    {
        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartFamiliarFreeItemUse>().prepare();
        }

        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartFamiliarFreeItemUse>().unprepare();
        }
    }




    public class UseAbilitiesAsSwiftAction : SwiftActionAbilityUseBase
    {
        public BlueprintAbility[] abilities;

        public override bool canUseOnAbility(AbilityData ability, CommandType actual_action_type)
        {
            if (ability == null)
            {
                return false;
            }

            return abilities.Contains(ability.Blueprint) || (ability.Blueprint.Parent == null ? false : abilities.Contains(ability.Blueprint.Parent));
        }
    }


    public class UseAbilitiesAsFreeAction : FreeActionAbilityUseBase
    {
        public BlueprintAbility[] abilities;

        public override bool canUseOnAbility(AbilityData ability, CommandType actual_action_type)
        {
            if (ability == null)
            {
                return false;
            }

            return abilities.Contains(ability.Blueprint) || (ability.Blueprint.Parent == null ? false : abilities.Contains(ability.Blueprint.Parent));
        }
    }


    [Harmony12.HarmonyPatch(typeof(AbilityData))]
    [Harmony12.HarmonyPatch("ActionType", Harmony12.MethodType.Getter)]
    class AbilityData_ActionType__Patch
    {
        static void Postfix(AbilityData __instance, ref CommandType __result)
        {
            if (__instance.Caster == null)
            {
                return;
            }

            var free_use_part = __instance.Caster.Get<UnitPartFreeAbilityUse>();

            if (free_use_part != null && free_use_part.canBeUsedOnAbility(__instance, __result))
            {
                __result = CommandType.Free;
                return;
            }

            var swift_use_part = __instance.Caster.Get<UnitPartSwiftAbilityUse>();

            if (swift_use_part != null && swift_use_part.canBeUsedOnAbility(__instance, __result))
            {
                __result = CommandType.Swift;
                return;
            }


            var move_use_part = __instance.Caster.Get<UnitPartMoveAbilityUse>();

            if (move_use_part != null && move_use_part.canBeUsedOnAbility(__instance, __result))
            {
                __result = CommandType.Move;
                return;
            }


            if (__result == CommandType.Standard && __instance.Blueprint.ActionType == CommandType.Swift)
            {
                //metamagic applied to swift action spells
                var fast_metamagic = __instance.Caster.Get<SpellManipulationMechanics.UnitPartNoSpontnaeousMetamagicCastingTimeIncrease>();
                if (fast_metamagic == null)
                {
                    return;
                }

                if (!__instance.Blueprint.IsSpell)
                {
                    return;
                }

                if (__instance.MetamagicData.MetamagicMask == 0)
                {
                    return;
                }

                if (fast_metamagic.canBeUsedOnAbility(__instance))
                {
                    __result = CommandType.Swift;
                }
            }
        }
    }



    public class UnitPartFullRoundAbilityUse : AdditiveUnitPartWithCheckLock
    {
        public bool canBeUsedOnAbility(AbilityData ability)
        {
            if (buffs.Empty())
            {
                return false;
            }

            foreach (var b in buffs)
            {
                bool result = check<FullRoundAbilityUseBase>(b, c => c.canUseOnAbility(ability));

                if (result)
                {
                    return true;
                }
            }

            return false;
        }
    }



    public abstract class FullRoundAbilityUseBase : BuffLogic
    {
        public abstract bool canUseOnAbility(AbilityData ability);

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartFullRoundAbilityUse>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartFullRoundAbilityUse>().removeBuff(this.Fact);
        }
    }


    [Harmony12.HarmonyPatch(typeof(AbilityData))]
    [Harmony12.HarmonyPatch("RequireFullRoundAction", Harmony12.MethodType.Getter)]
    class AbilityData__RequireFullRoundAction__Patch
    {
        static void Postfix(AbilityData __instance, ref bool __result)
        {
            if (__result == true)
            {
                var standard_action_use = __instance.Caster.Get<UnitPartStandardActionAbilityUse>();
                if (standard_action_use != null)
                {
                    __result = !standard_action_use.canBeUsedOnAbility(__instance, CommandType.Standard);
                }
                return;
            }

            var full_round_use_part = __instance.Caster.Get<UnitPartFullRoundAbilityUse>();

            if (full_round_use_part == null)
            {
                return;
            }

            if (full_round_use_part.canBeUsedOnAbility(__instance))
            {
                __result = true;
            }

        }
    }


    public class ForceFullRoundOnAbilities : FullRoundAbilityUseBase
    {
        public BlueprintAbility[] abilities;

        public override bool canUseOnAbility(AbilityData ability)
        {
            if (ability == null)
            {
                return false;
            }

            return abilities.Contains(ability.Blueprint) || (ability.Blueprint.Parent == null ? false : abilities.Contains(ability.Blueprint.Parent));
        }
    }



    public class StandardActionIfWeaponTraining : StandardActionAbilityUseBase
    {
        public BlueprintAbility[] abilities;

        public override bool canUseOnAbility(AbilityData ability, CommandType command)
        {
            if (ability == null)
            {
                return false;
            }

            int? maxWeaponRank = this.Owner.Get<UnitPartWeaponTraining>()?.GetMaxWeaponRank();
            int num = !maxWeaponRank.HasValue ? 0 : maxWeaponRank.Value;

            if (num == 0)
            {
                return false;
            }

            return abilities.Contains(ability.Blueprint) || (ability.Blueprint.Parent == null ? false : abilities.Contains(ability.Blueprint.Parent));
        }
    }


    public class MoveActionIfWeaponTraining : MoveActionAbilityUseBase
    {
        public BlueprintAbility[] abilities;

        public override bool canUseOnAbility(AbilityData ability, CommandType command)
        {
            if (ability == null)
            {
                return false;
            }

            int? maxWeaponRank = this.Owner.Get<UnitPartWeaponTraining>()?.GetMaxWeaponRank();
            int num = !maxWeaponRank.HasValue ? 0 : maxWeaponRank.Value;

            if (num == 0)
            {
                return false;
            }

            return abilities.Contains(ability.Blueprint) || (ability.Blueprint.Parent == null ? false : abilities.Contains(ability.Blueprint.Parent));
        }
    }


    public class MoveActionAbilityUse : MoveActionAbilityUseBase
    {
        public BlueprintAbility[] abilities;

        public override bool canUseOnAbility(AbilityData ability, CommandType command)
        {
            if (ability == null)
            {
                return false;
            }

            return abilities.Contains(ability.Blueprint) || (ability.Blueprint.Parent == null ? false : abilities.Contains(ability.Blueprint.Parent));
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class ConsumeAction : ContextAction
    {
        public bool consume_swift;
        public bool consume_move;
        public bool consume_standard;
        public bool from_caster = true;

        public override string GetCaption()
        {
            return "Consume specified action";
        }

        public override void RunAction()
        {
            var unit = from_caster ? this.Context.MaybeCaster : this.Target.Unit;
            if (unit == null)
            {
                return;
            }
            if (consume_swift && unit.CombatState.Cooldown.SwiftAction == 0.0f)
            {
                unit.CombatState.Cooldown.SwiftAction += 6.0f;
            }
            if (consume_move && unit.CombatState.Cooldown.MoveAction <= 3.01f)
            {
                unit.CombatState.Cooldown.MoveAction += 3.0f;
            }
            if (consume_standard && unit.CombatState.Cooldown.StandardAction == 0.0f)
            {
                unit.CombatState.Cooldown.StandardAction = 6.0f;
            }
        }
    }


    public class ContextConditionHasAction : ContextCondition
    {
        public bool has_swift = false;
        public bool has_standard = false;
        public bool has_move = false;
        public bool check_caster = true;

        protected override string GetConditionCaption()
        {
            return "Has specified action";
        }

        protected override bool CheckCondition()
        {
            var unit = check_caster ?  this.Context.MaybeCaster : this.Target.Unit;
            if (unit == null)
            {
                return false;
            }

            if (has_swift && unit.CombatState.Cooldown.SwiftAction > 0.0f)
            {
                return false;
            }
            if (has_move && unit.CombatState.Cooldown.MoveAction > 3.01f)
            {
                return false;
            }
            if (has_standard && unit.CombatState.Cooldown.StandardAction > 0.0f)
            {
                return false;
            }

            return true;
        }
    }


    public class Aux
    {
        //return true if action1 is not slower than action2
        static public bool isNotSlower(CommandType command1, bool is_full_round1, CommandType command2, bool is_full_round2)
        {
            //1 is full round, 2 is not
            if (is_full_round1 && command1 == CommandType.Standard && !is_full_round2)
            {
                return false;
            }

            //2 is full round
            if (is_full_round2 && command2 == CommandType.Standard)
            {
                return true;
            }

            if (command1 == CommandType.Standard)
            {
                return command2 == CommandType.Standard;
            }

            if (command1 == CommandType.Move)
            {
                return command2 == CommandType.Standard || command2 == CommandType.Move;
            }

            if (command1 == CommandType.Swift)
            {
                return command2 != CommandType.Free;
            }

            return true;
        }
    }
}
