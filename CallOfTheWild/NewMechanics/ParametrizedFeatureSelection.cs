using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.NewMechanics.ParametrizedFeatureSelection
{

    public enum FeatureParameterTypeExtender
    {
        KnownSpell = 20,
        KnownSpontaneousSpell = 21,
    }


    [Harmony12.HarmonyPatch(typeof(BlueprintParametrizedFeature))]
    [Harmony12.HarmonyPatch("ExtractSelectionItems", Harmony12.MethodType.Normal)]
    class BlueprintParametrizedFeature__ExtractSelectionItems__Patch
    {
        static bool Prefix(BlueprintParametrizedFeature __instance, UnitDescriptor beforeLevelUpUnit, UnitDescriptor previewUnit, ref IEnumerable<IFeatureSelectionItem> __result)
        {
            switch (__instance.ParameterType)
            {
                case (FeatureParameterType)FeatureParameterTypeExtender.KnownSpell:
                    __result = (IEnumerable < IFeatureSelectionItem > )ExtractKnownSpells(__instance, previewUnit, false).ToArray<FeatureUIData>();
                    return false;
                case FeatureParameterType.SpellSpecialization:
                    __result = (IEnumerable<IFeatureSelectionItem>)ExtractItemsFromSpellbooks2(__instance, previewUnit).ToArray<FeatureUIData>();
                    return false;
                case (FeatureParameterType)FeatureParameterTypeExtender.KnownSpontaneousSpell:
                    __result = (IEnumerable<IFeatureSelectionItem>)ExtractKnownSpells(__instance, previewUnit, true).ToArray<FeatureUIData>();
                    return false;
                default:
                    return true;
            }
        }

        static private IEnumerable<FeatureUIData> ExtractKnownSpells(BlueprintParametrizedFeature feature, UnitDescriptor unit, bool only_spontaneous)
        {
            foreach (Spellbook spellbook in unit.Spellbooks)
            {               
                if (only_spontaneous && !spellbook.Blueprint.Spontaneous)
                {
                    continue;
                }

                if (!only_spontaneous && spellbook.Blueprint.GetComponent<SpellbookMechanics.GetKnownSpellsFromMemorizationSpellbook>() != null)
                {
                    continue;
                }
                foreach (var spell in spellbook.GetAllKnownSpells())
                {
                    if (spell.MetamagicData != null && spell.MetamagicData.MetamagicMask != 0)
                    {
                        continue;
                    }

                    yield return new FeatureUIData(feature, spell.Blueprint, spell.Blueprint.Name, spell.Blueprint.Description, spell.Blueprint.Icon, spell.Blueprint.name);
                }
            }
            yield break;
        }


        static private IEnumerable<FeatureUIData> ExtractItemsFromSpellbooks2(BlueprintParametrizedFeature feature, UnitDescriptor unit)
        {
            foreach (Spellbook spellbook in unit.Spellbooks)
            {
                if (spellbook.Blueprint.GetComponent<SpellbookMechanics.GetKnownSpellsFromMemorizationSpellbook>() != null)
                {
                    continue;
                }
                foreach (SpellLevelList spellLevel in spellbook.Blueprint.SpellList.SpellsByLevel)
                {
                    if (spellLevel.SpellLevel <= spellbook.MaxSpellLevel)
                    {
                        foreach (BlueprintAbility spell in spellLevel.SpellsFiltered)
                        {
                            if (unit.GetFeature(feature.Prerequisite, spell.School) != null)
                            {
                                yield return new FeatureUIData(feature, spell, spell.Name, spell.Description, spell.Icon, spell.name);
                            }
                        }
                    }
                }
            }
            yield break;
        }
    }





    [Harmony12.HarmonyPatch(typeof(BlueprintParametrizedFeature))]
    [Harmony12.HarmonyPatch("GetFullSelectionItems", Harmony12.MethodType.Normal)]
    class BlueprintParametrizedFeature__GetFullSelectionItems__Patch
    {
        static bool Prefix(BlueprintParametrizedFeature __instance, ref FeatureUIData[] ___m_CachedItems, ref IEnumerable<FeatureUIData> __result)
        {
            if (___m_CachedItems != null)
            {
                return true;
            }

            var tr = Harmony12.Traverse.Create(__instance);
            switch (__instance.ParameterType)
            {
                case (FeatureParameterType)FeatureParameterTypeExtender.KnownSpell:
                case (FeatureParameterType)FeatureParameterTypeExtender.KnownSpontaneousSpell:
                    ___m_CachedItems = tr.Method("ExtractItemsFromBlueprints", (IEnumerable<BlueprintScriptableObject>)__instance.BlueprintParameterVariants).GetValue<IEnumerable<FeatureUIData>>().ToArray<FeatureUIData>();
                    return true;
                default:
                    return true;
            }
        }
    }



}
