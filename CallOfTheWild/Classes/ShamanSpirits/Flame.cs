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
    public partial class SpiritsEngine
    {
        public class FlameSpirit
        {
            public BlueprintFeature spirit_ability;
            public BlueprintFeature greater_spirit_ability;
            public BlueprintFeature true_spirit_ability;
            public BlueprintFeature manifestation;
            public BlueprintFeature cinder_dance;
            public BlueprintFeature fire_nimbus;
            public BlueprintFeature flame_curse;
            public BlueprintFeature ward_of_flames;
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
                    library.Get<BlueprintAbility>("4783c3709a74a794dbe7c8e7e0b1b038"), //burning hands
                    library.Get<BlueprintAbility>("21ffef7791ce73f468b6fca4d9371e8b"), //resist energy
                    library.Get<BlueprintAbility>("2d81362af43aeac4387a3d4fced489c3"), //fireball
                    NewSpells.wall_of_fire,
                    library.Get<BlueprintAbility>("b3a203742191449458d2544b3f442194"), //summon elemental large fire
                    NewSpells.fire_seeds,
                    SpellDuplicates.addDuplicateSpell("e3d0dfe1c8527934294f241e0ae96a8d", "FireStormShamanFlameSpiritAbility"),//firestorm
                    NewSpells.incendiary_cloud,
                    library.Get<BlueprintAbility>("08ccad78cac525040919d51963f9ac39"), //fiery body
                };
                return new Oracle.Spirit("Flame",
                                         "Flame",
                                         "A shaman who selects the flame spirit has a radiant light behind her eyes and the faint smell of smoke about her. When she calls upon one of this spirit’s abilities, a hungry spectral flame dances around her body.",
                                         library.Get<BlueprintAbility>("19309b5551a28d74288f4b6f7d8d838d").Icon, //wall of fire
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

                return new Archetypes.SpiritWhisperer.Spirit("Flame",
                                                            "Flame",
                                                            "A shaman who selects the flame spirit has a radiant light behind her eyes and the faint smell of smoke about her. When she calls upon one of this spirit’s abilities, a hungry spectral flame dances around her body.",
                                                            library.Get<BlueprintAbility>("19309b5551a28d74288f4b6f7d8d838d").Icon, //wall of fire
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
                    library.Get<BlueprintAbility>("4783c3709a74a794dbe7c8e7e0b1b038"), //burning hands
                    library.Get<BlueprintAbility>("21ffef7791ce73f468b6fca4d9371e8b"), //resist energy
                    library.Get<BlueprintAbility>("2d81362af43aeac4387a3d4fced489c3"), //fireball
                    NewSpells.wall_of_fire_fire_domain,
                    library.Get<BlueprintAbility>("b3a203742191449458d2544b3f442194"), //summon elemental large fire
                    NewSpells.fire_seeds,
                    SpellDuplicates.addDuplicateSpell("e3d0dfe1c8527934294f241e0ae96a8d", "FireStormShamanFlameSpiritAbility"),
                    NewSpells.incendiary_cloud,
                    library.Get<BlueprintAbility>("08ccad78cac525040919d51963f9ac39"), //fiery body
                };




                return new Shaman.Spirit("Flame",
                                  "Flame",
                                  "A shaman who selects the flame spirit has a radiant light behind her eyes and the faint smell of smoke about her. When she calls upon one of this spirit’s abilities, a hungry spectral flame dances around her body.",
                                  library.Get<BlueprintAbility>("19309b5551a28d74288f4b6f7d8d838d").Icon, //wall of fire
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
                createCinderDanceHex();
                fire_nimbus = hex_engine.createFireNimbus(prefix + "FireNimbus",
                                            "Fire Nimbus",
                                            "The shaman causes a creature within 30 feet to gain a nimbus of fire. Though this doesn’t harm the creature, it does cause the creature to emit light like a torch, preventing it from gaining any benefit from concealment or invisibility. The target also takes a –2 penalty on saving throws against spells or effects that deal fire damage. The fire nimbus lasts for a number of rounds equal to the shaman’s level. A successful Will saving throw negates this effect. Whether or not the save is successful, the creature cannot be the target of this hex again for 24 hours."
                                            );

                flame_curse = hex_engine.createFlameCurse(prefix + "FlameCurse",
                                                        "Flame Curse",
                                                        "The shaman causes a creature within 30 feet to become vulnerable to fire until the end of the shaman’s next turn. If the creature is already vulnerable to fire, this hex has no effect. Fire immunity and resistances apply as normal, and any saving throw allowed by the effect that caused the damage reduces it as normal. At 8th and 16th levels, the duration of this hex is extended by 1 round. A creature affected by this hex cannot be affected by it again for 24 hours."
                                                        );

                ward_of_flames = hex_engine.createFlameWardHex(prefix + "FlameWard",
                                                                "Flame Ward",
                                                                $"The shaman touches a willing creature (including herself) and grants a ward of flames. The next time the warded creature is struck with a melee attack, the creature making the attack takes 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of fire damage + 1 point of fire damage for every 2 shaman levels she possesses. This ward lasts for 1 minute, after which it fades away if not already expended. At 8th and 16th levels, the ward lasts for one additional attack. A creature affected by this hex cannot be affected by it again for 24 hours."
                                                               );
                hexes = new BlueprintFeature[]
                {
                    cinder_dance,
                    fire_nimbus,
                    flame_curse,
                    ward_of_flames,
                };
            }

            public void createCinderDanceHex()
            {
                var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/CinderDance.png");
                var woodland_stride = library.CopyAndAdd<BlueprintFeature>("11f4072ea766a5840a46e6660894527d", prefix + "CinderDanceHex2Feature", "");
                woodland_stride.HideInCharacterSheetAndLevelUp = true;
                woodland_stride.SetNameDescriptionIcon("", "", null);

                cinder_dance = Helpers.CreateFeature(prefix + "CinderDanceHexFeature",
                                                     "Cinder Dance",
                                                     "Your base speed increases by 10 feet. At 10th level, you can ignore difficult terrain when moving.",
                                                     "",
                                                     icon,
                                                     FeatureGroup.None,
                                                     Helpers.CreateAddStatBonus(StatType.Speed, 10, ModifierDescriptor.UntypedStackable),
                                                     Helpers.CreateAddFeatureOnClassLevel(woodland_stride, 10, hex_engine.hex_classes)
                                                     );
            }


            void createSpiritAbility()
            {
                var icon = library.Get<BlueprintAbility>("4783c3709a74a794dbe7c8e7e0b1b038").Icon; //burning hands
                var resource = Helpers.CreateAbilityResource(prefix + "TouchOfFlamesResource", "", "", "", null);
                resource.SetIncreasedByStat(3, secondary_stat);
                var touch_of_flames = library.CopyAndAdd<BlueprintAbility>("4ecdf240d81533f47a5279f5075296b9", prefix + "TouchOfFlamesAbility", ""); //fire domain fire bolt 
                touch_of_flames.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Fire));
                touch_of_flames.RemoveComponents<SpellComponent>();
                touch_of_flames.RemoveComponents<AbilityResourceLogic>();
                touch_of_flames.ReplaceComponent<AbilityDeliverProjectile>(Helpers.CreateDeliverTouch());
                touch_of_flames.AddComponent(Common.createAbilitySpawnFx("316c51cc84afcfc4facc9ada44ff47c1", anchor: AbilitySpawnFxAnchor.SelectedTarget));
                touch_of_flames.setMiscAbilityParametersTouchHarmful();
                touch_of_flames.Range = AbilityRange.Touch;
                touch_of_flames.Type = AbilityType.Supernatural;
                touch_of_flames.SpellResistance = false;
                touch_of_flames.SetNameDescriptionIcon("Touch of Flames",
                                                       Main.settings.balance_fixes 
                                                       ? "As a standard action, the shaman can make a melee touch attack that deals 1d8 points of fire damage plus 1d8 points for every 2 shaman levels she possesses beyond first. A shaman can use this ability a number of times per day equal to 3 + her Charisma modifier."
                                                       : "As a standard action, the shaman can make a melee touch attack that deals 1d6 points of fire damage plus 1 point for every 2 shaman levels she possesses. A shaman can use this ability a number of times per day equal to 3 + her Charisma modifier.",
                                                       icon);
                touch_of_flames.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", hex_engine.hex_classes));
                var touch_of_flames_sticky = Helpers.CreateTouchSpellCast(touch_of_flames, resource);
                var flaming = library.Get<BlueprintWeaponEnchantment>("30f90becaaac51f41bf56641966c4121");

                var flaming_weapon_feature = Helpers.CreateFeature(prefix + "TouchOfFlamesFlamingWeaponFeature",
                                                              "",
                                                              "",
                                                              "",
                                                              null,
                                                              FeatureGroup.None,
                                                              Helpers.Create<NewMechanics.EnchantmentMechanics.PersistentWeaponEnchantment>(p => p.enchant = flaming)
                                                              );

                flaming_weapon_feature.HideInCharacterSheetAndLevelUp = true;

                spirit_ability = Helpers.CreateFeature(prefix + "TouchOfFlameFeature",
                                                       touch_of_flames.Name,
                                                       touch_of_flames.Description + " At 11th level, any weapon she wields is treated as a flaming weapon.",
                                                       "",
                                                       touch_of_flames.Icon,
                                                       FeatureGroup.None,
                                                       Helpers.CreateAddFact(touch_of_flames_sticky),
                                                       Helpers.CreateAddAbilityResource(resource),
                                                       Helpers.CreateAddFeatureOnClassLevel(flaming_weapon_feature, 11, hex_engine.hex_classes)
                                                       );
            }


            void createGreaterSpiritAbility()
            {
                var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/FierySoul.png");

                var resource = Helpers.CreateAbilityResource(prefix + "FierySoulResource", "", "", "", null);
                resource.SetFixedResource(3);
                var cooldown_buff = Helpers.CreateBuff(prefix + "FierySoulCooldownBuff",
                                       "Fiery Soul: Cooldown",
                                       "As a standard action she can unleash a 15-foot cone of flame from her mouth, dealing 1d4 points of fire damage per shaman level she possesses. A successful Reflex saving throw halves this damage. The shaman can use this ability three times per day, but she must wait 1d4 rounds between each use.",
                                       "",
                                       icon,
                                       null);
                var apply_cooldown = Common.createContextActionApplyBuff(cooldown_buff, Helpers.CreateContextDuration(0, diceType: DiceType.D4, diceCount: 1), dispellable: false);
                var fiery_soul = library.CopyAndAdd<BlueprintAbility>("4783c3709a74a794dbe7c8e7e0b1b038", prefix + "FierySoulAbility", "");
                fiery_soul.Type = AbilityType.Supernatural;
                fiery_soul.SpellResistance = false;
                fiery_soul.RemoveComponents<SpellComponent>();
                fiery_soul.RemoveComponents<SpellListComponent>();
                fiery_soul.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: hex_engine.hex_classes));
                fiery_soul.AddComponents(Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionOnContextCaster(apply_cooldown))),
                                         Helpers.CreateResourceLogic(resource),
                                         Common.createContextCalculateAbilityParamsBasedOnClass(hex_engine.hex_classes[0], primary_stat),
                                         Common.createAbilityCasterHasNoFacts(cooldown_buff)
                                        );
                fiery_soul.SetNameDescriptionIcon("Fiery Soul", cooldown_buff.Description, cooldown_buff.Icon);
                greater_spirit_ability = Helpers.CreateFeature(prefix + "FierySoulFeature",
                                                               fiery_soul.Name,
                                                               "The shaman gains fire resistance 10. In addition, as a standard action she can unleash a 15-foot cone of flame from her mouth, dealing 1d4 points of fire damage per shaman level she possesses. A successful Reflex saving throw halves this damage. The shaman can use this ability three times per day, but she must wait 1d4 rounds between each use.",
                                                               "",
                                                               fiery_soul.Icon,
                                                               FeatureGroup.None,
                                                               Helpers.CreateAddFact(fiery_soul),
                                                               Helpers.CreateAddAbilityResource(resource),
                                                               Common.createEnergyDR(10, DamageEnergyType.Fire)
                                                               );
            }


            void createTrueSpiritAbility()
            {
                var resource = Helpers.CreateAbilityResource(prefix + "FlameElementalFormResource", "", "", "", null);
                resource.SetFixedResource(1);

                var wildshape_fire_elemental_burn = library.CopyAndAdd<BlueprintFeature>("9320bbafe4e535b4eb37ff5c6eaff0ed", prefix + "FlamesElementalFormBurnFeature", "");
                wildshape_fire_elemental_burn.ReplaceComponent<ContextCalculateAbilityParamsBasedOnClass>(c => c.CharacterClass = hex_engine.hex_classes[0]);

                var buff = library.CopyAndAdd<BlueprintBuff>("e85abd773dbce30498efa8da745d7ca7", prefix + "FlamesElementalFormBuff", "");
                buff.SetName("Elemental Form (Huge Fire Elemental)");
                buff.ReplaceComponent<Polymorph>(p => p.Facts = new BlueprintUnitFact[] { p.Facts[0], wildshape_fire_elemental_burn });

                var ability = library.CopyAndAdd<BlueprintAbility>("90aa5552c8db06045b1303de6ec7b627", prefix + "FlamesElementalFormAbility", "");
                ability.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", hex_engine.hex_classes));
                ability.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = resource);
                ability.ReplaceComponent<AbilityTargetHasFact>(a => a.CheckedFacts = new BlueprintUnitFact[] { buff });
                ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(Common.changeAction<ContextActionApplyBuff>(a.Actions.Actions,
                                                                                                                                                       b => b.Buff = buff)
                                                                                                          )
                                                                );
                ability.SetName("Elemental Form (Huge Fire Elemental)");
                true_spirit_ability = Common.AbilityToFeature(ability, false);
                true_spirit_ability.SetDescription(Helpers.CreateString(true_spirit_ability.name + "2.Description", "As a standard action, the shaman assumes the form of a Huge fire elemental, as if using elemental body IV with a duration of 1 hour per level. The shaman can use this ability once per day."));
                true_spirit_ability.AddComponent(Helpers.CreateAddAbilityResource(resource));
            }


            void createManifestation()
            {
                manifestation = Helpers.CreateFeature(prefix + "FlamesManifestationFeature",
                                                      "Manifestation",
                                                      "Upon reaching 20th level, the shaman becomes a spirit of flame. The shaman gains fire resistance 30. She can also apply any one of the following feats to any fire spell she casts without increasing the spell’s level or casting time: Reach Spell, Extend Spell. She doesn’t need to possess these feats to use this ability.",
                                                      "",
                                                      library.Get<BlueprintProgression>("17cc794d47408bc4986c55265475c06f").Icon,
                                                      FeatureGroup.None,
                                                      Common.createEnergyDR(30, DamageEnergyType.Fire));

               var extend = Common.CreateMetamagicAbility(manifestation, "Extend", "Extend Spell (Fire)", Kingmaker.UnitLogic.Abilities.Metamagic.Extend, SpellDescriptor.Fire, "", "");
               extend.Group = ActivatableAbilityGroupExtension.ShamanFlamesMetamagic.ToActivatableAbilityGroup();
               var reach = Common.CreateMetamagicAbility(manifestation, "Reach", "Reach Spell (Fire)", Kingmaker.UnitLogic.Abilities.Metamagic.Reach, SpellDescriptor.Fire, "", "");
               reach.Group = ActivatableAbilityGroupExtension.ShamanFlamesMetamagic.ToActivatableAbilityGroup();
               manifestation.AddComponent(Helpers.CreateAddFacts(extend, reach));
            }


        }


    }
}
