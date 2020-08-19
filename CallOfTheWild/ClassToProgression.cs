﻿using System;
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
using Kingmaker.Designers.Mechanics.WeaponEnchants;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.Designers.Mechanics.EquipmentEnchants;
using Kingmaker.ResourceLinks;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Alignments;

namespace CallOfTheWild
{
    class ClassToProgression
    {
        static LibraryScriptableObject library => Main.library;
        public enum DomainSpellsType
        {
            NoSpells = 1,
            SpecialList = 2,
            NormalList = 3
        }

        public static void addClassToDomains(BlueprintCharacterClass class_to_add, BlueprintArchetype[] archetypes_to_add, DomainSpellsType spells_type, BlueprintFeatureSelection domain_selection)
        {
            var domains = domain_selection.AllFeatures;
            foreach (var domain_feature in domains)
            {
                addClassToFact(class_to_add, archetypes_to_add, spells_type, domain_feature);
            }
        }


        static void addClassToProgression(BlueprintCharacterClass class_to_add, BlueprintArchetype[] archetypes_to_add, DomainSpellsType spells_type, BlueprintProgression progression)
        {
            progression.Classes = progression.Classes.AddToArray(class_to_add);
            progression.Archetypes = progression.Archetypes.AddToArray(archetypes_to_add);

            foreach (var entry in progression.LevelEntries)
            {
                foreach (var feat in entry.Features)
                {
                    addClassToFact(class_to_add, archetypes_to_add, spells_type, feat);
                }
            }

            addClassToFeat(class_to_add, archetypes_to_add, spells_type, progression);
        }

        static public void addClassToFact(BlueprintCharacterClass class_to_add, BlueprintArchetype[] archetypes_to_add, DomainSpellsType spells_type, BlueprintUnitFact f)
        {
            if (f is BlueprintAbility)
            {
                addClassToAbility(class_to_add, archetypes_to_add, (f as BlueprintAbility));
            }
            else if (f is BlueprintActivatableAbility)
            {
                addClassToBuff(class_to_add, archetypes_to_add, (f as BlueprintActivatableAbility).Buff);
            }
            else if (f is BlueprintFeatureSelection)
            {
                if ((f as BlueprintFeatureSelection).Group == FeatureGroup.Feat 
                    || (f as BlueprintFeatureSelection).Group == FeatureGroup.WizardFeat
                    || (f as BlueprintFeatureSelection).Group == FeatureGroup.RogueTalent
                    )
                {
                    return;
                }
                foreach (var af in (f as BlueprintFeatureSelection).AllFeatures)
                {
                    if (af.HasGroup(FeatureGroup.Feat, FeatureGroup.WizardFeat, FeatureGroup.RagePower, FeatureGroup.RogueTalent))
                    {
                        return;
                    }
                    addClassToFact(class_to_add, archetypes_to_add, spells_type, af);
                }
                addClassToFeat(class_to_add, archetypes_to_add, spells_type, (f as BlueprintFeatureBase));
            }
            else if (f is BlueprintProgression)
            {
                addClassToProgression(class_to_add, archetypes_to_add, spells_type, (f as BlueprintProgression));
            }
            else if (f is BlueprintFeature)
            {
                addClassToFeat(class_to_add,  archetypes_to_add, spells_type, (f as BlueprintFeatureBase));
            }

            
        }


        static public void addClassToAbility(BlueprintCharacterClass class_to_add, BlueprintArchetype[] archetypes_to_add, BlueprintAbility a)
        {
            var components = a.ComponentsArray.ToArray();
            
            foreach (var c in components.ToArray())
            {
                if (c is AbilityVariants)
                {
                    foreach (var v in (c as AbilityVariants).Variants)
                    {
                        addClassToAbility(class_to_add, archetypes_to_add, v);
                    }
                }
                else if (c is ContextRankConfig)
                {
                    addClassToContextRankConfig(class_to_add, archetypes_to_add, c as ContextRankConfig, a.name);
                }
                else if (c is AbilityEffectRunAction)
                {
                    addClassToActionList(class_to_add, archetypes_to_add, (c as AbilityEffectRunAction).Actions);
                }
                else if (c is ContextCalculateAbilityParamsBasedOnClass)
                {
                    var c_typed = c as ContextCalculateAbilityParamsBasedOnClass;
                    a.ReplaceComponent(c, Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(new BlueprintCharacterClass[] { c_typed.CharacterClass, class_to_add }, archetypes_to_add, c_typed.StatType));
                }
                else if (c is NewMechanics.ContextCalculateAbilityParamsBasedOnClasses)
                {
                    var c_typed = c as NewMechanics.ContextCalculateAbilityParamsBasedOnClasses;
                    a.ReplaceComponent(c, Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(c_typed.CharacterClasses.AddToArray(class_to_add), c_typed.archetypes.AddToArray(archetypes_to_add), c_typed.StatType));
                }
                else if (c is AbilityEffectStickyTouch)
                {
                    addClassToAbility(class_to_add, archetypes_to_add, (c as AbilityEffectStickyTouch).TouchDeliveryAbility);
                }
            }
        }


