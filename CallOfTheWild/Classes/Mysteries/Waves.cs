using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
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
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;


namespace CallOfTheWild
{
    partial class MysteryEngine
    {
        public BlueprintFeature createBlizzard(string name_prefix, string display_name, string description)
        {
            var resource = Helpers.CreateAbilityResource($"{name_prefix}Resource", "", "", "", null);
            resource.SetFixedResource(1);

            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/SleetStorm.png");

            var buff = Helpers.CreateBuff(name_prefix + "Buff",
                              display_name,
                              description,
                              "",
                              icon,
                              null,
                              Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.DifficultTerrain),
                              Helpers.Create<AddConcealment>(c => { c.Concealment = Concealment.Total; c.Descriptor = ConcealmentDescriptor.Fog; c.CheckDistance = true; c.DistanceGreater = 5.Feet(); }),
                              Helpers.Create<ConcealementMechanics.AddOutgoingConcealment>(c => { c.Concealment = Concealment.Total; c.Descriptor = ConcealmentDescriptor.Fog; c.CheckDistance = true; c.DistanceGreater = 5.Feet(); }),
                              Helpers.Create<ConcealementMechanics.AddOutgoingConcealment>(c => { c.Concealment = Concealment.Partial; c.Descriptor = ConcealmentDescriptor.Fog; }),
                              Helpers.Create<AddConcealment>(c => { c.Concealment = Concealment.Partial; c.Descriptor = ConcealmentDescriptor.Fog; })
                              );


            var can_not_move_buff = Helpers.CreateBuff(name_prefix + "CanNotMoveBuff",
                                                      $"{display_name} (Can Not Move)",
                                                      "You can not move through the blizzard.",
                                                      "",
                                                      icon,
                                                      null,
                                                      Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.CantMove)
                                                      );

            can_not_move_buff.Stacking = StackingType.Replace;


            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("6ea87a0ff5df41c459d641326f9973d5", name_prefix + "Area", "");

            var apply_can_not_move = Common.createContextActionApplyBuff(can_not_move_buff, Helpers.CreateContextDuration(1), dispellable: false, is_child: true);

