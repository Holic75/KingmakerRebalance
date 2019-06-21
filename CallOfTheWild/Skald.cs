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
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Blueprints.Items.Armors;

namespace CallOfTheWild
{
   
    class Skald
    {
        static internal bool test_mode = false;
        static internal LibraryScriptableObject library => Main.library;

        static internal BlueprintCharacterClass skald_class;
        static internal BlueprintProgression skald_progression;

        static internal BlueprintFeature skald_proficiencies;
        static internal BlueprintFeature damage_reduction;
        static internal BlueprintFeature uncanny_dodge;
        static internal BlueprintFeature improved_uncanny_dodge;
        static internal BlueprintFeature fast_movement;
        static internal BlueprintFeatureSelection versatile_performance;
        static internal BlueprintFeature well_versed;
        static internal BlueprintAbilityResource performance_resource;
        static internal BlueprintFeature give_performance_resource;

        internal static void createSkaldClass()
        {
            Main.logger.Log("Skald class test mode: " + test_mode.ToString());
            var bard_class = library.TryGet<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");
            var barbarian_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f7d7eb166b3dd594fb330d085df41853");

            skald_class = Helpers.Create<BlueprintCharacterClass>();
            skald_class.name = "SkaldClass";
            library.AddAsset(skald_class, "");

            skald_class.LocalizedName = Helpers.CreateString("Skald.Name", "Skald");
            skald_class.LocalizedDescription = Helpers.CreateString("Skald.Description",
                                                                         "Skalds are poets, historians, and keepers of lore who use their gifts for oration and song to inspire allies into a frenzied rage. They balance a violent spirit with the veneer of civilization, recording events such as heroic battles and the deeds of great leaders, enhancing these stories in the retelling to earn bloodier victories in combat. A skald’s poetry is nuanced and often has multiple overlapping meanings, and he applies similar talents to emulate magic from other spellcasters.\n"
                                                                         + "Role: A skald inspires his allies, and often presses forward to fight enemies in melee. Outside of combat, he’s useful as a healer and scholar, less versatile but more durable than a bard."
                                                                         );
            skald_class.m_Icon = bard_class.Icon;
            skald_class.SkillPoints = barbarian_class.SkillPoints;
            skald_class.HitDie = DiceType.D8;
            skald_class.BaseAttackBonus = bard_class.BaseAttackBonus;
            skald_class.FortitudeSave = barbarian_class.FortitudeSave;
            skald_class.ReflexSave = barbarian_class.ReflexSave;
            skald_class.WillSave = bard_class.WillSave;
            skald_class.Spellbook = createSkaldSpellbook();
            skald_class.ClassSkills = new StatType[] {StatType.SkillAthletics, StatType.SkillMobility,
                                                      StatType.SkillKnowledgeArcana, StatType.SkillKnowledgeWorld, StatType.SkillLoreNature, StatType.SkillLoreReligion,
                                                      StatType.SkillPerception, StatType.SkillPersuasion, StatType.SkillUseMagicDevice}; //everything except stealth and trickety
            skald_class.IsDivineCaster = false;
            skald_class.IsArcaneCaster = true;
            skald_class.StartingGold = bard_class.StartingGold;
            skald_class.PrimaryColor = bard_class.PrimaryColor;
            skald_class.SecondaryColor = bard_class.SecondaryColor;
            skald_class.RecommendedAttributes = new StatType[] { StatType.Strength, StatType.Charisma, StatType.Constitution };
            skald_class.NotRecommendedAttributes = new StatType[0];
            skald_class.EquipmentEntities = bard_class.EquipmentEntities;
            skald_class.MaleEquipmentEntities = bard_class.MaleEquipmentEntities;
            skald_class.FemaleEquipmentEntities = bard_class.FemaleEquipmentEntities;
            skald_class.ComponentsArray =  bard_class.ComponentsArray;
            skald_class.StartingItems = bard_class.StartingItems;
            createSkaldProgression();
            skald_class.Progression = skald_progression;

          
            skald_class.Archetypes = new BlueprintArchetype[] {}; //battle scion, spellwarrior?, court poet ? urban skald? herald_of_the_horn?
            Helpers.RegisterClass(skald_class);
            //addToPrestigeClasses(); //to at, mt, ek, dd
            //fixExtraPerformance
        }


        static BlueprintCharacterClass[] getSkaldArray()
        {
            return new BlueprintCharacterClass[] { skald_class };
        }


