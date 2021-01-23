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

namespace CallOfTheWild
{
    public partial class SpiritsEngine
    {
        public class BonesSpirit
        {
            public  BlueprintFeature spirit_ability;
            public  BlueprintFeature greater_spirit_ability;
            public  BlueprintFeature true_spirit_ability;
            public  BlueprintFeature manifestation;
            public  BlueprintFeature bone_lock_hex;
            public  BlueprintFeature bone_ward_hex;
            public  BlueprintFeature deathly_being_hex;
            public  BlueprintFeature fearful_gaze_hex;
            public  BlueprintAbility[] spells;
            public  BlueprintFeature[] hexes;
            public StatType primary_stat;
            public StatType secondary_stat;

            HexEngine hex_engine;
            string prefix;
            string touch_of_the_grave_prefix;
            bool test_mode;


            public Oracle.Spirit createOracleSpirit(HexEngine associated_hex_engine, string asset_prefix, string touch_of_the_grave_asset_prefix, bool test = false)
            {
                test_mode = test;
                hex_engine = associated_hex_engine;
                prefix = asset_prefix;
                touch_of_the_grave_prefix = touch_of_the_grave_asset_prefix;
                primary_stat = hex_engine.hex_stat;
                secondary_stat = hex_engine.hex_stat;

                createHexes();

                createSpiritAbility();
                createGreaterSpiritAbility();

                spells = new BlueprintAbility[9]
                {
                    library.Get<BlueprintAbility>("bd81a3931aa285a4f9844585b5d97e51"), //cause fear
                    library.Get<BlueprintAbility>("7a5b5bf845779a941a67251539545762"), //false life
                    library.Get<BlueprintAbility>("4b76d32feb089ad4499c3a1ce8e1ac27"), //animate dead
                    library.Get<BlueprintAbility>("d2aeac47450c76347aebbc02e4f463e0"), //fear
                    library.Get<BlueprintAbility>("4fbd47525382517419c66fb548fe9a67"), //slay living
                    library.Get<BlueprintAbility>("a89dcbbab8f40e44e920cc60636097cf"), //circle of death
                    NewSpells.control_undead,
                    library.Get<BlueprintAbility>("08323922485f7e246acb3d2276515526"), //horrid witling
                    library.Get<BlueprintAbility>("b24583190f36a8442b212e45226c54fc"), //wail of banshee
                };


                return new Oracle.Spirit("Bones",
                                         "Bones",
                                         "A shaman who selects the bones spirit is cadaverously thin, with sunken eye sockets and dead eyes that stare off into the distance. Her body has a faint smell of the grave. When she calls upon one of this spirit’s abilities, a ghostly wind whips her hair and clothes about, and the unpleasant stench becomes more prominent.",
                                         Helpers.GetIcon("4b76d32feb089ad4499c3a1ce8e1ac27"), //animate dead
                                         "",
                                         spirit_ability,
                                         greater_spirit_ability,
                                         spells,
                                         hexes);
            }


            public Archetypes.SpiritWhisperer.Spirit createSpiritWhispererSpirit(HexEngine associated_hex_engine, string asset_prefix, string touch_of_the_grave_asset_prefix,  bool test = false)
            {
                test_mode = test;
                hex_engine = associated_hex_engine;
                prefix = asset_prefix;
                touch_of_the_grave_prefix = touch_of_the_grave_asset_prefix;
                primary_stat = hex_engine.hex_stat;
                secondary_stat = hex_engine.hex_secondary_stat;

                createHexes();

                createSpiritAbility();
                createGreaterSpiritAbility();
                createManifestation();

                return new Archetypes.SpiritWhisperer.Spirit("Bones",
                                                            "Bones",
                                                            "A shaman who selects the bones spirit is cadaverously thin, with sunken eye sockets and dead eyes that stare off into the distance. Her body has a faint smell of the grave. When she calls upon one of this spirit’s abilities, a ghostly wind whips her hair and clothes about, and the unpleasant stench becomes more prominent.",
                                                            manifestation.Icon,
                                                            "",
                                                            spirit_ability,
                                                            greater_spirit_ability,
                                                            manifestation,
                                                            hexes);
            }


