using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Class.Kineticist.Properties;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    public static class NewFeats
    {
        static public bool test_mode = false;
        static LibraryScriptableObject library => Main.library;
        static public BlueprintFeature raging_brutality;
        static public BlueprintFeature blooded_arcane_strike;
        static public BlueprintFeature riving_strike;
        static public BlueprintFeature coordinated_shot;
        static public BlueprintFeature stalwart;
        static public BlueprintFeature improved_stalwart;
        static public BlueprintFeature furious_focus;
        static public BlueprintFeature planar_wild_shape;
        static public BlueprintParametrizedFeature deity_favored_weapon;
        static public BlueprintFeature guided_hand;
        static public BlueprintFeature deadeyes_blessing;
        static public BlueprintFeature paladin_channel_extra;
        static public BlueprintFeature powerful_shape;
        static public BlueprintFeature discordant_voice;

        static public BlueprintFeature weapon_of_the_chosen;
        static public BlueprintFeature improved_weapon_of_the_chosen;
        static public BlueprintFeature greater_weapon_of_the_chosen;

        static public BlueprintFeature devastating_strike;
        static public BlueprintFeature strike_true;
        static public BlueprintFeature felling_smash;


        static public BlueprintFeature target_of_opportunity;
        static public BlueprintFeature distracting_charge;
        static public BlueprintFeature swarm_scatter;
        static public BlueprintFeature coordinated_charge;

        static public BlueprintFeature spell_bane;
        static public BlueprintFeature osyluth_guile;
        static public BlueprintFeature artful_dodge;

        static public BlueprintFeature hurtful;
        static public BlueprintFeature broken_wing_gambit;
        static public BlueprintFeature extended_bane;
        
        static public BlueprintFeature dazing_assult;
        static public BlueprintFeature stunning_assult;

        static public BlueprintFeature unsanctioned_knowledge;

        static public BlueprintFeature linnorm_style, linnorm_vengeance, linnorm_wrath;
        static public BlueprintFeature jabbing_style, jabbing_dancer, jabbing_master;


        static public BlueprintParametrizedFeature mages_tattoo;
        static public BlueprintParametrizedFeature spontaneous_metafocus;
        static public BlueprintParametrizedFeature spell_perfection;

        static public BlueprintFeature fey_foundling;

        static public BlueprintFeature snap_shot;
        static public BlueprintFeature improved_snap_shot;
        static public BlueprintFeature greater_snap_shot;

        static public BlueprintFeature dervish_dance;
        static public BlueprintFeature perfect_strike;
        static public BlueprintFeature perfect_strike_extra_reroll;
        static public BlueprintParametrizedFeature perfect_strike_unlocker;


        static public BlueprintParametrizedFeature feral_grace;
        static public BlueprintFeature shielded_mage;
        static public BlueprintFeature stumbling_bash;
        static public BlueprintFeature toppling_bash;

        static public BlueprintFeature shield_brace;
        static public BlueprintFeature unhindering_shield; //fix checks, magus spell combat
        static public BlueprintFeature tower_shield_specialsit;
        static public BlueprintFeature improved_shield_focus;
        static public BlueprintFeature prodigious_two_weapon_fighting;

        static public BlueprintFeature improved_spell_sharing;
        static public BlueprintFeatureSelection animal_ally;


        static public BlueprintFeature bullseye_shot;
        static public BlueprintFeature pinpoint_targeting;

        static public BlueprintFeature scales_and_skin;
        static public BlueprintFeature improved_combat_expertise;

        static public BlueprintFeature greater_spell_specialization;
        static public BlueprintFeatureSelection preferred_spell;

        static public BlueprintFeature vicious_stomp;

        static public BlueprintFeature steadfast_personality;
        static public BlueprintFeature two_weapon_rend;

        static internal void load()
        {
            Main.logger.Log("New Feats test mode " + test_mode.ToString());
            createFuriousFocus();
            replaceIconsForExistingFeats();
            createRagingBrutality();
            createBloodedArcaneStrike();
            createRivingStrike();
            createCoordiantedShot();
            createStalwart();
            FeralCombatTraining.load();

            createPlanarWildShape();
            createGuidedHand();
            createDeadeyesBlessing();
            createExtraChannelPaladin();
            ChannelEnergyEngine.createChannelingScourge();
            //improved channel is created in channel energy engine
            ChannelEnergyEngine.createVersatileChanneler();

            createPowerfulShape();
            createDiscordantVoice();

            createWeaponOfTheChosen();
            createGreaterWeaponOfTheChosen();

            createDevastatingStrike();
            createStrikeTrue();

            createFellingSmash();

            createDistractingCharge();
            createTargetOfOpportunity();
            createSwarmTactics();
            //createCoordinatedCharge();

            createSpellBane();
            createOsyluthGuile();
            createArtfulDodge();

            createHurtful();
            createBrokenWingGambit();
            createExtendedBane();

            createDazingAssault();
            createStunningAssault();

            createUnsanctionedKnowledge();
            createLinnormStyleFeats();
            createJabbingStyleFeats();
            fixKineticKnightCombatExpertiseReplacement();

            createMagesTattoo();
            createSpontaneousMetafocus();
            createSpellPerfection();
            createFeyFoundling();

            createSnapShot();
            createDervishDance();
            createPerfectStrike();

            createFeralGrace();

            createShieldedMage();
            createStumblingBash();
            createTopplingBash();

            createShieldBrace();
            createUnhinderingShield();
            createTowerShieldSpecialist();
            createTwoWeaponRend();
            createProdigalTwoWeaponFighting();
            createImprovedSpellSharing();
            createAnimalAlly();

            createBullsEyeShot();
            createPinpointTargeting();

            createScalesAndSkin();
            createImprovedCombatExpertise();
            createGreaterSpellSpecialization();

            createViciousStomp();
            createSteadfastPersonality();
        }


        static void createTwoWeaponRend()
        {
            var twf = library.Get<BlueprintFeature>("ac8aaf29054f5b74eb18f2af950e752d");
            var double_slice = library.Get<BlueprintFeature>("8a6a1920019c45d40b4561f05dcb3240");

            two_weapon_rend = Helpers.CreateFeature("TwoWeaponRendFeature",
                                                    "Two-Weapon Rend",
                                                    "Benefit: If you hit an opponent with both your primary hand and your off-hand weapon, you deal an additional 1d10 points of damage plus 1-1/2 times your Strength modifier. You can only deal this additional damage once each round.",
                                                    "",
                                                    Helpers.GetIcon("c126adbdf6ddd8245bda33694cd774e8"), //gtwf
                                                    FeatureGroup.CombatFeat,
                                                    Helpers.Create<NewMechanics.TwoWeaponRendFeature>(),
                                                    Helpers.PrerequisiteStatValue(StatType.Dexterity, 17),
                                                    Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 11),
                                                    Helpers.PrerequisiteFeature(twf),
                                                    Helpers.PrerequisiteFeature(double_slice)
                                                    );
            two_weapon_rend.Groups = two_weapon_rend.Groups.AddToArray(FeatureGroup.Feat);
            library.AddCombatFeats(two_weapon_rend);

            var ranger_style10 = library.Get<BlueprintFeatureSelection>("ee907845112b8aa4e907cf326e6142a6");
            ranger_style10.AllFeatures = ranger_style10.AllFeatures.AddToArray(two_weapon_rend);
            Antipaladin.iron_tyrant_bonus_feat.AllFeatures = Antipaladin.iron_tyrant_bonus_feat.AllFeatures.AddToArray(two_weapon_rend);

            var ranger_styles = new BlueprintFeatureSelection[]{library.Get<BlueprintFeatureSelection>("019e68517c95c5447bff125b8a91c73f"),
                                                       library.Get<BlueprintFeatureSelection>("59d5445392ac62245a5ece9b01c05ee8"),
                                                       library.Get<BlueprintFeatureSelection>("ee907845112b8aa4e907cf326e6142a6")
                                                      };

            foreach (var rs in ranger_styles)
            {
                rs.SetDescription("At 2nd level, the ranger can select from Double Slice, Shield Bash, and Two-Weapon Fighting.\nAt 6th level, he adds Improved Two-Weapon Fighting to the list.\nAt 10th level, he adds Greater Two-Weapon Fighting and Two-Weapon Rend to the list.");
            }
        }


        static void createSteadfastPersonality()
        {
            steadfast_personality = Helpers.CreateFeature("SteadfastPersonality",
                                                          "Steadfast Personality",
                                                          "Add your Charisma modifier instead of your Wisdom bonus on Will saves against mind-affecting effects. If you have a Wisdom penalty, you must apply both your Wisdom penalty and your Charisma modifier.",
                                                          "",
                                                          LoadIcons.Image2Sprite.Create(@"FeatIcons/SteadfastPersonality.png"),
                                                          FeatureGroup.Feat,
                                                          Helpers.Create<NewMechanics.ReplaceSaveStatForSpellDescriptor>(r =>
                                                          {
                                                              r.old_stat = StatType.Wisdom;
                                                              r.new_stat = StatType.Charisma;
                                                              r.keep_penalty = true;
                                                              r.save_type = SavingThrowType.Will;
                                                              r.spell_descriptor = SpellDescriptor.MindAffecting;
                                                          })
                                                          );

            library.AddFeats(steadfast_personality);
        }


        static void createViciousStomp()
        {
            vicious_stomp = Helpers.CreateFeature("ViciousStompFeature",
                                                  "Vicious Stomp",
                                                  "Whenever an opponent falls prone adjacent to you, that opponent provokes an attack of opportunity from you. This attack must be an unarmed strike.",
                                                  "b51d5a2516964ec5b21e4583e768476b",
                                                  LoadIcons.Image2Sprite.Create(@"FeatIcons/ViciousStomp.png"),
                                                  FeatureGroup.CombatFeat,
                                                  Helpers.Create<CombatManeuverMechanics.ViciousStomp>(),
                                                  Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("0f8939ae6f220984e8fb568abbdfba95")), //combat reflexes
                                                  Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("7812ad3672a4b9a4fb894ea402095167")) //improved unarmed strike
                                                  );

            vicious_stomp.Groups = vicious_stomp.Groups.AddToArray(FeatureGroup.Feat);
            library.AddCombatFeats(vicious_stomp);
        }



        static void createGreaterSpellSpecialization()
        {
            library.Get<BlueprintFeature>("fe9220cdc16e5f444a84d85d5fa8e3d5").SetName(library.Get<BlueprintFeature>("f327a765a4353d04f872482ef3e48c35").Name); //set name to spell specialization progression
            greater_spell_specialization = Helpers.CreateFeature("GreaterSpellSpecialization",
                                                 "Greater Spell Specialization",
                                                 "By sacrificing a prepared spell of the same level than your specialized spell, you may spontaneously cast your specialized spell. The specialized spell is treated as its normal level, regardless of the spell slot used to cast it. You may add a metamagic feat to the spell by increasing the spell slot and casting time, just like a cleric spontaneously casting a cure or inflict spell with a metamagic feat.",
                                                 "",
                                                 Helpers.GetIcon("f327a765a4353d04f872482ef3e48c35"),
                                                 FeatureGroup.WizardFeat,
                                                 Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("16fa59cc9a72a6043b566b49184f53fe")), //spell focus
                                                 Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("fe9220cdc16e5f444a84d85d5fa8e3d5")), //spell specialization progression
                                                 Common.createPrerequisiteCasterTypeSpellLevel(true, 5, true),
                                                 Helpers.Create<SpellbookMechanics.PrerequisiteDivineCasterTypeSpellLevel>(p => { p.RequiredSpellLevel = 5; p.Group = Prerequisite.GroupType.Any; }),
                                                 Helpers.Create<SpellbookMechanics.PrerequisitePsychicCasterTypeSpellLevel>(p => { p.RequiredSpellLevel = 5; p.Group = Prerequisite.GroupType.Any; })
                                                 );
            greater_spell_specialization.Groups = new FeatureGroup[] { FeatureGroup.WizardFeat, FeatureGroup.Feat };
            BlueprintFeature[] spell_specializations = Main.library.Get<BlueprintFeatureSelection>("fe67bc3b04f1cd542b4df6e28b6e0ff5").AllFeatures;
            spell_specializations = spell_specializations.AddToArray(Main.library.Get<BlueprintParametrizedFeature>("f327a765a4353d04f872482ef3e48c35")); //spell specialization first

            foreach (var ss in spell_specializations)
            {
                ss.AddComponents(Helpers.Create<SpellManipulationMechanics.GreaterSpellSpecialization>(g => g.required_feat = greater_spell_specialization),
                                 Helpers.Create<RecalculateOnFactsChange>(r => r.CheckedFacts = new BlueprintUnitFact[] {greater_spell_specialization})
                                 );
            }

            library.AddWizardFeats(greater_spell_specialization);
            library.AddFeats(greater_spell_specialization);
        }

        static internal void createPreferredSpell()
        {
            preferred_spell = Helpers.CreateFeatureSelection("PreferredSpellFeatureSelection",
                                                             "Preferred Spell",
                                                             "Choose one spell which you have the ability to cast. You can cast that spell spontaneously by sacrificing a prepared spell or spell slot of equal or higher level. You can apply any metamagic feats you possess to this spell when you cast it. This increases the minimum level of the prepared spell or spell slot you must sacrifice in order to cast it but does not affect the casting time.\n"
                                                             + "Special: You can gain this feat multiple times. Its effects do not stack. Each time you take the feat, it applies to a different spell.",
                                                             "",
                                                             LoadIcons.Image2Sprite.Create(@"FeatIcons/PreferredSpell.png"),
                                                             FeatureGroup.WizardFeat,
                                                             Helpers.PrerequisiteStatValue(StatType.SkillKnowledgeArcana, 5),
                                                             Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("2f5d1e705c7967546b72ad8218ccf99c"))
                                                             );
            preferred_spell.HideInCharacterSheetAndLevelUp = true;
            preferred_spell.Groups = new FeatureGroup[] { FeatureGroup.WizardFeat, FeatureGroup.Feat };
            library.AddWizardFeats(preferred_spell);
            library.AddFeats(preferred_spell);


            Common.addSpellbooksToSelection("PreferredSpellParametrized", 1, addToPreferredSpell,
                                            preferred_spell);
        }


        static void addToPreferredSpell(BlueprintFeatureSelection selection, BlueprintSpellbook spellbook, string name, string display_name, params BlueprintComponent[] components)
        {
            var @class = (components.AsEnumerable().First(f => (f is PrerequisiteClassSpellLevel)) as PrerequisiteClassSpellLevel).CharacterClass;
            var feature = Helpers.CreateParametrizedFeature(name,
                                                            preferred_spell.Name + " (" + display_name + ")",
                                                            preferred_spell.Description,
                                                            Helpers.MergeIds(preferred_spell.AssetGuid, spellbook.AssetGuid),
                                                            preferred_spell.Icon,
                                                            FeatureGroup.Feat,
                                                            (FeatureParameterType)NewMechanics.ParametrizedFeatureSelection.FeatureParameterTypeExtender.KnownSpell,
                                                            components.AddToArray(Helpers.Create<SpellManipulationMechanics.PreferredSpell>(p => p.character_class = @class))
                                                            );
            feature.AddComponent(Helpers.Create<SpellManipulationMechanics.NoSpontnaeousMetamagicCastingTimeIncreaseForSelectedSpell>(a => a.max_metamagics = 100));
            feature.Groups = feature.Groups.AddToArray(FeatureGroup.WizardFeat);
            feature.SpellcasterClass = @class;
            feature.BlueprintParameterVariants = library.Get<BlueprintParametrizedFeature>("e69a85f633ae8ca4398abeb6fa11b1fe").BlueprintParameterVariants;
            selection.AllFeatures = selection.AllFeatures.AddToArray(feature);
        }

        static void createImprovedCombatExpertise()
        {
            var combat_expertise_buff = library.Get<BlueprintBuff>("e81cd772a7311554090e413ea28ceea1");
            var ce = library.Get<BlueprintFeature>("4c44724ffa8844f4d9bedb5bb27d144a");
            improved_combat_expertise = Helpers.CreateFeature("ImprovedCombatExpertiseFeature",
                                                            "Improved Combat Expertise",
                                                            "When you use Combat Expertise, reduce the number you subtract from melee attack rolls and combat maneuvers by 2.",
                                                            "",
                                                            combat_expertise_buff.Icon,
                                                            FeatureGroup.Feat,
                                                            Helpers.PrerequisiteStatValue(StatType.Intelligence, 13),
                                                            Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 8),
                                                            Helpers.PrerequisiteFeature(ce)
                                                            );

            var ce_property = library.Get<BlueprintUnitProperty>("8a63b06d20838954e97eb444f805ec89");
            var comp = ce_property.GetComponent<BaseAttackPropertyWithFeatureList>();
            comp.Features = comp.Features.AddToArray(improved_combat_expertise, improved_combat_expertise);
            improved_combat_expertise.Groups = improved_combat_expertise.Groups.AddToArray(FeatureGroup.CombatFeat);
            library.AddCombatFeats(improved_combat_expertise);
        }


        static void createScalesAndSkin()
        {
            var scales_and_skin_buff = Helpers.CreateBuff("ScalesAndSkinBuff",
                                                          "Scales and Skin",
                                                          "Whenever a transmutation spell or spell-like ability affects you, your natural armor bonus increases by 1. If you have no natural armor bonus to Armor Class, treat your natural armor bonus as 0 for the purposes of this feat. The bonus to your natural armor bonus increases by 2 if the caster level of the effect is 10th or higher.",
                                                          "",
                                                          Helpers.GetIcon("7bc8e27cba24f0e43ae64ed201ad5785"),
                                                          null,
                                                          Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.NaturalArmor),
                                                          Helpers.CreateContextRankConfig(progression: ContextRankProgression.OnePlusDivStep, stepLevel: 10, max: 2),
                                                          Helpers.Create<OnCastMechanics.IgnoreActionOnApplyBuff>()
                                                          );

            var apply_buff = Common.createContextActionApplyBuff(scales_and_skin_buff, Helpers.CreateContextDuration(), is_child: true, is_permanent: true, dispellable: false);
            scales_and_skin = Helpers.CreateFeature("ScalesAndSkinFeature",
                                                    scales_and_skin_buff.Name,
                                                    scales_and_skin_buff.Description,
                                                    "",
                                                    scales_and_skin_buff.Icon,
                                                    FeatureGroup.Feat,
                                                    Helpers.PrerequisiteStatValue(StatType.Constitution, 13),
                                                    Helpers.Create<OnCastMechanics.ActionOnApplyBuff>(a =>
                                                                                                        {
                                                                                                            a.actions = Helpers.CreateActionList(apply_buff);
                                                                                                            a.school = SpellSchool.Transmutation;
                                                                                                            a.use_buff_context = true;
                                                                                                        }
                                                                                                      )
                                                    );
            library.AddFeats(scales_and_skin);                                    
        }


        static void createPinpointTargeting()
        {
            var ranged_weapons = new WeaponCategory[] {WeaponCategory.Longbow, WeaponCategory.Shortbow, WeaponCategory.Shuriken, WeaponCategory.Sling, WeaponCategory.SlingStaff, WeaponCategory.HandCrossbow,
                                                       WeaponCategory.LightCrossbow, WeaponCategory.HeavyCrossbow, WeaponCategory.Dart, WeaponCategory.ThrowingAxe, WeaponCategory.LightRepeatingCrossbow,
                                                        WeaponCategory.HeavyRepeatingCrossbow, WeaponCategory.Javelin};

            var buff = Helpers.CreateBuff("PinpointTargetingBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Helpers.Create<NewMechanics.IgnoreAcShieldAndNaturalArmor>());
            buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var ability = Helpers.CreateAbility("PinpointTargetingAbility",
                                                "Pinpoint Targeting",
                                                "As a standard action, make a single ranged attack. The target does not gain any armor, natural armor, or shield bonuses to its Armor Class.",
                                                "",
                                                LoadIcons.Image2Sprite.Create(@"FeatIcons/PinpointTargeting.png"),
                                                AbilityType.Special,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Weapon,
                                                "",
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionApplyBuffToCaster(buff, Helpers.CreateContextDuration(1), dispellable: false),
                                                                            Common.createContextActionAttack(Helpers.CreateActionList(Common.createContextActionRemoveBuffFromCaster(buff)),
                                                                                                            Helpers.CreateActionList(Common.createContextActionRemoveBuffFromCaster(buff))
                                                                                                            )
                                                                        ),
                                                Helpers.Create<NewMechanics.AttackAnimation>(),
                                                //Helpers.Create<NewMechanics.AbilityCasterMoved>(a => a.not = true),
                                                Common.createAbilityCasterMainWeaponCheck(ranged_weapons)
                                                );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful();

            pinpoint_targeting = Common.AbilityToFeature(ability, false);
            pinpoint_targeting.Groups = new FeatureGroup[] { FeatureGroup.Feat, FeatureGroup.CombatFeat };

            pinpoint_targeting.AddComponents(Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("0da0c194d6e1d43419eb8d990b28e0ab")),//point blank shot
                                            Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("8f3d1e6b4be006f4d896081f2f889665")),//precise shot
                                            Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("46f970a6b9b5d2346b10892673fe6e74")),//improved precise shot
                                            Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 16),
                                            Helpers.PrerequisiteStatValue(StatType.Dexterity, 19)
                                           );
            library.AddCombatFeats(pinpoint_targeting);

            var archery_style10 = library.Get<BlueprintFeatureSelection>("7ef950ca681955d47bc4efbe77073e2c");
            archery_style10.AllFeatures = archery_style10.AllFeatures.AddToArray(pinpoint_targeting);
            archery_style10.Features = archery_style10.Features.AddToArray(pinpoint_targeting);
        }

        static void createBullsEyeShot()
        {
            var true_strike = library.Get<BlueprintAbility>("2c38da66e5a599347ac95b3294acbe00");
            var buff = Helpers.CreateBuff("BullsEyeShotBuff",
                                            "Bullseye Shot",
                                            "You can spend a move action to steady your shot. When you do, you gain a +4 bonus on your next ranged attack roll before the end of your turn.",
                                            "",
                                            true_strike.Icon,
                                            null,
                                            Helpers.CreateAddStatBonus(StatType.AdditionalAttackBonus, 4, ModifierDescriptor.UntypedStackable),
                                            Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(Helpers.Create<ContextActionRemoveSelf>()), only_hit: false, on_initiator: true,
                                                                                             check_weapon_range_type: true, range_type: AttackTypeAttackBonus.WeaponRangeType.Ranged,
                                                                                             wait_for_attack_to_resolve: true)
                                          );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), dispellable: false, duration_seconds: 4);
            var strike_true_ability = Helpers.CreateAbility("BullseyeShotFeature",
                                                            buff.Name,
                                                            buff.Description,
                                                            "",
                                                            buff.Icon,
                                                            AbilityType.Special,
                                                            Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Move,
                                                            AbilityRange.Personal,
                                                            "First Attack or until end of the round.",
                                                            "",
                                                            Helpers.CreateRunActions(apply_buff)
                                                            );
            strike_true_ability.setMiscAbilityParametersSelfOnly(animation: Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Special);
            bullseye_shot = Common.AbilityToFeature(strike_true_ability, false);
            bullseye_shot.Groups = new FeatureGroup[] { FeatureGroup.Feat, FeatureGroup.CombatFeat };
            bullseye_shot.SetIcon(null);
            bullseye_shot.AddComponents(Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("0da0c194d6e1d43419eb8d990b28e0ab")),//point blank shot
                                        Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("8f3d1e6b4be006f4d896081f2f889665")),//precise shot
                                        Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 5)
                                       );
            library.AddCombatFeats(bullseye_shot);
        }


        static void createAnimalAlly()
        {
            var rank_feature = library.Get<BlueprintFeature>("1670990255e4fe948a863bafd5dbda5d");
            animal_ally = library.CopyAndAdd<BlueprintFeatureSelection>("2ecd6c64683b59944a7fe544033bb533", "AnimalAllyFeatureSelection", "");
            animal_ally.SetNameDescription("Animal Ally",
                                           "You gain an animal companion as if you were a druid of your character level –3 from the following list: leopard, dog or wolf. If you later gain an animal companion through another source (such as the Animal domain, divine bond, hunter’s bond, mount, or nature bond class features), the effective druid level granted by this feat stacks with that granted by other sources.");
            
            animal_ally.ComponentsArray = new BlueprintComponent[] {Helpers.Create<AddFeatureOnApply>(a => a.Feature = rank_feature),
                                                                    Helpers.Create<CompanionMechanics.SetAnimalCompanionRankToCharacterLevel>(s => { s.rank_feature = rank_feature; s.level_diff = -3; }) };
            animal_ally.Group = FeatureGroup.None;

            animal_ally.AllFeatures = new BlueprintFeature[]
            {
                library.Get<BlueprintFeature>("472091361cf118049a2b4339c4ea836a"), //empty
                library.Get<BlueprintFeature>("f894e003d31461f48a02f5caec4e3359"), //dog
                library.Get<BlueprintFeature>("e992949eba096644784592dc7f51a5c7"), //ekun wolf
                library.Get<BlueprintFeature>("2ee2ba60850dd064e8b98bf5c2c946ba"), //leopard
                library.Get<BlueprintFeature>("67a9dc42b15d0954ca4689b13e8dedea"), //wolf
            };

            animal_ally.AddComponent(Helpers.CreateAddFact(SharedSpells.ac_share_spell));

            var skill_focus_nature = library.Get<BlueprintFeature>("6507d2da389ed55448e0e1e5b871c013");
            animal_ally.AddComponent(Helpers.PrerequisiteFeature(skill_focus_nature));
            animal_ally.AddComponent(Helpers.PrerequisiteCharacterLevel(4));
            animal_ally.AddComponent(Helpers.Create<PrerequisitePet>(a => a.NoCompanion = true));
            animal_ally.IsClassFeature = true;
            animal_ally.ReapplyOnLevelUp = true;
            library.AddFeats(animal_ally);
        }


        static void createImprovedSpellSharing()
        {
            var animal_calss = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("4cd1757a0eea7694ba5c933729a53920");
            var icon = LoadIcons.Image2Sprite.Create(@"FeatIcons/ImprovedSpellSharing.png");

            var buff = Helpers.CreateBuff("ImprovedSpellSharingBuff",
                                          "Improved Spell Sharing",
                                          "When you are adjacent to or sharing a square with your companion creature and that companion creature has this feat, you can cast a spell on yourself and divide the duration evenly between yourself and the companion creature. You can use this feat only on spells with a duration of at least 2 rounds. For example, you could cast bull’s strength on yourself, and instead of the spell lasting 1 minute per level on yourself, it lasts 5 rounds per level on yourself and 5 rounds per level on your companion.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicOnPersonalSpell>(a => { a.Metamagic = (Metamagic)MetamagicFeats.MetamagicExtender.ImprovedSpellSharing; })
                                          );

            var toggle = Helpers.CreateActivatableAbility("ImprovedSpellSharingToggleAbility",
                                                          buff.Name,
                                                          buff.Description,
                                                          "",
                                                          buff.Icon,
                                                          buff,
                                                          AbilityActivationType.Immediately,
                                                          Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                          null,
                                                          Helpers.Create<CompanionMechanics.CompanionWithinRange>(c => c.range = 5.Feet())
                                                          );
            toggle.DeactivateImmediately = true;


            improved_spell_sharing = Common.ActivatableAbilityToFeature(toggle, false);
            improved_spell_sharing.Groups = new FeatureGroup[] { FeatureGroup.TeamworkFeat, FeatureGroup.Feat };

            improved_spell_sharing.AddComponents(Helpers.Create<PrerequisitePet>(p => p.Group = Prerequisite.GroupType.Any),
                                                 Helpers.PrerequisiteClassLevel(animal_calss, 1, any: true),
                                                 Helpers.PrerequisiteClassLevel(Eidolon.eidolon_class, 1, any: true));
            toggle.AddComponent(Helpers.Create<CompanionMechanics.CompanionHasFactRestriction>(c => c.fact = improved_spell_sharing));
            library.AddFeats(improved_spell_sharing);
            Common.addTemworkFeats(improved_spell_sharing);
          
            var spells = library.GetAllBlueprints().OfType<BlueprintAbility>().Where(b => b.IsSpell && !b.HasAreaEffect() && (b.CanTargetFriends || b.CanTargetSelf) && !b.CanTargetPoint && ((b.AvailableMetamagic & Metamagic.Extend) != 0)).Cast<BlueprintAbility>().ToArray();
            foreach (var s in spells)
            {
                s.AvailableMetamagic = s.AvailableMetamagic | (Metamagic)MetamagicFeats.MetamagicExtender.ImprovedSpellSharing;
                if (s.Parent != null)
                {
                    s.AvailableMetamagic = s.AvailableMetamagic | (Metamagic)MetamagicFeats.MetamagicExtender.ImprovedSpellSharing;
                }
            }
        }

        static void createProdigalTwoWeaponFighting()
        {
            prodigious_two_weapon_fighting = Helpers.CreateFeature("ProdigiousTwoWeaponFightingFeature",
                                                 "Prodigious Two-Weapon Fighting",
                                                 "You may fight with a one-handed weapon in your offhand as if it were a light weapon. In addition, you may use your Strength score instead of your Dexterity score for the purpose of qualifying for Two-Weapon Fighting and any feats with Two-Weapon Fighting as a prerequisite.",
                                                 "",
                                                 LoadIcons.Image2Sprite.Create(@"FeatIcons/ProdigiousTwoWeaponFighting.png"),
                                                 FeatureGroup.Feat,
                                                 Helpers.PrerequisiteStatValue(StatType.Strength, 13)
                                                 );

            var twf = library.Get<BlueprintFeature>("ac8aaf29054f5b74eb18f2af950e752d");
            var itwf = library.Get<BlueprintFeature>("9af88f3ed8a017b45a6837eab7437629");
            var gtwf = library.Get<BlueprintFeature>("c126adbdf6ddd8245bda33694cd774e8");
            var double_slice = library.Get<BlueprintFeature>("8a6a1920019c45d40b4561f05dcb3240");

            var twf_feats = new BlueprintFeature[] { twf, itwf, gtwf, double_slice, two_weapon_rend };

            //fix twf prerequisites
            foreach (var feat in twf_feats)
            {
                var dex_prerequisite = feat.GetComponents<PrerequisiteStatValue>().Where(p => p.Stat == StatType.Dexterity).FirstOrDefault();
                dex_prerequisite.Group = Prerequisite.GroupType.Any;

                feat.AddComponent(Helpers.Create<PrerequisiteMechanics.CompoundPrerequisite>(c =>
                                                                                            {
                                                                                                c.prerequisite1 = Helpers.PrerequisiteFeature(prodigious_two_weapon_fighting);
                                                                                                c.prerequisite2 = Helpers.PrerequisiteStatValue(StatType.Strength, dex_prerequisite.Value);
                                                                                                c.Group = Prerequisite.GroupType.Any;
                                                                                            }
                                                                                            )
                                 );
            }

            prodigious_two_weapon_fighting.Groups = prodigious_two_weapon_fighting.Groups.AddToArray(FeatureGroup.CombatFeat);
            library.AddCombatFeats(prodigious_two_weapon_fighting);
            Antipaladin.iron_tyrant_bonus_feat.AllFeatures = Antipaladin.iron_tyrant_bonus_feat.AllFeatures.AddToArray(prodigious_two_weapon_fighting);

            if (Main.settings.balance_fixes)
            {
                var ranger_styles = new BlueprintFeatureSelection[]{library.Get<BlueprintFeatureSelection>("019e68517c95c5447bff125b8a91c73f"),
                                                       library.Get<BlueprintFeatureSelection>("59d5445392ac62245a5ece9b01c05ee8"),
                                                       library.Get<BlueprintFeatureSelection>("ee907845112b8aa4e907cf326e6142a6")
                                                      };

                var rs1 = library.Get<BlueprintFeatureSelection>("019e68517c95c5447bff125b8a91c73f");

                rs1.AllFeatures = rs1.AllFeatures.RemoveFromArray(double_slice);
                foreach (var rs in ranger_styles)
                {
                    rs.AllFeatures = rs.AllFeatures.AddToArray(prodigious_two_weapon_fighting);
                    rs.SetDescription("At 2nd level, the ranger can select Prodigious Two-Weapon Fighting, Shield Bash, and Two-Weapon Fighting.\nAt 6th level, he adds Double Slice to the list.\nAt 10th level, he adds Two-Weapon Rend to the list.");
                }
            }
        }


        //fix twf to work correctly with prodigious two weapon fighting
        [Harmony12.HarmonyPriority(Harmony12.Priority.First)]
        [Harmony12.HarmonyPatch(typeof(TwoWeaponFightingAttackPenalty))]
        [Harmony12.HarmonyPatch("OnEventAboutToTrigger", Harmony12.MethodType.Normal)]
        [Harmony12.HarmonyPatch(new Type[] {typeof(RuleCalculateAttackBonusWithoutTarget) })]
        class TwoWeaponFightingAttackPenalty__OnEventAboutToTrigger__Patch
        {
            static BlueprintFeature shield_master = library.Get<BlueprintFeature>("dbec636d84482944f87435bd31522fcc");
            static bool Prefix(TwoWeaponFightingAttackPenalty __instance, RuleCalculateAttackBonusWithoutTarget evt)
            {
                Main.TraceLog();
                RulebookEvent rule = evt.Reason.Rule;
                if (rule != null && rule is RuleAttackWithWeapon attack && !attack.IsFullAttack)
                {
                    return false;
                }
                ItemEntityWeapon maybeWeapon1 = evt.Initiator.Body.PrimaryHand.MaybeWeapon;
                ItemEntityWeapon maybeWeapon2 = evt.Initiator.Body.SecondaryHand.MaybeWeapon;
                if (evt.Weapon == null || maybeWeapon1 == null || (maybeWeapon2 == null || maybeWeapon1.Blueprint.IsNatural) || (maybeWeapon2.Blueprint.IsNatural || maybeWeapon1 == evt.Initiator.Body.EmptyHandWeapon || maybeWeapon2 == evt.Initiator.Body.EmptyHandWeapon) || maybeWeapon1 != evt.Weapon && maybeWeapon2 != evt.Weapon)
                    return false;
                int rank = __instance.Fact.GetRank();
                int num1 = rank <= 1 ? -4 : -2;
                int num2 = rank <= 1 ? -8 : -2;
                int bonus = evt.Weapon != maybeWeapon1 ? num2 : num1;
                UnitPartWeaponTraining partWeaponTraining = __instance.Owner.Get<UnitPartWeaponTraining>();
                bool ignore_weapon_size = (bool)__instance.Owner.State.Features.EffortlessDualWielding && partWeaponTraining != null && partWeaponTraining.IsSuitableWeapon(maybeWeapon2);
                ignore_weapon_size = ignore_weapon_size || __instance.Owner.Unit.Descriptor.HasFact(prodigious_two_weapon_fighting);

                var consider_as_light_unit_part = __instance.Owner.Get<HoldingItemsMechanics.UnitPartConsiderAsLightWeapon>();
                bool is_light = maybeWeapon2.Blueprint.IsLight
                                || (consider_as_light_unit_part != null && consider_as_light_unit_part.canBeUsedOn(maybeWeapon2));

                if (!is_light  && !maybeWeapon1.Blueprint.Double && (!ignore_weapon_size))
                    bonus += -2;

                if (evt.Weapon.IsShield && __instance.Owner.Unit.Descriptor.HasFact(shield_master))
                {
                    bonus = 0;
                }
                evt.AddBonus(bonus, __instance.Fact);
                return false;
            }
        }

        [Harmony12.HarmonyPriority(Harmony12.Priority.First)]
        [Harmony12.HarmonyPatch(typeof(ShieldMaster))]
        [Harmony12.HarmonyPatch("OnEventAboutToTrigger", Harmony12.MethodType.Normal)]
        [Harmony12.HarmonyPatch(new Type[] { typeof(RuleCalculateAttackBonusWithoutTarget) })]
        class ShieldMaster__OnEventAboutToTrigger__Patch
        {
            static BlueprintFeature shield_master = library.Get<BlueprintFeature>("dbec636d84482944f87435bd31522fcc");
            static bool Prefix(ShieldMaster __instance, RuleCalculateAttackBonusWithoutTarget evt)
            {
                Main.TraceLog();
                if (!evt.Initiator.Body.SecondaryHand.HasShield || evt.Weapon == null || !evt.Weapon.IsShield)
                    return false;
                //do nothing regarding penalties, everything is taken care of in twf logic
                int bonus = GameHelper.GetItemEnhancementBonus((ItemEntity)evt.Initiator.Body.SecondaryHand.Shield.ArmorComponent);
                evt.AddBonus(bonus, __instance.Fact);
                return false;
            }
        }






        static void createTowerShieldSpecialist()
        {
            var fighter = library.Get<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");
            var shield_focus = library.Get<BlueprintFeature>("ac57069b6bf8c904086171683992a92a");
            var armor_training = library.Get<BlueprintFeature>("3c380607706f209499d951b29d3c44f3");

            tower_shield_specialsit = Helpers.CreateFeature("TowerShieldSpecialistFeature",
                                                 "Tower Shield Specialist",
                                                 "You reduce the armor check penalty for tower shields by 3, and if you have the armor training class feature, you modify the armor check penalty and maximum Dexterity bonus of tower shields as if they were armor.",
                                                 "",
                                                 LoadIcons.Image2Sprite.Create(@"FeatIcons/TowerShieldSpecialist.png"),
                                                 FeatureGroup.Feat,
                                                 Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 11, any: true),
                                                 Helpers.PrerequisiteClassLevel(fighter, 8, any: true),
                                                 Helpers.PrerequisiteFeaturesFromList(shield_focus, armor_training, Bloodrager.armor_training),
                                                 Helpers.Create<ArmorCheckPenaltyIncrease>(a =>
                                                 {
                                                     a.CheckCategory = true;
                                                     a.Category = ArmorProficiencyGroup.TowerShield;
                                                     a.Bonus = Helpers.CreateContextValue(AbilityRankType.Default);
                                                     a.BonesPerRank = 3;
                                                 }
                                                                                          ),
                                                  Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, ContextRankProgression.AsIs,
                                                                                  feature: armor_training),
                                                  Helpers.Create<RecalculateOnFactsChange>(r => r.CheckedFacts = new BlueprintUnitFact[] {armor_training})
                                                 );
            tower_shield_specialsit.ReapplyOnLevelUp = true;
            tower_shield_specialsit.Groups = tower_shield_specialsit.Groups.AddToArray(FeatureGroup.CombatFeat);
            library.AddCombatFeats(tower_shield_specialsit);
            Antipaladin.iron_tyrant_bonus_feat.AllFeatures = Antipaladin.iron_tyrant_bonus_feat.AllFeatures.AddToArray(tower_shield_specialsit);
        }


        static void createUnhinderingShield()
        {
            var fighter = library.Get<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");
            var shield_focus = library.Get<BlueprintFeature>("ac57069b6bf8c904086171683992a92a");
            var armor_training = library.Get<BlueprintFeature>("3c380607706f209499d951b29d3c44f3");

            unhindering_shield = Helpers.CreateFeature("UnhinderingShieldFeature",
                                                 "Unhindering Shield",
                                                 "You still gain a buckler’s bonus to AC even if you use your shield hand for some other purpose. When you wield a buckler, your shield hand is considered free for the purposes of casting spells, wielding two-handed weapons, and using any other abilities that require you to have a free hand or interact with your shield, such as the swashbuckler’s precise strike deed or the Weapon Finesse feat.",
                                                 "",
                                                 LoadIcons.Image2Sprite.Create(@"FeatIcons/UnhinderingShield.png"),
                                                 FeatureGroup.Feat,
                                                 Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 6, any: true),
                                                 Helpers.PrerequisiteClassLevel(fighter, 4, any: true),
                                                 Helpers.PrerequisiteFeaturesFromList(shield_focus, armor_training, Bloodrager.armor_training),
                                                 Helpers.Create<HoldingItemsMechanics.UnhinderingShield>()
                                                 );
            unhindering_shield.Groups = unhindering_shield.Groups.AddToArray(FeatureGroup.CombatFeat);
            library.AddCombatFeats(unhindering_shield);
        }


        static void createShieldBrace()
        {
            var fighter = library.Get<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");
            var shield_focus = library.Get<BlueprintFeature>("ac57069b6bf8c904086171683992a92a");
            var armor_training = library.Get<BlueprintFeature>("3c380607706f209499d951b29d3c44f3");

            shield_brace = Helpers.CreateFeature("ShieldBraceFeature",
                                                 "Shield Brace",
                                                 "You can use a two-handed weapon sized appropriately for you from the polearm or spears weapon group while also using a light, heavy, or tower shield with which you are proficient. The shield’s armor check penalty (if any) applies to attacks made with the weapon.",
                                                 "",
                                                 LoadIcons.Image2Sprite.Create(@"FeatIcons/ShieldBrace.png"),
                                                 FeatureGroup.Feat,
                                                 Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 3, any: true),
                                                 Helpers.PrerequisiteClassLevel(fighter, 1, any: true),
                                                 Helpers.PrerequisiteFeaturesFromList(shield_focus, armor_training, Bloodrager.armor_training),
                                                 Helpers.Create<HoldingItemsMechanics.ShieldBrace>()
                                                 );
            shield_brace.Groups = shield_brace.Groups.AddToArray(FeatureGroup.CombatFeat);
            library.AddCombatFeats(shield_brace);
            Antipaladin.iron_tyrant_bonus_feat.AllFeatures = Antipaladin.iron_tyrant_bonus_feat.AllFeatures.AddToArray(shield_brace);
        }


        static void createShieldedMage()
        {
            var fighter = library.Get<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");
            var shield_focus = library.Get<BlueprintFeature>("ac57069b6bf8c904086171683992a92a");
            var armor_training = library.Get<BlueprintFeature>("3c380607706f209499d951b29d3c44f3");
            shielded_mage = Helpers.CreateFeature("ShieldedMageFeature",
                                                    "Shielded Mage",
                                                    "You reduce the arcane spell failure of any shield you use by 15% (to a minimum of 0%). Using a shield does not prevent you from completing somatic spell components with the hand wielding the shield.",
                                                    "",
                                                    LoadIcons.Image2Sprite.Create(@"FeatIcons/ShieldedMage.png"),
                                                    FeatureGroup.Feat,
                                                    Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 3, any: true),
                                                    Helpers.PrerequisiteClassLevel(fighter, 1, any: true),
                                                    Helpers.PrerequisiteFeaturesFromList(shield_focus, armor_training, Bloodrager.armor_training),
                                                    Helpers.Create<ArcaneSpellFailureIncrease>(a => { a.ToShield = true; a.Bonus = -15; })
                                                    );
            shielded_mage.Groups = shielded_mage.Groups.AddToArray(FeatureGroup.CombatFeat);
            library.AddCombatFeats(shielded_mage);
        }


        static void createStumblingBash()
        {
            var fighter = library.Get<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");
            var shield_focus = library.Get<BlueprintFeature>("ac57069b6bf8c904086171683992a92a");
            var improved_shield_bash = library.Get<BlueprintFeature>("121811173a614534e8720d7550aae253");
            var armor_training = library.Get<BlueprintFeature>("3c380607706f209499d951b29d3c44f3");

            stumbling_bash = Helpers.CreateFeature("StumblingBashFeature",
                                                  "Stumbling Bash",
                                                  "Creatures struck by your shield bash take a –2 penalty to their AC until the end of your next turn.",
                                                  "",
                                                  LoadIcons.Image2Sprite.Create(@"FeatIcons/StumblingBash.png"),
                                                  FeatureGroup.Feat,
                                                  Helpers.PrerequisiteFeature(improved_shield_bash),
                                                  Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 6, any: true),
                                                  Helpers.PrerequisiteClassLevel(fighter, 4, any: true),
                                                  Helpers.PrerequisiteFeaturesFromList(shield_focus, armor_training, Bloodrager.armor_training)
                                                  );


            var buff = Helpers.CreateBuff("StumblingBashBuff",
                                          stumbling_bash.Name,
                                          stumbling_bash.Description,
                                          "",
                                          stumbling_bash.Icon,
                                          null,
                                          Helpers.CreateAddStatBonus(StatType.AC, -2, ModifierDescriptor.UntypedStackable)
                                          );
            buff.Stacking = StackingType.Replace;
            var apply_buff = Helpers.CreateActionList(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1), dispellable: false));

            stumbling_bash.AddComponent(Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(apply_buff, weapon_category: WeaponCategory.SpikedHeavyShield));
            stumbling_bash.AddComponent(Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(apply_buff, weapon_category: WeaponCategory.SpikedLightShield));
            stumbling_bash.AddComponent(Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(apply_buff, weapon_category: WeaponCategory.WeaponHeavyShield));
            stumbling_bash.AddComponent(Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(apply_buff, weapon_category: WeaponCategory.WeaponLightShield));

            stumbling_bash.Groups = stumbling_bash.Groups.AddToArray(FeatureGroup.CombatFeat);
            library.AddCombatFeats(stumbling_bash);

            Antipaladin.iron_tyrant_bonus_feat.AllFeatures = Antipaladin.iron_tyrant_bonus_feat.AllFeatures.AddToArray(stumbling_bash);
        }


        static void createTopplingBash()
        {
            var fighter = library.Get<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");
            var shield_focus = library.Get<BlueprintFeature>("ac57069b6bf8c904086171683992a92a");
            var improved_shield_bash = library.Get<BlueprintFeature>("121811173a614534e8720d7550aae253");
            var armor_training = library.Get<BlueprintFeature>("3c380607706f209499d951b29d3c44f3");

            toppling_bash = Helpers.CreateFeature("TopplingBashFeature",
                                                  "Toppling Bash",
                                                  "As a swift action when you hit a creature with a shield bash, you can attempt a trip combat maneuver against that creature at a –5 penalty.",
                                                  "",
                                                  LoadIcons.Image2Sprite.Create(@"FeatIcons/TopplingBash.png"),
                                                  FeatureGroup.Feat,
                                                  Helpers.PrerequisiteFeature(improved_shield_bash),
                                                  Helpers.PrerequisiteFeature(stumbling_bash),
                                                  Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 11, any: true),
                                                  Helpers.PrerequisiteClassLevel(fighter, 8, any: true),
                                                  Helpers.PrerequisiteFeaturesFromList(shield_focus, armor_training, Bloodrager.armor_training)
                                                  );


            var buff = Helpers.CreateBuff("TopplingBashBuff",
                                          toppling_bash.Name,
                                          toppling_bash.Description,
                                          "",
                                          toppling_bash.Icon,
                                          null,
                                          Helpers.Create<CombatManeuverMechanics.ManeuverOnAttackWithBonus>(m =>
                                                                                                                {
                                                                                                                    m.categories = new WeaponCategory[] { WeaponCategory.SpikedHeavyShield, WeaponCategory.SpikedLightShield, WeaponCategory.WeaponHeavyShield, WeaponCategory.WeaponLightShield };
                                                                                                                    m.use_swift_action = true;
                                                                                                                    m.bonus = -5;
                                                                                                                    m.maneuver = Kingmaker.RuleSystem.Rules.CombatManeuver.Trip;
                                                                                                                }
                                                                                                            )
                                          );

            var toggle = Helpers.CreateActivatableAbility("TopplingBashToggleAbility",
                                                          buff.Name,
                                                          buff.Description,
                                                          "",
                                                          buff.Icon,
                                                          buff,
                                                          AbilityActivationType.Immediately,
                                                          Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                          null
                                                          );
            toggle.DeactivateImmediately = true;
            toppling_bash.AddComponent(Helpers.CreateAddFact(toggle));
            
            toppling_bash.Groups = toppling_bash.Groups.AddToArray(FeatureGroup.CombatFeat);
            library.AddCombatFeats(toppling_bash);
            Antipaladin.iron_tyrant_bonus_feat.AllFeatures = Antipaladin.iron_tyrant_bonus_feat.AllFeatures.AddToArray(toppling_bash);
        }


        static void createFeralGrace()
        {
            var animal = library.Get<BlueprintCharacterClass>("4cd1757a0eea7694ba5c933729a53920");
            var weapon_finesse = library.Get<BlueprintFeature>("90e54424d682d104ab36436bd527af09");
            feral_grace = library.CopyAndAdd<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e", "FeralGraceParametrizedFeature", "");
            feral_grace.WeaponSubCategory = WeaponSubCategory.Natural;
            feral_grace.Groups = new FeatureGroup[] { FeatureGroup.Feat, FeatureGroup.CombatFeat };
            feral_grace.SetNameDescriptionIcon("Feral Grace",
                                               "Choose one of the animal companion’s natural attack. When the animal companion makes a melee attack with the chosen natural attack using its Dexterity bonus on attack rolls and its Strength bonus on damage rolls, it adds 1/4 of its Hit Dice as a bonus on damage rolls. This bonus damage doesn’t increase or decrease based upon whether the natural attack is a primary or secondary natural attack.\n"
                                               + "Special: You can select this feat multiple times. Its effects don’t stack. Each time you select this feat, choose a different natural attack to apply its benefit to.",
                                               LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Feral_Combat.png"));
            feral_grace.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.PrerequisiteFeature(weapon_finesse),
                Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 3),
                Helpers.PrerequisiteClassLevel(animal, 1, any: true),
                Helpers.PrerequisiteClassLevel(Eidolon.eidolon_class, 1, any: true),
                Helpers.Create<WeaponTrainingMechanics.GraceParametrized>(g => g.value = Helpers.CreateContextValue(AbilityRankType.Default)),
                Helpers.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel, progression: ContextRankProgression.DivStep, stepLevel: 4)
            };

            library.AddCombatFeats(feral_grace);
        }


        static void createPerfectStrike()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"FeatIcons/PerfectStrike.png");
            perfect_strike_unlocker = library.CopyAndAdd<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e", "PerfectStrikeUnlocker", "");
            perfect_strike_unlocker.SetNameDescriptionIcon("Perfect Strike",  "You can use perfect strike feat with specified weapon type.", icon);
            perfect_strike_unlocker.Groups = new FeatureGroup[0];
            perfect_strike_unlocker.ComponentsArray = new BlueprintComponent[0];

            perfect_strike_extra_reroll = Helpers.CreateFeature("PerfectStrikeExtraRerollFeature",
                                                    "",
                                                    "",
                                                    "",
                                                    null,
                                                    FeatureGroup.None);
            perfect_strike_extra_reroll.HideInUI = true;
            perfect_strike_extra_reroll.HideInCharacterSheetAndLevelUp = true;


            var resource = library.CopyAndAdd<BlueprintAbilityResource>("d2bae584db4bf4f4f86dd9d15ae56558", "PerfectStrikeResource", "");

            var cooldown_buff = Helpers.CreateBuff("PerfectStrikeCooldownBuff",
                                                   perfect_strike_unlocker.Name + " Cooldown",
                                                  "You must declare that you are using this feat before you make your attack roll (thus a failed attack roll ruins the attempt). You must use one of the following weapons to make the attack: kama, nunchaku, quarterstaff, and sai. You can roll your attack roll twice and take the higher result. You may attempt a perfect attack once per day for every four levels you have attained and no more than once per round.\n"
                                                  + "Special: A monk may attempt a perfect strike attack a number of times per day equal to his monk level, plus one more time per day for every four levels he has in classes other than monk.",
                                                  "",
                                                  perfect_strike_unlocker.Icon,
                                                  null
                                                  );


            var buff = Helpers.CreateBuff("PerfectStrikeBuff",
                                          perfect_strike_unlocker.Name,
                                          cooldown_buff.Description,
                                          "",
                                          perfect_strike_unlocker.Icon,
                                          null,
                                          Helpers.Create<NewMechanics.RerollOnWeaponCategoryAndSpendResource>(r =>
                                                                                                          {
                                                                                                              r.resource = resource;
                                                                                                              r.required_features = new BlueprintParametrizedFeature[] { perfect_strike_unlocker };
                                                                                                              r.prevent_fact = cooldown_buff;
                                                                                                              r.apply_cooldown_buff_rounds = 1;
                                                                                                              r.extra_reroll_feature = perfect_strike_extra_reroll;
                                                                                                          })
                                                                                                          );
            var toggle = Helpers.CreateActivatableAbility("PerfectStrikeToggleAbility",
                                                          buff.Name,
                                                          buff.Description,
                                                          "",
                                                          buff.Icon,
                                                          buff,
                                                          AbilityActivationType.Immediately,
                                                          Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                          null,
                                                          resource.CreateActivatableResourceLogic(ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                          );
            toggle.DeactivateImmediately = true;



            perfect_strike = Common.ActivatableAbilityToFeature(toggle, false);
            perfect_strike.Groups = new FeatureGroup[] { FeatureGroup.Feat, FeatureGroup.CombatFeat };
            var improved_unarmed_strike = library.Get<BlueprintFeature>("7812ad3672a4b9a4fb894ea402095167");
            perfect_strike.AddComponents(Helpers.PrerequisiteFeature(improved_unarmed_strike),
                                         Helpers.PrerequisiteStatValue(StatType.Dexterity, 13),
                                         Helpers.PrerequisiteStatValue(StatType.Wisdom, 13),
                                         Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 8),
                                         Helpers.CreateAddAbilityResource(resource),
                                         Common.createAddParametrizedFeatures(perfect_strike_unlocker, WeaponCategory.Kama),
                                         Common.createAddParametrizedFeatures(perfect_strike_unlocker, WeaponCategory.Nunchaku),
                                         Common.createAddParametrizedFeatures(perfect_strike_unlocker, WeaponCategory.Quarterstaff),
                                         Common.createAddParametrizedFeatures(perfect_strike_unlocker, WeaponCategory.Sai)
                                         );
            library.AddFeats(perfect_strike);
        }


        static void createDervishDance()
        {
            var slashing_grace = library.Get<BlueprintParametrizedFeature>("697d64669eb2c0543abb9c9b07998a38");
            var weapon_finesse = library.Get<BlueprintFeature>("90e54424d682d104ab36436bd527af09");

            dervish_dance = Helpers.CreateFeature("DervishDanceFeature", 
                                                  "Dervish Dance",
                                                    "When wielding a scimitar with one hand, you can use your Dexterity modifier instead of your Strength modifier on melee attack and damage rolls. You treat the scimitar as a one-handed piercing weapon for all feats and class abilities that require such a weapon (such as a duelist's precise strike ability). The scimitar must be for a creature of your size. You cannot use this feat if you are carrying a weapon or shield in your off hand.",
                                                    "",
                                                    slashing_grace.Icon,
                                                    FeatureGroup.Feat,
                                                    weapon_finesse.PrerequisiteFeature(),
                                                    Helpers.PrerequisiteStatValue(StatType.Dexterity, 13),
                                                    Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 1),
                                                    Helpers.Create<PrerequisiteProficiency>(p =>
                                                    {
                                                        p.WeaponProficiencies = new WeaponCategory[] { WeaponCategory.Scimitar };
                                                        p.ArmorProficiencies = new Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup[0];
                                                    }),
                                                    Helpers.Create<NewMechanics.DamageGraceForWeapon>(d => d.category = WeaponCategory.Scimitar));
            dervish_dance.Groups = dervish_dance.Groups.AddToArray(FeatureGroup.CombatFeat);
            library.AddCombatFeats(dervish_dance);
        }


        static void createSnapShot()
        {
            var weapon_focus = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");
            var point_blank_shot = library.Get<BlueprintFeature>("0da0c194d6e1d43419eb8d990b28e0ab");
            var rapid_shot = library.Get<BlueprintFeature>("9c928dc570bb9e54a9649b3ebfe47a41");

            var ranged_weapons = new WeaponCategory[] {WeaponCategory.Longbow, WeaponCategory.Shortbow, WeaponCategory.Shuriken, WeaponCategory.Sling, WeaponCategory.SlingStaff, WeaponCategory.HandCrossbow,
                                                       WeaponCategory.LightCrossbow, WeaponCategory.HeavyCrossbow, WeaponCategory.Dart, WeaponCategory.ThrowingAxe, WeaponCategory.LightRepeatingCrossbow,
                                                        WeaponCategory.HeavyRepeatingCrossbow, WeaponCategory.Javelin, WeaponCategory.Dagger, WeaponCategory.Starknife};
            snap_shot = Helpers.CreateFeature("SnapShotFeature",
                                                    "Snap Shot",
                                                    "While wielding a ranged weapon with which you have Weapon Focus, you threaten squares within 5 feet of you. You can make attacks of opportunity with that ranged weapon. You do not provoke attacks of opportunity when making a ranged attack as an attack of opportunity.",
                                                    "",
                                                    LoadIcons.Image2Sprite.Create(@"FeatIcons/SnapShot.png"),
                                                    FeatureGroup.Feat,
                                                    Helpers.Create<AooMechanics.AooWithRangedWeapon>(a => { a.required_feature = weapon_focus; a.weapon_categories = ranged_weapons; }),
                                                    Helpers.Create<AooMechanics.DoNotProvokeAooOnAoo>(d => d.weapon_categories = ranged_weapons),
                                                    Helpers.PrerequisiteStatValue(StatType.Dexterity, 13),
                                                    Helpers.PrerequisiteFeature(point_blank_shot),
                                                    Helpers.PrerequisiteFeature(rapid_shot),
                                                    Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 6)
                                                    );


            improved_snap_shot = Helpers.CreateFeature("ImprovedSnapShotFeature",
                                                        "Improved Snap Shot",
                                                        "You threaten an additional 5 feet with Snap Shot.",
                                                        "",
                                                        LoadIcons.Image2Sprite.Create(@"FeatIcons/SnapShot.png"),
                                                        FeatureGroup.Feat,
                                                        Helpers.Create<AooMechanics.AooWithRangedWeapon>(a => { a.required_feature = weapon_focus; a.weapon_categories = ranged_weapons; a.max_range = 8.Feet(); }),
                                                        Helpers.PrerequisiteStatValue(StatType.Dexterity, 15),
                                                        Helpers.PrerequisiteFeature(point_blank_shot),
                                                        Helpers.PrerequisiteFeature(rapid_shot),
                                                        Helpers.PrerequisiteFeature(snap_shot),
                                                        Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 9)
                                                        );


            greater_snap_shot = Helpers.CreateFeature("GreaterSnapShotFeature",
                                            "Greater Snap Shot",
                                            "Whenever you make an attack of opportunity using a ranged weapon and hit, you gain a +2 bonus on the damage roll and a +2 bonus on rolls to confirm a critical hit with that attack. These bonuses increase to +4 when you have base attack bonus +16, and to +6 when you have base attack bonus +20.",
                                            "",
                                            LoadIcons.Image2Sprite.Create(@"FeatIcons/SnapShot.png"),
                                            FeatureGroup.Feat,
                                            Helpers.Create<AooMechanics.AttackDamageBonusOnAoo>(a => { a.value = Helpers.CreateContextValue(AbilityRankType.Default); a.weapon_categories = ranged_weapons; }),
                                            Helpers.PrerequisiteStatValue(StatType.Dexterity, 17),
                                            Helpers.PrerequisiteFeature(point_blank_shot),
                                            Helpers.PrerequisiteFeature(rapid_shot),
                                            Helpers.PrerequisiteFeature(improved_snap_shot),
                                            Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 12),
                                            Helpers.CreateContextRankConfig(ContextRankBaseValueType.BaseAttack, ContextRankProgression.Custom,
                                                                            customProgression: new (int, int)[] { (15, 2), (19, 4), (20, 6) }
                                                                           )
                                            );
            foreach (var c in ranged_weapons)
            {
                snap_shot.AddComponent(Common.createPrerequisiteParametrizedFeatureWeapon(weapon_focus, c, any: true));
                improved_snap_shot.AddComponent(Common.createPrerequisiteParametrizedFeatureWeapon(weapon_focus, c, any: true));
            }

            snap_shot.Groups = snap_shot.Groups.AddToArray(FeatureGroup.CombatFeat);
            improved_snap_shot.Groups = snap_shot.Groups.AddToArray(FeatureGroup.CombatFeat);
            greater_snap_shot.Groups = snap_shot.Groups.AddToArray(FeatureGroup.CombatFeat);
            library.AddCombatFeats(snap_shot, improved_snap_shot, greater_snap_shot);
        }


        static void createFeyFoundling()
        {
            fey_foundling = Helpers.CreateFeature("FeyFoundling",
                                             "Fey Foundling",
                                             "You were found in the wilds as a child.\nWhenever you receive magical healing, you heal an additional 2 points per die rolled. You gain a +2 bonus on all saving throws against death effects. Unfortunately, you also suffer +1 point of damage from cold iron weapons (although you can wield cold iron weapons without significant discomfort).",
                                             "",
                                             Helpers.GetIcon("e8445256abbdc45488c2d90373f7dae8"),
                                             FeatureGroup.Feat,
                                             Helpers.Create<SavingThrowBonusAgainstDescriptor>(s => { s.SpellDescriptor = SpellDescriptor.Death; s.ModifierDescriptor = ModifierDescriptor.Feat; }),
                                             Helpers.Create<HealingMechanics.ReceiveBonusHpPerDie>(r => r.per_die_amount = 2),
                                             Helpers.Create<NewMechanics.PrerequisiteCharacterLevelExact>(p => p.level = 1)
                                             );
            library.AddFeats(fey_foundling);

        }

        static void createMagesTattoo()
        {
            mages_tattoo = library.CopyAndAdd<BlueprintParametrizedFeature>("5b04b45b228461c43bad768eb0f7c7bf", "MagesTatooFeature", "");
            mages_tattoo.SetNameDescriptionIcon("Mage’s Tattoo",
                                                "Select a school of magic (other than divination) in which you have Spell Focus—you cast spells from this school at +1 caster level.",
                                                LoadIcons.Image2Sprite.Create(@"FeatIcons/MagesTattoo.png")
                                                );

            mages_tattoo.SetComponents(Helpers.Create<NewMechanics.SpellLevelIncreaseParametrized>(s => s.bonus_dc = 1));
            library.AddFeats(mages_tattoo);
            library.AddWizardFeats(mages_tattoo);
        }


        static void createSpontaneousMetafocus()
        {
            spontaneous_metafocus = library.CopyAndAdd<BlueprintParametrizedFeature>("8a2bdb52b158bc24d83c9ef1a2cb2af4", "SpontaneousMetafocusFeature", "");
            spontaneous_metafocus.Prerequisite = null;
            spontaneous_metafocus.ParameterType = (FeatureParameterType)NewMechanics.ParametrizedFeatureSelection.FeatureParameterTypeExtender.KnownSpontaneousSpell;
            spontaneous_metafocus.SetNameDescriptionIcon("Spontaneous Metafocus",
                                                         "Pick a single spell that you are able to cast spontaneously. When you apply metamagic feats to that spell, you can cast the spell using the normal casting time instead of at the slower casting time.\n"
                                                         + "Special: You can take this feat multiple times. Each time you select this feat, choose a new spell that you can cast spontaneously; the feat applies to that spell.",
                                                         LoadIcons.Image2Sprite.Create(@"FeatIcons/SpontaneousMetafocus.png")
                                                         );
            var metamagic_feats = library.GetAllBlueprints().OfType<BlueprintFeature>().Where(b => b.Groups.Contains(FeatureGroup.WizardFeat) && (b.GetComponent<AddMetamagicFeat>() != null));

            spontaneous_metafocus.SetComponents(Helpers.Create<SpellManipulationMechanics.NoSpontnaeousMetamagicCastingTimeIncreaseForSelectedSpell>(a => a.max_metamagics = 100),
                                                Helpers.PrerequisiteStatValue(StatType.Charisma, 13),
                                                Helpers.PrerequisiteFeaturesFromList(metamagic_feats)
                                                );
            library.AddFeats(spontaneous_metafocus);
            library.AddWizardFeats(spontaneous_metafocus);
        }


        static void createSpellPerfection()
        {
            spell_perfection = library.CopyAndAdd<BlueprintParametrizedFeature>("8a2bdb52b158bc24d83c9ef1a2cb2af4", "SpellPerfectionFeature", "");
            spell_perfection.Prerequisite = null;
            spell_perfection.ParameterType = (FeatureParameterType)NewMechanics.ParametrizedFeatureSelection.FeatureParameterTypeExtender.KnownSpell;
            spell_perfection.SetNameDescriptionIcon("Spell Perfection",
                                                    "Pick one spell which you have the ability to cast. Whenever you cast that spell you may apply any one metamagic feat you have to that spell without affecting its level or casting time, as long as the total modified level of the spell does not use a spell slot above 9th level. In addition, if you have other feats which allow you to apply a set numerical bonus to any aspect of this spell (such as Spell Focus, Spell Penetration, Weapon Focus [ray], and so on), double the bonus granted by that feat when applied to this spell.",
                                                    LoadIcons.Image2Sprite.Create(@"FeatIcons/SpellPerfection.png")
                                                   );
            var metamagic_feats = library.GetAllBlueprints().OfType<BlueprintFeature>().Where(b => b.Groups.Contains(FeatureGroup.WizardFeat) && (b.GetComponent<AddMetamagicFeat>() != null));


            BlueprintUnitFact[] spell_resistance_feats = new BlueprintUnitFact[]
            {
                library.Get<BlueprintUnitFact>("ee7dc126939e4d9438357fbd5980d459"), //spell penetration
                library.Get<BlueprintUnitFact>("1978c3f91cfbbc24b9c9b0d017f4beec"), //spell penetration greater
            };

            BlueprintUnitFact[] spell_parameters_feats = new BlueprintUnitFact[]
            {
                library.Get<BlueprintUnitFact>("16fa59cc9a72a6043b566b49184f53fe"), //spell focus
                library.Get<BlueprintUnitFact>("5b04b45b228461c43bad768eb0f7c7bf"), //spell focus greater
                library.Get<BlueprintUnitFact>("f327a765a4353d04f872482ef3e48c35"), //spell specialization first
                NewFeats.mages_tattoo,
                Witch.witch_knife,
                library.Get<BlueprintFeature>("9093ceeefe9b84746a5993d619d7c86f"), //allied spell caster
            };
            spell_parameters_feats = spell_parameters_feats.AddToArray(library.Get<BlueprintFeatureSelection>("fe67bc3b04f1cd542b4df6e28b6e0ff5").AllFeatures); //other spell specializations
            spell_parameters_feats = spell_parameters_feats.AddToArray(library.Get<BlueprintFeatureSelection>("bb24cc01319528849b09a3ae8eec0b31").AllFeatures); //elemental foci

            BlueprintUnitFact[] weapon_focus_feats = new BlueprintUnitFact[]
            {
                library.Get<BlueprintUnitFact>("1e1f627d26ad36f43bbd26cc2bf8ac7e"), //wf
                library.Get<BlueprintUnitFact>("09c9e82965fb4334b984a1e9df3bd088") //gwf
            };

            BlueprintUnitFact[] weapon_specialization_feats = new BlueprintUnitFact[]
            {
                library.Get<BlueprintUnitFact>("31470b17e8446ae4ea0dacd6c5817d86"), //ws
                library.Get<BlueprintUnitFact>("7cf5edc65e785a24f9cf93af987d66b3") //gws
            };

            spell_perfection.SetComponents(Helpers.Create<SpellManipulationMechanics.NoSpontnaeousMetamagicCastingTimeIncreaseForSelectedSpell>(a => a.max_metamagics = 1),
                                           Helpers.Create<NewMechanics.MetamagicMechanics.OneFreeMetamagicForSpell>(),
                                           Helpers.Create<NewMechanics.SpellPerfectionDoubleFeatBonuses>(s =>
                                           {
                                               s.spell_resistance_feats = spell_resistance_feats;
                                               s.spell_parameters_feats = spell_parameters_feats;
                                               s.attatck_feats = weapon_specialization_feats;
                                               s.attack_roll_feats = weapon_focus_feats;
                                           }),
                                           Helpers.PrerequisiteStatValue(StatType.SkillKnowledgeArcana, 15),
                                           Helpers.Create<PrerequisiteFeaturesFromList>(p => { p.Amount = 3; p.Features = metamagic_feats.ToArray(); })
                                          );
            library.AddFeats(spell_perfection);
            library.AddWizardFeats(spell_perfection);
        }


        static void fixKineticKnightCombatExpertiseReplacement()
        {
            //make universal, that I no longer need to specify kinetic warrior as alternative to combat expertise for new feats
            var kinetic_warrior = library.Get<BlueprintFeature>("ff14cb2bfab1c0547be66d8aaa7e4ada");
            var combat_expertise = library.Get<BlueprintFeature>("4c44724ffa8844f4d9bedb5bb27d144a");
            combat_expertise.AddComponent(Helpers.Create<NewMechanics.FeatureReplacement>(f => f.replacement_feature = kinetic_warrior));
        }


        static void createJabbingStyleFeats()
        {
            var improved_unarmed_strike = library.Get<BlueprintFeature>("7812ad3672a4b9a4fb894ea402095167");
            var dodge = library.Get<BlueprintFeature>("97e216dbb46ae3c4faef90cf6bbe6fd5");
            var mobility = library.Get<BlueprintFeature>("2a6091b97ad940943b46262600eaeaeb");
            var power_attack = library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5");
            var monk = library.Get<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");

            var jabbing_style_icon = LoadIcons.Image2Sprite.Create(@"FeatIcons/JabbingStyle.png");
            var jabbing_dancer_icon = LoadIcons.Image2Sprite.Create(@"FeatIcons/JabbingDancer.png");
            var jabbing_master_icon = LoadIcons.Image2Sprite.Create(@"FeatIcons/JabbingMaster.png");

            var jabbing_dancer_buff = Helpers.CreateBuff("JabbingDancerBuff",
                                                          "Jabbing Dancer",
                                                          "If you hit an opponent with an unarmed strike while using Jabbing Style you receive + 10 bonus to your mobility until end of the round.",
                                                          "",
                                                          jabbing_dancer_icon,
                                                          null,
                                                          Helpers.CreateAddStatBonus(StatType.SkillMobility, 10, ModifierDescriptor.UntypedStackable)
                                                          );
            jabbing_dancer = Helpers.CreateFeature("JabbingDancerFeature",
                                                   jabbing_dancer_buff.Name,
                                                   jabbing_dancer_buff.Description,
                                                   "",
                                                   jabbing_dancer_buff.Icon,
                                                   FeatureGroup.Feat,
                                                   Helpers.PrerequisiteFeature(dodge),
                                                   Helpers.PrerequisiteFeature(improved_unarmed_strike),
                                                   Helpers.PrerequisiteFeature(mobility),
                                                   Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 9, any: true),
                                                   Helpers.PrerequisiteClassLevel(monk, 5, any: true)
                                                   );
            jabbing_dancer.Groups = jabbing_dancer.Groups.AddToArray(FeatureGroup.CombatFeat, FeatureGroup.StyleFeat);

            var apply_jabbing_dancer_buff = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(jabbing_dancer),
                                                                      Common.createContextActionApplyBuffToCaster(jabbing_dancer_buff, Helpers.CreateContextDuration(1), dispellable: false)
                                                                     );


            var master_target_buff = Helpers.CreateBuff("JabbingMasterTargetBuff",
                                                         "Jabbing Master Target",
                                                         "While using Jabbing Style, the extra damage you deal when you hit a single target with two unarmed strikes increases to 2d6, and the extra damage when you hit a single target with three or more unarmed strikes increases to 4d6.",
                                                         "",
                                                         jabbing_master_icon,
                                                         null);
            master_target_buff.Stacking = StackingType.Stack;

            var target_buff = Helpers.CreateBuff("JabbingStyleTargetBuff",
                                                 "Jabbing Style Target",
                                                 "When you hit a target with an unarmed strike and you have hit that target with an unarmed strike previously that round, you deal an extra 1d6 points of damage to that target.",
                                                 "",
                                                 jabbing_style_icon,
                                                 null,
                                                 Helpers.CreateAddFactContextActions(deactivated: Common.createContextActionRemoveBuffFromCaster(master_target_buff)));
            target_buff.Stacking = StackingType.Stack;

            var apply_target_buff = Helpers.CreateConditional(Common.createContextConditionHasBuffFromCaster(target_buff, not: true),
                                                              Common.createContextActionApplyBuff(target_buff, Helpers.CreateContextDuration(1), dispellable: false)
                                                              );

            jabbing_master = Helpers.CreateFeature("JabbingMasterFeature",
                                                   "Jabbing Master",
                                                   master_target_buff.Description,
                                                   "",
                                                   jabbing_master_icon,
                                                   FeatureGroup.Feat,
                                                   Helpers.PrerequisiteFeature(dodge),
                                                   Helpers.PrerequisiteFeature(improved_unarmed_strike),
                                                   Helpers.PrerequisiteFeature(mobility),
                                                   Helpers.PrerequisiteFeature(power_attack),
                                                   Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 12, any: true),
                                                   Helpers.PrerequisiteClassLevel(monk, 8, any: true));
            jabbing_master.Groups = jabbing_master.Groups.AddToArray(FeatureGroup.CombatFeat, FeatureGroup.StyleFeat);

            var apply_master_buff = Helpers.CreateConditional(new Condition[] { Common.createContextConditionCasterHasFact(jabbing_master), Common.createContextConditionHasBuffFromCaster(target_buff), Common.createContextConditionHasBuffFromCaster(master_target_buff, not: true) },
                                                              Common.createContextActionApplyBuff(master_target_buff, Helpers.CreateContextDuration(1), dispellable: false)
                                                             );

            var on_hit = Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(apply_master_buff, apply_target_buff, apply_jabbing_dancer_buff), wait_for_attack_to_resolve: true, check_weapon_range_type: true);
            var buff = Helpers.CreateBuff("JabbingStyleBuff",
                                          "Jabbing Style",
                                          target_buff.Description,
                                          "",
                                          jabbing_style_icon,
                                          null,
                                          Helpers.Create<NewMechanics.JabbingStrikeDamageBonus>(j => { j.target_buff = target_buff; j.target_buff_master = master_target_buff; j.master_fact = jabbing_master; }),
                                          FeralCombatTraining.AddInitiatorAttackWithWeaponTriggerOrFeralTraining.fromAddInitiatorAttackWithWeaponTrigger(on_hit)
                                          );

            var jabbing_style_ability = Helpers.CreateActivatableAbility("JabbingStyleToggleAbility",
                                                                         buff.Name,
                                                                         buff.Description,
                                                                         "",
                                                                         buff.Icon,
                                                                         buff,
                                                                         AbilityActivationType.Immediately,
                                                                         Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                                         null);
            jabbing_style_ability.Group = ActivatableAbilityGroup.CombatStyle;

            jabbing_style = Common.ActivatableAbilityToFeature(jabbing_style_ability, false);
            jabbing_style.Groups = new FeatureGroup[] { FeatureGroup.Feat, FeatureGroup.CombatFeat, FeatureGroup.StyleFeat };

            jabbing_style.AddComponents(Helpers.PrerequisiteFeature(improved_unarmed_strike),
                                        Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 6, any: true),
                                        Helpers.PrerequisiteClassLevel(monk, 1, any: true)
                                        );

            jabbing_dancer.AddComponent(Helpers.PrerequisiteFeature(jabbing_style));
            jabbing_master.AddComponent(Helpers.PrerequisiteFeature(jabbing_style));
            jabbing_master.AddComponent(Helpers.PrerequisiteFeature(jabbing_dancer));

            library.AddCombatFeats(jabbing_style, jabbing_dancer, jabbing_master);

            Warpriest.sacred_fist_syle_feat_selection.AllFeatures = Warpriest.sacred_fist_syle_feat_selection.AllFeatures.AddToArray(jabbing_style, jabbing_dancer, jabbing_master);
        }


        static void createLinnormStyleFeats()
        {
            var improved_unarmed_strike = library.Get<BlueprintFeature>("7812ad3672a4b9a4fb894ea402095167");

            var linnorm_style_icon = LoadIcons.Image2Sprite.Create(@"FeatIcons/LinnormStyle.png");
            var linnorm_vengeance_icon = LoadIcons.Image2Sprite.Create(@"FeatIcons/LinnormVengeance.png");
            var linnorm_wrath_icon = LoadIcons.Image2Sprite.Create(@"FeatIcons/LinnormWrath.png");
            var target_buff = Helpers.CreateBuff("LinnormStyleTargetBuff",
                                                 "Linnorm Style Target",
                                                 "While using this style, you take a –2 penalty to your AC against melee attacks. After a creature makes a melee attack against you, you can choose to add your Wisdom bonus to the damage from your unarmed strikes against that creature, rather than your Strength bonus. If you can normally add your Dexterity bonus to the attack’s damage, you can instead replace it with your Wisdom bonus. This lasts until the beginning of the target creature’s next turn.",
                                                 "",
                                                 linnorm_style_icon,
                                                 null);
            var apply_target_buff = Helpers.CreateConditional(Common.createContextConditionHasBuffFromCaster(target_buff, not: true),
                                      Common.createContextActionApplyBuff(target_buff, Helpers.CreateContextDuration(1), is_child:true, dispellable: false)
                                      );


            var vengeance_buff = Helpers.CreateBuff("LinnormVengeanceTargetBuff",
                                                     "Linnorm Vengeance Target",
                                                     "While you’re using the Linnorm Style feat and an enemy hits you with a melee attack, you gain a +2 bonus on unarmed strike attack rolls you make against that creature until the beginning of that creature’s next turn.",
                                                     "",
                                                     linnorm_vengeance_icon,
                                                     null,
                                                     Helpers.Create<AttackBonusAgainstTarget>(a => { a.CheckCaster = true; a.Value = 2; })
                                                     );
            vengeance_buff.Stacking = StackingType.Stack;
            linnorm_vengeance = Helpers.CreateFeature("LinnormVengeanceFeature",
                                                      "Linnorm Vengeance",
                                                      vengeance_buff.Description,
                                                      "",
                                                      vengeance_buff.Icon,
                                                      FeatureGroup.Feat,
                                                      Helpers.PrerequisiteStatValue(StatType.Wisdom, 13),
                                                      Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 6),
                                                      Helpers.PrerequisiteFeature(improved_unarmed_strike));
            linnorm_vengeance.Groups = linnorm_vengeance.Groups.AddToArray(FeatureGroup.CombatFeat, FeatureGroup.StyleFeat);

            var apply_vengeance_buff = Helpers.CreateConditional(new Condition[] { Common.createContextConditionCasterHasFact(linnorm_vengeance), Common.createContextConditionHasBuffFromCaster(vengeance_buff, not: true) },
                                                                 Common.createContextActionApplyBuff(vengeance_buff, Helpers.CreateContextDuration(1), is_child: true, dispellable: false)
                                                                );

            var cooldown_buff = Helpers.CreateBuff("LinnormWrathCooldownBuff",
                                                     "Linnorm Wrath Cooldown",
                                                     "When you use the Linnorm Style feat, you can make a retaliatory unarmed strike attack against an opponent that hits you once per round. This acts as an attack of opportunity, and counts against the number of attacks of opportunity you can make each round.",
                                                     "",
                                                     linnorm_wrath_icon,
                                                     null);

            linnorm_wrath = Helpers.CreateFeature("LinnormWrathFeature",
                                          "Linnorm Wrath",
                                          cooldown_buff.Description,
                                          "",
                                          cooldown_buff.Icon,
                                          FeatureGroup.Feat,
                                          Helpers.PrerequisiteStatValue(StatType.Wisdom, 13),
                                          Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 9),
                                          Helpers.PrerequisiteFeature(linnorm_vengeance),
                                          Helpers.PrerequisiteFeature(improved_unarmed_strike));
            linnorm_wrath.Groups = linnorm_vengeance.Groups.AddToArray(FeatureGroup.CombatFeat, FeatureGroup.StyleFeat);

            var make_aoo =  Helpers.CreateConditional(new Condition[] {Common.createContextConditionCasterHasFact(linnorm_wrath), Common.createContextConditionCasterHasFact(cooldown_buff, false) },
                                                      new GameAction[]{Common.createContextActionApplyBuffToCaster(cooldown_buff, Helpers.CreateContextDuration(1), dispellable: false),
                                                                      Helpers.Create<NewMechanics.ContextActionAttackOfOpportunity>()
                                                                     }
                                                     );
            var caster_buff = Helpers.CreateBuff("LinnormStyleCasterBuff",
                                                 "Linnorm Style",
                                                 "While using this style, you take a –2 penalty to your AC against melee attacks. After a creature makes a melee attack against you, you can choose to add your Wisdom bonus to the damage from your unarmed strikes against that creature, rather than your Strength bonus. If you can normally add your Dexterity bonus to the attack’s damage, you can instead replace it with your Wisdom bonus. This lasts until the beginning of the target creature’s next turn.",
                                                 "",
                                                 linnorm_style_icon,
                                                 null,
                                                 Helpers.Create<NewMechanics.WeaponDamageStatReplacementAgainstFactOwner>(w => { w.fact = target_buff; w.only_unarmed_or_feral_combat_training = true; w.new_stat = StatType.Wisdom; }),
                                                 Common.createAddTargetAttackWithWeaponTrigger(null, Helpers.CreateActionList(apply_target_buff), only_hit: false, not_reach: false),
                                                 Common.createAddTargetAttackWithWeaponTrigger(null, Helpers.CreateActionList(apply_vengeance_buff, make_aoo), only_hit: true, not_reach: false),
                                                 Helpers.CreateAddStatBonus(StatType.AC, -2, ModifierDescriptor.UntypedStackable)
                                                 );

            var linnorm_style_ability = Helpers.CreateActivatableAbility("LinnormStyleToggleAbility",
                                                                     caster_buff.Name,
                                                                     caster_buff.Description,
                                                                     "",
                                                                     caster_buff.Icon,
                                                                     caster_buff,
                                                                     AbilityActivationType.Immediately,
                                                                     Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                                     null,
                                                                     Helpers.Create<FeralCombatTraining.AbilityCasterMainWeaponCheckOrFeralCombat>());
            linnorm_style_ability.Group = ActivatableAbilityGroup.CombatStyle;

            linnorm_style = Common.ActivatableAbilityToFeature(linnorm_style_ability, false);
            linnorm_style.Groups = new FeatureGroup[] { FeatureGroup.Feat, FeatureGroup.CombatFeat, FeatureGroup.StyleFeat };
            linnorm_style.AddComponents(Helpers.PrerequisiteStatValue(StatType.Wisdom, 13),
                                        Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 3),
                                        Helpers.PrerequisiteFeature(improved_unarmed_strike));
            linnorm_vengeance.AddComponent(Helpers.PrerequisiteFeature(linnorm_style));
            linnorm_wrath.AddComponent(Helpers.PrerequisiteFeature(linnorm_style));

            library.AddCombatFeats(linnorm_style, linnorm_vengeance, linnorm_wrath);

            Warpriest.sacred_fist_syle_feat_selection.AllFeatures = Warpriest.sacred_fist_syle_feat_selection.AllFeatures.AddToArray(linnorm_style, linnorm_vengeance, linnorm_wrath);
        }


        static void createUnsanctionedKnowledge()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"FeatIcons/UnsanctionedKnowledge.png");
            var cleric_spell_list = library.Get<BlueprintSpellList>("8443ce803d2d31347897a3d85cc32f53");
            var inquisitor_spell_list = library.Get<BlueprintSpellList>("57c894665b7895c499b3dce058c284b3");
            var bard_spell_list = library.Get<BlueprintSpellList>("25a5013493bdcf74bb2424532214d0c8");

            var combined_spell_list = Common.combineSpellLists("UnsanctionedKnowledgeSpellList", cleric_spell_list, inquisitor_spell_list, bard_spell_list);

            var paladin = library.Get<BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec");
            Common.excludeSpellsFromList(combined_spell_list, paladin.Spellbook.SpellList);

            var combined_spell_list_antipaldin = Common.combineSpellLists("UnsanctionedKnowledgeAntipaladinSpellList", cleric_spell_list, inquisitor_spell_list, bard_spell_list);

            Common.excludeSpellsFromList(combined_spell_list_antipaldin, Antipaladin.antipaladin_class.Spellbook.SpellList);

            unsanctioned_knowledge = Helpers.CreateFeature("UnsanctionedKnowledgeFeature",
                                                            "Unsanctioned Knowledge (Paladin)",
                                                            "Pick one 1st-level spell, one 2nd-level spell, one 3rd-level spell, and one 4th-level spell from the bard, cleric, inquisitor, or oracle spell lists. Add these spells to your paladin spell list as paladin spells of the appropriate level. Once chosen, these spells cannot be changed.",
                                                            "",
                                                            icon,
                                                            FeatureGroup.Feat,

                                                            Helpers.Create<NewMechanics.addSpellChoice>(a => { a.spell_book = paladin.Spellbook; a.spell_level = 1; a.spell_list = combined_spell_list; }),
                                                            Helpers.Create<NewMechanics.addSpellChoice>(a => { a.spell_book = paladin.Spellbook; a.spell_level = 2; a.spell_list = combined_spell_list; }),
                                                            Helpers.Create<NewMechanics.addSpellChoice>(a => { a.spell_book = paladin.Spellbook; a.spell_level = 3; a.spell_list = combined_spell_list; }),
                                                            Helpers.Create<NewMechanics.addSpellChoice>(a => { a.spell_book = paladin.Spellbook; a.spell_level = 4; a.spell_list = combined_spell_list; }),
                                                            Helpers.PrerequisiteStatValue(StatType.Intelligence, 13),
                                                            Common.createPrerequisiteClassSpellLevel(paladin, 1)
                                                            );

            


            var bastard_unsanctioned_knowledge = library.CopyAndAdd<BlueprintFeature>(NewFeats.unsanctioned_knowledge.AssetGuid, "VindictiveBastardUnsanctionedKnowledgeFeature", "");
            bastard_unsanctioned_knowledge.ReplaceComponent<PrerequisiteClassSpellLevel>(p => p.CharacterClass = VindicativeBastard.vindicative_bastard_class);
            bastard_unsanctioned_knowledge.SetName("Unsanctioned Knowledge (Vindictive Bastard)");
            foreach (var c in bastard_unsanctioned_knowledge.GetComponents<NewMechanics.addSpellChoice>())
            {
                var new_c = c.CreateCopy(asc => asc.spell_book = VindicativeBastard.vindicative_bastard_spellbook);
                bastard_unsanctioned_knowledge.ReplaceComponent(c, new_c);
            }


            var antipaladin_unsanctioned_knowledge = library.CopyAndAdd<BlueprintFeature>(NewFeats.unsanctioned_knowledge.AssetGuid, "AntipaladinUnsanctionedKnowledgeFeature", "");
            antipaladin_unsanctioned_knowledge.ReplaceComponent<PrerequisiteClassSpellLevel>(p => p.CharacterClass = Antipaladin.antipaladin_class);
            antipaladin_unsanctioned_knowledge.SetName("Unsanctioned Knowledge (Antipaladin)");
            foreach (var c in antipaladin_unsanctioned_knowledge.GetComponents<NewMechanics.addSpellChoice>())
            {
                var new_c = c.CreateCopy(asc => { asc.spell_book = Antipaladin.antipaladin_class.Spellbook; asc.spell_list = combined_spell_list_antipaldin; });
                antipaladin_unsanctioned_knowledge.ReplaceComponent(c, new_c);
            }

            library.AddFeats(unsanctioned_knowledge, bastard_unsanctioned_knowledge, antipaladin_unsanctioned_knowledge);
        }


        static void createDazingAssault()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"FeatIcons/DazingAssault.png");
            var dazed = library.Get<BlueprintBuff>("9934fedff1b14994ea90205d189c8759");

            var apply = Common.createContextActionApplyBuff(dazed, Helpers.CreateContextDuration(1), dispellable: false);
            var effect = Common.createContextActionSavingThrow(SavingThrowType.Fortitude,
                                                               Helpers.CreateActionList(Helpers.CreateConditionalSaved(null, apply))
                                                              );
            var buff = Helpers.CreateBuff("DazingAssaultBuff",
                                          "Dazing Assault",
                                          "You can choose to take a –5 penalty on all melee attack rolls and combat maneuver checks to daze opponents you hit with your melee attacks for 1 round, in addition to the normal damage dealt by the attack. A successful Fortitude save negates the effect. The DC of this save is 10 + your base attack bonus. You must choose to use this feat before making the attack roll, and its effects last until your next turn.",
                                          "",
                                          icon,
                                          null,
                                          Common.createAttackTypeAttackBonus(-5, AttackTypeAttackBonus.WeaponRangeType.Melee, ModifierDescriptor.UntypedStackable),
                                          Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(effect), check_weapon_range_type: true),
                                          library.Get<BlueprintBuff>("f766db0dcd17aaa44b76181bdf85fee9").GetComponent<ContextCalculateAbilityParams>() //from staggering critical
                                          //Helpers.Create<ContextCalculateAbilityParams>(c => { c.StatType = StatType.BaseAttackBonus; c.ReplaceCasterLevel = true; c.ReplaceSpellLevel = true; })
                                          );
            BladeTutor.RuleCalculateAttackBonusWithoutTarget_OnTrigger_Patch.facts.Add(buff);
            var ability = Helpers.CreateActivatableAbility("DazingAssaultToggleAbility",
                                                           buff.Name,
                                                           buff.Description,
                                                           "",
                                                           buff.Icon,
                                                           buff,
                                                           AbilityActivationType.Immediately,
                                                           Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                           null);
            ability.DeactivateImmediately = true;

            dazing_assult = Common.ActivatableAbilityToFeature(ability, false);
            dazing_assult.Groups = new FeatureGroup[] { FeatureGroup.CombatFeat, FeatureGroup.Feat };
            dazing_assult.AddComponents(Helpers.PrerequisiteStatValue(StatType.Strength, 13),
                                        Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 11),
                                        Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5")) //power attack
                                        );
            library.AddCombatFeats(dazing_assult);
        }


        static void createStunningAssault()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"FeatIcons/StunningAssault.png");
            var stunned = library.Get<BlueprintBuff>("09d39b38bb7c6014394b6daced9bacd3");

            var apply = Common.createContextActionApplyBuff(stunned, Helpers.CreateContextDuration(1), dispellable: false);
            var effect = Common.createContextActionSavingThrow(SavingThrowType.Fortitude,
                                                               Helpers.CreateActionList(Helpers.CreateConditionalSaved(null, apply))
                                                              );
            var buff = Helpers.CreateBuff("StunninAssaultBuff",
                                          "Stunning Assault",
                                          "You can choose to take a –5 penalty on all melee attack rolls and combat maneuver checks to stun targets you hit with your melee attacks for 1 round. A successful Fortitude save negates the effect. The DC of this save is 10 + your base attack bonus. You must choose to use this feat before making the attack roll, and its effects last until your next turn.",
                                          "",
                                          icon,
                                          null,
                                          Common.createAttackTypeAttackBonus(-5, AttackTypeAttackBonus.WeaponRangeType.Melee, ModifierDescriptor.UntypedStackable),
                                          Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(effect), check_weapon_range_type: true),
                                          library.Get<BlueprintBuff>("f766db0dcd17aaa44b76181bdf85fee9").GetComponent<ContextCalculateAbilityParams>() //from staggering critical
                                          // Helpers.Create<ContextCalculateAbilityParams>(c => { c.StatType = StatType.BaseAttackBonus; c.ReplaceCasterLevel = true; c.ReplaceSpellLevel = true; })
                                          );
            BladeTutor.RuleCalculateAttackBonusWithoutTarget_OnTrigger_Patch.facts.Add(buff);
            var ability = Helpers.CreateActivatableAbility("StunningAssaultToggleAbility",
                                                           buff.Name,
                                                           buff.Description,
                                                           "",
                                                           buff.Icon,
                                                           buff,
                                                           AbilityActivationType.Immediately,
                                                           Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                           null);
            ability.DeactivateImmediately = true;

            stunning_assult = Common.ActivatableAbilityToFeature(ability, false);
            stunning_assult.Groups = new FeatureGroup[] { FeatureGroup.CombatFeat, FeatureGroup.Feat };
            stunning_assult.AddComponents(Helpers.PrerequisiteStatValue(StatType.Strength, 13),
                                        Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 16),
                                        Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5")) //power attack
                                        );
            library.AddCombatFeats(stunning_assult);
        }


        static void createExtendedBane()
        {
            extended_bane = library.CopyAndAdd<BlueprintFeature>("756dc2f7340b0b34285a1dc367ff7359", "ExtendedBaneFeature", "");
            extended_bane.SetNameDescription("Extended Bane", "Add your Wisdom bonus to the number of rounds per day that you can use your bane ability.");
            var old_increase = extended_bane.GetComponent<IncreaseResourceAmount>();
            extended_bane.ReplaceComponent(old_increase, Helpers.Create<ResourceMechanics.ContextIncreaseResourceAmount>(c => { c.Resource = old_increase.Resource; c.Value = Helpers.CreateContextValue(AbilityRankType.Default); }));
            extended_bane.AddComponents(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Wisdom),
                                        Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Wisdom));

            library.AddFeats(extended_bane);
        }


        static void createBrokenWingGambit()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"FeatIcons/BrokenWingGambit.png");
            broken_wing_gambit = Helpers.CreateFeature("BrokenWingGambitFeature",
                                                       "Broken Wing Gambit",
                                                       "Whenever you make a melee attack and hit your opponent, you can use a free action to grant that opponent a +2 bonus on attack and damage rolls against you until the end of your next turn or until your opponent attacks you, whichever happens first. If that opponent attacks you with this bonus, it provokes attacks of opportunity from your allies who have this feat.",
                                                       "",
                                                       icon,
                                                       FeatureGroup.Feat,
                                                       Helpers.PrerequisiteStatValue(StatType.SkillPersuasion, 5)
                                                       );

            var broken_wing_gambit_effect_buff = Helpers.CreateBuff("BrokenWingEffectGambitBuff",
                                                              broken_wing_gambit.Name,
                                                              broken_wing_gambit.Description,
                                                              "",
                                                              broken_wing_gambit.Icon,
                                                              null,
                                                              Helpers.Create<AttackBonusAgainstCaster>(a => a.Value = 2),
                                                              Helpers.Create<NewMechanics.DamageBonusAgainstCaster>(d => d.Value = 2)
                                                              );
            var apply_buff = Common.createContextActionApplyBuff(broken_wing_gambit_effect_buff, Helpers.CreateContextDuration(1), dispellable: false);
            var broken_wing_gambit_buff = Helpers.CreateBuff("BrokenWingGambitBuff",
                                                              broken_wing_gambit.Name,
                                                              broken_wing_gambit.Description,
                                                              "",
                                                              broken_wing_gambit.Icon,
                                                              null,
                                                              Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(apply_buff), check_weapon_range_type: true)
                                                              );
            broken_wing_gambit_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var broken_wing_ability = Helpers.CreateActivatableAbility("BrokenWingGambitToggleAbility",
                                                                       broken_wing_gambit.Name,
                                                                       broken_wing_gambit.Description,
                                                                       "",
                                                                       broken_wing_gambit.Icon,
                                                                       broken_wing_gambit_buff,
                                                                       AbilityActivationType.Immediately,
                                                                       Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                                       null);
            broken_wing_ability.DeactivateImmediately = true;

            var on_attack = Helpers.CreateConditional(Common.createContextConditionHasFact(broken_wing_gambit_effect_buff),
                                                      Helpers.Create<TeamworkMechanics.ProvokeAttackFromFactOwners>(p => p.fact = broken_wing_gambit)
                                                      );
            broken_wing_gambit.AddComponents(Helpers.CreateAddFact(broken_wing_ability),
                                             Common.createAddTargetAttackWithWeaponTrigger(null,
                                                                                           Helpers.CreateActionList(on_attack),
                                                                                           only_hit: false,
                                                                                           not_reach: false,
                                                                                           wait_for_attack_to_resolve: true)
                                            );

            broken_wing_gambit.Groups = broken_wing_gambit.Groups.AddToArray(FeatureGroup.CombatFeat, FeatureGroup.TeamworkFeat);

            library.AddCombatFeats(broken_wing_gambit);
            Common.addTemworkFeats(broken_wing_gambit);
        }


        static void createHurtful()
        {
            var shaken = library.Get<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220");
            var frightened = library.Get<BlueprintBuff>("f08a7239aa961f34c8301518e71d4cdf");
            var icon = LoadIcons.Image2Sprite.Create(@"FeatIcons/Hurtful.png");
            hurtful = Helpers.CreateFeature("HurtfulFeature",
                                            "Hurtful",
                                            "When you successfully demoralize an opponent within your melee reach with an Intimidate check, you can make a single melee attack against that creature as a swift action. If your attack fails to damage the target, its shaken condition from being demoralized immediately ends.",
                                            "",
                                            icon,
                                            FeatureGroup.Feat,
                                            Helpers.PrerequisiteStatValue(StatType.Strength, 13),
                                            Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5")) //power attack
                                            );

            var hurtful_buff = Helpers.CreateBuff("HurtfulBuff",
                                                  hurtful.Name,
                                                  hurtful.Description,
                                                  "",
                                                  hurtful.Icon,
                                                  null,
                                                  shaken.GetComponent<SpellDescriptorComponent>());
            
            var hurtful_ability = Helpers.CreateAbility("HurtfulAbility",
                                                        hurtful.Name,
                                                        hurtful.Description,
                                                        "",
                                                        hurtful.Icon,
                                                        AbilityType.Special,
                                                        Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift,
                                                        AbilityRange.Weapon,
                                                        "",
                                                        "",
                                                        Helpers.CreateRunActions(Common.createContextActionAttack(Helpers.CreateActionList(Common.createContextActionRemoveBuffFromCaster(hurtful_buff)),
                                                                                                                  Helpers.CreateActionList(Common.createContextActionRemoveBuffFromCaster(shaken),
                                                                                                                                           Common.createContextActionRemoveBuffFromCaster(frightened),
                                                                                                                                           Common.createContextActionRemoveBuffFromCaster(hurtful_buff))
                                                                                                                  )
                                                                                                                  ),
                                                        Helpers.Create<NewMechanics.AttackAnimation>(),
                                                        Helpers.Create<NewMechanics.AbilityTargetHasBuffFromCaster>(a => a.Buffs = new BlueprintBuff[] { hurtful_buff })
                                                        );
            hurtful_ability.setMiscAbilityParametersTouchHarmful();

            hurtful.AddComponent(Helpers.CreateAddFact(hurtful_ability));

            var action = Helpers.CreateConditional(new Condition[] { Common.createContextConditionCasterHasFact(hurtful), Helpers.Create<NewMechanics.ContextConditionEngagedByCaster>() },
                                                   Common.createContextActionApplyBuff(hurtful_buff, Helpers.CreateContextDuration(), dispellable: false, duration_seconds: 3));

            hurtful.AddComponent(Helpers.Create<DemoralizeMechanics.RunActionsOnDemoralize>(r => r.actions = Helpers.CreateActionList(action)));
            hurtful.Groups = hurtful.Groups.AddToArray(FeatureGroup.CombatFeat);
            library.AddCombatFeats(hurtful);
        }


        static void createArtfulDodge()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"FeatIcons/ArtfulDodge.png");
            artful_dodge = Helpers.CreateFeature("ArtfulDodgeFeature",
                                                 "Artful Dodge",
                                                 "If you are the only character threatening an opponent, you gain a +1 dodge bonus to AC against that opponent.\n"
                                                 + "Special: The Artful Dodge feat acts as the Dodge feat for the purpose of satisfying prerequisites that require Dodge.\n"
                                                 + "You can use Intelligence, rather than Dexterity, for feats with a minimum Dexterity prerequisite.",
                                                 "",
                                                 icon,
                                                 FeatureGroup.Feat,
                                                 Helpers.Create<ReplaceStatForPrerequisites>(r => { r.OldStat = StatType.Dexterity; r.NewStat = StatType.Intelligence; r.Policy = ReplaceStatForPrerequisites.StatReplacementPolicy.NewStat; }),
                                                 Helpers.Create<NewMechanics.ACBonusSingleThreat>(a => { a.Bonus = 1; a.Descriptor = ModifierDescriptor.Dodge; }),
                                                 Helpers.PrerequisiteStatValue(StatType.Intelligence, 13)
                                                 );

            library.Get<BlueprintFeature>("97e216dbb46ae3c4faef90cf6bbe6fd5").AddComponent(Helpers.Create<NewMechanics.FeatureReplacement>(f => f.replacement_feature = artful_dodge)); //dodge
            artful_dodge.Groups = artful_dodge.Groups.AddToArray(FeatureGroup.CombatFeat);
            library.AddCombatFeats(artful_dodge);
        }


        static void createOsyluthGuile()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"FeatIcons/OsyluthGuile.png");
            var buff = Helpers.CreateBuff("OsyluthGuileBuff",
                                          "Osyluth Guile",
                                          "While you are fighting defensively or using the total defense action or combat expertise, select one opponent. Add your Charisma bonus to your AC as a dodge bonus against that opponent’s melee attacks until your next turn. You cannot use this feat if you cannot see the selected opponent.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.Create<NewMechanics.ACBonusAgainstTargetIfHasFact>(a =>
                                                                                                      {
                                                                                                          a.CheckCaster = true;
                                                                                                          a.checked_fact = new BlueprintUnitFact[]
                                                                                                          { library.Get<BlueprintBuff>("6ffd93355fb3bcf4592a5d976b1d32a9"), //fight defensively
                                                                                                            library.Get<BlueprintBuff>("e81cd772a7311554090e413ea28ceea1"), //combat expertise
                                                                                                          };
                                                                                                          a.Descriptor = ModifierDescriptor.Dodge;
                                                                                                          a.Value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                                                          a.only_melee = true;
                                                                                                      }
                                                                                                     ),
                                          Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Charisma, min: 0),
                                          Helpers.Create<UniqueBuff>()
                                          );
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true);

            var ability = Helpers.CreateAbility("OsyluthGuileAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Special,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                AbilityRange.Long,
                                                "",
                                                "",
                                                Helpers.CreateRunActions(apply_buff)
                                                );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);

            osyluth_guile = Common.AbilityToFeature(ability, false);
            osyluth_guile.AddComponents(Helpers.PrerequisiteStatValue(StatType.SkillPersuasion, 8),
                                        Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("97e216dbb46ae3c4faef90cf6bbe6fd5"))//dodge
                                        );
            osyluth_guile.Groups = new FeatureGroup[] { FeatureGroup.Feat, FeatureGroup.CombatFeat };
            library.AddCombatFeats(osyluth_guile);
        }


        static void createSpellBane()
        {

            spell_bane = Helpers.CreateFeature("SpellBaneFeature",
                                               "Spell Bane",
                                               "While your bane class feature is affecting a creature type, the saving throw’s DCs for your spells increase by +2 for creatures of that type.",
                                               "",
                                               null,
                                               FeatureGroup.None,
                                               Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("7ddf7fbeecbe78342b83171d888028cf"))//bane
                                               );

            var buffs = new BlueprintBuff[] { library.Get<BlueprintBuff>("be190d2dd5433dd41a4aa00e1abc9a5b"), //bane
                                              library.Get<BlueprintBuff>("60dffde0dd392a84b8c26dc37c471cd1") }; //greater bane

            var spell_bane_buff = Helpers.CreateBuff("SpellBaneBuff",
                                                     "",
                                                     "",
                                                     "",
                                                     null,
                                                     null,
                                                     Helpers.Create<NewMechanics.IncreaseAllSpellsDC>(i => i.Value = 2)
                                                     );
            spell_bane_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var apply_spell_bane = Common.createContextActionApplyBuff(spell_bane_buff, Helpers.CreateContextDuration(), dispellable: false, is_child: true, is_permanent: true);

            foreach (var b in buffs)
            {
                b.AddComponent(Helpers.CreateAddFactContextActions(activated: Helpers.CreateConditional(Common.createContextConditionHasFact(spell_bane), apply_spell_bane)));
            }

            library.AddFeats(spell_bane);
        }


        static void createCoordinatedCharge()
        {
            //test swift charge
            var charge = library.Get<BlueprintAbility>("c78506dd0e14f7c45a599990e4e65038");
            charge.ActionType = Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift;
        }


        static void createSwarmTactics()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"FeatIcons/SwarmScatter.png");

            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("7ced0efa297bd5142ab749f6e33b112b", "AuraSwarmScatterArea", "");
            area.Size = 7.Feet();

            var buff = library.CopyAndAdd<BlueprintBuff>("c96380f6dcac83c45acdb698ae70ffc4", "AuraSwarmScatterBuff", "");
            buff.ReplaceComponent<AddAreaEffect>(a => a.AreaEffect = area);

            swarm_scatter = library.CopyAndAdd<BlueprintFeature>("e45ab30f49215054e83b4ea12165409f", "SwarmScatterFeature", "");
            swarm_scatter.SetName("Swarm Scatter");
            swarm_scatter.SetDescription("For each ally who has this feat and is adjacent to you, you gain a +1 bonus to AC. As long as you have this bonus, you are immune to the swarm attack and distraction ability of rat swarms.");
            swarm_scatter.SetIcon(icon);
            swarm_scatter.RemoveComponents<SpellImmunityToSpellDescriptor>();
            swarm_scatter.RemoveComponents<BuffDescriptorImmunity>();
            swarm_scatter.Groups = new FeatureGroup[] { FeatureGroup.Feat, FeatureGroup.TeamworkFeat };
            swarm_scatter.ReplaceComponent<AuraFeatureComponent>(a => a.Buff = buff);
            var rat_swarm_immunity = library.Get<BlueprintBuff>("60549b98735cde44e87bf247042604c1");

            area.ReplaceComponent<AbilityAreaEffectBuff>(a =>
            {
                a.Buff = rat_swarm_immunity;
                a.Condition = Helpers.CreateConditionsCheckerAnd(Helpers.Create<TeamworkMechanics.ContextConditionAllyOrCasterWithSoloTacticsSurroundedByAllies>(c => c.radius = 5.Feet().Meters),
                                                                 Helpers.CreateConditionHasFact(swarm_scatter)
                                                                );
            }
                                                         );
            swarm_scatter.AddComponent(Helpers.Create<TeamworkMechanics.TeamworkACBonus>(t =>
                                                                                        {
                                                                                            t.descriptor = ModifierDescriptor.Circumstance;
                                                                                            t.value_per_unit = 1;
                                                                                            t.teamwork_fact = swarm_scatter;
                                                                                            t.Radius = 7.Feet().Meters;
                                                                                        }
                                                                                        )
                                       );

            library.AddFeats(swarm_scatter);
            Common.addTemworkFeats(swarm_scatter);
        }

        static void createTargetOfOpportunity()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"FeatIcons/TargetOfOpportunity.png");

            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("7ced0efa297bd5142ab749f6e33b112b", "AuraTargetOfOpportunityArea", "");

            area.Size = 30.Feet();

            var buff = library.CopyAndAdd<BlueprintBuff>("c96380f6dcac83c45acdb698ae70ffc4", "AuraTargetOfOpportunityBuff", "");
            buff.ReplaceComponent<AddAreaEffect>(a => a.AreaEffect = area);

            target_of_opportunity = library.CopyAndAdd<BlueprintFeature>("e45ab30f49215054e83b4ea12165409f", "TargetOfOpportunityFeature", "");
            target_of_opportunity.SetName("Target of Opportunity");
            target_of_opportunity.SetDescription("When an ally who also has this feat makes a ranged attack and hits an opponent within 30 feet of you, you can spend a swift action to make a single ranged attack against that opponent.");
            target_of_opportunity.SetIcon(icon);
            target_of_opportunity.RemoveComponents<SpellImmunityToSpellDescriptor>();
            target_of_opportunity.RemoveComponents<BuffDescriptorImmunity>();
            target_of_opportunity.Groups = new FeatureGroup[] { FeatureGroup.Feat, FeatureGroup.CombatFeat, FeatureGroup.TeamworkFeat };
            target_of_opportunity.ReplaceComponent<AuraFeatureComponent>(a => a.Buff = buff);

            var target_buff = Helpers.CreateBuff("TargetOfOpportunityTargetBuff",
                                             target_of_opportunity.Name + " Target",
                                             "",
                                             "",
                                             icon,
                                             null
                                             );

            var apply_target_buff = Common.createContextActionApplyBuff(target_buff, Helpers.CreateContextDuration(1), dispellable: false);
            var ally_buff = Helpers.CreateBuff("TargetOfOpportunityTargetAllyBuff",
                                 target_of_opportunity.Name,
                                 "",
                                 "",
                                 icon,
                                 null
                                 );
            //ally_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var apply_ally_buff = Common.createContextActionApplyBuff(ally_buff, Helpers.CreateContextDuration(1), dispellable: false);

            var attack = Common.createAttackAbility("TargetOfOpportunityAttackAbility", target_of_opportunity.Name, target_of_opportunity.Description, "",
                                                    icon, Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift,
                                                    Common.createAbilityCasterMainWeaponCheck(Common.getRangedWeaponCategories()),
                                                    Common.createAbilityTargetHasFact(false, target_buff),
                                                    Common.createAbilityCasterHasFacts(ally_buff)
                                                    );

            var effect_on_ally1 = Helpers.CreateConditional(new Condition[] { Helpers.Create<ContextConditionIsAlly>(), Helpers.Create<TeamworkMechanics.ContextConditionHasSoloTactics>(), Helpers.CreateConditionHasFact(target_of_opportunity) },
                                                            apply_ally_buff
                                                          );

            var effect_on_ally2 = Helpers.CreateConditional(new Condition[] { Helpers.Create<ContextConditionIsAlly>(), Helpers.CreateConditionHasFact(target_of_opportunity) },
                                                            apply_ally_buff,
                                                            null

                                              );

            var actions_on_ally1 = Helpers.Create<TeamworkMechanics.ContextActionOnUnitsWithinRadius>(c => { c.Radius = 30.Feet(); c.actions = Helpers.CreateActionList(effect_on_ally1); });
            var actions_on_ally2 = Helpers.CreateConditional(Helpers.CreateConditionCasterHasFact(target_of_opportunity),
                                                             Helpers.Create<TeamworkMechanics.ContextActionOnUnitsWithinRadius>(c => { c.Radius = 30.Feet(); c.actions = Helpers.CreateActionList(effect_on_ally2); })
                                                             );

            var attacker_buff = Helpers.CreateBuff("TargetOfOpportunityAttackerBuff",
                                                   "TargetOfOpportunityAttackerBuff",
                                                   "",
                                                   "",
                                                   null,
                                                   null,
                                                   Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(apply_target_buff, actions_on_ally1), check_weapon_range_type: true, range_type: AttackTypeAttackBonus.WeaponRangeType.Ranged),
                                                   Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(Common.createContextActionRemoveBuff(ally_buff)),
                                                                                                    only_hit: false,
                                                                                                    on_initiator: true)
                                                   );

            attacker_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            area.ReplaceComponent<AbilityAreaEffectBuff>(a => a.Buff = attacker_buff);
            target_of_opportunity.AddComponents(Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(actions_on_ally2),
                                                                                                    check_weapon_range_type: true,
                                                                                                    range_type: AttackTypeAttackBonus.WeaponRangeType.Ranged),
                                               Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(Common.createContextActionRemoveBuff(ally_buff)),
                                                                                             only_hit: false, on_initiator: true),
                                               Helpers.CreateAddFact(attack),
                                               Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 6),
                                               Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("0da0c194d6e1d43419eb8d990b28e0ab"))
                                           );

            library.AddCombatFeats(target_of_opportunity);
            Common.addTemworkFeats(target_of_opportunity);
        }


        static void createDistractingCharge()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"FeatIcons/DistractingCharge.png");
            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("7ced0efa297bd5142ab749f6e33b112b", "AuraDistractingChargeArea", "");

            area.Size = 100.Feet();

            var buff = library.CopyAndAdd<BlueprintBuff>("c96380f6dcac83c45acdb698ae70ffc4", "AuraDistractingChargeBuff", "");
            buff.ReplaceComponent<AddAreaEffect>(a => a.AreaEffect = area);

            distracting_charge = library.CopyAndAdd<BlueprintFeature>("e45ab30f49215054e83b4ea12165409f", "DistractingChargeFeature", "");
            distracting_charge.SetName("Distracting Charge");
            distracting_charge.SetDescription("When your ally with this feat uses the charge action and hits, you gain a +2 bonus on your next attack roll against the target of that charge. This bonus must be used before your ally’s next turn, or it is lost.");
            distracting_charge.SetIcon(icon);
            distracting_charge.RemoveComponents<SpellImmunityToSpellDescriptor>();
            distracting_charge.RemoveComponents<BuffDescriptorImmunity>();
            distracting_charge.Groups = new FeatureGroup[] { FeatureGroup.Feat, FeatureGroup.CombatFeat, FeatureGroup.TeamworkFeat };
            distracting_charge.ReplaceComponent<AuraFeatureComponent>(a => a.Buff = buff);

            var target_buff = Helpers.CreateBuff("DistractingChargeTargetBuff",
                                             distracting_charge.Name + " Target",
                                             "",
                                             "",
                                             icon,
                                             null
                                             );

            var apply_target_buff = Common.createContextActionApplyBuff(target_buff, Helpers.CreateContextDuration(1), dispellable: false);
            var ally_buff = Helpers.CreateBuff("DistractingChargeAllyBuff",
                                             distracting_charge.Name,
                                             "",
                                             "",
                                             icon,
                                             null,
                                             Helpers.Create<AttackBonusAgainstFactOwner>(a => { a.AttackBonus = 2; a.CheckedFact = target_buff; })
                                             );
            //ally_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var apply_ally_buff = Common.createContextActionApplyBuff(ally_buff, Helpers.CreateContextDuration(1), dispellable: false);

            var effect_on_ally1 = Helpers.CreateConditional(new Condition[] { Helpers.Create<ContextConditionIsAlly>(), Helpers.Create<TeamworkMechanics.ContextConditionHasSoloTactics>(), Helpers.CreateConditionHasFact(distracting_charge) },
                                                            apply_ally_buff
                                                          );

            var effect_on_ally2 = Helpers.CreateConditional(new Condition[] { Helpers.Create<ContextConditionIsAlly>(), Helpers.CreateConditionHasFact(distracting_charge) },
                                                            apply_ally_buff,
                                                            null

                                              );

            var actions_on_ally1 = Helpers.Create<TeamworkMechanics.ContextActionOnUnitsWithinRadius>(c => { c.Radius = 100.Feet(); c.actions = Helpers.CreateActionList(effect_on_ally1); });
            var actions_on_ally2 = Helpers.CreateConditional(Helpers.CreateConditionCasterHasFact(distracting_charge),
                                                             Helpers.Create<TeamworkMechanics.ContextActionOnUnitsWithinRadius>(c => { c.Radius = 100.Feet(); c.actions = Helpers.CreateActionList(effect_on_ally2); })
                                                             );

            var attacker_buff = Helpers.CreateBuff("DistractingChargeAttackerBuff",
                                                   "DistractingChargeAttackerBuff",
                                                   "",
                                                   "",
                                                   null,
                                                   null,
                                                   Helpers.Create<NewMechanics.AddInitiatorAttackWithWeaponTriggerOnCharge>(a => a.Action = Helpers.CreateActionList(apply_target_buff, actions_on_ally1)),
                                                   Helpers.Create<NewMechanics.AddInitiatorAttackWithWeaponTriggerOnCharge>(a =>
                                                                                                                               {
                                                                                                                                   a.Action = Helpers.CreateActionList(Common.createContextActionRemoveBuff(ally_buff));
                                                                                                                                   a.ActionsOnInitiator = true;
                                                                                                                               })
                                                   );

            attacker_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            area.ReplaceComponent<AbilityAreaEffectBuff>(a => a.Buff = attacker_buff);
            distracting_charge.AddComponents(Helpers.Create<NewMechanics.AddInitiatorAttackWithWeaponTriggerOnCharge>(a => a.Action = Helpers.CreateActionList(actions_on_ally2)),
                                             Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(Common.createContextActionRemoveBuff(ally_buff)),
                                                                                             only_hit: false, on_initiator: true)
                                           );

            library.AddCombatFeats(distracting_charge);
            Common.addTemworkFeats(distracting_charge);
        }


        static void createFellingSmash()
        {
            var icon = library.Get<BlueprintAbility>("95851f6e85fe87d4190675db0419d112").Icon;
            var power_attack_buff = library.Get<BlueprintBuff>("5898bcf75a0942449a5dc16adc97b279");
            var power_attack_feature = library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5");
            var combat_expertise = library.Get<BlueprintFeature>("4c44724ffa8844f4d9bedb5bb27d144a");
            var improved_trip = library.Get<BlueprintFeature>("0f15c6f70d8fb2b49aa6cc24239cc5fa");

            var action = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(power_attack_buff),
                                                   Helpers.Create<ContextActionCombatManeuver>(c =>
                                                                                                 {
                                                                                                     c.Type = Kingmaker.RuleSystem.Rules.CombatManeuver.Trip;
                                                                                                     c.OnSuccess = Helpers.CreateActionList();
                                                                                                 }
                                                                                               )
                                                   );
            var buff = Helpers.CreateBuff("FellingSmashBuff",
                                          "Felling Smash",
                                          "If you use the attack action to make a single melee attack at your highest base attack bonus while using Power Attack and you hit an opponent, you can spend a swift action to attempt a trip combat maneuver against that opponent.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.Create<NewMechanics.ContextActionOnSingleMeleeAttack>(c =>
                                                                                                        { c.allowed_attack_types = new AttackType[] { AttackType.Melee, AttackType.Touch };
                                                                                                          c.action = Helpers.CreateActionList(action);
                                                                                                        })
                                          );

            var ability = Helpers.CreateActivatableAbility("FellingSmashActivatableAbility",
                                                           buff.Name,
                                                           buff.Description,
                                                           "",
                                                           buff.Icon,
                                                           buff,
                                                           AbilityActivationType.WithUnitCommand,
                                                           Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift,
                                                           null);
            if (!test_mode)
            {
                ability.AddComponent(Common.createActivatableAbilityUnitCommand(Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift));
            }

            felling_smash = Helpers.CreateFeature("FellingSmashFeature",
                                                  buff.Name,
                                                  buff.Description,
                                                  "",
                                                  null,
                                                  FeatureGroup.Feat,
                                                  Helpers.CreateAddFact(ability),
                                                  Helpers.PrerequisiteStatValue(StatType.Intelligence, 13),
                                                  Helpers.PrerequisiteStatValue(StatType.Strength, 13),
                                                  Helpers.PrerequisiteFeature(combat_expertise),
                                                  Helpers.PrerequisiteFeature(improved_trip),
                                                  Helpers.PrerequisiteFeature(power_attack_feature),
                                                  Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 6)
                                                  );


            felling_smash.Groups = felling_smash.Groups.AddToArray(FeatureGroup.CombatFeat);
            library.AddCombatFeats(felling_smash);
        }


        static void createDevastatingStrike()
        {
            devastating_strike = Helpers.CreateFeature("DevastatingStrikeFeature",
                                                       "Devastating Strike",
                                                       "Whenever you use Vital Strike, Improved Vital Strike, or Greater Vital Strike, you gain a +2 bonus on each extra weapon damage dice roll those feats grant (+6 maximum). This bonus damage is multiplied on a critical hit.",
                                                       "",
                                                       null,
                                                       FeatureGroup.Feat,
                                                       Common.createVitalStrikeScalingDamage(2),
                                                       Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 9),
                                                       Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("14a1fc1356df9f146900e1e42142fc9d")) //vital strike
                                                       );
            devastating_strike.Groups = devastating_strike.Groups.AddToArray(FeatureGroup.CombatFeat);
            library.AddCombatFeats(devastating_strike);
        }


        static void createStrikeTrue()
        {
            var true_strike = library.Get<BlueprintAbility>("2c38da66e5a599347ac95b3294acbe00");
            var buff = Helpers.CreateBuff("StrikeTrueBuff",
                                            "Strike True",
                                            "You can focus yourself as a move action. When focused, you gain a +4 bonus on your next melee attack roll before the end of your turn.",
                                            "",
                                            true_strike.Icon,
                                            null,
                                            Helpers.CreateAddStatBonus(StatType.AdditionalAttackBonus, 4, ModifierDescriptor.UntypedStackable),
                                            Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(Helpers.Create<ContextActionRemoveSelf>()), only_hit: false, on_initiator: true,
                                                                                             wait_for_attack_to_resolve: true)
                                          );
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), dispellable: false, duration_seconds: 4);
            var strike_true_ability = Helpers.CreateAbility("StrikeTrueFeature",
                                                            buff.Name,
                                                            buff.Description,
                                                            "",
                                                            buff.Icon,
                                                            AbilityType.Special,
                                                            Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Move,
                                                            AbilityRange.Personal,
                                                            "First Attack or until end of the round.",
                                                            "",
                                                            Helpers.CreateRunActions(apply_buff)
                                                            );
            strike_true_ability.setMiscAbilityParametersSelfOnly(animation: Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Special);
            strike_true = Common.AbilityToFeature(strike_true_ability, false);
            strike_true.Groups = new FeatureGroup[] { FeatureGroup.Feat, FeatureGroup.CombatFeat };
            strike_true.SetIcon(null);
            strike_true.AddComponents(Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("4c44724ffa8844f4d9bedb5bb27d144a")),//combat expertise
                                      Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 6)
                                     );
            library.AddCombatFeats(strike_true);
        }





        static void createGreaterWeaponOfTheChosen()
        {
            greater_weapon_of_the_chosen = Helpers.CreateFeature("GreaterWeaponChosenFeature",
                                                                 "Greater Weapon of the Chosen",
                                                                 "When you use your deity’s favored weapon to attempt a single attack with the attack action, you roll two dice for your attack roll and take the higher result. You do not need to use your Weapon of the Chosen feat to gain this feat’s benefit. As usual, the reroll does not apply to any confirmation rolls.",
                                                                 "",
                                                                 null,
                                                                 FeatureGroup.Feat,
                                                                 Helpers.Create<NewMechanics.RerollOnStandardSingleAttack>(r => r.required_features = new BlueprintParametrizedFeature[] { NewFeats.deity_favored_weapon }),
                                                                 Helpers.PrerequisiteFeature(NewFeats.improved_weapon_of_the_chosen)
                                                                 );

            greater_weapon_of_the_chosen.Groups = greater_weapon_of_the_chosen.Groups.AddToArray(FeatureGroup.CombatFeat);
            library.AddCombatFeats(greater_weapon_of_the_chosen);
        }


        static void createWeaponOfTheChosen()
        {
            string[] icon_names = new string[] { @"AbilityIcons/WeaponGood.png",
                                                 @"AbilityIcons/WeaponEvil.png",
                                                 @"AbilityIcons/WeaponLawful.png",
                                                 @"AbilityIcons/WeaponChaotic.png",
                                                 @"AbilityIcons/WeaponSilver.png"};

            string[] names = new string[] { "Good", "Evil", "Lawful", "Chaotic", "ColdIronSilver" };
            string[] display_names = new string[] { "Good", "Evil", "Lawful", "Chaotic", "Cold Iron and Silver" };
            BlueprintComponent[] weapon_components = new BlueprintComponent[]
            {
                Common.createAddOutgoingAlignment(Kingmaker.Enums.Damage.DamageAlignment.Good),
                Common.createAddOutgoingAlignment(Kingmaker.Enums.Damage.DamageAlignment.Evil),
                Common.createAddOutgoingAlignment(Kingmaker.Enums.Damage.DamageAlignment.Lawful),
                Common.createAddOutgoingAlignment(Kingmaker.Enums.Damage.DamageAlignment.Chaotic),
                Common.createAddOutgoingMaterial(Kingmaker.Enums.Damage.PhysicalDamageMaterial.ColdIron | Kingmaker.Enums.Damage.PhysicalDamageMaterial.Silver)
            };

            RestrictionHasFact[] restrictions = new RestrictionHasFact[]
            {
                Common.createActivatableAbilityRestrictionHasFact(library.Get<BlueprintFeature>("882521af8012fc749930b03dc18a69de")),//good
                Common.createActivatableAbilityRestrictionHasFact(library.Get<BlueprintFeature>("351235ac5fc2b7e47801f63d117b656c")),//evil
                Common.createActivatableAbilityRestrictionHasFact(library.Get<BlueprintFeature>("092714336606cfc45a37d2ab39fabfa8")),//law
                Common.createActivatableAbilityRestrictionHasFact(library.Get<BlueprintFeature>("8c7d778bc39fec642befc1435b00f613")),//chaos
            };

            BlueprintBuff[] improved_buffs = new BlueprintBuff[5];
            BlueprintBuff[] improved_effect_buffs = new BlueprintBuff[5];
            BlueprintActivatableAbility[] abilities = new BlueprintActivatableAbility[5];

            var improved_description = "This feat acts as Weapon of the Chosen, except you gain the benefits on all attacks until the start of your next turn. Your attacks gain a single alignment component of your deity—either chaotic, evil, good, or lawful—for the purpose of overcoming damage reduction. If your deity is neutral with no other alignment components, your attacks instead overcome damage reduction as though your weapon were both cold iron and silver.";
            for (int i = 0; i < abilities.Length; i++)
            {
                improved_buffs[i] = Helpers.CreateBuff($"ImprovedWeaponOfChosen{names[i]}Buff",
                                                       $"Improved Weapon of the Chosen: {display_names[i]}",
                                                       improved_description,
                                                       "",
                                                       LoadIcons.Image2Sprite.Create(icon_names[i]),
                                                       null);
                improved_buffs[i].SetBuffFlags(BuffFlags.HiddenInUi);

                improved_effect_buffs[i] = Helpers.CreateBuff($"ImprovedWeaponOfChosen{names[i]}EffectBuff",
                                                                improved_buffs[i].Name,
                                                                improved_buffs[i].Description,
                                                                "",
                                                                improved_buffs[i].Icon,
                                                                null,
                                                                weapon_components[i]
                                                              );

                abilities[i] = Helpers.CreateActivatableAbility($"ImprovedWeaponOfChosen{names[i]}Ability",
                                                                improved_buffs[i].Name,
                                                                improved_buffs[i].Description,
                                                                "",
                                                                improved_buffs[i].Icon,
                                                                improved_buffs[i],
                                                                AbilityActivationType.Immediately,
                                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                                null);
                if (i < 4)
                {
                    abilities[i].AddComponent(restrictions[i]);
                }
                else
                {
                    foreach (var r in restrictions)
                    {
                        var not_restriction = r.CreateCopy();
                        abilities[i].AddComponent(r.CreateCopy(c => c.Not = true));
                    }
                }
                abilities[i].DeactivateImmediately = true;
                abilities[i].Group = ActivatableAbilityGroupExtension.ImprovedWeaponOfChosen.ToActivatableAbilityGroup();
            }

            improved_weapon_of_the_chosen = Helpers.CreateFeature("ImprovedWeaponChosenFeature",
                                                                  "Improved Weapon of the Chosen",
                                                                  improved_description,
                                                                  "",
                                                                  null,
                                                                  FeatureGroup.CombatFeat,
                                                                  Helpers.CreateAddFacts(abilities));
            improved_weapon_of_the_chosen.Groups = improved_weapon_of_the_chosen.Groups.AddToArray(FeatureGroup.Feat);

            var remove_action = Helpers.CreateConditional(Helpers.CreateConditionCasterHasFact(improved_weapon_of_the_chosen, not: true),
                                                                                    Helpers.Create<ContextActionRemoveSelf>()
                                                         );
                                                         
            BlueprintBuff weapon_of_choosen_buff = Helpers.CreateBuff("WeaponOfChosentBuff",
                                                                      "Weapon of the Chosen",
                                                                       "As a swift action, you can call upon your deity to guide an attack you make with your deity’s favored weapon. On your next attack in that round with that weapon, your weapon counts as magical for the purpose of overcoming damage reduction or striking an incorporeal creature. If your attack misses because of concealment, you can reroll your miss chance one time to see whether you actually hit.",
                                                                       "",
                                                                       LoadIcons.Image2Sprite.Create(@"AbilityIcons/WeaponMagic.png"),
                                                                       null,
                                                                       Helpers.Create<RerollConcealment>(),
                                                                       Common.createAddOutgoingMagic(),
                                                                       Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(remove_action))
                                                                     );
            for (int i = 0; i < improved_buffs.Length; i++)
            {
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(weapon_of_choosen_buff, improved_effect_buffs[i], improved_buffs[i]);
            }

            var apply_buff = Common.createContextActionApplyBuff(weapon_of_choosen_buff, Helpers.CreateContextDuration(1), dispellable: false);
            var weapon_of_choosen_ability = Helpers.CreateAbility("WeaponOfTheChosenAbility",
                                                                  weapon_of_choosen_buff.Name,
                                                                  weapon_of_choosen_buff.Description,
                                                                  "",
                                                                  weapon_of_choosen_buff.Icon,
                                                                  AbilityType.Supernatural,
                                                                  Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift,
                                                                  AbilityRange.Personal,
                                                                  "First Attack",
                                                                  "",
                                                                  Helpers.CreateRunActions(apply_buff),
                                                                  Helpers.Create<NewMechanics.AbilityCasterMainWeaponCheckHasParametrizedFeature>(a => a.feature = deity_favored_weapon)
                                                                  );
            weapon_of_choosen_ability.setMiscAbilityParametersSelfOnly();

            weapon_of_the_chosen = Common.AbilityToFeature(weapon_of_choosen_ability, false);
            weapon_of_the_chosen.SetIcon(null);
            weapon_of_the_chosen.Groups = new FeatureGroup[] { FeatureGroup.Feat, FeatureGroup.CombatFeat };

            var weapon_focus = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");

            weapon_of_the_chosen.AddComponent(Helpers.Create<NewMechanics.PrerequisiteMatchingParamtrizedFeature>(p =>
                                                                                                                  {
                                                                                                                      p.base_feature = NewFeats.deity_favored_weapon;
                                                                                                                      p.matching_feature = weapon_focus;
                                                                                                                  }
                                                                                                                  )
                                             );
            weapon_of_the_chosen.AddComponent(Common.createPrerequisiteCasterTypeSpellLevel(false, 1));

            improved_weapon_of_the_chosen.AddComponent(Helpers.PrerequisiteFeature(weapon_of_the_chosen));

            library.AddCombatFeats(weapon_of_the_chosen, improved_weapon_of_the_chosen);
        }


        static void createDiscordantVoice()
        {
            var bard = library.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");
            var sound_burst = library.Get<BlueprintAbility>("c3893092a333b93499fd0a21845aa265");
            var deal_dmg = Helpers.CreateActionList(Helpers.CreateActionDealDamage(Kingmaker.Enums.Damage.DamageEnergyType.Sonic, Helpers.CreateContextDiceValue(DiceType.D6, 1), IgnoreCritical: true));
            var discordant_voice_effect_buff = Helpers.CreateBuff("DiscordantVoiceEffectBuff",
                                                                   "Discordant Voice",
                                                                   "Whenever you are using bardic performance to create a spell-like or supernatural effect, allies within 30 feet of you deal an extra 1d6 points of sonic damage with successful weapon attacks. This damage stacks with other energy damage a weapon might deal. Projectile weapons bestow this extra damage on their ammunition, but the extra damage is dealt only if the projectile hits a target within 30 feet of you.",
                                                                   "",
                                                                   sound_burst.Icon,
                                                                   null,
                                                                   Common.createAddTargetAttackWithWeaponTrigger(deal_dmg, null, not_reach: false, only_melee: false, wait_for_attack_to_resolve: true)
                                                                   );

            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("4a15b95f8e173dc4fb56924fe5598dcf", "DiscordantVoiceArea", ""); //dirge of doom
            area.Fx = new Kingmaker.ResourceLinks.PrefabLink();
            area.ReplaceComponent<AbilityAreaEffectBuff>(a => a.Buff = discordant_voice_effect_buff);
            if (test_mode)
            {
                area.ReplaceComponent<AbilityAreaEffectBuff>(a => a.Condition = Helpers.CreateConditionsCheckerOr());
            }

            var discordant_voice_buff = library.CopyAndAdd<BlueprintBuff>("83eab9b139717ad478d84bbf48ab457f", "DiscordantVoiceBuff", "");
            discordant_voice_buff.SetNameDescriptionIcon(discordant_voice_effect_buff.Name, discordant_voice_effect_buff.Description, discordant_voice_effect_buff.Icon);
            discordant_voice_buff.ReplaceComponent<AddAreaEffect>(a => a.AreaEffect = area);
            discordant_voice_buff.FxOnStart = new Kingmaker.ResourceLinks.PrefabLink();

            var monk = library.Get<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");
            var sensei = library.Get<BlueprintArchetype>("f8767821ec805bf479706392fcc3394c");
            discordant_voice = Helpers.CreateFeature("DiscordantVocieFeature",
                                                     discordant_voice_effect_buff.Name,
                                                     discordant_voice_effect_buff.Description,
                                                     "",
                                                     discordant_voice_effect_buff.Icon,
                                                     FeatureGroup.Feat,
                                                     Helpers.PrerequisiteClassLevel(bard, 8, any: true),
                                                     Helpers.PrerequisiteClassLevel(Skald.skald_class, 8, any: true),
                                                     Common.createPrerequisiteArchetypeLevel(monk, sensei, 8, any: true),
                                                     Common.createPrerequisiteArchetypeLevel(Archetypes.Evangelist.archetype.GetParentClass(), Archetypes.Evangelist.archetype, 8, any: true)
                                                     );
            library.AddFeats(discordant_voice);
            var performances = library.GetAllBlueprints().OfType<BlueprintActivatableAbility>().Where(a => a.Group == ActivatableAbilityGroup.BardicPerformance);


            foreach (var p in performances)
            {
                var b = p.Buff;
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(b, discordant_voice_buff, discordant_voice);
            }
        }


        static void createPowerfulShape()
        {
            var druid = library.Get<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");
            powerful_shape = Helpers.CreateFeature("PowerfulShapeFeature",
                                                    "Powerful Shape",
                                                    "When in wild shape, treat your size as one category larger for the purpose of calculating CMB, CMD, and any size-based special attacks you use or that are used against you.",
                                                    "",
                                                    null,
                                                    FeatureGroup.Feat,
                                                    Helpers.PrerequisiteFeature(Wildshape.first_wildshape_form),
                                                    Helpers.PrerequisiteClassLevel(druid, 8)
                                                    );
            library.AddFeats(powerful_shape);

            var powerful_shape_buff = Helpers.CreateBuff("PowerfulShapeBuff",
                                        powerful_shape.Name,
                                        powerful_shape.Description,
                                        "",
                                        null,
                                        null,
                                        Helpers.Create<CombatManeuverMechanics.FakeSizeBonus>(f => f.bonus = 1)
                                        );

            powerful_shape_buff.SetBuffFlags(BuffFlags.HiddenInUi);


            foreach (var wb in Wildshape.druid_wild_shapes)
            {
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(wb, powerful_shape_buff, powerful_shape);
            }
        }


        static void createExtraChannelPaladin()
        {
            var paladin_extra_channel_resource = Helpers.CreateAbilityResource("PaladinExtraChannelResource", "", "", "", null);
            paladin_extra_channel_resource.SetFixedResource(0);

            var paladin_channel_energy = library.Get<BlueprintFeature>("cb6d55dda5ab906459d18a435994a760");
            var paladin_heal = library.Get<BlueprintAbility>("6670f0f21a1d7f04db2b8b115e8e6abf");
            var paladin_harm = library.Get<BlueprintAbility>("4937473d1cfd7774a979b625fb833b47");

            BlueprintAbility paladin_harm_base = paladin_channel_energy.GetComponent<AddFacts>().Facts[0] as BlueprintAbility;
            BlueprintAbility paladin_heal_base = paladin_channel_energy.GetComponent<AddFacts>().Facts[1] as BlueprintAbility;

            var heal_living_extra = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHeal,
                                                          "PaladinChannelEnergyHealLivingExtra",
                                                          paladin_heal.Name + " (Extra)",
                                                          paladin_heal.Description,
                                                          "",
                                                          paladin_heal.GetComponent<ContextRankConfig>(),
                                                          paladin_heal.GetComponent<NewMechanics.ContextCalculateAbilityParamsBasedOnClasses>(),
                                                          Helpers.CreateResourceLogic(paladin_extra_channel_resource, true, 1));

            var harm_undead_extra = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHarm,
                                              "PaladinChannelEnergyHarmUndeadExtra",
                                              paladin_harm.Name + " (Extra)",
                                              paladin_harm.Description,
                                              "",
                                              paladin_harm.GetComponent<ContextRankConfig>(),
                                              paladin_harm.GetComponent<NewMechanics.ContextCalculateAbilityParamsBasedOnClasses>(),
                                              Helpers.CreateResourceLogic(paladin_extra_channel_resource, true, 1));
            heal_living_extra.AddComponent(Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.LawfulGood));
            harm_undead_extra.AddComponent(Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.LawfulGood));

            paladin_heal_base.addToAbilityVariants(heal_living_extra);
            paladin_harm_base.addToAbilityVariants(harm_undead_extra);
            ChannelEnergyEngine.storeChannel(heal_living_extra, paladin_channel_energy, ChannelEnergyEngine.ChannelType.PositiveHeal);
            ChannelEnergyEngine.storeChannel(harm_undead_extra, paladin_channel_energy, ChannelEnergyEngine.ChannelType.PositiveHarm);

            paladin_channel_energy.AddComponent(Helpers.CreateAddAbilityResource(paladin_extra_channel_resource));
            paladin_channel_extra = ChannelEnergyEngine.createExtraChannelFeat(heal_living_extra, paladin_channel_energy, "ExtraChannelPaladin", "Extra Channel (Paladin)", "");
        }


        static void createDeadeyesBlessing()
        {
            var weapon_focus = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");
            deadeyes_blessing = Helpers.CreateFeature("DeadeyesBlessingFeature",
                                                "Deadeye’s Blessing",
                                                "You can use your Wisdom modifier instead of your Dexterity modifier on ranged attack rolls when using a bow.",
                                                "",
                                                null,
                                                FeatureGroup.Feat,
                                                Helpers.Create<NewMechanics.AttackStatReplacementForWeaponCategory>(c =>
                                                                                                                    {
                                                                                                                        c.categories = new WeaponCategory[] { WeaponCategory.Longbow, WeaponCategory.Shortbow};
                                                                                                                        c.ReplacementStat = StatType.Wisdom;
                                                                                                                    }
                                                                                                                   ),
                                                Common.createPrerequisiteParametrizedFeatureWeapon(deity_favored_weapon, WeaponCategory.Longbow),
                                                Common.createPrerequisiteParametrizedFeatureWeapon(weapon_focus, WeaponCategory.Longbow)
                                                );
            deadeyes_blessing.Groups = deadeyes_blessing.Groups.AddToArray(FeatureGroup.CombatFeat);
            library.AddCombatFeats(deadeyes_blessing);
        }



        static void createGuidedHand()
        {
            guided_hand = Helpers.CreateFeature("GuidedHandFeature",
                                                "Guided Hand",
                                                "With your deity’s favored weapon, you can use your Wisdom modifier instead of your Strength or Dexterity modifier on attack rolls.",
                                                "",
                                                null,
                                                FeatureGroup.Feat,
                                                Helpers.Create<NewMechanics.AttackStatReplacementIfHasParametrizedFeature>(c =>
                                                                                                                            {
                                                                                                                                c.feature = deity_favored_weapon;
                                                                                                                                c.ReplacementStat = StatType.Wisdom;
                                                                                                                            }
                                                                                                                            ),
                                                Helpers.PrerequisiteFeature(ChannelEnergyEngine.channel_smite)
                                                );
            library.AddFeats(guided_hand);
        }

        internal static void createDeityFavoredWeapon()
        {
            deity_favored_weapon = library.CopyAndAdd<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e", "DeityFavoredWeapon", "");
            deity_favored_weapon.SetName("Deity's Favored Weapon");
            deity_favored_weapon.SetDescription("");
            deity_favored_weapon.Groups = new FeatureGroup[0];
            deity_favored_weapon.ComponentsArray = new BlueprintComponent[0];

            var deity_selection = library.Get<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4");

            foreach (var d in deity_selection.AllFeatures)
            {
                var add_features = d.GetComponents<AddFeatureOnClassLevel>();
                var starting_items = d.GetComponent<AddStartingEquipment>();
                if (add_features.Count() == 0 && starting_items!= null)
                {
                    var weapon_category = starting_items.CategoryItems[0];
                    d.AddComponent(Common.createAddParametrizedFeatures(deity_favored_weapon, weapon_category));
                }
                foreach (var add_feature in add_features)
                {
                    var proficiency = add_feature.Feature.GetComponent<AddProficiencies>();
                    var weapon_category = proficiency == null ? WeaponCategory.UnarmedStrike : proficiency.WeaponProficiencies[0];
                    d.AddComponent(Common.createAddParametrizedFeatures(deity_favored_weapon, weapon_category));
                }
            }
        }


        static void createPlanarWildShape()
        {

            var celestial_wildshape_buff = Helpers.CreateBuff("PlanarWildshapeCelestialBuff",
                                                                "Celestial Wild Shape",
                                                                "When you use wild shape to take the form of an animal, you can expend an additional daily use of your wild shape class feature to add the celestial template to your animal form.\n"
                                                                + Hunter.celestial_template.Description
                                                                + "\nIn addition you receive +2 bonus to confirm critical hits against evil creatures.",
                                                                "",
                                                                Hunter.celestial_template.Icon,
                                                                null);

            var fiendish_wildshape_buff = Helpers.CreateBuff("PlanarWildshapeFiendishBuff",
                                                    "Fiendish Wild Shape",
                                                    "When you use wild shape to take the form of an animal, you can expend an additional daily use of your wild shape class feature to add the fiendish template to your animal form.\n"
                                                    + Hunter.fiendish_template.Description
                                                    + "\nIn addition you receive +2 bonus to confirm critical hits against good creatures.",
                                                    "",
                                                    Hunter.fiendish_template.Icon,
                                                    null);

            var celestial_wildshape_effect_buff = Helpers.CreateBuff("PlanarWildshapeCelestialEffectBuff",
                                                        "",
                                                        "",
                                                        "",
                                                        null,
                                                        null,
                                                        Helpers.CreateAddFact(Hunter.celestial_template),
                                                        Helpers.Create<NewMechanics.CriticalConfirmationBonusAgainstALignment>(c =>
                                                                                                                                {
                                                                                                                                    c.Value = Common.createSimpleContextValue(2);
                                                                                                                                    c.EnemyAlignment = AlignmentComponent.Evil;
                                                                                                                                }
                                                                                                                                )
                                                        );
            celestial_wildshape_effect_buff.SetBuffFlags(BuffFlags.HiddenInUi);


            var fiendish_wildshape_effect_buff = Helpers.CreateBuff("PlanarWildshapeFiendishEffectBuff",
                                                                    "",
                                                                    "",
                                                                    "",
                                                                    null,
                                                                    null,
                                                                    Helpers.CreateAddFact(Hunter.fiendish_template),
                                                                    Helpers.Create<NewMechanics.CriticalConfirmationBonusAgainstALignment>(c =>
                                                                                                                                            {
                                                                                                                                                c.Value = Common.createSimpleContextValue(2);
                                                                                                                                                c.EnemyAlignment = AlignmentComponent.Good;
                                                                                                                                            }
                                                                                                                                            )
                                                                    );
            fiendish_wildshape_effect_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var celestial_wildshape_ability = Helpers.CreateActivatableAbility("PlanarWildshapeCelestialActivatableAbility",
                                                                       celestial_wildshape_buff.Name,
                                                                       celestial_wildshape_buff.Description,
                                                                       "",
                                                                       celestial_wildshape_buff.Icon,
                                                                       celestial_wildshape_buff,
                                                                       AbilityActivationType.Immediately,
                                                                       Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                                       null,
                                                                       Helpers.Create<NewMechanics.ActivatableAbilityNoAlignmentRestriction>(c => c.Alignment = AlignmentMaskType.Evil)
                                                                       );
            celestial_wildshape_ability.DeactivateImmediately = true;
            celestial_wildshape_ability.Group = ActivatableAbilityGroupExtension.PlanarWildshape.ToActivatableAbilityGroup();

            var fiendish_wildshape_ability = Helpers.CreateActivatableAbility("PlanarWildshapeFiendishActivatableAbility",
                                                           fiendish_wildshape_buff.Name,
                                                           fiendish_wildshape_buff.Description,
                                                           "",
                                                           fiendish_wildshape_buff.Icon,
                                                           fiendish_wildshape_buff,
                                                           AbilityActivationType.Immediately,
                                                           Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                           null,
                                                           Helpers.Create<NewMechanics.ActivatableAbilityNoAlignmentRestriction>(c => c.Alignment = AlignmentMaskType.Good));
            fiendish_wildshape_ability.DeactivateImmediately = true;
            fiendish_wildshape_ability.Group = ActivatableAbilityGroupExtension.PlanarWildshape.ToActivatableAbilityGroup();


            foreach (var wildshape in Wildshape.animal_wildshapes)
            {
                var buff =  ((ContextActionApplyBuff)wildshape.GetComponent<AbilityEffectRunAction>().Actions.Actions[0]).Buff;
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(buff, celestial_wildshape_effect_buff, celestial_wildshape_buff);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(buff, fiendish_wildshape_effect_buff, fiendish_wildshape_buff);

                wildshape.ReplaceComponent<AbilityResourceLogic>(a =>
                                                                 {
                                                                     a.ResourceCostIncreasingFacts.Add(celestial_wildshape_buff);
                                                                     a.ResourceCostIncreasingFacts.Add(fiendish_wildshape_buff);
                                                                 }
                                                                 );

            }

            planar_wild_shape = Helpers.CreateFeature("PlanarWildShapeFeature",
                                                      "Planar Wild Shape",
                                                      "When you use wild shape to take the form of an animal, you can expend an additional daily use of your wild shape class feature to add the celestial template or fiendish template to your animal form. (Good druids must use the celestial template, while evil druids must use the fiendish template.) If your form has the celestial template and you score a critical threat against an evil creature while using your form’s natural weapons, you gain a +2 bonus on the attack roll to confirm the critical hit. The same bonus applies if your form has the fiendish template and you score a critical threat against a good creature.",
                                                      "",
                                                      null,
                                                      FeatureGroup.None,
                                                      Helpers.CreateAddFacts(celestial_wildshape_ability, fiendish_wildshape_ability),
                                                      Helpers.PrerequisiteStatValue(StatType.SkillLoreReligion, 5),
                                                      Helpers.PrerequisiteFeature(Wildshape.first_wildshape_form)
                                                      );

            library.AddFeats(planar_wild_shape);
        }


        static void replaceIconsForExistingFeats()
        {
            var arcane_strike_feature = library.Get<BlueprintFeature>("0ab2f21a922feee4dab116238e3150b4");
            arcane_strike_feature.SetIcon(LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Arcane_Strike.png"));
            var arcane_strike_ability = arcane_strike_feature.GetComponent<AddFacts>().Facts[0] as BlueprintActivatableAbility;
            arcane_strike_ability.SetIcon(arcane_strike_feature.Icon);
            arcane_strike_ability.Buff.SetIcon(arcane_strike_feature.Icon);

            var wings_feat = library.Get<BlueprintFeature>("d9bd0fde6deb2e44a93268f2dfb3e169");
            wings_feat.SetIcon(LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Wings.png"));

            var combat_casting = library.Get<BlueprintFeature>("06964d468fde1dc4aa71a92ea04d930d");
            combat_casting.SetIcon(LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Casting_Combat.png"));

            string[] extra_channel_ids = new string[] {"cd9f19775bd9d3343a31a065e93f0c47",
                                                         "347e8d794c9598e4abed70adda868ccd",
                                                         "8d4f82fdb4d09b247ae8cd1ae7ce02de"};

            foreach (var id in extra_channel_ids)
            {
                var extra_channel = library.Get<BlueprintFeature>(id);
                extra_channel.SetIcon(LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Channel_Extra.png"));
            }


        }


        static void createFuriousFocus()
        {
            var arcane_strike_feature = library.Get<BlueprintFeature>("0ab2f21a922feee4dab116238e3150b4");
            var power_attack_buff = library.Get<BlueprintBuff>("5898bcf75a0942449a5dc16adc97b279");
            var power_attack_feature = library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5");

            furious_focus = Helpers.CreateFeature("FuriousFocusFeature",
                                                  "Furious Focus",
                                                  "When you are wielding a two-handed weapon or a one-handed weapon with two hands, and using the Power Attack feat, you do not suffer Power Attack’s penalty on melee attack rolls on the first attack you make each turn. You still suffer the penalty on any additional attacks, including attacks of opportunity.",
                                                  "",
                                                  arcane_strike_feature.Icon.CreateCopy(),
                                                  FeatureGroup.CombatFeat,
                                                  Helpers.Create<NewMechanics.AttackBonusOnAttackInitiationIfHasFact>(a =>
                                                                                                                      {
                                                                                                                          a.Bonus = Helpers.CreateContextValue(AbilityRankType.StatBonus);
                                                                                                                          a.CheckedFact = power_attack_buff;
                                                                                                                          a.OnlyFirstAttack = true;
                                                                                                                          a.OnlyTwoHanded = true;
                                                                                                                          a.WeaponAttackTypes = new AttackType[] {AttackType.Melee };
                                                                                                                      }),
                                                  Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.BaseAttack, progression: ContextRankProgression.OnePlusDivStep,
                                                                                  type: AbilityRankType.StatBonus, stepLevel: 4),
                                                  Helpers.PrerequisiteStatValue(StatType.Strength, 13),
                                                  Helpers.PrerequisiteFeature(power_attack_feature),
                                                  Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 1)
                                                  );


            furious_focus.Groups = furious_focus.Groups.AddToArray(FeatureGroup.Feat);
            library.AddCombatFeats(furious_focus);

            BladeTutor.RuleCalculateAttackBonusWithoutTarget_OnTrigger_Patch.facts.Add(furious_focus);
        }

        static void createRagingBrutality()
        {
            //var destructive_smite = library.Get<BlueprintActivatableAbility>("e69898f762453514780eb5e467694bdb");
            var power_attack_buff = library.Get<BlueprintBuff>("5898bcf75a0942449a5dc16adc97b279");
            var rage_resource = library.Get<BlueprintAbilityResource>("24353fcf8096ea54684a72bf58dedbc9");
            var power_attack_feature = library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5");
            var rage_feature = library.Get<BlueprintFeature>("2479395977cfeeb46b482bc3385f4647");
            var buff = Helpers.CreateBuff("RagingBrutalityBuff",
                                          "Raging Brutality",
                                          "While raging and using Power Attack, you can spend 3 additional rounds of your rage as a swift action to add your Constitution bonus on damage rolls for melee attacks or thrown weapon attacks you make on your turn. If you are using the weapon two-handed, instead add 1-1/2 times your Constitution bonus.",
                                          "",
                                          LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Raging_Brutality.png"), //destructive_smite.Icon,
                                          null,
                                          Common.createContextWeaponDamageBonus(Helpers.CreateContextValue(AbilityRankType.DamageBonus)),
                                          Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Constitution, type: AbilityRankType.DamageBonus));

            var ability = Helpers.CreateAbility("RagingBrutalityAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift,
                                                AbilityRange.Personal,
                                                Helpers.oneRoundDuration,
                                                Helpers.savingThrowNone,
                                                Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff,
                                                                                                             Helpers.CreateContextDuration(Common.createSimpleContextValue(1), rate: DurationRate.Rounds),
                                                                                                             dispellable: false
                                                                                                             )
                                                                        ),
                                                Common.createAbilityCasterHasFacts(NewRagePowers.rage_marker_caster),
                                                Common.createAbilityCasterHasFacts(power_attack_buff),
                                                Helpers.CreateResourceLogic(rage_resource, amount: 3)
                                                );

            raging_brutality = Helpers.CreateFeature("RagingBrutalityFeature",
                                                      buff.Name,
                                                      buff.Description,
                                                      "",
                                                      buff.Icon,
                                                      FeatureGroup.Feat,
                                                      Helpers.CreateAddFact(ability),
                                                      Helpers.PrerequisiteStatValue(StatType.Strength, 13),
                                                      Helpers.PrerequisiteFeature(power_attack_feature),
                                                      Helpers.PrerequisiteFeature(rage_feature, any: true),
                                                      Helpers.PrerequisiteFeature(Bloodrager.bloodrage, any: true),
                                                      Helpers.PrerequisiteFeature(Bloodrager.urban_bloodrage, any: true),
                                                      Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 12)
                                                      );
            library.AddFeats(raging_brutality);
        }


        static void createBloodedArcaneStrike()
        {
            var arcane_strike_buff = library.Get<BlueprintBuff>("98ac795afd1b2014eb9fdf2b9820808f");
            var arcane_strike_feature = library.Get<BlueprintFeature>("0ab2f21a922feee4dab116238e3150b4");
            var arcane_strike_bonus = arcane_strike_buff.GetComponent<AddContextStatBonus>();
            var vital_strike_buff = Helpers.CreateBuff("BloodedArcaneStrikeVitalStrikeBuff",
                                                       "",
                                                       "",
                                                       "",
                                                       null,
                                                       null,
                                                       Common.createVitalStrikeScalingDamage(arcane_strike_bonus.Value, arcane_strike_bonus.Multiplier),
                                                       arcane_strike_buff.GetComponent<ContextRankConfig>()
                                                       );
            vital_strike_buff.SetBuffFlags(arcane_strike_buff.GetBuffFlags() | BuffFlags.HiddenInUi);

            blooded_arcane_strike = Helpers.CreateFeature("BloodedArcaneStrikeFeature",
                                                          "Blooded Arcane Strike",
                                                          "While you are bloodraging, you don’t need to spend a swift action to use your Arcane Strike—it is always in effect. When you use this ability with Vital Strike, Improved Vital Strike, or Greater Vital Strike, the bonus on damage rolls for Arcane Strike is multiplied by the number of times (two, three, or four) you roll damage dice for one of those feats.",
                                                          "",
                                                          LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Arcane_Blooded_Strike.png"), //arcane_strike_feature.Icon
                                                          FeatureGroup.CombatFeat,
                                                          Helpers.PrerequisiteFeature(arcane_strike_feature),
                                                          Helpers.PrerequisiteClassLevel(Bloodrager.bloodrager_class, 1)
                                                          );

            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(arcane_strike_buff, vital_strike_buff, blooded_arcane_strike);
            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(Bloodrager.bloodrage_buff, arcane_strike_buff, blooded_arcane_strike);
            //not needed since addFactContextActions is shared between bloodrage buffs
            /*foreach (var b in Bloodrager.urban_bloodrage_buffs)
            {
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(b, arcane_strike_buff, blooded_arcane_strike);
            }*/

            blooded_arcane_strike.Groups = blooded_arcane_strike.Groups.AddToArray(FeatureGroup.Feat);
            library.AddCombatFeats(blooded_arcane_strike);           
        }


        static void createRivingStrike()
        {
            var arcane_strike_buff = library.Get<BlueprintBuff>("98ac795afd1b2014eb9fdf2b9820808f");
            var arcane_strike_feature = library.Get<BlueprintFeature>("0ab2f21a922feee4dab116238e3150b4");

            var debuff = Helpers.CreateBuff("RivingStrikeEnemyBuff",
                                            "Riving Strike Penalty",
                                            "Target receives -2 penalty to saving throws against spells and spell-like abilities",
                                            "",
                                            LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Arcane_Riving_Strike.png"), //arcane_strike_feature.Icon,
                                            null,
                                            Common.createSavingThrowBonusAgainstAbilityType(-2, Common.createSimpleContextValue(0), AbilityType.Spell),
                                            Common.createSavingThrowBonusAgainstAbilityType(-2, Common.createSimpleContextValue(0), AbilityType.SpellLike)
                                            );
            debuff.Stacking = StackingType.Replace;

            var debuff_action = Common.createContextActionApplyBuff(debuff,
                                                             Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Rounds),
                                                             dispellable: false
                                                             );
            var buff = Helpers.CreateBuff("RivingStrikeBuff",
                                          "Riving Strike",
                                          "If you have a weapon that is augmented by your Arcane Strike feat, when you damage a creature with an attack made with that weapon, that creature takes a –2 penalty on saving throws against spells and spell-like abilities. This effect lasts for 1 round.",
                                          "",
                                          debuff.Icon,
                                          null,
                                          Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(debuff_action))
                                          );

            riving_strike = Helpers.CreateFeature("RivingStrikeFeature",
                                                  buff.Name,
                                                  buff.Description,
                                                  "",
                                                  buff.Icon,
                                                  FeatureGroup.CombatFeat,
                                                  Helpers.PrerequisiteFeature(arcane_strike_feature));
            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(arcane_strike_buff, buff, riving_strike);
            library.AddCombatFeats(riving_strike);
            riving_strike.Groups = riving_strike.Groups.AddToArray(FeatureGroup.Feat);
        }


        static void createCoordiantedShot()
        {
            var point_blank_shot = library.Get<BlueprintFeature>("0da0c194d6e1d43419eb8d990b28e0ab");
            coordinated_shot = Helpers.CreateFeature("CoordinatedShotFeature",
                                                     "Coordinated Shot",
                                                     "If your ally with this feat is threatening an opponent and is not providing cover to that opponent against your ranged attacks, you gain a +1 bonus on ranged attacks against that opponent. If your ally with this feat is flanking that opponent with another ally (even if that other ally doesn’t have this feat), this bonus increases to +2.",
                                                     "",
                                                     LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Shot_Coordinated.png"), //point_blank_shot.Icon,
                                                     FeatureGroup.Feat,
                                                     Helpers.PrerequisiteFeature(point_blank_shot));

            coordinated_shot.AddComponent(Helpers.Create<TeamworkMechanics.CoordinatedShotAttackBonus>(c =>
                                                                                                 {
                                                                                                     c.AttackBonus = 1;
                                                                                                     c.AdditionalFlankBonus = 1;
                                                                                                     c.CoordinatedShotFact = coordinated_shot;
                                                                                                 })
                                         );
            coordinated_shot.Groups = coordinated_shot.Groups.AddToArray(FeatureGroup.CombatFeat, FeatureGroup.TeamworkFeat);
            library.AddCombatFeats(coordinated_shot);
            Common.addTemworkFeats(coordinated_shot);
        }


        static void createStalwart()
        {
            var diehard = library.Get<BlueprintFeature>("86669ce8759f9d7478565db69b8c19ad");
            var endurance = library.Get<BlueprintFeature>("54ee847996c25cd4ba8773d7b8555174");
            var half_orc_ferocity = library.Get<BlueprintFeature>("c99f3405d1ef79049bd90678a666e1d7");

            //var resistance = library.Get<BlueprintBuff>("df680f6687f935e408eba6fb5124930e");
            stalwart = Helpers.CreateFeature("StalwartFeat",
                                                         "Stalwart",
                                                         "While using the total defense action, fighting defensively action, or Combat Expertise, you can forgo the dodge bonus to AC you would normally gain to instead gain an equivalent amount of DR, until the start of your next turn.",
                                                         "",
                                                         LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Stalwart.png"),
                                                         FeatureGroup.Feat,
                                                         Helpers.PrerequisiteFeature(endurance),
                                                         Helpers.PrerequisiteFeature(diehard, true),
                                                         Helpers.PrerequisiteFeature(half_orc_ferocity, true),
                                                         Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 4)
                                                         );

            var stalwart_toggle_buff = Helpers.CreateBuff("StalwartToggleBuff",
                                                          stalwart.Name,
                                                          stalwart.Description
                                                          + "\nNote: your DR bonus doubles if you have Improved Stalwart feat.",
                                                          "",
                                                          stalwart.Icon,
                                                          null);
            var stalwart_toggle = Helpers.CreateActivatableAbility("StalwartActivatableAbility",
                                                                   stalwart_toggle_buff.Name,
                                                                   stalwart_toggle_buff.Description,
                                                                   "",
                                                                   stalwart_toggle_buff.Icon,
                                                                   stalwart_toggle_buff,
                                                                   AbilityActivationType.Immediately,
                                                                   Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                                   null);
            stalwart_toggle.DeactivateImmediately = true;
            stalwart.AddComponent(Helpers.CreateAddFact(stalwart_toggle));

            improved_stalwart = Helpers.CreateFeature("ImprovedStalwartFeat",
                                             "Improved Stalwart",
                                             "Double the DR you gain from Stalwart",
                                             "",
                                             LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Stalwart_Improved.png"),
                                             FeatureGroup.Feat,
                                             Helpers.PrerequisiteFeature(stalwart),
                                             Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 11)
                                             );
            

            var combat_expertise_toggle = library.Get<BlueprintActivatableAbility>("a75f33b4ff41fc846acbac75d1a88442");
            var fight_defensively_toggle = library.Get<BlueprintActivatableAbility>("09d742e8b50b0214fb71acfc99cc00b3");

            stalwarBuffReplacement(combat_expertise_toggle, stalwart_toggle_buff);
            stalwarBuffReplacement(fight_defensively_toggle, stalwart_toggle_buff);


            library.AddFeats(stalwart, improved_stalwart);
        }

        static void stalwarBuffReplacement(BlueprintActivatableAbility toggle_ability, BlueprintBuff stalwart_toggle_buff)
        {
            var toggle_buff = toggle_ability.Buff;
            var stalwart_buff = Helpers.CreateBuff("Stalwart" + toggle_buff.name,
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   null,
                                                   toggle_buff.ComponentsArray
                                                   );
            stalwart_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var ac_component = stalwart_buff.GetComponents<AddStatBonusAbilityValue>().Where(c => c.Stat == StatType.AC).FirstOrDefault();
            if (ac_component != null)
            {//fight defensively
                stalwart_buff.ReplaceComponent(ac_component, Common.createContextPhysicalDR(ac_component.Value));
                //fix for crane wing
                var crane_style_buff = library.Get<BlueprintBuff>("e8ea7bd10136195478d8a5fc5a44c7da");
                var crane_style_conditional = (crane_style_buff.GetComponent<AddInitiatorAttackWithWeaponTrigger>().Action.Actions[0] as Conditional);

                var has_fight_defensively = crane_style_conditional.ConditionsChecker.Conditions[2] as ContextConditionHasFact;

                crane_style_conditional.ConditionsChecker.Conditions = crane_style_conditional.ConditionsChecker.Conditions.RemoveFromArray(has_fight_defensively);
                var combined_condition = Helpers.CreateConditional(new Condition[] { has_fight_defensively, Helpers.CreateConditionHasFact(stalwart_buff) }, crane_style_conditional.IfTrue.Actions[0]);
                combined_condition.ConditionsChecker.Operation = Operation.Or;
                crane_style_conditional.IfTrue = Helpers.CreateActionList(combined_condition);
            }
            else
            {//combat expertise
                var scaled_dr = Common.createContextPhysicalDR(Helpers.CreateContextValue(AbilityRankType.StatBonus));
                stalwart_buff.ReplaceComponent<AddStatBonus>(scaled_dr);
                stalwart_buff.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.BaseAttack, progression: ContextRankProgression.OnePlusDivStep,
                                                                           type: AbilityRankType.StatBonus, stepLevel: 4));
            }
            
            var improved_stalwart_buff = library.CopyAndAdd<BlueprintBuff>(stalwart_buff.AssetGuid, "ImprovedStalwart" + toggle_buff.name, "");
            improved_stalwart_buff.AddComponent(improved_stalwart_buff.GetComponent<AddDamageResistancePhysical>());

            var new_toggle_buff = library.CopyAndAdd<BlueprintBuff>(toggle_buff.AssetGuid, "Base" + toggle_buff.name, "");
            toggle_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var context_action = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(stalwart_toggle_buff),
                                                           Helpers.CreateConditional(Common.createContextConditionCasterHasFact(improved_stalwart),
                                                                                     Common.createContextActionApplyBuff(improved_stalwart_buff, Helpers.CreateContextDuration(),
                                                                                                                         is_child: true, dispellable: false, is_permanent: true),
                                                                                     Common.createContextActionApplyBuff(stalwart_buff, Helpers.CreateContextDuration(),
                                                                                                                         is_child: true, dispellable: false, is_permanent: true)
                                                                                     ),
                                                           Common.createContextActionApplyBuff(toggle_buff, Helpers.CreateContextDuration(),
                                                                                               is_child: true, dispellable: false, is_permanent: true)
                                                         );
            new_toggle_buff.SetComponents(Helpers.CreateAddFactContextActions(context_action));
            toggle_ability.Buff = new_toggle_buff;

            BladeTutor.RuleCalculateAttackBonusWithoutTarget_OnTrigger_Patch.facts.Add(improved_stalwart_buff);
            BladeTutor.RuleCalculateAttackBonusWithoutTarget_OnTrigger_Patch.facts.Add(stalwart);
        }

    }
}
