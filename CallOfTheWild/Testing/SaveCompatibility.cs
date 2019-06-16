// Copyright (c) 2019 Jennifer Messerly
// This code is licensed under MIT license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.LevelUp;
using Newtonsoft.Json;
using UnityEngine;
using UnityModManagerNet;

namespace EldritchArcana
{
    static class SaveCompatibility
    {
        [Conditional("DEBUG")]
        public static void CheckComponent(BlueprintScriptableObject obj, BlueprintComponent c)
        {
#if DEBUG
            var type = c.GetType();
            if (IsStatefulComponent(type))
            {
                statefulComponentMessage.AppendLine($"Warning: in object {obj.name}, stateful {type.Name} should be named.");
            }
#endif
        }

        public static void CheckCompat()
        {
#if DEBUG
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // If this doesn't match the previous baseline (or it didn't exist), write the current state.
            if (MatchesBaseline(Path.Combine(dir, "baseline_assets.txt"))) return;

            WriteAssetInfo(Path.Combine(dir, "current_assets.txt"), GetCurrentAssetInfo());

            Log.Write(statefulComponentMessage.ToString());
#endif
        }

#if DEBUG
        static SortedDictionary<String, SortedSet<String>> GetCurrentAssetInfo()
        {
            var assets = new SortedDictionary<String, SortedSet<String>>();
            foreach (var asset in ExtensionMethods.newAssets)
            {
                if (asset is BlueprintPortrait) continue;

                var components = new SortedSet<String>();
                foreach (var component in asset.ComponentsArray)
                {
                    var fields = new SortedSet<String>();
                    // Don't track compatibility for the base game components.
                    if (!component.GetType().Namespace.StartsWith("Kingmaker"))
                    {
                        foreach (var field in GetStatefulComponentFields(component.GetType()))
                        {
                            fields.Add($"{field.Name}|{field.FieldType.FullName}");
                        }
                    }
                    if (string.IsNullOrEmpty(component.name))
                    {
                        Log.Write($"Warning: component {component.GetType().Name} of {asset.name} is missing a name");
                    }
                    components.Add(component.name + (fields.Count == 0 ? "" : ";" + string.Join(";", fields)));
                }
                // Note: the name of the asset does not appear to be used for compatibility, but
                // it makes it much easier to see what's going on in the baseline file.
                assets.Add($"{asset.name}|{asset.AssetGuid}", components);
            }
            return assets;
        }

        static bool MatchesBaseline(string baselinePath)
        {
            if (!File.Exists(baselinePath))
            {
                Log.Write($"Warning: baseline does not exist at path {baselinePath}");
                return false;
            }

            var baseline = ReadAssetInfo(baselinePath);
            var current = GetCurrentAssetInfo();

            // Check that all baseline assets are still found.
            var success = DiffAndReportAssets(baseline, current, "Missing", $"Error: missing assets from baseline {baselinePath}");
            // Also report new components, to indicate the baseline needs to be updated.
            var success2 = DiffAndReportAssets(current, baseline, "New", $"Info: new assets, baseline needs to be updated at {baselinePath}");
            return success && success2;
        }

        static bool DiffAndReportAssets(SortedDictionary<String, SortedSet<String>> baseline, SortedDictionary<String, SortedSet<String>> current, string missingOrNew, string introMessage)
        {
            var missingAssets = new List<String>();
            var missingComponents = new List<String>();
            foreach (var id in baseline.Keys)
            {
                SortedSet<String> currentAsset;
                if (!current.TryGetValue(id, out currentAsset))
                {
                    missingAssets.Add(id);
                    continue;
                }
                missingComponents.AddRange(baseline[id].Where(b => !currentAsset.Contains(b)).Select(c => $"{id}::{c}"));
            }
            if (missingAssets.Count > 0 || missingComponents.Count > 0)
            {
                Log.Append(introMessage);
                Log.Append($"{missingOrNew} assets:");
                missingAssets.ForEach(id => Log.Append($"  - {id}"));
                Log.Append($"{missingOrNew} components:");
                missingComponents.ForEach(c => Log.Append($"  - {c}"));
                Log.Flush();
                return false;
            }
            return true;
        }

        static void WriteAssetInfo(string assetPath, SortedDictionary<String, SortedSet<String>> info)
        {
            var lines = new List<String>();
            foreach (var pair in info)
            {
                lines.Add(pair.Key);
                foreach (var item in pair.Value) lines.Add($"  {item}");
            }
            File.WriteAllLines(assetPath, lines);
        }

        static SortedDictionary<String, SortedSet<String>> ReadAssetInfo(string assetPath)
        {
            var assets = new SortedDictionary<String, SortedSet<String>>();
            SortedSet<String> components = null;
            foreach (var line in File.ReadAllLines(assetPath))
            {
                if (line.StartsWith("  ") && components != null)
                {
                    components.Add(line.TrimStart());
                    continue;
                }
                assets.Add(line, components = new SortedSet<string>());
            }
            return assets;
        }

        static bool IsStatefulComponent(Type type) => GetStatefulComponentFields(type).Count > 0;

        internal static List<FieldInfo> GetStatefulComponentFields(Type type)
        {
            List<FieldInfo> fields;
            if (statefulComponentFields.TryGetValue(type, out fields)) return fields;

            fields = new List<FieldInfo>();
            for (var t = type; t != null; t = t.BaseType)
            {
                fields.AddRange(t.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(
                    f => f.CustomAttributes.Any(a => a.AttributeType == typeof(JsonPropertyAttribute) ||
                        a.AttributeType == typeof(SerializableAttribute))));
            }
            statefulComponentFields.Add(type, fields);
            return fields;
        }

        static readonly Dictionary<Type, List<FieldInfo>> statefulComponentFields = new Dictionary<Type, List<FieldInfo>>();

        static readonly StringBuilder statefulComponentMessage = new StringBuilder();
#endif
    }
}
