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
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
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
        public BlueprintFeature createBloodOfHeroes(string name_prefix, string display_name, string description)
        {
            var resource = Helpers.CreateAbilityResource($"{name_prefix}Resource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 5, 1, 5, 1, 0, 0, classes);
            var icon = library.Get<BlueprintAbility>("97b991256e43bb140b263c326f690ce2").Icon; //rage

            var buff = Helpers.CreateBuff(name_prefix + "Buff",
                                           display_name,
                                           description,
                                           "",
                                           icon,
                                           null,
                                           Helpers.CreateAddContextStatBonus(StatType.AdditionalAttackBonus, ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus),
                                           Helpers.CreateAddContextStatBonus(StatType.AdditionalDamage, ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus),
                                           Common.createContextSavingThrowBonusAgainstDescriptor(Helpers.CreateContextValue(AbilityRankType.StatBonus), ModifierDescriptor.Morale, SpellDescriptor.Fear),
                                           Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.OnePlusDivStep,
                                                                           type: AbilityRankType.StatBonus, stepLevel: 7, classes: classes)
                                           );
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.SpeedBonus)), dispellable: false);

            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Move,
                                                AbilityRange.Personal,
                                                "Variable",
                                                "",
                                                Helpers.CreateRunActions(apply_buff),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, progression: ContextRankProgression.AsIs,
                                                                           type: AbilityRankType.SpeedBonus, stat: stat),
                                                Common.createAbilitySpawnFx("97b991256e43bb140b263c326f690ce2", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                Helpers.CreateResourceLogic(resource)
                                               );

            ability.setMiscAbilityParametersSelfOnly();

            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                  ability.Name,
                                                  ability.Description,
                                                  "",
                                                  ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(ability),
                                                  Helpers.CreateAddAbilityResource(resource));
            feature.Ranks = 1;
            return feature;
        }


        public BlueprintFeature createPhantomTouch(string name_prefix, string display_name, string description)
        {
            var resource = Helpers.CreateAbilityResource($"{name_prefix}Resource", "", "", "", null);
            resource.SetIncreasedByStat(3, StatType.Wisdom);

            var shaken = library.Get<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220");
            var frightened = library.Get<BlueprintBuff>("f08a7239aa961f34c8301518e71d4cdf");

            var construct = library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f");
            var undead = library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");

            var apply_shaken = Common.createContextActionApplyBuff(shaken, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.SpeedBonus)), dispellable: false);
            var apply_frightened = Common.createContextActionApplyBuff(frightened, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.SpeedBonus)), dispellable: false);

            var effect = Helpers.CreateConditional(Helpers.CreateConditionHasFact(shaken), apply_frightened, apply_shaken);

            var ability = Helpers.CreateAbility($"{name_prefix}Ability",
                                                 display_name,
                                                 description,
                                                 "",
                                                 Helpers.GetIcon("989ab5c44240907489aba0a8568d0603"), // bestow curse
                                                 AbilityType.Supernatural,
                                                 CommandType.Standard,
                                                 AbilityRange.Touch,
                                                 "",
                                                 Helpers.savingThrowNone,
                                                 Helpers.CreateDeliverTouch(),
                                                 Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.Div2, 
                                                                                 AbilityRankType.SpeedBonus, min: 1, classes: classes),
                                                 Helpers.CreateRunActions(effect),
                                                 Common.createAbilityTargetHasFact(true, undead),
                                                 Common.createAbilityTargetHasFact(true, construct),
                                                 shaken.GetComponent<SpellDescriptorComponent>()
                                                );
            ability.setMiscAbilityParametersTouchHarmful();

            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                             ability.Name,
                                             ability.Description,
                                             "",
                                             ability.Icon,
                                             FeatureGroup.None,
                                             resource.CreateAddAbilityResource(),
                                             ability.CreateTouchSpellCast(resource).CreateAddFact()
                                             );
            return feature;
        }


        public BlueprintFeature createSacredCouncil(string name_prefix, string display_name, string description)
        {
            //+2 bonus for 6 seconds on attack rolls, skill checks, spell penetration, CombatManeuver
            var icon = Helpers.GetIcon("1bb08308c9f6a5e4697887cd438b7221"); //judgement protection

            var resource = Helpers.CreateAbilityResource($"{name_prefix}Resource", "", "", "", null);
            resource.SetIncreasedByStat(0, StatType.Wisdom);

            var components = new BlueprintComponent[]
            {
                Helpers.CreateAddStatBonus(StatType.AdditionalAttackBonus, 2, ModifierDescriptor.UntypedStackable),
                Helpers.Create<BuffAllSavesBonus>(b => b.Value = 2),
                Helpers.Create<BuffAllSkillsBonusAbilityValue>(b => b.Value = 2),
                Helpers.Create<NewMechanics.CasterLevelCheckBonus>(b => b.Value = 2),
                Helpers.Create<CombatManeuverMechanics.CombatManeuverBonus>(b => b.Value = 2),
            };

            var names = new string[]{"Attack Rolls", "Saving throws", "Skills", "Caster Level Checks", "Combat Maneuvers"};

            BlueprintAbility[] abilities = new BlueprintAbility[components.Length];

            for (int i = 0; i < abilities.Length; i++)
            {
                var ext = names[i].Replace(" ", "");
                var buff = Helpers.CreateBuff(name_prefix + ext + "Buff",
                                              display_name + ": " + names[i] + " Bonus",
                                              description,
                                              "",
                                              icon,
                                              null,
                                              components[i]
                                              );
                var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1), dispellable: false);
                var ability = Helpers.CreateAbility(name_prefix + ext + "Ability",
                                                       buff.Name,
                                                       description,
                                                       "",
                                                       icon,
                                                       AbilityType.Supernatural,
                                                       CommandType.Move,
                                                       AbilityRange.Personal,
                                                       Helpers.oneRoundDuration,
                                                       "",
                                                       Helpers.CreateRunActions(apply_buff),
                                                       Common.createAbilitySpawnFx("44295e9774b2864488f4a3790b8b0bcf", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                       Helpers.CreateResourceLogic(resource)
                                                    );
                ability.setMiscAbilityParametersSelfOnly();
                abilities[i] = ability;                           
            }

            var wrapper = Common.createVariantWrapper(name_prefix + "Ability", "", abilities);
            wrapper.SetName(display_name);
            wrapper.AddComponent(Helpers.CreateResourceLogic(resource));

            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                 wrapper.Name,
                                                 wrapper.Description,
                                                 "",
                                                 wrapper.Icon,
                                                 FeatureGroup.None,
                                                 resource.CreateAddAbilityResource(),
                                                 Helpers.CreateAddFact(wrapper)
                                                 );
            return feature;
        }


        public BlueprintFeature createSpiritOfTheWarrior(string name_prefix, string display_name, string description)
        {
            var resource = Helpers.CreateAbilityResource($"{name_prefix}Resource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(0, 2, 1, 2, 1, 0, 0, classes);

            var buff = library.CopyAndAdd<BlueprintBuff>("287682389d2011b41b5a65195d9cbc84", name_prefix + "Buff", "");
            buff.RemoveComponents<AddProficiencies>();
            buff.RemoveComponents<ForbidSpellCasting>();
            buff.RemoveComponents<ContextRankConfig>();
            buff.RemoveComponents<AddStatBonus>(c => c.Stat == StatType.SaveFortitude);

            buff.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, type: AbilityRankType.StatBonus, classes: classes));
            var keen = library.Get<BlueprintWeaponEnchantment>("102a9c8c9b7a75e4fb5844e79deaf4c0");
            buff.AddComponent(Helpers.Create<NewMechanics.EnchantmentMechanics.PersistentWeaponEnchantment>(p => { p.enchant = keen; p.secondary_hand = false; }));
            buff.SetName(display_name);
            buff.SetDescription(description);

            var ability = Helpers.CreateActivatableAbility(name_prefix + "ActivatableAbility",
                                                           buff.Name,
                                                           buff.Description,
                                                           "",
                                                           buff.Icon,
                                                           buff,
                                                           Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.Immediately,
                                                           CommandType.Standard,
                                                           null,
                                                           Helpers.CreateActivatableResourceLogic(resource, ActivatableAbilityResourceLogic.ResourceSpendType.NewRound)
                                                          );


            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                 ability.Name,
                                                 ability.Description,
                                                 "",
                                                 ability.Icon,
                                                 FeatureGroup.None,
                                                 resource.CreateAddAbilityResource(),
                                                 Helpers.CreateAddFact(ability)
                                                 );
            foreach (var c in classes)
            {
                feature.AddComponents(Helpers.PrerequisiteClassLevel(c, 11, any: true));
            }
            return feature;
        }

        //spirit armor = air barrier shaman hex


        public BlueprintFeature createStormOfSouls(string name_prefix, string display_name, string description)
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/StormOfSouls.png");
            var resource = Helpers.CreateAbilityResource($"{name_prefix}Resource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 11, 1, 4, 1, 0, 0, classes);

            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Divine, Helpers.CreateContextDiceValue(DiceType.D8, Helpers.CreateContextValue(AbilityRankType.DamageDice)), 
                                                     halfIfSaved: true, isAoE: true);
            var dmg_undead = Helpers.CreateActionDealDamage(DamageEnergyType.Divine, Helpers.CreateContextDiceValue(DiceType.D8, Helpers.CreateContextValue(AbilityRankType.DamageDiceAlternative)),
                                                             halfIfSaved: true, isAoE: true);
            var undead = library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");

            var effect = Helpers.CreateConditional(Helpers.CreateConditionHasFact(undead), dmg_undead, dmg);

            var ability = Helpers.CreateAbility($"{name_prefix}Ability",
                                                 display_name,
                                                 description,
                                                 "",
                                                 icon,
                                                 AbilityType.Supernatural,
                                                 CommandType.Standard,
                                                 AbilityRange.Medium,
                                                 "",
                                                 "Fortitude half",
                                                 Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.Div2,
                                                                                 AbilityRankType.DamageDice, classes: classes),
                                                 Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.AsIs,
                                                                                 AbilityRankType.DamageDiceAlternative, classes: classes),
                                                 Helpers.CreateRunActions(SavingThrowType.Fortitude, effect),
                                                 Helpers.CreateAbilityTargetsAround(20.Feet(), Kingmaker.UnitLogic.Abilities.Components.TargetType.Any),

                                                 Common.createAbilityTargetHasFact(true, undead),
                                                 Common.createAbilitySpawnFx("bbd6decdae32bce41ae8f06c6c5eb893", anchor: AbilitySpawnFxAnchor.ClickedTarget),
                                                 Helpers.CreateResourceLogic(resource),
                                                 Common.createContextCalculateAbilityParamsBasedOnClasses(classes, stat)
                                                );
            ability.setMiscAbilityParametersRangedDirectional();

            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                             ability.Name,
                                             ability.Description,
                                             "",
                                             ability.Icon,
                                             FeatureGroup.None,
                                             resource.CreateAddAbilityResource(),
                                             Helpers.CreateAddFact(ability)
                                             );
            foreach (var c in classes)
            {
                feature.AddComponents(Helpers.PrerequisiteClassLevel(c, 7, any: true));
            }
            return feature;
        }
    }
}
