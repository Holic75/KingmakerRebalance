﻿using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Log;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.SkillMechanics
{

    //make dialog skill checks account for temporary skill bonuses if possible
    /*[Harmony12.HarmonyPatch(typeof(RulePartySkillCheck))]
    [Harmony12.HarmonyPatch("Calculate", Harmony12.MethodType.Normal)]
    class Patch_RulePartySkillCheck_Calculate_Transpiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var check_d20_index = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("D20"));

            codes[check_d20_index] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                   new Func<RuleSkillCheck, int>(getD20Value).Method);
            return codes.AsEnumerable();
        }


        static private int getD20Value(RuleSkillCheck evt)
        {
            if (evt.IsTriggererd)
            {
                return evt.D20 + evt.Bonus;
            }
            else
            {
                return evt.D20;
            }
        }
    }*/


    [Harmony12.HarmonyPatch(typeof(RulePartySkillCheck))]
    [Harmony12.HarmonyPatch("Calculate", Harmony12.MethodType.Normal)]
    class Patch_RulePartySkillCheck_Calculate_Patch
    {
        static bool Prefix(RulePartySkillCheck __instance, bool isTrigger, ref int ___m_D20, ref int ___m_StatValue, StatType ___m_StatType, int ___m_DifficultyClass)
        {
            Main.TraceLog();
            ___m_StatValue = int.MinValue;
            var tr = Harmony12.Traverse.Create(__instance);
            tr.Property("Roller").SetValue(null);
            RuleSkillCheck selected_evt = null;
            foreach (UnitEntityData unitEntityData in Game.Instance.Player.Party)
            {
                if (unitEntityData.Descriptor.State.CanAct)
                {
                    ModifiableValue stat = unitEntityData.Stats.GetStat(___m_StatType);
                    ModifiableValueAttributeStat valueAttributeStat = stat as ModifiableValueAttributeStat;
                    int num = valueAttributeStat != null ? valueAttributeStat.Bonus : stat.ModifiedValue;
                    RuleSkillCheck evt = new RuleSkillCheck(unitEntityData, ___m_StatType, ___m_DifficultyClass)
                    {
                        Voice = __instance.Voice,
                        EnsureSuccess = __instance.EnsureSuccess
                    };

                    if (isTrigger)
                    {
                        evt.Silent = true; ;
                        Rulebook.Trigger<RuleSkillCheck>(evt);
                        num += evt.Bonus;
                    }

                    if (___m_StatValue < num)
                    {
                        ___m_StatValue = num;
                        tr.Property("Roller").SetValue(unitEntityData);
                        selected_evt = evt;
                    }
                }
            }
            if (__instance.Roller == null)
            {
                UberDebug.Log("Roller is null, in the party skillcheck", (object[])Array.Empty<object>());
            }
            else
            {

                if (!isTrigger)
                {
                    selected_evt.Calculate();
                }
                else
                {
                    selected_evt.Silent = false;
                    EventBus.RaiseEvent<IRollSkillCheckHandler>((Action<IRollSkillCheckHandler>)(h => h.HandleOnRuleSkillCheck(selected_evt)));
                    //Game.Instance?.UI?.BattleLogManager?.HandleUnitSkillCheckRolled(selected_evt);
                }
                ___m_D20 = selected_evt.D20;
            }
            return false;
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    [AllowMultipleComponents]
    public class DependentAbilityScoreCheckStatReplacement : RuleInitiatorLogicComponent<RuleSkillCheck>
    {
        public StatType stat;
        public StatType old_stat;
        public StatType new_stat;

        public BlueprintUnitFact do_not_apply_if_has_fact = null;

        private MechanicsContext Context
        {
            get
            {
                MechanicsContext context = (this.Fact as Buff)?.Context;
                if (context != null)
                    return context;
                return (this.Fact as Feature)?.Context;
            }
        }

        public override void OnEventDidTrigger(RuleSkillCheck evt)
        {

        }

        public override void OnEventAboutToTrigger(RuleSkillCheck evt)
        {
            if (do_not_apply_if_has_fact != null && this.Owner.HasFact(do_not_apply_if_has_fact))
            {
                return;
            }

            if (evt.StatType != stat)
                return;

            var dependent_stat = this.Owner.Stats.GetStat<ModifiableValueDependant>(evt.StatType);
            if (dependent_stat == null)
            {
                return;
            }

            var base_stat = (dependent_stat.BaseStat as ModifiableValueSkill);
            if (base_stat == null)
            {
                return;
            }
            if (base_stat.BaseStat.Type != old_stat)
            {
                return;
            }

            var old_stat_value = this.Owner.Stats.GetStat<ModifiableValueAttributeStat>(old_stat).Bonus;
            var new_stat_value = this.Owner.Stats.GetStat<ModifiableValueAttributeStat>(new_stat).Bonus;

            if (new_stat_value <= old_stat_value)
            {
                return;
            }

            evt.Bonus.AddModifier(new_stat_value - old_stat_value, this, Kingmaker.Enums.ModifierDescriptor.Inherent);
        }
    }


    class SetSkillRankToValue : OwnedGameLogicComponent<UnitDescriptor>, IUnitGainLevelHandler
    {
        public StatType skill;
        public ContextValue value;
        public bool increase_by1_on_apply = false;
        [JsonProperty]
        private int previous_value = 0;

        public override void OnFactActivate()
        {
            try
            {
                if (previous_value <= 0)
                {
                    var LevelUp = Game.Instance.UI.CharacterBuildController.LevelUpController;
                    if (base.Owner == LevelUp.Preview || base.Owner == LevelUp.Unit)
                    {
                        var @class = LevelUp.State.SelectedClass;
                        //allow retraining
                        int bonus_ranks = this.value.Calculate(this.Fact.MaybeContext);
                        
                        int extra_skill_points = Math.Max(0, base.Owner.Stats.GetStat(skill).BaseValue + bonus_ranks - LevelUp.State.NextLevel);
                        LevelUp.State.ExtraSkillPoints += extra_skill_points;
                        previous_value = extra_skill_points;
                        //base.Owner.Stats.GetStat(skill).BaseValue = 0;
                    }
                    HandleUnitGainLevel(this.Owner, null);
                    ModifiableValueSkill stat = this.Owner.Stats.GetStat<ModifiableValueSkill>(skill);
                    stat?.ClassSkill.Retain();
                    stat?.UpdateValue();
                    if ((base.Owner == LevelUp.Preview || base.Owner == LevelUp.Unit) && increase_by1_on_apply)
                    {
                        previous_value--;
                    }
                }
            }
            catch (Exception e)
            {
                Main.logger.Log(e.ToString());
            }
        }

        public override void OnFactDeactivate()
        {
            if (previous_value > 0)
            {
                base.Owner.Stats.GetStat(skill).BaseValue -= previous_value;
                previous_value = 0;
            }
            ModifiableValueSkill stat = this.Owner.Stats.GetStat<ModifiableValueSkill>(skill);
            stat?.ClassSkill.Release();
            stat?.UpdateValue();
            base.OnFactDeactivate();
        }

        public void HandleUnitGainLevel(UnitDescriptor unit, BlueprintCharacterClass @class)
        {
            int old_value = previous_value;
            int new_value = this.value.Calculate(this.Fact.MaybeContext);

            base.Owner.Stats.GetStat(skill).BaseValue += (new_value - old_value);
            previous_value = new_value;
        }
    }


    public class ContextActionSkillCheckWithFailures : ContextAction
    {
        public StatType Stat;
        public bool use_custom_dc;
        public ContextValue custom_dc;
        public ActionList Success = Helpers.CreateActionList();
        public ActionList Failure5 = Helpers.CreateActionList();
        public ActionList Failure10 = Helpers.CreateActionList();
        public ActionList Failure = Helpers.CreateActionList();
        public bool on_caster = false;

        public override void RunAction()
        {
            if (this.Target.Unit == null || this.Context.MaybeCaster == null)
            {
                UberDebug.LogError((object)"Target unit is missing", (object[])Array.Empty<object>());
            }
            else
            {
                var dc = !this.use_custom_dc ? this.Context.Params.DC : custom_dc.Calculate(this.Context);
                var skill_check = this.Context.TriggerRule<RuleSkillCheck>(new RuleSkillCheck(on_caster ? this.Context.MaybeCaster : this.Target.Unit, this.Stat, dc) { ShowAnyway = true });

                if (skill_check.IsPassed)
                {
                    this.Success.Run();
                }
                else if (!skill_check.IsSuccessRoll(skill_check.D20, 9))
                {
                    this.Failure10.Run();
                }
                else if (!skill_check.IsSuccessRoll(skill_check.D20, 4))
                {
                    this.Failure5.Run();
                }
                else
                {
                    this.Failure.Run();
                }
            }
        }

        public override string GetCaption()
        {
            return string.Format("Skill check {0} {1}", (object)this.Stat, !this.use_custom_dc ? (object)string.Empty : (object)string.Format("(DC: {0})", (object)this.custom_dc));
        }

        [Serializable]
        private struct ConditionalDCIncrease
        {
            public ConditionsChecker Condition;
            public ContextValue Value;
        }
    }
}
