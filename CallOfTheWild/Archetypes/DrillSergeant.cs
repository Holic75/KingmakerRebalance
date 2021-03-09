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
    public class DrillSergeant
    {
        static public BlueprintArchetype archetype;

        static public BlueprintFeatureSelection tactician;
        static public BlueprintFeatureSelection greater_tactician;
        static public BlueprintFeatureSelection master_tactician;
        static public BlueprintFeature weapon_trainer;

        static HashSet<BlueprintFeature> processed_teamwork_feats = new HashSet<BlueprintFeature>();

        static BlueprintCharacterClass fighter_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");

        static LibraryScriptableObject library => Main.library;

        static public void create()
        {
            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "DrillSergeantArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Drill Sergeant");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Drill sergeants excel at training other combatants in fighting techniques.");
            });
            Helpers.SetField(archetype, "m_ParentClass", fighter_class);
            library.AddAsset(archetype, "");

            createTactician();
            createWeaponTrainer();

            var bravery = library.Get<BlueprintFeature>("f6388946f9f472f4585591b80e9f2452");
            var weapon_training_extra = library.Get<BlueprintFeature>("b8cecf4e5e464ad41b79d5b42b76b399");

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(2, bravery),
                                                          Helpers.LevelEntry(6, bravery),
                                                          Helpers.LevelEntry(9, weapon_training_extra),
                                                          Helpers.LevelEntry(10, bravery),
                                                          Helpers.LevelEntry(13, weapon_training_extra),
                                                          Helpers.LevelEntry(14, bravery),
                                                          Helpers.LevelEntry(17, weapon_training_extra),
                                                          Helpers.LevelEntry(18, bravery)
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(2, tactician),
                                                          Helpers.LevelEntry(9, greater_tactician),
                                                          Helpers.LevelEntry(13, weapon_trainer),
                                                          Helpers.LevelEntry(17, master_tactician)
                                                       };
    
            fighter_class.Progression.UIGroups = fighter_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(tactician, greater_tactician, weapon_trainer, master_tactician));
            fighter_class.Archetypes = fighter_class.Archetypes.AddToArray(archetype);
        }


        static void createTactician()
        {
            var resource = Helpers.CreateAbilityResource("DrillSergeantTacticianResource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 5, 1, 5, 1, 0, 0.0f, new BlueprintCharacterClass[] { fighter_class });
            var ability = library.CopyAndAdd<BlueprintAbility>("f1c8ec6179505714083ed9bd47599268", "DrillSergeantTactician", "");
            ability.SetNameDescription("Tactician",
                                       "At 2nd level, a drill sergeant receives a teamwork feat as a bonus feat. He must meet the prerequisites for this feat. As a standard action, the drill sergeant can grant any teamwork feat he has to all allies within 30 feet who can see and hear him. Allies retain the use of this bonus feat for 3 rounds plus 1 round for every two levels the drill sergeant possesses. Allies do not need to meet the prerequisites of these bonus feats. The drill sergeant can use this ability once per day at 1st level, plus one additional time per day at 5th level and for every 5 levels thereafter.");

            ability.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", new BlueprintCharacterClass[] {fighter_class }));
            ability.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = resource);

            tactician = Helpers.CreateFeatureSelection("DrillSergeantTacticianFeatureSelection",
                                                       ability.Name,
                                                       ability.Description,
                                                       "",
                                                       ability.Icon,
                                                       FeatureGroup.None,
                                                       Helpers.CreateAddFact(ability),
                                                       resource.CreateAddAbilityResource()
                                                       );
            var teamwork_feats = library.Get<BlueprintBuff>("a603a90d24a636c41910b3868f434447").GetComponent<TeamworkMechanics.AddFactsFromCasterIfHasBuffs>().facts.Cast<BlueprintFeature>().ToArray();

            foreach (var tf in teamwork_feats)
            {
                var add_comp = tf.GetComponent<AddFeatureIfHasFact>().CreateCopy(a => a.CheckedFact = tactician);
                tf.AddComponent(add_comp);
            }

            tactician.AllFeatures = library.Get<BlueprintFeatureSelection>("d87e2f6a9278ac04caeb0f93eff95fcb").AllFeatures;

            var comp = library.Get<BlueprintFeature>("4ca47c023f1c158428bd55deb44c735f").GetComponent<AutoMetamagic>().CreateCopy(a => a.Abilities = new BlueprintAbility[] { ability }.ToList()) ;
            greater_tactician = Helpers.CreateFeatureSelection("DrillSergeantGreaterTacticianFeatureSelection",
                                           "Greater Tactician",
                                           "At 9th level, the drill sergeant receives an additional teamwork feat as a bonus feat. He must meet the prerequisites for this feat. Using the tactician ability is a swift action.",
                                           "",
                                           ability.Icon,
                                           FeatureGroup.None,
                                           comp
                                           );
            greater_tactician.AllFeatures = tactician.AllFeatures;


            master_tactician = Helpers.CreateFeatureSelection("DrillSergeantMasterTacticianFeatureSelection",
                                                               "Master Tactician",
                                                               "At 17th level, the drill sergeant receives an additional teamwork feat as a bonus feat. He must meet the prerequisites for this feat. The drill sergeant can grant this feat to his allies using the tactician ability. Whenever the drill sergeant uses the tactician ability, he grants any two teamwork feats that he knows.",
                                                               "",
                                                               ability.Icon,
                                                               FeatureGroup.None,
                                                               Common.createIncreaseActivatableAbilityGroupSize(ActivatableAbilityGroupExtension.TacticianTeamworkFeatShare.ToActivatableAbilityGroup())
                                                               );
            master_tactician.AllFeatures = tactician.AllFeatures;
        }

        static void createWeaponTrainer()
        {
            var buff = Helpers.CreateBuff("WeaponTrainerEffectBuff",
                                          "Weapon Trainer",
                                          "At 13th level, when a drill sergeant wields a weapon he has weapon training in, all allies within 30 feet who can see and hear the drill sergeant gain half his weapon training bonus when they wield any weapon from the same weapon group.\n"
                                          + "This bonus doesn’t stack with any weapon training bonus an ally already possesses.",
                                          "",
                                          Helpers.GetIcon("9d5d2d3ffdd73c648af3eb3e585b1113"),
                                          null,
                                          Helpers.Create<WeaponTrainingMechanics.ShareWeaponGroupAttackDamageBonus>());

            weapon_trainer = Helpers.CreateFeature("WeaponTrainerFeature",
                                                   buff.Name,
                                                   buff.Description,
                                                   "",
                                                   buff.Icon,
                                                   FeatureGroup.None,
                                                   Common.createAuraEffectFeatureComponentCustom(buff,
                                                                                                 30.Feet(),
                                                                                                  Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsAlly>(),
                                                                                                                                     Common.createContextConditionIsCaster(not: true)))
                                                   );

        }
    }
}
