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
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
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

namespace CallOfTheWild
{
    public class Evolutions
    {
        static LibraryScriptableObject library => Main.library;

        static BlueprintFeature bear = library.Get<BlueprintFeature>("f6f1cdcc404f10c4493dc1e51208fd6f");
        static BlueprintFeature boar = library.Get<BlueprintFeature>("afb817d80b843cc4fa7b12289e6ebe3d");
        static BlueprintFeature centipede = library.Get<BlueprintFeature>("f9ef7717531f5914a9b6ecacfad63f46");
        static BlueprintFeature dog = library.Get<BlueprintFeature>("f894e003d31461f48a02f5caec4e3359");
        static BlueprintFeature elk = library.Get<BlueprintFeature>("aa92fea676be33d4dafd176d699d7996");
        static BlueprintFeature leopard = library.Get<BlueprintFeature>("2ee2ba60850dd064e8b98bf5c2c946ba");
        static BlueprintFeature mammoth = library.Get<BlueprintFeature>("6adc3aab7cde56b40aa189a797254271");
        static BlueprintFeature monitor = library.Get<BlueprintFeature>("ece6bde3dfc76ba4791376428e70621a");
        static BlueprintFeature smilodon = library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895");
        static BlueprintFeature wolf = library.Get<BlueprintFeature>("67a9dc42b15d0954ca4689b13e8dedea");
        static BlueprintFeature wolf_ekun = library.Get<BlueprintFeature>("e992949eba096644784592dc7f51a5c7");
        static BlueprintFeature[] animals = new BlueprintFeature[] { bear, boar, centipede, dog, elk, leopard, mammoth, monitor, smilodon, wolf, wolf_ekun };


        static public BlueprintFeature claws;
        static public BlueprintFeature slam_biped;
        static public BlueprintFeature bite;
        static public BlueprintFeature tail_slap;
        static public BlueprintFeature constrict;
        static public BlueprintFeature[] improved_natural_attacks;
        static public BlueprintFeature[] improved_natural_armor;
        static public BlueprintFeature reach;
        static public BlueprintFeature[] resistance; //summoner scaling
        static public BlueprintFeature[] skilled;
        static public BlueprintFeature wing_buffet;
        static public BlueprintFeature[][] ability_increase; // 6 x 4        
        static public BlueprintFeature[] energy_attacks;
        static public BlueprintFeature flight;
        static public BlueprintFeature gore;
        static public BlueprintFeature[] immunity;
        static public BlueprintFeature rake;
        static public BlueprintFeature trip;
        static public BlueprintFeature[] weapon_training;
        static public BlueprintFeature blindsense;
        static public BlueprintFeature pounce;
        static public BlueprintFeature amorphous;
        static public BlueprintFeature blindsight;
        static public BlueprintFeature[] fast_healing;
        static public BlueprintFeature[][] breath_weapon; //eidolon scaling
        static public BlueprintFeature spell_resistance; //summoner scaling
        static public BlueprintFeature[] size_increase;
        static public BlueprintFeature damage_reduction;
        static public BlueprintFeature poison_strength;
        static public BlueprintFeature poison_constitution;
        static public BlueprintFeature[] shared_slots;
        static public BlueprintFeature extra_attack_serpentine;
        static public BlueprintFeature extra_off_hand_attack_serpentine;
        static public BlueprintFeature extra_attack;
        static public BlueprintFeature extra_attack2;
        static public BlueprintFeature extra_off_hand_attack;
        static public BlueprintFeature extra_off_hand_attack2;

        static BlueprintFeature summoner_rank = library.Get<BlueprintFeature>("1670990255e4fe948a863bafd5dbda5d");
        static public BlueprintFeature[] extra_evolution = new BlueprintFeature[5];
        static BlueprintFeature eidolon;
        static public BlueprintFeatureSelection evolved_companion;

        static public List<BlueprintFeature> evolutions_list = new List<BlueprintFeature>();

        public class EvolutionEntry
        {
            public BlueprintFeature[] required_evolutions;
            public BlueprintFeature[] conflicting_evolutions;
            public BlueprintFeature[] subtypes;
            public int cost;
            public int summoner_level;
            public string group;
            public int total_cost;
            public bool has_upgrade;
            public int upgrade_cost;
            public BlueprintFeature[] base_evolutions;
            public BlueprintFeature evolution;
            public BlueprintFeature permanent_evolution;
            public BlueprintFeature self_selection_feature;
            public BlueprintFeature selection_feature;
            public BlueprintBuff buff;


            public EvolutionEntry(BlueprintFeature evolution_feature, int evolution_cost, int min_level, BlueprintFeature[] required_evolution_features,
                                  BlueprintFeature[] conflicting_evolution_features, BlueprintFeature[] authorised_subtypes, string evolution_group = "",
                                  bool upgradeable = false, int next_level_total_cost = 0, BlueprintFeature[] previous_evolutions = null, int full_cost = 0)
            {
                evolution = evolution_feature;
                cost = evolution_cost;
                summoner_level = min_level;
                conflicting_evolutions = conflicting_evolution_features;
                required_evolutions = required_evolution_features;
                subtypes = authorised_subtypes.ToArray();

                foreach (var subtype in authorised_subtypes)
                {
                    var lesser_subtype = Eidolon.getLesserEidolon(subtype as BlueprintProgression);
                    if (lesser_subtype != null)
                    {
                        subtypes = subtypes.AddToArray(lesser_subtype);
                    }
                }
                has_upgrade = upgradeable;
                upgrade_cost = next_level_total_cost;
                base_evolutions = previous_evolutions ?? new BlueprintFeature[0];
                group = evolution_group;
                total_cost = full_cost == 0 ? evolution_cost : full_cost;

                self_selection_feature = Helpers.CreateFeature("SelfSelect" + evolution.name,
                                          evolution.Name + $" ({cost} E. P., Personal)",
                                          evolution.Description,
                                          "",
                                          evolution.Icon,
                                          FeatureGroup.None,
                                          Helpers.Create<EvolutionMechanics.AddTemporarySelfEvolution>(a => { a.cost = cost; a.Feature = evolution; })
                                          );
                self_selection_feature.AddComponent(Helpers.Create<EvolutionMechanics.PrerequisiteEnoughSelfEvolutionPoints>(p => { p.amount = cost; p.feature = selection_feature; }));

                foreach (var e in required_evolutions)
                {
                    self_selection_feature.AddComponent(Helpers.Create<EvolutionMechanics.PrerequisiteSelfEvolution>(p => p.evolution = e));
                }
                foreach (var e in conflicting_evolutions)
                {
                    self_selection_feature.AddComponent(Helpers.Create<EvolutionMechanics.PrerequisiteSelfEvolution>(p => { p.evolution = e; p.not = true; }));
                }
                if (!subtypes.Empty())
                {
                    self_selection_feature.AddComponent(Helpers.PrerequisiteFeaturesFromList(subtypes));
                }
                if (summoner_level > 0)
                {
                    self_selection_feature.AddComponent(Helpers.Create<NewMechanics.PrerequisiteMinimumFeatureRank>(p => { p.Feature = summoner_rank; p.value = summoner_level; }));
                }

                selection_feature = Helpers.CreateFeature("Select" + evolution.name,
                                                          evolution.Name + $" ({cost} E. P.)",
                                                          evolution.Description,
                                                          "",
                                                          evolution.Icon,
                                                          FeatureGroup.None,
                                                          Helpers.Create<EvolutionMechanics.AddTemporaryEvolution>(a => { a.cost = cost; a.Feature = evolution; }),
                                                          Helpers.Create<EvolutionMechanics.PrerequisiteNoPermanentEvolution>(p => p.evolution = evolution)
                                                          );
                selection_feature.AddComponent(Helpers.Create<EvolutionMechanics.PrerequisiteEnoughEvolutionPoints>(p => { p.amount = cost; p.feature = selection_feature;}));

                foreach (var e in required_evolutions)
                {
                    selection_feature.AddComponent(Helpers.Create<EvolutionMechanics.PrerequisiteEvolution>(p => p.evolution = e));
                }
                foreach (var e in conflicting_evolutions)
                {
                    selection_feature.AddComponent(Helpers.Create<EvolutionMechanics.PrerequisiteEvolution>(p => { p.evolution = e; p.not = true; }));
                }
                if (!subtypes.Empty())
                {
                    selection_feature.AddComponent(Helpers.PrerequisiteFeaturesFromList(subtypes));
                }
                if (summoner_level > 0)
                {
                    selection_feature.AddComponent(Helpers.Create<NewMechanics.PrerequisiteMinimumFeatureRank>(p => { p.Feature = summoner_rank; p.value = summoner_level; }));
                }


                permanent_evolution = Helpers.CreateFeature("Permanent" + evolution.name,
                                                          evolution.Name + $" (Permanent)",
                                                          evolution.Description,
                                                          "",
                                                          evolution.Icon,
                                                          FeatureGroup.None,
                                                          Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => {a.Feature = evolution; }),
                                                          Helpers.Create<EvolutionMechanics.PrerequisiteEvolution>(p => { p.evolution = evolution; p.not = true; }));
                if (!subtypes.Empty())
                {
                    permanent_evolution.AddComponent(Helpers.PrerequisiteFeaturesFromList(subtypes));
                }
                foreach (var e in required_evolutions)
                {
                    permanent_evolution.AddComponent(Helpers.Create<EvolutionMechanics.PrerequisiteEvolution>(p => p.evolution = e));
                }
                foreach (var e in conflicting_evolutions)
                {
                    permanent_evolution.AddComponent(Helpers.Create<EvolutionMechanics.PrerequisiteEvolution>(p => { p.evolution = e; p.not = true; }));
                }
                if (summoner_level > 0)
                {
                    permanent_evolution.AddComponent(Helpers.Create<NewMechanics.PrerequisiteMinimumFeatureRank>(p => { p.Feature = summoner_rank; p.value = summoner_level; }));
                }

                buff = Helpers.CreateBuff(evolution.name +"Buff",
                                            "",
                                            "",
                                            "",
                                            evolution.Icon,
                                            null,
                                            Helpers.Create<EvolutionMechanics.AddShortDurationEvolution>(a => { a.Feature = evolution; }));
                foreach (var be in base_evolutions)
                {
                    buff.AddComponent(Helpers.Create<EvolutionMechanics.AddShortDurationEvolution>(a => { a.Feature = be; }));
                }
            }
        }

