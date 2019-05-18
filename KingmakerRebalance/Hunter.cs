using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;



namespace KingmakerRebalance
{
    public class Hunter
    {
        static internal readonly Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup AnimalFocusGroup = Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.Judgment;
        public static void addAnimalFocus()
        {
            Kingmaker.Blueprints.Classes.LevelEntry initial_entry = new Kingmaker.Blueprints.Classes.LevelEntry();
            Kingmaker.Blueprints.Classes.LevelEntry add_animal_focus_use_entry = new Kingmaker.Blueprints.Classes.LevelEntry();
            var wildshape_wolf = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Classes.BlueprintFeature>("19bb148cb92db224abb431642d10efeb");
            var acid_maw = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("75de4ded3e731dc4f84d978fe947dc67");
            var add_animal_focuse_use_component = new Kingmaker.UnitLogic.FactLogic.IncreaseActivatableAbilityGroupSize();
            add_animal_focuse_use_component.Group = AnimalFocusGroup;

            var add_animal_focus_use = Helpers.CreateFeature("AdditionalAnimalFocusFeature",
                                                             "Additional Animal Focus",
                                                             "The character can apply additional animal focus to herself.",
                                                             "896f036314b049bfa723b74b0e509a89",
                                                             wildshape_wolf.Icon,
                                                             FeatureGroup.None,
                                                             add_animal_focuse_use_component
                                                            );
            add_animal_focus_use.Ranks = 1;

            var animal_focus = createAnimalFocus();

            var animal_focus_ac = Helpers.CreateFeature("AnimalFocusAc",
                                                        "Animal Focus (Animal Companion)",
                                                        "The character can apply animal focus to her animal companion.",
                                                        "5eea1e98d11c4acbafc1f9b4abf6cae6",
                                                        acid_maw.Icon,
                                                        FeatureGroup.None,
                                                        createAddFeatToAnimalCompanion(animal_focus)
                                                        );
            var add_animal_focus_use_ac = Helpers.CreateFeature("AdditonalAnimalFocusAc",
                                                        "Additional Animal Focus (Animal Companion)",
                                                        "The character can apply one more animal focus to her animal companion.",
                                                        "06bd293935354563be67cb5d2679a9bf",
                                                        acid_maw.Icon,
                                                        FeatureGroup.None,
                                                        createAddFeatToAnimalCompanion(add_animal_focus_use)
                                                        );


            initial_entry.Level = 4;
            initial_entry.Features.Add(animal_focus);
            initial_entry.Features.Add(animal_focus_ac);
            add_animal_focus_use_entry.Level = 17;
            add_animal_focus_use_entry.Features.Add(add_animal_focus_use);
            add_animal_focus_use_entry.Features.Add(add_animal_focus_use_ac);

            var sacred_huntsmaster_archetype = ResourcesLibrary.TryGetBlueprint<BlueprintArchetype>("46eb929c8b6d7164188eb4d9bcd0a012");
            var additional_level_entry_features = new LevelEntry[2] { initial_entry, add_animal_focus_use_entry };
            var new_features = additional_level_entry_features.Concat(sacred_huntsmaster_archetype.AddFeatures).ToArray();
            Array.Sort(new_features, (x, y) => x.Level.CompareTo(y.Level));
            sacred_huntsmaster_archetype.AddFeatures = new_features;

            var inquisitor_progression = ResourcesLibrary.TryGetBlueprint<BlueprintProgression>("4e945c2fe5e252f4ea61eee7fb560017");
            Kingmaker.Blueprints.Classes.UIGroup animal_focus_ui_group = new Kingmaker.Blueprints.Classes.UIGroup();
            animal_focus_ui_group.Features.Add(animal_focus);
            animal_focus_ui_group.Features.Add(add_animal_focus_use);
            inquisitor_progression.UIGroups = inquisitor_progression.UIGroups.AddToArray(animal_focus_ui_group);

            Kingmaker.Blueprints.Classes.UIGroup animal_focus_ac_ui_group = new Kingmaker.Blueprints.Classes.UIGroup();
            animal_focus_ac_ui_group.Features.Add(animal_focus_ac);
            animal_focus_ac_ui_group.Features.Add(add_animal_focus_use_ac);
            inquisitor_progression.UIGroups = inquisitor_progression.UIGroups.AddToArray(animal_focus_ac_ui_group);

            //remove racial enemies on sacred huntsmaster
            var racial_enemy_feat = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("16cc2c937ea8d714193017780e7d4fc6");
            var racial_enemy_rankup_feat = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("c1be13839472aad46b152cf10cf46179");
            foreach (var lvl_entry in sacred_huntsmaster_archetype.AddFeatures)
            {
                lvl_entry.Features.Remove(racial_enemy_feat);
                lvl_entry.Features.Remove(racial_enemy_rankup_feat);
            }
        }


