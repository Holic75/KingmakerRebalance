using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Blueprints.Items;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.UI.Log;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker;
using UnityEngine;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem.Rules;

namespace CallOfTheWild
{

    class Common
    {

        static internal string[] roman_id = { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };


        static internal BlueprintFeatureSelection EldritchKnightSpellbookSelection = Main.library.Get<BlueprintFeatureSelection>("dc3ab8d0484467a4787979d93114ebc3");
        static internal BlueprintFeatureSelection ArcaneTricksterSelection = Main.library.Get<BlueprintFeatureSelection>("ae04b7cdeb88b024b9fd3882cc7d3d76");
        static internal BlueprintFeatureSelection DragonDiscipleSpellbookSelection = Main.library.Get<BlueprintFeatureSelection>("8c1ba14c0b6dcdb439c56341385ee474");
        static internal BlueprintFeatureSelection MysticTheurgeArcaneSpellbookSelection = Main.library.Get<BlueprintFeatureSelection>("97f510c6483523c49bc779e93e4c4568");
        static internal BlueprintFeatureSelection MysticTheurgeDivineSpellbookSelection = Main.library.Get<BlueprintFeatureSelection>("7cd057944ce7896479717778330a4933");
        static LibraryScriptableObject library => Main.library;

        internal enum DomainSpellsType
        {
            NoSpells = 1,
            SpecialList = 2,
            NormalList = 3
        }

        internal struct SpellId
        {
            public readonly string guid;
            public readonly int level;
            public SpellId(string spell_guid, int spell_level)
            {
                guid = spell_guid;
                level = spell_level;
            }
        }

        internal static BlueprintFeature createSwitchActivatableAbilityBuff(string v1, string v2, string v3, string v4, BlueprintBuff buff, BlueprintBuff rage_buff, AnimationClip activateWithUnitAnimation, ActivatableAbilityGroup barbarianStance, object command_type)
        {
            throw new NotImplementedException();
        }

        internal class ExtraSpellList
        {
            SpellId[] spells;

            public ExtraSpellList(params SpellId[] list_spells)
            {
                spells = list_spells;
            }


            public ExtraSpellList(params string[] list_spell_guids)
            {
                spells = new SpellId[list_spell_guids.Length];
                for (int i = 0; i < list_spell_guids.Length; i++)
                {
                    spells[i] = new SpellId(list_spell_guids[i], i + 1);
                }
            }


            public Kingmaker.Blueprints.Classes.Spells.BlueprintSpellList createSpellList(string name, string guid)
            {
                var spell_list = Helpers.Create<Kingmaker.Blueprints.Classes.Spells.BlueprintSpellList>();
                spell_list.name = name;
                library.AddAsset(spell_list, guid);
                spell_list.SpellsByLevel = new SpellLevelList[10];
                for (int i = 0; i < spell_list.SpellsByLevel.Length; i++)
                {
                    spell_list.SpellsByLevel[i] = new SpellLevelList(i);
                }
                foreach (var s in spells)
                {
                    var spell = library.Get<BlueprintAbility>(s.guid);
                    spell.AddToSpellList(spell_list, s.level);
                }
                return spell_list;
            }


            public Kingmaker.UnitLogic.FactLogic.LearnSpellList createLearnSpellList(string name, string guid, BlueprintCharacterClass character_class, BlueprintArchetype archetype = null)
            {
                Kingmaker.UnitLogic.FactLogic.LearnSpellList learn_spell_list = Helpers.Create<Kingmaker.UnitLogic.FactLogic.LearnSpellList>();
                learn_spell_list.Archetype = archetype;
                learn_spell_list.CharacterClass = character_class;
                learn_spell_list.SpellList = createSpellList(name, guid);
                return learn_spell_list;
            }

        }


        internal static BlueprintFeature createCantrips(string name, string display_name, string description, UnityEngine.Sprite icon, string guid, BlueprintCharacterClass character_class,
                                       StatType stat, BlueprintAbility[] spells)
        {
            var learn_spells = Helpers.Create<LearnSpells>();
            learn_spells.CharacterClass = character_class;
            learn_spells.Spells = spells;

            var bind_spells = Helpers.CreateBindToClass(character_class, stat, spells);
            bind_spells.LevelStep = 1;
            bind_spells.Cantrip = true;
            return Helpers.CreateFeature(name,
                                  display_name,
                                  description,
                                  guid,
                                  icon,
                                  FeatureGroup.None,
                                  Helpers.CreateAddFacts(spells),
                                  learn_spells,
                                  bind_spells
                                  );
        }

        internal static Kingmaker.UnitLogic.Mechanics.Actions.ContextActionConditionalSaved createContextSavedApplyBuff(BlueprintBuff buff, DurationRate duration_rate,
                                                                                                                        AbilityRankType rank_type = AbilityRankType.Default,
                                                                                                                        bool is_from_spell = true, bool is_permanent = false, bool is_child = false,
                                                                                                                        bool on_failed_save = true, bool is_dispellable = true)
        {
            var context_saved = Helpers.Create<Kingmaker.UnitLogic.Mechanics.Actions.ContextActionConditionalSaved>();

            var apply_buff = Helpers.Create<Kingmaker.UnitLogic.Mechanics.Actions.ContextActionApplyBuff>();
            apply_buff.IsFromSpell = is_from_spell;
            apply_buff.AsChild = is_child;
            apply_buff.Permanent = is_permanent;
            apply_buff.Buff = buff;
            apply_buff.IsNotDispelable = !is_dispellable;
            var bonus_value = Helpers.CreateContextValue(rank_type);
            bonus_value.ValueType = ContextValueType.Rank;
            apply_buff.DurationValue = Helpers.CreateContextDuration(bonus: bonus_value,
                                                                           rate: duration_rate);
            if (on_failed_save)
            {
                context_saved.Succeed = new Kingmaker.ElementsSystem.ActionList();
                context_saved.Failed = Helpers.CreateActionList(apply_buff);
            }
            else
            {
                context_saved.Failed = new Kingmaker.ElementsSystem.ActionList();
                context_saved.Succeed = Helpers.CreateActionList(apply_buff);
            }
            return context_saved;
        }


        internal static Kingmaker.UnitLogic.Mechanics.Actions.ContextActionConditionalSaved createContextSavedApplyBuff(BlueprintBuff buff, ContextDurationValue duration, bool is_from_spell = false,
                                                                                                                  bool is_child = false, bool is_permanent = false, bool is_dispellable = true)
        {
            var context_saved = Helpers.Create<Kingmaker.UnitLogic.Mechanics.Actions.ContextActionConditionalSaved>();
            context_saved.Succeed = new Kingmaker.ElementsSystem.ActionList();
            var apply_buff = Helpers.Create<Kingmaker.UnitLogic.Mechanics.Actions.ContextActionApplyBuff>();
            apply_buff.IsFromSpell = true;
            apply_buff.Buff = buff;
            apply_buff.DurationValue = duration;
            apply_buff.IsFromSpell = is_from_spell;
            apply_buff.AsChild = is_child;
            apply_buff.Permanent = is_permanent;
            apply_buff.IsNotDispelable = !is_dispellable;
            context_saved.Failed = Helpers.CreateActionList(apply_buff);
            return context_saved;
        }


        static internal Kingmaker.UnitLogic.Mechanics.Components.DeathActions createDeathActions(Kingmaker.ElementsSystem.ActionList action_list,
                                                                                                 BlueprintAbilityResource resource = null)
        {
            var a = Helpers.Create<Kingmaker.UnitLogic.Mechanics.Components.DeathActions>();
            a.Actions = action_list;
            a.CheckResource = (resource != null);
            a.Resource = resource;
            return a;
        }


        static internal Kingmaker.Designers.Mechanics.Facts.CriticalConfirmationACBonus createCriticalConfirmationACBonus(int bonus)
        {
            var c = Helpers.Create<CriticalConfirmationACBonus>();
            c.Bonus = bonus;
            return c;
        }


        static internal CriticalConfirmationBonus createCriticalConfirmationBonus(int bonus)
        {
            var c = Helpers.Create<CriticalConfirmationBonus>();
            c.Bonus = bonus;
            return c;
        }

        internal static ContextActionSavingThrow createContextActionSavingThrow(SavingThrowType saving_throw, Kingmaker.ElementsSystem.ActionList action)
        {
            var c = Helpers.Create<ContextActionSavingThrow>();
            c.Type = saving_throw;
            c.Actions = action;
            return c;
        }


