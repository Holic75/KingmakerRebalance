using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Weapons;
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

namespace CallOfTheWild
{
    public class RogueTalents
    {
        static public BlueprintFeatureSelection minor_magic;
        static public BlueprintFeatureSelection major_magic;
        static public BlueprintFeatureSelection feat;
        static public BlueprintFeature bleeding_attack;

        static public BlueprintFeature assasinate;
        static public BlueprintFeature swift_death;

        static public BlueprintFeature armored_marudeur;
        static public BlueprintFeature armored_swiftness;
        static public BlueprintFeature reaping_stalker;
        static public BlueprintFeature slippery_mind;

        static internal bool test_mode = false;
        static LibraryScriptableObject library => Main.library;

        static public void load()
        {
            Main.logger.Log("Rogue Talents test mode: " + test_mode.ToString());

            if (test_mode)
            {
                var study_target = library.Get<BlueprintAbility>("b96d810ceb1708b4e895b695ddbb1813");
                study_target.RemoveComponents <Kingmaker.UnitLogic.Abilities.Components.TargetCheckers.AbilityTargetIsPartyMember>();
                study_target.CanTargetFriends = true;
            }
            createMinorMagic();
            createMajorMagic();

            createFeatAndFixCombatTrick();
            createBleedingAttack();

            createAssasinateAndSwiftDeath();
            createArmoredMaraudeur();
            createArmoredSwiftness();
            createReapingStalker();
            createSlipperyMind();

            fixRogueSneakAttackTalents();
            fixSlayerTrapfindingScaling();
        }

        static void createSlipperyMind()
        {
            slippery_mind = Helpers.CreateFeature("SlipperyMindRogueTalentFeature",
                                       "Slippery Mind",
                                       "This ability represents the rogue’s ability to wriggle free from magical effects that would otherwise control or compel her. If a rogue with slippery mind is affected by an enchantment spell or effect and fails her saving throw, she can attempt it again 1 round later at the same DC. She gets only this one extra chance to succeed on her saving throw.",
                                       "",
                                       Helpers.GetIcon("eabf94e4edc6e714cabd96aa69f8b207"),//mind fog
                                       FeatureGroup.RogueTalent,
                                       Helpers.Create<NewMechanics.SecondRollToRemoveBuffAfterOneRound>(m =>
                                                                                                       {
                                                                                                           m.school = SpellSchool.Enchantment;
                                                                                                           m.save_type = SavingThrowType.Will;
                                                                                                       }
                                                                                                       )
                                       );
            addToTalentSelection(slippery_mind, advanced: true);
        }

        static void fixSlayerTrapfindingScaling()
        {
            var trapfinding = library.Get<BlueprintFeature>("e3c12938c2f93544da89824fbe0933a5");
            var archetype_list_feature = Helpers.CreateFeature("SlayerTrapfindingArchetypeExtensionFeature",
                               "",
                               "",
                               "",
                               null,
                               FeatureGroup.None);
            archetype_list_feature.AddComponent(Helpers.Create<ContextRankConfigArchetypeList>(c => c.archetypes = new BlueprintArchetype[] { Archetypes.NatureFang.archetype, Archetypes.NatureFang.archetype }));
            archetype_list_feature.HideInCharacterSheetAndLevelUp = true;
            trapfinding.ReplaceComponent<ContextRankConfig>(c =>
            {
                Helpers.SetField(c, "m_BaseValueType", ContextRankBaseValueTypeExtender.SummClassLevelWithArchetypes.ToContextRankBaseValueType());
                Helpers.SetField(c, "m_Feature", archetype_list_feature);
                Helpers.SetField(c, "m_Class", Helpers.GetField<BlueprintCharacterClass[]>(c, "m_Class").AddToArray(Archetypes.NatureFang.archetype.GetParentClass(), Archetypes.SanctifiedSlayer.archetype.GetParentClass()));
            }
            );
        }


        static void fixRogueSneakAttackTalents()
        {
            //fix missing dispelling strike sneak attack requirement
            library.Get<BlueprintFeature>("1b92146b8a9830d4bb97ab694335fa7c").AddComponent(Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("9b9eac6709e1c084cb18c3a366e0ec87")));

