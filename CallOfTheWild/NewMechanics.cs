using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.Visual.HitSystem;
using System;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Newtonsoft.Json;
using Kingmaker.Utility;
using Kingmaker.UI.GenericSlot;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.EntitySystem.Entities;
using System.Collections.Generic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.Blueprints.Validation;

namespace CallOfTheWild
{
    namespace NewMechanics
    {
        [ComponentName("Apply metamagic for resource")]
        [AllowedOn(typeof(Kingmaker.Blueprints.Facts.BlueprintUnitFact))]
        public class MetaRage : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            public Metamagic metamagic;
            public BlueprintAbilityResource resource;

            public MetaRage(Metamagic metamagic_to_apply, BlueprintAbilityResource resource_to_use)
            {
                metamagic = metamagic_to_apply;
                resource = resource_to_use;
            }
            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                bool is_metamagic_available = ((evt.Spell.AvailableMetamagic & metamagic) != 0);
                if (evt.Spell == null || evt.Spellbook == null || evt.Spell.Type != AbilityType.Spell || !is_metamagic_available)
                {
                    return;
                }

                int resource_needed = 2 * (evt.Spellbook.GetSpellLevel(evt.Spell) + MetamagicHelper.DefaultCost(metamagic));
                Main.logger.Log(resource_needed.ToString());
                if (this.resource == null || this.Owner.Resources.GetResourceAmount((BlueprintScriptableObject)this.resource) < resource_needed)
                {
                    return;
                }

                evt.AddMetamagic(this.metamagic);
                this.Owner.Resources.Spend((BlueprintScriptableObject)this.resource, resource_needed);
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }


