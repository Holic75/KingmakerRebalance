using System;
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

namespace CallOfTheWild
{
    partial class Shaman
    {
        internal class WavesSpirit
        {
            internal static BlueprintFeature spirit_ability;
            internal static BlueprintFeature greater_spirit_ability;
            internal static BlueprintFeature true_spirit_ability;
            internal static BlueprintFeature manifestation;
            internal static BlueprintFeature fluid_magic;
            internal static BlueprintFeature beckoning_chill;
            internal static BlueprintFeature mists_shroud;
            internal static BlueprintFeature crashing_waves;
            internal static BlueprintAbility[] spells;
            internal static BlueprintFeature[] hexes;

            internal static Spirit create()
            {
                createFluidMagicHex();

                createSpiritAbility();
                createGreaterSpiritAbility();
                createTrueSpiritAbility();
                createManifestation();

                spells = new BlueprintAbility[9]
                {
                    NewSpells.frost_bite,
                    library.Get<BlueprintAbility>("b6010dda6333bcf4093ce20f0063cd41"), //frigid touch
                    library.Get<BlueprintAbility>("68a9e6d7256f1354289a39003a46d826"), //stinking cloud
                    library.Get<BlueprintAbility>("fcb028205a71ee64d98175ff39a0abf9"), //ice storm
                    library.Get<BlueprintAbility>("9374c81817be79f4e92b48a4aa5ded6e"), //summon elemental large water
                    NewSpells.fluid_form, 
                    NewSpells.ice_body, 
                    library.Get<BlueprintAbility>("7ef49f184922063499b8f1346fb7f521"), //seamantle
                    library.Get<BlueprintAbility>("d8144161e352ca846a73cf90e85bf9ac"), //tsunami
                };

                beckoning_chill = hex_engine.createBeckoningChill("ShamanBeckoningChill",
                                                                  "Beckoning Chill ",
                                                                  "The shaman causes one creature within 30 feet to become more susceptible to the sapping powers of cold for 1 minute. When a creature takes cold damage while under this effect, it is entangled for 1 round. Once affected, the creature cannot be the target of this hex again for 24 hours."
                                                                  );

                crashing_waves = hex_engine.createCrashingWaves("ShamanCrashingWaves",
                                                                "Crashing Waves",
                                                                "The force of a waves shaman’s water spells can bring even the mightiest of foes to the ground. When the shaman casts a spell with the cold descriptor, she does so at 1 caster level higher. If that spell deals damage, the target must succeed at a Fortitude saving throw or be knocked prone. At 8th level, the shaman casts water spells at 2 caster levels higher. At 16th level, her ability to knock creatures prone extends to any spell that deals damage."
                                                                );

                mists_shroud = hex_engine.createMistsShroud("ShamanMistsShroud",
                                                                "Mist's Shroud",
                                                                "The shaman touches a willing creature (including herself ) and enshrouds that creature in mist. This grants the creature concealment as the blur spell. The mist dissipates after it causes an attack to miss because of concealment or after 1 minute, whichever comes first. At 8th and 16th levels, the mist lasts for one additional attack. A creature affected by this hex cannot be affected by it again for 24 hours."
                                                               );
                hexes = new BlueprintFeature[]
                {
                    beckoning_chill,
                    crashing_waves,
                    mists_shroud,
                    fluid_magic,
                };


                return new Spirit("Waves",
                                  "Waves",
                                  "A shaman who selects the waves spirit has a fluid grace that exhibits itself whenever she moves. When she calls upon one of this spirit’s abilities, floating orbs dance about her, sublimating between icy crystals, misty vapors, and globules of water.",
                                  library.Get<BlueprintAbility>("40681ea748d98f54ba7f5dc704507f39").Icon,//charged blast
                                  "",
                                  spirit_ability,
                                  greater_spirit_ability,
                                  true_spirit_ability,
                                  manifestation,
                                  hexes,
                                  spells);
            }


            static internal void createFluidMagicHex()
            {
                fluid_magic = Helpers.CreateFeature("ShamanFluidMagic",
                                                    "Fluid Magic",
                                                    "The shaman’s magic is not constrained by the reservoirs of magic that hold others back. She is able to prepare her spirit magic spells in her regular spell slots.",
                                                    "",
                                                    library.Get<BlueprintAbility>("e3f41966c2d662a4e9582a0497621c46").Icon,
                                                    FeatureGroup.None);
            }


