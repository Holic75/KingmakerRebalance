// Copyright (c) 2019 Jennifer Messerly
// Copyright (c) 2020 Denis Biryukov
// This code is licensed under MIT license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Blueprints.Validation;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Designers.Mechanics.Recommendations;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI;
using Kingmaker.UI.Log;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.Utility;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Designers.Mechanics.Prerequisites;

namespace CallOfTheWild
{
    // A few notes discovered while debugging issues:
    //
    // - don't import Harmony12, ever! It has an extension method called "Add" that can
    //   be selected over `List<T>.Add` when you should get a downcast error.
    //   This silences the compiler, and leads to difficult to debug failures (item not added).
    // - don't use Harmony12.Traverse! It silently ignores failures. For example, you try to set
    //   a field but the name is wrong. There's no error, but the feature won't work and may
    //   lead to exceptions in the game code.
    // - stack overflows lead to silent crashes, e.g. if a helpers points at itself instead of
    //   another overload, the game will silently crash.
    // - in most cases, exceptions will be reported to UnityModManager.log or GameLogFull.txt
    //   and those can be debugged.
    //
    public static class ExtensionMethods
    {
        public static V PutIfAbsent<K, V>(this IDictionary<K, V> self, K key, V value) where V : class
        {
            V oldValue;
            if (!self.TryGetValue(key, out oldValue))
            {
                self.Add(key, value);
                return value;
            }
            return oldValue;
        }

        public static V PutIfAbsent<K, V>(this IDictionary<K, V> self, K key, Func<V> ifAbsent) where V : class
        {
            V value;
            if (!self.TryGetValue(key, out value))
            {
                self.Add(key, value = ifAbsent());
                return value;
            }
            return value;
        }

        public static T[] AddToArray<T>(this T[] array, T value)
        {
            var len = array.Length;
            var result = new T[len + 1];
            Array.Copy(array, result, len);
            result[len] = value;
            return result;
        }


        public static T[] RemoveFromArrayByType<T, V>(this T[] array)
        {
            List<T> list = new List<T>();

            foreach (var c in array)
            {
                if (!(c is V))
                {
                    list.Add(c);
                }
            }

            return list.ToArray();
        }


        public static T[] AddToArray<T>(this T[] array, params T[] values)
        {
            var len = array.Length;
            var valueLen = values.Length;
            var result = new T[len + valueLen];
            Array.Copy(array, result, len);
            Array.Copy(values, 0, result, len, valueLen);
            return result;
        }


        public static T[] AddToArray<T>(this T[] array, IEnumerable<T> values) => AddToArray(array, values.ToArray());

        public static T[] RemoveFromArray<T>(this T[] array, T value)
        {
            var list = array.ToList();
            return list.Remove(value) ? list.ToArray() : array;
        }

        public static string StringJoin<T>(this IEnumerable<T> array, Func<T, string> map, string separator = " ") => string.Join(separator, array.Select(map));

        static readonly FastSetter blueprintScriptableObject_set_AssetId = Helpers.CreateFieldSetter<BlueprintScriptableObject>("m_AssetGuid");

#if DEBUG
        static readonly Dictionary<String, BlueprintScriptableObject> assetsByName = new Dictionary<String, BlueprintScriptableObject>();

        internal static readonly List<BlueprintScriptableObject> newAssets = new List<BlueprintScriptableObject>();
#endif

        public static void AddAsset(this LibraryScriptableObject library, BlueprintScriptableObject blueprint, String guid)
        {
            if (guid == "")
            {
                guid = Helpers.GuidStorage.getGuid(blueprint.name);
            }
            else if (guid[0] == '$')
            {
                guid = Helpers.GuidStorage.maybeGetGuid(blueprint.name, guid.Remove(0));
            }
            blueprintScriptableObject_set_AssetId(blueprint, guid);
            // Sanity check that we don't stop on our own GUIDs or someone else's.
            BlueprintScriptableObject existing;
            if (library.BlueprintsByAssetId.TryGetValue(guid, out existing))
            {
                throw Main.Error($"Duplicate AssetId for {blueprint.name}, existing entry ID: {guid}, name: {existing.name}, type: {existing.GetType().Name}");
            }
            else if (guid == "")
            {
                throw Main.Error($"Missing AssetId: {guid}, name: {existing.name}, type: {existing.GetType().Name}");
            }
#if DEBUG
            newAssets.Add(blueprint);
#endif
#if false
            // Sanity check that names are unique. This is less important, but the feat selection UI
            // gets confused if multiple entries have the same name.
            if (assetsByName.TryGetValue(blueprint.name, out existing))
            {
                Log.Write($"Warning: Duplicate name, existing entry ID: {existing.AssetGuid}, name: {existing.name}, type: {existing.GetType().Name}");
            }
            else
            {
                assetsByName.Add(blueprint.name, blueprint);
            }
#endif

            library.GetAllBlueprints().Add(blueprint);
            library.BlueprintsByAssetId[guid] = blueprint;
            Helpers.GuidStorage.addEntry(blueprint.name, guid);
        }

        public static void SetFeatures(this BlueprintFeatureSelection selection, IEnumerable<BlueprintFeature> features)
        {
            SetFeatures(selection, features.ToArray());
        }

        public static void SetFeatures(this BlueprintFeatureSelection selection, params BlueprintFeature[] features)
        {
            selection.AllFeatures = selection.Features = features;
        }

        public static void InsertComponent(this BlueprintScriptableObject obj, int index, BlueprintComponent component)
        {
            var components = obj.ComponentsArray.ToList();
            components.Insert(index, component);
            obj.SetComponents(components);
        }

        public static void AddComponent(this BlueprintScriptableObject obj, BlueprintComponent component)
        {
            obj.SetComponents(obj.ComponentsArray.AddToArray(component));
        }

        public static void RemoveComponent(this BlueprintScriptableObject obj, BlueprintComponent component)
        {
            obj.SetComponents(obj.ComponentsArray.RemoveFromArray(component));
        }


        public static void RemoveComponents<T>(this BlueprintScriptableObject obj) where T : BlueprintComponent
        {
            var compnents_to_remove = obj.GetComponents<T>().ToArray();
            foreach (var c in compnents_to_remove)
            {
                obj.SetComponents(obj.ComponentsArray.RemoveFromArray(c));
            }
        }


        public static void RemoveComponents<T>(this BlueprintScriptableObject obj, Predicate<T> predicate) where T : BlueprintComponent
        {
            var compnents_to_remove = obj.GetComponents<T>().ToArray();
            foreach (var c in compnents_to_remove)
            {
                if (predicate(c))
                {
                    obj.SetComponents(obj.ComponentsArray.RemoveFromArray(c));
                }
            }
        }

        public static void AddComponents(this BlueprintScriptableObject obj, IEnumerable<BlueprintComponent> components) => AddComponents(obj, components.ToArray());

        public static void AddComponents(this BlueprintScriptableObject obj, params BlueprintComponent[] components)
        {
            var c = obj.ComponentsArray.ToList();
            c.AddRange(components);
            obj.SetComponents(c.ToArray());
        }

        public static void SetComponents(this BlueprintScriptableObject obj, params BlueprintComponent[] components)
        {
            // Fix names of components. Generally this doesn't matter, but if they have serialization state,
            // then their name needs to be unique.
            var names = new HashSet<string>();
            foreach (var c in components)
            {
                if (string.IsNullOrEmpty(c.name))
                {
                    c.name = $"${c.GetType().Name}";
                }
                if (!names.Add(c.name))
                {
                    SaveCompatibility.CheckComponent(obj, c);
                    String name;
                    for (int i = 0; !names.Add(name = $"{c.name}${i}"); i++) ;
                    c.name = name;
                }
                Log.Validate(c, obj);
            }

            obj.ComponentsArray = components;
        }

        public static void SetComponents(this BlueprintScriptableObject obj, IEnumerable<BlueprintComponent> components)
        {
            SetComponents(obj, components.ToArray());
        }

        public static void AddAsset(this LibraryScriptableObject library, BlueprintScriptableObject blueprint, String guid1, String guid2)
        {
            library.AddAsset(blueprint, Helpers.MergeIds(guid1, guid2));
        }

        public static T Get<T>(this LibraryScriptableObject library, String assetId) where T : BlueprintScriptableObject
        {
            return (T)library.BlueprintsByAssetId[assetId];
        }

        public static T TryGet<T>(this LibraryScriptableObject library, String assetId) where T : BlueprintScriptableObject
        {
            BlueprintScriptableObject result;
            if (library.BlueprintsByAssetId.TryGetValue(assetId, out result))
            {
                return (T)result;
            }
            return null;
        }

        public static T CopyAndAdd<T>(this LibraryScriptableObject library, String assetId, String newName, String newAssetId, String newAssetId2 = null) where T : BlueprintScriptableObject
        {
            return CopyAndAdd(library, Get<T>(library, assetId), newName, newAssetId, newAssetId2);
        }

        public static T CopyAndAdd<T>(this LibraryScriptableObject library, T original, String newName, String newAssetId, String newAssetId2 = null) where T : BlueprintScriptableObject
        {
            var clone = UnityEngine.Object.Instantiate(original);
            clone.name = newName;
            var id = newAssetId2 != null ? Helpers.MergeIds(newAssetId, newAssetId2) : newAssetId;
            AddAsset(library, clone, id);
            return clone;
        }


        public static T CreateCopy<T>(this T original, Action<T> action = null) where T : UnityEngine.Object
        {
            var clone = UnityEngine.Object.Instantiate(original);
            if (action != null)
            {
                action(clone);
            }
            return clone;
        }

        static readonly FastSetter blueprintUnitFact_set_Description = Helpers.CreateFieldSetter<BlueprintUnitFact>("m_Description");
        static readonly FastSetter blueprintItem_set_Description = Helpers.CreateFieldSetter<BlueprintItem>("m_DescriptionText");
        static readonly FastSetter blueprintUnitFact_set_Icon = Helpers.CreateFieldSetter<BlueprintUnitFact>("m_Icon");
        static readonly FastSetter blueprintUnitFact_set_DisplayName = Helpers.CreateFieldSetter<BlueprintUnitFact>("m_DisplayName");
        static readonly FastGetter blueprintUnitFact_get_Description = Helpers.CreateFieldGetter<BlueprintUnitFact>("m_Description");
        static readonly FastGetter blueprintUnitFact_get_DisplayName = Helpers.CreateFieldGetter<BlueprintUnitFact>("m_DisplayName");

        public static void SetNameDescriptionIcon(this BlueprintUnitFact feature, String displayName, String description, Sprite icon)
        {
            SetNameDescription(feature, displayName, description);
            feature.SetIcon(icon);
        }

        public static void SetNameDescriptionIcon(this BlueprintUnitFact feature, BlueprintUnitFact other)
        {
            SetNameDescription(feature, other);
            feature.SetIcon(other.Icon);
        }

        public static void SetNameDescription(this BlueprintUnitFact feature, String displayName, String description)
        {
            feature.SetName(Helpers.CreateString(feature.name + ".Name", displayName));
            feature.SetDescription(description);
        }

        public static void SetNameDescription(this BlueprintUnitFact feature, BlueprintUnitFact other)
        {
            blueprintUnitFact_set_DisplayName(feature, other.GetName());
            blueprintUnitFact_set_Description(feature, other.GetDescription());
        }

        public static LocalizedString GetName(this BlueprintUnitFact fact) => (LocalizedString)blueprintUnitFact_get_DisplayName(fact);
        public static LocalizedString GetDescription(this BlueprintUnitFact fact) => (LocalizedString)blueprintUnitFact_get_Description(fact);

        public static void SetIcon(this BlueprintUnitFact feature, Sprite icon)
        {
            blueprintUnitFact_set_Icon(feature, icon);
        }

        public static void SetName(this BlueprintUnitFact feature, LocalizedString name)
        {
            blueprintUnitFact_set_DisplayName(feature, name);
        }

        public static void SetName(this BlueprintUnitFact feature, String name)
        {
            blueprintUnitFact_set_DisplayName(feature, Helpers.CreateString(feature.name + ".Name", name));
        }

        public static void SetDescription(this BlueprintUnitFact feature, String description)
        {
            blueprintUnitFact_set_Description(feature, Helpers.CreateString(feature.name + ".Description", description));
        }

        public static void SetDescription(this BlueprintItem item, String description)
        {
            blueprintItem_set_Description(item, Helpers.CreateString(item.name + ".Description", description));
        }

        public static void SetDescription(this BlueprintUnitFact feature, LocalizedString description)
        {
            blueprintUnitFact_set_Description(feature, description);
        }

        public static bool HasFeatureWithId(this LevelEntry level, String id)
        {
            return level.Features.Any(f => HasFeatureWithId(f, id));
        }

        public static bool HasFeatureWithId(this BlueprintUnitFact fact, String id)
        {
            if (fact.AssetGuid == id) return true;
            foreach (var c in fact.ComponentsArray)
            {
                var addFacts = c as AddFacts;
                if (addFacts != null) return addFacts.Facts.Any(f => HasFeatureWithId(f, id));
            }
            return false;
        }

        public static CasterSpellProgression GetCasterSpellProgression(this BlueprintSpellbook spellbook)
        {
            var spellsPerDay = spellbook.SpellsPerDay;
            if (spellsPerDay.GetCount(6, 3).HasValue)
            {
                return CasterSpellProgression.FullCaster;
            }
            else if (spellsPerDay.GetCount(7, 3).HasValue)
            {
                return CasterSpellProgression.ThreeQuartersCaster;
            }
            else if (spellsPerDay.GetCount(10, 3).HasValue)
            {
                return CasterSpellProgression.HalfCaster;
            }
            return CasterSpellProgression.UnknownCaster;
        }

        public static CasterSpellProgression GetCasterSpellProgression(this BlueprintSpellList spellList)
        {
            if (spellList.GetSpells(9).Count > 0)
            {
                return CasterSpellProgression.FullCaster;
            }
            else if (spellList.GetSpells(6).Count > 0)
            {
                return CasterSpellProgression.ThreeQuartersCaster;
            }
            else if (spellList.GetSpells(4).Count > 0)
            {
                return CasterSpellProgression.HalfCaster;
            }
            return CasterSpellProgression.UnknownCaster;
        }


        public static void setMiscAbilityParametersSingleTargetRangedHarmful(this BlueprintAbility ability, bool works_on_allies =  false,
                                                               Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Point,
                                                               Kingmaker.View.Animation.CastAnimationStyle animation_style = Kingmaker.View.Animation.CastAnimationStyle.CastActionPoint)
        {
            ability.CanTargetFriends = works_on_allies;
            ability.CanTargetEnemies = true;
            ability.CanTargetSelf = false;
            ability.CanTargetPoint = false;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.EffectOnAlly = works_on_allies ? AbilityEffectOnUnit.Harmful : AbilityEffectOnUnit.None;
            ability.Animation = animation;
            ability.AnimationStyle = animation_style;
        }


        public static void setMiscAbilityParametersSingleTargetRangedFriendly(this BlueprintAbility ability, bool works_on_self = false,
                                                       Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Point,
                                                       Kingmaker.View.Animation.CastAnimationStyle animation_style = Kingmaker.View.Animation.CastAnimationStyle.CastActionPoint)
        {
            ability.CanTargetFriends = true;
            ability.CanTargetEnemies = false;
            ability.CanTargetSelf = works_on_self;
            ability.CanTargetPoint = false;
            ability.EffectOnEnemy = AbilityEffectOnUnit.None;
            ability.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            ability.Animation = animation;
            ability.AnimationStyle = animation_style;
        }


