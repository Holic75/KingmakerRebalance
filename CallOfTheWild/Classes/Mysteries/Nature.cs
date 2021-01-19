using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
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
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;


namespace CallOfTheWild
{
    partial class MysteryEngine
    {
        public BlueprintFeatureSelection createAnimalCompanion(string name_prefix, string display_name, string description)
        {
            var selection = library.CopyAndAdd<BlueprintFeatureSelection>("2ecd6c64683b59944a7fe544033bb533", name_prefix + "Selection", "");
            selection.SetNameDescription(display_name, description);
            selection.ComponentsArray = new BlueprintComponent[] { selection.ComponentsArray[1] };
            selection.Group = FeatureGroup.None;

            var companion_rank = library.Get<BlueprintFeature>("1670990255e4fe948a863bafd5dbda5d");
            for (int i = 2; i <= 20; i++)
            {
                selection.AddComponent(Helpers.CreateAddFeatureOnClassLevel(companion_rank, i, classes, archetypes: getArchetypeArray()));
            }

            selection.AddComponent(Helpers.PrerequisiteNoFeature(selection));

            return selection;
        }


        public BlueprintFeature createErosionTouch(string name_prefix, string display_name, string description)
        {
            var resource = Helpers.CreateAbilityResource($"{name_prefix}Resource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 3, 1, 3, 1, 0, 0, classes, getArchetypeArray());
            var construct_damage = Helpers.CreateActionDealDamage(DamageEnergyType.Magic, BalanceFixes.getDamageDie(DiceType.D6).CreateContextDiceValue(Helpers.CreateContextValue(AbilityRankType.DamageDice)));
            construct_damage.DamageType.Type = Kingmaker.RuleSystem.Rules.Damage.DamageType.Direct;

            var effect = Helpers.CreateConditional(Common.createContextConditionHasFact(Common.construct), construct_damage);

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
                                                 createClassScalingConfig(ContextRankProgression.AsIs, AbilityRankType.DamageDice),
                                                 Helpers.CreateRunActions(effect),
                                                 Common.createAbilitySpawnFx("9a38d742801be084d89bd34318c600e8", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                 Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(classes, getArchetypeArray(),stat),
                                                 Common.createAbilityTargetHasFact(true, Common.construct)
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


        public BlueprintFeature createFriendToAnimals(string name_prefix, string display_name, string description)
        {
            var icon = library.Get<BlueprintAbility>("c6147854641924442a3bb736080cfeb6").Icon; //sna I
            var spontnaeous_summon = library.Get<BlueprintFeature>("b296531ffe013c8499ad712f8ae97f6b");
            var animal = library.Get<BlueprintFeature>("a95311b3dc996964cbaa30ff9965aaf6");
            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                      display_name,
                                      description,
                                      "",
                                      icon,
                                      FeatureGroup.None);
            feature.Ranks = 1;

            var buff = Helpers.CreateBuff(name_prefix + "Buff",
                                          display_name,
                                          description,
                                          "",
                                          icon,
                                          null,
                                          Helpers.CreateAddContextStatBonus(StatType.SaveFortitude, ModifierDescriptor.Sacred),
                                          Helpers.CreateAddContextStatBonus(StatType.SaveReflex, ModifierDescriptor.Sacred),
                                          Helpers.CreateAddContextStatBonus(StatType.SaveWill, ModifierDescriptor.Sacred),
                                          Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, stat: stat,
                                                                          min: 0),
                                          Helpers.CreateAddFactContextActions(newRound: Helpers.CreateConditional(Common.createContextConditionHasFact(animal),
                                                                                                                  null,
                                                                                                                  Helpers.Create<ContextActionRemoveSelf>()
                                                                                                                  )
                                                                             )
                                         );

            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("7ced0efa297bd5142ab749f6e33b112b", name_prefix + "Area", "");
            area.Size = 30.Feet();
            
            area.ReplaceComponent<AbilityAreaEffectBuff>(a => { a.Buff = buff; a.Condition = Helpers.CreateConditionsCheckerOr(Common.createContextConditionHasFact(animal)); });

            var aura_buff = library.CopyAndAdd<BlueprintBuff>("c96380f6dcac83c45acdb698ae70ffc4", name_prefix + "AuraBuff", "");
            aura_buff.ReplaceComponent<AddAreaEffect>(a => a.AreaEffect = area);
            feature.AddComponent(Common.createAuraFeatureComponent(aura_buff));

            BlueprintAbility[] summon_nature_ally = new BlueprintAbility[]{library.TryGet<BlueprintAbility>("c6147854641924442a3bb736080cfeb6"),
                                                                             library.TryGet<BlueprintAbility>("298148133cdc3fd42889b99c82711986"),
                                                                             library.TryGet<BlueprintAbility>("fdcf7e57ec44f704591f11b45f4acf61"),
                                                                             library.TryGet<BlueprintAbility>("c83db50513abdf74ca103651931fac4b"),
                                                                             library.TryGet<BlueprintAbility>("8f98a22f35ca6684a983363d32e51bfe"),
                                                                             library.TryGet<BlueprintAbility>("55bbce9b3e76d4a4a8c8e0698d29002c"),
                                                                             library.TryGet<BlueprintAbility>("051b979e7d7f8ec41b9fa35d04746b33"),
                                                                             library.TryGet<BlueprintAbility>("ea78c04f0bd13d049a1cce5daf8d83e0"),
                                                                             library.TryGet<BlueprintAbility>("a7469ef84ba50ac4cbf3d145e3173f8e")
                                                                            };

            /*foreach (var c in classes)
            {
                for (int i = 0; i < summon_nature_ally.Length; i++)
                {
                    feature.AddComponents(Helpers.CreateAddKnownSpell(summon_nature_ally[i], c, i + 1));
                }
            }*/

            
            return feature;
        }


        public BlueprintFeature createLifeLich(string name_prefix, string display_name, string description)
        {
            var resource = Helpers.CreateAbilityResource($"{name_prefix}Resource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 11, 1, 4, 1, 0, 0, classes, getArchetypeArray());
            var spell = Common.convertToSuperNatural(library.Get<BlueprintAbility>("6cbb040023868574b992677885390f92"), name_prefix, classes, stat, resource, archetypes: getArchetypeArray());
            spell.RemoveComponents<AbilityDeliverTouch>();
            spell.SetNameDescription(display_name, description);

            spell.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, type: AbilityRankType.StatBonus, stat: stat));
            var actions = spell.GetComponent<AbilityEffectRunAction>().Actions.Actions;

            var new_actions = Common.changeAction<ContextActionApplyBuff>(actions, c => c.DurationValue = Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus), DurationRate.Hours));
            new_actions = Common.changeAction<ContextActionDealDamage>(new_actions, c => c.HalfIfSaved = true);

