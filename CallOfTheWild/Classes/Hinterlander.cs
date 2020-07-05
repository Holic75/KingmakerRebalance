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
using Kingmaker.Blueprints.Items.Weapons;
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

namespace CallOfTheWild
{
    public class Hinterlander
    {
        static LibraryScriptableObject library => Main.library;
        internal static bool test_mode = false;
        static public BlueprintCharacterClass hinterlander_class;
        static public BlueprintProgression hinterlander_progression;

        static public BlueprintFeature hinterlander_proficiencies;
        static public BlueprintFeatureSelection favored_enemy_selection;
        static public BlueprintFeatureSelection improved_favored_enemy_selection;
        static public BlueprintFeatureSelection spellbook_selection;
        static public BlueprintFeature favored_terrain;
        static public BlueprintFeatureSelection master_archer1;
        static public BlueprintFeatureSelection master_archer2;
        static public BlueprintFeatureSelection master_archer3;
        static public BlueprintFeature imbue_arrow;
        static public BlueprintFeature choosen_kin;


        internal static void createHinterlanderClass()
        {
            var ranger_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("cda0615668a6df14eb36ba19ee881af6");
            var druid_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");

            var endurance_feat = library.Get<BlueprintFeature>("54ee847996c25cd4ba8773d7b8555174");
            var weapon_focus = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");

            var savesPrestigeLow = library.Get<BlueprintStatProgression>("dc5257e1100ad0d48b8f3b9798421c72");
            var savesPrestigeHigh = library.Get<BlueprintStatProgression>("1f309006cd2855e4e91a6c3707f3f700");

            hinterlander_class = Helpers.Create<BlueprintCharacterClass>();
            hinterlander_class.name = "HinterlanderClass";
            library.AddAsset(hinterlander_class, "");
            
            hinterlander_class.LocalizedName = Helpers.CreateString("Hinterlander.Name", "Hinterlander");
            hinterlander_class.LocalizedDescription = Helpers.CreateString("Hinterlander.Description",
                "Guards patrol great cities and druids protect the deep forests, but what of the boundaries between them? The hinterlands comprise the farmlands and woodland villages, a liminal space between humanity and the wild. Some of the rugged men and women who defend these pockets of civilization are the hinterlanders.\n"
                + "Hinterlanders shield rural folk from dangerous creatures, extraplanar interlopers, and marauding undead. A hinterlander is skilled with a bow and in tune with the wild, yet capable of harnessing natural resources to nourish mortal communities. They are often touched by the divine, sworn to guard the borders of towns and villages against the forces that threaten to snuff out the hearth fires."
                );
            hinterlander_class.m_Icon = ranger_class.Icon;
            hinterlander_class.SkillPoints = druid_class.SkillPoints;
            hinterlander_class.HitDie = DiceType.D10;
            hinterlander_class.BaseAttackBonus = druid_class.BaseAttackBonus;
            hinterlander_class.FortitudeSave = savesPrestigeLow;
            hinterlander_class.ReflexSave = savesPrestigeLow;
            hinterlander_class.WillSave = savesPrestigeHigh;
            hinterlander_class.ClassSkills = new StatType[] {StatType.SkillAthletics, StatType.SkillMobility, StatType.SkillPerception, StatType.SkillLoreReligion, StatType.SkillKnowledgeWorld, StatType.SkillStealth };
            hinterlander_class.IsDivineCaster = true;
            hinterlander_class.IsArcaneCaster = true;
            hinterlander_class.PrestigeClass = true;
            hinterlander_class.StartingGold = ranger_class.StartingGold;
            hinterlander_class.PrimaryColor = ranger_class.PrimaryColor;
            hinterlander_class.SecondaryColor = ranger_class.SecondaryColor;
            hinterlander_class.RecommendedAttributes = new StatType[] { StatType.Dexterity };
            hinterlander_class.EquipmentEntities = ranger_class.EquipmentEntities;
            hinterlander_class.MaleEquipmentEntities = ranger_class.MaleEquipmentEntities;
            hinterlander_class.FemaleEquipmentEntities = ranger_class.FemaleEquipmentEntities;
            hinterlander_class.StartingItems = ranger_class.StartingItems;

            hinterlander_class.ComponentsArray = ranger_class.ComponentsArray;
            hinterlander_class.AddComponent(Common.createPrerequisiteAlignment(AlignmentMaskType.LawfulGood | AlignmentMaskType.NeutralGood));
            hinterlander_class.AddComponent(Helpers.PrerequisiteStatValue(StatType.SkillLoreNature, 5));
            hinterlander_class.AddComponent(Helpers.PrerequisiteFeature(endurance_feat));
            hinterlander_class.AddComponent(Common.createPrerequisiteParametrizedFeatureWeapon(weapon_focus, WeaponCategory.Longbow));
            hinterlander_class.AddComponent(Common.createPrerequisiteCasterTypeSpellLevel(true, 1, true));
            hinterlander_class.AddComponent(Helpers.Create<SpellbookMechanics.PrerequisiteDivineCasterTypeSpellLevel>(p => { p.RequiredSpellLevel = 1; p.Group = Prerequisite.GroupType.Any; }));
            hinterlander_class.AddComponent(Helpers.Create<SpellbookMechanics.PrerequisitePsychicCasterTypeSpellLevel>(p => { p.RequiredSpellLevel = 1; p.Group = Prerequisite.GroupType.Any; }));

            createHinterlanderProgression();
            hinterlander_class.Progression = hinterlander_progression;

            Helpers.RegisterClass(hinterlander_class);
        }


