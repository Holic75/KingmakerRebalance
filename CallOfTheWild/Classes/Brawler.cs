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
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Items.Equipment;
using Newtonsoft.Json;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.PubSubSystem;

namespace CallOfTheWild
{
    class Brawler
    {
        static internal LibraryScriptableObject library => Main.library;
        static internal bool test_mode = false;
        static public BlueprintCharacterClass brawler_class;
        static public BlueprintProgression brawler_progression;

        static public BlueprintFeatureSelection combat_feat;
        static public BlueprintFeature martial_training;
        static public BlueprintFeature brawlers_cunning;
        static public BlueprintFeature unarmed_strike;
        static public BlueprintFeature brawlers_flurry;
        static public BlueprintFeature brawlers_flurry11;
        static public BlueprintFeature ac_bonus;
        static public BlueprintFeature brawler_proficiencies;
        static public BlueprintFeatureSelection[] maneuver_training = new BlueprintFeatureSelection[5];
        static public BlueprintAbilityResource knockout_resource;
        static public BlueprintFeature knockout;
        static public BlueprintFeature brawlers_strike_magic;
        static public BlueprintFeature brawlers_strike_cold_iron_and_silver;
        static public BlueprintFeatureSelection brawlers_strike_alignment;
        static public BlueprintFeature brawlers_strike_adamantine;
        static public BlueprintFeature close_weapon_mastery;
        static public BlueprintFeature awesome_blow;
        static public BlueprintFeature awesome_blow_improved;
        static public BlueprintFeature perfect_warrior;


        internal static void createBrawlerClass()
        {
            Main.logger.Log("Brawler class test mode: " + test_mode.ToString());
            var monk_class = library.Get<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");
            var fighter_class = library.Get<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");

            brawler_class = Helpers.Create<BlueprintCharacterClass>();
            brawler_class.name = "BrawlerClass";
            library.AddAsset(brawler_class, "");

            brawler_class.LocalizedName = Helpers.CreateString("Brawler.Name", "Brawler");
            brawler_class.LocalizedDescription = Helpers.CreateString("Brawler.Description",
                                                                         "Deadly even with nothing in her hands, a brawler eschews using the fighter’s heavy armor and the monk’s mysticism, focusing instead on perfecting many styles of brutal unarmed combat. Versatile, agile, and able to adapt to most enemy attacks, a brawler’s body is a powerful weapon.\n"
                                                                         + "Role: Brawlers are maneuverable and well suited for creating flanking situations or dealing with lightly armored enemies, as well as quickly adapting to a rapidly changing battlefield."
                                                                         );
            brawler_class.m_Icon = monk_class.Icon;
            brawler_class.SkillPoints = monk_class.SkillPoints;
            brawler_class.HitDie = DiceType.D10;
            brawler_class.BaseAttackBonus = monk_class.BaseAttackBonus;
            brawler_class.FortitudeSave = monk_class.FortitudeSave;
            brawler_class.ReflexSave = monk_class.ReflexSave;
            brawler_class.WillSave = monk_class.WillSave;
            brawler_class.Spellbook = null;
            brawler_class.ClassSkills = monk_class.ClassSkills.RemoveFromArray(StatType.SkillLoreReligion).RemoveFromArray(StatType.SkillStealth);
            brawler_class.IsDivineCaster = true;
            brawler_class.IsArcaneCaster = false;
            brawler_class.StartingGold = monk_class.StartingGold;
            brawler_class.PrimaryColor = fighter_class.PrimaryColor;
            brawler_class.SecondaryColor = fighter_class.SecondaryColor;
            brawler_class.RecommendedAttributes = monk_class.RecommendedAttributes;
            brawler_class.NotRecommendedAttributes = monk_class.NotRecommendedAttributes;
            brawler_class.EquipmentEntities = monk_class.EquipmentEntities;
            brawler_class.MaleEquipmentEntities = monk_class.MaleEquipmentEntities;
            brawler_class.FemaleEquipmentEntities = monk_class.FemaleEquipmentEntities;
            brawler_class.ComponentsArray = monk_class.ComponentsArray.ToArray().RemoveFromArray(monk_class.GetComponent<PrerequisiteAlignment>());
            brawler_class.StartingItems = new Kingmaker.Blueprints.Items.BlueprintItem[]
            {
                library.Get<BlueprintItemShield>("f4cef3ba1a15b0f4fa7fd66b602ff32b"), //heavy shield
                library.Get<BlueprintItemWeapon>("ada85dae8d12eda4bbe6747bb8b5883c"), //quarterstaff
                library.Get<BlueprintItemArmor>("afbe88d27a0eb544583e00fa78ffb2c7"), //studded leather
                library.Get<BlueprintItemEquipmentUsable>("d52566ae8cbe8dc4dae977ef51c27d91"), //cure light wounds potion
                library.Get<BlueprintItemEquipmentUsable>("d52566ae8cbe8dc4dae977ef51c27d91"), //cure light wounds potion
            };
            createBrawlerProgression();
            brawler_class.Progression = brawler_progression;

            brawler_class.Archetypes = new BlueprintArchetype[] { };
            Helpers.RegisterClass(brawler_class);

            //fix stunning fist and perfect strike to use brawler levels 
            //fix monk robes
        }


