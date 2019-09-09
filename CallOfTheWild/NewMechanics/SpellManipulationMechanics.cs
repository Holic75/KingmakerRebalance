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
using Kingmaker.UI.Tooltip;
using Kingmaker.Blueprints.Root.Strings;

namespace CallOfTheWild
{
    namespace SpellManipulationMechanics
    {
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class AddStoredSpellToCaption : OwnedGameLogicComponent<UnitDescriptor>
        {
            public BlueprintUnitFact store_fact;

            public void getStoredSpellName(out string spell_name)
            {
                spell_name = "";
                var stored_buff = Owner.Buffs.GetFact(store_fact);
                if (stored_buff == null)
                {
                    stored_buff = Owner.Progression.Features.GetFact(store_fact);
                }

                if (stored_buff != null)
                {
                    AbilityData data = null;
                    stored_buff.CallComponents<FactStoreSpell>(c => c.getStoredSpell(out data));
                    if (data != null)
                    {
                        spell_name = data.Name;
                    }
                }
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class InferIsFullRoundFromParamSpellSlot : BlueprintComponent
        {

        }

        [Harmony12.HarmonyPatch(typeof(AbilityData))]
        [Harmony12.HarmonyPatch("RequireFullRoundAction", Harmony12.MethodType.Getter)]
        class AbilityData__RequireFullRoundAction__Patch
        {
            static void Postfix(AbilityData __instance, ref bool __result)
            {
                if (__result == true)
                {
                    return;
                }

                if (__instance.Blueprint.GetComponent<InferIsFullRoundFromParamSpellSlot>() == null)
                {
                    return;
                }

                if (__instance.ParamSpellSlot?.Spell == null)
                {
                    return;
                }

                __result = __instance.ParamSpellSlot.Spell.RequireFullRoundAction;
            }
        }


 

        public class FactStoreSpell : OwnedGameLogicComponent<UnitDescriptor>
        {
            public ActionList actions_on_store = new ActionList();
            public bool ignore_target_checkers = false;
            [JsonProperty]
            private AbilityData spell = null;



            public void releaseSpellOnTarget(TargetWrapper target)
            {
                if (spell != null && (spell.CanTarget(target) || ignore_target_checkers))
                {
                    Rulebook.Trigger<RuleCastSpell>(new RuleCastSpell(spell, target));
                    Common.AddBattleLogMessage($"{this.Owner.CharacterName} released {spell.Blueprint.Name} from {this.Fact.Name}.");
                    spell = null;
                }
            }


            public void clearSpell()
            {
                spell = null;
            }

            public void storeSpell(AbilityData new_spell)
            {
                spell = new_spell;
 
                Common.AddBattleLogMessage($"{this.Fact.MaybeContext.MaybeOwner.CharacterName} stored {spell.Blueprint.Name} in {this.Fact.Name}.");
                (this.Fact as IFactContextOwner)?.RunActionInContext(this.actions_on_store, this.Owner.Unit);
            }


            public void getStoredSpell(out AbilityData stored_spell)
            {
                stored_spell = spell;
            }


            public static AbilityData getSpellStoredInFact(UnitDescriptor unit, BlueprintUnitFact fact)
            {
                if (unit == null || fact == null)
                {
                    return null;
                }

                var stored_buff = unit.Buffs.GetFact(fact);
                if (stored_buff == null)
                {
                    stored_buff = unit.Progression.Features.GetFact(fact);
                }

                if (stored_buff == null)
                {
                    return null;
                }
                AbilityData data = null;
                stored_buff.CallComponents<FactStoreSpell>(c => c.getStoredSpell(out data));
                return data;
            }

            public static bool hasSpellStoredInFact(UnitDescriptor unit, BlueprintUnitFact fact)
            {
                return getSpellStoredInFact(unit, fact) != null;
            }


            public static void storeSpell(UnitDescriptor unit, BlueprintUnitFact fact, AbilityData spell)
            {
                var store_buff = unit.Buffs.GetFact(fact);
                if (store_buff == null)
                {
                    store_buff = unit.Progression.Features.GetFact(fact);
                }
                if (store_buff != null)
                {
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
                }
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


        [ComponentName("Remove spell stored in specified fact")]
        [AllowMultipleComponents]
        [PlayerUpgraderAllowed]
        public class ClearSpellStoredInSpecifiedBuff : ContextAction
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
                    stored_buff.CallComponents<FactStoreSpell>(c => c.clearSpell());
                }
            }

            public override string GetCaption()
            {
                return "Clear Spell Stored in Specified Buff (" + this.Comment + " )";
            }
        }


        public class AbilityConvertSpell : AbilityApplyEffect, IAbilityAvailabilityProvider, IAbilityParameterRequirement
        {
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
            }


            public bool IsAvailableFor(AbilityData ability)
            {
                AbilityData spell = ability.ParamSpellSlot?.Spell;
                Spellbook spellbook = spell != null ? spell.Spellbook : null;
                if (spell == null || spellbook == null)
                    return false;

                return hasSpellSlot(ability.ParamSpellSlot);
            }


            public void spendSpellSlot(SpellSlot spell_slot)
            {
                spell_slot.Spell.Spellbook.Spend(spell_slot.Spell);

                /*if (!spell_slot.Spell.Spellbook.Blueprint.Spontaneous)
                {
                   
                    spell_slot.Spell.Spellbook.Spend(spell_slot.Spell);
                }
                else
                {
                    spell_slot.Spell.Spellbook.RestoreSpontaneousSlots(spell_slot.SpellLevel, -1);
                }*/
            }


            public bool hasSpellSlot(SpellSlot spell_slot)
            {
                if (!spell_slot.Spell.Spellbook.Blueprint.Spontaneous)
                {
                    return GetAvailableSpellSlot(spell_slot.Spell) != null;
                }
                else
                {
                    return spell_slot.Spell.Spellbook.GetSpontaneousSlots(spell_slot.SpellLevel) >= 1;
                }
            }


            public string GetReason()
            {
                return string.Empty;
            }


            [CanBeNull]
            protected SpellSlot GetAvailableSpellSlot(AbilityData ability)
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


            public AbilityData getSpellOrVariant(AbilityData ability)
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


            public BlueprintAbility getSpellOrVariantBlueprint(BlueprintAbility ability)
            {
                var variants = ability.GetComponent<AbilityVariants>();

                if (variants == null)
                {
                    return variant == 0 ? ability : null;
                }
                else
                {
                    return variants.Variants.Length > variant ? variants.Variants[variant] : null;
                }
            }
        }


