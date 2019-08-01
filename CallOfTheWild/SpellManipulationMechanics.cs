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
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UI.Constructor;

namespace CallOfTheWild
{
    namespace SpellManipulationMechanics
    {
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class FactStoreSpell : OwnedGameLogicComponent<UnitDescriptor>
        {
            [JsonProperty]
            private AbilityData spell = null;

            public void releaseSpellOnTarget(TargetWrapper target)
            {
                if (spell != null && spell.CanTarget(target))
                {
                    Rulebook.Trigger<RuleCastSpell>(new RuleCastSpell(spell, target));
                    Common.AddBattleLogMessage($"{this.Owner.CharacterName} released {spell.Blueprint.Name} from {this.Fact.Name}.");
                    spell = null;
                }
                
            }

            public void storeSpell(AbilityData new_spell)
            {
                spell = new_spell;
 
                Common.AddBattleLogMessage($"{this.Fact.MaybeContext.MaybeOwner.CharacterName} stored {spell.Blueprint.Name} in {this.Fact.Name}.");
            }
        }


        [ComponentName("Release spell stored in specified fact")]
        [AllowMultipleComponents]
        [PlayerUpgraderAllowed]
        public class ReleaseSpellStoredInSpecifiedBuff : ContextAction
        {
            public string Comment;
            public BlueprintUnitFact fact;

            public override void RunAction()
            {
                var stored_buff = Context.MaybeOwner.Buffs.GetFact(fact);
                if (stored_buff == null)
                {
                    stored_buff = Context.MaybeOwner.Descriptor.Progression.Features.GetFact(fact);
                }

                if (stored_buff != null)
                {
                    stored_buff.CallComponents<FactStoreSpell>(c => c.releaseSpellOnTarget(this.Target));
                }
            }

            public override string GetCaption()
            {
                return "Release Spell Stored in Specified Buff (" + this.Comment + " )";
            }
        }


        public class AbilityStoreSpellInFact : AbilityApplyEffect, IAbilityAvailabilityProvider, IAbilityParameterRequirement
        {
            public BlueprintUnitFact fact;
            public Predicate<AbilityData> check_slot_predicate;
            public int variant = 0;

            public bool canBeUsedOn(AbilityData ability)
            {
                var spell = getSpellOrVariant(ability);
                if (spell == null)
                {
                    return false;
                }

                return check_slot_predicate(spell);
            }

            public bool RequireSpellSlot
            {
                get
                {
                    return true;
                }
            }

            public bool RequireSpellbook
            {
                get
                {
                    return false;
                }
            }

            public bool RequireSpellLevel
            {
                get
                {
                    return false;
                }
            }

            public override void Apply(AbilityExecutionContext context, TargetWrapper target)
            {
                if (context.Ability.ParamSpellSlot == null || context.Ability.ParamSpellSlot.Spell == (AbilityData)null)
                    return;
                else if (context.Ability.ParamSpellSlot.Spell.Spellbook == null)
                    return;

                else
                {
                    SpellSlot availableSpellSlot = GetAvailableSpellSlot(context.Ability.ParamSpellSlot.Spell);
                    if (availableSpellSlot == null)
                        return;
                    else
                    {
                        storeSpell(availableSpellSlot, context);
                    }

                }
            }

            public bool IsAvailableFor(AbilityData ability)
            { 
                AbilityData spell = ability.ParamSpellSlot?.Spell;
                Spellbook spellbook = spell != null ? spell.Spellbook : null;
                if (spell == null || spellbook == null)
                    return false;
               
                return GetAvailableSpellSlot(spell) != null;
            }


            public string GetReason()
            {
                return string.Empty;
            }


            [CanBeNull]
            private SpellSlot GetAvailableSpellSlot(AbilityData ability)
            {
                if (ability.Spellbook == null)
                    return (SpellSlot)null;
                foreach (SpellSlot memorizedSpellSlot in ability.Spellbook.GetMemorizedSpellSlots(ability.SpellLevel))
                {
                    if (memorizedSpellSlot.Available && memorizedSpellSlot.Spell == ability && canBeUsedOn(memorizedSpellSlot.Spell))
                        return memorizedSpellSlot;
                }
                return (SpellSlot)null;
            }


