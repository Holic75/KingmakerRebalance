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
        public class UnitPartNoSpontnaeousMetamagicCastingTimeIncrease : AdditiveUnitPart
        {
            public bool canBeUsedOnAbility(AbilityData ability)
            {
                if (buffs.Empty())
                {
                    return false;
                }

                foreach (var b in buffs)
                {
                    bool result = false;
                    //Main.logger.Log(b.Name);
                    b.CallComponents<INoSpontnaeousMetamagicCastingTimeIncrease>(c => result = c.canUseOnAbility(ability));
                    if (result)
                    {
                        return true;
                    }
                }

                return false;
            }
        }


        public class UnitPartArcanistPreparedMetamagic : AdditiveUnitPart
        {
            [JsonProperty]
            public BlueprintSpellbook spellbook = Arcanist.arcanist_spellbook;
            [JsonProperty]
            private Dictionary<string, List<Metamagic>> prepared_spells_with_metamagic = new Dictionary<string, List<Metamagic>>();

            private int metamixing = 0;
            public void add(BlueprintAbility ability, Metamagic metamagic)
            {
                if (!prepared_spells_with_metamagic.ContainsKey(ability.AssetGuid))
                {
                    prepared_spells_with_metamagic.Add(ability.AssetGuid, new List<Metamagic>());
                }
                prepared_spells_with_metamagic[ability.AssetGuid].Add(metamagic);
            }

            public void clear()
            {
                prepared_spells_with_metamagic.Clear();
            }


            public bool metamixingEnabled()
            {
                return metamixing > 0;
            }

            public void enableMetamixing()
            {
                metamixing++;
            }

            public void disableMetamaixing()
            {
                metamixing--;
            }

            public void remove(BlueprintAbility blueprint, Metamagic metamagic)
            {
                if (!prepared_spells_with_metamagic.ContainsKey(blueprint.AssetGuid))
                {
                    return;
                }
                prepared_spells_with_metamagic[blueprint.AssetGuid].RemoveAll(m => m == metamagic);
                if (prepared_spells_with_metamagic[blueprint.AssetGuid].Empty())
                {
                    prepared_spells_with_metamagic.Remove(blueprint.AssetGuid);
                }
            }

            public bool noCastingTimeIncreaseForMetamagic(BlueprintAbility ability, Metamagic metamagic, int additional_free_metamagic = 0)
            {
 
                bool is_ok = noCastingTimeIncreaseForMetamagicInternal(ability, metamagic, additional_free_metamagic);

                if (ability.Parent != null)
                {
                    is_ok = is_ok || noCastingTimeIncreaseForMetamagicInternal(ability.Parent, metamagic, additional_free_metamagic);
                }
                return is_ok;
            }


            public bool isUsedWithMetamixing(BlueprintAbility ability, Metamagic metamagic)
            {
                if (!metamixingEnabled())
                {
                    return false;
                }

                if (ability.Parent != null)
                {
                    return prepared_spells_with_metamagic[ability.Parent.AssetGuid].Any(m => (m | metamagic) == metamagic && Helpers.PopulationCount((int)(metamagic & ~m)) == 1)
                            && !prepared_spells_with_metamagic[ability.Parent.AssetGuid].Any(m => (m | metamagic) == metamagic && Helpers.PopulationCount((int)(metamagic & ~m)) == 0);
                }
                else
                {
                    return prepared_spells_with_metamagic[ability.AssetGuid].Any(m => (m | metamagic) == metamagic && Helpers.PopulationCount((int)(metamagic & ~m)) == 1)
                           && !prepared_spells_with_metamagic[ability.AssetGuid].Any(m => (m | metamagic) == metamagic && Helpers.PopulationCount((int)(metamagic & ~m)) == 0);
                }
                
            }


            private bool noCastingTimeIncreaseForMetamagicInternal(BlueprintAbility ability, Metamagic metamagic, int additional_free_metamagic)
            {
                if (!prepared_spells_with_metamagic.ContainsKey(ability.AssetGuid))
                {
                    return false;
                }

                int free_metamagics = additional_free_metamagic;
                if (metamixingEnabled())
                {
                    free_metamagics++;
                }

                return prepared_spells_with_metamagic[ability.AssetGuid].Any(m => (m | metamagic) == metamagic && Helpers.PopulationCount((int)(metamagic & ~m)) <= free_metamagics);
            }


            public bool authorisedMetamagic(BlueprintAbility ability, Metamagic metamagic)
            {
                //check that there exist at least one prepared spell with metamagic mask which is subset of this one
                if (!prepared_spells_with_metamagic.ContainsKey(ability.AssetGuid))
                {
                    return false;
                }

                return prepared_spells_with_metamagic[ability.AssetGuid].Any(m => (metamagic | m) == metamagic);
            }


            public void refreshSpellLevel(int level)
            {
                var active_spellbook = this.Owner.GetSpellbook(spellbook);
                if (active_spellbook == null)
                {
                    return;
                }

                var memorization_spellbook_blueprint = active_spellbook.Blueprint.GetComponent<SpellbookMechanics.GetKnownSpellsFromMemorizationSpellbook>()?.spellbook;
                if (memorization_spellbook_blueprint == null)
                {
                    return;
                }
                var memorization_spellbook = this.Owner.Spellbooks.Where(s => s.Blueprint == memorization_spellbook_blueprint).FirstOrDefault();

                if (memorization_spellbook == null)
                {
                    return;
                }

                var m_KnownSpells = Helpers.GetField<List<AbilityData>[]>(active_spellbook, "m_KnownSpells");
                var m_customSpells = Helpers.GetField<List<AbilityData>[]>(active_spellbook, "m_CustomSpells");
                Dictionary<BlueprintAbility, int> m_KnownSpellLevels = Helpers.GetField<Dictionary<BlueprintAbility, int>>(active_spellbook, "m_KnownSpellLevels");
                var m_MemorizedSpells = Helpers.GetField<List<SpellSlot>[]>(memorization_spellbook, "m_MemorizedSpells");
                foreach (var known_spell in active_spellbook.GetKnownSpells(level).ToArray())
                {
                    m_KnownSpells[level].Remove(known_spell);
                    remove(known_spell.Blueprint, known_spell.MetamagicData?.MetamagicMask ?? (Metamagic)0);
                    EventBus.RaiseEvent<ISpellBookCustomSpell>((Action<ISpellBookCustomSpell>)(h => h.RemoveSpellHandler(known_spell)));
                    if (!prepared_spells_with_metamagic.ContainsKey(known_spell.Blueprint.AssetGuid))
                    {
                        m_KnownSpellLevels.Remove(known_spell.Blueprint);
                    }
                }
              
                foreach (var slot in m_MemorizedSpells[level].ToArray())
                {
                    if (slot.Spell != null)
                    {
                        var new_ability = new AbilityData(slot.Spell.Blueprint, active_spellbook);
                        if (slot.Spell.MetamagicData != null)
                        {
                            new_ability.MetamagicData = slot.Spell.MetamagicData.Clone();
                            this.add(new_ability.Blueprint, new_ability.MetamagicData.MetamagicMask);
                        }
                        else
                        {
                           this.add(new_ability.Blueprint, (Metamagic)0);
                        }
                        var spell_level = memorization_spellbook.GetSpellLevel(slot.Spell.Blueprint);
                        new_ability.DecorationBorderNumber = slot.Spell.DecorationBorderNumber;
                        new_ability.DecorationColorNumber = slot.Spell.DecorationColorNumber;

                        m_KnownSpells[slot.Spell.SpellLevel].Add(new_ability);
                        m_KnownSpellLevels[slot.Spell.Blueprint] = spell_level;
                        EventBus.RaiseEvent<ISpellBookCustomSpell>((Action<ISpellBookCustomSpell>)(h => h.AddSpellHandler(new_ability)));
                    }
                }


                for (int lvl = 0; lvl <= 9; lvl++)
                {
                    foreach (var s in m_customSpells[lvl].ToArray())
                    {                       
                        if (!authorisedMetamagic(s.Blueprint, s.MetamagicData?.MetamagicMask ?? (Metamagic)0))
                        {
                            m_customSpells[lvl].Remove(s);
                            EventBus.RaiseEvent<ISpellBookCustomSpell>((Action<ISpellBookCustomSpell>)(h => h.RemoveSpellHandler(s)));
                        }
                    }
                }
            }
        }


        public class RefreshArcanistSpellLevel: ContextAction
        {
            public int spell_level;

            public override string GetCaption()
            {
                return "Refresh Arcanist Spell Level";
            }

            public override void RunAction()
            {
                if (spell_level <1 || spell_level >9)
                {
                    return;
                }

                this.Context?.MaybeCaster?.Ensure<UnitPartArcanistPreparedMetamagic>().refreshSpellLevel(spell_level);
            }
        }


        public interface INoSpontnaeousMetamagicCastingTimeIncrease
        {
            bool canUseOnAbility(AbilityData ability);
        }



        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ArcanistPreparedMetamagicNoSpellCastingTimeIncrease : OwnedGameLogicComponent<UnitDescriptor>, INoSpontnaeousMetamagicCastingTimeIncrease
        {
            public override void OnTurnOn()
            {
                this.Owner.Ensure<UnitPartNoSpontnaeousMetamagicCastingTimeIncrease>().addBuff(this.Fact);
            }

            public override void OnTurnOff()
            {
                this.Owner.Ensure<UnitPartNoSpontnaeousMetamagicCastingTimeIncrease>().removeBuff(this.Fact);
            }

            public bool canUseOnAbility(AbilityData ability)
            {
                var arcanist_part = this.Owner.Ensure<UnitPartArcanistPreparedMetamagic>();
                if (arcanist_part == null)
                {
                    return false;
                }

                if (ability.MetamagicData == null)
                {
                    return false;
                }

                if (ability.Spellbook.Blueprint != arcanist_part.spellbook)
                {
                    return false;
                }
                return this.Owner.Ensure<UnitPartArcanistPreparedMetamagic>().noCastingTimeIncreaseForMetamagic(ability.Blueprint, ability.MetamagicData.MetamagicMask & ~(Metamagic)MetamagicFeats.MetamagicExtender.FreeMetamagic);
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class InitializeArcanistPart : OwnedGameLogicComponent<UnitDescriptor>
        {
            public BlueprintSpellbook spellbook;
            public override void OnTurnOn()
            {
                this.Owner.Ensure<UnitPartArcanistPreparedMetamagic>().spellbook = spellbook;
            }

            public override void OnTurnOff()
            {

            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class Metamixing : OwnedGameLogicComponent<UnitDescriptor>
        {
            public override void OnTurnOn()
            {
                this.Owner.Ensure<UnitPartArcanistPreparedMetamagic>().enableMetamixing();
            }

            public override void OnTurnOff()
            {
                this.Owner.Ensure<UnitPartArcanistPreparedMetamagic>().disableMetamaixing();
            }
        }



        [AllowedOn(typeof(BlueprintUnitFact))]
        public class NoSpontnaeousMetamagicCastingTimeIncreaseIfLessMetamagic : OwnedGameLogicComponent<UnitDescriptor>, INoSpontnaeousMetamagicCastingTimeIncrease
        {
            public int max_metamagics = 1;

            public override void OnTurnOn()
            {
                this.Owner.Ensure<UnitPartNoSpontnaeousMetamagicCastingTimeIncrease>().addBuff(this.Fact);
            }

            public override void OnTurnOff()
            {
                this.Owner.Ensure<UnitPartNoSpontnaeousMetamagicCastingTimeIncrease>().removeBuff(this.Fact);
            }

            public bool canUseOnAbility(AbilityData ability)
            {
                if (ability.MetamagicData == null)
                {
                    return false;
                }
                int metamagic_count = Helpers.PopulationCount((int)(ability.MetamagicData.MetamagicMask & ~((Metamagic)MetamagicFeats.MetamagicExtender.FreeMetamagic)));
                if (metamagic_count <= max_metamagics)
                {
                    return true;
                }

                var arcanist_part = this.Owner.Get<UnitPartArcanistPreparedMetamagic>();
                if (arcanist_part != null && ability.Spellbook.Blueprint == arcanist_part.spellbook)
                {
                    return arcanist_part.noCastingTimeIncreaseForMetamagic(ability.Blueprint, ability.MetamagicData.MetamagicMask & ~(Metamagic)MetamagicFeats.MetamagicExtender.FreeMetamagic, max_metamagics);
                }
                return false;
            }
        }


        [AllowedOn(typeof(BlueprintParametrizedFeature))]
        public class NoSpontnaeousMetamagicCastingTimeIncreaseForSelectedSpell : ParametrizedFeatureComponent, INoSpontnaeousMetamagicCastingTimeIncrease
        {
            public int max_metamagics = 1;

            public override void OnTurnOn()
            {
                this.Owner.Ensure<UnitPartNoSpontnaeousMetamagicCastingTimeIncrease>().addBuff(this.Fact);
            }

            public override void OnTurnOff()
            {
                this.Owner.Ensure<UnitPartNoSpontnaeousMetamagicCastingTimeIncrease>().removeBuff(this.Fact);
            }

            public bool canUseOnAbility(AbilityData ability)
            {
                //Main.logger.Log("Checking Spell and Ability");
                var spell = this.Param?.Blueprint as BlueprintAbility;
               
                if (spell == null || ability?.Blueprint == null)
                {
                    return false;
                }

               // Main.logger.Log("Checking spellbook");
                if (!spell.IsSpell)
                {
                    return false;
                }

                if (ability.MetamagicData == null)
                {
                    return false;
                }

                // Main.logger.Log("Checking correct spell");
                var allowed = ability.Blueprint.Parent == null ? SpellDuplicates.isDuplicate(ability.Blueprint, spell) : SpellDuplicates.isDuplicate(ability.Blueprint.Parent, spell);
                if (!allowed)
                {
                    return false;
                }

                //Main.logger.Log("Checking metamagic");
                int metamagic_count = Helpers.PopulationCount((int)(ability.MetamagicData.MetamagicMask & ~((Metamagic)MetamagicFeats.MetamagicExtender.FreeMetamagic)));
                if (metamagic_count > max_metamagics)
                {
                    var arcanist_part = this.Owner.Get<UnitPartArcanistPreparedMetamagic>();
                    if (arcanist_part != null && ability.Spellbook.Blueprint == arcanist_part.spellbook)
                    {
                        return arcanist_part.noCastingTimeIncreaseForMetamagic(ability.Blueprint, ability.MetamagicData.MetamagicMask & ~(Metamagic)MetamagicFeats.MetamagicExtender.FreeMetamagic, max_metamagics);
                    }
                    return false;
                }

                return true;
            }
        }



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
                if (__result == false)
                {
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
                else if (!__instance.Blueprint.IsFullRoundAction)
                {
                    if (__instance.MetamagicData.MetamagicMask == 0)
                    {
                        return;
                    }


                    if ((__instance.MetamagicData.MetamagicMask | (Metamagic)MetamagicFeats.MetamagicExtender.FreeMetamagic) == (Metamagic)MetamagicFeats.MetamagicExtender.FreeMetamagic)
                    {
                        __result = false;
                        return;
                    }

                    var fast_metamagic = __instance.Caster.Get<UnitPartNoSpontnaeousMetamagicCastingTimeIncrease>();
                    if (fast_metamagic == null)
                    {
                        return;
                    }

                    if (!__instance.Blueprint.IsSpell)
                    {
                        return;
                    }


                    __result = !fast_metamagic.canBeUsedOnAbility(__instance);
                }
            }
        }


 

        public class FactStoreSpell : OwnedGameLogicComponent<UnitDescriptor>
        {
            public ActionList actions_on_store = new ActionList();
            public bool ignore_target_checkers = false;
            public bool always_hit = false;
            [JsonProperty]
            private AbilityData spell = null;



            public void releaseSpellOnTarget(TargetWrapper target)
            {
                if (spell != null && (spell.CanTarget(target) || ignore_target_checkers))
                {
                    var rule_cast_spell = new RuleCastSpell(spell, target);
                    rule_cast_spell.Context.AttackRoll = Rulebook.CurrentContext.AllEvents.LastOfType<RuleAttackWithWeapon>()?.AttackRoll;
                    if (always_hit)
                    {
                        rule_cast_spell.Context.ForceAlwaysHit = true;
                    }
                    Rulebook.Trigger<RuleCastSpell>(rule_cast_spell);
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
                var rule_cast_spell = new RuleCastSpell(spell, target);
                rule_cast_spell.Context.AttackRoll = Rulebook.CurrentContext.AllEvents.LastOfType<RuleAttackWithWeapon>()?.AttackRoll;
                Rulebook.Trigger<RuleCastSpell>(rule_cast_spell);
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



        
        [AllowedOn(typeof(Kingmaker.Blueprints.Facts.BlueprintUnitFact))]
        public class MagicalSupremacy : RuleInitiatorLogicComponent<RuleCastSpell>, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>
        {
            public int bonus;
            public BlueprintAbilityResource resource;
            private BlueprintAbility current_spell = null;
            private int spell_level = -1;

            
            public override void OnEventAboutToTrigger(RuleCastSpell evt)
            {
                if (evt.Spell.SourceItem != null || evt.Spell.Blueprint != current_spell)
                {
                    spell_level = -1;
                    return;
                }
            }


            public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                spell_level = -1;
                int cost = 1 + evt.SpellLevel;
                if (this.resource == null || this.Owner.Resources.GetResourceAmount((BlueprintScriptableObject)this.resource) < cost)
                {
                    return;
                }
                if (evt.Spell == null || evt.Spellbook == null || evt.Spell.Type != AbilityType.Spell)
                {
                    return;
                }
                evt.AddBonusCasterLevel(bonus);
                evt.AddBonusDC(bonus);
                current_spell = evt.Spell;
                spell_level = evt.SpellLevel;
            }

            public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {

            }

            public override void OnEventDidTrigger(RuleCastSpell evt)
            {
                if (spell_level == -1)
                {
                    return;
                }
                if (evt.Spell.Blueprint != current_spell)
                {
                    return;
                }
                this.Owner.Resources.Spend(this.resource, spell_level + 1);
                /*if (evt.Spell.Spellbook.Blueprint.Spontaneous)
                {
                    evt.Spell.Spellbook.RestoreSpontaneousSlots(spell_level, 1);
                }*/
                evt.Spell.Caster.Ensure<SpellbookMechanics.UnitPartDoNotSpendNextSpell>().active = true;
                current_spell = null;
                spell_level = -1;
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


        public class ReplaceDCWithCasterLevelCheckForSchool : OwnedGameLogicComponent<UnitDescriptor>, MetamagicFeats.IRuleSavingThrowTriggered
        {
            public SpellSchool school;
            public SavingThrowType save_type;

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

                if (context.SpellSchool == school 
                     && (save_type == SavingThrowType.Unknown || evt.Type == save_type))
                {
                    var cl_check = RulebookEvent.Dice.D(new DiceFormula(1, DiceType.D20)) + evt.Reason.Context.Params.CasterLevel;
                    if (cl_check > evt.DifficultyClass)
                    {
                        Common.AddBattleLogMessage("Changing spell DC for "  + evt.Initiator.CharacterName + $" form {evt.DifficultyClass} to {cl_check} due to {this.Fact.Name}");
                        Helpers.SetField(evt, "DifficultyClass", cl_check);
                    }
                }
            }
        }


        public class ModifierBonusForSchool : OwnedGameLogicComponent<UnitDescriptor>, IOnModifierAdd
        {
            public SpellSchool school;
            public ModifierDescriptor descriptor;
            public ContextValue bonus;

            public void onModifierAdd(ModifiableValue.Modifier modifier)
            {
                var spellContext = Helpers.GetMechanicsContext()?.SourceAbilityContext;
                if (spellContext == null)
                {
                    return;
                }

                if (spellContext.MaybeCaster != this.Owner.Unit)
                {
                    return;
                }

                if (school != SpellSchool.None && spellContext.SpellSchool != school)
                {
                    return;
                }

                if (descriptor != ModifierDescriptor.None && descriptor != modifier.ModDescriptor)
                {
                    return;
                }

                modifier.ModValue += bonus.Calculate(this.Fact.MaybeContext);
            }


        }


        public interface IOnModifierAdd : IGlobalSubscriber
        {
            void onModifierAdd(ModifiableValue.Modifier modifier);
        }




        [Harmony12.HarmonyPatch(typeof(ModifiableValue))]
        [Harmony12.HarmonyPatch("AddModifier", Harmony12.MethodType.Normal)]
        [Harmony12.HarmonyPatch(new Type[] {typeof(ModifiableValue.Modifier) })]
        static class ModifiableValue_AddModifier_Patch
        {
            internal static bool Prefix(ModifiableValue __instance , ModifiableValue.Modifier mod)
            {
                EventBus.RaiseEvent<IOnModifierAdd>((Action<IOnModifierAdd>)(h => h.onModifierAdd(mod)));

                return true;
            }
        }
    }
}
