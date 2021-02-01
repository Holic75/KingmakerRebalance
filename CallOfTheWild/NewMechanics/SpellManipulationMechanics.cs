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
using Kingmaker.UnitLogic.FactLogic;

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


        public class UnitPartArcanistPreparedMetamagic : UnitPart
        {
            [JsonProperty]
            public BlueprintSpellbook spellbook = Arcanist.arcanist_spellbook;
            [JsonProperty]
            private Dictionary<string, List<Metamagic>> prepared_spells_with_metamagic = new Dictionary<string, List<Metamagic>>();
            [JsonProperty]
            private Dictionary<string, int> extra_spells_level_map = new Dictionary<string, int>();


            public bool isExtraSpell(BlueprintAbility spell)
            {
                if (spell.Parent != null)
                {
                    return extra_spells_level_map.ContainsKey(spell.Parent.AssetGuid);
                }
                return extra_spells_level_map.ContainsKey(spell.AssetGuid);
            }


            public void addExtraSpell(BlueprintAbility ability, int level)
            {
                extra_spells_level_map.Add(ability.AssetGuid, level);
                this.Owner.DemandSpellbook(spellbook).AddKnown(level, ability);
            }


            public void removeExtraSpell(BlueprintAbility ability)
            {
                extra_spells_level_map.Remove(ability.AssetGuid);
            }



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
                    slot.Available = false;
                }


                foreach (var kv in extra_spells_level_map)
                {
                    if (kv.Value == level)
                    {
                        var new_ability = new AbilityData(Main.library.Get<BlueprintAbility>(kv.Key), active_spellbook);
                        this.add(new_ability.Blueprint, (Metamagic)0);
                        m_KnownSpells[kv.Value].Add(new_ability);
                        m_KnownSpellLevels[new_ability.Blueprint] = kv.Value;
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
            public override void OnFactActivate()
            {
                this.Owner.Ensure<UnitPartArcanistPreparedMetamagic>().spellbook = spellbook;
            }

            public override void OnFactDeactivate()
            {
                this.Owner.Remove<UnitPartArcanistPreparedMetamagic>();
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
                var stored_buff = Owner.GetFact(store_fact);

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


        [AllowedOn(typeof(BlueprintAbility))]
        public class WishSpell : BlueprintComponent
        {

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
                Main.TraceLog();
                if (__result == false)
                {
                    if (__instance.Blueprint.GetComponent<InferIsFullRoundFromParamSpellSlot>() != null
                        && __instance.ParamSpellSlot?.Spell != null)
                    {
                        __result = __instance.ParamSpellSlot.Spell.RequireFullRoundAction;
                        return;
                    }

                    if (__instance.ActionType == UnitCommand.CommandType.Standard
                        && __instance.ConvertedFrom != null
                        && __instance.MetamagicData != null
                        && __instance.MetamagicData.NotEmpty) 
                    {

                        __result = (__instance.ConvertedFrom.Blueprint?.StickyTouch?.TouchDeliveryAbility != __instance.Blueprint);//to avoid issues with touch spells that write their generating spells to ConvertedFrom
                    }
                }

                if (!__instance.Blueprint.IsFullRoundAction && __result)
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
            public bool do_not_clear_spell_after_release;
            [JsonProperty]
            private AbilityData spell = null;
            [JsonProperty]
            private AbilityData base_spell = null;

            public void releaseSpellOnTarget(TargetWrapper target)
            {
                var check_spell = base_spell == null ? spell : base_spell;

                if (check_spell != null && (check_spell.CanTarget(target) || ignore_target_checkers))
                {
                    var rule_cast_spell = new RuleCastSpell(spell, target);
                    rule_cast_spell.Context.AttackRoll = Rulebook.CurrentContext.AllEvents.LastOfType<RuleAttackWithWeapon>()?.AttackRoll;
                    if (always_hit)
                    {
                        rule_cast_spell.Context.ForceAlwaysHit = true;
                    }
                    Rulebook.Trigger<RuleCastSpell>(rule_cast_spell);
                    Common.AddBattleLogMessage($"{this.Owner.CharacterName} released {spell.Blueprint.Name} from {this.Fact.Name}.");
                    if (!do_not_clear_spell_after_release)
                    {
                        clearSpell();
                    }
                }
            }


            public void clearSpell()
            {
                spell = null;
                base_spell = null;
            }

            public void storeSpell(AbilityData new_spell)
            {
                spell = new_spell;
                base_spell = new_spell;
                Common.AddBattleLogMessage($"{this.Fact.MaybeContext.MaybeOwner.CharacterName} stored {spell.Blueprint.Name} in {this.Fact.Name}.");
                (this.Fact as IFactContextOwner)?.RunActionInContext(this.actions_on_store, this.Owner.Unit);
            }


            public void storeSpellTouch(AbilityData new_spell, AbilityData cast_spell)
            {
                spell = new_spell;
                base_spell = cast_spell;
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

                var stored_buff = unit.GetFact(fact);
           
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
                var store_buff = unit.GetFact(fact);

                if (store_buff != null)
                {
                    var sticky_touch = spell.Blueprint.GetComponent<AbilityEffectStickyTouch>();
                    if (sticky_touch != null)
                    {
                        var touch_spell = new AbilityData(spell, sticky_touch.TouchDeliveryAbility);
                        store_buff.CallComponents<FactStoreSpell>(c => c.storeSpellTouch(touch_spell, spell));
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
                var stored_buff = Context.MaybeOwner.Descriptor.GetFact(fact);


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


        public class RunActionOnTargetBasedOnSpellType : AbilityConvertSpell
        {
            public ActionList action_divine = Helpers.CreateActionList();
            public ActionList action_arcane = Helpers.CreateActionList();
            public ActionList action_alchemist = Helpers.CreateActionList();
            public ActionList action_psychic = Helpers.CreateActionList();
            public BlueprintUnitFact context_fact;

            public override void Apply(AbilityExecutionContext context, TargetWrapper target)
            {
                if (context.Ability.ParamSpellSlot == null || context.Ability.ParamSpellSlot.Spell == (AbilityData)null)
                    return;
                else if (context.Ability.ParamSpellSlot.Spell.Spellbook == null)
                    return;
                else
                {
                    applyEffect(context.Ability.ParamSpellSlot, context, target);
                }
            }


            private void applyEffect(SpellSlot spell_slot, AbilityExecutionContext context, TargetWrapper target)
            {
                spendSpellSlot(spell_slot);
                var fact = context?.Caster?.Descriptor.GetFact(context_fact);
                if (context_fact == null)
                {
                    return;
                }
                if (context.Ability.ParamSpellSlot.Spell.Spellbook.Blueprint.IsArcane)
                {
                     (fact as IFactContextOwner).RunActionInContext(action_arcane, target);
                }
                else if (context.Ability.ParamSpellSlot.Spell.Spellbook.Blueprint.IsAlchemist)
                {
                    (fact as IFactContextOwner).RunActionInContext(action_alchemist, target);
                }
                else if (context.Ability.ParamSpellSlot.Spell.Spellbook.Blueprint.GetComponent<SpellbookMechanics.PsychicSpellbook>() != null)
                {
                    (fact as IFactContextOwner).RunActionInContext(action_psychic, target);
                }
                else
                {
                    (fact as IFactContextOwner).RunActionInContext(action_divine, target);
                }
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class IncreaseSpellTypeDC : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            public bool apply_to_arcane = false;
            public bool apply_to_divine = false;
            public bool apply_to_alchemist = false;
            public bool apply_to_psychic = false;
            public ContextValue bonus;
            public bool remove_after_use;
            public int max_lvl = 100;

            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                if (evt.Spellbook == null)
                {
                    return;
                }
                if (evt.AbilityData.SpellLevel > max_lvl)
                {
                    return;
                }
                if (evt.Spellbook.Blueprint.IsArcane)
                {
                    if (apply_to_arcane)
                    {
                        apply(evt);
                    }
                }
                else if (evt.Spellbook.Blueprint.IsAlchemist)
                {
                    if (apply_to_alchemist)
                    {
                        apply(evt);
                    }
                }
                else if (evt.Spellbook.Blueprint.GetComponent<SpellbookMechanics.PsychicSpellbook>() != null)
                {
                    if (apply_to_psychic)
                    {
                        apply(evt);
                    }
                }
                else if (apply_to_divine)
                {
                    apply(evt);
                }
            }

            private void apply(RuleCalculateAbilityParams evt)
            {
                evt.AddBonusDC(this.bonus.Calculate(this.Fact.MaybeContext));
                if (remove_after_use)
                {
                    (this.Fact as Buff)?.Remove();
                }
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }

        [ComponentName("Increase spell descriptor CL")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class IncreaseSpellSchoolCasterLevel : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            public ContextValue bonus;
            public SpellSchool school;
            public BlueprintAbility[] extra_spells = new BlueprintAbility[0];

            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                if (evt.Spell == null)
                {
                    return;
                }
                if ((evt.Spellbook == null || evt.Spell.School != school) && !extra_spells.Contains(evt.Spell))
                {
                    return;
                }
                evt.AddBonusCasterLevel(this.bonus.Calculate(this.Fact.MaybeContext));
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }


        [ComponentName("Increase spell descriptor CL")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class IncreaseSpellTypeCasterLevel : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            public bool apply_to_arcane = false;
            public bool apply_to_divine = false;
            public bool apply_to_alchemist = false;
            public bool apply_to_psychic = false;
            public ContextValue bonus;
            public bool remove_after_use;
            public int max_lvl = 100;

            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                if (evt.Spellbook == null)
                {
                    return;
                }
                if (evt.AbilityData.SpellLevel > max_lvl)
                {
                    return;
                }
                if (evt.Spellbook.Blueprint.IsArcane)
                {
                    if (apply_to_arcane)
                    {
                        apply(evt);
                    }
                }
                else if (evt.Spellbook.Blueprint.IsAlchemist)
                {
                    if (apply_to_alchemist)
                    {
                        apply(evt);
                    }
                }
                else if (evt.Spellbook.Blueprint.GetComponent<SpellbookMechanics.PsychicSpellbook>() != null)
                {
                    if (apply_to_psychic)
                    {
                        apply(evt);
                    }
                }
                else if (apply_to_divine)
                {
                    apply(evt);
                }
            }

            private void apply(RuleCalculateAbilityParams evt)
            {
                evt.AddBonusCasterLevel(this.bonus.Calculate(this.Fact.MaybeContext));
                if (remove_after_use)
                {
                    (this.Fact as Buff)?.Remove();
                }
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
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



        public class AbilityStoreInFact : ContextAction
        {
            public BlueprintUnitFact fact;
            public BlueprintAbility ability;

            public override string GetCaption()
            {
                return "Store ability in fact";
            }

            public override void RunAction()
            {
                var data = new AbilityData(ability, this.Context.MaybeCaster?.Descriptor);
                FactStoreSpell.storeSpell(this.Context.MaybeCaster?.Descriptor, fact, data);
                data.SpendMaterialComponent();
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
        public class SpendLowerLevelSpellSlot : RuleInitiatorLogicComponent<RuleCastSpell>, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>
        {
            public BlueprintAbilityResource resource;
            public int amount;
            public int spell_slot_decrease = 0;
            public bool undercast_only;

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

                if (undercast_only)
                {
                    var comp = evt.Spell.GetComponent<SpellbookMechanics.SpellUndercast>();
                    if (comp == null || comp.getRank() < spell_slot_decrease)
                    {
                        return;
                    }
                }
                if (evt.SpellLevel <= spell_slot_decrease)
                {
                    return;
                }

                if (this.resource == null || this.Owner.Resources.GetResourceAmount((BlueprintScriptableObject)this.resource) < amount)
                {
                    return;
                }
                if (evt.Spell == null || evt.Spellbook == null || evt.Spell.Type != AbilityType.Spell)
                {
                    return;
                }

                if (evt.Spellbook.GetSpontaneousSlots(evt.SpellLevel - spell_slot_decrease) == 0)
                {
                    return;
                }

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
                this.Owner.Resources.Spend(this.resource, amount);

                evt.Spell.Caster.Ensure<SpellbookMechanics.UnitPartDoNotSpendNextSpell>().active = true;
                evt.Spell.Spellbook.RestoreSpontaneousSlots(spell_level - spell_slot_decrease, -1);
                current_spell = null;
                spell_level = -1;
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
                Main.TraceLog();
                Spellbook spellBook = spell.Spellbook;
                if (spellBook == null)
                    return false;
                ___Conversion = spellBook.GetSpontaneousConversionSpells(spell).EmptyIfNull<BlueprintAbility>().Select<BlueprintAbility, AbilityData>((Func<BlueprintAbility, AbilityData>)(b => new AbilityData(b, spellBook)
                {
                    ConvertedFrom = spell
                })).ToList<AbilityData>();
                if (Main.settings.metamagic_for_spontaneous_spell_conversion)
                {
                    ___Conversion.AddRange(getMetamagicSpontaneousConversionSpells(spell, spellBook));
                }
                
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


            static List<AbilityData> getMetamagicSpontaneousConversionSpells(AbilityData spell, Spellbook spellbook)
            {
                var spell_level = spellbook.GetSpellLevel(spell);
                var spells = new List<AbilityData>();
                var metamagics = spell.Caster.Progression.Features.Enumerable.Where(f => f.Blueprint.GetComponent<AddMetamagicFeat>() != null)
                    .Select(f => f.Blueprint.GetComponent<AddMetamagicFeat>().Metamagic).ToArray();

                for (int i = 1; i <= spell_level; i++)
                {
                    var conversion_spells = spellbook.GetSpontaneousConversionSpells(i).EmptyIfNull<BlueprintAbility>().ToArray();
                    //we need to find all possible metamagic combinations for every spell here
                    foreach (var s in conversion_spells)
                    {
                        spells.AddRange(getMetamagicCombinationForGivenLevel(spell_level, spellbook, spell, s, i, metamagics.Where(m => (s.AvailableMetamagic & m) != 0).ToArray()));
                    }

                }
                return spells;
            }


            static List<AbilityData> getMetamagicCombinationForGivenLevel(int target_level, Spellbook spellbook, AbilityData converted_spell, BlueprintAbility spell, int spell_level,
                                                                   Metamagic[] metamagics)
            {
                var spells = new List<AbilityData>();
                if (metamagics.Empty())
                {
                    return spells;
                }

                
                List<int> next_metamagic = new List<int>();
                List<Metamagic> selected_metamagics = new List<Metamagic>();
                next_metamagic.Add(0);
                while (true)
                {
                    while(!next_metamagic.Empty() && next_metamagic.Last() >= metamagics.Count())
                    {
                        next_metamagic.Remove(next_metamagic.Last());
                        if (!next_metamagic.Empty())
                        {
                            next_metamagic[next_metamagic.Count - 1]++;
                            selected_metamagics.RemoveAt(selected_metamagics.Count - 1);
                        }
                    }
                    if (next_metamagic.Empty())
                    {
                        break;
                    }
                    selected_metamagics.Add(metamagics[next_metamagic.Last()]);

                    //try to get metamagic data for required cost, return null if impossible
                    //if impossible we either have too much or too little metamagic,
                    //if too much remove last added metamagic, increase next_metamagic.last
                    //if too little continue adding more metamagics
                    //else add spell to conversion list
                    bool need_more;
                    var metamagic_data = getMetamagicDataForSpecifiedCost(selected_metamagics, spellbook, spell, target_level - spell_level, spell_level, out need_more);
                    if (metamagic_data == null && !need_more)
                    {
                        selected_metamagics.Remove(selected_metamagics.Last());
                        next_metamagic[next_metamagic.Count - 1]++;
                    }
                    else if (need_more)
                    {
                        next_metamagic.Add(next_metamagic.Last() + 1);
                    }
                    else
                    {
                        next_metamagic.Add(next_metamagic.Last() + 1);
                        spells.Add(new AbilityData(spell, spellbook)
                        {
                            ConvertedFrom = converted_spell,
                            MetamagicData = metamagic_data
                        });
                    }
                     
                }

                return spells;
            }


            static MetamagicData getMetamagicDataForSpecifiedCost(List<Metamagic> metamagics, Spellbook spellbook, BlueprintAbility spell, int required_cost, int expected_spell_level, out bool add_more)
            {
                add_more = false;
                //avoid more than one elemental metamagic
                if (metamagics.Count(m => (m & (Metamagic)MetamagicFeats.MetamagicExtender.Elemental) != 0) > 1)
                {
                    return null;
                }

                var sl = spellbook.GetSpellLevel(spell);
                if (sl < 0)
                {
                    sl = expected_spell_level;
                }

                int d_level = expected_spell_level - sl;
                
                int cost = 0;
                foreach (var m in metamagics)
                {
                    cost += m.DefaultCost();
                }

                
                for (int i = (metamagics.Contains(Metamagic.Heighten) ? 1 : 0); i < (metamagics.Contains(Metamagic.Heighten) ? 9 : 1); i++)
                {
                    if (cost + i + expected_spell_level > 9)
                    {
                        return null;
                    }
                    var rule_apply_metamagic = new RuleApplyMetamagic(spellbook, spell, metamagics, i);
                    rule_apply_metamagic.Result.SpellLevelCost += d_level;
                    var result = Rulebook.Trigger(rule_apply_metamagic).Result;

                    add_more = result.SpellLevelCost < required_cost + d_level;
                    if (result.SpellLevelCost == required_cost + d_level)
                    {
                        return result;
                    }
                    
                }
                return null;
            }
        }
       
        //update name of spell on store abilities
        [Harmony12.HarmonyPatch(typeof(DescriptionTemplatesAbility))]
        [Harmony12.HarmonyPatch("AbilityDataHeader", Harmony12.MethodType.Normal)]
        class DescriptionTemplatesAbility__AbilityDataHeader__Patch
        {
            static bool Prefix(DescriptionTemplatesAbility __instance,  bool isTooltip, DescriptionBricksBox box, AbilityData abilityData)
            {
                Main.TraceLog();
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
                Main.TraceLog();
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

            public void ruleSavingThrowBeforeTrigger(RuleSavingThrow evt)
            {

            }

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


        public class TargetDescriptorModifierBonus : OwnedGameLogicComponent<UnitDescriptor>, IOnModifierAdd
        {
            public ModifierDescriptor descriptor;
            public ContextValue bonus;


            public bool isCorrectModifier(ModifiableValue val, ModifiableValue.Modifier mod)
            {
                return  (val.Owner == this.Owner) && mod.ModDescriptor == descriptor || descriptor == ModifierDescriptor.None;
            }

            public override void OnTurnOn()
            {
                int val = bonus.Calculate(this.Fact.MaybeContext);
                var stats = this.Owner.Stats.GetList();
                /*List<Fact> sources = new List<Fact>();

                foreach (var s in stats)
                {                
                    foreach (var m in s.Modifiers)
                    {
                        if (isCorrectModifier(s, m) && m.Source != null && !sources.Contains(m.Source))
                        {
                            sources.Add(m.Source);
                        }
                    }
                }

                foreach (var s in sources)
                {
                    s.Recalculate();
                }*/

                foreach (var s in stats)
                {
                    var tr = Harmony12.Traverse.Create(s);
                    foreach (var m in s.Modifiers.ToArray())
                    {
                        if (isCorrectModifier(s, m))
                        {
                            if (m.Remove())
                            {
                                m.ModValue += val;
                                tr.Method("AddModifier", new Type[] { typeof(ModifiableValue.Modifier) }, new object[] { m }).GetValue<ModifiableValue.Modifier>();
                            }
                        }
                    }
                }
            }

            public void onModifierAdd(ModifiableValue value, ModifiableValue.Modifier modifier)
            {
                if (!isCorrectModifier(value, modifier))
                {
                    return;
                }
                modifier.ModValue += bonus.Calculate(this.Fact.MaybeContext);
            }

            public void onModifierRemove(ModifiableValue value, ModifiableValue.Modifier modifier)
            {
                /*if (isCorrectModifier(value, modifier))
                {
                   Main.logger.Log("Removing " + modifier.Source?.Name);
                }*/
            }
        }

        public class ModifierBonusForSchool : OwnedGameLogicComponent<UnitDescriptor>, IOnModifierAdd
        {
            public SpellSchool school;
            public ModifierDescriptor descriptor;
            public ContextValue bonus;

            public bool isCorrectModifier(ModifiableValue val, ModifiableValue.Modifier mod)
            {
                var context = mod.Source?.MaybeContext;
                if (context == null)
                {
                    return false;
                }

                return context.MaybeCaster == this.Owner?.Unit
                       && (school == SpellSchool.None || context.SpellSchool == school)
                       && (mod.ModDescriptor == descriptor || descriptor == ModifierDescriptor.None);
            }

            public override void OnTurnOn()
            {
                int val = bonus.Calculate(this.Fact.MaybeContext);

                //we will need to scan all units that may have buffs from caster
                var units = Game.Instance.State.Units;

                foreach (var u in units)
                {
                    var stats = u.Stats.GetList();
                    foreach (var s in stats)
                    {
                        var tr = Harmony12.Traverse.Create(s);
                        foreach (var m in s.Modifiers.ToArray())
                        {
                            if (isCorrectModifier(s, m))
                            {
                                if (m.Remove())
                                {
                                    m.ModValue += val;
                                    tr.Method("AddModifier", new Type[] { typeof(ModifiableValue.Modifier) }, new object[] { m }).GetValue<ModifiableValue.Modifier>();
                                }
                            }
                        }
                    }
                }              
            }


            public void onModifierAdd(ModifiableValue value, ModifiableValue.Modifier modifier)
            {
                if (!isCorrectModifier(value, modifier))
                {
                    return;
                }

                modifier.ModValue += bonus.Calculate(this.Fact.MaybeContext);
            }

            public void onModifierRemove(ModifiableValue value, ModifiableValue.Modifier modifier)
            {
                
            }
        }


        public interface IOnModifierAdd : IGlobalSubscriber
        {
            void onModifierAdd(ModifiableValue value, ModifiableValue.Modifier modifier);
            void onModifierRemove(ModifiableValue value, ModifiableValue.Modifier modifier);
        }


        [AllowedOn(typeof(BlueprintParametrizedFeature))]
        public class AddExtraArcanistSpellParametrized : ParametrizedFeatureComponent
        {
            public BlueprintSpellList spell_list;

            public override void OnFactActivate()
            {
                BlueprintAbility spell = !(this.Param != (FeatureParam)null) ? (BlueprintAbility)null : this.Param.Value.Blueprint as BlueprintAbility;
                if (spell == null)
                    return;

                var arcanist_part = this.Owner.Get<UnitPartArcanistPreparedMetamagic>();

                arcanist_part?.addExtraSpell(spell, this.spell_list.GetLevel(spell));
            }

            public override void OnFactDeactivate()
            {
                BlueprintAbility spell = !(this.Param != (FeatureParam)null) ? (BlueprintAbility)null : this.Param.Value.Blueprint as BlueprintAbility;
                if (spell == null)
                    return;

                var arcanist_part = this.Owner.Get<UnitPartArcanistPreparedMetamagic>();

                arcanist_part?.removeExtraSpell(spell);
            }
        }


        public class SpellRequiringResourceIfCastFromSpecificSpellbook : BlueprintComponent, IAbilityAvailabilityProvider
        {
            public BlueprintSpellbook spellbook;
            public bool arcanist_spellbook;
            public BlueprintAbilityResource resource;
            public int amount = 1;
            public bool half_level;
            public BlueprintUnitFact[] cost_increasing_facts;
            public bool only_from_extra_arcanist_spell_list;

            public string GetReason()
            {
                return "Insufficient resource";
            }

            public bool IsAvailableFor(AbilityData ability)
            {
                if (arcanist_spellbook)
                {
                    var arcanist_part = ability.Caster?.Get<UnitPartArcanistPreparedMetamagic>();
                    if (arcanist_part == null || arcanist_part.spellbook != ability.Spellbook?.Blueprint)
                    {
                        return true;
                    }
                    if (only_from_extra_arcanist_spell_list && !arcanist_part.isExtraSpell(ability.Blueprint))
                    {
                        return true;
                    }
                }
                else if (ability.Spellbook?.Blueprint != spellbook)
                {
                    return true;
                }



                int required_amount = calcualteCost(ability);

                return ability.Caster.Resources.GetResourceAmount(resource) >= required_amount;
            }


            private int calcualteCost(AbilityData ability)
            {
                int cost = half_level ? Math.Max(1, ability.SpellLevel / 2) : amount;

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

                if (arcanist_spellbook && ability.Caster.HasFact(Arcanist.magical_supremacy_buff))
                {
                    cost += ability.SpellLevel + 1;
                }

                return cost < 0 ? 0 : cost;
            }
        }



        [AllowedOn(typeof(BlueprintUnitFact))]
        public class DamageBoostAndReflection : BuffLogic, ITargetRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, IInitiatorRulebookSubscriber
        {
            public SpellDescriptorWrapper descriptor;
            public bool change_damage_type = false;
            public DamageEnergyType damage_type = DamageEnergyType.Holy;
            public bool reflect_damage = false;
            public bool change_reflect_damage_type = false;
            public DamageEnergyType reflect_damage_type = DamageEnergyType.Holy;
            public bool only_spells = true;
            public bool remove_self_on_apply = false;
            public bool only_from_caster = true;


            public void OnEventDidTrigger(RuleCalculateDamage evt)
            {
                var damage = evt.DamageBundle.FirstOrDefault();
                if (damage == null)
                {
                    return;
                }

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

                if (context2.MaybeCaster != this.Fact.MaybeContext.MaybeCaster && only_from_caster)
                {
                    return;
                }

                if ((context2.Ability.Blueprint.Type != AbilityType.Spell || (context2.SpellDescriptor & descriptor) == 0) && only_spells)
                {
                    return;
                }

                var new_damage = damage.Copy();
                if (change_damage_type)
                {
                    new_damage = new EnergyDamage(damage.Dice, damage_type);
                    new_damage.AddBonus(damage.Bonus);
                    new_damage.Half = true;
                }
                evt.DamageBundle.Add(new_damage);
                var calcualted_damage = evt.CalculatedDamage.FirstOrDefault();
                evt.CalculatedDamage.Add(new DamageValue(new_damage, calcualted_damage.ValueWithoutReduction/2, calcualted_damage.RolledValue/2));
                if (reflect_damage)
                {
                    var reflected_damage = damage.Copy();
                    if (change_reflect_damage_type)
                    {
                        reflected_damage = new EnergyDamage(new DiceFormula(calcualted_damage.ValueWithoutReduction/2, DiceType.One), reflect_damage_type);
                        reflected_damage.AddBonus(damage.Bonus);
                        reflected_damage.Half = false;

                        RuleDealDamage evt_dmg = new RuleDealDamage(evt.Initiator, evt.Initiator, new DamageBundle(reflected_damage));
                        Rulebook.Trigger<RuleDealDamage>(evt_dmg);
                    }
                }

                if (remove_self_on_apply)
                {
                    this.Buff.Remove();
                }
            }

            public void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {
            }
        }


        [AllowedOn(typeof(Kingmaker.Blueprints.Facts.BlueprintUnitFact))]
        public class SpendResourceOnExtraArcanistSpellCast : RuleInitiatorLogicComponent<RuleCastSpell>
        {
            public BlueprintAbilityResource resource;
            public int amount = 1;
            public bool half_level;


            public override void OnEventAboutToTrigger(RuleCastSpell evt)
            {

            }

            public override void OnEventDidTrigger(RuleCastSpell evt)
            {
                var spellbook_blueprint = evt.Spell?.Spellbook?.Blueprint;
                var arcanist_part = evt.Spell?.Caster?.Get<UnitPartArcanistPreparedMetamagic>();
                if (arcanist_part == null)
                {
                    return;
                }
                

                if (spellbook_blueprint != arcanist_part.spellbook)
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

                if (!arcanist_part.isExtraSpell(evt.Spell.Blueprint))
                {
                    return;
                }

                int cost = half_level ? Math.Max(1, evt.Spell.SpellLevel / 2) : amount;
                evt.Spell.Caster.Resources.Spend(resource, cost);
            }
        }



        public class ChangeElementalDamage : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCalculateDamage>, IInitiatorRulebookSubscriber, IRulebookHandler<RuleCalculateDamage>
        {
            public DamageEnergyType Element;
            public ContextValue max_level;

            public void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {
                AbilityData ability = evt.Reason.Ability;
                if (ability == (AbilityData)null || !(ability.Blueprint.IsSpell || ability.Blueprint.Type == AbilityType.Supernatural))
                    return;

                var caster_level = evt.Reason.Context?.Params?.CasterLevel;

                int max_level_value = max_level.Calculate(this.Fact.MaybeContext);
                if (max_level_value < caster_level.GetValueOrDefault())
                {
                    return;
                }
                foreach (BaseDamage baseDamage in evt.DamageBundle)
                    (baseDamage as EnergyDamage)?.ReplaceEnergy(this.Element);
            }

            public void OnEventDidTrigger(RuleCalculateDamage evt)
            {
            }

            public override void Validate(ValidationContext context)
            {
                base.Validate(context);
                if (this.Element == DamageEnergyType.Fire || this.Element == DamageEnergyType.Cold || (this.Element == DamageEnergyType.Acid || this.Element == DamageEnergyType.Electricity))
                    return;
                context.AddError("Only Fire, Cold, Acid or Electricity are allowed", (object[])Array.Empty<object>());
            }
        }


        public class GreaterSpellSpecialization : ParametrizedFeatureComponent
        {
            [JsonProperty]
            private Dictionary<BlueprintSpellbook, List<BlueprintAbility[]>> spellbook_spell_lists_map;

            public BlueprintFeature required_feat;

            public override void OnTurnOn()
            {
                enable();
            }

            private void enable()
            {
                if (!this.Owner.Progression.Features.HasFact(required_feat))
                {
                    return;
                }
                if (spellbook_spell_lists_map != null)
                {
                    return;
                }
                spellbook_spell_lists_map = new Dictionary<BlueprintSpellbook, List<BlueprintAbility[]>>();
                var param_spell = this.Param.Blueprint as BlueprintAbility;

                foreach (var sb in this.Owner.Spellbooks)
                {
                    var spell_lists = new List<BlueprintAbility[]>();
                    var spell_level = 1000;
                    BlueprintAbility spell = null;

                    foreach (var sd in SpellDuplicates.getDuplicates(param_spell))
                    {
                        var sl  = sb.GetSpellLevel(sd);
                        
                        if (sl != -1 && sl <spell_level)
                        {
                            spell = sd;
                            spell_level = sl;
                        }
                    }

                    if (spell_level == 1000)
                    {
                        continue;
                    }

                    var spells = new List<BlueprintAbility>();
                    if (!spell.HasVariants)
                    {
                        spells.Add(spell);
                    }
                    else
                    {
                        spells.AddRange(spell.Variants);
                    }

                    var spellbook = SpellbookMechanics.Helpers.getCastingSpellbook(sb, this.Owner);
                    if (sb == null)
                    {
                        return;
                    }

                    for (int i = 0; i < spells.Count; i++)
                    {
                        var spell_list = new BlueprintAbility[10];
                        spell_list[spell_level] = spells[i];
                        spell_lists.Add(spell_list);
                        spellbook?.AddSpellConversionList(spell_lists.Last());
                    }
                    spellbook_spell_lists_map.Add(spellbook.Blueprint, spell_lists);
                }
            }


            public void disable()
            {
                if (spellbook_spell_lists_map == null)
                {
                    return;
                }

                foreach (var kv in spellbook_spell_lists_map)
                {
                    var spellbook = this.Owner.GetSpellbook(kv.Key);
                    foreach (var sl in kv.Value)
                    {
                        spellbook?.RemoveSpellConversionList(sl);
                    }
                }
                spellbook_spell_lists_map = null;
            }

            public override void OnTurnOff()
            {
                disable();
            }
        }




        public class PreferredSpell : ParametrizedFeatureComponent
        {
            public BlueprintCharacterClass character_class;
            [JsonProperty]
            private List<BlueprintAbility[]> spell_lists;
            public override void OnTurnOn()
            {
                var spell_book = this.Owner.GetSpellbook(this.character_class);
                if (spell_book == null)
                {
                    return;
                }
                var spell = this.Param.Blueprint as BlueprintAbility;
                var spell_level = spell_book.GetSpellLevel(spell);

                var spells = new List<BlueprintAbility>();
                if (!spell.HasVariants)
                {
                    spells.Add(spell);
                }
                else
                {
                    spells.AddRange(spell.Variants);
                }
                spell_lists = new List<BlueprintAbility[]>();

                spell_book = SpellbookMechanics.Helpers.getCastingSpellbook(spell_book, this.Owner);
                for (int i = 0; i < spells.Count; i++)
                {
                    var spell_list = new BlueprintAbility[10];
                    spell_list[spell_level] = spells[i];
                    spell_lists.Add(spell_list);
                    spell_book?.AddSpellConversionList(spell_lists.Last());
                }
            }

            public override void OnTurnOff()
            {
                var spell_book = this.Owner.GetSpellbook(this.character_class);
                if (spell_book == null)
                {
                    return;
                }
                spell_book = SpellbookMechanics.Helpers.getCastingSpellbook(spell_book, this.Owner);
                if (spell_lists == null)
                {
                    return;
                }
                foreach (var sl in spell_lists)
                {
                    spell_book?.RemoveSpellConversionList(sl);
                }
            }
        }


        public class ExtraEffectOnSpellApplyTarget : OwnedGameLogicComponent<UnitDescriptor>, IApplyAbilityEffectHandler, IGlobalSubscriber
        {
            public ActionList actions;
            public bool check_caster;
            public SpellDescriptorWrapper descriptor;
            public BlueprintCharacterClass specific_class;
            public BlueprintSpellbook spellbook;

            public void OnAbilityEffectApplied(AbilityExecutionContext context)
            {

            }

            public void OnAbilityEffectAppliedToTarget(AbilityExecutionContext context, TargetWrapper target)
            {
                if (target.Unit?.Descriptor != this.Owner)
                {
                    return;
                }

                if (descriptor != SpellDescriptor.None && !context.SpellDescriptor.Intersects(descriptor))
                {
                    return;
                }

                if (check_caster && context.MaybeCaster != this.Fact.MaybeContext.MaybeCaster)
                {
                    return;
                }

                if (!Helpers.checkSpellbook(spellbook, specific_class, context.Ability?.Spellbook, context.MaybeCaster?.Descriptor))
                {
                    return;
                }

                (this.Fact as IFactContextOwner).RunActionInContext(actions, this.Owner.Unit);
            }

            public void OnTryToApplyAbilityEffect(AbilityExecutionContext context, TargetWrapper target)
            {
               
            }
        }


        public class ExtraEffectOnSpellApplyOnSpellCaster : OwnedGameLogicComponent<UnitDescriptor>, IApplyAbilityEffectHandler, IGlobalSubscriber
        {
            public ActionList actions;
            public SpellDescriptorWrapper descriptor;
            public bool only_enemies;

            public void OnAbilityEffectApplied(AbilityExecutionContext context)
            {

            }

            public void OnAbilityEffectAppliedToTarget(AbilityExecutionContext context, TargetWrapper target)
            {
                if (target.Unit?.Descriptor != this.Owner)
                {
                    return;
                }

                if (context.MaybeCaster == null)
                {
                    return;
                }
                if (descriptor != SpellDescriptor.None && !context.SpellDescriptor.Intersects(descriptor))
                {
                    return;
                }

                if (only_enemies && !this.Owner.Unit.IsEnemy(context.MaybeCaster))
                {
                    return;
                }
                (this.Fact as IFactContextOwner).RunActionInContext(actions, context.MaybeCaster);
            }

            public void OnTryToApplyAbilityEffect(AbilityExecutionContext context, TargetWrapper target)
            {

            }
        }


        public class ExtraEffectOnSpellApplyCaster : OwnedGameLogicComponent<UnitDescriptor>, IApplyAbilityEffectHandler, IGlobalSubscriber
        {
            public ActionList actions;

            public void OnAbilityEffectApplied(AbilityExecutionContext context)
            {

            }

            public void OnAbilityEffectAppliedToTarget(AbilityExecutionContext context, TargetWrapper target)
            {
                if (target.Unit == null)
                {
                    return;
                }

                if (context.MaybeCaster != this.Fact.MaybeContext.MaybeCaster)
                {
                    return;
                }

                (this.Fact as IFactContextOwner).RunActionInContext(actions, target.Unit);
            }

            public void OnTryToApplyAbilityEffect(AbilityExecutionContext context, TargetWrapper target)
            {

            }
        }






        [Harmony12.HarmonyPatch(typeof(ModifiableValue))]
        [Harmony12.HarmonyPatch("AddModifier", Harmony12.MethodType.Normal)]
        [Harmony12.HarmonyPatch(new Type[] {typeof(ModifiableValue.Modifier) })]
        static class ModifiableValue_AddModifier_Patch
        {
            internal static bool Prefix(ModifiableValue __instance , ModifiableValue.Modifier mod)
            {
                Main.TraceLog();
                EventBus.RaiseEvent<IOnModifierAdd>((Action<IOnModifierAdd>)(h => h.onModifierAdd(__instance, mod)));

                return true;
            }
        }


        [Harmony12.HarmonyPatch(typeof(ModifiableValue))]
        [Harmony12.HarmonyPatch("RemoveModifier", Harmony12.MethodType.Normal)]
        [Harmony12.HarmonyPatch(new Type[] { typeof(ModifiableValue.Modifier) })]
        static class ModifiableValue_RemoveModifier_Patch
        {
            internal static void Postfix(ModifiableValue __instance, ModifiableValue.Modifier mod, bool __result)
            {
                if (__result)
                {
                    EventBus.RaiseEvent<IOnModifierAdd>((Action<IOnModifierAdd>)(h => h.onModifierRemove(__instance, mod)));
                }
            }
        }
    }
}
