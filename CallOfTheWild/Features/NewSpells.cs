using CallOfTheWild.NewMechanics;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Designers.Mechanics.Recommendations;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Buffs.Conditions;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityModManagerNet;

namespace CallOfTheWild
{
    public class NewSpells
    {
        static public BlueprintFeature immunity_to_wind;
        static public BlueprintBuff ranged_attacks_forbidden_buff;
        static public LibraryScriptableObject library => Main.library;
        static public BlueprintAbility shillelagh;
        static public BlueprintAbility flame_blade;
        static public BlueprintAbility flame_blade_electric;
        static public BlueprintAbility virtuoso_performance;
        static public BlueprintAbility deadly_juggernaut;
        static public BlueprintBuff deadly_juggernaut_buff;
        static public BlueprintAbility invisibility_purge;
        static public BlueprintAbility sanctuary;
        static public BlueprintBuff sanctuary_buff;

        static public BlueprintAbility command;
        static public BlueprintAbility command_greater;
        static public BlueprintAbility fire_shield;
        static public Dictionary<DamageEnergyType, BlueprintBuff> fire_shield_buffs = new Dictionary<DamageEnergyType, BlueprintBuff>();
        static public Dictionary<DamageEnergyType, BlueprintAbility> fire_shield_variants = new Dictionary<DamageEnergyType, BlueprintAbility>();


        static public BlueprintAbility strong_jaw;
        static public BlueprintAbility contingency;
        static public BlueprintAbility produce_flame;
        static public BlueprintAbility flurry_of_snowballs;
        static public BlueprintAbility ice_slick;
        static public BlueprintAbility vine_strike;
        static public BlueprintAbility sheet_lightning;
        static public BlueprintAbility poison_breath;

        static public BlueprintAbility countless_eyes;
        static public BlueprintAbility righteous_vigor;
        static public BlueprintAbility force_sword;
        static public BlueprintAbility blood_armor;
        static public BlueprintAbility flame_arrow;
        static public BlueprintAbility keen_edge;

        static public BlueprintAbility savage_maw;
        static public BlueprintAbility blood_mist;
        static public BlueprintAbility winter_grasp;

        static public BlueprintAbility frost_bite;
        static public BlueprintWeaponType touch_slam_cold;
        static public BlueprintAbility chill_touch;
        static public BlueprintWeaponType touch_slam_negative;


        static public BlueprintAbility earth_tremor;
        static public BlueprintAbility bone_fists;
        static public BlueprintAbility explosion_of_rot;

        static public BlueprintAbility aura_of_doom;
        static public BlueprintAbility archons_trumpet;

        static public BlueprintAbility burning_entanglement;
        static public BlueprintAbility bow_spirit;
        static public BlueprintAbility ray_of_exhaustion;

        static public BlueprintAbility obscuring_mist;
        static public BlueprintAbilityAreaEffect obscuring_mist_area;
        static public BlueprintAbility burst_of_radiance;

        static public BlueprintAbility river_of_wind;
        static public BlueprintAbility winds_of_vengeance;

        static public BlueprintAbility blistering_invective;
        static public BlueprintAbility flashfire;
        static public BlueprintAbility stricken_heart;
        static public BlueprintAbility rigor_mortis;
        
        static public BlueprintAbility ice_body;

        static public BlueprintAbility fire_seeds;

        static public BlueprintAbility suffocation, mass_suffocation;
        static public BlueprintBuff suffocation_buff;
        static public BlueprintBuff fast_suffocation_buff;
        static public BlueprintAbility fluid_form;

        static public BlueprintAbility hold_person_mass;
        static public BlueprintAbility irresistible_dance;
        static public BlueprintAbility particulate_form;
        static public BlueprintAbility curse_major;
        static public BlueprintAbility hold_monster_mass;

        static public BlueprintAbility freezing_sphere;

        static public BlueprintAbility howling_agony;
        static public BlueprintAbility spite;
        static public BlueprintAbility forceful_strike;

        static public BlueprintAbility ghoul_touch;
        static public BlueprintAbility fleshworm_infestation;
        static public BlueprintAbility rebuke;

        static public BlueprintAbility bladed_dash;
        static public BlueprintAbility bladed_dash_greater;
        static public BlueprintAbility dimension_door_free;
        static public BlueprintBuff bladed_dash_buff;

        static public BlueprintAbility fiery_runes;

        static public BlueprintAbility lend_judgement;
        static public BlueprintAbility lend_judgement_greater;
        static public BlueprintAbility flames_of_the_faithful;

        static public BlueprintAbility magic_weapon;
        static public BlueprintAbility magic_weapon_greater;

        static public BlueprintAbility dazzling_blade;
        static public BlueprintAbility dazzling_blade_mass;

        static public BlueprintAbility accursed_glare;
        static public BlueprintAbility solid_fog;
        static public BlueprintAbilityAreaEffect solid_fog_area;
        static public BlueprintAbility thirsting_entanglement;

        static public BlueprintAbility resinous_skin;
        static public BlueprintAbility delayed_consumption;

        static public BlueprintAbility long_arm;
        static public BlueprintAbility blade_lash;
        static public BlueprintAbility stunning_barrier_greater;
        static public BlueprintAbility tidal_surge;

        static public BlueprintAbility path_of_glory;
        static public BlueprintAbility path_of_glory_greater;

        static public BlueprintAbility meteor_swarm;

        static public BlueprintAbility fly;
        static public BlueprintAbility fly_mass;
        static public BlueprintBuff fly_buff;
        static public BlueprintAbility air_walk;
        static public BlueprintAbility air_walk_communal;
        static public BlueprintAbility overland_flight;
        static public BlueprintAbility wind_walk;

        static public BlueprintAbility hypnotic_pattern;

        static public BlueprintAbility wall_of_nausea;
        static public BlueprintAbility wall_of_blindness;
        static public BlueprintAbility wall_of_fire;
        static public BlueprintAbility wall_of_fire_fire_domain;
        static public BlueprintAbility incendiary_cloud;
        static public BlueprintAbility scouring_winds;
        static public BlueprintAbility aggressive_thundercloud;
        static BlueprintBuff thunder_cloud_fx_buff;
        static public BlueprintAbility aggressive_thundercloud_greater;

        static public BlueprintAbility threefold_aspect;
        static public BlueprintAbility sands_of_time;

        static public BlueprintAbility temporal_stasis;
        static public BlueprintAbility time_stop;

        static public BlueprintAbility sleet_storm;
        static public BlueprintAbility control_undead;
        static public BlueprintAbility halt_undead;

        static public BlueprintAbility touch_of_blood_letting;
        static public BlueprintAbility bloody_claws;

        static public BlueprintAbility[] mind_thrust = new BlueprintAbility[6];
        static public BlueprintAbility[] thought_shield = new BlueprintAbility[3];
        static public BlueprintAbility[] mental_barrier = new BlueprintAbility[5];
        static public BlueprintAbility intellect_fortress;

        static public BlueprintAbility consecrate;
        static public BlueprintAbilityAreaEffect consecrate_area;
        static public BlueprintAbility desecrate;
        static public BlueprintAbilityAreaEffect desecrate_area;
        static public BlueprintAbility animate_dead_lesser;
        static public BlueprintUnit animate_dead_skeleton;

        static public BlueprintAbility barrow_haze;
        static public BlueprintAbility screech;
        static public BlueprintAbility fickle_winds;

        static public BlueprintAbility fiery_shiriken;

        static public BlueprintAbility burst_of_adrenaline;
        static public BlueprintAbility burst_of_insight;
        static public BlueprintAbility synapse_overload;
        static public BlueprintAbility burst_of_force;

        static public BlueprintAbility[] psychic_crush = new BlueprintAbility[5];

        static public BlueprintAbility pain_strike, pain_strike_mass;
        static public BlueprintAbility inflict_pain, inflict_pain_mass;
        static public BlueprintAbility synesthesia, synesthesia_mass;
        static public BlueprintAbility iron_body;
        static public BlueprintAbility orb_of_the_void;
        static public BlueprintAbility psychic_surgery;
        static public BlueprintAbility akashic_form;
        static public BlueprintAbility divide_mind;
        static public BlueprintAbility telekinetic_storm;

        static public BlueprintBuff nauseted_non_poison;

        static public BlueprintAbility synaptic_pulse;
        static public BlueprintAbility synaptic_pulse_greater;
        static public BlueprintAbility babble;
        static public BlueprintAbility song_of_discord_greater;
        static public BlueprintAbility silence;
        static public BlueprintBuff silence_buff;

        static public BlueprintAbility shadow_enchantment;
        static public BlueprintAbility shadow_enchantment_greater;
        static public BlueprintAbility shadow_conjuration;
        static public BlueprintAbility shadow_conjuration_greater;
        static public BlueprintAbility shades;
        static public BlueprintAbility wrathful_weapon;
        static public BlueprintAbility blade_tutor;
      
        static public BlueprintAbility channel_vigor;
        static public BlueprintAbility control_construct;

        static public BlueprintAbility telekinetic_strikes;
        static public BlueprintAbility debilitating_portent;

        static public BlueprintAbility daze_mass;

        static public BlueprintAbility invigorate;
        static public BlueprintAbility invigorate_mass;
        static public BlueprintAbility cloak_of_winds;
        static public BlueprintAbility spirit_bound_blade;
        static public BlueprintAbility phantom_limbs;
        static public BlueprintAbility ghostbane_dirge;
        static public BlueprintAbility ghostbane_dirge_mass;
        static public BlueprintAbility arcane_concordance;

        static public BlueprintAbility shadow_claws;
        static public BlueprintAbility second_wind;
        static public BlueprintAbility wracking_ray;
        static public BlueprintAbility smite_abomination;

        static public BlueprintAbility corrosive_consumption;
        static public BlueprintAbility warding_weapon;
        static public BlueprintAbility globe_of_invulnerability;
        static public BlueprintAbility globe_of_invulnerability_lesser;
        static public BlueprintAbility locate_weakness;

        static public BlueprintAbility weapon_of_awe;
        static public BlueprintAbility allied_cloak;

        static public BlueprintAbility miracle;


        //binding_earth; ?
        //binding_earth_mass ?
        //battle mind link ?
        //condensed ether ?

        //allied cloak
        //implosion
        //blood rage
        //etheric shards
        //tactical acumen
        //weapon of awe
        //flaming sphere (greater)

        static public void load()
        {
            fixPhantasmalSpells();
            createSilence();
            createImmunityToWind();
            createShillelagh();
            createFlameBlade();
            createFlameBladeElectric();
            createVirtuosoPerformance();
            createDeadlyJuggernaut();
            createInvisibilityPurge();
            createSanctuary();
            createCommand();
            createFireShield();
            createContingency();
            createDelayedConsumption();
            createStrongJaw();
            createProduceFlame();
            createFlurryOfSnowballs();
            createIceSlick();
            createSheetLightning();
            createVineStrike();
            createPoisonBreath();

            createCountlessEyes();
            createRighteousVigor();
            createForceSword();
            createBloodArmor();
            createKeenEdge();
            createFlameArrow();

            createSavageMaw();
            createBloodMist();
            createWinterGrasp();

            createFrostBite();
            createChillTouch();
            createEarthTremor();
            createExplosionOfRot();
            createBoneFists();
            createAuraOfDoom();
            createArchonsTrumpet();
            createBurningEntanglement();
            createBowSpirit();
            createRayOfExhaustion();
            createObscuringMist();

            createBurstOfRadiance();
            createRiverOfWind();
            createWindsOfVengeance();

            createBlisteringInvective();
            createStrickenHeart();
            createFlashfire();
            createRigorMortis();
            createSuffocation();
            createIceBody();

            createFireSeeds();
            createFluidForm();

            createCurseMajor();
            createHoldPersonMass();
            createParticulateForm();
            createIrresistibleDance();
            createHoldMonsterMass();
            createFreezingSphere();

            createHowlingAgony();
            createSpite();
            createForcefulStrike();

            createGhoulTouch();
            createFleshwormInfestation();
            createRebuke();

            createBladedDash();
            createFieryRunes();

            createLendJudgement();
            createFlamesOfTheFaithful();
            createMagicWeapon();
            createDazzlingBlade();

            createAccursedGlare();
            createSolidFogAndFixAcidFog();
            createThirstingEntanglement();

            createResinousSkin();

            createLongArm();
            createBladeLash();
            createStunningBarrierGreater();

            createTidalSurge();
            createPathOfGlory();
            createPathOfGloryGreater();

            createMeteorSwarm();
            createFlyAndOverlandFlight();
            createAirWalk();
            createWindWalk();

            createHypnoticPattern();

            createWallOfNausea();
            createWallOfBlindness();
            createWallOfFire();
            createIncendiaryCloud();
            createScouringWinds();

            createAggressiveThunderCloud();
            createAggressiveThunderCloudGreater();

            createThreefoldAspect();
            createSandsOfTime();

            createTemporalStasis();
            createTimeStop();

            createSleetStorm();
            createControlUndead();
            createHaltUndead();

            createTouchOfBloodletting();
            createBloodyClaws();

            createMindThrust();
            createThoughtShield();
            createMentalBarrier();
            createIntellectFortress();

            createConsecreate();
            createDesecrate();
            createAnimateDeadLesser();

            createBarrowHaze();
            createFickleWinds();
            createFireShuriken();

            createBurstOfAdrenaline();
            createBurstOfInsight();
            createSynapseOverload();
            createBurstOfForce();
            createPsychicCrush();
            createPainStrike();
            createInflictPain();
            createSynesthesia();

            createIronBody();
            createOrbOfTheVoid();
            createPsychicSurgery();
            createDivideMind();
            createTelekineticStorm();
            createAkashicForm();
            createSynapticPulse();
            createBabble();
            createSongOfDiscordGreater();

            createShadowEnchantment();
            createShadowConjuration();
            createWrathfulWeapon();
            createBladeTutor();
            createChannelVigor();
            createControlConstruct();

            createTelekinticStrikes();
            createDebilitatingPortent();

            createDazeMass();
            createInvigorateAndInvigorateMass();
            createCloakOfWinds();

            createSpiritBoundBlade();
            createPhantomLimbs();
            createGhostbaneDirge();
            createArcaneConcordance();

            createShadowClaws();
            createSecondWind();
            createWrackingRay();
            createSmiteAbomination();
            SpiritualWeapons.load();

            createCorrosiveConsumption();
            createWardingWeapon();
            createGlobeOfInvulnerability();
            createLocateWeakness();

            createWeaponOfAwe();
            createAlliedCloak();



            //createMiracle();
        }


        static void createMiracle()
        {
            var blacklist_guids = Helpers.readStringsfromFile(UnityModManager.modsPath + @"/CallOfTheWild/WishBlackLists/miracle_black_list.txt", ' ');
            miracle = createWish("Miracle",
                                 "Miracle",
                                 "You don’t so much cast a miracle as request one. You state what you would like to have happen and request that your deity (or the power you pray to for spells) intercede.\n"
                                 + "A miracle can do any of the following things.\n"
                                 + "Duplicate any cleric spell of 8th level or lower.\n"
                                 + "Duplicate any other spell of 7th level or lower.\n",
                                 //+ "Undo the harmful effects of certain spells, such as feeblemind or insanity.\n"
                                 //+ "Have any effect whose power level is in line with the above effects.\n",
                                 LoadIcons.Image2Sprite.Create(@"AbilityIcons/Wish.png"),
                                 library.Get<BlueprintSpellbook>("4673d19a0cf2fab4f885cc4d1353da33"),
                                 UnitCommand.CommandType.Standard,
                                 blacklist_guids,
                                 AbilityType.Spell,
                                 full_round: false);
            miracle.AddComponent(Helpers.CreateSpellComponent(SpellSchool.Evocation));
            miracle.AddComponent(Helpers.Create<SpellManipulationMechanics.WishSpell>());
            miracle.AddToSpellList(Helpers.clericSpellList, 9);
            Helpers.AddSpell(miracle);
        }

        static void createAlliedCloak()
        {
            var buff = Helpers.CreateBuff("AlliedCloakBuff",
                                          "Allied Cloak",
                                          "You cause a cloak, shawl, poncho, or other outer garment you are wearing to animate to aid and defend you. The cloak provides a +2 shield bonus to your AC. In addition, once each round during your turn, you can take a free action to direct your cloak to use the aid another action to assist your attack roll, or AC.",
                                          "",
                                          Rebalance.aid_self_free.Icon,
                                          null,
                                          Helpers.CreateAddStatBonus(StatType.AC, 2, ModifierDescriptor.Shield),
                                          Helpers.CreateAddFact(Rebalance.aid_self_free)
                                         );

            allied_cloak = Helpers.CreateAbility("AlliedCloakAbility",
                                                    buff.Name,
                                                    buff.Description,
                                                    "",
                                                    buff.Icon,
                                                    AbilityType.Spell,
                                                    UnitCommand.CommandType.Standard,
                                                    AbilityRange.Personal,
                                                    Helpers.roundsPerLevelDuration,
                                                    "",
                                                    Helpers.CreateRunActions(Common.createContextActionApplySpellBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)))),
                                                    Helpers.CreateSpellComponent(SpellSchool.Abjuration),
                                                    Common.createAbilitySpawnFx("c4d861e816edd6f4eab73c55a18fdadd", anchor: AbilitySpawnFxAnchor.SelectedTarget)
                                                    );
            allied_cloak.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken;
            allied_cloak.setMiscAbilityParametersSelfOnly();
            allied_cloak.AddToSpellList(Helpers.wizardSpellList, 3);
            allied_cloak.AddToSpellList(Helpers.bardSpellList, 3);
            allied_cloak.AddToSpellList(Helpers.magusSpellList, 3);

            allied_cloak.AddSpellAndScroll("04011c80b5ffb054ba3027c32d07f8f8"); //cape of wasps

        }


        static void createWeaponOfAwe()
        {
            var shaken = library.Get<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220");

            var enchant = Common.createWeaponEnchantment("WeaponOfAweEnchantment",
                                          "Weapon of Awe",
                                          "You transform a single weapon into an awe-inspiring instrument. The weapon gains a +2 sacred bonus on damage rolls, and if the weapon scores a critical hit, the target of that critical hit becomes shaken for 1 round with no saving throw. This is a mind-affecting fear effect. A ranged weapon affected by this spell applies these effects to its ammunition.",
                                           "",
                                           "",
                                           "",
                                           5,
                                           null,
                                           Helpers.Create<NewMechanics.EnchantmentMechanics.WeaponDamageBonus>(c => c.damage  = 2),
                                           Common.createAddInitiatorAttackRollTrigger2(Helpers.CreateActionList(Common.createContextActionApplyBuff(shaken, Helpers.CreateContextDuration(1))), critical_hit: true)
                                          );


            var icon = Helpers.GetIcon("24fe1f546e07987418557837b0e0f8f5"); //keen light weapon

            var ability1 = library.CopyAndAdd<BlueprintAbility>("831e942864e924846a30d2e0678e438b", "WeaponOfAwePrimaryHandAbility", "");

            ability1.SetNameDescriptionIcon(enchant.Name, enchant.Description, icon);

            ability1.setMiscAbilityParametersTouchFriendly();
            ability1.RemoveComponents<AbilityDeliverTouch>();
            var action_old = (ability1.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionEnchantWornItem);

            var action = Common.createItemEnchantmentAction(enchant.name + "PrimaryHandWeaponOfAweBuff",
                                                Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes),
                                                enchant,
                                                true,
                                                off_hand: false
                                                );
            ability1.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(action));
            ability1.LocalizedDuration = Helpers.minutesPerLevelDuration;

            var ability2 = library.CopyAndAdd(ability1, "WeaponOfAweSecondaryHandAbility", "");
            ability2.SetName("Weapon of Awe (Off-Hand)");
            var action2 = Common.createItemEnchantmentAction(enchant.name + "SecondaryHandWeaponOfAweBuff",
                                                Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes),
                                                enchant,
                                                true,
                                                off_hand: true
                                                );
            ability2.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(action2));
            ability2.AddComponent(Helpers.Create<NewMechanics.AbilitTargetManufacturedWeapon>(a => { a.off_hand = true; a.works_on_summoned = true; }));
            ability1.AddComponent(Helpers.Create<NewMechanics.AbilitTargetManufacturedWeapon>(a => a.works_on_summoned = true));

            weapon_of_awe = Common.createVariantWrapper("WeaponOfAweAbility", "", ability1, ability2);
            weapon_of_awe.AddComponent(Helpers.CreateSpellComponent(SpellSchool.Transmutation));
            weapon_of_awe.AddToSpellList(Helpers.inquisitorSpellList, 2);
            weapon_of_awe.AddToSpellList(Helpers.clericSpellList, 2);
            weapon_of_awe.AddToSpellList(Helpers.paladinSpellList, 2);

            weapon_of_awe.AddSpellAndScroll("fbdd06f0414c3ef458eb4b2a8072e502");

        }


        static void createLocateWeakness()
        {
            var buff = Helpers.CreateBuff("LocateWeaknessBuff",
                              "Locate Weakness",
                              "You can sense your foes’ weak points, granting you greater damage with critical hits. Whenever you score a critical hit, your weapon damage is maximized.",
                              "",
                              Helpers.GetIcon("2c38da66e5a599347ac95b3294acbe00"),
                              null,
                              Helpers.Create<NewMechanics.MaximumWeaponDamageOnCriticalHit>()
                              );

            locate_weakness = Helpers.CreateAbility("LocateWeaknessAbility",
                                                   buff.Name,
                                                   buff.Description,
                                                   "",
                                                   buff.Icon,
                                                   AbilityType.Spell,
                                                   UnitCommand.CommandType.Standard,
                                                   AbilityRange.Personal,
                                                   Helpers.minutesPerLevelDuration,
                                                   "",
                                                   Helpers.CreateRunActions(Common.createContextActionApplySpellBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes))),
                                                   Common.createAbilitySpawnFx("8de64fbe047abc243a9b4715f643739f", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                   Helpers.CreateSpellComponent(SpellSchool.Divination),
                                                   Helpers.CreateContextRankConfig()
                                                   );
            locate_weakness.AvailableMetamagic = Metamagic.Extend | Metamagic.Quicken | Metamagic.Heighten;
            locate_weakness.setMiscAbilityParametersSelfOnly();
            locate_weakness.AddToSpellList(Helpers.wizardSpellList, 3);
            locate_weakness.AddToSpellList(Helpers.magusSpellList, 3);
            locate_weakness.AddToSpellList(Helpers.rangerSpellList, 2);
            locate_weakness.AddToSpellList(Helpers.inquisitorSpellList, 3);
            locate_weakness.AddSpellAndScroll("5b6d4c0bbd882074eb6b6996ea77b77c"); //true strike
        }


        static void createGlobeOfInvulnerability()
        {
            var buff_lesser = Helpers.CreateBuff("GlobeOfInvulnerabilityLesserBuff",
                                                  "Globe of Invulnerability, Lesser",
                                                  "An immobile, faintly shimmering magical sphere surrounds you and excludes all spell effects of 3rd level or lower. The area or effect of any such spells does not include the area of the lesser globe of invulnerability. Such spells fail to affect any target located within the globe. Excluded effects include spell-like abilities and spells or spell-like effects from items. Any type of spell, however, can be cast through or out of the magical globe. Spells of 4th level and higher are not affected by the globe, nor are spells already in effect when the globe is cast. The globe can be brought down by a dispel magic spell. You can leave and return to the globe without penalty.",
                                                  "",
                                                  LoadIcons.Image2Sprite.Create(@"AbilityIcons/Metamixing.png"),
                                                  null,
                                                  Helpers.Create<InvulnerabilityMechanics.ImmunityUpToSpellLevel>(i => i.max_spell_level = 3)
                                                  );
            var buff = Helpers.CreateBuff("GlobeOfInvulnerabilityBuff",
                                          "Globe of Invulnerability",
                                          "This spell functions like lesser globe of invulnerability, except that it also excludes 4th-level spells and spell-like effects.\n"
                                          + buff_lesser.Name + ": " + buff_lesser.Description,
                                          "",
                                          LoadIcons.Image2Sprite.Create(@"AbilityIcons/Metamixing.png"),
                                          null,
                                          Helpers.Create<InvulnerabilityMechanics.ImmunityUpToSpellLevel>(i => i.max_spell_level = 4)
                                          );
            var buffs = new BlueprintBuff[] {buff_lesser, buff };
            var abilities = new BlueprintAbility[2];

            for (int i = 0; i < buffs.Length; i++)
            {
                var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("7ced0efa297bd5142ab749f6e33b112b", buffs[i].name + "Area", "");
                area.Fx = Common.createPrefabLink("cda35ba5c34a61b499f5858eabcedec7");
                area.ReplaceComponent<AbilityAreaEffectBuff>(a =>
                {
                    a.Buff = buffs[i];
                    a.Condition = Helpers.CreateConditionsCheckerAnd();
                });

                abilities[i] = Helpers.CreateAbility(buffs[i].name + "Ability",
                                                     buffs[i].Name,
                                                     buffs[i].Description,
                                                     "",
                                                     buffs[i].Icon,
                                                     AbilityType.Spell,
                                                     UnitCommand.CommandType.Standard,
                                                     AbilityRange.Personal,
                                                     Helpers.roundsPerLevelDuration,
                                                     "",
                                                     Helpers.CreateRunActions(Common.createContextActionSpawnAreaEffect(area, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)))),
                                                     Helpers.CreateContextRankConfig(),
                                                     Helpers.CreateSpellComponent(SpellSchool.Abjuration),
                                                     Common.createAbilityAoERadius(13.Feet(), TargetType.Any)
                                                     );
                abilities[i].setMiscAbilityParametersSelfOnly();
                abilities[i].AvailableMetamagic = Metamagic.Extend | Metamagic.Quicken | Metamagic.Heighten;
            }

            globe_of_invulnerability_lesser = abilities[0];
            globe_of_invulnerability = abilities[1];
            globe_of_invulnerability.AddToSpellList(Helpers.wizardSpellList, 6);
            globe_of_invulnerability_lesser.AddToSpellList(Helpers.wizardSpellList, 4);
            globe_of_invulnerability.AddSpellAndScroll("59110d30bb15dcd4d89f762b6aa9db9b"); //protection from chaos
            globe_of_invulnerability_lesser.AddSpellAndScroll("59110d30bb15dcd4d89f762b6aa9db9b"); //protection from chaos
        }

        static void createWardingWeapon()
        {
            var buff = Helpers.CreateBuff("WardingWeaponBuff",
                                          "Warding Weapon",
                                          "The focus of this spell flies upward above your head and takes a defensive position within your space. It lunges at opponents, as if guided by a martially trained hand, parrying and turning back melee attacks aimed at you, but does not strike back at any opponent nor does it damage them. The weapon serves only as a defense. While it protects you, you can cast spells without provoking attacks of opportunity, without the need to cast them defensively.",
                                          "",
                                          LoadIcons.Image2Sprite.Create(@"AbilityIcons/WardingWeapon.png"),
                                          null,
                                          Helpers.Create<ConcentrationMechanics.DoNotProvokeAooOnSpellCast>()
                                          );

            warding_weapon = Helpers.CreateAbility("WardingWeaponAbility",
                                                   buff.Name,
                                                   buff.Description,
                                                   "",
                                                   buff.Icon,
                                                   AbilityType.Spell,
                                                   UnitCommand.CommandType.Standard,
                                                   AbilityRange.Personal,
                                                   Helpers.roundsPerLevelDuration,
                                                   "",
                                                   Helpers.CreateRunActions(Common.createContextActionApplySpellBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)))),
                                                   Common.createAbilitySpawnFx("c4d861e816edd6f4eab73c55a18fdadd", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                   Helpers.CreateSpellComponent(SpellSchool.Abjuration),
                                                   Helpers.CreateContextRankConfig()
                                                   );
            warding_weapon.AvailableMetamagic = Metamagic.Extend | Metamagic.Quicken | Metamagic.Heighten;
            warding_weapon.setMiscAbilityParametersSelfOnly();
            warding_weapon.AddToSpellList(Helpers.wizardSpellList, 2);
            warding_weapon.AddToSpellList(Helpers.magusSpellList, 1);
            warding_weapon.AddSpellAndScroll("fbdd06f0414c3ef458eb4b2a8072e502");
        }


        static void createCorrosiveConsumption()
        {
            var icon = Helpers.GetIcon("1e418794638cf95409f6e33c8c3dbe1a"); //elemental wall acid

            var dmg1 = Helpers.CreateActionDealDamage(DamageEnergyType.Acid, Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilityRankType.Default)));
            var dmg2 = Helpers.CreateActionDealDamage(DamageEnergyType.Acid, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D4), Helpers.CreateContextValue(AbilityRankType.Default), 0));
            var dmg3 = Helpers.CreateActionDealDamage(DamageEnergyType.Acid, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.Default), 0));

            var buff = Helpers.CreateBuff("CorrosiveConsumptionBuff",
                                          "Corrosive Consumption",
                                          $"With a touch, this spell causes a small, rapidly growing patch of corrosive acid to appear on the target. On the first round, the acid deals 1 point of acid damage per caster level (maximum 15). On the second round, the acid patch grows and deals 1d{BalanceFixes.getDamageDieString(DiceType.D4)} points of acid damage per caster level (maximum 15d{BalanceFixes.getDamageDieString(DiceType.D4)}). On the third and final round, the acid patch covers the entire creature and deals 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of acid damage per caster level (maximum 15d{BalanceFixes.getDamageDieString(DiceType.D6)}).",
                                          "",
                                          icon,
                                          Common.createPrefabLink("4debc3ac7f4781042935ca6c61b1b0e9"), //acid theme
                                          Helpers.CreateAddFactContextActions(activated: dmg1,
                                                                              newRound: Helpers.CreateConditional(Common.createBuffConditionCheckRoundNumber(2), dmg2,
                                                                                                                  Helpers.CreateConditional(Common.createBuffConditionCheckRoundNumber(3),
                                                                                                                                            dmg3
                                                                                                                                            )
                                                                                                                  )
                                                                             ),
                                          Helpers.CreateContextRankConfig(max: 15, feature: MetamagicFeats.intensified_metamagic),
                                          Helpers.CreateSpellDescriptor(SpellDescriptor.Acid)
                                          );

            var ability = Helpers.CreateAbility("CorrosiveTouchAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Spell,
                                                UnitCommand.CommandType.Standard,
                                                AbilityRange.Touch,
                                                "3 rounds",
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), is_from_spell: true, duration_seconds: 18)),
                                                Helpers.CreateDeliverTouch(),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.Acid),
                                                Helpers.CreateSpellComponent(SpellSchool.Conjuration),
                                                Common.createAbilitySpawnFx("524f5d0fecac019469b9e58ce1b8402d", anchor: AbilitySpawnFxAnchor.SelectedTarget)
                                                );
            ability.setMiscAbilityParametersTouchHarmful();
            ability.SpellResistance = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Reach | Metamagic.Quicken | Metamagic.Heighten | (Metamagic)MetamagicFeats.MetamagicExtender.IntensifiedGeneral | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;

            corrosive_consumption = Helpers.CreateTouchSpellCast(ability);
            corrosive_consumption.AddToSpellList(Helpers.wizardSpellList, 5);
            corrosive_consumption.AddToSpellList(Helpers.magusSpellList, 5);
            corrosive_consumption.AddSpellAndScroll("68d5aa212b7323e4e95e0fe731ea50cf");
        }


        static void createSmiteAbomination()
        {
            smite_abomination = library.CopyAndAdd<BlueprintAbility>("7bb9eb2042e67bf489ccd1374423cdec", "SmiteAbominationAbility", "");

            smite_abomination.SetName("Smite Abomination");
            smite_abomination.SetDescription("Drawing upon positive energy, you emulate some of a paladin’s power to smite undead. Choose one undead creature as your target. You gain a bonus equal to your Charisma or Wisdom modifier, whichever is higher, on your attack rolls, and as deflection bonus to your ac against the target, and a bonus equal to your caster level on damage rolls. Your attacks also bypass the target’s damage reduction. These bonuses do not stack with the bonuses from a paladin’s smite.");
            smite_abomination.LocalizedDuration = Helpers.CreateString("SmiteAbomination.Duration", Helpers.roundsPerLevelDuration);
            smite_abomination.RemoveComponents<AbilityCasterAlignment>();
            smite_abomination.RemoveComponents<AbilityResourceLogic>();
            smite_abomination.RemoveComponents<ContextRankConfig>();

            var wis_cha_property = NewMechanics.HighestStatPropertyGetter.createProperty("SmiteAbominationWisChaUnitProperty", "", StatType.Wisdom, StatType.Charisma);
            smite_abomination.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, type: AbilityRankType.StatBonus, customProperty: wis_cha_property));
            smite_abomination.AddComponent(Helpers.CreateContextRankConfig(type: AbilityRankType.DamageBonus, max: 20));
            smite_abomination.AddComponent(Helpers.CreateSpellComponent(SpellSchool.Evocation));

            var smite_action = smite_abomination.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>();

            var old_conditional = (Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional)smite_action.Actions.Actions[0];
            var conditions = new Condition[] {Common.createContextConditionHasFact(Common.undead), old_conditional.ConditionsChecker.Conditions[1] };

            var smite_buff = ((ContextActionApplyBuff)old_conditional.IfTrue.Actions[0]).Buff;
            var apply_smite_buff = Common.createContextActionApplyBuff(smite_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.DamageBonus)), is_from_spell: true);
            var new_smite_action = Helpers.CreateConditional(conditions, new GameAction[] { apply_smite_buff }, null);
            smite_abomination.ReplaceComponent(smite_action, Helpers.CreateRunActions(new_smite_action));
            smite_abomination.Type = AbilityType.Spell;
            smite_abomination.ActionType = UnitCommand.CommandType.Standard;

            smite_abomination.AddToSpellList(Helpers.inquisitorSpellList, 4);
            smite_abomination.AddToSpellList(Helpers.clericSpellList, 5);

            smite_abomination.AddSpellAndScroll("8f01e5cb9e8ff8244b827185bb9c93f9"); //crusaders edge
            smite_abomination.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach;
        }


        static void createWrackingRay()
        {
            wracking_ray = library.CopyAndAdd<BlueprintAbility>("450af0402422b0b4980d9c2175869612", "WrackingRayAbility", "");//ray of enfeeblement
            wracking_ray.RemoveComponents<SpellListComponent>();
            wracking_ray.RemoveComponents<AbilityEffectRunAction>();
            wracking_ray.RemoveComponents<SpellDescriptorComponent>();
            wracking_ray.LocalizedDuration = Helpers.CreateString("WrackingRay.Duration", "");
            wracking_ray.LocalizedSavingThrow = Helpers.CreateString("WrackingRay.Savingthrow", "Fortitude half");
            wracking_ray.SetNameDescription("Wracking Ray",
                                            "A ray of sickly greenish-gray negative energy issues forth from the palm of your hand. Make a ranged touch attack against the target. A creature hit by this spell is wracked by painful spasms as its muscles and sinews wither and twist. The subject takes 1d4 points of Dexterity and Strength damage per 3 caster levels you possess (maximum 5d4 each). A successful Fortitude save halves the damage.");
            wracking_ray.Range = AbilityRange.Medium;
            wracking_ray.AvailableMetamagic = wracking_ray.AvailableMetamagic | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing | (Metamagic)MetamagicFeats.MetamagicExtender.IntensifiedGeneral;
            wracking_ray.CanTargetFriends = true;
            var dice = Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilitySharedValue.Damage));
            var str_damage = Helpers.CreateActionDealDamage(StatType.Strength, dice, halfIfSaved: true);
            var dex_damage = Helpers.CreateActionDealDamage(StatType.Dexterity, dice, halfIfSaved: true);
            wracking_ray.AddComponents(Helpers.CreateRunActions(SavingThrowType.Fortitude, str_damage, dex_damage),
                                       Helpers.CreateCalculateSharedValue(Helpers.CreateContextDiceValue(DiceType.D4, Helpers.CreateContextValue(AbilityRankType.Default), 0), AbilitySharedValue.Damage),
                                       Helpers.CreateContextRankConfig(progression: ContextRankProgression.DivStep, stepLevel: 3, max: 5, feature: MetamagicFeats.intensified_metamagic),
                                       Helpers.CreateSpellDescriptor(SpellDescriptor.Death | SpellDescriptor.Evil)
                                       );

            wracking_ray.AddToSpellList(Helpers.wizardSpellList, 5);
            wracking_ray.AddSpellAndScroll("792862674c565ad4fbb1ab0c97c42acd"); //ray of enfeeblement
        }


        static void createSecondWind()
        {
            second_wind = Helpers.CreateAbility("SecondWindAbility",
                                               "Second Wind",
                                               $"You can cast this spell only when you have fewer than one-quarter of your total hit points. With a gasping utterance, you summon invigorating air to fill your lungs. You heal 2d{BalanceFixes.getDamageDieString(DiceType.D8)} points of damage + 1 point per caster level (maximum +10).",
                                               "",
                                               Helpers.GetIcon("4ebaf39efb8ffb64baf92784808dc49c"), //destruction judgment
                                               AbilityType.Spell,
                                               UnitCommand.CommandType.Swift,
                                               AbilityRange.Personal,
                                               "",
                                               "",
                                               Helpers.CreateRunActions(Common.createContextActionHealTarget(Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D8), 2, Helpers.CreateContextValue(AbilityRankType.Default)))),
                                               Helpers.CreateSpellComponent(SpellSchool.Conjuration),
                                               Helpers.CreateContextRankConfig(max: 10),
                                               Common.createAbilitySpawnFx("e9399b6d57369ab4a9c3d88798d92f33", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                               Helpers.CreateSpellDescriptor(SpellDescriptor.Cure | SpellDescriptor.RestoreHP | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Air)
                                               );
            second_wind.setMiscAbilityParametersSelfOnly();
            second_wind.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Heighten;
            second_wind.AddToSpellList(Helpers.clericSpellList, 3);
            second_wind.AddToSpellList(Helpers.paladinSpellList, 3);
            second_wind.AddToSpellList(Helpers.inquisitorSpellList, 3);
            second_wind.AddToSpellList(Helpers.rangerSpellList, 3);
            Helpers.AddSpellAndScroll(second_wind, "08cf11d25aaab074388207b66f64a162"); //aid
        }


        static void createShadowClaws()
        {
            var claw1d4 = library.Get<BlueprintItemWeapon>("118fdd03e569a66459ab01a20af6811a");

            var effect = Helpers.CreateActionDealDamage(StatType.Strength, Helpers.CreateContextDiceValue(DiceType.Zero, 0, 1));
            var saved_effect = Helpers.CreateActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateConditionalSaved(null, effect));
            var buff = Helpers.CreateBuff("ShadowClawsBuff",
                                          "Shadow Claws",
                                          "You summon a pair of claws over your hands formed from semireal material. This grants you two primary claw attacks dealing 1d4 points of damage if you are Medium (1d3 if Small) plus 1 point of Strength damage. A successful Fortitude saving throw negates the Strength damage (DC = this spell’s DC).",
                                          "",
                                          LoadIcons.Image2Sprite.Create(@"AbilityIcons/PhantomLimbs.png"),
                                          null,
                                          Common.createEmptyHandWeaponOverride(claw1d4),
                                          Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(Helpers.CreateActionList(saved_effect), weapon_category: WeaponCategory.Claw)
                                          );

            shadow_claws = Helpers.CreateAbility("ShadowClawsAbility",
                                                 buff.Name,
                                                 buff.Description,
                                                 "",
                                                 buff.Icon,
                                                 AbilityType.Spell,
                                                 UnitCommand.CommandType.Standard,
                                                 AbilityRange.Personal,
                                                 Helpers.minutesPerLevelDuration,
                                                 "",
                                                 Helpers.CreateRunActions(Common.createContextActionApplySpellBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes))),
                                                 Helpers.CreateContextRankConfig(),
                                                 Helpers.CreateSpellComponent(SpellSchool.Illusion),
                                                 Common.createAbilitySpawnFx("790eb82d267bf0749943fba92b7953c2", anchor: AbilitySpawnFxAnchor.SelectedTarget)
                                                 );
            shadow_claws.setMiscAbilityParametersSelfOnly();
            shadow_claws.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent;
            shadow_claws.AddToSpellList(Helpers.wizardSpellList, 2);
            shadow_claws.AddToSpellList(Helpers.bardSpellList, 2);
            shadow_claws.AddToSpellList(Helpers.magusSpellList, 2);
            Helpers.AddSpellAndScroll(shadow_claws, "d1d24c5613bb8c14a9a089c54b77527d");
        }


        static void createArcaneConcordance()
        {
            var buff = Helpers.CreateBuff("ArcaneConcordanceBuff",
                                          "Arcane Concordance",
                                          "A shimmering, blue and gold radiance surrounds you, enhancing arcane spells cast by your allies within its area. Any arcane spell cast by a creature within the area gains a +1 enhancement bonus to the DC of any saving throws against the spell, and can be cast as if extend spell metamagic feat was applied to it (without increasing the spell level or casting time).",
                                          "",
                                          LoadIcons.Image2Sprite.Create(@"AbilityIcons/ArcaneConcordance.png"),
                                          null,
                                          Helpers.Create<SpellManipulationMechanics.IncreaseSpellTypeDC>(i => { i.apply_to_arcane = true; i.bonus = 1; }),
                                          Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicOnSpellType>(m => { m.Metamagic = Metamagic.Extend; m.apply_to_arcane = true; m.amount = 0; })
                                          );

            var aura_buff = Common.createBuffAreaEffect(buff, 13.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsAlly>()));

            aura_buff.SetBuffFlags(0);
            aura_buff.SetNameDescriptionIcon(buff);
            aura_buff.GetComponent<AddAreaEffect>().AreaEffect.Fx = Common.createPrefabLink("cda35ba5c34a61b499f5858eabcedec7"); //abjuration aoe

            arcane_concordance = Helpers.CreateAbility("ArcaneConcordanceAbility",
                                                       buff.Name,
                                                       buff.Description,
                                                       "",
                                                       buff.Icon,
                                                       AbilityType.Spell,
                                                       UnitCommand.CommandType.Standard,
                                                       AbilityRange.Personal,
                                                       Helpers.roundsPerLevelDuration,
                                                       "",
                                                       Helpers.CreateRunActions(Common.createContextActionApplyBuff(aura_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), is_from_spell: true)),
                                                       Helpers.CreateSpellComponent(SpellSchool.Evocation)
                                                       );
            arcane_concordance.setMiscAbilityParametersSelfOnly();
            arcane_concordance.AvailableMetamagic = Metamagic.Extend | Metamagic.Quicken | Metamagic.Heighten;
            arcane_concordance.AddToSpellList(Helpers.bardSpellList, 3);
            arcane_concordance.AddSpellAndScroll("08cf11d25aaab074388207b66f64a162");
        }



        static void createGhostbaneDirge()
        {
            var buff = Helpers.CreateBuff("GhostbaneDirgeBuff",
                                          "Ghostbane Dirge",
                                          "The target coalesces into a semi-physical form for a short period of time. While subject to the spell, the incorporeal creature takes half damage (50%) from nonmagical attack forms, and full damage from magic weapons, spells, spell-like effects, and supernatural effects.",
                                          "",
                                          LoadIcons.Image2Sprite.Create(@"AbilityIcons/GhostbaneDirge.png"),
                                          Common.createPrefabLink("3cf209e5299921349a1c159f35cfa369"), //faerie fire
                                          Helpers.Create<IncorporealMechanics.GhostbaneDirge>()
                                          );

            var apply_buff = Helpers.CreateActionSavingThrow(SavingThrowType.Will,
                                                             Helpers.CreateConditionalSaved(null,
                                                                                           Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), is_from_spell: true)
                                                                                           )
                                                            );

            ghostbane_dirge = Helpers.CreateAbility("GhostbaneDirgeAbility",
                                                    buff.Name,
                                                    buff.Description,
                                                    "",
                                                    buff.Icon,
                                                    AbilityType.Spell,
                                                    UnitCommand.CommandType.Standard,
                                                    AbilityRange.Close,
                                                    Helpers.roundsPerLevelDuration,
                                                    Helpers.willNegates,
                                                    Helpers.CreateRunActions(apply_buff),
                                                    Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                                    Common.createAbilityTargetHasFact(Common.incorporeal),
                                                    Helpers.CreateContextRankConfig()
                                                    );
            ghostbane_dirge.SpellResistance = true;
            ghostbane_dirge.setMiscAbilityParametersSingleTargetRangedHarmful();
            ghostbane_dirge.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent;
            ghostbane_dirge.AddToSpellList(Helpers.bardSpellList, 2);
            ghostbane_dirge.AddToSpellList(Helpers.clericSpellList, 2);
            ghostbane_dirge.AddToSpellList(Helpers.inquisitorSpellList, 2);
            ghostbane_dirge.AddToSpellList(Helpers.paladinSpellList, 1);

            ghostbane_dirge.AddSpellAndScroll("fa34c6a083cb8f345be3bc4911b733ff");
            ghostbane_dirge_mass = Helpers.CreateAbility("GhostbaneDirgeMassAbility",
                                        "Ghostbane Dirge, Mass",
                                        "This spell functions as ghostbane dirge, except that it affects multiple targets.\n" + ghostbane_dirge.Name + ": " + ghostbane_dirge.Description,
                                        "",
                                        buff.Icon,
                                        AbilityType.Spell,
                                        UnitCommand.CommandType.Standard,
                                        AbilityRange.Close,
                                        Helpers.roundsPerLevelDuration,
                                        Helpers.willNegates,
                                        Helpers.CreateRunActions(Helpers.CreateConditional(Common.createContextConditionHasFact(Common.incorporeal),
                                                                                           apply_buff)
                                                                ),
                                        Helpers.CreateContextRankConfig(),
                                        Helpers.CreateAbilityTargetsAround(15.Feet(), TargetType.Ally),
                                        Helpers.CreateSpellComponent(SpellSchool.Transmutation)
                                        );

            ghostbane_dirge_mass.SpellResistance = true;
            ghostbane_dirge_mass.setMiscAbilityParametersRangedDirectional();
            ghostbane_dirge_mass.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent;
            ghostbane_dirge_mass.AddToSpellList(Helpers.bardSpellList, 4);
            ghostbane_dirge_mass.AddToSpellList(Helpers.clericSpellList, 5);
            ghostbane_dirge_mass.AddToSpellList(Helpers.inquisitorSpellList, 5);
            ghostbane_dirge_mass.AddToSpellList(Helpers.paladinSpellList, 3);
            ghostbane_dirge_mass.AddSpellAndScroll("fa34c6a083cb8f345be3bc4911b733ff");
        }


        static void createPhantomLimbs()
        {
            var claw1d4 = library.Get<BlueprintItemWeapon>("118fdd03e569a66459ab01a20af6811a");
            var buff = Helpers.CreateBuff("PhantomLimbsBuff",
                                          "Phantom Limb",
                                          "The target grows two phantom arms, granting it two extra natural claw attacks; the target cannot use the phantom arms for any other purpose.",
                                          "",
                                          LoadIcons.Image2Sprite.Create(@"AbilityIcons/PhantomLimbs.png"),
                                          null,
                                          Common.createAddAdditionalLimb(claw1d4),
                                          Common.createAddAdditionalLimb(claw1d4)
                                          );


            phantom_limbs = Helpers.CreateAbility("PhantomLimbsAbility",
                                                  buff.Name,
                                                  buff.Description,
                                                  "",
                                                  buff.Icon,
                                                  AbilityType.Spell,
                                                  UnitCommand.CommandType.Standard,
                                                  AbilityRange.Touch,
                                                  Helpers.tenMinPerLevelDuration,
                                                  "",
                                                  Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.TenMinutes), is_from_spell: true)),
                                                  Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                  Common.createAbilitySpawnFx("cbfe312cb8e63e240a859efaad8e467c", anchor: AbilitySpawnFxAnchor.SelectedTarget, position_anchor: AbilitySpawnFxAnchor.None, orientation_anchor: AbilitySpawnFxAnchor.None),
                                                  Helpers.CreateContextRankConfig()
                                                  );
            phantom_limbs.setMiscAbilityParametersTouchFriendly();
            phantom_limbs.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach;
            Helpers.AddSpell(phantom_limbs);
        }


        static void createSpiritBoundBlade()
        {
            BlueprintWeaponEnchantment[] enchants = new BlueprintWeaponEnchantment[]
            {
                library.Get<BlueprintWeaponEnchantment>("a1455a289da208144981e4b1ef92cc56"), //vicious
                library.Get<BlueprintWeaponEnchantment>("102a9c8c9b7a75e4fb5844e79deaf4c0"), //keen
                WeaponEnchantments.cruel,
                WeaponEnchantments.menacing,
                WeaponEnchantments.heartseeker
            };

            UnityEngine.Sprite[] enchant_icons = new UnityEngine.Sprite[]
            {
                LoadIcons.Image2Sprite.Create(@"AbilityIcons/HWVicious.png"),
                library.Get<BlueprintActivatableAbility>("27d76f1afda08a64d897cc81201b5218").Icon, //keen
                LoadIcons.Image2Sprite.Create(@"AbilityIcons/HWCruel.png"),
                LoadIcons.Image2Sprite.Create(@"AbilityIcons/HWMenacing.png"),
                Helpers.GetIcon("49083bf0cdd00ec4dacbffb4be26e69a"), //keen light weapon buff
            };

            var ghost_touch = library.Get<BlueprintWeaponEnchantment>("47857e1a5a3ec1a46adf6491b1423b4f");

            var abilities = new List<BlueprintAbility>();
            var abilities_off_hand = new List<BlueprintAbility>();
            var abilities_primary_hand = new List<BlueprintAbility>();
            var buffs_off_hand = new List<BlueprintBuff>();
            var buffs_primary_hand = new List<BlueprintBuff>();
            var spell_name = "Spirit Bound Blade";
            var spell_description = "You focus emotional energy and weave it into a shroud of hardened ectoplasm around the weapon you touch, infusing it with a ghostly glow and great power. The weapon becomes a ghost touch weapon, and gains one of the following additional enchantments: vicious, keen, cruel, menacing or heartseeker.";

            for (int i = 0; i < enchants.Length; i++)
            {
                var ability1 = library.CopyAndAdd<BlueprintAbility>("831e942864e924846a30d2e0678e438b", enchants[i].name + "SpiritBoundBladeAbility", "");

                ability1.SetIcon(enchant_icons[i]);
                ability1.SetDescription(spell_description + "\n" + enchants[i].Description);
                ability1.SetName(spell_name + ": " + enchants[i].Name);
                ability1.setMiscAbilityParametersTouchFriendly();
                ability1.RemoveComponents<AbilityDeliverTouch>();
                var action_old = (ability1.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionEnchantWornItem);
                var action = Common.createItemEnchantmentsAction(enchants[i].name + "PrimaryHandSpiritBoundBladeBuff",
                                                                action_old.DurationValue,
                                                                new BlueprintWeaponEnchantment[] { enchants[i], ghost_touch },
                                                                true,
                                                                off_hand: false
                                                                );
                buffs_primary_hand.Add(action.Buff);

                ability1.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(action));
                ability1.ReplaceComponent<SpellComponent>(s => s.School = SpellSchool.Evocation);

                var ability2 = library.CopyAndAdd(ability1, enchants[i].name + "SpiritBoundBladeSecondaryHandAbility", "");
                ability2.SetName(spell_name + ": " + enchants[i].Name + " (Off-Hand)");
                var action2 = Common.createItemEnchantmentsAction(enchants[i].name + "SecondaryHandSpiritBoundBladeBuff",
                                                                action_old.DurationValue,
                                                                new BlueprintWeaponEnchantment[] { enchants[i], ghost_touch },
                                                                true,
                                                                off_hand: true
                                                                );
                buffs_off_hand.Add(action2.Buff);
                ability2.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(action2));
                ability2.AddComponent(Helpers.Create<NewMechanics.AbilitTargetManufacturedWeapon>(a => { a.off_hand = true; a.works_on_summoned = true; a.only_melee = true; }));
                ability1.AddComponent(Helpers.Create<NewMechanics.AbilitTargetManufacturedWeapon>(a => { a.works_on_summoned = true; a.only_melee = true; }));
                abilities.Add(ability1);
                abilities.Add(ability2);
                abilities_primary_hand.Add(ability1);
                abilities_off_hand.Add(ability2);
            }

            foreach (var a in abilities_off_hand)
            {
                a.ReplaceComponent<AbilityEffectRunAction>(ab => ab.Actions.Actions = new GameAction[] { Helpers.Create<NewMechanics.ContextActionRemoveBuffs>(c => c.Buffs = buffs_off_hand.ToArray()) }.AddToArray(ab.Actions.Actions));
            }
            foreach (var a in abilities_primary_hand)
            {
                a.ReplaceComponent<AbilityEffectRunAction>(ab => ab.Actions.Actions = new GameAction[] { Helpers.Create<NewMechanics.ContextActionRemoveBuffs>(c => c.Buffs = buffs_primary_hand.ToArray()) }.AddToArray(ab.Actions.Actions));
            }


            spirit_bound_blade = Common.createVariantWrapper("SpiritBoundBladeAbility", "", abilities.ToArray());
            spirit_bound_blade.SetNameDescriptionIcon(spell_name, spell_description, enchant_icons[0]);
            spirit_bound_blade.AddComponent(Helpers.CreateSpellComponent(SpellSchool.Evocation));

            Helpers.AddSpell(spirit_bound_blade);
        }


        static void createCloakOfWinds()
        {
            var buff = Helpers.CreateBuff("CloakOfWindsBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Helpers.Create<ACBonusAgainstAttacks>(a =>
                                          {
                                              a.ArmorClassBonus = 0;
                                              a.Value = 4;
                                              a.AgainstRangedOnly = true;
                                              a.Descriptor = ModifierDescriptor.UntypedStackable;
                                          }
                                          )
                                          );

            cloak_of_winds = Helpers.CreateAbility("CloakOfWindsAbility",
                                   "Cloak of Winds",
                                   "The subject is never checked or blown away by strong winds of windstorm or lesser strength (whether natural or magically created), and ranged attack rolls against the subject take a -4 penalty. ",
                                   "",
                                   Helpers.GetIcon("c28de1f98a3f432448e52e5d47c73208"), //protection from arrows
                                   AbilityType.Spell,
                                   UnitCommand.CommandType.Standard,
                                   AbilityRange.Touch,
                                   Helpers.minutesPerLevelDuration,
                                   "",
                                   Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), is_from_spell: true)),
                                   Helpers.CreateContextRankConfig(),
                                   Common.createAbilitySpawnFx("c30bef03751b7e844bc22646fb6927c4", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                   Helpers.CreateSpellComponent(SpellSchool.Abjuration),
                                   Helpers.CreateSpellDescriptor((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Air)
                                   );
            cloak_of_winds.setMiscAbilityParametersTouchFriendly();
            cloak_of_winds.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach | Metamagic.Extend;
            cloak_of_winds.AddToSpellList(Helpers.wizardSpellList, 3);
            cloak_of_winds.AddToSpellList(Helpers.druidSpellList, 3);
            cloak_of_winds.AddToSpellList(Helpers.rangerSpellList, 3);
            cloak_of_winds.AddToSpellList(Helpers.magusSpellList, 3);
            Helpers.AddSpellAndScroll(cloak_of_winds, "179d91b899fa6304b8c076e002890317");
        }


        static void createInvigorateAndInvigorateMass()
        {
            var fatigued = library.Get<BlueprintBuff>("e6f2fc5d73d88064583cb828801212f4");
            var exhaused = library.Get<BlueprintBuff>("46d1b9cc3d0fd36469a471b047d773a2");
            var buff = Helpers.CreateBuff("InvigorateBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Helpers.Create<BuffMechanics.SuppressBuffsCorrect>(s => { s.Buffs = new BlueprintBuff[] { fatigued, exhaused }; })
                                          );

            invigorate = Helpers.CreateAbility("InvigorateAbility",
                                               "Invigorate",
                                               "This spell banishes feelings of weariness. For the duration, the subject takes no penalties from the fatigued or exhausted conditions. The effect of invigorate is merely an illusion, however, not a substitute for actual rest or respite.",
                                               "",
                                               Helpers.GetIcon("4ebaf39efb8ffb64baf92784808dc49c"), //destruciton judgment
                                               AbilityType.Spell,
                                               UnitCommand.CommandType.Standard,
                                               AbilityRange.Touch,
                                               Helpers.tenMinPerLevelDuration,
                                               HarmlessSaves.HarmlessSaves.will_harmless,
                                               Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.TenMinutes), is_from_spell: true)),
                                               Helpers.CreateSpellComponent(SpellSchool.Illusion),
                                               Helpers.CreateContextRankConfig(),
                                               Common.createAbilitySpawnFx("790eb82d267bf0749943fba92b7953c2", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                               Helpers.Create<HarmlessSaves.HarmlessSpell>()
                                               );
            invigorate.setMiscAbilityParametersTouchFriendly();
            invigorate.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach | Metamagic.Extend;

            invigorate_mass = Helpers.CreateAbility("InvigorateMass Ability",
                                                   "Invigorate, Mass",
                                                   "This spell work as invigorate, except it affects multiple creatures.\n"
                                                   + invigorate.Name + ": " + invigorate.Description,
                                                   "",
                                                   Helpers.GetIcon("4ebaf39efb8ffb64baf92784808dc49c"), //destruciton judgment
                                                   AbilityType.Spell,
                                                   UnitCommand.CommandType.Standard,
                                                   AbilityRange.Personal,
                                                   Helpers.tenMinPerLevelDuration,
                                                   HarmlessSaves.HarmlessSaves.will_harmless,
                                                   Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.TenMinutes), is_from_spell: true)),
                                                   Helpers.CreateSpellComponent(SpellSchool.Illusion),
                                                   Common.createAbilitySpawnFx("790eb82d267bf0749943fba92b7953c2", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                   Helpers.Create<SharedSpells.CannotBeShared>(),
                                                   Helpers.CreateContextRankConfig(),
                                                    Helpers.CreateAbilityTargetsAround(15.Feet(), TargetType.Ally),
                                                    Helpers.Create<HarmlessSaves.HarmlessSpell>()
                                                   );
            invigorate_mass.setMiscAbilityParametersSelfOnly();
            invigorate_mass.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken | Metamagic.Extend;
            invigorate.AddToSpellList(Helpers.bardSpellList, 1);
            Helpers.AddSpellAndScroll(invigorate, "08cf11d25aaab074388207b66f64a162"); //aid
            invigorate_mass.AddToSpellList(Helpers.bardSpellList, 3);
            Helpers.AddSpellAndScroll(invigorate_mass, "08cf11d25aaab074388207b66f64a162"); //aid
        }  


        static void createDazeMass()
        {
            var serpentine_arcana = library.Get<BlueprintFeature>("02707231be1d3a74ba7e38a426c8df37");
            var daze = library.Get<BlueprintAbility>("55f14bc84d7c85446b07a1b5dd6b2b4c");
            var actions = daze.GetComponent<AbilityEffectRunAction>().Actions.Actions;

            var effect = Helpers.CreateConditional(Common.createContextConditionHasFacts(false, Common.aberration, Common.construct, Common.dragon, Common.fey, Common.outsider, Common.plant),
                                                   null,
                                                   Helpers.CreateConditional(new Condition[]{Common.createContextConditionHasFacts(false, Common.animal, Common.monstrous_humanoid, Common.magical_beast, Common.vermin),
                                                                                             Common.createContextConditionCasterHasFact(serpentine_arcana, false)
                                                                                            },
                                                                             null,
                                                                             Helpers.CreateActionSavingThrow(SavingThrowType.Will, actions)
                                                                            )
                                                   );

            daze_mass = Helpers.CreateAbility("DazeMassAbility",
                                              "Daze, Mass",
                                              "This spell functions as daze except that it can affect multiple targets without HD limit.\n"
                                              + daze.Name + ": " + daze.Description,
                                              "",
                                              daze.Icon,
                                              AbilityType.Spell,
                                              UnitCommand.CommandType.Standard,
                                              AbilityRange.Close,
                                              Helpers.oneRoundDuration,
                                              Helpers.willNegates,
                                              Helpers.CreateRunActions(effect),
                                              Helpers.CreateSpellComponent(SpellSchool.Enchantment),
                                              Helpers.CreateSpellDescriptor(SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting),
                                              Helpers.CreateAbilityTargetsAround(15.Feet(), TargetType.Enemy)
                                              );
            daze_mass.setMiscAbilityParametersRangedDirectional();
            daze_mass.AvailableMetamagic = Metamagic.Heighten | Metamagic.Extend | Metamagic.Quicken | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent;
            daze_mass.SpellResistance = true;

            daze_mass.AddToSpellList(Helpers.wizardSpellList, 4);
            daze_mass.AddToSpellList(Helpers.bardSpellList, 4);
            daze_mass.AddToSpellList(Helpers.inquisitorSpellList, 4);

            Helpers.AddScroll(daze_mass, "11987b82dbc808143963b9acaa81bf07");
        }


        static void createDebilitatingPortent()
        {
            var buff = Helpers.CreateBuff("DebilitatingPortentBuff",
                                          "Debilitating Portent",
                                          "The target is surrounded by a glowing green aura of ill fate. Each time the spell’s subject makes an attack or casts a spell, it must succeed at a Will saving throw with a DC = 10 + 1/2 caster level + best mental stat bonus. If it fails the saving throw, it deals half damage with the attack or spell.",
                                          "",
                                          Helpers.GetIcon("e6f2fc5d73d88064583cb828801212f4"),
                                          null,
                                          Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Compulsion),
                                          Helpers.Create<NewMechanics.SaveOrReduceDamage>(s => { s.save_type = SavingThrowType.Will; s.use_caster_level_and_stat = true; })
                                          );


            debilitating_portent = Helpers.CreateAbility("DebilitatingPortentAbility",
                                                         buff.Name,
                                                         buff.Description,
                                                         "",
                                                         buff.Icon,
                                                         AbilityType.Spell,
                                                         UnitCommand.CommandType.Standard,
                                                         AbilityRange.Medium,
                                                         Helpers.roundsPerLevelDuration,
                                                         "",
                                                         Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), is_from_spell: true)),
                                                         Common.createAbilitySpawnFx("c14a2f46018cb0e41bfeed61463510ff", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                         Helpers.CreateSpellComponent(SpellSchool.Enchantment),
                                                         Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Compulsion),
                                                         Helpers.CreateContextRankConfig()
                                                         );
            debilitating_portent.SpellResistance = true;
            debilitating_portent.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            debilitating_portent.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            debilitating_portent.AddToSpellList(Helpers.clericSpellList, 4);
            Helpers.AddSpellAndScroll(debilitating_portent, "e8f59c0e2bbbb514db0dfff42dbdde91");
        }


        static void createTelekinticStrikes()
        {
            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Holy, Helpers.CreateContextDiceValue(DiceType.D4, 1));
            dmg.DamageType.Type = DamageType.Force;
            var on_attack = Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(dmg), wait_for_attack_to_resolve: true);
            on_attack.AllNaturalAndUnarmed = true;
            var buff = Helpers.CreateBuff("TelekinticStrikesBuff",
                                          "Telekinetic Strikes",
                                          "The touched creature’s limbs are charged with telekinetic force. For the duration of the spell, the target’s unarmed attacks or natural weapons deal an additional 1d4 points of force damage on each successful unarmed melee attack.",
                                          "",
                                          Helpers.GetIcon("810992c76efdde84db707a0444cf9a1c"),
                                          null,
                                          on_attack,
                                          Helpers.CreateSpellDescriptor(SpellDescriptor.Force)
                                          );

            telekinetic_strikes = Helpers.CreateAbility("TelekineticStrikesAbility",
                                                        buff.Name,
                                                        buff.Description,
                                                        "",
                                                        buff.Icon,
                                                        AbilityType.Spell,
                                                        UnitCommand.CommandType.Standard,
                                                        AbilityRange.Touch,
                                                        Helpers.minutesPerLevelDuration,
                                                        "",
                                                        Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), is_from_spell: true)),
                                                        Common.createAbilitySpawnFx("52d413df527f9fa4a8cf5391fd593edd", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                        Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                        Helpers.CreateSpellDescriptor(SpellDescriptor.Force),
                                                        Helpers.CreateContextRankConfig()
                                                        );
            telekinetic_strikes.setMiscAbilityParametersTouchFriendly();
            telekinetic_strikes.AvailableMetamagic = Metamagic.Extend | Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Toppling;
            telekinetic_strikes.AddToSpellList(Helpers.wizardSpellList, 2);
            telekinetic_strikes.AddToSpellList(Helpers.magusSpellList, 2);
            Helpers.AddSpellAndScroll(telekinetic_strikes, "42d9445b9cdfac94385eaa2a3499b204");



        }


        static void createControlConstruct()
        {
            var dominate_person_buff = library.Get<BlueprintBuff>("c0f4e1c24c9cd334ca988ed1bd9d201f");
            var buff = Helpers.CreateBuff("ControlConstructBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          dominate_person_buff.FxOnStart,
                                          dominate_person_buff.GetComponent<ChangeFaction>(),
                                          Helpers.CreateAddFactContextActions(newRound: Helpers.Create<SkillMechanics.ContextActionCasterSkillCheck>(c =>
                                          {
                                              c.Failure = Helpers.CreateActionList(Helpers.Create<ContextActionRemoveSelf>());
                                              c.Stat = StatType.SkillKnowledgeArcana;
                                          })),
                                          Helpers.Create<UniqueBuff>()
                                          );

            control_construct = Helpers.CreateAbility("ControlConstructAbility",
                                                      "Control Construct",
                                                      "You wrest the control of a construct from its master. For as long as you concentrate, you can control the construct as if you were its master. You must make a Knowledge (Arcana) check each round to maintain control. The DC of the Knowledge (Arcana) check is (10 + the construct’s HD). You can not maintain control over more than one construct at a time.",
                                                      "",
                                                      dominate_person_buff.Icon,
                                                      AbilityType.Spell,
                                                      UnitCommand.CommandType.Standard,
                                                      AbilityRange.Close,
                                                      Helpers.roundsPerLevelDuration,
                                                      "",
                                                      Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), is_from_spell: true)),
                                                      Helpers.CreateContextRankConfig(),
                                                      Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                                      Common.createAbilityTargetHasFact(inverted: false, Common.construct)
                                                      );
            control_construct.AvailableMetamagic = Metamagic.Heighten | Metamagic.Extend | Metamagic.Quicken | Metamagic.Reach;
            control_construct.setMiscAbilityParametersSingleTargetRangedHarmful();
            control_construct.AddToSpellList(Helpers.wizardSpellList, 7);
            Helpers.AddSpellAndScroll(control_construct, "f199f6e5026488c499042900b572eb7f");
        }


        static void createChannelVigor()
        {
            var icon = Helpers.GetIcon("c3a8f31778c3980498d8f00c980be5f5"); //guidance
            var limbs_buff = library.Get<BlueprintBuff>("03464790f40c3c24aa684b57155f3280");
            var mind_buff = Helpers.CreateBuff("ChannelVigorMindBuff",
                                               "",
                                               "",
                                               "",
                                               null,
                                               limbs_buff.FxOnStart,
                                               Helpers.CreateAddStatBonus(StatType.SkillKnowledgeArcana, 4, ModifierDescriptor.Competence),
                                               Helpers.CreateAddStatBonus(StatType.SkillKnowledgeWorld, 4, ModifierDescriptor.Competence),
                                               Helpers.CreateAddStatBonus(StatType.SkillLoreNature, 4, ModifierDescriptor.Competence),
                                               Helpers.CreateAddStatBonus(StatType.SkillLoreReligion, 4, ModifierDescriptor.Competence),
                                               Helpers.CreateAddStatBonus(StatType.SkillPerception, 4, ModifierDescriptor.Competence),
                                               Common.createAttackTypeAttackBonus(4, AttackTypeAttackBonus.WeaponRangeType.Ranged, ModifierDescriptor.Competence)
                                               );
            var spirit_buff = Helpers.CreateBuff("ChannelVigorSpiritBuff",
                                               "",
                                               "",
                                               "",
                                               null,
                                               limbs_buff.FxOnStart,
                                               Helpers.CreateAddStatBonus(StatType.CheckIntimidate, 6, ModifierDescriptor.Competence),
                                               Helpers.CreateAddStatBonus(StatType.CheckBluff, 6, ModifierDescriptor.Competence),
                                               Helpers.CreateAddStatBonus(StatType.SaveWill, 6, ModifierDescriptor.Competence)
                                               );

            var torso_buff = Helpers.CreateBuff("ChannelVigorTorsoBuff",
                                   "",
                                   "",
                                   "",
                                   null,
                                   limbs_buff.FxOnStart,
                                   Helpers.CreateAddStatBonus(StatType.SaveFortitude, 6, ModifierDescriptor.Competence),
                                   Helpers.Create<ConcentrationBonus>(c => c.Value  = 6)
                                   );

            var description = "You focus the energy of your mind, body, and spirit into a specific part of your being, granting yourself an exceptional ability to perform certain tasks.When you cast the spell, choose one of the following portions of your self as your focus target. You can gain the benefit of only one channel vigor spell at a time.\n"
                              + "Limbs: You gain the benefits of a haste spell.\n"
                              + "Mind: You gain a +4 competence bonus on Knowledge and Perception skill checks and on ranged attack rolls.\n"
                              + "Spirit: You gain a +6 competence bonus on Will saving throws and Bluff and Intimidate checks.\n"
                              + "Torso: You gain a +6 competence bonus on Fortitude saving throws and concentration checks.\n";

            var display_name = "Channel Vigor";

            var name_buff_map = new Dictionary<string, BlueprintBuff>()
            {
                {"Limbs", limbs_buff },
                {"Mind", mind_buff },
                {"Spirit", spirit_buff },
                {"Torso", torso_buff }
            };

            var abilities = new List<BlueprintAbility>();

            foreach (var kv in name_buff_map)
            {
                var ability = Helpers.CreateAbility("ChannelVigor" + kv.Key + "Ability",
                                                    display_name + ": " + kv.Key,
                                                    description,
                                                    "",
                                                    icon,
                                                    AbilityType.Spell,
                                                    UnitCommand.CommandType.Standard,
                                                    AbilityRange.Personal,
                                                    Helpers.roundsPerLevelDuration,
                                                    "",
                                                    Helpers.CreateRunActions(Helpers.Create<NewMechanics.ContextActionRemoveBuffs>(c => c.Buffs = name_buff_map.Values.ToArray()),
                                                                             Common.createContextActionApplyBuff(kv.Value, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), is_from_spell: true)
                                                                             ),
                                                    Helpers.CreateContextRankConfig(),
                                                    Helpers.CreateSpellComponent(SpellSchool.Transmutation)
                                                    );
                ability.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten | Metamagic.Extend;
                ability.setMiscAbilityParametersSelfOnly();
                abilities.Add(ability);
            }

            channel_vigor = Common.createVariantWrapper("ChannelVigorBaseAbility", "", abilities.ToArray());
            channel_vigor.SetName(display_name);

            channel_vigor.AddComponent(Helpers.CreateSpellComponent(SpellSchool.Transmutation));
            channel_vigor.AddToSpellList(Helpers.alchemistSpellList, 3);
            channel_vigor.AddToSpellList(Helpers.clericSpellList, 3);
            channel_vigor.AddToSpellList(Helpers.inquisitorSpellList, 3);
            channel_vigor.AddToSpellList(Helpers.magusSpellList, 3);

            Helpers.AddSpellAndScroll(channel_vigor, "50f9e398017b34c43aa08a2c2c44e8af");
        }


        static void createBladeTutor()
        {
            var buff = Helpers.CreateBuff("BladeTutorBuff",
                                          "Blade Tutor",
                                          "You summon an insubstantial spirit of force that resembles a cloudy vapor hovering around your fists or any melee weapons you wield. The spirit compensates for your defensive or reckless melee attacks, nudging your weapons in the proper direction.\n"
                                          + "When you voluntarily use one or more actions or feats that apply penalties to attack rolls with your melee weapons (such as fighting defensively, or using the Power Attack feat), the spirit reduces the total penalty on affected attacks by 1 (to a minimum penalty of 0). The penalty is reduced by an additional 1 for every 5 caster levels you possess (to a minimum penalty of 0). Only penalties incurred by voluntary use of feats or maneuvers are reduced by this spell.",
                                          "",
                                          Helpers.GetIcon("bea9deffd3ab6734c9534153ddc70bde"),
                                          null,
                                          Helpers.Create<BladeTutor.BladeTutor>(b => b.value = Helpers.CreateContextValue(AbilityRankType.StatBonus)),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.CasterLevel, ContextRankProgression.OnePlusDivStep, AbilityRankType.StatBonus,
                                                                         stepLevel: 5)
                                         );

            blade_tutor = Helpers.CreateAbility("BladeTutorAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Spell,
                                                UnitCommand.CommandType.Standard,
                                                AbilityRange.Personal,
                                                Helpers.minutesPerLevelDuration,
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), is_from_spell: true)),
                                                Helpers.CreateSpellComponent(SpellSchool.Conjuration),
                                                Common.createAbilitySpawnFx("930c1a4aa129b8344a40c8c401d99a04", anchor: AbilitySpawnFxAnchor.SelectedTarget, position_anchor: AbilitySpawnFxAnchor.None, orientation_anchor: AbilitySpawnFxAnchor.None)
                                                );
            blade_tutor.setMiscAbilityParametersSelfOnly(Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.EnchantWeapon);
            blade_tutor.NeedEquipWeapons = true;

            blade_tutor.AvailableMetamagic = Metamagic.Heighten | Metamagic.Extend | Metamagic.Quicken;
            blade_tutor.AddToSpellList(Helpers.paladinSpellList, 2);
            blade_tutor.AddToSpellList(Helpers.magusSpellList, 1);
            blade_tutor.AddToSpellList(Helpers.wizardSpellList, 2);

            Helpers.AddSpellAndScroll(blade_tutor, "e40d940818ed94645829632007b2a871");//holy sword

        }


        static void fixPhantasmalSpells()
        {
            var phantasmal_web = library.Get<BlueprintAbility>("12fb4a4c22549c74d949e2916a2f0b6a");
            var phantasmal_putrefaction = library.Get<BlueprintAbility>("1f2e6019ece86d64baa5effa15e81ecc");
            var phantasmal_killer = library.Get<BlueprintAbility>("6717dbaef00c0eb4897a1c908a75dfe5");
            var weird = library.Get<BlueprintAbility>("870af83be6572594d84d276d7fc583e0");
            phantasmal_web.AddComponent(Helpers.Create<ShadowSpells.DisbeliefSpell>());
            phantasmal_putrefaction.AddComponent(Helpers.Create<ShadowSpells.DisbeliefSpell>());
            phantasmal_killer.AddComponent(Helpers.Create<ShadowSpells.DisbeliefSpell>());
            weird.AddComponent(Helpers.Create<ShadowSpells.DisbeliefSpell>());
        }

        static void createWrathfulWeapon()
        {
            BlueprintWeaponEnchantment[] enchants = new BlueprintWeaponEnchantment[]
            {
                library.Get<BlueprintWeaponEnchantment>("d05753b8df780fc4bb55b318f06af453"), //unholy
                library.Get<BlueprintWeaponEnchantment>("28a9964d81fedae44bae3ca45710c140"), //holy
                library.Get<BlueprintWeaponEnchantment>("57315bc1e1f62a741be0efde688087e9"), //chaotic
                library.Get<BlueprintWeaponEnchantment>("0ca43051edefcad4b9b2240aa36dc8d4") //axiomatic
            };

            UnityEngine.Sprite[] enchant_icons = new UnityEngine.Sprite[]
            {
                LoadIcons.Image2Sprite.Create(@"AbilityIcons/HWUnholy.png"),
                library.Get<BlueprintActivatableAbility>("ce0ece459ebed9941bb096f559f36fa8").Icon,//holy
                LoadIcons.Image2Sprite.Create(@"AbilityIcons/HWAnarchic.png"),
                library.Get<BlueprintActivatableAbility>("d76e8a80ab14ac942b6a9b8aaa5860b1").Icon,//axiomatic
            };

            SpellDescriptor[] descriptors = new SpellDescriptor[]
            {
                SpellDescriptor.Evil,
                SpellDescriptor.Good,
                SpellDescriptor.Chaos,
                SpellDescriptor.Law
            };

            var abilities = new List<BlueprintAbility>();
            var spell_name = "Wrathful Weapon";
            var spell_description = "You grant the targeted weapon one of the following weapon special abilities: anarchic, axiomatic, holy, or unholy. If anarchic, this spell has the chaos descriptor; if axiomatic, the law descriptor; if holy, the good descriptor; and if unholy, the evil descriptor. If the caster attempts to place a special ability on a weapon that already has that special ability, the spell fails.";

            for (int i = 0; i < 4; i++)
            {
                var ability1 = library.CopyAndAdd<BlueprintAbility>("831e942864e924846a30d2e0678e438b", enchants[i].name +"WrathfulWeaponAbility" , "");

                ability1.SetIcon(enchant_icons[i]);
                ability1.SetDescription(spell_description + "\n" + enchants[i].Description);
                ability1.SetName(spell_name + ": " + enchants[i].Name);
                ability1.setMiscAbilityParametersTouchFriendly();
                ability1.RemoveComponents<AbilityDeliverTouch>();
                var action_old = (ability1.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionEnchantWornItem);
                var action = Common.createItemEnchantmentAction(enchants[i].name + "PrimaryHandWrathfulWeaponBuff",
                                                                action_old.DurationValue,
                                                                enchants[i],
                                                                true,
                                                                off_hand: false
                                                                );
            
                ability1.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(action));
                ability1.AddComponent(Helpers.CreateSpellDescriptor(descriptors[i]));

                var ability2 = library.CopyAndAdd(ability1, enchants[i].name + "WrathfulWeaponSecondaryHandAbility", "");
                ability2.SetName(spell_name + ": " + enchants[i].Name + " (Off-Hand)");
                var action2 = Common.createItemEnchantmentAction(enchants[i].name + "SecondaryHandWrathfulWeaponBuff",
                                                                action_old.DurationValue,
                                                                enchants[i],
                                                                true,
                                                                off_hand: true
                                                                );
                ability2.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(action2));
                ability2.AddComponent(Helpers.Create<NewMechanics.AbilitTargetManufacturedWeapon>(a => { a.off_hand = true; a.works_on_summoned = true; }));
                ability1.AddComponent(Helpers.Create<NewMechanics.AbilitTargetManufacturedWeapon>(a => a.works_on_summoned = true));
                abilities.Add(ability1);
                abilities.Add(ability2);
            }

  

            wrathful_weapon = Common.createVariantWrapper("WrathfulWeaponAbility", "", abilities.ToArray());
            wrathful_weapon.SetNameDescriptionIcon(spell_name, spell_description, enchant_icons[1]);
            wrathful_weapon.AddComponent(Helpers.CreateSpellComponent(SpellSchool.Transmutation));
            wrathful_weapon.AddToSpellList(Helpers.clericSpellList, 4);

            wrathful_weapon.AddSpellAndScroll("fbdd06f0414c3ef458eb4b2a8072e502");
        }


        static void createShadowEnchantment()
        {
            shadow_enchantment = Helpers.CreateAbility("ShadowEnchantment",
                                                       "Shadow Enchantment",
                                                       "You use material from the Shadow Plane to cast a quasi-real, illusory version of a psychic, sorcerer, or wizard enchantment spell of 2nd level or lower. Spells that deal damage or have other effects work as normal unless the affected creature succeeds at a Will save. If the disbelieved enchantment spell has a damaging effect, that effect is one-fifth as strong (if applicable) or only 20% likely to occur. If recognized as a shadow enchantment, a damaging spell deals only one-fifth (20%) the normal amount of damage.\nIf the disbelieved attack has a special effect other than damage, that effect is one-fifth as strong (if applicable) or only 20% likely to occur. Regardless of the result of the save to disbelieve, an affected creature is also allowed any save (or spell resistance) that the spell being simulated allows, but the save DC is set according to shadow enchantment’s level (3rd) rather than the spell’s normal level. Objects, mindless creatures, and creatures immune to mind-affecting effects automatically succeed at their Will saves against this spell.",
                                                       "",
                                                       LoadIcons.Image2Sprite.Create(@"AbilityIcons/FontOfSpiritMagic.png"),
                                                       AbilityType.Spell,
                                                       UnitCommand.CommandType.Standard,
                                                       AbilityRange.Unlimited,
                                                       "See text",
                                                       "See text");
            shadow_enchantment.ComponentsArray = new BlueprintComponent[]
            {
                //Helpers.CreateAbilityVariants(shadow_enchantment),
                Helpers.CreateSpellComponent(SpellSchool.Illusion),
            };
            shadow_enchantment.AddToSpellList(Helpers.wizardSpellList, 3);
            shadow_enchantment.AddToSpellList(Helpers.bardSpellList, 3);
            Helpers.AddSpell(shadow_enchantment);

            shadow_enchantment_greater = Helpers.CreateAbility("ShadowEnchantmentGreater",
                                           "Shadow Enchantment, Greater",
                                           "This spell functions like shadow enchantment, except that it enables you to create partially real, illusory versions of psychic, sorcerer, or wizard enchantment spells of 5th level or lower. If the spell is recognized as a greater shadow enchantment, it’s only three-fifths (60%) as effective.",
                                           "",
                                           LoadIcons.Image2Sprite.Create(@"AbilityIcons/FontOfSpiritMagic.png"),
                                           AbilityType.Spell,
                                           UnitCommand.CommandType.Standard,
                                           AbilityRange.Unlimited,
                                           "See text",
                                           "See text");
            shadow_enchantment_greater.ComponentsArray = new BlueprintComponent[]
            {
                //Helpers.CreateAbilityVariants(shadow_enchantment_greater),
                Helpers.CreateSpellComponent(SpellSchool.Illusion),
            };
            shadow_enchantment_greater.AddToSpellList(Helpers.wizardSpellList, 6);
            shadow_enchantment_greater.AddToSpellList(Helpers.bardSpellList, 6);
            Helpers.AddSpell(shadow_enchantment_greater);
        }


        static void createShadowConjuration()
        {
            shadow_conjuration = Helpers.CreateAbility("ShadowConjuration",
                                                       "Shadow Conjuration",
                                                       "You use material from the Plane of Shadow to shape quasi-real illusions of one or more creatures, objects, or forces. Shadow conjuration can mimic any sorcerer or wizard conjuration (summoning) or conjuration (creation) spell of 3rd level or lower. Shadow conjurations are only one-fifth (20%) as strong as the real things, though creatures who believe the shadow conjurations to be real are affected by them at full strength. Any creature that interacts with the spell can make a Will save to recognize its true nature.\n"
                                                       + "Spells that deal damage have normal effects unless the affected creature succeeds on a Will save. Each disbelieving creature takes only one-fifth (20%) damage from the attack. If the disbelieved attack has a special effect other than damage, that effect is only 20% likely to occur. Regardless of the result of the save to disbelieve, an affected creature is also allowed any save that the spell being simulated allows, but the save DC is set according to shadow conjuration‘s level (4th) rather than the spell’s normal level. In addition, any effect created by shadow conjuration allows Spell Resistance, even if the spell it is simulating does not. Shadow objects or substances have normal effects except against those who disbelieve them. Against disbelievers, they are 20% likely to work.\n"
                                                       + "A shadow creature has one-fifth the hit points of a normal creature of its kind (regardless of whether it’s recognized as shadowy). It deals normal damage and has all normal abilities and weaknesses. Against a creature that recognizes it as a shadow creature, however, the shadow creature’s damage is one-fifth (20%) normal, and all special abilities that do not deal lethal damage are only 20% likely to work. (Roll for each use and each affected character separately.) Furthermore, the shadow creature’s AC bonuses are just one-fifth as large. A creature that succeeds on its save sees the shadow conjurations as transparent images superimposed on vague, shadowy forms. Objects automatically succeed on their Will saves against this spell.",
                                                       "",
                                                       LoadIcons.Image2Sprite.Create(@"AbilityIcons/StormOfSouls.png"),
                                                       AbilityType.Spell,
                                                       UnitCommand.CommandType.Standard,
                                                       AbilityRange.Unlimited,
                                                       "See text",
                                                       "See text");
            shadow_conjuration.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.CreateSpellComponent(SpellSchool.Illusion),
            };
            shadow_conjuration.AddToSpellList(Helpers.wizardSpellList, 4);
            shadow_conjuration.AddToSpellList(Helpers.bardSpellList, 4);
            Helpers.AddSpell(shadow_conjuration);

            shadow_conjuration_greater = Helpers.CreateAbility("ShadowConjurationGreater",
                                                       "Shadow Conjuration Greater",
                                                       "This spell functions like shadow conjuration, except that it duplicates any sorcerer or wizard conjuration (summoning) or conjuration (creation) spell of 6th level or lower. The illusory conjurations created deal three-fifths (60%) damage to nonbelievers, and non-damaging effects are 60% likely to work against nonbelievers.\n"
                                                       + $"{shadow_conjuration.Name}: {shadow_conjuration.Description}",
                                                       "",
                                                       LoadIcons.Image2Sprite.Create(@"AbilityIcons/StormOfSouls.png"),
                                                       AbilityType.Spell,
                                                       UnitCommand.CommandType.Standard,
                                                       AbilityRange.Unlimited,
                                                       "See text",
                                                       "See text");
            shadow_conjuration_greater.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.CreateSpellComponent(SpellSchool.Illusion),
            };
            shadow_conjuration_greater.AddToSpellList(Helpers.wizardSpellList, 7);
            Helpers.AddSpell(shadow_conjuration_greater);

            shades = Helpers.CreateAbility("Shades",
                                           "Shades",
                                           "This spell functions like shadow conjuration, except that it mimics conjuration spells of 8th level or lower. The illusory conjurations created deal four-fifths (80%) damage to nonbelievers, and non-damaging effects are 80% likely to work against nonbelievers.\n"
                                           + $"{shadow_conjuration.Name}: {shadow_conjuration.Description}",
                                           "",
                                           LoadIcons.Image2Sprite.Create(@"AbilityIcons/StormOfSouls.png"),
                                           AbilityType.Spell,
                                           UnitCommand.CommandType.Standard,
                                           AbilityRange.Unlimited,
                                           "See text",
                                           "See text");
            shades.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.CreateSpellComponent(SpellSchool.Illusion),
            };
            shades.AddToSpellList(Helpers.wizardSpellList, 9);
            Helpers.AddSpell(shades);


            Common.replaceDomainSpell(library.Get<BlueprintProgression>("1e1b4128290b11a41ba55280ede90d7d"), shadow_conjuration, 4); //instead of enervation for darkness domain
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("1e1b4128290b11a41ba55280ede90d7d"), shades, 9); //instead of polar midnight for darkness domain
        }


        static public void addShadowSpells(BlueprintAbility base_ability, SpellDescriptor descriptor, params BlueprintAbility[] spells)
        {
            base_ability.AddComponent(Helpers.CreateSpellDescriptor(descriptor));

            var ability_variants = base_ability.GetComponent<AbilityVariants>();
            if (ability_variants == null)
            {
                ability_variants = Helpers.CreateAbilityVariants(base_ability);

            }
            foreach (var s in spells)
            {
                var shadow_s = library.CopyAndAdd<BlueprintAbility>(s, base_ability.name + s.name, Helpers.MergeIds(base_ability.AssetGuid, s.AssetGuid));
                shadow_s.Parent = base_ability;
                shadow_s.RemoveComponents<SpellListComponent>();
                shadow_s.RemoveComponents<SpellComponent>();
                shadow_s.RemoveComponents<HarmlessSaves.HarmlessSpell>();
                shadow_s.RemoveComponents<HarmlessSaves.HarmlessHealSpell>();
                shadow_s.AddComponent(Helpers.CreateSpellComponent(SpellSchool.Illusion));
                Common.addSpellDescriptor(shadow_s, descriptor, false);
                shadow_s.SetNameDescription(base_ability.Name + " (" + s.Name + ")",
                                           base_ability.Description + "\n" + s.Description);

                var shadow_touch = shadow_s.GetComponent<AbilityEffectStickyTouch>();
                if (shadow_touch != null)
                {
                    var touch_s = shadow_touch.TouchDeliveryAbility;
                    var shadow_sticky_touch = library.CopyAndAdd<BlueprintAbility>(touch_s, base_ability.name + touch_s.name, Helpers.MergeIds(base_ability.AssetGuid, touch_s.AssetGuid));
                    shadow_sticky_touch.RemoveComponents<SpellListComponent>();
                    shadow_sticky_touch.RemoveComponents<SpellComponent>();
                    shadow_sticky_touch.AddComponent(Helpers.CreateSpellComponent(SpellSchool.Illusion));
                    shadow_sticky_touch.RemoveComponents<HarmlessSaves.HarmlessSpell>();
                    shadow_sticky_touch.RemoveComponents<HarmlessSaves.HarmlessHealSpell>();
                    Common.addSpellDescriptor(shadow_sticky_touch, descriptor);
                    shadow_s.ReplaceComponent<AbilityEffectStickyTouch>(a => a.TouchDeliveryAbility = shadow_sticky_touch);
                }
                ability_variants.Variants = ability_variants.Variants.AddToArray(shadow_s);
                base_ability.AvailableMetamagic = base_ability.AvailableMetamagic | shadow_s.AvailableMetamagic;
                shadow_s.ActionType = UnitCommand.CommandType.Standard;
                Common.unsetAsFullRoundAction(shadow_s);
            }
            base_ability.RemoveComponents<AbilityVariants>();
            base_ability.AddComponent(ability_variants);
        }


        static void addShadowSpells(BlueprintAbility base_ability, SpellDescriptor descriptor,
                                                 BlueprintSpellList[] spell_lists, int max_level, SpellSchool school, params BlueprintAbility[] except_spells)
        {
            var extracted_spells = new BlueprintAbility[0];
            foreach (var sl in spell_lists)
            {
                for (int i = 1; i <= max_level; i++)
                {
                    extracted_spells = extracted_spells.AddToArray(sl.GetSpells(i).Where(a => a.School == school));
                }
            }
            extracted_spells = extracted_spells.Distinct().ToArray();

            var spells = new List<BlueprintAbility>();
            foreach (var s in extracted_spells)
            {
                if (except_spells.Contains(s))
                {
                    continue;
                }
                if (s.HasVariants)
                {
                    spells.AddRange(s.Variants);
                }
                else
                {
                    spells.Add(s);
                }
            }

            addShadowSpells(base_ability, descriptor, spells.ToArray());

        }

        static public void fixShadowSpells()
        {
            var shadow_evocation = library.Get<BlueprintAbility>("237427308e48c3341b3d532b9d3a001f");
            shadow_evocation.SetDescription("You tap energy from the Plane of Shadow to cast a quasi-real, illusory version of any evocation spell of 4th level or lower. Spells that deal damage have normal effects unless an affected creature succeeds at a Will save. Each disbelieving creature takes only one-fifth damage from the attack. Regardless of the result of the save to disbelieve, an affected creature is also allowed any save (or spell resistance) that the spell being simulated allows, but the save DC is set according to shadow evocation's level (5th) rather than the spell's normal level.");

            var base_spells = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("2d81362af43aeac4387a3d4fced489c3"),//fireball
                library.Get<BlueprintAbility>("d2cff9243a7ee804cb6d5be47af30c73"),//lightning bolt
                library.Get<BlueprintAbility>("f09453607e683784c8fca646eec49162"),//shout
                library.Get<BlueprintAbility>("16ce660837fb2544e96c3b7eaad73c63"),//volcanic storm
                library.Get<BlueprintAbility>("fcb028205a71ee64d98175ff39a0abf9"),//ice storm
            };

            for (int i = 0; i < shadow_evocation.Variants.Length; i++)
            {
                shadow_evocation.Variants[i].ComponentsArray = base_spells[i].ComponentsArray.ToArray();
                shadow_evocation.Variants[i].RemoveComponents<SpellListComponent>();
                if (shadow_evocation.Variants[i].GetComponent<SpellDescriptorComponent>() == null)
                {
                    shadow_evocation.Variants[i].AddComponent(Helpers.CreateSpellDescriptor((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow20));
                }
                else
                {
                    shadow_evocation.Variants[i].ReplaceComponent<SpellDescriptorComponent>(sd => sd.Descriptor = sd.Descriptor | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow20);
                }
            }


            addShadowSpells(shadow_evocation, (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow20,
                           new BlueprintSpellList[] { Helpers.wizardSpellList }, 4, SpellSchool.Evocation,
                           base_spells.AddToArray(aggressive_thundercloud,
                                                  aggressive_thundercloud_greater
                                                 )
                           );

            

            var shadow_evocation_greater = library.Get<BlueprintAbility>("3c4a2d4181482e84d9cd752ef8edc3b6");
            shadow_evocation_greater.SetDescription("This spell functions like shadow evocation, except that it enables you to create a partially real, illusory version of any evocation spell of 7th level or lower. If recognized as a greater shadow evocation, a damaging spell deals only three-fifths (60%) damage.\nShadow Evocation: You tap energy from the Plane of Shadow to cast a quasi-real, illusory version of any evocation spell of 4th level or lower. Spells that deal damage have normal effects unless an affected creature succeeds at a Will save. Each disbelieving creature takes only one-fifth damage from the attack. Regardless of the result of the save to disbelieve, an affected creature is also allowed any save (or spell resistance) that the spell being simulated allows, but the save DC is set according to shadow evocation's level (5th) rather than the spell's normal level.");


            base_spells = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("645558d63604747428d55f0dd3a4cb58"),//chain lightning
                library.Get<BlueprintAbility>("5ef85d426783a5347b420546f91a677b"),//cold ice strike
                library.Get<BlueprintAbility>("5c8cde7f0dcec4e49bfa2632dfe2ecc0"),//ki shout
                library.Get<BlueprintAbility>("093ed1d67a539ad4c939d9d05cfe192c"),//sirocco
                library.Get<BlueprintAbility>("6303b404df12b0f4793fa0763b21dd2c"),//elemental assessor
                library.Get<BlueprintAbility>("8c29e953190cc67429dc9c701b16b7c2"),//caustic erruption
            };

            for (int i = 0; i < shadow_evocation_greater.Variants.Length; i++)
            {
                shadow_evocation_greater.Variants[i].ComponentsArray = base_spells[i].ComponentsArray.ToArray();
                shadow_evocation_greater.Variants[i].RemoveComponents<SpellListComponent>();
                if (shadow_evocation_greater.Variants[i].GetComponent<SpellDescriptorComponent>() == null)
                {
                    shadow_evocation_greater.Variants[i].AddComponent(Helpers.CreateSpellDescriptor((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow60));
                }
                else
                {
                    shadow_evocation_greater.Variants[i].ReplaceComponent<SpellDescriptorComponent>(sd => sd.Descriptor = sd.Descriptor | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow60);
                }
            }

            addShadowSpells(shadow_evocation_greater, (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow60,
               new BlueprintSpellList[] { Helpers.wizardSpellList }, 7, SpellSchool.Evocation,
                base_spells.AddToArray(aggressive_thundercloud,
                                        aggressive_thundercloud_greater,
                                        contingency
                                      )
               );


            addShadowSpells(shadow_enchantment, (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow20,
                           new BlueprintSpellList[] { Helpers.wizardSpellList, Helpers.bardSpellList, Psychic.psychic_class.Spellbook.SpellList }, 2, SpellSchool.Enchantment
                          );
            addShadowSpells(shadow_enchantment_greater, (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow60,
                           new BlueprintSpellList[] { Helpers.wizardSpellList, Helpers.bardSpellList, Psychic.psychic_class.Spellbook.SpellList }, 5, SpellSchool.Enchantment
                          );


            var conjuration_except_spells = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("4a648b57935a59547b7a2ee86fb4f26a"), //dimension door
                library.Get<BlueprintAbility>("15a04c40f84545949abeedef7279751a"), //joyful rapture
            };

            addShadowSpells(shadow_conjuration, (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow20,
                           new BlueprintSpellList[] { Helpers.wizardSpellList}, 3, SpellSchool.Conjuration, conjuration_except_spells
                          );
            addShadowSpells(shadow_conjuration_greater, (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow60,
                           new BlueprintSpellList[] { Helpers.wizardSpellList}, 6, SpellSchool.Conjuration, conjuration_except_spells
                          );
            addShadowSpells(shades, (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow80,
                               new BlueprintSpellList[] { Helpers.wizardSpellList }, 8, SpellSchool.Conjuration,
                               conjuration_except_spells
                              );
        }

        static void createSilence()
        {
            silence_buff = Helpers.CreateBuff("SilenceBuff",
                                          "Silence",
                                          "Upon the casting of this spell, complete silence prevails in the affected area. All sound is stopped: Conversation is impossible, spells with verbal components cannot be cast, and no noise whatsoever issues from, enters, or passes through the area. Creatures in an area of a silence spell are immune to sonic or language-based attacks, spells, and effects.",
                                          "",
                                          LoadIcons.Image2Sprite.Create(@"AbilityIcons/Silence.png"),
                                          Common.createPrefabLink("c4e5e6e8407f1774b97af4957364852c"),
                                          Helpers.Create<SpellFailureMechanics.Silence>(),
                                          Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Sonic | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.LanguageDependent),
                                          Helpers.Create<BuffMechanics.SuppressBuffsCorrect>(s =>  {s.Descriptor = SpellDescriptor.Sonic; }),
                                          Helpers.Create<BuffMechanics.SuppressBuffsCorrect>(s => s.Buffs = new BlueprintBuff[] { library.Get<BlueprintBuff>("cbfd2f5279f5946439fe82570fd61df2") }) //echolocation
                                          );
            silence_buff.Stacking = StackingType.Stack;
            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("7f57a1fabe15a3e4f96d1e12f838a476", "SilenceAreaEffect", "");
            area.Size = 20.Feet();
            area.Fx = Common.createPrefabLink("63f322580ec0e7c4c96fc62ecabad40f");
            area.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.CreateAreaEffectRunAction(unitEnter: Helpers.CreateActionSavingThrow(SavingThrowType.Will,
                                                                                             Helpers.CreateConditionalSaved(null,
                                                                                                                            Common.createContextActionApplyBuff(silence_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false)
                                                                                                                            )
                                                                                            ),
                                                  unitExit: Helpers.CreateConditional(Common.createContextConditionHasBuffFromCaster(silence_buff),
                                                                                      Common.createContextActionRemoveBuffFromCaster(silence_buff))
                                                                                      ),
            };

            silence = Helpers.CreateAbility("SilenceAbility",
                                            silence_buff.Name,
                                            silence_buff.Description,
                                            "",
                                            silence_buff.Icon,
                                            AbilityType.Spell,
                                            UnitCommand.CommandType.Standard,
                                            AbilityRange.Long,
                                            Helpers.roundsPerLevelDuration,
                                            Helpers.willNegates,
                                            Helpers.CreateRunActions(Common.createContextActionSpawnAreaEffect(area, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)))),
                                            Helpers.CreateSpellComponent(SpellSchool.Illusion),
                                            Common.createAbilityAoERadius(20.Feet(), TargetType.Any)
                                            );
            Common.setAsFullRoundAction(silence);
            silence.setMiscAbilityParametersRangedDirectional();
            silence.SpellResistance = true;
            silence.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;

            silence.AddToSpellList(Helpers.bardSpellList, 2);
            silence.AddToSpellList(Helpers.clericSpellList, 2);
            silence.AddToSpellList(Helpers.inquisitorSpellList, 2);

            Helpers.AddSpellAndScroll(silence, "1db86aaa479be6944abe90eaddb4afa2");//confusion

            var soothing_performance = library.Get<BlueprintAbility>("9b7fa6cadc0349449829873c63cc5b0b");
            Common.addSpellDescriptor(soothing_performance, SpellDescriptor.Sonic);


            var trollhound_howl = library.Get<BlueprintAbility>("78e79b09c2d724447a5c432e54ce4294");
            Common.addSpellDescriptor(trollhound_howl, SpellDescriptor.Sonic);


            var croak = library.Get<BlueprintAbility>("d7ab3a110325b174e90ae6c7b4e96bb9");
            Common.addSpellDescriptor(croak, SpellDescriptor.Sonic);

        }


        static void createSongOfDiscordGreater()
        {
            var buff = library.CopyAndAdd<BlueprintBuff>("2e1646c2449c88a4188e58043455a43a", "SongOfDiscordGreaterBuff", "");
            buff.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Compulsion | SpellDescriptor.Sonic),
                Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.AttackNearest),
                Helpers.CreateAddStatBonus(StatType.Strength, 4, ModifierDescriptor.Morale)
            };

            song_of_discord_greater = library.CopyAndAdd<BlueprintAbility>("d38aaf487e29c3d43a3bffa4a4a55f8f", "SongOfDiscordGreaterAbility", "");
            song_of_discord_greater.RemoveComponents<SpellListComponent>();
            song_of_discord_greater.RemoveComponents<AbilityEffectRunAction>();
            song_of_discord_greater.SetNameDescription("Song of Discord, Greater",
                                                       "This spell functions as song of discord except that affected creatures automatically attack the nearest target each round. In addition, all affected creatures gain a +4 morale bonus to Strength for the duration of the spell. A creature that succeeds at the Will save reduces the effect’s duration to 1 round.");
            song_of_discord_greater.AddComponent(Helpers.CreateRunActions(SavingThrowType.Will,
                                                                          Helpers.CreateConditionalSaved(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1), is_from_spell: true),
                                                                                                         Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), is_from_spell: true)
                                                                                                        )
                                                                         )
                                                );
            song_of_discord_greater.AddToSpellList(Helpers.bardSpellList, 6);
        }

        static void createBabble()
        {
            var nauseted = library.Get<BlueprintBuff>("956331dba5125ef48afe41875a00ca0e");
            var fascinated = library.Get<BlueprintBuff>("9c70d2ae017665b4b845e6c299cb7439");
            silence_buff.AddComponent(Helpers.Create<BuffMechanics.SuppressBuffsCorrect>(s => s.Buffs = new BlueprintBuff[] { fascinated }));
            var immune_to_fascinate = library.Get<BlueprintBuff>("a50373fa77d30d34c8c6efb198b36921");

            var apply_fascinate_immune = Common.createContextActionApplyBuff(immune_to_fascinate, Helpers.CreateContextDuration(1, DurationRate.Days), dispellable: false);
            var apply_fascinate = Common.createContextActionApplyBuff(fascinated, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true);

            var fascinate_area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("a4fc1c0798359974e99e1d790935501d", "BabbleFascinateArea", "");
            fascinate_area.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.CreateAreaEffectRunAction(unitEnter: new GameAction[]{ Helpers.CreateConditional(Common.createContextConditionHasFact(immune_to_fascinate), null,
                                                                                       Helpers.CreateActionSavingThrow(SavingThrowType.Will,
                                                                                                                       Helpers.CreateConditionalSaved(apply_fascinate_immune,
                                                                                                                                                      apply_fascinate)
                                                                                                                       )
                                                                                      )
                                                                             },
                                                   unitExit: new GameAction[]{Common.createContextActionRemoveBuffFromCaster(fascinated),
                                                                              Common.createContextActionRemoveBuffFromCaster(immune_to_fascinate)
                                                                             }
                                                  ),
                Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting)
            };

            var buff = library.CopyAndAdd<BlueprintBuff>("555930f121b364a4e82670b433028728", "BabbleBuff", "");
            buff.ComponentsArray = new BlueprintComponent[]
            {
                Common.createAddAreaEffect(fascinate_area),
                Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.Nauseated),
                Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Compulsion)
            };
            buff.SetNameDescriptionIcon("", "", null);
            buff.SetBuffFlags(0);
            buff.IsClassFeature = false;
            silence_buff.AddComponent(Helpers.Create<BuffMechanics.SuppressBuffsCorrect>(s => s.Buffs = new BlueprintBuff[] { buff }));

            babble = Helpers.CreateAbility("BabbleAbility",
                                           "Babble",
                                           "This spell causes the target to break into a fit of bizarre, uncontrollable babbling. The target also becomes nauseated. If the target succeeds at its save, the effects end. If not, the creature continues babbling and is nauseated for the entire duration.\n"
                                           + "Creatures within 30 feet of the subject that can hear the target’s babbling must succeed at a Will save or become fascinated for as long as the babbling persists. Once a creature’s fascination ends, it can’t become fascinated by the same instance of babble again as long as it does not leave spell area.",
                                           "",
                                           Helpers.GetIcon("886c7407dc629dc499b9f1465ff382df"), //confusion
                                           AbilityType.Spell,
                                           UnitCommand.CommandType.Standard,
                                           AbilityRange.Medium,
                                           Helpers.roundsPerLevelDuration,
                                           Helpers.willNegates,
                                           Helpers.CreateRunActions(SavingThrowType.Will,
                                                                    Helpers.CreateConditionalSaved(null,
                                                                                                   Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), is_from_spell: true))
                                                                   ),
                                           Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Compulsion),
                                           Helpers.CreateSpellComponent(SpellSchool.Enchantment),
                                           Common.createAbilityAoERadius(30.Feet(), TargetType.Any),
                                           Helpers.CreateContextRankConfig()
                                           );
           
            babble.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            babble.SpellResistance = true;
            babble.AvailableMetamagic = Metamagic.Quicken | Metamagic.Reach | Metamagic.Heighten | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            babble.AddToSpellList(Helpers.bardSpellList, 2);
            babble.AddToSpellList(Helpers.wizardSpellList, 3);
            Helpers.AddSpellAndScroll(babble, "1db86aaa479be6944abe90eaddb4afa2");

        }


        static void createSynapticPulse()
        {
            var icon = Helpers.GetIcon("df3950af5a783bd4d91ab73eb8fa0fd3");//stagger buff

            var stunned = library.Get<BlueprintBuff>("09d39b38bb7c6014394b6daced9bacd3");
            var sickened = library.Get<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323");
            var stun1 = Helpers.CreateConditionalSaved(null, Common.createContextActionApplyBuff(stunned, Helpers.CreateContextDuration(1), is_from_spell: true));
            var sickened1 = Common.createContextActionApplyBuff(sickened, Helpers.CreateContextDuration(1), is_from_spell: true);
            var stun1d4 = Helpers.CreateConditionalSaved(sickened1,
                                                         Common.createContextActionApplyBuff(stunned, Helpers.CreateContextDuration(0, diceType: DiceType.D4, diceCount: 1), is_from_spell: true)
                                                         );
            synaptic_pulse = Helpers.CreateAbility("SynapticPulseAbility",
                                                   "Synaptic Pulse",
                                                   "You emit a pulsating mental blast that stuns all creatures in range of your psychic shriek for 1 round.",
                                                   "",
                                                   icon,
                                                   AbilityType.Spell,
                                                   UnitCommand.CommandType.Standard,
                                                   AbilityRange.Personal,
                                                   Helpers.oneRoundDuration,
                                                   Helpers.willNegates,
                                                   Helpers.CreateRunActions(SavingThrowType.Will,
                                                                            Helpers.CreateConditional(Common.createContextConditionIsCaster(), null, stun1)
                                                                            ),
                                                   Helpers.CreateSpellComponent(SpellSchool.Enchantment),
                                                   Helpers.CreateSpellDescriptor(SpellDescriptor.Stun | SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting),
                                                   Helpers.Create<SharedSpells.CannotBeShared>(),
                                                   Helpers.CreateAbilityTargetsAround(30.Feet(), TargetType.Any),
                                                   Common.createAbilitySpawnFxDestroyOnCast("9012980c54f45ee428aa717c69446820", anchor: AbilitySpawnFxAnchor.Caster,  position_anchor: AbilitySpawnFxAnchor.None, orientation_anchor: AbilitySpawnFxAnchor.None)                                                
                                                   );
            synaptic_pulse.SpellResistance = true;
            synaptic_pulse.setMiscAbilityParametersSelfOnly();
            synaptic_pulse.AvailableMetamagic = Metamagic.Quicken | Metamagic.Extend | Metamagic.Heighten | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing | (Metamagic)MetamagicFeats.MetamagicExtender.Selective;
            Helpers.AddSpell(synaptic_pulse);


            synaptic_pulse_greater = Helpers.CreateAbility("SynapticPulseGreaterAbility",
                                       "Synaptic Pulse, Greater",
                                       "You emit a pulsating mental blast that stuns all creatures in range of your psychic shriek for 1d4 rounds. On a successful save, a creature is instead sickened for 1 round.",
                                       "",
                                       icon,
                                       AbilityType.Spell,
                                       UnitCommand.CommandType.Standard,
                                       AbilityRange.Personal,
                                       "1d4 rounds",
                                       Helpers.willNegates,
                                       Helpers.CreateRunActions(SavingThrowType.Will,
                                                                Helpers.CreateConditional(Common.createContextConditionIsCaster(), null, stun1d4)
                                                                ),
                                       Helpers.CreateSpellComponent(SpellSchool.Enchantment),
                                       Helpers.CreateSpellDescriptor(SpellDescriptor.Stun | SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting),
                                       Helpers.Create<SharedSpells.CannotBeShared>(),
                                       Helpers.CreateAbilityTargetsAround(30.Feet(), TargetType.Any),
                                       Common.createAbilitySpawnFxDestroyOnCast("9012980c54f45ee428aa717c69446820", anchor: AbilitySpawnFxAnchor.Caster, position_anchor: AbilitySpawnFxAnchor.None, orientation_anchor: AbilitySpawnFxAnchor.None)
                                       );
            synaptic_pulse_greater.AvailableMetamagic = synaptic_pulse.AvailableMetamagic;
            synaptic_pulse_greater.SpellResistance = true;
            synaptic_pulse_greater.setMiscAbilityParametersSelfOnly();
            Helpers.AddSpell(synaptic_pulse_greater);
        }


        static void createAkashicForm()
        {
            var buff = Helpers.CreateBuff("AkashicFormBuff",
                                           "Akashic Form",
                                           "If at any point within the duration of the spell you are reduced to fewer than 0 hit points or are slain by a death effect that is not mind-affecting, you can immediately let your current physical body die and assume the record of your physical body on your next turn.",
                                           "",
                                           Helpers.GetIcon("fafd77c6bfa85c04ba31fdc1c962c914"),
                                           null
                                           );

            var buff_resurrect = Helpers.CreateBuff("AkashicFormHealBuff",
                                "Akashic Form Resurrect",
                                "",
                                "0a3800d5e3d442bdaa0a4c81cbec875f",
                                Helpers.GetIcon("fafd77c6bfa85c04ba31fdc1c962c914"),
                                null,
                                Helpers.CreateAddFactContextActions(deactivated: new GameAction[]
                                {
                                    Helpers.Create<ContextActionResurrect>(c => c.FullRestore = true),
                                    Common.createContextActionRemoveBuffFromCaster(buff),

                                }
                                )
                                );
            buff_resurrect.SetBuffFlags(BuffFlags.StayOnDeath);

            buff.AddComponent(Common.createDeathActions(Helpers.CreateActionList(Common.createContextActionApplyBuff(buff_resurrect, Helpers.CreateContextDuration(0), is_child: true, dispellable: false)
                                                                                )
                                                       ));



            buff.SetBuffFlags(BuffFlags.RemoveOnRest | BuffFlags.StayOnDeath);

            akashic_form = Helpers.CreateAbility("AkashicFormAbility",
                                                 buff.Name,
                                                 buff.Description,
                                                 "",
                                                 buff.Icon,
                                                 AbilityType.Spell,
                                                 UnitCommand.CommandType.Standard,
                                                 AbilityRange.Personal,
                                                 "24 hours",
                                                 "",
                                                 Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1, DurationRate.Days), is_from_spell: true)),
                                                 Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                 Common.createAbilitySpawnFx("e93261ee4c3ea474e923f6a645a3384f", anchor: AbilitySpawnFxAnchor.ClickedTarget, position_anchor: AbilitySpawnFxAnchor.None, orientation_anchor: AbilitySpawnFxAnchor.None)
                                                 );
            akashic_form.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken | Metamagic.Extend;
            akashic_form.setMiscAbilityParametersSelfOnly();
            Common.setAsFullRoundAction(akashic_form);
            Helpers.AddSpell(akashic_form);
        }


        static void createTelekineticStorm()
        {
            var dazed = Common.dazed_non_mind_affecting;
            var stunned = library.Get<BlueprintBuff>("09d39b38bb7c6014394b6daced9bacd3");

            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Holy, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.Default)), isAoE: true, halfIfSaved: true);
            dmg.DamageType.Type = DamageType.Force;

            var effect = Helpers.CreateConditionalSaved(null,
                                                        new GameAction[] {Common.createContextActionApplyBuff(dazed, Helpers.CreateContextDuration(1, DurationRate.Rounds), is_from_spell: true),
                                                                          Common.createContextActionApplyBuff(stunned, Helpers.CreateContextDuration(1, DurationRate.Rounds), is_from_spell: true)
                                                                         }
                                                        );

            telekinetic_storm = Helpers.CreateAbility("TelekineticStorm",
                                                      "Telekinetic Storm",
                                                      $"You generate a storm of telekinetic energy that emanates from you, ripping through the spell’s area of effect with devastating force. Any creature caught in the spell’s radius takes 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage per caster level (maximum 20d{BalanceFixes.getDamageDieString(DiceType.D6)}) and is dazed and stunned for 1 round. A successful Fortitude save reduces the damage by half and negates the dazed and stunned effects.\n"
                                                      + "You can designate any number of creatures to be immune to the spell’s effect, though you must be capable of targeting those creatures.",
                                                      "",
                                                      burst_of_force.Icon,
                                                      AbilityType.Spell,
                                                      UnitCommand.CommandType.Standard,
                                                      AbilityRange.Personal,
                                                      "",
                                                      "Fortitude partial",
                                                      Helpers.CreateRunActions(SavingThrowType.Fortitude, dmg, effect),
                                                      Helpers.CreateSpellDescriptor(SpellDescriptor.Force),
                                                      Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                      Helpers.CreateAbilityTargetsAround(40.Feet(), TargetType.Enemy, spreadSpeed: 28.Feet()),
                                                      Common.createAbilitySpawnFx("5c60b61bfa134fa47a4628f2ae880e05",
                                                                                       orientation_anchor: AbilitySpawnFxAnchor.None,
                                                                                       position_anchor: AbilitySpawnFxAnchor.None
                                                                                       ),
                                                      Helpers.Create<SharedSpells.CannotBeShared>(),
                                                      Helpers.CreateContextRankConfig(max: 20, feature: MetamagicFeats.intensified_metamagic)
                                                      );
            telekinetic_storm.setMiscAbilityParametersSelfOnly();
            telekinetic_storm.SpellResistance = true;
            telekinetic_storm.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | (Metamagic)MetamagicFeats.MetamagicExtender.IntensifiedGeneral | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing | (Metamagic)MetamagicFeats.MetamagicExtender.Toppling;
            Helpers.AddSpell(telekinetic_storm);
        }

        static void createDivideMind()
        {
            var buff = Helpers.CreateBuff("DivideMindBuff",
                                          "Divide Mind",
                                          "You partition your mind to maximize your mental power. Until the spell ends, you roll twice and use the higher result for all Will saves, Intelligence checks, and Intelligence-based skill checks. In addition, as a swift action you can have your second mind perform any purely mental action that normally requires a standard action or a move action. This includes casting psychic spells, using spell-like abilities, and concentrating on spells.\n"
                                          + "Spells and spell-like abilities cast or used by your secondary mind this way can’t exceed 5th level.",
                                          "",
                                          Helpers.GetIcon("fb96d35da88acb1498dc51a934f6c4d5"),
                                          Common.createPrefabLink("c388856d0e8855f429a83ccba67944ba"),
                                          Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicUpToSpellLevel>(m => { m.max_level = 5; m.Metamagic = Metamagic.Quicken; m.except_fullround_action = true; }),
                                          Helpers.Create<ModifyD20>(m =>
                                          {
                                              m.Rule = RuleType.SavingThrow;
                                              m.TakeBest = true;
                                              m.RollsAmount = 1;
                                              Helpers.SetField(m, "m_SavingThrowType", 4);
                                          }),
                                          Helpers.Create<ModifyD20>(m =>
                                          {
                                              m.Rule = RuleType.SkillCheck;
                                              m.TakeBest = true;
                                              m.SpecificSkill = true;
                                              m.RollsAmount = 1;
                                              m.Skill = new StatType[] { StatType.Intelligence, StatType.SkillKnowledgeArcana, StatType.SkillKnowledgeWorld };
                                          })
                                          );

            divide_mind = Helpers.CreateAbility("DivindMindAbility",
                                               buff.Name,
                                               buff.Description,
                                               "",
                                               buff.Icon,
                                               AbilityType.Spell,
                                               UnitCommand.CommandType.Standard,
                                               AbilityRange.Personal,
                                               Helpers.oneMinuteDuration,
                                               "",
                                               Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), is_from_spell: true)),
                                               Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting),
                                               Helpers.CreateSpellComponent(SpellSchool.Enchantment)
                                               );
            divide_mind.AvailableMetamagic = Metamagic.Quicken | Metamagic.Extend | Metamagic.Heighten;
            divide_mind.setMiscAbilityParametersSelfOnly();
            Helpers.AddSpell(divide_mind);
        }

        static void createPsychicSurgery()
        {
            var heal_stat_damage = Helpers.Create<ContextActionHealStatDamage>(h =>
            {
                h.HealDrain = true;
                Helpers.SetField(h, "m_HealType", 2);
                Helpers.SetField(h, "m_StatClass", 2);
            });

            psychic_surgery = Helpers.CreateAbility("PsychicSurgeryAbility",
                                                    "Psychic Surgery",
                                                    "Psychic surgery cures the target of all Intelligence, Wisdom, and Charisma damage and restores all points permanently drained from the target’s Intelligence, Wisdom, and Charisma scores. It also eliminates all ongoing insanity, confusion, and fear effects. Psychic surgery can also remove other mental afflictions, including enchantment spells and abilities, and even instantaneous effects, but in this case, if dispel magic couldn’t remove the effect, psychic surgery works only if the spell level or equivalent spell level of the effect was 6th level or lower.",
                                                    "",
                                                    Helpers.GetIcon("03a9630394d10164a9410882d31572f0"),
                                                    AbilityType.Spell,
                                                    UnitCommand.CommandType.Standard,
                                                    AbilityRange.Touch,
                                                    "",
                                                    "",
                                                    Helpers.CreateRunActions(heal_stat_damage,
                                                                             Common.createContextActionDispelMagic(SpellDescriptor.MindAffecting, new SpellSchool[0],
                                                                                                                   Kingmaker.RuleSystem.Rules.RuleDispelMagic.CheckType.None)
                                                                                                                   ),
                                                    Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting),
                                                    Helpers.CreateSpellComponent(SpellSchool.Enchantment),
                                                    Common.createAbilitySpawnFx("319f507857aeb58499713b105df2bf29", anchor: AbilitySpawnFxAnchor.SelectedTarget, position_anchor: AbilitySpawnFxAnchor.None, orientation_anchor: AbilitySpawnFxAnchor.None)
                                                  );
            psychic_surgery.setMiscAbilityParametersTouchFriendly();
            psychic_surgery.AvailableMetamagic = Metamagic.Quicken | Metamagic.Reach | Metamagic.Heighten;
            psychic_surgery.MaterialComponent = library.Get<BlueprintAbility>("c66e86905f7606c4eaa5c774f0357b2b").MaterialComponent; //stoneskin
            Helpers.AddSpell(psychic_surgery);
        }


        static void createOrbOfTheVoid()
        {

            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/Metamixing.png");

            var caster_dmg_done = Helpers.CreateBuff("OrbOfTheVoidCasterDamageDoneBuff",
                                                      "Orb of the Void (Affected This Turn)",
                                                      "",
                                                      "",
                                                      icon,
                                                      null
                                                      );

            var orb_fx_buff = library.CopyAndAdd<BlueprintBuff>("1eb1c1aab1b319c43b06b40c4800f16c", "OrbOfTheVoidFxBuff", "");
            orb_fx_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("3659ce23ae102ca47a7bf3a30dd98609", "OrbOfTheVoidArea", "");
            area.Size = 2.Feet();
            area.Fx = Common.createPrefabLink("af6fac67b4ed1b04dae392e92cb228cd"); //will o wisp fx

            var dmg = Helpers.CreateActionEnergyDrain(Helpers.CreateContextDiceValue(DiceType.Zero, 0, 1), Helpers.CreateContextDuration(1, DurationRate.Days), Kingmaker.RuleSystem.Rules.EnergyDrainType.Temporary, IgnoreCritical: true);
            var undead_buff = library.CopyAndAdd<BlueprintBuff>("b2c17e0e1f08d6549b47e7f7cfe4986d", "OrbOfTheVoidUndeadBuff", "");
            undead_buff.ReplaceComponent<TemporaryHitPointsFromAbilityValue>(t => { t.Value = Helpers.CreateContextValue(AbilitySharedValue.Damage); t.RemoveWhenHitPointsEnd = true; });
            undead_buff.AddComponent(Helpers.CreateCalculateSharedValue(Helpers.CreateContextDiceValue(DiceType.D4, 10, 0), AbilitySharedValue.Damage));
            undead_buff.Stacking = StackingType.Replace;
            var effect = Helpers.CreateConditional(Helpers.Create<UndeadMechanics.ContextConditionHasNegativeEnergyAffinity>(),
                                                   Common.createContextActionApplyBuff(undead_buff, Helpers.CreateContextDuration(1, DurationRate.Hours), is_from_spell: true),
                                                   Helpers.CreateActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateConditionalSaved(null, dmg))
                                                   );
            effect = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(caster_dmg_done), new GameAction[0],
                                               new GameAction[] { effect, Common.createContextActionApplyBuffToCaster(caster_dmg_done, Helpers.CreateContextDuration(1), dispellable: false, duration_seconds: 6) }
                                               );
            var dmg_action = Helpers.CreateActionList(effect);
            area.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.Create<NewMechanics.AbilityAreaEffectRunActionWithFirstRound>(a => {a.FirstRound = dmg_action;
                                                                                            a.Round = dmg_action;
                                                                                            a.UnitEnter = dmg_action;
                                                                                            }
                                                                                      ),
                Helpers.CreateSpellDescriptor(SpellDescriptor.Electricity),
                Helpers.Create<AbilityAreaEffectBuff>(a => {a.Buff = orb_fx_buff; a.Condition = Helpers.CreateConditionsCheckerOr(); })
            };
            area.SpellResistance = true;

            var caster_buff = Helpers.CreateBuff("OrbOfTheVoidCasterBuff",
                                                  "Orb of the Void",
                                                  "You create a small weightless sphere of pure negative energy. As a move action, you can move it up to any place within close range. Any creature passing through or ending its turn in the space occupied by the sphere gains one negative level (Fortitude negates). Twenty-four hours after gaining a negative level from the sphere, the subject must make a Fortitude saving throw (the DC of this save is equal to the DC of this spell) for each negative level. If the save succeeds, that negative level is removed. If it fails, that negative level becomes permanent.\n"
                                                  + "An undead creature that passes through or ends its turn in the space occupied by the orb gains 2d4 × 5 temporary hit points for 1 hour.",
                                                  "",
                                                  icon,
                                                  null
                                                  );
            caster_buff.AddComponent(Helpers.CreateAddFactContextActions(deactivated: Helpers.Create<NewMechanics.RemoveUniqueArea>(a => a.feature = caster_buff)));
            area.AddComponent(Helpers.Create<UniqueAreaEffect>(u => u.Feature = caster_buff));

            var spawn_area = Common.createContextActionSpawnAreaEffect(area, Helpers.CreateContextDuration(100));


            var move_ability = Helpers.CreateAbility("OrbOfTheVoidMoveAbility",
                                                     "Orb of the Void",
                                                     "Use this ability to move orb of the void.",
                                                     "",
                                                     icon,
                                                     AbilityType.SpellLike,
                                                     UnitCommand.CommandType.Move,
                                                     AbilityRange.Close,
                                                     "",
                                                     "",
                                                     Helpers.CreateRunActions(spawn_area),
                                                     Common.createAbilityAoERadius(2.Feet(), TargetType.Any),
                                                     Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                     Common.createAbilityCasterHasNoFacts(caster_dmg_done)
                                                     );

            move_ability.setMiscAbilityParametersRangedDirectional();
            move_ability.AvailableMetamagic =  Metamagic.Heighten | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;

            caster_buff.AddComponent(Helpers.CreateAddFact(move_ability));
            caster_buff.AddComponent(Helpers.Create<ReplaceAbilityParamsWithContext>(r => r.Ability = move_ability));


            var apply_caster_buff = Common.createContextActionApplyBuffToCaster(caster_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)));


            orb_of_the_void = Helpers.CreateAbility("OrbOfTheVoidAbility",
                                                             caster_buff.Name,
                                                             caster_buff.Description,
                                                             "",
                                                             icon,
                                                             AbilityType.Spell,
                                                             UnitCommand.CommandType.Standard,
                                                             AbilityRange.Close,
                                                             Helpers.roundsPerLevelDuration,
                                                             "Fortitude Negates",
                                                             Helpers.CreateRunActions(spawn_area),
                                                             Common.createAbilityAoERadius(2.Feet(), TargetType.Any),
                                                             Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                             Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(apply_caster_buff))
                                                           );
            orb_of_the_void.MaterialComponent = library.Get<BlueprintAbility>("7c5d556b9a5883048bf030e20daebe31").MaterialComponent.CloneObject(); //diamond dust
            orb_of_the_void.MaterialComponent.Count = 2;
            orb_of_the_void.setMiscAbilityParametersRangedDirectional();
            orb_of_the_void.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach |  (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            orb_of_the_void.SpellResistance = true;

            orb_of_the_void.AddToSpellList(Helpers.clericSpellList, 8);
            orb_of_the_void.AddToSpellList(Helpers.wizardSpellList, 8);
            orb_of_the_void.AddSpellAndScroll("7b6600001c048ef4dab56af3fd8cd82e"); //energy drain
        }


        static void createIronBody()
        {
            var icon = library.Get<BlueprintAbility>("65e8d23aef5e7784dbeb27b1fca40931").Icon; //ice prison

            var weapon = library.Get<BlueprintItemWeapon>("f60c5a820b69fb243a4cce5d1d07d06e"); //unarmed 1d6

            var buff = Helpers.CreateBuff("IronBodyBuff",
                                          "Iron Body",
                                          "This spell transforms your body into living iron, which grants you several powerful resistances and abilities. You gain damage reduction 15/adamantine. You are immune to blindness, critical hits, ability score damage, deafness, disease, drowning, electricity, poison, stunning, and all spells or attacks that affect your physiology or respiration, because you have no physiology or respiration while this spell is in effect. You take only half damage from acid and fire. However, you also become vulnerable to all special attacks that affect iron golems.\n"
                                          + "You gain a +6 enhancement bonus to your Strength score, but you take a -6 penalty to Dexterity as well (to a minimum Dexterity score of 1), and your speed is reduced to half normal. You have an arcane spell failure chance of 35% and a -6 armor check penalty, just as if you were clad in full plate armor.",
                                          "",
                                          Helpers.GetIcon("c66e86905f7606c4eaa5c774f0357b2b"), //stoneskin
                                          Common.createPrefabLink("eec87237ed7b61149a952f56da85bbb1"), //stone skin
                                          Common.createAddEnergyDamageImmunity(DamageEnergyType.Electricity),
                                          Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Blindness | SpellDescriptor.Disease | SpellDescriptor.Stun | SpellDescriptor.Poison | SpellDescriptor.Death | SpellDescriptor.Death),
                                          Common.createBuffDescriptorImmunity(SpellDescriptor.Electricity | SpellDescriptor.Cold | SpellDescriptor.Blindness | SpellDescriptor.Disease | SpellDescriptor.Death | SpellDescriptor.Stun | SpellDescriptor.Poison),
                                          Common.createMaterialDR(15, PhysicalDamageMaterial.Adamantite),
                                          Helpers.Create<AddImmunityToAbilityScoreDamage>(),
                                          Helpers.Create<AddImmunityToCriticalHits>(),
                                          Helpers.CreateAddStatBonus(StatType.Strength, 6, ModifierDescriptor.Enhancement),
                                          Helpers.CreateAddStatBonus(StatType.Dexterity, -6, ModifierDescriptor.UntypedStackable),
                                          Common.createEmptyHandWeaponOverride(weapon),
                                          Common.createSpecificBuffImmunity(suffocation_buff),
                                          Common.createSpecificBuffImmunity(fast_suffocation_buff),
                                          Common.createSpecificBuffImmunity(Common.deafened),
                                          Common.createAddEnergyDamageDurability(DamageEnergyType.Acid, 0.5f),
                                          Common.createAddEnergyDamageDurability(DamageEnergyType.Fire, 0.5f),
                                          Helpers.Create<ArcaneSpellFailureIncrease>(a => a.Bonus = 35),
                                          Helpers.Create<NewMechanics.ArmorCheckPenaltyIncrease>(a => a.Bonus = -6),
                                          Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.Slowed)
                                          );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), is_from_spell: true);

            iron_body = Helpers.CreateAbility("IronBodyAbility",
                                             buff.Name,
                                             buff.Description,
                                             "",
                                             buff.Icon,
                                             AbilityType.Spell,
                                             UnitCommand.CommandType.Standard,
                                             AbilityRange.Personal,
                                             Helpers.minutesPerLevelDuration,
                                             "",
                                             Helpers.CreateRunActions(apply_buff),
                                             Helpers.CreateSpellComponent(SpellSchool.Transmutation)
                                             );
            iron_body.setMiscAbilityParametersSelfOnly();
            iron_body.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken;

            iron_body.AddToSpellList(Helpers.wizardSpellList, 8);
            iron_body.AddSpellAndScroll("a8422c98704e6a0429ebc5a56e132d95"); //stone skin
        }


        static void createPainStrike()
        {
            var sickened = library.Get<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323");
            var apply_sickened = Common.createContextActionApplyBuff(sickened, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false, is_child: true, is_from_spell: true);
            var dot = Helpers.CreateBuff("PainStrikeBuff",
                                         "Pain Strike",
                                         $"Pain strike racks the targeted creature with agony, inflicting 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage per round for 1 round per level (maximum 10 rounds). Additionally, the affected creature is sickened for the spell’s duration.",
                                         "",
                                         sickened.Icon,
                                         null,
                                         Helpers.CreateAddFactContextActions(activated: apply_sickened,
                                                                             newRound: Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 1, 0), IgnoreCritical: true)
                                                                             ),
                                         Helpers.CreateSpellDescriptor(SpellDescriptor.Death)
                                        );

            var apply_dot = Common.createContextActionApplyBuff(dot, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Rounds), is_from_spell: true);
            pain_strike = Helpers.CreateAbility("PainStrikeAbility",
                                                "Pain Strike",
                                                $"Pain strike racks the targeted creature with agony, inflicting 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage per round for 1 round per level (maximum 10 rounds). Additionally, the affected creature is sickened for the spell’s duration.",
                                                "",
                                                sickened.Icon,
                                                AbilityType.Spell,
                                                UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                Helpers.roundsPerLevelDuration,
                                                Helpers.fortNegates,
                                                Helpers.CreateRunActions(SavingThrowType.Fortitude,
                                                                         Helpers.CreateConditionalSaved(null, apply_dot)),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.Death | SpellDescriptor.Evil),
                                                Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                Helpers.CreateContextRankConfig(max: 10)
                                                );
            pain_strike.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            pain_strike.SpellResistance = true;
            pain_strike.AvailableMetamagic = Metamagic.Extend | Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Reach | Metamagic.Heighten | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing;
            pain_strike_mass = library.CopyAndAdd(pain_strike, "PainStrikeMassAbility", "");
            pain_strike_mass.SetNameDescription("Pain Strike, Mass",
                                                "This spell works like pain strike, except as noted above.\n" + pain_strike.Name + ": " + pain_strike.Description);
            pain_strike_mass.AddComponent(Helpers.CreateAbilityTargetsAround(15.Feet(), TargetType.Enemy));
            pain_strike_mass.Range = AbilityRange.Medium;
            pain_strike.AddToSpellList(Helpers.wizardSpellList, 3);
            pain_strike_mass.AddToSpellList(Helpers.wizardSpellList, 5);

            pain_strike.AddSpellAndScroll("c92308c160d6d424fb64f1fd708aa6cd"); //stinking cloud
            pain_strike_mass.AddSpellAndScroll("c92308c160d6d424fb64f1fd708aa6cd"); //stinking cloud
        }


        static void createInflictPain()
        {
            var buff = Helpers.CreateBuff("InflictPainBuff",
                                         "Inflict Pain",
                                         "You telepathically wrack the target’s mind and body with agonizing pain that imposes a –4 penalty on attack rolls, skill checks, and ability checks. A successful Will save reduces the duration to 1 round.",
                                         "",
                                         Helpers.GetIcon("e5cb4c4459e437e49a4cd73fde6b9063"),
                                         Common.createPrefabLink("d3c03d0642effaf4c8f4deb356926870"),//sickened
                                         Helpers.CreateAddStatBonus(StatType.AdditionalAttackBonus, -4, ModifierDescriptor.UntypedStackable),
                                         Helpers.Create<BuffAllSkillsBonus>(b => { b.Value = -4; b.Descriptor = ModifierDescriptor.UntypedStackable; }),
                                         Common.createAbilityScoreCheckBonus(-4, ModifierDescriptor.UntypedStackable),
                                         Helpers.CreateSpellDescriptor(SpellDescriptor.Death | SpellDescriptor.MindAffecting)
                                        );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Rounds), is_from_spell: true);
            var apply_buff1 = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1, DurationRate.Rounds), is_from_spell: true);
            inflict_pain = Helpers.CreateAbility("InflictPainAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Spell,
                                                UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                Helpers.roundsPerLevelDuration,
                                                "Will partial",
                                                Helpers.CreateRunActions(SavingThrowType.Will,
                                                                         Helpers.CreateConditionalSaved(apply_buff1, apply_buff)),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.Death | SpellDescriptor.MindAffecting),
                                                Helpers.CreateSpellComponent(SpellSchool.Enchantment),
                                                Helpers.CreateContextRankConfig()
                                                );
            inflict_pain.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            inflict_pain.SpellResistance = true;
            inflict_pain.AvailableMetamagic = Metamagic.Extend | Metamagic.Quicken | Metamagic.Reach | Metamagic.Heighten | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            inflict_pain_mass = library.CopyAndAdd(inflict_pain, "InflictPainMassAbility", "");
            inflict_pain_mass.SetNameDescription("Inflict Pain, Mass",
                                                "This spell functions like inflict pain except as noted above.\n" + inflict_pain.Name + ": " + inflict_pain.Description
                                                );
            inflict_pain_mass.AddComponent(Helpers.CreateAbilityTargetsAround(15.Feet(), TargetType.Enemy));

            inflict_pain.AddToSpellList(Helpers.wizardSpellList, 3);
            inflict_pain.AddToSpellList(Helpers.inquisitorSpellList, 2);
            inflict_pain_mass.AddToSpellList(Helpers.inquisitorSpellList, 5);
            inflict_pain_mass.AddToSpellList(Helpers.wizardSpellList, 7);

            inflict_pain.AddSpellAndScroll("7385837b610622b4d96c87c7b7e63e05"); //inflict light wounds
            inflict_pain_mass.AddSpellAndScroll("7385837b610622b4d96c87c7b7e63e05"); //inflict light wounds
        }


        static void createSynesthesia()
        {
            var buff = Helpers.CreateBuff("SynesthesiaBuff",
                                         "Synesthesia",
                                         "You overstimulate the senses of the affected creature, causing its senses to interfere with another. While a creature is under the effects of this spell, sensory input is processed by the wrong senses, such that noise triggers bursts of colors, smells create sounds, and so on. The affected creature moves at half speed, has a 20% miss chance on attacks, and takes a –4 penalty to AC and on skill checks and Reflex saves. Successful spellcasting while affected requires a concentration check with a DC equal to 15 plus the level of spell being cast.",
                                         "",
                                         Helpers.GetIcon("55f14bc84d7c85446b07a1b5dd6b2b4c"), //daze
                                         Common.createPrefabLink("396af91a93f6e2b468f5fa1a944fae8a"),//daze
                                         Helpers.CreateAddStatBonus(StatType.AC, -4, ModifierDescriptor.UntypedStackable),
                                         Helpers.CreateAddStatBonus(StatType.SaveReflex, -4, ModifierDescriptor.UntypedStackable),
                                         Helpers.Create<BuffAllSkillsBonus>(b => { b.Value = -4; b.Descriptor = ModifierDescriptor.UntypedStackable; }),
                                         Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.SpellCastingIsDifficult),
                                         Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.Slowed),
                                         Helpers.Create<ConcealementMechanics.AddOutgoingConcealment>(a =>
                                         {
                                             a.Concealment = Concealment.Partial;
                                             a.Descriptor = ConcealmentDescriptor.InitiatorIsBlind;
                                         }
                                         ),
                                         Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting)
                                        );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Rounds), is_from_spell: true);
            synesthesia = Helpers.CreateAbility("SynestesiaAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Spell,
                                                UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                Helpers.roundsPerLevelDuration,
                                                Helpers.willNegates,
                                                Helpers.CreateRunActions(SavingThrowType.Will,
                                                                         Helpers.CreateConditionalSaved(null, apply_buff)),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting),
                                                Helpers.CreateSpellComponent(SpellSchool.Illusion),
                                                Helpers.CreateContextRankConfig()
                                                );
            synesthesia.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            synesthesia.SpellResistance = true;
            synesthesia.AvailableMetamagic = Metamagic.Extend | Metamagic.Quicken | Metamagic.Reach | Metamagic.Heighten | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            synesthesia_mass = library.CopyAndAdd(synesthesia, "SynesthesiaMassAbility", "");
            synesthesia_mass.SetNameDescription("Synesthesia, Mass",
                                                "This spell functions like synesthesia, except as noted above.\n" + synesthesia.Name + ": " + synesthesia.Description
                                                );
            synesthesia_mass.Range = AbilityRange.Medium;
            synesthesia_mass.AddComponent(Helpers.CreateAbilityTargetsAround(15.Feet(), TargetType.Enemy));
            Helpers.AddSpell(synesthesia_mass);
            Helpers.AddSpell(synesthesia);
        }


        static void createPsychicCrush()
        {
            var check_intelligent = Common.createAbilityTargetHasFact(true, Common.construct, Common.vermin);
            var sickened = library.Get<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323");
            var apply_sickened = Common.createContextActionApplyBuff(sickened, Helpers.CreateContextDuration(1), is_from_spell: true);
            var icon = Helpers.GetIcon("137af566f68fd9b428e2e12da43c1482");

            var spawn_fx = Common.createContextActionSpawnFx(Common.createPrefabLink("2a37573c2eb79f04a85d3832f1195962"));
            var dmg1 = Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 3, Helpers.CreateContextValue(AbilityRankType.Default)));
            var effect1 = Helpers.CreateActionSavingThrow(SavingThrowType.Fortitude,
                                                          Helpers.CreateConditionalSaved(new GameAction[] { dmg1, apply_sickened },
                                                                                         new GameAction[] { Helpers.Create<ContextActionKillTarget>() }
                                                                                        )
                                                         );
            Common.addConditionalDCIncrease(effect1, Helpers.CreateConditionsCheckerOr(Helpers.Create<NewMechanics.ContextConditionCompareTargetHPPercent>(c => { c.Value = 50; c.Not = true; })), -4);

            psychic_crush[0] = Helpers.CreateAbility("PsychicCrush1Ability",
                                                     "Psychic Crush I",
                                                     $"Using your psychic power, you invade the mind of the target and tear it asunder, causing massive internal damage to both its mind and body. If the target succeeds at the initial Will save, it is sickened for 1 round. If the target fails its Will save, it must attempt a Fortitude save (with a +4 circumstance bonus on this save if it has more than half its total hit points remaining). If it also fails the Fortitude save, the target is reduced to –1 hit points and is dying. If the target succeeds at its Fortitude save, it instead takes 3d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage + 1 point of damage per caster level, which cannot reduce the target below –1 hit point, and the target is sickened for 1 round. This attack has no effect on creatures without an Intelligence score.",
                                                     "",
                                                     icon,
                                                     AbilityType.Spell,
                                                     UnitCommand.CommandType.Standard,
                                                     AbilityRange.Close,
                                                     "",
                                                     "Will partial and Fortitude partial",
                                                     Helpers.CreateRunActions(SavingThrowType.Will,
                                                                              Helpers.CreateConditionalSaved(new GameAction[] { apply_sickened }, new GameAction[] { spawn_fx, effect1 })
                                                                             ),
                                                     Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                     Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting),
                                                     Helpers.CreateContextRankConfig(),
                                                     Common.createAbilitySpawnFx("cbfe312cb8e63e240a859efaad8e467c", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                     check_intelligent
                                                     );
            psychic_crush[0].SpellResistance = true;
            psychic_crush[0].setMiscAbilityParametersSingleTargetRangedHarmful(true);
            psychic_crush[0].AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Reach | Metamagic.Heighten | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;

            var dmg2 = Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 5, Helpers.CreateContextValue(AbilityRankType.Default)));
            var effect2 = Helpers.CreateActionSavingThrow(SavingThrowType.Fortitude,
                                                          Helpers.CreateConditionalSaved(new GameAction[] { dmg2, apply_sickened },
                                                                                         new GameAction[] { Helpers.Create<ContextActionKillTarget>() }
                                                                                        )
                                                         );
            Common.addConditionalDCIncrease(effect2, 
                                            new ConditionsChecker[]
                                            { Helpers.CreateConditionsCheckerOr(Helpers.Create<NewMechanics.ContextConditionCompareTargetHPPercent>(c => { c.Value = 50; c.Not = true; })),
                                              Helpers.CreateConditionsCheckerOr(Helpers.Create<NewMechanics.ContextConditionCompareTargetHPPercent>(c => { c.Value = 100; c.Not = true; }))
                                            }, 
                                            -2);

            psychic_crush[1] = Helpers.CreateAbility("PsychicCrush2Ability",
                                                     "Psychic Crush II",
                                                     $"This functions as psychic crush I, but on a successful Fortitude save, the target takes 5d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage + 1 point of damage per caster level. In addition, the target receives a +4 circumstance bonus on the Fortitude save only if it is at full hit points; otherwise, it gains a +2 bonus if it has more than half its total hit points remaining.\n"
                                                     + psychic_crush[0].Name + ": " + psychic_crush[0].Description,
                                                     "",
                                                     icon,
                                                     AbilityType.Spell,
                                                     UnitCommand.CommandType.Standard,
                                                     AbilityRange.Close,
                                                     "",
                                                     "Will partial and Fortitude partial",
                                                     Helpers.CreateRunActions(SavingThrowType.Will,
                                                                              Helpers.CreateConditionalSaved(new GameAction[] { apply_sickened }, new GameAction[] { spawn_fx, effect2 })
                                                                             ),
                                                     Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                     Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting),
                                                     Helpers.CreateContextRankConfig(),
                                                     Common.createAbilitySpawnFx("cbfe312cb8e63e240a859efaad8e467c", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                     check_intelligent
                                                     );
            psychic_crush[1].SpellResistance = true;
            psychic_crush[1].setMiscAbilityParametersSingleTargetRangedHarmful(true);
            psychic_crush[1].AvailableMetamagic = psychic_crush[0].AvailableMetamagic;


            var dmg3 = Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 7, Helpers.CreateContextValue(AbilityRankType.Default)));
            var effect3 = Helpers.CreateActionSavingThrow(SavingThrowType.Fortitude,
                                                          Helpers.CreateConditionalSaved(new GameAction[] { dmg3, apply_sickened },
                                                                                         new GameAction[] { Helpers.Create<ContextActionKillTarget>() }
                                                                                        )
                                                         );
            Common.addConditionalDCIncrease(effect3,
                                            Helpers.CreateConditionsCheckerOr(Helpers.Create<NewMechanics.ContextConditionCompareTargetHPPercent>(c => { c.Value = 100; c.Not = true; })),
                                            -2);

            psychic_crush[2] = Helpers.CreateAbility("PsychicCrush3Ability",
                                                     "Psychic Crush III",
                                                     $"This functions as psychic crush I, but the target takes 7d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage + 1 point of damage per caster level on a successful Fortitude save and 1 point of damage per caster level on a successful Will save. The target receives a +2 circumstance bonus on the Fortitude save if it is at full hit points, and no bonus if it has taken any damage.\n"
                                                     + psychic_crush[0].Name + ": " + psychic_crush[0].Description,
                                                     "",
                                                     icon,
                                                     AbilityType.Spell,
                                                     UnitCommand.CommandType.Standard,
                                                     AbilityRange.Close,
                                                     "",
                                                     "Will partial and Fortitude partial",
                                                     Helpers.CreateRunActions(SavingThrowType.Will,
                                                                              Helpers.CreateConditionalSaved(new GameAction[] { apply_sickened }, new GameAction[] { spawn_fx, effect3 })
                                                                             ),
                                                     Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                     Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting),
                                                     Helpers.CreateContextRankConfig(),
                                                     Common.createAbilitySpawnFx("cbfe312cb8e63e240a859efaad8e467c", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                     check_intelligent
                                                     );
            psychic_crush[2].SpellResistance = true;
            psychic_crush[2].setMiscAbilityParametersSingleTargetRangedHarmful(true);
            psychic_crush[2].AvailableMetamagic = psychic_crush[0].AvailableMetamagic;


            var dmg4 = Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 9, Helpers.CreateContextValue(AbilityRankType.Default)));
            var effect4_1 = Helpers.CreateActionSavingThrow(SavingThrowType.Fortitude,
                                                          Helpers.CreateConditionalSaved(new GameAction[] { dmg4, apply_sickened },
                                                                                         new GameAction[] { Helpers.Create<ContextActionKillTarget>() }
                                                                                        )
                                                         );
            var effect4 = Helpers.CreateConditional(Helpers.CreateConditionsCheckerOr(Helpers.Create<NewMechanics.ContextConditionCompareTargetHPPercent>(c => c.Value = 50)),
                                                    Helpers.Create<ContextActionKillTarget>(),
                                                    effect4_1
                                                    );

            psychic_crush[3] = Helpers.CreateAbility("PsychicCrush4Ability",
                                                     "Psychic Crush IV",
                                                     $"This functions as psychic crush I, but the target takes 9d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage + 1 point of damage per caster level on a successful Fortitude or Will save. The target does not receive any saving throw bonus because of its hit points. If it is at fewer than half its total hit points, it doesn’t gain a Fortitude save to resist this spell but instead proceeds as if it had automatically failed its Fortitude save.\n"
                                                     + psychic_crush[0].Name + ": " + psychic_crush[0].Description,
                                                     "",
                                                     icon,
                                                     AbilityType.Spell,
                                                     UnitCommand.CommandType.Standard,
                                                     AbilityRange.Close,
                                                     "",
                                                     "Will partial and Fortitude partial",
                                                     Helpers.CreateRunActions(SavingThrowType.Will,
                                                                              Helpers.CreateConditionalSaved(new GameAction[] { apply_sickened, dmg4 }, new GameAction[] { spawn_fx, effect4 })
                                                                             ),
                                                     Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                     Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting),
                                                     Helpers.CreateContextRankConfig(),
                                                     Common.createAbilitySpawnFx("cbfe312cb8e63e240a859efaad8e467c", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                     check_intelligent
                                                     );
            psychic_crush[3].SpellResistance = true;
            psychic_crush[3].setMiscAbilityParametersSingleTargetRangedHarmful(true);
            psychic_crush[3].AvailableMetamagic = psychic_crush[0].AvailableMetamagic;



            var dmg5 = Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 11, Helpers.CreateContextValue(AbilityRankType.Default)));
            var effect5_1 = Helpers.CreateActionSavingThrow(SavingThrowType.Fortitude,
                                                          Helpers.CreateConditionalSaved(new GameAction[] { dmg5, apply_sickened },
                                                                                         new GameAction[] { Helpers.Create<ContextActionKillTarget>() }
                                                                                        )
                                                         );
            var effect5 = Helpers.CreateConditional(Helpers.CreateConditionsCheckerOr(Helpers.Create<NewMechanics.ContextConditionCompareTargetHPPercent>(c => c.Value = 50)),
                                                    Helpers.Create<ContextActionKillTarget>(),
                                                    effect4_1
                                                    );

            var effect5_2 = Helpers.CreateActionSavingThrow(SavingThrowType.Will,
                                                            Helpers.CreateConditionalSaved(new GameAction[] { dmg5, apply_sickened },
                                                                                           new GameAction[] { effect5 }
                                                                                          )
                                                           );

            Common.addConditionalDCIncrease(effect5_2,
                                            Helpers.CreateConditionsCheckerOr(Helpers.Create<NewMechanics.ContextConditionCompareTargetHPPercent>(c => c.Value = 50)),
                                            2);

            psychic_crush[4] = Helpers.CreateAbility("PsychicCrush5Ability",
                                                     "Psychic Crush V",
                                                     $"This functions as psychic crush IV, but on a successful Fortitude or Will save, the target takes 11d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage + 1 point of damage per caster level. If it is at fewer than half its total hit points, the target takes a –2 penalty on the Will save to resist this spell.\n"
                                                     + psychic_crush[3].Name + ": " + psychic_crush[3].Description,
                                                     "",
                                                     icon,
                                                     AbilityType.Spell,
                                                     UnitCommand.CommandType.Standard,
                                                     AbilityRange.Close,
                                                     "",
                                                     "Will partial and Fortitude partial",
                                                     Helpers.CreateRunActions(effect5_2),
                                                     Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                     Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting),
                                                     Helpers.CreateContextRankConfig(),
                                                     Common.createAbilitySpawnFx("cbfe312cb8e63e240a859efaad8e467c", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                     check_intelligent
                                                     );
            psychic_crush[4].SpellResistance = true;
            psychic_crush[4].setMiscAbilityParametersSingleTargetRangedHarmful(true);
            psychic_crush[4].AvailableMetamagic = psychic_crush[0].AvailableMetamagic;


            for (int i = 0; i < psychic_crush.Length; i++)
            {
                psychic_crush[i].AddComponent(Helpers.Create<SpellbookMechanics.SpellUndercast>(s =>
                {
                    s.undercast_abilities = psychic_crush.Take(i).ToArray();
                    s.overcast_abilities = psychic_crush.Skip(i+1).ToArray();
                })
                );

                psychic_crush[i].SetDescription(psychic_crush[i].Description + (i == 0 ? "" : "\nThis spell can be undercast."));
                Helpers.AddSpell(psychic_crush[i]);
            }
        }

        
        static void createBurstOfForce()
        {
            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Holy, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.Default)), isAoE: true, halfIfSaved: true);
            dmg.DamageType.Type = DamageType.Force;

            var effect = Helpers.CreateActionSavingThrow(SavingThrowType.Reflex, Helpers.CreateConditionalSaved(null,
                                                                                                                Helpers.Create<ContextActionKnockdownTarget>()
                                                                                                               )
                                                        );

            burst_of_force = Helpers.CreateAbility("BurstOfForceAbility",
                                                    "Burst of Force",
                                                    $"With a burst of telekinetic force, you deal 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of force damage per caster level (maximum 15d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage) to all other creatures in the affected area. A successful Fortitude save reduces the damage taken by half. A creature that fails its Fortitude save must also succeed at a Reflex save or be knocked prone.",
                                                    "",
                                                    Helpers.GetIcon("3cf0a759bc612264fb9b03aa2f90b24b"), //fragmentation
                                                    AbilityType.Spell,
                                                    UnitCommand.CommandType.Standard,
                                                    AbilityRange.Personal,
                                                    "",
                                                    "Fortitude half",
                                                    Helpers.CreateRunActions(SavingThrowType.Fortitude,
                                                                            Helpers.CreateConditional(Common.createContextConditionIsCaster(), new GameAction[0],
                                                                                                        new GameAction[]{dmg,
                                                                                                                        Helpers.CreateConditionalSaved(null, effect)
                                                                                                                        }
                                                                                                    )
                                                                            ),
                                                    Helpers.CreateContextRankConfig(max: 15, feature: MetamagicFeats.intensified_metamagic),
                                                    Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                    Helpers.CreateSpellDescriptor(SpellDescriptor.Force),
                                                    Helpers.Create<SharedSpells.CannotBeShared>(),
                                                    Helpers.CreateAbilityTargetsAround(20.Feet(), TargetType.Any),
                                                    Common.createAbilitySpawnFxDestroyOnCast("859a6d74aedf5f349a470ab14afb47d3", anchor: AbilitySpawnFxAnchor.SelectedTarget, position_anchor: AbilitySpawnFxAnchor.SelectedTarget)
                                                   );
            burst_of_force.setMiscAbilityParametersSelfOnly();
            burst_of_force.EffectOnAlly = AbilityEffectOnUnit.Harmful;
            burst_of_force.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            burst_of_force.SpellResistance = true;

            burst_of_force.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | (Metamagic)MetamagicFeats.MetamagicExtender.IntensifiedGeneral | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing | (Metamagic)MetamagicFeats.MetamagicExtender.Toppling | (Metamagic)MetamagicFeats.MetamagicExtender.Selective;

            burst_of_force.AddToSpellList(Helpers.wizardSpellList, 5);
            burst_of_force.AddSpellAndScroll("e029ec259c9a37249b113060df32a01d"); //stunning barrier
            Helpers.AddSpell(burst_of_force);
        }

        
        static void createSynapseOverload()
        {
            var stagger = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");

            var synapse_overload_touch = Helpers.CreateAbility("SynapseOverloadAbility",
                                                             "Synapse Overload",
                                                             $"You cause the target’s mind to unleash a vast overflowing torrent of information throughout the target’s body, causing the target’s synapses to violently trigger. The target takes 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of electrical damage per caster level (maximum 15d{BalanceFixes.getDamageDieString(DiceType.D6)}) and is staggered for 1 minute. A successful Fortitude saving throw doesn’t reduce the damage, but it negates the staggered effect.",
                                                             "",
                                                             LoadIcons.Image2Sprite.Create(@"AbilityIcons/TemporalStasis.png"),
                                                             AbilityType.Spell,
                                                             UnitCommand.CommandType.Standard,
                                                             AbilityRange.Touch,
                                                             Helpers.oneMinuteDuration,
                                                             "Fortitude partial",
                                                             Helpers.CreateRunActions(SavingThrowType.Fortitude,
                                                                                       Helpers.CreateActionDealDamage(DamageEnergyType.Electricity, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.Default), 0), false, false),
                                                                                       Helpers.CreateConditionalSaved(null,
                                                                                                                      Common.createContextActionApplyBuff(stagger, Helpers.CreateContextDuration(1, DurationRate.Minutes), is_from_spell: true)
                                                                                                                      )
                                                                                       ),
                                                             Helpers.CreateSpellComponent(SpellSchool.Divination),
                                                             Helpers.CreateDeliverTouch(),
                                                             Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting),
                                                             Helpers.CreateContextRankConfig(max: 15, feature: MetamagicFeats.intensified_metamagic)
                                                             );
            synapse_overload_touch.setMiscAbilityParametersTouchHarmful();
            synapse_overload_touch.SpellResistance = true;
            synapse_overload_touch.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Reach | Metamagic.Heighten | (Metamagic)MetamagicFeats.MetamagicExtender.IntensifiedGeneral | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            synapse_overload = Helpers.CreateTouchSpellCast(synapse_overload_touch);
            Helpers.AddSpell(synapse_overload);
        }


        static void createBurstOfInsight()
        {
            var stats = new StatType[] { StatType.Charisma, StatType.Intelligence, StatType.Wisdom };

            var after_buff = library.Get<BlueprintBuff>("9934fedff1b14994ea90205d189c8759"); //dazed
            var apply_after_buff = Common.createContextActionApplyBuff(after_buff, Helpers.CreateContextDuration(1), dispellable: false);

            var variants = new List<BlueprintAbility>();
            string description = "You plumb the depths of your mind for insight, leaving you momentarily frazzled. For one round you gain a +8 enhancement bonus on Intelligence, Wisdom, or Charisma, but you are dazed for 1 round afterward.";
            var icon = Helpers.GetIcon("4cf3d0fae3239ec478f51e86f49161cb");
            foreach (var s in stats)
            {
                var buff = Helpers.CreateBuff(s.ToString() + "BurstOfInsightBuff",
                                              "Burst of Insight: " + s.ToString(),
                                              description,
                                              "",
                                              icon,
                                              library.Get<BlueprintBuff>("a1ffec0ce7c167a40aaea13dc49b757b").FxOnStart,
                                              Helpers.CreateAddStatBonus(s, 8, ModifierDescriptor.Enhancement),
                                              Helpers.CreateAddFactContextActions(deactivated: apply_after_buff)
                                              );

                var ability = Helpers.CreateAbility(s.ToString() + "BurstOfInsightAbility",
                                                    buff.Name,
                                                    buff.Description,
                                                    "",
                                                    buff.Icon,
                                                    AbilityType.Spell,
                                                    UnitCommand.CommandType.Swift,
                                                    AbilityRange.Personal,
                                                    Helpers.oneRoundDuration,
                                                    "",
                                                    Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1), is_from_spell: true)),
                                                    Helpers.CreateSpellComponent(SpellSchool.Transmutation)
                                                    );
                ability.setMiscAbilityParametersSelfOnly();
                ability.AvailableMetamagic = Metamagic.Heighten;
                variants.Add(ability);
            }

            burst_of_insight = Common.createVariantWrapper("BurstOfInsightAbility", "", variants.ToArray());
            burst_of_insight.SetName("Burst of Insight");
            burst_of_insight.AddComponent(Helpers.CreateSpellComponent(SpellSchool.Transmutation));
            Helpers.AddSpell(burst_of_insight);
        }


        static void createBurstOfAdrenaline()
        {
            var stats = new StatType[] { StatType.Constitution, StatType.Dexterity, StatType.Strength };

            var after_buff = library.Get<BlueprintBuff>("e6f2fc5d73d88064583cb828801212f4"); //fatigued
            var apply_after_buff = Common.createContextActionApplyBuff(after_buff, Helpers.CreateContextDuration(1), dispellable: false);

            var variants = new List<BlueprintAbility>();
            string description = "You draw upon your body’s inner reserves of strength, leaving you winded. For one round you gain a +8 enhancement bonus to Strength, Dexterity, or Constitution, but you are fatigued for 1 round afterward.";
            var icon = Helpers.GetIcon("3dccdf27a8209af478ac71cded18a271");
            foreach (var s in stats)
            {
                var buff = Helpers.CreateBuff(s.ToString() + "BurstOfAdrenalineBuff",
                                              "Burst of Adrenaline: " + s.ToString(),
                                              description,
                                              "",
                                              icon,
                                              library.Get<BlueprintBuff>("a1ffec0ce7c167a40aaea13dc49b757b").FxOnStart,
                                              Helpers.CreateAddStatBonus(s, 8, ModifierDescriptor.Enhancement),
                                              Helpers.CreateAddFactContextActions(deactivated: apply_after_buff)
                                              );

                var ability = Helpers.CreateAbility(s.ToString() + "BurstOfAdrenalineAbility",
                                                    buff.Name,
                                                    buff.Description,
                                                    "",
                                                    buff.Icon,
                                                    AbilityType.Spell,
                                                    UnitCommand.CommandType.Swift,
                                                    AbilityRange.Personal,
                                                    Helpers.oneRoundDuration,
                                                    "",
                                                    Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1), is_from_spell: true)),
                                                    Helpers.CreateSpellComponent(SpellSchool.Transmutation)
                                                    );
                ability.setMiscAbilityParametersSelfOnly();
                ability.AvailableMetamagic = Metamagic.Heighten;
                variants.Add(ability);    
            }

            burst_of_adrenaline = Common.createVariantWrapper("BurstOfAdrenalineAbility", "", variants.ToArray());
            burst_of_adrenaline.SetName("Burst of Adrenaline");
            burst_of_adrenaline.AddComponent(Helpers.CreateSpellComponent(SpellSchool.Transmutation));
            Helpers.AddSpell(burst_of_adrenaline);
        }


        static void createFireShuriken()
        {
            var fire_bolt = library.Get<BlueprintAbility>("4ecdf240d81533f47a5279f5075296b9");

            var resource = Helpers.CreateAbilityResource("FieryShurikenResource", "", "", "", null);
            resource.SetFixedResource(8);

            var description = $"You call forth two fiery projectiles resembling shuriken, plus one more for every two caster levels beyond 3rd (to a maximum of eight shuriken at 15th level), which hover in front of you. When these shuriken appear, you can launch some or all of them at the same target or different targets.Each shuriken requires a ranged touch attack roll to hit and deals 1d{BalanceFixes.getDamageDieString(DiceType.D8)} points of fire damage. You provoke no attacks of opportunity when launching them. Any shuriken you do not launch as part of casting this spell remains floating near you for the spell’s duration.On rounds subsequent to your casting of this spell, you can spend a swift action to launch one of these remaining shuriken or a standard action to launch any number of these remaining shuriken. If you fail to launch a shuriken before the duration ends, that shuriken disappears and is wasted.";
            var one_projectile_spell_swift = library.CopyAndAdd(fire_bolt, "FieryShurikenSingleProjectileAbility", "");
            one_projectile_spell_swift.SetNameDescription("Fiery Shuriken (1 Projectile, Swift)", description);

            one_projectile_spell_swift.Type = AbilityType.SpellLike;
            one_projectile_spell_swift.SpellResistance = true;
            one_projectile_spell_swift.ActionType = UnitCommand.CommandType.Swift;
            one_projectile_spell_swift.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            one_projectile_spell_swift.RemoveComponents<SpellComponent>();
            one_projectile_spell_swift.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = resource);
            one_projectile_spell_swift.RemoveComponents<ContextRankConfig>();
            one_projectile_spell_swift.RemoveComponents<AbilityEffectRunAction>();
            one_projectile_spell_swift.AddComponent(Helpers.CreateRunActions(Helpers.CreateActionDealDamage(DamageEnergyType.Fire, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D8), 1, 0))));
            one_projectile_spell_swift.LocalizedDuration = Helpers.CreateString("FireShurikenSingleProjectile.Duration", "");
            one_projectile_spell_swift.AddComponent(Helpers.CreateSpellComponent(SpellSchool.Conjuration));
            one_projectile_spell_swift.AvailableMetamagic = Metamagic.Reach | Metamagic.Empower | Metamagic.Maximize | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Rime;
            var abilities = new List<BlueprintAbility>();
            abilities.Add(one_projectile_spell_swift);
            for (int i = 2; i <= 7; i++)
            {
                var ability = library.CopyAndAdd(one_projectile_spell_swift, $"FieryShuriken{i}ProjectilesAbility", "");
                ability.ActionType = UnitCommand.CommandType.Standard;
                ability.ReplaceComponent<AbilityResourceLogic>(a => { a.Amount = i; });
                ability.AddComponent(Helpers.Create<NewMechanics.AbilityShowIfCasterHasResource>(a => { a.resource = resource; a.amount = i; }));
                var projectile = one_projectile_spell_swift.GetComponent<AbilityDeliverProjectile>().Projectiles[0];
                ability.ReplaceComponent<AbilityDeliverProjectile>(a => { a.Projectiles = Enumerable.Repeat(projectile, i).ToArray(); a.DelayBetweenProjectiles = 0.2f; });
                ability.SetNameDescription($"Fiery Shuriken ({i} Projectiles)", description);
                abilities.Add(ability);
            }


            var wrapper = Common.createVariantWrapper("FireShurikenAbilityBase", "", abilities.ToArray());
            wrapper.SetName("Fiery Shuriken");

            var buff = Helpers.CreateBuff("FieryShurikenBuff",
                                          wrapper.Name,
                                          wrapper.Description,
                                          "",
                                          wrapper.Icon,
                                          null,
                                          Helpers.CreateAddAbilityResourceNoRestore(resource),
                                          Helpers.CreateAddFact(wrapper),
                                          Helpers.CreateAddFactContextActions(newRound: Helpers.CreateConditional(Helpers.Create<ResourceMechanics.ContextConditionTargetHasEnoughResource>(c => c.resource = resource),
                                                                                                                  null,
                                                                                                                  Helpers.Create<ContextActionRemoveSelf>()
                                                                                                                  )
                                                                             )

                                        );
            buff.SetBuffFlags(BuffFlags.RemoveOnRest);
            foreach (var a in abilities)
            {
                buff.AddComponent(Helpers.Create<ReplaceAbilityParamsWithContext>(r => r.Ability = a));
            }

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Rounds), is_from_spell: true);



            var fiery_shuriken_one_projectile = library.CopyAndAdd(one_projectile_spell_swift, "FieryShurikenOneProjectileSpell", "");
            fiery_shuriken_one_projectile.Type = AbilityType.Spell;
            fiery_shuriken_one_projectile.SetName("Fiery Shuriken (1 Projectile)");
            fiery_shuriken_one_projectile.ActionType = UnitCommand.CommandType.Standard;
            fiery_shuriken_one_projectile.AvailableMetamagic = fiery_shuriken_one_projectile.AvailableMetamagic | Metamagic.Heighten | Metamagic.Extend | Metamagic.Quicken | Metamagic.Reach;

            fiery_shuriken_one_projectile.RemoveComponents<AbilityResourceLogic>();
            var action_on_caster = Common.createContextActionOnContextCaster(apply_buff,
                                                                Helpers.Create<ResourceMechanics.ContextRestoreResource>(c =>
                                                                {
                                                                    c.amount = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                    c.Resource = resource;
                                                                })
                                                                );
            fiery_shuriken_one_projectile.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(a.Actions.Actions.AddToArray(action_on_caster)));
            fiery_shuriken_one_projectile.AddComponent(Helpers.CreateContextRankConfig(progression: ContextRankProgression.DelayedStartPlusDivStep, 
                                                                                       startLevel: 3, stepLevel: 2, max: 7)
                                                                                       );

            var fiery_shuriken_all_projectiles = library.CopyAndAdd(one_projectile_spell_swift, "FieryShurikenAllProjectilesSpell", "");
            fiery_shuriken_all_projectiles.Type = AbilityType.Spell;
            fiery_shuriken_all_projectiles.SetName("Fiery Shuriken (All Projectiles)");
            fiery_shuriken_all_projectiles.RemoveComponents<AbilityResourceLogic>();
            fiery_shuriken_all_projectiles.ActionType = UnitCommand.CommandType.Standard;
            fiery_shuriken_all_projectiles.AvailableMetamagic = fiery_shuriken_one_projectile.AvailableMetamagic | Metamagic.Heighten | Metamagic.Extend | Metamagic.Quicken | Metamagic.Reach;
            fiery_shuriken_all_projectiles.ReplaceComponent<AbilityDeliverProjectile>(a =>
                                                                                     {
                                                                                         a.Projectiles = Enumerable.Repeat(a.Projectiles[0], 8).ToArray();
                                                                                         a.UseMaxProjectilesCount = true;
                                                                                         a.DelayBetweenProjectiles = 0.2f;
                                                                                         a.MaxProjectilesCountRank = AbilityRankType.Default;
                                                                                     });
            fiery_shuriken_all_projectiles.AddComponent(Helpers.CreateContextRankConfig(progression: ContextRankProgression.DelayedStartPlusDivStep,
                                                                                       startLevel: 1, stepLevel: 2, max: 8));

            
            fiery_shiriken = Common.createVariantWrapper("FieryShurikenBaseSpell", "", fiery_shuriken_one_projectile, fiery_shuriken_all_projectiles);
            fiery_shiriken.SetName("Fiery Shuriken");
            fiery_shiriken.AddComponent(Helpers.CreateSpellComponent(SpellSchool.Conjuration));
            fiery_shiriken.AddToSpellList(Helpers.wizardSpellList, 2);
            fiery_shiriken.AddSpellAndScroll("95bf8273fd7930c4da3eaaf636a6cd29");
        }


        static void createFickleWinds()
        {
            var icon = library.Get<BlueprintFeature>("f2fa7541f18b8af4896fbaf9f2a21dfe").Icon;//cyclone infusion

            var buff = Helpers.CreateBuff("FickleWindsBuff",
                                          "Fickle Winds",
                                          "You create a mobile cylinder of wind encompassing every target of the spell and protecting them as wind wall, but not interfering with them in any way. For example, arrows and bolts fired at the targets are deflected upward and miss, but the targets’ own arrows or bolts pass through the wall as if it were not there.",
                                          "",
                                          icon,
                                          Common.createPrefabLink("6dc97e33e73b5ec49bd03b90c2345d7f"),//"ea8ddc3e798aa25458e2c8a15e484c68"),
                                          Helpers.Create<NewMechanics.WeaponAttackAutoMiss>(w => w.attack_types = new AttackType[] { AttackType.Ranged})
                                          );
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), is_from_spell: true);
            fickle_winds = Helpers.CreateAbility("FickleWindsAbility",
                                                       buff.Name,
                                                       buff.Description,
                                                       "",
                                                       icon,
                                                       AbilityType.Spell,
                                                       Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                       AbilityRange.Medium,
                                                       Helpers.minutesPerLevelDuration,
                                                       "",
                                                       Helpers.CreateRunActions(apply_buff),
                                                       Helpers.CreateContextRankConfig(),
                                                       Helpers.CreateAbilityTargetsAround(15.Feet(), TargetType.Ally),
                                                       Helpers.CreateSpellComponent(SpellSchool.Transmutation)
                                                       );
            fickle_winds.setMiscAbilityParametersRangedDirectional();
            fickle_winds.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken;

            fickle_winds.AddToSpellList(Helpers.clericSpellList, 5);
            fickle_winds.AddToSpellList(Helpers.druidSpellList, 5);
            fickle_winds.AddToSpellList(Helpers.wizardSpellList, 5);
            fickle_winds.AddToSpellList(Helpers.rangerSpellList, 3);

            fickle_winds.AddSpellAndScroll("179d91b899fa6304b8c076e002890317"); //protection from arrows
        }


        static void createBarrowHaze()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/FogCloud.png");

            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("fe5102d734382b74586f56980086e5e8", "BarrowHazeArea", ""); //mind fog
            area.Fx = Common.createPrefabLink("e63a0d8a1f2d74343b374ea4bbf9d951"); //yellow stench cloud ???
            area.Size = 20.Feet();
            area.SpellResistance = false;

            var buff = Helpers.CreateBuff("BarrowHazeBuff",
                                            "Barrow Haze",
                                            "Barrow haze creates a bank of fog similar to that created by obscuring mist, except that the vapors are dark and they have a necromantic link to you. The vapors do not interfere with your vision.",
                                            "",
                                            icon,
                                            null,
                                            Helpers.Create<ConcealementMechanics.IgnoreCasterConcealemnt>(a => { a.CheckDistance = true; a.DistanceGreater = 5.Feet(); a.Descriptor = ConcealmentDescriptor.Fog; a.Concealment = Concealment.Total; })
                                            );
            var buff2 = library.CopyAndAdd(buff, "BarrowHazeOutgoingConcealementBuff", "");
            buff2.SetBuffFlags(BuffFlags.HiddenInUi);
            buff2.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.Create<ConcealementMechanics.AddOutgoingConcealment>(a => { a.Descriptor = ConcealmentDescriptor.Fog; a.Concealment = Concealment.Partial; a.CheckDistance = false; }),
                Helpers.Create<ConcealementMechanics.AddOutgoingConcealment>(a => { a.CheckDistance = true; a.DistanceGreater = 5.Feet(); a.Descriptor = ConcealmentDescriptor.Fog; a.Concealment = Concealment.Total; }),
            };

            var buff3 = library.CopyAndAdd(buff, "BarrowHazeCasterConcealement2Buff", "");
            buff3.SetBuffFlags(BuffFlags.HiddenInUi);
            buff3.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.Create<ConcealementMechanics.IgnoreCasterConcealemnt>(a => { a.Descriptor = ConcealmentDescriptor.Fog; a.Concealment = Concealment.Partial; a.CheckDistance = false; })
            };

            area.ComponentsArray = new BlueprintComponent[] { Helpers.Create<AbilityAreaEffectBuff>(a => { a.Buff = buff2; a.Condition = Helpers.CreateConditionsCheckerAnd(Common.createContextConditionIsCaster(not:true)); }),
                                                              Helpers.Create<AbilityAreaEffectBuff>(a => { a.Buff = buff3; a.Condition = Helpers.CreateConditionsCheckerOr(); }),
                                                              Helpers.Create<AbilityAreaEffectBuff>(a => { a.Buff = buff; a.Condition = Helpers.CreateConditionsCheckerOr(); })};


            barrow_haze = library.CopyAndAdd<BlueprintAbility>("68a9e6d7256f1354289a39003a46d826", "BarrowHazeAbility", "");
            barrow_haze.SpellResistance = false;
            barrow_haze.RemoveComponents<SpellListComponent>();
            barrow_haze.ReplaceComponent<SpellComponent>(s => s.School = SpellSchool.Necromancy);
            barrow_haze.RemoveComponents<SpellDescriptorComponent>();
            barrow_haze.ReplaceComponent<AbilityAoERadius>(a => Helpers.SetField(a, "m_Radius", 20.Feet()));
            barrow_haze.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(Common.changeAction<ContextActionSpawnAreaEffect>(a.Actions.Actions, s => { s.AreaEffect = area; s.DurationValue = Helpers.CreateContextDuration(s.DurationValue.BonusValue, DurationRate.Minutes); })));
            barrow_haze.SetNameDescriptionIcon(buff.Name, buff.Description, buff.Icon);
            barrow_haze.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach | Metamagic.Extend;
            barrow_haze.LocalizedDuration = Helpers.minutesPerLevelDuration;
            barrow_haze.AddToSpellList(Helpers.wizardSpellList, 3);
            barrow_haze.LocalizedSavingThrow = Helpers.CreateString("BarrowHaze.SavingThrow", "");

            barrow_haze.AddSpellAndScroll("61bacc43652d76c42b60d965b65cd741");//mind fog
        }


        static void createDesecrate()
        {
            var desecrate_buff = Helpers.CreateBuff("DesecrateBuff",
                  "Desecrate",
                  "This spell imbues an area with negative energy. The DC to resist negative channeled energy within this area gains a +3 profane bonus. Every undead creature entering a desecrated area gains a +1 profane bonus on all attack rolls, damage rolls, and saving throws.\n"
                  + "Furthermore, anyone who casts animate dead within this area may create as many as double the normal amount of undead (that is, 4 HD per caster level rather than 2 HD per caster level).",
                  "",
                  Helpers.GetIcon("48e2744846ed04b4580be1a3343a5d3d"),
                  null
                  );
            ChannelEnergyEngine.registerDesecreate(desecrate_buff);
            var desecrate_undead_buff = Helpers.CreateBuff("DesecrateUndeadBuff",
                                                            desecrate_buff.Name,
                                                            desecrate_buff.Description,
                                                            "",
                                                            Helpers.GetIcon("db0f61cd985ca09498cafde9a4b27a16"),
                                                            null,
                                                            Helpers.CreateAddStatBonus(StatType.AdditionalAttackBonus, 1, ModifierDescriptor.Profane),
                                                            Helpers.CreateAddStatBonus(StatType.AdditionalDamage, 1, ModifierDescriptor.Profane),
                                                            Helpers.CreateAddStatBonus(StatType.SaveFortitude, 1, ModifierDescriptor.Profane),
                                                            Helpers.CreateAddStatBonus(StatType.SaveReflex, 1, ModifierDescriptor.Profane),
                                                            Helpers.CreateAddStatBonus(StatType.SaveWill, 1, ModifierDescriptor.Profane)
                                                            );

            desecrate_undead_buff.SetBuffFlags(BuffFlags.HiddenInUi);


            desecrate_area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("c08bd33a377d5014a81be94e33ec8ce4", "DesecrateArea", "");
            desecrate_area.Size = 20.Feet();
            desecrate_area.Fx = Common.createPrefabLink("8a80d991f3d68e84293e098a6faa7620"); //unholy aoe
            desecrate_area.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.Create<AbilityAreaEffectBuff>(a => {a.Buff = desecrate_buff; a.Condition = Helpers.CreateConditionsCheckerOr(); }),
                Helpers.Create<AbilityAreaEffectBuff>(a => {a.Buff = desecrate_undead_buff; a.Condition = Helpers.CreateConditionsCheckerOr(Common.createContextConditionHasFact(Common.undead)); })
            };

            desecrate = Helpers.CreateAbility("DesecrateAbility",
                                                desecrate_buff.Name,
                                                desecrate_buff.Description,
                                                "",
                                                desecrate_buff.Icon,
                                                AbilityType.Spell,
                                                UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                Helpers.minutesPerLevelDuration,
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionSpawnAreaEffect(desecrate_area, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes))),
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.CasterLevel),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.Evil),
                                                Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                Common.createAbilityAoERadius(20.Feet(), TargetType.Any)
                                                );
            desecrate.setMiscAbilityParametersRangedDirectional();
            desecrate.AvailableMetamagic = Metamagic.Extend | Metamagic.Quicken | Metamagic.Reach;

            desecrate.AddToSpellList(Helpers.clericSpellList, 2);
            desecrate.AddToSpellList(Helpers.inquisitorSpellList, 2);
            desecrate.AddSpellAndScroll("17959707c7004bd4abad2983f8a4af66"); //bane

        }


        static void createConsecreate()
        {
            var consecreate_buff = Helpers.CreateBuff("ConsecrateBuff",
                                          "Consecrate",
                                          "This spell blesses an area with positive energy. The DC to resist positive channeled energy within this area gains a +3 sacred bonus. Every undead creature entering a consecrated area suffers minor disruption, suffering a -1 penalty on attack rolls, damage rolls, and saves.\n"
                                          + "Undead cannot be created within or summoned into a consecrated area.",
                                          "",
                                          Helpers.GetIcon("db0f61cd985ca09498cafde9a4b27a16"),
                                          null
                                          );
            ChannelEnergyEngine.registerConsecrate(consecreate_buff);
            var consecreate_undead_buff = Helpers.CreateBuff("ConsecrateUndeadBuff",
                                                              consecreate_buff.Name,
                                                              consecreate_buff.Description,
                                                              "",
                                                              Helpers.GetIcon("db0f61cd985ca09498cafde9a4b27a16"),
                                                              null,
                                                              Helpers.CreateAddStatBonus(StatType.AdditionalAttackBonus, -1, ModifierDescriptor.None),
                                                              Helpers.CreateAddStatBonus(StatType.AdditionalDamage, -1, ModifierDescriptor.None),
                                                              Helpers.CreateAddStatBonus(StatType.SaveFortitude, -1, ModifierDescriptor.None),
                                                              Helpers.CreateAddStatBonus(StatType.SaveReflex, -1, ModifierDescriptor.None),
                                                              Helpers.CreateAddStatBonus(StatType.SaveWill, -1, ModifierDescriptor.None)
                                                              );
            consecreate_undead_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            consecrate_area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("c08bd33a377d5014a81be94e33ec8ce4", "ConsecrateArea", "");
            consecrate_area.Size = 20.Feet();
            consecrate_area.Fx = Common.createPrefabLink("bbd6decdae32bce41ae8f06c6c5eb893"); //holy holy aoe
            consecrate_area.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.Create<AbilityAreaEffectBuff>(a => {a.Buff = consecreate_buff; a.Condition = Helpers.CreateConditionsCheckerOr(); }),
                Helpers.Create<AbilityAreaEffectBuff>(a => {a.Buff = consecreate_undead_buff; a.Condition = Helpers.CreateConditionsCheckerOr(Common.createContextConditionHasFact(Common.undead)); })
            };

            consecrate = Helpers.CreateAbility("ConsecrateAbility",
                                                consecreate_buff.Name,
                                                consecreate_buff.Description,
                                                "",
                                                consecreate_buff.Icon,
                                                AbilityType.Spell,
                                                UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                Helpers.minutesPerLevelDuration,
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionSpawnAreaEffect(consecrate_area, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes))),
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.CasterLevel),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.Good),
                                                Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                Common.createAbilityAoERadius(20.Feet(), TargetType.Any)
                                                );
            consecrate.setMiscAbilityParametersRangedDirectional();
            consecrate.AvailableMetamagic = Metamagic.Extend | Metamagic.Quicken | Metamagic.Reach;

            consecrate.AddToSpellList(Helpers.clericSpellList, 2);
            consecrate.AddToSpellList(Helpers.inquisitorSpellList, 2);
            consecrate.AddSpellAndScroll("be452dba5acdd9441841d2189e1ae55a"); //bless

            var animate_dead = library.Get<BlueprintAbility>("4b76d32feb089ad4499c3a1ce8e1ac27");
            animate_dead.AddComponent(Common.createAbilityCasterHasNoFacts(consecreate_buff));
            var create_undead = library.Get<BlueprintAbility>("76a11b460be25e44ca85904d6806e5a3");
            foreach (var v in create_undead.Variants)
            {
                v.AddComponent(Common.createAbilityCasterHasNoFacts(consecreate_buff));
            }

            //fix jaethal ability
            var summon_undead = library.Get<BlueprintAbility>("4c1556984f24e5c4282c6fcda832b7b2");
            summon_undead.AddComponent(Common.createAbilityCasterHasNoFacts(consecreate_buff));
        }


        static void createAnimateDeadLesser()
        {
            var skeleton = library.CopyAndAdd<BlueprintUnit>("6c94133d39dea8544a591836c78eaaf3", "AnimatedSkeletonUnit", "aaf0f057ecff4fc0b8ad010be9fe4a97");
            
            skeleton.Body = skeleton.Body.CloneObject();
            skeleton.Body.PrimaryHand = null;
            skeleton.Body.Armor = null;
            skeleton.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("118fdd03e569a66459ab01a20af6811a");//claw 1d4
            skeleton.Faction = library.Get<BlueprintFaction>("1b08d9ed04518ec46a9b3e4e23cb5105"); //summoned
            skeleton.RemoveComponents<Experience>();
            skeleton.RemoveComponents<AddTags>();
            skeleton.Visual = library.Get<BlueprintUnit>("4050be4512d245f40bf9461074b672f4").Visual; //from summoned skeleton
            skeleton.StartingInventory = new Kingmaker.Blueprints.Items.BlueprintItem[0];
            skeleton.AddFacts = skeleton.AddFacts.AddToArray(library.Get<BlueprintFeature>("203992ef5b35c864390b4e4a1e200629"), //martial weapons
                                                             library.Get<BlueprintFeature>("cb8686e7357a68c42bdd9d4e65334633"), //shield prof
                                                             library.Get<BlueprintFeature>("6105f450bb2acbd458d277e71e19d835"), //tower shield prof
                                                             library.Get<BlueprintFeature>("6d3728d4e9c9898458fe5e9532951132"), //ligth armor prof
                                                             library.Get<BlueprintFeature>("46f4fb320f35704488ba3d513397789d"), //medium armor prof
                                                             library.Get<BlueprintFeature>("1b0f68188dcc435429fb87a022239681")); //shield prof

            var exotic_weapon_prof = library.Get<BlueprintFeatureSelection>("9a01b6815d6c3684cb25f30b8bf20932");
            foreach (var wp in exotic_weapon_prof.AllFeatures)
            {
                skeleton.AddFacts = skeleton.AddFacts.AddToArray(wp);
            }


            animate_dead_skeleton = skeleton;

            var animate_dead = library.Get<BlueprintAbility>("4b76d32feb089ad4499c3a1ce8e1ac27");
            var after_spawn = (animate_dead.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionSpawnMonster).AfterSpawn;

            var icon = Helpers.GetIcon("4b76d32feb089ad4499c3a1ce8e1ac27");


            var animate_action = Helpers.Create<DeadTargetMechanics.ContextActionAnimateDead>(a =>
                                                                                               {
                                                                                                   a.Blueprint = skeleton;
                                                                                                   a.AfterSpawn = after_spawn;
                                                                                                   a.dex_bonus = 2;
                                                                                                   a.DurationValue = Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.TenMinutes);
                                                                                                   a.transfer_equipment = true;
                                                                                                   a.SummonPool = Common.animate_dead_summon_pool;
                                                                                               }
                                                                                               );
            animate_dead.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(Helpers.CreateConditional(Helpers.Create<DeadTargetMechanics.ContextConditionCanBeAnimated>(), 
                                                                                                                         animate_action)));
            animate_dead.LocalizedDuration = Helpers.CreateString("AnimateDead.Description", Helpers.tenMinPerLevelDuration);
            animate_dead.setMiscAbilityParametersRangedDirectional();
            animate_dead.SetDescription("This spell turns corpses into undead skeletons that obey your spoken commands.\n"
                                        + "They remain animated until they are destroyed. A destroyed skeleton can’t be animated again.\n"
                                        + "You can’t create an undead with more HD than twice your caster level.\n"
                                        + "The undead you create remain under your control. No matter how many times you use this spell, however, you can control only 4 HD worth of undead creatures per caster level."
                                        );
            animate_dead.AddComponent(Helpers.CreateAbilityTargetsAround(10.Feet(), TargetType.Any, includeDead: true));
            animate_dead_lesser = Helpers.CreateAbility("AniamteDeadLesserAbility",
                                                        "Animate Dead, Lesser",
                                                        "This spell functions as animate dead, except you can only create a single Small or Medium skeleton.\n"
                                                        + "Animate Dead: " + animate_dead.Description,
                                                        "",
                                                        icon,
                                                        AbilityType.Spell,
                                                        UnitCommand.CommandType.Standard,
                                                        AbilityRange.Close,
                                                        Helpers.tenMinPerLevelDuration,
                                                        "",
                                                        Helpers.CreateRunActions(animate_action),
                                                        Helpers.CreateContextRankConfig(),
                                                        Helpers.Create<DeadTargetMechanics.AbilityTargetCanBeAnimated>(a => a.max_size = Size.Medium),
                                                        Helpers.CreateSpellDescriptor(SpellDescriptor.Evil),
                                                        Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                        Common.createAbilityCasterHasNoFacts(ChannelEnergyEngine.consecrate_buff)
                                                        );
            animate_dead_lesser.setMiscAbilityParametersSingleTargetRangedHarmful();
            animate_dead_lesser.AddToSpellList(Helpers.clericSpellList, 2);
            animate_dead_lesser.AddToSpellList(Helpers.wizardSpellList, 3);
            animate_dead.MaterialComponent = library.Get<BlueprintAbility>("7c5d556b9a5883048bf030e20daebe31").MaterialComponent.CloneObject();
            animate_dead.MaterialComponent.Count = 6;
            animate_dead_lesser.MaterialComponent = library.Get<BlueprintAbility>("c66e86905f7606c4eaa5c774f0357b2b").MaterialComponent.CloneObject();
            animate_dead_lesser.MaterialComponent.Count = 3;
            animate_dead_lesser.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken | Metamagic.Extend;
            //fix jaethal ability
            var summon_undead = library.Get<BlueprintAbility>("4c1556984f24e5c4282c6fcda832b7b2");
            summon_undead.LocalizedDuration = animate_dead.LocalizedDuration;
            summon_undead.SetDescription(animate_dead.Description);
            summon_undead.ReplaceComponent<AbilityEffectRunAction>(animate_dead.GetComponent<AbilityEffectRunAction>());
            summon_undead.AddComponent(animate_dead.GetComponent<AbilityTargetsAround>()); 
            var summon_undead_feature = library.Get<BlueprintFeature>("f06f246950e76864fa545c13cb334ba5");
            summon_undead_feature.SetDescription("Jaethal now can cast animate dead spell three times per day.");

        }


        static void createIntellectFortress()
        {
            var icon = Helpers.GetIcon("183d5bb91dea3a1489a6db6c9cb64445");

            var effect_buff = Helpers.CreateBuff("IntellectFortressEffectBuff",
                                                 "Intellect Fortress Effect",
                                                 "Using the power of pure logic, you disrupt mental attacks. Intellect fortress suppresses all effects within 20 feet radius with the emotion and fear descriptors for its duration.",
                                                 "",
                                                 icon,
                                                 Common.createPrefabLink("d541609d507424640a84153b89abf210"), //remove fear
                                                 Helpers.Create<BuffMechanics.SuppressBuffsCorrect>(s => s.Descriptor = SpellDescriptor.Fear | SpellDescriptor.Emotion | SpellDescriptor.NegativeEmotion)
                                                 );


            var buff = Common.createBuffAreaEffect(effect_buff, 20.Feet(), Helpers.CreateConditionsCheckerOr());
            buff.SetNameDescriptionIcon("Intellect Fortress",
                                        effect_buff.Description,
                                        effect_buff.Icon);
            buff.SetBuffFlags(0);
            buff.GetComponent<AddAreaEffect>().AreaEffect.Fx = Common.createPrefabLink("63f322580ec0e7c4c96fc62ecabad40f"); //axiomatic aoe
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(2), is_from_spell: true);
            intellect_fortress = Helpers.CreateAbility("IntellectFortressAbility",
                                          buff.Name,
                                          buff.Description,
                                          "",
                                          buff.Icon,
                                          AbilityType.Spell,
                                          UnitCommand.CommandType.Swift,
                                          AbilityRange.Personal,
                                          "2 rounds",
                                          "",
                                          Helpers.CreateRunActions(apply_buff),
                                          Helpers.CreateSpellComponent(SpellSchool.Abjuration),
                                          Common.createAbilitySpawnFx("c4d861e816edd6f4eab73c55a18fdadd", anchor: AbilitySpawnFxAnchor.SelectedTarget)//abjuration buff
                                          );
            intellect_fortress.setMiscAbilityParametersSelfOnly();
            intellect_fortress.AvailableMetamagic = Metamagic.Heighten | (Metamagic)MetamagicFeats.MetamagicExtender.ExtraRoundDuration;
            Helpers.AddSpell(intellect_fortress);
        }


        static void createMentalBarrier()
        {
            var icon = Helpers.GetIcon("62888999171921e4dafb46de83f4d67d"); //shield of dawn

            for (int i = 0; i < 5; i++)
            {
                var description = $"You put a barrier of mental energy that protects you from harm.\nThis barrier grants you a +{Math.Min(8, 4 + 2 * i)} shield bonus to AC.";
                if (i >=3)
                {
                    description += $"\nIf you are struck by a critical hit or sneak attack, there is a {(i - 2)*25}% chance that the additional damage is negated. This does not stack with similar effects that negate the additional damage from a critical hit or sneak attack.";
                }
                var buff = Helpers.CreateBuff($"MentalBarrier{i + 1}Buff",
                                              "Mental Barrier " + Common.roman_id[i+1],
                                              description,
                                              "",
                                              icon,
                                              null,
                                              Helpers.CreateAddStatBonus(StatType.AC, Math.Min(8, 4 + 2 * i), ModifierDescriptor.Shield) 
                                              );

                if (i >=3)
                {
                    buff.AddComponent(Common.createAddFortification((i - 2) * 25));
                }

                var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(2), is_from_spell: true);

                mental_barrier[i] = Helpers.CreateAbility($"MentalBarrier{i + 1}Ability",
                                                          buff.Name,
                                                          buff.Description,
                                                          "",
                                                          buff.Icon,
                                                          AbilityType.Spell,
                                                          UnitCommand.CommandType.Swift,
                                                          AbilityRange.Personal,
                                                          "2 rounds",
                                                          "",
                                                          Helpers.CreateRunActions(apply_buff),
                                                          Helpers.CreateSpellComponent(SpellSchool.Abjuration),
                                                          Common.createAbilitySpawnFx("6bebb42fd1a2ab9499d19f7a1dce3359", anchor: AbilitySpawnFxAnchor.SelectedTarget)//shield of faith
                                                          );
                mental_barrier[i].setMiscAbilityParametersSelfOnly();
                mental_barrier[i].AvailableMetamagic = Metamagic.Heighten | (Metamagic)MetamagicFeats.MetamagicExtender.ExtraRoundDuration;
                Helpers.AddSpell(mental_barrier[i]);
            }

            for (int i = 0; i < mental_barrier.Length; i++)
            {
                mental_barrier[i].AddComponent(Helpers.Create<SpellbookMechanics.SpellUndercast>(s =>
                {
                    s.undercast_abilities = mental_barrier.Take(i).ToArray();
                    s.overcast_abilities = mental_barrier.Skip(i + 1).ToArray();
                })
                );
                mental_barrier[i].SetDescription(mental_barrier[i].Description + (i == 0 ? "" : "\nThis spell can be undercast."));
                Helpers.AddSpell(mental_barrier[i]);
            }
        }


        static void createThoughtShield()
        {
            var icon = Helpers.GetIcon("eabf94e4edc6e714cabd96aa69f8b207");

            for (int i  = 0; i < 3; i++)
            {
                var buff = Helpers.CreateBuff($"ThoughtShield{i + 1}Buff",
                                              "Thought Shield " + Common.roman_id[i+1],
                                              $"Sensing an intrusion, you throw up a defense to protect your mind from attack or analysis. This grants you a +{4 + 2 * i} circumstance bonus on throws against mind-affecting effects.",
                                              "",
                                              icon,
                                              null,
                                              Common.createSavingThrowBonusAgainstDescriptor(4 + 2 * i, ModifierDescriptor.Circumstance, SpellDescriptor.MindAffecting)
                                              );

                var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(2), is_from_spell: true);

                thought_shield[i] = Helpers.CreateAbility($"ThoughtShield{i + 1}Ability",
                                                          buff.Name,
                                                          buff.Description,
                                                          "",
                                                          buff.Icon,
                                                          AbilityType.Spell,
                                                          UnitCommand.CommandType.Swift,
                                                          AbilityRange.Personal,
                                                          "2 rounds",
                                                          "",
                                                          Helpers.CreateRunActions(apply_buff),
                                                          Helpers.CreateSpellComponent(SpellSchool.Abjuration),
                                                          Common.createAbilitySpawnFx("c4d861e816edd6f4eab73c55a18fdadd", anchor: AbilitySpawnFxAnchor.SelectedTarget)//abjuration buff
                                                          );
                thought_shield[i].setMiscAbilityParametersSelfOnly();
                thought_shield[i].AvailableMetamagic = Metamagic.Heighten | (Metamagic)MetamagicFeats.MetamagicExtender.ExtraRoundDuration;
                Helpers.AddSpell(thought_shield[i]);

            }

            for (int i = 0; i < thought_shield.Length; i++)
            {
                thought_shield[i].AddComponent(Helpers.Create<SpellbookMechanics.SpellUndercast>(s =>
                {
                    s.undercast_abilities = thought_shield.Take(i).ToArray();
                    s.overcast_abilities = thought_shield.Skip(i + 1).ToArray();
                })
                );
                thought_shield[i].SetDescription(thought_shield[i].Description + (i == 0 ? "" : "\nThis spell can be undercast."));
                Helpers.AddSpell(thought_shield[i]);
            }
        }


        static void createMindThrust()
        {
            var check_intelligent = Common.createAbilityTargetHasFact(true, Common.construct, Common.vermin);

            var icon = Helpers.GetIcon("dd2a5a6e76611c04e9eac6254fcf8c6b");
            var fatigued = library.Get<BlueprintBuff>("e6f2fc5d73d88064583cb828801212f4");
            var exhausted = library.Get<BlueprintBuff>("46d1b9cc3d0fd36469a471b047d773a2");
            var stunned = library.Get<BlueprintBuff>("09d39b38bb7c6014394b6daced9bacd3");

            var apply_fatigued = Common.createContextActionApplyBuff(fatigued, Helpers.CreateContextDuration(1));
            var apply_exhausted = Common.createContextActionApplyBuff(exhausted, Helpers.CreateContextDuration(1));
            var apply_stunned = Common.createContextActionApplyBuff(stunned, Helpers.CreateContextDuration(1));

            mind_thrust[0] = Helpers.CreateAbility("MindThrust1Ability",
                                                   "Mind Thrust I",
                                                   $"You divine the most vulnerable portions of your opponent’s mind and overload it with a glut of psychic information.\nThis attack deals 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage per caster level (maximum 5d{BalanceFixes.getDamageDieString(DiceType.D6)}). The target receives a Will save for half damage.",
                                                   "",
                                                   icon,
                                                   AbilityType.Spell,
                                                   UnitCommand.CommandType.Standard,
                                                   AbilityRange.Close,
                                                   "",
                                                   "Will half",
                                                   Helpers.CreateRunActions(SavingThrowType.Will,
                                                                            Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.Default), 0), false, true)
                                                                            ),
                                                   Helpers.CreateContextRankConfig(max: 5, feature: MetamagicFeats.intensified_metamagic),
                                                   Common.createAbilitySpawnFx("c388856d0e8855f429a83ccba67944ba", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                   Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting),
                                                   Helpers.CreateSpellComponent(SpellSchool.Divination),
                                                   check_intelligent
                                                   );
            mind_thrust[0].setMiscAbilityParametersSingleTargetRangedHarmful(true);
            mind_thrust[0].SpellResistance = true;
            mind_thrust[0].AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Reach | Metamagic.Heighten | (Metamagic)MetamagicFeats.MetamagicExtender.IntensifiedGeneral | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            Helpers.AddSpell(mind_thrust[0]);


            mind_thrust[1] = Helpers.CreateAbility("MindThrust2Ability",
                                       "Mind Thrust II",
                                       $"This functions as mind thrust I, but the target takes 1d{BalanceFixes.getDamageDieString(DiceType.D8)} points of damage per caster level (maximum 5d{BalanceFixes.getDamageDieString(DiceType.D8)}).",
                                       "",
                                       icon,
                                       AbilityType.Spell,
                                       UnitCommand.CommandType.Standard,
                                       AbilityRange.Close,
                                       "",
                                       "Will half",
                                       Helpers.CreateRunActions(SavingThrowType.Will,
                                                                Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D8), Helpers.CreateContextValue(AbilityRankType.Default), 0), false, true)
                                                                ),
                                       Helpers.CreateContextRankConfig(max: 5, feature: MetamagicFeats.intensified_metamagic),
                                       Common.createAbilitySpawnFx("c388856d0e8855f429a83ccba67944ba", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                       Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting),
                                       Helpers.CreateSpellComponent(SpellSchool.Divination),
                                       check_intelligent
                                       );
            mind_thrust[1].setMiscAbilityParametersSingleTargetRangedHarmful(true);
            mind_thrust[1].SpellResistance = true;
            mind_thrust[1].AvailableMetamagic = mind_thrust[0].AvailableMetamagic;
            Helpers.AddSpell(mind_thrust[1]);

            mind_thrust[2] = Helpers.CreateAbility("MindThrust3Ability",
                           "Mind Thrust III",
                           $"This functions as mind thrust I, but the target takes 1d{BalanceFixes.getDamageDieString(DiceType.D8)} points of damage per caster level (maximum 10d{BalanceFixes.getDamageDieString(DiceType.D8)}).",
                           "",
                           icon,
                           AbilityType.Spell,
                           UnitCommand.CommandType.Standard,
                           AbilityRange.Close,
                           "",
                           "Will half",
                           Helpers.CreateRunActions(SavingThrowType.Will,
                                                    Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D8), Helpers.CreateContextValue(AbilityRankType.Default), 0), false, true)
                                                    ),
                           Helpers.CreateContextRankConfig(max: 10, feature: MetamagicFeats.intensified_metamagic),
                           Common.createAbilitySpawnFx("c388856d0e8855f429a83ccba67944ba", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                           Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting),
                           Helpers.CreateSpellComponent(SpellSchool.Divination),
                           check_intelligent
                           );
            mind_thrust[2].setMiscAbilityParametersSingleTargetRangedHarmful(true);
            mind_thrust[2].SpellResistance = true;
            mind_thrust[2].AvailableMetamagic = mind_thrust[0].AvailableMetamagic;
            Helpers.AddSpell(mind_thrust[2]);

            mind_thrust[3] = Helpers.CreateAbility("MindThrust4Ability",
                                                   "Mind Thrust IV",
                                                   $"This functions as mind thrust I, but the target takes 1d{BalanceFixes.getDamageDieString(DiceType.D8)} points of damage per caster level (maximum 15d{BalanceFixes.getDamageDieString(DiceType.D8)}) and is fatigued for 1 round if it fails its save.",
                                                   "",
                                                   icon,
                                                   AbilityType.Spell,
                                                   UnitCommand.CommandType.Standard,
                                                   AbilityRange.Close,
                                                   "",
                                                   "Will half",
                                                   Helpers.CreateRunActions(SavingThrowType.Will,
                                                                            Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D8), Helpers.CreateContextValue(AbilityRankType.Default), 0), false, true),
                                                                            Helpers.CreateConditionalSaved(null, apply_fatigued)                                                                            
                                                                            ),
                                                   Helpers.CreateContextRankConfig(max: 15, feature: MetamagicFeats.intensified_metamagic),
                                                   Common.createAbilitySpawnFx("c388856d0e8855f429a83ccba67944ba", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                   Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting),
                                                   Helpers.CreateSpellComponent(SpellSchool.Divination),
                                                   check_intelligent
                                                   );
            mind_thrust[3].setMiscAbilityParametersSingleTargetRangedHarmful(true);
            mind_thrust[3].SpellResistance = true;
            mind_thrust[3].AvailableMetamagic = mind_thrust[0].AvailableMetamagic;
            Helpers.AddSpell(mind_thrust[3]);

            mind_thrust[4] = Helpers.CreateAbility("MindThrust5Ability",
                                       "Mind Thrust V",
                                       $"This functions as mind thrust IV, but the target is also exhausted for 1 round if it fails its save and fatigued for 1 round if it succeeds at its save.",
                                       "",
                                       icon,
                                       AbilityType.Spell,
                                       UnitCommand.CommandType.Standard,
                                       AbilityRange.Close,
                                       "",
                                       "Will partial",
                                       Helpers.CreateRunActions(SavingThrowType.Will,
                                                                Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D8), Helpers.CreateContextValue(AbilityRankType.Default), 0), false, true),
                                                                Helpers.CreateConditionalSaved(apply_fatigued, apply_exhausted)
                                                                ),
                                       Helpers.CreateContextRankConfig(max: 15, feature: MetamagicFeats.intensified_metamagic),
                                       Common.createAbilitySpawnFx("c388856d0e8855f429a83ccba67944ba", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                       Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting),
                                       Helpers.CreateSpellComponent(SpellSchool.Divination),
                                       check_intelligent
                                       );
            mind_thrust[4].setMiscAbilityParametersSingleTargetRangedHarmful(true);
            mind_thrust[4].SpellResistance = true;
            mind_thrust[4].AvailableMetamagic = mind_thrust[0].AvailableMetamagic;
            Helpers.AddSpell(mind_thrust[4]);

            mind_thrust[5] = Helpers.CreateAbility("MindThrust6Ability",
                           "Mind Thrust VI",
                           $"This functions as mind thrust V, but the target takes 1d{BalanceFixes.getDamageDieString(DiceType.D8)} points of damage per caster level (maximum 20d{BalanceFixes.getDamageDieString(DiceType.D8)}) and is exhausted and stunned for 1 round if it fails its save.",
                           "",
                           icon,
                           AbilityType.Spell,
                           UnitCommand.CommandType.Standard,
                           AbilityRange.Close,
                           "",
                           "Will partial",
                           Helpers.CreateRunActions(SavingThrowType.Will,
                                                    Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D8), Helpers.CreateContextValue(AbilityRankType.Default), 0), false, true),
                                                    Helpers.CreateConditionalSaved(new GameAction[] { apply_fatigued }, new GameAction[] { apply_stunned, apply_exhausted })
                                                    ),
                           Helpers.CreateContextRankConfig(max: 20, feature: MetamagicFeats.intensified_metamagic),
                           Common.createAbilitySpawnFx("c388856d0e8855f429a83ccba67944ba", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                           Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting),
                           Helpers.CreateSpellComponent(SpellSchool.Divination),
                           check_intelligent
                           );
            mind_thrust[5].setMiscAbilityParametersSingleTargetRangedHarmful(true);
            mind_thrust[5].SpellResistance = true;
            mind_thrust[5].AvailableMetamagic = mind_thrust[0].AvailableMetamagic;
            Helpers.AddSpell(mind_thrust[5]);

            for (int i = 0; i < mind_thrust.Length; i++)
            {
                mind_thrust[i].AddComponent(Helpers.Create<SpellbookMechanics.SpellUndercast>(s =>
                {
                    s.undercast_abilities = mind_thrust.Take(i).ToArray();
                    s.overcast_abilities = mind_thrust.Skip(i + 1).ToArray();
                })
                );
                mind_thrust[i].SetDescription(mind_thrust[i].Description + (i == 0 ? "" : "\nThis spell can be undercast."));
                Helpers.AddSpell(mind_thrust[i]);
            }
        }


        static void createBloodyClaws()
        {
            var icon = Helpers.GetIcon("4a51dca9d9456214e9a382b9e47385b3"); //abyssal claws
            var bleed1d6 = library.Get<BlueprintBuff>("75039846c3d85d940aa96c249b97e562");
            var effect_buff = Helpers.CreateBuff("BloodyClawsEffectBuff",
                                          "Bloody Claws Effect",
                                          "You give a creature the ability to deal bleed damage when making natural attacks so long as the attack deals slashing or piercing damage. This bleed damage for each attack is equal to one-half your caster level (limited to the creature’s maximum damage with that attack), though bleed damage does not stack. When two or more attacks deal bleed damage, take the worse effect.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.Create<BleedMechanics.BleedBuff>(b => b.dice_value = Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilityRankType.Default))),
                                          bleed1d6.GetComponent<SpellDescriptorComponent>(),
                                          bleed1d6.GetComponent<AddHealTrigger>(),
                                          bleed1d6.GetComponent<CombatStateTrigger>(),
                                          Helpers.CreateContextRankConfig(progression: ContextRankProgression.Div2)
                                          );

            var apply_effect_buff = Common.createContextActionApplyBuff(effect_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true);
            var on_hit = Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(apply_effect_buff));
            on_hit.AllNaturalAndUnarmed = true;

            var buff = Helpers.CreateBuff("BloodyClawsBuff",
                                          "Bloody Claws",
                                          effect_buff.Description,
                                          "",
                                          effect_buff.Icon,
                                          library.Get<BlueprintBuff>("da8ce41ac3cd74742b80984ccc3c9613").FxOnStart,
                                          on_hit);

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), is_from_spell: true);

            bloody_claws = Helpers.CreateAbility("BloodyClawsAbility",
                                                       buff.Name,
                                                       buff.Description,
                                                       "",
                                                       buff.Icon,
                                                       AbilityType.Spell,
                                                       UnitCommand.CommandType.Standard,
                                                       AbilityRange.Touch,
                                                       Helpers.minutesPerLevelDuration,
                                                       Helpers.willNegates,
                                                       Helpers.CreateRunActions(apply_buff),
                                                       Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                       Common.createAbilityTargetHasFact(true, Common.undead),
                                                       Common.createAbilityTargetHasFact(true, Common.construct),
                                                       Common.createAbilityTargetHasFact(true, Common.elemental),
                                                       Helpers.CreateContextRankConfig()
                                                       );
            bloody_claws.setMiscAbilityParametersTouchFriendly();
            bloody_claws.AvailableMetamagic = Metamagic.Extend | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;
            bloody_claws.AddToSpellList(Helpers.druidSpellList, 4);
            bloody_claws.AddToSpellList(Helpers.rangerSpellList, 3);

            bloody_claws.AddSpellAndScroll("d1d24c5613bb8c14a9a089c54b77527d");
        }


        static void createTouchOfBloodletting()
        {
            var icon = Helpers.GetIcon("6cbb040023868574b992677885390f92"); //vampyric touch
            var exhausted = library.Get<BlueprintBuff>("46d1b9cc3d0fd36469a471b047d773a2");
            var bleed1d6 = library.Get<BlueprintBuff>("75039846c3d85d940aa96c249b97e562");
            var buff = Helpers.CreateBuff("TouchOfBloodlettingBuff",
                                          "Touch of Bloodletting",
                                          "This spell causes any existing wounds that the target possesses to bleed profusely. If the creature’s current total hit points are less than its maximum, this spell causes the creature to take 1 point of bleed damage each round and become exhausted for the duration of the spell. A successful DC 15 Heal check or any spell that cures hit point damage negates the effects of this spell.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.Create<BleedMechanics.BleedBuff>(b => b.dice_value = Helpers.CreateContextDiceValue(DiceType.Zero, 0, 1)),
                                          bleed1d6.GetComponent<SpellDescriptorComponent>(),
                                          bleed1d6.GetComponent<AddHealTrigger>()
                                          );

            buff.AddComponents(exhausted.ComponentsArray);

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Rounds), is_from_spell: true);

            var touch_of_blood_letting_touch = Helpers.CreateAbility("TouchOfBloodlettingAbility",
                                                                   buff.Name,
                                                                   buff.Description,
                                                                   "",
                                                                   buff.Icon,
                                                                   AbilityType.Spell,
                                                                   UnitCommand.CommandType.Standard,
                                                                   AbilityRange.Touch,
                                                                   Helpers.roundsPerLevelDuration,
                                                                   Helpers.willNegates,
                                                                   Helpers.CreateRunActions(SavingThrowType.Will, Helpers.CreateConditionalSaved(null, apply_buff)),
                                                                   Helpers.CreateSpellDescriptor(SpellDescriptor.Bleed | SpellDescriptor.Exhausted),
                                                                   Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                                   Common.createAbilityTargetHasFact(true, Common.undead),
                                                                   Common.createAbilityTargetHasFact(true, Common.construct),
                                                                   Common.createAbilityTargetHasFact(true, Common.elemental),
                                                                   Helpers.Create<NewMechanics.AbilityTargetWounded>(),
                                                                   Helpers.CreateDeliverTouch(),
                                                                   Common.createAbilitySpawnFx("cbfe312cb8e63e240a859efaad8e467c", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                                   Helpers.CreateContextRankConfig()
                                                                   );
            touch_of_blood_letting_touch.setMiscAbilityParametersTouchHarmful();
            touch_of_blood_letting_touch.SpellResistance = true;
            touch_of_blood_letting_touch.AvailableMetamagic = Metamagic.Extend | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            touch_of_blood_letting = touch_of_blood_letting_touch.CreateTouchSpellCast();
            touch_of_blood_letting.AddComponents(Common.createAbilityTargetHasFact(true, Common.undead),
                                                 Common.createAbilityTargetHasFact(true, Common.construct),
                                                 Common.createAbilityTargetHasFact(true, Common.elemental),
                                                 Helpers.Create<NewMechanics.AbilityTargetWounded>());

            touch_of_blood_letting.AddToSpellList(Helpers.clericSpellList, 1);
            touch_of_blood_letting.AddToSpellList(Helpers.druidSpellList, 1);
            touch_of_blood_letting.AddToSpellList(Helpers.wizardSpellList, 2);

            touch_of_blood_letting.AddSpellAndScroll("b693e8cc94f4477498d396a40817ff77");
        }


        static void createHaltUndead()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/HaltUndead.png");

            var buff = library.CopyAndAdd<BlueprintBuff>("11cb2fe4fe9c44b448cfe1788ae1ab59", "HaltUndeadBuff", "");
            buff.SetNameDescriptionIcon("Halt Undead",
                                        "This spell renders all undead creatures within its radius immobile. If the spell is successful, it renders the undead creature immobile for the duration of the spell (similar to the effect of hold person on a living creature). The effect is broken if the halted creatures are attacked or take damage.",
                                        icon);
            buff.ReplaceComponent<SpellDescriptorComponent>(s => s.Descriptor = SpellDescriptor.MovementImpairing);
            buff.AddComponent(Common.createIncomingDamageTrigger(Helpers.Create<ContextActionRemoveSelf>()));

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Rounds), is_from_spell: true);

            var effect = Helpers.CreateConditional(Common.createContextConditionHasFact(Common.undead), Helpers.CreateConditionalSaved(null, apply_buff));
            halt_undead = Helpers.CreateAbility("HaltUndeadAbility",
                                                   buff.Name,
                                                   buff.Description,
                                                   "",
                                                   buff.Icon,
                                                   AbilityType.Spell,
                                                   UnitCommand.CommandType.Standard,
                                                   AbilityRange.Medium,
                                                   Helpers.roundsPerLevelDuration,
                                                   Helpers.willNegates,
                                                   Helpers.CreateRunActions(SavingThrowType.Will, effect),
                                                   Helpers.CreateAbilityTargetsAround(10.Feet(), TargetType.Enemy),
                                                   Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                   Helpers.CreateContextRankConfig(),
                                                   Helpers.CreateSpellDescriptor(SpellDescriptor.MovementImpairing)
                                                   );
            halt_undead.setMiscAbilityParametersRangedDirectional();
            halt_undead.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach | Metamagic.Extend | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            halt_undead.SpellResistance = true;
            halt_undead.AddToSpellList(Helpers.wizardSpellList, 3);
            halt_undead.AddToSpellList(Helpers.inquisitorSpellList, 3);
            halt_undead.AddSpellAndScroll("4f71bf5971c5f3c46a66e5893a1b1e83"); //life blast
        }



        static void createControlUndead()
        {
            var icon = Helpers.GetIcon("1a5e7191279e7cd479b17a6ca438498c"); //undead arcana

            var buff = library.CopyAndAdd<BlueprintBuff>("c0f4e1c24c9cd334ca988ed1bd9d201f", "ControlUndeadBuff", "");
            buff.SetNameDescriptionIcon("Control Undead",
                                        "This spell enables you to control undead creature for a short period of time. You command it by voice and it understands you, no matter what language you speak. Even if vocal communication is impossible, the controlled undead does not attack you. At the end of the spell, the subject reverts to its normal behavior.",
                                        icon);
            buff.RemoveComponents<SpellDescriptorComponent>();

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), is_from_spell: true);

            var effect = Helpers.CreateConditionalSaved(null, apply_buff);
            control_undead = Helpers.CreateAbility("ControlUndeadAbility",
                                                   buff.Name,
                                                   buff.Description,
                                                   "",
                                                   buff.Icon,
                                                   AbilityType.Spell,
                                                   UnitCommand.CommandType.Standard,
                                                   AbilityRange.Close,
                                                   Helpers.minutesPerLevelDuration,
                                                   Helpers.willNegates,
                                                   Helpers.CreateRunActions(SavingThrowType.Will, effect),
                                                   Common.createAbilityTargetHasFact(false, Common.undead),
                                                   Common.createAbilityTargetHasFact(true, library.Get<BlueprintBuff>("8728e884eeaa8b047be04197ecf1a0e4")),
                                                   Common.createAbilitySpawnFx("09f795c3900b21b47a1254bcb3f263c8", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                   Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                   Helpers.CreateContextRankConfig()
                                                   );
            control_undead.setMiscAbilityParametersSingleTargetRangedHarmful();
            control_undead.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach | Metamagic.Extend | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            control_undead.SpellResistance = true;
            control_undead.AddToSpellList(Helpers.wizardSpellList, 7);
            control_undead.AddSpellAndScroll("bc0180b8b29abf9468dea1a24332d159");
        }


        static void createSleetStorm()
        {
            var invisibility = library.Get<BlueprintBuff>("e6b35473a237a6045969253beb09777c");
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/SleetStorm.png");

            var buff = Helpers.CreateBuff("SleetStormBuff",
                              "Sleet Storm",
                              "Driving sleet blocks all sight (even darkvision) within it and causes the ground in the area to be icy. A creature can walk within or through the area of sleet at half normal speed with a DC 10 Acrobatics check. Failure means it can’t move in that round.\n"
                              + "Ranged attacks are impossible in the area.",
                              "",
                              icon,
                              null,
                              Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Blindness | SpellDescriptor.SightBased),
                              Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.DifficultTerrain),
                              Common.createBuffDescriptorImmunity(SpellDescriptor.SightBased),
                              Helpers.Create<AddConcealment>(c => { c.Concealment = Concealment.Total; c.Descriptor = ConcealmentDescriptor.Fog; }),
                              Helpers.Create<ConcealementMechanics.AddOutgoingConcealment>(c => { c.Concealment = Concealment.Total; c.Descriptor = ConcealmentDescriptor.Fog; })
                              );

            //buff.FxOnStart = invisibility.FxOnStart;
            //buff.FxOnRemove = invisibility.FxOnRemove;


            var can_not_move_buff = Helpers.CreateBuff("SleetStormCanNotMoveBuff",
                                                      "Sleet Storm (Can Not Move)",
                                                      "You can not move through the storm.",
                                                      "",
                                                      icon,
                                                      null,
                                                      Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.CantMove)
                                                      );

            can_not_move_buff.Stacking = StackingType.Replace;


            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("6ea87a0ff5df41c459d641326f9973d5", "SleetStormArea", "");

            var apply_can_not_move = Common.createContextActionApplyBuff(can_not_move_buff, Helpers.CreateContextDuration(1), dispellable: false, is_child: true);

            var check_movement = Helpers.CreateConditional(new Condition[]{Helpers.Create<CombatManeuverMechanics.ContextConditionTargetSizeLessOrEqual>(c => c.target_size = Size.Medium),
                                                                           Helpers.CreateConditionHasFact(immunity_to_wind, not: true)},
                                                           Common.createContextActionSkillCheck(StatType.SkillMobility,
                                                                                                failure: Helpers.CreateActionList(apply_can_not_move),
                                                                                                custom_dc: 10)
                                                           );

            var actions = Helpers.CreateActionList(check_movement);
            area.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.Create<NewMechanics.AbilityAreaEffectRunActionWithFirstRound>(a =>
                {
                    a.UnitEnter = actions;
                    a.Round = actions;
                    a.FirstRound = actions;
                }),
                Helpers.Create<AbilityAreaEffectBuff>(a => {a.Buff = buff; a.Condition = Helpers.CreateConditionsCheckerOr(Common.createContextConditionHasFact(immunity_to_wind, false)); }),
                Helpers.Create<AbilityAreaEffectBuff>(a => {a.Buff = ranged_attacks_forbidden_buff; a.Condition = Helpers.CreateConditionsCheckerOr(); })
            };


            var spawn_area = Common.createContextActionSpawnAreaEffect(area, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)));


            sleet_storm = Helpers.CreateAbility("SleetStormAbility",
                                                     buff.Name,
                                                     buff.Description,
                                                     "",
                                                     icon,
                                                     AbilityType.Spell,
                                                     UnitCommand.CommandType.Standard,
                                                     AbilityRange.Long,
                                                     Helpers.roundsPerLevelDuration,
                                                     "",
                                                     Helpers.CreateRunActions(spawn_area),
                                                     Common.createAbilityAoERadius(20.Feet(), TargetType.Any),
                                                     Helpers.CreateSpellComponent(SpellSchool.Conjuration),
                                                     Helpers.CreateContextRankConfig()
                                                     );
            sleet_storm.setMiscAbilityParametersRangedDirectional();
            sleet_storm.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken;

            sleet_storm.AddToSpellList(Helpers.druidSpellList, 3);
            sleet_storm.AddToSpellList(Helpers.magusSpellList, 3);
            sleet_storm.AddToSpellList(Helpers.wizardSpellList, 3);
            sleet_storm.AddSpellAndScroll("c17e4bd5028d6534a8c8d317cd8244ca");
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("c18a821ee662db0439fb873165da25be"), sleet_storm, 4); //instead of slowing mud for weather domain
        }


        static void createTemporalStasis()
        {
            var buff = Helpers.CreateBuff("TemporalStasisBuff",
                                          "Temporal Stasis",
                                          "You must succeed on a melee touch attack. You place the subject into a state of suspended animation. For the creature, time ceases to flow, and its condition becomes fixed. The creature does not grow older. Its body functions virtually cease, and no force or effect can harm it. This state persists until the spell ends.",
                                          "",
                                          LoadIcons.Image2Sprite.Create(@"AbilityIcons/TemporalStasis.png"),
                                          Common.createPrefabLink("eb0e36f1de0c05347963262d56d90cf5"), //hold person
                                          Helpers.Create<TImeStopMechanics.EraseFromTime>()
                                          );
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), is_from_spell: true);
            var temporal_stasis_touch = Helpers.CreateAbility("TemporalStasisAbility",
                                                                buff.Name,
                                                                buff.Description,
                                                                "",
                                                                buff.Icon,
                                                                AbilityType.Spell,
                                                                UnitCommand.CommandType.Standard,
                                                                AbilityRange.Touch,
                                                                Helpers.roundsPerLevelDuration,
                                                                Helpers.fortNegates,
                                                                Helpers.CreateRunActions(SavingThrowType.Fortitude, Helpers.CreateConditionalSaved(null, apply_buff)),
                                                                Helpers.CreateContextRankConfig(),
                                                                Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                                                Helpers.CreateDeliverTouch()
                                                                );
            temporal_stasis_touch.SpellResistance = true;
            temporal_stasis_touch.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            temporal_stasis_touch.setMiscAbilityParametersTouchHarmful();

            temporal_stasis = Helpers.CreateTouchSpellCast(temporal_stasis_touch);

            temporal_stasis.AddToSpellList(Helpers.wizardSpellList, 8);
            temporal_stasis.AddSpellAndScroll("4b2d0e65fb9775341b6c4f7c178f0fe5");
        }


        static void createTimeStop()
        {
            var buff = Helpers.CreateBuff("TimeStopTargetBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          Common.createPrefabLink("eb0e36f1de0c05347963262d56d90cf5"), //hold person
                                          Helpers.Create<TImeStopMechanics.EraseFromTime>()
                                          );
            buff.SetBuffFlags(BuffFlags.HiddenInUi);


            var caster_buff = Helpers.CreateBuff("TimeStopCasterBuff",
                                                 "",
                                                 "",
                                                 "",
                                                 null,
                                                 null
                                                 );
                                                 
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilitySharedValue.Duration)));
            var apply_caster_buff = Common.createContextActionApplyBuff(caster_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilitySharedValue.Duration)));



            time_stop = Helpers.CreateAbility("TimeStopAbility",
                                                "Time Stop",
                                                "This spell seems to make time cease to flow for everyone but you. In fact, you speed up so greatly that all other creatures seem frozen, though they are actually still moving at their normal speeds. You are free to act for 1d4+1 rounds of apparent time. Normal and magical fire, cold, gas, and the like can still harm you. While the time stop is in effect, other creatures are invulnerable to your attacks and spells; you cannot target such creatures with any attack or spell. A spell that affects an area and has a duration longer than the remaining duration of the time stop have their normal effects on other creatures once the time stop ends. Most spellcasters use the additional time to improve their defenses, summon allies, or flee from combat.\n"
                                                + "You cannot move or harm items held, carried, or worn by a creature stuck in normal time, but you can affect any item that is not in another creature’s possession.",
                                                "",
                                                LoadIcons.Image2Sprite.Create(@"AbilityIcons/TimeStop.png"),
                                                AbilityType.Spell,
                                                UnitCommand.CommandType.Standard,
                                                AbilityRange.Personal,
                                                "1d4+1 rounds (apparent time)",
                                                "",
                                                Helpers.CreateRunActions(apply_caster_buff, 
                                                                         Helpers.Create<NewMechanics.ApplyActionToAllUnits>(a => a.actions = Helpers.CreateActionList(apply_buff))
                                                                        ),
                                                Helpers.CreateCalculateSharedValue(Helpers.CreateContextDiceValue(DiceType.D4, 1, 2), sharedValue: AbilitySharedValue.Duration),
                                                Helpers.CreateSpellComponent(SpellSchool.Transmutation)
                                                );
            time_stop.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Empower | Metamagic.Maximize;
            time_stop.setMiscAbilityParametersSelfOnly();

            time_stop.AddToSpellList(Helpers.wizardSpellList, 9);
            time_stop.AddSpellAndScroll("4b2d0e65fb9775341b6c4f7c178f0fe5");
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("cc2d330bb0200e840aeb79140e770198"), time_stop, 9);
        }




        static void createSandsOfTime()
        {
            var icon = library.Get<BlueprintAbility>("55f14bc84d7c85446b07a1b5dd6b2b4c").Icon;

            var buff = Helpers.CreateBuff("SandsOfTimeBuff",
                                          "Sands of Time",
                                          "You temporarily age the target, immediately advancing it to the next age category. The target immediately takes the age penalties to Strength, Dexterity, and Constitution for its new age category, but does not gain the bonuses for that category. A creature whose age is unknown is treated as if the spell advances it to old age (-2 to Strength, Dexterity and Constitution). Ageless or immortal creatures are immune to this spell.\n"
                                          + $"If you cast this on an object, construct, or undead creature, it takes 3d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage + 1 point per caster level (maximum + 15) as time weathers and corrodes it. This version of the spell has an instantaneous duration.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.CreateAddStatBonus(StatType.Strength, -2, ModifierDescriptor.UntypedStackable),
                                          Helpers.CreateAddStatBonus(StatType.Dexterity, -2, ModifierDescriptor.UntypedStackable),
                                          Helpers.CreateAddStatBonus(StatType.Constitution, -2, ModifierDescriptor.UntypedStackable)
                                          );
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.TenMinutes), is_from_spell: true);
            var deal_damage = Helpers.CreateActionDealDamage(DamageEnergyType.Unholy, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 3, Helpers.CreateContextValue(AbilityRankType.Default)));

            var action = Helpers.CreateConditional(Common.createContextConditionHasFacts(all: false, Common.undead, Common.construct), deal_damage, apply_buff);
            var sands_of_time_touch = Helpers.CreateAbility("SandsOfTimeTouchAbility",
                                                  buff.Name,
                                                  buff.Description,
                                                  "",
                                                  buff.Icon,
                                                  AbilityType.Spell,
                                                  UnitCommand.CommandType.Standard,
                                                  AbilityRange.Touch,
                                                  Helpers.tenMinPerLevelDuration,
                                                  Helpers.savingThrowNone,
                                                  Helpers.CreateRunActions(action),
                                                  Common.createAbilityTargetHasFact(inverted: true, Common.elemental),
                                                  Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                  Common.createAbilitySpawnFx("cbfe312cb8e63e240a859efaad8e467c", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                  Helpers.CreateDeliverTouch()
                                                  );
            sands_of_time_touch.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach | Metamagic.Extend | Metamagic.Empower | Metamagic.Maximize | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            sands_of_time_touch.SpellResistance = true;
            sands_of_time_touch.setMiscAbilityParametersTouchHarmful();
            sands_of_time = Helpers.CreateTouchSpellCast(sands_of_time_touch);

            sands_of_time.AddToSpellList(Helpers.clericSpellList, 3);
            sands_of_time.AddToSpellList(Helpers.wizardSpellList, 3);
            sands_of_time.AddSpellAndScroll("a9a0d65ec202e25478bcae4a87e844f9"); //forced repentance
        }



        static void createThreefoldAspect()
        {
            var icon = library.Get<BlueprintAbility>("3e4ab69ada402d145a5e0ad3ad4b8564").Icon;

            var buff1 = Helpers.CreateBuff("ThreeFoldAspect1Buff",
                                           "Threefold Aspect: Young",
                                           "Threefold aspect allows you to shift your appearance between your natural age and three idealized age categories: young adult (youth/maiden), adulthood (father/mother), or elderly (elder/crone). In each case, your appearance is your own at the appropriate age, rather than that of a new individual.\n"
                                           + "You may change between these three aspects or your actual age as a standard action. As the young adult, you gain a +2 enhancement bonus to Dexterity and Constitution, but suffer a -2 penalty to Wisdom. In the adult aspect, you gain a +2 enhancement bonus to Wisdom and Intelligence, but take a -2 penalty to Dexterity. As the elderly aspect, you gain a +4 enhancement bonus to Wisdom and Intelligence, but take a -2 penalty to Strength and Dexterity. As enhancement bonuses, these stack with any bonuses or penalties you may have from your actual age (which are untyped bonuses)-the bonuses granted by this spell represent your idealized form in this threefold aspect rather than simply duplicating your ability scores at any one particular age.",
                                           "",
                                           icon,
                                           null,
                                           Helpers.CreateAddStatBonus(StatType.Dexterity, 2, ModifierDescriptor.Enhancement),
                                           Helpers.CreateAddStatBonus(StatType.Constitution, 2, ModifierDescriptor.Enhancement),
                                           Helpers.CreateAddStatBonus(StatType.Wisdom, -2, ModifierDescriptor.UntypedStackable)
                                           );
            var buff2 = Helpers.CreateBuff("ThreeFoldAspect2Buff",
                                           "Threefold Aspect: Adult",
                                            buff1.Description,
                                           "",
                                           icon,
                                           null,
                                           Helpers.CreateAddStatBonus(StatType.Wisdom, 2, ModifierDescriptor.Enhancement),
                                           Helpers.CreateAddStatBonus(StatType.Intelligence, 2, ModifierDescriptor.Enhancement),
                                           Helpers.CreateAddStatBonus(StatType.Dexterity, -2, ModifierDescriptor.UntypedStackable)
                                           );
            var buff3 = Helpers.CreateBuff("ThreeFoldAspect3Buff",
                                           "Threefold Aspect: Elder",
                                           buff1.Description,
                                           "",
                                           icon,
                                           null,
                                           Helpers.CreateAddStatBonus(StatType.Wisdom, 4, ModifierDescriptor.Enhancement),
                                           Helpers.CreateAddStatBonus(StatType.Intelligence, 4, ModifierDescriptor.Enhancement),
                                           Helpers.CreateAddStatBonus(StatType.Strength, -2, ModifierDescriptor.UntypedStackable),
                                           Helpers.CreateAddStatBonus(StatType.Dexterity, -2, ModifierDescriptor.UntypedStackable)
                                           );

            var buffs = new BlueprintBuff[] { buff1, buff2, buff3 };
            var abilities = new BlueprintActivatableAbility[3];

            for (int i = 0; i < buffs.Length; i++)
            {
                abilities[i] = Helpers.CreateActivatableAbility($"ThreeFoldAspect{i}ToggleAbility",
                                                                buffs[i].Name,
                                                                buffs[i].Description,
                                                                "",
                                                                icon,
                                                                buffs[i],
                                                                AbilityActivationType.WithUnitCommand,
                                                                UnitCommand.CommandType.Standard,
                                                                null);
                abilities[i].Group = ActivatableAbilityGroupExtension.ThreefoldAspect.ToActivatableAbilityGroup();
            }


            var buff = Helpers.CreateBuff("ThreeFoldAspectBuff",
                                           "Threefold Aspect",
                                           buff1.Description,
                                           "",
                                           icon,
                                           null,
                                           Helpers.CreateAddFacts(abilities)
                                           );


            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1, DurationRate.Days), dispellable: false);

            threefold_aspect = Helpers.CreateAbility("ThreeFoldAspectAbility",
                                                     buff.Name,
                                                     buff.Description,
                                                     "",
                                                     buff.Icon,
                                                     AbilityType.Spell,
                                                     UnitCommand.CommandType.Standard,
                                                     AbilityRange.Personal,
                                                     "24 hours",
                                                     "",
                                                     Helpers.CreateRunActions(apply_buff),
                                                     Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                                     Common.createAbilitySpawnFx("352469f228a3b1f4cb269c7ab0409b8e", anchor: AbilitySpawnFxAnchor.SelectedTarget)
                                                     );
            threefold_aspect.setMiscAbilityParametersSelfOnly();
            threefold_aspect.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken | Metamagic.Extend;

            threefold_aspect.AddToSpellList(Helpers.druidSpellList, 5);
            threefold_aspect.AddSpellAndScroll("bae1ca27b27c3eb4f9e38c03abcdb64a");
        }


        static void createAggressiveThunderCloudGreater()
        {
            var icon = library.Get<BlueprintAbility>("fc432e7a63f5a3545a93118af13bcb89").Icon;

            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("3659ce23ae102ca47a7bf3a30dd98609", "AggressiveThundercloudGreaterArea", "");
            area.Size = 2.Feet();
            area.Fx = Common.createPrefabLink("cfacbb7d39eaf624382c58bad8ba2df1");//will o wisp fx


            var stunned = library.CopyAndAdd<BlueprintBuff>("09d39b38bb7c6014394b6daced9bacd3", "AggressiveThundercloudGreaterStunBuff", "");
            stunned.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Sonic));
            var caster_dmg_done = Helpers.CreateBuff("AggressiveThundercloudGreaterCasterDamageDoneBuff",
                                                      "Aggressive Thundercloud Greater (Attacked This Turn)",
                                                      "",
                                                      "",
                                                      icon,
                                                      null
                                                      );

            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Electricity, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 6));


            
            var apply_stun = Common.createContextActionApplyBuff(stunned, Helpers.CreateContextDuration(1));

            var stun_allowed = Helpers.CreateBuff("AggressiveThundercloudGreaterStunAllowedBuff",
                                                  "",
                                                  "",
                                                  "",
                                                  null,
                                                  null
                                                  );
            stun_allowed.SetBuffFlags(BuffFlags.HiddenInUi);

            var stun_action = Helpers.CreateConditional(new Condition[]{Common.createContextConditionCasterHasFact(stun_allowed) },
                                                        new GameAction[]{Common.createContextActionSavingThrow(SavingThrowType.Fortitude,
                                                                                                               Helpers.CreateActionList(Helpers.CreateConditionalSaved(null, apply_stun))
                                                                                                               ),
                                                                         Common.createContextActionOnContextCaster(Common.createContextActionRemoveBuff(stun_allowed))
                                                                         }
                                                       );

            var dmg_save_action = Common.createContextActionSavingThrow(SavingThrowType.Reflex,
                                                                                            Helpers.CreateActionList(Helpers.CreateConditionalSaved(new GameAction[0],
                                                                                                                                                    new GameAction[] { dmg, stun_action }
                                                                                                                                                   )
                                                                                            )
                                                                         );
            var dmg_action = Helpers.CreateActionList(Helpers.CreateConditional(Common.createContextConditionCasterHasFact(caster_dmg_done),
                                                      new GameAction[0],
                                                      new GameAction[]{dmg_save_action,
                                                                       Common.createContextActionApplyBuffToCaster(caster_dmg_done, Helpers.CreateContextDuration(1), dispellable: false, duration_seconds: 6)
                                                                       }
                                                                       )
                                          );

            area.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.Create<NewMechanics.AbilityAreaEffectRunActionWithFirstRound>(a => {a.FirstRound = dmg_action;
                                                                                            a.Round = dmg_action;
                                                                                            a.UnitEnter = dmg_action;
                                                                                            }
                                                                                      ),
                Helpers.CreateSpellDescriptor(SpellDescriptor.Electricity),
                Helpers.Create<AbilityAreaEffectBuff>(a => {a.Buff = thunder_cloud_fx_buff; a.Condition = Helpers.CreateConditionsCheckerOr(); })
            };
            area.SpellResistance = true;


            var caster_buff = Helpers.CreateBuff("AggressiveThundercloudGreaterCasterBuff",
                                                  "Aggressive Thundercloud, Greater",
                                                  $"This spell functions as aggressive thundercloud, except it deals 6d{BalanceFixes.getDamageDieString(DiceType.D6)} points of electricity damage to any creature it strikes. The first creature damaged by the cloud is also stunned for 1 round (Fort negates); this is a sonic effect.\n"
                                                  + "Aggressive Thundercloud: " + aggressive_thundercloud.Description,
                                                  "",
                                                  icon,
                                                  null,
                                                  Helpers.CreateAddFactContextActions(Common.createContextActionApplyBuff(stun_allowed, Helpers.CreateContextDuration(), is_child: true, is_permanent: true, dispellable: false))
                                                  );
            caster_buff.AddComponent(Helpers.CreateAddFactContextActions(deactivated: Helpers.Create<NewMechanics.RemoveUniqueArea>(a => a.feature = caster_buff)));
            area.AddComponent(Helpers.Create<UniqueAreaEffect>(u => u.Feature = caster_buff));

            var spawn_area = Common.createContextActionSpawnAreaEffect(area, Helpers.CreateContextDuration(100));


            var move_ability = Helpers.CreateAbility("AggressiveThundercloudGreaterMoveAbility",
                                                     "Aggressive Thundercloud, Greater",
                                                     "Use this ability to move Aggressive Thundercloud, Greater area.",
                                                     "",
                                                     icon,
                                                     AbilityType.SpellLike,
                                                     UnitCommand.CommandType.Move,
                                                     AbilityRange.Medium,
                                                     "",
                                                     "",
                                                     Helpers.CreateRunActions(spawn_area),
                                                     Common.createAbilityAoERadius(2.Feet(), TargetType.Any),
                                                     Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                     Helpers.CreateSpellDescriptor(SpellDescriptor.Electricity),
                                                     Common.createAbilityCasterHasNoFacts(caster_dmg_done)
                                                     );

            move_ability.setMiscAbilityParametersRangedDirectional();
            move_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Heighten | Metamagic.Maximize | (Metamagic)MetamagicFeats.MetamagicExtender.Rime | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;

            caster_buff.AddComponent(Helpers.CreateAddFact(move_ability));
            caster_buff.AddComponent(Helpers.Create<ReplaceAbilityParamsWithContext>(r => r.Ability = move_ability));


            var apply_caster_buff = Common.createContextActionApplyBuffToCaster(caster_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)));
            

            aggressive_thundercloud_greater = Helpers.CreateAbility("AggressiveThundercloudGreaterAbility",
                                                             caster_buff.Name,
                                                             caster_buff.Description,
                                                             "",
                                                             icon,
                                                             AbilityType.Spell,
                                                             UnitCommand.CommandType.Standard,
                                                             AbilityRange.Medium,
                                                             Helpers.roundsPerLevelDuration,
                                                             "Reflex Negates; Fortitude Negates",
                                                             Helpers.CreateRunActions(spawn_area),
                                                             Common.createAbilityAoERadius(2.Feet(), TargetType.Any),
                                                             Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                             Helpers.CreateSpellDescriptor(SpellDescriptor.Electricity),
                                                             Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(apply_caster_buff))
                                                           );

            aggressive_thundercloud_greater.setMiscAbilityParametersRangedDirectional();
            aggressive_thundercloud_greater.AvailableMetamagic = Metamagic.Empower | Metamagic.Extend | Metamagic.Heighten | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Rime | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            aggressive_thundercloud_greater.SpellResistance = true;

            aggressive_thundercloud_greater.AddToSpellList(Helpers.magusSpellList, 4);
            aggressive_thundercloud_greater.AddToSpellList(Helpers.druidSpellList, 4);
            aggressive_thundercloud_greater.AddToSpellList(Helpers.wizardSpellList, 4);
            aggressive_thundercloud_greater.AddSpellAndScroll("e8f59c0e2bbbb514db0dfff42dbdde91"); //jolting portent
        }



        static void createAggressiveThunderCloud()
        {
            var icon = library.Get<BlueprintAbility>("fc432e7a63f5a3545a93118af13bcb89").Icon;

            thunder_cloud_fx_buff = library.CopyAndAdd<BlueprintBuff>("bedf9d0d6be45bb4eb197b83e2ad38ee", "AggressiveThundercloudFxBuff", "");
            thunder_cloud_fx_buff.ComponentsArray = new BlueprintComponent[] { Helpers.Create<AddConcealment>(a => { a.Descriptor = ConcealmentDescriptor.Fog; a.Concealment = Concealment.Partial; a.CheckDistance = false; }), };
            thunder_cloud_fx_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("3659ce23ae102ca47a7bf3a30dd98609", "AggressiveThundercloudArea", "");
            area.Size = 2.Feet();
            area.Fx = Common.createPrefabLink("cfacbb7d39eaf624382c58bad8ba2df1"); //will o wisp fx

            var caster_dmg_done = Helpers.CreateBuff("AggressiveThundercloudCasterDamageDoneBuff",
                                      "Aggressive Thundercloud (Attacked This Turn)",
                                      "",
                                      "",
                                      icon,
                                      null
                                      );

            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Electricity, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 3));
            var dmg_action = Helpers.CreateActionList(Helpers.CreateConditional(Common.createContextConditionCasterHasFact(caster_dmg_done),
                                                     new GameAction[0], 
                                                     new GameAction[]{Common.createContextActionSavingThrow(SavingThrowType.Reflex, Helpers.CreateActionList(Helpers.CreateConditionalSaved(null, dmg))),
                                                                        Common.createContextActionApplyBuffToCaster(caster_dmg_done, Helpers.CreateContextDuration(1), dispellable: false, duration_seconds: 6)
                                                                       }
                                                                       )
                                                      );

            area.ComponentsArray = new BlueprintComponent[]
            {            
                Helpers.Create<NewMechanics.AbilityAreaEffectRunActionWithFirstRound>(a => {a.FirstRound = dmg_action;
                                                                                            a.Round = dmg_action;
                                                                                            a.UnitEnter = dmg_action;
                                                                                            }
                                                                                      ),
                Helpers.CreateSpellDescriptor(SpellDescriptor.Electricity),
                Helpers.Create<AbilityAreaEffectBuff>(a => {a.Buff = thunder_cloud_fx_buff; a.Condition = Helpers.CreateConditionsCheckerOr(); })
            };
            area.SpellResistance = true;

            var caster_buff = Helpers.CreateBuff("AggressiveThundercloudCasterBuff",
                                                  "Aggressive Thundercloud",
                                                  $"A crackling, spherical storm cloud appears at the point you designate and deals electricity damage to those inside it. As a move action you can move the cloud to any point within close range. If it enters a space that contains a creature, the storm stops moving for the round and deals 3d{BalanceFixes.getDamageDieString(DiceType.D6)} points of electricity damage to that creature, though a successful Reflex save negates that damage. It provides concealment (20% miss chance) to anything within it.\n"
                                                  + "You can move the sphere as a move action for you; otherwise, it stays at rest and crackles with lightning.",
                                                  "",
                                                  icon,
                                                  null
                                                  );
            caster_buff.AddComponent(Helpers.CreateAddFactContextActions(deactivated: Helpers.Create<NewMechanics.RemoveUniqueArea>(a => a.feature = caster_buff)));
            area.AddComponent(Helpers.Create<UniqueAreaEffect>(u => u.Feature = caster_buff));

            var spawn_area = Common.createContextActionSpawnAreaEffect(area, Helpers.CreateContextDuration(100));


            var move_ability = Helpers.CreateAbility("AggressiveThundercloudMoveAbility",
                                                     "Aggressive Thundercloud",
                                                     "Use this ability to move Aggressive Thundercloud area.",
                                                     "",
                                                     icon,
                                                     AbilityType.SpellLike,
                                                     UnitCommand.CommandType.Move,
                                                     AbilityRange.Medium,
                                                     "",
                                                     "",
                                                     Helpers.CreateRunActions(spawn_area),
                                                     Common.createAbilityAoERadius(2.Feet(), TargetType.Any),
                                                     Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                     Helpers.CreateSpellDescriptor(SpellDescriptor.Electricity),
                                                     Common.createAbilityCasterHasNoFacts(caster_dmg_done)
                                                     );

            move_ability.setMiscAbilityParametersRangedDirectional();
            move_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Heighten | Metamagic.Maximize | (Metamagic)MetamagicFeats.MetamagicExtender.Rime | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;

            caster_buff.AddComponent(Helpers.CreateAddFact(move_ability));
            caster_buff.AddComponent(Helpers.Create<ReplaceAbilityParamsWithContext>(r => r.Ability = move_ability));


            var apply_caster_buff = Common.createContextActionApplyBuffToCaster(caster_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)));


            aggressive_thundercloud = Helpers.CreateAbility("AggressiveThundercloudAbility",
                                                             caster_buff.Name,
                                                             caster_buff.Description,
                                                             "",
                                                             icon,
                                                             AbilityType.Spell,
                                                             UnitCommand.CommandType.Standard,
                                                             AbilityRange.Medium,
                                                             Helpers.roundsPerLevelDuration,
                                                             "Reflex Negates",
                                                             Helpers.CreateRunActions(spawn_area),
                                                             Common.createAbilityAoERadius(2.Feet(), TargetType.Any),
                                                             Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                             Helpers.CreateSpellDescriptor(SpellDescriptor.Electricity),
                                                             Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(apply_caster_buff))
                                                           );

            aggressive_thundercloud.setMiscAbilityParametersRangedDirectional();
            aggressive_thundercloud.AvailableMetamagic = Metamagic.Empower | Metamagic.Extend | Metamagic.Heighten | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Rime | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            aggressive_thundercloud.SpellResistance = true;

            aggressive_thundercloud.AddToSpellList(Helpers.magusSpellList, 2);
            aggressive_thundercloud.AddToSpellList(Helpers.druidSpellList, 2);
            aggressive_thundercloud.AddToSpellList(Helpers.wizardSpellList, 2);
            aggressive_thundercloud.AddSpellAndScroll("e8f59c0e2bbbb514db0dfff42dbdde91"); //jolting portent
        }


        static void createScouringWinds()
        {
            var invisibility = library.Get<BlueprintBuff>("e6b35473a237a6045969253beb09777c");
            var icon = library.Get<BlueprintFeature>("f2fa7541f18b8af4896fbaf9f2a21dfe").Icon;

            var buff = Helpers.CreateBuff("ScouringWindsTargetBuff",
                              "Scouring Winds",
                              "This spell brings forth a windstorm of stinging sand that blocks all vision. You can move the storm to any area within medium range of you as move action.\n"
                              + $"Any creature in the area takes 3d{BalanceFixes.getDamageDieString(DiceType.D6)} points of piercing damage each round. The area is considered a windstorm, creatures of medium size or smaller need to make a DC 10 Strength check or be unable to move. Creatures of small size or smaller are knocked prone and take additional 2d{BalanceFixes.getDamageDieString(DiceType.D6)} bludgeoning damage unless they succeed on a DC 15 Strength check.\n"
                              + "Ranged attacks are impossible in the area.",
                              "",
                              icon,
                              null,
                              Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Blindness | SpellDescriptor.SightBased),
                              Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.DifficultTerrain),
                              Common.createBuffDescriptorImmunity(SpellDescriptor.SightBased),
                              Helpers.Create<AddConcealment>(c => { c.Concealment = Concealment.Total; c.Descriptor = ConcealmentDescriptor.Fog; }),
                              Helpers.Create<ConcealementMechanics.AddOutgoingConcealment>(c => { c.Concealment = Concealment.Total; c.Descriptor = ConcealmentDescriptor.Fog; })
                              );
            //buff.FxOnStart = invisibility.FxOnStart;
            //buff.FxOnRemove = invisibility.FxOnRemove;


            var can_not_move_buff = Helpers.CreateBuff("ScouringWindsCanNotMoveBuff",
                                                      "Scouring Winds (Can Not Move)",
                                                      "You can not move through the wind.",
                                                      "",
                                                      icon,
                                                      null,
                                                      Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.CantMove)                               
                                                      );

            can_not_move_buff.Stacking = StackingType.Replace;


            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("b21bc337e2beaa74b8823570cd45d6dd", "ScouringWindsArea", "");

            var apply_can_not_move = Common.createContextActionApplyBuff(can_not_move_buff, Helpers.CreateContextDuration(1), dispellable: false, is_child: true);

            var dmg_regular = Helpers.CreateActionDealDamage(PhysicalDamageForm.Piercing, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 3, 0), isAoE: true);
            var check_movement = Helpers.CreateConditional(new Condition[]{Helpers.Create<CombatManeuverMechanics.ContextConditionTargetSizeLessOrEqual>(c => c.target_size = Size.Medium),
                                                                           Helpers.CreateConditionHasFact(immunity_to_wind, not: true)},
                                                           Common.createContextActionSkillCheck(StatType.Strength,
                                                                                                failure: Helpers.CreateActionList(apply_can_not_move),
                                                                                                custom_dc: 10)
                                                           );
            var check_small = Helpers.CreateConditional(new Condition[]{Helpers.Create<CombatManeuverMechanics.ContextConditionTargetSizeLessOrEqual>(c => c.target_size = Size.Small),
                                                                        Helpers.CreateConditionHasFact(immunity_to_wind, not: true)},
                                               Common.createContextActionSkillCheck(StatType.Strength,
                                                                                    failure: Helpers.CreateActionList(Helpers.Create<ContextActionKnockdownTarget>(), 
                                                                                                                      Helpers.CreateActionDealDamage(PhysicalDamageForm.Bludgeoning, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 2, 0))
                                                                                                                     ),
                                                                                    custom_dc: 15)
                                               );

            var actions = Helpers.CreateActionList(dmg_regular, check_movement, check_small);
            area.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.Create<NewMechanics.AbilityAreaEffectRunActionWithFirstRound>(a =>
                {
                    a.UnitEnter = actions;
                    a.Round = actions;
                    a.FirstRound = actions;
                }),
                Helpers.Create<AbilityAreaEffectBuff>(a => {a.Buff = buff; a.Condition = Helpers.CreateConditionsCheckerOr(Common.createContextConditionHasFact(immunity_to_wind, false)); }),
                Helpers.Create<AbilityAreaEffectBuff>(a => {a.Buff = ranged_attacks_forbidden_buff; a.Condition = Helpers.CreateConditionsCheckerOr(); })
            };

            var caster_buff = Helpers.CreateBuff("ScouringWindsCasterBuff",
                              buff.Name + " (Caster)",
                              buff.Description,
                              "",
                              icon,
                              null
                              );
            caster_buff.AddComponent(Helpers.CreateAddFactContextActions(deactivated: Helpers.Create<NewMechanics.RemoveUniqueArea>(a => a.feature = caster_buff)));
            area.AddComponent(Helpers.Create<UniqueAreaEffect>(u => u.Feature = caster_buff));

            var spawn_area = Common.createContextActionSpawnAreaEffect(area, Helpers.CreateContextDuration(100));


            var move_ability = Helpers.CreateAbility("ScouringWindsMoveAbility",
                                                     "Scouring Winds",
                                                     "Use this ability to move Scouring Winds area.",
                                                     "",
                                                     icon,
                                                     AbilityType.SpellLike,
                                                     UnitCommand.CommandType.Move,
                                                     AbilityRange.Medium,
                                                     "",
                                                     "",
                                                     Helpers.CreateRunActions(spawn_area),
                                                     Common.createAbilityAoERadius(20.Feet(), TargetType.Any),
                                                     Helpers.CreateSpellDescriptor((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Air | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Earth),
                                                     Helpers.CreateSpellComponent(SpellSchool.Evocation)
                                                     );

            move_ability.setMiscAbilityParametersRangedDirectional();
            move_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Heighten | Metamagic.Maximize | (Metamagic)MetamagicFeats.MetamagicExtender.Rime | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental;

            caster_buff.AddComponent(Helpers.CreateAddFact(move_ability));
            caster_buff.AddComponent(Helpers.Create<ReplaceAbilityParamsWithContext>(r => r.Ability = move_ability));
            

            var apply_caster_buff = Common.createContextActionApplyBuffToCaster(caster_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)));


            scouring_winds = Helpers.CreateAbility("ScouringWindsAbility",
                                         buff.Name,
                                         buff.Description,
                                         "",
                                         icon,
                                         AbilityType.Spell,
                                         UnitCommand.CommandType.Standard,
                                         AbilityRange.Medium,
                                         Helpers.roundsPerLevelDuration,
                                         Helpers.savingThrowNone,
                                         Helpers.CreateRunActions(spawn_area),
                                         Common.createAbilityAoERadius(20.Feet(), TargetType.Any),
                                         Helpers.CreateSpellDescriptor((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Air | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Earth),
                                         Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                         Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(apply_caster_buff))
                                         );

            scouring_winds.setMiscAbilityParametersRangedDirectional();
            scouring_winds.AvailableMetamagic = Metamagic.Empower | Metamagic.Extend | Metamagic.Heighten | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental;

            scouring_winds.AddToSpellList(Helpers.druidSpellList, 7);
            scouring_winds.AddToSpellList(Helpers.wizardSpellList, 7);
            scouring_winds.AddSpellAndScroll("c17e4bd5028d6534a8c8d317cd8244ca");
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("c18a821ee662db0439fb873165da25be"), scouring_winds, 7); //weather domain (instead of fire storm)
        }


        static void createWallOfFire()
        {
            var icon = library.Get<BlueprintAbility>("19309b5551a28d74288f4b6f7d8d838d").Icon; //blessing of salamander

            var area  = library.CopyAndAdd<BlueprintAbilityAreaEffect>("ac8737ccddaf2f948adf796b5e74eee7", "WallOfFireSpellAbilityArea", "");

            var configs = area.GetComponents<ContextRankConfig>();

            foreach (var c in configs)
            {
                var new_c = c.CreateCopy();
                Helpers.SetField(new_c, "m_BaseValueType", ContextRankBaseValueType.CasterLevel);
                area.ReplaceComponent(c, new_c);
            }

            wall_of_fire = library.CopyAndAdd<BlueprintAbility>("77d255c06e4c6a745b807400793cf7b1", "WallOfFireSpellAbility", "");
            wall_of_fire.SetNameDescriptionIcon("Wall of Fire",
                                                $"An immobile, blazing curtain of shimmering violet fire springs into existence. The wall deals 2d{BalanceFixes.getDamageDieString(DiceType.D6)} points of fire damage + 1 point of fire damage per caster level (maximum +20) to any creature that attempts to pass through and to any creature that ends turn in its area. The wall deals double damage to undead creatures.",
                                                icon);
            var spawn_area = Common.createContextActionSpawnAreaEffect(area, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)));

            wall_of_fire.RemoveComponents<AbilityResourceLogic>();
            wall_of_fire.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(spawn_area));
            wall_of_fire.Type = AbilityType.Spell;
            wall_of_fire.Range = AbilityRange.Medium;
            wall_of_fire.SpellResistance = true;
            wall_of_fire.AvailableMetamagic = Metamagic.Empower | Metamagic.Extend | Metamagic.Heighten | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Rime | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;

            wall_of_fire.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig());

            wall_of_fire.AddToSpellList(Helpers.druidSpellList, 5);
            wall_of_fire.AddToSpellList(Helpers.magusSpellList, 4);
            wall_of_fire.AddToSpellList(Helpers.wizardSpellList, 4);
            wall_of_fire.AddSpellAndScroll("d8f2bcc130113194998810b7ae3e07f5"); //blessing of salamander

            wall_of_fire_fire_domain = SpellDuplicates.addDuplicateSpell(wall_of_fire, "FireDomain" + wall_of_fire.name);
            //replace 4th level spell in fire domain
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("881b2137a1779294c8956fe5b497cc35"), wall_of_fire_fire_domain, 4);
        }


        static void createIncendiaryCloud()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/FierySoul.png");
            var steam_cloud_area = library.Get<BlueprintAbilityAreaEffect>("35a62ad81dd5ae3478956c61d6cd2d2e");

            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>(obscuring_mist_area.AssetGuid, "IncendiaryCloudAbilityArea", "");
            area.AddComponent(Helpers.CreateAreaEffectRunAction(round: Helpers.CreateActionSavingThrow(SavingThrowType.Reflex,
                                                                                                       Helpers.CreateActionDealDamage(DamageEnergyType.Fire,
                                                                                                       Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 6, 0), halfIfSaved: true, isAoE: true))));
            area.Fx = steam_cloud_area.Fx;
            area.AffectEnemies = true;
            area.AggroEnemies = true;

            var spawn_area = Common.createContextActionSpawnAreaEffect(area, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)));
            incendiary_cloud = Helpers.CreateAbility("IncendiaryCloudAbility",
                                                     "Incendiary Cloud",
                                                     $"An incendiary cloud spell creates a cloud of roiling smoke shot through with white-hot embers. The smoke obscures all sight as a fog cloud does. In addition, the white-hot embers within the cloud deal 6d{BalanceFixes.getDamageDieString(DiceType.D6)} points of fire damage to everything within the cloud on your turn each round. All targets can make Reflex saves each round to take half damage.",
                                                     "",
                                                     icon,
                                                     AbilityType.Spell,
                                                     UnitCommand.CommandType.Standard,
                                                     AbilityRange.Medium,
                                                     Helpers.roundsPerLevelDuration,
                                                     Helpers.reflexHalfDamage,
                                                     Helpers.CreateSpellComponent(SpellSchool.Conjuration),
                                                     Helpers.CreateSpellDescriptor(SpellDescriptor.Fire),
                                                     Helpers.CreateRunActions(spawn_area),
                                                     Common.createAbilityAoERadius(20.Feet(), TargetType.Any)
                                                     );
            incendiary_cloud.setMiscAbilityParametersRangedDirectional();
            incendiary_cloud.AvailableMetamagic = Metamagic.Empower | Metamagic.Extend | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Rime | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental;
            incendiary_cloud.AddToSpellList(Helpers.wizardSpellList, 8);
            incendiary_cloud.AddSpellAndScroll("1cbb88fbf2a6bb74aa437fadf6946d22"); // scroll fire storm
            //replace 8th level spell in fire domain
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("881b2137a1779294c8956fe5b497cc35"), incendiary_cloud, 8);
        }



        static void createWallOfNausea()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/WallOfNausea.png");
            var nauseted = library.CopyAndAdd<BlueprintBuff>("956331dba5125ef48afe41875a00ca0e", "NausetedWithoutPoisonBuff", "");
            nauseted.ReplaceComponent<SpellDescriptorComponent>(s => s.Descriptor = SpellDescriptor.Nauseated);
            var apply_buff = Common.createContextActionApplyBuff(nauseted, Helpers.CreateContextDuration(1));
            var knock_down = Common.createContextActionSkillCheck(StatType.SkillMobility, null, Helpers.CreateActionList(Helpers.Create<ContextActionKnockdownTarget>()), 12);

            var action = Helpers.CreateConditionalSaved(null, new GameAction[] { apply_buff, knock_down });
            var effect = Helpers.CreateConditional(Common.createContextConditionHasFacts(false, Common.construct, Common.undead, Common.elemental),
                                                   null,
                                                   Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(action))
                                                   );

            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("2a90aa7f771677b4e9624fa77697fdc6", "WallOfNauseaArea", "");
            area.ComponentsArray = new BlueprintComponent[] {Helpers.Create<NewMechanics.AbilityAreaEffectRunActionWithFirstRound>(a => { a.FirstRound = Helpers.CreateActionList(Helpers.Create<ContextActionRemoveSelf>());
                                                                                                                                          a.UnitEnter = Helpers.CreateActionList(effect); }) };

            area.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Nauseated | SpellDescriptor.MindAffecting));
            area.SpellResistance = true;
            wall_of_nausea = Helpers.CreateAbility("WallOfNauseaAbility",
                                                  "Wall of Nausea",
                                                  "You create a transparent, shimmering wall through which creatures and objects appear to be wildly distorted to viewers. Any creature that passes through the wall is immediately assailed by overwhelming vertigo, becoming nauseated for 1 round unless it succeeds at a Fortitude save; if nauseated, the creature must also succeed at a DC 12 Acrobatics check or fall prone.\n"
                                                  + "The wall must be continuous and unbroken when formed. If its surface is broken by any object or creature when it is cast, the spell fails.",
                                                  "",
                                                  icon,
                                                  AbilityType.Spell,
                                                  Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                  AbilityRange.Medium,
                                                  Helpers.roundsPerLevelDuration,
                                                  Helpers.fortNegates,
                                                  Helpers.CreateRunActions(Common.createContextActionSpawnAreaEffect(area, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)))
                                                                           ),
                                                  Helpers.CreateSpellComponent(SpellSchool.Illusion),
                                                  Helpers.CreateContextRankConfig(),
                                                  Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Nauseated)
                                                  );
            wall_of_nausea.setMiscAbilityParametersRangedDirectional();
            wall_of_nausea.SpellResistance = true;
            wall_of_nausea.AvailableMetamagic =  Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            wall_of_nausea.AddToSpellList(Helpers.wizardSpellList, 3);
            wall_of_nausea.AddToSpellList(Helpers.bardSpellList, 3);
            wall_of_nausea.AddSpellAndScroll("70239ec6d83b5064388e64d309fef942");

            nauseted_non_poison = nauseted;
        }


        static void createWallOfBlindness()
        {
            var icon = library.Get<BlueprintAbility>("46fd02ad56c35224c9c91c88cd457791").Icon; //blindness
            var blind = library.Get<BlueprintBuff>("187f88d96a0ef464280706b63635f2af");

            var apply_buff = Common.createContextActionApplyBuff(blind, Helpers.CreateContextDuration(), is_permanent: true);
          
            var action = Helpers.CreateConditionalSaved(null, new GameAction[] { apply_buff });
            var effect = Helpers.CreateConditional(Common.createContextConditionHasFacts(false, Common.construct, Common.undead, Common.elemental),
                                                   null,
                                                   Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(action))
                                                   );

            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("2a90aa7f771677b4e9624fa77697fdc6", "WallOfBlindnessArea", "");
            area.ComponentsArray = new BlueprintComponent[] {Helpers.Create<NewMechanics.AbilityAreaEffectRunActionWithFirstRound>(a => { a.FirstRound = Helpers.CreateActionList(Helpers.Create<ContextActionRemoveSelf>());
                                                                                                                                          a.UnitEnter = Helpers.CreateActionList(effect); }) };

            area.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Blindness));
            area.SpellResistance = true;
            wall_of_blindness = Helpers.CreateAbility("WallOfBlindnessAbility",
                                                  "Wall of Blindness",
                                                  "You create a translucent wall of energy, within which can be seen indistinct images of faces with their eyes sewn shut. Any creature that passes through the wall must save or become permanently blinded.\n"
                                                  + "The wall must be continuous and unbroken when formed. If its surface is broken by any object or creature when it is cast, the spell fails.",
                                                  "",
                                                  icon,
                                                  AbilityType.Spell,
                                                  Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                  AbilityRange.Medium,
                                                  Helpers.roundsPerLevelDuration,
                                                  Helpers.fortNegates,
                                                  Helpers.CreateRunActions(Common.createContextActionSpawnAreaEffect(area, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)))
                                                                           ),
                                                  Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                  Helpers.CreateContextRankConfig(),
                                                  Helpers.CreateSpellDescriptor(SpellDescriptor.Blindness)
                                                  );
            wall_of_blindness.setMiscAbilityParametersRangedDirectional();
            wall_of_blindness.SpellResistance = true;
            wall_of_blindness.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            wall_of_blindness.AddToSpellList(Helpers.wizardSpellList, 4);
            wall_of_blindness.AddToSpellList(Helpers.bardSpellList, 4);
            wall_of_blindness.AddToSpellList(Helpers.clericSpellList, 5);
            wall_of_blindness.AddSpellAndScroll("baa7ebab329ca4c4e8343d1e2144edf6");
        }


        static void createHypnoticPattern()
        {
            var buff = library.Get<BlueprintBuff>("6477ae917b0ec7a4ca76bc9f36b023ac"); //rainbow pattern
            buff.SetDescription("");

            hypnotic_pattern = library.CopyAndAdd<BlueprintAbility>("4b8265132f9c8174f87ce7fa6d0fe47b", "HypnoticPatternAbility", "");

            hypnotic_pattern.SetNameDescription("Hypnotic Pattern",
                                                "A twisting pattern of subtle, shifting colors weaves through the air, fascinating creatures within it. Roll 2d4 and add your caster level (maximum 10) to determine the total number of HD of creatures affected. Creatures with the fewest HD are affected first; and, among creatures with equal HD, those who are closest to the spell’s point of origin are affected first. HD that are not sufficient to affect a creature are wasted. Affected creatures become fascinated by the pattern of colors. Sightless creatures are not affected.");
            hypnotic_pattern.RemoveComponents<SpellListComponent>();
            hypnotic_pattern.ReplaceComponent<AbilitySpawnFx>(a => a.PrefabLink = Common.createPrefabLink("bb95a6177968e3f499f39e7c90c59fee"));//blinding aoe 10 feet
            hypnotic_pattern.ReplaceComponent<AbilityTargetsAround>(Helpers.CreateAbilityTargetsAround(10.Feet(), TargetType.Any));
            hypnotic_pattern.ReplaceComponent<ContextCalculateSharedValue>(c => c.Value = Helpers.CreateContextDiceValue(DiceType.D4, 2, Helpers.CreateContextValue(AbilityRankType.DamageDice)));
            hypnotic_pattern.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(min: 3, max:32));
            hypnotic_pattern.AddComponent(Helpers.CreateContextRankConfig(type: AbilityRankType.DamageDice, max: 10));
            hypnotic_pattern.LocalizedDuration = Helpers.CreateString("HypnoticPattern.Description","3 rounds");

            hypnotic_pattern.AddToSpellList(Helpers.wizardSpellList, 2);
            hypnotic_pattern.AddToSpellList(Helpers.bardSpellList, 2);
            Helpers.AddSpellAndScroll(hypnotic_pattern, "84cd707a7ae9f934389ed6bbf51b023a"); // scroll rainbow pattern
        }


        static void createFlyAndOverlandFlight()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/Fly.png");
            var spawn_fx = library.Get<BlueprintAbility>("f3c0b267dd17a2a45a40805e31fe3cd1").GetComponent<AbilitySpawnFx>();
            var buff = Helpers.CreateBuff("FlyBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Helpers.CreateAddStatBonus(StatType.Speed, 10, ModifierDescriptor.UntypedStackable)
                                          );
            buff.AddComponents(FixFlying.airborne.ComponentsArray);

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), is_from_spell: true);
            fly = Helpers.CreateAbility("FlyAbility",
                                        "Fly",
                                        "The subject can fly. Its speed is increased by 10 feet and it gains immunity to difficult terrain and ground-based effects. It also receives +3 dodge bonus to melee AC against non-flying creatures.",
                                        "",
                                        icon,
                                        AbilityType.Spell,
                                        UnitCommand.CommandType.Standard,
                                        AbilityRange.Touch,
                                        Helpers.minutesPerLevelDuration,
                                        HarmlessSaves.HarmlessSaves.will_harmless,
                                        Helpers.CreateRunActions(apply_buff),
                                        spawn_fx,
                                        Helpers.CreateContextRankConfig(),
                                        Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                        Helpers.Create<HarmlessSaves.HarmlessSpell>()
                                        );
            fly.setMiscAbilityParametersTouchFriendly();
            fly.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach;

            fly.AddToSpellList(Helpers.alchemistSpellList, 3);
            fly.AddToSpellList(Helpers.magusSpellList, 3);
            fly.AddToSpellList(Helpers.wizardSpellList, 3);
            fly.AddSpellAndScroll("1b3b15e90ba582047a40f2d593a70e5e"); //feather step
            
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("d169dd2de3630b749a2363c028bb6e7b"), fly, 3);//travel


            var apply_buff10 = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.TenMinutes), is_from_spell: true);
            fly_mass = Helpers.CreateAbility("FlyMassAbility",
                                        "Fly, Mass",
                                        "This spell functions as fly, save that it can target numerous creatures and lasts longer.",
                                        "",
                                        icon,
                                        AbilityType.Spell,
                                        UnitCommand.CommandType.Standard,
                                        AbilityRange.Close,
                                        Helpers.tenMinPerLevelDuration,
                                        HarmlessSaves.HarmlessSaves.will_harmless,
                                        Helpers.CreateRunActions(apply_buff10),
                                        spawn_fx,
                                        Helpers.CreateContextRankConfig(),
                                        Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                        Helpers.CreateAbilityTargetsAround(30.Feet(), TargetType.Ally),
                                        Helpers.Create<HarmlessSaves.HarmlessSpell>()
                                        );
            fly_mass.setMiscAbilityParametersRangedDirectional();
            fly_mass.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach;
            fly_mass.AddToSpellList(Helpers.wizardSpellList, 7);
            fly_mass.AddSpellAndScroll("1b3b15e90ba582047a40f2d593a70e5e"); //feather step


            var apply_buff_long = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Hours), is_from_spell: true);
            overland_flight = Helpers.CreateAbility("OverlandFlightAbility",
                                        "Overland Flight",
                                        "This spell functions like a fly spell, except that it lasts much longer.\n"
                                        + "Fly: " + fly.Description,
                                        "",
                                        icon,
                                        AbilityType.Spell,
                                        UnitCommand.CommandType.Standard,
                                        AbilityRange.Personal,
                                        Helpers.hourPerLevelDuration,
                                        "",
                                        Helpers.CreateRunActions(apply_buff_long),
                                        spawn_fx,
                                        Helpers.CreateContextRankConfig(),
                                        Helpers.CreateSpellComponent(SpellSchool.Transmutation)
                                        );
            overland_flight.setMiscAbilityParametersSelfOnly();
            overland_flight.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken;

            overland_flight.AddToSpellList(Helpers.alchemistSpellList, 5);
            overland_flight.AddToSpellList(Helpers.magusSpellList, 5);
            overland_flight.AddToSpellList(Helpers.wizardSpellList, 5);
            overland_flight.AddSpellAndScroll("1b3b15e90ba582047a40f2d593a70e5e"); //feather step

            fly_buff = buff;
        }


        static void createWindWalk()
        {
            var icon = library.Get<BlueprintFeature>("3c53ee4965a13d74e81b37ae34f0861b").Icon; //cloud infusion

            var buff = Helpers.CreateBuff("WindWalkBuff",
                                          "Wind Walk",
                                          "You alter the substance of your body to a cloud-like vapor and move through the air, possibly at great speed. You can take other creatures with you, each of which acts independently.\n"
                                          + "A wind walker can regain its physical form as desired and later resume the cloud form.\n"
                                          + "The speed of your group while on global map increases to 20 mph.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.Create<TravelMap.OverrideGlobalMapTravelMultiplier>(o => o.travel_map_multiplier = 7)
                                          );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Hours), is_from_spell: true);
            wind_walk = Helpers.CreateAbility("WindWalkAbility",
                                              buff.Name,
                                              buff.Description,
                                              "",
                                              icon,
                                              AbilityType.Spell,
                                              UnitCommand.CommandType.Standard,
                                              AbilityRange.Personal,
                                              Helpers.hourPerLevelDuration,
                                              "",
                                              Helpers.CreateRunActions(apply_buff),
                                              Helpers.CreateContextRankConfig(),
                                              Helpers.Create<SharedSpells.CannotBeShared>(),
                                              Helpers.CreateSpellDescriptor((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Air),
                                              Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                              Common.createAbilitySpawnFx("352469f228a3b1f4cb269c7ab0409b8e", anchor: AbilitySpawnFxAnchor.ClickedTarget)
                                              );
            wind_walk.setMiscAbilityParametersSelfOnly();

            wind_walk.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken;

            wind_walk.AddToSpellList(Helpers.alchemistSpellList, 6);
            wind_walk.AddToSpellList(Helpers.clericSpellList, 6);
            wind_walk.AddToSpellList(Helpers.druidSpellList, 7);

            wind_walk.AddSpellAndScroll("c92308c160d6d424fb64f1fd708aa6cd");
        }


        static void createAirWalk()
        {
            var icon = library.Get<BlueprintAbility>("f3c0b267dd17a2a45a40805e31fe3cd1").Icon;
            var spawn_fx = library.Get<BlueprintAbility>("f3c0b267dd17a2a45a40805e31fe3cd1").GetComponent<AbilitySpawnFx>();
            var buff = Helpers.CreateBuff("AirWalkBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Ground),
                                          Common.createBuffDescriptorImmunity(Kingmaker.Blueprints.Classes.Spells.SpellDescriptor.Ground),
                                          Common.createAddConditionImmunity(Kingmaker.UnitLogic.UnitCondition.DifficultTerrain),
                                          Helpers.CreateAddFact(FixFlying.pit_spell_immunity),
                                          Helpers.CreateSpellDescriptor((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Air)
                                          );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.TenMinutes), is_from_spell: true);
            air_walk = Helpers.CreateAbility("AirWalkAbility",
                                        "Air Walk",
                                        "The subject can tread on air as if walking on solid ground. It gains immunity to difficult terrain and ground-based effects.",
                                        "",
                                        icon,
                                        AbilityType.Spell,
                                        UnitCommand.CommandType.Standard,
                                        AbilityRange.Touch,
                                        Helpers.tenMinPerLevelDuration,
                                        "",
                                        Helpers.CreateRunActions(apply_buff),
                                        spawn_fx,
                                        Helpers.CreateContextRankConfig(),
                                        Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                        Helpers.CreateSpellDescriptor((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Air)
                                        );
            air_walk.setMiscAbilityParametersTouchFriendly();
            air_walk.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach;

            air_walk.AddToSpellList(Helpers.alchemistSpellList, 4);
            air_walk.AddToSpellList(Helpers.clericSpellList, 4);
            air_walk.AddToSpellList(Helpers.druidSpellList, 4);
            air_walk.AddSpellAndScroll("1b3b15e90ba582047a40f2d593a70e5e"); //feather step
           
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("750bfcd133cd52f42acbd4f7bc9cc365"), air_walk, 4);//air

            air_walk_communal = Helpers.CreateAbility("AirWalkCommunalAbility",
                                        "Air Walk, Communal",
                                        "This spell functions like air walk, except that it effects all allies in 30 ft radius.\n"
                                        + "Air Walk: " + air_walk.Description,
                                        "",
                                        icon,
                                        AbilityType.Spell,
                                        UnitCommand.CommandType.Standard,
                                        AbilityRange.Personal,
                                        Helpers.tenMinPerLevelDuration,
                                        "",
                                        Helpers.CreateRunActions(apply_buff),
                                        spawn_fx,
                                        Helpers.CreateContextRankConfig(),
                                        Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                        Helpers.CreateAbilityTargetsAround(30.Feet(), TargetType.Ally),
                                        Helpers.Create<SharedSpells.CannotBeShared>(),
                                        Helpers.CreateSpellDescriptor((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Air)
                                        );
            air_walk_communal.setMiscAbilityParametersSelfOnly();
            air_walk_communal.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken;
            air_walk_communal.AddToSpellList(Helpers.alchemistSpellList, 5);
            air_walk_communal.AddToSpellList(Helpers.clericSpellList, 5);
            air_walk_communal.AddToSpellList(Helpers.druidSpellList, 5);
            air_walk_communal.AddSpellAndScroll("3ac4b3793a6e580479c9fea69d737dc4"); //feather step mass
        }


        static void createMeteorSwarm()
        {
            var icon = library.Get<BlueprintAbility>("16ce660837fb2544e96c3b7eaad73c63").Icon;

            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("1d649d8859b25024888966ba1cc291d1", "MeteorSwarmFxArea", ""); //volcaninc storm
            var spawn_area = Common.createContextActionSpawnAreaEffect(area, Helpers.CreateContextDuration(1));
            area.ComponentsArray = new BlueprintComponent[] {Helpers.CreateAreaEffectRunAction(new GameAction[0]) };

            var firestorm = library.Get<BlueprintAbility>("e3d0dfe1c8527934294f241e0ae96a8d");

            var on_hit_buff = Helpers.CreateBuff("MeteorSwarmOnHitBuff",
                                             "",
                                             "",
                                             "",
                                             null,
                                             null);
            on_hit_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var damage = Helpers.CreateActionDealDamage(DamageEnergyType.Fire, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 24), true, true);
            var main_action = Common.createContextActionSavingThrow(SavingThrowType.Reflex, Helpers.CreateActionList(damage));
            Common.addConditionalDCIncrease(main_action, Helpers.CreateConditionsCheckerOr(Common.createContextConditionHasBuffFromCaster(on_hit_buff)), 4);

            var on_hit_actions = new GameAction[]{Helpers.CreateActionDealDamage(PhysicalDamageForm.Bludgeoning, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 2)),
                                                  Common.createContextActionApplyBuff(on_hit_buff, Helpers.CreateContextDuration(1)),
                                                 };


            var ranged_touch_attack_action = NewMechanics.ContextActionRangedTouchAttack.Create(on_hit_actions);
            meteor_swarm = Helpers.CreateAbility("MeteorSwarmAbility",
                                                 "Meteor Swarm",
                                                 "Meteor swarm is a very powerful and spectacular spell that is similar to fireball in many aspects.\n"
                                                 + $"When you cast it, a rain of meteors falls on the area. You make a ranged touch attack against creatures in the area to see if meteors has stuck them. Any creature struck by a meteor takes 2d{BalanceFixes.getDamageDieString(DiceType.D6)} points of bludgeoning damage (no save) and takes a -4 penalty on the saving throw against the meteor’s fire damage (see below). If a meteor misses its target, it simply explodes at the nearest corner of the target’s space.\n"
                                                 + $"Once meteors land, they explode, dealing 24d{BalanceFixes.getDamageDieString(DiceType.D6)} damage to each creature in the area.",
                                                 "",
                                                 icon,
                                                 AbilityType.Spell,
                                                 UnitCommand.CommandType.Standard,
                                                 AbilityRange.Long,
                                                 "",
                                                 Helpers.reflexHalfDamage,
                                                 Helpers.CreateRunActions(ranged_touch_attack_action, 
                                                                          main_action,
                                                                          Common.createContextActionRemoveBuffFromCaster(on_hit_buff)),
                                                 firestorm.GetComponent<AbilitySpawnFx>(),
                                                 Helpers.Create<AbilityEffectRunActionOnClickedTarget>(a => a.Action = Helpers.CreateActionList(spawn_area)),
                                                 Helpers.CreateAbilityTargetsAround(40.Feet(), TargetType.Any),
                                                 Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                 Helpers.CreateSpellDescriptor(SpellDescriptor.Fire)
                                                 );
            meteor_swarm.setMiscAbilityParametersRangedDirectional();
            meteor_swarm.SpellResistance = true;
            meteor_swarm.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | (Metamagic)MetamagicFeats.MetamagicExtender.Rime | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental | (Metamagic)MetamagicFeats.MetamagicExtender.Selective | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;

            meteor_swarm.AddToSpellList(Helpers.wizardSpellList, 9);
            meteor_swarm.AddSpellAndScroll("5c6a20a7dbe44ae478a2bd72efc98e2c");
        }

        static void createPathOfGlory()
        {
            List<Vector2> points = new List<Vector2>();
            for (int i = -3; i <= 3; i++)
            {
                points.Add(new Vector2(0.0f, (i * 8.0f).Feet().Meters));
            }
            var buff = Helpers.CreateBuff("PathOfGloryBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Common.createAddContextEffectFastHealing(1)
                                          );
            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("b8a7c68b040695a40b3a87b9676f7b50", "PathOfGloryArea", "");//cave fangs trap
            area.ComponentsArray = new BlueprintComponent[] { Helpers.Create<AbilityAreaEffectBuff>(a => 
                                                                                                   { a.Condition = Helpers.CreateConditionsCheckerOr(Helpers.Create<ContextConditionIsAlly>());
                                                                                                     a.Buff = buff;
                                                                                                   }
                                                                                                   ) 
                                                            };
                               
            area.AffectEnemies = false;
            area.AggroEnemies = false;

            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/PathOfGlory.png");
            var duration = Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Rounds);
            path_of_glory = Helpers.CreateAbility("PathOfGloryAbility",
                                                  "Path of Glory",
                                                  "You cause a 60-ft line to glow with dim illumination. While standing on glowing area your allies get fast healing 1.",
                                                  "",
                                                  icon,
                                                  AbilityType.Spell,
                                                  UnitCommand.CommandType.Standard,
                                                  AbilityRange.Close,
                                                  Helpers.roundsPerLevelDuration,
                                                  "",
                                                  Helpers.CreateRunActions(Common.createContextActionSpawnAreaEffectMultiple(area, duration, points.ToArray())),
                                                  Helpers.CreateSpellComponent(SpellSchool.Conjuration),
                                                  Helpers.CreateSpellDescriptor(SpellDescriptor.RestoreHP),
                                                  Helpers.CreateContextRankConfig(),
                                                  Common.createAbilityAoERadius(30.Feet(), TargetType.Ally),
                                                  Helpers.Create<AoeMechanics.AbilityRectangularAoeVisualizer>(ab => { ab.target_type = TargetType.Ally; ab.length = 60.Feet(); ab.width = 5.Feet(); })
                                                  );
            path_of_glory.setMiscAbilityParametersRangedDirectional();
            path_of_glory.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Reach | Metamagic.Quicken;
            path_of_glory.AddToSpellList(Helpers.clericSpellList, 2);
            path_of_glory.AddToSpellList(Helpers.bardSpellList, 2);
            path_of_glory.AddSpellAndScroll("792862674c565ad4fbb1ab0c97c42acd");
        }


        static void createPathOfGloryGreater()
        {

            List<Vector2> points = new List<Vector2>();
            for (int i = -3; i <= 3; i++)
            {
                points.Add(new Vector2(0.0f, (i * 8.0f).Feet().Meters));
            }
            var buff = Helpers.CreateBuff("PathOfGloryGreaterBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Common.createAddContextEffectFastHealing(5)
                                          );
            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("b8a7c68b040695a40b3a87b9676f7b50", "PathOfGloryGreaterArea", "");//cave fangs trap
            area.ComponentsArray = new BlueprintComponent[] { Helpers.Create<AbilityAreaEffectBuff>(a =>
                                                                                                   { a.Condition = Helpers.CreateConditionsCheckerOr(Helpers.Create<ContextConditionIsAlly>());
                                                                                                     a.Buff = buff;
                                                                                                   }
                                                                                                   )
                                                            };
            area.AffectEnemies = false;
            area.AggroEnemies = false;

            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/PathOfGlory.png");
            var duration = Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Rounds);
            path_of_glory_greater = Helpers.CreateAbility("PathOfGloryGreaterAbility",
                                                  "Path of Glory, Greater",
                                                  "This spell functions as path of glory, except as noted above, and spell area provides fast healing 5 instead of 1.\n"
                                                  + "Path of Glory: " + path_of_glory.Description,
                                                  "",
                                                  icon,
                                                  AbilityType.Spell,
                                                  UnitCommand.CommandType.Standard,
                                                  AbilityRange.Close,
                                                  Helpers.roundsPerLevelDuration,
                                                  "",
                                                  Helpers.CreateRunActions(Common.createContextActionSpawnAreaEffectMultiple(area, duration, points.ToArray())),
                                                  Helpers.CreateSpellComponent(SpellSchool.Conjuration),
                                                  Helpers.CreateSpellDescriptor(SpellDescriptor.RestoreHP),
                                                  Helpers.CreateContextRankConfig(),
                                                  Common.createAbilityAoERadius(30.Feet(), TargetType.Ally),
                                                  Helpers.Create<AoeMechanics.AbilityRectangularAoeVisualizer>(ab => { ab.target_type = TargetType.Ally; ab.length = 60.Feet(); ab.width = 5.Feet(); })
                                                  );
            path_of_glory_greater.setMiscAbilityParametersRangedDirectional();
            path_of_glory_greater.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Reach | Metamagic.Quicken;
            path_of_glory_greater.AddToSpellList(Helpers.clericSpellList, 4);
            path_of_glory_greater.AddToSpellList(Helpers.bardSpellList, 4);
            path_of_glory_greater.AddSpellAndScroll("792862674c565ad4fbb1ab0c97c42acd");
        }


        static void createTidalSurge()
        {
            var icon = library.Get<BlueprintAbility>("40681ea748d98f54ba7f5dc704507f39").Icon;//charged blast
            var deliver_projectile = library.Get<BlueprintAbility>("963da934d652bdc41900ed68f63ca1fa").GetComponent<AbilityDeliverProjectile>(); //from water blast

            var dmg = Helpers.CreateActionDealDamage(PhysicalDamageForm.Bludgeoning, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.Default)), isAoE: true, halfIfSaved: true);
            var knock_out = Helpers.CreateConditional(Helpers.Create<CombatManeuverMechanics.ContextConditionTargetSizeLessOrEqual>(c => c.target_size = Size.Medium),
                                                      Helpers.Create<ContextActionKnockdownTarget>());
            var move = Helpers.Create<CombatManeuverMechanics.ContextActionForceMove>(c => c.distance_dice = Helpers.CreateContextDiceValue(DiceType.D4, 5, 0));


            tidal_surge = Helpers.CreateAbility("TidalSurgeAbility",
                                                "Tidal Surge",
                                                $"You create an onrushing surge of water 10 feet high in a 30 - foot cone that deals 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of bludgeoning damage for every 2 caster levels you have (maximum 10d{BalanceFixes.getDamageDieString(DiceType.D6)} at 20th level). In addition to taking damage, creatures that fail their Reflex saves are pushed 1d4×5 feet away from you, and Medium or smaller creatures are also knocked prone.",
                                                "",
                                                icon,
                                                AbilityType.Spell,
                                                UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                "",
                                                Helpers.reflexHalfDamage,
                                                Helpers.CreateRunActions(SavingThrowType.Reflex, dmg, Helpers.CreateConditionalSaved(new GameAction[0], new GameAction[] { knock_out, move })),
                                                Helpers.CreateContextRankConfig(progression: ContextRankProgression.Div2, max: 10),
                                                Helpers.CreateSpellComponent(SpellSchool.Conjuration),
                                                deliver_projectile,
                                                Helpers.CreateSpellDescriptor((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Water)
                                                );
            tidal_surge.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | (Metamagic)MetamagicFeats.MetamagicExtender.Rime | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Selective;
            tidal_surge.setMiscAbilityParametersRangedDirectional();
            tidal_surge.AddToSpellList(Helpers.druidSpellList, 5);
            tidal_surge.AddSpellAndScroll("449bf6d894ed8de4bbf148023cdf36f5");
        }


        static void createStunningBarrierGreater()
        {
            var resource = Helpers.CreateAbilityResource("StunningBarrierResource", "", "", "", null);
            resource.SetFixedResource(30);

            var buff = library.CopyAndAdd<BlueprintBuff>("705fec87d90607444a1ae629acfeb94e", "StunningBarrierGreaterBuff", "");
            buff.ReplaceComponent<AddStatBonus>(a => a.Value = 2);
            buff.ReplaceComponent<BuffAllSavesBonus>(b => b.Value = 2);
            buff.AddComponent(Helpers.CreateAddAbilityResourceNoRestore(resource));

            var after_discharge = Helpers.CreateConditional(Helpers.Create<ResourceMechanics.ContextConditionTargetHasEnoughResource>(c => { c.resource = resource; c.amount = 2; }),
                                                                                                                  Helpers.Create<NewMechanics.ContextActionSpendResource>(c => c.resource = resource),
                                                                                                                  Helpers.Create<ContextActionRemoveSelf>()
                                                                                                                  );
            buff.ReplaceComponent<AddTargetAttackWithWeaponTrigger>(a =>
                                                                    {
                                                                        a.ActionOnSelf = Helpers.CreateActionList(after_discharge);
                                                                    });

            buff.AddComponent(Helpers.CreateAddFactContextActions(activated: Helpers.Create<ResourceMechanics.ContextRestoreResource>(c =>
                                                                                                                                     {
                                                                                                                                         c.Resource = resource;
                                                                                                                                         c.amount = Helpers.CreateContextValue(AbilityRankType.DamageDice);
                                                                                                                                     }
                                                                                                                                     )));
            buff.AddComponent(Helpers.CreateContextRankConfig(type: AbilityRankType.DamageDice, max: 30));


            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), is_from_spell: true);

            stunning_barrier_greater = Helpers.CreateAbility("StunningBarrierGreaterAbility",
                                                             "Stunning Barrier, Greater",
                                                             "This spell functions as stunning barrier, except as noted above, and it provides a +2 bonus to AC and on saving throws. It is not discharged until it has stunned a number of creatures equal to your caster level.\n"
                                                             + "Stunning Barrier: You are closely surrounded by a barely visible magical field. The field provides a +1 deflection bonus to AC and a +1 resistance bonus on saves. Any creature that strikes you with a melee attack is stunned for 1 round (Will negates). Once the field has stunned an opponent, the spell is discharged.",
                                                             "",
                                                             buff.Icon,
                                                             AbilityType.Spell,
                                                             UnitCommand.CommandType.Standard,
                                                             AbilityRange.Personal,
                                                             Helpers.roundsPerLevelDuration,
                                                             Helpers.willNegates,
                                                             Helpers.CreateRunActions(apply_buff),
                                                             Helpers.CreateSpellComponent(SpellSchool.Abjuration)
                                                             );
            stunning_barrier_greater.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent;
            stunning_barrier_greater.setMiscAbilityParametersSelfOnly();
            stunning_barrier_greater.AddToSpellList(Helpers.paladinSpellList, 3);
            stunning_barrier_greater.AddToSpellList(Helpers.clericSpellList, 3);
            stunning_barrier_greater.AddToSpellList(Helpers.inquisitorSpellList, 3);
            stunning_barrier_greater.AddToSpellList(Helpers.wizardSpellList, 3);
            stunning_barrier_greater.AddSpellAndScroll("e029ec259c9a37249b113060df32a01d");
        } 


        static void createBladeLash()
        {
            var buff = Helpers.CreateBuff("BladeLashCMBBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Common.createManeuverBonus(Kingmaker.RuleSystem.Rules.CombatManeuver.Trip, 10)
                                          );
            buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var apply_buff = Common.createContextActionApplyBuffToCaster(buff, Helpers.CreateContextDuration(1));
            var remove_buff = Common.createContextActionOnContextCaster(Common.createContextActionRemoveBuff(buff));

            var maneuver = Helpers.Create<ContextActionCombatManeuver>(c => { c.Type = Kingmaker.RuleSystem.Rules.CombatManeuver.Trip; c.OnSuccess = Helpers.CreateActionList(); });

            blade_lash = Helpers.CreateAbility("BladeLashAbility",
                                               "Blade Lash",
                                               "Your weapon elongates and becomes whip-like. As part of casting this spell, you can use this weapon to attempt a trip combat maneuver against one creature within 20 feet, and you gain a +10 bonus on your roll, after which the weapon returns to its previous form.",
                                               "",
                                               library.Get<BlueprintActivatableAbility>("85742dd6788c6914f96ddc4628b23932").Icon, //arcane bond speed
                                               AbilityType.Spell,
                                               UnitCommand.CommandType.Standard,
                                               AbilityRange.Custom,
                                               "",
                                               "",
                                               Helpers.CreateRunActions(apply_buff, maneuver, remove_buff),
                                               Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                               Common.createAbilitySpawnFx("352469f228a3b1f4cb269c7ab0409b8e", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                               Helpers.Create<NewMechanics.AttackAnimation>(),
                                               Helpers.Create<AbilityCasterMainWeaponIsMelee>()
                                               );
            blade_lash.NeedEquipWeapons = true;
            blade_lash.CustomRange = 20.Feet();
            blade_lash.setMiscAbilityParametersSingleTargetRangedHarmful(true);

            blade_lash.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken;
            blade_lash.AddToSpellList(Helpers.magusSpellList, 1);

            blade_lash.AddSpellAndScroll("fbdd06f0414c3ef458eb4b2a8072e502");
        }


        static void createLongArm()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/LongArm.png");

            var buff = Helpers.CreateBuff("LongArmBuff",
                                          "Long Arm",
                                          "Your arms temporarily grow in length, increasing your reach with those limbs by 5 feet.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.CreateAddStatBonus(StatType.Reach, 5, ModifierDescriptor.Enhancement)
                                          );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), is_from_spell: true);
            long_arm = Helpers.CreateAbility("LongArmAbility",
                                             buff.Name,
                                             buff.Description,
                                             "",
                                             icon,
                                             AbilityType.Spell,
                                             UnitCommand.CommandType.Standard,
                                             AbilityRange.Personal,
                                             Helpers.minutesPerLevelDuration,
                                             "",
                                             Helpers.CreateRunActions(apply_buff),
                                             Helpers.CreateContextRankConfig(),
                                             Common.createAbilitySpawnFx("352469f228a3b1f4cb269c7ab0409b8e", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                             Helpers.CreateSpellComponent(SpellSchool.Transmutation)
                                             );
            long_arm.setMiscAbilityParametersSelfOnly();
            long_arm.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken;
            long_arm.AddToSpellList(Helpers.alchemistSpellList, 1);
            long_arm.AddToSpellList(Helpers.magusSpellList, 1);
            long_arm.AddToSpellList(Helpers.wizardSpellList, 1);

            long_arm.AddSpellAndScroll("42d9445b9cdfac94385eaa2a3499b204"); //stone fist
        }


        static void createResinousSkin()
        {
            var icon = library.Get<BlueprintAbility>("0a5ddfbcfb3989543ac7c936fc256889").Icon; //spell resistance

            var main_hand_buff = library.CopyAndAdd<BlueprintBuff>("f7db19748af8b69469073485a65f37cf", "ResionousSKinMainHandBuff", "");
            var off_hand_buff = library.CopyAndAdd<BlueprintBuff>("afb1ea46a8c41e04eaab833b7b1b9321", "ResionousSKinOffHandBuff", "");

            var break_free = Helpers.CreateAddFactContextActions(newRound: Common.createContextActionSkillCheck(StatType.Strength, Helpers.CreateActionList(Helpers.Create<ContextActionRemoveSelf>())));
            main_hand_buff.AddComponent(break_free);
            off_hand_buff.AddComponent(break_free);

            var apply_main_hand = Common.createContextActionApplyBuff(main_hand_buff, Helpers.CreateContextDuration(), is_child: true, is_permanent: true, dispellable: false);
            var apply_off_hand = Common.createContextActionApplyBuff(off_hand_buff, Helpers.CreateContextDuration(), is_child: true, is_permanent: true, dispellable: false);

            var apply_main_hand_saved = Common.createContextActionSavingThrow(SavingThrowType.Reflex, Helpers.CreateActionList(Helpers.CreateConditionalSaved(null, apply_main_hand)));
            var apply_off_hand_saved = Common.createContextActionSavingThrow(SavingThrowType.Reflex, Helpers.CreateActionList(Helpers.CreateConditionalSaved(null, apply_off_hand)));
            var buff = Helpers.CreateBuff("ResinousSkinBuff",
                                          "Resinous Skin",
                                          "You coat your body with a resinous substance, protecting you from attacks and binding weapons that strike you. You gain DR 5/piercing, as well as a +4 circumstance bonus to your CMD against disarm attempts and on saving throws against effects that cause you to drop something you are holding. Additionally, you gain a +2 circumstance bonus on combat maneuver checks to initiate a grapple and maintain a grapple. Any weapon, that strikes you becomes stuck unless its wielder succeeds at a Reflex saving throw. Such a weapon can be pulled free of you only with a successful Strength check (DC = your saving throw DC for this spell). This spell has no effect on unarmed strikes or natural weapons.",
                                          "",
                                          icon,
                                          library.Get<BlueprintBuff>("0bc608c3f2b548b44b7146b7530613ac").FxOnStart, //slow
                                          Common.createContextFormDR(5, PhysicalDamageForm.Piercing),
                                          Common.createContextManeuverDefenseBonus(Kingmaker.RuleSystem.Rules.CombatManeuver.Disarm, 4),
                                          Common.createContextManeuverDefenseBonus(Kingmaker.RuleSystem.Rules.CombatManeuver.Grapple, 2),
                                          Common.createManeuverBonus(Kingmaker.RuleSystem.Rules.CombatManeuver.Grapple, 2),
                                          Helpers.Create<CombatManeuverMechanics.ApplyBuffOnHit>(a =>
                                                                                                  {
                                                                                                      a.main_hand_action = Helpers.CreateActionList(apply_main_hand_saved);
                                                                                                      a.off_hand_action = Helpers.CreateActionList(apply_off_hand_saved);
                                                                                                  }
                                                                                                 )
                                          );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.TenMinutes), is_from_spell: true);
            resinous_skin = Helpers.CreateAbility("ResinousSkinAbility",
                                                  buff.Name,
                                                  buff.Description,
                                                  "",
                                                  buff.Icon,
                                                  AbilityType.Spell,
                                                  UnitCommand.CommandType.Standard,
                                                  AbilityRange.Personal,
                                                  Helpers.tenMinPerLevelDuration,
                                                  "See text",
                                                  Helpers.CreateRunActions(apply_buff),
                                                  Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                                  Helpers.CreateContextRankConfig()
                                                  );
            resinous_skin.setMiscAbilityParametersSelfOnly();
            resinous_skin.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken;

            resinous_skin.AddToSpellList(Helpers.alchemistSpellList, 3);
            resinous_skin.AddToSpellList(Helpers.wizardSpellList, 3);
            resinous_skin.AddToSpellList(Helpers.druidSpellList, 3);

            resinous_skin.AddSpellAndScroll("05c7f7bc5f565214ca74146774c02694");
        }


        static void createSolidFogAndFixAcidFog()
        {
            var icon = Helpers.GetIcon("3c53ee4965a13d74e81b37ae34f0861b");

            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("fe5102d734382b74586f56980086e5e8", "SolidFogArea", ""); //mind fog
            area.Fx = Common.createPrefabLink("597682efc0419a142a3174fd6bb408f7"); //mind fog
            area.Size = 20.Feet();
            area.SpellResistance = false;

            var buff = Helpers.CreateBuff("SolidFogBuff",
                                          "Solid Fog",
                                          "This spell functions like fog cloud, but in addition to obscuring sight, the solid fog is so thick that it impedes movement. Creatures moving through a solid fog move at half their normal speed and take a -2 penalty on all melee attack and melee damage rolls. The vapors prevent effective ranged weapon attacks (except for magic rays and the like).",
                                          "",
                                          icon,
                                          null,
                                          Helpers.Create<AddConcealment>(a => { a.Descriptor = ConcealmentDescriptor.Fog; a.Concealment = Concealment.Partial; a.CheckDistance = false; }),
                                          Helpers.Create<AddConcealment>(a => { a.CheckDistance = true; a.DistanceGreater = 5.Feet(); a.Descriptor = ConcealmentDescriptor.Fog; a.Concealment = Concealment.Total; }),
                                          Helpers.Create<WeaponParametersAttackBonus>(a => a.AttackBonus = -2),
                                          Helpers.Create<WeaponParametersDamageBonus>(a => a.DamageBonus = -2),
                                          Helpers.Create<NewMechanics.WeaponAttackAutoMiss>(w => w.attack_types = new AttackType[] {AttackType.Ranged}),
                                          Helpers.Create<NewMechanics.OutgoingWeaponAttackAutoMiss>(w => w.attack_types = new AttackType[] { AttackType.Ranged }),
                                          Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.Slowed)
                                          );

            foreach (var c in buff.GetComponents<AddConcealment>().ToArray())
            {
                buff.AddComponent(Common.createOutgoingConcelement(c));
            }

            area.ComponentsArray = new BlueprintComponent[] { Helpers.Create<AbilityAreaEffectBuff>(a => { a.Buff = buff; a.Condition = Helpers.CreateConditionsCheckerAnd(); }) };
            solid_fog_area = area;

            solid_fog = library.CopyAndAdd<BlueprintAbility>("68a9e6d7256f1354289a39003a46d826", "SolidFogAbility", "");
            solid_fog.SpellResistance = false;
            solid_fog.RemoveComponents<SpellListComponent>();
            solid_fog.RemoveComponents<SpellDescriptorComponent>();
            solid_fog.ReplaceComponent<AbilityAoERadius>(a => Helpers.SetField(a, "m_Radius", 20.Feet()));
            solid_fog.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(Common.changeAction<ContextActionSpawnAreaEffect>(a.Actions.Actions, s => { s.AreaEffect = area; s.DurationValue = Helpers.CreateContextDuration(s.DurationValue.BonusValue, DurationRate.Minutes); })));
            solid_fog.SetNameDescriptionIcon(buff.Name, buff.Description, buff.Icon);
            solid_fog.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach | Metamagic.Extend;
            solid_fog.AddToSpellList(Helpers.magusSpellList, 4);
            solid_fog.AddToSpellList(Helpers.wizardSpellList, 4);
            solid_fog.LocalizedDuration = Helpers.minutesPerLevelDuration;
            solid_fog.AddSpellAndScroll("c92308c160d6d424fb64f1fd708aa6cd");//stiking cloud
            //fix acid fog 
            var acid_fog = library.Get<BlueprintAbility>("dbf99b00cd35d0a4491c6cc9e771b487");
            acid_fog.SetDescription("Acid fog creates a billowing mass of misty vapors like the solid fog spell. In addition to slowing down creatures and obscuring sight, this spell’s vapors are highly acidic. Each round on your turn, starting when you cast the spell, the fog deals 2d6 points of acid damage to each creature and object within it.\n"
                                    + "Solid Fog: " + solid_fog.Description);

            var acid_fog_buff = library.Get<BlueprintBuff>("af76754540cacca45bfb1f0074bf3013");
            acid_fog_buff.ComponentsArray = buff.ComponentsArray;
            acid_fog_buff.SetDescription(acid_fog.Description);
        }


        static void createAccursedGlare()
        {
            var icon = library.Get<BlueprintAbility>("3167d30dd3c622c46b0c0cb242061642").Icon; //eyebite
            var doom_spell = library.Get<BlueprintAbility>("fbdd8c455ac4cde4a9a3e18c84af9485");
            accursed_glare = library.CopyAndAdd<BlueprintAbility>("ca1a4cd28737ae544a0a7e5415c79d9b", "AccursedGlareAbility", ""); //touch of chaos as base
            accursed_glare.SetIcon(icon);
            accursed_glare.Type = AbilityType.Spell;
            accursed_glare.SetName("Accursed Glare");
            accursed_glare.LocalizedDuration = Helpers.CreateString("AccursedGlareAbility.Duration", "1 day/level");
            accursed_glare.SetDescription("You channel a fell curse through your glare. If the target fails its saving throw, it begins to obsessively second guess its actions and attract bad luck. Whenever the target attempts an attack roll or saving throw while the curse lasts, it must roll twice and take the lower result.");
            accursed_glare.Range = AbilityRange.Close;
            accursed_glare.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Point;
            accursed_glare.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionPoint;
            accursed_glare.RemoveComponent(accursed_glare.GetComponent<AbilityDeliverTouch>());
            accursed_glare.RemoveComponent(accursed_glare.GetComponent<AbilityResourceLogic>());
            accursed_glare.RemoveComponents<SpellComponent>();
            accursed_glare.LocalizedSavingThrow = Helpers.CreateString("AccursedGlare.SavingThrow", Helpers.willNegates);

            var buff = library.CopyAndAdd<BlueprintBuff>("96bbd279e0bed0f4fb208a1761f566b5", "AccursedGlareBuff", "");
            buff.SetName(accursed_glare.Name);
            buff.SetDescription(accursed_glare.Description);
               
            var action = Common.createContextActionSavingThrow(SavingThrowType.Will,
                                                                Helpers.CreateActionList(Common.createContextSavedApplyBuff(buff, DurationRate.Days, AbilityRankType.Default)));

            accursed_glare.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(action));
            accursed_glare.AddComponent(Helpers.CreateContextRankConfig());
            accursed_glare.AddComponent(doom_spell.GetComponent<Kingmaker.UnitLogic.Abilities.Components.Base.AbilitySpawnFx>());
            accursed_glare.AddComponent(Helpers.CreateSpellComponent(SpellSchool.Necromancy));
            accursed_glare.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Curse));
            accursed_glare.AvailableMetamagic = Metamagic.Quicken | Metamagic.Extend | Metamagic.Heighten | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            accursed_glare.SpellResistance = true;

            accursed_glare.AddToSpellList(Helpers.wizardSpellList, 3);
            accursed_glare.AddSpellAndScroll("f80549ec4e61ae64d8051bb3b24cf10e");
        }


        static void createDazzlingBlade()
        {
            
            var enchant = WeaponEnchantments.dazzling_blade_fx_enchant;
            var icon = library.Get<BlueprintBuff>("50d0501ad05f15d498c3aa8d602af273").Icon;
            var blinded = library.Get<BlueprintBuff>("187f88d96a0ef464280706b63635f2af");
            var dazzled = library.Get<BlueprintBuff>("df6d1025da07524429afbae248845ecc");

            var release_action = Helpers.CreateConditionalSaved(Common.createContextActionApplyBuff(dazzled, Helpers.CreateContextDuration(1)),
                                                      Common.createContextActionApplyBuff(blinded, Helpers.CreateContextDuration(1)));

            var release_ability = Helpers.CreateAbility("DazzlingBladeReleaseAbility",
                                                        "Dazzling Blade: Release",
                                                        "Dazzling blade makes a weapon appear dazzlingly shiny, as if crafted from pure silver and heavily polished. In combat, the flashing movements of a dazzling blade become almost hypnotic. The wielder of a weapon under the effects of dazzling blade gains a + 1 competence bonus on all Bluff checks made to feint in combat. The wielder also gains a + 1 competence bonus on all CMB checks made to disarm a foe, and a +1 competence bonus to his CMD against disarm attempts made against the weapon bearing the dazzling blade effect. This bonus increases by +1 for every 3 caster levels, to a maximum bonus of + 5 at 12th level.\n"
                                                        + "The wielder of a dazzling blade can discharge the spell into a blinding burst of silvery light as a free action.The wielder selects an adjacent opponent as the focal point of this burst of light—that creature must make a Will save to avoid being blinded for 1 round(with a successful save, the creature is instead dazzled for 1 round).\n"
                                                        + "Despite its shiny appearance, a dazzling blade grants no extra benefit against creatures that are vulnerable to silver.",
                                                        "",
                                                        icon,
                                                        AbilityType.SpellLike,
                                                        UnitCommand.CommandType.Free,
                                                        AbilityRange.Weapon,
                                                        Helpers.oneRoundDuration,
                                                        Helpers.willNegates,
                                                        Helpers.CreateSpellDescriptor(SpellDescriptor.Blindness)
                                                        );
            release_ability.setMiscAbilityParametersTouchHarmful();

            var buff = Helpers.CreateBuff("DazzlingBladeBuff",
                                          "Dazzling Blade",
                                          release_ability.Description,
                                          "",
                                          release_ability.Icon,
                                          null,
                                          Helpers.CreateAddFact(release_ability),
                                          Helpers.Create<ReplaceAbilityParamsWithContext>(r => r.Ability = release_ability),
                                          Common.createContextManeuverDefenseBonus(Kingmaker.RuleSystem.Rules.CombatManeuver.Disarm, Helpers.CreateContextValue(AbilityRankType.Default)),
                                          Helpers.Create<NewMechanics.ContextCombatManeuverBonus>(c => { c.Type = Kingmaker.RuleSystem.Rules.CombatManeuver.Disarm; c.Bonus = Helpers.CreateContextValue(AbilityRankType.Default); }),
                                          Helpers.Create<NewMechanics.SkillBonusInCombat>(s => { s.value = Helpers.CreateContextValue(AbilityRankType.Default); s.skill = StatType.CheckBluff; s.descriptor = ModifierDescriptor.Competence; }),
                                          Helpers.CreateContextRankConfig(progression: ContextRankProgression.OnePlusDivStep, stepLevel: 3, max: 5),
                                          Helpers.Create<NewMechanics.EnchantmentMechanics.PersistentWeaponEnchantment>(p => { p.enchant = enchant; p.secondary_hand = false; p.only_melee = true; })
                                          );

            release_ability.AddComponent(Helpers.CreateRunActions(SavingThrowType.Will, release_action, Common.createContextActionOnContextCaster(Common.createContextActionRemoveBuff(buff))));

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus), DurationRate.Minutes), is_from_spell: true);

            dazzling_blade = Helpers.CreateAbility("DazzlingBladeAbility",
                                                   buff.Name,
                                                   buff.Description,
                                                   "",
                                                   buff.Icon,
                                                   AbilityType.Spell,
                                                   UnitCommand.CommandType.Swift,
                                                   AbilityRange.Touch,
                                                   Helpers.minutesPerLevelDuration,
                                                   Helpers.willNegates,
                                                   Helpers.CreateRunActions(apply_buff),
                                                   Helpers.CreateSpellComponent(SpellSchool.Illusion),
                                                   Common.createAbilitySpawnFx("790eb82d267bf0749943fba92b7953c2", anchor: AbilitySpawnFxAnchor.SelectedTarget)
                                                   );
            dazzling_blade.setMiscAbilityParametersTouchFriendly();
            dazzling_blade.AvailableMetamagic = Metamagic.Heighten | Metamagic.Extend | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent;
            release_ability.AvailableMetamagic = (Metamagic)MetamagicFeats.MetamagicExtender.Persistent;

            dazzling_blade_mass = Helpers.CreateAbility("DazzlingBladeMassAbility",
                                                       "Dazzling Blade, Mass",
                                                       "This spell functions like dazzling blade, except as noted above and that it affects multiple weapons. Each wielder of a dazzling blade can discharge the weapon’s effect to attempt to blind a foe independently of the others.\n"
                                                       + "Dazzling Blade: " + dazzling_blade.Description,
                                                       "",
                                                       buff.Icon,
                                                       AbilityType.SpellLike,
                                                       UnitCommand.CommandType.Swift,
                                                       AbilityRange.Close,
                                                       Helpers.minutesPerLevelDuration,
                                                       Helpers.willNegates,
                                                       Helpers.CreateRunActions(apply_buff),
                                                       Helpers.CreateSpellComponent(SpellSchool.Illusion),
                                                       Helpers.CreateAbilityTargetsAround(15.Feet(), TargetType.Ally),
                                                       Common.createAbilitySpawnFx("790eb82d267bf0749943fba92b7953c2", anchor: AbilitySpawnFxAnchor.SelectedTarget)
                                                       );
            dazzling_blade_mass.setMiscAbilityParametersRangedDirectional();
            dazzling_blade_mass.AvailableMetamagic = Metamagic.Heighten | Metamagic.Extend | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent;

            dazzling_blade.AddToSpellList(Helpers.bardSpellList, 1);
            dazzling_blade.AddToSpellList(Helpers.wizardSpellList, 1);

            dazzling_blade_mass.AddToSpellList(Helpers.wizardSpellList, 3);

            dazzling_blade.AddSpellAndScroll("fbdd06f0414c3ef458eb4b2a8072e502");
            dazzling_blade_mass.AddSpellAndScroll("fbdd06f0414c3ef458eb4b2a8072e502");
        }


        static void createMagicWeapon()
        {
            var icon = library.Get<BlueprintAbility>("7ff088ab58c69854b82ea95c2b0e35b4").Icon;
            var off_hand = new bool[]{ false, true};
            var magic_weapon_variants = new List<BlueprintAbility>();
            var greater_magic_weapon_variants = new List<BlueprintAbility>();
            foreach (var oh in off_hand)
            {
                var buff = Helpers.CreateBuff((oh ? "OffHand" : "") + "MagicWeaponBaseBuff",
                                              "Magic Weapon",
                                              "Magic weapon gives a weapon a +1 enhancement bonus on attack and damage rolls. An enhancement bonus does not stack with a masterwork weapon’s +1 bonus on attack rolls.\n"
                                              + "You can’t cast this spell on a natural weapon, such as an unarmed strike (instead, see magic fang).",
                                              "",
                                              icon,
                                              null,
                                              Helpers.Create<NewMechanics.EnchantmentMechanics.BuffContextEnhancePrimaryHandWeaponUpToValue>(b => { b.value = 1; b.in_off_hand = oh; })
                                              );
                buff.Stacking = StackingType.Stack;
                buff.SetBuffFlags(BuffFlags.HiddenInUi);
                var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus), DurationRate.Minutes), true);

                var greater_buff = Helpers.CreateBuff((oh ? "OffHand" : "") + "MagicWeaponGreaterBuff",
                                              "Magic Weapon, Greater",
                                              "This spell functions like magic weapon, except that it gives a weapon an enhancement bonus on attack and damage rolls of +1 per four caster levels (maximum +5).\n"
                                              + "Magic Weapon: " + buff.Description,
                                              "",
                                              icon,
                                              null,
                                              Helpers.Create<NewMechanics.EnchantmentMechanics.BuffContextEnhancePrimaryHandWeaponUpToValue>(b => { b.value = Helpers.CreateContextValue(AbilityRankType.Default); b.in_off_hand = oh; }),
                                              Helpers.CreateContextRankConfig(progression: ContextRankProgression.DivStep, stepLevel: 4)
                                              );

                greater_buff.Stacking = StackingType.Stack;
                greater_buff.SetBuffFlags(BuffFlags.HiddenInUi);
                var apply_greater_buff = Common.createContextActionApplyBuff(greater_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus), DurationRate.Hours), true);

                var magic_weapon_v = Helpers.CreateAbility((oh ? "OffHand" : "MainHand") + "MagicWeaponBaseAbility",
                                                     buff.Name + (oh ? " (Off-Hand)" : ""),
                                                     buff.Description,
                                                     "",
                                                     buff.Icon,
                                                     AbilityType.Spell,
                                                     UnitCommand.CommandType.Standard,
                                                     AbilityRange.Touch,
                                                     Helpers.minutesPerLevelDuration,
                                                     HarmlessSaves.HarmlessSaves.will_harmless,
                                                     Helpers.CreateRunActions(apply_buff),
                                                     Helpers.CreateContextRankConfig(type: AbilityRankType.StatBonus),
                                                     Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                                     Common.createAbilitySpawnFx("cf69c140c7d4d3c49a37e9202c5b835e", anchor: AbilitySpawnFxAnchor.SelectedTarget), //bless weapon
                                                     Helpers.Create<NewMechanics.AbilitTargetManufacturedWeapon>(a => a.off_hand = oh),
                                                     Helpers.Create<HarmlessSaves.HarmlessSpell>()
                                                     );
                magic_weapon_v.setMiscAbilityParametersTouchFriendly();
                magic_weapon_v.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken | Metamagic.Extend;

               var magic_weapon_greater_v = Helpers.CreateAbility((oh ? "OffHand" : "MainHand") + "MagicWeaponGreaterAbility",
                                                             greater_buff.Name + (oh ? " (Off-Hand)" : ""),
                                                             greater_buff.Description,
                                                             "",
                                                             greater_buff.Icon,
                                                             AbilityType.Spell,
                                                             UnitCommand.CommandType.Standard,
                                                             AbilityRange.Touch,
                                                             Helpers.hourPerLevelDuration,
                                                             HarmlessSaves.HarmlessSaves.will_harmless,
                                                             Helpers.CreateRunActions(apply_greater_buff),
                                                             Helpers.CreateContextRankConfig(type: AbilityRankType.StatBonus),
                                                             Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                                             Common.createAbilitySpawnFx("cf69c140c7d4d3c49a37e9202c5b835e", anchor: AbilitySpawnFxAnchor.SelectedTarget), //bless weapon
                                                             Helpers.Create<NewMechanics.AbilitTargetManufacturedWeapon>(a => a.off_hand = oh),
                                                             Helpers.Create<HarmlessSaves.HarmlessSpell>()
                                                             );
                magic_weapon_greater_v.setMiscAbilityParametersTouchFriendly();
                magic_weapon_greater_v.AvailableMetamagic = magic_weapon_v.AvailableMetamagic;
                magic_weapon_variants.Add(magic_weapon_v);
                greater_magic_weapon_variants.Add(magic_weapon_greater_v);
            }

            magic_weapon = Common.createVariantWrapper("MagicWeaponBaseAbility", "", magic_weapon_variants.ToArray());
            magic_weapon.AddComponent(Helpers.CreateSpellComponent(SpellSchool.Transmutation));
            magic_weapon_greater = Common.createVariantWrapper("MagicWeaponGreaterAbility", "", greater_magic_weapon_variants.ToArray());
            magic_weapon_greater.AddComponent(Helpers.CreateSpellComponent(SpellSchool.Transmutation));

            magic_weapon.AddToSpellList(Helpers.inquisitorSpellList, 1);
            magic_weapon.AddToSpellList(Helpers.clericSpellList, 1);
            magic_weapon.AddToSpellList(Helpers.magusSpellList, 1);
            magic_weapon.AddToSpellList(Helpers.paladinSpellList, 1);
            magic_weapon.AddToSpellList(Helpers.wizardSpellList, 1);
            magic_weapon.AddSpellAndScroll("fbdd06f0414c3ef458eb4b2a8072e502"); //bless weapon


            magic_weapon_greater.AddToSpellList(Helpers.inquisitorSpellList, 3);
            magic_weapon_greater.AddToSpellList(Helpers.clericSpellList, 4);
            magic_weapon_greater.AddToSpellList(Helpers.magusSpellList, 3);
            magic_weapon_greater.AddToSpellList(Helpers.paladinSpellList, 3);
            magic_weapon_greater.AddToSpellList(Helpers.wizardSpellList, 3);
            magic_weapon_greater.AddSpellAndScroll("fbdd06f0414c3ef458eb4b2a8072e502"); //bless weapon

            //replace 1st level spell in war domain
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("8d454cbb7f25070419a1c8eaf89b5be5"), magic_weapon, 1);
        }


        static void createFlamesOfTheFaithful()
        {

            var flaming = library.Get<BlueprintWeaponEnchantment>("30f90becaaac51f41bf56641966c4121");
            var flaming_burst = library.Get<BlueprintWeaponEnchantment>("3f032a3cd54e57649a0cdad0434bf221");

            var buff_flaming_burst = Helpers.CreateBuff("FlamesOfTheFaithfulFlamingBurstBuff",
                                                           "",
                                                           "",
                                                           "",
                                                           null,
                                                           null,
                                                           Common.createBuffContextEnchantPrimaryHandWeapon(1, false, true, flaming_burst)
                                                           );
            buff_flaming_burst.SetBuffFlags(BuffFlags.HiddenInUi);

            var judgment_watcher = library.Get<BlueprintBuff>("9b8bb2ce8f67e5b4fa634ed6a6671f7a");
            var conditional = Helpers.CreateConditional(Helpers.CreateConditionHasFact(judgment_watcher),
                                                        Common.createContextActionApplyBuff(buff_flaming_burst, Helpers.CreateContextDuration(), is_child: true, is_permanent: true, dispellable: false),
                                                        Common.createContextActionRemoveBuff(buff_flaming_burst)
                                                        );

            var buff = Helpers.CreateBuff("FlamesOfTheFaithfulBuff",
                                            "",
                                            "",
                                            "",
                                            null,
                                            null,
                                            Common.createBuffContextEnchantPrimaryHandWeapon(1, false, true, flaming),
                                            Helpers.CreateAddFactContextActions(activated: conditional, newRound: conditional)
                                            );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), is_from_spell: true);
            flames_of_the_faithful = Helpers.CreateAbility("FlamesOfTheFaithful",
                                                           "Flames of the Faithful",
                                                           "With a touch, you cause a glowing rune to appear on a single weapon, granting that weapon the flaming property (and allowing it to cause an extra 1d6 points of fire damage on a successful hit). If you are using the judgment class feature, your weapon gains the flaming burst property instead.\n"
                                                           + "The spell functions only for weapons that you wield. If the weapon leaves your hand for any reason, the spell effect ends. The effects of this spell do not stack with any existing flaming or flaming burst weapon property that the target weapon may already possess.",
                                                           "",
                                                           library.Get<BlueprintBuff>("32e17840df49fbd48b835d080f5673a4").Icon, //arcane weapon flaming
                                                           AbilityType.Spell,
                                                           UnitCommand.CommandType.Standard,
                                                           AbilityRange.Personal,
                                                           Helpers.roundsPerLevelDuration,
                                                           "",
                                                           Helpers.CreateSpellDescriptor(SpellDescriptor.Fire),
                                                           Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                                           Helpers.CreateRunActions(apply_buff),
                                                           Helpers.CreateContextRankConfig()
                                                           );
            flames_of_the_faithful.setMiscAbilityParametersSelfOnly(Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.EnchantWeapon);
            flames_of_the_faithful.NeedEquipWeapons = true;
            flames_of_the_faithful.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken | (Metamagic)MetamagicFeats.MetamagicExtender.Rime | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental;

            flames_of_the_faithful.AddToSpellList(Helpers.inquisitorSpellList, 2);
            flames_of_the_faithful.AddSpellAndScroll("fbdd06f0414c3ef458eb4b2a8072e502");
        }


        static void createLendJudgement()
        {
            var icon = library.Get<BlueprintAbility>("db0f61cd985ca09498cafde9a4b27a16").Icon; //eccle blessing short
            List <BlueprintActivatableAbility> judgments = new List<BlueprintActivatableAbility>();

            judgments.AddRange(library.Get<BlueprintFeature>("981def910b98200499c0c8f85a78bde8").GetComponent<AddFacts>().Facts.Cast<BlueprintActivatableAbility>()); //base
            //judgments.Add(library.Get<BlueprintActivatableAbility>("bfec9cee57e7a7d47a8641c5c9d43c41")); //resilency base
            judgments.Add(library.Get<BlueprintActivatableAbility>("d7e61eb9f0cec5e49bd1b0c428fa98e4")); //smiting base
            judgments.Add(library.Get<BlueprintActivatableAbility>("2c448ab4135c7c741b6f0f223901f9fa")); //smiting adamantite;
            judgments.Add(library.Get<BlueprintActivatableAbility>("72fe16312b4479145afc6cc6c87cd08f")); //smiting alignment

            foreach (var comp in library.Get<BlueprintFeature>("112314658d0848645b34555a41def2ff").GetComponents<AddFeatureOnAlignment>())
            {
                judgments.Add(comp.Facts[0] as BlueprintActivatableAbility);
            }

            List<BlueprintAbility> variants = new List<BlueprintAbility>();
            List<BlueprintBuff> variant_buffs = new List<BlueprintBuff>();
            List<BlueprintBuff> j_buffs = new List<BlueprintBuff>();
            foreach (var judgment in judgments)
            {
                var judgment_buff = judgment.Buff;
                j_buffs.Add(judgment_buff);
                var buff = Helpers.CreateBuff(judgment_buff.name + "LendJudgmentBuff",
                                              "Lend Judgment: " + judgment_buff.Name,
                                              judgment_buff.Description,
                                              "",
                                              judgment_buff.Icon,
                                              judgment_buff.FxOnStart,
                                              judgment_buff.ComponentsArray.Where(c => !(c as AddFactContextActions)).ToArray()
                                              );
               
                var remove_condition = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(judgment_buff), null, Helpers.Create<ContextActionRemoveSelf>());
                buff.AddComponent(Helpers.CreateAddFactContextActions(newRound: remove_condition));

                var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), is_from_spell: true);
                var ability = Helpers.CreateAbility(judgment_buff.name + "LendJudgmentAbility",
                                                    buff.Name,
                                                    buff.Description,
                                                    "",
                                                    buff.Icon,
                                                    AbilityType.Spell,
                                                    UnitCommand.CommandType.Standard,
                                                    AbilityRange.Touch,
                                                    Helpers.roundsPerLevelDuration,
                                                    HarmlessSaves.HarmlessSaves.will_harmless,
                                                    Helpers.CreateRunActions(apply_buff),
                                                    Helpers.Create<NewMechanics.AbilityShowIfCasterHasFact2>(a => a.UnitFact = judgment_buff),
                                                    Helpers.CreateSpellComponent(SpellSchool.Divination),
                                                    Helpers.CreateContextRankConfig(),
                                                    Helpers.Create<HarmlessSaves.HarmlessSpell>()
                                                    );
                ability.setMiscAbilityParametersTouchFriendly(false);
                ability.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach;

                variants.Add(ability);
                variant_buffs.Add(buff);
            }

            lend_judgement = Common.createVariantWrapper("LendJudgmentAbility", "", variants.ToArray());
            lend_judgement.SetNameDescriptionIcon("Lend Judgment",
                                                  "You create a conduit of divine knowledge and outrage between you and an ally. That ally gains the benefit of one of your active judgments (as do you). If you cannot use a judgment (for example, if you are not in combat, are frightened or unconscious, and so on) or change judgments, the ally loses the benefit of the judgment. If you have multiple judgments active, the ally gains only one, chosen when you cast this spell.",
                                                  icon
                                                  );
            lend_judgement.AddComponent(Helpers.CreateSpellComponent(SpellSchool.Divination));

            var greater_buff = Helpers.CreateBuff("LendJudgmentGreaterBuff",
                                                  "Lend Judgement, Greater",
                                                  "This functions as lend judgment, except the ally gains the benefit of all your active judgments.\n"
                                                  + lend_judgement.Name + ": " + lend_judgement.Description,
                                                  "",
                                                  icon,
                                                  null
                                                  );
            greater_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            List<GameAction> actions = new List<GameAction>();


            for (int i = 0; i < variant_buffs.Count; i++)
            {
                var apply_greater = Common.createContextActionApplyBuff(variant_buffs[i], Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), is_child: true, dispellable: false);
                var apply_greater_condition = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(j_buffs[i]), apply_greater, null);
                actions.Add(apply_greater_condition);
            }

            greater_buff.AddComponent(Helpers.CreateAddFactContextActions(activated: actions.ToArray()));
            greater_buff.AddComponent(Helpers.CreateContextRankConfig());

            var apply_greater_buff = Common.createContextActionApplyBuff(greater_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), is_from_spell: true);
            lend_judgement_greater = Helpers.CreateAbility("LendJudgmentGreaterAbility",
                                                            greater_buff.Name,
                                                            greater_buff.Description,
                                                            "",
                                                            greater_buff.Icon,
                                                            AbilityType.Spell,
                                                            UnitCommand.CommandType.Standard,
                                                            AbilityRange.Touch,
                                                            Helpers.roundsPerLevelDuration,
                                                            HarmlessSaves.HarmlessSaves.will_harmless,
                                                            Helpers.CreateRunActions(apply_greater_buff),
                                                            Helpers.CreateSpellComponent(SpellSchool.Divination),
                                                            Helpers.CreateContextRankConfig(),
                                                            Common.createAbilityCasterHasFacts(j_buffs.ToArray()),
                                                            Helpers.Create<HarmlessSaves.HarmlessSpell>()
                                                            );
            lend_judgement_greater.setMiscAbilityParametersTouchFriendly(false);
            lend_judgement_greater.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach;


            lend_judgement.AddToSpellList(Helpers.inquisitorSpellList, 1);
            lend_judgement.AddSpellAndScroll("bd2b396f9e96fb24786d3744fe874a19");
            lend_judgement_greater.AddToSpellList(Helpers.inquisitorSpellList, 5);
            lend_judgement_greater.AddSpellAndScroll("bd2b396f9e96fb24786d3744fe874a19");
        }


        static void createFieryRunes()
        {
            var icon = library.Get<BlueprintFeature>("5e33543285d1c3d49b55282cf466bef3").Icon; //evocation

            var allow_buff = Helpers.CreateBuff("FieryRunesAllowBuff",
                                                "",
                                                "",
                                                "",
                                                null,
                                                null);
            allow_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var activatable_ability = Helpers.CreateActivatableAbility("FieryRunesDischargeActivatableAbility",
                                                                       "Fiery Runes: Discharge",
                                                                       "You charge a target's weapon with a magic rune of fire.\n"
                                                                       + $"When the wielder of the weapon successfully strikes a foe in melee with the weapon, it can discharge the rune as a swift action to deal 1d{BalanceFixes.getDamageDieString(DiceType.D4)} + 1 points of fire damage to the target. This damage isn’t multiplied on a critical hit.\n"
                                                                       + $"For every 2 caster levels beyond 3rd the caster possesses, the rune deals an additional 1d{BalanceFixes.getDamageDieString(DiceType.D4)} + 1 points of fire damage (2d{BalanceFixes.getDamageDieString(DiceType.D4)} + 2 at caster level 5th, 3d{BalanceFixes.getDamageDieString(DiceType.D4)} + 3 at 7th, and so on) to a maximum of 5d{BalanceFixes.getDamageDieString(DiceType.D4)} + 5 points of fire damage at caster level 11th.",
                                                                       "",
                                                                       icon,
                                                                       allow_buff,
                                                                       AbilityActivationType.WithUnitCommand,
                                                                       UnitCommand.CommandType.Swift,
                                                                       null
                                                                       );
            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Fire, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D4), Helpers.CreateContextValue(AbilityRankType.DamageDice), Helpers.CreateContextValue(AbilityRankType.DamageDice)),
                                                     IgnoreCritical: true);

            var effect = Helpers.CreateConditional(Helpers.Create<NewMechanics.ContextConditionMainTargetHasFact>(c => c.Fact = allow_buff), dmg);
            
            var buff = Helpers.CreateBuff("FieryRunesBuff",
                                          "Fiery Runes",
                                          activatable_ability.Description,
                                          "",
                                          icon,
                                          null,
                                          Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(effect), check_weapon_range_type: true, wait_for_attack_to_resolve: true),
                                          Helpers.CreateAddFact(activatable_ability),
                                          Helpers.CreateContextRankConfig(type: AbilityRankType.DamageDice, progression: ContextRankProgression.StartPlusDivStep, startLevel: 3, stepLevel: 2, max: 5, feature: MetamagicFeats.intensified_metamagic)
                                          );
            var remove = Helpers.CreateConditional(Helpers.Create<NewMechanics.ContextConditionMainTargetHasFact>(c => c.Fact = allow_buff), Common.createContextActionRemoveBuff(buff));
            buff.AddComponent(Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(remove), check_weapon_range_type: true, on_initiator: true, wait_for_attack_to_resolve: true));

            fiery_runes = Helpers.CreateAbility("FieryRunesAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                icon,
                                                AbilityType.Spell,
                                                UnitCommand.CommandType.Standard,
                                                AbilityRange.Touch,
                                                Helpers.minutesPerLevelDuration,
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValueRank(AbilityRankType.Default), DurationRate.Minutes), is_from_spell: true)),
                                                Common.createAbilitySpawnFx("52d413df527f9fa4a8cf5391fd593edd", anchor: AbilitySpawnFxAnchor.SelectedTarget), //evocation buff
                                                Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.Fire),
                                                Helpers.CreateContextRankConfig()
                                                );

            fiery_runes.setMiscAbilityParametersTouchFriendly();
            fiery_runes.AvailableMetamagic = Metamagic.Empower | Metamagic.Extend | Metamagic.Reach | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Maximize | (Metamagic)MetamagicFeats.MetamagicExtender.IntensifiedGeneral  | (Metamagic)MetamagicFeats.MetamagicExtender.Rime | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental;

            fiery_runes.AddToSpellList(Helpers.alchemistSpellList, 2);
            fiery_runes.AddToSpellList(Helpers.magusSpellList, 2);
            fiery_runes.AddToSpellList(Helpers.druidSpellList, 2);
            fiery_runes.AddToSpellList(Helpers.wizardSpellList, 2);

            fiery_runes.AddSpellAndScroll("fbdd06f0414c3ef458eb4b2a8072e502");
        }


        static void createBladedDash()
        {
            var icon = library.Get<BlueprintAbility>("0087fc2d64b6095478bc7b8d7d512caf").Icon; //freedom of movement
            dimension_door_free = library.CopyAndAdd<BlueprintAbility>("a9b8be9b87865744382f7c64e599aeb2", "BladedDashTeleportAbility", "");
            dimension_door_free.ActionType = UnitCommand.CommandType.Free;
            dimension_door_free.CanTargetEnemies = true;
            dimension_door_free.CanTargetFriends = true;
            dimension_door_free.Type = AbilityType.Special;
            dimension_door_free.SetNameDescriptionIcon("", "", icon);

            var buff = Helpers.CreateBuff("BladedDashBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Helpers.Create<NewMechanics.AttackBonusMaxFromMultipleStats>(a => { a.stats = new StatType[] { StatType.Charisma, StatType.Intelligence }; a.descriptor = ModifierDescriptor.Circumstance; })
                                          );
            buff.SetBuffFlags(BuffFlags.HiddenInUi);
            bladed_dash = Helpers.CreateAbility("BladedDashAbility",
                                                "Bladed Dash",
                                                "When you cast this spell, you immediately move to any one target in close range, and make a single melee attack against it at your base attack bonus. You gain a circumstance bonus on your attack roll equal to your Intelligence or Charisma modifier, whichever is higher. Despite the name, the spell works with any melee weapon.",
                                                "",
                                                dimension_door_free.Icon,
                                                AbilityType.Spell,
                                                UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                "",
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionOnContextCaster(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1), dispellable: false)),
                                                                         Helpers.Create<ContextActionCastSpell>(c => c.Spell = dimension_door_free),
                                                                         Common.createContextActionAttack(Helpers.CreateActionList(Common.createContextActionOnContextCaster(Common.createContextActionRemoveBuff(buff))),
                                                                                                          Helpers.CreateActionList(Common.createContextActionOnContextCaster(Common.createContextActionRemoveBuff(buff)))
                                                                                                         )
                                                                         ),
                                                Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                                Helpers.Create<AbilityCasterMainWeaponIsMelee>()
                                                );

            bladed_dash.setMiscAbilityParametersSingleTargetRangedHarmful(false);
            bladed_dash.NeedEquipWeapons = true;
            bladed_dash.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken;
            bladed_dash.AddToSpellList(Helpers.magusSpellList, 2);
            bladed_dash.AddToSpellList(Helpers.bardSpellList, 2);

            bladed_dash.AddSpellAndScroll("f49fc4e47cef56e42a49d561289dd500");


            bladed_dash_greater = Helpers.CreateAbility("BladedDashAbilityGreater",
                                    "Bladed Dash, Greater",
                                    "This spell functions like bladed dash, save that you can make a single melee attack against every creature you pass during the 30 feet of your dash. You cannot attack an individual creature more than once with spell.\n"
                                    + bladed_dash.Name + ": " + bladed_dash.Description,
                                    "",
                                    dimension_door_free.Icon,
                                    AbilityType.Spell,
                                    UnitCommand.CommandType.Standard,
                                    AbilityRange.Projectile,
                                    "",
                                    "",
                                    Helpers.CreateRunActions(Helpers.CreateConditional(Helpers.Create<ContextConditionIsEnemy>(), 
                                                                                      new GameAction[]{Common.createContextActionOnContextCaster(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1), dispellable: false)),
                                                                                                       Helpers.CreateConditional(Helpers.Create<ContextConditionIsMainTarget>(),
                                                                                                                                 Helpers.Create<ContextActionCastSpell>(c => c.Spell = dimension_door_free)
                                                                                                                                ),
                                                                                                       Common.createContextActionAttack(),
                                                                                                       Common.createContextActionOnContextCaster(Common.createContextActionRemoveBuff(buff))
                                                                                                      }
                                                                                      )
                                                             ),
                                    library.Get<BlueprintAbility>("c3e84b5e4400f16459ae8f0585e8dc2b").GetComponent<AbilityDeliverProjectile>(), //air blast torrent
                                    Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                    Helpers.Create<AbilityCasterMainWeaponIsMelee>()
                                    );

            bladed_dash_greater.setMiscAbilityParametersSingleTargetRangedHarmful(false);
            bladed_dash_greater.NeedEquipWeapons = true;
            bladed_dash_greater.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken;
            bladed_dash_greater.AddToSpellList(Helpers.magusSpellList, 5);
            bladed_dash_greater.AddToSpellList(Helpers.bardSpellList, 5);

            bladed_dash_greater.AddSpellAndScroll("f49fc4e47cef56e42a49d561289dd500");
            bladed_dash_buff = buff;
        }


        static void createRebuke()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/Rebuke.png");

            var sonic_dmg1 = Helpers.CreateActionDealDamage(DamageEnergyType.Sonic, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D8), Helpers.CreateContextValue(AbilityRankType.Default)),
                            isAoE: true, halfIfSaved: true);
            sonic_dmg1.Half = true;

            var holy_dmg1 = Helpers.CreateActionDealDamage(DamageEnergyType.Divine, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D8), Helpers.CreateContextValue(AbilityRankType.Default)),
                isAoE: true, halfIfSaved: true);
            holy_dmg1.Half = true;

            var staggered = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");

            var effect1 = Helpers.CreateConditionalSaved(null, Common.createContextActionApplyBuff(staggered, Helpers.CreateContextDuration(1)));


            var sonic_dmg2 = Helpers.CreateActionDealDamage(DamageEnergyType.Sonic, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.DamageDice)),
                isAoE: true, halfIfSaved: true);
            sonic_dmg2.Half = true;

            var holy_dmg2 = Helpers.CreateActionDealDamage(DamageEnergyType.Divine, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.DamageDice)),
                isAoE: true, halfIfSaved: true);
            holy_dmg2.Half = true;

            var stunned = library.Get<BlueprintBuff>("09d39b38bb7c6014394b6daced9bacd3");

            var effect2 = Helpers.CreateConditionalSaved(null, Common.createContextActionApplyBuff(stunned, Helpers.CreateContextDuration(0, DurationRate.Rounds, DiceType.D4, 1)));


            var deities = library.Get<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4").AllFeatures;

            var effect = Helpers.CreateConditional(Helpers.Create<NewMechanics.ContextConditionCasterAndTargetHasFactFromList>(c => c.facts = deities),
                                                    new GameAction[] { sonic_dmg2, holy_dmg2, effect2 },
                                                    new GameAction[] { sonic_dmg1, holy_dmg1, effect1 }
                                                  );

            rebuke = Helpers.CreateAbility("RebukeAbility",
                                           "Rebuke",
                                           "Your wrathful words cause physical harm to your enemies.\n"
                                           + $"Your enemies take 1d{BalanceFixes.getDamageDieString(DiceType.D8)} points of damage per two caster levels (maximum 5d{BalanceFixes.getDamageDieString(DiceType.D8)}) and are staggered for 1 round. Half of this damage is sonic damage, but the other half results directly from divine power and is therefore not subject to being reduced by resistance to sonic - based attacks.Rebuke is especially devastating to foes who worship your god, inflicting 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage per caster level (maximum 10d{BalanceFixes.getDamageDieString(DiceType.D6)}) and stunning them for 1d4 rounds. A successful Fortitude save halves the damage and negates the staggering or stunning effect.",
                                           "",
                                           icon,
                                           AbilityType.Spell,
                                           UnitCommand.CommandType.Standard,
                                           AbilityRange.Personal,
                                           "",
                                           "Fortitude partial",
                                           Helpers.CreateRunActions(effect),
                                           Common.createAbilitySpawnFx("2483780330931b64f97cbb6bb7cbd352", position_anchor: AbilitySpawnFxAnchor.None, orientation_anchor: AbilitySpawnFxAnchor.None),
                                           Helpers.Create<SharedSpells.CannotBeShared>(),
                                           Helpers.Create<NewMechanics.AbilityTargetIsCaster>(),
                                           Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                           Helpers.CreateSpellDescriptor(SpellDescriptor.Sonic),
                                           Helpers.CreateContextRankConfig(max: 5, feature: MetamagicFeats.intensified_metamagic),
                                           Helpers.CreateContextRankConfig(max: 10, type: AbilityRankType.DamageDice, feature: MetamagicFeats.intensified_metamagic),
                                           Helpers.CreateAbilityTargetsAround(20.Feet(), TargetType.Enemy)
                                           );
            rebuke.setMiscAbilityParametersSelfOnly();
            rebuke.SpellResistance = true;
            rebuke.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | (Metamagic)MetamagicFeats.MetamagicExtender.IntensifiedGeneral  | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;

            rebuke.AddToSpellList(Helpers.inquisitorSpellList, 4);
            rebuke.AddSpellAndScroll("4c73f11f91ca3fb4a8af325686b660d8");                            
        }


        static void createFleshwormInfestation()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/FleshwormInfestation.png");
            var protection_from_evil = library.Get<BlueprintBuff>("4a6911969911ce9499bf27dde9bfcedc");
            var sickened = library.Get<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323");
            var staggered = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");

            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Unholy, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 1, 0));
            dmg.DamageType.Type = DamageType.Direct;

            var dex_dmg = Helpers.CreateActionDealDamage(StatType.Dexterity, Helpers.CreateContextDiceValue(DiceType.Zero, 0, 2));

            var effect = Helpers.CreateConditionalSaved(new GameAction[] { Common.createContextActionApplyBuff(sickened, Helpers.CreateContextDuration(1)) },
                                                         new GameAction[] { Common.createContextActionApplyBuff(staggered, Helpers.CreateContextDuration(1)), dmg, dex_dmg }
                                                       );
            var action = Helpers.CreateConditional(Common.createContextConditionHasFacts(false, Common.construct, Common.undead, Common.elemental),
                                                   null,
                                                   Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(effect))
                                                   );

            var conditional = Helpers.CreateConditional(Common.createContextConditionHasFact(protection_from_evil),
                                                    null,
                                                    action
                                                    );

            var buff = Helpers.CreateBuff("FleshwormInfestationBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          Common.createPrefabLink("fbf39991ad3f5ef4cb81868bb9419bff"),
                                          Helpers.CreateAddFactContextActions(activated: conditional, newRound: conditional)
                                          );

            var ability = Helpers.CreateAbility("FleshwormInfestationAbility",
                                                "Fleshworm Infestation",
                                                $"With a touch, you cause an infestation of ravenous worms to manifest in the target’s flesh. The target must make a Fortitude save every round. Failure means it takes 1d{BalanceFixes.getDamageDieString(DiceType.D6)} hit points of damage and 2 points of Dexterity damage, and is staggered for 1 round. If it makes the save, it takes no hit point or Dexterity damage and is only sickened for 1 round rather than staggered. Fleshworm infestation cannot be ended early by remove disease or heal, as the infestation starts anew if the current worms are slain. Protection from evil negates this spell’s effects for as long as the two durations overlap.",
                                                "",
                                                icon,
                                                AbilityType.Spell,
                                                UnitCommand.CommandType.Standard,
                                                AbilityRange.Touch,
                                                Helpers.roundsPerLevelDuration,
                                                "Fortitude partial",
                                                Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), is_from_spell: true)),
                                                Helpers.CreateContextRankConfig(),
                                                Common.createAbilityTargetHasFact(inverted: true, Common.undead, Common.construct, Common.elemental),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.Evil),
                                                Helpers.CreateSpellComponent(SpellSchool.Conjuration),
                                                Helpers.CreateDeliverTouch()
                                                );

            ability.AvailableMetamagic = Metamagic.Extend | Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Maximize | Metamagic.Reach | Metamagic.Empower | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            ability.setMiscAbilityParametersTouchHarmful();
            ability.SpellResistance = true;

            fleshworm_infestation = ability.CreateTouchSpellCast();
            fleshworm_infestation.AddComponent(Common.createAbilityTargetHasFact(inverted: true, Common.undead, Common.construct, Common.elemental));
            fleshworm_infestation.AddToSpellList(Helpers.clericSpellList, 4);
            fleshworm_infestation.AddToSpellList(Helpers.inquisitorSpellList, 4);
            fleshworm_infestation.AddToSpellList(Helpers.wizardSpellList, 4);

            fleshworm_infestation.AddSpellAndScroll("a9a0d65ec202e25478bcae4a87e844f9");//forced repentance
        }

        static void createGhoulTouch()
        {
            var icon = library.Get<BlueprintAbility>("989ab5c44240907489aba0a8568d0603").Icon; //bestow curse
            var sickened = library.CopyAndAdd<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323", "GhoulTouchSickened", "");
            sickened.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Poison));

            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("7ced0efa297bd5142ab749f6e33b112b", "GhoulTouchSickenedArea", "");
            var apply_buff = Common.createContextActionApplyBuff(sickened, Helpers.CreateContextDuration(), is_child: true, is_permanent: true);
            var action = Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(Helpers.CreateConditionalSaved(null, apply_buff)));

            var effect = Helpers.CreateConditional(Helpers.CreateConditionsCheckerOr(Common.createContextConditionHasFact(Common.undead),
                                                                                                                Common.createContextConditionHasFact(Common.construct),
                                                                                                                Common.createContextConditionHasFact(Common.elemental),
                                                                                                                Common.createContextConditionIsCaster()),
                                                   null,
                                                   action);

            var remove = Helpers.CreateConditional(Helpers.CreateConditionsCheckerOr(Common.createContextConditionHasFact(Common.undead),
                                                                                                    Common.createContextConditionHasFact(Common.construct),
                                                                                                    Common.createContextConditionHasFact(Common.elemental),
                                                                                                    Common.createContextConditionIsCaster()),
                                       null,
                                       Helpers.Create<ContextActionRemoveBuffSingleStack>(r => r.TargetBuff = sickened));



            area.ReplaceComponent<AbilityAreaEffectBuff>(Helpers.CreateAreaEffectRunAction(unitEnter: effect,
                                                                                           round: effect,
                                                                                           unitExit: remove)
                                                         );
            area.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Poison));
            //area.Fx = Common.createPrefabLink("e1278588b084bc344842635e44770c90"); //poison cloud

            var aura_buff = Helpers.CreateBuff("GhoulTouchAuraBuff",
                                                  "",
                                                  "",
                                                  "",
                                                  null,
                                                  null,
                                                  Common.createAddAreaEffect(area)
                                               );
            aura_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var buff = Helpers.CreateBuff("GhoulTouchBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          Common.createPrefabLink("fbf39991ad3f5ef4cb81868bb9419bff"),
                                          Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.Paralyzed),
                                          Helpers.CreateSpellDescriptor(SpellDescriptor.Paralysis | SpellDescriptor.MovementImpairing),
                                          Common.createAuraFeatureComponent(aura_buff)
                                          );
            var primary_effect = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(2, DurationRate.Rounds, DiceType.D6, 1));
            var primary_effect_saved = Helpers.CreateConditionalSaved(null, primary_effect);
            var ability = Helpers.CreateAbility("GhoulTouchAbility",
                                                "Ghoul Touch",
                                                "Imbuing you with negative energy, this spell allows you to paralyze a single living humanoid for the duration of the spell with a successful melee touch attack.\n" +
                                                "A paralyzed subject exudes a carrion stench that causes all living creatures (except you) in a 10 - foot - radius spread to become sickened (Fortitude negates). A neutralize poison spell removes the effect from a sickened creature, and creatures immune to poison are unaffected by the stench.This is a poison effect.",
                                                "",
                                                icon,
                                                AbilityType.Spell,
                                                UnitCommand.CommandType.Standard,
                                                AbilityRange.Touch,
                                                "1d6+2 rounds",
                                                Helpers.fortNegates,
                                                Helpers.CreateRunActions(SavingThrowType.Fortitude, primary_effect_saved),
                                                Common.createAbilitySpawnFx("cbfe312cb8e63e240a859efaad8e467c", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                Helpers.CreateDeliverTouch(),
                                                Helpers.CreateSpellComponent(SpellSchool.Necromancy)
                                               );
            var hold_person = library.Get<BlueprintAbility>("c7104f7526c4c524f91474614054547e");
            var checkers = hold_person.GetComponents<AbilityTargetHasNoFactUnless>().ToArray<BlueprintComponent>().AddToArray(hold_person.GetComponents<AbilityTargetHasFact>()).AddToArray(hold_person.GetComponents<AbilityTargetHasNoFactUnless>());

            ability.AddComponents(checkers);
            ability.setMiscAbilityParametersTouchHarmful();
            ability.SpellResistance = true;
            ability.AvailableMetamagic = Metamagic.Extend | Metamagic.Empower | Metamagic.Maximize | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;

            ghoul_touch = ability.CreateTouchSpellCast();

            ghoul_touch.AddToSpellList(Helpers.wizardSpellList, 2);
            ghoul_touch.AddSpellAndScroll("d1d24c5613bb8c14a9a089c54b77527d");
        }


        static void createForcefulStrike()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/ForcefulStrike.png");

            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Holy, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D4), Helpers.CreateContextValue(AbilityRankType.Default)), halfIfSaved: true);
            dmg.DamageType.Type = DamageType.Force;

            var bull_rush = Helpers.Create<ContextActionCombatManeuver>(c => { c.Type = Kingmaker.RuleSystem.Rules.CombatManeuver.BullRush; c.IgnoreConcealment = true; c.OnSuccess = Helpers.CreateActionList(); });

            var bull_rush_buff = Helpers.CreateBuff("ForcefullStrikeBullRushBuff",
                                                    "",
                                                    "",
                                                    "",
                                                    null,
                                                    null,
                                                    Helpers.Create<NewMechanics.ContextCombatManeuverBonus>(c => { c.Type = Kingmaker.RuleSystem.Rules.CombatManeuver.BullRush; c.Bonus = Helpers.CreateContextValue(AbilityRankType.StatBonus); }),
                                                    Helpers.CreateContextRankConfig(type: AbilityRankType.StatBonus)
                                                    );
            bull_rush_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var apply_bull_rush_buff = Common.createContextActionOnContextCaster(Common.createContextActionApplyBuff(bull_rush_buff, Helpers.CreateContextDuration(1), is_child: true));


            var effect = Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(dmg, Helpers.CreateConditionalSaved(null, bull_rush)));

            var buff = Helpers.CreateBuff("ForcefulStrikeBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(apply_bull_rush_buff, effect, Helpers.Create<ContextActionRemoveSelf>()), check_weapon_range_type: true),
                                          Helpers.CreateContextRankConfig(max: 10, feature: MetamagicFeats.intensified_metamagic)
                                          );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), is_from_spell: true);

            forceful_strike = Helpers.CreateAbility("ForcefulStrikeAbility",
                                                    "Forceful Strike",
                                                    $"You cast this spell as you strike a creature with a melee weapon, unarmed strike, or natural attack to unleash a concussive blast of force. You deal normal weapon damage from the blow, but also deal an additional amount of force damage equal to 1d{BalanceFixes.getDamageDieString(DiceType.D4)} points per caster level (maximum of 10d{BalanceFixes.getDamageDieString(DiceType.D4)}). The force of the blow may be enough to knock the target backward as well. To determine if the target is pushed back, make a combat maneuver check with a bonus equal to your caster level to resolve a bull rush attempt against the creature struck. You do not move as a result of this free bull rush, but it can push the target back if it defeats the target’s CMD. A successful Fortitude save halves the force damage and negates the bull rush effect.",
                                                    "",
                                                    icon,
                                                    AbilityType.Spell,
                                                    UnitCommand.CommandType.Swift,
                                                    AbilityRange.Personal,
                                                    "One minute or until discharged",
                                                    "Fortitude Partial",
                                                    Helpers.CreateRunActions(apply_buff),
                                                    Common.createAbilitySpawnFx("52d413df527f9fa4a8cf5391fd593edd", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                    Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                    Helpers.CreateSpellDescriptor(SpellDescriptor.Force),
                                                    Helpers.Create<AbilityCasterMainWeaponIsMelee>()
                                                    );
            forceful_strike.setMiscAbilityParametersSelfOnly();
            forceful_strike.AvailableMetamagic = Metamagic.Heighten | Metamagic.Empower | Metamagic.Maximize | (Metamagic)MetamagicFeats.MetamagicExtender.IntensifiedGeneral  | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Toppling | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent;

            forceful_strike.AddToSpellList(Helpers.clericSpellList, 4);
            forceful_strike.AddToSpellList(Helpers.magusSpellList, 4);
            forceful_strike.AddToSpellList(Helpers.inquisitorSpellList, 4);
            forceful_strike.AddToSpellList(Helpers.paladinSpellList, 4);

            forceful_strike.AddSpellAndScroll("cbf529be5408e9042822a3b22ca55bca");
        }

        static void createSpite()
        {
            var icon = library.Get<BlueprintFeature>("fb343ede45ca1a84496c91c190a847ff").Icon;

            var divination_buff = library.Get<BlueprintBuff>("6d338078b1a8cdc41bf3a39f65247161");

            var spite_give_ability_buff = Helpers.CreateBuff("SpiteGiveAbilityBuff",
                                                                   "",
                                                                   "",
                                                                   "",
                                                                   null,
                                                                   null);
            spite_give_ability_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var action_remove_spite_use = Helpers.CreateActionList(Common.createContextActionRemoveBuff(spite_give_ability_buff));
            var spite_store_buff = Helpers.CreateBuff("SpiteBuff",
                                                        "Spite",
                                                        "Choose a single touch range spell of 4th level or lower with a casting time of 1 standard action or less. As part of the action of casting spite, you cast the associated spell and bind it into a defensive ward in the form of a tattoo, birthmark, or wart somewhere upon your body. The next time you are hit by a melee attack or a combat maneuver is used successfully against you, the stored spell is triggered against your foe. You do not need to succeed on a touch attack to affect the target, but in all other respects the spell is treated as though you had cast it normally. If the attacking creature is not a valid target for the spell, the stored spell is lost with no effect.\n"
                                                        + "You can have only one spite spell in effect at a time; if you cast this spell a second time, the previous spell effect ends.",
                                                        "",
                                                        icon,
                                                        divination_buff.FxOnStart,
                                                        Helpers.Create<SpellManipulationMechanics.FactStoreSpell>(f => { f.actions_on_store = action_remove_spite_use; f.always_hit = true; }),
                                                        Helpers.CreateAddFactContextActions(Common.createContextActionApplyBuff(spite_give_ability_buff,
                                                                                                                                Helpers.CreateContextDuration(),
                                                                                                                                is_child: true, is_permanent: true)
                                                                                                                                )
                                                        );
            spite_store_buff.AddComponent(Helpers.Create<SpellManipulationMechanics.AddStoredSpellToCaption>(a => a.store_fact = spite_store_buff));
            var release_action = Helpers.Create<SpellManipulationMechanics.ReleaseSpellStoredInSpecifiedBuff>(r => r.fact = spite_store_buff);

            var release_buff = Helpers.CreateBuff("SpiteReleaseBuff",
                                                  "",
                                                  "",
                                                  "",
                                                  null,
                                                  null,
                                                  Common.createAddTargetAttackWithWeaponTrigger(Helpers.CreateActionList(Common.createContextActionRemoveBuff(spite_store_buff), Helpers.Create<ContextActionRemoveSelf>()),
                                                                                                Helpers.CreateActionList(release_action))
                                                 );
            release_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            spite_store_buff.AddComponent(Helpers.CreateAddFactContextActions(deactivated: Common.createContextActionRemoveBuff(release_buff)));

            var apply_release_buff = Common.createContextActionApplyBuff(release_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true);
            int max_variants = 10;
            Predicate<AbilityData> check_slot_predicate = delegate (AbilityData spell)
            {
                return spell.Spellbook != null
                        && spell.SpellLevel <= 4
                        && spell.Blueprint.StickyTouch != null
                        && (!spell.Blueprint.HasVariants || spell.Variants.Count < max_variants)
                        && (!spell.RequireMaterialComponent || spell.HasEnoughMaterialComponent)
                        && !SpellManipulationMechanics.FactStoreSpell.hasSpellStoredInFact(spell.Caster, spite_store_buff);
            };



            for (int i = 0; i < max_variants; i++)
            {
                var spite_use = Helpers.CreateAbility($"SpiteStoreAbility{i}",
                                                            "Spite: Store",
                                                            spite_store_buff.Description,
                                                            "",
                                                            spite_store_buff.Icon,
                                                            AbilityType.Spell,
                                                            Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                            AbilityRange.Personal,
                                                            "1 hour/level or until discharged",
                                                            "",
                                                            Helpers.CreateSpellComponent(SpellSchool.Abjuration),
                                                            Helpers.Create<SpellManipulationMechanics.AbilityStoreSpellInFact>(s =>
                                                                                                                                {
                                                                                                                                    s.fact = spite_store_buff;
                                                                                                                                    s.check_slot_predicate = check_slot_predicate;
                                                                                                                                    s.variant = i;
                                                                                                                                    s.actions = Helpers.CreateActionList(apply_release_buff);
                                                                                                                                }
                                                                                                                              ),
                                                            Helpers.CreateContextRankConfig()
                                                            );
                spite_give_ability_buff.AddComponent(Helpers.CreateAddFact(spite_use));
                spite_use.setMiscAbilityParametersSelfOnly();
                Common.setAsFullRoundAction(spite_use);
            }

            var apply_buff = Common.createContextActionApplyBuff(spite_store_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), rate: DurationRate.Hours), is_from_spell: true);
            spite = Helpers.CreateAbility("SpiteAbility",
                                                "Spite",
                                                spite_store_buff.Description,
                                                "",
                                                spite_store_buff.Icon,
                                                AbilityType.Spell,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Personal,
                                                "1 hour/level or until discharged",
                                                "",
                                                Helpers.Create<NewMechanics.AbilityTargetIsCaster>(),
                                                Helpers.CreateRunActions(apply_buff),
                                                Helpers.CreateContextRankConfig(),
                                                Helpers.CreateSpellComponent(SpellSchool.Abjuration)
                                                );
            spite.MaterialComponent = library.Get<BlueprintAbility>("c66e86905f7606c4eaa5c774f0357b2b").MaterialComponent; //same as for stoneskin
            spite.setMiscAbilityParametersSelfOnly();
            Common.setAsFullRoundAction(spite);
            spite.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken | Metamagic.Extend;
            //spite.AddSpellAndScroll("beab337b352b5ac479698e2bbc08f4ce"); //circle of death
        }


        static void createHowlingAgony()
        {
            var icon = library.Get<BlueprintAbility>("d42c6d3f29e07b6409d670792d72bc82").Icon; //banshee blast
            var buff = Helpers.CreateBuff("HowlingAgonyBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Helpers.CreateAddStatBonus(StatType.SaveReflex, -2, ModifierDescriptor.UntypedStackable),
                                          Helpers.CreateAddStatBonus(StatType.AdditionalAttackBonus, -2, ModifierDescriptor.UntypedStackable),
                                          Helpers.CreateAddStatBonus(StatType.AC, -2, ModifierDescriptor.UntypedStackable),
                                          Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.SpellCastingIsDifficult),
                                          Helpers.Create<WeaponAttackTypeDamageBonus>(w => { w.AttackBonus = 1; w.Value = -2; w.Descriptor = ModifierDescriptor.UntypedStackable; w.Type = AttackTypeAttackBonus.WeaponRangeType.Melee; }),
                                          Helpers.CreateSpellDescriptor(SpellDescriptor.Death)
                                          );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), is_from_spell: true);
            howling_agony = Helpers.CreateAbility("HowlingAgonyAbility",
                                                  "Howling Agony",
                                                  "You send wracking pains through the targets’ bodies. Because of the pain, affected creatures take a –2 penalty to AC, attacks, melee damage rolls, and Reflex saving throws, and must succeed at a concentration check (DC 15 + spell level) to cast spells.",
                                                  "",
                                                  icon,
                                                  AbilityType.Spell,
                                                  UnitCommand.CommandType.Standard,
                                                  AbilityRange.Close,
                                                  Helpers.roundsPerLevelDuration,
                                                  Helpers.fortNegates,
                                                  Helpers.CreateRunActions(SavingThrowType.Fortitude, Helpers.CreateConditionalSaved(null, apply_buff)),
                                                  Common.createAbilitySpawnFx("cbfe312cb8e63e240a859efaad8e467c", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                  Helpers.CreateAbilityTargetsAround(30.Feet(), TargetType.Enemy),
                                                  Helpers.CreateSpellDescriptor(SpellDescriptor.Death),
                                                  Helpers.CreateSpellComponent(SpellSchool.Necromancy)
                                                  );
            howling_agony.SpellResistance = true;
            howling_agony.setMiscAbilityParametersRangedDirectional();
            howling_agony.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Reach | Metamagic.Quicken | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;

            howling_agony.AddToSpellList(Helpers.inquisitorSpellList, 2);
            howling_agony.AddToSpellList(Helpers.wizardSpellList, 3);
            howling_agony.AddSpellAndScroll("f6073b26d3e1d61418f62928f34b601f");
        }


        static void createFreezingSphere()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/FreezingSphere.png");

            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Cold, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.Default)), true, true);
            var dmg_water = Helpers.CreateActionDealDamage(DamageEnergyType.Cold, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D8), Helpers.CreateContextValue(AbilityRankType.Default)), true, true);

            var stagger_buff = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");
            var apply_staggered = Common.createContextActionApplyBuff(stagger_buff, Helpers.CreateContextDuration(0, DurationRate.Rounds, DiceType.D4, 1));
            var water_subtype = library.Get<BlueprintFeature>("bf7ee56ec9e43c14fa17727997e91993");
            var effect = Helpers.CreateConditional(Common.createContextConditionHasFact(water_subtype), new GameAction[] { dmg_water, apply_staggered}, new GameAction[] { dmg });

            var projectile = library.Get<BlueprintProjectile>("99be5f02870297b48b0342ba44156dc2");


            freezing_sphere = Helpers.CreateAbility("FreezingSphereAbility",
                                                    "Freezing Sphere",
                                                    $"Freezing sphere creates a frigid globe of cold energy that streaks from your fingertips to the location you select, where it explodes in a 40-foot-radius burst, dealing 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of cold damage per caster level (maximum 15d{BalanceFixes.getDamageDieString(DiceType.D6)}) to each creature in the area. A creature of the water subtype instead takes 1d{BalanceFixes.getDamageDieString(DiceType.D8)} points of cold damage per caster level (maximum 15d{BalanceFixes.getDamageDieString(DiceType.D8)}) and is staggered for 1d4 rounds.",
                                                    "",
                                                    icon,
                                                    AbilityType.Spell,
                                                    UnitCommand.CommandType.Standard,
                                                    AbilityRange.Long,
                                                    "",
                                                    Helpers.reflexHalfDamage,
                                                    Helpers.CreateRunActions(SavingThrowType.Reflex, effect),
                                                    Common.createAbilityDeliverProjectile(AbilityProjectileType.Simple, projectile, 5.Feet(), 5.Feet()),
                                                    Helpers.CreateAbilityTargetsAround(30.Feet(), TargetType.Any),
                                                    Helpers.CreateSpellDescriptor(SpellDescriptor.Cold),
                                                    Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                    Helpers.CreateContextRankConfig(max: 15, feature: MetamagicFeats.intensified_metamagic)
                                                    );
            freezing_sphere.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | (Metamagic)MetamagicFeats.MetamagicExtender.IntensifiedGeneral  | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Rime | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Selective | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            freezing_sphere.setMiscAbilityParametersRangedDirectional();
            freezing_sphere.SpellResistance = true;

            freezing_sphere.AddToSpellList(Helpers.magusSpellList, 6);
            freezing_sphere.AddToSpellList(Helpers.wizardSpellList, 6);

            freezing_sphere.AddSpellAndScroll("66fc961f9c39ae94fb87a79adc87212e");
        }

   



        static void createHoldMonsterMass()
        {
            var hold_monster = library.Get<BlueprintAbility>("41e8a952da7a5c247b3ec1c2dbb73018");

            var checker_fact = hold_monster.GetComponents<AbilityTargetHasNoFactUnless>().ToArray();
            var does_not_work = hold_monster.GetComponent<AbilityTargetHasFact>();

            hold_monster_mass = library.CopyAndAdd<BlueprintAbility>("41e8a952da7a5c247b3ec1c2dbb73018", "HoldMonsterMassAbility", "");
            hold_monster_mass.RemoveComponents<SpellListComponent>();
            hold_monster_mass.RemoveComponents<AbilityTargetHasNoFactUnless>();
            hold_monster_mass.RemoveComponents<AbilityTargetHasFact>();
            hold_monster_mass.RemoveComponents<RecommendationNoFeatFromGroup>();
            hold_monster_mass.setMiscAbilityParametersRangedDirectional();
            hold_monster_mass.AddComponent(Helpers.CreateAbilityTargetsAround(15.Feet(), TargetType.Enemy));

            hold_monster_mass.SetName("Hold Monster, Mass");
            hold_monster_mass.SetDescription("This spell functions like hold monster, except as noted above.\n" + hold_monster.Description);

            var action = Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Common.createContextConditionHasFacts(false, checker_fact[0].CheckedFacts), Common.createContextConditionCasterHasFact(checker_fact[0].UnlessFact, has: false)),
                                                   null,
                                                   Helpers.CreateConditional(Common.createContextConditionHasFacts(false, does_not_work.CheckedFacts),
                                                                                                        null,
                                                                                                        hold_monster.GetComponent<AbilityEffectRunAction>().Actions.Actions[0]
                                                                                                        )
                                                   );

            hold_monster_mass.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(action));
            hold_monster_mass.setMiscAbilityParametersRangedDirectional();
            hold_monster_mass.AddToSpellList(Helpers.wizardSpellList, 9);
            hold_monster_mass.AddSpellAndScroll("d8c52df98e5815d4aa9d2bd2b73b88c9");
        }

        static void createIrresistibleDance()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/IrresistibleDance.png");
            var buff = Helpers.CreateBuff("IrresisitbleDanceBuff",
                                          "Irresistible Dance",
                                          "The subject feels an undeniable urge to dance and begins doing so, complete with foot shuffling and tapping. The spell effect makes it impossible for the subject to do anything other than caper and prance in place. The effect imposes a -4 penalty to Armor Class and a -10 penalty on Reflex saves, and it negates any Armor Class bonus granted by a shield the target holds. The dancing subject provokes attacks of opportunity each round on its turn. A successful Will save reduces the duration of this effect to 1 round.",
                                          "",
                                          icon,
                                          Common.createPrefabLink("602fa850c4a94d84eb8aa1bcc0d008c7"),
                                          Helpers.CreateAddStatBonus(StatType.SaveReflex, -10, ModifierDescriptor.UntypedStackable),
                                          Helpers.CreateAddStatBonus(StatType.AC, -4, ModifierDescriptor.UntypedStackable),
                                          Common.createBuffStatusCondition(Kingmaker.UnitLogic.UnitCondition.Confusion),
                                          Helpers.Create<ConfusionControl.ControlConfusionBuff>(a => a.allowed_states = new Kingmaker.UnitLogic.Parts.ConfusionState[] {Kingmaker.UnitLogic.Parts.ConfusionState.DoNothing}),
                                          Helpers.Create<NewMechanics.RemoveShieldACBonus>(),
                                          Helpers.CreateAddFactContextActions(newRound: Helpers.Create<ContextActionProvokeAttackOfOpportunity>()),
                                          Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting)
                                          );

            var effect_saved = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1), is_from_spell: true);
            var effect = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1, diceType: DiceType.D4, diceCount: 1), is_from_spell: true);

            var ability = Helpers.CreateAbility("IrresistibleDanceAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                icon,
                                                AbilityType.Spell,
                                                UnitCommand.CommandType.Standard,
                                                AbilityRange.Touch,
                                                "1d4 + 1 rounds",
                                                "Will partial",
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Compulsion),
                                                Helpers.CreateSpellComponent(SpellSchool.Enchantment),
                                                Helpers.CreateDeliverTouch(),
                                                Helpers.CreateRunActions(SavingThrowType.Will, Helpers.CreateConditionalSaved(effect_saved, effect))
                                                );
            var dominate_monster = library.Get<BlueprintAbility>("3c17035ec4717674cae2e841a190e757");
            ability.AddComponents(dominate_monster.GetComponents<AbilityTargetHasFact>());
            ability.AddComponents(dominate_monster.GetComponents<AbilityTargetHasNoFactUnless>());
            ability.setMiscAbilityParametersTouchHarmful();
            ability.SpellResistance = true;
            ability.AvailableMetamagic = Metamagic.Extend | Metamagic.Empower | Metamagic.Heighten | Metamagic.Reach | Metamagic.Quicken | Metamagic.Maximize | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            irresistible_dance = Helpers.CreateTouchSpellCast(ability);

            irresistible_dance.AddToSpellList(Helpers.bardSpellList, 6);
            irresistible_dance.AddToSpellList(Helpers.wizardSpellList, 8);

            irresistible_dance.AddSpellAndScroll("aed1e536ad8851947a3d37644fa87c03");
        }

        static void createParticulateForm()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/ParticulateForm.png");

            var buff = Helpers.CreateBuff("ParticulateFormBuff",
                                          "Particulate Form",
                                          $"The targets’ physical forms undergo a bizarre transformation. They look and function normally, but are composed of countless particles that separate and reconnect to remain whole. Each target gains fast healing 1 and is immune to bleed damage, critical hits, sneak attacks, and other forms of precision damage. The value of this fast healing increases by 1 at caster levels 10th, 15th, and 20th. Any target can end the spell effect on itself as a swift action; the target then regains 5d{BalanceFixes.getDamageDieString(DiceType.D6)} hit points.",
                                          "",
                                          icon,
                                          null,
                                          Common.createAddContextEffectFastHealing(Helpers.CreateContextValue(AbilityRankType.StatBonus)),
                                          Helpers.Create<AddImmunityToPrecisionDamage>(),
                                          Helpers.Create<AddImmunityToCriticalHits>(),
                                          Common.createBuffDescriptorImmunity(SpellDescriptor.Bleed),
                                          Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Bleed),
                                          Helpers.CreateContextRankConfig(type: AbilityRankType.StatBonus, progression: ContextRankProgression.StartPlusDivStep, startLevel: 5, stepLevel: 5)
                                          );

            var particualte_form_swift = Helpers.CreateAbility("ParticulateFormSwiftAction",
                                                               "Particulate Form: Dismiss",
                                                               buff.Description,
                                                               "",
                                                               icon,
                                                               AbilityType.Special,
                                                               UnitCommand.CommandType.Swift,
                                                               AbilityRange.Personal,
                                                               "",
                                                               "",
                                                               Helpers.CreateRunActions(Common.createContextActionHealTarget(Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 5)),
                                                                                        Common.createContextActionRemoveBuff(buff)
                                                                                        )
                                                               );
            buff.AddComponent(Helpers.CreateAddFact(particualte_form_swift));
            particulate_form = Helpers.CreateAbility("ParticulateFormAbility",
                                                      buff.Name,
                                                      buff.Description,
                                                      "",
                                                      buff.Icon,
                                                      AbilityType.Spell,
                                                      UnitCommand.CommandType.Standard,
                                                      AbilityRange.Close,
                                                      Helpers.roundsPerLevelDuration,
                                                      "",
                                                      Common.createAbilitySpawnFx("352469f228a3b1f4cb269c7ab0409b8e", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                      Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                                      Helpers.CreateAbilityTargetsAround(30.Feet(), TargetType.Ally),
                                                      Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), is_from_spell: true)),
                                                      Helpers.CreateContextRankConfig()
                                                      );
            particulate_form.setMiscAbilityParametersRangedDirectional();
            particulate_form.AvailableMetamagic = Metamagic.Reach | Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken;
            particulate_form.AddToSpellList(Helpers.clericSpellList, 7);
            particulate_form.AddToSpellList(Helpers.wizardSpellList, 7);
            particulate_form.AddSpellAndScroll("f157e6a94573ff94694f114b119229da");
        }


        static void createHoldPersonMass()
        {
            var hold_person = library.Get<BlueprintAbility>("c7104f7526c4c524f91474614054547e");

            var checker_fact = hold_person.GetComponents<AbilityTargetHasNoFactUnless>().ToArray();
            var does_not_work = hold_person.GetComponent<AbilityTargetHasFact>();

            hold_person_mass = library.CopyAndAdd<BlueprintAbility>("c7104f7526c4c524f91474614054547e", "HoldPersonMassAbility", "");
            hold_person_mass.RemoveComponents<SpellListComponent>();
            hold_person_mass.RemoveComponents<AbilityTargetHasNoFactUnless>();
            hold_person_mass.RemoveComponents<AbilityTargetHasFact>();
            hold_person_mass.RemoveComponents<RecommendationNoFeatFromGroup>();
            hold_person_mass.setMiscAbilityParametersRangedDirectional();
            hold_person_mass.AddComponents(Helpers.CreateAbilityTargetsAround(15.Feet(), TargetType.Enemy));

            hold_person_mass.SetName("Hold Person, Mass");
            hold_person_mass.SetDescription("This spell functions like hold person, except as noted above.\n" + hold_person.Description);

            var action = Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Common.createContextConditionHasFacts(false, checker_fact[0].CheckedFacts), Common.createContextConditionCasterHasFact(checker_fact[0].UnlessFact, has: false)),
                                                                                null,
                                                                                Helpers.CreateConditional(Common.createContextConditionHasFacts(false, does_not_work.CheckedFacts),
                                                                                                        null,
                                                                                                        hold_person.GetComponent<AbilityEffectRunAction>().Actions.Actions[0]
                                                                                )
                                                  );


            hold_person_mass.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(action));
            hold_person_mass.setMiscAbilityParametersRangedDirectional();
            hold_person_mass.AddToSpellList(Helpers.wizardSpellList, 7);
            hold_person_mass.AddSpellAndScroll("e236e280f8be487428dcc09fe44dd5fd");
        }

        static void createCurseMajor()
        {
            var bestow_curse = library.Get<BlueprintAbility>("989ab5c44240907489aba0a8568d0603");

            List<BlueprintAbility> variants = new List<BlueprintAbility>();

            var description = "This spell functions as bestow curse, except the DC to remove the curse is equal to the save DC +5.\n"
                              + bestow_curse.Description;


            var cl_bonus = Helpers.CreateBuff("BestowCurseCLBonusBuff",
                                              "",
                                              "",
                                              "",
                                              null,
                                              null,
                                              Helpers.Create<IncreaseCasterLevel>(i => i.Value = 5)
                                              );


            foreach (var v in bestow_curse.GetComponent<AbilityVariants>().Variants)
            {
                var ability = library.CopyAndAdd<BlueprintAbility>(v.StickyTouch.TouchDeliveryAbility, v.StickyTouch.TouchDeliveryAbility.name + "Major", "");
                ability.SetName(ability.Name + ", Major");
                ability.RemoveComponents<AbilityDeliverTouch>();
                ability.Range = AbilityRange.Close;

                var apply_buff = new GameAction[] { Common.createContextActionOnContextCaster(Common.createContextActionApplyBuff(cl_bonus, Helpers.CreateContextDuration(1))) };
                var remove_buff = Common.createContextActionOnContextCaster(Common.createContextActionRemoveBuff(cl_bonus));

                ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(apply_buff.AddToArray(a.Actions.Actions).AddToArray(remove_buff)));
                ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);
                variants.Add(ability);
            }


            curse_major = Common.createVariantWrapper("CurseMajorAbility", "", variants.ToArray());

            curse_major.AddComponents(Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                      Helpers.CreateSpellDescriptor(SpellDescriptor.Curse)
                                      );
            curse_major.SetName("Curse, Major");
            curse_major.SetDescription(description);
            curse_major.AddToSpellList(Helpers.clericSpellList, 5);
            curse_major.AddToSpellList(Helpers.wizardSpellList, 6);

            curse_major.AddSpellAndScroll("01bd2a92d4112864b871995c93456b54");
        }


        static void createFluidForm()
        {
            var icon = library.Get<BlueprintAbility>("40681ea748d98f54ba7f5dc704507f39").Icon;//charged water blast
            var water_subtype = library.Get<BlueprintFeature>("bf7ee56ec9e43c14fa17727997e91993");
            var buff = Helpers.CreateBuff("FluidFormBuff",
                                            "Fluid Form",
                                            "When you cast this spell, your body takes on a slick, oily appearance. For the duration of this spell, your form can stretch and shift with ease and becomes slightly transparent, as if you were composed of liquid. This transparency is not enough to grant concealment.You gain DR 10/slashing and your reach increases by 10 feet.",
                                            "",
                                            icon,
                                            Common.createPrefabLink("9e2750fa744d28d4c95b9c72cc94868d"),
                                            Common.createContextFormDR(10, PhysicalDamageForm.Slashing),
                                            Helpers.CreateAddStatBonus(StatType.Reach, 8, ModifierDescriptor.Enhancement),
                                            Helpers.CreateAddFact(water_subtype)
                                            );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), is_from_spell: true);

            fluid_form = Helpers.CreateAbility("FluidFormAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Spell,
                                                UnitCommand.CommandType.Standard,
                                                AbilityRange.Personal,
                                                Helpers.minutesPerLevelDuration,
                                                "",
                                                Helpers.CreateRunActions(apply_buff),
                                                Helpers.CreateSpellComponent(SpellSchool.Transmutation)
                                                );
            fluid_form.setMiscAbilityParametersSelfOnly();
            fluid_form.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken;

            fluid_form.AddToSpellList(Helpers.wizardSpellList, 6);
            fluid_form.AddToSpellList(Helpers.alchemistSpellList, 4);
            fluid_form.AddSpellAndScroll("de05d46f44c8439488a8bbcc0059c09f"); //icy prison
        }


        static void createSuffocation()
        {
            var icon = library.Get<BlueprintAbility>("cc0aeb74b35cb7147bff6c53538bbc76").Icon; //foreced repentance
            var staggered = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");
            var apply_staggered = Common.createContextActionApplyBuff(staggered, Helpers.CreateContextDuration(), true, true, true, false);

            BlueprintBuff[] marker_buffs = new BlueprintBuff[2]; //0 hp, -1 hp
            GameAction[] apply_marker_buff = new GameAction[2];
            for (int i = 0; i < marker_buffs.Length; i++)
            {
                marker_buffs[i] = Helpers.CreateBuff($"SuffocationMarkerBuff{i + 1}Buff",
                                                     "",
                                                     "",
                                                     "",
                                                     null,
                                                     null);
                marker_buffs[i].SetBuffFlags(BuffFlags.HiddenInUi);
                apply_marker_buff[i] = Common.createContextActionApplyBuff(marker_buffs[i], Helpers.CreateContextDuration(), true, true, true, false);
            }

            var new_round = Helpers.CreateConditional(Helpers.CreateConditionHasFact(marker_buffs[1]), Helpers.Create<ContextActionKillTarget>(),
                                                   Helpers.CreateConditional(Helpers.CreateConditionHasFact(marker_buffs[0]),
                                                                                                            new GameAction[] { apply_marker_buff[1],
                                                                                                                               Common.createContextActionRemoveBuff(marker_buffs[0]),
                                                                                                                               Helpers.Create<NewMechanics.ReduceHpToValue>(r => r.value = -1)
                                                                                                                             },
                                                                                                            new GameAction[] { apply_marker_buff[0],
                                                                                                                               Helpers.Create<NewMechanics.ReduceHpToValue>(r => r.value = 0)
                                                                                                                             }
                                                                            )
                                                   );         
            var new_round_saved = Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(Helpers.CreateConditionalSaved(null, new_round)));
            var new_round_after = Helpers.CreateConditional(Helpers.Create<BuffConditionCheckRoundNumber>(b => b.RoundNumber = 0), null, new_round_saved);

            suffocation_buff = Helpers.CreateBuff("SuffocationBuff",
                                          "",
                                          "",
                                          "",
                                          icon,
                                          null,
                                          Helpers.CreateAddFactContextActions(activated: apply_staggered, newRound: new_round_after),
                                          Helpers.CreateSpellDescriptor(SpellDescriptor.Death)
                                          );


            var fast_death = Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(Helpers.CreateConditionalSaved(null, Helpers.Create<NewMechanics.ReduceHpToValue>(r => r.value = -1))));
            fast_suffocation_buff = Helpers.CreateBuff("FastSuffocationBuff",
                                                      "",
                                                      "",
                                                      "",
                                                      icon,
                                                      null,
                                                      Helpers.CreateAddFactContextActions(activated: Helpers.Create<NewMechanics.ReduceHpToValue>(r => r.value = 0), 
                                                                                          newRound: Helpers.CreateConditional(Helpers.Create<BuffConditionCheckRoundNumber>(b => b.RoundNumber = 0), null, fast_death)
                                                                                          ),
                                                      Helpers.CreateSpellDescriptor(SpellDescriptor.Death)
                                                      );

            var effect = Helpers.CreateConditionalSaved(Common.createContextActionApplyBuff(staggered, Helpers.CreateContextDuration(1, DurationRate.Rounds), is_from_spell: true),
                                                        Common.createContextActionApplyBuff(suffocation_buff, Helpers.CreateContextDuration(3, DurationRate.Rounds), is_from_spell: true));

            var effect_mass = Helpers.CreateConditionalSaved(Common.createContextActionApplyBuff(staggered, Helpers.CreateContextDuration(1, DurationRate.Rounds), is_from_spell: true),
                                            Common.createContextActionApplyBuff(suffocation_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Rounds), is_from_spell: true));

            var undead = library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");
            var construct = library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f");
            var elemental = library.Get<BlueprintFeature>("198fd8924dabcb5478d0f78bd453c586");

            suffocation = Helpers.CreateAbility("SuffocationAbility",
                                                "Suffocation",
                                                "This spell extracts the air from the target’s lungs, causing swift suffocation.\n"
                                                + "The target can attempt to resist this spell’s effects with a Fortitude save - if he succeeds, he is merely staggered for 1 round as he gasps for breath. If the target fails, he immediately begins to suffocate. On the target’s next turn, he falls unconscious and is reduced to 0 hit points. One round later, the target drops to - 1 hit points and is dying. One round after that, the target dies. Each round, the target can delay that round’s effects from occurring by making a successful Fortitude save, but the spell continues for 3 rounds, and each time a target fails his Fortitude save, he moves one step further along the track to suffocation. This spell only affects living creatures that must breathe. It is impossible to defeat the effects of this spell by simply holding one’s breath -if the victim fails the initial Saving Throw, the air in his lungs is extracted.",
                                                "",
                                                icon,
                                                AbilityType.Spell,
                                                UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                "3 rounds",
                                                Helpers.fortNegates,
                                                Helpers.CreateRunActions(SavingThrowType.Fortitude, effect),
                                                Common.createAbilityTargetHasFact(true, undead),
                                                Common.createAbilityTargetHasFact(true, construct),
                                                Common.createAbilityTargetHasFact(true, elemental),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.Death),
                                                Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                Common.createAbilitySpawnFx("cbfe312cb8e63e240a859efaad8e467c", anchor: AbilitySpawnFxAnchor.SelectedTarget)
                                                );
            suffocation.setMiscAbilityParametersSingleTargetRangedHarmful(true);

            suffocation.AvailableMetamagic = Metamagic.Extend | Metamagic.Quicken | Metamagic.Reach | Metamagic.Heighten | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            suffocation.SpellResistance = true;


            mass_suffocation = Helpers.CreateAbility("SuffocationMassAbility",
                                    "Suffocation, Mass",
                                    "This spell functions as suffocation. Note that the duration of this spell is much longer, forcing those suffering from the effect to make far more Fortitude saves to stave off eventual suffocation.\n"
                                    + suffocation.Description,
                                    "",
                                    icon,
                                    AbilityType.Spell,
                                    UnitCommand.CommandType.Standard,
                                    AbilityRange.Close,
                                    Helpers.roundsPerLevelDuration,
                                    Helpers.fortNegates,
                                    Helpers.CreateRunActions(SavingThrowType.Fortitude, effect_mass),
                                    Helpers.CreateSpellDescriptor(SpellDescriptor.Death),
                                    Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                    Common.createAbilitySpawnFx("cbfe312cb8e63e240a859efaad8e467c", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                    Helpers.CreateContextRankConfig(),
                                    Helpers.CreateAbilityTargetsAround(30.Feet(), TargetType.Enemy)
                                    );
            mass_suffocation.setMiscAbilityParametersSingleTargetRangedHarmful(true);

            mass_suffocation.AvailableMetamagic = Metamagic.Extend | Metamagic.Quicken | Metamagic.Reach | Metamagic.Heighten | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;

            suffocation.AddToSpellList(Helpers.wizardSpellList, 5);
            suffocation.AddSpellAndScroll("a9a0d65ec202e25478bcae4a87e844f9"); //force repentance


            mass_suffocation.AddToSpellList(Helpers.wizardSpellList, 9);
            mass_suffocation.AddSpellAndScroll("a9a0d65ec202e25478bcae4a87e844f9"); //force repentance
            mass_suffocation.SpellResistance = true;
        }


        static void createFireSeeds()
        {
            var icon = library.Get<BlueprintAbility>("f72f8f03bf0136c4180cd1d70eb773a5").Icon;
            List<BlueprintAbility> abilities = new List<BlueprintAbility>();
            var resource = Helpers.CreateAbilityResource("FireSeedsResource", "", "", "", null);
            resource.SetFixedResource(30);

            var alchemist_bomb = library.Get<BlueprintAbility>("5fa0111ac60ed194db82d3110a9d0352");
            var description = $"Acorns turn into special thrown splash weapons. An acorn grenade has a range of 25 feet. A ranged touch attack roll is required to strike the intended target. Together, the acorns are capable of dealing 1d{BalanceFixes.getDamageDieString(DiceType.D4)} points of fire damage per caster level (maximum 20d{BalanceFixes.getDamageDieString(DiceType.D4)}) divided among the acorns as you wish. No acorn can deal more than 10d{BalanceFixes.getDamageDieString(DiceType.D4)} points of damage.\n"
                              + "Each acorn grenade explodes upon striking any hard surface. In addition to its regular fire damage, all creatures adjacent to the explosion take 1 point of fire damage per die of the explosion.This explosion of fire ignites any combustible materials adjacent to the target.";

            for (int i = 1; i <= 10; i++)
            {
                var primary_damage = Helpers.CreateActionDealDamage(DamageEnergyType.Fire, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D4), i));
                var secondary_damage = Helpers.CreateActionDealDamage(DamageEnergyType.Fire, Helpers.CreateContextDiceValue(DiceType.Zero, 0, i));

                var conditional = Helpers.CreateConditional(Helpers.Create<ContextConditionIsMainTarget>(), new GameAction[] { primary_damage, secondary_damage }, new GameAction[] { secondary_damage });

                var ability = Helpers.CreateAbility($"FireSeeds{i}Ability",
                                                    $"Fire Seeds ({i}d{BalanceFixes.getDamageDieString(DiceType.D4)})",
                                                    description,
                                                    "",
                                                    icon,
                                                    AbilityType.SpellLike,
                                                    UnitCommand.CommandType.Standard,
                                                    AbilityRange.Close,
                                                    "",
                                                    Helpers.savingThrowNone,
                                                    Helpers.CreateResourceLogic(resource, amount: i),
                                                    alchemist_bomb.GetComponent<AbilityDeliverProjectile>(),
                                                    Helpers.CreateRunActions(conditional),
                                                    alchemist_bomb.GetComponent<AbilityTargetsAround>(),
                                                    Helpers.Create<AbilityEffectMiss>(a => { a.MissAction = Helpers.CreateActionList(secondary_damage); a.UseTargetSelector = true; }),
                                                    Helpers.CreateSpellDescriptor(SpellDescriptor.Bomb),
                                                    Helpers.Create<AbilityCasterNotPolymorphed>()
                                                    );
                ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental | (Metamagic)MetamagicFeats.MetamagicExtender.Selective | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing;
                ability.setMiscAbilityParametersSingleTargetRangedHarmful(true, animation: Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Thrown);
                abilities.Add(ability);
            }

            var wrapper = Common.createVariantWrapper("FireSeedsAbility", "", abilities.ToArray());
            wrapper.SetName("Fire Seeds");

            var buff = Helpers.CreateBuff("FireSeedsBuff",
                                          wrapper.Name,
                                          wrapper.Description,
                                          "",
                                          wrapper.Icon,
                                          null,
                                          Helpers.CreateAddAbilityResourceNoRestore(resource),
                                          Helpers.CreateAddFact(wrapper),
                                          Helpers.CreateAddFactContextActions(newRound: Helpers.CreateConditional(Helpers.Create<ResourceMechanics.ContextConditionTargetHasEnoughResource>(c => c.resource = resource),
                                                                                                                  null,
                                                                                                                  Helpers.Create<ContextActionRemoveSelf>()
                                                                                                                  )
                                                                             )
                                          //Helpers.Create<ReplaceAbilityParamsWithContext>(r => r.Ability = wrapper)
                                         );
            buff.SetBuffFlags(BuffFlags.RemoveOnRest);
            foreach (var a in abilities)
            {
                buff.AddComponent(Helpers.Create<ReplaceAbilityParamsWithContext>(r => r.Ability = a));
            }

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.TenMinutes), is_from_spell: true);

            fire_seeds = Helpers.CreateAbility("FireSeedsCastAbility",
                                               buff.Name,
                                               buff.Description,
                                               "",
                                               icon,
                                               AbilityType.Spell,
                                               UnitCommand.CommandType.Standard,
                                               AbilityRange.Touch,
                                               Helpers.tenMinPerLevelDuration,
                                               Helpers.savingThrowNone,
                                               Helpers.CreateRunActions(apply_buff,
                                                                        Helpers.Create<ResourceMechanics.ContextRestoreResource>(c =>
                                                                        {
                                                                            c.amount = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                            c.Resource = resource;
                                                                        })
                                                                        ),
                                               Common.createAbilitySpawnFx("930c1a4aa129b8344a40c8c401d99a04", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                               Helpers.CreateContextRankConfig(),
                                               Helpers.CreateSpellComponent(SpellSchool.Conjuration),
                                               Helpers.CreateSpellDescriptor(SpellDescriptor.Fire)
                                               );

            fire_seeds.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach | Metamagic.Extend | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental | (Metamagic)MetamagicFeats.MetamagicExtender.Selective | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing;
            fire_seeds.setMiscAbilityParametersTouchFriendly();

            fire_seeds.AddToSpellList(Helpers.druidSpellList, 6);
            fire_seeds.AddSpellAndScroll("4b0ff254dca06894cba7eace7eef6bfe");

            //replace 6th level spell in fire domain
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("881b2137a1779294c8956fe5b497cc35"), fire_seeds, 6);
            //replace 6th level spell in sun domain
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("c85c8791ee13d4c4ea10d93c97a19afc"), fire_seeds, 6);

        }


        static void createIceBody()
        {
            var icon = library.Get<BlueprintAbility>("65e8d23aef5e7784dbeb27b1fca40931").Icon; //ice prison

            var weapon = library.CopyAndAdd<BlueprintItemWeapon>("f60c5a820b69fb243a4cce5d1d07d06e", "IceBodyFist", "");
            var frost_enchantment = library.Get<BlueprintWeaponEnchantment>("421e54078b7719d40915ce0672511d0b");

            var enchantment = Common.createWeaponEnchantment("IceBodyWeaponEcnchantment",
                                                             "Ice Body",
                                                             "This weapon deals an additional point of cold damage.",
                                                             "",
                                                             "",
                                                             "",
                                                             0,
                                                             frost_enchantment.WeaponFxPrefab,
                                                             Common.weaponEnergyDamageDice(DamageEnergyType.Cold, new DiceFormula(1, DiceType.One))
                                                             );
            weapon.Enchantments.Add(enchantment);

            var buff = Helpers.CreateBuff("IceBodyBuff",
                                          "Ice Body",
                                          "Your form transmutes into living ice, granting you several abilities. You gain the cold subtype and damage reduction 5/magic. You are immune to ability score damage, blindness, critical hits, deafness, disease, drowning, electricity, poison, stunning, and all spells or attacks that affect your physiology or respiration, because you have no physiology or respiration while this spell is in effect.\n"
                                          + "Your unarmed attack deals damage equal to a club sized for you (1d4 for Small characters or 1d6 for Medium characters) plus 1 point of cold damage, and you are considered armed when making unarmed attacks.",
                                          "",
                                          icon,
                                          Common.createPrefabLink("d7f055790fceee34e87c4902877c894f"),
                                          Common.createAddEnergyDamageImmunity(DamageEnergyType.Cold),
                                          Common.createAddEnergyDamageImmunity(DamageEnergyType.Electricity),
                                          Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Blindness | SpellDescriptor.Disease | SpellDescriptor.Stun | SpellDescriptor.Poison | SpellDescriptor.Death),
                                          Common.createBuffDescriptorImmunity(SpellDescriptor.Electricity | SpellDescriptor.Cold | SpellDescriptor.Blindness | SpellDescriptor.Disease | SpellDescriptor.Stun | SpellDescriptor.Poison),
                                          Helpers.CreateAddFact(library.Get<BlueprintFeature>("5e4d22d5cb6869e499f5fdc82e2127ad")), //cold subtype
                                          Common.createMagicDR(5),
                                          Helpers.Create<AddImmunityToAbilityScoreDamage>(),
                                          Helpers.Create<AddImmunityToCriticalHits>(),
                                          Common.createEmptyHandWeaponOverride(weapon),
                                          Helpers.CreateSpellDescriptor(SpellDescriptor.Cold),
                                          Common.createSpecificBuffImmunity(suffocation_buff),
                                          Common.createSpecificBuffImmunity(fast_suffocation_buff),
                                          Common.createSpecificBuffImmunity(Common.deafened)
                                          );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), is_from_spell: true);

            ice_body = Helpers.CreateAbility("IceBodyAbility",
                                             buff.Name,
                                             buff.Description,
                                             "",
                                             buff.Icon,
                                             AbilityType.Spell,
                                             UnitCommand.CommandType.Standard,
                                             AbilityRange.Personal,
                                             Helpers.minutesPerLevelDuration,
                                             "",
                                             Helpers.CreateRunActions(apply_buff),
                                             Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                             Helpers.CreateSpellDescriptor(SpellDescriptor.Cold)
                                             );
            ice_body.setMiscAbilityParametersSelfOnly();
            ice_body.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken;

            ice_body.AddToSpellList(Helpers.wizardSpellList, 7);
            ice_body.AddSpellAndScroll("de05d46f44c8439488a8bbcc0059c09f"); //icy prison
        }


        static void createRigorMortis()
        {
            var icon = library.Get<BlueprintAbility>("cc0aeb74b35cb7147bff6c53538bbc76").Icon; //forced repentance
            var undead = library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");
            var construct = library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f");
            var elemental = library.Get<BlueprintFeature>("198fd8924dabcb5478d0f78bd453c586");

            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Unholy, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.DamageBonus)),
                                                     halfIfSaved: true);
            dmg.DamageType.Type = Kingmaker.RuleSystem.Rules.Damage.DamageType.Direct;

            var buff = Helpers.CreateBuff("RigorMortisBuff",
                                          "Rigor Mortis",
                                          $"The joints of a creature affected by this spell stiffen and swell, making movement painful and slow. The target takes 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage per caster level. Additionally, the target takes a –4 penalty to Dexterity and its movement speed decreases by 10 feet; these additional effects last for 1 minute per caster level. A successful save halves the damage and negates the penalty to Dexterity and movement.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.CreateAddStatBonus(StatType.Dexterity, -4, ModifierDescriptor.UntypedStackable),
                                          Helpers.CreateAddStatBonus(StatType.Speed, -10, ModifierDescriptor.UntypedStackable)
                                          );
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), is_from_spell: true);
            var effect = Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(dmg, Helpers.CreateConditionalSaved(null, apply_buff)));

            rigor_mortis = Helpers.CreateAbility("RigorMortisAbility",
                                                 buff.Name,
                                                 buff.Description,
                                                 "",
                                                 buff.Icon,
                                                 AbilityType.Spell,
                                                 UnitCommand.CommandType.Standard,
                                                 AbilityRange.Medium,
                                                 Helpers.minutesPerLevelDuration,
                                                 "Fortitude partial",
                                                 Helpers.CreateRunActions(effect),
                                                 Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                                 Common.createAbilityTargetHasFact(true, undead),
                                                 Common.createAbilityTargetHasFact(true, construct),
                                                 Common.createAbilityTargetHasFact(true, elemental),
                                                 Common.createAbilitySpawnFx("352469f228a3b1f4cb269c7ab0409b8e", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                 Helpers.CreateContextRankConfig()
                                                 );
            rigor_mortis.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            rigor_mortis.AvailableMetamagic = Metamagic.Empower | Metamagic.Extend | Metamagic.Heighten | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent;

            rigor_mortis.AddToSpellList(Helpers.clericSpellList, 4);
            rigor_mortis.AddToSpellList(Helpers.magusSpellList, 4);
            rigor_mortis.AddToSpellList(Helpers.wizardSpellList, 4);

            rigor_mortis.AddSpellAndScroll("a9a0d65ec202e25478bcae4a87e844f9");
        }   

        static void createFlashfire()
        {
            var icon = library.Get<BlueprintAbility>("19309b5551a28d74288f4b6f7d8d838d").Icon;
            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("edb2896d49015434bbbe401ee27338c3", "FlashfireArea", "");
            area.Fx = Common.createPrefabLink("42eec2a08ed8721448fac9b8a51e5912"); //wall of fire 30 ft
            area.Size = 30.Feet();

            var burn1d6 = library.Get<BlueprintBuff>("e92ecaa76b5db674fa5b0aaff5b21bc9");
            var apply_burn = Common.createContextActionApplyBuff(burn1d6, Helpers.CreateContextDuration(), is_permanent: true);
            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Fire, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.DamageDice)), isAoE: true);
            var saved_dmg = Common.createContextActionSavingThrow(SavingThrowType.Reflex, Helpers.CreateActionList(Helpers.CreateConditionalSaved(new GameAction[0], 
                                                                                                                                                  new GameAction[] { dmg, apply_burn })));

            var area_run_action = Helpers.Create<NewMechanics.AbilityAreaEffectRunActionWithFirstRound>(a => { a.UnitEnter = Helpers.CreateActionList(saved_dmg); a.Round = Helpers.CreateActionList(saved_dmg); });

            area.ComponentsArray = new BlueprintComponent[] { area_run_action,
                                                              Helpers.CreateContextRankConfig(progression: ContextRankProgression.StartPlusDivStep, 
                                                                                              startLevel: 3, stepLevel: 3, max: 5, type: AbilityRankType.DamageDice, feature: MetamagicFeats.intensified_metamagic),
                                                              Helpers.CreateSpellDescriptor(SpellDescriptor.Fire)};

            area.SpellResistance = true;
            flashfire = Helpers.CreateAbility("FlashfireAbility",
                                                "Flashfire",
                                                $"You cause flames to spring up in a 30-ft line, these flames deal 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of fire damage for every 3 caster levels you have (maximum 5d{BalanceFixes.getDamageDieString(DiceType.D6)}) to each creature that enters a burning area or begins its turn in the area; these creatures also catch on fire. A creature that succeeds at a Reflex save negates the damage and avoids catching on fire.",
                                                "",
                                                icon,
                                                AbilityType.Spell,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Medium,
                                                Helpers.roundsPerLevelDuration,
                                                "Reflex negates",
                                                Helpers.CreateRunActions(Common.createContextActionSpawnAreaEffect(area, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)))
                                                                        ),
                                                Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                Helpers.CreateContextRankConfig(),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.Fire)
                                                );
            flashfire.setMiscAbilityParametersRangedDirectional();
            flashfire.SpellResistance = true;
            flashfire.AvailableMetamagic = Metamagic.Empower | Metamagic.Extend | Metamagic.Heighten | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.IntensifiedGeneral  | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Rime | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            flashfire.AddToSpellList(Helpers.druidSpellList, 3);
            flashfire.AddSpellAndScroll("d8f2bcc130113194998810b7ae3e07f5"); //blessing of salamander
        }

        static void createBlisteringInvective()
        {
            var icon = library.Get<BlueprintAbility>("cf6c901fb7acc904e85c63b342e9c949").Icon; //confusion

            var burn1d6 = library.Get<BlueprintBuff>("e92ecaa76b5db674fa5b0aaff5b21bc9");
            var dazzling_display = library.Get<BlueprintAbility>("5f3126d4120b2b244a95cb2ec23d69fb");

            var demoralize = (dazzling_display.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as Demoralize);


            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Fire, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D10), 1, 0));
            var apply_burn = Common.createContextActionApplyBuff(burn1d6, Helpers.CreateContextDuration(), is_permanent: true);

            var effect = Common.createContextActionSavingThrow(SavingThrowType.Reflex, Helpers.CreateActionList(dmg, Helpers.CreateConditionalSaved(null, apply_burn)));


            blistering_invective = Helpers.CreateAbility("BlisteringInvectiveAbility",
                                                         "Blistering Invective",
                                                         $"You unleash an insulting tirade so vicious and spiteful that enemies who hear it are physically scorched by your fury. When you cast this spell, make an Intimidate check to demoralize each enemy within 30 feet of you. Enemies that are demoralized this way take 1d{BalanceFixes.getDamageDieString(DiceType.D10)} points of fire damage and must succeed at a Reflex save or catch fire.",
                                                         "",
                                                         icon,
                                                         AbilityType.Spell,
                                                         UnitCommand.CommandType.Standard,
                                                         AbilityRange.Personal,
                                                         "",
                                                         "Reflex partial",
                                                         Helpers.CreateRunActions(demoralize),
                                                         Helpers.Create<DemoralizeMechanics.ScopedDemoralizeActions>(a => a.actions = Helpers.CreateActionList(effect)),
                                                         dazzling_display.GetComponent<AbilityTargetsAround>(),
                                                         dazzling_display.GetComponent<AbilitySpawnFx>(),
                                                         Helpers.Create<SharedSpells.CannotBeShared>(),
                                                         Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                         Helpers.CreateSpellDescriptor(SpellDescriptor.Fire | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.LanguageDependent)
                                                         );

            blistering_invective.setMiscAbilityParametersSelfOnly();
            blistering_invective.AvailableMetamagic = Metamagic.Empower | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Maximize | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Rime | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental;

            blistering_invective.AddToSpellList(Helpers.alchemistSpellList, 2);
            blistering_invective.AddToSpellList(Helpers.inquisitorSpellList, 2);
            blistering_invective.AddToSpellList(Helpers.bardSpellList, 2);

            blistering_invective.AddSpellAndScroll("1db86aaa479be6944abe90eaddb4afa2");
        }


        static void createStrickenHeart()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/StrickenHeart.png");
            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.NegativeEnergy, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 2, 0));

            var frigid_touch = library.Get<BlueprintAbility>("c83447189aabc72489164dfc246f3a36");
            var ability = Helpers.CreateAbility("StrickenHeartAbility",
                                               "Stricken Heart",
                                               $"This spell covers your hand with a writhing black aura. As part of casting the spell, you can make a melee touch attack that deals 2d{BalanceFixes.getDamageDieString(DiceType.D6)} points of negative energy damage and causes the target to be staggered for 1 round. If the attack is a critical hit, the target is staggered for 1 minute instead. Creatures immune to precision damage are immune to the staggered effect.",
                                               "",
                                               icon,
                                               AbilityType.Spell,
                                               UnitCommand.CommandType.Standard,
                                               AbilityRange.Touch,
                                               "",
                                               Helpers.savingThrowNone,
                                               Helpers.CreateRunActions(new GameAction[] { dmg }.AddToArray(frigid_touch.GetComponent<AbilityEffectRunAction>().Actions.Actions.Skip(1))),
                                               Helpers.CreateDeliverTouch(),
                                               Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                               Helpers.CreateSpellDescriptor(SpellDescriptor.Death),
                                               frigid_touch.GetComponent<AbilitySpawnFx>()
                                               );
            ability.setMiscAbilityParametersTouchHarmful();
            ability.SpellResistance = true;
            ability.AvailableMetamagic = Metamagic.Maximize | Metamagic.Empower | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            stricken_heart = Helpers.CreateTouchSpellCast(ability);

            stricken_heart.AddToSpellList(Helpers.inquisitorSpellList, 2);
            stricken_heart.AddToSpellList(Helpers.wizardSpellList, 2);
            stricken_heart.AddSpellAndScroll("2fff640003e17ca459f65e787d2d65de");//unbreakable heart
        }


        static void createRiverOfWind()
        {
            var dmg = Helpers.CreateActionDealDamage(PhysicalDamageForm.Bludgeoning, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 4), true, true);
            var dmg2 = Helpers.CreateActionDealDamage(PhysicalDamageForm.Bludgeoning, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 2), true, true);
            var saved = Helpers.CreateConditionalSaved(null, Helpers.Create<ContextActionKnockdownTarget>());
            var effect = Helpers.CreateConditional(Helpers.CreateConditionHasFact(immunity_to_wind),
                                                   null,
                                                   Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(dmg, saved))
                                                   );
            var effect2 = Helpers.CreateConditional(Helpers.CreateConditionHasFact(immunity_to_wind),
                                                   null,
                                                   Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(dmg2, saved))
                                                   );
            var icon = library.Get<BlueprintFeature>("2aad85320d0751340a0786de073ee3d5").Icon;//torrent infusion

            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("2eef9ca9e79968547a01d06d3828e17f", "RiverOfWindArea", ""); //wall sandstorm
            area.ComponentsArray = new BlueprintComponent[] {Helpers.Create<NewMechanics.AbilityAreaEffectRunActionWithFirstRound>(a => { a.FirstRound = Helpers.CreateActionList(effect, dmg2);
                                                                                                                                          a.Round = Helpers.CreateActionList(effect2); }) };
            area.SpellResistance = true;
            river_of_wind = Helpers.CreateAbility("RiverOfWindAbility",
                                                  "River of Wind",
                                                  $"Summoning up the power of the tempest, you direct a current of forceful winds where you please. This spell creates a 5-foot-diameter 60-foot length line of wind where you designate, it remains constant in that direction for the spell duration. Creatures caught in a river of wind take 4d{BalanceFixes.getDamageDieString(DiceType.D6)} bludgeoning damage and are knocked prone. A successful Fortitude save halves the damage and prevents being knocked prone.\n"
                                                  + $"On every next round a creature that is wholly or partially within a river of wind must make a Fortitude save or take 2d{BalanceFixes.getDamageDieString(DiceType.D6)} bludgeoning damage, and be knocked prone - a successful Fortitude save means the creature merely takes 1d6 bludgeoning damage. Creatures under the effect of freedom of movement and creatures with the air subtype are unaffected by a river of wind.",
                                                  "",
                                                  icon,
                                                  AbilityType.Spell,
                                                  Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                  AbilityRange.Medium,
                                                  Helpers.roundsPerLevelDuration,
                                                  Helpers.fortNegates,
                                                  Helpers.CreateRunActions(Common.createContextActionSpawnAreaEffect(area, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)))
                                                                           ),
                                                  Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                  Helpers.CreateContextRankConfig(),
                                                  Helpers.CreateSpellDescriptor(SpellDescriptor.MovementImpairing | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Air)
                                                  );
            river_of_wind.setMiscAbilityParametersRangedDirectional();
            river_of_wind.SpellResistance = true;
            river_of_wind.AvailableMetamagic = Metamagic.Empower | Metamagic.Extend | Metamagic.Heighten | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            river_of_wind.AddToSpellList(Helpers.druidSpellList, 4);
            river_of_wind.AddToSpellList(Helpers.magusSpellList, 4);
            river_of_wind.AddToSpellList(Helpers.wizardSpellList, 4);

            //add immunity to this spell to freedom of movement
            var freedom_buffs = new string[] {"1533e782fca42b84ea370fc1dcbf4fc1",
                                              "235533b62159790499ced35860636bb2",
                                              "60906dd9e4ddec14c8ac9a0f4e47f54c",
                                              "67519ff6ba615c045afca2347608bfe3"};

            foreach (var b in freedom_buffs)
            {
                var buff = library.Get<BlueprintBuff>(b);
                buff.AddComponent(Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.MovementImpairing));
            }
            river_of_wind.AddSpellAndScroll("c17e4bd5028d6534a8c8d317cd8244ca");
        }


        static void createWindsOfVengeance()
        {
            var icon = library.Get<BlueprintFeature>("f2fa7541f18b8af4896fbaf9f2a21dfe").Icon;//cyclone infusion

            var dmg = Helpers.CreateActionDealDamage(PhysicalDamageForm.Bludgeoning, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D8), 5), false, true);
            var saved = Helpers.CreateConditionalSaved(null, Helpers.Create<ContextActionKnockdownTarget>());
            var cooldown_buff = Helpers.CreateBuff("WindsOfVengeanceCooldownBuff",
                                                   "Winds of Vengeance: Cooldown",
                                                   "",
                                                   "",
                                                   icon,
                                                   null);
            var apply_cooldown = Common.createContextActionApplyBuff(cooldown_buff, Helpers.CreateContextDuration(1), is_child: true, dispellable: false);
            var apply_cooldown_on_main_target = Helpers.Create<NewMechanics.ContextActionOnMainTarget>(c => c.Actions = Helpers.CreateActionList(apply_cooldown));
            var action = Helpers.CreateConditional(new Condition[]{Helpers.Create<NewMechanics.ContextConditionMainTargetHasFact>(c => c.Fact = cooldown_buff),
                                                                   Helpers.CreateConditionHasFact(immunity_to_wind, not: false) },
                                                   null,
                                                   new GameAction[] { Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(dmg, saved)),
                                                                                                                                       apply_cooldown_on_main_target });
            var on_hit = Common.createAddTargetAttackWithWeaponTrigger(Helpers.CreateActionList(),
                                                                       Helpers.CreateActionList(action)
                                                                       );
            var buff = Helpers.CreateBuff("WindsOfVengeanceBuff",
                                          "Winds of Vengeance",
                                          "You surround yourself with a buffeting shroud of supernatural, tornado-force winds. These winds grant you ability to fly and a 30-ft bonus to speed.  The winds shield you from any other wind effects, and form a shell of breathable air around you, allowing you to fly and breathe underwater or in outer space.\n"
                                          + "Ranged weapons (including giant - thrown boulders, siege weapon projectiles, and other massive ranged weapons) passing through the winds are deflected by the winds and automatically miss you. Gases and most gaseous breath weapons cannot pass though the winds.\n"
                                          + $"In addition, once per round, when a creature hits you with a melee attack, winds lash out at that creature. The creature must make a Fortitude Saving Throw or take 5d{BalanceFixes.getDamageDieString(DiceType.D8)} points of bludgeoning damage and be knocked prone.\n"
                                          + "On a successful save, the damage is halved and the creature is not knocked prone.",
                                          "",
                                          icon,
                                          Common.createPrefabLink("40c31beb53bffb845b095542133ac9bc"),//"ea8ddc3e798aa25458e2c8a15e484c68"),
                                          Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.BreathWeapon | SpellDescriptor.Poison | SpellDescriptor.Ground),
                                          Helpers.CreateAddStatBonus(StatType.Speed, 30, ModifierDescriptor.UntypedStackable),
                                          Helpers.Create<NewMechanics.WeaponAttackAutoMiss>(w => w.attack_types = new AttackType[] { AttackType.Ranged, AttackType.RangedTouch }),
                                          on_hit,
                                          Helpers.CreateAddFact(immunity_to_wind)
                                          );
            buff.AddComponents(FixFlying.airborne.ComponentsArray);
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), is_from_spell: true);
            winds_of_vengeance = Helpers.CreateAbility("WindsOfVengeanceAbility",
                                                       buff.Name,
                                                       buff.Description,
                                                       "",
                                                       icon,
                                                       AbilityType.Spell,
                                                       Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                       AbilityRange.Personal,
                                                       Helpers.minutesPerLevelDuration,
                                                       "",
                                                       Helpers.CreateSpellDescriptor((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Air),
                                                       Helpers.CreateRunActions(apply_buff),
                                                       Helpers.CreateContextRankConfig(),
                                                       Helpers.CreateSpellComponent(SpellSchool.Evocation)
                                                       );
            winds_of_vengeance.setMiscAbilityParametersSelfOnly();
            winds_of_vengeance.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent;

            winds_of_vengeance.AddToSpellList(Helpers.clericSpellList, 9);
            winds_of_vengeance.AddToSpellList(Helpers.druidSpellList, 9);
            winds_of_vengeance.AddToSpellList(Helpers.wizardSpellList, 9);

            winds_of_vengeance.AddSpellAndScroll("c17e4bd5028d6534a8c8d317cd8244ca");
        }




        static void createBurstOfRadiance()
        {
            burst_of_radiance = library.CopyAndAdd<BlueprintAbility>("39a602aa80cc96f4597778b6d4d49c0a", "BurstOfRadianceAbility", "");

            burst_of_radiance.RemoveComponents<SpellListComponent>();
            burst_of_radiance.SetName("Burst of Radiance");
            burst_of_radiance.SetDescription($"This spell fills the area with a brilliant flash of shimmering light. Creatures in the area are blinded for 1d4 rounds, or dazzled for 1d4 rounds if they succeed at a Reflex save. Evil creatures in the area of the burst take 1d{BalanceFixes.getDamageDieString(DiceType.D4)} points of damage per caster level (max 5d{BalanceFixes.getDamageDieString(DiceType.D4)}), whether they succeed at the Reflex save or not.");
            burst_of_radiance.LocalizedSavingThrow = Helpers.CreateString("BurstOfRadianceAbility.SavingThrow", "Reflex partial");
            burst_of_radiance.Range = AbilityRange.Long;
            burst_of_radiance.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten | Metamagic.Maximize | Metamagic.Empower | (Metamagic)MetamagicFeats.MetamagicExtender.IntensifiedGeneral  | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing | (Metamagic)MetamagicFeats.MetamagicExtender.Selective;
            burst_of_radiance.ReplaceComponent<SpellDescriptorComponent>(Helpers.CreateSpellDescriptor(SpellDescriptor.Good | SpellDescriptor.Blindness | SpellDescriptor.SightBased));

            var dazzled = library.Get<BlueprintBuff>("df6d1025da07524429afbae248845ecc");
            var blind = library.Get<BlueprintBuff>("187f88d96a0ef464280706b63635f2af");

            var duration = Helpers.CreateContextDuration(0, diceType: DiceType.D4, diceCount: 1);
            var effect = Helpers.CreateConditionalSaved(Common.createContextActionApplyBuff(dazzled, duration, is_from_spell: true),
                                                        Common.createContextActionApplyBuff(blind, duration, is_from_spell: true));

            var damage = Helpers.CreateActionDealDamage(DamageEnergyType.Divine, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D4), Helpers.CreateContextValue(AbilityRankType.Default)), isAoE: true);

            var apply_damage = Helpers.CreateConditional(Helpers.CreateContextConditionAlignment(AlignmentComponent.Evil),
                                                         damage);

            burst_of_radiance.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(SavingThrowType.Reflex, effect, apply_damage));
            burst_of_radiance.AddComponent(Helpers.CreateContextRankConfig(max: 5, feature: MetamagicFeats.intensified_metamagic));

            burst_of_radiance.AddToSpellList(Helpers.clericSpellList, 2);
            burst_of_radiance.AddToSpellList(Helpers.druidSpellList, 2);
            burst_of_radiance.AddToSpellList(Helpers.wizardSpellList, 2);
            burst_of_radiance.AddSpellAndScroll("2af933b93ac31ba449001e7e3b911b5b");
        }



        static void createObscuringMist()
        {
            var icon = Helpers.GetIcon("3c53ee4965a13d74e81b37ae34f0861b");

            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("fe5102d734382b74586f56980086e5e8", "ObscuringMistFogArea", ""); //mind fog
            area.Fx = Common.createPrefabLink("9510e38e500ea2a4ca60959687230219"); //mind fog
            area.Size = 20.Feet();
            area.SpellResistance = false;
            obscuring_mist_area = area;

            var buff = Helpers.CreateBuff("ObscuringMistBuff",
                                          "Obscuring Mist",
                                          "A misty vapor arises around you. It is stationary. The vapor obscures all sight, including darkvision, beyond 5 feet. A creature 5 feet away has concealment (attacks have a 20% miss chance). Creatures farther away have total concealment (50% miss chance, and the attacker cannot use sight to locate the target).",
                                          "",
                                          icon,
                                          null,
                                          Helpers.Create<AddConcealment>(a => { a.Descriptor = ConcealmentDescriptor.Fog; a.Concealment = Concealment.Partial; a.CheckDistance = false; }),
                                          Helpers.Create<AddConcealment>(a => { a.CheckDistance = true; a.DistanceGreater = 5.Feet(); a.Descriptor = ConcealmentDescriptor.Fog; a.Concealment = Concealment.Total; })
                                          );

            foreach (var c in buff.GetComponents<AddConcealment>().ToArray())
            {
                buff.AddComponent(Common.createOutgoingConcelement(c));
            }

            area.ComponentsArray = new BlueprintComponent[] { Helpers.Create<AbilityAreaEffectBuff>(a => { a.Buff = buff; a.Condition = Helpers.CreateConditionsCheckerAnd(); }) };

            var stinking_cloud_area = library.Get<BlueprintAbilityAreaEffect>("aa2e0a0fe89693f4e9205fd52c5ba3e5");
            var cloudkill_area = library.Get<BlueprintAbilityAreaEffect>("6df1ac314d4e6e9418e34470b79f90d8");
            var mind_fog_area = library.Get<BlueprintAbilityAreaEffect>("fe5102d734382b74586f56980086e5e8");
            stinking_cloud_area.AddComponents(area.ComponentsArray);
            cloudkill_area.AddComponents(area.ComponentsArray);
            mind_fog_area.AddComponents(area.ComponentsArray);

            obscuring_mist = library.CopyAndAdd<BlueprintAbility>("68a9e6d7256f1354289a39003a46d826", "ObscuringMistAbility", "");
            obscuring_mist.SpellResistance = false;
            obscuring_mist.RemoveComponents<SpellListComponent>();
            obscuring_mist.RemoveComponents<SpellDescriptorComponent>();
            obscuring_mist.ReplaceComponent<AbilityAoERadius>(a => Helpers.SetField(a, "m_Radius", 20.Feet()));
            obscuring_mist.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(Common.changeAction<ContextActionSpawnAreaEffect>(a.Actions.Actions, s => { s.AreaEffect = area; s.DurationValue = Helpers.CreateContextDuration(s.DurationValue.BonusValue, DurationRate.Minutes); })));
            obscuring_mist.SetNameDescriptionIcon(buff.Name, buff.Description, buff.Icon);
            obscuring_mist.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach | Metamagic.Extend;
            obscuring_mist.LocalizedDuration = Helpers.minutesPerLevelDuration;
            obscuring_mist.AddToSpellList(Helpers.druidSpellList, 1);
            obscuring_mist.AddToSpellList(Helpers.clericSpellList, 1);
            obscuring_mist.AddToSpellList(Helpers.magusSpellList, 1);
            obscuring_mist.AddToSpellList(Helpers.wizardSpellList, 1);
            obscuring_mist.LocalizedSavingThrow = Helpers.CreateString("ObscuringMist.SavingThrow", "");
            //replace 1st level spell in air, darkness, water and weather domains
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("750bfcd133cd52f42acbd4f7bc9cc365"), obscuring_mist, 1);//air
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("1e1b4128290b11a41ba55280ede90d7d"), obscuring_mist, 1);//darkness
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("c18a821ee662db0439fb873165da25be"), obscuring_mist, 1);//weather
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("e63d9133cebf2cf4788e61432a939084"), obscuring_mist, 1);//water
            obscuring_mist.AddSpellAndScroll("c92308c160d6d424fb64f1fd708aa6cd");//stiking cloud
        }





        static void createRayOfExhaustion()
        {
            var fatigued = library.Get<BlueprintBuff>("e6f2fc5d73d88064583cb828801212f4");
            var exhausted = library.Get<BlueprintBuff>("46d1b9cc3d0fd36469a471b047d773a2");

            var undead = library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");
            var construct = library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f");
            var elemental = library.Get<BlueprintFeature>("198fd8924dabcb5478d0f78bd453c586");

            var apply_fatigued = Common.createContextActionApplyBuff(fatigued, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), is_from_spell: true);
            var apply_exhausted = Common.createContextActionApplyBuff(exhausted, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), is_from_spell: true);

            var exhausted_guard = Helpers.CreateConditional(Common.createContextConditionHasFact(exhausted, false), apply_exhausted);
            var fatigued_guard = Helpers.CreateConditional(Common.createContextConditionHasFact(fatigued), exhausted_guard, apply_fatigued);

            var conditional_saved = Helpers.CreateConditionalSaved(fatigued_guard, exhausted_guard);

            var effect = Helpers.CreateConditional(new Condition[] { Common.createContextConditionHasFact(undead, false), Common.createContextConditionHasFact(construct, false),
                                                                     Common.createContextConditionHasFact(elemental, false)},
                                                    conditional_saved);
            var deliver = library.Get<BlueprintAbility>("450af0402422b0b4980d9c2175869612").GetComponent<AbilityDeliverProjectile>().CreateCopy(); //ray of enfeeblement
            deliver.Projectiles = new BlueprintProjectile[] { library.Get<BlueprintProjectile>("8e38d2cfc358e124e93c792dea56ff9a") }; //ray of exhaustion ?
            ray_of_exhaustion = Helpers.CreateAbility("RayOfExhaustionAbility",
                                                      "Ray of Exhaustion",
                                                      "A black ray projects from your pointing finger. You must succeed on a ranged touch attack with the ray to strike a target.\n"
                                                     + "The subject is immediately exhausted for the spell’s duration. A successful Fortitude save means the creature is only fatigued.\n"
                                                     + "A character that is already fatigued instead becomes exhausted.\n"
                                                     + "This spell has no effect on a creature that is already exhausted. Unlike normal exhaustion or fatigue, the effect ends as soon as the spell’s duration expires.",
                                                     "",
                                                     LoadIcons.Image2Sprite.Create(@"AbilityIcons/RayOfExhaustion.png"),
                                                     AbilityType.Spell,
                                                     Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                     AbilityRange.Close,
                                                     Helpers.minutesPerLevelDuration,
                                                     "Fortitude partial",
                                                     Helpers.CreateRunActions(SavingThrowType.Fortitude, effect),
                                                     deliver,
                                                     Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                     Helpers.CreateSpellDescriptor(SpellDescriptor.Fatigue | SpellDescriptor.Exhausted));
            ray_of_exhaustion.setMiscAbilityParametersSingleTargetRangedHarmful(works_on_allies: true);
            ray_of_exhaustion.SpellResistance = true;
            ray_of_exhaustion.AvailableMetamagic = Metamagic.Extend | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;

            ray_of_exhaustion.AddToSpellList(Helpers.magusSpellList, 3);
            ray_of_exhaustion.AddToSpellList(Helpers.wizardSpellList, 3);
            ray_of_exhaustion.AddSpellAndScroll("792862674c565ad4fbb1ab0c97c42acd");
        }

        static void createBowSpirit()
        {
            var bow_spirit_swift = Helpers.CreateAbility("BowSpiritEffectAbility",
                                                         "Bow Spirit",
                                                         "A bow spirit is a shapeless force that hovers about you, taking ammunition from your quiver and firing it. For as long as the bow spirit lasts, you can spend a swift action to direct the bow spirit to fire an arrow or a bolt at a target of your choice, as if the bow spirit were firing the necessary ranged weapon. The bow spirit uses your base attack bonus plus your Dexterity modifier, as well as any bonuses and effects from feats you have that affect ranged attacks, or bonuses from the ammunition it uses.\n"
                                                         + "A bow spirit‘s attacks do not provoke attacks of opportunity.\n"
                                                         + "There must be ammunition available for the bow spirit to use, and it expends ammunition as if used by you.\n"
                                                         + "A bow spirit occupies your space, and moves with you.\n",
                                                         "",
                                                         LoadIcons.Image2Sprite.Create(@"AbilityIcons/BowSpirit.png"),
                                                         AbilityType.Special,
                                                         Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift,
                                                         AbilityRange.Weapon,
                                                         "",
                                                         "",
                                                         Common.createAbilityCasterMainWeaponCheck(WeaponCategory.Longbow, WeaponCategory.Shortbow),
                                                         Helpers.CreateRunActions((Common.createContextActionAttack()))
                                                        );
            bow_spirit_swift.setMiscAbilityParametersSingleTargetRangedHarmful(works_on_allies: true);

            var buff = Helpers.CreateBuff("BowSpiritBuff",
                                          bow_spirit_swift.Name,
                                          bow_spirit_swift.Description,
                                          "",
                                          bow_spirit_swift.Icon,
                                          null,
                                          Helpers.CreateAddFact(bow_spirit_swift)
                                          );


            bow_spirit = Helpers.CreateAbility("BowSpiritAbility",
                                               buff.Name,
                                               buff.Description,
                                               "",
                                               buff.Icon,
                                               AbilityType.Spell,
                                               Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                               AbilityRange.Personal,
                                               Helpers.roundsPerLevelDuration,
                                               "",
                                               Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), is_from_spell: true)),
                                               Helpers.CreateSpellComponent(SpellSchool.Conjuration),
                                               Common.createAbilitySpawnFx("0c07afb9ee854184cb5110891324e3ad", position_anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                               Common.createAbilitTargetMainWeaponCheck(WeaponCategory.Longbow, WeaponCategory.Shortbow)
                                               );
            bow_spirit.setMiscAbilityParametersSelfOnly();
            bow_spirit.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken;

            bow_spirit.AddToSpellList(Helpers.rangerSpellList, 4);
            bow_spirit.AddSpellAndScroll("ce41e625eae914d4bad729f090e9001f"); // hurricane bow
        }


        static void createBurningEntanglement()
        {
            var fog_area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("aa2e0a0fe89693f4e9205fd52c5ba3e5", "BurningEntanglementFogArea", "");
            fog_area.Fx = Common.createPrefabLink("098a29fefbbc4564281afa5a6887cd2c");
            fog_area.Size = 40.Feet();
            GameAction no_action = null;
            fog_area.ComponentsArray = new BlueprintComponent[] { Helpers.CreateAreaEffectRunAction(no_action) };

            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("bcb6329cefc66da41b011299a43cc681", "BurningEntanglementArea", ""); //entangle

            var area_run_actions = area.GetComponent<AbilityAreaEffectRunAction>().CreateCopy();
            var entnagle_buff = library.Get<BlueprintBuff>("f7f6330726121cf4b90a6086b05d2e38");

            var dmg = Helpers.CreateConditional(Helpers.CreateConditionHasFact(entnagle_buff),
                                                Helpers.CreateActionDealDamage(DamageEnergyType.Fire, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 4), halfIfSaved: true),
                                                Helpers.CreateActionDealDamage(DamageEnergyType.Fire, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 2), halfIfSaved: true)
                                               );
            var save_action = Helpers.CreateActionSavingThrow(SavingThrowType.Reflex, dmg);

            area_run_actions.Round = Helpers.CreateActionList(area_run_actions.Round.Actions.AddToArray(save_action));
            var concealement_buff = library.CopyAndAdd<BlueprintBuff>("49786ccc94a5ee848a5637b4145b2092", "BurningEntanglementConcealementBuff", ""); //chameleon stride

            area.ReplaceComponent<AbilityAreaEffectRunAction>(area_run_actions);
            area.AddComponent(Helpers.Create<AbilityAreaEffectBuff>(a => { a.Buff = concealement_buff; a.Condition = Helpers.CreateConditionsCheckerOr(); }));

            burning_entanglement = library.CopyAndAdd<BlueprintAbility>("0fd00984a2c0e0a429cf1a911b4ec5ca", "BurningEntanglementAbility", "");
            var spawn_area = Common.createContextActionSpawnAreaEffect(area, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Rounds));

            List<Vector2> fog_points = new List<Vector2>();
            for (int i = -1; i <= 1; i++)
            {
                fog_points.Add(new Vector2(0.0f, (i * 20).Feet().Meters));
            }


            var spawn_fog = Common.createContextActionSpawnAreaEffectMultiple(fog_area, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Rounds),
                                                                              fog_points.ToArray());
            burning_entanglement.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(spawn_area, spawn_fog));
            burning_entanglement.RemoveComponents<SpellListComponent>();
            burning_entanglement.AvailableMetamagic = burning_entanglement.AvailableMetamagic | Metamagic.Empower | Metamagic.Maximize | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Rime | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental;

            burning_entanglement.SetNameDescriptionIcon("Burning Entanglement",
                                                        $"This spell functions as per entangle, except it sets the foliage on fire. A creature that begins its turn entangled by the spell takes 4d{BalanceFixes.getDamageDieString(DiceType.D6)} points of fire damage (Reflex half), and a creature that begins its turn in the area but is not entangled takes 2d{BalanceFixes.getDamageDieString(DiceType.D6)} points of fire damage (Reflex negates). Smoke rising from the vines partially obscures visibility. Creatures can see things in the smoke within 5 feet clearly, but attacks against anything farther away in the smoke must contend with concealment (20% miss chance). When the spell’s duration expires, the vines burn away entirely.",
                                                        LoadIcons.Image2Sprite.Create(@"AbilityIcons/BurningEntanglement.png")
                                                        );
            burning_entanglement.LocalizedDuration = Helpers.CreateString("BurningEntanglementAbility.Duration", Helpers.roundsPerLevelDuration);
            concealement_buff.SetNameDescriptionIcon(burning_entanglement.Name, burning_entanglement.Description, burning_entanglement.Icon);
            concealement_buff.FxOnStart = Common.createPrefabLink("ba8a4e16282d63a439434697ee656a3a"); //fire theme buff
            burning_entanglement.ReplaceComponent<SpellComponent>(s => s.School = SpellSchool.Evocation);

            burning_entanglement.AddToSpellList(Helpers.druidSpellList, 3);
            burning_entanglement.AddToSpellList(Helpers.rangerSpellList, 3);

            burning_entanglement.AddSpellAndScroll("5022612735a9e2345bfc5110106823d8"); //entangle
        }


        static void createThirstingEntanglement()
        {
            var icon = library.Get<BlueprintAbility>("6c7467f0344004d48848a43d8c078bf8").Icon;
            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("bcb6329cefc66da41b011299a43cc681", "ThirstingEntanglementArea", ""); //entangle
            var difficult_terrain = library.Get<BlueprintBuff>("762f5da59182e9b4b90c62ed3142b732"); //difficultTerrain

            var area_run_actions = area.GetComponent<AbilityAreaEffectRunAction>().CreateCopy();
            var buff = library.CopyAndAdd<BlueprintBuff>("f7f6330726121cf4b90a6086b05d2e38", "ThirstinEntanglementBuff", "");
            buff.SetIcon(icon);
            buff.SetName("Thirsting Entanglement");
            buff.SetDescription("This spell functions as entangle, except the plants latch on to targets and drain away their vitality. Any creature that fails a save to avoid becoming entangled or fails a check to break free takes 1d2 points of Constitution damage.");
            var con_damage = Helpers.CreateActionDealDamage(StatType.Constitution, Helpers.CreateContextDiceValue(DiceType.D2, 1, 0));

            var break_free = Helpers.Create<ContextActionBreakFree>(c => { c.Success = Helpers.CreateActionList(Helpers.Create<ContextActionRemoveSelf>());
                                                                           c.Failure = Helpers.CreateActionList(con_damage);
                                                                         }
                                                                   );
            buff.ReplaceComponent<AddFactContextActions>(a =>
                                                        {
                                                            a.Activated = Helpers.CreateActionList(con_damage);
                                                            a.NewRound = Helpers.CreateActionList(break_free);
                                                        }
                                                        );
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), is_child: true, is_permanent: true);
            var save_action = Helpers.CreateActionSavingThrow(SavingThrowType.Reflex, Helpers.CreateConditionalSaved(null, apply_buff));
            var remove_buff = Helpers.Create<ContextActionRemoveBuffSingleStack>(c => c.TargetBuff = buff);
            var remove_difficult_terrain = Helpers.Create<ContextActionRemoveBuffSingleStack>(c => c.TargetBuff = difficult_terrain);
            var apply_difficult_terrain = Common.createContextActionApplyBuff(difficult_terrain, Helpers.CreateContextDuration(), is_child: true, is_permanent: true);
            var area_run_action = Helpers.CreateAreaEffectRunAction(unitEnter: new GameAction[] { save_action, apply_difficult_terrain },
                                                                   unitExit: new GameAction[] { remove_buff, remove_difficult_terrain },
                                                                   round: new GameAction[] {Helpers.CreateConditional(Helpers.CreateConditionHasFact(buff),
                                                                                                                      null,
                                                                                                                      save_action)}
                                                                   );

            area.ReplaceComponent<AbilityAreaEffectRunAction>(area_run_action);
            var spawn_area = Common.createContextActionSpawnAreaEffect(area, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes));

            thirsting_entanglement = library.CopyAndAdd<BlueprintAbility>("0fd00984a2c0e0a429cf1a911b4ec5ca", "ThirstingEntanglementAbility", "");

            thirsting_entanglement.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(spawn_area));
            thirsting_entanglement.RemoveComponents<SpellListComponent>();
            thirsting_entanglement.AvailableMetamagic = thirsting_entanglement.AvailableMetamagic | Metamagic.Empower | Metamagic.Maximize | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent;

            thirsting_entanglement.SetNameDescriptionIcon(buff.Name,
                                                          buff.Description,
                                                          buff.Icon
                                                          );

            thirsting_entanglement.AddToSpellList(Helpers.druidSpellList, 4);
            thirsting_entanglement.AddToSpellList(Helpers.rangerSpellList, 4);

            thirsting_entanglement.AddSpellAndScroll("9e0655dcd2f81f84d8fcaff6d439a4a0"); //sickening entanglement
        }


        static void createArchonsTrumpet()
        {
            var paralyzed = library.Get<BlueprintBuff>("af1e2d232ebbb334aaf25e2a46a92591");
            var icon = library.Get<BlueprintAbility>("f09453607e683784c8fca646eec49162").Icon;

            var apply_paralyzed = Common.createContextActionApplyBuff(paralyzed, Helpers.CreateContextDuration(0, DurationRate.Rounds, DiceType.D4, 1), is_from_spell: true);
            var on_failed = Helpers.CreateConditionalSaved(null, apply_paralyzed);

            archons_trumpet = Helpers.CreateAbility("ArchonsTrumpetAbility",
                                                    "Archon’s Trumpet",
                                                    "Upon hearing a booming report, as if from a trumpet archon’s mighty horn, all creatures in the area of the burst are paralyzed for 1d4 rounds.",
                                                    "",
                                                    icon,
                                                    AbilityType.Spell,
                                                    Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                    AbilityRange.Projectile,
                                                    "",
                                                    Helpers.fortNegates,
                                                    Helpers.CreateRunActions(SavingThrowType.Fortitude, on_failed),
                                                    Common.createAbilityDeliverProjectile(AbilityProjectileType.Cone, library.Get<BlueprintProjectile>("c7fd792125b79904881530dbc2ff83de"),
                                                                                          30.Feet(), 5.Feet()),
                                                    Helpers.CreateSpellDescriptor(SpellDescriptor.Good | SpellDescriptor.Sonic | SpellDescriptor.Paralysis | SpellDescriptor.MovementImpairing),
                                                    Helpers.CreateSpellComponent(SpellSchool.Evocation)
                                                    );
            archons_trumpet.setMiscAbilityParametersRangedDirectional();
            archons_trumpet.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing | (Metamagic)MetamagicFeats.MetamagicExtender.Selective;
            archons_trumpet.SpellResistance = true;

            archons_trumpet.AddToSpellList(Helpers.bardSpellList, 5);
            archons_trumpet.AddToSpellList(Helpers.clericSpellList, 7);
            archons_trumpet.AddToSpellList(Helpers.paladinSpellList, 4);
            archons_trumpet.AddToSpellList(Helpers.wizardSpellList, 7);
            archons_trumpet.AddSpellAndScroll("4c73f11f91ca3fb4a8af325686b660d8"); //shout
        }


        static void createAuraOfDoom()
        {
            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("a70dc66c3059b7a4cb5b2a2e8ac37762", "AuraOfDoomArea", "");

            var shaken2 = library.CopyAndAdd<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220", "AuraOfDoomShakenBuff", "");
            shaken2.Stacking = StackingType.Stack;
            var apply_shaken = Common.createContextActionApplyBuff(shaken2, Helpers.CreateContextDuration(), is_child: false, dispellable: false, is_permanent: true);
            var on_save_failed = Helpers.CreateConditionalSaved(null, apply_shaken);
            var effect = Common.createContextActionSavingThrow(SavingThrowType.Will, Helpers.CreateActionList(on_save_failed));

            var conditional_effect = Helpers.CreateConditional(Helpers.Create<ContextConditionIsEnemy>(), effect);
            area.ReplaceComponent<AbilityAreaEffectRunAction>(a =>
            {
                a.UnitEnter = Helpers.CreateActionList(conditional_effect);
                a.UnitExit = Helpers.CreateActionList(Helpers.CreateConditional(Common.createContextConditionHasBuffFromCaster(shaken2),
                                                                            Helpers.Create<ContextActionRemoveBuffSingleStack>(c => c.TargetBuff = shaken2))
                                                );
            }
            );
            area.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Emotion | SpellDescriptor.Fear | SpellDescriptor.Shaken));
            area.SpellResistance = true;
            var buff = Helpers.CreateBuff("AuraOfDoomBuff",
                                          "Aura of Doom",
                                          "You emanate an almost palpable aura of horror (with 20 ft. radius). All enemies within this spell’s area, or that later enter the area, must make a Will save to avoid becoming shaken. A successful save suppresses the effect. Creatures that leave the area and come back must save again to avoid being affected by the effect.",
                                          "",
                                          LoadIcons.Image2Sprite.Create(@"AbilityIcons/AuraOfDoom.png"),
                                          null,
                                          Common.createAddAreaEffect(area)
                                          );
            var apply_aura = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.TenMinutes), is_from_spell: true);
            aura_of_doom = Helpers.CreateAbility("AuraOfDoomAbility",
                                                 buff.Name,
                                                 buff.Description,
                                                 "",
                                                 buff.Icon,
                                                 AbilityType.Spell,
                                                 Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                 AbilityRange.Personal,
                                                 Helpers.tenMinPerLevelDuration,
                                                 Helpers.willNegates,
                                                 Helpers.CreateRunActions(apply_aura),
                                                 Common.createAbilitySpawnFx("cbfe312cb8e63e240a859efaad8e467c", anchor: AbilitySpawnFxAnchor.SelectedTarget),//necromancy buff
                                                 Helpers.CreateSpellComponent(SpellSchool.Necromancy)
                                                 );
            aura_of_doom.setMiscAbilityParametersSelfOnly();
            //aura_of_doom.SpellResistance = true;
            aura_of_doom.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;

            aura_of_doom.AddToSpellList(Helpers.clericSpellList, 4);
            aura_of_doom.AddSpellAndScroll("124d26c97479b424383124e047183828");

        }


        static void createBoneFists()
        {
            var allowed_categories = new WeaponCategory[] { WeaponCategory.Claw, WeaponCategory.Bite, WeaponCategory.Gore, WeaponCategory.OtherNaturalWeapons, WeaponCategory.UnarmedStrike };
            var buff = Helpers.CreateBuff("BoneFistsBuff",
                                          "Bone Fists",
                                          "The bones of your targets’ joints grow thick and sharp, protruding painfully through the skin at the knuckles, elbows, shoulders, spine, and knees. The targets each gain a +1 bonus to natural armor and a +2 bonus on damage rolls with natural weapons.",
                                          "",
                                          LoadIcons.Image2Sprite.Create(@"AbilityIcons/BoneFists.png"),
                                          null,
                                          Helpers.CreateAddStatBonus(StatType.AC, 1, ModifierDescriptor.NaturalArmor),
                                          Helpers.Create<NewMechanics.ContextWeaponCategoryDamageBonus>(c => { c.Value = 2; c.categories = allowed_categories; })
                                          );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes));

            bone_fists = Helpers.CreateAbility("BoneFistsAbility",
                                               buff.Name,
                                               buff.Description,
                                               "",
                                               buff.Icon,
                                               AbilityType.Spell,
                                               Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                               AbilityRange.Personal,
                                               Helpers.minutesPerLevelDuration,
                                               "",
                                               Helpers.CreateRunActions(apply_buff),
                                               Common.createAbilitySpawnFx("cbfe312cb8e63e240a859efaad8e467c", anchor: AbilitySpawnFxAnchor.SelectedTarget),//necromancy buff
                                               Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                               Helpers.CreateAbilityTargetsAround(15.Feet(), TargetType.Ally),
                                               Helpers.Create<SharedSpells.CannotBeShared>(),
                                               Helpers.Create<NewMechanics.AbilityTargetIsCaster>()
                                               );
            bone_fists.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken;
            bone_fists.setMiscAbilityParametersSelfOnly();

            bone_fists.AddToSpellList(Helpers.druidSpellList, 2);
            bone_fists.AddToSpellList(Helpers.clericSpellList, 2);
            bone_fists.AddToSpellList(Helpers.wizardSpellList, 2);


            bone_fists.AddSpellAndScroll("42d9445b9cdfac94385eaa2a3499b204");
        }


        static void createExplosionOfRot()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/ExplosionOfRot.png");
            var normal_dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Unholy, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.DamageDice)),
                                                     isAoE: true, halfIfSaved: true);
            var plant_dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Unholy, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.DamageDice), Helpers.CreateContextValue(AbilityRankType.DamageDice)));

            var staggered = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");

            var apply_staggered = Common.createContextActionApplyBuff(staggered, Helpers.CreateContextDuration(0, DurationRate.Rounds, diceType: DiceType.D4, diceCount: 1));

            var plant = library.Get<BlueprintFeature>("706e61781d692a042b35941f14bc41c5");
            var is_plant = Helpers.CreateConditionHasFact(plant);
            var dmg = Helpers.CreateConditional(is_plant, plant_dmg, normal_dmg);
            var effect = Helpers.CreateConditionalSaved(new GameAction[0], new GameAction[] { apply_staggered });
            var action_savingthrow = Common.createContextActionSavingThrow(SavingThrowType.Reflex, Helpers.CreateActionList(effect, dmg));
            Common.addConditionalDCIncrease(action_savingthrow, Helpers.CreateConditionsCheckerOr(is_plant), 2);


            explosion_of_rot = Helpers.CreateAbility("ExplosionOfRotAbility",
                                                     "Explosion of Rot",
                                                     $"You call forth a burst of decay that ravages all creatures in the area. Even nonliving creatures such as constructs and undead crumble or wither in this malignant eruption of rotting energy. Creatures in the area of effect take 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage per caster level (maximum 15d{BalanceFixes.getDamageDieString(DiceType.D6)}) and are staggered for 1d4 rounds. A target that succeeds at a Reflex saving throw takes half damage and negates the staggered effect. Plant creatures are particularly susceptible to this rotting effect; a plant creature caught in the burst takes a –2 penalty on the saving throw and takes 1 extra point of damage per die.",
                                                     "",
                                                     icon,
                                                     AbilityType.Spell,
                                                     Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                     AbilityRange.Close,
                                                     "",
                                                     Helpers.reflexHalfDamage,
                                                     Helpers.CreateRunActions(action_savingthrow),
                                                     Common.createAbilitySpawnFx("a65a25d577ea8564e81c0368f09bf585", anchor: AbilitySpawnFxAnchor.ClickedTarget),
                                                     Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                     Helpers.CreateAbilityTargetsAround(10.Feet(), TargetType.Any),
                                                     Helpers.CreateContextRankConfig(type: AbilityRankType.DamageDice, max: 15, feature: MetamagicFeats.intensified_metamagic)
                                                     );
            explosion_of_rot.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Heighten | Metamagic.Reach | Metamagic.Quicken | (Metamagic)MetamagicFeats.MetamagicExtender.IntensifiedGeneral  | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing | (Metamagic)MetamagicFeats.MetamagicExtender.Selective;
            explosion_of_rot.setMiscAbilityParametersRangedDirectional();
            explosion_of_rot.SpellResistance = true;

            explosion_of_rot.AddToSpellList(Helpers.druidSpellList, 4);
            explosion_of_rot.AddSpellAndScroll("a578283bf8f2eaa418823e5b5f661966"); //contagion
        }


        static void createEarthTremor()
        {
            var icon = library.Get<BlueprintAbility>("29ccc62632178d344ad0be0865fd3113").Icon; //create pit
            var difficult_terrain = library.CopyAndAdd<BlueprintBuff>("1914ccc0f3da5b1439f0b90d90d05811", "EarthTremorDifficultTerrainBuff", "");
            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("8b4ea698ae053c541beed4e050f32dc3", "EarthTremorArea", "");
            area.RemoveComponents<AbilityAreaEffectRunAction>();
            area.AddComponent(Helpers.Create<AbilityAreaEffectBuff>(a => { a.Buff = difficult_terrain; a.Condition = Helpers.CreateConditionsCheckerOr(); }));
            area.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Ground));

            var dmg = Helpers.CreateActionDealDamage(PhysicalDamageForm.Bludgeoning, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D4), Helpers.CreateContextValue(AbilityRankType.Default)),
                                                     isAoE: true, halfIfSaved: true);

            var apply_prone = Helpers.Create<ContextActionKnockdownTarget>();
            var failure_prone_action = Helpers.CreateConditional(Helpers.Create<CombatManeuverMechanics.ContextConditionTargetSizeLessOrEqual>(c => c.target_size = Size.Medium),
                                                           Helpers.CreateConditionalSaved(null, apply_prone)
                                                          );

            var description = "You strike the ground and unleash a tremor of seismic force, hurling up earth, rock, and sand.\n"
                              + $"You choose whether the earth tremor affects a 30 - foot line, a 20 - foot cone - shaped spread, or a 10 - foot - radius spread centered on you. The space you occupy is not affected by earth tremor. The area you choose becomes dense rubble and is considered as difficult terrain. Creatures on the ground in the area take 1d{BalanceFixes.getDamageDieString(DiceType.D4)} points of bludgeoning damage per caster level you have (maximum 10d{BalanceFixes.getDamageDieString(DiceType.D4)}) or half damage on a successful save. Medium or smaller creatures that fail their saves are knocked prone.\n"
                              + "This spell can be cast only on a surface of earth, sand, or stone. It has no effect if you are in a wooden or metal structure or if you are not touching the ground.";

            var spawn_in_cone = Helpers.Create<NewMechanics.ContextActionSpawnAreaEffectMultiple>(c =>
            {
                c.AreaEffect = area;
                c.use_caster_as_origin = true;
                c.points_around_target = new Vector2[] { new Vector2(5.Feet().Meters, 0.0f), new Vector2(10.Feet().Meters,5.Feet().Meters), new Vector2(10.Feet().Meters, -5.Feet().Meters)};
                c.DurationValue = Helpers.CreateContextDuration(1, DurationRate.Hours);
            });
            var earth_tremor_cone = Helpers.CreateAbility("EarthTremorCone",
                                                 "Earth Tremor: 20-foot Cone",
                                                 description,
                                                 "",
                                                 icon,
                                                 AbilityType.Spell,
                                                 Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                 AbilityRange.Projectile,
                                                 "",
                                                 Helpers.reflexHalfDamage,
                                                 Helpers.CreateRunActions(SavingThrowType.Reflex, dmg, failure_prone_action
                                                                          //,Common.createContextActionSpawnAreaEffect(area, Helpers.CreateContextDuration(1, DurationRate.Hours))
                                                                          ),
                                                 Helpers.Create<AbilityEffectRunActionOnClickedTarget>(a => a.Action = Helpers.CreateActionList(spawn_in_cone)),
                                                 Helpers.CreateContextRankConfig(max: 10, feature: MetamagicFeats.intensified_metamagic),
                                                 Helpers.CreateSpellDescriptor(SpellDescriptor.Ground | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Earth),
                                                 Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                                 Common.createAbilityDeliverProjectile(AbilityProjectileType.Cone,
                                                                                      library.Get<BlueprintProjectile>("868f9126707bdc5428528dd492524d52"), 20.Feet(), 5.Feet()) //sonic cone
                                                 );

            earth_tremor_cone.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Heighten | Metamagic.Quicken | (Metamagic)MetamagicFeats.MetamagicExtender.IntensifiedGeneral  | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent;
            earth_tremor_cone.setMiscAbilityParametersRangedDirectional();

            var earth_tremor_line = library.CopyAndAdd<BlueprintAbility>(earth_tremor_cone.AssetGuid, "EarthTremorLine", "");
            earth_tremor_line.SetName("Earth Tremor: 30-foot Line");
            earth_tremor_line.ReplaceComponent<AbilityDeliverProjectile>(a =>
            {
                a.Type = AbilityProjectileType.Line;
                a.Length = 30.Feet();
                a.Projectiles = new BlueprintProjectile[] { library.Get<BlueprintProjectile>("868f9126707bdc5428528dd492524d52") };//same cone
            });

            var spawn_in_line = Helpers.Create<NewMechanics.ContextActionSpawnAreaEffectMultiple>(c =>
            {
                c.AreaEffect = area;
                c.use_caster_as_origin = true;
                c.points_around_target = new Vector2[] { new Vector2(5.Feet().Meters, 0.0f), new Vector2(15.Feet().Meters, 0), new Vector2(25.Feet().Meters,0) };
                c.DurationValue = Helpers.CreateContextDuration(1, DurationRate.Hours);
            });
            earth_tremor_line.ReplaceComponent<AbilityEffectRunActionOnClickedTarget>(a =>
            {
                a.Action = Helpers.CreateActionList(spawn_in_line);
            }
            );

            var earth_tremor_burst = library.CopyAndAdd<BlueprintAbility>(earth_tremor_cone.AssetGuid, "EarthTremorBurst", "");
            earth_tremor_burst.SetName("Earth Tremor: 10-foot Spread");
            earth_tremor_burst.ReplaceComponent<AbilityDeliverProjectile>(Helpers.CreateAbilityTargetsAround(10.Feet(), TargetType.Any,
                                                                                                            Helpers.CreateConditionsCheckerOr(Common.createContextConditionIsCaster(not: true))
                                                                                                            )
                                                                        );

            var spawn_in_burst = Helpers.Create<NewMechanics.ContextActionSpawnAreaEffectMultiple>(c =>
            {
                c.AreaEffect = area;
                c.use_caster_as_origin = true;
                c.ignore_direction = true;
                c.points_around_target = new Vector2[] {new Vector2(5.Feet().Meters, 5.Feet().Meters), new Vector2(5.Feet().Meters, -5.Feet().Meters),
                                                        new Vector2(-5.Feet().Meters, 5.Feet().Meters), new Vector2(-5.Feet().Meters, -5.Feet().Meters)};
                c.DurationValue = Helpers.CreateContextDuration(1, DurationRate.Hours);
            });
            earth_tremor_burst.ReplaceComponent<AbilityEffectRunActionOnClickedTarget>(a =>
            {
                a.Action = Helpers.CreateActionList(spawn_in_burst);
            }
            );


            earth_tremor_burst.setMiscAbilityParametersSelfOnly();
            earth_tremor_burst.Range = AbilityRange.Personal;
            earth_tremor = Common.createVariantWrapper("EarthTremorAbility", "", earth_tremor_cone, earth_tremor_line, earth_tremor_burst);
            earth_tremor.SetName("Earth Tremor");
            earth_tremor.AddComponents(Helpers.CreateSpellDescriptor(SpellDescriptor.Ground),
                                       Helpers.CreateSpellComponent(SpellSchool.Transmutation));
            earth_tremor.Range = AbilityRange.Unlimited;
            earth_tremor.AddToSpellList(Helpers.druidSpellList, 3);
            earth_tremor.AddToSpellList(Helpers.magusSpellList, 3);
            earth_tremor.AddToSpellList(Helpers.wizardSpellList, 3);
            earth_tremor.AddSpellAndScroll("bc948837c7acb664eb8d89ac7749fa18");
        }


        static void createFrostBite()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/FrostBite.png");

            BlueprintWeaponEnchantment[] frost_bite_enchantments = new BlueprintWeaponEnchantment[11];
            var cold_damage = Common.createEnergyDamageDescription(Kingmaker.Enums.Damage.DamageEnergyType.Cold);

            var frost_enchant = library.Get<BlueprintWeaponEnchantment>("421e54078b7719d40915ce0672511d0b");
            touch_slam_cold = library.CopyAndAdd<BlueprintWeaponType>("f18cbcb39a1b35643a8d129b1ec4e716", "TouchSlamColdType", "");//slam
            touch_slam_cold.Category = WeaponCategory.Touch;
            Helpers.SetField(touch_slam_cold, "m_IsTwoHanded", false);

            var damage_type = new DamageTypeDescription()
            {
                Type = DamageType.Energy,
                Energy = DamageEnergyType.Cold,
                Common = new DamageTypeDescription.CommomData(),
                Physical = new DamageTypeDescription.PhysicalData()
            };

            Helpers.SetField(touch_slam_cold, "m_DamageType", damage_type);
            Helpers.SetField(touch_slam_cold, "m_TypeNameText", Helpers.CreateString("TouchSlamColdName", "Touch"));
            Helpers.SetField(touch_slam_cold, "m_DefaultNameText", Helpers.CreateString("TouchSlamColdName", "Touch"));


            var weapon = library.CopyAndAdd<BlueprintItemWeapon>("767e6932882a99c4b8ca95c88d823137", "FrostBiteWeapon", "");//sling
            Helpers.SetField(weapon, "m_Type", touch_slam_cold);
            Helpers.SetField(weapon, "m_DisplayNameText", Helpers.CreateString("FrostBiteWeaponName", "Frost Bite"));
            Helpers.SetField(weapon, "m_Icon", icon);
            Common.addEnchantment(weapon, WeaponEnchantments.summoned_weapon_enchant);

            var fatigued = library.Get<BlueprintBuff>("e6f2fc5d73d88064583cb828801212f4");
            var apply_fatigued = Common.createContextActionApplyBuff(fatigued, Helpers.CreateContextDuration(1, DurationRate.Minutes), is_child: true);

            for (int i = 0; i < frost_bite_enchantments.Length; i++)
            {
                var frost_bite_enchant = Helpers.Create<NewMechanics.EnchantmentMechanics.WeaponDamageChange>(w =>
                {
                    w.bonus_damage = 1 + i;
                    w.dice_formula = new DiceFormula(1, DiceType.D6);
                    w.damage_type_description = cold_damage;
                });

                frost_bite_enchantments[i] = Common.createWeaponEnchantment($"FrostBite{i}Enchantment",
                                                                             "Frostbite",
                                                                             "Your melee touch attack deals 1d6 points of cold damage + 1 point per level (maximum + 10), and the target is fatigued for 1 minute. This spell cannot make a creature exhausted even if it is already fatigued. Each attack you make reduces the remaining duration of the spell by 1 minute.\n"
                                                                             + "Your primary hand must be free when you cast this spell.",
                                                                             "",
                                                                             "",
                                                                             "",
                                                                             0,
                                                                             frost_enchant.WeaponFxPrefab,
                                                                             Helpers.Create<NewMechanics.EnchantmentMechanics.Immaterial>(),
                                                                             Helpers.Create<NewMechanics.EnchantmentMechanics.NoDamageScalingEnchant>(),
                                                                             frost_bite_enchant,
                                                                             Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(apply_fatigued))
                                                                             );
            }


            var empower_buff = Common.createBuffContextEnchantPrimaryHandWeaponIfHasMetamagic(Kingmaker.UnitLogic.Abilities.Metamagic.Empower,
                                                                                                  false, false,
                                                                                                  new BlueprintWeaponType[0], WeaponEnchantments.empower_enchant);

            var maximize_buff = Common.createBuffContextEnchantPrimaryHandWeaponIfHasMetamagic(Kingmaker.UnitLogic.Abilities.Metamagic.Maximize,
                                                                                                  false, false,
                                                                                                  new BlueprintWeaponType[0], WeaponEnchantments.maximize_enchant);
            /*
            var fire_buff = Common.createBuffContextEnchantPrimaryHandWeaponIfHasMetamagic((Metamagic)MetamagicFeats.MetamagicExtender.ElementalFire,
                                                                   false, false,
                                                                   new BlueprintWeaponType[0], WeaponEnchantments.elemental_enchants[DamageEnergyType.Fire]);
            var acid_buff = Common.createBuffContextEnchantPrimaryHandWeaponIfHasMetamagic((Metamagic)MetamagicFeats.MetamagicExtender.ElementalAcid,
                                                                                          false, false,
                                                                                          new BlueprintWeaponType[0], WeaponEnchantments.elemental_enchants[DamageEnergyType.Acid]);
            var elec_buff = Common.createBuffContextEnchantPrimaryHandWeaponIfHasMetamagic((Metamagic)MetamagicFeats.MetamagicExtender.ElementalElectricity,
                                                                                          false, false,
                                                                                          new BlueprintWeaponType[0], WeaponEnchantments.elemental_enchants[DamageEnergyType.Electricity]);
            */
            var buff = Helpers.CreateBuff("FrostBiteBuff",
                                            frost_bite_enchantments[0].Name + " (As Weapon)",
                                            frost_bite_enchantments[0].Description,
                                            "",
                                            icon,
                                            null,
                                            Helpers.Create<NewMechanics.EnchantmentMechanics.CreateWeapon>(c => { c.weapon = weapon; }),
                                            Common.createBuffContextEnchantPrimaryHandWeapon(Helpers.CreateContextValue(AbilityRankType.DamageBonus), false, false,
                                                                                            new BlueprintWeaponType[0], frost_bite_enchantments),
                                            empower_buff,
                                            maximize_buff,
                                            //fire_buff,
                                            //acid_buff,
                                            //elec_buff,
                                            Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CasterLevel, progression: ContextRankProgression.DivStep,
                                                                            type: AbilityRankType.DamageBonus, stepLevel: 1, max: 10)
                                            );
            buff.Stacking = Kingmaker.UnitLogic.Buffs.Blueprints.StackingType.Replace;
            weapon.AddComponent(Helpers.Create<NewMechanics.EnchantmentMechanics.WeaponSourceBuff>(w => w.buff = buff));

            var reduce_buff_duration = Helpers.Create<ContextActionReduceBuffDuration>(c => { c.TargetBuff = buff; c.DurationValue = Helpers.CreateContextDuration(1, DurationRate.Minutes); });
            foreach (var e in frost_bite_enchantments)
            {
                e.AddComponent(Helpers.Create<NewMechanics.EnchantmentMechanics.ActionOnAttackWithEnchantedWeapon>(a => { a.ActionsOnSelf = Helpers.CreateActionList(reduce_buff_duration); a.only_on_hit = true; }));
            }

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), is_from_spell: true);
            var frost_bite_weapon = Helpers.CreateAbility("FrostBiteWeaponAbility",
                                                  buff.Name,
                                                  buff.Description,
                                                  "",
                                                  buff.Icon,
                                                  AbilityType.Spell,
                                                  Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                  AbilityRange.DoubleMove,
                                                  Helpers.minutesPerLevelDuration,
                                                  Helpers.savingThrowNone,
                                                  Helpers.CreateRunActions(Common.createContextActionForceAttack()),
                                                  Helpers.CreateSpellComponent(Kingmaker.Blueprints.Classes.Spells.SpellSchool.Transmutation),
                                                  Helpers.Create<NewMechanics.AbilityCasterPrimaryHandFree>(),
                                                  Helpers.CreateSpellDescriptor(SpellDescriptor.Cold),
                                                  Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionOnContextCaster(apply_buff)))
                                                  );
            frost_bite_weapon.setMiscAbilityParametersTouchHarmful();

            frost_bite_weapon.AvailableMetamagic = Kingmaker.UnitLogic.Abilities.Metamagic.Extend | Kingmaker.UnitLogic.Abilities.Metamagic.Heighten | Kingmaker.UnitLogic.Abilities.Metamagic.Empower | Kingmaker.UnitLogic.Abilities.Metamagic.Maximize | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Rime | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental;

            var frost_bite_charge = Helpers.CreateAbility("FrostBiteChargeAbility",
                                      "Frostbite",
                                      "Your melee touch attack deals 1d6 points of cold damage + 1 point per level (max + 10), and the target is fatigued. This spell cannot make a creature exhausted even if it is already fatigued. You can use this melee touch attack up to one time per caster level.",
                                      "",
                                      buff.Icon,
                                      AbilityType.Spell,
                                      Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                      AbilityRange.Touch,
                                      "",
                                      Helpers.savingThrowNone,
                                      Helpers.CreateRunActions(apply_fatigued, Helpers.CreateActionDealDamage(DamageEnergyType.Cold, Helpers.CreateContextDiceValue(DiceType.D6, 1, Helpers.CreateContextValue(AbilityRankType.DamageBonus)))),
                                      Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CasterLevel, progression: ContextRankProgression.DivStep,
                                                                            type: AbilityRankType.DamageBonus, stepLevel: 1, max: 10),
                                      Helpers.CreateSpellComponent(Kingmaker.Blueprints.Classes.Spells.SpellSchool.Transmutation),
                                      Helpers.CreateSpellDescriptor(SpellDescriptor.Cold),
                                      Helpers.CreateDeliverTouch(),
                                      library.Get<BlueprintAbility>("c83447189aabc72489164dfc246f3a36").GetComponent<AbilitySpawnFx>() //from frigid touch
                                      );

            frost_bite_charge.AvailableMetamagic = frost_bite_weapon.AvailableMetamagic;
            frost_bite_charge.setMiscAbilityParametersTouchHarmful();
            var frost_bite_sticky = Helpers.CreateTouchSpellCast(frost_bite_charge);
            frost_bite_sticky.AddComponents(Helpers.Create<StickyTouchMechnics.AbilityEffectStickyTouchMultiple>(a => a.num_charges = Helpers.CreateContextValue(AbilityRankType.ProjectilesCount)),
                                 Helpers.CreateContextRankConfig(type: AbilityRankType.ProjectilesCount)
                                 );

            frost_bite = Common.createVariantWrapper("FrostBiteAbility", "", frost_bite_sticky, frost_bite_weapon);
            frost_bite.AddComponent(frost_bite_charge.GetComponent<SpellComponent>());
            frost_bite.AddToSpellList(Helpers.druidSpellList, 1);
            frost_bite.AddToSpellList(Helpers.magusSpellList, 1);
            frost_bite.AddSpellAndScroll("1cd597e316ac49941a568312de2be6ae"); //acid maw
        }


        static void createChillTouch()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/ChillTouch.png");

            BlueprintWeaponEnchantment[] chill_touch_echantments = new BlueprintWeaponEnchantment[10];
            var negative_damage = Common.createEnergyDamageDescription(Kingmaker.Enums.Damage.DamageEnergyType.NegativeEnergy);

            var frost_enchant = library.Get<BlueprintWeaponEnchantment>("421e54078b7719d40915ce0672511d0b");
            touch_slam_negative = library.CopyAndAdd<BlueprintWeaponType>("f18cbcb39a1b35643a8d129b1ec4e716", "TouchSlamNegativeType", "");//slam
            touch_slam_negative.Category = WeaponCategory.Touch;
            Helpers.SetField(touch_slam_negative, "m_IsTwoHanded", false);

            var damage_type = new DamageTypeDescription()
            {
                Type = DamageType.Energy,
                Energy = DamageEnergyType.NegativeEnergy,
                Common = new DamageTypeDescription.CommomData(),
                Physical = new DamageTypeDescription.PhysicalData()
            };

            Helpers.SetField(touch_slam_negative, "m_DamageType", damage_type);
            Helpers.SetField(touch_slam_negative, "m_TypeNameText", Helpers.CreateString("TouchSlamNegativeName", "Touch"));
            Helpers.SetField(touch_slam_negative, "m_DefaultNameText", Helpers.CreateString("TouchSlamNegativeName", "Touch"));



            var weapon = library.CopyAndAdd<BlueprintItemWeapon>("767e6932882a99c4b8ca95c88d823137", "ChillTouchWeapon", "");
            Helpers.SetField(weapon, "m_Type", touch_slam_negative);
            Helpers.SetField(weapon, "m_DisplayNameText", Helpers.CreateString("ChillTouchWeaponName", "Chill Touch"));
            Helpers.SetField(weapon, "m_Icon", icon);
            Common.addEnchantment(weapon, WeaponEnchantments.summoned_weapon_enchant);

            var frightened = library.CopyAndAdd<BlueprintBuff>("f08a7239aa961f34c8301518e71d4cdf", "ChillTouchUndeadFrightenedBuff", "");
            frightened.RemoveComponents<SpellDescriptorComponent>();
            var undead = library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");


            for (int i = 0; i < chill_touch_echantments.Length; i++)
            {
                var on_living = Helpers.CreateConditionalSaved(null, Helpers.CreateActionDealDamage(StatType.Strength, Helpers.CreateContextDiceValue(DiceType.Zero, bonus: 1)));
                var on_undead = Helpers.CreateConditionalSaved(null, Common.createContextActionApplyBuff(frightened, Helpers.CreateContextDuration(i + 1, diceType: DiceType.D4, diceCount: 1), is_from_spell: true));
                var effect = Helpers.CreateConditional(Helpers.CreateConditionHasFact(undead),
                                                       Common.createContextActionSavingThrow(SavingThrowType.Will, Helpers.CreateActionList(on_undead)),
                                                       Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(on_living))
                                                      );
                chill_touch_echantments[i] = Common.createWeaponEnchantment($"ChillTouch{i}Enchantment",
                                                                             "Chill Touch",
                                                                             "A touch from your hand, which glows with blue energy, disrupts the life force of living creatures. Each touch channels negative energy that deals 1d6 points of damage. The touched creature also takes 1 point of Strength damage unless it makes a successful Fortitude saving throw.  Each attack you make reduces the remaining duration of the spell by 1 minute.\n"
                                                                             + "An undead creature you touch takes no damage of either sort, but it must make a successful Will saving throw or flee as if panicked for 1d4 rounds + 1 round per caster level (max + 10)\n."
                                                                             + "Your primary hand must be free when you cast this spell.",
                                                                             "",
                                                                             "",
                                                                             "",
                                                                             0,
                                                                             frost_enchant.WeaponFxPrefab,
                                                                             Helpers.Create<NewMechanics.EnchantmentMechanics.Immaterial>(),
                                                                             Helpers.Create<NewMechanics.EnchantmentMechanics.NoDamageScalingEnchant>(),
                                                                             Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(effect))
                                                                             );
            }

            var empower_buff = Common.createBuffContextEnchantPrimaryHandWeaponIfHasMetamagic(Kingmaker.UnitLogic.Abilities.Metamagic.Empower,
                                                                                                  false, false,
                                                                                                  new BlueprintWeaponType[0], WeaponEnchantments.empower_enchant);
            var maximize_buff = Common.createBuffContextEnchantPrimaryHandWeaponIfHasMetamagic(Kingmaker.UnitLogic.Abilities.Metamagic.Maximize,
                                                                                                  false, false,
                                                                                                  new BlueprintWeaponType[0], WeaponEnchantments.maximize_enchant);
            var buff = Helpers.CreateBuff("ChillTouchBuff",
                                            "Chill Touch (As Weapon)",
                                            chill_touch_echantments[0].Description,
                                            "",
                                            icon,
                                            null,
                                            Helpers.Create<NewMechanics.EnchantmentMechanics.CreateWeapon>(c => { c.weapon = weapon; }),
                                            Common.createBuffContextEnchantPrimaryHandWeapon(Helpers.CreateContextValue(AbilityRankType.DamageBonus), false, false,
                                                                                            new BlueprintWeaponType[0], chill_touch_echantments),
                                            empower_buff,
                                            maximize_buff,
                                            Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CasterLevel, progression: ContextRankProgression.DivStep,
                                                                            type: AbilityRankType.DamageBonus, stepLevel: 1, max: 10)
                                            );
            buff.Stacking = Kingmaker.UnitLogic.Buffs.Blueprints.StackingType.Replace;
            weapon.AddComponent(Helpers.Create<NewMechanics.EnchantmentMechanics.WeaponSourceBuff>(w => w.buff = buff));

            var reduce_buff_duration = Helpers.Create<ContextActionReduceBuffDuration>(c => { c.TargetBuff = buff; c.DurationValue = Helpers.CreateContextDuration(1, DurationRate.Minutes); });
            foreach (var e in chill_touch_echantments)
            {
                e.AddComponent(Helpers.Create<NewMechanics.EnchantmentMechanics.ActionOnAttackWithEnchantedWeapon>(a => { a.ActionsOnSelf = Helpers.CreateActionList(reduce_buff_duration); a.only_on_hit = true; }));
            }

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), is_from_spell: true);
            var chill_touch_weapon = Helpers.CreateAbility("ChillTouchWeaponAbility",
                                                  buff.Name,
                                                  buff.Description,
                                                  "",
                                                  buff.Icon,
                                                  AbilityType.Spell,
                                                  Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                  AbilityRange.DoubleMove,
                                                  Helpers.minutesPerLevelDuration,
                                                  "Fortitude partial or Will negates",
                                                  Helpers.CreateRunActions(Common.createContextActionForceAttack()),
                                                  Helpers.CreateSpellComponent(Kingmaker.Blueprints.Classes.Spells.SpellSchool.Necromancy),
                                                  Helpers.Create<NewMechanics.AbilityCasterPrimaryHandFree>(),
                                                  Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionOnContextCaster(apply_buff)))
                                                  );
            chill_touch_weapon.setMiscAbilityParametersTouchHarmful();

            chill_touch_weapon.AvailableMetamagic = Kingmaker.UnitLogic.Abilities.Metamagic.Extend | Kingmaker.UnitLogic.Abilities.Metamagic.Heighten | Kingmaker.UnitLogic.Abilities.Metamagic.Empower | Kingmaker.UnitLogic.Abilities.Metamagic.Maximize | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing;


            var charge_on_living = Helpers.CreateConditionalSaved(null, Helpers.CreateActionDealDamage(StatType.Strength, Helpers.CreateContextDiceValue(DiceType.Zero, bonus: 1)));
            var charge_on_undead = Helpers.CreateConditionalSaved(null, Common.createContextActionApplyBuff(frightened, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), diceType: DiceType.D4, diceCount: 1), is_from_spell: true));
            var charge_effect = Helpers.CreateConditional(Helpers.CreateConditionHasFact(undead),
                                                   new GameAction[] { Common.createContextActionSavingThrow(SavingThrowType.Will, Helpers.CreateActionList(charge_on_undead)) },
                                                   new GameAction[] {Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(charge_on_living)),
                                                                     Helpers.CreateActionDealDamage(DamageEnergyType.NegativeEnergy, Helpers.CreateContextDiceValue(DiceType.D6, 1, 0))
                                                                     }
                                                  );
            var chill_touch_charge = Helpers.CreateAbility("ChillTouchChargeAbility",
                                                           "Chill Touch",
                                                           "A touch from your hand, which glows with blue energy, disrupts the life force of living creatures. Each touch channels negative energy that deals 1d6 points of damage. The touched creature also takes 1 point of Strength damage unless it makes a successful Fortitude saving throw.  You can make one touch attack per caster level.\n"
                                                           + "An undead creature you touch takes no damage of either sort, but it must make a successful Will saving throw or flee as if panicked for 1d4 rounds + 1 round per caster level (max + 10)\n.",
                                                           "",
                                                           chill_touch_weapon.Icon,
                                                           AbilityType.Spell,
                                                           UnitCommand.CommandType.Standard,
                                                           AbilityRange.Touch,
                                                           "",
                                                           "Fortitude partial or Will negates",
                                                           Helpers.CreateRunActions(charge_effect),
                                                           Helpers.CreateDeliverTouch(),
                                                           Helpers.CreateSpellComponent(Kingmaker.Blueprints.Classes.Spells.SpellSchool.Necromancy),
                                                           library.Get<BlueprintAbility>("cbecdd04ad2523c438123ef596fd2b9f").GetComponent<AbilitySpawnFx>(), //touch of fatigue fx
                                                           Helpers.CreateContextRankConfig(max: 10)
                                                           );
            chill_touch_charge.setMiscAbilityParametersTouchHarmful();
            chill_touch_charge.SpellResistance = true;
            chill_touch_charge.AvailableMetamagic = chill_touch_weapon.AvailableMetamagic;
            var chill_touch_sticky = Helpers.CreateTouchSpellCast(chill_touch_charge);
            chill_touch_sticky.AddComponents(Helpers.Create<StickyTouchMechnics.AbilityEffectStickyTouchMultiple>(a => a.num_charges = Helpers.CreateContextValue(AbilityRankType.ProjectilesCount)),
                                             Helpers.CreateContextRankConfig(type: AbilityRankType.ProjectilesCount)
                                             );

            chill_touch = Common.createVariantWrapper("ChillTouchAbility", "", chill_touch_sticky, chill_touch_weapon);
            chill_touch.AddComponent(chill_touch_charge.GetComponent<SpellComponent>());

            chill_touch.AddToSpellList(Helpers.wizardSpellList, 1);
            chill_touch.AddToSpellList(Helpers.magusSpellList, 1);
            chill_touch.AddSpellAndScroll("01bd2a92d4112864b871995c93456b54"); //bestow curse
        }


        static void createWinterGrasp()
        {
            var icon = library.Get<BlueprintFeature>("6ac87a3af9ccf014787c49745df75e6a").Icon; //chilling infusion

            var difficult_terrain = library.CopyAndAdd<BlueprintBuff>("1914ccc0f3da5b1439f0b90d90d05811", "WinterGraspDifficultTerrainBuff", "");

            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("eca936a9e235875498d1e74ff7c09ecd", "WinterGraspArea", ""); //spike stones
            area.Size = 20.Feet();

            var chill_buff = Helpers.CreateBuff("WinterGraspChillBuff",
                                                "Winter's Grasp",
                                                $"Ice encrusts the ground, radiating supernatural cold and making it hard for creatures to maintain their balance. This icy ground is treated as difficult terrain. A creature that begins its turn in the affected area takes 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of cold damage and takes a –2 penalty on saving throws against spells with the cold descriptor for 1 round.",
                                                "",
                                                icon,
                                                Common.createPrefabLink("d7f055790fceee34e87c4902877c894f"), //cold creature
                                                Common.createContextSavingThrowBonusAgainstDescriptor(-2, ModifierDescriptor.UntypedStackable, SpellDescriptor.Cold),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.Cold)
                                                );

            area.Fx = new Kingmaker.ResourceLinks.PrefabLink();
            area.Fx.AssetId = "fd21d914e9f6f5e4faa77365549ad0a7";
            area.ReplaceComponent<AbilityAreaEffectBuff>(a => a.Buff = difficult_terrain);
            area.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Cold));
            var area_effect = Helpers.CreateAreaEffectRunAction(round: new GameAction[]{Helpers.CreateActionDealDamage(DamageEnergyType.Cold, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 1)),
                                                                                        Common.createContextActionApplyBuff(chill_buff, Helpers.CreateContextDuration(1), is_child: true)
                                                                                        }
                                                               );
            area.ReplaceComponent<AbilityAreaEffectRunAction>(area_effect);

            var spawn_area = Common.createContextActionSpawnAreaEffect(area, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Rounds));
            winter_grasp = Helpers.CreateAbility("WinterGraspAbility",
                                                  chill_buff.Name,
                                                  chill_buff.Description,
                                                  "",
                                                  icon,
                                                  AbilityType.Spell,
                                                  Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                  AbilityRange.Medium,
                                                  Helpers.roundsPerLevelDuration,
                                                  Helpers.savingThrowNone,
                                                  Helpers.Create<AbilityEffectRunActionOnClickedTarget>(a => a.Action = Helpers.CreateActionList(spawn_area)),
                                                  Helpers.CreateSpellComponent(SpellSchool.Conjuration),
                                                  Helpers.CreateSpellDescriptor(SpellDescriptor.Cold | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Water),
                                                  Helpers.CreateAbilityTargetsAround(20.Feet(), TargetType.Any)
                                                  );

            winter_grasp.setMiscAbilityParametersRangedDirectional();
            winter_grasp.SpellResistance = false;
            winter_grasp.AvailableMetamagic = Metamagic.Extend | Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Rime | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            winter_grasp.AddToSpellList(Helpers.druidSpellList, 2);

            winter_grasp.AddSpellAndScroll("341915f4a228e184e8d28886ea551a17");
        }


        static void createBloodMist()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/BloodMist.png");

            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("aa2e0a0fe89693f4e9205fd52c5ba3e5", "BloodMistArea", "");
            area.Fx = Common.createPrefabLink("bce77ff907d02f04b883ae53d77a983f");
            area.Size = 30.Feet();

            var buff = Helpers.CreateBuff("BloodMistBuff",
                                          "Blood Mist",
                                          "This spell summons forth a misty cloud of rust-red toxic algae. Any creature within the mist is coated by it, turning the creature the same reddish color. Any creature within the mist must save or take 1d4 points of Wisdom damage and become enraged, attacking any creatures it detects nearby (as the “attack nearest creature” result of the confused condition). An enraged creature remains so as long as the spell is in effect. A creature only needs to save once each time it is within the mist (though leaving and returning requires another save).",
                                          "",
                                          icon,
                                          null,
                                          Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.AttackNearest),
                                          Helpers.CreateSpellDescriptor(SpellDescriptor.Poison));

            var concleament_buff = library.CopyAndAdd<BlueprintBuff>("dd3ad347240624d46a11a092b4dd4674", "BloodMistConcleamentBuff", "");
            concleament_buff.SetName("Blood Mist Concealment");
            concleament_buff.SetDescription(buff.Description);
            concleament_buff.FxOnStart = null;
            concleament_buff.ReplaceComponent<AddConcealment>(a => a.Descriptor = ConcealmentDescriptor.Fog);
            concleament_buff.AddComponent(Common.createOutgoingConcelement(concleament_buff.GetComponent<AddConcealment>()));

            var undead = library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");
            var construct = library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f");
            var apply_attack = Helpers.CreateConditionalSaved(new GameAction[0],
                                                              new GameAction[]{Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), is_child: true, is_permanent: true),
                                                                               Helpers.CreateActionDealDamage(StatType.Wisdom, Helpers.CreateContextDiceValue(DiceType.D4, 1, 0))
                                                                              }
                                                              );
            var apply_buff = Helpers.CreateConditional(new Condition[] { Helpers.CreateConditionHasFact(undead, not: true), Helpers.CreateConditionHasFact(construct, not: true) },
                                                       Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(apply_attack))
                                                       );
            var apply_concleament = Common.createContextActionApplyBuff(concleament_buff, Helpers.CreateContextDuration(), is_child: true, is_permanent: true);

            var mist_action = Helpers.CreateAreaEffectRunAction(unitEnter: new GameAction[] { apply_buff },
                                                                unitExit: new GameAction[] {Helpers.Create<ContextActionRemoveBuffSingleStack>(r => r.TargetBuff = buff)
                                                                                            }
                                                               );

            area.ReplaceComponent<AbilityAreaEffectRunAction>(mist_action);

            blood_mist = library.CopyAndAdd<BlueprintAbility>("68a9e6d7256f1354289a39003a46d826", "BloodMistAbility", "");
            blood_mist.RemoveComponents<SpellListComponent>();
            blood_mist.ReplaceComponent<AbilityAoERadius>(a => Helpers.SetField(a, "m_Radius", 30.Feet()));
            blood_mist.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(Common.changeAction<ContextActionSpawnAreaEffect>(a.Actions.Actions, s => s.AreaEffect = area)));
            blood_mist.SetNameDescriptionIcon(buff.Name, buff.Description, buff.Icon);
            blood_mist.AvailableMetamagic = Metamagic.Heighten | Metamagic.Extend | Metamagic.Reach | Metamagic.Quicken | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent;
            blood_mist.AddToSpellList(Helpers.druidSpellList, 8);
            blood_mist.AddSpellAndScroll("c92308c160d6d424fb64f1fd708aa6cd");//stiking cloud
        }

        static void createSavageMaw()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/SavageMaw.png");
            var buff = library.CopyAndAdd<BlueprintBuff>("a67b51a8074ae47438280be0a87b01b6", "SavageMawBuff", "");//animal fury
            var bleed1d4 = library.Get<BlueprintBuff>("5eb68bfe186d71a438d4f85579ce40c1");
            var apply_bleed = Common.createContextActionApplyBuff(bleed1d4, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);
            buff.AddComponent(Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(Helpers.CreateActionList(apply_bleed), critical_hit: true, weapon_category: WeaponCategory.Bite));

            var roar = library.CopyAndAdd<BlueprintAbility>("5f3126d4120b2b244a95cb2ec23d69fb", "SavageMawRoarAbility", "");//dazzling display
            roar.RemoveComponents<NewMechanics.AbilityCasterEquippedWeaponCheckHasParametrizedFeature>(); //remove weapon requirement added by rebalance
            roar.ActionType = Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift;
            roar.SetNameDescriptionIcon("Savage Maw: Roar",
                                        "Your teeth extend and sharpen, transforming your mouth into a maw of razor-sharp fangs. You gain a secondary bite attack that deals 1d4 points of damage plus your Strength modifier. If you confirm a critical hit with this attack, it also deals 1d4 bleed damage. You can end this spell before its normal duration by making a bestial roar as a swift action. When you do, you can make an Intimidate check to demoralize all foes within a 30-foot radius that can hear the roar.",
                                        icon);
            buff.AddComponent(Helpers.CreateAddFact(roar));
            buff.SetBuffFlags(BuffFlags.RemoveOnRest);

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), is_from_spell: true);

            savage_maw = Helpers.CreateAbility("SavageMawAbility",
                                               "Savage Maw",
                                                roar.Description,
                                                "",
                                                roar.Icon,
                                                AbilityType.Spell,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Personal,
                                                Helpers.minutesPerLevelDuration,
                                                "",
                                                Helpers.CreateRunActions(apply_buff),
                                                Common.createAbilitySpawnFx("352469f228a3b1f4cb269c7ab0409b8e", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                                Helpers.CreateContextRankConfig()
                                                );
            savage_maw.setMiscAbilityParametersSelfOnly();
            savage_maw.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken;
            var remove_self = Common.createContextActionOnContextCaster(Common.createContextActionRemoveBuff(buff));
            //roar.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(a.Actions.Actions.AddToArray(Common.createContextActionRemoveBuff(buff))));
            roar.AddComponent(Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(remove_self)));

            savage_maw.AddToSpellList(Helpers.druidSpellList, 2);
            savage_maw.AddToSpellList(Helpers.clericSpellList, 2);
            savage_maw.AddToSpellList(Helpers.inquisitorSpellList, 2);
            savage_maw.AddToSpellList(Helpers.magusSpellList, 2);
            savage_maw.AddToSpellList(Helpers.rangerSpellList, 1);

            savage_maw.AddSpellAndScroll("1cd597e316ac49941a568312de2be6ae");
        }


        static void createKeenEdge()
        {
            var keen_edge_enchant = library.Get<BlueprintWeaponEnchantment>("102a9c8c9b7a75e4fb5844e79deaf4c0");

            var icon = library.Get<BlueprintActivatableAbility>("27d76f1afda08a64d897cc81201b5218").Icon; //keen weapon bond

            var keen_edge1 = library.CopyAndAdd<BlueprintAbility>("831e942864e924846a30d2e0678e438b", "KeenEdgePrimaryHandAbility", "");

            keen_edge1.SetIcon(icon);
            keen_edge1.SetDescription("This spell makes a weapon magically keen, improving its ability to deal telling blows. This transmutation doubles the threat range of the weapon. A threat range of 20 becomes 19-20, a threat range of 19-20 becomes 17-20, and a threat range of 18-20 becomes 15-20. The spell can be cast only on piercing or slashing weapons. If cast on arrows or crossbow bolts, the keen edge on a particular projectile ends after one use, whether or not the missile strikes its intended target. Treat shuriken as arrows, rather than as thrown weapons, for the purpose of this spell.\n"
                + "Multiple effects that increase a weapon’s threat range (such as the keen special weapon property and the Improved Critical feat) don’t stack. You can’t cast this spell on a natural weapon, such as a claw.");
            keen_edge1.SetName("Keen Edge");
            keen_edge1.setMiscAbilityParametersTouchFriendly();
            keen_edge1.RemoveComponents<AbilityDeliverTouch>();
            var action_old = (keen_edge1.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionEnchantWornItem);

            var action = Common.createItemEnchantmentAction(keen_edge_enchant.name + "PrimaryHandKeenEdgeBuff",
                                                Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.TenMinutes),
                                                keen_edge_enchant,
                                                true,
                                                off_hand: false
                                                );
            keen_edge1.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(action));
            keen_edge1.LocalizedDuration = Helpers.tenMinPerLevelDuration;

            var keen_edge2 = library.CopyAndAdd(keen_edge1, "KeenEdgeSecondaryHandAbility", "");
            keen_edge2.SetName("Keen Edge (Off-Hand)");
            var action2 = Common.createItemEnchantmentAction(keen_edge_enchant.name + "SecondaryHandKeenEdgeBuff",
                                                Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.TenMinutes),
                                                keen_edge_enchant,
                                                true,
                                                off_hand: true
                                                );
            keen_edge2.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(action2));
            keen_edge2.AddComponent(Helpers.Create<NewMechanics.AbilitTargetManufacturedWeapon>(a => { a.off_hand = true; a.works_on_summoned = true; }));
            keen_edge1.AddComponent(Helpers.Create<NewMechanics.AbilitTargetManufacturedWeapon>(a => a.works_on_summoned = true));

            keen_edge = Common.createVariantWrapper("KeenEdgeAbility", "", keen_edge1, keen_edge2);
            keen_edge.AddComponent(Helpers.CreateSpellComponent(SpellSchool.Transmutation));
            keen_edge.AddToSpellList(Helpers.inquisitorSpellList, 3);
            keen_edge.AddToSpellList(Helpers.magusSpellList, 3);
            keen_edge.AddToSpellList(Helpers.wizardSpellList, 3);

            

            keen_edge.AddSpellAndScroll("fbdd06f0414c3ef458eb4b2a8072e502");
        }


        static void createFlameArrow()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/FlameArrow.png");
            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Fire, Helpers.CreateContextDiceValue(DiceType.D6, 1), IgnoreCritical: true);
            var buff = Helpers.CreateBuff("FlameArrowBuff",
                                          "Flame Arrow",
                                          "This spell allows you to turn ammunition (such as arrows, crossbow bolts, shuriken, and sling stones) into fiery projectiles. Each piece of ammunition deals an extra 1d6 points of fire damage to any target it hits. A flaming projectile can easily ignite a flammable object or structure, but it won’t ignite a creature it strikes.",
                                          "",
                                          icon,
                                          null,
                                          Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(Helpers.CreateActionList(dmg), wait_for_attack_to_resolve: true, weapon_category: WeaponCategory.Longbow),
                                          Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(Helpers.CreateActionList(dmg), wait_for_attack_to_resolve: true, weapon_category: WeaponCategory.Shortbow),
                                          Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(Helpers.CreateActionList(dmg), wait_for_attack_to_resolve: true, weapon_category: WeaponCategory.LightCrossbow),
                                          Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(Helpers.CreateActionList(dmg), wait_for_attack_to_resolve: true, weapon_category: WeaponCategory.HeavyCrossbow),
                                          Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(Helpers.CreateActionList(dmg), wait_for_attack_to_resolve: true, weapon_category: WeaponCategory.LightRepeatingCrossbow),
                                          Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(Helpers.CreateActionList(dmg), wait_for_attack_to_resolve: true, weapon_category: WeaponCategory.HeavyRepeatingCrossbow),
                                          Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(Helpers.CreateActionList(dmg), wait_for_attack_to_resolve: true, weapon_category: WeaponCategory.HandCrossbow),
                                          Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(Helpers.CreateActionList(dmg), wait_for_attack_to_resolve: true, weapon_category: WeaponCategory.Sling),
                                          Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(Helpers.CreateActionList(dmg), wait_for_attack_to_resolve: true, weapon_category: WeaponCategory.Javelin),
                                          Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(Helpers.CreateActionList(dmg), wait_for_attack_to_resolve: true, weapon_category: WeaponCategory.SlingStaff)
                                          );
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), is_from_spell: true);
            flame_arrow = Helpers.CreateAbility("FlameArrowAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                icon,
                                                AbilityType.Spell,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Touch,
                                                Helpers.minutesPerLevelDuration,
                                                "",
                                                Helpers.CreateRunActions(apply_buff),
                                                Common.createAbilitySpawnFx("352469f228a3b1f4cb269c7ab0409b8e", anchor: AbilitySpawnFxAnchor.SelectedTarget), //CommonTransmutationBuff00
                                                Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                                Helpers.CreateContextRankConfig()
                                                );

            flame_arrow.AvailableMetamagic = Metamagic.Empower | Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Maximize | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Rime | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental;

            flame_arrow.setMiscAbilityParametersTouchFriendly();

            flame_arrow.AddToSpellList(Helpers.magusSpellList, 3);
            flame_arrow.AddToSpellList(Helpers.wizardSpellList, 3);

            flame_arrow.AddSpellAndScroll("ce41e625eae914d4bad729f090e9001f"); //hurricane arrow
        }


        static void createCountlessEyes()
        {
            var improved_uncanny_dodge = library.Get<BlueprintFeature>("485a18c05792521459c7d06c63128c79");
            countless_eyes = library.CopyAndAdd<BlueprintAbility>("c927a8b0cd3f5174f8c0b67cdbfde539", "CountlessEyesAbility", "");
            countless_eyes.LocalizedSavingThrow = Helpers.CreateString(countless_eyes.name + ".LocalizedSavingThrow", HarmlessSaves.HarmlessSaves.will_harmless);
            countless_eyes.SetName("Countless Eyes");
            countless_eyes.SetDescription("The target sprouts extra eyes all over its body, including on the back of its head. It gains all-around vision and cannot be flanked.");
            countless_eyes.LocalizedDuration = Helpers.CreateString("CountlessEyes.Duration", Helpers.hourPerLevelDuration);
            var buff = Helpers.CreateBuff("NoFlankingBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          improved_uncanny_dodge.ComponentsArray[0]
                                          );
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Hours), true);
            countless_eyes.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(apply_buff));
            countless_eyes.RemoveComponents<SpellListComponent>();
            countless_eyes.ReplaceComponent<SpellComponent>(s => s.School = SpellSchool.Transmutation);
            countless_eyes.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach | Metamagic.Extend;

            countless_eyes.AddToSpellList(Helpers.alchemistSpellList, 3);
            countless_eyes.AddToSpellList(Helpers.inquisitorSpellList, 3);
            countless_eyes.AddToSpellList(Helpers.wizardSpellList, 3);
            countless_eyes.RemoveComponents<HarmlessSaves.HarmlessSpell>();
            countless_eyes.AddComponent(Helpers.Create<HarmlessSaves.HarmlessSpell>());

            countless_eyes.AddSpellAndScroll("de172db6e10f6d54896cb6a48b9fe8f7");
        }


        static void createRighteousVigor()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/RighteousVigor.png");
            var freedom_of_movement = library.Get<BlueprintAbility>("4c349361d720e844e846ad8c19959b1e");

            var buff = Helpers.CreateBuff("RighteousVigorBuff",
                                          "Righteous Vigor",
                                          "Infusing the target with a surge of furious divine energy, you enhance a creature’s ability to hit an opponent based on the number of times it has already hit that opponent with a successful attack. Each time the subject successfully strikes an opponent with a successful melee attack, the subject gains a cumulative +1 morale bonus on attack rolls (maximum +4 bonus) and gains 1d8 temporary hit points (to a maximum of 20 temporary hit points). If an attack misses, the attack bonus resets to +0 but any accumulated temporary hit points remain. The temporary hit points disappear at the end of the spell’s duration.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.CreateAddStatBonus(StatType.AdditionalAttackBonus, 0, ModifierDescriptor.Morale),
                                          Helpers.Create<NewMechanics.PersistentTemporaryHitPoints>(t => { t.Value = 0; t.Descriptor = ModifierDescriptor.Morale; }),
                                          Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting)
                                          );
            var on_hit_actions = Helpers.CreateActionList(Common.createAddStackingStatBonusToModifierFromFact(1, StatType.AdditionalAttackBonus, ModifierDescriptor.Morale, buff, 4),
                                                          Common.createAddStackingStatBonusToModifierFromFact(Helpers.CreateContextDiceValue(DiceType.D8, 1), StatType.TemporaryHitPoints,
                                                                                                               ModifierDescriptor.Morale, buff, 20)
                                                         );

            var on_hit = Common.createAddInitiatorAttackWithWeaponTrigger(on_hit_actions, on_initiator: true);

            var on_miss_actions = Helpers.CreateActionList(Common.createAddStackingStatBonusToModifierFromFact(0, StatType.AdditionalAttackBonus, ModifierDescriptor.Morale, buff, 0, set: true));
            var on_miss = Helpers.Create<NewMechanics.AddInitiatorAttackRollMissTrigger>(a => { a.Action = on_miss_actions; a.OnOwner = true; });

            buff.AddComponents(on_hit, on_miss);

            righteous_vigor = Helpers.CreateAbility("RighteousVigorAbility",
                                                    buff.Name,
                                                    buff.Description,
                                                    "",
                                                    buff.Icon,
                                                    AbilityType.Spell,
                                                    Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                    AbilityRange.Touch,
                                                    Helpers.roundsPerLevelDuration,
                                                    "",
                                                    freedom_of_movement.GetComponent<AbilitySpawnFx>(),
                                                    Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), is_from_spell: true)),
                                                    Helpers.CreateSpellComponent(SpellSchool.Enchantment),
                                                    Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting)
                                                    );
            righteous_vigor.setMiscAbilityParametersTouchFriendly();
            righteous_vigor.AvailableMetamagic = Metamagic.Empower | Metamagic.Extend | Metamagic.Quicken | Metamagic.Reach | Metamagic.Heighten | Metamagic.Maximize;

            righteous_vigor.AddToSpellList(Helpers.paladinSpellList, 2);
            righteous_vigor.AddToSpellList(Helpers.inquisitorSpellList, 3);
            righteous_vigor.AddSpellAndScroll("f49fc4e47cef56e42a49d561289dd500");
        }


        static void createForceSword()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/ForceSword.png");
            var air_enchatment = library.Get<BlueprintWeaponEnchantment>("1d64abd0002b98043b199c0e3109d3ee");
            var force_damage = Common.createForceDamageDescription();

            var force_enchant = Common.createWeaponEnchantment("ForceWeaponEnchantment", "Force", "This weapon deals force damage.", "", "", "", 0, air_enchatment.WeaponFxPrefab);
            force_enchant.AddComponent(Helpers.Create<NewMechanics.EnchantmentMechanics.WeaponDamageChange>(w =>
                                                                                                            {
                                                                                                                w.dice_formula = new DiceFormula(1, DiceType.D8);
                                                                                                                w.damage_type_description = force_damage;
                                                                                                            }
                                                                                                            )
                                      );

            BlueprintWeaponEnchantment[] enchants = new BlueprintWeaponEnchantment[]
            {
                WeaponEnchantments.standard_enchants[0],
                WeaponEnchantments.standard_enchants[1],
                WeaponEnchantments.standard_enchants[2]
            };



            var weapon = library.CopyAndAdd<BlueprintItemWeapon>("6fd0a849531617844b195f452661b2cd", "ForceSwordWeapon", "");//longsword

            Helpers.SetField(weapon, "m_DamageType", force_damage);
            Helpers.SetField(weapon, "m_DisplayNameText", Helpers.CreateString("ForceBladeName", "Force Sword"));
            Helpers.SetField(weapon, "m_Icon", icon);
            Common.addEnchantment(weapon, WeaponEnchantments.summoned_weapon_enchant);


            var buff = Helpers.CreateBuff("ForceSwordBuff",
                                            "Force Sword",
                                            "You create a +1 longsword of pure force sized appropriately for you that you can wield or give to another creature like any other longsword. At 8th level, the sword functions as a +2 longsword. "
                                            + "At 13th level, it functions as a + 3 longsword. A force sword cannot be attacked or harmed by physical attacks, but dispel magic, disintegrate, a sphere of annihilation, or a rod of cancellation affects it.\n"
                                            + "Target's primary hand must be free when you cast this spell.",
                                            "",
                                            icon,
                                            null,
                                            Helpers.Create<NewMechanics.EnchantmentMechanics.CreateWeapon>(c => c.weapon = weapon),
                                            Common.createBuffContextEnchantPrimaryHandWeapon(Helpers.CreateContextValue(AbilityRankType.DamageBonus), false, false,
                                                                                            new BlueprintWeaponType[] { }, enchants),
                                            Common.createBuffContextEnchantPrimaryHandWeapon(1, false, false, new BlueprintWeaponType[] { }, force_enchant),
                                            Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CasterLevel, progression: ContextRankProgression.StartPlusDivStep,
                                                                            type: AbilityRankType.DamageBonus, stepLevel: 5, startLevel: 3, max: 3)
                                            );
            buff.Stacking = Kingmaker.UnitLogic.Buffs.Blueprints.StackingType.Replace;

            force_sword = library.CopyAndAdd<BlueprintAbility>(shillelagh.AssetGuid, "ForceSwordAbility", "");
            force_sword.setMiscAbilityParametersTouchFriendly();
            force_sword.Range = AbilityRange.Touch;
            force_sword.NeedEquipWeapons = false;
            force_sword.SetIcon(icon);
            force_sword.SetName(buff.Name);
            force_sword.SetDescription(buff.Description);
            force_sword.ActionType = Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard;

            force_sword.ReplaceComponent<NewMechanics.AbilitTargetMainWeaponCheck>(Helpers.Create<NewMechanics.AbilityTargetPrimaryHandFree>());
            force_sword.ReplaceComponent<SpellComponent>(Helpers.CreateSpellComponent(Kingmaker.Blueprints.Classes.Spells.SpellSchool.Evocation));

            var apply_buff = Common.createContextActionApplyBuff(buff,
                                                    Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes),
                                                    is_from_spell: true
                                                );
            force_sword.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(apply_buff));
            force_sword.AvailableMetamagic = Metamagic.Heighten | Metamagic.Reach | Metamagic.Extend;
            force_sword.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Force));

            force_sword.AddToSpellList(Helpers.magusSpellList, 2);
            force_sword.AddToSpellList(Helpers.wizardSpellList, 2);
            force_sword.AddSpellAndScroll("7a02193480a473f44b8c307627985f97"); //blade barrier
        }


        static void createBloodArmor()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/BloodArmor.png");
            var blood_fx = library.Get<BlueprintBuff>("a1ffec0ce7c167a40aaea13dc49b757b").FxOnStart;

            BlueprintBuff[] blood_armor_buffs = new BlueprintBuff[5];

            for (int i = 0; i < blood_armor_buffs.Length; i++)
            {
                blood_armor_buffs[i] = Helpers.CreateBuff($"BloodArmor{i + 1}Buff",
                                                          "",
                                                          "",
                                                          "",
                                                          icon,
                                                          blood_fx,
                                                          Common.createBuffContextEnchantArmor(1, false, true, new BlueprintArmorEnchantment[] { ArmorEnchantments.temporary_armor_enchantments[i] })
                                                          );
                blood_armor_buffs[i].SetBuffFlags(BuffFlags.HiddenInUi);
            }


            var conditional = Helpers.CreateConditional(Helpers.CreateConditionHasBuff(blood_armor_buffs[0]),
                                            new GameAction[] {Common.createContextActionRemoveBuff(blood_armor_buffs[0]),
                                                              Common.createContextActionApplyBuff(blood_armor_buffs[1], Helpers.CreateContextDuration(), is_from_spell: true, is_child: true, is_permanent: true, dispellable: false)
                                                             },
                                            new GameAction[] { Common.createContextActionApplyBuff(blood_armor_buffs[0], Helpers.CreateContextDuration(), is_from_spell: true, is_child: true, is_permanent: true, dispellable: false) }
                                           );
            for (int i = 1; i < blood_armor_buffs.Length; i++)
            {
                GameAction[] if_true = null;
                if (i < blood_armor_buffs.Length - 1)
                {
                    if_true = new GameAction[] {Common.createContextActionRemoveBuff(blood_armor_buffs[i]),
                                                Common.createContextActionApplyBuff(blood_armor_buffs[i+1],
                                                                                    Helpers.CreateContextDuration(),
                                                                                    is_from_spell: true,
                                                                                    is_child: true,
                                                                                    is_permanent:true,
                                                                                    dispellable: false)
                                               };
                }
                conditional = Helpers.CreateConditional(Helpers.CreateConditionHasBuff(blood_armor_buffs[i]),
                                                        if_true,
                                                        new GameAction[] { conditional });

            }


            var on_dmg = Helpers.Create<NewMechanics.AddIncomingDamageTriggerOnAttacker>(a =>
                                                                                        {
                                                                                            a.on_self = true;
                                                                                            a.min_dmg = 5;
                                                                                            a.consider_damage_type = true;
                                                                                            a.Actions = Helpers.CreateActionList(conditional);
                                                                                            a.physical_types = new PhysicalDamageForm[] { PhysicalDamageForm.Piercing, PhysicalDamageForm.Slashing };
                                                                                        }
                                                                                        );

            var buff = Helpers.CreateBuff("BloodArmorBuff",
                                          "Blood Armor",
                                          "Your blood becomes as hard as iron upon contact with air. Each time you take at least 5 points of piercing or slashing damage, your armor gains a +1 enhancement bonus to your AC. An outfit of regular clothing counts as armor that grants no AC bonus for the purpose of this spell. This enhancement bonus stacks with itself, but not with an existing enhancement bonus, to a maximum enhancement bonus of +5. This spell has no effect while underwater or in environments that lack air.",
                                          "",
                                          icon,
                                          null,
                                          on_dmg);
            buff.Stacking = StackingType.Replace;

            var apply_buff = Common.createContextActionApplyBuff(buff,
                                                     Helpers.CreateContextDuration(bonus: Helpers.CreateContextValue(AbilityRankType.Default), rate: DurationRate.Minutes),
                                                     is_from_spell: true);

            blood_armor = library.CopyAndAdd<BlueprintAbility>("9e1ad5d6f87d19e4d8883d63a6e35568", "BloodArmorAbility", "");//mage armor
            blood_armor.setMiscAbilityParametersSelfOnly();
            blood_armor.Range = AbilityRange.Personal;
            blood_armor.SetIcon(buff.Icon);
            blood_armor.SetName(buff.Name);
            blood_armor.SetDescription(buff.Description);
            blood_armor.setMiscAbilityParametersSelfOnly();
            blood_armor.LocalizedDuration = Helpers.CreateString("BloodArmor.Duration", Helpers.minutesPerLevelDuration);
            blood_armor.RemoveComponents<RecommendationNoFeatFromGroup>();
            blood_armor.ReplaceComponent<SpellComponent>(s => s.School = SpellSchool.Transmutation);
            blood_armor.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(apply_buff));
            blood_armor.AddComponent(Helpers.Create<NewMechanics.AbilitTargetHasArmor>());

            blood_armor.AddToSpellList(Helpers.alchemistSpellList, 2);
            blood_armor.AddToSpellList(Helpers.wizardSpellList, 2);

            blood_armor.AddSpellAndScroll("e8308a74821762e49bc3211358e81016");
        }

        static void createPoisonBreath()
        {
            poison_breath = library.CopyAndAdd<BlueprintAbility>("d797007a142a6c0409a74b064065a15e", "PoisonBreathAbility", "");
            poison_breath.SetName("Poison Breath");
            poison_breath.SetDescription("You expel a cone-shaped burst of toxic mist from your mouth, subjecting everyone caught in the area to a deadly poison, as per the poison spell.");
            poison_breath.Range = AbilityRange.Projectile;
            poison_breath.setMiscAbilityParametersRangedDirectional();
            poison_breath.Type = AbilityType.Spell;
            poison_breath.ActionType = Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard;
            poison_breath.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Selective;
            poison_breath.RemoveComponents<AbilitySpawnFx>();
            poison_breath.RemoveComponents<AbilityDeliverTouch>();

            var poison_cone = library.Get<BlueprintProjectile>("2758f6a35e0e3544f8d7367e57f70d61");
            poison_breath.AddComponent(Common.createAbilityDeliverProjectile(AbilityProjectileType.Cone, poison_cone, 15.Feet(), 5.Feet()));

            poison_breath.AddToSpellList(Helpers.clericSpellList, 7);
            poison_breath.AddToSpellList(Helpers.druidSpellList, 6);

            poison_breath.AddSpellAndScroll("423304685924a3445bd135221496400b");//poison
        }


        static void createVineStrike()
        {
            var entangle_buff = library.Get<BlueprintBuff>("f7f6330726121cf4b90a6086b05d2e38");
            var cooldown_buff = Helpers.CreateBuff("VineStrikeCooldownBuff",
                                                   "Vine Strike: Cooldown",
                                                   "Bristles burst from your body, lodging in your opponent and blossoming into entangling vines as you pummel your target. While this spell is in effect, one of your natural attacks or unarmed strikes deals an additional 1d6 points of damage, and each creature hit with that natural weapon or unarmed strike must succeed at a Reflex save or be entangled for the duration of the spell; on a successful Reflex save, the creature is immune to the entangled effect for 1 round. A creature entangled by this spell can spend a standard action to remove the vines, but can be entangled again by further unarmed strikes.",
                                                   "",
                                                   entangle_buff.Icon,
                                                   null);

            var apply_entangle = Common.createContextActionApplyBuff(entangle_buff, Helpers.CreateContextDuration(1));
            var apply_cooldown = Common.createContextActionApplyBuff(cooldown_buff, Helpers.CreateContextDuration(1));

            var save_action = Helpers.CreateConditionalSaved(new GameAction[] { apply_cooldown }, new GameAction[] { apply_entangle, apply_cooldown });
            var effect_action = Helpers.CreateConditional(Common.createContextConditionHasBuffFromCaster(cooldown_buff, not: true),
                                                          Common.createContextActionSavingThrow(SavingThrowType.Reflex, Helpers.CreateActionList(save_action))
                                                          );
            var dmg = Helpers.CreateActionDealDamage(PhysicalDamageForm.Piercing, Helpers.CreateContextDiceValue(DiceType.D6, 1), IgnoreCritical: true);
            var action_list = Helpers.CreateActionList(dmg, effect_action);

            WeaponCategory[] categories = new WeaponCategory[] { WeaponCategory.UnarmedStrike, WeaponCategory.Claw, WeaponCategory.Bite, WeaponCategory.Gore, WeaponCategory.OtherNaturalWeapons };
            string[] category_name = new string[] { "Unarmed Strike", "Claw", "Bite", "Gore", "Other Natural Weapons" };

            List<BlueprintAbility> variants = new List<BlueprintAbility>();

            for (int i = 0; i < categories.Length; i++)
            {
                var buff = Helpers.CreateBuff("VineStrike" + categories[i].ToString() + "Buff",
                                              "Vine Strike: " + category_name[i],
                                              cooldown_buff.Description,
                                              "",
                                              cooldown_buff.Icon,
                                              null,
                                              Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(action_list, weapon_category: categories[i])
                                              );

                var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), is_from_spell: true);
                var ability = Helpers.CreateAbility("VineStrike" + categories[i].ToString() + "Ability",
                                                    buff.Name,
                                                    buff.Description,
                                                    "",
                                                    buff.Icon,
                                                    AbilityType.Spell,
                                                    Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                    AbilityRange.Personal,
                                                    Helpers.minutesPerLevelDuration,
                                                    "",
                                                    Helpers.CreateRunActions(apply_buff),
                                                    Common.createAbilitySpawnFx("352469f228a3b1f4cb269c7ab0409b8e", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                    Helpers.CreateSpellComponent(SpellSchool.Conjuration)
                                                    );
                ability.setMiscAbilityParametersSelfOnly();
                ability.AvailableMetamagic = Metamagic.Heighten | Metamagic.Empower | Metamagic.Extend | Metamagic.Maximize | Metamagic.Quicken | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing;
                variants.Add(ability);

            }

            vine_strike = Common.createVariantWrapper("VineStrikeAbility", "", variants.ToArray());
            vine_strike.SetName("Vine Strike");
            vine_strike.AddComponent(Helpers.CreateSpellComponent(SpellSchool.Conjuration));


            vine_strike.AddToSpellList(Helpers.druidSpellList, 2);
            vine_strike.AddToSpellList(Helpers.alchemistSpellList, 2);
            vine_strike.AddToSpellList(Helpers.rangerSpellList, 2);
            vine_strike.AddToSpellList(Helpers.wizardSpellList, 2);

            vine_strike.AddSpellAndScroll("5022612735a9e2345bfc5110106823d8");
        }

        static void createSheetLightning()
        {
            var constructs = library.Get<BlueprintFeature>("6ea5a4a19ccb81a498e18a229cc5038a");
            var undead = library.Get<BlueprintFeature>("5941963eae3e9864d91044ba771f2cc2");

            var chain_lightning = library.Get<BlueprintAbility>("645558d63604747428d55f0dd3a4cb58");
            var dazed = Common.dazed_non_mind_affecting;
            var dazzled = library.Get<BlueprintBuff>("df6d1025da07524429afbae248845ecc");

            var apply_dazed = Common.createContextActionApplyBuff(dazed, Helpers.CreateContextDuration(1));
            var apply_dazzled = Common.createContextActionApplyBuff(dazzled, Helpers.CreateContextDuration(1));
            var deal_damage = Helpers.CreateActionDealDamage(DamageEnergyType.Electricity, Helpers.CreateContextDiceValue(DiceType.Zero, 0, 1));


            var save_result = Helpers.CreateConditionalSaved(apply_dazzled, apply_dazed);
            var context_saved = Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(save_result));
            Common.addConditionalDCIncrease(context_saved, Helpers.CreateConditionsCheckerOr(Helpers.Create<NewMechanics.ContextConditionTargetHasMetalArmor>()), 2);

            sheet_lightning = Helpers.CreateAbility("SheetLightningAbility",
                                                    "Sheet Lightning",
                                                    "You create a dazzling flash of electricity that fills the target area. Sheet lightning inflicts 1 point of electricity damage to all creatures within the area of effect (no save). The true power of the spell, though, lies not in the damage it inflicts but in the overwhelming pain the lightning creates. The sudden flash and jolt dazes living creatures for 1 round if they fail a saving throw. Creatures that save are instead dazzled for 1 round. Any creature wearing metal armor takes a –2 penalty to its saving throw against this spell.",
                                                    "",
                                                    chain_lightning.Icon,
                                                    AbilityType.Spell,
                                                    Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                    AbilityRange.Medium,
                                                    Helpers.oneRoundDuration,
                                                    "Fortitude Special",
                                                    Helpers.CreateRunActions(deal_damage, Helpers.CreateConditional(Common.createContextConditionHasFacts(false, undead, constructs),
                                                                                                                    null,
                                                                                                                    context_saved)
                                                                            ),
                                                    Helpers.CreateSpellDescriptor(SpellDescriptor.Electricity),
                                                    Helpers.CreateAbilityTargetsAround(20.Feet(), TargetType.Any),
                                                    Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                    Common.createAbilitySpawnFx("86e72b9ddeef0324d9d23c48594d6b7d", AbilitySpawnFxAnchor.ClickedTarget) //thunderstorm kineticist blast aoe
                                                    );

            sheet_lightning.setMiscAbilityParametersRangedDirectional();
            sheet_lightning.SpellResistance = true;
            sheet_lightning.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken | Metamagic.Extend | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Rime | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;

            sheet_lightning.AddToSpellList(Helpers.druidSpellList, 3);
            sheet_lightning.AddToSpellList(Helpers.wizardSpellList, 3);
            sheet_lightning.AddSpellAndScroll("141ffc89f1630ff45812f50ed3922088");//chain lightning

        }


        static void createFlurryOfSnowballs()
        {
            flurry_of_snowballs = library.CopyAndAdd<BlueprintAbility>("e7c530f8137630f4d9d7ee1aa7b1edc0", "FlurryOfSnowballsAbility", "");
            flurry_of_snowballs.RemoveComponents<SpellListComponent>();
            flurry_of_snowballs.RemoveComponents<ContextRankConfig>();
            flurry_of_snowballs.SpellResistance = false;
            flurry_of_snowballs.SetName("Flurry of Snowballs");
            flurry_of_snowballs.SetDescription("You send a flurry of snowballs hurtling at your foes.\n"
                                                + $"Any creature in the area takes 4d{BalanceFixes.getDamageDieString(DiceType.D6)} points of cold damage from being pelted with the icy spheres.");
            var damage = Helpers.CreateActionDealDamage(DamageEnergyType.Cold, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 4), true, true);
            flurry_of_snowballs.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(damage));
            var cold_cone30 = library.Get<BlueprintProjectile>("72b45860bdfb81f4284aa005c04594dd");
            flurry_of_snowballs.ReplaceComponent<AbilityDeliverProjectile>(a => { a.Projectiles = new BlueprintProjectile[] { cold_cone30 }; a.Length = 30.Feet(); });
            flurry_of_snowballs.AvailableMetamagic = Metamagic.Quicken | Metamagic.Empower | Metamagic.Reach | Metamagic.Heighten | Metamagic.Maximize | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Rime | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental | (Metamagic)MetamagicFeats.MetamagicExtender.Selective;
            flurry_of_snowballs.ReplaceComponent<SpellDescriptorComponent>(s => s.Descriptor = s.Descriptor | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Water);

            flurry_of_snowballs.AddToSpellList(Helpers.magusSpellList, 2);
            flurry_of_snowballs.AddToSpellList(Helpers.druidSpellList, 2);
            flurry_of_snowballs.AddToSpellList(Helpers.wizardSpellList, 2);

            flurry_of_snowballs.AddSpellAndScroll("5344f2240620b27478d12f00643fc292");
        }


        static void createIceSlick()
        {
            var difficult_terrain = library.CopyAndAdd<BlueprintBuff>("1914ccc0f3da5b1439f0b90d90d05811", "IceSlickDifficultTerrainBuff", "");
            var slick_area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("eca936a9e235875498d1e74ff7c09ecd", "IceSlickArea", ""); //spike stones
            slick_area.Size = 5.Feet();

            slick_area.Fx = new Kingmaker.ResourceLinks.PrefabLink();
            slick_area.Fx.AssetId = "b6a8750499b0ec647ba68430e83bfc2f";// "d0b113580baee53449fe4c5cb8f941e0"; //obsidian
            slick_area.ReplaceComponent<AbilityAreaEffectBuff>(a => a.Buff = difficult_terrain);

            var apply_prone = Helpers.Create<ContextActionKnockdownTarget>();
            var area_effect = Helpers.CreateAreaEffectRunAction(//unitEnter: Common.createContextActionApplyBuff(difficult_terrain, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false),
                                                                //unitExit: Helpers.Create<ContextActionRemoveBuffSingleStack>(r => r.TargetBuff = difficult_terrain),
                                                                unitMove: Common.createContextActionSkillCheck(StatType.SkillMobility, failure: Helpers.CreateActionList(apply_prone), custom_dc: 10));
            slick_area.ReplaceComponent<AbilityAreaEffectRunAction>(area_effect);
            slick_area.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Ground));

            var deal_damage = Helpers.CreateActionDealDamage(DamageEnergyType.Cold, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 1, Helpers.CreateContextValue(AbilityRankType.DamageBonus)), true, true);
            var spawn_area = Common.createContextActionSpawnAreaEffect(slick_area, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes));
            ice_slick = Helpers.CreateAbility("IceSlickAbility",
                                              "Ice Slick",
                                              "You create a blast of intense cold, coating all solid surfaces in the area with a thin coating of ice.\n"
                                              + $"Any creature in the area when the spell is cast takes 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of cold damage + 1 point per caster level (maximum +10) and falls prone; creatures that succeed at a Reflex save take half damage and don’t fall prone. Spell resistance applies to this initial effect.\n"
                                              + "A creature can walk within or through the area of ice at half its normal speed with a successful DC 10 Mobility check. Failure means the creature can’t move and  it falls. Creatures that do not move on their turn do not need to attempt this check.",
                                              "",
                                              LoadIcons.Image2Sprite.Create(@"AbilityIcons/IceSlick.png"),
                                              AbilityType.Spell,
                                              Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                              AbilityRange.Close,
                                              Helpers.minutesPerLevelDuration,
                                              Helpers.reflexHalfDamage,
                                              Helpers.CreateRunActions(SavingThrowType.Reflex, deal_damage, Helpers.CreateConditionalSaved(null, apply_prone)),
                                              Helpers.CreateContextRankConfig(type: AbilityRankType.DamageBonus, max: 10),
                                              Helpers.Create<AbilityEffectRunActionOnClickedTarget>(a => a.Action = Helpers.CreateActionList(spawn_area)),
                                              Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                              Helpers.CreateSpellDescriptor(SpellDescriptor.Cold),
                                              Helpers.CreateAbilityTargetsAround(5.Feet(), TargetType.Any)
                                              );
            ice_slick.setMiscAbilityParametersRangedDirectional();
            ice_slick.SpellResistance = true;
            ice_slick.AvailableMetamagic = Metamagic.Extend | Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Rime | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            ice_slick.AddToSpellList(Helpers.druidSpellList, 2);
            ice_slick.AddToSpellList(Helpers.magusSpellList, 2);
            ice_slick.AddToSpellList(Helpers.rangerSpellList, 2);
            ice_slick.AddToSpellList(Helpers.wizardSpellList, 2);

            ice_slick.AddSpellAndScroll("a4fbba95ffa58144ca7189bc350ed622");
        }

        static void createStrongJaw()
        {
            var acid_maw = library.Get<BlueprintAbility>("75de4ded3e731dc4f84d978fe947dc67");
            var magic_fang = library.Get<BlueprintAbility>("403cf599412299a4f9d5d925c7b9fb33");

            var buff = Helpers.CreateBuff("StrongJawBuff",
                                          "Strong Jaw",
                                          "Laying a hand upon an allied creature’s jaw, claws, tentacles, or other natural weapons, you enhance the power of that creature’s natural attacks. Each natural attack that creature makes deals damage as if the creature were two sizes larger than it actually is. If the creature is already Gargantuan or Colossal-sized, double the amount of damage dealt by each of its natural attacks instead. This spell does not actually change the creature’s size; all of its statistics except the amount of damage dealt by its natural attacks remain unchanged.",
                                          "",
                                          acid_maw.Icon,
                                          null,
                                          Helpers.Create<SizeMechanics.DoubleWeaponSize>(d => d.categories = new WeaponCategory[] { WeaponCategory.OtherNaturalWeapons, WeaponCategory.Bite, WeaponCategory.Claw, WeaponCategory.Gore, WeaponCategory.UnarmedStrike })
                                          );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), true);
            strong_jaw = Helpers.CreateAbility("StrongJawAbility",
                                               buff.Name,
                                               buff.Description,
                                               "",
                                               buff.Icon,
                                               AbilityType.Spell,
                                               Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                               AbilityRange.Touch,
                                               Helpers.minutesPerLevelDuration,
                                               HarmlessSaves.HarmlessSaves.fort_harmless,
                                               Helpers.CreateRunActions(apply_buff),
                                               magic_fang.GetComponent<AbilitySpawnFx>(),
                                               Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                               Helpers.Create<HarmlessSaves.HarmlessSpell>(h => h.save_type = SavingThrowType.Fortitude)
                                               );
            strong_jaw.AvailableMetamagic = magic_fang.AvailableMetamagic;
            strong_jaw.setMiscAbilityParametersTouchFriendly();
            strong_jaw.AddToSpellList(Helpers.druidSpellList, 4);
            strong_jaw.AddToSpellList(Helpers.rangerSpellList, 3);

            strong_jaw.AddSpellAndScroll("1cd597e316ac49941a568312de2be6ae");
        }


        static void createContingency()
        {
            var evocation = library.Get<BlueprintFeature>("c46512b796216b64899f26301241e4e6");
            var divination_buff = library.Get<BlueprintBuff>("6d338078b1a8cdc41bf3a39f65247161");

            var contingency_give_ability_buff = Helpers.CreateBuff("ContingencyGiveAbilityBuff",
                                                                   "",
                                                                   "",
                                                                   "",
                                                                   null,
                                                                   null);
            contingency_give_ability_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var action_remove_contingency_use = Helpers.CreateActionList(Common.createContextActionRemoveBuff(contingency_give_ability_buff));
            var contingency_store_buff = Helpers.CreateBuff("ContingencyBuff",
                                                            "Contingency",
                                                            "You can place another spell upon your person so that it comes into effect at some point in the future whenever you desire. You will need to spend another full round action to apply companion spell.\n"
                                                            + "The spell to be brought into effect by the contingency must be one that affects your person and be of a spell level no higher than one-third your caster level (rounded down, maximum 6th level).\n"
                                                            + "At any moment during spell duration you can release a companion spell as a free action. You can use only one contingency spell at a time; if a second is cast, the first one (if still active) is dispelled.",
                                                            "",
                                                            evocation.Icon,
                                                            divination_buff.FxOnStart,
                                                            Helpers.Create<SpellManipulationMechanics.FactStoreSpell>(f => f.actions_on_store = action_remove_contingency_use),
                                                            Helpers.CreateAddFactContextActions(Common.createContextActionApplyBuff(contingency_give_ability_buff,
                                                                                                                                    Helpers.CreateContextDuration(),
                                                                                                                                    is_child: true, is_permanent: true)
                                                                                                                                    )
                                                            );
            contingency_store_buff.AddComponent(Helpers.Create<SpellManipulationMechanics.AddStoredSpellToCaption>(a => a.store_fact = contingency_store_buff));
            var release_action = Helpers.Create<SpellManipulationMechanics.ReleaseSpellStoredInSpecifiedBuff>(r => r.fact = contingency_store_buff);
            var remove_buff = Common.createContextActionRemoveBuff(contingency_store_buff);

            var contingency_release = Helpers.CreateAbility("ContingencyReleaseAbility",
                                                            "Contingency: Release",
                                                            contingency_store_buff.Description,
                                                            "",
                                                            contingency_store_buff.Icon,
                                                            AbilityType.SpellLike,
                                                            Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                            AbilityRange.Personal,
                                                            "",
                                                            "",
                                                            Helpers.CreateRunActions(release_action, remove_buff),
                                                            Helpers.Create<SpellManipulationMechanics.AbilityCasterHasSpellStoredInFact>(a => a.store_fact = contingency_store_buff)
                                                            );
            contingency_release.setMiscAbilityParametersSelfOnly();
            contingency_store_buff.AddComponent(Helpers.CreateAddFact(contingency_release));

            int max_variants = 10;
            Predicate<AbilityData> check_slot_predicate = delegate (AbilityData spell)
            {
                return spell.Spellbook != null
                        && spell.SpellLevel <= (spell.Spellbook.CasterLevel / 3)
                        && !spell.Spellbook.Blueprint.IsAlchemist
                        && spell.Blueprint.CanTargetSelf
                        && (!spell.Blueprint.HasVariants || spell.Variants.Count < max_variants)
                        && (!spell.RequireMaterialComponent || spell.HasEnoughMaterialComponent)
                        && !SpellManipulationMechanics.FactStoreSpell.hasSpellStoredInFact(spell.Caster, contingency_store_buff);
            };



            for (int i = 0; i < max_variants; i++)
            {
                var contingency_use = Helpers.CreateAbility($"ContingencyStoreAbility{i}",
                                                            "Contingency: Store",
                                                            contingency_store_buff.Description,
                                                            "",
                                                            contingency_store_buff.Icon,
                                                            AbilityType.Special,
                                                            Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                            AbilityRange.Personal,
                                                            "1 day/level or until discharged",
                                                            "",
                                                            Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                            Helpers.Create<SpellManipulationMechanics.AbilityStoreSpellInFact>(s =>
                                                                                                                              {
                                                                                                                                  s.fact = contingency_store_buff;
                                                                                                                                  s.check_slot_predicate = check_slot_predicate;
                                                                                                                                  s.variant = i;
                                                                                                                              }
                                                                                                                              )
                                                            );
                contingency_give_ability_buff.AddComponent(Helpers.CreateAddFact(contingency_use));
                contingency_use.setMiscAbilityParametersSelfOnly();
                Common.setAsFullRoundAction(contingency_use);
            }

            var apply_buff = Common.createContextActionApplyBuff(contingency_store_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), rate: DurationRate.Days), is_from_spell: true);
            contingency = Helpers.CreateAbility("ContingencyAbility",
                                                "Contingency",
                                                contingency_store_buff.Description,
                                                "",
                                                contingency_store_buff.Icon,
                                                AbilityType.Spell,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Personal,
                                                "1 day/level or until discharged",
                                                "",
                                                Helpers.CreateRunActions(apply_buff),
                                                Helpers.CreateContextRankConfig(),
                                                Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                Helpers.CreateContextRankConfig(),
                                                Helpers.Create<SharedSpells.CannotBeShared>(),
                                                Helpers.Create<NewMechanics.AbilityTargetIsCaster>()
                                                );
            contingency.setMiscAbilityParametersSelfOnly();
            Common.setAsFullRoundAction(contingency);
            contingency.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken;
            contingency.AddToSpellList(Helpers.wizardSpellList, 6);
            contingency.AddSpellAndScroll("beab337b352b5ac479698e2bbc08f4ce"); //circle of death
        }



        static void createDelayedConsumption()
        {
            var icon = library.Get<BlueprintAbility>("07b608fab304f894880898dc0764e6e5").Icon; //geniekind
            var divination_buff = library.Get<BlueprintBuff>("6d338078b1a8cdc41bf3a39f65247161");

            var delayed_consumption_give_ability_buff = Helpers.CreateBuff("DelayedConsumptionGiveAbilityBuff",
                                                                   "",
                                                                   "",
                                                                   "",
                                                                   null,
                                                                   null);
            delayed_consumption_give_ability_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var action_remove_delayed_consumption_use = Helpers.CreateActionList(Common.createContextActionRemoveBuff(delayed_consumption_give_ability_buff));
            var delayed_consumption_store_buff = Helpers.CreateBuff("DelayedConsumptionBuff",
                                                            "Delayed Consumption",
                                                            "When you consume this extract, you quickly consume another extract of your choice-this second extract’s effects do not come into effect until a later point.  The companion extract can be no higher than 4th level, and you must pay any costs associated with the companion extract when you consume it.\n"
                                                            + "At any point during the duration of this extract, you can cause the companion extract to take effect as a swift action. You can only have one delayed consumption in effect at one time. If a second is consumed, the first is dispelled without any effect.\n",
                                                            "",
                                                            icon,
                                                            divination_buff.FxOnStart,
                                                            Helpers.Create<SpellManipulationMechanics.FactStoreSpell>(f => f.actions_on_store = action_remove_delayed_consumption_use),
                                                            Helpers.CreateAddFactContextActions(Common.createContextActionApplyBuff(delayed_consumption_give_ability_buff,
                                                                                                                                    Helpers.CreateContextDuration(),
                                                                                                                                    is_child: true, is_permanent: true)
                                                                                                                                    )
                                                            );
            delayed_consumption_store_buff.AddComponent(Helpers.Create<SpellManipulationMechanics.AddStoredSpellToCaption>(a => a.store_fact = delayed_consumption_store_buff));
            var release_action = Helpers.Create<SpellManipulationMechanics.ReleaseSpellStoredInSpecifiedBuff>(r => r.fact = delayed_consumption_store_buff);
            var remove_buff = Common.createContextActionRemoveBuff(delayed_consumption_store_buff);

            var delay_consumption_release = Helpers.CreateAbility("DelayConsumptionReleaseAbility",
                                                            "Delayed Consumption: Take Effect",
                                                            delayed_consumption_store_buff.Description,
                                                            "",
                                                            delayed_consumption_store_buff.Icon,
                                                            AbilityType.SpellLike,
                                                            Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift,
                                                            AbilityRange.Personal,
                                                            "",
                                                            "",
                                                            Helpers.CreateRunActions(release_action, remove_buff),
                                                            Helpers.Create<SpellManipulationMechanics.AbilityCasterHasSpellStoredInFact>(a => a.store_fact = delayed_consumption_store_buff)
                                                            );
            delay_consumption_release.setMiscAbilityParametersSelfOnly();
            delayed_consumption_store_buff.AddComponent(Helpers.CreateAddFact(delay_consumption_release));

            int max_variants = 10;
            Predicate<AbilityData> check_slot_predicate = delegate (AbilityData spell)
            {
                return spell.Spellbook != null
                        && spell.SpellLevel <= 4
                        && spell.Spellbook.Blueprint.IsAlchemist
                        && spell.Blueprint.CanTargetSelf
                        && (!spell.Blueprint.HasVariants || spell.Variants.Count < max_variants)
                        && (!spell.RequireMaterialComponent || spell.HasEnoughMaterialComponent)
                        && !SpellManipulationMechanics.FactStoreSpell.hasSpellStoredInFact(spell.Caster, delayed_consumption_store_buff);
            };



            for (int i = 0; i < max_variants; i++)
            {
                var delay_consumption_use = Helpers.CreateAbility($"DelayConsumptionStoreAbility{i}",
                                                            "Delayed Consumption: Companion Extract",
                                                            delayed_consumption_store_buff.Description,
                                                            "",
                                                            delayed_consumption_store_buff.Icon,
                                                            AbilityType.Special,
                                                            Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                            AbilityRange.Personal,
                                                            "1 day/level or until discharged",
                                                            "",
                                                            Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                                            Helpers.Create<SpellManipulationMechanics.AbilityStoreSpellInFact>(s =>
                                                            {
                                                                s.fact = delayed_consumption_store_buff;
                                                                s.check_slot_predicate = check_slot_predicate;
                                                                s.variant = i;
                                                            }
                                                                                                                              )
                                                            );
                delayed_consumption_give_ability_buff.AddComponent(Helpers.CreateAddFact(delay_consumption_use));
                delay_consumption_use.setMiscAbilityParametersSelfOnly();
            }

            var apply_buff = Common.createContextActionApplyBuff(delayed_consumption_store_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), rate: DurationRate.Days), is_from_spell: true);
            delayed_consumption = Helpers.CreateAbility("DelayConsumptionAbility",
                                                "Delayed Consumption",
                                                delayed_consumption_store_buff.Description,
                                                "",
                                                delayed_consumption_store_buff.Icon,
                                                AbilityType.Spell,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Personal,
                                                "1 day/level or until discharged",
                                                "",
                                                Helpers.CreateRunActions(apply_buff),
                                                Helpers.CreateContextRankConfig(),
                                                Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                                Helpers.CreateContextRankConfig(),
                                                Helpers.Create<SharedSpells.CannotBeShared>(),
                                                Helpers.Create<NewMechanics.AbilityTargetIsCaster>()
                                                );
            delayed_consumption.setMiscAbilityParametersSelfOnly();

            delayed_consumption.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken;
            delayed_consumption.AddToSpellList(Helpers.alchemistSpellList, 5);
            delayed_consumption.AddSpellAndScroll("f948342d6a9f2ce49b6aa5f362569d72"); //geniekind
        }




        static void createFireShield()
        {
            var shield_of_dawn = library.Get<BlueprintAbility>("62888999171921e4dafb46de83f4d67d");
            var shield = library.Get<BlueprintAbility>("ef768022b0785eb43a18969903c537c4");
            var shield_of_dawn_buff = library.Get<BlueprintBuff>("07abad76e7b688242b56749cd25f5d3d");
            var shield_buff = library.Get<BlueprintBuff>("9c0fa9b438ada3f43864be8dd8b3e741");


            DamageEnergyType[] energy = new DamageEnergyType[] { DamageEnergyType.Fire, DamageEnergyType.Cold };
            SpellDescriptor[] descriptors = new SpellDescriptor[] { SpellDescriptor.Fire, SpellDescriptor.Cold };
            BlueprintBuff[] prototype_buffs = new BlueprintBuff[] { shield_of_dawn_buff, shield_buff };
            BlueprintAbility[] prototype_spells = new BlueprintAbility[] { shield_of_dawn, shield };
            string[] names = new string[] { "Warm Shield", "Chill Shield" };
            string[] descriptions = new string[] { "The flames are warm to the touch. You take only half damage from cold-based attacks. If such an attack allows a Reflex save for half damage, you take no damage on a successful saving throw.",
                                                   "The flames are cool to the touch. You take only half damage from fire-based attacks. If such an attack allows a Reflex save for half damage, you take no damage on a successful saving throw." };


            BlueprintAbility[] shields = new BlueprintAbility[energy.Length];

            for (int i = 0; i < shields.Length; i++)
            {
                var deal_damage = Helpers.CreateActionDealDamage(energy[i],
                                                                 Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Common.createSimpleContextValue(1), Helpers.CreateContextValue(AbilityRankType.DamageBonus))
                                                                 );

                var dmg_component = shield_of_dawn_buff.GetComponent<AddTargetAttackRollTrigger>().CreateCopy();
                dmg_component.ActionsOnAttacker = Helpers.CreateActionList(deal_damage);
                var buff = Helpers.CreateBuff($"FireShield{i + 1}Buff",
                                              names[i],
                                              descriptions[i],
                                              "",
                                              prototype_buffs[i].Icon,
                                              prototype_buffs[i].FxOnStart,
                                              Common.createAddEnergyDamageDurability(energy[i], 0.5f),
                                              Common.createEvasionAgainstDescriptor(descriptors[i], SavingThrowType.Fortitude),
                                              Common.createEvasionAgainstDescriptor(descriptors[i], SavingThrowType.Reflex),
                                              Common.createEvasionAgainstDescriptor(descriptors[i], SavingThrowType.Will),
                                              Helpers.CreateContextRankConfig(type: AbilityRankType.DamageBonus, max: 15),
                                              dmg_component,
                                              Helpers.CreateSpellDescriptor(descriptors[i])
                                              );

                fire_shield_buffs.Add(energy[i], buff);

                var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), true);
                shields[i] = Helpers.CreateAbility($"FireShield{i + 1}Ability",
                                                  buff.Name,
                                                  buff.Description,
                                                  "",
                                                  buff.Icon,
                                                  AbilityType.Spell,
                                                  Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                  AbilityRange.Personal,
                                                  Helpers.roundsPerLevelDuration,
                                                  Helpers.savingThrowNone,
                                                  Helpers.CreateRunActions(apply_buff),
                                                  prototype_spells[i].GetComponent<AbilitySpawnFx>(),
                                                  Helpers.CreateSpellDescriptor(descriptors[i]),
                                                  shield_of_dawn.GetComponent<SpellComponent>()
                                                  );
                shields[i].AvailableMetamagic = shields[i].AvailableMetamagic | (Metamagic)MetamagicFeats.MetamagicExtender.Rime | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing; 
                shields[i].setMiscAbilityParametersSelfOnly();
                fire_shield_variants.Add(energy[i], shields[i]);
                shields[i].SpellResistance = true;
            }

            fire_shield = Helpers.CreateAbility("FireShieldAbility",
                                                "Fire Shield",
                                                "This spell wreathes you in flame and causes damage to each creature that attacks you in melee. The flames also protect you from either cold-based or fire-based attacks, depending on if you choose cool or warm flames for your fire shield.\n"
                                                + $"Any creature striking you with its body or a hand - held weapon deals normal damage, but at the same time the attacker takes 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage + 1 point per caster level (maximum + 15). This damage is either cold damage (if you choose a chill shield) or fire damage (if you choose a warm shield). If the attacker has spell resistance, it applies to this effect. Creatures wielding melee weapons with reach are not subject to this damage if they attack you.",
                                                "",
                                                shield_of_dawn.Icon,
                                                AbilityType.Spell,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Personal,
                                                Helpers.roundsPerLevelDuration,
                                                Helpers.savingThrowNone,
                                                shield_of_dawn.GetComponent<SpellComponent>());
            fire_shield.AvailableMetamagic = shield_of_dawn.AvailableMetamagic | (Metamagic)MetamagicFeats.MetamagicExtender.Rime | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            fire_shield.setMiscAbilityParametersSelfOnly();
            fire_shield.SpellResistance = true;

            fire_shield.AddComponent(fire_shield.CreateAbilityVariants(shields));
            fire_shield.AddToSpellList(Helpers.wizardSpellList, 4);
            fire_shield.AddToSpellList(Helpers.magusSpellList, 4);
            fire_shield.AddToSpellList(Helpers.alchemistSpellList, 4);

            fire_shield.AddSpellAndScroll("8e0c81ac23fe75b4288c21ee57f55e3f"); // shield of dawn

            //replace 5th level spell in fire domain
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("881b2137a1779294c8956fe5b497cc35"), 
                                      SpellDuplicates.addDuplicateSpell(fire_shield, "FireDomain" + fire_shield.name), 5);
            //replace 4th level spell in sun domain
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("c85c8791ee13d4c4ea10d93c97a19afc"), fire_shield, 4);
        }


        static void createCommand()
        {
            var dominate_monster = library.Get<BlueprintAbility>("3c17035ec4717674cae2e841a190e757");
            BlueprintBuff[] buffs = new BlueprintBuff[]{Common.dazed_non_stun,
                                                         library.CopyAndAdd<BlueprintBuff>("bd9d11c630f645443b8a1061044d5cf0", "ProneCommandBuff", ""), //prone
                                                         library.CopyAndAdd<BlueprintBuff>("f08a7239aa961f34c8301518e71d4cdf", "FrightenedCommandBuff", "") //frightened
                                                        };
            buffs[2].ReplaceComponent<AddCondition>(Common.createBuffStatusCondition(Kingmaker.UnitLogic.UnitCondition.Frightened, SavingThrowType.Will, save_each_round: true));
            foreach (var b in buffs)
            {
                b.ReplaceComponent<BuffStatusCondition>(s => { s.SaveEachRound = true; s.SaveType = SavingThrowType.Will; });
            }
            string[] names = { "Halt", "Fall", "Run" };
            string[] descriptions = { "The subject stands in place for 1 round. It may not take any actions but is not considered helpless.",
                                      "On its turn, the subject falls to the ground and remains prone for 1 round. It may act normally while prone but takes any appropriate penalties.",
                                      "On its turn, the subject moves away from you as quickly as possible for 1 round. It may do nothing but move during its turn, and it provokes attacks of opportunity for this movement as normal." };

            List<BlueprintAbility> commands = new List<BlueprintAbility>();
            List<BlueprintAbility> greater_commands = new List<BlueprintAbility>();

            command = Helpers.CreateAbility("CommandSpellAbility",
                                            "Command",
                                            "You give the subject a single command, which it obeys to the best of its ability at its earliest opportunity.",
                                            "",
                                            dominate_monster.Icon,
                                            AbilityType.Spell,
                                            Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                            AbilityRange.Close,
                                            Helpers.oneRoundDuration,
                                            Helpers.willNegates,
                                            dominate_monster.GetComponent<SpellDescriptorComponent>().CreateCopy(s => s.Descriptor = s.Descriptor | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.LanguageDependent),
                                            dominate_monster.GetComponent<SpellComponent>()
                                            );

            command.setMiscAbilityParametersSingleTargetRangedHarmful();
            command.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken | Metamagic.Extend | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            command.SpellResistance = true;


            var checker_fact = dominate_monster.GetComponents<AbilityTargetHasNoFactUnless>().ToArray(); //vermin
            var does_not_work = dominate_monster.GetComponent<AbilityTargetHasFact>();//construct, plant type

            command_greater = Helpers.CreateAbility("CommandGreaterSpellAbility",
                                "Command, Greater",
                                "This spell functions like command, except several creatures may be affected, and the activities continue beyond 1 round. At the start of each commanded creature’s action after the first, it gets another Will save to attempt to break free from the spell. Each creature must receive the same command.",
                                "",
                                dominate_monster.Icon,
                                AbilityType.Spell,
                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                AbilityRange.Close,
                                Helpers.roundsPerLevelDuration,
                                Helpers.willNegates,
                                dominate_monster.GetComponent<SpellDescriptorComponent>().CreateCopy(s => s.Descriptor = s.Descriptor | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.LanguageDependent),
                                Helpers.CreateAbilityTargetsAround(15.Feet(), TargetType.Enemy),
                                dominate_monster.GetComponent<SpellComponent>(),
                                Helpers.CreateSpellDescriptor((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.LanguageDependent)
                                );

            command_greater.setMiscAbilityParametersRangedDirectional();
            command_greater.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach | Metamagic.Extend | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            command_greater.SpellResistance = true;

            for (int i = 0; i < buffs.Length; i++)
            {
                var variant_command = library.CopyAndAdd<BlueprintAbility>(command.AssetGuid, $"CommandSpell{i + 1}Ability", "");
                variant_command.SetDescription(descriptions[i]);
                variant_command.SetName($"Command ({names[i]})");

                var buff_action = Common.createContextSavedApplyBuff(buffs[i],
                                                                      Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Rounds)
                                                                     );
                var buff_save = Common.createContextActionSavingThrow(SavingThrowType.Will, Helpers.CreateActionList(buff_action));

                variant_command.AddComponent(Helpers.CreateRunActions(buff_save));
                variant_command.AddComponent(dominate_monster.GetComponent<AbilitySpawnFx>());
                variant_command.AddComponent(Helpers.CreateContextRankConfig(min: 1, max: 1)); 
                variant_command.AddComponents(dominate_monster.GetComponents<AbilityTargetHasNoFactUnless>());
                commands.Add(variant_command);

                var variant_greater = library.CopyAndAdd<BlueprintAbility>(command_greater.AssetGuid, $"CommandGreateSpell{i + 1}Ability", "");
                variant_greater.SetName($"Command, Greater ({names[i]})");
                variant_greater.SetDescription(descriptions[i]);
                variant_greater.AddComponent(Helpers.CreateContextRankConfig());

                variant_greater.AddComponent(Helpers.CreateRunActions(buff_save));
                variant_greater.AddComponent(dominate_monster.GetComponent<AbilitySpawnFx>());

                greater_commands.Add(variant_greater);
            }

            
            command.AddComponent(command.CreateAbilityVariants(commands));
            command.AddToSpellList(Helpers.clericSpellList, 1);
            command.AddToSpellList(Helpers.inquisitorSpellList, 1);
            command.AddSpellAndScroll("f199f6e5026488c499042900b572eb7f"); //dominate person

            command_greater.AddComponents(command_greater.CreateAbilityVariants(greater_commands));
            command_greater.AddToSpellList(Helpers.clericSpellList, 5);
            command_greater.AddToSpellList(Helpers.inquisitorSpellList, 5);
            command_greater.AddSpellAndScroll("f199f6e5026488c499042900b572eb7f"); //dominate person

            //nobility domain fix
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("8480f2d1ca764774895ee6fd610a568e"), command_greater, 5);

        }




        static void createSanctuary()
        {
            var lesser_restoration = library.Get<BlueprintAbility>("e84fc922ccf952943b5240293669b171");
            var sancturay_logic = Helpers.Create<SanctuaryMechanics.Sanctuary>(c =>
                                                                         {
                                                                             c.save_type = SavingThrowType.Will;
                                                                             c.offensive_action_effect = SanctuaryMechanics.OffensiveActionEffect.REMOVE_FROM_OWNER;
                                                                         }
                                                                         );
            sanctuary_buff = library.CopyAndAdd<BlueprintBuff>("525f980cb29bc2240b93e953974cb325", "SanctuaryBuff", "");//invisibility

            sanctuary_buff.ComponentsArray = new BlueprintComponent[] { sancturay_logic };

            var apply_buff = Common.createContextActionApplyBuff(sanctuary_buff,
                                                                Helpers.CreateContextDuration(bonus: Helpers.CreateContextValue(AbilityRankType.Default), rate: DurationRate.Rounds),
                                                                is_from_spell: true);

            sanctuary = Helpers.CreateAbility("SanctuaryAbility",
                                                "Sanctuary",
                                                "Any opponent attempting to directly attack the warded creature, even with a targeted spell, must attempt a Will save. If the save succeeds, the opponent can attack normally and is unaffected by that casting of the spell. If the save fails, the opponent can’t follow through with the attack, that part of its action is lost, and it can’t directly attack the warded creature for the duration of the spell. Those not attempting to attack the subject remain unaffected. This spell does not prevent the warded creature from being attacked or affected by area of effect spells. The subject cannot attack without breaking the spell but may use non-attack spells or otherwise act.",
                                                "",
                                                lesser_restoration.Icon,
                                                AbilityType.Spell,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Touch,
                                                Helpers.roundsPerLevelDuration,
                                                Helpers.willNegates,
                                                Helpers.CreateSpellComponent(SpellSchool.Abjuration),
                                                Helpers.CreateRunActions(apply_buff)
                                                );

            sanctuary_buff.SetDescription(sanctuary.Description);
            sanctuary_buff.SetIcon(sanctuary.Icon);

            sanctuary.CanTargetSelf = true;
            sanctuary.CanTargetPoint = false;
            sanctuary.CanTargetFriends = true;
            sanctuary.CanTargetEnemies = false;
            sanctuary.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Touch;
            sanctuary.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionTouch;
            sanctuary.AvailableMetamagic = Kingmaker.UnitLogic.Abilities.Metamagic.Extend | Kingmaker.UnitLogic.Abilities.Metamagic.Quicken | Kingmaker.UnitLogic.Abilities.Metamagic.Heighten | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent;
            sanctuary.AddToSpellList(Helpers.clericSpellList, 1);
            sanctuary.AddToSpellList(Helpers.inquisitorSpellList, 1);

            sanctuary.AddSpellAndScroll("c0af0b5277e91e347ade3aa8994b0d17"); //invisibility

            //replace 1st spell in protection domain
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("b750650400d9d554b880dbf4c8347b24"), sanctuary, 1);
        }



        static internal void createShillelagh()
        {
            var bless_weapon = library.Get<BlueprintAbility>("831e942864e924846a30d2e0678e438b");
            var enchant_dice = Helpers.Create<NewMechanics.EnchantmentMechanics.WeaponDamageChange>();
            enchant_dice.dice_formula = new DiceFormula(2, DiceType.D6);
            enchant_dice.bonus_damage = 0;
            enchant_dice.damage_type_description = null;
            var enchantment_size = Common.createWeaponEnchantment("ShillelaghEnchantment",
                                                                  "Shillelagh",
                                                                  "Your own non - magical club or quarterstaff becomes a weapon with a + 1 enhancement bonus on attack and damage rolls. A quarterstaff gains this enhancement for both ends of the weapon. It deals damage as if it were two size categories larger (a Small club or quarterstaff so transmuted deals 1d8 points of damage, a Medium 2d6, and a Large 3d6), +1 for its enhancement bonus. If you stop wielding it, the weapon loses magical properties.",
                                                                  "Shillelagh",
                                                                  "",
                                                                  "",
                                                                  0,
                                                                  null,
                                                                  enchant_dice);
            var enhantment1 = library.Get<BlueprintWeaponEnchantment>("d704f90f54f813043a525f304f6c0050");

            BlueprintWeaponType[] shillelagh_types = new BlueprintWeaponType[] {library.Get<BlueprintWeaponType>("26aa0672af2c7d84ba93bec37758c712"), // club
                                                                                library.Get<BlueprintWeaponType>("629736dabac7f9f4a819dc854eaed2d6")  // quarterstaff
                                                                               };
            var buff = Helpers.CreateBuff("ShillelaghBuff",
                                          enchantment_size.Name,
                                          enchantment_size.Description,
                                          "",
                                          LoadIcons.Image2Sprite.Create(@"AbilityIcons/Shillelagh.png"),
                                          null,
                                          Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), true, true, shillelagh_types, enchantment_size),
                                          Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), true, true, shillelagh_types, enhantment1)
                                          );
            buff.Stacking = Kingmaker.UnitLogic.Buffs.Blueprints.StackingType.Replace;


            var apply_buff = Common.createContextActionApplyBuff(buff,
                                                                 Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes),
                                                                 is_from_spell: true
                                                                );

            shillelagh = Helpers.CreateAbility("ShillelaghAbility",
                                               buff.Name,
                                               buff.Description,
                                               "",
                                               buff.Icon,
                                               AbilityType.Spell,
                                               Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                               AbilityRange.Personal,
                                               Helpers.minutesPerLevelDuration,
                                               HarmlessSaves.HarmlessSaves.will_harmless,
                                               bless_weapon.GetComponent<AbilitySpawnFx>(),
                                               Helpers.CreateContextRankConfig(),
                                               Helpers.CreateRunActions(apply_buff),
                                               Helpers.CreateSpellComponent(Kingmaker.Blueprints.Classes.Spells.SpellSchool.Transmutation),
                                               Common.createAbilitTargetMainWeaponCheck(shillelagh_types[0].Category, shillelagh_types[1].Category),
                                               Helpers.Create<HarmlessSaves.HarmlessSpell>()
                                               );
            shillelagh.NeedEquipWeapons = true;
            shillelagh.CanTargetSelf = true;
            shillelagh.CanTargetPoint = false;
            shillelagh.CanTargetFriends = false;
            shillelagh.CanTargetEnemies = false;
            shillelagh.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.EnchantWeapon;
            shillelagh.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionOmni;
            shillelagh.AvailableMetamagic = Kingmaker.UnitLogic.Abilities.Metamagic.Extend | Kingmaker.UnitLogic.Abilities.Metamagic.Quicken | Kingmaker.UnitLogic.Abilities.Metamagic.Heighten;
            shillelagh.AddToSpellList(Helpers.druidSpellList, 1);
            shillelagh.AddSpellAndScroll("98abe0fd52e9d7d49a4a94615acbbc60"); //boneshacker
        }


        static BlueprintAbility createFlameBladeVariant(string prefix, string display_name, string description,  UnityEngine.Sprite icon,
                                                        DamageEnergyType energy, SpellDescriptor descriptor,
                                                        BlueprintWeaponEnchantment fx_enchant)
        {
            var scimitar_type = library.Get<BlueprintWeaponType>("d9fbec4637d71bd4ebc977628de3daf3");
            var immaterial = Helpers.Create<NewMechanics.EnchantmentMechanics.Immaterial>();
            BlueprintWeaponEnchantment[] flame_blade_enchantments = new BlueprintWeaponEnchantment[11];
            var fire_damage = Common.createEnergyDamageDescription(energy);

            var weapon = library.CopyAndAdd<BlueprintItemWeapon>("5363519e36752d84698e03a86fb33afb", prefix + "Weapon", "");//scimitar
            var damage_type = new DamageTypeDescription()
            {
                Type = DamageType.Energy,
                Energy = energy,
                Common = new DamageTypeDescription.CommomData(),
                Physical = new DamageTypeDescription.PhysicalData()
            };

            Helpers.SetField(weapon, "m_DamageType", damage_type);
            Helpers.SetField(weapon, "m_DisplayNameText", Helpers.CreateString(prefix + "Name", display_name));
            Helpers.SetField(weapon, "m_Icon", icon);

            Common.addEnchantment(weapon, WeaponEnchantments.summoned_weapon_enchant);

            var weapon_off_hand = library.CopyAndAdd<BlueprintItemWeapon>(weapon, prefix + "WeaponOffHand", "");
            for (int i = 0; i < flame_blade_enchantments.Length; i++)
            {
                var flame_blade_enchant = Helpers.Create<NewMechanics.EnchantmentMechanics.WeaponDamageChange>(w =>
                {
                    w.bonus_damage = i;
                    w.dice_formula = new DiceFormula(1, DiceType.D8);
                    w.damage_type_description = fire_damage;
                });
                flame_blade_enchantments[i] = Common.createWeaponEnchantment(prefix + $"{i}Enchantment",
                                                                             display_name,
                                                                             description,
                                                                             "",
                                                                             "",
                                                                             "",
                                                                             0,
                                                                             fx_enchant.WeaponFxPrefab,
                                                                             immaterial,
                                                                             flame_blade_enchant
                                                                             );
            }

            var is_off_hand = new bool[] { false, true };
            var flame_blades = new List<BlueprintAbility>();

            foreach (var oh in is_off_hand)
            {
                var empower_buff = Common.createBuffContextEnchantPrimaryHandWeaponIfHasMetamagic(Kingmaker.UnitLogic.Abilities.Metamagic.Empower,
                                                                                                      false, false,
                                                                                                      new BlueprintWeaponType[] { scimitar_type }, WeaponEnchantments.empower_enchant);

                var maximize_buff = Common.createBuffContextEnchantPrimaryHandWeaponIfHasMetamagic(Kingmaker.UnitLogic.Abilities.Metamagic.Maximize,
                                                                                                      false, false,
                                                                                                       new BlueprintWeaponType[] { scimitar_type }, WeaponEnchantments.maximize_enchant);
                var fire_dmg = Common.createBuffContextEnchantPrimaryHandWeapon(Helpers.CreateContextValue(AbilityRankType.DamageBonus), false, false,
                                                                                                new BlueprintWeaponType[] { scimitar_type }, flame_blade_enchantments);
                if (oh)
                {
                    empower_buff.in_off_hand = true;
                    maximize_buff.in_off_hand = true;
                    fire_dmg.in_off_hand = true;
                }
                var buff = Helpers.CreateBuff((oh ? "OffHand" : "") + prefix + "Buff",
                                                flame_blade_enchantments[0].Name + (oh ? " (Off-Hand)" : ""),
                                                flame_blade_enchantments[0].Description,
                                                "",
                                                icon,
                                                null,
                                                Helpers.Create<NewMechanics.EnchantmentMechanics.CreateWeapon>(c => { c.weapon = oh ? weapon_off_hand : weapon; c.create_in_offhand = oh; }),
                                                fire_dmg,
                                                empower_buff,
                                                maximize_buff,
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CasterLevel, progression: ContextRankProgression.OnePlusDivStep,
                                                                                type: AbilityRankType.DamageBonus, stepLevel: 2, max: 10)
                                                );
                buff.Stacking = Kingmaker.UnitLogic.Buffs.Blueprints.StackingType.Replace;



                var flame_blade_v = library.CopyAndAdd<BlueprintAbility>(shillelagh.AssetGuid, (oh ? "OffHand" : "MainHand") + prefix + "Ability", "");
                flame_blade_v.LocalizedSavingThrow = Helpers.savingThrowNone;
                flame_blade_v.RemoveComponents<HarmlessSaves.HarmlessSpell>();
                flame_blade_v.setMiscAbilityParametersSelfOnly();
                flame_blade_v.NeedEquipWeapons = false;
                flame_blade_v.SetIcon(icon);
                flame_blade_v.SetName(buff.Name);
                flame_blade_v.SetDescription(buff.Description);
                flame_blade_v.ActionType = Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard;
                if (oh)
                {
                    flame_blade_v.ReplaceComponent<NewMechanics.AbilitTargetMainWeaponCheck>(Helpers.Create<NewMechanics.AbilityTargetSecondaryHandFree>());
                    weapon_off_hand.AddComponent(Helpers.Create<NewMechanics.EnchantmentMechanics.WeaponSourceBuff>(w => w.buff = buff));
                }
                else
                {
                    flame_blade_v.ReplaceComponent<NewMechanics.AbilitTargetMainWeaponCheck>(Helpers.Create<NewMechanics.AbilityTargetPrimaryHandFree>());
                    weapon.AddComponent(Helpers.Create<NewMechanics.EnchantmentMechanics.WeaponSourceBuff>(w => w.buff = buff));
                }

                flame_blade_v.ReplaceComponent<SpellComponent>(Helpers.CreateSpellComponent(Kingmaker.Blueprints.Classes.Spells.SpellSchool.Evocation));

                var apply_buff = Common.createContextActionApplyBuff(buff,
                                                        Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), is_from_spell: true
                                                    );
                flame_blade_v.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(apply_buff));
                flame_blade_v.AvailableMetamagic = Metamagic.Quicken |Metamagic.Extend | Metamagic.Heighten | Metamagic.Empower | Metamagic.Maximize | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing | (Metamagic)MetamagicFeats.MetamagicExtender.Rime;
                flame_blade_v.AddComponent(Helpers.CreateSpellDescriptor(descriptor));
                flame_blades.Add(flame_blade_v);
            }

            var spell = Common.createVariantWrapper(prefix + "Ability", "", flame_blades.ToArray());
            spell.AddComponents(Helpers.CreateSpellDescriptor(descriptor), Helpers.CreateSpellComponent(Kingmaker.Blueprints.Classes.Spells.SpellSchool.Evocation));
            return spell;
        }


        static void createFlameBladeElectric()
        {
            flame_blade_electric = createFlameBladeVariant("FlameBladeElectric",
                                                  "Flame Blade (Electric)",
                                                  "This spell works like flame blade, but deals electricity damage instead.\n"
                                                  + "Flame Blade: A 3 - foot - long, blazing beam of red - hot fire springs forth from your hand.You wield this blade - like beam as if it were a scimitar. Attacks with the flame blade are melee touch attacks.The blade deals 1d8 points of fire damage +1 point per two caster levels(maximum + 10). Since the blade is immaterial, your Strength modifier does not apply to the damage. A flame blade can ignite combustible materials such as parchment, straw, dry sticks, and cloth.\n"
                                                   + "Your hand must be free when you cast this spell.",
                                                  LoadIcons.Image2Sprite.Create(@"AbilityIcons/WeaponEvil.png"),
                                                  DamageEnergyType.Electricity,
                                                  SpellDescriptor.Electricity,
                                                  library.Get<BlueprintWeaponEnchantment>("38f12574e91296f42b6a264542cb32a0")); //kinetic blade electric
        }

        static void createFlameBlade()
        {
            flame_blade = createFlameBladeVariant("FlameBlade",
                                                  "Flame Blade",
                                                  "A 3 - foot - long, blazing beam of red - hot fire springs forth from your hand.You wield this blade - like beam as if it were a scimitar. Attacks with the flame blade are melee touch attacks.The blade deals 1d8 points of fire damage +1 point per two caster levels(maximum + 10). Since the blade is immaterial, your Strength modifier does not apply to the damage.A flame blade can ignite combustible materials such as parchment, straw, dry sticks, and cloth.\n"
                                                   + "Your hand must be free when you cast this spell.",
                                                  LoadIcons.Image2Sprite.Create(@"AbilityIcons/FlameBlade.png"),
                                                  DamageEnergyType.Fire,
                                                  SpellDescriptor.Fire,
                                                  library.Get<BlueprintWeaponEnchantment>("ed7b5eb80e2a974499c3dd7aeca71f88")); //kinetic blade fire

            flame_blade.AddToSpellList(Helpers.druidSpellList, 2);
            flame_blade.AddSpellAndScroll("fbdd06f0414c3ef458eb4b2a8072e502"); //bless weapon
        }


        static void createProduceFlame()
        {
            var fireball = library.Get<BlueprintAbility>("2d81362af43aeac4387a3d4fced489c3");
            var flaming_enchatment = library.Get<BlueprintWeaponEnchantment>("30f90becaaac51f41bf56641966c4121");

            BlueprintWeaponEnchantment[] produce_flame_enchantments = new BlueprintWeaponEnchantment[11];
            var fire_damage = Common.createEnergyDamageDescription(Kingmaker.Enums.Damage.DamageEnergyType.Fire);


            var weapon_type = library.CopyAndAdd<BlueprintWeaponType>("f807334ef058b7148a5d1582767c70ab", "ProduceFlameType", "");//sling
            weapon_type.Category = WeaponCategory.Ray;
            Helpers.SetField(weapon_type, "m_IsTwoHanded", false);

            Helpers.SetField(weapon_type, "m_Enchantments", new BlueprintWeaponEnchantment[0]); //remove str bonus

            var damage_type = new DamageTypeDescription()
            {
                Type = DamageType.Energy,
                Energy = DamageEnergyType.Fire,
                Common = new DamageTypeDescription.CommomData(),
                Physical = new DamageTypeDescription.PhysicalData()
            };

            Helpers.SetField(weapon_type, "m_DamageType", damage_type);
            Helpers.SetField(weapon_type, "m_TypeNameText", Helpers.CreateString("ProduceFlameTypeName", "Ray"));
            Helpers.SetField(weapon_type, "m_DefaultNameText", Helpers.CreateString("ProduceFlameTypeName", "Ray"));


            var weapon = library.CopyAndAdd<BlueprintItemWeapon>("d30a1e8901890a04eaddaceb4abd7002", "ProduceFlameWeapon", "");//sling
            Helpers.SetField(weapon, "m_Type", weapon_type);
            Helpers.SetField(weapon, "m_DisplayNameText", Helpers.CreateString("ProduceFlameWeaponName", "Produce Flame"));
            Helpers.SetField(weapon, "m_Icon", fireball.Icon);
            var fire_ray = library.Get<BlueprintProjectile>("30a5f408ea9d163418c86a7107fc4326");
            Helpers.SetField(weapon, "m_VisualParameters", Common.replaceProjectileInWeaponVisualParameters(weapon.VisualParameters, fire_ray));
            Common.addEnchantment(weapon, WeaponEnchantments.summoned_weapon_enchant);

            for (int i = 0; i < produce_flame_enchantments.Length; i++)
            {
                var produce_flame_enchant = Helpers.Create<NewMechanics.EnchantmentMechanics.WeaponDamageChange>(w =>
                {
                    w.bonus_damage = 1 + i;
                    w.dice_formula = new DiceFormula(1, BalanceFixes.getDamageDie(DiceType.D6));
                    w.damage_type_description = fire_damage;
                });

                produce_flame_enchantments[i] = Common.createWeaponEnchantment($"ProduceFlame{i}Enchantment",
                                                                             "Produce Flame",
                                                                             "Flames as bright as a torch appear in your open hand. The flames harm neither you nor your equipment.\n"
                                                                             + $"In addition to providing illumination, the flames can be hurled or used to touch enemies. You can strike an opponent with a melee touch attack, dealing fire damage equal to 1d{BalanceFixes.getDamageDieString(DiceType.D6)} + 1 point per caster level (maximum +5). Alternatively, you can hurl the flames up to 40 feet as a thrown weapon. When doing so, you attack with a ranged touch attack (with no range penalty) and deal the same damage as with the melee attack. No sooner do you hurl the flames than a new set appears in your hand. Each attack you make reduces the remaining duration by 1 minute. If an attack reduces the remaining duration to 0 minutes or less, the spell ends after the attack resolves.\n"
                                                                             + "Your primary hand must be free when you cast this spell.",
                                                                             "",
                                                                             "",
                                                                             "",
                                                                             0,
                                                                             flaming_enchatment.WeaponFxPrefab,
                                                                             Helpers.Create<NewMechanics.EnchantmentMechanics.RangedTouchEnchant>(),
                                                                             Helpers.Create<NewMechanics.EnchantmentMechanics.NoDamageScalingEnchant>(),
                                                                             Helpers.Create<NewMechanics.EnchantmentMechanics.DoNotProvokeAooEnchant>(),
                                                                             produce_flame_enchant
                                                                             );
            }


            var empower_buff = Common.createBuffContextEnchantPrimaryHandWeaponIfHasMetamagic(Kingmaker.UnitLogic.Abilities.Metamagic.Empower,
                                                                                                  false, false,
                                                                                                  new BlueprintWeaponType[0], WeaponEnchantments.empower_enchant);

            var maximize_buff = Common.createBuffContextEnchantPrimaryHandWeaponIfHasMetamagic(Kingmaker.UnitLogic.Abilities.Metamagic.Maximize,
                                                                                                  false, false,
                                                                                                  new BlueprintWeaponType[0], WeaponEnchantments.maximize_enchant);

            /*var cold_buff = Common.createBuffContextEnchantPrimaryHandWeaponIfHasMetamagic((Metamagic)MetamagicFeats.MetamagicExtender.ElementalCold,
                                                                               false, false,
                                                                               new BlueprintWeaponType[0], WeaponEnchantments.elemental_enchants[DamageEnergyType.Cold]);
            var acid_buff = Common.createBuffContextEnchantPrimaryHandWeaponIfHasMetamagic((Metamagic)MetamagicFeats.MetamagicExtender.ElementalAcid,
                                                                                          false, false,
                                                                                          new BlueprintWeaponType[0], WeaponEnchantments.elemental_enchants[DamageEnergyType.Acid]);
            var elec_buff = Common.createBuffContextEnchantPrimaryHandWeaponIfHasMetamagic((Metamagic)MetamagicFeats.MetamagicExtender.ElementalElectricity,
                                                                                          false, false,
                                                                                          new BlueprintWeaponType[0], WeaponEnchantments.elemental_enchants[DamageEnergyType.Electricity]);
            */

            var buff = Helpers.CreateBuff("ProduceFlameBuff",
                                            produce_flame_enchantments[0].Name,
                                            produce_flame_enchantments[0].Description,
                                            "",
                                            fireball.Icon,
                                            null,
                                            Helpers.Create<NewMechanics.EnchantmentMechanics.CreateWeapon>(c => { c.weapon = weapon; }),
                                            Common.createBuffContextEnchantPrimaryHandWeapon(Helpers.CreateContextValue(AbilityRankType.DamageBonus), false, false,
                                                                                            new BlueprintWeaponType[0], produce_flame_enchantments),
                                            empower_buff,
                                            maximize_buff,
                                            //cold_buff,
                                            //acid_buff,
                                            //elec_buff,
                                            Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CasterLevel, progression: ContextRankProgression.DivStep,
                                                                            type: AbilityRankType.DamageBonus, stepLevel: 1, max: 5)
                                            );
            buff.Stacking = Kingmaker.UnitLogic.Buffs.Blueprints.StackingType.Replace;
            weapon.AddComponent(Helpers.Create<NewMechanics.EnchantmentMechanics.WeaponSourceBuff>(w => w.buff = buff));

            var reduce_buff_duration = Helpers.Create<ContextActionReduceBuffDuration>(c => { c.TargetBuff = buff; c.DurationValue = Helpers.CreateContextDuration(1, DurationRate.Minutes); });
            foreach (var e in produce_flame_enchantments)
            {
                e.AddComponent(Helpers.Create<NewMechanics.EnchantmentMechanics.ActionOnAttackWithEnchantedWeapon>(a => { a.ActionsOnSelf = Helpers.CreateActionList(reduce_buff_duration); }));
            }

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), is_from_spell: true);
            produce_flame = Helpers.CreateAbility("ProduceFlameAbility",
                                                  buff.Name,
                                                  buff.Description,
                                                  "",
                                                  buff.Icon,
                                                  AbilityType.Spell,
                                                  Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                  AbilityRange.Medium,
                                                  Helpers.minutesPerLevelDuration,
                                                  "",
                                                  Helpers.CreateRunActions(Common.createContextActionAttack()),
                                                  Helpers.CreateSpellComponent(Kingmaker.Blueprints.Classes.Spells.SpellSchool.Evocation),
                                                  Helpers.Create<NewMechanics.AbilityCasterPrimaryHandFree>(),
                                                  Helpers.CreateSpellDescriptor(SpellDescriptor.Fire),
                                                  Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionOnContextCaster(apply_buff))),
                                                  Helpers.CreateContextRankConfig()
                                                  );
            produce_flame.setMiscAbilityParametersSingleTargetRangedHarmful();

            produce_flame.AvailableMetamagic = Kingmaker.UnitLogic.Abilities.Metamagic.Extend | Kingmaker.UnitLogic.Abilities.Metamagic.Heighten | Kingmaker.UnitLogic.Abilities.Metamagic.Empower | Kingmaker.UnitLogic.Abilities.Metamagic.Maximize | (Metamagic)MetamagicFeats.MetamagicExtender.Elemental | (Metamagic)MetamagicFeats.MetamagicExtender.Rime | (Metamagic)MetamagicFeats.MetamagicExtender.Dazing;

            produce_flame.AddToSpellList(Helpers.druidSpellList, 1);
            produce_flame.AddSpellAndScroll("5b172c2c3e356eb43ba5a8f8008a8a5a"); //fireball
            //replace 2nd level spell in fire domain
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("881b2137a1779294c8956fe5b497cc35"), 
                                      SpellDuplicates.addDuplicateSpell(produce_flame, "FireDomain"+produce_flame.name),  2);
        }


        static void createVirtuosoPerformance()
        {
            var performance_resource = library.Get<BlueprintAbilityResource>("e190ba276831b5c4fa28737e5e49e6a6");
            var inspire_competence = library.Get<BlueprintActivatableAbility>("430ab3bb57f2cfc46b7b3a68afd4f74e");
            var increase_group_size = Common.createIncreaseActivatableAbilityGroupSize(ActivatableAbilityGroup.BardicPerformance);
            var consume_additional_resource = Helpers.Create<NewMechanics.ConsumeResourceIfAbilitiesFromGroupActivated>(c =>
                                                                                                                        {
                                                                                                                            c.group = ActivatableAbilityGroup.BardicPerformance;
                                                                                                                            c.num_abilities_activated = 2;
                                                                                                                            c.resource = performance_resource;
                                                                                                                        }
                                                                                                                        );
            var deactivate_performance = Helpers.Create<NewMechanics.DeactivateAbilityFromGroup>(c =>
                                                                                                    {
                                                                                                        c.group = ActivatableAbilityGroup.BardicPerformance;
                                                                                                        c.num_abilities_activated = 1;
                                                                                                    }
                                                                                                  );
            virtuoso_performance = library.CopyAndAdd<BlueprintAbility>("20b548bf09bb3ea4bafea78dcb4f3db6", "VirtuosoPerformanceAbility", ""); //echolocation
            virtuoso_performance.SetIcon(inspire_competence.Icon);
            virtuoso_performance.SetName("Virtuoso Performance");
            virtuoso_performance.SetDescription("While this spell is active, you may start a second bardic performance while maintaining another. Starting the second performance costs 2 rounds of bardic performance instead of 1. Maintaining both performances costs a total of 3 rounds of bardic performance for each round they are maintained. When this spell ends, one of the performances ends immediately.");
            virtuoso_performance.RemoveComponents<SpellListComponent>();
            virtuoso_performance.RemoveComponents<AbilityEffectRunAction>();
            virtuoso_performance.LocalizedDuration = Helpers.roundsPerLevelDuration;

            var buff = Helpers.CreateBuff("VirtuosoPerformanceBuff",
                                          virtuoso_performance.Name,
                                          virtuoso_performance.Description,
                                          "",
                                          virtuoso_performance.Icon,
                                          null,
                                          increase_group_size,
                                          Helpers.CreateAddFactContextActions(newRound: consume_additional_resource, deactivated: deactivate_performance)
                                          );
            var apply_buff = Common.createContextActionApplyBuff(buff,
                                                                 Helpers.CreateContextDuration(bonus: Helpers.CreateContextValue(AbilityRankType.Default), rate: DurationRate.Rounds),
                                                                 is_from_spell: true);

            virtuoso_performance.AddComponent(Helpers.CreateRunActions(apply_buff));
            virtuoso_performance.AddToSpellList(Helpers.bardSpellList, 4);
            virtuoso_performance.AddSpellAndScroll("33770ff24b320e343bb767815f800fc4"); //echolocation
        }


        static internal void createDeadlyJuggernaut()
        {
            var sneak_attack = library.Get<BlueprintFeature>("df4f34f7cac73ab40986bc33f87b1a3c");
            var false_life = library.Get<BlueprintAbility>("7a5b5bf845779a941a67251539545762");
            deadly_juggernaut = library.CopyAndAdd<BlueprintAbility>("779179912e6c6fe458fa4cfb90d96e10", "DeadlyJuggernautAbility", "");
            deadly_juggernaut.RemoveComponents<SpellListComponent>();
            deadly_juggernaut.ReplaceComponent<AbilitySpawnFx>(false_life.GetComponent<AbilitySpawnFx>());
            deadly_juggernaut.ReplaceComponent<SpellComponent>(false_life.GetComponent<SpellComponent>());

            deadly_juggernaut.SetIcon(sneak_attack.Icon);
            deadly_juggernaut.SetName("Deadly Juggernaut");
            deadly_juggernaut.SetDescription("With every enemy life you take, you become increasingly dangerous and difficult to stop. During the duration of the spell, you gain a cumulative +1 luck bonus on melee attack rolls, melee weapon damage rolls, Strength checks, and Strength-based skill checks as well as DR 2/— each time you reduce a qualifying opponent to 0 or few hit points (maximum +5 bonus and DR 10/—) with a melee attack.");
            deadly_juggernaut.RemoveComponents<AbilityEffectRunAction>();


            BlueprintBuff[] buffs = new BlueprintBuff[5];

            for (int i = 0; i < buffs.Length; i++)
            {
                int bonus = i + 1;
                buffs[i] = Helpers.CreateBuff($"DeadlyJuggernaut{i + 1}Buff",
                                              $"Deadly Juggernaut (+{i + 1})",
                                              deadly_juggernaut.Description,
                                              "",
                                              deadly_juggernaut.Icon,
                                              null,
                                              Common.createAttackTypeAttackBonus(bonus, AttackTypeAttackBonus.WeaponRangeType.Melee, ModifierDescriptor.Luck),
                                              Helpers.Create<WeaponAttackTypeDamageBonus>(w => { w.AttackBonus = 1; w.Value = bonus; w.Descriptor = ModifierDescriptor.Luck; w.Type = AttackTypeAttackBonus.WeaponRangeType.Melee; }),
                                              Common.createAbilityScoreCheckBonus(bonus, ModifierDescriptor.Luck, StatType.Strength),
                                              Helpers.CreateAddStatBonus(StatType.SkillAthletics, bonus, ModifierDescriptor.Luck),
                                              Common.createPhysicalDR(bonus * 2)
                                              );
            }

            var conditional = Helpers.CreateConditional(Helpers.CreateConditionHasBuff(buffs[0]),
                                                        new GameAction[] {Common.createContextActionApplyBuff(buffs[1], Helpers.CreateContextDuration(), is_from_spell: true, is_child: true, is_permanent: true),
                                                                           Common.createContextActionRemoveBuff(buffs[0])
                                                                          },
                                                        new GameAction[] { Common.createContextActionApplyBuff(buffs[0], Helpers.CreateContextDuration(), is_from_spell: true, is_child: true, is_permanent: true) }
                                                       );
            for (int i = 1; i < buffs.Length; i++)
            {
                GameAction[] if_true = null;
                if (i < buffs.Length - 1)
                {
                    if_true = new GameAction[] {Common.createContextActionRemoveBuff(buffs[i]),
                                                Common.createContextActionApplyBuff(buffs[i+1],
                                                                                    Helpers.CreateContextDuration(),
                                                                                    is_from_spell: true,
                                                                                    is_child: true,
                                                                                    is_permanent:true)
                                               };
                }
                conditional = Helpers.CreateConditional(Helpers.CreateConditionHasBuff(buffs[i]),
                                                        if_true,
                                                        new GameAction[] { conditional });

            }




            var on_kill = Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(conditional),
                                                                           reduce_hp_to_zero: true,
                                                                           check_weapon_range_type: true,
                                                                           on_initiator: true,
                                                                           range_type: AttackTypeAttackBonus.WeaponRangeType.Melee);

            var buff = Helpers.CreateBuff("DeadlyJuggernautBuff",
                                          deadly_juggernaut.Name,
                                          deadly_juggernaut.Description,
                                          "",
                                          deadly_juggernaut.Icon,
                                          null,
                                          on_kill);
            buff.Stacking = StackingType.Replace;
            var apply_buff = Common.createContextActionApplyBuff(buff,
                                                     Helpers.CreateContextDuration(bonus: Helpers.CreateContextValue(AbilityRankType.Default), rate: DurationRate.Minutes),
                                                     is_from_spell: true);
            deadly_juggernaut.AddComponent(Helpers.CreateRunActions(apply_buff));


            deadly_juggernaut.AddToSpellList(Helpers.clericSpellList, 3);
            deadly_juggernaut.AddToSpellList(Helpers.inquisitorSpellList, 3);
            deadly_juggernaut.AddToSpellList(Helpers.paladinSpellList, 3);
            deadly_juggernaut.AddSpellAndScroll("539ff89add7d8e4409ab92df30e6afee"); //lead_blades
            deadly_juggernaut_buff = buff;
        }


        static internal void createInvisibilityPurge()
        {
            var invisibility = library.Get<BlueprintBuff>("525f980cb29bc2240b93e953974cb325");
            var invisibility_greater = library.Get<BlueprintBuff>("e6b35473a237a6045969253beb09777c");
            var divination_area = library.Get<BlueprintAbilityAreaEffect>("4ba26a4641c911d4487e3f7f11bcf801");
            var area_effect = Helpers.Create<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbilityAreaEffect>();
            area_effect.name = "InvisibilityPurgeArea";
            area_effect.AffectEnemies = true;
            area_effect.AggroEnemies = true;
            area_effect.Size = 30.Feet();
            area_effect.Shape = AreaEffectShape.Cylinder;

            var remove_invisibility = new GameAction[] { Common.createContextActionRemoveBuff(invisibility), Common.createContextActionRemoveBuff(invisibility_greater) };

            area_effect.AddComponent(Helpers.CreateAreaEffectRunAction(unitEnter: remove_invisibility, round: remove_invisibility));
            area_effect.Fx = divination_area.Fx;
            library.AddAsset(area_effect, "");

            var see_invisibility = library.Get<BlueprintAbility>("30e5dc243f937fc4b95d2f8f4e1b7ff3");
            var buff = Helpers.CreateBuff("InvisibilityPurgeBuff",
                                          "Invisibility Purge",
                                          "You surround yourself with a sphere of power with a radius of 30 feet that negates all forms of invisibility.\n" +
                                          "Anything invisible becomes visible while in the area.",
                                          "",
                                          see_invisibility.Icon,
                                          null,
                                          Common.createAddAreaEffect(area_effect)
                                          );

            var apply_buff = Helpers.CreateApplyBuff(buff,
                                                     Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes),
                                                     fromSpell: true, dispellable: true, asChild: true);
            invisibility_purge = Helpers.CreateAbility("InvisibilityPurgeAbility",
                                                       buff.Name,
                                                       buff.Description,
                                                       "",
                                                       buff.Icon,
                                                       AbilityType.Spell,
                                                       Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                       AbilityRange.Personal,
                                                       Helpers.minutesPerLevelDuration,
                                                       Helpers.savingThrowNone,
                                                       Helpers.CreateRunActions(apply_buff),
                                                       Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                       see_invisibility.GetComponent<AbilitySpawnFx>(),
                                                       see_invisibility.GetComponent<ContextRankConfig>()
                                                       );
            invisibility_purge.Animation = see_invisibility.Animation;
            invisibility_purge.AnimationStyle = see_invisibility.AnimationStyle;
            invisibility_purge.CanTargetSelf = true;
            invisibility_purge.CanTargetPoint = false;
            invisibility_purge.CanTargetEnemies = false;
            invisibility_purge.CanTargetFriends = false;
            invisibility_purge.AvailableMetamagic = see_invisibility.AvailableMetamagic;

            invisibility_purge.AddToSpellList(Helpers.inquisitorSpellList, 3);
            invisibility_purge.AddToSpellList(Helpers.clericSpellList, 3);

            invisibility_purge.AddSpellAndScroll("12f4ee72c02537244b5b2bacfa236bc7"); //see invisibility scroll
        }


        //clear cache to allow new spells to be picked
        [Harmony12.HarmonyPatch(typeof(SpellLevelList))]
        [Harmony12.HarmonyPatch("SpellsFiltered", Harmony12.MethodType.Getter)]
        class SpellLevelList_SpellsFiltered
        {
            static bool Prefix(SpellLevelList __instance)
            {
                Main.TraceLog();
                var tr = Harmony12.Traverse.Create(__instance);
                tr.Field("m_SpellsFiltered").SetValue(null).GetValue();
                return true;
            }
        }


        static public BlueprintAbility createWish(string name_prefix, string display_name, string description, UnityEngine.Sprite icon, BlueprintSpellbook primary_spellbook,
                                     UnitCommand.CommandType command_type,
                                     string[] blacklist_guids,
                                     AbilityType ability_type = AbilityType.Spell, 
                                     BlueprintComponent[] additional_components = null,
                                     bool allow_spells_with_material_components = true,
                                     bool full_round = true, int primary_spellbook_level = 8, int secondary_spellbook_level = 7)
        {
            if (secondary_spellbook_level > primary_spellbook_level)
            {
                throw Main.Error($"primary_spellbook_level < secondary_spellbook_level");
            }
            var spellbooks = library.GetAllBlueprints().OfType<BlueprintSpellbook>();
            //create spell list of all concerned spells and pick their lowest levels
            Dictionary<string, int> spell_guid_level_map = new Dictionary<string, int>();

            foreach (var spellbook in spellbooks)
            {
                if (spellbook == Summoner.summoner_class?.Spellbook)
                {
                    continue;
                }
                int max_level = spellbook == primary_spellbook ? primary_spellbook_level : secondary_spellbook_level;
                max_level = Math.Min(max_level, (int)spellbook.SpellList?.SpellsByLevel?.Length - 1);
                for (int i = 1; i <= max_level; i++)
                {
                    foreach (var spell in spellbook.SpellList.SpellsByLevel[i].Spells)
                    {
                        if (!spell_guid_level_map.ContainsKey(spell.AssetGuid))
                        {
                            spell_guid_level_map.Add(spell.AssetGuid, i);
                        }
                        else if (spell_guid_level_map[spell.AssetGuid] > i)
                        {
                            spell_guid_level_map[spell.AssetGuid] = i;
                        }
                    }
                }
            }


            List<BlueprintAbility>[] spells_per_level = new List<BlueprintAbility>[primary_spellbook_level];
            for (int i = 0; i < spells_per_level.Length; i++)
            {
                spells_per_level[i] = new List<BlueprintAbility>();
            }
            List<BlueprintAbility> wish_variants = new List<BlueprintAbility>();
            foreach (var entry in spell_guid_level_map)
            {
                var spell = library.Get<BlueprintAbility>(entry.Key);
                if (spell.MaterialComponent.Item == null || allow_spells_with_material_components)
                {
                    spells_per_level[entry.Value - 1].Add(library.Get<BlueprintAbility>(entry.Key));
                }
            }

            var all_spells = spells_per_level.Aggregate(new List<BlueprintAbility>(), (list, next) => { list.AddRange(next); return list; }).ToArray();

            var wish = Helpers.CreateAbility(name_prefix + "Ability",
                                            display_name,
                                            description,
                                            "",
                                            icon,
                                            ability_type,
                                            command_type,
                                            AbilityRange.Personal,
                                            "",
                                            "");
            Helpers.SetField(wish, "m_IsFullRoundAction", full_round);

            var spells_to_process = all_spells.Where(s => !NewSpells.getShadowSpells().Contains(s)
                                                          && s.GetComponent<SpellManipulationMechanics.WishSpell>() == null
                                                          && !blacklist_guids.Contains(s.AssetGuid)
                                                          && !(s.HasVariants && s.Variants.Any(v => blacklist_guids.Contains(v.AssetGuid)))
                                                          ).ToArray();
            var variant_spells = Common.CreateAbilityVariantsReplace(wish, name_prefix,
                                                                                (s, v) => {
                                                                                    s.Type = ability_type;
                                                                                    s.ActionType = command_type;
                                                                                    if (full_round)
                                                                                    {
                                                                                        Helpers.SetField(s, "m_IsFullRoundAction", true);
                                                                                    }
                                                                                    if (additional_components != null)
                                                                                    {
                                                                                        s.AddComponents(additional_components);
                                                                                    }
                                                                                    Main.logger.Log("Creating Wish: " + v.AssetGuid + " " + v.name);
                                                                                },
                                                                                ability_type == AbilityType.Spell || ability_type == AbilityType.SpellLike,
                                                                                spells_to_process
                                                                                );
            wish.AddComponent(Helpers.CreateAbilityVariants(wish, variant_spells));

            foreach (var s in variant_spells)
            {
                wish.AvailableMetamagic = wish.AvailableMetamagic | s.AvailableMetamagic;
            }
            
            return wish;
        }


        static public BlueprintAbility[] createWishSpellLevelVariants(string name_prefix, string display_name, string description,  UnityEngine.Sprite icon, BlueprintSpellbook primary_spellbook,
                                             UnitCommand.CommandType command_type, AbilityType ability_type = AbilityType.Spell, BlueprintComponent[] additional_components = null,
                                             bool allow_metamagic = true, bool allow_spells_with_material_components = true, 
                                             bool full_round =  true, int primary_spellbook_level = 8, int secondary_spellbook_level = 7, BlueprintAbilityResource resource = null)
        {
            if (secondary_spellbook_level > primary_spellbook_level)
            {
                throw Main.Error($"primary_spellbook_level < secondary_spellbook_level");
            }
            var spellbooks = library.GetAllBlueprints().OfType<BlueprintSpellbook>();
            //create spell list of all concerned spells and pick their lowest levels
            Dictionary<string, int> spell_guid_level_map = new Dictionary<string, int>();

            foreach (var spellbook in spellbooks)
            {
                if (spellbook == Summoner.summoner_class.Spellbook)
                {
                    continue;
                }
                int max_level = spellbook == primary_spellbook ? primary_spellbook_level : secondary_spellbook_level;
                max_level = Math.Min(max_level, (int)spellbook.SpellList?.SpellsByLevel?.Length - 1);
                for (int i = 1; i <= max_level; i++)
                {
                    foreach (var spell in spellbook.SpellList.SpellsByLevel[i].Spells)
                    {
                        if (!spell_guid_level_map.ContainsKey(spell.AssetGuid))
                        {
                            spell_guid_level_map.Add(spell.AssetGuid, i);
                        }
                        else if (spell_guid_level_map[spell.AssetGuid] > i)
                        {
                            spell_guid_level_map[spell.AssetGuid] = i;
                        }
                    }
                }
            }


            List<BlueprintAbility>[] spells_per_level = new List<BlueprintAbility>[primary_spellbook_level];
            for (int i = 0; i < spells_per_level.Length; i++)
            {
                spells_per_level[i] = new List<BlueprintAbility>();
            }
            List<BlueprintAbility> wish_variants = new List<BlueprintAbility>();
            foreach (var entry in spell_guid_level_map)
            {
                var spell = library.Get<BlueprintAbility>(entry.Key);
                if (spell.MaterialComponent.Item == null || allow_spells_with_material_components)
                {
                    spells_per_level[entry.Value - 1].Add(library.Get<BlueprintAbility>(entry.Key));
                }
            }

            for (int i = 0; i < spells_per_level.Length; i++)
            {
                if (spells_per_level[i].Empty())
                {
                    continue;
                }

                var wish_variant = Helpers.CreateAbility(name_prefix + (i + 1).ToString() + "Ability",
                                                         display_name + $" ({Common.roman_id[i + 1]})",
                                                         description,
                                                         "",
                                                         icon,
                                                         ability_type,
                                                         command_type,
                                                         AbilityRange.Personal,
                                                         "",
                                                         "");
                Helpers.SetField(wish_variant, "m_IsFullRoundAction", full_round);
                if (resource != null)
                {
                    wish_variant.AddComponent(Helpers.CreateResourceLogic(resource));
                }

                var variant_spells = Common.CreateAbilityVariantsReplace(wish_variant, name_prefix + (i + 1).ToString(),
                                                                                    (s, v) => {
                                                                                        s.Type = ability_type;
                                                                                        s.ActionType = command_type;
                                                                                        if (full_round)
                                                                                        {
                                                                                            Helpers.SetField(s, "m_IsFullRoundAction", true);
                                                                                        }
                                                                                        if (resource != null)
                                                                                        {
                                                                                            s.AddComponent(Helpers.CreateResourceLogic(resource));
                                                                                        }
                                                                                        if (additional_components != null)
                                                                                        {
                                                                                            s.AddComponents(additional_components);
                                                                                        }
                                                                                    },
                                                                                    ability_type == AbilityType.Spell || ability_type == AbilityType.SpellLike,
                                                                                  spells_per_level[i].Where(s => !NewSpells.getShadowSpells().Contains(s) && s.GetComponent<SpellManipulationMechanics.WishSpell>() == null).ToArray()
                                                                                  );
                wish_variant.AddComponent(Helpers.CreateAbilityVariants(wish_variant, variant_spells));
                wish_variants.Add(wish_variant);
                if (!allow_metamagic)
                {
                    continue;
                }

                foreach (var s in variant_spells)
                {
                    wish_variant.AvailableMetamagic = wish_variant.AvailableMetamagic | s.AvailableMetamagic;
                }
            }

            return wish_variants.ToArray();
        }



        static void createImmunityToWind()
        {
            ranged_attacks_forbidden_buff = Helpers.CreateBuff("RangedAttacksForbiddenBuff",
                                                    "Ranged Attacks Inefficient",
                                                    "Ranged Attacks are inefficient due to strong wind, or dense fog.",
                                                    "",
                                                    Helpers.GetIcon("c28de1f98a3f432448e52e5d47c73208"),
                                                    null,
                                                    Helpers.Create<NewMechanics.WeaponAttackAutoMiss>(w => w.attack_types = new AttackType[] { AttackType.Ranged }),
                                                    Helpers.Create<NewMechanics.OutgoingWeaponAttackAutoMiss>(w => w.attack_types = new AttackType[] { AttackType.Ranged })
                                                    );

            immunity_to_wind = Helpers.CreateFeature("ImmunityToWindEffectsFeature",
                                                     "",
                                                     "",
                                                     "",
                                                     null,
                                                     FeatureGroup.None,
                                                     Helpers.Create<WeatherMechanics.IgnoreWhetherMovementEffects>(),
                                                     Helpers.Create<SpecificBuffImmunity>(s => s.Buff = library.Get<BlueprintBuff>("bb57c37bfb5982d4bbed8d0fea75e404"))
                                                     );
            immunity_to_wind.HideInCharacterSheetAndLevelUp = true;


            var storm_burst = library.Get<BlueprintAbility>("f166325c271dd29449ba9f98d11542d9"); //from weather domain
            storm_burst.AddComponent(Common.createAbilityTargetHasFact(true, immunity_to_wind));

            var air_subtype = library.Get<BlueprintFeature>("dd3d0c7f4f57f304cbdbb68170b1b775");
            air_subtype.AddComponent(Helpers.CreateAddFact(immunity_to_wind));
        }


        


        static public BlueprintAbility[] getShadowSpells()
        {
            return new BlueprintAbility[]
            {
                shadow_enchantment,
                shadow_enchantment_greater,
                library.Get<BlueprintAbility>("237427308e48c3341b3d532b9d3a001f"), //shadow_evocation
                library.Get<BlueprintAbility>("3c4a2d4181482e84d9cd752ef8edc3b6"), //shadow evocation greater
                shadow_conjuration,
                shadow_conjuration_greater,
                shades
        };

        }
    }
}
