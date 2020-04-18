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
    class DivineTracker
    {
        static public BlueprintArchetype archetype;

        static public BlueprintFeatureSelection blessings;

        static LibraryScriptableObject library => Main.library;

        static public void create()
        {
            var ranger_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("cda0615668a6df14eb36ba19ee881af6");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "DivineTrackerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Divine Tracker");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Blessed by his deity, a divine tracker hunts down those he deems deserving of his retribution. His weapon is likely to find purchase in his favored enemy.");
            });
            Helpers.SetField(archetype, "m_ParentClass", ranger_class);
            library.AddAsset(archetype, "");

            createBlessings();
            var deity_selection = library.Get<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4");
            var hunters_bond = library.Get<BlueprintFeatureSelection>("b705c5184a96a84428eeb35ae2517a14");

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(4, hunters_bond)};

            archetype.AddFeatures = new LevelEntry[] {    Helpers.LevelEntry(1, deity_selection),
                                                          Helpers.LevelEntry(4, blessings, blessings)
                                                       };

            ranger_class.Progression.UIDeterminatorsGroup = ranger_class.Progression.UIDeterminatorsGroup.AddToArray(deity_selection);
            ranger_class.Archetypes = ranger_class.Archetypes.AddToArray(archetype);
        }


        static void createBlessings()
        {
            blessings = Helpers.CreateFeatureSelection("DivineTrackerBlessingsSelection",
                                                     "Blessing",
                                                     "At 4th level, a divine tracker forms a close bond with his deity’s ethos. He selects two warpriest domains from among the domains granted by his deity, and gains the minor blessings of those domains. A divine tracker can select an alignment domain (Chaos, Evil, Good, or Law) only if his alignment matches that domain.\n"
                                                     + "Each blessing grants a minor power at 1st level and a major power at 13th level. A divine tracker can call upon the power of his blessings a number of times per day (in any combination) equal to 3 + 1 / 2 his divine tracker level (to a maximum of 13 times per day at 20th level). Each time he calls upon any one of his blessings, it counts against his daily limit. The save DC for these blessings is equal to 10 + 1 / 2 the divine tracker’s level + the divine tracker’s Wisdom modifier.",
                                                     "",
                                                     null,
                                                     FeatureGroup.None);
            //blessing will be created in warpriest part
        }

    }
}
