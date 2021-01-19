using CallOfTheWild.NewMechanics;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    partial class MysteryEngine
    {
        public BlueprintFeature createAgingTouch(string name_prefix, string display_name, string description)
        {
            var resource = Helpers.CreateAbilityResource($"{name_prefix}Resource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 5, 1, 5, 1, 0, 0, classes, getArchetypeArray());

            var construct = library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f");
            var undead = library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");
            var construct_damage = Helpers.CreateActionDealDamage(DamageEnergyType.Magic, BalanceFixes.getDamageDie(DiceType.D6).CreateContextDiceValue(Helpers.CreateContextValue(AbilityRankType.DamageDice)));
            construct_damage.DamageType.Type = Kingmaker.RuleSystem.Rules.Damage.DamageType.Direct;
            var effect = Helpers.CreateConditional(Helpers.CreateConditionHasFact(construct),
                                                   construct_damage,
                                                   Helpers.CreateConditional(Helpers.CreateConditionHasFact(undead),
                                                                             null,
                                                                             Helpers.CreateActionDealDamage(StatType.Strength, Helpers.CreateContextDiceValue(DiceType.Zero, bonus: Helpers.CreateContextValue(AbilityRankType.Default)))
                                                                             )
                                                   );

            var ability = Helpers.CreateAbility($"{name_prefix}Ability",
                                                 display_name,
                                                 description,
                                                 "",
                                                 Helpers.GetIcon("5bf3315ce1ed4d94e8805706820ef64d"), // touch of fatigue
                                                 AbilityType.Supernatural,
                                                 CommandType.Standard,
                                                 AbilityRange.Touch,
                                                 "",
                                                 Helpers.savingThrowNone,
                                                 Helpers.CreateDeliverTouch(),

                                                createClassScalingConfig(ContextRankProgression.AsIs, // configure construct damage (1d6 per level)
                                                                          AbilityRankType.DamageDice),
                                                createClassScalingConfig(ContextRankProgression.Div2, min: 1), // configure strength damage (1 per two oracle levels).
                                                Helpers.CreateRunActions(effect),
                                                Common.createAbilitySpawnFx("9a38d742801be084d89bd34318c600e8", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(classes, getArchetypeArray(), stat)
                                                );
            ability.setMiscAbilityParametersTouchHarmful();

            var feat = Helpers.CreateFeature(name_prefix + "Feature",
                                             ability.Name, 
                                             ability.Description, 
                                             "",
                                             ability.Icon,
                                             FeatureGroup.None,
                                             resource.CreateAddAbilityResource(),
                                             ability.CreateTouchSpellCast(resource).CreateAddFact()
                                             );
            return feat;
        }



        public BlueprintFeature createSpeedOrSlowTime(string name_prefix, string display_name, string description)
        {
            var resource = Helpers.CreateAbilityResource($"{name_prefix}Resource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 7, 1, 5, 1, 0, 0, classes, getArchetypeArray());

            var haste = library.CopyAndAdd<BlueprintAbility>("486eaff58293f6441a5c2759c4872f98", name_prefix + "HasteAbility", "");
            haste.Type = AbilityType.Supernatural;
            haste.RemoveComponents<SpellComponent>();
            haste.RemoveComponents<SpellListComponent>();
            haste.AddComponent(Helpers.CreateResourceLogic(resource));
            haste.SetName(display_name + ": Haste");
            haste.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(classes, getArchetypeArray(),stat));
            var slow = library.CopyAndAdd<BlueprintAbility>("f492622e473d34747806bdb39356eb89", name_prefix + "SlowAbility", "");
            slow.Type = AbilityType.Supernatural;
            slow.RemoveComponents<SpellComponent>();
            slow.RemoveComponents<SpellListComponent>();
            slow.AddComponent(Helpers.CreateResourceLogic(resource));
            slow.SetName(display_name + ": Slow");
            slow.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(classes, getArchetypeArray(), stat));

            var wrapper = Common.createVariantWrapper(name_prefix + "Ability", "", haste, slow);
            wrapper.SetNameDescription(display_name, description);
            wrapper.AddComponent(Helpers.CreateResourceLogic(resource));

            var feature = Common.AbilityToFeature(wrapper, hide: false);
            addMinLevelPrerequisite(feature, 7);
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));
            return feature;
        }


        public BlueprintFeature createTimeFlicker(string name_prefix, string display_name, string description)
        {
            var resource = Helpers.CreateAbilityResource($"{name_prefix}Resource", "", "", "", null);
            resource.SetIncreasedByLevel(0, 1, classes, getArchetypeArray());
            // Note: reworked to use Displacement instead of Blink
            var blur = library.Get<BlueprintBuff>("dd3ad347240624d46a11a092b4dd4674");
            var displacement = library.Get<BlueprintBuff>("00402bae4442a854081264e498e7a833");

            var apply_blur = Common.createContextActionApplyBuff(blur, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false);

            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                blur.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Personal,
                                                Helpers.oneMinuteDuration,
                                                "",
                                                Helpers.CreateRunActions(apply_blur),                     
                                                Helpers.CreateResourceLogic(resource)
                                                );
            ability.setMiscAbilityParametersSelfOnly();

            var ability2 = Helpers.CreateActivatableAbility(name_prefix + "ActivatableAbility",
                                                            display_name + ": Displacement",
                                                            description,
                                                            "",
                                                            blur.Icon,
                                                            displacement,
                                                            Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
                                                            CommandType.Standard,
                                                            null,
                                                            Helpers.CreateActivatableResourceLogic(resource, Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic.ResourceSpendType.NewRound)
                                                            );

            var feature = Common.AbilityToFeature(ability, hide: false);
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));

            var feature2 = Common.ActivatableAbilityToFeature(ability2);
            feature.AddComponent(Helpers.CreateAddFeatureOnClassLevel(feature2, 7, classes, archetypes: getArchetypeArray()));
            addMinLevelPrerequisite(feature, 3);
            return feature;
        }


        public BlueprintFeature createTemporalCelerity(string name_prefix, string display_name, string description)
        {
            var feat2 = Helpers.CreateFeature(name_prefix + "Initiative2Feature",
                                              "",
                                              "",
                                              "",
                                              null,
                                              FeatureGroup.None,
                                              Helpers.Create<ModifyD20>(m => { m.Rule = RuleType.Intiative; m.RollsAmount = 2; m.TakeBest = true; })
                                              );
            feat2.HideInCharacterSheetAndLevelUp = true;

            var feat3 = Helpers.CreateFeature(name_prefix + "Initiative3Feature",
                                              "",
                                              "",
                                              "",
                                              null,
                                              FeatureGroup.None,
                                              Helpers.Create<ModifyD20>(m => { m.Rule = RuleType.Intiative; m.RollsAmount = 3; m.TakeBest = true; })
                                              );
            feat3.HideInCharacterSheetAndLevelUp = true;

            var feat_surprise = Helpers.CreateFeature(name_prefix + "SurpriseRoundFeature",
                                  "",
                                  "",
                                  "",
                                  null,
                                  FeatureGroup.None,
                                  Helpers.Create <InitiativeMechanics.CanActInSurpriseRoundLogic>()
                                  );
            feat_surprise.HideInCharacterSheetAndLevelUp = true;

            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                display_name,
                                                description,
                                                "",
                                                Helpers.GetIcon("797f25d709f559546b29e7bcb181cc74"), // improved initiative
                                                FeatureGroup.None,
                                                Helpers.CreateAddFeatureOnClassLevel(feat2, 11, classes, before: true, archetypes: getArchetypeArray()),
                                                Helpers.CreateAddFeatureOnClassLevel(feat_surprise, 7, classes, archetypes: getArchetypeArray()),
                                                Helpers.CreateAddFeatureOnClassLevel(feat3, 11, classes, archetypes: getArchetypeArray())
                                                );
            return feature;
        }


        public BlueprintFeature createTimeHop(string name_prefix, string display_name, string description)
        {
            // Note: dimension door is 50 feet in game.
            // Since time hop allows 10 ft/level, that'd work out to 1 resource per 5 levels.
            // But the average jump may be shorter, so 1 per 3 seems like a good compromise.
            var resource = Helpers.CreateAbilityResource($"{name_prefix}Resource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 3, 1, 3, 1, 0, 0, classes, getArchetypeArray());

            var dimension_door = library.CopyAndAdd<BlueprintAbility>("5bdc37e4acfa209408334326076a43bc", name_prefix +"MassAbility", "");
            dimension_door.Type = AbilityType.Supernatural;
            dimension_door.RemoveComponents<SpellComponent>();
            dimension_door.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(classes, getArchetypeArray(), stat));
            dimension_door.AddComponent(Helpers.CreateResourceLogic(resource, amount: 2));
            dimension_door.ActionType = CommandType.Move;
            var dimension_door_caster = library.CopyAndAdd<BlueprintAbility>("a9b8be9b87865744382f7c64e599aeb2", name_prefix + "CasterAbility", "");
            dimension_door_caster.Type = AbilityType.Supernatural;
            dimension_door_caster.RemoveComponents<SpellComponent>();
            dimension_door_caster.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(classes, getArchetypeArray(),stat));
            dimension_door_caster.AddComponent(Helpers.CreateResourceLogic(resource, amount: 1));
            dimension_door_caster.ActionType = CommandType.Move;

            dimension_door.SetName(display_name + " - Mass");
            var wrapper = Common.createVariantWrapper(name_prefix + "Ability", "", dimension_door_caster, dimension_door);
            wrapper.SetNameDescription(display_name, description);

            var feature = Common.AbilityToFeature(wrapper, hide: false);
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));

            addMinLevelPrerequisite(feature, 7);

            return feature;
        }


        public BlueprintFeature createRewindTime(string name_prefix, string display_name, string description)
        {
            // Note: will replace with toggle allowing to reroll corresponding d20

            var resource = Helpers.CreateAbilityResource($"{name_prefix}Resource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 7, 0, 4, 1, 0, 0, classes, getArchetypeArray());

            var buff = library.CopyAndAdd<BlueprintBuff>("3bc40c9cbf9a0db4b8b43d8eedf2e6ec", name_prefix + "Buff", "");
            buff.SetNameDescription(display_name, description);
            buff.ReplaceComponent<ModifyD20>(m => { m.DispellOnRerollFinished = true; m.RerollOnlyIfFailed = true; });

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1), dispellable: false);

            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                    display_name,
                                    description,
                                    "",
                                    buff.Icon,
                                    AbilityType.Supernatural,
                                    CommandType.Swift,
                                    AbilityRange.Personal,
                                    Helpers.oneRoundDuration,
                                    "",
                                    Helpers.CreateRunActions(apply_buff),
                                    Helpers.CreateResourceLogic(resource)
                                   );
            ability.setMiscAbilityParametersSelfOnly();


            var rule_types = new ModifyD20WithActions.RuleType[] { ModifyD20WithActions.RuleType.AttackRoll,
                                                                   ModifyD20WithActions.RuleType.Intiative,
                                                                   ModifyD20WithActions.RuleType.Maneuver,
                                                                   ModifyD20WithActions.RuleType.SavingThrow,
                                                                   ModifyD20WithActions.RuleType.SkillCheck,
                                                                   ModifyD20WithActions.RuleType.SpellResistance };
            var names = new string[] { "Attack Roll", "Initiative", "Combat Maneuver", "Saving Throw", "Skill", "Spell Resistance" };
            var abilities = new List<BlueprintActivatableAbility>();
            for (int i = 0; i < rule_types.Length; i++)
            {
                var buff2 = library.CopyAndAdd<BlueprintBuff>("3bc40c9cbf9a0db4b8b43d8eedf2e6ec", rule_types[i].ToString() + name_prefix + "Buff", "");
                buff2.SetNameDescription(display_name + ": " + names[i], description);
                buff2.RemoveComponents<ModifyD20>();
                buff2.AddComponent(Helpers.Create<NewMechanics.ModifyD20WithActions>(m =>
                                                                                    {
                                                                                        m.Rule = rule_types[i];
                                                                                        m.RollsAmount = 1;
                                                                                        m.TakeBest = true;
                                                                                        m.RerollOnlyIfFailed = true;
                                                                                        m.actions = Helpers.CreateActionList(Common.createContextActionSpendResource(resource, 1));
                                                                                        m.required_resource = resource;
                                                                                    })
                                                                                    );
                var ability2 = Helpers.CreateActivatableAbility(rule_types[i].ToString() + name_prefix + "ToggleAbility",
                                                               buff2.Name,
                                                               buff2.Description,
                                                               "",
                                                               buff2.Icon,
                                                               buff2,
                                                               AbilityActivationType.Immediately,
                                                               Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                               null,
                                                               Helpers.CreateActivatableResourceLogic(resource, ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                               );
                ability2.DeactivateImmediately = true;
                abilities.Add(ability2);
            }


            var feature = Common.AbilityToFeature(ability, hide: false);
            feature.ComponentsArray = new BlueprintComponent[] { Helpers.CreateAddFacts(abilities.ToArray()) };
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));

            addMinLevelPrerequisite(feature, 7);

            return feature;
        }


        public BlueprintFeature createTimeSight(string name_prefix, string display_name, string description)
        {
            var resource = Helpers.CreateAbilityResource($"{name_prefix}Resource", "", "", "", null);
            resource.SetIncreasedByLevel(0, 1, classes);

            var true_seeing_buff = library.Get<BlueprintBuff>("09b4b69169304474296484c74aa12027");
            var foresight_buff = library.Get<BlueprintBuff>("8c385a7610aa409468f3a6c0f904ac92");

            var apply_true_seeing = Common.createContextActionApplyBuff(true_seeing_buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false);
            var apply_foresight = Common.createContextActionApplyBuff(foresight_buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false);

            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                true_seeing_buff.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Personal,
                                                Helpers.oneMinuteDuration,
                                                "",
                                                Helpers.CreateRunActions(Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.Default),
                                                                                                                        Helpers.CreateActionList(apply_true_seeing),
                                                                                                                        Helpers.CreateActionList(apply_true_seeing, apply_foresight)
                                                                                                                        )
                                                                        ),
                                                createClassScalingConfig(progression: ContextRankProgression.Custom, customProgression: new (int, int)[] { (17, 1), (20, 2) }),
                                                Helpers.CreateResourceLogic(resource)
                                               );

            var feature = Common.AbilityToFeature(ability, hide: false);
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));
            addMinLevelPrerequisite(feature, 11);
            return feature;
        }


        public BlueprintFeature createEraseFromTime(string name_prefix, string display_name, string description)
        {
            var resource = Helpers.CreateAbilityResource($"{name_prefix}Resource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 11, 1, 100, 0, 0, 0, classes, getArchetypeArray());
            var buff = Helpers.CreateBuff(name_prefix + "Buff",
                                          display_name,
                                          description,
                                          "",
                                          LoadIcons.Image2Sprite.Create(@"AbilityIcons/TimeStop.png"),
                                          Common.createPrefabLink("eb0e36f1de0c05347963262d56d90cf5"), //hold person
                                          Helpers.Create<TImeStopMechanics.EraseFromTime>()
                                          );
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)));
            var erase_from_time_touch = Helpers.CreateAbility(name_prefix + "Ability",
                                                                buff.Name,
                                                                buff.Description,
                                                                "",
                                                                buff.Icon,
                                                                AbilityType.Supernatural,
                                                                CommandType.Standard,
                                                                AbilityRange.Touch,
                                                                "1 round/ 2 levels",
                                                                Helpers.fortNegates,
                                                                Helpers.CreateRunActions(SavingThrowType.Fortitude, Helpers.CreateConditionalSaved(null, apply_buff)),
                                                                createClassScalingConfig(progression: ContextRankProgression.Div2, min: 1),
                                                                Helpers.CreateDeliverTouch(),
                                                                Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(classes, getArchetypeArray(), stat)
                                                                );
            erase_from_time_touch.setMiscAbilityParametersTouchHarmful();
            var erase_from_time = Helpers.CreateTouchSpellCast(erase_from_time_touch);

            erase_from_time.AddComponent(Helpers.CreateResourceLogic(resource));
            var feature = Common.AbilityToFeature(erase_from_time, false);
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));

            return feature;
        }



    }

}