        internal static Kingmaker.UnitLogic.Mechanics.Components.ContextCalculateAbilityParamsBasedOnClass createContextCalculateAbilityParamsBasedOnClass(BlueprintCharacterClass character_class,
                                                                                                                                                    StatType stat, bool use_kineticist_main_stat = false)
        {
            var c = Helpers.Create<ContextCalculateAbilityParamsBasedOnClass>();
            c.CharacterClass = character_class;
            c.StatType = stat;
            c.UseKineticistMainStat = use_kineticist_main_stat;
            return c;
        }


        internal static NewMechanics.ContextCalculateAbilityParamsBasedOnClasses createContextCalculateAbilityParamsBasedOnClasses(BlueprintCharacterClass[] character_classes,
                                                                                                                                            StatType stat)
        {
            var c = Helpers.Create<NewMechanics.ContextCalculateAbilityParamsBasedOnClasses>();
            c.CharacterClasses = character_classes;
            c.StatType = stat;
            return c;
        }


        internal static Kingmaker.UnitLogic.FactLogic.AddSecondaryAttacks createAddSecondaryAttacks(params Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon[] weapons)
        {
            var c = Helpers.Create<Kingmaker.UnitLogic.FactLogic.AddSecondaryAttacks>();
            c.Weapon = weapons;
            return c;
        }


        internal static Kingmaker.UnitLogic.Mechanics.Components.AddIncomingDamageTrigger createIncomingDamageTrigger(params Kingmaker.ElementsSystem.GameAction[] actions)
        {
            var c = Helpers.Create<Kingmaker.UnitLogic.Mechanics.Components.AddIncomingDamageTrigger>();
            c.Actions = Helpers.CreateActionList(actions);
            return c;
        }


        static internal Kingmaker.UnitLogic.Mechanics.Actions.ContextActionApplyBuff createContextActionApplyBuff(BlueprintBuff buff, ContextDurationValue duration, bool is_from_spell = false,
                                                                                                                  bool is_child = false, bool is_permanent = false, bool dispellable = true, 
                                                                                                                  bool use_duration_seconds = false)
        {
            var apply_buff = Helpers.Create<Kingmaker.UnitLogic.Mechanics.Actions.ContextActionApplyBuff>();
            apply_buff.IsFromSpell = is_from_spell;
            apply_buff.Buff = buff;
            apply_buff.Permanent = is_permanent;
            apply_buff.DurationValue = duration;
            apply_buff.IsNotDispelable = !dispellable;
            apply_buff.UseDurationSeconds = use_duration_seconds;
            return apply_buff;
        }


        static internal Kingmaker.UnitLogic.Mechanics.Actions.ContextActionRandomize createContextActionRandomize(params Kingmaker.ElementsSystem.ActionList[] actions)
        {
            var c = Helpers.Create<Kingmaker.UnitLogic.Mechanics.Actions.ContextActionRandomize>();
            Type m_Action_type = Helpers.GetField(c, "m_Actions").GetType().GetElementType();
            var y = Array.CreateInstance(m_Action_type, actions.Length);
            var field = m_Action_type.GetField("Action");
            for (int i = 0; i < actions.Length; i++)
            {
                var yi = m_Action_type.GetConstructor(new System.Type[0]).Invoke(new object[0]);
                field.SetValue(yi, actions[i]);
                y.SetValue(yi, i);
            }
            Helpers.SetField(c, "m_Actions", y);
            return c;
        }


        static internal BuffDescriptorImmunity createBuffDescriptorImmunity(SpellDescriptor descriptor)
        {
            var b = Helpers.Create<BuffDescriptorImmunity>();
            b.Descriptor = descriptor;
            return b;
        }


        static internal SpecificBuffImmunity createSpecificBuffImmunity(BlueprintBuff buff)
        {
            var b = Helpers.Create<SpecificBuffImmunity>();
            b.Buff = buff;
            return b;
        }


        static internal NewMechanics.SpecificBuffImmunityExceptCaster createSpecificBuffImmunityExceptCaster(BlueprintBuff buff, bool except_caster = true)
        {
            var b = Helpers.Create<NewMechanics.SpecificBuffImmunityExceptCaster>();
            b.Buff = buff;
            b.except_caster = except_caster;
            return b;
        }

        static internal Blindsense createBlindsense(int range)
        {
            var b = Helpers.Create<Blindsense>();
            b.Range = range.Feet();
            return b;
        }


        static internal Blindsense createBlindsight(int range)
        {
            var b = Helpers.Create<Blindsense>();
            b.Range = range.Feet();
            b.Blindsight = true;
            return b;
        }


        static internal Kingmaker.Designers.Mechanics.Facts.AddFortification createAddFortification(int bonus = 0, ContextValue value = null)
        {
            var a = Helpers.Create<AddFortification>();
            a.Bonus = bonus;
            a.UseContextValue = value == null ? true : false;
            a.Value = value;
            return a;
        }


        static internal Kingmaker.Designers.Mechanics.Buffs.BuffStatusCondition createBuffStatusCondition(UnitCondition condition, SavingThrowType save_type = SavingThrowType.Unknown,
                                                                                                           bool save_each_round = true)
        {
            var c = Helpers.Create<Kingmaker.Designers.Mechanics.Buffs.BuffStatusCondition>();
            c.SaveType = save_type;
            c.SaveEachRound = save_each_round;
            c.Condition = condition;
            return c;
        }

        static internal Kingmaker.UnitLogic.Buffs.Conditions.BuffConditionCheckRoundNumber createBuffConditionCheckRoundNumber(int round_number, bool not = false)
        {
            var c = Helpers.Create<Kingmaker.UnitLogic.Buffs.Conditions.BuffConditionCheckRoundNumber>();
            c.RoundNumber = round_number;
            c.Not = not;
            return c;
        }


        static internal ContextValue createSimpleContextValue(int value)
        {
            var v = new ContextValue();
            v.Value = value;
            v.ValueType = ContextValueType.Simple;
            return v;
        }


        static internal Kingmaker.UnitLogic.FactLogic.SpontaneousSpellConversion createSpontaneousSpellConversion(BlueprintCharacterClass character_class, params BlueprintAbility[] spells)
        {
            var sc = Helpers.Create<Kingmaker.UnitLogic.FactLogic.SpontaneousSpellConversion>();
            sc.CharacterClass = character_class;
            sc.SpellsByLevel = spells;
            return sc;
        }


        static internal Kingmaker.Blueprints.Classes.Prerequisites.PrerequisiteAlignment createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType alignment)
        {
            var p = Helpers.Create<Kingmaker.Blueprints.Classes.Prerequisites.PrerequisiteAlignment>();
            p.Alignment = alignment;
            return p;
        }


        static internal Kingmaker.Designers.Mechanics.Facts.AddCasterLevelForAbility createAddCasterLevelToAbility(BlueprintAbility spell, int bonus)
        {
            var a = Helpers.Create<Kingmaker.Designers.Mechanics.Facts.AddCasterLevelForAbility>();
            a.Bonus = bonus;
            a.Spell = spell;
            return a;
        }

        static internal PrerequisiteArchetypeLevel createPrerequisiteArchetypeLevel(BlueprintCharacterClass character_class, BlueprintArchetype archetype, int level)
        {
            var p = Helpers.Create<PrerequisiteArchetypeLevel>();
            p.CharacterClass = character_class;
            p.Archetype = archetype;
            p.Level = level;
            return p;
        }


        static internal Kingmaker.Designers.Mechanics.Facts.ArcaneArmorProficiency createArcaneArmorProficiency(params Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup[] armor)
        {
            var p = Helpers.Create<Kingmaker.Designers.Mechanics.Facts.ArcaneArmorProficiency>();
            p.Armor = armor;
            return p;
        }


        static internal Kingmaker.Blueprints.Classes.Spells.SpellsLevelEntry createSpellsLevelEntry(params int[] count)
        {
            var s = new Kingmaker.Blueprints.Classes.Spells.SpellsLevelEntry();
            s.Count = count;
            return s;
        }

        static internal Kingmaker.Blueprints.Classes.Spells.BlueprintSpellsTable createSpellsTable(string name, string guid, params Kingmaker.Blueprints.Classes.Spells.SpellsLevelEntry[] levels)
        {
            var t = Helpers.Create<Kingmaker.Blueprints.Classes.Spells.BlueprintSpellsTable>();
            t.name = name;
            library.AddAsset(t, guid);
            t.Levels = levels;
            return t;
        }


