using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    class VersatilePerformance
    {
        static LibraryScriptableObject library => Main.library;
        static List<BlueprintFeature> versatile_performances = new List<BlueprintFeature>();
        static BlueprintFeatureSelection martial_performance;
        
        static internal void create()
        {
            createMartialPerformance();
            createExpandedVersality();
            var versatile_perfrormance = library.Get<BlueprintFeatureSelection>("94e2cd84bf3a8e04f8609fe502892f4f");
            versatile_perfrormance.AllFeatures = versatile_performances.ToArray().AddToArray(martial_performance);
            versatile_perfrormance.SetNameDescription("Versatile Performance",
                                                      versatile_performances[0].Description);

            Skald.versatile_performance.SetNameDescription(versatile_perfrormance.Name, versatile_perfrormance.Description);
            Skald.versatile_performance.AllFeatures = versatile_perfrormance.AllFeatures;

            fixArchaelogist();
        }


        static void fixArchaelogist()
        {
            var versatile_perfrormance = library.Get<BlueprintFeatureSelection>("94e2cd84bf3a8e04f8609fe502892f4f");
            var archaelogist = library.Get<BlueprintArchetype>("38384e0c1e99c2e42ac6ed70a04aca46");
            List<int> remove_vt_levels = new int[] { 2, 6, 10, 14, 18 }.ToList();
            List<int> add_rogue_talent = new int[] { 4, 8, 12, 16, 20 }.ToList();

            var rogue_talent = library.CopyAndAdd<BlueprintFeatureSelection>("c074a5d615200494b8f2a9c845799d93", "ArchaelogistRogueTalent", "");
            rogue_talent.SetDescription("At 4th level, an archaeologist gains a rogue talent. He gains an additional rogue talent for every four levels of archaeologist gained after 4th level. Otherwise, this works as the rogue’s rogue talent ability.");


            foreach (var le in archaelogist.RemoveFeatures)
            {
                if (remove_vt_levels.Contains(le.Level))
                {
                    remove_vt_levels.Remove(le.Level);
                    le.Features.Add(versatile_perfrormance);
                }
            }

            foreach (var l in remove_vt_levels)
            {
                archaelogist.RemoveFeatures = archaelogist.RemoveFeatures.AddToArray(Helpers.LevelEntry(l, versatile_perfrormance));
            }

            foreach (var le in archaelogist.AddFeatures)
            {
                if (add_rogue_talent.Contains(le.Level))
                {
                    add_rogue_talent.Remove(le.Level);
                    le.Features.Add(rogue_talent);
                }
            }


            foreach (var l in add_rogue_talent)
            {
                archaelogist.AddFeatures = archaelogist.AddFeatures.AddToArray(Helpers.LevelEntry(l, rogue_talent));
            }
        }


        static void createMartialPerformance()
        {
            var bard_class = library.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");
            var fighter = library.Get<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");
            martial_performance = Helpers.CreateFeatureSelection("MartialPerformanceFeatureSelection",
                                                    "Martial Performance",
                                                    "The bard or skald selects a weapon she is proficient with. She receives a weapon focus with associated weapon category and treats her bard or skald level as half fighter level.",
                                                    "",
                                                    library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e").Icon,
                                                    FeatureGroup.None,
                                                    Common.createClassLevelsForPrerequisites(fighter, bard_class, 0.5),
                                                    Common.createClassLevelsForPrerequisites(fighter, Skald.skald_class, 0.5),
                                                    Helpers.PrerequisiteClassLevel(bard_class, 6, any: true),
                                                    Helpers.PrerequisiteClassLevel(Skald.skald_class, 6, any: true)
                                                    );



            foreach (var category in Enum.GetValues(typeof(WeaponCategory)).Cast<WeaponCategory>())
            {
                var feature = Helpers.CreateFeature(category.ToString() + "MartialPerformanceFeature",
                                                    martial_performance.Name + $" ({LocalizedTexts.Instance.Stats.GetText(category)})",
                                                    martial_performance.Description,
                                                    "",
                                                    martial_performance.Icon,
                                                    FeatureGroup.None,
                                                    Common.createAddParametrizedFeatures(library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e"), category),
                                                    Helpers.Create<PrerequisiteProficiency>(p => p.WeaponProficiencies = new WeaponCategory[] { category })
                                                    );
                martial_performance.AllFeatures = martial_performance.AllFeatures.AddToArray(feature);
            }       
        }

        static void createExpandedVersality()
        {
            var bard_class = library.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");

            var skill_foci = library.Get<BlueprintFeatureSelection>("c9629ef9eebb88b479b2fbc5e836656a").AllFeatures;
            var skills = new StatType[] { StatType.SkillMobility, StatType.SkillStealth, StatType.SkillPersuasion, StatType.SkillPerception, StatType.SkillUseMagicDevice };

            for (int i = 0; i < skill_foci.Length; i++)
            {
                StatType stat = skill_foci[i].GetComponent<AddContextStatBonus>().Stat;
                if (!skills.Contains(stat))
                {
                    continue;
                }

                string name = LocalizedTexts.Instance.Stats.GetText(stat);

                var feature = Helpers.CreateFeature(stat.ToString() + "VersatilePerformanceFeature",
                                                   "Versatile Performance: " + name,
                                                   "At 2nd level a bard or skald selects one of the following skills: Mobility, Stealth, Perception, Persuation or Use Magic Device. She receives a number of ranks in this skill equal to half her bard or skald level. In addition she uses charisma modifier in place of the ability modifier the skill would normally use. The bard can immediately retrain all of her ranks in the selected skill at no additional cost in money or time.",
                                                   "",
                                                   skill_foci[i].Icon,
                                                   FeatureGroup.None,
                                                   Helpers.Create<SkillMechanics.SetSkillRankToValue>(ss => { ss.skill = stat; ss.value = Helpers.CreateContextValue(AbilityRankType.Default); }),
                                                   Helpers.Create<StatReplacementMechanics.ReplaceBaseStatForStatTypeLogic>(r => { r.only_if_greater = false; r.NewBaseStatType = StatType.Charisma; r.StatTypeToReplaceBastStatFor = stat; }),
                                                   Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,progression: ContextRankProgression.OnePlusDiv2,
                                                                                   classes: new BlueprintCharacterClass[] {bard_class, Skald.skald_class}
                                                                                   )
                                                   );
                versatile_performances.Add(feature);
            }
        }
    }
}
