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
                //createGreaterSpiritAbility();
                //createTrueSpiritAbility();
                //createManifestation();

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
                var saves_feature = Helpers.CreateFeature("ShamanDeathlyBeing2LivingFeature",
                                                          "Deathly Being II",
                                                          "If the shaman is a living creature, she reacts to positive and negative energy as if she were undead—positive energy harms her, while negative energy heals her. If she’s an undead creature or a creature with the negative energy affinity ability, she gains a + 1 bonus to her channel resistance. At 8th level, if she’s a living creature she gains a + 4 bonus on saves against death effects and effects that drain energy, or if she’s an undead creature her bonus to channel resistance increases to + 2.\n"
                                                           + "At 16th level, if the shaman a living creature, she takes no penalties from energy drain effects. If the shaman is an undead creature, her bonus to channel resistance increases to + 4.",
                                                          "",
                                                          icon,
                                                          FeatureGroup.None,
                                                          Common.createSavingThrowBonusAgainstDescriptor(4, ModifierDescriptor.UntypedStackable, SpellDescriptor.Death)
                                                          );
                saves_feature.HideInCharacterSheetAndLevelUp = true;
                var energy_drain_feature = Helpers.CreateFeature("ShamanDeathlyBeing3LivingFeature",
                                                                  "Deathly Being III",
                                                                  saves_feature.Description,
                                                                  "",
                                                                  icon,
                                                                  FeatureGroup.None,
                                                                  Helpers.Create<AddImmunityToEnergyDrain>()
                                                                  );
                energy_drain_feature.HideInCharacterSheetAndLevelUp = true;

                var undead_feature = Helpers.CreateFeature("ShamanDeathlyBeingUndeadFeature",
                                                          "Deathly Being",
                                                          saves_feature.Description,
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
                                                                                          customProgression: new (int, int)[3] { (7, 1), (15, 2), (20, 4) }, type: AbilityRankType.StatBonus)
                                                          );
                undead_feature.HideInCharacterSheetAndLevelUp = true;

                var living_feature = Helpers.CreateFeature("ShamanDeathlyBeingLivingFeature",
                                                            "Deathly Being",
                                                            saves_feature.Description,
                                                            "",
                                                            icon,
                                                            FeatureGroup.None,
                                                            Helpers.Create<UndeadMechanics.ConsiderUndeadForHealing>(),
                                                            Helpers.Create<AddEnergyImmunity>(a => a.Type = DamageEnergyType.NegativeEnergy),
                                                            Helpers.CreateAddFeatureOnClassLevel(saves_feature, 8, getShamanArray()),
                                                            Helpers.CreateAddFeatureOnClassLevel(energy_drain_feature, 16, getShamanArray())
                                                            );
                living_feature.HideInUI = true;

                var undead = library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");
                deathly_being_hex = Helpers.CreateFeature("ShamanDeathlyBeingLivingFeature",
                                                            "Deathly Being",
                                                            saves_feature.Description,
                                                            "",
                                                            icon,
                                                            FeatureGroup.None,
                                                            Common.createAddFeatureIfHasFact(undead, living_feature, not: true),
                                                            Common.createAddFeatureIfHasFact(undead, undead_feature)
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
                spirit_ability.AddComponent(Helpers.CreateAddFeatureOnClassLevel(unholy_weapon_feature, 11, getShamanArray()));
            }

        }
 
    }
}