        static Kingmaker.Designers.Mechanics.Facts.AddFeatureToCompanion createAddFeatToAnimalCompanion(BlueprintFeature feat)
        {
            var add_feat_ac = new Kingmaker.Designers.Mechanics.Facts.AddFeatureToCompanion();
            add_feat_ac.Feature = feat;
            return add_feat_ac;
        }


        public static BlueprintFeature createAnimalFocus()
        {
            var inquistor_class = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Classes.BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce");
            var animal_class = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Classes.BlueprintCharacterClass>("4cd1757a0eea7694ba5c933729a53920");
            var sacred_huntsmaster_archetype = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Classes.BlueprintArchetype>("46eb929c8b6d7164188eb4d9bcd0a012");
            var bull_strength = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("4c3d08935262b6544ae97599b3a9556d");
            var bear_endurance = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("a900628aea19aa74aad0ece0e65d091a");
            var cat_grace = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("de7a025d48ad5da4991e7d3c682cf69d");
            var aspect_falcon = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("7bdb6a9fb6b37614e96f155748ae50c6");
            var owl_wisdom = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("f0455c9295b53904f9e02fc571dd2ce1");
            var heroism = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("5ab0d42fb68c9e34abae4921822b9d63"); //for monkey
            var feather_step = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("f3c0b267dd17a2a45a40805e31fe3cd1"); //for frog
            var longstrider  = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("14c90900b690cac429b229efdf416127"); //for stag
            var summon_monster1 = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("8fd74eddd9b6c224693d9ab241f25e84"); //for mouse
            (int, int)[] progressionStat = new (int, int)[3]{(7,2), (14, 4), (20, 6)};
            (int, int)[] progressionSkill = new (int, int)[3]{(7,4), (14, 6), (20, 8)};
            (int, int)[] progressionSpeed = new (int, int)[3]{(7,5), (14, 10), (20, 20)};
            BlueprintCharacterClass[] allowed_classes = new BlueprintCharacterClass[2]{inquistor_class, animal_class};

            var mouse_focus = createMouseFocus(summon_monster1.Icon, allowed_classes, sacred_huntsmaster_archetype, 12);
            BlueprintComponent animal_foci = KingmakerRebalance.Helpers.CreateAddFacts(createScaledFocus("BullAspect",
                                                                                            "Bull Aspect",
                                                                                            "The character gains a +2 enhancement bonus to Strength. This bonus increases to +4 at 8th level and +6 at 15th level",
                                                                                            "1fa6cfa7421b4b60b41b2f055363ebe5",
                                                                                            "3230b40a4c314fe6be77e4b07af49a4a",
                                                                                            bull_strength.Icon,
                                                                                            Kingmaker.Enums.ModifierDescriptor.Enhancement,
                                                                                            Kingmaker.EntitySystem.Stats.StatType.Strength,
                                                                                            progressionStat,
                                                                                            allowed_classes,
                                                                                            sacred_huntsmaster_archetype),
                                                                         createScaledFocus("BearAspect",
                                                                                            "Bear Aspect",
                                                                                            "The character gains a +2 enhancement bonus to Constitution. This bonus increases to +4 at 8th level and +6 at 15th level.",
                                                                                            "dd96c3c629e94c17830a4d8f0fcdc08f",
                                                                                            "de5113284399417e94a0e5c15cca5872",
                                                                                            bear_endurance.Icon,
                                                                                            Kingmaker.Enums.ModifierDescriptor.Enhancement,
                                                                                            Kingmaker.EntitySystem.Stats.StatType.Constitution,
                                                                                            progressionStat, 
                                                                                            allowed_classes,
                                                                                            sacred_huntsmaster_archetype),
                                                                         createScaledFocus("TigerAspect",
                                                                                            "Tiger Aspect",
                                                                                            "The character gains a +2 enhancement bonus to Dexterity. This bonus increases to +4 at 8th level and +6 at 15th level.",
                                                                                            "a175ea28855547c6a5ac9c4ee8bd6429",
                                                                                            "7e9910e8cd394f5398f3d6b36885c26a",
                                                                                            cat_grace.Icon,
                                                                                            Kingmaker.Enums.ModifierDescriptor.Enhancement,
                                                                                            Kingmaker.EntitySystem.Stats.StatType.Dexterity,
                                                                                            progressionStat, 
                                                                                            allowed_classes,
                                                                                            sacred_huntsmaster_archetype),
                                                                         createScaledFocus("FalconAspect",
                                                                                            "Falcon Aspect",
                                                                                            "The character gains a +4 competence bonus on Perception checks. This bonus increases to +6 at 8th level and +8 at 15th level.",
                                                                                            "7fc508da39444660a47225979635d904",
                                                                                            "1a73b65baa024ce18943977284033df4",
                                                                                            aspect_falcon.Icon,
                                                                                            Kingmaker.Enums.ModifierDescriptor.Competence,
                                                                                            Kingmaker.EntitySystem.Stats.StatType.SkillPerception,
                                                                                            progressionSkill, 
                                                                                            allowed_classes,
                                                                                            sacred_huntsmaster_archetype),
                                                                         createScaledFocus("MonkeyAspect",
                                                                                            "Monkey Aspect",
                                                                                            "The character gains a +4 competence bonus on Athletics checks. This bonus increases to +6 at 8th level and +8 at 15th level.",
                                                                                            "54ab00a5b0e54b0dbd727c352bf61a19",
                                                                                            "0da42655cad24cb2bce785b13bb93e09",
                                                                                            heroism.Icon,
                                                                                            Kingmaker.Enums.ModifierDescriptor.Competence,
                                                                                            Kingmaker.EntitySystem.Stats.StatType.SkillAthletics,
                                                                                            progressionSkill, 
                                                                                            allowed_classes,
                                                                                            sacred_huntsmaster_archetype),
                                                                        createScaledFocus("OwlAspect",
                                                                                            "Owl Aspect",
                                                                                            "The character gains a +4 competence bonus on Stealth checks. This bonus increases to +6 at 8th level and +8 at 15th level.",
                                                                                            "4fad7e82e03c4ece81aa94e246f3f1c1",
                                                                                            "c02ae54193b044539db3236e4ef99139",
                                                                                            owl_wisdom.Icon,
                                                                                            Kingmaker.Enums.ModifierDescriptor.Competence,
                                                                                            Kingmaker.EntitySystem.Stats.StatType.SkillStealth,
                                                                                            progressionSkill, 
                                                                                            allowed_classes,
                                                                                            sacred_huntsmaster_archetype),
                                                                        createScaledFocus("FrogAspect",
                                                                                            "Frog Aspect",
                                                                                            "The character gains a +4 competence bonus on Mobility checks. This bonus increases to +6 at 8th level and +8 at 15th level.",
                                                                                            "a5e28c9f687849988f6c8d8586b9ed3f",
                                                                                            "2248cfbd1c1349d68095431244428843",
                                                                                            feather_step.Icon,
                                                                                            Kingmaker.Enums.ModifierDescriptor.Competence,
                                                                                            Kingmaker.EntitySystem.Stats.StatType.SkillMobility,
                                                                                            progressionSkill, 
                                                                                            allowed_classes,
                                                                                            sacred_huntsmaster_archetype),
                                                                       createScaledFocus("StagAspect",
                                                                                            "Stag Aspect",
                                                                                            "The character gains a 5-foot enhancement bonus to its base land speed. This bonus increases to 10 feet at 8th level and 20 feet at 15th level.",
                                                                                            "14c371bccb2240c18565d34fd210ff83",
                                                                                            "9e7bcb3eb48c4b67993365a599156077",
                                                                                            longstrider.Icon,
                                                                                            Kingmaker.Enums.ModifierDescriptor.Enhancement,
                                                                                            Kingmaker.EntitySystem.Stats.StatType.Speed,
                                                                                            progressionSpeed, 
                                                                                            allowed_classes,
                                                                                            sacred_huntsmaster_archetype)
                                                                         );

            var wildshape_wolf = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Classes.BlueprintFeature>("19bb148cb92db224abb431642d10efeb");
            var feat = KingmakerRebalance.Helpers.CreateFeature("AnimalFocusFeature",
                                                            "Animal Focus",
                                                            "Character can take the aspect of an animal as a swift action. She must select one type of animal to emulate, gaining a bonus or special ability based on the type of animal emulated and her level.",
                                                            "5a05ef60442c4fd38c418d4d190cb250",
                                                            wildshape_wolf.Icon,
                                                            FeatureGroup.None,
                                                            animal_foci, mouse_focus[0], mouse_focus[1]);
            return feat;
        }