            static void createSpiritAbility()
            {
                var resource = Helpers.CreateAbilityResource("ShamanIceSplinterResource", "", "", "", null);
                resource.SetIncreasedByStat(3, StatType.Charisma);

                var ice_splinter = library.CopyAndAdd<BlueprintAbility>("5e1db2ef80ff361448549beeb7785791", "ShamanIceSplinterAbility", ""); //water domain icicle 
                ice_splinter.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = resource);
                ice_splinter.Type = AbilityType.Supernatural;
                ice_splinter.SpellResistance = false;
                ice_splinter.SetNameDescription("Ice Splinter",
                                                "As a standard action, the shaman can shoot razor-sharp icicles at an enemy within 30 feet as a ranged touch attack. This barrage deals 1d6 points of piercing damage + 1 point for every 2 shaman levels she has."
                                                );
                ice_splinter.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", getShamanArray()));

                var frost = library.Get<BlueprintWeaponEnchantment>("421e54078b7719d40915ce0672511d0b");

                var frost_weapon_feature = Helpers.CreateFeature("ShamanIceSplinterFrostWeaponFeature",
                                                              "",
                                                              "",
                                                              "",
                                                              null,
                                                              FeatureGroup.None,
                                                              Helpers.Create<NewMechanics.EnchantmentMechanics.PersistentWeaponEnchantment>(p => p.enchant = frost)
                                                              );

                frost_weapon_feature.HideInCharacterSheetAndLevelUp = true;

                spirit_ability = Helpers.CreateFeature("ShamanIceSplinterFeature",
                                                       ice_splinter.Name,
                                                       ice_splinter.Description + "\nThe shaman can use this ability a number of times per day equal to 3 + her Charisma modifier. At 11th level, any weapon she wields is treated as a frost weapon.",
                                                       "",
                                                       ice_splinter.Icon,
                                                       FeatureGroup.None,
                                                       Helpers.CreateAddFact(ice_splinter),
                                                       Helpers.CreateAddAbilityResource(resource),
                                                       Helpers.CreateAddFeatureOnClassLevel(frost_weapon_feature, 11, getShamanArray())
                                                       );
            }


