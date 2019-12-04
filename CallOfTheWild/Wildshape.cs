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
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.ElementsSystem;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using static CallOfTheWild.NewMechanics.EnchantmentMechanics.TransferPrimaryHandWeaponEnchantsToPolymorph;
using static Kingmaker.Blueprints.BlueprintUnit;
using Kingmaker.Items.Slots;

namespace CallOfTheWild
{
    public class Wildshape
    {
        static internal BlueprintAbility beast_shape_prototype = library.CopyAndAdd<BlueprintAbility>("61a7ed778dd93f344a5dacdbad324cc9", "BeastShapePrototype", "");

        static LibraryScriptableObject library => Main.library;
        static public BlueprintBuff wolf_form = Main.library.Get<BlueprintBuff>("00d8fbe9cf61dc24298be8d95500c84b");
        static public BlueprintBuff leopard_form = Main.library.Get<BlueprintBuff>("200bd9b179ee660489fe88663115bcbc");
        static public BlueprintBuff bear_form = Main.library.Get<BlueprintBuff>("0c0afabcfddeecc40a1545a282f2bec8");
        static public BlueprintBuff smilodon_form = Main.library.Get<BlueprintBuff>("c38def68f6ce13b4b8f5e5e0c6e68d08");

        static public BlueprintAbility turn_back = library.Get<BlueprintAbility>("bd09b025ee2a82f46afab922c4decca9");
        static public BlueprintBuff dire_wolf_form;
        static public BlueprintBuff mastodon_form;
        static public BlueprintBuff hodag_form;
        static public BlueprintBuff winter_wolf_form;
        static public BlueprintBuff mandragora_form;
        static public BlueprintBuff shambling_mound_form;
        static public BlueprintBuff treant_form;
        static public BlueprintBuff giant_flytrap_form;
        static public BlueprintBuff fire_giant_form;
        static public BlueprintBuff frost_giant_form;
        static public BlueprintBuff troll_form;
        static public BlueprintBuff storm_giant_form;
        static public BlueprintBuff athach_form;

        static public BlueprintFeature toss_feature;
        static public BlueprintFeature engulf;


        static BlueprintUnitFact reduced_reach = library.Get<BlueprintUnitFact>("c33f2d68d93ceee488aa4004347dffca");
        static BlueprintFeature tripping_bite = library.Get<BlueprintFeature>("f957b4444b6fb404e84ae2a5765797bb");
        static BlueprintFeature trip_defense_4legs = library.Get<BlueprintFeature>("13c87ac5985cc85498ef9d1ac8b78923");
        static BlueprintFeature trample = library.Get<BlueprintFeature>("9292099e5fd70f84fb07fbb9b8b6a5a5");
        static BlueprintAbility overrun = library.Get<BlueprintAbility>("1a3b471ecea51f7439a946b23577fd70");
        static BlueprintAbility winter_wolf_breath;
        static BlueprintItemWeapon Bite1d6 = library.Get<BlueprintItemWeapon>("a000716f88c969c499a535dadcf09286");
        static BlueprintItemWeapon mastodon_gore = library.Get<BlueprintItemWeapon>("de42c58801037b84c9d992634ddd7220");
        static BlueprintItemWeapon mastodon_slam = library.Get<BlueprintItemWeapon>("c2ce7bc3559b2024ea91ddf5bb321f0a");
        static BlueprintItemWeapon mandaragora_slam = library.Get<BlueprintItemWeapon>("7445b0b255796d34495a8bca81b2e2d4");
        static BlueprintItemWeapon mandragora_bite = library.Get<BlueprintItemWeapon>("61bc14eca5f8c1040900215000cfc218");
        static BlueprintItemWeapon treant_slam = library.Get<BlueprintItemWeapon>("04499d551301bf9488c1e94b74f8c6d2");
        static BlueprintItemWeapon giant_flytrap_bite = library.Get<BlueprintItemWeapon>("61bc14eca5f8c1040900215000cfc218");
        static BlueprintItemWeapon athach_slam = library.Get<BlueprintItemWeapon>("340ab07dff974a841b76f22ba4c3cf84");
        static BlueprintItemWeapon athach_claw = library.Get<BlueprintItemWeapon>("f8c9fc15f5966f74298cfb2b9bde0986");
        static BlueprintItemWeapon athach_bite = library.Get<BlueprintItemWeapon>("d2f99947db522e24293a7ec4eded453f");

        static public BlueprintAbility bear_form_spell;
        static public BlueprintAbility dire_wolf_form_spell;
        static public BlueprintAbility smilodon_form_spell = library.Get<BlueprintAbility>("502cd7fd8953ac74bb3a3df7e84818ae");
        static public BlueprintAbility mastodon_form_spell;
        static public BlueprintAbility leopard_form_spell;
        static public BlueprintAbility wolf_form_spell;
        static public BlueprintAbility hodag_form_spell;
        static public BlueprintAbility winter_wolf_form_spell;

        static public BlueprintAbility treant_form_spell;
        static public BlueprintAbility giant_flytrap_form_spell;
        static public BlueprintAbility plant_shapeI;
        static public BlueprintAbility plant_shapeII;
        static public BlueprintAbility plant_shapeIII;

        static public BlueprintAbility troll_form_spell;
        static public BlueprintAbility fire_giant_form_spell;
        static public BlueprintAbility frost_giant_form_spell;
        static public BlueprintAbility storm_giant_form_spell;
        static public BlueprintAbility athach_form_form_spell;
        static public BlueprintAbility giant_formI;
        static public BlueprintAbility giant_formII;

        static public BlueprintAbility shapechange;


        static public List<BlueprintAbility> animal_wildshapes = new List<BlueprintAbility>();
        static public BlueprintFeature first_wildshape_form;

        static public BlueprintFeature wild_armor_feature;
        static BlueprintBuff allow_wild_armor_buff;
        static public BlueprintBuff[] druid_wild_shapes;

        static public BlueprintFeature weapon_shift;
        static public BlueprintFeature improved_weapon_shift;
        static public BlueprintFeature greater_weapon_shift;
        static public BlueprintFeature mutated_shape;

        static internal void load()
        {
            fixFormOfTheDragonIWeapons();
            createToss();
            createEngulf();
            fixBeastShape1();
            fixBeastShape2();
            fixBeastShape3();
            fixBeastShape4();
            fixPolymorph1();
            fixPolymorph2();

            createPlantShapeI();
            createPlantShapeII();
            createPlantShapeIII();

            fixLegendaryProportions();
            fixAirElementalDC();
            createGiantFormI();
            createGiantFormII();
            createShapechange();

            fixDruid();
            createWildArmor();
            fixTransmuter();

            createWeaponShift();
            createMutatedShape();


            createFavoredClassNaturalAcBonus();
        }


        static void createFavoredClassNaturalAcBonus()
        {
            var icon = library.Get<BlueprintFeature>("c4d651bc0d4eabd41b08ee81bfe701d8").Icon; //wildshape

            var natural_ac_feature = Helpers.CreateFeature("DruidWildshapeNaturalAcFeature",
                                                           "Wildshape Natural AC Bonus",
                                                           "Add +1/3 to the druid’s natural armor bonus when using wild shape.",
                                                           "38ac98295f614fc394e74a444d03c67d",
                                                           icon,
                                                           FeatureGroup.None);
            natural_ac_feature.Ranks = 6;
            foreach (var shape in druid_wild_shapes)
            {
                shape.AddComponents(Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.NaturalArmor, rankType: AbilityRankType.ProjectilesCount),
                                    Helpers.CreateContextRankConfig(type: AbilityRankType.ProjectilesCount, baseValueType: ContextRankBaseValueType.FeatureRank, feature: natural_ac_feature)
                                   );
            }                                        
        }

        static void createMutatedShape()
        {
            mutated_shape = Helpers.CreateFeature("MutatedShapeFeature",
                                                  "Mutated Shape",
                                                  "When you use wild shape, you grow an additional appendage that allows you to make one more attack per round.",
                                                  "",
                                                  null,
                                                  FeatureGroup.Feat,
                                                  Helpers.PrerequisiteFeature(first_wildshape_form),
                                                  Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 6),
                                                  Helpers.PrerequisiteStatValue(StatType.Wisdom, 19)
                                                 );

            var buff = Helpers.CreateBuff("MutatedShapeBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Helpers.Create<BuffExtraAttack>(b => b.Number = 1)
                                          );
            buff.SetBuffFlags(BuffFlags.HiddenInUi);

