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

namespace CallOfTheWild
{
    namespace NewMechanics.MetamagicMechanics
    {
        public interface ISpellTargetRestrictor
        {
            bool canBeCastOnTarget(AbilityData spell, UnitDescriptor caster, UnitDescriptor target);
        }

        [AllowedOn(typeof(BlueprintUnitFact))]
        public class MetamagicOnPersonalSpell : AutoMetamagicExtender, IInitiatorRulebookHandler<RuleCastSpell>, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookSubscriber,
                                                ISpellTargetRestrictor
        {
            public BlueprintAbilityResource resource = null;
            public int amount;
            public BlueprintUnitFact[] cost_reducing_facts = new BlueprintUnitFact[0];
            public BlueprintSpellbook spellbook = null;
            private int cost_to_pay;
            public BlueprintAbility[] allowed_spells = new BlueprintAbility[0];


            public override bool CanBeUsedOn(BlueprintAbility ability, [CanBeNull] AbilityData data)
            {
                if (data == null || ability == null)
                {
                    return false;
                }
                if (data?.Spellbook == null)
                {
                    return false;
                }

                if (spellbook != null && spellbook != data.Spellbook.Blueprint)
                {
                    return false;
                }

                if ((ability.AvailableMetamagic & Metamagic) == 0)
                {
                    return false;
                }

                if (!allowed_spells.Empty() && !allowed_spells.Contains(ability))
                {
                    return false;
                }

                if (!Common.isPersonalSpell(data))
                {
                    return false;
                }
                cost_to_pay = calculate_cost(this.Owner.Unit);
                if (resource != null && this.Owner.Resources.GetResourceAmount((BlueprintScriptableObject)this.resource) < cost_to_pay)
                {
                    return false;
                }
                return true;
            }


            public bool canBeCastOnTarget(AbilityData spell, UnitDescriptor caster, UnitDescriptor target)
            {
                if (!CanBeUsedOn(spell?.Blueprint, spell))
                {
                    return true;
                }

                return caster == target;
            }


            private int calculate_cost(UnitEntityData caster)
            {
                if (resource == null)
                {
                    return 0;
                }
                var cost = amount;
                foreach (var f in cost_reducing_facts)
                {
                    if (caster.Buffs.HasFact(f))
                    {
                        cost--;
                    }
                }
                return cost < 0 ? 0 : cost;
            }

            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {

                cost_to_pay = 0;

                if (!CanBeUsedOn(evt.Spell, evt.AbilityData))
                {
                    return;
                }
                cost_to_pay = calculate_cost(this.Owner.Unit);
                
                evt.AddMetamagic(Metamagic);
            }


            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {

            }

            public void OnEventAboutToTrigger(RuleCastSpell evt)
            {

                if (cost_to_pay == 0 || evt.Spell.SourceItem != null)
                {
                    cost_to_pay = 0;
                    return;
                }

                if (evt.SpellTarget.Unit != evt.Initiator)
                {
                    evt.SpellFailureChance = 100;
                    cost_to_pay = 0;
                }
            }

            public void OnEventDidTrigger(RuleCastSpell evt)
            {
                if (cost_to_pay == 0)
                {
                    return;
                }

                this.Owner.Resources.Spend(resource, cost_to_pay);
                cost_to_pay = 0;
            }
        }



        [ComponentName("Apply metamagic for resource")]
        [AllowedOn(typeof(Kingmaker.Blueprints.Facts.BlueprintUnitFact))]
        public class MetaRage : AutoMetamagicExtender, IInitiatorRulebookHandler<RuleCastSpell>, IInitiatorRulebookHandler<RuleCalculateAbilityParams>
        {
            public BlueprintAbilityResource resource;
            private int cost_to_pay = 0;

            public MetaRage(Metamagic metamagic_to_apply, BlueprintAbilityResource resource_to_use)
            {
                Metamagic = metamagic_to_apply;
                resource = resource_to_use;
            }


            public override bool CanBeUsedOn(BlueprintAbility ability, [CanBeNull] AbilityData data)
            {
                bool is_metamagic_not_available = ability == null || data?.Spellbook == null || ability.Type != AbilityType.Spell
                                              || ((ability.AvailableMetamagic & Metamagic) == 0);

                if (is_metamagic_not_available)
                {
                    return false;
                }

                int cost = 2 * (data.SpellLevel + MetamagicHelper.DefaultCost(Metamagic));
                if (this.resource == null || this.Owner.Resources.GetResourceAmount((BlueprintScriptableObject)this.resource) < cost)
                {
                    return false;
                }

                return true;
            }


            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                cost_to_pay = 0;
                if (!CanBeUsedOn(evt.Spell, evt.AbilityData))
                {
                    return;
                }

