using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild.Archetypes
{
    public class ZenArcher
    {
        static public BlueprintArchetype archetype;
        static public BlueprintFeature zen_archer_proficiencies;
        static public BlueprintFeature flurry1, flurry11;
        static public BlueprintFeatureSelection bonus_feat1, bonus_feat6, bonus_feat10;
        static public BlueprintFeature perfect_strike;
        static public BlueprintFeature zen_archery;
        static public BlueprintFeatureSelection way_of_the_bow;
        static public BlueprintFeature point_blank_master;
        static public BlueprintFeature ki_arrows;
        static public BlueprintFeature snap_shot;
        static public BlueprintFeature ki_focus_bow;

        static LibraryScriptableObject library => Main.library;

        static BlueprintCharacterClass[] getMonkArray()
        {
            var monk_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");
            return new BlueprintCharacterClass[] { monk_class };
        }

        static public void create()
        {
            var monk_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "ZenArcherArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Zen Archer");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Some monks seek to become one with another weapon entirely—the bow. The zen archer takes a weapon most other monks eschew and seeks perfection in the pull of a taut bowstring, the flex of a bow’s limbs, and the flight of an arrow fired true.");
            });
            Helpers.SetField(archetype, "m_ParentClass", monk_class);
            library.AddAsset(archetype, "");

            var monk_proficiencies = library.Get<BlueprintFeature>("c7d6f5244c617734a8a76b6785a752b4");
            var monk_flurry1 = library.Get<BlueprintFeature>("fd99770e6bd240a4aab70f7af103e56a");
            var monk_flurry11 = library.Get<BlueprintFeature>("a34b8a9fcc9024b42bacfd5e6b614bfa");
            var stunning_fist = library.Get<BlueprintFeature>("a29a582c3daa4c24bb0e991c596ccb28");
            var evasion = library.Get<BlueprintFeature>("576933720c440aa4d8d42b0c54b77e80");
            var monk_feat1 = library.Get<BlueprintFeatureSelection>("ac3b7f5c11bce4e44aeb332f66b75bab");

            var still_mind = library.Get<BlueprintFeature>("b8933d223d87087418a627e61ea42ae6");
            var stunning_fist_fatigue = library.Get<BlueprintFeature>("819645da2e446f84d9b168ed1676ec29");
            var purity_of_body = library.Get<BlueprintFeature>("9b02f77c96d6bba4daf9043eff876c76");
            var style_strike = library.Get<BlueprintFeatureSelection>("7bc6a93f6e48eff49be5b0cde83c9450");
            var monk_feat6 = library.Get<BlueprintFeatureSelection>("b993f42cb119b4f40ac423ae76394374");
            var stunning_fist_sickened = library.Get<BlueprintFeature>("d256ab3837538cc489d4b571e3a813eb");

            var improved_evasion = library.Get<BlueprintFeature>("ce96af454a6137d47b9c6a1e02e66803");
            var monk_feat10 = library.Get<BlueprintFeatureSelection>("1051170c612d5b844bfaa817d6f4cfff");

            createZenArcherProficiencies();
            createPerfectStrike();
            createFlurry();
            createFeats();
            createWayOfTheBow();
            createZenArchery();
            createPointBlankMaster();
            createKiArrows();
            createSnapshot();
            createKiFocusBow();

            

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, monk_proficiencies, monk_flurry1, monk_feat1, stunning_fist),
                                                          Helpers.LevelEntry(2, evasion, monk_feat1),
                                                          Helpers.LevelEntry(4, still_mind, stunning_fist_fatigue),
                                                          Helpers.LevelEntry(5, purity_of_body, style_strike),
                                                          Helpers.LevelEntry(6, monk_feat6),
                                                          Helpers.LevelEntry(8, stunning_fist_sickened),
                                                          Helpers.LevelEntry(9, improved_evasion, style_strike),
                                                          Helpers.LevelEntry(10, monk_feat10),
                                                          Helpers.LevelEntry(11, monk_flurry11),
                                                          Helpers.LevelEntry(12, MonkStunningFists.stunning_fist_staggered),
                                                          Helpers.LevelEntry(13, style_strike),
                                                          Helpers.LevelEntry(14, monk_feat10),
                                                          Helpers.LevelEntry(16, MonkStunningFists.stunning_fist_blind),
                                                          Helpers.LevelEntry(17, style_strike),
                                                          Helpers.LevelEntry(18, monk_feat10),
                                                          Helpers.LevelEntry(20, MonkStunningFists.stunning_fist_paralyzed),
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, zen_archer_proficiencies, flurry1, perfect_strike, bonus_feat1),
                                                          Helpers.LevelEntry(2, bonus_feat1, way_of_the_bow),
                                                          Helpers.LevelEntry(3, zen_archery),
                                                          Helpers.LevelEntry(4, point_blank_master),
                                                          Helpers.LevelEntry(5, ki_arrows),
                                                          Helpers.LevelEntry(6, bonus_feat6),
                                                          Helpers.LevelEntry(9, snap_shot),
                                                          Helpers.LevelEntry(10, bonus_feat10, NewFeats.perfect_strike_extra_reroll), 
                                                          Helpers.LevelEntry(11, flurry11),
                                                          Helpers.LevelEntry(13, ki_focus_bow),
                                                          Helpers.LevelEntry(14, bonus_feat10),
                                                          Helpers.LevelEntry(18, bonus_feat10),
                                                       };

            monk_class.Progression.UIDeterminatorsGroup = monk_class.Progression.UIDeterminatorsGroup.AddToArray(zen_archer_proficiencies);
            monk_class.Progression.UIGroups[2].Features.Add(bonus_feat1);
            monk_class.Progression.UIGroups[2].Features.Add(bonus_feat6);
            monk_class.Progression.UIGroups[2].Features.Add(bonus_feat10);
            monk_class.Progression.UIGroups = monk_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(perfect_strike, way_of_the_bow, point_blank_master, snap_shot));
            monk_class.Progression.UIGroups = monk_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(flurry1, zen_archery, ki_arrows, flurry11, ki_focus_bow));
            monk_class.Archetypes = monk_class.Archetypes.AddToArray(archetype);
        }


        static void createKiFocusBow()
        {
            ki_focus_bow = Helpers.CreateFeature("KiFocusBowZenArcherFeature",
                                                "Ki Focus Bow",
                                                "At 13th level, a zen archer may treat arrows fired from his bow as if they were ki focus weapons, allowing him to use his special ki attacks as if his arrows were unarmed attacks.",
                                                "",
                                                null,
                                                FeatureGroup.None,
                                                Common.createAddParametrizedFeatures(FeralCombatTraining.ki_focus_weapon, WeaponCategory.Longbow),
                                                Common.createAddParametrizedFeatures(FeralCombatTraining.ki_focus_weapon, WeaponCategory.Shortbow)
                                                );
        }


        static void createSnapshot()
        {
            snap_shot = Helpers.CreateFeature("ZenArcherSnapshotFeature",
                                            "Snap Shot",
                                            "At 9th level, a zen archer gains Snap Shot as a bonus feat, even if he does not meet the prerequisites.",
                                            "",
                                            NewFeats.snap_shot.Icon,
                                            FeatureGroup.None,
                                            Helpers.CreateAddFact(NewFeats.snap_shot)
                                            );
        }


        static void createKiArrows()
        {
            var resource = library.Get<BlueprintAbilityResource>("9d9c90a9a1f52d04799294bf91c80a82"); //ki resource
            //var unarmed1d6 = library.Get<BlueprintFeature>("c3fbeb2ffebaaa64aa38ce7a0bb18fb0");
            var unarmed1d8 = library.Get<BlueprintFeature>("8267a0695a4df3f4ca508499e6164b98");
            var unarmed1d10 = library.Get<BlueprintFeature>("f790a36b5d6f85a45a41244f50b947ca");
            var unarmed2d6 = library.Get<BlueprintFeature>("b3889f445dbe42948b8bb1ba02e6d949");
            var unarmed2d8 = library.Get<BlueprintFeature>("078636a2ce835e44394bb49a930da230");
            var unarmed2d10 = library.Get<BlueprintFeature>("df38e56fa8b3f0f469d55f9aa26b3f5c");

            DiceFormula[] dice_formulas = new DiceFormula[] {new DiceFormula(1, DiceType.D6),
                                                            new DiceFormula(1, DiceType.D8),
                                                            new DiceFormula(1, DiceType.D10),
                                                            new DiceFormula(2, DiceType.D6),
                                                            new DiceFormula(2, DiceType.D8),
                                                            new DiceFormula(2, DiceType.D10)};


            var buff = Helpers.CreateBuff("ZenArcherKiArrowsBuff",
                                                       "Ki Arrows",
                                                       "At 5th level, a zen archer may spend 1 point from his ki pool as a swift action to change the damage dice of arrows he shoots to that of his unarmed strikes. This lasts until the start of his next turn. For example, a Medium zen archer’s shortbow normally deals 1d6 damage; using this ability, his arrows deal 1d8 damage until the start of his next turn.",
                                                        "",
                                                       NewSpells.flame_arrow.Icon,
                                                       null,
                                                       Helpers.Create<NewMechanics.ContextWeaponDamageDiceReplacementWeaponCategory>(c =>
                                                                                                                                       {
                                                                                                                                           c.value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                                                                                           c.dice_formulas = dice_formulas;
                                                                                                                                           c.categories = new WeaponCategory[] { WeaponCategory.Longbow, WeaponCategory.Shortbow };
                                                                                                                                       }
                                                                                                                                     ),
                                                       Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                       classes: getMonkArray(),
                                                                                       type: AbilityRankType.Default,
                                                                                       progression: ContextRankProgression.DivStep,
                                                                                       stepLevel: 4
                                                                                       )
                                                      );

            var ability = Helpers.CreateAbility("ZenArcherKiArrowsAbility",
                                               buff.Name,
                                               buff.Description,
                                               "",
                                               buff.Icon,
                                               AbilityType.Supernatural,
                                               CommandType.Swift,
                                               AbilityRange.Personal,
                                               Helpers.oneRoundDuration,
                                               "",
                                               Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1), dispellable: false)),
                                               resource.CreateResourceLogic()
                                               );

            ki_arrows = Common.AbilityToFeature(ability, false);
        }


        static void createPointBlankMaster()
        {
            var feat = library.Get<BlueprintParametrizedFeature>("05a3b543b0a0a0346a5061e90f293f0b");
            point_blank_master = Helpers.CreateFeature("ZenArcherPointBlankMasterFeature",
                                                        "Point Blank Master",
                                                        "At 4th level, a zen archer gains Point Blank Master as a bonus feat, even if he does not meet the prerequisites.",
                                                        "",
                                                        feat.Icon,
                                                        FeatureGroup.None,
                                                        Common.createAddParametrizedFeatures(feat, WeaponCategory.Longbow),
                                                        Common.createAddParametrizedFeatures(feat, WeaponCategory.Shortbow)
                                                        );
        }


        static void createZenArchery()
        {
            zen_archery = Helpers.CreateFeature("ZenArcheryFeature",
                                                "Zen Archery",
                                                "At 3rd level, a zen archer may use his Wisdom modifier instead of his Dexterity modifier on ranged attack rolls when using a bow.",
                                                "",
                                                null,
                                                FeatureGroup.None,
                                                Helpers.Create<NewMechanics.AttackStatReplacementForWeaponCategory>(c =>
                                                                                                                    {
                                                                                                                        c.categories = new WeaponCategory[] {WeaponCategory.Longbow, WeaponCategory.Shortbow};
                                                                                                                        c.ReplacementStat = StatType.Wisdom;
                                                                                                                    }
                                                )
                                                );
        }


        static void createWayOfTheBow()
        {
            var categories = new WeaponCategory[] { WeaponCategory.Longbow, WeaponCategory.Shortbow };

            way_of_the_bow = Helpers.CreateFeatureSelection("WayOfTheBowFeature",
                                                "Way of the Bow",
                                                "At 2nd level, a zen archer gains Weapon Focus as a bonus feat with one type of bow.\n"
                                                + "At 6th level, the monk gains Weapon Specialization with the same weapon as a bonus feat, even if he does not meet the prerequisites.",
                                                "",
                                                null,
                                                FeatureGroup.None);
            var weapon_focus = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");
            var weapon_specialization = library.Get<BlueprintParametrizedFeature>("31470b17e8446ae4ea0dacd6c5817d86");
            foreach (var c in categories)
            {
                var feature6 = Helpers.CreateFeature(c.ToString() + "WayOfTheBow6Feature",
                                     way_of_the_bow.name + $" (Weapon Specialization: {LocalizedTexts.Instance.Stats.GetText(c)})",
                                     way_of_the_bow.Description,
                                     "",
                                     Helpers.GetIcon("1e1f627d26ad36f43bbd26cc2bf8ac7e"),
                                     FeatureGroup.None,
                                     Common.createAddParametrizedFeatures(weapon_specialization, c)
                                     );
                feature6.HideInCharacterSheetAndLevelUp = true;

                var feature = Helpers.CreateFeature(c.ToString() + "WayOfTheBowFeature",
                                                    way_of_the_bow.Name + $": {LocalizedTexts.Instance.Stats.GetText(c)}",
                                                    way_of_the_bow.Description,
                                                    "",
                                                    null,
                                                    FeatureGroup.None,
                                                    Common.createAddParametrizedFeatures(weapon_focus, c),
                                                    Helpers.CreateAddFeatureOnClassLevel(feature6, 6, getMonkArray())
                                                    );

                way_of_the_bow.AllFeatures = way_of_the_bow.AllFeatures.AddToArray(feature);
            }
        }


        static void createFeats()
        {
            bonus_feat1 = Helpers.CreateFeatureSelection("BonusFeats1ZenArcherFeature",
                                                "Bonus Feat",
                                                "A zen archer’s bonus feats must be taken from the following list: Combat Reflexes, Deflect Arrows, Dodge, Point-Blank Shot, Precise Shot, and Rapid Shot.\n"
                                                + "At 6th level, the following feats are added to the list: Improved Precise Shot, Manyshot and Mobility.\n"
                                                + "At 10th level, the following feats are added to the list: Improved Critical.\n"
                                                + "A monk need not have any of the prerequisites normally required for these feats to select them.",
                                                "",
                                                null,
                                                FeatureGroup.None);
            bonus_feat1.IgnorePrerequisites = true;
            bonus_feat1.AllFeatures = new BlueprintFeature[]
            {
                library.Get<BlueprintFeature>("0f8939ae6f220984e8fb568abbdfba95"), //combat reflexes
                library.Get<BlueprintFeature>("2c61fdbf242866f4e93c3e1477fb96b5"), //deflect arrows
                library.Get<BlueprintFeature>("97e216dbb46ae3c4faef90cf6bbe6fd5"), //dodge
                library.Get<BlueprintFeature>("0da0c194d6e1d43419eb8d990b28e0ab"), //point blank shot
                library.Get<BlueprintFeature>("8f3d1e6b4be006f4d896081f2f889665"), //precise shot
                library.Get<BlueprintFeature>("9c928dc570bb9e54a9649b3ebfe47a41"), //rapid shot
            };

            bonus_feat6 = library.CopyAndAdd(bonus_feat1, "BonusFeat6ZenArcherFeature", "");
            bonus_feat6.AllFeatures = bonus_feat6.AllFeatures.AddToArray(library.Get<BlueprintFeature>("46f970a6b9b5d2346b10892673fe6e74"), //improved precis shot
                                                                         library.Get<BlueprintFeature>("adf54af2a681792489826f7fd1b62889"), //many shot
                                                                         library.Get<BlueprintFeature>("2a6091b97ad940943b46262600eaeaeb")// mobility
                                                                         );
            bonus_feat10 = library.CopyAndAdd(bonus_feat6, "BonusFeat10ZenArcherFeature", "");
            bonus_feat10.AllFeatures = bonus_feat10.AllFeatures.AddToArray(library.Get<BlueprintFeature>("f4201c85a991369408740c6888362e20") //mproved critical
                                                                          );
        }


        static void createFlurry()
        {
            flurry1 = Helpers.CreateFeature("ZenArcherFlurry1Feature",
                                     "Flurry of Blows",
                                     "Starting at 1st level, a zen archer can make a flurry of blows as a full-attack action, but only when using a bow (even though it is a ranged weapon). He may not make a flurry of blows with his unarmed attacks or any other weapons. A zen archer does not apply his Strength bonus on damage rolls made with flurry of blows unless he is using a composite bow with a Strength rating.\n"
                                     + "A zen archer’s flurry of blows otherwise functions as normal for a monk of his level. (One additional attack at highest base attack bonus. Stacks with bonus attacks from haste and other similar effects. At 11th level, one additional attack at highest base attack bonus).\n"
                                     + "A zen archer cannot use Rapid Shot or Manyshot when making a flurry of blows with his bow.",
                                     "",
                                     zen_archer_proficiencies.Icon,
                                     FeatureGroup.None);
            flurry11 = Helpers.CreateFeature("ZenArcherFlurry11Feature",
                                     flurry1.Name,
                                     flurry1.Description,
                                     "",
                                     zen_archer_proficiencies.Icon,
                                     FeatureGroup.None);

            var flurry_effect = Helpers.CreateFeature("ZenArcherFlurryEffectFeature",
                                             flurry1.Name,
                                             flurry1.Description,
                                             "",
                                             zen_archer_proficiencies.Icon,
                                             FeatureGroup.None,
                                             Helpers.Create<NewMechanics.BuffExtraAttackCategorySpecific>(b =>
                                                                                                            {
                                                                                                                b.categories = new WeaponCategory[] { WeaponCategory.Longbow, WeaponCategory.Shortbow };
                                                                                                                b.num_attacks = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                                                            }
                                                                                                          ),
                                             Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureList, featureList: new BlueprintFeature[] { flurry1, flurry11 }),
                                             Helpers.Create<FeatureMechanics.DeactivateManyshot>());
            flurry_effect.HideInCharacterSheetAndLevelUp = true;

            var buff = Helpers.CreateBuff("ZenArcherFlurryBuff",
                                             flurry11.Name,
                                             flurry1.Description,
                                             "",
                                             flurry1.Icon,
                                             null,
                                             Helpers.Create<MonkNoArmorFeatureUnlock>(m => m.NewFact = flurry_effect)
                                             );

            var ability = Helpers.CreateActivatableAbility("ZenArcherFlurryToggleAbility",
                                                           buff.Name,
                                                           buff.Description,
                                                           "",
                                                           buff.Icon,
                                                           buff,
                                                           AbilityActivationType.Immediately,
                                                           CommandType.Free,
                                                           null);
            ability.IsOnByDefault = true;
            ability.Group = ActivatableAbilityGroupExtension.RapidShot.ToActivatableAbilityGroup();

            //fix rapid shot
            library.Get<BlueprintActivatableAbility>("90a77bfe25ec2e14caf8bd5cde9febf2").Group = ability.Group;

            flurry1.AddComponent(Helpers.CreateAddFact(ability));
        }

        static void createPerfectStrike()
        {
            perfect_strike = Helpers.CreateFeature("ZenArcherPerfectStrike",
                                                   "Perfect Strike",
                                                   "At 1st level, a zen archer gains Perfect Strike as a bonus feat, even if he does not meet the prerequisites. A zen archer can use Perfect Strike with any bow. At 10th level, the monk can roll his attack roll three times and take the highest result.",
                                                   "",
                                                   NewFeats.perfect_strike.Icon,
                                                   FeatureGroup.None,
                                                   Helpers.CreateAddFact(NewFeats.perfect_strike),
                                                   Common.createAddParametrizedFeatures(NewFeats.perfect_strike_unlocker, WeaponCategory.Longbow),
                                                   Common.createAddParametrizedFeatures(NewFeats.perfect_strike_unlocker, WeaponCategory.Shortbow)
                                                   );
        }


        static void createZenArcherProficiencies()
        {
            zen_archer_proficiencies = library.CopyAndAdd<BlueprintFeature>("c7d6f5244c617734a8a76b6785a752b4", "ZenArcherProficiencies", "");//monk proficiencies
            zen_archer_proficiencies.SetNameDescriptionIcon("Zen Archer Proficiencies",
                                                            "Zen archers are proficient with longbows, shortbows, composite longbows, and composite shortbows in addition to their normal weapon proficiencies.",
                                                            Helpers.GetIcon("3e9d1119d43d07c4c8ba9ebfd1671952"));
            zen_archer_proficiencies.ReplaceComponent<AddProficiencies>(a => a.WeaponProficiencies = a.WeaponProficiencies.AddToArray(WeaponCategory.Longbow, WeaponCategory.Shortbow));
        }
    }
}
