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
    public class Preservationist
    {
        static public BlueprintArchetype archetype;

        static public BlueprintFeature[] bottled_ally = new BlueprintFeature[6];

        static LibraryScriptableObject library => Main.library;

        static public void create()
        {
            var alchemist_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("0937bec61c0dabc468428f496580c721");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "PreservationistArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Preservationist");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Some alchemists are obsessed with collecting and preserving exotic creatures. These preservationists may use bottled animals and monsters as teaching tools, but some learn how to reanimate them for short periods to battle on the alchemist’s behalf. A preservationist has the following class features.");
            });
            Helpers.SetField(archetype, "m_ParentClass", alchemist_class);
            library.AddAsset(archetype, "");

            createBottledAlly();

            var posion_resistance = library.Get<BlueprintFeature>("c9022272c87bd66429176ce5c597989c");
            var poison_immunity = library.Get<BlueprintFeature>("202af59b918143a4ab7c33d72c8eb6d5");
            var persistent_mutagen = library.Get<BlueprintFeature>("75ba281feb2b96547a3bfb12ecaff052");
            var discovery = library.Get<BlueprintFeatureSelection>("cd86c437488386f438dcc9ae727ea2a6");

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(5, posion_resistance),
                                                          Helpers.LevelEntry(8, posion_resistance),
                                                          Helpers.LevelEntry(10, poison_immunity),
                                                          Helpers.LevelEntry(14, persistent_mutagen),
                                                          Helpers.LevelEntry(18, discovery)
                                                       };

            archetype.AddFeatures = new LevelEntry[] {    Helpers.LevelEntry(2, bottled_ally[0]),
                                                          Helpers.LevelEntry(5, bottled_ally[1]),
                                                          Helpers.LevelEntry(8, bottled_ally[2]),
                                                          Helpers.LevelEntry(10, bottled_ally[3]),
                                                          Helpers.LevelEntry(14, bottled_ally[4]),
                                                          Helpers.LevelEntry(18, bottled_ally[5]),
                                                       };

            alchemist_class.Progression.UIGroups = alchemist_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(bottled_ally));
            alchemist_class.Archetypes = alchemist_class.Archetypes.AddToArray(archetype);
        }


        static void createBottledAlly()
        {
            var spells = new BlueprintAbility[]
{
                library.Get<BlueprintAbility>("c6147854641924442a3bb736080cfeb6"),//I
                library.Get<BlueprintAbility>("298148133cdc3fd42889b99c82711986"),//II
                library.Get<BlueprintAbility>("c83db50513abdf74ca103651931fac4b"),//VI
                library.Get<BlueprintAbility>("8f98a22f35ca6684a983363d32e51bfe"),//V
                library.Get<BlueprintAbility>("051b979e7d7f8ec41b9fa35d04746b33"),//VII
                library.Get<BlueprintAbility>("a7469ef84ba50ac4cbf3d145e3173f8e"),//IX
            };


            var descriptions = new string[]
            {
                "At 2nd level, a preservationist adds Handle Animal to his list of class skills. He adds summon nature’s ally I to his formula book as a 1st-level extract. When he prepares that extract, he actually prepares a tiny, preserved specimen in a bottle (as with a caster casting the spell, the preservationist doesn’t have to choose the creature until he uses the extract). When the alchemist opens the bottle, the specimen animates and grows to normal size, serving the preservationist as per the spell and otherwise being treated as a summoned creature. When the duration expires, the preserved creature decays into powder. The Augment Summoning feat can be applied to these specimens.",
                "At 5th level, a preservationist adds summon nature’s ally II to his formula book as a 2nd-level extract.",
                "At 8th level, a preservationist adds summon nature’s ally IV to his formula book as a 3rd-level extract.",
                "At 10th level, a preservationist adds summon nature’s ally V to his formula book as a 4th-level extract.",
                "At 14th level, a preservationist adds summon nature’s ally VII to his formula book as a 5th-level extract.",
                "At 18th level, a preservationist adds summon nature’s ally IX to his formula book as a 6th-level extract."
            };


            for (int i = 0; i < spells.Length; i++)
            {
                var base_spell = library.CopyAndAdd(spells[i], "Preservationist" + spells[i].name, "");
                Common.unsetAsFullRoundAction(base_spell);
                base_spell.RemoveComponents<SpellListComponent>();

                if (base_spell.HasVariants)
                {
                    var variants = new List<BlueprintAbility>();

                    foreach (var v in base_spell.Variants)
                    {
                        var new_variant = library.CopyAndAdd(v, "Preservationist" + v.name, "");
                        Common.unsetAsFullRoundAction(new_variant);
                        new_variant.RemoveComponents<SpellListComponent>();
                        variants.Add(new_variant);
                    }
                    base_spell.ReplaceComponent<AbilityVariants>(base_spell.CreateAbilityVariants(variants));
                }

                bottled_ally[i] = Helpers.CreateFeature($"PreservationistBottledAlly{i + 1}Feature",
                                                        "Bottled Ally " + Common.roman_id[i + 1],
                                                        descriptions[i],
                                                        "",
                                                        base_spell.Icon,
                                                        FeatureGroup.None,
                                                        Helpers.CreateAddKnownSpell(base_spell, archetype.GetParentClass(), i + 1)
                                                        );
            }
        }
    }
}
