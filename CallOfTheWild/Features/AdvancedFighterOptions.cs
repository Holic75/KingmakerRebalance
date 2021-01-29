using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    class AdvancedFighterOptions
    {
        static LibraryScriptableObject library => Main.library;
        static public BlueprintFeatureSelection advanced_armor_training;
        static public BlueprintFeatureSelection advanced_weapon_training = library.Get<BlueprintFeatureSelection>("b8cecf4e5e464ad41b79d5b42b76b399");
        static public BlueprintFeatureSelection weapon_training_rankup = library.Get<BlueprintFeatureSelection>("5f3cc7b9a46b880448275763fe70c0b0");
        static public BlueprintFeature monk_weapon_group, thrown_weapon_group;
        static BlueprintFeature two_handed_weapon_training = Main.library.Get<BlueprintFeature>("88da2a5dfc505054f933bb81014e864f");
        static BlueprintCharacterClass fighter = library.Get<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");
        /////////////weapon training/////////////////
        static public BlueprintFeature dazzling_intimidation;
        static public BlueprintFeature defensive_weapon_training;
        static public BlueprintFeature fighters_finesse;
        static public BlueprintFeatureSelection focus_weapon;
        static public BlueprintFeature trained_grace;
        static public BlueprintFeature trained_throw;
        static public BlueprintFeatureSelection versatile_training;
        static public BlueprintFeatureSelection warrior_spirit;

        /////////////armor training////////////////////
        static public BlueprintFeatureSelection adaptable_training;
        static public BlueprintFeatureSelection armor_specialization;
        static public BlueprintFeature armored_confidence;
        static public BlueprintFeature armored_juggernaut;
        static public BlueprintFeature critical_deflection;
        static public BlueprintFeature steel_headbutt;

        static Dictionary<WeaponCategory, WeaponFighterGroup> category_group_map = new Dictionary<WeaponCategory, WeaponFighterGroup>();
        static Dictionary<WeaponCategory, BlueprintFeature> category_training_map = new Dictionary<WeaponCategory, BlueprintFeature>();
        static public Dictionary<WeaponCategory, BlueprintFeature> category_finesse_training_map = new Dictionary<WeaponCategory, BlueprintFeature>();
        static public Dictionary<WeaponFighterGroup, BlueprintFeature> group_training_map = new Dictionary<WeaponFighterGroup, BlueprintFeature>();


        static public BlueprintFeatureSelection[] extra_armor_training_feats;
        static public BlueprintFeatureSelection[] extra_weapon_training_feats;

        static public List<WeaponCategory> two_handed_categories = new List<WeaponCategory>();

        static WeaponFighterGroup sacred_weapon_group = (WeaponFighterGroup)100;

        public static void load()
        {
            Main.logger.Log("Enabling Advanced Fighter Training Options");
            createAdvancedArmorTraining();
            createMoreWeaponGroups();

            prepareLookupData();

            createVerstileTrainingAndAdaptableTraining();
            createDazzlingIntimidation();
            createDefensiveWeaponTraining();
            createFightersFinesse();
            createFocusWeapon();
            createTrainedGrace();
            createTrainedThrow();
            createWarriorSpirit();

            createArmorSpecialization();
            createArmoredConfidence();
            createArmoredJuggernaut();
            createCriticalDeflection();
            createSteelHeadbutt();

            createFeats();
        }


        static internal void prepareLookupData()
        {
            var weapon_types = library.GetAllBlueprints().OfType<BlueprintWeaponType>();

            foreach (var wt in weapon_types)
            {
                category_group_map[wt.Category] = wt.FighterGroup;

                if (wt.IsTwoHanded && !wt.IsRanged)
                {
                    two_handed_categories.Add(wt.Category);
                }
            }

            foreach (var f in weapon_training_rankup.AllFeatures)
            {
                group_training_map.Add(f.GetComponent<WeaponGroupAttackBonus>().WeaponGroup, f);
            }
            group_training_map.Add(WeaponFighterGroup.None, two_handed_weapon_training);
            //group_training_map.Add(sacred_weapon_group, Warpriest.arsenal_chaplain_weapon_training);


            foreach (var wc in category_group_map)
            {
                var feature = weapon_training_rankup.AllFeatures.FirstOrDefault(f => f.GetComponent<WeaponGroupAttackBonus>().WeaponGroup == wc.Value);
                if (feature == null)
                {
                    continue;
                }
                category_training_map[wc.Key] = feature;
            }

            var finesse_training = library.Get<BlueprintFeatureSelection>("b78d146cea711a84598f0acef69462ea");

            foreach (var ft in finesse_training.AllFeatures)
            {
                category_finesse_training_map.Add(ft.GetComponent<WeaponTypeDamageStatReplacement>().Category, ft);
            }
        }


        static void createFeats()
        {
            var armor_training = library.Get<BlueprintFeature>("3c380607706f209499d951b29d3c44f3");
            extra_armor_training_feats = new BlueprintFeatureSelection[4];

            for (int i = 0; i < extra_armor_training_feats.Length; i++ )
            {
                extra_armor_training_feats[i] = Helpers.CreateFeatureSelection($"AdvancedArmorTrainingExtra{i+1}FeatureSelection",
                                                                               "Advanced Armor Training " + Common.roman_id[i+1],
                                                                               "Select one advanced armor training option.",
                                                                               "",
                                                                               advanced_armor_training.Icon,
                                                                               FeatureGroup.Feat,
                                                                               Helpers.PrerequisiteClassLevel(fighter, 3 + i * 5),
                                                                               Helpers.PrerequisiteFeature(armor_training)
                                                                               );
                extra_armor_training_feats[i].Groups = extra_armor_training_feats[i].Groups.AddToArray(FeatureGroup.CombatFeat);
                extra_armor_training_feats[i].AllFeatures = advanced_armor_training.AllFeatures.RemoveFromArray(armor_training);
                extra_armor_training_feats[i].AddComponent(Helpers.PrerequisiteNoFeature(extra_armor_training_feats[i]));
            }
            library.AddCombatFeats(extra_armor_training_feats);

            extra_weapon_training_feats = new BlueprintFeatureSelection[4];

            var advanced_weapon_trainings = advanced_weapon_training.AllFeatures;

            foreach (var t in group_training_map.Values)
            {
                advanced_weapon_trainings = advanced_weapon_trainings.RemoveFromArray(t);               
            }

            foreach (var t in advanced_weapon_trainings)
            {
                t.ReplaceComponent<PrerequisiteClassLevel>(Helpers.PrerequisiteFeaturesFromList(group_training_map.Values));
            }

            for (int i = 0; i < extra_weapon_training_feats.Length; i++)
            {
                extra_weapon_training_feats[i] = Helpers.CreateFeatureSelection($"AdvancedWeaponTrainingExtra{i+1}FeatureSelection",
                                                                               "Advanced Weapon Training " + Common.roman_id[i+1],
                                                                               "Select one advanced weapon training option, applying it to one fighter weapon group you have already selected with the weapon training class feature.",
                                                                               "",
                                                                               Helpers.GetIcon("b8cecf4e5e464ad41b79d5b42b76b399"),
                                                                               FeatureGroup.Feat,
                                                                               Helpers.PrerequisiteClassLevel(fighter, 5 + i * 5),
                                                                               Helpers.PrerequisiteFeaturesFromList(group_training_map.Values)
                                                                               );
                extra_weapon_training_feats[i].Groups = extra_armor_training_feats[i].Groups.AddToArray(FeatureGroup.CombatFeat);
                extra_weapon_training_feats[i].AllFeatures = advanced_weapon_trainings;
                extra_weapon_training_feats[i].AddComponent(Helpers.PrerequisiteNoFeature(extra_weapon_training_feats[i]));
            }
            library.AddCombatFeats(extra_weapon_training_feats);
        }


        static void createSteelHeadbutt()
        {
            var gore1d3 = library.CopyAndAdd<BlueprintItemWeapon>("daf4ab765feba8548b244e174e7af5be", "SteelHeadbutt1d3", "");
            Helpers.SetField(gore1d3, "m_OverrideDamageDice", true);
            Helpers.SetField(gore1d3, "m_DamageDice", new DiceFormula(1, DiceType.D3));

            var gore1d4 = library.CopyAndAdd<BlueprintItemWeapon>("daf4ab765feba8548b244e174e7af5be", "SteelHeadbutt1d4", "");
            Helpers.SetField(gore1d3, "m_OverrideDamageDice", true);
            Helpers.SetField(gore1d3, "m_DamageDice", new DiceFormula(1, DiceType.D4));

            var mithral = library.Get<BlueprintArmorEnchantment>("7b95a819181574a4799d93939aa99aff");
            var adamantine_heavy = library.Get<BlueprintArmorEnchantment>("933456ff83c454146a8bf434e39b1f93");
            var adamantine_medium = library.Get<BlueprintArmorEnchantment>("aa25531ab5bb58941945662aa47b73e7");

            var adamantine_weapon = library.Get<BlueprintWeaponEnchantment>("ab39e7d59dd12f4429ffef5dca88dc7b");
            var cold_iron = library.Get<BlueprintWeaponEnchantment>("e5990dc76d2a613409916071c898eee8");

            var enchant_map = new Dictionary<BlueprintArmorEnchantment, BlueprintWeaponEnchantment>();
            enchant_map.Add(mithral, cold_iron);
            enchant_map.Add(adamantine_heavy, adamantine_weapon);
            enchant_map.Add(adamantine_medium, adamantine_weapon);


            var feature1d3 = Helpers.CreateFeature("SteelHeadbutt1d3Feature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Common.createAddSecondaryAttacks(gore1d3),
                                                   Helpers.Create<NewMechanics.EnchantmentMechanics.RemapBodyArmorEnchantsToSpecificWeapon>(r =>
                                                                                                                                               {
                                                                                                                                                   r.enchantment_map = enchant_map;
                                                                                                                                                   r.transfer_enhancement = true;
                                                                                                                                                   r.target_weapon = gore1d3;
                                                                                                                                               }
                                                                                                                                               )
                                                   );
            feature1d3.HideInCharacterSheetAndLevelUp = true;

            var feature1d4 = Helpers.CreateFeature("SteelHeadbutt1d4Feature",
                                       "",
                                       "",
                                       "",
                                       null,
                                       FeatureGroup.None,
                                       Common.createAddSecondaryAttacks(gore1d4),
                                       Helpers.Create<NewMechanics.EnchantmentMechanics.RemapBodyArmorEnchantsToSpecificWeapon>(r =>
                                       {
                                           r.enchantment_map = enchant_map;
                                           r.transfer_enhancement = true;
                                           r.target_weapon = gore1d4;
                                       }                                                                                                                                  )
                                       );
            feature1d4.HideInCharacterSheetAndLevelUp = true;

            steel_headbutt = Helpers.CreateFeature("SteelHeadbuttAdvancedArmorTrainingFeature",
                                                   "Steel Headbutt",
                                                   "While wearing medium or heavy armor, a fighter can deliver a headbutt with his helm as part of a full attack action. This headbutt is in addition to his normal attacks, and is made using the fighter’s base attack bonus – 5. A helmet headbutt deals 1d3 points of damage if the fighter is wearing medium armor, or 1d4 points of damage if he is wearing heavy armor (1d2 and 1d3, respectively, for Small creatures), plus an amount of damage equal to the fighter’s Strength modifier. Treat this attack as a weapon attack made using the same special material and echantment bonus (if any) as the armor.",
                                                   "",
                                                   Helpers.GetIcon("4c3d08935262b6544ae97599b3a9556d"), //bulls stength
                                                   FeatureGroup.None,
                                                   Helpers.Create<WeaponTrainingMechanics.AddFeatureOnArmor>(a => { a.feature = feature1d3; a.required_armor = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.Medium }; }),
                                                   Helpers.Create<WeaponTrainingMechanics.AddFeatureOnArmor>(a => { a.feature = feature1d4; a.required_armor = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.Heavy }; })
                                                   );
            addToAdvancedArmorTraining(steel_headbutt);
        }


        static void createCriticalDeflection()
        {
            critical_deflection = Helpers.CreateFeature("CriticalDeflectionAdvancedArmorTrainingFeature",
                                                       "Critical Deflection",
                                                       "While wearing armor or using a shield, the fighter gains a +2 bonus to his AC against attack rolls made to confirm a critical hit. This bonus increases by 1 at 7th level and every 4 fighter levels thereafter, to a maximum of +6 at 19th level.",
                                                       "",
                                                       Helpers.GetIcon("d09b20029e9abfe4480b356c92095623"),
                                                       FeatureGroup.None);

            var armor_types = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.Light, ArmorProficiencyGroup.Medium, ArmorProficiencyGroup.Heavy, ArmorProficiencyGroup.Buckler,
                                                             ArmorProficiencyGroup.LightShield, ArmorProficiencyGroup.HeavyShield, ArmorProficiencyGroup.TowerShield };

            var feature = Helpers.CreateFeature("CriticalDeflectionAdvancedArmorTrainingEffectFeature",
                                                critical_deflection.Name,
                                                critical_deflection.Description,
                                                "",
                                                critical_deflection.Icon,
                                                FeatureGroup.None,
                                                Helpers.Create<CriticalConfirmationACBonus>(c => c.Value = Helpers.CreateContextValue(AbilityRankType.Default)),
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { fighter },
                                                                                progression: ContextRankProgression.StartPlusDivStep, startLevel: -1,
                                                                                stepLevel: 4)
                                               );
            feature.HideInCharacterSheetAndLevelUp = true;
            critical_deflection.AddComponent(Helpers.Create<WeaponTrainingMechanics.AddFeatureOnArmor>(a => { a.feature = feature; a.required_armor = armor_types; }));
            addToAdvancedArmorTraining(critical_deflection);
        }


        static void createArmoredJuggernaut()
        {
            armored_juggernaut = Helpers.CreateFeature("ArmoredJuggernautAdvancedArmorTrainingFeature",
                                                       "Armored Juggernaut",
                                                       "When wearing heavy armor, the fighter gains DR 1/—. At 7th level, the fighter gains DR 1/— when wearing medium armor, and DR 2/— when wearing heavy armor. At 11th level, the fighter gains DR 1/— when wearing light armor, DR 2/— when wearing medium armor, and DR 3/— when wearing heavy armor. If the fighter is 19th level and has the armor mastery class feature, these DR values increase by 5. The DR from this ability stacks with that provided by adamantine armor, but not with other forms of damage reduction.",
                                                       "",
                                                       Helpers.GetIcon("479c7f3b0dba69a4bbcb43e101f3f7f9"),
                                                       FeatureGroup.None);

            var armor_types = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.Light, ArmorProficiencyGroup.Medium, ArmorProficiencyGroup.Heavy };

            foreach (var at in armor_types)
            {
                int shift = at == ArmorProficiencyGroup.Heavy ? 0 : (at == ArmorProficiencyGroup.Medium ? 1 : 2);
                var feature = Helpers.CreateFeature(at.ToString() + "ArmoredJuggernautAdvancedArmorTrainingFeature",
                                                    armored_juggernaut.Name,
                                                    armored_juggernaut.Description,
                                                    "",
                                                    armored_juggernaut.Icon,
                                                    FeatureGroup.None,
                                                    Common.createContextPhysicalDR(Helpers.CreateContextValue(AbilityRankType.Default)),
                                                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { fighter },
                                                                                    progression: ContextRankProgression.StartPlusDivStep, startLevel: 3 + shift * 4,
                                                                                    stepLevel: 4, max: 3 - shift )
                                                   );
                feature.HideInCharacterSheetAndLevelUp = true;
                armored_juggernaut.AddComponent(Helpers.Create<WeaponTrainingMechanics.AddFeatureOnArmor>(a => { a.feature = feature; a.required_armor = new ArmorProficiencyGroup[] { at }; }));
            }
            addToAdvancedArmorTraining(armored_juggernaut);
        }


        static void createArmoredConfidence()
        {
            armored_confidence = Helpers.CreateFeature("ArmoredConfidenceAdvancedArmorTrainingFeature",
                                                       "Armored Confidence",
                                                       "While wearing armor, the fighter gains a bonus on Intimidate checks based upon the type of armor he is wearing: +1 for light armor, +2 for medium armor, or +3 for heavy armor. This bonus increases by 1 at 7th level and every 4 fighter levels thereafter, to a maximum of +4 at 19th level.",
                                                       "",
                                                       Helpers.GetIcon("d76497bfc48516e45a0831628f767a0f"),
                                                       FeatureGroup.None);

            var armor_types = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.Light, ArmorProficiencyGroup.Medium, ArmorProficiencyGroup.Heavy };

            foreach (var at in armor_types)
            {
                int shift = at == ArmorProficiencyGroup.Light ? 0 : (at == ArmorProficiencyGroup.Medium ? 1 : 2);
                var feature = Helpers.CreateFeature(at.ToString() + "ArmoredConfidenceAdvancedArmorTrainingFeature",
                                                    armored_confidence.Name,
                                                    armored_confidence.Description,
                                                    "",
                                                    armored_confidence.Icon,
                                                    FeatureGroup.None,
                                                    Helpers.CreateAddContextStatBonus(StatType.CheckIntimidate, ModifierDescriptor.UntypedStackable),
                                                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { fighter },
                                                                                    progression: ContextRankProgression.StartPlusDivStep, startLevel: 3 - shift * 4,
                                                                                    stepLevel: 4)
                                                   );
                feature.HideInCharacterSheetAndLevelUp = true;
                armored_confidence.AddComponent(Helpers.Create<WeaponTrainingMechanics.AddFeatureOnArmor>(a => { a.feature = feature; a.required_armor = new ArmorProficiencyGroup[] { at }; }));
            }
            addToAdvancedArmorTraining(armored_confidence);
        }





        static void createArmorSpecialization()
        {
            var armor_focus = library.Get<BlueprintFeatureSelection>("76d4885a395976547a13c5d6bf95b482");

            armor_specialization = Helpers.CreateFeatureSelection("ArmorSpecializationAdvancedArmorTrainingFeature",
                                                         "Armor Specialization",
                                                         "The fighter selects one specific type of armor with which he is proficient, such as light or heavy. While wearing the selected type of armor, the fighter adds one-quarter of his fighter level to the armor’s armor bonus, up to a maximum bonus of +3 for light armor, +4 for medium armor, or +5 for heavy armor. This increase to the armor bonus doesn’t increase the benefit that the fighter gains from feats, class abilities, or other effects that are determined by his armor’s base armor bonus, including other advanced armor training options. A fighter can choose this option multiple times. Each time he chooses it, he applies its benefit to a different type of armor.",
                                                         "",
                                                         armor_focus.Icon,
                                                         FeatureGroup.None);
            foreach (var f in armor_focus.AllFeatures)
            {
                var feature = library.CopyAndAdd(f, f.name.Replace("Focus", "Specialization"), "");
                var armor_type = f.GetComponent<ArmorFocus>().ArmorCategory;
                feature.SetNameDescription(armor_type.ToString() + " Armor Specialization",
                                           armor_specialization.Description);
                feature.RemoveComponents<ArmorFocus>();
                feature.RemoveComponents<PrerequisiteStatValue>();

                int max_armor = armor_type == ArmorProficiencyGroup.Light ? 3 : armor_type == ArmorProficiencyGroup.Medium ? 4 : 5;

                var effect_feature = library.CopyAndAdd(feature, feature.name + "Effect", "");
                effect_feature.ComponentsArray = new BlueprintComponent[]
                {
                    Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.ArmorFocus),
                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { fighter}, progression: ContextRankProgression.DivStep,
                                                    stepLevel: 4, max: max_armor)
                };

                effect_feature.HideInUI = true;
                feature.AddComponent(Helpers.Create<WeaponTrainingMechanics.AddFeatureOnArmor>(a => { a.feature = effect_feature; a.required_armor = new ArmorProficiencyGroup[] { armor_type }; }));
                armor_specialization.AllFeatures = armor_specialization.AllFeatures.AddToArray(feature);
            }
            addToAdvancedArmorTraining(armor_specialization);
        }

        static void createWarriorSpirit()
        {
            var gloves_of_dueling1_feature = library.Get<BlueprintFeature>("5238b7d5f4c81574ba914d609ac1e692");
            var gloves_of_dueling2_feature = library.Get<BlueprintFeature>("f063942ce72136c49a12d34a3fd88197");

            var warrior_spirit_group = ActivatableAbilityGroupExtension.WarriorSpirit.ToActivatableAbilityGroup();
            var resource = Helpers.CreateAbilityResource("WarriorSpiritResource", "", "", "", null);
            resource.SetFixedResource(1);

            var divine_weapon = library.Get<BlueprintAbility>("7ff088ab58c69854b82ea95c2b0e35b4");
            var enchants = WeaponEnchantments.temporary_enchants;

            var enhancement_buff = Helpers.CreateBuff("WarriorSpiritWeaponEnchancementBaseBuff",
                                         "",
                                         "",
                                         "",
                                         null,
                                         null,
                                         Common.createBuffRemainingGroupsSizeEnchantPrimaryHandWeapon(warrior_spirit_group,
                                                                                                      false, true,
                                                                                                      enchants
                                                                                                      )
                                         );
            var weapon_enhancement_buff = Helpers.CreateBuff("WarriorSpiritWeaponEnchancementSwitchBuff",
                                                                 "Warrior Spirit",
                                                                 "The fighter can forge a spiritual bond with a weapon that belongs to the associated weapon group, allowing him to unlock the weapon’s potential.\n"
                                                                 + "Each day, he designates one such weapon and gains a number of points of spiritual energy equal to 1 + his weapon training bonus. While wielding this weapon, he can spend 1 point of spiritual energy to grant the weapon an enhancement bonus equal to his weapon training bonus. Enhancement bonuses gained by this advanced weapon training option stack with those of the weapon, to a maximum of +5.\n"
                                                                 + "The fighter can also imbue the weapon with any weapon special abilities with an equivalent total enhancement bonus less than or equal to his maximum bonus by reducing the granted enhancement bonus by the amount of the equivalent enhancement bonus.",
                                                                 "",
                                                                 divine_weapon.Icon,
                                                                 null,
                                                                 Helpers.CreateAddFactContextActions(activated: Common.createContextActionApplyBuff(enhancement_buff, Helpers.CreateContextDuration(),
                                                                                                                is_child: true, is_permanent: true, dispellable: false)
                                                                                                     )
                                                                 );
            enhancement_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var brilliant_energy = Common.createEnchantmentAbility("WarriorSpiritEnchancementBrilliantEnergy",
                                                                        "Warrior Spirit - Brilliant Energy",
                                                                        "A fighter can add the brilliant energy property to her weapon, but this consumes 4 points of enhancement bonus granted to this weapon.\nA brilliant energy weapon ignores nonliving matter.Armor and shield bonuses to AC(including any enhancement bonuses to that armor) do not count against it because the weapon passes through armor. (Dexterity, deflection, dodge, natural armor, and other such bonuses still apply.) A brilliant energy weapon cannot harm undead, constructs, or objects.",
                                                                        library.Get<BlueprintActivatableAbility>("f1eec5cc68099384cbfc6964049b24fa").Icon,
                                                                        weapon_enhancement_buff,
                                                                        library.Get<BlueprintWeaponEnchantment>("66e9e299c9002ea4bb65b6f300e43770"),
                                                                        4, warrior_spirit_group);

            var flaming = Common.createEnchantmentAbility("WarriorSpiritEnchancementFlaming",
                                                                "Warrior Spirit - Flaming",
                                                                "A fighter can add the flaming property to her weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA flaming weapon is sheathed in fire that deals an extra 1d6 points of fire damage on a successful hit. The fire does not harm the wielder.",
                                                                library.Get<BlueprintActivatableAbility>("7902941ef70a0dc44bcfc174d6193386").Icon,
                                                                weapon_enhancement_buff,
                                                                library.Get<BlueprintWeaponEnchantment>("30f90becaaac51f41bf56641966c4121"),
                                                                1, warrior_spirit_group);

            var frost = Common.createEnchantmentAbility("WarriorSpiritEnchancementFrost",
                                                            "Warrior Spirit - Frost",
                                                            "A fighter can add the frost property to her weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA frost weapon is sheathed in a terrible, icy cold that deals an extra 1d6 points of cold damage on a successful hit. The cold does not harm the wielder.",
                                                            library.Get<BlueprintActivatableAbility>("b338e43a8f81a2f43a73a4ae676353a5").Icon,
                                                            weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("421e54078b7719d40915ce0672511d0b"),
                                                            1, warrior_spirit_group);

            var shock = Common.createEnchantmentAbility("WarriorSpiritEnchancementShock",
                                                            "Warrior Spirit - Shock",
                                                            "A fighter can add the shock property to her weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA shock weapon is sheathed in crackling electricity that deals an extra 1d6 points of electricity damage on a successful hit. The electricity does not harm the wielder.",
                                                            library.Get<BlueprintActivatableAbility>("a3a9e9a2f909cd74e9aee7788a7ec0c6").Icon,
                                                            weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("7bda5277d36ad114f9f9fd21d0dab658"),
                                                            1, warrior_spirit_group);

            var ghost_touch = Common.createEnchantmentAbility("WarriorSpiritEnchancementGhostTouch",
                                                                    "Warrior Spirit - Ghost Touch",
                                                                    "A fighter can add the ghost touch property to her weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA ghost touch weapon deals damage normally against incorporeal creatures, regardless of its bonus. An incorporeal creature's 50% reduction in damage from corporeal sources does not apply to attacks made against it with ghost touch weapons.",
                                                                    library.Get<BlueprintActivatableAbility>("688d42200cbb2334c8e27191c123d18f").Icon,
                                                                    weapon_enhancement_buff,
                                                                    library.Get<BlueprintWeaponEnchantment>("47857e1a5a3ec1a46adf6491b1423b4f"),
                                                                    1, warrior_spirit_group);

            var keen = Common.createEnchantmentAbility("WarriorSpiritEnchancementKeen",
                                                            "Warrior Spirit - Keen",
                                                            "A fighter can add the keen property to her weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nThe keen property doubles the threat range of a weapon. This benefit doesn't stack with any other effects that expand the threat range of a weapon (such as the keen edge spell or the Improved Critical feat).",
                                                            library.Get<BlueprintActivatableAbility>("27d76f1afda08a64d897cc81201b5218").Icon,
                                                            weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("102a9c8c9b7a75e4fb5844e79deaf4c0"),
                                                            1, warrior_spirit_group);

            var disruption = Common.createEnchantmentAbility("WarriorSpiritEnchancementDisruption",
                                                                    "Warrior Spirit - Disruption",
                                                                    "A fighter can add the disruption property to her weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nA disruption weapon is the bane of all undead. Any undead creature struck in combat must succeed on a DC 14 Will save or be destroyed. A disruption weapon must be a bludgeoning melee weapon.",
                                                                    library.Get<BlueprintActivatableAbility>("27d76f1afda08a64d897cc81201b5218").Icon,
                                                                    weapon_enhancement_buff,
                                                                    library.Get<BlueprintWeaponEnchantment>("0f20d79b7049c0f4ca54ca3d1ea44baa"),
                                                                    2, warrior_spirit_group);

            var holy = Common.createEnchantmentAbility("WarriorSpiritEnchancementHoly",
                                                            "Warrior Spirit - Holy",
                                                            "A fighter can add the holy property to her weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nA holy weapon is imbued with holy power. This power makes the weapon good-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of evil alignment.",
                                                            library.Get<BlueprintActivatableAbility>("ce0ece459ebed9941bb096f559f36fa8").Icon,
                                                            weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("28a9964d81fedae44bae3ca45710c140"),
                                                            2, warrior_spirit_group);

            var unholy = Common.createEnchantmentAbility("WarriorSpiritEnchancementUnholy",
                                                            "Warrior Spirit - Unholy",
                                                            "A fighter can add the unholy property to her weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn unholy weapon is imbued with unholy power. This power makes the weapon evil-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of good alignment.",
                                                            library.Get<BlueprintActivatableAbility>("561803a819460f34ea1fe079edabecce").Icon,
                                                            weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("d05753b8df780fc4bb55b318f06af453"),
                                                            2, warrior_spirit_group);

            var axiomatic = Common.createEnchantmentAbility("WarriorSpiritEnchancementAxiomatic",
                                                            "Warrior Spirit - Axiomatic",
                                                            "A fighter can add the axiomatic property to her weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn axiomatic weapon is infused with lawful power. It makes the weapon lawful-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against chaotic creatures.",
                                                            library.Get<BlueprintActivatableAbility>("d76e8a80ab14ac942b6a9b8aaa5860b1").Icon,
                                                            weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("0ca43051edefcad4b9b2240aa36dc8d4"),
                                                            2, warrior_spirit_group);

            var anarchic = Common.createEnchantmentAbility("WarriorSpiritEnchancementAnarchic",
                                                "Warrior Spirit - Anarchic",
                                                "A fighter can add the anarchic property to her weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn anarchic weapon is infused with the power of chaos. It makes the weapon chaotic-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of lawful alignment.",
                                                LoadIcons.Image2Sprite.Create(@"AbilityIcons/HWAnarchic.png"),
                                                weapon_enhancement_buff,
                                                library.Get<BlueprintWeaponEnchantment>("57315bc1e1f62a741be0efde688087e9"),
                                                2, warrior_spirit_group);

            var vicious = Common.createEnchantmentAbility("WarriorSpiritEnchancementVicious",
                                                        "Warrior Spirit - Vicious",
                                                        $"A fighter can add the vicious property to her weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\n{WeaponEnchantments.vicious.Description}",
                                                        LoadIcons.Image2Sprite.Create(@"AbilityIcons/HWVicious.png"),
                                                        weapon_enhancement_buff,
                                                        WeaponEnchantments.vicious,
                                                        1, warrior_spirit_group);

            var vorpal = Common.createEnchantmentAbility("WarriorSpiritEnchancementVorpal",
                                                        "Warrior Spirit - Vorpal",
                                                        $"A fighter can add the vorpal property to her weapon, but this consumes 5 points of enhancement bonus granted to this weapon.\n{WeaponEnchantments.vorpal.Description}",
                                                        LoadIcons.Image2Sprite.Create(@"AbilityIcons/HWVorpal.png"),
                                                        weapon_enhancement_buff,
                                                        WeaponEnchantments.vorpal,
                                                        5, warrior_spirit_group);

            var cruel = Common.createEnchantmentAbility("WarriorSpiritEnchancementCruel",
                                                        "Warrior Spirit - Cruel",
                                                        $"A fighter can add the cruel property to her weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\n{WeaponEnchantments.cruel.Description}",
                                                        LoadIcons.Image2Sprite.Create(@"AbilityIcons/HWCruel.png"),
                                                        weapon_enhancement_buff,
                                                        WeaponEnchantments.cruel,
                                                        1, warrior_spirit_group);

            var bane = Common.createEnchantmentAbility("WarriorSpiritEnchancementBane",
                                                        "Warrior Spirit - Bane",
                                                        $"A fighter can add the bane property to her weapon, but this consumes 4 points of enhancement bonus granted to this weapon.\n{WeaponEnchantments.bane.Description}",
                                                        LoadIcons.Image2Sprite.Create(@"AbilityIcons/HWBane.png"),
                                                        weapon_enhancement_buff,
                                                        WeaponEnchantments.bane,
                                                        4, warrior_spirit_group);

            var speed_enchant = library.Get<BlueprintWeaponEnchantment>("f1c0c50108025d546b2554674ea1c006");
            var speed = Common.createEnchantmentAbility("WarriorSpiritEnchancementSpeed",
                                                            "Warrior Spirit - Speed",
                                                            "A fighter can add the vicious property to her weapon, but this consumes 3 points of enhancement bonus granted to this weapon.\n" + speed_enchant.Description,
                                                            library.Get<BlueprintActivatableAbility>("ed1ef581af9d9014fa1386216b31cdae").Icon, //speed
                                                            weapon_enhancement_buff,
                                                            speed_enchant,
                                                            3, warrior_spirit_group);

            var warrior_spirit_features = new BlueprintFeature[5];
            warrior_spirit_features[0] = Helpers.CreateFeature("WarriorSpiritEnchancementFeature",
                                                            "Warrior Spirit +1",
                                                            weapon_enhancement_buff.Description,
                                                            "",
                                                            weapon_enhancement_buff.Icon,
                                                            FeatureGroup.None,
                                                            Helpers.CreateAddFacts(flaming, frost, shock, ghost_touch, keen, cruel, vicious),
                                                            resource.CreateIncreaseResourceAmount(1)
                                                            );

            warrior_spirit_features[1] = Helpers.CreateFeature("WarriorSpiritEnchancement2Feature",
                                                            "Warrior Spirit +2",
                                                            weapon_enhancement_buff.Description,
                                                            "",
                                                            weapon_enhancement_buff.Icon,
                                                            FeatureGroup.None,
                                                            Common.createIncreaseActivatableAbilityGroupSize(warrior_spirit_group),
                                                            Helpers.CreateAddFacts(disruption, holy, unholy, axiomatic, anarchic),
                                                            resource.CreateIncreaseResourceAmount(1)
                                                            );

            warrior_spirit_features[2] = Helpers.CreateFeature("WarriorSpiritEnchancement3Feature",
                                                                            "Warrior Spirit +3",
                                                                            weapon_enhancement_buff.Description,
                                                                            "",
                                                                            weapon_enhancement_buff.Icon,
                                                                            FeatureGroup.None,
                                                                            Helpers.CreateAddFacts(speed),
                                                                            Common.createIncreaseActivatableAbilityGroupSize(warrior_spirit_group),
                                                                            resource.CreateIncreaseResourceAmount(1)
                                                                            );

            warrior_spirit_features[3] = Helpers.CreateFeature("WarriorSpiritEnchancement4Feature",
                                                                            "Warrior Spirit +4",
                                                                            weapon_enhancement_buff.Description,
                                                                            "",
                                                                            weapon_enhancement_buff.Icon,
                                                                            FeatureGroup.None,
                                                                            Common.createIncreaseActivatableAbilityGroupSize(warrior_spirit_group),
                                                                            Helpers.CreateAddFacts(brilliant_energy, bane),
                                                                            resource.CreateIncreaseResourceAmount(1)
                                                                            );

            warrior_spirit_features[4] = Helpers.CreateFeature("WarriorSpiritEnchancement5Feature",
                                                                            "Warrior Spirit +5",
                                                                            weapon_enhancement_buff.Description,
                                                                            "",
                                                                            weapon_enhancement_buff.Icon,
                                                                            FeatureGroup.None,
                                                                            Helpers.CreateAddFact(vorpal),
                                                                            Common.createIncreaseActivatableAbilityGroupSize(warrior_spirit_group),
                                                                            resource.CreateIncreaseResourceAmount(1)
                                                                            );


            warrior_spirit = Helpers.CreateFeatureSelection("WarriorSpiritEnchancementFeatureSelection",
                                                            weapon_enhancement_buff.Name,
                                                            weapon_enhancement_buff.Description,
                                                            "",
                                                            weapon_enhancement_buff.Icon,
                                                            FeatureGroup.None);

            var apply_buff = Common.createContextActionApplyBuff(weapon_enhancement_buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false);
            foreach (var f in group_training_map)
            {
                var group_name = f.Value == two_handed_weapon_training ? "Two-Handed Weapons" 
                                            : (f.Value == Warpriest.arsenal_chaplain_weapon_training ? "Sacred Weapons" : getGroupName(f.Key));
                var warrior_spirit_ability = Helpers.CreateAbility(f.Key.ToString() + "WarriorSpiritEnchantmentAbility",
                                                                    weapon_enhancement_buff.Name + " (" + group_name + ")",
                                                                    weapon_enhancement_buff.Description,
                                                                    "",
                                                                    weapon_enhancement_buff.Icon,
                                                                    AbilityType.Supernatural,
                                                                    Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                                    AbilityRange.Personal,
                                                                    Helpers.oneMinuteDuration,
                                                                    "",
                                                                    Helpers.CreateRunActions(apply_buff),
                                                                    resource.CreateResourceLogic(),
                                                                    Helpers.Create<NewMechanics.AbilityCasterMainWeaponGroupCheck>(a => { a.groups = new WeaponFighterGroup[] { f.Key }; a.is_2h = f.Key == WeaponFighterGroup.None; a.is_sacred = f.Key == sacred_weapon_group; })
                                                                    );
                warrior_spirit_ability.setMiscAbilityParametersSelfOnly();
                var feature = Common.AbilityToFeature(warrior_spirit_ability, false);
                feature.AddComponents(resource.CreateAddAbilityResource(),
                                      Helpers.Create<ResourceMechanics.ContextIncreaseResourceAmount>(c => { c.Resource = resource; c.Value = Helpers.CreateContextValue(AbilityRankType.Default); }),
                                      Helpers.PrerequisiteFeature(f.Value)
                                      );
                for (int i = 0; i < warrior_spirit_features.Length; i++)
                {
                    feature.AddComponent(Helpers.Create<NewMechanics.AddFeatureOnFactRank>(a => 
                                                                                          { a.feature = warrior_spirit_features[i];
                                                                                            a.fact_rank = i + 1;
                                                                                            a.checked_fact = f.Value;
                                                                                            a.additional_triggering_features = new BlueprintFeature[] { gloves_of_dueling1_feature, gloves_of_dueling2_feature }; }
                                                                                          )
                                       );
                }
                feature.ReapplyOnLevelUp = true;
                warrior_spirit.AllFeatures = warrior_spirit.AllFeatures.AddToArray(feature);
            }
            warrior_spirit.AddComponent(Helpers.PrerequisiteNoFeature(warrior_spirit));
            addToAdvancedWeaponTraining(warrior_spirit);
        }


        static void createDazzlingIntimidation()
        {
            var dazzling_display = library.Get<BlueprintAbility>("5f3126d4120b2b244a95cb2ec23d69fb");
            var demoralize = library.Get<BlueprintAbility>("7d2233c3b7a0b984ba058a83b736e6ac");

            dazzling_intimidation = Helpers.CreateFeature("DazzlingIntimidationAdvancedWeaponTrainingFeature",
                                                          "Dazzling Intimidation",
                                                          "The fighter applies his weapon training bonus to Intimidate checks and can attempt an Intimidate check to demoralize an opponent as a move action instead of a standard action. If he has the Dazzling Display feat, he can use it as a standard action instead of a full-round action.",
                                                          "",
                                                          dazzling_display.Icon,
                                                          FeatureGroup.None,
                                                          Helpers.Create<TurnActionMechanics.StandardActionIfWeaponTraining>(s => s.abilities = new BlueprintAbility[] { dazzling_display }),
                                                          Helpers.Create<TurnActionMechanics.MoveActionIfWeaponTraining>(s => s.abilities = new BlueprintAbility[] { demoralize }),
                                                          Helpers.Create<WeaponTrainingBonuses>(w => { w.Stat = StatType.CheckIntimidate; w.Descriptor = ModifierDescriptor.UntypedStackable; })
                                                          );
            addToAdvancedWeaponTraining(dazzling_intimidation);
        }


        static void createDefensiveWeaponTraining()
        {
            var defensive_weapon_training_effect = Helpers.CreateFeature("DefensiveWeaponTrainingAdvancedWeaponTrainingEffectFeature",
                                                          "Defensive Weapon Training",
                                                          "The fighter gains a +1 shield bonus to his Armor Class. The fighter adds half his weapon’s enhancement bonus (if any) to this shield bonus. When his weapon training bonus for weapons from the associated fighter weapon group reaches +4, this shield bonus increases to +2. This shield bonus is lost if the fighter is immobilized or helpless.",
                                                          "",
                                                          Helpers.GetIcon("4c44724ffa8844f4d9bedb5bb27d144a"), //combat expertise
                                                          FeatureGroup.None,
                                                          Helpers.Create<AddContextStatBonus>(a => { a.Stat = StatType.AC; a.Descriptor = ModifierDescriptor.Shield; a.Value = Helpers.CreateContextValue(AbilitySharedValue.StatBonus); }),
                                                          Helpers.CreateCalculateSharedValue(Helpers.CreateContextDiceValue(Kingmaker.RuleSystem.DiceType.One, Helpers.CreateContextValue(AbilityRankType.Default), Helpers.CreateContextValue(AbilityRankType.StatBonus)), AbilitySharedValue.StatBonus),
                                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, progression: ContextRankProgression.DelayedStartPlusDivStep,
                                                                                          customProperty: WeaponTrainingMechanics.WeaponTrainingPropertyGetter.Blueprint.Value,
                                                                                          startLevel: 1, stepLevel: 3, max: 2),
                                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, progression: ContextRankProgression.Div2,
                                                                                          customProperty: NewMechanics.EnchantmentMechanics.WeaponEnchantmentPropertyGetter.Blueprint.Value,
                                                                                          type: AbilityRankType.StatBonus)
                                                          );
            defensive_weapon_training_effect.HideInCharacterSheetAndLevelUp = true;
            defensive_weapon_training = Helpers.CreateFeature("DefensiveWeaponTrainingAdvancedWeaponTrainingFeature",
                                                          "Defensive Weapon Training",
                                                          "The fighter gains a +1 shield bonus to his Armor Class. The fighter adds half his weapon’s enhancement bonus (if any) to this shield bonus. When his weapon training bonus for weapons from the associated fighter weapon group reaches +4, this shield bonus increases to +2. This shield bonus is lost if the fighter is immobilized or helpless.",
                                                          "",
                                                          Helpers.GetIcon("4c44724ffa8844f4d9bedb5bb27d144a"), //combat expertise
                                                          FeatureGroup.None,
                                                          Helpers.Create<WeaponTrainingMechanics.AddFeatureOnWeaponTraining>(a => a.feature = defensive_weapon_training_effect)
                                                          );
            defensive_weapon_training.ReapplyOnLevelUp = true;
            addToAdvancedWeaponTraining(defensive_weapon_training);
        }


        static void createFightersFinesse()
        {
            var weapon_finesse = library.Get<BlueprintFeature>("90e54424d682d104ab36436bd527af09");
            fighters_finesse = Helpers.CreateFeature("FightersFinesseAdvancedWeaponTrainingFeature",
                                                          "Fighter’s Finesse",
                                                          "The fighter gains the benefits of the Weapon Finesse feat with all melee weapons that belong to the associated fighter weapon group (even if they cannot normally be used with Weapon Finesse). The fighter must have the Weapon Finesse feat before choosing this option.",
                                                          "",
                                                          weapon_finesse.Icon,
                                                          FeatureGroup.None,
                                                          Helpers.Create<WeaponTrainingMechanics.AttackStatReplacementIfWeaponTraining>(a => a.repalcement_stat = StatType.Dexterity),
                                                          Helpers.PrerequisiteFeature(weapon_finesse)
                                                          );
            addToAdvancedWeaponTraining(fighters_finesse);
        }


        static void createTrainedThrow()
        {
            trained_throw = Helpers.CreateFeature("TrainedThrowAdvancedWeaponTrainingFeature",
                                                    "Trained Throw",
                                                    "When the fighter makes a ranged attack with a thrown weapon and applies his Dexterity modifier on attack rolls and his Strength modifier on damage rolls, he doubles his weapon training bonus on damage rolls.",
                                                    "",
                                                    Helpers.GetIcon("65c538dcfd91930489ad3ab18ad9204b"), //throw anything
                                                    FeatureGroup.None,
                                                    Helpers.Create<WeaponTrainingMechanics.WeaponCategoryGrace>(w => w.group = WeaponFighterGroup.Thrown)
                                                    );
            addToAdvancedWeaponTraining(trained_throw);
        }


        static void createTrainedGrace()
        {
            var weapon_finesse = library.Get<BlueprintFeature>("90e54424d682d104ab36436bd527af09");
            trained_grace = Helpers.CreateFeature("TrainedGraceeAdvancedWeaponTrainingFeature",
                                                    "Trained Grace",
                                                    "When the fighter uses Weapon Finesse to make a melee attack with a weapon, using his Dexterity modifier on attack rolls and his Strength modifier on damage rolls, he doubles his weapon training bonus on damage rolls. The fighter must have Weapon Finesse in order to choose this option.",
                                                    "",
                                                    weapon_finesse.Icon,
                                                    FeatureGroup.None,
                                                    Helpers.Create<WeaponTrainingMechanics.TrainedGrace>(),
                                                    Helpers.PrerequisiteFeature(weapon_finesse)
                                                    );
            addToAdvancedWeaponTraining(trained_grace);
        }


        static void createFocusWeapon()
        {
            DiceFormula[] diceFormulas = new DiceFormula[] {new DiceFormula(1, DiceType.D6),
                                                            new DiceFormula(1, DiceType.D8),
                                                            new DiceFormula(1, DiceType.D10),
                                                            new DiceFormula(2, DiceType.D6),
                                                            new DiceFormula(2, DiceType.D8)};

            var weapon_focus = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");

            focus_weapon = Helpers.CreateFeatureSelection("FocusedWeaponAdvancedWeaponTrainingFeatureSelection",
                                                          "Focused Weapon",
                                                          "The fighter selects one weapon for which he has Weapon Focus and that belongs to the associated fighter weapon group. The fighter can deal damage with this weapon based on the damage of the warpriest’s sacred weapon class feature, treating his fighter level as his warpriest level. The fighter must have Weapon Focus with the selected weapon in order to choose this option.",
                                                          "",
                                                          weapon_focus.Icon,
                                                          FeatureGroup.None
                                                          );
            
            foreach (var wt in category_training_map)
            {
                var feature = Helpers.CreateFeature(wt.Key.ToString() + "FocusedWeaponAdvancedWeaponTrainingFeatureSelection",
                                                    "Focused Weapon: " + LocalizedTexts.Instance.Stats.GetText(wt.Key),
                                                    focus_weapon.Description,
                                                    "",
                                                    focus_weapon.Icon,
                                                    FeatureGroup.None,
                                                    Common.createPrerequisiteParametrizedFeatureWeapon(weapon_focus, wt.Key),
                                                    Helpers.Create<NewMechanics.ContextWeaponDamageDiceReplacementForSpecificCategory>(c => 
                                                                                                                                            { c.category = wt.Key;
                                                                                                                                              c.dice_formulas = diceFormulas;
                                                                                                                                              c.value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                                                                                            }),
                                                    Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                    type: AbilityRankType.Default,
                                                                                    progression: ContextRankProgression.DivStep,
                                                                                    stepLevel: 5,
                                                                                    classes: new BlueprintCharacterClass[] { fighter })
                                                    );

                if (two_handed_categories.Contains(wt.Key))
                {
                    feature.AddComponents(Helpers.PrerequisiteFeature(wt.Value, any: true), Helpers.PrerequisiteFeature(two_handed_weapon_training, any: true));
                }
                else
                {
                    feature.AddComponents(Helpers.PrerequisiteFeature(wt.Value));
                }

                focus_weapon.AllFeatures = focus_weapon.AllFeatures.AddToArray(feature);                
            }
            addToAdvancedWeaponTraining(focus_weapon);
        }


        static void createMoreWeaponGroups()
        {
            two_handed_weapon_training.AddComponent(Helpers.Create<WeaponTraining>());
            var axes = library.Get<BlueprintFeature>("1b18d6a1297950f4bba9d121cfc735e9");

            monk_weapon_group = library.CopyAndAdd<BlueprintFeature>("1b18d6a1297950f4bba9d121cfc735e9", "WeaponTrainingMonk", "");
            monk_weapon_group.ReplaceComponent<WeaponGroupDamageBonus>(w => w.WeaponGroup = Kingmaker.Blueprints.Items.Weapons.WeaponFighterGroup.Monk);
            monk_weapon_group.ReplaceComponent<WeaponGroupAttackBonus>(w => w.WeaponGroup = Kingmaker.Blueprints.Items.Weapons.WeaponFighterGroup.Monk);
            monk_weapon_group.SetName("Weapon Training (Monk)");
            monk_weapon_group.SetDescription("Whenever a fighter attacks with a weapon from this group, he gains a +1 bonus on attack and damage rolls.\nThis group includes kama, nunchaku and sai.");

            thrown_weapon_group = library.CopyAndAdd<BlueprintFeature>("1b18d6a1297950f4bba9d121cfc735e9", "WeaponTrainingThrown", "");
            thrown_weapon_group.ReplaceComponent<WeaponGroupDamageBonus>(w => w.WeaponGroup = Kingmaker.Blueprints.Items.Weapons.WeaponFighterGroup.Thrown);
            thrown_weapon_group.ReplaceComponent<WeaponGroupAttackBonus>(w => w.WeaponGroup = Kingmaker.Blueprints.Items.Weapons.WeaponFighterGroup.Thrown);
            thrown_weapon_group.SetName("Weapon Training (Thrown)");
            thrown_weapon_group.SetDescription("Whenever a fighter attacks with a weapon from this group, he gains a +1 bonus on attack and damage rolls.\nThis group includes dart, javelin, shuriken, sling, sling staff and throwing axe.");

            /*flails_weapon_group = library.CopyAndAdd<BlueprintFeature>("1b18d6a1297950f4bba9d121cfc735e9", "WeaponTrainingFlails", "");
            flails_weapon_group.ReplaceComponent<WeaponGroupDamageBonus>(w => w.WeaponGroup = Kingmaker.Blueprints.Items.Weapons.WeaponFighterGroup.Flails);
            flails_weapon_group.ReplaceComponent<WeaponGroupAttackBonus>(w => w.WeaponGroup = Kingmaker.Blueprints.Items.Weapons.WeaponFighterGroup.Flails);
            flails_weapon_group.SetName("Weapon Training (Flails)");*/

            weapon_training_rankup.AllFeatures = weapon_training_rankup.AllFeatures.AddToArray(monk_weapon_group, thrown_weapon_group);
            advanced_weapon_training.AllFeatures = advanced_weapon_training.AllFeatures.AddToArray(monk_weapon_group, thrown_weapon_group);
        }

        static void createAdvancedArmorTraining()
        {
            var armor_training = library.Get<BlueprintFeature>("3c380607706f209499d951b29d3c44f3");
            advanced_armor_training = Helpers.CreateFeatureSelection("FighterAdvancedArmorTraining",
                                                                      "Advanced Armor Training",
                                                                      "Beginning at 7th level, instead of increasing the benefits provided by armor training (reducing his armor’s check penalty by 1 and increasing its maximum Dexterity bonus by 1), a fighter can choose an advanced armor training option. If the fighter does so, he still gains the ability to move at his normal speed while wearing medium armor at 3rd level, and while wearing heavy armor at 7th level.",
                                                                      "",
                                                                      armor_training.Icon,
                                                                      FeatureGroup.None,
                                                                      Helpers.CreateAddMechanics(Kingmaker.UnitLogic.FactLogic.AddMechanicsFeature.MechanicsFeatureType.ImmunToArmorSpeedPenalty)
                                                                      );
            advanced_armor_training.AllFeatures = new BlueprintFeature[] { armor_training };

            var fighter_progression = library.Get<BlueprintProgression>("b50e94b57be32f74892f381ae2a8905a");
            var level_entries = new LevelEntry[][]{ fighter_progression.LevelEntries,//fighter progression
                                                    library.Get<BlueprintArchetype>("84643e02a764bff4a9c1aba333a53c89").RemoveFeatures, //2h fighter
                                                    library.Get<BlueprintArchetype>("d80a67a264f206e4b8d2fcf7e560d48f").RemoveFeatures, //aldori sword lord
                                                   };

            foreach (var les in level_entries)
            {
                foreach (var le in les)
                {
                    if (le.Level > 3 && le.Features.Contains(armor_training))
                    {
                        le.Features.Remove(armor_training);
                        le.Features.Add(advanced_armor_training);
                    }
                }
            }
            fighter_progression.UIGroups[0].Features.Add(advanced_armor_training);
        }


        static void addToAdvancedArmorTraining(BlueprintFeature feature)
        {
            advanced_armor_training.AllFeatures = advanced_armor_training.AllFeatures.AddToArray(feature);
        }

        static void addToAdvancedWeaponTraining(BlueprintFeature feature)
        {
            feature.AddComponent(Helpers.PrerequisiteClassLevel(fighter, 9));
            advanced_weapon_training.AllFeatures = advanced_weapon_training.AllFeatures.AddToArray(feature);
        }


        static void createVerstileTrainingAndAdaptableTraining()
        {
            //armor skills: mobility, athletics, persuation

            var axes = library.Get<BlueprintFeature>("1b18d6a1297950f4bba9d121cfc735e9");
            var bows = library.Get<BlueprintFeature>("e0401ecade57d4144978dbd714c4069f");
            var close = library.Get<BlueprintFeature>("bd75a95b36a3cd8459513ee1932c8c22");
            var crossbows = library.Get<BlueprintFeature>("9cdfc2a236ee6d349ad6d8a2170477d5");
            var double_weapon = library.Get<BlueprintFeature>("a7a7ad500d4e2a847b450b85cbe68d65");
            var hammers = library.Get<BlueprintFeature>("8bb8579622b823c4285d851274a009c3");
            var heavy_blades = library.Get<BlueprintFeature>("2a0ce0186af38ed419f47fce16f93c2a");
            var light_blades = library.Get<BlueprintFeature>("4923409590bdb604590e04da4253ab78");
            var natural = library.Get<BlueprintFeature>("3ab76d4a8aa9e4c459add32139080206");
            var polearms = library.Get<BlueprintFeature>("c062c6d16aecddc4ab67d9c783b2ad46");
            var spears = library.Get<BlueprintFeature>("d5c04077fc063e44784384a00377b7cf");

            Dictionary<StatType, BlueprintFeature[]> skill_weapon_group_map = new Dictionary<StatType, BlueprintFeature[]>();
            Dictionary<StatType, BlueprintFeature> skill_armor_feature_map = new Dictionary<StatType, BlueprintFeature>();

            var armor_skills = new StatType[] { StatType.SkillMobility, StatType.SkillAthletics, StatType.SkillPersuasion };

            adaptable_training = Helpers.CreateFeatureSelection("AdaptableArmorTraining",
                                                                "Adaptable Training",
                                                                "The fighter can use his base attack bonus in place of his ranks in one skill of his choice.\n"
                                                                + "The fighter need not be wearing armor or using a shield to use this option. When using adaptable training, the fighter substitutes his total base attack bonus (including his base attack bonus gained through levels in other classes) for his ranks in this skill, but adds the skill’s usual ability score modifier and any other bonuses or penalties that would modify that skill. Once a skill has been selected, it cannot be changed and the fighter can immediately retrain all of his ranks in the selected skill at no additional cost in money or time. In addition, the fighter adds all skills chosen with this option to his list of class skills.",
                                                                "",
                                                                null,
                                                                FeatureGroup.None);
            adaptable_training.AllFeatures = new BlueprintFeature[0];

            versatile_training = Helpers.CreateFeatureSelection("VersatileWeaponTraining",
                                                    "Versatile Training",
                                                    "The fighter can use his base attack bonus in place of his ranks in skill of his choice that is associated with the fighter weapon group he has chosen with this option. The fighter need not be wielding an associated weapon to use this option. When using versatile training, the fighter substitutes his total base attack bonus (including his base attack bonus gained through levels in other classes) for his ranks in this skill, but adds the skill’s usual ability score modifier and any other bonuses or penalties that would modify those skills. Once the skill has been selected, it cannot be changed and the fighter can immediately retrain all of his skill ranks in the selected skill at no additional cost in money or time. In addition, the fighter adds all skills chosen with this option to his list of class skills.",
                                                    "",
                                                    null,
                                                    FeatureGroup.None
                                                    );
            versatile_training.AllFeatures = new BlueprintFeature[0];


            skill_weapon_group_map.Add(StatType.SkillAthletics, new BlueprintFeature[] { axes, natural, spears, hammers });
            skill_weapon_group_map.Add(StatType.SkillLoreNature, new BlueprintFeature[] { axes, spears});
            skill_weapon_group_map.Add(StatType.SkillPerception, new BlueprintFeature[] { bows, close, crossbows, double_weapon, polearms, thrown_weapon_group });
            skill_weapon_group_map.Add(StatType.SkillStealth, new BlueprintFeature[] { close, light_blades, crossbows });
            skill_weapon_group_map.Add(StatType.SkillMobility, new BlueprintFeature[] { double_weapon, monk_weapon_group, thrown_weapon_group });
            skill_weapon_group_map.Add(StatType.SkillPersuasion, new BlueprintFeature[] { hammers, heavy_blades, light_blades, polearms });



            var skill_foci = library.Get<BlueprintFeatureSelection>("c9629ef9eebb88b479b2fbc5e836656a").AllFeatures;
            foreach (var s in armor_skills)
            {
                var feature = Helpers.CreateFeature(s.ToString() + "AdaptableTrainingFeature",
                                                    "Adaptable Training: " + LocalizedTexts.Instance.Stats.GetText(s),
                                                    adaptable_training.Description,
                                                    "",
                                                    skill_foci.FirstOrDefault(sf => sf.GetComponent<AddContextStatBonus>().Stat == s).Icon,
                                                    FeatureGroup.None,
                                                    Helpers.Create<SkillMechanics.SetSkillRankToValue>(ss => { ss.skill = s; ss.value = Helpers.CreateContextValue(AbilityRankType.Default); ss.increase_by1_on_apply = true; }),
                                                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.BaseAttack)
                                                    );
                skill_armor_feature_map.Add(s, feature);
                adaptable_training.AllFeatures = adaptable_training.AllFeatures.AddToArray(feature);
            }


            foreach (var s in skill_weapon_group_map)
            {
                var feature = Helpers.CreateFeature(s.ToString() + "VersatileTrainingFeature",
                                                    "Verstile Training: " + LocalizedTexts.Instance.Stats.GetText(s.Key),
                                                    versatile_training.Description,
                                                    "",
                                                    skill_foci.FirstOrDefault(sf => sf.GetComponent<AddContextStatBonus>().Stat == s.Key).Icon,
                                                    FeatureGroup.None,
                                                    Helpers.Create<SkillMechanics.SetSkillRankToValue>(ss => { ss.skill = s.Key; ss.value = Helpers.CreateContextValue(AbilityRankType.Default); ss.increase_by1_on_apply = true; }),
                                                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.BaseAttack)
                                                    );
                versatile_training.AllFeatures = versatile_training.AllFeatures.AddToArray(feature);

                foreach (var f in s.Value)
                {
                    feature.AddComponent(Helpers.PrerequisiteFeature(f, any: true));
                }

                if (skill_armor_feature_map.ContainsKey(s.Key))
                {
                    skill_armor_feature_map[s.Key].AddComponent(Helpers.PrerequisiteNoFeature(feature));
                    feature.AddComponent(Helpers.PrerequisiteNoFeature(skill_armor_feature_map[s.Key]));
                }
            }

            addToAdvancedArmorTraining(adaptable_training);
            addToAdvancedWeaponTraining(versatile_training);
        }



        static public string getGroupName(WeaponFighterGroup group)
        {
            switch (group)
            {
                case WeaponFighterGroup.Axes:
                    return "Axes";
                case WeaponFighterGroup.BladesHeavy:
                    return "Blades, Heavy";                    
                case WeaponFighterGroup.BladesLight:
                    return "Blades, Light";                   
                case WeaponFighterGroup.Bows:
                    return "Bows";                    
                case WeaponFighterGroup.Close:
                    return "Close";                    
                case WeaponFighterGroup.Crossbows:
                    return "Crossbows";                   
                case WeaponFighterGroup.Double:
                    return "Double";                 
                case WeaponFighterGroup.Flails:
                    return "Flails";                    
                case WeaponFighterGroup.Hammers:
                    return "Hammers";                    
                case WeaponFighterGroup.Monk:
                    return "Monk";                    
                case WeaponFighterGroup.Natural:
                    return "Natural";                   
                case WeaponFighterGroup.None:
                    return "Unknown";                   
                case WeaponFighterGroup.Polearms:
                    return "Polearms";                    
                case WeaponFighterGroup.Spears:
                    return "Spears";                    
                case WeaponFighterGroup.Thrown:
                    return "Thrown";                   
            }

            return "None";
        }
    }
}
