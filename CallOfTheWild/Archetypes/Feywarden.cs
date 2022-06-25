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
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild.Archetypes
{
    public class Feywarden
    {
        static public BlueprintArchetype archetype;

        static public BlueprintFeature fey_touched; //+fey thoughts
        static public BlueprintFeature resist_natures_lure;
        static public BlueprintFeature fey_magic;
        static public BlueprintFeatureSelection[] fey_magic_extra_spell;
        static public BlueprintSpellbook spellbook;
        static public BlueprintUnitProperty ranger_casting_stat_property;

        static LibraryScriptableObject library => Main.library;

        static public void create()
        {
            var ranger_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("cda0615668a6df14eb36ba19ee881af6");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "FeywardenArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Feywarden");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "While most rangers have a spiritual connection to the natural world, some have a stronger connection to the First World and its denizens: the fey. Many of these feywardens have had some significant encounter with fey at some point in their lives, though some merely share the creatures’ spontaneity and wildly shifting emotions. ");
            });
            Helpers.SetField(archetype, "m_ParentClass", ranger_class);
            library.AddAsset(archetype, "");

            createSpellbook();
            createFeyMagic();
            

            resist_natures_lure = library.Get<BlueprintFeature>("ad6a5b0e1a65c3540986cf9a7b006388"); //description is already updated by hunter class

            archetype.ReplaceClassSkills = true;
            archetype.ReplaceSpellbook = spellbook;
            archetype.ChangeCasterType = true;
            archetype.IsDivineCaster = true;
            archetype.ClassSkills = new StatType[] { StatType.SkillPersuasion, StatType.SkillLoreNature, StatType.SkillStealth, StatType.SkillThievery, StatType.SkillPerception, StatType.SkillKnowledgeArcana, StatType.SkillUseMagicDevice };
            archetype.FortitudeSave = library.Get<BlueprintStatProgression>("dc0c7c1aba755c54f96c089cdf7d14a3"); //low
            archetype.WillSave = library.Get<BlueprintStatProgression>("ff4662bde9e75f145853417313842751"); //low

            var endurance = library.Get<BlueprintFeature>("54ee847996c25cd4ba8773d7b8555174");
            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(3, endurance) };


            fey_touched = Helpers.CreateFeature("FeytouchedFeyWardenFeature",
                                                "Feytouched",
                                                "Any of the feywarden’s class abilities that make calculations based on her Wisdom (including bonus feats with DCs or uses per day, but not Wisdom-based skills or Will saving throws) are instead based on her Charisma. Additionally, a feywarden’s base Will save bonus from her ranger levels is equal to 2 + ½ her ranger level, but her base Fortitude save bonus is equal to ⅓ her ranger level.\n"
                                                + "The feywarden gains Trickery and Use Magic Device as class skills, in place of Athletics and Knowledge (World).",
                                                "",
                                                Helpers.GetIcon("e8445256abbdc45488c2d90373f7dae8"), //bloodline fey
                                                FeatureGroup.None
                                                );

            fixRangerAbilitiesScaling();
            archetype.AddFeatures = new LevelEntry[] {    Helpers.LevelEntry(1, fey_touched),
                                                          Helpers.LevelEntry(3, resist_natures_lure),
                                                          Helpers.LevelEntry(4, fey_magic),
                                                          Helpers.LevelEntry(7, fey_magic_extra_spell[0]),
                                                          Helpers.LevelEntry(10, fey_magic_extra_spell[1]),
                                                          Helpers.LevelEntry(13, fey_magic_extra_spell[2]),
                                                          Helpers.LevelEntry(16, fey_magic_extra_spell[3]),
                                                       };

            ranger_class.Progression.UIGroups = ranger_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(fey_magic_extra_spell.AddToArray(resist_natures_lure, fey_magic)));
            ranger_class.Progression.UIDeterminatorsGroup = ranger_class.Progression.UIDeterminatorsGroup.AddToArray(fey_touched);
            ranger_class.Archetypes = ranger_class.Archetypes.AddToArray(archetype);

            //need to create different progression for mt
            var ranger_mt = library.Get<BlueprintProgression>("a823e7aa48bbec24f87f4a92a3ac0aa2");
            ranger_mt.AddComponent(Common.prerequisiteNoArchetype(archetype));

            Common.addMTDivineSpellbookProgression(archetype.GetParentClass(), spellbook, "MysticTheurgeFeyWardenProgression",
                                                    Common.createPrerequisiteArchetypeLevel(archetype, 1),
                                                    Common.createPrerequisiteClassSpellLevel(archetype.GetParentClass(), 2)
                                                    );
        }


        static void fixRangerAbilitiesScaling()
        {
            ranger_casting_stat_property = NewMechanics.CastingStatPropertyGetter.createProperty("RangerCastingStatProperty", "", StatType.Wisdom, archetype.GetParentClass());

            var hunters_bond_ability = library.Get<BlueprintAbility>("cd80ea8a7a07a9d4cb1a54e67a9390a5");
            hunters_bond_ability.ReplaceComponent<ContextRankConfig>(c =>
            {
                Helpers.SetField(c, "m_BaseValueType", ContextRankBaseValueType.CustomProperty);
                Helpers.SetField(c, "m_CustomProperty", ranger_casting_stat_property);
            }
            );


            var master_hunter = library.Get<BlueprintFeature>("9d53ef63441b5d84297587d75f72fc17");
            master_hunter.RemoveComponents<BindAbilitiesToClass>();
            master_hunter.RemoveComponents<ReplaceCasterLevelOfAbility>();
            master_hunter.RemoveComponents<ReplaceAbilitiesStat>();

            var master_hunter_ability = library.Get<BlueprintAbility>("8a57e1072da4f6f4faaa55b7b7dc633c");
            master_hunter_ability.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClassesWithProperty(new BlueprintCharacterClass[] { archetype.GetParentClass() }, ranger_casting_stat_property));

            //fix stromwreden weapon buff resource
            var stromwarden_weapon_ability_resource = library.Get<BlueprintAbilityResource>("6bc707c120d6d38498ccb70767d55a69");
            fey_touched.AddComponents(Helpers.Create<IncreaseResourceAmountBySharedValue>(i =>
                                    {
                                        i.Decrease = true;
                                        i.Resource = stromwarden_weapon_ability_resource;
                                        i.Value = Helpers.CreateContextValue(AbilityRankType.Default);
                                    }
                                    ),
                                    Helpers.Create<IncreaseResourceAmountBySharedValue>(i =>
                                    {
                                        i.Resource = stromwarden_weapon_ability_resource;
                                        i.Value = Helpers.CreateContextValue(AbilityRankType.StatBonus);
                                    }
                                    ),
                                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, type: AbilityRankType.StatBonus, stat: StatType.Charisma),
                                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, stat: StatType.Wisdom),
                                    Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Wisdom),
                                    Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Charisma)

            );
            //warpriest blessing have 3 + 1/2 level so no need to change
        }


        static void createSpellbook()
        {
            spellbook = library.CopyAndAdd<BlueprintSpellbook>("762858a4a28eaaf43aa00f50441d7027", "FeywardenSpellbook", "");
            spellbook.Name = Helpers.CreateString("FeywardenSpellbook.Name", archetype.Name);
            spellbook.AllSpellsKnown = false;
            spellbook.CastingAttribute = StatType.Charisma;
            spellbook.CanCopyScrolls = false;
            spellbook.Spontaneous = true;
            spellbook.SpellsPerLevel = 0;
            spellbook.SpellsKnown = Common.createSpontaneousHalfCasterSpellsKnownDelayed("FeywardenSpellKnownTable", "");
            spellbook.SpellsPerDay = Common.createSpontaneousHalfCasterSpellsPerDayDelayed("FeywardenSpellPerDayTable", "");
        }


        static void createFeyMagic()
        {
            fey_magic = Helpers.CreateFeature("FeyWardenFeyMagic",
                                              "Fey Magic",
                                              "Beginning at 4th level, the feywarden’s magic has been influenced by the whimsical nature of the fey. A feywarden gains the ability to cast a small number of divine spells drawn from the ranger spell list. To learn or cast a spell, a feywarden must have a Charisma score equal to at least 10 + the spell level. She can cast spells she knows without preparing them ahead of time. The saving throw DC against a feywarden’s spell is 10 + the spell level + the feywarden’s Charisma modifier.\n"
                                              + "Like other spellcasters, a feywarden can cast only a certain number of spells of each level per day. Her base daily spell allotment is the same as a bloodrager of her level. In addition, she receives bonus spells per day if she has a high Charisma score.The feywarden does not need to prepare these spells in advance; she can cast any spell he knows at any time, assuming she hasn’t yet used up her allotment of spells per day for the spell’s level.\n"
                                              + "The feywarden’s selection of spells is limited. At 4th level, a feywarden knows two 1st - level spells of her choice. A feywarden gains more spells as she increases in level, with the same number of spells known per level as a bloodrager of her level.\n"
                                              + "Additionally, at 7th level, and every 3 levels thereafter, a feywarden may choose to add a spell from the bard’s spell list to her ranger list as a divine ranger spell of the spell’s level. She learns this spell as a bonus spell.",
                                              "",
                                              Helpers.GetIcon("a9ffe3e08a080c8478bae321e70c7de6"), //fey magic
                                              FeatureGroup.None
                                              );

            fey_magic_extra_spell = new BlueprintFeatureSelection[4];

            var bard_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");
            var combined_spell_list = Common.combineSpellLists("FeywardenFeylMagicSpellList",
                                                               (spell, spell_list, lvl) =>
                                                               {
                                                                   if (lvl > 4)
                                                                   {
                                                                       return false;
                                                                   }
                                                                   if (spellbook.SpellList.Contains(spell)
                                                                       && bard_class.Spellbook.SpellList.GetLevel(spell) != lvl)
                                                                   {
                                                                       return false;
                                                                   }
                                                                   return true;
                                                               },
                                                               bard_class.Spellbook.SpellList
                                                               );
            for (int i = 1; i <= 4; i++)
            {
                fey_magic_extra_spell[i - 1] = Helpers.CreateFeatureSelection($"FeyMagicExtraSpell{i}FeatureSelection",
                                                "Fey Magic Bonus Spell " + $"(Level {i})",
                                                fey_magic.Description,
                                                "",
                                                fey_magic.Icon,
                                                FeatureGroup.None);
                var learn_spell = library.CopyAndAdd<BlueprintParametrizedFeature>("bcd757ac2aeef3c49b77e5af4e510956", $"FeyMagicExtraSpell{i}ParametrizedFeature", "");
                learn_spell.SpellLevel = i;
                learn_spell.SpecificSpellLevel = true;
                learn_spell.SpellLevelPenalty = 0;
                learn_spell.SpellcasterClass = archetype.GetParentClass();
                learn_spell.SpellList = combined_spell_list;
                learn_spell.ReplaceComponent<LearnSpellParametrized>(l => { l.SpellList = combined_spell_list; l.SpecificSpellLevel = true; l.SpellLevel = i; l.SpellcasterClass = archetype.GetParentClass(); });
                learn_spell.SetName(Helpers.CreateString($"FeyMagicExtraSpell{i}ParametrizedFeature.Name", "Fey Magic Bonus Spell " + $"(Level {i})"));
                learn_spell.SetDescription(fey_magic_extra_spell[i - 1].Description);
                learn_spell.SetIcon(fey_magic_extra_spell[i - 1].Icon);
                fey_magic_extra_spell[i - 1].AllFeatures = new BlueprintFeature[] { learn_spell };
            }


        }

    }
}

