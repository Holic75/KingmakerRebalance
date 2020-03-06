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


        [AllowedOn(typeof(BlueprintBuff))]
        public class MetamagicOnSpellDescriptor : AutoMetamagicExtender, IInitiatorRulebookHandler<RuleCastSpell>, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookSubscriber
        {
            public SpellDescriptorWrapper spell_descriptor;
            public BlueprintAbilityResource resource = null;
            public int amount;
            public BlueprintUnitFact[] cost_reducing_facts = new BlueprintUnitFact[0];
            private int cost_to_pay;

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

                if (!spell_descriptor.HasAnyFlag(data.Blueprint.SpellDescriptor) && spell_descriptor != SpellDescriptor.None)
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
        public class MetamagicOnSpellList : AutoMetamagicExtender, IInitiatorRulebookHandler<RuleCastSpell>, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookSubscriber
        {
            public BlueprintAbility[] spell_list;
            public BlueprintAbilityResource resource = null;
            public int amount;
            public BlueprintUnitFact[] cost_reducing_facts = new BlueprintUnitFact[0];
            private int cost_to_pay = -1;
            public bool consume_extra_spell_slot = false;

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

                bool same_spells = evt.Spell.Parent == null ? SpellDuplicates.isDuplicate(evt.Spell, spell) : SpellDuplicates.isDuplicate(evt.Spell.Parent, spell);

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
                if (spellbook == null || !spells.Contains(evt.Spell))
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


        [Harmony12.HarmonyPatch(typeof(AutoMetamagic))]
        [Harmony12.HarmonyPatch("ShouldApplyTo", Harmony12.MethodType.Normal)]
        class AutoMetamagic__ShouldApplyTo__Patch
        {
            static bool Prefix(AutoMetamagic __instance, BlueprintAbility ability, AbilityData data, ref bool __result)
            {
                if (__instance is AutoMetamagicExtender)
                {
                    __result = ((AutoMetamagicExtender)__instance).CanBeUsedOn(ability, data);
                    return false;
                }

                return true;
            }
        }


        
    }
}
