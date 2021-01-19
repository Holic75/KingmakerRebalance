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
using Kingmaker.Designers.EventConditionActionSystem.Actions;
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
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
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
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;

namespace CallOfTheWild
{
    partial class SpiritsEngine
    {
        public class HeavensSpirit
        {
            public  BlueprintFeature spirit_ability;
            public  BlueprintFeature greater_spirit_ability;
            public  BlueprintFeature true_spirit_ability;
            public  BlueprintFeature manifestation;
            public  BlueprintFeature enveloping_void;
            public  BlueprintFeature heavens_leap;
            public  BlueprintFeature lure_of_heavens;
            public  BlueprintFeature starburn;
            public  BlueprintAbility[] spells;
            public  BlueprintFeature[] hexes;
            public StatType primary_stat;
            public StatType secondary_stat;

            HexEngine hex_engine;
            string prefix;
            bool test_mode;


            public Oracle.Spirit createOracleSpirit(HexEngine associated_hex_engine, string asset_prefix, bool test = false)
            {
                test_mode = test;
                hex_engine = associated_hex_engine;
                prefix = asset_prefix;
                primary_stat = hex_engine.hex_stat;
                secondary_stat = hex_engine.hex_stat;
                createHexes();

                createSpiritAbility();
                createGreaterSpiritAbility();

                spells = new BlueprintAbility[9]
                {
                    library.Get<BlueprintAbility>("91da41b9793a4624797921f221db653c"), //color sparay
                    NewSpells.hypnotic_pattern,
                    SpellDuplicates.addDuplicateSpell(library.Get<BlueprintAbility>("ce7dad2b25acf85429b6c9550787b2d9"), "ShamanHeavensGlitterdust", ""), //glitterdust
                    library.Get<BlueprintAbility>("4b8265132f9c8174f87ce7fa6d0fe47b"), //rainbow pattern
                    NewSpells.overland_flight,
                    library.Get<BlueprintAbility>("645558d63604747428d55f0dd3a4cb58"), //chain lightning
                    library.Get<BlueprintAbility>("b22fd434bdb60fb4ba1068206402c4cf"), //prismatic spray
                    library.Get<BlueprintAbility>("e96424f70ff884947b06f41a765b7658"), //sunburst
                    NewSpells.meteor_swarm
                };

                return new Oracle.Spirit("Heavens",
                                        "Heavens",
                                        "A shaman who selects the heavens spirit has eyes that sparkle like starlight, exuding an aura of otherworldliness to those she is around. When she calls upon one of this spirit’s abilities, her eyes turn pitch black and the colors around her drain for a brief moment.",
                                        library.Get<BlueprintAbility>("ce7dad2b25acf85429b6c9550787b2d9").Icon,//glitterdust
                                        "",
                                        spirit_ability,
                                        greater_spirit_ability,
                                        spells,
                                        hexes);
            }


            public Archetypes.SpiritWhisperer.Spirit createSpiritWhispererSpirit(HexEngine associated_hex_engine, string asset_prefix, bool test = false)
            {
                test_mode = test;
                hex_engine = associated_hex_engine;
                prefix = asset_prefix;
                primary_stat = hex_engine.hex_stat;
                secondary_stat = hex_engine.hex_secondary_stat;
                createHexes();

                createSpiritAbility();
                createGreaterSpiritAbility();
                createManifestation();

                return new Archetypes.SpiritWhisperer.Spirit("Heavens",
                                                              "Heavens",
                                                              "A shaman who selects the heavens spirit has eyes that sparkle like starlight, exuding an aura of otherworldliness to those she is around. When she calls upon one of this spirit’s abilities, her eyes turn pitch black and the colors around her drain for a brief moment.",
                                                              library.Get<BlueprintAbility>("ce7dad2b25acf85429b6c9550787b2d9").Icon,//glitterdust
                                                              "",
                                                              spirit_ability,
                                                              greater_spirit_ability,
                                                              manifestation,
                                                              hexes);
            }

