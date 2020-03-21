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

        static public BlueprintProgression eidolon_progression;
        static public BlueprintProgression angel_eidolon; //ok
        static public BlueprintProgression azata_eidolon; //ok
        static public BlueprintProgression fire_elemental_eidolon; //ok
        static public BlueprintProgression water_elemental_eidolon; //ok
        static public BlueprintProgression air_elemental_eidolon; //ok? - visual might be better?
        static public BlueprintProgression earth_elemental_eidolon;
        static public BlueprintProgression demon_eidolon; //ok ?
        static public BlueprintProgression daemon_eidolon;
        static public BlueprintProgression devil_eidolon;//ok
        static public BlueprintProgression fey_eidolon; //ok
        static public BlueprintProgression inevitable_eidolon;//ok
        static public BlueprintProgression infernal_eidolon;
        static public BlueprintArchetype fey_archetype;
        static public BlueprintArchetype infernal_archetype;
        static public BlueprintFeatureSelection extra_class_skills;

        static BlueprintFeature outsider = library.Get<BlueprintFeature>("9054d3988d491d944ac144e27b6bc318");



        static public void create()
        {
            createEidolonClass();         
            createEidolonUnits();
            Evolutions.initialize();
            fillEidolonProgressions();
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
            infernal_archetype.RemoveFeatures = new LevelEntry[0];
            infernal_archetype.BaseAttackBonus = library.Get<BlueprintStatProgression>("0538081888b2d8c41893d25d098dee99"); //low bab
            var charisma_bonus = Helpers.CreateFeature("InfernalEidolonCharismaBonus",
                                                       "Eidolon Charisma Bonus",
                                                       "At 4th level and every 4 levels thereafter, the eidolon’s Charisma score increases by 2.",
                                                       "",
                                                       null,
                                                       FeatureGroup.None,
                                                       Helpers.CreateAddContextStatBonus(StatType.Charisma, ModifierDescriptor.UntypedStackable, multiplier: 2)
                                                       );
            charisma_bonus.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureRank, progression: ContextRankProgression.AsIs,
                                                                                       feature: charisma_bonus));
            charisma_bonus.Ranks = 10;
            infernal_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1)};
        }


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
            fey_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[] { fey_archetype };
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillMobility, StatType.SkillLoreNature, StatType.SkillUseMagicDevice };
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


        static void createDemonUnit()
        {
            var fx_feature = Helpers.CreateFeature("DemonEidolonFxFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Helpers.Create<UnitViewMechanics.ReplaceUnitView>(r => r.prefab = Common.createUnitViewLink("f271f20076d660c4a9eeb1992f8b96e0")) //kanerah                                                   
                                                   );
            fx_feature.HideInCharacterSheetAndLevelUp = true;
            fx_feature.HideInUI = true;

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var kanerah = library.Get<BlueprintUnit>("562750329f2aad34699e5b3c610a7d29");
            var demon_unit = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "DemonEidolonUnit", "");
            demon_unit.Color = kanerah.Color;

            demon_unit.Visual = kanerah.Visual;
            demon_unit.LocalizedName = demon_unit.LocalizedName.CreateCopy();
            demon_unit.LocalizedName.String = Helpers.CreateString(demon_unit.name + ".Name", "Damon Eidolon");

            demon_unit.Alignment = Alignment.ChaoticEvil;
            demon_unit.Strength = 16;
            demon_unit.Dexterity = 12;
            demon_unit.Constitution = 13;
            demon_unit.Intelligence = 7;
            demon_unit.Wisdom = 10;
            demon_unit.Charisma = 11;
            demon_unit.Speed = 30.Feet();
            demon_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2, fx_feature }; // { natural_armor2, fx_feature };
            demon_unit.Body = demon_unit.Body.CloneObject();
            demon_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74"); //claws 1d4
            demon_unit.Body.PrimaryHand = null;
            demon_unit.Body.SecondaryHand = null;
            demon_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            demon_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[0];
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillPerception, StatType.SkillLoreReligion, StatType.SkillStealth };
            });
            demon_unit.AddComponents(Helpers.Create<EidolonComponent>());

            Helpers.SetField(demon_unit, "m_Portrait", Helpers.createPortrait("EidolonDemonProtrait", "Demon", ""));
            demon_eidolon = Helpers.CreateProgression("DemonEidolonProgression",
                                                        "Demon Eidolon",
                                                        "Raw destruction given material substance, demon eidolons form out of the Abyss’s stew of soul energy, leading some scholars to speculate that the summoner’s arts are related to the magical tampering that gave rise to the first demons. Demon eidolons revel in causing destruction and inflicting suffering, and they will do so for their summoners without question, taking pleasure in whatever havoc they can create. For a demon eidolon, the means justify the ends.",
                                                        "",
                                                        Helpers.GetIcon("d3a4cb7be97a6694290f0dcfbd147113"), //abyssal progression
                                                        FeatureGroup.AnimalCompanion,
                                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                                        );
            demon_eidolon.IsClassFeature = true;
            demon_eidolon.ReapplyOnLevelUp = true;
            demon_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            demon_eidolon.AddComponent(Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.ChaoticEvil | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.ChaoticNeutral | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralEvil));
            demon_eidolon.ReplaceComponent<AddPet>(a => a.Pet = demon_unit);

            Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(demon_eidolon);
        }


        static void createDaemonUnit()
        {
            var fx_feature = Helpers.CreateFeature("DaemonEidolonFxFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Helpers.Create<UnitViewMechanics.ReplaceUnitView>(r => r.prefab = Common.createUnitViewLink("fbc5f03051dda4c42b49e9ccf7dd8abe")) //lich
                                                   );
            fx_feature.HideInCharacterSheetAndLevelUp = true;
            fx_feature.HideInUI = true;

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var lich = library.Get<BlueprintUnit>("d58b4a0df3282b84c97b751590053bcf");
            var daemon_unit = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "DaemonEidolonUnit", "");
            daemon_unit.Color = lich.Color;

            daemon_unit.Visual = lich.Visual;
            daemon_unit.LocalizedName = daemon_unit.LocalizedName.CreateCopy();
            daemon_unit.LocalizedName.String = Helpers.CreateString(daemon_unit.name + ".Name", "Daemon Eidolon");
            daemon_unit.Alignment = Alignment.NeutralEvil;
            daemon_unit.Strength = 16;
            daemon_unit.Dexterity = 12;
            daemon_unit.Constitution = 13;
            daemon_unit.Intelligence = 7;
            daemon_unit.Wisdom = 10;
            daemon_unit.Charisma = 11;
            daemon_unit.Speed = 30.Feet();
            daemon_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2, fx_feature }; // { natural_armor2, fx_feature };
            daemon_unit.Body = daemon_unit.Body.CloneObject();
            daemon_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74"); //claws 1d4
            daemon_unit.Body.PrimaryHand = null;
            daemon_unit.Body.SecondaryHand = null;
            daemon_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            //daemon_unit.Size = Size.Large;
            daemon_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[0];
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillPerception, StatType.SkillLoreReligion, StatType.SkillStealth };
            });
            daemon_unit.AddComponents(Helpers.Create<EidolonComponent>());


            Helpers.SetField(daemon_unit, "m_Portrait", Helpers.createPortrait("EidolonDaemonProtrait", "Daemon", ""));
            daemon_eidolon = Helpers.CreateProgression("DaemonEidolonProgression",
                                                        "Daemon Eidolon",
                                                        "The agents of horrible deaths, daemon eidolons desire the utter annihilation of all things. Their forms vary wildly depending on which type of death they embody, and daemon eidolons sometimes represent a more obscure kind of death than the most famous daemons. Daemon eidolons wish to sow death and misery through a variety of means. Most are capable of seeing the big picture, and will obediently follow even a neutral summoner. Ending lives is a typical part of an adventurer’s career, so following along with a summoner gives a daemon eidolon many opportunities to gather mortal soul energy for its own dark and inscrutable purposes.",
                                                        "",
                                                        Helpers.GetIcon("b32fd17ae27982648a30cf076790b0e8"), //daemon spawned
                                                        FeatureGroup.AnimalCompanion,
                                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                                        );
            daemon_eidolon.IsClassFeature = true;
            daemon_eidolon.ReapplyOnLevelUp = true;
            daemon_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            daemon_eidolon.AddComponent(Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.Evil | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.TrueNeutral));
            daemon_eidolon.ReplaceComponent<AddPet>(a => a.Pet = daemon_unit);

            Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(daemon_eidolon);
        }


        static void createDevilUnit()
        {
            var fx_feature = Helpers.CreateFeature("DevilEidolonFxFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Helpers.Create<UnitViewMechanics.ReplaceUnitView>(r => r.prefab = Common.createUnitViewLink("c78e19a2f6fa01343b4a188aacf38e50")) //devil apostate
                                                   //Helpers.Create<SizeMechanics.PermanentSizeOverride>(p => p.size = Size.Medium)
                                                   );
            fx_feature.HideInCharacterSheetAndLevelUp = true;
            fx_feature.HideInUI = true;

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var devil = library.Get<BlueprintUnit>("07c5044acbd443b468b6badd778f8cad");
            var devil_unit = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "DevilEidolonUnit", "");
            devil_unit.Color = devil.Color;

            devil_unit.Visual = devil.Visual;
            devil_unit.LocalizedName = devil_unit.LocalizedName.CreateCopy();
            devil_unit.LocalizedName.String = Helpers.CreateString(devil_unit.name + ".Name", "Devil Eidolon");

            devil_unit.Alignment = Alignment.LawfulEvil;
            devil_unit.Strength = 16;
            devil_unit.Dexterity = 12;
            devil_unit.Constitution = 13;
            devil_unit.Intelligence = 7;
            devil_unit.Wisdom = 10;
            devil_unit.Charisma = 11;
            devil_unit.Speed = 30.Feet();
            devil_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2, library.Get<BlueprintFeature>("203992ef5b35c864390b4e4a1e200629"), fx_feature }; // { natural_armor2, fx_feature };
            devil_unit.Body = devil_unit.Body.CloneObject();
            devil_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            devil_unit.Body.PrimaryHand = null;
            devil_unit.Body.SecondaryHand = null;
            devil_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            //devil_unit.Size = Size.Large;
            devil_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[0];
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillPerception, StatType.SkillLoreReligion, StatType.SkillStealth };
            });
            devil_unit.AddComponents(Helpers.Create<EidolonComponent>());


            Helpers.SetField(devil_unit, "m_Portrait", Helpers.createPortrait("EidolonDevilProtrait", "Devil", ""));
            devil_eidolon = Helpers.CreateProgression("DevilEidolonProgression",
                                                        "Devil Eidolon",
                                                        "Corruptors, tempters, and despoilers, devil eidolons often serve their summoners obediently and efficiently, all in a long-term attempt to damn the summoner’s soul to the deepest depths of Hell. While some types of devils have truly unusual forms, devil eidolons have found that the more traditional bipedal form allows them to build up a strong rapport with their summoners—and consequently to corrupt them—more easily than if they possessed a more monstrous appearance.",
                                                        "",
                                                        Helpers.GetIcon("e76a774cacfb092498177e6ca706064d"), //infernal bloodline
                                                        FeatureGroup.AnimalCompanion,
                                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                                        );
            devil_eidolon.IsClassFeature = true;
            devil_eidolon.ReapplyOnLevelUp = true;
            devil_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            devil_eidolon.AddComponent(Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.LawfulEvil | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralEvil | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.LawfulNeutral));
            devil_eidolon.ReplaceComponent<AddPet>(a => a.Pet = devil_unit);

            Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(devil_eidolon);


            var infernal_unit = library.CopyAndAdd<BlueprintUnit>(devil_unit, "InfernalEidolonUnit", "");
            infernal_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[] { infernal_archetype };
            });
            infernal_eidolon = library.CopyAndAdd<BlueprintProgression>(devil_eidolon, "InfernalEidolonProgression", "");
            infernal_eidolon.SetNameDescription("Infernal Binding",
                                                "A devil binder must select an eidolon of the devil subtype. The devil binder’s eidolon never increases its maximum number of attacks, and its base attack bonus is equal to half its Hit Dice. At 4th level and every 4 levels thereafter, the eidolon’s Charisma score increases by 2.");
            infernal_eidolon.ReplaceComponent<AddPet>(a => a.Pet = infernal_unit);
            infernal_eidolon.ReplaceComponent<PrerequisiteAlignment>(p => p.Alignment = Kingmaker.UnitLogic.Alignments.AlignmentMaskType.LawfulEvil | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.LawfulNeutral);
        }


        static void createInevitableUnit()
        {
            var fx_feature = Helpers.CreateFeature("InevitableEidolonFxFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Helpers.Create<UnitViewMechanics.ReplaceUnitView>(r => r.prefab = Common.createUnitViewLink("c9f3318f6aa6a3a4a9ce476989a07df5")), //adamantine golem
                                                   Helpers.Create<SizeMechanics.PermanentSizeOverride>(p => p.size = Size.Medium)); 
            fx_feature.HideInCharacterSheetAndLevelUp = true;
            fx_feature.HideInUI = true;

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var axiomite = library.Get<BlueprintUnit>("a97cc6e80fe9a454db9c0fb519fa4087");
            var inevitable_unit = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "InevitableEidolonUnit", "");
            inevitable_unit.Color = axiomite.Color;

            inevitable_unit.Visual = axiomite.Visual;
            inevitable_unit.LocalizedName = inevitable_unit.LocalizedName.CreateCopy();
            inevitable_unit.LocalizedName.String = Helpers.CreateString(inevitable_unit.name + ".Name", "Inevitable Eidolon");

            inevitable_unit.Alignment = Alignment.LawfulNeutral;
            inevitable_unit.Strength = 16;
            inevitable_unit.Dexterity = 12;
            inevitable_unit.Constitution = 13;
            inevitable_unit.Intelligence = 7;
            inevitable_unit.Wisdom = 10;
            inevitable_unit.Charisma = 11;
            inevitable_unit.Speed = 30.Feet();
            inevitable_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2, fx_feature }; // { natural_armor2, fx_feature };
            inevitable_unit.Body = inevitable_unit.Body.CloneObject();
            inevitable_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            inevitable_unit.Body.PrimaryHand = null;
            inevitable_unit.Body.SecondaryHand = null;
            inevitable_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            inevitable_unit.Size = Size.Large;
            inevitable_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[0];
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillPerception, StatType.SkillLoreReligion, StatType.SkillStealth };
            });
            inevitable_unit.AddComponents(Helpers.Create<EidolonComponent>(),
                                          Helpers.Create<UnitViewMechanics.ChangeUnitScaleForInventory>(c => c.scale_factor = 1.51f));



            Helpers.SetField(inevitable_unit, "m_Portrait", Helpers.createPortrait("EidolonInevitableProtrait", "Golem", ""));
            inevitable_eidolon = Helpers.CreateProgression("InevitableEidolonProgression",
                                        "Inevitable Eidolon",
                                        "Implacable and ceaseless in their fight against chaos and those who break natural laws, inevitables make loyal, if literal-minded, companions for lawful summoners. Summoners of inevitables generally get along well with axiomites, who share their understanding of the process of forging and modifying an inevitable. Inevitable eidolons appear as a mixture between clockwork constructs and idealized humanoid statues.",
                                        "",
                                        Helpers.GetIcon("c66e86905f7606c4eaa5c774f0357b2b"), //stone_skin
                                        FeatureGroup.AnimalCompanion,
                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                        );
            inevitable_eidolon.IsClassFeature = true;
            inevitable_eidolon.ReapplyOnLevelUp = true;
            inevitable_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            inevitable_eidolon.AddComponent(Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.Lawful | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.TrueNeutral));
            inevitable_eidolon.ReplaceComponent<AddPet>(a => a.Pet = inevitable_unit);

            Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(inevitable_eidolon);
        }



        static void createAzataUnit()
        {
            var fx_feature = Helpers.CreateFeature("AzataEidolonFxFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Helpers.Create<UnitViewMechanics.ReplaceUnitView>(r => r.prefab = Common.createUnitViewLink("255c1c746b1c31b40b16add1bb6b783e")));
            fx_feature.HideInCharacterSheetAndLevelUp = true;
            fx_feature.HideInUI = true;

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var azata = library.Get<BlueprintUnit>("d6fdf2d1776817b4bab5d4a43d9ea708");
            var azata_unit = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "AzataEidolonUnit", "");
            azata_unit.Color = azata.Color;
            azata_unit.Visual = azata.Visual;
            azata_unit.LocalizedName = azata.LocalizedName.CreateCopy();
            azata_unit.LocalizedName.String = Helpers.CreateString(azata_unit.name + ".Name", "Azata Eidolon");

            azata_unit.Alignment = Alignment.ChaoticGood;
            azata_unit.Strength = 16;
            azata_unit.Dexterity = 12;
            azata_unit.Constitution = 13;
            azata_unit.Intelligence = 7;
            azata_unit.Wisdom = 10;
            azata_unit.Charisma = 11;
            azata_unit.Speed = 30.Feet();
            azata_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2,  fx_feature }; // { natural_armor2, fx_feature };
            azata_unit.Body = azata_unit.Body.CloneObject();
            azata_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            azata_unit.Body.PrimaryHand = null;
            azata_unit.Body.SecondaryHand = null;
            azata_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            azata_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[0];
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillPerception, StatType.SkillLoreReligion, StatType.SkillStealth };
            });
            azata_unit.AddComponents(Helpers.Create<EidolonComponent>());


            Helpers.SetField(azata_unit, "m_Portrait", Helpers.createPortrait("EidolonAzataProtrait", "Azata", ""));
            azata_eidolon = Helpers.CreateProgression("AzataEidolonProgression",
                                        "Azata Eidolon",
                                        "Embodiments of the untamable beauty and noble passion of Elysium, azata eidolons have wild and beautiful features. They often take graceful forms reminiscent of elves or fey, but they occasionally appear like lillends, with serpentine tails. Azata eidolons are flighty and independent, and they often have their own ideas about how to defeat evil or have a good time. Thus, an azata eidolon is likely to balk if its summoner commands it to perform offensive or nefarious actions. On the other hand, an azata eidolon in sync with its summoner is a passionate and devoted companion.",
                                        "",
                                        Helpers.GetIcon("90810e5cf53bf854293cbd5ea1066252"), //righteous might
                                        FeatureGroup.AnimalCompanion,
                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                        );
            azata_eidolon.IsClassFeature = true;
            azata_eidolon.ReapplyOnLevelUp = true;
            azata_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            azata_eidolon.AddComponent(Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.Chaotic | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralGood));
            azata_eidolon.ReplaceComponent<AddPet>(a => a.Pet = azata_unit);
          
            Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(azata_eidolon);
        }


        static void createAngelUnit()
        {
            var fx_buff = Helpers.CreateBuff("AngelEidolonFxBuff",
                                 "",
                                 "",
                                 "",
                                 null,
                                 Common.createPrefabLink("20832f2c72b574d4cb42ee82fc244d78")); //aasimar halo
            fx_buff.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);
            var fx_feature = Helpers.CreateFeature("AngelEidolonFxFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Helpers.Create<UnitViewMechanics.ReplaceUnitView>(r => r.prefab = Common.createUnitViewLink("db4e061c2d26e01408a264cc7c569daf")), //ghaele
                                                   Common.createAuraFeatureComponent(fx_buff));
            fx_feature.HideInCharacterSheetAndLevelUp = true;
            fx_feature.HideInUI = true;

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var azata = library.Get<BlueprintUnit>("bc8ca1437c0f48948b317b7e64febf0d");
            var angel_unit = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "AngelEidolonUnit", "");
            angel_unit.Color = azata.Color;

            angel_unit.Visual = azata.Visual;
            angel_unit.LocalizedName = azata.LocalizedName.CreateCopy();
            angel_unit.LocalizedName.String = Helpers.CreateString(angel_unit.name + ".Name", "Angel Eidolon");

            angel_unit.Alignment = Alignment.NeutralGood;
            angel_unit.Strength = 16;
            angel_unit.Dexterity = 12;
            angel_unit.Constitution = 13;
            angel_unit.Intelligence = 7;
            angel_unit.Wisdom = 10;
            angel_unit.Charisma = 11;
            angel_unit.Speed = 30.Feet();
            angel_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2, fx_feature }; // { natural_armor2, fx_feature };
            angel_unit.Body = angel_unit.Body.CloneObject();
            angel_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            angel_unit.Body.PrimaryHand = null;
            angel_unit.Body.SecondaryHand = null;
            angel_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            angel_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[0];
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillPerception, StatType.SkillLoreReligion, StatType.SkillStealth };
            });
            angel_unit.AddComponents(Helpers.Create<EidolonComponent>());

            Helpers.SetField(angel_unit, "m_Portrait", Helpers.createPortrait("EidolonAngelProtrait", "Angel", ""));
            angel_eidolon = Helpers.CreateProgression("AngelEidolonProgression",
                                                        "Angel Eidolon",
                                                        "Hailing from the higher planes, angel eidolons are creatures of exquisite beauty. They usually appear in idealized humanoid forms, with smooth skin, shining hair, and bright eyes. Angel eidolons are impeccably honorable, trustworthy, and diplomatic, but they do not shy away from confrontation when facing off against evil and its minions.",
                                                        "",
                                                        Helpers.GetIcon("75a10d5a635986641bfbcceceec87217"), //angelic aspect
                                                        FeatureGroup.AnimalCompanion,
                                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                                        );
            angel_eidolon.IsClassFeature = true;
            angel_eidolon.ReapplyOnLevelUp = true;
            angel_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            angel_eidolon.AddComponent(Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.Good | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.TrueNeutral));
            angel_eidolon.ReplaceComponent<AddPet>(a => a.Pet = angel_unit);
            
            Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(angel_eidolon);
        }


        static void createEarthElementalUnit()
        {

            var fx_feature = Helpers.CreateFeature("EarthElementalEidolonFxFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Helpers.Create<UnitViewMechanics.ReplaceUnitView>(r => r.prefab = Common.createUnitViewLink("f59e51021e055b1459b0260a76cc4e54")), //stone golem
                                                   Helpers.Create<SizeMechanics.PermanentSizeOverride>(p => p.size = Size.Medium));
            fx_feature.HideInCharacterSheetAndLevelUp = true;
            fx_feature.HideInUI = true;

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var earth_elemental = library.Get<BlueprintUnit>("11d8e4b048acc0e4c8e42e76b8ab869d");
            var earth_elemental_unit = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "EarthElementalEidolonUnit", "");
            earth_elemental_unit.Color = earth_elemental.Color;

            earth_elemental_unit.Visual = earth_elemental.Visual;
            earth_elemental_unit.LocalizedName = earth_elemental_unit.LocalizedName.CreateCopy();
            earth_elemental_unit.LocalizedName.String = Helpers.CreateString(earth_elemental_unit.name + ".Name", "Elemental Eidolon (Earth)");

            earth_elemental_unit.Strength = 16;
            earth_elemental_unit.Dexterity = 12;
            earth_elemental_unit.Constitution = 13;
            earth_elemental_unit.Intelligence = 7;
            earth_elemental_unit.Wisdom = 10;
            earth_elemental_unit.Charisma = 11;
            earth_elemental_unit.Speed = 30.Feet();
            earth_elemental_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2, fx_feature }; // { natural_armor2, fx_feature };
            earth_elemental_unit.Body = earth_elemental_unit.Body.CloneObject();
            earth_elemental_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            earth_elemental_unit.Body.PrimaryHand = null;
            earth_elemental_unit.Body.SecondaryHand = null;
            earth_elemental_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            earth_elemental_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[0];
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillPerception, StatType.SkillLoreReligion, StatType.SkillStealth };
                a.DoNotApplyAutomatically = true;
            });
            earth_elemental_unit.AddComponents(Helpers.Create<EidolonComponent>(),
                                               Helpers.Create<UnitViewMechanics.ChangeUnitScaleForInventory>(c => c.scale_factor = 1.51f));
            earth_elemental_unit.Size = Size.Large;

            Helpers.SetField(earth_elemental_unit, "m_Portrait", Helpers.createPortrait("EidolonEarthElementalProtrait", "EarthElemental", ""));

            earth_elemental_eidolon = Helpers.CreateProgression("EarthElementalEidolonProgression",
                                                        "Elemental Eidolon (Earth)",
                                                        "Pulled in from one of the four elemental planes, these eidolons are linked to one of the four elements: air, earth, fire, or water. Generally, an elemental eidolon appears as a creature made entirely of one element, but there is some variation. Elemental eidolons are decidedly moderate in their views and actions. They tend to avoid the conflicts of others when they can and seek to maintain balance. The only exception is when facing off against emissaries of their opposing elements, which they hate utterly.",
                                                        "",
                                                        Helpers.GetIcon("650f8c91aaa5b114db83f541addd66d6"), //summon elemental
                                                        FeatureGroup.AnimalCompanion,
                                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                                        );
            earth_elemental_eidolon.IsClassFeature = true;
            earth_elemental_eidolon.ReapplyOnLevelUp = true;
            earth_elemental_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            earth_elemental_eidolon.AddComponent(Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.ChaoticNeutral
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.LawfulNeutral
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralEvil
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralGood
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.TrueNeutral));
            earth_elemental_eidolon.ReplaceComponent<AddPet>(a => a.Pet = earth_elemental_unit);
            Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(earth_elemental_eidolon);
        }


        static void createFireElementalUnit()
        {
            var fx_buff = Helpers.CreateBuff("FireElementalEidolonFxBuff",
                                             "",
                                             "",
                                             "",
                                             null,
                                             Common.createPrefabLink("f5eaec10b715dbb46a78890db41fa6a0"));
            fx_buff.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);

            var fx_feature = Helpers.CreateFeature("FireElementalEidolonFxFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Helpers.Create<UnitViewMechanics.ReplaceUnitView>(r => r.prefab = Common.createUnitViewLink("7cc1c50366f08814eb5a5e7c47c71a2a")),
                                                   Common.createAuraFeatureComponent(fx_buff));
            fx_feature.HideInCharacterSheetAndLevelUp = true;
            fx_feature.HideInUI = true;

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var fire_elemental = library.Get<BlueprintUnit>("37b3eb7ca48264247b3247c732007aef");
            var fire_elemental_unit = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "FireElementalEidolonUnit", "");
            fire_elemental_unit.Color = fire_elemental.Color;

            fire_elemental_unit.Visual = fire_elemental.Visual;
            fire_elemental_unit.LocalizedName = fire_elemental_unit.LocalizedName.CreateCopy();
            fire_elemental_unit.LocalizedName.String = Helpers.CreateString(fire_elemental_unit.name + ".Name", "Elemental Eidolon (Fire)");

            fire_elemental_unit.Strength = 16;
            fire_elemental_unit.Dexterity = 12;
            fire_elemental_unit.Constitution = 13;
            fire_elemental_unit.Intelligence = 7;
            fire_elemental_unit.Wisdom = 10;
            fire_elemental_unit.Charisma = 11;
            fire_elemental_unit.Speed = 30.Feet();
            fire_elemental_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2, fx_feature}; // { natural_armor2, fx_feature };
            fire_elemental_unit.Body = fire_elemental_unit.Body.CloneObject();
            fire_elemental_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            fire_elemental_unit.Body.PrimaryHand = null;
            fire_elemental_unit.Body.SecondaryHand = null;
            fire_elemental_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            fire_elemental_unit.ReplaceComponent<AddClassLevels>(a => 
                                                                { a.Archetypes = new BlueprintArchetype[0];
                                                                    a.CharacterClass = eidolon_class;
                                                                    a.Skills = new StatType[] { StatType.SkillPerception, StatType.SkillLoreReligion, StatType.SkillStealth };
                                                                    a.DoNotApplyAutomatically = true;
                                                                });
            fire_elemental_unit.AddComponents(Helpers.Create<EidolonComponent>());


            Helpers.SetField(fire_elemental_unit, "m_Portrait", Helpers.createPortrait("EidolonFireElementalProtrait", "FireElemental", ""));

            fire_elemental_eidolon = Helpers.CreateProgression("FireElementalEidolonProgression",
                                                        "Elemental Eidolon (Fire)",
                                                        "Pulled in from one of the four elemental planes, these eidolons are linked to one of the four elements: air, earth, fire, or water. Generally, an elemental eidolon appears as a creature made entirely of one element, but there is some variation. Elemental eidolons are decidedly moderate in their views and actions. They tend to avoid the conflicts of others when they can and seek to maintain balance. The only exception is when facing off against emissaries of their opposing elements, which they hate utterly.",
                                                        "",
                                                        Helpers.GetIcon("650f8c91aaa5b114db83f541addd66d6"), //summon elemental
                                                        FeatureGroup.AnimalCompanion,
                                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                                        );
            fire_elemental_eidolon.IsClassFeature = true;
            fire_elemental_eidolon.ReapplyOnLevelUp = true;
            fire_elemental_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            fire_elemental_eidolon.AddComponent(Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.ChaoticNeutral
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.LawfulNeutral
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralEvil
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralGood
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.TrueNeutral));
            fire_elemental_eidolon.ReplaceComponent<AddPet>(a => a.Pet = fire_elemental_unit);
            Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(fire_elemental_eidolon);
        }


        static void createWaterElementalUnit()
        {
            var fx_buff = Helpers.CreateBuff("WaterElementalEidolonFxBuff",
                                             "",
                                             "",
                                             "",
                                             null,
                                             Common.createPrefabLink("191b45b04a55aef4fa8b0d63992dbb16"));
            fx_buff.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);

            var fx_feature = Helpers.CreateFeature("WaterElementalEidolonFxFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Helpers.Create<UnitViewMechanics.ReplaceUnitView>(r => r.prefab = Common.createUnitViewLink("f5e1fc6f049cd55478fd31ace4d35ca1")),
                                                   Common.createAuraFeatureComponent(fx_buff));
            fx_feature.HideInCharacterSheetAndLevelUp = true;
            fx_feature.HideInUI = true;

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var water_elemental = library.Get<BlueprintUnit>("9922c4c5d1ec4cf409cf3b4742c90b51");
            var water_elemental_unit = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "WaterElementalEidolonUnit", "");
            water_elemental_unit.Color = water_elemental.Color;

            water_elemental_unit.Visual = water_elemental.Visual;
            water_elemental_unit.LocalizedName = water_elemental_unit.LocalizedName.CreateCopy();
            water_elemental_unit.LocalizedName.String = Helpers.CreateString(water_elemental_unit.name + ".Name", "Elemental Eidolon (Water)");

            water_elemental_unit.Strength = 16;
            water_elemental_unit.Dexterity = 12;
            water_elemental_unit.Constitution = 13;
            water_elemental_unit.Intelligence = 7;
            water_elemental_unit.Wisdom = 10;
            water_elemental_unit.Charisma = 11;
            water_elemental_unit.Speed = 30.Feet();
            water_elemental_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2, fx_feature }; // { natural_armor2, fx_feature };
            water_elemental_unit.Body = water_elemental_unit.Body.CloneObject();
            water_elemental_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            water_elemental_unit.Body.PrimaryHand = null;
            water_elemental_unit.Body.SecondaryHand = null;
            water_elemental_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            water_elemental_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[0];
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillPerception, StatType.SkillLoreReligion, StatType.SkillStealth };
                a.DoNotApplyAutomatically = true;
            });
            water_elemental_unit.AddComponents(Helpers.Create<EidolonComponent>());


            Helpers.SetField(water_elemental_unit, "m_Portrait", Helpers.createPortrait("EidolonWaterElementalProtrait", "WaterElemental", ""));

            water_elemental_eidolon = Helpers.CreateProgression("WaterElementalEidolonProgression",
                                                        "Elemental Eidolon (Water)",
                                                        "Pulled in from one of the four elemental planes, these eidolons are linked to one of the four elements: air, earth, fire, or water. Generally, an elemental eidolon appears as a creature made entirely of one element, but there is some variation. Elemental eidolons are decidedly moderate in their views and actions. They tend to avoid the conflicts of others when they can and seek to maintain balance. The only exception is when facing off against emissaries of their opposing elements, which they hate utterly.",
                                                        "",
                                                        Helpers.GetIcon("650f8c91aaa5b114db83f541addd66d6"), //summon elemental
                                                        FeatureGroup.AnimalCompanion,
                                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                                        );
            water_elemental_eidolon.IsClassFeature = true;
            water_elemental_eidolon.ReapplyOnLevelUp = true;
            water_elemental_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            water_elemental_eidolon.AddComponent(Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.ChaoticNeutral
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.LawfulNeutral
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralEvil
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralGood
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.TrueNeutral));
            water_elemental_eidolon.ReplaceComponent<AddPet>(a => a.Pet = water_elemental_unit);
            Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(water_elemental_eidolon);
        }


        static void createAirElementalUnit()
        {
            var fx_buff = Helpers.CreateBuff("AirElementalEidolonFxBuff",
                                             "",
                                             "",
                                             "",
                                             null,
                                             Common.createPrefabLink("6dc97e33e73b5ec49bd03b90c2345d7f"));
            fx_buff.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);

            var fx_feature = Helpers.CreateFeature("AirElementalEidolonFxFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Helpers.Create<UnitViewMechanics.ReplaceUnitView>(r => r.prefab = Common.createUnitViewLink("f5e1fc6f049cd55478fd31ace4d35ca1")),
                                                   Common.createAuraFeatureComponent(fx_buff));
            fx_feature.HideInCharacterSheetAndLevelUp = true;
            fx_feature.HideInUI = true;

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var air_elemental = library.Get<BlueprintUnit>("f739047597b7a2849b14def122e1ee0d");
            var air_elemental_unit = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "AirElementalEidolonUnit", "");
            air_elemental_unit.Color = air_elemental.Color;

            air_elemental_unit.Visual = air_elemental.Visual;
            air_elemental_unit.LocalizedName = air_elemental_unit.LocalizedName.CreateCopy();
            air_elemental_unit.LocalizedName.String = Helpers.CreateString(air_elemental_unit.name + ".Name", "Elemental Eidolon (Air)");

            air_elemental_unit.Strength = 16;
            air_elemental_unit.Dexterity = 12;
            air_elemental_unit.Constitution = 13;
            air_elemental_unit.Intelligence = 7;
            air_elemental_unit.Wisdom = 10;
            air_elemental_unit.Charisma = 11;
            air_elemental_unit.Speed = 30.Feet();
            air_elemental_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2, fx_feature }; // { natural_armor2, fx_feature };
            air_elemental_unit.Body = air_elemental_unit.Body.CloneObject();
            air_elemental_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            air_elemental_unit.Body.PrimaryHand = null;
            air_elemental_unit.Body.SecondaryHand = null;
            air_elemental_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            air_elemental_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[0];
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillPerception, StatType.SkillLoreReligion, StatType.SkillStealth };
                a.DoNotApplyAutomatically = true;
            });
            air_elemental_unit.AddComponents(Helpers.Create<EidolonComponent>());


            Helpers.SetField(air_elemental_unit, "m_Portrait", Helpers.createPortrait("EidolonAirElementalProtrait", "AirElemental", ""));

            air_elemental_eidolon = Helpers.CreateProgression("AirElementalEidolonProgression",
                                                        "Elemental Eidolon (Air)",
                                                        "Pulled in from one of the four elemental planes, these eidolons are linked to one of the four elements: air, earth, fire, or water. Generally, an elemental eidolon appears as a creature made entirely of one element, but there is some variation. Elemental eidolons are decidedly moderate in their views and actions. They tend to avoid the conflicts of others when they can and seek to maintain balance. The only exception is when facing off against emissaries of their opposing elements, which they hate utterly.",
                                                        "",
                                                        Helpers.GetIcon("650f8c91aaa5b114db83f541addd66d6"), //summon elemental
                                                        FeatureGroup.AnimalCompanion,
                                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                                        );
            air_elemental_eidolon.IsClassFeature = true;
            air_elemental_eidolon.ReapplyOnLevelUp = true;
            air_elemental_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            air_elemental_eidolon.AddComponent(Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.ChaoticNeutral
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.LawfulNeutral
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralEvil
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralGood
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.TrueNeutral));
            air_elemental_eidolon.ReplaceComponent<AddPet>(a => a.Pet = air_elemental_unit);
            Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(air_elemental_eidolon);
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
            eidolon_class.Archetypes = new BlueprintArchetype[] {fey_archetype, infernal_archetype};
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

            var devotion = library.CopyAndAdd<BlueprintFeature>("226f939b7dfd47b4697ec52f79799012", "EidolonDevotionFeature", "");
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

            eidolon_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, outsider,
                                                                                       library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                       library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa"),  //touch calculate feature
                                                                                       library.Get<BlueprintFeature>("0aeba56961779e54a8a0f6dedef081ee")), //inside the storm
                                                                    Helpers.LevelEntry(2, natural_armor, evasion, str_dex_bonus, extra_class_skills, extra_class_skills),
                                                                    Helpers.LevelEntry(3),
                                                                    Helpers.LevelEntry(4, natural_armor, str_dex_bonus),
                                                                    Helpers.LevelEntry(5, devotion),
                                                                    Helpers.LevelEntry(6, natural_armor, str_dex_bonus),
                                                                    Helpers.LevelEntry(7),
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


            eidolon_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(evasion, devotion, improved_evasion),
                                                           Helpers.CreateUIGroup(natural_armor),
                                                           Helpers.CreateUIGroup(str_dex_bonus),
                                                        };
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

    }




}
