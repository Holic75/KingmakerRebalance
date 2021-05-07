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
    public class Beastmorph
    {
        static public BlueprintArchetype archetype;

        static public BlueprintFeature feral_wings;
        static public BlueprintFeature pounce;
        static public BlueprintFeature feral_mutagen;
        static public BlueprintFeature blindsense;

        static LibraryScriptableObject library => Main.library;

        static public void create()
        {
            var alchemist_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("0937bec61c0dabc468428f496580c721");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "BeastmorphArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Beastmorph");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Beastmorphs study the anatomy of monsters, learning how they achieve their strange powers. They use their knowledge to duplicate these abilities, but at the cost of taking on inhuman shapes when they use mutagens.");
            });
            Helpers.SetField(archetype, "m_ParentClass", alchemist_class);
            library.AddAsset(archetype, "");


            var posion_resistance = library.Get<BlueprintFeature>("c9022272c87bd66429176ce5c597989c");
            var poison_immunity = library.Get<BlueprintFeature>("202af59b918143a4ab7c33d72c8eb6d5");
            var persistent_mutagen = library.Get<BlueprintFeature>("75ba281feb2b96547a3bfb12ecaff052");

            createFeralMutagenAndWings();
            createBeastformMutagen();

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(5, posion_resistance),
                                                          Helpers.LevelEntry(8, posion_resistance),
                                                          Helpers.LevelEntry(10, poison_immunity),
                                                          Helpers.LevelEntry(14, persistent_mutagen),
                                                       };

            archetype.AddFeatures = new LevelEntry[] {    Helpers.LevelEntry(3, feral_mutagen),
                                                          Helpers.LevelEntry(6, feral_wings),
                                                          Helpers.LevelEntry(10, pounce),
                                                          Helpers.LevelEntry(14, blindsense),
                                                       };

            alchemist_class.Progression.UIGroups = alchemist_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(feral_mutagen, feral_wings, pounce, blindsense));
            alchemist_class.Archetypes = alchemist_class.Archetypes.AddToArray(archetype);
        }


        static void createBeastformMutagen()
        {
            var pounce_buff = Helpers.CreateBuff("BeasmorphPounceBuff",
                                                "Pounce",
                                                "At 10th level, when under effect of his mutagen, the beastmorph gains pounce ability.",
                                                "",
                                                Helpers.GetIcon("c78506dd0e14f7c45a599990e4e65038"),
                                                null,
                                                Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.Pounce)
                                                );

            var blindsense_buff = Helpers.CreateBuff("BeasmorphBlindsenseBuff",
                                                "Blindsense",
                                                "At 14th level, when under effect of his mutagen, the beastmorph gains blindsense ability within 30 feet.",
                                                "",
                                                Helpers.GetIcon("b3da3fbee6a751d4197e446c7e852bcb"), //true seeing
                                                null,
                                                Common.createBlindsense(30)
                                                );

            var buffs = new BlueprintBuff[] { pounce_buff, blindsense_buff };
            List<BlueprintFeature> features = new List<BlueprintFeature>();

            var mutagens = new BlueprintFeature[]
            {
                library.Get<BlueprintFeature>("cee8f65448ce71c4b8b8ca13751dd8ea"), //mutagen
                library.Get<BlueprintFeature>("76c61966afdd82048911f3d63c6fe0bc"), //greater mutagen
                library.Get<BlueprintFeature>("6f5cb651e26bd97428523061b07ffc85"), //grand mutagen

                library.Get<BlueprintFeature>("e3f460ea61fcc504183c7d6818bbbf7a"), //cognatogen
                library.Get<BlueprintFeature>("18eb29676492e844eb5a55d1c855ce69"), //greater cognatogen
                library.Get<BlueprintFeature>("af4a320648eb5724889d6ff6255090b2"), //grand cognatogen
            };

            foreach (var b in buffs)
            {
                var feature = Helpers.CreateFeature(b.name + "Feature",
                                                    b.Name,
                                                    b.Description,
                                                    "",
                                                    b.Icon,
                                                    FeatureGroup.None
                                                    );
                foreach (var m in mutagens)
                {
                    var comp = m.GetComponent<AddFacts>();

                    foreach (var f in comp.Facts)
                    {
                        var mutagen_buff = Common.extractActions<ContextActionApplyBuff>((f as BlueprintAbility).GetComponent<AbilityEffectRunAction>().Actions.Actions)[0].Buff;
                        Common.addContextActionApplyBuffOnCasterFactsToActivatedAbilityBuffNoRemove(mutagen_buff, b, feature);
                    }
                }
                features.Add(feature);
            }

            pounce = features[0];
            blindsense = features[1];
        }


        static void createFeralMutagenAndWings()
        {
            var feral_mutagen_discovery = library.Get<BlueprintFeature>("fd5f7b37ab4301c48a88cc196ee5f0ce");
            feral_mutagen = Helpers.CreateFeature("BeastmorphFeralMutagenFeature",
                                                "Feral Mutagen",
                                                "At 3rd level, beastmorph receives Feral Mutagen alchemist discovery.",
                                                "",
                                                feral_mutagen_discovery.Icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddFact(feral_mutagen_discovery)
                                                );

            var feral_wings_discovery = library.Get<BlueprintFeature>("78197196e096c6e4eaed5c62fa108b52");
            feral_wings = Helpers.CreateFeature("BeastmorphFeralWingsFeature",
                                                "Feral Wings",
                                                "At 6th level, beastmorph receives Feral Wings alchemist discovery.",
                                                "",
                                                feral_wings_discovery.Icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddFact(feral_wings_discovery)
                                                );
        }
    }
}
