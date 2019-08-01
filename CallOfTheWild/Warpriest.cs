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
    class Warpriest
    {
        static LibraryScriptableObject library => Main.library;
        internal static bool test_mode = false;
        static internal BlueprintCharacterClass warpriest_class;
        static internal BlueprintProgression warpriest_progression;

        static internal BlueprintFeature warpriest_fighter_feat_prerequisite_replacement;
        static internal BlueprintFeature warpriest_orisons;
        static internal BlueprintFeature warpriest_proficiencies;

        static internal BlueprintFeature warpriest_sacred_armor;
        static internal BlueprintBuff sacred_armor_enhancement_buff;
        static internal BlueprintFeature warpriest_sacred_armor2;
        static internal BlueprintFeature warpriest_sacred_armor3;
        static internal BlueprintFeature warpriest_sacred_armor4;
        static internal BlueprintFeature warpriest_sacred_armor5;
        static internal BlueprintAbilityResource sacred_armor_resource;

        static internal BlueprintFeature warpriest_sacred_weapon_damage;
        static internal BlueprintFeature warpriest_sacred_weapon_enhancement;
        static internal BlueprintBuff sacred_weapon_enhancement_buff;
        static internal BlueprintFeature warpriest_sacred_weapon_enhancement2;
        static internal BlueprintFeature warpriest_sacred_weapon_enhancement3;
        static internal BlueprintFeature warpriest_sacred_weapon_enhancement4;
        static internal BlueprintFeature warpriest_sacred_weapon_enhancement5;
        static internal BlueprintAbilityResource sacred_weapon_resource;

        static internal BlueprintFeatureSelection warpriest_deity_selection;
        static internal BlueprintFeature warpriest_spontaneous_heal;
        static internal BlueprintFeature warpriest_spontaneous_harm;
        static internal BlueprintFeatureSelection warpriest_energy_selection;
        static internal BlueprintAbilityResource warpriest_fervor_resource;
        static internal BlueprintFeature warpriest_fervor;
        static internal BlueprintFeature fervor_positive;
        static internal BlueprintFeature fervor_negative;
        static internal BlueprintFeature warpriest_channel_energy;
        static internal ActivatableAbilityGroup sacred_weapon_enchancement_group = ActivatableAbilityGroup.TrueMagus;
        static internal ActivatableAbilityGroup sacred_armor_enchancement_group = ActivatableAbilityGroup.ArcaneWeaponProperty;

        static internal BlueprintBuff warpriest_aspect_of_war_buff;
        static internal BlueprintFeature warpriest_aspect_of_war;
        static internal BlueprintAbilityResource warpriest_aspect_of_war_resource;
        static internal BlueprintFeatureSelection warpriest_blessings;
        static internal BlueprintAbilityResource warpriest_blessing_resource;
        static internal BlueprintFeature add_warpriest_blessing_resource;

        static internal ActivatableAbilityGroup confusion_control_group = ActivatableAbilityGroup.DivineWeaponProperty;
        static internal BlueprintBuff warpriest_blessing_special_sancturay_buff;

        internal static void createWarpriestClass()
        {
            Main.logger.Log("Warpriest class test mode: " + test_mode.ToString());
            var cleric_class = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");

            warpriest_class = Helpers.Create<BlueprintCharacterClass>();
            warpriest_class.name = "WarpriestClass";
            library.AddAsset(warpriest_class, "");


            warpriest_class.LocalizedName = Helpers.CreateString("Warpriest.Name", "Warpriest");
            warpriest_class.LocalizedDescription = Helpers.CreateString("Warpriest.Description",
                                                                        "Capable of calling upon the power of the gods in the form of blessings and spells, warpriests blend divine magic with martial skill.They are unflinching bastions of their faith, shouting gospel as they pummel foes into submission, and never shy away from a challenge to their beliefs.While clerics might be subtle and use diplomacy to accomplish their aims, warpriests aren’t above using violence whenever the situation warrants it. In many faiths, warpriests form the core of the church’s martial forces—reclaiming lost relics, rescuing captured clergy, and defending the church’s tenets from all challenges.\n"
                                                                        + "Role: Warpriests can serve as capable healers or spellcasters, calling upon their divine powers from the center of the fight, where their armor and martial skills are put to the test.\n"
                                                                        + "Alignment: A warpriest’s alignment must be within one step of his deity’s, along either the law/ chaos axis or the good/ evil axis."
                                                                        );

            warpriest_class.m_Icon = cleric_class.Icon;
            warpriest_class.SkillPoints = cleric_class.SkillPoints;
            warpriest_class.HitDie = DiceType.D8;
            warpriest_class.BaseAttackBonus = cleric_class.BaseAttackBonus;
            warpriest_class.FortitudeSave = cleric_class.FortitudeSave;
            warpriest_class.ReflexSave = cleric_class.ReflexSave;
            warpriest_class.WillSave = cleric_class.WillSave;
            warpriest_class.Spellbook = createWarpriestSpellbook();
            warpriest_class.ClassSkills = cleric_class.ClassSkills.AddToArray(StatType.SkillAthletics, StatType.SkillLoreNature);
            warpriest_class.IsDivineCaster = true;
            warpriest_class.IsArcaneCaster = false;
            warpriest_class.StartingGold = cleric_class.StartingGold;
            warpriest_class.PrimaryColor = cleric_class.PrimaryColor;
            warpriest_class.SecondaryColor = cleric_class.SecondaryColor;
            warpriest_class.RecommendedAttributes = cleric_class.RecommendedAttributes;
            warpriest_class.NotRecommendedAttributes = cleric_class.NotRecommendedAttributes;
            warpriest_class.EquipmentEntities = cleric_class.EquipmentEntities;
            warpriest_class.MaleEquipmentEntities = cleric_class.MaleEquipmentEntities;
            warpriest_class.FemaleEquipmentEntities = cleric_class.FemaleEquipmentEntities;
            warpriest_class.ComponentsArray = cleric_class.ComponentsArray;
            warpriest_class.StartingItems = cleric_class.StartingItems;

            createWarpriestProgression();
            warpriest_class.Progression = warpriest_progression;
            //createSacredFist();
            //createChampionOfTheFaith();
            //createCultLeader();
            warpriest_class.Archetypes = new BlueprintArchetype[] { }; // { sacred_fist_archetype, champion_of_the_faith_archetype, cult_leader_archetype };
            Helpers.RegisterClass(warpriest_class);

            //addToPrestigeClasses(); //mt
        }

        static BlueprintCharacterClass[] getWarpriestArray()
        {
            return new BlueprintCharacterClass[] { warpriest_class };
        }


        static BlueprintSpellbook createWarpriestSpellbook()
        {
            var cleric_class = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var magus_class = library.Get<BlueprintCharacterClass>("45a4607686d96a1498891b3286121780");
            var warpriest_spellbook = Helpers.Create<BlueprintSpellbook>();
            warpriest_spellbook.Name = warpriest_class.LocalizedName;
            warpriest_spellbook.name = "WarpriestSpellbook";
            library.AddAsset(warpriest_spellbook, "");
            warpriest_spellbook.Name = warpriest_class.LocalizedName;
            warpriest_spellbook.SpellsPerDay = magus_class.Spellbook.SpellsPerDay;
            warpriest_spellbook.SpellsKnown = magus_class.Spellbook.SpellsKnown;
            warpriest_spellbook.Spontaneous = false;
            warpriest_spellbook.IsArcane = false;
            warpriest_spellbook.AllSpellsKnown = true;
            warpriest_spellbook.CanCopyScrolls = false;
            warpriest_spellbook.CastingAttribute = StatType.Wisdom;
            warpriest_spellbook.CharacterClass = warpriest_class;
            warpriest_spellbook.CasterLevelModifier = 0;
            warpriest_spellbook.CantripsType = CantripsType.Orisions;
            warpriest_spellbook.SpellsPerLevel = cleric_class.Spellbook.SpellsPerLevel;
            warpriest_spellbook.SpellList = cleric_class.Spellbook.SpellList;
            return warpriest_spellbook;
        }


        static void createWarpriestProgression()
        {
            createWarpriestProficiencies();
            createWarpriestFighterFeatPrerequisiteReplacement();
            createWarpriestOrisions();
            createDeitySelection();
            createSacredWeaponDamage();
            createSacredWeaponEnhancement();
            createSpontaneousConversion();
            createFervor();
            createChannelEnergy();
            createSacredArmor();
            createAspectOfWar();
            createWarpriestBlessings();
            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");
            var fighter_feat = library.Get<BlueprintFeatureSelection>("41c8486641f7d6d4283ca9dae4147a9f");
            var weapon_focus = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");
            var weapon_focus_selection = Helpers.CreateFeatureSelection("WarpriestWeaponFocusSelection",
                                                                        "Focus Weapon",
                                                                        "At 1st level, a warpriest receives Weapon Focus as a bonus feat (she can choose any weapon, not just his deity’s favored weapon).",
                                                                        "",
                                                                        weapon_focus.Icon,
                                                                        FeatureGroup.None);
            weapon_focus_selection.IgnorePrerequisites = true;
            weapon_focus_selection.AllFeatures = new BlueprintFeature[] { weapon_focus };

            warpriest_progression = Helpers.CreateProgression("WarpriestProgression",
                                                               warpriest_class.Name,
                                                               warpriest_class.Description,
                                                               "",
                                                               warpriest_class.Icon,
                                                               FeatureGroup.None);
            warpriest_progression.Classes = getWarpriestArray();

            warpriest_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, warpriest_proficiencies, detect_magic, warpriest_orisons,
                                                                                        warpriest_fighter_feat_prerequisite_replacement,
                                                                                        warpriest_deity_selection,
                                                                                        warpriest_energy_selection,
                                                                                        weapon_focus_selection,
                                                                                        warpriest_sacred_weapon_damage,
                                                                                        add_warpriest_blessing_resource,
                                                                                        warpriest_blessings,
                                                                                        warpriest_blessings,
                                                                                        library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                        library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")),  // touch calculate feature};
                                                                    Helpers.LevelEntry(2, warpriest_fervor),
                                                                    Helpers.LevelEntry(3, fighter_feat),
                                                                    Helpers.LevelEntry(4, warpriest_channel_energy, warpriest_sacred_weapon_enhancement),
                                                                    Helpers.LevelEntry(5),
                                                                    Helpers.LevelEntry(6, fighter_feat),
                                                                    Helpers.LevelEntry(7, warpriest_sacred_armor),
                                                                    Helpers.LevelEntry(8, warpriest_sacred_weapon_enhancement2),
                                                                    Helpers.LevelEntry(9, fighter_feat),
                                                                    Helpers.LevelEntry(10, warpriest_sacred_armor2),
                                                                    Helpers.LevelEntry(11),
                                                                    Helpers.LevelEntry(12, fighter_feat, warpriest_sacred_weapon_enhancement3),
                                                                    Helpers.LevelEntry(13, warpriest_sacred_armor3),
                                                                    Helpers.LevelEntry(14),
                                                                    Helpers.LevelEntry(15, fighter_feat),
                                                                    Helpers.LevelEntry(16, warpriest_sacred_weapon_enhancement4, warpriest_sacred_armor4),
                                                                    Helpers.LevelEntry(17),
                                                                    Helpers.LevelEntry(18, fighter_feat),
                                                                    Helpers.LevelEntry(19, warpriest_sacred_armor5),
                                                                    Helpers.LevelEntry(20, warpriest_sacred_weapon_enhancement5, warpriest_aspect_of_war)
                                                                    };

            warpriest_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] {warpriest_proficiencies, detect_magic, warpriest_orisons,
                                                                                        warpriest_fighter_feat_prerequisite_replacement,
                                                                                        warpriest_deity_selection, warpriest_blessings, warpriest_blessings};
            warpriest_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(weapon_focus_selection, fighter_feat, fighter_feat, fighter_feat, fighter_feat, fighter_feat, fighter_feat),
                                                         Helpers.CreateUIGroup(warpriest_energy_selection, warpriest_fervor, warpriest_channel_energy, warpriest_aspect_of_war),
                                                         Helpers.CreateUIGroup(warpriest_sacred_weapon_damage, warpriest_sacred_weapon_enhancement, warpriest_sacred_weapon_enhancement2,
                                                                               warpriest_sacred_weapon_enhancement3, warpriest_sacred_weapon_enhancement4, warpriest_sacred_weapon_enhancement5),
                                                         Helpers.CreateUIGroup(warpriest_sacred_armor, warpriest_sacred_armor2, warpriest_sacred_armor3,
                                                                               warpriest_sacred_armor4, warpriest_sacred_armor5)
                                                        };
        }


        static void createWarpriestProficiencies()
        {
            warpriest_proficiencies = library.CopyAndAdd<BlueprintFeature>("b10ff88c03308b649b50c31611c2fefb", "WarpriestProficiencies", "");
            warpriest_proficiencies.SetName("Warpriest Proficiencies");
            warpriest_proficiencies.SetDescription("A warpriest is proficient with all simple and martial weapons, as well as the favored weapon of his deity, and with all armor (heavy, light, and medium) and shields (except tower shields). If the warpriest worships a deity with unarmed strike as its favored weapon, the warpriest gains Improved Unarmed Strike as a bonus feat.");
        }


        static void createWarpriestOrisions()
        {
            warpriest_orisons = library.CopyAndAdd<BlueprintFeature>(
                 "e62f392949c24eb4b8fb2bc9db4345e3", // cleric orisions
                 "WarpriestOrisonsFeature",
                 "");
            warpriest_orisons.SetDescription("Warpriests learn a number of orisons, or 0-level spells. These spells are cast like any other spell, but they do not consume any slots and may be used again.");
            warpriest_orisons.ReplaceComponent<BindAbilitiesToClass>(c => c.CharacterClass = warpriest_class);
        }


        static void createWarpriestFighterFeatPrerequisiteReplacement()
        {
            var fighter_class = library.Get<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");
            var fighter_training = library.Get<BlueprintFeature>("2b636b9e8dd7df94cbd372c52237eebf");
            warpriest_fighter_feat_prerequisite_replacement = Helpers.CreateFeature("WarpriestFighterFeatPrerequisiteReplacement",
                                                                                    "Fighter Training",
                                                                                    "Warpriest treats his warpriest level as his base attack bonus (in addition to base attack bonuses gained from other classes and Hit Dice) for the purpose of qualifying for these feats.\n"
                                                                                    + "Finally, for the purposes of these feats, the warpriest can select feats that have a minimum number of fighter levels as a prerequisite, treating his warpriest level as his fighter level.",
                                                                                    "",
                                                                                    fighter_training.Icon,
                                                                                    FeatureGroup.None,
                                                                                    Common.createClassLevelsForPrerequisites(fighter_class, warpriest_class),
                                                                                    Common.createReplace34BabWithClassLevel(warpriest_class)
                                                                                    );
        }



        static void createDeitySelection()
        {
            warpriest_deity_selection = library.Get<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4");
            
            foreach (var f in warpriest_deity_selection.AllFeatures)
            {
                var f1 = f.GetComponent<ForbidSpellbookOnAlignmentDeviation>();
                if (f1 != null)
                {
                    f1.Spellbooks = f1.Spellbooks.AddToArray(warpriest_class.Spellbook);
                }

                var f2 = f.GetComponent<AddStartingEquipment>();
                if (f2 != null)
                {
                    f2.RestrictedByClass = f2.RestrictedByClass.AddToArray(warpriest_class);
                }


                var f3 = f.GetComponent<AddFeatureOnClassLevel>();
                if (f3 != null)
                {
                    f3.AdditionalClasses = f3.AdditionalClasses.AddToArray(warpriest_class);
                }
            }
        }


        static void createSpontaneousConversion()
        {
            var channel_positive_allowed = library.Get<BlueprintFeature>("8c769102f3996684fb6e09a2c4e7e5b9");
            var channel_negative_allowed = library.Get<BlueprintFeature>("dab5255d809f77c4395afc2b713e9cd6");

            warpriest_spontaneous_heal = library.CopyAndAdd<BlueprintFeature>("5e4620cea099c9345a9207c11d7bc916", "WarpriestSpontaneousHealFeature", "");
            warpriest_spontaneous_heal.SetDescription("A good warpriest (or a neutral warpriest of a good deity) can channel stored spell energy into healing spells that he did not prepare ahead of time. The warpriest can expend any prepared spell that isn’t an orison to cast any cure spell of the same spell level (a cure spell is any spell with \"cure\" in its name).");
            warpriest_spontaneous_heal.ReplaceComponent<SpontaneousSpellConversion>(c => c.CharacterClass = warpriest_class);
            warpriest_spontaneous_heal.AddComponent(Helpers.PrerequisiteFeature(channel_positive_allowed));

            warpriest_spontaneous_harm = library.CopyAndAdd<BlueprintFeature>("5ba6b9cc18acafd45b6293d1e03221ac", "WarpriestSpontaneousHarmFeature", "");
            warpriest_spontaneous_harm.SetDescription("An evil cleric (or a neutral cleric of an evil deity) can channel stored spell energy into wounding spells that she did not prepare ahead of time. The cleric can \"lose\" any prepared spell that is not an orison in order to cast any inflict spell of the same spell level (an inlict spell is any spell with \"inflict\" in its name).");
            warpriest_spontaneous_harm.ReplaceComponent<SpontaneousSpellConversion>(c => c.CharacterClass = warpriest_class);
            warpriest_spontaneous_harm.AddComponent(Helpers.PrerequisiteFeature(channel_negative_allowed));

            warpriest_energy_selection = Helpers.CreateFeatureSelection("WarpriestEnergySelection",
                                                                      "Spontaneous Spell Conversion",
                                                                      "A good warpriest (or a neutral warpriest of a good deity) can channel stored spell energy into healing spells that he did not prepare ahead of time. The warpriest can expend any prepared spell that isn’t an orison to cast any cure spell of the same spell level. A cure spell is any spell with \"cure\" in its name.\n"
                                                                      + "An evil warpriest (or a neutral warpriest of an evil deity) can’t convert spells to cure spells, but can convert them to inflict spells.An inflict spell is any spell with \"inflict\" in its name.\n"
                                                                      + "A warpriest that is neither good nor evil and whose deity is neither good nor evil chooses whether he can convert spells into either cure spells or inflict spells.Once this choice is made, it cannot be changed. This choice also determines whether the warpriest channels positive or negative energy.",
                                                                      "",
                                                                      null,
                                                                      FeatureGroup.None);
            warpriest_energy_selection.AllFeatures = new BlueprintFeature[] { warpriest_spontaneous_heal, warpriest_spontaneous_harm };
        }


        static void createFervor()
        {
            warpriest_fervor_resource = Helpers.CreateAbilityResource("WarpriestFervorResource", "", "", "", null);
            warpriest_fervor_resource.SetIncreasedByLevelStartPlusDivStep(0, 2, 1, 2, 1, 0, 0.0f, getWarpriestArray());
            warpriest_fervor_resource.SetIncreasedByStat(0, StatType.Wisdom);

            var cure_light_wounds = library.Get<BlueprintAbility>("47808d23c67033d4bbab86a1070fd62f");
            var inflict_light_wounds = library.Get<BlueprintAbility>("244a214d3b0188e4eb43d3a72108b67b");
            var construct_type = library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f");
            var undead_type = library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");

            var dice = Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.DamageBonus));
            var heal_action = Common.createContextActionHealTarget(dice);
            var damage_undead_action = Helpers.CreateActionDealDamage(DamageEnergyType.PositiveEnergy, dice);
            var damage_living_action = Helpers.CreateActionDealDamage(DamageEnergyType.NegativeEnergy, dice);

            var context_rank_config = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.StartPlusDivStep,
                                                                      type: AbilityRankType.DamageBonus, classes: getWarpriestArray(), startLevel: 2, stepLevel: 3); 

            var fervor_positive_ability_others = Helpers.CreateAbility("WarpriestFervorPositiveOthersAbility",
                                                                        "Fervor (Postive Energy) Others",
                                                                        "At 2nd level, a warpriest can draw upon the power of his faith to heal wounds or harm foes. This ability can be used a number of times per day equal to 1/2 his warpriest level + his Wisdom modifier. By expending one use of this ability, a good warpriest (or one who worships a good deity) can touch a creature to heal it of 1d6 points of damage, plus an additional 1d6 points of damage for every 3 warpriest levels he possesses above 2nd (to a maximum of 7d6 at 20th level). Using this ability is a standard action (unless the warpriest targets himself, in which case it’s a swift action). Alternatively, the warpriest can use this ability to harm an undead creature, dealing the same amount of damage he would otherwise heal with a melee touch attack. Using fervor in this way is a standard action that provokes an attack of opportunity. Undead do not receive a saving throw against this damage. This counts as positive energy.",
                                                                        "",
                                                                        cure_light_wounds.Icon,
                                                                        AbilityType.Supernatural,
                                                                        CommandType.Standard,
                                                                        AbilityRange.Touch,
                                                                        "",
                                                                        Helpers.savingThrowNone,
                                                                        Helpers.CreateRunActions(Helpers.CreateConditional(Common.createContextConditionHasFact(undead_type),
                                                                                                                           damage_undead_action,
                                                                                                                           heal_action)),
                                                                        cure_light_wounds.GetComponent<SpellDescriptorComponent>(),
                                                                        cure_light_wounds.GetComponent<AbilityDeliverTouch>(),
                                                                        cure_light_wounds.GetComponent<AbilitySpawnFx>(),
                                                                        Helpers.Create<AbilityUseOnRest>(c => c.Type = AbilityUseOnRestType.HealDamage),
                                                                        context_rank_config,
                                                                        Common.createAbilityTargetHasFact(true, construct_type),
                                                                        Helpers.CreateResourceLogic(warpriest_fervor_resource)
                                                                        );
            fervor_positive_ability_others.CanTargetFriends = true;
            fervor_positive_ability_others.CanTargetEnemies = true;
            fervor_positive_ability_others.CanTargetSelf = false;
            fervor_positive_ability_others.CanTargetPoint = false;
            fervor_positive_ability_others.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            fervor_positive_ability_others.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            fervor_positive_ability_others.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Touch;
            fervor_positive_ability_others.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionOmni;

            var fervor_positive_ability_self = library.CopyAndAdd<BlueprintAbility>(fervor_positive_ability_others, "WarpriestFervorPositiveSelfAbility", "");
            fervor_positive_ability_self.SetName("Fervor (Postive Energy) Self");
            fervor_positive_ability_self.Range = AbilityRange.Personal;
            fervor_positive_ability_self.CanTargetSelf = true;
            fervor_positive_ability_self.CanTargetFriends = false;
            fervor_positive_ability_self.CanTargetEnemies = false;
            fervor_positive_ability_self.ActionType = CommandType.Swift;


            fervor_positive = Helpers.CreateFeature("WarpriestFervorPositive",
                                                    "Fervor (Positive Energy)",
                                                    fervor_positive_ability_others.Description,
                                                    "",
                                                    fervor_positive_ability_others.Icon,
                                                    FeatureGroup.None,
                                                    Helpers.CreateAddFacts(fervor_positive_ability_self, fervor_positive_ability_others)
                                                    );


            var fervor_negative_ability_others = Helpers.CreateAbility("WarpriestFervorNegativeOthersAbility",
                                                            "Fervor (Negative Energy) Others",
                                                            "At 2nd level, a warpriest can draw upon the power of his faith to heal wounds or harm foes. This ability can be used a number of times per day equal to 1/2 his warpriest level + his Wisdom modifier. By expending one use of this ability, An evil warpriest (or one who worships an evil deity) can touch a living creature and deal to it 1d6 points of damage, plus an additional 1d6 points of damage for every 3 warpriest levels he possesses above 2nd (to a maximum of 7d6 at 20th level). Using this ability is a standard action (unless the warpriest targets himself, in which case it’s a swift action). Alternatively, the warpriest can use this ability to heal an undead creature for same amount of damage he would otherwise deal with a melee touch attack. Using fervor in this way is a standard action that provokes an attack of opportunity. Living creatures do not receive a saving throw against this damage. This counts as negative energy.",
                                                            "",
                                                            inflict_light_wounds.Icon,
                                                            AbilityType.Supernatural,
                                                            CommandType.Standard,
                                                            AbilityRange.Touch,
                                                            "",
                                                            Helpers.savingThrowNone,
                                                            Helpers.CreateRunActions(Helpers.CreateConditional(Common.createContextConditionHasFact(undead_type),
                                                                                                               heal_action,
                                                                                                               damage_living_action)),
                                                            inflict_light_wounds.GetComponent<AbilityDeliverTouch>(),
                                                            inflict_light_wounds.GetComponent<AbilitySpawnFx>(),
                                                            context_rank_config,
                                                            Helpers.Create<AbilityUseOnRest>(c => c.Type = AbilityUseOnRestType.HealUndead),
                                                            Common.createAbilityTargetHasFact(true, construct_type),
                                                            Helpers.CreateResourceLogic(warpriest_fervor_resource)
                                                            );
            fervor_negative_ability_others.CanTargetFriends = true;
            fervor_negative_ability_others.CanTargetEnemies = true;
            fervor_negative_ability_others.CanTargetSelf = false;
            fervor_negative_ability_others.CanTargetPoint = false;
            fervor_negative_ability_others.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            fervor_negative_ability_others.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            fervor_negative_ability_others.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Touch;
            fervor_negative_ability_others.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionOmni;

            var fervor_negative_ability_self = library.CopyAndAdd<BlueprintAbility>(fervor_negative_ability_others, "WarpriestFervorNegativeSelfAbility", "");
            fervor_negative_ability_self.SetName("Fervor (Negative Energy) Self");
            fervor_negative_ability_self.Range = AbilityRange.Personal;
            fervor_negative_ability_self.CanTargetSelf = true;
            fervor_negative_ability_self.CanTargetFriends = false;
            fervor_negative_ability_self.CanTargetEnemies = false;
            fervor_negative_ability_self.ActionType = CommandType.Swift;


            fervor_negative = Helpers.CreateFeature("WarpriestFervorNegative",
                                                    "Fervor (Negative Energy)",
                                                    fervor_negative_ability_others.Description,
                                                    "",
                                                    fervor_negative_ability_others.Icon,
                                                    FeatureGroup.None,
                                                    Helpers.CreateAddFacts(fervor_negative_ability_self, fervor_negative_ability_others)
                                                    );

            warpriest_fervor = Helpers.CreateFeature("WarpriestFervorFeature",
                                                     "Fervor",
                                                     "At 2nd level, a warpriest can draw upon the power of his faith to heal wounds or harm foes. This ability can be used a number of times per day equal to 1/2 his warpriest level + his Wisdom modifier. By expending one use of this ability, a good warpriest (or one who worships a good deity) can touch a creature to heal it of 1d6 points of damage, plus an additional 1d6 points of damage for every 3 warpriest levels he possesses above 2nd (to a maximum of 7d6 at 20th level). Using this ability is a standard action (unless the warpriest targets himself, in which case it’s a swift action). Alternatively, the warpriest can use this ability to harm an undead creature, dealing the same amount of damage he would otherwise heal with a melee touch attack. Using fervor in this way is a standard action that provokes an attack of opportunity. Undead do not receive a saving throw against this damage. This counts as positive energy.\n"
                                                     + "An evil warpriest(or one who worships an evil deity) can use this ability to instead deal damage to living creatures with a melee touch attack and heal undead creatures with a touch.This counts as negative energy.\n"
                                                     + "A neutral warpriest who worships a neutral deity(or one who is not devoted to a particular deity) uses this ability as a good warpriest if he chose to spontaneously cast cure spells or as an evil warpriest if he chose to spontaneously cast inflict spells.",
                                                     "",
                                                     null,
                                                     FeatureGroup.None,
                                                     Common.createAddFeatureIfHasFact(warpriest_spontaneous_heal, fervor_positive),
                                                     Common.createAddFeatureIfHasFact(warpriest_spontaneous_harm, fervor_negative),
                                                     Helpers.CreateAddAbilityResource(warpriest_fervor_resource)
                                                     );
        }



        static void createChannelEnergy()
        {
            var positive_energy_feature = library.Get<BlueprintFeature>("a79013ff4bcd4864cb669622a29ddafb");
            var negative_energy_feature = library.Get<BlueprintFeature>("3adb2c906e031ee41a01bfc1d5fb7eea");
            var context_rank_config = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.StartPlusDivStep,
                                                                                  type: AbilityRankType.Default, classes: getWarpriestArray(), startLevel: 2, stepLevel: 3);

            var channel_positive_energy = Helpers.CreateFeature("WarpriestChannelPositiveEnergyFeature",
                                                                "Channel Positive Energy",
                                                                "A good warpriest (or a neutral warpriest who worships a good deity) channels positive energy and can choose to deal damage to undead creatures or to heal living creatures.\n"
                                                                + "Channeling energy causes a burst that either heals all living creatures or damages all undead creatures in a 30-foot radius centered on the warpriest. The amount of damage dealt or healed is equal to that of fervor ability. Creatures that take damage from channeled energy receive a Will save to halve the damage. The DC of this save is equal to 10 + 1/2 the warpriest's level + the warpriest's Wisdom modifier. Creatures healed by channel energy cannot exceed their maximum hit point total—all excess healing is lost. Channeling positive energy consumes two uses of fervor ability. This is a standard action that does not provoke an attack of opportunity.",
                                                                "",
                                                                positive_energy_feature.Icon,
                                                                FeatureGroup.None);

            var heal_living = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHeal,
                                                                      "WarpriestChannelEnergyHealLiving",
                                                                      "",
                                                                      channel_positive_energy,
                                                                      context_rank_config,
                                                                      Helpers.CreateResourceLogic(warpriest_fervor_resource, true, 2),
                                                                      true);
            heal_living.SetDescription("Channeling positive energy causes a burst that heals all living creatures in a 30 - foot radius centered on the warpriest. The amount of damage healed is equal to that of fervor ability.");
            var harm_undead = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHarm,
                                                          "WarpriestChannelEnergyHarmUndead",
                                                          "",
                                                          channel_positive_energy,
                                                          context_rank_config,
                                                          Helpers.CreateResourceLogic(warpriest_fervor_resource, true, 2),
                                                          true);
            harm_undead.SetDescription("Channeling energy causes a burst that damages all undead creatures in a 30-foot radius centered on the warpriest. The amount of damage dealt is equal to that of fervor ability. Creatures that take damage from channeled energy receive a Will save to halve the damage. The DC of this save is equal to 10 + 1/2 the warpriest's level + the warpriest's Wisdom modifier.");
            channel_positive_energy.AddComponent(Helpers.CreateAddFacts(heal_living, harm_undead));

            var channel_negative_energy = Helpers.CreateFeature("WarpriestChannelNegativeEnergyFeature",
                                                    "Channel Negative Energy",
                                                    "An evil warpriest (or a neutral cleric who worships an evil deity) channels negative energy and can choose to deal damage to living creatures or to heal undead creatures.\n"
                                                    + "Channeling energy causes a burst that either heals all undead creatures or damages all living creatures in a 30-foot radius centered on the warpriest. The amount of damage dealt or healed is equal to that of fervor ability. Creatures that take damage from channeled energy receive a Will save to halve the damage. The DC of this save is equal to 10 + 1/2 the warpriest's level + the warpriest's Wisdom modifier. Creatures healed by channel energy cannot exceed their maximum hit point total—all excess healing is lost. Channeling positive energy consumes two uses of fervor ability. This is a standard action that does not provoke an attack of opportunity.",
                                                    "",
                                                    negative_energy_feature.Icon,
                                                    FeatureGroup.None);

            var harm_living = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.NegativeHarm,
                                                          "WarpriestChannelEnergyHarmLiving",
                                                          "",
                                                          channel_negative_energy,
                                                          context_rank_config,
                                                          Helpers.CreateResourceLogic(warpriest_fervor_resource, true, 2),
                                                          true);
            harm_living.SetDescription("Channeling energy causes a burst that damages all living creatures in a 30 - foot radius centered on the warpriest. The amount of damage dealt is equal to that of fervor ability. Creatures that take damage from channeled energy receive a Will save to halve the damage.The DC of this save is equal to 10 + 1 / 2 the warpriest's level + the warpriest's Wisdom modifier. Channeling positive energy consumes two uses of fervor ability. This is a standard action that does not provoke an attack of opportunity.");

            var heal_undead = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.NegativeHeal,
                                              "WarpriestChannelEnergyHealUndead",
                                              "",
                                              channel_negative_energy,
                                              context_rank_config,
                                              Helpers.CreateResourceLogic(warpriest_fervor_resource, true, 2),
                                              true);
            heal_undead.SetDescription("Channeling positive energy causes a burst that heals all undead creatures in a 30 - foot radius centered on the warpriest. The amount of damage healed is equal to that of fervor ability.");

            channel_negative_energy.AddComponent(Helpers.CreateAddFacts(harm_living, heal_undead));
            warpriest_channel_energy = Helpers.CreateFeature("WarpriestChannelEnergyFeature",
                                                             "Channel energy",
                                                             "Starting at 4th level, a warpriest can release a wave of energy by channeling the power of his faith through his holy (or unholy) symbol. This energy can be used to deal or heal damage, depending on the type of energy channeled and the creatures targeted. Using this ability is a standard action that expends two uses of his fervor ability and doesn’t provoke an attack of opportunity. The warpriest must present a holy (or unholy) symbol to use this ability. A good warpriest (or one who worships a good deity) channels positive energy and can choose to heal living creatures or to deal damage to undead creatures. An evil warpriest (or one who worships an evil deity) channels negative energy and can choose to deal damage to living creatures or heal undead creatures. A neutral warpriest who worships a neutral deity (or one who is not devoted to a particular deity) channels positive energy if he chose to spontaneously cast cure spells or negative energy if he chose to spontaneously cast inflict spells.\n"
                                                             + "Channeling energy causes a burst that affects all creatures of one type(either undead or living) in a 30 - foot radius centered on the warpriest.The amount of damage dealt or healed is equal to the amount listed in the fervor ability. Creatures that take damage from channeled energy must succeed at a Will saving throw to halve the damage. The save DC is 10 + 1 / 2 the warpriest’s level + the warpriest’s Wisdom modifier.Creatures healed by channeled energy cannot exceed their maximum hit point total—all excess healing is lost.A warpriest can choose whether or not to include himself in this effect.",
                                                             "",
                                                             null,
                                                             FeatureGroup.None,
                                                             Common.createAddFeatureIfHasFact(warpriest_spontaneous_heal, channel_positive_energy),
                                                             Common.createAddFeatureIfHasFact(warpriest_spontaneous_harm, channel_negative_energy)
                                                             );
        } 


        static void createSacredWeaponDamage()
        {
            var bless_weapon = library.Get<BlueprintAbility>("831e942864e924846a30d2e0678e438b");
            var weapon_focus = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");
            DiceFormula[] diceFormulas = new DiceFormula[] {new DiceFormula(1, DiceType.D6),
                                                            new DiceFormula(1, DiceType.D8),
                                                            new DiceFormula(1, DiceType.D10),
                                                            new DiceFormula(2, DiceType.D6),
                                                            new DiceFormula(2, DiceType.D8)};
            warpriest_sacred_weapon_damage = Helpers.CreateFeature("WarpriestSacredWeaponDamage",
                                                                   "Sacred Weapon",
                                                                   "At 1st level, weapons wielded by a warpriest are charged with the power of his faith. In addition to the favored weapon of his deity, the warpriest can designate a weapon as a sacred weapon by selecting that weapon with the Weapon Focus feat; if he has multiple Weapon Focus feats, this ability applies to all of them. Whenever the warpriest hits with his sacred weapon, the weapon damage is based on his level and not the weapon type. The warpriest can decide to use the weapon’s base damage instead of the sacred weapon damage—this must be declared before the attack roll is made. (If the weapon’s base damage exceeds the sacred weapon damage, its damage is unchanged.) This increase in damage does not affect any other aspect of the weapon, and doesn’t apply to alchemical items, bombs, or other weapons that only deal energy damage.\n"
                                                                   + "The damage dealt by medium warpriest with her sacred weapon is 1d6 at levels 1-4, 1d8 at levels 5-9, 1d10 at levels 10 - 14, 2d6 at levels 15-19 and finally 2d8 at level 20.",
                                                                   "",
                                                                   bless_weapon.Icon,
                                                                   FeatureGroup.None,
                                                                   Common.createContextWeaponDamageDiceReplacement(weapon_focus,
                                                                                                                   Helpers.CreateContextValue(AbilityRankType.Default),
                                                                                                                   diceFormulas),
                                                                   Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                                   type: AbilityRankType.Default,
                                                                                                   progression: ContextRankProgression.DivStep,
                                                                                                   stepLevel: 5,
                                                                                                   classes: getWarpriestArray())
                                                                  );
        }


        static void createSacredWeaponEnhancement()
        {
            var weapon_focus = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");
            var divine_weapon = library.Get<BlueprintAbility>("7ff088ab58c69854b82ea95c2b0e35b4");
            var enchants = new BlueprintWeaponEnchantment[] {library.Get<BlueprintWeaponEnchantment>("d704f90f54f813043a525f304f6c0050"),
                                                             library.Get<BlueprintWeaponEnchantment>("9e9bab3020ec5f64499e007880b37e52"),
                                                             library.Get<BlueprintWeaponEnchantment>("d072b841ba0668846adeb007f623bd6c"),
                                                             library.Get<BlueprintWeaponEnchantment>("6a6a0901d799ceb49b33d4851ff72132"),
                                                             library.Get<BlueprintWeaponEnchantment>("746ee366e50611146821d61e391edf16") };

            var enhancement_buff = Helpers.CreateBuff("WarpriestSacredWeaponEnchancementBaseBuff",
                                         "",
                                         "",
                                         "",
                                         null,
                                         null,
                                         Common.createBuffRemainingGroupsSizeEnchantPrimaryHandWeapon(sacred_armor_enchancement_group,
                                                                                                      false, true,
                                                                                                      enchants
                                                                                                      )
                                         );
            sacred_weapon_enhancement_buff = Helpers.CreateBuff("WarpriestSacredWeaponEnchancementSwitchBuff",
                                                                 "Sacred Weapon Enhancement",
                                                                 "At 4th level, the warpriest gains the ability to enhance one of his sacred weapons with divine power as a swift action. This power grants the weapon a +1 enhancement bonus. For every 4 levels beyond 4th, this bonus increases by 1 (to a maximum of +5 at 20th level). The warpriest can use this ability a number of rounds per day equal to his warpriest level, but these rounds need not be consecutive.\n"
                                                                 + "These bonuses stack with any existing bonuses the weapon might have, to a maximum of + 5. The warpriest can enhance a weapon with any of the following weapon special abilities: brilliant energy, disruption, flaming, frost, keen, and shock. In addition, if the warpriest is chaotic, he can add anarchic. If he is evil, he can add unholy. If he is good, he can add holy. If he is lawful, he can add axiomatic. If he is neutral, he can add ghost touch. Adding any of these special abilities replaces an amount of bonus equal to the special ability’s base cost. Duplicate abilities do not stack.",
                                                                 "",
                                                                 divine_weapon.Icon,
                                                                 null,
                                                                 Helpers.CreateAddFactContextActions(activated: Common.createContextActionApplyBuff(enhancement_buff, Helpers.CreateContextDuration(),
                                                                                                                is_child: true, is_permanent: true, dispellable: false)
                                                                                                     )
                                                                 );
            sacred_weapon_enhancement_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var brilliant_energy = createSacredEnhancementFeature("WarpriestSacredWeaponEnchancementBrilliantEnergy",
                                                                        "Sacred Weapon - Brilliant Energy",
                                                                        "A warpriest can add the brilliant energy property to her sacred weapon, but this consumes 4 points of enhancement bonus granted to this weapon.\nA brilliant energy weapon ignores nonliving matter.Armor and shield bonuses to AC(including any enhancement bonuses to that armor) do not count against it because the weapon passes through armor. (Dexterity, deflection, dodge, natural armor, and other such bonuses still apply.) A brilliant energy weapon cannot harm undead, constructs, or objects.",
                                                                        library.Get<BlueprintActivatableAbility>("f1eec5cc68099384cbfc6964049b24fa").Icon,
                                                                        sacred_weapon_enhancement_buff,
                                                                        library.Get<BlueprintWeaponEnchantment>("66e9e299c9002ea4bb65b6f300e43770"),
                                                                        4, sacred_weapon_enchancement_group);

            var flaming = createSacredEnhancementFeature("WarpriestSacredWeaponEnchancementFlaming",
                                                                "Sacred Weapon - Flaming",
                                                                "A warpriest can add the flaming property to her sacred weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA flaming weapon is sheathed in fire that deals an extra 1d6 points of fire damage on a successful hit. The fire does not harm the wielder.",
                                                                library.Get<BlueprintActivatableAbility>("7902941ef70a0dc44bcfc174d6193386").Icon,
                                                                sacred_weapon_enhancement_buff,
                                                                library.Get<BlueprintWeaponEnchantment>("30f90becaaac51f41bf56641966c4121"),
                                                                1, sacred_weapon_enchancement_group);

            var frost = createSacredEnhancementFeature("WarpriestSacredWeaponEnchancementFrost",
                                                            "Sacred Weapon - Frost",
                                                            "A warpriest can add the frost property to her sacred weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA frost weapon is sheathed in a terrible, icy cold that deals an extra 1d6 points of cold damage on a successful hit. The cold does not harm the wielder.",
                                                            library.Get<BlueprintActivatableAbility>("b338e43a8f81a2f43a73a4ae676353a5").Icon,
                                                            sacred_weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("421e54078b7719d40915ce0672511d0b"),
                                                            1, sacred_weapon_enchancement_group);

            var shock = createSacredEnhancementFeature("WarpriestSacredWeaponEnchancementShock",
                                                            "Sacred Weapon - Shock",
                                                            "A warpriest can add the shock property to her sacred weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA shock weapon is sheathed in crackling electricity that deals an extra 1d6 points of electricity damage on a successful hit. The electricity does not harm the wielder.",
                                                            library.Get<BlueprintActivatableAbility>("a3a9e9a2f909cd74e9aee7788a7ec0c6").Icon,
                                                            sacred_weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("7bda5277d36ad114f9f9fd21d0dab658"),
                                                            1, sacred_weapon_enchancement_group);

            var ghost_touch = createSacredEnhancementFeature("WarpriestSacredWeaponEnchancementGhostTouch",
                                                                    "Sacred Weapon - Ghost Touch",
                                                                    "A warpriest can add the ghost touch property to her sacred weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA ghost touch weapon deals damage normally against incorporeal creatures, regardless of its bonus. An incorporeal creature's 50% reduction in damage from corporeal sources does not apply to attacks made against it with ghost touch weapons.",
                                                                    library.Get<BlueprintActivatableAbility>("688d42200cbb2334c8e27191c123d18f").Icon,
                                                                    sacred_weapon_enhancement_buff,
                                                                    library.Get<BlueprintWeaponEnchantment>("47857e1a5a3ec1a46adf6491b1423b4f"),
                                                                    1, sacred_weapon_enchancement_group,
                                                                    AlignmentMaskType.LawfulNeutral | AlignmentMaskType.NeutralEvil | AlignmentMaskType.NeutralGood | AlignmentMaskType.ChaoticNeutral | AlignmentMaskType.TrueNeutral);

            var keen = createSacredEnhancementFeature("WarpriestSacredWeaponEnchancementKeen",
                                                            "Sacred Weapon - Keen",
                                                            "A warpriest can add the keen property to her sacred weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nThe keen property doubles the threat range of a weapon. This benefit doesn't stack with any other effects that expand the threat range of a weapon (such as the keen edge spell or the Improved Critical feat).",
                                                            library.Get<BlueprintActivatableAbility>("27d76f1afda08a64d897cc81201b5218").Icon,
                                                            sacred_weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("102a9c8c9b7a75e4fb5844e79deaf4c0"),
                                                            1, sacred_weapon_enchancement_group);

            var disruption = createSacredEnhancementFeature("WarpriestSacredWeaponEnchancementDisruption",
                                                                    "Sacred Weapon - Disruption",
                                                                    "A warpriest can add the disruption property to her sacred weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nA disruption weapon is the bane of all undead. Any undead creature struck in combat must succeed on a DC 14 Will save or be destroyed. A disruption weapon must be a bludgeoning melee weapon.",
                                                                    library.Get<BlueprintActivatableAbility>("27d76f1afda08a64d897cc81201b5218").Icon,
                                                                    sacred_weapon_enhancement_buff,
                                                                    library.Get<BlueprintWeaponEnchantment>("0f20d79b7049c0f4ca54ca3d1ea44baa"),
                                                                    2, sacred_weapon_enchancement_group);

            var holy = createSacredEnhancementFeature("WarpriestSacredWeaponEnchancementHoly",
                                                            "Sacred Weapon - Holy",
                                                            "A warpriest can add the holy property to her sacred weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nA holy weapon is imbued with holy power. This power makes the weapon good-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of evil alignment.",
                                                            library.Get<BlueprintActivatableAbility>("ce0ece459ebed9941bb096f559f36fa8").Icon,
                                                            sacred_weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("28a9964d81fedae44bae3ca45710c140"),
                                                            2, sacred_weapon_enchancement_group,
                                                            AlignmentMaskType.Good);

            var unholy = createSacredEnhancementFeature("WarpriestSacredWeaponEnchancementUnholy",
                                                            "Sacred Weapon - Unholy",
                                                            "A warpriest can add the unholy property to her sacred weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn unholy weapon is imbued with unholy power. This power makes the weapon evil-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of good alignment.",
                                                            library.Get<BlueprintActivatableAbility>("561803a819460f34ea1fe079edabecce").Icon,
                                                            sacred_weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("d05753b8df780fc4bb55b318f06af453"),
                                                            2, sacred_weapon_enchancement_group,
                                                            AlignmentMaskType.Evil);

            var axiomatic = createSacredEnhancementFeature("WarpriestSacredWeaponEnchancementAxiomatic",
                                                            "Sacred Weapon - Axiomatic",
                                                            "A warpriest can add the axiomatic property to her sacred weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn axiomatic weapon is infused with lawful power. It makes the weapon lawful-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against chaotic creatures.",
                                                            library.Get<BlueprintActivatableAbility>("d76e8a80ab14ac942b6a9b8aaa5860b1").Icon,
                                                            sacred_weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("0ca43051edefcad4b9b2240aa36dc8d4"),
                                                            2, sacred_weapon_enchancement_group,
                                                            AlignmentMaskType.Lawful);

            var anarchic = createSacredEnhancementFeature("WarpriestSacredWeaponEnchancementAnarchic",
                                                "Sacred Weapon - Anarchic",
                                                "A warpriest can add the anarchic property to her sacred weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn anarchic weapon is infused with the power of chaos. It makes the weapon chaotic-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of lawful alignment.",
                                                library.Get<BlueprintActivatableAbility>("8ed07b0cc56223c46953348f849f3309").Icon,
                                                sacred_weapon_enhancement_buff,
                                                library.Get<BlueprintWeaponEnchantment>("57315bc1e1f62a741be0efde688087e9"),
                                                2, sacred_weapon_enchancement_group,
                                                AlignmentMaskType.Chaotic);

            sacred_weapon_resource = Helpers.CreateAbilityResource("WarpriestSacredWeaponResource", "", "", "", null);
            sacred_weapon_resource.SetIncreasedByLevel(0, 1, getWarpriestArray());

            var sacred_weapon_ability = Helpers.CreateActivatableAbility("WarpriestSacredWeaponEnchantmentToggleAbility",
                                                                         sacred_weapon_enhancement_buff.Name,
                                                                         sacred_weapon_enhancement_buff.Description,
                                                                         "",
                                                                         sacred_weapon_enhancement_buff.Icon,
                                                                         sacred_weapon_enhancement_buff,
                                                                         AbilityActivationType.Immediately,
                                                                         CommandType.Swift,
                                                                         null,
                                                                         Helpers.CreateActivatableResourceLogic(sacred_weapon_resource, ResourceSpendType.NewRound),
                                                                         Helpers.Create<NewMechanics.ActivatableAbilityMainWeaponHasParametrizedFeatureRestriction>(c => c.feature = weapon_focus));
            if (!test_mode)
            {
                sacred_weapon_ability.AddComponent(Common.createActivatableAbilityUnitCommand(CommandType.Swift));
            }

            warpriest_sacred_weapon_enhancement = Helpers.CreateFeature("WarpriestSacredWeaponEnchancementFeature",
                                                                        "Sacred Weapon +1",
                                                                        sacred_weapon_ability.Description,
                                                                        "",
                                                                        sacred_weapon_ability.Icon,
                                                                        FeatureGroup.None,
                                                                        Helpers.CreateAddAbilityResource(sacred_weapon_resource),
                                                                        Helpers.CreateAddFacts(sacred_weapon_ability, flaming, frost, shock, ghost_touch, keen)
                                                                        );

            warpriest_sacred_weapon_enhancement2 = Helpers.CreateFeature("WarpriestSacredWeaponEnchancement2Feature",
                                                            "Sacred Weapon +2",
                                                            sacred_weapon_ability.Description,
                                                            "",
                                                            sacred_weapon_ability.Icon,
                                                            FeatureGroup.None,
                                                            Common.createIncreaseActivatableAbilityGroupSize(sacred_weapon_enchancement_group),
                                                            Helpers.CreateAddFacts(disruption, holy, unholy, axiomatic, anarchic)
                                                            );

            warpriest_sacred_weapon_enhancement3 = Helpers.CreateFeature("WarpriestSacredWeaponEnchancement3Feature",
                                                                            "Sacred Weapon +3",
                                                                            sacred_weapon_ability.Description,
                                                                            "",
                                                                            sacred_weapon_ability.Icon,
                                                                            FeatureGroup.None,
                                                                            Common.createIncreaseActivatableAbilityGroupSize(sacred_weapon_enchancement_group)
                                                                            );

            warpriest_sacred_weapon_enhancement4 = Helpers.CreateFeature("WarpriestSacredWeaponEnchancement4Feature",
                                                                            "Sacred Weapon +4",
                                                                            sacred_weapon_ability.Description,
                                                                            "",
                                                                            sacred_weapon_ability.Icon,
                                                                            FeatureGroup.None,
                                                                            Common.createIncreaseActivatableAbilityGroupSize(sacred_weapon_enchancement_group),
                                                                            Helpers.CreateAddFact(brilliant_energy)
                                                                            );

            warpriest_sacred_weapon_enhancement5 = Helpers.CreateFeature("WarpriestSacredWeaponEnchancement5Feature",
                                                                            "Sacred Weapon +5",
                                                                            sacred_weapon_ability.Description,
                                                                            "",
                                                                            sacred_weapon_ability.Icon,
                                                                            FeatureGroup.None,
                                                                            Common.createIncreaseActivatableAbilityGroupSize(sacred_weapon_enchancement_group)
                                                                            );
        }


        static BlueprintActivatableAbility createSacredEnhancementFeature(string name_prefix, string display_name, string description, UnityEngine.Sprite icon, BlueprintBuff sacred_buff,
                                                                           BlueprintItemEnchantment enchantment, int group_size, ActivatableAbilityGroup group,
                                                                           AlignmentMaskType alignment = AlignmentMaskType.Any)
        {
            //create buff
            //create activatable ability that gives buff
            //on main buff in activate add corresponding enchantment
            //create feature that gives activatable ability

            BlueprintBuff buff;

            if (enchantment is BlueprintWeaponEnchantment)
            {
                buff = Helpers.CreateBuff(name_prefix + "Buff",
                                              display_name,
                                              description,
                                              "",
                                              icon,
                                              null,
                                              Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), false, true,
                                                                                               new Kingmaker.Blueprints.Items.Weapons.BlueprintWeaponType[0],
                                                                                               (BlueprintWeaponEnchantment)enchantment)
                                                                                               );
            }
            else
            {
                buff = Helpers.CreateBuff(name_prefix + "Buff",
                                              display_name,
                                              description,
                                              "",
                                              icon,
                                              null,
                                              Common.createBuffContextEnchantArmor(Common.createSimpleContextValue(1), false, true,
                                                                                               (BlueprintArmorEnchantment)enchantment)
                                                                                               );
            }
            buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var switch_buff = Helpers.CreateBuff(name_prefix + "SwitchBuff",
                                  display_name,
                                  description,
                                  "",
                                  icon,
                                  null);
            switch_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(sacred_buff, buff, switch_buff);

            var ability = Helpers.CreateActivatableAbility(name_prefix + "ToggleAbility",
                                                                        display_name,
                                                                        description,
                                                                        "",
                                                                        icon,
                                                                        switch_buff,
                                                                        AbilityActivationType.Immediately,
                                                                        CommandType.Free,
                                                                        null
                                                                        );
            ability.WeightInGroup = group_size;
            ability.Group = group;

            if (alignment != AlignmentMaskType.Any)
            {
                ability.AddComponent(Helpers.Create<NewMechanics.ActivatableAbilityAlignmentRestriction>(c => c.Alignment = alignment));
            }
            return ability;
        }


        static void createSacredArmor()
        {
            var mage_armor = library.Get<BlueprintAbility>("9e1ad5d6f87d19e4d8883d63a6e35568");
            var enchants = ArmorEnchantments.temporary_armor_enchantments;
            var enhancement_buff = Helpers.CreateBuff("WarpriestSacredArmorEnchancementBaseBuff",
                                         "",
                                         "",
                                         "",
                                         null,
                                         null,
                                         Common.createBuffRemainingGroupSizetEnchantArmor(sacred_armor_enchancement_group,
                                                                                                      false, true,
                                                                                                      enchants
                                                                                                      )
                                         );
            enhancement_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            sacred_armor_enhancement_buff = Helpers.CreateBuff("WarpriestSacredArmorEnhancementSwitchBuff",
                                                                 "Sacred Armor",
                                                                 "At 7th level, the warpriest gains the ability to enhance his armor with divine power as a swift action. This power grants the armor a +1 enhancement bonus. For every 3 levels beyond 7th, this bonus increases by 1 (to a maximum of +5 at 19th level). The warpriest can use this ability a number of minutes per day equal to his warpriest level. This duration must be used in 1-minute increments, but they don’t need to be consecutive.\n"
                                                                 + "These bonuses stack with any existing bonuses the armor might have, to a maximum of +5. The warpriest can enhance armor any of the following armor special abilities: energy resistance (normal, improved, and greater), fortification (heavy, light, or moderate), and spell resistance (13, 17, 21, and 25). Adding any of these special abilities replaces an amount of bonus equal to the special ability’s base cost. Duplicate abilities do not stack.",
                                                                 "",
                                                                 mage_armor.Icon,
                                                                 null,
                                                                 Helpers.CreateAddFactContextActions(activated: Common.createContextActionApplyBuff(enhancement_buff, Helpers.CreateContextDuration(),
                                                                                                                is_child: true, dispellable: false, is_permanent: true)
                                                                                                     )
                                                                 );
            //sacred_armor_enhancement_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            //energy resistance - normal (10 +2), improved (20 +4), greater (+5)  - fire, cold, shock, acid = 12 //pick existing
            //fortification - light (25% +1), medium(50% +3), heavy (75% +5) //create
            //spell resistance (+13 +2), (+17 +3), (+21 +4), (+25, +5) //create

            var sr_icon = library.Get<BlueprintAbility>("0a5ddfbcfb3989543ac7c936fc256889").Icon;
            var fortification_icon = library.Get<BlueprintAbility>("c66e86905f7606c4eaa5c774f0357b2b").Icon; //stoneskin
            Dictionary<DamageEnergyType, UnityEngine.Sprite> energy_resistance_icons = new Dictionary<DamageEnergyType, UnityEngine.Sprite>();
            energy_resistance_icons.Add(DamageEnergyType.Fire, library.Get<BlueprintAbility>("ddfb4ac970225f34dbff98a10a4a8844").Icon);
            energy_resistance_icons.Add(DamageEnergyType.Cold, library.Get<BlueprintAbility>("5368cecec375e1845ae07f48cdc09dd1").Icon);
            energy_resistance_icons.Add(DamageEnergyType.Electricity, library.Get<BlueprintAbility>("90987584f54ab7a459c56c2d2f22cee2").Icon);
            energy_resistance_icons.Add(DamageEnergyType.Acid, library.Get<BlueprintAbility>("fedc77de9b7aad54ebcc43b4daf8decd").Icon);

            List<BlueprintActivatableAbility>[] enchant_abilities = new List<BlueprintActivatableAbility>[5];
            
            for (int i = 0; i< enchant_abilities.Length; i++)
            {
                enchant_abilities[i] = new List<BlueprintActivatableAbility>();
            }

            foreach (var e in ArmorEnchantments.spell_resistance_enchantments)
            {
                int cost = e.EnchantmentCost;
                var ability = createSacredEnhancementFeature("WarpriestSacredArmor" + e.name, "Sacred Armor - " + e.Name, e.Description +$"\nThis consumes {cost} point(s) of armor enhancement bonus.", sr_icon, sacred_armor_enhancement_buff, e, cost, sacred_armor_enchancement_group);
                enchant_abilities[cost - 1].Add(ability);
            }


            foreach (var e in ArmorEnchantments.fortification_enchantments)
            {
                int cost = e.EnchantmentCost;
                var ability = createSacredEnhancementFeature("WarpriestSacredArmor" + e.name, "Sacred Armor - " + e.Name, e.Description + $"\nThis consumes {cost} point(s) of armor enhancement bonus.", fortification_icon, sacred_armor_enhancement_buff, e, cost, sacred_armor_enchancement_group);
                enchant_abilities[cost - 1].Add(ability);
            }

            foreach (var item in ArmorEnchantments.energy_resistance_enchantments)
            {
                foreach (var e in item.Value)
                {
                    int cost = e.EnchantmentCost;
                    var ability = createSacredEnhancementFeature("WarpriestSacredArmor" + e.name, "Sacred Armor - " + e.Name, e.Description + $"\nThis consumes {cost} point(s) of armor enhancement bonus.", energy_resistance_icons[item.Key], sacred_armor_enhancement_buff, e, cost, sacred_armor_enchancement_group);
                    enchant_abilities[cost - 1].Add(ability);
                }
            }


            sacred_armor_resource = Helpers.CreateAbilityResource("WarpriestSacredArmorResource", "", "", "", null);
            sacred_armor_resource.SetIncreasedByLevel(0, 1, getWarpriestArray());

            var apply_buff = Common.createContextActionApplyBuff(sacred_armor_enhancement_buff,
                                                                 Helpers.CreateContextDuration(Common.createSimpleContextValue(1), rate: DurationRate.Minutes),
                                                                 dispellable: false);
            var sacred_armor_ability = Helpers.CreateAbility("WarpriestSacredArmorAbility",
                                                                         sacred_armor_enhancement_buff.Name,
                                                                         sacred_armor_enhancement_buff.Description,
                                                                         "",
                                                                         sacred_armor_enhancement_buff.Icon,
                                                                         AbilityType.Supernatural,
                                                                         CommandType.Free,
                                                                         AbilityRange.Personal,
                                                                         "1 minute",
                                                                         Helpers.savingThrowNone,
                                                                         mage_armor.GetComponent<AbilitySpawnFx>(),
                                                                         Helpers.CreateRunActions(apply_buff),
                                                                         Helpers.CreateResourceLogic(sacred_armor_resource)
                                                                         );
                                                                         

            warpriest_sacred_armor = Helpers.CreateFeature("WarpriestSacredArmorFeature",
                                                                        "Sacred Armor +1",
                                                                        sacred_armor_ability.Description,
                                                                        "",
                                                                        sacred_armor_ability.Icon,
                                                                        FeatureGroup.None,
                                                                        Helpers.CreateAddAbilityResource(sacred_armor_resource),
                                                                        Helpers.CreateAddFact(sacred_armor_ability),
                                                                        Helpers.CreateAddFacts(enchant_abilities[0].ToArray())
                                                                        );

            warpriest_sacred_armor2 = Helpers.CreateFeature("WarpriestSacredArmor2Feature",
                                                            "Sacred Armor +2",
                                                            sacred_armor_ability.Description,
                                                            "",
                                                            sacred_armor_ability.Icon,
                                                            FeatureGroup.None,
                                                            Common.createIncreaseActivatableAbilityGroupSize(sacred_armor_enchancement_group),
                                                            Helpers.CreateAddFacts(enchant_abilities[1].ToArray())
                                                            );

            warpriest_sacred_armor3 = Helpers.CreateFeature("WarpriestSacredArmor3Feature",
                                                 "Sacred Armor +3",
                                                 sacred_armor_ability.Description,
                                                 "",
                                                 sacred_armor_ability.Icon,
                                                 FeatureGroup.None,
                                                 Common.createIncreaseActivatableAbilityGroupSize(sacred_armor_enchancement_group),
                                                 Helpers.CreateAddFacts(enchant_abilities[2].ToArray())
                                                 );

            warpriest_sacred_armor4 = Helpers.CreateFeature("WarpriestSacredArmor4Feature",
                                                "Sacred Armor +4",
                                                sacred_armor_ability.Description,
                                                "",
                                                sacred_armor_ability.Icon,
                                                FeatureGroup.None,
                                                Common.createIncreaseActivatableAbilityGroupSize(sacred_armor_enchancement_group),
                                                Helpers.CreateAddFacts(enchant_abilities[3].ToArray())
                                                );

            warpriest_sacred_armor5 = Helpers.CreateFeature("WarpriestSacredArmor5Feature",
                                                "Sacred Armor +5",
                                                sacred_armor_ability.Description,
                                                "",
                                                sacred_armor_ability.Icon,
                                                FeatureGroup.None,
                                                Common.createIncreaseActivatableAbilityGroupSize(sacred_armor_enchancement_group),
                                                Helpers.CreateAddFacts(enchant_abilities[4].ToArray())
                                                );
        }


        static void createAspectOfWar()
        {
            var rage = library.Get<BlueprintAbility>("97b991256e43bb140b263c326f690ce2");
            warpriest_aspect_of_war_resource = Helpers.CreateAbilityResource("WarpriestAspectOfWarResource", "", "", "", null);
            warpriest_aspect_of_war_resource.SetFixedResource(1);
            var remove_armor_speed_penalty_feature = Helpers.CreateFeature("RemoveArmorSpeedPenaltyFeature",
                                                                           "Remove Armor Penalty",
                                                                           "",
                                                                           "",
                                                                           null,
                                                                           FeatureGroup.None,
                                                                           Helpers.Create<ArmorSpeedPenaltyRemoval>()
                                                                           );
            remove_armor_speed_penalty_feature.Ranks = 2;
            //remove_armor_speed_penalty_feature.HideInCharacterSheetAndLevelUp = true;

            var transformation = library.Get<BlueprintAbility>("27203d62eb3d4184c9aced94f22e1806");
            var transformation_buff = library.Get<BlueprintBuff>("287682389d2011b41b5a65195d9cbc84");
            warpriest_aspect_of_war_buff = Helpers.CreateBuff("WarpriestAspectOfWarBuff",
                                          "Aspect of War",
                                          "At 20th level, the warpriest can channel an aspect of war, growing in power and martial ability. Once per day as a swift action, a warpriest can treat his level as his base attack bonus, gains DR 10/—, and can move at his full speed regardless of the armor he is wearing. In addition, the blessings he calls upon don’t count against his daily limit during this time. This ability lasts for 1 minute.",
                                          "",
                                          rage.Icon,
                                          transformation_buff.FxOnStart,
                                          Common.createPhysicalDR(10),
                                          Helpers.Create<RaiseBAB>(c => c.TargetValue = Common.createSimpleContextValue(20)),
                                          Helpers.CreateAddFact(remove_armor_speed_penalty_feature),
                                          Helpers.CreateAddFact(remove_armor_speed_penalty_feature)
                                          );

            var apply_buff = Common.createContextActionApplyBuff(warpriest_aspect_of_war_buff,
                                                                 Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes),
                                                                 true, false, false, false);

            var warpriest_aspect_of_war_ability = Helpers.CreateAbility("WarpriestAspectOfWarFeature",
                                                                    warpriest_aspect_of_war_buff.Name,
                                                                    warpriest_aspect_of_war_buff.Description,
                                                                    "",
                                                                    warpriest_aspect_of_war_buff.Icon,
                                                                    AbilityType.Supernatural,
                                                                    CommandType.Swift,
                                                                    AbilityRange.Personal,
                                                                    "1 minute",
                                                                    Helpers.savingThrowNone,
                                                                    Helpers.CreateRunActions(apply_buff),
                                                                    transformation.GetComponent<AbilitySpawnFx>(),
                                                                    Helpers.CreateResourceLogic(warpriest_aspect_of_war_resource)
                                                                    );
            warpriest_aspect_of_war_ability.CanTargetSelf = true;
            warpriest_aspect_of_war_ability.Animation = transformation.Animation;
            warpriest_aspect_of_war_ability.AnimationStyle = transformation.AnimationStyle;

            warpriest_aspect_of_war = Common.AbilityToFeature(warpriest_aspect_of_war_ability, false);
            warpriest_aspect_of_war.AddComponent(Helpers.CreateAddAbilityResource(warpriest_aspect_of_war_resource));

        }


        static void createWarpriestBlessings()
        {
            warpriest_blessings = Helpers.CreateFeatureSelection("WarpriestBlessingsSelection",
                                                                 "Blessing",
                                                                 "A warpriest can select any two blessings granted by his deity. Deities grant blessings of the same name as the domains they grant.\n"
                                                                 + "Each blessing grants a minor power at 1st level and a major power at 10th level. A warpriest can call upon the power of his blessings a number of times per day (in any combination) equal to 3 + 1 / 2 his warpriest level (to a maximum of 13 times per day at 20th level). Each time he calls upon any one of his blessings, it counts against his daily limit.The save DC for these blessings is equal to 10 + 1 / 2 the warpriest’s level + the warpriest’s Wisdom modifier.",
                                                                 "",
                                                                 null,
                                                                 FeatureGroup.None);
            warpriest_blessing_resource = Helpers.CreateAbilityResource("WarpriestBlessingsResource", "", "", "", null);
            warpriest_blessing_resource.SetIncreasedByLevelStartPlusDivStep(1, 2, 0, 2, 1, 0, 0.0f, getWarpriestArray());
            warpriest_blessing_resource.SetIncreasedByStat(1, StatType.Wisdom);
            add_warpriest_blessing_resource = Helpers.CreateFeature("AddWarpriestBlessingsResource",
                                                                    "",
                                                                    "",
                                                                    "",
                                                                    null,
                                                                    FeatureGroup.None,
                                                                    Helpers.CreateAddAbilityResource(warpriest_blessing_resource)
                                                                    );
            add_warpriest_blessing_resource.HideInCharacterSheetAndLevelUp = true;
            add_warpriest_blessing_resource.HideInUI = true;

            createAirBlessing();
            createAnimalBlessing();
            createArtificeBlessing();
            createChaosBlessing();
            createCharmBlessing();
            createCommunityBlessing();
            createDarknessBlessing();
            createCreateDeathBlessing();
            createDestructionBlessing();
            createEarthBlessing();
            createEvilBlessing();
            createFireBlessing();
            createGloryBlessing();
            createGoodBlessing();
            createHealingBlessing();
            createKnowledgeBlessing();
            createLawBlessing();
            createLiberationBlessing();
            createLuckBlessing();
            createMadnessBlessing();
        }


        static void addBlessing(string name_prefix, string Name, BlueprintAbility minor_blessing, BlueprintAbility major_blessing, string allowed_key)
        {
            addBlessing(name_prefix, Name, Common.AbilityToFeature(minor_blessing, false), Common.AbilityToFeature(major_blessing, false), allowed_key);
        }


        static void addBlessing(string name_prefix, string Name, BlueprintFeature minor_blessing, BlueprintFeature major_blessing, string allowed_key)
        {
            var allowed_blessings = Helpers.CreateFeature(name_prefix + "AllowedFeature",
                                                          "",
                                                          "",
                                                          "",
                                                          null,
                                                          FeatureGroup.None);
            allowed_blessings.HideInCharacterSheetAndLevelUp = true;

            foreach (var a in warpriest_deity_selection.AllFeatures)
            {
                var add_facts = a.GetComponent<AddFacts>();
                if (add_facts == null)
                {
                    continue;
                }
                var facts = add_facts.Facts;
                foreach (var f in facts)
                {
                    if (f.AssetGuid == allowed_key)
                    {
                        add_facts.Facts = add_facts.Facts.AddToArray(allowed_blessings);
                    }
                }
            }

            var progression = Helpers.CreateProgression(name_prefix + "Progression",
                                                        Name,
                                                        minor_blessing.Name + " (minor): " + minor_blessing.Description + "\n"
                                                        + major_blessing.Name + " (major): " + major_blessing.Description,
                                                        "",
                                                        null,
                                                        FeatureGroup.Domain,
                                                        Helpers.PrerequisiteFeature(allowed_blessings));
            progression.Classes = getWarpriestArray();
            progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, minor_blessing),
                                                         Helpers.LevelEntry(10, major_blessing)
                                                        };
            progression.UIGroups = new UIGroup[] { Helpers.CreateUIGroup(minor_blessing, major_blessing) };

            warpriest_blessings.AllFeatures = warpriest_blessings.AllFeatures.AddToArray(progression);
        }


        static void addBlessingResourceLogic(BlueprintAbility blessing, int amount = 1)
        {
            blessing.AddComponent(Helpers.CreateResourceLogic(warpriest_blessing_resource, amount: 1, cost_is_custom: true));
            blessing.AddComponent(Helpers.Create<NewMechanics.ResourseCostCalculatorWithDecreasincFacts>(r => r.cost_reducing_facts = new BlueprintFact[] { warpriest_aspect_of_war_buff }));
        }


        static void createAirBlessing()
        {
            var hurricane_bow = library.Get<BlueprintAbility>("3e9d1119d43d07c4c8ba9ebfd1671952");
            var hurricane_bow_buff = library.Get<BlueprintBuff>("002c51d933574824c8ef2b04c9d09ff5");

            var minor_buff = Helpers.CreateBuff("WarpriestAirBlessingMinorBuff",
                                                "Zephyr's Gift",
                                                "At 1st level, you can touch any one ranged weapon and enhance it with the quality of air. For 1 minute, any attacks made with the weapon receive + 1 bonus on attack and damage rolls. In addition, making a ranged attack with this weapon doesn’t provoke an attack of opportunity.",
                                                "",
                                                hurricane_bow.Icon,
                                                hurricane_bow_buff.FxOnStart,
                                                Common.createWeaponGroupAttackBonus(1, ModifierDescriptor.UntypedStackable, Kingmaker.Blueprints.Items.Weapons.WeaponFighterGroup.Bows),
                                                Common.createWeaponGroupAttackBonus(1, ModifierDescriptor.UntypedStackable, Kingmaker.Blueprints.Items.Weapons.WeaponFighterGroup.Crossbows),
                                                Common.createWeaponGroupAttackBonus(1, ModifierDescriptor.UntypedStackable, Kingmaker.Blueprints.Items.Weapons.WeaponFighterGroup.Thrown),
                                                Helpers.Create<NewMechanics.SpecifiedWeaponCategoryIgnoreAoo>(s => s.WeaponGroups = new WeaponFighterGroup[] { WeaponFighterGroup.Bows,
                                                                                                                                                               WeaponFighterGroup.Crossbows,
                                                                                                                                                               WeaponFighterGroup.Thrown})
                                                );

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestAirBlessingMinorAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      Helpers.savingThrowNone,
                                                      hurricane_bow.GetComponent<AbilitySpawnFx>(),
                                                      Helpers.CreateRunActions(apply_minor_buff),
                                                      Helpers.Create<NewMechanics.MonsterLore.AbilityTargetInspected>()
                                                      );

            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic(minor_ability);

            var charge_buff = library.Get<BlueprintBuff>("f36da144a379d534cad8e21667079066");
            var wings_demon = library.Get<BlueprintBuff>("3c958be25ab34dc448569331488bee27");
            var wings_angel = library.Get<BlueprintBuff>("d596694ff285f3f429528547f441b1c0");


            var damage_action = Helpers.CreateActionDealDamage(DamageEnergyType.Electricity, Helpers.CreateContextDiceValue(DiceType.Zero, bonus: Helpers.CreateContextValue(AbilityRankType.DamageBonus)));

            var major_buff = Helpers.CreateBuff("WarpriestAirBlessingMajorBuff",
                                                "Soaring Assault",
                                                "At 10th level, you can touch an ally and give her the gift of flight for 1 minute. The ally gains immunity to ground based abilities and difficult terrain, as well as receives +3 dodge bonus against melee attacks. Whenever the ally succeeds at a charge attack, that attack deals an amount of additional electricity damage equal to your level.",
                                                "",
                                                wings_angel.Icon,
                                                null,
                                                Helpers.Create<NewMechanics.AddInitiatorAttackWithWeaponTriggerOnCharge>(a => a.Action = Helpers.CreateActionList(damage_action)),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getWarpriestArray(),
                                                                                type: AbilityRankType.DamageBonus, progression: ContextRankProgression.AsIs)
                                                );

            var apply_major_buff = Common.createContextActionApplyBuff(major_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var apply_wings_angel = Common.createContextActionApplyBuff(wings_angel, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var apply_wings_demon = Common.createContextActionApplyBuff(wings_demon, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var major_ability = Helpers.CreateAbility("WarpriestAirBlessingMajorAbility",
                                                      major_buff.Name,
                                                      major_buff.Description,
                                                      "",
                                                      major_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      Helpers.savingThrowNone,
                                                      Helpers.CreateRunActions(apply_major_buff, Helpers.CreateConditional(Helpers.CreateConditionCasterHasFact(warpriest_spontaneous_heal), 
                                                                                                                           apply_wings_angel,
                                                                                                                           apply_wings_demon
                                                                                                                           )
                                                                              )
                                                      );

            major_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic(major_ability);

            addBlessing("WarpriestBlessingAir", "Air", minor_ability, major_ability, "6e5f4ff5a7010754ca78708ce1a9b233");
        }


        static void createAnimalBlessing()
        {
            var minor_buff = library.CopyAndAdd<BlueprintBuff>("a67b51a8074ae47438280be0a87b01b6", "WarpriestAnimalMinorBuff", ""); //animal fury
            minor_buff.SetBuffFlags(0);
            var animal_fury = library.Get<BlueprintFeature>("25954b1652bebc2409f9cb9d5728bceb");
            var magic_fang = library.Get<BlueprintAbility>("403cf599412299a4f9d5d925c7b9fb33");
            minor_buff.AddComponent(animal_fury.GetComponent<AddCalculatedWeapon>());
            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestAnimalBlessingMinorAbility",
                                                      "Animal Fury",
                                                      "At 1st level, you can touch one ally and grant it feral features. The ally gains 1 secondary bite attack that deals 1d8 points of damage if the ally is Medium or 1d6 if it’s Small. This effect lasts for 1 minute.",
                                                      "",
                                                      animal_fury.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      Helpers.savingThrowNone,
                                                      magic_fang.GetComponent<AbilitySpawnFx>(),
                                                      Helpers.CreateRunActions(apply_minor_buff)
                                                      );

            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic(minor_ability);

            string[] nature_ally_guids = new string[] {"28ea1b2e0c4a9094da208b4c186f5e4f", "060afb9e13d8a3547ad0dd20c407c0a5",
                                                      "6d8d59aa38713be4fa3be76c19107cc0", "8d3d5b62878d5b24391c1d7834d0d706", "f6751c3b22dbd884093e350a37420368" };

            var summon_na8 = library.Get<BlueprintAbility>("8d3d5b62878d5b24391c1d7834d0d706");

            List<ActionList> summon_actions = new List<ActionList>();
            foreach (var s in nature_ally_guids)
            {
                summon_actions.Add(library.Get<BlueprintAbility>(s).GetComponent<AbilityEffectRunAction>().Actions);
            }

            var major_ability = library.CopyAndAdd<BlueprintAbility>(summon_na8.AssetGuid, "WarpirestAnimalBlessingMajorAbility", "");
            major_ability.SetName("Battle Companion");
            major_ability.SetDescription("At 10th level, you can summon a battle companion. This ability functions as summon nature’s ally V with a duration of 1 minute. This ability can summon only one creature, regardless of the list used. For every 2 levels beyond 10th, the level of the summon nature’s ally spell increases by 1 (to a maximum of summon nature’s ally IX at 18th level).");
            major_ability.RemoveComponents<SpellComponent>();
            major_ability.Type = AbilityType.Supernatural;
            var action = Helpers.CreateRunActions(Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus), summon_actions.ToArray()));
            major_ability.ReplaceComponent<AbilityEffectRunAction>(action);
            major_ability.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(min: 10, max: 10));
            major_ability.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getWarpriestArray(),
                                                                       progression: ContextRankProgression.DelayedStartPlusDivStep,
                                                                       startLevel: 10, stepLevel: 2, max: 5, type: AbilityRankType.StatBonus
                                                                      )
                                      );
            major_ability.LocalizedDuration = Helpers.oneMinuteDuration;
            major_ability.Parent = null;
            addBlessingResourceLogic(major_ability);
            addBlessing("WarpriestBlessingAnimal", "Animal", minor_ability, major_ability, "9f05f9da2ea5ae44eac47d407a0000e5");
        }


        static void createArtificeBlessing()
        {
            var resounding_blow_buff = library.Get<BlueprintBuff>("06173a778d7067a439acffe9004916e9");
            var construct_type = library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f");

            var enchant = Common.createWeaponEnchantment("WarpriestArtificeBlessingMinorEnchant",
                                                         "Crafter’s Wrath",
                                                         "Whenever this weapon deals damage to constructs, it bypasses damage reduction.",
                                                         "",
                                                         "",
                                                         "",
                                                         0,
                                                         null,
                                                         Helpers.Create<NewMechanics.WeaponIgnoreDRIfTargetHasFact>(c => c.fact = construct_type)
                                                         );

            var minor_buff = Helpers.CreateBuff("WarpriestArtificeBlessingMinorBuff",
                                                enchant.Name,
                                                "At 1st level, you can touch one melee weapon and grant it greater power to harm and destroy crafted objects. For 1 minute, whenever this weapon deals damage to constructs, it bypasses damage reduction.",
                                                "",
                                                resounding_blow_buff.Icon,
                                                resounding_blow_buff.FxOnStart,
                                                Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), false, true, new BlueprintWeaponType[0], enchant)
                                                );

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestArtificeBlessingMinorAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      Helpers.savingThrowNone,
                                                      Helpers.CreateRunActions(apply_minor_buff)
                                                      );

            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic(minor_ability);

            var spell_combat = library.Get<BlueprintFeature>("2464ba53317c7fc4d88f383fac2b45f9");
            var major_feature = Helpers.CreateFeature("WarpriestArtificeBlessingMajorFeature",
                                                "Spell Storing",
                                                "At 10th level, you can cast a single target non-personal spell of 3rd level or lower into a weapon that will be released on target upon sucessful attack. If the stored spell is not used within 1 minute, it dissipates.",
                                                "",
                                                spell_combat.Icon,
                                                FeatureGroup.None,
                                                Helpers.Create<SpellManipulationMechanics.FactStoreSpell>());

            var release_action = Helpers.Create<SpellManipulationMechanics.ReleaseSpellStoredInSpecifiedBuff>(r => r.fact = major_feature);
            major_feature.AddComponent(Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(release_action)));
            int max_variants = 6; //due to ui limitation
            Predicate<AbilityData> check_slot_predicate = delegate (AbilityData spell) {
                                                                                        return spell.SpellLevel <= 3
                                                                                                && spell.Blueprint.Range != AbilityRange.Personal
                                                                                                && spell.Blueprint.CanTargetEnemies
                                                                                                && !spell.Blueprint.IsFullRoundAction
                                                                                                && (!spell.Blueprint.HasVariants || spell.Variants.Count < max_variants)
                                                                                                && !spell.Blueprint.HasAreaEffect();
                                                                                       };
           
            for (int i = 0; i < max_variants; i++)
            {
                var major_ability = Helpers.CreateAbility($"WarpriestArtificeBlessingMajor{i+1}Ability",
                                                          $"{ major_feature.Name} {Common.roman_id[i+1]}",
                                                          major_feature.Description,
                                                          "",
                                                          major_feature.Icon,
                                                          AbilityType.Supernatural,
                                                          CommandType.Standard,
                                                          AbilityRange.Personal,
                                                          "",
                                                          "",
                                                          Helpers.Create<SpellManipulationMechanics.AbilityStoreSpellInFact>(s => { s.fact = major_feature; s.check_slot_predicate = check_slot_predicate; s.variant = i;})
                                                          );
                major_ability.setMiscAbilityParametersSelfOnly();
                addBlessingResourceLogic(major_ability);
                major_feature.AddComponent(Helpers.CreateAddFact(major_ability));
            }
            
            addBlessing("WarpriestBlessingArtifice", "Artifice", Common.AbilityToFeature(minor_ability, false), major_feature, "9656b1c7214180f4b9a6ab56f83b92fb");
        }


        static void createChaosBlessing()
        {
            var sneak_attack = library.Get<BlueprintFeature>("df4f34f7cac73ab40986bc33f87b1a3c");
            var bless_weapon = library.Get<BlueprintAbility>("831e942864e924846a30d2e0678e438b");
            var anarchic_enchantment = library.Get<BlueprintWeaponEnchantment>("57315bc1e1f62a741be0efde688087e9");

            var enchantment = Common.createWeaponEnchantment("WarpriestChaosMinorBlessingWeaponEcnchantment",
                                                             "Anarchic Strike",
                                                             "This weapon glows yellow or purple and deals an additional 1d6 points of damage against lawful creatures. It is also treated as chaotic for the purposes of overcoming damage reduction.",
                                                             "",
                                                             "",
                                                             "",
                                                             0,
                                                             anarchic_enchantment.WeaponFxPrefab,
                                                             Common.createWeaponDamageAgainstAlignment(DamageEnergyType.Unholy, DamageAlignment.Chaotic, AlignmentComponent.Lawful,
                                                                                                       Helpers.CreateContextDiceValue(DiceType.D6, Common.createSimpleContextValue(1)))
                                                             );

            var minor_buff = Helpers.CreateBuff("WarpriestChaosMinorBuff",
                                                enchantment.Name,
                                                "At 1st level, you can touch one weapon and grant it a chaotic blessing. For 1 minute, this weapon glows yellow or purple and deals an additional 1d6 points of damage against lawful creatures. During this time, it’s treated as chaotic for the purposes of overcoming damage reduction.",
                                                "",
                                                sneak_attack.Icon,
                                                null,
                                                Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), false, true, enchantment));

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestChaosBlessingMinorAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      Helpers.savingThrowNone,
                                                      bless_weapon.GetComponent<AbilitySpawnFx>(),
                                                      Helpers.CreateRunActions(apply_minor_buff)
                                                      );

            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic(minor_ability);

            string[] summon_monster_guids = new string[] {"efa433a38e9c7c14bb4e780f8a3fe559", "0964bf88b582bed41b74e79596c4f6d9", "02de4dd8add69aa42a3d1330b573e2ab",
                                                      "2920d48574933c24391fbb9e18f87bf5", "eb6df7ddfc0669d4fb3fc9af4bd34bca", "e96593e67d206ab49ad1b567327d1e75" };

            var summon_m9 = library.Get<BlueprintAbility>("e96593e67d206ab49ad1b567327d1e75");

            List<ActionList> summon_actions = new List<ActionList>();
            foreach (var s in summon_monster_guids)
            {
                summon_actions.Add(library.Get<BlueprintAbility>(s).GetComponent<AbilityEffectRunAction>().Actions);
            }

            var major_ability = library.CopyAndAdd<BlueprintAbility>(summon_m9.AssetGuid, "WarpirestChaosBlessingMajorAbility", "");
            major_ability.SetName("Battle Companion");
            major_ability.SetDescription("At 10th level, you can summon a battle companion. This ability functions as summon monster IV with a duration of 1 minute. This ability can summon only one creature, regardless of the list used. For every 2 levels beyond 10th, the level of the summon monster spell increases by 1 (to a maximum of summon monster IX at 20th level).");
            major_ability.RemoveComponents<SpellComponent>();
            major_ability.Type = AbilityType.Supernatural;
            var action = Helpers.CreateRunActions(Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus), summon_actions.ToArray()));
            major_ability.ReplaceComponent<AbilityEffectRunAction>(action);
            major_ability.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(min: 10, max: 10));
            major_ability.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getWarpriestArray(),
                                                                       progression: ContextRankProgression.DelayedStartPlusDivStep,
                                                                       startLevel: 10, stepLevel: 2, max: 6, type: AbilityRankType.StatBonus
                                                                      )
                                      );
            major_ability.LocalizedDuration = Helpers.oneMinuteDuration;
            major_ability.Parent = null;
            addBlessingResourceLogic(major_ability);
            addBlessing("WarpriestBlessingChaos", "Chaos", minor_ability, major_ability, "8c7d778bc39fec642befc1435b00f613");
        }


        static void createCharmBlessing()
        {
            var sancturay_logic = Helpers.Create<NewMechanics.Sanctuary>(c =>
            {
                c.save_type = SavingThrowType.Will;
                c.offensive_action_effect = NewMechanics.Sanctuary.OffensiveActionEffect.REMOVE_FROM_TARGET;
            }                                                   );
            warpriest_blessing_special_sancturay_buff = library.CopyAndAdd<BlueprintBuff>("525f980cb29bc2240b93e953974cb325", "WarpriestSpecialSanctuaryBuff", "");//invisibility
            warpriest_blessing_special_sancturay_buff.ComponentsArray = new BlueprintComponent[] { sancturay_logic, Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting) };

            var lesser_restoration = library.Get<BlueprintAbility>("e84fc922ccf952943b5240293669b171");
            var apply_minor_buff = Common.createContextActionApplyBuff(warpriest_blessing_special_sancturay_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestCharmMinorBlessingAbility",
                                                      "Charming Presence",
                                                      "At 1st level, you can touch an ally and grant an entrancing blessing. For 1 minute, the ally becomes mesmerizing to her opponents, filling them with either abject admiration or paralyzing fear. This effect functions as sanctuary, except if the ally attacks an opponent, the effect ends with respect to only that opponent. This is a mind-affecting effect.",
                                                      "",
                                                      lesser_restoration.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      Helpers.willNegates,
                                                      Helpers.CreateRunActions(apply_minor_buff));
            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic(minor_ability);


            var swift_command = library.CopyAndAdd<BlueprintAbility>(NewSpells.command.AssetGuid, "WarpriestCharmDomainCommandAbility", "");
            swift_command.ActionType = CommandType.Swift;
            swift_command.Type = AbilityType.SpellLike;
            swift_command.RemoveComponents<SpellComponent>();
            var dc_replace = Common.createContextCalculateAbilityParamsBasedOnClass(warpriest_class, StatType.Wisdom);
            swift_command.ReplaceComponent<AbilityVariants>(a => a.Variants = Common.CreateAbilityVariantsReplace(swift_command, "WarpriestCharmDomain", 
                                                                                                                  v =>
                                                                                                                  { v.Type = swift_command.Type;
                                                                                                                    v.ActionType = swift_command.ActionType;
                                                                                                                    v.RemoveComponents<SpellComponent>();
                                                                                                                    v.AddComponent(dc_replace);
                                                                                                                  },
                                                                                                                  a.Variants));
            var clock_of_dreams_buff = library.Get<BlueprintBuff>("2e4b85213927f0a4ea2198e0f2a6028b");
            var major_buff = Helpers.CreateBuff("WarpriestCharmMajorBlessingBuff",
                                                "Dominance Aura",
                                                "At 10th level, you can surround yourself with a tangible aura of majesty for 1 minute. While this aura is active, once per round as a swift action you can issue a command (as the command spell) to one creature within 30 feet; the creature must succeed at a Will saving throw or submit for 1 round.",
                                                "",
                                                swift_command.Icon,
                                                clock_of_dreams_buff.FxOnStart,
                                                Helpers.CreateAddFact(swift_command)
                                                );
            var apply_major_buff = Common.createContextActionApplyBuff(major_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var major_ability = Helpers.CreateAbility("WarpriestCharmMajorBlessingAbility",
                                                      major_buff.Name,
                                                      major_buff.Description,
                                                      "",
                                                      major_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Personal,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_major_buff));
            major_ability.setMiscAbilityParametersSelfOnly();
            addBlessingResourceLogic(major_ability);
            addBlessing("WarpriestBlessingCharm", "Charm", minor_ability, major_ability, "f1ceba79ee123cc479cece27bc994ff2");
        }


        static void createCommunityBlessing()
        {
            var remove_fear = library.Get<BlueprintAbility>("55a037e514c0ee14a8e3ed14b47061de");
            var remove_fear_buff = library.Get<BlueprintBuff>("c5c86809a1c834e42a2eb33133e90a28");
            var eccli_blessing = library.Get<BlueprintAbility>("db0f61cd985ca09498cafde9a4b27a16");
            var remove_self_action = Helpers.CreateActionList(Helpers.Create<ContextActionRemoveSelf>());
            BlueprintBuff[] aid_another_buffs = new BlueprintBuff[2];
            aid_another_buffs[0] = Helpers.CreateBuff("WarpriestCommunityBlessingAidAnother1Buff",
                                                                "Aid Another (Attack Bonus)",
                                                                "In melee combat, you can help a friend attack or defend by distracting or interfering with an opponents. If you’re in position to make a melee attack on an opponent that is engaging a friend in melee combat, you can attempt to aid your friend as a standard action. Your friend gains either a +2 bonus on his next attack roll or a +2 bonus to AC against next attack (your choice), as long as that attack comes before the beginning of your next turn. Multiple characters can aid the same friend, and similar bonuses stack.",
                                                                "",
                                                                remove_fear.Icon,
                                                                null,
                                                                Common.createAttackTypeAttackBonus(Common.createSimpleContextValue(4), AttackTypeAttackBonus.WeaponRangeType.Melee, ModifierDescriptor.UntypedStackable),
                                                                Common.createAddInitiatorAttackWithWeaponTrigger(remove_self_action, check_weapon_range_type: true, wait_for_attack_to_resolve: true)
                                                                );
            aid_another_buffs[0].Stacking = StackingType.Stack;

            aid_another_buffs[1] = Helpers.CreateBuff("WarpriestCommunityBlessingAidAnother2Buff",
                                                    "Aid Another (AC)",
                                                    aid_another_buffs[0].Description,
                                                    "",
                                                    remove_fear.Icon,
                                                    null,
                                                    Helpers.Create<ACBonusAgainstAttacks>(a => { a.Value = Common.createSimpleContextValue(4); a.Descriptor = ModifierDescriptor.UntypedStackable; a.AgainstMeleeOnly = true; }),
                                                    Helpers.Create<AddTargetAttackWithWeaponTrigger>(a =>
                                                                                                    {
                                                                                                        a.ActionOnSelf = remove_self_action;
                                                                                                        a.WaitForAttackResolve = true;
                                                                                                        a.OnlyMelee = true;
                                                                                                    })
                                                    );
            aid_another_buffs[1].Stacking = StackingType.Stack;

            BlueprintAbility[] aid_another_abilities = new BlueprintAbility[aid_another_buffs.Length];

            for (int i = 0; i < aid_another_buffs.Length; i++)
            {
                var apply_buff = Common.createContextActionApplyBuff(aid_another_buffs[i], Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Rounds), dispellable: false);
                aid_another_abilities[i] = Helpers.CreateAbility($"WarpriestCommunityBlessingAidAnother{i + 1}Ability",
                                                                 aid_another_buffs[i].Name,
                                                                 aid_another_buffs[i].Description,
                                                                 "",
                                                                 aid_another_buffs[i].Icon,
                                                                 AbilityType.Special,
                                                                 CommandType.Standard,
                                                                 AbilityRange.Touch,
                                                                 Helpers.oneRoundDuration,
                                                                 "",
                                                                 Helpers.CreateRunActions(apply_buff)
                                                                 );
                aid_another_abilities[i].setMiscAbilityParametersTouchFriendly(works_on_self: false);
            }

            var minor_buff = Helpers.CreateBuff("WarpriestCommunityMinorBlessingBuff",
                                                "Communal Aid",
                                                "At 1st level, you can touch an ally and grant it the blessing of community. For the next minute, whenever that ally uses the aid another action, the bonus granted increases to +4. You can instead use this ability on yourself as a swift action.",
                                                "",
                                                eccli_blessing.Icon,
                                                remove_fear_buff.FxOnStart,
                                                Helpers.CreateAddFacts(aid_another_abilities)
                                                );

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestCommunityMinorBlessingAbility",
                                                      minor_buff.Name + " (Others)",
                                                      minor_buff.Description,
                                                      "",
                                                      eccli_blessing.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      Helpers.willNegates,
                                                      Helpers.CreateRunActions(apply_minor_buff));
            minor_ability.setMiscAbilityParametersTouchFriendly(works_on_self: false);
            addBlessingResourceLogic(minor_ability);

            var minor_ability2 = library.CopyAndAdd<BlueprintAbility>(minor_ability.AssetGuid, "WarpriestCommunityMinorBlessingSelfAbility", "");
            minor_ability2.CanTargetFriends = false;
            minor_ability2.CanTargetSelf = true;
            minor_ability2.ActionType = CommandType.Swift;
            minor_ability2.SetName(minor_buff.Name + " (Self)");
            addBlessingResourceLogic(minor_ability2);

            var true_strike = library.Get<BlueprintAbility>("2c38da66e5a599347ac95b3294acbe00");

            var target_melee_buff = Helpers.CreateBuff("WarpriestCommunityMajorBlessingMeleeTargetBuff",
                                                       "",
                                                       "",
                                                       "",
                                                       null,
                                                       null);
            target_melee_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var target_ranged_buff = library.CopyAndAdd<BlueprintBuff>(target_melee_buff.AssetGuid, "WarpriestCommunityMajorBlessingRangedTargetBuff", "");
            var target_melee_buff2 = library.CopyAndAdd<BlueprintBuff>(target_melee_buff.AssetGuid, "WarpriestCommunityMajorBlessingMeleeTarget2Buff", "");
            var target_ranged_buff2 = library.CopyAndAdd<BlueprintBuff>(target_melee_buff.AssetGuid, "WarpriestCommunityMajorBlessingRangedTarget2Buff", "");

            var apply_melee_buff1 = Common.createContextActionApplyBuff(target_melee_buff,
                                                             Helpers.CreateContextDuration(Common.createSimpleContextValue(1)),
                                                             dispellable: false);
            var apply_melee_buff2 = Common.createContextActionApplyBuff(target_melee_buff2,
                                                 Helpers.CreateContextDuration(Common.createSimpleContextValue(1)),
                                                 dispellable: false);
            var apply_ranged_buff1 = Common.createContextActionApplyBuff(target_ranged_buff,
                                                 Helpers.CreateContextDuration(Common.createSimpleContextValue(1)),
                                                 dispellable: false);
            var apply_ranged_buff2 = Common.createContextActionApplyBuff(target_ranged_buff2,
                                                 Helpers.CreateContextDuration(Common.createSimpleContextValue(1)),
                                                 dispellable: false);

            var ally_buff = Helpers.CreateBuff("WarpriestCommunityMajorBlessingAllyBuff",
                                                     "",
                                                     "",
                                                     "",
                                                     null,
                                                     null,
                                                     Helpers.Create<NewMechanics.AttackBonusAgainstFactsOwner>(a =>
                                                                                                              {
                                                                                                                  a.Bonus = Common.createSimpleContextValue(2);
                                                                                                                  a.attack_types = new AttackType[] { AttackType.Melee, AttackType.Touch };
                                                                                                                  a.Descriptor = ModifierDescriptor.Insight;
                                                                                                                  a.only_from_caster = true;
                                                                                                                  a.CheckedFacts = new BlueprintUnitFact[] { target_melee_buff, target_melee_buff2 };
                                                                                                              }),
                                                      Helpers.Create<NewMechanics.AttackBonusAgainstFactsOwner>(a =>
                                                                                                          {
                                                                                                              a.Bonus = Common.createSimpleContextValue(2);
                                                                                                              a.attack_types = new AttackType[] { AttackType.Ranged, AttackType.RangedTouch };
                                                                                                              a.Descriptor = ModifierDescriptor.Insight;
                                                                                                              a.only_from_caster = true;
                                                                                                              a.CheckedFacts = new BlueprintUnitFact[] { target_ranged_buff, target_ranged_buff2 };
                                                                                                          })
                                                      );
            ally_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var area_effect = library.CopyAndAdd<BlueprintAbilityAreaEffect>("8c5617838c1195443b584e005d0913eb", "WarpriestCommunityMajorBlessingArea", "");
            area_effect.ReplaceComponent<AbilityAreaEffectBuff>(a => a.Buff = ally_buff);
            var major_buff = Helpers.CreateBuff("WarpriestCommunityMajorBlessingCasterBuff",
                                                 "Fight as One",
                                                 "At 10th level, you can rally your allies to fight together. For 1 minute, whenever you make a successful melee or ranged attack against a foe, allies within 10 feet of you gain a +2 insight bonus on attacks of the same type you made against that foe—melee attacks if you made a melee attack, or ranged attacks if you made a ranged attack. If you score a critical hit, this bonus increases to +4 until the start of your next turn.",
                                                 "",
                                                 true_strike.Icon,
                                                 null,
                                                 Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(apply_melee_buff1),
                                                                                                  check_weapon_range_type: true, range_type: AttackTypeAttackBonus.WeaponRangeType.Melee),
                                                 Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(apply_melee_buff2), critical_hit: true,
                                                                                                  check_weapon_range_type: true, range_type: AttackTypeAttackBonus.WeaponRangeType.Melee),
                                                 Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(apply_ranged_buff1),
                                                                                                  check_weapon_range_type: true, range_type: AttackTypeAttackBonus.WeaponRangeType.Ranged),
                                                 Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(apply_ranged_buff2), critical_hit: true,
                                                                                                  check_weapon_range_type: true, range_type: AttackTypeAttackBonus.WeaponRangeType.Ranged),
                                                 Common.createAddAreaEffect(area_effect)
                                                 );

            var apply_major_buff = Common.createContextActionApplyBuff(major_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var major_ability = Helpers.CreateAbility("WarpriestCommunityMajorBlessingAbility",
                                                      major_buff.Name,
                                                      major_buff.Description,
                                                      "",
                                                      major_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Personal,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_major_buff),
                                                      true_strike.GetComponent<AbilitySpawnFx>()
                                                      );
            major_ability.setMiscAbilityParametersSelfOnly();
            addBlessingResourceLogic(major_ability);


            addBlessing("WarpriestBlessingCommunity", "Community",
                        Helpers.CreateFeature("WarpriesCommunityBlessingMinorFeature",
                                              minor_buff.Name,
                                              minor_buff.Description,
                                              "",
                                              minor_buff.Icon,
                                              FeatureGroup.None,
                                              Helpers.CreateAddFacts(minor_ability, minor_ability2)
                                              ),
                        Common.AbilityToFeature(major_ability, false),
                        "c87004460f3328c408d22c5ead05291f");
        }


        static void createDarknessBlessing()
        {
            var blur_buff = library.Get<BlueprintBuff>("dd3ad347240624d46a11a092b4dd4674"); //blur

            var apply_minor_buff = Common.createContextActionApplyBuff(blur_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestDarknessMinorBlessingAbility",
                                                      "Enshrouding Darkness",
                                                      "At 1st level, you can touch an ally and bestow a darkness blessing. For 1 minute, the ally becomes enshrouded in shadows while in combat, granting it concealment (20%). Creatures that are normally able to see in supernatural darkness ignore this concealment.",
                                                      "",
                                                      blur_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_minor_buff));
            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic(minor_ability);


            var blindness_buff = library.Get<BlueprintBuff>("187f88d96a0ef464280706b63635f2af");
            var blindness_spell = library.Get<BlueprintAbility>("46fd02ad56c35224c9c91c88cd457791");

            var apply_major_buff = Common.createContextSavedApplyBuff(blindness_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), is_dispellable: true);
            var apply_major_buff_save = Common.createContextActionSavingThrow(SavingThrowType.Will, Helpers.CreateActionList(apply_major_buff));
            var major_ability = Helpers.CreateAbility("WarpriestDarknessMajorBlessingAbility",
                                                      "Darkened Vision",
                                                      "At 10th level, you can place a shroud of darkness around the eyes of one foe within 30 feet. The target must succeed at a Will saving throw or be blinded for 1 minute.",
                                                      "",
                                                      blindness_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Close,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_major_buff_save),
                                                      blindness_spell.GetComponent<AbilityTargetHasFact>(),
                                                      Common.createContextCalculateAbilityParamsBasedOnClass(warpriest_class, StatType.Wisdom)
                                                      );
            major_ability.setMiscAbilityParametersSingleTargetRangedHarmful(works_on_allies: true);
            addBlessingResourceLogic(major_ability);

            addBlessing("WarpriestBlessingDarkness", "Darkness", minor_ability, major_ability, "6d8e7accdd882e949a63021af5cde4b8");
        }


        static void createCreateDeathBlessing()
        {
            var undead_bloodline_progression = library.Get<BlueprintProgression>("a1a8bf61cadaa4143b2d4966f2d1142e");
            var vampiric_touch = library.Get<BlueprintAbility>("6cbb040023868574b992677885390f92");
            var false_life = library.Get<BlueprintAbility>("7a5b5bf845779a941a67251539545762");
            var minor_buff = Helpers.CreateBuff("WarpriestDeathMinorBlessingBuff",
                                                "From the Grave",
                                                "At 1st level, you can take on a corpse-like visage for 1 minute, making you more intimidating and giving you undead-like protection from harm. You gain a +4 bonus on Intimidate checks, as well as a +2 profane bonus on saving throws against disease, mind-affecting effects, paralysis, poison, and stun.",
                                                "",
                                                undead_bloodline_progression.Icon,
                                                null,
                                                Helpers.CreateAddStatBonus(StatType.CheckIntimidate, 4, ModifierDescriptor.Profane),
                                                Common.createSavingThrowBonusAgainstDescriptor(2, ModifierDescriptor.Profane, SpellDescriptor.Disease | SpellDescriptor.MindAffecting | SpellDescriptor.Paralysis | SpellDescriptor.Poison | SpellDescriptor.Stun)
                                                );

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestDeathMinorBlessingAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Personal,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_minor_buff),
                                                      false_life.GetComponent<AbilitySpawnFx>());

            minor_ability.setMiscAbilityParametersSelfOnly();
            addBlessingResourceLogic(minor_ability);


            var undead = library.Get<BlueprintUnitFact>("734a29b693e9ec346ba2951b27987e33");
            var construct = library.Get<BlueprintUnitFact>("fd389783027d63343b4a5634bd81645f");

            var energy_drain = Helpers.CreateActionEnergyDrain(Helpers.CreateContextDiceValue(DiceType.Zero, bonus: Common.createSimpleContextValue(1)),
                                                               Helpers.CreateContextDuration(bonus: Common.createSimpleContextValue(1), rate: DurationRate.Minutes),
                                                               Kingmaker.RuleSystem.Rules.EnergyDrainType.Temporary);
            var effect_action = Helpers.CreateConditional(new Condition[] { Helpers.CreateConditionHasFact(undead, not: true), Helpers.CreateConditionHasFact(construct, not: true) },
                                                                    energy_drain);
            var projectile_action = vampiric_touch.GetComponent<AbilityEffectRunAction>().Actions.Actions.Last();
           
            var ability = Helpers.CreateAbility("WarpriestDeathMajorBlessingBaseAbility",
                                                "Death’s Touch",
                                                "At 10th level, you can make a melee touch attack against an opponent to deliver grim suffering. If you succeed, you inflict 1 temporary negative level on the target for 1 minute. Alternatively, you can activate this ability as a swift action upon hitting an opponent with a melee attack. These temporary negative levels stack. You gain no benefit from imposing these negative levels (such as the temporary hit points undead gain from enervation).",
                                                "",
                                                vampiric_touch.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Touch,
                                                Helpers.oneMinuteDuration,
                                                Helpers.savingThrowNone,
                                                vampiric_touch.GetComponent<AbilityDeliverTouch>(),
                                                Helpers.CreateRunActions(effect_action, projectile_action),
                                                vampiric_touch.GetComponent<AbilityTargetHasFact>()
                                                );

            ability.setMiscAbilityParametersTouchHarmful(works_on_allies: true);
            var major_ability_touch = Helpers.CreateAbility("WarpriestDeathMajorBlessingTouchAbility",
                                                            ability.Name,
                                                            ability.Description,
                                                            "",
                                                            ability.Icon,
                                                            AbilityType.Supernatural,
                                                            CommandType.Standard,
                                                            AbilityRange.Touch,
                                                            "",
                                                            Helpers.savingThrowNone,
                                                            Helpers.CreateStickyTouch(ability)
                                                            );

            major_ability_touch.setMiscAbilityParametersTouchHarmful(works_on_allies: true);
            addBlessingResourceLogic(major_ability_touch);

            var on_hit_action = Helpers.CreateActionList(effect_action, 
                                                         projectile_action);
            var spend_resource = Common.createContextActionSpendResource(warpriest_blessing_resource, 1, warpriest_aspect_of_war_buff);
            var on_hit_buff = Helpers.CreateBuff("WarpriestDeathMajorOnHitBuff",
                                                 major_ability_touch.Name,
                                                 major_ability_touch.Description,
                                                 "",
                                                 major_ability_touch.Icon,
                                                 null,
                                                 Common.createAddInitiatorAttackWithWeaponTrigger(on_hit_action, check_weapon_range_type: true, only_first_hit: true),
                                                 Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(spend_resource), on_initiator: true, 
                                                                                                  check_weapon_range_type: true, only_first_hit: true)
                                                 );
            var major_activatable_ability = Helpers.CreateActivatableAbility("WarpriestDeathMajorBlessingOnHitActivatable",
                                                                             major_ability_touch.Name + " (On Hit)",
                                                                             major_ability_touch.Description,
                                                                             "",
                                                                             major_ability_touch.Icon,
                                                                             on_hit_buff,
                                                                             AbilityActivationType.Immediately,
                                                                             CommandType.Free,
                                                                             null,
                                                                             Helpers.CreateActivatableResourceLogic(warpriest_blessing_resource, ResourceSpendType.Never)
                                                                             );
            if (!test_mode)
            {
                major_activatable_ability.AddComponent(Common.createActivatableAbilityUnitCommand(CommandType.Swift));
            }
            major_ability_touch.AddComponent(Common.createAbilityCasterHasNoFacts(on_hit_buff));

            addBlessing("WarpriestDeathBlessing", "Death",
                        Common.AbilityToFeature(minor_ability, false),
                        Helpers.CreateFeature("WarpriestDeathMajorBlessingFeature",
                                              major_ability_touch.Name,
                                              major_ability_touch.Description,
                                              "",
                                              major_ability_touch.Icon,
                                              FeatureGroup.None,
                                              Helpers.CreateAddFacts(major_ability_touch, major_activatable_ability)
                                              ),
                         "a099afe1b0b32554199b230699a69525");
        }


        static void createDestructionBlessing()
        {
            var resounding_blow_buff = library.Get<BlueprintBuff>("06173a778d7067a439acffe9004916e9");
            var minor_buff = Helpers.CreateBuff("WarpriestDestructionMinorBlessingBuff",
                                    "Destructive Attacks",
                                    "At 1st level, you can touch an ally and bless it with the power of destruction. For 1 minute, the ally gains a morale bonus on weapon damage rolls equal to half your level (minimum 1).",
                                    "",
                                    resounding_blow_buff.Icon,
                                    resounding_blow_buff.FxOnStart,
                                    Helpers.CreateAddContextStatBonus(StatType.AdditionalDamage, ModifierDescriptor.Morale, rankType: AbilityRankType.DamageBonus),
                                    Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getWarpriestArray(),
                                                                    type: AbilityRankType.DamageBonus, progression: ContextRankProgression.Div2, min: 1)
                                    );

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestDestructionMinorBlessingAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_minor_buff)
                                                      );

            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic(minor_ability);

            var defensive_stance_buff = library.Get<BlueprintBuff>("3dccdf27a8209af478ac71cded18a271");
            var major_buff = Helpers.CreateBuff("WarpriestDestructionMajorBlessingBuff",
                        "Heart of Carnage",
                        "At 10th level, you can touch an ally and bless it with even greater destructive power. For 1 minute, the ally gains a +4 insight bonus on attack rolls made to confirm critical hits and has a 50% chance to treat any critical hit or sneak attack against it as a normal hit.",
                        "",
                        defensive_stance_buff.Icon,
                        defensive_stance_buff.FxOnStart,
                        Common.createCriticalConfirmationBonus(4),
                        Common.createAddFortification(50)
                        );

            var apply_major_buff = Common.createContextActionApplyBuff(major_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var major_ability = Helpers.CreateAbility("WarpriestDestructionMajorBlessingAbility",
                                                      major_buff.Name,
                                                      major_buff.Description,
                                                      "",
                                                      major_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_major_buff)
                                                      );

            major_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic(major_ability);

            addBlessing("WarpriestBlessingDestruction", "Destruction", minor_ability, major_ability, "6832681c9a91bf946a1d9da28c5be4b4");
        }


        static void createEarthBlessing()
        {
            var bless_weapon = library.Get<BlueprintAbility>("831e942864e924846a30d2e0678e438b");
            var acid_enchantment = library.Get<BlueprintWeaponEnchantment>("633b38ff1d11de64a91d490c683ab1c8");
            var corrosive_touch = library.Get<BlueprintAbility>("1a40fc88aeac9da4aa2fbdbb88335f5d");

            var enchantment = Common.createWeaponEnchantment("WarpriestEarthMinorBlessingWeaponEcnchantment",
                                                             "Acid Strike",
                                                             "This weapon emits acrid fumes that deal an additional 1d4 points of acid damage with each strike.",
                                                             "",
                                                             "",
                                                             "",
                                                             0,
                                                             acid_enchantment.WeaponFxPrefab,
                                                             Common.weaponEnergyDamageDice(DamageEnergyType.Acid, new DiceFormula(1, DiceType.D4))
                                                             );
            var minor_buff = Helpers.CreateBuff("WarpriestEarthMinorBlessingBuff",
                                     enchantment.Name,
                                     "At 1st level, you can touch one weapon and enhance it with acidic potency. For 1 minute, this weapon emits acrid fumes that deal an additional 1d4 points of acid damage with each strike.",
                                     "",
                                     corrosive_touch.Icon,
                                     null,
                                     Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), false, true, enchantment)
                                    );

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestEarthMinorBlessingAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_minor_buff),
                                                      bless_weapon.GetComponent<AbilitySpawnFx>()
                                                      );

            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic(minor_ability);

            var stoneskin_buff = library.Get<BlueprintBuff>("7aeaf147211349b40bb55c57fec8e28d");
            var major_buff = Helpers.CreateBuff("WarpriestEarthMajorBlessingBuff",
                        "Armor of Earth",
                        "At 10th level, you can touch an ally and harden its armor or clothing. For 1 minute, the ally gains DR 1/—. For every 2 levels beyond 10th, this DR increases by 1 (to a maximum of DR 5/— at 18th level). This doesn’t stack with any other damage resistance or reduction.",
                        "",
                        stoneskin_buff.Icon,
                        stoneskin_buff.FxOnStart,
                        Common.createContextPhysicalDR(Helpers.CreateContextValue(AbilityRankType.StatBonus)),
                        Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getWarpriestArray(),
                                                        type: AbilityRankType.StatBonus, progression: ContextRankProgression.StartPlusDivStep, startLevel: 10, stepLevel: 2, max: 5)
                        );

            var apply_major_buff = Common.createContextActionApplyBuff(major_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var major_ability = Helpers.CreateAbility("WarpriestEarthMajorBlessingAbility",
                                                      major_buff.Name,
                                                      major_buff.Description,
                                                      "",
                                                      major_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_major_buff)
                                                      );

            major_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic(major_ability);

            addBlessing("WarpriestBlessingEarth", "Earth", minor_ability, major_ability, "5ca99a6ae118feb449dbbd165a8fe7c4");
        }


        static void createEvilBlessing()
        {
            var unholy_weapon = library.Get<BlueprintActivatableAbility>("561803a819460f34ea1fe079edabecce");
            var bless_weapon = library.Get<BlueprintAbility>("831e942864e924846a30d2e0678e438b");
            var unholy_enchantment = library.Get<BlueprintWeaponEnchantment>("d05753b8df780fc4bb55b318f06af453");

            var enchantment = Common.createWeaponEnchantment("WarpriestEvilMinorBlessingWeaponEcnchantment",
                                                             "Unholy Strike",
                                                             "This weapon takes on a black, orange, or violet cast and deals an additional 1d6 points of damage against good creatures. During this time, it’s treated as evil for the purposes of overcoming damage reduction.",
                                                             "",
                                                             "",
                                                             "",
                                                             0,
                                                             unholy_enchantment.WeaponFxPrefab,
                                                             Common.createWeaponDamageAgainstAlignment(DamageEnergyType.Unholy, DamageAlignment.Evil, AlignmentComponent.Good,
                                                                                                       Helpers.CreateContextDiceValue(DiceType.D6, Common.createSimpleContextValue(1)))
                                                             );

            var minor_buff = Helpers.CreateBuff("WarpriestEvilMinorBuff",
                                                enchantment.Name,
                                                "At 1st level, you can touch one weapon and give it an evil blessing. For 1 minute, this weapon takes on a black, orange, or violet cast and deals an additional 1d6 points of damage against good creatures. During this time, it’s treated as evil for the purposes of overcoming damage reduction.",
                                                "",
                                                unholy_weapon.Icon,
                                                null,
                                                Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), false, true, enchantment));

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestEvilBlessingMinorAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      Helpers.savingThrowNone,
                                                      bless_weapon.GetComponent<AbilitySpawnFx>(),
                                                      Helpers.CreateRunActions(apply_minor_buff)
                                                      );

            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic(minor_ability);

            string[] summon_monster_guids = new string[] {"efa433a38e9c7c14bb4e780f8a3fe559", "0964bf88b582bed41b74e79596c4f6d9", "02de4dd8add69aa42a3d1330b573e2ab",
                                                      "2920d48574933c24391fbb9e18f87bf5", "eb6df7ddfc0669d4fb3fc9af4bd34bca", "e96593e67d206ab49ad1b567327d1e75" };

            var summon_m9 = library.Get<BlueprintAbility>("e96593e67d206ab49ad1b567327d1e75");

            List<ActionList> summon_actions = new List<ActionList>();
            foreach (var s in summon_monster_guids)
            {
                summon_actions.Add(library.Get<BlueprintAbility>(s).GetComponent<AbilityEffectRunAction>().Actions);
            }

            var major_ability = library.CopyAndAdd<BlueprintAbility>(summon_m9.AssetGuid, "WarpirestEvilBlessingMajorAbility", "");
            major_ability.SetName("Battle Companion");
            major_ability.SetDescription("At 10th level, you can summon a battle companion. This ability functions as summon monster IV with a duration of 1 minute. This ability can summon only one creature, regardless of the list used. For every 2 levels beyond 10th, the level of the summon monster spell increases by 1 (to a maximum of summon monster IX at 20th level).");
            major_ability.RemoveComponents<SpellComponent>();
            major_ability.Type = AbilityType.Supernatural;
            var action = Helpers.CreateRunActions(Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus), summon_actions.ToArray()));
            major_ability.ReplaceComponent<AbilityEffectRunAction>(action);
            major_ability.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(min: 10, max: 10));
            major_ability.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getWarpriestArray(),
                                                                       progression: ContextRankProgression.DelayedStartPlusDivStep,
                                                                       startLevel: 10, stepLevel: 2, max: 6, type: AbilityRankType.StatBonus
                                                                      )
                                      );
            major_ability.LocalizedDuration = Helpers.oneMinuteDuration;
            major_ability.Parent = null;
            addBlessingResourceLogic(major_ability);
            addBlessing("WarpriestBlessingEvil", "Evil", minor_ability, major_ability, "351235ac5fc2b7e47801f63d117b656c");
        }


        static void createFireBlessing()
        {
            var flaming_enchantment = library.Get<BlueprintWeaponEnchantment>("30f90becaaac51f41bf56641966c4121");
            var bless_weapon = library.Get<BlueprintAbility>("831e942864e924846a30d2e0678e438b");

            var enchantment = Common.createWeaponEnchantment("WarpriestFireMinorBlessingWeaponEcnchantment",
                                                             "Fire Strike",
                                                             "This weapon glows red-hot and deals an additional 1d4 points of fire damage with each hit.",
                                                             "",
                                                             "",
                                                             "",
                                                             0,
                                                             flaming_enchantment.WeaponFxPrefab,
                                                             Common.weaponEnergyDamageDice(DamageEnergyType.Acid, new DiceFormula(1, DiceType.D4))
                                                             );
            var minor_buff = Helpers.CreateBuff("WarpriestFireMinorBlessingBuff",
                                     enchantment.Name,
                                     "At 1st level, you can touch one weapon and enhance it with the grandeur of fire. For 1 minute, this weapon glows red-hot and deals an additional 1d4 points of fire damage with each hit.",
                                     "",
                                     bless_weapon.Icon,
                                     null,
                                     Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), false, true, enchantment)
                                    );

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestFireMinorBlessingAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_minor_buff),
                                                      bless_weapon.GetComponent<AbilitySpawnFx>()
                                                      );

            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic(minor_ability);

            var warm_shield = NewSpells.fire_shield_variants[DamageEnergyType.Fire];
            var major_buff = library.CopyAndAdd<BlueprintBuff>(NewSpells.fire_shield_buffs[DamageEnergyType.Fire], "WarpriestFireMajorBlessingBuff", "");
            major_buff.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getWarpriestArray(),
                                                                                           max: 15, type: AbilityRankType.DamageBonus
                                                                                           )
                                                          );
            major_buff.SetName("Armor of Flame");
            major_buff.SetDescription("At 10th level, you can touch an ally to wreath it in flames. This works as fire shield (warm shield only) with a duration of 1 minute.");

            var apply_major_buff = Common.createContextActionApplyBuff(major_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var major_ability = Helpers.CreateAbility("WarpriestFireMajorBlessingAbility",
                                                      major_buff.Name,
                                                      major_buff.Description,
                                                      "",
                                                      major_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_minor_buff),
                                                      warm_shield.GetComponent<SpellDescriptorComponent>(),
                                                      warm_shield.GetComponent<AbilitySpawnFx>()
                                                      );

            major_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic(major_ability);

            addBlessing("WarpriestBlessingFire", "Fire", minor_ability, major_ability, "8d4e9731082008640b28417f577f5f31");
        }


        static void createGloryBlessing()
        {
            var lesser_restoration = library.Get<BlueprintAbility>("e84fc922ccf952943b5240293669b171");
            var apply_minor_buff = Common.createContextActionApplyBuff(warpriest_blessing_special_sancturay_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestGloryMinorBlessingAbility",
                                                      "Glorious Presence",
                                                      "At 1st level, you can touch an ally and grant it a glorious blessing. For 1 minute, the ally becomes mesmerizing to her foes. This functions as sanctuary, except if the ally attacks an opponent, this effect ends with respect to only that opponent. This is a mind-affecting effect.",
                                                      "",
                                                      lesser_restoration.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      Helpers.willNegates,
                                                      Helpers.CreateRunActions(apply_minor_buff));
            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic(minor_ability);

            var heroism = library.Get<BlueprintAbility>("5ab0d42fb68c9e34abae4921822b9d63");
            var cornugon_smash = library.Get<BlueprintFeature>("ceea53555d83f2547ae5fc47e0399e14");
            var demoralize_action = ((Conditional)cornugon_smash.GetComponent<AddInitiatorAttackWithWeaponTrigger>().Action.Actions[0]).IfTrue.Actions[0];
            var spend_resource = Common.createContextActionSpendResource(warpriest_blessing_resource, 1, warpriest_aspect_of_war_buff);
            var demoralize_on_damage = Helpers.Create<AddOutgoingDamageTrigger>(a =>
                                                                                {
                                                                                    a.Actions = Helpers.CreateActionList(demoralize_action, spend_resource);
                                                                                    a.NotZeroDamage = true;
                                                                                });


            var major_buff = Helpers.CreateBuff("WarpriestGloryMajorOnHitBuff",
                                                "Demoralizing Glory",
                                                "At 10th level, when you successfully damage an opponent with a melee attack or attack spell, as a swift action you can attempt to demoralize that opponent with the Intimidate skill using your ranks in Intimidate or your warpriest level, whichever is higher.",
                                                "",
                                                 heroism.Icon,
                                                 null,
                                                 demoralize_on_damage);

            major_buff.AddComponent(Helpers.Create<NewMechanics.ReplaceSkillRankWithClassLevel>(r =>
                                                                                                {
                                                                                                    r.character_class = warpriest_class;
                                                                                                    r.skill = StatType.CheckIntimidate;
                                                                                                    r.reason = major_buff;
                                                                                                })
                                   );
                                                 
            var major_activatable_ability = Helpers.CreateActivatableAbility("WarpriestGloryMajorBlessingActivatable",
                                                                             major_buff.Name,
                                                                             major_buff.Description,
                                                                             "",
                                                                             major_buff.Icon,
                                                                             major_buff,
                                                                             AbilityActivationType.Immediately,
                                                                             CommandType.Free,
                                                                             null,
                                                                             Helpers.CreateActivatableResourceLogic(warpriest_blessing_resource, ResourceSpendType.Never)
                                                                             );
            if (!test_mode)
            {
                major_activatable_ability.AddComponent(Common.createActivatableAbilityUnitCommand(CommandType.Swift));
            }
            addBlessing("WarpriestGloryBlessing", "Glory",
                        Common.AbilityToFeature(minor_ability, false),
                        Common.ActivatableAbilityToFeature(major_activatable_ability, false),
                        "2418251fa9c8ada4bbfbaaf5c90ac200"
                        );
        }


        static void createGoodBlessing()
        {
            var holy_weapon = library.Get<BlueprintActivatableAbility>("ce0ece459ebed9941bb096f559f36fa8");
            var bless_weapon = library.Get<BlueprintAbility>("831e942864e924846a30d2e0678e438b");
            var holy_enchantment = library.Get<BlueprintWeaponEnchantment>("28a9964d81fedae44bae3ca45710c140");

            var enchantment = Common.createWeaponEnchantment("WarpriestGoodMinorBlessingWeaponEcnchantment",
                                                             "Holy Strike",
                                                             "This weapon glows green, white, or yellow-gold and deals an additional 1d6 points of damage against evil creatures. During this time, it’s treated as good for the purposes of overcoming damage reduction.",
                                                             "",
                                                             "",
                                                             "",
                                                             0,
                                                             holy_enchantment.WeaponFxPrefab,
                                                             Common.createWeaponDamageAgainstAlignment(DamageEnergyType.Holy, DamageAlignment.Good, AlignmentComponent.Evil,
                                                                                                       Helpers.CreateContextDiceValue(DiceType.D6, Common.createSimpleContextValue(1)))
                                                             );

            var minor_buff = Helpers.CreateBuff("WarpriestGoodMinorBuff",
                                                enchantment.Name,
                                                "At 1st level, you can touch one weapon and bless it with the power of purity and goodness. For 1 minute, this weapon glows green, white, or yellow-gold and deals an additional 1d6 points of damage against evil creatures. During this time, it’s treated as good for the purposes of overcoming damage reduction.",
                                                "",
                                                holy_weapon.Icon,
                                                null,
                                                Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), false, true, enchantment));

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestGoodBlessingMinorAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      Helpers.savingThrowNone,
                                                      bless_weapon.GetComponent<AbilitySpawnFx>(),
                                                      Helpers.CreateRunActions(apply_minor_buff)
                                                      );

            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic(minor_ability);

            string[] summon_monster_guids = new string[] {"efa433a38e9c7c14bb4e780f8a3fe559", "0964bf88b582bed41b74e79596c4f6d9", "02de4dd8add69aa42a3d1330b573e2ab",
                                                      "2920d48574933c24391fbb9e18f87bf5", "eb6df7ddfc0669d4fb3fc9af4bd34bca", "e96593e67d206ab49ad1b567327d1e75" };

            var summon_m9 = library.Get<BlueprintAbility>("e96593e67d206ab49ad1b567327d1e75");

            List<ActionList> summon_actions = new List<ActionList>();
            foreach (var s in summon_monster_guids)
            {
                summon_actions.Add(library.Get<BlueprintAbility>(s).GetComponent<AbilityEffectRunAction>().Actions);
            }

            var major_ability = library.CopyAndAdd<BlueprintAbility>(summon_m9.AssetGuid, "WarpirestGoodBlessingMajorAbility", "");
            major_ability.SetName("Battle Companion");
            major_ability.SetDescription("At 10th level, you can summon a battle companion. This ability functions as summon monster IV with a duration of 1 minute. This ability can summon only one creature, regardless of the list used. For every 2 levels beyond 10th, the level of the summon monster spell increases by 1 (to a maximum of summon monster IX at 20th level).");
            major_ability.RemoveComponents<SpellComponent>();
            major_ability.Type = AbilityType.Supernatural;
            var action = Helpers.CreateRunActions(Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus), summon_actions.ToArray()));
            major_ability.ReplaceComponent<AbilityEffectRunAction>(action);
            major_ability.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(min: 10, max: 10));
            major_ability.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getWarpriestArray(),
                                                                       progression: ContextRankProgression.DelayedStartPlusDivStep,
                                                                       startLevel: 10, stepLevel: 2, max: 6, type: AbilityRankType.StatBonus
                                                                      )
                                      );
            major_ability.LocalizedDuration = Helpers.oneMinuteDuration;
            major_ability.Parent = null;
            addBlessingResourceLogic(major_ability);
            addBlessing("WarpriestBlessingGood", "Good", minor_ability, major_ability, "882521af8012fc749930b03dc18a69de");
        }


        static void createHealingBlessing()
        {
            var healing_domain_feature = library.Get<BlueprintFeature>("b9ea4eb16ded8b146868540e711f81c8");
            var touch_of_good = library.Get<BlueprintAbility>("18f734e40dd7966438ab32086c3574e1");
            var healing_metamagic = Helpers.Create<NewMechanics.MetamagicOnSpellDescriptor>(m =>
                                                                                            {
                                                                                                m.amount = 1;
                                                                                                m.Metamagic = Kingmaker.UnitLogic.Abilities.Metamagic.Empower;
                                                                                                m.resource = warpriest_blessing_resource;
                                                                                                m.spell_descriptor = SpellDescriptor.RestoreHP | SpellDescriptor.Cure;
                                                                                                m.cost_reducing_facts = new BlueprintUnitFact[] { warpriest_aspect_of_war_buff };
                                                                                            });

            var minor_buff = Helpers.CreateBuff("WarpriestHealingMinorBuff",
                                    "Powerful Healer",
                                    "At 1st level, you can add power to a cure spell as you cast it. As a swift action, you can treat any cure spell as if it were empowered (as the Empower Spell feat), causing it to heal 50% more damage (or deal 50% more damage if used against undead). This ability doesn’t stack with itself or the Empower Spell feat.",
                                     "",
                                     healing_domain_feature.Icon,
                                     null,
                                     healing_metamagic);

            var minor_activatable_ability = Helpers.CreateActivatableAbility("WarpriestHealingMinorBlessingActivatableAbility",
                                                                             minor_buff.Name,
                                                                             minor_buff.Description,
                                                                             "",
                                                                             minor_buff.Icon,
                                                                             minor_buff,
                                                                             AbilityActivationType.Immediately,
                                                                             CommandType.Free,
                                                                             null,
                                                                             Helpers.CreateActivatableResourceLogic(warpriest_blessing_resource, ResourceSpendType.Never)
                                                                             );

            if (!test_mode)
            {
                minor_activatable_ability.AddComponent(Common.createActivatableAbilityUnitCommand(CommandType.Swift));
            }

            var major_buff = Helpers.CreateBuff("WarpriestHealingMajorBuff",
                                                "Fast Healing",
                                                "At 10th level, you can touch an ally and grant it fast healing 3 for 1 minute.",
                                                 "",
                                                 touch_of_good.Icon,
                                                 null,
                                                 Helpers.Create<AddEffectFastHealing>(a => a.Heal = 3)
                                                 );

            var apply_major_buff = Common.createContextActionApplyBuff(major_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var major_ability = Helpers.CreateAbility("WarpriestHealingMajorBlessingAbility",
                                                      major_buff.Name,
                                                      major_buff.Description,
                                                      "",
                                                      touch_of_good.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      "",
                                                      Helpers.CreateRunActions(apply_major_buff),
                                                      touch_of_good.GetComponent<AbilitySpawnFx>());
            major_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic(major_ability);

            addBlessing("WarpriestHealingBlessing", "Healing",
                        Common.ActivatableAbilityToFeature(minor_activatable_ability, false),
                        Common.AbilityToFeature(major_ability, false),
                        "73ae164c388990c43ade94cfe8ed5755"
                        );
        }


        static void createKnowledgeBlessing()
        {
            var aid = library.Get<BlueprintAbility>("03a9630394d10164a9410882d31572f0");
            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");

            var lore_action = Helpers.Create<NewMechanics.MonsterLore.ContextMonsterLoreCheckUsingClassAndStat>(c =>
                                                                                                               {
                                                                                                                   c.bonus = 15;
                                                                                                                   c.character_class = warpriest_class;
                                                                                                                   c.stat_type = StatType.Wisdom;
                                                                                                               }
                                                                                                               );

            var minor_ability_resolve = Helpers.CreateAbility("WarpriestKnowledgeMinorBlessingResolveAbility",
                                                              "Lore Keeper",
                                                              "At 1st level, you can touch a creature to learn about its abilities and weaknesses. With a successful touch attack, you gain information as if your result on the appropriate Knowledge skill check were equal to 15 + your warpriest level + your Wisdom modifier.",
                                                              "",
                                                              aid.Icon,
                                                              AbilityType.Supernatural,
                                                              CommandType.Standard,
                                                              AbilityRange.Touch,
                                                              "",
                                                              Helpers.savingThrowNone,
                                                              Helpers.CreateDeliverTouch(),
                                                              Helpers.CreateRunActions(lore_action),
                                                              Helpers.Create<NewMechanics.MonsterLore.AbilityTargetCanBeInspected>(),
                                                              aid.GetComponent<AbilitySpawnFx>()
                                                              );
            minor_ability_resolve.setMiscAbilityParametersTouchHarmful();
            
            var minor_ability      = Helpers.CreateAbility("WarpriestKnowledgeMinorBlessingAbility",
                                                            minor_ability_resolve.Name,
                                                            minor_ability_resolve.Description,
                                                            "",
                                                            minor_ability_resolve.Icon,
                                                            AbilityType.Supernatural,
                                                            CommandType.Standard,
                                                            AbilityRange.Touch,
                                                            "",
                                                            Helpers.savingThrowNone,
                                                            Helpers.CreateStickyTouch(minor_ability_resolve),
                                                            Helpers.Create<NewMechanics.MonsterLore.AbilityTargetCanBeInspected>(),
                                                            aid.GetComponent<AbilitySpawnFx>()
                                                           );
            minor_ability.setMiscAbilityParametersTouchHarmful();
            addBlessingResourceLogic(minor_ability);
            var major_target_buff = Helpers.CreateBuff("WarpriestKnowledgBlessingeMajorTargetBuff",
                                                "Monster Lore",
                                                "At 10th level, you can as a swift action gain a +2 insight bonus on attacks, saving throws, as well as to your AC against the creature that was previously sucessfully inspected by you or your allies. This effect lasts for 1 minute.",
                                                "",
                                                detect_magic.Icon,
                                                null,
                                                Helpers.Create<AttackBonusAgainstTarget>(c => { c.Value = Common.createSimpleContextValue(2); c.CheckCaster = true; }),
                                                Helpers.Create<ACBonusAgainstTarget>(c => { c.Value = Common.createSimpleContextValue(2); c.CheckCaster = true; c.Descriptor = ModifierDescriptor.Insight; })
                                                );

            var major_caster_buff = Helpers.CreateBuff("WarpriestKnowledgBlessingeMajorCasterBuff",
                                    "",
                                    "",
                                    "",
                                    detect_magic.Icon,
                                    null,
                                    Helpers.Create<NewMechanics.SavingThrowBonusAgainstFactFromCaster>(c =>
                                                                                                       { c.Value = Common.createSimpleContextValue(2);
                                                                                                         c.CheckedFact = major_target_buff;
                                                                                                         c.Descriptor = ModifierDescriptor.Insight;
                                                                                                       })
                                    );
            major_caster_buff.Stacking = StackingType.Stack;
            major_caster_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var apply_target_buff = Common.createContextActionApplyBuff(major_target_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: true);
            var apply_caster_buff = Common.createContextActionApplyBuff(major_caster_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: true);
            var major_ability = Helpers.CreateAbility("WarpriestKnowledgeBlessingMajorAbility",
                                                      major_target_buff.Name,
                                                      major_target_buff.Description,
                                                      "",
                                                      major_target_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Swift,
                                                      AbilityRange.Close,
                                                      Helpers.oneMinuteDuration,
                                                      Helpers.savingThrowNone,
                                                      aid.GetComponent<AbilitySpawnFx>(),
                                                      Helpers.CreateRunActions(apply_target_buff, Helpers.Create<ContextActionOnContextCaster>(c => c.Actions = Helpers.CreateActionList(apply_caster_buff)))
                                                      );
            major_ability.setMiscAbilityParametersTouchHarmful();
            addBlessingResourceLogic(major_ability);
            addBlessing("WarpriestBlessingKnowledge", "Knowledge", minor_ability, major_ability, "443d44b3e0ea84046a9bf304c82a0425");
        }


        static void createLawBlessing()
        {
            var axiomatic_weapon = library.Get<BlueprintActivatableAbility>("d76e8a80ab14ac942b6a9b8aaa5860b1");
            var bless_weapon = library.Get<BlueprintAbility>("831e942864e924846a30d2e0678e438b");
            var axiomatic_enchantment = library.Get<BlueprintWeaponEnchantment>("0ca43051edefcad4b9b2240aa36dc8d4");

            var enchantment = Common.createWeaponEnchantment("WarpriestLawMinorBlessingWeaponEcnchantment",
                                                             "Axiomatic Strike",
                                                             "This weapon glows blue, pale yellow, or white and deals an additional 1d6 points of damage against chaotic creatures. During this time, it’s treated as lawful for the purposes of overcoming damage reduction.",
                                                             "",
                                                             "",
                                                             "",
                                                             0,
                                                             axiomatic_enchantment.WeaponFxPrefab,
                                                             Common.createWeaponDamageAgainstAlignment(DamageEnergyType.Holy, DamageAlignment.Lawful, AlignmentComponent.Chaotic,
                                                                                                       Helpers.CreateContextDiceValue(DiceType.D6, Common.createSimpleContextValue(1)))
                                                             );

            var minor_buff = Helpers.CreateBuff("WarpriestLawMinorBuff",
                                                enchantment.Name,
                                                "At 1st level, you can touch one weapon and enhance it with the essence of law. For 1 minute, this weapon glows blue, pale yellow, or white and deals an additional 1d6 points of damage against chaotic creatures. During this time, it’s treated as lawful for the purposes of overcoming damage reduction.",
                                                "",
                                                axiomatic_weapon.Icon,
                                                null,
                                                Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), false, true, enchantment));

            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestLawBlessingMinorAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Touch,
                                                      Helpers.oneMinuteDuration,
                                                      Helpers.savingThrowNone,
                                                      bless_weapon.GetComponent<AbilitySpawnFx>(),
                                                      Helpers.CreateRunActions(apply_minor_buff)
                                                      );

            minor_ability.setMiscAbilityParametersTouchFriendly();
            addBlessingResourceLogic(minor_ability);

            string[] summon_monster_guids = new string[] {"efa433a38e9c7c14bb4e780f8a3fe559", "0964bf88b582bed41b74e79596c4f6d9", "02de4dd8add69aa42a3d1330b573e2ab",
                                                      "2920d48574933c24391fbb9e18f87bf5", "eb6df7ddfc0669d4fb3fc9af4bd34bca", "e96593e67d206ab49ad1b567327d1e75" };

            var summon_m9 = library.Get<BlueprintAbility>("e96593e67d206ab49ad1b567327d1e75");

            List<ActionList> summon_actions = new List<ActionList>();
            foreach (var s in summon_monster_guids)
            {
                summon_actions.Add(library.Get<BlueprintAbility>(s).GetComponent<AbilityEffectRunAction>().Actions);
            }

            var major_ability = library.CopyAndAdd<BlueprintAbility>(summon_m9.AssetGuid, "WarpirestLawBlessingMajorAbility", "");
            major_ability.SetName("Battle Companion");
            major_ability.SetDescription("At 10th level, you can summon a battle companion. This ability functions as summon monster IV with a duration of 1 minute. This ability can summon only one creature, regardless of the list used. For every 2 levels beyond 10th, the level of the summon monster spell increases by 1 (to a maximum of summon monster IX at 20th level).");
            major_ability.RemoveComponents<SpellComponent>();
            major_ability.Type = AbilityType.Supernatural;
            var action = Helpers.CreateRunActions(Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus), summon_actions.ToArray()));
            major_ability.ReplaceComponent<AbilityEffectRunAction>(action);
            major_ability.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(min: 10, max: 10));
            major_ability.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getWarpriestArray(),
                                                                       progression: ContextRankProgression.DelayedStartPlusDivStep,
                                                                       startLevel: 10, stepLevel: 2, max: 6, type: AbilityRankType.StatBonus
                                                                      )
                                      );
            major_ability.LocalizedDuration = Helpers.oneMinuteDuration;
            major_ability.Parent = null;
            addBlessingResourceLogic(major_ability);
            addBlessing("WarpriestBlessingLaw", "Law", minor_ability, major_ability, "092714336606cfc45a37d2ab39fabfa8");
        }


        static void createLiberationBlessing()
        {
            var freedom_of_movement_buff = library.Get<BlueprintBuff>("1533e782fca42b84ea370fc1dcbf4fc1");
            var buff = library.CopyAndAdd<BlueprintBuff>("60906dd9e4ddec14c8ac9a0f4e47f54c", "WarpriestLiberationBlessingBuff", ""); //freedom of movement no strings
            var spend_resource = Common.createContextActionSpendResource(warpriest_blessing_resource, 1, warpriest_aspect_of_war_buff);
            var spend_resource_caster = Helpers.CreateConditional(Common.createContextConditionIsCaster(), spend_resource);
            buff.AddComponent(Helpers.CreateAddFactContextActions(newRound: spend_resource_caster));
            buff.FxOnStart = freedom_of_movement_buff.FxOnStart;
            buff.SetIcon(null);

            var minor_activatable_ability = Helpers.CreateActivatableAbility("WarpriestLiberationBlessingMinorActivatableAbility",
                                                                            "Liberation",
                                                                            "At 1st level, for 1 round as a swift action, you can ignore impediments to your mobility and effects that cause paralysis (as freedom of movement).",
                                                                             "",
                                                                             freedom_of_movement_buff.Icon,
                                                                             buff,
                                                                             AbilityActivationType.Immediately,
                                                                             CommandType.Free,
                                                                             null,
                                                                             Helpers.CreateActivatableResourceLogic(warpriest_blessing_resource, ResourceSpendType.Never)
                                                                             );
            if (!test_mode)
            {
                minor_activatable_ability.AddComponent(Common.createActivatableAbilityUnitCommand(CommandType.Swift));
            }





            var major_area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("cd23b709497500142b59802d7bc85edc", "WarpriestLiberationMajorBlessingArea", "");
            major_area.ReplaceComponent<AbilityAreaEffectBuff>(c => c.Buff = buff);

            var major_buff = library.CopyAndAdd<BlueprintBuff>("aa561f70d2260524e82c794d6140677c", "WarpriestMajorBlessingBuff", "");
            major_buff.SetName("Freedom’s Shout");
            major_buff.SetDescription("At 10th level, as a swift action you can emit a 30-foot aura that affects all allies with the liberation blessing");

            var major_activatable_ability = Helpers.CreateActivatableAbility("WarpriestLiberationBlessingMajorActivatableAbility",
                                                                             major_buff.Name,
                                                                             major_buff.Description,
                                                                             "",
                                                                             major_buff.Icon,
                                                                             major_buff,
                                                                             AbilityActivationType.Immediately,
                                                                             CommandType.Free,
                                                                             null,
                                                                             Helpers.CreateActivatableResourceLogic(warpriest_blessing_resource, ResourceSpendType.Never)
                                                                             );
            if (!test_mode)
            {
                major_activatable_ability.AddComponent(Common.createActivatableAbilityUnitCommand(CommandType.Swift));
            }

            addBlessing("WarpriestLiberationBlessing", "Liberation",
                        Common.ActivatableAbilityToFeature(minor_activatable_ability, false),
                        Common.ActivatableAbilityToFeature(major_activatable_ability, false),
                        "801ca88338451a546bca2ee59da87c53"
                        );
        }


        static void createLuckBlessing()
        {
            var minor_ability = library.CopyAndAdd<BlueprintAbility>("9af0b584f6f754045a0a79293d100ab3", "WarpriestLuckBlessingMinorAbility", "");
            minor_ability.RemoveComponents<ReplaceAbilitiesStat>();
            minor_ability.RemoveComponents<AbilityResourceLogic>();
            minor_ability.SetName("Lucky Presence");
            minor_ability.SetDescription("You can touch a willing creature as a standard action, giving it a bit of luck. For the next round, any time the target rolls a d20, he may roll twice and take the more favorable result.");
            addBlessingResourceLogic(minor_ability);

            var major_ability = library.CopyAndAdd<BlueprintAbility>("0e0668a703fbfcf499d9aa9d918b71ea", "WarpriestLuckBlessingMajorAbility", ""); //divine fortune
            major_ability.SetDescription("At 10th level, you can call on your deity to give you unnatural luck. This ability functions like Lucky Presence, but it affects you and lasts for a number of rounds equal to 1/2 your warpriest level.");
            major_ability.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", getWarpriestArray()));
            major_ability.RemoveComponents<AbilityResourceLogic>();
            addBlessingResourceLogic(major_ability);

            addBlessing("WarpriestBlessingLuck", "Luck", minor_ability, major_ability, "d4e192475bb1a1045859c7664addd461");
        }


        static void createMadnessBlessing()
        {
            var minor_buff = library.CopyAndAdd<BlueprintBuff>("886c7407dc629dc499b9f1465ff382df", "WarpriestMadnessBlessingMinorBuff", ""); //confusion
            var confusion = library.Get<BlueprintAbility>("cf6c901fb7acc904e85c63b342e9c949");
            minor_buff.SetName("Madness Supremacy");
            minor_buff.SetDescription("At 1st level, as a swift action you can target a creature within 30 feet that has frightened or paralyzed condition. That condition is suspended for 1 round, and the chosen creature gains the confused condition instead. The confused creature rerolls any result other than “attack self” or “attack nearest creature.” The round spent confused counts toward the duration of the suspended effect. At the end of the confused round, the suspended condition resumes.");
            minor_buff.AddComponent(Common.createAddConditionImmunity(UnitCondition.Frightened));
            minor_buff.AddComponent(Common.createAddConditionImmunity(UnitCondition.Paralyzed));
            minor_buff.AddComponent(Helpers.Create<ConfusionControl.ControlConfusionBuff>(c => c.allowed_states = new ConfusionState[] { ConfusionState.AttackNearest, ConfusionState.SelfHarm }));


            var apply_minor_buff = Common.createContextActionApplyBuff(minor_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Minutes), dispellable: false);
            var minor_ability = Helpers.CreateAbility("WarpriestMadnessBlessingMinorAbility",
                                                      minor_buff.Name,
                                                      minor_buff.Description,
                                                      "",
                                                      minor_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Swift,
                                                      AbilityRange.Close,
                                                      Helpers.oneRoundDuration,
                                                      Helpers.savingThrowNone,
                                                      Helpers.CreateRunActions(apply_minor_buff),
                                                      confusion.GetComponent<AbilitySpawnFx>(),
                                                      confusion.GetComponent<SpellDescriptorComponent>(),
                                                      Common.createAbilityTargetCompositeOr(false, Common.createAbilityTargetHasCondition(UnitCondition.Paralyzed),
                                                                                                   Common.createAbilityTargetHasCondition(UnitCondition.Frightened)
                                                                                            )
                                                     );
            minor_ability.setMiscAbilityParametersSingleTargetRangedHarmful(works_on_allies: true);
            addBlessingResourceLogic(minor_ability);
            ConfusionState[] confusion_states = { ConfusionState.AttackNearest, ConfusionState.SelfHarm, ConfusionState.ActNormally, ConfusionState.DoNothing };
            string[] names = new string[] { "Attack Nearest", "Attack Self", "Act Normally", "Do Nothing" };

            BlueprintActivatableAbility[] major_abilities = new BlueprintActivatableAbility[confusion_states.Length];
            var hideous_laughter = library.Get<BlueprintBuff>("4b1f07a71a982824988d7f48cd49f3f8");
            var spend_resource = Common.createContextActionSpendResource(warpriest_blessing_resource, 1, warpriest_aspect_of_war_buff);
            var spend_resource_caster = Helpers.CreateConditional(Common.createContextConditionIsCaster(), spend_resource);

            for (int i = 0; i < confusion_states.Length; i++)
            {
               
                var buff = Helpers.CreateBuff($"WarpriestMadnessBlessingMajor{i+1}Buff",
                                              $"Control Madness ({names[i]})",
                                              "At 10th level, as a swift action you can choose one behavior for all confused creatures within 30 feet to exhibit (as if all creatures rolled the same result). This effect lasts for 1 round. You can use this ability even while you are confused.",
                                              "",
                                              hideous_laughter.Icon,
                                              hideous_laughter.FxOnStart,
                                              Helpers.Create<ConfusionControl.ControlConfusionBuff>(c => c.allowed_states = new ConfusionState[] { confusion_states[i] }),
                                              Helpers.CreateAddFactContextActions(spend_resource_caster)
                                              );
                

                var area_effect = library.CopyAndAdd<BlueprintAbilityAreaEffect>("4a15b95f8e173dc4fb56924fe5598dcf", $"WarpriestMadnessBlessingMajor{i+1}Area", "");//dirge of doom area
                area_effect.ReplaceComponent<AbilityAreaEffectBuff>(a =>
                                                                    {
                                                                        a.Buff = buff;
                                                                        a.Condition = Helpers.CreateConditionsCheckerOr();
                                                                    }
                                                                    );
                var caster_buff = Helpers.CreateBuff($"WarpriestMadnessBlessingMajor{i + 1}CasterBuff",
                                                     buff.Name,
                                                     buff.Description,
                                                     "",
                                                     buff.Icon,
                                                     null,
                                                     Common.createAddAreaEffect(area_effect)
                                                     );
                caster_buff.SetBuffFlags(BuffFlags.HiddenInUi);
                major_abilities[i] = Helpers.CreateActivatableAbility($"WarpriestMadnessBlessingMajor{i + 1}Ability",
                                                                     buff.Name,
                                                                     buff.Description,
                                                                     "",
                                                                     buff.Icon,
                                                                     caster_buff,
                                                                     AbilityActivationType.Immediately,
                                                                     CommandType.Free,
                                                                     null,
                                                                     Helpers.CreateActivatableResourceLogic(warpriest_blessing_resource, ResourceSpendType.Never)
                                                                     );
                major_abilities[i].Group = confusion_control_group;
                if (!test_mode)
                {
                    major_abilities[i].AddComponent(Common.createActivatableAbilityUnitCommand(CommandType.Swift));
                }
            }


            addBlessing("WarpriestBlessingMadness", "Madness",
                        Common.AbilityToFeature(minor_ability),
                        Helpers.CreateFeature("WarpriestBlessingMadness",
                                              "Control Madness",
                                              major_abilities[0].Description,
                                              "",
                                              major_abilities[0].Icon,
                                              FeatureGroup.None,
                                              Helpers.CreateAddFacts(major_abilities)
                                              ),
                        "c346bcc77a6613040b3aa915b1ceddec");
        }




    }
}
