using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
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
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;


namespace CallOfTheWild
{
    partial class MysteryEngine
    {
        //air barrier is the same as spirirt shield from ancestor mystery


        public BlueprintFeature createInvisibility(string name_prefix, string display_name, string description)
        {
            var invisibility_buff = library.Get<BlueprintBuff>("525f980cb29bc2240b93e953974cb325");
            var improved_invisibility_buff = library.Get<BlueprintBuff>("e6b35473a237a6045969253beb09777c");

            var resource = Helpers.CreateAbilityResource(name_prefix + "Resource", "", "", "", null);
            resource.SetIncreasedByLevel(0, 1, classes, getArchetypeArray());

            var apply_invisibility = Common.createContextActionApplyBuff(invisibility_buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false);

            var ability1 = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                invisibility_buff.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Personal,
                                                Helpers.oneMinuteDuration,
                                                "",
                                                Helpers.CreateRunActions(apply_invisibility),
                                                Helpers.CreateResourceLogic(resource)
                                                );
            ability1.setMiscAbilityParametersSelfOnly();

            var ability2 = Helpers.CreateActivatableAbility(name_prefix + "ActivatableAbility",
                                                            "Greater " + display_name,
                                                            description,
                                                            "",
                                                            improved_invisibility_buff.Icon,
                                                            improved_invisibility_buff,
                                                            AbilityActivationType.Immediately,
                                                            CommandType.Standard,
                                                            null,
                                                            Helpers.CreateActivatableResourceLogic(resource, ActivatableAbilityResourceLogic.ResourceSpendType.NewRound)
                                                            );

            var add_ability2_feature = Common.ActivatableAbilityToFeature(ability2);

            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                display_name,
                                                description,
                                                "",
                                                ability1.Icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddAbilityResource(resource),
                                                Helpers.CreateAddFact(ability1),
                                                Helpers.CreateAddFeatureOnClassLevel(add_ability2_feature, 9, classes, archetypes: getArchetypeArray())
                                                );