            var talent_ids = new string[] {"955ff81c596c1c3489406d03e81e6087", //focusing attack confused
                                           "791f50e199d069d4f8e933996a2ce054", //focusing attack shaken
                                           "79475c263e538c94f8e23907bd570a35", //focusing attack sickened
                                           "b696bd7cb38da194fa3404032483d1db", //cripling strike
                                           "1b92146b8a9830d4bb97ab694335fa7c", //dispelling attack
                                           "7787030571e87704d9177401c595408e", //slow reactions
                                          };
            foreach (var id in talent_ids)
            {
                var feature = library.Get<BlueprintFeature>(id);
                var buff = Helpers.CreateBuff(feature.name + "Buff",
                                              feature.Name,
                                              feature.Description,
                                              "",
                                              feature.Icon,
                                              null,
                                              feature.GetComponent<AddInitiatorAttackRollTrigger>()
                                              );

                if (feature.AssetGuid == "1b92146b8a9830d4bb97ab694335fa7c")
                {//cl for dispelling strike
                    buff.AddComponents(feature.GetComponents<ContextRankConfig>());
                    buff.AddComponents(feature.GetComponents<NewMechanics.ReplaceCasterLevelOfFactWithContextValue>());
                    buff.ReplaceComponent<NewMechanics.ReplaceCasterLevelOfFactWithContextValue>(r => r.Feature = buff);
                    feature.RemoveComponents<ContextRankConfig>();
                    feature.RemoveComponents<NewMechanics.ReplaceCasterLevelOfFactWithContextValue>();

                    //add druid scaling
                    var archetype_list_feature = Helpers.CreateFeature("DispellingAttackArchetypeExtensionFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None);
                    archetype_list_feature.AddComponent(Helpers.Create<ContextRankConfigArchetypeList>(c => c.archetypes = new BlueprintArchetype[] { Archetypes.NatureFang.archetype }));
                    archetype_list_feature.HideInCharacterSheetAndLevelUp = true;
                    buff.ReplaceComponent<ContextRankConfig>(c =>
                    {
                        Helpers.SetField(c, "m_BaseValueType", ContextRankBaseValueTypeExtender.SummClassLevelWithArchetypes.ToContextRankBaseValueType());
                        Helpers.SetField(c, "m_Feature", archetype_list_feature);
                        Helpers.SetField(c, "m_Class", Helpers.GetField<BlueprintCharacterClass[]>(c, "m_Class").AddToArray(Archetypes.NatureFang.archetype.GetParentClass()));
                    }
                                                        );
                }


                var toggle = Helpers.CreateActivatableAbility(feature.name + "ToggleAbility",
                                                              feature.Name,
                                                              feature.Description,
                                                              "",
                                                              feature.Icon,
                                                              buff,
                                                              AbilityActivationType.Immediately,
                                                              UnitCommand.CommandType.Free,
                                                              null
                                                              );
                toggle.Group = ActivatableAbilityGroupExtension.SneakAttack.ToActivatableAbilityGroup();
                toggle.DeactivateImmediately = true;
                feature.RemoveComponents<AddInitiatorAttackRollTrigger>();
                feature.AddComponent(Helpers.CreateAddFact(toggle));

            }
        }


        static void createReapingStalker()
        {
            reaping_stalker = Helpers.CreateFeature("ReapingStalkerSlayerTalentFeature",
                                                    "Reaping Stalker",
                                                    "A slayer with this talent treats any sickle or scythe he wields as though it were one size larger for the purpose of determining its damage dice.\n"
                                                    + "In addition, the slayer increases the critical threat range of any sickle or scythe he wields by 1; this does not stack with other effects that alter a weapon’s threat range.",
                                                    "",
                                                    Helpers.GetIcon("4eacfc7e152930a45a1a16217c35011c"), //scyth
                                                    FeatureGroup.None,
                                                    Helpers.Create<NewMechanics.WeaponCategorySizeChange>(w => w.category = WeaponCategory.Sickle),
                                                    Helpers.Create<NewMechanics.WeaponCategorySizeChange>(w => w.category = WeaponCategory.Scythe),
                                                    Helpers.Create<WeaponTypeCriticalEdgeIncrease>(w => w.WeaponType = library.Get<BlueprintWeaponType>("ec2da496c7936e14c9a28ce616a6b4cd")), //sickle
                                                    Helpers.Create<WeaponTypeCriticalEdgeIncrease>(w => w.WeaponType = library.Get<BlueprintWeaponType>("4eacfc7e152930a45a1a16217c35011c")) //scythe
                                                    );
            addToSlayerTalentSelection(reaping_stalker, advanced: true);
        }


