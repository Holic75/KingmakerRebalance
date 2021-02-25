using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
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
    public class Inquisitions
    {
        static LibraryScriptableObject library => Main.library;
        static internal bool test_mode = false;
        static public BlueprintProgression anger;
        static public BlueprintProgression heresy;
        static public BlueprintProgression conversion;
        static public BlueprintProgression tactics;
        static public BlueprintProgression valor;
        static public BlueprintProgression imprisonment;
        static public BlueprintProgression persistance;

        static BlueprintCharacterClass[] scaling_classes = new BlueprintCharacterClass[] { library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0"), //cleric
                                                                                           library.Get<BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce")};//inquisitor


        static internal void create(bool is_test = false)
        {
            test_mode = is_test;
            Main.logger.Log("Inquisitions test mode: " + test_mode.ToString());
            createAngerInquisiton();
            createHeresyInquisition();
            createConversionInquisition();
            createValorInquisition();
            createTacticsInquisition();
            createImprisonmentInquisition();
            createPersistanceInquisition();
        }


        static void addToDomainSelection(BlueprintProgression inquisition, params string[] deities)
        {
            var domain_selections = new BlueprintFeatureSelection[] { library.Get<BlueprintFeatureSelection>("48525e5da45c9c243a343fc6545dbdb9"), library.Get<BlueprintFeatureSelection>("43281c3d7fe18cc4d91928395837cd1e") };
            foreach (var selection in domain_selections)
            {
                selection.AllFeatures = selection.AllFeatures.AddToArray(inquisition);
            }

            var gods = library.Get<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4").AllFeatures;

            foreach (var deity in deities)
            {
                foreach (var god in gods)
                {
                    if (god.name.Contains(deity))
                    {
                        inquisition.AddComponent(Helpers.PrerequisiteFeature(god, any: true));
                    }
                }
            }



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
            divine_anger_ability.DeactivateIfCombatEnded = !test_mode;
            var divine_anger = Common.ActivatableAbilityToFeature(divine_anger_ability, false);
            divine_anger.AddComponent(Helpers.CreateAddAbilityResource(divine_anger_resource));

            var greater_rage = library.Get<BlueprintFeature>("ce49c579fe0bcc647a32c96929fae982");
            var tireless_rage = library.Get<BlueprintFeature>("ca9343d75a83a2745a22fa11c383153a");

            var extra_rage = library.Get<BlueprintFeature>("1a54bbbafab728348a015cf9ffcf50a7");
            extra_rage.AddComponent(Helpers.PrerequisiteFeature(divine_anger, any: true));

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

            addToDomainSelection(anger, "Gorum", "Rovagug");
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
                                                                                                       m.TakeBest = true;
                                                                                                       m.Rule = NewMechanics.ModifyD20WithActions.RuleType.SkillCheck;
                                                                                                       m.RerollOnlyIfFailed = true;
                                                                                                      //m.DispellOnRerollFinished = true;
                                                                                                       m.required_resource = blessed_infiltration_resource;
                                                                                                       m.actions = Helpers.CreateActionList(Common.createContextActionSpendResource(blessed_infiltration_resource, 1));
                                              })
                                                                                                       
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
                ability.DeactivateImmediately = true;

                blessed_infiltration.AddComponent(Helpers.CreateAddFact(ability));
            }

            var word_of_anathema_resource = Helpers.CreateAbilityResource("WordOfAnathemaResource", "", "", "", null);
            word_of_anathema_resource.SetFixedResource(1);

            var word_of_anathema = library.CopyAndAdd<BlueprintAbility>("69851cc3b821c2d479ac1f2d86e8ffa5", "WordOfAnathemaAbility", "");//curse deterioration
            word_of_anathema.Type = AbilityType.SpellLike;
            word_of_anathema.Range = AbilityRange.Medium;
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
            word_of_anathema.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClasses(scaling_classes, StatType.Wisdom));
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


            addToDomainSelection(heresy, "Abadar", "Asmodeus", "Calistria", "Desna", "Erastil", "Gorum", "Gozreh", "Iomedae", "Lamashtu", "Nethys",
                                         "Norgorber", "Pharasma", "Rovagug", "Sarenrae", "Shelyn", "Torag", "Urgathoa", "ZonKuthon");
        }


        static void createConversionInquisition()
        {
            var charm_of_wisdom = Helpers.CreateFeature("CharmOfWisdomFeature",
                                                               "Charm of Wisdom",
                                                               "You use your Wisdom modifier instead of your Charisma modifier when making Bluff, Diplomacy, and Intimidate checks.",
                                                               "",
                                                               library.Get<BlueprintAbility>("d316d3d94d20c674db2c24d7de96f6a7").Icon, //serenity
                                                               FeatureGroup.None,
                                                               Helpers.Create<StatReplacementMechanics.ReplaceBaseStatForStatTypeLogic>(s => {
                                                                                                         s.StatTypeToReplaceBastStatFor = StatType.SkillPersuasion;
                                                                                                         s.NewBaseStatType = StatType.Wisdom;
                                                                                                     }),
                                                               //Helpers.Create<NewMechanics.SkillStatReplacement>(s => { s.Skill = StatType.SkillPersuasion; s.ReplacementStat = StatType.Wisdom; }),
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
            swaying_word.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClasses(scaling_classes, StatType.Wisdom));
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

            addToDomainSelection(conversion);
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

            addToDomainSelection(valor, "CaydenCailean", "Erastil", "Iomedae", "Sarenrae");

        }


        static void createTacticsInquisition()
        {
            var inquisitors_direction = library.CopyAndAdd<BlueprintAbility>("486eaff58293f6441a5c2759c4872f98", "InquisitorsDirectionAbility", "");
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
                                                            a.Condition = Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsAlly>(), Common.createContextConditionIsCaster(not: true));
                                                        }
                                                        );
            var buff = library.CopyAndAdd<BlueprintBuff>("c96380f6dcac83c45acdb698ae70ffc4", "GrantTheInitiativeBuff", "");
            buff.ReplaceComponent<AddAreaEffect>(a => a.AreaEffect = area);

            var grant_the_initiative = Helpers.CreateFeature("GrantInitiativeFeature",
                                                             grant_the_initiative_buff.Name,
                                                             grant_the_initiative_buff.Description,
                                                             "",
                                                             grant_the_initiative_buff.Icon,
                                                             FeatureGroup.None,
                                                             Common.createAuraFeatureComponent(buff),
                                                             Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Wisdom)
                                                             );

            tactics = Helpers.CreateProgression("TacticsInquisitionProgression",
                                  "Tactics Inquisition",
                                  "It is the cold and tactical mind that often wins the day. A proper, carefully considered sacrifice can inspire one’s allies to serve your cause.",
                                  "",
                                  null,
                                  FeatureGroup.Domain);
            tactics.Classes = scaling_classes;

            tactics.LevelEntries = new LevelEntry[]{
                                                    Helpers.LevelEntry(1, inquisitors_direction_feat),
                                                    Helpers.LevelEntry(8, grant_the_initiative)
                                                 };
            tactics.UIGroups = Helpers.CreateUIGroups(inquisitors_direction_feat, grant_the_initiative);

            addToDomainSelection(tactics, "Gorum", "Irori", "Torag");
        }


        static void createImprisonmentInquisition()
        {
            var caging_strike_resource = Helpers.CreateAbilityResource("CagingStrikeResource", "", "", "", null);
            caging_strike_resource.SetIncreasedByStat(3, StatType.Wisdom);

            var entangle = library.Get<BlueprintBuff>("f7f6330726121cf4b90a6086b05d2e38");
            var apply_entangle = Common.createContextActionApplyBuff(entangle, Helpers.CreateContextDuration(null, DurationRate.Rounds, Kingmaker.RuleSystem.DiceType.D4, 1), dispellable: false);
            var apply_entnagle_save = Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(Helpers.CreateConditionalSaved(null, apply_entangle)));
            var caging_strike_buff = Helpers.CreateBuff("CagingStrikeBuff",
                                                        "Caging Strike",
                                                        "With a devastating weapon strike, spectral chains wrap around your target for a short period of time. Whenever you confirm a critical hit with a melee or ranged weapon attack (including spells that require attack rolls), you can choose to also entangle that target for 1d4 rounds (Fortitude negates). You may use this ability a number of times per day equal to 3 + your Wisdom modifier.",
                                                        "",
                                                        entangle.Icon,
                                                        null,
                                                        Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(apply_entnagle_save), critical_hit: true),
                                                        Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(Common.createContextActionSpendResource(caging_strike_resource, 1)),
                                                                                                         on_initiator: true, critical_hit: true),
                                                        Common.createContextCalculateAbilityParamsBasedOnClasses(scaling_classes, StatType.Wisdom)
                                                        );
            var caging_strike = Helpers.CreateActivatableAbility("CagingStrikeActivatableAbility",
                                                                 caging_strike_buff.Name,
                                                                 caging_strike_buff.Description,
                                                                 "",
                                                                 caging_strike_buff.Icon,
                                                                 caging_strike_buff,
                                                                 AbilityActivationType.Immediately,
                                                                 Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                                 null,
                                                                 Helpers.CreateActivatableResourceLogic(caging_strike_resource, ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                                 );
            caging_strike.DeactivateImmediately = true;

            var caging_strike_feature = Common.ActivatableAbilityToFeature(caging_strike, false);
            caging_strike_feature.AddComponent(Helpers.CreateAddAbilityResource(caging_strike_resource));


            var divine_prison_resource = Helpers.CreateAbilityResource("DivinePrisonResource", "", "", "", null);
            divine_prison_resource.SetFixedResource(1);

            var hold_monster = library.Get<BlueprintBuff>("2cfcce5b62d3e6d4082ec31b58468cc8");
            var apply_hold_monster = Common.createContextActionApplyBuff(hold_monster, Helpers.CreateContextDuration(Helpers.CreateContextValue(Kingmaker.Enums.AbilityRankType.Default)), dispellable: false);
            var apply_hold_monster_save = Common.createContextActionSavingThrow(SavingThrowType.Will, Helpers.CreateActionList(Helpers.CreateConditionalSaved(null, apply_hold_monster)));
            var apply_hold_monster_buff = Helpers.CreateBuff("DivinePrisonBuff",
                                                        "Divine Prison",
                                                        "At 8th level, once per day upon making a successful melee attack, you can affect your target with hold monster (Will negates).",
                                                        "",
                                                        hold_monster.Icon,
                                                        null,
                                                        Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(apply_hold_monster_save)),
                                                        Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(Common.createContextActionSpendResource(divine_prison_resource, 1)),
                                                                                                         on_initiator: true),
                                                        Common.createContextCalculateAbilityParamsBasedOnClasses(scaling_classes, StatType.Wisdom),
                                                        Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                                 classes: scaling_classes)
                                                        );
            var divine_prison = Helpers.CreateActivatableAbility("DivinePrisonActivatableAbility",
                                                                 apply_hold_monster_buff.Name,
                                                                 apply_hold_monster_buff.Description,
                                                                 "",
                                                                 apply_hold_monster_buff.Icon,
                                                                 apply_hold_monster_buff,
                                                                 AbilityActivationType.Immediately,
                                                                 Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                                 null,
                                                                 Helpers.CreateActivatableResourceLogic(divine_prison_resource, ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                                 );
            divine_prison.DeactivateImmediately = true;

            var divine_prison_feature = Common.ActivatableAbilityToFeature(divine_prison, false);
            divine_prison_feature.AddComponent(Helpers.CreateAddAbilityResource(divine_prison_resource));


            imprisonment = Helpers.CreateProgression("ImprisonmentnquisitionProgression",
                                                      "Imprisonment Inquisition",
                                                      "Sometimes it is better to capture foes than to kill them—whether your intention is to punish them for their crimes or to torture them for information.",
                                                      "",
                                                      null,
                                                      FeatureGroup.Domain);
            imprisonment.Classes = scaling_classes;

            imprisonment.LevelEntries = new LevelEntry[]{
                                                    Helpers.LevelEntry(1, caging_strike_feature),
                                                    Helpers.LevelEntry(8, divine_prison_feature)
                                                 };
            imprisonment.UIGroups = Helpers.CreateUIGroups(caging_strike_feature, divine_prison_feature);


            addToDomainSelection(imprisonment, "Abadar", "Asmodeus", "Torag");
        }


        static void createPersistanceInquisition()
        {
            var combat_reflexes = library.Get<BlueprintFeature>("0f8939ae6f220984e8fb568abbdfba95");

            var add_feat = Helpers.CreateFeature("PersistanceInquisitionBonusFeat",
                                                 combat_reflexes.Name,
                                                 "You receive Combat Reflexes as a bonus feat.",
                                                 "",
                                                 combat_reflexes.Icon,
                                                 FeatureGroup.None,
                                                 Helpers.CreateAddFact(combat_reflexes)
                                                 );

            var relentless_footing_resource = Helpers.CreateAbilityResource("RelentlessFootingResource", "", "", "", null);
            relentless_footing_resource.SetIncreasedByStat(3, StatType.Wisdom);

            var longstrider_buff = library.Get<BlueprintBuff>("035ed56eb973f0e469a288ff5991c9ff");
            var relentless_footing = Helpers.CreateAbility("RelentlessFootingAbility",
                                                           "Relentless Footing",
                                                           "As a swift action, you can add 10 feet to your land speed. This increase counts as an enhancement bonus. You can use this ability a number of times per day equal to 3 + your Wisdom bonus (minimum 1).",
                                                           "",
                                                           longstrider_buff.Icon,
                                                           AbilityType.Extraordinary,
                                                           Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift,
                                                           AbilityRange.Personal,
                                                           Helpers.oneRoundDuration,
                                                           "",
                                                           Helpers.CreateRunActions(Common.createContextActionApplyBuff(longstrider_buff, Helpers.CreateContextDuration(1), dispellable: false)),
                                                           Helpers.CreateResourceLogic(relentless_footing_resource),
                                                           library.Get<BlueprintAbility>("14c90900b690cac429b229efdf416127").GetComponent<AbilitySpawnFx>() //from longstrider
                                                           );
            relentless_footing.setMiscAbilityParametersSelfOnly();
            var relentless_footing_feature = Common.AbilityToFeature(relentless_footing, false);
            relentless_footing_feature.AddComponent(Helpers.CreateAddAbilityResource(relentless_footing_resource));

            var inner_strength_resource = Helpers.CreateAbilityResource("InnerStrengthResource", "", "", "", null);
            inner_strength_resource.SetFixedResource(1);

            var lay_on_hands = library.Get<BlueprintAbility>("8d6073201e5395d458b8251386d72df1"); //lay on hands self
            var inner_strength_variants = new List<BlueprintAbility>();
            var descriptors = new SpellDescriptor[] { SpellDescriptor.Blindness, SpellDescriptor.Confusion, SpellDescriptor.Frightened, SpellDescriptor.Shaken, SpellDescriptor.Sickened, SpellDescriptor.Staggered };

            foreach (var d in descriptors)
            {
                var ability = Helpers.CreateAbility($"InnerStrength{d.ToString()}Ability",
                                                       $"Inner Strength: {d.ToString()}",
                                                       "At 6th level, once per day, you may heal yourself as a swift action, healing 1d6 hit points for every two inquisitor levels you possess. When you use this ability, you can also remove one of the following conditions from yourself: blinded, confused, frightened, nauseated, shaken, sickened, or staggered.",
                                                       "",
                                                       lay_on_hands.Icon,
                                                       AbilityType.Supernatural,
                                                       Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift,
                                                       AbilityRange.Personal,
                                                       "",
                                                       "",
                                                       Helpers.CreateRunActions(Common.createContextActionHealTarget(Helpers.CreateContextDiceValue(Kingmaker.RuleSystem.DiceType.D6, Helpers.CreateContextValue(Kingmaker.Enums.AbilityRankType.Default), 0)),
                                                                                Common.createContextActionDispelMagic(d, new SpellSchool[0], Kingmaker.RuleSystem.Rules.RuleDispelMagic.CheckType.DC)
                                                                                ),
                                                       lay_on_hands.GetComponent<AbilitySpawnFx>(),
                                                       Helpers.CreateResourceLogic(inner_strength_resource),
                                                       Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: scaling_classes),
                                                       Common.createContextCalculateAbilityParamsBasedOnClasses(scaling_classes, StatType.Wisdom)
                                                       );
                ability.setMiscAbilityParametersSelfOnly();
                inner_strength_variants.Add(ability);
            }

            var inner_strength = Common.createVariantWrapper("InnerStrengthAbility", "", inner_strength_variants.ToArray());
            inner_strength.SetName("Inner Strength");
            inner_strength.AddComponent(Helpers.CreateResourceLogic(inner_strength_resource));

            var inner_strength_feature = Common.AbilityToFeature(inner_strength, false);
            inner_strength_feature.AddComponent(Helpers.CreateAddAbilityResource(inner_strength_resource));


            persistance = Helpers.CreateProgression("PersistanceInquisitionProgression",
                                                    "Persistance Inquisition",
                                                    "Your deity chose you for your persistence. You have vowed to pursue the enemies of the faith to the world’s end if necessary.",
                                                    "",
                                                    null,
                                                    FeatureGroup.Domain);
            persistance.Classes = scaling_classes;

            persistance.LevelEntries = new LevelEntry[]{
                                                    Helpers.LevelEntry(1, add_feat, relentless_footing_feature),
                                                    Helpers.LevelEntry(8, inner_strength_feature)
                                                    };
            persistance.UIGroups = Helpers.CreateUIGroups(relentless_footing_feature, inner_strength_feature);

            addToDomainSelection(persistance, "Asmodeus", "Iomedae", "Urgathoa");
        }
    }
}