            public Shaman.Spirit createShamanSpirit(HexEngine associated_hex_engine, string asset_prefix, bool test = false)
            {
                test_mode = test;
                hex_engine = associated_hex_engine;
                prefix = asset_prefix;
                primary_stat = hex_engine.hex_stat;
                secondary_stat = hex_engine.hex_secondary_stat;
                createHexes();

                createSpiritAbility();
                createGreaterSpiritAbility();
                createTrueSpiritAbility();
                createManifestation();

                spells = new BlueprintAbility[9]
                {
                    library.Get<BlueprintAbility>("91da41b9793a4624797921f221db653c"), //color sparay
                    NewSpells.hypnotic_pattern,
                    SpellDuplicates.addDuplicateSpell(library.Get<BlueprintAbility>("ce7dad2b25acf85429b6c9550787b2d9"), "ShamanHeavensGlitterdust", ""), //glitterdust
                    library.Get<BlueprintAbility>("4b8265132f9c8174f87ce7fa6d0fe47b"), //rainbow pattern
                    NewSpells.overland_flight,
                    library.Get<BlueprintAbility>("645558d63604747428d55f0dd3a4cb58"), //chain lightning
                    library.Get<BlueprintAbility>("b22fd434bdb60fb4ba1068206402c4cf"), //prismatic spray
                    library.Get<BlueprintAbility>("e96424f70ff884947b06f41a765b7658"), //sunburst
                    NewSpells.meteor_swarm
                };

                return new Shaman.Spirit("Heavens",
                                  "Heavens",
                                  "A shaman who selects the heavens spirit has eyes that sparkle like starlight, exuding an aura of otherworldliness to those she is around. When she calls upon one of this spirit’s abilities, her eyes turn pitch black and the colors around her drain for a brief moment.",
                                  library.Get<BlueprintAbility>("ce7dad2b25acf85429b6c9550787b2d9").Icon,//glitterdust
                                  "",
                                  spirit_ability,
                                  greater_spirit_ability,
                                  true_spirit_ability,
                                  manifestation,
                                  hexes,
                                  spells);
            }


            void createHexes()
            {
                enveloping_void = hex_engine.createEnveloppingVoid(prefix + "EnvelopingVoid",
                                            "Enveloping Void",
                                            "The shaman curses one creature with the dark void. As a standard action, the shaman can cause one enemy within close range to become blind. This effect lasts for a number of rounds equal to the shaman’s level. A successful Will saving throw negates this effect. Whether or not the save is successful, the creature cannot be the target of this hex again for 24 hours."
                                            );

                lure_of_heavens = hex_engine.CreateFlightHex(prefix + "LureOfHeavens",
                                                            "Lure of Heavens",
                                                            "The shaman gains ability to fly as per fly spell."
                                                            );
                lure_of_heavens.AddComponent(Helpers.PrerequisiteClassLevel(hex_engine.hex_classes[0], 10));

                heavens_leap = hex_engine.createHeavensLeap(prefix + "HeavensLeap",
                                                                "Heaven's Leap",
                                                                "The shaman is adept at creating tiny tears in the fabric of space, and temporarily stitching them together to reach other locations through a limited, one-way wormhole. As a standard action, the shaman can designate herself or a single ally that she can see who is within close range of her. She can move that creature to any point within close range of herself. Once targeted by this hex, the ally cannot be the target of this hex again for 24 hours."
                                                               );

                starburn = hex_engine.createStarburn(prefix + "Starburn",
                                                    "Starburn",
                                                    $"As a standard action, the shaman causes one creature within close range to burn like a star. The creature takes 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of fire damage for every 2 levels the shaman possesses and emits bright light as per faerie fire spell for 1 round. A successful Fortitude saving throw halves the damage and negates the emission of light. The shaman can use this hex a number of times per day equal to her Charisma modifier + 1, but must wait 1d4 rounds between uses."
                                                   );
                hexes = new BlueprintFeature[]
                {
                    enveloping_void,
                    lure_of_heavens,
                    heavens_leap,
                    starburn
                };
            }