        public static BlueprintCharacterClass[] getBrawlerArray()
        {
            return new BlueprintCharacterClass[] { brawler_class };
        }


        static void createBrawlerProgression()
        {
            var monk_class = library.Get<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");
            var fighter_class = library.Get<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");
            brawler_proficiencies = library.CopyAndAdd<BlueprintFeature>("8c971173613282844888dc20d572cfc9", "BrawlerProficiencies", ""); //cleric proficiencies
            brawler_proficiencies.ReplaceComponent<AddFacts>(a => a.Facts = a.Facts.RemoveFromArray(library.Get<BlueprintFeature>("46f4fb320f35704488ba3d513397789d")));
            brawler_proficiencies.AddComponent(Common.createAddWeaponProficiencies(WeaponCategory.Handaxe, WeaponCategory.PunchingDagger,
                                                                                   WeaponCategory.SpikedHeavyShield, WeaponCategory.SpikedLightShield,
                                                                                   WeaponCategory.WeaponLightShield, WeaponCategory.WeaponHeavyShield)
                                                                                   );
            brawler_proficiencies.SetNameDescriptionIcon("Brawler Proficiencies",
                                                         "A brawler is proficient with all simple weapons plus the handaxe, short sword, and weapons from the close fighter weapon group. She is proficient with light armor and shields (except tower shields).",
                                                         null);

            var fist1d6_monk = library.Get<BlueprintFeature>("c3fbeb2ffebaaa64aa38ce7a0bb18fb0");
            ClassToProgression.addClassToFeat(brawler_class, new BlueprintArchetype[] { }, ClassToProgression.DomainSpellsType.NoSpells, fist1d6_monk, monk_class);
            unarmed_strike = library.CopyAndAdd(fist1d6_monk, "BrawlerUnarmedStrikeFeature", "");
            unarmed_strike.SetDescription("At 1st level, a brawler gains Improved Unarmed Strike as a bonus feat. The damage dealt by a Medium brawler's unarmed strike increases with level: 1d6 at levels 1–3, 1d8 at levels 4–7, 1d10 at levels 8–11, 2d6 at levels 12–15, 2d8 at levels 16–19, and 2d10 at level 20.\nIf the sacred fist is Small, his unarmed strike damage increases as follows: 1d4 at levels 1–3, 1d6 at levels 4–7, 1d8 at levels 8–11, 1d10 at levels 12–15, 2d6 at levels 16–19, and 2d8 at level 20.\nIf the brawler is Large, his unarmed strike damage increases as follows: 1d8 at levels 1–3, 2d6 at levels 4–7, 2d8 at levels 8–11, 3d6 at levels 12–15, 3d8 at levels 16–19, and 4d8 at level 20.");
            var improved_unarmed_strike = library.Get<BlueprintFeature>("7812ad3672a4b9a4fb894ea402095167");

            combat_feat = library.CopyAndAdd<BlueprintFeatureSelection>("41c8486641f7d6d4283ca9dae4147a9f", "BrawlerCombatFeat", "");
            combat_feat.SetDescription("At 1st level, and at every even level thereafter, a brawler gains a bonus feat in addition to those gained from normal advancement (meaning that the brawler gains a feat at every level). These bonus feats must be selected from those listed as Combat Feats.");

            createMartialTraining();
            createBrawlersCunning();
            createBrawlersFlurry();
            createPerfectWarrior();
            createManeuverTraining();
            createACBonus();
            createKnockout();
            createBrawlersStrike();
            createCloseWeaponMastery();
            createAwesomeBlow();

            brawler_progression = Helpers.CreateProgression("BrawlerProgression",
                                                              brawler_class.Name,
                                                              brawler_class.Description,
                                                              "",
                                                              brawler_class.Icon,
                                                              FeatureGroup.None);
            brawler_progression.Classes = getBrawlerArray();




            brawler_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, brawler_proficiencies, brawlers_cunning, unarmed_strike, improved_unarmed_strike,
                                                                                       martial_training, combat_feat,
                                                                                       library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                       library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")), // touch calculate feature                                                                                      
                                                                    Helpers.LevelEntry(2, brawlers_flurry, combat_feat),
                                                                    Helpers.LevelEntry(3, maneuver_training[0]),
                                                                    Helpers.LevelEntry(4, ac_bonus, knockout, combat_feat),
                                                                    Helpers.LevelEntry(5, brawlers_strike_magic, close_weapon_mastery),
                                                                    Helpers.LevelEntry(6, combat_feat),
                                                                    Helpers.LevelEntry(7, maneuver_training[1]),
                                                                    Helpers.LevelEntry(8, combat_feat),
                                                                    Helpers.LevelEntry(9, brawlers_strike_cold_iron_and_silver),
                                                                    Helpers.LevelEntry(10, combat_feat),
                                                                    Helpers.LevelEntry(11, maneuver_training[2], brawlers_flurry11),
                                                                    Helpers.LevelEntry(12, combat_feat, brawlers_strike_alignment),
                                                                    Helpers.LevelEntry(13),
                                                                    Helpers.LevelEntry(14, combat_feat),
                                                                    Helpers.LevelEntry(15, maneuver_training[3]),
                                                                    Helpers.LevelEntry(16, combat_feat, awesome_blow),
                                                                    Helpers.LevelEntry(17, brawlers_strike_adamantine),
                                                                    Helpers.LevelEntry(18, combat_feat),
                                                                    Helpers.LevelEntry(19, maneuver_training[4]),
                                                                    Helpers.LevelEntry(20, combat_feat, awesome_blow_improved, perfect_warrior)
                                                                    };

