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
using System.Text;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UI.LevelUp;

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


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilityCasterMainWeaponIsRanged : BlueprintComponent, IAbilityCasterChecker
        {
            public bool CorrectCaster(UnitEntityData caster)
            {
                if (caster.Body.PrimaryHand.HasWeapon)
                    return !caster.Body.PrimaryHand.Weapon.Blueprint.IsMelee;
                return false;
            }

            public string GetReason()
            {
                return "Ranged weapon required";
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
            private BlueprintAbility current_spell = null;


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
                current_spell = null;
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
            }
        }


        [AllowedOn(typeof(Kingmaker.Blueprints.Facts.BlueprintUnitFact))]
        public class MetamagicAdept : RuleInitiatorLogicComponent<RuleCastSpell>, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>
        {
            public BlueprintAbilityResource resource;
            private BlueprintAbility current_spell = null;


            public override void OnEventAboutToTrigger(RuleCastSpell evt)
            {
                if (evt.Spell.SourceItem != null || evt.Spell.Blueprint != current_spell)
                {
                    current_spell = null;
                    return;
                }
            }


            public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                if (evt.Spell.Type == AbilityType.Spell)
                {
                    current_spell = evt.Spell;
                }
            }

            public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {

            }

            public override void OnEventDidTrigger(RuleCastSpell evt)
            {
                if (current_spell == null)
                {
                    return;
                }
                current_spell = null;

                if (evt.Spell.MetamagicData == null)
                {
                    return;
                }

                int metamagic_count = Helpers.PopulationCount((int)(evt.Spell.MetamagicData.MetamagicMask & ~((Metamagic)MetamagicFeats.MetamagicExtender.FreeMetamagic)));
                if (metamagic_count > 1)
                {
                    return;
                }
                if (evt.Spell.MetamagicData.MetamagicMask != 0 && evt.Spell.MetamagicData.MetamagicMask != Metamagic.Quicken)
                {
                    if (resource != null)
                    {
                        this.Owner.Resources.Spend((BlueprintScriptableObject)this.resource, 1);
                    }
                }
            }
        }


        [AllowedOn(typeof(Kingmaker.Blueprints.Facts.BlueprintUnitFact))]
        public class SpendResourceOnSpellCast : BuffLogic, IInitiatorRulebookHandler<RuleCastSpell>
        {
            public BlueprintAbilityResource resource;
            public BlueprintSpellbook spellbook;
            public int amount = 1;
            public bool used_for_reducing_metamagic_cast_time = false;
            public bool is_metamixing = false;
            public bool remove_self = false;

            public void OnEventAboutToTrigger(RuleCastSpell evt)
            {

            }

            public void OnEventDidTrigger(RuleCastSpell evt)
            {
                var spellbook_blueprint = evt.Spell?.Spellbook?.Blueprint;
                if (spellbook_blueprint == null)
                {
                    return;
                }

                if (evt.Spell.Blueprint.Type != AbilityType.Spell)
                {
                    return;
                }

                if (evt.Spell.StickyTouch != null)
                {
                    return;
                }


                if (used_for_reducing_metamagic_cast_time &&
                    (evt.Spell.MetamagicData == null || (evt.Spell.MetamagicData.MetamagicMask != 0 && (evt.Spell.MetamagicData.MetamagicMask & Metamagic.Quicken) != 0))
                    )
                {
                    return;
                }

                if (used_for_reducing_metamagic_cast_time && is_metamixing)
                {
                    var arcanist_part = this.Owner.Get<SpellManipulationMechanics.UnitPartArcanistPreparedMetamagic>();
                    if (arcanist_part != null && arcanist_part.isUsedWithMetamixing(evt.Spell.Blueprint, evt.Spell.MetamagicData.MetamagicMask))
                    {
                        this.Owner.Resources.Spend((BlueprintScriptableObject)this.resource, amount);
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
                 
                if (spellbook == null || spellbook_blueprint == spellbook)
                {
                    this.Owner.Resources.Spend((BlueprintScriptableObject)this.resource, amount);
                }

                if (remove_self)
                {
                    this.Buff.Remove();
                }
            }
        }


        [AllowedOn(typeof(Kingmaker.Blueprints.Facts.BlueprintUnitFact))]
        public class SpendResourceOnSpecificSpellCast : BuffLogic, IInitiatorRulebookHandler<RuleCastSpell>
        {            
            public BlueprintAbilityResource resource;
            public BlueprintSpellbook spellbook;
            public BlueprintCharacterClass specific_class;
            public int amount = 1;
            public SpellSchool school;
            public BlueprintAbility[] spell_list = new BlueprintAbility[0];
            public SpellDescriptorWrapper spell_descriptor;
            public bool remove_self = false;

            public void OnEventAboutToTrigger(RuleCastSpell evt)
            {
            }

            public void OnEventDidTrigger(RuleCastSpell evt)
            {
                var spellbook_blueprint = evt.Spell?.Spellbook?.Blueprint;
                if (spellbook_blueprint == null)
                {
                    return;
                }

                if (evt.Spell.StickyTouch != null)
                {
                    return;
                }

                if (!Helpers.checkSpellbook(spellbook, specific_class, evt.Spell?.Spellbook, evt.Initiator.Descriptor))
                {
                    return;
                }

                if (school != SpellSchool.None && evt.Spell.Blueprint.School != school)
                {
                    return;
                }

                if (!spell_list.Empty() && !spell_list.Contains(evt.Spell.Blueprint))
                {
                    return;
                }

                if (spell_descriptor != SpellDescriptor.None && ((evt.Context.SpellDescriptor & spell_descriptor) == 0))
                {
                    return;
                }

                this.Owner.Resources.Spend((BlueprintScriptableObject)this.resource, amount);
                if (remove_self)
                {
                    this.Buff.Remove();
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
                var caster = evt.Reason?.Caster;
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


        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintAbility))]
        public class AbilityTargetIsCaster : BlueprintComponent, IAbilityTargetChecker
        {

            public bool CanTarget(UnitEntityData caster, TargetWrapper target)
            {
                UnitEntityData unit = target.Unit;

                return target == caster;
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

                var primary_hand = unit.Body?.PrimaryHand;
                if (primary_hand == null)
                {
                    return false;
                }

                if (primary_hand.HasWeapon)
                    return ((IEnumerable<WeaponCategory>)this.Category).Contains<WeaponCategory>(primary_hand.Weapon.Blueprint.Type.Category);
                return false;
            }

            public string GetReason()
            {
                return (string)LocalizedTexts.Instance.Reasons.SpecificWeaponRequired;
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilitTargetManufacturedWeapon : BlueprintComponent, IAbilityTargetChecker
        {
            public bool works_on_summoned = false;
            public bool off_hand = false;
            public bool only_melee;
            public bool CanTarget(UnitEntityData caster, TargetWrapper target)
            {
                UnitEntityData unit = target.Unit;
                if (unit == null)
                    return false;

                if (unit.Body.IsPolymorphed)
                {
                    return false;
                }

                var hand = !off_hand ? unit.Body?.PrimaryHand : unit.Body?.SecondaryHand;
                if (hand == null)
                {
                    return false;
                }

                if (hand.HasWeapon)
                {
                    if (only_melee && !hand.Weapon.Blueprint.IsMelee)
                    {
                        return false;
                    }
                    bool monk_strike = hand.Weapon.Blueprint.IsUnarmed && (bool)unit.Descriptor.State.Features.ImprovedUnarmedStrike && !off_hand;
                    return monk_strike || (!hand.Weapon.Blueprint.IsNatural && (!EnchantmentMechanics.Helpers.isSummoned(hand.Weapon) || works_on_summoned));
                }


                return false;
            }

            public string GetReason()
            {
                return "Require manufactured weapon";
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

                if (unit.Body.IsPolymorphed)
                {
                    return false;
                }

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
            public bool use_kineticist_main_stat;
            public StatType StatType = StatType.Charisma;
            public BlueprintCharacterClass[] CharacterClasses = new BlueprintCharacterClass[0];
            public BlueprintArchetype[] archetypes = new BlueprintArchetype[0];

            public override AbilityParams Calculate(MechanicsContext context)
            {
                UnitEntityData maybeCaster = context?.MaybeCaster;
                if (maybeCaster == null)
                {
                    return context?.Params;
                }
                StatType statType = this.StatType;
                if (this.use_kineticist_main_stat)
                {
                    UnitPartKineticist unitPartKineticist = context.MaybeCaster?.Get<UnitPartKineticist>();
                    if (unitPartKineticist == null)
                        UberDebug.LogError((UnityEngine.Object)context.AssociatedBlueprint, (object)string.Format("Caster is not kineticist: {0} ({1})", (object)context.MaybeCaster, (object)context.AssociatedBlueprint.NameSafe()), (object[])Array.Empty<object>());
                    StatType? mainStatType = unitPartKineticist?.MainStatType;
                    statType = !mainStatType.HasValue ? this.StatType : mainStatType.Value;
                }

                AbilityData ability = context.SourceAbilityContext?.Ability;
                RuleCalculateAbilityParams rule = !(ability != (AbilityData)null) ? new RuleCalculateAbilityParams(maybeCaster, context.AssociatedBlueprint, (Spellbook)null) : new RuleCalculateAbilityParams(maybeCaster, ability);
                rule.ReplaceStat = new StatType?(statType);

                int class_level = 0;
                foreach (var c in this.CharacterClasses)
                {
                    var class_archetypes = archetypes.Where(a => a.GetParentClass() == c);

                    if (class_archetypes.Empty() || class_archetypes.Any(a => maybeCaster.Descriptor.Progression.IsArchetype(a)))
                    {
                        class_level += maybeCaster.Descriptor.Progression.GetClassLevel(c);
                    }
                    
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


        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class CrowdACBonus : RuleTargetLogicComponent<RuleCalculateAC>
        {
            public int num_characters_around;
            public int Radius;
            public ContextValue value;

            public override void OnEventAboutToTrigger(RuleCalculateAC evt)
            {
                int num = 0;
                foreach (UnitEntityData unitEntityData in GameHelper.GetTargetsAround(this.Owner.Unit.Position, (float)this.Radius, true, false))
                {
                    if (unitEntityData != this.Owner.Unit)
                    {
                        num++;
                    }
                }
                if (num < num_characters_around)
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

        [ComponentName("Increase spell level by descriptor")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ContextIncreaseDescriptorSpellLevel : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            public ContextValue Value;
            public SpellDescriptorWrapper Descriptor;
            public BlueprintSpellbook spellbook = null;
            public BlueprintCharacterClass specific_class = null;
            public bool only_spells = true;

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
                if (evt.Initiator == null)
                {
                    return;
                }

                if (!Helpers.checkSpellbook(spellbook, specific_class, evt.Spellbook, evt.Initiator.Descriptor))
                {
                    return;
                }

                if (evt.Spellbook?.Blueprint == null && only_spells)
                {
                    return;
                }
                if (this.Descriptor != SpellDescriptor.None)
                {
                    bool? nullable = evt.Blueprint.GetComponent<SpellDescriptorComponent>()?.Descriptor.HasAnyFlag((SpellDescriptor)this.Descriptor);
                    if (!nullable.HasValue || !nullable.Value)
                        return;
                }
                evt.AddBonusCasterLevel(this.Value.Calculate(this.Context));
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }


        [ComponentName("Increase context spells DC by descriptor")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ContextIncreaseDescriptorSpellsDC : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            public ContextValue Value;
            public SpellDescriptorWrapper Descriptor;
            public BlueprintSpellbook spellbook = null;
            public BlueprintCharacterClass specific_class = null;
            public bool only_spells = true;

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
                if (evt.Initiator == null)
                {
                    return;
                }

                if (!Helpers.checkSpellbook(spellbook, specific_class, evt.Spellbook, evt.Initiator.Descriptor))
                {
                    return;
                }

                if (evt.Spellbook?.Blueprint == null && only_spells)
                {
                    return;
                }
                if (this.Descriptor != SpellDescriptor.None)
                {
                    bool? nullable = evt.Blueprint.GetComponent<SpellDescriptorComponent>()?.Descriptor.HasAnyFlag((SpellDescriptor)this.Descriptor);
                    if (!nullable.HasValue || !nullable.Value)
                        return;
                }
                evt.AddBonusDC(this.Value.Calculate(this.Context));
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }


        [ComponentName("Increase context spells DC by descriptor")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ContextIncreaseSchoolSpellsDC : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            public ContextValue Value;
            public SpellSchool school;
            public BlueprintSpellbook spellbook = null;
            public BlueprintCharacterClass specific_class = null;
            public bool only_spells = true;

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
                if (evt.Initiator == null)
                {
                    return;
                }

                if (!Helpers.checkSpellbook(spellbook, specific_class, evt.Spellbook, evt.Initiator.Descriptor))
                {
                    return;
                }

                if (evt.Spellbook?.Blueprint == null && only_spells)
                {
                    return;
                }
                if (this.school != (evt.AbilityData?.Blueprint?.School).GetValueOrDefault())
                {
                        return;
                }
                evt.AddBonusDC(this.Value.Calculate(this.Context));
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }



        [ComponentName("Increase context spells DC by descriptor")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ContextIncreaseAbilitiesDC : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            public ContextValue Value;
            public BlueprintAbility[] abilities;

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
                if (abilities.Contains(evt.Spell) || (evt.Spell.Parent != null && abilities.Contains(evt.Spell.Parent)))
                {
                    evt.AddBonusDC(this.Value.Calculate(this.Context));
                }
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
        public class ComeAndGetMe : RuleTargetLogicComponent<RuleCalculateAC>, ITargetRulebookHandler<RuleDealDamage>, ITargetRulebookHandler<RuleAttackWithWeapon>
        {
            public override void OnEventAboutToTrigger(RuleCalculateAC evt)
            {
                evt.AddBonus(-4, this.Fact);
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

            public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
            {

            }

            public void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
                if (this.Owner.Body.PrimaryHand.MaybeWeapon != null && this.Owner.Body.PrimaryHand.MaybeWeapon.Blueprint.IsMelee && evt.Weapon.Blueprint.IsMelee && this.Owner.Unit.CombatState.IsEngage(evt.Initiator))
                {
                    //this.Owner.Unit.CombatState.AttackOfOpportunity(evt.Initiator);
                    Game.Instance.CombatEngagementController.ForceAttackOfOpportunity(this.Owner.Unit, evt.Initiator);
                }
            }
        }


        public class AttackTypeCriticalEdgeIncrease : RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
        {
            public AttackTypeAttackBonus.WeaponRangeType Type;
            public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
            {
                if (evt.Weapon == null || !AttackTypeAttackBonus.CheckRangeType(evt.Weapon.Blueprint, this.Type))
                    return;
                evt.DoubleCriticalEdge = true;
            }

            public override void OnEventDidTrigger(RuleCalculateWeaponStats evt)
            {
            }
        }


        public class WeaponTypeSizeChange : RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
        {
            public int SizeCategoryChange;
            public BlueprintWeaponType[] WeaponTypes;

            public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
            {
                if ((!WeaponTypes.Empty() && !this.WeaponTypes.Contains(evt.Weapon.Blueprint.Type)) || this.SizeCategoryChange == 0)
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

        public class RestrictionHasFacts : ActivatableAbilityRestriction
        {
            public BlueprintUnitFact[] features;
            public bool not;
            public bool all;

            public override bool IsAvailable()
            {
                bool res = false;
                foreach (var f in features)
                {
                   bool has_fact = this.Owner.HasFact(f);
                   if (all && !has_fact)
                   {
                        res = false;
                        break;
                   }
                  
                   if (has_fact && !all)
                   {
                        res = true;
                        break;
                   }
                    res = res || has_fact;
                }

                return res != not;
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ForbidSpellCastingUnlessHasFacts : OwnedGameLogicComponent<UnitDescriptor>
        {
            public bool ForbidMagicItems;
            public BlueprintUnitFact[] allowed_facts;
            private bool activated = false;

            public override void OnTurnOn()
            {
                foreach (var f in allowed_facts)
                {
                    if (this.Owner.HasFact(f))
                    {
                        return;
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

                var wielder_size = evt.Initiator.Descriptor.State.Size;
                //scale weapon to the wielder size if need (note polymophs do not change their size, so their weapon dice is not supposed to scale)
                var base_dice = evt.Initiator.Body.IsPolymorphed ? evt.Weapon.Blueprint.BaseDamage  : WeaponDamageScaleTable.Scale(evt.Weapon.Blueprint.BaseDamage, wielder_size);

                var new_dice = WeaponDamageScaleTable.Scale(dice_formulas[dice_id], wielder_size);

                var new_dmg_avg = new_dice.MinValue(0) + new_dice.MaxValue(0);
                int current_dmg_avg = (base_dice.MaxValue(0) + base_dice.MinValue(0));
                if (new_dmg_avg > current_dmg_avg)
                {
                    evt.WeaponDamageDiceOverride = dice_formulas[dice_id];
                }
            }

            public void OnEventDidTrigger(RuleCalculateWeaponStats evt)
            {

            }
        }


        public class ShroudOfWater2 : OwnedGameLogicComponent<UnitDescriptor>
        {
            public ModifierDescriptor Descriptor1;
            public ModifierDescriptor Descriptor2;
            public StatType Stat;
            public ContextValue BaseValue;
            public BlueprintFeature UpgradeFeature;
            public BlueprintArchetype kinetic_knight_archetype;
            private ModifiableValue.Modifier m_Modifier;
            private ModifiableValue.Modifier m_Modifier2;

            private MechanicsContext Context
            {
                get
                {
                    return this.Fact.MaybeContext;
                }
            }

            public override void OnTurnOn()
            {
                ModifiableValue stat = this.Owner.Stats.GetStat(this.Stat);
                Fact fact = this.Owner.Progression.Features.GetFact((BlueprintFact)this.UpgradeFeature);
                if ((this.Owner.Progression.GetClassData(kinetic_knight_archetype.GetParentClass())?.Archetypes.Contains(kinetic_knight_archetype)).GetValueOrDefault())
                {
                    int base_bonus = this.BaseValue.Calculate(this.Context);
                    this.m_Modifier = stat.AddModifier(base_bonus, (GameLogicComponent)this, this.Descriptor1);
                    if (fact != null)
                    {
                        int extra_bonus = Math.Min((int)Math.Floor((double)base_bonus * 0.5), fact.GetRank());
                        this.m_Modifier2 = stat.AddModifier(extra_bonus, (GameLogicComponent)this, this.Descriptor2);
                    }
                }
                else
                {
                    int num = fact != null ? Math.Min((int)Math.Floor((double)this.BaseValue.Calculate(this.Context) * 1.5), this.BaseValue.Calculate(this.Context) + fact.GetRank()) : this.BaseValue.Calculate(this.Context);
                    this.m_Modifier = stat.AddModifier(num, (GameLogicComponent)this, this.Descriptor1);
                }
            }

            public override void OnTurnOff()
            {
                this.m_Modifier.Remove();
                this.m_Modifier2?.Remove();
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ContextWeaponDamageDiceReplacementForSpecificCategory : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookSubscriber
        {
            public WeaponCategory category;
            public DiceFormula[] dice_formulas;
            public ContextValue value;

            private MechanicsContext Context
            {
                get
                {
                    return this.Fact.MaybeContext;
                }
            }

            public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
            {
                if (evt.Weapon.Blueprint.Category != category)
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
                var old_dice_formula = evt.Weapon.Blueprint.ScaleDamage(Size.Medium);
                double current_avg_damage = (old_dice_formula.MaxValue(0) + old_dice_formula.MinValue(0)) / 2.0;

                var new_dmg_scaled = WeaponDamageScaleTable.Scale(dice_formulas[dice_id], evt.Weapon.Size);
                var old_dmg_scaled = evt.Weapon.Damage;

                double new_avg_dmg_scaled = (new_dmg_scaled.MinValue(0) + new_dmg_scaled.MaxValue(0)) / 2.0;
                double current_avg_damage_scaled = (old_dmg_scaled.MaxValue(0) + old_dmg_scaled.MinValue(0)) / 2.0;

                if (new_avg_dmg > current_avg_damage)
                {
                    evt.WeaponDamageDiceOverride = dice_formulas[dice_id];
                }
                else if (new_avg_dmg_scaled > current_avg_damage_scaled)
                {
                    evt.WeaponDamageDiceOverride = dice_formulas[dice_id];
                }
            }

            public void OnEventDidTrigger(RuleCalculateWeaponStats evt)
            {

            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ContextWeaponDamageDiceReplacementWeaponCategory : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookSubscriber
        {
            public WeaponCategory[] categories;
            public DiceFormula[] dice_formulas;
            public ContextValue value;

            private MechanicsContext Context
            {
                get
                {
                    return this.Fact.MaybeContext;
                }
            }

            public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
            {
                if (!categories.Contains(evt.Weapon.Blueprint.Category))
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

                var wielder_size = evt.Initiator.Descriptor.State.Size;
                //scale weapon to the wielder size if need (note polymophs do not change their size, so their weapon dice is not supposed to scale)
                var base_dice = evt.Initiator.Body.IsPolymorphed ? evt.Weapon.Blueprint.BaseDamage : WeaponDamageScaleTable.Scale(evt.Weapon.Blueprint.BaseDamage, wielder_size);

                var new_dice = WeaponDamageScaleTable.Scale(dice_formulas[dice_id], wielder_size);

                var new_dmg_avg = new_dice.MinValue(0) + new_dice.MaxValue(0);
                int current_dmg_avg = (base_dice.MaxValue(0) + base_dice.MinValue(0));
                if (new_dmg_avg > current_dmg_avg)
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
        public class SneakAttackDamageBonus : RuleInitiatorLogicComponent<RulePrepareDamage>
        {
            public ContextValue value;
          
            public override void OnEventAboutToTrigger(RulePrepareDamage evt)
            {
            }


            public override void OnEventDidTrigger(RulePrepareDamage evt)
            {
                foreach (var dmg in evt.DamageBundle)
                {
                    if (dmg.Sneak)
                    {
                        dmg.AddBonus(value.Calculate(this.Fact.MaybeContext));
                        break;
                    }
                }
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class DamageBonusAgainstFlankedTarget : RuleInitiatorLogicComponent<RuleCalculateDamage>
        {
            public int bonus;

            public override void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {
                if (evt.Target.CombatState.IsFlanked && (evt.DamageBundle.Weapon?.Blueprint.IsMelee).GetValueOrDefault() == true)
                {
                    evt.DamageBundle.WeaponDamage?.AddBonusTargetRelated(bonus);
                }
            }

            public override void OnEventDidTrigger(RuleCalculateDamage evt) { }
        }

        public class ArmorCategoryAcBonus : OwnedGameLogicComponent<UnitDescriptor>, IUnitActiveEquipmentSetHandler, IUnitEquipmentHandler, IGlobalSubscriber
        {
            public ArmorProficiencyGroup category;
            public ContextValue value;
            public ModifierDescriptor descriptor;
            private ModifiableValue.Modifier m_Modifier;

            public override void OnTurnOn()
            {
                base.OnTurnOn();
                this.CheckArmor();
            }

            public override void OnTurnOff()
            {
                base.OnTurnOff();
                this.DeactivateModifier();
            }

            public void HandleUnitChangeActiveEquipmentSet(UnitDescriptor unit)
            {
                this.CheckArmor();
            }

            public void CheckArmor()
            {
                if (this.Owner.Body.Armor.HasArmor && this.Owner.Body.Armor.Armor.Blueprint.ProficiencyGroup == this.category)
                    this.ActivateModifier();
                else
                    this.DeactivateModifier();
            }

            public void ActivateModifier()
            {
                if (this.m_Modifier != null)
                    return;
                this.m_Modifier = this.Owner.Stats.AC.AddModifier(value.Calculate(this.Fact.MaybeContext), (GameLogicComponent)this, descriptor);
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
                this.CheckArmor();
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
                foreach (var dmg in evt.DamageBundle)
                {
                    (dmg as PhysicalDamage)?.AddAlignment(damage_alignment);
                }
            }
        }





        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowedOn(typeof(BlueprintBuff))]
        [AllowMultipleComponents]
        public class AddOutgoingPhysicalDamageMaterialIfParametrizedFeature : RuleInitiatorLogicComponent<RulePrepareDamage>
        {
            public BlueprintParametrizedFeature required_parametrized_feature;
            public PhysicalDamageMaterial material;
            public bool add_magic = false;

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
                foreach (var dmg in evt.DamageBundle)
                {
                    var physical_dmg = (dmg as PhysicalDamage);
                    if (physical_dmg == null)
                    {
                        continue;
                    }
                    if (!add_magic)
                    {
                        physical_dmg?.AddMaterial(material);
                    }
                    else if (physical_dmg.Enchantment < 1)
                    {
                        physical_dmg.Enchantment = 1;
                        physical_dmg.EnchantmentTotal++;
                    }
                }
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


        public class SpellListAffinity : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookSubscriber
        {
            public BlueprintSpellList base_spell_list;
            public BlueprintSpellList second_spell_list;

            public int bonus;

            public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                if (evt.Spell.IsSpell && base_spell_list.Contains(evt.Spell.Parent ?? evt.Spell) && second_spell_list.Contains(evt.Spell.Parent ?? evt.Spell))
                {
                    evt.AddBonusDC(bonus);
                    evt.AddBonusCasterLevel(bonus);
                }
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


        public class ActivatableARestrictionCasterPolymorphed : ActivatableAbilityRestriction
        {
            public bool not = false;

            public override bool IsAvailable()
            {
                return Owner.Body.IsPolymorphed != not;
            }
        }


        public class ActivatableAbilityMeleeWeaponRestriction : ActivatableAbilityRestriction
        {
            public override bool IsAvailable()
            {
                if (Owner.Body.PrimaryHand.HasWeapon)
                    return Owner.Body.PrimaryHand.Weapon.Blueprint.IsMelee;
                return false;
            }
        }



        public class ActivatableAbilityMainHandWeaponEnhancementIfHasArchetype : ActivatableAbilityRestriction
        {
            public BlueprintArchetype archetype;
            public BlueprintWeaponEnchantment enchant;

            public override bool IsAvailable()
            {
                if (Owner.Progression.IsArchetype(archetype))
                {
                    return true;
                }
                var  weapon = Owner.Body?.PrimaryHand?.MaybeWeapon;
                if (weapon == null || weapon.EnchantmentsCollection == null)
                {
                    return false;
                }

                return weapon.EnchantmentsCollection.HasFact(enchant);
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
        public class AbilityCasterEquippedWeaponCheckHasParametrizedFeature : BlueprintComponent, IAbilityCasterChecker
        {
            public BlueprintParametrizedFeature feature = null;
            public BlueprintFeature alternative = null;
            public bool allow_kinetic_blast = false;

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
                var weapon1 = caster.Body.PrimaryHand.HasWeapon ? caster.Body.PrimaryHand.MaybeWeapon : caster.Body.EmptyHandWeapon;
                if (weapon1 != null && checkFeature(caster, weapon1.Blueprint.Category) || checkAlternative(caster))
                {
                    return true;
                }

                if (allow_kinetic_blast && checkFeature(caster, WeaponCategory.KineticBlast))
                {
                    return true;
                }

                var weapon2 = caster.Body.SecondaryHand.HasWeapon ? caster.Body.SecondaryHand.MaybeWeapon : null;
                if (weapon2 == null)
                {
                    return false;
                }

                return checkFeature(caster, weapon2.Blueprint.Category) || checkAlternative(caster);
            }

            public string GetReason()
            {
                return (string)LocalizedTexts.Instance.Reasons.SpecificWeaponRequired;
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
        public class AbilityCasterHasShield : BlueprintComponent, IAbilityCasterChecker
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


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilityCasterKnowsSpell : BlueprintComponent, IAbilityCasterChecker
        {
            public bool not;
            public BlueprintSpellbook spellbook;
            public BlueprintAbility spell;
            public bool CorrectCaster(UnitEntityData caster)
            {
                var sb = caster?.Descriptor?.GetSpellbook(spellbook);
                if (sb == null)
                {
                    return not;
                }

                return sb.IsKnown(spell) != not;
            }

            public string GetReason()
            {
                return "Spell " + spell.Name + " is " + (not ? " already known" : " unknown");
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilityCasterMoved : BlueprintComponent, IAbilityCasterChecker
        {
            public bool not;
            public bool CorrectCaster(UnitEntityData caster)
            {
                return not != caster.CombatState.IsFullAttackRestrictedBecauseOfMoveAction;
            }

            public string GetReason()
            {
                return "Moved this round";
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilityCasterPetIsAlive : BlueprintComponent, IAbilityCasterChecker
        {
            public bool CorrectCaster(UnitEntityData caster)
            {
                return caster.Descriptor.Pet != null && !caster.Descriptor.Pet.Descriptor.State.IsDead;
            }

            public string GetReason()
            {
                return "Companion is dead";
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilityCasterMasterIsAlive : BlueprintComponent, IAbilityCasterChecker
        {
            public bool CorrectCaster(UnitEntityData caster)
            {
                return caster.Descriptor.Master.Value != null && !caster.Descriptor.Master.Value.Descriptor.State.IsDead;
            }

            public string GetReason()
            {
                return "Companion is dead";
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
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class AddFeatureOnFactRank : OwnedGameLogicComponent<UnitDescriptor>, IUnitGainFactHandler, IUnitLostFactHandler, IGlobalSubscriber
        {
            public BlueprintFeature checked_fact;
            public BlueprintFeature[] additional_triggering_features = new BlueprintFeature[0];
            public int fact_rank;
            public BlueprintUnitFact feature;
            public bool not;
            [JsonProperty]
            private Fact m_AppliedFact;

            public override void OnFactActivate()
            {
                this.Apply();
            }

            public override void OnFactDeactivate()
            {
                this.Owner.RemoveFact(this.m_AppliedFact);
                this.m_AppliedFact = null;
            }

            public void HandleUnitGainFact(Fact fact)
            {
                if (fact.Blueprint == checked_fact || additional_triggering_features.Contains(fact.Blueprint))
                {
                    this.Apply();
                }
            }


            public void HandleUnitLostFact(Fact fact)
            {
                if (fact.Blueprint == checked_fact || additional_triggering_features.Contains(fact.Blueprint))
                {
                    this.Apply();
                }
            }

            private void Apply()
            {
                OnFactDeactivate();
                if (this.m_AppliedFact != null || ((this.Owner.Progression.Features.GetRank(this.checked_fact) >= fact_rank) == not))
                    return;
                this.m_AppliedFact = this.Owner.AddFact(this.feature, null, null);
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilityCasterMainWeaponGroupCheck : BlueprintComponent, IAbilityCasterChecker
        {
            public WeaponFighterGroup[] groups;
            public bool is_2h = false;
            public bool is_sacred = false;
            static BlueprintParametrizedFeature weapon_focus = Main.library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");

            public bool CorrectCaster(UnitEntityData caster)
            {
                if (!caster.Body.PrimaryHand.HasWeapon)
                {
                    return false;
                }

                if (is_2h)
                {
                    return caster.Body.PrimaryHand.Weapon.Blueprint.IsTwoHanded && caster.Body.PrimaryHand.Weapon.Blueprint.IsMelee;
                }
                if (is_sacred)
                {
                    return checkFeature(caster.Descriptor, caster.Body.PrimaryHand.Weapon.Blueprint.Category, weapon_focus, NewFeats.deity_favored_weapon);
                }
                if (caster.Body.PrimaryHand.HasWeapon)
                    return (groups.Contains(caster.Body.PrimaryHand.Weapon.Blueprint.Type.FighterGroup));
                return false;
            }

            public string GetReason()
            {
                return (string)LocalizedTexts.Instance.Reasons.SpecificWeaponRequired;
            }

            static bool checkFeature(UnitDescriptor unit, WeaponCategory category, params BlueprintParametrizedFeature[] required_parametrized_features)
            {
                if (required_parametrized_features.Empty())
                {
                    return true;
                }
                foreach (var f in required_parametrized_features)
                {
                    if (unit.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => p.Blueprint == f).Any(p => p.Param == category))
                    {
                        return true;
                    }
                }
                return false;
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


        [AllowMultipleComponents]
        public class PrerequisiteAbility : Prerequisite
        {
            [NotNull]
            public BlueprintAbility Ability;

            public override bool Check(
              FeatureSelectionState selectionState,
              UnitDescriptor unit,
              LevelUpState state)
            {
                return unit.HasFact(Ability);
            }

            public override string GetUIText()
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (this.Ability == null)
                {
                    UberDebug.LogError((object)("Empty Feature fild in prerequisite component: " + this.name), (object[])Array.Empty<object>());
                }
                else
                {
                    if (string.IsNullOrEmpty(this.Ability.Name))
                        UberDebug.LogError((object)string.Format("{0} has no Display Name", this.Ability.name), (object[])Array.Empty<object>());
                    stringBuilder.Append(this.Ability.Name);
                }
                return stringBuilder.ToString();
            }
        }


        public class ResourseCostCalculatorWithDecreasingFacts : BlueprintComponent, IAbilityResourceCostCalculator
        {
            public BlueprintFact[] cost_reducing_facts = new BlueprintFact[0];
            public BlueprintFact[] cost_increasing_facts = new BlueprintFact[0];

            public int Calculate(AbilityData ability)
            {
                var cost = ability.Blueprint.GetComponent<AbilityResourceLogic>().Amount;
                foreach (var f in cost_reducing_facts)
                {
                    if (f is BlueprintBuff)
                    {
                        cost -= ability.Caster.Buffs.RawFacts.Count(b => b.Blueprint == f);
                    }
                    if (f is BlueprintFeature)
                    {
                        var feature = ability.Caster.GetFeature(f as BlueprintFeature);

                        if (feature != null)
                        {
                            cost -= feature.GetRank();
                        }
                    }
                }

                foreach (var f in cost_increasing_facts)
                {
                    if (f is BlueprintBuff)
                    {
                        cost += ability.Caster.Buffs.RawFacts.Count(b => b.Blueprint == f);
                    }
                    if (f is BlueprintFeature)
                    {
                        var feature = ability.Caster.GetFeature(f as BlueprintFeature);

                        if (feature != null)
                        {
                            cost += feature.GetRank();
                        }
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
                Main.TraceLog();
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
            public bool no_action_on_negative_value = false;

            public override void RunAction()
            {
                int action_id = value.Calculate(this.Context) - 1;
                if (action_id < 0)
                {
                    if (no_action_on_negative_value)
                    {
                        return;
                    }
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


        public class RunActionList : ContextAction
        {
            public string Comment;
            public ActionList actions;

            public override void RunAction()
            {
                actions.Run();
            }

            public override string GetCaption()
            {
                return "Actions from list (" + this.Comment + " )";
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
            public bool check_only_one_fact = false;

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
                            total_bonus += bonus;
                            if (check_only_one_fact)
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    var caster = this.Context.MaybeCaster;
                    foreach (var b in evt.Target.Buffs)
                    {
                        if (CheckedFacts.Contains(b.Blueprint) && b.MaybeContext.MaybeCaster == caster)
                        {
                            total_bonus += bonus;
                            if (check_only_one_fact)
                            {
                                break;
                            }
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


        [ComponentName("Replace caster level with class level")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class SpellLevelByClassLevel : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            public BlueprintAbility Ability;
            public BlueprintCharacterClass Class;
            public BlueprintCharacterClass ExtraClass;
            public BlueprintFeature ExtraFeatureToCheck;

            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                int classLevel = this.Owner.Progression.GetClassLevel(this.Class);
                if (ExtraClass != null && evt.Initiator.Descriptor.HasFact(ExtraFeatureToCheck))
                {
                    classLevel += this.Owner.Progression.GetClassLevel(this.ExtraClass);
                }
                if (!(evt.Spell == this.Ability))
                    return;
                evt.ReplaceCasterLevel = new int?(classLevel <= 0 ? 1 : classLevel);
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }


        public class ContextConditionStrictAlignmentDifference : ContextCondition
        {

            protected override string GetConditionCaption()
            {
                return string.Format("Check Strict Alignment Difference");
            }

            protected override bool CheckCondition()
            {
                if (this.Target.Unit == null)
                {
                    UberDebug.LogError((object)"Target is missing", (object[])Array.Empty<object>());
                    return false;
                }
                if (this.Context.MaybeCaster != null)
                {
                    var caster_goodness = Context.MaybeCaster.Descriptor.Alignment.Value.GetGoodness();
                    var caster_lawfulness = Context.MaybeCaster.Descriptor.Alignment.Value.GetLawfulness();

                    var target_goodness = this.Target.Unit.Descriptor.Alignment.Value.GetGoodness();
                    var target_lawfulness = this.Target.Unit.Descriptor.Alignment.Value.GetLawfulness();

                    return Math.Max(Math.Abs(caster_goodness - target_goodness), Math.Abs(caster_lawfulness - target_lawfulness)) == 2;
                }
                   
                UberDebug.LogError((object)"Caster is missing", (object[])Array.Empty<object>());
                return false;
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


        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class SaveOrReduceDamage : RuleInitiatorLogicComponent<RuleCalculateDamage>
        {
            public float residual_damage = 0.5f;
            public SavingThrowType save_type;
            public bool use_caster_level_and_stat;

            public override void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {
                Rulebook rulebook = Game.Instance.Rulebook;
                int dc = 10;
                if (use_caster_level_and_stat)
                {
                    CharacterStats stats = this.Fact.MaybeContext.MaybeCaster.Stats;
                    var boni = new int[] { stats.Charisma.Bonus, stats.Wisdom.Bonus, stats.Intelligence.Bonus };

                    dc = 10 + this.Fact.MaybeContext.Params.CasterLevel / 2 + boni.Max();
                }
                else
                {
                   dc  = this.Fact.MaybeContext.Params.DC;
                }
                RuleSavingThrow ruleSavingThrow = new RuleSavingThrow(this.Owner.Unit, this.save_type, dc);
                ruleSavingThrow.Reason = (RuleReason)this.Fact;

                if (rulebook.TriggerEvent<RuleSavingThrow>(ruleSavingThrow).IsPassed)
                    return;

                foreach (BaseDamage base_damage in evt.DamageBundle)
                {
                    base_damage.Durability = residual_damage;
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


        [ComponentName("Skill bonus in combat")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowedOn(typeof(BlueprintBuff))]
        [AllowMultipleComponents]
        public class SkillBonusInCombat : RuleInitiatorLogicComponent<RuleSkillCheck>
        {
            public BlueprintUnitFact reason = null;
            public ContextValue value;
            public StatType skill;
            public ModifierDescriptor descriptor;

            public override void OnEventAboutToTrigger(RuleSkillCheck evt)
            {
                if (evt.StatType != skill)
                {
                    return;
                }
                if (reason != null && (evt.Reason.Fact == null || evt.Reason.Fact.Blueprint != reason))
                {
                    return;
                }

                var bonus = value.Calculate(this.Fact.MaybeContext);

                evt.Bonus.AddModifier(bonus, this, descriptor);
            }

            public override void OnEventDidTrigger(RuleSkillCheck evt)
            {
            }
        }



        [AllowMultipleComponents]
        [ComponentName("Saving throw bonus against fact from caster")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class SavingThrowBonusAgainstCaster : RuleInitiatorLogicComponent<RuleSavingThrow>
        {
            public ModifierDescriptor Descriptor;
            public ContextValue Value;

            public override void OnEventAboutToTrigger(RuleSavingThrow evt)
            {
                if (evt.Reason.Caster != this.Fact.MaybeContext?.MaybeCaster)
                {
                    return;
                }
                int bonus = Value.Calculate(this.Fact.MaybeContext);
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveWill.AddModifier(bonus, (GameLogicComponent)this, this.Descriptor));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveReflex.AddModifier(bonus, (GameLogicComponent)this, this.Descriptor));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveFortitude.AddModifier(bonus, (GameLogicComponent)this, this.Descriptor));
            }

            public override void OnEventDidTrigger(RuleSavingThrow evt)
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


        [ComponentName("Weapon Attack Auto Miss")]
        [AllowedOn(typeof(Kingmaker.Blueprints.Facts.BlueprintUnitFact))]
        public class AutoMissChance : RuleTargetLogicComponent<RuleAttackRoll>, ITargetRulebookHandler<RuleAttackRoll>
        {
            public AttackType[] attack_types;
            public bool illusion_effect;
            public ContextValue value;

            public override void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
                if ((evt.Initiator.Descriptor.State.HasCondition(UnitCondition.SeeInvisibility) || evt.Initiator.Descriptor.State.HasCondition(UnitCondition.TrueSeeing)) 
                    && illusion_effect)
                {
                    return;
                }

                var d100 = RulebookEvent.Dice.D(new DiceFormula(1, DiceType.D100));

                var chance = value.Calculate(this.Fact.MaybeContext);
                Common.AddBattleLogMessage(evt.Initiator.CharacterName + $" {(chance >= d100 ? "fais to overcome" : "overcomes")} miss chance on " + this.Owner.CharacterName + $": {d100}/{chance}");
                if (chance < d100)
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


        [ComponentName("Outgoing Weapon Attack Auto Miss")]
        [AllowedOn(typeof(Kingmaker.Blueprints.Facts.BlueprintUnitFact))]
        public class OutgoingWeaponAttackAutoMiss : RuleInitiatorLogicComponent<RuleAttackRoll>, IInitiatorRulebookHandler<RuleAttackRoll>
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


        public class ContextConditionCasterHasFacts : ContextCondition
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
                    if (this.Context.MaybeCaster.Descriptor.HasFact(f) && !all)
                    {
                        return true;
                    }
                    else if (!this.Context.MaybeCaster.Descriptor.HasFact(f) && all)
                    {
                        return false;
                    }
                }
                return all;
            }
        }

        public class EquipmentRestrictionFeature : EquipmentRestriction
        {
            public BlueprintFeature feature;
            public bool Not;

            public override bool CanBeEquippedBy(UnitDescriptor unit)
            {
                return unit.HasFact(feature) != Not;
            }
        }


        public class ContextConditionHasCondtionImmunity : ContextCondition
        {
            public UnitCondition condition;

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

                return unit.Descriptor.State.HasConditionImmunity(condition);
            }
        }


        public class ContextConditionHasCondtion: ContextCondition
        {
            public UnitCondition condition;

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

                return unit.Descriptor.State.HasCondition(condition);
            }
        }


        public class ContextConditionIsMaster : ContextCondition
        {

            protected override string GetConditionCaption()
            {
                return string.Empty;
            }

            protected override bool CheckCondition()
            {
                return (this.Target?.Unit == this.Context?.MaybeCaster?.Descriptor?.Master.Value);
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
        public class AbilityUsedTrigger : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            public BlueprintAbility[] Spells = (BlueprintAbility[])Array.Empty<BlueprintAbility>();
            [NotNull]
            public ActionList Actions = new ActionList();

            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
            }


            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
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


        [ComponentName("BuffMechanics/Extra Attack")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class BuffExtraOffHandAttack : RuleInitiatorLogicComponent<RuleCalculateAttacksCount>
        {
            public int Number = 1;

            public override void OnEventAboutToTrigger(RuleCalculateAttacksCount evt)
            {
                if (evt.Initiator.Body.SecondaryHand.MaybeWeapon != null)
                {
                    evt.SecondaryHand.AdditionalAttacks+= Number;
                }
            }

            public override void OnEventDidTrigger(RuleCalculateAttacksCount evt)
            {
            }
        }

        public class ThirdElementKineticistBonus : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>, 
                                                      IRulebookHandler<RuleCalculateAttackBonusWithoutTarget>, IRulebookHandler<RuleCalculateAbilityParams>
        {
            public int value;

            public void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
            {
                if (evt.Weapon.Blueprint.Category != WeaponCategory.KineticBlast)
                {
                    evt.AddBonus(value, this.Fact);
                }
            }

            public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                var ability_context = Helpers.GetMechanicsContext()?.SourceAbilityContext;
                var component = ability_context?.AssociatedBlueprint?.GetComponent<AbilityKineticist>() ;
                component = component ?? evt.Spell?.GetComponent<AbilityKineticist>();
                if (component == null )
                    return;
                evt.AddBonusDC(value);
            }

            public void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt)
            {

            }

            public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {

            }
        }


        public class addSelectionIfHasFacts : OwnedGameLogicComponent<UnitDescriptor>, ILevelUpCompleteUIHandler
        {
            public BlueprintUnitFact[] facts;
            public BlueprintFeatureSelection selection;

            public void HandleLevelUpComplete(UnitEntityData unit, bool isChargen)
            {
            }

            public override void OnFactActivate()
            {
                try
                {
                    bool is_ok = false;
                    foreach (var f in facts)
                    {
                        if (this.Owner.HasFact(f))
                        {
                            is_ok = true;
                            break;
                        }
                    }
                    if (!is_ok)
                    {
                        return;
                    }


                    var levelUp = Game.Instance.UI.CharacterBuildController.LevelUpController;
                    if (Owner == levelUp.Preview || Owner == levelUp.Unit)
                    {
                        int index = levelUp.State.Selections.Count<FeatureSelectionState>((Func<FeatureSelectionState, bool>)(s => s.Selection == selection));
                        FeatureSelectionState featureSelectionState = new FeatureSelectionState(null, null, selection, index, 0);
                        levelUp.State.Selections.Add(featureSelectionState);
                    }
                }
                catch (Exception e)
                {
                    Main.logger.Error(e.ToString());
                }
            }
        }





        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class SpellCastTrigger : RuleInitiatorLogicComponent<RuleCastSpell>
        {
            public BlueprintAbility[] Spells = (BlueprintAbility[])Array.Empty<BlueprintAbility>();
            [NotNull]
            public ActionList Actions = new ActionList();

            public override void OnEventAboutToTrigger(RuleCastSpell evt)
            {
            }


            public override void OnEventDidTrigger(RuleCastSpell evt)
            {
                if (!Spells.Contains<BlueprintAbility>(evt.Spell.Blueprint))
                    return;

                (this.Fact as IFactContextOwner)?.RunActionInContext(this.Actions, evt.Initiator);
            }
        }

        [AllowedOn(typeof(BlueprintUnitFact))]
        public class TransferDescriptorBonusToTouchAC : OwnedGameLogicComponent<UnitDescriptor>, ITargetRulebookHandler<RuleCalculateAC>, IRulebookHandler<RuleCalculateAC>, ITargetRulebookSubscriber
        {
            public ModifierDescriptor Descriptor;
            public ContextValue value;
            public BlueprintUnitFact required_target_fact;


            public void OnEventAboutToTrigger(RuleCalculateAC evt)
            {
                int bonus = value.Calculate(this.Fact.MaybeContext);
                bonus = bonus > 0 ? Math.Min(bonus, this.Owner.Stats.AC.GetDescriptorBonus(this.Descriptor)) : this.Owner.Stats.AC.GetDescriptorBonus(this.Descriptor);

                if (!evt.AttackType.IsTouch())
                    return;

                if (required_target_fact != null && !evt.Initiator.Descriptor.HasFact(required_target_fact))
                {
                    return;
                }
                evt.AddBonus(bonus, this.Fact);
            }

            public void OnEventDidTrigger(RuleCalculateAC evt)
            {
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

                if (bonus <= 0 || !evt.AttackType.IsTouch())
                {
                    return;
                }
                evt.AddBonus(bonus, this.Fact);
            }

            public void OnEventDidTrigger(RuleCalculateAC evt)
            {
            }
        }

        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilityCasterMainWeaponIsMeleeUnlessHasFact : BlueprintComponent, IAbilityCasterChecker
        {
            public BlueprintFeature ranged_allowed_fact;

            public bool CorrectCaster(UnitEntityData caster)
            {
                var weapon = caster.Body.PrimaryHand.MaybeWeapon;
                if (weapon == null)
                {
                    return true;
                }

                if (weapon.Blueprint.IsMelee || (ranged_allowed_fact != null && caster.Descriptor.HasFact(ranged_allowed_fact)))
                {
                    return true;
                }

                return false;
            }

            public string GetReason()
            {
                return (string)LocalizedTexts.Instance.Reasons.SpecificWeaponRequired;
            }
        }


        [ComponentName("BuffMechanics/Extra Attack")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class BuffExtraAttackIfHasFact : RuleInitiatorLogicComponent<RuleCalculateAttacksCount>
        {
            public BlueprintUnitFact fact;
            public int num_attacks = 1;
            

            public override void OnEventAboutToTrigger(RuleCalculateAttacksCount evt)
            {
                if (evt.Initiator.Descriptor.HasFact(fact))
                {
                    evt.AddExtraAttacks(this.num_attacks, false, (ItemEntity)null);
                }
            }

            public override void OnEventDidTrigger(RuleCalculateAttacksCount evt)
            {
            }
        }


        public class AbilityCasterHpCondition : BlueprintComponent, IAbilityCasterChecker
        {
            public int CurrentHPLessThan;
            public bool Inverted;

            public bool CorrectCaster(UnitEntityData caster)
            {
                if (caster == null)
                    return false;
                if ((int)((ModifiableValue)caster.Stats.HitPoints) - caster.Damage < this.CurrentHPLessThan)
                    return !this.Inverted;
                return this.Inverted;
            }

            public string GetReason()
            {
                return "No enough HP";
            }
        }


        public class DamageBonusAgainstSpellUser : RuleInitiatorLogicComponent<RuleCalculateDamage>
        {
            public ContextValue Value;
            public bool arcane = true;
            public bool divine = true;
            public bool psychic = true;
            public bool spell_like = true;

            private bool isValidTarget(UnitDescriptor unit)
            {
                return Helpers.isValidSpellUser(unit, arcane, divine, psychic, spell_like);
            }

            public override void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {
                if (!isValidTarget(evt.Target.Descriptor))
                {
                    return;
                }

                int bonus = Value.Calculate(this.Fact.MaybeContext);
                evt.DamageBundle.First?.AddBonus(bonus);
            }

            public override void OnEventDidTrigger(RuleCalculateDamage evt)
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


        public class AddWeaponEnergyDamageDice : RuleInitiatorLogicComponent<RuleCalculateWeaponStats>, IRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookSubscriber
        {
            public ContextDiceValue dice_value;
            public DamageEnergyType Element;
            public AttackType[] range_types;
            public WeaponCategory[] categories = new WeaponCategory[0];


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
                if (evt.Weapon == null && !this.range_types.Contains(evt.Weapon.Blueprint.AttackType))
                    return;

                if (!categories.Empty() && !categories.Contains(evt.Weapon.Blueprint.Category))
                {
                    return;
                }
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

            public override void OnEventDidTrigger(RuleCalculateWeaponStats evt)
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
        [AllowedOn(typeof(BlueprintBuff))]
        public class ContextACBonusIfAdjacentCasterWithShield : BuffLogic, ITargetRulebookHandler<RuleAttackRoll>, IRulebookHandler<RuleAttackRoll>, ITargetRulebookSubscriber
        {
            public ContextValue value;
            public ModifierDescriptor descriptor;


            public void OnEventAboutToTrigger(RuleAttackRoll evt)
            {

                var caster = this.Buff.Context.MaybeCaster;
                if (caster == null)
                {
                    return;
                }
                if (this.Owner.Unit.DistanceTo(caster) > 7.Feet().Meters)
                {
                    return;
                }

                if (!caster.Descriptor.Body.HandsAreEnabled || caster.Descriptor.Body.SecondaryHand.MaybeShield == null)
                {
                    return;
                }
                  

                int bonus = value.Calculate(this.Fact.MaybeContext);
                evt.AddTemporaryModifier(evt.Target.Stats.AC.AddModifier(bonus, (GameLogicComponent)this, descriptor));
            }

            public void OnEventDidTrigger(RuleAttackRoll evt)
            {
            }
        }


        
        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class FlankingAttackBonus : RuleInitiatorLogicComponent<RuleAttackRoll>
        {
            public int Bonus;
            public ModifierDescriptor Descriptor;

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
                if (evt.Weapon == null || !evt.Weapon.Blueprint.IsMelee)
                    return;

                if (evt.Target.CombatState.IsFlanked)
                {
                    evt.AddTemporaryModifier(evt.Initiator.Stats.AdditionalAttackBonus.AddModifier(this.Bonus, (GameLogicComponent)this, this.Descriptor));
                }
            }

            public override void OnEventDidTrigger(RuleAttackRoll evt)
            {
            }
        }

        public class DamageBonusIfInvisibleToTarget : RuleInitiatorLogicComponent<RuleCalculateDamage>
        {
            public int Bonus;

            public override void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {
                if (evt.DamageBundle.Weapon?.Blueprint.IsMelee != true) return;

                var initiator = evt.Initiator;
                var target = evt.Target;
                // Flat-footed isn't enough, but we need to run the rule to assess the other variables
                // (such as IgnoreVisibility and IgnoreConcealment)
                var rule = Rulebook.Trigger(new RuleCheckTargetFlatFooted(initiator, target));
                if (rule.IsFlatFooted)
                {
                    var targetCannotSeeUs = target.Descriptor.State.IsHelpless || // sleeping, etc
                        !target.Memory.Contains(initiator) && !rule.IgnoreVisibility || // hasn't seen us, e.g. stealth/ambush
                        UnitPartConcealment.Calculate(target, initiator) == Concealment.Total && !rule.IgnoreConcealment; // invisibility/blindness etc

                    if (targetCannotSeeUs)
                    {
                        evt.DamageBundle.First?.AddBonusTargetRelated(Bonus);
                    }
                }
            }

            public override void OnEventDidTrigger(RuleCalculateDamage evt) { }
        }


        [AllowMultipleComponents]
        [ComponentName("Saving throw bonus against fact")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ContextSavingThrowBonusAgainstFact : RuleInitiatorLogicComponent<RuleSavingThrow>
        {
            public BlueprintUnitFact CheckedFact;
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
        public class SavingThrowBonusAgainstSchoolOrDescriptor : RuleInitiatorLogicComponent<RuleSavingThrow>
        {
            public SpellSchool School;
            public SpellDescriptorWrapper SpellDescriptor;
            public ModifierDescriptor ModifierDescriptor;
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

            public override void OnEventAboutToTrigger(RuleSavingThrow evt)
            {
                if (evt.Reason.Context == null)
                    return;
                SpellSchool? school = evt.Reason.Context?.SourceAbility?.School;

                bool is_ok = (evt.Reason.Context.SpellDescriptor & this.SpellDescriptor) != Kingmaker.Blueprints.Classes.Spells.SpellDescriptor.None;
                is_ok = is_ok || school.GetValueOrDefault() == School;

                if (!is_ok)
                {
                    return;
                }

                int bonus = this.Value.Calculate(this.Context);

                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveWill.AddModifier(bonus, (GameLogicComponent)this, this.ModifierDescriptor));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveReflex.AddModifier(bonus, (GameLogicComponent)this, this.ModifierDescriptor));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveFortitude.AddModifier(bonus, (GameLogicComponent)this, this.ModifierDescriptor));
            }

            public override void OnEventDidTrigger(RuleSavingThrow evt)
            {
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class AttackBonusOnAttacksOfOpportunity : RuleInitiatorLogicComponent<RuleAttackRoll>
        {
            public ContextValue Value;
            public ModifierDescriptor Descriptor;
            public WeaponCategory[] categories = new WeaponCategory[0];

            public override void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
                if (!(evt.RuleAttackWithWeapon?.IsAttackOfOpportunity).GetValueOrDefault())
                    return;
                if (!categories.Empty() && !categories.Contains(evt.Weapon.Blueprint.Category))
                {
                    return;
                }
                evt.AddTemporaryModifier(evt.Initiator.Stats.AdditionalAttackBonus.AddModifier(this.Value.Calculate(this.Fact.MaybeContext), (GameLogicComponent)this, this.Descriptor));
            }

            public override void OnEventDidTrigger(RuleAttackRoll evt)
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


        public class HasEnergyImmunityOrDR : ContextCondition
        {
            public int min_dr = 5;
            public DamageEnergyType energy;

            protected override string GetConditionCaption()
            {
                return string.Empty;
            }

            protected override bool CheckCondition()
            {
                bool has_immunity = false;
                int dr = 0;

                this.Target.Unit.Descriptor?.Buffs?.CallFactComponents<AddEnergyImmunity>(a =>
                {
                    has_immunity = has_immunity || (a.Type == energy);
                });
                if (has_immunity)
                {
                    return true;
                }

                this.Target.Unit.Descriptor.Progression?.Features?.CallFactComponents<AddDamageResistanceEnergy>(a =>
                {
                    has_immunity = has_immunity || (a.Type == energy);
                });
                if (has_immunity)
                {
                    return true;
                }


                this.Target.Unit.Descriptor?.Buffs?.CallFactComponents<AddDamageResistanceEnergy>(a =>
                {
                    if (a.Type == energy)
                    {
                        dr += a.GetValue();
                    }
                });

                this.Target.Unit.Descriptor.Progression?.Features?.CallFactComponents<AddDamageResistanceEnergy>(a =>
                {
                    if (a.Type == energy)
                    {
                        dr += a.GetValue();
                    }
                });

                return dr >= min_dr;

            }
        }


        [ComponentName("Attacks ignore armor natural ac and shields")]
        public class IgnoreAcShieldAndNaturalArmor : RuleInitiatorLogicComponent<RuleCalculateAC>, IRulebookHandler<RuleCalculateAC>, IInitiatorRulebookSubscriber
        {
            public override void OnEventAboutToTrigger(RuleCalculateAC evt)
            {
                evt.BrilliantEnergy = this.Fact;
                int natural_ac_bonus = 0;
                foreach (ModifiableValue.Modifier modifier in evt.Target.Stats.AC.Modifiers)
                {
                    natural_ac_bonus += modifier.ModDescriptor == ModifierDescriptor.NaturalArmor ? modifier.ModValue : 0;
                    natural_ac_bonus += modifier.ModDescriptor == ModifierDescriptor.NaturalArmorEnhancement ? modifier.ModValue : 0;
                }
                evt.AddBonus(-natural_ac_bonus, this.Fact);
                
            }

            public override void OnEventDidTrigger(RuleCalculateAC evt)
            {
            }
        }


        public class ContextActionAttack : ContextAction
        {
            public ActionList action_on_success = null;
            public ActionList action_on_miss = null;
            public ActionList action_on_before_attack = null;

            public BlueprintItemWeapon specific_weapon = null;

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

                    action_on_before_attack?.Run();

                    var current_weapon = maybeCaster.Body.PrimaryHand.MaybeWeapon;
                    if (specific_weapon != null && current_weapon?.Blueprint != specific_weapon)
                    {
                        var weapon = maybeCaster.Body.AdditionalLimbs.Where(a => a.HasWeapon && a.Weapon.Blueprint == specific_weapon).FirstOrDefault();
                        if (weapon != null)
                        {
                            current_weapon = weapon.MaybeWeapon;
                        }
                    }
                    RuleAttackWithWeapon attackWithWeapon = new RuleAttackWithWeapon(maybeCaster, target.Unit, current_weapon, 0);
                    attackWithWeapon.Reason = (RuleReason)this.Context;
                    RuleAttackWithWeapon rule = attackWithWeapon;
                    rule.Reason = this.Context;
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
                return caster.Descriptor.Unit.View.AnimationManager.CreateHandle(UnitAnimationType.MainHandAttack).Action;
            }
        }


        public class DamageBonusForAbilities : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, IInitiatorRulebookSubscriber
        {
            public BlueprintAbility[] abilities;
            public ContextValue value;
            public void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {
                MechanicsContext context = evt.Reason.Context;
                if (context?.SourceAbility == null || evt.DamageBundle.Empty())
                    return;

                if (abilities.Contains(context.SourceAbility) || abilities.Contains(context.SourceAbility?.Parent))
                {
                    evt.DamageBundle.First.AddBonus(value.Calculate(this.Fact.MaybeContext));
                }
            }

            public void OnEventDidTrigger(RuleCalculateDamage evt)
            {
            }
        }


        [ComponentName("Reduces DR against fact owner")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ReduceDRForFactOwner : RuleInitiatorLogicComponent<RuleCalculateDamage>
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

                if (!correct_dc && !evt.Spell.IsSpell)
                {
                    evt.AddBonusDC(-(multiplier * bonus / 2));
                }
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }


        [ComponentName("Increase specific spells CL")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ContextIncreaseCasterLevelForSchool : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            public ContextValue value;
            public SpellSchool school;
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
                if (!evt.Spell.IsSpell)
                {
                    return;
                }


                if (evt.Spell.School != school)
                {
                    return;
                }


                int bonus = this.value.Calculate(this.Context);
                evt.AddBonusCasterLevel(bonus);
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


        public class ActivatableAbilityHasShieldRestriction : ActivatableAbilityRestriction
        {
            public override bool IsAvailable()
            {
                if (Owner.Body.IsPolymorphed)
                {
                    return true;
                }

                return Owner.Body?.SecondaryHand?.MaybeShield != null;

            }
        }


        public class ActivatableAbilityLightOrNoArmor : ActivatableAbilityRestriction
        {
            public override bool IsAvailable()
            {
                if (Owner.Body.IsPolymorphed)
                {
                    return true;
                }

                if (!Owner.Body.Armor.HasArmor)
                {
                    return true;
                }

                if (Owner.Body.Armor.Armor.Blueprint.ProficiencyGroup == ArmorProficiencyGroup.Light 
                    || Owner.Body.Armor.Armor.Blueprint.ProficiencyGroup == ArmorProficiencyGroup.None)
                {
                    return true;
                }

                return false;
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
            public bool not = false;
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
                if (Owner.HasFact(Feature))
                {
                    return;
                }
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

                if ((facts_found == amount) != not)
                {
                    this.m_AppliedFact = this.Owner.AddFact(this.Feature, (MechanicsContext)null, (FeatureParam)null);
                }
            }
        }



        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class AddFeatureIfQuadrupedOrSerpentine : OwnedGameLogicComponent<UnitDescriptor>, IGlobalSubscriber
        {
            public BlueprintFeature Feature;
            public bool not = false;
            private static BlueprintCharacterClass animal = Main.library.Get<BlueprintCharacterClass>("4cd1757a0eea7694ba5c933729a53920");
            [JsonProperty]
            private Fact m_AppliedFact;

            public override void OnFactActivate()
            {
                this.Apply();
            }

            public override void OnFactDeactivate()
            {
                if (m_AppliedFact != null)
                {
                    this.Owner.RemoveFact(this.m_AppliedFact);
                    this.m_AppliedFact = (Fact)null;
                }
            }


            private void Apply()
            {
                if (this.m_AppliedFact != null)
                    return;

                bool is_quadruped = this.Owner.Progression.IsArchetype(Eidolon.quadruped_archetype) || this.Owner.Progression.Classes.Any(c => c.CharacterClass == animal) || this.Owner.Progression.IsArchetype(Eidolon.serpentine_archetype);
                if (is_quadruped != not)
                {
                    this.m_AppliedFact = this.Owner.AddFact(this.Feature, (MechanicsContext)null, (FeatureParam)null);
                }
            }
        }


        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class AddFeatureIfHasArchetype : OwnedGameLogicComponent<UnitDescriptor>, IUnitGainLevelHandler, IGlobalSubscriber
        {
            public bool not = false;
            public BlueprintArchetype archetype;
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
                if (this.m_AppliedFact != null)
                    return;

                var unit = this.Owner;
                

                if (unit == null)
                {
                    return;
                }

                if (unit.Progression.IsArchetype(archetype))
                {
                    this.m_AppliedFact = this.Owner.AddFact(this.Feature, (MechanicsContext)null, (FeatureParam)null);
                }
            }
        }



        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class AddFeatureIfMasterHasFactsFromList : OwnedGameLogicComponent<UnitDescriptor>, IUnitGainLevelHandler, IGlobalSubscriber
        {
            public bool not = false;
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
                var unit = this.Owner.IsPet ? this.Owner.Master.Value?.Descriptor : this.Owner;

                if (unit == null)
                {
                    return;
                }

                foreach (var f in CheckedFacts)
                {
                    if (facts_found == amount)
                    {
                        break;
                    }
                    if (unit.HasFact(f))
                    {
                        facts_found++;
                    }
                }

                if ((facts_found == amount) != not)
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

        [AllowedOn(typeof(BlueprintUnitFact))]
        public class RunActionOnCombatStart : RuleInitiatorLogicComponent<RuleInitiativeRoll>
        {
            public ActionList actions;

            public override void OnEventAboutToTrigger(RuleInitiativeRoll evt)
            {
                (this.Fact as IFactContextOwner).RunActionInContext(actions, this.Owner.Unit);
            }

            public override void OnEventDidTrigger(RuleInitiativeRoll evt)
            {
            }
        }


        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class AddFeatureIfHasFactAndNotHasFactDynamic : OwnedGameLogicComponent<UnitDescriptor>, IUnitGainLevelHandler, IGlobalSubscriber
        {
            public BlueprintUnitFact HasFact;
            public BlueprintUnitFact NotHasFact;
            public BlueprintUnitFact Feature;

            [JsonProperty]
            private Fact m_AppliedFact;

            public override void OnTurnOn()
            {
                this.Apply();
            }

            public override void OnTurnOff()
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
            public bool any;

            public bool IsAbilityVisible(AbilityData ability)
            {
                foreach (var fact in UnitFacts)
                {
                    if (ability.Caster.HasFact(fact) == any)
                    {
                        return any;
                    }
                }
                return !any;
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        public class AbilityShowIfCasterHasAlignment : BlueprintComponent, IAbilityVisibilityProvider
        {
            public AlignmentMaskType alignment;

            public bool IsAbilityVisible(AbilityData ability)
            {
                return (uint)(ability.Caster.Alignment.Value.ToMask() & this.alignment) > 0U;
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilityShowIfCasterKnowsSpell : BlueprintComponent, IAbilityVisibilityProvider
        {
            public bool not;
            public BlueprintSpellbook spellbook;
            public BlueprintAbility spell;
            public bool IsAbilityVisible(AbilityData ability)
            {
                var sb = ability?.Caster?.GetSpellbook(spellbook);
                if (sb == null)
                {
                    return not;
                }

                return sb.IsKnown(spell) != not;
            }

        }


        public class ContextActionClearSummonPoolFromCaster : ContextAction
        {
            public BlueprintSummonPool SummonPool;

            public override string GetCaption()
            {
                return "Clear Summon Pool from caster";
            }

            public override void RunAction()
            {
                ISummonPool pool = Game.Instance.SummonPools.GetPool(this.SummonPool);
                if (pool == null)
                    return;
                foreach (UnitEntityData unit in pool.Units)
                {
                    if (unit.Ensure<UnitPartSummonedMonster>().Summoner == this.Context.MaybeCaster)
                    {
                        unit.Descriptor.RemoveFact((BlueprintUnitFact)Game.Instance.BlueprintRoot.SystemMechanics.SummonedUnitBuff);
                    }
                }
            }
        }



        [AllowedOn(typeof(BlueprintAbility))]
        public class AbilityShowIfCasterHasResource : BlueprintComponent, IAbilityVisibilityProvider
        {
            public BlueprintAbilityResource resource;
            public int amount = 1;

            public bool IsAbilityVisible(AbilityData ability)
            {

                if (resource == null)
                {
                    return true;
                }
                if (ability.Caster.Resources.GetResourceAmount(resource) < amount)
                {
                    return false;
                }

                return true;
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        public class AbilityShowIfCasterHasFactsFromList : BlueprintComponent, IAbilityVisibilityProvider
        {
            public BlueprintUnitFact[] UnitFacts;

            public bool IsAbilityVisible(AbilityData ability)
            {
                foreach (var fact in UnitFacts)
                {
                    if (ability.Caster.Progression.Features.HasFact(fact))
                    {
                        return true;
                    }
                }
                return false;
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


        [AllowMultipleComponents]
        [ComponentName("Predicates/Target point has no units around")]
        [AllowedOn(typeof(BlueprintAbility))]
        public class AbilityTargetPointHasNoUnitsAround : BlueprintComponent, IAbilityTargetChecker
        {
            public float distance;
            public bool CanTarget(UnitEntityData caster, TargetWrapper target)
            {
                return !GameHelper.GetTargetsAround(target.Point, (float)this.distance, true, false).Where(u => u != caster).Any();             
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilityCasterNoUnitsAround : BlueprintComponent, IAbilityCasterChecker
        {
            public float distance;
            public bool CorrectCaster(UnitEntityData caster)
            {
                return !GameHelper.GetTargetsAround(caster.Position, (float)this.distance, true, false).Where(u => u != caster).Any();
            }

            public string GetReason()
            {
                return "There are units around";
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilityCasterPrimaryHandFree : BlueprintComponent, IAbilityCasterChecker
        {
            public bool not;
            public bool for_2h_item = false;
            public bool CorrectCaster(UnitEntityData caster)
            {
                if (!for_2h_item)
                {
                    return not != (!caster.Body.PrimaryHand.HasItem && !HoldingItemsMechanics.Helpers.has2hWeapon(caster.Body.SecondaryHand));
                }
                
                return not != (!caster.Body.PrimaryHand.HasItem && HoldingItemsMechanics.Helpers.hasFreeHand(caster.Body.SecondaryHand));
            }

            public string GetReason()
            {
                return "Primary hand must be free";
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilityCasterSecondaryHandFree : BlueprintComponent, IAbilityCasterChecker
        {
            public bool not;
            public bool CorrectCaster(UnitEntityData caster)
            {
               return not != (!caster.Body.SecondaryHand.HasItem && !HoldingItemsMechanics.Helpers.has2hWeapon(caster.Body.PrimaryHand));
            }

            public string GetReason()
            {
                return "Secondary hand must be free";
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilityCasterCompanionDead : BlueprintComponent, IAbilityCasterChecker
        {
            public bool not;
            public bool CorrectCaster(UnitEntityData caster)
            {
                var pet = caster?.Descriptor?.Pet;
                return not != (pet == null || pet.Descriptor.State.IsDead);
            }

            public string GetReason()
            {
                return $"Companion is {(not ? "dead" : "alive")}";
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilityTargetPrimaryHandFree : BlueprintComponent, IAbilityTargetChecker
        {
            public bool not;
            public bool for_2h_item = false;
            public bool CanTarget(UnitEntityData caster, TargetWrapper target)
            {
                if (target?.Unit == null)
                {
                    return false;
                }
                if (!for_2h_item)
                {
                    return not != (!target.Unit.Body.PrimaryHand.HasItem && !HoldingItemsMechanics.Helpers.has2hWeapon(target.Unit.Body.SecondaryHand));
                }

                return not != (!target.Unit.Body.PrimaryHand.HasItem && HoldingItemsMechanics.Helpers.hasFreeHand(target.Unit.Body.SecondaryHand));
            }

            public string GetReason()
            {
                return "Need free primary hand.";
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilityTargetSecondaryHandFree : BlueprintComponent, IAbilityTargetChecker
        {
            public bool not;
            public bool CanTarget(UnitEntityData caster, TargetWrapper target)
            {
                if (target?.Unit == null)
                {
                    return false;
                }

                return not != (!target.Unit.Body.SecondaryHand.HasItem && !HoldingItemsMechanics.Helpers.has2hWeapon(target.Unit.Body.PrimaryHand));
            }

            public string GetReason()
            {
                return "Need free secondary hand.";
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


        public class PrintMessageOnApply: OwnedGameLogicComponent<UnitDescriptor>
        {

            public override void OnFactActivate()
            {
                Main.logger.Log(this.Fact.Blueprint.name + " applied");
                Main.logger.Log(this.Owner.CharacterName);
            }

            public override void OnFactDeactivate()
            {
                Main.logger.Log(this.Fact.Blueprint.name + " removed");
                Main.logger.Log(this.Owner.CharacterName);
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

            public DamageEnergyType[] energy_types = new DamageEnergyType[0];
            public PhysicalDamageForm[] physical_types = new PhysicalDamageForm[0];

            private void RunAction(UnitEntityData target)
            {
                if (target == null)
                {
                    return;
                }
                if (!this.Actions.HasActions)
                    return;
                (this.Fact as IFactContextOwner)?.RunActionInContext(this.Actions, target);
            }

            public void OnEventAboutToTrigger(RuleDealDamage evt)
            {
            }

            public void OnEventDidTrigger(RuleDealDamage evt)
            {
                var spellbook = Helpers.GetMechanicsContext()?.SourceAbilityContext?.Ability?.Spellbook;
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

                if (received_damage < min_dmg)
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



        public class ActionOnSpellDamage : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, IInitiatorRulebookSubscriber
        {
            public ActionList action;
            public int min_dmg = 1;
            public bool only_critical;
            public bool use_energy;
            public DamageEnergyType energy;

            public SavingThrowType save_type = SavingThrowType.Unknown;
            public SpellDescriptorWrapper descriptor;
            public bool use_existing_save;
            public bool action_only_on_save;

            public void OnEventAboutToTrigger(RuleDealDamage evt)
            {
            }

            public void OnEventDidTrigger(RuleDealDamage evt)
            {
                Common.runActionOnDamageDealt(evt, action, min_dmg, only_critical, save_type, descriptor, use_existing_save, action_only_on_save, energy, use_energy);
                /*if (!this.action.HasActions)
                    return;
                (this.Fact as IFactContextOwner)?.RunActionInContext(this.action, target);*/
            }
        }


        public class ActionOnDamageAbsorbed : OwnedGameLogicComponent<UnitDescriptor>, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ITargetRulebookSubscriber
        {
            public ActionList action;
            public int min_dmg = 1;
            public DamageEnergyType energy;

            public void OnEventAboutToTrigger(RuleDealDamage evt)
            {
            }

            public void OnEventDidTrigger(RuleDealDamage evt)
            {
                var original_damage = 0;
                if (evt.IgnoreDamageReduction)
                {
                    return;
                }
                foreach (var d in evt.ResultDamage)
                {
                    var energy_damage = (d.Source as EnergyDamage);
                    if (energy_damage == null || energy_damage.EnergyType != energy)
                    {
                        continue;
                    }
                    original_damage += (d.ValueWithoutReduction - d.FinalValue);
                }

                if (original_damage >= min_dmg)
                {
                    (this.Fact as IFactContextOwner).RunActionInContext(action, evt.Target);
                }
            }
        }


        public class ActionOnDamageReceived : OwnedGameLogicComponent<UnitDescriptor>, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ITargetRulebookSubscriber
        {
            public ActionList action;
            public int min_dmg = 1;
            public DamageEnergyType energy;

            public void OnEventAboutToTrigger(RuleDealDamage evt)
            {
            }

            public void OnEventDidTrigger(RuleDealDamage evt)
            {
                var damage = 0;

                foreach (var d in evt.ResultDamage)
                {
                    var energy_damage = (d.Source as EnergyDamage);
                    if (energy_damage == null || energy_damage.EnergyType != energy)
                    {
                        continue;
                    }
                    damage += (d.FinalValue);
                }

                if (damage >= min_dmg)
                {
                    (this.Fact as IFactContextOwner).RunActionInContext(action, evt.Target);
                }
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class IgnoreDamageReductionIfTargetHasFact : RuleInitiatorLogicComponent<RuleDealDamage>
        {
            public BlueprintBuff fact;
            public bool from_caster;

            public override void OnEventAboutToTrigger(RuleDealDamage evt)
            {
                if (evt.Target.Descriptor.Buffs.Enumerable.Any(b => b.Blueprint == fact && (!from_caster || b.MaybeContext?.MaybeCaster == evt.Initiator)))
                {
                    evt.IgnoreDamageReduction = true;
                }
            }

            public override void OnEventDidTrigger(RuleDealDamage evt)
            {
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



        [AllowedOn(typeof(BlueprintUnitFact))]
        public class WriteTargetHDtoSharedValue : ContextAction
        {
            public AbilitySharedValue shared_value;
            public int divisor = 1;

            public override string GetCaption()
            {
                return "";
            }

            public override void RunAction()
            {
                var unit = this.Target?.Unit;
                if (unit == null)
                {
                    return;
                }
                this.Context[this.shared_value] = unit.Descriptor.Progression.CharacterLevel / divisor;
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


        [ComponentName("Attack bonus against alignment")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class AttackBonusAgainstAlignment : RuleInitiatorLogicComponent<RuleAttackRoll>
        {
            public AlignmentComponent alignment;
            public ContextValue value;
            public ModifierDescriptor descriptor;

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
                if (evt.Weapon == null || !evt.Target.Descriptor.Alignment.Value.HasComponent(this.alignment))
                    return;
                evt.AddTemporaryModifier(evt.Initiator.Stats.AdditionalAttackBonus.AddModifier(this.value.Calculate(this.Context), (GameLogicComponent)this, this.descriptor));
            }

            public override void OnEventDidTrigger(RuleAttackRoll evt)
            {
            }
        }


        [ComponentName("Attack bonus against alignment")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class DamageBonusAgainstAlignment : RuleInitiatorLogicComponent<RuleAttackWithWeapon>
        {
            public AlignmentComponent alignment;
            public ContextValue value;
            public ModifierDescriptor descriptor;

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

            public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
            {
                if (evt.Weapon == null || !evt.Target.Descriptor.Alignment.Value.HasComponent(this.alignment))
                    return;
                evt.AddTemporaryModifier(evt.Initiator.Stats.AdditionalDamage.AddModifier(this.value.Calculate(this.Context), (GameLogicComponent)this, this.descriptor));
            }

            public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
            }
        }


        [AllowMultipleComponents]
        [ComponentName("Saving throw bonus against fact")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ContextSavingThrowBonusAgainstAlignment : RuleInitiatorLogicComponent<RuleSavingThrow>
        {
            public ModifierDescriptor descriptor;
            public ContextValue value;
            public AlignmentComponent alignment;
            public SavingThrowType save_type;


            public override void OnEventAboutToTrigger(RuleSavingThrow evt)
            {
                UnitDescriptor descriptor = evt.Reason.Caster?.Descriptor;
                if (descriptor == null || !descriptor.Alignment.Value.HasComponent(this.alignment))
                    return;

                var bonus = value.Calculate(this.Fact.MaybeContext);

                if (save_type == SavingThrowType.Will)
                {
                    evt.AddTemporaryModifier(evt.Initiator.Stats.SaveWill.AddModifier(bonus, (GameLogicComponent)this, this.descriptor));
                }
                else if (save_type == SavingThrowType.Reflex)
                {
                    evt.AddTemporaryModifier(evt.Initiator.Stats.SaveReflex.AddModifier(bonus, (GameLogicComponent)this, this.descriptor));
                }
                else if (save_type == SavingThrowType.Fortitude)
                {
                    evt.AddTemporaryModifier(evt.Initiator.Stats.SaveFortitude.AddModifier(bonus, (GameLogicComponent)this, this.descriptor));
                }
            }

            public override void OnEventDidTrigger(RuleSavingThrow evt)
            {
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class ArmorClassBonusAgainstAlignment : RuleInitiatorLogicComponent<RuleAttackWithWeapon>
        {
            public AlignmentComponent alignment;
            public ModifierDescriptor descriptor;
            public ContextValue value;

            public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
            {
                if (!evt.Initiator.Descriptor.Alignment.Value.HasComponent(this.alignment) || evt.Target.Descriptor != this.Owner)
                    return;
                evt.AddTemporaryModifier(evt.Target.Stats.AC.AddModifier(this.value.Calculate(this.Fact.MaybeContext), (GameLogicComponent)this, this.descriptor));
            }

            public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
            }
        }


        public class SpellsDCBonusAgainstAlignment : OwnedGameLogicComponent<UnitDescriptor>, MetamagicFeats.IRuleSavingThrowTriggered
        {
            public AlignmentComponent alignment;
            public ContextValue value;
            public ModifierDescriptor descriptor;

            public void ruleSavingThrowBeforeTrigger(RuleSavingThrow evt)
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

                if (!(context.SourceAbility?.IsSpell).GetValueOrDefault())
                {
                    return;
                }

                if (!evt.Initiator.Descriptor.Alignment.Value.HasComponent(this.alignment))
                {
                    return;
                }


                var bonus = -value.Calculate(this.Fact.MaybeContext);
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveWill.AddModifier(bonus, (GameLogicComponent)this, this.descriptor));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveReflex.AddModifier(bonus, (GameLogicComponent)this, this.descriptor));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveFortitude.AddModifier(bonus, (GameLogicComponent)this, this.descriptor));
            }

            public void ruleSavingThrowTriggered(RuleSavingThrow evt)
            {

            }
        }


        public class SpellsDCBonusAgainstFact : OwnedGameLogicComponent<UnitDescriptor>, MetamagicFeats.IRuleSavingThrowTriggered
        {
            public BlueprintUnitFact fact;
            public ContextValue value;
            public ModifierDescriptor descriptor;

            public void ruleSavingThrowBeforeTrigger(RuleSavingThrow evt)
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

                if (!(context.SourceAbility?.IsSpell).GetValueOrDefault())
                {
                    return;
                }

                if (!evt.Initiator.Descriptor.HasFact(fact))
                {
                    return;
                }


                var bonus = -value.Calculate(this.Fact.MaybeContext);
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveWill.AddModifier(bonus, (GameLogicComponent)this, this.descriptor));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveReflex.AddModifier(bonus, (GameLogicComponent)this, this.descriptor));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveFortitude.AddModifier(bonus, (GameLogicComponent)this, this.descriptor));
            }

            public void ruleSavingThrowTriggered(RuleSavingThrow evt)
            {

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
                    evt.SetReroll(1, true, this.Fact.Name);
                }
            }

            public override void OnEventDidTrigger(RuleRollD20 evt)
            {

            }
        }



        public class AttackOfOpportunityDamgeBonus : RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
        {
            public ModifierDescriptor descriptor;
            public ContextValue value;

            public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
            {
                int num = this.value.Calculate(this.Fact.MaybeContext);
                if (evt.AttackWithWeapon == null || !evt.AttackWithWeapon.IsAttackOfOpportunity)
                    return;
                evt.AddBonusDamage(num);
            }

            public override void OnEventDidTrigger(RuleCalculateWeaponStats evt)
            {
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class RerollOnWeaponCategoryAndSpendResource : RuleInitiatorLogicComponent<RuleRollD20>, ITargetRulebookSubscriber
        {
            public BlueprintParametrizedFeature[] required_features;
            public BlueprintAbilityResource resource;
            public int amount = 1;
            public BlueprintBuff prevent_fact;
            public int apply_cooldown_buff_rounds = 1;
            public BlueprintFeature extra_reroll_feature = null;

            private bool checkFeatures(WeaponCategory category)
            {
                if (prevent_fact != null && this.Owner.HasFact(prevent_fact))
                {
                    return false;
                }
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

                if (resource != null)
                {

                    if (evt.Initiator.Descriptor.Resources.GetResourceAmount(resource) < amount)
                    {
                        return;
                    }
                }

                if (previous_event != null && (previous_event is RuleAttackRoll) && !(previous_event as RuleAttackRoll).IsCriticalRoll)
                {
                    if (extra_reroll_feature != null && evt.Initiator.Descriptor.HasFact(extra_reroll_feature))
                    {
                        evt.SetReroll(3, true, this.Fact.Name);
                    }
                    else
                    {
                        evt.SetReroll(1, true, this.Fact.Name);
                    }
                    if (resource != null)
                    {
                        evt.Initiator.Descriptor.Resources.Spend(resource, amount);
                    }

                    if (apply_cooldown_buff_rounds > 0 && prevent_fact != null)
                    {
                        this.Owner.Buffs.AddBuff(prevent_fact, this.Fact.MaybeContext, apply_cooldown_buff_rounds.Rounds().Seconds);
                    }
                }
            }

            public override void OnEventDidTrigger(RuleRollD20 evt)
            {

            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class BuffExtraAttackCategorySpecific : RuleInitiatorLogicComponent<RuleCalculateAttacksCount>, IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>
        {
            public WeaponCategory[] categories;
            public ContextValue num_attacks = 1;
            public int attack_bonus = 0;

            public override void OnEventAboutToTrigger(RuleCalculateAttacksCount evt)
            {
                if (!this.Owner.Body.PrimaryHand.HasWeapon || !categories.Contains(this.Owner.Body.PrimaryHand.Weapon.Blueprint.Category))
                    return;
                var attacks = num_attacks.Calculate(this.Fact.MaybeContext);
                evt.AddExtraAttacks(attacks, false, (ItemEntity)this.Owner.Body.PrimaryHand.Weapon);
            }

            public void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
            {
                if (evt.Weapon == null)
                    return;
                RulebookEvent rule = evt.Reason.Rule;
                if (rule != null && rule is RuleAttackWithWeapon && !(rule as RuleAttackWithWeapon).IsFullAttack)
                    return;

                if (!categories.Contains(evt.Weapon.Blueprint.Category))
                {
                    return;
                }
                evt.AddBonus(attack_bonus, this.Fact);
            }

            public override void OnEventDidTrigger(RuleCalculateAttacksCount evt)
            {
            }

            public void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt)
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
            public bool use_caster_as_origin = false;
            public bool ignore_direction = false;

            public override string GetCaption()
            {
                return string.Format("Spawn multiple {0} for {1}", !((UnityEngine.Object)this.AreaEffect != (UnityEngine.Object)null) ? (object)"<undefined>" : (object)this.AreaEffect.ToString(), (object)this.DurationValue);
            }

            public override void RunAction()
            {
                var origin = use_caster_as_origin ? this.Context.MaybeCaster.Position : this.Target.Point;
                var nx = new Vector2();
                var ny = new Vector2();
                if (!ignore_direction)
                {
                    nx = (this.Target.Point - this.Context.MaybeCaster.Position).To2D().normalized;
                    ny = new Vector2(-nx.y, nx.x);                 
                }
                foreach (var p in points_around_target)
                {
                    var dp = ignore_direction ? p : (nx * p.x + ny * p.y);
                    var target = new TargetWrapper(origin + dp.To3D());
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

            [JsonProperty]
            private bool applied = false;



            protected override void OnUnitEnter(
              MechanicsContext context,
              AreaEffectEntityData areaEffect,
              UnitEntityData unit)
            {
                if (!this.UnitEnter.HasActions || isFirstRound(areaEffect))
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
                if (!this.UnitExit.HasActions || isFirstRound(areaEffect))
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
                if (!this.UnitMove.HasActions || isFirstRound(areaEffect))
                    return;
                using (new AreaEffectContextData(areaEffect))
                {
                    using (context.GetDataScope((TargetWrapper)unit))
                        this.UnitMove.Run();
                }
            }

            protected override void OnRound(MechanicsContext context, AreaEffectEntityData areaEffect)
            {
                if (!this.Round.HasActions && !this.FirstRound.HasActions)
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
                return Helpers.GetField<TimeSpan>(areaEffect, "m_CreationTime").Add(new TimeSpan(0, 0, 0, 0, 100)) > Game.Instance.TimeController.GameTime;
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
        public class ReplaceCasterLevelOfFactWithContextValue : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            public BlueprintUnitFact Feature;
            public ContextValue value;

            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                if (evt.Blueprint != this.Feature)
                    return;
                evt.ReplaceCasterLevel = new int?(value.Calculate(this.Fact.MaybeContext));
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
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


        public class AbilityCasterHasCondition : BlueprintComponent, IAbilityCasterChecker
        {
            public UnitCondition Condition;
            public bool Not;

            public bool CorrectCaster(UnitEntityData caster)
            {
                if (caster == null)
                    return false;
                bool flag = caster.Descriptor.State.HasCondition(this.Condition);
                if (!this.Not & flag)
                    return true;
                if (this.Not)
                    return !flag;
                return false;
            }

            public string GetReason()
            {
                return (string)LocalizedTexts.Instance.Reasons.NoRequiredCondition;
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





        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ReplaceSaveStatForSpellDescriptor : RuleInitiatorLogicComponent<RuleSavingThrow>
        {
            public StatType old_stat;
            public StatType new_stat;
            public SavingThrowType save_type;
            public bool keep_penalty;
            public SpellDescriptorWrapper spell_descriptor;

            StatType getSaveStat()
            {
                if (save_type == SavingThrowType.Fortitude)
                {
                    return StatType.SaveFortitude;
                }
                if (save_type == SavingThrowType.Will)
                {
                    return StatType.SaveWill;
                }
                if (save_type == SavingThrowType.Reflex)
                {
                    return StatType.SaveReflex;
                }

                return StatType.SaveReflex;
            }

            public override void OnEventAboutToTrigger(RuleSavingThrow evt)
            {
                if (evt.Reason?.Context == null)
                {
                    return;
                }
                
                if ((evt.Reason.Context.SpellDescriptor & spell_descriptor) == 0)
                {
                    return;
                }
                
                if (evt.Type != save_type)
                {
                    return;
                }
                
                if (this.Owner.Stats.GetStat<ModifiableValueSavingThrow>(getSaveStat()).BaseStat.Type != old_stat)
                {
                    return;
                }
                
                int bonus = this.Owner.Stats.GetStat<ModifiableValueAttributeStat>(new_stat).Bonus;
                int old_bonus = this.Owner.Stats.GetStat<ModifiableValueAttributeStat>(old_stat).Bonus;
                if (keep_penalty)
                {
                    old_bonus = Math.Max(old_bonus, 0);
                }
                bonus = bonus - old_bonus;

                evt.AddTemporaryModifier(evt.Initiator.Stats.GetStat<ModifiableValueSavingThrow>(getSaveStat()).AddModifier(bonus, (GameLogicComponent)this, ModifierDescriptor.UntypedStackable));
            }

            public override void OnEventDidTrigger(RuleSavingThrow evt)
            {

            }
        }


        /*public class DemoralizeWithAction : ContextAction
        {
            public BlueprintBuff Buff;
            public BlueprintBuff GreaterBuff;
            public bool DazzlingDisplay;
            public BlueprintFeature SwordlordProwessFeature;
            public BlueprintFeature ShatterConfidenceFeature;
            public BlueprintBuff ShatterConfidenceBuff;
            public ActionList actions;
            public int bonus = 0;
            public ModifierDescriptor modifier_descriptor;

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
                        var skill_check = new RuleSkillCheck(maybeCaster, StatType.CheckIntimidate, dc);
                        if (bonus > 0)
                        {
                            skill_check.Bonus.AddModifier(bonus, null, modifier_descriptor);
                        }
                        RuleSkillCheck ruleSkillCheck = context.TriggerRule<RuleSkillCheck>(skill_check);
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
        }*/

        /*public class ActionOnDemoralize : ContextAction
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
        }*/

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

        public class ContextConditionIsCasterAndHasFact : ContextCondition
        {
            public BlueprintUnitFact fact;

            protected override string GetConditionCaption()
            {
                return "Is caster and has fact";
            }

            protected override bool CheckCondition()
            {
                UnitEntityData maybeCaster = this.Context.MaybeCaster;
                if (maybeCaster == null)
                {
                    UberDebug.LogError((object)"Caster is missing", (object[])Array.Empty<object>());
                    return false;
                }
                UnitEntityData unit = this.Target.Unit;
                if (unit != null)
                    return unit == maybeCaster && maybeCaster.Descriptor.HasFact(fact);
                UberDebug.LogError((object)"Target unit is missing", (object[])Array.Empty<object>());
                return false;
            }
        }


        public class ContextConditionIsAllyAndCasterHasFact : ContextCondition
        {
            public BlueprintUnitFact fact;

            protected override string GetConditionCaption()
            {
                return "Is ally and caster has fact";
            }

            protected override bool CheckCondition()
            {
                UnitEntityData maybeCaster = this.Context.MaybeCaster;
                if (maybeCaster == null)
                {
                    UberDebug.LogError((object)"Caster is missing", (object[])Array.Empty<object>());
                    return false;
                }
                UnitEntityData unit = this.Target.Unit;
                if (unit != null)
                    return maybeCaster.IsAlly(unit) && maybeCaster.Descriptor.HasFact(fact);
                UberDebug.LogError((object)"Target unit is missing", (object[])Array.Empty<object>());
                return false;
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilityTargetIsPet : BlueprintComponent, IAbilityTargetChecker
        {
            public bool CanTarget(UnitEntityData caster, TargetWrapper target)
            {
                return caster.Descriptor.Pet != null && caster.Descriptor.Pet == target.Unit;
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilityTargetIsAnimalCompanion : BlueprintComponent, IAbilityTargetChecker
        {
            public bool CanTarget(UnitEntityData caster, TargetWrapper target)
            {
                return target.Unit?.Descriptor?.Master.Value != null;
            }
        }



        public class ContextActionsOnMaster : ContextAction
        {
            public ActionList Actions;

            public override void RunAction()
            {
                if (this.Target.Unit == null)
                {
                    UberDebug.LogError((object)"Target unit is missing", (object[])Array.Empty<object>());
                }
                else
                {
                    if (this.Target.Unit.Descriptor.Master.Value == null)
                        return;
                    using (this.Context.GetDataScope((TargetWrapper)this.Target.Unit.Descriptor.Master.Value))
                        this.Actions.Run();
                }
            }

            public override string GetCaption()
            {
                return "Run actions on targets pet";
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
            public ModifyD20WithActions.InnerSavingThrowType m_SavingThrowType = ModifyD20WithActions.InnerSavingThrowType.All;
            [EnumFlagsAsButtons(ColumnCount = 3)]
            public ModifyD20WithActions.RuleType Rule;
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

            public BlueprintAbilityResource required_resource = null;

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


            private bool IsConentrationCheck
            {
                get
                {
                    return (this.Rule & RuleType.Concentration) != (RuleType)0;
                }
            }

            public override void OnEventAboutToTrigger(RuleRollD20 evt)
            {
                if (required_resource != null && this.Fact.MaybeContext.MaybeCaster.Descriptor.Resources.GetResourceAmount(required_resource) < 1)
                {
                    return;
                }
                this.m_Roll = null;
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
                {
                    evt.SetReroll(this.RollsAmount, this.TakeBest, this.Fact.Name);
                }
            }

            private static bool IsRollFailed(int roll, RulebookEvent evt)
            {
                if (evt is RuleAttackRoll)
                {
                    var evt2 = evt as RuleAttackRoll;
                    if (roll <= 1)
                    {
                        return true;
                    }
                    if (roll == 20)
                    {
                        return false;
                    }

                    return roll + evt2.AttackBonus < evt2.TargetAC;
                }
                else if (evt is RuleSkillCheck)
                {
                    return !(evt as RuleSkillCheck).IsSuccessRoll(roll);
                }
                else if (evt is RuleSavingThrow)
                {
                    return !(evt as RuleSavingThrow).IsSuccessRoll(roll);
                }
                else if (evt is RuleCombatManeuver)
                {
                    return !(evt as RuleCombatManeuver).IsSuccessRoll(roll);
                }
                else if (evt is RuleSpellResistanceCheck)
                {
                    return (evt as RuleSpellResistanceCheck).SpellResistance > (evt as RuleSpellResistanceCheck).SpellPenetration + roll;
                }
                else if (evt is RuleCheckConcentration)
                {
                    return (evt as RuleCheckConcentration).DC > (evt as RuleCheckConcentration).Concentration + roll;
                }
                else if (evt is RuleCheckCastingDefensively)
                {
                    return (evt as RuleCheckCastingDefensively).DC > (evt as RuleCheckCastingDefensively).Concentration + roll;
                }

                return false;
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
                if (this.IsConentrationCheck && (rule is RuleCheckConcentration || rule is RuleCheckCastingDefensively))
                {
                    return true;
                }
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
                //isFlanked here is used to determine that enemy is engaged by at least two units and not for actual flanking
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
            public enum InnerSavingThrowType
            {
                Fortitude = 1,
                Reflex = 2,
                Will = 4,
                All = Will | Reflex | Fortitude, // 0x00000007
            }


            [Flags]
            public enum RuleType
            {
                AttackRoll = 1,
                SkillCheck = 2,
                SavingThrow = 4,
                Intiative = 8,
                Maneuver = 16,
                SpellResistance = 32,
                Concentration = 64,
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
            public BlueprintUnitFact[] checked_fact = new BlueprintUnitFact[0];
            public bool only_melee = false;

            public void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
                UnitEntityData maybeCaster = this.Buff.Context.MaybeCaster;
                bool flag1 = this.CheckCaster && evt.Target == maybeCaster;
                bool flag2 = this.CheckCasterFriend && maybeCaster != null && evt.Target.GroupId == maybeCaster.GroupId && evt.Target != maybeCaster;
                if (!flag1 && !flag2)
                    return;



                bool has_fact = checked_fact.Empty();
                foreach (var f in checked_fact)
                {
                    if (has_fact)
                    {
                        break;
                    }
                    has_fact = evt.Target.Descriptor.HasFact(f);
                }

                if (! has_fact)
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
                Main.TraceLog();
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


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ReduceMaxHp : OwnedGameLogicComponent<UnitDescriptor>, IUnitGainLevelHandler, IGlobalSubscriber
        {
            private ModifiableValue.Modifier m_Modifier;
            public int hp_percent;
            public override void OnTurnOn()
            {
                this.Apply();
            }

            public override void OnTurnOff()
            {
                this.m_Modifier?.Remove();
                this.m_Modifier = (ModifiableValue.Modifier)null;
            }

            public void HandleUnitGainLevel(UnitDescriptor unit, BlueprintCharacterClass @class)
            {
                this.Apply();
            }

            private void Apply()
            {
                
                this.m_Modifier?.Remove();
                var current_hp = this.Owner.Stats.HitPoints.ModifiedValue;
                int remove_hp = hp_percent * current_hp /100;
                this.m_Modifier = this.Owner.Stats.HitPoints.AddModifier(-remove_hp, (GameLogicComponent)this, ModifierDescriptor.UntypedStackable);
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        public class AbilityShowIfCasterHasFact2 : BlueprintComponent, IAbilityVisibilityProvider
        {
            public BlueprintUnitFact UnitFact;

            public bool IsAbilityVisible(AbilityData ability)
            {
                return ability.Caster.Progression.Features.HasFact((BlueprintFact)this.UnitFact) || ability.Caster.Buffs.HasFact(UnitFact);
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        public class AbilityShowIfCasterProficientWithWeaponCategory : BlueprintComponent, IAbilityVisibilityProvider
        {
            public WeaponCategory category;
            public bool require_full_proficiency = false;
            public bool IsAbilityVisible(AbilityData ability)
            {
                if (require_full_proficiency 
                    && (WeaponCategory.BastardSword == category || WeaponCategory.DwarvenWaraxe == category))
                {
                    return (ability.Caster.Get<WeaponsFix.UnitPartFullProficiency>()?.hasFullProficiency(category)).GetValueOrDefault();
                }
                return ability.Caster.Proficiencies.Contains(category);
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        public class AbilityShowIfHasClassSpellLevel : BlueprintComponent, IAbilityVisibilityProvider
        {
            public BlueprintCharacterClass character_class;
            public int level;

            public bool IsAbilityVisible(AbilityData ability)
            {
                ClassData classData = ability.Caster.Progression.GetClassData(this.character_class);
                return classData?.Spellbook != null && ability.Caster.DemandSpellbook(classData.CharacterClass).MaxSpellLevel >= level;
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        public class AbilityShowIfHasClassLevel : BlueprintComponent, IAbilityVisibilityProvider
        {
            public BlueprintCharacterClass character_class;
            public int level;

            public bool IsAbilityVisible(AbilityData ability)
            {
                return ability.Caster.Progression.GetClassLevel(character_class) >= level;
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        public class AbilityShowIfHasClassLevels : BlueprintComponent, IAbilityVisibilityProvider
        {
            public BlueprintCharacterClass[] character_classes;
            public int level;

            public bool IsAbilityVisible(AbilityData ability)
            {
                var lvl = 0;
                foreach (var c in character_classes)
                {
                    lvl += ability.Caster.Progression.GetClassLevel(c);
                }
                return lvl >= level;
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


        public class ContextConditionCriticalHitFromCaster : ContextCondition
        {
            protected override string GetConditionCaption()
            {
                return string.Empty;
            }

            public int required_edge = 0;

            protected override bool CheckCondition()
            {
                var caster = this.Context.MaybeCaster;

                var attack = Rulebook.CurrentContext.AllEvents.LastOfType<RuleAttackWithWeapon>()?.AttackRoll;

                if (attack == null || attack.Initiator != caster || attack.WeaponStats == null)
                {
                    return false;
                }

                return attack.WeaponStats.CriticalEdge == required_edge || required_edge == 0;               
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


        public class addClassSpellChoice : OwnedGameLogicComponent<UnitDescriptor>, ILevelUpCompleteUIHandler
        {
            [JsonProperty]
            bool applied;
            public BlueprintSpellList spell_list;
            public int spell_level;
            public BlueprintCharacterClass character_class;

            public void HandleLevelUpComplete(UnitEntityData unit, bool isChargen)
            {
            }

            public override void OnFactActivate()
            {
                var spell_book = this.Owner.GetSpellbook(character_class)?.Blueprint;              
                try
                {
                    var levelUp = Game.Instance.UI.CharacterBuildController?.LevelUpController;
                    if (Owner == levelUp?.Preview || Owner == levelUp?.Unit)
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


            public override void OnFactDeactivate()
            {
                var spell_book = this.Owner.GetSpellbook(character_class)?.Blueprint;
                try
                {
                    var levelUp = Game.Instance.UI.CharacterBuildController?.LevelUpController;
                    if (Owner == levelUp?.Preview || Owner == levelUp?.Unit)
                    {
                        var spellSelection = levelUp.State.DemandSpellSelection(spell_book, spell_list);
                        int existingNewSpells = spellSelection.LevelCount[spell_level]?.SpellSelections.Length ?? 0;
                        spellSelection.SetLevelSpells(spell_level, existingNewSpells - 1);
                        applied = false;
                    }

                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
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
                    var levelUp = Game.Instance.UI.CharacterBuildController?.LevelUpController;
                    if (Owner == levelUp?.Preview || Owner == levelUp?.Unit)
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


            public override void OnFactDeactivate()
            {
                try
                {
                    var levelUp = Game.Instance.UI.CharacterBuildController?.LevelUpController;
                    if (Owner == levelUp?.Preview || Owner == levelUp?.Unit)
                    {
                        var spellSelection = levelUp.State.DemandSpellSelection(spell_book, spell_list);
                        int existingNewSpells = spellSelection.LevelCount[spell_level]?.SpellSelections.Length ?? 0;
                        spellSelection.SetLevelSpells(spell_level, existingNewSpells - 1);
                        applied = false;
                    }

                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }


        public class ContextActionOnEngagedTargets : ContextAction
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


        public class ContextActionOnTargetsInReach : ContextAction
        {
            public ActionList actions;
            public bool only_enemies = true;

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

                var treat_hand = this.Target.Unit.GetThreatHand();
                if (treat_hand == null)
                {
                    return;
                }
                var units = GameHelper.GetTargetsAround(this.Target.Unit.Position, 30.Feet().Meters, false, false);

                foreach (UnitEntityData engagee in units)
                {
                    if (engagee == this.Target.Unit)
                    {
                        continue;
                    }
                    if ((engagee.IsEnemy(this.Target.Unit) || !only_enemies) && this.Target.Unit.IsReach(engagee, treat_hand))
                    {
                        using (this.Context.GetDataScope((TargetWrapper)engagee))
                        {
                            this.actions.Run();
                        }
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
                if (context?.SourceAbility == null || !context.SourceAbility.IsSpell)
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
            public bool only_energy = true;

            public void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {
                MechanicsContext context = evt.Reason.Context;
                if (context?.SourceAbility == null || !context.SourceAbility.IsSpell || !context.SpellDescriptor.HasAnyFlag((Kingmaker.Blueprints.Classes.Spells.SpellDescriptor)this.SpellDescriptor))
                    return;

                var dmg = value.Calculate(this.Fact.MaybeContext);
                if (!only_energy)
                {
                    evt.DamageBundle.First?.AddBonus(dmg);
                }
                else
                {
                    evt.DamageBundle.First(d => d is EnergyDamage)?.AddBonus(dmg);
                }
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



        public class FlurryOfBlowsRestriciton : ActivatableAbilityRestriction
        {

            public override bool IsAvailable()
            {
                return !HoldingItemsMechanics.Helpers.hasShield2(this.Owner.Body.SecondaryHand)
                                    && (!this.Owner.Body.Armor.HasArmor || !this.Owner.Body.Armor.Armor.Blueprint.IsArmor)
                                    && (this.Owner.Body.PrimaryHand.Weapon.Blueprint.IsMonk || FeralCombatTraining.checkHasFeralCombat(this.Owner.Unit, this.Owner.Body.PrimaryHand.Weapon, allow_crusaders_flurry: true));
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


        public class OutdoorsUnlessHasFact : ActivatableAbilityRestriction
        {
            public BlueprintUnitFact fact;
            public override bool IsAvailable()
            {
                bool indoor = Game.Instance.CurrentlyLoadedArea.IsIndoor;

                return !indoor || this.Owner.HasFact(fact);
            }
        }


        public class RemoveUniqueArea : ContextAction
        {
            public BlueprintUnitFact feature;

            public override string GetCaption()
            {
                return "Remove unique Area";
            }

            public override void RunAction()
            {
                var area = this.Context.MaybeCaster.Ensure<UnitPartUniqueAreaEffects>().Areas.Where(a => a.Feature == feature).FirstOrDefault();
                if (area == null)
                {
                    return;
                }
                var areaEffect = Game.Instance.State.AreaEffects[area.AreaId];
                this.Context.MaybeCaster.Ensure<UnitPartUniqueAreaEffects>().RemoveAreaEffect(areaEffect, feature);
            }
        }



        public class ContextActionRangedTouchAttack : ContextAction
        {
            public BlueprintItemWeapon Weapon;

            public ActionList OnHit, OnMiss;

            internal static ContextActionRangedTouchAttack Create(GameAction[] onHit, GameAction[] onMiss = null)
            {
                var r = Helpers.Create<ContextActionRangedTouchAttack>();
                r.Weapon = Main.library.Get<BlueprintItemWeapon>("f6ef95b1f7bb52b408a5b345a330ffe8");
                r.OnHit = Helpers.CreateActionList(onHit);
                r.OnMiss = Helpers.CreateActionList(onMiss);
                return r;
            }

            public override string GetCaption() => $"Ranged touch attack";

            public override void RunAction()
            {
                try
                {
                    var weapon = Weapon.CreateEntity<ItemEntityWeapon>();
                    var context = AbilityContext;
                    var attackRoll = context.AttackRoll ?? new RuleAttackRoll(context.MaybeCaster, Target.Unit, weapon, 0);
                    attackRoll = context.TriggerRule(attackRoll);
                    //if (context.ForceAlwaysHit) attackRoll.SetFake(AttackResult.Hit);
                    Log.Write($"Ranged touch attack on {Target.Unit}, hit? {attackRoll.IsHit}");
                    if (attackRoll.IsHit)
                    {
                        OnHit.Run();
                    }
                    else
                    {
                        OnMiss.Run();
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }


        public class BloodHavoc : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, IInitiatorRulebookSubscriber
        {
            public BlueprintParametrizedFeature feature;
            static public Dictionary<string, List<BlueprintFeature>> spell_asset_id_feature_map;

            public void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {

                var ability = evt.Reason.Context?.SourceAbility;
                if (ability == null)
                {
                    return;
                }

                if (!ability.IsSpell)
                {
                    return;
                }

                if (spell_asset_id_feature_map.ContainsKey(ability.AssetGuid))
                {
                    foreach (var f in spell_asset_id_feature_map[ability.AssetGuid])
                    {
                        if (this.Owner.HasFact(f))
                        {
                            foreach (BaseDamage baseDamage in evt.DamageBundle)
                                baseDamage.AddBonus(baseDamage.Dice.Rolls);
                            return;
                        }
                    }
                }

                var school = ability.School;
                if (school == SpellSchool.None)
                {
                    return;
                }

                if (feature != null && this.Owner.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => p.Blueprint == feature).Any(p => p.Param == school))
                {
                    foreach (BaseDamage baseDamage in evt.DamageBundle)
                        baseDamage.AddBonus(baseDamage.Dice.Rolls);
                    return;
                }


            }

            public void OnEventDidTrigger(RuleCalculateDamage evt)
            {
            }
        }


        public class SpellLevelIncreaseParametrized : ParametrizedFeatureComponent, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookSubscriber
        {
            public int bonus_dc = 1;

            public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                BlueprintAbility spell = evt.Spell;
                SpellSchool? nullable = spell != null ? spell.GetComponent<SpellComponent>()?.School : new SpellSchool?();
                if (!((!nullable.HasValue ? (FeatureParam)null : (FeatureParam)nullable.GetValueOrDefault()) == this.Param))
                    return;
                evt.AddBonusCasterLevel(bonus_dc);
            }

            public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }

        public class ContextConditionAlignmentStrict : ContextCondition
        {
            public bool CheckCaster;
            public AlignmentMaskType Alignment;

            protected override string GetConditionCaption()
            {
                return string.Format("Check if {0} is {1}", this.CheckCaster ? (object)"caster" : (object)"target", (object)this.Alignment);
            }

            protected override bool CheckCondition()
            {
                UnitEntityData unitEntityData = this.CheckCaster ? this.Context.MaybeCaster : this.Target.Unit;
                if (unitEntityData != null)
                    return (unitEntityData.Descriptor.Alignment.Value.ToMask() & this.Alignment) > 0U;
                UberDebug.LogError((object)"Target is missing", (object[])Array.Empty<object>());
                return false;
            }
        }

        public class ContextConditionAlignmentUnlessCasterHasFact : ContextCondition
        {
            public bool CheckCaster;
            public AlignmentComponent Alignment;
            public BlueprintUnitFact fact;

            protected override string GetConditionCaption()
            {
                return string.Format("Check if {0} is {1}", this.CheckCaster ? (object)"caster" : (object)"target", (object)this.Alignment);
            }

            protected override bool CheckCondition()
            {
                UnitEntityData unitEntityData = this.CheckCaster ? this.Context.MaybeCaster : this.Target.Unit;
                if (unitEntityData != null)
                    return unitEntityData.Descriptor.Alignment.Value.HasComponent(this.Alignment) || this.Context.MaybeCaster.Descriptor.HasFact(fact);
                UberDebug.LogError((object)"Target is missing", (object[])Array.Empty<object>());
                return false;
            }
        }


        public class ContextConditionHasFactsOrClassLevelsUnlessCasterHasFact : ContextCondition
        {
            public bool CheckCaster;
            public BlueprintUnitFact[] facts = new BlueprintUnitFact[0];
            public BlueprintCharacterClass[] classes = new BlueprintCharacterClass[0];
            public BlueprintUnitFact caster_fact;

            protected override string GetConditionCaption()
            {
                return string.Format("Check if target has facts or class levels");
            }

            protected override bool CheckCondition()
            {
                UnitEntityData unit = this.CheckCaster ? this.Context.MaybeCaster : this.Target.Unit;
                if (unit == null)
                {
                    return false;
                }

                if (caster_fact != null && this.Context.MaybeCaster.Descriptor.HasFact(caster_fact))
                {
                    return true;
                }

                foreach (var f in facts)
                {
                    if (unit.Descriptor.HasFact(f))
                    {
                        return true;
                    }
                }

                foreach (var c in classes)
                {
                    if (unit.Descriptor.Progression.Classes.Any(cd => cd.CharacterClass == c && cd.Level > 0))
                    {
                        return true;
                    }
                }

                return false;
            }
        }


        [AllowedOn(typeof(BlueprintParametrizedFeature))]
        public class SpellPerfectionDoubleFeatBonuses : ParametrizedFeatureComponent, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookHandler<RuleSpellResistanceCheck>, IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>, IInitiatorRulebookSubscriber
        {
            public BlueprintUnitFact[] spell_resistance_feats;
            public BlueprintUnitFact[] spell_parameters_feats;
            public BlueprintUnitFact[] attack_roll_feats;
            public BlueprintUnitFact[] attatck_feats;


            public void OnEventAboutToTrigger(RuleSpellResistanceCheck evt)
            {
                var spell = Param?.Blueprint as BlueprintAbility;
                if (!SpellDuplicates.isDuplicateOrParent(spell, evt.Ability))
                {
                    return;
                }

                // Double spell penetration bonus.
                foreach (var fact in Owner.Progression.Features.Enumerable)
                {
                    if (!spell_resistance_feats.Contains(fact.Blueprint))
                    {
                        continue;
                    }
                    foreach (var c in fact.SelectComponents<SpellPenetrationBonus>())
                    {
                        c.OnEventAboutToTrigger(evt);
                    }
                }
            }

            public void OnEventDidTrigger(RuleSpellResistanceCheck evt)
            {
            }

            public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                var spell = Param?.Blueprint as BlueprintAbility;
                if (!SpellDuplicates.isDuplicateOrParent(spell, evt.Spell))
                {
                    return;
                }

                // Double spell penetration bonus.
                foreach (var fact in Owner.Progression.Features.Enumerable)
                {
                    if (!spell_parameters_feats.Contains(fact.Blueprint))
                    {
                        continue;
                    }
                    foreach (var c in fact.SelectComponents<IInitiatorRulebookHandler<RuleCalculateAbilityParams>>())
                    {
                        c.OnEventAboutToTrigger(evt);
                    }
                }
            }

            public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {

            }

            public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
            {
                var spell = Param?.Blueprint as BlueprintAbility;
                var evt_spell = ElementsContext.GetData<MechanicsContext.Data>()?.Context?.SourceAbility;

                if (!SpellDuplicates.isDuplicateOrParent(spell, evt_spell))
                {
                    return;
                }

                // Double spell penetration bonus.
                foreach (var fact in Owner.Progression.Features.Enumerable)
                {
                    if (!attatck_feats.Contains(fact.Blueprint))
                    {
                        continue;
                    }
                    foreach (var c in fact.SelectComponents<IInitiatorRulebookHandler<RuleCalculateWeaponStats>>())
                    {
                        c.OnEventAboutToTrigger(evt);
                    }
                }
            }

            public void OnEventDidTrigger(RuleCalculateWeaponStats evt)
            {

            }

            public void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
            {
                var spell = Param?.Blueprint as BlueprintAbility;
                var evt_spell = ElementsContext.GetData<MechanicsContext.Data>()?.Context?.SourceAbility;

                if (!SpellDuplicates.isDuplicateOrParent(spell, evt_spell))
                {
                    return;
                }

                // Double spell penetration bonus.
                foreach (var fact in Owner.Progression.Features.Enumerable)
                {
                    if (!attack_roll_feats.Contains(fact.Blueprint))
                    {
                        continue;
                    }
                    foreach (var c in fact.SelectComponents<IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>>())
                    {
                        c.OnEventAboutToTrigger(evt);
                    }
                }
            }

            public void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt)
            {

            }
        }


        public class IncomingPhysicalDamageMaterialBonus : OwnedGameLogicComponent<UnitDescriptor>, ITargetRulebookHandler<RuleCalculateDamage>
        {
            public PhysicalDamageMaterial material;
            public int amount = 1;

            public void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {
                var weaponDamage = evt.DamageBundle.WeaponDamage as PhysicalDamage;
                if (weaponDamage != null && (weaponDamage.MaterialsMask & material) != 0)
                {
                    weaponDamage.AddBonusTargetRelated(amount);
                }
            }

            public void OnEventDidTrigger(RuleCalculateDamage evt)
            {
            }
        }


        public class PrerequisiteCharacterLevelExact : Prerequisite
        {
            public int level;

            public override bool Check(FeatureSelectionState selectionState, UnitDescriptor unit, LevelUpState state)
            {
                return unit.Progression.CharacterLevel == level;
            }

            public override string GetUIText() => $"{UIStrings.Instance.Tooltips.CharacterLevel}: {level}";
        }


        public class PrerequisiteNoClassSkill : Prerequisite
        {
            public StatType skill;
            public override bool Check(FeatureSelectionState selectionState, UnitDescriptor unit, LevelUpState state)
            {
                return !(bool)unit.Stats.GetStat<ModifiableValueSkill>(skill).ClassSkill;
            }

            public override string GetUIText() => $"No class skill: {LocalizedTexts.Instance.Stats.GetText(skill)}";
        }



        [AllowedOn(typeof(BlueprintUnitFact))]
        public class IncreaseAllSpellsDCForSpecificSpellbook : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            public ContextValue Value;
            public BlueprintSpellbook spellbook;
            public BlueprintCharacterClass specific_class;
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
                if (spellbook != null && evt.Spellbook?.Blueprint != spellbook)
                {
                    return;
                }

                var class_spellbook = specific_class == null ? null : evt.Initiator.Descriptor.GetSpellbook(specific_class);
                if (specific_class != null && (class_spellbook == null || evt.Spellbook != evt.Initiator.Descriptor.GetSpellbook(specific_class)))
                {
                    return;
                }

                evt.AddBonusDC(this.Value.Calculate(this.Context));
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class IncreaseAllSpellsCLForSpecificSpellbook : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            public ContextValue Value;
            public BlueprintSpellbook spellbook;
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
                if (evt.Spellbook?.Blueprint != spellbook)
                {
                    return;
                }
                evt.AddBonusCasterLevel(this.Value.Calculate(this.Context));
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }


        [ComponentName("Spontaneous Spell Conversion to Spellbook")]
        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintUnit))]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class SpontaneousSpellConversionForSpellbook : OwnedGameLogicComponent<UnitDescriptor>
        {
            [NotNull]
            public BlueprintSpellbook spellbook;
            public BlueprintAbility[] SpellsByLevel;

            public override void OnTurnOn()
            {
                this.Owner.DemandSpellbook(this.spellbook).AddSpellConversionList(this.SpellsByLevel);
            }

            public override void OnTurnOff()
            {
                this.Owner.DemandSpellbook(this.spellbook).RemoveSpellConversionList(this.SpellsByLevel);
            }
        }

        [ComponentName("Spontaneous Spell Conversion to arcanist Spellbook")]
        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintUnit))]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class SpontaneousSpellConversionForArcanistSpellbook : OwnedGameLogicComponent<UnitDescriptor>
        {
            public BlueprintAbility[] SpellsByLevel;

            public override void OnTurnOn()
            {
                this.Owner.DemandSpellbook(this.Owner.Get<SpellManipulationMechanics.UnitPartArcanistPreparedMetamagic>().spellbook).AddSpellConversionList(this.SpellsByLevel);
            }

            public override void OnTurnOff()
            {
                this.Owner.DemandSpellbook(this.Owner.Get<SpellManipulationMechanics.UnitPartArcanistPreparedMetamagic>().spellbook).RemoveSpellConversionList(this.SpellsByLevel);
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class IntenseSpellsForClasses : RuleInitiatorLogicComponent<RuleCalculateDamage>
        {
            public BlueprintCharacterClass[] classes;

            public override void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {
                BaseDamage baseDamage = evt.DamageBundle.FirstOrDefault<BaseDamage>();
                AbilityData ability = evt.Reason.Ability;
                if (ability == (AbilityData)null || ability.Blueprint.School != SpellSchool.Evocation || baseDamage == null || evt.ParentRule.Projectile != null && !evt.ParentRule.Projectile.IsFirstProjectile)
                    return;

                var lvl = 0;
                foreach (var c in classes)
                {
                    lvl += this.Owner.Progression.GetClassLevel(c);
                }
                int bonusDamage = Math.Max(lvl / 2, 1);
                baseDamage.AddBonus(bonusDamage);
            }

            public override void OnEventDidTrigger(RuleCalculateDamage evt)
            {
            }
        }



        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ContextValueIntenseSpells : RuleInitiatorLogicComponent<RuleCalculateDamage>
        {
            public ContextValue value;

            public override void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {
                BaseDamage baseDamage = evt.DamageBundle.FirstOrDefault<BaseDamage>();
                AbilityData ability = evt.Reason.Ability;
                if (ability == (AbilityData)null || ability.Blueprint.School != SpellSchool.Evocation || baseDamage == null || evt.ParentRule.Projectile != null && !evt.ParentRule.Projectile.IsFirstProjectile)
                    return;

                int bonusDamage = value.Calculate(this.Fact.MaybeContext);

                baseDamage.AddBonus(bonusDamage);
            }

            public override void OnEventDidTrigger(RuleCalculateDamage evt)
            {
            }
        }


        public class AddClassesLevelToSummonDuration : RuleInitiatorLogicComponent<RuleSummonUnit>
        {
            public BlueprintCharacterClass[] CharacterClasses;
            public bool Half;

            public override void OnEventAboutToTrigger(RuleSummonUnit evt)
            {
                AbilityData ability = evt.Reason.Ability;
                if (((object)ability != null ? ability.Spellbook : (Spellbook)null) == null || ability.Blueprint.School != SpellSchool.Conjuration)
                    return;
                int classLevel = 0;
                foreach (var c in CharacterClasses)
                {
                    classLevel += this.Owner.Progression.GetClassLevel(c);
                }
                
                int num = !this.Half ? classLevel : Math.Max(classLevel / 2, 1);
                evt.BonusDuration += num.Rounds();
            }

            public override void OnEventDidTrigger(RuleSummonUnit evt)
            {
            }
        }


        public class AddContextValueToSummonDuration : RuleInitiatorLogicComponent<RuleSummonUnit>
        {
            public ContextValue value;

            public override void OnEventAboutToTrigger(RuleSummonUnit evt)
            {
                AbilityData ability = evt.Reason.Ability;
                if (((object)ability != null ? ability.Spellbook : (Spellbook)null) == null || ability.Blueprint.School != SpellSchool.Conjuration)
                    return;

                int num = value.Calculate(this.Fact.MaybeContext);
                evt.BonusDuration += num.Rounds();
            }

            public override void OnEventDidTrigger(RuleSummonUnit evt)
            {
            }
        }


        [AllowedOn(typeof(BlueprintBuff))]
        public class MaximumWeaponDamageOnCriticalHit : RuleInitiatorLogicComponent<RuleCalculateDamage>
        {
            public override void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {
                var weapon_damage = evt.DamageBundle?.WeaponDamage;
                if (weapon_damage == null)
                {
                    return;
                }

                if (!(evt.ParentRule?.AttackRoll?.IsCriticalConfirmed).GetValueOrDefault())
                {
                    return;
                }

                weapon_damage.Maximized = true;
            }

            public override void OnEventDidTrigger(RuleCalculateDamage evt)
            {
            }
        }

        [ComponentName("Armor check penalty increase")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ArmorCheckPenaltyIncrease : RuleInitiatorLogicComponent<RuleCalculateArmorCheckPenalty>
        {
            public ContextValue Bonus;
            public int BonesPerRank;
            public bool CheckCategory;
            [ShowIf("CheckCategory")]
            public ArmorProficiencyGroup Category;

            private MechanicsContext Context
            {
                get
                {
                    return this.Fact.MaybeContext;
                }
            }

            public override void OnTurnOn()
            {
                this.Owner.Body.Armor.MaybeArmor?.RecalculateStats();
            }

            public override void OnEventAboutToTrigger(RuleCalculateArmorCheckPenalty evt)
            {
                if (this.CheckCategory && evt.Armor.Blueprint.ProficiencyGroup != this.Category)
                    return;
                evt.AddBonus(this.Bonus.Calculate(this.Context) + this.BonesPerRank * this.Fact.GetRank());
            }

            public override void OnEventDidTrigger(RuleCalculateArmorCheckPenalty evt)
            {
            }
        }


        public class ApplyActionToAllUnits : ContextAction
        {
            public bool apply_to_target;
            public ActionList actions;

            public override string GetCaption()
            {
                return "Apply Action To All Units";
            }

            public override void RunAction()
            {
                var caster = this.Context?.MaybeCaster;

                foreach (var u in Game.Instance.State.Units)
                {
                    if (u == this.Target.Unit && !apply_to_target)
                    {
                        continue;
                    }

                    if (!u.IsEnemy(caster) && !u.IsAlly(caster))
                    {
                        continue;
                    }

                    using (this.Context.GetDataScope((TargetWrapper)u))
                    {
                        this.actions.Run();
                    }
                }
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class AddSpeedBonusBasedOnRaceSize : OwnedGameLogicComponent<UnitDescriptor>
        {
            [JsonProperty]
            private ModifiableValue.Modifier m_Modifier;
            public int small_race_speed_bonus;
            public int normal_race_speed_bonus;
            public ModifierDescriptor descriptor = ModifierDescriptor.Racial;
             


            public override void OnTurnOn()
            {
                var speed = Owner.Stats.Speed;
                var penalty = speed.Racial >= 30 ? normal_race_speed_bonus : small_race_speed_bonus;
                m_Modifier = speed.AddModifier(penalty, this, descriptor);
                base.OnTurnOn();
            }

            public override void OnTurnOff()
            {
                m_Modifier?.Remove();
                m_Modifier = null;
                base.OnTurnOff();
            }
        }


        class SpellLevelPropertyGetter : PropertyValueGetter
        {
            internal static readonly Lazy<BlueprintUnitProperty> Blueprint = new Lazy<BlueprintUnitProperty>(() =>
            {
                var p = Helpers.Create<BlueprintUnitProperty>();
                p.name = "SpellLevelCustomProperty";
                Main.library.AddAsset(p, "a01545ff992d404181e050a119a35a61");
                p.SetComponents(Helpers.Create<SpellLevelPropertyGetter>());
                return p;
            });

            public override int GetInt(UnitEntityData unit)
            {
                return Helpers.GetMechanicsContext()?.SourceAbilityContext?.SpellLevel ?? 0;
            }
        }

        class HighestStatPropertyGetter : PropertyValueGetter
        {
            public StatType[] stats;
            public static BlueprintUnitProperty createProperty(string name, string guid, params StatType[] stats)
            {
                var p = Helpers.Create<BlueprintUnitProperty>();
                p.name = name;
                Main.library.AddAsset(p, guid);
                p.SetComponents(Helpers.Create<HighestStatPropertyGetter>(a => a.stats = stats));
                return p;
            }

            public override int GetInt(UnitEntityData unit)
            {
                int val = -100;
                foreach (var s in stats)
                {
                    int bonus = unit.Stats.GetStat<ModifiableValueAttributeStat>(s).Bonus;
                    if (bonus > val)
                    {
                        val = bonus;
                    }
                }
                return val;
            }
        }


    [ComponentName("change weapon damage")]
    public class WeaponDamageChange : RuleInitiatorLogicComponent<RuleCalculateWeaponStats>, IRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookSubscriber
    {
        public DiceFormula dice_formula;
        public ContextValue bonus_damage;
        public DamageTypeDescription damage_type_description = null;

        public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            evt.WeaponDamageDiceOverride = dice_formula;
            evt.AddBonusDamage(bonus_damage.Calculate(this.Fact.MaybeContext));
        }

        public override void OnEventDidTrigger(RuleCalculateWeaponStats evt)
        {
            if (damage_type_description != null && evt.DamageDescription.Count() > 0)
            {
                evt.DamageDescription[0].TypeDescription = damage_type_description;
            }
        }
    }


        class SneakAttackDiceGetter : PropertyValueGetter
        {
            internal static readonly Lazy<BlueprintUnitProperty> Blueprint = new Lazy<BlueprintUnitProperty>(() =>
            {
                var p = Helpers.Create<BlueprintUnitProperty>();
                p.name = "SneakAttackDiceCustomProperty";
                Main.library.AddAsset(p, "a9d8d3c40dab4e8e8d92112cea65dc65");
                p.SetComponents(Helpers.Create<SneakAttackDiceGetter>());
                return p;
            });

            public override int GetInt(UnitEntityData unit)
            {
                return unit.Stats.SneakAttack.ModifiedValue;
            }
        }



        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilityTargetWounded : BlueprintComponent, IAbilityTargetChecker
        {
            public int CurrentHPLessThan;
            public bool Inverted;

            public bool CanTarget(UnitEntityData caster, TargetWrapper target)
            {
                if (target.Unit == null)
                    return false;
                return this.Inverted != (target.Unit.Damage > 0);
            }
        }


        public class ImmuneToAttackOfOpportunityForSpells : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            public SpellDescriptorWrapper Descriptor;

            public static ImmuneToAttackOfOpportunityForSpells Create(SpellDescriptor descriptor = SpellDescriptor.None)
            {
                var i = Helpers.Create<ImmuneToAttackOfOpportunityForSpells>();
                i.Descriptor = descriptor;
                return i;
            }

            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                if (evt.Spell.Type == AbilityType.Spell &&
                    (Descriptor == SpellDescriptor.None || (evt.Spell.SpellDescriptor & Descriptor) != 0))
                {
                    evt.AddBonusConcentration(1000);
                }
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt) { }
        }



        [AllowMultipleComponents]
        public class AddInitiatorAttackRollTrigger2 : GameLogicComponent, IInitiatorRulebookHandler<RuleAttackRoll>, IRulebookHandler<RuleAttackRoll>, IInitiatorRulebookSubscriber
        {
            [HideIf("CriticalHit")]
            public bool OnlyHit = true;
            public bool CriticalHit;
            public bool only_natural20;
            public bool SneakAttack;
            public bool OnOwner;
            public bool CheckWeapon;
            [ShowIf("CheckWeapon")]
            public WeaponCategory WeaponCategory;
            public bool CheckWeaponRangeType;
            [ShowIf("CheckWeaponRangeType")]
            public AttackTypeAttackBonus.WeaponRangeType RangeType;
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
                if (weapon == null)
                {
                    weapon = evt.Weapon;
                }

                if (weapon == null)
                {
                    return false;
                }

                if (this.CheckWeaponRangeType && !AttackTypeAttackBonus.CheckRangeType(evt.Weapon.Blueprint, this.RangeType))
                {
                    return false;
                }

                if (only_natural20 && evt.Roll != 20)
                {
                    return false;
                }

                return (owner == null || owner == weapon) && (!this.CheckWeapon || weapon != null && this.WeaponCategory == weapon.Blueprint.Category) && ((!this.OnlyHit || evt.IsHit) && (!this.CriticalHit || evt.IsCriticalConfirmed && !evt.FortificationNegatesCriticalHit)) && ((!this.SneakAttack || evt.IsSneakAttack && !evt.FortificationNegatesSneakAttack) && (this.AffectFriendlyTouchSpells || evt.Initiator.IsEnemy(evt.Target) || evt.Weapon.Blueprint.Type.AttackType != AttackType.Touch));
            }
        }



        [AllowedOn(typeof(BlueprintBuff))]
        public class DamageBonusPrecisionAgainstFactOwner : RuleInitiatorLogicComponent<RuleCalculateDamage>
        {
            public BlueprintUnitFact checked_fact;
            public ContextDiceValue bonus;
            public AttackType[] attack_types;
            public bool only_from_caster = false;
            public BlueprintFeature attacker_fact = null;
            public bool remove_after_damage;

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

            public override void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {
                if (attacker_fact != null && !evt.Initiator.Descriptor.HasFact(attacker_fact))
                {
                    return;
                }
                if (evt.DamageBundle.Empty())
                {
                    return;
                }
                var attack_roll = evt.ParentRule?.AttackRoll;
                if (attack_roll == null)
                {
                    return;
                }
                if (!attack_types.Contains(attack_roll.AttackType))
                {
                    return;
                }

                bool is_ok = false;

                if (!only_from_caster)
                {
                    is_ok = evt.Target.Descriptor.HasFact(checked_fact);
                }
                else
                {
                    var caster = this.Context.MaybeCaster;
                    is_ok = evt.Target.Buffs.Enumerable.Any(b => b.Blueprint == checked_fact && b.MaybeContext.MaybeCaster == caster);
                }

                if (!is_ok)
                {
                    return;
                }

                var dice_fomula = new DiceFormula(bonus.DiceCountValue.Calculate(Context), bonus.DiceType);

                BaseDamage new_damage = evt.DamageBundle.First.CreateTypeDescription().CreateDamage(dice_fomula, bonus.BonusValue.Calculate(Context));
                new_damage.Precision = true;
                new_damage.CriticalModifier = new int?();
                evt.DamageBundle.Add(new_damage);

                if (remove_after_damage)
                {
                    (this.Fact as Buff).Remove();
                }
            }

            public override void OnEventDidTrigger(RuleCalculateDamage evt)
            {

            }
        }


        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class DamageBonusAgainstAnyFactsOwner : RuleInitiatorLogicComponent<RuleAttackWithWeapon>
        {
            public BlueprintUnitFact[] facts;
            public int DamageBonus;
            public ContextValue Bonus;
            public ModifierDescriptor Descriptor;

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

            public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
            {
                if (evt.Weapon == null)
                    return;

                foreach (var f in facts)
                {
                    if (evt.Target.Descriptor.HasFact(f))
                    {
                        evt.AddTemporaryModifier(evt.Initiator.Stats.AdditionalDamage.AddModifier(this.DamageBonus * this.Fact.GetRank() + this.Bonus.Calculate(this.Context), (GameLogicComponent)this, this.Descriptor));
                        return;
                    }
                }
            }

            public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
            }
        }


        [AllowedOn(typeof(BlueprintBuff))]
        public class ActioOnCalculateDamageAfterAttackRoll : RuleInitiatorLogicComponent<RuleCalculateDamage>
        {
            public ActionList action;
            public bool OnOwner;
            public bool CriticalHit;
            public bool SneakAttack;
            public bool CheckWeapon;
            [ShowIf("CheckWeapon")]
            public WeaponCategory WeaponCategory;
            public bool CheckWeaponRangeType;
            [ShowIf("CheckWeaponRangeType")]
            public AttackTypeAttackBonus.WeaponRangeType RangeType;
            public bool AffectFriendlyTouchSpells;

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

            public override void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {

            }

            public override void OnEventDidTrigger(RuleCalculateDamage evt)
            {
                var attack_roll = evt.ParentRule?.AttackRoll;
                if (attack_roll == null)
                {
                    return;
                }

                if (!CheckConditions(attack_roll))
                {
                    return;
                }

                using (new ContextAttackData(attack_roll, (Projectile)null))
                {
                    if (this.OnOwner)
                    {
                        UnitDescriptor unitDescriptor = (this.Fact as OwnedFact<UnitDescriptor>)?.Owner ?? (this.Fact as ItemEnchantment)?.Owner.Wielder;
                        if (unitDescriptor != null)
                            (this.Fact as IFactContextOwner)?.RunActionInContext(this.action, (TargetWrapper)unitDescriptor.Unit);
                        else
                            UberDebug.LogError((object)string.Format("Fact has no owner: {0}", (object)this.Fact), (object[])Array.Empty<object>());
                    }
                    else
                        (this.Fact as IFactContextOwner)?.RunActionInContext(this.action, (TargetWrapper)evt.Target);
                }
            }


            private bool CheckConditions(RuleAttackRoll evt)
            {
                ItemEntity owner = (this.Fact as ItemEnchantment)?.Owner;
                ItemEntityWeapon weapon = (evt.Reason.Rule as RuleAttackWithWeapon)?.Weapon;
                if (weapon == null)
                {
                    weapon = evt.Weapon;
                }

                
                if (weapon == null)
                {
                    return false;
                }

                if (this.CheckWeaponRangeType && !AttackTypeAttackBonus.CheckRangeType(evt.Weapon.Blueprint, this.RangeType))
                {
                    return false;
                }

                return (owner == null || owner == weapon) && (!this.CheckWeapon || weapon != null && this.WeaponCategory == weapon.Blueprint.Category) && ((!this.CriticalHit || evt.IsCriticalConfirmed && !evt.FortificationNegatesCriticalHit)) && ((!this.SneakAttack || evt.IsSneakAttack && !evt.FortificationNegatesSneakAttack));
            }
        }


        public class CasterLevelChecksBonusForSpecifiedSpells : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>, IInitiatorRulebookHandler<RuleSpellResistanceCheck>
        {
            public BlueprintAbility[] spells;
            public ContextValue value;

            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                if (!spells.Contains(evt.Spell))
                {
                    return;
                }

                var bonus = value.Calculate(this.Fact.MaybeContext);
                evt.AddBonusConcentration(bonus);
            }

            public void OnEventAboutToTrigger(RuleSpellResistanceCheck evt)
            {
                if (!spells.Contains(evt.Ability))
                {
                    return;
                }

                var bonus = value.Calculate(this.Fact.MaybeContext);
                evt.AdditionalSpellPenetration += bonus;
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt) { }

            public void OnEventDidTrigger(RuleSpellResistanceCheck evt)
            {

            }
        }


        [AllowMultipleComponents]
        public class PrerequisiteMinimumFeatureRank : Prerequisite
        {
            [NotNull]
            public BlueprintFeature Feature;
            public bool not;
            public int value;


            public override bool Check(
              FeatureSelectionState selectionState,
              UnitDescriptor unit,
              LevelUpState state)
            {
                var feat = unit.Progression.Features.GetFact(this.Feature);

                if (feat == null)
                {
                    return not;
                }
                else
                {
                    return (feat.GetRank() >= value) != not;
                }
            }

            public override string GetUIText()
            {
                StringBuilder stringBuilder = new StringBuilder();
                if ((UnityEngine.Object)this.Feature == (UnityEngine.Object)null)
                {
                    UberDebug.LogError((object)("Empty Feature fild in prerequisite component: " + this.name), (object[])Array.Empty<object>());
                }
                else
                {
                    if (string.IsNullOrEmpty(this.Feature.Name))
                        UberDebug.LogError((object)string.Format("{0} has no Display Name", (object)this.Feature.name), (object[])Array.Empty<object>());
                    stringBuilder.Append(this.Feature.Name);
                }

                if (not)
                {
                    return $"{stringBuilder.ToString()} rank less than: {value}";
                }
                else
                {
                    return $"{stringBuilder.ToString()} rank: {value}";
                }
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        public class AbilityShowIfHasFeatureRank : BlueprintComponent, IAbilityVisibilityProvider
        {
            public int min_value;
            public int max_value = 1000;
            public BlueprintFeature Feature;

            public bool IsAbilityVisible(AbilityData ability)
            {
                var feat = ability.Caster.Progression.Features.GetFact(this.Feature);

                if (feat == null)
                {
                    return false;
                }
                else
                {
                    return (feat.GetRank() >= min_value) && (feat.GetRank() <= max_value);
                }
            }
        }

        [AllowMultipleComponents]
        public class PrerequisiteFeatureFullRank : Prerequisite
        {
            [NotNull]
            public BlueprintFeature Feature;
            public BlueprintFeature checked_feature;
            public int divisor;
            public bool not;


            public override bool Check(
              FeatureSelectionState selectionState,
              UnitDescriptor unit,
              LevelUpState state)
            {
                if (selectionState != null && selectionState.IsSelectedInChildren(this.Feature))
                    return false;
                if (selectionState != null && checked_feature != null && selectionState.IsSelectedInChildren(this.checked_feature))
                    return false;
                var feat = unit.Progression.Features.GetFact(this.Feature);
                var checked_feat = unit.Progression.Features.GetFact(this.checked_feature);

                int rank = (feat == null ? 0 : feat.GetRank()) + ((checked_feat == null || checked_feature == Feature) ? 0 : checked_feat.GetRank());

                return (((rank + 1) % divisor) == 0) != not;
            }

            public override string GetUIText()
            {
                StringBuilder stringBuilder = new StringBuilder();
                if ((UnityEngine.Object)this.Feature == (UnityEngine.Object)null)
                {
                    UberDebug.LogError((object)("Empty Feature field in prerequisite component: " + this.name), (object[])Array.Empty<object>());
                }
                else
                {
                    if (string.IsNullOrEmpty(this.Feature.Name))
                        UberDebug.LogError((object)string.Format("{0} has no Display Name", (object)this.Feature.name), (object[])Array.Empty<object>());
                    stringBuilder.Append(this.Feature.Name);
                }

                if (not)
                {
                    return $"Less than {divisor - 1} rank(s) of {stringBuilder.ToString()}";
                }
                else
                {
                    return $"{divisor - 1} rank(s) of {stringBuilder.ToString()}";
                }
            }
        }




        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowedOn(typeof(BlueprintBuff))]
        [AllowMultipleComponents]
        public class AddBonusToSkillCheckIfNoClassSkill : RuleInitiatorLogicComponent<RuleSkillCheck>
        {
            public StatType check;
            public StatType skill;

            public override void OnEventAboutToTrigger(RuleSkillCheck evt)
            {
                if (evt.StatType != check)
                {
                    return;
                }

                if ((bool)evt.Initiator.Stats.GetStat<ModifiableValueSkill>(skill).ClassSkill)
                {
                    return;
                }
                evt.Bonus.AddModifier(3, this, ModifierDescriptor.Trait);
            }

            public override void OnEventDidTrigger(RuleSkillCheck evt)
            {

            }
        }


        [ComponentName("AddRandomBonusOnSkillCheck")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowedOn(typeof(BlueprintBuff))]
        [AllowMultipleComponents]
        public class AddRandomBonusOnSkillCheckAndConsumeResource : RuleInitiatorLogicComponent<RuleSkillCheck>
        {
            public ContextValue dice_count;
            public ContextValue dice_type;
            public DiceType[] dices;
            public BlueprintAbilityResource resource;
            public int amount;
            public BlueprintUnitFact[] cost_reducing_facts;
            public BlueprintUnitFact allow_reroll_fact;
            public StatType[] stats;
            private int will_spend = 0;

            private int getResourceAmount(RuleSkillCheck evt)
            {
                int reduction = 0;
                foreach (var f in cost_reducing_facts)
                {
                    if (evt.Initiator.Descriptor.HasFact(f))
                    {
                        reduction++;
                    }
                }

                int need_resource = amount - reduction;
                return need_resource > 0 ? need_resource : 0;
            }

            public override void OnEventAboutToTrigger(RuleSkillCheck evt)
            {
                will_spend = 0;
                if (dices.Empty())
                {
                    return;
                }
                if (!stats.Contains(evt.StatType))
                {
                    return;
                }
                if (resource != null)
                {
                    int need_resource = getResourceAmount(evt);

                    if (evt.Initiator.Descriptor.Resources.GetResourceAmount(resource) < need_resource)
                    {
                        return;
                    }
                    will_spend = need_resource;
                }

                var dice_id = dice_type.Calculate(this.Fact.MaybeContext) - 1;
                dice_id = Math.Max(0, Math.Min(dices.Length - 1, dice_id));
                DiceFormula dice_formula = new DiceFormula(dice_count.Calculate(this.Fact.MaybeContext), dices[dice_id]);

                RuleRollDice rule = new RuleRollDice(evt.Initiator, dice_formula);
                int result = this.Fact.MaybeContext.TriggerRule<RuleRollDice>(rule).Result;
                if (allow_reroll_fact != null && evt.Initiator.Descriptor.HasFact(allow_reroll_fact))
                {
                    result = Math.Max(result, this.Fact.MaybeContext.TriggerRule<RuleRollDice>(new RuleRollDice(evt.Initiator, dice_formula)).Result);
                }

                evt.Bonus.AddModifier(result, null, ModifierDescriptor.UntypedStackable);
            }

            public override void OnEventDidTrigger(RuleSkillCheck evt)
            {
                if (will_spend > 0)
                {
                    evt.Initiator.Descriptor.Resources.Spend(resource, will_spend);
                }
                will_spend = 0;
            }
        }


        [ComponentName("Context Max Dex bonus increase")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ContextMaxDexBonusIncrease : RuleInitiatorLogicComponent<RuleCalculateArmorMaxDexBonusLimit>
        {
            public ContextValue bonus;
            public bool check_category;
            [ShowIf("CheckCategory")]
            public ArmorProficiencyGroup[] category;

            public override void OnTurnOn()
            {
                this.Owner.Body.Armor.MaybeArmor?.RecalculateStats();
            }

            public override void OnEventAboutToTrigger(RuleCalculateArmorMaxDexBonusLimit evt)
            {
                if (!category.Empty() && !category.Contains(evt.Armor.Blueprint.ProficiencyGroup))
                    return;
                evt.AddBonus(this.bonus.Calculate(this.Fact.MaybeContext));
            }

            public override void OnEventDidTrigger(RuleCalculateArmorMaxDexBonusLimit evt)
            {
            }
        }


        [ComponentName("AddRandomBonusOnAttackRoll")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowedOn(typeof(BlueprintBuff))]
        [AllowMultipleComponents]
        public class AddRandomBonusOnAttackRollAndConsumeResource : RuleInitiatorLogicComponent<RuleAttackRoll>
        {
            public ContextValue dice_count;
            public ContextValue dice_type;
            public DiceType[] dices;
            public BlueprintAbilityResource resource;
            public int amount;
            public BlueprintUnitFact[] cost_reducing_facts;
            public BlueprintUnitFact allow_reroll_fact;
            public BlueprintParametrizedFeature parametrized_feature;
            private int will_spend = 0;

            private int getResourceAmount(RuleAttackRoll evt)
            {
                int reduction = 0;
                foreach (var f in cost_reducing_facts)
                {
                    if (evt.Initiator.Descriptor.HasFact(f))
                    {
                        reduction++;
                    }
                }

                if (evt.Weapon != null
                    && this.Owner.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => p.Blueprint == parametrized_feature).Any(p => p.Param == evt.Weapon.Blueprint.Category))
                {
                    reduction++;
                }

                int need_resource = amount - reduction;
                return need_resource > 0 ? need_resource : 0;
            }

            public override void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
                will_spend = 0;

                if (resource != null)
                {
                    int need_resource = getResourceAmount(evt);

                    if (evt.Initiator.Descriptor.Resources.GetResourceAmount(resource) < need_resource)
                    {
                        return;
                    }
                    will_spend = need_resource;
                }

                var dice_id = dice_type.Calculate(this.Fact.MaybeContext) - 1;
                dice_id = Math.Max(0, Math.Min(dices.Length - 1, dice_id));
                DiceFormula dice_formula = new DiceFormula(dice_count.Calculate(this.Fact.MaybeContext), dices[dice_id]);

                RuleRollDice rule = new RuleRollDice(evt.Initiator, dice_formula);
                int result = this.Fact.MaybeContext.TriggerRule<RuleRollDice>(rule).Result;
                if (allow_reroll_fact != null && evt.Initiator.Descriptor.HasFact(allow_reroll_fact))
                {
                    result = Math.Max(result, this.Fact.MaybeContext.TriggerRule<RuleRollDice>(new RuleRollDice(evt.Initiator, dice_formula)).Result);
                }

                evt.AddTemporaryModifier(evt.Initiator.Stats.AdditionalAttackBonus.AddModifier(result, this, ModifierDescriptor.UntypedStackable));
            }

            public override void OnEventDidTrigger(RuleAttackRoll evt)
            {
                if (will_spend > 0)
                {
                    evt.Initiator.Descriptor.Resources.Spend(resource, will_spend);
                }
                will_spend = 0;
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class IncreaseSpellDCForBlueprints : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            public BlueprintScriptableObject[] blueprints;
            public ContextValue value;

            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                if (blueprints.Contains(evt.Blueprint))
                    evt.AddBonusDC(this.value.Calculate(this.Fact.MaybeContext));
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }


        [ComponentName("Increase spell descriptor DC")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class SavingthrowBonusAgainstCasterAbilities : RuleInitiatorLogicComponent<RuleSavingThrow>
        {
            public BlueprintScriptableObject[] sources;
            public ContextValue value;
            public int multiplier;
            public ModifierDescriptor descriptor;

            public override void OnEventAboutToTrigger(RuleSavingThrow evt)
            {
                if (evt.Reason.Caster != this.Fact.MaybeContext.MaybeCaster)
                {
                    return;
                }

                var source = evt.Reason.Context?.AssociatedBlueprint;
                if (source == null)
                {
                    return;
                }

                if (!sources.Contains(source))
                {
                    return;
                }

                var bonus = this.value.Calculate(this.Fact.MaybeContext) * this.multiplier;
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveWill.AddModifier(bonus, (GameLogicComponent)this, this.descriptor));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveReflex.AddModifier(bonus, (GameLogicComponent)this, this.descriptor));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveFortitude.AddModifier(bonus, (GameLogicComponent)this, this.descriptor));
            }

            public override void OnEventDidTrigger(RuleSavingThrow evt)
            {
            }
        }


        [ComponentName("AddRandomBonusOnSavingthrowsRoll")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowedOn(typeof(BlueprintBuff))]
        [AllowMultipleComponents]
        public class AddRandomBonusOnSavingThrowAndConsumeResource : RuleInitiatorLogicComponent<RuleSavingThrow>
        {
            public ContextValue dice_count;
            public ContextValue dice_type;
            public DiceType[] dices;
            public BlueprintAbilityResource resource;
            public int amount;
            public BlueprintUnitFact[] cost_reducing_facts;
            public BlueprintUnitFact allow_reroll_fact;
            private int will_spend = 0;

            private int getResourceAmount(RuleSavingThrow evt)
            {
                int reduction = 0;
                foreach (var f in cost_reducing_facts)
                {
                    if (evt.Initiator.Descriptor.HasFact(f))
                    {
                        reduction++;
                    }
                }

                int need_resource = amount - reduction;
                return need_resource > 0 ? need_resource : 0;
            }


            public override void OnEventAboutToTrigger(RuleSavingThrow evt)
            {
                will_spend = 0;

                if (resource != null)
                {
                    int need_resource = getResourceAmount(evt);

                    if (evt.Initiator.Descriptor.Resources.GetResourceAmount(resource) < need_resource)
                    {
                        return;
                    }
                    will_spend = need_resource;
                }

                var dice_id = dice_type.Calculate(this.Fact.MaybeContext) - 1;
                dice_id = Math.Max(0, Math.Min(dices.Length - 1, dice_id));
                DiceFormula dice_formula = new DiceFormula(dice_count.Calculate(this.Fact.MaybeContext), dices[dice_id]);
                RuleRollDice rule = new RuleRollDice(evt.Initiator, dice_formula);
                int result = this.Fact.MaybeContext.TriggerRule<RuleRollDice>(rule).Result;
                if (allow_reroll_fact != null && evt.Initiator.Descriptor.HasFact(allow_reroll_fact))
                {
                    result = Math.Max(result, this.Fact.MaybeContext.TriggerRule<RuleRollDice>(new RuleRollDice(evt.Initiator, dice_formula)).Result);
                }

                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveWill.AddModifier(result, this, ModifierDescriptor.UntypedStackable));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveFortitude.AddModifier(result, this, ModifierDescriptor.UntypedStackable));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveReflex.AddModifier(result, this, ModifierDescriptor.UntypedStackable));
            }


            public override void OnEventDidTrigger(RuleSavingThrow evt)
            {
                if (will_spend > 0)
                {
                    evt.Initiator.Descriptor.Resources.Spend(resource, will_spend);
                }
                will_spend = 0;
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class SpecificSavingThrowBonusAgainstSchool : RuleInitiatorLogicComponent<RuleSavingThrow>
        {
            public SavingThrowType type;
            public SpellSchool School;
            public ModifierDescriptor ModifierDescriptor;
            public ContextValue Value;

            public override void OnEventAboutToTrigger(RuleSavingThrow evt)
            {
                if (evt.Reason.Context == null)
                    return;
                SpellSchool? school = evt.Reason.Context?.SourceAbility?.School;
                if ((school.GetValueOrDefault() != this.School ? 0 : (school.HasValue ? 1 : 0)) == 0)
                    return;
                if (evt.Type != type)
                {
                    return;
                }
                int bonus = this.Value.Calculate(this.Fact.MaybeContext);
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveWill.AddModifier(bonus, (GameLogicComponent)this, this.ModifierDescriptor));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveReflex.AddModifier(bonus, (GameLogicComponent)this, this.ModifierDescriptor));
                evt.AddTemporaryModifier(evt.Initiator.Stats.SaveFortitude.AddModifier(bonus, (GameLogicComponent)this, this.ModifierDescriptor));
            }

            public override void OnEventDidTrigger(RuleSavingThrow evt)
            {
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class BindAbilitiesToClassFixedLevel : BindAbilitiesToClass
        {
            public int fixed_level = 1;
            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                base.OnEventAboutToTrigger(evt);
                if (!((IEnumerable<BlueprintAbility>)this.Abilites).Contains<BlueprintAbility>(evt.Spell))
                    return;

                evt.ReplaceSpellLevel = new int?(fixed_level);
            }
        }


        [ComponentName("Weapon parameters attack bonus")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class WeaponsOnlyAttackBonus : RuleInitiatorLogicComponent<RuleCalculateAttackBonusWithoutTarget>
        {
            public ContextValue value;

            public override void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
            {
                if (evt.Weapon == null)
                    return;
               if (evt.Weapon.Blueprint.IsNatural || evt.Weapon.Blueprint.IsUnarmed)
               {
                    return;
               }

               if (evt.Weapon.Blueprint.Category == WeaponCategory.Ray || evt.Weapon.Blueprint.Category == WeaponCategory.Touch)
               {
                   return;
               }
                evt.AddBonus(value.Calculate(this.Fact.MaybeContext), this.Fact);
            }

            public override void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt)
            {
            }
        }


        public class IncreaseActivatableAbilityGroupSizeForPet : OwnedGameLogicComponent<UnitDescriptor>
        {
            public ActivatableAbilityGroup Group;
            public int amount = 1;

            public override void OnTurnOn()
            {
                for (int i = 0; i < amount; i++)
                {
                    this.Owner.Pet?.Ensure<UnitPartActivatableAbility>().IncreaseGroupSize(this.Group);
                }
            }

            public override void OnTurnOff()
            {
                for (int i = 0; i < amount; i++)
                {
                    this.Owner.Pet?.Ensure<UnitPartActivatableAbility>().DecreaseGroupSize(this.Group);
                }
            }
        }



        [AllowedOn(typeof(BlueprintAbility))]
        public class AbilityTargetIsDead : BlueprintComponent, IAbilityTargetChecker
        {
            public bool CanTarget(UnitEntityData caster, TargetWrapper t)
            {
                UnitEntityData unit = t.Unit;
                if (unit != null && (unit.Descriptor.State.IsDead || unit.Descriptor.State.HasCondition(UnitCondition.DeathDoor)))
                    return !unit.Descriptor.State.HasCondition(UnitCondition.Petrified);
                return false;
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class SpellPenetrationBonusAgainstFactAndAlignment : RuleInitiatorLogicComponent<RuleSpellResistanceCheck>
        {
            public ContextValue value;
            public BlueprintUnitFact fact;
            public Alignment alignment;

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

            public override void OnEventAboutToTrigger(RuleSpellResistanceCheck evt)
            {
                if (evt.Target.Descriptor.Alignment.Value != this.alignment || !evt.Target.Descriptor.HasFact(fact))
                    return;
                int num = this.value.Calculate(this.Context);
                evt.AdditionalSpellPenetration += num;
            }

            public override void OnEventDidTrigger(RuleSpellResistanceCheck evt)
            {
            }
        }

        [ComponentName("Attack bonus against fact owner")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class AttackBonusAgainstFactAndAlignment : RuleInitiatorLogicComponent<RuleAttackRoll>
        {
            public BlueprintUnitFact CheckedFact;
            public Alignment alignment;
            public int AttackBonus;
            public ContextValue Bonus;
            public ModifierDescriptor Descriptor;

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
                if (evt.Weapon == null || !evt.Target.Descriptor.HasFact(this.CheckedFact) || evt.Target.Descriptor.Alignment.Value != this.alignment)
                    return;
                evt.AddTemporaryModifier(evt.Initiator.Stats.AdditionalAttackBonus.AddModifier(this.AttackBonus * this.Fact.GetRank() + this.Bonus.Calculate(this.Context), (GameLogicComponent)this, this.Descriptor));
            }

            public override void OnEventDidTrigger(RuleAttackRoll evt)
            {
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class AttackBonusIfAloneAgainstBiggerSize : RuleInitiatorLogicComponent<RuleAttackRoll>
        {
            public ContextValue Bonus;
            public ModifierDescriptor Descriptor;
            public bool only_melee;
            public bool only_non_reach;
            public bool only_alone;
            public bool only_if_smaller;

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
                if (evt.Weapon == null)
                    return;

                if (!evt.Weapon.Blueprint.IsMelee &&  only_melee)
                {
                    return;
                }
                if (evt.Weapon.Blueprint.Type.AttackRange > GameConsts.MinWeaponRange && only_non_reach)
                {
                    return;
                }

                if (evt.Target.Descriptor.State.Size <= evt.Initiator.Descriptor.State.Size && only_if_smaller)
                {
                    return;
                }

                if (only_alone)
                {
                    var units_around = GameHelper.GetTargetsAround(this.Owner.Unit.Position, 5.Feet().Meters, true, false);
                    foreach (var u in units_around)
                    {
                        if (u == evt.Initiator)
                        {
                            continue;
                        }
                        if (u.IsAlly(evt.Initiator))
                        {
                            return;
                        }
                    }
                }

                evt.AddTemporaryModifier(evt.Initiator.Stats.AdditionalAttackBonus.AddModifier(this.Bonus.Calculate(this.Context), (GameLogicComponent)this, this.Descriptor));
            }

            public override void OnEventDidTrigger(RuleAttackRoll evt)
            {
            }
        }




        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class WeaponCategorySizeChange : RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
        {
            public WeaponCategory category;
            public int SizeCategoryChange = 1;

            public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
            {
                if (evt.Weapon.Blueprint.Category != category || this.SizeCategoryChange == 0)
                    return;
                if (this.SizeCategoryChange > 0)
                    evt.IncreaseWeaponSize();
                else
                    evt.DecreaseWeaponSize();
            }

            public override void OnEventDidTrigger(RuleCalculateWeaponStats evt)
            {
            }
        }

        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintFeature))]
        public class ActivateUnitPartMagus : OwnedGameLogicComponent<UnitDescriptor>
        {
            public BlueprintCharacterClass magus_class;

            public override void OnFactActivate()
            {
                UnitPartMagus unitPartMagus = this.Owner.Ensure<UnitPartMagus>();
                unitPartMagus.Class = magus_class;
            }

            public override void OnFactDeactivate()
            {
 
            }
        }



            [ComponentName("Override owner's empty hand weapon")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class EmptyHandWeaponOverrideIfNoWeapon : OwnedGameLogicComponent<UnitDescriptor>
        {
            static public BlueprintItemWeapon empty_hand = Main.library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            public BlueprintItemWeapon Weapon;
            private ItemEntityWeapon m_Weapon;

            public override void OnTurnOn()
            {
                base.OnTurnOn();
                if (this.Owner.Body.EmptyHandWeapon?.Blueprint == empty_hand)
                {
                    this.m_Weapon = this.Owner.Body.SetEmptyHandWeapon(this.Weapon);
                }
                else
                {
                    m_Weapon = null;                  
                }
            }

            public override void OnTurnOff()
            {
                base.OnTurnOff();
                if (m_Weapon != null)
                {
                    this.Owner.Body.RemoveEmptyHandWeapon(this.m_Weapon);
                }
            }
        }


        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ConsiderAsFinessable : OwnedGameLogicComponent<UnitDescriptor>
        {
            public WeaponCategory category;

            public override void OnTurnOn()
            {
                // Using DamageGracePart should ensure this works correctly with other
                // features that work with finessable wepaons.
                // (e.g. this is how Weapon Finesse picks it up.) 
                Owner.Ensure<DamageGracePart>().AddEntry(category, Fact);
            }

            public override void OnTurnOff()
            {
                Owner.Ensure<DamageGracePart>().RemoveEntry(Fact);
            }
        }


        [AllowMultipleComponents]
        [ComponentName("Replace damage stat for weapon")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class DamageGraceForWeapon : RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
        {
            public WeaponCategory category;

            public override void OnTurnOn()
            {
                // Using DamageGracePart should ensure this works correctly with other
                // features that work with finessable wepaons.
                // (e.g. this is how Weapon Finesse picks it up.) 
                Owner.Ensure<DamageGracePart>().AddEntry(category, Fact);
            }

            public override void OnTurnOff()
            {
                Owner.Ensure<DamageGracePart>().RemoveEntry(Fact);
            }

            public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
            {
                if (evt.Weapon.Blueprint.Type.Category == category)
                {
                    var offHand = evt.Initiator.Body.SecondaryHand;
                    if (HoldingItemsMechanics.Helpers.hasFreeHand(offHand))
                    {
                        var dexterity = evt.Initiator.Descriptor.Stats.Dexterity;
                        var existingStat = !evt.DamageBonusStat.HasValue ? null : (Owner.Unit.Descriptor.Stats.GetStat(evt.DamageBonusStat.Value) as ModifiableValueAttributeStat);
                        if (dexterity != null && (existingStat == null || dexterity.Bonus > existingStat.Bonus))
                        {
                            evt.OverrideDamageBonusStat(StatType.Dexterity);
                        }
                    }
                }
            }

            public override void OnEventDidTrigger(RuleCalculateWeaponStats evt) { }
        }


        public class HasUnitsInSummonPool : ContextCondition
        {
            public BlueprintSummonPool SummonPool;

            protected override string GetConditionCaption()
            {
                return "";
            }

            protected override bool CheckCondition()
            {
                return GameHelper.GetSummonPool(this.SummonPool).Count > 0;
            }
        }

        public class HasUnitsInSummonPoolFromCaster : ContextCondition
        {
            public BlueprintSummonPool SummonPool;

            protected override string GetConditionCaption()
            {
                return "";
            }

            protected override bool CheckCondition()
            {
                ISummonPool pool = Game.Instance.SummonPools.GetPool(this.SummonPool);
                if (pool == null)
                    return false;
                foreach (UnitEntityData unit in pool.Units)
                {
                    if (unit.Ensure<UnitPartSummonedMonster>().Summoner == this.Context.MaybeCaster)
                    {
                        return true;
                    }
                }
                return false;
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ReduceDRFromCaster : RuleTargetLogicComponent<RuleCalculateDamage>
        {
            public int reduction_reduction;

            public override void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {
                if (evt.DamageBundle.Weapon == null || evt.DamageBundle.WeaponDamage == null || evt.Target != this.Owner.Unit)
                    return;
                
                if (this.Fact.MaybeContext?.MaybeCaster != evt.Initiator)
                {
                    return;
                }
                evt.DamageBundle.WeaponDamage.SetReductionPenalty(this.reduction_reduction);
            }

            public override void OnEventDidTrigger(RuleCalculateDamage evt)
            {
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class CritAutoconfirmIfHasNoFact : RuleInitiatorLogicComponent<RuleAttackRoll>
        {
            public BlueprintUnitFact fact;
            public override void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
                if (this.Owner.HasFact(fact))
                {
                    return;
                }
                evt.AutoCriticalConfirmation = true;
            }

            public override void OnEventDidTrigger(RuleAttackRoll evt)
            {
            }
        }

        public class ContextActionRemoveBuffs : ContextAction
        {
            public BlueprintBuff[] Buffs;
            public bool ToCaster;

            public override string GetCaption()
            {
                return "Remove Buffs";
            }

            public override void RunAction()
            {
                MechanicsContext context = ElementsContext.GetData<MechanicsContext.Data>()?.Context;
                if (context == null)
                    UberDebug.LogError((UnityEngine.Object)this, (object)"Unable to remove buff: no context found", (object[])Array.Empty<object>());
                else
                {
                    foreach (var b in Buffs)
                    {
                        (!this.ToCaster ? this.Target.Unit : context.MaybeCaster).Buffs.RemoveFact(b);
                    }
                }
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        public class AbilityShowIfCasterHasNoFact : BlueprintComponent, IAbilityVisibilityProvider
        {
            public BlueprintUnitFact UnitFact;

            public bool IsAbilityVisible(AbilityData ability)
            {
                return !ability.Caster.Progression.Features.HasFact((BlueprintFact)this.UnitFact);
            }
        }

        [ComponentName("Weapon group attack bonus")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class WeaponTrainingIfHasParametrizedFeatures : RuleInitiatorLogicComponent<RuleCalculateWeaponStats>, IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>
        {
            public BlueprintParametrizedFeature[] required_parametrized_features;

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

            public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
            {

            }

            public void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
            {
                if (evt.Weapon == null || !checkFeature(evt.Weapon.Blueprint.Category))
                    return;
                evt.AddBonus(this.Fact.GetRank(), this.Fact);
            }

            public void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt)
            {
            }

            public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
            {
                if (evt.Weapon == null || !checkFeature(evt.Weapon.Blueprint.Category))
                    return;
                evt.AddTemporaryModifier(evt.Initiator.Stats.AdditionalDamage.AddModifier(this.Fact.GetRank(), (GameLogicComponent)this, ModifierDescriptor.UntypedStackable));
            }

            public override void OnEventDidTrigger(RuleCalculateWeaponStats evt)
            {
            }
        }



        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class SecondRollToRemoveBuffAfterOneRound : RuleInitiatorLogicComponent<RuleApplyBuff>
        {
            public SpellDescriptorWrapper spell_descriptor;
            public SpellSchool school = SpellSchool.None;
            public SavingThrowType save_type;

            private RuleSavingThrow last_saving_throw = null;
            private bool passed = false;
            public override void OnEventAboutToTrigger(RuleApplyBuff evt)
            {
                if (evt.Context == null 
                    || (spell_descriptor != SpellDescriptor.None && !evt.Context.SpellDescriptor.Intersects(spell_descriptor))
                    || (school != SpellSchool.None && evt.Context.SpellSchool != school)
                    )
                {
                    return;
                }

                var saving_throw = evt.Context.SavingThrow;
                if (saving_throw == null || saving_throw.IsPassed 
                    || (save_type != SavingThrowType.Unknown && saving_throw.Type != save_type))
                {
                    return;
                }

                if (last_saving_throw != saving_throw)
                {
                    last_saving_throw = saving_throw;
                    int new_roll = RulebookEvent.Dice.D20;
                    passed = saving_throw.IsSuccessRoll(new_roll);
                    Common.AddBattleLogMessage(saving_throw.Initiator.CharacterName + $" rolls due to {this.Fact.Name}: " + $"{new_roll}  ({(passed ? "Success" : "Failure")})");
                }
                if (passed)
                {
                    TimeSpan round = 6.Seconds();
                    Harmony12.Traverse.Create(evt).Property("Duration").SetValue(new TimeSpan?(round));

                }
            }

            public override void OnEventDidTrigger(RuleApplyBuff evt)
            {

            }
        }



        public interface IMentalFocusChangedHandler : IGlobalSubscriber
        {
            void onMentalFocusChanged(UnitDescriptor unit);
        }


        class ContextValueWithLimitProperty : PropertyValueGetter
        {
            public ContextRankConfig base_value;
            public ContextRankConfig max_value = null;
            public BlueprintUnitFact parent_feature;

            public static BlueprintUnitProperty createProperty(string name, string guid, ContextRankConfig base_value, ContextRankConfig max_value, BlueprintUnitFact parent_fact)
            {
                var p = Helpers.Create<BlueprintUnitProperty>();
                p.name = name;
                Main.library.AddAsset(p, guid);
                p.SetComponents(Helpers.Create<ContextValueWithLimitProperty>(a => { a.base_value = base_value; a.max_value = max_value; a.parent_feature = parent_fact; }));

                return p;
            }

            public override int GetInt(UnitEntityData unit)
            {
                var context = parent_feature == null ? Helpers.GetMechanicsContext() : unit.Descriptor?.GetFact(parent_feature)?.MaybeContext;

                if (context == null)
                {
                    return 0;
                }

                int val = base_value.GetValue(context);
                if (max_value != null)
                {
                    var max_val = max_value.GetValue(context);
                    val = Math.Min(max_val, val);
                }
                return val;
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class TwoWeaponRendFeature : RuleInitiatorLogicComponent<RuleAttackWithWeapon>
        {
            public DiceFormula RendDamage;
            private bool can_rend;

            public override void OnTurnOn()
            {
                this.Owner.State.HasRend.Retain();
            }

            public override void OnTurnOff()
            {
                this.Owner.State.HasRend.Release();
            }

            public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
            {
            }

            public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
                if (evt.IsFirstAttack)
                {
                    can_rend = true;
                }

                if (!can_rend)
                {
                    return;
                }

                if (!evt.Initiator.Body.PrimaryHand.HasWeapon || !evt.Initiator.Body.SecondaryHand.HasWeapon || (evt.Initiator.Body.PrimaryHand.Weapon.Blueprint.IsNatural || evt.Initiator.Body.SecondaryHand.Weapon.Blueprint.IsNatural) || (evt.Initiator.Body.PrimaryHand.Weapon == evt.Initiator.Body.EmptyHandWeapon || evt.Initiator.Body.SecondaryHand.Weapon == evt.Initiator.Body.EmptyHandWeapon))
                    return;
                if (!evt.IsRend || !evt.AttackRoll.IsHit)
                    return;
                if (this.Owner.Body.SecondaryHand.MaybeWeapon?.Blueprint == null)
                {
                    return;
                }

                
                BaseDamage damage = this.Owner.Body.SecondaryHand.MaybeWeapon.Blueprint.DamageType.GetDamageDescriptor(this.RendDamage, (int)((double)this.Owner.Stats.Strength.Bonus * 1.5)).CreateDamage();
                Game.Instance.Rulebook.TriggerEvent<RuleDealDamage>(new RuleDealDamage(this.Owner.Unit, evt.Target, (DamageBundle)damage));
                can_rend = false;
            }
        }


        public class ReceiveExtraDamageOnWeaponAttack : RuleTargetLogicComponent<RulePrepareDamage>
        {
            public DamageDescription damage;

            public override void OnEventAboutToTrigger(RulePrepareDamage evt)
            {
                if (evt.DamageBundle.Weapon == null)
                {
                    return;
                }
            
                BaseDamage damage = this.damage.CreateDamage();
                evt.DamageBundle.Add(damage);
            }

            public override void OnEventDidTrigger(RulePrepareDamage evt)
            {
            }
        }


        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class AddFeatureOnClassLevelIfHasFact : OwnedGameLogicComponent<UnitDescriptor>, IUnitGainLevelHandler, IGlobalSubscriber
        {
            public BlueprintCharacterClass Class;
            public int Level;
            public BlueprintFeature Feature;
            public bool BeforeThisLevel;
            public BlueprintCharacterClass[] AdditionalClasses;
            public BlueprintArchetype[] Archetypes;
            public BlueprintUnitFact fact;
            [JsonProperty]
            private Fact m_AppliedFact;

            public override void OnFactActivate()
            {
                this.Apply();
            }

            public override void OnFactDeactivate()
            {
                this.Owner.RemoveFact(this.m_AppliedFact);
            }

            public void HandleUnitGainLevel(UnitDescriptor unit, BlueprintCharacterClass @class)
            {
                this.Apply();
            }

            private void Apply()
            {
                if (this.IsFeatureShouldBeApplied())
                {
                    if (this.m_AppliedFact != null)
                        return;
                    this.m_AppliedFact = this.Owner.AddFact((BlueprintUnitFact)this.Feature, (MechanicsContext)null, (FeatureParam)null);
                }
                else
                {
                    if (this.m_AppliedFact == null)
                        return;
                    this.Owner.RemoveFact(this.m_AppliedFact);
                    this.m_AppliedFact = (Fact)null;
                }
            }

            private bool IsFeatureShouldBeApplied()
            {
                if (!this.Owner.HasFact(fact))
                {
                    return false;
                }
                int classLevel = ReplaceCasterLevelOfAbility.CalculateClassLevel(this.Class, this.AdditionalClasses, this.Owner, this.Archetypes);
                if (this.BeforeThisLevel && classLevel >= this.Level)
                    return false;
                if (classLevel < this.Level && this.BeforeThisLevel)
                    return true;
                if (classLevel >= this.Level)
                    return !this.BeforeThisLevel;
                return false;
            }

            public override void PostLoad()
            {
                base.PostLoad();
                int num = this.m_AppliedFact == null ? 0 : (!this.Owner.HasFact(this.m_AppliedFact) ? 1 : 0);
                if (num != 0)
                {
                    this.m_AppliedFact.Dispose();
                    this.m_AppliedFact = (Fact)null;
                }
                if (num == 0 || !((IList<BlueprintFeatureBase>)BlueprintRoot.Instance.PlayerUpgradeActions.AllowedForRestoreFeatures).HasItem<BlueprintFeatureBase>((BlueprintFeatureBase)this.Feature))
                    return;
                this.Apply();
            }
        }
    }
}
