using System;
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
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild.Archetypes
{
    public class PackRager
    {
        static public BlueprintArchetype archetype;

        static public BlueprintFeatureSelection teamwork_feat;
        static public BlueprintFeature[] raging_tactician;
        static public BlueprintFeature raging_tactician_increased_range;

        static public BlueprintFeatureSelection master_tactician;
        static public BlueprintFeature weapon_trainer;

        static BlueprintCharacterClass barbarian_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f7d7eb166b3dd594fb330d085df41853");

        static LibraryScriptableObject library => Main.library;
        static BlueprintBuff raging_tactician_buff;

        static public void create()
        {
            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "PackRagerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Pack Rager");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Barbarian rages can be a thing of savage beauty, exhibiting a lethal grace. While such uncontrolled displays of carnage often disregard group tactics, there are those barbarians whose rages inspire and spur on their allies during the ferocious dance of death.");
            });
            Helpers.SetField(archetype, "m_ParentClass", barbarian_class);
            library.AddAsset(archetype, "");

            createTeamworkFeat();
            createRagingTactician();

            var rage_power = library.Get<BlueprintFeatureSelection>("28710502f46848d48b3f0d6132817c4e");
            var damage_reduction = library.Get<BlueprintFeature>("cffb5cddefab30140ac133699d52a8f8");

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(2, rage_power),
                                                          Helpers.LevelEntry(6, rage_power),
                                                          Helpers.LevelEntry(7, damage_reduction),
                                                          Helpers.LevelEntry(10, rage_power, damage_reduction),
                                                          Helpers.LevelEntry(13, damage_reduction),
                                                          Helpers.LevelEntry(14, rage_power),
                                                          Helpers.LevelEntry(16, damage_reduction),
                                                          Helpers.LevelEntry(18, rage_power),
                                                          Helpers.LevelEntry(19, damage_reduction),
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(2, teamwork_feat),
                                                       Helpers.LevelEntry(6, teamwork_feat),
                                                       Helpers.LevelEntry(7, raging_tactician[0]),
                                                       Helpers.LevelEntry(10, teamwork_feat),
                                                       Helpers.LevelEntry(13, raging_tactician[1]),
                                                       Helpers.LevelEntry(14, teamwork_feat),
                                                       Helpers.LevelEntry(16, raging_tactician_increased_range),
                                                       Helpers.LevelEntry(18, teamwork_feat),
                                                       Helpers.LevelEntry(19, raging_tactician[2])
                                                     };

            barbarian_class.Progression.UIGroups = barbarian_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(teamwork_feat));
            barbarian_class.Progression.UIGroups = barbarian_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(raging_tactician.AddToArray(raging_tactician_increased_range)));
            barbarian_class.Archetypes = barbarian_class.Archetypes.AddToArray(archetype);
        }



        static void createRagingTactician()
        {
            raging_tactician = new BlueprintFeature[3];
            raging_tactician[0] = Helpers.CreateFeature("RagingTactician1Feature",
                                                        "Raging Tactician",
                                                        "At 7th level, while a pack rager is raging, she grants a single teamwork feat she knows to all allies within 30 feet who can see and hear her. She chooses the feat at the start of the rage, and cannot change it during the rage. An ally who moves more than 30 feet away from the raging pack rager loses access to the feat, but regains it again each time he moves back within 30 feet of the raging pack rager. Allies do not need to meet the prerequisites of these teamwork feats.\n"
                                                        + "At 13th level, the pack rager chooses two teamwork feats when she enters a rage, and grants her allies the use of both of those feats when they are within 30 feet of her.\n"
                                                        + "At 16th level, the pack rager grants these teamwork feats as long as her allies are within 60 feet of her.\n"
                                                        + "At 19th level, the pack rager chooses three teamwork feats when she enters a rage, and grants her allies the use of all three feats when they are within 60 feet of her.",
                                                        "",
                                                        Helpers.GetIcon("93e78cad499b1b54c859a970cbe4f585"),
                                                        FeatureGroup.None
                                                        );

            raging_tactician_increased_range = library.CopyAndAdd(raging_tactician[0], "RagingTacticianIncreasedRangeFeature", "");

            raging_tactician_buff = Helpers.CreateBuff("RagingTacticianEffectBuff",
                                                           raging_tactician[0].Name,
                                                           raging_tactician[0].Description,
                                                           "",
                                                           raging_tactician[0].Icon,
                                                           null,
                                                           Helpers.Create<TeamworkMechanics.AddFactsFromCasterIfHasBuffs>()
                                                           );
                                                           

            foreach (var tw in teamwork_feat.AllFeatures)
            {
                addToRagingTactician(tw);
            }

            var rage_buff = Common.createBuffAreaEffect(raging_tactician_buff, 
                                                        30.Feet(), 
                                                        Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsAlly>(), Common.createContextConditionIsCaster(not: true))
                                                        );

            var rage_buff2 = Common.createBuffAreaEffect(raging_tactician_buff,
                                            60.Feet(),
                                            Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsAlly>(), Common.createContextConditionIsCaster(not: true)),
                                            "AreaIncreasedRange");

            Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuffNoRemove(NewRagePowers.rage_buff,
                                                                                      Helpers.CreateConditional(Common.createContextConditionCasterHasFact(raging_tactician_increased_range),
                                                                                                                Common.createContextActionApplyChildBuff(rage_buff2),
                                                                                                                Helpers.CreateConditional(Common.createContextConditionHasFact(raging_tactician[0]),
                                                                                                                                          Common.createContextActionApplyChildBuff(rage_buff2)
                                                                                                                                          )
                                                                                                                )
                                                                                      );

            for (int i = 1; i <= 2; i++)
            {
                raging_tactician[i] = library.CopyAndAdd(raging_tactician[0], $"RagingTactician{i + 1}Feature", "");
                raging_tactician[i].AddComponent(Common.createIncreaseActivatableAbilityGroupSize(ActivatableAbilityGroupExtension.RagingTacticianTeamworkFeatShare.ToActivatableAbilityGroup()));
            }
        }


        static public void addToRagingTactician(BlueprintFeature tw)
        {
            var choice_buff = Helpers.CreateBuff(tw.name + "RagingTacticianShareBuff",
                                                  "Raging Tactician: " + tw.Name,
                                                  "You can grant this teamwork feat to all allies within 30 feet who can see and hear you, using your tactician ability.",
                                                  Helpers.MergeIds("52674e84501b449c811bfcf8ce3d5a48", tw.AssetGuid),
                                                  tw.Icon,
                                                  null
                                                  );
            var toggle = Helpers.CreateActivatableAbility(tw.name + "RagingTacticianShareToggleAbility",
                                                          choice_buff.Name,
                                                          choice_buff.Description,
                                                          Helpers.MergeIds("311f55033cea46af96129d053e1e9c36", tw.AssetGuid),
                                                          tw.Icon,
                                                          choice_buff,
                                                          AbilityActivationType.Immediately,
                                                          UnitCommand.CommandType.Free,
                                                          null
                                                          );
            toggle.DeactivateImmediately = true;
            toggle.Group = ActivatableAbilityGroupExtension.RagingTacticianTeamworkFeatShare.ToActivatableAbilityGroup();
            toggle.WeightInGroup = 1;

            var feature = Common.ActivatableAbilityToFeature(toggle, true, Helpers.MergeIds("949676fc1d3c482c84289c1920b5923c", tw.AssetGuid));
            raging_tactician[0].AddComponent(Helpers.Create<NewMechanics.AddFeatureIfHasFactsFromList>(a => { a.Feature = feature; a.CheckedFacts = new Kingmaker.Blueprints.Facts.BlueprintUnitFact[] { tw }; }));
            tw.AddComponent(Helpers.Create<NewMechanics.AddFeatureIfHasFactsFromList>(a => { a.Feature = feature; a.CheckedFacts = new Kingmaker.Blueprints.Facts.BlueprintUnitFact[] { raging_tactician[0] }; }));

            raging_tactician_buff.GetComponent<TeamworkMechanics.AddFactsFromCasterIfHasBuffs>().facts.Add(tw);
            raging_tactician_buff.GetComponent<TeamworkMechanics.AddFactsFromCasterIfHasBuffs>().prerequsites.Add(choice_buff);
        }



        static void createTeamworkFeat()
        {
            teamwork_feat = Helpers.CreateFeatureSelection("RagingTacticianFeatureSelection",
                                                           "Raging Tactician",
                                                           "At 2nd level and every 4 levels thereafter, the pack rager can take a bonus teamwork feat. This teamwork feat must also be a combat feat.",
                                                           "",
                                                           null,
                                                           FeatureGroup.None
                                                           );

            teamwork_feat.AllFeatures = library.Get<BlueprintFeatureSelection>("d87e2f6a9278ac04caeb0f93eff95fcb").AllFeatures.Where(f => f.Groups.Contains(FeatureGroup.CombatFeat)).ToArray();
        }
    }
}

