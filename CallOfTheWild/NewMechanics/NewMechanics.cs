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
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.ElementsSystem;
using Kingmaker.Controllers;
using Kingmaker;
using static Kingmaker.UnitLogic.Abilities.Components.AbilityCustomMeleeAttack;
using Kingmaker.UnitLogic.Mechanics.ContextData;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.EntitySystem;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.EntitySystem.Persistence.Versioning;
using JetBrains.Annotations;
using Kingmaker.Enums.Damage;
using Kingmaker.Inspect;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Blueprints.Area;
using Kingmaker.Items.Slots;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.AreaLogic.Cutscenes.Commands;
using Pathfinding;
using Kingmaker.Controllers.Combat;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;

namespace CallOfTheWild
{
    namespace NewMechanics
    {




        [ComponentName("Increase spell descriptor DC by spell level up to BonusDC and then deals dc_increase d6 damage")]
        [AllowedOn(typeof(Kingmaker.Blueprints.Facts.BlueprintUnitFact))]
        public class RageCasting : RuleInitiatorLogicComponent<RuleCastSpell>, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>
        {
            public int BonusDC;
            private int actual_dc = 0;
            public BlueprintAbility current_spell = null;

            public override void OnEventAboutToTrigger(RuleCastSpell evt)
            {

                if (evt.Spell.SourceItem != null || evt.Spell.Blueprint != current_spell)
                {
                    actual_dc = 0;
                    return;
                }
            }

            public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {

            }


            public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                actual_dc = 0;
                bool no_save = evt.Spell.EffectOnEnemy != AbilityEffectOnUnit.Harmful; //TODO: properly check for saving throw
                if (evt.Spell == null || evt.Spellbook == null || evt.Spell.Type != AbilityType.Spell || no_save)
                {
                    return;
                }
                actual_dc = Mathf.Min(evt.SpellLevel, BonusDC);
                evt.AddBonusDC(actual_dc);
                current_spell = evt.Spell;
            }

            public override void OnEventDidTrigger(RuleCastSpell evt)
            {
                if (actual_dc == 0)
                {
                    return;
                }
                Common.AddBattleLogMessage($"{Owner.CharacterName}: Rage Casting increases spell DC by {actual_dc}");
                RuleDealDamage evt_dmg = new RuleDealDamage(this.Owner.Unit, this.Owner.Unit, new DamageBundle(new BaseDamage[1]
                {
                (BaseDamage) new EnergyDamage(new DiceFormula(actual_dc, DiceType.D6), Kingmaker.Enums.Damage.DamageEnergyType.Holy)
                }));
                actual_dc = 0;
                evt_dmg.Reason = (RuleReason)this.Fact;
                //temporary remove temp hp
                var temp_hp_modifiers = this.Owner.Stats.TemporaryHitPoints.Modifiers.ToArray();
                foreach (var m in temp_hp_modifiers)
                {
                    this.Owner.Stats.TemporaryHitPoints.RemoveModifier(m);
                }
                this.Owner.Stats.TemporaryHitPoints.UpdateValue();
                Rulebook.Trigger<RuleDealDamage>(evt_dmg);
                if (this.Owner.HPLeft <= 0)
                { //do not give hp back if owner is unconscious
                    return;
                }
                foreach (var m in temp_hp_modifiers)
                {
                    this.Owner.Stats.TemporaryHitPoints.AddModifier(m.ModValue, m.Source, m.SourceComponent, m.ModDescriptor);
                }
                this.Owner.Stats.TemporaryHitPoints.UpdateValue();
                current_spell = null;
            }
        }


        [ComponentName("Increase caster level by value and apply on caster debuff for duration equal to rate*spell_level if it fails saving throw against (dc_base + spell_level + caster_level_increase)")]
        [AllowedOn(typeof(Kingmaker.Blueprints.Facts.BlueprintUnitFact))]
        public class ConduitSurge : RuleInitiatorLogicComponent<RuleCastSpell>, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>
        {
            public BlueprintBuff buff;
            public DurationRate rate = DurationRate.Rounds;
            public ContextDiceValue dice_value;
            public SavingThrowType save_type;
            public int dc_base = 10;
            public string display_name = "Conduit Surge";
            public BlueprintAbilityResource resource;
            private int caster_level_increase = -1;
            public BlueprintAbility current_spell = null;


            public override void OnEventAboutToTrigger(RuleCastSpell evt)
            {
                if (evt.Spell.SourceItem != null || evt.Spell.Blueprint != current_spell)
                {
                    caster_level_increase = -1;
                    return;
                }
            }


            public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                caster_level_increase = -1;
                if (evt.Spell == null || evt.Spellbook == null || evt.Spell.Type != AbilityType.Spell)
                {
                    return;
                }
                caster_level_increase = dice_value.Calculate(this.Fact.MaybeContext);
                evt.AddBonusCasterLevel(caster_level_increase);
                current_spell = evt.Spell;
            }

            public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {

            }

