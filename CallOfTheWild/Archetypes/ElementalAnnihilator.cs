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
using Kingmaker.UnitLogic.Class.Kineticist;
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
    public class ElementalAnnihilator
    {
        static public BlueprintArchetype archetype;
        static public BlueprintFeature devastating_infusion;
        static public BlueprintFeatureSelection dampened_versality;
        static public BlueprintFeature increased_range;
        static public BlueprintFeature blast_training;
        static public BlueprintFeature omnicide;

        static LibraryScriptableObject library => Main.library;

        internal static void create()
        {
            var kineticist_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("42a455d9ec1ad924d889272429eb8391");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "ElementalAnnihilatorArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Elemental Annihilator");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "For some kineticists, nothing in life is as sweet as destruction and pain. Elemental annihilators pursue only uses of their powers that harm others.");
            });
            Helpers.SetField(archetype, "m_ParentClass", kineticist_class);
            library.AddAsset(archetype, "");


            kineticist_class.Archetypes = kineticist_class.Archetypes.AddToArray(archetype);
        }
    }
}