            var damage = Helpers.CreateActionDealDamage(DamageEnergyType.Cold, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D4), Helpers.CreateContextValue(AbilityRankType.DamageDice)),
                                                        isAoE: true, halfIfSaved: true);
            var damage_save = Common.createContextActionSavingThrow(SavingThrowType.Reflex,
                                                                    Helpers.CreateActionList(damage));
            var check_movement = Helpers.CreateConditional(new Condition[]{Helpers.Create<CombatManeuverMechanics.ContextConditionTargetSizeLessOrEqual>(c => c.target_size = Size.Medium),
                                                                           Helpers.CreateConditionHasFact(NewSpells.immunity_to_wind, not: true)},
                                                                           Common.createContextActionSkillCheck(StatType.Strength,
                                                                                                                failure: Helpers.CreateActionList(apply_can_not_move),
                                                                                                                custom_dc: 10)
                                                           );

            var actions = Helpers.CreateActionList(damage_save, check_movement);
            area.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.Create<NewMechanics.AbilityAreaEffectRunActionWithFirstRound>(a =>
                {
                    a.UnitEnter = actions;
                    a.Round = actions;
                    a.FirstRound = actions;
                }),
                Helpers.Create<AbilityAreaEffectBuff>(a => {a.Buff = buff; a.Condition = Helpers.CreateConditionsCheckerOr(); }),
               createClassScalingConfig(type: AbilityRankType.DamageDice),
            };


            var spawn_area = Common.createContextActionSpawnAreaEffect(area, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)));


            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                     buff.Name,
                                                     buff.Description,
                                                     "",
                                                     icon,
                                                     AbilityType.Supernatural,
                                                     UnitCommand.CommandType.Standard,
                                                     AbilityRange.Close,
                                                     $"1 round/{stat.ToString()} modifier",
                                                     "",
                                                     Helpers.CreateRunActions(spawn_area),
                                                     Common.createAbilityAoERadius(20.Feet(), TargetType.Any),
                                                     Helpers.CreateSpellDescriptor(SpellDescriptor.Cold),
                                                     Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, stat: stat),
                                                     Helpers.CreateResourceLogic(resource),
                                                     Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(classes, getArchetypeArray(),stat)
                                                     );
            ability.setMiscAbilityParametersRangedDirectional();
            var feature = Common.AbilityToFeature(ability, false);
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));
            addMinLevelPrerequisite(feature, 11);
            return feature;
        }


        public BlueprintFeature createFluidNature(string name_prefix, string display_name, string description)
        {
            var dodge = library.Get<BlueprintFeature>("97e216dbb46ae3c4faef90cf6bbe6fd5");

            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                display_name,
                                                description,
                                                "",
                                                dodge.Icon,
                                                FeatureGroup.None,
                                                Common.createCriticalConfirmationACBonus(4),
                                                Common.createManeuverDefenseBonus(Kingmaker.RuleSystem.Rules.CombatManeuver.BullRush, 4),
                                                Common.createManeuverDefenseBonus(Kingmaker.RuleSystem.Rules.CombatManeuver.Grapple, 4),
                                                Common.createManeuverDefenseBonus(Kingmaker.RuleSystem.Rules.CombatManeuver.Trip, 4),
                                                Helpers.CreateAddFeatureOnClassLevel(dodge, 5, classes, archetypes: getArchetypeArray())
                                                );

            return feature;
        }


        public BlueprintFeature createFreezingSpells(string name_prefix, string display_name, string description)
        {
            var icon = library.Get<BlueprintAbility>("e377feb2ecec95e478e0565da621ea55").Icon; //cold wall

            var slowed = library.Get<BlueprintBuff>("488e53ede2802ff4da9372c6a494fb66");
            var apply_slowed1 = Common.createContextActionApplyBuff(slowed, Helpers.CreateContextDuration(1));
            var apply_slowed2 = Common.createContextActionApplyBuff(slowed, Helpers.CreateContextDuration(0, DurationRate.Rounds, DiceType.D4, 1));
            var feature1 = Helpers.CreateFeature(name_prefix + "1Feature",
                                                display_name,
                                                description,
                                                "",
                                                icon,
                                                FeatureGroup.None,
                                                Helpers.Create<NewMechanics.ActionOnSpellDamage>(a =>
                                                                                                {
                                                                                                    a.descriptor = SpellDescriptor.Cold;
                                                                                                    a.use_existing_save = true;
                                                                                                    a.action_only_on_save = true;
                                                                                                    a.action = Helpers.CreateActionList(apply_slowed1);
                                                                                                }
                                                                                                )
                                                );
            feature1.HideInCharacterSheetAndLevelUp = true;
            var feature2 = Helpers.CreateFeature(name_prefix + "2Feature",
                                    display_name,
                                    description,
                                    "",
                                    icon,
                                    FeatureGroup.None,
                                    Helpers.Create<NewMechanics.ActionOnSpellDamage>(a =>
                                                                                    {
                                                                                        a.descriptor = SpellDescriptor.Cold;
                                                                                        a.use_existing_save = true;
                                                                                        a.action_only_on_save = true;
                                                                                        a.action = Helpers.CreateActionList(apply_slowed2);
                                                                                    }
                                                                                    )
                                    );
            feature2.HideInCharacterSheetAndLevelUp = true;
            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                display_name,
                                                description,
                                                "",
                                                icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddFeatureOnClassLevel(feature1, 11, classes, before: true, archetypes: getArchetypeArray()),
                                                Helpers.CreateAddFeatureOnClassLevel(feature2, 11, classes, archetypes: getArchetypeArray())
                                                );

            return feature;
        }

        public BlueprintFeature createIceArmor(string name_prefix, string display_name, string description)
        {
            var mage_armor = library.Get<BlueprintAbility>("9e1ad5d6f87d19e4d8883d63a6e35568");
            var buff = Helpers.CreateBuff(name_prefix + "Buff",
                                          display_name,
                                          description,
                                          "",
                                          mage_armor.Icon,
                                          null,
                                          Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.Armor, rankType: AbilityRankType.Default, multiplier: 2),
                                          createClassScalingConfig(progression: ContextRankProgression.StartPlusDivStep, stepLevel: 4, startLevel: -1, min: 2)
                                         );
            var buff2 = Helpers.CreateBuff(name_prefix + "2Buff",
                              display_name,
                              description,
                              "",
                              buff.Icon,
                              null,
                              Common.createContextFormDR(5, PhysicalDamageForm.Piercing)
                             );
            buff2.SetBuffFlags(BuffFlags.HiddenInUi);

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1, DurationRate.Hours), dispellable: false);
            var apply_buff2 = Common.createContextActionApplyBuff(buff2, Helpers.CreateContextDuration(1, DurationRate.Hours), dispellable: false);
            var resource = Helpers.CreateAbilityResource(name_prefix + "Resource", "", "", "", null);
            resource.SetIncreasedByLevel(0, 1, classes, getArchetypeArray());

            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Personal,
                                                "One hour",
                                                "",
                                                mage_armor.GetComponent<AbilitySpawnFx>(),
                                                Helpers.CreateRunActions(Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus),
                                                                                                                        Helpers.CreateActionList(apply_buff),
                                                                                                                        Helpers.CreateActionList(apply_buff, apply_buff2)
                                                                                                                        )
                                                                        ),
                                                createClassScalingConfig(type: AbilityRankType.StatBonus, progression: ContextRankProgression.OnePlusDivStep,
                                                                                stepLevel: 13),
                                                Helpers.CreateResourceLogic(resource)
                                                );
            ability.setMiscAbilityParametersSelfOnly();
            var feature = Common.AbilityToFeature(ability);
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));

            return feature;
        }


        public BlueprintFeature createIcySkin(string name_prefix, string display_name, string description)
        {
            var icon = Helpers.GetIcon("5368cecec375e1845ae07f48cdc09dd1"); //resist cold

            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                   display_name,
                                                   description,
                                                   "",
                                                   icon,
                                                   FeatureGroup.None,
                                                   createClassScalingConfig(ContextRankProgression.Custom,
                                                                                  customProgression: new (int, int)[] {
                                                                                                                        (4, 5),
                                                                                                                        (10, 10),
                                                                                                                        (20, 20)
                                                                                                                      }),
                                                   Helpers.Create<AddDamageResistanceEnergy>(a =>
                                                   {
                                                       a.Type = DamageEnergyType.Cold;
                                                       a.Value = Helpers.CreateContextValueRank();
                                                   }
                                                                                            )
                                                   );

            var immunity = Helpers.CreateFeature(name_prefix + "ImmunityFeature",
                                                     display_name,
                                                     description,
                                                     "",
                                                     icon,
                                                     FeatureGroup.None,
                                                     Helpers.Create<AddEnergyDamageImmunity>(a => a.EnergyType = DamageEnergyType.Cold)
                                                     );

            feature.AddComponent(Helpers.CreateAddFeatureOnClassLevel(immunity, 17, classes, archetypes: getArchetypeArray()));
            return feature;
        }


        public BlueprintFeature createPunitiveTransformation(string name_prefix, string display_name, string description)
        {
            var resource = Helpers.CreateAbilityResource(name_prefix + "Resource", "", "", "", null);
            resource.SetIncreasedByStat(0, stat);

            var buff = library.CopyAndAdd<BlueprintBuff>("0a52d8761bfd125429842103aed48b90", name_prefix + "Buff", "");
            buff.AddComponent(Helpers.Create<UniqueBuff>());
            buff.SetNameDescription("", "");

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);

            var baleful_polymorph = library.Get<BlueprintAbility>("3105d6e9febdc3f41a08d2b7dda1fe74");
            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Close,
                                                Helpers.roundsPerLevelDuration,
                                                Helpers.fortNegates,
                                                Helpers.CreateRunActions(SavingThrowType.Fortitude, Helpers.CreateConditionalSaved(null, apply_buff)),
                                                createClassScalingConfig(),
                                                Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(classes, getArchetypeArray(),stat),
                                                baleful_polymorph.GetComponent<AbilityTargetHasFact>(),
                                                baleful_polymorph.GetComponent<SpellDescriptorComponent>(),
                                                baleful_polymorph.GetComponent<AbilitySpawnFx>(),
                                                Helpers.CreateResourceLogic(resource)
                                                );

            ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);

            var feature = Common.AbilityToFeature(ability, false);
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));

            addMinLevelPrerequisite(feature, 7);
            return feature;
        }

        public BlueprintFeature createWaterForm(string name_prefix, string display_name, string description)
        {
            var resource = Helpers.CreateAbilityResource($"{name_prefix}Resource", "", "", "", null);
            resource.SetFixedResource(1);

            var form_ids = new string[] {
                "2d46a6f1dbef51f4c9e14fc163ee4124",
                "935b63be93800394f8f7ae17060b041a",
                "c82a0d6472794d245a186eff5d6f0f41",
                "96d2ab91f2d2329459a8dab496c5bede"
            };

            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                display_name,
                                                description,
                                                "",
                                                Helpers.GetIcon(form_ids[0]),
                                                FeatureGroup.None,
                                                Helpers.CreateAddAbilityResource(resource)
                                                );


            for (int i = 0; i < form_ids.Length; i++)
            {
                var spell = library.Get<BlueprintAbility>(form_ids[i]);
                var ability = Common.convertToSuperNatural(spell, name_prefix, classes, stat, resource, archetypes: getArchetypeArray());

                var new_actions = Common.changeAction<ContextActionApplyBuff>(ability.GetComponent<AbilityEffectRunAction>().Actions.Actions, c => c.DurationValue = Helpers.CreateContextDuration(c.DurationValue.BonusValue, DurationRate.Hours));
                ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(new_actions));
                ability.LocalizedDuration = Helpers.CreateString(ability.name + ".Duration", Helpers.hourPerLevelDuration);
                ability.SetName(display_name + " " + Common.roman_id[i + 1]);
                if (i == 0)
                {
                    feature.AddComponent(Helpers.CreateAddFact(ability));
                }
                else
                {
                    var feat = Common.AbilityToFeature(ability);
                    feature.AddComponent(Helpers.CreateAddFeatureOnClassLevel(feat, 7 + i * 2, classes, archetypes: getArchetypeArray()));
                }

            }
            addMinLevelPrerequisite(feature, 7);
            return feature;
        }


        public BlueprintFeature createWintryTouch(string name_prefix, string display_name, string description)
        {
            var resource = Helpers.CreateAbilityResource(name_prefix + "Resource", "", "", "", null);
            resource.SetIncreasedByStat(3, stat);

            var touch = library.CopyAndAdd<BlueprintAbility>("5e1db2ef80ff361448549beeb7785791", name_prefix + "Ability", ""); //water domain icicle 
            touch.RemoveComponents<AbilityResourceLogic>();
            touch.ReplaceComponent<AbilityDeliverProjectile>(Helpers.CreateDeliverTouch());
            touch.setMiscAbilityParametersTouchHarmful();
            touch.Type = AbilityType.Supernatural;
            touch.Range = AbilityRange.Touch;
            touch.SpellResistance = false;
            touch.AddComponent(Common.createAbilitySpawnFx("274fbd84b4c9d794bb5fe677472292b1", anchor: AbilitySpawnFxAnchor.SelectedTarget));
            touch.SetNameDescriptionIcon(display_name,
                                         description,
                                         Helpers.GetIcon("c83447189aabc72489164dfc246f3a36") //frigid touch
                                         );
            touch.ReplaceComponent<ContextRankConfig>(createClassScalingConfig(ContextRankProgression.Div2));
            var touch_sticky = Helpers.CreateTouchSpellCast(touch, resource);
            

            var frost = library.Get<BlueprintWeaponEnchantment>("421e54078b7719d40915ce0672511d0b");

            var frost_weapon_feature = Helpers.CreateFeature(name_prefix + "WeaponFeature",
                                                          "",
                                                          "",
                                                          "",
                                                          null,
                                                          FeatureGroup.None,
                                                          Helpers.Create<NewMechanics.EnchantmentMechanics.PersistentWeaponEnchantment>(p => p.enchant = frost)
                                                          );

            frost_weapon_feature.HideInCharacterSheetAndLevelUp = true;

            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                   display_name,
                                                   description,
                                                   "",
                                                   touch.Icon,
                                                   FeatureGroup.None,
                                                   Helpers.CreateAddFact(touch_sticky),
                                                   Helpers.CreateAddAbilityResource(resource),
                                                   Helpers.CreateAddFeatureOnClassLevel(frost_weapon_feature, 11, classes, archetypes: getArchetypeArray())
                                                   );
            return feature;
        }
    }
}