            brawler_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { brawler_proficiencies, unarmed_strike, improved_unarmed_strike, martial_training };
            brawler_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(brawlers_flurry, brawlers_flurry11),
                                                                Helpers.CreateUIGroup(combat_feat),
                                                                Helpers.CreateUIGroup(maneuver_training),
                                                                Helpers.CreateUIGroup(brawlers_strike_magic, brawlers_strike_cold_iron_and_silver, brawlers_strike_alignment, brawlers_strike_adamantine),
                                                                Helpers.CreateUIGroup(brawlers_cunning, ac_bonus, close_weapon_mastery, perfect_warrior),
                                                                Helpers.CreateUIGroup(knockout, awesome_blow, awesome_blow_improved)
                                                           };
        }


        static void createAwesomeBlow()
        {
            awesome_blow_improved = Helpers.CreateFeature("AwesomeBlowImprovedFeature",
                                                          "Improved Awesome Blow",
                                                          "At 20th level, the brawler can use her awesome blow ability on creatures of any size.",
                                                          "",
                                                          Helpers.GetIcon("c5a39c8f1a2d6824ca565e6c1e4075a5"), //pummeling charge
                                                          FeatureGroup.None
                                                          );

            var ability = library.CopyAndAdd<BlueprintAbility>("6fd05c4ecfebd6f4d873325de442fc17", "AwesomeBlowAction", "");
            ability.SetNameDescriptionIcon("Awesome Blow",
                                           "At 16th level, the brawler can as a standard action perform an awesome blow combat maneuver against a corporeal creature of her size or smaller. If the combat maneuver check succeeds, the opponent takes damage as if the brawler hit it with the close weapon she is wielding or an unarmed strike, it is knocked flying 10 feet in a direction of attack.",
                                           Helpers.GetIcon("bdf58317985383540920c723db07aa3b") //pummeling bully
                                           );

            ability.ReplaceComponent<AbilityEffectRunAction>(a =>
            {
                a.Actions = Helpers.CreateActionList(Helpers.Create<ContextActionCombatManeuver>(c =>
                {
                    c.Type = CombatManeuverTypeExtender.AwesomeBlow.ToCombatManeuverType();
                    c.OnSuccess = Helpers.CreateActionList(Helpers.Create<ContextActionDealWeaponDamage>(),
                                                           Helpers.Create<CombatManeuverMechanics.ContextActionForceMove>(f => f.distance_dice = Helpers.CreateContextDiceValue(DiceType.Zero, 0, 10))
                                                           );
                })
                );
            });

            ability.AddComponents(Helpers.Create<CombatManeuverMechanics.AbilityTargetNotBiggerUnlesFact>(a => a.fact = awesome_blow_improved),
                                  Helpers.Create<NewMechanics.AbilityCasterMainWeaponGroupCheck>(a =>
                                  {
                                      a.groups = new WeaponFighterGroup[] { WeaponFighterGroup.Close };
                                      a.extra_categories = new WeaponCategory[] { WeaponCategory.UnarmedStrike };
                                  }
                                  )
                                  );

            awesome_blow = Common.AbilityToFeature(ability, false);
        }


        static void createCloseWeaponMastery()
        {
            DiceFormula[] diceFormulas = new DiceFormula[] {new DiceFormula(1, DiceType.D6),
                                                            new DiceFormula(1, DiceType.D8),
                                                            new DiceFormula(1, DiceType.D10),
                                                            new DiceFormula(2, DiceType.D6),
                                                            new DiceFormula(2, DiceType.D8)};

            close_weapon_mastery = Helpers.CreateFeature("BrawlerCloseWeaponMasteryFeature",
                                              "Close Weapon Mastery",
                                              "At 5th level, a brawler’s damage with close weapons increases. When wielding a close weapon, she uses the unarmed strike damage of a brawler 4 levels lower instead of the base damage for that weapon (for example, a 5th-level Medium brawler wielding a punching dagger deals 1d6 points of damage instead of the weapon’s normal 1d4). If the weapon normally deals more damage than this, its damage is unchanged. This ability does not affect any other aspect of the weapon. The brawler can decide to use the weapon’s base damage instead of her adjusted unarmed strike damage—this must be declared before the attack roll is made.",
                                              "",
                                              Helpers.GetIcon("121811173a614534e8720d7550aae253"), //shield bash
                                              FeatureGroup.None,
                                              Helpers.Create<NewMechanics.ContextWeaponDamageDiceReplacementWeaponCategory>(c =>
                                              {
                                                  c.dice_formulas = diceFormulas;
                                                  c.value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                  c.categories = new WeaponCategory[] {WeaponCategory.SpikedHeavyShield, WeaponCategory.SpikedLightShield,
                                                                                       WeaponCategory.WeaponLightShield, WeaponCategory.WeaponHeavyShield,
                                                                                       WeaponCategory.UnarmedStrike, WeaponCategory.PunchingDagger};
                                              }),
                                              Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                type: AbilityRankType.Default,
                                                                                progression: ContextRankProgression.StartPlusDivStep,
                                                                                startLevel: 4,
                                                                                stepLevel: 4,
                                                                                classes: getBrawlerArray())
                                              );
        }

        static void createBrawlersStrike()
        {
            brawlers_strike_magic = library.CopyAndAdd<BlueprintFeature>("1188005ee3160f84f8bed8b19a7d46cf", "BrawlerKiStrikeMagic", "");
            brawlers_strike_magic.SetNameDescription("Brawler's Strike —  Magic",
                                                     "At 5th level, a brawler’s unarmed strikes are treated as magic weapons for the purpose of overcoming damage reduction");

            brawlers_strike_cold_iron_and_silver = library.CopyAndAdd<BlueprintFeature>("7b657938fde78b14cae10fc0d3dcb991", "BrawlerStrikeColdIronSilver", "");
            brawlers_strike_cold_iron_and_silver.SetNameDescription("Brawler's Strike —  Cold Iron and Silver",
                                                                "At 9th level, the brawler's unarmed attacks are treated as cold iron and silver for the purpose of overcoming damage reduction.");
            var damage_alignments = new DamageAlignment[]
            {
                DamageAlignment.Lawful,
                DamageAlignment.Chaotic,
                DamageAlignment.Good,
                DamageAlignment.Evil
            };

            var alignment = new AlignmentMaskType[]
            {
               AlignmentMaskType.Lawful,
               AlignmentMaskType.Chaotic,
               AlignmentMaskType.Good,
               AlignmentMaskType.Evil
            };
          
            brawlers_strike_alignment = Helpers.CreateFeatureSelection("BrawlersStrikeAlignmentFeatureSelection",
                                                                        "Aligned Brawler's Strike",
                                                                        "At 12th level, the brawler chooses one alignment component: chaotic, evil, good, or lawful; her unarmed strikes also count as this alignment for the purpose of overcoming damage reduction. (This alignment component cannot be the opposite of the brawler’s actual alignment, such as a good brawler choosing evil strikes.)",
                                                                        "",
                                                                        null,
                                                                        FeatureGroup.None
                                                                        );

            for (int i = 0; i < damage_alignments.Length; i++)
            {
                var strike = library.CopyAndAdd<BlueprintFeature>("34439e527a8f5fb4588024e71960dd42", "BrawlersStrike" + damage_alignments[i].ToString(), "");
                strike.SetNameDescription($"Braawler's Strike — {damage_alignments[i].ToString()}",
                                          brawlers_strike_alignment.Description);
                strike.AddComponent(Common.createPrerequisiteAlignment(~alignment[i]));

                brawlers_strike_alignment.AllFeatures = brawlers_strike_alignment.AllFeatures.AddToArray(strike);
            }

            brawlers_strike_adamantine = library.CopyAndAdd<BlueprintFeature>("ddc10a3463bd4d54dbcbe993655cf64e", "BrawlersStrikeAdamantine", "");
            brawlers_strike_adamantine.SetNameDescription("Brawler's Strike — Adamantine",
                                                          "At 19th level, the brawler's unarmed attacks are treated as adamantine weapons for the purpose of overcoming damage reduction and bypassing hardness.");
        }


        static void createKnockout()
        {
            knockout_resource = Helpers.CreateAbilityResource("BrawlerKnockoutResource", "", "", "", null);
            knockout_resource.SetIncreasedByLevelStartPlusDivStep(1, 10, 1, 6, 1, 0, 0.0f, getBrawlerArray());

            var effect_buff = Helpers.CreateBuff("BrawlerKnockoutBuff",
                                          "Knocked out",
                                          "At 4th level, once per day a brawler can unleash a devastating attack that can instantly knock a target unconscious. She must announce this intent before making her attack roll. If the brawler hits and the target takes damage from the blow, the target must succeed at a Fortitude saving throw (DC = 10 + 1/2 the brawler’s level + the higher of the brawler’s Strength or Dexterity modifier) or fall unconscious for 1d6 rounds. Each round on its turn, the unconscious target may attempt a new saving throw to end the effect as a full-round action that does not provoke attacks of opportunity. Creatures immune to critical hits or nonlethal damage are immune to this ability. At 10th level, the brawler may use this ability twice per day; at 16th level, she may use it three times per day.",
                                          "",
                                          Helpers.GetIcon("247a4068296e8be42890143f451b4b45"),//basic feat
                                          null,
                                          Common.createBuffStatusCondition(UnitCondition.Unconscious, SavingThrowType.Fortitude)
                                          );

            var apply_buff = Common.createContextActionApplyBuff(effect_buff, Helpers.CreateContextDuration(0, DurationRate.Rounds, DiceType.D6, 1), dispellable: false);
            var effect = Helpers.CreateActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateConditionalSaved(null, apply_buff));
            var on_hit = Helpers.CreateConditional(Common.createContextConditionHasFacts(all: false, Common.undead, Common.construct, Common.elemental, Common.aberration),
                                                   null,
                                                   effect);
            var buff = Helpers.CreateBuff("BrawlerKnockoutOwnerBuff",
                                          "Knockout",
                                          effect_buff.Description,
                                          "",
                                          effect_buff.Icon,
                                          null
                                          );

            var physical_stat_property = NewMechanics.HighestStatPropertyGetter.createProperty("BrawlerStrOrDexProperty", "", StatType.Strength, StatType.Dexterity);

            buff.AddComponents(Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(effect, Common.createContextActionOnContextCaster(Common.createContextActionRemoveBuff(buff))),
                                                                               wait_for_attack_to_resolve: true),

                             Common.createContextCalculateAbilityParamsBasedOnClassesWithProperty(getBrawlerArray(), physical_stat_property)
                             );

            var ability = Helpers.CreateAbility("BrawlerKnockoutAbility",
                                                 buff.Name,
                                                 buff.Description,
                                                 "",
                                                 buff.Icon,
                                                 AbilityType.Extraordinary,
                                                 CommandType.Free,
                                                 AbilityRange.Personal,
                                                 "",
                                                 "",
                                                 Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false)),
                                                 Common.createAbilityCasterHasNoFacts(buff),
                                                 knockout_resource.CreateResourceLogic()
                                                 );

            knockout = Common.AbilityToFeature(ability, false);
            knockout.AddComponent(knockout_resource.CreateAddAbilityResource());
        }


        static void createPerfectWarrior()
        {
            perfect_warrior = Helpers.CreateFeature("PerfectWarriorBrawlerFeature",
                                                    "Perfect Warrior",
                                                    "At 20th level, the brawler has reached the highest levels of her art. The brawler’s maneuver training increases by 2 and her dodge bonus to AC improves by 2. This replaces the 20th-level improvement to martial flexibility.",
                                                    "",
                                                    Helpers.GetIcon("2a6a2f8e492ab174eb3f01acf5b7c90a"), //defensive stance
                                                    FeatureGroup.None
                                                    );
        }


        static void createACBonus()
        {
            var ac_bonus_feature = Helpers.CreateFeature("BrawlerACBonusFeature",
                                                         "AC Bonus",
                                                         "At 4th level, when a brawler wears light or no armor, she gains a +1 dodge bonus to AC and CMD. This bonus increases by 1 at 9th, 13th, and 18th levels.\n"
                                                         + "These bonuses to AC apply against touch attacks. She loses these bonuses while immobilized or helpless, wearing medium or heavy armor, or carrying a medium or heavy load.",
                                                         "",
                                                         Helpers.GetIcon("97e216dbb46ae3c4faef90cf6bbe6fd5"), //dodge
                                                         FeatureGroup.None,
                                                         Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.Dodge),
                                                         Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.Dodge, rankType: AbilityRankType.StatBonus),
                                                         Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                     classes: getBrawlerArray(),
                                                                                     progression: ContextRankProgression.Custom,
                                                                                     customProgression: new (int, int)[] { (8, 1), (12, 2), (17, 3), (20, 4) }),
                                                         Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureList,
                                                                                     type: AbilityRankType.StatBonus,
                                                                                     featureList: new BlueprintFeature[] { perfect_warrior, perfect_warrior }
                                                                                     )
                                                         );

            ac_bonus = Helpers.CreateFeature("BrawlerACBonusUnlock",
                                             ac_bonus_feature.Name,
                                             ac_bonus_feature.Description,
                                             "",
                                             ac_bonus_feature.Icon,
                                             FeatureGroup.None,
                                             Helpers.Create<WeaponTrainingMechanics.AddFeatureOnArmor>(a =>
                                             {
                                                 a.feature = ac_bonus_feature;
                                                 a.required_armor = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.Light, ArmorProficiencyGroup.None };
                                             })
                                             );

        }


        static void createManeuverTraining()
        {
            var maneuvers = new CombatManeuver[][] { new CombatManeuver[] { CombatManeuver.BullRush },
                                                     new CombatManeuver[] {CombatManeuver.Disarm },
                                                     new CombatManeuver[] {CombatManeuver.Trip },
                                                     new CombatManeuver[] {CombatManeuver.SunderArmor },
                                                     new CombatManeuver[] {CombatManeuver.DirtyTrickBlind, CombatManeuver.DirtyTrickEntangle, CombatManeuver.DirtyTrickSickened }
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

            for (int i = 0; i < maneuver_training.Length; i++)
            {

                maneuver_training[i] = Helpers.CreateFeatureSelection($"ManeuverTraining{i + 1}FeatureSelection",
                                                                   "Maneuver Training ",
                                                                   "At 3rd level, a brawler can select one combat maneuver to receive additional training. She gains a +1 bonus on combat maneuver checks when performing that combat maneuver and a +1 bonus to her CMD when defending against that maneuver.\n"
                                                                   + "At 7th level and every 4 levels thereafter, the brawler becomes further trained in another combat maneuver, gaining the above +1 bonus combat maneuver checks and to CMD. In addition, the bonuses granted by all previous maneuver training increase by 1 each.",
                                                                   "",
                                                                   null,
                                                                   FeatureGroup.None);
                maneuver_training[i].AllFeatures = new BlueprintFeature[0];

                int lvl = 3 + i * 4;
                for (int j = 0; j < maneuvers.Length; j++)
                {
                    var feat = Helpers.CreateFeature($"ManeuverTraining{i+1}" + maneuvers[j][0].ToString() + "Feature",
                                                     maneuver_training[i].Name + ": " + names[j] + $" ({lvl})",
                                                     maneuver_training[i].Description,
                                                     "",
                                                     icons[j],
                                                     FeatureGroup.None,
                                                     Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                     classes: getBrawlerArray(),
                                                                                     progression: ContextRankProgression.StartPlusDivStep,
                                                                                     startLevel: lvl,
                                                                                     stepLevel: 4),
                                                     Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureList,
                                                                                     type: AbilityRankType.StatBonus,
                                                                                     featureList: new BlueprintFeature[] {perfect_warrior, perfect_warrior}
                                                                                     )
                                                     );

                    foreach (var maneuver in maneuvers[j])
                    {
                        feat.AddComponents(Helpers.Create<CombatManeuverMechanics.SpecificCombatManeuverBonus>(s => { s.maneuver_type = maneuver; s.Value = Helpers.CreateContextValue(AbilityRankType.Default); }),
                                           Common.createContextManeuverDefenseBonus(maneuver, Helpers.CreateContextValue(AbilityRankType.Default)),
                                           Helpers.Create<CombatManeuverMechanics.SpecificCombatManeuverBonus>(s => { s.maneuver_type = maneuver; s.Value = Helpers.CreateContextValue(AbilityRankType.StatBonus); }),
                                           Common.createContextManeuverDefenseBonus(maneuver, Helpers.CreateContextValue(AbilityRankType.StatBonus))
                                           );
                    }
                    maneuver_training[i].AllFeatures = maneuver_training[i].AllFeatures.AddToArray(feat);
                }
            }
        }


        static void createBrawlersFlurry()
        {
            var flurry1_monk = library.Get<BlueprintFeature>("332362f3bd39ebe46a740a36960fdcb4");
            var flurry11_monk = library.Get<BlueprintFeature>("de25523acc24b1448aa90f74d6512a08");
            flurry1_monk.Ranks++;
            flurry11_monk.Ranks++;


            brawlers_flurry = Helpers.CreateFeature("BrawlersFlurryUnlock",
                                                    "Brawler’s Flurry",
                                                    "At 2nd level, a brawler can make a flurry of blows as a full attack. When making a flurry of blows, the brawler can make one additional attack at his highest base attack bonus. This additional attack stacks with the bonus attacks from haste and other similar effects. When using this ability, the brawler can make these attacks with any combination of his unarmed strikes, weapons from the close fighter weapon group, or weapons with thee monk special weapon quality. He takes no penalty for using multiple weapons when making a flurry of blows, but he does not gain any additional attacks beyond what's already granted by the flurry for doing so. (He can still gain additional attacks from a high base attack bonus, from this ability, and from haste and similar effects).\nAt 11th level, a brawler can make an additional attack at his highest base attack bonus whenever he makes a flurry of blows. This stacks with the first attack from this ability and additional attacks from haste and similar effects.",
                                                    "",
                                                    null,
                                                    FeatureGroup.None,
                                                    Helpers.Create<FeralCombatTraining.SpecificWeaponGroupOrFeralCombatFeatureUnlock>(s =>
                                                    {
                                                        s.NewFact = flurry1_monk;
                                                        s.weapon_groups = new WeaponFighterGroup[] { WeaponFighterGroup.Monk, WeaponFighterGroup.Close };
                                                    })
                                                    );

            brawlers_flurry11 = library.CopyAndAdd(brawlers_flurry, "BrawlersFlurry11Unlock", "");
            brawlers_flurry11.ReplaceComponent<FeralCombatTraining.SpecificWeaponGroupOrFeralCombatFeatureUnlock>(f => f.NewFact = flurry11_monk);


            var buff = Helpers.CreateBuff("BrawlerTwoWeaponFlurryEnabledBuff",
                                          "Brawler’s Flurry: Use Off-Hand attacks",
                                          brawlers_flurry.Description,
                                          "",
                                          Helpers.GetIcon("ac8aaf29054f5b74eb18f2af950e752d"), //twf
                                          null,
                                          Helpers.Create<ActivateBrawlerTwoWeaponFlurry>()
                                          );

            var toggle = Common.buffToToggle(buff, CommandType.Free, false);

            brawlers_flurry.AddComponents(Helpers.Create<AddBrawlerTwoWeaponFlurryPart>(a => a.groups = new WeaponFighterGroup[] { WeaponFighterGroup.Monk, WeaponFighterGroup.Close }),
                              Helpers.Create<BrawlerTwoWeaponFlurryExtraAttack>(),
                              Helpers.CreateAddFact(toggle)
                             );
            brawlers_flurry11.AddComponents(Helpers.Create<BrawlerTwoWeaponFlurryExtraAttack>());
        }

        static void createBrawlersCunning()
        {
            brawlers_cunning = Helpers.CreateFeature("BrawlersCunningFeature",
                                                     "Brawler’s Cunning",
                                                     "If the brawler’s Intelligence score is less than 13, it counts as 13 for the purpose of meeting the prerequisites of combat feats.",
                                                     "",
                                                     Helpers.GetIcon("ae4d3ad6a8fda1542acf2e9bbc13d113"), //foxs cunning
                                                     FeatureGroup.None,
                                                     Helpers.Create<ReplaceStatForPrerequisites>(r => { r.OldStat = StatType.Intelligence; r.SpecificNumber = 13; r.Policy = ReplaceStatForPrerequisites.StatReplacementPolicy.SpecificNumber; })
                                                     );
        }


        static void createMartialTraining()
        {
            var monk_class = library.Get<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");
            var fighter_class = library.Get<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");

            martial_training = Helpers.CreateFeature("BrawlerMartialTrainingFeat",
                                         "Martial Training",
                                         "At 1st level, a brawler counts her total brawler levels as both fighter levels and monk levels for the purpose of qualifying for feats. She also counts as both a fighter and a monk for feats and magic items that have different effects based on whether the character has levels in those classes (such as Stunning Fist and a monk’s robe). This ability does not automatically grant feats normally granted to fighters and monks based on class level, namely Stunning Fist.",
                                         "",
                                         null,
                                         FeatureGroup.None,
                                         Common.createClassLevelsForPrerequisites(monk_class, brawler_class),
                                         Common.createClassLevelsForPrerequisites(fighter_class, brawler_class)
                                         );

            var stunning_fist_resource = library.Get<BlueprintAbilityResource>("d2bae584db4bf4f4f86dd9d15ae56558");
            ClassToProgression.addClassToResource(brawler_class, new BlueprintArchetype[0], stunning_fist_resource, monk_class);
            //perfect strike will make a copy of resource, so will be update automatically

            //fix robes ?
        }


        public class UnitPartBrawler : UnitPart
        {
            [JsonProperty]
            private bool active;
            [JsonProperty]
            private int extra_attacks = 0;
            [JsonProperty]
            private WeaponFighterGroup[] weapon_groups;

            public void initialize(params WeaponFighterGroup[] brawler_weapon_groups)
            {
                weapon_groups = brawler_weapon_groups;
            }

            public bool isActive()
            {
                return active;
            }

            public void activate()
            {
                active = true;
            }

            public void deactivate()
            {
                active = false;
            }

            public void increseExtraAttacks()
            {
                extra_attacks++;
            }

            public void decreaseExtraAttacks()
            {
                extra_attacks--;
            }

            public int getNumExtraAttacks()
            {
                return extra_attacks;
            }


            public bool checkTwoWeapponFlurry()
            {
                if (!isActive())
                {
                    return false;
                }

                if (!Owner.Body.PrimaryHand.HasWeapon || !Owner.Body.SecondaryHand.HasWeapon
                    || (Owner.Body.PrimaryHand.Weapon.Blueprint.IsNatural || Owner.Body.SecondaryHand.Weapon.Blueprint.IsNatural)
                    || (Owner.Body.PrimaryHand.Weapon == Owner.Body.EmptyHandWeapon || Owner.Body.SecondaryHand.Weapon == Owner.Body.EmptyHandWeapon))
                {
                    return false;
                }

                var weapon1 = Owner.Body.PrimaryHand.MaybeWeapon;
                var weapon2 = Owner.Body.SecondaryHand.MaybeWeapon;

                return (weapon_groups.Contains(weapon1.Blueprint.FighterGroup)
                      || FeralCombatTraining.checkHasFeralCombat(this.Owner.Unit, weapon1, allow_crusaders_flurry: true))
                    && (weapon_groups.Contains(weapon2.Blueprint.FighterGroup)
                        || FeralCombatTraining.checkHasFeralCombat(this.Owner.Unit, weapon2, allow_crusaders_flurry: true));
            }
        }



        //fix twf to work correctly with and brawlers flurry
        [Harmony12.HarmonyPriority(Harmony12.Priority.First)]
        [Harmony12.HarmonyPatch(typeof(TwoWeaponFightingAttacks))]
        [Harmony12.HarmonyPatch("OnEventAboutToTrigger", Harmony12.MethodType.Normal)]
        [Harmony12.HarmonyPatch(new Type[] { typeof(RuleCalculateAttacksCount) })]
        class TwoWeaponFightingAttacks__OnEventAboutToTrigger__Patch
        {
            static bool Prefix(TwoWeaponFightingAttacks __instance, RuleCalculateAttacksCount evt)
            {
                var brawler_part = evt.Initiator?.Get<Brawler.UnitPartBrawler>();
                if ((brawler_part?.checkTwoWeapponFlurry()).GetValueOrDefault())
                {
                    for (int i = 1; i < brawler_part.getNumExtraAttacks(); i++)
                    {
                        ++evt.SecondaryHand.MainAttacks;
                    }
                    return true;
                }

                return true;
            }
        }


        //fix twf to work correctly with and brawlers flurry
        [Harmony12.HarmonyPriority(Harmony12.Priority.First)]
        [Harmony12.HarmonyPatch(typeof(TwoWeaponFightingDamagePenalty))]
        [Harmony12.HarmonyPatch("OnEventAboutToTrigger", Harmony12.MethodType.Normal)]
        [Harmony12.HarmonyPatch(new Type[] { typeof(RuleCalculateWeaponStats) })]
        class TwoWeaponFightingDamagePenalty__OnEventAboutToTrigger__Patch
        {
            static bool Prefix(TwoWeaponFightingDamagePenalty __instance, RuleCalculateWeaponStats evt)
            {
                var brawler_part = evt.Initiator?.Get<Brawler.UnitPartBrawler>();
                if ((brawler_part?.checkTwoWeapponFlurry()).GetValueOrDefault())
                {
                    return false;
                }

                return true;
            }
        }



        [AllowedOn(typeof(BlueprintUnitFact))]
        public class AddBrawlerTwoWeaponFlurryPart : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
        {
            public WeaponFighterGroup[] groups;

            public override void OnFactActivate()
            {
                this.Owner.Ensure<UnitPartBrawler>().initialize(groups);
            }


            public override void OnFactDeactivate()
            {
                this.Owner.Remove<UnitPartBrawler>();
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class BrawlerTwoWeaponFlurryExtraAttack : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
        {

            public override void OnFactActivate()
            {
                this.Owner.Ensure<UnitPartBrawler>().increseExtraAttacks();
            }


            public override void OnFactDeactivate()
            {
                this.Owner.Ensure<UnitPartBrawler>().decreaseExtraAttacks();
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ActivateBrawlerTwoWeaponFlurry : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
        {

            public override void OnTurnOn()
            {
                this.Owner.Get<UnitPartBrawler>()?.activate();
            }


            public override void OnTurnOff()
            {
                this.Owner.Get<UnitPartBrawler>()?.activate();
            }
        }
    }
}