            static void createGreaterSpiritAbility()
            {
                var icon = library.Get<BlueprintFeature>("52292a32bb5d0ab45a86621bac2c4c9a").Icon;

                var resource = Helpers.CreateAbilityResource("ShamanFrigidBlastResource", "", "", "", null);
                resource.SetFixedResource(3);
                var cooldown_buff = Helpers.CreateBuff("ShamanFrigidBlastCooldownBuff",
                                       "Frigid Blast: Cooldown",
                                       "As a standard action, she can summon an icy blast in a 20-foot-radius burst originating from a point she can see within 30 feet. This blast deals cold damage equal to 1d6 per shaman level she has to each creature caught in the burst. Each target can attempt a Reflex saving throw to halve this damage. The shaman can use this ability three times per day, but she must wait at least 1d4 rounds between each use.",
                                       "",
                                       icon,
                                       null);
                var apply_cooldown = Common.createContextActionApplyBuff(cooldown_buff, Helpers.CreateContextDuration(0, diceType: DiceType.D4, diceCount: 1), dispellable: false);
                var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Cold, Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.Default)), true, true);
                var frigid_blast = Helpers.CreateAbility("ShamanFrigidBlastAbility",
                                                         "Frigid Blast",
                                                         cooldown_buff.Description,
                                                         "",
                                                         icon,
                                                         AbilityType.Supernatural,
                                                         CommandType.Standard,
                                                         AbilityRange.Close,
                                                         "",
                                                         Helpers.reflexHalfDamage,
                                                         Helpers.CreateRunActions(SavingThrowType.Reflex, dmg),
                                                         Helpers.CreateAbilityTargetsAround(20.Feet(), TargetType.Any),
                                                         Common.createAbilitySpawnFx("b1e9a6f1066c22d45adc8ad0701b5b70", anchor: AbilitySpawnFxAnchor.ClickedTarget),
                                                         Helpers.CreateSpellDescriptor(SpellDescriptor.Cold),
                                                         Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionOnContextCaster(apply_cooldown))),
                                                         Helpers.CreateResourceLogic(resource),
                                                         Common.createContextCalculateAbilityParamsBasedOnClass(shaman_class, StatType.Wisdom),
                                                         Common.createAbilityCasterHasNoFacts(cooldown_buff)
                                                         );
                frigid_blast.setMiscAbilityParametersRangedDirectional();

                greater_spirit_ability = Helpers.CreateFeature("ShamanFrigidBlastFeature",
                                                               frigid_blast.Name,
                                                               "The shaman gains cold resistance 10. In addition, as a standard action, she can summon an icy blast in a 20-foot-radius burst originating from a point she can see within 30 feet. This blast deals cold damage equal to 1d6 per shaman level she has to each creature caught in the burst. Each target can attempt a Reflex saving throw to halve this damage. The shaman can use this ability three times per day, but she must wait at least 1d4 rounds between each use.",
                                                               "",
                                                               frigid_blast.Icon,
                                                               FeatureGroup.None,
                                                               Helpers.CreateAddFact(frigid_blast),
                                                               Helpers.CreateAddAbilityResource(resource),
                                                               Common.createEnergyDR(10, DamageEnergyType.Cold)
                                                               );
            }


            static void createTrueSpiritAbility()
            {
                var resource = Helpers.CreateAbilityResource("ShamanWavesElementalFormResource", "", "", "", null);
                resource.SetFixedResource(1);

                var wildshape_water_elemental_freeze = library.CopyAndAdd<BlueprintFeature>("182ec5f31231ad24b96a84a3f9e87166", "ShamanWavesElementalFormFreezeFeature", "");
                wildshape_water_elemental_freeze.ReplaceComponent<ContextCalculateAbilityParamsBasedOnClass>(c => c.CharacterClass = shaman_class);

                var buff = library.CopyAndAdd<BlueprintBuff>("ea2cd08bdf2ca1c4f8a8870804790cd7", "ShamanWavesElementalFormBuff", "");
                buff.SetName("Elemental Form (Huge Water Elemental)");
                buff.ReplaceComponent<Polymorph>(p => p.Facts = new BlueprintUnitFact[] { p.Facts[0], wildshape_water_elemental_freeze });

                var ability = library.CopyAndAdd<BlueprintAbility>("621cc9c46f5961b47adda27791e41f75", "ShamanWavesElementalFormAbility", "");
                ability.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", getShamanArray()));
                ability.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = resource);
                ability.ReplaceComponent<AbilityTargetHasFact>(a => a.CheckedFacts = new BlueprintUnitFact[] { buff });
                ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(Common.changeAction<ContextActionApplyBuff>(a.Actions.Actions,
                                                                                                                                                       b => b.Buff = buff)
                                                                                                          )
                                                                );
                ability.SetName("Elemental Form (Huge Water Elemental)");
                true_spirit_ability = Common.AbilityToFeature(ability, false);
                true_spirit_ability.SetDescription(Helpers.CreateString(true_spirit_ability.name + "2.Description", "As a standard action, the shaman assumes the form of a Huge water elemental, as elemental body IV with a duration of 1 hour per level. The shaman can use this ability once per day."));
                true_spirit_ability.AddComponent(Helpers.CreateAddAbilityResource(resource));
            }


            static void createManifestation()
            {
                manifestation = Helpers.CreateFeature("ShamanWavesManifestationFeature",
                                                      "Manifestation",
                                                      "Upon reaching 20th level, the shaman becomes a spirit of water. The shaman gains cold resistance 30. She can also apply any one of the following feats to any cold spell she casts without increasing the spell’s level or casting time: Reach Spell, Extend Spell. She doesn’t need to possess these feats to use this ability.",
                                                      "",
                                                      library.Get<BlueprintProgression>("7c692e90592257a4e901d12ae6ec1e41").Icon, //cold wall
                                                      FeatureGroup.None,
                                                      Common.createEnergyDR(30, DamageEnergyType.Cold));

                var extend = Common.CreateMetamagicAbility(manifestation, "Extend", "Extend Spell (Cold)", Kingmaker.UnitLogic.Abilities.Metamagic.Extend, SpellDescriptor.Cold, "", "");
                extend.Group = ActivatableAbilityGroupExtension.ShamanWavesMetamagic.ToActivatableAbilityGroup();
                var reach = Common.CreateMetamagicAbility(manifestation, "Reach", "Reach Spell (Cold)", Kingmaker.UnitLogic.Abilities.Metamagic.Reach, SpellDescriptor.Cold, "", "");
                reach.Group = ActivatableAbilityGroupExtension.ShamanWavesMetamagic.ToActivatableAbilityGroup();
                manifestation.AddComponent(Helpers.CreateAddFacts(extend, reach));
            }

        }
    }
}