        public class ApplySpellOnTarget : AbilityConvertSpell
        {

            public override void Apply(AbilityExecutionContext context, TargetWrapper target)
            {
                if (context.Ability.ParamSpellSlot == null || context.Ability.ParamSpellSlot.Spell == (AbilityData)null)
                    return;
                else if (context.Ability.ParamSpellSlot.Spell.Spellbook == null)
                    return;
                else
                {
                    applySpell(context.Ability.ParamSpellSlot, context, target);
                }
            }


            private void applySpell(SpellSlot spell_slot, AbilityExecutionContext context, TargetWrapper target)
            {
                var spell = getSpellOrVariant(spell_slot.Spell);
                spendSpellSlot(spell_slot);
                spell.SpendMaterialComponent();
                Rulebook.Trigger<RuleCastSpell>(new RuleCastSpell(spell, target));
            }
        }


        public class AbilityStoreSpellInFact : AbilityConvertSpell
        {
            public BlueprintUnitFact fact;
            public ActionList actions = null;

            public override void Apply(AbilityExecutionContext context, TargetWrapper target)
            {
                if (context.Ability.ParamSpellSlot == null || context.Ability.ParamSpellSlot.Spell == (AbilityData)null)
                    return;
                else if (context.Ability.ParamSpellSlot.Spell.Spellbook == null)
                    return;
                else
                {
                  storeSpell(context.Ability.ParamSpellSlot, context);

                    if (actions != null)
                    {
                      using (context.GetDataScope(target))
                        actions.Run();
                    }
                }
            }

            private void storeSpell(SpellSlot spell_slot, AbilityExecutionContext context)
            {
                var spell = getSpellOrVariant(spell_slot.Spell);

                FactStoreSpell.storeSpell(context.MaybeOwner.Descriptor, fact, spell);
                spendSpellSlot(spell_slot);
                spell.SpendMaterialComponent();
            }
        }


        public class ActivatableAbilitySpellStoredInFactRestriction : ActivatableAbilityRestriction
        {
            public BlueprintUnitFact fact;

            public override bool IsAvailable()
            {
                return FactStoreSpell.hasSpellStoredInFact(Owner, fact);
            }
        }


        [AllowedOn(typeof(BlueprintAbility))]
        [AllowMultipleComponents]
        public class AbilityCasterHasSpellStoredInFact : BlueprintComponent, IAbilityCasterChecker
        {
            public BlueprintUnitFact store_fact;

            public bool CorrectCaster(UnitEntityData caster)
            {
                return FactStoreSpell.hasSpellStoredInFact(caster.Descriptor, store_fact);
            }