                cost_to_pay = 2 * (evt.Spellbook.GetSpellLevel(evt.Spell) + MetamagicHelper.DefaultCost(Metamagic));
                evt.AddMetamagic(this.Metamagic);
            }

            public void OnEventAboutToTrigger(RuleCastSpell evt)
            {
                if (cost_to_pay == 0 || evt.Spell.SourceItem != null)
                {
                    cost_to_pay = 0;
                    return;
                }
            }

            public void OnEventDidTrigger(RuleCastSpell evt)
            {
                if (cost_to_pay == 0)
                {
                    return;
                }
                this.Owner.Resources.Spend((BlueprintScriptableObject)this.resource, cost_to_pay);
            }



            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }



        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ReachSpellStrike : AutoMetamagicExtender, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookSubscriber
        {         
            public override bool CanBeUsedOn(BlueprintAbility ability, [CanBeNull] AbilityData data)
            {
                bool is_metamagic_not_available = ability == null || data?.Spellbook == null || ability.Type != AbilityType.Spell
                                              || ((ability.AvailableMetamagic & this.Metamagic) == 0);


                if (is_metamagic_not_available)
                {
                    return false;
                }

                var caster = data?.Caster;

                if (caster == null)
                {
                    return false;
                }

                if (data.Blueprint.StickyTouch == null && data.Blueprint.GetComponent<AbilityDeliverTouch>() == null)
                {
                    return false;
                }

                var unit_part_magus = caster.Get<UnitPartMagus>();

                if (unit_part_magus == null)
                {
                    return false;
                }

                if ((bool)unit_part_magus.EldritchArcher && unit_part_magus.Spellstrike.Active)
                {
                    return true;
                }

                return false;
            }


            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                if (CanBeUsedOn(evt.Spell, evt.AbilityData))
                {
                    evt.AddMetamagic(this.Metamagic);
                }
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }

        [AllowedOn(typeof(BlueprintBuff))]
        public class MetamagicUpToSpellLevel : AutoMetamagicExtender, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookSubscriber
        {
            public int max_level = 10;
            public bool except_fullround_action;

            public override bool CanBeUsedOn(BlueprintAbility ability, [CanBeNull] AbilityData data)
            {
                bool is_metamagic_not_available = ability == null || data?.Spellbook == null || ability.Type != AbilityType.Spell
                                              || ((ability.AvailableMetamagic & Metamagic) == 0);

                if (is_metamagic_not_available)
                {
                    return false;
                }

                if (data.SpellLevel > max_level)
                {
                    return false;
                }

                if (ability.IsFullRoundAction && except_fullround_action)
                {
                    return false;
                }
                return true;
            }


            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                if (!CanBeUsedOn(evt.Spell, evt.AbilityData))
                {
                    return;
                }
                evt.AddMetamagic(Metamagic);
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }



        [AllowedOn(typeof(BlueprintBuff))]
        public class MetamagicOnSchool : AutoMetamagicExtender, IInitiatorRulebookHandler<RuleCastSpell>, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookSubscriber
        {
            public SpellSchool school;
            public BlueprintAbilityResource resource = null;
            public int amount;
            public BlueprintUnitFact[] cost_reducing_facts = new BlueprintUnitFact[0];
            private int cost_to_pay;
            public BlueprintSpellbook spellbook = null;

            private int calculate_cost(UnitEntityData caster)
            {
                var cost = amount;
                foreach (var f in cost_reducing_facts)
                {
                    if (caster.Buffs.HasFact(f))
                    {
                        cost--;
                    }
                }
                return cost < 0 ? 0 : cost;
            }