        static internal Kingmaker.Blueprints.Classes.Spells.BlueprintSpellsTable createSpontaneousHalfCasterSpellsPerDay(string name, string guid)
        {
            return createSpellsTable(name, guid,
                                       Common.createSpellsLevelEntry(),  //0
                                       Common.createSpellsLevelEntry(),  //1
                                       Common.createSpellsLevelEntry(),  //2
                                       Common.createSpellsLevelEntry(),  //2
                                       Common.createSpellsLevelEntry(0, 1), //4
                                       Common.createSpellsLevelEntry(0, 1), //5
                                       Common.createSpellsLevelEntry(0, 1), //6
                                       Common.createSpellsLevelEntry(0, 1, 1), //7
                                       Common.createSpellsLevelEntry(0, 1, 1), //8
                                       Common.createSpellsLevelEntry(0, 2, 1), //9
                                       Common.createSpellsLevelEntry(0, 2, 1, 1), //10
                                       Common.createSpellsLevelEntry(0, 2, 1, 1), //11
                                       Common.createSpellsLevelEntry(0, 2, 2, 1), //12
                                       Common.createSpellsLevelEntry(0, 3, 2, 1, 1), //13
                                       Common.createSpellsLevelEntry(0, 3, 2, 1, 1), //14
                                       Common.createSpellsLevelEntry(0, 3, 2, 2, 1), //15
                                       Common.createSpellsLevelEntry(0, 3, 3, 2, 1), //16
                                       Common.createSpellsLevelEntry(0, 4, 3, 2, 1), //17
                                       Common.createSpellsLevelEntry(0, 4, 4, 2, 2), //18
                                       Common.createSpellsLevelEntry(0, 4, 3, 3, 2), //19
                                       Common.createSpellsLevelEntry(0, 4, 4, 3, 2) //20
                                       );
        }


        static internal Kingmaker.Blueprints.Classes.Spells.BlueprintSpellsTable createSpontaneousHalfCasterSpellsKnown(string name, string guid)
        {
            return createSpellsTable(name, guid,
                                       Common.createSpellsLevelEntry(),  //0
                                       Common.createSpellsLevelEntry(),  //1
                                       Common.createSpellsLevelEntry(),  //2
                                       Common.createSpellsLevelEntry(),  //2
                                       Common.createSpellsLevelEntry(0, 2), //4
                                       Common.createSpellsLevelEntry(0, 3), //5
                                       Common.createSpellsLevelEntry(0, 4), //6
                                       Common.createSpellsLevelEntry(0, 4, 2), //7
                                       Common.createSpellsLevelEntry(0, 4, 3), //8
                                       Common.createSpellsLevelEntry(0, 5, 4), //9
                                       Common.createSpellsLevelEntry(0, 5, 4, 2), //10
                                       Common.createSpellsLevelEntry(0, 5, 4, 3), //11
                                       Common.createSpellsLevelEntry(0, 6, 5, 4), //12
                                       Common.createSpellsLevelEntry(0, 6, 5, 4, 2), //13
                                       Common.createSpellsLevelEntry(0, 6, 5, 4, 3), //14
                                       Common.createSpellsLevelEntry(0, 6, 6, 5, 4), //15
                                       Common.createSpellsLevelEntry(0, 6, 6, 5, 4), //16
                                       Common.createSpellsLevelEntry(0, 6, 6, 5, 4), //17
                                       Common.createSpellsLevelEntry(0, 6, 6, 6, 5), //18
                                       Common.createSpellsLevelEntry(0, 6, 6, 6, 5), //19
                                       Common.createSpellsLevelEntry(0, 6, 6, 6, 5) //20
                                       );
        }


        static internal Kingmaker.Designers.Mechanics.Buffs.EmptyHandWeaponOverride createEmptyHandWeaponOverride(BlueprintItemWeapon weapon)
        {
            var c = Helpers.Create<Kingmaker.Designers.Mechanics.Buffs.EmptyHandWeaponOverride>();
            c.Weapon = weapon;
            return c;
        }


        static internal RemoveFeatureOnApply createRemoveFeatureOnApply(BlueprintFeature feature)
        {
            var c = Helpers.Create<RemoveFeatureOnApply>();
            c.Feature = feature;
            return c;
        }


        static internal void addContextActionApplyBuffOnFactsToActivatedAbilityBuff(BlueprintBuff target_buff, BlueprintBuff buff_to_add, Kingmaker.ElementsSystem.GameAction[] pre_actions,
                                                                                      params BlueprintUnitFact[] facts)
        {
            if (target_buff.GetComponent<AddFactContextActions>() == null)
            {
                target_buff.AddComponent(Helpers.CreateEmptyAddFactContextActions());
            }
            var condition = new Kingmaker.UnitLogic.Mechanics.Conditions.ContextConditionHasFact[facts.Length];
            for (int i = 0; i < facts.Length; i++)
            {
                condition[i] = Helpers.CreateConditionHasFact(facts[i]);
            }
            var action = Helpers.CreateConditional(condition, pre_actions.AddToArray(Common.createContextActionApplyBuff(buff_to_add, Helpers.CreateContextDuration(), false, true, true)));
            addContextActionApplyBuffOnConditionToActivatedAbilityBuff(target_buff, buff_to_add, action);
            /*(var activated = target_buff.GetComponent<Kingmaker.UnitLogic.Mechanics.Components.AddFactContextActions>().Activated;
            activated.Actions = activated.Actions.AddToArray(action);
            var deactivated = target_buff.GetComponent<Kingmaker.UnitLogic.Mechanics.Components.AddFactContextActions>().Deactivated;
            var remove_buff = Helpers.Create<Kingmaker.UnitLogic.Mechanics.Actions.ContextActionRemoveBuff>();
            remove_buff.Buff = buff_to_add;
            deactivated.Actions = deactivated.Actions.AddToArray(remove_buff);*/
        }


        static internal void addContextActionApplyBuffOnConditionToActivatedAbilityBuff(BlueprintBuff target_buff, BlueprintBuff buff_to_add, Conditional conditional_action)
        {
            if (target_buff.GetComponent<AddFactContextActions>() == null)
            {
                target_buff.AddComponent(Helpers.CreateEmptyAddFactContextActions());
            }

            var activated = target_buff.GetComponent<Kingmaker.UnitLogic.Mechanics.Components.AddFactContextActions>().Activated;
            activated.Actions = activated.Actions.AddToArray(conditional_action);
            var deactivated = target_buff.GetComponent<Kingmaker.UnitLogic.Mechanics.Components.AddFactContextActions>().Deactivated;
            var remove_buff = Helpers.Create<Kingmaker.UnitLogic.Mechanics.Actions.ContextActionRemoveBuff>();
            remove_buff.Buff = buff_to_add;
            deactivated.Actions = deactivated.Actions.AddToArray(remove_buff);
        }


        static internal void addContextActionApplyBuffOnFactsToActivatedAbilityBuff(BlueprintBuff target_buff, BlueprintBuff buff_to_add, params BlueprintUnitFact[] facts)
        {
            addContextActionApplyBuffOnFactsToActivatedAbilityBuff(target_buff, buff_to_add, new Kingmaker.ElementsSystem.GameAction[0], facts);
        }


        static internal Kingmaker.UnitLogic.Buffs.Components.AddAreaEffect createAddAreaEffect(BlueprintAbilityAreaEffect area_effect)
        {
            var a = Helpers.Create<Kingmaker.UnitLogic.Buffs.Components.AddAreaEffect>();
            a.AreaEffect = area_effect;
            return a;
        }


        static internal AddInitiatorAttackWithWeaponTrigger createAddInitiatorAttackWithWeaponTrigger(Kingmaker.ElementsSystem.ActionList action, bool only_hit = true, bool critical_hit = false,
                                                                                                      bool check_weapon_range_type = false,
                                                                                                      AttackTypeAttackBonus.WeaponRangeType range_type = AttackTypeAttackBonus.WeaponRangeType.Melee)
        {
            var t = Helpers.Create<AddInitiatorAttackWithWeaponTrigger>();
            t.Action = action;
            t.OnlyHit = only_hit;
            t.CriticalHit = critical_hit;
            t.CheckWeaponRangeType = check_weapon_range_type;
            t.RangeType = range_type;
            return t;
        }


        static internal Kingmaker.UnitLogic.FactLogic.AddOutgoingPhysicalDamageProperty createAddOutgoingAlignment(DamageAlignment alignment, bool check_range = false, bool is_ranged = false)
        {
            var a = Helpers.Create<AddOutgoingPhysicalDamageProperty>();
            a.AddAlignment = true;
            a.Alignment = alignment;
            a.CheckRange = check_range;
            a.IsRanged = is_ranged;
            return a;
        }