        static Kingmaker.Designers.Mechanics.Facts.AddFeatureOnClassLevel[] createMouseFocus(UnityEngine.Sprite icon, BlueprintCharacterClass[] allowed_classes,
                                                                                                      BlueprintArchetype archetype, int update_lvl)
        {

            var mouse_focus1 = createToggleFocus("MouseAspect1",
                                            "Mouse Aspect",
                                            "The creature gains evasion, as the rogue class feature. At 12th level, this increases to improved evasion, as the rogue advanced talent.",
                                            "35e970a820614e1e92d8f616f70a6785",
                                            "9a1f080c968a4bd0ada30bf35ee7aa34",
                                            icon,
                                            new Kingmaker.Designers.Mechanics.Facts.Evasion());
            var mouse_focus2 = createToggleFocus("MouseAspect2",
                                           "Mouse Aspect",
                                           "The creature gains evasion, as the rogue class feature. At 12th level, this increases to improved evasion, as the rogue advanced talent.",
                                           "8bd28d45179441a2962a1acc3ed679d2",
                                           "b7977b0f52094ce4be1aaf113200ca1f",
                                           icon,
                                           new Kingmaker.Designers.Mechanics.Facts.Evasion(), new Kingmaker.Designers.Mechanics.Facts.ImprovedEvasion());

            var mouse_focus1f = Helpers.CreateFeature("MouseAspect1Feature",
                                                       "",
                                                       "708f7602863f4dda88d6dff8d9579d42",
                                                       "0ed62ee6b78e411e87c931fb6939fede",
                                                       icon,
                                                       FeatureGroup.None,
                                                       KingmakerRebalance.Helpers.CreateAddFact(mouse_focus1));
            mouse_focus1f.HideInUI = true;
            mouse_focus1f.HideNotAvailibleInUI = true;
            var mouse_focus2f = Helpers.CreateFeature("MouseAspect2Feature",
                                           "",
                                           "78b653737fa74c72be9c5933139376ad",
                                           "ee6fe29a2f7c4d4ebaad71c630ca4061",
                                           icon,
                                           FeatureGroup.None,
                                           KingmakerRebalance.Helpers.CreateAddFact(mouse_focus2));
            mouse_focus2f.HideInUI = true;
            mouse_focus2f.HideInCharacterSheetAndLevelUp = true;

            Kingmaker.Designers.Mechanics.Facts.AddFeatureOnClassLevel[] mouse_focus = new Kingmaker.Designers.Mechanics.Facts.AddFeatureOnClassLevel[2] {
                                                Helpers.CreateAddFeatureOnClassLevel(mouse_focus1f, update_lvl, allowed_classes, new BlueprintArchetype[1] { archetype }, true),
                                                Helpers.CreateAddFeatureOnClassLevel(mouse_focus2f, update_lvl, allowed_classes, new BlueprintArchetype[1] { archetype }, false)
                                                };

            return mouse_focus;
        }