        static List<EvolutionEntry> evolution_entries = new List<EvolutionEntry>();
        static public BlueprintFeatureSelection evolution_selection;
        static public BlueprintFeatureSelection self_evolution_selection;
        static public BlueprintAbility getGrantTemporaryEvolutionAbility(int max_cost,
                                                                          bool remove_buffs,
                                                                          string name_prefix, string display_name,
                                                                          string description,
                                                                          UnityEngine.Sprite icon,
                                                                          AbilityType ability_type,
                                                                          UnitCommand.CommandType command_type,
                                                                          string duration,
                                                                          bool reuse_remaining_cost,
                                                                          params BlueprintComponent[] components)
        {
            BlueprintBuff[] remaining_points_buffs = new BlueprintBuff[0];
            if (reuse_remaining_cost)
            {
                var remaining_ability = getGrantTemporaryEvolutionAbility(max_cost - 1, false,
                                                                      $"Remaining" + name_prefix,
                                                                      display_name + $" (Remaining E. P.)",
                                                                      description,
                                                                      icon,
                                                                      ability_type,
                                                                      CommandType.Free,
                                                                      duration,
                                                                      false,
                                                                      components);

                remaining_points_buffs = new BlueprintBuff[max_cost - 1];
                for (int i = 1; i <= max_cost - 1; i++)
                {
                    var remaining_buff = Helpers.CreateBuff(remaining_ability.name + $"{i}Buff",
                                                            display_name + $" (Remained {i} E. P.)",
                                                            description,
                                                            "",
                                                            icon,
                                                            null,
                                                            Helpers.Create<ReplaceAbilityParamsWithContext>(r => r.Ability = remaining_ability),
                                                            Helpers.CreateAddFact(remaining_ability)
                                                            );
                    remaining_points_buffs[i - 1] = remaining_buff;
                }

                var remove_remaining_buffs = Helpers.Create<NewMechanics.ContextActionRemoveBuffs>(c => c.Buffs = remaining_points_buffs);
                foreach (var v in remaining_ability.Variants)
                {
                    var execute_action_on_cast = v.GetComponent<AbilityExecuteActionOnCast>();
                    if (execute_action_on_cast == null)
                    {
                        v.AddComponent(Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(remove_remaining_buffs)));
                    }
                    else
                    {
                        execute_action_on_cast = v.GetComponent<AbilityExecuteActionOnCast>();
                        execute_action_on_cast.Actions = Helpers.CreateActionList(execute_action_on_cast.Actions.Actions.AddToArray(remove_remaining_buffs));
                    }
                    v.AddComponent(Common.createAbilityCasterHasFacts(remaining_points_buffs.Skip(v.GetComponent<EvolutionMechanics.AbilityEvolutionCost>().cost - 1).ToArray()));
                    v.AddComponent(Common.createAbilityShowIfCasterHasAnyFacts(remaining_points_buffs.Skip(v.GetComponent<EvolutionMechanics.AbilityEvolutionCost>().cost - 1).ToArray()));
                }
            }
            List<BlueprintAbility> abilities = new List<BlueprintAbility>();
            List<ContextActionRemoveBuff> buffs_to_remove = new List<ContextActionRemoveBuff>();
            if (remove_buffs)
            {
                foreach (var ee in evolution_entries)
                {
                    buffs_to_remove.Add(Common.createContextActionRemoveBuff(ee.buff));
                }
                foreach (var b in remaining_points_buffs)
                {
                    buffs_to_remove.Add(Common.createContextActionRemoveBuff(b));
                }
            }
            var fx = Helpers.Create<ContextActionsOnPet>(c => c.Actions = Helpers.CreateActionList(Common.createContextActionSpawnFx(Common.createPrefabLink("352469f228a3b1f4cb269c7ab0409b8e"))));
            foreach (var ee in evolution_entries)
            {
                if (ee.total_cost <= max_cost && (!ee.has_upgrade || ee.upgrade_cost > max_cost))
                {
                   var apply_buff = Common.createContextActionApplyBuffToCaster(ee.buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes),
                                                                                 dispellable: ability_type == AbilityType.Spell || ability_type == AbilityType.SpellLike,
                                                                                 is_from_spell: ability_type == AbilityType.Spell || ability_type == AbilityType.SpellLike);

                   var ability = Helpers.CreateAbility(name_prefix + ee.buff.name + "Ability",
                                                        display_name + " (" + ee.evolution.Name +")",
                                                        description + "\n" + ee.evolution.Name + ": " + ee.evolution.Description,
                                                        "",
                                                        ee.buff.Icon,
                                                        ability_type,
                                                        command_type,
                                                        AbilityRange.Personal,
                                                        duration,
                                                        Helpers.savingThrowNone,
                                                        Helpers.CreateRunActions(buffs_to_remove.ToArray<GameAction>().AddToArray(apply_buff).AddToArray(fx)));

                    if (reuse_remaining_cost && ee.total_cost < max_cost)
                    {
                        var apply_remaining = Common.createContextActionApplyBuffToCaster(remaining_points_buffs[max_cost - ee.total_cost - 1],
                                                                                          Helpers.CreateContextDuration(1, DurationRate.Rounds),
                                                                                          dispellable: ability_type == AbilityType.Spell || ability_type == AbilityType.SpellLike,
                                                                                          is_from_spell: ability_type == AbilityType.Spell || ability_type == AbilityType.SpellLike);
                        ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(a.Actions.Actions.AddToArray(apply_remaining)));
                    }
                    ability.AddComponents(components);
                    foreach (var e in ee.required_evolutions)
                    {
                        if (!ee.base_evolutions.Contains(e))
                        {
                            ability.AddComponent(Helpers.Create<EvolutionMechanics.AbilityShowIfHasEvolution>(a => a.evolution = e));
                        }
                    }
                    foreach (var e in ee.conflicting_evolutions)
                    {
                        ability.AddComponent(Helpers.Create<EvolutionMechanics.AbilityShowIfHasEvolution>(a => {a.evolution = e; a.not = true; }));
                    }
                    if (ee.base_evolutions.Length > 0)
                    {
                        ability.AddComponent(Helpers.Create<EvolutionMechanics.AbilityShowIfHasEvolution>(a => { a.evolution = ee.base_evolutions[0]; a.not = true; }));
                    }
                    ability.AddComponent(Helpers.Create<EvolutionMechanics.AbilityShowIfHasEvolution>(a => { a.evolution = ee.evolution; a.not = true; }));
                    if (!ee.subtypes.Empty())
                    {
                        ability.AddComponent(Helpers.Create<NewMechanics.AbilityShowIfCasterHasFactsFromList>(a => a.UnitFacts = ee.subtypes));
                    }
                    ability.AddComponent(Helpers.Create<NewMechanics.AbilityShowIfHasFeatureRank>(a =>
                                                                                                      {
                                                                                                          a.Feature = summoner_rank;
                                                                                                          a.min_value = ee.summoner_level;
                                                                                                          a.max_value = 1000;
                                                                                                      }));
                    ability.setMiscAbilityParametersSelfOnly();
                    ability.AddComponent(Helpers.Create<SharedSpells.CannotBeShared>());
                    if (ability_type == AbilityType.Spell)
                    {
                        ability.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken;
                    }
                    ability.AddComponent(Helpers.Create<EvolutionMechanics.AbilityEvolutionCost>(a => a.cost = ee.total_cost));
                    abilities.Add(ability);
                }
            }
            var wrapper = Common.createVariantWrapper(name_prefix + "BaseAbility", "", abilities.ToArray());
            wrapper.SetName(display_name);
            wrapper.SetDescription(description);
            wrapper.SetIcon(icon);
            return wrapper;
        }


        public static BlueprintFeature[] getPermanenetEvolutions(Predicate<BlueprintFeature> evolution_filter)
        {
            List<BlueprintFeature> evolutions = new List<BlueprintFeature>();

            foreach (var ee in evolution_entries)
            {
                var pe = ee.permanent_evolution.GetComponent<EvolutionMechanics.AddPermanentEvolution>();
                if (evolution_filter(pe.Feature))
                {
                    evolutions.Add(ee.permanent_evolution);
                }
            }
            return evolutions.ToArray();
        }


        public static BlueprintFeature[] getPermanenetEvolutions2(Predicate<EvolutionEntry> evolution_filter)
        {
            List<BlueprintFeature> evolutions = new List<BlueprintFeature>();

            foreach (var ee in evolution_entries)
            {
                var pe = ee.permanent_evolution.GetComponent<EvolutionMechanics.AddPermanentEvolution>();
                if (evolution_filter(ee))
                {
                    evolutions.Add(ee.permanent_evolution);
                }
            }
            return evolutions.ToArray();
        }

        public static void initialize()
        {
            eidolon = Helpers.CreateFeature("BipedEidolonFeature",
                                            "Biped Eidolon",
                                            "",
                                            "",
                                            null,
                                            FeatureGroup.None);
            createEvolutions();
            createEvolutionEntries();
            createEvolutionSelection();
            createSelfEvolutionSelection();
            createExtraEvolutionFeat();

            //put all possible evolutions inside the list to simplify usage
            foreach (var ee in evolution_entries)
            {
                evolutions_list.Add(ee.evolution);
            }

            createEvolvedCompanion();
        }


        static void createEvolvedCompanion()
        {
            evolved_companion = Helpers.CreateFeatureSelection("EvolvedCompanionFeature",
                                                               "Evolved Companion",
                                                               "Select a 1-point evolution other than reach from those available to a summoner’s eidolon. Your animal companion gains this evolution. The animal companion must conform to any limitations of the evolution.\n"
                                                               + "Special: You can take this feat multiple times. Each time you do, select an additional 1-point evolution for your animal companion.",
                                                               "",
                                                               null,
                                                               FeatureGroup.Feat,
                                                               Helpers.Create<PrerequisitePet>(),
                                                               Helpers.PrerequisiteStatValue(StatType.Charisma, 13)
                                                               );

            evolved_companion.AllFeatures = Evolutions.getPermanenetEvolutions2(ee => ee.cost == 1 && (ee.evolution != reach) && ee.summoner_level == 0 && ee.base_evolutions.Empty());
            library.AddFeats(evolved_companion);
        }




        static void createExtraEvolutionFeat()
        {
            for (int i = 0; i < extra_evolution.Length; i++)
            {
                extra_evolution[i] = Helpers.CreateFeature($"ExtraEvolution{i + 1}Feature",
                                                           "Extra Evolution " + Common.roman_id[i + 1],
                                                           "Your eidolon’s evolution pool increases by 1.",
                                                           "",
                                                           null,
                                                           FeatureGroup.Feat,
                                                           Helpers.Create<EvolutionMechanics.IncreaseEvolutionPool>(p => p.amount = 1));
                if (i > 0)
                {
                    extra_evolution[i].AddComponent(Helpers.PrerequisiteFeature(extra_evolution[i - 1]));
                }
            }
            library.AddFeats(extra_evolution);
        }


        static public void addClassToExtraEvalution(BlueprintCharacterClass character_class, BlueprintArchetype archetype = null)
        {
            for (int i = 0; i < extra_evolution.Length; i++)
            {
                if (archetype != null)
                {
                    extra_evolution[i].AddComponent(Common.createPrerequisiteArchetypeLevel(character_class, archetype, i == 0 ? 1 : i*5, any: true));
                }
                else
                {
                    extra_evolution[i].AddComponent(Helpers.PrerequisiteClassLevel(character_class, i == 0 ? 1 : i * 5, any: true));
                }
            }
        }


        static void createSelfEvolutionSelection()
        {
            self_evolution_selection = Helpers.CreateFeatureSelection("SelfEvolutionFeatureSelection",
                                                                 "Personal Evolution Selection",
                                                                 "Summoner can divert a number of evolution points from her Eidolon and spend them to apply evolutions to herself.",
                                                                 "",
                                                                 null,
                                                                 FeatureGroup.None);
            Dictionary<String, List<BlueprintFeature>> evolutions_selections_map = new Dictionary<string, List<BlueprintFeature>>();
            foreach (var ee in evolution_entries)
            {
                if (!evolutions_selections_map.ContainsKey(ee.group))
                {
                    evolutions_selections_map.Add(ee.group, new List<BlueprintFeature>());
                }
                evolutions_selections_map[ee.group].Add(ee.self_selection_feature);
                ee.self_selection_feature.AddComponent(Helpers.Create<EvolutionMechanics.addSelfEvolutionSelection>(a => a.selection = self_evolution_selection));
            }

            foreach (var key in evolutions_selections_map.Keys)
            {
                if (key == "")
                {
                    self_evolution_selection.AllFeatures = self_evolution_selection.AllFeatures.AddToArray(evolutions_selections_map[key]);
                }
                else
                {
                    var key_evolution_selection = Helpers.CreateFeatureSelection(key.Replace(" ", "") + "SelfEvolutionFeatureSelection",
                                                                             key,
                                                                             evolutions_selections_map[key][0].Description,
                                                                             "",
                                                                             null,
                                                                             FeatureGroup.None);
                    key_evolution_selection.AllFeatures = evolutions_selections_map[key].ToArray();
                    key_evolution_selection.Obligatory = true;
                    self_evolution_selection.AllFeatures = self_evolution_selection.AllFeatures.AddToArray(key_evolution_selection);
                }
            }
        }


        static void createEvolutionSelection()
        {
            evolution_selection = Helpers.CreateFeatureSelection("EvolutionFeatureSelection",
                                                                 "Evolution Selection",
                                                                 "Summoner gains a pool of evalution points based on her class level, that can be used to give the primal comapnion or eidolon different abilities and powers. Whenever the summoner gains a level, he must decide how these points are spent, and they are set until he gains another level of summoner.",
                                                                 "",
                                                                 null,
                                                                 FeatureGroup.None);
            Dictionary<String, List<BlueprintFeature>> evolutions_selections_map = new Dictionary<string, List<BlueprintFeature>>();
            foreach (var ee in evolution_entries)
            {
                if (!evolutions_selections_map.ContainsKey(ee.group))
                {
                    evolutions_selections_map.Add(ee.group, new List<BlueprintFeature>());
                }
                evolutions_selections_map[ee.group].Add(ee.selection_feature);
                ee.selection_feature.AddComponent(Helpers.Create<EvolutionMechanics.addEvolutionSelection>(a => a.selection = evolution_selection));
            }

            foreach (var key in evolutions_selections_map.Keys)
            {
                if (key == "")
                {
                    evolution_selection.AllFeatures = evolution_selection.AllFeatures.AddToArray(evolutions_selections_map[key]);
                }
                else
                {
                    var key_evolution_selection = Helpers.CreateFeatureSelection(key.Replace(" ", "") + "EvolutionFeatureSelection",
                                                                             key,
                                                                             evolutions_selections_map[key][0].Description,
                                                                             "",
                                                                             null,
                                                                             FeatureGroup.None);
                    key_evolution_selection.AllFeatures = evolutions_selections_map[key].ToArray();
                    key_evolution_selection.Obligatory = true;
                    evolution_selection.AllFeatures = evolution_selection.AllFeatures.AddToArray(key_evolution_selection);
                }
            }
        }



        static void createEvolutionEntries()
        {
            var quadruped_eidolons = new BlueprintFeature[] { Eidolon.agathion_eidolon, Eidolon.air_quadruped_eidolon, Eidolon.fire_quadruped_eidolon, Eidolon.earth_quadruped_eidolon };
            var serpentine_eidolons = new BlueprintFeature[] { Eidolon.protean_eidolon, Eidolon.water_serpentine_eidolon };
            var biped_eidolons = new BlueprintFeature[] {Eidolon.angel_eidolon, Eidolon.azata_eidolon,
                                                   Eidolon.air_elemental_eidolon, Eidolon.earth_elemental_eidolon, Eidolon.fire_elemental_eidolon, Eidolon.water_elemental_eidolon,
                                                   Eidolon.fey_eidolon, Eidolon.inevitable_eidolon, Eidolon.fey_eidolon,
                                                   Eidolon.demon_eidolon, Eidolon.daemon_eidolon, Eidolon.devil_eidolon, Eidolon.infernal_eidolon,
                                                   Eidolon.twinned_eidolon, Eidolon.twinned_eidolon_small};
            var eidolons = biped_eidolons.AddToArray(serpentine_eidolons).AddToArray(quadruped_eidolons);
            var eidolons_with_hands = biped_eidolons.AddToArray(serpentine_eidolons);

            var devil_elemental = new BlueprintFeature[]{Eidolon.air_elemental_eidolon, Eidolon.earth_elemental_eidolon, Eidolon.fire_elemental_eidolon, Eidolon.water_elemental_eidolon, 
                                                         Eidolon.demon_eidolon, Eidolon.daemon_eidolon, Eidolon.devil_eidolon,
                                                         Eidolon.air_quadruped_eidolon, Eidolon.earth_quadruped_eidolon, Eidolon.fire_quadruped_eidolon, Eidolon.water_serpentine_eidolon };
            evolution_entries.Add(new EvolutionEntry(claws, 1, 0, new BlueprintFeature[0], new BlueprintFeature[0],
                                                     devil_elemental.AddToArray(new BlueprintFeature[] { Eidolon.infernal_eidolon, boar, dog, mammoth, monitor, wolf, wolf_ekun, Eidolon.agathion_eidolon, Eidolon.protean_eidolon})));
            evolution_entries.Add(new EvolutionEntry(slam_biped, 1, 0, new BlueprintFeature[0], new BlueprintFeature[0], biped_eidolons));

            foreach (var e in improved_natural_attacks)
            {
                evolution_entries.Add(new EvolutionEntry(e, 1, 0, new BlueprintFeature[0], new BlueprintFeature[0], new BlueprintFeature[0], 
                                                         evolution_group: "Improved Damage"));
            }

            evolution_entries.Add(new EvolutionEntry(bite, 1, 0, new BlueprintFeature[0], new BlueprintFeature[] {extra_off_hand_attack2 },
                                                     devil_elemental.AddToArray(new BlueprintFeature[] {boar, elk, mammoth, Eidolon.agathion_eidolon, Eidolon.protean_eidolon })));
            evolution_entries.Add(new EvolutionEntry(tail_slap, 1, 0, new BlueprintFeature[0], new BlueprintFeature[0], serpentine_eidolons.AddToArray(centipede)));

            smilodon.AddComponent(Helpers.Create<EvolutionMechanics.AddFakeEvolution>(a => a.Feature = bite));
            centipede.AddComponent(Helpers.Create<EvolutionMechanics.AddFakeEvolution>(a => a.Feature = bite));
            monitor.AddComponent(Helpers.Create<EvolutionMechanics.AddFakeEvolution>(a => a.Feature = bite));
            bear.AddComponent(Helpers.Create<EvolutionMechanics.AddFakeEvolution>(a => a.Feature = bite));

            for (int i = 0; i < improved_natural_armor.Length; i++)
            {
                bool is_last = (i + 1) == improved_natural_armor.Length;
                evolution_entries.Add(new EvolutionEntry(improved_natural_armor[i], 1,  i * 5, improved_natural_armor.Take(i).ToArray(),
                                                         new BlueprintFeature[0], new BlueprintFeature[0], evolution_group: "Improved Natural Armor"
                                                         )
                                                         );
            }

            evolution_entries.Add(new EvolutionEntry(reach, 1, 0, new BlueprintFeature[0], new BlueprintFeature[0], new BlueprintFeature[0]));

            foreach (var e in resistance)
            {
                evolution_entries.Add(new EvolutionEntry(e, 1, 0, new BlueprintFeature[0], new BlueprintFeature[0], new BlueprintFeature[0],
                                                         evolution_group: "Resistance"));
            }

            foreach (var e in skilled)
            {
                evolution_entries.Add(new EvolutionEntry(e, 1, 0, new BlueprintFeature[0], new BlueprintFeature[0], new BlueprintFeature[0],
                                                         evolution_group: "Skilled"));
            }

            evolution_entries.Add(new EvolutionEntry(wing_buffet, 1, 9, new BlueprintFeature[] {flight}, new BlueprintFeature[0],
                                         eidolons.AddToArray(animals).RemoveFromArray(Eidolon.infernal_eidolon)));

            for (int i = 0; i < ability_increase.Length; i++)
            {
                for (int j = 0; j < ability_increase[i].Length; j++)
                {
                    evolution_entries.Add(new EvolutionEntry(ability_increase[i][j], 2, j * 6, ability_increase[i].Take(j).ToArray(),
                                                             new BlueprintFeature[0], 
                                                             new BlueprintFeature[0],
                                                             evolution_group: "Ability Increase"
                                                             )
                                         );
                }
            }

            for (int i = 0; i < energy_attacks.Length; i++)
            {
                evolution_entries.Add(new EvolutionEntry(energy_attacks[i], 2, 5, new BlueprintFeature[0],
                                                         energy_attacks.RemoveFromArray(energy_attacks[i]), new BlueprintFeature[0],
                                                         evolution_group: "Energy Attacks")
                                                         );
            }


            for (int i = 0; i < shared_slots.Length; i++)
            {
                evolution_entries.Add(new EvolutionEntry(shared_slots[i], 2, 0, new BlueprintFeature[0],
                                                         new BlueprintFeature[0], new BlueprintFeature[] { Eidolon.twinned_eidolon, Eidolon.twinned_eidolon_small },
                                                         evolution_group: "Shared Slot")
                                                         );
            }

            evolution_entries.Add(new EvolutionEntry(flight, 2, 5, new BlueprintFeature[0], new BlueprintFeature[0], new BlueprintFeature[0]));
            evolution_entries.Add(new EvolutionEntry(gore, 2, 14, new BlueprintFeature[0], new BlueprintFeature[] {extra_attack2, extra_off_hand_attack2 },
                                                     devil_elemental.AddToArray(new BlueprintFeature[] {bear, dog, monitor, wolf, wolf_ekun, leopard, smilodon, centipede, Eidolon.agathion_eidolon })));

            foreach (var e in immunity)
            {
                evolution_entries.Add(new EvolutionEntry(e, 2, 7, new BlueprintFeature[0], new BlueprintFeature[0], new BlueprintFeature[0],
                                                         evolution_group: "Immunity"));
            }

            evolution_entries.Add(new EvolutionEntry(rake, 2, 4, new BlueprintFeature[0], new BlueprintFeature[0],
                                                     quadruped_eidolons.AddToArray(new BlueprintFeature[] { bear, dog, monitor, wolf, wolf_ekun, leopard, elk, mammoth, boar})));

            evolution_entries.Add(new EvolutionEntry(constrict, 2, 0, new BlueprintFeature[] { tail_slap }, new BlueprintFeature[0],
                                         serpentine_eidolons.AddToArray(centipede)));

            evolution_entries.Add(new EvolutionEntry(trip, 2, 4, new BlueprintFeature[] { bite }, new BlueprintFeature[0],
                                                     new BlueprintFeature[0]));

            for (int i = 0; i < weapon_training.Length; i++)
            {
                evolution_entries.Add(new EvolutionEntry(weapon_training[i], 2, 0, weapon_training.Take(i).ToArray(), new BlueprintFeature[0], biped_eidolons,
                                                         evolution_group: "Weapon Training",
                                                         upgradeable: i + 1 != weapon_training.Length,
                                                         next_level_total_cost: 4 + i*2,
                                                         previous_evolutions: weapon_training.Take(i).ToArray(),
                                                         full_cost: 2 + i*2
                                                         )
                                     );
            }

            evolution_entries.Add(new EvolutionEntry(blindsense, 3, 9, new BlueprintFeature[0], new BlueprintFeature[0],
                                                    new BlueprintFeature[0]));

            evolution_entries.Add(new EvolutionEntry(pounce, 3, 7, new BlueprintFeature[0], new BlueprintFeature[0],
                                        quadruped_eidolons.AddToArray(new BlueprintFeature[] { bear, dog, monitor, wolf, wolf_ekun, leopard, elk, mammoth, boar }))
                                        );
            evolution_entries.Add(new EvolutionEntry(amorphous, 4, 0, new BlueprintFeature[0], new BlueprintFeature[0],
                                        new BlueprintFeature[0]));

            evolution_entries.Add(new EvolutionEntry(blindsight, 4, 11, new BlueprintFeature[] {blindsense}, new BlueprintFeature[0],
                                        new BlueprintFeature[0]));

            for (int i = 0; i < fast_healing.Length; i++)
            {
                bool is_last = (i + 1) == fast_healing.Length;
                evolution_entries.Add(new EvolutionEntry(fast_healing[i], i == 0 ? 4 : 2, 11, fast_healing.Take(i).ToArray(), new BlueprintFeature[0], new BlueprintFeature[0],
                                                         evolution_group: "Fast Healing",
                                                         upgradeable: i + 1 != fast_healing.Length,
                                                         next_level_total_cost: 6 + 2 * i,
                                                         previous_evolutions: fast_healing.Take(i).ToArray(),
                                                         full_cost: 4 + i * 2
                                                         )
                                     );
            }

            evolution_entries.Add(new EvolutionEntry(size_increase[0], 4, 8, new BlueprintFeature[0], new BlueprintFeature[0],
                                                     eidolons.RemoveFromArray(Eidolon.twinned_eidolon_small), 
                                                     upgradeable: true,
                                                     next_level_total_cost: 10
                                                     ));
            evolution_entries.Add(new EvolutionEntry(size_increase[1], 6, 13, new BlueprintFeature[] { size_increase[0] }, new BlueprintFeature[0],
                                                     eidolons.RemoveFromArray(Eidolon.twinned_eidolon_small),
                                                     previous_evolutions: new BlueprintFeature[] { size_increase[0] },
                                                     full_cost: 10));


            var bw1 = new BlueprintFeature[breath_weapon.Length];
            for (int i = 0; i < breath_weapon.Length; i++)
            {
                bw1[i] = breath_weapon[i][0];
            }
            for (int i = 0; i < breath_weapon.Length; i++)
            {
                for (int j = 0; j < breath_weapon[i].Length; j++)
                {
                    evolution_entries.Add(new EvolutionEntry(breath_weapon[i][j], j == 0 ? 4 : 1, 9, breath_weapon[i].Take(j).ToArray(),
                                                             bw1.RemoveFromArray(breath_weapon[i][0]),
                                                             new BlueprintFeature[0],
                                                             evolution_group: "Breath Weapon",
                                                             upgradeable: j + 1 != breath_weapon[i].Length,
                                                             next_level_total_cost: 5 + j,
                                                             previous_evolutions: breath_weapon[i].Take(j).ToArray(),
                                                             full_cost: 4 + j
                                                             )
                                                             );
                }
            }

            evolution_entries.Add(new EvolutionEntry(damage_reduction, 3, 15, new BlueprintFeature[0], new BlueprintFeature[0],
                                         new BlueprintFeature[] {Eidolon.angel_eidolon, Eidolon.azata_eidolon, Eidolon.twinned_eidolon, Eidolon.twinned_eidolon_small,
                                                                   Eidolon.earth_elemental_eidolon, Eidolon.earth_quadruped_eidolon, Eidolon.protean_eidolon,
                                                                   Eidolon.fey_eidolon, Eidolon.inevitable_eidolon, Eidolon.infernal_eidolon,
                                                                   Eidolon.demon_eidolon, Eidolon.daemon_eidolon, Eidolon.devil_eidolon, Eidolon.agathion_eidolon}));

            evolution_entries.Add(new EvolutionEntry(poison_strength, 2, 7, new BlueprintFeature[] { bite }, new BlueprintFeature[] { poison_constitution },
                                         devil_elemental.AddToArray(Eidolon.protean_eidolon)
                                         ));
            evolution_entries.Add(new EvolutionEntry(poison_constitution, 4, 7, new BlueprintFeature[] { bite }, new BlueprintFeature[] { poison_strength },
                                         devil_elemental.AddToArray(Eidolon.protean_eidolon)
                                         ));

            evolution_entries.Add(new EvolutionEntry(extra_attack, 2, 0, new BlueprintFeature[0], new BlueprintFeature[0], biped_eidolons));
            evolution_entries.Add(new EvolutionEntry(extra_attack_serpentine, 2, 14, new BlueprintFeature[0], new BlueprintFeature[0], serpentine_eidolons));
            evolution_entries.Add(new EvolutionEntry(extra_attack2, 2, 14, new BlueprintFeature[] { extra_attack }, new BlueprintFeature[] {gore}, biped_eidolons.RemoveFromArray(Eidolon.infernal_eidolon)));
            evolution_entries.Add(new EvolutionEntry(extra_off_hand_attack, 1, 0, new BlueprintFeature[] {extra_attack }, new BlueprintFeature[0], biped_eidolons.RemoveFromArray(Eidolon.infernal_eidolon)));
            evolution_entries.Add(new EvolutionEntry(extra_off_hand_attack_serpentine, 1, 14, new BlueprintFeature[0], new BlueprintFeature[0], serpentine_eidolons));
            evolution_entries.Add(new EvolutionEntry(extra_off_hand_attack2, 1, 14, new BlueprintFeature[] { extra_attack2, extra_off_hand_attack }, new BlueprintFeature[] { gore, bite }, biped_eidolons.RemoveFromArray(Eidolon.infernal_eidolon)));
        }

        static void createEvolutions()
        {
            createClaws();
            createSlam();
            createImprovedNaturalAttacks();
            createBite();
            createTailSlap();
            createImprovedNaturalArmor();
            createReach();
            createResistance();            
            createSkilled();
            createWingBuffet();
            createExtraAttacks();

            createAbilityIncrease();
            createEnergyAttacks();
            createFlight();
            createGore();
            createImmunity();
            createRake();
            createConstrict();
            createTrip();
            createWeaponTraining();

            createBlindsense();
            createPounce();

            createAmorphous();
            createBlindsight();
            createFastHealing();
            createSpellResistance();
            createSizeIncrease();
            createBreathWeapon();
            createDamageReduction();
            createPoison();
            createSharedSlots();
        }


        static void createSharedSlots()
        {
            shared_slots = new BlueprintFeature[9];
            shared_slots[0] = createShareSlot<BlueprintItemEquipmentHead>("Head", Helpers.GetIcon("3d33605336c62274483f382f58908f5b")); //headband of wisdom
            shared_slots[1] = createShareSlot<BlueprintItemEquipmentNeck>("Neck", Helpers.GetIcon("11f435140501db84e8e787bf8792fac2")); //amulet of nat armor + 5
            shared_slots[2] = createShareSlot<BlueprintItemEquipmentShoulders>("Shoulders", Helpers.GetIcon("a34cd0f80d04ec647af741d924a3e2a3")); //cloak of resistance
            shared_slots[3] = createShareSlot<BlueprintItemEquipmentWrist>("Wrist", Helpers.GetIcon("bade8137f5101834c849c277ba0301a1")); //bracers of armor
            shared_slots[4] = createShareSlot<BlueprintItemEquipmentGloves>("Gloves", Helpers.GetIcon("6555965e6540c3b48b9a352214ecba41"));//glvoes of manticore
            shared_slots[5] = createShareSlot<BlueprintItemEquipmentFeet>("Feet", Helpers.GetIcon("815cc85ce13ab64428253aea3b6708a8"));//boots of the light step
            shared_slots[6] = createShareSlot<BlueprintItemEquipmentRing>("Ring I", Helpers.GetIcon("31315100c28e6a2418396fb152466fcd"), 1);//ring of protection +3
            shared_slots[7] = createShareSlot<BlueprintItemEquipmentRing>("Ring II", Helpers.GetIcon("2d576daea5f62ae489028bce40469285"), 2);//ring of protection +5
            shared_slots[8] = createShareSlot<BlueprintItemEquipmentBelt>("Belt", Helpers.GetIcon("d8062d43606e89e4bb3f0483ed3aba5d"));//belt of strength +6
        }

        static BlueprintFeature createShareSlot<T>(string display_name, UnityEngine.Sprite icon, int slot_id = 0)
        {
            var feature = Helpers.CreateFeature("Share" + typeof(T).ToString() + $"{slot_id}Feature",
                                                "Share Slot: " + display_name,
                                                "Select a magic item slot. A magic item worn by the eidolon in that slot remains active even if the summoner is also wearing a magic item in that slot. This evolution can be selected more than once. Each time an eidolon selects this evolution, it applies to a new slot.",
                                                "",
                                                icon,
                                                FeatureGroup.None,
                                                Helpers.Create<CompanionMechanics.SharedSlotComponent>(s => { s.slot_type = typeof(T); s.slot_id = slot_id; })
                                                );
            return feature;
        }


        static void createDamageReduction()
        {
            var icon = Helpers.GetIcon("9e1ad5d6f87d19e4d8883d63a6e35568"); //mage armor

            damage_reduction = Helpers.CreateFeature("DamageReductionEvolutionFeature",
                                         "Damage Reduction",
                                         "The eidolon’s body becomes more resistant to harm. Increase the damage reduction granted by the eidolon’s subtype by 5.",
                                         "",
                                         icon,
                                         FeatureGroup.None
                                         );
        }


        static void createWingBuffet()
        {
            var icon = Helpers.GetIcon("13143852b74718144ac4267b949615f0"); //righteous might
            var wing1d4 = library.Get<BlueprintItemWeapon>("864e29d3e07ad4a4f96d576b366b4a86");

            wing_buffet = Helpers.CreateFeature("WingBuffetEvolutionFeature",
                                         "Wing Buffet",
                                         "The eidolon learns to use its wings to batter foes, granting it two wing buffet attacks. These attacks are secondary attacks. The wing buffets deal 1d4 points of damage (1d6 if Large, 1d8 if Huge).",
                                         "",
                                         icon,
                                         FeatureGroup.None,
                                         Common.createAddSecondaryAttacks(wing1d4, wing1d4)
                                         );
        }


        static void createExtraAttacks()
        {
            extra_attack = Helpers.CreateFeature("ExtraAttackEvolutionFeature",
                                                "Extra Attack",
                                                "Eidolon can make one more primary hand attack.",
                                                "",
                                                Helpers.GetIcon("9d5d2d3ffdd73c648af3eb3e585b1113"),//divine favor
                                                FeatureGroup.None,
                                                Helpers.Create<BuffExtraAttack>(b => { b.Number = 1; b.Haste = false; })
                                                );

            extra_attack2 = library.CopyAndAdd(extra_attack, "ExtraAttackEvolutionIIFeature", "");
            extra_attack2.SetName("Extra Attack II");
            extra_attack_serpentine = library.CopyAndAdd(extra_attack, "ExtraAttackEvolutionSerpentineFeature","");

            extra_off_hand_attack = Helpers.CreateFeature("ExtraSecondaryAttackEvolutionFeature",
                                                         "Extra Off-Hand Attack",
                                                         "Eidolon can make one more secondary hand attack.",
                                                         "",
                                                         Helpers.GetIcon("c35eb3e0093960d4998522be47a1bca6"),//judgment smiting
                                                         FeatureGroup.None,
                                                         Helpers.Create<NewMechanics.BuffExtraOffHandAttack>(b => { b.Number = 1; })
                                                         );

            extra_off_hand_attack2 = library.CopyAndAdd(extra_off_hand_attack, "ExtraSecondaryAttackEvolutionIIFeature", "");
            extra_off_hand_attack2.SetName("Extra Off-Hand Attack II");
            extra_off_hand_attack_serpentine = library.CopyAndAdd(extra_off_hand_attack, "ExtraOffHandAttackEvolutionSerpentineFeature", "");
        }


        static void createImprovedNaturalAttacks()
        {
            var icon = library.Get<BlueprintAbility>("403cf599412299a4f9d5d925c7b9fb33").Icon; //magic fang
            string[] names = { "Bite", "Claw", "Gore", "Other Natural Weapons" };
            WeaponCategory[] categories = new WeaponCategory[] { WeaponCategory.Bite, WeaponCategory.Claw, WeaponCategory.Gore, WeaponCategory.OtherNaturalWeapons };
            improved_natural_attacks = new BlueprintFeature[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                improved_natural_attacks[i] = Helpers.CreateFeature(categories[i].ToString() + "ImprovedNaturalAttacksEvolutionFeature",
                                                                    "Improved Damage: " + names[i],
                                                                    "One of the eidolon’s natural attacks is particularly deadly. Select one natural attack form and increase the damage die by one step. This evolution can be selected more than once. Its effects do not stack. Each time the eidolon selects this evolution, it applies to a different natural attack.",
                                                                    "",
                                                                    icon,
                                                                    FeatureGroup.None,
                                                                    Helpers.Create<NewMechanics.WeaponCategorySizeChange>(a => a.category = categories[i])
                                                                    );
            }
        }





        static void createBite()
        {
            var icon = library.Get<BlueprintAbility>("75de4ded3e731dc4f84d978fe947dc67").Icon; //acid maw
            var bite1d6 = library.Get<BlueprintItemWeapon>("a000716f88c969c499a535dadcf09286");

            bite = Helpers.CreateFeature("BiteEvolutionFeature",
                                         "Bite",
                                         "The eidolon’s maw is full of razor-sharp teeth, giving it a bite attack. This attack is a primary attack. The bite deals 1d6 points of damage (1d8 if Large, 2d6 if Huge).",
                                         "",
                                         icon,
                                         FeatureGroup.None,
                                         Helpers.Create<AddAdditionalLimb>(a => a.Weapon = bite1d6)
                                         );
        }


        static void createTailSlap()
        {
            var tail1d6 = library.Get<BlueprintItemWeapon>("b21cd5b03fbb0f542815580e66f85915");

            tail_slap = Helpers.CreateFeature("TailSlapEvolutionFeature",
                                         "Tail Slap",
                                         "The eidolon can use its tail to bash nearby foes, granting it a tail slap attack. This attack is a secondary attack. The tail slap deals 1d6 points of damage (1d8 if Large, 2d6 if Huge).",
                                         "",
                                         tail1d6.Icon,
                                         FeatureGroup.None,
                                         Helpers.Create<AddSecondaryAttacks>(a => a.Weapon = new BlueprintItemWeapon[] { tail1d6 })
                                         );
        }


        static void createConstrict()
        {
            var icon = library.Get<BlueprintAbility>("3fce8e988a51a2a4ea366324d6153001").Icon; //constricting coils
            var tail1d6 = library.Get<BlueprintItemWeapon>("b21cd5b03fbb0f542815580e66f85915");

            constrict = Helpers.CreateFeature("ConstrictEvolutionFeature",
                                         "Constrict",
                                         $"The eidolon gains powerful muscles that allow it to crush those it grapples. It can make one additional secondary attack with its tail when making a full attack.",
                                         "",
                                         icon,
                                         FeatureGroup.None);


            constrict.AddComponent(Common.createAddSecondaryAttacks(tail1d6));                            
        }


        static void createClaws()
        {
            var icon = Helpers.GetIcon("f68af48f9ebf32549b5f9fdc4edfd475"); //claws          
            var claw1d4 = library.Get<BlueprintItemWeapon>("118fdd03e569a66459ab01a20af6811a");

            var claws_biped = Helpers.CreateFeature("ClawsBipedEvolutionFeature",
                             "Claws",
                             "The eidolon has a pair of vicious claws at the ends of its limbs, giving it two claw attacks. These attacks are primary attacks. The claws deal 1d4 points of damage (1d6 if Large, 1d8 if Huge).",
                             "",
                             icon,
                             FeatureGroup.None,
                             Common.createEmptyHandWeaponOverride(claw1d4)
                             );
            claws_biped.HideInCharacterSheetAndLevelUp = true;
            var claws_quadruped = Helpers.CreateFeature("ClawsQuadrupedEvolutionFeature",
                             "Claws",
                             "The eidolon has a pair of vicious claws at the ends of its limbs, giving it two claw attacks. These attacks are primary attacks. The claws deal 1d4 points of damage (1d6 if Large, 1d8 if Huge).",
                             "",
                             icon,
                             FeatureGroup.None,
                             Helpers.Create<AddAdditionalLimb>(a => a.Weapon = claw1d4),
                             Helpers.Create<AddAdditionalLimb>(a => a.Weapon = claw1d4)
                             );
            claws_quadruped.HideInCharacterSheetAndLevelUp = true;
            claws = Helpers.CreateFeature("ClawsEvolutionFeature",
                                         "Claws",
                                         "The eidolon has a pair of vicious claws at the ends of its limbs, giving it two claw attacks. These attacks are primary attacks. The claws deal 1d4 points of damage (1d6 if Large, 1d8 if Huge).",
                                         "",
                                         icon,
                                         FeatureGroup.None,
                                         Helpers.Create<NewMechanics.AddFeatureIfQuadrupedOrSerpentine>(a => a.Feature = claws_quadruped),
                                         Helpers.Create<NewMechanics.AddFeatureIfQuadrupedOrSerpentine>(a => { a.Feature = claws_biped; a.not = true; })
                                         );
        }


        static void createSlam()
        {
            var icon = Helpers.GetIcon("247a4068296e8be42890143f451b4b45"); //basic feat
            var slam1d8 = library.Get<BlueprintItemWeapon>("5ea80d97dcfc81f46a1b9b2f256340f2");

            slam_biped = Helpers.CreateFeature("SlamBipedEvolutionFeature",
                                         "Slam",
                                         "The eidolon can deliver a devastating slam attack. This attack is a primary attack. The slam deals 1d8 points of damage (2d6 if Large, 2d8 if Huge). The eidolon must have the limbs (arms) evolution to take this evolution. Alternatively, the eidolon can replace the claws from its base form with this slam attack (this still costs 1 evolution point).",
                                         "",
                                         icon,
                                         FeatureGroup.None,
                                         Common.createEmptyHandWeaponOverride(slam1d8)
                                         );
        }


        static void createImprovedNaturalArmor()
        {
            var icon = library.Get<BlueprintAbility>("7bc8e27cba24f0e43ae64ed201ad5785").Icon; //resistance
            improved_natural_armor = new BlueprintFeature[5];
            for (int i = 0; i < improved_natural_armor.Length; i++)
            {
                improved_natural_armor[i] = Helpers.CreateFeature($"ImprovedNaturalArmor{i + 1}EvolutionFeature",
                                                                  $"Improved Natural Armor" + (i == 0 ? "" : $" {Common.roman_id[i + 1]}"),
                                                                   "The eidolon’s hide grows thick fur, rigid scales, or bony plates, giving it a +2 bonus to its natural armor. For every 5 levels the summoner possesses, summoner can spend 2 additional evolution points to increase armor bonus by 2.",
                                                                   "",
                                                                   icon,
                                                                   FeatureGroup.None
                                                                   );                                                                  
            }
            improved_natural_armor[0].AddComponents(Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.NaturalArmor, multiplier: 2),
                                                    Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureList, featureList: improved_natural_armor),
                                                    Helpers.Create<RecalculateOnFactsChange>(r => r.CheckedFacts = improved_natural_armor)
                                                    );
        }


        static void createReach()
        {
            var icon = library.Get<BlueprintAbility>("93d9d74dac46b9b458d4d2ea7f4b1911").Icon; //polymorph         
            reach = Helpers.CreateFeature("ReachEvolutionFeature",
                                         "Reach",
                                         "Eidolon’s attacks are capable of striking foes at a distance. The eidolon’s reach increases by 5 feet.",
                                         "",
                                         icon,
                                         FeatureGroup.None,
                                         Helpers.CreateAddStatBonus(StatType.Reach, 4, ModifierDescriptor.Enhancement)
                                         );
        }


        static void createResistance()
        {
            var energies = new DamageEnergyType[] { DamageEnergyType.Acid, DamageEnergyType.Cold, DamageEnergyType.Electricity, DamageEnergyType.Fire };
            var icon_ids = new string[] { "fedc77de9b7aad54ebcc43b4daf8decd", "5368cecec375e1845ae07f48cdc09dd1", "90987584f54ab7a459c56c2d2f22cee2", "ddfb4ac970225f34dbff98a10a4a8844" };

            resistance = new BlueprintFeature[energies.Length];

            for (int i = 0; i < resistance.Length; i++)
            {
                resistance[i] = Helpers.CreateFeature(energies[i].ToString() + "ResistanceEvolutionFeature",
                                                      "Resistance: " + energies[i].ToString(),
                                                      "The eidolon’s form takes on a resiliency to one particular energy type, which is usually reflected in its physical body (ashen hide for fire, icy breath for cold, and so on). Select one energy type (acid, cold, electricity or fire). The eidolon gains resistance 5 against that energy type. This resistance increases by 5 for every 5 levels the summoner possesses, to a maximum of 15 at 10th level. This evolution can be selected more than once. Its effects do not stack. Each time the eidolon selects this evolution, it applies to a different energy type.",
                                                      "",
                                                      Helpers.GetIcon(icon_ids[i]),
                                                      FeatureGroup.None,
                                                      Common.createEnergyDRContextRank(energies[i], multiplier: 5),
                                                      Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueTypeExtender.MasterFeatureRank.ToContextRankBaseValueType(),
                                                                                      feature: summoner_rank,
                                                                                      type: AbilityRankType.StatBonus,
                                                                                      progression: ContextRankProgression.OnePlusDivStep,
                                                                                      stepLevel: 5, max: 3)
                                                      );
            }
        }


        static void createImmunity()
        {
            var energies = new DamageEnergyType[] { DamageEnergyType.Acid, DamageEnergyType.Cold, DamageEnergyType.Electricity, DamageEnergyType.Fire };
            var icon_ids = new string[] { "3d77ee3fc4913c44b9df7c5bbcdc4906", "021d39c8e0eec384ba69140f4875e166", "e24ce0c3e8eaaaf498d3656b534093df", "3f9605134d34e1243b096e1f6cb4c148" };

            immunity = new BlueprintFeature[energies.Length];

            for (int i = 0; i < immunity.Length; i++)
            {
                immunity[i] = Helpers.CreateFeature(energies[i].ToString() + "ImmunityEvolutionFeature",
                                                      "Immunity: " + energies[i].ToString(),
                                                      "he eidolon’s body becomes extremely resilient to one energy type. Select one energy type: acid, cold, electricity or fire. The eidolon gains immunity to that energy type. This evolution can be selected more than once. Its effects do not stack. Each time it applies to a different energy type. ",
                                                      "",
                                                      Helpers.GetIcon(icon_ids[i]),
                                                      FeatureGroup.None,
                                                      Common.createAddEnergyDamageImmunity(energies[i])
                                                      );
            }
        }


        static void createSkilled()
        {
            var skill_foci = library.Get<BlueprintFeatureSelection>("c9629ef9eebb88b479b2fbc5e836656a").AllFeatures;
            skilled = new BlueprintFeature[skill_foci.Length];
            for (int i = 0; i < skill_foci.Length; i ++)
            {
                StatType stat = skill_foci[i].GetComponent<AddContextStatBonus>().Stat;
                string name = LocalizedTexts.Instance.Stats.GetText(stat);

                skilled[i] = Helpers.CreateFeature(stat.ToString() + "SkilledEvolutionFeature",
                                                   "Skilled: " + name,
                                                   "The eidolon becomes especially adept at a specific skill, gaining a +6 racial bonus on that skill. This evolution can be selected more than once. Its effects do not stack. Each time the eidolon selects this evolution, it applies to a different skill.",
                                                   "",
                                                   skill_foci[i].Icon,
                                                   FeatureGroup.None,
                                                   Helpers.CreateAddStatBonus(stat, 6, ModifierDescriptor.Racial)
                                                   );
            }

        }


        static void createAbilityIncrease()
        {
            var icon_ids = new string[] { "4c3d08935262b6544ae97599b3a9556d","de7a025d48ad5da4991e7d3c682cf69d", "a900628aea19aa74aad0ece0e65d091a", "ae4d3ad6a8fda1542acf2e9bbc13d113",
                                          "f0455c9295b53904f9e02fc571dd2ce1", "446f7bf201dc1934f96ac0a26e324803"};
            var stats = new StatType[] { StatType.Strength, StatType.Dexterity, StatType.Constitution, StatType.Intelligence, StatType.Wisdom, StatType.Charisma };

            ability_increase = new BlueprintFeature[stats.Length][];
            for (int j = 0; j < stats.Length; j++)
            {
                
                ability_increase[j] = new BlueprintFeature[4];
                for (int i = 0; i < 4; i++)
                {
                    int bonus = (i + 1) * 2;
                    ability_increase[j][i] = Helpers.CreateFeature(stats[j].ToString() + bonus.ToString() + "AbilityIncreaseFeature",
                                                                   "Ability Increase: " + stats[j].ToString() + (i == 0 ? "" : $" {Common.roman_id[i+1]}"),
                                                                   "The eidolon grows larger muscles, gains faster reflexes, achieves greater intelligence, or acquires another increase to one of its abilities. Increase one of the eidolon’s ability scores by 2. This evolution can be selected more than once. It can be applied only once to an individual ability score. For every 6 levels summoner can spend 2 additional evolutions points to increase bonus by 2.",
                                                                   "",
                                                                   Helpers.GetIcon(icon_ids[j]),
                                                                   FeatureGroup.None
                                                                   );
                }
                ability_increase[j][0].AddComponents(Helpers.CreateAddContextStatBonus(stats[j], ModifierDescriptor.Feat, multiplier: 2),
                                                     Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureList,
                                                                                   featureList: ability_increase[j]),
                                                     Helpers.Create<RecalculateOnFactsChange>(r => r.CheckedFacts = ability_increase[j])
                                                    );
            }
        }


        static void createEnergyAttacks()
        {
            var energies = new DamageEnergyType[] { DamageEnergyType.Acid, DamageEnergyType.Cold, DamageEnergyType.Electricity, DamageEnergyType.Fire };
            var icon_ids = new string[] { "97d0a51ca60053047afb9aca900fb71b", "83ed16546af22bb43bd08734a8b51941", "728b3daffb1d9fd45958c6e60876b7a9",  "4783c3709a74a794dbe7c8e7e0b1b038" };

            energy_attacks = new BlueprintFeature[energies.Length];

            for (int i = 0; i < energy_attacks.Length; i++)
            {
                var effect = Common.createAddWeaponEnergyDamageDiceBuff(Helpers.CreateContextDiceValue(DiceType.D6, 1, 0),
                                                                                                        energies[i],
                                                                                                        AttackType.Melee, AttackType.Touch);
                effect.categories = new WeaponCategory[] { WeaponCategory.Claw, WeaponCategory.Bite, WeaponCategory.Gore, WeaponCategory.OtherNaturalWeapons, WeaponCategory.UnarmedStrike };
                energy_attacks[i] = Helpers.CreateFeature(energies[i].ToString() + "EnergyAttacksEvolutionFeature",
                                                      "Energy Attacks: " + energies[i].ToString(),
                                                      "The eidolon’s attacks become charged with energy. Select one energy type: acid, cold, electricity, or fire. All of the eidolon’s natural attacks deal 1d6 points of energy damage of the chosen type on a successful hit. Requirements: Summoner level 5th.",
                                                      "",
                                                      Helpers.GetIcon(icon_ids[i]),
                                                      FeatureGroup.None,
                                                      effect
                                                      );
            }
        }


        static void createGore()
        {
            var icon = library.Get<BlueprintProgression>("e76a774cacfb092498177e6ca706064d").Icon; //infernal bloodline
            var gore1d6 = library.Get<BlueprintItemWeapon>("76ada2578e9121a44b8ffbb7c1f2b5f0");

            gore = Helpers.CreateFeature("GoreEvolutionFeature",
                                         "Gore",
                                         "The eidolon grows a number of horns on its head, giving it a gore attack. This attack is a primary attack. The gore deals 1d6 points of damage (1d8 if Large, 2d6 if Huge).",
                                         "",
                                         icon,
                                         FeatureGroup.None,
                                         Helpers.Create<AddAdditionalLimb>(a => a.Weapon = gore1d6)
                                         );
        }


        static void createFlight()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/Fly.png");
            var silver_dragon_wings = library.Get<BlueprintActivatableAbility>("7679910a16368cc43b496cef2babe1cb");
            var angel_wings = library.Get<BlueprintActivatableAbility>("13143852b74718144ac4267b949615f0");
            var devil_wings = library.Get<BlueprintActivatableAbility>("9ae14c50ef7a28e468b585c673b5c48f");

            var flight_devil = Helpers.CreateFeature("FlightDevilEvolutionFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Common.createAuraFeatureComponent(devil_wings.Buff)
                                                   );
            flight_devil.HideInCharacterSheetAndLevelUp = true;
            var flight_angel = Helpers.CreateFeature("FlightAngelEvolutionFeature",
                                       "",
                                       "",
                                       "",
                                       null,
                                       FeatureGroup.None,
                                       Common.createAuraFeatureComponent(angel_wings.Buff)
                                       );
            flight_angel.HideInCharacterSheetAndLevelUp = true;
            var flight_dragon = Helpers.CreateFeature("FlightDragonEvolutionFeature",
                                                       "",
                                                       "",
                                                       "",
                                                       null,
                                                       FeatureGroup.None,
                                                       Common.createAuraFeatureComponent(silver_dragon_wings.Buff)
                                                       );
            flight_dragon.HideInCharacterSheetAndLevelUp = true;


            flight = Helpers.CreateFeature("FlightEvolutionFeature",
                                           "Flight",
                                           "The eidolon grows large wings, like those of a bat, bird, insect, or dragon, gaining the ability to fly.",
                                           "",
                                           icon,
                                           FeatureGroup.None,
                                           Helpers.Create<NewMechanics.AddFeatureIfMasterHasFactsFromList>(a => { a.Feature = flight_angel; a.CheckedFacts = new BlueprintUnitFact[] { Eidolon.angel_eidolon, Eidolon.azata_eidolon, Eidolon.agathion_eidolon }; }),
                                           Helpers.Create<NewMechanics.AddFeatureIfMasterHasFactsFromList>(a => { a.Feature = flight_devil; a.CheckedFacts = new BlueprintUnitFact[] { Eidolon.demon_eidolon, Eidolon.daemon_eidolon, Eidolon.devil_eidolon, Eidolon.infernal_eidolon }; }),
                                           Helpers.Create<NewMechanics.AddFeatureIfMasterHasFactsFromList>(a => { a.Feature = flight_dragon; a.CheckedFacts = new BlueprintUnitFact[] { Eidolon.angel_eidolon, Eidolon.azata_eidolon, Eidolon.agathion_eidolon, Eidolon.demon_eidolon, Eidolon.daemon_eidolon, Eidolon.devil_eidolon, Eidolon.infernal_eidolon }; a.not = true; })
                                           );
        }


        static void createRake()
        {
            var icon = Helpers.GetIcon("120e51788082260498a961a38a4fa617"); //dragon calws
            var claw1d4 = library.Get<BlueprintItemWeapon>("118fdd03e569a66459ab01a20af6811a");

            rake = Helpers.CreateFeature("RakeEvolutionFeature",
                                         "Rake",
                                         $"The eidolon grows dangerous claws on its feet, allowing it to make two rake attacks against its foes. These attacks are {(Main.settings.secondary_rake_attacks ? "secondary" : "primary")} attacks. These rake attacks deal 1d4 points of damage (1d6 if Large, 1d8 if Huge).",
                                         "",
                                         icon,
                                         FeatureGroup.None);
            if (Main.settings.secondary_rake_attacks)
            {
                rake.AddComponents(Helpers.Create<AddSecondaryAttacks>(a => a.Weapon = new BlueprintItemWeapon[] { claw1d4, claw1d4 }));
            }
            else
            {
                rake.AddComponents(Helpers.Create<AddAdditionalLimb>(a => a.Weapon = claw1d4),
                                  Helpers.Create<AddAdditionalLimb>(a => a.Weapon = claw1d4));
            }
        }


        static void createTrip()
        {
            var icon = Helpers.GetIcon("95851f6e85fe87d4190675db0419d112"); //grease
            
            trip = Helpers.CreateFeature("TripEvolutionFeature",
                             "Trip",
                             "The eidolon becomes adept at knocking foes to the ground with its bite, granting it a trip attack. Whenever the eidolon makes a successful bite attack, it can attempt a free combat maneuver check. If the eidolon succeeds at this check, the target is knocked prone. If the eidolon fails, it is not tripped in return.",
                             "",
                             icon,
                             FeatureGroup.None,
                             Helpers.Create<ManeuverOnAttack>(m => { m.Category = WeaponCategory.Bite; m.Maneuver = Kingmaker.RuleSystem.Rules.CombatManeuver.Trip; })
                             );
        }


        static void createWeaponTraining()
        {
            var simple_wp = library.Get<BlueprintFeature>("e70ecf1ed95ca2f40b754f1adb22bbdd");
            var martial_wp = library.Get<BlueprintFeature>("203992ef5b35c864390b4e4a1e200629");

            weapon_training = new BlueprintFeature[2];
            weapon_training[0] = Helpers.CreateFeature("WeponTraining1Evolution",
                                                     "Weapon Training I",
                                                     "The eidolon learns to use a weapon, gaining Simple Weapon Proficiency as a bonus feat. If 2 additional evolution points are spent, it gains proficiency with all martial weapons as well.",
                                                     "",
                                                     simple_wp.Icon,
                                                     FeatureGroup.None,
                                                     Helpers.CreateAddFacts(simple_wp)
                                                     );
            weapon_training[1] = Helpers.CreateFeature("WeponTraining2Evolution",
                                         "Weapon Training II",
                                         "The eidolon learns to use a weapon, gaining Simple Weapon Proficiency as a bonus feat. If 2 additional evolution points are spent, it gains proficiency with all martial weapons as well.",
                                         "",
                                         martial_wp.Icon,
                                         FeatureGroup.None,
                                         Helpers.CreateAddFacts(martial_wp)
                                         );
        }


        static void createBlindsense()
        {
            var icon = Helpers.GetIcon("6e668702fdc53c343a0363813683346e");

            blindsense = Helpers.CreateFeature("BlindsenseEvolutionFeature",
                                               "Blindsense",
                                               "The eidolon’s senses become incredibly acute, giving it blindsense with a range of 30 feet. This ability allows the eidolon to pinpoint the location of creatures that it can’t see without having to attempt a Perception check, but such creatures still have total concealment from the eidolon. Visibility still affects the eidolon’s movement, and it is still denied its Dexterity bonus to Armor Class against attacks from creatures it cannot see.",
                                               "",
                                               icon,
                                               FeatureGroup.None,
                                               Common.createBlindsense(30)
                                              );
        }


        static void createPounce()
        {
            var icon = Helpers.GetIcon("c78506dd0e14f7c45a599990e4e65038"); //charge

            pounce = Helpers.CreateFeature("PounceEvolutionFeature",
                                           "Pounce",
                                           "The eidolon gains quick reflexes, allowing it to make a full attack after a charge.",
                                           "",
                                           icon,
                                           FeatureGroup.None,
                                           Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.Pounce)
                                           );
        }


        static void createAmorphous()
        {
            var icon = Helpers.GetIcon("3e4ab69ada402d145a5e0ad3ad4b8564"); //mirror image

            amorphous = Helpers.CreateFeature("AmorphousEvolutionFeature",
                                           "Amorphous",
                                           "The eidolon’s biology lacks discernible weak points. It is not subject to critical hits and sneak attacks. ",
                                           "",
                                           icon,
                                           FeatureGroup.None,
                                           Helpers.Create<AddImmunityToCriticalHits>(),
                                           Helpers.Create<AddImmunityToPrecisionDamage>()
                                           );
        }


        static void createBlindsight()
        {
            var icon = Helpers.GetIcon("30e5dc243f937fc4b95d2f8f4e1b7ff3"); //see invisibility

            blindsight = Helpers.CreateFeature("BlindsightEvolutionFeature",
                                               "Blindsight",
                                               "The eidolon’s senses sharpen even further, granting it blindsight with a range of 30 feet. The eidolon can maneuver and attack as normal, ignoring darkness, invisibility, and most forms of concealment as long as it has line of effect to the target.",
                                               "",
                                               icon,
                                               FeatureGroup.None,
                                               Common.createBlindsight(30),
                                               Common.createBuffDescriptorImmunity(SpellDescriptor.GazeAttack),
                                               Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.GazeAttack)
                                              );
        }


        static void createFastHealing()
        {
            var icon = Helpers.GetIcon("4093d5a0eb5cae94e909eb1e0e1a6b36"); //remove disiease

            fast_healing = new BlueprintFeature[5];
            var buff = Helpers.CreateBuff("FastHealingEvolutionBuff",
                                        "Fast Healing",
                                        "The eidolon’s body gains the ability to heal wounds very quickly, giving it fast healing 1. The eidolon heals 1 point of damage per round, just like via natural healing. Fast healing does not restore hit points lost due to starvation, thirst, or suffocation, nor does it allow the eidolon to regrow lost body parts (or to reattach severed parts). Fast healing functions as long as the eidolon is alive. This fast healing does not function when the eidolon is not on the same plane as its summoner. This healing can be increased by 1 point per round for every 2 additional evolution points spent (to a maximum of 5 points per round).",
                                        "",
                                        icon,
                                        null,
                                        Common.createAddContextEffectFastHealing(Helpers.CreateContextValue(AbilityRankType.Default))
                                        );

            for (int i = 0; i < fast_healing.Length; i++)
            {
                fast_healing[i] = Helpers.CreateFeature($"FastHealing{i + 1}EvolutionFeature",
                                                        "Fast Healing" + (i == 0 ? "" : $" {Common.roman_id[i+1]}"),
                                                        buff.Description,
                                                        "",
                                                        icon,
                                                        FeatureGroup.None
                                                        );
            }
            fast_healing[0].AddComponent(Common.createAuraFeatureComponent(buff));
            buff.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureList, featureList: fast_healing));
        }


        static void createSpellResistance()
        {
            var icon = Helpers.GetIcon("0a5ddfbcfb3989543ac7c936fc256889"); //spell resistance

            spell_resistance = Helpers.CreateFeature("SpellResitanceEvolutionFeature",
                                               "Spell Resistance",
                                               "The eidolon is protected against magic, gaining spell resistance. The eidolon’s spell resistance is equal to 11 + the summoner’s level. This spell resistance does not apply to spells cast by the summoner.",
                                               "",
                                               icon,
                                               FeatureGroup.None,
                                               Helpers.Create<AddSpellResistance>(a => a.Value = Helpers.CreateContextValue(AbilitySharedValue.StatBonus)),
                                               Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueTypeExtender.MasterFeatureRank.ToContextRankBaseValueType(),
                                                                                feature: summoner_rank
                                                                                ),
                                               Helpers.CreateCalculateSharedValue(Helpers.CreateContextDiceValue(DiceType.One, Helpers.CreateContextValue(AbilityRankType.Default), 11))
                                              );
        }


        static void createSizeIncrease()
        {
            var reduced_reach = library.Get<BlueprintUnitFact>("c33f2d68d93ceee488aa4004347dffca");
            size_increase = new BlueprintFeature[2];
            size_increase[0] = Helpers.CreateFeature("SizeIncreaseLargeEvolutionFeature",
                                                     "Size Increase: Large",
                                                      "The eidolon grows in size, becoming Large. The eidolon gains a +4 bonus to Strength, a +2 bonus to Constitution, and a +2 bonus to its natural armor. It takes a –2 penalty to Dexterity.\n"
                                                      + "The ability increase evolution gives only 1 point when adding to the Strength or Constitution scores of a Large or Huge eidolon.\n",
                                                     "",
                                                     Helpers.GetIcon("c60969e7f264e6d4b84a1499fdcf9039"),
                                                     FeatureGroup.None,
                                                     Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureList, featureList: ability_increase[0]),
                                                     Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureList, type: AbilityRankType.StatBonus, featureList: ability_increase[2]),
                                                     Helpers.Create<SizeMechanics.PermanentSizeOverride>(a => a.size = Size.Large),
                                                     //Common.createChangeUnitSize(Size.Large),
                                                     Helpers.CreateAddStatBonus(StatType.Strength, 4, ModifierDescriptor.Feat),
                                                     Helpers.CreateAddStatBonus(StatType.Constitution, 2, ModifierDescriptor.Feat),
                                                     Helpers.CreateAddStatBonus(StatType.AC, 2, ModifierDescriptor.NaturalArmor),
                                                     Helpers.CreateAddStatBonus(StatType.Dexterity, -2, ModifierDescriptor.UntypedStackable),
                                                     Helpers.CreateAddContextStatBonus(StatType.Strength, ModifierDescriptor.Feat, multiplier: -1),
                                                     Helpers.CreateAddContextStatBonus(StatType.Constitution, ModifierDescriptor.Feat, rankType: AbilityRankType.StatBonus, multiplier: -1),
                                                     Helpers.Create<RecalculateOnFactsChange>(r => r.CheckedFacts = ability_increase[0].AddToArray(ability_increase[2])),
                                                     Helpers.Create<NewMechanics.AddFeatureIfHasArchetype>(a =>
                                                                     {
                                                                         a.Feature = reduced_reach;
                                                                         a.archetype = Eidolon.quadruped_archetype;
                                                                     }
                                                                     )
                                                     );
            size_increase[1] = Helpers.CreateFeature("SizeIncreaseHugeEvolutionFeature",
                                                     "Size Increase: Huge",
                                                     "The eidolon grows in size, becoming Huge. The eidolon gains a +8 bonus to Strength, a +4 bonus to Constitution, and a +5 bonus to its natural armor. It takes a –4 penalty to Dexterity. These bonuses and penalties replace, and do not stack with, those gained from becoming Large.\n"
                                                     + "The ability increase evolution gives only 1 point when adding to the Strength or Constitution scores of a Large or Huge eidolon.\n",
                                                     "",
                                                     Helpers.GetIcon("c60969e7f264e6d4b84a1499fdcf9039"),
                                                     FeatureGroup.None,
                                                     Helpers.Create<SizeMechanics.PermanentSizeOverride>(a => a.size = Size.Huge),
                                                     //Common.createChangeUnitSize(Size.Huge),
                                                     Helpers.CreateAddStatBonus(StatType.Strength, 4, ModifierDescriptor.Feat),
                                                     Helpers.CreateAddStatBonus(StatType.Constitution, 2, ModifierDescriptor.Feat),
                                                     Helpers.CreateAddStatBonus(StatType.AC, 2, ModifierDescriptor.NaturalArmor),
                                                     Helpers.CreateAddStatBonus(StatType.Dexterity, -2, ModifierDescriptor.UntypedStackable)
                                                     );
        }


        static void createPoison()
        {

            var poison_cooldown_buff = Helpers.CreateBuff("PoisonEvolutionCooldownBuff",
                                                             "",
                                                             "",
                                                             "",
                                                             null,
                                                             null);
            poison_cooldown_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var apply_cooldown = Common.createContextActionApplyBuffToCaster(poison_cooldown_buff, Helpers.CreateContextDuration(1, DurationRate.Rounds), dispellable: false);

            StatType[] stats = new StatType[] { StatType.Strength, StatType.Constitution };
            BlueprintFeature[] features = new BlueprintFeature[2];

            for (int i = 0; i < 2; i++)
            {
                var poison_buff = library.CopyAndAdd<BlueprintBuff>("625363a810f4d884dad551b26b3454d3", $"Poison{stats[i]}EvolutionBuff", "");
                poison_buff.SetDescription("");
                poison_buff.ReplaceComponent<BuffPoisonStatDamage>(b => { b.Stat = stats[i]; b.Value = new DiceFormula(1, DiceType.D4); b.Ticks = 4; b.SuccesfullSaves = 1; });
                var apply_str_poison = Common.createContextActionApplyBuff(poison_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);
                var apply_poison_saved = Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(Helpers.CreateConditionalSaved(null, apply_str_poison)));

                var action_str = Helpers.CreateConditional(new Condition[] { Common.createContextConditionCasterHasFact(poison_cooldown_buff) }, new GameAction[0],
                                   new GameAction[] { apply_poison_saved, apply_cooldown });
                features[i] = Helpers.CreateFeature($"Poison{stats[i]}EvolutionFeature",
                                                        $"Poison: {stats[i].ToString()}",
                                                        "The eidolon secretes toxic venom, gaining a poison attack. Select one bite or sting attack. Whenever the selected attack hits, the target is poisoned.\n"
                                                        + "Eidolon Poison: Injury; save Fort negates; frequency 1/round for 4 rounds; effect 1d4 Str damage; cure 1 save. The save DC is equal to 10 + 1/2 the eidolon’s Hit Dice + the eidolon’s Constitution modifier. For 2 additional evolution points, this poison deals Constitution damage instead. This poison can be used no more than once per round.",
                                                        "",
                                                        poison_buff.Icon,
                                                        FeatureGroup.None,
                                                        Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(Helpers.CreateActionList(action_str),
                                                                                                                     weapon_category: WeaponCategory.Bite),
                                                        Helpers.Create<ContextCalculateAbilityParams>(c => c.StatType = StatType.Constitution),
                                                        Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Constitution),
                                                        Helpers.CreateSpellDescriptor(SpellDescriptor.Poison)
                                                        );
            }
            poison_strength = features[0];
            poison_constitution = features[1];
        }


        static void createBreathWeapon()
        {
            var resource = Helpers.CreateAbilityResource("BreathWeaponEvolutionResource", "", "", "", null);
            resource.SetFixedResource(1);
            var dragon_info = new List<(string, string, string)>();

            dragon_info.Add(("Black", "60-foot Line", "Acid"));
            dragon_info.Add(("Blue", "60-foot Line", "Electricity"));
            dragon_info.Add(("Brass", "60-foot Line", "Fire"));
            dragon_info.Add(("Gold", "30-foot Cone", "Fire"));
            dragon_info.Add(("Green", "30-foot Cone", "Acid"));
            dragon_info.Add(("Silver", "30-foot Cone", "Cold"));
            var description = $"The eidolon learns to exhale a cone or line of magical energy, gaining a breath weapon. Select acid, cold, electricity, or fire. The eidolon can breathe a 30-foot cone (or 60-foot line) that deals 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage of the selected type per Hit Dice it possesses. Those caught in the breath weapon can attempt a Reflex save for half damage. The DC is equal to 10 + 1/2 the eidolon’s Hit Dice + the eidolon’s Constitution modifier. The eidolon can use this ability once per day. The eidolon can gain additional uses of this ability per day by spending 1 evolution point per additional use (to a maximum of three total uses per day).";

            breath_weapon = new BlueprintFeature[dragon_info.Count][];

            for (int i = 0; i < dragon_info.Count; i++)
            {
                breath_weapon[i] = new BlueprintFeature[3];
                var prototype = library.GetAllBlueprints().OfType<BlueprintAbility>().Where(a => a.name == ("BloodlineDraconic" + dragon_info[i].Item1 + "BreathWeaponAbility")).FirstOrDefault();
                var ability = library.CopyAndAdd<BlueprintAbility>(prototype, dragon_info[i].Item1 + "BreathWeaponEvolutionAbility", "");
                ability.SetName("Breath Weapon: " + $"{dragon_info[i].Item3}, {dragon_info[i].Item2}");
                ability.SetDescription(description);
                ability.RemoveComponents<ContextRankConfig>();
                ability.RemoveComponents<AbilityResourceLogic>();
                ability.Type = AbilityType.Supernatural;
                ability.AddComponent(Helpers.CreateResourceLogic(resource));
                ability.AddComponents(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CharacterLevel),
                                      Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CharacterLevel, progression: ContextRankProgression.Div2, type: AbilityRankType.StatBonus),
                                      Helpers.Create<ContextCalculateAbilityParams>(c =>
                                      {
                                          c.ReplaceCasterLevel = true;
                                          c.ReplaceSpellLevel = true;
                                          c.StatType = StatType.Constitution;
                                          c.CasterLevel = Helpers.CreateContextValue(AbilityRankType.Default);
                                          c.SpellLevel = Helpers.CreateContextValue(AbilityRankType.StatBonus);
                                      })
                                      );
                breath_weapon[i][0] = Common.AbilityToFeature(ability, false);
                breath_weapon[i][0].AddComponent(Helpers.CreateAddAbilityResource(resource));
                breath_weapon[i][0].SetName("Breath Weapon: " + $"{dragon_info[i].Item3}, {dragon_info[i].Item2}");


                breath_weapon[i][1] = library.CopyAndAdd<BlueprintFeature>(breath_weapon[i][0], breath_weapon[i][0].name + "2", "");
                breath_weapon[i][1].SetName("Breath Weapon II: " + $"{dragon_info[i].Item3}, {dragon_info[i].Item2}");
                breath_weapon[i][1].ComponentsArray = new BlueprintComponent[0];
                breath_weapon[i][1].AddComponent(Helpers.CreateIncreaseResourceAmount(resource, 1));
                
                breath_weapon[i][2] = library.CopyAndAdd<BlueprintFeature>(breath_weapon[i][0], breath_weapon[i][0].name + "3", "");
                breath_weapon[i][2].SetName("Breath Weapon III: " + $"{dragon_info[i].Item3}, {dragon_info[i].Item2}");
                breath_weapon[i][2].ComponentsArray = new BlueprintComponent[0];
                breath_weapon[i][2].AddComponent(Helpers.CreateIncreaseResourceAmount(resource, 1));
            }

        }
    }
}