            spell.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(SavingThrowType.Fortitude, new_actions));
            spell.Range = AbilityRange.Close;
            spell.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            spell.LocalizedDuration = Helpers.CreateString(spell.name + ".Duration", $"Temporary hit points last number of hours equal to {stat.ToString()} modifier");

            var feature = Common.AbilityToFeature(spell, false);
            feature.AddComponent(resource.CreateAddAbilityResource());

            addMinLevelPrerequisite(feature, 7);

            return feature;
        }


        public BlueprintFeature createSpiritOfNature(string name_prefix, string display_name, string description)
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/SpiritOfNature.png");
            var buff = Helpers.CreateBuff(name_prefix + "Buff",
                                          display_name,
                                          description,
                                          "",
                                          icon,
                                          null,
                                          Common.createAddContextEffectFastHealing(Helpers.CreateContextValue(AbilityRankType.Default)),
                                          createClassScalingConfig(progression: ContextRankProgression.Custom, customProgression: new (int, int)[] { (14, 1), (20, 3) })
                                         );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(0, diceType: DiceType.D4, diceCount: 1), dispellable: false);
            var effect = Helpers.CreateConditional(new Condition[] {Common.createContextConditionHasFact(buff, has: false),
                                                                        Helpers.Create<NewMechanics.ContextConditionCompareTargetHPPercent>(c => c.Value = 25) },
                                                                    apply_buff);

            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                           buff.Name,
                                                           buff.Description,
                                                           "",
                                                           icon,
                                                           FeatureGroup.None,
                                                           Common.createIncomingDamageTrigger(effect)
                                                           //Helpers.CreateAddFactContextActions(newRound: effect)
                                                           );
            return feature;
        }


        public BlueprintFeature createFormOfTheBeast(string name_prefix, string display_name, string description)
        {
            var resource = Helpers.CreateAbilityResource($"{name_prefix}Resource", "", "", "", null);
            resource.SetFixedResource(1);

            var form_ids = new string[] {
                "61a7ed778dd93f344a5dacdbad324cc9", //beast shape I
                "5d4028eb28a106d4691ed1b92bbb1915", //beast shape II
                "9b93040dad242eb43ac7de6bb6547030", //beast shape III
                "940a545a665194b48b722c1f9dd78d53", //beast shape IV
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
                foreach (var v in spell.Variants)
                {
                    var ability = Common.convertToSuperNatural(v, name_prefix, classes, stat, resource, archetypes: getArchetypeArray());

                    var new_actions = Common.changeAction<ContextActionApplyBuff>(ability.GetComponent<AbilityEffectRunAction>().Actions.Actions, c => c.DurationValue = Helpers.CreateContextDuration(c.DurationValue.BonusValue, DurationRate.Hours));
                    ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(new_actions));
                    ability.LocalizedDuration = Helpers.CreateString(ability.name + ".Duration", Helpers.hourPerLevelDuration);
                    ability.SetName(display_name + " " + Common.roman_id[i + 1] + " " + ability.Name.Substring(ability.Name.IndexOf('(')));

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

            }
            addMinLevelPrerequisite(feature, 7);
            return feature;
        }


        public BlueprintFeature createGiftOfClawAndHorn(string name_prefix, string display_name, string description)
        {
            var icon = Helpers.GetIcon("75de4ded3e731dc4f84d978fe947dc67");
            var resource = Helpers.CreateAbilityResource($"{name_prefix}Resource", "", "", "", null);
            resource.SetIncreasedByStat(3, stat);

            var bite = library.Get<BlueprintItemWeapon>("35dfad6517f401145af54111be04d6cf");
            var gore = library.Get<BlueprintItemWeapon>("daf4ab765feba8548b244e174e7af5be");

            var bites = new BlueprintItemWeapon[5];
            var gores = new BlueprintItemWeapon[5];

            var apply_gore_buffs = new ActionList[5];
            var apply_bite_buffs = new ActionList[5];
            var apply_buffs = new ActionList[5];

            for (int i = 0; i < 5; i++)
            {
                bites[i] = library.CopyAndAdd<BlueprintItemWeapon>(bite.AssetGuid, name_prefix + $"Bite{i}", "");
                gores[i] = library.CopyAndAdd<BlueprintItemWeapon>(gore.AssetGuid, name_prefix + $"Gore{i}", "");

                if (i > 0)
                {
                    Common.addEnchantment(bites[i], WeaponEnchantments.standard_enchants[i - 1]);
                    Common.addEnchantment(gores[i], WeaponEnchantments.standard_enchants[i - 1]);
                }

                var bite_buff = Helpers.CreateBuff(name_prefix + $"Bite{i}Buff",
                                                   display_name + ": Bite",
                                                   description,
                                                   "",
                                                   icon,
                                                   null,
                                                   Common.createAddAdditionalLimb(bites[i])
                                                   );
                var gore_buff = Helpers.CreateBuff(name_prefix + $"Gore{i}Buff",
                                   display_name + ": Gore",
                                   description,
                                   "",
                                   icon,
                                   null,
                                   Common.createAddAdditionalLimb(gores[i])
                                   );

                var buff = Helpers.CreateBuff(name_prefix + $"{i}Buff",
                                   display_name,
                                   description,
                                   "",
                                   icon,
                                   null,
                                   Common.createAddAdditionalLimb(bites[i]),
                                   Common.createAddAdditionalLimb(gores[i])
                                   );

                var duration = Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default));
                apply_bite_buffs[i] = Helpers.CreateActionList(Common.createContextActionApplyBuff(bite_buff, duration, dispellable: false));
                apply_gore_buffs[i] = Helpers.CreateActionList(Common.createContextActionApplyBuff(gore_buff, duration, dispellable: false));
                apply_buffs[i] = Helpers.CreateActionList(Common.createContextActionApplyBuff(buff, duration, dispellable: false));

            }

            var fx = Common.createAbilitySpawnFx("352469f228a3b1f4cb269c7ab0409b8e", anchor: AbilitySpawnFxAnchor.SelectedTarget);
            var bite_spell = Helpers.CreateAbility(name_prefix + "BiteAbility",
                                                   display_name + ": Bite",
                                                   description,
                                                   "",
                                                   icon,
                                                   AbilityType.Supernatural,
                                                   CommandType.Swift,
                                                   AbilityRange.Personal,
                                                   "1 round/2 levels",
                                                   "",
                                                   Helpers.CreateRunActions(Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus), apply_bite_buffs)),
                                                   createClassScalingConfig(progression: ContextRankProgression.Div2, min: 1),
                                                   createClassScalingConfig(type: AbilityRankType.StatBonus, progression: ContextRankProgression.OnePlusDivStep, stepLevel: 5),
                                                   fx,
                                                   resource.CreateResourceLogic()
                                                   );
            bite_spell.setMiscAbilityParametersSelfOnly();
            var gore_spell = library.CopyAndAdd<BlueprintAbility>(bite_spell.AssetGuid, name_prefix + "GoreAbility", "");
            gore_spell.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus),
                                                                                         apply_gore_buffs)));
            gore_spell.SetName(display_name + ": Gore");

            var spell = library.CopyAndAdd<BlueprintAbility>(bite_spell.AssetGuid, name_prefix + "CombinedAbility", "");
            spell.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus),
                                                                                    apply_buffs)));
            spell.SetName(display_name);

            var gore_bite_wrapper = Common.createVariantWrapper(name_prefix + "BaseAbility", "", bite_spell, gore_spell);
            gore_bite_wrapper.SetName(display_name);


            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                display_name,
                                                description,
                                                "",
                                                icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddFeatureOnClassLevel(Common.AbilityToFeature(gore_bite_wrapper), 11, classes, before: true, archetypes: getArchetypeArray()),
                                                Helpers.CreateAddFeatureOnClassLevel(Common.AbilityToFeature(spell), 11, classes, archetypes: getArchetypeArray()),
                                                resource.CreateAddAbilityResource()
                                                );

            return feature;
        }


        public BlueprintFeature createNaturesWhispers(string name_prefix, string display_name, string description)
        {
            var icon = Helpers.GetIcon("e418c20c8ce362943a8025d82c865c1c");
            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                display_name,
                                                description,
                                                "",
                                                icon,
                                                FeatureGroup.None,
                                                Helpers.Create<StatReplacementMechanics.ReplaceACStat>(r => r.stat = stat),
                                                Helpers.Create<StatReplacementMechanics.ReplaceCMDStat>(r => r.stat = stat)
                                                );
            return feature;
        }

    }
}