            private void storeSpell(SpellSlot spell_slot, AbilityExecutionContext context)
            {
                var store_buff = context.MaybeOwner.Buffs.GetFact(fact);
                if (store_buff == null)
                {
                    store_buff = context.MaybeOwner.Descriptor.Progression.Features.GetFact(fact);
                }
                if (store_buff != null)
                {
                    var spell = getSpellOrVariant(spell_slot.Spell);

                    var sticky_touch = spell.Blueprint.GetComponent<AbilityEffectStickyTouch>();
                    if (sticky_touch != null)
                    {
                        var touch_spell = new AbilityData(spell, sticky_touch.TouchDeliveryAbility);
                        store_buff.CallComponents<FactStoreSpell>(c => c.storeSpell(touch_spell));
                    }
                    else
                    {
                        store_buff.CallComponents<FactStoreSpell>(c => c.storeSpell(spell));
                    }
                    spell_slot.Available = false;
                }
            }


            AbilityData getSpellOrVariant(AbilityData ability)
            {
                var variants = ability.Blueprint.GetComponent<AbilityVariants>();

                if (variants == null)
                {
                    return variant == 0 ? ability : null;
                }
                else
                {
                    return variants.Variants.Length > variant ? new AbilityData(ability, variants.Variants[variant]) : null;
                }
            }
        }


        [Harmony12.HarmonyPatch(typeof(ActionBarGroupSlot))]
        [Harmony12.HarmonyPatch("SetToggleAdditionalSpells", Harmony12.MethodType.Normal)]
        class ActionBarGroupSlot__SetToggleAdditionalSpells__Patch
        {
            static bool Prefix(ActionBarGroupSlot __instance, ref List<AbilityData> ___Conversion, ref ButtonPF ___ToggleAdditionalSpells, AbilityData spell)
            {
                Spellbook spellBook = spell.Spellbook;
                if (spellBook == null)
                    return false;
                ___Conversion = spellBook.GetSpontaneousConversionSpells(spell).EmptyIfNull<BlueprintAbility>().Select<BlueprintAbility, AbilityData>((Func<BlueprintAbility, AbilityData>)(b => new AbilityData(b, spellBook)
                {
                    ConvertedFrom = spell
                })).ToList<AbilityData>();
                SpellSlot spellSlot = (__instance.MechanicSlot as MechanicActionBarSlotMemorizedSpell)?.SpellSlot;
                if (spellSlot != null)
                {
                    foreach (Ability ability in spell.Caster.Abilities)
                    {
                        if ((bool)((UnityEngine.Object)ability.Blueprint.GetComponent<AbilityRestoreSpellSlot>()))
                            ___Conversion.Add(new AbilityData(ability)
                            {
                                ParamSpellSlot = spellSlot
                            });

                        var store_spell = ability.Blueprint.GetComponent<AbilityStoreSpellInFact>();
                        if (store_spell!= null && store_spell.canBeUsedOn(spell))
                        {
                            ___Conversion.Add(new AbilityData(ability)
                            {
                                ParamSpellSlot = spellSlot
                            });
                        }
                    }
                }
                SpellSlot paramSpellSlot = (__instance.MechanicSlot as MechanicActionBarSlotSpontaneusSpell)?.Spell.ParamSpellSlot;
                if (paramSpellSlot != null)
                {
                    foreach (Ability ability in spell.Caster.Abilities)
                    {
                        if ((bool)(ability.Blueprint.GetComponent<AbilityRestoreSpellSlot>()))
                            ___Conversion.Add(new AbilityData(ability)
                            {
                                ParamSpellSlot = paramSpellSlot
                            });
                    }
                }
                BlueprintAbility spellBlueprint = spell.Blueprint;
                if (!___Conversion.Any<AbilityData>((Func<AbilityData, bool>)(s => s.Blueprint != spellBlueprint)) && (spellBlueprint.Variants == null || !(spellBlueprint.Variants).Any<BlueprintAbility>()) || ___ToggleAdditionalSpells == null)
                    return false;
                ___ToggleAdditionalSpells.gameObject.SetActive(true);
                return false;
            }
        }
    }
}
