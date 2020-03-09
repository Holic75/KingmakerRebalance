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


        static public BlueprintFeature claws;
        static public BlueprintFeature bite;
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

        static BlueprintFeature summoner_rank = library.Get<BlueprintFeature>("1670990255e4fe948a863bafd5dbda5d");
        static public BlueprintFeature[] extra_evolution = new BlueprintFeature[5];
        static BlueprintFeature eidolon;

        public class EvolutionEntry
        {
            public BlueprintFeature[] required_evolutions;
            public BlueprintFeature[] conflicting_evolutions;
            public BlueprintFeature[] subtypes;
            public int cost;
            public int summoner_level;
            public int upgrade_level;
            public bool is_final;
            public int upgrade_cost;
            public string group;

            public BlueprintFeature evolution;
            public BlueprintFeature permanent_evolution;
            public BlueprintFeature selection_feature;
            public BlueprintBuff buff;


            public EvolutionEntry(BlueprintFeature evolution_feature, int evolution_cost, int min_level, BlueprintFeature[] required_evolution_features,
                                  BlueprintFeature[] conflicting_evolution_features, BlueprintFeature[] authorised_subtypes,
                                  int next_level_cost = 0, string evolution_group = "", int next_level = 0
                                  )
            {
                evolution = evolution_feature;
                cost = evolution_cost;
                summoner_level = min_level;
                conflicting_evolutions = conflicting_evolution_features;
                required_evolutions = required_evolution_features;
                subtypes = authorised_subtypes;
                is_final = next_level_cost == 0;
                upgrade_cost = next_level_cost;
                upgrade_level = next_level;
                group = evolution_group;

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
                                                          Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => {a.Feature = evolution; }));

                buff = Helpers.CreateBuff(evolution.name +"Buff",
                                            "",
                                            "",
                                            "",
                                            evolution.Icon,
                                            null,
                                            Helpers.Create<EvolutionMechanics.AddTemporaryEvolution>(a => { a.cost = cost; a.Feature = evolution; }));
            }
        }

        static List<EvolutionEntry> evolution_entries = new List<EvolutionEntry>();
        static public BlueprintFeatureSelection evolution_selection;

        static public BlueprintAbility getGrantTemporaryEvolutionAbility(int max_cost, 
                                                                          bool remove_buffs,
                                                                          string name_prefix, string display_name,
                                                                          string description,
                                                                          UnityEngine.Sprite icon,
                                                                          AbilityType ability_type,
                                                                          UnitCommand.CommandType command_type,
                                                                          string duration,
                                                                          params BlueprintComponent[] components)
        {
            List<BlueprintAbility> abilities = new List<BlueprintAbility>();
            List<ContextActionRemoveBuff> buffs_to_remove = new List<ContextActionRemoveBuff>();
            if (remove_buffs)
            {
                foreach (var ee in evolution_entries)
                {
                    buffs_to_remove.Add(Common.createContextActionRemoveBuff(ee.buff));
                }
            }
            var fx = Helpers.Create<ContextActionsOnPet>(c => c.Actions = Helpers.CreateActionList(Common.createContextActionSpawnFx(Common.createPrefabLink("352469f228a3b1f4cb269c7ab0409b8e"))));
            foreach (var ee in evolution_entries)
            {
                if (ee.cost <= max_cost 
                    && (ee.is_final || ee.upgrade_cost > max_cost || ee.upgrade_level != 0))
                {
                   var apply_buff = Common.createContextActionApplyBuffToCaster(ee.buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes),
                                                                                 dispellable: ability_type == AbilityType.Spell || ability_type == AbilityType.SpellLike);

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
                    ability.AddComponents(components);
                    foreach (var e in ee.required_evolutions)
                    {
                        ability.AddComponent(Helpers.Create<EvolutionMechanics.AbilityShowIfHasEvolution>(a => a.evolution = e));
                    }
                    foreach (var e in ee.conflicting_evolutions)
                    {
                        ability.AddComponent(Helpers.Create<EvolutionMechanics.AbilityShowIfHasEvolution>(a => {a.evolution = e; a.not = true; }));
                    }
                    ability.AddComponent(Helpers.Create<EvolutionMechanics.AbilityShowIfHasEvolution>(a => { a.evolution = ee.evolution; a.not = true; }));
                    if (!ee.subtypes.Empty())
                    {
                        ability.AddComponent(Helpers.Create<NewMechanics.AbilityShowIfCasterHasFactsFromList>(a => a.UnitFacts = ee.subtypes));
                    }
                    if (ee.summoner_level > 0 || (ee.upgrade_level > 0 && ee.upgrade_cost <= max_cost))
                    {
                        ability.AddComponent(Helpers.Create<NewMechanics.AbilityShowIfHasFeatureRank>(a =>
                                                                                                      {
                                                                                                          a.Feature = summoner_rank;
                                                                                                          a.min_value = ee.summoner_level;
                                                                                                          a.max_value = (ee.upgrade_level > 0 && ee.upgrade_cost <= max_cost) ? ee.upgrade_level : 1000;
                                                                                                      }));
                    }
                    ability.setMiscAbilityParametersSelfOnly();
                    ability.AddComponent(Helpers.Create<SharedSpells.CannotBeShared>());
                    if (ability_type == AbilityType.Spell)
                    {
                        ability.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken;
                    }
                    abilities.Add(ability);

                }
            }
            var wrapper = Common.createVariantWrapper(name_prefix + "BaseAbility", "", abilities.ToArray());
            wrapper.SetName(display_name);
            wrapper.SetDescription(description);
            wrapper.SetIcon(icon);
            return wrapper;
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
            createExtraEvolutionFeat();
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
            evolution_entries.Add(new EvolutionEntry(claws, 1, 0, new BlueprintFeature[0], new BlueprintFeature[0],
                                                     new BlueprintFeature[] { boar, dog, elk, mammoth, monitor, wolf }));

            foreach (var e in improved_natural_attacks)
            {
                evolution_entries.Add(new EvolutionEntry(e, 1, 0, new BlueprintFeature[0], new BlueprintFeature[0], new BlueprintFeature[0], 
                                                         evolution_group: "Improved Damage"));
            }

            evolution_entries.Add(new EvolutionEntry(bite, 1, 0, new BlueprintFeature[0], new BlueprintFeature[0],
                                         new BlueprintFeature[] {boar, elk, mammoth}));

            for (int i = 0; i < improved_natural_armor.Length; i++)
            {
                bool is_last = (i + 1) == improved_natural_armor.Length;
                evolution_entries.Add(new EvolutionEntry(improved_natural_armor[i], i + 1,  i * 5, new BlueprintFeature[0],
                                                         improved_natural_armor.RemoveFromArray(improved_natural_armor[i]), new BlueprintFeature[0],
                                                         next_level_cost: is_last ? 0 : i + 2, evolution_group: "Improved Natural Armor",
                                                         next_level: is_last ? 0 : (i + 1) * 5)
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

            evolution_entries.Add(new EvolutionEntry(wing_buffet, 1, 0, new BlueprintFeature[] {flight}, new BlueprintFeature[0],
                                         new BlueprintFeature[0]));

            for (int i = 0; i < ability_increase.Length; i++)
            {
                bool is_last = (i + 1) == ability_increase.Length;
                for (int j = 0; j < ability_increase[i].Length; j++)
                {
                    var same_ability_increase = new List<BlueprintFeature>(); ;
                    for (int k = 0; k < ability_increase.Length; k++)
                    {
                        if (k != i)
                        {
                            same_ability_increase.Add(ability_increase[k][j]);
                        }
                    }

                    evolution_entries.Add(new EvolutionEntry(ability_increase[i][j], 2*(i + 1), i * 6, new BlueprintFeature[0],
                                                             same_ability_increase.ToArray(), 
                                                             new BlueprintFeature[0],
                                                             next_level_cost: is_last ? 0 : 2*(i + 2), evolution_group: "Ability Increase",
                                                             next_level: is_last ? 0 : (i + 1) * 6)
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

            evolution_entries.Add(new EvolutionEntry(flight, 2, 5, new BlueprintFeature[0], new BlueprintFeature[0], new BlueprintFeature[0]));
            evolution_entries.Add(new EvolutionEntry(gore, 2, 0, new BlueprintFeature[0], new BlueprintFeature[0],
                                         new BlueprintFeature[] {bear, dog, monitor, wolf, leopard, smilodon, centipede }));

            foreach (var e in immunity)
            {
                evolution_entries.Add(new EvolutionEntry(e, 2, 7, new BlueprintFeature[0], new BlueprintFeature[0], new BlueprintFeature[0],
                                                         evolution_group: "Immunity"));
            }

            evolution_entries.Add(new EvolutionEntry(rake, 2, 4, new BlueprintFeature[0], new BlueprintFeature[0],
                             new BlueprintFeature[] { bear, dog, monitor, wolf, leopard, elk, mammoth, boar}));

            evolution_entries.Add(new EvolutionEntry(trip, 2, 4, new BlueprintFeature[] { bite }, new BlueprintFeature[0],
                 new BlueprintFeature[0]));

            for (int i = 0; i < weapon_training.Length; i++)
            {
                bool is_last = (i + 1) == weapon_training.Length;
                evolution_entries.Add(new EvolutionEntry(weapon_training[i], 2*(i+1), 0, new BlueprintFeature[0], new BlueprintFeature[0], new BlueprintFeature[] { eidolon },
                                                         evolution_group: "Weapon Training",
                                                         next_level_cost: is_last ? 0 : 2 * (i + 2))
                                                         );
            }

            evolution_entries.Add(new EvolutionEntry(blindsense, 3, 9, new BlueprintFeature[0], new BlueprintFeature[0],
                                                    new BlueprintFeature[0]));

            evolution_entries.Add(new EvolutionEntry(pounce, 3, 7, new BlueprintFeature[0], new BlueprintFeature[0],
                                        new BlueprintFeature[] { bear, dog, monitor, wolf, leopard, elk, mammoth, boar })
                                        );
            evolution_entries.Add(new EvolutionEntry(amorphous, 4, 0, new BlueprintFeature[0], new BlueprintFeature[0],
                                        new BlueprintFeature[0]));

            evolution_entries.Add(new EvolutionEntry(blindsight, 4, 11, new BlueprintFeature[] {blindsense}, new BlueprintFeature[0],
                                        new BlueprintFeature[0]));

            for (int i = 0; i < fast_healing.Length; i++)
            {
                bool is_last = (i + 1) == fast_healing.Length;
                evolution_entries.Add(new EvolutionEntry(fast_healing[i], 4 + 2*i, 11, new BlueprintFeature[0], fast_healing.RemoveFromArray(fast_healing[i]), new BlueprintFeature[0],
                                                         evolution_group: "Fast Healing",
                                                         next_level: 0,
                                                         next_level_cost: is_last ? 0 : 6 + 2*i)
                                                         );
            }

            evolution_entries.Add(new EvolutionEntry(size_increase[0], 4, 8, new BlueprintFeature[0], new BlueprintFeature[] { size_increase[1] },
                                                     new BlueprintFeature[] { eidolon }));
            evolution_entries.Add(new EvolutionEntry(size_increase[1], 10, 13, new BlueprintFeature[0], new BlueprintFeature[] { size_increase[0] },
                                                     new BlueprintFeature[] { eidolon }));

            var breath_weapon_flat = new BlueprintFeature[0];
            foreach (var bw in breath_weapon)
            {
                breath_weapon_flat = breath_weapon_flat.AddToArray(bw);
            }
            for (int i = 0; i < breath_weapon.Length; i++)
            {
                bool is_last = (i + 1) == ability_increase.Length;
                for (int j = 0; j < ability_increase[i].Length; j++)
                {
                    evolution_entries.Add(new EvolutionEntry(breath_weapon[i][j], 4 + i, 9, new BlueprintFeature[0],
                                                             breath_weapon_flat.RemoveFromArray(breath_weapon[i][j]),
                                                             new BlueprintFeature[0],
                                                             next_level_cost: is_last ? 0 : 5 + i, evolution_group: "Breath Weapon")
                                                             );
                }
            }
        }

        static void createEvolutions()
        {
            createClaws();
            createImprovedNaturalAttacks();
            createBite();
            createImprovedNaturalArmor();
            createReach();
            createResistance();            
            createSkilled();
            createWingBuffet();

            createAbilityIncrease();
            createEnergyAttacks();
            createFlight();
            createGore();
            createImmunity();
            createRake();
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
        }


        static void createWingBuffet()
        {
            var icon = Helpers.GetIcon("13143852b74718144ac4267b949615f0"); //righteous might
            var wing1d4 = library.Get<BlueprintItemWeapon>("864e29d3e07ad4a4f96d576b366b4a86");

            wing_buffet = Helpers.CreateFeature("WingBuffetEvolutionFeature",
                                         "Wing Buffet",
                                         "he eidolon learns to use its wings to batter foes, granting it two wing buffet attacks. These attacks are secondary attacks. The wing buffets deal 1d4 points of damage (1d6 if Large, 1d8 if Huge).",
                                         "",
                                         icon,
                                         FeatureGroup.None,
                                         Common.createAddSecondaryAttacks(wing1d4, wing1d4)
                                         );
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


        static void createClaws()
        {
            var icon = Helpers.GetIcon("120e51788082260498a961a38a4fa617"); //dragon calws
            var claw1d6 = library.Get<BlueprintItemWeapon>("65eb73689b94d894080d33a768cdf645");

            claws = Helpers.CreateFeature("ClawsEvolutionFeature",
                                         "Claw",
                                         "The eidolon has a pair of vicious claws at the ends of its limbs, giving it two claw attacks. These attacks are primary attacks. The claws deal 1d4 points of damage (1d6 if Large, 1d8 if Huge).",
                                         "",
                                         icon,
                                         FeatureGroup.None,
                                         Helpers.Create<AddAdditionalLimb>(a => a.Weapon = claw1d6),
                                         Helpers.Create<AddAdditionalLimb>(a => a.Weapon = claw1d6)
                                         );
        }


        static void createImprovedNaturalArmor()
        {
            var icon = library.Get<BlueprintAbility>("7bc8e27cba24f0e43ae64ed201ad5785").Icon; //resistance
            improved_natural_armor = new BlueprintFeature[5];
            for (int i = 0; i < improved_natural_armor.Length; i++)
            {
                improved_natural_armor[i] = Helpers.CreateFeature($"ImprovedNaturalArmor{i + 1}EvolutionFeature",
                                                                  $"Improved Natural Armor {Common.roman_id[i + 1]}",
                                                                   "The eidolon’s hide grows thick fur, rigid scales, or bony plates, giving it a +2 bonus to its natural armor. For every 5 levels the summoner possesses, summoner can spend 2 additional evolution points to increase armor bonus by 2.",
                                                                   "",
                                                                   icon,
                                                                   FeatureGroup.None,
                                                                   Helpers.CreateAddStatBonus(StatType.AC, 2 * (i + 1), ModifierDescriptor.NaturalArmor)
                                                                   );                                                                  
            }
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
            var energies = new DamageEnergyType[] { DamageEnergyType.Acid, DamageEnergyType.Cold, DamageEnergyType.Fire, DamageEnergyType.Electricity };
            var icon_ids = new string[] { "fedc77de9b7aad54ebcc43b4daf8decd", "5368cecec375e1845ae07f48cdc09dd1", "ddfb4ac970225f34dbff98a10a4a8844", "90987584f54ab7a459c56c2d2f22cee2" };

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
            var energies = new DamageEnergyType[] { DamageEnergyType.Acid, DamageEnergyType.Cold, DamageEnergyType.Fire, DamageEnergyType.Electricity };
            var icon_ids = new string[] { "3d77ee3fc4913c44b9df7c5bbcdc4906", "021d39c8e0eec384ba69140f4875e166", "3f9605134d34e1243b096e1f6cb4c148", "e24ce0c3e8eaaaf498d3656b534093df"};

            immunity = new BlueprintFeature[energies.Length];

            for (int i = 0; i < resistance.Length; i++)
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

            ability_increase = new BlueprintFeature[4][];
            for (int i = 0; i < 4; i++)
            {
                int bonus = (i + 1) * 2;
                ability_increase[i] = new BlueprintFeature[stats.Length];
                for (int j = 0; j < stats.Length; j++)
                {
                    ability_increase[i][j] = Helpers.CreateFeature(stats[j].ToString() + bonus.ToString() + "AbilityIncreaseFeature",
                                                                   "Ability Increase: " + stats[j].ToString() + $" (+{bonus})",
                                                                   "The eidolon grows larger muscles, gains faster reflexes, achieves greater intelligence, or acquires another increase to one of its abilities. Increase one of the eidolon’s ability scores by 2. This evolution can be selected more than once. It can be applied only once to an individual ability score. For every 6 levels summoner can spend 2 additional evolutions points to increase bonus by 2.",
                                                                   "",
                                                                   Helpers.GetIcon(icon_ids[j]),
                                                                   FeatureGroup.None,
                                                                   Helpers.CreateAddStatBonus(stats[j], (i + 1) * 2, ModifierDescriptor.Inherent)
                                                                   );
                }
            }
        }


        static void createEnergyAttacks()
        {
            var energies = new DamageEnergyType[] { DamageEnergyType.Acid, DamageEnergyType.Cold, DamageEnergyType.Fire, DamageEnergyType.Electricity };
            var icon_ids = new string[] { "97d0a51ca60053047afb9aca900fb71b", "83ed16546af22bb43bd08734a8b51941", "4783c3709a74a794dbe7c8e7e0b1b038", "728b3daffb1d9fd45958c6e60876b7a9" };

            energy_attacks = new BlueprintFeature[energies.Length];

            for (int i = 0; i < resistance.Length; i++)
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
            var gore1d6 = library.Get<BlueprintItemWeapon>("daf4ab765feba8548b244e174e7af5be");

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
            flight = Helpers.CreateFeature("FlightEvolutionFeature",
                             "Flight",
                             "The eidolon grows large wings, like those of a bat, bird, insect, or dragon, gaining the ability to fly.",
                             "",
                             icon,
                             FeatureGroup.None,
                             Common.createAuraFeatureComponent(silver_dragon_wings.Buff)
                             );
        }


        static void createRake()
        {
            var icon = Helpers.GetIcon("f68af48f9ebf32549b5f9fdc4edfd475"); //claws
            var claw1d4 = library.Get<BlueprintItemWeapon>("118fdd03e569a66459ab01a20af6811a");

            rake = Helpers.CreateFeature("RakeEvolutionFeature",
                                         "Rake",
                                         "The eidolon grows dangerous claws on its feet, allowing it to make two rake attacks against its foes. These attacks are primary attacks. These rake attacks deal 1d4 points of damage (1d6 if Large, 1d8 if Huge).",
                                         "",
                                         icon,
                                         FeatureGroup.None,
                                         Helpers.Create<AddAdditionalLimb>(a => a.Weapon = claw1d4),
                                         Helpers.Create<AddAdditionalLimb>(a => a.Weapon = claw1d4)
                                         );
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
                                         Helpers.CreateAddFacts(simple_wp, martial_wp)
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
                                               Common.createBlindsight(30)
                                              );
        }


        static void createFastHealing()
        {
            var icon = Helpers.GetIcon("4093d5a0eb5cae94e909eb1e0e1a6b36"); //remove disiease

            fast_healing = new BlueprintFeature[5];

            for (int i = 0; i < fast_healing.Length; i++)
            {
                var buff = Helpers.CreateBuff($"FastHealing{i+1}EvolutionBuff",
                                                "Fast Healing " + Common.roman_id[i + 1],
                                                "The eidolon’s body gains the ability to heal wounds very quickly, giving it fast healing 1. The eidolon heals 1 point of damage per round, just like via natural healing. Fast healing does not restore hit points lost due to starvation, thirst, or suffocation, nor does it allow the eidolon to regrow lost body parts (or to reattach severed parts). Fast healing functions as long as the eidolon is alive. This fast healing does not function when the eidolon is not on the same plane as its summoner. This healing can be increased by 1 point per round for every 2 additional evolution points spent (to a maximum of 5 points per round).",
                                                "",
                                                icon,
                                                null,
                                                Common.createAddContextEffectFastHealing(i + 1)
                                                );

                fast_healing[i] = Helpers.CreateFeature($"FastHealing{i + 1}EvolutionFeature",
                                                                            buff.Name,
                                                                            buff.Description,
                                                                            "",
                                                                            icon,
                                                                            FeatureGroup.None,
                                                                            Common.createAuraFeatureComponent(buff)
                                                                            );
            }
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
            size_increase = new BlueprintFeature[2];
            size_increase[0] = Helpers.CreateFeature("SizeIncreaseLargeEvolutionFeature",
                                                     "Size Increase: Large",
                                                      "The eidolon grows in size, becoming Large. The eidolon gains a +4 bonus to Strength, a +2 bonus to Constitution, and a +2 bonus to its natural armor. It takes a –2 penalty to Dexterity.",
                                                     "",
                                                     Helpers.GetIcon("c60969e7f264e6d4b84a1499fdcf9039"),
                                                     FeatureGroup.None,
                                                     Common.createChangeUnitSize(Size.Large),
                                                     Helpers.CreateAddStatBonus(StatType.Strength, 4, ModifierDescriptor.Size),
                                                     Helpers.CreateAddStatBonus(StatType.Constitution, 2, ModifierDescriptor.Size),
                                                     Helpers.CreateAddStatBonus(StatType.AC, 2, ModifierDescriptor.NaturalArmor),
                                                     Helpers.CreateAddStatBonus(StatType.Dexterity, -2, ModifierDescriptor.Other)
                                                     );
            size_increase[1] = Helpers.CreateFeature("SizeIncreaseHugeEvolutionFeature",
                                                     "Size Increase: Huge",
                                                     "The eidolon grows in size, becoming Huge. The eidolon gains a +8 bonus to Strength, a +4 bonus to Constitution, and a +5 bonus to its natural armor. It takes a –4 penalty to Dexterity.",
                                                     "",
                                                     Helpers.GetIcon("c60969e7f264e6d4b84a1499fdcf9039"),
                                                     FeatureGroup.None,
                                                     Common.createChangeUnitSize(Size.Huge),
                                                     Helpers.CreateAddStatBonus(StatType.Strength, 8, ModifierDescriptor.Size),
                                                     Helpers.CreateAddStatBonus(StatType.Constitution, 4, ModifierDescriptor.Size),
                                                     Helpers.CreateAddStatBonus(StatType.AC, 4, ModifierDescriptor.NaturalArmor),
                                                     Helpers.CreateAddStatBonus(StatType.Dexterity, -4, ModifierDescriptor.Other)
                                                     );
        }


        static void createBreathWeapon()
        {
            var resource = Helpers.CreateAbilityResource("BreathWeaponEvolutionResource", "", "", "", null);

            var dragon_info = new List<(string, string, string)>();

            dragon_info.Add(("Black", "60-foot Line", "Acid"));
            dragon_info.Add(("Blue", "60-foot Line", "Electricity"));
            dragon_info.Add(("Brass", "60-foot Line", "Fire"));
            dragon_info.Add(("Gold", "30-foot Cone", "Fire"));
            dragon_info.Add(("Green", "30-foot Cone", "Acid"));
            dragon_info.Add(("Silver", "30-foot Cone", "Cold"));
            var description = "The eidolon learns to exhale a cone or line of magical energy, gaining a breath weapon. Select acid, cold, electricity, or fire. The eidolon can breathe a 30-foot cone (or 60-foot line) that deals 1d6 points of damage of the selected type per Hit Dice it possesses. Those caught in the breath weapon can attempt a Reflex save for half damage. The DC is equal to 10 + 1/2 the eidolon’s Hit Dice + the eidolon’s Constitution modifier. The eidolon can use this ability once per day. The eidolon can gain additional uses of this ability per day by spending 1 evolution point per additional use (to a maximum of three total uses per day).";

            breath_weapon = new BlueprintFeature[3][];
            breath_weapon[0] = new BlueprintFeature[dragon_info.Count];
            breath_weapon[1] = new BlueprintFeature[dragon_info.Count];
            breath_weapon[2] = new BlueprintFeature[dragon_info.Count];

            for (int i = 0; i < dragon_info.Count; i++)
            {
                var prototype = library.GetAllBlueprints().OfType<BlueprintAbility>().Where(a => a.name == ("BloodlineDraconic" + dragon_info[i].Item1 + "BreathWeaponAbility")).FirstOrDefault();
                var ability = library.CopyAndAdd<BlueprintAbility>(prototype, dragon_info[i].Item1 + "BreathWeaponEvolutionAbility", "");
                ability.SetName("Breath Weapon " + $"({dragon_info[i].Item3}, {dragon_info[i].Item2})");
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
                breath_weapon[0][i] = Common.AbilityToFeature(ability, false);
                breath_weapon[0][i].AddComponent(Helpers.CreateAddAbilityResource(resource));
                breath_weapon[0][i].SetName("Breath Weapon I " + $"({dragon_info[i].Item3}, {dragon_info[i].Item2})");


                breath_weapon[1][i] = library.CopyAndAdd<BlueprintFeature>(breath_weapon[0][i], breath_weapon[0][i].name + "2", "");
                breath_weapon[1][i].SetName("Breath Weapon II " + $"({dragon_info[i].Item3}, {dragon_info[i].Item2})");
                breath_weapon[1][i].AddComponent(Helpers.CreateIncreaseResourceAmount(resource, 1));
                breath_weapon[2][i] = library.CopyAndAdd<BlueprintFeature>(breath_weapon[0][i], breath_weapon[0][i].name + "3", "");
                breath_weapon[2][i].SetName("Breath Weapon III " + $"({dragon_info[i].Item3}, {dragon_info[i].Item2})");
                breath_weapon[2][i].AddComponent(Helpers.CreateIncreaseResourceAmount(resource, 2));
            }

        }
    }
}