            foreach (var shape in druid_wild_shapes)
            {
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(shape, buff, mutated_shape);
            }
            library.AddFeats(mutated_shape);
        }


        static void createWeaponShift()
        {
            BlueprintWeaponEnchantment[] enchants1 = new BlueprintWeaponEnchantment[]
            {
                library.Get<BlueprintWeaponEnchantment>("e5990dc76d2a613409916071c898eee8"), //cold iron 
                library.Get<BlueprintWeaponEnchantment>("0ae8fc9f2e255584faf4d14835224875"), //mithral
                library.Get<BlueprintWeaponEnchantment>("ab39e7d59dd12f4429ffef5dca88dc7b"), //adamantine
            };

            weapon_shift = Helpers.CreateFeature("WeaponShiftFeature",
                                                 "Weapon Shift",
                                                 "When you use your wild shape ability, any melee weapons you are wielding and proficient with meld into your new form. While in your new form, your natural attacks gain all your primary weapon's material properties.",
                                                 "",
                                                 null,
                                                 FeatureGroup.Feat,
                                                 Helpers.PrerequisiteFeature(first_wildshape_form) 
                                                );

            var buff1 = Helpers.CreateBuff("WeaponShiftBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Helpers.Create<NewMechanics.EnchantmentMechanics.TransferPrimaryHandWeaponEnchantsToPolymorph>(t =>
                                                                                                                                           {
                                                                                                                                               t.transfer_type = TransferType.Only;
                                                                                                                                               t.enchants = enchants1;
                                                                                                                                           }
                                                                                                                                         )
                                          );
           buff1.SetBuffFlags(BuffFlags.HiddenInUi);



            BlueprintWeaponEnchantment[] enchants2 = new BlueprintWeaponEnchantment[]
            {
                library.Get<BlueprintWeaponEnchantment>("c3209eb058d471548928a200d70765e0"), //composite
                library.Get<BlueprintWeaponEnchantment>("c4d213911e9616949937e1520c80aaf3"), //strength thrown
                WeaponEnchantments.summoned_weapon_enchant,
                WeaponEnchantments.master_work,
                //temporary enchants
                library.Get<BlueprintWeaponEnchantment>("d704f90f54f813043a525f304f6c0050"),
                library.Get<BlueprintWeaponEnchantment>("9e9bab3020ec5f64499e007880b37e52"),
                library.Get<BlueprintWeaponEnchantment>("d072b841ba0668846adeb007f623bd6c"),
                library.Get<BlueprintWeaponEnchantment>("6a6a0901d799ceb49b33d4851ff72132"),
                library.Get<BlueprintWeaponEnchantment>("746ee366e50611146821d61e391edf16"),
                //permanent enchants
                library.Get<BlueprintWeaponEnchantment>("d42fc23b92c640846ac137dc26e000d4"),
                library.Get<BlueprintWeaponEnchantment>("eb2faccc4c9487d43b3575d7e77ff3f5"),
                library.Get<BlueprintWeaponEnchantment>("80bb8a737579e35498177e1e3c75899b"),
                library.Get<BlueprintWeaponEnchantment>("783d7d496da6ac44f9511011fc5f1979"),
                library.Get<BlueprintWeaponEnchantment>("bdba267e951851449af552aa9f9e3992"),

            };

            improved_weapon_shift = Helpers.CreateFeature("ImprovedWeaponShiftFeature",
                                                 "Improved Weapon Shift",
                                                 "When you apply a melee weapon’s damage type and properties to your natural attacks using the Weapon Shift feat, your natural attacks also gain the weapon special abilities of the weapon, such as the flaming special ability.\n"
                                                 + "Improved Weapon Shift does not apply the weapon’s enhancement bonuses to your attacks.",
                                                 "",
                                                 null,
                                                 FeatureGroup.Feat,
                                                 Helpers.PrerequisiteFeature(first_wildshape_form),
                                                 Helpers.PrerequisiteFeature(weapon_shift),
                                                 Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 6)
                                                );

            var buff2 = Helpers.CreateBuff("ImprovedWeaponShiftBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Helpers.Create<NewMechanics.EnchantmentMechanics.TransferPrimaryHandWeaponEnchantsToPolymorph>(t =>
                                                                                                                                          {
                                                                                                                                              t.transfer_type = TransferType.Except;
                                                                                                                                              t.enchants = enchants2;
                                                                                                                                          }
                                                                                                                                         )
                                          );
            buff2.SetBuffFlags(BuffFlags.HiddenInUi);


            BlueprintWeaponEnchantment[] enchants3 = new BlueprintWeaponEnchantment[]
            {
                library.Get<BlueprintWeaponEnchantment>("c3209eb058d471548928a200d70765e0"), //composite
                library.Get<BlueprintWeaponEnchantment>("c4d213911e9616949937e1520c80aaf3"), //strength thrown
                WeaponEnchantments.summoned_weapon_enchant,
                WeaponEnchantments.master_work
            };

            greater_weapon_shift = Helpers.CreateFeature("GreaterWeaponShiftFeature",
                                                 "Greater Weapon Shift",
                                                 "When you apply a melee weapon’s damage type and properties to your natural attacks using the Weapon Shift feat, your natural attacks also gain an enhancement bonus on attack and damage rolls equal to the enhancement bonus (if any) of the weapon.",
                                                 "",
                                                 null,
                                                 FeatureGroup.Feat,
                                                 Helpers.PrerequisiteFeature(first_wildshape_form),
                                                 Helpers.PrerequisiteFeature(weapon_shift),
                                                 Helpers.PrerequisiteFeature(improved_weapon_shift),
                                                 Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 8)
                                                );

            var buff3 = Helpers.CreateBuff("GreaterWeaponShiftBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Helpers.Create<NewMechanics.EnchantmentMechanics.TransferPrimaryHandWeaponEnchantsToPolymorph>(t =>
                                                                                                                                          {
                                                                                                                                              t.transfer_type = TransferType.Except;
                                                                                                                                              t.enchants = enchants3;
                                                                                                                                          }
                                                                                                                                         )
                                          );
            buff3.SetBuffFlags(BuffFlags.HiddenInUi);


            foreach (var shape in druid_wild_shapes)
            {
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(shape, buff3, greater_weapon_shift);
                Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuff(shape, Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Common.createContextConditionCasterHasFact(improved_weapon_shift),
                                                                                                                                                      Common.createContextConditionCasterHasFact(greater_weapon_shift, false)),
                                                                                                                  Common.createContextActionApplyBuff(buff2, Helpers.CreateContextDuration(), is_child: true, dispellable: false, is_permanent: true)
                                                                                                                  )
                                                                                  );
                Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuff(shape, Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Common.createContextConditionCasterHasFact(weapon_shift),
                                                                                                                                      Common.createContextConditionCasterHasFact(greater_weapon_shift, false),
                                                                                                                                      Common.createContextConditionCasterHasFact(improved_weapon_shift, false)),
                                                                                                  Common.createContextActionApplyBuff(buff1, Helpers.CreateContextDuration(), is_child: true, dispellable: false, is_permanent: true)
                                                                                                  )
                                                                  );
            }
            library.AddFeats(weapon_shift, improved_weapon_shift, greater_weapon_shift);
        }

        static void fixFormOfTheDragonIWeapons()
        {
            var buff_ids = new string[]
            {
                "268fafac0a5b78c42a58bd9c1ae78bcf",
                "b117bc8b41735924dba3fb23318f39ff",
                "17d330af03f5b3042a4417ab1d45e484",
                "1032d4ffb1c56444ca5bfce2c778614d",
                "a4cc7169fb7e64a4a8f53bdc774341b1",
                "89669cfba3d9c15448c23b79dd604c41",
                "02611a12f38bed340920d1d427865917",
                "294cbb3e1d547f341a5d7ec8500ffa44",
                "feb2ab7613e563e45bcf9f7ffe4e05c6",
                "a6acd3ad1e9fa6c45998d43fd5dcd86d",
            };


            foreach (var id in buff_ids)
            {
                var buff = library.Get<BlueprintBuff>(id);
                buff.ReplaceComponent<Polymorph>(p => { p.MainHand = p.AdditionalLimbs[0]; p.OffHand = p.AdditionalLimbs[1]; p.AdditionalLimbs = p.AdditionalLimbs.Skip(2).ToArray(); });
            }
        }


        internal static void allowToUseArmorInWildshape()
        {
            Main.logger.Log("Enabling Wild Armor.");
            first_wildshape_form.AddComponent(Helpers.CreateAddFact(wild_armor_feature));
        }

        static void createWildArmor()
        {
            allow_wild_armor_buff = Helpers.CreateBuff("AllowWildArmorBuff",
                                                             "",
                                                             "",
                                                             "",
                                                             null,
                                                             null,
                                                             Helpers.Create<WildArmorMechanics.WildArmorLogic>());
            allow_wild_armor_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            wild_armor_feature = Helpers.CreateFeature("WildArmorFeature",
                                                        "Wild Armor",
                                                        "Armor with this special ability usually appears to be made from magically hardened animal pelt. The wearer of a suit of armor or a shield with this ability preserves his armor bonus (and any enhancement bonus) while in a wild shape. Armor and shields with this ability usually appear to be covered in leaf patterns. While the wearer is in a wild shape, the armor cannot be seen.",
                                                        "",
                                                        null,
                                                        FeatureGroup.None);
            wild_armor_feature.HideInUI = true;

            foreach (var wb in druid_wild_shapes)
            {
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(wb, allow_wild_armor_buff, wild_armor_feature);
            }
        }

        static void createEngulf()
        {
            var dmg_action = Helpers.CreateActionDealDamage(DamageEnergyType.Acid, Helpers.CreateContextDiceValue(DiceType.D6, Common.createSimpleContextValue(2)));
            var acid_maw = library.Get<BlueprintAbility>("75de4ded3e731dc4f84d978fe947dc67");

            engulf = Helpers.CreateFeature("EngulfFeature",
                                           "Engulf",
                                           "Giant Flytrap can close its jaws completely around the foe which is at least two size categories smaller than it, by making a grapple combat maneuver check. If it succeeds, it engulfs the prey and inflicts its bite damage plus 2d6 acid damage every turn as the cavity floods with digestive enzymes. The seal formed is airtight, so an engulfed creature risks suffocation. Engulf is a special form of pinning, and an engulfed creature can escape in the same way as he can from being pinned, but since an engulfed creature is contained wholly inside the plant‘s jaws, the flytrap’s victim cannot be targeted by effects or attacks that require line of sight or line of effect. A giant flytrap that is grappling or pinning a foe cannot attack other targets with that bite, but is not otherwise hindered. It can not simultaneously engulf more than 3 foes.",
                                            "",
                                            acid_maw.Icon,
                                            FeatureGroup.None
                                            );

            var engulf_self = Helpers.CreateBuff("EngulfSelfBuff",
                                                 "Foe Engulfed",
                                                 engulf.Description,
                                                 "",
                                                 engulf.Icon,
                                                 null,
                                                 Helpers.Create<BuffExtraAttack>(b => b.Number = -1)
                                                 /*Helpers.CreateAddFactContextActions(newRound: Helpers.CreateConditional(Common.createContextConditionCasterHasFact(engulf, false), 
                                                                                                                         Helpers.Create<ContextActionRemoveSelf>())
                                                                                    )*/
                                                 );

            var remove_engulf_self = Helpers.Create<ContextActionOnContextCaster>(c => c.Actions = Helpers.CreateActionList(Helpers.Create<ContextActionRemoveBuffSingleStack>(r => r.TargetBuff = engulf_self)));
            var engulf_buff = Helpers.CreateBuff("EngulfTargetBuff",
                                                 "Engulfed",
                                                 engulf_self.Description,
                                                 "",
                                                 engulf_self.Icon,
                                                 null,
                                                 Helpers.CreateAddFactContextActions(newRound: new GameAction[] { dmg_action, Helpers.Create<ContextActionDealWeaponDamage>() },
                                                                                     deactivated: new GameAction[] { remove_engulf_self })
                                                 /*Helpers.CreateAddFactContextActions(newRound: new GameAction[] { dmg_action, Helpers.Create<ContextActionDealWeaponDamage>(),
                                                                                                                  Helpers.CreateConditional(Common.createContextConditionCasterHasFact(engulf, false),
                                                                                                                                             Helpers.Create<ContextActionRemoveSelf>())},
                                                                                     deactivated: new GameAction[] { remove_engulf_self })*/
                                                 );

            engulf_self.Stacking = StackingType.Stack;

            var swallow = Helpers.Create<ContextActionSwallowWhole>(c => c.TargetBuff = engulf_buff);
            var apply_engulf_self = Common.createContextActionApplyBuff(engulf_self, Helpers.CreateContextDuration(), is_child: true, is_permanent: true, dispellable: false);
            var swallow_self = Helpers.Create<ContextActionOnContextCaster>(c => c.Actions = Helpers.CreateActionList(apply_engulf_self));
            var engulf_action = Helpers.Create<ContextActionCombatManeuver>(c =>
                                                                            {
                                                                                c.IgnoreConcealment = true;
                                                                                c.Type = Kingmaker.RuleSystem.Rules.CombatManeuver.Grapple;
                                                                                c.OnSuccess = Helpers.CreateActionList(swallow, swallow_self);
                                                                            }
                                                                            );
            var engulf_enabled = Helpers.CreateBuff("EngulfEnabledBuff",
                                                    "Engulf",
                                                    engulf_buff.Description,
                                                    "",
                                                    engulf_buff.Icon,
                                                    null,
                                                    Helpers.CreateAddFactContextActions(deactivated: new GameAction[] {Helpers.Create<CombatManeuverMechanics.ContextActionSpitOut>(),
                                                                                                                       Common.createContextActionRemoveBuff(engulf_self)})
                                                    );

            var engulf_ability = Helpers.CreateActivatableAbility("EnableEngulfActivatableAbility",
                                                                  engulf_enabled.Name,
                                                                  engulf_enabled.Description,
                                                                  "",
                                                                  engulf_enabled.Icon,
                                                                  engulf_enabled,
                                                                  AbilityActivationType.Immediately,
                                                                  CommandType.Free,
                                                                  null);
            engulf_ability.DeactivateImmediately = true;

            var effect_action = Helpers.CreateConditional(new Condition[]{Common.createContextConditionCasterHasFact(engulf_enabled), 
                                                                          Helpers.Create<CombatManeuverMechanics.ContextConditionCasterSizeGreater>(c => c.size_delta = 2),
                                                                          Helpers.Create<CombatManeuverMechanics.ContextConditionCasterBuffRankLess>(c => {c.buff = engulf_self; c.rank = 3;})}, 
                                                                          engulf_action);
            engulf.AddComponent(Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(effect_action)));
            engulf.AddComponent(Helpers.CreateAddFact(engulf_ability));
        }


        static void createToss()
        {
            var charge_buff = library.Get<BlueprintBuff>("f36da144a379d534cad8e21667079066");
            var toss_damage = Helpers.CreateActionDealDamage(PhysicalDamageForm.Bludgeoning, Helpers.CreateContextDiceValue(DiceType.D6, Common.createSimpleContextValue(1)), IgnoreCritical: true);
            var toss_action = Helpers.Create<ContextActionCombatManeuver>(c =>
                                                                        { c.Type = Kingmaker.RuleSystem.Rules.CombatManeuver.Trip;
                                                                          c.OnSuccess = Helpers.CreateActionList(toss_damage);
                                                                        }
                                                                        );

            var toss_buff = Helpers.CreateBuff("TossBuff",
                                               "",
                                               "",
                                               "",
                                               null,
                                               null,
                                               Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(toss_action), check_weapon_range_type: true)
                                               );
            toss_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            toss_feature = Helpers.CreateFeature("TossFeature",
                                         "Toss",
                                         "When creature suceeds on charge attack, it can toss opponent into the air by succeeding on trip combat maneuver check.\n" +
                                         "The victim lands at the same square, falls prone and takes 1d6 points of damage.",
                                         "",
                                         null,
                                         FeatureGroup.None
                                         );
            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(charge_buff, toss_buff, toss_feature);
        }



        static void fixAirElementalDC()
        {
            var dc = Helpers.Create<ContextCalculateAbilityParams>();
            dc.StatType = StatType.Strength;
            
            var effects = new BlueprintAbilityAreaEffect[]{library.Get<BlueprintAbilityAreaEffect>("91f4541e0eb353b4681289cc9615a79d"),
                                                  library.Get<BlueprintAbilityAreaEffect>("ebd2fe081029a6b438aed873607e6375"),
                                                  library.Get<BlueprintAbilityAreaEffect>("c73f9a028b78f6e4d8a709b62f6344e0"),
                                                  library.Get<BlueprintAbilityAreaEffect>("0fb8d185085539e41b11b780bc7d9b9e")
                                                 };

            var dc2 = Helpers.Create<ContextCalculateAbilityParams>();
            dc2.StatType = StatType.Constitution;
            var features = new BlueprintBuff[] {
                                                library.Get<BlueprintBuff>("335dbee275613e946a4de1aaf574d7d9"),
                                                library.Get<BlueprintBuff>("23fafce3910c9df41b64343a2b3acbd1"),
                                                library.Get<BlueprintBuff>("f1f1d30e0e3e0f94cb5216a693da3934"),
                                                library.Get<BlueprintBuff>("f84809d94da65cd41a161c2ee7f4d569"),

                                               // library.Get<BlueprintBuff>("84a0966e2cded1e4cafeadc82cfe7027"),
                                                //library.Get<BlueprintBuff>("d2ac546636e4f5740ad06025c7df088d"),
                                                //library.Get<BlueprintBuff>("280943949a73d794a9137406f7c0ad0d"),
                                                library.Get<BlueprintBuff>("2dd5485ae456fc942b48b90a292b3d8a"),
                                            };
            foreach (var e in effects)
            {    //dc is only computed based on druid class, changed to be based on character level
                e.ReplaceComponent<ContextCalculateAbilityParamsBasedOnClass>(dc);
            }

            foreach (var f in features)
            {    //dc is only computed based on druid class, changed to be based on character level
                f.RemoveComponents<ContextCalculateAbilityParamsBasedOnClass>();
                f.AddComponent(dc2);
            }
        }


        static void fixDruid()
        {
            string description = "At 4th level, a druid gains the ability to turn herself into a wolf or a leopard and back again once per day. The effect lasts for 1 hour per druid level, or until she changes back.\nChanging form is a standard action and doesn't provoke an attack of opportunity. A druid can use this ability an additional time per day at 6th level and every two levels thereafter, for a total of eight times at 18th level.\nAt 6th level, a druid can use wild shape to change into a bear, dire wold or a small elemental. At 8th level, a druid can use wild shape to change into a smilodon, mastodon, mandragora or a medium elemental. At 10th level, a druid can use wild shape to change into a shambling mound or a large elemental. At 12th level, a druid can use wild shape to change into a giant flytrap, treant or a huge elemental.\nFor the feyspeaker archetype, all level prerequisites are increased by 2.";
            var wildshape_wolf = library.Get<BlueprintAbility>("ac8811714a45a5948b27208538ce4f03");
            var wolf_feature = library.Get<BlueprintFeature>("19bb148cb92db224abb431642d10efeb");

            var wildshape_leopard = library.Get<BlueprintAbility>("92c47b04f6c9aa44abf1693b32554804");

            var wildshape_bear_buff = library.CopyAndAdd<BlueprintBuff>(bear_form.AssetGuid, "DruidWildshapeIIBearBuff", "");
            wildshape_bear_buff.SetName("Wild Shape (Bear)");
            var wildshape_bear = replaceForm(wildshape_wolf, wildshape_bear_buff, "DruidWildshapeIIBearAbility", wildshape_bear_buff.Name, bear_form_spell.Description);
            //fix thundering claw
            var thudnering_conditional = (library.Get<BlueprintFeature>("f418b53b2a597b54b810699e9f68e061").GetComponent<AddInitiatorAttackWithWeaponTrigger>().Action.Actions[0] as Conditional);
            (thudnering_conditional.ConditionsChecker.Conditions[0] as ContextConditionCasterHasFact).Fact = wildshape_bear_buff;

            var wildshape_dire_wolf_buff = library.CopyAndAdd<BlueprintBuff>(dire_wolf_form.AssetGuid, "DruidWildshapeIIDireWolfBuff", "");
            wildshape_dire_wolf_buff.SetName("Wild Shape (Dire Wolf)");
            var wildshape_dire_wolf = replaceForm(wildshape_wolf, wildshape_dire_wolf_buff, "DruidWildshapeIIDireWolfAbility", wildshape_dire_wolf_buff.Name, dire_wolf_form_spell.Description);

            var wildshape_smilodon = library.Get<BlueprintAbility>("32f1f208ad635224f89ef158140ab509");

            var wildshape_mastodon_buff = library.CopyAndAdd<BlueprintBuff>(mastodon_form.AssetGuid, "DruidWildshapeIIIMastodonBuff", "");
            wildshape_mastodon_buff.SetName("Wild Shape (Mastodon)");
            var wildshape_mastodon = replaceForm(wildshape_wolf, wildshape_mastodon_buff, "DruidWildshapeIIIMastodonAbility", wildshape_mastodon_buff.Name, mastodon_form_spell.Description);

            var wildshape_mandragora_buff = library.CopyAndAdd<BlueprintBuff>(mandragora_form.AssetGuid, "DruidWildshapeIIIMandragoraBuff", "");
            wildshape_mandragora_buff.SetName("Wild Shape (Mandragora)");
            var wildshape_mandragora = replaceForm(wildshape_wolf, wildshape_mandragora_buff, "DruidWildshapeIIIMandragoraAbility", wildshape_mandragora_buff.Name, plant_shapeI.Description);

            var wildshape_shambling_mound = library.Get<BlueprintAbility>("943d41b6aaef1dc4e82f115118dbf902");

            var wildshape_flytrap_buff = library.CopyAndAdd<BlueprintBuff>(giant_flytrap_form.AssetGuid, "DruidWildshapeVGiantFlytrapBuff", "");
            wildshape_flytrap_buff.SetName("Wild Shape (Giant Flytrap)");
            var wildshape_flytrap = replaceForm(wildshape_wolf, wildshape_flytrap_buff, "DruidWildshapeVGiantFlytrapAbility", wildshape_flytrap_buff.Name, giant_flytrap_form_spell.Description);

            var wildshape_treant_buff = library.CopyAndAdd<BlueprintBuff>(treant_form.AssetGuid, "DruidWildshapeVTreantBuff", "");
            wildshape_treant_buff.SetName("Wild Shape (Treant)");
            var wildshape_treant = replaceForm(wildshape_wolf, wildshape_treant_buff, "DruidWildshapeVTreantAbility", wildshape_treant_buff.Name, treant_form_spell.Description);

            var leopard_feature = library.Get<BlueprintFeature>("c4d651bc0d4eabd41b08ee81bfe701d8");
            //leopard_feature.AddComponent(Helpers.CreateAddAbilityResource(library.Get<BlueprintAbilityResource>("ae6af4d58b70a754d868324d1a05eda4")));

            var bear_feature = library.Get<BlueprintFeature>("1368c7ce69702444893af5ffd3226e19");
            bear_feature.GetComponent<AddFacts>().Facts[0] = wildshape_bear;

            var shambling_mound_feature = library.Get<BlueprintFeature>("0f31b23c2ab39354bbde4e33e8151495");
            var smilodon_feature = library.Get<BlueprintFeature>("253c0c0d00e50a24797445f20af52dc8");
            var dire_wolf_feature = createWildshapeFeature(wildshape_dire_wolf, description);
            var mastodon_feature = createWildshapeFeature(wildshape_mastodon, description);
            var mandragora_feature = createWildshapeFeature(wildshape_mandragora, description);
            var flytrap_feature = createWildshapeFeature(wildshape_flytrap, description);
            var treant_feature = createWildshapeFeature(wildshape_treant, description);

            var small_elemental_feature = library.Get<BlueprintFeature>("bddd46a6f6a3e6e4b99008dcf5271c3b");
            var medium_elemental_feature = library.Get<BlueprintFeature>("4d517e670ed4b6e4282d52855237a44f");
            var large_elemental_feature = library.Get<BlueprintFeature>("1186fc7362560c94bad3de6338cc509e");
            var huge_elemental_feature = library.Get<BlueprintFeature>("fe58dd496a36e274b86958f4677071b2");

            var medium_elemental_add = library.Get<BlueprintFeature>("6e4b88e2a044c67469c038ac2f09d061");
            var large_elemental_add = library.Get<BlueprintFeature>("e66154511a6f9fc49a9de644bd8922db");
            medium_elemental_add.SetDescription(description);
            large_elemental_add.SetDescription(description);

            animal_wildshapes.Add(wildshape_wolf);
            animal_wildshapes.Add(wildshape_leopard);
            animal_wildshapes.Add(wildshape_bear);
            animal_wildshapes.Add(wildshape_dire_wolf);
            animal_wildshapes.Add(wildshape_smilodon);
            animal_wildshapes.Add(wildshape_mastodon);

            var shape_features = new BlueprintFeature[] {
                                                        wolf_feature,
                                                        leopard_feature,
                                                        bear_feature,
                                                        shambling_mound_feature,    
                                                        smilodon_feature,        
                                                        small_elemental_feature,
                                                        medium_elemental_feature,
                                                        large_elemental_feature,
                                                        huge_elemental_feature
                                                        };
            foreach (var s in shape_features)
            {
                s.SetDescription(description);
            }


            druid_wild_shapes = new BlueprintBuff[] {library.Get<BlueprintBuff>("470fb1a22e7eb5849999f1101eacc5dc"), //wolf
                                                                 library.Get<BlueprintBuff>("8abf1c437ebee8048a4a3335efc27eb3"), //leopard
                                                                 wildshape_bear_buff ,
                                                                 wildshape_dire_wolf_buff,
                                                                 library.Get<BlueprintBuff>("49a77c5c5266c42429f7afbb038ada60"), //smilodon
                                                                 wildshape_mastodon_buff,
                                                                 wildshape_mandragora_buff,
                                                                 library.Get<BlueprintBuff>("0d29c50c956e82d4eae56710987de9f7"),//shambling mound
                                                                 wildshape_flytrap_buff,
                                                                 //air
                                                                 library.Get<BlueprintBuff>("eb52d24d6f60fc742b32fe943b919180"),
                                                                 library.Get<BlueprintBuff>("814bc75e74f969641bf110addf076ff9"),
                                                                 library.Get<BlueprintBuff>("65fdf187fea97c94b9cf4ff6746901a6"),
                                                                 library.Get<BlueprintBuff>("dc1ef6f6d52b9fd49bc0696ab1a4f18b"),
                                                                 //earth
                                                                 library.Get<BlueprintBuff>("f0826c3794c158c4cbbe9ceb4210d6d6"),
                                                                 library.Get<BlueprintBuff>("bf145574939845d43b68e3f4335986b4"),
                                                                 library.Get<BlueprintBuff>("e76500bc1f1f269499bf027a5aeb1471"),
                                                                 library.Get<BlueprintBuff>("add5378a75feeaf4384766da10ddc40d"),
                                                                 //fire
                                                                 library.Get<BlueprintBuff>("e85abd773dbce30498efa8da745d7ca7"),
                                                                 library.Get<BlueprintBuff>("7f30b0f7f3c4b6748a2819611fb236f8"),
                                                                 library.Get<BlueprintBuff>("3e3f33fb3e581ab4e8923a5eabd15923"),
                                                                 library.Get<BlueprintBuff>("9e6b7b058bc74fc45903679adcab8553"),
                                                                 //water
                                                                 library.Get<BlueprintBuff>("ea2cd08bdf2ca1c4f8a8870804790cd7"),
                                                                 library.Get<BlueprintBuff>("5993b78c793667e45bf0380e9275fab7"),
                                                                 library.Get<BlueprintBuff>("c5925e7b9e7fc2e478526b4cfc8c6427"),
                                                                 library.Get<BlueprintBuff>("9c58cfcad11f7fd4cb85e22187fddac7"),
                                                                };

            var druid_progression = library.Get<BlueprintProgression>("01006f2ac8866764fb7af135e73be81c");
            druid_progression.LevelEntries[3].Features.Add(leopard_feature);
            druid_progression.LevelEntries[5].Features[0] = dire_wolf_feature;
            druid_progression.LevelEntries[5].Features.Insert(0, bear_feature);
            druid_progression.LevelEntries[7].Features[0] = mastodon_feature;
            druid_progression.LevelEntries[7].Features.Insert(0, smilodon_feature);
            druid_progression.LevelEntries[7].Features.Add(mandragora_feature);
            druid_progression.LevelEntries[9].Features.Remove(smilodon_feature);
            druid_progression.LevelEntries[11].Features.Add(flytrap_feature);
            druid_progression.LevelEntries[11].Features.Add(treant_feature);
            druid_progression.UIGroups[0].Features.Remove(library.Get<BlueprintFeature>("19bb148cb92db224abb431642d10efeb"));//remove wolf feature
            var wildshape_ui_groups = new UIGroup[] {Helpers.CreateUIGroup(wolf_feature, dire_wolf_feature, smilodon_feature),
                                                     Helpers.CreateUIGroup(leopard_feature, bear_feature, mastodon_feature, treant_feature),
                                                     Helpers.CreateUIGroup(mandragora_feature, shambling_mound_feature, flytrap_feature),
                                                     Helpers.CreateUIGroup(small_elemental_feature, medium_elemental_add , large_elemental_add, huge_elemental_feature)};
            druid_progression.UIGroups = druid_progression.UIGroups.AddToArray(wildshape_ui_groups);


            var feyspeaker = library.Get<BlueprintArchetype>("da69747aa3dd0044ebff5f3d701cdde3");
            feyspeaker.AddFeatures[2].Features.Add(leopard_feature);
            feyspeaker.AddFeatures[3].Features[1] = dire_wolf_feature;
            feyspeaker.AddFeatures[3].Features.Add(bear_feature);
            feyspeaker.AddFeatures[4].Features[1] = mastodon_feature;
            feyspeaker.AddFeatures[4].Features.Add(smilodon_feature);
            feyspeaker.AddFeatures[4].Features.Add(mandragora_feature);
            feyspeaker.AddFeatures[5].Features[1] = shambling_mound_feature;
            feyspeaker.AddFeatures[6].Features.Add(flytrap_feature);
            feyspeaker.AddFeatures[6].Features.Add(treant_feature);


            feyspeaker.RemoveFeatures[1].Features.Add(leopard_feature);
            feyspeaker.RemoveFeatures[2].Features[0] = bear_feature;
            feyspeaker.RemoveFeatures[2].Features.Add(dire_wolf_feature);
            feyspeaker.RemoveFeatures[3].Features[0] = smilodon_feature;
            feyspeaker.RemoveFeatures[3].Features.Add(mandragora_feature);
            feyspeaker.RemoveFeatures[3].Features.Add(mastodon_feature);
            feyspeaker.RemoveFeatures[4].Features[0] = shambling_mound_feature;
            feyspeaker.RemoveFeatures = feyspeaker.RemoveFeatures.AddToArray(Helpers.LevelEntry(12, flytrap_feature, treant_feature, huge_elemental_feature));

            first_wildshape_form = leopard_feature;
            //fix natural spell requirement
            var natural_spell = library.Get<BlueprintFeature>("c806103e27cce6f429e5bf47067966cf");
            natural_spell.GetComponent<PrerequisiteFeature>().Feature = first_wildshape_form;

            //add wolf form if it is missing
            Action<UnitDescriptor> save_game_fix = delegate (UnitDescriptor u)
            {
                if (u.HasFact(leopard_feature) && !u.HasFact(wolf_feature))
                {
                    u.AddFact(wolf_feature);
                }
            };
            SaveGameFix.save_game_actions.Add(save_game_fix);
        }


        static BlueprintFeature createWildshapeFeature(BlueprintAbility wildshape_ability, string description)
        {
            var wildshape_wolf_feature = library.Get<BlueprintFeature>("19bb148cb92db224abb431642d10efeb");
            var f = Helpers.CreateFeature(wildshape_ability.name.Replace("Ability", "Feature"),
                                               wildshape_ability.Name,
                                               description,
                                               "",
                                               wildshape_wolf_feature.Icon,
                                               FeatureGroup.None,
                                               Helpers.CreateAddFact(wildshape_ability)
                                               );
            return f;
        }


        static void fixTransmuter()
        {
            var bear_shape = library.Get<BlueprintActivatableAbility>("e634c83bf9a81054a85ccb44f8152896");
            bear_shape.Buff.ComponentsArray = bear_form.ComponentsArray;
            bear_shape.Buff.SetDescription(bear_form.Description);
            bear_shape.SetDescription(bear_form_spell.Description);

            var wolf_shape = createChangeShapeAbility(wolf_form, "TransmutationSchoolChangeShapeWolf", "Change Shape (Wolf)", wolf_form_spell.Description);
            var dire_wolf_shape = createChangeShapeAbility(dire_wolf_form, "TransmutationSchoolChangeShapeDireWolf", "Change Shape (Dire Wolf)", dire_wolf_form_spell.Description);
            var mastodon_shape = createChangeShapeAbility(mastodon_form, "TransmutationSchoolChangeShapeMastodon", "Change Shape (Mastodon)", mastodon_form_spell.Description);
            var smilodon_shape = createChangeShapeAbility(smilodon_form, "TransmutationSchoolChangeShapeSmilodon", "Change Shape (Smilodon)", smilodon_form_spell.Description);

            var change_shapeI = library.Get<BlueprintFeature>("aeb56418768235640a3ee858d5ee05e8");
            var factsI = change_shapeI.GetComponent<AddFacts>().Facts.ToList();
            factsI.Insert(1, wolf_shape);
            factsI.Insert(1, bear_shape);
            factsI.Insert(2, dire_wolf_shape);
            change_shapeI.GetComponent<AddFacts>().Facts = factsI.ToArray();
            var change_shapeII = library.Get<BlueprintFeature>("b699aa5a98519ee469afddb71b9a8fd0");
            var factsII = change_shapeII.GetComponent<AddFacts>().Facts.ToList();
            factsII[0] = mastodon_shape;
            factsII.Insert(1, smilodon_shape);
            change_shapeII.GetComponent<AddFacts>().Facts = factsII.ToArray();
        }


        static BlueprintActivatableAbility createChangeShapeAbility(BlueprintBuff buff, string name, string display_name, string description)
        {
            var shape = library.CopyAndAdd<BlueprintActivatableAbility>("e634c83bf9a81054a85ccb44f8152896", name + "Ability", "");
            var shape_buff = library.CopyAndAdd<BlueprintBuff>("a5d38d44a92ff4a44b9583d1d196ba64", name + "Buff", "");
            shape_buff.SetDescription(buff.Description);
            shape_buff.SetName(display_name);
            shape_buff.ComponentsArray = buff.ComponentsArray;
            shape.SetName(display_name);
            shape.SetDescription(description);
            shape.Buff = shape_buff;
            return shape;
        }


        static void createShapechange()
        {
            var disintegrate = library.Get<BlueprintAbility>("4aa7942c3e62a164387a73184bca3fc1");
            BlueprintAbility[] forms = {library.Get<BlueprintAbility>("c12c98cfd3cde22488f09e9618ff7435"), //black dragon
                                        library.Get<BlueprintAbility>("5c6791821d8a2ae4cb134a4bd925de50"), //blue dragon
                                        library.Get<BlueprintAbility>("4fda7f6a51d989a4794ff4401178b5fe"), //brass dragon
                                        library.Get<BlueprintAbility>("04d7a690e60feca40890bc3db144b335"), //bronze dragon
                                        library.Get<BlueprintAbility>("ab069196fb37dfc4e848fe482f7f620d"), //copper dragon
                                        library.Get<BlueprintAbility>("c511266a705a6e94186cb51e0503775f"), //gold dragon
                                        library.Get<BlueprintAbility>("00b3a04140c39b447925fe5a79522087"), //green dragon
                                        library.Get<BlueprintAbility>("2c1ee791f53ed4f42bd86d8659c638c0"), //red dragon
                                        library.Get<BlueprintAbility>("0b1e76be6f786ca45b2ac247ac3a278e"), //silver dragon
                                        library.Get<BlueprintAbility>("ded61c155aaa39440be67f877623378e"), //white dragon
                                        athach_form_form_spell,
                                        storm_giant_form_spell,
                                        troll_form_spell,
                                        fire_giant_form_spell,
                                        frost_giant_form_spell,
                                        giant_flytrap_form_spell,
                                        treant_form_spell,
                                        library.Get<BlueprintAbility>("ee63301f83c76694692d4704d8a05bdc"),//air elemental
                                        library.Get<BlueprintAbility>("facdc8851a0b3f44a8bed50f0199b83c"),//earth elemental
                                        library.Get<BlueprintAbility>("c281eeecc554b72449fef43924e522ce"),//fire elemental
                                        library.Get<BlueprintAbility>("96d2ab91f2d2329459a8dab496c5bede")//water elemental
                                      };


            List<BlueprintAbility> shapechange_forms = new List<BlueprintAbility>();
            foreach (var f in forms)
            {
                var form = library.CopyAndAdd<BlueprintAbility>(f.AssetGuid, "Shapechange" + f.name, "");
                form.Type = AbilityType.Supernatural;
                form.ActionType = CommandType.Free;
                form.SetName("Shapechange: " + f.Name);
                form.RemoveComponents<SpellListComponent>();
                var apply_buff = ((Kingmaker.UnitLogic.Mechanics.Actions.ContextActionApplyBuff)form.GetComponent<AbilityEffectRunAction>().Actions.Actions[0]).CreateCopy();
                apply_buff.Permanent = true; //permanent form
                form.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(apply_buff));
                shapechange_forms.Add(form);
            }

            var ability = library.CopyAndAdd<BlueprintAbility>("940a545a665194b48b722c1f9dd78d53", "ShapechangeAbility", "");
            ability.SetIcon(disintegrate.Icon);
            ability.SetName("Shapechange Ability");
            ability.SetDescription("This spell allows you to take the form of a wide variety of creatures.This spell can function as elemental body IV, form of the dragon III, giant form I, giant form II and plant shape III depending on what form you take.You can change form once each round as a free action.The change takes place either immediately before your regular action or immediately after it, but not during the action.");
            ability.RemoveComponents<SpellListComponent>();
            ability.Type = AbilityType.Supernatural;
            ability.ActionType = CommandType.Free;
            ability.ReplaceComponent<AbilityVariants>(Helpers.CreateAbilityVariants(ability, shapechange_forms));


            var buff = Helpers.CreateBuff("ShapechangeBuff",
                              "Shapechange",
                              ability.Description,
                              "",
                              ability.Icon,
                              null,
                              Helpers.CreateAddFact(ability),
                              Helpers.CreateAddFactContextActions(deactivated: Common.createContextActionRemoveBuffsByDescriptor(SpellDescriptor.Polymorph)),
                              Helpers.Create<ReplaceAbilityParamsWithContext>(r => r.Ability = ability)
                              );



            shapechange = Helpers.CreateAbility("ShapechangeSpell",
                                              "Shapechange",
                                              ability.Description,
                                              "",
                                              ability.Icon,
                                              AbilityType.Spell,
                                              CommandType.Standard,
                                              AbilityRange.Personal,
                                              Helpers.tenMinPerLevelDuration,
                                              Helpers.savingThrowNone,
                                              beast_shape_prototype.GetComponent<AbilitySpawnFx>(),
                                              beast_shape_prototype.GetComponent<ContextRankConfig>(),
                                              beast_shape_prototype.GetComponent<SpellComponent>(),
                                              Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(rate: DurationRate.TenMinutes), is_from_spell: true))
                                           );

            shapechange.ResourceAssetIds = beast_shape_prototype.ResourceAssetIds;
            shapechange.AvailableMetamagic = beast_shape_prototype.AvailableMetamagic;
            shapechange.Animation = beast_shape_prototype.Animation;
            shapechange.AnimationStyle = beast_shape_prototype.AnimationStyle;

            shapechange.MaterialComponent = library.Get<BlueprintAbility>("da1b292d91ba37948893cdbe9ea89e28").MaterialComponent; //from legendary proportions

            shapechange.AddToSpellList(Helpers.wizardSpellList, 9);
            shapechange.AddToSpellList(Helpers.druidSpellList, 9);
            Helpers.AddSpellAndScroll(shapechange, "bb2f172e429b40840a7dc25bc83732cb"); //disintegrate

            //replace 9th level spell in animal domain
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("23d2f87aa54c89f418e68e790dba11e0"), shapechange, 9);
        }

        static void createGiantFormII()
        {
            var defensive_stance = library.Get<BlueprintFeature>("2a6a2f8e492ab174eb3f01acf5b7c90a");

            storm_giant_form = Helpers.CreateBuff("GiantShapeIIStormGiantBuff",
                                "Giant Form (Storm Giant)",
                                "You are in storm giant form now. You have a + 8 size bonus to your Strength, +6 size bonus to your Constitution, -2 penalty to Dexterity and a + 6 natural armor bonus. Your movement speed is increased by 10 feet. You also have two 2d6 slam attacks and electricity resistance 20.",
                                "",
                                defensive_stance.Icon,
                                null,
                                Common.createChangeUnitSize(Size.Huge),
                                Common.createAddGenericStatBonus(8, ModifierDescriptor.Size, StatType.Strength),
                                Common.createAddGenericStatBonus(10, ModifierDescriptor.Enhancement, StatType.Speed),
                                Common.createAddGenericStatBonus(6, ModifierDescriptor.Size, StatType.Constitution),
                                Common.createAddGenericStatBonus(-2, ModifierDescriptor.Size, StatType.Dexterity),
                                Common.createAddGenericStatBonus(6, ModifierDescriptor.NaturalArmor, StatType.AC),
                                Helpers.CreateSpellDescriptor(SpellDescriptor.Polymorph),
                                Common.createEnergyDR(20, DamageEnergyType.Electricity),
                                Common.createEmptyHandWeaponOverride(library.Get<BlueprintItemWeapon>("767e6932882a99c4b8ca95c88d823137")),//slam
                                Helpers.CreateAddFact(turn_back)
                              );
            storm_giant_form_spell = replaceForm(beast_shape_prototype, storm_giant_form, "GiantFormIIStormGiantAbility", "Giant Form II (Storm Giant)",
                         "You acquire storm giant features. Your size changes to huge. You gain a + 8 size bonus to your Strength, + 6 size bonus to your Constitution, -2 penalty to Dexterity and a + 6 natural armor bonus. Your movement speed is increased by 10 feet. You also gain two 2d6 slam attacks and electricity resistance 20.");
            storm_giant_form_spell.RemoveComponents<SpellListComponent>();
            storm_giant_form_spell.SetIcon(defensive_stance.Icon);


            athach_form = Helpers.CreateBuff("GiantShapeIIAthachBuff",
                    "Giant Form (Athach)",
                    "You are in athach form now. You have a + 8 size bonus to your Strength, +6 size bonus to your Constitution, -2 penalty to Dexterity and a + 6 natural armor bonus. Your movement speed is increased by 10 feet. You also have two 1d8 slam attacks, one 2d6 bite attack, one 1d10 secondary claw attack and poison ability.",
                    "",
                    defensive_stance.Icon,
                    null,
                    Common.createChangeUnitSize(Size.Huge),
                    Common.createAddGenericStatBonus(8, ModifierDescriptor.Size, StatType.Strength),
                    Common.createAddGenericStatBonus(10, ModifierDescriptor.Enhancement, StatType.Speed),
                    Common.createAddGenericStatBonus(6, ModifierDescriptor.Size, StatType.Constitution),
                    Common.createAddGenericStatBonus(-2, ModifierDescriptor.Size, StatType.Dexterity),
                    Common.createAddGenericStatBonus(6, ModifierDescriptor.NaturalArmor, StatType.AC),
                    Helpers.CreateSpellDescriptor(SpellDescriptor.Polymorph),
                    Common.createEmptyHandWeaponOverride(athach_slam),
                    Common.createAddSecondaryAttacks(athach_bite, athach_claw),
                    Helpers.CreateAddFacts(library.Get<BlueprintFeature>("366f54decfc4c08438fa66427cd92939"), //poison
                                           turn_back)
                  );
            athach_form_form_spell = replaceForm(beast_shape_prototype, athach_form, "GiantFormIIAthachAbility", "Giant Form II (Athach)",
                         "You acquire athach features. Your size changes to huge. You gain a + 8 size bonus to your Strength, + 6 size bonus to your Constitution, -2 penalty to Dexterity and a + 6 natural armor bonus. Your movement speed is increased by 10 feet. You also have two 1d8 slam attacks, one 2d6 bite attack, one 1d10 secondary claw attack and poison ability.");
            athach_form_form_spell.RemoveComponents<SpellListComponent>();
            athach_form_form_spell.SetIcon(defensive_stance.Icon);

            giant_formII = library.CopyAndAdd<BlueprintAbility>("940a545a665194b48b722c1f9dd78d53", "GiantFormIISpell", "");
            giant_formII.SetIcon(defensive_stance.Icon);
            giant_formII.SetName("Giant Form II");
            giant_formII.SetDescription("You can aquire features of Huge Athach or Storm Giant.");
            giant_formII.ReplaceComponent<AbilityVariants>(Helpers.CreateAbilityVariants(giant_formII, athach_form_form_spell, storm_giant_form_spell));

            giant_formII.AddToSpellList(Helpers.wizardSpellList, 8);
            Helpers.AddSpellAndScroll(giant_formII, "2778cd9dc966c3641afa1e455969a022"); //legendary proportions
        }


        static void createGiantFormI()
        {
            var defensive_stance = library.Get<BlueprintFeature>("2a6a2f8e492ab174eb3f01acf5b7c90a");
            troll_form = Helpers.CreateBuff("GiantShapeITrollFormBuff",
                                            "Giant Form (Troll)",
                                            "You are in troll form now. You have a + 6 size bonus to your Strength, +4 size bonus to your Constitution, -2 penalty to Dexterity and a + 4 natural armor bonus. You also have two 1d6 claw attacks, one 1d8 bite, regeneration 5 (acid or fire) and rend ability.",
                                            "",
                                            defensive_stance.Icon,
                                            null,
                                            Common.createChangeUnitSize(Size.Large),
                                            Common.createAddGenericStatBonus(6, ModifierDescriptor.Size, StatType.Strength),
                                            Common.createAddGenericStatBonus(4, ModifierDescriptor.Size, StatType.Constitution),
                                            Common.createAddGenericStatBonus(-2, ModifierDescriptor.Size, StatType.Dexterity),
                                            Common.createAddGenericStatBonus(4, ModifierDescriptor.NaturalArmor, StatType.AC),
                                            Helpers.CreateSpellDescriptor(SpellDescriptor.Polymorph),
                                            Common.createEmptyHandWeaponOverride(library.Get<BlueprintItemWeapon>("de21b6c00e6adaa409a6e7c2ae9f87f4")),//claws
                                            Common.createAddSecondaryAttacks(library.Get<BlueprintItemWeapon>("1f8a2e1e5e078014baebc90c2c46796f")), //bite
                                            library.Get<BlueprintBuff>("ac95eba4690dbca46b9a2ab18f656d4f").GetComponent<AddEffectRegeneration>(), //regeneration
                                            Helpers.CreateAddFacts(library.Get<BlueprintFeature>("e80ba26500d22e546baba542032aad0d"), //rend
                                                                   turn_back)
                                          );
            troll_form_spell = replaceForm(smilodon_form_spell, troll_form, "GiantFormITrollAbility", "Giant Form I (Troll)",
                         "You acquire troll features. Your size changes to large. You gain a + 6 size bonus to your Strength, +4 size bonus to your Constitution, -2 penalty to Dexterity and a + 4 natural armor bonus. You also gain two 1d6 claw attacks, one 1d8 bite, regeneration 5 (acid or fire) and rend ability.\nYou can continue use your equipment and cast spells while in this form.");
            troll_form_spell.RemoveComponents<SpellListComponent>();
            troll_form_spell.SetIcon(defensive_stance.Icon);


            fire_giant_form = Helpers.CreateBuff("GiantShapeIFireGiantFormBuff",
                                "Giant Form (Fire Giant)",
                                "You are in fire giant form now. You have a + 6 size bonus to your Strength, +4 size bonus to your Constitution, -2 penalty to Dexterity and a + 4 natural armor bonus. You also have two 1d8 slam attacks, fire resistance 20 and cold vulnerability.",
                                "",
                                defensive_stance.Icon,
                                null,
                                Common.createChangeUnitSize(Size.Large),
                                Common.createAddGenericStatBonus(6, ModifierDescriptor.Size, StatType.Strength),
                                Common.createAddGenericStatBonus(4, ModifierDescriptor.Size, StatType.Constitution),
                                Common.createAddGenericStatBonus(-2, ModifierDescriptor.Size, StatType.Dexterity),
                                Common.createAddGenericStatBonus(4, ModifierDescriptor.NaturalArmor, StatType.AC),
                                Helpers.CreateSpellDescriptor(SpellDescriptor.Polymorph),
                                Common.createAddEnergyVulnerability(DamageEnergyType.Cold),
                                Common.createEnergyDR(20, DamageEnergyType.Fire),
                                Common.createEmptyHandWeaponOverride(library.Get<BlueprintItemWeapon>("767e6932882a99c4b8ca95c88d823137")),//slam
                                Helpers.CreateAddFact(turn_back)
                              );
            fire_giant_form_spell = replaceForm(smilodon_form_spell, fire_giant_form, "GiantFormIFireGiantAbility", "Giant Form I (Fire Giant)",
                         "You acquire fire giant features. Your size changes to large. You gain a + 6 size bonus to your Strength, +4 size bonus to your Constitution, -2 penalty to Dexterity and a + 4 natural armor bonus. You also gain two 1d8 slam attacks, fire resistance 20 and cold vulnerability.");
            fire_giant_form_spell.RemoveComponents<SpellListComponent>();
            fire_giant_form_spell.SetIcon(defensive_stance.Icon);


            frost_giant_form = Helpers.CreateBuff("GiantShapeIFrostGiantFormBuff",
                    "Giant Form (Frost Giant)",
                    "You are in frost giant form now. You have a + 6 size bonus to your Strength, +4 size bonus to your Constitution, -2 penalty to Dexterity and a + 4 natural armor bonus. You also have two 1d8 slam attacks, cold resistance 20 and fire vulnerability.",
                    "",
                    defensive_stance.Icon,
                    null,
                    Common.createChangeUnitSize(Size.Large),
                    Common.createAddGenericStatBonus(6, ModifierDescriptor.Size, StatType.Strength),
                    Common.createAddGenericStatBonus(4, ModifierDescriptor.Size, StatType.Constitution),
                    Common.createAddGenericStatBonus(-2, ModifierDescriptor.Size, StatType.Dexterity),
                    Common.createAddGenericStatBonus(4, ModifierDescriptor.NaturalArmor, StatType.AC),
                    Helpers.CreateSpellDescriptor(SpellDescriptor.Polymorph),
                    Common.createAddEnergyVulnerability(DamageEnergyType.Fire),
                    Common.createEnergyDR(20, DamageEnergyType.Cold),
                    Common.createEmptyHandWeaponOverride(library.Get<BlueprintItemWeapon>("767e6932882a99c4b8ca95c88d823137")),//slam
                    Helpers.CreateAddFact(turn_back)
                  );
            frost_giant_form_spell = replaceForm(smilodon_form_spell, frost_giant_form, "GiantFormIFrostGiantAbility", "Giant Form I (Frost Giant)",
                         "You acquire frost giant features. Your size changes to large. You gain a + 6 size bonus to your Strength, +4 size bonus to your Constitution, -2 penalty to Dexterity and a + 4 natural armor bonus. You also gain two 1d8 slam attacks, cold resistance 20 and fire vulnerability.");
            frost_giant_form_spell.RemoveComponents<SpellListComponent>();
            frost_giant_form_spell.SetIcon(defensive_stance.Icon);


            giant_formI = library.CopyAndAdd<BlueprintAbility>("940a545a665194b48b722c1f9dd78d53", "GiantFormISpell", "");
            giant_formI.SetIcon(defensive_stance.Icon);
            giant_formI.SetName("Giant Form I");
            giant_formI.SetDescription("You can aquire features of Troll, Fire Giant or Frost Giant.");
            giant_formI.ReplaceComponent<AbilityVariants>(Helpers.CreateAbilityVariants(giant_formI, troll_form_spell, fire_giant_form_spell, frost_giant_form_spell));

            giant_formI.RemoveComponents<SpellListComponent>();
            giant_formI.AddToSpellList(Helpers.wizardSpellList, 7);
            giant_formI.AddToSpellList(Helpers.alchemistSpellList, 6);
            Helpers.AddSpellAndScroll(giant_formI, "2778cd9dc966c3641afa1e455969a022"); //legendary proportions
        }


        static void fixLegendaryProportions()
        {
            var legendary_proportions_buff = library.Get<BlueprintBuff>("4ce640f9800d444418779a214598d0a3");
            legendary_proportions_buff.GetComponent<ChangeUnitSize>().SizeDelta = 1;
            var legendary_proportions_spell = library.Get<BlueprintAbility>("da1b292d91ba37948893cdbe9ea89e28");
            legendary_proportions_spell.SetDescription("You call upon the primordial power of ancient megafauna to boost the size of your target. Because of its connection to living creatures of the distant past, the spell does not function on outsiders, undead, and summoned creatures. Your target grows to legendary proportions, increasing in size by one category.The creature's height doubles and its weight increases by a factor of 8. The target gains a +6 size bonus to its Strength score and a +4 size bonus to its Constitution score. It gains a +6 bonus to its natural armor, and DR 10/adamantine. Melee and ranged weapons used by this creature deal more damage.");
        }


        static void createPlantShapeI()
        {
            //fix poison
            var mandragora_poison = library.Get<BlueprintFeature>("ec44af8b3449c5b4889145dbfc246a00");
            var on_save = (ContextActionSavingThrow)mandragora_poison.GetComponent<AddInitiatorAttackWithWeaponTrigger>().Action.Actions[0];
            var effect = (ContextActionConditionalSaved)on_save.Actions.Actions[0];
            var apply_buff = (ContextActionApplyBuff)effect.Failed.Actions[0];
            apply_buff.DurationValue.DiceCountValue = Common.createSimpleContextValue(1);
            apply_buff.DurationValue.DiceType = DiceType.D4;

            var entangle = library.Get<BlueprintAbility>("0fd00984a2c0e0a429cf1a911b4ec5ca");
            BlueprintUnit mandragora = library.Get<BlueprintUnit>("f30beec3bfcfc374883cbbc700c6ad47");
            mandragora_form = createPolymorphForm("PlantshapeIMandragoraBuff",
                                                 "Plant Shape (Mandragora)",
                                                 "You are in mandragora form now. You have a +2 size bonus to your Strength and Constitution and a +2 natural armor bonus. Your movement speed is increased by 10 feet. You also have one 1d6 bite attack, two 1d4 slams and poison ability.",
                                                 entangle.Icon,
                                                 mandragora,
                                                 2, 0, 2, 2, 10, Size.Small,
                                                 mandaragora_slam, mandaragora_slam, new BlueprintItemWeapon[] {mandragora_bite},
                                                 mandragora_poison
                                                 );  

            plant_shapeI = replaceForm(beast_shape_prototype, mandragora_form, "PlantShapeISpell", "Plant Shape I",
                                                "You become a Small mandragora. You gain a +2 size bonus to your Strength and Constitution and a +2 natural armor bonus. Your movement speed is increased by 10 feet. You also gain one 1d6 bite attack, two 1d4 slams and poison ability.");
            plant_shapeI.RemoveComponents<SpellListComponent>();
            plant_shapeI.AddToSpellList(Helpers.alchemistSpellList, 5);
            plant_shapeI.AddToSpellList(Helpers.wizardSpellList, 5);
            plant_shapeI.SetIcon(entangle.Icon);
            Helpers.AddSpellAndScroll(plant_shapeI, "5022612735a9e2345bfc5110106823d8");
        }



        

        static void createPlantShapeII()
        {
            var entangle = library.Get<BlueprintAbility>("0fd00984a2c0e0a429cf1a911b4ec5ca");
            shambling_mound_form = library.CopyAndAdd<BlueprintBuff>("50ab9c820eb9cf94d8efba3632ad5ce2", "PlantShapeIIBuff", ""); //from beast shape 4
            shambling_mound_form.SetName("Plant Shape (Shambling Mound)");
            shambling_mound_form.SetIcon(entangle.Icon);

            plant_shapeII = replaceForm(beast_shape_prototype, shambling_mound_form, "PlantShapeIISpell", "Plant Shape II",
                                                "You become a Large shambling mound. You gain a +4 size bonus to your Strength, a +2 size bonus to your Constitution, +4 natural armor bonus, resist fire 20, and resist electricity 20. Your movement speed is reduced by 10 feet. You also have two 2d6 slam attacks, the constricting vines ability, and the poison ability.\nConstricting Vines: A shambling mound's vines coil around any creature it hits with a slam attack. The shambling mound attempts a grapple maneuver check against its target, and on a successful check its vines deal 2d6+5 damage and the foe is grappled.\nGrappled characters cannot move, and take a -2 penalty on all attack rolls and a -4 penalty to Dexterity. Grappled characters attempt to escape every round by making a successful combat maneuver, Strength, Athletics, or Mobility check. The DC of this check is the shambling mound's CMD.\nEach round, creatures grappled by a shambling mound suffer 4d6+Strength modifier × 2 damage.\nA shambling mound receives a +4 bonus on grapple maneuver checks.\nPoison:\nSlam; Save: Fortitude\nFrequency: 1/round for 2 rounds\nEffect: 1d2 Strength and 1d2 Dexterity damage\nCure: 1 save\nThe save DC is Constitution-based.");

            plant_shapeII.AddToSpellList(Helpers.alchemistSpellList, 6);
            plant_shapeII.AddToSpellList(Helpers.wizardSpellList, 6);
            plant_shapeII.SetIcon(entangle.Icon);
            Helpers.AddSpellAndScroll(plant_shapeII, "5022612735a9e2345bfc5110106823d8");
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("467d2a1d2107da64395b591393baad17"), plant_shapeII, 6);
        }


        static void createPlantShapeIII()
        {
            var entangle = library.Get<BlueprintAbility>("0fd00984a2c0e0a429cf1a911b4ec5ca");
            BlueprintUnit treant = library.Get<BlueprintUnit>("0ef5c65ca08a0204cba840f01cd415af");
            treant_form = createPolymorphForm("PlantshapeIIITreantBuff",
                                                 "Plant Shape (Treant)",
                                                 "You are in treant form now. You have a +8 size bonus to your Strength, +4 to Constitution, -2 penalty to Dexterity and a +6 natural armor bonus. You also have two 2d6 slam attacks, damage reduction 10/slashing, vulnerability to fire and trample ability.",
                                                 entangle.Icon,
                                                 treant,
                                                 8, -2, 6, 6, 0, Size.Huge,
                                                 treant_slam, treant_slam, new BlueprintItemWeapon[0],
                                                 trample, overrun,
                                                 library.Get<BlueprintFeature>("8e934134fec60ab4c8972c85a7b62f89"),
                                                 library.Get<BlueprintFeature>("0df8cdae87d2a3047ad2b1c0568407e9")
                                                 );

            BlueprintUnit giant_flytrap = library.Get<BlueprintUnit>("fb824352b7968fb4d8103ac439644633");
            giant_flytrap_form = createPolymorphForm("PlantshapeIIIGiantFlytrapBuff",
                                                 "Plant Shape (Giant Flytrap)",
                                                 "You are in giant flytrap form now. You have a +8 size bonus to your Strength, +4 to Constitution, -2 penalty to Dexterity and a +6 natural armor bonus. You also have four 1d8 bite attacks, acid Resistance 20, immunity to trip and blindsight. It also has engulf ability.",
                                                 entangle.Icon,
                                                 giant_flytrap,
                                                 8, -2, 6, 6, 0, Size.Huge,
                                                 giant_flytrap_bite, null, new BlueprintItemWeapon[0],
                                                 //giant_flytrap_bite, giant_flytrap_bite, new BlueprintItemWeapon[] { giant_flytrap_bite, giant_flytrap_bite },
                                                 library.Get<BlueprintFeature>("416386972c8de2e42953533c4946599a"), //acid resistance
                                                 library.Get<BlueprintFeature>("236ec7f226d3d784884f066aa4be1570"), //blindsight
                                                 library.Get<BlueprintFeature>("e0ac733ba38f74d46a1712c95b9874f2"), //trip immunity
                                                 engulf
                                                 );
            giant_flytrap_form.AddComponent(Helpers.Create<BuffExtraAttack>(b => b.Number = 3)); //+ bite x 3

            treant_form_spell = replaceForm(smilodon_form_spell, treant_form, "PlantShapeIIITreantAbility", "Plant Shape III (Treant)",
                                     "You become a Huge treant. You gain a +8 size bonus to your Strength, +4 to Constitution, -2 penalty to Dexterity and a +6 natural armor bonus. You also gain two 2d6 slam attacks, damage reduction 10/slashing, vulnerability to fire and trample ability.");
            treant_form_spell.RemoveComponents<SpellListComponent>();
            treant_form_spell.SetIcon(entangle.Icon);
            giant_flytrap_form_spell = replaceForm(smilodon_form_spell, giant_flytrap_form, "PlantShapeIIIGiantFlytrapAbility", "Plant Shape III (Giant Flytrap)",
                                                 "You become a Huge giant flytrap. You gain a +8 size bonus to your Strength, +4 to Constitution, -2 penalty to Dexterity and a +6 natural armor bonus. You also gain four 1d8 bite attacks, acid Resistance 20 and blindsight and poison ability.");

            giant_flytrap_form_spell.RemoveComponents<SpellListComponent>();
            giant_flytrap_form_spell.SetIcon(entangle.Icon);
            plant_shapeIII = library.CopyAndAdd<BlueprintAbility>("940a545a665194b48b722c1f9dd78d53", "PlantShapeIIISpell", "");
            plant_shapeIII.SetIcon(entangle.Icon);
            plant_shapeIII.SetName("Plant Shape III");
            plant_shapeIII.SetDescription("You become a Huge Treant or a Huge Giant Flytrap");
            plant_shapeIII.ReplaceComponent<AbilityVariants>(Helpers.CreateAbilityVariants(plant_shapeIII, treant_form_spell, giant_flytrap_form_spell));

            plant_shapeIII.RemoveComponents<SpellListComponent>();
            plant_shapeIII.AddToSpellList(Helpers.wizardSpellList, 7);
            Helpers.AddSpellAndScroll(plant_shapeIII, "5022612735a9e2345bfc5110106823d8");
        }


        static void fixBeastShape1()
        {
            var beast_shape1 = library.Get<BlueprintAbility>("61a7ed778dd93f344a5dacdbad324cc9");
            beast_shape1.SetDescription("You become a Medium wolf or a Medium leopard.");
            beast_shape1.RemoveComponent(beast_shape1.GetComponent<AbilityTargetHasFact>());
            beast_shape1.RemoveComponent(beast_shape1.GetComponent<AbilitySpawnFx>());
            beast_shape1.RemoveComponent(beast_shape1.GetComponent<AbilityExecuteActionOnCast>());
            beast_shape1.RemoveComponent(beast_shape1.GetComponent<ContextRankConfig>());

            leopard_form.GetComponent<Polymorph>().Facts = leopard_form.GetComponent<Polymorph>().Facts.AddToArray(trip_defense_4legs);
            leopard_form_spell = replaceForm(smilodon_form_spell, leopard_form, "BeastShapeILeopardAbility", "Beast Shape I (Leopard)",
                                                "You become a Medium leopard. You gain a +2 size bonus to your Strength and a +2 natural armor bonus. You also gain two 1d3 claw attacks and one 1d6 bite attack.");


            wolf_form.GetComponent<Polymorph>().Facts = wolf_form.GetComponent<Polymorph>().Facts.AddToArray(trip_defense_4legs);
            wolf_form_spell = replaceForm(smilodon_form_spell, wolf_form, "BeastShapeIWolfAbility", "Beast Shape I (Wolf)",
                                                "You become a Medium wolf. You gain a +2 size bonus to your Strength and a +2 natural armor bonus. Your movement speed is increased by 20 feet. You also gain a 1d6 bite attack and the tripping bite ability.\nTripping Bite: A wolf can attempt to trip its opponent as a free action if it hits with bite attack.");

            beast_shape1.AddComponent(Helpers.CreateAbilityVariants(beast_shape1, wolf_form_spell, leopard_form_spell));
        }


        static void fixBeastShape2()
        {
            var beast_shape2 = library.Get<BlueprintAbility>("5d4028eb28a106d4691ed1b92bbb1915");
            beast_shape2.RemoveComponent(beast_shape2.GetComponent<AbilityTargetHasFact>());
            beast_shape2.RemoveComponent(beast_shape2.GetComponent<AbilitySpawnFx>());
            beast_shape2.RemoveComponent(beast_shape2.GetComponent<AbilityExecuteActionOnCast>());
            beast_shape2.RemoveComponent(beast_shape2.GetComponent<ContextRankConfig>());
            beast_shape2.SetDescription("You become a Large bear or a Large dire wolf.");


            BlueprintUnit dire_wolf = library.Get<BlueprintUnit>("87b83e0e06432a44eb50fb03c71bc8f5");
            dire_wolf_form = createPolymorphForm("BeastShapeIIDireWolfBuff",
                                                 "Wild Shape (Dire Wolf)",
                                                 "You are in dire wolf form now. You have a +4 size bonus to your Strength, -2 penalty to dexterity and a +4 natural armor bonus. Your movement speed is increased by 10 feet. You also have a 1d8 bite attack and the tripping bite ability.",
                                                 beast_shape2.Icon,
                                                 dire_wolf,
                                                 4, -2, 0, 4, 10, Size.Large,
                                                 Bite1d6, null, new BlueprintItemWeapon[0],
                                                 tripping_bite, trip_defense_4legs);

            bear_form.GetComponent<Polymorph>().Facts[0] = trip_defense_4legs;
            bear_form.SetDescription("You are in bear now. You have a +4 size bonus to your Strength, a –2 penalty to your Dexterity and a +4 natural armor bonus. Your movement speed is increased by 10 feet. You also have two 1d6 claw attacks and one 1d6 bite attack.");
                                                   
            dire_wolf_form_spell = replaceForm(smilodon_form_spell, dire_wolf_form, "BeastShapeIIDireWolfAbility", "Beast Shape II (Dire Wolf)",
                                                "You become a Large dire wolf. You gain a +4 size bonus to your Strength, -2 penalty to Dexterity, and a +4 natural armor. Your movement speed is increased by 10 feet. You also gain a 1d8 bite attack and the tripping bite ability.");
            bear_form_spell = replaceForm(smilodon_form_spell, bear_form, "BeastShapeIIBearAbility", "Beast Shape II (Bear)",
                                            "You become a Large bear. You gain a +4 size bonus to your Strength, a –2 penalty to your Dexterity, and a +4 natural armor bonus. Your movement speed is increased by 10 feet. You also gain two 1d6 claw attacks and one 1d6 bite attack.");


            beast_shape2.AddComponent(Helpers.CreateAbilityVariants(beast_shape2, bear_form_spell, dire_wolf_form_spell));
        }


        static void fixBeastShape3()
        {
            var beast_shape3 = library.Get<BlueprintAbility>("9b93040dad242eb43ac7de6bb6547030");
            beast_shape3.SetDescription("You become a Large smilodon or a Huge mastodon.");
            beast_shape3.RemoveComponent(beast_shape3.GetComponent<AbilityTargetHasFact>());
            beast_shape3.RemoveComponent(beast_shape3.GetComponent<AbilitySpawnFx>());
            beast_shape3.RemoveComponent(beast_shape3.GetComponent<AbilityExecuteActionOnCast>());
            beast_shape3.RemoveComponent(beast_shape3.GetComponent<ContextRankConfig>());

            BlueprintUnit mastodon = library.Get<BlueprintUnit>("24dceb9ad134ee344a142c2a68f0f695");
            mastodon_form = createPolymorphForm("BeastShapeIIIMastodonBuff",
                                                 "Wild Shape (Mastodon)",
                                                 "You are in mastodon form now. You have a +6 size bonus to your Strength, -4 penalty to Dexterity, and a +6 natural armor bonus. Your movement speed is increased by 10 feet. You also have a 2d8 gore attack, 2d6 slam attack and trample ability.",
                                                 beast_shape3.Icon,
                                                 mastodon,
                                                 6, -4, 0, 6, 10, Size.Huge,
                                                 mastodon_gore, mastodon_slam, new BlueprintItemWeapon[0],
                                                 trample, overrun, trip_defense_4legs);

            smilodon_form.GetComponent<Polymorph>().Facts = smilodon_form.GetComponent<Polymorph>().Facts.AddToArray(trip_defense_4legs);
            smilodon_form_spell.SetName("Beast Shape III (Smilodon)");

            mastodon_form_spell = replaceForm(smilodon_form_spell, mastodon_form, "BeastShapeIIIMastodonAbility", "Beast Shape III (Mastodon)",
                "You become a Huge mastodon. You gain a +6 size bonus to your Strength, -4 penalty to Dexterity, and a +6 natural armor bonus. Your movement speed is increased by 10 feet. You also gain a 2d8 gore attack, 2d6 slam attack and trample ability.");

            beast_shape3.AddComponent(Helpers.CreateAbilityVariants(beast_shape3, smilodon_form_spell, mastodon_form_spell));
        }


        static void fixBeastShape4()
        {
            var beast_shape4 = library.Get<BlueprintAbility>("940a545a665194b48b722c1f9dd78d53");
            beast_shape4.SetDescription("You become a Large hodag or a Large winter wolf");


            BlueprintUnit hodag = library.Get<BlueprintUnit>("3822f050c76b00240a248e1ba8597636");
            hodag_form = createPolymorphForm("BeastShapeIVHodagBuff",
                                                 "Wild Shape (Hodag)",
                                                 "You are in hodag form now. You have a +6 size bonus to your Strength, +2 bonus to your Constitution, -2 penalty to Dexterity, and a +6 natural armor bonus. You also have one 1d8 bite attack, two 1d6 claw attacks, one 1d8 tail attack and toss ability.",
                                                 beast_shape4.Icon,
                                                 hodag,
                                                 6, -2, 2, 6, 0, Size.Large,
                                                 library.Get<BlueprintItemWeapon>("c76f72a862d168d44838206524366e1c"),
                                                 library.Get<BlueprintItemWeapon>("c76f72a862d168d44838206524366e1c"),
                                                 new BlueprintItemWeapon[]{library.Get<BlueprintItemWeapon>("ec35ef997ed5a984280e1a6d87ae80a8"),
                                                                           library.Get<BlueprintItemWeapon>("ae822725634c6f0418b8c48bd29df255")
                                                                           },
                                                 trip_defense_4legs, toss_feature);

            hodag_form_spell = replaceForm(smilodon_form_spell, hodag_form, "BeastShapeIVHodagAbility", "Beast Shape IV (Hodag)",
                                            "You become a Large hodag. You gain a +6 size bonus to your Strength, +2 bonus to your Constitution, -2 penalty to Dexterity, and a +6 natural armor bonus. You also gain one 1d8 bite attack, two 1d6 claw attacks, one 1d8 tail attack and toss ability.");



            createWinterWolfBreath();
            BlueprintUnit winter_wolf = library.Get<BlueprintUnit>("b0793ff228ff32242b9a472da19d33f1");
            var winter_wolf_trait = Helpers.CreateFeature("BeastShapeIVWinterWolfTraitFeature",
                                                          "",
                                                          "",
                                                          "",
                                                          null,
                                                          FeatureGroup.None,
                                                          Common.createEnergyDR(20, DamageEnergyType.Cold),
                                                          Common.createAddEnergyVulnerability(DamageEnergyType.Fire)
                                                          );
            winter_wolf_trait.HideInUI = true;
            winter_wolf_trait.HideInCharacterSheetAndLevelUp = true;
            winter_wolf_form = createPolymorphForm("BeastShapeIVWinterWolfBuff",
                                                 "Wild Shape (Winterwolf)",
                                                 "You are in winter wolf form now. You have a +6 size bonus to your Strength, +2 bonus to your Constitution, -2 penalty to Dexterity, and a +6 natural armor bonus. Your movement speed is increased by 10 feet. You also have one 1d8 bite attack and breath weapon (6d6 cold damage, Reflex save for half, can be used once in 1d4 rounds). You also gain cold resistance 20 and fire vulnerability.",
                                                 beast_shape4.Icon,
                                                 winter_wolf,
                                                 6, -2, 2, 6, 10, Size.Large,
                                                 library.Get<BlueprintItemWeapon>("570d6349d0b642846b5ba781d2ad3b26"),
                                                 null,
                                                 new BlueprintItemWeapon[0],
                                                 tripping_bite, trip_defense_4legs, winter_wolf_breath, winter_wolf_trait);
           
            winter_wolf_form_spell = replaceForm(smilodon_form_spell, winter_wolf_form, "BeastShapeIVWinterWolfAbility", "Beast Shape IV (Winter Wolf)",
                                  "You become a Large winter wolf. You gain a + 6 size bonus to your Strength, +2 bonus to your Constitution, -2 penalty to Dexterity, and a + 6 natural armor bonus. Your movement speed is increased by 10 feet. You also gain one 1d8 bite attack and breath weapon(6d6 cold damage, Reflex save for half can be used once in 1d4 rounds).  You also gain cold resistance 20 and fire vulnerability.");
            winter_wolf_form.AddComponent(Helpers.Create<ReplaceAbilityParamsWithContext>(r => r.Ability = winter_wolf_breath));
            beast_shape4.ReplaceComponent<AbilityVariants>(Helpers.CreateAbilityVariants(beast_shape4, winter_wolf_form_spell, hodag_form_spell));
        }



        static void createWinterWolfBreath()
        {
            winter_wolf_breath = library.CopyAndAdd<BlueprintAbility>("29ae2c77ee0041a4dad829bf374c91ee", "BeastShapeIVWinterWolfBreathAbility", "");
            winter_wolf_breath.SetName("Winter Wolf Breath");
            winter_wolf_breath.SetDescription("6d6 cold damage. Can be used once in 1d4 rounds");
            winter_wolf_breath.LocalizedSavingThrow = Helpers.reflexHalfDamage;
            winter_wolf_breath.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(min: 6, max: 6));
            winter_wolf_breath.CanTargetPoint = true;
            var winter_wolf_cooldown_buff = Helpers.CreateBuff("BeastShapeIVWinterWolfBreathWeaponCooldownBuff",
                                                          "Winter Wolf Breath Weapon Cooldown",
                                                          "You can not use Breath Weapon now.",
                                                          "",
                                                          winter_wolf_breath.Icon,
                                                          null);
            var cooldown_action = Helpers.Create<AbilityExecuteActionOnCast>();
            var cooldown_action_apply = Helpers.CreateApplyBuff(winter_wolf_cooldown_buff,
                                                              duration: Helpers.CreateContextDuration(bonus: Common.createSimpleContextValue(0), diceType: DiceType.D4, diceCount: Common.createSimpleContextValue(1)),
                                                              fromSpell: false,
                                                              dispellable: false,
                                                              toCaster: true
                                                              );
            cooldown_action.Actions = Helpers.CreateActionList(cooldown_action_apply);
            cooldown_action.Conditions = new Kingmaker.ElementsSystem.ConditionsChecker();
            winter_wolf_breath.AddComponent(cooldown_action);
            winter_wolf_breath.AddComponent(Common.createAbilityCasterHasNoFacts(winter_wolf_cooldown_buff));
        }


        static void fixPolymorph1()
        {
            var polymorph_animal = library.Get<BlueprintAbility>("963be80e4c1b3734ab6b276659d834c4");

            var polymorph_spell = library.Get<BlueprintAbility>("93d9d74dac46b9b458d4d2ea7f4b1911");
            var variants = polymorph_spell.GetComponent<AbilityVariants>();
            var polymorph_bear = replaceForm(polymorph_animal, bear_form, "PolymorphSpellBearAbility", "Polymorph (Bear)",
                                                "Target becomes a Large bear. It gains a + 4 size bonus to Strength, a –2 penalty to Dexterity, and a + 4 natural armor bonus. Its movement speed is increased by 10 feet. It also gains two 1d6 claw attacks and one 1d6 bite attack.");
            var polymorph_dire_wolf = replaceForm(polymorph_animal, dire_wolf_form, "PolymorphSpellDireWolfAbility", "Polymorph (Dire Wolf)",
                                                "Target becomes a Large dire wolf. It gains a +4 size bonus to Strength, -2 penalty to Dexterity, and a +4 natural armor bonus. Its movement speed is increased by 10 feet. It also gains a 1d8 bite attack and the tripping bite ability.");

            var polymorph_wolf = replaceForm(polymorph_animal, wolf_form, "PolymorphSpellWolfAbility", "Polymorph (Wolf)",
                                               "Target becomes a Medium wolf. It gains a +2 size bonus to Strength and a +2 natural armor bonus. Its movement speed is increased by 20 feet. It also gains a 1d6 bite attack and the tripping bite ability.\nTripping Bite: A wolf can attempt to trip its opponent as a free action if it hits with bite attack.");

            variants.Variants = variants.Variants.AddToArray(polymorph_wolf, polymorph_bear, polymorph_dire_wolf);
            polymorph_spell.SetDescription("This spell transforms an allied creature into a wolf, a leopard, a Large bear, a Large dire wolf or a small elemental. The subject may choose to resume its normal form as a full-round action; doing so ends the spell for that subject.");
        }


        static void fixPolymorph2()
        {
            var polymorph_animal = library.Get<BlueprintAbility>("763430b60405dc645a048b4be22d3f63");

            var polymorph_spell = library.Get<BlueprintAbility>("a9fc28e147dbb364ea4a3c1831e7e55f");
            var variants = polymorph_spell.GetComponent<AbilityVariants>();

            var polymorph_mastodon = replaceForm(polymorph_animal, mastodon_form, "PolymorphGreaterSpellMastodonAbility", "Polymorph, Greater (Mastodon)",
                                                "Target becomes a Huge mastodon. It gains a +6 size bonus to Strength, -4 penalty to Dexterity, and a +6 natural armor bonus. Its movement speed is increased by 10 feet. It also gains a 2d8 gore attack, 2d6 slam attack and trample ability.");
            var polymorph_hodag = replaceForm(polymorph_animal, hodag_form, "PolymorphGreaterSpellHodagAbility", "Polymorph, Greater (Hodag)",
                                     "Target becomes a Large hodag. It gains a +6 size bonus to Strength, +2 bonus to Constitution, -2 penalty to Dexterity, and a +6 natural armor bonus. It also gains one 1d8 bite attack, two 1d6 claw attacks, one 1d8 tail attack and poison ability.");
            var polymorph_winter_wolf = replaceForm(polymorph_animal, winter_wolf_form, "PolymorphGreaterSpellWinterWolfAbility", "Polymorph, Greater (Winter Wolf)",
                                     "Target becomes a Large winter wolf. It gains a + 6 size bonus to Strength, +2 bonus to Constitution, -2 penalty to Dexterity, and a + 6 natural armor bonus. Its movement speed is increased by 10 feet. It also gains one 1d8 bite attack and breath weapon(6d6 cold damage, Reflex save for half can be used once in 1d4 rounds).  It also gains cold resistance 20 and fire vulnerability.");
            variants.Variants = variants.Variants.AddToArray(polymorph_mastodon, polymorph_hodag, polymorph_winter_wolf);
            polymorph_spell.SetDescription("This spell transforms an allied creature into a large smilodon, huge mastodon, large hodag, large winter wolf, large shambling mound, large elemental, wyvern or medium dragon-like creature. The subject may choose to resume its normal form as a full-round action; doing so ends the spell for that subject.");

        }



        static BlueprintBuff createPolymorphForm(string name, string display_name, string description, UnityEngine.Sprite icon,
                                                BlueprintUnit unit, int str_bonus, int dex_bonus, int con_bonus, int na_bonus, int speed_bonus, Size size, 
                                                BlueprintItemWeapon main_hand, BlueprintItemWeapon off_hand, BlueprintItemWeapon[] additional_limbs,
                                                params BlueprintUnitFact[] facts)
        {
            var buff = library.CopyAndAdd<BlueprintBuff>("00d8fbe9cf61dc24298be8d95500c84b", name, ""); //wolf wildshape as base
            buff.SetDescription(description);
            buff.SetName(display_name);
            buff.SetIcon(icon);
            var polymorph_component = buff.GetComponent<Polymorph>().CreateCopy();
            polymorph_component.Prefab = unit.Prefab;
            
            polymorph_component.Size = size;
            polymorph_component.StrengthBonus = str_bonus;
            polymorph_component.ConstitutionBonus = con_bonus;
            polymorph_component.DexterityBonus = dex_bonus;
            polymorph_component.NaturalArmor = na_bonus;
            polymorph_component.Facts = facts.AddToArray(turn_back);
            polymorph_component.MainHand = main_hand;
            polymorph_component.OffHand = off_hand;
            polymorph_component.AdditionalLimbs = additional_limbs;

            buff.ReplaceComponent<Polymorph>(polymorph_component);

            var bark = buff.GetComponent<ReplaceAsksList>().CreateCopy();
            bark.Asks = unit.Visual.Barks;
            buff.ReplaceComponent<ReplaceAsksList>(bark);

            var speed = buff.GetComponent<BuffMovementSpeed>().CreateCopy();
            speed.Value = speed_bonus;
            buff.ReplaceComponent<BuffMovementSpeed>(speed);

            return buff;
        }


        static BlueprintAbility replaceForm(BlueprintAbility ability_base, BlueprintBuff form_buff, string new_name, string display_name, string description)
        {
            var ability = library.CopyAndAdd<BlueprintAbility>(ability_base.AssetGuid, new_name, "");
            ability.SetName(display_name);
            ability.SetDescription(description);
            var check_component = ability.GetComponent<AbilityTargetHasFact>().CreateCopy();
            check_component.CheckedFacts[0] = form_buff;
            ability.ReplaceComponent<AbilityTargetHasFact>(check_component);
            var action = ability.GetComponent<AbilityEffectRunAction>().CreateCopy();
            var original_action = ((ContextActionApplyBuff)ability.GetComponent<AbilityEffectRunAction>().Actions.Actions[0]).CreateCopy();
            original_action.Buff = form_buff;
            action.Actions = Helpers.CreateActionList(original_action);

            ability.ReplaceComponent<AbilityEffectRunAction>(action);

            return ability;
        }


        static public void allowElementalsToCast()
        {
            Main.logger.Log("Enabling casting in elemental shape.");
            var  spellIds = new string[] {
                "690c90a82bf2e58449c6b541cb8ea004", // elemental body i, ii, iii, iv
                "6d437be73b459594ab103acdcae5b9e2",
                "459e6d5aab080a14499e13b407eb3b85",
                "376db0590f3ca4945a8b6dc16ed14975"
            };
            foreach (var spellId in spellIds)
            {
                var baseSpell = library.Get<BlueprintAbility>(spellId);
                foreach (var spell in baseSpell.Variants)
                {
                    var buff = spell.GetComponent<AbilityEffectRunAction>().Actions.Actions
                            .OfType<ContextActionApplyBuff>().First().Buff;
                    buff.AddComponent(AddMechanicsFeature.MechanicsFeatureType.NaturalSpell.CreateAddMechanics());
                }
            }

            var forms_ids = new string[]
            {
                //transmuter air
                "3689b69a30d6d7c48b90e28228fb7b7c",
                "2b2060036a20108448299f3ee2b14015",

                //transmuter fire
                "51107ed2162aa8542834362c3a10c74c",
                "8c026422d0be0684fa2ba0986fa901db",

                //transmuter earth
                "073918bcdc83a82418af6816d719ca7c",
                "66906f2ff64be8e4eb8f87b04501b7c4",

                //transmuter water
                "a543c3c5e909af8479044c34d0f3f33b",
                "872961d85b9cd9444b57560aeeb6e383",
                //druid
                //air
                "eb52d24d6f60fc742b32fe943b919180",
                "814bc75e74f969641bf110addf076ff9",
                "65fdf187fea97c94b9cf4ff6746901a6",
                "dc1ef6f6d52b9fd49bc0696ab1a4f18b",
                //earth
                "f0826c3794c158c4cbbe9ceb4210d6d6",
                "bf145574939845d43b68e3f4335986b4",
                "e76500bc1f1f269499bf027a5aeb1471",
                "add5378a75feeaf4384766da10ddc40d",
                //fire
                "e85abd773dbce30498efa8da745d7ca7",
                "7f30b0f7f3c4b6748a2819611fb236f8",
                "3e3f33fb3e581ab4e8923a5eabd15923",
                "9e6b7b058bc74fc45903679adcab8553",
                //water
                "ea2cd08bdf2ca1c4f8a8870804790cd7",
                "5993b78c793667e45bf0380e9275fab7",
                "c5925e7b9e7fc2e478526b4cfc8c6427",
                "9c58cfcad11f7fd4cb85e22187fddac7"
            };

            foreach (var form_id in forms_ids)
            {
                var buff = library.Get<BlueprintBuff>(form_id);
                buff.AddComponent(AddMechanicsFeature.MechanicsFeatureType.NaturalSpell.CreateAddMechanics());
            }
        }


        [Harmony12.HarmonyPatch(typeof(Kingmaker.Items.UnitBody))]
        [Harmony12.HarmonyPatch("ApplyPolymorphEffect", Harmony12.MethodType.Normal)]
        class Patch_FixPolymorphLockingTemporaryItems_UnitBody_ApplyPolymorphEffect
        {
            static public void Postfix(Kingmaker.Items.UnitBody __instance)
            {
                foreach (ItemSlot itemSlot in __instance.AllSlots)
                {
                    itemSlot.Lock.Release();
                }
            }
        }

    }
}