        static void createHinterlanderProgression()
        {
            createHinterlanderProficiencies();
            createFavoredEnemy();
            createFavoredTerrain();

            var fast_movement = library.Get<BlueprintFeature>("d294a5dddd0120046aae7d4eb6cbc4fc");
            var woodland_stride = library.Get<BlueprintFeature>("11f4072ea766a5840a46e6660894527d");

            createMasterArcher();
            createSpellbookSelection();
            createImbueArrow();

            hinterlander_progression = Helpers.CreateProgression("HinterlandertProgression",
                                                               hinterlander_class.Name,
                                                               hinterlander_class.Description,
                                                               "",
                                                               hinterlander_class.Icon,
                                                               FeatureGroup.None);
            hinterlander_progression.Classes = getHinterlanderArray();

            hinterlander_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, hinterlander_proficiencies, master_archer1, favored_enemy_selection),
                                                                      Helpers.LevelEntry(2, fast_movement, favored_terrain, spellbook_selection),
                                                                      Helpers.LevelEntry(3, woodland_stride, master_archer2),
                                                                      Helpers.LevelEntry(4, choosen_kin),
                                                                      Helpers.LevelEntry(5, master_archer3),
                                                                      Helpers.LevelEntry(6),
                                                                      Helpers.LevelEntry(7, imbue_arrow),
                                                                      Helpers.LevelEntry(8, favored_enemy_selection, improved_favored_enemy_selection),
                                                                      Helpers.LevelEntry(9, favored_terrain),
                                                                      Helpers.LevelEntry(10, master_archer3)
                                                                    };

