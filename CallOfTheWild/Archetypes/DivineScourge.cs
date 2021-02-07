using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CallOfTheWild.NewMechanics;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Experience;
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
    class DivineScourge
    {
        static public BlueprintArchetype archetype;
        static public BlueprintFeatureSelection curse_subdomain;
        static public BlueprintFeatureSelection divine_hexes;
        static public BlueprintFeatureSelection spontnaeous_conversion;
        static LibraryScriptableObject library => Main.library;

        static public void create()
        {
            var cleric_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "DivineScourgeArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Divine Scourge");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Some divine servants take on the role of dealing out unique punishments on behalf of their deities, taking pleasure in carrying out their sacrosanct duties. Such divine scourges are most common among worshipers of Abadar (meting out punishment to lawbreakers in concert with local courts), Calistria (punishing those truly deserving of vengeance), and Zon-Kuthon (seeing punishment as an applied form of pain and suffering). Divine scourges make a point of inflicting long-lasting maladies and curses on those deserving of such fates under the tenets of the scourges’ religions.");
            });
            Helpers.SetField(archetype, "m_ParentClass", cleric_class);
            library.AddAsset(archetype, "");

            createSpontaneousConversion();
            createCurseSubdomain();
            createDivineHexes();
            

            var channel_selection = library.Get<BlueprintFeatureSelection>("d332c1748445e8f4f9e92763123e31bd");
            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1,  library.Get<BlueprintFeature>("48525e5da45c9c243a343fc6545dbdb9"), //domain selection
                                                                                library.Get<BlueprintFeature>("43281c3d7fe18cc4d91928395837cd1e"), //second domain
                                                                                channel_selection)
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, curse_subdomain, spontnaeous_conversion),
                                                       Helpers.LevelEntry(3, divine_hexes),
                                                       Helpers.LevelEntry(7, divine_hexes),
                                                       Helpers.LevelEntry(11, divine_hexes),
                                                       Helpers.LevelEntry(15, divine_hexes),
                                                       Helpers.LevelEntry(19, divine_hexes),
                                                     };

            cleric_class.Progression.UIDeterminatorsGroup = cleric_class.Progression.UIDeterminatorsGroup.AddToArray(curse_subdomain);
            cleric_class.Progression.UIGroups = cleric_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(divine_hexes));
            cleric_class.Archetypes = cleric_class.Archetypes.AddToArray(archetype);
        }


        static void createSpontaneousConversion()
        {
            var cure_conversion = library.Get<BlueprintFeature>("5e4620cea099c9345a9207c11d7bc916");
            cure_conversion.AddComponent(Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("8c769102f3996684fb6e09a2c4e7e5b9")));

            var inflict_conversion = library.Get<BlueprintFeature>("5ba6b9cc18acafd45b6293d1e03221ac");
            inflict_conversion.AddComponent(Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("dab5255d809f77c4395afc2b713e9cd6")));

            spontnaeous_conversion = library.CopyAndAdd<BlueprintFeatureSelection>("d332c1748445e8f4f9e92763123e31bd", "DivineScourgeSpontnaeousConversionSelection", "");
            spontnaeous_conversion.AllFeatures = new BlueprintFeature[] { cure_conversion, inflict_conversion };
            spontnaeous_conversion.SetDescription("Divine Scourge is unable to channel energy. He can still channel stored spell energy into cure or wounding spells.");
        }


        static void createCurseSubdomain()
        {
            curse_subdomain = Helpers.CreateFeatureSelection("CurseSubdomainDivineScourgeFeature",
                                                    Subdomains.curse_domain.Name,
                                                    "A divine scourge must take the curse subdomain as a domain, regardless of the actual domains offered by her deity. The divine scourge does not receive a second domain.",
                                                    "",
                                                    Subdomains.curse_domain.Icon,
                                                    FeatureGroup.Domain
                                                    );
            curse_subdomain.IgnorePrerequisites = true;
            curse_subdomain.AllFeatures = new BlueprintFeature[] { Subdomains.curse_domain };
        }


        static void createDivineHexes()
        {
            var hex_engine = new HexEngine(new BlueprintCharacterClass[] {archetype.GetParentClass() }, StatType.Wisdom, StatType.Charisma, archetype: archetype);

            var slumber_hex = hex_engine.createSlumber("DivineScourgeSlumber",
                                       Witch.slumber_hex.Name,
                                       Witch.slumber_hex.Description,
                                       "", "", "");

            var misfortune_hex = hex_engine.createMisfortune("DivineScourgeMisfortune",
                                                           Witch.misfortune_hex.Name,
                                                           Witch.misfortune_hex.Description,
                                                           "", "", "", "");

            var evil_eye = hex_engine.createEvilEye("DivineScourgeEvilEye",
                                                   Witch.evil_eye.Name,
                                                   Witch.evil_eye.Description,
                                                   "", "", "", "", "", "", "", "");

            var agony = hex_engine.createAgony("DivineScourgeAgony",
                                               Witch.agony.Name,
                                               Witch.agony.Description,
                                               "", "", "", "");

            var restless_slumber = hex_engine.createRestlessSlumber("DivineScourgeRestlessSlumber",
                                                                   Witch.restless_slumber.Name,
                                                                   Witch.restless_slumber.Description
                                                                   );
            restless_slumber.AddComponent(slumber_hex.PrerequisiteFeature());

            var retribution = hex_engine.createRetribution("DivineScourgeRetribution",
                                                                   Witch.retribution.Name,
                                                                   Witch.retribution.Description,
                                                                   "", "", ""
                                                                   );

            divine_hexes = Helpers.CreateFeatureSelection("DivineScourgeDivineHexSelection",
                                                          "Divine Hexes",
                                                          "At 3rd level and every 4 cleric levels thereafter, a divine scourge can select the following hexes from the witch class hex list, up to a maximum of five hexes at 19th level: evil eye, misfortune, and slumber.\n"
                                                          + "At 11th level, a divine scourge can instead select from the following list of major hexes: agony, restless slumber, and retribution.\n"
                                                          + "The divine scourge uses her Wisdom modifier instead of her Intelligence modifier to determine the save DCs of her hexes. Any hex that refers to using her Intelligence modifier to determine its duration or effect instead uses her Charisma modifier for that purpose.",
                                                          "",
                                                          null,
                                                          FeatureGroup.None);

            divine_hexes.AllFeatures = new BlueprintFeature[] { slumber_hex, evil_eye, misfortune_hex, agony, restless_slumber, retribution };
        }

    }
}