        static internal NewMechanics.ContextWeaponTypeDamageBonus createContextWeaponTypeDamageBonus(ContextValue bonus, params BlueprintWeaponType[] weapon_types)
        {
            var c = Helpers.Create<NewMechanics.ContextWeaponTypeDamageBonus>();
            c.Value = bonus;
            c.weapon_types = weapon_types;
            return c;
        }

        internal static BlueprintFeatureSelection copyRenameSelection(string original_selection_guid, string name_prefix, string description, string selection_guid, string[] feature_guids)
        {
            var old_selection = library.Get<BlueprintFeatureSelection>(original_selection_guid);
            var new_selection = library.CopyAndAdd<BlueprintFeatureSelection>(original_selection_guid, name_prefix + old_selection, selection_guid);

            new_selection.SetDescription(description);

            BlueprintFeature[] new_features = new BlueprintFeature[feature_guids.Length];

            var old_features = old_selection.AllFeatures;
            if (new_features.Length != old_features.Length)
            {
                throw Main.Error($"Incorrect number of guids passed to Common.copyRenameSelection:: guids.Length =  {new_features.Length}, terrains.Length: {old_features.Length}");
            }
            for (int i = 0; i < old_features.Length; i++)
            {
                new_features[i] = library.CopyAndAdd<BlueprintFeature>(old_features[i].AssetGuid, name_prefix + old_features[i].name, feature_guids[i]);
                new_features[i].SetDescription(description);
            }
            new_selection.AllFeatures = new_features;
            return new_selection;
        }





        internal static BlueprintFeature createSmite(string name, string display_name, string description, string guid, string ability_guid, UnityEngine.Sprite icon,
                                                     BlueprintCharacterClass[] classes, AlignmentComponent smite_alignment)
        {
            var smite_ability = library.CopyAndAdd<BlueprintAbility>("7bb9eb2042e67bf489ccd1374423cdec", name + "Ability", ability_guid);
            var smite_feature = library.CopyAndAdd<BlueprintFeature>("3a6db57fce75b0244a6a5819528ddf26", name + "Feature", guid);


            smite_feature.SetName(display_name);
            smite_feature.SetDescription(description);
            smite_feature.SetIcon(icon);

            smite_feature.ReplaceComponent<Kingmaker.UnitLogic.FactLogic.AddFacts>(Helpers.CreateAddFact(smite_ability));
            smite_ability.SetName(smite_feature.Name);
            smite_ability.SetDescription(smite_feature.Description);
            smite_ability.SetIcon(icon);
            smite_ability.RemoveComponent(smite_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.CasterCheckers.AbilityCasterAlignment>());
            var context_rank_config = smite_ability.GetComponents<Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig>().Where(a => a.Type == AbilityRankType.DamageBonus).ElementAt(0);
            var new_context_rank_config = context_rank_config.CreateCopy();
            Helpers.SetField(new_context_rank_config, "m_Class", classes);
            smite_ability.ReplaceComponent(context_rank_config, new_context_rank_config);

            var smite_action = smite_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>();
            var new_smite_action = smite_action.CreateCopy();
            var condition = (Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional)new_smite_action.Actions.Actions[0];
            new_smite_action.Actions = Helpers.CreateActionList(new_smite_action.Actions.Actions);
            var old_conditional = (Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional)new_smite_action.Actions.Actions[0];
            var conditions = new Kingmaker.ElementsSystem.Condition[] { Helpers.CreateContextConditionAlignment(smite_alignment, false, false), old_conditional.ConditionsChecker.Conditions[1] };
            new_smite_action.Actions.Actions[0] = Helpers.CreateConditional(conditions, old_conditional.IfTrue.Actions, old_conditional.IfFalse.Actions);

            return smite_feature;
        }


        internal static PrerequisiteNoArchetype prerequisiteNoArchetype(BlueprintCharacterClass character_class, BlueprintArchetype archetype)
        {
            var p = Helpers.Create<PrerequisiteNoArchetype>();
            p.Archetype = archetype;
            p.CharacterClass = character_class;
            return p;
        }

        internal static BlueprintFeature createSpellResistance(string name, string display_name, string description, string guid, BlueprintCharacterClass character_class, int start_value)
        {
            var spell_resistance = library.CopyAndAdd<BlueprintFeature>("01182bcee8cb41640b7fa1b1ad772421", //monk diamond soul
                                                                        name,
                                                                        guid);
            spell_resistance.SetName(display_name);
            spell_resistance.SetDescription(description);
            spell_resistance.Groups = new FeatureGroup[0];
            spell_resistance.RemoveComponent(spell_resistance.GetComponent<Kingmaker.Blueprints.Classes.Prerequisites.PrerequisiteClassLevel>());
            var context_rank_config = spell_resistance.GetComponent<Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig>();
            Helpers.SetField(context_rank_config, "m_StepLevel", start_value);
            Helpers.SetField(context_rank_config, "m_Class", new BlueprintCharacterClass[] { character_class });

            return spell_resistance;
        }





        internal static Kingmaker.UnitLogic.FactLogic.AddDamageResistancePhysical createAlignmentDR(int dr_value, DamageAlignment alignment)
        {
            var feat = Helpers.Create<Kingmaker.UnitLogic.FactLogic.AddDamageResistancePhysical>();
            feat.Alignment = alignment;
            feat.BypassedByAlignment = true;
            feat.Value.ValueType = ContextValueType.Simple;
            feat.Value.Value = dr_value;

            return feat;
        }


        internal static Kingmaker.UnitLogic.FactLogic.AddDamageResistancePhysical createMatrialDR(int dr_value, PhysicalDamageMaterial material)
        {
            var feat = Helpers.Create<Kingmaker.UnitLogic.FactLogic.AddDamageResistancePhysical>();
            feat.Material = material;
            feat.BypassedByMaterial = true;
            feat.Value.ValueType = ContextValueType.Simple;
            feat.Value.Value = dr_value;

            return feat;
        }


        internal static Kingmaker.UnitLogic.FactLogic.AddDamageResistancePhysical createMagicDR(int dr_value)
        {
            var feat = Helpers.Create<Kingmaker.UnitLogic.FactLogic.AddDamageResistancePhysical>();
            feat.BypassedByMagic = true;
            feat.Value.ValueType = ContextValueType.Simple;
            feat.Value.Value = dr_value;

            return feat;
        }


        internal static AddCondition createAddCondition(UnitCondition condition)
        {
            var a = Helpers.Create<AddCondition>();
            a.Condition = condition;
            return a;
        }


        internal static Kingmaker.UnitLogic.FactLogic.AddDamageResistancePhysical createAlignmentDRContextRank(DamageAlignment alignment, AbilityRankType rank = AbilityRankType.StatBonus)
        {
            var feat = Helpers.Create<Kingmaker.UnitLogic.FactLogic.AddDamageResistancePhysical>();
            feat.Alignment = alignment;
            feat.BypassedByAlignment = true;
            feat.Value = Helpers.CreateContextValueRank(rank);
            return feat;
        }


        internal static Kingmaker.UnitLogic.FactLogic.AddDamageResistanceEnergy createEnergyDR(int dr_value, DamageEnergyType energy)
        {
            var feat = Helpers.Create<Kingmaker.UnitLogic.FactLogic.AddDamageResistanceEnergy>();
            feat.Type = energy;
            feat.Value.ValueType = ContextValueType.Simple;
            feat.Value.Value = dr_value;

            return feat;
        }


        internal static Kingmaker.UnitLogic.FactLogic.AddDamageResistanceEnergy createEnergyDRContextRank(DamageEnergyType energy, AbilityRankType rank = AbilityRankType.StatBonus)
        {
            var feat = Helpers.Create<Kingmaker.UnitLogic.FactLogic.AddDamageResistanceEnergy>();
            feat.Type = energy;
            feat.Value = Helpers.CreateContextValueRank(rank);
            return feat;
        }


