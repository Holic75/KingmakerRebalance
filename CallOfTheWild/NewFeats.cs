using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
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

            ChannelEnergyEngine.createQuickChannel();
            ChannelEnergyEngine.createChannelSmite();
            createPlanarWildShape();
            createGuidedHand();
            createDeadeyesBlessing();
            createExtraChannelPaladin();
            ChannelEnergyEngine.createChannelingScourge();
            ChannelEnergyEngine.createImprovedChannel();
            ChannelEnergyEngine.createVersatileChanneler();

            createPowerfulShape();
            createDiscordantVoice();

            createWeaponOfTheChosen();
            createGreaterWeaponOfTheChosen();

            createDevastatingStrike();
            createStrikeTrue();

            createFellingSmash();

            createDistractingCharge();
        }


        static void createDistractingCharge()
        {
            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("7ced0efa297bd5142ab749f6e33b112b", "AuraDistractingChargeArea", "");

            area.Size = 100.Feet();

            var buff = library.CopyAndAdd<BlueprintBuff>("c96380f6dcac83c45acdb698ae70ffc4", "AuraDistractingChargeBuff", "");
            buff.ReplaceComponent<AddAreaEffect>(a => a.AreaEffect = area);

            distracting_charge = library.CopyAndAdd<BlueprintFeature>("e45ab30f49215054e83b4ea12165409f", "DistractingChargeFeature", "");
            distracting_charge.SetName("Distracting Charge");
            distracting_charge.SetDescription("When your ally with this feat uses the charge action and hits, you gain a +2 bonus on your next attack roll against the target of that charge. This bonus must be used before your ally’s next turn, or it is lost.");
            distracting_charge.SetIcon(null);
            distracting_charge.RemoveComponents<SpellImmunityToSpellDescriptor>();
            distracting_charge.RemoveComponents<BuffDescriptorImmunity>();
            distracting_charge.Groups = new FeatureGroup[] { FeatureGroup.Feat, FeatureGroup.CombatFeat, FeatureGroup.TeamworkFeat };
            distracting_charge.ReplaceComponent<AuraFeatureComponent>(a => a.Buff = buff);

            var target_buff = Helpers.CreateBuff("DistractingChargeTargetBuff",
                                             distracting_charge.Name + " Target",
                                             "",
                                             "",
                                             library.Get<BlueprintBuff>("f36da144a379d534cad8e21667079066").Icon, //charge
                                             null
                                             );

            var apply_target_buff = Common.createContextActionApplyBuff(target_buff, Helpers.CreateContextDuration(1), dispellable: false);
            var ally_buff = Helpers.CreateBuff("DistractingChargeAllyBuff",
                                             distracting_charge.Name,
                                             "",
                                             "",
                                             library.Get<BlueprintBuff>("f36da144a379d534cad8e21667079066").Icon, //charge,
                                             null,
                                             Helpers.Create<AttackBonusAgainstFactOwner>(a => { a.AttackBonus = 2; a.CheckedFact = target_buff; })
                                             );
            ally_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var apply_ally_buff = Common.createContextActionApplyBuff(ally_buff, Helpers.CreateContextDuration(1), dispellable: false);

            var effect_on_ally1 = Helpers.CreateConditional(new Condition[] { Helpers.Create<ContextConditionIsAlly>(), Helpers.Create<TeamworkMechanics.ContextConditionHasSoloTactics>(), Helpers.CreateConditionHasFact(distracting_charge) },
                                                            apply_ally_buff
                                                          );

            var effect_on_ally2 = Helpers.CreateConditional(new Condition[] { Helpers.Create<ContextConditionIsAlly>(), Helpers.CreateConditionHasFact(distracting_charge) },
                                                            apply_ally_buff,
                                                            null

                                              );

            var actions_on_ally1 = Helpers.Create<TeamworkMechanics.ContextActionOnUnitsWithinRadius>(c => { c.Radius = 100; c.actions = Helpers.CreateActionList(effect_on_ally1); });
            var actions_on_ally2 = Helpers.CreateConditional(Helpers.CreateConditionCasterHasFact(distracting_charge),
                                                             Helpers.Create<TeamworkMechanics.ContextActionOnUnitsWithinRadius>(c => { c.Radius = 100; c.actions = Helpers.CreateActionList(effect_on_ally2); })
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
            distracting_charge.AddComponents(Helpers.Create<NewMechanics.AddInitiatorAttackWithWeaponTriggerOnCharge>(a => {a.Action = Helpers.CreateActionList(actions_on_ally2);
                                                                                                                           a.ActionsOnInitiator = true;
                                                                                                                           }),
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
                                                           AbilityActivationType.Immediately,
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
                                            Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(Helpers.Create<ContextActionRemoveSelf>()), only_hit: false, on_initiator: true)
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
                                                            "First Attack or untile end of the round.",
                                                            "",
                                                            Helpers.CreateRunActions(apply_buff)
                                                            );
            strike_true_ability.setMiscAbilityParametersSelfOnly(animation: Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Special);
            strike_true = Common.AbilityToFeature(strike_true_ability);
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
                                                                   Common.createAddTargetAttackWithWeaponTrigger(deal_dmg, null, not_reach: false, only_melee: false)
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


            discordant_voice = Helpers.CreateFeature("DiscordantVocieFeature",
                                                     discordant_voice_effect_buff.Name,
                                                     discordant_voice_effect_buff.Description,
                                                     "",
                                                     discordant_voice_effect_buff.Icon,
                                                     FeatureGroup.Feat,
                                                     Helpers.PrerequisiteClassLevel(bard, 8, any: true),
                                                     Helpers.PrerequisiteClassLevel(Skald.skald_class, 8, any: true)
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

            paladin_heal_base.addToAbilityVariants(heal_living_extra);
            paladin_harm_base.addToAbilityVariants(harm_undead_extra);
            ChannelEnergyEngine.storeChannel(heal_living_extra, paladin_channel_energy, ChannelEnergyEngine.ChannelType.PositiveHeal);
            ChannelEnergyEngine.storeChannel(harm_undead_extra, paladin_channel_energy, ChannelEnergyEngine.ChannelType.PositiveHarm);

            paladin_channel_energy.AddComponent(Helpers.CreateAddFacts(heal_living_extra, harm_undead_extra));
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
                                                      Helpers.PrerequisiteFeature(rage_feature),
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

            //library.AddFeats(blooded_arcane_strike);
            library.AddCombatFeats(blooded_arcane_strike);
            blooded_arcane_strike.Groups = blooded_arcane_strike.Groups.AddToArray(FeatureGroup.Feat);
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
            debuff.Stacking = StackingType.Stack;

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
        }

    }
}