            public override void OnEventDidTrigger(RuleCastSpell evt)
            {
                if (caster_level_increase == -1)
                {
                    return;
                }
                Common.AddBattleLogMessage($"{Owner.CharacterName}: {display_name} increases caster level by {caster_level_increase}");
                RuleSavingThrow ruleSavingThrow = this.Fact.MaybeContext.TriggerRule<RuleSavingThrow>(new RuleSavingThrow(this.Owner.Unit, save_type, dc_base + evt.Spell.SpellLevel + caster_level_increase));
                if (!ruleSavingThrow.IsPassed)
                {
                    this.Owner.Buffs.AddBuff(buff, this.Owner.Unit, (rate.ToRounds() * evt.Spell.SpellLevel).Seconds);
                }
                if (resource != null)
                {
                    this.Owner.Resources.Spend((BlueprintScriptableObject)this.resource, 1);
                }
                caster_level_increase = -1;
                current_spell = null;
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



        [AllowMultipleComponents]
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


        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintAbility))]
        public class AbilityTargetHasBuffFromCaster : BlueprintComponent, IAbilityTargetChecker
        {
            public BlueprintBuff[] Buffs;
            public bool not = false;


            public bool CanTarget(UnitEntityData caster, TargetWrapper target)
            {
                UnitEntityData unit = target.Unit;
                if (unit == null)
                    return false;

                foreach (var cb in this.Buffs)
                {
                    foreach (var b in unit.Descriptor.Buffs)
                    {
                        if (cb == b.Blueprint && (b.MaybeContext?.MaybeCaster == caster))
                        {
                            return !not ? true : false;
                        }
                    }
                }

                return not ? true : false;
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilitTargetMainWeaponCheck : BlueprintComponent, IAbilityTargetChecker
        {
            public WeaponCategory[] Category;

            public bool CanTarget(UnitEntityData caster, TargetWrapper target)
            {
                UnitEntityData unit = target.Unit;
                if (unit == null)
                    return false;

                if (unit.Body.PrimaryHand.HasWeapon)
                    return ((IEnumerable<WeaponCategory>)this.Category).Contains<WeaponCategory>(unit.Body.PrimaryHand.Weapon.Blueprint.Type.Category);
                return false;
            }

            public string GetReason()
            {
                return (string)LocalizedTexts.Instance.Reasons.SpecificWeaponRequired;
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilitTargetMainWeaponNonNatural : BlueprintComponent, IAbilityTargetChecker
        {

            public bool CanTarget(UnitEntityData caster, TargetWrapper target)
            {
                UnitEntityData unit = target.Unit;
                if (unit == null)
                    return false;

                if (unit.Body.PrimaryHand.HasWeapon)
                    return !(unit.Body.PrimaryHand.Weapon.Blueprint.Type.IsNatural || unit.Body.PrimaryHand.Weapon.Blueprint.Type.IsUnarmed);
                return false;
            }

            public string GetReason()
            {
                return (string)LocalizedTexts.Instance.Reasons.SpecificWeaponRequired;
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilitTargetHasShield : BlueprintComponent, IAbilityTargetChecker
        {

            public bool CanTarget(UnitEntityData caster, TargetWrapper target)
            {
                UnitEntityData unit = target.Unit;
                if (unit == null)
                    return false;

                return unit.Body.SecondaryHand.HasShield;
            }

            public string GetReason()
            {
                return "Shield Required";
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilitTargetHasArmor : BlueprintComponent, IAbilityTargetChecker
        {

            public bool CanTarget(UnitEntityData caster, TargetWrapper target)
            {
                UnitEntityData unit = target.Unit;
                if (unit == null)
                    return false;

                return unit.Body.Armor.HasArmor;
            }

            public string GetReason()
            {
                return "Armor Required";
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


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ContextSavingThrowBonusAgainstSpecificSpells : RuleInitiatorLogicComponent<RuleSavingThrow>
        {
            public BlueprintAbility[] Spells;
            public ModifierDescriptor ModifierDescriptor;
            public ContextValue Value;
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

                int val = this.Value.Calculate(this.Fact.MaybeContext);
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveWill.AddModifier(val, (GameLogicComponent)this, this.ModifierDescriptor));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveReflex.AddModifier(val, (GameLogicComponent)this, this.ModifierDescriptor));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveFortitude.AddModifier(val, (GameLogicComponent)this, this.ModifierDescriptor));
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



        public class ContextActionResurrectInstant : ContextAction
        {
            public bool FullRestore;
            [HideIf("FullRestore")]
            public float ResultHealth = 0.5f;

            public override string GetCaption()
            {
                return "Resurrect";
            }

            public override void RunAction()
            {
                UnitEntityData unit = this.Target.Unit;
                if (unit != null && this.Context.MaybeCaster != null)
                {
                    UnitEntityData pair = UnitPartDualCompanion.GetPair(unit);
                    if (this.FullRestore)
                    {
                        unit.Descriptor.ResurrectAndFullRestore();
                        pair?.Descriptor.ResurrectAndFullRestore();
                    }
                    else
                    {
                        unit.Descriptor.Resurrect(this.ResultHealth, true);
                        pair?.Descriptor.Resurrect(this.ResultHealth, true);
                    }

                }
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class CrowdAlliesACBonus : RuleTargetLogicComponent<RuleCalculateAC>
        {
            public int num_allies_around;
            public int Radius;
            public ContextValue value;

            public override void OnEventAboutToTrigger(RuleCalculateAC evt)
            {
                int num = 0;
                foreach (UnitEntityData unitEntityData in GameHelper.GetTargetsAround(this.Owner.Unit.Position, (float)this.Radius, true, false))
                {
                    if ((unitEntityData != this.Owner.Unit && !unitEntityData.IsEnemy(this.Owner.Unit)))
                    {
                        num++;
                    }
                }
                if (num < num_allies_around)
                {
                    return;
                }
                var ac_bonus = value.Calculate(this.Fact.MaybeContext);
                evt.AddBonus(ac_bonus, this.Fact);
            }

            public override void OnEventDidTrigger(RuleCalculateAC evt)
            {
            }
        }


        [ComponentName("Increase context spells DC by descriptor")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ContextIncreaseDescriptorSpellsDC : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            public ContextValue Value;
            public SpellDescriptorWrapper Descriptor;

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

            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                bool? nullable = evt.Spell.GetComponent<SpellDescriptorComponent>()?.Descriptor.HasAnyFlag((SpellDescriptor)this.Descriptor);
                if (!nullable.HasValue || !nullable.Value)
                    return;
                evt.AddBonusDC(this.Value.Calculate(this.Context));
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        [ComponentName("Damage bonus for specific weapon types")]
        public class ContextWeaponCategoryDamageBonus : RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
        {
            public WeaponCategory[] categories;
            public ContextValue Value;

            public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
            {
                int num = Value.Calculate(this.Fact.MaybeContext);

                if (categories.Contains(evt.Weapon.Blueprint.Category))
                {
                    evt.AddBonusDamage(num);
                }
            }

            public override void OnEventDidTrigger(RuleCalculateWeaponStats evt)
            {
            }
        }


        [AllowedOn(typeof(BlueprintBuff))]
        [ComponentName("Buffs/Damage bonus for specific weapon types")]
        public class ContextWeaponTypeDamageBonus : RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
        {
            public BlueprintWeaponType[] weapon_types;
            public ContextValue Value;

            public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
            {
                int num = Value.Calculate(this.Fact.MaybeContext);
                foreach (var w in weapon_types)
                {
                    if (evt.Weapon.Blueprint.Type == w)
                    {
                        evt.AddBonusDamage(num);
                        return;
                    }
                }

            }

            public override void OnEventDidTrigger(RuleCalculateWeaponStats evt)
            {
            }
        }


        public class ContextActionRemoveBuffFromCaster : ContextAction
        {
            public BlueprintBuff Buff;
            public int remove_delay_seconds = 0;

            public override string GetCaption()
            {
                return "Remove Buff From Caster: " + this.Buff.Name;
            }

            public override void RunAction()
            {
                MechanicsContext context = ElementsContext.GetData<MechanicsContext.Data>()?.Context;
                if (context == null)
                    return;
                UnitEntityData maybeCaster = this.Context.MaybeCaster;
                foreach (var b in this.Target.Unit.Buffs)
                {
                    if (b.Blueprint == Buff && b.Context.MaybeCaster == maybeCaster)
                    {
                        if (remove_delay_seconds > 0)
                            b.RemoveAfterDelay(new TimeSpan(0, 0, remove_delay_seconds));
                        else
                            b.Remove();
                        //this.Target.Unit.Buffs.RemoveFact((BlueprintFact)this.Buff);
                    }
                }
            }
        }


        //gives target immunity to buff unless target is caster
        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class SpecificBuffImmunityExceptCaster : RuleInitiatorLogicComponent<RuleApplyBuff>
        {
            public BlueprintBuff Buff;
            public bool except_caster = false;

            public override void OnEventAboutToTrigger(RuleApplyBuff evt)
            {
                if (evt.Context.MaybeCaster == this.Owner.Unit && except_caster)
                    return;
                if (evt.Blueprint != this.Buff)
                    return;
                evt.CanApply = false;
            }

            public override void OnEventDidTrigger(RuleApplyBuff evt)
            {
            }
        }



        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ComeAndGetMe : RuleTargetLogicComponent<RuleCalculateAC>, ITargetRulebookHandler<RuleDealDamage>
        {
            public override void OnEventAboutToTrigger(RuleCalculateAC evt)
            {
                if (this.Owner.Body.PrimaryHand.MaybeWeapon != null && this.Owner.Body.PrimaryHand.MaybeWeapon.Blueprint.IsMelee)
                {
                    //this.Owner.Unit.CombatState.AttackOfOpportunity(evt.Initiator);
                    Game.Instance.CombatEngagementController.ForceAttackOfOpportunity(this.Owner.Unit, evt.Initiator);
                }
                evt.AddBonus(-4, this.Fact);
                return;
            }

            public override void OnEventDidTrigger(RuleCalculateAC evt)
            {
            }


            public void OnEventAboutToTrigger(RuleDealDamage evt)
            {
                if (evt.DamageBundle.Count() > 0 && evt.Reason.Rule is RuleAttackWithWeapon)
                {
                    evt.DamageBundle.ElementAt(0).AddBonus(4);
                }

            }
            public void OnEventDidTrigger(RuleDealDamage evt)
            {
            }
        }


        public class WeaponTypeSizeChange : RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
        {
            public int SizeCategoryChange;
            public BlueprintWeaponType[] WeaponTypes;

            public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
            {
                if (!this.WeaponTypes.Contains(evt.Weapon.Blueprint.Type) || this.SizeCategoryChange == 0)
                    return;
                if (this.SizeCategoryChange > 0)
                {
                    for (int i = 0; i < this.SizeCategoryChange; i++)
                    {
                        evt.IncreaseWeaponSize();
                    }
                }
                else
                {
                    for (int i = 0; i > this.SizeCategoryChange; i--)
                    {
                        evt.DecreaseWeaponSize();
                    }
                }
            }

            public override void OnEventDidTrigger(RuleCalculateWeaponStats evt)
            {
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class ContextWeaponDamageBonus : RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
        {
            public ContextValue value;
            public bool apply_to_melee = true;
            public bool apply_to_ranged = false;
            public bool apply_to_thrown = true;
            public bool scale_for_2h = true;


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

            public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
            {
                int damage_bonus = value.Calculate(this.Context);
                if (damage_bonus <= 0)
                {
                    return;
                }

                var weapon = evt.Weapon;
                if (weapon == null)
                {
                    return;
                }
                if (weapon.Blueprint.IsMelee && !apply_to_melee
                    || weapon.Blueprint.IsRanged && !apply_to_ranged && weapon.Blueprint.FighterGroup != WeaponFighterGroup.Thrown
                    || weapon.Blueprint.FighterGroup == WeaponFighterGroup.Thrown && !apply_to_thrown)
                {
                    return;
                }
                if (scale_for_2h
                    && (weapon.Blueprint.IsTwoHanded || (weapon.Blueprint.IsOneHandedWhichCanBeUsedWithTwoHands && !evt.Initiator.Body.SecondaryHand.HasItem))
                    )
                {
                    damage_bonus += damage_bonus / 2;
                }
                evt.AddBonusDamage(damage_bonus);
            }

            public override void OnEventDidTrigger(RuleCalculateWeaponStats evt)
            {
            }
        }



        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ForbidSpellCastingUnlessHasClass : OwnedGameLogicComponent<UnitDescriptor>
        {
            public bool ForbidMagicItems;
            public BlueprintCharacterClass[] allowed_classes;
            private bool activated = false;

            public override void OnTurnOn()
            {
                foreach (var c in allowed_classes)
                {
                    foreach (Kingmaker.UnitLogic.ClassData classData in this.Owner.Progression.Classes)
                    {
                        if (classData.CharacterClass == c)
                        {
                            return;
                        }
                    }
                }

                activated = true;
                this.Owner.State.SpellCastingForbidden.Retain();
                if (!this.ForbidMagicItems)
                    return;
                this.Owner.State.MagicItemsForbidden.Retain();
            }

            public override void OnTurnOff()
            {
                if (!activated)
                {
                    return;
                }
                activated = false;
                this.Owner.State.SpellCastingForbidden.Release();
                if (!this.ForbidMagicItems)
                    return;
                this.Owner.State.MagicItemsForbidden.Release();
            }
        }

        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ReflectDamage : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, IInitiatorRulebookSubscriber
        {

            public bool reflect_melee_weapon = false;
            public bool reflect_ranged_weapon = false;
            public bool reflect_magic = false;

            public float reflection_coefficient = 0.0f;

            public void OnEventAboutToTrigger(RuleDealDamage evt)
            {

            }

            public void OnEventDidTrigger(RuleDealDamage evt)
            {
                if (evt.Target == evt.Initiator)
                {
                    return;
                }
                if ((evt.Reason.Rule is RuleAttackWithWeapon))
                {
                    var rule_attack_with_weapon = (RuleAttackWithWeapon)evt.Reason.Rule;
                    bool is_melee = (rule_attack_with_weapon.Weapon == null || rule_attack_with_weapon.Weapon.Blueprint.IsMelee);
                    bool is_ranged = (rule_attack_with_weapon.Weapon != null && rule_attack_with_weapon.Weapon.Blueprint.IsRanged);
                    if (is_melee && !reflect_melee_weapon)
                    {
                        return;
                    }

                    if (is_ranged && !reflect_ranged_weapon)
                    {
                        return;
                    }
                }
                else
                {
                    if (!reflect_magic)
                    {
                        return;
                    }
                }

                int reflected_dmage = (int)(reflection_coefficient * evt.Damage);
                if (reflected_dmage <= 0)
                {
                    return;
                }

                var base_dmg = new EnergyDamage(DiceFormula.Zero, Kingmaker.Enums.Damage.DamageEnergyType.Holy);
                base_dmg.AddBonus(reflected_dmage);

                RuleDealDamage evt_dmg = new RuleDealDamage(this.Owner.Unit, this.Owner.Unit, new DamageBundle(base_dmg));
                Rulebook.Trigger<RuleDealDamage>(evt_dmg);
            }
        }

        [ComponentName("Weapon Stat Replacement")]
        [AllowedOn(typeof(BlueprintBuff))]
        public class BuffWeaponStatReplacement : BuffLogic, IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookSubscriber,
                                                           IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>, IRulebookHandler<RuleCalculateAttackBonusWithoutTarget>
        {
            public StatType Stat;
            public BlueprintItemWeapon weapon;
            public bool use_caster_value = false;
            public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
            {
                if (evt.Weapon == null)
                {
                    return;
                }

                if (evt.Weapon.Blueprint != weapon)
                    return;

                evt.OverrideDamageBonusStat(this.Stat);

                var caster = this.Fact.MaybeContext?.MaybeCaster;
                if (use_caster_value && caster != null)
                {
                    int caster_bonus = caster.Stats.GetStat<ModifiableValueAttributeStat>(Stat).Bonus;
                    int owner_bonus = evt.Initiator.Stats.GetStat<ModifiableValueAttributeStat>(Stat).Bonus;
                    evt.AddBonusDamage(caster_bonus - owner_bonus);
                }
            }

            public void OnEventDidTrigger(RuleCalculateWeaponStats evt)
            {
            }


            public void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
            {
                if (evt.Weapon == null)
                {
                    return;
                }

                if (evt.Weapon.Blueprint != weapon)
                    return;

                evt.AttackBonusStat = Stat;

                var caster = this.Fact.MaybeContext?.MaybeCaster;
                if (use_caster_value && caster != null)
                {
                    int caster_bonus = caster.Stats.GetStat<ModifiableValueAttributeStat>(Stat).Bonus;
                    int owner_bonus = evt.Initiator.Stats.GetStat<ModifiableValueAttributeStat>(Stat).Bonus;
                    evt.AddBonus(caster_bonus - owner_bonus, this.Fact);
                }

            }

            public void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt)
            {
            }
        }

        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ContextWeaponDamageDiceReplacement : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookSubscriber
        {
            public BlueprintParametrizedFeature[] required_parametrized_features;
            public DiceFormula[] dice_formulas;
            public ContextValue value;

            private MechanicsContext Context
            {
                get
                {
                    return this.Fact.MaybeContext;
                }
            }


            private bool checkFeature(WeaponCategory category)
            {
                if (required_parametrized_features.Empty())
                {
                    return true;
                }
                foreach (var f in required_parametrized_features)
                {
                    if (this.Owner.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => p.Blueprint == f).Any(p => p.Param == category))
                    {
                        return true;
                    }
                }
                return false;
            }

            public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
            {
                if (!checkFeature(evt.Weapon.Blueprint.Category))
                {
                    return;
                }

                int dice_id = value.Calculate(this.Context);
                if (dice_id < 0)
                {
                    dice_id = 0;
                }
                if (dice_id >= dice_formulas.Length)
                {
                    dice_id = dice_formulas.Length - 1;
                }

                double new_avg_dmg = (dice_formulas[dice_id].MinValue(0) + dice_formulas[dice_id].MaxValue(0)) / 2.0;
                double current_avg_damage = (evt.Weapon.Damage.MaxValue(0) + evt.Weapon.Damage.MinValue(0)) / 2.0;

                if (new_avg_dmg > current_avg_damage)
                {
                    evt.WeaponDamageDiceOverride = dice_formulas[dice_id];
                }
            }

            public void OnEventDidTrigger(RuleCalculateWeaponStats evt)
            {

            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowedOn(typeof(BlueprintBuff))]
        [AllowMultipleComponents]
        public class AddOutgoingPhysicalDamageAlignmentIfParametrizedFeature : RuleInitiatorLogicComponent<RulePrepareDamage>
        {
            public BlueprintParametrizedFeature required_parametrized_feature;
            public DamageAlignment damage_alignment;

            public override void OnEventAboutToTrigger(RulePrepareDamage evt)
            {
            }

            private bool checkFeature(WeaponCategory category)
            {
                if (required_parametrized_feature == null)
                {
                    return true;
                }
                return this.Owner.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => p.Blueprint == required_parametrized_feature).Any(p => p.Param == category);
            }

            public override void OnEventDidTrigger(RulePrepareDamage evt)
            {
                ItemEntityWeapon weapon = evt.DamageBundle.Weapon;
                if (weapon == null)
                {
                    return;
                }

                if (!checkFeature(weapon.Blueprint.Category))
                {
                    return;
                }

                var damage = evt.DamageBundle.WeaponDamage as PhysicalDamage;
                if (damage == null)
                {
                    return;
                }

                damage.AddAlignment(damage_alignment);
            }

        }


        public class ConsumeResourceIfAbilitiesFromGroupActivated : ContextAction
        {
            public ActivatableAbilityGroup group;
            public int num_abilities_activated;
            public BlueprintAbilityResource resource;


            public override string GetCaption()
            {
                return $"Consume resource ({resource.Name}) on {num_abilities_activated} from {group.ToString()}";
            }

            public override void RunAction()
            {
                if (resource == null)
                {
                    return;
                }
                var unit = this.Target?.Unit;
                if (unit == null)
                {
                    return;
                }

                if (unit.Descriptor.Resources.GetResourceAmount(resource) <= 0)
                {
                    return;
                }

                int num_activated = 0;
                foreach (var a in unit.ActivatableAbilities)
                {
                    if (a.Blueprint.Group == group && a.IsOn)
                    {
                        num_activated++;
                    }
                }
                if (num_activated >= num_abilities_activated)
                {
                    unit.Descriptor.Resources.Spend((BlueprintScriptableObject)this.resource, 1);
                }
            }
        }


        public class DeactivateAbilityFromGroup : ContextAction
        {
            public ActivatableAbilityGroup group;
            public int num_abilities_activated;


            public override string GetCaption()
            {
                return $"Deactivate Ability From Group {group.ToString()} if more than {num_abilities_activated}.";
            }

            public override void RunAction()
            {
                var unit = this.Target?.Unit;
                if (unit == null)
                {
                    return;
                }
                int num_activated = 0;
                foreach (var a in unit.ActivatableAbilities)
                {
                    if (a.Blueprint.Group == group && a.IsOn)
                    {
                        if (num_activated < num_abilities_activated)
                        {
                            num_activated++;
                        }
                        else
                        {
                            a.Stop();
                        }
                    }

                }
            }
        }


        [ComponentName("Healing bonus")]
        [AllowedOn(typeof(Kingmaker.Blueprints.Facts.BlueprintUnitFact))]
        public class HealingBonusCasterLevel : OwnedGameLogicComponent<UnitDescriptor>, ITargetRulebookSubscriber, ITargetRulebookHandler<RuleCalculateAbilityParams>, ITargetRulebookHandler<RuleHealDamage>,
                                               IRulebookHandler<RuleHealDamage>, IRulebookHandler<RuleCalculateAbilityParams>
        {
            private int bonus = 0;
            public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                bonus = 0;
            }

            public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
                bonus = evt.Result.CasterLevel;
            }



            public void OnEventAboutToTrigger(RuleHealDamage evt)
            {

            }


            public void OnEventDidTrigger(RuleHealDamage evt)
            {
                if (bonus == 0 || evt.Target.Descriptor != this.Owner)
                {
                    bonus = 0;
                    return;
                }
                int old_value = bonus;
                bonus = 0;
                GameHelper.HealDamage(evt.Target, evt.Target, old_value);
            }
        }


        [ComponentName("Increase specified spells  DC")]
        [AllowedOn(typeof(BlueprintBuff))]
        public class IncreaseSpecifiedSpellsDC : BuffLogic, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookSubscriber
        {
            public BlueprintAbility[] spells;
            public int BonusDC;

            public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                if (!spells.Contains(evt.Spell))
                    return;
                evt.AddBonusDC(this.BonusDC);

            }

            public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {

            }
        }

        public class ActivatableAbilityAlignmentRestriction : ActivatableAbilityRestriction
        {
            [AlignmentMask]
            public AlignmentMaskType Alignment;

            public override bool IsAvailable()
            {
                return (Owner.Alignment.Value.ToMask() & this.Alignment) != AlignmentMaskType.None;
            }
        }


        public class ActivatableAbilityNoAlignmentRestriction : ActivatableAbilityRestriction
        {
            [AlignmentMask]
            public AlignmentMaskType Alignment;

            public override bool IsAvailable()
            {
                return (Owner.Alignment.Value.ToMask() & this.Alignment) == AlignmentMaskType.None;
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilityCasterMainWeaponCheckHasParametrizedFeature : BlueprintComponent, IAbilityCasterChecker
        {
            public BlueprintParametrizedFeature feature = null;
            public BlueprintFeature alternative = null;

            private bool checkFeature(UnitEntityData caster, WeaponCategory category)
            {
                if (feature == null)
                {
                    return false;
                }
                return caster.Descriptor.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => p.Blueprint == feature).Any(p => p.Param == category);
            }

            private bool checkAlternative(UnitEntityData caster)
            {
                if (alternative == null)
                {
                    return false;
                }
                return caster.Descriptor.Progression.Features.HasFact(alternative);
            }

            public bool CorrectCaster(UnitEntityData caster)
            {
                var weapon = caster.Body.PrimaryHand.HasWeapon ? caster.Body.PrimaryHand.MaybeWeapon : caster.Body.EmptyHandWeapon;
                if (weapon == null)
                {
                    return false;
                }

                return checkFeature(caster, weapon.Blueprint.Category) || checkAlternative(caster);
            }

            public string GetReason()
            {
                return (string)LocalizedTexts.Instance.Reasons.SpecificWeaponRequired;
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilityCasterHasSHield : BlueprintComponent, IAbilityCasterChecker
        {
            public bool CorrectCaster(UnitEntityData caster)
            {
                return caster.Body.SecondaryHand.HasShield;
            }

            public string GetReason()
            {
                return (string)LocalizedTexts.Instance.Reasons.SpecificWeaponRequired;
            }
        }


        public class ActivatableAbilityMainWeaponHasParametrizedFeatureRestriction : ActivatableAbilityRestriction
        {
            public BlueprintParametrizedFeature[] features;

            private bool checkFeature(WeaponCategory category)
            {
                if (features.Empty())
                {
                    return true;
                }
                foreach (var f in features)
                {
                    if (Owner.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => p.Blueprint == f).Any(p => p.Param == category))
                    {
                        return true;
                    }
                }
                return false;
            }

            public override bool IsAvailable()
            {
                var weapon = Owner.Body.PrimaryHand.HasWeapon ? Owner.Body.PrimaryHand.MaybeWeapon : Owner.Body.EmptyHandWeapon;
                if (weapon == null)
                {
                    return false;
                }

                return checkFeature(weapon.Blueprint.Category);
            }
        }


        [ComponentName("Ignores Aoo with specified weapons")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class SpecifiedWeaponCategoryIgnoreAoo : RuleInitiatorLogicComponent<RuleAttackRoll>
        {
            public WeaponFighterGroup[] WeaponGroups;

            public override void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
                if (evt.Weapon == null || !WeaponGroups.Contains(evt.Weapon.Blueprint.FighterGroup))
                    return;
                evt.DoNotProvokeAttacksOfOpportunity = true;
            }

            public override void OnEventDidTrigger(RuleAttackRoll evt)
            {
            }
        }



        [AllowMultipleComponents]
        public class AddInitiatorAttackWithWeaponTriggerOnCharge : GameLogicComponent, IInitiatorRulebookHandler<RuleAttackWithWeapon>, IInitiatorRulebookHandler<RuleAttackWithWeaponResolve>, IRulebookHandler<RuleAttackWithWeapon>, IInitiatorRulebookSubscriber, IRulebookHandler<RuleAttackWithWeaponResolve>
        {
            static private BlueprintBuff charge_buff => ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("f36da144a379d534cad8e21667079066");

            public bool WaitForAttackResolve;
            public BlueprintWeaponType WeaponType;
            public bool CheckWeaponCategory;
            [ShowIf("CheckWeaponCategory")]
            public WeaponCategory Category;
            public bool ActionsOnInitiator;
            [Tooltip("For melee attacks only")]
            public bool ReduceHPToZero;
            public bool AllNaturalAndUnarmed;
            public ActionList Action;

            public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
            {
            }

            public void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
                if (this.WaitForAttackResolve)
                    return;
                this.TryRunActions(evt);
            }

            public void OnEventAboutToTrigger(RuleAttackWithWeaponResolve evt)
            {
            }

            public void OnEventDidTrigger(RuleAttackWithWeaponResolve evt)
            {
                if (!this.WaitForAttackResolve)
                    return;
                this.TryRunActions(evt.AttackWithWeapon);
            }

            private void TryRunActions(RuleAttackWithWeapon rule)
            {
                if (!this.CheckCondition(rule))
                    return;
                if (!this.ActionsOnInitiator)
                {
                    using (new ContextAttackData(rule.AttackRoll, (Projectile)null))
                        (this.Fact as IFactContextOwner)?.RunActionInContext(this.Action, (TargetWrapper)rule.Target);
                }
                else
                {
                    using (new ContextAttackData(rule.AttackRoll, (Projectile)null))
                        (this.Fact as IFactContextOwner)?.RunActionInContext(this.Action, (TargetWrapper)rule.Initiator);
                }
            }

            private bool CheckCondition(RuleAttackWithWeapon evt)
            {
                ItemEntity owner = (this.Fact as ItemEnchantment)?.Owner;

                if (owner != null && owner != evt.Weapon || !evt.AttackRoll.IsHit || ((bool)(this.WeaponType) && this.WeaponType != evt.Weapon.Blueprint.Type || this.CheckWeaponCategory && this.Category != evt.Weapon.Blueprint.Category) || (this.AllNaturalAndUnarmed && !evt.Weapon.Blueprint.Type.IsNatural && !evt.Weapon.Blueprint.Type.IsUnarmed))
                    return false;

                if (!evt.Initiator.Buffs.HasFact(charge_buff))
                {
                    return false;
                }

                if (this.ReduceHPToZero)
                {
                    if (evt.MeleeDamage == null || evt.MeleeDamage.IsFake || evt.Target.HPLeft > 0)
                        return false;
                    return evt.Target.HPLeft + evt.MeleeDamage.Damage > 0;
                }

                return true;
            }
        }


        public class ResourseCostCalculatorWithDecreasingFacts : BlueprintComponent, IAbilityResourceCostCalculator
        {
            public BlueprintFact[] cost_reducing_facts = new BlueprintFact[0];

            public int Calculate(AbilityData ability)
            {
                var cost = ability.Blueprint.GetComponent<AbilityResourceLogic>().Amount;
                foreach (var f in cost_reducing_facts)
                {
                    if (ability.Caster.Buffs.HasFact(f))
                    {
                        cost--;
                    }
                }
                return cost < 0 ? 0 : cost;
            }
        }

        //fix for 0 cost ability data to avoid division by 0
        //we temporary IsSpendResource to false
        //and restore it in prefix
        [Harmony12.HarmonyPatch(typeof(AbilityData))]
        [Harmony12.HarmonyPatch("GetAvailableForCastCount", Harmony12.MethodType.Normal)]
        class AbilityData__GetAvailableForCastCount__Patch
        {
            static bool Prefix(AbilityData __instance, ref bool __state, ref int __result)
            {
                __state = false;

                if (__instance.StickyTouch != null)
                {
                    var num_charges = __instance.Caster.Get<StickyTouchMechnics.UnitPartTouchMultipleCharges>();
                    __result = num_charges == null ? 1 : num_charges.getNumCharges();
                    return false;
                }

                if (__instance.Fact != null)
                {
                    AbilityResourceLogic abilityResourceLogic = __instance.Fact.Blueprint.GetComponents<AbilityResourceLogic>().FirstOrDefault<AbilityResourceLogic>();
                    if (abilityResourceLogic == null)
                    {
                        return true;
                    }
                    if (abilityResourceLogic.CalculateCost(__instance) == 0 && abilityResourceLogic.IsSpendResource)
                    {
                        abilityResourceLogic.IsSpendResource = false;
                        __state = true;
                    }
                }
                return true;
            }


            static void Postfix(AbilityData __instance, ref bool __state, ref int __result)
            {
                if (__state)
                {
                    AbilityResourceLogic abilityResourceLogic = __instance.Fact.Blueprint.GetComponents<AbilityResourceLogic>().FirstOrDefault<AbilityResourceLogic>();
                    abilityResourceLogic.IsSpendResource = true;
                }
            }
        }


        [ComponentName("Actions depending on context value")]
        [AllowMultipleComponents]
        [PlayerUpgraderAllowed]
        public class RunActionsDependingOnContextValue : ContextAction
        {
            public string Comment;
            public ContextValue value;
            public ActionList[] actions;

            public override void RunAction()
            {
                int action_id = value.Calculate(this.Context) - 1;
                if (action_id < 0)
                {
                    action_id = 0;
                }
                if (action_id >= actions.Length)
                {
                    action_id = actions.Length - 1;
                }
                actions[action_id].Run();
            }

            public override string GetCaption()
            {
                return "Actions based on context (" + this.Comment + " )";
            }
        }


        [ComponentName("Attack bonus against fact owner for attack type")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class AttackBonusAgainstFactsOwner : RuleInitiatorLogicComponent<RuleAttackRoll>
        {
            public BlueprintUnitFact[] CheckedFacts;
            public ContextValue Bonus;
            public ModifierDescriptor Descriptor;
            public AttackType[] attack_types;
            public bool only_from_caster = false;

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

            public override void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
                if (!attack_types.Contains(evt.AttackType))
                {
                    return;
                }


                var bonus = Bonus.Calculate(this.Context);
                int total_bonus = 0;
                if (!only_from_caster)
                {
                    foreach (var f in CheckedFacts)
                    {
                        if (evt.Target.Descriptor.HasFact(f))
                        {

                        }
                    }
                }

                if (only_from_caster)
                {
                    var caster = this.Context.MaybeCaster;
                    foreach (var b in evt.Target.Buffs)
                    {
                        if (CheckedFacts.Contains(b.Blueprint) && b.MaybeContext.MaybeCaster == caster)
                        {
                            total_bonus += bonus;
                        }
                    }
                }

                if (total_bonus > 0)
                {
                    evt.AddTemporaryModifier(evt.Initiator.Stats.AdditionalAttackBonus.AddModifier(total_bonus, this, Descriptor));
                }
            }

            public override void OnEventDidTrigger(RuleAttackRoll evt)
            {
            }
        }


        [ComponentName("spend resource")]
        [AllowMultipleComponents]
        [PlayerUpgraderAllowed]
        public class ContextActionSpendResource : ContextAction
        {
            public BlueprintAbilityResource resource;
            public int amount = 1;
            public BlueprintUnitFact[] cost_reducing_facts = new BlueprintUnitFact[0];


            public override void RunAction()
            {
                
                int need_resource = amount;

                var owner = this.Context.MaybeOwner.Descriptor;

                foreach (var f in cost_reducing_facts)
                {
                    if (owner.HasFact(f))
                    {
                        need_resource--;
                    }
                }

                if (need_resource < 0)
                {
                    need_resource = 0;
                }

                if (this.resource == null || owner.Resources.GetResourceAmount(this.resource) < need_resource)
                {
                    return;
                }
                
                owner.Resources.Spend(this.resource, need_resource);
            }


            public override string GetCaption()
            {
                return $"Spend {resource.name} ({amount})";
            }
        }


        [ComponentName("Buffs/AddEffect/EnergyDurability")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowedOn(typeof(BlueprintBuff))]
        [AllowMultipleComponents]
        public class AddEnergyDamageDurability : RuleTargetLogicComponent<RuleCalculateDamage>
        {
            public DamageEnergyType Type;
            public float scaling = 0.5f;

            public override void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {
                foreach (BaseDamage baseDamage in evt.DamageBundle)
                {
                    EnergyDamage energyDamage = baseDamage as EnergyDamage;
                    if (energyDamage != null && energyDamage.EnergyType == this.Type)
                    {
                        energyDamage.Durability = scaling;
                    }
                }
            }

            public override void OnEventDidTrigger(RuleCalculateDamage evt)
            {
            }
        }


        [ComponentName("ReplaceSkillRankWithClassLevel")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowedOn(typeof(BlueprintBuff))]
        [AllowMultipleComponents]
        public class ReplaceSkillRankWithClassLevel : RuleInitiatorLogicComponent<RuleSkillCheck>
        {
            public BlueprintUnitFact reason = null;
            public StatType skill;
            public BlueprintCharacterClass character_class;

            public override void OnEventAboutToTrigger(RuleSkillCheck evt)
            {
                ModifiableValue stat_value = evt.Initiator.Stats.GetStat(skill);
                if (skill == StatType.CheckIntimidate || skill == StatType.CheckDiplomacy || skill == StatType.CheckBluff)
                {
                    stat_value = evt.Initiator.Stats.GetStat(StatType.SkillPersuasion);
                }
                if (evt.StatType != skill)
                {
                    return;
                }
                if (reason != null && (evt.Reason.Fact == null || evt.Reason.Fact.Blueprint != reason))
                {
                    return;
                }

                int class_level = evt.Initiator.Descriptor.Progression.GetClassLevel(character_class);
                if (class_level <= stat_value.BaseValue)
                {
                    return;
                }

                evt.Bonus.AddModifier(class_level - stat_value.BaseValue, this, ModifierDescriptor.UntypedStackable);

            }

            public override void OnEventDidTrigger(RuleSkillCheck evt)
            {
            }
        }


        [AllowMultipleComponents]
        [ComponentName("Saving throw bonus against fact from caster")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class SavingThrowBonusAgainstFactFromCaster : RuleInitiatorLogicComponent<RuleSavingThrow>
        {
            public BlueprintUnitFact CheckedFact;
            public ModifierDescriptor Descriptor;
            public ContextValue Value;

            public override void OnEventAboutToTrigger(RuleSavingThrow evt)
            {
                UnitDescriptor descriptor = evt.Reason.Caster?.Descriptor;
                if (descriptor == null)
                    return;
                int bonus = Value.Calculate(this.Fact.MaybeContext);
                foreach (var b in descriptor.Buffs)
                {
                    if (b.Blueprint == CheckedFact && b.Context.MaybeCaster == evt.Initiator)
                    {
                        evt.AddTemporaryModifier(evt.Initiator.Stats.SaveWill.AddModifier(bonus, (GameLogicComponent)this, this.Descriptor));
                        evt.AddTemporaryModifier(evt.Initiator.Stats.SaveReflex.AddModifier(bonus, (GameLogicComponent)this, this.Descriptor));
                        evt.AddTemporaryModifier(evt.Initiator.Stats.SaveFortitude.AddModifier(bonus, (GameLogicComponent)this, this.Descriptor));
                        return;
                    }
                }

            }

            public override void OnEventDidTrigger(RuleSavingThrow evt)
            {
            }
        }

        public class AbilityTargetCompositeOr : BlueprintComponent, IAbilityTargetChecker
        {
            public IAbilityTargetChecker[] ability_checkers;

            public bool Not;

            public bool CanTarget(UnitEntityData caster, TargetWrapper target)
            {
                bool result = false;

                foreach (var c in ability_checkers)
                {
                    result = result || c.CanTarget(caster, target);
                }

                return result != Not;
            }
        }

        [ComponentName("Weapon Attack Auto Miss")]
        [AllowedOn(typeof(Kingmaker.Blueprints.Facts.BlueprintUnitFact))]
        public class WeaponAttackAutoMiss : RuleTargetLogicComponent<RuleAttackRoll>, ITargetRulebookHandler<RuleAttackRoll>
        {

            public AttackType[] attack_types;


            public override void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
                if (evt.Weapon == null)
                {
                    return;
                }

                if (attack_types.Contains(evt.AttackType))
                {
                    evt.AutoMiss = true;
                }
            }

            public override void OnEventDidTrigger(RuleAttackRoll evt)
            {

            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilityCasterHasResource : BlueprintComponent, IAbilityCasterChecker
        {
            public BlueprintAbilityResource resource;
            public int amount = 1;

            public bool CorrectCaster(UnitEntityData caster)
            {
                if (resource == null)
                {
                    return true;
                }
                if (caster.Descriptor.Resources.GetResourceAmount(resource) < amount)
                {
                    return false;
                }

                return true;
            }

            public string GetReason()
            {
                return "Insufficient secondary resource";
            }
        }


        public class ContextConditionHasFacts : ContextCondition
        {
            public BlueprintUnitFact[] Facts;
            public bool all = false;

            protected override string GetConditionCaption()
            {
                return string.Empty;
            }

            protected override bool CheckCondition()
            {
                foreach (var f in Facts)
                {
                    if (this.Target.Unit.Descriptor.HasFact(f) && !all)
                    {
                        return true;
                    }
                    else if (!this.Target.Unit.Descriptor.HasFact(f) && all)
                    {
                        return false;
                    }
                }
                return all;
            }
        }


        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class AddFeatureIfKnownSpellAquired : OwnedGameLogicComponent<UnitDescriptor>, IUnitGainLevelHandler, IGlobalSubscriber
        {
            public int spell_level = 1;
            public int num_spells_will_learn = 2;
            [NotNull]
            public BlueprintSpellbook spellbook;
            public BlueprintFeature Feature;

            [JsonProperty]
            private Fact m_AppliedFact;

            public override void OnFactActivate()
            {
                this.Apply();
            }

            public override void OnFactDeactivate()
            {
                this.Owner.RemoveFact(this.m_AppliedFact);
                this.m_AppliedFact = (Fact)null;
            }

            public void HandleUnitGainLevel(UnitDescriptor unit, BlueprintCharacterClass @class)
            {
                this.Apply();
            }

            private void Apply()
            {
                if (this.m_AppliedFact != null || (!this.Check(Owner)))
                    return;
                this.m_AppliedFact = this.Owner.AddFact(this.Feature, (MechanicsContext)null, (FeatureParam)null);
            }


            public bool Check(UnitDescriptor unit)
            {
                foreach (ClassData classData in unit.Progression.Classes)
                {
                    BlueprintSpellbook class_spellbook = classData.Spellbook;
                    if (class_spellbook != spellbook)
                    {
                        continue;
                    }

                    var caster_level = unit.DemandSpellbook(classData.CharacterClass).CasterLevel;
                    int? knows_spells = spellbook.SpellsKnown.GetCount(classData.Level, spell_level);
                    //nothing to learn
                    if (!knows_spells.HasValue)
                    {
                        return false;
                    }
                    int? will_know_spells = spellbook.SpellsKnown.GetCount(classData.Level + 1, spell_level);
                    if (!will_know_spells.HasValue)
                    {
                        return false;
                    }

                    if (will_know_spells.Value - knows_spells.Value < num_spells_will_learn)
                    {
                        return false;
                    }
                    return true;
                }

                return false;
            }
        }


        public class PrerequisiteKnownSpellAquired : Prerequisite
        {
            public int spell_level = 1;
            public int num_spells_will_learn = 2;
            [NotNull]
            public BlueprintSpellbook spellbook;

            public override bool Check(
              FeatureSelectionState selectionState,
              UnitDescriptor unit,
              LevelUpState state)
            {
                foreach (ClassData classData in unit.Progression.Classes)
                {
                    BlueprintSpellbook class_spellbook = classData.Spellbook;
                    if (class_spellbook != spellbook)
                    {
                        continue;
                    }

                    var caster_level = unit.DemandSpellbook(spellbook).CasterLevel;
                    int? knows_spells = spellbook.SpellsKnown.GetCount(caster_level - 1, spell_level);
                    //nothing to learn
                    if (!knows_spells.HasValue)
                    {
                        knows_spells = 0;
                    }
                    int? will_know_spells = spellbook.SpellsKnown.GetCount(caster_level, spell_level);
                    if (!will_know_spells.HasValue)
                    {
                        return false;
                    }

                    if (will_know_spells.Value - knows_spells.Value < num_spells_will_learn)
                    {
                        return false;
                    }
                    return true;
                }

                return false;
            }

            public override string GetUIText()
            {
                return "";
            }
        }

        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class AbilityUsedTrigger : GameLogicComponent, IGlobalRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>, IGlobalRulebookSubscriber
        {
            [NotNull]
            public Dictionary<BlueprintAbility, ActionList> spell_action_map = new Dictionary<BlueprintAbility, ActionList>();
            public BlueprintAbility[] Spells = (BlueprintAbility[])Array.Empty<BlueprintAbility>();
            [NotNull]
            public ActionList Actions = new ActionList();

            public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
            }

            public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
                if (evt.Spell.IsSpell)
                {
                    return;
                }
                if (!Spells.Contains<BlueprintAbility>(evt.Spell))
                    return;

                (this.Fact as IFactContextOwner)?.RunActionInContext(this.Actions, evt.Initiator);
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class TouchACBonus : OwnedGameLogicComponent<UnitDescriptor>, ITargetRulebookHandler<RuleCalculateAC>, IRulebookHandler<RuleCalculateAC>, ITargetRulebookSubscriber
        {
            public ModifierDescriptor Descriptor;
            public ContextValue value;

            public void OnEventAboutToTrigger(RuleCalculateAC evt)
            {
                int bonus = value.Calculate(this.Fact.MaybeContext);
                if (!ModifiableValue.DefaultStackingDescriptors.Contains(Descriptor))
                {
                    bonus -= this.Owner.Stats.AC.GetDescriptorBonus(this.Descriptor);
                }

                bonus = Math.Min(this.Owner.Stats.AC.ModifiedValue - this.Owner.Stats.AC.Touch, bonus);

                if (bonus <= 0 || evt.IsTargetFlatFooted || !evt.AttackType.IsTouch())
                {
                    return;
                }
                evt.AddBonus(bonus, this.Fact);
            }

            public void OnEventDidTrigger(RuleCalculateAC evt)
            {
            }
        }


        [AllowedOn(typeof(BlueprintBuff))]
        public class DamageBonusAgainstSpellUser : BuffLogic, ITargetRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ITargetRulebookSubscriber
        {
            public ContextValue Value;
            public bool arcane = true;
            public bool divine = true;
            public bool spell_like = true;


            private bool isValidTarget(UnitDescriptor unit)
            {
                foreach (ClassData classData in unit.Progression.Classes)
                {
                    BlueprintSpellbook spellbook = classData.Spellbook;
                    if (spellbook == null)
                    {
                        continue;
                    }

                    if (spellbook.IsArcane && arcane)
                    {
                        return true;
                    }

                    if (!spellbook.IsArcane && !spellbook.IsAlchemist && divine)
                    {
                        return true;
                    }
                }

                if (!spell_like)
                {
                    return false;
                }

                foreach (var a in unit.Abilities)
                {
                    if (a.Blueprint.Type == AbilityType.SpellLike || a.Blueprint.Type == AbilityType.Spell)
                    {
                        return true;
                    }
                }

                return false;
            }

            public void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {
                if (!isValidTarget(evt.Target.Descriptor))
                {
                    return;
                }

                int bonus = Value.Calculate(this.Context);
                evt.DamageBundle.First?.AddBonus(bonus);
            }

            public void OnEventDidTrigger(RuleCalculateDamage evt)
            {
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ACBonusIfHasFacts : OwnedGameLogicComponent<UnitDescriptor>, ITargetRulebookHandler<RuleAttackRoll>, IRulebookHandler<RuleAttackRoll>, ITargetRulebookSubscriber
        {
            public BlueprintUnitFact[] CheckedFacts;
            public bool all = false;
            public ContextValue Bonus;
            public ModifierDescriptor Descriptor;

            public void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
                bool is_ok = true;
                if (!CheckedFacts.Empty())
                {
                    is_ok = all;
                    foreach (var f in CheckedFacts)
                    {
                        if (evt.Target.Descriptor.HasFact(f))
                        {
                            if (!all)
                            {
                                is_ok = true;
                                break;
                            }
                        }
                        else if (all)
                        {

                            is_ok = false;
                            break;
                        }
                    }
                }

                if (!is_ok)
                {
                    return;
                }

                int bonus = this.Bonus.Calculate(this.Fact.MaybeContext);


                evt.AddTemporaryModifier(evt.Target.Stats.AC.AddModifier(bonus, (GameLogicComponent)this, this.Descriptor));
            }

            public void OnEventDidTrigger(RuleAttackRoll evt)
            {
            }
        }


        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class AttackBonusOnAttackInitiationIfHasFact : RuleInitiatorLogicComponent<RuleAttackRoll>
        {
            public BlueprintUnitFact CheckedFact;
            public ContextValue Bonus;
            public ModifierDescriptor Descriptor;
            public bool OnlyTwoHanded;
            public bool OnlyFirstAttack;
            public AttackType[] WeaponAttackTypes;

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

            public override void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
                if (evt.RuleAttackWithWeapon == null)
                {
                    return;
                }
                if (evt.Weapon == null || (CheckedFact != null && !evt.Initiator.Descriptor.HasFact(this.CheckedFact))
                    || (!evt.Weapon.HoldInTwoHands && OnlyTwoHanded)
                    || (!evt.RuleAttackWithWeapon.IsFirstAttack && OnlyFirstAttack)
                    || !WeaponAttackTypes.Contains(evt.AttackType))
                    return;

                evt.AddTemporaryModifier(evt.Initiator.Stats.AdditionalAttackBonus.AddModifier(this.Bonus.Calculate(this.Context), (GameLogicComponent)this, this.Descriptor));
            }

            public override void OnEventDidTrigger(RuleAttackRoll evt)
            {
            }
        }





        [AllowedOn(typeof(BlueprintUnitFact))]
        public class CriticalConfirmationBonusAgainstALignment : RuleInitiatorLogicComponent<RuleAttackRoll>
        {
            public ContextValue Value;
            public AlignmentComponent EnemyAlignment;

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

            public override void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
                int num = this.Value.Calculate(this.Context);
                if (!evt.Target.Descriptor.Alignment.Value.HasComponent(EnemyAlignment))
                    return;
                evt.CriticalConfirmationBonus += num;
            }

            public override void OnEventDidTrigger(RuleAttackRoll evt)
            {
            }
        }


        public class AddWeaponEnergyDamageDice : BuffLogic, IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookSubscriber
        {
            public ContextDiceValue dice_value;
            public DamageEnergyType Element;
            public AttackType[] range_types;

            public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
            {
                if (evt.Weapon == null && !this.range_types.Contains(evt.Weapon.Blueprint.AttackType))
                    return;

                DiceFormula dice_formula = new DiceFormula(this.dice_value.DiceCountValue.Calculate(this.Context), this.dice_value.DiceType);
                int bonus = this.dice_value.BonusValue.Calculate(this.Context);

                DamageDescription damageDescription = new DamageDescription()
                {
                    TypeDescription = new DamageTypeDescription()
                    {
                        Type = DamageType.Energy,
                        Energy = this.Element
                    },
                    Dice = dice_formula,
                    Bonus = bonus
                };
                evt.DamageDescription.Add(damageDescription);
            }

            public void OnEventDidTrigger(RuleCalculateWeaponStats evt)
            {
            }
        }


        public class AddWeaponEnergyDamageDiceIfHasFact : BuffLogic, IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookSubscriber
        {
            public ContextDiceValue dice_value;
            public DamageEnergyType Element;
            public AttackType[] range_types;
            public BlueprintUnitFact checked_fact;

            public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
            {
                if (evt.Weapon == null && !this.range_types.Contains(evt.Weapon.Blueprint.AttackType) || !this.Owner.HasFact(checked_fact))
                    return;

                DamageDescription damageDescription = new DamageDescription()
                {
                    TypeDescription = new DamageTypeDescription()
                    {
                        Type = DamageType.Energy,
                        Energy = this.Element
                    },
                    Dice = new DiceFormula(this.dice_value.DiceCountValue.Calculate(this.Context), this.dice_value.DiceType),
                    Bonus = this.dice_value.BonusValue.Calculate(this.Context)
                };
                evt.DamageDescription.Add(damageDescription);
            }

            public void OnEventDidTrigger(RuleCalculateWeaponStats evt)
            {
            }
        }


        [ComponentName("Replace attack stat if has parametrized feature")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class AttackStatReplacementIfHasParametrizedFeature : RuleInitiatorLogicComponent<RuleCalculateAttackBonusWithoutTarget>
        {
            public StatType ReplacementStat;
            public BlueprintParametrizedFeature feature;

            public override void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
            {
                ModifiableValueAttributeStat stat1 = this.Owner.Stats.GetStat(evt.AttackBonusStat) as ModifiableValueAttributeStat;
                ModifiableValueAttributeStat stat2 = this.Owner.Stats.GetStat(this.ReplacementStat) as ModifiableValueAttributeStat;
                bool flag = stat2 != null && stat1 != null && stat2.Bonus >= stat1.Bonus;

                if (flag
                    && this.Owner.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => p.Blueprint == feature).Any(p => p.Param == evt.Weapon.Blueprint.Category))
                {
                    evt.AttackBonusStat = this.ReplacementStat;
                }
            }

            public override void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt)
            {
            }
        }





        [ComponentName("Replace attack stat for specific weapon")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class AttackStatReplacementForWeaponCategory : RuleInitiatorLogicComponent<RuleCalculateAttackBonusWithoutTarget>
        {
            public StatType ReplacementStat;
            public WeaponCategory[] categories;

            public override void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
            {
                ModifiableValueAttributeStat stat1 = this.Owner.Stats.GetStat(evt.AttackBonusStat) as ModifiableValueAttributeStat;
                ModifiableValueAttributeStat stat2 = this.Owner.Stats.GetStat(this.ReplacementStat) as ModifiableValueAttributeStat;
                bool flag = stat2 != null && stat1 != null && stat2.Bonus >= stat1.Bonus;

                if (flag && (categories.Contains(evt.Weapon.Blueprint.Category) || categories.Empty()))
                {
                    evt.AttackBonusStat = this.ReplacementStat;
                }
            }

            public override void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt)
            {
            }
        }


        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ContextACBonusAgainstFactOwner : OwnedGameLogicComponent<UnitDescriptor>, ITargetRulebookHandler<RuleAttackRoll>, IRulebookHandler<RuleAttackRoll>, ITargetRulebookSubscriber
        {
            public BlueprintUnitFact CheckedFact;
            public ContextValue Bonus;
            public ModifierDescriptor Descriptor;
            public AlignmentComponent Alignment;

            public void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
                if (!evt.Initiator.Descriptor.HasFact(this.CheckedFact) || !evt.Initiator.Descriptor.Alignment.Value.HasComponent(this.Alignment))
                    return;
                int bonus = Bonus.Calculate(this.Fact.MaybeContext);
                evt.AddTemporaryModifier(evt.Target.Stats.AC.AddModifier(bonus, (GameLogicComponent)this, this.Descriptor));
            }

            public void OnEventDidTrigger(RuleAttackRoll evt)
            {
            }
        }


        [AllowMultipleComponents]
        [ComponentName("Saving throw bonus against fact")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ContextSavingThrowBonusAgainstFact : RuleInitiatorLogicComponent<RuleSavingThrow>
        {
            public BlueprintFeature CheckedFact;
            public ModifierDescriptor Descriptor;
            public ContextValue Bonus;
            public AlignmentComponent Alignment;

            public override void OnEventAboutToTrigger(RuleSavingThrow evt)
            {
                UnitDescriptor descriptor = evt.Reason.Caster?.Descriptor;
                if (descriptor == null || !descriptor.HasFact((BlueprintUnitFact)this.CheckedFact) || !descriptor.Alignment.Value.HasComponent(this.Alignment))
                    return;
                int bonus = Bonus.Calculate(this.Fact.MaybeContext);
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveWill.AddModifier(bonus, (GameLogicComponent)this, this.Descriptor));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveReflex.AddModifier(bonus, (GameLogicComponent)this, this.Descriptor));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveFortitude.AddModifier(bonus, (GameLogicComponent)this, this.Descriptor));
            }

            public override void OnEventDidTrigger(RuleSavingThrow evt)
            {
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class AttackBonusOnAttacksOfOpportunity : RuleInitiatorLogicComponent<RuleAttackWithWeapon>
        {
            public ContextValue Value;
            public ModifierDescriptor Descriptor;

            public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
            {
                if (!evt.IsAttackOfOpportunity)
                    return;
                evt.AddTemporaryModifier(evt.Target.Stats.AdditionalAttackBonus.AddModifier(this.Value.Calculate(this.Fact.MaybeContext), (GameLogicComponent)this, this.Descriptor));
            }

            public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
            }

        }


        [ComponentName("Maneuver Defence Bonus")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ContextManeuverDefenceBonus : RuleTargetLogicComponent<RuleCalculateCMD>
        {
            public CombatManeuver Type;
            public ContextValue Bonus;

            public override void OnEventAboutToTrigger(RuleCalculateCMD evt)
            {
                if (evt.Type != this.Type && this.Type != CombatManeuver.None)
                    return;
                evt.AddBonus(this.Bonus.Calculate(this.Fact.MaybeContext), this.Fact);
            }

            public override void OnEventDidTrigger(RuleCalculateCMD evt)
            {
            }
        }



        [ComponentName("Maneuver Bonus")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ContextCombatManeuverBonus : RuleInitiatorLogicComponent<RuleCombatManeuver>
        {
            public CombatManeuver Type;
            public ContextValue Bonus;

            public override void OnEventAboutToTrigger(RuleCombatManeuver evt)
            {
                if (evt.Type != this.Type && this.Type != CombatManeuver.None)
                    return;
                evt.AddBonus(this.Bonus.Calculate(this.Fact.MaybeContext), this.Fact);
            }

            public override void OnEventDidTrigger(RuleCombatManeuver evt)
            {
            }
        }


        public class ContextConditionCasterAndTargetHasFactFromList : ContextCondition
        {
            public BlueprintUnitFact[] facts;

            protected override string GetConditionCaption()
            {
                return string.Empty;
            }

            protected override bool CheckCondition()
            {
                foreach (var f in facts)
                {
                    if (this.Target.Unit.Descriptor.HasFact(f) && this.Context.MaybeCaster.Descriptor.HasFact(f))
                    {
                        return !Not;
                    }
                }
                return Not;
            }
        }



        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class DoubleWeaponSize : RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
        {
            public WeaponCategory[] categories;

            public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
            {
                if (!categories.Empty() && !categories.Contains(evt.Weapon.Blueprint.Category))
                {
                    return;
                }

                if (evt.DoNotScaleDamage)
                {
                    return;
                }
                var wielder_size = evt.Initiator.Descriptor.State.Size;
                evt.DoNotScaleDamage = true;

                //scale weapon to the wielder size if need (note polymophs do not change their size, so their weapon dice is not supposed to scale)
                var base_weapon_dice = evt.Initiator.Body.IsPolymorphed ? evt.Weapon.Blueprint.Damage : evt.Weapon.Blueprint.ScaleDamage(wielder_size);
                DiceFormula baseDice = !evt.WeaponDamageDiceOverride.HasValue ? base_weapon_dice : evt.WeaponDamageDiceOverride.Value;


                if (wielder_size == Size.Colossal || wielder_size == Size.Gargantuan)
                {
                    //double damage dice
                    DiceFormula double_damage = new DiceFormula(2 * baseDice.Rolls, baseDice.Dice);
                    evt.WeaponDamageDiceOverride = new DiceFormula?(double_damage);
                }
                else
                {
                    evt.WeaponDamageDiceOverride = new DiceFormula?(WeaponDamageScaleTable.Scale(baseDice, wielder_size + 2, wielder_size, evt.Weapon.Blueprint));
                }
            }

            public override void OnEventDidTrigger(RuleCalculateWeaponStats evt)
            {
            }
        }


        public class ContextActionAttack : ContextAction
        {
            public ActionList action_on_success = null;
            public ActionList action_on_miss = null;
            public override string GetCaption()
            {
                return string.Format("Caster attack");
            }

            public override void RunAction()
            {
                UnitEntityData maybeCaster = this.Context.MaybeCaster;
                if (maybeCaster == null)
                {
                    UberDebug.LogError((object)"Caster is missing", (object[])Array.Empty<object>());
                }
                else
                {
                    var target = this.Target;
                    if (target == null)
                        return;
                    // UnitAttack attack = new UnitAttack(this.Target.Unit);
                    // attack.IgnoreCooldown(null);
                    // maybeCaster.Commands.AddToQueueFirst(attack);

                    RuleAttackWithWeapon attackWithWeapon = new RuleAttackWithWeapon(maybeCaster, target.Unit, maybeCaster.Body.PrimaryHand.MaybeWeapon, 0);
                    attackWithWeapon.Reason = (RuleReason)this.Context;
                    RuleAttackWithWeapon rule = attackWithWeapon;
                    this.Context.TriggerRule<RuleAttackWithWeapon>(rule);
                    if (rule.AttackRoll.IsHit)
                    {
                        action_on_success?.Run();
                    }
                    else
                    {
                        action_on_miss?.Run();
                    }
                }
            }
        }


        public class ContextActionForceAttack : ContextAction
        {
            public override string GetCaption()
            {
                return string.Format("Caster force attack");
            }

            public override void RunAction()
            {
                UnitEntityData maybeCaster = this.Context.MaybeCaster;
                if (maybeCaster == null)
                {
                    UberDebug.LogError((object)"Caster is missing", (object[])Array.Empty<object>());
                }
                else
                {
                    var target = this.Target;
                    if (target == null)
                        return;

                    if (isInThreatRange(maybeCaster.Descriptor.Unit, target.Unit))
                    {
                        RuleAttackWithWeapon attackWithWeapon = new RuleAttackWithWeapon(maybeCaster, target.Unit, maybeCaster.Body.PrimaryHand.MaybeWeapon, 0);
                        attackWithWeapon.Reason = (RuleReason)this.Context;
                        RuleAttackWithWeapon rule = attackWithWeapon;
                        this.Context.TriggerRule<RuleAttackWithWeapon>(rule);
                    }
                    else
                    {
                        var attack_command = new UnitAttack(target.Unit);
                        attack_command.Init(maybeCaster);
                        attack_command.IgnoreCooldown(Game.Instance.TimeController.GameTime + 1.Rounds().Seconds);
                        maybeCaster.Commands.AddToQueueFirst(attack_command);
                    }
                }
            }


            private bool isInThreatRange(UnitEntityData unit, UnitEntityData enemy)
            {
                WeaponSlot threatHand = unit.Body?.PrimaryHand;
                if (threatHand == null || !unit.IsReach(enemy, threatHand))
                    return false;

                return true;
            }
        }


        public class AttackAnimation : BlueprintComponent, IAbilityCustomAnimation
        {
            public UnitAnimationAction GetAbilityAction(UnitEntityData caster)
            {
                //Main.logger.Log("here " + $"{ caster.Descriptor.Unit.View.AnimationManager.CreateHandle(UnitAnimationType.MainHandAttack).Action != null}");
                return caster.Descriptor.Unit.View.AnimationManager.CreateHandle(UnitAnimationType.MainHandAttack).Action;
            }
        }


        [ComponentName("Reduces DR against fact owner")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ReduceDRForFactOwner : RuleTargetLogicComponent<RuleCalculateDamage>
        {
            public int Reduction;
            public BlueprintFeature CheckedFact;
            public AttackType[] attack_types;

            public override void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {
                if (evt.DamageBundle.Weapon == null || evt.DamageBundle.WeaponDamage == null || evt.Initiator != this.Owner.Unit
                    || !evt.Initiator.Descriptor.HasFact(CheckedFact) || !attack_types.Contains(evt.DamageBundle.Weapon.Blueprint.AttackType))
                    return;

                evt.DamageBundle.WeaponDamage.SetReductionPenalty(this.Reduction);
            }

            public override void OnEventDidTrigger(RuleCalculateDamage evt)
            {
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class FavoredTerrainBonus : BuffLogic, IAreaLoadingStagesHandler, IGlobalSubscriber
        {
            public LootSetting[] Settings;
            private ModifiableValue.Modifier m_InitiativeModifier;
            private ModifiableValue.Modifier m_PerceptionModifier;
            private ModifiableValue.Modifier m_StealthModifier;
            private ModifiableValue.Modifier m_LoreNatureModifier;
            public ContextValue Value;

            public override void OnTurnOn()
            {
                base.OnTurnOn();
                this.CheckSettings();
                this.Owner.Ensure<UnitPartFavoredTerrain>().AddEntry(this.Settings, this.Fact);
            }

            public override void OnTurnOff()
            {
                base.OnTurnOff();
                this.DeactivateModifier();
                this.Owner.Ensure<UnitPartFavoredTerrain>().RemoveEntry(this.Fact);
            }

            public void CheckSettings()
            {
                BlueprintArea currentlyLoadedArea = Game.Instance.CurrentlyLoadedArea;
                if (currentlyLoadedArea != null && ((IEnumerable<LootSetting>)this.Settings).Contains<LootSetting>(currentlyLoadedArea.LootSetting))
                    this.ActivateModifier();
                else
                    this.DeactivateModifier();
            }

            public void ActivateModifier()
            {
                int value = Value.Calculate(this.Fact.MaybeContext);
                if (this.m_InitiativeModifier == null)
                    this.m_InitiativeModifier = this.Owner.Stats.Initiative.AddModifier(value, (GameLogicComponent)this, ModifierDescriptor.None);
                if (this.m_PerceptionModifier == null)
                    this.m_PerceptionModifier = this.Owner.Stats.SkillPerception.AddModifier(value, (GameLogicComponent)this, ModifierDescriptor.None);
                if (this.m_StealthModifier == null)
                    this.m_StealthModifier = this.Owner.Stats.SkillStealth.AddModifier(value, (GameLogicComponent)this, ModifierDescriptor.None);
                if (this.m_LoreNatureModifier != null)
                    return;
                this.m_LoreNatureModifier = this.Owner.Stats.SkillLoreNature.AddModifier(value, (GameLogicComponent)this, ModifierDescriptor.None);
            }

            public void DeactivateModifier()
            {
                if (this.m_InitiativeModifier != null)
                {
                    if (this.m_InitiativeModifier != null)
                        this.m_InitiativeModifier.Remove();
                    this.m_InitiativeModifier = (ModifiableValue.Modifier)null;
                }
                if (this.m_PerceptionModifier != null)
                {
                    if (this.m_PerceptionModifier != null)
                        this.m_PerceptionModifier.Remove();
                    this.m_PerceptionModifier = (ModifiableValue.Modifier)null;
                }
                if (this.m_StealthModifier != null)
                {
                    if (this.m_StealthModifier != null)
                        this.m_StealthModifier.Remove();
                    this.m_StealthModifier = (ModifiableValue.Modifier)null;
                }
                if (this.m_LoreNatureModifier == null)
                    return;
                if (this.m_LoreNatureModifier != null)
                    this.m_LoreNatureModifier.Remove();
                this.m_LoreNatureModifier = (ModifiableValue.Modifier)null;
            }

            public void OnAreaScenesLoaded()
            {
            }

            public void OnAreaLoadingComplete()
            {
                this.CheckSettings();
            }
        }


        [ComponentName("Increase specific spells CL")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ContextIncreaseCasterLevelForSelectedSpells : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            public ContextValue value;
            public BlueprintAbility[] spells;
            public bool correct_dc = false;
            public int multiplier = 1;

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

            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                if (!spells.Contains(evt.Spell))
                {
                    return;
                }
                int bonus = this.value.Calculate(this.Context);
                evt.AddBonusCasterLevel(multiplier * bonus);

                if (!correct_dc)
                {
                    evt.AddBonusDC((multiplier * bonus / 2));
                }
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }


        public class ActivatableAbilityMainWeaponTypeAllowed : ActivatableAbilityRestriction
        {
            public BlueprintWeaponType[] weapon_types;

            public override bool IsAvailable()
            {

                if (weapon_types == null || weapon_types.Empty())
                {
                    return true;
                }

                var weapon = Owner.Body.PrimaryHand.HasWeapon ? Owner.Body.PrimaryHand.MaybeWeapon : Owner.Body.EmptyHandWeapon;
                if (weapon == null)
                {
                    return false;
                }

                return weapon_types.Contains(weapon.Blueprint.Type);
            }
        }


        public class ActivatableAbilityMainWeaponCategoryAllowed : ActivatableAbilityRestriction
        {
            public WeaponCategory[] categories;

            public override bool IsAvailable()
            {

                if (categories == null || categories.Empty())
                {
                    return true;
                }

                var weapon = Owner.Body.PrimaryHand.HasWeapon ? Owner.Body.PrimaryHand.MaybeWeapon : Owner.Body.EmptyHandWeapon;
                if (weapon == null)
                {
                    return false;
                }

                return categories.Contains(weapon.Blueprint.Category);
            }
        }


        [ComponentName("Add stat bonus if owner has shield")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class AddStatBonusIfHasShield : OwnedGameLogicComponent<UnitDescriptor>, IUnitActiveEquipmentSetHandler, IUnitEquipmentHandler, IGlobalSubscriber
        {
            private ModifiableValue.Modifier m_Modifier;
            public ContextValue value;
            public ModifierDescriptor descriptor;
            public StatType stat;

            public override void OnTurnOn()
            {
                this.CheckShield();
            }

            public override void OnTurnOff()
            {
                this.DeactivateModifier();
            }

            public void HandleUnitChangeActiveEquipmentSet(UnitDescriptor unit)
            {
                this.CheckShield();
            }

            public void CheckShield()
            {
                if (this.Owner.Body.SecondaryHand.HasShield)
                    this.ActivateModifier();
                else
                    this.DeactivateModifier();
            }

            public void ActivateModifier()
            {
                if (this.m_Modifier != null)
                    return;
                this.m_Modifier = this.Owner.Stats.GetStat(stat).AddModifier(value.Calculate(this.Fact.MaybeContext), this, descriptor);
            }

            public void DeactivateModifier()
            {
                if (this.m_Modifier != null)
                    this.m_Modifier.Remove();
                this.m_Modifier = (ModifiableValue.Modifier)null;
            }

            public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
            {
                if (slot.Owner != this.Owner)
                    return;
                this.CheckShield();
            }
        }


        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class AddFeatureIfHasFactsFromList : OwnedGameLogicComponent<UnitDescriptor>, IUnitGainLevelHandler, IGlobalSubscriber
        {
            public BlueprintUnitFact[] CheckedFacts;
            public BlueprintUnitFact Feature;
            public int amount = 1;
            [JsonProperty]
            private Fact m_AppliedFact;

            public override void OnFactActivate()
            {
                this.Apply();
            }

            public override void OnFactDeactivate()
            {
                this.Owner.RemoveFact(this.m_AppliedFact);
                this.m_AppliedFact = (Fact)null;
            }

            public void HandleUnitGainLevel(UnitDescriptor unit, BlueprintCharacterClass @class)
            {
                this.Apply();
            }

            private void Apply()
            {
                if (this.m_AppliedFact != null)
                    return;

                int facts_found = 0;

                foreach (var f in CheckedFacts)
                {
                    if (facts_found == amount)
                    {
                        break;
                    }
                    if (this.Owner.HasFact(f))
                    {
                        facts_found++;
                    }
                }

                if (facts_found >= amount)
                {
                    this.m_AppliedFact = this.Owner.AddFact(this.Feature, (MechanicsContext)null, (FeatureParam)null);
                }
            }
        }


        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class AddFeatureIfHasFactAndNotHasFact : OwnedGameLogicComponent<UnitDescriptor>, IUnitGainLevelHandler, IGlobalSubscriber
        {
            public BlueprintUnitFact HasFact;
            public BlueprintUnitFact NotHasFact;
            public BlueprintUnitFact Feature;

            [JsonProperty]
            private Fact m_AppliedFact;

            public override void OnFactActivate()
            {
                this.Apply();
            }

            public override void OnFactDeactivate()
            {
                this.Owner.RemoveFact(this.m_AppliedFact);
                this.m_AppliedFact = (Fact)null;
            }

            public void HandleUnitGainLevel(UnitDescriptor unit, BlueprintCharacterClass @class)
            {
                this.Apply();
            }

            private void Apply()
            {
                if (this.m_AppliedFact != null || !this.Owner.HasFact(this.HasFact) || this.Owner.HasFact(this.NotHasFact))
                    return;
                this.m_AppliedFact = this.Owner.AddFact(this.Feature, (MechanicsContext)null, (FeatureParam)null);
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        public class AbilityShowIfCasterHasFacts : BlueprintComponent, IAbilityVisibilityProvider
        {
            public BlueprintUnitFact[] UnitFacts;

            public bool IsAbilityVisible(AbilityData ability)
            {
                foreach (var fact in UnitFacts)
                {
                    if (!ability.Caster.Progression.Features.HasFact(fact))
                    {
                        return false;
                    }
                }
                return true;
            }
        }


        [ComponentName("Buff remove on save")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class BuffRemoveOnSave : BuffLogic, ITickEachRound
        {
            public SavingThrowType SaveType;
            public void OnNewRound()
            {

                Rulebook rulebook = Game.Instance.Rulebook;
                RuleSavingThrow ruleSavingThrow = new RuleSavingThrow(this.Owner.Unit, this.SaveType, this.Buff.Context.Params.DC);
                ruleSavingThrow.Reason = (RuleReason)this.Fact;
                RuleSavingThrow evt = ruleSavingThrow;
                if (!rulebook.TriggerEvent<RuleSavingThrow>(evt).IsPassed)
                    return;
                this.Buff.Remove();
            }

            public override void OnTurnOn()
            {
            }

            public override void OnTurnOff()
            {
            }
        }


        [AllowMultipleComponents]
        [ComponentName("Predicates/Target point has no specified area effect")]
        [AllowedOn(typeof(BlueprintAbility))]
        public class AbilityTargetPointDoesNotContainAreaEffect : BlueprintComponent, IAbilityTargetChecker
        {
            public BlueprintAbilityAreaEffect area_effect;
            public float corpulence = 0.5f;

            public bool CanTarget(UnitEntityData caster, TargetWrapper target)
            {
                foreach (AreaEffectEntityData areaEffect in Game.Instance.State.AreaEffects)
                {
                    if (areaEffect.View.Shape != null && areaEffect.View.Shape.Contains(target.Point, corpulence) && areaEffect.Blueprint == area_effect)
                    {
                        return false;
                    }
                }

                return true;
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilityCasterPrimaryHandFree : BlueprintComponent, IAbilityCasterChecker
        {

            public bool CorrectCaster(UnitEntityData caster)
            {
                return !caster.Body.PrimaryHand.HasItem;
            }

            public string GetReason()
            {
                return (string)LocalizedTexts.Instance.Reasons.SpecificWeaponRequired;
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilityTargetPrimaryHandFree : BlueprintComponent, IAbilityTargetChecker
        {

            public bool CanTarget(UnitEntityData caster, TargetWrapper target)
            {
                if (target?.Unit == null)
                {
                    return false;
                }
                return !target.Unit.Body.PrimaryHand.HasItem;
            }

            public string GetReason()
            {
                return "Need free primary hand.";
            }
        }


        public class ContextConditionTargetHasMetalArmor : ContextCondition
        {
            static BlueprintCharacterClass druid = Main.library.Get<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");
            protected override string GetConditionCaption()
            {
                return string.Empty;
            }

            protected override bool CheckCondition()
            {
                var unit = this.Target?.Unit;
                if (unit == null)
                {
                    return false;
                }

                var armor = unit.Body?.Armor?.MaybeArmor;
                if (armor == null)
                {
                    return false;
                }

                return armor.Blueprint.GetComponents<EquipmentRestrictionClass>().Where(c => c.Not && c.Class == druid).Count() != 0;
            }
        }


        public class AddIncomingDamageTriggerOnAttacker : OwnedGameLogicComponent<UnitDescriptor>, ITargetRulebookHandler<RuleDealDamage>, ITargetRulebookHandler<RuleDealStatDamage>, ITargetRulebookHandler<RuleDrainEnergy>, IRulebookHandler<RuleDealDamage>, ITargetRulebookSubscriber, IRulebookHandler<RuleDealStatDamage>, IRulebookHandler<RuleDrainEnergy>
        {
            public ActionList Actions;
            public bool TriggerOnStatDamageOrEnergyDrain;
            public bool reduce_below0;
            public int min_dmg = 1;
            public bool on_self = false;

            public bool only_from_weapon = false;
            public bool only_from_spell = false;
            public bool consider_damage_type = false;

            public DamageEnergyType[] energy_types;
            public PhysicalDamageForm[] physical_types;

            private void RunAction(TargetWrapper target)
            {
                if (!this.Actions.HasActions)
                    return;
                (this.Fact as IFactContextOwner)?.RunActionInContext(this.Actions, target);
            }

            public void OnEventAboutToTrigger(RuleDealDamage evt)
            {
            }

            public void OnEventDidTrigger(RuleDealDamage evt)
            {
                var spellbook = Helpers.GetMechanicsContext()?.SourceAbilityContext?.Ability.Spellbook;
                if (only_from_spell && spellbook == null)
                {
                    return;
                }
                if (only_from_weapon && !(evt.Reason?.Rule is RuleAttackWithWeapon))
                {
                    return;
                }

                int received_damage = 0;


                if (!consider_damage_type)
                {
                    received_damage = evt.Damage;
                }
                else
                {
                    foreach (var d in evt.Calculate.CalculatedDamage)
                    {
                        var energy_damage = (d.Source as EnergyDamage);
                        var physical_damage = (d.Source as PhysicalDamage);

                        if (energy_damage != null && energy_types.Contains(energy_damage.EnergyType))
                        {
                            received_damage += d.FinalValue;
                        }
                        if (physical_damage != null && physical_damage.Form.HasValue && physical_types.Contains(physical_damage.Form.Value))
                        {
                            received_damage += d.FinalValue;
                        }
                    }
                }

                if (received_damage <= min_dmg)
                {
                    return;
                }

                if ((evt.Target.Damage + evt.Target.HPLeft < 0 || evt.Target.HPLeft > 0) && reduce_below0)
                {
                    return;
                }

                this.RunAction(on_self ? evt.Target : evt.Initiator);

            }

            public void OnEventAboutToTrigger(RuleDealStatDamage evt)
            {
            }

            public void OnEventDidTrigger(RuleDealStatDamage evt)
            {
                if (!this.TriggerOnStatDamageOrEnergyDrain)
                    return;

                if (reduce_below0 && !evt.Target.Descriptor.State.MarkedForDeath)
                {
                    return;
                }
                this.RunAction(on_self ? evt.Target : evt.Initiator);
            }

            public void OnEventAboutToTrigger(RuleDrainEnergy evt)
            {
            }

            public void OnEventDidTrigger(RuleDrainEnergy evt)
            {
                if (!this.TriggerOnStatDamageOrEnergyDrain)
                    return;
                if (reduce_below0 && !evt.Target.Descriptor.State.MarkedForDeath)
                {
                    return;
                }
                this.RunAction(on_self ? evt.Target : evt.Initiator);
            }
        }



        public class ActionOnSpellDamage : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ITargetRulebookSubscriber
        {
            public ActionList action;
            public int min_dmg = 1;
            public bool only_critical;

            public SavingThrowType save_type = SavingThrowType.Unknown;
            public SpellDescriptorWrapper descriptor;

            public void OnEventAboutToTrigger(RuleDealDamage evt)
            {
            }

            public void OnEventDidTrigger(RuleDealDamage evt)
            {
                if (only_critical && (evt.AttackRoll == null || !evt.AttackRoll.IsCriticalConfirmed))
                {
                    return;
                }
                //Main.logger.Log("Checking " + evt.Target.CharacterName);
                var context = Helpers.GetMechanicsContext();
                var spellContext = context?.SourceAbilityContext;
                var target = evt.Target;
                if (spellContext == null || target == null)
                {
                    return;
                }
                var spellbook = spellContext.Ability.Spellbook;
                if (spellbook == null)
                {
                    return;
                }

                if (descriptor.Value != SpellDescriptor.None && !descriptor.HasAnyFlag(spellContext.SpellDescriptor))
                {
                    return;
                }



                if (evt.Damage <= min_dmg)
                {
                    return;
                }

                var dc = context.Params.DC;

                if (save_type != SavingThrowType.Unknown)
                {
                    var rule_saving_throw = new RuleSavingThrow(target, save_type, dc);

                    Rulebook.Trigger(rule_saving_throw);
                    if (rule_saving_throw.IsPassed)
                    {
                        return;
                    }
                }

                if (!this.action.HasActions)
                    return;
                (this.Fact as IFactContextOwner)?.RunActionInContext(this.action, target);
            }


        }



        [ComponentName("BuffStackingBonus")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class AddStackingStatBonusToModifierFromFact : ContextAction
        {
            public ModifierDescriptor Descriptor;
            public ContextDiceValue dice_value;
            public StatType stat;
            public int max_value = 0;
            public BlueprintUnitFact storing_fact;
            public bool set = false;

            public override string GetCaption()
            {
                return "";
            }

            public override void RunAction()
            {
                ModifiableValue target_stat = null;
                if (stat == StatType.TemporaryHitPoints)
                {
                    target_stat = this.Target?.Unit?.Descriptor.Stats.TemporaryHitPoints;
                }
                else
                {
                    target_stat = this.Target?.Unit?.Descriptor.Stats.GetStat(stat);
                }
                if (target_stat == null)
                {
                    return;
                }

                var modifier = target_stat.Modifiers.Where(m => m.Source?.Blueprint == this.storing_fact && m.ModDescriptor == Descriptor).FirstOrDefault();
                if (modifier == null)
                {
                    return;
                }
                else
                {
                    modifier.ModValue = set ? dice_value.Calculate(this.Context) : modifier.ModValue + dice_value.Calculate(this.Context);
                    if (modifier.ModValue > max_value && max_value != 0)
                    {
                        modifier.ModValue = max_value;
                    }
                    this.Target?.Unit?.Descriptor.Stats.GetStat(stat).UpdateValue();
                }
            }
        }


        [AllowMultipleComponents]
        public class AddInitiatorAttackRollMissTrigger : GameLogicComponent, IInitiatorRulebookHandler<RuleAttackRoll>, IRulebookHandler<RuleAttackRoll>, IInitiatorRulebookSubscriber
        {
            [HideIf("CriticalHit")]
            public bool OnOwner;
            public bool CheckWeapon;
            [ShowIf("CheckWeapon")]
            public WeaponCategory WeaponCategory;
            public bool AffectFriendlyTouchSpells;
            public ActionList Action;

            public void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
            }

            public void OnEventDidTrigger(RuleAttackRoll evt)
            {
                if (!this.CheckConditions(evt))
                    return;
                using (new ContextAttackData(evt, (Projectile)null))
                {
                    if (this.OnOwner)
                    {
                        UnitDescriptor unitDescriptor = (this.Fact as OwnedFact<UnitDescriptor>)?.Owner ?? (this.Fact as ItemEnchantment)?.Owner.Wielder;
                        if (unitDescriptor != null)
                            (this.Fact as IFactContextOwner)?.RunActionInContext(this.Action, (TargetWrapper)unitDescriptor.Unit);
                        else
                            UberDebug.LogError((object)string.Format("Fact has no owner: {0}", (object)this.Fact), (object[])Array.Empty<object>());
                    }
                    else
                        (this.Fact as IFactContextOwner)?.RunActionInContext(this.Action, (TargetWrapper)evt.Target);
                }
            }

            private bool CheckConditions(RuleAttackRoll evt)
            {
                ItemEntity owner = (this.Fact as ItemEnchantment)?.Owner;
                ItemEntityWeapon weapon = (evt.Reason.Rule as RuleAttackWithWeapon)?.Weapon;
                return (owner == null || owner == weapon) && (!this.CheckWeapon || weapon != null && this.WeaponCategory == weapon.Blueprint.Category) && (!evt.IsHit) && ((this.AffectFriendlyTouchSpells || evt.Initiator.IsEnemy(evt.Target) || evt.AttackType != AttackType.Touch));
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class PersistentTemporaryHitPoints : OwnedGameLogicComponent<UnitDescriptor>, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ITargetRulebookSubscriber
        {
            public ModifierDescriptor Descriptor;
            public ContextValue Value;
            [JsonProperty]
            private ModifiableValue.Modifier m_Modifier;

            private MechanicsContext Context
            {
                get
                {
                    return this.Fact.MaybeContext;
                }
            }

            public override void OnFactActivate()
            {
                this.m_Modifier = this.Owner.Stats.TemporaryHitPoints.AddModifier(this.Value.Calculate(this.Context), (GameLogicComponent)this, this.Descriptor);
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
                if (this.m_Modifier.AppliedTo != null)
                    this.m_Modifier.Remove();
                this.m_Modifier = this.Owner.Stats.TemporaryHitPoints.AddModifier(0, (GameLogicComponent)this, this.Descriptor);
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class RerollOnStandardSingleAttack : RuleInitiatorLogicComponent<RuleRollD20>, ITargetRulebookSubscriber
        {
            public BlueprintParametrizedFeature[] required_features;

            private bool checkFeatures(WeaponCategory category)
            {
                if (required_features.Empty())
                {
                    return true;
                }

                bool ok = true;

                foreach (var f in required_features)
                {
                    ok = ok && this.Owner.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => p.Blueprint == f).Any(p => p.Param == category);
                }
                return ok;
            }

            public override void OnEventAboutToTrigger(RuleRollD20 evt)
            {
                var previous_event = Rulebook.CurrentContext.PreviousEvent;
                if (previous_event == null)
                {
                    return;
                }
                var attack_roll = previous_event as RuleAttackRoll;
                if (attack_roll == null || attack_roll.IsCriticalRoll)
                {
                    return;
                }

                var attack_with_weapon = attack_roll.RuleAttackWithWeapon;

                if (attack_with_weapon == null || attack_roll.Weapon == null)
                {
                    return;
                }

                if (!checkFeatures(attack_roll.Weapon.Blueprint.Category))
                {
                    return;
                }

                if (attack_with_weapon.IsCharge || attack_with_weapon.AttacksCount != 1 || attack_with_weapon.IsAttackOfOpportunity || attack_with_weapon.IsFullAttack)
                {
                    return;
                }

                if (previous_event != null && (previous_event is RuleAttackRoll) && !(previous_event as RuleAttackRoll).IsCriticalRoll)
                {
                    evt.SetReroll(1, true);
                }
            }

            public override void OnEventDidTrigger(RuleRollD20 evt)
            {

            }
        }



        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ContextActionOnSingleMeleeAttack : RuleInitiatorLogicComponent<RuleAttackWithWeapon>
        {
            public ActionList action;
            public AttackType[] allowed_attack_types;


            public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
            {

            }

            public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
                if (evt.IsCharge || evt.AttacksCount != 1 || evt.IsAttackOfOpportunity || evt.IsFullAttack)
                {
                    return;
                }

                if (allowed_attack_types.Contains(evt.AttackRoll.AttackType) && evt.AttackRoll.IsHit)
                {
                    using (new ContextAttackData(evt.AttackRoll, (Projectile)null))
                        (this.Fact as IFactContextOwner)?.RunActionInContext(this.action, (TargetWrapper)evt.Target);
                }
            }
        }


        public class PrerequisiteMatchingParamtrizedFeature : Prerequisite
        {
            public BlueprintParametrizedFeature base_feature;
            public BlueprintParametrizedFeature matching_feature;

            public override bool Check(
              FeatureSelectionState selectionState,
              UnitDescriptor unit,
              LevelUpState state)
            {
                if (selectionState != (FeatureSelectionState)null && selectionState.IsSelectedInChildren((IFeatureSelectionItem)this.matching_feature))
                    return false;

                foreach (var f in unit.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => p.Blueprint == base_feature))
                {
                    if (unit.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => p.Blueprint == matching_feature).Any(p => (p.Param == f.Param)))
                    {
                        return true;
                    }
                }

                return false;
            }

            public override string GetUIText()
            {
                return $"{matching_feature.Name} for {base_feature.Name}";
            }
        }




        public class ContextActionSpawnAreaEffectMultiple : ContextAction
        {
            public BlueprintAbilityAreaEffect AreaEffect;
            public ContextDurationValue DurationValue;
            public Vector2[] points_around_target;

            public override string GetCaption()
            {
                return string.Format("Spawn multiple {0} for {1}", !((UnityEngine.Object)this.AreaEffect != (UnityEngine.Object)null) ? (object)"<undefined>" : (object)this.AreaEffect.ToString(), (object)this.DurationValue);
            }

            public override void RunAction()
            {
                foreach (var p in points_around_target)
                {
                    var target = new TargetWrapper(this.Target.Point + p.To3D());
                    AreaEffectEntityData effectEntityData = AreaEffectsController.Spawn(this.Context, this.AreaEffect, target, new TimeSpan?(this.DurationValue.Calculate(this.Context).Seconds));
                    if (this.AbilityContext == null)
                        return;
                    foreach (UnitEntityData unit in Game.Instance.State.Units)
                    {
                        UnitEntityData u = unit;

                        if (!u.Descriptor.State.IsDead && u.IsInGame && (effectEntityData.View.Shape != null && effectEntityData.View.Shape.Contains(u.Position, u.View.Corpulence)) && (effectEntityData.AffectEnemies || !this.AbilityContext.Caster.IsEnemy(u)))
                            EventBus.RaiseEvent<IApplyAbilityEffectHandler>((Action<IApplyAbilityEffectHandler>)(h => h.OnTryToApplyAbilityEffect(this.AbilityContext, (TargetWrapper)u)));
                    }
                }
            }
        }



        public class AddInitiatorSavingThrowTrigger : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleSavingThrow>, IRulebookHandler<RuleSavingThrow>, IInitiatorRulebookSubscriber
        {
            public bool OnPass;
            public bool OnFail;
            public ActionList Action;

            public void OnEventAboutToTrigger(RuleSavingThrow evt)
            {
            }

            public void OnEventDidTrigger(RuleSavingThrow evt)
            {
                if (!this.CheckConditions(evt) || this.Fact.MaybeContext == null)
                    return;
                using (this.Fact.MaybeContext?.GetDataScope((TargetWrapper)this.Owner.Unit))
                    (this.Fact as IFactContextOwner)?.RunActionInContext(this.Action, (TargetWrapper)this.Owner.Unit);
            }

            private bool CheckConditions(RuleSavingThrow evt)
            {
                return (this.OnPass && evt.IsPassed) || (this.OnFail && !evt.IsPassed);
            }
        }


        [AllowMultipleComponents]
        public class AddTargetConcealmentRollTrigger : OwnedGameLogicComponent<UnitDescriptor>, ITargetRulebookHandler<RuleConcealmentCheck>, IRulebookHandler<RuleConcealmentCheck>, IInitiatorRulebookSubscriber
        {
            public bool only_on_miss;
            public bool only_on_success;

            public ActionList actions;
            public bool on_attacker = false;

            public void OnEventAboutToTrigger(RuleConcealmentCheck evt)
            {
            }

            public void OnEventDidTrigger(RuleConcealmentCheck evt)
            {
                if (evt.Success && only_on_miss)
                {
                    return;
                }

                if (!evt.Success && only_on_success)
                {
                    return;
                }

                if (on_attacker)
                {
                    using (this.Fact.MaybeContext?.GetDataScope((TargetWrapper)this.Owner.Unit))
                        (this.Fact as IFactContextOwner)?.RunActionInContext(this.actions, (TargetWrapper)evt.Initiator);
                }
                else
                {
                    using (this.Fact.MaybeContext?.GetDataScope((TargetWrapper)this.Owner.Unit))
                        (this.Fact as IFactContextOwner)?.RunActionInContext(this.actions, (TargetWrapper)evt.Target);
                }

            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class TargetWeaponSubCategoryAttackTrigger : OwnedGameLogicComponent<UnitDescriptor>, ITargetRulebookHandler<RuleAttackWithWeapon>, ITargetRulebookHandler<RuleAttackWithWeaponResolve>, IRulebookHandler<RuleAttackWithWeapon>, ITargetRulebookSubscriber, IRulebookHandler<RuleAttackWithWeaponResolve>
        {
            public bool WaitForAttackResolve;
            public bool CriticalHit;
            public bool OnlyMelee;
            public bool NotReach;
            [ShowIf("CheckCategory")]
            public WeaponSubCategory SubCategory;
            public ActionList ActionsOnAttacker;
            public ActionList ActionOnSelf;

            public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
            {
            }

            public void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
                if (this.WaitForAttackResolve)
                    return;
                this.TryRunActions(evt);
            }

            public void OnEventAboutToTrigger(RuleAttackWithWeaponResolve evt)
            {
            }

            public void OnEventDidTrigger(RuleAttackWithWeaponResolve evt)
            {
                if (!this.WaitForAttackResolve)
                    return;
                this.TryRunActions(evt.AttackWithWeapon);
            }

            private void TryRunActions(RuleAttackWithWeapon evt)
            {
                if (!this.CheckConditions(evt))
                    return;
                using (new ContextAttackData(evt.AttackRoll, (Projectile)null))
                {
                    (this.Fact as IFactContextOwner)?.RunActionInContext(this.ActionsOnAttacker, (TargetWrapper)evt.Initiator);
                    (this.Fact as IFactContextOwner)?.RunActionInContext(this.ActionOnSelf, (TargetWrapper)evt.Target);
                }
            }

            private bool CheckConditions(RuleAttackWithWeapon evt)
            {
                return evt.AttackRoll.IsHit && (!this.CriticalHit || evt.AttackRoll.IsCriticalConfirmed && !evt.AttackRoll.FortificationNegatesCriticalHit) && ((!this.OnlyMelee || evt.Weapon != null && evt.Weapon.Blueprint.IsMelee) && (!this.NotReach || evt.Weapon != null && !(evt.Weapon.Blueprint.Type.AttackRange > GameConsts.MinWeaponRange))) && (evt.Weapon != null && (evt.Weapon.Blueprint.Type.Category.HasSubCategory(SubCategory)));
            }
        }



        [ComponentName("Increase spell descriptor CL")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ContextIncreaseSpellDescriptorCasterLevel : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            public SpellDescriptorWrapper Descriptor;
            public ContextValue BonusCasterLevel;

            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                if (evt.Spellbook == null)
                {
                    return;
                }
                bool flag = false;
                foreach (SpellDescriptorComponent component in evt.Spell.GetComponents<SpellDescriptorComponent>())
                    flag = component.Descriptor.HasAnyFlag((SpellDescriptor)this.Descriptor);
                if (!flag)
                    return;
                evt.AddBonusCasterLevel(this.BonusCasterLevel.Calculate(this.Fact.MaybeContext));
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }


        public class AbilityAreaEffectRunActionWithFirstRound : AbilityAreaEffectLogic
        {
            public ActionList UnitEnter = Helpers.CreateActionList(null);
            public ActionList UnitExit = Helpers.CreateActionList(null);
            public ActionList UnitMove = Helpers.CreateActionList(null);
            public ActionList Round = Helpers.CreateActionList(null);
            public ActionList FirstRound = Helpers.CreateActionList(null);
            private int round_number = 0;

            protected override void OnUnitEnter(
              MechanicsContext context,
              AreaEffectEntityData areaEffect,
              UnitEntityData unit)
            {
                if (!this.UnitEnter.HasActions)
                    return;
                using (new AreaEffectContextData(areaEffect))
                {
                    using (context.GetDataScope((TargetWrapper)unit))
                        this.UnitEnter.Run();
                }
            }

            protected override void OnUnitExit(
              MechanicsContext context,
              AreaEffectEntityData areaEffect,
              UnitEntityData unit)
            {
                if (!this.UnitExit.HasActions)
                    return;
                using (new AreaEffectContextData(areaEffect))
                {
                    using (context.GetDataScope((TargetWrapper)unit))
                        this.UnitExit.Run();
                }
            }

            protected override void OnUnitMove(
              MechanicsContext context,
              AreaEffectEntityData areaEffect,
              UnitEntityData unit)
            {
                if (!this.UnitMove.HasActions)
                    return;
                using (new AreaEffectContextData(areaEffect))
                {
                    using (context.GetDataScope((TargetWrapper)unit))
                        this.UnitMove.Run();
                }
            }

            protected override void OnRound(MechanicsContext context, AreaEffectEntityData areaEffect)
            {
                if (!this.Round.HasActions)
                    return;
                using (new AreaEffectContextData(areaEffect))
                {
                    foreach (UnitEntityData unitEntityData in areaEffect.UnitsInside)
                    {
                        using (context.GetDataScope((TargetWrapper)unitEntityData))
                        {
                            if (!isFirstRound(areaEffect))
                            {
                                this.Round.Run();
                            }
                            else
                            {
                                this.FirstRound.Run();
                            }
                        }
                    }
                }
            }

            private bool isFirstRound(AreaEffectEntityData areaEffect)
            {
                return Helpers.GetField<TimeSpan>(areaEffect, "m_CreationTime").Add(new TimeSpan(0, 0, 1)) > Game.Instance.TimeController.GameTime;
            }
        }


        public class ContextConditionMainTargetHasFact : ContextCondition
        {
            public BlueprintUnitFact Fact;

            protected override string GetConditionCaption()
            {
                return string.Empty;
            }

            protected override bool CheckCondition()
            {
                if (this.Context.MainTarget?.Unit != null)
                    return this.Context.MainTarget.Unit.Descriptor.HasFact(this.Fact);
                UberDebug.LogError((UnityEngine.Object)this, (object)"Target is missing", (object[])Array.Empty<object>());
                return false;
            }
        }


        public class ContextActionOnMainTarget : ContextAction
        {
            public ActionList Actions;

            public override string GetCaption()
            {
                return "Run a context action on main target of that context";
            }

            public override void RunAction()
            {
                UnitEntityData maybeTarget = this.Context.MainTarget?.Unit;
                if (maybeTarget == null)
                    return;
                using (this.Context.GetDataScope((TargetWrapper)maybeTarget))
                    this.Actions.Run();
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class LearnSpellListToSpecifiedSpellbook : OwnedGameLogicComponent<UnitDescriptor>, ILevelUpCompleteUIHandler, IGlobalSubscriber
        {
            public BlueprintSpellbook spellbook;
            public BlueprintSpellList SpellList;

            public override void OnTurnOn()
            {
                this.LearnList();
            }

            public void HandleLevelUpComplete(UnitEntityData unit, bool isChargen)
            {
                this.LearnList();
            }

            public void LearnList()
            {
                Spellbook spellbook = this.Owner.DemandSpellbook(this.spellbook);
                foreach (SpellLevelList spellLevelList in this.SpellList.SpellsByLevel)
                {
                    if (spellbook.MaxSpellLevel >= spellLevelList.SpellLevel)
                    {
                        foreach (BlueprintAbility blueprintAbility in spellLevelList.SpellsFiltered)
                        {
                            BlueprintAbility spell = blueprintAbility;
                            if (spellbook.GetKnownSpells(spellLevelList.SpellLevel).All<AbilityData>((Func<AbilityData, bool>)(spellFromSpellbook => (UnityEngine.Object)spellFromSpellbook.Blueprint != (UnityEngine.Object)spell)))
                                spellbook.AddKnown(spellLevelList.SpellLevel, spell, false);
                        }
                    }
                }
            }
        }


        public class ContextConditionCompareTargetHPPercent : ContextCondition
        {
            public int Value;

            protected override string GetConditionCaption()
            {
                return string.Format("Target's HP% is less then {0}", (object)this.Value);
            }

            protected override bool CheckCondition()
            {
                if (this.Target.Unit == null)
                {
                    UberDebug.LogError((object)"Target unit is missing", (object[])Array.Empty<object>());
                    return false;
                }
                float part = (float)Value / 100.0f;
                return ((float)this.Target.Unit.HPLeft / (float)this.Target.Unit.MaxHP) < part;
            }
        }


        public class ContextConditionHasDamage : ContextCondition
        {
            protected override bool CheckCondition() => Target.Unit?.Damage > 0;

            protected override string GetConditionCaption() => "Whether the target is damaged";
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class SkillStatReplacement : OwnedGameLogicComponent<UnitDescriptor>
        {
            public StatType ReplacementStat;
            public StatType Skill;
            public ModifierDescriptor Descriptor = ModifierDescriptor.UntypedStackable;

            private ModifiableValue.Modifier m_Modifier;

            public override void OnTurnOn()
            {
                ModifiableValueSkill owner_skill = this.Owner.Stats.GetStat(Skill) as ModifiableValueSkill;
                ModifiableValueAttributeStat skill_stat = owner_skill.BaseStat;
                ModifiableValueAttributeStat replacement_stat = this.Owner.Stats.GetStat(ReplacementStat) as ModifiableValueAttributeStat;

                //Main.logger.Log(skill_stat.Bonus.ToString() + " : " + replacement_stat.Bonus.ToString());
                int bonus = replacement_stat.Bonus - skill_stat.Bonus;
                if (bonus <= 0)
                {
                    return;
                }
                this.m_Modifier = owner_skill.AddModifier(bonus, (GameLogicComponent)this, this.Descriptor);
            }

            public override void OnTurnOff()
            {
                if (this.m_Modifier != null)
                    this.m_Modifier.Remove();
                this.m_Modifier = (ModifiableValue.Modifier)null;
            }
        }





        [AllowedOn(typeof(BlueprintUnitFact))]
        public class AddStatDifferenceBonus : OwnedGameLogicComponent<UnitDescriptor>
        {
            public StatType ReplacementStat;
            public StatType OldStat;
            public StatType TargetStat;
            public ModifierDescriptor Descriptor = ModifierDescriptor.UntypedStackable;

            private ModifiableValue.Modifier m_Modifier;

            public override void OnTurnOn()
            {

                var target_stat = this.Owner.Stats.GetStat(TargetStat);
                ModifiableValueAttributeStat replacement_stat = this.Owner.Stats.GetStat(ReplacementStat) as ModifiableValueAttributeStat;
                ModifiableValueAttributeStat old_stat = this.Owner.Stats.GetStat(OldStat) as ModifiableValueAttributeStat;

                int bonus = replacement_stat.Bonus - old_stat.Bonus;
                if (bonus <= 0)
                {
                    return;
                }
                this.m_Modifier = target_stat.AddModifier(bonus, (GameLogicComponent)this, this.Descriptor);
            }

            public override void OnTurnOff()
            {
                if (this.m_Modifier != null)
                    this.m_Modifier.Remove();
                this.m_Modifier = (ModifiableValue.Modifier)null;
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class CasterLevelCheckBonus : RuleInitiatorLogicComponent<RuleSpellResistanceCheck>, IInitiatorRulebookHandler<RuleDispelMagic>
        {
            public ContextValue Value;

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

            public void OnEventAboutToTrigger(RuleDispelMagic evt)
            {
                int num = this.Value.Calculate(this.Context);
                evt.Bonus += num;
            }

            public override void OnEventAboutToTrigger(RuleSpellResistanceCheck evt)
            {
                int num = this.Value.Calculate(this.Context);
                evt.AdditionalSpellPenetration += num;
            }

            public void OnEventDidTrigger(RuleDispelMagic evt)
            {
            }

            public override void OnEventDidTrigger(RuleSpellResistanceCheck evt)
            {

            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class AddSkillPointOnEvenLevels : OwnedGameLogicComponent<UnitDescriptor>, ILevelUpSelectClassHandler, IGlobalSubscriber
        {
            public void HandleSelectClass(UnitDescriptor unit, LevelUpState state)
            {
                if (this.Owner != unit)
                    return;

                if ((state.NextLevel % 2) == 0)
                {
                    ++state.ExtraSkillPoints;
                }
            }
        }



        public class DemoralizeWithAction : ContextAction
        {
            public BlueprintBuff Buff;
            public BlueprintBuff GreaterBuff;
            public bool DazzlingDisplay;
            public BlueprintFeature SwordlordProwessFeature;
            public BlueprintFeature ShatterConfidenceFeature;
            public BlueprintBuff ShatterConfidenceBuff;
            public ActionList actions;

            public override string GetCaption()
            {
                return "Demoralize target";
            }

            public override void RunAction()
            {
                MechanicsContext context = ElementsContext.GetData<MechanicsContext.Data>()?.Context;
                UnitEntityData maybeCaster = context?.MaybeCaster;
                if (maybeCaster == null || !this.Target.IsUnit)
                {
                    UberDebug.LogError((UnityEngine.Object)this, (object)"Unable to apply buff: no context found", (object[])Array.Empty<object>());
                }
                else
                {
                    int dc = 10 + this.Target.Unit.Descriptor.Progression.CharacterLevel + this.Target.Unit.Stats.Wisdom.Bonus;
                    ModifiableValue.Modifier modifier = (ModifiableValue.Modifier)null;
                    try
                    {
                        if (this.DazzlingDisplay && (bool)maybeCaster.Descriptor.State.Features.SwordlordWeaponProwess)
                        {
                            int num = 0;
                            foreach (Feature feature in maybeCaster.Descriptor.Progression.Features)
                            {
                                FeatureParam featureParam = feature.Param;
                                WeaponCategory? nullable1;
                                WeaponCategory? nullable2;
                                if ((object)featureParam == null)
                                {
                                    nullable1 = new WeaponCategory?();
                                    nullable2 = nullable1;
                                }
                                else
                                    nullable2 = featureParam.WeaponCategory;
                                nullable1 = nullable2;
                                if ((nullable1.GetValueOrDefault() != WeaponCategory.DuelingSword ? 0 : (nullable1.HasValue ? 1 : 0)) != 0)
                                    ++num;
                            }
                            modifier = maybeCaster.Stats.CheckIntimidate.AddModifier(num, (GameLogicComponent)null, ModifierDescriptor.None);
                        }
                        RuleSkillCheck ruleSkillCheck = context.TriggerRule<RuleSkillCheck>(new RuleSkillCheck(maybeCaster, StatType.CheckIntimidate, dc));
                        if (!ruleSkillCheck.IsPassed)
                            return;
                        if (this.actions != null)
                        {
                            this.actions.Run();
                        }
                        int num1 = 1 + (ruleSkillCheck.RollResult - dc) / 5 + (!(bool)maybeCaster.Descriptor.State.Features.FrighteningThug ? 0 : 1);
                        if ((bool)maybeCaster.Descriptor.State.Features.FrighteningThug && num1 >= 4)
                            this.Target.Unit.Descriptor.AddBuff(this.GreaterBuff, context, new TimeSpan?(1.Rounds().Seconds));
                        Kingmaker.UnitLogic.Buffs.Buff buff1 = this.Target.Unit.Descriptor.AddBuff(this.Buff, context, new TimeSpan?(num1.Rounds().Seconds));
                        if (this.ShatterConfidenceFeature != null && !maybeCaster.Descriptor.HasFact(this.ShatterConfidenceFeature) || buff1 == null)
                            return;
                        Kingmaker.UnitLogic.Buffs.Buff buff2 = this.Target.Unit.Descriptor.AddBuff(this.ShatterConfidenceBuff, context, new TimeSpan?(num1.Rounds().Seconds));
                        buff1.StoreFact((Fact)buff2);
                    }
                    finally
                    {
                        modifier?.Remove();
                    }
                }
            }
        }

        public class ActionOnDemoralize : ContextAction
        {
            public BlueprintBuff Buff;
            public BlueprintBuff GreaterBuff;
            public ActionList actions;


            public override string GetCaption()
            {
                return "Demoralize target with action";
            }

            public override void RunAction()
            {
                MechanicsContext context = ElementsContext.GetData<MechanicsContext.Data>()?.Context;
                UnitEntityData maybeCaster = context?.MaybeCaster;
                if (maybeCaster == null || !this.Target.IsUnit)
                {
                    UberDebug.LogError((UnityEngine.Object)this, (object)"Unable to apply buff: no context found", (object[])Array.Empty<object>());
                }
                else
                {
                    int dc = 10 + this.Target.Unit.Descriptor.Progression.CharacterLevel + this.Target.Unit.Stats.Wisdom.Bonus;
                    try
                    {
                        RuleSkillCheck ruleSkillCheck = context.TriggerRule<RuleSkillCheck>(new RuleSkillCheck(maybeCaster, StatType.CheckIntimidate, dc));
                        if (!ruleSkillCheck.IsPassed)
                            return;
                        int num1 = 1 + (ruleSkillCheck.RollResult - dc) / 5 + (!(bool)maybeCaster.Descriptor.State.Features.FrighteningThug ? 0 : 1);
                        if ((bool)maybeCaster.Descriptor.State.Features.FrighteningThug && num1 >= 4)
                            this.Target.Unit.Descriptor.AddBuff(this.GreaterBuff, context, new TimeSpan?(1.Rounds().Seconds));
                        Kingmaker.UnitLogic.Buffs.Buff buff1 = this.Target.Unit.Descriptor.AddBuff(this.Buff, context, new TimeSpan?(num1.Rounds().Seconds));

                        if (this.actions != null)
                        {
                            this.actions.Run();
                        }
                    }
                    finally
                    {
                    }
                }
            }
        }


        public class ConsumeMoveAction : ContextAction
        {
            public override string GetCaption()
            {
                return "Consume move action";
            }


            public override void RunAction()
            {
                var unit = this.Target?.Unit;
                if (unit == null)
                {
                    return;
                }

                unit.CombatState.Cooldown.MoveAction += Math.Min(6f - unit.CombatState.Cooldown.MoveAction, 3f);
            }
        }


        public class ReduceHpToValue : ContextAction
        {
            public int value;
            bool remove_temporary_hp = true;

            public override string GetCaption()
            {
                return "Reduce HP to 0";
            }


            public override void RunAction()
            {
                var unit = this.Target?.Unit;
                if (unit == null)
                {
                    return;
                }
                if (remove_temporary_hp)
                {
                    unit.Descriptor.Stats.TemporaryHitPoints.RemoveAllModifiers();
                }

                var dmg = unit.HPLeft - value;
                var base_dmg = (BaseDamage)new EnergyDamage(new DiceFormula(dmg, DiceType.One), Kingmaker.Enums.Damage.DamageEnergyType.Unholy);
                base_dmg.IgnoreReduction = true;

                RuleDealDamage evt_dmg = new RuleDealDamage(this.Context.MaybeCaster, unit, new DamageBundle(new BaseDamage[1] { base_dmg }));
                evt_dmg.SourceAbility = Context?.SourceAbility;
                Rulebook.Trigger<RuleDealDamage>(evt_dmg);
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class RemoveShieldACBonus : RuleTargetLogicComponent<RuleCalculateAC>
        {
            public override void OnEventAboutToTrigger(RuleCalculateAC evt)
            {
                if (!this.Owner.Body.SecondaryHand.HasShield)
                    return;

                var mods = evt.Target.Stats.AC.Modifiers.Where(m => m.ModDescriptor == ModifierDescriptor.Shield || m.ModDescriptor == ModifierDescriptor.ShieldFocus || m.ModDescriptor == ModifierDescriptor.ShieldEnhancement).ToArray();
                var bonus = 0;

                foreach (var m in mods)
                {
                    bonus += m.ModValue;
                }
                if (bonus == 0)
                {
                    return;
                }

                evt.AddBonus(-bonus, this.Fact);
            }

            public override void OnEventDidTrigger(RuleCalculateAC evt)
            {
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class AttackBonusMaxFromMultipleStats : RuleInitiatorLogicComponent<RuleAttackRoll>
        {
            public StatType[] stats;
            public ModifierDescriptor descriptor;

            public override void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
                int bonus = 0;

                foreach (var s in stats)
                {
                    bonus = Math.Max(bonus, evt.Initiator.Stats.GetStat<ModifiableValueAttributeStat>(s).Bonus);
                }
                evt.AddTemporaryModifier(evt.Initiator.Stats.AdditionalAttackBonus.AddModifier(bonus, (GameLogicComponent)this, this.descriptor));
            }

            public override void OnEventDidTrigger(RuleAttackRoll evt)
            {
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilityTargetCasterHDDifference : BlueprintComponent, IAbilityTargetChecker
        {
            public int difference = 0;
            public int min_hd = 1;

            public bool CanTarget(UnitEntityData caster, TargetWrapper target)
            {
                UnitEntityData unit = target.Unit;
                if (unit == null)
                    return false;

                var target_hd = unit.Descriptor.Progression.CharacterLevel;
                if (target_hd <= min_hd)
                {
                    return true;
                }
                var caster_hd = caster.Descriptor.Progression.CharacterLevel;

                return caster_hd >= target_hd + difference;
            }
        }


        [ComponentName("Increase all spells DC")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class IncreaseAllSpellsDC : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            public ContextValue Value;

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

            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                if (evt.Spell.IsSpell)
                {
                    evt.AddBonusDC(this.Value.Calculate(this.Context));
                }
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }




        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ModifyD20WithActions : RuleInitiatorLogicComponent<RuleRollD20>
        {
            [SerializeField]
            [ShowIf("IsSavingThrow")]
            [EnumFlagsAsButtons(ColumnCount = 4)]
            private ModifyD20WithActions.InnerSavingThrowType m_SavingThrowType = ModifyD20WithActions.InnerSavingThrowType.All;
            [EnumFlagsAsButtons(ColumnCount = 3)]
            public RuleType Rule;
            public bool Replace;
            [HideIf("Replace")]
            public int RollsAmount;
            [HideIf("Replace")]
            public bool TakeBest;
            [ShowIf("Replace")]
            public int Roll;
            public bool WithChance;
            [ShowIf("WithChance")]
            [Tooltip("[0..100]")]
            public ContextValue Chance;
            [HideIf("IsInitiative")]
            public bool RerollOnlyIfFailed;
            public bool DispellOnRerollFinished;
            public bool DispellOn20;
            [ShowIf("IsSkillCheck")]
            public bool SpecificSkill;
            [ShowIf("SpecificSkill")]
            public StatType[] Skill;
            [ShowIf("IsSavingThrow")]
            public bool SpecificDescriptor;
            [ShowIf("SpecificDescriptor")]
            public SpellDescriptorWrapper SpellDescriptor;
            [ShowIf("IsSavingThrow")]
            public bool AddSavingThrowBonus;
            [ShowIf("AddSavingThrowBonus")]
            public ModifierDescriptor ModifierDescriptor;
            [ShowIf("AddSavingThrowBonus")]
            public ContextValue Value;
            [ShowIf("IsCombatManeuver")]
            public bool TandemTrip;
            [ShowIf("TandemTrip")]
            public BlueprintFeature TandemTripFeature;
            private RuleRollD20 m_Roll;

            public ActionList actions;

            private bool IsSkillCheck
            {
                get
                {
                    return (this.Rule & RuleType.SkillCheck) != (RuleType)0;
                }
            }

            private bool IsSavingThrow
            {
                get
                {
                    return (this.Rule & RuleType.SavingThrow) != (RuleType)0;
                }
            }

            private bool IsAttackRoll
            {
                get
                {
                    return (this.Rule & RuleType.AttackRoll) != (RuleType)0;
                }
            }

            private bool IsCombatManeuver
            {
                get
                {
                    return (this.Rule & RuleType.Maneuver) != (RuleType)0;
                }
            }

            private bool IsInitiative
            {
                get
                {
                    return (this.Rule & RuleType.Intiative) != (RuleType)0;
                }
            }

            private bool IsSpellResistanceCheck
            {
                get
                {
                    return (this.Rule & RuleType.SpellResistance) != (RuleType)0;
                }
            }

            public override void OnEventAboutToTrigger(RuleRollD20 evt)
            {
                RulebookEvent previousEvent = Rulebook.CurrentContext.PreviousEvent;
                if (previousEvent == null || !this.CheckRule(previousEvent))
                    return;
                int roll = 0;
                if (this.RerollOnlyIfFailed)
                    roll = evt.PreRollDice();
                if (this.RerollOnlyIfFailed && !ModifyD20WithActions.IsRollFailed(roll, previousEvent) || this.WithChance && UnityEngine.Random.Range(0, 101) >= this.Chance.Calculate(this.Fact.MaybeContext))
                    return;
                this.m_Roll = evt;
                if (this.AddSavingThrowBonus)
                {
                    CharacterStats stats = evt.Initiator.Stats;
                    int num = this.Value.Calculate(this.Fact.MaybeContext);
                    previousEvent.AddTemporaryModifier(stats.SaveWill.AddModifier(num, (GameLogicComponent)this, this.ModifierDescriptor));
                    previousEvent.AddTemporaryModifier(stats.SaveReflex.AddModifier(num, (GameLogicComponent)this, this.ModifierDescriptor));
                    previousEvent.AddTemporaryModifier(stats.SaveFortitude.AddModifier(num, (GameLogicComponent)this, this.ModifierDescriptor));
                }
                if (this.Replace)
                    evt.Override(this.Roll);
                else
                    evt.SetReroll(this.RollsAmount, this.TakeBest);
            }

            private static bool IsRollFailed(int roll, RulebookEvent evt)
            {
                bool? nullable1 = (evt as RuleAttackRoll)?.IsSuccessRoll(roll);
                int num;
                if ((!nullable1.HasValue ? 0 : (nullable1.Value ? 1 : 0)) == 0)
                {
                    bool? nullable2 = (evt as RuleSkillCheck)?.IsSuccessRoll(roll, 0);
                    if ((!nullable2.HasValue ? 0 : (nullable2.Value ? 1 : 0)) == 0)
                    {
                        bool? nullable3 = (evt as RuleSavingThrow)?.IsSuccessRoll(roll, 0);
                        if ((!nullable3.HasValue ? 0 : (nullable3.Value ? 1 : 0)) == 0)
                        {
                            bool? nullable4 = (evt as RuleCombatManeuver)?.IsSuccessRoll(roll);
                            num = !nullable4.HasValue ? 0 : (nullable4.Value ? 1 : 0);
                            goto label_5;
                        }
                    }
                }
                num = 1;
            label_5:
                return num == 0;
            }

            public override void OnEventDidTrigger(RuleRollD20 evt)
            {
                if (this.m_Roll != evt)
                    return;
                if (evt.Result == 20 && this.DispellOn20 || this.DispellOnRerollFinished)
                    this.Owner.RemoveFact(this.Fact);
                this.m_Roll = (RuleRollD20)null;

                (this.Fact as IFactContextOwner)?.RunActionInContext(this.actions, this.Owner.Unit);
            }

            private bool CheckRule(RulebookEvent rule)
            {
                if (this.IsAttackRoll && rule is RuleAttackRoll || (this.IsSuitableSkillCheck(rule) || this.IsSuitableSavingThrow(rule)) || (this.IsSuitableCombatManeuver(rule) || this.IsInitiative && rule is RuleInitiativeRoll))
                    return true;
                if (this.IsSpellResistanceCheck)
                    return rule is RuleSpellResistanceCheck;
                return false;
            }

            private bool IsSuitableSkillCheck(RulebookEvent rule)
            {
                RuleSkillCheck ruleSkillCheck = rule as RuleSkillCheck;
                if (!this.IsSkillCheck || ruleSkillCheck == null)
                    return false;
                if (this.SpecificSkill)
                    return ((IList<StatType>)this.Skill).HasItem<StatType>(ruleSkillCheck.StatType);
                return true;
            }

            private bool IsSuitableSavingThrow(RulebookEvent rule)
            {
                RuleSavingThrow ruleSavingThrow = rule as RuleSavingThrow;
                if (!this.IsSavingThrow || ruleSavingThrow == null)
                    return false;
                Kingmaker.Blueprints.Classes.Spells.SpellDescriptor? spellDescriptor = rule.Reason.Context?.SpellDescriptor;
                Kingmaker.Blueprints.Classes.Spells.SpellDescriptor descriptor1 = !spellDescriptor.HasValue ? Kingmaker.Blueprints.Classes.Spells.SpellDescriptor.None : spellDescriptor.Value;
                if (this.SpecificDescriptor && !descriptor1.Intersects((Kingmaker.Blueprints.Classes.Spells.SpellDescriptor)this.SpellDescriptor))
                    return false;
                return (this.m_SavingThrowType & ModifyD20WithActions.ConvertToInnerSavingThrowType(ruleSavingThrow.Type)) != (ModifyD20WithActions.InnerSavingThrowType)0;
            }

            private bool IsSuitableCombatManeuver(RulebookEvent rule)
            {
                RuleCombatManeuver evt = rule as RuleCombatManeuver;
                if (!this.IsCombatManeuver || evt == null)
                    return false;
                if (this.TandemTrip)
                    return this.CheckTandemTrip(evt);
                return true;
            }

            private static ModifyD20WithActions.InnerSavingThrowType ConvertToInnerSavingThrowType(
              SavingThrowType type)
            {
                switch (type)
                {
                    case SavingThrowType.Fortitude:
                        return ModifyD20WithActions.InnerSavingThrowType.Fortitude;
                    case SavingThrowType.Reflex:
                        return ModifyD20WithActions.InnerSavingThrowType.Reflex;
                    case SavingThrowType.Will:
                        return ModifyD20WithActions.InnerSavingThrowType.Will;
                    default:
                        return (ModifyD20WithActions.InnerSavingThrowType)0;
                }
            }

            private bool CheckTandemTrip(RuleCombatManeuver evt)
            {
                if (!evt.Target.CombatState.IsFlanked || evt.Type != CombatManeuver.Trip)
                    return false;
                bool flag = (bool)this.Owner.State.Features.SoloTactics;
                if (!flag)
                {
                    foreach (UnitEntityData unitEntityData in evt.Target.CombatState.EngagedBy)
                    {
                        flag = unitEntityData.Descriptor.HasFact((BlueprintUnitFact)this.TandemTripFeature) && unitEntityData != this.Owner.Unit;
                        if (flag)
                            break;
                    }
                }
                return flag;
            }

            public override void Validate(ValidationContext context)
            {
                base.Validate(context);
                if (this.Replace && this.Roll < 1)
                    context.AddError("Replace roll value must be > 0", (object[])Array.Empty<object>());
                if (!this.Replace && this.RollsAmount < 1)
                    context.AddError("Rolls amount must be > 0", (object[])Array.Empty<object>());
                if (!this.IsSavingThrow || this.m_SavingThrowType != (ModifyD20WithActions.InnerSavingThrowType)0)
                    return;
                context.AddError("No Saving Throw specified", (object[])Array.Empty<object>());
            }

            [Flags]
            private enum InnerSavingThrowType
            {
                Fortitude = 1,
                Reflex = 2,
                Will = 4,
                All = Will | Reflex | Fortitude, // 0x00000007
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class ActionOnNearMissIfHasFact : RuleTargetLogicComponent<RuleAttackWithWeapon>
        {
            [HideIf("RangedOnly")]
            public bool MeleeOnly;
            [HideIf("MeleeOnly")]
            public bool RangedOnly;
            public int HitAndArmorDifference;
            public ActionList Action;
            public bool OnAttacker;
            public BlueprintUnitFact checked_fact;

            public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
            {
            }

            public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
                if (!this.Check(evt) || evt.AttackRoll.IsHit || (checked_fact != null && !evt.Target.Descriptor.HasFact(checked_fact)))
                    return;
                //UberDebug.LogError((object)("MISS BY: " + (object)evt.AttackRoll.TargetAC + " " + (object)evt.AttackRoll.Roll + " " + (object)evt.AttackRoll.AttackBonus), (object[])Array.Empty<object>());
                if (evt.AttackRoll.TargetAC - evt.AttackRoll.Roll - evt.AttackRoll.AttackBonus > this.HitAndArmorDifference)
                    return;

                (this.Fact as IFactContextOwner)?.RunActionInContext(this.Action, (TargetWrapper)(!this.OnAttacker ? evt.Target : evt.Initiator));
            }

            private bool Check(RuleAttackWithWeapon evt)
            {
                return (!this.MeleeOnly || evt.Weapon.Blueprint.IsMelee) && (!this.RangedOnly || evt.Weapon.Blueprint.IsRanged);
            }
        }


        [AllowedOn(typeof(BlueprintBuff))]
        public class ACBonusAgainstTargetIfHasFact : BuffLogic, IInitiatorRulebookHandler<RuleAttackRoll>, IRulebookHandler<RuleAttackRoll>, IInitiatorRulebookSubscriber
        {
            public ContextValue Value;
            public bool CheckCaster;
            public bool CheckCasterFriend;
            public ModifierDescriptor Descriptor;
            public BlueprintUnitFact checked_fact;
            public bool only_melee = false;

            public void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
                UnitEntityData maybeCaster = this.Buff.Context.MaybeCaster;
                bool flag1 = this.CheckCaster && evt.Target == maybeCaster;
                bool flag2 = this.CheckCasterFriend && maybeCaster != null && evt.Target.GroupId == maybeCaster.GroupId && evt.Target != maybeCaster;
                if (!flag1 && !flag2)
                    return;
                if (!evt.Target.Descriptor.HasFact(checked_fact))
                {
                    return;
                }
                if (!evt.Weapon.Blueprint.IsMelee && only_melee)
                {
                    return;
                }
                evt.AddTemporaryModifier(evt.Target.Stats.AC.AddModifier(this.Value.Calculate(this.Buff.Context), (GameLogicComponent)this, this.Descriptor));
            }

            public void OnEventDidTrigger(RuleAttackRoll evt)
            {
            }

            public override void Validate(ValidationContext context)
            {
                base.Validate(context);
                if (this.CheckCaster || this.CheckCasterFriend)
                    return;
                context.AddError("CheckCaster or CheckCasterFriend must be true", (object[])Array.Empty<object>());
            }
        }


        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ACBonusSingleThreat : OwnedGameLogicComponent<UnitDescriptor>, ITargetRulebookHandler<RuleAttackRoll>, IRulebookHandler<RuleAttackRoll>, ITargetRulebookSubscriber
        {
            public int Bonus;
            public ModifierDescriptor Descriptor;

            public void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
                if (evt.Target.IsEngage(evt.Initiator) && evt.Initiator.CombatState.EngagedBy.Count == 1)
                {
                    evt.AddTemporaryModifier(evt.Target.Stats.AC.AddModifier(this.Bonus * this.Fact.GetRank(), (GameLogicComponent)this, this.Descriptor));
                }
            }

            public void OnEventDidTrigger(RuleAttackRoll evt)
            {
            }
        }


        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class FeatureReplacement : BlueprintComponent
        {
            public BlueprintFact replacement_feature;
        }


        [Harmony12.HarmonyPatch(typeof(FactCollection))]
        [Harmony12.HarmonyPatch("HasFact", Harmony12.MethodType.Normal)]
        [Harmony12.HarmonyPatch(new Type[] { typeof(BlueprintFact) })]
        class FactCollection__HasFact__Patch
        {

            static void Postfix(FactCollection __instance, ref bool __result, BlueprintFact blueprint)
            {
                if (!__result)
                {
                    foreach (var c in blueprint.GetComponents<FeatureReplacement>())
                    {
                        if (__instance.GetFact(c.replacement_feature) != null)
                        {
                            __result = true;
                            return;
                        }
                    }
                }
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        public class AbilityShowIfCasterHasFact : BlueprintComponent, IAbilityVisibilityProvider
        {
            public BlueprintUnitFact UnitFact;

            public bool IsAbilityVisible(AbilityData ability)
            {
                return ability.Caster.Progression.Features.HasFact((BlueprintFact)this.UnitFact) || ability.Caster.Buffs.HasFact(UnitFact);
            }
        }

        public class ContextConditionEngagedByCaster : ContextCondition
        {
            protected override string GetConditionCaption()
            {
                return string.Empty;
            }

            protected override bool CheckCondition()
            {
                var target = this.Target?.Unit;
                var caster = this.Context.MaybeCaster;

                if (target == null || caster == null)
                {
                    return false;
                }

                return caster.IsEngage(target);
            }
        }


        [AllowedOn(typeof(BlueprintBuff))]
        public class DamageBonusAgainstCaster : BuffLogic, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, IInitiatorRulebookSubscriber
        {
            public ContextValue Value;
            public bool ApplyToSpellDamage = false;

            public void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {
                UnitEntityData maybeCaster = this.Buff.Context.MaybeCaster;
                if (evt.Target != maybeCaster)
                    return;
                if (!this.ApplyToSpellDamage && evt.DamageBundle.Weapon == null)
                    return;

                evt.DamageBundle.First?.AddBonusTargetRelated(this.Value.Calculate(this.Buff.Context));
            }

            public void OnEventDidTrigger(RuleCalculateDamage evt)
            {
            }
        }





        public class ContextConditionHasArchetype : ContextCondition
        {
            public BlueprintArchetype archetype;
            protected override string GetConditionCaption()
            {
                return string.Empty;
            }

            protected override bool CheckCondition()
            {
                var target = this.Target?.Unit;

                if (target == null)
                {
                    return false;
                }
                return target.Descriptor.Progression.IsArchetype(archetype);
            }
        }



        public class addSpellChoice : OwnedGameLogicComponent<UnitDescriptor>, ILevelUpCompleteUIHandler
        {
            [JsonProperty]
            bool applied;
            public BlueprintSpellList spell_list;
            public int spell_level;
            public BlueprintSpellbook spell_book;

            public void HandleLevelUpComplete(UnitEntityData unit, bool isChargen)
            {
            }

            public override void OnFactActivate()
            {
                try
                {
                    var levelUp = Game.Instance.UI.CharacterBuildController.LevelUpController;
                    if (Owner == levelUp.Preview || Owner == levelUp.Unit)
                    {
                        var spellSelection = levelUp.State.DemandSpellSelection(spell_book, spell_list);
                        int existingNewSpells = spellSelection.LevelCount[spell_level]?.SpellSelections.Length ?? 0;
                        spellSelection.SetLevelSpells(spell_level, 1 + existingNewSpells);
                        applied = true;
                    }

                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }


        public class ContextActionOnEngagedTargets: ContextAction
        {
            public ActionList actions;

            public override string GetCaption()
            {
                return string.Empty;
            }

            public override void RunAction()
            {
                if (actions == null)
                {
                    return;
                }

                foreach (UnitEntityData engagee in this.Target.Unit.CombatState.EngagedUnits)
                {
                    using (this.Context.GetDataScope((TargetWrapper)engagee))
                    {
                        this.actions.Run();
                    }
                }
            }
        }


        public class EnergyDamageTypeSpellBonus : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, IInitiatorRulebookSubscriber
        {
            public DamageEnergyType energy_type;
            public ContextValue value;

            public void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {
                MechanicsContext context = evt.Reason.Context;
                if (context?.SourceAbility == null  || !context.SourceAbility.IsSpell)
                    return;

                var dmg = value.Calculate(this.Fact.MaybeContext);

                foreach (BaseDamage baseDamage in evt.DamageBundle)
                {
                    
                    var energy_damage = baseDamage as EnergyDamage;
                    if (energy_damage == null)
                    {
                        continue;
                    }

                    if (energy_damage.EnergyType == energy_type)
                    {
                        energy_damage.AddBonus(dmg);
                        return;
                    }
                }
            }

            public void OnEventDidTrigger(RuleCalculateDamage evt)
            {
            }
        }


        public class SpellDescriptorDamageSpellBonus : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, IInitiatorRulebookSubscriber
        {
            public SpellDescriptorWrapper SpellDescriptor;
            public ContextValue value;

            public void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {
                MechanicsContext context = evt.Reason.Context;
                if (context?.SourceAbility == null || !context.SourceAbility.IsSpell || !context.SpellDescriptor.HasAnyFlag((Kingmaker.Blueprints.Classes.Spells.SpellDescriptor)this.SpellDescriptor))
                    return;

                var dmg = value.Calculate(this.Fact.MaybeContext);
                evt.DamageBundle.First?.AddBonus(dmg);
            }

            public void OnEventDidTrigger(RuleCalculateDamage evt)
            {
            }
        }

        public class DamageAbilityBonus : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, IInitiatorRulebookSubscriber
        {
            public BlueprintAbility[] abilities;
            public ContextValue value;

            public void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {
                MechanicsContext context = evt.Reason.Context;
                if (context?.SourceAbility == null || !abilities.Contains(context.SourceAbility))
                    return;

                var dmg = value.Calculate(this.Fact.MaybeContext);
                evt.DamageBundle.First?.AddBonus(dmg);
            }

            public void OnEventDidTrigger(RuleCalculateDamage evt)
            {
            }
        }


        [ComponentName("Weapon Damage Stat Replacement against fact owner")]
        public class WeaponDamageStatReplacementAgainstFactOwner : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookSubscriber
        {
            public StatType new_stat;
            public BlueprintUnitFact fact;
            public bool only_unarmed_or_feral_combat_training = false;

            public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
            {
                if (evt.AttackWithWeapon == null)
                {
                    return;
                }

                if (!evt.AttackWithWeapon.Target.Descriptor.HasFact(fact))
                {
                    return;
                }

                if (only_unarmed_or_feral_combat_training && !FeralCombatTraining.checkHasFeralCombat(evt.Initiator, evt.AttackWithWeapon.Weapon))
                {
                    return;
                }

                var current_stat = evt.AttackWithWeapon.WeaponStats.DamageBonusStat;

                if (!current_stat.HasValue || evt.Initiator.Stats.GetStat(current_stat.Value).ModifiedValue >= evt.Initiator.Stats.GetStat(new_stat).ModifiedValue)
                {
                    return;
                }

                evt.OverrideDamageBonusStat(new_stat);
            }

            public void OnEventDidTrigger(RuleCalculateWeaponStats evt)
            {
            }
        }


        public class ContextActionAttackOfOpportunity : ContextAction
        {
            public override string GetCaption()
            {
                return "";
            }

            public override void RunAction()
            {
                if (this.Context.MaybeCaster == null || this.Target?.Unit == null)
                {
                    return;
                }
                Game.Instance.CombatEngagementController.ForceAttackOfOpportunity(this.Context.MaybeCaster, this.Target.Unit);
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class JabbingStrikeDamageBonus : RuleInitiatorLogicComponent<RulePrepareDamage>
        {
            public BlueprintUnitFact target_buff;
            public BlueprintUnitFact target_buff_master;
            public BlueprintUnitFact master_fact;

            public override void OnEventAboutToTrigger(RulePrepareDamage evt)
            {
                if (evt.DamageBundle.Empty())
                {
                    return;
                }
                if (!FeralCombatTraining.checkHasFeralCombat(evt.Initiator, evt.DamageBundle.Weapon))
                {
                    return;
                }

                int dmg_multiplier = evt.Initiator.Descriptor.HasFact(master_fact) ? 2 : 1;

                if (evt.Target.Descriptor.HasFact(target_buff_master) && evt.Target.Descriptor.HasFact(target_buff))
                {
                    evt.DamageBundle.Add(evt.DamageBundle.FirstOrDefault().CreateTypeDescription().CreateDamage(new DiceFormula(dmg_multiplier * 2, DiceType.D6), 0));
                }
                else if (evt.Target.Descriptor.HasFact(target_buff))
                {
                    evt.DamageBundle.Add(evt.DamageBundle.FirstOrDefault().CreateTypeDescription().CreateDamage(new DiceFormula(dmg_multiplier, DiceType.D6), 0));
                }
            }

            public override void OnEventDidTrigger(RulePrepareDamage evt)
            {
            }
        }


        public class AddIncomingDamageTriggerExceptAbility : OwnedGameLogicComponent<UnitDescriptor>, ITargetRulebookHandler<RuleDealDamage>, ITargetRulebookHandler<RuleDealStatDamage>, ITargetRulebookHandler<RuleDrainEnergy>, IRulebookHandler<RuleDealDamage>, ITargetRulebookSubscriber, IRulebookHandler<RuleDealStatDamage>, IRulebookHandler<RuleDrainEnergy>
        {
            public ActionList Actions;
            public bool TriggerOnStatDamageOrEnergyDrain;
            public BlueprintAbility[] except_abilities;

            private void RunAction()
            {
                if (!this.Actions.HasActions)
                    return;
                (this.Fact as IFactContextOwner)?.RunActionInContext(this.Actions, (TargetWrapper)this.Owner.Unit);
            }

            public void OnEventAboutToTrigger(RuleDealDamage evt)
            {
            }

            public void OnEventDidTrigger(RuleDealDamage evt)
            {
                if (evt.SourceAbility != null && except_abilities.Contains(evt.SourceAbility))
                {
                    return;
                }
                this.RunAction();
            }

            public void OnEventAboutToTrigger(RuleDealStatDamage evt)
            {
            }

            public void OnEventDidTrigger(RuleDealStatDamage evt)
            {
                if (!this.TriggerOnStatDamageOrEnergyDrain)
                    return;
                this.RunAction();
            }

            public void OnEventAboutToTrigger(RuleDrainEnergy evt)
            {
            }

            public void OnEventDidTrigger(RuleDrainEnergy evt)
            {
                if (!this.TriggerOnStatDamageOrEnergyDrain)
                    return;
                this.RunAction();
            }
        }


        public class TwoWeaponFightingRestriction : ActivatableAbilityRestriction
        {

            public override bool IsAvailable()
            {
                if (Owner.Body.IsPolymorphed)
                {
                    return false;
                }

              
                var weapon1 = Owner.Body.PrimaryHand.HasWeapon ? Owner.Body.PrimaryHand.MaybeWeapon : null;
                var weapon2 = Owner.Body.SecondaryHand.HasWeapon ? Owner.Body.SecondaryHand.MaybeWeapon : null;

                if (weapon1 == null || weapon2 == null)
                {
                    return false;
                }

                if (weapon1.Blueprint.IsNatural || weapon2.Blueprint.IsNatural)
                {
                    return false;
                }

                return true;
            }
        }


        public class PrimaryHandMeleeWeaponRestriction : ActivatableAbilityRestriction
        {
            public override bool IsAvailable()
            {
                if (Owner.Body.PrimaryHand.HasWeapon)
                    return Owner.Body.PrimaryHand.Weapon.Blueprint.IsMelee;
                return false;
            }
        }

    }
}