        static void addClassToActionList(BlueprintCharacterClass class_to_add, BlueprintArchetype[] archetypes_to_add,  ActionList action_list)
        {
            foreach (var a in action_list.Actions)
            {
                if (a == null)
                {
                    continue;
                }
                if (a is ContextActionApplyBuff)
                {
                    addClassToBuff(class_to_add, archetypes_to_add, (a as ContextActionApplyBuff).Buff);
                }
                else if (a is ContextActionSpawnAreaEffect)
                {
                    addClassToAreaEffect(class_to_add, archetypes_to_add, (a as ContextActionSpawnAreaEffect).AreaEffect);
                }
            }
        }


        static void addClassToContextRankConfig(BlueprintCharacterClass class_to_add, BlueprintArchetype[] archetypes_to_add, ContextRankConfig c, string archetypes_list_prefix)
        {
            var classes = Helpers.GetField<BlueprintCharacterClass[]>(c, "m_Class");
            
            if (classes != null && !classes.Empty())
            {
                classes = classes.AddToArray(class_to_add);
                Helpers.SetField(c, "m_Class", classes);
            }

            if (!archetypes_to_add.Empty())
            {
                var base_value_type = Helpers.GetField<ContextRankBaseValueType>(c, "m_BaseValueType");
                var rank_type = Helpers.GetField<AbilityRankType>(c, "m_Type");
                if (base_value_type == ContextRankBaseValueType.ClassLevel || base_value_type == ContextRankBaseValueType.SummClassLevelWithArchetype)
                {
                    var archetypes_list = Helpers.CreateFeature(archetypes_list_prefix + rank_type.ToString() + "ArchetypesListFeature",
                                            "",
                                            "",
                                            "",
                                            null,
                                            FeatureGroup.None,
                                            Helpers.Create<ContextRankConfigArchetypeList>(a => a.archetypes = archetypes_to_add)
                                            );
                    Helpers.SetField(c, "m_BaseValueType",ContextRankBaseValueTypeExtender.SummClassLevelWithArchetypes.ToContextRankBaseValueType());
                    Helpers.SetField(c, "m_Feature", archetypes_list);
                }
                else if (base_value_type == ContextRankBaseValueType.MaxClassLevelWithArchetype)
                {
                    var archetypes_list = Helpers.CreateFeature(archetypes_list_prefix + rank_type.ToString() + "ArchetypesListFeature",
                        "",
                        "",
                        "",
                        null,
                        FeatureGroup.None,
                        Helpers.Create<ContextRankConfigArchetypeList>(a => a.archetypes = archetypes_to_add)
                        );
                    Helpers.SetField(c, "m_BaseValueType", ContextRankBaseValueTypeExtender.MaxClassLevelWithArchetypes.ToContextRankBaseValueType());
                    Helpers.SetField(c, "m_Feature", archetypes_list);
                }
                else if (base_value_type == ContextRankBaseValueTypeExtender.SummClassLevelWithArchetypes.ToContextRankBaseValueType() ||
                         base_value_type == ContextRankBaseValueTypeExtender.MaxClassLevelWithArchetypes.ToContextRankBaseValueType())
                {
                    var archetypes_list = Helpers.GetField<BlueprintFeature>(c, "m_Feature");
                    archetypes_list.ReplaceComponent<ContextRankConfigArchetypeList>(a => a.archetypes = a.archetypes.AddToArray(archetypes_to_add));
                }
            }
        }