            public override bool CanBeUsedOn(BlueprintAbility ability, [CanBeNull] AbilityData data)
            {
                bool is_metamagic_not_available = ability == null || data?.Spellbook == null || ability.Type != AbilityType.Spell
                                              || ((ability.AvailableMetamagic & Metamagic) == 0);

                if (is_metamagic_not_available)
                {
                    return false;
                }

                if (spellbook != null && data?.Spellbook?.Blueprint != spellbook)
                {
                    return false;
                }

                if (this.Abilities != null && !this.Abilities.Empty() && !this.Abilities.Contains(ability))
                {
                    return false;
                }

                if (ability.School != school)
                {
                    return false;
                }

                int cost = calculate_cost(this.Owner.Unit);
                if (resource != null && this.Owner.Resources.GetResourceAmount((BlueprintScriptableObject)this.resource) < cost)
                {
                    return false;
                }

                return true;
            }


            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                cost_to_pay = 0;
                if (!CanBeUsedOn(evt.Spell, evt.AbilityData))
                {
                    return;
                }
                cost_to_pay = calculate_cost(this.Owner.Unit);
                evt.AddMetamagic(Metamagic);
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }

            public void OnEventAboutToTrigger(RuleCastSpell evt)
            {
                if (cost_to_pay == 0 || evt.Spell.SourceItem != null)
                {
                    cost_to_pay = 0;
                    return;
                }
            }