            addMinLevelPrerequisite(feature, 3);
            return feature;
        }


        public BlueprintFeature createLightningBreath(string name_prefix, string display_name, string description)
        {
            var icon = library.Get<BlueprintAbility>("d2cff9243a7ee804cb6d5be47af30c73").Icon; //lightning bolt

            var resource = Helpers.CreateAbilityResource(name_prefix + "Resource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 5, 1, 5, 1, 0, 0.0f, classes, getArchetypeArray());

            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Electricity, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D4), Helpers.CreateContextValue(AbilityRankType.Default)),
                                         isAoE: true, halfIfSaved: true);

            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                           display_name,
                                           description,
                                           "",
                                           icon,
                                           AbilityType.Supernatural,
                                           CommandType.Standard,
                                           AbilityRange.Projectile,
                                           "",
                                           Helpers.reflexHalfDamage,
                                           Helpers.CreateRunActions(SavingThrowType.Reflex, dmg),
                                           createClassScalingConfig(),
                                           Helpers.CreateSpellDescriptor(SpellDescriptor.Electricity),
                                           Helpers.CreateResourceLogic(resource),
                                           library.Get<BlueprintAbility>("c073af2846b8e054fb28e6f72bc02749").GetComponent<AbilityDeliverProjectile>(),//kinetic thunderstorm torrent,
                                           Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(classes, getArchetypeArray(),stat)
                                           );
            ability.setMiscAbilityParametersRangedDirectional();

            var feature = Common.AbilityToFeature(ability, true);
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));
            return feature;
        }


        public BlueprintFeature createSparkSkin(string name_prefix, string display_name, string description)
        {
            var icon = Helpers.GetIcon("90987584f54ab7a459c56c2d2f22cee2");

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
                                                       a.Type = DamageEnergyType.Electricity;
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
                                                     Helpers.Create<AddEnergyDamageImmunity>(a => a.EnergyType = DamageEnergyType.Electricity)
                                                     );

            feature.AddComponent(Helpers.CreateAddFeatureOnClassLevel(immunity, 17, classes, archetypes: getArchetypeArray()));
            return feature;
        }


        public BlueprintFeature createThunderburst(string name_prefix, string display_name, string description)
        {
            //20 feet radious burts
            var icon = Helpers.GetIcon("9fbc4fe045472984aa4a2d15d88bdaf9");

            var resource = Helpers.CreateAbilityResource(name_prefix + "Resource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 11, 1, 4, 1, 0, 0.0f, classes, getArchetypeArray());

            var dmg = Helpers.CreateActionDealDamage(PhysicalDamageForm.Bludgeoning, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.Default)),
                                         isAoE: true, halfIfSaved: true);

            var apply_deafened = Helpers.CreateConditionalSaved(null,
                                                               Common.createContextActionApplyBuff(Common.deafened, Helpers.CreateContextDuration(1, DurationRate.Hours), dispellable: false)
                                                               );

            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                           display_name,
                                           description,
                                           "",
                                           icon,
                                           AbilityType.Supernatural,
                                           CommandType.Standard,
                                           AbilityRange.Medium,
                                           "",
                                           "Fortitude half",
                                           Helpers.CreateRunActions(SavingThrowType.Fortitude, dmg, apply_deafened),
                                           createClassScalingConfig(),
                                           Helpers.CreateResourceLogic(resource),
                                           library.Get<BlueprintAbility>("9fbc4fe045472984aa4a2d15d88bdaf9").GetComponent<AbilitySpawnFx>(),//air blast cyclone,
                                           Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(classes, getArchetypeArray(),stat),
                                           Helpers.CreateAbilityTargetsAround(20.Feet(), TargetType.Any)
                                           );
            ability.setMiscAbilityParametersRangedDirectional();

            var feature = Common.AbilityToFeature(ability, true);
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));

            addMinLevelPrerequisite(feature, 7);

            return feature;
        }


        public BlueprintFeature createTouchOfElectricity(string name_prefix, string display_name, string description)
        {
            var icon = library.Get<BlueprintAbility>("ab395d2335d3f384e99dddee8562978f").Icon; //shocking grasp
            var resource = Helpers.CreateAbilityResource(name_prefix + "Resource", "", "", "", null);
            resource.SetIncreasedByStat(3, stat);
            var shocking_touch = library.CopyAndAdd<BlueprintAbility>("b3494639791901e4db3eda6117ad878f", name_prefix + "Ability", ""); //air domain arck of lightning 
            shocking_touch.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Electricity));
            shocking_touch.RemoveComponents<SpellComponent>();
            shocking_touch.RemoveComponents<AbilityResourceLogic>();
            shocking_touch.ReplaceComponent<AbilityDeliverProjectile>(Helpers.CreateDeliverTouch());
            shocking_touch.setMiscAbilityParametersTouchHarmful();
            shocking_touch.Type = AbilityType.Supernatural;
            shocking_touch.Range = AbilityRange.Touch;
            shocking_touch.SpellResistance = false;
            shocking_touch.SetNameDescriptionIcon(display_name,
                                                   description,
                                                   icon);
            shocking_touch.ReplaceComponent<ContextRankConfig>(createClassScalingConfig(ContextRankProgression.Div2));
            var shocking_touch_sticky = Helpers.CreateTouchSpellCast(shocking_touch, resource);
            var shocking = library.Get<BlueprintWeaponEnchantment>("7bda5277d36ad114f9f9fd21d0dab658");

            var shocking_weapon_feature = Helpers.CreateFeature(name_prefix + "WeaponFeature",
                                                          "",
                                                          "",
                                                          "",
                                                          null,
                                                          FeatureGroup.None,
                                                          Helpers.Create<NewMechanics.EnchantmentMechanics.PersistentWeaponEnchantment>(p => p.enchant = shocking)
                                                          );

            shocking_weapon_feature.HideInCharacterSheetAndLevelUp = true;

            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                   display_name,
                                                   description,
                                                   "",
                                                   shocking_touch.Icon,
                                                   FeatureGroup.None,
                                                   Helpers.CreateAddFact(shocking_touch_sticky),
                                                   Helpers.CreateAddAbilityResource(resource),
                                                   Helpers.CreateAddFeatureOnClassLevel(shocking_weapon_feature, 11, classes, archetypes: getArchetypeArray())
                                                   );
            return feature;
        }


        public BlueprintFeature createVortexSpells(string name_prefix, string display_name, string description)
        {
            var icon = library.Get<BlueprintFeature>("f2fa7541f18b8af4896fbaf9f2a21dfe").Icon; //cyclone form infusion

            var staggered = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");
            var apply_staggered1 = Common.createContextActionApplyBuff(staggered, Helpers.CreateContextDuration(1), dispellable: false);
            var apply_staggered1d4 = Common.createContextActionApplyBuff(staggered, Helpers.CreateContextDuration(0, diceType: Kingmaker.RuleSystem.DiceType.D4, diceCount: 1), dispellable: false);
            var action = Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.Default),
                                                                        Helpers.CreateActionList(apply_staggered1),
                                                                        Helpers.CreateActionList(apply_staggered1d4)
                                                                        );
            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                display_name,
                                                description,
                                                "",
                                                icon,
                                                FeatureGroup.None,
                                                Helpers.Create<NewMechanics.ActionOnSpellDamage>(a => { a.only_critical = true; a.action = Helpers.CreateActionList(action); }),
                                                createClassScalingConfig(progression: ContextRankProgression.OnePlusDivStep, stepLevel: 11)
                                               );
            return feature;
        }


        public BlueprintFeature CreateWingsOfAir(string name_prefix, string display_name, string description)
        {
            var ability = library.Get<BlueprintActivatableAbility>("13143852b74718144ac4267b949615f0"); //wings angel
            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                  display_name,
                                                  description,
                                                  "",
                                                  ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(ability));
            addMinLevelPrerequisite(feature, 7);
            return feature;
        }



    }
}