        static void addClassToBuff(BlueprintCharacterClass class_to_add, BlueprintArchetype[] archetypes_to_add, BlueprintBuff b)
        {
            var components = b.ComponentsArray;
            foreach (var c in components.ToArray())
            {
                if (c is ContextRankConfig)
                {
                    addClassToContextRankConfig(class_to_add, archetypes_to_add, c as ContextRankConfig, b.name);
                }
                else if (c is Kingmaker.UnitLogic.Buffs.Components.AddAreaEffect)
                {
                    addClassToAreaEffect(class_to_add, archetypes_to_add, (c as AddAreaEffect).AreaEffect);
                }
                else if (c is ContextCalculateAbilityParamsBasedOnClass)
                {
                    var c_typed = c as ContextCalculateAbilityParamsBasedOnClass;
                    b.ReplaceComponent(c, Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(new BlueprintCharacterClass[] { c_typed.CharacterClass, class_to_add }, archetypes_to_add, c_typed.StatType));
                }
                else if (c is NewMechanics.ContextCalculateAbilityParamsBasedOnClasses)
                {
                    var c_typed = c as NewMechanics.ContextCalculateAbilityParamsBasedOnClasses;
                    b.ReplaceComponent(c, Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(c_typed.CharacterClasses.AddToArray(class_to_add), c_typed.archetypes.AddToArray(archetypes_to_add), c_typed.StatType));
                }
                else if (c is AddFactContextActions)
                {
                    var c_typed = c as AddFactContextActions;
                    if (c_typed.NewRound != null)
                    {
                        addClassToActionList(class_to_add, archetypes_to_add, c_typed.NewRound);
                    }
                    if (c_typed.Activated != null)
                    {
                        addClassToActionList(class_to_add, archetypes_to_add, c_typed.Activated);
                    }
                    if (c_typed.Deactivated != null)
                    {
                        addClassToActionList(class_to_add, archetypes_to_add, c_typed.Deactivated);
                    }
                }
            }
        }


        static void addClassToAreaEffect(BlueprintCharacterClass class_to_add, BlueprintArchetype[] archetypes_to_add, BlueprintAbilityAreaEffect a)
        {
            var components = a.ComponentsArray;
            foreach (var c in components)
            {
                if (c is ContextRankConfig)
                {
                    addClassToContextRankConfig(class_to_add, archetypes_to_add, c as ContextRankConfig, a.name);
                }
                else if (c is AbilityAreaEffectBuff)
                {
                    addClassToBuff(class_to_add, archetypes_to_add, (c as AbilityAreaEffectBuff).Buff);
                }
                else if (c is AbilityAreaEffectRunAction)
                {
                    var c_typed = c as AbilityAreaEffectRunAction;
                    if (c_typed.Round != null)
                    {
                        addClassToActionList(class_to_add, archetypes_to_add, c_typed.Round);
                    }
                    if (c_typed.UnitEnter != null)
                    {
                        addClassToActionList(class_to_add, archetypes_to_add, c_typed.UnitEnter);
                    }
                    if (c_typed.UnitEnter != null)
                    {
                        addClassToActionList(class_to_add, archetypes_to_add, c_typed.UnitExit);
                    }
                    if (c_typed.UnitMove != null)
                    {
                        addClassToActionList(class_to_add, archetypes_to_add, c_typed.UnitMove);
                    }
                }
            }
        }


        static void addClassToResource(BlueprintCharacterClass class_to_add, BlueprintArchetype[] archetypes_to_add, BlueprintAbilityResource rsc)
        {
            //lueprintCharacterClass cleric_class = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var amount = ExtensionMethods.getMaxAmount(rsc);
            var classes = Helpers.GetField<BlueprintCharacterClass[]>(amount, "Class");
            var archetypes = Helpers.GetField<BlueprintArchetype[]>(amount, "Archetypes");

            if (classes != null && !classes.Empty())
            {
                classes = classes.AddToArray(class_to_add);
                archetypes = archetypes.AddToArray(archetypes_to_add);
                Helpers.SetField(amount, "Class", classes);
                Helpers.SetField(amount, "Archetypes", archetypes);
                ExtensionMethods.setMaxAmount(rsc, amount);
            }

            var classes_div = Helpers.GetField<BlueprintCharacterClass[]>(amount, "ClassDiv");
            var archetypes_div = Helpers.GetField<BlueprintArchetype[]>(amount, "ArchetypesDiv");

            if (classes_div != null && !classes_div.Empty())
            {
                classes_div = classes_div.AddToArray(class_to_add);
                archetypes_div = archetypes_div.AddToArray(archetypes_to_add);
                Helpers.SetField(amount, "ClassDiv", classes_div);
                Helpers.SetField(amount, "ArchetypesDiv", archetypes_div);
                ExtensionMethods.setMaxAmount(rsc, amount);
            }
        }


