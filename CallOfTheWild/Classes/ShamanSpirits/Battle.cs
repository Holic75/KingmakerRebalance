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
using  static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using  static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    public partial class SpiritsEngine
    {
        public class BattleSpirit
        {
            public  BlueprintFeature spirit_ability;
            public  BlueprintFeature greater_spirit_ability;
            public  BlueprintFeature true_spirit_ability;
            public  BlueprintFeature manifestation;
            public  BlueprintFeatureSelection battle_master_hex;
            public  BlueprintFeature battle_ward_hex;
            public  BlueprintFeature curse_of_suffering_hex;
            public  BlueprintFeature hampering_hex;
            public  BlueprintAbility[] spells;
            public  BlueprintFeature[] hexes;
            public StatType primary_stat;
            public StatType secondary_stat;

            HexEngine hex_engine;
            string prefix;
            string manifestation_prefix;
            bool test_mode;


            public Oracle.Spirit createOracleSpirit(HexEngine associated_hex_engine, string asset_prefix, bool test = false)
            {
                test_mode = test;
                hex_engine = associated_hex_engine;
                prefix = asset_prefix;
                primary_stat = hex_engine.hex_stat;
                secondary_stat = hex_engine.hex_stat;

                createSpiritAbility();
                createGreaterSpiritAbility();

                spells = new BlueprintAbility[9]
                {
                    library.Get<BlueprintAbility>("c60969e7f264e6d4b84a1499fdcf9039"), //enlarge person
                    library.Get<BlueprintAbility>("c28de1f98a3f432448e52e5d47c73208"), //protection from arrows
                    library.Get<BlueprintAbility>("2d4263d80f5136b4296d6eb43a221d7d"), //magical vestment,
                    NewSpells.wall_of_fire,
                    library.Get<BlueprintAbility>("90810e5cf53bf854293cbd5ea1066252"), //righteous might
                    library.Get<BlueprintAbility>("6a234c6dcde7ae94e94e9c36fd1163a7"), //bulls strength mass
                    library.Get<BlueprintAbility>("da1b292d91ba37948893cdbe9ea89e28"), //legendary proportions
                    library.Get<BlueprintAbility>("7cfbefe0931257344b2cb7ddc4cdff6f"), //stormbolts
                    library.Get<BlueprintAbility>("01300baad090d634cb1a1b2defe068d6"), //clashing rocks
                };

                createHexes();

                return new Oracle.Spirit("Battle",
                                  "Battle",
                                  "A shaman who selects the battle spirit gains scars from every wound she takes, and the grit of battle always seems to cling on her body. When she calls upon one of this spirit’s abilities, she grows in stature—becoming taller and more muscular, with a grimace of rage stretching across her face.",
                                  library.Get<BlueprintAbility>("c78506dd0e14f7c45a599990e4e65038").Icon,
                                  "",
                                  spirit_ability,
                                  greater_spirit_ability,
                                  spells,
                                  hexes);
            }


            public Archetypes.SpiritWhisperer.Spirit createSpiritWhispererSpirit(HexEngine associated_hex_engine, string asset_prefix, string manifestation_asset_prefix, bool test = false)
            {
                test_mode = test;
                hex_engine = associated_hex_engine;
                prefix = asset_prefix;
                manifestation_prefix = manifestation_asset_prefix;
                primary_stat = hex_engine.hex_stat;
                secondary_stat = hex_engine.hex_secondary_stat;

                createSpiritAbility();
                createGreaterSpiritAbility();
                createManifestation();

                createHexes();

                return new Archetypes.SpiritWhisperer.Spirit("Battle",
                                                             "Battle",
                                                             "A shaman who selects the battle spirit gains scars from every wound she takes, and the grit of battle always seems to cling on her body. When she calls upon one of this spirit’s abilities, she grows in stature—becoming taller and more muscular, with a grimace of rage stretching across her face.",
                                                             manifestation.Icon,
                                                             "",
                                                             spirit_ability,
                                                             greater_spirit_ability,
                                                             manifestation,
                                                             hexes);
            }


            public  Shaman.Spirit createShamanSpirit(HexEngine associated_hex_engine, string asset_prefix, string manifestation_asset_prefix, bool test = false)
            {
                test_mode = test;
                hex_engine = associated_hex_engine;
                prefix = asset_prefix;
                manifestation_prefix = manifestation_asset_prefix;
                primary_stat = hex_engine.hex_stat;
                secondary_stat = hex_engine.hex_secondary_stat;

                createSpiritAbility();
                createGreaterSpiritAbility();
                createTrueSpiritAbility();
                createManifestation();

                spells = new BlueprintAbility[9]
                {
                    library.Get<BlueprintAbility>("c60969e7f264e6d4b84a1499fdcf9039"), //enlarge person
                    library.Get<BlueprintAbility>("c28de1f98a3f432448e52e5d47c73208"), //protection from arrows
                    library.Get<BlueprintAbility>("2d4263d80f5136b4296d6eb43a221d7d"), //magical vestment,
                    NewSpells.wall_of_fire_fire_domain,
                    library.Get<BlueprintAbility>("90810e5cf53bf854293cbd5ea1066252"), //righteous might
                    library.Get<BlueprintAbility>("6a234c6dcde7ae94e94e9c36fd1163a7"), //bulls strength mass
                    library.Get<BlueprintAbility>("da1b292d91ba37948893cdbe9ea89e28"), //legendary proportions
                    library.Get<BlueprintAbility>("7cfbefe0931257344b2cb7ddc4cdff6f"), //stormbolts
                    library.Get<BlueprintAbility>("01300baad090d634cb1a1b2defe068d6"), //clashing rocks
                };

                createHexes();

                return new Shaman.Spirit("Battle",
                                  "Battle",
                                  "A shaman who selects the battle spirit gains scars from every wound she takes, and the grit of battle always seems to cling on her body. When she calls upon one of this spirit’s abilities, she grows in stature—becoming taller and more muscular, with a grimace of rage stretching across her face.",
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
                createBattleMasterHex();
                battle_ward_hex = hex_engine.createBattleWardHex(prefix + "BattleWard",
                                                 "Battle Ward",
                                                 "The shaman touches a willing creature (including herself) and grants a battle ward. The next time a foe makes an attack roll against the target, the ward activates and grants a +3 deflection bonus to the warded creature’s AC. Each subsequent time she’s attacked, the defection bonus reduces by 1 (to +2 for the second time she’s attacked and +1 for the third). The ward fades when the bonus is reduced to +0 or after 24 hours, whichever comes first. At 8th level, the ward’s starting bonus increases to +4. At 16th level, it increases to +5. A creature affected by this hex cannot be affected by it again for 24 hours."
                                                 );

                curse_of_suffering_hex = hex_engine.createCurseOfSuffering(prefix + "CurseOfSuffering",
                                                                        "Curse of Suffering",
                                                                        "The shaman causes a creature within 30 feet to take more damage from bleed effects and causes its wounds to heal at a slower rate. When the cursed creature takes bleed damage, it takes 1 additional point of bleed damage (even if the bleed is ability damage). Furthermore, when the target is subject to an effect that would restore its hit points, that effect restores only half the normal amount of hit points. This curse lasts for a number of rounds equal to the shaman’s level. A creature affected by this hex cannot be affected by it again for 24 hours."
                                                                       );

                hampering_hex = hex_engine.createHamperingHex(prefix + "HamperingHex",
                                                                "Hampering Hex",
                                                                "The shaman causes a creature within 30 feet to take a –2 penalty to AC and CMD for a number of rounds equal to the shaman’s level. A successful Will saving throw reduces this to just 1 round. At 8th level, the penalty becomes –4. Whether or not the save is successful, a creature affected by a hampering hex cannot be the target of this hex again for 24 hours."
                                                               );
                hexes = new BlueprintFeature[]
                {
                    battle_master_hex,
                    battle_ward_hex,
                    curse_of_suffering_hex,
                    hampering_hex,
                };
            }


            void createBattleMasterHex()
            {
                battle_master_hex = Helpers.CreateFeatureSelection(prefix + "BattleMasterHex",
                                                                   "Battle Master",
                                                                   "The shaman makes an extra attack of opportunity each round. This ability stacks with the attacks of opportunity granted by the Combat Reflexes feat. At 8th level, the shaman gains the Weapon Specialization feat in a weapon of her choice as a bonus feat. At 16th level, the shaman gains the Greater Weapon Focus feat as a bonus feat, for the same weapon chosen for Weapon Specialization. The shaman doesn’t need to meet the prerequisites of these feats.",
                                                                   "",
                                                                   library.Get<BlueprintParametrizedFeature>("31470b17e8446ae4ea0dacd6c5817d86").Icon,
                                                                   FeatureGroup.None
                                                                   );



                foreach (var category in Enum.GetValues(typeof(WeaponCategory)).Cast<WeaponCategory>())
                {

                    var ws_feature = Helpers.CreateFeature(prefix + "BattleMasterHex" + category.ToString() + "WeaponSpecializationFeature",
                                                           "",
                                                           "",
                                                           "",
                                                           null,
                                                           FeatureGroup.None,
                                                           Common.createAddParametrizedFeatures(library.Get<BlueprintParametrizedFeature>("31470b17e8446ae4ea0dacd6c5817d86"), category)
                                                           );
                    ws_feature.HideInCharacterSheetAndLevelUp = true;
                    var gws_feature = Helpers.CreateFeature(prefix + "BattleMasterHex" + category.ToString() + "GreaterWeaponFocusFeature",
                                                               "",
                                                               "",
                                                               "",
                                                               null,
                                                               FeatureGroup.None,
                                                               Common.createAddParametrizedFeatures(library.Get<BlueprintParametrizedFeature>("09c9e82965fb4334b984a1e9df3bd088"), category)
                                                               );
                    gws_feature.HideInCharacterSheetAndLevelUp = true;
                    var feature = Helpers.CreateFeature(prefix + "BattleMasterHex" + category.ToString() + "Feature",
                                                        battle_master_hex.Name + $" ({LocalizedTexts.Instance.Stats.GetText(category)})",
                                                        battle_master_hex.Description,
                                                        "",
                                                        battle_master_hex.Icon,
                                                        FeatureGroup.None,
                                                        Helpers.CreateAddStatBonus(StatType.AttackOfOpportunityCount, 1, ModifierDescriptor.UntypedStackable),
                                                        Helpers.CreateAddFeatureOnClassLevel(ws_feature, 8, hex_engine.hex_classes),
                                                        Helpers.CreateAddFeatureOnClassLevel(gws_feature, 16, hex_engine.hex_classes),
                                                        Helpers.Create<PrerequisiteProficiency>(p =>
                                                        {
                                                            p.WeaponProficiencies = new WeaponCategory[] {category};
                                                            p.ArmorProficiencies = new Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup[0];
                                                        }));
                    battle_master_hex.AllFeatures = battle_master_hex.AllFeatures.AddToArray(feature);
                }
                battle_master_hex.AddComponent(Helpers.PrerequisiteNoFeature(battle_master_hex));
            }


            void createSpiritAbility()
            {
                var resource = Helpers.CreateAbilityResource(prefix + "BattleSpiritResource", "", "", "", null);
                resource.SetIncreasedByStat(3, secondary_stat);
                var rage = library.Get<BlueprintBuff>("a1ffec0ce7c167a40aaea13dc49b757b");
                var buff = Helpers.CreateBuff(prefix + "BattleSpiritEffectBuff",
                                              "Battle Spirit",
                                              "A shaman surrounds herself with the spirit of battle. Allies within 30 feet of the shaman (including the shaman) receive a +1 morale bonus on attack rolls and weapon damage rolls. At 8th level and 16th level, these bonuses increase by 1. The shaman can use this ability for a number of rounds per day equal to 3 + her Charisma modifier. These rounds do not need to be consecutive.",
                                              "",
                                              rage.Icon,
                                              rage.FxOnStart,
                                              Helpers.CreateAddContextStatBonus(StatType.AdditionalAttackBonus, ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus),
                                              Helpers.CreateAddContextStatBonus(StatType.AdditionalDamage, ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus),
                                              Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.OnePlusDivStep,
                                                                              classes: hex_engine.hex_classes, stepLevel: 8, type: AbilityRankType.StatBonus)
                                             );

                var ability = Common.convertPerformance(library.Get<BlueprintActivatableAbility>("5250fe10c377fdb49be449dfe050ba70"), buff, prefix + "BattleSpirit");
                ability.Group = ActivatableAbilityGroup.None;
                Helpers.SetField(ability, "m_ActivateWithUnitCommand", CommandType.Standard);
                ability.ReplaceComponent<ActivatableAbilityResourceLogic>(a => a.RequiredResource = resource);
                ability.DeactivateIfCombatEnded = true;
                spirit_ability = Common.ActivatableAbilityToFeature(ability, hide: false);
                spirit_ability.AddComponent(Helpers.CreateAddAbilityResource(resource));
            }


            void createGreaterSpiritAbility()
            {
                var resource = Helpers.CreateAbilityResource(prefix + "EnemiesBaneResource", "", "", "", null);
                resource.SetIncreasedByStat(3, secondary_stat);

                var bane_enchant = library.Get<BlueprintWeaponEnchantment>("1a93ab9c46e48f3488178733be29342a");
                var icon = library.Get<BlueprintFeature>("4a24abda0d564f94094e55ef117fc5b2").Icon; // bane
                var rage = library.Get<BlueprintBuff>("a1ffec0ce7c167a40aaea13dc49b757b");
                var buff = Helpers.CreateBuff(prefix + "EnemiesBaneBuff",
                                              "Enemies’ Bane",
                                              "As a swift action, the shaman imbues a single weapon she’s wielding with the bane weapon special ability, choosing the type of creature affected each time she does. The effect lasts for 1 minute. If the weapon already has the bane weapon special ability of the type chosen, the additional damage dealt by bane increases to 4d6. The shaman can use this ability a number of times per day equal to 3 + her Charisma modifier.",
                                              "",
                                              icon,
                                              null,
                                              Common.createBuffEnchantWornItem(bane_enchant)
                                             );

                var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false);
                var ability = Helpers.CreateAbility(prefix + "EnemiesBaneAbility",
                                                    buff.Name,
                                                    buff.Description,
                                                    "",
                                                    buff.Icon,
                                                    AbilityType.Supernatural,
                                                    CommandType.Swift,
                                                    AbilityRange.Personal,
                                                    Helpers.oneMinuteDuration,
                                                    "",
                                                    Helpers.CreateRunActions(apply_buff),
                                                    Helpers.CreateResourceLogic(resource)
                                                    );
                ability.setMiscAbilityParametersSelfOnly(animation: Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.EnchantWeapon);
                ability.NeedEquipWeapons = true;
                greater_spirit_ability = Common.AbilityToFeature(ability, hide: false);
                greater_spirit_ability.AddComponent(Helpers.CreateAddAbilityResource(resource));
            }


             void createTrueSpiritAbility()
            {
                var resource = Helpers.CreateAbilityResource(prefix + "ParagonOfBattleResource", "", "", "", null);
                resource.SetIncreasedByStat(3, secondary_stat);

                var enlarge_buff = library.Get<BlueprintBuff>("4f139d125bb602f48bfaec3d3e1937cb");

                var apply_enlarge = Common.createContextActionApplyBuff(enlarge_buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false);
                var apply_dj = Common.createContextActionApplyBuff(NewSpells.deadly_juggernaut_buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false);
                var ability = Helpers.CreateAbility(prefix + "ParagonOfBattleAbility",
                                                    "Paragon of Battle",
                                                    "As a standard action, the shaman assumes a form that combines the effects of enlarge person and deadly juggernaut for 1 minute. The shaman can use this ability a number of times per day equal to 3 + her Charisma modifier.",
                                                    "",
                                                    NewSpells.deadly_juggernaut_buff.Icon,
                                                    AbilityType.Supernatural,
                                                    CommandType.Swift,
                                                    AbilityRange.Personal,
                                                    Helpers.oneMinuteDuration,
                                                    "",
                                                    Helpers.CreateRunActions(apply_enlarge, apply_dj),
                                                    Common.createAbilitySpawnFx("352469f228a3b1f4cb269c7ab0409b8e", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                    Helpers.CreateResourceLogic(resource)
                                                    );
                ability.setMiscAbilityParametersSelfOnly();


                true_spirit_ability = Common.AbilityToFeature(ability, hide: false);
                true_spirit_ability.AddComponent(Helpers.CreateAddAbilityResource(resource));
            }


             void createManifestation()
            {
                var icon = library.Get<BlueprintAbility>("c78506dd0e14f7c45a599990e4e65038").Icon;
                manifestation = Helpers.CreateFeature(manifestation_prefix + "BattleSpiritManifestationFeature",
                                                      "Manifestation",
                                                      "Upon reaching 20th level, the shaman becomes a spirit of battle. He gains pounce ability and diehard feat. Whenever she scores a critical hit, the attack ignores damage reduction. She gains a +4 insight bonus to AC for the purposes of confirming critical hits against her.",
                                                      "",
                                                      icon,
                                                      FeatureGroup.None,
                                                      Common.createCriticalConfirmationACBonus(4),
                                                      library.Get<BlueprintFeature>("1a8149c09e0bdfc48a305ee6ac3729a8").GetComponent<AddMechanicsFeature>(), //pounce
                                                      Helpers.Create<IgnoreDamageReductionOnCriticalHit>(),
                                                      Common.createAddFeatureIfHasFact(library.Get<BlueprintFeature>("c99f3405d1ef79049bd90678a666e1d7"), //diehard
                                                                                       library.Get<BlueprintFeature>("86669ce8759f9d7478565db69b8c19ad"),
                                                                                       not: true)
                                                      );
            }

        }
    }
}
