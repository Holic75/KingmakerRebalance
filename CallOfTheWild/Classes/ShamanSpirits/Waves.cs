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
    partial class SpiritsEngine
    {
        public class WavesSpirit
        {
            public BlueprintFeature spirit_ability;
            public BlueprintFeature greater_spirit_ability;
            public BlueprintFeature true_spirit_ability;
            public BlueprintFeature manifestation;
            public BlueprintFeature fluid_magic;
            public BlueprintFeature beckoning_chill;
            public BlueprintFeature mists_shroud;
            public BlueprintFeature crashing_waves;
            public BlueprintAbility[] spells;
            public BlueprintFeature[] hexes;
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
                    NewSpells.frost_bite,
                    library.Get<BlueprintAbility>("b6010dda6333bcf4093ce20f0063cd41"), //frigid touch
                    NewSpells.sleet_storm,
                    library.Get<BlueprintAbility>("fcb028205a71ee64d98175ff39a0abf9"), //ice storm
                    library.Get<BlueprintAbility>("65e8d23aef5e7784dbeb27b1fca40931"), //icy prison
                    NewSpells.fluid_form,
                    NewSpells.ice_body,
                    library.Get<BlueprintAbility>("7ef49f184922063499b8f1346fb7f521"), //seamantle
                    library.Get<BlueprintAbility>("d8144161e352ca846a73cf90e85bf9ac"), //tsunami
                };


                return new Oracle.Spirit("Waves",
                                         "Waves",
                                         "A shaman who selects the waves spirit has a fluid grace that exhibits itself whenever she moves. When she calls upon one of this spirit’s abilities, floating orbs dance about her, sublimating between icy crystals, misty vapors, and globules of water.",
                                         library.Get<BlueprintAbility>("40681ea748d98f54ba7f5dc704507f39").Icon,//charged blast
                                         "",
                                         spirit_ability,
                                         greater_spirit_ability,
                                         spells,
                                         hexes.Skip(1).ToArray()
                                         );
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


                return new Archetypes.SpiritWhisperer.Spirit("Waves",
                                                              "Waves",
                                                              "A shaman who selects the waves spirit has a fluid grace that exhibits itself whenever she moves. When she calls upon one of this spirit’s abilities, floating orbs dance about her, sublimating between icy crystals, misty vapors, and globules of water.",
                                                              library.Get<BlueprintAbility>("40681ea748d98f54ba7f5dc704507f39").Icon,//charged blast
                                                              "",
                                                              spirit_ability,
                                                              greater_spirit_ability,
                                                              manifestation,
                                                              hexes.Skip(1).ToArray());
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
                    NewSpells.frost_bite,
                    library.Get<BlueprintAbility>("b6010dda6333bcf4093ce20f0063cd41"), //frigid touch
                    NewSpells.sleet_storm,
                    library.Get<BlueprintAbility>("fcb028205a71ee64d98175ff39a0abf9"), //ice storm
                    library.Get<BlueprintAbility>("65e8d23aef5e7784dbeb27b1fca40931"), //icy prison
                    NewSpells.fluid_form, 
                    NewSpells.ice_body, 
                    library.Get<BlueprintAbility>("7ef49f184922063499b8f1346fb7f521"), //seamantle
                    library.Get<BlueprintAbility>("d8144161e352ca846a73cf90e85bf9ac"), //tsunami
                };


                return new Shaman.Spirit("Waves",
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


            void createHexes()
            {
                createFluidMagicHex();
                beckoning_chill = hex_engine.createBeckoningChill(prefix + "BeckoningChill",
                                                                  "Beckoning Chill ",
                                                                  "The shaman causes one creature within 30 feet to become more susceptible to the sapping powers of cold for 1 minute. When a creature takes cold damage while under this effect, it is entangled for 1 round. Once affected, the creature cannot be the target of this hex again for 24 hours."
                                                                  );

                crashing_waves = hex_engine.createCrashingWaves(prefix + "CrashingWaves",
                                                                "Crashing Waves",
                                                                "The force of a waves shaman’s water spells can bring even the mightiest of foes to the ground. When the shaman casts a spell with the water descriptor, she does so at 1 caster level higher. If that spell deals damage, the target must succeed at a Fortitude saving throw or be knocked prone. At 8th level, the shaman casts cold spells at 2 caster levels higher. At 16th level, her ability to knock creatures prone extends to any spell that deals damage."
                                                                );

                mists_shroud = hex_engine.createMistsShroud(prefix + "MistsShroud",
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
            }


            public void createFluidMagicHex()
            {
                //will be empty, will need to fill it manually
                fluid_magic = Helpers.CreateFeature(prefix + "FluidMagic",
                                                    "Fluid Magic",
                                                    "The shaman’s magic is not constrained by the reservoirs of magic that hold others back. She is able to prepare her spirit magic spells in her regular spell slots.",
                                                    "",
                                                    library.Get<BlueprintAbility>("e3f41966c2d662a4e9582a0497621c46").Icon,
                                                    FeatureGroup.None);
            }


            void createSpiritAbility()
            {
                var resource = Helpers.CreateAbilityResource(prefix + "IceSplinterResource", "", "", "", null);
                resource.SetIncreasedByStat(3, secondary_stat);

                var ice_splinter = library.CopyAndAdd<BlueprintAbility>("5e1db2ef80ff361448549beeb7785791", prefix + "IceSplinterAbility", ""); //water domain icicle 
                ice_splinter.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = resource);
                ice_splinter.Type = AbilityType.Supernatural;
                ice_splinter.SpellResistance = false;
                ice_splinter.SetNameDescription("Ice Splinter",
                                                Main.settings.balance_fixes
                                                ? "As a standard action, the shaman can shoot razor-sharp icicles at an enemy within 30 feet as a ranged touch attack. This barrage deals 1d8 points of cold damage plus 1d8 points for every 2 shaman levels she has beyond first.\nThe shaman can use this ability a number of times per day equal to 3 + her Charisma modifier."
                                                : "As a standard action, the shaman can shoot razor-sharp icicles at an enemy within 30 feet as a ranged touch attack. This barrage deals 1d6 points of cold damage plus 1 point for every 2 shaman levels she has.\nThe shaman can use this ability a number of times per day equal to 3 + her Charisma modifier."
                                                );
                ice_splinter.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", hex_engine.hex_classes));

                var frost = library.Get<BlueprintWeaponEnchantment>("421e54078b7719d40915ce0672511d0b");

                var frost_weapon_feature = Helpers.CreateFeature(prefix + "IceSplinterFrostWeaponFeature",
                                                              "",
                                                              "",
                                                              "",
                                                              null,
                                                              FeatureGroup.None,
                                                              Helpers.Create<NewMechanics.EnchantmentMechanics.PersistentWeaponEnchantment>(p => p.enchant = frost)
                                                              );

                frost_weapon_feature.HideInCharacterSheetAndLevelUp = true;

                spirit_ability = Helpers.CreateFeature(prefix + "IceSplinterFeature",
                                                       ice_splinter.Name,
                                                       ice_splinter.Description + " At 11th level, any weapon she wields is treated as a frost weapon.",
                                                       "",
                                                       ice_splinter.Icon,
                                                       FeatureGroup.None,
                                                       Helpers.CreateAddFact(ice_splinter),
                                                       Helpers.CreateAddAbilityResource(resource),
                                                       Helpers.CreateAddFeatureOnClassLevel(frost_weapon_feature, 11, hex_engine.hex_classes)
                                                       );
            }


            void createGreaterSpiritAbility()
            {
                var icon = library.Get<BlueprintFeature>("52292a32bb5d0ab45a86621bac2c4c9a").Icon;

                var resource = Helpers.CreateAbilityResource(prefix + "FrigidBlastResource", "", "", "", null);
                resource.SetFixedResource(3);
                var cooldown_buff = Helpers.CreateBuff(prefix + "FrigidBlastCooldownBuff",
                                       "Frigid Blast: Cooldown",
                                       $"As a standard action, she can summon an icy blast in a 20-foot-radius burst originating from a point she can see within 30 feet. This blast deals cold damage equal to 1d{BalanceFixes.getDamageDieString(DiceType.D6)} per shaman level she has to each creature caught in the burst. Each target can attempt a Reflex saving throw to halve this damage. The shaman can use this ability three times per day, but she must wait at least 1d4 rounds between each use.",
                                       "",
                                       icon,
                                       null);
                var apply_cooldown = Common.createContextActionApplyBuff(cooldown_buff, Helpers.CreateContextDuration(0, diceType: DiceType.D4, diceCount: 1), dispellable: false);
                var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Cold, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.Default)), true, true);
                var frigid_blast = Helpers.CreateAbility(prefix + "FrigidBlastAbility",
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
                                                         Common.createContextCalculateAbilityParamsBasedOnClasses(hex_engine.hex_classes, primary_stat),
                                                         Common.createAbilityCasterHasNoFacts(cooldown_buff)
                                                         );
                frigid_blast.setMiscAbilityParametersRangedDirectional();

                greater_spirit_ability = Helpers.CreateFeature(prefix + "FrigidBlastFeature",
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


            void createTrueSpiritAbility()
            {
                var resource = Helpers.CreateAbilityResource(prefix + "WavesElementalFormResource", "", "", "", null);
                resource.SetFixedResource(1);

                var wildshape_water_elemental_freeze = library.CopyAndAdd<BlueprintFeature>("182ec5f31231ad24b96a84a3f9e87166", prefix + "WavesElementalFormFreezeFeature", "");
                wildshape_water_elemental_freeze.ReplaceComponent<ContextCalculateAbilityParamsBasedOnClass>(c => c.CharacterClass = hex_engine.hex_classes[0]);

                var buff = library.CopyAndAdd<BlueprintBuff>("ea2cd08bdf2ca1c4f8a8870804790cd7", prefix + "WavesElementalFormBuff", "");
                buff.SetName("Elemental Form (Huge Water Elemental)");
                buff.ReplaceComponent<Polymorph>(p => p.Facts = new BlueprintUnitFact[] { p.Facts[0], wildshape_water_elemental_freeze });

                var ability = library.CopyAndAdd<BlueprintAbility>("621cc9c46f5961b47adda27791e41f75", prefix + "WavesElementalFormAbility", "");
                ability.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", hex_engine.hex_classes));
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


            void createManifestation()
            {
                manifestation = Helpers.CreateFeature(prefix + "WavesManifestationFeature",
                                                      "Manifestation",
                                                      "Upon reaching 20th level, the shaman becomes a spirit of water. The shaman gains cold resistance 30. She can also apply any one of the following feats to any cold or water spell she casts without increasing the spell’s level or casting time: Reach Spell, Extend Spell. She doesn’t need to possess these feats to use this ability.",
                                                      "",
                                                      library.Get<BlueprintProgression>("7c692e90592257a4e901d12ae6ec1e41").Icon, //cold wall
                                                      FeatureGroup.None,
                                                      Common.createEnergyDR(30, DamageEnergyType.Cold));

                var extend = Common.CreateMetamagicAbility(manifestation, "Extend", "Extend Spell (Cold)", Kingmaker.UnitLogic.Abilities.Metamagic.Extend, SpellDescriptor.Cold | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Water, "", "");
                extend.Group = ActivatableAbilityGroupExtension.ShamanWavesMetamagic.ToActivatableAbilityGroup();
                var reach = Common.CreateMetamagicAbility(manifestation, "Reach", "Reach Spell (Cold)", Kingmaker.UnitLogic.Abilities.Metamagic.Reach, SpellDescriptor.Cold | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Water, "", "");
                reach.Group = ActivatableAbilityGroupExtension.ShamanWavesMetamagic.ToActivatableAbilityGroup();
                manifestation.AddComponent(Helpers.CreateAddFacts(extend, reach));
            }

        }
    }
}