        static Kingmaker.UnitLogic.ActivatableAbilities.BlueprintActivatableAbility createScaledFocus(string name, string display_name, string description,
                                                                                                      string buff_guid, string ability_guid,
                                                                                                      UnityEngine.Sprite icon,
                                                                                                      Kingmaker.Enums.ModifierDescriptor descriptor,
                                                                                                      Kingmaker.EntitySystem.Stats.StatType stat_type,
                                                                                                      (int, int)[] progression,
                                                                                                      BlueprintCharacterClass[] allowed_classes,
                                                                                                      BlueprintArchetype archetype)
        {
            BlueprintComponent[] components = new BlueprintComponent[2]{ KingmakerRebalance.Helpers.CreateContextRankConfig(ContextRankBaseValueType.MaxClassLevelWithArchetype, 
                                                                                                                      ContextRankProgression.Custom,
                                                                                                                      classes: allowed_classes,
                                                                                                                      archetype: archetype,
                                                                                                                      customProgression: progression),
                                                                       KingmakerRebalance.Helpers.CreateAddContextStatBonus(stat_type, 
                                                                                                                        descriptor,
                                                                                                                        Kingmaker.UnitLogic.Mechanics.ContextValueType.Rank)
                                                                     };          
            var focus = createToggleFocus(name,
                                          display_name,
                                          description,
                                          buff_guid,
                                          ability_guid,
                                          icon,
                                          components);
            return focus;
        }

 

        static Kingmaker.UnitLogic.ActivatableAbilities.BlueprintActivatableAbility createToggleFocus(string name, string display_name, string description, 
                                                                                                      string buff_guid, string ability_guid,
                                                                                                      UnityEngine.Sprite icon, params BlueprintComponent[] components)
        {
            var buff = KingmakerRebalance.Helpers.CreateBuff(name + "Buff",
                                                         display_name,
                                                         description,
                                                         buff_guid,
                                                         icon,
                                                         null,
                                                         components);

            var aspect = KingmakerRebalance.Helpers.CreateActivatableAbility(name,
                                                                         display_name,
                                                                         description,
                                                                         ability_guid,
                                                                         icon,
                                                                         buff,
                                                                         Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.Immediately,
                                                                         Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift,
                                                                         null);
            aspect.Group = AnimalFocusGroup;
            aspect.WeightInGroup = 1;
            aspect.DeactivateImmediately = true;
            return aspect;
        }



    }
}
