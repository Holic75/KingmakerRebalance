using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.CharacterSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    public partial class Eidolon
    {
        static LibraryScriptableObject library => Main.library;
        static public BlueprintCharacterClass eidolon_class;
        static public bool test_mode = false;

        static public BlueprintFeature multi_attack;
        static public BlueprintFeature devotion;
        static public BlueprintProgression eidolon_progression;
        static public BlueprintProgression angel_eidolon; //ok
        static public BlueprintProgression azata_eidolon; //ok
        static public BlueprintProgression twinned_eidolon;
        static public BlueprintProgression twinned_eidolon_small;
        static public BlueprintProgression agathion_eidolon; //ok
        static public BlueprintProgression fire_elemental_eidolon; //ok
        static public BlueprintProgression water_elemental_eidolon; //ok
        static public BlueprintProgression air_elemental_eidolon; //ok? - visual might be better?
        static public BlueprintProgression earth_elemental_eidolon;
        static public BlueprintProgression demon_eidolon; //ok ?
        static public BlueprintProgression daemon_eidolon;
        static public BlueprintProgression devil_eidolon;//ok
        static public BlueprintProgression fey_eidolon; //ok
        static public BlueprintProgression inevitable_eidolon;//ok
        static public BlueprintProgression construct_eidolon;//ok
        static public BlueprintProgression infernal_eidolon;
        static public BlueprintProgression protean_eidolon;
        static public BlueprintProgression air_quadruped_eidolon;
        static public BlueprintProgression fire_quadruped_eidolon;
        static public BlueprintProgression earth_quadruped_eidolon;
        static public BlueprintProgression water_serpentine_eidolon;
        static public BlueprintArchetype fey_archetype;
        static public BlueprintArchetype twinned_archetype;
        static public BlueprintArchetype quadruped_archetype;
        static public BlueprintArchetype serpentine_archetype;
        static public BlueprintArchetype infernal_archetype;
        static public BlueprintFeatureSelection extra_class_skills;

        static public BlueprintFeature water_fx_feature;

        static public List<BlueprintFeature> transferable_abilities = new List<BlueprintFeature>();

        static BlueprintFeature outsider = library.Get<BlueprintFeature>("9054d3988d491d944ac144e27b6bc318");

        static Dictionary<BlueprintProgression, BlueprintProgression> normal_to_lesser_eidolon_map = new Dictionary<BlueprintProgression, BlueprintProgression>();

        static public void create()
        {
            createEidolonClass();         
            createEidolonUnits();
            Evolutions.initialize();
            fillEidolonProgressions();
            correctText();
        }


        static void addLesserEidolon(BlueprintProgression normal_eidolon)
        {
            var lesser_eidolon = library.CopyAndAdd(normal_eidolon, "Lesser" + normal_eidolon.Name, "");
            lesser_eidolon.SetName("Lesser " + normal_eidolon.Name);
            normal_to_lesser_eidolon_map.Add(normal_eidolon, lesser_eidolon);
        }


        static void setLesserEidolonProgression(BlueprintProgression normal_eidolon)
        {
            normal_to_lesser_eidolon_map[normal_eidolon].LevelEntries = new LevelEntry[]
            {
                Helpers.LevelEntry(1, normal_eidolon.LevelEntries[0].Features),
                Helpers.LevelEntry(8, normal_eidolon.LevelEntries[1].Features),
                Helpers.LevelEntry(16, normal_eidolon.LevelEntries[2].Features)
            };

            normal_to_lesser_eidolon_map[normal_eidolon].UIGroups = normal_eidolon.UIGroups;
        }



        static public BlueprintFeature[] getLesserEidolons()
        {
            return normal_to_lesser_eidolon_map.Values.ToArray();
        }


        static public BlueprintProgression getLesserEidolon(BlueprintProgression normal_eidolon)
        {
            if (normal_eidolon == null)
            {
                return null;
            }
            return normal_to_lesser_eidolon_map.ContainsKey(normal_eidolon) ? normal_to_lesser_eidolon_map[normal_eidolon] : null;
        }


        public static AddFeatureToCompanion addTransferableFeatToEidolon(string name, params BlueprintComponent[] components)
        {
            var feature = Helpers.CreateFeature(name,
                                                "",
                                                "",
                                                "",
                                                null,
                                                FeatureGroup.None,
                                                components);
            feature.HideInCharacterSheetAndLevelUp = true;

            var add_feat_ac = Helpers.Create<Kingmaker.Designers.Mechanics.Facts.AddFeatureToCompanion>();
            add_feat_ac.Feature = feature;
            transferable_abilities.Add(feature);
            return add_feat_ac;
        }


        static void correctText()
        {
            var empty_animal_companion = library.Get<BlueprintFeature>("472091361cf118049a2b4339c4ea836a");
            empty_animal_companion.SetName("Companion — Continue");

            var animal_rank = library.Get<BlueprintFeature>("1670990255e4fe948a863bafd5dbda5d");
            animal_rank.SetNameDescription("Companion", "Companion");
        }


        static void fillEidolonProgressions()
        {
            fillAngelProgression();
            fillAzataProgression();
            fillDaemonProgression();
            fillDemonProgression();
            fillDevilProgression();
            fillInevitableProgression();
            fillFireElementalProgression();
            fillAirElementalProgression();
            fillWaterElementalProgression();
            fillEarthElementalProgression();
            fillFeyProgression();
            fillAgathionProgression();
            fillProteanProgression();
            fillTwinnedProgression();
        }


        static void createEidolonUnits()
        {
            createFireElementalUnit();
            createWaterElementalUnit();
            createAirElementalUnit();
            createEarthElementalUnit();
            createAngelUnit();
            createAzataUnit();
            createFeyUnit();
            createInevitableUnit();
            createDevilUnit();
            createDemonUnit();
            createDaemonUnit();
            createAgathionUnit();
            createProteanUnit();
            createQuadrupedEarthElementalUnit();
            createSerpentineWaterElementalUnit();
            createQuadrupedFireElementalUnit();
            createQuadrupedAirElementalUnit();
            createTwinnedEidolon();
        }


        static void createFeyEidolonArchetype()
        {
            fey_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "FeyEidolonArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Fey Eidolon");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Fey eidolons are whimsical and mysterious creatures, prone to flights of fancy, odd compulsions, and alien humor. While some creatures from the faerie realms have truly unusual shapes, the ones close enough to the human mind to serve as eidolons tend to look like idealized humanoids with unusual features that set them apart, such as pointed ears or gossamer wings.");
            });
            Helpers.SetField(fey_archetype, "m_ParentClass", eidolon_class);
            library.AddAsset(fey_archetype, "");
            fey_archetype.ReplaceClassSkills = true;
            fey_archetype.ClassSkills = new StatType[] { StatType.SkillMobility, StatType.SkillPersuasion, StatType.SkillLoreNature, StatType.SkillThievery, StatType.SkillUseMagicDevice };
            fey_archetype.RemoveFeatures = new LevelEntry[0];
            var fey_type = library.Get<BlueprintFeature>("018af8005220ac94a9a4f47b3e9c2b4e");
            fey_type.HideInUI = true;
            fey_type.HideInCharacterSheetAndLevelUp = true;
            fey_archetype.AddFeatures = new LevelEntry[] {Helpers.LevelEntry(1, fey_type) };
        }


        static void createTwinnedArchetype()
        {
            twinned_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "TwinnedEidolonArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Twinned Eidolon");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", eidolon_class.Description);
            });
            Helpers.SetField(twinned_archetype, "m_ParentClass", eidolon_class);
            library.AddAsset(twinned_archetype, "");
            twinned_archetype.RemoveFeatures = new LevelEntry[0];
            twinned_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(10, library.Get<BlueprintFeatureSelection>("247a4068296e8be42890143f451b4b45")) };
        }


        static void createQuadrupedArchetype()
        {
            quadruped_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "QuadruoedEidolonArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Quadruped Eidolon");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", Eidolon.eidolon_class.Description);
            });
            Helpers.SetField(quadruped_archetype, "m_ParentClass", eidolon_class);
            library.AddAsset(quadruped_archetype, "");

            quadruped_archetype.RemoveFeatures = new LevelEntry[0];
            quadruped_archetype.ReflexSave = library.Get<BlueprintStatProgression>("ff4662bde9e75f145853417313842751");
            quadruped_archetype.WillSave = library.Get<BlueprintStatProgression>("dc0c7c1aba755c54f96c089cdf7d14a3");

            quadruped_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1) };
        }


        static void createSerpentineArchetype()
        {
            serpentine_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "SerpentineEidolonArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Serpentine Eidolon");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", Eidolon.eidolon_class.Description);
            });
            Helpers.SetField(serpentine_archetype, "m_ParentClass", eidolon_class);
            library.AddAsset(serpentine_archetype, "");

            serpentine_archetype.RemoveFeatures = new LevelEntry[0];
            serpentine_archetype.ReflexSave = library.Get<BlueprintStatProgression>("ff4662bde9e75f145853417313842751");
            serpentine_archetype.FortitudeSave = library.Get<BlueprintStatProgression>("dc0c7c1aba755c54f96c089cdf7d14a3");

            serpentine_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1) };
        }


        static void createInfernalEidolonArchetype()
        {
            infernal_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "InfernalEidolonArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Infernal Eidolon");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "The devil binder’s eidolon never increases its maximum number of attacks, and its base attack bonus is equal to half its Hit Dice. At 4th level and every 4 levels thereafter, the eidolon’s Charisma score increases by 2.");
            });
            Helpers.SetField(infernal_archetype, "m_ParentClass", eidolon_class);
            library.AddAsset(infernal_archetype, "");
            infernal_archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(5, devotion) };
            infernal_archetype.BaseAttackBonus = library.Get<BlueprintStatProgression>("0538081888b2d8c41893d25d098dee99"); //low bab

            infernal_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1)};
        }


        static void createEidolonClass()
        {
            Main.logger.Log("Eidolon class test mode: " + test_mode.ToString());
            var druid_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");
            var fighter_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");
            var animal_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("4cd1757a0eea7694ba5c933729a53920");

            eidolon_class = Helpers.Create<BlueprintCharacterClass>();
            eidolon_class.name = "EidolonClass";
            library.AddAsset(eidolon_class, "e3b3ad6decb14cdba2e7e14982d90035");


            eidolon_class.LocalizedName = Helpers.CreateString("Eidolon.Name", "Eidolon");
            eidolon_class.LocalizedDescription = Helpers.CreateString("Eidolon.Description",
                "The eidolon takes a form shaped by the summoner’s desires. The eidolon’s Hit Dice, saving throws, skills, feats, and abilities are tied to the summoner’s class level and increase as the summoner gains levels. In addition, each eidolon gains a pool of evolution points based on the summoner’s class level that can be used to give the eidolon different abilities and powers. Whenever the summoner gains a level, he must decide how these points are spent, and they are set until he gains another level of summoner.\n"
                + "The eidolon’s physical appearance is up to the summoner, but it always appears as some sort of fantastical creature appropriate to its subtype. This control is not fine enough to make the eidolon appear like a specific creature."
                );
            eidolon_class.m_Icon = druid_class.Icon;
            eidolon_class.SkillPoints = druid_class.SkillPoints + 1;
            eidolon_class.HitDie = DiceType.D10;
            eidolon_class.BaseAttackBonus = fighter_class.BaseAttackBonus;
            eidolon_class.FortitudeSave = druid_class.FortitudeSave;
            eidolon_class.ReflexSave = druid_class.ReflexSave;
            eidolon_class.WillSave = druid_class.WillSave;
            eidolon_class.Spellbook = null;
            eidolon_class.ClassSkills = new StatType[] { StatType.SkillPersuasion, StatType.SkillPerception, StatType.SkillLoreReligion, StatType.SkillStealth };
            eidolon_class.IsDivineCaster = false;
            eidolon_class.IsArcaneCaster = false;
            eidolon_class.StartingGold = fighter_class.StartingGold;
            eidolon_class.PrimaryColor = fighter_class.PrimaryColor;
            eidolon_class.SecondaryColor = fighter_class.SecondaryColor;
            eidolon_class.RecommendedAttributes = new StatType[0];
            eidolon_class.NotRecommendedAttributes = new StatType[0];
            eidolon_class.EquipmentEntities = animal_class.EquipmentEntities;
            eidolon_class.MaleEquipmentEntities = animal_class.MaleEquipmentEntities;
            eidolon_class.FemaleEquipmentEntities = animal_class.FemaleEquipmentEntities;
            eidolon_class.ComponentsArray = new BlueprintComponent[] { Helpers.PrerequisiteClassLevel(eidolon_class, 1)};
            eidolon_class.StartingItems = animal_class.StartingItems;
            createEidolonProgression();
            eidolon_class.Progression = eidolon_progression;

            createFeyEidolonArchetype();
            createInfernalEidolonArchetype();
            createQuadrupedArchetype();
            createSerpentineArchetype();
            createTwinnedArchetype();
            eidolon_class.Archetypes = new BlueprintArchetype[] {fey_archetype, infernal_archetype, quadruped_archetype, serpentine_archetype, twinned_archetype};
            Helpers.RegisterClass(eidolon_class);
        }


        static void createEidolonProgression()
        {
            outsider.HideInCharacterSheetAndLevelUp = true;
            outsider.HideInUI = true;
            //devotion
            //evasion
            //natural armor
            //str/dex increase
            //improved evasion


            multi_attack = Helpers.CreateFeature("MultiAttackFeature",
                                                     "Multiattack",
                                                     "The creature reduces secondary attacks penalty to -2 if it has 3 or more natural attacks. If it does not have the requisite 3 or more natural attacks (or it is reduced to less than 3 attacks), the creature instead gains a second attack with one of its natural weapons, albeit at a –5 penalty.",
                                                     "",
                                                     null,
                                                     FeatureGroup.Feat,
                                                     Helpers.Create<CompanionMechanics.MultiAttack>(),
                                                     Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 6)
                                                     );
            library.AddFeats(multi_attack);

            devotion = library.CopyAndAdd<BlueprintFeature>("226f939b7dfd47b4697ec52f79799012", "EidolonDevotionFeature", "");
            devotion.SetDescription("An eidolon gains a +4 morale bonus on Will saves against enchantment spells and effects.");
            var evasion = library.CopyAndAdd<BlueprintFeature>("815bec596247f9947abca891ef7f2ca8", "EidolonEvasionFeature", "");
            evasion.SetDescription("If the eidolon is subjected to an attack that normally allows a Reflex save for half damage, it takes no damage if it succeeds at its saving throw.");
            var improved_evasion = library.CopyAndAdd<BlueprintFeature>("bcb37922402e40d4684e7fb7e001d110", "EidolonImprovedEvasionFeature", "");
            improved_evasion.SetDescription("When subjected to an attack that allows a Reflex saving throw for half damage, an eidolon takes no damage if it succeeds at its saving throw and only half damage if it fails.");

            var natural_armor = library.CopyAndAdd<BlueprintFeature>("0d20d88abb7c33a47902bd99019f2ed1", "EidolonNaturalArmorFeature", "");
            natural_armor.SetNameDescription("Armor Bonus",
                                             "Eidolon receives bonuses to their natural armor. An eidolon cannot wear armor of any kind, as the armor interferes with the summoner’s connection to the eidolon.");
            var str_dex_bonus = library.CopyAndAdd<BlueprintFeature>("0c80276018694f24fbaf59ec7b841f2b", "EidolonStrDexIncreaseFeature", "");
            str_dex_bonus.SetNameDescription("Physical Prowess", "Eidolon receives +1 bonus to their Strength and Dexterity.");

            createExtraClassSkill();
            eidolon_progression = Helpers.CreateProgression("EidolonProgression",
                                                   eidolon_class.Name,
                                                   eidolon_class.Description,
                                                   "",
                                                   eidolon_class.Icon,
                                                   FeatureGroup.None);
            eidolon_progression.Classes = new BlueprintCharacterClass[] { eidolon_class };

            var bonus_feat = library.Get<BlueprintFeatureSelection>("247a4068296e8be42890143f451b4b45");
            eidolon_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, outsider,
                                                                                       library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                       library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa"),  //touch calculate feature
                                                                                       library.Get<BlueprintFeature>("0aeba56961779e54a8a0f6dedef081ee")), //inside the storm
                                                                    Helpers.LevelEntry(2, natural_armor, evasion, str_dex_bonus, extra_class_skills, extra_class_skills, bonus_feat),
                                                                    Helpers.LevelEntry(3),
                                                                    Helpers.LevelEntry(4, natural_armor, str_dex_bonus),
                                                                    Helpers.LevelEntry(5, devotion),
                                                                    Helpers.LevelEntry(6, natural_armor, str_dex_bonus),
                                                                    Helpers.LevelEntry(7, multi_attack),
                                                                    Helpers.LevelEntry(8, natural_armor, str_dex_bonus),
                                                                    Helpers.LevelEntry(9, natural_armor, str_dex_bonus),
                                                                    Helpers.LevelEntry(10),
                                                                    Helpers.LevelEntry(11, improved_evasion),
                                                                    Helpers.LevelEntry(12, natural_armor, str_dex_bonus),
                                                                    Helpers.LevelEntry(13, natural_armor, str_dex_bonus),
                                                                    Helpers.LevelEntry(14),
                                                                    Helpers.LevelEntry(15, natural_armor, str_dex_bonus),
                                                                    Helpers.LevelEntry(16),
                                                                    Helpers.LevelEntry(17),
                                                                    Helpers.LevelEntry(18),
                                                                    Helpers.LevelEntry(19),
                                                                    Helpers.LevelEntry(20)
                                                                    };


            eidolon_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(evasion, devotion, multi_attack, improved_evasion),
                                                           Helpers.CreateUIGroup(natural_armor),
                                                           Helpers.CreateUIGroup(str_dex_bonus),
                                                        };

            var animal_companion_archetype = library.Get<BlueprintArchetype>("9f8a232fbe435a9458bf64c3024d7bee");
            foreach (var le in animal_companion_archetype.AddFeatures)
            {
                if (le.Level == 8)
                {
                    le.Features.Add(multi_attack);
                }
            }
        }


        static void createExtraClassSkill()
        {
            var skill_foci = library.Get<BlueprintFeatureSelection>("c9629ef9eebb88b479b2fbc5e836656a").AllFeatures;
            BlueprintFeature[] skills = new BlueprintFeature[skill_foci.Length];
            for (int i = 0; i < skill_foci.Length; i++)
            {
                StatType stat = skill_foci[i].GetComponent<AddContextStatBonus>().Stat;
                string name = LocalizedTexts.Instance.Stats.GetText(stat);

                skills[i] = Helpers.CreateFeature(stat.ToString() + "ExtraEidolonSkillFeature",
                                                   "Extra Class Skill: " + name,
                                                   "The Eidolon can choose 2 additional skills as its class skills.",
                                                   "",
                                                   skill_foci[i].Icon,
                                                   FeatureGroup.None,
                                                   Helpers.Create<AddClassSkill>(a => a.Skill = stat),
                                                   Helpers.Create<NewMechanics.PrerequisiteNoClassSkill>(p => p.skill = stat)
                                                   );
            }


            extra_class_skills = Helpers.CreateFeatureSelection("EidolonExtraClassSkill",
                                                               "Extra Class Skill",
                                                               skills[0].Description,
                                                               "",
                                                               null,
                                                               FeatureGroup.None);
            extra_class_skills.AllFeatures = skills;
        }


        public class EidolonComponent : BlueprintComponent
        {
            public static readonly int[] rank_to_level = new int[21]
                                                        {
                                                            0,
                                                            1,
                                                            2,
                                                            3,
                                                            3,
                                                            4,
                                                            5,
                                                            6,
                                                            6,
                                                            7,
                                                            8,
                                                            9,
                                                            9,
                                                            10,
                                                            11,
                                                            12,
                                                            12,
                                                            13,
                                                            14,
                                                            15,
                                                            15,
                                                        };

            public int getEidolonLevel(AddPet add_pet_component)
            {
                if (add_pet_component.LevelRank == null)
                    return 1;
                int? rank = add_pet_component.Owner.GetFact(add_pet_component.LevelRank)?.GetRank();
                int index = Mathf.Min(20, !rank.HasValue ? 0 : rank.Value);
                return EidolonComponent.rank_to_level[index];
            }
        }


        public class CorpseCompanionComponent : BlueprintComponent
        {
            public static readonly int[] rank_to_level = new int[21]
                                                        {
                                                            0,
                                                            1,
                                                            2,
                                                            3,
                                                            3,
                                                            4,
                                                            5,
                                                            6,
                                                            6,
                                                            7,
                                                            8,
                                                            9,
                                                            9,
                                                            10,
                                                            11,
                                                            12,
                                                            12,
                                                            13,
                                                            14,
                                                            15,
                                                            15,
                                                        };

            public int getEidolonLevel(AddPet add_pet_component)
            {
                if (add_pet_component.LevelRank == null)
                    return 1;
                int? rank = add_pet_component.Owner.GetFact(add_pet_component.LevelRank)?.GetRank();
                int index = Mathf.Min(20, !rank.HasValue ? 0 : rank.Value);
                return EidolonComponent.rank_to_level[index];
            }
        }


    }




}
