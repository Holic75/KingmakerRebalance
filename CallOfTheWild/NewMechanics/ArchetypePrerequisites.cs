using Kingmaker;
using Kingmaker.Assets.UI.LevelUp;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.UI.LevelUp;
using Kingmaker.UI.Tooltip;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.ArchetypePrerequisites
{



    [Harmony12.HarmonyPatch(typeof(DescriptionTemplatesLevelup), "LevelUpClassPrerequisites", typeof(DescriptionBricksBox), typeof(TooltipData), typeof(bool))]
    static class DescriptionTemplatesLevelup_LevelUpClassPrerequisites_Patch
    {
        static void Postfix(DescriptionTemplatesLevelup __instance, DescriptionBricksBox box, TooltipData data, bool b)
        {
            try
            {
                if (data?.Archetype == null) return;
                Prerequisites(__instance, box, data.Archetype.GetComponents<Prerequisite>(), data.ParentFeatureSelection);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        static readonly FastInvoke Prerequisites = Helpers.CreateInvoker<DescriptionTemplatesLevelup>("Prerequisites", new Type[] { typeof(DescriptionBricksBox), typeof(IEnumerable<Prerequisite>), typeof(FeatureSelectionState) });
    }

    [Harmony12.HarmonyPatch(typeof(CharBSelectorLayer), "FillData", typeof(BlueprintCharacterClass), typeof(BlueprintArchetype[]), typeof(CharBFeatureSelector))]
    static class CharBSelectorLayer_FillData_Patch
    {
        static void Postfix(CharBSelectorLayer __instance, BlueprintCharacterClass charClass, BlueprintArchetype[] archetypesList)
        {
            try
            {
                var self = __instance;
                var items = self.SelectorItems;
                if (items == null || archetypesList == null || items.Count == 0)
                {
                    return;
                }

                // Note: conceptually this is the same as `CharBSelectorLayer.FillDataLightClass()`,
                // but for archetypes.

                // TODO: changing race won't refresh the prereq, although it does update if you change class.
                var state = Game.Instance.UI.CharacterBuildController.LevelUpController.State;
                foreach (var item in items)
                {
                    var archetype = item?.Archetype;
                    if (archetype == null || !archetypesList.Contains(archetype)) continue;

                    item.Show(state: true);
                    item.Toggle.interactable = item.enabled = MeetsPrerequisites(archetype, state.Unit, state);
                    var classData = state.Unit.Progression.GetClassData(state.SelectedClass);
                    self.SilentSwitch(classData.Archetypes.Contains(archetype), item);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }


        static bool MeetsPrerequisites(BlueprintArchetype archetype, UnitDescriptor unit, LevelUpState state)
        {
            bool? all = null;
            bool? any = null;
            foreach (var prerequisite in archetype.GetComponents<Prerequisite>())
            {
                var passed = prerequisite.Check(null, unit, state);
                if (prerequisite.Group == Prerequisite.GroupType.All)
                {
                    all = (!all.HasValue) ? passed : (all.Value && passed);
                }
                else
                {
                    any = (!any.HasValue) ? passed : (any.Value || passed);
                }
            }
            var result = (!all.HasValue || all.Value) && (!any.HasValue || any.Value);
            return result;
        }
    }

    [Harmony12.HarmonyPatch(typeof(CharacterBuildController), "SetRace", typeof(BlueprintRace))]
    static class CharacterBuildController_SetRace_Patch
    {
        static bool Prefix(CharacterBuildController __instance, BlueprintRace race)
        {
            try
            {
                if (race == null) return true;
                var self = __instance;
                var levelUp = self.LevelUpController;
                var @class = levelUp.State.SelectedClass;
                if (@class == null) return true;

                if (@class.Archetypes.Any(a => a.GetComponents<Prerequisite>() != null))
                {
                    self.SetArchetype(null);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return true;
        }
    }


    [Harmony12.HarmonyPatch(typeof(AddArchetype), "Apply", typeof(LevelUpState), typeof(UnitDescriptor))]
    static class AddArchetype_Apply_Patch
    {
        static void Postfix(AddArchetype __instance, LevelUpState state, UnitDescriptor unit)
        {
            if (__instance?.Archetype != null)
            {
                RestrictPrerequisites(__instance.Archetype, unit, state);
            }
        }

        static public void RestrictPrerequisites(BlueprintArchetype archetype, UnitDescriptor unit, LevelUpState state)
        {
            if (IgnorePrerequisites.Ignore)
                return;
            for (int index = 0; index < archetype.ComponentsArray.Length; ++index)
            {
                Prerequisite components = archetype.ComponentsArray[index] as Prerequisite;
                if ((bool)((UnityEngine.Object)components))
                    components.Restrict((FeatureSelectionState)null, unit, state);
            }
        }
    }
}