        internal static void addClassToDomains(BlueprintCharacterClass class_to_add, BlueprintArchetype[] archetypes_to_add, DomainSpellsType spells_type, BlueprintFeatureSelection domain_selection)
        {
            var domains = domain_selection.AllFeatures;
            foreach (var domain_feature in domains)
            {

                BlueprintProgression domain = (BlueprintProgression)domain_feature;
                domain.Classes = domain.Classes.AddToArray(class_to_add);
                domain.Archetypes = domain.Archetypes.AddToArray(archetypes_to_add);
                // Main.logger.Log("Processing " + domain.Name);

                foreach (var entry in domain.LevelEntries)
                {
                    foreach (var feat in entry.Features)
                    {
                        addClassToFeat(class_to_add, archetypes_to_add, spells_type, feat);
                    }
                }

                if (spells_type == DomainSpellsType.NormalList)
                {

                    var spell_list = domain.GetComponent<Kingmaker.UnitLogic.FactLogic.LearnSpellList>().SpellList;

                    if (archetypes_to_add.Empty())
                    {
                        var learn_spells_fact = Helpers.Create<Kingmaker.UnitLogic.FactLogic.LearnSpellList>();
                        learn_spells_fact.SpellList = spell_list;
                        learn_spells_fact.CharacterClass = class_to_add;
                        domain.AddComponent(learn_spells_fact);

                    }
                    else
                    {
                        foreach (var ar_type in archetypes_to_add)
                        {
                            var learn_spells_fact = Helpers.Create<Kingmaker.UnitLogic.FactLogic.LearnSpellList>();
                            learn_spells_fact.SpellList = spell_list;
                            learn_spells_fact.CharacterClass = class_to_add;
                            learn_spells_fact.Archetype = ar_type;
                            domain.AddComponent(learn_spells_fact);
                        }
                    }
                }
            }
        }


        static void addClassToContextRankConfig(BlueprintCharacterClass class_to_add, ContextRankConfig c)
        {
            BlueprintCharacterClass cleric_class = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var classes = Helpers.GetField<BlueprintCharacterClass[]>(c, "m_Class");
            if (classes.Contains(cleric_class))
            {
                classes = classes.AddToArray(class_to_add);
                Helpers.SetField(c, "m_Class", classes);
            }
        }


        static void addClassToAreaEffect(BlueprintCharacterClass class_to_add, Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbilityAreaEffect a)
        {
            var components = a.ComponentsArray;
            foreach (var c in components)
            {
                if (c is ContextRankConfig)
                {
                    var c_typed = (Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig)c;
                    addClassToContextRankConfig(class_to_add, c_typed);
                }
                else if (c is Kingmaker.UnitLogic.Abilities.Components.AreaEffects.AbilityAreaEffectBuff)
                {
                    var c_typed = (Kingmaker.UnitLogic.Abilities.Components.AreaEffects.AbilityAreaEffectBuff)c;
                    addClassToBuff(class_to_add, c_typed.Buff);
                }
            }
        }


        static void addClassToBuff(BlueprintCharacterClass class_to_add, BlueprintBuff b)
        {
            var components = b.ComponentsArray;
            foreach (var c in components)
            {
                if (c is ContextRankConfig)
                {
                    var c_typed = (Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig)c;
                    addClassToContextRankConfig(class_to_add, c_typed);
                }
                else if (c is Kingmaker.UnitLogic.Buffs.Components.AddAreaEffect)
                {
                    var c_typed = (Kingmaker.UnitLogic.Buffs.Components.AddAreaEffect)c;
                    addClassToAreaEffect(class_to_add, c_typed.AreaEffect);
                }
            }
        }


        static void addClassToAbility(BlueprintCharacterClass class_to_add, BlueprintAbility a)
        {
            var components = a.ComponentsArray;
            foreach (var c in components)
            {
                if (c is Kingmaker.UnitLogic.Abilities.Components.AbilityVariants)
                {
                    var c_typed = (Kingmaker.UnitLogic.Abilities.Components.AbilityVariants)c;
                    foreach (var v in c_typed.Variants)
                    {
                        addClassToAbility(class_to_add, v);
                    }
                }
                else if (c is Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig)
                {
                    var c_typed = (Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig)c;
                    addClassToContextRankConfig(class_to_add, c_typed);
                }
                else if (c is AbilityEffectRunAction)
                {
                    var c_typed = (AbilityEffectRunAction)c;
                    foreach (var aa in c_typed.Actions.Actions)
                    {
                        if (aa == null)
                        {
                            continue;
                        }
                        if (aa is Kingmaker.UnitLogic.Mechanics.Actions.ContextActionApplyBuff)
                        {
                            var aa_typed = (Kingmaker.UnitLogic.Mechanics.Actions.ContextActionApplyBuff)aa;
                            addClassToBuff(class_to_add, aa_typed.Buff);
                        }
                    }
                }

            }
        }


        static void addClassToFact(BlueprintCharacterClass class_to_add, BlueprintArchetype[] archetypes_to_add, DomainSpellsType spells_type, BlueprintUnitFact f)
        {
            if (f is Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility)
            {
                var f_typed = (Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility)f;
                addClassToAbility(class_to_add, f_typed);
            }
            else if (f is Kingmaker.UnitLogic.ActivatableAbilities.BlueprintActivatableAbility)
            {
                var f_typed = (Kingmaker.UnitLogic.ActivatableAbilities.BlueprintActivatableAbility)f;
                addClassToBuff(class_to_add, f_typed.Buff);
            }
        }


        static void addClassToResource(BlueprintCharacterClass class_to_add, BlueprintArchetype[] archetypes_to_add, BlueprintAbilityResource rsc)
        {
            BlueprintCharacterClass cleric_class = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var amount = ExtensionMethods.getMaxAmount(rsc);
            var classes = Helpers.GetField<BlueprintCharacterClass[]>(amount, "Class");
            var archetypes = Helpers.GetField<BlueprintArchetype[]>(amount, "Archetypes");

            if (classes.Contains(cleric_class))
            {
                classes = classes.AddToArray(class_to_add);
                archetypes = archetypes.AddToArray(archetypes_to_add);
                Helpers.SetField(amount, "Class", classes);
                Helpers.SetField(amount, "Archetypes", archetypes);
                ExtensionMethods.setMaxAmount(rsc, amount);
            }

        }


        static void addClassToFeat(BlueprintCharacterClass class_to_add, BlueprintArchetype[] archetypes_to_add, DomainSpellsType spells_type, BlueprintFeatureBase feat)
        {
            foreach (var c in feat.ComponentsArray)
            {
                if (c is Kingmaker.Designers.Mechanics.Buffs.IncreaseSpellDamageByClassLevel)
                {
                    var c_typed = (Kingmaker.Designers.Mechanics.Buffs.IncreaseSpellDamageByClassLevel)c;
                    c_typed.AdditionalClasses = c_typed.AdditionalClasses.AddToArray(class_to_add);
                    c_typed.Archetypes = c_typed.Archetypes.AddToArray(archetypes_to_add);
                }
                else if (c is Kingmaker.Designers.Mechanics.Facts.AddFeatureOnClassLevel)
                {
                    var c_typed = (Kingmaker.Designers.Mechanics.Facts.AddFeatureOnClassLevel)c;
                    if (c_typed.Feature.ComponentsArray.Length > 0
                          && c_typed.Feature.ComponentsArray[0] is Kingmaker.UnitLogic.FactLogic.AddSpecialSpellList)
                    {
                        if (spells_type == DomainSpellsType.SpecialList)
                        {
                            //TODO: will need to make a copy of feature and replace CharacterClass in component with class_to_add
                        }
                        else
                        {
                            continue;
                        }
                    }
                    c_typed.AdditionalClasses = c_typed.AdditionalClasses.AddToArray(class_to_add);
                    c_typed.Archetypes = c_typed.Archetypes.AddToArray(archetypes_to_add);
                    addClassToFeat(class_to_add, archetypes_to_add, spells_type, c_typed.Feature);
                }
                else if (c is Kingmaker.UnitLogic.FactLogic.AddSpecialSpellList && spells_type == DomainSpellsType.SpecialList)
                {
                    /*var c_typed = (Kingmaker.UnitLogic.FactLogic.AddSpecialSpellList)c;
                    if (c_typed.CharacterClass != class_to_add)
                    {
                        var c2 = Helpers.Create<Kingmaker.UnitLogic.FactLogic.AddSpecialSpellList>();
                        c2.CharacterClass = class_to_add;
                        c2.SpellList = c_typed.SpellList;
                        feat.AddComponent(c2);
                    }*/
                }
                else if (c is Kingmaker.UnitLogic.FactLogic.AddFacts)
                {
                    var c_typed = (Kingmaker.UnitLogic.FactLogic.AddFacts)c;
                    foreach (var f in c_typed.Facts)
                    {
                        addClassToFact(class_to_add, archetypes_to_add, spells_type, f);
                    }
                }
                else if (c is Kingmaker.Designers.Mechanics.Facts.AddAbilityResources)
                {
                    var c_typed = (Kingmaker.Designers.Mechanics.Facts.AddAbilityResources)c;
                    addClassToResource(class_to_add, archetypes_to_add, c_typed.Resource);
                }
                else if (c is Kingmaker.Designers.Mechanics.Facts.FactSinglify)
                {
                    var c_typed = (Kingmaker.Designers.Mechanics.Facts.FactSinglify)c;
                    foreach (var f in c_typed.NewFacts)
                    {
                        addClassToFact(class_to_add, archetypes_to_add, spells_type, f);
                    }
                }
                else if (c is Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig)
                {
                    var c_typed = (Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig)c;
                    addClassToContextRankConfig(class_to_add, c_typed);
                }


            }
        }


