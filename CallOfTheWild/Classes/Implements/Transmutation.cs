using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
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
    public partial class ImplementsEngine
    {
        public BlueprintFeature createLegacyWeapon()
        {
            var legacy_weapon_group = ActivatableAbilityGroupExtension.LegacyWeaponImplement.ToActivatableAbilityGroup();

            var divine_weapon = library.Get<BlueprintAbility>("7ff088ab58c69854b82ea95c2b0e35b4");
            var enchants = WeaponEnchantments.temporary_enchants;

            var enhancement_buff = Helpers.CreateBuff(prefix + "LegacyWeaponEnchancementBaseBuff",
                                         "",
                                         "",
                                         "",
                                         null,
                                         null,
                                         Common.createBuffRemainingGroupsSizeEnchantPrimaryHandWeapon(legacy_weapon_group,
                                                                                                      false, true,
                                                                                                      enchants
                                                                                                      )
                                         );
            var weapon_enhancement_buff = Helpers.CreateBuff(prefix + "LegacyWeaponSwitchBuff",
                                                                 "Legacy Weapon",
                                                                 "As a standard action, you can expend 1 point of mental focus and touch a weapon to grant it an enhancement bonus. The bonus is equal to 1 + 1 for every 6 occultist levels you possess (to a maximum of +4 at 18th level). Enhancement bonuses gained by this ability stack with those of the weapon, to a maximum of +5.\n"
                                                                 + "You can also imbue the weapon with any one weapon special ability with an equivalent enhancement bonus less than or equal to your maximum bonus by reducing the granted enhancement bonus by the appropriate amount. The item must have an enhancement bonus of at least +1 (from the item itself or from legacy weapon) to gain a weapon special ability. In either case, these bonuses last for 1 minute.\n"
                                                                 + "You can expend 1 extra point of mental focus to use this ability as swift action.",
                                                                 "",
                                                                 divine_weapon.Icon,
                                                                 null,
                                                                 Helpers.CreateAddFactContextActions(activated: Common.createContextActionApplyBuff(enhancement_buff, Helpers.CreateContextDuration(),
                                                                                                                is_child: true, is_permanent: true, dispellable: false)
                                                                                                     )
                                                                 );
            enhancement_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var brilliant_energy = Common.createEnchantmentAbility(prefix + "LegacyWeaponEnchancementBrilliantEnergy",
                                                                        "Legacy Weapon - Brilliant Energy",
                                                                        "An occultist can add the brilliant energy property to her weapon, but this consumes 4 points of enhancement bonus granted to this weapon.\nA brilliant energy weapon ignores nonliving matter.Armor and shield bonuses to AC(including any enhancement bonuses to that armor) do not count against it because the weapon passes through armor. (Dexterity, deflection, dodge, natural armor, and other such bonuses still apply.) A brilliant energy weapon cannot harm undead, constructs, or objects.",
                                                                        library.Get<BlueprintActivatableAbility>("f1eec5cc68099384cbfc6964049b24fa").Icon,
                                                                        weapon_enhancement_buff,
                                                                        library.Get<BlueprintWeaponEnchantment>("66e9e299c9002ea4bb65b6f300e43770"),
                                                                        4, legacy_weapon_group);

            var flaming = Common.createEnchantmentAbility(prefix + "LegacyWeaponEnchancementFlaming",
                                                                "Legacy Weapon - Flaming",
                                                                "An occultist can add the flaming property to her weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA flaming weapon is sheathed in fire that deals an extra 1d6 points of fire damage on a successful hit. The fire does not harm the wielder.",
                                                                library.Get<BlueprintActivatableAbility>("7902941ef70a0dc44bcfc174d6193386").Icon,
                                                                weapon_enhancement_buff,
                                                                library.Get<BlueprintWeaponEnchantment>("30f90becaaac51f41bf56641966c4121"),
                                                                1, legacy_weapon_group);

            var frost = Common.createEnchantmentAbility(prefix + "LegacyWeaponEnchancementFrost",
                                                            "Legacy Weapon - Frost",
                                                            "An occultist can add the frost property to her weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA frost weapon is sheathed in a terrible, icy cold that deals an extra 1d6 points of cold damage on a successful hit. The cold does not harm the wielder.",
                                                            library.Get<BlueprintActivatableAbility>("b338e43a8f81a2f43a73a4ae676353a5").Icon,
                                                            weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("421e54078b7719d40915ce0672511d0b"),
                                                            1, legacy_weapon_group);

            var shock = Common.createEnchantmentAbility(prefix + "LegacyWeaponEnchancementShock",
                                                            "Legacy Weapon - Shock",
                                                            "An occultist can add the shock property to her weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA shock weapon is sheathed in crackling electricity that deals an extra 1d6 points of electricity damage on a successful hit. The electricity does not harm the wielder.",
                                                            library.Get<BlueprintActivatableAbility>("a3a9e9a2f909cd74e9aee7788a7ec0c6").Icon,
                                                            weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("7bda5277d36ad114f9f9fd21d0dab658"),
                                                            1, legacy_weapon_group);

            var ghost_touch = Common.createEnchantmentAbility(prefix + "LegacyWeaponEnchancementGhostTouch",
                                                                    "Legacy Weapon - Ghost Touch",
                                                                    "An occultist can add the ghost touch property to her weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA ghost touch weapon deals damage normally against incorporeal creatures, regardless of its bonus. An incorporeal creature's 50% reduction in damage from corporeal sources does not apply to attacks made against it with ghost touch weapons.",
                                                                    library.Get<BlueprintActivatableAbility>("688d42200cbb2334c8e27191c123d18f").Icon,
                                                                    weapon_enhancement_buff,
                                                                    library.Get<BlueprintWeaponEnchantment>("47857e1a5a3ec1a46adf6491b1423b4f"),
                                                                    1, legacy_weapon_group);

            var keen = Common.createEnchantmentAbility(prefix + "LegacyWeaponEnchancementKeen",
                                                            "Legacy Weapon - Keen",
                                                            "An occultist can add the keen property to her weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nThe keen property doubles the threat range of a weapon. This benefit doesn't stack with any other effects that expand the threat range of a weapon (such as the keen edge spell or the Improved Critical feat).",
                                                            library.Get<BlueprintActivatableAbility>("27d76f1afda08a64d897cc81201b5218").Icon,
                                                            weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("102a9c8c9b7a75e4fb5844e79deaf4c0"),
                                                            1, legacy_weapon_group);

            var disruption = Common.createEnchantmentAbility(prefix + "LegacyWeaponEnchancementDisruption",
                                                                    "Legacy Weapon - Disruption",
                                                                    "An occultist can add the disruption property to her weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nA disruption weapon is the bane of all undead. Any undead creature struck in combat must succeed on a DC 14 Will save or be destroyed. A disruption weapon must be a bludgeoning melee weapon.",
                                                                    library.Get<BlueprintActivatableAbility>("27d76f1afda08a64d897cc81201b5218").Icon,
                                                                    weapon_enhancement_buff,
                                                                    library.Get<BlueprintWeaponEnchantment>("0f20d79b7049c0f4ca54ca3d1ea44baa"),
                                                                    2, legacy_weapon_group);

            var holy = Common.createEnchantmentAbility(prefix + "LegacyWeaponEnchancementHoly",
                                                            "Legacy Weapon - Holy",
                                                            "An occultist can add the holy property to her weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nA holy weapon is imbued with holy power. This power makes the weapon good-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of evil alignment.",
                                                            library.Get<BlueprintActivatableAbility>("ce0ece459ebed9941bb096f559f36fa8").Icon,
                                                            weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("28a9964d81fedae44bae3ca45710c140"),
                                                            2, legacy_weapon_group);

            var unholy = Common.createEnchantmentAbility(prefix + "LegacyWeaponEnchancementUnholy",
                                                            "Legacy Weapon - Unholy",
                                                            "An occultist can add the unholy property to her weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn unholy weapon is imbued with unholy power. This power makes the weapon evil-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of good alignment.",
                                                            library.Get<BlueprintActivatableAbility>("561803a819460f34ea1fe079edabecce").Icon,
                                                            weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("d05753b8df780fc4bb55b318f06af453"),
                                                            2, legacy_weapon_group);

            var axiomatic = Common.createEnchantmentAbility(prefix + "LegacyWeaponEnchancementAxiomatic",
                                                            "Legacy Weapon - Axiomatic",
                                                            "An occultist can add the axiomatic property to her weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn axiomatic weapon is infused with lawful power. It makes the weapon lawful-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against chaotic creatures.",
                                                            library.Get<BlueprintActivatableAbility>("d76e8a80ab14ac942b6a9b8aaa5860b1").Icon,
                                                            weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("0ca43051edefcad4b9b2240aa36dc8d4"),
                                                            2, legacy_weapon_group);

            var anarchic = Common.createEnchantmentAbility(prefix + "LegacyWeaponEnchancementAnarchic",
                                                "Legacy Weapon - Anarchic",
                                                "An occultist can add the anarchic property to her weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn anarchic weapon is infused with the power of chaos. It makes the weapon chaotic-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of lawful alignment.",
                                                LoadIcons.Image2Sprite.Create(@"AbilityIcons/HWAnarchic.png"),
                                                weapon_enhancement_buff,
                                                library.Get<BlueprintWeaponEnchantment>("57315bc1e1f62a741be0efde688087e9"),
                                                2, legacy_weapon_group);

            var vicious = Common.createEnchantmentAbility(prefix + "LegacyWeaponEnchancementVicious",
                                                        "Legacy Weapon - Vicious",
                                                        $"An occultist can add the vicious property to her weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\n{WeaponEnchantments.vicious.Description}",
                                                        LoadIcons.Image2Sprite.Create(@"AbilityIcons/HWVicious.png"),
                                                        weapon_enhancement_buff,
                                                        WeaponEnchantments.vicious,
                                                        1, legacy_weapon_group);

            var cruel = Common.createEnchantmentAbility(prefix + "LegacyWeaponEnchancementCruel",
                                                        "Legacy Weapon - Cruel",
                                                        $"An occultist can add the cruel property to her weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\n{WeaponEnchantments.cruel.Description}",
                                                        LoadIcons.Image2Sprite.Create(@"AbilityIcons/HWCruel.png"),
                                                        weapon_enhancement_buff,
                                                        WeaponEnchantments.cruel,
                                                        1, legacy_weapon_group);

            var vorpal = Common.createEnchantmentAbility(prefix + "LegacyWeaponEnchancementVorpal",
                                                        "Legacy Weapon - Vorpal",
                                                        $"An occultist can add the vorpal property to her weapon, but this consumes 5 points of enhancement bonus granted to this weapon.\n{WeaponEnchantments.vorpal.Description}",
                                                        LoadIcons.Image2Sprite.Create(@"AbilityIcons/HWVorpal.png"),
                                                        weapon_enhancement_buff,
                                                        WeaponEnchantments.vorpal,
                                                        5, legacy_weapon_group);

            var speed_enchant = library.Get<BlueprintWeaponEnchantment>("f1c0c50108025d546b2554674ea1c006");
            var speed = Common.createEnchantmentAbility(prefix + "WarriorSpiritEnchancementSpeed",
                                                        "Warrior Spirit - Speed",
                                                        "An occultist can add the vicious property to her weapon, but this consumes 3 points of enhancement bonus granted to this weapon.\n" + speed_enchant.Description,
                                                        library.Get<BlueprintActivatableAbility>("ed1ef581af9d9014fa1386216b31cdae").Icon, //speed
                                                        weapon_enhancement_buff,
                                                        speed_enchant,
                                                        3, legacy_weapon_group);

            var bane = Common.createEnchantmentAbility(prefix + "LegacyWeaponEnchancementBane",
                                                        "Legacy Weapon - Bane",
                                                        $"An occultist can add the bane property to her weapon, but this consumes 4 points of enhancement bonus granted to this weapon.\n{WeaponEnchantments.bane.Description}",
                                                        LoadIcons.Image2Sprite.Create(@"AbilityIcons/HWBane.png"),
                                                        weapon_enhancement_buff,
                                                        WeaponEnchantments.bane,
                                                        4, legacy_weapon_group);

            var apply_buff = Common.createContextActionApplyBuff(weapon_enhancement_buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false);

            var ability = Helpers.CreateAbility(prefix + "LegacyWeaponEnchantmentAbility",
                                                    weapon_enhancement_buff.Name,
                                                    weapon_enhancement_buff.Description,
                                                    "",
                                                    weapon_enhancement_buff.Icon,
                                                    AbilityType.Supernatural,
                                                    Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                    AbilityRange.Personal,
                                                    Helpers.oneMinuteDuration,
                                                    "",
                                                    Helpers.CreateRunActions(apply_buff),
                                                    resource.CreateResourceLogic()
                                                    );
            ability.setMiscAbilityParametersSelfOnly();

            var ability_swift = library.CopyAndAdd(ability, "Swift" + ability.name, "");
            ability_swift.ActionType = CommandType.Swift;
            ability_swift.SetName(ability_swift.Name + " (Swift)");
            ability_swift.ReplaceComponent<AbilityResourceLogic>(a => a.Amount = 2);
            var wrapper = Common.createVariantWrapper(prefix + "LegacyWeaponEnchantmentAbilityBase", "", ability, ability_swift);
            addFocusInvestmentCheck(wrapper, SpellSchool.Transmutation);

            var legacy_weapon_features = new BlueprintFeature[5];
            legacy_weapon_features[0] = Helpers.CreateFeature(prefix + "LegacyWeaponEnchancementFeature",
                                                            "Legacy Weapon",
                                                            weapon_enhancement_buff.Description,
                                                            "",
                                                            weapon_enhancement_buff.Icon,
                                                            FeatureGroup.None,
                                                            Helpers.CreateAddFacts(wrapper, flaming, frost, shock, ghost_touch, keen, cruel, vicious)
                                                            );

            legacy_weapon_features[1] = Helpers.CreateFeature(prefix + "LegacyWeaponEnchancement2Feature",
                                                            "Legacy Weapon +2",
                                                            weapon_enhancement_buff.Description,
                                                            "",
                                                            weapon_enhancement_buff.Icon,
                                                            FeatureGroup.None,
                                                            Common.createIncreaseActivatableAbilityGroupSize(legacy_weapon_group),
                                                            Helpers.CreateAddFacts(disruption, holy, unholy, axiomatic, anarchic)
                                                            );

            legacy_weapon_features[2] = Helpers.CreateFeature(prefix + "LegacyWeaponEnchancement3Feature",
                                                                "Legacy Weapon +3",
                                                                weapon_enhancement_buff.Description,
                                                                "",
                                                                weapon_enhancement_buff.Icon,
                                                                FeatureGroup.None,
                                                                Helpers.CreateAddFacts(speed),
                                                                Common.createIncreaseActivatableAbilityGroupSize(legacy_weapon_group)
                                                                );

            legacy_weapon_features[3] = Helpers.CreateFeature(prefix + "LegacyWeaponEnchancement4Feature",
                                                                            "Legacy Weapon +4",
                                                                            weapon_enhancement_buff.Description,
                                                                            "",
                                                                            weapon_enhancement_buff.Icon,
                                                                            FeatureGroup.None,
                                                                            Common.createIncreaseActivatableAbilityGroupSize(legacy_weapon_group),
                                                                            Helpers.CreateAddFacts(brilliant_energy, bane)
                                                                            );
            legacy_weapon_features[4] = Helpers.CreateFeature(prefix + "LegacyWeaponEnchancement5Feature",
                                                                "Legacy Weapon +5",
                                                                weapon_enhancement_buff.Description,
                                                                "",
                                                                weapon_enhancement_buff.Icon,
                                                                FeatureGroup.None,
                                                                Common.createIncreaseActivatableAbilityGroupSize(legacy_weapon_group),
                                                                Helpers.CreateAddFact(vorpal)
                                                                );

            legacy_weapon_features[0].AddComponents(createAddFeatureInLevelRange(legacy_weapon_features[1], 6, 100),
                                                   createAddFeatureInLevelRange(legacy_weapon_features[2], 12, 100),
                                                   createAddFeatureInLevelRange(legacy_weapon_features[3], 18, 100)
                                                   );
            mastery_features.Add(legacy_weapon_features[4]);
            return legacy_weapon_features[0];
        }


        public BlueprintFeature createMindOverGravity()
        {
            var ability = Common.convertToSpellLike(NewSpells.fly, prefix, classes, stat, resource, archetypes: getArchetypeArray());
            ability.Range = AbilityRange.Personal;
            ability.SetNameDescription("Mind Over Gravity",
                                       "As a standard action, you can expend 1 point of mental focus to give yourself an ability to fly and increase your speed by 10 feet. This effect lasts for 1 minute per occultist level you possess. You must be at least 7th level to select this focus power.");
            var feature = Common.AbilityToFeature(ability, false);
            addFocusInvestmentCheck(ability, SpellSchool.Transmutation);
            addMinLevelPrerequisite(feature, 7);
            return feature;
        }


        public BlueprintFeature createPhilosophersTouch()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/WeaponMagic.png");
            List<BlueprintAbility> abilities_single = new List<BlueprintAbility>();
            List<BlueprintAbility> abilities_multiple = new List<BlueprintAbility>();
            var enchants = new BlueprintWeaponEnchantment[]
            {
                WeaponEnchantments.cold_iron,
                WeaponEnchantments.mithral,
                WeaponEnchantments.adamantine
            };

            var remove_buffs = Helpers.Create<NewMechanics.ContextActionRemoveBuffs>(c => c.Buffs = new BlueprintBuff[0]);
            foreach (var e in enchants)
            {
                var buff = Helpers.CreateBuff(prefix + e.name + "PhilosophersTouchBuff",
                                              "Philosopher’s Touch: " + e.Name,
                                              "As a standard action, you can expend 1 point of mental focus and touch a weapon, causing it to gain the properties of a special material. You can cause the weapon to be treated as cold iron or silver for the purposes of overcoming damage reduction for 1 minute per occultist level you possess.\n"
                                              + "At 8th level, you can affect all weapons in 15-foot burst centered on you (still expending only 1 point of mental focus). At 11th level, you can cause any weapon affected by this ability to act as if it were adamantine instead.",
                                              "",
                                              icon,
                                              null,
                                              Helpers.Create<NewMechanics.EnchantmentMechanics.PersistentWeaponEnchantment>(p => { p.secondary_hand = false; p.only_melee = true; p.enchant = e; })
                                              );
                remove_buffs.Buffs = remove_buffs.Buffs.AddToArray(buff);
                var ability_single = Helpers.CreateAbility(prefix + e.name + "PhilosophersTouchSingleAbility",
                                                           buff.Name,
                                                           buff.Description,
                                                           "",
                                                           buff.Icon,
                                                           AbilityType.Supernatural,
                                                           CommandType.Standard,
                                                           AbilityRange.Touch,
                                                           Helpers.minutesPerLevelDuration,
                                                           "",
                                                           Helpers.CreateRunActions(remove_buffs, Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), dispellable: false)),
                                                           Common.createAbilitySpawnFx("352469f228a3b1f4cb269c7ab0409b8e", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                           resource.CreateResourceLogic(),
                                                           createClassScalingConfig()
                                                           );
                ability_single.setMiscAbilityParametersTouchFriendly();
                if (e == WeaponEnchantments.adamantine)
                {
                    ability_single.AddComponent(Helpers.Create<NewMechanics.AbilityShowIfHasClassLevels>(a => { a.character_classes = classes; a.level = 11; }));
                }

                var ability_multiple = library.CopyAndAdd(ability_single, prefix + e.name + "PhilosophersTouchSingleMultiple", "");
                ability_multiple.Range = AbilityRange.Personal;
                ability_multiple.AddComponent(Helpers.CreateAbilityTargetsAround(15.Feet(), TargetType.Ally));

                abilities_single.Add(ability_single);
                abilities_multiple.Add(ability_multiple);
            }


            var wrapper_single = Common.createVariantWrapper(prefix + "PhilosophersTouchSingleAbilityBase", "", abilities_single.ToArray());
            wrapper_single.SetName("Philosopher’s Touch");
            var wrapper_multiple = Common.createVariantWrapper(prefix + "PhilosophersTouchMultipleAbilityBase", "", abilities_multiple.ToArray());
            wrapper_multiple.SetName("Philosopher’s Touch");

            addFocusInvestmentCheck(wrapper_single, SpellSchool.Transmutation);
            addFocusInvestmentCheck(wrapper_multiple, SpellSchool.Transmutation);
            var feature_single = Common.AbilityToFeature(wrapper_single);
            var feature_multiple = Common.AbilityToFeature(wrapper_multiple);

            var feature = Helpers.CreateFeature(prefix + "PhilosophersTouchFeature",
                                                feature_single.Name,
                                                feature_single.Description,
                                                "",
                                                feature_single.Icon,
                                                FeatureGroup.None,
                                                createAddFeatureInLevelRange(feature_single, 0, 7),
                                                createAddFeatureInLevelRange(feature_multiple, 8, 100)
                                                );
            return feature;
        }


        public BlueprintFeature createSuddenSpeed()
        {
            var ability = Common.convertToSpellLike(library.Get<BlueprintAbility>("4f8181e7a7f1d904fbaea64220e83379"), prefix, classes, stat, resource, archetypes: getArchetypeArray());

            ability.SetNameDescription("Sudden Speed",
                                       "As a swift action, you can expend 1 point of mental focus to grant yourself a burst of speed. This increases your land speed by 30 feet for 1 minute. This ability does not stack with itself.");
            ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(Common.changeAction<ContextActionApplyBuff>(a.Actions.Actions, c => c.DurationValue = Helpers.CreateContextDuration(1, DurationRate.Minutes))));
            ability.ActionType = CommandType.Swift;
            ability.LocalizedDuration = Helpers.CreateString(ability.name + ".Duration", Helpers.oneMinuteDuration);
            addFocusInvestmentCheck(ability, SpellSchool.Transmutation);
            return Common.AbilityToFeature(ability, false);
        }


        public BlueprintFeature createSizeAlteration()
        {
            var enlarge = library.Get<BlueprintAbility>("c60969e7f264e6d4b84a1499fdcf9039");
            var reduce = library.Get<BlueprintAbility>("4e0e9aba6447d514f88eff1464cc4763");

            var enlarge_sp = Common.convertToSpellLike(enlarge, prefix, classes, stat, resource, archetypes: getArchetypeArray());
            var reduce_sp = Common.convertToSpellLike(reduce, prefix, classes, stat, resource, archetypes: getArchetypeArray());
            enlarge_sp.RemoveComponents<CompanionMechanics.AbilityTargetHasFactUnlessPet>();
            reduce_sp.RemoveComponents<CompanionMechanics.AbilityTargetHasFactUnlessPet>();
            enlarge_sp.SetName("Size Alteration: " + enlarge_sp.Name);
            reduce_sp.SetName("Size Alteration: " + reduce_sp.Name);
            Common.unsetAsFullRoundAction(reduce_sp);
            Common.unsetAsFullRoundAction(enlarge_sp);
            var wrapper = Common.createVariantWrapper(prefix + "SizeAlterationAbilityBase", "", enlarge_sp, reduce_sp);
            wrapper.SetNameDescription("Size Alteration",
                                       "As a standard action, you can expend 1 point of mental focus and touch a creature to alter its size.\n"
                                       + "You can increase or decrease the creature’s size by one step, as enlarge person or reduce person but not limited by the creature’s type.");

            addFocusInvestmentCheck(wrapper, SpellSchool.Transmutation);
            return Common.AbilityToFeature(wrapper, false);
        }


        public BlueprintFeature createQuickness()
        {
            var haste_buff = library.Get<BlueprintBuff>("03464790f40c3c24aa684b57155f3280"); //haste
            var apply_haste = Common.createContextActionApplyBuff(haste_buff, Helpers.CreateContextDuration(), is_child: true, dispellable: false, is_permanent: true);

            var effect_buff = Helpers.CreateBuff(prefix + "QuicknessBuff",
                                    "Quickness",
                                    "As a standard action, you can expend 1 point of mental focus to grant supernatural quickness and reflexes to yourself or a willing living creature you touch.\n"
                                    + "This functions as haste, but the bonus to attack rolls, AC and on Reflex saving throws increases by 1 for every 6 occultist levels. This effect lasts for 1 round per occultist level you possess. You must be at least 5th level to select this focus power.",
                                    "",
                                    haste_buff.Icon,
                                    null,
                                    Helpers.CreateAddContextStatBonus(StatType.AdditionalAttackBonus, ModifierDescriptor.UntypedStackable),
                                    Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.Dodge),
                                    Helpers.CreateAddContextStatBonus(StatType.SaveReflex, ModifierDescriptor.UntypedStackable),
                                    createClassScalingConfig(ContextRankProgression.DivStep, stepLevel: 6),
                                    Helpers.CreateAddFactContextActions(activated: apply_haste)
                                    );

            var ability = library.CopyAndAdd<BlueprintAbility>("486eaff58293f6441a5c2759c4872f98", prefix + "QuicknessAbility", "");//from haste
            ability.RemoveComponents<SpellListComponent>();
            ability.RemoveComponents<AbilityTargetsAround>();
            ability.Range = AbilityRange.Touch;
            ability.Type = AbilityType.SpellLike;
            ability.SetNameDescription(effect_buff.Name, effect_buff.Description);
            ability.AddComponent(Common.createAbilityTargetHasFact(true, Common.undead, Common.construct, Common.elemental));
            ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(Common.changeAction<ContextActionApplyBuff>(a.Actions.Actions, c => c.Buff = effect_buff)));
            ability.AddComponent(createClassScalingConfig());
            ability.AddComponent(resource.CreateResourceLogic());
            addFocusInvestmentCheck(ability, SpellSchool.Transmutation);

            var feature = Common.AbilityToFeature(ability, false);
            addMinLevelPrerequisite(feature, 5);
            return feature;
        }


        public BlueprintBuff[] createPhysicalEnhancement()
        {
            var property = ImplementMechanics.InvestedImplementFocusAmountProperty.createProperty(prefix + "PhysicalEnhancementProperty", "",
                                                                                                  createClassScalingConfig(ContextRankProgression.StartPlusDivStep, type: AbilityRankType.StatBonus, startLevel: -4, stepLevel: 2),//1 + (lvl + 4)/2 = 3 + lvl/2
                                                                                                  false,
                                                                                                  SpellSchool.Transmutation);

            var stats = new StatType[] { StatType.Constitution, StatType.Dexterity, StatType.Strength };
            var icons = new UnityEngine.Sprite[] { Helpers.GetIcon("99cf556b967c2074ca284e127d815711"), Helpers.GetIcon("3553bda4d6dfe6344ad89b25f7be939a"), Helpers.GetIcon("c7773d1b408fea24dbbb0f7bf3eb864e") };
            var buffs = new BlueprintBuff[3];

            for (int i = 0; i < 3; i++)
            {
                buffs[i] = Helpers.CreateBuff(prefix + stats[i].ToString() + "PhysicalEnhancementBuff",
                                              "Physical Enhancement: " + stats[i].ToString(),
                                              "The implement enhances its bearer’s body. When you invest mental focus in the implement, select a physical ability score. The implement grants a +2 temporary enhancement bonus to that physical ability score for every 3 points of mental focus invested in the implement (to a maximum of +2 at 1st level, plus an additional 2 for every 6 occultist levels you possess).",
                                              "",
                                              icons[i],
                                              null,
                                              Helpers.CreateAddContextStatBonus(stats[i], ModifierDescriptor.Enhancement, multiplier: 2),
                                              Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, ContextRankProgression.DivStep, stepLevel: 3,
                                                                              customProperty: property)                                            
                                              );
            }
            return buffs;
        }
    }
}
