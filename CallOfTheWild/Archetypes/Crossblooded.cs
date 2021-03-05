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
    public class Crossblooded
    {
        public static BlueprintArchetype archetype;
        public static Dictionary<BlueprintProgression, BlueprintProgression> normal_seeker_map = new Dictionary<BlueprintProgression, BlueprintProgression>();
        public static BlueprintFeatureSelection bloodline_selection1;
        public static BlueprintFeatureSelection bloodline_selection2;

        public static BlueprintFeature drawbacks;
        public static BlueprintSpellbook spellbook;

        static BlueprintFeature[] selected_features;
        static BlueprintFeature[] selected_spells;

        static LibraryScriptableObject library => Main.library;


        public static void create()
        {
            var sorcerer = library.Get<BlueprintCharacterClass>("b3a505fb61437dc4097f43c3f8f9a4cf");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "CrossbloodedSorcererArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Crossblooded");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "A crossblooded bloodline combines the powers of two distinct heritages. In most cases, sorcerers with this bloodline are the offspring of two sorcerers from different ancestries, but occasionally a crossblooded sorcerer arises from the conjunction of other powers. A draconic sorcerer who is also the culmination of a great destiny, an abyssal sorcerer from a family that dealt with devils, and an arcane sorcerer raised from birth by fey are all possible sources for crossblooded bloodlines.");
            });
            Helpers.SetField(archetype, "m_ParentClass", sorcerer);
            library.AddAsset(archetype, "");

            createSelectedMarkers();
            createDrawbacks();
            createCombinedBloodlines();
            archetype.ReplaceSpellbook = spellbook;

            var bloodlines = library.Get<BlueprintFeatureSelection>("24bef8d1bee12274686f6da6ccbc8914");

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, bloodlines) };
            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, bloodline_selection1, bloodline_selection2, drawbacks) };

            sorcerer.Archetypes = sorcerer.Archetypes.AddToArray(archetype);
            sorcerer.Progression.UIDeterminatorsGroup = sorcerer.Progression.UIDeterminatorsGroup.AddToArray(bloodline_selection1, bloodline_selection2, drawbacks);

            addToPrestigeClasses();
        }




        static void createSelectedMarkers()
        {
            selected_features = new BlueprintFeature[5];
            selected_spells = new BlueprintFeature[9];
            var power_levels = new int[] { 1, 3, 9, 15, 20 };
            for (int i = 0; i < selected_features.Length; i++)
            {
                selected_features[i] = Helpers.CreateFeature($"CrossbloddedSorcererSelectedPower{i}", $"Bloodline Power ({power_levels[i]}{Common.getNumExtension(power_levels[i])} Level)", "", "", null, FeatureGroup.None);
                selected_features[i].HideInUI = true;
                selected_features[i].HideInCharacterSheetAndLevelUp = true;
                selected_features[i].HideNotAvailibleInUI = true;
                selected_features[i].IsClassFeature = true;
            }


            for (int i = 0; i < selected_spells.Length; i++)
            {
                selected_spells[i] = Helpers.CreateFeature($"CrossbloddedSorcererSelectedSpell{i}", $"Bloodline Spell ({i + 1}{Common.getNumExtension(i + 1)} Level)", "", "", null, FeatureGroup.None);
                selected_spells[i].HideInUI = true;
                selected_spells[i].HideInCharacterSheetAndLevelUp = true;
                selected_spells[i].HideNotAvailibleInUI = true;
                selected_spells[i].IsClassFeature = true;
            }
        }


        static void createCombinedBloodlines()
        {
            var bloodlines = library.Get<BlueprintFeatureSelection>("24bef8d1bee12274686f6da6ccbc8914");
            bloodline_selection1 = library.CopyAndAdd(bloodlines, "CrossbloodedBloodlinesSelection", "");

            for (int i = 0; i < bloodline_selection1.AllFeatures.Length; i++)
            {
                bloodline_selection1.AllFeatures[i] = createCrossbloodedBloodline(bloodline_selection1.AllFeatures[i] as BlueprintProgression);
            }

            bloodline_selection1.SetNameDescription("Crossblooded bloodline",
                                                    "A crossblooded sorcerer selects two different bloodlines. The sorcerer may gain access to the skills, feats, and some of the powers of both bloodlines she is descended from, but at the cost of reduced mental clarity and choice (see Drawbacks).\n"
                                                    + "At 1st, 3rd, 9th, 15th, and 20th levels, a crossblooded sorcerer gains one of the two new bloodline powers available to her at that level.\n"
                                                    + "A crossblooded sorcerer may select her bonus spells from either of her bloodlines");
            bloodline_selection2 = library.CopyAndAdd(bloodline_selection1, "CrossbloodedBloodlinesSelection2", "");
        }


        static void createDrawbacks()
        {
            drawbacks = Helpers.CreateFeature("CrossbloodedDrawbacksFeature",
                                              "Drawbacks",
                                              "A crossblooded sorcerer has one fewer spell known at each level than is presented on the sorcerer spells known table. Furthermore, the conflicting urges created by the divergent nature of the crossblooded sorcerer’s dual heritage forces her to constantly take some mental effort just to remain focused on her current situation and needs. This leaves her with less mental resolve to deal with external threats. A crossblooded sorcerer always takes a –2 penalty on Will saves.",
                                              "",
                                              Helpers.GetIcon("eabf94e4edc6e714cabd96aa69f8b207"),
                                              FeatureGroup.None,
                                              Helpers.CreateAddStatBonus(StatType.SaveWill, -2, ModifierDescriptor.UntypedStackable)
                                              );

            spellbook = library.CopyAndAdd<BlueprintSpellbook>("b3db3766a4b605040b366265e2af0e50", "CrossbloodedSorcererSpellbook", "");
            spellbook.Name = Helpers.CreateString(archetype.name + ".Spellbook", archetype.Name + " Sorcerer");

            spellbook.SpellsKnown = Common.createSpellsTable("CrossbloodedSpellsKnownTable", "",
                                       Common.createSpellsLevelEntry(),  //0
                                       Common.createSpellsLevelEntry(0, 1),  //1
                                       Common.createSpellsLevelEntry(0, 1),  //2
                                       Common.createSpellsLevelEntry(0, 2),  //3
                                       Common.createSpellsLevelEntry(0, 2),  //4
                                       Common.createSpellsLevelEntry(0, 3, 1), //5
                                       Common.createSpellsLevelEntry(0, 3, 1), //6
                                       Common.createSpellsLevelEntry(0, 4, 2, 1), //7
                                       Common.createSpellsLevelEntry(0, 4, 2, 1), //8
                                       Common.createSpellsLevelEntry(0, 4, 3, 2, 1), //9
                                       Common.createSpellsLevelEntry(0, 4, 3, 2, 1), //10
                                       Common.createSpellsLevelEntry(0, 4, 4, 3, 2, 1), //11
                                       Common.createSpellsLevelEntry(0, 4, 4, 3, 2, 1), //12
                                       Common.createSpellsLevelEntry(0, 4, 4, 3, 3, 2, 1), //13
                                       Common.createSpellsLevelEntry(0, 4, 4, 3, 3, 2, 1), //14
                                       Common.createSpellsLevelEntry(0, 4, 4, 3, 3, 3, 2, 1), //15
                                       Common.createSpellsLevelEntry(0, 4, 4, 3, 3, 3, 2, 1), //16
                                       Common.createSpellsLevelEntry(0, 4, 4, 3, 3, 3, 2, 2, 1), //17
                                       Common.createSpellsLevelEntry(0, 4, 4, 3, 3, 3, 2, 2, 1), //18
                                       Common.createSpellsLevelEntry(0, 4, 4, 3, 3, 3, 2, 2, 2, 1), //19
                                       Common.createSpellsLevelEntry(0, 4, 4, 3, 3, 3, 2, 2, 2, 2) //20
                                       );
        }

        static void addToPrestigeClasses()
        {
            var sorcerer_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("b3a505fb61437dc4097f43c3f8f9a4cf");

            //add check against arrowsong minstrel archetype for exisitng bard spellbooks
            var selections_to_fix = new BlueprintFeatureSelection[] {Common.EldritchKnightSpellbookSelection,
                                                                     Common.ArcaneTricksterSelection,
                                                                     Common.MysticTheurgeArcaneSpellbookSelection,
                                                                     Common.DragonDiscipleSpellbookSelection,
                                                                    };
            foreach (var s in selections_to_fix)
            {
                foreach (var f in s.AllFeatures)
                {
                    if (f.GetComponents<PrerequisiteClassSpellLevel>().Where(c => c.CharacterClass == sorcerer_class).Count() > 0)
                    {
                        f.AddComponent(Common.prerequisiteNoArchetype(sorcerer_class, archetype));
                    }
                }
            }


            Common.addReplaceSpellbook(Common.EldritchKnightSpellbookSelection, spellbook, "EldritchKnightCrossbloodedSorcerer",
                                       Common.createPrerequisiteClassSpellLevel(sorcerer_class, 3),
                                       Common.createPrerequisiteArchetypeLevel(sorcerer_class, archetype, 1));

            Common.addReplaceSpellbook(Common.ArcaneTricksterSelection, spellbook, "ArcaneTricksterCrossbloodedSorcerer",
                                       Common.createPrerequisiteClassSpellLevel(sorcerer_class, 2),
                                       Common.createPrerequisiteArchetypeLevel(sorcerer_class, archetype, 1));

            Common.addReplaceSpellbook(Common.MysticTheurgeArcaneSpellbookSelection, spellbook, "MysticTheurgeCrossbloodedSorcerer",
                                       Common.createPrerequisiteClassSpellLevel(sorcerer_class, 2),
                                       Common.createPrerequisiteArchetypeLevel(sorcerer_class, archetype, 1));

            Common.addReplaceSpellbook(Common.DragonDiscipleSpellbookSelection, spellbook, "DragonDiscipleCrossbloodedSorcerer",
                                       Common.createPrerequisiteClassSpellLevel(sorcerer_class, 1),
                                       Common.createPrerequisiteArchetypeLevel(sorcerer_class, archetype, 1));
        }

        static BlueprintProgression createCrossbloodedBloodline(BlueprintProgression b)
        {
            List<BlueprintFeature> spells = new List<BlueprintFeature>();
            List<BlueprintFeature> powers = new List<BlueprintFeature>();
            BlueprintFeature skill = b.LevelEntries[0].Features.Where(f => f.name.Contains("ClassSkill")).FirstOrDefault() as BlueprintFeature;
            BlueprintFeature arcana = b.LevelEntries[0].Features.Where(f => f.name.Contains("Arcana")).FirstOrDefault() as BlueprintFeature; 

            foreach (var le in b.LevelEntries)
            {
                foreach (var f in le.Features)
                {
                    if (f.GetComponent<AddKnownSpell>() != null)
                    {
                        spells.Add(f as BlueprintFeature);
                    }
                    else if (f != skill && f != arcana)
                    {
                        powers.Add(f as BlueprintFeature);
                    }           
                }
            }

            //Main.logger.Log(b.name + "; powers: " + powers.Count.ToString() + "; spells: " + spells.Count.ToString());
            for (int i = 0; i < spells.Count; i++)
            {
                var s = Helpers.CreateFeatureSelection(b.name + spells[i].name + "CrossbloddedFeatureSelection",
                                                      spells[i].Name,
                                                      spells[i].Description,
                                                      Helpers.MergeIds(b.AssetGuid, "db20ceda6591475ba6d68c0133c18d58", spells[i].AssetGuid),
                                                      spells[i].Icon,
                                                      FeatureGroup.None);
                s.HideInCharacterSheetAndLevelUp = true;

                var no_spell = Helpers.CreateFeature(b.name + spells[i].name + "NoSpellFeature",
                                              "No Bloodline Spell",
                                              "No Bloodline Spell",
                                              Helpers.MergeIds(b.AssetGuid, "b597eb6818ca4d4db80c69d3a176f7d4", spells[i].AssetGuid),
                                              null,
                                              FeatureGroup.None);
                no_spell.HideInCharacterSheetAndLevelUp = true;
                no_spell.AddComponent(Helpers.PrerequisiteFeature(selected_spells[i]));

                s.AllFeatures = new BlueprintFeature[] { spells[i], no_spell};
                spells[i].AddComponent(Helpers.CreateAddFact(selected_spells[i]));
                spells[i].AddComponent(Helpers.PrerequisiteNoFeature(selected_spells[i], any: true));
                spells[i].AddComponent(Helpers.PrerequisiteFeature(spells[i], any: true));
                spells[i] = s;
                s.Obligatory = false;
            }


            for (int i = 0; i < powers.Count; i++)
            {
                var s = Helpers.CreateFeatureSelection(b.name + powers[i].name + "CrossbloddedFeatureSelection",
                                                      powers[i].Name,
                                                      powers[i].Description,
                                                      Helpers.MergeIds(b.AssetGuid, "b5e3ce19d9fa4cafbf2f158be2ed8933", powers[i].AssetGuid),
                                                      powers[i].Icon,
                                                      FeatureGroup.None);
                s.HideInCharacterSheetAndLevelUp = true;

                var no_power = Helpers.CreateFeature(b.name + powers[i].name + "NoPowerFeature",
                                                              "No Bloodline Power",
                                                              "No Bloodline Power",
                                                              Helpers.MergeIds(b.AssetGuid, "3d9e381a843b4277aad3f80f4b48e445", powers[i].AssetGuid),
                                                              null,
                                                              FeatureGroup.None);
                no_power.HideInCharacterSheetAndLevelUp = true;
                no_power.AddComponent(Helpers.PrerequisiteFeature(selected_features[i]));

                s.AllFeatures = new BlueprintFeature[] { powers[i], no_power };
                powers[i].AddComponent(Helpers.CreateAddFact(selected_features[i]));
                powers[i].AddComponent(Helpers.PrerequisiteNoFeature(selected_features[i], any: true));
                powers[i].AddComponent(Helpers.PrerequisiteFeature(powers[i], any: true));
                powers[i] = s;
            }

            var crossblooded_bloodline = library.CopyAndAdd(b, "Crossblooded" + b.name, Helpers.MergeIds(b.AssetGuid, "b983f40038634f5db1d8a19ed3229223"));


            crossblooded_bloodline.LevelEntries = new LevelEntry[]
            {
                Helpers.LevelEntry(1, skill, arcana, powers[0]),
                Helpers.LevelEntry(3, powers[1], spells[0]),
                Helpers.LevelEntry(5, spells[1]),
                Helpers.LevelEntry(7, spells[2]),
                Helpers.LevelEntry(9, powers[2], spells[3]),
                Helpers.LevelEntry(11, spells[4]),
                Helpers.LevelEntry(13, spells[5]),
                Helpers.LevelEntry(15, powers[3], spells[6]),
                Helpers.LevelEntry(17, spells[7]),
                Helpers.LevelEntry(19, spells[8]),
                Helpers.LevelEntry(20, powers[4]),
            };
            b.AddComponent(Helpers.Create<NewMechanics.FeatureReplacement>(a => a.replacement_feature = crossblooded_bloodline));
            crossblooded_bloodline.UIGroups = new UIGroup[] { Helpers.CreateUIGroup(powers), Helpers.CreateUIGroup(spells) };

            return crossblooded_bloodline;
        }

    }

}