        static void createArmoredMaraudeur()
        {
            var slayer = library.Get<BlueprintCharacterClass>("c75e0971973957d4dbad24bc7957e4fb");
            var druid = library.Get<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");
            var heavy_armor_proficiency = library.Get<BlueprintFeature>("1b0f68188dcc435429fb87a022239681");
            armored_marudeur = Helpers.CreateFeature("ArmoredMaraudeurSlayerTalentFeature",
                                                     "Armored Marauder",
                                                     "The slayer gains proficiency with heavy armor. In addition, the armor check penalty of any heavy armor the slayer wears is reduced by 1 for every 6 slayer levels he has.",
                                                     "",
                                                     heavy_armor_proficiency.Icon,
                                                     FeatureGroup.RogueTalent,
                                                     Helpers.Create<ArmorCheckPenaltyIncrease>(a => { a.Bonus = Helpers.CreateContextValue(AbilityRankType.Default); a.CheckCategory = true; a.Category = ArmorProficiencyGroup.Heavy; }),
                                                     Helpers.CreateContextRankConfig(ContextRankBaseValueType.SummClassLevelWithArchetype,
                                                                                     archetype: Archetypes.NatureFang.archetype,
                                                                                     classes: new BlueprintCharacterClass[] { slayer, druid },
                                                                                     progression: ContextRankProgression.DivStep, stepLevel: 6),
                                                     Helpers.CreateAddFact(heavy_armor_proficiency),
                                                     Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("46f4fb320f35704488ba3d513397789d")) //medium armor prof
                                                     );

            addToSlayerTalentSelection(armored_marudeur, advanced: true);
        }


        static void createArmoredSwiftness()
        {
            var druid = library.Get<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");
            var slayer = library.Get<BlueprintCharacterClass>("c75e0971973957d4dbad24bc7957e4fb");
            armored_swiftness = Helpers.CreateFeature("ArmoredSwiftnessSlayerTalentFeature",
                                                     "Armored Swiftness",
                                                     "A slayer with this talent can move at full speed in heavy armor. In addition, the maximum Dexterity bonus of heavy armor the slayer wears increases by 1 for every 6 class levels he has.",
                                                     "",
                                                     Helpers.GetIcon("76d4885a395976547a13c5d6bf95b482"),
                                                     FeatureGroup.RogueTalent,
                                                     Helpers.Create<NewMechanics.ContextMaxDexBonusIncrease>(a => { a.bonus = Helpers.CreateContextValue(AbilityRankType.Default); a.category = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.Heavy }; }),
                                                     Helpers.CreateContextRankConfig(ContextRankBaseValueType.SummClassLevelWithArchetype,
                                                                                     archetype: Archetypes.NatureFang.archetype,
                                                                                     classes: new BlueprintCharacterClass[] { slayer, druid },
                                                                                     progression: ContextRankProgression.DivStep, stepLevel: 6),
                                                     Helpers.PrerequisiteFeature(armored_marudeur)
                                                     );

