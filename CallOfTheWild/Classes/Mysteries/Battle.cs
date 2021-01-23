using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    partial class MysteryEngine
    {
        public BlueprintFeature createBattleCry(string name_prefix, string display_name, string description)
        {
            var bless = library.Get<BlueprintAbility>("90e59f4a4ada87243b7b3535a06d0638");
            var resource = Helpers.CreateAbilityResource(name_prefix + "Resource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 5, 1, 5, 1, 0, 0.0f, classes, getArchetypeArray());
            var rage = library.Get<BlueprintBuff>("a1ffec0ce7c167a40aaea13dc49b757b");
            var buff = Helpers.CreateBuff(name_prefix + "EffectBuff",
                                          display_name,
                                          description,
                                          "",
                                          rage.Icon,
                                          rage.FxOnStart,
                                          Helpers.Create<BuffAllSkillsBonus>(b => { b.Descriptor = ModifierDescriptor.Morale; b.Value = 1; b.Multiplier = Helpers.CreateContextValue(AbilityRankType.StatBonus); }),
                                          Helpers.CreateAddContextStatBonus(StatType.SaveWill, ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus),
                                          Helpers.CreateAddContextStatBonus(StatType.SaveFortitude, ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus),
                                          Helpers.CreateAddContextStatBonus(StatType.SaveReflex, ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus),
                                          Helpers.CreateAddContextStatBonus(StatType.AdditionalAttackBonus, ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus),
                                          Helpers.CreateAddContextStatBonus(StatType.AdditionalDamage, ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus),
                                          createClassScalingConfig(progression: ContextRankProgression.OnePlusDivStep, stepLevel: 10, type: AbilityRankType.StatBonus, max: 2)
                                         );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);
            var ability = Helpers.CreateAbility($"{name_prefix}Ability",
                                                display_name,
                                                description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Extraordinary,
                                                CommandType.Standard,
                                                AbilityRange.Personal,
                                                $"1 round/{stat.ToString()} modifier",
                                                "",
                                                bless.GetComponent<AbilitySpawnFx>(),
                                                Helpers.CreateAbilityTargetsAround(50.Feet(), Kingmaker.UnitLogic.Abilities.Components.TargetType.Ally),
                                                Helpers.CreateResourceLogic(resource),
                                                Helpers.CreateRunActions(apply_buff),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, stat: stat)
                                                );
            ability.setMiscAbilityParametersSelfOnly();

            var feature = Common.AbilityToFeature(ability, false);
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));
            return feature;
        }



        public BlueprintFeature createCombatHealer(string name_prefix, string display_name, string description)
        {
            var icon = Helpers.GetIcon("6b90c773a6543dc49b2505858ce33db5"); // cure moderate wounds
   
            var resource = Helpers.CreateAbilityResource($"{name_prefix}Resource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(0, 7, 1, 4, 1, 0, 0, classes, getArchetypeArray());

            var cure_spells = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("5590652e1c2225c4ca30c4a699ab3649"),
                library.Get<BlueprintAbility>("6b90c773a6543dc49b2505858ce33db5"),
                library.Get<BlueprintAbility>("3361c5df793b4c8448756146a88026ad"),
                library.Get<BlueprintAbility>("41c9016596fe1de4faf67425ed691203"),
                library.Get<BlueprintAbility>("5d3d689392e4ff740a761ef346815074"),
                library.Get<BlueprintAbility>("571221cc141bc21449ae96b3944652aa"),
                library.Get<BlueprintAbility>("0cea35de4d553cc439ae80b3a8724397"),
                library.Get<BlueprintAbility>("1f173a16120359e41a20fc75bb53d449"),
            };

            var buff = Helpers.CreateBuff($"{name_prefix}Buff",
                                          display_name,
                                          description,
                                          "",
                                          icon,
                                          null,
                                          Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicOnSpellList>(m =>
                                                                                                              {
                                                                                                                  m.spell_list = cure_spells;
                                                                                                                  m.Metamagic = Kingmaker.UnitLogic.Abilities.Metamagic.Quicken;
                                                                                                                  m.resource = resource;
                                                                                                                  m.consume_extra_spell_slot = true;
                                                                                                                  m.amount = 1;
                                                                                                              }
                                                                                                              )
                                          );


            var ability = Helpers.CreateActivatableAbility($"{name_prefix}ToggleAbility",
                                                           display_name,
                                                           description,
                                                           "",
                                                           icon,
                                                           buff,
                                                           Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.Immediately,
                                                           CommandType.Free,
                                                           null,
                                                           Helpers.CreateActivatableResourceLogic(resource, ResourceSpendType.Never)
                                                           );
            ability.DeactivateImmediately = true;


            var feature = Common.ActivatableAbilityToFeature(ability, false);
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));
            addMinLevelPrerequisite(feature, 7);
            return feature;
        }


        public BlueprintFeature createIronSkin(string name_prefix, string display_name, string description)
        {
            var stoneskin = library.Get<BlueprintAbility>("c66e86905f7606c4eaa5c774f0357b2b");
            var resource = Helpers.CreateAbilityResource($"{name_prefix}Resource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 15, 1, 10, 0, 0, 0, classes, getArchetypeArray());

            var ability = Common.convertToSuperNatural(stoneskin, name_prefix, classes, stat, resource, archetypes: getArchetypeArray());
            ability.SetNameDescription(display_name, description);
            ability.Range = AbilityRange.Personal;
            ability.setMiscAbilityParametersSelfOnly();

            var feature = Common.AbilityToFeature(ability, false);
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));

            addMinLevelPrerequisite(feature, 11);

            return feature;
        }


        public BlueprintFeature createManeuverMastery(string name_prefix, string display_name, string description)
        {
            var maneuvers = new CombatManeuver[][] { new CombatManeuver[] { CombatManeuver.BullRush },
                                                     new CombatManeuver[] {CombatManeuver.Disarm },
                                                     new CombatManeuver[] {CombatManeuver.Trip },
                                                     new CombatManeuver[] {CombatManeuver.SunderArmor },
                                                     new CombatManeuver[] {CombatManeuver.DirtyTrickBlind, CombatManeuver.DirtyTrickEntangle, CombatManeuver.DirtyTrickSickened }
                                                   };
            var feat_ids = new (string, string)[]
            {
                ("b3614622866fe7046b787a548bbd7f59", "72ba6ad46d94ecd41bad8e64739ea392"), //bull rush
                ("25bc9c439ac44fd44ac3b1e58890916f", "63d8e3a9ab4d72e4081a7862d7246a79"), //disarm
                ("0f15c6f70d8fb2b49aa6cc24239cc5fa", "4cc71ae82bdd85b40b3cfe6697bb7949"), //trip
                ("9719015edcbf142409592e2cbaab7fe1", "54d824028117e884a8f9356c7c66149b"), //sunder
                ("ed699d64870044b43bb5a7fbe3f29494", "52c6b07a68940af41b270b3710682dc7"), //dirty trick
            };

            var names = new string[] { "Bull Rush", "Disarm", "Trip", "Sunder", "Dirty Trick" };
            var icons = new UnityEngine.Sprite[]
            {
                library.Get<BlueprintFeature>("b3614622866fe7046b787a548bbd7f59").Icon,
                library.Get<BlueprintFeature>("25bc9c439ac44fd44ac3b1e58890916f").Icon,
                library.Get<BlueprintFeature>("0f15c6f70d8fb2b49aa6cc24239cc5fa").Icon,
                library.Get<BlueprintFeature>("9719015edcbf142409592e2cbaab7fe1").Icon,
                library.Get<BlueprintFeature>("ed699d64870044b43bb5a7fbe3f29494").Icon,
            };

            var maneuver_mastery = Helpers.CreateFeatureSelection(name_prefix + "FeatureSelection",
                                                               display_name,
                                                               description,
                                                               "",
                                                               null,
                                                               FeatureGroup.None);
            for (int i = 0; i < maneuvers.Length; i++)
            {
                var feat = Helpers.CreateFeature(name_prefix + maneuvers[i][0].ToString() + "Feature",
                                                 maneuver_mastery.Name + ": " + names[i],
                                                 maneuver_mastery.Description,
                                                 "",
                                                 icons[i],
                                                 FeatureGroup.MagusArcana,
                                                 createClassScalingConfig(progression: ContextRankProgression.StartPlusDivStep,
                                                                            startLevel: 1,
                                                                            stepLevel: 4),
                                                 Helpers.CreateAddFeatureOnClassLevel(library.Get<BlueprintFeature>(feat_ids[i].Item1), 7, classes, archetypes: getArchetypeArray()),
                                                 Helpers.CreateAddFeatureOnClassLevel(library.Get<BlueprintFeature>(feat_ids[i].Item2), 11, classes, archetypes: getArchetypeArray())
                                                 );

                foreach (var maneuver in maneuvers[i])
                {
                    feat.AddComponent(Helpers.Create<CombatManeuverMechanics.SpecificCombatManeuverBonusUnlessHasFacts>(s => { s.maneuver_type = maneuver; s.Value = Helpers.CreateContextValue(AbilityRankType.Default); }));
                }
                maneuver_mastery.AllFeatures = maneuver_mastery.AllFeatures.AddToArray(feat);
            }
            maneuver_mastery.AddComponent(Helpers.PrerequisiteNoFeature(maneuver_mastery));

            return maneuver_mastery;
        }


        public BlueprintFeature createSkillAtArms(string name_prefix, string display_name, string description)
        {
            var heavy_armor = library.Get<BlueprintFeature>("1b0f68188dcc435429fb87a022239681");
            var martial = library.Get<BlueprintFeature>("203992ef5b35c864390b4e4a1e200629");
            var scalemail = library.Get<BlueprintItemArmor>("d7963e1fcf260c148877afd3252dbc91");
            var feat = Helpers.CreateFeature(name_prefix + "Feature", 
                                            display_name,
                                            description,
                                            "",
                                            heavy_armor.Icon,
                                            FeatureGroup.None,
                                            Helpers.CreateAddFacts(heavy_armor, martial),
                                            Helpers.Create<AddStartingEquipment>(a =>
                                            {
                                                a.CategoryItems = Array.Empty<WeaponCategory>();
                                                a.RestrictedByClass = Array.Empty<BlueprintCharacterClass>();
                                                a.BasicItems = new BlueprintItem[] { scalemail };
                                            }));
            return feat;
        }


        public BlueprintFeature createSurprisingCharge(string name_prefix, string display_name, string description)
        {
            //will give pounce for 1 round
            var resource = Helpers.CreateAbilityResource($"{name_prefix}Resource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 7, 1, 8, 1, 0, 0, classes, getArchetypeArray());
            var expeditious_retreat = library.Get<BlueprintAbility>("4f8181e7a7f1d904fbaea64220e83379");
            var buff = Helpers.CreateBuff(name_prefix + "Buff",
                                            display_name,
                                            description,
                                            "",
                                            expeditious_retreat.Icon,
                                            null,
                                            AddMechanicsFeature.MechanicsFeatureType.Pounce.CreateAddMechanics()
                                            );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1), dispellable: false);

            var ability = Helpers.CreateAbility($"{name_prefix}Ability",
                                               display_name,
                                               description,
                                               "",
                                               expeditious_retreat.Icon,
                                               AbilityType.Extraordinary,
                                               CommandType.Swift,
                                               AbilityRange.Personal,
                                               Helpers.oneRoundDuration,
                                               "",
                                               Helpers.CreateRunActions(apply_buff),
                                               expeditious_retreat.GetComponent<AbilitySpawnFx>(),
                                               Helpers.CreateResourceLogic(resource)
                                               );
            ability.setMiscAbilityParametersSelfOnly();

            var feature = Common.AbilityToFeature(ability, false);
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));

            return feature;
        }


        //warsighted is the same as temporal celerity from time domain


        public BlueprintFeature createWeaponMastery(string name_prefix, string display_name, string description)
        {
            var feature = Helpers.CreateFeatureSelection(name_prefix + "Feature",
                                                         display_name,
                                                         description,                                                           
                                                         "",
                                                         library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e").Icon, //weapon focus
                                                         FeatureGroup.None
                                                         );
            feature.AddComponent(Helpers.PrerequisiteNoFeature(feature));

            foreach (var category in Enum.GetValues(typeof(WeaponCategory)).Cast<WeaponCategory>())
            {

                var ic_feature = Helpers.CreateFeature(name_prefix + "" + category.ToString() + "ImprovedCriticalFeature",
                                                       "",
                                                       "",
                                                       "",
                                                       null,
                                                       FeatureGroup.None,
                                                       Common.createAddParametrizedFeatures(library.Get<BlueprintParametrizedFeature>("f4201c85a991369408740c6888362e20"), category)
                                                       );
                ic_feature.HideInCharacterSheetAndLevelUp = true;
                var gws_feature = Helpers.CreateFeature(name_prefix + "" + category.ToString() + "GreaterWeaponFocusFeature",
                                                           "",
                                                           "",
                                                           "",
                                                           null,
                                                           FeatureGroup.None,
                                                           Common.createAddParametrizedFeatures(library.Get<BlueprintParametrizedFeature>("09c9e82965fb4334b984a1e9df3bd088"), category)
                                                           );
                gws_feature.HideInCharacterSheetAndLevelUp = true;
                var ws_feature = Helpers.CreateFeature(name_prefix + "" + category.ToString() + "Feature",
                                                        display_name + $" ({LocalizedTexts.Instance.Stats.GetText(category)})",
                                                        description,
                                                        "",
                                                        feature.Icon,
                                                        FeatureGroup.None,
                                                        Common.createAddParametrizedFeatures(library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e"), category), //weapon focus
                                                        Helpers.CreateAddFeatureOnClassLevel(ic_feature, 8, classes, archetypes: getArchetypeArray()),
                                                        Helpers.CreateAddFeatureOnClassLevel(gws_feature, 16, classes, archetypes: getArchetypeArray()),
                                                        Helpers.Create<PrerequisiteProficiency>(p =>
                                                        {
                                                            p.WeaponProficiencies = new WeaponCategory[] { category };
                                                            p.ArmorProficiencies = new ArmorProficiencyGroup[0];
                                                        }));
                feature.AllFeatures = feature.AllFeatures.AddToArray(ws_feature);
            }

            return feature;
        }







    }
}