        [ComponentName("Increase spell descriptor DC for 1d6 damage")]
        [AllowedOn(typeof(Kingmaker.Blueprints.Facts.BlueprintUnitFact))]
        public class RageCasting : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            public int BonusDC;
            private int actual_dc = 0;

            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                actual_dc = 0;
                bool no_save = evt.Spell.EffectOnEnemy != AbilityEffectOnUnit.Harmful; //TODO: properly check for saving throw
                if (evt.Spell == null || evt.Spellbook == null || evt.Spell.Type != AbilityType.Spell || no_save)
                {
                    return;
                }
                actual_dc = Mathf.Min(evt.Spellbook.GetSpellLevel(evt.Spell), BonusDC);
                evt.AddBonusDC(actual_dc);
                Common.AddBattleLogMessage($"{Owner.CharacterName}: Rage Casting increases spell level by {actual_dc}");
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
                if (actual_dc == 0)
                {
                    return;
                }
                RuleDealDamage evt_dmg = new RuleDealDamage(this.Owner.Unit, this.Owner.Unit, new DamageBundle(new BaseDamage[1]
                {
                (BaseDamage) new EnergyDamage(new DiceFormula(actual_dc, DiceType.D6), Kingmaker.Enums.Damage.DamageEnergyType.Holy)
                }));
                evt_dmg.Reason = (RuleReason)this.Fact;
                //temporary remove temp hp
                var temp_hd_modifiers = this.Owner.Stats.TemporaryHitPoints.Modifiers.ToArray();
                foreach (var m in temp_hd_modifiers)
                {
                    this.Owner.Stats.TemporaryHitPoints.RemoveModifier(m);
                }
                this.Owner.Stats.TemporaryHitPoints.UpdateValue();
                Rulebook.Trigger<RuleDealDamage>(evt_dmg);
                if (this.Owner.HPLeft <= 0)
                { //do not give hp back if owner is unconscious
                    return;
                }
                foreach (var m in temp_hd_modifiers)
                {
                    this.Owner.Stats.TemporaryHitPoints.AddModifier(m.ModValue, m.Source, m.SourceComponent, m.ModDescriptor);
                }
                this.Owner.Stats.TemporaryHitPoints.UpdateValue();
            }
        }


        [ComponentName("increase caster level by value and apply on caster debuff for duration equal to rate*spell_level if it fails saving throw against (dc_base + spell_level + caster_level_increase)")]
        [AllowedOn(typeof(Kingmaker.Blueprints.Facts.BlueprintUnitFact))]
        public class ConduitSurge : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            public BlueprintBuff buff;
            public DurationRate rate = DurationRate.Rounds;
            public ContextDiceValue dice_value;
            public SavingThrowType save_type;
            public int dc_base = 10;
            public string display_name = "Conduit Surge";
            public BlueprintAbilityResource resource;
            private int caster_level_increase;


            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                caster_level_increase = -1;
                if (evt.Spell == null || evt.Spellbook == null || evt.Spell.Type != AbilityType.Spell)
                {
                    return;
                }
                caster_level_increase = dice_value.Calculate(this.Fact.MaybeContext);
                evt.AddBonusCasterLevel(caster_level_increase);
                Common.AddBattleLogMessage($"{Owner.CharacterName}: {display_name} increases caster level by {caster_level_increase}");
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
                if (caster_level_increase == -1)
                {
                    return;
                }
                RuleSavingThrow ruleSavingThrow = this.Fact.MaybeContext.TriggerRule<RuleSavingThrow>(new RuleSavingThrow(this.Owner.Unit, save_type, dc_base + evt.SpellLevel + caster_level_increase));
                if (!ruleSavingThrow.IsPassed)
                {
                    this.Owner.Buffs.AddBuff(buff, this.Owner.Unit, (rate.ToRounds() * evt.SpellLevel).Seconds);
                }
                if (resource != null)
                {
                    this.Owner.Resources.Spend((BlueprintScriptableObject)this.resource, 1);
                }
            }
        }


        [AllowedOn(typeof(BlueprintBuff))]
        [ComponentName("Buffs/AddEffect/ContextFastHealing")]
        public class AddContextEffectFastHealing : BuffLogic, ITickEachRound, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ITargetRulebookSubscriber
        {
            public int Multiplier = 1;
            public ContextValue Value;

            public void OnNewRound()
            {
                int heal_amount = this.Value.Calculate(this.Context);
                if (this.Owner.State.IsDead || this.Owner.Damage <= 0)
                {
                    return;
                }
                GameHelper.HealDamage(this.Owner.Unit, this.Owner.Unit, heal_amount * Multiplier);
            }

            public void OnEventAboutToTrigger(RuleDealDamage evt)
            {
            }

            public void OnEventDidTrigger(RuleDealDamage evt)
            {
            }
        }


        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintUnitFact))]
        [ComponentName("Saving throw bonus against allies")]
        public class SavingThrowBonusAgainstAllies : RuleInitiatorLogicComponent<RuleSavingThrow>
        {
            public ModifierDescriptor Descriptor;
            public int Value;
            
            public override void OnEventAboutToTrigger(RuleSavingThrow evt)
            {
                var caster = evt.Reason.Caster;
                if (caster == null || caster.IsPlayersEnemy)
                    return;
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveWill.AddModifier(this.Value * this.Fact.GetRank(), (GameLogicComponent)this, this.Descriptor));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveReflex.AddModifier(this.Value * this.Fact.GetRank(), (GameLogicComponent)this, this.Descriptor));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveFortitude.AddModifier(this.Value * this.Fact.GetRank(), (GameLogicComponent)this, this.Descriptor));
            }

            public override void OnEventDidTrigger(RuleSavingThrow evt)
            {
            }
        }


        public class BuffContextEnchantShield : BuffLogic
        {
            public BlueprintArmorEnchantment[] enchantments;
            public ContextValue value;
            [JsonProperty]
            private ItemEnchantment m_Enchantment;
            private ItemEntityShield m_Shield;

            public override void OnFactActivate()
            {
                var unit = this.Owner;
                if (unit == null) return;

                var shield = unit.Body.SecondaryHand.MaybeShield;
                if (shield == null)
                {
                    return;
                }

                int bonus = value.Calculate(Context) - 1;
                if (bonus < 0)
                {
                    bonus = 0;
                }
                if (bonus >= enchantments.Length)
                {
                    bonus = enchantments.Length - 1;
                }

                var fact = shield.Enchantments.Find(x => x.Blueprint == enchantments[bonus]);
                if (fact != null && fact.IsTemporary)
                {
                    shield.RemoveEnchantment(fact);
                }
                m_Enchantment = shield.ArmorComponent.AddEnchantment(enchantments[bonus], Context, new Rounds?());
                shield.ArmorComponent.RecalculateStats();
                m_Shield = shield;
            }

            public override void OnFactDeactivate()
            {
                if (this.m_Enchantment == null)
                    return;
                this.m_Enchantment.Owner?.RemoveEnchantment(this.m_Enchantment);
                if (m_Shield != null)
                {
                    m_Shield.ArmorComponent.RecalculateStats();
                }
            }
        }


        public class BuffContextEnchantArmor : BuffLogic
        {
            public BlueprintArmorEnchantment[] enchantments;
            public ContextValue value;
            [JsonProperty]
            private ItemEnchantment m_Enchantment;
            private ItemEntityArmor m_Armor;

            public override void OnFactActivate()
            {
                var unit = this.Owner;
                if (unit == null) return;

                var armor = unit.Body.Armor.MaybeArmor;
                if (armor == null) return;

              
                int bonus = value.Calculate(Context) - 1;
                if (bonus <0)
                {
                    bonus = 0;
                }
                if (bonus >= enchantments.Length)
                {
                    bonus = enchantments.Length - 1;
                }

                var fact = armor.Enchantments.Find(x => x.Blueprint == enchantments[bonus]);
                if (fact != null && fact.IsTemporary)
                {
                    armor.RemoveEnchantment(fact);
                }
                m_Enchantment = armor.AddEnchantment(enchantments[bonus], Context, new Rounds?());
                armor.RecalculateStats();
                m_Armor = armor;
            }

            public override void OnFactDeactivate()
            {
                if (this.m_Enchantment == null)
                    return;
                this.m_Enchantment.Owner?.RemoveEnchantment(this.m_Enchantment);
                if (m_Armor != null)
                {
                    m_Armor.RecalculateStats();
                }
            }
        }


        [AllowMultipleComponents]
        [ComponentName("Predicates/Target has fact unless alternative")]
        [AllowedOn(typeof(BlueprintAbility))]
        public class AbilityTargetHasNoFactUnlessBuffsFromCaster : BlueprintComponent, IAbilityTargetChecker
        {
            public BlueprintBuff[] CheckedBuffs;
            public BlueprintBuff[] AlternativeBuffs;


            public bool CanTarget(UnitEntityData caster, TargetWrapper target)
            {
                UnitEntityData unit = target.Unit;
                if (unit == null)
                    return false;
                bool flag1 = false;

                foreach (var CheckedBuff in this.CheckedBuffs)
                {
                    foreach (var b in unit.Descriptor.Buffs)
                    {
                        flag1 = (b.Blueprint == CheckedBuff) && (b.MaybeContext.MaybeCaster == caster);
                        if (flag1) break;
                    }
                    if (flag1) break;
                }

                bool flag2 = false;
                foreach (var AlternativeBuff in this.AlternativeBuffs)
                {
                    foreach (var b in unit.Descriptor.Buffs)
                    {
                        flag2 = (b.Blueprint == AlternativeBuff) && (b.MaybeContext.MaybeCaster == caster);
                        if (flag2) break;
                    }
                    if (flag2) break;
                }

                if (flag1)
                {
                    return flag2;
                }
                return true;
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class SavingThrowBonusAgainstSpecificSpells : RuleInitiatorLogicComponent<RuleSavingThrow>
        {
            public BlueprintAbility[] Spells;
            public ModifierDescriptor ModifierDescriptor;
            public int Value;
            public BlueprintUnitFact[] BypassFeatures;

            public override void OnEventAboutToTrigger(RuleSavingThrow evt)
            {
               
                BlueprintAbility sourceAbility = evt.Reason.Context?.SourceAbility;
                UnitEntityData maybeCaster = evt.Reason.Context?.MaybeCaster;
                bool flag = maybeCaster != null;
                if (flag)
                {
                    flag = false;
                    foreach (BlueprintUnitFact bypassFeature in this.BypassFeatures)
                        flag = maybeCaster.Descriptor.HasFact(bypassFeature);
                }
                if (!(sourceAbility != null) || !((IEnumerable<BlueprintAbility>)this.Spells).Contains<BlueprintAbility>(sourceAbility) || flag)
                    return;

                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveWill.AddModifier(this.Value, (GameLogicComponent)this, this.ModifierDescriptor));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveReflex.AddModifier(this.Value, (GameLogicComponent)this, this.ModifierDescriptor));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveFortitude.AddModifier(this.Value, (GameLogicComponent)this, this.ModifierDescriptor));
            }

            public override void OnEventDidTrigger(RuleSavingThrow evt)
            {
            }
        }


        public class ContextCalculateAbilityParamsBasedOnClasses : ContextAbilityParamsCalculator
        {
            public StatType StatType = StatType.Charisma;
            public BlueprintCharacterClass[] CharacterClasses;

            public override AbilityParams Calculate(MechanicsContext context)
            {
                UnitEntityData maybeCaster = context.MaybeCaster;
                if (maybeCaster == null)
                {
                    return context.Params;
                }
                StatType statType = this.StatType;

                AbilityData ability = context.SourceAbilityContext?.Ability;
                RuleCalculateAbilityParams rule = !(ability != (AbilityData)null) ? new RuleCalculateAbilityParams(maybeCaster, context.AssociatedBlueprint, (Spellbook)null) : new RuleCalculateAbilityParams(maybeCaster, ability);
                rule.ReplaceStat = new StatType?(statType);

                int class_level = 0;
                foreach (var c in this.CharacterClasses)
                {
                    class_level += maybeCaster.Descriptor.Progression.GetClassLevel(c);
                }
                rule.ReplaceCasterLevel = new int?(class_level);
                rule.ReplaceSpellLevel = new int?(class_level / 2);
                return context.TriggerRule<RuleCalculateAbilityParams>(rule).Result;
            }

            public override void Validate(ValidationContext context)
            {
                base.Validate(context);
                if (this.StatType.IsAttribute() || this.StatType == StatType.BaseAttackBonus)
                    return;
                string str = string.Join(", ", ((IEnumerable<StatType>)StatTypeHelper.Attributes).Select<StatType, string>((Func<StatType, string>)(s => s.ToString())));
                context.AddError("StatType must be Base Attack Bonus or an attribute: {0}", (object)str);
            }
        }


    }
}