        static internal Kingmaker.UnitLogic.FactLogic.AddConditionImmunity createAddConditionImmunity(UnitCondition condition)
        {
            Kingmaker.UnitLogic.FactLogic.AddConditionImmunity c = Helpers.Create<Kingmaker.UnitLogic.FactLogic.AddConditionImmunity>();
            c.Condition = condition;
            return c;
        }


        static internal Kingmaker.Designers.Mechanics.Facts.SavingThrowBonusAgainstDescriptor createSavingThrowBonusAgainstDescriptor(int bonus, ModifierDescriptor descriptor, SpellDescriptor spell_descriptor)
        {
            Kingmaker.Designers.Mechanics.Facts.SavingThrowBonusAgainstDescriptor c = Helpers.Create<Kingmaker.Designers.Mechanics.Facts.SavingThrowBonusAgainstDescriptor>();
            c.Bonus = bonus;
            c.ModifierDescriptor = descriptor;
            c.SpellDescriptor = spell_descriptor;
            return c;
        }


        static internal Kingmaker.Designers.Mechanics.Facts.SavingThrowContextBonusAgainstDescriptor createContextSavingThrowBonusAgainstDescriptor(ContextValue value, ModifierDescriptor descriptor, SpellDescriptor spell_descriptor)
        {
            var c = Helpers.Create<Kingmaker.Designers.Mechanics.Facts.SavingThrowContextBonusAgainstDescriptor>();
            c.ModifierDescriptor = descriptor;
            c.SpellDescriptor = spell_descriptor;
            c.Value = value;
            return c;
        }


        static internal SavingThrowBonusAgainstSchool createSavingThrowBonusAgainstSchool(int bonus, ModifierDescriptor descriptor, SpellSchool school)
        {
            var c = Helpers.Create<SavingThrowBonusAgainstSchool>();
            c.School = school;
            c.ModifierDescriptor = descriptor;
            c.Value = bonus;
            return c;
        }


        static internal Kingmaker.UnitLogic.FactLogic.BuffEnchantWornItem createBuffEnchantWornItem(Kingmaker.Blueprints.Items.Ecnchantments.BlueprintItemEnchantment enchantment)
        {
            var b = Helpers.Create<BuffEnchantWornItem>();
            b.Enchantment = enchantment;
            return b;
        }


        static internal Kingmaker.UnitLogic.FactLogic.AddEnergyDamageImmunity createAddEnergyDamageImmunity(DamageEnergyType energy_type)
        {
            var a = Helpers.Create<Kingmaker.UnitLogic.FactLogic.AddEnergyDamageImmunity>();
            a.EnergyType = energy_type;
            return a;
        }


        static internal Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityUnitCommand createActivatableAbilityUnitCommand(CommandType command_type)
        {
            var a = Helpers.Create<ActivatableAbilityUnitCommand>();
            a.Type = command_type;
            return a;
        }


        static internal Kingmaker.Designers.Mechanics.Facts.AttackTypeAttackBonus createAttackTypeAttackBonus(ContextValue value, AttackTypeAttackBonus.WeaponRangeType attack_type,
                                                                                                              ModifierDescriptor descriptor)
        {
            var a = Helpers.Create<AttackTypeAttackBonus>();
            a.AttackBonus = 1;
            a.Type = attack_type;
            a.Value = value;
            a.Descriptor = descriptor;
            return a;
        }


        static internal BlueprintFeature createSwitchActivatableAbilityBuff(string name, string switch_guid, string ability_guid, string feature_guid,
                                                                    BlueprintBuff effect, BlueprintBuff target_buff, Kingmaker.ElementsSystem.GameAction[] pre_actions,
                                                                    UnityEngine.AnimationClip animation,
                                                                    ActivatableAbilityGroup group = ActivatableAbilityGroup.None, int weight = 1,
                                                                    CommandType command_type = CommandType.Free, CommandType unit_command = CommandType.Free)

        {
            effect.SetBuffFlags(BuffFlags.HiddenInUi | effect.GetBuffFlags());
            var switch_buff = Helpers.CreateBuff(name + "SwitchBuff",
                                              effect.Name,
                                              effect.Description,
                                              switch_guid,
                                              effect.Icon,
                                              null,
                                              Helpers.CreateEmptyAddFactContextActions());

            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(switch_buff, effect, pre_actions, target_buff);
            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(target_buff, effect, pre_actions, switch_buff);

            var ability = Helpers.CreateActivatableAbility(name + "ToggleAbility",
                                                                        effect.Name,
                                                                        effect.Description,
                                                                        ability_guid,
                                                                        effect.Icon,
                                                                        switch_buff,
                                                                        AbilityActivationType.Immediately,
                                                                        command_type,
                                                                        animation
                                                                        );
            if (unit_command != CommandType.Free)
            {
                ability.AddComponent(Common.createActivatableAbilityUnitCommand(unit_command));
            }
            ability.Group = group;
            ability.WeightInGroup = weight;
            var feature = Helpers.CreateFeature(name + "Feature",
                                                effect.Name,
                                                effect.Description,
                                                feature_guid,
                                                effect.Icon,
                                                FeatureGroup.None,
                                                CallOfTheWild.Helpers.CreateAddFact(ability));

            return feature;
        }


        static internal BlueprintFeature createSwitchActivatableAbilityBuff(string name, string switch_guid, string ability_guid, string feature_guid,
                                                                            BlueprintBuff effect, BlueprintBuff target_buff, UnityEngine.AnimationClip animation,
                                                                            ActivatableAbilityGroup group = ActivatableAbilityGroup.None, int weight = 1,
                                                                            CommandType command_type = CommandType.Free, CommandType unit_command = CommandType.Free)

        {
            return createSwitchActivatableAbilityBuff(name, switch_guid, ability_guid, feature_guid, effect, target_buff, new Kingmaker.ElementsSystem.GameAction[0],
                                                      animation, group, weight, command_type, unit_command);
        }


        static internal ContextActionRemoveBuffsByDescriptor createContextActionRemoveBuffsByDescriptor(SpellDescriptor descriptor, bool not_self = true)
        {
            var r = Helpers.Create<ContextActionRemoveBuffsByDescriptor>();
            r.SpellDescriptor = descriptor;
            r.NotSelf = true;
            return r;
        }


        static internal NewMechanics.AddContextEffectFastHealing createAddContextEffectFastHealing(ContextValue value)
        {
            var a = Helpers.Create<NewMechanics.AddContextEffectFastHealing>();
            a.Value = value;
            return a;
        }


        static internal Kingmaker.Designers.Mechanics.Facts.AuraFeatureComponent createAuraFeatureComponent(BlueprintBuff buff)
        {
            var a = Helpers.Create<Kingmaker.Designers.Mechanics.Facts.AuraFeatureComponent>();
            a.Buff = buff;
            return a;
        }


        static internal Kingmaker.UnitLogic.Mechanics.Actions.ContextActionHealTarget createContextActionHealTarget(ContextDiceValue value)
        {
            var c = Helpers.Create<ContextActionHealTarget>();
            c.Value = value;
            return c;
        }


        static internal Kingmaker.UnitLogic.FactLogic.AddProficiencies createAddArmorProficiencies(params ArmorProficiencyGroup[] armor)
        {
            var a = Helpers.Create<Kingmaker.UnitLogic.FactLogic.AddProficiencies>();
            a.ArmorProficiencies = armor;
            return a;
        }

        static internal Kingmaker.UnitLogic.FactLogic.AddProficiencies createAddWeaponProficiencies(params WeaponCategory[] weapons)
        {
            var a = Helpers.Create<Kingmaker.UnitLogic.FactLogic.AddProficiencies>();
            a.WeaponProficiencies = weapons;
            return a;
        }


