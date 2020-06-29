using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
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
        static void createTwinnedEidolon()
        {
            var fx_feature = Helpers.CreateFeature("TwinnedEidolonFxFeature",
                                       "",
                                       "",
                                       "",
                                       null,
                                       FeatureGroup.None,
                                       Helpers.Create<UnitViewMechanics.ReplaceUnitView>(r =>  r.use_master_view = true));



            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");

            var twinned_unit = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "TwinnedEidolonUnit", "");
            twinned_unit.LocalizedName = twinned_unit.LocalizedName.CreateCopy();
            twinned_unit.LocalizedName.String = Helpers.CreateString(twinned_unit.name + ".Name", "Twinned Eidolon");

            twinned_unit.Alignment = Alignment.TrueNeutral;
            twinned_unit.Strength = 16;
            twinned_unit.Dexterity = 12;
            twinned_unit.Constitution = 13;
            twinned_unit.Intelligence = 7;
            twinned_unit.Wisdom = 10;
            twinned_unit.Charisma = 11;
            twinned_unit.Speed = 30.Feet();
            twinned_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2, fx_feature }; // { natural_armor2, fx_feature };
            twinned_unit.Body = twinned_unit.Body.CloneObject();
            twinned_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            twinned_unit.Body.PrimaryHand = null;
            twinned_unit.Body.SecondaryHand = null;
            twinned_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            twinned_unit.Body.AdditionalSecondaryLimbs = new BlueprintItemWeapon[0];
            twinned_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[] {Eidolon.twinned_archetype };
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillAthletics, StatType.SkillMobility, StatType.SkillStealth };
                a.Selections = new SelectionEntry[0];
            });
            twinned_unit.AddComponents(Helpers.Create<EidolonComponent>());

            var twinned_unit_small = library.CopyAndAdd<BlueprintUnit>(twinned_unit, "TwinnedEidolonUnitSmall", "");
            twinned_unit_small.Size = Size.Small;
            twinned_unit_small.Speed = 20.Feet();

            twinned_eidolon = Helpers.CreateProgression("TwinnedEidolonProgression",
                                                        "Twinned Eidolon",
                                                        "Just like a summoner, a twinned eidolon learns to use weapons and magic. While twinned eidolons are ethically more flexible than most eidolons, they are quite adamant about being treated as equals and not as servants. They demonstrate an eerie consistency with the summoner’s manner of thinking, providing similar answers to questions and reacting similarly to startling events. This subtype is restricted to twinned summoners.",
                                                        "",
                                                        Helpers.GetIcon("3e4ab69ada402d145a5e0ad3ad4b8564"), //mirror image
                                                        FeatureGroup.AnimalCompanion,
                                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                                        );
            twinned_eidolon.IsClassFeature = true;
            twinned_eidolon.ReapplyOnLevelUp = true;
            twinned_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            twinned_eidolon.ReplaceComponent<AddPet>(a => a.Pet = twinned_unit);
            

            twinned_eidolon_small = library.CopyAndAdd(twinned_eidolon, "TwinnedEidolonSmallProgression", "");
            twinned_eidolon_small.ReplaceComponent<AddPet>(a => a.Pet = twinned_unit_small);
            twinned_eidolon.AddComponent(Helpers.Create<SizeMechanics.PrerequisiteCharacterSize>(p => { p.or_larger = true; p.value = Size.Medium; }));
            twinned_eidolon_small.AddComponent(Helpers.Create<SizeMechanics.PrerequisiteCharacterSize>(p => { p.or_smaller = true; p.value = Size.Small; }));
        }


        static void fillTwinnedProgression()
        {
            var feature1 = Helpers.CreateFeature("TwinnedEidolonLevel1Feature",
                                                  "Base Evolutions",
                                                  "A twinned eidolon gains the weapon training evolution. The twinned eidolon also gains the skilled (Stealth) evolution.",
                                                  "",
                                                  twinned_eidolon.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.weapon_training[0]),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.skilled[8])
                                                  );

            var resource = Helpers.CreateAbilityResource("TwinnedEidolonSpellLikeResource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 9, 1, 5, 1, 0, 0.0f, new BlueprintCharacterClass[] { Eidolon.eidolon_class });
            var feature4 = createTwinnedSpellLikeAbility("Spell-Like Ability",
                                                          "At 4th level, a twinned eidolon can cast a summoner spell once per day as a spell-like ability. The spell must be one known by the eidolon’s summoner and must be at least 1 level lower than the highest-level spell the summoner can cast. The eidolon must have a Charisma score of at least 10 + the spell level. The caster level for this spell-like ability is equal to the eidolon’s Hit Dice. The save DC is 10 + half the eidolon’s HD + the eidolon’s Charisma modifier.\n"
                                                          + "Eidolon can use this ability one more time per day when summoner reaches levels 11 and 18.",
                                                          Helpers.GetIcon("55edf82380a1c8540af6c6037d34f322"), //elven magic
                                                          1,
                                                          resource,
                                                          false
                                                        );
            var feature4_7 = createTwinnedSpellLikeAbility(feature4.Name,
                                                           feature4.Description,
                                                           feature4.Icon,
                                                           2,
                                                           resource);
            var feature4_10 = createTwinnedSpellLikeAbility(feature4.Name,
                                                           feature4.Description,
                                                           feature4.Icon,
                                                           3,
                                                           resource);
            var feature4_13 = createTwinnedSpellLikeAbility(feature4.Name,
                                                           feature4.Description,
                                                           feature4.Icon,
                                                           4,
                                                           resource);
            var feature4_16 = createTwinnedSpellLikeAbility(feature4.Name,
                                                           feature4.Description,
                                                           feature4.Icon,
                                                           5,
                                                           resource);

            var feature8 = Helpers.CreateFeatureSelection("TwinnedEidolonLevel8FeatureSelection",
                                                  "Shared Slot",
                                                  "At 8th level, a twinned eidolon gains the shared slot evolution.",
                                                  "",
                                                  null,
                                                  FeatureGroup.None
                                                  );
            feature8.AllFeatures = Evolutions.getPermanenetEvolutions(e => Evolutions.shared_slots.Any(ai => ai == e));

            var feature12 = Helpers.CreateFeature("TwinnedEidolonLevel12Feature",
                                                  "Damage Reduction",
                                                  "At 12th level, a twinned eidolon gains DR 5/magic. In addition the eidolon also gains the extra feat when it reaches lvl 10.",
                                                  "",
                                                  Helpers.GetIcon("9e1ad5d6f87d19e4d8883d63a6e35568"), //mage armor
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("TwinnedEidolonLevel12AddFeature",
                                                                                Common.createMagicDR(Helpers.CreateContextValue(AbilityRankType.Default)),
                                                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureList,
                                                                                                                progression: ContextRankProgression.MultiplyByModifier,
                                                                                                                stepLevel: 10,
                                                                                                                min: 5,
                                                                                                                featureList: new BlueprintFeature[] { Evolutions.damage_reduction }
                                                                                                                ),
                                                                                Helpers.Create<RecalculateOnFactsChange>(r => r.CheckedFacts = new BlueprintUnitFact[] { Evolutions.damage_reduction }))
                                                                                //extra feat to eidolon
                                                  );


            var feature16_1 = Helpers.CreateFeatureSelection("TwinnedEidolonLevel161Feature",
                                                          "Ability Increase Evolution",
                                                          "At 16th level, a twinned eidolon gains the skilled evolution and the ability increase evolution, applied to a skill and an ability score of the summoner’s choice.",
                                                          "",
                                                          null,
                                                          FeatureGroup.None
                                                          );
            feature16_1.AllFeatures = Evolutions.getPermanenetEvolutions(e => Evolutions.ability_increase.Any(ai => ai[0] == e));

            var feature16_2 = Helpers.CreateFeatureSelection("TwinnedEidolonLevel162Feature",
                                              "Skilled Evolution",
                                              "At 16th level, a twinned eidolon gains the skilled evolution and the ability increase evolution, applied to a skill and an ability score of the summoner’s choice.",
                                              "",
                                              null,
                                              FeatureGroup.None
                                              );
            feature16_2.AllFeatures = Evolutions.getPermanenetEvolutions(e => Evolutions.skilled.Any(ai => ai == e));

            var feature20 = Helpers.CreateFeature("TwinnedEidolonLevel20Feature",
                                                  "Fast Healing",
                                                  "At 20th level, a twinned eidolon gains fast healing V evolution.",
                                                  "",
                                                  Helpers.GetIcon("4093d5a0eb5cae94e909eb1e0e1a6b36"),
                                                  FeatureGroup.None,
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.fast_healing[0]),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.fast_healing[1]),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.fast_healing[2]),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.fast_healing[3]),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.fast_healing[4])
                                                  );

            twinned_eidolon.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, feature1),
                                                           Helpers.LevelEntry(4, feature4),
                                                           Helpers.LevelEntry(7, feature4_7),
                                                           Helpers.LevelEntry(8, feature8),
                                                           Helpers.LevelEntry(10, feature4_10),
                                                           Helpers.LevelEntry(12, feature12),
                                                           Helpers.LevelEntry(13, feature4_13),
                                                           Helpers.LevelEntry(16, feature16_1, feature16_2, feature4_16),
                                                           Helpers.LevelEntry(20, feature20)
                                                           };
            twinned_eidolon.UIGroups = Helpers.CreateUIGroups(feature1, feature4, feature8, feature12, feature16_1, feature20);

            twinned_eidolon_small.LevelEntries = twinned_eidolon.LevelEntries;
            twinned_eidolon_small.UIGroups = twinned_eidolon.UIGroups;
        }


        static BlueprintFeature createTwinnedSpellLikeAbility(string display_name, string description, UnityEngine.Sprite icon,
                                                              int spell_level, BlueprintAbilityResource resource, bool hide = true)
        {
            var spells = Summoner.summoner_class.Spellbook.SpellList.SpellsByLevel[spell_level].Spells;

            var abilities = new List<BlueprintAbility>();

            foreach (var s in spells)
            {
                if (!s.HasVariants)
                {
                    var spell_like = Common.convertToSpellLike(s, "TwinnedEidolon", new BlueprintCharacterClass[] { eidolon_class }, StatType.Charisma, resource, no_scaling: true,
                                                               guid: Helpers.MergeIds("cff70e146d58443e82072d759e61e7ef", s.AssetGuid));
                    spell_like.AddComponent(Helpers.Create<CompanionMechanics.AbilityShowIfMasterKnowsSpellAndPetHAsSufficientStat>(a =>
                                                                                                                                    {
                                                                                                                                        a.master_class = Summoner.summoner_class;
                                                                                                                                        a.stat = StatType.Charisma;
                                                                                                                                        a.min_stat = 10 + spell_level;
                                                                                                                                        a.spell = s;
                                                                                                                                    }));
                    abilities.Add(spell_like);
                }
                else
                {
                    List<BlueprintAbility> spell_likes = new List<BlueprintAbility>();
                    foreach (var v in s.Variants)
                    {
                        var spell_like = Common.convertToSpellLike(v, "TwinnedEidolon", new BlueprintCharacterClass[] { eidolon_class }, StatType.Charisma, resource, no_scaling: true,
                                           guid: Helpers.MergeIds("cff70e146d58443e82072d759e61e7ef", v.AssetGuid));
                        spell_like.AddComponent(Helpers.Create<CompanionMechanics.AbilityShowIfMasterKnowsSpellAndPetHAsSufficientStat>(a =>
                                                                                                                                        {
                                                                                                                                            a.master_class = Summoner.summoner_class;
                                                                                                                                            a.stat = StatType.Charisma;
                                                                                                                                            a.min_stat = 10 + spell_level;
                                                                                                                                            a.spell = s;
                                                                                                                                        }));
                        abilities.Add(spell_like);
                    }
                }
            }

            var wrapper = Common.createVariantWrapper($"TwinnedSpellLike{spell_level}Ability", "", abilities.ToArray());
            wrapper.SetNameDescriptionIcon(display_name + " " + Common.roman_id[spell_level], description, icon);
            var feature = Common.AbilityToFeature(wrapper, hide);
            feature.SetName(display_name);

            if (spell_level == 1)
            {
                feature.AddComponent(resource.CreateAddAbilityResource());
            }
            var ac_feature =  Common.createAddFeatToAnimalCompanion(feature, "");
            if (hide)
            {
                ac_feature.HideInUI = true;
                ac_feature.HideInCharacterSheetAndLevelUp = true;
            }
            return ac_feature;
        }
    }
}