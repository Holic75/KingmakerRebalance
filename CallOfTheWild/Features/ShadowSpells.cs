using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.ShadowSpells
{

    public class ShadowSpells
    {
        public static BlueprintBuff shadow20_buff;
        public static BlueprintBuff shadow60_buff;
        public static BlueprintBuff shadow80_buff;




        static internal void init()
        {
            var icon = Helpers.GetIcon("14ec7a4e52e90fa47a4c8d63c69fd5c1");
            //add component that will make them receive 5 times more damage
            shadow20_buff = Helpers.CreateBuff("ShadowSummon20Buff",
                                               "Shadow Creature (20%)",
                                               "A shadow creature has one-fifth the hit points of a normal creature of its kind (regardless of whether it’s recognized as shadowy). It deals normal damage and has all normal abilities and weaknesses. Against a creature that recognizes it as a shadow creature, however, the shadow creature’s damage is one-fifth (20%) normal, and all special abilities that do not deal lethal damage are only 20% likely to work. (Roll for each use and each affected character separately.)",
                                               "",
                                               icon,
                                               Common.createPrefabLink("e0a060bdf0389704db438820279c1f79"),
                                               Helpers.CreateSpellDescriptor((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow20),
                                               Helpers.Create<NewMechanics.ReduceMaxHp>(r => r.hp_percent = 80)
                                               );
            //add component that will make them receive 1.66 times more damage
            shadow60_buff = Helpers.CreateBuff("ShadowSummon60Buff",
                                               "Shadow Creature (60%)",
                                               "A shadow creature has three-fifth the hit points of a normal creature of its kind (regardless of whether it’s recognized as shadowy). It deals normal damage and has all normal abilities and weaknesses. Against a creature that recognizes it as a shadow creature, however, the shadow creature’s damage is three-fifth (60%) normal, and all special abilities that do not deal lethal damage are only 60% likely to work. (Roll for each use and each affected character separately.)",
                                               "",
                                               icon,
                                               Common.createPrefabLink("e0a060bdf0389704db438820279c1f79"),
                                               Helpers.CreateSpellDescriptor((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow60),
                                               Helpers.Create<NewMechanics.ReduceMaxHp>(r => r.hp_percent = 40)
                                               );
            //add component that will make them receive 1.20 times more damage
            shadow80_buff = Helpers.CreateBuff("ShadowSummon80Buff",
                                               "Shadow Creature (80%)",
                                               "A shadow creature has four-fifth the hit points of a normal creature of its kind (regardless of whether it’s recognized as shadowy). It deals normal damage and has all normal abilities and weaknesses. Against a creature that recognizes it as a shadow creature, however, the shadow creature’s damage is four-fifth (80%) normal, and all special abilities that do not deal lethal damage are only 80% likely to work. (Roll for each use and each affected character separately.)",
                                               "",
                                               icon,
                                               Common.createPrefabLink("e0a060bdf0389704db438820279c1f79"),
                                               Helpers.CreateSpellDescriptor((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow80),
                                               Helpers.Create<NewMechanics.ReduceMaxHp>(r => r.hp_percent = 20)
                                               );
            //also fix ac calcualtion
        }


        static public bool isShadowBuff(BlueprintBuff buff)
        {
            return buff == shadow20_buff || buff == shadow60_buff || buff == shadow80_buff;
        }

        static public Fact getShadowBuff(UnitDescriptor descriptor)
        {
            var shadow20_fact = descriptor.GetFact(shadow20_buff);
            var shadow60_fact = descriptor.GetFact(shadow60_buff);
            var shadow80_fact = descriptor.GetFact(shadow80_buff);

            if (shadow20_fact != null)
            {
                return shadow20_fact;
            }

            if (shadow60_fact != null)
            {
                return shadow60_fact;
            }

            if (shadow80_fact != null)
            {
                return shadow80_fact;
            }

            return null;
        }


        static bool disbelief_save_in_progress = false;
       
        public static bool is_making_disbelief_save
        {
            get
            {
                return disbelief_save_in_progress;
            }
        }

        static public bool makeDisbeliefSave(MechanicsContext context, UnitEntityData target)
        {
            disbelief_save_in_progress = true;
            Common.AddBattleLogMessage(target.CharacterName + " attempts a disbelief saving throw");
            RuleSavingThrow ruleSavingThrow = context.TriggerRule<RuleSavingThrow>(new RuleSavingThrow(target, SavingThrowType.Will, context.Params.DC));
            bool res =  ruleSavingThrow.IsPassed;
            disbelief_save_in_progress = false;
            return res;
        }
    }



    public class UnitPartDisbelief : UnitPart, IUnitCombatHandler, IGlobalSubscriber
    {
        public Dictionary<MechanicsContext, bool> disbelief_contexts = new Dictionary<MechanicsContext, bool>();

        public void HandleUnitJoinCombat(UnitEntityData unit)
        {
            /*if (unit.Descriptor == this.Owner)
            {
                disbelief_contexts.Clear();
            }*/
        }

        public void HandleUnitLeaveCombat(UnitEntityData unit)
        {
            if (unit.Descriptor == this.Owner)
            {
                Main.logger.Log("Clearing disbelief part for " + this.Owner.CharacterName);
                disbelief_contexts.Clear();
            }
        }
    }

    class DisbeliefSpell : BlueprintComponent
    {

    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class SavingthrowBonusAgainstDisbelief : RuleInitiatorLogicComponent<RuleSavingThrow>
    {
        public SpellSchool school;
        public ModifierDescriptor descriptor;
        public ContextValue value;
        public SavingThrowType save_type = SavingThrowType.Will;

        public override void OnEventAboutToTrigger(RuleSavingThrow evt)
        {
            var context = evt.Reason?.Context;
            if (context == null)
            {
                return;
            }

            var caster = context.MaybeCaster;
            if (caster == null)
            {
                return;
            }

            if (caster != this.Owner.Unit)
            {
                return;
            }

            if (!ShadowSpells.is_making_disbelief_save)
            {
                return;
            }

            if (context.SpellSchool == school
                 && (save_type == SavingThrowType.Unknown || evt.Type == save_type)
                 && (context?.SourceAbility.GetComponent<DisbeliefSpell>() != null
                     || (context.SpellDescriptor.Intersects((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow)
                         && !evt.Initiator.Ensure<UnitPartDisbelief>().disbelief_contexts.ContainsKey(context)
                         )
                    )
                )
            {
                int bonus = this.value.Calculate(this.Fact.MaybeContext);
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveWill.AddModifier(bonus, (GameLogicComponent)this, descriptor));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveReflex.AddModifier(bonus, (GameLogicComponent)this, descriptor));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveFortitude.AddModifier(bonus, (GameLogicComponent)this, descriptor));
            }
        }

        public override void OnEventDidTrigger(RuleSavingThrow evt)
        {
        }
    }


    public class ResilentSpells : OwnedGameLogicComponent<UnitDescriptor>, MetamagicFeats.IRuleSavingThrowTriggered
    {
        public SpellSchool school = SpellSchool.Illusion;
        public SavingThrowType save_type = SavingThrowType.Will;

        public void ruleSavingThrowBeforeTrigger(RuleSavingThrow evt)
        {

        }

        public void ruleSavingThrowTriggered(RuleSavingThrow evt)
        {
            var context = evt.Reason?.Context;
            if (context == null)
            {
                return;
            }

            var caster = context.MaybeCaster;
            if (caster == null)
            {
                return;
            }

            if (caster != this.Owner.Unit)
            {
                return;
            }

            if (!ShadowSpells.is_making_disbelief_save)
            {
                return;
            }

            if (context.SpellSchool == school
                 && (save_type == SavingThrowType.Unknown || evt.Type == save_type)
                 && (context?.SourceAbility.GetComponent<DisbeliefSpell>() != null
                     || (context.SpellDescriptor.Intersects((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow)
                         && !evt.Initiator.Ensure<UnitPartDisbelief>().disbelief_contexts.ContainsKey(context)
                         )
                    )
                )
            {
                var cl_check = RulebookEvent.Dice.D(new DiceFormula(1, DiceType.D20)) + evt.Reason.Context.Params.CasterLevel;
                if (cl_check > evt.DifficultyClass)
                {
                    Common.AddBattleLogMessage("Changing spell DC for " + evt.Initiator.CharacterName + $" form {evt.DifficultyClass} to {cl_check} due to {this.Fact.Name}");
                    Helpers.SetField(evt, "DifficultyClass", cl_check);
                }
            }
        }
    }

    namespace Patches
    {


        //check damage
        [Harmony12.HarmonyPatch(typeof(RuleDealDamage))]
        [Harmony12.HarmonyPatch("OnTrigger", Harmony12.MethodType.Normal)]
        class Patch_RuleDealDamage_OnTrigger_Prefix
        {
            static public bool Prefix(RuleDealDamage __instance, RulebookEventContext context)
            {
                if (__instance.Target == null)
                {
                    return true;
                }

                var context2 = __instance.Reason.Context;
                if (context2?.AssociatedBlueprint != null && context2.AssociatedBlueprint is BlueprintBuff)
                {//do not apply shadow twice
                    return true;
                }
                var summoned_context = ShadowSpells.getShadowBuff(__instance.Initiator.Descriptor)?.MaybeContext;

                if (context2 == null && summoned_context == null)
                {
                    return true;
                }

                var shadow_descriptor2 = (context2?.SpellDescriptor).GetValueOrDefault() & (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow;
                var shadow_summon = (summoned_context?.SpellDescriptor).GetValueOrDefault() & (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow;

                if (shadow_summon == SpellDescriptor.None && shadow_descriptor2 == SpellDescriptor.None)
                {
                    return true;
                }

                if (shadow_summon > shadow_descriptor2)
                {
                    context2 = summoned_context;
                }

                if (!__instance.Target.Ensure<UnitPartDisbelief>().disbelief_contexts.ContainsKey(context2))
                {
                    if (__instance.Target.Descriptor.State.HasCondition(UnitCondition.TrueSeeing))
                    {
                        __instance.Target.Ensure<UnitPartDisbelief>().disbelief_contexts[context2] = true;
                    }
                    else
                    {
                        __instance.Target.Ensure<UnitPartDisbelief>().disbelief_contexts[context2] = ShadowSpells.makeDisbeliefSave(context2, __instance.Target);
                    }
                }

                if (__instance.Target.Ensure<UnitPartDisbelief>().disbelief_contexts[context2])
                {
                    if ((context2.SpellDescriptor & (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow) == (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow20)
                    {
                        __instance.ReducedBecauseOfShadowEvocation = true;
                    }
                    else if ((context2.SpellDescriptor & (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow) == (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow60)
                    {
                        __instance.ReducedBecauseOfShadowEvocationGreater = true;
                    }
                    else if ((context2.SpellDescriptor & (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow) == (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow80)
                    {
                        __instance.Modifier = new float?( (__instance.Modifier.HasValue ? __instance.Modifier.GetValueOrDefault() : 1f) *0.8f);
                    }
                }
                return true;
            }
        }

        //check if buffs will be applied
        [Harmony12.HarmonyPatch(typeof(RuleApplyBuff))]
        [Harmony12.HarmonyPatch("OnTrigger", Harmony12.MethodType.Normal)]
        class Patch_RuleApplyBuff_OnTrigger_Prefix
        {
            static public bool Prefix(RuleApplyBuff __instance, RulebookEventContext context)
            {
                /*if (ShadowSpells.isShadowBuff(__instance.Blueprint))
                {
                    return true;
                }*/
                var rule_summon = Rulebook.CurrentContext.AllEvents.LastOfType<RuleSummonUnit>();
                if (rule_summon != null)
                {//do not interrupt summon buffs
                    return true;
                }

                var context2 = __instance.Reason.Context;
                //there are also actions after summon that should not be affected
                //we need to check if we are still inside SpawnMonsterComponent
                var summon_context = __instance.Initiator.Buffs?.GetBuff(Game.Instance.BlueprintRoot.SystemMechanics.SummonedUnitBuff)?.MaybeContext?.ParentContext;
                if (summon_context == context2)
                {
                    return true;
                }

                       
                if (context2?.AssociatedBlueprint != null && context2.AssociatedBlueprint is BlueprintBuff)
                {//do not apply shadow twice
                    return true;
                }
                var summoned_context = ShadowSpells.getShadowBuff(__instance.Initiator.Descriptor)?.MaybeContext;

                if (context2 == null && summoned_context == null)
                {
                    return true;
                }


                var shadow_descriptor2 = (context2?.SpellDescriptor).GetValueOrDefault() & (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow;
                var shadow_summon = (summoned_context?.SpellDescriptor).GetValueOrDefault() & (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow;

                if (shadow_summon == SpellDescriptor.None && shadow_descriptor2 == SpellDescriptor.None)
                {
                    return true;
                }


                if (shadow_summon > shadow_descriptor2)
                {
                    context2 = summoned_context;
                }
           
                if (__instance.Initiator == null)
                {
                    return true;
                }

                if (!__instance.Initiator.Ensure<UnitPartDisbelief>().disbelief_contexts.ContainsKey(context2))
                {
                    if (__instance.Initiator.Descriptor.State.HasCondition(UnitCondition.TrueSeeing))
                    {
                        __instance.Initiator.Ensure<UnitPartDisbelief>().disbelief_contexts[context2] = true;
                    }
                    else
                    {
                        __instance.Initiator.Ensure<UnitPartDisbelief>().disbelief_contexts[context2] = ShadowSpells.makeDisbeliefSave(context2, __instance.Initiator);
                    }
                }

                if (__instance.Initiator.Ensure<UnitPartDisbelief>().disbelief_contexts[context2])
                {
                    if ((context2.SpellDescriptor & (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow) == (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow20)
                    {
                        if (RulebookEvent.Dice.D(new DiceFormula(1, DiceType.D100)) > 20)
                        {
                            __instance.CanApply = false;
                            Common.AddBattleLogMessage(__instance.Initiator.CharacterName + " avoids " + context2.SourceAbility.Name + " effect due to disbelief");
                            return false;
                        }
                    }
                    else if ((context2.SpellDescriptor & (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow) == (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow60)
                    {
                        if (RulebookEvent.Dice.D(new DiceFormula(1, DiceType.D100)) > 60)
                        {
                            __instance.CanApply = false;
                            Common.AddBattleLogMessage(__instance.Initiator.CharacterName + " avoids " + context2.SourceAbility.Name + " effect due to disbelief");
                            return false;
                        }
                    }
                    else if ((context2.SpellDescriptor & (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow) == (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow80)
                    {
                        if (RulebookEvent.Dice.D(new DiceFormula(1, DiceType.D100)) > 80)
                        {
                            __instance.CanApply = false;
                            Common.AddBattleLogMessage(__instance.Initiator.CharacterName + " avoids " + context2.SourceAbility.Name + " effect due to disbelief");
                            return false;
                        }
                    }
                }

                return true;
            }
        }


        //check if buffs will be applied
        [Harmony12.HarmonyPatch(typeof(RuleSummonUnit))]
        [Harmony12.HarmonyPatch("OnTrigger", Harmony12.MethodType.Normal)]
        class Patch_ContextActionSpawnMonster_RunAction_Prefix
        {
            static public void Postfix(RuleSummonUnit __instance)
            {
                var context = __instance.Context;
                if (context == null)
                {
                    return;
                }
                var rounds = __instance.Duration + __instance.BonusDuration;
                if ((context.SpellDescriptor & (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow) == (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow20)
                {
                    __instance.SummonedUnit.Descriptor.AddBuff(ShadowSpells.shadow20_buff, __instance.Context, new TimeSpan?(rounds.Seconds));
                }
                else if ((context.SpellDescriptor & (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow) == (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow60)
                {
                    __instance.SummonedUnit.Descriptor.AddBuff(ShadowSpells.shadow60_buff, __instance.Context, new TimeSpan?(rounds.Seconds));
                }
                else if ((context.SpellDescriptor & (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow) == (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow80)
                {
                    __instance.SummonedUnit.Descriptor.AddBuff(ShadowSpells.shadow80_buff, __instance.Context, new TimeSpan?(rounds.Seconds));
                }
            }
        }
    }
}
