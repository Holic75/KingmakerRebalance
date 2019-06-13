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

namespace KingmakerRebalance
{
    class Wildshape
    {
        static internal BlueprintAbility beast_shape1 = library.Get<BlueprintAbility>("61a7ed778dd93f344a5dacdbad324cc9");

        static internal LibraryScriptableObject library => Main.library;
        static internal BlueprintBuff leopard_form = Main.library.Get<BlueprintBuff>("200bd9b179ee660489fe88663115bcbc");
        static internal BlueprintBuff bear_form = Main.library.Get<BlueprintBuff>("0c0afabcfddeecc40a1545a282f2bec8");
        static internal BlueprintBuff smilodon_form = Main.library.Get<BlueprintBuff>("c38def68f6ce13b4b8f5e5e0c6e68d08");

        static internal BlueprintAbility turn_back = library.Get<BlueprintAbility>("bd09b025ee2a82f46afab922c4decca9");
        static internal BlueprintBuff dire_wolf_form;
        static internal BlueprintBuff mastodon_form;
        static internal BlueprintBuff hodag_form;
        static internal BlueprintBuff winter_wolf_form;
        static internal BlueprintBuff mandragora_form;
        static internal BlueprintBuff shambling_mound_form;
        static internal BlueprintBuff treant_form;
        static internal BlueprintBuff giant_flytrap_form;

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

        static internal BlueprintAbility bear_form_spell;
        static internal BlueprintAbility dire_wolf_form_spell;
        static internal BlueprintAbility smilodon_form_spell = library.Get<BlueprintAbility>("502cd7fd8953ac74bb3a3df7e84818ae");
        static internal BlueprintAbility mastodon_form_spell;
        static internal BlueprintAbility leopard_form_spell;
        static internal BlueprintAbility hodag_form_spell;
        static internal BlueprintAbility winter_wolf_form_spell;
        static internal BlueprintAbility plant_shapeI;
        static internal BlueprintAbility plant_shapeII;
        static internal BlueprintAbility plant_shapeIII;

        

        static internal void fixBeastShape()
        {
            fixBeastShape1();
            fixBeastShape2();
            fixBeastShape3();
            fixBeastShape4();
            fixPolymorph1();
            fixPolymorph2();

            createPlantShapeI();
            createPlantShapeII();
            createPlantShapeIII();

            //add giant form I, II and shapechange
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

            plant_shapeI = replaceForm(beast_shape1, mandragora_form, "PlantShapeISpell", "Plant Shape I",
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

            plant_shapeII = replaceForm(beast_shape1, shambling_mound_form, "PlantShapeIISpell", "Plant Shape II",
                                                "You become a Large shambling mound. You gain a +4 size bonus to your Strength, a +2 size bonus to your Constitution, +4 natural armor bonus, resist fire 20, and resist electricity 20. Your movement speed is reduced by 10 feet. You also have two 2d6 slam attacks, the constricting vines ability, and the poison ability.\nConstricting Vines: A shambling mound's vines coil around any creature it hits with a slam attack. The shambling mound attempts a grapple maneuver check against its target, and on a successful check its vines deal 2d6+5 damage and the foe is grappled.\nGrappled characters cannot move, and take a -2 penalty on all attack rolls and a -4 penalty to Dexterity. Grappled characters attempt to escape every round by making a successful combat maneuver, Strength, Athletics, or Mobility check. The DC of this check is the shambling mound's CMD.\nEach round, creatures grappled by a shambling mound suffer 4d6+Strength modifier × 2 damage.\nA shambling mound receives a +4 bonus on grapple maneuver checks.\nPoison:\nSlam; Save: Fortitude\nFrequency: 1/round for 2 rounds\nEffect: 1d2 Strength and 1d2 Dexterity damage\nCure: 1 save\nThe save DC is Constitution-based.");

            plant_shapeII.AddToSpellList(Helpers.alchemistSpellList, 6);
            plant_shapeII.AddToSpellList(Helpers.wizardSpellList, 6);
            plant_shapeII.SetIcon(entangle.Icon);
            Helpers.AddSpellAndScroll(plant_shapeII, "5022612735a9e2345bfc5110106823d8");
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
                                                 "You are in giant flytrap form now. You have a +8 size bonus to your Strength, +4 to Constitution, -2 penalty to Dexterity and a +6 natural armor bonus. You also have four 1d8 bite attacks, acid Resistance 20, blindsight and poison ability.",
                                                 entangle.Icon,
                                                 giant_flytrap,
                                                 8, -2, 6, 6, 0, Size.Huge,
                                                 giant_flytrap_bite, giant_flytrap_bite, new BlueprintItemWeapon[] { giant_flytrap_bite, giant_flytrap_bite },
                                                 library.Get<BlueprintFeature>("416386972c8de2e42953533c4946599a"), //acid resistance
                                                 library.Get<BlueprintFeature>("236ec7f226d3d784884f066aa4be1570"), //blindsight
                                                 library.Get<BlueprintFeature>("1180eb46f39f0cd41a0b2e293d1502cb") //poison
                                                 );

            var treant_form_spell = replaceForm(smilodon_form_spell, treant_form, "PlantShapeIIITreantAbility", "Plant Shape III (Treant)",
                                     "You become a Huge treant. You gain a +8 size bonus to your Strength, +4 to Constitution, -2 penalty to Dexterity and a +6 natural armor bonus. You also gain two 2d6 slam attacks, damage reduction 10/slashing, vulnerability to fire and trample ability.");
            treant_form_spell.RemoveComponents<SpellListComponent>();
            treant_form_spell.SetIcon(entangle.Icon);
            var giant_flytrap_form_spell = replaceForm(smilodon_form_spell, giant_flytrap_form, "PlantShapeIIIGiantFlytrapAbility", "Plant Shape III (Giant Flytrap)",
                                                 "You become a Huge giant flytrap. You gain a +8 size bonus to your Strength, +4 to Constitution, -2 penalty to Dexterity and a +6 natural armor bonus. You also gain four 1d8 bite attacks, acid Resistance 20, blindsight and poison ability.");

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
            beast_shape1.SetDescription("You become a Medium leopard. You gain a +2 size bonus to your Strength and a +2 natural armor bonus. You also gain two 1d3 claw attacks and one 1d6 bite attack.");
            beast_shape1.GetComponent<AbilityTargetHasFact>().CheckedFacts[0] = leopard_form;
            leopard_form.GetComponent<Polymorph>().Facts = leopard_form.GetComponent<Polymorph>().Facts.AddToArray(trip_defense_4legs);
            ((ContextActionApplyBuff)beast_shape1.GetComponent<AbilityEffectRunAction>().Actions.Actions[0]).Buff = leopard_form;
            leopard_form_spell = beast_shape1;
        }


