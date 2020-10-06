using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Brain.Blueprints;
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
    public partial class Phantom
    {
        static LibraryScriptableObject library => Main.library;
        static public BlueprintCharacterClass phantom_class;
        static public bool test_mode = false;

        static public BlueprintFeature devotion;
        static public BlueprintFeature dex_cha_bonus;
        static public BlueprintFeature str_cha_bonus;
        static public BlueprintFeature slam_damage;
        static public BlueprintFeature slam_damage_large;
        static public BlueprintFeature dr5_slashing;
        static public BlueprintFeature dr_magic;
        static public BlueprintFeature dr15;
        static public BlueprintFeature magic_attacks;


        static public BlueprintProgression phantom_progression;
        static public BlueprintProgression anger; 
        static public BlueprintProgression despair;
        static public BlueprintProgression fear;
        static public BlueprintProgression hatred;
        static public BlueprintProgression whimsy;
        static public BlueprintProgression zeal;

        static public BlueprintPortrait phantom_portrait;

        static public Dictionary<string, BlueprintProgression> phantom_progressions = new Dictionary<string, BlueprintProgression>();
        static public Dictionary<string, BlueprintFeature> potent_phantom = new Dictionary<string, BlueprintFeature>();
        static public Dictionary<string, List<BlueprintFeature>> phantom_skill_foci = new Dictionary<string, List<BlueprintFeature>>();

      

       
        static public BlueprintFeatureSelection extra_class_skills;

        static BlueprintFeature outsider = library.Get<BlueprintFeature>("9054d3988d491d944ac144e27b6bc318");


        static public void create()
        {
            phantom_portrait = Helpers.createPortrait("PhantomProtrait", "Phantom", "");
            createPhantomClass();
            createPhantoms();
        }


        static void createPhantoms()
        {
            createAnger();
            createDespair();
            createHatred();
            createFear();
            createZeal();
            createKindness();
            //whimsy
        }



        static void createPhantom(string name, string display_name, string descripton, UnityEngine.Sprite icon,
                                  BlueprintArchetype archetype,
                                  BlueprintFeature feature1, BlueprintFeature feature7, BlueprintFeature feature12, BlueprintFeature feature17,
                                  StatType[] skills, int str_value, int dex_value,
                                  BlueprintAbility[] spell_like_abilities
                                  )
        {
            var ghost_fx = library.Get<BlueprintBuff>("20f79fea035330b479fc899fa201d232");

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var spectre = library.Get<BlueprintUnit>("2f91d7337b60e3b4b9b137198a8c8745");
            var unit = library.CopyAndAdd<BlueprintUnit>(spectre, name + "PhantomUnit", "");


            unit.Alignment = Alignment.TrueNeutral;
            unit.Strength = str_value;
            unit.Dexterity = dex_value;
            unit.Constitution = 13;
            unit.Intelligence = 7;
            unit.Wisdom = 10;
            unit.Charisma = 13;
            unit.Speed = 30.Feet();
            unit.AddFacts = new BlueprintUnitFact[] { natural_armor2, ghost_fx };
            unit.Body = unit.Body.CloneObject();
            unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("767e6932882a99c4b8ca95c88d823137");
            unit.Body.PrimaryHand = null;
            unit.Body.SecondaryHand = null;
            unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            unit.Body.AdditionalSecondaryLimbs = new BlueprintItemWeapon[0];
            unit.Body.DisableHands = false;
            unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.DoNotApplyAutomatically = true;
                a.Levels = 0;
                a.Archetypes = new BlueprintArchetype[] { archetype };
                a.CharacterClass = phantom_class;
                a.Skills = new StatType[] { StatType.SkillAthletics, StatType.SkillMobility, StatType.SkillStealth, StatType.SkillPersuasion, StatType.SkillKnowledgeArcana }.RemoveFromArray(skills[0]).RemoveFromArray(skills[1]);
                a.Selections = new SelectionEntry[0];
            });
            Helpers.SetField(unit, "m_Portrait", phantom_portrait);
            unit.AddComponents(Helpers.Create<Eidolon.EidolonComponent>());
            unit.AddComponents(Helpers.Create<AllowDyingCondition>());
            unit.AddComponents(Helpers.Create<AddResurrectOnRest>());
            unit.LocalizedName = unit.LocalizedName.CreateCopy();
            unit.LocalizedName.String = Helpers.CreateString(unit.name + ".Name", display_name + " Phantom");
            unit.RemoveComponents<Experience>();
            unit.RemoveComponents<AddTags>();
            unit.Brain = library.Get<BlueprintBrain>("cf986dd7ba9d4ec46ad8a3a0406d02ae"); //character brain
            unit.Faction = library.Get<BlueprintFaction>("d8de50cc80eb4dc409a983991e0b77ad"); //neutrals
            unit.Visual = library.Get<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9").Visual.CloneObject();
            unit.Visual.BloodType = Kingmaker.Visual.HitSystem.BloodType.BlackUndead;
            unit.Visual.FootstepSoundSizeType = Kingmaker.Visual.Sound.FootstepSoundSizeType.Ghost;
            unit.Visual.Barks = spectre.Visual.Barks;

           var progression = Helpers.CreateProgression(name + "PhantomProgression",
                                                        "Emotion Focus: " + display_name,
                                                        descripton,
                                                        "",
                                                        icon,
                                                        FeatureGroup.AnimalCompanion,
                                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                                        );
            progression.IsClassFeature = true;
            progression.ReapplyOnLevelUp = true;
            progression.Classes = new BlueprintCharacterClass[] { Spiritualist.spiritualist_class };
            progression.ReplaceComponent<AddPet>(a => a.Pet = unit);

            var feature1_s = Common.createAddFeatToAnimalCompanion(feature1, "");
            var feature7_s = Common.createAddFeatToAnimalCompanion(feature7, "");
            var feature12_s = Common.createAddFeatToAnimalCompanion(feature12, "");
            var feature17_s = Common.createAddFeatToAnimalCompanion(feature17, "");


            BlueprintFeature[] spell_likes = new BlueprintFeature[spell_like_abilities.Length];

            for (int i = 0; i < spell_like_abilities.Length; i++)
            {
                var resource = Helpers.CreateAbilityResource(name + spell_like_abilities[i].name + "Resource", "", "", "", null);
                resource.SetIncreasedByLevelStartPlusDivStep(0, i == 3 ? 16 : 5 + i * 2, 1, 4, 1, 0, 0.0f, Spiritualist.getSpiritualistArray());

                BlueprintAbility ability = null;
                if (!spell_like_abilities[i].HasVariants)
                {
                    ability = Common.convertToSpellLike(spell_like_abilities[i], name, Spiritualist.getSpiritualistArray(), StatType.Wisdom,
                                                               resource);
                }
                else
                {
                    List<BlueprintAbility> variants = new List<BlueprintAbility>();
                    foreach (var v in spell_like_abilities[i].Variants)
                    {
                        var a = Common.convertToSpellLike(v, name, Spiritualist.getSpiritualistArray(), StatType.Wisdom,
                                                               resource);
                        variants.Add(a);
                    }

                    ability = Common.createVariantWrapper(name + spell_like_abilities[i].name, "", variants.ToArray());
                    ability.SetNameDescriptionIcon(spell_like_abilities[i]);
                }
                spell_likes[i] = Common.AbilityToFeature(ability, false);
                spell_likes[i].AddComponent(resource.CreateAddAbilityResource());
                spell_likes[i].SetNameDescription("Emotional Power: " + spell_likes[i].Name,
                                                  "A spiritualist gains a number of spell-like abilities, which are tied to her phantom’s emotional focus. She gains one spell-like ability at 5th level, a second at 7th level, a third at 9th level, and a fourth at 16th level. A spiritualist can use each of these abilities once per day, plus one additional time per day for every 4 spiritualist levels she possesses beyond the level at which she gained the spell-like ability. The saving throw DCs for these spell-like abilities are equal to 10 + 1/2 spiritualist class level + Wisdom modifier, rather than being based on the spell’s level.");

            }

            progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, feature1_s),
                                                         Helpers.LevelEntry(5, spell_likes[0]),
                                                           Helpers.LevelEntry(7, feature7_s, spell_likes[1]),
                                                           Helpers.LevelEntry(9, spell_likes[2]),
                                                           Helpers.LevelEntry(12, feature12_s),
                                                           Helpers.LevelEntry(16, spell_likes[3]),
                                                           Helpers.LevelEntry(17, feature17_s)
                                                           };
            progression.UIGroups = new UIGroup[] {Helpers.CreateUIGroup(feature1_s, feature7_s, feature12_s, feature17_s),
                                                  Helpers.CreateUIGroup(spell_likes)
                                                 };


            var capstone = Helpers.CreateFeature(name + "PotentPhantomFeature",
                                                 "Potent Phantom: " + display_name,
                                                 display_name,
                                                 "",
                                                 icon,
                                                 FeatureGroup.None,
                                                 Helpers.CreateAddFacts(feature1_s, feature7_s, feature12_s, feature17_s),
                                                 Helpers.PrerequisiteNoFeature(progression)
                                                 );

            phantom_progressions[name] = progression;
            potent_phantom[name] = capstone;

            phantom_skill_foci[name] = new List<BlueprintFeature>();
            var skill_foci = library.Get<BlueprintFeatureSelection>("c9629ef9eebb88b479b2fbc5e836656a").AllFeatures;
            for (int i = 0; i < skill_foci.Length; i++)
            {
                StatType stat = skill_foci[i].GetComponent<AddContextStatBonus>().Stat;
                if (skills.Contains(stat))
                {
                    phantom_skill_foci[name].Add(skill_foci[i]);
                }
            }
        }


        static void createPhantomClass()
        {
            Main.logger.Log("Phantom class test mode: " + test_mode.ToString());
            var druid_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");
            var fighter_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");
            var animal_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("4cd1757a0eea7694ba5c933729a53920");

            phantom_class = Helpers.Create<BlueprintCharacterClass>();
            phantom_class.name = "PhantomClass";
            library.AddAsset(phantom_class, "");


            phantom_class.LocalizedName = Helpers.CreateString("Phantom.Name", "Phantom");
            phantom_class.LocalizedDescription = Helpers.CreateString("Phantom.Description",
                "A phantom was once a sentient, living creature that experienced great turmoil in life or during death. The power of its emotional trauma ripped it from the flow of spirits rushing toward the Astral Plane and the fates beyond, pulling it through the Ethereal Plane and toward the Negative Energy Plane. During the decent to nothingness and undeath, the spirit was able to break free, and made its way back to the Material Plane to find shelter within the consciousness of a powerful psychic spellcaster. That fusion created a spiritualist.\n"
                + "Phantoms may retain some of their memories from life, but not many. Some phantoms wish to unburden themselves of their emotional shackles, while others just wish to continue existing while avoiding the corruption of undeath. Others still wish nothing more than to inflict their torment upon the living—taking their revenge on life for the horrors they faced during and after death.\n"
                + "Phantoms are powerful beings, but they are far more emotional than they are rational. Phantoms are still shackled by the emotions that created them, and spiritualists must maintain strong control over their phantoms to keep the phantom’s often-violent emotions in check."
                );
            phantom_class.m_Icon = druid_class.Icon;
            phantom_class.SkillPoints = druid_class.SkillPoints - 1;
            phantom_class.HitDie = DiceType.D10;
            phantom_class.BaseAttackBonus = fighter_class.BaseAttackBonus;
            phantom_class.FortitudeSave = druid_class.ReflexSave;
            phantom_class.ReflexSave = druid_class.ReflexSave;
            phantom_class.WillSave = druid_class.ReflexSave;
            phantom_class.Spellbook = null;
            phantom_class.ClassSkills = new StatType[] { StatType.SkillPersuasion, StatType.SkillPerception, StatType.SkillLoreReligion, StatType.SkillStealth };
            phantom_class.IsDivineCaster = false;
            phantom_class.IsArcaneCaster = false;
            phantom_class.StartingGold = fighter_class.StartingGold;
            phantom_class.PrimaryColor = fighter_class.PrimaryColor;
            phantom_class.SecondaryColor = fighter_class.SecondaryColor;
            phantom_class.RecommendedAttributes = new StatType[0];
            phantom_class.NotRecommendedAttributes = new StatType[0];
            phantom_class.EquipmentEntities = animal_class.EquipmentEntities;
            phantom_class.MaleEquipmentEntities = animal_class.MaleEquipmentEntities;
            phantom_class.FemaleEquipmentEntities = animal_class.FemaleEquipmentEntities;
            phantom_class.ComponentsArray = new BlueprintComponent[] { Helpers.PrerequisiteClassLevel(phantom_class, 1) };
            phantom_class.StartingItems = animal_class.StartingItems;
            createPhantomProgression();
            phantom_class.Progression = phantom_progression;

            phantom_class.Archetypes = new BlueprintArchetype[] { };
            Helpers.RegisterClass(phantom_class);
        }


        static void createPhantomProgression()
        {
            outsider.HideInCharacterSheetAndLevelUp = true;
            outsider.HideInUI = true;
            //devotion
            //evasion
            //natural armor
            //str/dex increase
            //improved evasion

            devotion = library.CopyAndAdd<BlueprintFeature>("226f939b7dfd47b4697ec52f79799012", "PhantomDevotionFeature", "");
            devotion.SetDescription("AThen phantom gains a +4 morale bonus on Will saves against enchantment spells and effects.");
          
            var natural_armor = library.CopyAndAdd<BlueprintFeature>("0d20d88abb7c33a47902bd99019f2ed1", "PhantomNaturalArmorFeature", "");
            natural_armor.SetNameDescription("Armor Bonus",
                                             "Phantom receives bonuses to their natural armor. An eidolon cannot wear armor of any kind, as the armor interferes with the summoner’s connection to the eidolon.");
            dex_cha_bonus = library.CopyAndAdd<BlueprintFeature>("0c80276018694f24fbaf59ec7b841f2b", "PhantomDexChaIncreaseFeature", "");
            dex_cha_bonus.SetNameDescription("Phantom Prowess", "Phantom receives +1 bonus to their Dexterity and Charisma.");
            dex_cha_bonus.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.CreateAddStatBonus(StatType.Dexterity, 2, ModifierDescriptor.None),
                Helpers.CreateAddStatBonus(StatType.Charisma, 2, ModifierDescriptor.None),
            };
            str_cha_bonus = library.CopyAndAdd<BlueprintFeature>("0c80276018694f24fbaf59ec7b841f2b", "PhantomStrChaIncreaseFeature", "");
            str_cha_bonus.SetNameDescription("Phantom Prowess", "Anger phantom receives +1 bonus to their Strength and Charisma.");
            str_cha_bonus.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.CreateAddStatBonus(StatType.Strength, 2, ModifierDescriptor.None),
                Helpers.CreateAddStatBonus(StatType.Charisma, 2, ModifierDescriptor.None),
            };

            createExtraClassSkill();
            createSlamDamage();
            createDr();
            phantom_progression = Helpers.CreateProgression("PhantomProgression",
                                                   phantom_class.Name,
                                                   phantom_class.Description,
                                                   "",
                                                   phantom_class.Icon,
                                                   FeatureGroup.None);
            phantom_progression.Classes = new BlueprintCharacterClass[] { phantom_class };


            magic_attacks = Helpers.CreateFeature("PhantomMagicAttacksFeature",
                                     "Magic Attacks",
                                     "Starting from 3rd level, the phantom treats its slam attacks as if they were magic for the purposes of overcoming damage reduction.",
                                     "",
                                     null,
                                     FeatureGroup.None,
                                     Common.createAddOutgoingMagic()
                                     );


            var bonus_feat = library.Get<BlueprintFeatureSelection>("247a4068296e8be42890143f451b4b45");
            phantom_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, outsider, dr5_slashing, slam_damage, 
                                                                                       library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                       library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa"),  //touch calculate feature
                                                                                       library.Get<BlueprintFeature>("0aeba56961779e54a8a0f6dedef081ee")), //inside the storm
                                                                    Helpers.LevelEntry(2, natural_armor, dex_cha_bonus, extra_class_skills, bonus_feat),
                                                                    Helpers.LevelEntry(3, magic_attacks),
                                                                    Helpers.LevelEntry(4, natural_armor, dex_cha_bonus, dr_magic),
                                                                    Helpers.LevelEntry(5, devotion),
                                                                    Helpers.LevelEntry(6, natural_armor, dex_cha_bonus),
                                                                    Helpers.LevelEntry(7, Eidolon.multi_attack),
                                                                    Helpers.LevelEntry(8, natural_armor, dex_cha_bonus),
                                                                    Helpers.LevelEntry(9, natural_armor, dex_cha_bonus),
                                                                    Helpers.LevelEntry(10),
                                                                    Helpers.LevelEntry(11),
                                                                    Helpers.LevelEntry(12, natural_armor, dex_cha_bonus),
                                                                    Helpers.LevelEntry(13, natural_armor, dex_cha_bonus),
                                                                    Helpers.LevelEntry(14),
                                                                    Helpers.LevelEntry(15, natural_armor, dex_cha_bonus, dr15),
                                                                    Helpers.LevelEntry(16),
                                                                    Helpers.LevelEntry(17),
                                                                    Helpers.LevelEntry(18),
                                                                    Helpers.LevelEntry(19),
                                                                    Helpers.LevelEntry(20)
                                                                    };


            phantom_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(slam_damage, magic_attacks, devotion, Eidolon.multi_attack),
                                                           Helpers.CreateUIGroup(dr5_slashing, dr_magic,dr15),
                                                           Helpers.CreateUIGroup(natural_armor),
                                                           Helpers.CreateUIGroup(dex_cha_bonus, str_cha_bonus),
                                                        };
        }


        static BlueprintCharacterClass[] getPhantomArray()
        {
            return new BlueprintCharacterClass[] { phantom_class };
        }


        static void createDr()
        {
            dr5_slashing = Helpers.CreateFeature("PhantomDR5SlashingFeature",
                                                 "Damage Reduction I",
                                                 "A phantom has DR 5/slashing.",
                                                 "",
                                                 Helpers.GetIcon("9e1ad5d6f87d19e4d8883d63a6e35568"),
                                                 FeatureGroup.None,
                                                 Common.createContextFormDR(5, PhysicalDamageForm.Slashing)
                                                 );
            dr_magic = Helpers.CreateFeature("PhantomDRMagicFeature",
                                     "Damage Reduction II",
                                     "At 4th level, the phantom gains DR 5/magic. At 8th level, the damage resistance increases to 10/magic. At 12th level, it increases to 15/magic.",
                                     "",
                                     Helpers.GetIcon("9e1ad5d6f87d19e4d8883d63a6e35568"),
                                     FeatureGroup.None,
                                     Common.createMagicDR(Helpers.CreateContextValue(AbilityRankType.Default)),
                                     Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getPhantomArray(),
                                                                     progression: ContextRankProgression.Custom,
                                                                     customProgression: new (int, int)[] { (7, 5), (11, 10), (20, 15) }
                                                                     )
                                     );

            dr15 = Helpers.CreateFeature("PhantomDR15Feature",
                                     "Damage Reduction III",
                                     "At 15th level, the pantom's damage reduction becomes DR 15/-.",
                                     "",
                                     Helpers.GetIcon("9e1ad5d6f87d19e4d8883d63a6e35568"),
                                     FeatureGroup.None,
                                     Common.createContextFormDR(5, PhysicalDamageForm.Slashing)
                                     );
        }


        static void createSlamDamage()
        {
            DiceFormula[] diceFormulas = new DiceFormula[] {new DiceFormula(1, DiceType.D6),
                                                            new DiceFormula(1, DiceType.D8),
                                                            new DiceFormula(1, DiceType.D10),
                                                            new DiceFormula(2, DiceType.D6),
                                                            new DiceFormula(2, DiceType.D8)};
            slam_damage = Helpers.CreateFeature("PhantomSlamDamage",
                                                "Slam Damage",
                                                "Phantoms have two slam natural weapon attacks. \n"
                                                + "The damage dealt by medium phantom with her slam attack is 1d6 at levels 1-3, 1d8 at levels 4-6, 1d10 at levels 7 - 9, 2d6 at levels 10-12 and finally 2d8 at level 15.",
                                                "",
                                                Helpers.GetIcon("247a4068296e8be42890143f451b4b45"), //basic feat
                                                FeatureGroup.None,
                                               Helpers.Create<NewMechanics.ContextWeaponDamageDiceReplacementForSpecificCategory>(c =>
                                               {
                                                   c.category = WeaponCategory.OtherNaturalWeapons;
                                                   c.value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                   c.dice_formulas = diceFormulas;
                                               }),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                type: AbilityRankType.Default,
                                                                                progression: ContextRankProgression.DelayedStartPlusDivStep,
                                                                                startLevel: 4,
                                                                                stepLevel: 3,
                                                                                classes: getPhantomArray())
                                                );

            DiceFormula[] diceFormulas2 = new DiceFormula[] {new DiceFormula(1, DiceType.D8),
                                                            new DiceFormula(2, DiceType.D6),
                                                            new DiceFormula(2, DiceType.D8),
                                                            new DiceFormula(3, DiceType.D6),
                                                            new DiceFormula(3, DiceType.D8)};
            slam_damage_large = Helpers.CreateFeature("PhantomSlamDamageLarge",
                                                "Anger Phantom Slam Damage",
                                                "Phantoms have two slam natural weapon attacks. \n"
                                                + "The damage dealt by medium anger phantom with her slam attack is 1d8 at levels 1-3, 2d6 at levels 4-6, 2d18 at levels 7 - 9, 3d6 at levels 10-12 and finally 3d8 at level 15.",
                                                "",
                                                Helpers.GetIcon("247a4068296e8be42890143f451b4b45"), //basic feat
                                                FeatureGroup.None,
                                               Helpers.Create<NewMechanics.ContextWeaponDamageDiceReplacementForSpecificCategory>(c =>
                                               {
                                                   c.category = WeaponCategory.OtherNaturalWeapons;
                                                   c.value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                   c.dice_formulas = diceFormulas2;
                                               }),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                type: AbilityRankType.Default,
                                                                                progression: ContextRankProgression.DelayedStartPlusDivStep,
                                                                                startLevel: 4,
                                                                                stepLevel: 3,
                                                                                classes: getPhantomArray())
                                                );
        }


        static void createExtraClassSkill()
        {
            var skill_foci = library.Get<BlueprintFeatureSelection>("c9629ef9eebb88b479b2fbc5e836656a").AllFeatures;
            BlueprintFeature[] skills = new BlueprintFeature[skill_foci.Length];
            for (int i = 0; i < skill_foci.Length; i++)
            {
                StatType stat = skill_foci[i].GetComponent<AddContextStatBonus>().Stat;
                string name = LocalizedTexts.Instance.Stats.GetText(stat);

                skills[i] = Helpers.CreateFeature(stat.ToString() + "ExtraPhantomSkillFeature",
                                                   "Extra Class Skill: " + name,
                                                   "The Phantom can choose 1 additional skill as its class skill.",
                                                   "",
                                                   skill_foci[i].Icon,
                                                   FeatureGroup.None,
                                                   Helpers.Create<AddClassSkill>(a => a.Skill = stat),
                                                   Helpers.Create<NewMechanics.PrerequisiteNoClassSkill>(p => p.skill = stat)
                                                   );
            }


            extra_class_skills = Helpers.CreateFeatureSelection("PhantomExtraClassSkill",
                                                               "Extra Class Skill",
                                                               skills[0].Description,
                                                               "",
                                                               null,
                                                               FeatureGroup.None);
            extra_class_skills.AllFeatures = skills;


        }


        static BlueprintArchetype createPhantomArchetype(string name, string display_name, 
                                           bool fortitude_high, bool reflex_high, bool will_high,
                                           StatType[] skills,
                                           LevelEntry[] add_features, LevelEntry[] remove_features
                                           )
        {
            var archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = name;
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", display_name);
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", phantom_class.Description);
            });
            Helpers.SetField(archetype, "m_ParentClass", phantom_class);
            library.AddAsset(archetype, "");


            archetype.RemoveFeatures = remove_features;
            archetype.AddFeatures = add_features;
            if (fortitude_high)
            {
                archetype.FortitudeSave = library.Get<BlueprintStatProgression>("ff4662bde9e75f145853417313842751");
            }
            if (reflex_high)
            {
                archetype.ReflexSave = library.Get<BlueprintStatProgression>("ff4662bde9e75f145853417313842751");
            }
            if (will_high)
            {
                archetype.WillSave = library.Get<BlueprintStatProgression>("ff4662bde9e75f145853417313842751");
            }

            var skill_feature = Helpers.CreateFeature(name + "SkillFeature",
                                                      "",
                                                      "",
                                                      "",
                                                      null,
                                                      FeatureGroup.None
                                                      );
            skill_feature.HideInCharacterSheetAndLevelUp = true;
            skill_feature.HideInUI = true;
            foreach (var s in skills)
            {
                skill_feature.AddComponents(Helpers.Create<AddClassSkill>(a => a.Skill = s),
                                            Helpers.Create<SkillMechanics.SetSkillRankToValue>(ss => { ss.skill = s; ss.value = Helpers.CreateContextValue(AbilityRankType.Default); /*ss.increase_by1_on_apply = true;*/ }),
                                                   Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.OnePlusDivStep,
                                                                                   stepLevel: 1,
                                                                                   classes: new BlueprintCharacterClass[] { phantom_class }
                                                                                   ));
            }

            if (archetype.AddFeatures.Length > 0 && archetype.AddFeatures[0].Level == 1)
            {
                archetype.AddFeatures[0].Features.Add(skill_feature);
            }
            else
            {
                archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, skill_feature) }.AddToArray(archetype.AddFeatures);
            }
                                                      
            phantom_class.Archetypes = phantom_class.Archetypes.AddToArray(archetype);
            return archetype;
        }
    }
}
