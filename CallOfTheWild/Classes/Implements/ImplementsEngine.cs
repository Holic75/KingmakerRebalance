using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    public partial class ImplementsEngine
    {
        static LibraryScriptableObject library => Main.library;
        BlueprintCharacterClass[] classes;
        BlueprintArchetype archetype;
        StatType stat;
        BlueprintAbilityResource resource;
        string prefix;

        public ImplementsEngine(string name_prefix, BlueprintAbilityResource ability_resource, BlueprintCharacterClass[] scaling_classes, StatType scaling_stat, BlueprintArchetype scaling_archetype = null)
        {
            stat = scaling_stat;
            classes = scaling_classes;
            archetype = scaling_archetype;
            resource = ability_resource;
            prefix = name_prefix + "Implement";
        }

        BlueprintArchetype[] getArchetypeArray()
        {
            return archetype == null ? null : new BlueprintArchetype[] { archetype };
        }


        ContextRankConfig createClassScalingConfig(
                    ContextRankProgression progression = ContextRankProgression.AsIs,
                    AbilityRankType type = AbilityRankType.Default,
                    int? min = null, int? max = null, int startLevel = 0, int stepLevel = 0,
                    (int, int)[] customProgression = null)
        {
            if (archetype == null)
            {
                return Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, progression, type, min, max, startLevel, stepLevel, false,
                                                      StatType.Unknown, null, classes, null, customProgression: customProgression);
            }
            else
            {
                return Helpers.CreateContextRankConfig(ContextRankBaseValueType.SummClassLevelWithArchetype, progression, type, min, max, startLevel, stepLevel, false,
                                      StatType.Unknown, null, classes, archetype, customProgression: customProgression);
            }
        }


        NewMechanics.ContextCalculateAbilityParamsBasedOnClasses createDCScaling()
        {
            if (archetype == null)
            {
                return Common.createContextCalculateAbilityParamsBasedOnClasses(classes, stat);
            }
            else
            {
                return Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(classes, getArchetypeArray(), stat);
            }
        }


        void addMinLevelPrerequisite(BlueprintFeature feature, int lvl)
        {
            foreach (var c in classes)
            {
                if (archetype != null && archetype.GetParentClass() == c)
                {
                    feature.AddComponents(Common.createPrerequisiteArchetypeLevel(c, archetype, lvl, any: true));
                }
                else
                {
                    feature.AddComponents(Helpers.PrerequisiteClassLevel(c, lvl, any: true));
                }
            }
        }


        LevelUpMechanics.AddFeatureOnClassLevelRange createAddFeatureInLevelRange(BlueprintFeature feature, int min_lvl, int max_lvl)
        {
            return Helpers.Create<LevelUpMechanics.AddFeatureOnClassLevelRange>(a =>
            {
                a.Feature = feature;
                if (archetype != null)
                {
                    a.archetypes = new BlueprintArchetype[] { archetype };
                }
                a.classes = classes;
                a.min_level = min_lvl;
                a.max_level = max_lvl;
            });
        }
    }
}