        static public void addClassToFeat(BlueprintCharacterClass class_to_add, BlueprintArchetype[] archetypes_to_add, DomainSpellsType spells_type, BlueprintFeatureBase feat)
        {
            foreach (var c in feat.ComponentsArray.ToArray())
            {
                if (c is IncreaseSpellDamageByClassLevel)
                {
                    var c_typed = c as IncreaseSpellDamageByClassLevel;
                    c_typed.AdditionalClasses = c_typed.AdditionalClasses.AddToArray(class_to_add);
                    c_typed.Archetypes = c_typed.Archetypes.AddToArray(archetypes_to_add);
                }
                if (c is AddFeatureOnApply)
                {
                    var c_typed = c as AddFeatureOnApply;
                    addClassToFact(class_to_add, archetypes_to_add, spells_type, c_typed.Feature);
                }
                else if (c is AddFeatureOnClassLevel)
                {
                    var c_typed = c as AddFeatureOnClassLevel;
                    c_typed.AdditionalClasses = c_typed.AdditionalClasses.AddToArray(class_to_add);
                    c_typed.Archetypes = c_typed.Archetypes.AddToArray(archetypes_to_add);
                    addClassToFact(class_to_add, archetypes_to_add, spells_type, c_typed.Feature as BlueprintUnitFact);
                }
                else if (c is LevelUpMechanics.AddFeatureOnClassLevelRange)
                {
                    var c_typed = c as LevelUpMechanics.AddFeatureOnClassLevelRange;
                    c_typed.classes = c_typed.classes.AddToArray(class_to_add);
                    c_typed.archetypes = c_typed.archetypes.AddToArray(archetypes_to_add);
                    if (!c_typed.class_bonuses.Empty())
                    {
                        c_typed.class_bonuses = c_typed.class_bonuses.AddToArray(c_typed.class_bonuses.Last());
                    }
                    addClassToFact(class_to_add, archetypes_to_add, spells_type, c_typed.Feature as BlueprintUnitFact);
                }
                else if (c is AddFeatureIfHasFact)
                {
                    var c_typed = c as AddFeatureIfHasFact;
                    addClassToFact(class_to_add, archetypes_to_add, spells_type, c_typed.Feature as BlueprintUnitFact);
                }
                else if (c is AddSpecialSpellList && spells_type == DomainSpellsType.SpecialList)
                {
                    var c_typed = c as AddSpecialSpellList;
                    if (c_typed.CharacterClass != class_to_add)
                    {
                        var c2 = Helpers.Create<Kingmaker.UnitLogic.FactLogic.AddSpecialSpellList>();
                        c2.CharacterClass = class_to_add;
                        c2.SpellList = c_typed.SpellList;
                        feat.AddComponent(c2);
                    }
                }
                else if (c is AddOppositionSchool && spells_type == DomainSpellsType.SpecialList)
                {
                    var c_typed = c as AddOppositionSchool;
                    if (c_typed.CharacterClass != class_to_add)
                    {
                        var c2 = Helpers.Create<AddOppositionSchool>();
                        c2.CharacterClass = class_to_add;
                        c2.School = c_typed.School;
                        feat.AddComponent(c2);
                    }
                }
                else if (c is Kingmaker.UnitLogic.FactLogic.AddFacts)
                {
                    var c_typed = (Kingmaker.UnitLogic.FactLogic.AddFacts)c;
                    foreach (var f in c_typed.Facts)
                    {
                        addClassToFact(class_to_add, archetypes_to_add, spells_type, f);
                    }
                }
                else if (c is AddAbilityResources)
                {
                    var c_typed = (Kingmaker.Designers.Mechanics.Facts.AddAbilityResources)c;
                    addClassToResource(class_to_add, archetypes_to_add, c_typed.Resource);
                }
                else if (c is FactSinglify)
                {
                    var c_typed = c as FactSinglify;
                    foreach (var f in c_typed.NewFacts)
                    {
                        addClassToFact(class_to_add, archetypes_to_add, spells_type, f);
                    }
                }
                else if (c is ReplaceCasterLevelOfAbility)
                {
                    var c_typed = c as ReplaceCasterLevelOfAbility;

                    c_typed.AdditionalClasses = c_typed.AdditionalClasses.AddToArray(class_to_add);
                    c_typed.Archetypes = c_typed.Archetypes.AddToArray(archetypes_to_add);
                }
                else if (c is BindAbilitiesToClass)
                {
                    var c_typed = c as BindAbilitiesToClass;

                    c_typed.AdditionalClasses = c_typed.AdditionalClasses.AddToArray(class_to_add);
                    c_typed.Archetypes = c_typed.Archetypes.AddToArray(archetypes_to_add);
                }
                else if (c is ContextRankConfig)
                {
                    addClassToContextRankConfig(class_to_add, archetypes_to_add, c as ContextRankConfig, feat.name);
                }
                else if (c is IntenseSpells)
                {
                    feat.AddComponent(Helpers.Create<NewMechanics.IntenseSpellsForClasses>(i => i.classes = new BlueprintCharacterClass[] { class_to_add, (c as IntenseSpells).Wizard }));
                    feat.RemoveComponent(c);
                }
                else if (c is NewMechanics.IntenseSpellsForClasses)
                {
                    var c_typed = c as NewMechanics.IntenseSpellsForClasses;
                    if (!c_typed.classes.Contains(class_to_add))
                    {
                        c_typed.classes = c_typed.classes.AddToArray(class_to_add);
                    }
                }
                else if (c is AddClassLevelToSummonDuration)
                {
                    feat.AddComponent(Helpers.Create<NewMechanics.AddClassesLevelToSummonDuration>(i => i.CharacterClasses = new BlueprintCharacterClass[] { class_to_add, (c as AddClassLevelToSummonDuration).CharacterClass }));
                    feat.RemoveComponent(c);
                }
                else if (c is NewMechanics.AddClassesLevelToSummonDuration)
                {
                    var c_typed = c as NewMechanics.AddClassesLevelToSummonDuration;
                    if (!c_typed.CharacterClasses.Contains(class_to_add))
                    {
                        c_typed.CharacterClasses = c_typed.CharacterClasses.AddToArray(class_to_add);
                    }
                }
                else if (c is LearnSpellList && spells_type == DomainSpellsType.NormalList)
                {
                    var spell_list = (c as LearnSpellList)?.SpellList;
                    if (spell_list == null)
                    {
                        continue;
                    }
                    if (archetypes_to_add.Empty())
                    {
                        var learn_spells_fact = Helpers.Create<Kingmaker.UnitLogic.FactLogic.LearnSpellList>();
                        learn_spells_fact.SpellList = spell_list;
                        learn_spells_fact.CharacterClass = class_to_add;
                        feat.AddComponent(learn_spells_fact);

                    }
                    else
                    {
                        foreach (var ar_type in archetypes_to_add)
                        {
                            var learn_spells_fact = Helpers.Create<Kingmaker.UnitLogic.FactLogic.LearnSpellList>();
                            learn_spells_fact.SpellList = spell_list;
                            learn_spells_fact.CharacterClass = class_to_add;
                            learn_spells_fact.Archetype = ar_type;
                            feat.AddComponent(learn_spells_fact);
                        }
                    }
                }
                else if (c is AddKnownSpell && spells_type == DomainSpellsType.NormalList)
                {
                    if (archetypes_to_add.Empty())
                    {
                        var learn_spells_fact = Helpers.Create<AddKnownSpell>();
                        learn_spells_fact.Spell = (c as AddKnownSpell).Spell;
                        learn_spells_fact.SpellLevel = (c as AddKnownSpell).SpellLevel;
                        learn_spells_fact.CharacterClass = class_to_add;
                        feat.AddComponent(learn_spells_fact);

                    }
                    else
                    {
                        foreach (var ar_type in archetypes_to_add)
                        {
                            var learn_spells_fact = Helpers.Create<AddKnownSpell>();
                            learn_spells_fact.Spell = (c as AddKnownSpell).Spell;
                            learn_spells_fact.SpellLevel = (c as AddKnownSpell).SpellLevel;
                            learn_spells_fact.CharacterClass = class_to_add;
                            learn_spells_fact.Archetype = ar_type;
                            feat.AddComponent(learn_spells_fact);
                        }
                    }
                }
            }
        }



        [Harmony12.HarmonyPatch(typeof(AddSpecialSpellList))]
        [Harmony12.HarmonyPatch("OnFactActivate", Harmony12.MethodType.Normal)]
        class AddSpecialSpellList__OnFactActivate__Patch
        {
            static bool Prefix(AddSpecialSpellList __instance)
            {
                Main.TraceLog();
                if (__instance.Owner.GetSpellbook(__instance.CharacterClass) == null)
                {
                    return false;
                }

                return true;
            }
        }


        [Harmony12.HarmonyPatch(typeof(AddOppositionSchool))]
        [Harmony12.HarmonyPatch("OnFactActivate", Harmony12.MethodType.Normal)]
        class AddOppositionSchool__OnFactActivate__Patch
        {
            static bool Prefix(AddOppositionSchool __instance)
            {
                Main.TraceLog();
                if (__instance.Owner.GetSpellbook(__instance.CharacterClass) == null)
                {
                    return false;
                }

                return true;
            }
        }
    }
}