        public static void setMiscAbilityParametersTouchHarmful(this BlueprintAbility ability, bool works_on_allies = true,
                                                       Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Touch,
                                                       Kingmaker.View.Animation.CastAnimationStyle animation_style = Kingmaker.View.Animation.CastAnimationStyle.CastActionTouch)
        {
            ability.CanTargetFriends = works_on_allies;
            ability.CanTargetEnemies = true;
            ability.CanTargetSelf = works_on_allies;
            ability.CanTargetPoint = false;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.EffectOnAlly = works_on_allies ? AbilityEffectOnUnit.Harmful : AbilityEffectOnUnit.None;
            ability.Animation = animation;
            ability.AnimationStyle = animation_style;
        }


        public static void setMiscAbilityParametersTouchFriendly(this BlueprintAbility ability, bool works_on_self = true,
                                               Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Touch,
                                               Kingmaker.View.Animation.CastAnimationStyle animation_style = Kingmaker.View.Animation.CastAnimationStyle.CastActionTouch)
        {
            ability.CanTargetFriends = true;
            ability.CanTargetEnemies = false;
            ability.CanTargetSelf = works_on_self;
            ability.CanTargetPoint = false;
            ability.EffectOnEnemy = AbilityEffectOnUnit.None;
            ability.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            ability.Animation = animation;
            ability.AnimationStyle = animation_style;
        }


        public static void setMiscAbilityParametersRangedDirectional(this BlueprintAbility ability, bool works_on_units = true,
                                                                     AbilityEffectOnUnit effect_on_ally = AbilityEffectOnUnit.Harmful,
                                                                     AbilityEffectOnUnit effect_on_enemy = AbilityEffectOnUnit.Harmful,
                                                                     Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Directional,
                                                                     Kingmaker.View.Animation.CastAnimationStyle animation_style = Kingmaker.View.Animation.CastAnimationStyle.CastActionDirectional)
        { 
            ability.CanTargetFriends = works_on_units;
            ability.CanTargetEnemies = works_on_units;
            ability.CanTargetSelf = works_on_units;
            ability.CanTargetPoint = true;
            ability.EffectOnEnemy = effect_on_enemy;
            ability.EffectOnAlly = effect_on_ally;
            ability.Animation = animation;
            ability.AnimationStyle = animation_style;
        }


        public static void setMiscAbilityParametersSelfOnly(this BlueprintAbility ability, 
                                                               Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Self,
                                                               Kingmaker.View.Animation.CastAnimationStyle animation_style = Kingmaker.View.Animation.CastAnimationStyle.CastActionSelf)
        {
            ability.CanTargetFriends = false;
            ability.CanTargetEnemies = false;
            ability.CanTargetSelf = true;
            ability.CanTargetPoint = false;
            ability.EffectOnEnemy = AbilityEffectOnUnit.None;
            ability.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            ability.Animation = animation;
            ability.AnimationStyle = animation_style;
        }




        static readonly FastSetter blueprintArchetype_set_Icon = Helpers.CreateFieldSetter<BlueprintArchetype>("m_Icon");

        public static void SetIcon(this BlueprintArchetype self, Sprite icon)
        {
            blueprintArchetype_set_Icon(self, icon);
        }

        public static void AddToSpellList(this BlueprintAbility spell, BlueprintSpellList spellList, int level)
        {
            var feyspeaker_spell_list = ResourcesLibrary.TryGetBlueprint<BlueprintSpellList>("640b4c89527334e45b19d884dd82e500");//feyspeaker
            var comp = Helpers.Create<SpellListComponent>();
            comp.SpellLevel = level;
            comp.SpellList = spellList;
            spell.AddComponent(comp);
            spellList.SpellsByLevel[level].Spells.Add(spell);
            if (spellList == Helpers.wizardSpellList)
            {
                var school = spell.School;
                var specialistList = specialistSchoolList.Value[(int)school];
                specialistList?.SpellsByLevel[level].Spells.Add(spell);

                for (int i = 0; i < thassilonianSchoolList.Value.Length; i++)
                {
                    if (thassilonianOpposedSchools.Value[i] != null && !thassilonianOpposedSchools.Value[i].Contains(school))
                    {
                        thassilonianSchoolList.Value[i]?.SpellsByLevel[level].Spells.Add(spell);
                    }
                }

                if (school == SpellSchool.Enchantment || school == SpellSchool.Illusion)
                {
                    feyspeaker_spell_list.SpellsByLevel[level].Spells.Add(spell);
                }
            }
        }

        static readonly Lazy<BlueprintSpellList[]> specialistSchoolList = new Lazy<BlueprintSpellList[]>(() =>
        {
            var result = new BlueprintSpellList[(int)SpellSchool.Universalist + 1];
            var library = Main.library;
            result[(int)SpellSchool.Abjuration] = library.Get<BlueprintSpellList>("c7a55e475659a944f9229d89c4dc3a8e");
            result[(int)SpellSchool.Conjuration] = library.Get<BlueprintSpellList>("69a6eba12bc77ea4191f573d63c9df12");
            result[(int)SpellSchool.Divination] = library.Get<BlueprintSpellList>("d234e68b3d34d124a9a2550fdc3de9eb");
            result[(int)SpellSchool.Enchantment] = library.Get<BlueprintSpellList>("c72836bb669f0c04680c01d88d49bb0c");
            result[(int)SpellSchool.Evocation] = library.Get<BlueprintSpellList>("79e731172a2dc1f4d92ba229c6216502");
            result[(int)SpellSchool.Illusion] = library.Get<BlueprintSpellList>("d74e55204daa9b14993b2e51ae861501");
            result[(int)SpellSchool.Necromancy] = library.Get<BlueprintSpellList>("5fe3acb6f439db9438db7d396f02c75c");
            result[(int)SpellSchool.Transmutation] = library.Get<BlueprintSpellList>("becbcfeca9624b6469319209c2a6b7f1");
            return result;
        });


        static readonly Lazy<BlueprintSpellList[]> thassilonianSchoolList = new Lazy<BlueprintSpellList[]>(() =>
        {
            var result = new BlueprintSpellList[(int)SpellSchool.Universalist + 1];
            var library = Main.library;
            result[(int)SpellSchool.Abjuration] = library.Get<BlueprintSpellList>("280dd5167ccafe449a33fbe93c7a875e");
            result[(int)SpellSchool.Conjuration] = library.Get<BlueprintSpellList>("5b154578f228c174bac546b6c29886ce");
            result[(int)SpellSchool.Enchantment] = library.Get<BlueprintSpellList>("ac551db78c1baa34eb8edca088be13cb");
            result[(int)SpellSchool.Evocation] = library.Get<BlueprintSpellList>("17c0bfe5b7c8ac3449da655cdcaed4e7");
            result[(int)SpellSchool.Illusion] = library.Get<BlueprintSpellList>("c311aed33deb7a346ab715baef4a0572");
            result[(int)SpellSchool.Necromancy] = library.Get<BlueprintSpellList>("5c08349132cb6b04181797f58ccf38ae");
            result[(int)SpellSchool.Transmutation] = library.Get<BlueprintSpellList>("f3a8f76b1d030a64084355ba3eea369a");
            return result;
        });


        static readonly Lazy<SpellSchool[][]> thassilonianOpposedSchools = new Lazy<SpellSchool[][]>(() =>
        {
            var result = new SpellSchool[(int)SpellSchool.Universalist + 1][];
           
            result[(int)SpellSchool.Abjuration] = new SpellSchool[] {SpellSchool.Evocation, SpellSchool.Necromancy };
            result[(int)SpellSchool.Conjuration] = new SpellSchool[] { SpellSchool.Evocation, SpellSchool.Illusion };
            result[(int)SpellSchool.Enchantment] = new SpellSchool[] { SpellSchool.Necromancy, SpellSchool.Transmutation };
            result[(int)SpellSchool.Evocation] = new SpellSchool[] { SpellSchool.Abjuration, SpellSchool.Conjuration };
            result[(int)SpellSchool.Illusion] = new SpellSchool[] { SpellSchool.Conjuration, SpellSchool.Transmutation };
            result[(int)SpellSchool.Necromancy] = new SpellSchool[] { SpellSchool.Abjuration, SpellSchool.Enchantment };
            result[(int)SpellSchool.Transmutation] = new SpellSchool[] { SpellSchool.Enchantment, SpellSchool.Illusion };
            return result;
        });

        public static void FixDomainSpell(this BlueprintAbility spell, int level, string spellListId)
        {
            var spellList = Main.library.Get<BlueprintSpellList>(spellListId);
            var spells = spellList.SpellsByLevel.First(s => s.SpellLevel == level).Spells;
            spells.Clear();
            spells.Add(spell);
        }


        public static bool HasAreaEffect(this BlueprintAbility spell)
        {
            return spell.AoERadius.Meters > 0f || spell.ProjectileType != AbilityProjectileType.Simple;
        }

        public static void AddSelection(this BlueprintFeatureSelection feat, LevelUpState state, UnitDescriptor unit, int level)
        {
            // TODO: we may want to add the selection feat to the unit.
            // (But I don't think Respec mod will be able to clear it out if we do that.)
            // unit.AddFact(feat);
            state.AddSelection(null, feat, feat, level);
        }

        public static void SetIcon(this BlueprintAbilityResource resource, Sprite icon) => setIcon(resource, icon);

        static readonly FastSetter setIcon = Helpers.CreateFieldSetter<BlueprintAbilityResource>("m_Icon");
        internal static readonly FastSetter setMaxAmount = Helpers.CreateFieldSetter<BlueprintAbilityResource>("m_MaxAmount");
        internal static readonly FastGetter getMaxAmount = Helpers.CreateFieldGetter<BlueprintAbilityResource>("m_MaxAmount");
        //static readonly Type blueprintAbilityResource_Amount = Harmony12.AccessTools.Inner(typeof(BlueprintAbilityResource), "Amount");

        public static void SetIncreasedByLevel(this BlueprintAbilityResource resource, int baseValue, int levelIncrease, BlueprintCharacterClass[] classes, BlueprintArchetype[] archetypes = null)
        {
            var amount = getMaxAmount(resource);
            Helpers.SetField(amount, "BaseValue", baseValue);
            Helpers.SetField(amount, "IncreasedByLevel", true);
            Helpers.SetField(amount, "LevelIncrease", levelIncrease);
            Helpers.SetField(amount, "Class", classes);
            var emptyArchetypes = Array.Empty<BlueprintArchetype>();
            Helpers.SetField(amount, "Archetypes", archetypes ?? emptyArchetypes);

            // Enusre arrays are at least initialized to empty.
            var field = "ClassDiv";
            if (Helpers.GetField(amount, field) == null) Helpers.SetField(amount, field, Array.Empty<BlueprintCharacterClass>());
            field = "ArchetypesDiv";
            if (Helpers.GetField(amount, field) == null) Helpers.SetField(amount, field, emptyArchetypes);

            setMaxAmount(resource, amount);
        }

        public static void SetFixedResource(this BlueprintAbilityResource resource, int baseValue)
        {
            var amount = getMaxAmount(resource);
            Helpers.SetField(amount, "BaseValue", baseValue);

            // Enusre arrays are at least initialized to empty.
            var emptyClasses = Array.Empty<BlueprintCharacterClass>();
            var emptyArchetypes = Array.Empty<BlueprintArchetype>();
            var field = "Class";
            if (Helpers.GetField(amount, field) == null) Helpers.SetField(amount, field, emptyClasses);
            field = "ClassDiv";
            if (Helpers.GetField(amount, field) == null) Helpers.SetField(amount, field, emptyClasses);
            field = "Archetypes";
            if (Helpers.GetField(amount, field) == null) Helpers.SetField(amount, field, emptyArchetypes);
            field = "ArchetypesDiv";
            if (Helpers.GetField(amount, field) == null) Helpers.SetField(amount, field, emptyArchetypes);

            setMaxAmount(resource, amount);
        }

        public static void SetIncreasedByStat(this BlueprintAbilityResource resource, int baseValue, StatType stat)
        {
            var amount = getMaxAmount(resource);
            Helpers.SetField(amount, "BaseValue", baseValue);
            Helpers.SetField(amount, "IncreasedByStat", true);
            Helpers.SetField(amount, "ResourceBonusStat", stat);

            // Enusre arrays are at least initialized to empty.
            var emptyClasses = Array.Empty<BlueprintCharacterClass>();
            var emptyArchetypes = Array.Empty<BlueprintArchetype>();
            var field = "Class";
            if (Helpers.GetField(amount, field) == null) Helpers.SetField(amount, field, emptyClasses);
            field = "ClassDiv";
            if (Helpers.GetField(amount, field) == null) Helpers.SetField(amount, field, emptyClasses);
            field = "Archetypes";
            if (Helpers.GetField(amount, field) == null) Helpers.SetField(amount, field, emptyArchetypes);
            field = "ArchetypesDiv";
            if (Helpers.GetField(amount, field) == null) Helpers.SetField(amount, field, emptyArchetypes);

            setMaxAmount(resource, amount);
        }

        public static void SetIncreasedByLevelStartPlusDivStep(this BlueprintAbilityResource resource, int baseValue,
            int startingLevel, int startingIncrease, int levelStep, int perStepIncrease, int minClassLevelIncrease, float otherClassesModifier,
            BlueprintCharacterClass[] classes, BlueprintArchetype[] archetypes = null)
        {
            var amount = getMaxAmount(resource);
            Helpers.SetField(amount, "BaseValue", baseValue);
            Helpers.SetField(amount, "IncreasedByLevelStartPlusDivStep", true);
            Helpers.SetField(amount, "StartingLevel", startingLevel);
            Helpers.SetField(amount, "StartingIncrease", startingIncrease);
            Helpers.SetField(amount, "LevelStep", levelStep);
            Helpers.SetField(amount, "PerStepIncrease", perStepIncrease);
            Helpers.SetField(amount, "MinClassLevelIncrease", minClassLevelIncrease);
            Helpers.SetField(amount, "OtherClassesModifier", otherClassesModifier);
            Helpers.SetField(amount, "IncreasedByLevel", false);
            Helpers.SetField(amount, "LevelIncrease", 0);

            Helpers.SetField(amount, "ClassDiv", classes);
            var emptyArchetypes = Array.Empty<BlueprintArchetype>();
            Helpers.SetField(amount, "ArchetypesDiv", archetypes ?? emptyArchetypes);

            // Enusre arrays are at least initialized to empty.
            var fieldName = "Class";
            if (Helpers.GetField(amount, fieldName) == null) Helpers.SetField(amount, fieldName, Array.Empty<BlueprintCharacterClass>());
            fieldName = "Archetypes";
            if (Helpers.GetField(amount, fieldName) == null) Helpers.SetField(amount, fieldName, emptyArchetypes);

            setMaxAmount(resource, amount);
        }

        internal static LocalizedString GetText(this DurationRate duration)
        {
            switch (duration)
            {
                case DurationRate.Rounds:
                    return Helpers.roundsPerLevelDuration;
                case DurationRate.Minutes:
                    return Helpers.minutesPerLevelDuration;
                case DurationRate.TenMinutes:
                    return Helpers.tenMinPerLevelDuration;
                case DurationRate.Hours:
                    return Helpers.hourPerLevelDuration;
            }
            throw new NotImplementedException($"DurationRate: {duration}");
        }

        internal static IEnumerable<BlueprintComponent> WithoutSpellComponents(this IEnumerable<BlueprintComponent> components)
        {
            return components.Where(c => !(c is SpellComponent) && !(c is SpellListComponent));
        }