            var reduce_heavy_armor_speed_penalty = Helpers.CreateFeature("ArmoredSwiftnessSpeedPenaltyRemovalFeature",
                                                     "",
                                                     "",
                                                     "",
                                                     null,
                                                     FeatureGroup.None,
                                                     Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.ImmunToArmorSpeedPenalty)
                                                     );
            reduce_heavy_armor_speed_penalty.HideInCharacterSheetAndLevelUp = true;
            armored_swiftness.AddComponent(Helpers.Create<WeaponTrainingMechanics.AddFeatureOnArmor>(a => { a.feature = reduce_heavy_armor_speed_penalty; a.required_armor = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.Heavy }; }));

            addToSlayerTalentSelection(armored_swiftness, advanced: true);
        }


        static void createAssasinateAndSwiftDeath()
        {
            var druid = library.Get<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");
            var slayer_class = library.Get<BlueprintCharacterClass>("c75e0971973957d4dbad24bc7957e4fb");
            var rogue_class = library.Get<BlueprintCharacterClass>("299aa766dee3cbf4790da4efb8c72484");

            var sneak_attack = library.Get<BlueprintFeature>("9b9eac6709e1c084cb18c3a366e0ec87");
            var assasinate_buff = Helpers.CreateBuff("SlayerAssasinateBuff",
                                                     "Assasinate Target",
                                                     "A slayer or ninja with this advanced talent can kill foes that are unable to defend themselves. To attempt to assassinate a target, the slayer or ninja must first study his target for 1 round as a standard action. On the following round, if the slayer or ninja makes a sneak attack against the target and that target is denied its Dexterity bonus to AC, the sneak attack has the additional effect of possibly killing the target. This attempt automatically fails if this is not the first slayer attack against the enemy. If the sneak attack is successful, the target must attempt a Fortitude saving throw with a DC equal to 10 + 1/2 the slayer’s level + the slayer’s Intelligence modifier. If the target fails this save, it dies; otherwise, the target takes the sneak attack damage as normal and is then immune to that slayer’s assassinate ability for 24 hours.",
                                                     "",
                                                     sneak_attack.Icon,
                                                     null);
            assasinate_buff.Stacking = StackingType.Stack;

            var assasinate_cooldown = Helpers.CreateBuff("SlayerAssasinateCooldownBuff",
                                         "Assassinate Target Cooldown",
                                         assasinate_buff.Description,
                                         "",
                                         sneak_attack.Icon,
                                         null);
            assasinate_cooldown.Stacking = StackingType.Stack;
            var apply_cooldown = Helpers.CreateConditional(Common.createContextConditionHasBuffFromCaster(assasinate_cooldown), null,
                                                           Common.createContextActionApplyBuff(assasinate_cooldown, Helpers.CreateContextDuration(1, DurationRate.Days), dispellable: false));
            var apply_buff = Common.createContextActionApplyBuff(assasinate_buff, Helpers.CreateContextDuration(2, DurationRate.Rounds), dispellable: false);

            var ability = Helpers.CreateAbility("AssasinateAbility",
                                                "Assassinate",
                                                assasinate_buff.Description,
                                                "",
                                                assasinate_buff.Icon,
                                                AbilityType.Extraordinary,
                                                UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                "2 rounds",
                                                "",
                                                Helpers.CreateRunActions(apply_buff),
                                                Common.createAbilityTargetHasNoFactUnlessBuffsFromCaster(new BlueprintBuff[] { assasinate_cooldown, assasinate_buff}),
                                                Common.createAbilityTargetHasFact(true, Common.construct, Common.elemental, Common.undead),
                                                Helpers.Create<NonOffensiveActionMechanics.NonOffensiveAbility>()
                                                );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful();
            ability.EffectOnEnemy = AbilityEffectOnUnit.None;
            ability.EffectOnAlly = AbilityEffectOnUnit.None;
            //Common.setAsFullRoundAction(ability);

            var attempt_assasinate = Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsFlatFooted>(), Common.createContextConditionHasBuffFromCaster(assasinate_buff)),
                                                               Helpers.CreateActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateConditionalSaved(null, Helpers.Create<ContextActionKillTarget>()))
                                                               );
            assasinate = Helpers.CreateFeature("AssasinateSlayerTalentFeature",
                                               ability.Name,
                                               ability.Description,
                                               "",
                                               ability.Icon,
                                               FeatureGroup.RogueTalent,
                                               Helpers.CreateAddFact(ability),
                                               Helpers.Create<AddInitiatorAttackRollTrigger>(a =>
                                                                                            {
                                                                                                a.OnlyHit = true;
                                                                                                a.SneakAttack = true;
                                                                                                a.Action = Helpers.CreateActionList(attempt_assasinate);
                                                                                            }
                                                                                            ),
                                               Helpers.Create<AddInitiatorAttackRollTrigger>(a =>
                                                                                              {
                                                                                                  a.OnlyHit = false;
                                                                                                  a.Action = Helpers.CreateActionList(apply_cooldown, Common.createContextActionRemoveBuffFromCaster(assasinate_buff));
                                                                                              }
                                                                                            ),
                                               Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(new BlueprintCharacterClass[] {slayer_class, rogue_class, druid},
                                                                                                                      new BlueprintArchetype[] {Archetypes.NatureFang.archetype },
                                                                                                                      StatType.Intelligence)
                                               );

            Common.addToSlayerStudiedTargetDC(assasinate);

            addToSlayerTalentSelection(assasinate, advanced: true);

            var swift_resource = Helpers.CreateAbilityResource("SlayerSwiftDeathResource", "", "", "", null);
            swift_resource.SetIncreasedByLevelStartPlusDivStep(1, 19, 1, 100, 0, 0, 0.0f, new BlueprintCharacterClass[] { slayer_class });

            var swift_ability = library.CopyAndAdd<BlueprintAbility>(ability, "SlayerSwiftDeathAbility", "");
            swift_ability.ActionType = UnitCommand.CommandType.Swift;
            Common.unsetAsFullRoundAction(swift_ability);

            swift_ability.AddComponent(swift_resource.CreateResourceLogic());
            swift_ability.SetNameDescription("Swift Death", "At 14th level, once per day an executioner can attempt to assassinate a foe without studying his foe beforehand. He must still succeed at a sneak attack against the target. At 19th level, he can make two such attacks per day.");

            swift_death = Common.AbilityToFeature(swift_ability, false);
            swift_death.AddComponent(swift_resource.CreateAddAbilityResource());
        }


        static void createBleedingAttack()
        {
            var bleed1d6 = library.Get<BlueprintBuff>("75039846c3d85d940aa96c249b97e562");
            var icon = NewSpells.deadly_juggernaut.Icon;
            var effect_buff = Helpers.CreateBuff("RogueBleedingAttackEffectBuff",
                                          "Bleeding Attack Effect",
                                          "A rogue with this ability can cause living opponents to bleed by hitting them with a sneak attack. This attack causes the target to take 1 additional point of damage each round for each die of the rogue’s sneak attack (e.g., 4d6 equals 4 points of bleed). Bleeding creatures take that amount of damage every round at the start of each of their turns. The bleeding can be stopped by a successful DC 15 Heal check or the application of any effect that heals hit point damage. Bleed damage from this ability does not stack with itself. Bleed damage bypasses any damage reduction the creature might possess.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.Create<BleedMechanics.BleedBuff>(b => b.dice_value = Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilityRankType.Default))),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, customProperty: NewMechanics.SneakAttackDiceGetter.Blueprint.Value),
                                          Helpers.CreateSpellDescriptor(SpellDescriptor.Bleed),
                                          bleed1d6.GetComponent<CombatStateTrigger>(),
                                          bleed1d6.GetComponent<AddHealTrigger>()
                                          );

            var apply_buff = Common.createContextActionApplyBuff(effect_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true);
            var buff = Helpers.CreateBuff("RogueBleedingAttackBuff",
                                            "Bleeding Attack",
                                            effect_buff.Description,
                                            "",
                                            icon,
                                            null,
                                            Helpers.Create<AddInitiatorAttackRollTrigger>(a =>
                                                                                            {
                                                                                                a.OnlyHit = true;
                                                                                                a.SneakAttack = true;
                                                                                                a.Action = Helpers.CreateActionList(apply_buff);
                                                                                            }
                                                                                          )
                                            );

            var toggle = Helpers.CreateActivatableAbility("RogueBleedingAttackToggleAbility",
                                                          buff.Name,
                                                          buff.Description,
                                                          "",
                                                          buff.Icon,
                                                          buff,
                                                          AbilityActivationType.Immediately,
                                                          UnitCommand.CommandType.Free,
                                                          null
                                                          );
            toggle.Group = ActivatableAbilityGroupExtension.SneakAttack.ToActivatableAbilityGroup();
            toggle.DeactivateImmediately = true;

            bleeding_attack = Common.ActivatableAbilityToFeature(toggle, false);

            bleeding_attack.AddComponent(Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("9b9eac6709e1c084cb18c3a366e0ec87")));//sneak attack

            addToTalentSelection(bleeding_attack, for_investigator: false);

            var medical_discoveries = library.Get<BlueprintFeatureSelection>("67f499218a0e22944abab6fe1c9eaeee");
            medical_discoveries.AllFeatures = medical_discoveries.AllFeatures.AddToArray(bleeding_attack);
        }


        static void createFeatAndFixCombatTrick()
        {
            var combat_trick = library.Get<BlueprintFeatureSelection>("c5158a6622d0b694a99efb1d0025d2c1");
            combat_trick.AddComponent(Helpers.PrerequisiteNoFeature(combat_trick));

            feat = library.CopyAndAdd<BlueprintFeatureSelection>("247a4068296e8be42890143f451b4b45", "RogueTalentFeat", "");
            feat.SetDescription(" A rogue can gain any feat that she qualifies for in place of a rogue talent.");
            feat.AddComponents(Helpers.PrerequisiteNoFeature(feat));
            addToTalentSelection(feat, advanced: true);
        }


        public static void addToTalentSelection(BlueprintFeature f, bool advanced = false, bool for_investigator = true)
        {
            var selections =
                new BlueprintFeatureSelection[]
                {
                    library.Get<BlueprintFeatureSelection>("c074a5d615200494b8f2a9c845799d93"), //rogue talent
                    library.Get<BlueprintFeatureSelection>("04430ad24988baa4daa0bcd4f1c7d118"), //slayer talent2
                    library.Get<BlueprintFeatureSelection>("43d1b15873e926848be2abf0ea3ad9a8"), //slayer talent6
                    library.Get<BlueprintFeatureSelection>("913b9cf25c9536949b43a2651b7ffb66"), //slayerTalent10
                    Archetypes.SanctifiedSlayer.talented_slayer,
                    Archetypes.NatureFang.slayer_talent4,
                    Archetypes.NatureFang.slayer_talent6,
                    Archetypes.NatureFang.slayer_talent10,
                    Skald.rogue_talents, //red tongue
                    VersatilePerformance.rogue_talents, //archaelogist
                };

            if (for_investigator)
            {
                selections = selections.AddToArray(Investigator.investigator_talent_selection);
            }

            foreach (var s in selections)
            {
                s.AllFeatures = s.AllFeatures.AddToArray(f);
            }

            var advanced_talents = library.Get<BlueprintFeature>("a33b99f95322d6741af83e9381b2391c");
            if (advanced)
            {
                f.AddComponent(Helpers.PrerequisiteFeature(advanced_talents));
            }
        }


        static void addToSlayerTalentSelection(BlueprintFeature f, bool advanced = false)
        {
            var selections =
                new BlueprintFeatureSelection[]
                {
                    library.Get<BlueprintFeatureSelection>("04430ad24988baa4daa0bcd4f1c7d118"), //slayer talent2
                    library.Get<BlueprintFeatureSelection>("43d1b15873e926848be2abf0ea3ad9a8"), //slayer talent6
                    library.Get<BlueprintFeatureSelection>("913b9cf25c9536949b43a2651b7ffb66"), //slayerTalent10
                    Archetypes.SanctifiedSlayer.talented_slayer,
                    Archetypes.NatureFang.slayer_talent4,
                    Archetypes.NatureFang.slayer_talent6,
                    Archetypes.NatureFang.slayer_talent10,
                };

            foreach (var s in selections)
            {
                s.AllFeatures = s.AllFeatures.AddToArray(f);
            }
            var advanced_talents = library.Get<BlueprintFeature>("a33b99f95322d6741af83e9381b2391c");
            if (advanced)
            {
                f.AddComponent(Helpers.PrerequisiteFeature(advanced_talents));
            }
        }


        static void createMinorMagic()
        {
            var druid = library.Get<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");
            var spells = Helpers.wizardSpellList.SpellsByLevel[0].Spells;

            minor_magic = Helpers.CreateFeatureSelection("MinorMagicRogueTalent",
                                                         "Minor Magic",
                                                         "A rogue with this talent gains the ability to cast a 0-level spell from the sorcerer/wizard spell list. This spell can be cast three times a day as a spell-like ability. The caster level for this ability is equal to the rogue’s level. The save DC for this spell is 10 + the rogue’s Intelligence modifier.",
                                                         "",
                                                         Helpers.GetIcon("16e23c7a8ae53cc42a93066d19766404"), //jolt
                                                         FeatureGroup.RogueTalent,
                                                         Helpers.PrerequisiteStatValue(StatType.Intelligence, 10)
                                                         );

            var classes = new BlueprintCharacterClass[] {library.Get<BlueprintCharacterClass>("299aa766dee3cbf4790da4efb8c72484"), //rogue
                                                         library.Get<BlueprintCharacterClass>("c75e0971973957d4dbad24bc7957e4fb"), //slayer
                                                         Investigator.investigator_class,
                                                         druid, //for nature fang
                                                         Skald.skald_class, //skald for red tongue
                                                         library.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f"),//bard for archaelogist
                                                         library.Get<BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce")}; //inquisitor for sanctified slayer


            foreach (var s in spells)
            {
                BlueprintFeature feature = null;
                if (!s.HasVariants)
                {
                    var spell_like = Common.convertToSpellLike(s, "MinorMagic", classes, StatType.Intelligence, no_resource: true, no_scaling: true,
                                                               guid: Helpers.MergeIds("0e6a36ac029049c98a682f688e419f45", s.AssetGuid));
                    feature = Common.AbilityToFeatureMaybeReuseGuid(spell_like, false, Helpers.MergeIds("fb026930ab7943da96f6e17b7c778f2b", s.AssetGuid));
                    feature.AddComponent(Helpers.Create<BindAbilitiesToClass>(b =>
                                                                                {
                                                                                    b.Abilites = new BlueprintAbility[] { spell_like };
                                                                                    b.CharacterClass = classes[0];
                                                                                    b.AdditionalClasses = classes.Skip(1).ToArray();
                                                                                    b.Cantrip = true;
                                                                                    b.Stat = StatType.Intelligence;
                                                                                    b.Archetypes = new BlueprintArchetype[] { Archetypes.SanctifiedSlayer.archetype, Archetypes.NatureFang.archetype };
                                                                                }
                                                                                )
                                                                            );
                }
                else
                {
                    List<BlueprintAbility> spell_likes = new List<BlueprintAbility>();
                    foreach (var v in s.Variants)
                    {
                        spell_likes.Add(Common.convertToSpellLike(v, "MinorMagic", classes, StatType.Intelligence, no_resource: true, no_scaling: true,
                                                                  guid: Helpers.MergeIds("0e6a36ac029049c98a682f688e419f45", v.AssetGuid)));
                    }
                    var wrapper = Common.createVariantWrapper("MinorMagic" + s.name, Helpers.MergeIds("0e6a36ac029049c98a682f688e419f45", s.AssetGuid), spell_likes.ToArray());
                    wrapper.SetNameDescriptionIcon(s.Name, s.Description, s.Icon);
                    feature = Common.AbilityToFeatureMaybeReuseGuid(wrapper, false, Helpers.MergeIds("fb026930ab7943da96f6e17b7c778f2b", s.AssetGuid));
                    feature.AddComponent(Helpers.Create<BindAbilitiesToClass>(b =>
                                                                                {
                                                                                    b.Abilites = spell_likes.ToArray();
                                                                                    b.CharacterClass = classes[0];
                                                                                    b.AdditionalClasses = classes.Skip(1).ToArray();
                                                                                    b.Cantrip = true;
                                                                                    b.Stat = StatType.Intelligence;
                                                                                    b.Archetypes = new BlueprintArchetype[] { Archetypes.SanctifiedSlayer.archetype, Archetypes.NatureFang.archetype };
                                                                                }
                                                                                )
                                                                             );
                }
                feature.SetName("Minor Magic: " + feature.Name);
                feature.Groups = new FeatureGroup[] { FeatureGroup.RogueTalent };
                minor_magic.AllFeatures = minor_magic.AllFeatures.AddToArray(feature);
            }
            addToTalentSelection(minor_magic);                                                       
        }


        static void createMajorMagic()
        {
            var druid = library.Get<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");
            var spells = Helpers.wizardSpellList.SpellsByLevel[1].Spells;

            major_magic = Helpers.CreateFeatureSelection("MajorMagicRogueTalent",
                                                         "Major Magic",
                                                         "A rogue with this talent gains the ability to cast a 1st-level spell from the sorcerer/wizard spell list once per day as a spell-like ability for every 2 rogue levels she possesses. The rogue’s caster level for this ability is equal to her rogue level. The save DC for this spell is 11 + the rogue’s Intelligence modifier. A rogue must have the minor magic rogue talent and an Intelligence score of at least 11 to select this talent.",
                                                         "",
                                                         Helpers.GetIcon("4ac47ddb9fa1eaf43a1b6809980cfbd2"), //magic missile
                                                         FeatureGroup.RogueTalent,
                                                         Helpers.PrerequisiteStatValue(StatType.Intelligence, 11),
                                                         Helpers.PrerequisiteFeature(minor_magic)
                                                         );

            var classes = new BlueprintCharacterClass[] {library.Get<BlueprintCharacterClass>("299aa766dee3cbf4790da4efb8c72484"), //rogue
                                                         library.Get<BlueprintCharacterClass>("c75e0971973957d4dbad24bc7957e4fb"),
                                                         Investigator.investigator_class,
                                                         druid, //for nature fang
                                                         Skald.skald_class, //for red tongue
                                                         library.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f"),//bard for archaelogist
                                                         library.Get<BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce")}; //inquisitor for sanctified slayer


            foreach (var s in spells)
            {
                var resource = Helpers.CreateAbilityResource("MajorMagic" + s.name + "Resource", "", "", Helpers.MergeIds("27fea41a99cd46609f8ab2283d1afce0", s.AssetGuid), null);
                resource.SetIncreasedByLevelStartPlusDivStep(0, 2, 1, 2, 1, 0, 0.0f, classes);
                BlueprintFeature feature = null;
                if (!s.HasVariants)
                {
                    var spell_like = Common.convertToSpellLike(s, "MajorMagic", classes, StatType.Intelligence, resource, no_scaling: true,
                                                               guid: Helpers.MergeIds("0e6a36ac029049c98a682f688e419f45", s.AssetGuid));
                    feature = Common.AbilityToFeatureMaybeReuseGuid(spell_like, false, Helpers.MergeIds("fb026930ab7943da96f6e17b7c778f2b", s.AssetGuid));
                    spell_like.AddComponent(Helpers.Create<NewMechanics.BindAbilitiesToClassFixedLevel>(b =>
                                                                                {
                                                                                    b.Abilites = new BlueprintAbility[] { spell_like };
                                                                                    b.CharacterClass = classes[0];
                                                                                    b.AdditionalClasses = classes.Skip(1).ToArray();
                                                                                    b.Cantrip = false;
                                                                                    b.Stat = StatType.Intelligence;
                                                                                    b.fixed_level = 1;
                                                                                    b.Archetypes = new BlueprintArchetype[] { Archetypes.SanctifiedSlayer.archetype, Archetypes.NatureFang.archetype };
                                                                                }
                                                                                )
                                                                            );
                }
                else
                {
                    List<BlueprintAbility> spell_likes = new List<BlueprintAbility>();
                    foreach (var v in s.Variants)
                    {
                        spell_likes.Add(Common.convertToSpellLike(v, "MajorMagic", classes, StatType.Intelligence, resource, no_scaling: true,
                                                                  guid: Helpers.MergeIds("0e6a36ac029049c98a682f688e419f45", v.AssetGuid)));
                    }
                    var wrapper = Common.createVariantWrapper("MajorMagic" + s.name, guid: Helpers.MergeIds("0e6a36ac029049c98a682f688e419f45", s.AssetGuid), spell_likes.ToArray());
                    wrapper.SetNameDescriptionIcon(s.Name, s.Description, s.Icon);
                    feature = Common.AbilityToFeatureMaybeReuseGuid(wrapper, false, Helpers.MergeIds("3704cc05c1b64ea990ae6a2b97d35311", s.AssetGuid));
                    feature.AddComponent(Helpers.Create<NewMechanics.BindAbilitiesToClassFixedLevel>(b =>
                                                                                {
                                                                                    b.Abilites = spell_likes.ToArray();
                                                                                    b.CharacterClass = classes[0];
                                                                                    b.AdditionalClasses = classes.Skip(1).ToArray();
                                                                                    b.Cantrip = false;
                                                                                    b.Stat = StatType.Intelligence;
                                                                                    b.fixed_level = 1;
                                                                                    b.Archetypes = new BlueprintArchetype[] { Archetypes.SanctifiedSlayer.archetype, Archetypes.NatureFang.archetype };
                                                                                }
                                                                                )
                                                                             );
                }
                feature.SetName("Major Magic: " + feature.Name);
                feature.Groups = new FeatureGroup[] { FeatureGroup.RogueTalent };
                feature.AddComponent(resource.CreateAddAbilityResource());
                major_magic.AllFeatures = major_magic.AllFeatures.AddToArray(feature);
            }
            addToTalentSelection(major_magic);
        }




    }
}
