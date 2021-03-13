using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Actions;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.HealingMechanics
{

    [AllowedOn(typeof(BlueprintUnitFact))]
    public class IncomingHealingModifier : OwnedGameLogicComponent<UnitDescriptor>, ITargetRulebookSubscriber, ITargetRulebookHandler<RuleHealDamage>
    {
        public ContextValue ModifierPercents;

        public void OnEventAboutToTrigger(RuleHealDamage evt)
        {
            float val = (float)this.ModifierPercents.Calculate(this.Fact.MaybeContext) / 100.0f;
            if (evt.Modifier.HasValue)
            {
                evt.Modifier = new float?(evt.Modifier.Value * val);
            }
            else
            {
                evt.Modifier = new float?(val);
            }
        }

        public void OnEventDidTrigger(RuleHealDamage evt)
        {
        }
    }


    public class UnitPartReceiveBonusCasterLevelHealing : AdditiveUnitPart
    {
        public bool active()
        {
            return !buffs.Empty();
        }
    }


    public class UnitPartHealingMetamagic : AdditiveUnitPart
    {
        public bool hasEmpower(BlueprintAbility spell, UnitEntityData caster, UnitEntityData target)
        {
            bool is_ok = false;
            foreach (var b in buffs)
            {
                b.CallComponents<HealingMetamagic>(h => is_ok = h.worksOnEmpower(spell, caster, target));
                if (is_ok)
                {
                    return true;
                }
            }

            return false;
        }

        public bool hasMaximize(BlueprintAbility spell, UnitEntityData caster, UnitEntityData target)
        {
            bool is_ok = false;
            foreach (var b in buffs)
            {
                b.CallComponents<HealingMetamagic>(h => is_ok = h.worksOnMaximize(spell, caster, target));
                if (is_ok)
                {
                    return true;
                }
            }

            return false;
        }
    }



    public class UnitPartExtendHpBonusToCasterLevel : AdditiveUnitPart
    {
        public bool active(BlueprintAbility spell)
        {
            //Main.logger.Log("Checking UnitPartExtendHpBonusToCasterLevel " + spell.AssetGuid + " " + spell.name);
            foreach (var b in buffs)
            {
                var comp = b.Blueprint.GetComponent<ExtendHpBonusToCasterLevel>();
                if (comp == null)
                {
                    continue;
                }
                if (comp.worksOn(spell))
                {
                    return true;
                }
            }

            return false;
        }
    }



    public class UnitPartReceiveBonusHpPerDie : AdditiveUnitPart
    {
        public int getAmount(BlueprintAbility spell)
        {
            var amount = 0;

            foreach (var b in buffs)
            {
                var comp = b.Blueprint.GetComponent<ReceiveBonusHpPerDie>();
                if (comp == null)
                {
                    continue;
                }
                b.CallComponents<ReceiveBonusHpPerDie>(a => amount += a.perDieAmount(spell));
            }

            return amount;
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class ReceiveBonusHpPerDie : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {
        public int per_die_amount;

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartReceiveBonusHpPerDie>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartReceiveBonusHpPerDie>().removeBuff(this.Fact);
        }

        public int perDieAmount(BlueprintAbility spell)
        {
            return per_die_amount;
        }
    }

    public class UnitPartBonusHealing : AdditiveUnitPart
    {
        public int getAmount(BlueprintAbility spell)
        {
            var amount = 0;

            foreach (var b in buffs)
            {
                var comp = b.Blueprint.GetComponent<BonusHealing>();
                if (comp == null)
                {
                    continue;
                }
                b.CallComponents<BonusHealing>(a => amount += a.getBonusHealing(spell));
            }

            return amount;
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class BonusHealing : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {
        public ContextValue value;
        public BlueprintAbility[] spells;

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartBonusHealing>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartBonusHealing>().removeBuff(this.Fact);
        }

        public int getBonusHealing(BlueprintAbility spell)
        {
            if (spells.Contains(spell))
            {
                return value.Calculate(this.Fact.MaybeContext);
            }
            return 0;
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class ExtendHpBonusToCasterLevel : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {
        public BlueprintAbility[] spells;

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartExtendHpBonusToCasterLevel>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartExtendHpBonusToCasterLevel>().removeBuff(this.Fact);
        }

        public bool worksOn(BlueprintAbility spell)
        {
            return spells.Contains(spell);
        }
    }



    [AllowedOn(typeof(BlueprintUnitFact))]
    public class HealingMetamagic : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {
        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartHealingMetamagic>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartHealingMetamagic>().removeBuff(this.Fact);
        }

        public virtual bool worksOnMaximize(BlueprintAbility spell, UnitEntityData caster, UnitEntityData target)
        {
            return false;
        }

        public virtual bool worksOnEmpower(BlueprintAbility spell, UnitEntityData caster, UnitEntityData target)
        {
            return false;
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class SelfHealingMetamagic : HealingMetamagic
    {
        public BlueprintAbility[] spells;
        public bool empower = false;
        public bool maximize = false;

        public override bool worksOnMaximize(BlueprintAbility spell, UnitEntityData caster, UnitEntityData target)
        {
            return maximize && spells.Contains(spell) && caster == target;
        }

        public override bool worksOnEmpower(BlueprintAbility spell, UnitEntityData caster, UnitEntityData target)
        {
            return empower && spells.Contains(spell) && caster == target;
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class UndeadHealingMetamagic : HealingMetamagic
    {
        public bool empower = false;
        public bool maximize = false;

        public override bool worksOnMaximize(BlueprintAbility spell, UnitEntityData caster, UnitEntityData target)
        {
            return maximize && target.Descriptor.IsUndead;
        }

        public override bool worksOnEmpower(BlueprintAbility spell, UnitEntityData caster, UnitEntityData target)
        {
            return empower && target.Descriptor.IsUndead;
        }
    }





    [AllowedOn(typeof(BlueprintUnitFact))]
    public class ReceiveBonusCasterLevelHealing : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartReceiveBonusCasterLevelHealing>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartReceiveBonusCasterLevelHealing>().removeBuff(this.Fact);
        }
    }


    [Harmony12.HarmonyPatch(typeof(ContextActionHealTarget))]
    [Harmony12.HarmonyPatch("RunAction", Harmony12.MethodType.Normal)]
    [Harmony12.HarmonyPriority(Harmony12.Priority.Last)]
    class Patch_ContextActionHealTarget_RunAction_Prefix
    {
        static public bool Prefix(ContextActionHealTarget __instance)
        {
            Main.TraceLog();
            var tr = Harmony12.Traverse.Create(__instance);
            var context = tr.Property("Context").GetValue<MechanicsContext>();
            var target = tr.Property("Target").GetValue<TargetWrapper>().Unit;

            if (target == null)
                UberDebug.LogError(__instance, (object)"Invalid target for effect '{0}'", __instance.GetType().Name);
            else if (context.MaybeCaster == null)
            {
                UberDebug.LogError(__instance, (object)"Caster is missing", (object[])Array.Empty<object>());
            }
            else
            {
                int bonus = __instance.Value.Calculate(context);
                var bonus_to_dice = __instance.Value.BonusValue.Calculate(context);

                var hp_bonus_to_caster_level = context.MaybeCaster.Get<UnitPartExtendHpBonusToCasterLevel>();
                
                if (hp_bonus_to_caster_level != null && hp_bonus_to_caster_level.active(context.SourceAbility) && context.Params != null)
                {
                    var additional_hp = context.Params.CasterLevel - bonus_to_dice;
                    //Main.logger.Log("Additional Hp: " + additional_hp.ToString());
                    if (additional_hp < 0)
                    {
                        additional_hp = 0;
                    }

                    if (context.HasMetamagic(Metamagic.Empower))
                    {
                        additional_hp += additional_hp / 2;
                    }

                    bonus_to_dice += additional_hp;
                    bonus += additional_hp;
                }

                if (!context.HasMetamagic(Metamagic.Maximize) && context.MaybeCaster.Ensure<UnitPartHealingMetamagic>().hasMaximize(context.SourceAbility, context.MaybeCaster, target))
                {
                    
                    bonus = (int)__instance.Value.DiceType * __instance.Value.DiceCountValue.Calculate(context) + bonus_to_dice;

                    if (context.HasMetamagic(Metamagic.Empower))
                    {
                        bonus = (int)(bonus * 1.5f);
                    }
                }

                if (!context.HasMetamagic(Metamagic.Empower) && context.MaybeCaster.Ensure<UnitPartHealingMetamagic>().hasEmpower(context.SourceAbility, context.MaybeCaster, target))
                {
                    bonus = (int)(bonus * 1.5f);
                }

                int dice_count = __instance.Value.DiceCountValue.Calculate(context);
                bonus += target.Descriptor.Ensure<UnitPartReceiveBonusHpPerDie>().getAmount(context.SourceAbility) * dice_count;
                var extra_bonus = (context.MaybeCaster?.Get<UnitPartBonusHealing>()?.getAmount(context.SourceAbility)).GetValueOrDefault();
                bonus += extra_bonus;

                if (target.Descriptor.Ensure<UnitPartReceiveBonusCasterLevelHealing>().active() && context.SourceAbility != null && __instance.Value != null)
                {
                    var dice = __instance.Value.DiceType;
                    if (dice_count != 0 && dice != DiceType.Zero && context.Params != null)
                    {
                        bonus += context.Params.CasterLevel;
                    }
                }


                if (target.Get<HarmlessSaves.UnitPartSaveAgainstHarmlessSpells>() != null 
                    && context.SourceAbility?.GetComponent<HarmlessSaves.HarmlessHealSpell>() != null 
                    && context.SourceAbility.IsSpell
                    && target.IsAlly(context.MaybeCaster) 
                    && target != context.MaybeCaster)
                {
                    RuleSavingThrow ruleSavingThrow = new RuleSavingThrow(target, context.AssociatedBlueprint.GetComponent<HarmlessSaves.HarmlessHealSpell>().save_type, context.Params.DC);
                    ruleSavingThrow.Reason = (RuleReason)((MechanicsContext)context);
                    if (context.TriggerRule<RuleSavingThrow>(ruleSavingThrow).IsPassed)
                    {
                        bonus /= 2;
                    }
                }

                if (bonus > 0)
                {
                    context.TriggerRule<RuleHealDamage>(new RuleHealDamage(context.MaybeCaster, target, DiceFormula.Zero, bonus));
                }
            }

            return false;
        }
    }


    [Harmony12.HarmonyPatch(typeof(ContextActionBreathOfLife))]
    [Harmony12.HarmonyPatch("RunAction", Harmony12.MethodType.Normal)]
    [Harmony12.HarmonyPriority(100)]
    class Patch_ContextActionBreathOfLife_RunAction_Prefix
    {
        static public void Postfix(ContextActionBreathOfLife __instance)
        {
            Main.TraceLog();
            var tr = Harmony12.Traverse.Create(__instance);
            var context = tr.Property("Context").GetValue<MechanicsContext>();
            var target = tr.Property("Target").GetValue<TargetWrapper>().Unit;

            bool was_dead = false;
            if (target.HPLeft <= -(int)((ModifiableValue)target.Stats.Constitution))
            {
                was_dead = true;
            }

            int dice_count = __instance.Value.DiceCountValue.Calculate(context);
            int bonus_hp = target.Descriptor.Ensure<UnitPartReceiveBonusHpPerDie>().getAmount(context.SourceAbility) * dice_count;
            
            if (target.Descriptor.Ensure<UnitPartReceiveBonusCasterLevelHealing>().active() && context.SourceAbility != null && __instance.Value != null)
            {
                var dice = __instance.Value.DiceType;
                if (dice_count == 0 || dice == DiceType.Zero || context.Params == null)
                {
                    return;
                }
                bonus_hp += context.Params.CasterLevel;
                
            }

            if (target.Get<HarmlessSaves.UnitPartSaveAgainstHarmlessSpells>() != null
                && context.SourceAbility?.GetComponent<HarmlessSaves.HarmlessHealSpell>() != null
                && context.SourceAbility.IsSpell
                && target.IsAlly(context.MaybeCaster)
                && target != context.MaybeCaster)
            {
                RuleSavingThrow ruleSavingThrow = new RuleSavingThrow(target, context.AssociatedBlueprint.GetComponent<HarmlessSaves.HarmlessHealSpell>().save_type, context.Params.DC);
                ruleSavingThrow.Reason = (RuleReason)((MechanicsContext)context);
                if (context.TriggerRule<RuleSavingThrow>(ruleSavingThrow).IsPassed)
                {
                    bonus_hp /= 2;
                }
            }

            if (bonus_hp > 0)
            {
                context.TriggerRule<RuleHealDamage>(new RuleHealDamage(target, target, DiceFormula.Zero, bonus_hp));
            }
            

            if (target.HPLeft > -(int)((ModifiableValue)target.Stats.Constitution) && was_dead)
            {
                target.Descriptor.Resurrect(0.0f, false);
            }
        }
    }


    public class ContextActionTransferDamageToCaster : BuffAction
    {
        public ContextValue Value;

        public override string GetCaption() => $"Transfer {Value} of hit point damage to caster.";

        public override void RunAction()
        {
            int value = Value.Calculate(Context);
            var caster = Context.MaybeCaster;
            // Prevent the damage transfer from killing the caster, and automatically end the Life Link.
            // (Makes it easier to manage, since this is a CRPG.)
            if (caster.HPLeft < value)
            {
               // Log.Write($"Can't transfer {value} damage, caster only has {caster.HPLeft} HP.");
                Buff.RemoveAfterDelay();
                return;
            }
            var target = Buff.Owner.Unit;
            var rule = Context.TriggerRule(new RuleHealDamage(caster, target, DiceFormula.Zero, value));
            if (rule.Value > 0)
            {
                Context.TriggerRule(new RuleDealDamage(caster, caster, new DamageBundle(
                    new EnergyDamage(new DiceFormula(rule.Value, DiceType.One), DamageEnergyType.Holy))));
            }
            //Log.Write($"Transfer {rule.Value} damage from {target.CharacterName} to {caster.CharacterName}");
        }
    }



    public class HealSkillBonus : RuleInitiatorLogicComponent<RuleDispelMagic>
    {
        public ContextValue value;
        public override void OnEventAboutToTrigger(RuleDispelMagic evt)
        {
            var bonus = value.Calculate(this.Fact.MaybeContext);
            if (evt.Check == RuleDispelMagic.CheckType.SkillDC && evt.Skill == StatType.SkillLoreReligion)
            {
                evt.AddTemporaryModifier(Owner.Stats.SkillLoreReligion.AddModifier(bonus, this, ModifierDescriptor.UntypedStackable));
            }
        }

        public override void OnEventDidTrigger(RuleDispelMagic evt)
        {

        }
    }


    public class StatsCannotBeReducedBelow1 : RuleTargetLogicComponent<RuleDealStatDamage>
    {
        bool wasMarkedForDeath;

        public override void OnEventAboutToTrigger(RuleDealStatDamage evt)
        {
            wasMarkedForDeath = evt.Target.Descriptor.State.MarkedForDeath;
        }

        public override void OnEventDidTrigger(RuleDealStatDamage evt)
        {
            var stat = evt.Stat;
            var value = stat.ModifiedValueRaw;
            if (value < 1)
            {
                evt.Target.Descriptor.State.MarkedForDeath = wasMarkedForDeath;
                var adjust = 1 - value; // bring the value back to 1.
                if (evt.IsDrain)
                {
                    stat.Drain -= adjust;
                }
                else
                {
                    stat.Damage -= adjust;
                }
            }
        }
    }

    public class HealingWithOverflowToTemporaryHp : RuleInitiatorLogicComponent<RuleHealDamage>
    {
        public BlueprintBuff temporary_hp_buff;
        public AbilitySharedValue shared_value_type;
        public ContextValue duration;
        public ContextValue max_hp;

        public override void OnEventAboutToTrigger(RuleHealDamage evt)
        {
            var context = Helpers.GetMechanicsContext()?.SourceAbilityContext;
            var spell = context?.SourceAbility;

            if (spell != null && spell.Type == AbilityType.Spell &&
                (spell.SpellDescriptor & SpellDescriptor.RestoreHP) != 0)
            {
                int temporary_hp = evt.Bonus - evt.Target.Damage;
                if (temporary_hp > 0)
                {
                    //Main.logger.Log(temporary_hp.ToString());
                    context[shared_value_type] = Math.Min(temporary_hp, max_hp.Calculate(this.Fact.MaybeContext));
                    //TemporaryHpBonusInternal.value = Math.Min(temporary_hp, max_hp.Calculate(this.Fact.MaybeContext));
                    var duration_seconds = duration.Calculate(this.Fact.MaybeContext).Rounds().Seconds;
                    evt.Target.Buffs.AddBuff(temporary_hp_buff, context, duration_seconds);
                }
            }
        }

        public override void OnEventDidTrigger(RuleHealDamage evt)
        {

        }
    }

    //additional healing action that does not take into account any bonuses (can be used for hp transfer) some special healing cases which should not benefit from bonuses
    public class ContextActionHealTargetNoBonus : ContextAction
    {
        public ContextDiceValue Value;

        public override string GetCaption()
        {
            return string.Format("Heal {0} of hit point damage", (object)this.Value);
        }

        public override void RunAction()
        {
            if (this.Target.Unit == null)
                UberDebug.LogError((UnityEngine.Object)this, (object)"Invalid target for effect '{0}'", (object)this.GetType().Name);
            else if (this.Context.MaybeCaster == null)
            {
                UberDebug.LogError((UnityEngine.Object)this, (object)"Caster is missing", (object[])Array.Empty<object>());
            }
            else
            {
                int bonus = this.Value.Calculate(this.Context);
                this.Context.TriggerRule<RuleHealDamage>(new RuleHealDamage(this.Context.MaybeCaster, this.Target.Unit, DiceFormula.Zero, bonus));
            }
        }
    }

    public class ContextActionTreatDeadlyWounds : ContextAction
    {
        public ContextValue Value;
        public StatType[] stats_to_heal = new StatType[] {StatType.Strength, StatType.Dexterity, StatType.Constitution, StatType.Intelligence, StatType.Wisdom, StatType.Charisma };
        public bool multiply_by_hd = true;
        public override string GetCaption()
        {
            return string.Format("Heal {0} of hit point damage", (object)this.Value);
        }

        public override void RunAction()
        {
            if (this.Target.Unit == null)
                UberDebug.LogError((UnityEngine.Object)this, (object)"Invalid target for effect '{0}'", (object)this.GetType().Name);
            else if (this.Context.MaybeCaster == null)
            {
                UberDebug.LogError((UnityEngine.Object)this, (object)"Caster is missing", (object[])Array.Empty<object>());
            }
            else
            {
                int bonus = this.Value.Calculate(this.Context);
                int hps = bonus;
                if (multiply_by_hd)
                {
                    hps = bonus * this.Target.Unit.Descriptor.Progression.CharacterLevel;
                }

                this.Context.TriggerRule<RuleHealDamage>(new RuleHealDamage(this.Context.MaybeCaster, this.Target.Unit, DiceFormula.Zero, hps));

                foreach (var s in stats_to_heal)
                {
                    this.Context.TriggerRule<RuleHealStatDamage>(new RuleHealStatDamage(this.Context.MaybeCaster, this.Target.Unit, s, bonus));
                }
            }
        }
    }


    public class OnHealingReceivedActionTrigger: OwnedGameLogicComponent<UnitDescriptor>, IHealingHandler
    {
        public ActionList actions;
        public int min_amount = 0;

        public void HandleHealing(UnitEntityData source, UnitEntityData target, int amount)
        {
            if (target != this.Owner.Unit)
            {
                return;
            }
            if (amount < min_amount)
            {
                return;
            }
            (this.Fact as IFactContextOwner).RunActionInContext(actions, target);
        }
    }

    /*public class TemporaryHpBonusInternal : BuffLogic, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ITargetRulebookSubscriber
    {
        public ModifierDescriptor Descriptor;
        public static int value;
        public bool RemoveWhenHitPointsEnd;
        [JsonProperty]
        private ModifiableValue.Modifier m_Modifier;

        public override void OnFactActivate()
        {
            this.m_Modifier = this.Owner.Stats.TemporaryHitPoints.AddModifier(value, (GameLogicComponent)this, this.Descriptor);
        }

        public override void OnFactDeactivate()
        {
            if (this.m_Modifier != null)
                this.m_Modifier.Remove();
            this.m_Modifier = (ModifiableValue.Modifier)null;
        }

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            if (!this.RemoveWhenHitPointsEnd || this.m_Modifier.AppliedTo != null)
                return;
            this.Buff.Remove();
        }
    }*/
}