            void createSpiritAbility()
            {
                var icon = library.Get<BlueprintAbility>("ce7dad2b25acf85429b6c9550787b2d9").Icon; //glitterdust
                var resource = Helpers.CreateAbilityResource(prefix + "StardustResource", "", "", "", null);
                resource.SetIncreasedByStat(3, secondary_stat);

                var buff = library.CopyAndAdd<BlueprintBuff>("cc383a9eaae4d2b45a925d442b367b54", prefix + "StardustBuff", "");
                buff.SetNameDescription("Stardust",
                                        "As a standard action, the shaman causes stardust to materialize around one creature within 30 feet. This stardust causes the target to shed light as a candle, and it cannot benefit from concealment or any invisibility effects. The creature takes a –1 penalty on attack rolls and sight-based Perception checks. This penalty to attack rolls and Perception checks increases by 1 at 4th level and every 4 levels thereafter, to a maximum of –6 at 20th level. This effect lasts for a number of rounds equal to half the shaman’s level (minimum 1). Sightless creatures cannot be affected by this ability. The shaman can use this ability a number of times per day equal to 3 + her Charisma modifier.");

                buff.AddComponent(Helpers.CreateAddContextStatBonus(StatType.SkillPerception, ModifierDescriptor.UntypedStackable, multiplier: -1));
                buff.AddComponent(Helpers.CreateAddContextStatBonus(StatType.AdditionalAttackBonus, ModifierDescriptor.UntypedStackable, multiplier: -1));
                buff.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.SightBased));
                buff.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: hex_engine.hex_classes, progression: ContextRankProgression.OnePlusDivStep, stepLevel: 4));

                var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);

                var stardust = Helpers.CreateAbility(prefix + "StardustAbility",
                                                     buff.Name,
                                                     buff.Description,
                                                     "",
                                                     buff.Icon,
                                                     AbilityType.Supernatural,
                                                     CommandType.Standard,
                                                     AbilityRange.Close,
                                                     "1 round/2 shaman levels",
                                                     Helpers.savingThrowNone,
                                                     Helpers.CreateRunActions(apply_buff),
                                                     Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: hex_engine.hex_classes, progression: ContextRankProgression.Div2, min: 1),
                                                     Helpers.CreateSpellDescriptor(SpellDescriptor.SightBased),
                                                     Helpers.CreateResourceLogic(resource)
                                                     );
                stardust.setMiscAbilityParametersSingleTargetRangedHarmful(test_mode);

                spirit_ability = Helpers.CreateFeature(prefix + "StardustFeature",
                                                       stardust.Name,
                                                       stardust.Description,
                                                       "",
                                                       stardust.Icon,
                                                       FeatureGroup.None,
                                                       Helpers.CreateAddFact(stardust),
                                                       Helpers.CreateAddAbilityResource(resource)
                                                       );
            }


            void createGreaterSpiritAbility()
            {
                var icon = library.Get<BlueprintAbility>("14ec7a4e52e90fa47a4c8d63c69fd5c1").Icon; //blur

                greater_spirit_ability = Helpers.CreateFeature(prefix + "VoidAdaptationFeature",
                                                               "Void Adaptation",
                                                               "The shaman gains blindsense 30 feet, immunity to poison and suffocation.",
                                                               "",
                                                               icon,
                                                               FeatureGroup.None,
                                                               Common.createBlindsense(30),
                                                               Common.createBuffDescriptorImmunity(SpellDescriptor.Poison),
                                                               Common.createSpecificBuffImmunity(NewSpells.suffocation_buff),
                                                               Common.createSpecificBuffImmunity(NewSpells.fast_suffocation_buff)
                                                               );
            }


            void createTrueSpiritAbility()
            {
                var resource = Helpers.CreateAbilityResource(prefix + "PhantasmagoricDisplay", "", "", "", null);
                resource.SetFixedResource(3);

                var ability = library.CopyAndAdd<BlueprintAbility>("b22fd434bdb60fb4ba1068206402c4cf", prefix + "HeavensPrismaticSprayAbility", "");
                ability.Type = AbilityType.SpellLike;

                ability.RemoveComponents<SpellListComponent>();
                ability.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClasses(hex_engine.hex_classes, primary_stat));
                ability.AddComponent(Helpers.CreateResourceLogic(resource));

                true_spirit_ability = Helpers.CreateFeature(prefix + "HeavensPrismaticSprayFeature",
                                                           "Phantasmagoric Display",
                                                           "Shaman can use Prismatic Spray spell 3 times per day as spell-like ability.",
                                                           "",
                                                           ability.Icon,
                                                           FeatureGroup.None,
                                                           Helpers.CreateAddFact(ability),
                                                           Helpers.CreateAddAbilityResource(resource)
                                                           );
            }


            void createManifestation()
            {
                manifestation = Helpers.CreateFeature(prefix + "HeavensManifestationFeature",
                                                      "Manifestation",
                                                      "Upon reaching 20th level, the shaman becomes the spirit of heaven. She receives a bonus on all saving throws equal to her Wisdom modifier. She’s immune to fear effects, and she automatically confirms all critical hits she threatens.",
                                                      "",
                                                      library.Get<BlueprintAbility>("90810e5cf53bf854293cbd5ea1066252").Icon, //righteous might
                                                      FeatureGroup.None,
                                                      Common.createAddConditionImmunity(UnitCondition.Shaken),
                                                      Common.createAddConditionImmunity(UnitCondition.Frightened),
                                                      Common.createBuffDescriptorImmunity(SpellDescriptor.Frightened | SpellDescriptor.Shaken),
                                                      Helpers.Create<CritAutoconfirmAgainstAlignment>(c => c.EnemyAlignment = AlignmentComponent.None),
                                                      Helpers.CreateAddContextStatBonus(StatType.SaveFortitude, ModifierDescriptor.UntypedStackable),
                                                      Helpers.CreateAddContextStatBonus(StatType.SaveReflex, ModifierDescriptor.UntypedStackable),
                                                      Helpers.CreateAddContextStatBonus(StatType.SaveWill, ModifierDescriptor.UntypedStackable),
                                                      Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, stat: primary_stat)
                                                      );
            }


        }


    }
}

