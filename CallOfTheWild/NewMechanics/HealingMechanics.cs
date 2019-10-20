using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Actions;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
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


    public class UnitPartSelfHealingMetamagic : AdditiveUnitPart
    {
        public bool hasEmpower(BlueprintAbility spell)
        {
            foreach (var b in buffs)
            {
                var comp = b.Blueprint.GetComponent<SelfHealingMetamagic>();
                if (comp == null)
                {
                    continue;
                }
                if (comp.empower && comp.worksOn(spell))
                {
                    return true;
                }
            }

            return false;
        }

        public bool hasMaximize(BlueprintAbility spell)
        {
            foreach (var b in buffs)
            {
                var comp = b.Blueprint.GetComponent<SelfHealingMetamagic>();
                if (comp == null)
                {
                    continue;
                }
                
                if (comp.maximize && comp.worksOn(spell))
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



    [AllowedOn(typeof(BlueprintUnitFact))]
    public class ExtendHpBonusToCasterLevel : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {
        public BlueprintAbility[] spells;

        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartExtendHpBonusToCasterLevel>().addBuff(this.Fact);
        }


        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartExtendHpBonusToCasterLevel>().removeBuff(this.Fact);
        }

        public bool worksOn(BlueprintAbility spell)
        {
            return spells.Contains(spell);
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class SelfHealingMetamagic : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {
        public BlueprintAbility[] spells;
        public bool empower = false;
        public bool maximize = false;

        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartSelfHealingMetamagic>().addBuff(this.Fact);
        }


        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartSelfHealingMetamagic>().removeBuff(this.Fact);
        }

        public bool worksOn(BlueprintAbility spell)
        {
            return spells.Contains(spell);
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
    class Patch_ContextActionHealTarget_RunAction_Postfix
    {

        static public bool Prefix(ContextActionHealTarget __instance)
        {
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

                var hp_bonus_to_caster_level = target.Get<UnitPartExtendHpBonusToCasterLevel>();
                if (hp_bonus_to_caster_level != null && hp_bonus_to_caster_level.active(context.SourceAbility) && context.Params != null)
                {
                    var additional_hp = context.Params.CasterLevel - bonus_to_dice;
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

                if (!context.HasMetamagic(Metamagic.Maximize) && target.Ensure<UnitPartSelfHealingMetamagic>().hasMaximize(context.SourceAbility) && target == context.MaybeCaster)
                {
                    
                    bonus = (int)__instance.Value.DiceType * __instance.Value.DiceCountValue.Calculate(context) + bonus_to_dice;

                    if (context.HasMetamagic(Metamagic.Empower))
                    {
                        bonus = (int)(bonus * 1.5f);
                    }
                }

                if (!context.HasMetamagic(Metamagic.Empower) && target.Ensure<UnitPartSelfHealingMetamagic>().hasEmpower(context.SourceAbility) && target == context.MaybeCaster)
                {
                    bonus = (int)(bonus * 1.5f);
                }

                context.TriggerRule<RuleHealDamage>(new RuleHealDamage(context.MaybeCaster, target, DiceFormula.Zero, bonus));

                if (target.Descriptor.Ensure<UnitPartReceiveBonusCasterLevelHealing>().active() && context.SourceAbility != null && __instance.Value != null)
                {
                    int dice_count = __instance.Value.DiceCountValue.Calculate(context);
                    var dice = __instance.Value.DiceType;
                    if (dice_count == 0 || dice == DiceType.Zero || context.Params == null)
                    {
                        return false;
                    }
                    int bonus_hp = context.Params.CasterLevel;
                    context.TriggerRule<RuleHealDamage>(new RuleHealDamage(target, target, DiceFormula.Zero, bonus_hp));
                }
            }

            return false;
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
}
