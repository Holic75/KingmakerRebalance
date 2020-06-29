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
        static void createFeyUnit()
        {
            var fx_feature = Helpers.CreateFeature("FeyEidolonFxFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Helpers.Create<UnitViewMechanics.ReplaceUnitView>(r => r.prefab = Common.createUnitViewLink("7cc1c50366f08814eb5a5e7c47c71a2a")));
            fx_feature.HideInCharacterSheetAndLevelUp = true;
            fx_feature.HideInUI = true;

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var dryad_unit = library.Get<BlueprintUnit>("20660a3d7ef5ec54a9c1f08b0b58d753");
            var fey_unit = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "FeyEidolonUnit", "");
            fey_unit.Color = dryad_unit.Color;

            fey_unit.Visual = dryad_unit.Visual;
            fey_unit.LocalizedName = dryad_unit.LocalizedName.CreateCopy();
            fey_unit.LocalizedName.String = Helpers.CreateString(fey_unit.name + ".Name", "Fey Eidolon");

            fey_unit.Strength = 16;
            fey_unit.Dexterity = 12;
            fey_unit.Constitution = 13;
            fey_unit.Intelligence = 7;
            fey_unit.Wisdom = 10;
            fey_unit.Charisma = 11;
            fey_unit.Speed = 30.Feet();
            fey_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2, fx_feature }; // { natural_armor2, fx_feature };
            fey_unit.Body = fey_unit.Body.CloneObject();
            fey_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            fey_unit.Body.PrimaryHand = null;
            fey_unit.Body.SecondaryHand = null;
            fey_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            fey_unit.Body.AdditionalSecondaryLimbs = new BlueprintItemWeapon[0];
            fey_unit.Gender = Gender.Female;
            fey_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[] { fey_archetype };
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillAthletics, StatType.SkillMobility, StatType.SkillStealth };
                a.Selections = new SelectionEntry[0];
            });
            fey_unit.AddComponents(Helpers.Create<EidolonComponent>());


            Helpers.SetField(fey_unit, "m_Portrait", Helpers.createPortrait("EidolonFeyProtrait", "Fey", ""));

            fey_eidolon = Helpers.CreateProgression("FeyEidolonProgression",
                                                    "Fey Eidolon",
                                                    "Fey eidolons usually choose to bond with mortals for their own mysterious reasons that vary as much as their disparate temperaments; occasionally, their need may be immediate, such as when a dryad whose tree is dying decides to bond with a summoner instead and become something new. On the other hand, a redcap just looking for bloodshed might connect with an equally sadistic summoner. Whatever their reasons, they tend to have strong bonds of loyalty to their summoners entangled with equally strong emotional attachments, even evil fey eidolons.",
                                                    "",
                                                    Helpers.GetIcon("e8445256abbdc45488c2d90373f7dae8"),
                                                    FeatureGroup.AnimalCompanion,
                                                    library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                                    );
            fey_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            fey_eidolon.ReplaceComponent<AddPet>(a => a.Pet = fey_unit);
            fey_eidolon.IsClassFeature = true;
            fey_eidolon.ReapplyOnLevelUp = true;
            //Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(fey_eidolon);
        }


        static void fillFeyProgression()
        {
            var feature1 = Helpers.CreateFeatureSelection("FeyEidolonLevel1Feature",
                                                  "Base Evolutions",
                                                  "Starting at 1st level, fey eidolons gain Mobility, Persuasion, Lore (nature), Trickery, and Use Magic Device as class skills instead of those gained by most eidolons. They also gain the slam evolution and the skilled evolution (selecting one class skill).",
                                                  "",
                                                  fey_eidolon.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.slam_biped)
                                                  );
            feature1.AllFeatures = Evolutions.getPermanenetEvolutions(e => Evolutions.skilled.Contains(e));

            var woodland_stride = library.Get<BlueprintFeature>("11f4072ea766a5840a46e6660894527d");
            transferable_abilities.Add(woodland_stride);
            var feature4 = Helpers.CreateFeature("FeyEidolonLevel4Feature",
                                                  "Woodland Stride",
                                                  "At 4th level, fey eidolons gain woodland stride.",
                                                  "",
                                                  woodland_stride.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.Create<AddFeatureToCompanion>(a => a.Feature = woodland_stride)
                                                  );

            var fey_magic1 = createFeySpellLikeAbilitiesSpellList(3, library.Get<BlueprintAbility>("0fd00984a2c0e0a429cf1a911b4ec5ca"), //entangle  
                                                                  library.Get<BlueprintAbility>("95851f6e85fe87d4190675db0419d112"), //grease
                                                                  library.Get<BlueprintAbility>("f001c73999fb5a543a199f890108d936"),
                                                                  NewSpells.obscuring_mist);
            var feature8 = Helpers.CreateFeatureSelection("FeyEidolonLevel8Feature",
                                                          "Fey Magic I",
                                                          "At 8th level, fey eidolons gain the ability to use either entangle, grease, obscuring mist, or vanish as a spell-like ability three times per day. At 16th level Fey Eidolon can select another spell-like ability.",
                                                          "",
                                                          Helpers.GetIcon("0fd00984a2c0e0a429cf1a911b4ec5ca"), //entangle
                                                          FeatureGroup.None
                                                          );
            feature8.AllFeatures = fey_magic1;
            var feature82 = library.CopyAndAdd<BlueprintFeatureSelection>(feature8, "FeyEidolon2Level8Feature", "");


            var fey_dr = Helpers.CreateFeature("FeyEidolonDRFeature",
                                               "Resistance and Wings",
                                               "At 12th level, fey eidolons gain DR 5/ cold iron. They also grow wings, gaining the flight evolution.",
                                               "",
                                               Helpers.GetIcon("9e1ad5d6f87d19e4d8883d63a6e35568"), //mage armor
                                               FeatureGroup.None,
                                               Common.createMaterialDR(Helpers.CreateContextValue(AbilityRankType.Default), PhysicalDamageMaterial.ColdIron)
                                               );
            var fey_extra_dr = Helpers.CreateFeature("FeyEidolonExtraDrFeature", "", "", "", null, FeatureGroup.None);
            fey_extra_dr.HideInCharacterSheetAndLevelUp = true;
            fey_dr.AddComponents(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureList,
                                                                progression: ContextRankProgression.MultiplyByModifier,
                                                                stepLevel: 5,
                                                                min: 5,
                                                                featureList: new BlueprintFeature[] { Evolutions.damage_reduction, fey_dr, fey_extra_dr }
                                                                ),
                                 Helpers.Create<RecalculateOnFactsChange>(r => r.CheckedFacts = new BlueprintUnitFact[] { Evolutions.damage_reduction, fey_extra_dr })
                                 );
            transferable_abilities.Add(fey_dr);
            var feature12 = Helpers.CreateFeature("FeyEidolonLevel12Feature",
                                                  fey_dr.Name,
                                                  fey_dr.Description,
                                                  "",
                                                  fey_dr.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.Create<AddFeatureToCompanion>(a => a.Feature = fey_dr),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.flight)
                                                  );

            var fey_magic2 = createFeySpellLikeAbilitiesSpellList(3, library.Get<BlueprintAbility>("46fd02ad56c35224c9c91c88cd457791"), //blindness
                                          library.Get<BlueprintAbility>("ce7dad2b25acf85429b6c9550787b2d9"), //glitterdust
                                          library.Get<BlueprintAbility>("fd4d9fd7f87575d47aafe2a64a6e2d8d"), //hideous laughter
                                          library.Get<BlueprintAbility>("89940cde01689fb46946b2f8cd7b66b7"), //invisibility
                                          library.Get<BlueprintAbility>("3e4ab69ada402d145a5e0ad3ad4b8564") //mirror image
                                          );
            var feature16 = Helpers.CreateFeatureSelection("FeyEidolonLevel16Feature",
                                                          "Fey Magic II",
                                                          "At 16th level, fey eidolons gain the ability to use either blindness, glitterdust, hideous laughter, invisibility, or mirror image as a spell-like ability three times per day.",
                                                          "",
                                                          Helpers.GetIcon("89940cde01689fb46946b2f8cd7b66b7"), //invisibility
                                                          FeatureGroup.None
                                                          );
            feature16.AllFeatures = fey_magic2;

            var fey_magic3 = createFeySpellLikeAbilitiesSpellList(1, library.Get<BlueprintAbility>("7f71a70d822af94458dc1a235507e972"), //claock of dreams
                                                      library.Get<BlueprintAbility>("1e2d1489781b10a45a3b70192bba9be3"), //waves of ectasy
                                                      library.Get<BlueprintAbility>("ecaa0def35b38f949bd1976a6c9539e0"), //invisibility greater
                                                      library.Get<BlueprintAbility>("98310a099009bbd4dbdf66bcef58b4cd") //invisibility mass
                                                      );
            transferable_abilities.Add(fey_extra_dr);
            var feature20 = Helpers.CreateFeatureSelection("FeyEidolonLevel20Feature",
                                                          "Fey Magic III",
                                                          "At 20th level, fey eidolons increase their DR to DR 10/cold iron. They gain the ability to use either waves of ectasy, invisibility, mass, invisibility greater, or cloak of dreams as a spell-like ability once per day.",
                                                          "",
                                                          Helpers.GetIcon("7f71a70d822af94458dc1a235507e972"), //cloak of dreams
                                                          FeatureGroup.None,
                                                          Helpers.Create<AddFeatureToCompanion>(a => a.Feature = fey_extra_dr)
                                                          );
            feature20.AllFeatures = fey_magic3;

            var feature220 = Helpers.CreateFeatureSelection("FeyEidolon2Level20Feature",
                                                          "Fey Magic I or II",
                                                          "At 20th level, fey eidolons can select another spell-like ability from 8th level list or 16th level list.",
                                                          "",
                                                          Helpers.GetIcon("ce7dad2b25acf85429b6c9550787b2d9"), //glitterdust
                                                          FeatureGroup.None
                                                          );
            feature220.AllFeatures = fey_magic1.AddToArray(fey_magic2);

            fey_eidolon.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, feature1),
                                                           Helpers.LevelEntry(4, feature4),
                                                           Helpers.LevelEntry(8, feature8),
                                                           Helpers.LevelEntry(12, feature12),
                                                           Helpers.LevelEntry(16, feature16, feature82),
                                                           Helpers.LevelEntry(20, feature20, feature220)
                                                           };
            fey_eidolon.UIGroups = new UIGroup[]{ Helpers.CreateUIGroup(feature1, feature4, feature8, feature12, feature16, feature20),
                                                  Helpers.CreateUIGroup(feature82, feature220)
                                                }; 
        }


        static BlueprintFeature[] createFeySpellLikeAbilitiesSpellList(int uses, params BlueprintAbility[] spells)
        {
            var features = new List<BlueprintFeature>();

            foreach (var s in spells)
            {
                var resource = Helpers.CreateAbilityResource(s.name + "FeyEidolonResource", "", "", "", null);
                resource.SetFixedResource(uses);
                var spell_like = Common.convertToSpellLike(s, "FeyEidolon", new BlueprintCharacterClass[] { Eidolon.eidolon_class }, StatType.Charisma, resource, no_scaling: true);
                var feature = Common.AbilityToFeature(spell_like, false);
                feature.AddComponent(resource.CreateAddAbilityResource());
                feature.SetDescription($"Fey Eidolon can use {feature.Name} as spell - like ability {uses} time{(uses == 1 ? "" : "s")} per day.\n"
                                       + feature.Name + ": " + feature.Description);

                transferable_abilities.Add(feature);
                features.Add(Common.createAddFeatToAnimalCompanion(feature, ""));
            }

            return features.ToArray();
        }
    }



}
