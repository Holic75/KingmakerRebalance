using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
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

namespace CallOfTheWild
{
    public class NewSpells
    {
        static LibraryScriptableObject library => Main.library;
        static public BlueprintAbility shillelagh;
        static public BlueprintAbility flame_blade;
        static public BlueprintAbility virtuoso_performance;
        static public BlueprintAbility deadly_juggernaut;
        static public BlueprintBuff deadly_juggernaut_buff;
        static public BlueprintAbility invisibility_purge;
        static public BlueprintAbility sanctuary;
        static public BlueprintBuff sanctuary_buff;

        static public BlueprintAbility command;
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
        static public BlueprintAbility burst_of_radiance;

        static public BlueprintAbility river_of_wind;
        static public BlueprintAbility winds_of_vengeance;

        static public BlueprintAbility blistering_invective;
        static public BlueprintAbility flashfire;
        static public BlueprintAbility stricken_heart;
        static public BlueprintAbility rigor_mortis;
        //static public BlueprintAbility touch_of_blood_letting; - ? to powerful due to ai and a bit complicated to implement
        static public BlueprintAbility ice_body;

        static public BlueprintAbility fire_seeds;

        static public BlueprintAbility suffocation, mass_suffocation;
        static public BlueprintBuff suffocation_buff;
        static public BlueprintAbility fluid_form;


        static public BlueprintAbility hold_person_mass;
        static public BlueprintAbility irresistible_dance;
        static public BlueprintAbility particulate_form;
        static public BlueprintAbility curse_major;
        static public BlueprintAbility hold_monster_mass;

        static public void load()
        {
            createShillelagh();
            createFlameBlade();
            createVirtuosoPerformance();
            createDeadlyJuggernaut();
            createInvisibilityPurge();
            createSanctuary();
            createCommand();
            createFireShield();
            createContingency();
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
        }


        static void createHoldMonsterMass()
        {
            var hold_monster = library.Get<BlueprintAbility>("41e8a952da7a5c247b3ec1c2dbb73018");

            var checker_fact = hold_monster.GetComponent<AbilityTargetHasNoFactUnless>();
            var does_not_work = hold_monster.GetComponent<AbilityTargetHasFact>();

            hold_monster_mass = library.CopyAndAdd<BlueprintAbility>("c7104f7526c4c524f91474614054547e", "HoldMonsterMassAbility", "");
            hold_monster_mass.RemoveComponents<SpellListComponent>();
            hold_monster_mass.RemoveComponents<AbilityTargetHasNoFactUnless>();
            hold_monster_mass.RemoveComponents<AbilityTargetHasFact>();
            hold_monster_mass.RemoveComponents<RecommendationNoFeatFromGroup>();
            hold_monster_mass.setMiscAbilityParametersRangedDirectional();
            hold_monster_mass.AddComponent(Helpers.CreateAbilityTargetsAround(30.Feet(), TargetType.Enemy));

            hold_monster_mass.SetName("Hold Monster, Mass");
            hold_monster_mass.SetDescription("This spell functions like hold monster, except as noted above.\n" + hold_monster.Description);

            var action = Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Common.createContextConditionHasFacts(false, checker_fact.CheckedFacts), Common.createContextConditionCasterHasFact(checker_fact.UnlessFact, has: false)),
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

