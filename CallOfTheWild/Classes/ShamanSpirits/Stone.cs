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
        public class StoneSpirit
        {
            public BlueprintFeature spirit_ability;
            public BlueprintFeature greater_spirit_ability;
            public BlueprintFeature true_spirit_ability;
            public BlueprintFeature manifestation;
            public BlueprintFeature stone_stability;
            public BlueprintFeature loadstone;
            public BlueprintFeature metal_curse;
            public BlueprintFeature ward_of_stone;
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
                    library.Get<BlueprintAbility>("85067a04a97416949b5d1dbf986d93f3"), //stone fist
                    library.Get<BlueprintAbility>("5181c2ed0190fc34b8a1162783af5bf4"), //stone call
                    library.Get<BlueprintAbility>("1a36c8b9ed655c249a9f9e8d4731f001"), //soothing mud
                    library.Get<BlueprintAbility>("d1afa8bc28c99104da7d784115552de5"), //spike stones
                    library.Get<BlueprintAbility>("c66e86905f7606c4eaa5c774f0357b2b"), //stoneskin
                    library.Get<BlueprintAbility>("989d3ed13d27d054ea2d26ab4956d075"), //summon elemental huge earth
                    library.Get<BlueprintAbility>("facdc8851a0b3f44a8bed50f0199b83c"), //elemental body IV earth
                    library.Get<BlueprintAbility>("65254c7a2cf18944287207e1de3e44e8"), //summon elemental elder earth
                    library.Get<BlueprintAbility>("01300baad090d634cb1a1b2defe068d6"), //clashing rocks
                };

                return new Oracle.Spirit("Stone",
                                         "Stone",
                                          "The skin of a shaman who selects the stone spirit takes on a rough, stony appearance. When the shaman calls upon one of this spirit’s abilities, tiny gemstones underneath her flesh pulse with a bright glow, like phosphorescent geodes glittering in a dark cave.",
                                          library.Get<BlueprintAbility>("01300baad090d634cb1a1b2defe068d6").Icon,//clashing rocks
                                          "",
                                          spirit_ability,
                                          greater_spirit_ability,
                                          spells,
                                          hexes
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
                return new Archetypes.SpiritWhisperer.Spirit("Stone",
                                                              "Stone",
                                                              "The skin of a shaman who selects the stone spirit takes on a rough, stony appearance. When the shaman calls upon one of this spirit’s abilities, tiny gemstones underneath her flesh pulse with a bright glow, like phosphorescent geodes glittering in a dark cave.",
                                                              library.Get<BlueprintAbility>("01300baad090d634cb1a1b2defe068d6").Icon,//clashing rocks
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
                    library.Get<BlueprintAbility>("85067a04a97416949b5d1dbf986d93f3"), //stone fist
                    library.Get<BlueprintAbility>("5181c2ed0190fc34b8a1162783af5bf4"), //stone call
                    library.Get<BlueprintAbility>("1a36c8b9ed655c249a9f9e8d4731f001"), //soothing mud
                    library.Get<BlueprintAbility>("d1afa8bc28c99104da7d784115552de5"), //spike stones
                    library.Get<BlueprintAbility>("c66e86905f7606c4eaa5c774f0357b2b"), //stoneskin
                    library.Get<BlueprintAbility>("989d3ed13d27d054ea2d26ab4956d075"), //summon elemental huge earth
                    library.Get<BlueprintAbility>("facdc8851a0b3f44a8bed50f0199b83c"), //elemental body IV earth
                    library.Get<BlueprintAbility>("65254c7a2cf18944287207e1de3e44e8"), //summon elemental elder earth
                    library.Get<BlueprintAbility>("01300baad090d634cb1a1b2defe068d6"), //clashing rocks
                };

                return new Shaman.Spirit("Stone",
                                  "Stone",
                                  "The skin of a shaman who selects the stone spirit takes on a rough, stony appearance. When the shaman calls upon one of this spirit’s abilities, tiny gemstones underneath her flesh pulse with a bright glow, like phosphorescent geodes glittering in a dark cave.",
                                  library.Get<BlueprintAbility>("01300baad090d634cb1a1b2defe068d6").Icon,//clashing rocks
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
                stone_stability = hex_engine.createStoneStability(prefix + "StoneStability",
                                             "Stone Stability",
                                             "The shaman receives a +4 bonus to her CMD when resisting bull rush or trip attempts as long as she is standing on the ground. At 5th level, the shaman receives Improved Trip as a bonus feat. At 10th level, the shaman receives Greater Trip as a bonus feat. The shaman does not need to meet the prerequisites of these feats."
                                             );

                loadstone = hex_engine.createLoadStone(prefix + "Loadstone",
                                                        "Loadstone",
                                                        "The shaman causes one creature within 30 feet to become heavy and lethargic. The creature is treated as if it suffered effect of slow spell. The effect lasts for a number of rounds equal to the shaman’s level. A successful Will saving throw negates this effect. Whether or not the save is successful, the creature cannot be the target of this hex again for 24 hours."
                                                        );

                metal_curse = hex_engine.createMetalCurse(prefix + "MetalCurse",
                                                          "Metal Curse",
                                                          "The shaman causes a creature within 30 feet to become slightly magnetic until the end of the shaman’s next turn. Whenever the creature is attacked with a melee or ranged weapon constructed primarily of metal, it takes a –2 penalty to AC. At 8th and 16th levels, the penalty increases by –2 and the duration extends by 1 round. Once affected, the creature cannot be the target of this hex again for 24 hours."
                                                         );
                ward_of_stone = hex_engine.createWardOfStone(prefix + "WardOfStone",
                                                             "Ward of Stone",
                                                             "The shaman touches a willing creature (including herself ) and grants a ward of stoene. The next time the warded creature is struck with a melee attack, it is treated as if it has DR 5/adamantine. This ward lasts for 1 minute, after which it fades away if not already expended. At 8th and 16th levels, the ward lasts for one additional attack. A creature affected by this hex cannot be affected by it again for 24 hours."
                                                            );
                hexes = new BlueprintFeature[]
                {
                    stone_stability,
                    loadstone,
                    metal_curse,
                    ward_of_stone,
                };
            }


            void createSpiritAbility()
            {
                var icon = library.Get<BlueprintAbility>("97d0a51ca60053047afb9aca900fb71b").Icon; //burning hands acid
                var resource = Helpers.CreateAbilityResource(prefix + "TouchOfAcidResource", "", "", "", null);
                resource.SetIncreasedByStat(3, secondary_stat);
                var touch_of_acid = library.CopyAndAdd<BlueprintAbility>("3ff40918d33219942929f0dbfe5d1dee", prefix + "TouchOfAcidAbility", ""); //earth domain acid dart
                touch_of_acid.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Acid));
                touch_of_acid.RemoveComponents<SpellComponent>();
                touch_of_acid.RemoveComponents<AbilityResourceLogic>();
                touch_of_acid.ReplaceComponent<AbilityDeliverProjectile>(Helpers.CreateDeliverTouch());
                touch_of_acid.setMiscAbilityParametersTouchHarmful();
                touch_of_acid.Type = AbilityType.Supernatural;
                touch_of_acid.Range = AbilityRange.Touch;
                touch_of_acid.SpellResistance = false;
                touch_of_acid.SetNameDescriptionIcon("Touch of Acid",
                                                      Main.settings.balance_fixes
                                                      ? "As a standard action, the shaman can make a melee touch attack that deals 1d8 points of acid damage plus 1d8 points for every 2 shaman levels she possesses beyond first. A shaman can use this ability a number of times per day equal to 3 + her Charisma modifier."
                                                      : "As a standard action, the shaman can make a melee touch attack that deals 1d6 points of acid damage plus 1 point for every 2 shaman levels she possesses. A shaman can use this ability a number of times per day equal to 3 + her Charisma modifier.",
                                                       icon);
                touch_of_acid.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", hex_engine.hex_classes));
                var touch_of_acid_sticky = Helpers.CreateTouchSpellCast(touch_of_acid, resource);
                var corrosive = library.Get<BlueprintWeaponEnchantment>("633b38ff1d11de64a91d490c683ab1c8");

                var corrosive_weapon_feature = Helpers.CreateFeature(prefix + "TouchOfAcidCorrosiveWeaponFeature",
                                                              "",
                                                              "",
                                                              "",
                                                              null,
                                                              FeatureGroup.None,
                                                              Helpers.Create<NewMechanics.EnchantmentMechanics.PersistentWeaponEnchantment>(p => p.enchant = corrosive)
                                                              );

                corrosive_weapon_feature.HideInCharacterSheetAndLevelUp = true;

                spirit_ability = Helpers.CreateFeature(prefix + "TouchOfAcidFeature",
                                                       touch_of_acid.Name,
                                                       touch_of_acid.Description + " At 11th level, any weapon she wields is treated as a corrosive weapon.",
                                                       "",
                                                       touch_of_acid.Icon,
                                                       FeatureGroup.None,
                                                       Helpers.CreateAddFact(touch_of_acid_sticky),
                                                       Helpers.CreateAddAbilityResource(resource),
                                                       Helpers.CreateAddFeatureOnClassLevel(corrosive_weapon_feature, 11, hex_engine.hex_classes)
                                                       );
            }


            void createGreaterSpiritAbility()
            {
                var resource = Helpers.CreateAbilityResource(prefix + "BodyOfEarthResource", "", "", "", null);
                resource.SetFixedResource(3);

                var icon = library.Get<BlueprintAbility>("4aa7942c3e62a164387a73184bca3fc1").Icon;//disintegrate

                var cooldown_buff = Helpers.CreateBuff(prefix + "BodyOfEarthCooldownBuff",
                                                       "Body of Earth: Cooldown",
                                                       $"As a standard action, she can cause jagged pieces of stone to explode from her body in a 10-foot-radius burst. This deals 1d{BalanceFixes.getDamageDie(DiceType.D6)} points of piercing damage per 2 shaman levels she possesses. A successful Reflex saving throw halves this damage. The shaman can use this ability three times per day, but she must wait 1d4 rounds between each use.",
                                                       "",
                                                       icon,
                                                       null);
                var apply_cooldown = Common.createContextActionApplyBuff(cooldown_buff, Helpers.CreateContextDuration(0, diceType: DiceType.D4, diceCount: 1), dispellable: false);
                var dmg = Helpers.CreateActionDealDamage(PhysicalDamageForm.Piercing,
                                                                                     Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.Default)),
                                                                                     isAoE: true, halfIfSaved: true);

                var effect = Helpers.CreateConditional(Common.createContextConditionIsCaster(),
                                                      null,
                                                      Common.createContextActionSavingThrow(SavingThrowType.Reflex, Helpers.CreateActionList(dmg))
                                                      );

                var body_of_Earth_ability = Helpers.CreateAbility(prefix + "BodyOfEarthAbility",
                                                               "Body of Earth",
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
                body_of_Earth_ability.setMiscAbilityParametersSelfOnly();
                body_of_Earth_ability.AddComponent(Common.createAbilityCasterHasNoFacts(cooldown_buff));

                greater_spirit_ability = Common.AbilityToFeature(body_of_Earth_ability, false);
                greater_spirit_ability.AddComponents(Common.createMaterialDR(Helpers.CreateContextValue(AbilityRankType.Default), PhysicalDamageMaterial.Adamantite),
                                                     Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.DivStep,
                                                                                     classes: hex_engine.hex_classes, stepLevel: 4),
                                                     Helpers.CreateAddAbilityResource(resource)
                                                    );
                greater_spirit_ability.SetDescription("The shaman gains DR 2/adamantine. This DR increases by 1 for every 4 levels beyond 8th the shaman possesses. In addition, as a standard action, she can cause jagged pieces of stone to explode from her body in a 10-foot-radius burst. This deals 1d6 points of piercing damage per 2 shaman levels she possesses. A successful Reflex saving throw halves this damage. The shaman can use this ability three times per day, but she must wait 1d4 rounds between each use.");
            }


            void createTrueSpiritAbility()
            {
                var resource = Helpers.CreateAbilityResource(prefix + "StoneElementalFormResource", "", "", "", null);
                resource.SetFixedResource(1);

                var buff = library.CopyAndAdd<BlueprintBuff>("f0826c3794c158c4cbbe9ceb4210d6d6", prefix + "StoneElementalFormBuff", "");
                buff.SetName("Elemental Form (Huge Earth Elemental)");

                var ability = library.CopyAndAdd<BlueprintAbility>("e49d2cde42f25e546826600d11b4833e", prefix + "StoneElementalFormAbility", "");
                ability.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", hex_engine.hex_classes));
                ability.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = resource);
                ability.ReplaceComponent<AbilityTargetHasFact>(a => a.CheckedFacts = new BlueprintUnitFact[] { buff });
                ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(Common.changeAction<ContextActionApplyBuff>(a.Actions.Actions,
                                                                                                                                                       b => b.Buff = buff)
                                                                                                          )
                                                                );
                ability.SetName("Elemental Form (Huge Earth Elemental)");
                true_spirit_ability = Common.AbilityToFeature(ability, false);
                true_spirit_ability.SetDescription(Helpers.CreateString(true_spirit_ability.name + "2.Description", "As a standard action, the shaman assumes the form of a Huge (or smaller) earth elemental, as elemental body IV with a duration of 1 hour per level. The shaman can use this ability once per day."));
                true_spirit_ability.AddComponent(Helpers.CreateAddAbilityResource(resource));
            }


            void createManifestation()
            {
                manifestation = Helpers.CreateFeature(prefix + "StoneManifestationFeature",
                                                      "Manifestation",
                                                      "Upon reaching 20th level, the shaman becomes a being of acid and earth. The shaman gains acid resistance 30. She can also apply any one of the following feats to any acid or earth spell she casts without increasing the spell’s level or casting time: Reach Spell, Extend Spell. She doesn’t need to possess these feats to use this ability.",
                                                      "",
                                                      library.Get<BlueprintProgression>("32393034410fb2f4d9c8beaa5c8c8ab7").Icon, //wall of acid
                                                      FeatureGroup.None,
                                                      Common.createEnergyDR(30, DamageEnergyType.Acid));

                var extend = Common.CreateMetamagicAbility(manifestation, "Extend", "Extend Spell (Acid)", Kingmaker.UnitLogic.Abilities.Metamagic.Extend, SpellDescriptor.Acid | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Earth, "", "");
                extend.Group = ActivatableAbilityGroupExtension.ShamanStoneMetamagic.ToActivatableAbilityGroup();
                var reach = Common.CreateMetamagicAbility(manifestation, "Reach", "Reach Spell (Acid)", Kingmaker.UnitLogic.Abilities.Metamagic.Reach, SpellDescriptor.Acid | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Earth, "", "");
                reach.Group = ActivatableAbilityGroupExtension.ShamanStoneMetamagic.ToActivatableAbilityGroup();
                manifestation.AddComponent(Helpers.CreateAddFacts(extend, reach));
            }


        }

    }
}