        internal static int GetCost(this BlueprintAbility.MaterialComponentData material)
        {
            var item = material?.Item;
            return item == null ? 0 : item.Cost * material.Count;
        }

        static public BuffFlags GetBuffFlags(this BlueprintBuff buff)
        {
            return (BuffFlags)(int)Helpers.GetField(buff, "m_Flags");
        }

        static public void SetBuffFlags(this BlueprintBuff buff, BuffFlags flags)
        {
            Helpers.SetField(buff, "m_Flags", (int)flags);
        }

        public static AddConditionImmunity CreateImmunity(this UnitCondition condition)
        {
            var b = Helpers.Create<AddConditionImmunity>();
            b.Condition = condition;
            return b;
        }

        public static AddCondition CreateAddCondition(this UnitCondition condition)
        {
            var a = Helpers.Create<AddCondition>();
            a.Condition = condition;
            return a;
        }

        public static BuffDescriptorImmunity CreateBuffImmunity(this SpellDescriptor spell)
        {
            var b = Helpers.Create<BuffDescriptorImmunity>();
            b.Descriptor = spell;
            return b;
        }

        public static SpellImmunityToSpellDescriptor CreateSpellImmunity(this SpellDescriptor spell)
        {
            var s = Helpers.Create<SpellImmunityToSpellDescriptor>();
            s.Descriptor = spell;
            return s;
        }

        public static void addAction(this Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction action, Kingmaker.ElementsSystem.GameAction game_action)
        {
            if (action.Actions != null)
            {
                action.Actions = Helpers.CreateActionList(action.Actions.Actions);
                action.Actions.Actions = action.Actions.Actions.AddToArray(game_action);
            }
            else
            {
                action.Actions = Helpers.CreateActionList(game_action);
            }  
        }
    }

    [Flags]
    public enum BuffFlags
    {
        IsFromSpell = 0x1,
        HiddenInUi = 0x2,
        StayOnDeath = 0x8,
        RemoveOnRest = 0x10,
        RemoveOnResurrect = 0x20,
        Harmful = 0x40
    }

    public enum CasterSpellProgression
    {
        FullCaster,
        ThreeQuartersCaster,
        HalfCaster,
        UnknownCaster
    }

    // TODO: convert everything to instance classes/methods, and subclass Helpers (renamed, of course)?
    // Benefits:
    // - less `static` noise.
    // - less `Helpers.` etc.
    public static class Helpers
    {
        public static class GuidStorage
        {
            static Dictionary<string, string> guids_in_use = new Dictionary<string, string>();
            static bool allow_guid_generation;

            static public void load(string file_content)
            {
                load(file_content, false);
            }

