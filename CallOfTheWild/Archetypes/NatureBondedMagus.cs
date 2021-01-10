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
    public class NatureBondedMagus
    {
        static public BlueprintArchetype archetype;
        static public BlueprintFeatureSelection[] natural_magic = new BlueprintFeatureSelection[6];
        static public BlueprintFeatureSelection familiar;
        static public BlueprintFeature symbiosis;
        static public BlueprintFeature improved_symbiosis;

        static LibraryScriptableObject library => Main.library;

        static public void create()
        {
            var magus_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("45a4607686d96a1498891b3286121780");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "NatureBondedMagusArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Nature-Bonded Magus");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "A nature-bonded magus synergizes arcane magic and the divine magic traditions of druids into a deadly synthesis.");
            });
            Helpers.SetField(archetype, "m_ParentClass", magus_class);
            library.AddAsset(archetype, "");

            createFamiliar();
            createNaturalMagic();
            createSymbiosis();
            createImprovedSymbiosis();

            var arcane_pool_feature = library.Get<BlueprintFeature>("3ce9bb90749c21249adc639031d5eed1");
            var magus_arcana = library.Get<BlueprintFeatureSelection>("e9dc4dfc73eaaf94aae27e0ed6cc9ada");
            var spell_recall = library.Get<BlueprintFeature>("61fc0521e9992624e9c518060bf89c0f");
            var improved_spell_recall = library.Get<BlueprintFeature>("0ef6ec1c2fdfc204fbd3bff9f1609490");

            var arcane_weapon5 = library.Get<BlueprintFeature>("36b609a6946733c42930c55ac540416b");
            var arcane_weapon9 = library.Get<BlueprintFeature>("70be888059f99a245a79d6d61b90edc5");
            var arcane_weapon13 = library.Get<BlueprintFeature>("1804187264121cd439d70a96234d4ddb");
            var arcane_weapon17 = library.Get<BlueprintFeature>("3cbe3e308342b3247ba2f4fbaf5e6307");

            var woodland_stride = library.CopyAndAdd<BlueprintFeature>("11f4072ea766a5840a46e6660894527d",
                                                            "NatureBondedMagusWooldlandStride",
                                                            "");
            woodland_stride.SetDescription("At 7th level, a nature-bonded magus can move through any sort of undergrowth (such as natural briars, overgrown areas, thorns, and similar terrain) at his normal speed and without taking damage or suffering any other impairment.");

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, arcane_pool_feature),
                                                          Helpers.LevelEntry(4, spell_recall),
                                                          Helpers.LevelEntry(5, arcane_weapon5),
                                                          Helpers.LevelEntry(9, arcane_weapon9),
                                                          Helpers.LevelEntry(11, improved_spell_recall),
                                                          Helpers.LevelEntry(13, arcane_weapon13),
                                                          Helpers.LevelEntry(17, arcane_weapon17),
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, familiar, natural_magic[0]),
                                                          Helpers.LevelEntry(4, symbiosis, natural_magic[1]),
                                                          Helpers.LevelEntry(7, symbiosis, woodland_stride, natural_magic[2]),
                                                          Helpers.LevelEntry(10, natural_magic[3]),
                                                          Helpers.LevelEntry(11, symbiosis, improved_symbiosis),
                                                          Helpers.LevelEntry(13, natural_magic[4]),
                                                          Helpers.LevelEntry(15, symbiosis),
                                                          Helpers.LevelEntry(16, natural_magic[5]),
                                                          Helpers.LevelEntry(19, symbiosis),
                                                       };
            magus_class.Archetypes = magus_class.Archetypes.AddToArray(archetype);

            magus_class.Progression.UIGroups = magus_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(natural_magic));
            magus_class.Progression.UIGroups = magus_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(symbiosis));
            magus_class.Progression.UIGroups = magus_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(familiar, woodland_stride, improved_symbiosis));


            var restricted_arcanas_ids = new string[]
            {
                "2eacbdbf1c4f4134aa7fea99ab8763dc",
                "a2e0691dcfda2374e84d8bbf480e06a0",
                "4be0bb10e110a35419e406da767bd1e3",
                "cb6916027e3c25e4185de068249254dc",
                "7a73bf165e8eda6478b4419f857d1ab5",
                "8896f327c59569c4eaf129bf35b96c1f",
                "85c05a8120e3e9f4e8ae01625038809a",
                "a3909a7293533fe49a2d7cfe051f17e4",
                "42f96fc8d6c80784194262e51b0a1d25", //extra arcane pool
            };

            foreach (var id in restricted_arcanas_ids)
            {
                var feature = library.Get<BlueprintFeature>(id);
                feature.AddComponent(Common.prerequisiteNoArchetype(magus_class, archetype));
            }
        }


        static void createFamiliar()
        {
            familiar = Helpers.CreateFeatureSelection("FamiliarNatureBondedMagusFeature",
                                                       "Familiar",
                                                       "A nature-bonded magus gains the familiar magus arcana.",
                                                       "",
                                                       null,
                                                       FeatureGroup.None
                                                       );
            familiar.AllFeatures = new BlueprintFeature[] { MagusArcana.familiar };
        }


        static void createSymbiosis()
        {
            symbiosis = Helpers.CreateFeature("SymbiosisNatureBondedMagusFeature",
                                              "Symbiosis",
                                                "A 4th level, a nature-bonded magus gains +1 bonus to its natural AC. This bonus increases by 1 at 7th level and every 4 levels thereafter.",
                                                "",
                                                Helpers.GetIcon("5b77d7cc65b8ab74688e74a37fc2f553"), //barkskin
                                                FeatureGroup.None,
                                                Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.NaturalArmor)
                                                );
            symbiosis.Ranks = 5;
            symbiosis.ReapplyOnLevelUp = true;
            symbiosis.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, feature: symbiosis));
        }


        static void createImprovedSymbiosis()
        {
            improved_symbiosis = Helpers.CreateFeature("ImprovedSymbiosisNatureBondedMagusFeature",
                                                    "Improved Symbiosis",
                                                    "At 11th level, a nature-bonded magus gains a +4 enhancement bonus to his Strength and Constitution. This bonus increases to +6 at level 15 and to +8 at level 19.",
                                                    "",
                                                    Helpers.GetIcon("4c3d08935262b6544ae97599b3a9556d"), //bulls strength
                                                    FeatureGroup.None,
                                                    Helpers.CreateAddContextStatBonus(StatType.Strength, ModifierDescriptor.Enhancement),
                                                    Helpers.CreateAddContextStatBonus(StatType.Constitution, ModifierDescriptor.Enhancement),
                                                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { archetype.GetParentClass() },
                                                                                    progression: ContextRankProgression.Custom,
                                                                                    customProgression: new (int, int)[] { (14, 4), (18, 6), (20, 8) }
                                                                                    )
                                                    );
            improved_symbiosis.ReapplyOnLevelUp = true;
        }


        static void createNaturalMagic()
        {
            var magus_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("45a4607686d96a1498891b3286121780");
            var drudi_spell_list = library.Get<BlueprintSpellList>("bad8638d40639d04fa2f80a1cac67d6b");
            var magus_spell_list = library.Get<BlueprintSpellList>("4d72e1e7bd6bc4f4caaea7aa43a14639");
            var combined_spell_list = Common.combineSpellLists("NatureBondedMagusNaturalMagicSpellList", drudi_spell_list);
            Common.excludeSpellsFromList(combined_spell_list, magus_spell_list);

            for (int i = 1; i <= 6; i++)
            {
                natural_magic[i-1] = Helpers.CreateFeatureSelection($"NatureBondedMagusNaturalMagic{i}FeatureSelection",
                                                "Natural Magic " + $"(Level {i})",
                                                "A nature - bonded magus adds one 1st - level spell from the druid spell list to his spellbook. Each time a nature - bonded magus gains the ability to cast a new level of spells, he can add one spell of that level from the druid spell list to his spellbook.",
                                                "",
                                                Helpers.GetIcon("0fd00984a2c0e0a429cf1a911b4ec5ca"), //entangle
                                                FeatureGroup.None);
                var learn_spell = library.CopyAndAdd<BlueprintParametrizedFeature>("bcd757ac2aeef3c49b77e5af4e510956", $"NatureBondedMagusNaturalMagic{i}ParametrizedFeature", "");
                learn_spell.SpellLevel = i;
                learn_spell.SpecificSpellLevel = true;
                learn_spell.SpellLevelPenalty = 0;
                learn_spell.SpellcasterClass = magus_class;
                learn_spell.SpellList = combined_spell_list;
                learn_spell.ReplaceComponent<LearnSpellParametrized>(l => { l.SpellList = combined_spell_list; l.SpecificSpellLevel = true; l.SpellLevel = i; l.SpellcasterClass = magus_class; });
                learn_spell.SetName(Helpers.CreateString($"NatureBondedMagusNaturalMagic{i}ParametrizedFeature.Name", "Natural Magic " + $"(Level {i})"));
                learn_spell.SetDescription(natural_magic[i - 1].Description);
                learn_spell.SetIcon(natural_magic[i - 1].Icon);
                natural_magic[i - 1].AllFeatures = new BlueprintFeature[] { learn_spell };
            }
        }
    }
}
