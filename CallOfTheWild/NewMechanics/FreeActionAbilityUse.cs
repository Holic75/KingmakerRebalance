using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild.FreeActionAbilityUseMechanics
{
    public class UnitPartFreeAbilityUse : AdditiveUnitPart
    {
        public bool canBeUsedOnAbility(AbilityData ability, CommandType actual_action_type)
        {
            if (buffs.Empty())
            {
                return false;
            }

            foreach (var b in buffs)
            {
                if (b.Get<FreeActionAbilityUse>().canUseOnAbility(ability, actual_action_type))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public abstract class FreeActionAbilityUse: BuffLogic
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


    public class IterativeAttacksWithAbilities : FreeActionAbilityUse, IInitiatorRulebookHandler<RuleAttackRoll>, IGlobalSubscriber, IRulebookHandler<RuleAttackRoll>, IInitiatorRulebookSubscriber
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

    public class SpellSynthesis : FreeActionAbilityUse, IUnitSubscriber, IInitiatorRulebookHandler<RuleCastSpell>
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
                bool need_full_round = !ability.HasMetamagic(Metamagic.Quicken)
                                         && (
                                             (ability.Spellbook.Blueprint.Spontaneous && ability.MetamagicData != null && ability.MetamagicData.NotEmpty)
                                             || (ability.Blueprint.IsFullRoundAction)
                                             );
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

            if (free_use_part == null)
            {
                return;
            }

            if (free_use_part.canBeUsedOnAbility(__instance, __result))
            {
                __result = CommandType.Free;
            }
        }
    }

}
