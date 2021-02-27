using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    public partial class ImplementsEngine
    {
        public BlueprintFeature createMindBarrier()
        {
            var mage_shield = library.Get<BlueprintAbility>("ef768022b0785eb43a18969903c537c4");

            var buff = Helpers.CreateBuff(prefix + "MindBarrierBuff",
                                          "Mind Barrier",
                                          "As a swift action, you can expend 1 point of mental focus to create a shield of mental energy around you that protects you from harm. The shield prevents a total of 2 points of damage per occultist level you possess.\n"
                                          + "It lasts until the start of your next turn or until exhausted.\n",
                                          "",
                                          mage_shield.Icon,
                                          null,
                                          Helpers.Create<TemporaryHitPointsFromAbilityValue>(t =>
                                          {
                                              t.Descriptor = Kingmaker.Enums.ModifierDescriptor.UntypedStackable;
                                              t.RemoveWhenHitPointsEnd = true;
                                              t.Value = Helpers.CreateContextValue(Kingmaker.Enums.AbilityRankType.Default);
                                          }),
                                          createClassScalingConfig(ContextRankProgression.MultiplyByModifier, stepLevel: 2)
                                          );

            var ability = Helpers.CreateAbility(prefix + "MindBarrierAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.SpellLike,
                                                UnitCommand.CommandType.Swift,
                                                AbilityRange.Personal,
                                                Helpers.oneRoundDuration,
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionApplySpellBuff(buff, Helpers.CreateContextDuration(1))),
                                                mage_shield.GetComponent<AbilitySpawnFx>(),
                                                Helpers.CreateSpellComponent(SpellSchool.Abjuration),
                                                resource.CreateResourceLogic()
                                                );
            ability.setMiscAbilityParametersSelfOnly();
            addFocusInvestmentCheck(ability, SpellSchool.Abjuration);

            return Common.AbilityToFeature(ability, false);
        }


        public BlueprintFeature createUnraveling()
        {
            var dispel_magic = library.Get<BlueprintAbility>("143775c49ae6b7446b805d3b2e702298");
            var ability = Common.convertToSpellLike(dispel_magic, prefix, classes, stat, resource, archetypes: getArchetypeArray());
            ability.setMiscAbilityParametersTouchHarmful();
            ability.SetNameDescription("Unraveling",
                                       "As a standard action, you can expend 1 point of mental focus to unravel a magical effect. This functions as a targeted dispel magic spell, but you must be adjacent to the effect to unravel it.");
            var feature =  Common.AbilityToFeature(ability, false);
            addMinLevelPrerequisite(feature, 5);
            addFocusInvestmentCheck(ability, SpellSchool.Abjuration);
            return feature;
        }


        public BlueprintFeature createEnergyShield()
        {
            var icon = Helpers.GetIcon("d2f116cfe05fcdd4a94e80143b67046f"); //protection from energy

            var buff = Helpers.CreateBuff(prefix + "EnergyShieldBuff",
                                          "Energy Shield",
                                          "As a swift action, you can expend 1 point of mental focus to surround yourself with a shield that protects you from energy damage. Whenever you take acid, cold, electricity, or fire damage, the shield absorbs the damage (as protection from energy). The energy shield can absorb up to 5 points of energy damage per occultist level you possess. This shield lasts for 1 minute or until its power is exhausted. Its effect doesn’t stack with itself, with protection from energy, or with resist energy.\n"
                                          + "You must be at least 3rd level to select this focus power.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.Create<LocalPoolMechanics.BuffLocalPool>(b => { b.value = Helpers.CreateContextValue(AbilityRankType.Default); b.multiplier = 5; }),
                                          Helpers.Create<LocalPoolMechanics.AddDamageProtectionEnergyFromLocalPool>(a => a.Type = DamageEnergyType.Acid),
                                          Helpers.Create<LocalPoolMechanics.AddDamageProtectionEnergyFromLocalPool>(a => a.Type = DamageEnergyType.Electricity),
                                          Helpers.Create<LocalPoolMechanics.AddDamageProtectionEnergyFromLocalPool>(a => a.Type = DamageEnergyType.Fire),
                                          Helpers.Create<LocalPoolMechanics.AddDamageProtectionEnergyFromLocalPool>(a => a.Type = DamageEnergyType.Cold),
                                          Helpers.Create<LocalPoolMechanics.AddDamageProtectionEnergyFromLocalPool>(a => a.Type = DamageEnergyType.Sonic),
                                          createClassScalingConfig()
                                          );

            var ability = Helpers.CreateAbility(prefix + "EnergyShieldAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.SpellLike,
                                                CommandType.Swift,
                                                AbilityRange.Personal,
                                                Helpers.oneMinuteDuration,
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionApplySpellBuff(buff, Helpers.CreateContextDuration(1, DurationRate.Minutes))),
                                                resource.CreateResourceLogic(),
                                                Common.createAbilitySpawnFx("8475ed069cfb5ed4daedd2945d9d7555", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                Helpers.CreateSpellComponent(SpellSchool.Abjuration)
                                                );
            ability.setMiscAbilityParametersSelfOnly();
            addFocusInvestmentCheck(ability, SpellSchool.Abjuration);
            var feature = Common.AbilityToFeature(ability, false);
            addMinLevelPrerequisite(feature, 3);
            return feature;
        }


        public BlueprintFeature createGlobeOfNegation()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/Metamixing.png");

            var buff = Helpers.CreateBuff(prefix + "GlobeOfNegationBuff",
                                          "Globe of Negation",
                                          "As a standard action, you can expend 3 points of mental focus to create a mobile globe of negation. This globe surrounds yourself and cancels any spell effect that is cast into or through its area. This functions as globe of invulnerability, but it affects spells of any level. The globe can negate a total number of spell levels equal to your occultist level, after which the globe collapses.\n"
                                          + "You must be at least 11th level to select this focus power.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.Create<InvulnerabilityMechanics.SpellLevelsImmunity>(i => i.spell_levels = Helpers.CreateContextValue(AbilityRankType.Default)),
                                          createClassScalingConfig()
                                          );

            var ability = Helpers.CreateAbility(prefix + "GlobeOfNegationAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.SpellLike,
                                                CommandType.Standard,
                                                AbilityRange.Personal,
                                                "Until the globe collapses",
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), is_permanent: true)),
                                                resource.CreateResourceLogic(amount: 3),
                                                Common.createAbilitySpawnFx("8475ed069cfb5ed4daedd2945d9d7555", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                Helpers.CreateSpellComponent(SpellSchool.Abjuration)
                                                );
            ability.setMiscAbilityParametersSelfOnly();
            addFocusInvestmentCheck(ability, SpellSchool.Abjuration);

            var feature =  Common.AbilityToFeature(ability, false);
            addMinLevelPrerequisite(feature, 11);
            return feature;
        }


        public BlueprintFeature createAegis()
        {
            var group = ActivatableAbilityGroupExtension.AegisImplement.ToActivatableAbilityGroup();
            var mage_armor = library.Get<BlueprintAbility>("9e1ad5d6f87d19e4d8883d63a6e35568");
            var enchants = ArmorEnchantments.temporary_armor_enchantments;
            var enhancement_buff = Helpers.CreateBuff(prefix + "AegisArmorEnchancementBaseBuff",
                                            "",
                                            "",
                                            "",
                                            null,
                                            null,
                                            Common.createBuffRemainingGroupSizetEnchantArmor(group,
                                                                                            false, true, enchants
                                                                                            )
                                            );
            enhancement_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var bond_enhancement_buff = Helpers.CreateBuff(prefix + "AegisArmorEnhancementSwitchBuff",
                                                            "Aegis",
                                                            "As a standard action, you can expend 1 point of mental focus and touch a suit of armor to grant it an enhancement bonus. The bonus is equal to 1 + 1 for every 6 occultist levels you possess (to a maximum bonus of +4 at 18th level). Enhancement bonuses gained via this ability stack with those of the armor or shield, to a maximum total enhancement bonus of +5. You can also imbue the armor or shield with any one armor or shield special ability that has an equivalent enhancement bonus less than or equal to your maximum bonus granted by this ability by reducing the granted enhancement bonus by the appropriate amount.\n" 
                                                            + "Alternatively you can spend 1 extra point of mental focus to use this ability as swift action.",
                                                            "",
                                                            mage_armor.Icon,
                                                            null,
                                                            Helpers.CreateAddFactContextActions(activated: Common.createContextActionApplyBuff(enhancement_buff, Helpers.CreateContextDuration(),
                                                                                                        is_child: true, dispellable: false, is_permanent: true)
                                                                                                )
                                                            );
            //fortification - light (25% +1), medium(50% +3), heavy (+75%)
         

          
            var fortification_icon = library.Get<BlueprintAbility>("c66e86905f7606c4eaa5c774f0357b2b").Icon; //stoneskin
            Dictionary<DamageEnergyType, UnityEngine.Sprite> energy_resistance_icons = new Dictionary<DamageEnergyType, UnityEngine.Sprite>();
            energy_resistance_icons.Add(DamageEnergyType.Fire, library.Get<BlueprintAbility>("ddfb4ac970225f34dbff98a10a4a8844").Icon);
            energy_resistance_icons.Add(DamageEnergyType.Cold, library.Get<BlueprintAbility>("5368cecec375e1845ae07f48cdc09dd1").Icon);
            energy_resistance_icons.Add(DamageEnergyType.Electricity, library.Get<BlueprintAbility>("90987584f54ab7a459c56c2d2f22cee2").Icon);
            energy_resistance_icons.Add(DamageEnergyType.Acid, library.Get<BlueprintAbility>("fedc77de9b7aad54ebcc43b4daf8decd").Icon);

            List<BlueprintActivatableAbility>[] enchant_abilities = new List<BlueprintActivatableAbility>[5];

            for (int i = 0; i < enchant_abilities.Length; i++)
            {
                enchant_abilities[i] = new List<BlueprintActivatableAbility>();
            }

            foreach (var e in ArmorEnchantments.fortification_enchantments)
            {
                int cost = e.EnchantmentCost;
                if (cost > 5)
                {
                    continue;
                }
                var a = Common.createEnchantmentAbility(prefix + "Aegis" + e.name, "Aegis - " + e.Name, e.Description + $"\nThis consumes {cost} point(s) of armor enhancement bonus.", fortification_icon, bond_enhancement_buff, e, cost, group);
                enchant_abilities[cost - 1].Add(a);
            }

            foreach (var item in ArmorEnchantments.energy_resistance_enchantments)
            {
                foreach (var e in item.Value)
                {
                    int cost = e.EnchantmentCost;
                    if (cost > 5)
                    {
                        continue;
                    }
                    var a = Common.createEnchantmentAbility(prefix + "Aegis" + e.name, "Aegis - " + e.Name, e.Description + $"\nThis consumes {cost} point(s) of armor enhancement bonus.", energy_resistance_icons[item.Key], bond_enhancement_buff, e, cost, group);
                    enchant_abilities[cost - 1].Add(a);
                }
            }

            {
                var e = ArmorEnchantments.spell_storing;
                int cost = e.EnchantmentCost;
                var a = Common.createEnchantmentAbility(prefix + "Aegis" + e.name, "Aegis - " + e.Name, e.Description + $"\nThis consumes {cost} point(s) of armor enhancement bonus.", 
                                                        LoadIcons.Image2Sprite.Create(@"AbilityIcons/SpellstoringArmor.png"),
                                                        bond_enhancement_buff, e, 1, group);
                enchant_abilities[0].Add(a);
            }

            var apply_buff = Common.createContextActionApplyBuff(bond_enhancement_buff,
                                                                 Helpers.CreateContextDuration(1, DurationRate.Minutes),
                                                                 dispellable: false);
            var ability = Helpers.CreateAbility(prefix + "AegisAbility",
                                                bond_enhancement_buff.Name,
                                                bond_enhancement_buff.Description,
                                                "",
                                                bond_enhancement_buff.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Personal,
                                                Helpers.oneMinuteDuration,
                                                Helpers.savingThrowNone,
                                                mage_armor.GetComponent<AbilitySpawnFx>(),
                                                Helpers.CreateRunActions(apply_buff),
                                                Helpers.CreateResourceLogic(resource),
                                                Helpers.CreateSpellComponent(SpellSchool.Abjuration)
                                                );
            ability.setMiscAbilityParametersSelfOnly();
            var swift_ability = library.CopyAndAdd(ability, prefix + "SwiftAegisAbility", "");
            swift_ability.SetName(bond_enhancement_buff.Name + " (Swift)");
            swift_ability.ActionType = CommandType.Swift;
            swift_ability.ReplaceComponent<AbilityResourceLogic>(a => a.Amount = 2);
            var wrapper = Common.createVariantWrapper(prefix + "AegisBase", "", ability, swift_ability);
            addFocusInvestmentCheck(wrapper, SpellSchool.Abjuration);
            var add_enchants = new BlueprintFeature[5];
            for (int i = 0; i < add_enchants.Length; i++)
            {
                add_enchants[i] = Helpers.CreateFeature(prefix + $"Aegis{i + 1}Feature",
                                                        i == 0 ? "Aegis" : $"Aegis +{i + 1}",
                                                        ability.Description,
                                                        "",
                                                        ability.Icon,
                                                        FeatureGroup.None
                                                        );
                if (i == 0)
                {
                    add_enchants[i].AddComponent(Helpers.CreateAddFacts(wrapper));
                }
                if (i > 0)
                {
                    add_enchants[i].AddComponent(Common.createIncreaseActivatableAbilityGroupSize(group));
                }
                if (enchant_abilities[i].Count > 0)
                {
                    add_enchants[i].AddComponent(Helpers.CreateAddFacts(enchant_abilities[i].ToArray()));
                }
            }

            add_enchants[0].AddComponents(createAddFeatureInLevelRange(add_enchants[1], 6, 100),
                              createAddFeatureInLevelRange(add_enchants[2], 12, 100),
                              createAddFeatureInLevelRange(add_enchants[3], 18, 100)
                              );

            mastery_features.Add(add_enchants[4]);
            return add_enchants[0];
        }





        public BlueprintBuff createWardingTalisman()
        {
            var property = ImplementMechanics.InvestedImplementFocusAmountProperty.createProperty(prefix + "WardingTalismanProperty", "",
                                                                                                  createClassScalingConfig(ContextRankProgression.StartPlusDivStep, type: AbilityRankType.StatBonus, startLevel: -2, stepLevel: 2),//1 + (lvl + 2)/2 = 2 + lvl/2
                                                                                                  false,
                                                                                                  SpellSchool.Abjuration);
            var buff = Helpers.CreateBuff(prefix + "WardingTalismanBuff",
                                          "Warding Talisman",
                                          "The implement wards against adverse effects. Whoever wears the implement gains a +1 resistance bonus on saving throws for every 2 points of mental focus invested in the implement, to a maximum bonus of 1 + 1 for every 4 occultist levels you possess.",
                                          "",
                                          Helpers.GetIcon("0a5ddfbcfb3989543ac7c936fc256889"),
                                          null,
                                          Helpers.CreateAddContextStatBonus(StatType.SaveFortitude, ModifierDescriptor.Resistance),
                                          Helpers.CreateAddContextStatBonus(StatType.SaveReflex, ModifierDescriptor.Resistance),
                                          Helpers.CreateAddContextStatBonus(StatType.SaveWill, ModifierDescriptor.Resistance),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, ContextRankProgression.DivStep, stepLevel: 2,
                                                                          customProperty: property)
                                          );

            return buff;
        }
    }
}
