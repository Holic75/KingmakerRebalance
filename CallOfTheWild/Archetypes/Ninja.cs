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
    public class Ninja
    {
        static public BlueprintArchetype archetype;

        static public BlueprintFeature ninja_proficiencies;
        static public BlueprintFeature ki_pool;
        static public BlueprintAbilityResource ki_resource;
        static public BlueprintFeature no_trace;
        static public BlueprintFeature light_steps;

        static public BlueprintFeatureSelection ninja_trick;
        //base tricks
        static public BlueprintFeature acceleration_of_form;//
        static public BlueprintFeature chocking_bomb;//
        static public BlueprintFeature herbal_compound;
        static public BlueprintFeature kamikaze;
        //static public BlueprintFeature pressure_points; ~ similar to cripling strike
        static public BlueprintFeature smoke_bomb;//
        static public BlueprintFeatureSelection style_master;//
        static public BlueprintFeature unarmed_combat_training;//
        static public BlueprintFeature shadow_clone;//
        static public BlueprintFeature vanishing_trick;//
        //+all rogue talents
        //master tricks
        static public BlueprintFeature unarmed_combat_mastery;
        static public BlueprintFeature blinding_bomb;//
        static public BlueprintFeature invisible_blade;//
        static public BlueprintFeature see_the_unseen;//
        static public BlueprintFeature flurry_of_darts;
        //+evasion


        static LibraryScriptableObject library => Main.library;


        static BlueprintCharacterClass[] getRogueArray()
        {
            var rogue_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("299aa766dee3cbf4790da4efb8c72484");
            return new BlueprintCharacterClass[] {rogue_class };
        }

        static public void create()
        {
            var rogue_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("299aa766dee3cbf4790da4efb8c72484");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "NinjaArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Ninja");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "When the wealthy and the powerful need an enemy eliminated quietly and without fail, they call upon the ninja. When a general needs to sabotage the siege engines of his foes before they can reach the castle walls, he calls upon the ninja. And when fools dare to move against a ninja or her companions, they will find the ninja waiting for them while they sleep, ready to strike. These shadowy killers are masters of infiltration, sabotage, and assassination, using a wide variety of weapons, practiced skills, and mystical powers to achieve their goals.");
            });
            Helpers.SetField(archetype, "m_ParentClass", rogue_class);
            library.AddAsset(archetype, "");

            var rogue_proficiencies = library.Get<BlueprintFeature>("33e2a7e4ad9daa54eaf808e1483bb43c");
            var weapon_finesse = library.Get<BlueprintFeature>("90e54424d682d104ab36436bd527af09");
            var trapfinding = library.Get<BlueprintFeature>("dbb6b3bffe6db3547b31c3711653838e");
            var evasion = library.Get<BlueprintFeature>("576933720c440aa4d8d42b0c54b77e80");
            var rogue_talent = library.Get<BlueprintFeatureSelection>("c074a5d615200494b8f2a9c845799d93");
            var finesse_training_selection = library.Get<BlueprintFeature>("b78d146cea711a84598f0acef69462ea");
            var danger_sense = library.Get<BlueprintFeature>("0bcbe9e450b0e7b428f08f66c53c5136");
            var debilitating_injury = library.Get<BlueprintFeature>("def114eb566dfca448e998969bf51586");
            var uncanny_dodge = library.Get<BlueprintFeature>("3c08d842e802c3e4eb19d15496145709");
            var improved_uncanny_dodge = library.Get<BlueprintFeature>("485a18c05792521459c7d06c63128c79");
            var advanced_talents = library.Get<BlueprintFeature>("a33b99f95322d6741af83e9381b2391c");
            var master_strike = library.Get<BlueprintFeature>("72dcf1fb106d5054a81fd804fdc168d3");

            createNinjaProficiencies();
            createNoTrace();
            createKiPool();
            createLightSteps();
            createNinjaTrick();

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, rogue_proficiencies, trapfinding),
                                                          Helpers.LevelEntry(2, evasion, rogue_talent),
                                                          Helpers.LevelEntry(3, danger_sense),
                                                          Helpers.LevelEntry(4, rogue_talent, debilitating_injury),
                                                          Helpers.LevelEntry(6, rogue_talent, danger_sense),
                                                          Helpers.LevelEntry(8, rogue_talent),
                                                          Helpers.LevelEntry(9, danger_sense),
                                                          Helpers.LevelEntry(10, rogue_talent),
                                                          Helpers.LevelEntry(12, rogue_talent, danger_sense),
                                                          Helpers.LevelEntry(14, rogue_talent),
                                                          Helpers.LevelEntry(15, danger_sense),
                                                          Helpers.LevelEntry(16, rogue_talent),
                                                          Helpers.LevelEntry(18, rogue_talent, danger_sense),
                                                          Helpers.LevelEntry(20, rogue_talent), //will leave master strike as capstone
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, ninja_proficiencies),
                                                          Helpers.LevelEntry(2, ki_pool, ninja_trick),
                                                          Helpers.LevelEntry(3, no_trace),
                                                          Helpers.LevelEntry(4, ninja_trick),
                                                          Helpers.LevelEntry(6, ninja_trick, no_trace, light_steps),
                                                          Helpers.LevelEntry(8, ninja_trick),
                                                          Helpers.LevelEntry(9, no_trace),
                                                          Helpers.LevelEntry(10, ninja_trick), //master_trick = advanced talent?
                                                          Helpers.LevelEntry(12, ninja_trick, no_trace),
                                                          Helpers.LevelEntry(14, ninja_trick),
                                                          Helpers.LevelEntry(15, no_trace),
                                                          Helpers.LevelEntry(16, ninja_trick),
                                                          Helpers.LevelEntry(18, ninja_trick, no_trace),
                                                          Helpers.LevelEntry(20, ninja_trick),
                                                       };

            rogue_class.Progression.UIDeterminatorsGroup = rogue_class.Progression.UIDeterminatorsGroup.AddToArray(ninja_proficiencies);
            rogue_class.Progression.UIGroups[2].Features.Add(ki_pool);
            rogue_class.Progression.UIGroups[2].Features.Add(light_steps);
            rogue_class.Archetypes = rogue_class.Archetypes.AddToArray(archetype);
        }


        static void createNinjaProficiencies()
        {
            ninja_proficiencies = library.CopyAndAdd<BlueprintFeature>("33e2a7e4ad9daa54eaf808e1483bb43c", "NinjaProficiencies", "");
            ninja_proficiencies.ReplaceComponent<AddProficiencies>(a => a.WeaponProficiencies = new WeaponCategory[]
                                                                                                {
                                                                                                    WeaponCategory.Kama,
                                                                                                    WeaponCategory.Nunchaku,
                                                                                                    WeaponCategory.Sai,
                                                                                                    WeaponCategory.Shortbow,
                                                                                                    WeaponCategory.Shortsword,
                                                                                                    WeaponCategory.Shuriken,
                                                                                                    WeaponCategory.Scimitar
                                                                                                });
            ninja_proficiencies.AddComponent(Helpers.CreateAddFact(library.Get<BlueprintFeature>("9c37279588fd9e34e9c4cb234857492c")));//duelling sword proficiency
            ninja_proficiencies.SetNameDescription("Ninja Proficiencies",
                                                   "Ninja are proficient with all simple weapons, plus the kama, dueling sword, nunchaku, sai, shortbow, short sword, shuriken, and scimitar. Ninjas are proficient with light armor but not with shields.");

        }


        static void createNoTrace()
        {
            no_trace = Helpers.CreateFeature("NoTraceNinjaFeature",
                                             "No Trace",
                                             "At 3rd level, a ninja gains a +1 bonus on Stealth checks. This bonus increases by 1 every 3 ninja levels thereafter.",
                                             "",
                                             Helpers.GetIcon("97a6aa2b64dd21a4fac67658a91067d7"), //fast stealth
                                             FeatureGroup.None,
                                             Helpers.CreateAddContextStatBonus(StatType.SkillStealth, ModifierDescriptor.UntypedStackable)
                                             );
            no_trace.Ranks = 6;
            no_trace.ReapplyOnLevelUp = true;
            no_trace.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, feature: no_trace));
        }


        static void createKiPool()
        {
            var rogue_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("299aa766dee3cbf4790da4efb8c72484");

            ki_resource = Helpers.CreateAbilityResource("NinjaKiResource", "", "", "", null);
            ki_resource.SetIncreasedByStat(0, StatType.Charisma);
            ki_resource.SetIncreasedByLevelStartPlusDivStep(0, 0, 0, 2, 1, 0, 0.0f, getRogueArray());

            var buff_extra_attack = library.Get<BlueprintBuff>("cadf8a5c42002494cabfc6c1196b514a");
            buff_extra_attack.SetNameDescription("", ""); //inherit from parent ability

            var ki_extra_attack = library.CopyAndAdd<BlueprintAbility>("ca948bb4ce1a2014fbf4d8d44b553074", "NinjaKiExtraAttackAbility", "");
            ki_extra_attack.SetDescription("By spending 1 point from his ki pool as a swift action, a ninja can make one additional attack at his highest attack bonus when making a full attack. This bonus attack stacks with haste and similar effects.");
            ki_extra_attack.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = ki_resource);

            var ki_speed_buff = library.CopyAndAdd<BlueprintBuff>("9ea4ec3dc30cd7940a372a4d699032e7", "NinjaKiSpeedBoostBuff", "");
            ki_speed_buff.ReplaceComponent<BuffMovementSpeed>(b => b.Value = 20);
            var ki_speed_burst = library.CopyAndAdd<BlueprintAbility>("8c98b8f3ac90fa245afe14116e48c7da", "NinjaKiSpeedBoostAbility", "");

            var new_actions = Common.changeAction<ContextActionApplyBuff>(ki_speed_burst.GetComponent<AbilityEffectRunAction>().Actions.Actions,
                                                                          c =>
                                                                          {
                                                                              c.Buff = ki_speed_buff;
                                                                              c.DurationValue = Helpers.CreateContextDuration(1, DurationRate.Rounds);
                                                                          }
                                                                          );
            ki_speed_burst.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(new_actions));
            ki_speed_burst.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = ki_resource);
            ki_speed_burst.SetNameDescription("Ki Speed Burst", 
                                              "A ninja with this ki power can spend 1 point from his ki pool as a swift action to grant himself a sudden burst of speed. This increases the ninja's base land speed by 20 feet for 1 round.");

            ki_pool = Helpers.CreateFeature("KiPoolNinjaFeature",
                                            "Ki Pool",
                                            "At 2nd level, a ninja gains a pool of ki points, supernatural energy she can use to accomplish amazing feats. The number of points in the ninja’s ki pool is equal to 1/2 her ninja level + her Charisma modifier.\n"
                                            + "By spending 1 point from her ki pool, a ninja can make one additional attack at her highest attack bonus, but she can do so only when making a full attack. In addition, she can spend 1 point to increase her speed by 20 feet for 1 round. Each of these powers is activated as a swift action. A ninja can gain additional powers that consume points from her ki pool by selecting certain ninja tricks.",
                                            "",
                                            null,
                                            FeatureGroup.None,
                                            Helpers.CreateAddFacts(ki_extra_attack, ki_speed_burst),
                                            ki_resource.CreateAddAbilityResource());
        }


        static void createLightSteps()
        {
            var ability = library.CopyAndAdd<BlueprintAbility>("336a841704b7e2341b51f89fc9491f54", "NinjaLightSteps", "");
            ability.RemoveComponents<AbilityResourceLogic>();
            ability.Range = AbilityRange.Medium;
            Common.setAsFullRoundAction(ability);
            ability.SetNameDescription("Light Steps",
                                       "At 6th level, a ninja learns to move while barely touching the surface underneath her. As a full-round action, she can move to any location within medium range, ignoring difficult terrain. While moving in this way, any surface will support her, no matter how much she weighs. This allows her to move across water, lava, or even the thinnest tree branches. She must end her move on a surface that can support her normally. She cannot move across air in this way, nor can she walk up walls or other vertical surfaces. When moving in this way, she does not take damage from surfaces or hazards that react to being touched, such as lava or caltrops, nor does she need to make Acrobatics checks to avoid falling on slippery or rough surfaces. Finally, when using light steps, the ninja ignores any mechanical traps that use a location-based trigger.");
            light_steps = Common.AbilityToFeature(ability, false);
        }


        static void createNinjaTrick()
        {
            ninja_trick = library.CopyAndAdd<BlueprintFeatureSelection>("c074a5d615200494b8f2a9c845799d93", "NinjaTrickSelection", "");
            ninja_trick.SetNameDescriptionIcon("Ninja Tricks",
                                               "As a ninja continues her training, she learns a number of tricks that allow her to confuse her foes and grant her supernatural abilities. Starting at 2nd level, a ninja gains one ninja trick. She gains one additional ninja trick for every 2 levels attained after 2nd. Unless otherwise noted, a ninja cannot select an individual ninja trick more than once.",
                                               Helpers.GetIcon("14ec7a4e52e90fa47a4c8d63c69fd5c1")
                                               );


            var improved_unarmed_strike = library.Get<BlueprintFeature>("7812ad3672a4b9a4fb894ea402095167");
            ninja_trick.AllFeatures = ninja_trick.AllFeatures.AddToArray(improved_unarmed_strike, RogueTalents.assasinate);

            var evasion = library.Get<BlueprintFeature>("576933720c440aa4d8d42b0c54b77e80");

            createStyleMaster();
            createBombs();
            createAccelerationOfForm();
            createShadowClone();
            createVanishingTrickAndInvisibleBlade();
            createSeeTheUnseen();
            createHerbalCompound();
            createKamikaze();
            createUnarmedCombatMastery();
            createFlurryOfDarts();


            addToNinjaTricks(style_master);
            addToNinjaTricks(smoke_bomb);
            addToNinjaTricks(chocking_bomb);
            addToNinjaTricks(blinding_bomb, true);
            addToNinjaTricks(acceleration_of_form);
            addToNinjaTricks(shadow_clone);
            addToNinjaTricks(vanishing_trick);
            addToNinjaTricks(invisible_blade, true);
            addToNinjaTricks(see_the_unseen, true);
            addToNinjaTricks(herbal_compound);
            addToNinjaTricks(kamikaze);
            addToNinjaTricks(unarmed_combat_mastery, true);
            addToNinjaTricks(evasion, true);
            addToNinjaTricks(flurry_of_darts, false);
        }


        static void addToNinjaTricks(BlueprintFeature feature, bool advanced = false)
        {
            var advanced_talents = library.Get<BlueprintFeature>("a33b99f95322d6741af83e9381b2391c");
            feature.Groups = feature.Groups.AddToArray(FeatureGroup.RogueTalent);
            ninja_trick.AllFeatures = ninja_trick.AllFeatures.AddToArray(feature);
            if (advanced)
            {
                feature.AddComponent(Helpers.PrerequisiteFeature(advanced_talents));
            }
        }


        static void createFlurryOfDarts()
        {
            var buff = Helpers.CreateBuff("FlurryOfDartsBuff",
                                          "Flurry of Darts",
                                          "A ninja with this ability can expend 1 ki point from her ki pool as a swift action before she makes a full-attack attack with darts. During that attack, she can throw two additional darts at her highest attack bonus, but all of her darts attacks are made at a –2 penalty, including the two extra attacks.\n"
                                          + "Additionaly, darts are considered as light weapons for the purpose of two-weapon fighting.",
                                          "",
                                          Helpers.GetIcon("b296531ffe013c8499ad712f8ae97f6b"), //acid dart
                                          null,
                                          Helpers.Create<NewMechanics.BuffExtraAttackCategorySpecific>(b => { b.categories = new WeaponCategory[] { WeaponCategory.Dart }; b.num_attacks = 2; b.attack_bonus = -2; })
                                          );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1), dispellable: false);

            var ability = Helpers.CreateAbility("FlurryOfDartsAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Extraordinary,
                                                CommandType.Swift,
                                                AbilityRange.Personal,
                                                Helpers.oneRoundDuration,
                                                "",
                                                Helpers.CreateRunActions(apply_buff),
                                                ki_resource.CreateResourceLogic(),
                                                Helpers.Create<AbilityCasterMainWeaponCheck>(a => a.Category = new WeaponCategory[] {WeaponCategory.Dart})
                                                );
            ability.setMiscAbilityParametersSelfOnly();

            flurry_of_darts = Common.AbilityToFeature(ability, false);
            flurry_of_darts.AddComponent(Helpers.Create<HoldingItemsMechanics.ConsiderWeaponCategoriesAsLightWeapon>(c => c.categories = new WeaponCategory[] { WeaponCategory.Dart }));
        }


        static void createStyleMaster()
        {
            style_master = library.CopyAndAdd(Warpriest.sacred_fist_syle_feat_selection, "NinjaStyleMasterFeatureSelection", "");
            style_master.SetNameDescription("Style Master", "A ninja who selects this ninja trick gains a style feat that she qualifies for as a bonus feat.");
            style_master.AddComponent(Helpers.PrerequisiteNoFeature(style_master));
        }


        static void createBombs()
        {
            var smoke_bomb_ability = library.CopyAndAdd(NewSpells.obscuring_mist, "NinjaSmokeBombAbility", "");
            smoke_bomb_ability.RemoveComponents<SpellListComponent>();
            smoke_bomb_ability.RemoveComponents<ContextRankConfig>();
            smoke_bomb_ability.Type = AbilityType.Extraordinary;
            smoke_bomb_ability.AvailableMetamagic = 0;
            smoke_bomb_ability.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Thrown;
            smoke_bomb_ability.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionThrown;
            smoke_bomb_ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(Common.changeAction<ContextActionSpawnAreaEffect>(a.Actions.Actions, s => { s.AreaEffect = NewSpells.obscuring_mist_area; s.DurationValue = Helpers.CreateContextDuration(1, DurationRate.Minutes); })));
            smoke_bomb_ability.LocalizedDuration = Helpers.oneMinuteDuration;
            smoke_bomb_ability.Range = AbilityRange.Close;
            smoke_bomb_ability.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClass(getRogueArray()[0], StatType.Charisma));
            smoke_bomb_ability.AddComponent(ki_resource.CreateResourceLogic());
            smoke_bomb_ability.SetNameDescription("Smoke Bomb",
                                                  "This ability allows a ninja to throw a smoke bomb that creates a cloud of smoke with a 20 - foot radius. This acts like a fog cloud spell with duration of 1 minute. Using this ability is a standard action. Each use of this ability uses up 1 ki point.");
            smoke_bomb_ability.AddComponent(ki_resource.CreateAddAbilityResource());
            smoke_bomb = Common.AbilityToFeature(smoke_bomb_ability, false);


            var chocking_bomb_ability = library.CopyAndAdd(smoke_bomb_ability, "NinjaNausetingBombAbility", "");
            var chocking_bomb_area = library.CopyAndAdd<BlueprintAbilityAreaEffect>(NewSpells.obscuring_mist_area, "ChockingBombArea", "");
            var staggered_buff = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");
            var apply_buff = Helpers.CreateConditionalSaved(null, Common.createContextActionApplyBuff(staggered_buff, Helpers.CreateContextDuration(0, DurationRate.Rounds, DiceType.D4, 1), dispellable: false));

            var apply_buff_saved = Helpers.CreateConditional(Common.createContextConditionHasFact(staggered_buff), null,
                                                             Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(apply_buff)));
            var chocking_bomb_action = Helpers.Create<NewMechanics.AbilityAreaEffectRunActionWithFirstRound>(a =>
                                            {
                                                a.UnitEnter = Helpers.CreateActionList(apply_buff_saved);
                                                a.Round = Helpers.CreateActionList(apply_buff_saved);
                                                a.FirstRound = Helpers.CreateActionList(apply_buff_saved);
                                            });
            chocking_bomb_area.AddComponent(chocking_bomb_action);
            chocking_bomb_area.Fx = library.Get<BlueprintAbilityAreaEffect>("aa2e0a0fe89693f4e9205fd52c5ba3e5").Fx;  //stinking cloud
            chocking_bomb_ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(Common.changeAction<ContextActionSpawnAreaEffect>(a.Actions.Actions, s => { s.AreaEffect = chocking_bomb_area; s.DurationValue = Helpers.CreateContextDuration(1, DurationRate.Minutes); })));
            chocking_bomb_ability.SetNameDescriptionIcon("Chocking Bomb",
                                                     "Whenever a ninja throws a smoke bomb, all living creatures in the resulting cloud must make a Fortitude save or become staggered by the choking black smoke for 1d4 rounds. The DC of this saving throw is equal to 10 + 1/2 the ninja’s level + the ninja’s Charisma modifier.",
                                                     staggered_buff.Icon);
            chocking_bomb = Common.AbilityToFeature(chocking_bomb_ability, false);
            chocking_bomb.AddComponent(Helpers.PrerequisiteFeature(smoke_bomb));


            var blinding_bomb_ability = library.CopyAndAdd(smoke_bomb_ability, "NinjaBlindingBombAbility", "");
            var blinding_bomb_area = library.CopyAndAdd<BlueprintAbilityAreaEffect>(NewSpells.obscuring_mist_area, "BlindingBombArea", "");
            var blind_buff = library.Get<BlueprintBuff>("187f88d96a0ef464280706b63635f2af");
            var apply_buff2 = Helpers.CreateConditionalSaved(null, Common.createContextActionApplyBuff(blind_buff, Helpers.CreateContextDuration(0, DurationRate.Rounds, DiceType.D4, 1), dispellable: false));

            var apply_buff_saved2 = Helpers.CreateConditional(Common.createContextConditionHasFact(blind_buff), null,
                                                             Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(apply_buff2)));
            var blinding_bomb_action = Helpers.Create<NewMechanics.AbilityAreaEffectRunActionWithFirstRound>(a =>
            {
                a.UnitEnter = Helpers.CreateActionList(apply_buff_saved2);
                a.Round = Helpers.CreateActionList(apply_buff_saved2);
                a.FirstRound = Helpers.CreateActionList(apply_buff_saved2);
            });
            blinding_bomb_area.AddComponent(blinding_bomb_action);
            blinding_bomb_area.Fx = library.Get<BlueprintAbilityAreaEffect>("aa2e0a0fe89693f4e9205fd52c5ba3e5").Fx;  //stinking cloud
            blinding_bomb_ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(Common.changeAction<ContextActionSpawnAreaEffect>(a.Actions.Actions, s => { s.AreaEffect = blinding_bomb_area; s.DurationValue = Helpers.CreateContextDuration(1, DurationRate.Minutes); })));
            blinding_bomb_ability.SetNameDescriptionIcon("Blinding Bomb",
                                                         "Whenever the ninja throws a smoke bomb, all living creatures in the cloud must make a Fortitude save or be blinded by the black smoke for 1d4 rounds. The DC of this saving throw is equal to 10 + 1/2 the ninja’s level + the ninja’s Charisma modifier.",
                                                         Helpers.GetIcon("bd05918a568c41e49aed7b9526ba596b") //blinding bomb
                                                         );
            blinding_bomb = Common.AbilityToFeature(blinding_bomb_ability, false);
            blinding_bomb.AddComponents(Helpers.PrerequisiteFeature(smoke_bomb), Helpers.PrerequisiteFeature(chocking_bomb));
        }


        static void createAccelerationOfForm()
        {
            var displacement_buff = library.Get<BlueprintBuff>("00402bae4442a854081264e498e7a833");
            var haste_buff = library.Get<BlueprintBuff>("03464790f40c3c24aa684b57155f3280");

            var apply_displacement = Common.createContextActionApplyBuff(displacement_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Rounds), dispellable: false);
            var apply_haste = Common.createContextActionApplyBuff(haste_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Rounds), dispellable: false);

            var ability = Helpers.CreateAbility("NinjaAccelerationOfFormAbility",
                                                "Acceleration of Form",
                                                "A ninja with this trick can spend 1 ki point as a standard action to gain the benefits of displacement and haste for 1 round per 2 ninja levels.",
                                                "",
                                                displacement_buff.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Personal,
                                                "1 round/ 2 levels",
                                                "",
                                                Helpers.CreateRunActions(apply_displacement, apply_haste),
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getRogueArray(), progression: ContextRankProgression.Div2),
                                                ki_resource.CreateResourceLogic()
                                                );
            ability.setMiscAbilityParametersSelfOnly();
            acceleration_of_form = Common.AbilityToFeature(ability, false);
        }


        static void createShadowClone()
        {
            var mirror_image = library.Get<BlueprintAbility>("3e4ab69ada402d145a5e0ad3ad4b8564");
            var ability = Common.convertToSuperNatural(mirror_image, "NinjaShadowClone", getRogueArray(), StatType.Charisma, ki_resource);
            ability.SetName("Shadow Clone");
            shadow_clone = Helpers.CreateFeature("NinjaShadowCloneFeature",
                                                 "Shadow Clone",
                                                 "The ninja can create 1d4 shadowy duplicates of herself that conceal her true location. This ability functions as mirror image, using the ninja’s level as her caster level. Using this ability is a standard action that uses up 1 ki point.",
                                                 "",
                                                 mirror_image.Icon,
                                                 FeatureGroup.None,
                                                 Helpers.CreateAddFact(ability)
                                                 );
        }


        static void createVanishingTrickAndInvisibleBlade()
        {
            var invisibility_buff = library.Get<BlueprintBuff>("525f980cb29bc2240b93e953974cb325");
            var improved_invisibility_buff = library.Get<BlueprintBuff>("e6b35473a237a6045969253beb09777c");

            invisible_blade = Helpers.CreateFeature("NinjaInvisibleBladeFeature",
                                                    "Invisible Blade",
                                                    "Whenever a ninja uses the vanishing trick ninja trick, she is treated as if she were under the effects of greater invisibility.",
                                                    "",
                                                    improved_invisibility_buff.Icon,
                                                    FeatureGroup.None);

            var apply_invisibility = Common.createContextActionApplyBuff(invisibility_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);
            var apply_invisibility_improved = Common.createContextActionApplyBuff(improved_invisibility_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);

            var action = Helpers.CreateConditional(Common.createContextConditionHasFact(invisible_blade), apply_invisibility_improved, apply_invisibility);

            var ability = Helpers.CreateAbility("NinjaVanishingTrickAbility",
                                                "Vanishing Trick",
                                                "As a swift action, the ninja can disappear for 1 round per level. This ability functions as invisibility. Using this ability uses up 1 ki point.",
                                                "",
                                                invisibility_buff.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Swift,
                                                AbilityRange.Personal,
                                                Helpers.roundsPerLevelDuration,
                                                "",
                                                Helpers.CreateRunActions(action),
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getRogueArray(), progression: ContextRankProgression.AsIs),
                                                ki_resource.CreateResourceLogic()
                                                );
            ability.setMiscAbilityParametersSelfOnly();
            vanishing_trick = Common.AbilityToFeature(ability, false);
            invisible_blade.AddComponent(Helpers.PrerequisiteFeature(vanishing_trick));
        }


        static void createSeeTheUnseen()
        {
            var see_invisibility = library.Get<BlueprintAbility>("30e5dc243f937fc4b95d2f8f4e1b7ff3");

            var ability = Common.convertToSuperNatural(see_invisibility, "NinjaSeeTheUnseen", getRogueArray(), StatType.Charisma, ki_resource);
            ability.SetName("See the Unseen");
            ability.ActionType = CommandType.Swift;
            see_the_unseen = Common.AbilityToFeature(ability, false);
            see_the_unseen.SetDescription("A ninja with this trick learns how to see that which cannot be seen. As a swift action, the ninja can cast see invisibility, using her level as the caster level. Each use of this ability uses up 1 ki point.");
        }


        static void createHerbalCompound()
        {
            var buff = Helpers.CreateBuff("NinjaHerbalCompoundBuff",
                                          "Herbal Compound",
                                          "A ninja with this trick can consume specially prepared herbs to strengthen her mind. The ninja can spend 1 ki point and smoke, eat, or inject an herbal compound as a move action. The ninja takes a –2 penalty to AC and on Reflex saves but gains a +4 alchemical bonus on Will saves for 10 minutes per ninja level.",
                                          "",
                                          Helpers.GetIcon("557898e059f5ff644848b0a4df087391"), //force bomb
                                          null,
                                          Helpers.CreateAddStatBonus(StatType.AC, -2, ModifierDescriptor.Penalty),
                                          Helpers.CreateAddStatBonus(StatType.SaveReflex, -2, ModifierDescriptor.Penalty),
                                          Helpers.CreateAddStatBonus(StatType.SaveWill, 4, ModifierDescriptor.Alchemical)
                                          );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.TenMinutes), dispellable: false);
          
            var ability = Helpers.CreateAbility("NinjaHerbalCompoundAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Extraordinary,
                                                CommandType.Move,
                                                AbilityRange.Personal,
                                                Helpers.tenMinPerLevelDuration,
                                                "",
                                                Helpers.CreateRunActions(apply_buff),
                                                ki_resource.CreateResourceLogic(),
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getRogueArray(), progression: ContextRankProgression.AsIs),
                                                Common.createAbilitySpawnFx("2afcad44a5112d54ebfc8248df7e6525", anchor: AbilitySpawnFxAnchor.SelectedTarget) //owls wisdom
                                                );
            ability.setMiscAbilityParametersSelfOnly();
            herbal_compound = Common.AbilityToFeature(ability, false);
        }


        static void createKamikaze()
        {
            var rage_buff = library.Get<BlueprintBuff>("da8ce41ac3cd74742b80984ccc3c9613");
            var vicious = library.Get<BlueprintWeaponEnchantment>("a1455a289da208144981e4b1ef92cc56");
            var buff = Helpers.CreateBuff("NinjaKamikazeBuff",
                                          "Kamikaze",
                                          "A ninja with this ability strikes without concern for her own well-being. The ninja can spend 1 point from her ki pool to give her unarmed strikes and any weapons she wields the vicious weapon special ability for 1 round per level. ",
                                          "",
                                          rage_buff.Icon,
                                          rage_buff.FxOnStart,
                                          Helpers.Create<NewMechanics.EnchantmentMechanics.PersistentWeaponEnchantment>(p => p.enchant = vicious)
                                          );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Rounds), dispellable: false);

            var ability = Helpers.CreateAbility("NinjaKamikazeAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Personal,
                                                Helpers.roundsPerLevelDuration,
                                                "",
                                                Helpers.CreateRunActions(apply_buff),
                                                ki_resource.CreateResourceLogic(),
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getRogueArray(), progression: ContextRankProgression.AsIs)
                                                );
            ability.setMiscAbilityParametersSelfOnly();
            kamikaze = Common.AbilityToFeature(ability, false);
        }


        static void createUnarmedCombatMastery()
        {
            var monk = library.Get<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");
            var fist1d6 = library.Get<BlueprintFeature>("c3fbeb2ffebaaa64aa38ce7a0bb18fb0");
           
            var fake_monk_class = library.CopyAndAdd<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124", "FakeNinjjaMonkClassForUnarmedCombatMastery", "");
            ClassToProgression.addClassToFeat(fake_monk_class, new BlueprintArchetype[] {}, ClassToProgression.DomainSpellsType.NoSpells, fist1d6, monk);

            /*var unarmed1d8 = library.Get<BlueprintFeature>("8267a0695a4df3f4ca508499e6164b98");
            var unarmed1d10 = library.Get<BlueprintFeature>("f790a36b5d6f85a45a41244f50b947ca");
            var unarmed2d6 = library.Get<BlueprintFeature>("b3889f445dbe42948b8bb1ba02e6d949");
            var unarmed2d8 = library.Get<BlueprintFeature>("078636a2ce835e44394bb49a930da230");*/

            var improved_unarmed_strike = library.Get<BlueprintFeature>("7812ad3672a4b9a4fb894ea402095167");

            unarmed_combat_mastery = Helpers.CreateFeature("NinjaUnarmedCombatMastery",
                                                           "Unarmed Combat Mastery",
                                                           "A ninja who selects this trick deals damage with her unarmed strikes as if she were a monk of her ninja level –4. If the ninja has levels in monk, this ability stacks with monk levels to determine how much damage she can do with her unarmed strikes.",
                                                           "",
                                                           Helpers.GetIcon("641dc4bbfb8c13b43a879ba9a2e196b3"), //finesse training unarmed
                                                           FeatureGroup.None,
                                                           fist1d6.ComponentsArray
                                                           );
            unarmed_combat_mastery.AddComponents(Helpers.PrerequisiteFeature(improved_unarmed_strike),
                                                 Helpers.Create<FakeClassLevelMechanics.AddFakeClassLevel>(a => { a.fake_class = fake_monk_class; a.value = Helpers.CreateContextValue(AbilityRankType.DamageDiceAlternative); }),
                                                 Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getRogueArray(), type: AbilityRankType.DamageDiceAlternative,
                                                                                 progression: ContextRankProgression.BonusValue, stepLevel: -4, min: 0)
                                                );
            unarmed_combat_mastery.ReapplyOnLevelUp = true;
        }

    }
}
