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
    partial class Shaman
    {
        internal class WindSpirit
        {
            internal static BlueprintFeature spirit_ability;
            internal static BlueprintFeature greater_spirit_ability;
            internal static BlueprintFeature true_spirit_ability;
            internal static BlueprintFeature manifestation;
            internal static BlueprintFeature air_barrier;
            internal static BlueprintFeature vortex_spells;
            internal static BlueprintFeature sparkling_aura;
            internal static BlueprintFeature wind_ward;
            internal static BlueprintAbility[] spells;
            internal static BlueprintFeature[] hexes;

            internal static Spirit create()
            {
                createSpiritAbility();
                createGreaterSpiritAbility();
                createTrueSpiritAbility();
                createManifestation();

                spells = new BlueprintAbility[9]
                {
                    library.Get<BlueprintAbility>("ab395d2335d3f384e99dddee8562978f"), //shocking grasp
                    library.Get<BlueprintAbility>("c28de1f98a3f432448e52e5d47c73208"), //protection from arrows
                    library.Get<BlueprintAbility>("d2cff9243a7ee804cb6d5be47af30c73"), //lightning bolt
                    NewSpells.river_of_wind, 
                    library.Get<BlueprintAbility>("16fff43f034133a4a86e914a523e021f"), //summon elemental large air
                    library.Get<BlueprintAbility>("093ed1d67a539ad4c939d9d05cfe192c"), //sirocco
                    library.Get<BlueprintAbility>("ee63301f83c76694692d4704d8a05bdc"), //elemental body IV air
                    library.Get<BlueprintAbility>("333efbf776ab61c4da53e9622751d95f"), //summon elemental elder air
                    NewSpells.winds_of_vengeance
                };

                air_barrier = hex_engine.createAirBarrier("ShamanAirBarrier",
                                                            "Air Barrier",
                                                            "The shaman creates an invisible shell of air that grants her a +4 armor bonus to AC. At 7th level and every 4 levels thereafter, this bonus increases by 2. At 13th level, this barrier causes incoming arrows, rays, and other ranged attacks requiring an attack roll against her to suffer a 50% miss chance. The shaman can use this barrier for 1 hour per shaman level. This duration does not need to be consecutive, but it must be spent in 1-hour increments."
                                                            );

                sparkling_aura = hex_engine.createBoneWard("ShamanSparklingAura",
                                                        "Sparkling Aura",
                                                        "The shaman causes a creature within 30 feet to spark and shimmer with electrical energy. Though this does not harm the creature, it does cause the creature to emit light like a torch, preventing it from gaining any benefit from concealment or invisibility. Furthermore, while the aura lasts, whenever the target is hit with a metal melee weapon, it also takes an amount of electricity damage equal to the shaman’s Charisma modifier. The sparking aura lasts a 1 round for every 2 shaman levels the shaman possesses. A creature affected by this hex cannot be affected by it again for 24 hours."
                                                        );

                vortex_spells = hex_engine.createFearfulGaze("ShamanVortexSpells",
                                                                "Vortex Spells",
                                                                "Whenever the shaman confirms a critical hit against an opponent with a spell, the target is staggered for 1 round. At 11th level, the duration increases to 1d4 rounds."
                                                               );

                wind_ward = hex_engine.createFearfulGaze("ShamanWindWard",
                                                "Wind Ward",
                                                "The shaman can touch a willing creature (including herself ) and grants a ward of wind. This ward lasts for a number of rounds equal to the shaman’s level. When a warded creature is attacked with an arrow, ray, or other ranged attack that requires an attack roll, that attack suffers a 20% miss chance. At 8th level, the ward lasts for 1 minute for every level the shaman possesses. At 16th level, the miss chance increases to 50%. Once affected, the creature cannot be the target of this hex again for 24 hours."
                                               );
                hexes = new BlueprintFeature[]
                {
                    air_barrier,
                    sparkling_aura,
                    vortex_spells,
                    wind_ward
                };


                return new Spirit("Wind",
                                  "Wind",
                                  "A shaman who selects the wind spirit appears windswept, and her movements seem lithe and carefree.",
                                  library.Get<BlueprintAbility>("093ed1d67a539ad4c939d9d05cfe192c").Icon,//sirocco
                                  "",
                                  spirit_ability,
                                  greater_spirit_ability,
                                  true_spirit_ability,
                                  manifestation,
                                  hexes,
                                  spells);
            }


            static void createSpiritAbility()
            {
                var icon = library.Get<BlueprintAbility>("ab395d2335d3f384e99dddee8562978f").Icon; //shocking grasp
                var resource = Helpers.CreateAbilityResource("ShamanShockingTOuchResource", "", "", "", null);
                resource.SetIncreasedByStat(3, StatType.Charisma);
                var shocking_touch = library.CopyAndAdd<BlueprintAbility>("b3494639791901e4db3eda6117ad878f", "ShamanShockingTouchAbility", ""); //air domain arck of lightning 
                shocking_touch.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Electricity));
                shocking_touch.RemoveComponents<SpellComponent>();
                shocking_touch.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = resource);
                shocking_touch.ReplaceComponent<AbilityDeliverProjectile>(Helpers.CreateDeliverTouch());
                shocking_touch.setMiscAbilityParametersTouchHarmful();
                shocking_touch.Type = AbilityType.Supernatural;
                shocking_touch.SpellResistance = false;
                shocking_touch.SetNameDescriptionIcon("Shocking Touch",
                                                       "As a standard action, the shaman can make a melee touch attack that deals 1d6 points of electricity damage + 1 point for every 2 shaman levels she possesses. A shaman can use this ability a number of times per day equal to 3 + her Charisma modifier.",
                                                       icon);
                shocking_touch.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", getShamanArray()));
                var shocking = library.Get<BlueprintWeaponEnchantment>("7bda5277d36ad114f9f9fd21d0dab658");

                var shocking_weapon_feature = Helpers.CreateFeature("ShamanShockingTouchShockingWeaponFeature",
                                                              "",
                                                              "",
                                                              "",
                                                              null,
                                                              FeatureGroup.None,
                                                              Helpers.Create<NewMechanics.EnchantmentMechanics.PersistentWeaponEnchantment>(p => p.enchant = shocking)
                                                              );

                shocking_weapon_feature.HideInCharacterSheetAndLevelUp = true;

                spirit_ability = Helpers.CreateFeature("ShamanShockingTouchFeature",
                                                       shocking_touch.Name,
                                                       "As a standard action, the shaman can make a melee touch attack that deals 1d6 points of electricity damage + 1 point for every 2 shaman levels she possesses. A shaman can use this ability a number of times per day equal to 3 + her Charisma modifier. At 11th level, any weapon she wields is treated as a shocking weapon.",
                                                       "",
                                                       shocking_touch.Icon,
                                                       FeatureGroup.None,
                                                       Helpers.CreateAddFact(shocking_touch),
                                                       Helpers.CreateAddAbilityResource(resource),
                                                       Helpers.CreateAddFeatureOnClassLevel(shocking_weapon_feature, 11, getShamanArray())
                                                       );
            }


            static void createGreaterSpiritAbility()
            {
                var icon = library.Get<BlueprintAbility>("d2cff9243a7ee804cb6d5be47af30c73").Icon; //lightning bolt

                var resource = Helpers.CreateAbilityResource("ShamanSparkSoulResource", "", "", "", null);
                resource.SetFixedResource(3);
                var cooldown_buff = Helpers.CreateBuff("ShamanSparkSoulCooldownBuff",
                                       "Spark Soul: Cooldown",
                                       "In addition, as a standard action she can unleash a 30-foot line of sparks from her fingertips, dealing 1d4 points of electricity damage per shaman level she possesses. A successful Reflex saving throw halves this damage. The shaman can use this ability three times per day, but she must wait 1d4 rounds between each use.",
                                       "",
                                       icon,
                                       null);
                var apply_cooldown = Common.createContextActionApplyBuff(cooldown_buff, Helpers.CreateContextDuration(0, diceType: DiceType.D4, diceCount: 1), dispellable: false);

                var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Electricity, Helpers.CreateContextDiceValue(DiceType.D4, Helpers.CreateContextValue(AbilityRankType.Default)),
                                                         isAoE: true, halfIfSaved: true);
                var sparkling_soul = Helpers.CreateAbility("ShamanSparklingSoulAbility",
                                                           "Sparkling Soul",
                                                           cooldown_buff.Name,
                                                           "",
                                                           icon,
                                                           AbilityType.Supernatural,
                                                           CommandType.Standard,
                                                           AbilityRange.Projectile,
                                                           "",
                                                           Helpers.reflexHalfDamage,
                                                           Helpers.CreateRunActions(SavingThrowType.Reflex, dmg),
                                                           Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getShamanArray()),
                                                           Helpers.CreateSpellDescriptor(SpellDescriptor.Electricity),
                                                           Helpers.CreateResourceLogic(resource),
                                                           library.Get<BlueprintAbility>("c073af2846b8e054fb28e6f72bc02749").GetComponent<AbilityDeliverProjectile>()//kinetic thunderstorm torrent
                                                           );
                                                      
                greater_spirit_ability = Helpers.CreateFeature("ShamanSparklingSoulFeature",
                                                               sparkling_soul.Name,
                                                               "The shaman gains electricity resistance 10. In addition, as a standard action she can unleash a 20-foot line of sparks from her fingertips, dealing 1d4 points of electricity damage per shaman level she possesses. A successful Reflex saving throw halves this damage. The shaman can use this ability three times per day, but she must wait 1d4 rounds between each use.",
                                                               "",
                                                               sparkling_soul.Icon,
                                                               FeatureGroup.None,
                                                               Helpers.CreateAddFact(sparkling_soul),
                                                               Helpers.CreateAddAbilityResource(resource),
                                                               Common.createEnergyDR(10, DamageEnergyType.Electricity)
                                                               );
            }


            static void createTrueSpiritAbility()
            {
                var resource = Helpers.CreateAbilityResource("ShamanWindElementalFormResource", "", "", "", null);
                resource.SetFixedResource(1);

                var wildshape_air_elemental_whirlwind_area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("0fb8d185085539e41b11b780bc7d9b9e", "ShamanWindElementalFormWhirlwindArea", "");

                wildshape_air_elemental_whirlwind_area.ReplaceComponent<ContextCalculateAbilityParamsBasedOnClass>(c => c.CharacterClass = shaman_class);

                var whirlwind_ability_buff = library.CopyAndAdd<BlueprintBuff>("788662b4625f74c43b0dfb0308fb155e", "ShamanWindElementalFormWhirlwindBuff", "");
                whirlwind_ability_buff.ReplaceComponent<AddAreaEffect>(a => a.AreaEffect = wildshape_air_elemental_whirlwind_area);
                var whirlwind_ability = library.CopyAndAdd<BlueprintActivatableAbility>("524954358fc39c14fb0417fceb1322c6", "ShamanWindElementalFormWhirlwindActivatableAbility", "");
                whirlwind_ability.Buff = whirlwind_ability_buff;

                var buff = library.CopyAndAdd<BlueprintBuff>("eb52d24d6f60fc742b32fe943b919180", "ShamanWindElementalFormBuff", "");
                buff.SetName("Elemental Form (Huge Air Elemental)");
                buff.ReplaceComponent<Polymorph>(p => p.Facts = new BlueprintUnitFact[] { p.Facts[1], whirlwind_ability });

                var ability = library.CopyAndAdd<BlueprintAbility>("e3ee03c3a959ca046a39827508ab8943", "ShamanWindElementalFormAbility", "");
                ability.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", getShamanArray()));
                ability.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = resource);
                ability.ReplaceComponent<AbilityTargetHasFact>(a => a.CheckedFacts = new BlueprintUnitFact[] { buff });
                ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(Common.changeAction<ContextActionApplyBuff>(a.Actions.Actions,
                                                                                                                                                       b => b.Buff = buff)
                                                                                                          )
                                                                );
                ability.SetName("Elemental Form (Huge Air Elemental)");
                true_spirit_ability = Common.AbilityToFeature(ability, false);
                true_spirit_ability.SetDescription("As a standard action, the shaman assumes the form of a Huge air elemental, as if using elemental body IV with a duration of 1 hour per level. The shaman can use this ability once per day.");
                true_spirit_ability.AddComponent(Helpers.CreateAddAbilityResource(resource));
            }


            static void createManifestation()
            {
                manifestation = Helpers.CreateFeature("ShamanWindManifestationFeature",
                                                      "Manifestation",
                                                      "Upon reaching 20th level, the shaman becomes a spirit of flame. The shaman gains electricity resistance 30. She can also apply any one of the following feats to any electricity spell she casts without increasing the spell’s level or casting time: Reach Spell, Extend Spell. She doesn’t need to possess these feats to use this ability.",
                                                      "",
                                                      library.Get<BlueprintProgression>("cd788df497c6f10439c7025e87864ee4").Icon, //electric wall
                                                      FeatureGroup.None,
                                                      Common.createEnergyDR(30, DamageEnergyType.Electricity));

                var extend = Common.CreateMetamagicAbility(manifestation, "Extend", "Extend Spell (Electricity)", Kingmaker.UnitLogic.Abilities.Metamagic.Extend, SpellDescriptor.Fire, "", "");
                extend.Group = ActivatableAbilityGroupExtension.ShamanFlamesMetamagic.ToActivatableAbilityGroup();
                var reach = Common.CreateMetamagicAbility(manifestation, "Reach", "Reach Spell (Electricity)", Kingmaker.UnitLogic.Abilities.Metamagic.Reach, SpellDescriptor.Fire, "", "");
                reach.Group = ActivatableAbilityGroupExtension.ShamanFlamesMetamagic.ToActivatableAbilityGroup();
                manifestation.AddComponent(Helpers.CreateAddFacts(extend, reach));
            }


        }


    }
}