            hinterlander_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] {hinterlander_proficiencies};
            hinterlander_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(master_archer1, master_archer2, master_archer3, master_archer3),
                                                         Helpers.CreateUIGroup(fast_movement, woodland_stride, choosen_kin, improved_favored_enemy_selection),
                                                         Helpers.CreateUIGroup(favored_enemy_selection, favored_enemy_selection, favored_terrain, favored_terrain, choosen_kin),
                                                         Helpers.CreateUIGroup(spellbook_selection, imbue_arrow)
                                                        };
        }

        static BlueprintCharacterClass[] getHinterlanderArray()
        {
            return new BlueprintCharacterClass[] { hinterlander_class };
        }
        static void createHinterlanderProficiencies()
        {
            hinterlander_proficiencies = library.CopyAndAdd<BlueprintFeature>("8c971173613282844888dc20d572cfc9", "HinterlanderProficiencies", "");//cleric
            hinterlander_proficiencies.AddComponent(Common.createAddWeaponProficiencies(WeaponCategory.Longbow));
            hinterlander_proficiencies.SetName("Hinterlander Proficiencies");
            hinterlander_proficiencies.SetDescription("A hinterlander is proficient with all simple weapons, longbows, light armor, medium armor, and shields (except tower shields).");
        }


        static void createFavoredEnemy()
        {
            favored_enemy_selection = library.CopyAndAdd<BlueprintFeatureSelection>("16cc2c937ea8d714193017780e7d4fc6", "HinterlanderFavoredEnemySelection", "");
            favored_enemy_selection.SetDescription("At 1st level, a hinterlander chooses a favored enemy from the list below.This functions like the ranger class feature of the same name, except that the hinterlander gains an additional ability corresponding with the favored enemy chosen.Additionally, at 8th level, he can select a new favored enemy and the bonus for one such enemy increases by 2 (rather than at 5th and 10th levels).");
            favored_enemy_selection.AllFeatures = new BlueprintFeature[0];

            improved_favored_enemy_selection = library.CopyAndAdd<BlueprintFeatureSelection>("c1be13839472aad46b152cf10cf46179", "HinterlanderImprovedFavoredEnemy", "");
            improved_favored_enemy_selection.SetDescription(favored_enemy_selection.Description);
            improved_favored_enemy_selection.AllFeatures = new BlueprintFeature[0];
            improved_favored_enemy_selection.SetName("Improved Favored Enemy");

            var aberrations = library.Get<BlueprintFeature>("7081934ab5f8573429dbd26522adcc39");
            var constructs = library.Get<BlueprintFeature>("6ea5a4a19ccb81a498e18a229cc5038a");
            var outsiders = library.Get<BlueprintFeature>("f643b38acc23e8e42a3ed577daeb6949");
            var magical_beasts = library.Get<BlueprintFeature>("f807fac786faa86438428c79f5629654");
            var monstrous_humanoids = library.Get<BlueprintFeature>("0fd21e10dff071e4580ef9f30a0334df");
            var undead = library.Get<BlueprintFeature>("5941963eae3e9864d91044ba771f2cc2");


            addToFavoredEnemySelection("HinterlanderFavoredEnemyAberrations",
                                       "The hinterlander gains a +2 bonus on all saves against spells, spell-like abilities, and supernatural abilities used by aberrations.",
                                       aberrations,
                                       Common.createContextSavingThrowBonusAgainstFact(aberrations.GetComponent<AddFavoredEnemy>().CheckedFacts[0] as BlueprintFeature,
                                                                                     AlignmentComponent.None,
                                                                                     Common.createSimpleContextValue(2),
                                                                                     ModifierDescriptor.UntypedStackable)
                                       );

            addToFavoredEnemySelection("HinterlanderFavoredEnemyConstructs",
                                       "The hinterlander’s ranged attacks ignore the first 10 points of a construct’s DR.",
                                       constructs,
                                       Common.createReduceDRForFactOwner(10, constructs.GetComponent<AddFavoredEnemy>().CheckedFacts[0] as BlueprintFeature, AttackType.Ranged, AttackType.RangedTouch)
                                       );

            addToFavoredEnemySelection("HinterlanderFavoredEnemyOutsiders",
                                       "The hinterlander’s ranged attacks are treated as good weapons for the purposes of overcoming damage reduction.",
                                       outsiders,
                                       Common.createAddOutgoingAlignment(DamageAlignment.Good, check_range: true, is_ranged: true)
                                       );

            addToFavoredEnemySelection("HinterlanderFavoredEnemyMagicalBeasts",
                                       "The hinterlander gains a +2 bonus on all saves against spells, spell-like abilities, and supernatural abilities used by magical beasts.",
                                       magical_beasts,
                                       Common.createContextSavingThrowBonusAgainstFact(magical_beasts.GetComponent<AddFavoredEnemy>().CheckedFacts[0] as BlueprintFeature,
                                                                                     AlignmentComponent.None,
                                                                                     Common.createSimpleContextValue(2),
                                                                                     ModifierDescriptor.UntypedStackable)
                                       );

            addToFavoredEnemySelection("HinterlanderFavoredEnemyMonstrousHumanoids",
                                       "The hinterlander gains a +2 bonus on all saves against spells, spell-like abilities, and supernatural abilities used by monstrous humanoids.",
                                       monstrous_humanoids,
                                       Common.createContextSavingThrowBonusAgainstFact(monstrous_humanoids.GetComponent<AddFavoredEnemy>().CheckedFacts[0] as BlueprintFeature,
                                                                                     AlignmentComponent.None,
                                                                                     Common.createSimpleContextValue(2),
                                                                                     ModifierDescriptor.UntypedStackable)
                                       );

            addToFavoredEnemySelection("HinterlanderFavoredEnemyUndead",
                                       "Arrows loosed from a hinterlander’s bow are infused with divine power and deal full damage to any undead that are incorporeal, as if the arrows had the ghost touch weapon special ability.",
                                       undead,
                                       Common.createAddOutgoingGhost(check_range: true, is_ranged: true)
                                       );



        }


        static void addToFavoredEnemySelection(string name, string bonus_description, BlueprintFeature prototype, params BlueprintComponent[] bonus_components)
        {
            var base_description = favored_enemy_selection.Description + "\n";

            var improved_enemy = library.CopyAndAdd<BlueprintFeature>(prototype.AssetGuid, "Improved" + name, "");
            improved_enemy.SetDescription(base_description + "Bonus: " + bonus_description);

            var favored_enemy = Helpers.CreateFeature(name,
                                                      "Hinterlander " + improved_enemy.Name,
                                                      improved_enemy.Description,
                                                      "",
                                                      improved_enemy.Icon,
                                                      FeatureGroup.FavoriteEnemy,
                                                      bonus_components.AddToArray(Helpers.CreateAddFact(improved_enemy))
                                                     );

            favored_enemy_selection.AllFeatures = favored_enemy_selection.AllFeatures.AddToArray(favored_enemy);
            improved_favored_enemy_selection.AllFeatures = improved_favored_enemy_selection.AllFeatures.AddToArray(improved_enemy);
        }


        static void createFavoredTerrain()
        {
            favored_terrain = library.CopyAndAdd<BlueprintFeature>("c657b9b7ebab46541b5992cfa7a0e1ef", "HinterlanderFavoredTerrain", "");
            favored_terrain.SetDescription("The hinterlander gains a +2 bonus on initiative checks and Lore (Nature), Perception, and Stealth skill checks when he is in plains.\nAt 9th level these bonuses increase to +4.");

            var eagle_splendor = library.Get<BlueprintAbility>("446f7bf201dc1934f96ac0a26e324803");
            var choosen_kin_buff = Helpers.CreateBuff("HinterlanderChoosenKinBuff",
                                                      "Choosen Kin",
                                                      "At 4th level, hinterlander provides half of her favored terrain bonus to all her allies within 30 ft. At 7th level, the range increases to 60 feet.",
                                                      "",
                                                      eagle_splendor.Icon,
                                                      null,
                                                      Helpers.Create<NewMechanics.FavoredTerrainBonus>(f =>
                                                                                                      {
                                                                                                          f.Settings = favored_terrain.GetComponent<FavoredTerrain>().Settings;
                                                                                                          f.Value = Helpers.CreateContextValue(AbilityRankType.StatBonus);
                                                                                                      }
                                                                                                      ),
                                                      Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getHinterlanderArray(),
                                                                                      progression: ContextRankProgression.OnePlusDivStep, stepLevel: 9, type: AbilityRankType.StatBonus)
                                                    );

            int[] ranges = new int[] { 30, 60 };
            BlueprintFeature[] choosen_kin_features = new BlueprintFeature[ranges.Length];

            for (int i = 0; i<ranges.Length; i++)
            {
                var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("7ced0efa297bd5142ab749f6e33b112b", $"HinterlanderChoosenKin{i+1}Area", "");
                area.Size = ranges[i].Feet();
                area.ReplaceComponent<AbilityAreaEffectBuff>(a =>
                                                            {
                                                                a.Buff = choosen_kin_buff;
                                                                a.Condition = Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsAlly>(), Common.createContextConditionIsCaster(not: true));
                                                            }
                                                            );

                var effect_buff = Helpers.CreateBuff($"HinterlanderChoosenKinEffect{i + 1}Buff",
                                                     "",
                                                     "",
                                                     "",
                                                     null,
                                                     null,
                                                     Common.createAddAreaEffect(area)
                                                     );
                effect_buff.SetBuffFlags(BuffFlags.HiddenInUi);

                choosen_kin_features[i] = Helpers.CreateFeature($"HinterlanderChoosenKin{i+1}Feature",
                                                                "",
                                                                "",
                                                                "",
                                                                null,
                                                                FeatureGroup.None,
                                                                Common.createAuraFeatureComponent(effect_buff)
                                                                );
                choosen_kin_features[i].HideInUI = true;

            }


            choosen_kin = Helpers.CreateFeature("HinterlanderChoosenKinFeature",
                                                choosen_kin_buff.Name,
                                                choosen_kin_buff.Description,
                                                "",
                                                choosen_kin_buff.Icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddFeatureOnClassLevel(choosen_kin_features[0], 7, getHinterlanderArray(), new BlueprintArchetype[0], true),
                                                Helpers.CreateAddFeatureOnClassLevel(choosen_kin_features[1], 7, getHinterlanderArray(), new BlueprintArchetype[0])
                                                );
        }


        static void createMasterArcher()
        {
            var point_blank_shot = library.Get<BlueprintFeature>("0da0c194d6e1d43419eb8d990b28e0ab");
            var rapid_shot = library.Get<BlueprintFeature>("9c928dc570bb9e54a9649b3ebfe47a41");
            //deadeye's blessing

            var improved_precise_shot = library.Get<BlueprintFeature>("46f970a6b9b5d2346b10892673fe6e74");
            var manyshot = library.Get<BlueprintFeature>("adf54af2a681792489826f7fd1b62889");
            var pointblank_master = library.Get<BlueprintParametrizedFeature>("05a3b543b0a0a0346a5061e90f293f0b");

            var deadly_aim = library.Get<BlueprintFeature>("f47df34d53f8c904f9981a3ee8e84892");

            master_archer1 = Helpers.CreateFeatureSelection("HinterlanderMasterArcher1Feature",
                                                            "Master Archer",
                                                            "At 1st level, and again at 3rd, 5th and 10th levels, a hinterlander gains a bonus feat from the following list, even if he doesn’t meet the prerequisites: Deadeye’s Blessing, Point-Blank Shot, and Rapid Shot. At 3rd level, he adds Improved Precise Shot, Point Blank Master, and Manyshot to the list. At 5th level, he adds Deadly Aim and Pinpoint Targeting to the list.",
                                                            "",
                                                            null,
                                                            FeatureGroup.None);
            master_archer1.IgnorePrerequisites = true;
            master_archer1.AllFeatures = new BlueprintFeature[] { point_blank_shot, rapid_shot, NewFeats.deadeyes_blessing };

            master_archer2 = library.CopyAndAdd<BlueprintFeatureSelection>(master_archer1.AssetGuid, "HinterlanderMasterArcher2Feature", "");
            master_archer2.AllFeatures = master_archer2.AllFeatures.AddToArray(improved_precise_shot, manyshot, pointblank_master);

            master_archer3 = library.CopyAndAdd<BlueprintFeatureSelection>(master_archer2.AssetGuid, "HinterlanderMasterArcher3Feature", "");
            master_archer3.AllFeatures = master_archer3.AllFeatures.AddToArray(deadly_aim, NewFeats.pinpoint_targeting);
        }


        static void createSpellbookSelection()
        {
            spellbook_selection = Helpers.CreateFeatureSelection("HinterlanderSpellbookSelection",
                                                                 "Hinterlander Spellbook Selection",
                                                                 "Starting from 2nd level, a hinterlander gains new spells per day as if he had also gained a level in an arcane or divine spellcasting class he belonged to before adding the prestige class. He does not, however, gain any other benefit a character of that class would have gained, except for additional spells per day, spells known (if he is a spontaneous spellcaster), and an increased effective level of spellcasting. If a character had more than one spellcasting class before becoming a hinterlandert, he must decide to which class he adds the new level for purposes of determining spells per day.",
                                                                 "",
                                                                 null,
                                                                 FeatureGroup.EldritchKnightSpellbook);
            spellbook_selection.Obligatory = true;
            Common.addSpellbooksToSpellSelection2("Hinterlander", 1, spellbook_selection, alchemist: false);
        }


        static void createImbueArrow()
        {
            var hurricane_bow = library.Get<BlueprintAbility>("3e9d1119d43d07c4c8ba9ebfd1671952");
            var true_strike = library.Get<BlueprintAbility>("2c38da66e5a599347ac95b3294acbe00");
            imbue_arrow = Helpers.CreateFeature("ImbueArrowFeature",
                                                "Imbue Arrow",
                                                "Character gains the ability to place an area spell upon an arrow. When the arrow is fired, the spell’s area is centered where the arrow lands, even if the spell could normally be centered only on the caster. This ability allows the archer to use the bow’s range rather than the spell’s range. A spell cast in this way uses its standard casting time and the arcane archer can fire the arrow as part of the casting. If the arrow misses, the spell is wasted.",
                                                "",
                                                hurricane_bow.Icon,
                                                FeatureGroup.None,
                                                Helpers.Create<SpellManipulationMechanics.FactStoreSpell>(f => f.ignore_target_checkers = true));


            var hit_action = Helpers.CreateActionList(Helpers.Create<SpellManipulationMechanics.ReleaseSpellStoredInSpecifiedBuff>(r => r.fact = imbue_arrow));
            var miss_action = Helpers.CreateActionList(Helpers.Create<SpellManipulationMechanics.ClearSpellStoredInSpecifiedBuff>(r => r.fact = imbue_arrow));

            int max_variants = 6; //due to ui limitation
            Predicate<AbilityData> check_slot_predicate = delegate (AbilityData spell)
            {
                return (spell.Blueprint.AoERadius.Meters > 0.01)
                        && (!spell.Blueprint.HasVariants || spell.Variants.Count < max_variants)
                        && (!spell.RequireMaterialComponent || spell.HasEnoughMaterialComponent);
            };

            for (int i = 0; i < max_variants; i++)
            {
                var imbue_ability = Helpers.CreateAbility($"HinterlanderImbueArrow{i + 1}Ability",
                                                          imbue_arrow.Name,
                                                          imbue_arrow.Description,
                                                          "",
                                                          imbue_arrow.Icon,
                                                          AbilityType.Supernatural,
                                                          CommandType.Standard,
                                                          AbilityRange.Weapon,
                                                          "",
                                                          "",
                                                          Helpers.Create<SpellManipulationMechanics.InferIsFullRoundFromParamSpellSlot>(),
                                                          Helpers.Create<NewMechanics.AttackAnimation >(),
                                                          Helpers.Create<SpellManipulationMechanics.AbilityStoreSpellInFact>(s =>
                                                                                            { s.fact = imbue_arrow;
                                                                                                s.check_slot_predicate = check_slot_predicate;
                                                                                                s.variant = i;
                                                                                                s.actions = Helpers.CreateActionList(Common.createContextActionAttack(hit_action, miss_action));
                                                                                            }),
                                                          Common.createAbilityCasterMainWeaponCheck(WeaponCategory.Longbow, WeaponCategory.Shortbow)
                                                          );
                imbue_ability.setMiscAbilityParametersSingleTargetRangedHarmful(works_on_allies: true);
                imbue_ability.NeedEquipWeapons = true;

                var imbue_ground_ability = Helpers.CreateAbility($"HinterlanderImbueArrowGround{i + 1}Ability",
                                          imbue_arrow.Name + ": Attack Ground",
                                          imbue_arrow.Description,
                                          "",
                                          true_strike.Icon,
                                          AbilityType.Supernatural,
                                          CommandType.Standard,
                                          AbilityRange.Weapon,
                                          "",
                                          "",
                                          Helpers.Create<SpellManipulationMechanics.InferIsFullRoundFromParamSpellSlot>(),
                                          Helpers.Create<NewMechanics.AttackAnimation>(),
                                          Helpers.Create<SpellManipulationMechanics.AbilityStoreSpellInFact>(s =>
                                          {
                                              s.fact = imbue_arrow;
                                              s.check_slot_predicate = check_slot_predicate;
                                              s.variant = i;
                                              s.actions = hit_action;
                                          }),
                                          Common.createAbilityCasterMainWeaponCheck(WeaponCategory.Longbow, WeaponCategory.Shortbow)
                                          );
                imbue_ground_ability.setMiscAbilityParametersRangedDirectional();
                imbue_ground_ability.NeedEquipWeapons = true;

                imbue_arrow.AddComponent(Helpers.CreateAddFacts(imbue_ability, imbue_ground_ability));
            }
        }


    }
}