            static public void load(string file_content, bool debug_mode)
            {
                allow_guid_generation = debug_mode;
                guids_in_use = new Dictionary<string, string>();
                using (System.IO.StringReader reader = new System.IO.StringReader(file_content))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] items = line.Split('\t');
                        guids_in_use.Add(items[0], items[1]);
                    }
                }
            }

            static public void dump(string guid_file_name)
            {
                using (System.IO.StreamWriter sw = System.IO.File.CreateText(guid_file_name))
                {
                    foreach (var pair in guids_in_use)
                    {
                        BlueprintScriptableObject existing;
                        Main.library.BlueprintsByAssetId.TryGetValue(pair.Value, out existing);
                        if (existing != null)
                        {
                            sw.WriteLine(pair.Key + '\t' + pair.Value + '\t' + existing.GetType().FullName);
                        }
                    }
                }
            }

            static public void addEntry(string name, string guid)
            {
                string original_guid;
                if (guids_in_use.TryGetValue(name, out original_guid))
                {
                    if (original_guid != guid)
                    {
                        throw Main.Error($"Asset: {name}, is already registered for object with another guid: {guid}");
                    }
                }
                else
                {
                    guids_in_use.Add(name, guid);
                }
            }


            static public bool hasStoredGuid(string blueprint_name)
            {
                string stored_guid = "";
                return guids_in_use.TryGetValue(blueprint_name, out stored_guid);
            }


            static public string getGuid(string name)
            {
                string original_guid;
                if (guids_in_use.TryGetValue(name, out original_guid))
                {
                    return original_guid;
                }
                else if (allow_guid_generation)
                {
                    return Guid.NewGuid().ToString("N");
                }
                else
                {
                    throw Main.Error($"Missing AssetId for: {name}"); //ensure that no guids generated in release mode
                }
            }


            static public string maybeGetGuid(string name, string new_guid = "")
            {
                string original_guid;
                if (guids_in_use.TryGetValue(name, out original_guid))
                {
                    return original_guid;
                }
                else
                {
                    return new_guid;
                }
            }

        }
        public static BlueprintFeatureSelection skillFocusFeat;

        // All classes (including prestige classes).
        public static List<BlueprintCharacterClass> classes;

        public static List<BlueprintCharacterClass> prestigeClasses;

        public static BlueprintCharacterClass sorcererClass, magusClass, dragonDiscipleClass;
        public static BlueprintArchetype eldritchScionArchetype;

        const String basicFeatSelection = "247a4068296e8be42890143f451b4b45";
        public const String magusFeatSelection = "66befe7b24c42dd458952e3c47c93563";

        public static BlueprintRace human, halfElf, halfOrc, elf, dwarf, halfling, gnome, aasimar, tiefling;

        public static LocalizedString tenMinPerLevelDuration, minutesPerLevelDuration, hourPerLevelDuration, roundsPerLevelDuration, oneRoundDuration, oneMinuteDuration;

        public static LocalizedString reflexHalfDamage, savingThrowNone, willNegates, fortNegates;

        public static BlueprintSpellList wizardSpellList, magusSpellList, druidSpellList, clericSpellList, paladinSpellList, inquisitorSpellList, alchemistSpellList, bardSpellList, rangerSpellList;

        public static BlueprintItemWeapon touchWeapon;

        public static readonly List<BlueprintAbility> allSpells = new List<BlueprintAbility>();
        public static readonly List<BlueprintAbility> modSpells = new List<BlueprintAbility>();
        public static readonly List<BlueprintAbility> spellsWithResources = new List<BlueprintAbility>();
        public static readonly List<BlueprintItemEquipmentUsable> modScrolls = new List<BlueprintItemEquipmentUsable>();

        public static readonly List<BlueprintLoot> allLoots = new List<BlueprintLoot>();
        public static readonly List<BlueprintUnitLoot> allUnitLoots = new List<BlueprintUnitLoot>();

        public static BlueprintFeatureSelection bloodlineSelection;

        public static BlueprintWeaponEnchantment ghostTouch;

        internal static void Load()
        {
            var library = Main.library;

            // For some reason, Eldritch Scion is a class and an archetype.
            const String eldritchScionClassId = "f5b8c63b141b2f44cbb8c2d7579c34f5";
            classes = library.Root.Progression.CharacterClasses.Where(c => c.AssetGuid != eldritchScionClassId).ToList();
            prestigeClasses = classes.Where(c => c.PrestigeClass).ToList();
            sorcererClass = GetClass("b3a505fb61437dc4097f43c3f8f9a4cf");
            magusClass = GetClass("45a4607686d96a1498891b3286121780");
            dragonDiscipleClass = Helpers.GetClass("72051275b1dbb2d42ba9118237794f7c");
            eldritchScionArchetype = magusClass.Archetypes.First(a => a.AssetGuid == "d078b2ef073f2814c9e338a789d97b73");

            human = library.Get<BlueprintRace>("0a5d473ead98b0646b94495af250fdc4");
            halfElf = library.Get<BlueprintRace>("b3646842ffbd01643ab4dac7479b20b0");
            halfOrc = library.Get<BlueprintRace>("1dc20e195581a804890ddc74218bfd8e");
            elf = library.Get<BlueprintRace>("25a5878d125338244896ebd3238226c8");
            dwarf = library.Get<BlueprintRace>("c4faf439f0e70bd40b5e36ee80d06be7");
            halfling = library.Get<BlueprintRace>("b0c3ef2729c498f47970bb50fa1acd30");
            gnome = library.Get<BlueprintRace>("ef35a22c9a27da345a4528f0d5889157");
            aasimar = library.Get<BlueprintRace>("b7f02ba92b363064fb873963bec275ee");
            tiefling = library.Get<BlueprintRace>("5c4e42124dc2b4647af6e36cf2590500");

            skillFocusFeat = library.Get<BlueprintFeatureSelection>("c9629ef9eebb88b479b2fbc5e836656a");

            tenMinPerLevelDuration = library.Get<BlueprintAbility>("5b77d7cc65b8ab74688e74a37fc2f553").LocalizedDuration; // barkskin
            minutesPerLevelDuration = library.Get<BlueprintAbility>("ef768022b0785eb43a18969903c537c4").LocalizedDuration; // shield
            hourPerLevelDuration = library.Get<BlueprintAbility>("9e1ad5d6f87d19e4d8883d63a6e35568").LocalizedDuration; // mage armor
            roundsPerLevelDuration = library.Get<BlueprintAbility>("486eaff58293f6441a5c2759c4872f98").LocalizedDuration; // haste
            oneRoundDuration = library.Get<BlueprintAbility>("2c38da66e5a599347ac95b3294acbe00").LocalizedDuration; // true strike
            oneMinuteDuration = library.Get<BlueprintAbility>("93f391b0c5a99e04e83bbfbe3bb6db64").LocalizedDuration; // protection from evil communal
            reflexHalfDamage = library.Get<BlueprintAbility>("2d81362af43aeac4387a3d4fced489c3").LocalizedSavingThrow; // fireball
            savingThrowNone = library.Get<BlueprintAbility>("b6010dda6333bcf4093ce20f0063cd41").LocalizedSavingThrow; // frigid touch
            willNegates = library.Get<BlueprintAbility>("8bc64d869456b004b9db255cdd1ea734").LocalizedSavingThrow; //bane
            fortNegates = library.Get<BlueprintAbility>("48e2744846ed04b4580be1a3343a5d3d").LocalizedSavingThrow; //contagion

            wizardSpellList = library.Get<BlueprintSpellList>("ba0401fdeb4062f40a7aa95b6f07fe89");
            magusSpellList = library.Get<BlueprintSpellList>("4d72e1e7bd6bc4f4caaea7aa43a14639");
            druidSpellList = library.Get<BlueprintSpellList>("bad8638d40639d04fa2f80a1cac67d6b");
            clericSpellList = library.Get<BlueprintSpellList>("8443ce803d2d31347897a3d85cc32f53");
            paladinSpellList = library.Get<BlueprintSpellList>("9f5be2f7ea64fe04eb40878347b147bc");
            inquisitorSpellList = library.Get<BlueprintSpellList>("57c894665b7895c499b3dce058c284b3");
            alchemistSpellList = library.Get<BlueprintSpellList>("f60d0cd93edc65c42ad31e34a905fb2f");
            bardSpellList = library.Get<BlueprintSpellList>("25a5013493bdcf74bb2424532214d0c8");
            rangerSpellList = library.Get<BlueprintSpellList>("29f3c338532390546bc5347826a655c4");

            touchWeapon = library.Get<BlueprintItemWeapon>("bb337517547de1a4189518d404ec49d4"); // TouchItem

            bloodlineSelection = library.Get<BlueprintFeatureSelection>("24bef8d1bee12274686f6da6ccbc8914");

            ghostTouch = library.Get<BlueprintWeaponEnchantment>("47857e1a5a3ec1a46adf6491b1423b4f");

            // Note: we can't easily scan all class spell lists, because some spells are
            // only added via special lists, like the ice version of burning hands.
            foreach (var blueprint in Main.library.GetAllBlueprints())
            {
                var spell = blueprint as BlueprintAbility;
                if (spell != null)
                {
                    if (spell.Type == AbilityType.Spell)
                    {
                        // Tiefling racial SLAs are marked as spells rather than SLAs.
                        // (We can find them by the presence of the resource logic.)
                        if (spell.GetComponent<AbilityResourceLogic>() != null)
                        {
                            spellsWithResources.Add(spell);
                        }
                        else
                        {
                            allSpells.Add(spell);
                        }

                    }
                    continue;
                }

                var loot = blueprint as BlueprintLoot;
                if (loot != null)
                {
                    allLoots.Add(loot);
                    continue;
                }

                var unitLoot = blueprint as BlueprintUnitLoot;
                if (unitLoot != null)
                {
                    allUnitLoots.Add(unitLoot);
                    continue;
                }
            }
        }

        public static BlueprintCharacterClass GetClass(String assetId) => classes.First(c => c.AssetGuid == assetId);

        public static BlueprintFeature GetSkillFocus(StatType skill)
        {
            return skillFocusFeat.AllFeatures.First(
                f => f.ComponentsArray.Any(c => (c as AddContextStatBonus)?.Stat == skill));
        }

        public static T Create<T>(Action<T> init = null) where T : ScriptableObject
        {
            var result = ScriptableObject.CreateInstance<T>();
            if (init != null) init(result);
            return result;
        }

        public static PrerequisiteFeature PrerequisiteFeature(this BlueprintFeature feat, bool any = false)
        {
            var result = Create<PrerequisiteFeature>();
            result.Feature = feat;
            result.Group = any ? Prerequisite.GroupType.Any : Prerequisite.GroupType.All;
            return result;
        }

        public static PrerequisiteFeaturesFromList PrerequisiteFeaturesFromList(BlueprintFeature[] features, bool any = false)
        {
            var result = Create<PrerequisiteFeaturesFromList>();
            result.Features = features;
            result.Group = any ? Prerequisite.GroupType.Any : Prerequisite.GroupType.All;
            result.Amount = 1;
            return result;
        }

        public static PrerequisiteFeaturesFromList PrerequisiteFeaturesFromList(IEnumerable<BlueprintFeature> features, bool any = false)
        {
            return PrerequisiteFeaturesFromList(features.ToArray(), any);
        }

        public static PrerequisiteFeaturesFromList PrerequisiteFeaturesFromList(params BlueprintFeature[] features)
        {
            return PrerequisiteFeaturesFromList(features, false);
        }


        public static PrerequisiteNoFeature PrerequisiteNoFeature(this BlueprintFeature feat, bool any = false)
        {
            var result = Create<PrerequisiteNoFeature>();
            result.Feature = feat;
            result.Group = any ? Prerequisite.GroupType.Any : Prerequisite.GroupType.All;
            return result;
        }

        public static PrerequisiteCharacterLevel PrerequisiteCharacterLevel(int level, bool any = false)
        {
            var result = Create<PrerequisiteCharacterLevel>();
            result.Level = level;
            result.Group = any ? Prerequisite.GroupType.Any : Prerequisite.GroupType.All;
            return result;
        }

        public static PrerequisiteClassLevel PrerequisiteClassLevel(this BlueprintCharacterClass @class, int level, bool any = false)
        {
            var result = Create<PrerequisiteClassLevel>();
            result.CharacterClass = @class;
            result.Level = level;
            result.Group = any ? Prerequisite.GroupType.Any : Prerequisite.GroupType.All;
            return result;
        }

        public static PrerequisiteStatValue PrerequisiteStatValue(this StatType stat, int value, bool any = false)
        {
            var result = Create<PrerequisiteStatValue>();
            result.Stat = stat;
            result.Value = value;
            result.Group = any ? Prerequisite.GroupType.Any : Prerequisite.GroupType.All;
            return result;
        }


        public static PrerequisiteFullStatValue PrerequisiteFullStatValue(this StatType stat, int value, bool any = false)
        {
            var result = Create<PrerequisiteFullStatValue>();
            result.Stat = stat;
            result.Value = value;
            result.Group = any ? Prerequisite.GroupType.Any : Prerequisite.GroupType.All;
            return result;
        }

        public static void SetField(object obj, string name, object value)
        {
            Harmony12.AccessTools.Field(obj.GetType(), name).SetValue(obj, value);
        }

        public static void SetLocalizedStringField(BlueprintScriptableObject obj, string name, string value)
        {
            Harmony12.AccessTools.Field(obj.GetType(), name).SetValue(obj, Helpers.CreateString($"{obj.name}.{name}", value));
        }

        public static object GetField(object obj, string name)
        {
            return Harmony12.AccessTools.Field(obj.GetType(), name).GetValue(obj);
        }

        public static object GetField(Type type, object obj, string name)
        {
            return Harmony12.AccessTools.Field(type, name).GetValue(obj);
        }

        public static T GetField<T>(object obj, string name)
        {
            return (T)Harmony12.AccessTools.Field(obj.GetType(), name).GetValue(obj);
        }

        public static FastGetter CreateGetter<T>(string name) => CreateGetter(typeof(T), name);

        public static FastGetter CreateGetter(Type type, string name)
        {
            return new FastGetter(Harmony12.FastAccess.CreateGetterHandler(Harmony12.AccessTools.Property(type, name)));
        }

        public static FastGetter CreateFieldGetter<T>(string name) => CreateFieldGetter(typeof(T), name);

        public static FastGetter CreateFieldGetter(Type type, string name)
        {
            return new FastGetter(Harmony12.FastAccess.CreateGetterHandler(Harmony12.AccessTools.Field(type, name)));
        }

        public static FastSetter CreateSetter<T>(string name) => CreateSetter(typeof(T), name);

        public static FastSetter CreateSetter(Type type, string name)
        {
            return new FastSetter(Harmony12.FastAccess.CreateSetterHandler(Harmony12.AccessTools.Property(type, name)));
        }

        public static FastSetter CreateFieldSetter<T>(string name) => CreateFieldSetter(typeof(T), name);

        public static FastSetter CreateFieldSetter(Type type, string name)
        {
            return new FastSetter(Harmony12.FastAccess.CreateSetterHandler(Harmony12.AccessTools.Field(type, name)));
        }

        public static FastInvoke CreateInvoker<T>(String name) => CreateInvoker(typeof(T), name);

        public static FastInvoke CreateInvoker(Type type, String name)
        {
            return new FastInvoke(Harmony12.MethodInvoker.GetHandler(Harmony12.AccessTools.Method(type, name)));
        }

        public static FastInvoke CreateInvoker<T>(String name, Type[] args, Type[] typeArgs = null) => CreateInvoker(typeof(T), name, args, typeArgs);

        public static FastInvoke CreateInvoker(Type type, String name, Type[] args, Type[] typeArgs = null)
        {
            return new FastInvoke(Harmony12.MethodInvoker.GetHandler(Harmony12.AccessTools.Method(type, name, args, typeArgs)));
        }

        public static LocalizedString CreateString(string key, string value)
        {
            // See if we used the text previously.
            // (It's common for many features to use the same localized text.
            // In that case, we reuse the old entry instead of making a new one.)
            LocalizedString localized;
            /*if (textToLocalizedString.TryGetValue(value, out localized))
            {
                return localized;
            }*/
            var strings = LocalizationManager.CurrentPack.Strings;
            String oldValue;
            if (strings.TryGetValue(key, out oldValue) && value != oldValue)
            {
#if DEBUG
                Log.Write($"Info: duplicate localized string `{key}`, different text.");
#endif
            }
            strings[key] = value;
            localized = new LocalizedString();
            localizedString_m_Key(localized, key);
            textToLocalizedString[value] = localized;
            return localized;
        }

        // All localized strings created in this mod, mapped to their localized key. Populated by CreateString.
        static Dictionary<String, LocalizedString> textToLocalizedString = new Dictionary<string, LocalizedString>();
        static FastSetter localizedString_m_Key = Helpers.CreateFieldSetter<LocalizedString>("m_Key");

        public static Sprite GetIcon(string assetId)
        {
            var asset = (IUIDataProvider)Main.library.BlueprintsByAssetId[assetId];
            return asset.Icon;
        }

        // Returns a GUID that merges guid1 and guid2.
        //
        // Very often we're deriving something from existing game data (e.g. bloodlines)
        // For that code to be extensible to new bloodlines, we need to be able to combine
        // the GUIDs in the game assets with a GUID that is unique for that feat, and
        // get a new GUID that is stable across game reloads.
        //
        // These GUIDs are also nice in that they don't depend on order in which we create
        // our new Assets.
        //
        // Essentially, this prevents us from inadvertantly break existing saves that
        // use features from the mod.
        public static String MergeIds(String guid1, String guid2, String guid3 = null)
        {
            // Parse into low/high 64-bit numbers, and then xor the two halves.
            ulong low = ParseGuidLow(guid1);
            ulong high = ParseGuidHigh(guid1);

            low ^= ParseGuidLow(guid2);
            high ^= ParseGuidHigh(guid2);

            if (guid3 != null)
            {
                low ^= ParseGuidLow(guid3);
                high ^= ParseGuidHigh(guid3);
            }
            return high.ToString("x16") + low.ToString("x16");
        }


        public static String MergeIdsMultiple(String[] guids)
        {
            // Parse into low/high 64-bit numbers, and then xor the two halves.
            ulong low = ParseGuidLow(guids[0]);
            ulong high = ParseGuidHigh(guids[0]);

            for (int i = 1; i < guids.Length; i++)
            {
                low ^= ParseGuidLow(guids[i]);
                high ^= ParseGuidHigh(guids[i]);
            }

            return high.ToString("x16") + low.ToString("x16");
        }

        // Parses the lowest 64 bits of the Guid (which corresponds to the last 16 characters).
        static ulong ParseGuidLow(String id) => ulong.Parse(id.Substring(id.Length - 16), NumberStyles.HexNumber);

        // Parses the high 64 bits of the Guid (which corresponds to the first 16 characters).
        static ulong ParseGuidHigh(String id) => ulong.Parse(id.Substring(0, id.Length - 16), NumberStyles.HexNumber);

        public static MechanicsContext GetMechanicsContext()
        {
            return ElementsContext.GetData<MechanicsContext.Data>()?.Context;
        }

        public static TargetWrapper GetTargetWrapper()
        {
            return ElementsContext.GetData<MechanicsContext.Data>()?.CurrentTarget;
        }

        public static void AddFeats(this LibraryScriptableObject library, params BlueprintFeature[] feats)
        {
            library.AddFeats(basicFeatSelection, feats);
        }

        public static void AddCombatFeats(this LibraryScriptableObject library, params BlueprintFeature[] feats)
        {
            var featSelectionIds = new String[] {
                basicFeatSelection,
                magusFeatSelection,
                /*FighterFeatSelection*/
                "41c8486641f7d6d4283ca9dae4147a9f",
                /*EldritchKnightFeatSelection*/
                "da03141df23f3fe45b0c7c323a8e5a0e",
                /*WarDomainGreaterFeatSelection*/
                "79c6421dbdb028c4fa0c31b8eea95f16",
                Warpriest.fighter_feat?.AssetGuid,
                Oracle.fighter_feat?.AssetGuid,
                "c5158a6622d0b694a99efb1d0025d2c1", //combat trick
                Antipaladin.insinuator_bonus_feat?.AssetGuid,
                Occultist.bonus_feats?.AssetGuid
            };

            if (RogueTalents.feat != null)
            {
                featSelectionIds = featSelectionIds.AddToArray(RogueTalents.feat.AssetGuid);
            }
            foreach (var id in featSelectionIds)
            {
                if (id != null && id != "")
                {
                    AddFeats(library, id, feats);
                }
            }
        }


        public static void AddWizardFeats(this LibraryScriptableObject library, params BlueprintFeature[] feats)
        {
            var selections = new BlueprintFeatureSelection[]
            {
                library.Get<BlueprintFeatureSelection>("d6dd06f454b34014ab0903cb1ed2ade3"), //sorc feat
                library.Get<BlueprintFeatureSelection>("8c3102c2ff3b69444b139a98521a4899"), //wizard feat
                library.Get<BlueprintFeatureSelection>("66befe7b24c42dd458952e3c47c93563"), //magus bonus feats
                Arcanist.collegiate_initiate_bonus_feat
            };


            foreach (var s in selections)
            {
                if (s != null)
                {
                    s.AllFeatures = s.AllFeatures.AddToArray(feats);
                }
            }
        }

        public static void AddFeats(this LibraryScriptableObject library, String featSelectionId, params BlueprintFeature[] feats)
        {
            var featGroup = (BlueprintFeatureSelection)library.BlueprintsByAssetId[featSelectionId];
            var allFeats = featGroup.AllFeatures.ToList();
            allFeats.AddRange(feats);
            featGroup.SetFeatures(allFeats);
        }

        public static void SetFeatureInfo(BlueprintFeature feat, String name, String displayName, String description, String guid, Sprite icon,
            FeatureGroup group, params BlueprintComponent[] components)
        {
            feat.name = name;
            feat.SetComponents(components);
            feat.Groups = new FeatureGroup[] { group };
            feat.SetNameDescriptionIcon(displayName, description, icon);
            Main.library.AddAsset(feat, guid);
        }

        public static BlueprintFeatureSelection CreateFeatureSelection(String name, String displayName,
            String description, String guid, Sprite icon, FeatureGroup group, params BlueprintComponent[] components)
        {
            var feat = Create<BlueprintFeatureSelection>();
            SetFeatureInfo(feat, name, displayName, description, guid, icon, group, components);
            feat.Group = group;
            return feat;
        }

        public static BlueprintFeature CreateFeature(String name, String displayName, String description, String guid, Sprite icon,
            FeatureGroup group, params BlueprintComponent[] components)
        {
            var feat = Create<BlueprintFeature>();
            SetFeatureInfo(feat, name, displayName, description, guid, icon, group, components);
            return feat;
        }

        public static BlueprintProgression CreateProgression(String name, String displayName, String description, String guid, Sprite icon,
            FeatureGroup group, params BlueprintComponent[] components)
        {
            var feat = Create<BlueprintProgression>();
            SetFeatureInfo(feat, name, displayName, description, guid, icon, group, components);
            feat.UIDeterminatorsGroup = Array.Empty<BlueprintFeatureBase>();
            feat.UIGroups = Array.Empty<UIGroup>();
            feat.Classes = Array.Empty<BlueprintCharacterClass>();
            feat.Archetypes = Array.Empty<BlueprintArchetype>();
            return feat;
        }

        public static UIGroup CreateUIGroup(params BlueprintFeatureBase[] features) => CreateUIGroup((IEnumerable<BlueprintFeatureBase>)features);

        public static UIGroup CreateUIGroup(IEnumerable<BlueprintFeatureBase> features)
        {
            var result = new UIGroup();
            result.Features.AddRange(features);
            return result;
        }

        public static UIGroup[] CreateUIGroups(params BlueprintFeatureBase[] features) => CreateUIGroups((IEnumerable<BlueprintFeatureBase>)features);

        public static UIGroup[] CreateUIGroups(IEnumerable<BlueprintFeatureBase> features)
        {
            var result = new UIGroup();
            result.Features.AddRange(features);
            return new UIGroup[] { result };
        }

        public static BlueprintParametrizedFeature CreateParametrizedFeature(String name, String displayName, String description, String guid, Sprite icon,
            FeatureGroup group, FeatureParameterType type, params BlueprintComponent[] components)
        {
            var feat = Create<BlueprintParametrizedFeature>();
            SetFeatureInfo(feat, name, displayName, description, guid, icon, group, components);
            feat.ParameterType = type;
            return feat;
        }

        public static T CreateParamSelection<T>(String name, String displayName, String description, String guid, Sprite icon,
            FeatureGroup group, params BlueprintComponent[] components) where T : BlueprintParametrizedFeature
        {

            var feat = Create<T>();
            SetFeatureInfo(feat, name, displayName, description, guid, icon, group, components);
            return feat;
        }

        public static LevelEntry LevelEntry(int level, BlueprintFeatureBase feature)
        {
            var entry = new LevelEntry() { Level = level };
            entry.Features.Add(feature);
            return entry;
        }

        public static LevelEntry LevelEntry(int level, params BlueprintFeatureBase[] features)
        {
            var entry = new LevelEntry() { Level = level };
            entry.Features.AddRange(features);
            return entry;
        }

        public static LevelEntry LevelEntry(int level, IEnumerable<BlueprintFeatureBase> features)
        {
            var entry = new LevelEntry() { Level = level };
            entry.Features.AddRange(features);
            return entry;
        }

        public static AddFacts CreateAddFacts(params BlueprintUnitFact[] facts)
        {
            var result = Create<AddFacts>();
            result.Facts = facts;
            return result;
        }


        public static AddFacts CreateAddFactsNoRestore(params BlueprintUnitFact[] facts)
        {
            var result = Create<AddFacts>();
            result.Facts = facts;
            result.DoNotRestoreMissingFacts = true;
            return result;
        }

        public static AddKnownSpell CreateAddKnownSpell(this BlueprintAbility spell, BlueprintCharacterClass @class, int level, BlueprintArchetype archetype = null)
        {
            var addSpell = Create<AddKnownSpell>();
            addSpell.Spell = spell;
            addSpell.SpellLevel = level;
            addSpell.CharacterClass = @class;
            return addSpell;
        }

        public static AbilityEffectRunAction CreateRunActions(params GameAction[] actions)
        {
            var result = Create<AbilityEffectRunAction>();
            result.Actions = CreateActionList(actions);
            return result;
        }

        public static AddFactContextActions CreateAddFactContextActions(GameAction activated = null, GameAction deactivated = null, GameAction newRound = null)
        {
            var a = Create<AddFactContextActions>();
            a.Activated = CreateActionList(activated);
            a.Deactivated = CreateActionList(deactivated);
            a.NewRound = CreateActionList(newRound);
            return a;
        }


        public static void SetUnitName(this BlueprintUnit unit, string name)
        {
            unit.LocalizedName = unit.LocalizedName.CreateCopy();
            unit.LocalizedName.String = Helpers.CreateString(unit.name + ".Name", name);
        }

        public static AddFactContextActions CreateEmptyAddFactContextActions()
        {
            var a = Create<AddFactContextActions>();
            a.Activated = CreateActionList();
            a.Deactivated = CreateActionList();
            a.NewRound = CreateActionList();
            return a;
        }

        public static AddFactContextActions CreateAddFactContextActions(GameAction[] activated = null, GameAction[] deactivated = null, GameAction[] newRound = null)
        {
            var a = Create<AddFactContextActions>();
            a.Activated = CreateActionList(activated);
            a.Deactivated = CreateActionList(deactivated);
            a.NewRound = CreateActionList(newRound);
            return a;
        }

        public static ActionList CreateActionList(params GameAction[] actions)
        {
            if (actions == null || actions.Length == 1 && actions[0] == null) actions = Array.Empty<GameAction>();
            return new ActionList() { Actions = actions };
        }

        public static AbilityEffectRunAction CreateRunActions(SavingThrowType savingThrow, params GameAction[] actions)
        {
            var result = Create<AbilityEffectRunAction>();
            result.SavingThrowType = savingThrow;
            result.Actions = new ActionList() { Actions = actions };
            return result;
        }

        public static AddStatBonus CreateAddStatBonus(this StatType stat, int value, ModifierDescriptor descriptor)
        {
            var addStat = Create<AddStatBonus>();
            addStat.Stat = stat;
            addStat.Value = value;
            addStat.Descriptor = descriptor;
            return addStat;
        }

        public static AddContextStatBonus CreateAddContextStatBonus(StatType stat, ModifierDescriptor descriptor, ContextValueType type = ContextValueType.Rank, AbilityRankType rankType = AbilityRankType.Default, int multiplier = 1)
        {
            var addStat = Create<AddContextStatBonus>();
            addStat.Stat = stat;
            addStat.Value = new ContextValue() { ValueType = type };
            addStat.Descriptor = descriptor;
            addStat.Value.ValueRank = rankType;
            addStat.Multiplier = multiplier;
            return addStat;
        }

        public static ContextActionApplyBuff CreateApplyBuff(this BlueprintBuff buff, ContextDurationValue duration, bool fromSpell, bool dispellable = true, bool toCaster = false, bool asChild = false, bool permanent = false)
        {
            var result = Create<ContextActionApplyBuff>();
            result.Buff = buff;
            result.DurationValue = duration;
            result.IsFromSpell = fromSpell;
            result.IsNotDispelable = !dispellable;
            result.ToCaster = toCaster;
            result.AsChild = asChild;
            result.Permanent = permanent;
            return result;
        }

        public static BlueprintComponent CreateAddMechanics(this AddMechanicsFeature.MechanicsFeatureType mechanicsKind)
        {
            var a = Helpers.Create<AddMechanicsFeature>();
            SetField(a, "m_Feature", mechanicsKind);
            return a;
        }

        public static BlueprintAbilityResource CreateAbilityResource(String name, String displayName, String description, String guid, Sprite icon,
            params BlueprintComponent[] components)
        {
            var resource = Create<BlueprintAbilityResource>();
            resource.name = name;
            resource.SetComponents(components);
            resource.LocalizedName = CreateString(name + ".Name", displayName);
            resource.LocalizedDescription = CreateString(name + ".Name", description);
            resource.SetIcon(icon);
            Main.library.AddAsset(resource, guid);
            return resource;
        }

        public static AbilityResourceLogic CreateResourceLogic(this BlueprintAbilityResource resource, bool spend = true, int amount = 1, bool cost_is_custom = false)
        {
            var a = Create<AbilityResourceLogic>();
            a.IsSpendResource = spend;
            a.RequiredResource = resource;
            a.Amount = amount;
            a.CostIsCustom = cost_is_custom;
            return a;
        }

        public static IncreaseResourceAmount CreateIncreaseResourceAmount(this BlueprintAbilityResource resource, int amount)
        {
            var i = Create<IncreaseResourceAmount>();
            i.Resource = resource;
            i.Value = amount;
            return i;
        }

        public static ContextDurationValue CreateContextDuration(ContextValue bonus = null, DurationRate rate = DurationRate.Rounds, DiceType diceType = DiceType.Zero, ContextValue diceCount = null)
        {
            return new ContextDurationValue()
            {
                BonusValue = bonus ?? CreateContextValueRank(),
                Rate = rate,
                DiceCountValue = diceCount ?? 0,
                DiceType = diceType
            };
        }


        public static ContextDurationValue CreateContextDurationNonExtandable(ContextValue bonus = null, DurationRate rate = DurationRate.Rounds, DiceType diceType = DiceType.Zero, ContextValue diceCount = null)
        {
            var d =  new ContextDurationValue()
            {
                BonusValue = bonus ?? CreateContextValueRank(),
                Rate = rate,
                DiceCountValue = diceCount ?? 0,
                DiceType = diceType
            };
            Helpers.SetField(d, "m_IsExtendable", false);
            return d;
        }

        public static BlueprintBuff CreateBuff(String name, String displayName, String description, String guid, Sprite icon,
            PrefabLink fxOnStart,
            params BlueprintComponent[] components)
        {
            var buff = Create<BlueprintBuff>();
            buff.name = name;
            buff.FxOnStart = fxOnStart ?? new PrefabLink();
            buff.FxOnRemove = new PrefabLink();
            buff.SetComponents(components);
            buff.SetNameDescriptionIcon(displayName, description, icon);
            Main.library.AddAsset(buff, guid);
            return buff;
        }

        public static BlueprintAbility CreateAbility(String name, String displayName,
            String description, String guid, Sprite icon,
            AbilityType type, CommandType actionType, AbilityRange range,
            String duration, String savingThrow,
            params BlueprintComponent[] components)
        {
            var ability = Create<BlueprintAbility>();
            ability.name = name;
            ability.SetComponents(components);
            ability.SetNameDescriptionIcon(displayName, description, icon);
            ability.ResourceAssetIds = Array.Empty<string>();

            ability.Type = type;
            ability.ActionType = actionType;
            ability.Range = range;
            ability.LocalizedDuration = CreateString($"{name}.Duration", duration);
            ability.LocalizedSavingThrow = CreateString($"{name}.SavingThrow", savingThrow);

            Main.library.AddAsset(ability, guid);
            return ability;
        }

        public static BlueprintAbility CreateTouchSpellCast(this BlueprintAbility spell, BlueprintAbilityResource resource = null)
        {
            var castSpell = Main.library.CopyAndAdd(spell, $"{spell.name}Cast",
                MergeIds(spell.AssetGuid, "8de5133f37ff4cab8286f16c826651c1"));

            var components = new List<BlueprintComponent>();
            components.Add(Helpers.CreateStickyTouch(spell));

            var schoolComponent = spell.GetComponent<SpellComponent>();
            if (schoolComponent != null) components.Add(schoolComponent);

            var descriptorComponent = spell.GetComponent<SpellDescriptorComponent>();
            if (descriptorComponent != null) components.Add(descriptorComponent);

            if (resource != null) components.Add(resource.CreateResourceLogic());

            if (spell.GetComponent<AbilityResourceLogic>() != null)
            {
                Log.Write($"Warning: resource logic should be passed to CreateTouchSpellCast instead of a component: {spell.name}");
            }

            castSpell.SetComponents(components);
            return castSpell;
        }


        public static BlueprintAbility CreateTouchSpellCast(this BlueprintAbility spell, string name, string guid, BlueprintAbilityResource resource = null)
        {
            var castSpell = Main.library.CopyAndAdd(spell, name, guid);

            var components = new List<BlueprintComponent>();
            components.Add(Helpers.CreateStickyTouch(spell));

            var schoolComponent = spell.GetComponent<SpellComponent>();
            if (schoolComponent != null) components.Add(schoolComponent);
            if (resource != null) components.Add(resource.CreateResourceLogic());

            if (spell.GetComponent<AbilityResourceLogic>() != null)
            {
                Log.Write($"Warning: resource logic should be passed to CreateTouchSpellCast instead of a component: {spell.name}");
            }

            castSpell.SetComponents(components);
            return castSpell;
        }

        public static BindAbilitiesToClass CreateBindToClass(this BlueprintAbility ability, BlueprintProgression bloodline, StatType stat)
        {
            var b = Helpers.Create<BindAbilitiesToClass>();
            b.Stat = stat;
            b.Abilites = new BlueprintAbility[] { ability };
            b.CharacterClass = bloodline.Classes[0];
            b.Archetypes = bloodline.Archetypes;
            b.AdditionalClasses = bloodline.Classes.Skip(1).ToArray();
            return b;
        }

        public static BindAbilitiesToClass CreateBindToClass(this BlueprintAbility ability, BlueprintCharacterClass @class, StatType stat)
        {
            var b = Helpers.Create<BindAbilitiesToClass>();
            b.Abilites = new BlueprintAbility[] { ability };
            b.Stat = stat;
            b.CharacterClass = @class;
            b.Archetypes = Array.Empty<BlueprintArchetype>();
            b.AdditionalClasses = Array.Empty<BlueprintCharacterClass>();
            return b;
        }

        public static BindAbilitiesToClass CreateBindToClass(BlueprintCharacterClass @class, StatType stat, params BlueprintAbility[] abilities)
        {
            var b = Helpers.Create<BindAbilitiesToClass>();
            b.Abilites = abilities;
            b.Stat = stat;
            b.CharacterClass = @class;
            b.Archetypes = Array.Empty<BlueprintArchetype>();
            b.AdditionalClasses = Array.Empty<BlueprintCharacterClass>();
            return b;
        }

        public static BlueprintActivatableAbility CreateActivatableAbility(String name, String displayName, String description,
            string assetId, Sprite icon, BlueprintBuff buff, AbilityActivationType activationType, CommandType commandType,
            AnimationClip activateWithUnitAnimation,
            params BlueprintComponent[] components)
        {
            var ability = Create<BlueprintActivatableAbility>();
            ability.name = name;
            ability.SetNameDescriptionIcon(displayName, description, icon);
            ability.Buff = buff;
            ability.ResourceAssetIds = Array.Empty<string>();
            ability.ActivationType = activationType;
            set_ActivateWithUnitCommand(ability, commandType);
            ability.SetComponents(components);
            ability.ActivateWithUnitAnimation = activateWithUnitAnimation;
            Main.library.AddAsset(ability, assetId);
            return ability;
        }

        static readonly FastSetter set_ActivateWithUnitCommand = CreateFieldSetter<BlueprintActivatableAbility>("m_ActivateWithUnitCommand");

        public static ActivatableAbilityResourceLogic CreateActivatableResourceLogic(this BlueprintAbilityResource resource,
            ResourceSpendType spendType)
        {
            var logic = Create<ActivatableAbilityResourceLogic>();
            logic.RequiredResource = resource;
            logic.SpendType = spendType;
            return logic;
        }

        public static SpellComponent CreateSpellComponent(this SpellSchool school)
        {
            var s = Create<SpellComponent>();
            s.School = school;
            return s;
        }


        public static SpellDescriptorComponent CreateSpellDescriptor()
        {
            var s = Create<SpellDescriptorComponent>();
            s.Descriptor = SpellDescriptor.None;
            return s;
        }

        public static SpellDescriptorComponent CreateSpellDescriptor(this SpellDescriptor descriptor)
        {
            var s = Create<SpellDescriptorComponent>();
            s.Descriptor = descriptor;
            return s;
        }


        public static AbilityVariants CreateAbilityVariants(this BlueprintAbility parent, IEnumerable<BlueprintAbility> variants) => CreateAbilityVariants(parent, variants.ToArray());

        public static AbilityVariants CreateAbilityVariants(this BlueprintAbility parent, params BlueprintAbility[] variants)
        {
            var a = Create<AbilityVariants>();
            a.Variants = variants;
            foreach (var v in variants)
            {
                v.Parent = parent;
            }
            return a;
        }


        public static bool addToAbilityVariants(this BlueprintAbility parent, params BlueprintAbility[] variants)
        {
            var comp = parent.GetComponent<AbilityVariants>();

            comp.Variants = comp.Variants.AddToArray(variants);

            foreach (var v in variants)
            {
                v.Parent = parent;
            }
            return true;
        }

        public static AddFacts CreateAddFact(this BlueprintUnitFact fact)
        {
            var result = Create<AddFacts>();
            result.name = $"AddFacts${fact.name}";
            result.Facts = new BlueprintUnitFact[] { fact };
            return result;
        }


        public static AddFacts CreateAddFactNoRestore(this BlueprintUnitFact fact)
        {
            var result = Create<AddFacts>();
            result.name = $"AddFacts${fact.name}";
            result.Facts = new BlueprintUnitFact[] { fact };
            result.DoNotRestoreMissingFacts = true;
            return result;
        }





        public static AddFeatureOnClassLevel CreateAddFeatureOnClassLevel(this BlueprintFeature feat, int level, BlueprintCharacterClass[] classes, BlueprintArchetype[] archetypes, bool before = false)
        {
            var a = Create<AddFeatureOnClassLevel>();
            a.name = $"AddFeatureOnClassLevel${feat.name}";
            a.Level = level;
            a.BeforeThisLevel = before;
            a.Feature = feat;
            a.Class = classes[0];
            a.AdditionalClasses = classes.Skip(1).ToArray();
            a.Archetypes = archetypes == null ? new BlueprintArchetype[0] : archetypes;
            return a;
        }


        public static AddFeatureOnClassLevel CreateAddFeatureOnClassLevel(this BlueprintFeature feat, int level, BlueprintCharacterClass[] classes, bool before = false)
        {
            var a = Create<AddFeatureOnClassLevel>();
            a.name = $"AddFeatureOnClassLevel${feat.name}";
            a.Level = level;
            a.BeforeThisLevel = before;
            a.Feature = feat;
            a.Class = classes[0];
            a.AdditionalClasses = classes.Skip(1).ToArray();
            a.Archetypes = new BlueprintArchetype[0];
            return a;
        }

        public static AddAbilityResources CreateAddAbilityResource(this BlueprintAbilityResource resource)
        {
            var a = Create<AddAbilityResources>();
            a.Resource = resource;
            a.RestoreAmount = true;
            return a;
        }


        public static AddAbilityResources CreateAddAbilityResourceNoRestore(this BlueprintAbilityResource resource)
        {
            var a = Create<AddAbilityResources>();
            a.Resource = resource;
            a.RestoreAmount = false;
            return a;
        }

        public static AbilityResourceLogic ReplaceResourceLogic(this BlueprintAbility ability, BlueprintAbilityResource resource)
        {
            var original = ability.GetComponent<AbilityResourceLogic>();
            var replacement = UnityEngine.Object.Instantiate(original);
            replacement.RequiredResource = resource;
            ability.ReplaceComponent(original, replacement);
            return replacement;
        }

        public static void ReplaceContextRankConfig(this BlueprintScriptableObject obj, Action<ContextRankConfig> update)
        {
            foreach (var original in obj.GetComponents<ContextRankConfig>())
            {
                var replacement = UnityEngine.Object.Instantiate(original);
                update(replacement);
                obj.ReplaceComponent(original, replacement);
            }
        }


        public static bool checkSpellbook(BlueprintSpellbook spellbook_blueprint, BlueprintCharacterClass blueprint_class, 
                                          Spellbook spellbook,  UnitDescriptor unit_descriptor)
        {
            if (spellbook_blueprint != null && spellbook_blueprint != spellbook?.Blueprint)
            {
                return false;
            }

            var class_spellbook = blueprint_class == null ? null : unit_descriptor.GetSpellbook(blueprint_class);

            if (blueprint_class != null && (class_spellbook == null || spellbook != class_spellbook))
            {
                return false;
            }
            return true;
        }

        public static void ReplaceContextRankConfig(this BlueprintScriptableObject obj, ContextRankConfig newConfig)
        {
            obj.ReplaceComponent<ContextRankConfig>(newConfig);
        }

        public static void ReplaceComponent<T>(this BlueprintScriptableObject obj, BlueprintComponent replacement) where T : BlueprintComponent
        {
            ReplaceComponent(obj, obj.GetComponent<T>(), replacement);
        }


        public static void ReplaceComponent<T>(this BlueprintScriptableObject obj, Action<T> action) where T : BlueprintComponent
        {
            var replacement = obj.GetComponent<T>().CreateCopy();
            action(replacement);
            ReplaceComponent(obj, obj.GetComponent<T>(), replacement);
        }


        public static void MaybeReplaceComponent<T>(this BlueprintScriptableObject obj, Action<T> action) where T : BlueprintComponent
        {
            var replacement = obj.GetComponent<T>()?.CreateCopy();
            if (replacement == null)
            {
                return;
            }
            action(replacement);
            ReplaceComponent(obj, obj.GetComponent<T>(), replacement);
        }

        public static void ReplaceComponent(this BlueprintScriptableObject obj, BlueprintComponent original, BlueprintComponent replacement)
        {
            // Note: make a copy so we don't mutate the original component
            // (in case it's a clone of a game one).
            var components = obj.ComponentsArray;
            var newComponents = new BlueprintComponent[components.Length];
            for (int i = 0; i < components.Length; i++)
            {
                var c = components[i];
                newComponents[i] = c == original ? replacement : c;
            }
            obj.SetComponents(newComponents); // fix up names if needed
        }


        public static ContextRankConfig CreateContextRankConfig(
            ContextRankBaseValueType baseValueType = ContextRankBaseValueType.CasterLevel,
            ContextRankProgression progression = ContextRankProgression.AsIs,
            AbilityRankType type = AbilityRankType.Default,
            int? min = null, int? max = null, int startLevel = 0, int stepLevel = 0,
            bool exceptClasses = false, StatType stat = StatType.Unknown,
            BlueprintUnitProperty customProperty = null,
            BlueprintCharacterClass[] classes = null, BlueprintArchetype archetype = null,
            BlueprintFeature feature = null, BlueprintFeature[] featureList = null,
            (int, int)[] customProgression = null)
        {
            var config = Create<ContextRankConfig>();
            setType(config, type);
            setBaseValueType(config, baseValueType);
            setProgression(config, progression);
            setUseMin(config, min.HasValue);
            setMin(config, min.GetValueOrDefault());
            setUseMax(config, max.HasValue);
            setMax(config, max.GetValueOrDefault());
            setStartLevel(config, startLevel);
            setStepLevel(config, stepLevel);
            setFeature(config, feature);
            setExceptClasses(config, exceptClasses);
            setCustomProperty(config, customProperty);
            setStat(config, stat);
            setClass(config, classes ?? Array.Empty<BlueprintCharacterClass>());
            setArchetype(config, archetype);
            setFeatureList(config, featureList ?? Array.Empty<BlueprintFeature>());

            if (customProgression != null)
            {
                var items = Array.CreateInstance(customProgressionItemType, customProgression.Length);
                for (int i = 0; i < items.Length; i++)
                {
                    var item = Activator.CreateInstance(customProgressionItemType);
                    var p = customProgression[i];
                    SetField(item, "BaseValue", p.Item1);
                    SetField(item, "ProgressionValue", p.Item2);
                    items.SetValue(item, i);
                }
                setCustomProgression(config, items);
            }

            return config;
        }

        static readonly FastSetter setType = Helpers.CreateFieldSetter<ContextRankConfig>("m_Type");
        static readonly FastSetter setBaseValueType = Helpers.CreateFieldSetter<ContextRankConfig>("m_BaseValueType");
        static readonly FastSetter setProgression = Helpers.CreateFieldSetter<ContextRankConfig>("m_Progression");
        static readonly FastSetter setUseMin = Helpers.CreateFieldSetter<ContextRankConfig>("m_UseMin");
        static readonly FastSetter setMin = Helpers.CreateFieldSetter<ContextRankConfig>("m_Min");
        static readonly FastSetter setUseMax = Helpers.CreateFieldSetter<ContextRankConfig>("m_UseMax");
        static readonly FastSetter setMax = Helpers.CreateFieldSetter<ContextRankConfig>("m_Max");
        static readonly FastSetter setStartLevel = Helpers.CreateFieldSetter<ContextRankConfig>("m_StartLevel");
        static readonly FastSetter setStepLevel = Helpers.CreateFieldSetter<ContextRankConfig>("m_StepLevel");
        static readonly FastSetter setFeature = Helpers.CreateFieldSetter<ContextRankConfig>("m_Feature");
        static readonly FastSetter setExceptClasses = Helpers.CreateFieldSetter<ContextRankConfig>("m_ExceptClasses");
        static readonly FastSetter setCustomProperty = Helpers.CreateFieldSetter<ContextRankConfig>("m_CustomProperty");
        static readonly FastSetter setStat = Helpers.CreateFieldSetter<ContextRankConfig>("m_Stat");
        static readonly FastSetter setClass = Helpers.CreateFieldSetter<ContextRankConfig>("m_Class");
        static readonly FastSetter setArchetype = Helpers.CreateFieldSetter<ContextRankConfig>("Archetype");
        static readonly FastSetter setFeatureList = Helpers.CreateFieldSetter<ContextRankConfig>("m_FeatureList");
        static readonly FastSetter setCustomProgression = Helpers.CreateFieldSetter<ContextRankConfig>("m_CustomProgression");
        static readonly Type customProgressionItemType = Harmony12.AccessTools.Inner(typeof(ContextRankConfig), "CustomProgressionItem");


        public static AbilityTargetsAround CreateAbilityTargetsAround(Feet radius, TargetType targetType, ConditionsChecker conditions = null, Feet spreadSpeed = default(Feet), bool includeDead = false)
        {
            var around = Create<AbilityTargetsAround>();
            SetField(around, "m_Radius", radius);
            SetField(around, "m_TargetType", targetType);
            SetField(around, "m_IncludeDead", includeDead);
            SetField(around, "m_Condition", conditions ?? new ConditionsChecker() { Conditions = Array.Empty<Condition>() });
            SetField(around, "m_SpreadSpeed", spreadSpeed);
            return around;
        }

        public static ContextActionConditionalSaved CreateConditionalSaved(GameAction[] success, GameAction[] failed)
        {
            var c = Create<ContextActionConditionalSaved>();
            c.Succeed = CreateActionList(success);
            c.Failed = CreateActionList(failed);
            return c;
        }

        public static ContextActionConditionalSaved CreateConditionalSaved(GameAction success, GameAction failed)
        {
            return CreateConditionalSaved(success == null ? new GameAction[0] : new GameAction[] { success }, failed == null ? new GameAction[0] : new GameAction[] { failed });
        }

        public static Conditional CreateConditional(Condition condition, GameAction ifTrue, GameAction ifFalse = null)
        {
            var c = Create<Conditional>();
            c.ConditionsChecker = CreateConditionsCheckerAnd(condition);
            c.IfTrue = CreateActionList(ifTrue);
            c.IfFalse = CreateActionList(ifFalse);
            return c;
        }

        public static Conditional CreateConditional(Condition[] condition, GameAction ifTrue, GameAction ifFalse = null)
        {
            var c = Create<Conditional>();
            c.ConditionsChecker = CreateConditionsCheckerAnd(condition);
            c.IfTrue = CreateActionList(ifTrue);
            c.IfFalse = CreateActionList(ifFalse);
            return c;
        }


        public static Conditional CreateConditionalOr(Condition[] condition, GameAction ifTrue, GameAction ifFalse = null)
        {
            var c = Create<Conditional>();
            c.ConditionsChecker = CreateConditionsCheckerOr(condition);
            c.IfTrue = CreateActionList(ifTrue);
            c.IfFalse = CreateActionList(ifFalse);
            return c;
        }

        public static Conditional CreateConditional(Condition[] condition, GameAction[] ifTrue, GameAction[] ifFalse = null)
        {
            var c = Create<Conditional>();
            c.ConditionsChecker = CreateConditionsCheckerAnd(condition);
            c.IfTrue = CreateActionList(ifTrue);
            c.IfFalse = CreateActionList(ifFalse);
            return c;
        }



        public static Conditional CreateConditional(ConditionsChecker conditions, GameAction ifTrue, GameAction ifFalse = null)
        {
            var c = Create<Conditional>();
            c.ConditionsChecker = conditions;
            c.IfTrue = CreateActionList(ifTrue);
            c.IfFalse = CreateActionList(ifFalse);
            return c;
        }

        public static ConditionsChecker CreateConditionsCheckerAnd(params Condition[] conditions)
        {
            return new ConditionsChecker() { Conditions = conditions, Operation = Operation.And };
        }

        public static ConditionsChecker CreateConditionsCheckerOr(params Condition[] conditions)
        {
            return new ConditionsChecker() { Conditions = conditions, Operation = Operation.Or };
        }

        public static OrAndLogic CreateAndLogic(bool not, params Condition[] conditions)
        {
            var c = Helpers.Create<OrAndLogic>();
            c.Not = not;
            c.ConditionsChecker = CreateConditionsCheckerAnd(conditions);
            return c;
        }

        public static OrAndLogic CreateOrLogic(bool not, params Condition[] conditions)
        {
            var c = Helpers.Create<OrAndLogic>();
            c.Not = not;
            c.ConditionsChecker = CreateConditionsCheckerOr(conditions);
            return c;
        }

        public static Conditional CreateConditional(Condition condition, GameAction[] ifTrue, GameAction[] ifFalse = null)
        {
            var c = Create<Conditional>();
            c.ConditionsChecker = new ConditionsChecker() { Conditions = new Condition[] { condition } };
            c.IfTrue = CreateActionList(ifTrue);
            c.IfFalse = CreateActionList(ifFalse ?? Array.Empty<GameAction>());
            return c;
        }

        public static Conditional CreateConditional(ConditionsChecker condition, GameAction[] ifTrue, GameAction[] ifFalse = null)
        {
            var c = Create<Conditional>();
            c.ConditionsChecker = condition;
            c.IfTrue = CreateActionList(ifTrue);
            c.IfFalse = CreateActionList(ifFalse ?? Array.Empty<GameAction>());
            return c;
        }


        public static ContextConditionHasBuff CreateConditionHasBuff(this BlueprintBuff buff)
        {
            var hasBuff = Create<ContextConditionHasBuff>();
            hasBuff.Buff = buff;
            return hasBuff;
        }

        public static ContextConditionHasBuff CreateConditionHasNoBuff(this BlueprintBuff buff)
        {
            var hasBuff = Create<ContextConditionHasBuff>();
            hasBuff.Buff = buff;
            hasBuff.Not = true;
            return hasBuff;
        }


        public static ContextConditionHasBuffFromCaster CreateConditionHasBuffFromCaster(this BlueprintBuff buff, bool not = false)
        {
            var hasBuff = Create<ContextConditionHasBuffFromCaster>();
            hasBuff.Buff = buff;
            hasBuff.Not = not;
            return hasBuff;
        }


        public static ContextConditionHasFact CreateConditionHasFact(this BlueprintUnitFact fact, bool not = false)
        {
            var c = Create<ContextConditionHasFact>();
            c.Fact = fact;
            c.Not = not;
            return c;
        }

        public static ContextConditionCasterHasFact CreateConditionCasterHasFact(this BlueprintUnitFact fact, bool not = false)
        {
            var c = Create<ContextConditionCasterHasFact>();
            c.Fact = fact;
            c.Not = not;
            return c;
        }

        public static ContextConditionAlignment CreateContextConditionAlignment(AlignmentComponent alignment, bool check_caster = false, bool not = false)
        {
            var c = Create<ContextConditionAlignment>();
            c.Alignment = alignment;
            c.Not = not;
            c.CheckCaster = check_caster;
            return c;
        }

        public static BuffMechanics.SuppressBuffsCorrect CreateSuppressBuffs(params BlueprintBuff[] buffs)
        {
            var s = Create<BuffMechanics.SuppressBuffsCorrect>();
            s.Schools = Array.Empty<SpellSchool>();
            s.Buffs = buffs;
            return s;
        }

        public static BuffMechanics.SuppressBuffsCorrect CreateSuppressBuffs(IEnumerable<BlueprintBuff> buffs) => CreateSuppressBuffs(buffs.ToArray());


        public static AbilityAreaEffectRunAction CreateAreaEffectRunAction(GameAction unitEnter = null, GameAction unitExit = null, GameAction unitMove = null, GameAction round = null)
        {
            var a = Create<AbilityAreaEffectRunAction>();
            a.UnitEnter = CreateActionList(unitEnter);
            a.UnitExit = CreateActionList(unitExit);
            a.UnitMove = CreateActionList(unitMove);
            a.Round = CreateActionList(round);
            return a;
        }



        public static AbilityAreaEffectRunAction CreateAreaEffectRunAction(GameAction[] unitEnter = null, GameAction[] unitExit = null, GameAction[] unitMove = null, GameAction[] round = null)
        {
            var a = Create<AbilityAreaEffectRunAction>();
            a.UnitEnter = CreateActionList(unitEnter);
            a.UnitExit = CreateActionList(unitExit);
            a.UnitMove = CreateActionList(unitMove);
            a.Round = CreateActionList(round);
            return a;
        }


        public static ContextActionSavingThrow CreateActionSavingThrow(this SavingThrowType savingThrow, params GameAction[] actions)
        {
            var c = Create<ContextActionSavingThrow>();
            c.Type = savingThrow;
            c.Actions = CreateActionList(actions);
            return c;
        }

        public static ContextValue CreateContextValueRank(AbilityRankType value = AbilityRankType.Default) => value.CreateContextValue();

        public static ContextValue CreateContextValue(this AbilityRankType value)
        {
            return new ContextValue() { ValueType = ContextValueType.Rank, ValueRank = value };
        }

        public static ContextValue CreateContextValue(this AbilitySharedValue value)
        {
            return new ContextValue() { ValueType = ContextValueType.Shared, ValueShared = value };
        }

        public static ContextDiceValue CreateContextDiceValue(this DiceType dice, ContextValue diceCount = null, ContextValue bonus = null)
        {
            return new ContextDiceValue()
            {
                DiceType = dice,
                DiceCountValue = diceCount ?? Helpers.CreateContextValueRank(),
                BonusValue = bonus ?? 0
            };
        }


        public static ContextDiceValue CreateContextDiceValueFromSharedValue(AbilitySharedValue value_type)
        {
            return CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(value_type));
        }

        public static ContextCalculateSharedValue CreateCalculateSharedValue(ContextDiceValue value, AbilitySharedValue sharedValue = AbilitySharedValue.Damage, double modifier = 1.0)
        {
            var c = Helpers.Create<ContextCalculateSharedValue>();
            c.Value = value;
            c.ValueType = sharedValue;
            c.Modifier = modifier;
            return c;
        }

        public static ContextActionChangeSharedValue CreateActionChangeSharedValue(this SharedValueChangeType type, AbilitySharedValue value = AbilitySharedValue.Damage, ContextValue addValue = null, ContextValue setValue = null, ContextValue multiplyValue = null)
        {
            var c = Helpers.Create<ContextActionChangeSharedValue>();
            c.Type = type;
            c.SharedValue = value;
            c.AddValue = addValue ?? 0;
            c.SetValue = setValue ?? 0;
            c.MultiplyValue = multiplyValue; // it appears okay for this one to be null
            return c;
        }

        public static ContextActionDealDamage CreateActionDealDamage(DamageEnergyType energy, ContextDiceValue damage, bool isAoE = false, bool halfIfSaved = false, bool IgnoreCritical = false)
        {
            // energy damage
            var c = Create<ContextActionDealDamage>();
            c.DamageType = new DamageTypeDescription()
            {
                Type = DamageType.Energy,
                Energy = energy,
                Common = new DamageTypeDescription.CommomData(),
                Physical = new DamageTypeDescription.PhysicalData()
            };
            c.Duration = Helpers.CreateContextDuration(0);
            c.Value = damage;
            c.IsAoE = isAoE;
            c.HalfIfSaved = halfIfSaved;
            c.IgnoreCritical = IgnoreCritical;
            return c;
        }


        public static ContextActionDealDamage CreateActionDealDirectDamage(ContextDiceValue damage, bool isAoE = false, bool halfIfSaved = false, bool IgnoreCritical = false)
        {
            // energy damage
            var c = Create<ContextActionDealDamage>();
            c.DamageType = new DamageTypeDescription()
            {
                Type = DamageType.Direct,
                Common = new DamageTypeDescription.CommomData(),
                Physical = new DamageTypeDescription.PhysicalData()
            };
            c.Duration = Helpers.CreateContextDuration(0);
            c.Value = damage;
            c.IsAoE = isAoE;
            c.HalfIfSaved = halfIfSaved;
            c.IgnoreCritical = IgnoreCritical;
            return c;
        }


        public static ContextActionDealDamage CreateActionDealForceDamage(ContextDiceValue damage, bool isAoE = false, bool halfIfSaved = false, bool IgnoreCritical = false)
        {
            // energy damage
            var c = Create<ContextActionDealDamage>();
            c.DamageType = new DamageTypeDescription()
            {
                Type = DamageType.Force,
                Common = new DamageTypeDescription.CommomData(),
                Physical = new DamageTypeDescription.PhysicalData()
            };
            c.Duration = Helpers.CreateContextDuration(0);
            c.Value = damage;
            c.IsAoE = isAoE;
            c.HalfIfSaved = halfIfSaved;
            c.IgnoreCritical = IgnoreCritical;
            return c;
        }


        public static ContextActionDealDamage CreateActionDealDamage(PhysicalDamageForm physical, ContextDiceValue damage, bool isAoE = false, bool halfIfSaved = false, bool IgnoreCritical = false)
        {
            // physical damage
            var c = Create<ContextActionDealDamage>();
            c.DamageType = new DamageTypeDescription()
            {
                Type = DamageType.Physical,
                Common = new DamageTypeDescription.CommomData(),
                Physical = new DamageTypeDescription.PhysicalData() { Form = physical }
            };
            c.Duration = Helpers.CreateContextDuration(0);
            c.Value = damage;
            c.IsAoE = isAoE;
            c.HalfIfSaved = halfIfSaved;
            c.IgnoreCritical = IgnoreCritical;
            return c;
        }

        public static ContextActionDealDamage CreateActionEnergyDrain(ContextDiceValue damage, ContextDurationValue duration_value, EnergyDrainType drain_type,
                                                                               bool isAoE = false, bool halfIfSaved = false, bool IgnoreCritical = false)
        {
            var c = Create<ContextActionDealDamage>();
            SetField(c, "m_Type", 2 /*EnergyDrain*/);
            c.Duration = duration_value;
            c.EnergyDrainType = drain_type;
            c.Value = damage;
            c.IsAoE = isAoE;
            c.HalfIfSaved = halfIfSaved;
            c.IgnoreCritical = IgnoreCritical;

            return c;
        }


        public static ContextActionDealDamage CreateActionDealDamage(StatType abilityType, ContextDiceValue damage, bool drain = false, bool isAoE = false, bool halfIfSaved = false, bool IgnoreCritical = false)
        {
            var c = Create<ContextActionDealDamage>();
            SetField(c, "m_Type", 1 /*AbilityDamage*/);
            c.Duration = Helpers.CreateContextDuration(0);
            c.AbilityType = abilityType;
            c.Value = damage;
            c.IsAoE = isAoE;
            c.HalfIfSaved = halfIfSaved;
            c.Drain = drain;
            c.IgnoreCritical = IgnoreCritical;
            return c;
        }

        public static AbilityEffectStickyTouch CreateStickyTouch(BlueprintAbility deliverAbility)
        {
            var a = Create<AbilityEffectStickyTouch>();
            a.TouchDeliveryAbility = deliverAbility;
            return a;
        }

        public static AbilityDeliverTouch CreateDeliverTouch()
        {
            var a = Create<AbilityDeliverTouch>();
            a.TouchWeapon = touchWeapon;
            return a;
        }

        public static void AddRecommendNoFeature(this BlueprintAbility spell, BlueprintFeature feature)
        {
            var recommendNoFeat = spell.GetComponents<RecommendationNoFeatFromGroup>().FirstOrDefault(r => r.GoodIfNoFeature = false);
            if (recommendNoFeat != null)
            {
                recommendNoFeat.Features = recommendNoFeat.Features.AddToArray(feature);
            }
            else
            {
                recommendNoFeat = Helpers.Create<RecommendationNoFeatFromGroup>();
                recommendNoFeat.Features = new BlueprintUnitFact[] { feature };
                spell.AddComponent(recommendNoFeat);
            }
        }

        public static void RemoveRecommendNoFeature(this BlueprintAbility spell, BlueprintFeature feature)
        {
            var recommendNoFeat = spell.GetComponents<RecommendationNoFeatFromGroup>().FirstOrDefault(
                r => r.GoodIfNoFeature == false && r.Features.Contains(feature));
            if (recommendNoFeat == null)
            {
                Log.Write($"Warning: couldn't find recommendation for {feature.name} on spell:");
                Log.Write(spell, "  ");
                return;
            }

            var features = recommendNoFeat.Features.ToList();
            features.RemoveAll(f => f == feature);
            if (features.Count > 0)
            {
                recommendNoFeat.Features = features.ToArray();
            }
            else
            {
                spell.RemoveComponent(recommendNoFeat);
            }
        }

        public static void RemoveRecommendNoFeatureGroup(this BlueprintAbility spell, String assetId)
        {
            var recommendNoFeat = spell.GetComponents<RecommendationNoFeatFromGroup>().FirstOrDefault(
                r => r.GoodIfNoFeature == false && r.Features.Any(f => f.AssetGuid == assetId));
            if (recommendNoFeat == null)
            {
                Log.Append($"Warning: couldn't find recommendation for {assetId} on spell:");
                Log.Append(spell, "  ");
                foreach (var rec in spell.GetComponents<RecommendationNoFeatFromGroup>())
                {
                    Log.Append(rec, "  - ");
                    Log.Append($"      GoodIfNoFeature? {rec.GoodIfNoFeature}");
                    foreach (var f in rec.Features)
                    {
                        Log.Append($"      feat {f.name}, id {f.AssetGuid}");
                    }
                }
                Log.Flush();
                return;
            }

            spell.RemoveComponent(recommendNoFeat);
        }

        public static void AddNearSimilarLoot(
            Func<BlueprintItem, int, IEnumerable<BlueprintItem>> match,
            Func<BlueprintScriptableObject, bool> shouldVisit = null)
        {
            foreach (var loot in allLoots)
            {
                if (shouldVisit != null && !shouldVisit(loot)) continue;
                List<LootEntry> newItems = null;
                foreach (var entry in loot.Items)
                {
                    var item = entry.Item;
                    var count = entry.Count;
                    if (item == null) continue;

                    var matched = match(item, count);
                    if (matched == null) continue;

                    newItems = newItems ?? loot.Items.ToList();
                    foreach (var newItem in matched)
                    {
                        newItems.Add(new LootEntry() { Item = newItem, Count = entry.Count });
                    }
                }
                if (newItems != null)
                {
                    loot.Items = newItems.ToArray();
                }
            }
            foreach (var unitLoot in allUnitLoots)
            {
                if (shouldVisit != null) shouldVisit(unitLoot);
                List<BlueprintComponent> newComponents = null;
                foreach (var c in unitLoot.ComponentsArray)
                {
                    var pack = c as LootItemsPackFixed;
                    if (pack == null) continue;

                    var item = pack.Item?.Item;
                    if (item == null) continue;

                    var matched = match(item, pack.Count);
                    if (matched == null) continue;

                    newComponents = newComponents ?? unitLoot.ComponentsArray.ToList();
                    foreach (var newItem in matched)
                    {
                        var lootItem = new LootItem();
                        LootItem_setItem(lootItem, newItem);
                        var newPack = UnityEngine.Object.Instantiate(pack);
                        LootItemsPackFixed_setItem(newPack, lootItem);
                        newComponents.Add(newPack);
                        //Log.Append($"Adding metamagic rod {rod.name} to unitloot: {unitLoot.name}");
                    }
                }
                if (newComponents != null)
                {
                    unitLoot.SetComponents(newComponents);
                }
            }
        }


        static public void AddItemToSpecifiedVendorTable(BlueprintSharedVendorTable vendor_table, BlueprintItem new_item, int amount = 1)
        {
            var loot_item = new LootItem();
            LootItem_setItem(loot_item, new_item);
            var new_pack = Helpers.Create<LootItemsPackFixed>();
            LootItemsPackFixed_setItem(new_pack, loot_item);
            Helpers.SetField(new_pack, "m_Count", amount);
            vendor_table.AddComponent(new_pack);
        }

        static readonly FastSetter LootItem_setItem = CreateFieldSetter<LootItem>("m_Item");
        static readonly FastSetter LootItemsPackFixed_setItem = CreateFieldSetter<LootItemsPackFixed>("m_Item");

        public static BuffScaling CreateBuffScaling(BuffScaling.ScalingType type = BuffScaling.ScalingType.ByCasterLevel, int divModifier = 1, int startingMod = 0, int minimum = 1, BlueprintCharacterClass @class = null)
        {
            return new BuffScaling()
            {
                TypeOfScaling = type,
                Modifier = divModifier,
                ChosenClass = @class,
                StartingMod = startingMod,
                Minimum = minimum
            };
        }

        public static AddStatBonusScaled CreateAddStatBonusScaled(StatType stat, ModifierDescriptor descriptor, BuffScaling scaling, int value = 1)
        {
            var r = Create<AddStatBonusScaled>();
            r.Stat = stat;
            r.Value = value;
            r.Descriptor = descriptor;
            r.Scaling = scaling;
            return r;
        }

        public static BlueprintPortrait createPortrait(string name, string portrait_folder, string guid)
        {
            PortraitData portraitData = new PortraitData("CallOfTheWild" + portrait_folder);
            BlueprintPortrait portrait = Helpers.Create<BlueprintPortrait>();
            portrait.Data = portraitData;
            portrait.name = name;
            Main.library.AddAsset(portrait, guid);
            return portrait;
        }

        public static void AddSpell(BlueprintAbility spell)
        {
            allSpells.Add(spell);
            modSpells.Add(spell);
            if (spell.Type == AbilityType.Spell && spell.AvailableMetamagic == default(Metamagic))
            {
                Log.Write($"Error: spell {spell.name} is missing metamagic (should have heighten, quicken at least)");
            }
            //add to all parametrized features
            BlueprintFeature[] spell_specializations = Main.library.Get<BlueprintFeatureSelection>("fe67bc3b04f1cd542b4df6e28b6e0ff5").AllFeatures;
            spell_specializations = spell_specializations.AddToArray(Main.library.Get<BlueprintParametrizedFeature>("f327a765a4353d04f872482ef3e48c35"), //spell specialization first
                                                                     Main.library.Get<BlueprintParametrizedFeature>("4a2e8388c2f0dd3478811d9c947bebfb"), //arcane bloodline 
                                                                     Main.library.Get<BlueprintParametrizedFeature>("c66e61dea38f3d8479a54eabec20ac99"), //arcane bloodline magus
                                                                     Main.library.Get<BlueprintParametrizedFeature>("ea0ce0aeef8c9e04eadc1ed766455178"), //feyspeaker bonus spells 
                                                                     // mt inquisitor spells
                                                                     Main.library.Get<BlueprintParametrizedFeature>("bcd757ac2aeef3c49b77e5af4e510956"),
                                                                     Main.library.Get<BlueprintParametrizedFeature>("4869109802e135e45af20741f9056fd5"),
                                                                     Main.library.Get<BlueprintParametrizedFeature>("e3a9ed781f9093341ac1073f59018e3f"),
                                                                     Main.library.Get<BlueprintParametrizedFeature>("7668fd94a4f943e4f85ee025a0140434"),
                                                                     Main.library.Get<BlueprintParametrizedFeature>("d3d8b837733879848b549189f02f535c"),
                                                                     Main.library.Get<BlueprintParametrizedFeature>("0495474b37304054eaf016016d0002b4")
                                                                    );
            foreach (BlueprintParametrizedFeature ss in spell_specializations)
            {
                ss.BlueprintParameterVariants = ss.BlueprintParameterVariants.AddToArray(spell);
            }
        }

        public static void AddSpellAndScroll(this BlueprintAbility spell, String scrollIconId, int variant = 0)
        {
            AddSpell(spell);
            AddScroll(spell, scrollIconId, variant);
        }

        public static void AddScroll(BlueprintAbility spell, String scrollIconId, int variant = 0)
        {

            // Figure out the lowest spell level.
            int scrollSpellLevel = 0;
            int scrollCost = 0;
            int scrollCasterLevel = int.MaxValue;
            foreach (var c in spell.GetComponents<SpellListComponent>())
            {
                var spellList = c.SpellList;
                var spellLevel = c.SpellLevel;
                int casterLevel, cost;
                if (spellList.GetSpells(9).Count > 0)
                {
                    cost = fullCasterCost[spellLevel];
                    casterLevel = fullCasterLevel[spellLevel];
                }
                else if (spellList.GetSpells(6).Count > 0)
                {
                    cost = threeQuartersCost[spellLevel];
                    casterLevel = threeQuartersLevel[spellLevel];
                }
                else
                {
                    cost = halfCasterCost[spellLevel];
                    casterLevel = halfCasterLevel[spellLevel];
                }
                if (casterLevel < scrollCasterLevel)
                {
                    scrollCasterLevel = casterLevel;
                    scrollCost = cost;
                    scrollSpellLevel = spellLevel;
                }
            }
            if (scrollCasterLevel == int.MaxValue)
            {
                Log.Write($"Warning: spell {spell.name} did not have any spell lists, can't create scroll");
                return;
            }

            scrollCost += spell.MaterialComponent.GetCost();
            var scroll = Main.library.CopyAndAdd<BlueprintItemEquipmentUsable>(scrollIconId, $"ScrollOf{spell.name}",
                spell.AssetGuid, "600d36ee0d3641388f1f201d0b35059f");
            scroll.CasterLevel = scrollCasterLevel;
            scroll.SpellLevel = scrollSpellLevel;
            SetField(scroll, "m_Cost", scrollCost);
            scroll.DC = 10 + scrollSpellLevel * 3 / 2;

            //remove reference to custom spell (it is present sometimes ?)
            scroll.Ability = spell.HasVariants ? spell.Variants.Skip(variant).First() : spell;
            scroll.ReplaceComponent<CopyScroll>(c => c.CustomSpell = null);

            var zarcies = ResourcesLibrary.TryGetBlueprint<BlueprintSharedVendorTable>("5450d563aab78134196ee9a932e88671");
            var xelliren = ResourcesLibrary.TryGetBlueprint<BlueprintSharedVendorTable>("08e090bb2038e3d47be56d8752d5dcaf");
            AddItemToSpecifiedVendorTable(zarcies, scroll, 5);
            AddItemToSpecifiedVendorTable(xelliren, scroll, 5);
            modScrolls.Add(scroll);
        }

        public static LearnSpellParametrized CreateLearnSpell(this BlueprintSpellList fromList, BlueprintCharacterClass toClass, int spellLevel = 0, int penalty = 0)
        {
            var learn = Helpers.Create<LearnSpellParametrized>();
            learn.SpecificSpellLevel = spellLevel != 0;
            learn.SpellList = fromList;
            learn.SpellLevel = spellLevel;
            learn.SpellcasterClass = toClass;
            learn.SpellLevelPenalty = penalty;
            if (penalty != 0 && spellLevel != 0)
            {
                Log.Write($"Warning: invalid to provide both specific SpellLevel {spellLevel} and " +
                    $"SpellLevelPenalty {penalty} for LearnSpellParametrized");
            }
            return learn;
        }

        public static int PopulationCount(int i)
        {
            i = i - ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }


        public static void RegisterClass(BlueprintCharacterClass class_to_register)
        {
            var progressionRoot = Main.library.Root.Progression;
            var classes = progressionRoot.CharacterClasses.ToList();
            classes.Add(class_to_register);
            classes.Sort((x, y) =>
            {
                if (x.PrestigeClass != y.PrestigeClass) return x.PrestigeClass ? 1 : -1;
                return x.Name.CompareTo(y.Name);
            });
            progressionRoot.CharacterClasses = classes.ToArray();
            Helpers.classes.Add(class_to_register);

            //fix spell specialization
            var spell_specialization_progression = Main.library.Get<BlueprintProgression>("fe9220cdc16e5f444a84d85d5fa8e3d5");
            spell_specialization_progression.Classes = spell_specialization_progression.Classes.AddToArray(class_to_register);
        }

        internal static Condition CreateConditionHasFact(object undeadType)
        {
            throw new NotImplementedException();
        }

        static readonly int[] fullCasterCost = new int[] { 13, 25, 150, 375, 700, 1125, 1650, 2275, 3000, 3825 };
        static readonly int[] threeQuartersCost = new int[] { 13, 25, 200, 525, 1000, 1625, 2400 };
        static readonly int[] halfCasterCost = new int[] { 13, 125, 400, 825, 1300 };

        static readonly int[] fullCasterLevel = new int[] { 1, 1, 3, 5, 7, 9, 11, 13, 15, 17 };
        static readonly int[] threeQuartersLevel = new int[] { 1, 1, 4, 7, 10, 13, 16 };
        static readonly int[] halfCasterLevel = new int[] { 1, 4, 7, 10, 13 };

        // TODO: game log messages need to use a localization helpers
        public static BattleLogView GameLog => Game.Instance.UI.BattleLogManager.LogView;
    }

    public static class Log
    {
        static readonly StringBuilder str = new StringBuilder();

        public static void Flush()
        {
            if (str.Length == 0) return;
            Main.logger.Log(str.ToString());
            str.Clear();
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Write(String message)
        {
            Append(message);
            Flush();
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Append(String message)
        {
            str.AppendLine(message);
        }

        public static void Error(Exception e)
        {
            str.AppendLine(e.ToString());
            Flush();
        }

        public static void Error(String message)
        {
            str.AppendLine(message);
            Flush();
        }

        public static void Append(LevelEntry level, String indent = "", bool showSelection = false)
        {
            Append($"{indent}level {level.Level}");
            foreach (var f in level.Features)
            {
                Append(f, $"{indent}  | ", showSelection);
            }
        }

        public static void Write(BlueprintScriptableObject fact, String indent = "", bool showSelection = false)
        {
            Append(fact, indent, showSelection);
            Flush();
        }

        public static void Append(BlueprintScriptableObject fact, String indent = "", bool showSelection = false)
        {
            if (fact == null)
            {
                Append($"{indent}BlueprintScriptableObject is null");
                return;
            }

            var @class = fact as BlueprintCharacterClass;
            var displayName = (fact as BlueprintUnitFact)?.Name ?? @class?.Name;
            Append($"{indent}fact {fact.name}, display name: \"{displayName}\", type: {fact.GetType().Name}, id {fact.AssetGuid}");
            var race = fact as BlueprintRace;
            if (race != null)
            {
                Append($"{indent} race {race.RaceId} size {race.Size} features:");
                foreach (var f in race.Features)
                {
                    Append(f, $"{indent}    ");
                }
            }
            var selection = fact as BlueprintFeatureSelection;
            if (selection != null && showSelection)
            {
                Append($"{indent}  Group {selection.Group}, Group2 {selection.Group2 + $", Mode {selection.Mode}, AllFeatures:"}");
                foreach (var g in selection.AllFeatures)
                {
                    Append(g, $"{indent}    ");
                }
            }
            var parameterized = fact as BlueprintParametrizedFeature;
            if (parameterized != null)
            {
                Append($"{indent}  param type {parameterized.ParameterType}");
                Append($"{indent}  prereq {parameterized.Prerequisite}");
                switch (parameterized.ParameterType)
                {
                    case FeatureParameterType.WeaponCategory:
                        Append($"{indent}  weapon sub category " + parameterized.WeaponSubCategory +
                            $", req proficient? {parameterized.RequireProficiency}");
                        break;
                    case FeatureParameterType.Skill:
                    case FeatureParameterType.SpellSchool:
                        break;
                    case FeatureParameterType.LearnSpell:
                        Append($"{indent}  spellcaster class {parameterized.SpellcasterClass}");
                        if (parameterized.SpecificSpellLevel)
                        {
                            Append($"{indent}  specific spell level {parameterized.SpellLevel}");
                        }
                        else
                        {
                            Append($"{indent}  spell level penalty {parameterized.SpellLevelPenalty}");
                        }
                        goto case FeatureParameterType.SpellSpecialization;
                    case FeatureParameterType.Custom:
                        Append($"{indent}  has no such feature? {parameterized.HasNoSuchFeature}");
                        goto case FeatureParameterType.SpellSpecialization;
                    case FeatureParameterType.SpellSpecialization:
                        Append($"{indent}  param variants: {parameterized.BlueprintParameterVariants.Length}");
                        break;
                }
            }
            var feature = fact as BlueprintFeature;
            if (feature != null)
            {
                foreach (var g in feature.Groups)
                {
                    Append($"{indent}  group {g}");
                }
            }
            var ability = fact as BlueprintAbility;
            if (ability != null)
            {
                Append($"{indent}  type {ability.Type} action type " + ability.ActionType +
                    $" metamagic {ability.AvailableMetamagic.ToString("x")} descriptor " + ability.SpellDescriptor.ToString("x"));
                var resources = ability.ResourceAssetIds;
                if (resources != null && resources.Length > 0)
                {
                    Append($"{indent}  resource ids: {string.Join(",", resources)}");
                }
            }
            var buff = fact as BlueprintBuff;
            if (buff != null)
            {
                Append($"{indent}buff frequency {buff.Frequency} stacking {buff.Stacking}");
            }
            if (@class != null)
            {
                Append($"{indent}progression:");
                Append(@class.Progression, $"{indent}  ");
            }
            Append($"{indent}components:");
            foreach (var c in fact.ComponentsArray)
            {
                Append(c, $"{indent}  ");
            }
            var progression = fact as BlueprintProgression;
            if (progression != null)
            {
                foreach (var c in progression.Classes)
                {
                    Append($"{indent}  class {c.name}");
                }
                foreach (var a in progression.Archetypes)
                {
                    Append($"{indent}  archetype {a.name}");
                }
                foreach (var g in progression.UIGroups)
                {
                    Append($"{indent}  ui group");
                    foreach (var f in g.Features)
                    {
                        Append($"{indent}    {f.name}");
                    }
                }
                foreach (var level in progression.LevelEntries)
                {
                    Append(level, $"{indent}  ", showSelection);
                }
            }
        }

        public static void Append(BlueprintComponent c, String indent = "")
        {
            var spellComp = c as SpellComponent;
            if (spellComp != null)
            {
                Append($"{indent}SpellComponent school {spellComp.School.ToString()}");
                return;
            }
            var spellList = c as SpellListComponent;
            if (spellList != null)
            {
                Append($"{indent}SpellListComponent {spellList.SpellList.name} level {spellList.SpellLevel}");
                return;
            }
            var spellDesc = c as SpellDescriptorComponent;
            if (spellDesc != null)
            {
                Append($"{indent}SpellDescriptorComponent " + spellDesc.Descriptor.Value.ToString("x"));
                return;
            }
            var execOnCast = c as AbilityExecuteActionOnCast;
            if (execOnCast != null)
            {
                Append($"{indent}AbilityExecuteActionOnCast:");
                var conditions = execOnCast.Conditions;
                if (conditions != null)
                {
                    Append($"{indent}  conditions op {conditions.Operation}:");
                    foreach (var cond in conditions.Conditions)
                    {
                        Append($"{indent}  " + (cond.Not ? "not " : " ") + c.GetType().Name + $" {cond.GetCaption()}");
                    }
                }
                Append($"{indent}  actions:");
                foreach (var action in execOnCast.Actions.Actions)
                {
                    Append(action, $"{indent}    ");
                }
            }
            var runActions = c as AbilityEffectRunAction;
            if (runActions != null)
            {
                Append($"{indent}AbilityEffectRunAction saving throw {runActions.SavingThrowType}");
                foreach (var action in runActions.Actions.Actions)
                {
                    Append(action, $"{indent}  ");
                }
                return;
            }
            var config = c as ContextRankConfig;
            if (config != null)
            {
                Func<String, object> field = (name) => Helpers.GetField(config, name);

                var progression = (ContextRankProgression)field("m_Progression");
                Append($"{indent}ContextRankConfig type {config.Type} value type " +
                    ((ContextRankBaseValueType)field("m_BaseValueType")) + $" progression {progression}");

                if (config.IsBasedOnClassLevel)
                {
                    Append($"{indent}  class level " + ((BlueprintCharacterClass[])field("m_Class"))?.StringJoin(b => b.name));
                    Append($"{indent}  except classes? " + (bool)field("m_ExceptClasses"));
                }
                if (config.RequiresArchetype) Append($"{indent}  archetype " + ((BlueprintArchetype)field("Archetype")).name);
                if (config.IsBasedOnFeatureRank) Append($"{indent}  feature rank " + ((BlueprintFeature)field("m_Feature")).name);
                if (config.IsFeatureList) Append($"{indent}  feature list " + ((BlueprintFeature[])field("m_FeatureList"))?.StringJoin(f => f.name));
                if (config.IsBasedOnStatBonus) Append($"{indent}  stat bonus " + ((StatType)field("m_Stat")));
                if ((bool)field("m_UseMax")) Append($"{indent}  max " + ((int)field("m_Max")));
                if ((bool)field("m_UseMin")) Append($"{indent}  min " + ((int)field("m_Min")));
                if (config.IsDivisionProgression) Append($"{indent}  start level " + ((int)field("m_StartLevel")));
                if (config.IsDivisionProgressionStart) Append($"{indent}  step level " + ((int)field("m_StepLevel")));
                if (progression == ContextRankProgression.Custom)
                {
                    Append($"{indent}  custom progression:");
                    foreach (var p in (IEnumerable<object>)field("m_CustomProgression"))
                    {
                        Func<String, int> field2 = (name) => (int)Helpers.GetField(p, name);
                        Append($"{indent}    base value {field2("BaseValue")} progression {field2("ProgressionValue")}");
                    }
                }
                return;
            }

            Append($"{indent}component {c.name}, type {c.GetType().Name}");

            var abilityVariants = c as AbilityVariants;
            if (abilityVariants != null)
            {
                foreach (var v in abilityVariants.Variants)
                {
                    Append(v, $"{indent}    ");
                }
            }
            var polymorph = c as Polymorph;
            if (polymorph != null)
            {
                foreach (var f in polymorph.Facts)
                {
                    Append(f, $"{indent}    ");
                }
            }
            var stickyTouch = c as AbilityEffectStickyTouch;
            if (stickyTouch != null)
            {
                Append(stickyTouch.TouchDeliveryAbility, $"{indent}  ");
            }
            var addIf = c as AddFeatureIfHasFact;
            if (addIf != null)
            {
                Append($"{indent}  if {(addIf.Not ? "!" : "")}{addIf.CheckedFact.name} then add {addIf.Feature.name}");
                Append(addIf.Feature, $"{indent}    ");
            }
            var addOnLevel = c as AddFeatureOnClassLevel;
            if (addOnLevel != null)
            {
                Append($"{indent}  if level {(addOnLevel.BeforeThisLevel ? "before " : "")}{addOnLevel.Level} then add {addOnLevel.Feature.name}");
                Append(addOnLevel.Feature, $"{indent}    ");
            }
            var addFacts = c as AddFacts;
            if (addFacts != null)
            {
                Append($"{indent}  add facts");
                foreach (var f in addFacts.Facts)
                {
                    Append(f, $"{indent}    ");
                }
            }
            var prereq = c as Prerequisite;
            if (prereq != null)
            {
                Append($"{indent}  prerequisite group {prereq.Group}");

                var log = new StringBuilder();
                Action<String, object> logIf = (desc, name) =>
                {
                    if (name != null) log.Append(desc + name);
                };
                logIf(" class ", (prereq as PrerequisiteClassLevel)?.CharacterClass.name);
                logIf(" level ", (prereq as PrerequisiteClassLevel)?.Level);
                logIf(" feature ", (prereq as PrerequisiteFeature)?.Feature.name);
                logIf(" no feature ", (prereq as PrerequisiteNoFeature)?.Feature.name);
                logIf(" stat ", (prereq as PrerequisiteStatValue)?.Stat.ToString());
                logIf(" value ", (prereq as PrerequisiteStatValue)?.Value);
                Append($"{indent}   {log.ToString()}");
            }
        }

        public static void Append(GameAction action, String indent = "")
        {
            try
            {
                if (action == null)
                {
                    Append($"{indent}GameAction is null");
                    return;
                }

                {
                    Append(indent + action.GetType().Name + $" {action.GetCaption()}");
                }
                {
                    var a = action as Conditional;
                    if (a != null)
                    {
                        Append($"{indent}  operation {a.ConditionsChecker?.Operation}");
                        var conditions = a.ConditionsChecker?.Conditions;
                        if (conditions != null)
                        {
                            foreach (var c in conditions)
                            {
                                Append($"{indent}  {(c.Not ? "not " : " ")}{c.GetType().Name} {c.GetCaption()}");
                            }
                        }
                        if (a.IfTrue?.HasActions == true)
                        {
                            Append($"{indent}  if true:");
                            foreach (var nested in a.IfTrue.Actions)
                            {
                                Append(nested, $"{indent}   ");
                            }
                        }
                        if (a.IfFalse?.HasActions == true)
                        {
                            Append($"{indent}  if false:");
                            foreach (var nested in a.IfFalse.Actions)
                            {
                                Append(nested, $"{indent}   ");
                            }
                        }
                    }
                }
                {
                    var a = action as ContextActionConditionalSaved;
                    if (a != null)
                    {
                        Append($"{indent}  succeeded:");
                        if (a.Succeed?.HasActions == true)
                        {
                            foreach (var nested in a.Succeed.Actions ?? Array.Empty<GameAction>())
                            {
                                Append(nested, $"{indent}    ");
                            }
                        }
                        if (a.Failed?.HasActions == true)
                        {
                            Append($"{indent}  failed:");
                            foreach (var nested in a.Failed.Actions ?? Array.Empty<GameAction>())
                            {
                                Append(nested, $"{indent}    ");
                            }
                        }
                    }
                }
                {
                    var a = action as ContextActionSavingThrow;
                    if (a != null)
                    {
                        foreach (var nested in a.Actions.Actions)
                        {
                            Append(nested, $"{indent}  ");
                        }
                    }
                }
                {
                    var a = action as ContextActionApplyBuff;
                    if (a != null)
                    {
                        Append(a.Buff, $"{indent}  ");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error evaluating {action?.GetType()}:\n{e}");
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Validate(BlueprintComponent c, BlueprintScriptableObject parent)
        {
            c.Validate(validation);
            if (validation.HasErrors)
            {
                Append($"Error: component `{c.name}` of `{parent.name}` failed validation:");
                foreach (var e in validation.Errors) Append($"  {e}");
                ((List<ValidationContext.Error>)validation.ErrorsAdvanced).Clear();
                Flush();
            }
        }

        public static void MaybeFlush()
        {
            if (str.Length > 4096) Flush();
        }

        static ValidationContext validation = new ValidationContext();
    }


    public delegate void FastSetter(object source, object value);
    public delegate object FastGetter(object source);
    public delegate object FastInvoke(object target, params object[] paramters);
}