        static internal AddEnergyVulnerability createAddEnergyVulnerability(DamageEnergyType energy)
        {
            var a = Helpers.Create<AddEnergyVulnerability>();
            a.Type = energy;
            return a;
        }


        static internal AbilityCasterHasNoFacts createAbilityCasterHasNoFacts(params BlueprintUnitFact[] facts)
        {
            var a = Helpers.Create<AbilityCasterHasNoFacts>();
            a.Facts = facts;
            return a;
        }


        static internal AddGenericStatBonus createAddGenericStatBonus(int bonus, ModifierDescriptor descriptor, StatType stat)
        {
            var a = Helpers.Create<AddGenericStatBonus>();
            a.Stat = stat;
            a.Value = bonus;
            a.Descriptor = descriptor;
            return a;
        }


        static internal ChangeUnitSize createChangeUnitSize(Size size)
        {
            var c = Helpers.Create<ChangeUnitSize>();
            c.Size = size;
            Helpers.SetField(c, "m_Type", 1);
            return c;
        }


        static internal void addReplaceSpellbook(BlueprintFeatureSelection selection, BlueprintSpellbook spellbook, string name, params BlueprintComponent[] components)
        {
            var feature = Helpers.Create<BlueprintFeatureReplaceSpellbook>();
            feature.name = name;
            feature.Groups = new FeatureGroup[] { selection.Group };
            feature.IsClassFeature = true;
            feature.SetName(spellbook.Name);
            feature.SetDescription(selection.Description);
            feature.ComponentsArray = components;
            feature.Spellbook = spellbook;
            library.AddAsset(feature, "");
            selection.AllFeatures = selection.AllFeatures.AddToArray(feature);
        }


        static internal PrerequisiteClassSpellLevel createPrerequisiteClassSpellLevel(BlueprintCharacterClass character_class, int spell_level)
        {
            var p = Helpers.Create<PrerequisiteClassSpellLevel>();
            p.CharacterClass = character_class;
            p.RequiredSpellLevel = spell_level;
            return p;
        }


        static internal ContextActionRemoveBuff createContextActionRemoveBuff(BlueprintBuff buff)
        {
            var r = Helpers.Create<ContextActionRemoveBuff>();
            r.Buff = buff;
            return r;
        }


        static internal NewMechanics.SavingThrowBonusAgainstSpecificSpells createSavingThrowBonusAgainstSpecificSpells(int bonus, ModifierDescriptor descriptor, params BlueprintAbility[] spells)
        {
            var s = Helpers.Create<NewMechanics.SavingThrowBonusAgainstSpecificSpells>();
            s.Spells = spells;
            s.ModifierDescriptor = descriptor;
            s.Value = bonus;
            return s;
        }


        static internal AbilityTargetHasFact createAbilityTargetHasFact(bool inverted, params BlueprintUnitFact[] facts)
        {

            var a = Helpers.Create<AbilityTargetHasFact>();
            a.CheckedFacts = facts;
            a.Inverted = inverted;
            return a;
        }


        static internal NewMechanics.AbilityTargetHasNoFactUnlessBuffsFromCaster createAbilityTargetHasNoFactUnlessBuffsFromCaster(BlueprintBuff[] target_buffs,
                                                                                                          BlueprintBuff[] alternative_buffs)
        {
            var h = Helpers.Create<NewMechanics.AbilityTargetHasNoFactUnlessBuffsFromCaster>();
            h.CheckedBuffs = target_buffs;
            h.AlternativeBuffs = alternative_buffs;
            return h;
        }


        static internal Kingmaker.UnitLogic.Abilities.Components.TargetCheckers.AbilityTargetIsPartyMember createAbilityTargetIsPartyMember(bool val = false)
        {
            var a = Helpers.Create<AbilityTargetIsPartyMember>();
            a.Not = !val;
            return a;
        }


        static internal AbilityShowIfCasterHasFact createAbilityShowIfCasterHasFact(BlueprintUnitFact fact)
        {
            var a = Helpers.Create<AbilityShowIfCasterHasFact>();
            a.UnitFact = fact;
            return a;
        }


        static internal ContextConditionHasFact createContextConditionHasFact(BlueprintUnitFact fact, bool has = true)
        {
            var c = Helpers.Create<ContextConditionHasFact>();
            c.Fact = fact;
            c.Not = !has;
            return c;
        }


        static internal ContextConditionCasterHasFact createContextConditionCasterHasFact(BlueprintUnitFact fact, bool has = true)
        {
            var c = Helpers.Create<ContextConditionCasterHasFact>();
            c.Fact = fact;
            c.Not = !has;
            return c;
        }


        public static void AddBattleLogMessage(string message, object tooltip = null, Color? color = null)
        {
            var data = new LogDataManager.LogItemData(message, color ?? GameLogStrings.Instance.DefaultColor, tooltip, PrefixIcon.None);
            if (Game.Instance.UI.BattleLogManager)
            {
                Game.Instance.UI.BattleLogManager.LogView.AddLogEntry(data);
            }
        }


        static internal ClassLevelsForPrerequisites createClassLevelsForPrerequisites(BlueprintCharacterClass fake_class, BlueprintCharacterClass actual_class, double modifier = 1.0, int summand = 0)
        {
            var c = Helpers.Create<ClassLevelsForPrerequisites>();
            c.ActualClass = actual_class;
            c.FakeClass = fake_class;
            c.Modifier = modifier;
            c.Summand = summand;
            return c;
        }


        static internal void addTemworkFeats(params BlueprintFeature[] feats)
        {

            var tactical_leader_feat_share_buff = library.Get<BlueprintBuff>("a603a90d24a636c41910b3868f434447");
            var sh_teamwork_share = library.Get<BlueprintFeature>("e1f437048db80164792155102375b62c");
            var teamwork_feat = library.Get<BlueprintFeatureSelection>("d87e2f6a9278ac04caeb0f93eff95fcb");
            var teamwork_feat_vanguard = library.Get<BlueprintFeatureSelection>("90b882830b3988446ae681c6596460cc");

            sh_teamwork_share.GetComponent<ShareFeaturesWithCompanion>().Features = sh_teamwork_share.GetComponent<ShareFeaturesWithCompanion>().Features.AddToArray(feats);
            Hunter.hunter_tactics.GetComponent<ShareFeaturesWithCompanion>().Features = Hunter.hunter_tactics.GetComponent<ShareFeaturesWithCompanion>().Features.AddToArray(feats);
            tactical_leader_feat_share_buff.GetComponent<AddFactsFromCaster>().Facts = tactical_leader_feat_share_buff.GetComponent<AddFactsFromCaster>().Facts.AddToArray(feats);
            teamwork_feat.AllFeatures = teamwork_feat.AllFeatures.AddToArray(feats);
            teamwork_feat_vanguard.AllFeatures = teamwork_feat_vanguard.AllFeatures.AddToArray(feats);


            //update vanguard features
            var vanguard_variants = library.Get<BlueprintAbility>("00af3b5f43aa7ae4c87bcfe4e129f6e8").GetComponent<AbilityVariants>();
            foreach (var f in feats)
            {
                var buff = library.CopyAndAdd<BlueprintBuff>("9de63078d422dcc46a86ba0920b4991e", "VanguardTactician" + f.name + "Buff", "");
                var add_fact = buff.GetComponent<AddFactsFromCaster>().CreateCopy();
                add_fact.Facts[0] = f;
                buff.ReplaceComponent<AddFactsFromCaster>(add_fact);
                buff.SetName("Vanguard Tactician — " + f.Name);
                buff.SetDescription(f.Description);

                var ability = library.CopyAndAdd<BlueprintAbility>("53f4d8597163db24f8309462aadc4348", "VanguardTactician" + f.name + "Ability", "");
                ability.ReplaceComponent<AbilityShowIfCasterHasFact>(Common.createAbilityShowIfCasterHasFact(f));

                var condition_apply_buff = (Conditional)ability.GetComponent<AbilityEffectRunAction>().Actions.Actions[0];
                var context_apply_buff = ((ContextActionApplyBuff)condition_apply_buff.IfTrue.Actions[0]).CreateCopy();
                context_apply_buff.Buff = buff;
                var run_action = Helpers.CreateRunActions(Helpers.CreateConditional(Common.createContextConditionHasFact(f, false), context_apply_buff));
                ability.ReplaceComponent<AbilityEffectRunAction>(run_action);
                ability.SetName(buff.Name);
                ability.SetDescription(buff.Description);
                vanguard_variants.Variants = vanguard_variants.Variants.AddToArray(ability);
            }
        }


