using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    class Inquisitions
    {
        static LibraryScriptableObject library => Main.library;
        static public BlueprintProgression anger;
        static public BlueprintProgression heresy;
        static public BlueprintProgression conversion;
        static public BlueprintProgression tactics;
        static public BlueprintProgression valor;

        static BlueprintCharacterClass[] scaling_classes = new BlueprintCharacterClass[] { library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0"), //cleric
                                                                                           library.Get<BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce")};//inquisitor


        static internal void create()
        {
            createAngerInquisiton();
            createHeresyInquisition();
            createConversionInquisition();
            createValorInquisition();
            createTacticsInquisition();
            createTacticsInquisition();
        }


        static void createAngerInquisiton()
        {
            var hateful_retort_resource = Helpers.CreateAbilityResource("HatefulRetortResouce", "", "", "", null);
            hateful_retort_resource.SetFixedResource(1);


            var hateful_retort_buff = Helpers.CreateBuff("HatefulRetorBuff",
                                                         "Hateful Retort",
                                                         "Once per day, as a free action after you have been hit with a melee attack, you can make a melee attack against the creature that hit you. This melee attack is at your highest attack bonus, even if you’ve already attacked in the round.",
                                                         "",
                                                         library.Get<BlueprintAbility>("9047cb1797639924487ec0ad566a3fea").Icon, //resounding blow
                                                         null,
                                                         Common.createAddTargetAttackWithWeaponTrigger(Helpers.CreateActionList(Common.createContextActionSpendResource(hateful_retort_resource ,1)),
                                                                                                       Helpers.CreateActionList(Common.createContextActionAttack()),
                                                                                                       only_melee: true,
                                                                                                       wait_for_attack_to_resolve: true)
                                                         );
            var hateful_retort_ability = Helpers.CreateActivatableAbility("HatefulRetortToggleAbility",
                                                                          hateful_retort_buff.Name,
                                                                          hateful_retort_buff.Description,
                                                                          "",
                                                                          hateful_retort_buff.Icon,
                                                                          hateful_retort_buff,
                                                                          Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.Immediately,
                                                                          Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                                          null,
                                                                          Helpers.CreateActivatableResourceLogic(hateful_retort_resource, ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                                          );
            hateful_retort_ability.DeactivateImmediately = true;

            var hateful_retort = Common.ActivatableAbilityToFeature(hateful_retort_ability, false);
            hateful_retort.AddComponent(Helpers.CreateAddAbilityResource(hateful_retort_resource));

            var divine_anger_resource = Helpers.CreateAbilityResource("DivineAngerResource", "", "", "", null);
            divine_anger_resource.SetIncreasedByLevelStartPlusDivStep(0, 4, 1, 1, 1, 0, 0.0f, scaling_classes);
            divine_anger_resource.SetIncreasedByStat(0, StatType.Wisdom);
            
            var divine_anger_ability = library.CopyAndAdd<BlueprintActivatableAbility>("df6a2cce8e3a9bd4592fb1968b83f730", "DivineAngerToggleAbility", "");
            divine_anger_ability.SetNameDescription("Divine Anger", "At 6th level, you gain the ability to rage like a barbarian. Your effective barbarian level for this ability is your inquisitor level – 3. If you have levels in barbarian, these levels stack when determining the effect of your rage. You do not gain any rage powers from this granted power, though if you have rage powers from another class, you may use them with these rages. You can rage a number of rounds per day equal to your Wisdom bonus, plus 1 round for every inquisitor level above 4th.");
            divine_anger_ability.ReplaceComponent<ActivatableAbilityResourceLogic>(a => a.RequiredResource = divine_anger_resource);
            var divine_anger = Common.ActivatableAbilityToFeature(divine_anger_ability, false);
            divine_anger.AddComponent(Helpers.CreateAddAbilityResource(divine_anger_resource));

            var greater_rage = library.Get<BlueprintFeature>("ce49c579fe0bcc647a32c96929fae982");
            var tireless_rage = library.Get<BlueprintFeature>("ca9343d75a83a2745a22fa11c383153a");

            anger = Helpers.CreateProgression("AngerInquisitionProgression",
                                              "Anger Inquisition",
                                              "Holy (or unholy) rage, granted by your patron deity, ensures that when you fight, the battle ends with a bloody victory.",
                                              "",
                                              null,
                                              FeatureGroup.Domain);
            anger.Classes = scaling_classes;

            anger.LevelEntries = new LevelEntry[]{
                                                    Helpers.LevelEntry(1, hateful_retort),
                                                    Helpers.LevelEntry(6, divine_anger),
                                                    Helpers.LevelEntry(14, greater_rage),
                                                    Helpers.LevelEntry(20, tireless_rage)
                                                 };
            anger.UIGroups = Helpers.CreateUIGroups(hateful_retort, divine_anger, greater_rage, tireless_rage);
        }


        static void createHeresyInquisition()
        {
            var righteous_infiltration = Helpers.CreateFeature("RighteousInfiltrationFeature",
                                                               "Righteous Infiltration",
                                                               "You use your Wisdom modifier instead of your Charisma modifier when making Bluff and Intimidate checks.",
                                                               "",
                                                               library.Get<BlueprintAbility>("90e59f4a4ada87243b7b3535a06d0638").Icon, //bless
                                                               FeatureGroup.None,
                                                               Helpers.Create<NewMechanics.AddStatDifferenceBonus>(s => { s.TargetStat = StatType.CheckBluff; s.ReplacementStat = StatType.Wisdom; s.OldStat = StatType.Charisma; }),
                                                               Helpers.Create<NewMechanics.AddStatDifferenceBonus>(s => { s.TargetStat = StatType.CheckIntimidate; s.ReplacementStat = StatType.Wisdom; s.OldStat = StatType.Charisma; }),
                                                               Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Wisdom),
                                                               Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Charisma)
                                                               );

            var blessed_infiltration_resource = Helpers.CreateAbilityResource("BlessedInfiltrationResource", "", "", "", null);
            blessed_infiltration_resource.SetIncreasedByStat(0, StatType.Wisdom);
            var blessed_infiltration = Helpers.CreateFeature("BlessedInfiltartionFeature",
                                                             "Blessed Infiltration",
                                                             "At 4th level, when you make a Bluff, Diplomacy, or Stealth check, you may roll twice and take the more favorable result. You can use this ability a number of times per day equal to your Wisdom bonus.",
                                                             "",
                                                             library.Get<BlueprintAbility>("a5e23522eda32dc45801e32c05dc9f96").Icon, //good hope
                                                             FeatureGroup.None,
                                                             Helpers.CreateAddAbilityResource(blessed_infiltration_resource));


            var checks = new StatType[] { StatType.CheckBluff, StatType.SkillStealth, StatType.CheckDiplomacy };
            var names = new string[] { "Bluff Checks", "Stealth Checks", "Diplomacy Checks" };

            for (int i = 0; i < checks.Length; i++)
            {
                var buff = Helpers.CreateBuff("BlessedInfiltartion" + checks[i].ToString() + "Buff",
                                              "Blessed Infiltration: " + names[i],
                                              blessed_infiltration.Description,
                                              "",
                                              blessed_infiltration.Icon,
                                              null,
                                              Helpers.Create<NewMechanics.ModifyD20WithActions>(m => { m.SpecificSkill = true;
                                                                                                       m.Skill = new StatType[] { checks[i] };
                                                                                                       m.RollsAmount = 1;
                                                                                                       m.Rule = RuleType.SkillCheck;
                                                                                                       m.DispellOnRerollFinished = true;
                                                                                                       m.actions = Helpers.CreateActionList(Common.createContextActionSpendResource(blessed_infiltration_resource, 1)); })
                                              );

                var ability = Helpers.CreateActivatableAbility("BlessedInfiltartion" + checks[i].ToString() + "ToggleAbility",
                                                               buff.Name,
                                                               buff.Description,
                                                               "",
                                                               buff.Icon,
                                                               buff,
                                                               AbilityActivationType.Immediately,
                                                               Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                               null,
                                                               Helpers.CreateActivatableResourceLogic(blessed_infiltration_resource, ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                               );

                blessed_infiltration.AddComponent(Helpers.CreateAddFact(ability));
            }

            var word_of_anathema_resource = Helpers.CreateAbilityResource("WordOfAnathemaResource", "", "", "", null);
            word_of_anathema_resource.SetFixedResource(1);

            var word_of_anathema = library.CopyAndAdd<BlueprintAbility>("69851cc3b821c2d479ac1f2d86e8ffa5", "WordOfAnathemaAbility", "");//curse deterioration
            word_of_anathema.Type = AbilityType.SpellLike;
            word_of_anathema.Range = AbilityRange.Long;
            word_of_anathema.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            word_of_anathema.RemoveComponents<AbilityDeliverTouch>();
            word_of_anathema.RemoveComponents<SpellComponent>();
            word_of_anathema.Parent = null;
            word_of_anathema.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(Common.changeAction<ContextActionApplyBuff>(a.Actions.Actions,
                                                                                                                                                            c =>
                                                                                                                                                            {
                                                                                                                                                                c.DurationValue = Helpers.CreateContextDuration(1, DurationRate.Minutes);
                                                                                                                                                                c.Permanent = false;
                                                                                                                                                            }
                                                                                                                                                            )
                                                                                                               )
                                                                      );
            word_of_anathema.SetNameDescription("Word of Anathema", "At 8th level, once per day, you can speak a word of anathema against a single creature within 60 feet (Will negates). This acts as bestow curse and lasts for 1 minute, giving the target a –4 penalty on attack rolls, saves, ability checks, and skill checks.");
           // word_of_anathema.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClasses(scaling_classes, StatType.Wisdom));
            word_of_anathema.LocalizedDuration = Helpers.CreateString("WordOfAnathema.Duration", Helpers.oneMinuteDuration);
            word_of_anathema.AddComponent(Helpers.CreateResourceLogic(word_of_anathema_resource));
            var word_of_anathema_feat = Common.AbilityToFeature(word_of_anathema, false);
            word_of_anathema_feat.AddComponent(Helpers.CreateAddAbilityResource(word_of_anathema_resource));

            heresy = Helpers.CreateProgression("HeresyInquisitionProgression",
                                  "Heresy Inquisition",
                                  "Often it is hard to tell heretics from the faithful. You use duplicity, stealth, and the heretics’ own arguments to root them out and bring them to justice.",
                                  "",
                                  null,
                                  FeatureGroup.Domain);
            heresy.Classes = scaling_classes;

            heresy.LevelEntries = new LevelEntry[]{
                                                    Helpers.LevelEntry(1, righteous_infiltration),
                                                    Helpers.LevelEntry(4, blessed_infiltration),
                                                    Helpers.LevelEntry(8, word_of_anathema_feat)
                                                 };
            heresy.UIGroups = Helpers.CreateUIGroups(righteous_infiltration, blessed_infiltration, word_of_anathema_feat);
        }


        static void createConversionInquisition()
        {
            var charm_of_wisdom = Helpers.CreateFeature("CharmOfWisdomFeature",
                                                               "Charm of Wisdom",
                                                               " You use your Wisdom modifier instead of your Charisma modifier when making Bluff, Diplomacy, and Intimidate checks.",
                                                               "",
                                                               library.Get<BlueprintAbility>("d316d3d94d20c674db2c24d7de96f6a7").Icon, //serenity
                                                               FeatureGroup.None,
                                                               Helpers.Create<NewMechanics.SkillStatReplacement>(s => { s.Skill = StatType.SkillPersuasion; s.ReplacementStat = StatType.Wisdom; }),
                                                               Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Wisdom),
                                                               Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Charisma)
                                                               );


            var swaying_word_resource = Helpers.CreateAbilityResource("SwayingWordResource", "", "", "", null);
            swaying_word_resource.SetFixedResource(1);

            var swaying_word = library.CopyAndAdd<BlueprintAbility>("d7cbd2004ce66a042aeab2e95a3c5c61", "SwayingWordAbility", "");//domiante
            swaying_word.Type = AbilityType.SpellLike;
            swaying_word.RemoveComponents<SpellComponent>();
            swaying_word.RemoveComponents<SpellListComponent>();
            swaying_word.RemoveComponents<ContextRankConfig>();
            swaying_word.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(Common.changeAction<ContextActionApplyBuff>(a.Actions.Actions,
                                                                                                                                                            c =>
                                                                                                                                                            {
                                                                                                                                                                c.DurationValue = Helpers.CreateContextDuration(1, DurationRate.Minutes);
                                                                                                                                                                c.Permanent = false;
                                                                                                                                                            }
                                                                                                                                                            )
                                                                                                               )
                                                                      );
            swaying_word.SetNameDescription("Swaying Word", "At 8th level, once per day you may speak a word of divinely inspired wisdom that causes a single creature to switch its alliance to you. The target must be within line of sight and able to hear you. If he fails his Will save, he is affected by dominate person, except the duration is only 1 minute.");
            //swaying_word.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClasses(scaling_classes, StatType.Wisdom));
            swaying_word.LocalizedDuration = Helpers.CreateString("WordOfAnathema.Duration", Helpers.oneMinuteDuration);
            swaying_word.AddComponent(Helpers.CreateResourceLogic(swaying_word_resource));
            var swaying_word_feat = Common.AbilityToFeature(swaying_word, false);
            swaying_word_feat.AddComponent(Helpers.CreateAddAbilityResource(swaying_word_resource));

            conversion = Helpers.CreateProgression("ConversionInquisitionProgression",
                                  "Conversion Inquisition",
                                  "You are a powerful persuader. A honeyed tongue empowered by divine argumentation sways the indifferent and adversarial to your side.",
                                  "",
                                  null,
                                  FeatureGroup.Domain);
            conversion.Classes = scaling_classes;

            conversion.LevelEntries = new LevelEntry[]{
                                                    Helpers.LevelEntry(1, charm_of_wisdom),
                                                    Helpers.LevelEntry(8, swaying_word_feat)
                                                 };
            conversion.UIGroups = Helpers.CreateUIGroups(charm_of_wisdom, swaying_word_feat);
        }


        static void createValorInquisition()
        {
            var touch_of_resolve = library.CopyAndAdd<BlueprintAbility>("55a037e514c0ee14a8e3ed14b47061de", "TouchOfResolveAbility", ""); //remove fear
            touch_of_resolve.setMiscAbilityParametersTouchFriendly();
            touch_of_resolve.RemoveComponents<SpellListComponent>();
            touch_of_resolve.RemoveComponents<SpellComponent>();
            touch_of_resolve.RemoveComponents<AbilityTargetsAround>();
            touch_of_resolve.Range = AbilityRange.Touch;
            touch_of_resolve.SetNameDescription("Touch of Resolve", "You may use remove fear on a single creature a number of times per day equal to your 3 + your Wisdom bonus.");


            var touch_of_resolve_resource = Helpers.CreateAbilityResource("TouchOfResolveResource", "", "", "", null);
            touch_of_resolve_resource.SetIncreasedByStat(3, StatType.Wisdom);
            touch_of_resolve.AddComponent(Helpers.CreateResourceLogic(touch_of_resolve_resource));
            var touch_of_resolve_feat = Common.AbilityToFeature(touch_of_resolve, false);
            touch_of_resolve_feat.AddComponent(Helpers.CreateAddAbilityResource(touch_of_resolve_resource));

            var fearless = Helpers.CreateFeature("ValorFearlessFeature",
                                                 "Fearless",
                                                 "At 8th level, you become immune to fear.",
                                                 "",
                                                 library.Get<BlueprintActivatableAbility>("be68c660b41bc9247bcab727b10d2cd1").Icon, //defensive stance
                                                 FeatureGroup.None,
                                                 library.Get<BlueprintBuff>("993a5300cc84fde4bb4df441bf92d701").ComponentsArray //fearless defensive stance power
                                                 );

            valor = Helpers.CreateProgression("ValorInquisitionProgression",
                                              "Valor Inquisition",
                                              "It takes courage to confront the enemies of your faith.",
                                              "",
                                              null,
                                              FeatureGroup.Domain);
            valor.Classes = scaling_classes;

            valor.LevelEntries = new LevelEntry[]{
                                                    Helpers.LevelEntry(1, touch_of_resolve_feat),
                                                    Helpers.LevelEntry(8, fearless)
                                                 };
            valor.UIGroups = Helpers.CreateUIGroups(touch_of_resolve_feat, fearless);
        }


        static void createTacticsInquisition()
        {
            var inquisitors_direction = library.CopyAndAdd<BlueprintAbility>("486eaff58293f6441a5c2759c4872f98", "InquisitorsDirection", "");
            inquisitors_direction.RemoveComponents<SpellListComponent>();
            inquisitors_direction.RemoveComponents<SpellComponent>();
            inquisitors_direction.RemoveComponents<AbilityTargetsAround>();
            inquisitors_direction.setMiscAbilityParametersSingleTargetRangedFriendly();
            inquisitors_direction.LocalizedDuration = Helpers.CreateString("InquisitorsDirection.Duration", Helpers.oneRoundDuration);
            inquisitors_direction.Type = AbilityType.Supernatural;
            Common.setAsFullRoundAction(inquisitors_direction);
            inquisitors_direction.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(Common.changeAction<ContextActionApplyBuff>(a.Actions.Actions,
                                                                                                                                                            c =>
                                                                                                                                                            {
                                                                                                                                                                c.DurationValue = Helpers.CreateContextDuration(1, DurationRate.Rounds);
                                                                                                                                                                c.IsNotDispelable = true;
                                                                                                                                                            }
                                                                                                                                                            )
                                                                                                               )
                                                                      );
            inquisitors_direction.SetNameDescription("Inquisitor’s Direction", "As a full-round action, you can grant one ally of your choice within close range the benefit of haste for 1 round. You can use this ability a number of times per day equal to your 3 + Wisdom bonus (minimum 1).");

            var inquisitors_direction_resource = Helpers.CreateAbilityResource("InquisitorsDirectionResource", "", "", "", null);
            inquisitors_direction_resource.SetIncreasedByStat(3, StatType.Wisdom);
            inquisitors_direction.AddComponent(Helpers.CreateResourceLogic(inquisitors_direction_resource));
            var inquisitors_direction_feat = Common.AbilityToFeature(inquisitors_direction, false);
            inquisitors_direction_feat.AddComponent(Helpers.CreateAddAbilityResource(inquisitors_direction_resource));


            var grant_the_initiative_buff = Helpers.CreateBuff("GrantTheInitiativeEffectBuff",
                                                               "Grant the Initiative",
                                                               "At 8th level, you and all allies within 30 feet may add your Wisdom bonus to your initiative checks.",
                                                               "",
                                                               null,
                                                               null,
                                                               Helpers.CreateAddContextStatBonus(StatType.Initiative, Kingmaker.Enums.ModifierDescriptor.UntypedStackable),
                                                               Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Wisdom, min: 0)
                                                               );
            grant_the_initiative_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("7ced0efa297bd5142ab749f6e33b112b", "GrantTheInitiativeAreaEffect", "");
            area.Size = 30.Feet();
            area.ReplaceComponent<AbilityAreaEffectBuff>(a =>
                                                        {
                                                            a.Buff = grant_the_initiative_buff;
                                                            a.Condition = Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsAlly>(), Common.createContextConditionIsCaster(not: false));
                                                        }
                                                        );
            var buff = library.CopyAndAdd<BlueprintBuff>("c96380f6dcac83c45acdb698ae70ffc4", "GrantTheInitiativeBuff", "");
            buff.ReplaceComponent<AddAreaEffect>(a => a.AreaEffect = area);

            var grant_the_initiative = Helpers.CreateFeature("GrantTheInitiativeFeature",
                                                             grant_the_initiative_buff.Name,
                                                             grant_the_initiative_buff.Description,
                                                             "",
                                                             grant_the_initiative_buff.Icon,
                                                             FeatureGroup.None,
                                                             Common.createAuraFeatureComponent(buff)
                                                             );

            tactics.Classes = scaling_classes;

            tactics.LevelEntries = new LevelEntry[]{
                                                    Helpers.LevelEntry(1, inquisitors_direction_feat),
                                                    Helpers.LevelEntry(8, grant_the_initiative)
                                                 };
            tactics.UIGroups = Helpers.CreateUIGroups(inquisitors_direction_feat, grant_the_initiative);
        }
    }
}
