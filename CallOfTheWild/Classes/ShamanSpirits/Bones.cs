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
    partial class Shaman
    {
        public class BonesSpirit
        {
            public static BlueprintFeature spirit_ability;
            public static BlueprintFeature greater_spirit_ability;
            public static BlueprintFeature true_spirit_ability;
            public static BlueprintFeature manifestation;
            public static BlueprintFeature bone_lock_hex;
            public static BlueprintFeature bone_ward_hex;
            public static BlueprintFeature deathly_being_hex;
            public static BlueprintFeature fearful_gaze_hex;
            public static BlueprintAbility[] spells;
            public static BlueprintFeature[] hexes;

            public static void create()
            {
                createDeathlyBeingHex();

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

                bone_lock_hex = hex_engine.createBoneLock("ShamanBoneLock",
                                                            "Bone Lock",
                                                            "With a quick incantation, the shaman causes a creature within 30 feet to suffer stiffness in the joints and bones, causing the target to be staggered 1 round. A successful Fortitude saving throw negates this effect. At 8th level, the duration is increased to a number of rounds equal to her shaman level, though the target can attempt a save each round to end the effect if its initial saving throw fails. At 16th level, the target can no longer attempt a saving throw each round to end the effect, although it still attempts the initial Fortitude saving throw to negate the effect entirely."
                                                            );

                bone_ward_hex = hex_engine.createBoneWard("ShamanBoneWard",
                                                        "Bone Ward",
                                                        "A shaman touches a willing creature (including herself ) and grants a bone ward. The warded creature becomes encircled by a group of flying bones that grant it a +2 deflection bonus to AC for a number of rounds equal to the shaman’s level. At 8th level, the ward increases to +3 and lasts for 1 minute. At 16th level, the bonus increases to +4 and lasts for 1 hour. A creature affected by this hex cannot be affected by it again for 24 hours."
                                                        );

                fearful_gaze_hex = hex_engine.createFearfulGaze("ShamanFearfulGaze",
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


            static public void createDeathlyBeingHex()
            {
                var icon = library.Get<BlueprintFeature>("b0acce833384b9b428f32517163c9117").Icon; //deaths_embrace

                var energy_drain_immunity_feature = library.Get<BlueprintFeature>("efe0344bca1290244a277ed5c45d9ff2");
                energy_drain_immunity_feature.HideInCharacterSheetAndLevelUp = true;

                var living_feature1 = Helpers.CreateFeature("ShamanDeathlyBeingLiving1Feature",
                                            "Deathly Being",
                                            "",
                                            "",
                                            icon,
                                            FeatureGroup.None,
                                            Helpers.Create<UndeadMechanics.ConsiderUndeadForHealing>(),
                                            Helpers.Create<AddEnergyImmunity>(a => a.Type = DamageEnergyType.NegativeEnergy)
                                            );
                living_feature1.HideInUI = true;

                var living_feature2 = Helpers.CreateFeature("ShamanDeathlyBeingLiving2Feature",
                                                            "",
                                                            "",
                                                            "",
                                                            icon,
                                                            FeatureGroup.None,
                                                            Common.createContextSavingThrowBonusAgainstDescriptor(4, ModifierDescriptor.UntypedStackable, SpellDescriptor.Death),
                                                            Helpers.CreateAddFeatureOnClassLevel(energy_drain_immunity_feature, 16, getShamanArray())
                                                            );
                living_feature1.HideInUI = true;

                var undead_feature1 = Helpers.CreateFeature("ShamanDeathlyBeingUndead1Feature",
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

                var undead_feature2 = Helpers.CreateFeature("ShamanDeathlyBeingUndead2Feature",
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
                                                          Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getShamanArray(),
                                                                                          progression: ContextRankProgression.Custom,
                                                                                          customProgression: new (int, int)[2] {(15, 1), (20, 3) }, type: AbilityRankType.StatBonus)
                                                          );
                undead_feature2.HideInCharacterSheetAndLevelUp = true;


                var undead = library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");
                var deathly_being_hex2 = Helpers.CreateFeature("ShamanDeathlyBeing2Feature",
                                            "",
                                            "",
                                            "",
                                            icon,
                                            FeatureGroup.None,
                                            Common.createAddFeatureIfHasFact(undead, living_feature2, not: true),
                                            Common.createAddFeatureIfHasFact(undead, undead_feature2, not: false)
                                            );
                deathly_being_hex2.HideInUI = true;

                deathly_being_hex = Helpers.CreateFeature("ShamanDeathlyBeingFeature",
                                                          "Deathly Being",
                                                          "If the shaman is a living creature, she reacts to positive and negative energy as if she were undead—positive energy harms her, while negative energy heals her. If she’s an undead creature or a creature with the negative energy affinity ability, she gains a +1 bonus to her channel resistance. At 8th level, if she’s a living creature she gains a +4 bonus on saves against death effects and effects that drain energy, or if she’s an undead creature her bonus to channel resistance increases to +2.\n"
                                                          + "At 16th level, if the shaman a living creature, she takes no penalties from energy drain effects, though she can still be killed if she accrues more negative levels than she has Hit Dice. Furthermore, after 24 hours any negative levels the shaman has are removed without requiring her to succeed at an additional saving throw. If the shaman is an undead creature, her bonus to channel resistance increases to +4.",
                                                          "",
                                                          icon,
                                                          FeatureGroup.None,
                                                          Helpers.Create<UndeadMechanics.AddFeatureIfHasNegativeEnergyAffinity>(a => { a.Feature = living_feature1; a.inverted = true; }),
                                                          Helpers.Create<UndeadMechanics.AddFeatureIfHasNegativeEnergyAffinity>(a => { a.Feature = undead_feature1; a.inverted = true; }),
                                                          Helpers.CreateAddFeatureOnClassLevel(deathly_being_hex2, 8, getShamanArray())
                                                          );
            }


            static void createSpiritAbility()
            {
                var resource = Helpers.CreateAbilityResource("TouchOfTheGraveResource", "", "", "", null);
                resource.SetIncreasedByStat(3, StatType.Charisma);
                var inflict_light_wounds = library.Get<BlueprintAbility>("e5af3674bb241f14b9a9f6b0c7dc3d27");
                var touch_of_the_grave_ability =  Common.replaceCureInflictSpellParameters(inflict_light_wounds,
                                                                                            "TouchOfTheGraveAbility",
                                                                                            "Touch of the Grave",
                                                                                            "As a standard action, the shaman can make a melee touch attack infused with negative energy that deals 1d4 points of damage + 1 point of damage for every 2 shaman levels she possesses. She can instead touch an undead creature to heal it of the same amount of damage. A shaman can use this ability a number of times per day equal to 3 + her Charisma modifier",
                                                                                            AbilityType.Supernatural,
                                                                                            Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.Div2,
                                                                                                                            classes: getShamanArray()),
                                                                                            Helpers.CreateContextDiceValue(DiceType.D4, 1, Helpers.CreateContextValue(AbilityRankType.Default)),
                                                                                            false,
                                                                                            "",
                                                                                            "",
                                                                                            "",
                                                                                            ""
                                                                                            );
                touch_of_the_grave_ability.RemoveComponents<SpellComponent>();
                touch_of_the_grave_ability.RemoveComponents<SpellListComponent>();
                touch_of_the_grave_ability.AddComponent(Helpers.CreateResourceLogic(resource));

                var unholy = library.Get<BlueprintWeaponEnchantment>("d05753b8df780fc4bb55b318f06af453");

                var unholy_weapon_feature = Helpers.CreateFeature("TouchOfTheGraveUnholyWeaponFeature",
                                                              "",
                                                              "",
                                                              "",
                                                              null,
                                                              FeatureGroup.None,
                                                              Helpers.Create<NewMechanics.EnchantmentMechanics.PersistentWeaponEnchantment>(p => p.enchant = unholy)
                                                              );

                unholy_weapon_feature.HideInCharacterSheetAndLevelUp = true;

                spirit_ability = Common.AbilityToFeature(touch_of_the_grave_ability, false);
                spirit_ability.SetDescription("As a standard action, the shaman can make a melee touch attack infused with negative energy that deals 1d4 points of damage + 1 point of damage for every 2 shaman levels she possesses. She can instead touch an undead creature to heal it of the same amount of damage. A shaman can use this ability a number of times per day equal to 3 + her Charisma modifier. At 11th level, any weapon that the shaman wields is treated as an unholy weapon.");
                spirit_ability.AddComponents(Helpers.CreateAddFeatureOnClassLevel(unholy_weapon_feature, 11, getShamanArray()),
                                             Helpers.CreateAddAbilityResource(resource));
            }


            static void createGreaterSpiritAbility()
            {           
                var resource = Helpers.CreateAbilityResource("ShamanShardSoulResource", "", "", "", null);
                resource.SetFixedResource(3);

                var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/ShardSoul.png");

                var cooldown_buff = Helpers.CreateBuff("ShamanShardSoulCooldownBuff",
                                                       "Shard Soul: Cooldown",
                                                       "As a standard action Shaman can cause jagged pieces of bone to explode from her body in a 10-foot-radius burst. This deals 1d6 points of piercing damage for every 2 shaman levels she possesses. A successful Reflex saving throw halves this damage. The shaman can use this ability three times per day, but she must wait 1d4 rounds between each use.",
                                                       "",
                                                       icon,
                                                       null);
                var apply_cooldown = Common.createContextActionApplyBuff(cooldown_buff, Helpers.CreateContextDuration(0, diceType: DiceType.D4, diceCount: 1), dispellable: false);
                var effect = Helpers.CreateConditional(Common.createContextConditionIsCaster(),
                                                      apply_cooldown,
                                                      Helpers.CreateActionDealDamage(PhysicalDamageForm.Piercing, 
                                                                                     Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.Default)), 
                                                                                     isAoE: true, halfIfSaved: false)
                                                      );

                var shard_soul_ability = Helpers.CreateAbility("ShamanShardSoulAbility",
                                                               "Shard Soul",
                                                               cooldown_buff.Description,
                                                               "",
                                                               icon,
                                                               AbilityType.Supernatural,
                                                               CommandType.Standard,
                                                               AbilityRange.Personal,
                                                               "",
                                                               Helpers.reflexHalfDamage,
                                                               Helpers.CreateRunActions(SavingThrowType.Reflex, effect),
                                                               Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.Div2,
                                                                                               classes: getShamanArray()),
                                                               Helpers.CreateAbilityTargetsAround(10.Feet(), TargetType.Any),
                                                               Common.createContextCalculateAbilityParamsBasedOnClass(shaman_class, StatType.Charisma),
                                                               Common.createAbilitySpawnFx("2644dac00cee8b840a35f2445c4dffd9", anchor: AbilitySpawnFxAnchor.Caster)
                                                               );
                shard_soul_ability.setMiscAbilityParametersSelfOnly();
                shard_soul_ability.AddComponent(Common.createAbilityCasterHasNoFacts(cooldown_buff));

                greater_spirit_ability = Common.AbilityToFeature(shard_soul_ability, false);
                greater_spirit_ability.AddComponents(Common.createMagicDR(Helpers.CreateContextValue(AbilityRankType.Default)),
                                                     Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.OnePlusDivStep,
                                                                                     classes: getShamanArray(), startLevel: 4),
                                                     Helpers.CreateAddAbilityResource(resource)
                                                    );
            }


            static void createTrueSpiritAbility()
            {
                var resource = Helpers.CreateAbilityResource("ShamanSheddingFormResource", "", "", "", null);
                resource.SetIncreasedByLevel(0, 1, getShamanArray());

                var buff = library.CopyAndAdd<BlueprintBuff>("e82c0ec9a87a8514ba34fad5926ef129", "ShamanSheddingForm", "");
                buff.AddComponent(Common.createAddOutgoingGhost());
                buff.SetNameDescription("Shedding Form",
                                        "As a standard action, the shaman sheds her body and becomes incorporeal. While in this form, all of her weapon attacks are considered to have the ghost touch weapon special ability. The shaman can use this ability for a number of rounds equal to her shaman level, though those rounds do not need to be consecutive.");

                var ability = Helpers.CreateActivatableAbility("ShamanSheddingFormAbility",
                                                               buff.Name,
                                                               buff.Description,
                                                               "",
                                                               buff.Icon,
                                                               buff,
                                                               AbilityActivationType.Immediately,
                                                               CommandType.Standard,
                                                               null,
                                                               Helpers.CreateActivatableResourceLogic(resource, ResourceSpendType.NewRound)
                                                               );
                true_spirit_ability = Common.ActivatableAbilityToFeature(ability, false);
                true_spirit_ability.AddComponent(Helpers.CreateAddAbilityResource(resource));
            }


            static void createManifestation()
            {
                var animate_dead = library.CopyAndAdd<BlueprintAbility>("4b76d32feb089ad4499c3a1ce8e1ac27", "ShamanBonesManifestationAnimateDeadAbility", "");
                animate_dead.Type = AbilityType.Supernatural;
                animate_dead.RemoveComponents<SpellListComponent>();
                animate_dead.RemoveComponents<SpellComponent>();
                animate_dead.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getShamanArray()));

                var power_word_kill = library.CopyAndAdd<BlueprintAbility>("2f8a67c483dfa0f439b293e094ca9e3c", "ShamanBonesManifestationPowerWordKillAbility", "");
                power_word_kill.RemoveComponents<SpellListComponent>();
                power_word_kill.RemoveComponents<SpellComponent>();
                power_word_kill.Type = AbilityType.Supernatural;
                power_word_kill.SetDescription(power_word_kill.Description.Replace("101", "151"));
                power_word_kill.ReplaceComponent<AbilityTargetHPCondition>(a => a.CurrentHPLessThan = 151);
                var new_actions = Common.changeAction<Conditional>(power_word_kill.GetComponent<AbilityEffectRunAction>().Actions.Actions,
                                                                   c => c.ConditionsChecker = Helpers.CreateConditionsCheckerOr(Helpers.Create<ContextConditionCompareTargetHP>(h => h.Value = 151))
                                                                   );
                power_word_kill.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(new_actions));
                var resource = Helpers.CreateAbilityResource("ShamanBonesManifestationResource", "", "", "", null);
                resource.SetIncreasedByLevel(0, 1, getShamanArray());
                power_word_kill.AddComponent(Helpers.CreateResourceLogic(resource));

                manifestation = Helpers.CreateFeature("ShamanBonesManifestationFeature",
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