        static internal AddFeatureIfHasFact createAddFeatureIfHasFact(BlueprintUnitFact fact, BlueprintFeature feature, bool not = false)
        {
            var a = Helpers.Create<AddFeatureIfHasFact>();
            a.CheckedFact = fact;
            a.Feature = feature;
            a.Not = not;
            return a;
        }


        static internal BuffExtraAttack createBuffExtraAttack(int num, bool haste)
        {
            var b = Helpers.Create<BuffExtraAttack>();
            b.Number = num;
            b.Haste = haste;
            return b;
        }


        static internal ContextConditionIsCaster createContextConditionIsCaster(bool not = false)
        {
            var c = Helpers.Create<ContextConditionIsCaster>();
            c.Not = not;
            return c;
        }


        static internal AddWearinessHours createAddWearinessHours(int hours)
        {
            var a = Helpers.Create<AddWearinessHours>();
            a.Hours = hours;
            return a;
        }


        static internal AbilityScoreCheckBonus createAbilityScoreCheckBonus(ContextValue bonus, ModifierDescriptor descriptor, StatType stat)
        {
            var a = Helpers.Create<AbilityScoreCheckBonus>();
            a.Bonus = bonus;
            a.Descriptor = descriptor;
            a.Stat = stat;
            return a;
        }


        static internal ContextActionResurrect createContextActionResurrect(float result_health, bool full_restore = false)
        {
            var c = Helpers.Create<ContextActionResurrect>();
            c.ResultHealth = result_health;
            c.FullRestore = full_restore;
            return c;
        }


        static internal NewMechanics.ContextActionRemoveBuffFromCaster createContextActionRemoveBuffFromCaster(BlueprintBuff buff)
        {
            var c = Helpers.Create<NewMechanics.ContextActionRemoveBuffFromCaster>();
            c.Buff = buff;
            return c;
        }


        static internal BlueprintActivatableAbility convertPerformance(BlueprintActivatableAbility base_ability, BlueprintBuff effect_buff, string prefix)
        {
            var ability = library.CopyAndAdd<BlueprintActivatableAbility>(base_ability.AssetGuid, prefix + "Ability", "");
            var ability_buff = library.CopyAndAdd<BlueprintBuff>(base_ability.Buff.AssetGuid, prefix + "Buff", "");
            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>(ability_buff.GetComponent<AddAreaEffect>().AreaEffect.AssetGuid, prefix + "Area", "");

            area.ReplaceComponent<AbilityAreaEffectBuff>(c => { c.Buff = effect_buff; });

            ability_buff.ReplaceComponent<AddAreaEffect>(c => { c.AreaEffect = area; });
            ability_buff.SetName(effect_buff.Name);
            ability_buff.SetDescription(effect_buff.Description);
            ability_buff.SetIcon(effect_buff.Icon);

           
            ability.SetName(effect_buff.Name);
            ability.SetDescription(effect_buff.Description);
            ability.SetIcon(effect_buff.Icon);
            ability.Buff = ability_buff;

            return ability;
        }



        static internal BlueprintActivatableAbility convertPerformance(BlueprintActivatableAbility base_ability, BlueprintAbilityAreaEffect area, string prefix, 
                                                                                                                                     UnityEngine.Sprite icon,  string display_name, string description)
        {
            var ability = library.CopyAndAdd<BlueprintActivatableAbility>(base_ability.AssetGuid, prefix + "Ability", "");
            var ability_buff = library.CopyAndAdd<BlueprintBuff>(base_ability.Buff.AssetGuid, prefix + "Buff", "");

            ability_buff.ReplaceComponent<AddAreaEffect>(c => { c.AreaEffect = area; });
            ability_buff.SetName(display_name);
            ability_buff.SetDescription(description);
            ability_buff.SetIcon(icon);


            ability.SetName(display_name);
            ability.SetDescription(description);
            ability.SetIcon(icon);
            ability.Buff = ability_buff;

            return ability;
        }


        static internal ContextActionDispelMagic createContextActionDispelMagic(SpellDescriptor spell_descriptor, SpellSchool[] schools, RuleDispelMagic.CheckType check_type,
                                                                                 ContextValue max_spell_level = null, ContextValue max_caster_level = null)
        {
            var c = Helpers.Create<ContextActionDispelMagic>();
            c.Descriptor = spell_descriptor;
            c.Schools = schools;
            var spell_level = max_spell_level == null ? createSimpleContextValue(9) : max_spell_level;
            Helpers.SetField(c, "m_MaxSpellLevel", spell_level);
            if (max_caster_level == null)
            {
                Helpers.SetField(c, "m_UseMaxCasterLevel", false);
            }
            else
            {
                Helpers.SetField(c, "m_UseMaxCasterLevel", true);
                Helpers.SetField(c, "m_MaxCasterLevel", max_caster_level);
            }
            Helpers.SetField(c, "m_CheckType", check_type);
            return c;
        }


        static internal NewMechanics.CrowdAlliesACBonus createCrowdAlliesACBonus(int min_num_allies_around, ContextValue value, int radius = 2)
        {
            var c = Helpers.Create<NewMechanics.CrowdAlliesACBonus>();
            c.num_allies_around = min_num_allies_around;
            c.value = value;
            c.Radius = radius;
            return c;
        }


        static internal BlueprintFeature AbilityToFeature(BlueprintAbility ability, bool hide = true)
        {
            var feature = Helpers.CreateFeature(ability.name + "Feature",
                                                     ability.Name,
                                                     ability.Description,
                                                     "",
                                                     ability.Icon,
                                                     FeatureGroup.None,
                                                     Helpers.CreateAddFact(ability)
                                                     );
            if (hide)
            {
                feature.HideInCharacterSheetAndLevelUp = true;
                feature.HideInUI = true;
            }
            return feature;
        }


        static internal NewMechanics.ComeAndGetMe createComeAndGetMe()
        {
            var c = Helpers.Create<NewMechanics.ComeAndGetMe>();
            return c;
        }


        internal static BlueprintAbility[] CreateAbilityVariantsReplace(BlueprintAbility parent, string prefix, Action<BlueprintAbility> action = null, params BlueprintAbility[] variants)
        {
            var clear_variants = variants.Distinct().ToArray();
            List<BlueprintAbility> processed_spells = new List<BlueprintAbility>();
            
            foreach (var v in clear_variants)
            {
                var processed_spell = library.CopyAndAdd<BlueprintAbility>(v.AssetGuid, prefix + v.name, Helpers.MergeIds(parent.AssetGuid, v.AssetGuid));
                var variants_comp = processed_spell.GetComponent<AbilityVariants>();

                if (action != null)
                {
                    action(processed_spell);
                }
                if (variants_comp != null)
                {
                    var variant_spells = CreateAbilityVariantsReplace(parent, prefix, action, variants_comp.Variants);
                    processed_spells = processed_spells.Concat(variant_spells).ToList();
                }
                else
                {
                    processed_spell.Parent = parent;
                    processed_spell.RemoveComponents<SpellListComponent>();
                    processed_spells.Add(processed_spell);
                }
            }
            return processed_spells.ToArray();
        }


        static internal void addToFactInContextConditionHasFact(BlueprintBuff buff, BlueprintUnitFact inner_buff_to_locate = null,
                                                       Condition condition_to_add = null)
        {
            var component = buff.GetComponent<AddFactContextActions>();
            if (component == null)
            {
                return;
            }

            var activated_actions = component.Activated.Actions;

            for (int i = 0; i < activated_actions.Length; i++)
            {
                if (activated_actions[i] is Conditional)
                {
                    var c_action = (Conditional)activated_actions[i].CreateCopy();
                    for (int j = 0; j < c_action.ConditionsChecker.Conditions.Length; j++)
                    {
                        if (c_action.ConditionsChecker.Conditions[j] is ContextConditionHasFact)
                        {
                            var condition_entry = (ContextConditionHasFact)c_action.ConditionsChecker.Conditions[j];
                            var fact = condition_entry.Fact;
                            if (fact == inner_buff_to_locate)
                            {
                                c_action.ConditionsChecker.Conditions = c_action.ConditionsChecker.Conditions.AddToArray(condition_to_add);
                                c_action.ConditionsChecker.Operation = Kingmaker.ElementsSystem.Operation.Or;
                                activated_actions[i] = c_action;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
