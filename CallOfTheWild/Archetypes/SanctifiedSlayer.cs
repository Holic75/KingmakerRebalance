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
    public class SanctifiedSlayer
    {
        static public BlueprintArchetype archetype;
        static public BlueprintFeatureSelection talented_slayer;

        static LibraryScriptableObject library => Main.library;

        internal static void create()
        {
            var inquisitor_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "SanctifiedSlayerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Sanctified Slayer");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "While all inquisitors root out enemies of the faith, in many orders and churches there’s a select group of these religious hunters devoted to one goal, and one goal alone—to terminate the enemies of the faith wherever they can be found. Sometimes these sanctified slayers are given special dispensation to commit ruthless murders for the faith’s greater good. Other times, they’re simply willing to take the initiative to revel in the zeal of such grisly work.");
            });
            Helpers.SetField(archetype, "m_ParentClass", inquisitor_class);
            library.AddAsset(archetype, "");

            var judgment = library.Get<BlueprintFeature>("981def910b98200499c0c8f85a78bde8");
            var judgment_extra_use = library.Get<BlueprintFeature>("ee50875819478774b8968701893b52f5");
            var bane = library.Get<BlueprintFeature>("7ddf7fbeecbe78342b83171d888028cf");
            var bane_greater = library.Get<BlueprintFeature>("6e694114b2f9e0e40a6da5d13736ff33");
            var judgment2 = library.Get<BlueprintFeature>("33bf0404b70d65f42acac989ec5295b2");
            var judgment3 = library.Get<BlueprintFeature>("490c7e92b22cc8a4bb4885a027b355db");
            var judgment_true = library.Get<BlueprintFeature>("f069b6557a2013544ac3636219186632");

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, judgment),
                                                          Helpers.LevelEntry(4, judgment_extra_use), 
                                                          Helpers.LevelEntry(5, bane),                                                      
                                                          Helpers.LevelEntry(7, judgment_extra_use), 
                                                          Helpers.LevelEntry(8, judgment2), 
                                                          Helpers.LevelEntry(10, judgment_extra_use), 
                                                          Helpers.LevelEntry(12, bane_greater),
                                                          Helpers.LevelEntry(13, judgment_extra_use), 
                                                          Helpers.LevelEntry(16, judgment_extra_use, judgment3), 
                                                          Helpers.LevelEntry(19, judgment_extra_use),
                                                          Helpers.LevelEntry(20, judgment_true),
                                                       };

            var sneak_attack = library.Get<BlueprintFeature>("9b9eac6709e1c084cb18c3a366e0ec87");
            var studied_target = library.Get<BlueprintFeature>("09bdd9445ac38044389476689ae8d5a1");
            studied_target.SetDescription("You can study an opponent you can see as a move action. You then gain a +1 bonus on weapon attack and damage rolls against it.\nIf you deal sneak attack damage to a target, you study that target, allowing you to apply your studied target bonuses against that target (including to the normal weapon damage roll).\nAt 5th, 10th, 15th, and 20th levels, the bonuses on weapon attack rolls and damage rolls increase by 1.");

            var studied_ability = library.Get<BlueprintAbility>("b96d810ceb1708b4e895b695ddbb1813");
            var studied_buff = library.Get<BlueprintBuff>("45548967b714e254aa83f23354f174b0");
            studied_ability.SetDescription("The character gains a +1 bonus on weapon attack and damage rolls against it. The DCs of slayer class abilities against that opponent increase by 1.\nAt 5th, 10th, 15th, and 20th levels, the bonuses on weapon attack rolls and damage rolls increase by 1.");
            studied_buff.SetDescription(studied_ability.Description);

            var swift_study = library.Get<BlueprintFeature>("40d4f55a5ac0e4f469d67d36c0dfc40b");
            swift_study.SetDescription("A character can study opponent as a move or swift action.");

            talented_slayer = library.CopyAndAdd<BlueprintFeatureSelection>("913b9cf25c9536949b43a2651b7ffb66", "TalentedSlayerFeature", ""); //slayer 10
            talented_slayer.SetNameDescription("Talented Slayer", "At 8th, 12th, 16th, and 20th levels, a sanctified slayer can gain a single slayer talent, including those from the list of rogue talents that a slayer can take, but not an advanced slayer talent.");

            var quarry = library.Get<BlueprintFeature>("385260ca07d5f1b4e907ba22a02944fc");
            var improved_quarry = library.Get<BlueprintFeature>("25e009b7e53f86141adee3a1213af5af");

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, studied_target),
                                                       Helpers.LevelEntry(4, sneak_attack),
                                                       Helpers.LevelEntry(5, studied_target),
                                                       Helpers.LevelEntry(7, swift_study, sneak_attack),
                                                       Helpers.LevelEntry(8, talented_slayer),
                                                       Helpers.LevelEntry(10, studied_target, sneak_attack),
                                                       Helpers.LevelEntry(12, talented_slayer),
                                                       Helpers.LevelEntry(13, sneak_attack),
                                                       Helpers.LevelEntry(14, quarry),
                                                       Helpers.LevelEntry(15, studied_target),
                                                       Helpers.LevelEntry(16, sneak_attack, talented_slayer),
                                                       Helpers.LevelEntry(19, sneak_attack, improved_quarry),
                                                       Helpers.LevelEntry(20, studied_target, talented_slayer),
                                                    };

            inquisitor_class.Progression.UIGroups = inquisitor_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(studied_target, swift_study));
            inquisitor_class.Progression.UIGroups[0].Features.Add(sneak_attack);
            inquisitor_class.Progression.UIGroups = inquisitor_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(quarry, improved_quarry));
            inquisitor_class.Archetypes = inquisitor_class.Archetypes.AddToArray(archetype);

        }
    }
}
