using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
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
        bool check_invested_focus;
        public List<BlueprintFeature> mastery_features = new List<BlueprintFeature>();

        public ImplementsEngine(string name_prefix, BlueprintAbilityResource ability_resource, BlueprintCharacterClass[] scaling_classes, StatType scaling_stat, bool check_focus_investment = true, BlueprintArchetype scaling_archetype = null)
        {
            stat = scaling_stat;
            classes = scaling_classes;
            archetype = scaling_archetype;
            resource = ability_resource;
            prefix = name_prefix + "Implement";
            check_invested_focus = check_focus_investment;
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
                if (c.GetComponent<FakeClassLevelMechanics.FakeClass>() != null)
                {
                    continue;
                }
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


        void addFocusInvestmentCheck(BlueprintAbility ability, params SpellSchool[] schools)
        {
            if (!check_invested_focus)
            {
                return; 
            }
            var abilities = ability.HasVariants ? ability.Variants : new BlueprintAbility[] {ability};
            foreach (var a in abilities)
            {
                foreach (var s in schools)
                {
                    a.AddComponent(Helpers.Create<ImplementMechanics.AbilityCasterInvestedFocus>(ab => ab.school = s));
                }
            }
        }


        void addFocusInvestmentCheck(BlueprintActivatableAbility ability, params SpellSchool[] schools)
        {
            if (!check_invested_focus)
            {
                return;
            }
            foreach (var s in schools)
            {
                ability.AddComponent(Helpers.Create<ImplementMechanics.RestrictionInvestedFocus>(r => r.school = s));
            }
        }
    }
}
