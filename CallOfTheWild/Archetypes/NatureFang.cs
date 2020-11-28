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
    public class NatureFang
    {
        static LibraryScriptableObject library => Main.library;
        static public BlueprintArchetype archetype;
        static public BlueprintFeature studied_target;
        static public BlueprintFeature swift_study;
        static public BlueprintFeature sneak_attack;
        static public BlueprintFeatureSelection slayer_talent4, slayer_talent6, slayer_talent10;

        static public void create()
        {
            var druid = library.Get<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");
            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "NatureFangDruidArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Nature Fang");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "A nature fang is a druid who stalks and slays those who despoil nature, kill scarce animals, or introduce diseases to unprotected habitats. She gives up a close empathic connection with the natural world to become its deadly champion and avenger.");
            });
            Helpers.SetField(archetype, "m_ParentClass", druid);
            library.AddAsset(archetype, "");
            createStudiedTarget();
            createSlayerTalents();

            var nature_sense = library.Get<BlueprintFeature>("3a859e435fdd6d343b80d4970a7664c1");
            var resist_nature_lure = library.Get<BlueprintFeature>("ad6a5b0e1a65c3540986cf9a7b006388");
            var venom_immunity = library.Get<BlueprintFeature>("5078622eb5cecaf4683fa16a9b948c2c");
            var woodland_stride = library.Get<BlueprintFeature>("11f4072ea766a5840a46e6660894527d");
            var advanced_talents = library.Get<BlueprintFeature>("a33b99f95322d6741af83e9381b2391c");
            var sneak_attack = library.Get<BlueprintFeature>("9b9eac6709e1c084cb18c3a366e0ec87");
            var sneak_attack_give = library.CopyAndAdd<BlueprintFeature>("9b9eac6709e1c084cb18c3a366e0ec87", "NatureFangSneakAttack", "");
            sneak_attack_give.SetDescription("At 4th level, a nature fang gains sneak attack +1d6. This functions as the rogue sneak attack ability. If the nature fang gets a sneak attack bonus from another source, the bonuses on damage stack.");
            sneak_attack_give.ComponentsArray = new BlueprintComponent[] { Helpers.Create<AddFeatureOnApply>(a => a.Feature = sneak_attack) };
            advanced_talents.SetDescription("A character can choose one of the advanced rogue or slayer talents in place of a standard talent.");

            
            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, nature_sense, Wildshape.druid_wildshapes_progression),
                                                          Helpers.LevelEntry(2, woodland_stride),
                                                          Helpers.LevelEntry(4, resist_nature_lure),
                                                          Helpers.LevelEntry(9, venom_immunity),
                                                        };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, studied_target),
                                                       Helpers.LevelEntry(4, sneak_attack_give, slayer_talent4),
                                                       Helpers.LevelEntry(5, studied_target),
                                                       Helpers.LevelEntry(6, slayer_talent6),
                                                       Helpers.LevelEntry(8, slayer_talent6),
                                                       Helpers.LevelEntry(9, swift_study),
                                                       Helpers.LevelEntry(10, studied_target, slayer_talent10),
                                                       Helpers.LevelEntry(12, slayer_talent10, advanced_talents),
                                                       Helpers.LevelEntry(14, slayer_talent10),
                                                       Helpers.LevelEntry(15, studied_target),
                                                       Helpers.LevelEntry(16, slayer_talent10),
                                                       Helpers.LevelEntry(18, slayer_talent10),
                                                       Helpers.LevelEntry(20, studied_target, slayer_talent10)};

            druid.Progression.UIGroups = druid.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(sneak_attack_give, studied_target, swift_study, advanced_talents));
            druid.Progression.UIGroups = druid.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(slayer_talent4, slayer_talent6, slayer_talent10));
            druid.Archetypes = druid.Archetypes.AddToArray(archetype);
        }


        static void createStudiedTarget()
        {
            studied_target = library.Get<BlueprintFeature>("09bdd9445ac38044389476689ae8d5a1");
            swift_study = library.Get<BlueprintFeature>("40d4f55a5ac0e4f469d67d36c0dfc40b");
        }


        static void createSlayerTalents()
        {
            slayer_talent4 = library.CopyAndAdd<BlueprintFeatureSelection>("04430ad24988baa4daa0bcd4f1c7d118", "NatureFangSlayerTalent4FeatureSelection", "");
            slayer_talent4.SetDescription("At 4th level and every 2 levels thereafter, a nature fang selects a slayer talent. Starting at 12th level, she can select an advanced slayer talent in place of a slayer talent. She uses her druid level as her slayer level to determine what talents she can select.");
            slayer_talent6 = library.CopyAndAdd<BlueprintFeatureSelection>("43d1b15873e926848be2abf0ea3ad9a8", "NatureFangSlayerTalent6FeatureSelection", "");
            slayer_talent6.SetDescription(slayer_talent4.Description);
            slayer_talent10 = library.CopyAndAdd<BlueprintFeatureSelection>("913b9cf25c9536949b43a2651b7ffb66", "NatureFangSlayerTalent10FeatureSelection", "");
            slayer_talent10.SetDescription(slayer_talent10.Description);
        }
    }
}

