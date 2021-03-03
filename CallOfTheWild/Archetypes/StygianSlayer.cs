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
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
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
    public class StygianSlayer
    {
        static public BlueprintArchetype archetype;
        static public BlueprintFeature spellcasting;
        static public BlueprintFeature proficiencies;
        static public BlueprintSpellbook spellbook;

        static LibraryScriptableObject library => Main.library;

        internal static void create()
        {
            var slayer_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("c75e0971973957d4dbad24bc7957e4fb");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "StygianSlayerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Stygian Slayer");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "A stygian slayer crawls out of the darkest shadows to strike fear into the hearts of civilized folk. He’s a merciless killer who can control a sliver of magic, allowing him to arrive unseen, commit murder, and depart without detection.");
            });
            Helpers.SetField(archetype, "m_ParentClass", slayer_class);
            library.AddAsset(archetype, "");

            var slayer_talent2 = library.Get<BlueprintFeatureSelection>("04430ad24988baa4daa0bcd4f1c7d118");
            var slayer_talent6 = library.Get<BlueprintFeatureSelection>("43d1b15873e926848be2abf0ea3ad9a8");
            var slayer_talent10 = library.Get<BlueprintFeatureSelection>("913b9cf25c9536949b43a2651b7ffb66");
            var slayer_proficiencies = library.Get<BlueprintFeature>("41cd5ff7ad1bc5848906e050b06d02dc");
            var simple_proficiency = library.Get<BlueprintFeature>("e70ecf1ed95ca2f40b754f1adb22bbdd");
            var martial_proficiency = library.Get<BlueprintFeature>("203992ef5b35c864390b4e4a1e200629");
            var light_armor_proficiency = library.Get<BlueprintFeature>("6d3728d4e9c9898458fe5e9532951132");
            var proficiencies = library.CopyAndAdd(slayer_proficiencies, "StygianSlayerProficiencies", "");
            proficiencies.ReplaceComponent<AddFacts>(a => a.Facts = new BlueprintUnitFact[] { light_armor_proficiency, simple_proficiency, martial_proficiency });
            proficiencies.SetNameDescription("Stygian Slayer Proficiencies", 
                                             "A stygian slayer is proficient with light armor, but not with medium armor, heavy armor, or any kind of shield (including tower shields).");

            createSpellcasting();
            archetype.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, slayer_proficiencies),
                                                          Helpers.LevelEntry(4, slayer_talent2),
                                                          Helpers.LevelEntry(10, slayer_talent10),
                                                          Helpers.LevelEntry(16, slayer_talent10)
                                                        };


            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, proficiencies),
                                                       Helpers.LevelEntry(4, spellcasting),
                                                    };
            archetype.ChangeCasterType = true;
            archetype.IsArcaneCaster = true;
            archetype.ReplaceSpellbook = spellbook;
            slayer_class.Progression.UIDeterminatorsGroup.AddToArray(proficiencies);
            //slayer_class.Progression.UIDeterminatorsGroup.AddToArray(spellcasting);
            slayer_class.Archetypes = slayer_class.Archetypes.AddToArray(archetype);

            addToPrestigeClasses();
        }



        static void addToPrestigeClasses()
        {
            Common.addReplaceSpellbook(Common.EldritchKnightSpellbookSelection, spellbook, "EldritchKnightStygianSlayer",
                                        Common.createPrerequisiteClassSpellLevel(archetype.GetParentClass(), 3));
            Common.addReplaceSpellbook(Common.ArcaneTricksterSelection, spellbook, "ArcaneTricksterStygianSlayer",
                                        Common.createPrerequisiteClassSpellLevel(archetype.GetParentClass(), 2));
            Common.addReplaceSpellbook(Common.MysticTheurgeArcaneSpellbookSelection, spellbook, "MysticTheurgeStygianSlayer",
                                        Common.createPrerequisiteClassSpellLevel(archetype.GetParentClass(), 2));
        }


        static void createSpellcasting()
        {
            var wizard = library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
            spellbook = library.CopyAndAdd<BlueprintSpellbook>("762858a4a28eaaf43aa00f50441d7027", "StygianSlayerSpellbook", "");//ranger spellbook
            spellbook.IsArcane = true;
            spellbook.CharacterClass = archetype.GetParentClass();
            spellbook.Name = Helpers.CreateString("StygianSlayerSpellbook.Name", archetype.Name);
            spellbook.AllSpellsKnown = false;
            spellbook.CanCopyScrolls = true;
            spellbook.CastingAttribute = StatType.Intelligence;

            spellbook.SpellList = Common.combineSpellLists("StygianSlayerSpelllist",
                                                           (spell, spelllist, lvl) =>
                                                           {
                                                               return lvl <= 4 && lvl > 0
                                                                      && (spell.School == SpellSchool.Illusion
                                                                          || spell == NewSpells.obscuring_mist
                                                                          || spell == NewSpells.barrow_haze
                                                                          );
                                                           },
                                                           wizard.Spellbook.SpellList);

            spellcasting = Helpers.CreateFeature("StygianSlayerSpellcasting",
                                                 "Spell Use",
                                                 "Beginning at 4th level, a stygian slayer gains the ability to cast a small number of arcane spells. He can only cast wizard spells of the illusion school of spell level 1 through 4th, obscuring mist and barrow haze. A stygian slayer must choose and prepare his spells in advance.\n"
                                                 + "To prepare or cast a spell, a stygian slayer must have an Intelligence score equal to at least 10 + the spell level. The Difficulty Class for a saving throw against a stygian slayer’s spell is 10 + the spell level + the stygian slayer’s Intelligence modifier.\n"
                                                 + "A stygian slayer can cast only a certain number of spells of each spell level per day. Her base daily spell allotment is the same as the ranger class.\n"
                                                 + "Through 3rd level, a stygian slayer has no caster level. At 4th level and higher, his caster level is equal to his stygian slayer level – 3.\n"
                                                 + "Stygian slayer learns, prepares, and casts spells exactly as a wizard does, but does not gain additional spells known each time he gains a slayer level with this archetype.\n"
                                                 + "A stygian slayer can cast his spells while wearing light armor without incurring the normal arcane spell failure chance. Like any other arcane spellcaster, a stygian slayer wearing medium or heavy armor or using a shield incurs a chance of arcane spell failure if the spell in question has a somatic component.",
                                                 "",
                                                 Helpers.GetIcon("55edf82380a1c8540af6c6037d34f322"),
                                                 FeatureGroup.None,
                                                 Common.createArcaneArmorProficiency(ArmorProficiencyGroup.Light)
                                                 );
        }

    }
}

