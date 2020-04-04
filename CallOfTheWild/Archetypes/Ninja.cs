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
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild.Archetypes
{
    public class Ninja
    {
        static public BlueprintArchetype archetype;

        static public BlueprintFeature ninja_proficiencies;
        static public BlueprintFeature ki_pool;
        static public BlueprintFeature no_trace;
        static public BlueprintFeature light_steps;

        static public BlueprintFeatureSelection ninja_trick;
        //base tricks
        static public BlueprintFeature acceleration_of_form;
        static public BlueprintFeature chocking_bomb;
        static public BlueprintFeature herbal_compound;
        static public BlueprintFeature kamikaze;
        static public BlueprintFeature pressure_points;
        static public BlueprintFeature smoke_bomb;
        static public BlueprintFeature style_master;
        static public BlueprintFeature unarmed_combat_training;
        static public BlueprintFeature shadow_clone;
        static public BlueprintFeature vanishing_trick;
        //+all rogue talents
        //master tricks
        static public BlueprintFeature blinding_bomb;
        static public BlueprintFeature invisible_blade;
        static public BlueprintFeature see_the_unseen;
        //+evasion


        static LibraryScriptableObject library => Main.library;

        static public void create()
        {
            var rogue_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("299aa766dee3cbf4790da4efb8c72484");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "NinjaArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Ninja");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "When the wealthy and the powerful need an enemy eliminated quietly and without fail, they call upon the ninja. When a general needs to sabotage the siege engines of his foes before they can reach the castle walls, he calls upon the ninja. And when fools dare to move against a ninja or her companions, they will find the ninja waiting for them while they sleep, ready to strike. These shadowy killers are masters of infiltration, sabotage, and assassination, using a wide variety of weapons, practiced skills, and mystical powers to achieve their goals.");
            });
            Helpers.SetField(archetype, "m_ParentClass", rogue_class);
            library.AddAsset(archetype, "");

            var rogue_proficiencies = library.Get<BlueprintFeature>("33e2a7e4ad9daa54eaf808e1483bb43c");
            var weapon_finesse = library.Get<BlueprintFeature>("90e54424d682d104ab36436bd527af09");
            var trapfinding = library.Get<BlueprintFeature>("dbb6b3bffe6db3547b31c3711653838e");
            var evasion = library.Get<BlueprintFeature>("576933720c440aa4d8d42b0c54b77e80");
            var rogue_talent = library.Get<BlueprintFeatureSelection>("c074a5d615200494b8f2a9c845799d93");
            var finesse_training_selection = library.Get<BlueprintFeature>("b78d146cea711a84598f0acef69462ea");
            var danger_sense = library.Get<BlueprintFeature>("0bcbe9e450b0e7b428f08f66c53c5136");
            var debilitating_injury = library.Get<BlueprintFeature>("def114eb566dfca448e998969bf51586");
            var uncanny_dodge = library.Get<BlueprintFeature>("3c08d842e802c3e4eb19d15496145709");
            var improved_uncanny_dodge = library.Get<BlueprintFeature>("485a18c05792521459c7d06c63128c79");
            var advanced_talents = library.Get<BlueprintFeature>("a33b99f95322d6741af83e9381b2391c");
            var master_strike = library.Get<BlueprintFeature>("72dcf1fb106d5054a81fd804fdc168d3");

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, rogue_proficiencies, weapon_finesse, trapfinding),
                                                          Helpers.LevelEntry(2, evasion, rogue_talent),
                                                          Helpers.LevelEntry(3, finesse_training_selection, danger_sense),
                                                          Helpers.LevelEntry(4, rogue_talent, debilitating_injury),
                                                          Helpers.LevelEntry(6, rogue_talent, danger_sense),
                                                          Helpers.LevelEntry(8, rogue_talent),
                                                          Helpers.LevelEntry(9, danger_sense),
                                                          Helpers.LevelEntry(10, rogue_talent),
                                                          Helpers.LevelEntry(11, finesse_training_selection),
                                                          Helpers.LevelEntry(12, rogue_talent, danger_sense),
                                                          Helpers.LevelEntry(14, rogue_talent),
                                                          Helpers.LevelEntry(15, danger_sense),
                                                          Helpers.LevelEntry(16, rogue_talent),
                                                          Helpers.LevelEntry(18, rogue_talent, danger_sense),
                                                          Helpers.LevelEntry(19, finesse_training_selection),
                                                          Helpers.LevelEntry(20, rogue_talent), //will leave master strike as capstone
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, ninja_proficiencies),
                                                          Helpers.LevelEntry(2, ki_pool, ninja_trick),
                                                          Helpers.LevelEntry(3, no_trace),
                                                          Helpers.LevelEntry(4, ninja_trick),
                                                          Helpers.LevelEntry(6, ninja_trick, no_trace, light_steps),
                                                          Helpers.LevelEntry(8, ninja_trick),
                                                          Helpers.LevelEntry(9, no_trace),
                                                          Helpers.LevelEntry(10, ninja_trick), //master_trick ?
                                                          Helpers.LevelEntry(12, ninja_trick, no_trace),
                                                          Helpers.LevelEntry(14, ninja_trick),
                                                          Helpers.LevelEntry(15, no_trace),
                                                          Helpers.LevelEntry(16, ninja_trick),
                                                          Helpers.LevelEntry(18, ninja_trick, no_trace),
                                                          Helpers.LevelEntry(20, ninja_trick),
                                                       };

            rogue_class.Progression.UIDeterminatorsGroup = rogue_class.Progression.UIDeterminatorsGroup.AddToArray(ninja_proficiencies);
            rogue_class.Progression.UIGroups[2].Features.Add(ki_pool);
            rogue_class.Archetypes = rogue_class.Archetypes.AddToArray(archetype);
        }
    }
}
