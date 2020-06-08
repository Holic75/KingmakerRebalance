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
    public partial class MysteryEngine
    {
        static LibraryScriptableObject library => Main.library;
        BlueprintCharacterClass[] classes;
        BlueprintArchetype archetype;
        StatType stat;

        public MysteryEngine(BlueprintCharacterClass[] mystery_classes, StatType scaling_stat, BlueprintArchetype mystery_archetype = null)
        {
            stat = scaling_stat;
            classes = mystery_classes;
            archetype = mystery_archetype;
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
                                                      StatType.Unknown, null, classes, null);
            }
            else
            {
                return Helpers.CreateContextRankConfig(ContextRankBaseValueType.SummClassLevelWithArchetype, progression, type, min, max, startLevel, stepLevel, false,
                                      StatType.Unknown, null, classes, archetype);
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
    }
}