            public Shaman.Spirit createShamanSpirit(HexEngine associated_hex_engine, string asset_prefix, string touch_of_the_grave_asset_prefix, bool test = false)
            {
                test_mode = test;
                hex_engine = associated_hex_engine;
                prefix = asset_prefix;
                touch_of_the_grave_prefix = touch_of_the_grave_asset_prefix;
                primary_stat = hex_engine.hex_stat;
                secondary_stat = hex_engine.hex_secondary_stat;

                createHexes();

                createSpiritAbility();
                createGreaterSpiritAbility();
                createTrueSpiritAbility();
                createManifestation();

                spells = new BlueprintAbility[9]
                {
                    library.Get<BlueprintAbility>("bd81a3931aa285a4f9844585b5d97e51"), //cause fear
                    library.Get<BlueprintAbility>("7a5b5bf845779a941a67251539545762"), //false life
                    library.Get<BlueprintAbility>("4b76d32feb089ad4499c3a1ce8e1ac27"), //animate dead
                    library.Get<BlueprintAbility>("d2aeac47450c76347aebbc02e4f463e0"), //fear
                    library.Get<BlueprintAbility>("4fbd47525382517419c66fb548fe9a67"), //slay living
                    library.Get<BlueprintAbility>("a89dcbbab8f40e44e920cc60636097cf"), //circle of death
                    library.Get<BlueprintAbility>("76a11b460be25e44ca85904d6806e5a3"), //create undead
                    library.Get<BlueprintAbility>("08323922485f7e246acb3d2276515526"), //horrid witling
                    library.Get<BlueprintAbility>("b24583190f36a8442b212e45226c54fc"), //wail of banshee
                };


                return new Shaman.Spirit("Bones",
                                  "Bones",
                                  "A shaman who selects the bones spirit is cadaverously thin, with sunken eye sockets and dead eyes that stare off into the distance. Her body has a faint smell of the grave. When she calls upon one of this spirit’s abilities, a ghostly wind whips her hair and clothes about, and the unpleasant stench becomes more prominent.",
                                  manifestation.Icon,
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
                createDeathlyBeingHex();
                bone_lock_hex = hex_engine.createBoneLock(prefix + "BoneLock",
                                            "Bone Lock",
                                            "With a quick incantation, the shaman causes a creature within 30 feet to suffer stiffness in the joints and bones, causing the target to be staggered 1 round. A successful Fortitude saving throw negates this effect. At 8th level, the duration is increased to a number of rounds equal to her shaman level, though the target can attempt a save each round to end the effect if its initial saving throw fails. At 16th level, the target can no longer attempt a saving throw each round to end the effect, although it still attempts the initial Fortitude saving throw to negate the effect entirely."
                                            );

                bone_ward_hex = hex_engine.createBoneWard(prefix + "BoneWard",
                                                        "Bone Ward",
                                                        "A shaman touches a willing creature (including herself ) and grants a bone ward. The warded creature becomes encircled by a group of flying bones that grant it a +2 deflection bonus to AC for a number of rounds equal to the shaman’s level. At 8th level, the ward increases to +3 and lasts for 1 minute. At 16th level, the bonus increases to +4 and lasts for 1 hour. A creature affected by this hex cannot be affected by it again for 24 hours."
                                                        );

                fearful_gaze_hex = hex_engine.createFearfulGaze(prefix + "FearfulGaze",
                                                                "Fearful Gaze",
                                                                "With a single shout, the shaman causes one target creature within 30 feet to become shaken for 1 round. A successful Will saving throw negates this effect. At 8th level, she makes the target frightened instead. At 16th level, she makes it panicked instead. This is a mind-affecting fear effect. A creature affected by this hex cannot be affected by it again for 24 hours."
                                                               );

                hexes = new BlueprintFeature[]
                {
                    bone_ward_hex,
                    bone_lock_hex,
                    fearful_gaze_hex,
                    deathly_being_hex,
                };
            }


            void createDeathlyBeingHex()
            {
                var icon = library.Get<BlueprintFeature>("b0acce833384b9b428f32517163c9117").Icon; //deaths_embrace

                var energy_drain_immunity_feature = library.Get<BlueprintFeature>("efe0344bca1290244a277ed5c45d9ff2");
                energy_drain_immunity_feature.HideInCharacterSheetAndLevelUp = true;

                var living_feature1 = Helpers.CreateFeature(prefix + "DeathlyBeingLiving1Feature",
                                            "Deathly Being",
                                            "",
                                            "",
                                            icon,
                                            FeatureGroup.None,
                                            Helpers.Create<UndeadMechanics.ConsiderUndeadForHealing>(),
                                            Helpers.Create<AddEnergyImmunity>(a => a.Type = DamageEnergyType.NegativeEnergy)
                                            );
                living_feature1.HideInUI = true;

                var living_feature2 = Helpers.CreateFeature(prefix + "DeathlyBeingLiving2Feature",
                                                            "",
                                                            "",
                                                            "",
                                                            icon,
                                                            FeatureGroup.None,
                                                            Common.createContextSavingThrowBonusAgainstDescriptor(4, ModifierDescriptor.UntypedStackable, SpellDescriptor.Death),
                                                            Helpers.CreateAddFeatureOnClassLevel(energy_drain_immunity_feature, 16, hex_engine.hex_classes)
                                                            );
                living_feature1.HideInUI = true;

                var undead_feature1 = Helpers.CreateFeature(prefix + "DeathlyBeingUndead1Feature",
                                          "",
                                          "",
                                          "",
                                          icon,
                                          FeatureGroup.None,
                                          Helpers.Create<SavingThrowBonusAgainstSpecificSpells>(c =>
                                                                            {
                                                                                c.Spells = new BlueprintAbility[0];
                                                                                c.Value = 1;
                                                                                c.BypassFeatures = new BlueprintFeature[] { library.Get<BlueprintFeature>("3d8e38c9ed54931469281ab0cec506e9") }; //sun domain
                                                                            }
                                                                                                )
                                          );
                ChannelEnergyEngine.addChannelResitance(undead_feature1);

                var undead_feature2 = Helpers.CreateFeature(prefix + "DeathlyBeingUndead2Feature",
                                                          "",
                                                          "",
                                                          "",
                                                          icon,
                                                          FeatureGroup.None,
                                                          Helpers.Create<NewMechanics.ContextSavingThrowBonusAgainstSpecificSpells>(c =>
                                                          {
                                                              c.Spells = new BlueprintAbility[0];
                                                              c.Value = Helpers.CreateContextValue(AbilityRankType.StatBonus);
                                                              c.BypassFeatures = new BlueprintFeature[] { library.Get<BlueprintFeature>("3d8e38c9ed54931469281ab0cec506e9") }; //sun domain
                                                          }),
                                                          Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: hex_engine.hex_classes,
                                                                                          progression: ContextRankProgression.Custom,
                                                                                          customProgression: new (int, int)[2] {(15, 1), (20, 3) }, type: AbilityRankType.StatBonus)
                                                          );
                undead_feature2.HideInCharacterSheetAndLevelUp = true;
                ChannelEnergyEngine.addChannelResitance(undead_feature2);


                var undead = library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");
                var deathly_being_hex2 = Helpers.CreateFeature(prefix + "DeathlyBeing2Feature",
                                            "",
                                            "",
                                            "",
                                            icon,
                                            FeatureGroup.None,
                                            Common.createAddFeatureIfHasFact(undead, living_feature2, not: true),
                                            Common.createAddFeatureIfHasFact(undead, undead_feature2, not: false)
                                            );
                deathly_being_hex2.HideInUI = true;

                deathly_being_hex = Helpers.CreateFeature(prefix + "DeathlyBeingFeature",
                                                          "Deathly Being",
                                                          "If the shaman is a living creature, she reacts to positive and negative energy as if she were undead—positive energy harms her, while negative energy heals her. If she’s an undead creature or a creature with the negative energy affinity ability, she gains a +1 bonus to her channel resistance. At 8th level, if she’s a living creature she gains a +4 bonus on saves against death effects and effects that drain energy, or if she’s an undead creature her bonus to channel resistance increases to +2.\n"
                                                          + "At 16th level, if the shaman a living creature, she takes no penalties from energy drain effects, though she can still be killed if she accrues more negative levels than she has Hit Dice. Furthermore, after 24 hours any negative levels the shaman has are removed without requiring her to succeed at an additional saving throw. If the shaman is an undead creature, her bonus to channel resistance increases to +4.",
                                                          "",
                                                          icon,
                                                          FeatureGroup.None,
                                                          Helpers.Create<UndeadMechanics.AddFeatureIfHasNegativeEnergyAffinity>(a => { a.Feature = living_feature1; a.inverted = true; }),
                                                          Helpers.Create<UndeadMechanics.AddFeatureIfHasNegativeEnergyAffinity>(a => { a.Feature = undead_feature1; a.inverted = true; }),
                                                          Helpers.CreateAddFeatureOnClassLevel(deathly_being_hex2, 8, hex_engine.hex_classes)
                                                          );
            }


