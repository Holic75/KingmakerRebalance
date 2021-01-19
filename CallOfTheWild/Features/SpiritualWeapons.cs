using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    public class SpiritualWeapons
    {
        static public LibraryScriptableObject library => Main.library;
        static public BlueprintUnit spiritual_weapon_unit;
        static public BlueprintUnit spiritual_ally_unit;
        static public BlueprintUnit mages_sword_unit;
        static public BlueprintUnit twilight_knife_unit;
        static public BlueprintBuff weapon_metamagic_buff;
        static public Dictionary<WeaponCategory, BlueprintItemWeapon> category_weapon_map = new Dictionary<WeaponCategory, BlueprintItemWeapon>();
        static public BlueprintAbility spiritual_weapon;
        static public BlueprintAbility spiritual_ally;
        static public BlueprintAbility twilight_knife;
        static public BlueprintAbility mages_sword;
        static BlueprintUnitProperty best_mental_stat_property = NewMechanics.HighestStatPropertyGetter.createProperty("SpiritualAllyBestMentalStatProperty", "", StatType.Wisdom, StatType.Charisma, StatType.Intelligence);
        static BlueprintWeaponEnchantment fx_enchant;
        static public BlueprintFeature spiritual_guardian;

        public static void load()
        {
            init();
            fillSpiritualWeaponsMap();
            createSpiritualGuardian();

            createSpiritualWeapon();
            createSpiritualAlly();
            createMagesSword();
            createTwilightKnife();
        }


        static void createSpiritualGuardian()
        {
            spiritual_guardian = Helpers.CreateFeature("SpiritualGuardianFeature",
                                                       "Spiritual Guardian",
                                                       "Whenever you cast spiritual weapon, spiritual ally, or a similar spell that grants you a spiritual guardian, that guardian uses your caster level instead of your base attack bonus to determine its base attack bonus, potentially granting it multiple attacks.\n"
                                                       + "Additionally, it gains a +2 bonus on caster level checks to overcome spell resistance as well as on damage rolls.",
                                                       "",
                                                       LoadIcons.Image2Sprite.Create(@"AbilityIcons/SpiritualAlly.png"),
                                                       FeatureGroup.None);
            library.AddFeats(spiritual_guardian);
        }


        static void fillSpiritualWeaponsMap()
        {
            var empower_buff = Common.createBuffContextEnchantPrimaryHandWeaponIfHasMetamagic(Kingmaker.UnitLogic.Abilities.Metamagic.Empower,
                                                                                      false, false,
                                                                                      new BlueprintWeaponType[0], WeaponEnchantments.empower_enchant);

            var maximize_buff = Common.createBuffContextEnchantPrimaryHandWeaponIfHasMetamagic(Kingmaker.UnitLogic.Abilities.Metamagic.Maximize,
                                                                                                  false, false,
                                                                                                   new BlueprintWeaponType[0], WeaponEnchantments.maximize_enchant);

            weapon_metamagic_buff = Helpers.CreateBuff("SpiritualWeaponsEnchantBuff",
                                                       "",
                                                       "",
                                                       "",
                                                       null,
                                                       null,
                                                       empower_buff,
                                                       maximize_buff
                                                       );
            weapon_metamagic_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            foreach (var wc in Enum.GetValues(typeof(WeaponCategory)).OfType<WeaponCategory>())
            {
                var weapon = Kingmaker.Game.Instance.BlueprintRoot.Progression.CategoryDefaults.Entries.FirstOrDefault(p => p.Key == wc)?.DefaultWeapon;
                if (wc == WeaponCategory.UnarmedStrike)
                {
                    weapon = library.Get<BlueprintItemWeapon>("e5ea5042194712040a3b2944e6944d10"); //unarmed 1d3
                }

                category_weapon_map.Add(wc, weapon);

                if (weapon != null)
                {
                    var spiritual_weapon = library.CopyAndAdd(weapon, "Spiritual" + weapon.name, "$" + Helpers.MergeIds(weapon.AssetGuid, "2b9c3db072994c32b153caa00f2d5d47"));
                    spiritual_weapon.AddComponent(Helpers.Create<NewMechanics.EnchantmentMechanics.WeaponSourceBuff>(w => w.buff = weapon_metamagic_buff));
                }
            }
        }


        static void init()
        {
            var unit = library.CopyAndAdd<BlueprintUnit>("655ac57b330918c4aadc78a00fb2ccaf", "SpiritualWeaponUnit", ""); //cr7 ghost, but it does not matter
            unit.RemoveComponents<Experience>();
            unit.RemoveComponents<AddTags>();
            unit.Body = unit.Body.CloneObject();
            unit.Body.PrimaryHand = null;
            unit.Strength = 10;
            unit.Dexterity = 10;
           
            var target_marked_consideration = Helpers.Create<SpiritualAllyMechanics.TargetMarkedConsideration>();
            target_marked_consideration.name = "SpiritualAllyTargetMarkedConsideration";
            library.AddAsset(target_marked_consideration, "");

            var attack_action = library.CopyAndAdd<Kingmaker.Controllers.Brain.Blueprints.BlueprintAiAction>("866ffa6c34000cd4a86fb1671f86c7d8", "SpiritualAllyAttAiAction", "");
            attack_action.TargetConsiderations = new Kingmaker.Controllers.Brain.Blueprints.Considerations.Consideration[] { target_marked_consideration };

            var brain = library.CopyAndAdd(unit.Brain, "SpiritualAllyBrain", "");
            brain.Actions = new Kingmaker.Controllers.Brain.Blueprints.BlueprintAiAction[] { attack_action };
            unit.Brain = brain;
            unit.Faction = library.Get<BlueprintFaction>("1b08d9ed04518ec46a9b3e4e23cb5105"); //summoned

            var air_enchatment = library.Get<BlueprintWeaponEnchantment>("1d64abd0002b98043b199c0e3109d3ee"); //from kineticist
            fx_enchant = Common.createWeaponEnchantment("SpiritualWeaponEnchantment", "Force", "This weapon deals force damage.", "", "", "", 0, air_enchatment.WeaponFxPrefab);
            var spiritual_weapon_feature = Helpers.CreateFeature("SpiritualWeaponFeature",
                                                               "",
                                                               "",
                                                               "",
                                                               null,
                                                               FeatureGroup.None,
                                                               Common.createAddOutgoingGhost(),
                                                               Helpers.Create<Untargetable>(),
                                                               Helpers.Create<IgnoreDamageReductionOnAttack>(),
                                                               Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.IterativeNaturalAttacks),
                                                               Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.DisableAttacksOfOpportunity),
                                                               //Helpers.Create<AddImmortality>(),
                                                               Helpers.Create<AooMechanics.DoNotProvokeAoo>(),
                                                               Helpers.Create<AooMechanics.DoesNotEngage>(),
                                                               Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.CantMove),
                                                               Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.Unlootable),
                                                               Helpers.Create<PreciseShot>()
                                                               );
            unit.AddFacts = new Kingmaker.Blueprints.Facts.BlueprintUnitFact[] {spiritual_weapon_feature,
                                                                               library.Get<BlueprintFeature>("70cffb448c132fa409e49156d013b175"), //airborne
                                                                               library.Get<BlueprintFeature>("7812ad3672a4b9a4fb894ea402095167"), //improved unarmed strike (for irori)
                                                                               library.Get<BlueprintBuff>("20f79fea035330b479fc899fa201d232")}; //ghost fx - to be changed
            unit.AddComponents(Helpers.Create<UnitViewMechanics.InvisibleUnit>(),
                               Helpers.Create<UnitViewMechanics.WeaponsAlwaysDrawn>());
            spiritual_weapon_unit = unit;
            spiritual_weapon_unit.SetUnitName("Spiritual Weapon");

            var spiritual_ally_feature = Helpers.CreateFeature("SpiritualAllyFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Common.createAddOutgoingGhost(),
                                                   Helpers.Create<Untargetable>(),
                                                   Helpers.Create<IgnoreDamageReductionOnAttack>(),
                                                   Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.IterativeNaturalAttacks),
                                                   //Helpers.Create<AddImmortality>(),
                                                   Helpers.Create<AooMechanics.DoNotProvokeAoo>(),
                                                   Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.CantMove),
                                                   Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.Unlootable),
                                                   Helpers.Create<PreciseShot>()
                                                   );
            spiritual_ally_unit = library.CopyAndAdd<BlueprintUnit>(spiritual_weapon_unit, "SpiritualAllyUnit", "");
            spiritual_ally_unit.AddFacts = new Kingmaker.Blueprints.Facts.BlueprintUnitFact[] {spiritual_ally_feature,
                                                                               library.Get<BlueprintFeature>("70cffb448c132fa409e49156d013b175"), //airborne
                                                                               library.Get<BlueprintFeature>("7812ad3672a4b9a4fb894ea402095167"), //improved unarmed strike (for irori)
                                                                               library.Get<BlueprintBuff>("20f79fea035330b479fc899fa201d232")}; //ghost fx - to be changed
            spiritual_ally_unit.Prefab = Common.createUnitViewLink("209f5908484cfe348bf1e0eeb49955e8"); //ghost queen
            spiritual_ally_unit.RemoveComponents<UnitViewMechanics.InvisibleUnit>();
            spiritual_ally_unit.RemoveComponents<UnitViewMechanics.WeaponsAlwaysDrawn>();
            spiritual_ally_unit.SetUnitName("Spiritual Ally");

            var mages_sword_feature = Helpers.CreateFeature("MagesSwordFeature",
                                                           "",
                                                           "",
                                                           "",
                                                           null,
                                                           FeatureGroup.None,
                                                           Common.createAddOutgoingGhost(),
                                                           Helpers.Create<Untargetable>(),
                                                           Helpers.Create<IgnoreDamageReductionOnAttack>(),
                                                           Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.IterativeNaturalAttacks),
                                                           //Helpers.Create<AddImmortality>(),
                                                           Helpers.Create<AooMechanics.DoNotProvokeAoo>(),
                                                           Helpers.Create<AooMechanics.DoesNotEngage>(),
                                                           Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.DisableAttacksOfOpportunity),
                                                           Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.Unlootable)
                                                           //can move (?)
                                                           );
            mages_sword_unit = library.CopyAndAdd<BlueprintUnit>(spiritual_weapon_unit, "MagesSwordUnit", "");
            mages_sword_unit.AddFacts = new Kingmaker.Blueprints.Facts.BlueprintUnitFact[] {mages_sword_feature,
                                                                               library.Get<BlueprintFeature>("70cffb448c132fa409e49156d013b175"), //airborne
                                                                               library.Get<BlueprintFeature>("7812ad3672a4b9a4fb894ea402095167"), //improved unarmed strike (for irori)
                                                                               library.Get<BlueprintBuff>("20f79fea035330b479fc899fa201d232")}; //ghost fx - to be changed

            mages_sword_unit.SetUnitName("Mage's Sword");
            var twilight_knife_feature = Helpers.CreateFeature("TwilightKnifeFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Common.createAddOutgoingGhost(),
                                                   Helpers.Create<Untargetable>(),
                                                   Helpers.Create<IgnoreDamageReductionOnAttack>(),
                                                   Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.IterativeNaturalAttacks),
                                                   //Helpers.Create<AddImmortality>(),
                                                   Helpers.Create<AooMechanics.DoNotProvokeAoo>(),
                                                   Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.DisableAttacksOfOpportunity),
                                                   Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.Unlootable)
                                                   );
            twilight_knife_unit = library.CopyAndAdd<BlueprintUnit>(spiritual_weapon_unit, "TwilightKnifeUnit", "");
            twilight_knife_unit.AddFacts = new Kingmaker.Blueprints.Facts.BlueprintUnitFact[] {twilight_knife_feature,
                                                                               library.Get<BlueprintFeature>("70cffb448c132fa409e49156d013b175"), //airborne
                                                                               library.Get<BlueprintFeature>("7812ad3672a4b9a4fb894ea402095167"), //improved unarmed strike (for irori)
                                                                               library.Get<BlueprintBuff>("20f79fea035330b479fc899fa201d232")}; //ghost fx - to be changed
            twilight_knife_unit.SetUnitName("Twilight Knife");
        }

        static void createSpiritualWeapon()
        {
            var staggered = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3"); //to make only one attack on first round
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/SpiritualWeapon.png");
            var description = "A weapon made of force appears and attacks foes at a distance, as you direct it, dealing 1d8 force damage per hit, +1 point per three caster levels(maximum + 5 at 15th level). The weapon takes the shape of a weapon favored by your deity or a weapon with some spiritual significance or symbolism to you (see below) and has the same threat range and critical multipliers as a real weapon of its form.It strikes the opponent you designate, starting with one attack in the round the spell is cast and continuing each round thereafter on your turn.It uses your base attack bonus(possibly allowing it multiple attacks per round in subsequent rounds) plus your casting stat modifier as its attack bonus. It strikes as a spell, not as a weapon, so for example, it can damage creatures that have damage reduction.As a force effect, it can strike incorporeal creatures without the reduction in damage associated with incorporeality.The weapon always strikes from your direction.It does not get a flanking bonus or help a combatant get one. Your feats or combat actions do not affect the weapon.\n"
                                          + "Each round, you can use a move action to redirect the weapon to a new target. If you do not, the weapon continues to attack the previous round’s target. If the target you directed it at is dead, the weapon will temporary vanish until you point a new target. On any round that the weapon switches targets, it gets one attack. Subsequent rounds of attacking that target allow the weapon to make multiple attacks if your base attack bonus would allow it to.";


            var mark_buff = Helpers.CreateBuff("SpiritualWeaponMarkBuff",
                                               "Spiritual Weapon Target",
                                               description,
                                               "",
                                               icon,
                                               null,
                                               Helpers.Create<UniqueBuff>()
                                               );
            mark_buff.Stacking = StackingType.Stack;
            var apply_mark = Common.createContextActionApplyBuff(mark_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);

            var forbid_target_change_buff = Helpers.CreateBuff("SpiritualWeaponForbidTargetChangeBuff",
                                                               "",
                                                               "",
                                                               "",
                                                               icon,
                                                               null
                                                               );
            forbid_target_change_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            forbid_target_change_buff.Stacking = StackingType.Replace;
            var forbid_target_change = Common.createContextActionApplyBuffToCaster(forbid_target_change_buff, Helpers.CreateContextDurationNonExtandable(), duration_seconds: 3, dispellable: false);

            var spiritual_weapon_buff = Helpers.CreateBuff("SpiritualWeaponUnitBuff",
                                                           "Spiritual Weapon",
                                                           "",
                                                           "",
                                                           null,
                                                           null,
                                                           Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(forbid_target_change), only_hit: false),
                                                           Helpers.Create<NewMechanics.WeaponDamageChange>(w =>
                                                           {
                                                               w.dice_formula = new DiceFormula(1, DiceType.D8);
                                                               w.bonus_damage = Helpers.CreateContextValue(AbilityRankType.DamageBonus);
                                                               w.damage_type_description = Common.createForceDamageDescription();
                                                           }),
                                                           Helpers.Create<NewMechanics.EnchantmentMechanics.PersistentWeaponEnchantment>(p => p.enchant = fx_enchant),
                                                           Helpers.Create<SpiritualAllyMechanics.SpiritualWeaponBab>(s =>
                                                           {
                                                               s.alternative_feature = spiritual_guardian;
                                                               s.alternative_value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                           }
                                                           ),
                                                           Helpers.CreateContextRankConfig(),
                                                           Helpers.CreateAddContextStatBonus(StatType.AdditionalAttackBonus, ModifierDescriptor.UntypedStackable, rankType: AbilityRankType.StatBonus),
                                                           Helpers.CreateAddContextStatBonus(StatType.AdditionalDamage, ModifierDescriptor.UntypedStackable, rankType: AbilityRankType.DamageDice),
                                                           Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, type: AbilityRankType.StatBonus, customProperty: best_mental_stat_property),
                                                           Helpers.CreateContextRankConfig(type: AbilityRankType.DamageBonus, progression: ContextRankProgression.DivStep,
                                                                                           stepLevel: 3, max: 5),
                                                           Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureList, type: AbilityRankType.DamageDice,
                                                                                           featureList: new BlueprintFeature[] { spiritual_guardian, spiritual_guardian })
                                                           );

            var summon_pool = library.CopyAndAdd<BlueprintSummonPool>("490248a826bbf904e852f5e3afa6d138", "SpiritualWeaponSummonPool", "");
            var apply_summon_buff = Common.createContextActionApplyBuff(library.Get<BlueprintBuff>("6fcdf014694b2b542a867763b4369cb3"), Helpers.CreateContextDuration(), dispellable: false, is_permanent: true);
            var summon_weapon = Helpers.Create<SpiritualAllyMechanics.ContextActionSummonSpiritualAlly>(c =>
            {
                c.Blueprint = spiritual_weapon_unit;
                c.use_deity_weapon = true;
                c.SummonPool = summon_pool;
                c.CountValue = Helpers.CreateContextDiceValue(DiceType.Zero, 1, 1);
                c.DoNotLinkToCaster = false;
                c.DurationValue = Helpers.CreateContextDuration(1000);
                c.AfterSpawn = Helpers.CreateActionList(apply_summon_buff,
                                                        Common.createContextActionApplyBuff(weapon_metamagic_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true),
                                                        Common.createContextActionApplyBuff(spiritual_weapon_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true),
                                                        Common.createContextActionApplyBuff(staggered, Helpers.CreateContextDurationNonExtandable(1), dispellable: false, duration_seconds: 3));
                c.custom_name = "Spiritual Weapon";
                c.attack_mark_buff = mark_buff;
            });

            var clear_summon_pool = Helpers.Create<NewMechanics.ContextActionClearSummonPoolFromCaster>(c => c.SummonPool = summon_pool);
            var mark_ability = Helpers.CreateAbility("SpiritualWeaponTargetSelectionAbility",
                                                     "Spiritual Weapon Target Change",
                                                     description,
                                                     "",
                                                     icon,
                                                     AbilityType.SpellLike,
                                                     UnitCommand.CommandType.Move,
                                                     AbilityRange.Medium,
                                                     "",
                                                     "",
                                                     Helpers.CreateRunActions(clear_summon_pool, apply_mark, summon_weapon),
                                                     Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                     Helpers.CreateSpellDescriptor(SpellDescriptor.Force),
                                                     Common.createAbilityCasterHasNoFacts(forbid_target_change_buff)
                                                     );
            mark_ability.setMiscAbilityParametersSingleTargetRangedHarmful();
            mark_ability.AvailableMetamagic = Metamagic.Maximize | Metamagic.Empower | (Metamagic)MetamagicFeats.MetamagicExtender.Toppling | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing;
            mark_buff.AddComponent(Helpers.CreateAddFactContextActions(deactivated: clear_summon_pool));

            var spiritual_weapon_summoner_buff = Helpers.CreateBuff("SpiritualWeaponSummonerBuff",
                                                                    "Spiritual Weapon Summoner",
                                                                    description,
                                                                    "",
                                                                    icon,
                                                                    null,
                                                                    Helpers.Create<ReplaceAbilityParamsWithContext>(a => a.Ability = mark_ability),
                                                                    Helpers.CreateAddFact(mark_ability),
                                                                    Helpers.CreateAddFactContextActions(deactivated: clear_summon_pool)
                                                                    );
            spiritual_weapon_summoner_buff.Stacking = StackingType.Replace;
            var apply_summoner_buff = Common.createContextActionApplyBuffToCaster(spiritual_weapon_summoner_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);

            spiritual_weapon = Helpers.CreateAbility("SpiritualWeaponAbility",
                                                     "Spiritual Weapon",
                                                     description,
                                                     "",
                                                     icon,
                                                     AbilityType.Spell,
                                                     UnitCommand.CommandType.Standard,
                                                     AbilityRange.Medium,
                                                     Helpers.roundsPerLevelDuration,
                                                     "",
                                                     Helpers.CreateRunActions(clear_summon_pool, apply_summoner_buff, apply_mark, summon_weapon),
                                                     Helpers.CreateContextRankConfig(),
                                                     Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                     Helpers.CreateSpellDescriptor(SpellDescriptor.Force)
                                                     );

            spiritual_weapon.setMiscAbilityParametersSingleTargetRangedHarmful();
            spiritual_weapon.AvailableMetamagic = mark_ability.AvailableMetamagic | Metamagic.Reach | Metamagic.Heighten | Metamagic.Quicken;


            spiritual_weapon.AddToSpellList(Helpers.clericSpellList, 2);
            spiritual_weapon.AddToSpellList(Helpers.inquisitorSpellList, 2);
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("8d454cbb7f25070419a1c8eaf89b5be5"), spiritual_weapon, 2);
            spiritual_weapon.AddSpellAndScroll("fbdd06f0414c3ef458eb4b2a8072e502");
        }


        static void createSpiritualAlly()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/SpiritualAlly.png");
            var description = "An ally made of pure force appears in a single 5-foot square within range. The ally takes the form of a servant of your god.\n"
                                          + "The spiritual ally occupies its space, though you and your allies can move through it, since it is your ally. The spiritual ally carries a single weapon, one favored by your deity (as for spiritual weapon), which has the same threat range and critical modifiers as a real weapon of its form. Each round on your turn, starting with the turn that you cast this spell, your spiritual ally can make an attack against a foe within its reach that you designate. The spiritual ally threatens adjacent squares and can flank and make attacks of opportunity as if it were a normal creature. The spiritual ally uses your base attack bonus (gaining extra attacks if your base attack bonus is high enough) plus your spellcasting stat bonus when it makes a melee attack. When the spiritual ally hits, it deals 1d10 points of force damage + 1 point of damage per 3 caster levels (maximum +5 at 15th level). It strikes as a spell, not a weapon, so it bypasses DR and can affect incorporeal creatures.\n"
                                          + "Each round, you can move the spiritual ally to a new target as a swift action.";

            var mark_buff = Helpers.CreateBuff("SpirituaAllyMarkBuff",
                                               "Spiritual Ally Target",
                                               description,
                                               "",
                                               icon,
                                               null,
                                               Helpers.Create<UniqueBuff>()
                                               );
            mark_buff.Stacking = StackingType.Stack;
            var apply_mark = Common.createContextActionApplyBuff(mark_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);

            var forbid_target_change_buff = Helpers.CreateBuff("SpiritualAllyForbidTargetChangeBuff",
                                                               "",
                                                               "",
                                                               "",
                                                               icon,
                                                               null
                                                               );
            forbid_target_change_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            forbid_target_change_buff.Stacking = StackingType.Replace;
            var forbid_target_change = Common.createContextActionApplyBuffToCaster(forbid_target_change_buff, Helpers.CreateContextDurationNonExtandable(), duration_seconds: 3, dispellable: false);

            var spiritual_ally_buff = Helpers.CreateBuff("SpiritualAllyUnitBuff",
                                                           "Spiritual Ally",
                                                           "",
                                                           "",
                                                           null,
                                                           null,
                                                           Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(forbid_target_change), only_hit: false),
                                                           Helpers.Create<NewMechanics.WeaponDamageChange>(w =>
                                                           {
                                                               w.dice_formula = new DiceFormula(1, DiceType.D10);
                                                               w.bonus_damage = Helpers.CreateContextValue(AbilityRankType.DamageBonus);
                                                               w.damage_type_description = Common.createForceDamageDescription();
                                                           }),
                                                           Helpers.Create<NewMechanics.EnchantmentMechanics.PersistentWeaponEnchantment>(p => p.enchant = fx_enchant),
                                                           Helpers.Create<SpiritualAllyMechanics.SpiritualWeaponBab>(s =>
                                                           {
                                                               s.alternative_feature = spiritual_guardian;
                                                               s.alternative_value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                           }
                                                           ),
                                                           Helpers.CreateContextRankConfig(),
                                                           Helpers.CreateAddContextStatBonus(StatType.AdditionalAttackBonus, ModifierDescriptor.UntypedStackable, rankType: AbilityRankType.StatBonus),
                                                           Helpers.CreateAddContextStatBonus(StatType.AdditionalDamage, ModifierDescriptor.UntypedStackable, rankType: AbilityRankType.DamageDice),
                                                           Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, type: AbilityRankType.StatBonus, customProperty: best_mental_stat_property),
                                                           Helpers.CreateContextRankConfig(type: AbilityRankType.DamageBonus, progression: ContextRankProgression.DivStep,
                                                                                           stepLevel: 3, max: 5),
                                                           Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureList, type: AbilityRankType.DamageDice,
                                                                                           featureList: new BlueprintFeature[] { spiritual_guardian, spiritual_guardian })
                                                           );

            var summon_pool = library.CopyAndAdd<BlueprintSummonPool>("490248a826bbf904e852f5e3afa6d138", "SpiritualAllySummonPool", "");
            var apply_summon_buff = Common.createContextActionApplyBuff(library.Get<BlueprintBuff>("50d51854cf6a3434d96a87d050e1d09a"), Helpers.CreateContextDuration(), dispellable: false, is_permanent: true);
            var summon_ally = Helpers.Create<SpiritualAllyMechanics.ContextActionSummonSpiritualAlly>(c =>
            {
                c.Blueprint = spiritual_ally_unit;
                c.use_deity_weapon = true;
                c.SummonPool = summon_pool;
                c.CountValue = Helpers.CreateContextDiceValue(DiceType.Zero, 1, 1);
                c.DoNotLinkToCaster = false;
                c.DurationValue = Helpers.CreateContextDuration(1000);
                c.AfterSpawn = Helpers.CreateActionList(apply_summon_buff,
                                                        Common.createContextActionApplyBuff(weapon_metamagic_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true),
                                                        Common.createContextActionApplyBuff(spiritual_ally_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true));
                c.attack_mark_buff = mark_buff;
                c.custom_name = "Spiritual Ally";
            });

            var clear_summon_pool = Helpers.Create<NewMechanics.ContextActionClearSummonPoolFromCaster>(c => c.SummonPool = summon_pool);
            var mark_ability = Helpers.CreateAbility("SpiritualAllyTargetSelectionAbility",
                                                     "Spiritual Ally Target Change",
                                                     description,
                                                     "",
                                                     icon,
                                                     AbilityType.SpellLike,
                                                     UnitCommand.CommandType.Swift,
                                                     AbilityRange.Medium,
                                                     "",
                                                     "",
                                                     Helpers.CreateRunActions(clear_summon_pool, summon_ally, apply_mark),
                                                     Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                     Helpers.CreateSpellDescriptor(SpellDescriptor.Force),
                                                     Common.createAbilityCasterHasNoFacts(forbid_target_change_buff)
                                                     );
            mark_ability.setMiscAbilityParametersSingleTargetRangedHarmful();
            mark_ability.AvailableMetamagic = Metamagic.Maximize | Metamagic.Empower | (Metamagic)MetamagicFeats.MetamagicExtender.Toppling | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing;


            var spiritual_ally_summoner_buff = Helpers.CreateBuff("SpiritualAllySummonerBuff",
                                                                    "Spiritual Ally Summoner",
                                                                    description,
                                                                    "",
                                                                    icon,
                                                                    null,
                                                                    Helpers.Create<ReplaceAbilityParamsWithContext>(a => a.Ability = mark_ability),
                                                                    Helpers.CreateAddFact(mark_ability),
                                                                    Helpers.CreateAddFactContextActions(deactivated: Helpers.Create<NewMechanics.ContextActionClearSummonPoolFromCaster>(c => c.SummonPool = summon_pool))
                                                                    );
            spiritual_ally_summoner_buff.Stacking = StackingType.Replace;
            var apply_summoner_buff = Common.createContextActionApplyBuffToCaster(spiritual_ally_summoner_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);

            spiritual_ally = Helpers.CreateAbility("SpiritualAllyAbility",
                                                    "Spiritual Ally",
                                                    description,
                                                    "",
                                                    icon,
                                                    AbilityType.Spell,
                                                    UnitCommand.CommandType.Standard,
                                                    AbilityRange.Medium,
                                                    Helpers.roundsPerLevelDuration,
                                                    "",
                                                    Helpers.CreateRunActions(clear_summon_pool, apply_summoner_buff, summon_ally, apply_mark),
                                                    Helpers.CreateContextRankConfig(),
                                                    Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                    Helpers.CreateSpellDescriptor(SpellDescriptor.Force)
                                                    );

            spiritual_ally.setMiscAbilityParametersSingleTargetRangedHarmful();
            spiritual_ally.AvailableMetamagic = mark_ability.AvailableMetamagic | Metamagic.Reach | Metamagic.Heighten | Metamagic.Quicken;


            spiritual_ally.AddToSpellList(Helpers.clericSpellList, 4);
            spiritual_ally.AddToSpellList(Helpers.inquisitorSpellList, 4);
            spiritual_ally.AddSpellAndScroll("12f4ee72c02537244b5b2bacfa236bc7");
        }



        static void createMagesSword()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/MagesSword.png");
            var description = "This spell brings into being a shimmering, sword-like plane of force. The sword strikes at any opponent within its range, as you desire, starting in the round that you cast the spell. The sword attacks its designated target once each round on your turn. Its attack bonus is equal to your caster level + your Intelligence bonus or your Charisma bonus (for wizards or sorcerers, respectively) with an additional +3 enhancement bonus. As a force effect, it can strike ethereal and incorporeal creatures. It deals 4d6+3 points of force damage, with a threat range of 19–20 and a critical multiplier of ×2.\n"
                                          + "The sword always strikes from your direction. It does not get a bonus for flanking or help a combatant get one. If the sword goes beyond the spell range from you, goes out of your sight, or you are not directing it, it returns to you and hovers.\n"
                                          + "Each round after the first, you can use a move action to switch the sword to a new target. If you do not, the sword continues to attack the previous round’s target.";


            var mark_buff = Helpers.CreateBuff("MagesSwordMarkBuff",
                                               "Mage's Sword Target",
                                               description,
                                               "",
                                               icon,
                                               null,
                                               Helpers.Create<UniqueBuff>()
                                               );
            mark_buff.Stacking = StackingType.Stack;
            var apply_mark = Common.createContextActionApplyBuff(mark_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);

            var forbid_target_change_buff = Helpers.CreateBuff("MagesSwordForbidTargetChangeBuff",
                                                   "",
                                                   "",
                                                   "",
                                                   icon,
                                                   null
                                                   );
            forbid_target_change_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            forbid_target_change_buff.Stacking = StackingType.Replace;
            var forbid_target_change = Common.createContextActionApplyBuffToCaster(forbid_target_change_buff, Helpers.CreateContextDurationNonExtandable(), duration_seconds: 3, dispellable: false);

            var mages_sword_buff = Helpers.CreateBuff("MagesSwordUnitBuff",
                                                           "Mage's Sword",
                                                           "",
                                                           "",
                                                           null,
                                                           null,
                                                           Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(forbid_target_change), only_hit: false),
                                                           Helpers.Create<NewMechanics.WeaponDamageChange>(w =>
                                                           {
                                                               w.dice_formula = new DiceFormula(4, DiceType.D6);
                                                               w.bonus_damage = 3;
                                                               w.damage_type_description = Common.createForceDamageDescription();
                                                           }),
                                                           Helpers.Create<NewMechanics.EnchantmentMechanics.PersistentWeaponEnchantment>(p => p.enchant = fx_enchant),
                                                           Helpers.Create<RaiseBAB>(r => r.TargetValue = Helpers.CreateContextValue(AbilityRankType.Default)),
                                                           Helpers.CreateContextRankConfig(),
                                                           Helpers.CreateAddStatBonus(StatType.AdditionalAttackBonus, 3, ModifierDescriptor.Enhancement),
                                                           Helpers.CreateAddContextStatBonus(StatType.AdditionalAttackBonus, ModifierDescriptor.UntypedStackable, rankType: AbilityRankType.StatBonus),
                                                           Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, type: AbilityRankType.StatBonus, customProperty: best_mental_stat_property)
                                                           );

            var summon_pool = library.CopyAndAdd<BlueprintSummonPool>("490248a826bbf904e852f5e3afa6d138", "MagesSwordSummonPool", "");
            var apply_summon_buff = Common.createContextActionApplyBuff(library.Get<BlueprintBuff>("0dff842f06edace43baf8a2f44207045"), Helpers.CreateContextDuration(), dispellable: false, is_permanent: true);
            var summon_weapon = Helpers.Create<SpiritualAllyMechanics.ContextActionSummonSpiritualAlly>(c =>
            {
                c.Blueprint = mages_sword_unit;
                c.category = WeaponCategory.Greatsword;
                c.use_deity_weapon = false;
                c.SummonPool = summon_pool;
                c.CountValue = Helpers.CreateContextDiceValue(DiceType.Zero, 1, 1);
                c.DoNotLinkToCaster = false;
                c.DurationValue = Helpers.CreateContextDuration(1000);
                c.AfterSpawn = Helpers.CreateActionList(apply_summon_buff,
                                                        Common.createContextActionApplyBuff(weapon_metamagic_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true),
                                                        Common.createContextActionApplyBuff(mages_sword_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true)
                                                        );
                c.custom_name = "Mage's Sword";
                c.attack_mark_buff = mark_buff;
            });

            var clear_summon_pool = Helpers.Create<NewMechanics.ContextActionClearSummonPoolFromCaster>(c => c.SummonPool = summon_pool);
            var mark_ability = Helpers.CreateAbility("MagesSwordTargetSelectionAbility",
                                                     "Mage's Sword Target Change",
                                                     description,
                                                     "",
                                                     icon,
                                                     AbilityType.SpellLike,
                                                     UnitCommand.CommandType.Move,
                                                     AbilityRange.Medium,
                                                     "",
                                                     "",
                                                     Helpers.CreateRunActions(clear_summon_pool, apply_mark, summon_weapon),
                                                     Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                     Helpers.CreateSpellDescriptor(SpellDescriptor.Force),
                                                     Common.createAbilityCasterHasNoFacts(forbid_target_change_buff)
                                                     );
            mark_ability.setMiscAbilityParametersSingleTargetRangedHarmful();
            mark_ability.AvailableMetamagic = Metamagic.Maximize | Metamagic.Empower | (Metamagic)MetamagicFeats.MetamagicExtender.Toppling | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing;
            mark_buff.AddComponent(Helpers.CreateAddFactContextActions(deactivated: clear_summon_pool));

            var mages_sword_summoner_buff = Helpers.CreateBuff("MagesSwordSummonerBuff",
                                                                    "Mage's Sword Summoner",
                                                                    description,
                                                                    "",
                                                                    icon,
                                                                    null,
                                                                    Helpers.Create<ReplaceAbilityParamsWithContext>(a => a.Ability = mark_ability),
                                                                    Helpers.CreateAddFact(mark_ability),
                                                                    Helpers.CreateAddFactContextActions(deactivated: Helpers.Create<NewMechanics.ContextActionClearSummonPoolFromCaster>(c => c.SummonPool = summon_pool))
                                                                    );
            mages_sword_summoner_buff.Stacking = StackingType.Replace;
            var apply_summoner_buff = Common.createContextActionApplyBuffToCaster(mages_sword_summoner_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);

            mages_sword = Helpers.CreateAbility("MagesSwordAbility",
                                                "Mage's Sword",
                                                description,
                                                "",
                                                icon,
                                                AbilityType.Spell,
                                                UnitCommand.CommandType.Standard,
                                                AbilityRange.Medium,
                                                Helpers.roundsPerLevelDuration,
                                                "",
                                                Helpers.CreateRunActions(clear_summon_pool, apply_mark, apply_summoner_buff, summon_weapon),
                                                Helpers.CreateContextRankConfig(),
                                                Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.Force)
                                                );

            mages_sword.setMiscAbilityParametersSingleTargetRangedHarmful();
            mages_sword.AvailableMetamagic = mark_ability.AvailableMetamagic | Metamagic.Reach | Metamagic.Heighten | Metamagic.Quicken;

            mages_sword.AddToSpellList(Helpers.wizardSpellList, 7);
            mages_sword.AddSpellAndScroll("fbdd06f0414c3ef458eb4b2a8072e502");
        }



        static void createTwilightKnife()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/TwilightKnife.png");
            var description = "You create a darkly sinister floating knife that attacks the same creature as you each round. If you choose not to attack a creature, the knife makes no attack during that round.\n"
                                          + "The knife uses your base attack bonus modified by either your Intelligence or Charisma bonus (whichever is higher) when making this attack. Unless you specifically will it to do otherwise, or it proves impossible to do so, the knife always maneuvers itself so that it can flank your opponent before making the attack. The knife deals 1d4 points of force damage on a successful hit and has the same threat range and critical multipliers as a normal dagger. In addition, if the target is denied a Dexterity bonus to AC or the knife flanks the target, the knife can make sneak attacks as a rogue, inflicting an extra 1d6 points of force damage per four caster levels on a successful attack.";


            var mark_buff = Helpers.CreateBuff("TwilightKnifeMarkBuff",
                                               "Spiritual Weapon Target",
                                               description,
                                               "",
                                               icon,
                                               null,
                                               Helpers.Create<UniqueBuff>()
                                               );
            mark_buff.Stacking = StackingType.Stack;
            var apply_mark = Common.createContextActionApplyBuff(mark_buff, Helpers.CreateContextDuration(1), dispellable: false);

            var twilight_knife_buff = Helpers.CreateBuff("TwilightKnifeBuff",
                                                           "Twilight Knife",
                                                           "",
                                                           "",
                                                           null,
                                                           null,
                                                           Helpers.Create<NewMechanics.WeaponDamageChange>(w =>
                                                           {
                                                               w.dice_formula = new DiceFormula(1, DiceType.D4);
                                                               w.bonus_damage = 0;
                                                               w.damage_type_description = Common.createForceDamageDescription();
                                                           }),
                                                           Helpers.Create<NewMechanics.EnchantmentMechanics.PersistentWeaponEnchantment>(p => p.enchant = fx_enchant),
                                                           Helpers.Create<SpiritualAllyMechanics.SpiritualWeaponBab>(s =>
                                                           {
                                                               s.alternative_feature = null;
                                                               s.alternative_value = 0;
                                                           }
                                                           ),
                                                           Helpers.CreateAddContextStatBonus(StatType.AdditionalAttackBonus, ModifierDescriptor.UntypedStackable, rankType: AbilityRankType.StatBonus),
                                                           Helpers.CreateAddContextStatBonus(StatType.SneakAttack, ModifierDescriptor.UntypedStackable, rankType: AbilityRankType.DamageDice),
                                                           Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, type: AbilityRankType.StatBonus, customProperty: best_mental_stat_property),
                                                           Helpers.CreateContextRankConfig(type: AbilityRankType.DamageBonus, progression: ContextRankProgression.DivStep,
                                                                                           stepLevel: 3, max: 5),
                                                           Helpers.CreateContextRankConfig(type: AbilityRankType.DamageDice, progression: ContextRankProgression.DivStep,
                                                                                           stepLevel: 4)
                                                           );

            var summon_pool = library.CopyAndAdd<BlueprintSummonPool>("490248a826bbf904e852f5e3afa6d138", "TwilightKnifeSummonPool", "");
            var apply_summon_buff = Common.createContextActionApplyBuff(library.Get<BlueprintBuff>("6fcdf014694b2b542a867763b4369cb3"), Helpers.CreateContextDuration(), dispellable: false, is_permanent: true);
            var summon_weapon = Helpers.Create<SpiritualAllyMechanics.ContextActionSummonSpiritualAlly>(c =>
            {
                c.Blueprint = twilight_knife_unit;
                c.use_deity_weapon = false;
                c.category = WeaponCategory.Dagger;
                c.SummonPool = summon_pool;
                c.CountValue = Helpers.CreateContextDiceValue(DiceType.Zero, 1, 1);
                c.DoNotLinkToCaster = false;
                c.DurationValue = Helpers.CreateContextDuration(1000);
                c.AfterSpawn = Helpers.CreateActionList(apply_summon_buff,
                                                        Common.createContextActionApplyBuff(weapon_metamagic_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true),
                                                        Common.createContextActionApplyBuff(twilight_knife_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true));
                c.custom_name = "Twilight Knife";
                c.attack_mark_buff = mark_buff;
            });

            var clear_summon_pool = Helpers.Create<NewMechanics.ContextActionClearSummonPoolFromCaster>(c => c.SummonPool = summon_pool);

            mark_buff.AddComponent(Helpers.CreateAddFactContextActions(deactivated: clear_summon_pool));

            var action = Helpers.CreateConditional(Common.createContextConditionHasBuffFromCaster(mark_buff, not: true),
                                                   new GameAction[] { clear_summon_pool, apply_mark, summon_weapon }
                                                   );
            var twilight_knife_summoner_buff = Helpers.CreateBuff("TwilightKnifeSummonerBuff",
                                                                    "Twilight Knife Summoner",
                                                                    description,
                                                                    "",
                                                                    icon,
                                                                    null,
                                                                    Common.createAddInitiatorAttackRollTrigger2(Helpers.CreateActionList(action), only_hit: false),
                                                                    Helpers.CreateAddFactContextActions(deactivated: Helpers.Create<NewMechanics.ContextActionClearSummonPoolFromCaster>(c => c.SummonPool = summon_pool))
                                                                    );
            twilight_knife_summoner_buff.Stacking = StackingType.Replace;
            var apply_summoner_buff = Common.createContextActionApplyBuffToCaster(twilight_knife_summoner_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);

            twilight_knife = Helpers.CreateAbility("TwilightKnifeAbility",
                                                     "Twilight Knife",
                                                     description,
                                                     "",
                                                     icon,
                                                     AbilityType.Spell,
                                                     UnitCommand.CommandType.Standard,
                                                     AbilityRange.Personal,
                                                     Helpers.roundsPerLevelDuration,
                                                     "",
                                                     Helpers.CreateRunActions(apply_summoner_buff),
                                                     Helpers.CreateContextRankConfig(),
                                                     Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                     Helpers.CreateSpellDescriptor(SpellDescriptor.Force)
                                                     );

            twilight_knife.setMiscAbilityParametersSelfOnly();
            twilight_knife.AvailableMetamagic = Metamagic.Maximize | Metamagic.Empower | (Metamagic)MetamagicFeats.MetamagicExtender.Toppling | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | Metamagic.Reach | Metamagic.Heighten | Metamagic.Quicken;


            twilight_knife.AddToSpellList(Helpers.wizardSpellList, 3);
            twilight_knife.AddSpellAndScroll("fbdd06f0414c3ef458eb4b2a8072e502");
        }
    }
}