            public string GetReason()
            {
                return "No spell stored";
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
                        if (ability.Blueprint.GetComponent<AbilityRestoreSpellSlot>() != null)
                            ___Conversion.Add(new AbilityData(ability)
                            {
                                ParamSpellSlot = spellSlot
                            });

                        var store_spell = ability.Blueprint.GetComponent<AbilityConvertSpell>();
                        if (store_spell!= null && store_spell.canBeUsedOn(spell))
                        {
                            ___Conversion.Add(new AbilityData(ability)
                            {
                                ParamSpellSlot = spellSlot
                            });
                        }
                    }
                }
                AbilityData paramSpell = (__instance.MechanicSlot as MechanicActionBarSlotSpontaneusSpell)?.Spell;
                if (paramSpell != null)
                {
                    SpellSlot paramSpellSlot = new SpellSlot(paramSpell.SpellLevel, SpellSlotType.Common, 0); //create fake slot for AbilityConvertSpell
                    paramSpellSlot.Spell = paramSpell;
                    foreach (Ability ability in spell.Caster.Abilities)
                    {
                        if (ability.Blueprint.GetComponent<AbilityRestoreSpontaneousSpell>() != null) //try with spontnaeous convert spell
                            ___Conversion.Add(new AbilityData(ability)
                            {
                                ParamSpellbook = paramSpell.Spellbook,
                                ParamSpellLevel = new int?(paramSpell.SpellLevel)
                            });

                    var store_spell = ability.Blueprint.GetComponent<AbilityConvertSpell>();
                        if (store_spell != null && store_spell.canBeUsedOn(spell))
                        {
                            ___Conversion.Add(new AbilityData(ability)
                            {
                                ParamSpellSlot = paramSpellSlot
                            });
                        }
                    }
                }
                BlueprintAbility spellBlueprint = spell.Blueprint;
                if (!___Conversion.Any<AbilityData>((Func<AbilityData, bool>)(s => s.Blueprint != spellBlueprint)) && (spellBlueprint.Variants == null || !(spellBlueprint.Variants).Any<BlueprintAbility>()) || ___ToggleAdditionalSpells == null)
                    return false;
                ___ToggleAdditionalSpells.gameObject.SetActive(true);
                return false;
            }
        }

        //update name of spell on store abilities
        [Harmony12.HarmonyPatch(typeof(DescriptionTemplatesAbility))]
        [Harmony12.HarmonyPatch("AbilityDataHeader", Harmony12.MethodType.Normal)]
        class DescriptionTemplatesAbility__AbilityDataHeader__Patch
        {
            static bool Prefix(DescriptionTemplatesAbility __instance,  bool isTooltip, DescriptionBricksBox box, AbilityData abilityData)
            {
                if (abilityData.ParamSpellSlot == null || abilityData.Blueprint.GetComponent<AbilityStoreSpellInFact>() == null)
                {
                    return true;
                }

                string name = abilityData.Blueprint.Name;
                var c = abilityData.Blueprint.GetComponent<AbilityConvertSpell>().getSpellOrVariantBlueprint(abilityData.ParamSpellSlot.Spell.Blueprint);
                if (c != null)
                {
                    name += $" ({c.Name})";
                }

                DescriptionBrick descriptionBrick = DescriptionBuilder.Templates.IconNameHeader(box, name, abilityData.Blueprint.Icon, isTooltip);
                string text1 = LocalizedTexts.Instance.AbilityTypes.GetText(abilityData.Blueprint.Type);
                string text2 = abilityData.Blueprint.School == SpellSchool.None ? string.Empty : LocalizedTexts.Instance.SpellSchoolNames.GetText(abilityData.Blueprint.School);
                string text3 = abilityData.Blueprint.School == SpellSchool.None ? string.Empty : (string)UIStrings.Instance.SpellBookTexts.Level + ": " + (object)abilityData.SpellLevel;
                descriptionBrick.SetText(text1, 1);
                descriptionBrick.SetText(text2, 2);
                if (abilityData.Blueprint.Type != AbilityType.Spell)
                    return false;
                descriptionBrick.SetText(text3, 3);

                return false;
            }
        }

        //update name of spell on store buffs
        [Harmony12.HarmonyPatch(typeof(DescriptionBuilder))]
        [Harmony12.HarmonyPatch("Buff", Harmony12.MethodType.Normal)]
        class DescriptionBuilder__Buff__Patch
        {
            static bool Prefix(TooltipData data, DescriptionBody body, bool isTooltip)
            {
                if (data.Buff != null)
                {
                    var stored_spell_caption = data.Buff.Blueprint.GetComponent<AddStoredSpellToCaption>();
                    if (stored_spell_caption != null)
                    {
                        string spell_name = "";
                        data.Buff.CallComponents<AddStoredSpellToCaption>(c => c.getStoredSpellName(out spell_name));
                        if (!spell_name.Empty())
                        {
                            spell_name = $" ({spell_name})";
                        }
                        
                        DescriptionBuilder.Templates.IconNameHeader(body.HeaderBox, data.Buff.Name + spell_name, data.Buff.Icon, isTooltip);
                    }
                    else
                    {
                        DescriptionBuilder.Templates.IconNameHeader(body.HeaderBox, data.Buff.Name, data.Buff.Icon, isTooltip);
                    }
                    if (isTooltip)
                        DescriptionBuilder.Templates.Timer(body.ContentBox, data.Buff);
                    DescriptionBuilder.Templates.ParagraphDescription(body.ContentBox, data.Buff.Description);
                    return false;
                }
                else
                    return true;
            }
        }
    }
}