        static void createSkaldProgression()
        {
            //createInspireRage();
            //createSongOfMarching(); //remove fatigue? increase time until tired?
            //createSongOfStrength();
            //FixDirgeOfDoomToScaleWithSkaldLevels()
            //FixRequirementd of Barbarian rage powers to allow skald to use them
            //createSpellKenning();
            //createLoreMaster (+1 to knowledge at 7, 13, 19 ?)
            //createSkaldDamageReduction() ?keep barbarian one?
            //createSongOfTheFallen();


            createVersatilePerformance();
            createWellVersed();
            createPerfromanceResource();

            skald_progression = Helpers.CreateProgression("SkaldProgression",
                           skald_class.Name,
                           skald_class.Description,
                           "",
                           skald_class.Icon,
                           FeatureGroup.None);
            skald_progression.Classes = getSkaldArray();

            skald_proficiencies = library.CopyAndAdd<BlueprintFeature>(Bloodrager.bloodrager_proficiencies.AssetGuid,
                                                                            "SkaldProficiencies",
                                                                            "");
            skald_proficiencies.SetName("Skald Proficiencies");
            skald_proficiencies.SetDescription("A skald is proficient with all simple and martial weapons, light and medium armor, and shields (except tower shields). A skald can cast skald spells while wearing light or medium armor and even using a shield without incurring the normal arcane spell failure chance. ");

            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");
            fast_movement = Bloodrager.fast_movement;
            uncanny_dodge = Bloodrager.uncanny_dodge;
            improved_uncanny_dodge = Bloodrager.improved_uncanny_dodge;

            var cantrips = library.CopyAndAdd<BlueprintFeature>("4f422e8490ec7d94592a8069cce47f98", //bard cantrips
                                                                     "SkaldCantripsFeature",
                                                                     ""
                                                                     );
            cantrips.SetDescription("Skalds learn a number of cantrips, or 0-level spells. These spells are cast like any other spell, but they do not consume any slots and may be used again.");
            cantrips.ReplaceComponent<BindAbilitiesToClass>(c => { c.CharacterClass = skald_class; });





            skald_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, skald_proficiencies, fast_movement, detect_magic, cantrips,
                                                                                        give_performance_resource,
                                                                                        library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                        library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")),  // touch calculate feature};
                                                                    Helpers.LevelEntry(2, versatile_performance, well_versed),
                                                                    Helpers.LevelEntry(3), //rage power
                                                                    Helpers.LevelEntry(4, uncanny_dodge),
                                                                    Helpers.LevelEntry(5), //spell kenning
                                                                    Helpers.LevelEntry(6), //rage power, song of strength
                                                                    Helpers.LevelEntry(7, versatile_performance), //loremaster
                                                                    Helpers.LevelEntry(8, improved_uncanny_dodge),
                                                                    Helpers.LevelEntry(9), //rage power, dr
                                                                    Helpers.LevelEntry(10), //dirge od doom
                                                                    Helpers.LevelEntry(11), //spell kenning
                                                                    Helpers.LevelEntry(12, versatile_performance), //rage power
                                                                    Helpers.LevelEntry(13),
                                                                    Helpers.LevelEntry(14), //dr song of the fallen
                                                                    Helpers.LevelEntry(15), //rage power
                                                                    Helpers.LevelEntry(16),
                                                                    Helpers.LevelEntry(17, versatile_performance), //spell kenning
                                                                    Helpers.LevelEntry(18), //rage power
                                                                    Helpers.LevelEntry(19), //dr
                                                                    Helpers.LevelEntry(20)
                                                                    };

            skald_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { skald_proficiencies, detect_magic, cantrips };
            skald_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(versatile_performance, versatile_performance, versatile_performance, versatile_performance),
                                                         Helpers.CreateUIGroup(fast_movement, uncanny_dodge, improved_uncanny_dodge)
                                                        };
        }



        static internal void createVersatilePerformance()
        {
            versatile_performance = library.CopyAndAdd<BlueprintFeatureSelection>("94e2cd84bf3a8e04f8609fe502892f4f", "SkaldVersatilePerformance", "");
            versatile_performance.SetName("Skald Talent");
            versatile_performance.SetDescription("As a skald gains experience, she learns a number of talents that aid her and confound her foes. At 2nd level, a skald gains a rogue talent, as the rogue class feature of the same name. At 6th level and every 4 levels thereafter, the skald gains an additional rogue talent. A skald cannot select a rogue talent that modifies the sneak attack ability.");
        }

        static internal void createWellVersed()
        {
            well_versed = library.CopyAndAdd<BlueprintFeature>("8f4060852a4c8604290037365155662f", "SkaldWelLVersed", "");
            well_versed.SetDescription("At 2nd level, the skald becomes resistant to the bardic performance of others, and to sonic effects in general. The bard gains a +4 bonus on saving throws made against bardic performance, sonic, and language-dependent effects.");
        }


        static internal void createPerfromanceResource()
        {
            performance_resource = library.Get<BlueprintAbilityResource>("e190ba276831b5c4fa28737e5e49e6a6");
            var amount = Helpers.GetField(performance_resource, "m_MaxAmount");
            BlueprintCharacterClass[] classes = (BlueprintCharacterClass[])Helpers.GetField(amount, "Class");
            classes = classes.AddToArray(skald_class);
            Helpers.SetField(amount, "Class", classes);
            Helpers.SetField(performance_resource, "m_MaxAmount", amount);

            give_performance_resource = library.CopyAndAdd<BlueprintFeature>("b92bfc201c6a79e49afd0b5cfbfc269f", "SkaldPerformanceResourceFact", "");
            give_performance_resource.ReplaceComponent<IncreaseResourcesByClass>(c => { c.CharacterClass = skald_class;});

            //extra performance feat is fixed automatically
        }


        static BlueprintSpellbook createSkaldSpellbook()
        {
            var bard_class = library.TryGet<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");
            var skald_spellbook = Helpers.Create<BlueprintSpellbook>();
            skald_spellbook.name = "SkaldSpellbook";
            library.AddAsset(skald_spellbook, "");
            skald_spellbook.Name = skald_class.LocalizedName;
            skald_spellbook.SpellsPerDay = bard_class.Spellbook.SpellsPerDay;

            skald_spellbook.SpellsKnown = bard_class.Spellbook.SpellsKnown;
            skald_spellbook.Spontaneous = true;
            skald_spellbook.IsArcane = true;
            skald_spellbook.AllSpellsKnown = false;
            skald_spellbook.CanCopyScrolls = false;
            skald_spellbook.CastingAttribute = StatType.Charisma;
            skald_spellbook.CharacterClass = skald_class;
            skald_spellbook.CasterLevelModifier = 0;
            skald_spellbook.CantripsType = CantripsType.Cantrips;
            skald_spellbook.SpellsPerLevel = 0;
            skald_spellbook.SpellList = bard_class.Spellbook.SpellList;
           
            return skald_spellbook;
        }

    }
}
