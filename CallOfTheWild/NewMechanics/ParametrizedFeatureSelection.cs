using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
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
        AllLearnableSpells = 22,
        AvailableSpell = 23
    }


    public class MaxLearneableSpellLevelLimiter : BlueprintComponent
    {
        public int max_lvl = 9;
    }


    [Harmony12.HarmonyPatch(typeof(BlueprintParametrizedFeature))]
    [Harmony12.HarmonyPatch("ExtractSelectionItems", Harmony12.MethodType.Normal)]
    class BlueprintParametrizedFeature__ExtractSelectionItems__Patch
    {
        static bool Prefix(BlueprintParametrizedFeature __instance, UnitDescriptor beforeLevelUpUnit, UnitDescriptor previewUnit, ref IEnumerable<IFeatureSelectionItem> __result)
        {
            Main.TraceLog();
            switch (__instance.ParameterType)
            {
                case (FeatureParameterType)FeatureParameterTypeExtender.KnownSpell:
                    __result = (IEnumerable < IFeatureSelectionItem > )ExtractKnownSpells(__instance, previewUnit, false).ToArray<FeatureUIData>();
                    return false;
                case (FeatureParameterType)FeatureParameterTypeExtender.AvailableSpell:
                    __result = (IEnumerable<IFeatureSelectionItem>)ExtractAvailableSpells(__instance, previewUnit).ToArray<FeatureUIData>();
                    return false;
                case (FeatureParameterType)FeatureParameterTypeExtender.AllLearnableSpells:
                    __result = (IEnumerable<IFeatureSelectionItem>)ExtractAllLearnableSpells(__instance, previewUnit).ToArray<FeatureUIData>();
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
                if (feature.SpellcasterClass != null
                    && spellbook != unit.GetSpellbook(feature.SpellcasterClass))
                {
                    continue;
                }
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


        static private IEnumerable<FeatureUIData> ExtractAvailableSpells(BlueprintParametrizedFeature feature, UnitDescriptor unit)
        {
            foreach (Spellbook spellbook in unit.Spellbooks)
            {
                if (feature.SpellcasterClass != null
                    && spellbook != unit.GetSpellbook(feature.SpellcasterClass))
                {
                    continue;
                }

              

                foreach (var spell in spellbook.Blueprint.SpellList.GetSpells(feature.SpellLevel))
                {
                    yield return new FeatureUIData(feature, spell, spell.Name, spell.Description, spell.Icon, spell.name);
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
                foreach (var spell in spellbook.GetAllKnownSpells().Select(s => s.Blueprint).Distinct().ToArray())
                {
                    if (unit.GetFeature(feature.Prerequisite, spell.School) != null)
                    {
                        yield return new FeatureUIData(feature, spell, spell.Name, spell.Description, spell.Icon, spell.name);
                    }
                }
                /*foreach (SpellLevelList spellLevel in spellbook.Blueprint.SpellList.SpellsByLevel)
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
                }*/
            }
            yield break;
        }


        static private IEnumerable<FeatureUIData> ExtractAllLearnableSpells(BlueprintParametrizedFeature feature, UnitDescriptor unit)
        {
            var max_lvl = feature.GetComponent<MaxLearneableSpellLevelLimiter>().max_lvl;
            foreach (Spellbook spellbook in unit.Spellbooks)
            {
                if (spellbook.Blueprint.GetComponent<SpellbookMechanics.GetKnownSpellsFromMemorizationSpellbook>() != null)
                {
                    continue;
                }
                foreach (SpellLevelList spellLevel in spellbook.Blueprint.SpellList.SpellsByLevel)
                {
                    if (spellLevel.SpellLevel <= max_lvl)
                    {
                        foreach (BlueprintAbility spell in spellLevel.SpellsFiltered)
                        {
                           yield return new FeatureUIData(feature, spell, spell.Name, spell.Description, spell.Icon, spell.name);
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
            Main.TraceLog();
            if (___m_CachedItems != null)
            {
                return true;
            }

            var tr = Harmony12.Traverse.Create(__instance);
            switch (__instance.ParameterType)
            {
                case (FeatureParameterType)FeatureParameterTypeExtender.AllLearnableSpells:
                case (FeatureParameterType)FeatureParameterTypeExtender.KnownSpell:
                case (FeatureParameterType)FeatureParameterTypeExtender.KnownSpontaneousSpell:
                case (FeatureParameterType)FeatureParameterTypeExtender.AvailableSpell:
                    ___m_CachedItems = tr.Method("ExtractItemsFromBlueprints", (IEnumerable<BlueprintScriptableObject>)__instance.BlueprintParameterVariants).GetValue<IEnumerable<FeatureUIData>>().ToArray<FeatureUIData>();
                    return true;
                default:
                    return true;
            }
        }
    }


    [Harmony12.HarmonyPatch(typeof(AddClassLevels))]
    [Harmony12.HarmonyPatch("PerformSelections", Harmony12.MethodType.Normal)]
    class Patch_AddClassLevels_PerformSelections_Transpiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var check_parameter_type_index = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Ldfld && x.operand.ToString().Contains("ParameterType"));

            codes[check_parameter_type_index] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                           new Func<BlueprintParametrizedFeature, FeatureParameterType>(getParameterType).Method
                                                                           );
            return codes.AsEnumerable();
        }


        static private FeatureParameterType getParameterType(BlueprintParametrizedFeature feature)
        {
            Main.TraceLog();
            switch (feature.ParameterType)
            {
                case (FeatureParameterType)NewMechanics.ParametrizedFeatureSelection.FeatureParameterTypeExtender.KnownSpell:
                case (FeatureParameterType)NewMechanics.ParametrizedFeatureSelection.FeatureParameterTypeExtender.AllLearnableSpells:
                case (FeatureParameterType)NewMechanics.ParametrizedFeatureSelection.FeatureParameterTypeExtender.AvailableSpell:
                case (FeatureParameterType)NewMechanics.ParametrizedFeatureSelection.FeatureParameterTypeExtender.KnownSpontaneousSpell:

                    return FeatureParameterType.SpellSpecialization;
                default:
                    return feature.ParameterType;
            }
        }
    }



}
