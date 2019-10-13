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
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Root;
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
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
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
    partial class Shaman
    {
        static LibraryScriptableObject library => Main.library;
        internal static bool test_mode = false;
        static public BlueprintCharacterClass shaman_class;
        static public BlueprintProgression shaman_progression;
        static public BlueprintFeatureSelection primary_shaman_spirits;
        static public BlueprintFeatureSelection secondary_shaman_spirits;
        static public BlueprintFeatureSelection hex_selection;
        static public BlueprintFeatureSelection witch_hex_selection;
        static public BlueprintFeatureSelection shaman_familiar;
        static public BlueprintFeature shaman_cantrips;
        //hexes
        static public BlueprintFeature healing;
        static public BlueprintFeature chant;
        static public BlueprintFeature misfortune_hex;
        static public BlueprintFeature fortune_hex;
        static public BlueprintFeature evil_eye;
        static public BlueprintFeature ward;
        static public BlueprintFeature shapeshift;
        static public BlueprintFeature wings_attack_hex;
        static public BlueprintFeature wings_hex;
        static public BlueprintFeature draconic_resilence;
        static public BlueprintFeature fury;
        static public BlueprintFeature secret;
        static public BlueprintFeature intimidating_display;
        //additional witch hexes
        static public BlueprintFeature beast_of_ill_omen;
        static public BlueprintFeature slumber_hex;
        static public BlueprintFeature iceplant_hex;
        static public BlueprintFeature murksight_hex;
        static public BlueprintFeature ameliorating;
        static public BlueprintFeature summer_heat;


        static HexEngine hex_engine;


        static BlueprintCharacterClass[] getShamanArray()
        {
            return new BlueprintCharacterClass[] { shaman_class };
        }


    }
}
