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
using Kingmaker.UnitLogic.Alignments;
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
    public class Arcanist
    {
        static LibraryScriptableObject library => Main.library;
        internal static bool test_mode = false;
        static public BlueprintCharacterClass arcanist_class;
        static public BlueprintProgression arcanist_progression;
        static public BlueprintSpellbook memorization_spellbook;
        static public BlueprintSpellbook arcanist_spellbook;
        static public BlueprintFeature arcanist_proficiencies;
        static public BlueprintFeature arcanist_cantrips;
        static public BlueprintFeature arcanist_spellcasting;

        static public BlueprintAbilityResource arcane_reservoir_resource;
        static public BlueprintAbilityResource arcane_reservoir_partial_resource;
        static public BlueprintFeature arcane_reservoir;
        static public BlueprintActivatableAbility arcane_reservoir_spell_dc_boost;
        static public BlueprintActivatableAbility arcane_reservoir_caster_level_boost;
        static public BlueprintFeature consume_spells;

        static public BlueprintFeatureSelection arcane_exploits;
        static public BlueprintFeatureSelection arcane_exploits_wizard;
        static public BlueprintFeature arcane_reservoir_wizard;
        static public BlueprintFeature greater_arcane_exploits;
        static public BlueprintFeature quick_study;
        static public BlueprintFeature quick_study_wizard;
        static public BlueprintFeature acid_jet;
        static public BlueprintFeature lingering_acid;
        static public BlueprintFeature arcane_barrier;
        
        static public BlueprintFeature arcane_weapon;
        static public ActivatableAbilityGroup arcane_weapon_group = ActivatableAbilityGroupExtension.ArcanistArcaneWeapon.ToActivatableAbilityGroup();
        static public BlueprintFeature energy_shield;
        static public BlueprintFeature energy_absorption;
        static public Dictionary<DamageEnergyType, BlueprintBuff> energy_absorption_buffs = new Dictionary<DamageEnergyType, BlueprintBuff>();
        static public BlueprintFeature dimensional_slide;
        static public BlueprintFeature familiar;
        static public BlueprintFeature feral_shifting;
        static public BlueprintFeature flame_arc;
        static public BlueprintFeature burning_flame;
        static public BlueprintFeature force_strike;
        static public BlueprintFeature holy_water_jet;
        static public BlueprintFeature ice_missile;
        static public BlueprintFeature icy_tomb;
        static public BlueprintFeature lightning_lance;
        static public BlueprintFeature dancing_electricity;
        static public BlueprintFeatureSelection metamagic_knowledge;
        static public BlueprintFeatureSelection greater_metamagic_knowledge;
        static public BlueprintFeature metamixing;
        static public BlueprintBuff metamixing_buff;
        static public BlueprintFeature potent_magic;
        
        static public BlueprintFeature sonic_blast;
        static public BlueprintFeature spell_resistance;
        static public BlueprintFeature greater_spell_resistance;
        static public BlueprintFeature wooden_flesh;
        static public BlueprintFeature swift_consume;
        static public BlueprintFeature shift_caster;

        static public BlueprintFeature magical_supremacy;
        static public BlueprintBuff magical_supremacy_buff;

        static public BlueprintFeature extra_reservoir;
        static public BlueprintFeatureSelection extra_arcane_exploit;

        static public BlueprintArchetype school_savant_archetype;
        static public BlueprintArchetype blood_arcanist_archetype;
        static public BlueprintArchetype unlettered_arcanist_archetype;

        static public BlueprintFeatureSelection school_focus;
        static public BlueprintFeatureSelection bloodline_selection;
        static public BlueprintParametrizedFeature new_arcana_blood_arcanist;
        static public BlueprintSpellbook unlettered_arcanist_prepared_spellbook;
        static public BlueprintSpellbook unlettered_arcanist_spontaneous_spellbook;
        static public BlueprintFeature unlettered_arcanist_familiar;
        static public BlueprintFeature unlettered_arcanist_spell_casting;
        static public BlueprintFeature unlettered_arcanist_cantrips;

        static public BlueprintArchetype occultist;
        static public BlueprintFeature perfect_summoner;
        static public BlueprintFeature[] occultist_summon_monster = new BlueprintFeature[9];
        static public BlueprintSummonPool occultist_summon_pool;

        static public BlueprintBuff dc_buff, cl_buff;

        static public BlueprintArchetype exploiter_wizard_archetype;
        static public BlueprintArchetype collegiate_arcanist;
        static public BlueprintFeature collegiate_initiate_alignment;
        static public BlueprintFeatureSelection halcyon_spell_lore;
        static public BlueprintFeatureSelection collegiate_initiate_bonus_feat;
        static public BlueprintSpellList halcyon_lore_spell_list;

        static public BlueprintFeatureSelection school_understanding;
        static public BlueprintFeature item_bond;


        internal static void createArcanistClass()
        {
            Main.logger.Log("Arcanist class test mode: " + test_mode.ToString());
            var wizard_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");

            arcanist_class = Helpers.Create<BlueprintCharacterClass>();
            arcanist_class.name = "ArcanistClass";
            library.AddAsset(arcanist_class, "19c3cf3d51cf4cbf9a136a600c26585a");

            arcanist_class.LocalizedName = Helpers.CreateString("Arcanist.Name", "Arcanist");
            arcanist_class.LocalizedDescription = Helpers.CreateString("Arcanist.Description",
                "Some spellcasters seek the secrets of magic, pursuing the power to make the impossible possible. Others are born with magic in their blood, commanding unbelievable forces as effortlessly as they breathe. Yet still others seek to meld the science of arcane scholars with the natural might of innate casters. These arcanists seek to discover the mysterious laws of magic and through will and expertise bend those forces to their whims. Arcanists are the shapers and tinkers of the arcane world, and no magic can resist their control.\n"
                + "Role: Arcanists are scholars of all things magical. They constantly seek out new forms of magic to discover how they work, and in many cases, to collect the energy of such magic for their own uses. Many arcanists are seen as reckless, more concerned with the potency of magic than the ramifications of unleashing such power."
                );
            arcanist_class.m_Icon = wizard_class.Icon;
            arcanist_class.SkillPoints = wizard_class.SkillPoints;
            arcanist_class.HitDie = DiceType.D6;
            arcanist_class.BaseAttackBonus = wizard_class.BaseAttackBonus;
            arcanist_class.FortitudeSave = wizard_class.ReflexSave;
            arcanist_class.ReflexSave = wizard_class.ReflexSave;
            arcanist_class.WillSave = wizard_class.WillSave;
            arcanist_class.Spellbook = createArcanistSpellbook();
            arcanist_class.ClassSkills = wizard_class.ClassSkills.AddToArray(StatType.SkillUseMagicDevice);
            arcanist_class.IsDivineCaster = false;
            arcanist_class.IsArcaneCaster = true;
            arcanist_class.StartingGold = wizard_class.StartingGold;
            arcanist_class.PrimaryColor = wizard_class.PrimaryColor;
            arcanist_class.SecondaryColor = wizard_class.SecondaryColor;
            arcanist_class.RecommendedAttributes = new StatType[] { StatType.Intelligence, StatType.Charisma };
            arcanist_class.NotRecommendedAttributes = new StatType[0];
            arcanist_class.EquipmentEntities = wizard_class.EquipmentEntities;
            arcanist_class.MaleEquipmentEntities = wizard_class.MaleEquipmentEntities;
            arcanist_class.FemaleEquipmentEntities = wizard_class.FemaleEquipmentEntities;
            arcanist_class.ComponentsArray = wizard_class.ComponentsArray;
            arcanist_class.StartingItems = wizard_class.StartingItems;

            createArcanistProgression();
            arcanist_class.Progression = arcanist_progression;

            Helpers.RegisterClass(arcanist_class);

            createSchoolSavant();
            createBloodArcanist();
            createUnletteredArcanist();
            createOccultist();
            createCollegiateArcanist();

            arcanist_class.Archetypes = new BlueprintArchetype[] { school_savant_archetype, blood_arcanist_archetype, unlettered_arcanist_archetype, occultist, collegiate_arcanist };
            createExploiterWizard();
            createArcanistFeats();
            addToPrestigeClasses();
            createSchoolUnderstanding();
        }


        static void createCollegiateArcanist()
        {
            collegiate_arcanist = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "CollegiateArcanistArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Magaambyan Initiate");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Aspiring students of the Magaambya often spend decades researching arcane magic while learning to follow in the footsteps of the academy’s founder, Old-Mage Jatembe. Those particularly gifted in the art of sculpting spells are sometimes schooled privately in the art of traditional, esoteric, and righteous spells, in the hope that such knowledge will pave the way for the initiate’s acceptance into the school proper as a Magaambyan arcanist.\n"
                                                                                       + "But many initiates find themselves overwhelmed by the extensive training and end up leaving the Magaambya before completing their studies. These spellcasters retain many of the Magaambya’s techniques and philosophical bents, but are not considered to be graduates of the school. Yet they are still respected and valued, for the Magaambya’s staff fully understand that its teachings and the scholastic, often hermetic lifestyle required to master the techniques are not to everyone’s tastes. For the teachers of the Magaambya, there are no truly failed students save those who abandon their philosophy and succumb to the lure of cruelty and evil.\n"
                                                                                       + "Because they hold the Magaambya’s interests close to their hearts but are not hindered by the need to remain close at hand to study or serve as teachers or assistants, Magaambyan initiates often serve as strong supporters beyond the normal reach of the Magaambya. Furthermore, Magaambyan initiates can act immediately against the forces of evil without waiting to be officially sent out into the world.");
            });
            Helpers.SetField(collegiate_arcanist, "m_ParentClass", arcanist_class);
            library.AddAsset(collegiate_arcanist, "");

            createCollegiateInititateAlignment();
            createHalcyonSpellLore();
            collegiate_arcanist.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, arcane_exploits), Helpers.LevelEntry(5, arcane_exploits), Helpers.LevelEntry(9, arcane_exploits), Helpers.LevelEntry(17, arcane_exploits) };

            collegiate_arcanist.AddFeatures = new LevelEntry[20];
            collegiate_initiate_bonus_feat = library.CopyAndAdd<BlueprintFeatureSelection>("d6dd06f454b34014ab0903cb1ed2ade3", "CollegiateInitiateBonusFeatureSelection", "");
            collegiate_initiate_bonus_feat.SetNameDescription("Magaambyan Initiate Bonus Feat",
                                                              "At 5th level, a magaambyan initiate gains a bonus feat. She can choose a metamagic feat, spell focus feat, or any other spellcaster feat. The agaambyan initiate must still meet all prerequisites for a bonus feat, including caster level minimums.");
            for (int i = 0; i < 20; i++)
            {
                collegiate_arcanist.AddFeatures[i] = Helpers.LevelEntry(i + 1, halcyon_spell_lore);
            }
            collegiate_arcanist.AddFeatures[0].Features.Add(collegiate_initiate_alignment);
            //collegiate_arcanist.AddFeatures[4].Features.Add(collegiate_initiate_bonus_feat);

            arcanist_class.Progression.UIDeterminatorsGroup = arcanist_class.Progression.UIDeterminatorsGroup.AddToArray(collegiate_initiate_alignment);
        }


        static void createCollegiateInititateAlignment()
        {
            var feature = Helpers.CreateFeature("CollegiateInitiateAlignmentFeature",
                                                "Good Alignment",
                                                "A Collegiate initiate must be of a good alignment.",
                                                "",
                                                null,
                                                FeatureGroup.None,
                                                Helpers.Create<SpellManipulationMechanics.SpendResourceOnExtraArcanistSpellCast >(s => { s.half_level = true; s.resource = arcane_reservoir_resource; }),
                                                Common.createPrerequisiteAlignment(AlignmentMaskType.Good));
            collegiate_initiate_alignment = Common.featureToSelection(feature, false);
        }


        static void createHalcyonSpellLore()
        {
            var druid_spell_list = library.Get<BlueprintSpellList>("bad8638d40639d04fa2f80a1cac67d6b");
            var cleric_spell_list = library.Get<BlueprintSpellList>("8443ce803d2d31347897a3d85cc32f53");

            halcyon_lore_spell_list = Common.combineSpellLists("HalcyonLoreSpellList",
                                                               (spell, spell_list, lvl) =>
                                                               {
                                                                   if (arcanist_class.Spellbook.SpellList.Contains(spell) 
                                                                       && arcanist_class.Spellbook.SpellList.GetLevel(spell) != lvl)
                                                                   {
                                                                       return false;
                                                                   }
                                                                   if (Witch.witch_class.Spellbook.SpellList.Contains(spell)
                                                                         && Witch.witch_class.Spellbook.SpellList.GetLevel(spell) != lvl)
                                                                   {//for unlettered arcanist/maagambyan arcanist
                                                                       return false;
                                                                   }
                                                                   if (spell_list == cleric_spell_list && (spell.SpellDescriptor & SpellDescriptor.Good) == 0)
                                                                   {
                                                                       return false;
                                                                   }
                                                                   return true;
                                                               },
                                                               druid_spell_list, cleric_spell_list
                                                               );


            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/Wish.png");
            halcyon_spell_lore = Helpers.CreateFeatureSelection("HalcyonSpellLoreFeatureSelection",
                                                                "Halcyon Spell Lore",
                                                                "A Magaambyan initiate’s studies of the philanthropic teachings of Old-Mage Jatembe allow her to cast a limited number of spells per day beyond those she could normally prepare ahead of time. At each class level, she chooses one spell from the druid spell list or one spell with the good descriptor from the cleric spell list. The spell must be of a spell level that she can cast, and cannot be a spell that already appears on her arcanist spell list. A Magaambyan initiate can cast a spell that she has chosen with this ability as if it were on her spell list and prepared by expending a number of points from her arcane reservoir equal to half the spell’s level (minimum 1) and expending a spell slot of the spell’s level.",
                                                                "",
                                                                icon,
                                                                FeatureGroup.None);

            var availability_component = Helpers.Create<SpellManipulationMechanics.SpellRequiringResourceIfCastFromSpecificSpellbook>(r =>
                                                            {
                                                                r.arcanist_spellbook = true;
                                                                r.cost_increasing_facts = new BlueprintUnitFact[] { cl_buff, dc_buff, metamixing_buff };
                                                                r.half_level = true;
                                                                r.resource = arcane_reservoir_resource;
                                                                r.only_from_extra_arcanist_spell_list = true;
                                                            }
                                                            );
            for (int i = 1; i <= 9; i++)
            {
                foreach (var s in halcyon_lore_spell_list.GetSpells(i))
                {
                    if (s.HasVariants)
                    {
                        foreach (var v in s.Variants)
                        {
                            v.AddComponent(availability_component);
                        }                       
                    }
                    else
                    {
                        s.AddComponent(availability_component);
                    }
                }
                var learn_spell = library.CopyAndAdd<BlueprintParametrizedFeature>("bcd757ac2aeef3c49b77e5af4e510956", $"HalcyonSpellLore{i}ParametrizedFeature", "");
                learn_spell.SpellLevel = i;
                learn_spell.SpecificSpellLevel = true;
                learn_spell.SpellLevelPenalty = 0;
                learn_spell.SpellcasterClass = arcanist_class;
                learn_spell.SpellList = halcyon_lore_spell_list;
                learn_spell.ReplaceComponent<LearnSpellParametrized>(Helpers.Create<SpellManipulationMechanics.AddExtraArcanistSpellParametrized>(a => a.spell_list = halcyon_lore_spell_list));
                learn_spell.AddComponents(Common.createPrerequisiteClassSpellLevel(arcanist_class, i)
                                          );
                learn_spell.SetName(Helpers.CreateString($"HalcyonSpellLore{i}.Name", "Halcyon Spell Lore " + $"(Level {i})"));
                learn_spell.SetDescription(halcyon_spell_lore.Description);
                learn_spell.SetIcon(halcyon_spell_lore.Icon);

                halcyon_spell_lore.AllFeatures = halcyon_spell_lore.AllFeatures.AddToArray(learn_spell);
            }
        }

        static void createExploiterWizard()
        {
            var wizard = library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
            exploiter_wizard_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "ExploiterWizardArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Exploiter Wizard");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Contrary to traditional wizardly study, an exploiter wizard forgoes the tried and true methods of arcane focus and arcane schools for the exploits favored by an arcanist. Some wizards regard this blatant exploitation of arcane magic as somehow “cheating,” but most exploiters believe this prejudice is close-minded and overly traditional.");
            });
            Helpers.SetField(exploiter_wizard_archetype, "m_ParentClass", wizard);
            library.AddAsset(exploiter_wizard_archetype, "");

            var school_selection = library.Get<BlueprintFeatureSelection>("5f838049069f1ac4d804ce0862ab5110");
            var arcane_bond = library.Get<BlueprintFeatureSelection>("03a1781486ba98043afddaabf6b7d8ff");


            exploiter_wizard_archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, school_selection, arcane_bond)
                                                       };

            exploiter_wizard_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, arcane_reservoir_wizard, arcane_exploits_wizard),
                                                                                           Helpers.LevelEntry(5, arcane_exploits_wizard),
                                                                                           Helpers.LevelEntry(9, arcane_exploits_wizard),
                                                                                           Helpers.LevelEntry(13, arcane_exploits_wizard),
                                                                                           Helpers.LevelEntry(17, arcane_exploits_wizard)
                                                                                          };

            wizard.Progression.UIDeterminatorsGroup = wizard.Progression.UIDeterminatorsGroup.AddToArray(arcane_reservoir_wizard);
            wizard.Archetypes = wizard.Archetypes.AddToArray(exploiter_wizard_archetype);
        }


        static void addToPrestigeClasses()
        {
            var dragon_disiciple = library.Get<BlueprintCharacterClass>("72051275b1dbb2d42ba9118237794f7c");
            Common.addReplaceSpellbook(Common.EldritchKnightSpellbookSelection, arcanist_class.Spellbook, "EldritchKnightArcanist",
                                       Common.createPrerequisiteClassSpellLevel(arcanist_class, 3),
                                       Common.prerequisiteNoArchetype(arcanist_class, unlettered_arcanist_archetype));
            Common.addReplaceSpellbook(Common.ArcaneTricksterSelection, arcanist_class.Spellbook, "ArcaneTricksterArcanist",
                           Common.createPrerequisiteClassSpellLevel(arcanist_class, 2),
                           Common.prerequisiteNoArchetype(arcanist_class, unlettered_arcanist_archetype));
            Common.addReplaceSpellbook(Common.MysticTheurgeArcaneSpellbookSelection, arcanist_class.Spellbook, "MysticTheurgeArcanist",
                           Common.createPrerequisiteClassSpellLevel(arcanist_class, 2),
                           Common.prerequisiteNoArchetype(arcanist_class, unlettered_arcanist_archetype));
           /* Common.addReplaceSpellbook(Common.DragonDiscipleSpellbookSelection, arcanist_class.Spellbook, "DragonDiscipleArcanist",
                                       Common.createPrerequisiteClassSpellLevel(arcanist_class, 1),
                                       Common.prerequisiteNoArchetype(arcanist_class, unlettered_arcanist_archetype));*/

            Common.addReplaceSpellbook(Common.EldritchKnightSpellbookSelection, unlettered_arcanist_archetype.ReplaceSpellbook, "EldritchKnightUnletteredArcanist",
                           Common.createPrerequisiteClassSpellLevel(arcanist_class, 3),
                           Common.createPrerequisiteArchetypeLevel(arcanist_class, unlettered_arcanist_archetype, 1));
            Common.addReplaceSpellbook(Common.ArcaneTricksterSelection, unlettered_arcanist_archetype.ReplaceSpellbook, "ArcaneTricksterUnletteredArcanist",
                           Common.createPrerequisiteClassSpellLevel(arcanist_class, 2),
                           Common.createPrerequisiteArchetypeLevel(arcanist_class, unlettered_arcanist_archetype, 1));
            Common.addReplaceSpellbook(Common.MysticTheurgeArcaneSpellbookSelection, unlettered_arcanist_archetype.ReplaceSpellbook, "MysticTheurgeUnletteredArcanist",
                           Common.createPrerequisiteClassSpellLevel(arcanist_class, 2),
                           Common.createPrerequisiteArchetypeLevel(arcanist_class, unlettered_arcanist_archetype, 1));

            /*var allowed_bloodlines = dragon_disiciple.GetComponent<PrerequisiteMechanics.PrerequsiteOrAlternative>().alternative_prerequsite;
            Common.addReplaceSpellbook(Common.DragonDiscipleSpellbookSelection, unlettered_arcanist_archetype.ReplaceSpellbook, "DragonDiscipleUnletteredArcanist",
                           Common.createPrerequisiteClassSpellLevel(arcanist_class, 1),
                           Common.createPrerequisiteArchetypeLevel(arcanist_class, unlettered_arcanist_archetype, 1),
                           allowed_bloodlines);

            dragon_disiciple.AddComponent(Helpers.Create<PrerequisiteMechanics.PrerequsiteOrAlternative>(p =>
                                                                                                        {
                                                                                                            p.base_prerequsite = Common.prerequisiteNoArchetype(arcanist_class, unlettered_arcanist_archetype);
                                                                                                            p.alternative_prerequsite = allowed_bloodlines;
                                                                                                        }
                                                                                                        )
                                         );

            //allow arcanist to qualify for dragon disciple
            var prereq_spontaneous = dragon_disiciple.GetComponent<PrerequisiteCasterTypeSpellLevel>();
            dragon_disiciple.ReplaceComponent(prereq_spontaneous,
                                              Helpers.Create<PrerequisiteMechanics.PrerequsiteOrAlternative>(p =>
                                                                                                              {
                                                                                                                  p.base_prerequsite = prereq_spontaneous;
                                                                                                                  p.alternative_prerequsite = Helpers.PrerequisiteClassLevel(arcanist_class, 1);
                                                                                                              }
                                                                                                            )
                                              );*/
        }



        static void createOccultist()
        {
            occultist = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "OccultistArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Occultist");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Not all arcanists peer inward to discern the deepest secrets of magic. Some look outward, connecting with extraplanar creatures and bartering for secrets, power, and favor.");
            });
            Helpers.SetField(occultist, "m_ParentClass", arcanist_class);
            library.AddAsset(occultist, "");

            createOccultistSummoning();
            occultist.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, arcane_exploits), Helpers.LevelEntry(7, arcane_exploits), Helpers.LevelEntry(20, magical_supremacy) };
            occultist.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, occultist_summon_monster[0]),
                                                        Helpers.LevelEntry(3, occultist_summon_monster[1]),
                                                        Helpers.LevelEntry(5, occultist_summon_monster[2]),
                                                        Helpers.LevelEntry(7, occultist_summon_monster[3]),
                                                        Helpers.LevelEntry(9, occultist_summon_monster[4]),
                                                        Helpers.LevelEntry(11, occultist_summon_monster[5]),
                                                        Helpers.LevelEntry(13, occultist_summon_monster[6]),
                                                        Helpers.LevelEntry(15, occultist_summon_monster[7]),
                                                        Helpers.LevelEntry(17, occultist_summon_monster[8]),
                                                        Helpers.LevelEntry(20, perfect_summoner)
                                                    };
            arcanist_class.Progression.UIGroups = arcanist_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(occultist_summon_monster.AddToArray(perfect_summoner)));
        }


        static void createOccultistSummoning()
        {
            perfect_summoner = Helpers.CreateFeature("OccultistPerfectSummonerFeature",
                                                     "Perfect Summoner",
                                                     "At 20th level, an occultist can use her conjurer’s focus without spending points from her arcane reservoir, and the creatures summoned last one day or until dismissed.",
                                                     "",
                                                     Helpers.GetIcon("38155ca9e4055bb48a89240a2055dcc3"),
                                                     FeatureGroup.None
                                                     );
            perfect_summoner.Ranks = 1;

            var mt_feats = new BlueprintFeature[]
            {
                library.Get<BlueprintFeature>("c19f6f34ed0bc364cbdec88b49a54f67"),
                library.Get<BlueprintFeature>("45e466127a8961d40bb3030816ed245b"),
                library.Get<BlueprintFeature>("ea26b3a3acb98074fa34f80fcc4e497d"),
                library.Get<BlueprintFeature>("03168f4f13ff26f429d912085e88baba"),
                library.Get<BlueprintFeature>("00fda605a917fcc4e89612dd31683bdd"),
                library.Get<BlueprintFeature>("9b14b05456142914888a48354a0eec17"),
                library.Get<BlueprintFeature>("667fd017406abd548b89292edd7dbfb7"),
                library.Get<BlueprintFeature>("20d72612311ba914aaba5cc8a4cf312c"),
                library.Get<BlueprintFeature>("f63d23b4e41b3264fa6aa2be8079d28d")
            };

            var description = "An occultist can spend 1 point from her arcane reservoir to cast summon monster I. She can cast this spell as a standard action and the summoned creatures remain for 1 minute per level (instead of 1 round per level). At 3rd level and every 2 levels thereafter, the power of this ability increases by one spell level, allowing her to summon more powerful creatures (to a maximum of summon monster IX at 17th level), at the cost of an additional point from her arcane spell reserve per spell level. An occultist cannot have more than one summon monster spell active in this way at one time. If this ability is used again, any existing summon monster immediately ends.";
            occultist_summon_pool = library.CopyAndAdd<BlueprintSummonPool>("490248a826bbf904e852f5e3afa6d138", "OccultistSummonPool", "");

            for (int i = 0; i < mt_feats.Length; i++)
            {
                List<BlueprintAbility> summon_spells = new List<BlueprintAbility>();
                foreach (var f in mt_feats[i].GetComponent<AddFacts>().Facts)
                {
                    var ability = library.CopyAndAdd<BlueprintAbility>(f.AssetGuid, f.name.Replace("MonsterTactician", "Occultist"), "");
                    ability.ReplaceComponent<AbilityResourceLogic>(a => { a.RequiredResource = arcane_reservoir_resource; a.Amount = i + 1; a.CostIsCustom = true; });
                    ability.AddComponent(Helpers.Create<NewMechanics.ResourseCostCalculatorWithDecreasingFacts>(r => r.cost_reducing_facts = Enumerable.Repeat(perfect_summoner, i+1).ToArray()));
                    foreach (var c in ability.GetComponents<ContextRankConfig>())
                    {
                        if (c.IsBasedOnClassLevel)
                        {
                            var new_c = c.CreateCopy(crc => Helpers.SetField(crc, "m_Class", getArcanistArray()));
                            ability.ReplaceComponent(c, new_c);
                        }
                    }
                    var new_actions = Common.changeAction<ContextActionClearSummonPool>(ability.GetComponent<AbilityEffectRunAction>().Actions.Actions, a => a.SummonPool = occultist_summon_pool);
                    new_actions = Common.changeAction<ContextActionSpawnMonster>(new_actions,
                                                                                  a =>
                                                                                  {
                                                                                      a.SummonPool = occultist_summon_pool;
                                                                                      a.DurationValue = Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.DamageDiceAlternative), DurationRate.Minutes, DiceType.One, a.DurationValue.BonusValue);
                                                                                  }
                                                                                  );
                    ability.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(new_actions));
                    ability.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, ContextRankProgression.MultiplyByModifier, AbilityRankType.DamageDiceAlternative, 
                                                                         feature: perfect_summoner, stepLevel: 60*24 - 20)
                                                                         );
                    summon_spells.Add(ability);
                }

                BlueprintAbility summon_base = null;
                if (summon_spells.Count == 1)
                {
                    summon_base = summon_spells[0];
                }
                else
                {
                    summon_base = Common.createVariantWrapper($"OccultistSummon{i + 1}Base", "", summon_spells.ToArray());
                    summon_base.SetNameDescription("Summon Monster " + Common.roman_id[i + 1], description);
                }

                occultist_summon_monster[i] = Helpers.CreateFeature($"OccultistSummonMonster{i + 1}Feature",
                                                          "Conjurer's Focus: Summon Monster " + Common.roman_id[i + 1],
                                                          description,
                                                          "",
                                                          summon_spells[0].Icon,
                                                          FeatureGroup.None,
                                                          Helpers.CreateAddFact(summon_base)
                                                          );
            }
        }


        static void createSchoolSavant()
        {
            var wizard_class = library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
            school_savant_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "SchoolSavantArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "School Savant");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Some arcanists specialize in a school of magic and trade flexibility for focus. School savants are able to prepare more spells per day than typical arcanists, but their selection is more limited.");
            });
            Helpers.SetField(school_savant_archetype, "m_ParentClass", arcanist_class);
            library.AddAsset(school_savant_archetype, "");

            school_focus = library.CopyAndAdd<BlueprintFeatureSelection>("5f838049069f1ac4d804ce0862ab5110", "SchoolSavantSchoolFocusFeatureSelection", "");
            school_focus.SetNameDescription("School Focus",
                                            "At 1st level, a school savant chooses a school of magic. The arcanist gains the abilities granted by that school, as the arcane school class feature of the wizard, treating her arcanist level as her wizard level for these abilities. She can also further specialize by selecting a subschool. In addition, the arcanist can prepare one additional spell per day of each level she can cast, but this spell must be chosen from the selected school.\n"
                                            + "Finally, the arcanist must select two additional schools of magic as her opposition schools. Whenever she prepares spells from one of her opposition schools, the spell takes up two of her prepared spell slots. ");
            ClassToProgression.addClassToDomains(arcanist_class, new BlueprintArchetype[0], ClassToProgression.DomainSpellsType.SpecialList, school_focus, wizard_class);

            school_savant_archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, arcane_exploits), Helpers.LevelEntry(3, arcane_exploits), Helpers.LevelEntry(7, arcane_exploits) };
            school_savant_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, school_focus) };

            
            arcanist_class.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = wizard_class));
            arcanist_progression.UIDeterminatorsGroup = arcanist_progression.UIDeterminatorsGroup.AddToArray(school_focus);
            wizard_class.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = arcanist_class));
        }


        static void createBloodArcanist()
        {
            var sorcerer_class = library.Get<BlueprintCharacterClass>("b3a505fb61437dc4097f43c3f8f9a4cf");
            blood_arcanist_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "BloodArcanistArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Blood Arcanist");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Though most arcanists possess only a rudimentary innate arcane gift, the blood arcanist has the full power of a bloodline to draw upon.");
            });
            Helpers.SetField(blood_arcanist_archetype, "m_ParentClass", arcanist_class);
            library.AddAsset(blood_arcanist_archetype, "");

            bloodline_selection = library.CopyAndAdd<BlueprintFeatureSelection>("24bef8d1bee12274686f6da6ccbc8914", "BloodArcanistBloodlineSelectionFeatureSelection", "");
            List<BlueprintFeature> bloodlines = new List<BlueprintFeature>();

            bloodline_selection.SetNameDescription("Bloodline",
                                            "A blood arcanist selects one bloodline from those available through the sorcerer bloodline class feature. The blood arcanist gains the bloodline arcana and bloodline powers of that bloodline, treating her arcanist level as her sorcerer level. The blood arcanist does not gain bonus feats, or bonus spells from her bloodline.");
            ClassToProgression.addClassToDomains(arcanist_class, new BlueprintArchetype[] { blood_arcanist_archetype }, ClassToProgression.DomainSpellsType.NoSpells, bloodline_selection, sorcerer_class);

            foreach (var b in bloodline_selection.AllFeatures)
            {
                var bloodline_no_spells_skills = Common.removeEntriesFromProgression(b as BlueprintProgression, "Arcanist" + b.name, f => f.name.Contains("ClassSkill") || f.name.Contains("SpellLevel") /*|| f.name.Contains("NewArcanaSelection")*/);

                bloodlines.Add(bloodline_no_spells_skills);
                b.AddComponent(Helpers.Create<NewMechanics.FeatureReplacement>(f => f.replacement_feature = bloodline_no_spells_skills));
            }

            var new_arcana = library.Get<BlueprintFeatureSelection>("20a2435574bdd7f4e947f405df2b25ce");
            new_arcana_blood_arcanist = library.CopyAndAdd<BlueprintParametrizedFeature>("4a2e8388c2f0dd3478811d9c947bebfb", "BloodleneArcaneNewArcanaBloodArcanistFeature", "");
            new_arcana_blood_arcanist.ReplaceComponent<LearnSpellParametrized>(l => l.SpellcasterClass = arcanist_class);
            foreach (var f in new_arcana.AllFeatures)
            {
                f.AddComponent(Common.prerequisiteNoArchetype(blood_arcanist_archetype));
            }
            new_arcana.AllFeatures = new_arcana.AllFeatures.AddToArray(new_arcana_blood_arcanist);
            new_arcana.Features = new_arcana.Features.AddToArray(new_arcana_blood_arcanist);
            new_arcana_blood_arcanist.SpellcasterClass = arcanist_class;
            bloodline_selection.AllFeatures = bloodlines.ToArray();

            blood_arcanist_archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, arcane_exploits), Helpers.LevelEntry(3, arcane_exploits), Helpers.LevelEntry(9, arcane_exploits), Helpers.LevelEntry(15, arcane_exploits), Helpers.LevelEntry(20, magical_supremacy) };
            blood_arcanist_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, bloodline_selection) };
            arcanist_progression.UIDeterminatorsGroup = arcanist_progression.UIDeterminatorsGroup.AddToArray(bloodline_selection);

            
            var magus = library.Get<BlueprintCharacterClass>("45a4607686d96a1498891b3286121780");
            arcanist_class.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = sorcerer_class));
            sorcerer_class.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = arcanist_class));
            arcanist_class.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = magus));
            magus.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = arcanist_class));

            item_bond.AddComponent(Common.prerequisiteNoArchetype(blood_arcanist_archetype));
        }


        static void createUnletteredArcanist()
        {
            unlettered_arcanist_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "UnletteredArcanistArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Unlettered Arcanist");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Some arcanists store their spells as whispered secrets within familiars instead of on paper.");
            });
            Helpers.SetField(unlettered_arcanist_archetype, "m_ParentClass", arcanist_class);
            library.AddAsset(unlettered_arcanist_archetype, "");
            unlettered_arcanist_familiar = library.CopyAndAdd<BlueprintFeature>(familiar.AssetGuid, "UnletteredArcanistFamiliar", "");
            unlettered_arcanist_familiar.SetDescription("An unlettered arcanist does not keep a spellbook. Instead, she gains a familiar in which she stores her spells as a witch does, though she does not gain a witch’s patron. Treat her arcanist level as her witch level for determining the abilities and benefits granted by the familiar. Anything that would allow an unlettered arcanist to add spells to her spellbook allows her to add spells to her familiar instead.");
            familiar.AddComponent(Common.prerequisiteNoArchetype(arcanist_class, unlettered_arcanist_archetype));

            createUnletteredArcanistSpellcasting();

            unlettered_arcanist_archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, arcanist_cantrips, arcanist_spellcasting, consume_spells)};
            unlettered_arcanist_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, unlettered_arcanist_cantrips, unlettered_arcanist_spell_casting, consume_spells, unlettered_arcanist_familiar) };
            arcanist_progression.UIDeterminatorsGroup = arcanist_progression.UIDeterminatorsGroup.AddToArray(unlettered_arcanist_cantrips, unlettered_arcanist_spell_casting, unlettered_arcanist_familiar);
            unlettered_arcanist_archetype.ReplaceSpellbook = unlettered_arcanist_prepared_spellbook;
        }


        static void createUnletteredArcanistSpellcasting()
        {
            unlettered_arcanist_prepared_spellbook = library.CopyAndAdd<BlueprintSpellbook>(memorization_spellbook.AssetGuid, "UnletteredArcanistPreparedSpellbook", "");

            unlettered_arcanist_prepared_spellbook.Name = Helpers.CreateString("UnletteredArcanistPreparedSpellbook.Name", "Unlettered Arcanist (Prepared)");
            unlettered_arcanist_prepared_spellbook.SpellList = Witch.witch_class.Spellbook.SpellList;

            unlettered_arcanist_spontaneous_spellbook = library.CopyAndAdd<BlueprintSpellbook>(arcanist_spellbook.AssetGuid, "UnletteredArcanistSpontnaeousSpellbook", "");
            unlettered_arcanist_spontaneous_spellbook.Name = Helpers.CreateString("UnletteredArcanistSpontnaeousSpellbook.Name", "Unlettered Arcanist (Spontaneous)");
            unlettered_arcanist_spontaneous_spellbook.SpellList = unlettered_arcanist_prepared_spellbook.SpellList;

            unlettered_arcanist_prepared_spellbook.ReplaceComponent<SpellbookMechanics.CompanionSpellbook>(Helpers.Create<SpellbookMechanics.CompanionSpellbook>(c => c.spellbook = unlettered_arcanist_spontaneous_spellbook));
            unlettered_arcanist_spontaneous_spellbook.ReplaceComponent<SpellbookMechanics.GetKnownSpellsFromMemorizationSpellbook>(Helpers.Create<SpellbookMechanics.GetKnownSpellsFromMemorizationSpellbook>(c => c.spellbook = unlettered_arcanist_prepared_spellbook));

            dc_buff.AddComponents(Helpers.Create<NewMechanics.IncreaseAllSpellsDCForSpecificSpellbook>(i => { i.spellbook = unlettered_arcanist_spontaneous_spellbook; i.Value = Helpers.CreateContextValue(AbilityRankType.Default); }),
                                  Helpers.Create<NewMechanics.SpendResourceOnSpellCast>(s => { s.spellbook = unlettered_arcanist_spontaneous_spellbook; s.resource = arcane_reservoir_resource; })
                                  );
            cl_buff.AddComponents(Helpers.Create<NewMechanics.IncreaseAllSpellsCLForSpecificSpellbook>(i => { i.spellbook = unlettered_arcanist_spontaneous_spellbook; i.Value = Helpers.CreateContextValue(AbilityRankType.Default); }),
                                  Helpers.Create<NewMechanics.SpendResourceOnSpellCast>(s => { s.spellbook = unlettered_arcanist_spontaneous_spellbook; s.resource = arcane_reservoir_resource; })
                                 );


            unlettered_arcanist_spell_casting = Helpers.CreateFeature("WitchSpellsSpellCastingFeature",
                                              "Witch Spells",
                                              "An unlettered arcanist follows a different arcane tradition. She uses the witch spell list instead of the wizard/sorcerer spell list.",
                                              "",
                                              null,
                                              FeatureGroup.None,
                                              Helpers.Create<SpellManipulationMechanics.InitializeArcanistPart>(i => i.spellbook = unlettered_arcanist_spontaneous_spellbook)
                                              );

            var daze = library.Get<BlueprintAbility>("55f14bc84d7c85446b07a1b5dd6b2b4c");
            unlettered_arcanist_cantrips = Common.createCantrips("UnletteredArcanistCantripsFeature",
                                                                 "Cantrips",
                                                                 "Unlettered arcanist uses witch cantrips.",
                                                                 daze.Icon,
                                                                 "532ce6fd6fad4e1d9316785bcc09b02f",
                                                                 arcanist_class,
                                                                 StatType.Intelligence,
                                                                 Witch.witch_class.Spellbook.SpellList.SpellsByLevel[0].Spells.ToArray());
        }



        static void createArcanistFeats()
        {
            extra_reservoir = Helpers.CreateFeature("ExtraReservoirFeature",
                                                    "Extra Reservoir",
                                                    "You gain three more points in your arcane reservoir, and the maximum number of points in your arcane reservoir increases by that amount.\n"
                                                    + "You can take this feat multiple times. Its effects stack.",
                                                    "",
                                                    arcane_reservoir.Icon,
                                                    FeatureGroup.Feat,
                                                    Helpers.Create<IncreaseResourceAmount>(i => { i.Resource = arcane_reservoir_resource; i.Value = 3; }),
                                                    Helpers.Create<IncreaseResourceAmount>(i => { i.Resource = arcane_reservoir_partial_resource; i.Value = 3; }),
                                                    Helpers.PrerequisiteClassLevel(arcanist_class, 1, any: true),
                                                    Common.createPrerequisiteArchetypeLevel(exploiter_wizard_archetype, 1, any: true)
                                                    );

            extra_reservoir.Ranks = 10;
            library.AddFeats(extra_reservoir);

            extra_arcane_exploit = library.CopyAndAdd<BlueprintFeatureSelection>(arcane_exploits.AssetGuid, "ExtraArcaneExploitFeature", "");
            extra_arcane_exploit.SetNameDescription("Extra Arcanist Exploit",
                                                   "You gain one additional arcanist exploit. You must meet the prerequisites for this arcanist exploit.\n"
                                                   + "Special: You can take this feat multiple times. Each time you do, you gain another arcanist exploit.");
            extra_arcane_exploit.AddComponent(Helpers.PrerequisiteFeature(arcane_exploits));
            extra_arcane_exploit.Ranks = 10;
            library.AddFeats(extra_arcane_exploit);
        }


        static void createConsumeSpellsAndSwiftConsume()
        {
            var icon = library.Get<BlueprintFeature>("bfbaa0dd74b9909459e462cd8b091177").Icon;
            var resource = Helpers.CreateAbilityResource("ConsumeSpellsResource", "", "", "", null);
            resource.AddComponent(Helpers.Create<ResourceMechanics.MinResourceAmount>(m => m.value = 1));
            resource.SetIncreasedByStat(0, StatType.Charisma);

            List<BlueprintAbility> consume_abilities = new List<BlueprintAbility>();
            consume_abilities.Add(null);

            for (int i = 1; i <= 9; i++)
            {
                var ability = Helpers.CreateAbility($"ConsumeSpells{i}Ability",
                                                    "Consume Spells " + Common.roman_id[i],
                                                    "At 1st level, an arcanist can expend an available arcanist spell slot as a move action, making it unavailable for the rest of the day, just as if she had used it to cast a spell. She can use this ability a number of times per day equal to her Charisma modifier (minimum 1). Doing this adds a number of points to her arcane reservoir equal to the level of the spell slot consumed. She cannot consume cantrips (0 level spells) in this way. Points gained in excess of the reservoir’s maximum are lost.",
                                                    "",
                                                    icon,
                                                    AbilityType.Special,
                                                    CommandType.Move,
                                                    AbilityRange.Personal,
                                                    "",
                                                    "",
                                                    Helpers.CreateRunActions(Helpers.Create<ResourceMechanics.ContextRestoreResource>(c => { c.amount = i; c.Resource = arcane_reservoir_resource; })),
                                                    Helpers.CreateResourceLogic(resource)
                                                    );
                ability.setMiscAbilityParametersSelfOnly();
                consume_abilities.Add(ability);
            }

            consume_spells = Helpers.CreateFeature("ConsumeSpellsFeature",
                                                   "Consume Spells",
                                                   consume_abilities[1].Description,
                                                   "",
                                                   icon,
                                                   FeatureGroup.None,
                                                   Helpers.CreateAddAbilityResource(resource),
                                                   Helpers.Create<NewMechanics.SpontaneousSpellConversionForArcanistSpellbook>(s =>  s.SpellsByLevel = consume_abilities.ToArray())
                                                   );


            swift_consume = Helpers.CreateFeature("SwiftConsumeExploitFeature",
                                                  "Swift Consume",
                                                  "The arcanist can use the consume spells class feature or the consume magic items exploit as swift actions instead of as move actions.",
                                                  "",
                                                  null,
                                                  FeatureGroup.None,
                                                  Helpers.Create<TurnActionMechanics.UseAbilitiesAsSwiftAction>(u => u.abilities = consume_abilities.ToArray())
                                                  );
        }


        static void createArcaneReservoirAndPotentMagic()
        {
            var wizard = library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
            arcane_reservoir_resource = Helpers.CreateAbilityResource("ArcaneReservoirFullResource", "", "", "", null);
            arcane_reservoir_resource.SetIncreasedByLevel(3, 1, getExploitsUserArray());


            arcane_reservoir_partial_resource = Helpers.CreateAbilityResource("ArcaneReservoirPartialResource", "", "", "", null);
            arcane_reservoir_partial_resource.SetIncreasedByLevelStartPlusDivStep(3, 2, 1, 2, 1, 0, 0.0f, getExploitsUserArray());
            arcane_reservoir_resource.AddComponent(Helpers.Create<ResourceMechanics.FakeResourceAmountFullRestore>(f => f.fake_resource = arcane_reservoir_partial_resource));

            var icon = library.Get<BlueprintFeature>("55edf82380a1c8540af6c6037d34f322").Icon; //elven magic


            arcane_reservoir = Helpers.CreateFeature("ArcaneReservoirFeature",
                                                     "Arcane Reservoir",
                                                     "An arcanist has an innate pool of magical energy that she can draw upon to fuel her arcanist exploits and enhance her spells. The arcanist’s arcane reservoir can hold a maximum amount of magical energy equal to 3 + the arcanist’s level. Each day, when preparing spells, the arcanist’s arcane reservoir fills with raw magical energy, gaining a number of points equal to 3 + 1/2 her arcanist level. Any points she had from the previous day are lost. She can also regain these points through the consume spells class feature and some arcanist exploits. The arcane reservoir can never hold more points than the maximum amount noted above; points gained in excess of this total are lost.\n"
                                                     + "Points from the arcanist reservoir are used to fuel many of the arcanist’s powers. In addition, the arcanist can expend 1 point from her arcane reservoir as a free action whenever she casts an arcanist spell. If she does, she can choose to increase the caster level by 1 or increase the spell’s DC by 1. She can expend no more than 1 point from her reservoir on a given spell in this way.",
                                                     "",
                                                     icon,
                                                     FeatureGroup.None,
                                                     Helpers.CreateAddAbilityResource(arcane_reservoir_resource)
                                                     );
            potent_magic = Helpers.CreateFeature("PotentMagicExploitFeature",
                                                 "Potent Magic",
                                                 "Whenever the arcanist expends 1 point from her arcane reservoir to increase the caster level of a spell, the caster level increases by 2 instead of 1. Whenever she expends 1 point from her arcane reservoir to increase the spell’s DC, it increases by 2 instead of 1.",
                                                 "",
                                                 library.Get<BlueprintAbility>("92681f181b507b34ea87018e8f7a528a").Icon, //dispel magic
                                                 FeatureGroup.None
                                                 );

            dc_buff = Helpers.CreateBuff("ArcaneReservoirSpellDCBuff",
                                             "Spell DC Increase",
                                             "The arcanist can expend 1 point from her arcane reservoir as a free action whenever she casts an arcanist spell. If she does, she can choose to increase the caster level by 1 or increase the spell’s DC by 1. She can expend no more than 1 point from her reservoir on a given spell in this way.",
                                             "",
                                             icon,
                                             null,
                                             Helpers.Create<NewMechanics.IncreaseAllSpellsDCForSpecificSpellbook>(i => { i.spellbook = arcanist_spellbook; i.Value = Helpers.CreateContextValue(AbilityRankType.Default); }),
                                             Helpers.Create<NewMechanics.SpendResourceOnSpellCast>(s => { s.spellbook = arcanist_spellbook; s.resource = arcane_reservoir_resource; }),
                                             Helpers.Create<NewMechanics.IncreaseAllSpellsDCForSpecificSpellbook>(i => { i.spellbook = wizard.Spellbook; i.Value = Helpers.CreateContextValue(AbilityRankType.Default); }),
                                             Helpers.Create<NewMechanics.SpendResourceOnSpellCast>(s => { s.spellbook = wizard.Spellbook; s.resource = arcane_reservoir_resource; }),
                                             Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureListRanks, featureList: new BlueprintFeature[] {potent_magic}, progression: ContextRankProgression.OnePlusDivStep, stepLevel: 1)
                                             );
            arcane_reservoir_spell_dc_boost = Helpers.CreateActivatableAbility("ArcaneReservoirSpellDCToggleAbility",
                                                                               dc_buff.Name,
                                                                               dc_buff.Description,
                                                                               "",
                                                                               dc_buff.Icon,
                                                                               dc_buff,
                                                                               AbilityActivationType.Immediately,
                                                                               CommandType.Free,
                                                                               null,
                                                                               Helpers.CreateActivatableResourceLogic(arcane_reservoir_resource, ResourceSpendType.Never)
                                                                               );
            arcane_reservoir_spell_dc_boost.Group = ActivatableAbilityGroupExtension.ArcanistArcaneReservoirSpellboost.ToActivatableAbilityGroup();
            arcane_reservoir_spell_dc_boost.DeactivateImmediately = true;


            cl_buff = Helpers.CreateBuff("ArcaneReservoirSpellCLBuff",
                                 "Spell Caster Level Increase",
                                 "The arcanist can expend 1 point from her arcane reservoir as a free action whenever she casts an arcanist spell. If she does, she can choose to increase the caster level by 1 or increase the spell’s DC by 1. She can expend no more than 1 point from her reservoir on a given spell in this way.",
                                 "",
                                 icon,
                                 null,
                                 Helpers.Create<NewMechanics.IncreaseAllSpellsCLForSpecificSpellbook>(i => { i.spellbook = arcanist_spellbook; i.Value = Helpers.CreateContextValue(AbilityRankType.Default); }),
                                 Helpers.Create<NewMechanics.SpendResourceOnSpellCast>(s => { s.spellbook = arcanist_spellbook; s.resource = arcane_reservoir_resource; }),
                                 Helpers.Create<NewMechanics.IncreaseAllSpellsCLForSpecificSpellbook>(i => { i.spellbook = wizard.Spellbook; i.Value = Helpers.CreateContextValue(AbilityRankType.Default); }),
                                 Helpers.Create<NewMechanics.SpendResourceOnSpellCast>(s => { s.spellbook = wizard.Spellbook; s.resource = arcane_reservoir_resource; }),
                                 Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureListRanks, featureList: new BlueprintFeature[] {potent_magic }, progression: ContextRankProgression.OnePlusDivStep, stepLevel: 1)
                                 );

            arcane_reservoir_caster_level_boost = Helpers.CreateActivatableAbility("ArcaneReservoirSpellCLToggleAbility",
                                                                                   cl_buff.Name,
                                                                                   cl_buff.Description,
                                                                                   "",
                                                                                   cl_buff.Icon,
                                                                                   cl_buff,
                                                                                   AbilityActivationType.Immediately,
                                                                                   CommandType.Free,
                                                                                   null,
                                                                                   Helpers.CreateActivatableResourceLogic(arcane_reservoir_resource, ResourceSpendType.Never)
                                                                                   );
            arcane_reservoir_caster_level_boost.Group = ActivatableAbilityGroupExtension.ArcanistArcaneReservoirSpellboost.ToActivatableAbilityGroup();
            arcane_reservoir_caster_level_boost.DeactivateImmediately = true;


            arcane_reservoir.AddComponent(Helpers.CreateAddFacts(arcane_reservoir_spell_dc_boost, arcane_reservoir_caster_level_boost));

            arcane_reservoir_wizard = library.CopyAndAdd(arcane_reservoir, "WizardArcaneReservoir", "");
            arcane_reservoir_wizard.SetDescription("At 1st level, the exploiter wizard gains the arcanist’s arcane reservoir class feature. The exploiter wizard uses his wizard level as his arcanist level for determining how many arcane reservoir points he gains at each level.");
        }


        static BlueprintCharacterClass[] getArcanistArray()
        {
            return new BlueprintCharacterClass[] { arcanist_class };
        }


        static BlueprintCharacterClass[] getExploitsUserArray()
        {
            return new BlueprintCharacterClass[] { arcanist_class, library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e") };
        }


        static void createArcanistProgression()
        {
            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");

            arcanist_progression = Helpers.CreateProgression("ArcanistProgression",
                                                               arcanist_class.Name,
                                                               arcanist_class.Description,
                                                               "",
                                                               arcanist_class.Icon,
                                                               FeatureGroup.None,
                                                               Helpers.Create<SpellManipulationMechanics.ArcanistPreparedMetamagicNoSpellCastingTimeIncrease>());
            arcanist_progression.Classes = getArcanistArray();

            arcanist_proficiencies = library.CopyAndAdd<BlueprintFeature>("25c97697236ccf2479d0c6a4185eae7f", "ArcanistProficiencies", "");
            arcanist_proficiencies.SetNameDescription("Arcanist Proficiencies",
                                                      "Arcanists are proficient with all simple weapons. They are not proficient with any type of armor or shield. Armor interferes with an arcanist’s gestures, which can cause her spells with somatic components to fail.");
            arcanist_cantrips = library.CopyAndAdd<BlueprintFeature>("c58b36ec3f759c84089c67611d1bcc21", "ArcanistCantrips", "");
            arcanist_cantrips.SetNameDescription("Arcanist Cantrips",
                                                 "Arcanists can cast a number of cantrips, or 0-level spells. These spells are cast like any other spell, but they are not expended when cast and may be used again.");
            arcanist_cantrips.ReplaceComponent<LearnSpells>(l => l.CharacterClass = arcanist_class);
            arcanist_cantrips.ReplaceComponent<BindAbilitiesToClass>(b => { b.CharacterClass = arcanist_class; b.Stat = StatType.Intelligence; });

            createArcanistSpellCasting();
            createArcaneReservoirAndPotentMagic();
            createConsumeSpellsAndSwiftConsume();
            createArcaneExploits();
            createMagicalSupremacy();

            arcanist_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, arcanist_proficiencies, detect_magic, arcanist_cantrips, 
                                                                                        arcanist_spellcasting,
                                                                                        arcane_reservoir,
                                                                                        consume_spells,
                                                                                        arcane_exploits,
                                                                                        library.Get<BlueprintFeature>("0aeba56961779e54a8a0f6dedef081ee"), //inside the storm
                                                                                        library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), //ray calculate feature
                                                                                        library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")),//touch calculate feature};
                                                                    Helpers.LevelEntry(2),
                                                                    Helpers.LevelEntry(3, arcane_exploits),
                                                                    Helpers.LevelEntry(4),
                                                                    Helpers.LevelEntry(5, arcane_exploits),
                                                                    Helpers.LevelEntry(6),
                                                                    Helpers.LevelEntry(7, arcane_exploits),
                                                                    Helpers.LevelEntry(8),
                                                                    Helpers.LevelEntry(9, arcane_exploits),
                                                                    Helpers.LevelEntry(10),
                                                                    Helpers.LevelEntry(11, greater_arcane_exploits, arcane_exploits),
                                                                    Helpers.LevelEntry(12),
                                                                    Helpers.LevelEntry(13, arcane_exploits),
                                                                    Helpers.LevelEntry(14),
                                                                    Helpers.LevelEntry(15, arcane_exploits),
                                                                    Helpers.LevelEntry(16),
                                                                    Helpers.LevelEntry(17, arcane_exploits),
                                                                    Helpers.LevelEntry(18),
                                                                    Helpers.LevelEntry(19, arcane_exploits),
                                                                    Helpers.LevelEntry(20, magical_supremacy)
                                                                    };

            arcanist_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { arcanist_proficiencies, detect_magic, arcanist_cantrips, arcanist_spellcasting };

            arcanist_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(arcane_reservoir, greater_arcane_exploits, magical_supremacy),
                                                            Helpers.CreateUIGroup(arcane_exploits)
                                                           };
        }


        static void createMagicalSupremacy()
        {
            var buff = Helpers.CreateBuff("MagicalSupremacyBuff",
                                          "Magical Supremacy",
                                          "At 20th level, the arcanist learns how to convert her arcane reservoir into spells and back again. She can cast any spell she has prepared by expending a number of points from her arcane reservoir equal to 1 + the level of the spell to be cast instead of expending a spell slot. When she casts a spell in this fashion, she treats her caster level as 2 higher than normal, and the DCs of any saving throws associated with the spell increase by 2. She cannot further expend points from her arcane reservoir to enhance a spell cast in this way.",
                                          "",
                                          LoadIcons.Image2Sprite.Create(@"AbilityIcons/MagicalSupremacy.png"),
                                          null,
                                          Helpers.Create<SpellManipulationMechanics.MagicalSupremacy>(m => { m.resource = arcane_reservoir_resource; m.bonus = 2; })
                                          );
            var ability = Helpers.CreateActivatableAbility("MagicalSupremacyActivatableAbility",
                                                           buff.Name,
                                                           buff.Description,
                                                           "",
                                                           buff.Icon,
                                                           buff,
                                                           AbilityActivationType.Immediately,
                                                           CommandType.Free,
                                                           null,
                                                           Helpers.CreateActivatableResourceLogic(arcane_reservoir_resource, ResourceSpendType.Never)
                                                           );
            ability.DeactivateImmediately = true;
            ability.Group = ActivatableAbilityGroupExtension.ArcanistArcaneReservoirSpellboost.ToActivatableAbilityGroup();
            magical_supremacy = Common.ActivatableAbilityToFeature(ability, false);
            magical_supremacy_buff = buff;
        }


        static void createArcanistSpellCasting()
        {
            arcanist_spellcasting = Helpers.CreateFeature("ArcanistSpellCastingFeature",
                                                          "Arcanist Spell Casting",
                                                          "An arcanist casts arcane spells drawn from the sorcerer/wizard spell list. An arcanist must prepare her spells ahead of time, but unlike a wizard, her spells are not expended when they’re cast. Instead, she can cast any spell that she has prepared consuming a spell slot of the appropriate level, assuming she hasn’t yet used up her spell slots per day for that level.\n"
                                                          + "To learn, prepare, or cast a spell, the arcanist must have an Intelligence score equal to at least 10 + the spell’s level. The saving throw DC against an arcanist’s spell is 10 + the spell’s level + the arcanist’s Intelligence modifier.\n"
                                                          + "An arcanist can only cast a certain number of spells of each spell level per day. In addition, she receives bonus spells per day if she has a high Intelligence score.\n"
                                                          + "An arcanist may know any number of spells, but the number she can prepare each day is limited. Unlike the number of spells she can cast per day, the number of spells an arcanist can prepare each day is not affected by her Intelligence score. Feats and other effects that modify the number of spells known by a spellcaster instead affect the number of spells an arcanist can prepare.\n"
                                                          + "Like a sorcerer, an arcanist can choose to apply any metamagic feats she knows to a prepared spell as she casts it, with the same increase in casting time (see Spontaneous Casting and Metamagic Feats). However, she may also prepare a spell with any metamagic feats she knows and cast it without increasing casting time like a wizard. She cannot combine these options—a spell prepared with metamagic feats cannot be further modified with another metamagic feat at the time of casting (unless she has the metamixing arcanist exploit).",
                                                          "",
                                                          null,
                                                          FeatureGroup.None,
                                                          Helpers.Create<SpellManipulationMechanics.InitializeArcanistPart>(i => i.spellbook = arcanist_spellbook)
                                                          );
        }


        static void createArcaneExploits()
        {
            var icon = library.Get<BlueprintFeature>("30f20e6f850519b48aa59e8c0ff66ae9").Icon;
            var greater_icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/ArcaneExploit.png");
            arcane_exploits = Helpers.CreateFeatureSelection("ArcaneExploitsFeatureSelection",
                                                             "Arcanist Exploits",
                                                             "By bending and sometimes even breaking the rules of magic, the arcanist learns to exploit gaps and exceptions in the laws of magic. Some of these exploits allow her to break down various forms of magic, adding their essence to her arcane reservoir. At 1st level and every 2 levels thereafter, the arcanist learns a new arcane exploit selected from the following list. An arcanist exploit cannot be selected more than once. Once an arcanist exploit has been selected, it cannot be changed. Most arcanist exploits require the arcanist to expend points from her arcane reservoir to function. Unless otherwise noted, the saving throw DC for an arcanist exploit is equal to 10 + 1/2 the arcanist’s level + the arcanist’s Charisma modifier.",
                                                             "",
                                                             icon,
                                                             FeatureGroup.None);
            greater_arcane_exploits = Helpers.CreateFeature("GreateArcaneExploitsFeature",
                                                             "Greater Exploits",
                                                             "At 11th level and every 2 levels thereafter, an arcanist can choose one of the greater exploits in place of an arcanist exploit.",
                                                             "",
                                                             greater_icon,
                                                             FeatureGroup.None);
            createQuickStudy();
            createQuickStudyWizard();
            createArcaneBarrier();
            createArcaneWeapon();
            createEnergyShieldAndEnergyAbsorption();
            createAcidJetAndLingeringAcid();
            createDimensionalSlide();
            createFamiliar();
            createFeralShifting();
            createFlameArcAndBurningFlame();
            createForceStrike();
            createHolyWaterJet();
            createIceMissileAndIcyTomb();
            createLightningLanceAndDancingElectricity();
            createMetamagicKnowledgeAndGreaterMetamgicKnowledge();
            createMetamixing();
            createSonicBlast();
            createSpellResistanceAndSpellResistanceGreater();
            createWoodenFlesh();
            createShiftCaster();
            createItemBond();
            
            arcane_exploits.AllFeatures = new BlueprintFeature[] { quick_study, arcane_barrier, arcane_weapon, acid_jet, energy_shield, dimensional_slide, familiar, feral_shifting,
                                                                 flame_arc, force_strike, holy_water_jet, ice_missile, lightning_lance, metamagic_knowledge, metamixing, sonic_blast, swift_consume,
                                                                 spell_resistance, wooden_flesh, shift_caster,
                                                                 energy_absorption, lingering_acid, burning_flame, icy_tomb, dancing_electricity, greater_metamagic_knowledge,
                                                                 greater_spell_resistance, item_bond};

            arcane_exploits_wizard = Helpers.CreateFeatureSelection("ArcaneExploitsWizardFeatureSelection",
                                                 "Arcanist Exploits",
                                                 "At 1st level and every 4 levels thereafter, the exploiter wizard gains a single arcanist exploit. The exploiter wizard uses his wizard level as his arcanist level for determining the effects and DCs of his arcanist exploits.",
                                                 "",
                                                 icon,
                                                 FeatureGroup.None);
            arcane_exploits_wizard.AllFeatures = new BlueprintFeature[]
            {
                quick_study_wizard, arcane_barrier, arcane_weapon, acid_jet, energy_shield, dimensional_slide, familiar, feral_shifting, shift_caster,
                flame_arc, force_strike, holy_water_jet, ice_missile, lightning_lance, metamagic_knowledge, sonic_blast, spell_resistance, wooden_flesh, item_bond
            };


            if (!Main.settings.balance_fixes)
            {
                arcane_exploits_wizard.AllFeatures = arcane_exploits_wizard.AllFeatures.AddToArray(potent_magic);
                arcane_exploits.AllFeatures = arcane_exploits.AllFeatures.AddToArray(potent_magic);
            }
        }


        static void createItemBond()
        {
            var resource = Helpers.CreateAbilityResource("ArcanistItemBondResource", "", "", "", null);
            resource.SetFixedResource(1);

            var abilities = new List<BlueprintAbility>();
            var feature = library.Get<BlueprintFeature>("2fb5e65bd57caa943b45ee32d825e9b9");
            foreach (var f  in feature.GetComponent<AddFacts>().Facts)
            {
                var a = library.CopyAndAdd<BlueprintAbility>(f.AssetGuid, "Arcanist" + f.name, "");
                a.ReplaceComponent<AbilityResourceLogic>(ab => ab.RequiredResource = resource);
                a.AddComponent(Helpers.Create<NewMechanics.AbilityCasterHasResource>(ab => ab.resource = arcane_reservoir_resource));
                abilities.Add(a);
                a.SetDescription("Once per day an arcanist can spend 1 point from his arcane reservoir to restore any one spell that she had prepared for this day.");
                a.ActionType = CommandType.Swift;
            }

            item_bond = library.CopyAndAdd<BlueprintFeature>("2fb5e65bd57caa943b45ee32d825e9b9", "ArcanistItemBondFeature", "");
            item_bond.SetDescription(abilities[0].Description);
            item_bond.ReplaceComponent<AddFacts>(a => a.Facts = abilities.ToArray());
            item_bond.ReplaceComponent<AddAbilityResources>(a => a.Resource = resource);
            item_bond.AddComponent(Helpers.Create<ResourceMechanics.ConnectResource>(c => { c.base_resource = resource; c.connected_resources = new BlueprintAbilityResource[] { arcane_reservoir_resource }; }));
            item_bond.AddComponent(Helpers.PrerequisiteNoFeature(library.Get<BlueprintFeature>("2fb5e65bd57caa943b45ee32d825e9b9")));
        }


        static void createQuickStudyWizard()
        {
            var wizard_class = library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
            quick_study_wizard = Helpers.CreateFeature("QuickStudyWizardFeature",
                                           quick_study.Name,
                                           "The exploiter wizard can prepare a spell in place of an existing spell by expending 1 point from her arcane reservoir. Using this ability is a full-round action that provokes an attack of opportunity. The arcanist must be able to reference her spellbook when using this ability. The spell prepared must be of the same level as the spell being replaced.",
                                           "",
                                           quick_study.Icon,
                                           FeatureGroup.Feat);

            var initiate = new BlueprintAbility[9];
            var relearn = new BlueprintAbility[9];
            var buffs = new BlueprintBuff[9];

            for (int i = 0; i < 9; i++)
            {
                var buff = Helpers.CreateBuff($"QuickStudyInitiated{i+1}Buff",
                                              "Quick Study " + Common.roman_id[i + 1],
                                              quick_study_wizard.Description,
                                              "",
                                              quick_study_wizard.Icon,
                                              null);
                buff.SetBuffFlags(BuffFlags.RemoveOnRest);


                relearn[i] = Helpers.CreateAbility($"QuickStudyMemorize{i+1}Ability",
                                                    buff.Name + " (Pick Spell to Memorize)",
                                                    quick_study_wizard.Description,
                                                    "",
                                                    quick_study_wizard.Icon,
                                                    AbilityType.Special,
                                                    CommandType.Free,
                                                    AbilityRange.Personal,
                                                    "",
                                                    "",
                                                    Helpers.Create<AbilityRestoreSpellSlot>(a => a.SpellLevel = i + 1),
                                                    Common.createAbilityCasterHasFacts(buff),
                                                    Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionRemoveBuff(buff)))
                                                    );

                initiate[i] = Helpers.CreateAbility($"QuickStudyInitiate{i+1}Ability",
                                                    buff.Name + " (Pick Spell to Forget)",
                                                    quick_study_wizard.Description,
                                                    "",
                                                    quick_study_wizard.Icon,
                                                    AbilityType.SpellLike,
                                                    CommandType.Standard,
                                                    AbilityRange.Personal,
                                                    "",
                                                    "",
                                                    Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false)),
                                                    arcane_reservoir_resource.CreateResourceLogic()
                                                    );
                Common.setAsFullRoundAction(initiate[i]);
                buffs[i] = buff;
                buff.AddComponent(Helpers.CreateAddFact(relearn[i]));
            }

            foreach (var i in initiate)
            {
                i.AddComponent(Common.createAbilityCasterHasNoFacts(buffs));
            }

            //quick_study_wizard.AddComponent(Helpers.CreateAddFacts(relearn));
            quick_study_wizard.AddComponent(Common.createSpontaneousSpellConversion(wizard_class, new BlueprintAbility[] { null }.AddToArray(initiate)));
        }


        static void createShiftCaster()
        {
            var icon = library.Get<BlueprintFeature>("c806103e27cce6f429e5bf47067966cf").Icon; //natural spell

            var buff = Helpers.CreateBuff("ShiftCasterExploitBuff",
                                          "Shift Caster",
                                          "The arcanist can expend 1 point from her arcane reservoir to cast a spell while under the effects of a polymorph spell. This ability works like Natural Spell, except the arcanist uses the ability to cast while under the effects of a spell instead of wild shape.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.NaturalSpell),
                                          Helpers.Create<NewMechanics.SpendResourceOnSpellCast>(r => r.resource = arcane_reservoir_resource)
                                          );

            var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                             arcane_reservoir_resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never),
                                             Helpers.Create<NewMechanics.ActivatableARestrictionCasterPolymorphed>()
                                            );

            shift_caster = Common.ActivatableAbilityToFeature(toggle, false);           
        }


        static void createWoodenFlesh()
        {
            var icon = library.Get<BlueprintAbility>("5b77d7cc65b8ab74688e74a37fc2f553").Icon; //barksin

            var buff = Helpers.CreateBuff("WoodenFleshExploitBuff",
                                          "Wooden Flesh",
                                          "The arcanist infuses herself with the toughness of the plant life that she studies.The arcanist can spend 1 point from her arcane reservoir to gain a + 2 natural armor bonus and DR / slashing equal to her Charisma modifier(minimum 1) for 1 minute per arcanist level. While this ability is in effect, she counts as both her original creature type and a plant creature for the purpose of abilities and spells.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.CreateAddStatBonus(StatType.AC, 2, ModifierDescriptor.NaturalArmor),
                                          Common.createContextFormDR(Helpers.CreateContextValue(AbilityRankType.Default), PhysicalDamageForm.Slashing),
                                          Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Charisma, min: 1),
                                          Helpers.CreateAddFact(Common.plant)
                                          );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), dispellable: false);

            var ability = Helpers.CreateAbility("WoodenFleshExploitAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Personal,
                                                "Charisma modifier minutes (minimum 1)",
                                                "",
                                                Helpers.CreateRunActions(apply_buff),
                                                library.Get<BlueprintAbility>("5b77d7cc65b8ab74688e74a37fc2f553").GetComponent<AbilitySpawnFx>(),
                                                Helpers.CreateResourceLogic(arcane_reservoir_resource),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Charisma, min: 1)
                                                );
            ability.setMiscAbilityParametersSelfOnly();

            wooden_flesh = Common.AbilityToFeature(ability, false);
        }


        static void createSpellResistanceAndSpellResistanceGreater()
        {
            var icon = library.Get<BlueprintAbility>("0a5ddfbcfb3989543ac7c936fc256889").Icon; //spell resistance
            spell_resistance = Helpers.CreateFeature("SpellResistanceExploitFeature",
                                                     "Spell Resistance",
                                                     "The arcanist can grant herself spell resistance for a number of minutes equal to her Charisma modifier (minimum 1) as a standard action by expending 1 point from her arcane reservoir. This spell resistance is equal to 6 + her arcanist level.",
                                                     "",
                                                     icon,
                                                     FeatureGroup.None);

            greater_spell_resistance = Helpers.CreateFeature("GreaterSpellResistanceExploitFeature",
                                                             "Greater Spell Resistance",
                                                             "Whenever the arcanist uses the spell resistance exploit, the spell resistance is equal to 11 + the arcanist’s level. The arcanist must have the spell resistance exploit to select this exploit.",
                                                             "",
                                                             null,
                                                             FeatureGroup.None,
                                                             Helpers.PrerequisiteFeature(greater_arcane_exploits),
                                                             Helpers.PrerequisiteFeature(spell_resistance));

            var buff = Helpers.CreateBuff("SpellResistanceExploitBuff",
                                          spell_resistance.Name,
                                          spell_resistance.Description,
                                          "",
                                          spell_resistance.Icon,
                                          null,
                                          Helpers.Create<AddSpellResistance>(a => a.Value = Helpers.CreateContextValue(AbilitySharedValue.StatBonus)),
                                          Helpers.CreateCalculateSharedValue(Helpers.CreateContextDiceValue(DiceType.One,
                                                                                                            Helpers.CreateContextValue(AbilityRankType.StatBonus),
                                                                                                            Helpers.CreateContextValue(AbilityRankType.Default)
                                                                                                            ),
                                                                             sharedValue: AbilitySharedValue.StatBonus
                                                                            ),
                                          Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getExploitsUserArray()),
                                          Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureListRanks, type: AbilityRankType.StatBonus,
                                                                          featureList: new BlueprintFeature[] { spell_resistance, greater_spell_resistance },
                                                                          progression: ContextRankProgression.Custom,
                                                                          customProgression: new (int, int)[2] { (1, 6), (2, 11) }
                                                                          )
                                         );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), dispellable: false);
            var ability = Helpers.CreateAbility("SpellResistanceExploitAbility",
                                                spell_resistance.Name,
                                                spell_resistance.Description,
                                                "",
                                                icon,
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Personal,
                                                "Charisma modifier minutes (minimum 1)",
                                                "",
                                                Helpers.CreateRunActions(apply_buff),
                                                library.Get<BlueprintAbility>("0a5ddfbcfb3989543ac7c936fc256889").GetComponent<AbilitySpawnFx>(),
                                                Helpers.CreateResourceLogic(arcane_reservoir_resource),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Charisma, min: 1)
                                                );
            ability.setMiscAbilityParametersSelfOnly();
            spell_resistance.AddComponent(Helpers.CreateAddFact(ability));
        }


        static BlueprintFeature createEvocation()
        {
            //evocation
            var evocation_progression = library.Get<BlueprintProgression>("f8019b7724d72a241a97157bc37f1c3b");
            var resource = Helpers.CreateAbilityResource("EvocationSchoolUnderstandingBaseResource", "", "", "", null);
            var base_ability = library.CopyAndAdd<BlueprintAbility>("3d55cc710cc497843bb51788057cd93f", "SchoolUnderstandingEvocationSchoolBaseAbility", "");
            base_ability.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = resource);
            resource.SetIncreasedByStat(3, StatType.Charisma);

            var evocation_buff = Helpers.CreateBuff("SchoolUnderstangingEvocationBuff",
                                          "School Understanding (" + evocation_progression.Name + ")",
                                          school_understanding.Description + "\n" + evocation_progression.Name + ": " + evocation_progression.Description,
                                          "",
                                          evocation_progression.Icon,
                                          null,
                                          Helpers.Create<NewMechanics.IntenseSpellsForClasses>(i => i.classes = getExploitsUserArray()));
            base_ability.AddComponent(Common.createAbilityCasterHasFacts(evocation_buff));

            var evocation_feature = Helpers.CreateFeature("EvocationSchoolUnderstangingFeature",
                                                          evocation_buff.Name,
                                                          evocation_buff.Description,
                                                          "",
                                                          evocation_buff.Icon,
                                                          FeatureGroup.None,
                                                          Helpers.CreateAddFacts(base_ability, createSchoolUnderstandingAbility(evocation_buff)),
                                                          Helpers.CreateAddAbilityResource(resource),
                                                          Helpers.PrerequisiteNoFeature(evocation_progression),
                                                          Helpers.PrerequisiteNoFeature(Subschools.admixture));
            return evocation_feature;
        }


        static BlueprintFeature createAdmixture()
        {
            //evocation
            var evocation_progression = Subschools.admixture;
            var resource = Helpers.CreateAbilityResource("AdmixtureSchoolUnderstandingBaseResource", "", "", "", null);
            resource.SetIncreasedByStat(3, StatType.Charisma);

            var evocation_buff = Helpers.CreateBuff("SchoolUnderstangingAdmixtureBuff",
                                          "School Understanding (" + evocation_progression.Name + ")",
                                          school_understanding.Description + "\n" + evocation_progression.Name + ": " + evocation_progression.Description,
                                          "",
                                          evocation_progression.Icon,
                                          null,
                                          Helpers.Create<NewMechanics.IntenseSpellsForClasses>(i => i.classes = getExploitsUserArray()));
            

            var evocation_feature = Helpers.CreateFeature("AdmixtureSchoolUnderstangingFeature",
                                                          evocation_buff.Name,
                                                          evocation_buff.Description,
                                                          "",
                                                          evocation_buff.Icon,
                                                          FeatureGroup.None,
                                                          Helpers.CreateAddFacts(createSchoolUnderstandingAbility(evocation_buff)),
                                                          Helpers.CreateAddAbilityResource(resource),
                                                          Helpers.PrerequisiteNoFeature(library.Get<BlueprintProgression>("f8019b7724d72a241a97157bc37f1c3b")),
                                                          Helpers.PrerequisiteNoFeature(evocation_progression));

            foreach (var a in Subschools.versatile_evocation)
            {
                var a2 = library.CopyAndAdd(a, "SchoolUnderstanding" + a.name, "");
                var buff2 = library.CopyAndAdd(a2.Buff, "SchoolUnderstanding" + a2.Buff.name, "");
                buff2.ReplaceComponent<NewMechanics.MetamagicMechanics.MetamagicOnSchool>(m => m.resource = resource);
                a2.Buff = buff2;
                a2.ReplaceComponent<ActivatableAbilityResourceLogic>(ar => ar.RequiredResource = resource);
                a2.AddComponent(Common.createActivatableAbilityRestrictionHasFact(evocation_buff));
                var af = evocation_feature.GetComponent<AddFacts>();
                af.Facts = af.Facts.AddToArray(a2);
            }

            return evocation_feature;
        }


        static BlueprintFeature createAbjuration()
        {
            //abjuration
            var abjuration_progression = library.Get<BlueprintProgression>("c451fde0aec46454091b70384ea91989");
            var resource = Helpers.CreateAbilityResource("AbjurationSchoolUnderstandingBaseResource", "", "", "", null);
            var base_ability = library.CopyAndAdd<BlueprintAbility>("2433d465095a9984398a0482b1af0877", "SchoolUnderstandingAbjurationSchoolBaseAbility", "");
            base_ability.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = resource);
            resource.SetIncreasedByStat(3, StatType.Charisma);
            var resistance_feature = library.Get<BlueprintFeature>("1abe070e7a00ddd48b8a141d71f79e70");

            var abjuration_buff = Helpers.CreateBuff("SchoolUnderstangingAbjurationBuff",
                                          "School Understanding (" + abjuration_progression.Name + ")",
                                          school_understanding.Description + "\n" + abjuration_progression.Name + ": " + abjuration_progression.Description,
                                          "",
                                          abjuration_progression.Icon,
                                          null);
            base_ability.AddComponent(Common.createAbilityCasterHasFacts(abjuration_buff));

            var abjuration_feature = Helpers.CreateFeature("AbjurationSchoolUnderstangingFeature",
                                                          abjuration_buff.Name,
                                                          abjuration_buff.Description,
                                                          "",
                                                          abjuration_buff.Icon,
                                                          FeatureGroup.None,
                                                          Helpers.CreateAddFacts(base_ability, createSchoolUnderstandingAbility(abjuration_buff)),
                                                          Helpers.CreateAddAbilityResource(resource),
                                                          Helpers.PrerequisiteNoFeature(abjuration_progression),
                                                          Helpers.Create<ReplaceAbilitiesStat>(r => { r.Ability = new BlueprintAbility[] { base_ability }; r.Stat = StatType.Charisma; }),
                                                          resistance_feature.GetComponent<AddAbilityResources>());

            foreach (var a in resistance_feature.GetComponent<AddFacts>().Facts.Cast<BlueprintAbility>())
            {
                var new_a = library.CopyAndAdd(a, "SchoolUnderstanding" + a.name, "");
                new_a.AddComponent(Common.createAbilityCasterHasFacts(abjuration_buff));
                abjuration_feature.AddComponent(Helpers.CreateAddFact(new_a));
            }

            return abjuration_feature;
        }


        static BlueprintFeature createTransmutation()
        {
            //transmutation
            var transutation_progression = library.Get<BlueprintProgression>("b6a604dab356ac34788abf4ad79449ec");
            var resource = Helpers.CreateAbilityResource("TransmutationSchoolUnderstandingBaseResource", "", "", "", null);
            var base_ability = library.CopyAndAdd<BlueprintAbility>("810992c76efdde84db707a0444cf9a1c", "SchoolUnderstandingTransmutaionSchoolBaseAbility", ""); //telekientic fist
            base_ability.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = resource);
            resource.SetIncreasedByStat(3, StatType.Charisma);
            var enhancement_feature = library.Get<BlueprintFeature>("93919f8ce64dc5a4cbf058a486a44a1b");

            var transmutation_buff = Helpers.CreateBuff("SchoolUnderstangingTransmutationBuff",
                                                          "School Understanding (" + transutation_progression.Name + ")",
                                                          school_understanding.Description + "\n" + transutation_progression.Name + ": " + transutation_progression.Description,
                                                          "",
                                                          transutation_progression.Icon,
                                                          null);
            base_ability.AddComponent(Common.createAbilityCasterHasFacts(transmutation_buff));

            var transmutation_feature = Helpers.CreateFeature("TransmutatitonSchoolUnderstangingFeature",
                                                          transmutation_buff.Name,
                                                          transmutation_buff.Description,
                                                          "",
                                                          transmutation_buff.Icon,
                                                          FeatureGroup.None,
                                                          Helpers.CreateAddFacts(base_ability, createSchoolUnderstandingAbility(transmutation_buff)),
                                                          Helpers.CreateAddAbilityResource(resource),
                                                          Helpers.PrerequisiteNoFeature(transutation_progression),
                                                          Helpers.PrerequisiteNoFeature(Subschools.enhancement),
                                                          Helpers.Create<ReplaceAbilitiesStat>(r => { r.Ability = new BlueprintAbility[] { base_ability }; r.Stat = StatType.Charisma; }),
                                                          Helpers.CreateAddFeatureOnClassLevel(library.Get<BlueprintFeature>("6aa7d3496cd68e643adcd439a7306caa"), 20, getExploitsUserArray(), false));//capstone

            foreach (var a in enhancement_feature.GetComponent<AddFacts>().Facts.Cast<BlueprintActivatableAbility>())
            {
                var new_a = library.CopyAndAdd(a, "SchoolUnderstanding" + a.name, "");
                new_a.AddComponent(Common.createActivatableAbilityRestrictionHasFact(transmutation_buff));
                transmutation_feature.AddComponent(Helpers.CreateAddFact(new_a));
            }

            return transmutation_feature;
        }


        static BlueprintFeature createEnhancement()
        {
            //transmutation
            var transutation_progression = Subschools.enhancement;
            var resource = Helpers.CreateAbilityResource("EnhancementSchoolUnderstandingBaseResource", "", "", "", null);
            var base_ability = library.CopyAndAdd(Subschools.augment, "ShoolUndersatandingEnhancementAugmentBAseAbility", "");

            resource.SetIncreasedByStat(3, StatType.Charisma);
            var enhancement_feature = library.Get<BlueprintFeature>("93919f8ce64dc5a4cbf058a486a44a1b");

            var transmutation_buff = Helpers.CreateBuff("SchoolUnderstangingEnhancementBuff",
                                                          "School Understanding (" + transutation_progression.Name + ")",
                                                          school_understanding.Description + "\n" + transutation_progression.Name + ": " + transutation_progression.Description,
                                                          "",
                                                          transutation_progression.Icon,
                                                          null);

            base_ability.ReplaceComponent<AbilityVariants>(a =>
            {
                var abilities = new List<BlueprintAbility>();
                foreach (var v in a.Variants)
                {
                    var a2 = library.CopyAndAdd(v, "SchoolUnderstanding" + v.name, "");
                    a2.ReplaceComponent<AbilityResourceLogic>(ar => ar.RequiredResource = resource);
                    a2.AddComponent(Common.createAbilityCasterHasFacts(transmutation_buff));
                    a2.Parent = base_ability;
                    abilities.Add(a2);
                }
                a.Variants = abilities.ToArray();
            }
            );


            var transmutation_feature = Helpers.CreateFeature("EnhancementSchoolUnderstangingFeature",
                                                          transmutation_buff.Name,
                                                          transmutation_buff.Description,
                                                          "",
                                                          transmutation_buff.Icon,
                                                          FeatureGroup.None,
                                                          Helpers.CreateAddFacts(base_ability, createSchoolUnderstandingAbility(transmutation_buff)),
                                                          Helpers.CreateAddAbilityResource(resource),
                                                          Helpers.PrerequisiteNoFeature(library.Get<BlueprintProgression>("b6a604dab356ac34788abf4ad79449ec")),
                                                          Helpers.PrerequisiteNoFeature(Subschools.enhancement),
                                                          Helpers.Create<ReplaceAbilitiesStat>(r => { r.Ability = new BlueprintAbility[] { base_ability }; r.Stat = StatType.Charisma; }),
                                                          Helpers.CreateAddFeatureOnClassLevel(library.Get<BlueprintFeature>("6aa7d3496cd68e643adcd439a7306caa"), 20, getExploitsUserArray(), false));//capstone

            foreach (var a in enhancement_feature.GetComponent<AddFacts>().Facts.Cast<BlueprintActivatableAbility>())
            {
                var new_a = library.CopyAndAdd(a, "SchoolUnderstandingEnhancement" + a.name, "");
                new_a.AddComponent(Common.createActivatableAbilityRestrictionHasFact(transmutation_buff));
                transmutation_feature.AddComponent(Helpers.CreateAddFact(new_a));
            }

            return transmutation_feature;
        }


        static BlueprintFeature createIllusion()
        {
            //illusion
            var illusion_progression = library.Get<BlueprintProgression>("24d5402c0c1de48468b563f6174c6256");
            var resource = Helpers.CreateAbilityResource("IllusionSchoolUnderstandingBaseResource", "", "", "", null);
            var base_ability = library.CopyAndAdd<BlueprintAbility>("9b4d07751dd104243a94b495c571c9dd", "SchoolUnderstandingIllusionSchoolBaseAbility", ""); //blinding ray
            base_ability.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = resource);
            resource.SetIncreasedByStat(3, StatType.Charisma);

            var illusion_buff = Helpers.CreateBuff("SchoolUnderstangingIllusionBuff",
                                                          "School Understanding (" + illusion_progression.Name + ")",
                                                          school_understanding.Description + "\n" + illusion_progression.Name + ": " + illusion_progression.Description,
                                                          "",
                                                          illusion_progression.Icon,
                                                          null);
            base_ability.AddComponent(Common.createAbilityCasterHasFacts(illusion_buff));

            var illusion_feature = Helpers.CreateFeature("IllusionSchoolUnderstangingFeature",
                                                          illusion_buff.Name,
                                                          illusion_buff.Description,
                                                          "",
                                                          illusion_buff.Icon,
                                                          FeatureGroup.None,
                                                          Helpers.CreateAddFacts(base_ability, createSchoolUnderstandingAbility(illusion_buff)),
                                                          Helpers.CreateAddAbilityResource(resource),
                                                          Helpers.PrerequisiteNoFeature(illusion_progression),
                                                          Helpers.Create<ReplaceAbilitiesStat>(r => { r.Ability = new BlueprintAbility[] { base_ability }; r.Stat = StatType.Charisma; }));

            return illusion_feature;
        }


        static BlueprintFeature createConjuration()
        {
            //conjuration
            var conjuration_progression = library.Get<BlueprintProgression>("567801abe990faf4080df566fadcd038");
            var resource = Helpers.CreateAbilityResource("ConjurationSchoolUnderstandingBaseResource", "", "", "", null);
            var base_ability = library.CopyAndAdd<BlueprintAbility>("697291ff99d3fbb448be5b60b5f2a30c", "SchoolUnderstandingConjurationSchoolBaseAbility", ""); //acid dart
            base_ability.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = resource);
            resource.SetIncreasedByStat(3, StatType.Charisma);

            var conjuration_buff = Helpers.CreateBuff("SchoolUnderstangingConjurationBuff",
                                                          "School Understanding (" + conjuration_progression.Name + ")",
                                                          school_understanding.Description + "\n" + conjuration_progression.Name + ": " + conjuration_progression.Description,
                                                          "",
                                                          conjuration_progression.Icon,
                                                          null,
                                                          Helpers.Create<NewMechanics.AddClassesLevelToSummonDuration>(a => { a.Half = true; a.CharacterClasses = getExploitsUserArray(); }));
            base_ability.AddComponent(Common.createAbilityCasterHasFacts(conjuration_buff));

            var conjuration_feature = Helpers.CreateFeature("ConjurationSchoolUnderstangingFeature",
                                                          conjuration_buff.Name,
                                                          conjuration_buff.Description,
                                                          "",
                                                          conjuration_buff.Icon,
                                                          FeatureGroup.None,
                                                          Helpers.CreateAddFacts(base_ability, createSchoolUnderstandingAbility(conjuration_buff)),
                                                          Helpers.CreateAddAbilityResource(resource),
                                                          Helpers.PrerequisiteNoFeature(conjuration_progression),
                                                          Helpers.PrerequisiteNoFeature(Subschools.teleportation),
                                                          Helpers.Create<ReplaceAbilitiesStat>(r => { r.Ability = new BlueprintAbility[] { base_ability }; r.Stat = StatType.Charisma; }));

            return conjuration_feature;
        }


        static BlueprintFeature createTeleportation()
        {
            //conjuration
            var conjuration_progression = Subschools.teleportation;
            var resource = Helpers.CreateAbilityResource("TeleportationSchoolUnderstandingBaseResource", "", "", "", null);

            resource.SetIncreasedByStat(3, StatType.Charisma);

            var conjuration_buff = Helpers.CreateBuff("SchoolUnderstangingTeleportationBuff",
                                                          "School Understanding (" + conjuration_progression.Name + ")",
                                                          school_understanding.Description + "\n" + conjuration_progression.Name + ": " + conjuration_progression.Description,
                                                          "",
                                                          conjuration_progression.Icon,
                                                          null,
                                                          Helpers.Create<NewMechanics.AddClassesLevelToSummonDuration>(a => { a.Half = true; a.CharacterClasses = getExploitsUserArray(); }));

            var conjuration_feature = Helpers.CreateFeature("TeleportationSchoolUnderstangingFeature",
                                                          conjuration_buff.Name,
                                                          conjuration_buff.Description,
                                                          "",
                                                          conjuration_buff.Icon,
                                                          FeatureGroup.None,
                                                          Helpers.CreateAddFacts(createSchoolUnderstandingAbility(conjuration_buff)),
                                                          Helpers.CreateAddAbilityResource(resource),
                                                          Helpers.PrerequisiteNoFeature(conjuration_progression),
                                                          Helpers.PrerequisiteNoFeature(library.Get<BlueprintProgression>("567801abe990faf4080df566fadcd038")));

            for (int i = 1; i <= 10; i++)
            {
                var ability = library.CopyAndAdd<BlueprintAbility>("a9b8be9b87865744382f7c64e599aeb2", $"TeleprotationSchoolUnderstandingBase{i * 5}Ability", "");
                ability.Parent = null;
                ability.ActionType = CommandType.Swift;
                ability.Range = AbilityRange.Custom;
                ability.CustomRange = (i * 5).Feet();
                ability.Type = AbilityType.Supernatural;

                ability.SetNameDescription("Shift", 
                                           "At 1st level, you can teleport to a nearby space as a swift action as if using dimension door. This movement does not provoke an attack of opportunity. You must be able to see the space that you are moving into. You cannot take other creatures with you when you use this ability (except for familiars). You can move 5 feet for every two wizard levels you possess (minimum 5 feet). You can use this ability a number of times per day equal to 3 + your Intelligence modifier.");
                ability.AddComponent(resource.CreateResourceLogic());
                ability.AddComponent(Common.createAbilityCasterHasFacts(conjuration_buff));
                var feature = Common.AbilityToFeature(ability);
                int min_level = i == 1 ? 0 : 2 * i;
                int max_level = i == 10 ? 100 : 2 * i + 1;

                conjuration_feature.AddComponent(Helpers.Create<LevelUpMechanics.AddFeatureOnClassLevelRange>(a => { a.min_level = min_level; a.max_level = max_level; a.classes = getExploitsUserArray(); a.Feature = feature; }));
            }

            return conjuration_feature;
        }

        static BlueprintFeature createEnchantment()
        {
            //enchantment
            var enchantment_progression = library.Get<BlueprintProgression>("252363458703f144788af49ef04d0803");
            var resource = Helpers.CreateAbilityResource("EnchantmentSchoolUnderstandingBaseResource", "", "", "", null);
            var base_ability = library.CopyAndAdd<BlueprintAbility>("7b3cb9ad9ef68cd43837c6db054f7d9f", "SchoolUnderstandingEnchantmentSchoolBaseAbility", ""); //dazing touch
            base_ability.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = resource);
            resource.SetIncreasedByStat(3, StatType.Charisma);

            var base_feature = library.Get<BlueprintFeature>("9e4c4799735ae9c45964e9113107ef02");
            var enchantment_buff = Helpers.CreateBuff("SchoolUnderstangingEnchantmentBuff",
                                                          "School Understanding (" + enchantment_progression.Name + ")",
                                                          school_understanding.Description + "\n" + enchantment_progression.Name + ": " + enchantment_progression.Description,
                                                          "",
                                                          enchantment_progression.Icon,
                                                          null,
                                                          base_feature.GetComponent<SavingThrowBonusAgainstSchoolAbilityValue>(),
                                                          base_feature.GetComponent<AddContextStatBonus>(),
                                                          base_feature.GetComponent<ContextRankConfig>()
                                                          );
            base_ability.AddComponent(Common.createAbilityCasterHasFacts(enchantment_buff));

            var enchantment_feature = Helpers.CreateFeature("EnchantmentSchoolUnderstangingFeature",
                                                          enchantment_buff.Name,
                                                          enchantment_buff.Description,
                                                          "",
                                                          enchantment_buff.Icon,
                                                          FeatureGroup.None,
                                                          Helpers.CreateAddFacts(base_ability, createSchoolUnderstandingAbility(enchantment_buff)),
                                                          Helpers.CreateAddAbilityResource(resource),
                                                          Helpers.PrerequisiteNoFeature(enchantment_progression),
                                                          Helpers.Create<ReplaceAbilitiesStat>(r => { r.Ability = new BlueprintAbility[] { base_ability }; r.Stat = StatType.Charisma; }));
            return enchantment_feature;
        }


        static BlueprintFeature createNecromancy()
        {
            //necromancy
            var necromancy_progression = library.Get<BlueprintProgression>("e9450978cc9feeb468fb8ee3a90607e3");
            var resource = Helpers.CreateAbilityResource("NecromancySchoolUnderstandingBaseResource", "", "", "", null);
            var base_ability = library.CopyAndAdd<BlueprintAbility>("39af648796b7b9b4ab6321898ebb5fff", "SchoolUnderstandingNecromancySchoolBaseAbility", ""); //shaken
            base_ability.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = resource);
            resource.SetIncreasedByStat(3, StatType.Charisma);
            var resource2 = Helpers.CreateAbilityResource("NecromancySchoolUnderstandingBase2Resource", "", "", "", null);
            var base_ability2 = library.CopyAndAdd<BlueprintAbility>("71b8898b1d26d654b9a3eeac87e3e2f8", "SchoolUnderstandingNecromancySchoolBase2Ability", ""); //turn undead
            base_ability2.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = resource2);
            resource2.SetIncreasedByStat(3, StatType.Charisma);

            var necromancy_buff = Helpers.CreateBuff("SchoolUnderstangingNecromancyBuff",
                                                          "School Understanding (" + necromancy_progression.Name + ")",
                                                          school_understanding.Description + "\n" + necromancy_progression.Name + ": " + necromancy_progression.Description,
                                                          "",
                                                          necromancy_progression.Icon,
                                                          null
                                                          );
            base_ability.AddComponent(Common.createAbilityCasterHasFacts(necromancy_buff));
            base_ability2.AddComponent(Common.createAbilityCasterHasFacts(necromancy_buff));
            var necromancy_feature = Helpers.CreateFeature("NecromancySchoolUnderstangingFeature",
                                                          necromancy_buff.Name,
                                                          necromancy_buff.Description,
                                                          "",
                                                          necromancy_buff.Icon,
                                                          FeatureGroup.None,
                                                          Helpers.CreateAddFacts(base_ability, base_ability2, createSchoolUnderstandingAbility(necromancy_buff)),
                                                          Helpers.CreateAddAbilityResource(resource),
                                                          Helpers.CreateAddAbilityResource(resource2),
                                                          Helpers.PrerequisiteNoFeature(necromancy_progression),
                                                          Helpers.Create<ReplaceAbilitiesStat>(r => { r.Ability = new BlueprintAbility[] { base_ability }; r.Stat = StatType.Charisma; }));
            ChannelEnergyEngine.addToImprovedChannel(base_ability2, necromancy_feature);

            return necromancy_feature;
        }


        static BlueprintFeature createDivination()
        {
            //divination
            var divination_progression = library.Get<BlueprintProgression>("d7d18ce5c24bd324d96173fdc3309646");
            var resource = Helpers.CreateAbilityResource("DivinationSchoolUnderstandingBaseResource", "", "", "", null);
            var base_ability = library.CopyAndAdd<BlueprintAbility>("0997652c1d8eb164caae8a462401a25d", "SchoolUnderstandingDivinationSchoolBaseAbility", ""); //diviner's fortune
            base_ability.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = resource);
            resource.SetIncreasedByStat(3, StatType.Charisma);
            var divination_base_feature = library.Get<BlueprintFeature>("54d21b3221ea82a4d90d5a91b7872f3d");

            var divination_buff = Helpers.CreateBuff("SchoolUnderstangingDivinationBuff",
                                                          "School Understanding (" + divination_progression.Name + ")",
                                                          school_understanding.Description + "\n" + divination_progression.Name + ": " + divination_progression.Description,
                                                          "",
                                                          divination_progression.Icon,
                                                          null,
                                                          Helpers.CreateAddFeatureOnClassLevel(library.Get<BlueprintFeature>("a6839f2a709288e43b68d3281b3bbd8f"), 20, getExploitsUserArray()));
            base_ability.AddComponent(Common.createAbilityCasterHasFacts(divination_buff));

            var divination_feature = Helpers.CreateFeature("DivinationSchoolUnderstangingFeature",
                                                          divination_buff.Name,
                                                          divination_buff.Description,
                                                          "",
                                                          divination_buff.Icon,
                                                          FeatureGroup.None,
                                                          divination_base_feature.GetComponent<AddContextStatBonus>(),
                                                          divination_base_feature.GetComponent<ContextRankConfig>(),
                                                          Helpers.CreateAddFacts(base_ability, createSchoolUnderstandingAbility(divination_buff)),
                                                          Helpers.CreateAddAbilityResource(resource),
                                                          Helpers.PrerequisiteNoFeature(divination_progression),
                                                          Helpers.PrerequisiteNoFeature(Subschools.prophecy),
                                                          Helpers.Create<ReplaceAbilitiesStat>(r => { r.Ability = new BlueprintAbility[] { base_ability }; r.Stat = StatType.Charisma; }));

            return divination_feature;
        }


        static BlueprintFeature createProphecy()
        {
            //divination
            var divination_progression = Subschools.prophecy;
            var resource = Helpers.CreateAbilityResource("ProphecySchoolUnderstandingBaseResource", "", "", "", null);
            var base_ability = library.CopyAndAdd<BlueprintAbility>("0997652c1d8eb164caae8a462401a25d", "SchoolUnderstandingProphecySchoolBaseAbility", ""); //diviner's fortune
            base_ability.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = resource);
            resource.SetIncreasedByStat(3, StatType.Charisma);

            var resource2 = Helpers.CreateAbilityResource("ProphecySchoolUnderstanding2BaseResource", "", "", "", null);
            var base_ability2 = library.CopyAndAdd<BlueprintAbility>(Subschools.inspiring_prediciton_ability, "SchoolUnderstandingProphecySchoolBase2Ability", ""); 
            base_ability2.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = resource2);
            resource2.SetIncreasedByStat(3, StatType.Charisma);

            var divination_buff = Helpers.CreateBuff("SchoolUnderstangingProphecyBuff",
                                                          "School Understanding (" + divination_progression.Name + ")",
                                                          school_understanding.Description + "\n" + divination_progression.Name + ": " + divination_progression.Description,
                                                          "",
                                                          divination_progression.Icon,
                                                          null);
            base_ability.AddComponent(Common.createAbilityCasterHasFacts(divination_buff));
            base_ability2.AddComponent(Common.createAbilityCasterHasFacts(divination_buff));
            var divination_feature = Helpers.CreateFeature("ProphecySchoolUnderstangingFeature",
                                                          divination_buff.Name,
                                                          divination_buff.Description,
                                                          "",
                                                          divination_buff.Icon,
                                                          FeatureGroup.None,
                                                          Helpers.CreateAddFacts(base_ability, base_ability2, createSchoolUnderstandingAbility(divination_buff)),
                                                          Helpers.CreateAddAbilityResource(resource),
                                                          Helpers.CreateAddAbilityResource(resource2),
                                                          Helpers.PrerequisiteNoFeature(library.Get<BlueprintProgression>("d7d18ce5c24bd324d96173fdc3309646")),
                                                          Helpers.PrerequisiteNoFeature(Subschools.prophecy),
                                                          Helpers.Create<ReplaceAbilitiesStat>(r => { r.Ability = new BlueprintAbility[] { base_ability }; r.Stat = StatType.Charisma; }));

            return divination_feature;
        }

        static void createSchoolUnderstanding()
        {
            var description = "The arcanist can select one arcane school from any of the schools available to a character with the arcane school wizard class feature, but does not have to select any opposition schools. As a swift action, the arcanist can expend 1 point from her arcane reservoir to bolster her understanding, allowing her to gain 1st-level school abilities and treat her arcanist level as her wizard level for the purpose of using these ability for a number of rounds equal to her Charisma modifier (minimum 1). Arcanist uses  Charisma modifier in place of her Intelligence modifier for these abilities.";
            school_understanding = Helpers.CreateFeatureSelection("SchoolUnderstandingExploit",
                                                      "School Understanding",
                                                      description,
                                                      "",
                                                      null,
                                                      FeatureGroup.None);
            school_understanding.AddComponents(Helpers.PrerequisiteNoFeature(school_understanding),
                                               Common.prerequisiteNoArchetype(school_savant_archetype)
                                               );

            school_understanding.AllFeatures = new BlueprintFeature[]
            {
                createAbjuration(),
                createConjuration(),
                createDivination(),
                createEnchantment(),
                createEvocation(),
                createTransmutation(),
                createNecromancy(),
                createIllusion(),
                createAdmixture(),
                createTeleportation(),
                createEnhancement(),
                createProphecy()
            };
            school_understanding.AddComponent(Common.prerequisiteNoArchetype(school_savant_archetype));
            arcane_exploits.AllFeatures = arcane_exploits.AllFeatures.AddToArray(school_understanding);
            arcane_exploits_wizard.AllFeatures = arcane_exploits_wizard.AllFeatures.AddToArray(school_understanding);
        }

        static BlueprintAbility createSchoolUnderstandingAbility(BlueprintBuff buff)
        {
            var ability = Helpers.CreateAbility(buff.name + "Ability",
                                                "Activate " + buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Swift,
                                                AbilityRange.Personal,
                                                "Charisma modifier rounds",
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false)),
                                                arcane_reservoir_resource.CreateResourceLogic(),
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, stat: StatType.Charisma, min: 1)
                                                );
            ability.setMiscAbilityParametersSelfOnly();
            return ability;
        }





        static void createSonicBlast()
        {
            var icon = library.Get<BlueprintAbility>("8e7cfa5f213a90549aadd18f8f6f4664").Icon; //ear-piercing scream

            var buff = Common.deafened;

            var base_damage = Helpers.CreateActionDealDamage(DamageEnergyType.Sonic,
                                     Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.Default), Helpers.CreateContextValue(AbilityRankType.DamageBonus)), halfIfSaved: true);

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false);
            var extra_effect = Helpers.CreateConditionalSaved(null, apply_buff);

            var sonic_blast_ability = Helpers.CreateAbility("SonicBlastExploitAbility",
                                                        "Sonic Blast",
                                                        $"The arcanist can loose a deafening blast of sonic energy by expending 1 point from her arcane reservoir at any one target within close range. The blast deals an amount of sonic damage equal to 1d{BalanceFixes.getDamageDieString(DiceType.D6)} + the arcanist’s Charisma modifier, plus an additional 1d{BalanceFixes.getDamageDie(DiceType.D6)} points of sonic damage for every 2 levels beyond 1st (to a maximum of 10d6 at 19th level). The target is also deafened for 1 minute. The target can attempt a Fortitude save to halve the damage and negate the deafness.",
                                                        "",
                                                        icon,
                                                        AbilityType.Supernatural,
                                                        CommandType.Standard,
                                                        AbilityRange.Close,
                                                        "",
                                                        "Fortitude partial",
                                                        Helpers.CreateResourceLogic(arcane_reservoir_resource, cost_is_custom: true),
                                                        Helpers.Create<NewMechanics.ResourseCostCalculatorWithDecreasingFacts>(r => r.cost_reducing_facts = new BlueprintFact[] { energy_absorption_buffs[DamageEnergyType.Sonic] }),
                                                        Helpers.CreateSpellDescriptor(SpellDescriptor.Sonic),
                                                        library.Get<BlueprintAbility>("8e7cfa5f213a90549aadd18f8f6f4664").GetComponent<AbilitySpawnFx>(),
                                                        Helpers.CreateRunActions(SavingThrowType.Fortitude, base_damage, extra_effect),
                                                        Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getExploitsUserArray(), progression: ContextRankProgression.OnePlusDiv2), //base damage
                                                        Helpers.CreateContextRankConfig(type: AbilityRankType.DamageBonus, baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Charisma, min: 0), //extra damage
                                                        Common.createContextCalculateAbilityParamsBasedOnClasses(getExploitsUserArray(), StatType.Charisma),
                                                        Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionOnContextCaster(Common.createContextActionRemoveBuff(energy_absorption_buffs[DamageEnergyType.Sonic]))))
                                                        );
            sonic_blast_ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            sonic_blast = Common.AbilityToFeature(sonic_blast_ability, false);
        }

        static void createMetamixing()
        {
            var buff = Helpers.CreateBuff("MetamixingExploitBuff",
                                            "Metamixing",
                                            "The arcanist can expend 1 point from her arcane reservoir to cast a spell with one spontaneously applied metamgic feat without affecting the casting time. She can also use this ability with spells that she prepared using metamagic feats.",
                                            "",
                                            LoadIcons.Image2Sprite.Create(@"AbilityIcons/Metamixing.png"),
                                            null,
                                            Helpers.Create<SpellManipulationMechanics.Metamixing>(),
                                            Helpers.Create<NewMechanics.SpendResourceOnSpellCast>(s => {s.spellbook = null; s.resource = arcane_reservoir_resource; s.used_for_reducing_metamagic_cast_time = true; s.is_metamixing = true; }));

            var ability = Helpers.CreateActivatableAbility("MetamixingExplotToggleAbility",
                                                           buff.Name,
                                                           buff.Description,
                                                           "",
                                                           buff.Icon,
                                                           buff,
                                                           AbilityActivationType.Immediately,
                                                           CommandType.Free,
                                                           null,
                                                           Helpers.CreateActivatableResourceLogic(arcane_reservoir_resource, ResourceSpendType.Never)
                                                           );
            ability.DeactivateImmediately = true;

            metamixing = Helpers.CreateFeature("MetamixingExploitFeature",
                                               ability.Name,
                                               ability.Description,
                                               "",
                                               ability.Icon,
                                               FeatureGroup.None,
                                               Helpers.CreateAddFact(ability)
                                               );

            metamixing_buff = buff;
        }


        static void createMetamagicKnowledgeAndGreaterMetamgicKnowledge()
        {
            var metamagic_feats = library.GetAllBlueprints().OfType<BlueprintFeature>().Where(b => b.Groups.Contains(FeatureGroup.WizardFeat) && (b.GetComponent<AddMetamagicFeat>() != null));

            metamagic_knowledge = Helpers.CreateFeatureSelection("MetamagicKnowledgeExploitFeature",
                                                                 "Metamagic Knowledge",
                                                                 "The arcanist can select one metamagic feat as a bonus feat. She must meet the prerequisites of this feat.",
                                                                 "",
                                                                 null,
                                                                 FeatureGroup.None);
            metamagic_knowledge.AllFeatures = metamagic_feats.ToArray();

            greater_metamagic_knowledge = library.CopyAndAdd<BlueprintFeatureSelection>(metamagic_knowledge.AssetGuid, "GreaterMetamagicKnowledgeExploit", "");
            greater_metamagic_knowledge.SetName("Greater Metamagic Knowledge");


            metamagic_knowledge.AddComponent(Helpers.PrerequisiteNoFeature(metamagic_knowledge));
            greater_metamagic_knowledge.AddComponent(Helpers.PrerequisiteNoFeature(greater_metamagic_knowledge));
            greater_metamagic_knowledge.AddComponent(Helpers.PrerequisiteFeature(metamagic_knowledge));
            greater_metamagic_knowledge.AddComponent(Helpers.PrerequisiteFeature(greater_arcane_exploits));
        }



        static void createLightningLanceAndDancingElectricity()
        {
            var base_damage = Helpers.CreateActionDealDamage(DamageEnergyType.Electricity,
                                                 Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.Default), Helpers.CreateContextValue(AbilityRankType.DamageBonus)));
            var outgoing_concealement_buff = Helpers.CreateBuff("LightningLanceBuff",
                                                                "Lightning Lance",
                                                                $"The arcanist can unleash a lance of lightning by expending 1 point from her arcane reservoir and making a ranged touch attack against any one target within close range. If the attack hits, it deals 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of electricity damage + the arcanist’s Charisma modifier, plus 1d{BalanceFixes.getDamageDieString(DiceType.D6)}  points of electricity damage for every 2 levels beyond 1st (to a maximum of 10d{BalanceFixes.getDamageDieString(DiceType.D6)}  at 19th level). The target’s vision is also impaired, causing the target to treat all creatures as if they had concealment (20%) for 1 round. It can attempt a Fortitude saving throw to negate the impaired vision.",
                                                                "",
                                                                library.Get<BlueprintAbility>("b3494639791901e4db3eda6117ad878f").Icon, //air domain base ability
                                                                null,
                                                                Helpers.CreateSpellDescriptor(SpellDescriptor.SightBased),
                                                                Helpers.Create<ConcealementMechanics.AddOutgoingConcealment>(a => { a.Descriptor = ConcealmentDescriptor.InitiatorIsBlind; a.Concealment = Concealment.Partial; })
                                                                );

            var apply_buff = Common.createContextActionApplyBuff(outgoing_concealement_buff, Helpers.CreateContextDuration(1), dispellable: false);
            var extra_effect = Helpers.CreateConditionalSaved(null, apply_buff);

            var lightning_lance_ability = Helpers.CreateAbility("LightningLanceExploitAbility",
                                                        outgoing_concealement_buff.Name,
                                                        outgoing_concealement_buff.Description,
                                                        "",
                                                        outgoing_concealement_buff.Icon,
                                                        AbilityType.Supernatural,
                                                        CommandType.Standard,
                                                        AbilityRange.Close,
                                                        "",
                                                        "Fortitude partial",
                                                        Helpers.CreateResourceLogic(arcane_reservoir_resource, cost_is_custom: true),
                                                        Helpers.Create<NewMechanics.ResourseCostCalculatorWithDecreasingFacts>(r => r.cost_reducing_facts = new BlueprintFact[] { energy_absorption_buffs[DamageEnergyType.Electricity] }),
                                                        Helpers.CreateSpellDescriptor(SpellDescriptor.Electricity),
                                                        library.Get<BlueprintAbility>("b3494639791901e4db3eda6117ad878f").GetComponent<AbilityDeliverProjectile>(),
                                                        Helpers.CreateRunActions(SavingThrowType.Fortitude, base_damage, extra_effect),
                                                        Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getExploitsUserArray(), progression: ContextRankProgression.OnePlusDiv2), //base damage
                                                        Helpers.CreateContextRankConfig(type: AbilityRankType.DamageBonus, baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Charisma, min: 0), //extra damage
                                                        Common.createContextCalculateAbilityParamsBasedOnClasses(getExploitsUserArray(), StatType.Charisma),
                                                        Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionOnContextCaster(Common.createContextActionRemoveBuff(energy_absorption_buffs[DamageEnergyType.Electricity]))))
                                                        );
            lightning_lance_ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            lightning_lance = Common.AbilityToFeature(lightning_lance_ability, false);

            var adjacent_damage = Helpers.CreateActionDealDamage(DamageEnergyType.Electricity,
                                                 Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.Default), Helpers.CreateContextValue(AbilityRankType.DamageBonus)), halfIfSaved: true);
            adjacent_damage.Half = true;

            var adjacent_action = Common.createContextActionSavingThrow(SavingThrowType.Reflex, Helpers.CreateActionList(adjacent_damage));
            var action = Helpers.Create<TeamworkMechanics.ContextActionOnUnitsWithinRadius>(c =>
                                                                                            {
                                                                                                c.actions = Helpers.CreateActionList(adjacent_action);
                                                                                                c.ignore_target = true;
                                                                                                c.Radius = 5.Feet();
                                                                                            }
                                                                                            );

            var dancing_electricity_ability = library.CopyAndAdd<BlueprintAbility>(lightning_lance_ability.AssetGuid, "DancingElectricityArcaneExploitAbility", "");

            dancing_electricity_ability.ReplaceComponent<AbilityResourceLogic>(Helpers.CreateResourceLogic(arcane_reservoir_resource, amount: 2, cost_is_custom: true));
            dancing_electricity_ability.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(SavingThrowType.Fortitude, base_damage, extra_effect, action));
            dancing_electricity_ability.SetNameDescriptionIcon("Dancing Electricity",
                                                               "Whenever the arcanist uses the lightning lance exploit, she can expend 2 points from her arcane reservoir instead of one. If she does, all creatures adjacent to the target take an amount of damage equal to half the amount of electricity damage rolled. Adjacent creatures can attempt a Reflex saving throw to halve this damage. Whether or not the target makes its saving throw has no effect on adjacent targets. The arcanist must have the lightning lance exploit to select this exploit.",
                                                               library.Get<BlueprintAbility>("2a9ef0e0b5822a24d88b16673a267456").Icon); //call lightning

            dancing_electricity = Common.AbilityToFeature(dancing_electricity_ability, false);
            dancing_electricity.AddComponent(Helpers.PrerequisiteFeature(greater_arcane_exploits));
            dancing_electricity.AddComponent(Helpers.PrerequisiteFeature(lightning_lance));
        }


        static void createIceMissileAndIcyTomb()
        {
            var base_damage = Helpers.CreateActionDealDamage(DamageEnergyType.Cold,
                                                             Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.Default), Helpers.CreateContextValue(AbilityRankType.DamageBonus)));
            var staggered = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");
            var apply_staggered = Common.createContextActionApplyBuff(staggered, Helpers.CreateContextDuration(1), dispellable: false);
            var extra_effect = Helpers.CreateConditionalSaved(null, apply_staggered);
            var icon = library.Get<BlueprintAbility>("5e1db2ef80ff361448549beeb7785791").Icon; //ice ray from water domain
            var ice_missile_ability = Helpers.CreateAbility("IceMissileExploitAbility",
                                                        "Ice Missile",
                                                        $"The arcanist can unleash a freezing projectile by expending 1 point from her arcane reservoir and making a ranged touch attack against any one target within close range. If the attack hits, it deals 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of cold damage + the arcanist’s Charisma modifier, plus an additional 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of cold damage for every 2 levels beyond 1st (to a maximum of 10d6 at 19th level). In addition, the target is staggered for 1 round. It can attempt a Fortitude saving throw to negate the staggered condition.",
                                                        "",
                                                        icon,
                                                        AbilityType.Supernatural,
                                                        CommandType.Standard,
                                                        AbilityRange.Close,
                                                        "",
                                                        "Fortitude partial",
                                                        Helpers.CreateResourceLogic(arcane_reservoir_resource, cost_is_custom: true),
                                                        Helpers.Create<NewMechanics.ResourseCostCalculatorWithDecreasingFacts>(r => r.cost_reducing_facts = new BlueprintFact[] { energy_absorption_buffs[DamageEnergyType.Cold] }),
                                                        Helpers.CreateSpellDescriptor(SpellDescriptor.Cold),
                                                        library.Get<BlueprintAbility>("5e1db2ef80ff361448549beeb7785791").GetComponent<AbilityDeliverProjectile>(),
                                                        Helpers.CreateRunActions(SavingThrowType.Fortitude, base_damage, extra_effect),
                                                        Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getExploitsUserArray(), progression: ContextRankProgression.OnePlusDiv2), //base damage
                                                        Helpers.CreateContextRankConfig(type: AbilityRankType.DamageBonus, baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Charisma, min: 0), //extra damage
                                                        Common.createContextCalculateAbilityParamsBasedOnClasses(getExploitsUserArray(), StatType.Charisma),
                                                        Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionOnContextCaster(Common.createContextActionRemoveBuff(energy_absorption_buffs[DamageEnergyType.Cold]))))
                                                        );
            ice_missile_ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            ice_missile = Common.AbilityToFeature(ice_missile_ability, false);

            var ice_tomb_icon = library.Get<BlueprintAbility>("65e8d23aef5e7784dbeb27b1fca40931").Icon;
            var entangle_buff = library.CopyAndAdd<BlueprintBuff>("f7f6330726121cf4b90a6086b05d2e38", "IcyTombExploitBuff", "");
            entangle_buff.SetNameDescriptionIcon("Icy Tomb",
                                                 "Whenever the arcanist uses the ice missile exploit, she can expend 2 points from her arcane reservoir instead of one. If she does, the target is coated in rime if it fails its saving throw. As long as the ice remains (typically 1 minute per level in a warm area), the target is entangled (although not anchored) and takes 1 point of Dexterity damage at the start of each of its turns. The target can break free from the ice as a standard action by making a Strength check with a DC equal to 10 + the arcanist’s Charisma modifier. If the target takes more than 10 points of fire damage from a single attack, the ice melts and the effect ends. The arcanist must have the ice missile arcanist exploit to select this exploit.",
                                                 ice_tomb_icon);

            var dex_damage = Helpers.CreateActionDealDamage(StatType.Dexterity, Helpers.CreateContextDiceValue(DiceType.Zero, 0, 1), IgnoreCritical: true);
            entangle_buff.ReplaceComponent<AddFactContextActions>(a => a.NewRound = Helpers.CreateActionList(dex_damage, a.NewRound.Actions[0]));
            entangle_buff.FxOnStart = library.Get<BlueprintBuff>("c53b286bb06a0544c85ca0f8bcc86950").FxOnStart; //icy prison
            entangle_buff.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Cold));
            entangle_buff.AddComponent(Helpers.Create<NewMechanics.ActionOnDamageReceived>(a =>
                                                                                          {
                                                                                              a.min_dmg = 10;
                                                                                              a.energy = DamageEnergyType.Fire;
                                                                                              a.action = Helpers.CreateActionList(Common.createContextActionRemoveBuff(entangle_buff));
                                                                                          }
                                                                                          )
                                      );

            var apply_entangle = Helpers.CreateConditionalSaved(null, Common.createContextActionApplyBuff(entangle_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus), DurationRate.Minutes), dispellable: false));
            var icy_tomb_ability = library.CopyAndAdd<BlueprintAbility>(ice_missile_ability.AssetGuid, "IcyTombArcaneExploitAbility", "");

            var extra_effect2 = Helpers.CreateConditionalSaved(null, new GameAction[] { apply_staggered, apply_entangle });
            icy_tomb_ability.ReplaceComponent<AbilityResourceLogic>(Helpers.CreateResourceLogic(arcane_reservoir_resource, amount: 2, cost_is_custom: true));
            icy_tomb_ability.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(SavingThrowType.Fortitude, base_damage, extra_effect2));
            icy_tomb_ability.SetNameDescriptionIcon(entangle_buff.Name,
                                                    entangle_buff.Description,
                                                    entangle_buff.Icon);

            icy_tomb = Common.AbilityToFeature(icy_tomb_ability, false);
            icy_tomb.AddComponent(Helpers.PrerequisiteFeature(greater_arcane_exploits));
            icy_tomb.AddComponent(Helpers.PrerequisiteFeature(ice_missile));
        }


        static void createForceStrike()
        {
            var force_missile = library.Get<BlueprintAbility>("3d55cc710cc497843bb51788057cd93f");
            var magic_missile = library.Get<BlueprintAbility>("4ac47ddb9fa1eaf43a1b6809980cfbd2");

            var damage = Helpers.CreateActionDealDamage(DamageEnergyType.Magic, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D4), 1, Helpers.CreateContextValue(AbilityRankType.Default)));
            damage.DamageType = Common.createForceDamageDescription();

            var ability = Helpers.CreateAbility("ForceStrikeExploitAbility",
                                                "Force Strike",
                                                $"The arcanist can unleash a blast of force by expending 1 point from her arcane reservoir. This attack automatically strikes one target within close range (as magic missile) and deals 1d{BalanceFixes.getDamageDieString(DiceType.D4)} points of force damage, plus 1 point of damage per arcanist level. Spells and effects that negate magic missile also negate this effect.",
                                                "",
                                                force_missile.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Close,
                                                "",
                                                "",
                                                force_missile.GetComponent<AbilityDeliverProjectile>(),
                                                Helpers.CreateRunActions(damage),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getExploitsUserArray()),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.Force),
                                                Helpers.CreateResourceLogic(arcane_reservoir_resource)
                                                );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);

            force_strike = Common.AbilityToFeature(ability, false);

            var buffs = library.GetAllBlueprints().OfType<BlueprintBuff>();

            //add immunity as for magic missile
            foreach (var b in buffs)
            {
                var spell_immunities = b.GetComponents<AddSpellImmunity>().Where(a => a.Exceptions != null && a.Exceptions.Contains(magic_missile));

                foreach (var si in spell_immunities)
                {
                    si.Exceptions = si.Exceptions.AddToArray(ability);
                }
            }
        }


        static void createHolyWaterJet()
        {
            var base_damage = Helpers.CreateActionDealDamage(DamageEnergyType.Holy,
                                                             Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D8), Helpers.CreateContextValue(AbilityRankType.Default), Helpers.CreateContextValue(AbilityRankType.DamageBonus)),
                                                             isAoE: true, halfIfSaved: true);
            var damage = Helpers.CreateConditional(Common.createContextConditionHasFact(Common.undead),
                                                   base_damage,
                                                   Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Common.createContextConditionHasFact(Common.outsider),
                                                                                                                Helpers.CreateContextConditionAlignment(AlignmentComponent.Evil)),
                                                                             base_damage
                                                                            )
                                                  );
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/HolyWaterJet.png");
            var ability = Helpers.CreateAbility("HolyWaterJetExploitAbility",
                                                        "Holy Water Jet",
                                                        $"The arcanist can unleash a jet of holy water by expending 1 point from her arcane reservoir. This creates a 30-foot line of water that deals damage equal to 1d{BalanceFixes.getDamageDieString(DiceType.D8)} points of damage plus the arcanist’s Charisma modifier, plus an additional 1d8 points of damage for every 2 levels beyond 1st (to a maximum of 10d8 at 19th level) to each target in the line that would normally take damage from holy water. Creatures in the area of effect can attempt a Reflex saving throw to halve the damage.",
                                                        "",
                                                        icon,
                                                        AbilityType.Supernatural,
                                                        CommandType.Standard,
                                                        AbilityRange.Projectile,
                                                        "",
                                                        Helpers.reflexHalfDamage,
                                                        Helpers.CreateResourceLogic(arcane_reservoir_resource),
                                                        library.Get<BlueprintAbility>("93cc42235edc6824fa7d54b83ed4e1fe").GetComponent<AbilityDeliverProjectile>(), // water torrent
                                                        Helpers.CreateRunActions(SavingThrowType.Reflex, damage),
                                                        Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getExploitsUserArray(), progression: ContextRankProgression.OnePlusDiv2), //base damage
                                                        Helpers.CreateContextRankConfig(type: AbilityRankType.DamageBonus, baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Charisma, min: 0), //extra damage
                                                        Common.createContextCalculateAbilityParamsBasedOnClasses(getExploitsUserArray(), StatType.Charisma)
                                                        );
            ability.setMiscAbilityParametersRangedDirectional();
            holy_water_jet = Common.AbilityToFeature(ability, false);
        }


        static void createFlameArcAndBurningFlame()
        {
            var base_damage = Helpers.CreateActionDealDamage(DamageEnergyType.Fire, 
                                                             Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.Default), Helpers.CreateContextValue(AbilityRankType.DamageBonus)),
                                                             isAoE: true, halfIfSaved: true);                                  
            var icon = library.Get<BlueprintAbility>("ebade19998e1f8542a1b55bd4da766b3").Icon;
            var flame_arc_ability = Helpers.CreateAbility("FlameArcExploitAbility",
                                                        "Flame Arc",
                                                        $"The arcanist can unleash an arc of flame by expending 1 point from her arcane reservoir. This creates a 30-foot line of flame that deals 1d{BalanceFixes.getDamageDie(DiceType.D6)} points of fire damage + the arcanist’s Charisma modifier, plus an additional 1d{BalanceFixes.getDamageDieString(DiceType.D6)}  points of fire damage for every 2 levels beyond 1st (to a maximum of 10d{BalanceFixes.getDamageDieString(DiceType.D6)}  at 19th level) to each target in the line. Creatures in the area of effect may attempt a Reflex saving throw to halve the damage.",
                                                        "",
                                                        icon,
                                                        AbilityType.Supernatural,
                                                        CommandType.Standard,
                                                        AbilityRange.Projectile,
                                                        "",
                                                        Helpers.reflexHalfDamage,
                                                        Helpers.CreateResourceLogic(arcane_reservoir_resource, cost_is_custom: true),
                                                        Helpers.Create<NewMechanics.ResourseCostCalculatorWithDecreasingFacts>(r => r.cost_reducing_facts = new BlueprintFact[] { energy_absorption_buffs[DamageEnergyType.Fire] }),
                                                        Helpers.CreateSpellDescriptor(SpellDescriptor.Fire),
                                                        library.Get<BlueprintAbility>("5e4c7cb990de4034bbee9fb99be2e15d").GetComponent<AbilityDeliverProjectile>(),
                                                        Helpers.CreateRunActions(SavingThrowType.Reflex, base_damage),
                                                        Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getExploitsUserArray(), progression: ContextRankProgression.OnePlusDiv2), //base damage
                                                        Helpers.CreateContextRankConfig(type: AbilityRankType.DamageBonus, baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Charisma, min: 0), //extra damage
                                                        Common.createContextCalculateAbilityParamsBasedOnClasses(getExploitsUserArray(), StatType.Charisma),
                                                        Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionOnContextCaster(Common.createContextActionRemoveBuff(energy_absorption_buffs[DamageEnergyType.Fire]))))
                                                        );
            flame_arc_ability.setMiscAbilityParametersRangedDirectional(test_mode);
            flame_arc = Common.AbilityToFeature(flame_arc_ability, false);

            var burn3d6 = library.CopyAndAdd<BlueprintBuff>("e92ecaa76b5db674fa5b0aaff5b21bc9", "ArcaneExploitBurningFlameBuff", "");
            burn3d6.SetDescription("This creature is burning and takes 3d6 fire damage each turn.");

            var burn_damage = Helpers.CreateActionDealDamage(DamageEnergyType.Fire,
                                                             Helpers.CreateContextDiceValue(DiceType.D6, 3, 0));
            burn3d6.ReplaceComponent<AddFactContextActions>(a => a.NewRound = Helpers.CreateActionList(burn_damage, a.NewRound.Actions[1]));

            var apply_burn = Helpers.CreateConditionalSaved(null, Common.createContextActionApplyBuff(burn3d6, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false));

            var burning_flame_ability = library.CopyAndAdd<BlueprintAbility>(flame_arc_ability.AssetGuid, "BurningFlameArcaneExploitAbility", "");

            burning_flame_ability.ReplaceComponent<AbilityResourceLogic>(Helpers.CreateResourceLogic(arcane_reservoir_resource, amount: 2, cost_is_custom: true));
            burning_flame_ability.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(SavingThrowType.Reflex, base_damage, apply_burn));
            burning_flame_ability.SetNameDescriptionIcon("Burning Flame",
                                                         "Whenever the arcanist uses the flame arc exploit, she can expend 2 points from her arcane reservoir instead of one. If she does, each target catches on fire if it fails its saving throw. Until the fire is extinguished, the target takes 3d6 points of fire damage at the start of each of its turns. The target can attempt a Reflex saving throw as a full-round action to extinguish the flames. Applying at least 1 gallon of water to the target automatically extinguishes the flames. The arcanist must have the flame arc exploit to select this exploit.",
                                                         burn3d6.Icon);

            burning_flame = Common.AbilityToFeature(burning_flame_ability, false);
            burning_flame.AddComponent(Helpers.PrerequisiteFeature(greater_arcane_exploits));
            burning_flame.AddComponent(Helpers.PrerequisiteFeature(flame_arc));
        }


        static void createDimensionalSlide()
        {
            var ability = library.CopyAndAdd<BlueprintAbility>("a9b8be9b87865744382f7c64e599aeb2", "DimensionalSlideExploitAbility", "");

            ability.Type = AbilityType.Supernatural;
            ability.ActionType = CommandType.Move;

            ability.SetNameDescription("Dimensional Slide",
                                       "The arcanist can expend 1 point from her arcane reservoir to create a dimensional crack that she can step through to reach another location. This ability is used as a move action, allowing her to move to any point within long range. She can only use this ability once per round. She does not provoke attacks of opportunity when moving in this way.");
            ability.AddComponent(Helpers.CreateResourceLogic(arcane_reservoir_resource));

            var cooldown_buff = Helpers.CreateBuff("DimensionalSlideExploitCooldownBuff",
                                                   "Dimensional Slide: Cooldown",
                                                   ability.Description,
                                                   "",
                                                   ability.Icon,
                                                   null);
            var apply_cooldown = Common.createContextActionApplyBuffToCaster(cooldown_buff, Helpers.CreateContextDuration(1), dispellable: false);

            ability.AddComponent(Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(apply_cooldown)));
            ability.AddComponent(Common.createAbilityCasterHasNoFacts(cooldown_buff));

            dimensional_slide = Common.AbilityToFeature(ability, false);
        }


        static void createFamiliar()
        {
            familiar = library.CopyAndAdd<BlueprintFeature>("363cab72f77c47745bf3a8807074d183", "FamilairExploit", "");
            familiar.Groups = new FeatureGroup[] { FeatureGroup.None };
            familiar.ComponentsArray = new BlueprintComponent[0];
            familiar.SetDescription("An arcanist with this exploit can acquire a familiar as the arcane bond wizard class feature.");
            familiar.DlcType = DlcType.None;
            familiar.IsClassFeature = false;
            familiar.AddComponent(Helpers.PrerequisiteNoFeature(familiar));
        }


        static void createFeralShifting()
        {
            var buff = Helpers.CreateBuff("FeralShiftingExploitBuff",
                                               "Feral Shifting",
                                               "As a swift action, the arcanist can spend 1 point from her arcane reservoir to gain a bite attack that deals 1d6 points of damage (1d4 points of damage if she is Small) for a number of minutes equal to her Charisma modifier (minimum 1).",
                                               "",
                                               library.Get<BlueprintFeature>("25954b1652bebc2409f9cb9d5728bceb").Icon, //animal fury
                                               null,
                                               Common.createAddAdditionalLimb(library.Get<Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon>("a000716f88c969c499a535dadcf09286")) //bite 1d6
                                               );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), dispellable: false);
            var ability = Helpers.CreateAbility("FeralShiftingExploitAbility",
                                                   buff.Name,
                                                   buff.Description,
                                                   "",
                                                   buff.Icon,
                                                   AbilityType.Supernatural,
                                                   CommandType.Swift,
                                                   AbilityRange.Personal,
                                                   "Charisma modifier minutes (minimum 1)",
                                                   "",
                                                   Helpers.CreateRunActions(apply_buff),
                                                   Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Charisma, min: 1),
                                                   Common.createAbilitySpawnFx("352469f228a3b1f4cb269c7ab0409b8e", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                   Helpers.CreateResourceLogic(arcane_reservoir_resource)
                                                   );
            ability.setMiscAbilityParametersSelfOnly();

            feral_shifting = Common.AbilityToFeature(ability, false);
        }

        static void createAcidJetAndLingeringAcid()
        {
            var acid_arrow = library.Get<BlueprintAbility>("9a46dfd390f943647ab4395fc997936d");
            var base_damage = Helpers.CreateActionDealDamage(DamageEnergyType.Acid, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.Default), Helpers.CreateContextValue(AbilityRankType.DamageBonus)));
            var sickened = library.Get<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323");
            var apply_sickened = Common.createContextActionApplyBuff(sickened, Helpers.CreateContextDuration(0, DurationRate.Rounds, DiceType.D4, 1), dispellable: false);                                                                                                    
            var extra_effect = Helpers.CreateConditionalSaved(null, new GameAction[] { apply_sickened});

            var acid_jet_ability = Helpers.CreateAbility("AcidJetExploitAbility",
                                                        "Acid Jet",
                                                        $"The arcanist can unleash a jet of acid by expending 1 point from her arcane reservoir and making a ranged touch attack against any one target within close range. If the attack hits, it deals 1d{BalanceFixes.getDamageDieString(DiceType.D6)}  points of acid damage + the arcanist’s Charisma modifier, plus an additional 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of acid damage for every 2 levels beyond 1st (to a maximum of 10d{BalanceFixes.getDamageDieString(DiceType.D6)} at 19th level). The target is also sickened for 1d4 rounds. It can attempt a Fortitude saving throw to negate the sickened condition.",
                                                        "",
                                                        acid_arrow.Icon,
                                                        AbilityType.Supernatural,
                                                        CommandType.Standard,
                                                        AbilityRange.Close,
                                                        "",
                                                        "Fortitude partial",
                                                        Helpers.CreateResourceLogic(arcane_reservoir_resource, cost_is_custom: true),
                                                        Helpers.Create<NewMechanics.ResourseCostCalculatorWithDecreasingFacts>(r => r.cost_reducing_facts = new BlueprintFact[] {energy_absorption_buffs[DamageEnergyType.Acid]}),
                                                        acid_arrow.GetComponent<AbilityDeliverProjectile>(),
                                                        Helpers.CreateSpellDescriptor(SpellDescriptor.Acid),
                                                        Helpers.CreateRunActions(SavingThrowType.Fortitude, base_damage, extra_effect),
                                                        Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getExploitsUserArray(), progression: ContextRankProgression.OnePlusDiv2), //base damage
                                                        Helpers.CreateContextRankConfig(type: AbilityRankType.DamageBonus, baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Charisma, min: 0), //extra damage
                                                        Common.createContextCalculateAbilityParamsBasedOnClasses(getExploitsUserArray(), StatType.Charisma),
                                                        Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionOnContextCaster(Common.createContextActionRemoveBuff(energy_absorption_buffs[DamageEnergyType.Acid]))))
                                                        );
            acid_jet_ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            acid_jet = Common.AbilityToFeature(acid_jet_ability);

            //lingering buffs 5d6 -> 2d6 -> 1d6
            //                4d6 -> 2d6 -> 1d6
            //                3d6 -> 1d6
            //                2d6 -> 1d6
            //                1d6


            lingering_acid = Helpers.CreateFeature("LingeringAcidExploitFeature",
                                                   "Lingering Acid",
                                                   $"Whenever the arcanist uses the acid jet exploit, she can expend 2 points from her arcane reservoir instead of one. If she does, the target takes additional damage on the following rounds if it fails its saving throw. The target takes 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of acid damage on the following round for every 2d{BalanceFixes.getDamageDieString(DiceType.D6)} points of acid damage dealt by the initial attack. On subsequent rounds, the target continues to take 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of acid damage for every 2d{BalanceFixes.getDamageDieString(DiceType.D6)} points of acid damage dealt on the previous round. The damage continues until the amount of acid damage dealt on the previous round by this effect is 1d{BalanceFixes.getDamageDieString(DiceType.D6)}. For example, a 9th level arcanist would deal 5d{BalanceFixes.getDamageDieString(DiceType.D6)} points of acid damage + the arcanist’s Charisma modifier, 2d6 points of acid damage on the following round, and 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of acid damage on the third and final round. The arcanist must have the acid jet exploit to select this exploit.",
                                                   "",
                                                   LoadIcons.Image2Sprite.Create(@"AbilityIcons/LingeringAcid.png"),
                                                   FeatureGroup.None,
                                                   Helpers.PrerequisiteFeature(greater_arcane_exploits),
                                                   Helpers.PrerequisiteFeature(acid_jet)
                                                   );

            var damage_buffs = new BlueprintBuff[5];
            var apply_extra_damage_buff_actions = new ActionList[5];

            for (int i = 0; i < 5; i++)
            {
                damage_buffs[i] = Helpers.CreateBuff($"LingeringAcidExploit{i + 1}Buff",
                                                     lingering_acid.Name + $" ({i + 1}d6)",
                                                     lingering_acid.Description,
                                                     "",
                                                     lingering_acid.Icon,
                                                     null,
                                                     Helpers.CreateSpellDescriptor(SpellDescriptor.Acid)
                                                     );
                if ((i + 1) / 2 > 0)
                {
                    var apply_extra_buff = Common.createContextActionApplyBuff(damage_buffs[(i + 1) / 2 - 1], Helpers.CreateContextDuration(1), dispellable: false);
                    damage_buffs[i].AddComponent(Helpers.CreateAddFactContextActions(deactivated: new GameAction[] { Helpers.CreateActionDealDamage(DamageEnergyType.Acid, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), i + 1, 0)),
                                                                                        apply_extra_buff })
                                                );
                }
                else
                {
                    damage_buffs[i].AddComponent(Helpers.CreateAddFactContextActions(deactivated: Helpers.CreateActionDealDamage(DamageEnergyType.Acid, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), i + 1, 0))));
                }
               
                apply_extra_damage_buff_actions[i] = Helpers.CreateActionList(Common.createContextActionApplyBuff(damage_buffs[i], Helpers.CreateContextDuration(1), dispellable: false));
            }

            var lingering_effect = Common.createRunActionsDependingOnContextValueIgnoreNegative(Helpers.CreateContextValue(AbilityRankType.DamageDiceAlternative), apply_extra_damage_buff_actions);
            var extra_lingering = Helpers.CreateConditionalSaved(null, new GameAction[] { apply_sickened, lingering_effect });

            var lingering_acid_ability = library.CopyAndAdd<BlueprintAbility>(acid_jet_ability.AssetGuid, "LingeringAcidExploitAbility", "");
            lingering_acid_ability.SetNameDescriptionIcon(lingering_acid.Name, lingering_acid.Description, lingering_acid.Icon);
            lingering_acid_ability.AddComponent(Helpers.CreateContextRankConfig(type: AbilityRankType.DamageDiceAlternative, baseValueType: ContextRankBaseValueType.ClassLevel, classes: getExploitsUserArray(), progression: ContextRankProgression.DelayedStartPlusDivStep, startLevel: 3, stepLevel: 4));
            lingering_acid_ability.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(SavingThrowType.Fortitude, base_damage, extra_lingering));
            lingering_acid_ability.ReplaceComponent<AbilityResourceLogic>(Helpers.CreateResourceLogic(arcane_reservoir_resource, amount: 2, cost_is_custom: true));

            lingering_acid.AddComponent(Helpers.CreateAddFact(lingering_acid_ability));
        }


        static void createEnergyShieldAndEnergyAbsorption()
        {
            energy_absorption = Helpers.CreateFeature("EnergyAbsorptionExploitFeature",
                                                      "Energy Absorption",
                                                      "Whenever the arcanist is using the energy shield exploit, and the shield prevents 10 or more points of damage, she can absorb a portion of that energy and use it to fuel her exploits. After absorbing the damage, she can use any exploit that deals the same type of energy damage as the type her shield absorbed, reducing the cost to her arcane reservoir by 1 point. She must use this energy within 1 minute or it is lost. The arcanist does not gain more than one such use of energy per round, and she cannot store more than one use of this energy at a time. The arcanist must have the energy shield exploit to select this exploit.",
                                                      "",
                                                      library.Get<BlueprintActivatableAbility>("1452fb3e0e3e2f6488bee09050097b6f").Icon,
                                                      FeatureGroup.None,
                                                      Helpers.PrerequisiteFeature(greater_arcane_exploits)
                                                      );

            var resist_energy_base = library.Get<BlueprintAbility>("21ffef7791ce73f468b6fca4d9371e8b");

            var resist_variants = resist_energy_base.Variants;
            var energy = new DamageEnergyType[] { DamageEnergyType.Acid, DamageEnergyType.Cold, DamageEnergyType.Electricity, DamageEnergyType.Fire, DamageEnergyType.Sonic };

            BlueprintAbility[] energy_shield_variants = new BlueprintAbility[energy.Length];
            GameAction[] remove_buffs = new GameAction[energy.Length];

            for (int i = 0; i < energy_shield_variants.Length; i++)
            {
                var absorption_buff = Helpers.CreateBuff("EnergyAbsorptionExploit" + energy[i].ToString() + "Buff",
                                                         "Energy Absorption: " + energy[i].ToString(),
                                                         energy_absorption.Description,
                                                         "",
                                                         energy_absorption.Icon,
                                                         null);
                energy_absorption_buffs.Add(energy[i], absorption_buff);
                var apply_absorption_buff = Common.createContextActionApplyBuff(absorption_buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false);

                var buff = Helpers.CreateBuff("EnergyShieldExploit" + energy[i].ToString() + "Buff",
                                              "Energy Shield: " + energy[i].ToString(),
                                              "The arcanist can protect herself from energy damage as a standard action by expending 1 point from her arcane reservoir. She must pick one energy type and gains resistance 10 against that energy type for 1 minute per arcanist level. This protection increases by 5 for every 5 levels the arcanist possesses (up to a maximum of 30 at 20th level).",
                                              "",
                                              resist_variants[i].Icon,
                                              null,
                                              Common.createEnergyDRContextRank(energy[i], AbilityRankType.Default, 5),
                                              Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.StartPlusDivStep,
                                                                               startLevel: -5, stepLevel: 5, max: 6, classes: getExploitsUserArray()),
                                              Helpers.Create<NewMechanics.ActionOnDamageAbsorbed>(a =>
                                              {
                                                  a.min_dmg = 10;
                                                  a.energy = energy[i];
                                                  a.action = Helpers.CreateActionList(Helpers.CreateConditional(Common.createContextConditionHasFact(energy_absorption), apply_absorption_buff));
                                              })
                                              );

                var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), dispellable: false);
                energy_shield_variants[i] = Helpers.CreateAbility("EnergyShieldExploit" + energy[i].ToString() + "Ability",
                                                                  buff.Name,
                                                                  buff.Description,
                                                                  "",
                                                                  buff.Icon,
                                                                  AbilityType.Supernatural,
                                                                  CommandType.Standard,
                                                                  AbilityRange.Personal,
                                                                  "1 minute/ arcanist level",
                                                                  "",
                                                                  Helpers.CreateRunActions(apply_buff),
                                                                  Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getExploitsUserArray()),
                                                                  resist_variants[i].GetComponent<AbilitySpawnFx>(),
                                                                  Helpers.CreateResourceLogic(arcane_reservoir_resource)
                                                                  );
                energy_shield_variants[i].setMiscAbilityParametersSelfOnly();
                remove_buffs[i] = Common.createContextActionRemoveBuff(buff);
            }

            for (int i = 0; i < energy_shield_variants.Length; i++)
            {
                energy_shield_variants[i].AddComponent(Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(remove_buffs)));
            }

            var energy_shield_wrapper = Common.createVariantWrapper("EnergyShieldExploitAbility", "", energy_shield_variants);
            energy_shield_wrapper.SetName("Energy Shield");
            energy_shield_wrapper.SetIcon(resist_variants[0].Parent.Icon);

            energy_shield = Common.AbilityToFeature(energy_shield_wrapper);
            energy_absorption.AddComponent(Helpers.PrerequisiteFeature(energy_shield));
        }



        static void createArcaneWeapon()
        {
            var arcane_weapon_magus = library.Get<BlueprintAbility>("3c89dfc82c2a3f646808ea250eb91b91");
            var enchants = WeaponEnchantments.temporary_enchants;

            var enhancement_buff = Helpers.CreateBuff("ArcanistArcaneWeaponEnchancementBaseBuff",
                                         "",
                                         "",
                                         "",
                                         null,
                                         null,
                                         Common.createBuffRemainingGroupsSizeEnchantPrimaryHandWeapon(arcane_weapon_group,
                                                                                                      false, true,
                                                                                                      enchants
                                                                                                      )
                                         );
            enhancement_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var arcane_weapon_enhancement_buff = Helpers.CreateBuff("ArcanistArcaneWeaponEnchancementSwitchBuff",
                                                                 "Arcane Weapon",
                                                                 "As a standard action, the arcanist can expend 1 point from her arcane reservoir to enhance her weapon. The weapon gains a +1 enhancement bonus, which increases by 1 for every 4 levels beyond 5th (to a maximum of +4 at 17th level). These bonuses can be added to the weapon, stacking with existing weapon bonuses to a maximum of +5. An arcanist can also use this exploit to add one of the following weapon special abilities: flaming, frost, keen, shock and speed. Adding these special abilities replaces an amount of enhancement bonus equal to the ability’s cost. Duplicate special abilities do not stack. The benefits are decided upon when the exploit is used, and they cannot be changed unless the exploit is used again. These benefits only apply to weapons wielded by the arcanist; if another creature attempts to wield the weapon, it loses these benefits, though they resume if the arcanist regains possession of the weapon. The arcanist cannot have more than one use of this ability active at a time. This effect lasts for a number of minutes equal to the arcanist’s Charisma modifier (minimum 1).",
                                                                 "",
                                                                 arcane_weapon_magus.Icon,
                                                                 null,
                                                                 Helpers.CreateAddFactContextActions(activated: Common.createContextActionApplyBuff(enhancement_buff, Helpers.CreateContextDuration(),
                                                                                                                is_child: true, is_permanent: true, dispellable: false)
                                                                                                     )
                                                                 );
            //arcane_weapon_enhancement_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var flaming = Common.createEnchantmentAbility("ArcanistArcaneWeaponEnchancementFlaming",
                                                                "Arcane Weapon - Flaming",
                                                                "An arcanist can add the flaming property to her arcane weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA flaming weapon is sheathed in fire that deals an extra 1d6 points of fire damage on a successful hit. The fire does not harm the wielder.",
                                                                library.Get<BlueprintActivatableAbility>("05b7cbe45b1444a4f8bf4570fb2c0208").Icon,
                                                                arcane_weapon_enhancement_buff,
                                                                library.Get<BlueprintWeaponEnchantment>("30f90becaaac51f41bf56641966c4121"),
                                                                1, arcane_weapon_group);

            var frost = Common.createEnchantmentAbility("ArcanistArcaneWeaponEnchancementFrost",
                                                            "Arcane Weapon - Frost",
                                                            "An arcanist can add the frost property to her arcane weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA frost weapon is sheathed in a terrible, icy cold that deals an extra 1d6 points of cold damage on a successful hit. The cold does not harm the wielder.",
                                                            library.Get<BlueprintActivatableAbility>("b338e43a8f81a2f43a73a4ae676353a5").Icon,
                                                            arcane_weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("421e54078b7719d40915ce0672511d0b"),
                                                            1, arcane_weapon_group);

            var shock = Common.createEnchantmentAbility("ArcanistArcaneWeaponEnchancemenShock",
                                                            "Arcane Weapon - Shock",
                                                            "An arcanist can add the shock property to her arcane weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA shock weapon is sheathed in crackling electricity that deals an extra 1d6 points of electricity damage on a successful hit. The electricity does not harm the wielder.",
                                                            library.Get<BlueprintActivatableAbility>("a3a9e9a2f909cd74e9aee7788a7ec0c6").Icon,
                                                            arcane_weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("7bda5277d36ad114f9f9fd21d0dab658"),
                                                            1, arcane_weapon_group);

            var keen = Common.createEnchantmentAbility("ArcanistArcaneWeaponEnchancementKeen",
                                                            "Arcane Weapon - Keen",
                                                            "An arcanist can add the keen property to her arcane weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nThe keen property doubles the threat range of a weapon. This benefit doesn't stack with any other effects that expand the threat range of a weapon (such as the keen edge spell or the Improved Critical feat).",
                                                            library.Get<BlueprintActivatableAbility>("24fe1f546e07987418557837b0e0f8f5").Icon,
                                                            arcane_weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("102a9c8c9b7a75e4fb5844e79deaf4c0"),
                                                            1, arcane_weapon_group);

            var speed = Common.createEnchantmentAbility("ArcanistArcaneWeaponEnchancementSpeed",
                                                                    "Arcane Weapon - Speed",
                                                                    "An arcanist can add the speed property to her arcane weapon, but this consumes 3 points of enhancement bonus granted to this weapon.\nWhen making a full attack, the wielder of a speed weapon may make one extra attack with it. The attack uses the wielder's full base attack bonus, plus any modifiers appropriate to the situation. (This benefit is not cumulative with similar effects, such as a Haste spell.)",
                                                                    library.Get<BlueprintActivatableAbility>("85742dd6788c6914f96ddc4628b23932").Icon,
                                                                    arcane_weapon_enhancement_buff,
                                                                    library.Get<BlueprintWeaponEnchantment>("f1c0c50108025d546b2554674ea1c006"),
                                                                    3, arcane_weapon_group);

            var apply_buff = Common.createContextActionApplyBuff(arcane_weapon_enhancement_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), dispellable: false);
            var arcane_weapon_ability = Helpers.CreateAbility("ArcanistArcaneWeponAbility",
                                                                         arcane_weapon_enhancement_buff.Name,
                                                                         arcane_weapon_enhancement_buff.Description,
                                                                         "",
                                                                         arcane_weapon_enhancement_buff.Icon,
                                                                         AbilityType.Supernatural,
                                                                         CommandType.Standard,
                                                                         AbilityRange.Personal,
                                                                         "Charisma modifier minutes (minimum 1)",
                                                                         "",
                                                                         Helpers.CreateResourceLogic(arcane_reservoir_resource),
                                                                         Helpers.Create<NewMechanics.AbilityCasterPrimaryHandFree>(a => a.not = true),
                                                                         Helpers.CreateRunActions(apply_buff),
                                                                         arcane_weapon_magus.GetComponent<AbilitySpawnFx>(),
                                                                         Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Charisma, min: 1)
                                                                         );
            arcane_weapon_ability.setMiscAbilityParametersSelfOnly(Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.EnchantWeapon);
            arcane_weapon_ability.NeedEquipWeapons = true;


            arcane_weapon = Helpers.CreateFeature("ArcanistArcaneWeaponEnchancementFeature",
                                                  arcane_weapon_ability.Name,
                                                  arcane_weapon_ability.Description,
                                                  "",
                                                  arcane_weapon_ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFacts(arcane_weapon_ability, flaming, frost, shock, keen),
                                                  Helpers.PrerequisiteClassLevel(arcanist_class, 5, any: true),
                                                  Helpers.PrerequisiteClassLevel(library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e"), 5, any: true)
                                                  );

            var arcane_weapon2 = Helpers.CreateFeature("ArcanistArcaneWeaponEnchancement2Feature",
                                                        "Arcane Weapon +2",
                                                        arcane_weapon_ability.Description,
                                                        "",
                                                        arcane_weapon_ability.Icon,
                                                        FeatureGroup.None,
                                                        Common.createIncreaseActivatableAbilityGroupSize(arcane_weapon_group)
                                                        );
            var arcane_weapon3 = Helpers.CreateFeature("ArcanistArcaneWeaponEnchancement3Feature",
                                            "Arcane Weapon +3",
                                            arcane_weapon_ability.Description,
                                            "",
                                            arcane_weapon_ability.Icon,
                                            FeatureGroup.None,
                                            Helpers.CreateAddFacts(speed),
                                            Common.createIncreaseActivatableAbilityGroupSize(arcane_weapon_group)
                                            );

            var arcane_weapon4 = Helpers.CreateFeature("ArcanistArcaneWeaponEnchancement4Feature",
                                "Arcane Weapon +4",
                                arcane_weapon_ability.Description,
                                "",
                                arcane_weapon_ability.Icon,
                                FeatureGroup.None,
                                Common.createIncreaseActivatableAbilityGroupSize(arcane_weapon_group)
                                );

            arcane_weapon_ability.AddComponent(Helpers.CreateAddFeatureOnClassLevel(arcane_weapon2, 9, getExploitsUserArray()));
            arcane_weapon_ability.AddComponent(Helpers.CreateAddFeatureOnClassLevel(arcane_weapon3, 13, getExploitsUserArray()));
            arcane_weapon_ability.AddComponent(Helpers.CreateAddFeatureOnClassLevel(arcane_weapon4, 17, getExploitsUserArray()));
        }

        static void createArcaneBarrier()
        {
            var arcane_barrier_cost_buff = Helpers.CreateBuff("ArcaneBarrierCostBuff",
                                                               "ArcaneBarrierIncreaseCost",
                                                               "",
                                                               "",
                                                               null,
                                                               null
                                                               );
            arcane_barrier_cost_buff.SetBuffFlags(BuffFlags.RemoveOnRest | BuffFlags.HiddenInUi);
            arcane_barrier_cost_buff.Stacking = StackingType.Stack;
            var apply_cost_buff = Common.createContextActionApplyBuff(arcane_barrier_cost_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);
            var icon = library.Get<BlueprintAbility>("183d5bb91dea3a1489a6db6c9cb64445").Icon; //shield of faith
            var buff = Helpers.CreateBuff("ArcaneBarrierBuff",
                                          "Arcane Barrier",
                                          "As a swift action, the arcanist can expend 1 point from her arcane reservoir to create a barrier of magic that protects her from harm. This barrier grants the arcanist a number of temporary hit points equal to her arcanist level + her Charisma modifier, and lasts for 1 minute per arcanist level or until all the temporary hit points have been lost. Each additional time per day the arcanist uses this ability, the number of arcane reservoir points she must spend to activate it increases by 1 (so the second time it is used, the arcanist must expend 2 points from her arcane reservoir, 3 points for the third time, and so on). The temporary hit points from this ability do not stack with themselves, but additional uses do cause the total number of temporary hit points and the duration to reset.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.Create<TemporaryHitPointsFromAbilityValue>(t => { t.Value = Helpers.CreateContextValue(AbilityRankType.Default); t.RemoveWhenHitPointsEnd = true; }),
                                          Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueTypeExtender.ClassLevelPlusStatValue.ToContextRankBaseValueType(),
                                                                          classes: getExploitsUserArray(), stat: StatType.Charisma, min: 0)
                                          );
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), dispellable: false);

            var ability = Helpers.CreateAbility("ArcaneBarrierAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Swift,
                                                AbilityRange.Personal,
                                                Helpers.minutesPerLevelDuration,
                                                "",
                                                Helpers.CreateRunActions(apply_buff, apply_cost_buff),
                                                library.Get<BlueprintAbility>("183d5bb91dea3a1489a6db6c9cb64445").GetComponent<AbilitySpawnFx>(),
                                                Helpers.CreateResourceLogic(arcane_reservoir_resource, cost_is_custom: true),
                                                Helpers.Create<NewMechanics.ResourseCostCalculatorWithDecreasingFacts>(r => { r.cost_increasing_facts = new BlueprintFact[] { arcane_barrier_cost_buff }; }),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getExploitsUserArray())
                                                );
            ability.setMiscAbilityParametersSelfOnly();

            arcane_barrier = Common.AbilityToFeature(ability, false);
        }


        static void createQuickStudy()
        {
            var icon = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e").Icon;//detect magic

            List<BlueprintAbility> study_abilities = new List<BlueprintAbility>();

            for (int i = 1; i <= 9; i++)
            {
                var ability = Helpers.CreateAbility($"QuickStudy{i}Ability",
                                                    "Quick Study " + Common.roman_id[i],
                                                    "The arcanist can prepare a new all spells of the specified level in place of existing ones by expending 1 point from her arcane reservoir. Using this ability is a full-round action that provokes an attack of opportunity. The arcanist must be able to reference her spellbook when using this ability.",
                                                    "",
                                                    icon,
                                                    AbilityType.SpellLike,
                                                    CommandType.Standard,
                                                    AbilityRange.Personal,
                                                    "",
                                                    "",
                                                    Helpers.CreateRunActions(Helpers.Create<SpellManipulationMechanics.RefreshArcanistSpellLevel>(c => { c.spell_level = i; })),
                                                    Helpers.CreateResourceLogic(arcane_reservoir_resource),
                                                    Helpers.Create<NewMechanics.AbilityShowIfHasClassSpellLevel>(a => { a.character_class = arcanist_class; a.level = i; })
                                                    );
                Common.setAsFullRoundAction(ability);
                ability.setMiscAbilityParametersSelfOnly();
                study_abilities.Add(ability);
            }
            var quick_study_ability = Common.createVariantWrapper("QuickStudyAbility", "", study_abilities.ToArray());
            quick_study_ability.SetName("Quick Study");

            quick_study = Helpers.CreateFeature("QuickStudyFeature",
                                                quick_study_ability.Name,
                                                quick_study_ability.Description,
                                                "",
                                                icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddFact(quick_study_ability)
                                                );
        }


        static BlueprintSpellbook createArcanistSpellbook()
        {
            var wizard_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");

            memorization_spellbook = Helpers.Create<BlueprintSpellbook>();
            memorization_spellbook.Name = Helpers.CreateString("ArcanistMemorizationSpellbook.Name", "Arcanist (Prepared)");
            memorization_spellbook.name = "ArcanistMemorizationSpellbook";
            library.AddAsset(memorization_spellbook, "ab76417567444a6cb87d9d53e9752955");

            memorization_spellbook.Spontaneous = false;
            memorization_spellbook.IsArcane = true;
            memorization_spellbook.AllSpellsKnown = false;
            memorization_spellbook.CanCopyScrolls = true;
            memorization_spellbook.CastingAttribute = StatType.Intelligence;
            memorization_spellbook.CharacterClass = arcanist_class;
            memorization_spellbook.CasterLevelModifier = 0;
            memorization_spellbook.CantripsType = CantripsType.Cantrips;
            memorization_spellbook.SpellsPerLevel = 2;
            memorization_spellbook.SpellsPerDay =  Common.createSpellsTable("ArcanistSpellsKnown", "",
                                                                       Common.createSpellsLevelEntry(),  //0
                                                                       Common.createSpellsLevelEntry(0, 2),  //1
                                                                       Common.createSpellsLevelEntry(0, 2),  //2
                                                                       Common.createSpellsLevelEntry(0, 3),  //3
                                                                       Common.createSpellsLevelEntry(0, 3, 1), //4
                                                                       Common.createSpellsLevelEntry(0, 4, 2), //5
                                                                       Common.createSpellsLevelEntry(0, 4, 2, 1), //6
                                                                       Common.createSpellsLevelEntry(0, 5, 3, 2), //7
                                                                       Common.createSpellsLevelEntry(0, 5, 3, 2, 1), //8
                                                                       Common.createSpellsLevelEntry(0, 5, 4, 3, 2), //9
                                                                       Common.createSpellsLevelEntry(0, 5, 4, 3, 2, 1), //10
                                                                       Common.createSpellsLevelEntry(0, 5, 5, 4, 3, 2), //11
                                                                       Common.createSpellsLevelEntry(0, 5, 5, 4, 3, 2, 1), //12
                                                                       Common.createSpellsLevelEntry(0, 5, 5, 4, 4, 3, 2), //13
                                                                       Common.createSpellsLevelEntry(0, 5, 5, 4, 4, 3, 2, 1), //14
                                                                       Common.createSpellsLevelEntry(0, 5, 5, 4, 4, 4, 3, 2), //15
                                                                       Common.createSpellsLevelEntry(0, 5, 5, 4, 4, 4, 3, 2, 1), //16
                                                                       Common.createSpellsLevelEntry(0, 5, 5, 4, 4, 4, 3, 3, 2), //17
                                                                       Common.createSpellsLevelEntry(0, 5, 5, 4, 4, 4, 3, 3, 2, 1), //18
                                                                       Common.createSpellsLevelEntry(0, 5, 5, 4, 4, 4, 3, 3, 2, 2), //19
                                                                       Common.createSpellsLevelEntry(0, 5, 5, 4, 4, 4, 3, 3, 3, 3) //20
                                                                       );

            memorization_spellbook.SpellList = wizard_class.Spellbook.SpellList;
            memorization_spellbook.AddComponent(Helpers.Create<SpellbookMechanics.NoSpellsPerDaySacaling>());
            memorization_spellbook.AddComponent(Helpers.Create<SpellbookMechanics.CanNotUseSpells>());

            arcanist_spellbook = Helpers.Create<BlueprintSpellbook>();
            arcanist_spellbook.Name = Helpers.CreateString("ArcanistSpellbook.Name", "Arcanist (Spont.)");
            arcanist_spellbook.name = "ArcanistSpellbook";
            library.AddAsset(arcanist_spellbook, "0c21cfcab6ce4395bd4df330ab3cf715");

            arcanist_spellbook.Spontaneous = true;
            arcanist_spellbook.IsArcane = true;
            arcanist_spellbook.AllSpellsKnown = false;
            arcanist_spellbook.CanCopyScrolls = false;
            arcanist_spellbook.CastingAttribute = StatType.Intelligence;
            arcanist_spellbook.CharacterClass = arcanist_class;
            arcanist_spellbook.CasterLevelModifier = 0;
            arcanist_spellbook.CantripsType = CantripsType.Cantrips;
            arcanist_spellbook.SpellsPerLevel = 0;
            arcanist_spellbook.SpellsKnown = Common.createEmptySpellTable("ArcanistKnownPerDaySpellTable", "");
            arcanist_spellbook.SpellsPerDay = Common.createSpellsTable("ArcanistSpellsPerDay", "",
                                                                       Common.createSpellsLevelEntry(),  //0
                                                                       Common.createSpellsLevelEntry(0, 2),  //1
                                                                       Common.createSpellsLevelEntry(0, 3),  //2
                                                                       Common.createSpellsLevelEntry(0, 4),  //3
                                                                       Common.createSpellsLevelEntry(0, 4, 2), //4
                                                                       Common.createSpellsLevelEntry(0, 4, 3), //5
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 2), //6
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 3), //7
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 2), //8
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 3), //9
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 4, 2), //10
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 4, 3), //11
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 4, 4, 2), //12
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 4, 4, 3), //13
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 4, 4, 4, 2), //14
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 4, 4, 4, 3), //15
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 4, 4, 4, 4, 2), //16
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 4, 4, 4, 4, 3), //17
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 4, 4, 4, 4, 4, 2), //18
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 4, 4, 4, 4, 4, 3), //19
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 4, 4, 4, 4, 4, 4) //20
                                                                       );

            arcanist_spellbook.SpellList = wizard_class.Spellbook.SpellList;
            memorization_spellbook.AddComponent(Helpers.Create<SpellbookMechanics.CompanionSpellbook>(c => c.spellbook = arcanist_spellbook));
            arcanist_spellbook.AddComponent(Helpers.Create<SpellbookMechanics.GetKnownSpellsFromMemorizationSpellbook>(c => c.spellbook = memorization_spellbook));

            return memorization_spellbook;
        }
    }
}