            var effect_saved = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1));
            var effect = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1, diceType: DiceType.D4, diceCount: 1));

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
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting),
                                                Helpers.CreateSpellComponent(SpellSchool.Enchantment),
                                                Helpers.CreateDeliverTouch(),
                                                Helpers.CreateRunActions(SavingThrowType.Will, Helpers.CreateConditionalSaved(effect_saved, effect))
                                                );
            ability.setMiscAbilityParametersTouchHarmful();
            ability.SpellResistance = true;
            ability.AvailableMetamagic = Metamagic.Extend | Metamagic.Empower | Metamagic.Heighten | Metamagic.Reach | Metamagic.Quicken | Metamagic.Maximize;
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
                                          "The targets’ physical forms undergo a bizarre transformation. They look and function normally, but are composed of countless particles that separate and reconnect to remain whole. Each target gains fast healing 1 and is immune to bleed damage, critical hits, sneak attacks, and other forms of precision damage. The value of this fast healing increases by 1 at caster levels 10th, 15th, and 20th. Any target can end the spell effect on itself as a swift action; the target then regains 5d6 hit points.",
                                          "",
                                          icon,
                                          null,
                                          Common.createAddContextEffectFastHealing(Helpers.CreateContextValue(AbilityRankType.StatBonus)),
                                          Helpers.Create<AddImmunityToPrecisionDamage>(),
                                          Common.createBuffDescriptorImmunity(SpellDescriptor.Bleed),
                                          Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Bleed),
                                          Helpers.CreateContextRankConfig(type: AbilityRankType.StatBonus, progression: ContextRankProgression.StartPlusDivStep, startLevel: 5, stepLevel: 5)
                                          );

            var particualte_form_swift = Helpers.CreateAbility("ParticulateFormSwiftAction",
                                                               "Particualte Form: Dismiss",
                                                               buff.Description,
                                                               "",
                                                               icon,
                                                               AbilityType.Special,
                                                               UnitCommand.CommandType.Swift,
                                                               AbilityRange.Personal,
                                                               "",
                                                               "",
                                                               Helpers.CreateRunActions(Common.createContextActionHealTarget(Helpers.CreateContextDiceValue(DiceType.D6, 5)),
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
                                                      Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)))),
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
            hold_person_mass.AddComponents(Helpers.CreateAbilityTargetsAround(30.Feet(), TargetType.Enemy));

            hold_person_mass.SetName("Hold Person, Mass");
            hold_person_mass.SetDescription("This spell functions like hold person, except as noted above.\n" + hold_person.Description);

            var action = Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Common.createContextConditionHasFacts(false, checker_fact[0].CheckedFacts), Common.createContextConditionCasterHasFact(checker_fact[0].UnlessFact, has: false)),
                                                    null,
                                                    Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Common.createContextConditionHasFacts(false, checker_fact[1].CheckedFacts), Common.createContextConditionCasterHasFact(checker_fact[1].UnlessFact, has: false)),
                                                                                null,
                                                                                Helpers.CreateConditional(Common.createContextConditionHasFacts(false, does_not_work.CheckedFacts),
                                                                                                        null,
                                                                                                        hold_person.GetComponent<AbilityEffectRunAction>().Actions.Actions[0]
                                                                                                        )
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

            curse_major.AddSpellAndScroll("b2bd1f6b0505672408eb0ee5714e4640");
        }


        static void createFluidForm()
        {
            var icon = library.Get<BlueprintAbility>("40681ea748d98f54ba7f5dc704507f39").Icon;//charged water blast

            var buff = Helpers.CreateBuff("FluidFormBuff",
                                            "Fluid Form",
                                            "When you cast this spell, your body takes on a slick, oily appearance.For the duration of this spell, your form can stretch and shift with ease and becomes slightly transparent, as if you were composed of liquid. This transparency is not enough to grant concealment.You gain DR 10 / slashing and your reach increases by 10 feet.",
                                            "",
                                            icon,
                                            Common.createPrefabLink("9e2750fa744d28d4c95b9c72cc94868d"),
                                            Common.createContextFormDR(10, PhysicalDamageForm.Slashing),
                                            Helpers.CreateAddStatBonus(StatType.Reach, 10, ModifierDescriptor.UntypedStackable)
                                            );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes));

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

            var effect = Helpers.CreateConditionalSaved(Common.createContextActionApplyBuff(staggered, Helpers.CreateContextDuration(1, DurationRate.Rounds)),
                                                        Common.createContextActionApplyBuff(suffocation_buff, Helpers.CreateContextDuration(3, DurationRate.Rounds)));

            var effect_mass = Helpers.CreateConditionalSaved(Common.createContextActionApplyBuff(staggered, Helpers.CreateContextDuration(1, DurationRate.Rounds)),
                                            Common.createContextActionApplyBuff(suffocation_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Rounds)));

            var undead = library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");
            var construct = library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f");
            var elemental = library.Get<BlueprintFeature>("198fd8924dabcb5478d0f78bd453c586");

            suffocation = Helpers.CreateAbility("SuffocationAbility",
                                                "Suffocation",
                                                "This spell extracts the air from the target’s lungs, causing swift suffocation.\n"
                                                + "The target can attempt to resist this spell’s effects with a Fortitude save -if he succeeds, he is merely staggered for 1 round as he gasps for breath.If the target fails, he immediately begins to suffocate.On the target’s next turn, he falls unconscious and is reduced to 0 hit points. One round later, the target drops to - 1 hit points and is dying. One round after that, the target dies. Each round, the target can delay that round’s effects from occurring by making a successful Fortitude save, but the spell continues for 3 rounds, and each time a target fails his Fortitude save, he moves one step further along the track to suffocation.This spell only affects living creatures that must breathe.It is impossible to defeat the effects of this spell by simply holding one’s breath -if the victim fails the initial Saving Throw, the air in his lungs is extracted.",
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

            suffocation.AvailableMetamagic = Metamagic.Extend | Metamagic.Quicken | Metamagic.Reach | Metamagic.Heighten;
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

            mass_suffocation.AvailableMetamagic = Metamagic.Extend | Metamagic.Quicken | Metamagic.Reach | Metamagic.Heighten;

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
            var description = "Acorns turn into special thrown splash weapons. An acorn grenade has a range of 25 feet. A ranged touch attack roll is required to strike the intended target. Together, the acorns are capable of dealing 1d4 points of fire damage per caster level (maximum 20d4) divided among the acorns as you wish. No acorn can deal more than 10d4 points of damage.\n"
                              + "Each acorn grenade explodes upon striking any hard surface. In addition to its regular fire damage, all creatures adjacent to the explosion take 1 point of fire damage per die of the explosion.This explosion of fire ignites any combustible materials adjacent to the target.";

            for (int i = 1; i <= 10; i++)
            {
                var primary_damage = Helpers.CreateActionDealDamage(DamageEnergyType.Fire, Helpers.CreateContextDiceValue(DiceType.D4, i));
                var secondary_damage = Helpers.CreateActionDealDamage(DamageEnergyType.Fire, Helpers.CreateContextDiceValue(DiceType.Zero, 0, i));

                var conditional = Helpers.CreateConditional(Helpers.Create<ContextConditionIsMainTarget>(), new GameAction[] { primary_damage, secondary_damage }, new GameAction[] { secondary_damage });

                var ability = Helpers.CreateAbility($"FireSeeds{i}Ability",
                                                    $"Fire Seeds ({i}d4)",
                                                    description,
                                                    "",
                                                    icon,
                                                    AbilityType.Special,
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
                ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize;
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
            foreach (var a in abilities)
            {
                buff.AddComponent(Helpers.Create<ReplaceAbilityParamsWithContext>(r => r.Ability = a));
            }

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.TenMinutes));

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

            fire_seeds.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach | Metamagic.Extend;
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
                                                             "This weapon 1 an additional point of cold damage.",
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
                                          Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Electricity | SpellDescriptor.Cold | SpellDescriptor.Blindness | SpellDescriptor.Disease | SpellDescriptor.Stun | SpellDescriptor.Poison),
                                          Common.createBuffDescriptorImmunity(SpellDescriptor.Electricity | SpellDescriptor.Cold | SpellDescriptor.Blindness | SpellDescriptor.Disease | SpellDescriptor.Stun | SpellDescriptor.Poison),
                                          Common.createMagicDR(5),
                                          Helpers.Create<AddImmunityToAbilityScoreDamage>(),
                                          Helpers.Create<AddImmunityToCriticalHits>(),
                                          Common.createEmptyHandWeaponOverride(weapon),
                                          Helpers.CreateSpellDescriptor(SpellDescriptor.Cold),
                                          Common.createSpecificBuffImmunity(suffocation_buff)
                                          );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes));

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

            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Unholy, Helpers.CreateContextDiceValue(Kingmaker.RuleSystem.DiceType.D6, Helpers.CreateContextValue(AbilityRankType.DamageBonus)),
                                                     halfIfSaved: true);
            dmg.DamageType.Type = Kingmaker.RuleSystem.Rules.Damage.DamageType.Direct;

            var buff = Helpers.CreateBuff("RigorMortisBuff",
                                          "Rigor Mortis",
                                          "The joints of a creature affected by this spell stiffen and swell, making movement painful and slow. The target takes 1d6 points of damage per caster level. Additionally, the target takes a –4 penalty to Dexterity and its movement speed decreases by 10 feet; these additional effects last for 1 minute per caster level. A successful save halves the damage and negates the penalty to Dexterity and movement.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.CreateAddStatBonus(StatType.Dexterity, -4, ModifierDescriptor.UntypedStackable),
                                          Helpers.CreateAddStatBonus(StatType.Speed, -10, ModifierDescriptor.UntypedStackable)
                                          );
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes));
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
            rigor_mortis.AvailableMetamagic = Metamagic.Empower | Metamagic.Extend | Metamagic.Heighten | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Reach;

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
            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Fire, Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.DamageDice)), isAoE: true);
            var saved_dmg = Common.createContextActionSavingThrow(SavingThrowType.Reflex, Helpers.CreateActionList(Helpers.CreateConditionalSaved(new GameAction[0], 
                                                                                                                                                  new GameAction[] { dmg, apply_burn })));

            var area_run_action = Helpers.CreateAreaEffectRunAction(saved_dmg, null, null, saved_dmg);

            area.ComponentsArray = new BlueprintComponent[] { area_run_action,
                                                              Helpers.CreateContextRankConfig(progression: ContextRankProgression.StartPlusDivStep, 
                                                                                              startLevel: 3, stepLevel: 3, max: 5, type: AbilityRankType.DamageDice),
                                                              Helpers.CreateSpellDescriptor(SpellDescriptor.Fire)};

            area.SpellResistance = true;
            flashfire = Helpers.CreateAbility("FlashfireAbility",
                                                "Flashfire",
                                                "You cause flames to spring up in a 30-ft line, hese flames deal 1d6 points of fire damage for every 3 caster levels you have (maximum 5d6) to each creature that enters a burning area or begins its turn in the area; these creatures also catch on fire. A creature that succeeds at a Reflex save negates the damage and avoids catching on fire",
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
            flashfire.AvailableMetamagic = Metamagic.Empower | Metamagic.Extend | Metamagic.Heighten | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Reach;
            flashfire.AddToSpellList(Helpers.druidSpellList, 3);
            flashfire.AddSpellAndScroll("d8f2bcc130113194998810b7ae3e07f5"); //blessing of salamander
        }

        static void createBlisteringInvective()
        {
            var icon = library.Get<BlueprintAbility>("cf6c901fb7acc904e85c63b342e9c949").Icon; //confusion

            var burn1d6 = library.Get<BlueprintBuff>("e92ecaa76b5db674fa5b0aaff5b21bc9");
            var dazzling_display = library.Get<BlueprintAbility>("5f3126d4120b2b244a95cb2ec23d69fb");

            var demoralize = (dazzling_display.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as Demoralize);


            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Fire, Helpers.CreateContextDiceValue(DiceType.D10, 1, 0));
            var apply_burn = Common.createContextActionApplyBuff(burn1d6, Helpers.CreateContextDuration(), is_permanent: true);

            var effect = Common.createContextActionSavingThrow(SavingThrowType.Reflex, Helpers.CreateActionList(dmg, Helpers.CreateConditionalSaved(null, apply_burn)));

            var effect_on_demoralzie = Helpers.Create<NewMechanics.ActionOnDemoralize>(a =>
                                                                                        {
                                                                                            a.Buff = demoralize.Buff; a.GreaterBuff = demoralize.GreaterBuff;
                                                                                            a.actions = Helpers.CreateActionList(effect);
                                                                                        }
                                                                                      );

            blistering_invective = Helpers.CreateAbility("BlisteringInvectiveAbility",
                                                         "Blistering Invective",
                                                         "You unleash an insulting tirade so vicious and spiteful that enemies who hear it are physically scorched by your fury. When you cast this spell, make an Intimidate check to demoralize each enemy within 30 feet of you. Enemies that are demoralized this way take 1d10 points of fire damage and must succeed at a Reflex save or catch fire.",
                                                         "",
                                                         icon,
                                                         AbilityType.Spell,
                                                         UnitCommand.CommandType.Standard,
                                                         AbilityRange.Personal,
                                                         "",
                                                         "Reflex partial",
                                                         Helpers.CreateRunActions(effect_on_demoralzie),
                                                         dazzling_display.GetComponent<AbilityTargetsAround>(),
                                                         dazzling_display.GetComponent<AbilitySpawnFx>(),
                                                         Helpers.Create<SharedSpells.CannotBeShared>(),
                                                         Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                         Helpers.CreateSpellDescriptor(SpellDescriptor.Fire)
                                                         );

            blistering_invective.setMiscAbilityParametersSelfOnly();
            blistering_invective.AvailableMetamagic = Metamagic.Empower | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Maximize;

            blistering_invective.AddToSpellList(Helpers.alchemistSpellList, 2);
            blistering_invective.AddToSpellList(Helpers.inquisitorSpellList, 2);
            blistering_invective.AddToSpellList(Helpers.bardSpellList, 2);

            blistering_invective.AddSpellAndScroll("1db86aaa479be6944abe90eaddb4afa2");
        }


        static void createStrickenHeart()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/StrickenHeart.png");
            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.NegativeEnergy, Helpers.CreateContextDiceValue(DiceType.D6, 2, 0));

            var frigid_touch = library.Get<BlueprintAbility>("c83447189aabc72489164dfc246f3a36");
            var ability = Helpers.CreateAbility("StrickenHeartAbility",
                                               "Stricken Heart",
                                               "This spell covers your hand with a writhing black aura. As part of casting the spell, you can make a melee touch attack that deals 2d6 points of negative energy damage and causes the target to be staggered for 1 round. If the attack is a critical hit, the target is staggered for 1 minute instead. Creatures immune to precision damage are immune to the staggered effect.",
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
            ability.AvailableMetamagic = Metamagic.Maximize | Metamagic.Empower | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach;
            stricken_heart = Helpers.CreateTouchSpellCast(ability);

            stricken_heart.AddToSpellList(Helpers.inquisitorSpellList, 2);
            stricken_heart.AddToSpellList(Helpers.wizardSpellList, 2);
            stricken_heart.AddSpellAndScroll("2fff640003e17ca459f65e787d2d65de");//unbreakable heart
        }


        static void createRiverOfWind()
        {
            var air_subtype = library.Get<BlueprintFeature>("dd3d0c7f4f57f304cbdbb68170b1b775");

            var dmg = Helpers.CreateActionDealDamage(PhysicalDamageForm.Bludgeoning, Helpers.CreateContextDiceValue(DiceType.D6, 4), true, true);
            var dmg2 = Helpers.CreateActionDealDamage(PhysicalDamageForm.Bludgeoning, Helpers.CreateContextDiceValue(DiceType.D6, 2), true, true);
            var saved = Helpers.CreateConditionalSaved(null, Helpers.Create<ContextActionKnockdownTarget>());
            var effect = Helpers.CreateConditional(Helpers.CreateConditionHasFact(air_subtype),
                                                   null,
                                                   Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(dmg, saved))
                                                   );
            var effect2 = Helpers.CreateConditional(Helpers.CreateConditionHasFact(air_subtype),
                                                   null,
                                                   Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(dmg2, saved))
                                                   );
            var icon = library.Get<BlueprintFeature>("2aad85320d0751340a0786de073ee3d5").Icon;//torrent infusion

            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("2a90aa7f771677b4e9624fa77697fdc6", "RiverOfWindArea", "");
            area.ComponentsArray = new BlueprintComponent[] {Helpers.Create<NewMechanics.AbilityAreaEffectRunActionWithFirstRound>(a => { a.FirstRound = Helpers.CreateActionList(effect, dmg2);
                                                                                                                                          a.Round = Helpers.CreateActionList(effect2); }) };
            area.SpellResistance = true;
            river_of_wind = Helpers.CreateAbility("RiverOfWindAbility",
                                                  "River of Wind",
                                                  "Summoning up the power of the tempest, you direct a current of forceful winds where you please. This spell creates a 5-foot-diameter 60-foot length line of wind where you designate, it remains constant in that direction for the spell duration. Creatures caught in a river of wind take 4d6 bludgeoning damage and are knocked prone. A successful Fortitude save halves the damage and prevents being knocked prone.\n"
                                                  + "On every next round a creature that is wholly or partially within a river of wind must make a Fortitude save or take 2d6 bludgeoning damage, and be knocked prone - a successful Fortitude save means the creature merely takes 1d6 bludgeoning damage. Creatures under the effect of freedom of movement and creatures with the air subtype are unaffected by a river of wind.",
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
                                                  Helpers.CreateSpellDescriptor(SpellDescriptor.MovementImpairing)
                                                  );
            river_of_wind.setMiscAbilityParametersRangedDirectional();
            river_of_wind.SpellResistance = true;
            river_of_wind.AvailableMetamagic = Metamagic.Empower | Metamagic.Extend | Metamagic.Heighten | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Reach;
            river_of_wind.AddToSpellList(Helpers.wizardSpellList, 4);
            river_of_wind.AddToSpellList(Helpers.druidSpellList, 4);
            river_of_wind.AddToSpellList(Helpers.magusSpellList, 4);


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

            var dmg = Helpers.CreateActionDealDamage(PhysicalDamageForm.Bludgeoning, Helpers.CreateContextDiceValue(DiceType.D8, 5), false, true);
            var saved = Helpers.CreateConditionalSaved(null, Helpers.Create<ContextActionKnockdownTarget>());
            var cooldown_buff = Helpers.CreateBuff("WindsOfVengeanceCooldownBuff",
                                                   "Winds of Vengeance: Cooldown",
                                                   "",
                                                   "",
                                                   icon,
                                                   null);
            var apply_cooldown = Common.createContextActionApplyBuff(cooldown_buff, Helpers.CreateContextDuration(1), is_child: true, dispellable: false);
            var apply_cooldown_on_main_target = Helpers.Create<NewMechanics.ContextActionOnMainTarget>(c => c.Actions = Helpers.CreateActionList(apply_cooldown));
            var action = Helpers.CreateConditional(Helpers.Create<NewMechanics.ContextConditionMainTargetHasFact>(c => c.Fact = cooldown_buff),
                                                   null,
                                                   new GameAction[] { Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(dmg, saved)),
                                                                                                                                       apply_cooldown_on_main_target });
            var on_hit = Common.createAddTargetAttackWithWeaponTrigger(Helpers.CreateActionList(),
                                                                       Helpers.CreateActionList(action)
                                                                       );

            var buff = Helpers.CreateBuff("WindsOfVengeanceBuff",
                                          "Winds of Vengeance",
                                          "You surround yourself with a buffeting shroud of supernatural, tornado-force winds. These winds grant you to fly granting immunity to ground-based effects and a 30-ft bonus to speed.  The winds shield you from any other wind effects, and form a shell of breathable air around you, allowing you to fly and breathe underwater or in outer space.\n"
                                          + "Ranged weapons(including giant - thrown boulders, siege weapon projectiles, and other massive ranged weapons) passing through the winds are deflected by the winds and automatically miss you.Gases and most gaseous breath weapons cannot pass though the winds.\n"
                                          + "In addition, once per round, when a creature hits you with a melee attack, winds lash out at that creature. The creature must make a Fortitude Saving Throw or take 5d8 points of bludgeoning damage and be knocked prone.\n"
                                          + "On a successful save, the damage is halved and the creature is not knocked prone.",
                                          "",
                                          icon,
                                          Common.createPrefabLink("40c31beb53bffb845b095542133ac9bc"),//"ea8ddc3e798aa25458e2c8a15e484c68"),
                                          Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.BreathWeapon | SpellDescriptor.Poison | SpellDescriptor.Ground),
                                          Common.createBuffDescriptorImmunity(SpellDescriptor.Ground),
                                          Common.createAddConditionImmunity(Kingmaker.UnitLogic.UnitCondition.DifficultTerrain),
                                          Helpers.CreateAddStatBonus(StatType.Speed, 30, ModifierDescriptor.UntypedStackable),
                                          Helpers.Create<NewMechanics.WeaponAttackAutoMiss>(w => w.attack_types = new AttackType[] { AttackType.Ranged, AttackType.RangedTouch }),
                                          on_hit
                                          );
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes));
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
                                                       Helpers.CreateRunActions(apply_buff),
                                                       Helpers.CreateContextRankConfig(),
                                                       Helpers.CreateSpellComponent(SpellSchool.Evocation)
                                                       );
            winds_of_vengeance.setMiscAbilityParametersSelfOnly();
            winds_of_vengeance.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken;

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
            burst_of_radiance.SetDescription("This spell fills the area with a brilliant flash of shimmering light. Creatures in the area are blinded for 1d4 rounds, or dazzled for 1d4 rounds if they succeed at a Reflex save. Evil creatures in the area of the burst take 1d4 points of damage per caster level (max 5d4), whether they succeed at the Reflex save or not.");
            burst_of_radiance.LocalizedSavingThrow = Helpers.CreateString("BurstOfRadianceAbility.SavingThrow", "Reflex partial");
            burst_of_radiance.Range = AbilityRange.Long;
            burst_of_radiance.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten | Metamagic.Maximize | Metamagic.Empower;
            burst_of_radiance.ReplaceComponent<SpellDescriptorComponent>(Helpers.CreateSpellDescriptor(SpellDescriptor.Good | SpellDescriptor.Blindness));

            var dazzled = library.Get<BlueprintBuff>("df6d1025da07524429afbae248845ecc");
            var blind = library.Get<BlueprintBuff>("187f88d96a0ef464280706b63635f2af");

            var duration = Helpers.CreateContextDuration(0, diceType: DiceType.D4, diceCount: 1);
            var effect = Helpers.CreateConditionalSaved(Common.createContextActionApplyBuff(dazzled, duration),
                                                        Common.createContextActionApplyBuff(blind, duration));

            var damage = Helpers.CreateActionDealDamage(DamageEnergyType.Divine, Helpers.CreateContextDiceValue(DiceType.D4, Helpers.CreateContextValue(AbilityRankType.Default)), isAoE: true);

            var apply_damage = Helpers.CreateConditional(Helpers.CreateContextConditionAlignment(AlignmentComponent.Evil),
                                                         damage);

            burst_of_radiance.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(SavingThrowType.Reflex, effect, apply_damage));
            burst_of_radiance.AddComponent(Helpers.CreateContextRankConfig(max: 5));

            burst_of_radiance.AddToSpellList(Helpers.clericSpellList, 2);
            burst_of_radiance.AddToSpellList(Helpers.druidSpellList, 2);
            burst_of_radiance.AddToSpellList(Helpers.wizardSpellList, 2);
            burst_of_radiance.AddSpellAndScroll("2af933b93ac31ba449001e7e3b911b5b");
        }



        static void createObscuringMist()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/FogCloud.png");

            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("fe5102d734382b74586f56980086e5e8", "ObscuringMistFogArea", ""); //mind fog
            area.Fx = Common.createPrefabLink("597682efc0419a142a3174fd6bb408f7"); //mind fog
            area.Size = 20.Feet();
            area.SpellResistance = false;

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


            obscuring_mist = library.CopyAndAdd<BlueprintAbility>("68a9e6d7256f1354289a39003a46d826", "ObscuringMistAbility", "");
            obscuring_mist.SpellResistance = false;
            obscuring_mist.RemoveComponents<SpellListComponent>();
            obscuring_mist.RemoveComponents<SpellDescriptorComponent>();
            obscuring_mist.ReplaceComponent<AbilityAoERadius>(a => Helpers.SetField(a, "m_Radius", 20.Feet()));
            obscuring_mist.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(Common.changeAction<ContextActionSpawnAreaEffect>(a.Actions.Actions, s => s.AreaEffect = area)));
            obscuring_mist.SetNameDescriptionIcon(buff.Name, buff.Description, buff.Icon);
            obscuring_mist.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach | Metamagic.Extend;
            obscuring_mist.AddToSpellList(Helpers.druidSpellList, 1);
            obscuring_mist.AddToSpellList(Helpers.clericSpellList, 1);
            obscuring_mist.AddToSpellList(Helpers.magusSpellList, 1);
            obscuring_mist.AddToSpellList(Helpers.wizardSpellList, 1);
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

            var apply_fatigued = Common.createContextActionApplyBuff(fatigued, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes));
            var apply_exhausted = Common.createContextActionApplyBuff(exhausted, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes));

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
                                                     + "The subject is immediately exhausted for the spell’s duration.A successful Fortitude save means the creature is only fatigued.\n"
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
            ray_of_exhaustion.AvailableMetamagic = Metamagic.Extend | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;

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
                                               Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)))),
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
                                                Helpers.CreateActionDealDamage(DamageEnergyType.Fire, Helpers.CreateContextDiceValue(DiceType.D6, 4), halfIfSaved: true),
                                                Helpers.CreateActionDealDamage(DamageEnergyType.Fire, Helpers.CreateContextDiceValue(DiceType.D6, 2), halfIfSaved: true)
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
            burning_entanglement.AvailableMetamagic = burning_entanglement.AvailableMetamagic | Metamagic.Empower | Metamagic.Maximize;

            burning_entanglement.SetNameDescriptionIcon("Burning Entanglement",
                                                        "This spell functions as per entangle, except it sets the foliage on fire. A creature that begins its turn entangled by the spell takes 4d6 points of fire damage (Reflex half), and a creature that begins its turn in the area but is not entangled takes 2d6 points of fire damage (Reflex negates). Smoke rising from the vines partially obscures visibility. Creatures can see things in the smoke within 5 feet clearly, but attacks against anything farther away in the smoke must contend with concealment (20% miss chance). When the spell’s duration expires, the vines burn away entirely.",
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


        static void createArchonsTrumpet()
        {
            var paralyzed = library.Get<BlueprintBuff>("af1e2d232ebbb334aaf25e2a46a92591");
            var icon = library.Get<BlueprintAbility>("f09453607e683784c8fca646eec49162").Icon;

            var apply_paralyzed = Common.createContextActionApplyBuff(paralyzed, Helpers.CreateContextDuration(0, DurationRate.Rounds, DiceType.D4, 1));
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
            archons_trumpet.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
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

            var shaken = library.Get<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220");

            var apply_shaken = Common.createContextActionApplyBuff(shaken, Helpers.CreateContextDuration(1, DurationRate.Hours), is_child: true);
            var on_save_failed = Helpers.CreateConditionalSaved(null, apply_shaken);
            var effect = Common.createContextActionSavingThrow(SavingThrowType.Will, Helpers.CreateActionList(on_save_failed));

            var conditional_effect = Helpers.CreateConditional(Helpers.Create<ContextConditionIsEnemy>(), effect);
            area.ReplaceComponent<AbilityAreaEffectRunAction>(a => a.UnitEnter = Helpers.CreateActionList(conditional_effect));
            area.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Emotion | SpellDescriptor.Fear | SpellDescriptor.Shaken));

            var buff = Helpers.CreateBuff("AuraOfDoomBuff",
                                          "Aura of Doom",
                                          "You emanate an almost palpable aura of horror (with 20 ft. radius). All enemies within this spell’s area, or that later enter the area, must make a Will save to avoid becoming shaken. A successful save suppresses the effect. Creatures that leave the area and come back must save again to avoid being affected by the effect.",
                                          "",
                                          LoadIcons.Image2Sprite.Create(@"AbilityIcons/AuraOfDoom.png"),
                                          null,
                                          Common.createAddAreaEffect(area)
                                          );
            var apply_aura = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.TenMinutes));
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
            aura_of_doom.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken;

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
                                               Helpers.Create<SharedSpells.CannotBeShared>()
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
            var normal_dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Unholy, Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.DamageDice)),
                                                     isAoE: true, halfIfSaved: true);
            var plant_dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Unholy, Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.DamageDice), Helpers.CreateContextValue(AbilityRankType.DamageDice)));

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
                                                     "You call forth a burst of decay that ravages all creatures in the area. Even nonliving creatures such as constructs and undead crumble or wither in this malignant eruption of rotting energy. Creatures in the area of effect take 1d6 points of damage per caster level (maximum 15d6) and are staggered for 1d4 rounds. A target that succeeds at a Reflex saving throw takes half damage and negates the staggered effect. Plant creatures are particularly susceptible to this rotting effect; a plant creature caught in the burst takes a –2 penalty on the saving throw and takes 1 extra point of damage per die.",
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
                                                     Helpers.CreateAbilityTargetsAround(10.Feet(), TargetType.Any)
                                                     );
            explosion_of_rot.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Heighten | Metamagic.Reach | Metamagic.Quicken;
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

            var dmg = Helpers.CreateActionDealDamage(PhysicalDamageForm.Bludgeoning, Helpers.CreateContextDiceValue(DiceType.D4, Helpers.CreateContextValue(AbilityRankType.Default)),
                                                     isAoE: true, halfIfSaved: true);

            var apply_prone = Helpers.Create<ContextActionKnockdownTarget>();
            var failure_prone_action = Helpers.CreateConditional(Helpers.Create<CombatManeuverMechanics.ContextConditionTargetSizeLessOrEqual>(c => c.target_size = Size.Medium),
                                                           Helpers.CreateConditionalSaved(null, apply_prone)
                                                          );

            var description = "You strike the ground and unleash a tremor of seismic force, hurling up earth, rock, and sand.\n"
                              + "You choose whether the earth tremor affects a 30 - foot line, a 20 - foot cone - shaped spread, or a 10 - foot - radius spread centered on you. the space you occupy is not affected by earth tremor.the area you choose becomes dense rubble that costs 2 squares of movement to enter.Dense rubble and is considered as difficult terrain. Creatures on the ground in the area take 1d4 points of bludgeoning damage per caster level you have (maximum 10d4) or half damage on a successful save. Medium or smaller creatures that fail their saves are knocked prone.\n"
                              + "This spell can be cast only on a surface of earth, sand, or stone. It has no effect if you are in a wooden or metal structure or if you are not touching the ground.";

            var earth_tremor_cone = Helpers.CreateAbility("EarthTremorCone",
                                                 "Earth Tremor: 20-foot Cone",
                                                 description,
                                                 "",
                                                 icon,
                                                 AbilityType.Spell,
                                                 Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                 AbilityRange.Projectile,
                                                 "",
                                                 Helpers.savingThrowNone,
                                                 Helpers.CreateRunActions(SavingThrowType.Reflex, dmg, failure_prone_action,
                                                                          Common.createContextActionSpawnAreaEffect(area, Helpers.CreateContextDuration(1, DurationRate.Hours))
                                                                          ),
                                                 Helpers.CreateContextRankConfig(max: 10),
                                                 Helpers.CreateSpellDescriptor(SpellDescriptor.Ground),
                                                 Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                                                 Common.createAbilityDeliverProjectile(AbilityProjectileType.Cone,
                                                                                      library.Get<BlueprintProjectile>("868f9126707bdc5428528dd492524d52"), 20.Feet(), 5.Feet()) //sonic cone
                                                 );

            earth_tremor_cone.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Heighten | Metamagic.Quicken;
            earth_tremor_cone.setMiscAbilityParametersRangedDirectional();

            var earth_tremor_line = library.CopyAndAdd<BlueprintAbility>(earth_tremor_cone.AssetGuid, "EarthTremorLine", "");
            earth_tremor_line.SetName("Earth Tremor: 30-foot Line");
            earth_tremor_line.ReplaceComponent<AbilityDeliverProjectile>(a =>
            {
                a.Type = AbilityProjectileType.Line;
                a.Length = 30.Feet();
                a.Projectiles = new BlueprintProjectile[] { library.Get<BlueprintProjectile>("868f9126707bdc5428528dd492524d52") };//same cone
            });

            var earth_tremor_burst = library.CopyAndAdd<BlueprintAbility>(earth_tremor_cone.AssetGuid, "EarthTremorBurst", "");
            earth_tremor_burst.SetName("Earth Tremor: 10-foot Spread");
            earth_tremor_burst.ReplaceComponent<AbilityDeliverProjectile>(Helpers.CreateAbilityTargetsAround(10.Feet(), TargetType.Any,
                                                                                                            Helpers.CreateConditionsCheckerOr(Common.createContextConditionIsCaster(not: true))
                                                                                                            )
                                                                        );
            earth_tremor_burst.setMiscAbilityParametersSelfOnly();
            earth_tremor_burst.Range = AbilityRange.Personal;
            earth_tremor = Common.createVariantWrapper("EarthTremorAbility", "", earth_tremor_cone, earth_tremor_line, earth_tremor_burst);
            earth_tremor.SetName("Earth Tremor");
            earth_tremor.AddComponents(Helpers.CreateSpellDescriptor(SpellDescriptor.Ground),
                                       Helpers.CreateSpellComponent(SpellSchool.Transmutation));

            earth_tremor.AddToSpellList(Helpers.druidSpellList, 3);
            earth_tremor.AddToSpellList(Helpers.magusSpellList, 3);
            earth_tremor.AddToSpellList(Helpers.wizardSpellList, 3);
            earth_tremor.AddSpellAndScroll("bc948837c7acb664eb8d89ac7749fa18");
        }


        static void createFrostBite()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/FrostBite.png");

            BlueprintWeaponEnchantment[] frost_bite_enchantments = new BlueprintWeaponEnchantment[10];
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


            var buff = Helpers.CreateBuff("FrostBiteBuff",
                                            frost_bite_enchantments[0].Name,
                                            frost_bite_enchantments[0].Description,
                                            "",
                                            icon,
                                            null,
                                            Helpers.Create<NewMechanics.EnchantmentMechanics.CreateWeapon>(c => { c.weapon = weapon; }),
                                            Common.createBuffContextEnchantPrimaryHandWeapon(Helpers.CreateContextValue(AbilityRankType.DamageBonus), false, false,
                                                                                            new BlueprintWeaponType[0], frost_bite_enchantments),
                                            empower_buff,
                                            maximize_buff,
                                            Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CasterLevel, progression: ContextRankProgression.DivStep,
                                                                            type: AbilityRankType.DamageBonus, stepLevel: 1, max: 10)
                                            );
            buff.Stacking = Kingmaker.UnitLogic.Buffs.Blueprints.StackingType.Replace;

            var reduce_buff_duration = Helpers.Create<ContextActionReduceBuffDuration>(c => { c.TargetBuff = buff; c.DurationValue = Helpers.CreateContextDuration(1, DurationRate.Minutes); });
            foreach (var e in frost_bite_enchantments)
            {
                e.AddComponent(Helpers.Create<NewMechanics.EnchantmentMechanics.ActionOnAttackWithEnchantedWeapon>(a => { a.ActionsOnSelf = Helpers.CreateActionList(reduce_buff_duration); a.only_on_hit = true; }));
            }

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes));
            frost_bite = Helpers.CreateAbility("FrostBiteAbility",
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
            frost_bite.setMiscAbilityParametersTouchHarmful();

            frost_bite.AvailableMetamagic = Kingmaker.UnitLogic.Abilities.Metamagic.Extend | Kingmaker.UnitLogic.Abilities.Metamagic.Heighten | Kingmaker.UnitLogic.Abilities.Metamagic.Empower | Kingmaker.UnitLogic.Abilities.Metamagic.Maximize;

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
            Helpers.SetField(weapon, "m_DisplayNameText", Helpers.CreateString("ChillTouchWeaponName", "Chill Tocuh"));
            Helpers.SetField(weapon, "m_Icon", icon);
            Common.addEnchantment(weapon, WeaponEnchantments.summoned_weapon_enchant);

            var frightened = library.CopyAndAdd<BlueprintBuff>("f08a7239aa961f34c8301518e71d4cdf", "ChillTouchUndeadFrightenedBuff", "");
            frightened.RemoveComponents<SpellDescriptorComponent>();
            var undead = library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");


            for (int i = 0; i < chill_touch_echantments.Length; i++)
            {
                var on_living = Helpers.CreateConditionalSaved(null, Helpers.CreateActionDealDamage(StatType.Strength, Helpers.CreateContextDiceValue(DiceType.Zero, bonus: 1)));
                var on_undead = Helpers.CreateConditionalSaved(null, Common.createContextActionApplyBuff(frightened, Helpers.CreateContextDuration(i + 1, diceType: DiceType.D4, diceCount: 1)));
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
                                            chill_touch_echantments[0].Name,
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

            var reduce_buff_duration = Helpers.Create<ContextActionReduceBuffDuration>(c => { c.TargetBuff = buff; c.DurationValue = Helpers.CreateContextDuration(1, DurationRate.Minutes); });
            foreach (var e in chill_touch_echantments)
            {
                e.AddComponent(Helpers.Create<NewMechanics.EnchantmentMechanics.ActionOnAttackWithEnchantedWeapon>(a => { a.ActionsOnSelf = Helpers.CreateActionList(reduce_buff_duration); a.only_on_hit = true; }));
            }

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes));
            chill_touch = Helpers.CreateAbility("ChillTouchAbility",
                                                  buff.Name,
                                                  buff.Description,
                                                  "",
                                                  buff.Icon,
                                                  AbilityType.Spell,
                                                  Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                  AbilityRange.DoubleMove,
                                                  Helpers.minutesPerLevelDuration,
                                                  "",
                                                  Helpers.CreateRunActions(Common.createContextActionForceAttack()),
                                                  Helpers.CreateSpellComponent(Kingmaker.Blueprints.Classes.Spells.SpellSchool.Necromancy),
                                                  Helpers.Create<NewMechanics.AbilityCasterPrimaryHandFree>(),
                                                  Helpers.CreateSpellDescriptor(SpellDescriptor.Cold),
                                                  Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionOnContextCaster(apply_buff)))
                                                  );
            chill_touch.setMiscAbilityParametersTouchHarmful();

            chill_touch.AvailableMetamagic = Kingmaker.UnitLogic.Abilities.Metamagic.Extend | Kingmaker.UnitLogic.Abilities.Metamagic.Heighten | Kingmaker.UnitLogic.Abilities.Metamagic.Empower | Kingmaker.UnitLogic.Abilities.Metamagic.Maximize;

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
                                                "Ice encrusts the ground, radiating supernatural cold and making it hard for creatures to maintain their balance. This icy ground is treated as difficult terrain. A creature that begins its turn in the affected area takes 1d6 points of cold damage and takes a –2 penalty on saving throws against spells with the cold descriptor for 1 round.",
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
            var area_effect = Helpers.CreateAreaEffectRunAction(round: new GameAction[]{Helpers.CreateActionDealDamage(DamageEnergyType.Cold, Helpers.CreateContextDiceValue(DiceType.D6, 1)),
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
                                                  Helpers.CreateSpellDescriptor(SpellDescriptor.Cold),
                                                  Helpers.CreateAbilityTargetsAround(20.Feet(), TargetType.Any)
                                                  );

            winter_grasp.setMiscAbilityParametersRangedDirectional();
            winter_grasp.SpellResistance = false;
            winter_grasp.AvailableMetamagic = Metamagic.Extend | Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;
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
                                          Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.AttackNearest));

            var concleament_buff = library.CopyAndAdd<BlueprintBuff>("dd3ad347240624d46a11a092b4dd4674", "BloodMistConcleamentBuff", "");
            concleament_buff.SetName("Blood Mist Concleament");
            concleament_buff.SetDescription(buff.Description);
            concleament_buff.FxOnStart = null;
            concleament_buff.ReplaceComponent<AddConcealment>(a => a.Descriptor = ConcealmentDescriptor.Fog);
            concleament_buff.AddComponent(Common.createOutgoingConcelement(concleament_buff.GetComponent<AddConcealment>()));

            var undead = library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");
            var construct = library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f");
            var apply_attack = Helpers.CreateConditionalSaved(null, Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), is_child: true, is_permanent: true));
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
            blood_mist.AvailableMetamagic = Metamagic.Heighten | Metamagic.Extend | Metamagic.Reach | Metamagic.Quicken;
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

            var roar = library.CopyAndAdd<BlueprintAbility>("5f3126d4120b2b244a95cb2ec23d69fb", "SavageMawRoarAbility", "");
            roar.ActionType = Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift;
            roar.SetNameDescriptionIcon("Savage Maw: Roar",
                                        "Your teeth extend and sharpen, transforming your mouth into a maw of razor-sharp fangs. You gain a secondary bite attack that deals 1d4 points of damage plus your Strength modifier. If you confirm a critical hit with this attack, it also deals 1d4 bleed damage. You can end this spell before its normal duration by making a bestial roar as a swift action. When you do, you can make an Intimidate check to demoralize all foes within a 30-foot radius that can hear the roar.",
                                        icon);
            buff.AddComponent(Helpers.CreateAddFact(roar));
            buff.SetBuffFlags(BuffFlags.RemoveOnRest);

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes));

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
                                                Helpers.CreateSpellComponent(SpellSchool.Transmutation)
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
            savage_maw.AddToSpellList(Helpers.rangerSpellList, 2);

            savage_maw.AddSpellAndScroll("1cd597e316ac49941a568312de2be6ae");
        }


        static void createKeenEdge()
        {
            var keen_edge_enchant = library.Get<BlueprintWeaponEnchantment>("102a9c8c9b7a75e4fb5844e79deaf4c0");

            var icon = library.Get<BlueprintActivatableAbility>("27d76f1afda08a64d897cc81201b5218").Icon; //keen weapon bond

            keen_edge = library.CopyAndAdd<BlueprintAbility>("831e942864e924846a30d2e0678e438b", "KeenEdgeAbility", "");

            keen_edge.SetIcon(icon);
            keen_edge.SetDescription("This spell makes a weapon magically keen, improving its ability to deal telling blows. This transmutation doubles the threat range of the weapon. A threat range of 20 becomes 19-20, a threat range of 19-20 becomes 17-20, and a threat range of 18-20 becomes 15-20. The spell can be cast only on piercing or slashing weapons. If cast on arrows or crossbow bolts, the keen edge on a particular projectile ends after one use, whether or not the missile strikes its intended target. Treat shuriken as arrows, rather than as thrown weapons, for the purpose of this spell.\n"
                + "Multiple effects that increase a weapon’s threat range (such as the keen special weapon property and the Improved Critical feat) don’t stack. You can’t cast this spell on a natural weapon, such as a claw.");
            keen_edge.SetName("Keen Edge");
            keen_edge.setMiscAbilityParametersTouchFriendly();
            keen_edge.RemoveComponents<AbilityDeliverTouch>();
            var action = (keen_edge.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionEnchantWornItem).CreateCopy();
            action.Enchantment = keen_edge_enchant;
            action.DurationValue = Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.TenMinutes);

            keen_edge.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(action));
            keen_edge.LocalizedDuration = Helpers.tenMinPerLevelDuration;

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
                                          Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(Helpers.CreateActionList(dmg), wait_for_attack_to_resolve: true, weapon_category: WeaponCategory.SlingStaff)
                                          );
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes));
            flame_arrow = Helpers.CreateAbility("FlameArrowAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                icon,
                                                AbilityType.Spell,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Touch,
                                                Helpers.oneMinuteDuration,
                                                "",
                                                Helpers.CreateRunActions(apply_buff),
                                                Common.createAbilitySpawnFx("352469f228a3b1f4cb269c7ab0409b8e", anchor: AbilitySpawnFxAnchor.SelectedTarget), //CommonTransmutationBuff00
                                                Helpers.CreateSpellComponent(SpellSchool.Transmutation)
                                                );

            flame_arrow.AvailableMetamagic = Metamagic.Empower | Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Maximize | Metamagic.Reach;

            flame_arrow.setMiscAbilityParametersTouchFriendly();

            flame_arrow.AddToSpellList(Helpers.magusSpellList, 3);
            flame_arrow.AddToSpellList(Helpers.wizardSpellList, 3);

            flame_arrow.AddSpellAndScroll("ce41e625eae914d4bad729f090e9001f"); //hurricane arrow
        }


        static void createCountlessEyes()
        {
            var improved_uncanny_dodge = library.Get<BlueprintFeature>("485a18c05792521459c7d06c63128c79");
            countless_eyes = library.CopyAndAdd<BlueprintAbility>("c927a8b0cd3f5174f8c0b67cdbfde539", "CountlessEyesAbility", "");

            countless_eyes.SetName("Countless Eyes");
            countless_eyes.SetDescription("The target sprouts extra eyes all over its body, including on the back of its head. It gains all-around vision and cannot be flanked.");

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
                                                    Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)))),
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
                library.Get<BlueprintWeaponEnchantment>("d42fc23b92c640846ac137dc26e000d4"),
                library.Get<BlueprintWeaponEnchantment>("eb2faccc4c9487d43b3575d7e77ff3f5"),
                library.Get<BlueprintWeaponEnchantment>("80bb8a737579e35498177e1e3c75899b")
            };



            var weapon = library.CopyAndAdd<BlueprintItemWeapon>("6fd0a849531617844b195f452661b2cd", "ForceSwordWeapon", "");//longsword

            Helpers.SetField(weapon, "m_DamageType", force_damage);
            Helpers.SetField(weapon, "m_DisplayNameText", Helpers.CreateString("ForceBladeName", "Force Sword"));
            Helpers.SetField(weapon, "m_Icon", icon);
            Common.addEnchantment(weapon, WeaponEnchantments.summoned_weapon_enchant);


            var buff = Helpers.CreateBuff("ForceSwordBuff",
                                            "Force Sword",
                                            "You create a +1 longsword of pure force sized appropriately for you that you can wield or give to another creature like any other longsword. At 8th level, the sword functions as a +2 longsword. "
                                            + "At 13th level, it functions as a + 3 longsword.A force sword cannot be attacked or harmed by physical attacks, but dispel magic, disintegrate, a sphere of annihilation, or a rod of cancellation affects it.\n"
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
                                                    Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes)
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
                                                              Common.createContextActionApplyBuff(blood_armor_buffs[1], Helpers.CreateContextDuration(), is_from_spell: true, is_child: true, is_permanent: true)
                                                             },
                                            new GameAction[] { Common.createContextActionApplyBuff(blood_armor_buffs[0], Helpers.CreateContextDuration(), is_from_spell: true, is_child: true, is_permanent: true) }
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
                                                                                    is_permanent:true)
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
            poison_breath.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken;
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

            var save_action = Helpers.CreateConditionalSaved(apply_cooldown, apply_entangle);
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
                                              Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(action_list, only_first_hit: true, weapon_category: categories[i])
                                              );

                var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes));
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
                ability.AvailableMetamagic = Metamagic.Heighten | Metamagic.Empower | Metamagic.Extend | Metamagic.Maximize | Metamagic.Quicken;
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
            var dazed = library.Get<BlueprintBuff>("9934fedff1b14994ea90205d189c8759");
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
            sheet_lightning.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach;

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
                                                + "Any creature in the area takes 4d6 points of cold damage from being pelted with the icy spheres.");
            var damage = Helpers.CreateActionDealDamage(DamageEnergyType.Cold, Helpers.CreateContextDiceValue(DiceType.D6, 4), true, true);
            flurry_of_snowballs.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(damage));
            var cold_cone30 = library.Get<BlueprintProjectile>("72b45860bdfb81f4284aa005c04594dd");
            flurry_of_snowballs.ReplaceComponent<AbilityDeliverProjectile>(a => { a.Projectiles = new BlueprintProjectile[] { cold_cone30 }; a.Length = 30.Feet(); });


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
            var failure_action = Common.createContextActionSkillCheck(StatType.SkillMobility, Helpers.CreateActionList(apply_prone));
            var area_effect = Helpers.CreateAreaEffectRunAction(unitEnter: Common.createContextActionApplyBuff(difficult_terrain, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false),
                                                                unitExit: Helpers.Create<ContextActionRemoveBuffSingleStack>(r => r.TargetBuff = difficult_terrain),
                                                                unitMove: Common.createContextActionSkillCheck(StatType.SkillMobility, failure: Helpers.CreateActionList(failure_action), custom_dc: 10));
            slick_area.ReplaceComponent<AbilityAreaEffectRunAction>(area_effect);
            slick_area.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Ground));

            var deal_damage = Helpers.CreateActionDealDamage(DamageEnergyType.Cold, Helpers.CreateContextDiceValue(DiceType.D6, 1, Helpers.CreateContextValue(AbilityRankType.DamageBonus)), true, true);
            var spawn_area = Common.createContextActionSpawnAreaEffect(slick_area, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes));
            ice_slick = Helpers.CreateAbility("IceSlickAbility",
                                              "Ice Slick",
                                              "You create a blast of intense cold, coating all solid surfaces in the area with a thin coating of ice.\n"
                                              + "Any creature in the area when the spell is cast takes 1d6 points of cold damage + 1 point per caster level (maximum +10) and falls prone; creatures that succeed at a Reflex save take half damage and don’t fall prone. Spell resistance applies to this initial effect.\n"
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
            ice_slick.AvailableMetamagic = Metamagic.Extend | Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;
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
                                          Helpers.Create<NewMechanics.DoubleWeaponSize>(d => d.categories = new WeaponCategory[] { WeaponCategory.OtherNaturalWeapons, WeaponCategory.Bite, WeaponCategory.Claw, WeaponCategory.Gore, WeaponCategory.UnarmedStrike })
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
                                               "",
                                               Helpers.CreateRunActions(apply_buff),
                                               magic_fang.GetComponent<AbilitySpawnFx>(),
                                               Helpers.CreateSpellComponent(SpellSchool.Transmutation)
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

            int max_variants = 6;
            Predicate<AbilityData> check_slot_predicate = delegate (AbilityData spell)
            {
                return spell.Spellbook != null
                        && spell.SpellLevel <= (spell.Spellbook.CasterLevel / 3)
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
                                                            AbilityType.Spell,
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

            var apply_buff = Common.createContextActionApplyBuff(contingency_store_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), rate: DurationRate.Days));
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
                                                Helpers.CreateSpellComponent(SpellSchool.Evocation)
                                                );
            contingency.setMiscAbilityParametersSelfOnly();
            Common.setAsFullRoundAction(contingency);
            contingency.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken;
            contingency.AddToSpellList(Helpers.wizardSpellList, 6);
            contingency.AddSpellAndScroll("beab337b352b5ac479698e2bbc08f4ce"); //circle of death
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
                                                                 Helpers.CreateContextDiceValue(DiceType.D6, Common.createSimpleContextValue(1), Helpers.CreateContextValue(AbilityRankType.DamageBonus))
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
                shields[i].setMiscAbilityParametersSelfOnly();
                fire_shield_variants.Add(energy[i], shields[i]);
                shields[i].SpellResistance = true;
            }

            fire_shield = Helpers.CreateAbility("FireShieldAbility",
                                                "Fire Shield",
                                                "This spell wreathes you in flame and causes damage to each creature that attacks you in melee. The flames also protect you from either cold-based or fire-based attacks, depending on if you choose cool or warm flames for your fire shield.\n"
                                                + "Any creature striking you with its body or a hand - held weapon deals normal damage, but at the same time the attacker takes 1d6 points of damage + 1 point per caster level(maximum + 15). This damage is either cold damage (if you choose a chill shield) or fire damage (if you choose a warm shield). If the attacker has spell resistance, it applies to this effect. Creatures wielding melee weapons with reach are not subject to this damage if they attack you.",
                                                "",
                                                shield_of_dawn.Icon,
                                                AbilityType.Spell,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Personal,
                                                Helpers.roundsPerLevelDuration,
                                                Helpers.savingThrowNone,
                                                shield_of_dawn.GetComponent<SpellComponent>());
            fire_shield.AvailableMetamagic = shield_of_dawn.AvailableMetamagic;
            fire_shield.setMiscAbilityParametersSelfOnly();
            fire_shield.SpellResistance = true;

            fire_shield.AddComponent(fire_shield.CreateAbilityVariants(shields));
            fire_shield.AddToSpellList(Helpers.wizardSpellList, 4);
            fire_shield.AddToSpellList(Helpers.magusSpellList, 4);
            fire_shield.AddToSpellList(Helpers.alchemistSpellList, 4);

            fire_shield.AddSpellAndScroll("8e0c81ac23fe75b4288c21ee57f55e3f"); // shield of dawn

            //replace 5th level spell in fire domain
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("881b2137a1779294c8956fe5b497cc35"), fire_shield, 5);
            //replace 4th level spell in sun domain
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("c85c8791ee13d4c4ea10d93c97a19afc"), fire_shield, 4);
        }


        static void createCommand()
        {
            var dominate_person = library.Get<BlueprintAbility>("d7cbd2004ce66a042aeab2e95a3c5c61");
            BlueprintBuff[] buffs = new BlueprintBuff[]{library.Get<BlueprintBuff>("9934fedff1b14994ea90205d189c8759"), //daze
                                                         library.Get<BlueprintBuff>("24cf3deb078d3df4d92ba24b176bda97"), //prone
                                                         library.Get<BlueprintBuff>("f08a7239aa961f34c8301518e71d4cdf") //frightened
                                                        };
            string[] names = { "Halt", "Fall", "Run" };
            string[] descriptions = { "The subject stands in place for 1 round. It may not take any actions but is not considered helpless.",
                                      "On its turn, the subject falls to the ground and remains prone for 1 round. It may act normally while prone but takes any appropriate penalties.",
                                      "On its turn, the subject moves away from you as quickly as possible for 1 round. It may do nothing but move during its turn, and it provokes attacks of opportunity for this movement as normal." };

            List<BlueprintAbility> commands = new List<BlueprintAbility>();


            command = Helpers.CreateAbility("CommandSpellAbility",
                                            "Command",
                                            "You give the subject a single command, which it obeys to the best of its ability at its earliest opportunity.",
                                            "",
                                            dominate_person.Icon,
                                            AbilityType.Spell,
                                            Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                            AbilityRange.Close,
                                            Helpers.oneRoundDuration,
                                            Helpers.willNegates);

            command.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            command.EffectOnAlly = AbilityEffectOnUnit.Harmful;
            command.CanTargetEnemies = true;
            command.CanTargetFriends = true;
            command.CanTargetSelf = false;
            command.CanTargetPoint = false;
            command.Animation = dominate_person.Animation;
            command.AnimationStyle = dominate_person.AnimationStyle;
            command.AddComponent(dominate_person.GetComponent<SpellDescriptorComponent>());
            command.AddComponent(dominate_person.GetComponent<SpellComponent>());
            command.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach | Metamagic.Extend;
            command.SpellResistance = true;



            for (int i = 0; i < buffs.Length; i++)
            {
                var variant_command = library.CopyAndAdd<BlueprintAbility>(command.AssetGuid, $"CommandSpell{i + 1}Ability", "");
                variant_command.SetDescription(descriptions[i]);
                variant_command.SetName($"Command ({names[i]})");

                var buff_action = Common.createContextSavedApplyBuff(buffs[i],
                                                                      Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Rounds)
                                                                     );
                var buff_save = Common.createContextActionSavingThrow(SavingThrowType.Will, Helpers.CreateActionList(buff_action));

                variant_command.AddComponent(Helpers.CreateRunActions(buff_save));
                variant_command.AddComponent(dominate_person.GetComponent<AbilitySpawnFx>());
                variant_command.AddComponent(dominate_person.GetComponent<AbilityTargetHasFact>());
                variant_command.AddComponents(dominate_person.GetComponents<AbilityTargetHasNoFactUnless>());
                commands.Add(variant_command);
            }

            command.AddComponent(command.CreateAbilityVariants(commands));
            command.AddToSpellList(Helpers.clericSpellList, 1);
            command.AddToSpellList(Helpers.inquisitorSpellList, 1);

            command.AddSpellAndScroll("f199f6e5026488c499042900b572eb7f"); //dominate person
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
            sanctuary.AvailableMetamagic = Kingmaker.UnitLogic.Abilities.Metamagic.Extend | Kingmaker.UnitLogic.Abilities.Metamagic.Quicken | Kingmaker.UnitLogic.Abilities.Metamagic.Heighten;
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
                                                                 Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes)
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
                                               "",
                                               bless_weapon.GetComponent<AbilitySpawnFx>(),
                                               bless_weapon.GetComponent<ContextRankConfig>(),
                                               Helpers.CreateRunActions(apply_buff),
                                               Helpers.CreateSpellComponent(Kingmaker.Blueprints.Classes.Spells.SpellSchool.Transmutation),
                                               Common.createAbilitTargetMainWeaponCheck(shillelagh_types[0].Category, shillelagh_types[1].Category)
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


        static void createFlameBlade()
        {
            var bless_weapon = library.Get<BlueprintAbility>("831e942864e924846a30d2e0678e438b");
            var flaming_enchatment = library.Get<BlueprintWeaponEnchantment>("ed7b5eb80e2a974499c3dd7aeca71f88");
            var scimitar_type = library.Get<BlueprintWeaponType>("d9fbec4637d71bd4ebc977628de3daf3");
            var immaterial = Helpers.Create<NewMechanics.EnchantmentMechanics.Immaterial>();
            BlueprintWeaponEnchantment[] flame_blade_enchantments = new BlueprintWeaponEnchantment[11];
            var fire_damage = Common.createEnergyDamageDescription(Kingmaker.Enums.Damage.DamageEnergyType.Fire);

            var weapon = library.CopyAndAdd<BlueprintItemWeapon>("5363519e36752d84698e03a86fb33afb", "FlameBladeWeapon", "");//scimitar
            var damage_type = new DamageTypeDescription()
            {
                Type = DamageType.Energy,
                Energy = DamageEnergyType.Fire,
                Common = new DamageTypeDescription.CommomData(),
                Physical = new DamageTypeDescription.PhysicalData()
            };

            Helpers.SetField(weapon, "m_DamageType", damage_type);
            Helpers.SetField(weapon, "m_DisplayNameText", Helpers.CreateString("FlameBladeName", "Flame Blade"));
            Helpers.SetField(weapon, "m_Icon", bless_weapon.Icon);

            Common.addEnchantment(weapon, WeaponEnchantments.summoned_weapon_enchant);

            for (int i = 0; i < flame_blade_enchantments.Length; i++)
            {
                var flame_blade_enchant = Helpers.Create<NewMechanics.EnchantmentMechanics.WeaponDamageChange>(w =>
                                                                                    {
                                                                                        w.bonus_damage = i;
                                                                                        w.dice_formula = new DiceFormula(1, DiceType.D8);
                                                                                        w.damage_type_description = fire_damage;
                                                                                    });
                flame_blade_enchantments[i] = Common.createWeaponEnchantment($"FlameBlade{i}Enchantment",
                                                                             "Flame Blade",
                                                                             "A 3-foot-long, blazing beam of red-hot fire springs forth from your hand. You wield this blade-like beam as if it were a scimitar. Attacks with the flame blade are melee touch attacks. The blade deals 1d8 points of fire damage + 1 point per two caster levels (maximum +10). Since the blade is immaterial, your Strength modifier does not apply to the damage. A flame blade can ignite combustible materials such as parchment, straw, dry sticks, and cloth.\n"
                                                                             + "Your primary hand must be free when you cast this spell.",
                                                                             "",
                                                                             "",
                                                                             "",
                                                                             0,
                                                                             flaming_enchatment.WeaponFxPrefab,
                                                                             immaterial,
                                                                             flame_blade_enchant
                                                                             );
            }





            var empower_buff = Common.createBuffContextEnchantPrimaryHandWeaponIfHasMetamagic(Kingmaker.UnitLogic.Abilities.Metamagic.Empower,
                                                                                                  false, false,
                                                                                                  new BlueprintWeaponType[] { scimitar_type }, WeaponEnchantments.empower_enchant);

            var maximize_buff = Common.createBuffContextEnchantPrimaryHandWeaponIfHasMetamagic(Kingmaker.UnitLogic.Abilities.Metamagic.Maximize,
                                                                                                  false, false,
                                                                                                  new BlueprintWeaponType[] { scimitar_type }, WeaponEnchantments.maximize_enchant);


            var buff = Helpers.CreateBuff("FlameBladeBuff",
                                            flame_blade_enchantments[0].Name,
                                            flame_blade_enchantments[0].Description,
                                            "",
                                            bless_weapon.Icon,
                                            null,
                                            Helpers.Create<NewMechanics.EnchantmentMechanics.CreateWeapon>(c => c.weapon = weapon),
                                            Common.createBuffContextEnchantPrimaryHandWeapon(Helpers.CreateContextValue(AbilityRankType.DamageBonus), false, false,
                                                                                            new BlueprintWeaponType[] { scimitar_type }, flame_blade_enchantments),
                                            empower_buff,
                                            maximize_buff,
                                            Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CasterLevel, progression: ContextRankProgression.OnePlusDivStep,
                                                                            type: AbilityRankType.DamageBonus, stepLevel: 2)
                                            );
            buff.Stacking = Kingmaker.UnitLogic.Buffs.Blueprints.StackingType.Replace;

            flame_blade = library.CopyAndAdd<BlueprintAbility>(shillelagh.AssetGuid, "FlameBladeAbility", "");
            flame_blade.setMiscAbilityParametersSelfOnly();
            flame_blade.NeedEquipWeapons = false;
            flame_blade.SetIcon(bless_weapon.Icon);
            flame_blade.SetName(buff.Name);
            flame_blade.SetDescription(buff.Description);
            flame_blade.ActionType = Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard;

            flame_blade.ReplaceComponent<NewMechanics.AbilitTargetMainWeaponCheck>(Helpers.Create<NewMechanics.AbilityTargetPrimaryHandFree>());
            flame_blade.ReplaceComponent<SpellComponent>(Helpers.CreateSpellComponent(Kingmaker.Blueprints.Classes.Spells.SpellSchool.Evocation));

            var apply_buff = Common.createContextActionApplyBuff(buff,
                                                    Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes)
                                                );
            flame_blade.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(apply_buff));
            flame_blade.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Empower | Metamagic.Maximize;
            flame_blade.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Fire));

            flame_blade.AddToSpellList(Helpers.druidSpellList, 2);
            flame_blade.AddSpellAndScroll("fbdd06f0414c3ef458eb4b2a8072e502"); //bless weapon

        }


        static void createProduceFlame()
        {
            var fireball = library.Get<BlueprintAbility>("2d81362af43aeac4387a3d4fced489c3");
            var flaming_enchatment = library.Get<BlueprintWeaponEnchantment>("30f90becaaac51f41bf56641966c4121");

            BlueprintWeaponEnchantment[] produce_flame_enchantments = new BlueprintWeaponEnchantment[6];
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
                    w.dice_formula = new DiceFormula(1, DiceType.D6);
                    w.damage_type_description = fire_damage;
                });

                produce_flame_enchantments[i] = Common.createWeaponEnchantment($"ProduceFlame{i}Enchantment",
                                                                             "Produce Flame",
                                                                             "Flames as bright as a torch appear in your open hand. The flames harm neither you nor your equipment.\n"
                                                                             + "In addition to providing illumination, the flames can be hurled or used to touch enemies. You can strike an opponent with a melee touch attack, dealing fire damage equal to 1d6 + 1 point per caster level (maximum +5). Alternatively, you can hurl the flames up to 40 feet as a thrown weapon. When doing so, you attack with a ranged touch attack (with no range penalty) and deal the same damage as with the melee attack. No sooner do you hurl the flames than a new set appears in your hand. Each attack you make reduces the remaining duration by 1 minute. If an attack reduces the remaining duration to 0 minutes or less, the spell ends after the attack resolves.\n"
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
                                            Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CasterLevel, progression: ContextRankProgression.DivStep,
                                                                            type: AbilityRankType.DamageBonus, stepLevel: 1, max: 5)
                                            );
            buff.Stacking = Kingmaker.UnitLogic.Buffs.Blueprints.StackingType.Replace;

            var reduce_buff_duration = Helpers.Create<ContextActionReduceBuffDuration>(c => { c.TargetBuff = buff; c.DurationValue = Helpers.CreateContextDuration(1, DurationRate.Minutes); });
            foreach (var e in produce_flame_enchantments)
            {
                e.AddComponent(Helpers.Create<NewMechanics.EnchantmentMechanics.ActionOnAttackWithEnchantedWeapon>(a => { a.ActionsOnSelf = Helpers.CreateActionList(reduce_buff_duration); }));
            }

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes));
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
                                                  Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionOnContextCaster(apply_buff)))
                                                  );
            produce_flame.setMiscAbilityParametersSingleTargetRangedHarmful();

            produce_flame.AvailableMetamagic = Kingmaker.UnitLogic.Abilities.Metamagic.Extend | Kingmaker.UnitLogic.Abilities.Metamagic.Heighten | Kingmaker.UnitLogic.Abilities.Metamagic.Empower | Kingmaker.UnitLogic.Abilities.Metamagic.Maximize;

            produce_flame.AddToSpellList(Helpers.druidSpellList, 1);
            produce_flame.AddSpellAndScroll("5b172c2c3e356eb43ba5a8f8008a8a5a"); //fireball
            //replace 2nd level spell in fire domain
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("881b2137a1779294c8956fe5b497cc35"), produce_flame, 2);
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
                                              Common.createAttackTypeAttackBonus(Common.createSimpleContextValue(bonus), AttackTypeAttackBonus.WeaponRangeType.Melee, ModifierDescriptor.Luck),
                                              Helpers.CreateAddStatBonus(StatType.AdditionalDamage, bonus, ModifierDescriptor.Luck),
                                              Common.createAbilityScoreCheckBonus(Common.createSimpleContextValue(bonus), ModifierDescriptor.Luck, StatType.Strength),
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
                var tr = Harmony12.Traverse.Create(__instance);
                tr.Field("m_SpellsFiltered").SetValue(null).GetValue();
                return true;
            }
        }


        static public BlueprintAbility[] createWishSpellLevelVariants(string name_prefix, string display_name, string description,  UnityEngine.Sprite icon, BlueprintSpellbook primary_spellbook,
                                             UnitCommand.CommandType command_type, AbilityType ability_type = AbilityType.Spell, BlueprintComponent[] additional_components = null,
                                             bool allow_metamagic = true, bool allow_spells_with_material_components = true, 
                                             bool full_round =  true, int primary_spellbook_level = 8, int secondary_sepllbook_level = 7, BlueprintAbilityResource resource = null)
        {
            if (secondary_sepllbook_level > primary_spellbook_level)
            {
                throw Main.Error($"primary_spellbook_level < secondary_sepllbook_level");
            }
            var spellbooks = library.GetAllBlueprints().OfType<BlueprintSpellbook>();
            //create spell lsit of all concerned spells and pick their lowest levels
            Dictionary<string, int> spell_guid_level_map = new Dictionary<string, int>();

            foreach (var spellbook in spellbooks)
            {
                int max_level = spellbook == primary_spellbook ? primary_spellbook_level : secondary_sepllbook_level;
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
                                                                                    s => {
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
                                                                                  spells_per_level[i].ToArray()
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
    }
}