            void createSpiritAbility()
            {
                var resource = Helpers.CreateAbilityResource(touch_of_the_grave_prefix + "TouchOfTheGraveResource", "", "", "", null);
                resource.SetIncreasedByStat(3, secondary_stat);
                var inflict_light_wounds = library.Get<BlueprintAbility>("e5cb4c4459e437e49a4cd73fde6b9063");

                var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.NegativeEnergy, Helpers.CreateContextDiceValue(DiceType.D4, 1, Helpers.CreateContextValue(AbilityRankType.Default)));
                var heal = Common.createContextActionHealTarget(Helpers.CreateContextDiceValue(DiceType.D4, 1, Helpers.CreateContextValue(AbilityRankType.Default)));
                if (Main.settings.balance_fixes)
                {
                    dmg = Helpers.CreateActionDealDamage(DamageEnergyType.NegativeEnergy, Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.Default), 0));
                    heal = Common.createContextActionHealTarget(Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.Default), 0));
                }

                

                var effect = Helpers.CreateConditional(Helpers.Create<UndeadMechanics.ContextConditionHasNegativeEnergyAffinity>(), heal, dmg);

                var touch_of_the_grave_ability = Helpers.CreateAbility(touch_of_the_grave_prefix + "TouchOfGraveAbility",
                                                                       "Touch of the Grave",
                                                                       Main.settings.balance_fixes
                                                                       ? "As a standard action, the shaman can make a melee touch attack infused with negative energy that deals 1d6 points of damage plus 1d6 point of damage for every 2 shaman levels she possesses beyond first. She can instead touch an undead creature to heal it of the same amount of damage. A shaman can use this ability a number of times per day equal to 3 + her Charisma modifier"
                                                                       : "As a standard action, the shaman can make a melee touch attack infused with negative energy that deals 1d4 points of damage + 1 point of damage for every 2 shaman levels she possesses. She can instead touch an undead creature to heal it of the same amount of damage. A shaman can use this ability a number of times per day equal to 3 + her Charisma modifier",
                                                                       "",
                                                                       inflict_light_wounds.Icon,
                                                                       AbilityType.Supernatural,
                                                                       CommandType.Standard,
                                                                       AbilityRange.Touch,
                                                                       "",
                                                                       Helpers.savingThrowNone,
                                                                       Helpers.CreateRunActions(effect),
                                                                       inflict_light_wounds.GetComponent<AbilityTargetHasFact>(),
                                                                       inflict_light_wounds.GetComponent<AbilitySpawnFx>(),
                                                                       inflict_light_wounds.GetComponent<AbilityDeliverTouch>(),
                                                                       Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: hex_engine.hex_classes,
                                                                                                       progression: Main.settings.balance_fixes ? ContextRankProgression.OnePlusDiv2 : ContextRankProgression.Div2)
                                                                      );
                touch_of_the_grave_ability.setMiscAbilityParametersTouchHarmful(true);

                var touch_of_the_grave_ability_sticky = Helpers.CreateTouchSpellCast(touch_of_the_grave_ability, resource);

                var unholy = library.Get<BlueprintWeaponEnchantment>("d05753b8df780fc4bb55b318f06af453");

                var unholy_weapon_feature = Helpers.CreateFeature(touch_of_the_grave_prefix + "TouchOfTheGraveUnholyWeaponFeature",
                                                              "",
                                                              "",
                                                              "",
                                                              null,
                                                              FeatureGroup.None,
                                                              Helpers.Create<NewMechanics.EnchantmentMechanics.PersistentWeaponEnchantment>(p => p.enchant = unholy)
                                                              );

                unholy_weapon_feature.HideInCharacterSheetAndLevelUp = true;

                spirit_ability = Common.AbilityToFeature(touch_of_the_grave_ability_sticky, false);
                spirit_ability.SetDescription("As a standard action, the shaman can make a melee touch attack infused with negative energy that deals 1d4 points of damage + 1 point of damage for every 2 shaman levels she possesses. She can instead touch an undead creature to heal it of the same amount of damage. A shaman can use this ability a number of times per day equal to 3 + her Charisma modifier. At 11th level, any weapon that the shaman wields is treated as an unholy weapon.");
                spirit_ability.AddComponents(Helpers.CreateAddFeatureOnClassLevel(unholy_weapon_feature, 11, hex_engine.hex_classes),
                                             Helpers.CreateAddAbilityResource(resource));
            }


            void createGreaterSpiritAbility()
            {           
                var resource = Helpers.CreateAbilityResource(prefix + "ShardSoulResource", "", "", "", null);
                resource.SetFixedResource(3);

                var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/ShardSoul.png");

                var cooldown_buff = Helpers.CreateBuff(prefix + "ShardSoulCooldownBuff",
                                                       "Shard Soul: Cooldown",
                                                       $"As a standard action Shaman can cause jagged pieces of bone to explode from her body in a 10-foot-radius burst. This deals 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of piercing damage for every 2 shaman levels she possesses. A successful Reflex saving throw halves this damage. The shaman can use this ability three times per day, but she must wait 1d4 rounds between each use.",
                                                       "",
                                                       icon,
                                                       null);
                var dmg = Helpers.CreateActionDealDamage(PhysicalDamageForm.Piercing,
                                                                                     Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.Default)),
                                                                                     isAoE: true, halfIfSaved: true);
                var apply_cooldown = Common.createContextActionApplyBuff(cooldown_buff, Helpers.CreateContextDuration(0, diceType: DiceType.D4, diceCount: 1), dispellable: false);
                var effect = Helpers.CreateConditional(Common.createContextConditionIsCaster(),
                                                      null,
                                                      Common.createContextActionSavingThrow(SavingThrowType.Reflex, Helpers.CreateActionList(dmg))
                                                      );

                var shard_soul_ability = Helpers.CreateAbility(prefix + "ShardSoulAbility",
                                                               "Shard Soul",
                                                               cooldown_buff.Description,
                                                               "",
                                                               icon,
                                                               AbilityType.Supernatural,
                                                               CommandType.Standard,
                                                               AbilityRange.Personal,
                                                               "",
                                                               Helpers.reflexHalfDamage,
                                                               Helpers.CreateRunActions(effect),
                                                               Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.Div2,
                                                                                               classes: hex_engine.hex_classes),
                                                               Helpers.CreateAbilityTargetsAround(10.Feet(), TargetType.Any),
                                                               Common.createAbilitySpawnFx("2644dac00cee8b840a35f2445c4dffd9", anchor: AbilitySpawnFxAnchor.Caster),
                                                               Common.createContextCalculateAbilityParamsBasedOnClasses(hex_engine.hex_classes, primary_stat),
                                                               Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionOnContextCaster(apply_cooldown))),
                                                               Common.createAbilityCasterHasNoFacts(cooldown_buff),
                                                               Helpers.CreateResourceLogic(resource)
                                                               );
                shard_soul_ability.setMiscAbilityParametersSelfOnly();
                shard_soul_ability.AddComponent(Common.createAbilityCasterHasNoFacts(cooldown_buff));

                greater_spirit_ability = Common.AbilityToFeature(shard_soul_ability, false);
                greater_spirit_ability.AddComponents(Common.createMagicDR(Helpers.CreateContextValue(AbilityRankType.Default)),
                                                     Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.OnePlusDivStep,
                                                                                     classes: hex_engine.hex_classes, stepLevel: 4),
                                                     Helpers.CreateAddAbilityResource(resource)
                                                    );
                greater_spirit_ability.SetDescription("The shaman gains DR 3/magic. This DR increases by 1 for every 4 shaman levels she possesses beyond 8th. In addition, as a standard action she can cause jagged pieces of bone to explode from her body in a 10-foot-radius burst. This deals 1d6 points of piercing damage for every 2 shaman levels she possesses. A successful Reflex saving throw halves this damage. The shaman can use this ability three times per day, but she must wait 1d4 rounds between each use.");
            }


            void createTrueSpiritAbility()
            {
                var resource = Helpers.CreateAbilityResource(prefix + "SheddingFormResource", "", "", "", null);
                resource.SetIncreasedByLevel(0, 1, hex_engine.hex_classes);

                var buff = library.CopyAndAdd<BlueprintBuff>("e82c0ec9a87a8514ba34fad5926ef129", prefix + "SheddingForm", "");
                buff.AddComponent(Common.createAddOutgoingGhost());
                buff.SetNameDescription("Shedding Form",
                                        "As a standard action, the shaman sheds her body and becomes incorporeal. While in this form, all of her weapon attacks are considered to have the ghost touch weapon special ability. The shaman can use this ability for a number of rounds equal to her shaman level, though those rounds do not need to be consecutive.");

                var ability = Helpers.CreateActivatableAbility(prefix + "SheddingFormAbility",
                                                               buff.Name,
                                                               buff.Description,
                                                               "",
                                                               buff.Icon,
                                                               buff,
                                                               AbilityActivationType.WithUnitCommand,
                                                               CommandType.Standard,
                                                               null,
                                                               Helpers.CreateActivatableResourceLogic(resource, ResourceSpendType.NewRound)
                                                               );
                true_spirit_ability = Common.ActivatableAbilityToFeature(ability, false);
                true_spirit_ability.AddComponent(Helpers.CreateAddAbilityResource(resource));
            }


            void createManifestation()
            {
                var animate_dead = library.CopyAndAdd<BlueprintAbility>("4b76d32feb089ad4499c3a1ce8e1ac27", prefix + "BonesManifestationAnimateDeadAbility", "");
                animate_dead.Type = AbilityType.Supernatural;
                animate_dead.RemoveComponents<SpellListComponent>();
                animate_dead.RemoveComponents<SpellComponent>();
                animate_dead.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: hex_engine.hex_classes));
                animate_dead.MaterialComponent = library.Get<BlueprintAbility>("2d81362af43aeac4387a3d4fced489c3").MaterialComponent;//fireball

                var power_word_kill = library.CopyAndAdd<BlueprintAbility>("2f8a67c483dfa0f439b293e094ca9e3c", prefix + "BonesManifestationPowerWordKillAbility", "");
                power_word_kill.RemoveComponents<SpellListComponent>();
                power_word_kill.RemoveComponents<SpellComponent>();
                power_word_kill.Type = AbilityType.Supernatural;
                power_word_kill.SetDescription(power_word_kill.Description.Replace("101", "151"));
                power_word_kill.ReplaceComponent<AbilityTargetHPCondition>(a => a.CurrentHPLessThan = 151);
                var new_actions = Common.changeAction<Conditional>(power_word_kill.GetComponent<AbilityEffectRunAction>().Actions.Actions,
                                                                   c => c.ConditionsChecker = Helpers.CreateConditionsCheckerOr(Helpers.Create<ContextConditionCompareTargetHP>(h => h.Value = 151))
                                                                   );
                power_word_kill.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(new_actions));
                var resource = Helpers.CreateAbilityResource(prefix + "BonesManifestationResource", "", "", "", null);
                resource.SetFixedResource(1);
                power_word_kill.AddComponent(Helpers.CreateResourceLogic(resource));

                manifestation = Helpers.CreateFeature(prefix + "BonesManifestationFeature",
                                                      "Manifestation",
                                                      "Upon reaching 20th level, the shaman becomes a spirit of death. She can cast animate dead at will without paying a material component cost, although she is still subject to the usual Hit Dice control limit. Once per day, she can cast power word kill, but the spell can target a creature with 150 hit points or fewer.",
                                                      "",
                                                      animate_dead.Icon,
                                                      FeatureGroup.None,
                                                      Helpers.CreateAddFacts(animate_dead, power_word_kill),
                                                      Helpers.CreateAddAbilityResource(resource)
                                                      );
            }


        }
 
    }
}
