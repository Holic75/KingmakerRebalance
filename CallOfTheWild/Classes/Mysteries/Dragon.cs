using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Buffs.Blueprints;
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
        public BlueprintFeature createBreathWeapon(string name_prefix, string display_name, string description, BlueprintAbility breath_weapon_prototype)
        {
            var resource = Helpers.CreateAbilityResource(name_prefix + "Resource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 5, 1, 5, 1, 0, 0.0f, classes);

            var ability = library.CopyAndAdd<BlueprintAbility>(breath_weapon_prototype.AssetGuid, name_prefix + "Ability", "");
            ability.SetDescription(description);
            ability.RemoveComponents<ContextRankConfig>();
            ability.RemoveComponents<AbilityResourceLogic>();

            ability.AddComponent(Helpers.CreateResourceLogic(resource));
            ability.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                 progression: ContextRankProgression.Div2, min: 1,
                                                                 classes: classes));
            ability.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClasses(classes, stat));

            //at 20th level after revelation
            var ability2 = library.CopyAndAdd<BlueprintAbility>(ability.AssetGuid, name_prefix + "FinalAbility", "");
            ability2.RemoveComponents<AbilityResourceLogic>();

            var cooldown_buff = Helpers.CreateBuff(name_prefix + "CooldownBuff",
                                                   display_name + " Cooldown",
                                                   description,
                                                   "",
                                                   ability.Icon,
                                                   null);

            var apply_cooldown = Common.createContextActionApplyBuff(cooldown_buff, Helpers.CreateContextDuration(1, DurationRate.Rounds, DiceType.D4, 1), dispellable: false);
            ability2.AddComponent(Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionOnContextCaster(apply_cooldown))));
            ability2.AddComponent(Common.createAbilityCasterHasNoFacts(cooldown_buff));

            var feature1 = Common.AbilityToFeature(ability);
            feature1.AddComponent(Helpers.CreateAddAbilityResource(resource));
            var feature2 = Common.AbilityToFeature(ability2);
            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                display_name,
                                                description,
                                                "",
                                                ability.Icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddFeatureOnClassLevel(feature1, 20, classes, before: true),
                                                Helpers.CreateAddFeatureOnClassLevel(feature2, 20, classes)
                                                );

            return feature;
        }


        public BlueprintFeature createDraconicResistance(string name_prefix, string display_name, string description, DamageEnergyType energy, UnityEngine.Sprite icon)
        {
            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                display_name,
                                                description,
                                                "",
                                                icon,
                                                FeatureGroup.None,
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                                                                                ContextRankProgression.Custom, classes: classes,
                                                                                customProgression: new (int, int)[] {
                                                                                    (8, 1),
                                                                                    (14, 2),
                                                                                    (20, 4)
                                                                                }),
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                                                                                ContextRankProgression.Custom, AbilityRankType.StatBonus,
                                                                                classes: classes,
                                                                                customProgression: new (int, int)[] {
                                                                                    (8, 5),
                                                                                    (14, 10),
                                                                                    (20, 20)
                                                                                }),
                                                Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.NaturalArmor, ContextValueType.Rank),
                                                Helpers.Create<AddDamageResistanceEnergy>(a =>
                                                {
                                                    a.Type = energy;
                                                    a.Value = Helpers.CreateContextValueRank(AbilityRankType.StatBonus);
                                                })
            );

            return feature;
        }


        public BlueprintFeature createDragonMagic(string name_prefix, string display_name, string description)
        {
            //will give only one spell at most max_spell level -2
            var icon = Helpers.GetIcon("55edf82380a1c8540af6c6037d34f322"); // elven magic
            BlueprintFeatureSelection learn_selection = Helpers.CreateFeatureSelection(name_prefix + "SpellLevelSelection",
                                                                          display_name,
                                                                          description,
                                                                          "",
                                                                          icon,
                                                                          FeatureGroup.None);

            var wizard_spell_list = Common.combineSpellLists(name_prefix + "SpellList", library.Get<BlueprintSpellList>("ba0401fdeb4062f40a7aa95b6f07fe89"));
            Common.excludeSpellsFromList(wizard_spell_list, classes[0].Spellbook.SpellList);
            for (int i = 1; i <= 7; i++)
            {
                var learn_spell = library.CopyAndAdd<BlueprintParametrizedFeature>("bcd757ac2aeef3c49b77e5af4e510956", name_prefix + $"{i}ParametrizedFeature", "");
                learn_spell.SpellLevel = i;
                learn_spell.SpecificSpellLevel = true;
                learn_spell.SpellLevelPenalty = 0;
                learn_spell.SpellcasterClass = classes[0];
                learn_spell.SpellList = wizard_spell_list;
                learn_spell.ReplaceComponent<LearnSpellParametrized>(l => { l.SpellList = wizard_spell_list; l.SpecificSpellLevel = true; l.SpellLevel = i; l.SpellcasterClass = classes[0]; });
                learn_spell.AddComponents(Common.createPrerequisiteClassSpellLevel(classes[0], i + 2));
                learn_spell.SetName(Helpers.CreateString(name_prefix + $"ParametrizedFeature{i}.Name", display_name + $" (level {i})"));
                learn_spell.SetDescription(learn_selection.Description);
                learn_spell.SetIcon(learn_selection.Icon);
                learn_selection.AllFeatures = learn_selection.AllFeatures.AddToArray(learn_spell);
            }

            learn_selection.AddComponent(Helpers.PrerequisiteNoFeature(learn_selection));
            return learn_selection;
        }


        public BlueprintFeature createDragonSenses(string name_prefix, string display_name, string description)
        {
            // Note: this ability was reworked a bit, because the game does not have low-light vision/darkvision,
            // and it's tricky to code the "blindsense 30ft, or +30ft if you already had it".
            //
            // To capture the spirit of the PnP revelation, it now grants:
            //
            // +2 Perception
            // +4 Perception at level 5
            // Blindsense 30ft at level 11
            // Blindsense 60ft at level 15
            var blindsense30 = Helpers.CreateFeature(name_prefix + "Blindsense1Feature",
                                         "",
                                         "",
                                         "",
                                         null,
                                         FeatureGroup.None,
                                         Common.createBlindsense(30)
                                         );
            blindsense30.HideInUI = true;
            var blindsense60 = library.CopyAndAdd<BlueprintFeature>(blindsense30.AssetGuid, name_prefix + "Blindsense2Feature", "");
            blindsense60.ReplaceComponent<Blindsense>(b => b.Range = 60.Feet());            
            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                display_name,
                                                description,
                                                "",
                                                Helpers.GetIcon("82962a820ebc0e7408b8582fdc3f4c0c"), // sense vitals
                                                FeatureGroup.None,
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                                                                                ContextRankProgression.Custom, classes: classes,
                                                                                customProgression: new (int, int)[] {
                                                                                    (4, 2),
                                                                                    (20, 4)
                                                                                }),
                                                Helpers.CreateAddContextStatBonus(StatType.SkillPerception, ModifierDescriptor.UntypedStackable, ContextValueType.Rank),
                                                Helpers.CreateAddFeatureOnClassLevel(blindsense30, 11, classes),
                                                Helpers.CreateAddFeatureOnClassLevel(blindsense60, 15, classes)
                                                );
            return feature;
        }


        public BlueprintFeature createFormOfTheDragon(string name_prefix, string display_name, string description, string asset_guid1, string asset_guid2, string asset_guid3)
        {
            var resource = Helpers.CreateAbilityResource(name_prefix + "Resource", "", "", "", null);
            resource.SetFixedResource(1);

            var dragon_forms = new BlueprintAbility[4];
            dragon_forms[0] = Common.convertToSuperNatural(library.Get<BlueprintAbility>(asset_guid1), name_prefix, classes, stat, resource);
            dragon_forms[1] = Common.convertToSuperNatural(library.Get<BlueprintAbility>(asset_guid2), name_prefix, classes, stat, resource);
            dragon_forms[2] = Common.convertToSuperNatural(library.Get<BlueprintAbility>(asset_guid3), name_prefix, classes, stat, resource);
            dragon_forms[3] = Common.convertToSuperNatural(library.Get<BlueprintAbility>(asset_guid1), name_prefix + "Long", classes, stat, resource);

            var dragon_form_features = new BlueprintFeature[4];
            for (int i = 0; i < dragon_forms.Length; i++)
            {
                var actions = Common.changeAction<ContextActionApplyBuff>(dragon_forms[i].GetComponent<AbilityEffectRunAction>().Actions.Actions,
                                                                          c => c.DurationValue = Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default),
                                                                                                                                 i < 3 ? DurationRate.TenMinutes : DurationRate.Hours)
                                                                         );
                dragon_forms[i].ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(actions));
                dragon_forms[i].ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: classes));
                dragon_forms[i].LocalizedDuration = Helpers.CreateString(dragon_forms[i].name + ".Duration", i < 3 ? Helpers.tenMinPerLevelDuration : Helpers.hourPerLevelDuration);
                dragon_form_features[i] = Common.AbilityToFeature(dragon_forms[i]);
            }


            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                display_name,
                                                description,
                                                "",
                                                dragon_forms[0].Icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddFeatureOnClassLevel(dragon_form_features[0], 15, classes, before: true),
                                                Helpers.CreateAddFeatureOnClassLevel(dragon_form_features[1], 15, classes),
                                                Helpers.CreateAddFeatureOnClassLevel(dragon_form_features[2], 19, classes),
                                                Helpers.CreateAddFeatureOnClassLevel(dragon_form_features[3], 15, classes),
                                                Helpers.CreateAddAbilityResource(resource)
                                                );
            foreach (var c in classes)
            {
                feature.AddComponents(Helpers.PrerequisiteClassLevel(c, 11, any: true));
            }
            return feature;
        }


        public BlueprintFeature createPresenceOfDragons(string name_prefix, string display_name, string description)
        {
            var resource = Helpers.CreateAbilityResource(name_prefix + "Resource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 5, 1, 5, 1, 0, 0.0f, classes);
            var icon = Helpers.GetIcon("41cf93453b027b94886901dbfc680cb9"); // overwhelming presence

            var shaken = library.Get<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220");
            var immunity = Helpers.CreateBuff(name_prefix + "Immunity",
                                              display_name + " Immunity",
                                              description,
                                              "",
                                              icon,
                                              null);

            var apply_immunity = Common.createContextActionApplyBuff(immunity, Helpers.CreateContextDuration(1, DurationRate.Days), dispellable: false);
            var apply_shaken = Common.createContextActionApplyBuff(shaken, Helpers.CreateContextDuration(0, DurationRate.Rounds, DiceType.D6, 2), dispellable: false);

            var effect = Helpers.CreateConditional(Common.createContextConditionHasFact(immunity),
                                                   null,
                                                   Helpers.CreateConditionalSaved(apply_immunity, apply_shaken)
                                                   );

            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                icon,
                                                AbilityType.Supernatural,
                                                CommandType.Swift,
                                                AbilityRange.Personal,
                                                "2d6 rounds",
                                                Helpers.willNegates,
                                                Helpers.CreateRunActions(SavingThrowType.Will, effect),
                                                Helpers.CreateAbilityTargetsAround(30.Feet(), TargetType.Enemy),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.Fear | SpellDescriptor.MindAffecting),
                                                Helpers.CreateResourceLogic(resource),
                                                Common.createContextCalculateAbilityParamsBasedOnClasses(classes, stat)
                                                );
            ability.setMiscAbilityParametersSelfOnly();

            var feature = Common.AbilityToFeature(ability, false);
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));

            return feature;
        }


        public BlueprintFeature createScaledToughness(string name_prefix, string display_name, string description)
        {
            var barkskin = library.Get<BlueprintBuff>("533592a86adecda4e9fd5ed37a028432");
            var icon = barkskin.Icon;
            var resource = Helpers.CreateAbilityResource(name_prefix + "Resource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 13, 1, 13, 0, 0, 0.0f, classes);

            var buff = Helpers.CreateBuff($"{name_prefix}Buff",
                                         display_name,
                                         description,
                                         "",
                                         icon,
                                         barkskin.FxOnStart,
                                         UnitCondition.Sleeping.CreateImmunity(),
                                         UnitCondition.Paralyzed.CreateImmunity(),
                                         (SpellDescriptor.Sleep | SpellDescriptor.Paralysis).CreateBuffImmunity(),
                                         Common.createMagicDR(10)
                                        );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);

            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                icon,
                                                AbilityType.Supernatural,
                                                CommandType.Swift,
                                                AbilityRange.Personal,
                                                Helpers.roundsPerLevelDuration,
                                                "",
                                                Helpers.CreateRunActions(apply_buff),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: classes),
                                                Helpers.CreateResourceLogic(resource)
                                                );

            var feature = Common.AbilityToFeature(ability);
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));

            foreach (var c in classes)
            {
                feature.AddComponents(Helpers.PrerequisiteClassLevel(c, 7, any: true));
            }

            return feature;
        }


        public BlueprintFeature createWingsOfDragon(string name_prefix, BlueprintActivatableAbility wings_ability)
        {
            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                      wings_ability.Name,
                                      wings_ability.Description,
                                      "",
                                      wings_ability.Icon,
                                      FeatureGroup.None,
                                      Helpers.CreateAddFact(wings_ability));
            foreach (var c in classes)
            {
                feature.AddComponents(Helpers.PrerequisiteClassLevel(c, 11, any: true));
            }
            return feature;
        }



    }
}
