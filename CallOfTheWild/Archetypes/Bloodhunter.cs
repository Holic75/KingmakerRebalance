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
    public class Bloodhunter
    {
        static public BlueprintArchetype archetype;

        static public BlueprintFeature favored_target;
        static public BlueprintFeature hunting_ground;
        static public BlueprintFeature rapid_quarry;
        static public BlueprintFeature rapid_quarry19;
        static public BlueprintFeature blessed_hunter;
        static public BlueprintFeature blessed_hunter_stride;
        static public BlueprintFeature blessed_hunters_focus;

        static LibraryScriptableObject library => Main.library;

        static public void create()
        {
            var ranger_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("cda0615668a6df14eb36ba19ee881af6");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "BloodHunterArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Blood Hunter");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "The blood hunters are expert warriors who serve as the ultimate bounty hunters of the land. Their skills are unrivaled, and their uncanny prowess at tracking and hunting borders on the supernatural.");
            });
            Helpers.SetField(archetype, "m_ParentClass", ranger_class);
            library.AddAsset(archetype, "");

            var favored_enemy = library.Get<BlueprintFeatureSelection>("16cc2c937ea8d714193017780e7d4fc6");
            var favored_enemy_rank_up = library.Get<BlueprintFeatureSelection>("c1be13839472aad46b152cf10cf46179");
            var favored_terrain = library.Get<BlueprintFeatureSelection>("a6ea422d7308c0d428a541562faedefd");
            var favored_terrain_rank_up = library.Get<BlueprintFeatureSelection>("efa888832eae4e169f8ae285b0777b43");
            var quarry = library.Get<BlueprintFeature>("385260ca07d5f1b4e907ba22a02944fc");
            var improved_quarry = library.Get<BlueprintFeature>("25e009b7e53f86141adee3a1213af5af");
            var woodland_stride = library.Get<BlueprintFeature>("11f4072ea766a5840a46e6660894527d");

            archetype.RemoveSpellbook = true;
            archetype.ChangeCasterType = true;
            createFavoredTarget();
            createHuntingGround();
            createBlessedHunterFeats();
            createQuarry();

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, favored_enemy),
                                                          Helpers.LevelEntry(3, favored_terrain),
                                                          Helpers.LevelEntry(5, favored_enemy, favored_enemy_rank_up),
                                                          Helpers.LevelEntry(7, woodland_stride),
                                                          Helpers.LevelEntry(8, favored_terrain, favored_terrain_rank_up),
                                                          Helpers.LevelEntry(10, favored_enemy, favored_enemy_rank_up),
                                                          Helpers.LevelEntry(11, quarry),
                                                          Helpers.LevelEntry(13, favored_terrain, favored_terrain_rank_up),
                                                          Helpers.LevelEntry(15, favored_enemy, favored_enemy_rank_up),
                                                          Helpers.LevelEntry(18, favored_terrain, favored_terrain_rank_up),
                                                          Helpers.LevelEntry(19, improved_quarry),
                                                          Helpers.LevelEntry(20, favored_enemy, favored_enemy_rank_up)
                                                          };

            archetype.AddFeatures = new LevelEntry[] {    Helpers.LevelEntry(1, favored_target),
                                                          Helpers.LevelEntry(3, hunting_ground),
                                                          Helpers.LevelEntry(5, favored_target),
                                                          Helpers.LevelEntry(7, blessed_hunter),
                                                          Helpers.LevelEntry(8, hunting_ground),
                                                          Helpers.LevelEntry(10, favored_target, blessed_hunter_stride),
                                                          Helpers.LevelEntry(11, rapid_quarry),
                                                          Helpers.LevelEntry(13, hunting_ground, blessed_hunters_focus),
                                                          Helpers.LevelEntry(15, favored_target),
                                                          Helpers.LevelEntry(18, hunting_ground),
                                                          Helpers.LevelEntry(19, rapid_quarry19),
                                                          Helpers.LevelEntry(20, favored_target)
                                                       };

            ranger_class.Progression.UIGroups = ranger_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(blessed_hunter, blessed_hunters_focus, blessed_hunter_stride));
            ranger_class.Progression.UIGroups = ranger_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(rapid_quarry, rapid_quarry19));
            ranger_class.Archetypes = ranger_class.Archetypes.AddToArray(archetype);


            var ranger_mt = library.Get<BlueprintProgression>("a823e7aa48bbec24f87f4a92a3ac0aa2");
            ranger_mt.AddComponent(Common.prerequisiteNoArchetype(archetype));
        }


        static void createFavoredTarget()
        {
            var favored_enemy = library.Get<BlueprintFeatureSelection>("16cc2c937ea8d714193017780e7d4fc6");
            favored_target = Helpers.CreateFeature("FavoredTargetBloodHunterFeature",
                                                    "Favored Target",
                                                    "At 1st level, a blood hunter gains +1 bonus on weapon attack and damage rolls against every creature type from ranger favored enemies list. At 5th, 10th, 15th, and 20th levels, these bonuses increase by 1.\n"
                                                    + "Favored target is modified by any feat, spell, or effect that specifically works with the favored enemy ranger class feature. Additionally, the favored target ability always grants or uses a minimum of the blood hunter’s full favored target bonus for the purposes of effects that use the value of the ranger’s favored enemy bonus. For example, a blood hunter grants his full favored target bonus to his animal companion when using the hunter’s bond class feature.",
                                                    "",
                                                    Helpers.GetIcon("2c38da66e5a599347ac95b3294acbe00"), //true strike
                                                    FeatureGroup.None
                                                    );
            favored_target.Ranks = 10;
            favored_target.ReapplyOnLevelUp = true;
            foreach (var fe in favored_enemy.AllFeatures)
            {
                favored_target.AddComponent(Helpers.Create<FavoredEnemyMechanics.AddHalfFavoredEnemy>(a => a.CheckedFacts = fe.GetComponent<AddFavoredEnemy>().CheckedFacts));
            }
            var hunters_bond_buff = library.Get<BlueprintBuff>("2f93cad6b132aac4e80728d7fa03a8aa");
            var hunters_bond_full_buff = library.CopyAndAdd<BlueprintBuff>("2f93cad6b132aac4e80728d7fa03a8aa", "BloodHunterHuntersBond", "");
            hunters_bond_full_buff.ReplaceComponent<ShareFavoredEnemies>(s => s.Half = true);

            var hunters_bond_ability = library.Get<BlueprintAbility>("cd80ea8a7a07a9d4cb1a54e67a9390a5");

            var apply_buff = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(favored_target),
                                                       Common.createContextActionApplyBuff(hunters_bond_full_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false),
                                                       Common.createContextActionApplyBuff(hunters_bond_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false)
                                                       );
            hunters_bond_ability.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(apply_buff));
        }


        static void createHuntingGround()
        {
            var favored_terrain = library.Get<BlueprintFeatureSelection>("a6ea422d7308c0d428a541562faedefd");
            hunting_ground = Helpers.CreateFeature("HintingGroundBloodHunterFeature",
                                                   "Hunting Ground",
                                                   "Blood Hunter treats all terrains from ranger favored terrain list as his favored terrains. The blood hunter gains a +2 bonus on initiative checks and Lore (Nature), Perception, and Stealth skill checks when he is in his favored terrain.  The bonuses increase by 2 at 8th level and every 5 levels thereafter.",
                                                   "",
                                                   LoadIcons.Image2Sprite.Create(@"AbilityIcons/HuntingGround.png"),
                                                   FeatureGroup.None,
                                                   Helpers.Create<FavoredTerrain>(f => f.Settings = new LootSetting[0])
                                                   );
            hunting_ground.ReapplyOnLevelUp = true;
            hunting_ground.Ranks = 10;
            foreach (var ft in favored_terrain.AllFeatures)
            {
                var comp = hunting_ground.GetComponent<FavoredTerrain>();
                comp.Settings = comp.Settings.AddToArray(ft.GetComponent<FavoredTerrain>().Settings);


            }
        }


        static void createBlessedHunterFeats()
        {
            blessed_hunter = Helpers.CreateFeature("BlessedHunterBloodHunterFeat",
                                                   "Blessed Hunter",
                                                   "While in his favored terrain, blood hunter gains +1 bonus on damage rolls and saving throws.",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Helpers.Create<FavoredEnemyMechanics.BlessedHunterTerrain>(b => b.Settings = hunting_ground.GetComponent<FavoredTerrain>().Settings)
                                                   );


            blessed_hunter_stride = library.CopyAndAdd<BlueprintFeature>("11f4072ea766a5840a46e6660894527d", "BlessedHunterStrideFeature", "");
            blessed_hunter_stride.SetNameDescription("Blessed Hunter's Stride",
                                                     "Blood hunter's speed increases by 10 feet while he is in his favored terrain, and he ignores difficult terrain effects.");
            blessed_hunter_stride.AddComponent(Helpers.CreateAddStatBonus(StatType.Speed, 10, ModifierDescriptor.UntypedStackable));
            var animal_focus_engine = new AnimalFocusEngine();
            animal_focus_engine.initialize(new BlueprintCharacterClass[] { archetype.GetParentClass() }, archetype, 0, "BloodHunter");

            var animal_focus = animal_focus_engine.createAnimalFocus("At 13th level, while in his favored terrain, blood hunter gains the benefits of the hunter’s animal focus class feature, with an effective hunter level equal to his blood hunter level. If blood hunter formed a bond with animal companion, he can also apply animal focus to it.");
            animal_focus.HideInCharacterSheetAndLevelUp = true;

            var apply_focus_ability = animal_focus_engine.createApplyAnimalFocusAbility(animal_focus.name, "Apply Animal Focus (Permanent)", "This abilitiy applies selected animal foci.", animal_focus.Icon);
            animal_focus.AddComponent(Helpers.CreateAddFact(apply_focus_ability));


            blessed_hunters_focus = Helpers.CreateFeature("BlessedHunterFocusBloodHunterFeature",
                                                          "Blessed Hunter's Focus",
                                                          "At 13th level, while in his favored terrain, blood hunter gains the benefits of the hunter’s animal focus class feature, with an effective hunter level equal to his blood hunter level. The chosen aspect remains active until changed. If blood hunter formed a bond with animal companion, he can also apply animal focus to it. The blood hunter can apply extra animal focus if his animal companion is dead.",
                                                          "",
                                                          animal_focus.Icon,
                                                          FeatureGroup.None,
                                                          Helpers.CreateAddFact(animal_focus),
                                                          Common.createAddFeatToAnimalCompanion(animal_focus)
                                                          );

            var ranger_ac_selection = library.Get<BlueprintFeatureSelection>("ee63330662126374e8785cc901941ac7");
            var planar_focus = animal_focus_engine.createPlanarFocus("Blood Hunter", ranger_ac_selection);
            planar_focus.AddComponents(Common.createPrerequisiteArchetypeLevel(archetype.GetParentClass(), archetype, 13));
        }


        static void createQuarry()
        {
            var ability = library.CopyAndAdd<BlueprintAbility>("e93dfca6f025e6d4e9583e688c147aca", "BloodHunterQuarryAbility", "");
            ability.ActionType = CommandType.Swift;
            ability.SetName("Rapid Quarry");
            rapid_quarry19 = Helpers.CreateFeature("BloodHunterQuarry19Feature",
                                              "Rapid Quarry",
                                              "At 11th level, a blood hunter can denote any target as quarry as a swift action. Once his quarry dies, he can select a new quarry after 10 minutes have passed.\n"
                                              + "At 19th level, if his quarry is killed or dismissed, he can select a new one after 1 minute has passed.",
                                              "",
                                              ability.Icon,
                                              FeatureGroup.None
                                              );

            var cooldown_buff = Rebalance.quarry_cooldown_buff;

            var apply_cooldown = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(rapid_quarry19), 
                                                           Common.createContextActionApplyBuff(cooldown_buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false),
                                                           Common.createContextActionApplyBuff(cooldown_buff, Helpers.CreateContextDuration(1, DurationRate.TenMinutes), dispellable: false)
                                                          );

            ability.ReplaceComponent(ability.GetComponent<AbilityExecuteActionOnCast>(), 
                                     Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionOnContextCaster(apply_cooldown))));
            //ability.AddComponent(Common.createAbilityCasterHasNoFacts(cooldown_buff)); //already added in rebalance

            //ability.RemoveComponents<AbilityTargetIsPartyMember>();

            var quarry_buff = library.Get<BlueprintBuff>("b44184c7ca33c6a41bc11cc5ed07addb");
            quarry_buff.SetBuffFlags(quarry_buff.GetBuffFlags() | BuffFlags.RemoveOnRest);


            rapid_quarry = Helpers.CreateFeature("BloodHunterQuarryFeature",
                                  rapid_quarry19.Name,
                                  rapid_quarry19.Description,
                                  "",
                                  ability.Icon,
                                  FeatureGroup.None,
                                  Helpers.CreateAddFact(ability)
                                  );
        }
    }
}