        static void fixBeastShape2()
        {
            var beast_shape2 = library.Get<BlueprintAbility>("5d4028eb28a106d4691ed1b92bbb1915");
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
                                                 "You are in hodag form now. You have a +6 size bonus to your Strength, +2 bonus to your Constitution, -2 penalty to Dexterity, and a +6 natural armor bonus. You also have one 1d8 bite attack, two 1d6 claw attacks, one 1d8 tail attack and poison ability.",
                                                 beast_shape4.Icon,
                                                 hodag,
                                                 6, -2, 2, 6, 0, Size.Large,
                                                 library.Get<BlueprintItemWeapon>("c76f72a862d168d44838206524366e1c"),
                                                 library.Get<BlueprintItemWeapon>("c76f72a862d168d44838206524366e1c"),
                                                 new BlueprintItemWeapon[]{library.Get<BlueprintItemWeapon>("ec35ef997ed5a984280e1a6d87ae80a8"),
                                                                           library.Get<BlueprintItemWeapon>("ae822725634c6f0418b8c48bd29df255")
                                                                           },
                                                 trip_defense_4legs, library.Get<BlueprintFeature>("06b3d7ac8c130c947b1bebf82690194d"));

            hodag_form_spell = replaceForm(smilodon_form_spell, hodag_form, "BeastShapeIVHodagAbility", "Beast Shape IV (Hodag)",
                                            "You become a Large hodag. You gain a +6 size bonus to your Strength, +2 bonus to your Constitution, -2 penalty to Dexterity, and a +6 natural armor bonus. You also gain one 1d8 bite attack, two 1d6 claw attacks, one 1d8 tail attack and poison ability.");



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
            var cooldown_action = new AbilityExecuteActionOnCast();
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

            variants.Variants = variants.Variants.AddToArray(polymorph_bear, polymorph_dire_wolf);
            polymorph_spell.SetDescription("This spell transforms an allied creature into a leopard, a Large bear, a Large dire wolf or a small elemental. The subject may choose to resume its normal form as a full-round action; doing so ends the spell for that subject.");
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
    }
}
