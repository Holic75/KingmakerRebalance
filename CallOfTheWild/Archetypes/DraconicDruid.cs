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
    public class DraconicDruid
    {
        static LibraryScriptableObject library => Main.library;
        static public BlueprintArchetype archetype;
        static public BlueprintFeature dragon_shape;

        static public BlueprintFeature dragon_sense;
        static public BlueprintFeature resist_dragons_might;
        static public BlueprintFeatureSelection drake_companion;

        static public void create()
        {
            var druid = library.Get<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");
            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "DraconicDruidArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Draconic Druid");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Some druids believe that dragons are the ultimate expression of nature, combining elemental fury with majestic beauty.\nThese druids consort with dragons and eventually transform into draconic forms.");
            });
            Helpers.SetField(archetype, "m_ParentClass", druid);
            library.AddAsset(archetype, "");
            createDrakeCompanion();
            createMiscFeats();
            createDragonShape();

            var nature_sense = library.Get<BlueprintFeature>("3a859e435fdd6d343b80d4970a7664c1");
            var resist_nature_lure = library.Get<BlueprintFeature>("ad6a5b0e1a65c3540986cf9a7b006388");
            var venom_immunity = library.Get<BlueprintFeature>("5078622eb5cecaf4683fa16a9b948c2c");
            var woodland_stride = library.Get<BlueprintFeature>("11f4072ea766a5840a46e6660894527d");
            var nature_bond = library.Get<BlueprintFeature>("3830f3630a33eba49b60f511b4c8f2a8");
            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, nature_bond, nature_sense, Wildshape.druid_wildshapes_progression),
                                                          Helpers.LevelEntry(2, woodland_stride),
                                                          Helpers.LevelEntry(4, resist_nature_lure),
                                                          Helpers.LevelEntry(9, venom_immunity),
                                                        };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, dragon_shape, drake_companion, dragon_sense),
                                                       Helpers.LevelEntry(4, resist_dragons_might)
                                                     };


            druid.Progression.UIGroups = druid.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(dragon_sense, resist_dragons_might));
            druid.Progression.UIDeterminatorsGroup = druid.Progression.UIDeterminatorsGroup.AddToArray(drake_companion);
            druid.Archetypes = druid.Archetypes.AddToArray(archetype);
        }


        static void createDragonShape()
        {
            var wildshape_extra_use = library.Get<BlueprintFeature>("f78260b9a089ccc44b55f0fed08b1752");

            var druid = library.Get<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");

            dragon_shape = Wildshape.addWildshapeProgression("DraconicDruidWildshapeProgression",
                                                                    new BlueprintCharacterClass[] { druid },
                                                                    new BlueprintArchetype[0],
                                                                    new LevelEntry[] {Helpers.LevelEntry(4, Wildshape.dragon_wildshape0),
                                                                                        Helpers.LevelEntry(6, wildshape_extra_use),
                                                                                        Helpers.LevelEntry(8, wildshape_extra_use),
                                                                                        Helpers.LevelEntry(10, Wildshape.dragon_wildshape1, wildshape_extra_use),
                                                                                        Helpers.LevelEntry(12, Wildshape.dragon_wildshape2, wildshape_extra_use),
                                                                                        Helpers.LevelEntry(14, wildshape_extra_use),
                                                                                        Helpers.LevelEntry(16, wildshape_extra_use),
                                                                                        Helpers.LevelEntry(18, wildshape_extra_use),
                                                                                        Helpers.LevelEntry(20, wildshape_extra_use),
                                                                                        });
        }

        static void createMiscFeats()
        {
            dragon_sense = Helpers.CreateFeature("DragonSenseFeature",
                                                 "Dragon Sense",
                                                 "A draconic druid studies dragons and their history. She gains a +2 bonus on Knowledge (arcana) and Knowledge (world) checks.",
                                                 "",
                                                 Helpers.GetIcon("c927a8b0cd3f5174f8c0b67cdbfde539"), //remove blindness
                                                 FeatureGroup.None,
                                                 Helpers.CreateAddStatBonus(StatType.SkillKnowledgeArcana, 2, ModifierDescriptor.UntypedStackable),
                                                 Helpers.CreateAddStatBonus(StatType.SkillKnowledgeWorld, 2, ModifierDescriptor.UntypedStackable)
                                                 );

            resist_dragons_might = Helpers.CreateFeature("ResistDragonsMightFeature",
                                                         "Resist Dragon’s Might",
                                                         "A draconic druid gains a +4 bonus on saving throws against the spells, spell-like abilities, and supernatural abilities of dragons.",
                                                         "",
                                                         Helpers.GetIcon("21ffef7791ce73f468b6fca4d9371e8b"), //resist energy
                                                         FeatureGroup.None,
                                                         Common.createContextSavingThrowBonusAgainstFact(Common.dragon, AlignmentComponent.None, 4, ModifierDescriptor.UntypedStackable)
                                                         );
        }


        static void createDrakeCompanion()
        {
            var rank_profgression = library.CopyAndAdd<BlueprintProgression>("3853d5405ebfc0f4a86930bb7082b43b", "DrakeCompanionDraconicDruidProgression", "");
            rank_profgression.Classes = new BlueprintCharacterClass[] { archetype.GetParentClass() };

            drake_companion = library.CopyAndAdd(Shaman.drake_companion, "DraconicDruidDrakeCompanionFeatureSelection", "");
            drake_companion.SetDescription("A draconic druid gains a drake companion instead of an animal companion.");
            for (int i = 0; i < drake_companion.AllFeatures.Length; i++)
            {
                var ff = library.CopyAndAdd<BlueprintFeature>(drake_companion.AllFeatures[i].AssetGuid, "DraconicDruid" + drake_companion.AllFeatures[i].name, "");
                //ff.SetDescription(drake_companion.Description);
            }
            drake_companion.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.Create<AddFeatureOnApply>(a => a.Feature = library.Get<BlueprintFeature>("1670990255e4fe948a863bafd5dbda5d")),
                Helpers.Create<AddFeatureOnApply>(a => a.Feature = rank_profgression)
            };
        }



    }
}