            public void OnEventDidTrigger(RuleCastSpell evt)
            {

                if (cost_to_pay == 0)
                {
                    return;
                }
                this.Owner.Resources.Spend(resource, cost_to_pay);
                cost_to_pay = 0;
            }
        }


        [AllowedOn(typeof(BlueprintBuff))]
        public class MetamagicOnSpellType : AutoMetamagicExtender, IInitiatorRulebookHandler<RuleCastSpell>, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookSubscriber
        {
            public bool apply_to_arcane = false;
            public bool apply_to_divine = false;
            public bool apply_to_alchemist = false;
            public bool apply_to_psychic = false;
            public BlueprintAbilityResource resource = null;
            public int amount;
            public BlueprintUnitFact[] cost_reducing_facts = new BlueprintUnitFact[0];
            private int cost_to_pay;
            public bool limit_spell_level;

            private int calculate_cost(UnitEntityData caster)
            {
                var cost = amount;
                foreach (var f in cost_reducing_facts)
                {
                    if (caster.Buffs.HasFact(f))
                    {
                        cost--;
                    }
                }
                return cost < 0 ? 0 : cost;
            }


            public override bool CanBeUsedOn(BlueprintAbility ability, [CanBeNull] AbilityData data)
            {
                bool is_metamagic_not_available = ability == null || data?.Spellbook == null || ability.Type != AbilityType.Spell
                                              || ((ability.AvailableMetamagic & Metamagic) == 0);

                if (is_metamagic_not_available)
                {
                    return false;
                }

                if (!checkSpellbook(data?.Spellbook?.Blueprint))
                {
                    return false;
                }


                int cost = calculate_cost(this.Owner.Unit);
                if (resource != null && this.Owner.Resources.GetResourceAmount((BlueprintScriptableObject)this.resource) < cost)
                {
                    return false;
                }


                if (limit_spell_level && data.Spellbook.MaxSpellLevel < data.SpellLevel + Metamagic.DefaultCost())
                {
                    return false;
                }

                return true;
            }


            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                cost_to_pay = 0;
                if (!CanBeUsedOn(evt.Spell, evt.AbilityData))
                {
                    return;
                }
                cost_to_pay = calculate_cost(this.Owner.Unit);
                evt.AddMetamagic(Metamagic);
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }

            public void OnEventAboutToTrigger(RuleCastSpell evt)
            {
                if (cost_to_pay == 0 || evt.Spell.SourceItem != null)
                {
                    cost_to_pay = 0;
                    return;
                }
            }

            public void OnEventDidTrigger(RuleCastSpell evt)
            {

                if (cost_to_pay == 0)
                {
                    return;
                }
                this.Owner.Resources.Spend(resource, cost_to_pay);
                cost_to_pay = 0;
            }


            private bool checkSpellbook(BlueprintSpellbook spellbook)
            {
                if (spellbook == null)
                {
                    return false;
                }

                if (spellbook.IsArcane)
                {
                    return apply_to_arcane;
                }
                else if (spellbook.IsAlchemist)
                {
                    return apply_to_alchemist;
                }
                else if (spellbook.GetComponent<SpellbookMechanics.PsychicSpellbook>() != null)
                {
                    return apply_to_psychic;
                }
                else
                {
                    return apply_to_divine;
                }
            }
        }


        [AllowedOn(typeof(BlueprintBuff))]
        public class MetamagicOnSpellDescriptor : AutoMetamagicExtender, IInitiatorRulebookHandler<RuleCastSpell>, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookSubscriber
        {
            public SpellDescriptorWrapper spell_descriptor;
            public BlueprintAbilityResource resource = null;
            public int amount;
            public BlueprintUnitFact[] cost_reducing_facts = new BlueprintUnitFact[0];
            private int cost_to_pay;
            public BlueprintSpellbook spellbook = null;
            public BlueprintCharacterClass specific_class = null;
            public bool limit_spell_level;

            private int calculate_cost(UnitEntityData caster)
            {
                var cost = amount;
                foreach (var f in cost_reducing_facts)
                {
                    if (caster.Buffs.HasFact(f))
                    {
                        cost--;
                    }
                }
                return cost < 0 ? 0 : cost;
            }


            public override bool CanBeUsedOn(BlueprintAbility ability, [CanBeNull] AbilityData data)
            {
                bool is_metamagic_not_available = ability == null || data?.Spellbook == null || ability.Type != AbilityType.Spell
                                              || ((ability.AvailableMetamagic & Metamagic) == 0);
                
                if (is_metamagic_not_available)
                {
                    return false;
                }
              
                if (!Helpers.checkSpellbook(spellbook, specific_class, data?.Spellbook, this.Owner))
                {
                    return false;
                }

                if (this.Abilities != null && !this.Abilities.Empty() && !this.Abilities.Contains(ability))
                {
                    return false;
                }

                if (!spell_descriptor.HasAnyFlag(data.Blueprint.SpellDescriptor) && spell_descriptor != SpellDescriptor.None)
                {
                    return false;
                }

                int cost = calculate_cost(this.Owner.Unit);
                if (resource != null && this.Owner.Resources.GetResourceAmount((BlueprintScriptableObject)this.resource) < cost)
                {
                    return false;
                }


                if (limit_spell_level && data.Spellbook.MaxSpellLevel < data.SpellLevel + Metamagic.DefaultCost())
                {
                    return false;
                }

                return true;
            }


            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                cost_to_pay = 0;
                if (!CanBeUsedOn(evt.Spell, evt.AbilityData))
                {
                    return;
                }
                cost_to_pay = calculate_cost(this.Owner.Unit);
                evt.AddMetamagic(Metamagic);
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }

            public void OnEventAboutToTrigger(RuleCastSpell evt)
            {
                if (cost_to_pay == 0 || evt.Spell.SourceItem != null)
                {
                    cost_to_pay = 0;
                    return;
                }
            }

            public void OnEventDidTrigger(RuleCastSpell evt)
            {

                if (cost_to_pay == 0)
                {
                    return;
                }
                this.Owner.Resources.Spend(resource, cost_to_pay);
                cost_to_pay = 0;
            }
        }


        [AllowedOn(typeof(BlueprintBuff))]
        public class MetamagicOnSpellList : AutoMetamagicExtender, IInitiatorRulebookHandler<RuleCastSpell>, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookSubscriber
        {
            public BlueprintAbility[] spell_list;
            public BlueprintAbilityResource resource = null;
            public int amount;
            public BlueprintUnitFact[] cost_reducing_facts = new BlueprintUnitFact[0];
            private int cost_to_pay = -1;
            public bool consume_extra_spell_slot = false;
            public int increase_spell_dc;

            private int calculate_cost(UnitEntityData caster)
            {
                var cost = amount;
                foreach (var f in cost_reducing_facts)
                {
                    if (caster.Buffs.HasFact(f))
                    {
                        cost--;
                    }
                }
                return cost < 0 ? 0 : cost;
            }

            public override bool CanBeUsedOn(BlueprintAbility ability, [CanBeNull] AbilityData data)
            {
                bool is_metamagic_not_available = ability == null 
                                                 || (data?.Spellbook == null || ability.Type != AbilityType.Spell)
                                                 || ((ability.AvailableMetamagic & Metamagic) == 0);
                if (is_metamagic_not_available)
                {
                    return false;
                }

                if (!spell_list.Contains(ability))
                {
                    return false;
                }


                if (consume_extra_spell_slot && (!data.Spellbook.Blueprint.Spontaneous || data.Spellbook.GetSpontaneousSlots(data.SpellLevel) < 2))
                {
                    return false;
                }

                int cost = calculate_cost(this.Owner.Unit);
                if (resource != null && this.Owner.Resources.GetResourceAmount((BlueprintScriptableObject)this.resource) < cost)
                {
                    return false;
                }

                return true;
            }


            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                cost_to_pay = -1;
                if (!CanBeUsedOn(evt.Spell, evt.AbilityData))
                {
                    return;
                }
                cost_to_pay = calculate_cost(this.Owner.Unit);
                evt.AddMetamagic(Metamagic);

                if (increase_spell_dc > 0)
                {
                    evt.AddBonusDC(increase_spell_dc);
                }
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }

            public void OnEventAboutToTrigger(RuleCastSpell evt)
            {
                if (cost_to_pay == -1 || evt.Spell.SourceItem != null)
                {
                    cost_to_pay = -1;
                    return;
                }
            }

            public void OnEventDidTrigger(RuleCastSpell evt)
            {

                if (cost_to_pay == -1)
                {
                    return;
                }
                this.Owner.Resources.Spend(resource, cost_to_pay);
                if (consume_extra_spell_slot)
                {
                    evt.Spell.Spellbook.Spend(evt.Spell);
                }
                cost_to_pay = -1;
            }
        }



        [AllowedOn(typeof(BlueprintBuff))]
        public class MetamagicIfHasParametrizedFeature : AutoMetamagicExtender, IInitiatorRulebookHandler<RuleCastSpell>, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookSubscriber
        {
            public BlueprintParametrizedFeature[] required_features;
            public BlueprintAbilityResource resource = null;
            public int amount;
            public BlueprintUnitFact[] cost_reducing_facts = new BlueprintUnitFact[0];
            private int cost_to_pay = -1;
            public int increase_spell_dc;

            private int calculate_cost(UnitEntityData caster)
            {
                var cost = amount;
                foreach (var f in cost_reducing_facts)
                {
                    if (caster.Buffs.HasFact(f))
                    {
                        cost--;
                    }
                }
                return cost < 0 ? 0 : cost;
            }

            public override bool CanBeUsedOn(BlueprintAbility ability, [CanBeNull] AbilityData data)
            {
                bool is_metamagic_not_available = ability == null
                                                 || (data?.Spellbook == null || ability.Type != AbilityType.Spell)
                                                 || ((ability.AvailableMetamagic & Metamagic) == 0);

                if (is_metamagic_not_available)
                {
                    return false;
                }

                if (!this.Owner.Unit.Descriptor.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => required_features.Contains(p.Blueprint)).Any(p => p.Param == ability))
                {
                    return false;
                }

                int cost = calculate_cost(this.Owner.Unit);
                if (resource != null && this.Owner.Resources.GetResourceAmount((BlueprintScriptableObject)this.resource) < cost)
                {
                    return false;
                }

                return true;
            }


            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                cost_to_pay = -1;
                if (!CanBeUsedOn(evt.Spell, evt.AbilityData))
                {
                    return;
                }
                cost_to_pay = calculate_cost(this.Owner.Unit);
                evt.AddMetamagic(Metamagic);

                if (increase_spell_dc > 0)
                {
                    evt.AddBonusDC(increase_spell_dc);
                }
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }

            public void OnEventAboutToTrigger(RuleCastSpell evt)
            {
                if (cost_to_pay == -1 || evt.Spell.SourceItem != null)
                {
                    cost_to_pay = -1;
                    return;
                }
            }

            public void OnEventDidTrigger(RuleCastSpell evt)
            {

                if (cost_to_pay == -1)
                {
                    return;
                }
                this.Owner.Resources.Spend(resource, cost_to_pay);
                cost_to_pay = -1;
            }
        }


        public class ConsumeRodCharge: ContextAction
        {
            public BlueprintActivatableAbility rod_ability;

            public override string GetCaption()
            {
                return "Consume rod charge";
            }

            public override void RunAction()
            {
                foreach (ActivatableAbility activatableAbility in  this.Context.MaybeCaster.ActivatableAbilities)
                {
                    if (activatableAbility.Blueprint == this.rod_ability && activatableAbility.IsOn)
                    {
                        activatableAbility.Get<ActivatableAbilityResourceLogic>()?.ManualSpendResource();
                        break;
                    }
                }
            }
        }


        public class AutoMetamagicExtender : AutoMetamagic
        {
            protected static BlueprintAbility ExtractBlueprint(BlueprintAbility ability, [CanBeNull] AbilityData data)
            {
                BlueprintAbility blueprintAbility1;
                if (data == null)
                {
                    blueprintAbility1 = null;
                }
                else
                {
                    AbilityData convertedFrom = data.ConvertedFrom;
                    blueprintAbility1 = convertedFrom != null ? convertedFrom.Blueprint.Or<BlueprintAbility>(null) : null;
                }
                if (blueprintAbility1 == null)
                {
                    BlueprintAbility blueprintAbility2 = ability.Or<BlueprintAbility>(null);
                    blueprintAbility1 = (blueprintAbility2 != null ? blueprintAbility2.Parent.Or<BlueprintAbility>(null) : null) ?? ability;
                }
                return blueprintAbility1;
            }

            virtual public bool CanBeUsedOn(BlueprintAbility ability, [CanBeNull] AbilityData data)
            {
                return true;
            }
        }

        public class OneFreeMetamagicForSpell : ParametrizedFeatureComponent, IInitiatorRulebookHandler<RuleApplyMetamagic>
        {
            public int max_spell_level = 9;

            public void OnEventAboutToTrigger(RuleApplyMetamagic evt)
            {
                var spell = this.Param?.Blueprint as BlueprintAbility;
                if (spell == null)
                {
                    return;
                }

                bool same_spells = SpellDuplicates.isDuplicateOrParent(spell, evt.Spell);

                if (!same_spells)
                {
                    return;
                }
                var spellbook = evt.Spellbook;
                if (spellbook == null || spellbook.GetSpellLevel(spell) > max_spell_level)
                {
                    return;
                }
                if (evt.AppliedMetamagics.Count == 0) return;
                int reduction = evt.AppliedMetamagics.Max(m => m == Metamagic.Heighten ? 0 : m.DefaultCost());

                evt.ReduceCost(reduction);
            }

            public void OnEventDidTrigger(RuleApplyMetamagic evt)
            {
            }
        }


        public class ReduceMetamagicCostForSpecifiedSpells : RuleInitiatorLogicComponent<RuleApplyMetamagic>
        {
            public int reduction;
            public BlueprintAbility[] spells;

            public override void OnEventAboutToTrigger(RuleApplyMetamagic evt)
            {
                var spellbook = evt.Spellbook;
                if (spellbook == null || !SpellDuplicates.containsDuplicateOrParent(spells, evt.Spell))
                {
                    return;
                }
                if (evt.AppliedMetamagics.Count == 0) return;

                evt.ReduceCost(reduction);
            }


            public override void OnEventDidTrigger(RuleApplyMetamagic evt)
            {
            }
        }


        public class ReduceMetamagicCostForSpellParametrized : ParametrizedFeatureComponent, IInitiatorRulebookHandler<RuleApplyMetamagic>
        {
            public int reduction = 1;

            public void OnEventAboutToTrigger(RuleApplyMetamagic evt)
            {
                var spell = this.Param?.Blueprint as BlueprintAbility;
                if (spell == null)
                {
                    return;
                }

                var spellbook = evt.Spellbook;
                if (spellbook == null || !SpellDuplicates.isDuplicateOrParent(spell, evt.Spell))
                {
                    return;
                }
                if (evt.AppliedMetamagics.Count == 0) return;

                evt.ReduceCost(reduction);
            }

            public void OnEventDidTrigger(RuleApplyMetamagic evt)
            {
            }
        }


        [Harmony12.HarmonyPatch(typeof(AutoMetamagic))]
        [Harmony12.HarmonyPatch("ShouldApplyTo", Harmony12.MethodType.Normal)]
        class AutoMetamagic__ShouldApplyTo__Patch
        {
            static bool Prefix(AutoMetamagic __instance, BlueprintAbility ability, AbilityData data, ref bool __result)
            {
                Main.TraceLog();
                if (__instance is AutoMetamagicExtender)
                {                   
                    __result = ((AutoMetamagicExtender)__instance).CanBeUsedOn(ability, data);
                    return false;
                }

                return true;
            }
        }



        public class ChangeSpellElementalDamage : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCastSpell>, IRulebookHandler<RuleCastSpell>, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RulePrepareDamage>, IRulebookHandler<RulePrepareDamage>
        {
            public DamageEnergyType Element;

            public void OnEventAboutToTrigger(RuleCastSpell evt)
            {
            }

            public void OnEventDidTrigger(RuleCastSpell evt)
            {
                AbilityExecutionContext context = evt.Context;
                context.RemoveSpellDescriptor(SpellDescriptor.Fire);
                context.RemoveSpellDescriptor(SpellDescriptor.Cold);
                context.RemoveSpellDescriptor(SpellDescriptor.Acid);
                context.RemoveSpellDescriptor(SpellDescriptor.Electricity);
                context.AddSpellDescriptor(ChangeSpellElementalDamage.ElementToSpellDescriptor(this.Element));
                //context.Recalculate();
            }

            public void OnEventAboutToTrigger(RulePrepareDamage evt)
            {
                var context2 = Helpers.GetMechanicsContext()?.SourceAbilityContext;
                if (context2 == null)
                {
                    var source_buff = (evt.Reason?.Item as ItemEntityWeapon)?.Blueprint.GetComponent<NewMechanics.EnchantmentMechanics.WeaponSourceBuff>()?.buff;

                    if (source_buff != null)
                    {
                        context2 = evt.Initiator.Buffs?.GetBuff(source_buff)?.MaybeContext?.SourceAbilityContext;
                    }
                }
                if (context2 == null)
                {
                    return;
                }

                if (context2.SourceAbility.IsSpell)
                {

                    foreach (BaseDamage item in evt.DamageBundle)
                    {
                        (item as EnergyDamage)?.ReplaceEnergy(this.Element);
                    }
                }
            }

            public void OnEventDidTrigger(RulePrepareDamage evt)
            {
            }

            private static SpellDescriptor ElementToSpellDescriptor(DamageEnergyType element)
            {
                switch (element)
                {
                    case DamageEnergyType.Fire:
                        return SpellDescriptor.Fire;
                    case DamageEnergyType.Cold:
                        return SpellDescriptor.Cold;
                    case DamageEnergyType.Electricity:
                        return SpellDescriptor.Electricity;
                    case DamageEnergyType.Acid:
                        return SpellDescriptor.Acid;
                    default:
                        return SpellDescriptor.Fire;
                }
            }

            public override void Validate(ValidationContext context)
            {
                base.Validate(context);
                if (this.Element == DamageEnergyType.Fire || this.Element == DamageEnergyType.Cold || (this.Element == DamageEnergyType.Acid || this.Element == DamageEnergyType.Electricity))
                    return;
                context.AddError("Only Fire, Cold, Acid or Electricity are allowed", (object[])Array.Empty<object>());
            }
        }




        [ComponentName("Apply metamagic for resource")]
        [AllowedOn(typeof(Kingmaker.Blueprints.Facts.BlueprintUnitFact))]
        public class ApplyMetamagicToPersonalSpell : RuleInitiatorLogicComponent<RuleCastSpell>, IInitiatorRulebookSubscriber
        {
            public Metamagic metamagic;
            public int caster_level_increase;
            public int dc_increase;

            public override void OnEventAboutToTrigger(RuleCastSpell evt)
            {

            }

            public override void OnEventDidTrigger(RuleCastSpell evt)
            {
                if (evt.Spell.Blueprint == null || !Common.isPersonalSpell(evt.Spell) || evt.Spell.Blueprint.Type != AbilityType.Spell)
                {
                    return;
                }
                if (evt.Context.MainTarget != evt.Context.MaybeCaster)
                {
                    return;
                }

                evt.Context.Params.CasterLevel += caster_level_increase;
                evt.Context.Params.DC += dc_increase;

                if ((evt.Spell.Blueprint.AvailableMetamagic & metamagic) > 0)
                {
                    evt.Context.Params.Metamagic = evt.Context.Params.Metamagic | metamagic;
                }
                evt.Context.RecalculateRanks();

                //in case there is no explicit context rank config, it will not be recalcualted by above function, so we should do it manually
                var found_values = new bool[Enum.GetValues(typeof(AbilityRankType)).Cast<int>().Max() + 1];
                evt.Spell.Blueprint.GetComponents<ContextRankConfig>().ForEach(c => found_values[(int)c.Type] = true);
                var ranks = Helpers.GetField<int[]>(evt.Context, "m_Ranks");
                for (int i = 0; i < found_values.Length; i++)
                {
                    if (!found_values[i])
                    {
                        ranks[i] += caster_level_increase;
                    }
                }

                evt.Context.RecalculateSharedValues();
            }
        }



    }
}
