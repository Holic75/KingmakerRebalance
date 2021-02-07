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
    public class DawnflowerAnchorite
    {
        static LibraryScriptableObject library => Main.library;
        internal static bool test_mode = false;
        static public BlueprintCharacterClass dawnflower_anchorite;
        static public BlueprintProgression dawnflower_anchorite_progression;

        static public BlueprintFeature channel_energy_domain_progression;
        static public BlueprintFeatureSelection credence;
        static public BlueprintFeature dervish_dance;
        static public BlueprintFeature divine_light;
        static public BlueprintFeature extra_invocations;
        static public BlueprintFeature solar_defense;
        static public BlueprintFeature solar_defense2;
        static public BlueprintFeature solar_weapons;

        static public BlueprintFeature solar_invocation, solar_invocation2, solar_invocation3;

        static public BlueprintFeature solar_invocation_move_action;
        static public BlueprintFeature solar_invocation_swift_action;
        static public BlueprintFeature bask_in_radiance;
        static public BlueprintFeature sunbeam;
        static public BlueprintFeature dawnflower_invocation;
        static public BlueprintFeatureSelection spellbook_selection;

        static public BlueprintAbilityResource solar_invocation_resource;
        


        internal static void createDawnflowerAnchoriteClass()
        {
            var ranger_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("cda0615668a6df14eb36ba19ee881af6");
            var druid_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");

            var sarenrae = library.Get<BlueprintFeature>("c1c4f7f64842e7e48849e5e67be11a1b");

         
            var savesPrestigeLow = library.Get<BlueprintStatProgression>("dc5257e1100ad0d48b8f3b9798421c72");
            var savesPrestigeHigh = library.Get<BlueprintStatProgression>("1f309006cd2855e4e91a6c3707f3f700");

            dawnflower_anchorite = Helpers.Create<BlueprintCharacterClass>();
            dawnflower_anchorite.name = "DawnflowerAnchoriteClass";
            library.AddAsset(dawnflower_anchorite, "");

            dawnflower_anchorite.LocalizedName = Helpers.CreateString("DawnflowerAnchoriteClass.Name", "Dawnflower Anchorite");
            dawnflower_anchorite.LocalizedDescription = Helpers.CreateString("DawnflowerAnchoriteClass.Description",
                "Although most of Sarenrae’s worshipers seek to aid others or seek out those villains who can be redeemed, a rare few instead raise their eyes in awe of the greatest of Sarenrae’s gifts to mortals: the sun. These Dawnflower anchorites hope to receive enlightenment in their retreat to the wilds so that they can, some day, return to civilization and use their newfound grace to help heal the church’s wounds."
                );
            dawnflower_anchorite.m_Icon = druid_class.Icon;
            dawnflower_anchorite.SkillPoints = druid_class.SkillPoints;
            dawnflower_anchorite.HitDie = DiceType.D8;
            dawnflower_anchorite.BaseAttackBonus = druid_class.BaseAttackBonus;
            dawnflower_anchorite.FortitudeSave = savesPrestigeLow;
            dawnflower_anchorite.ReflexSave = savesPrestigeHigh;
            dawnflower_anchorite.WillSave = savesPrestigeHigh;
            dawnflower_anchorite.ClassSkills = new StatType[] { StatType.SkillAthletics, StatType.SkillPerception, StatType.SkillLoreReligion, StatType.SkillLoreNature, StatType.SkillKnowledgeWorld, StatType.SkillKnowledgeArcana };
            dawnflower_anchorite.IsDivineCaster = true;
            dawnflower_anchorite.IsArcaneCaster = true;
            dawnflower_anchorite.PrestigeClass = true;
            dawnflower_anchorite.StartingGold = druid_class.StartingGold;
            dawnflower_anchorite.PrimaryColor = druid_class.PrimaryColor;
            dawnflower_anchorite.SecondaryColor = druid_class.SecondaryColor;
            dawnflower_anchorite.RecommendedAttributes = new StatType[] { };
            dawnflower_anchorite.EquipmentEntities = druid_class.EquipmentEntities;
            dawnflower_anchorite.MaleEquipmentEntities = druid_class.MaleEquipmentEntities;
            dawnflower_anchorite.FemaleEquipmentEntities = druid_class.FemaleEquipmentEntities;
            dawnflower_anchorite.StartingItems = druid_class.StartingItems;

            dawnflower_anchorite.ComponentsArray = ranger_class.ComponentsArray;
            dawnflower_anchorite.AddComponent(Common.createPrerequisiteAlignment( AlignmentMaskType.NeutralGood));
            dawnflower_anchorite.AddComponent(Helpers.Create<PrerequisiteMechanics.PrerequsiteOrAlternative>(p =>
                                                                                                            {
                                                                                                                p.base_prerequsite = Helpers.PrerequisiteStatValue(StatType.SkillLoreNature, 5);
                                                                                                                p.alternative_prerequsite = Helpers.PrerequisiteStatValue(StatType.SkillLoreReligion, 5);
                                                                                                            }
                                                                                                            )
                                             );
            dawnflower_anchorite.AddComponent(Helpers.PrerequisiteFeature(sarenrae));
            dawnflower_anchorite.AddComponent(Common.createPrerequisiteCasterTypeSpellLevel(true, 1, true));
            dawnflower_anchorite.AddComponent(Helpers.Create<SpellbookMechanics.PrerequisiteDivineCasterTypeSpellLevel>(p => { p.RequiredSpellLevel = 1; p.Group = Prerequisite.GroupType.Any; }));
            dawnflower_anchorite.AddComponent(Helpers.Create<SpellbookMechanics.PrerequisitePsychicCasterTypeSpellLevel>(p => { p.RequiredSpellLevel = 1; p.Group = Prerequisite.GroupType.Any; }));

            createDawnflowerAnchoriteProgression();
            dawnflower_anchorite.Progression = dawnflower_anchorite_progression;

            Helpers.RegisterClass(dawnflower_anchorite);
        }

        static BlueprintCharacterClass[] getDawnflowerAcnchoriteArray()
        {
            return new BlueprintCharacterClass[] { dawnflower_anchorite };
        }
        static void createDawnflowerAnchoriteProgression()
        {
            createSpellbookSelection();
            createChannelEnergyDomainProgression();
            createSolarInvocation(); //also dawnflower invocation and related credences
            createCredence();
            createSunbeam();

           
            dawnflower_anchorite_progression = Helpers.CreateProgression("DawnfloweAnchoriteProgression",
                                                               dawnflower_anchorite.Name,
                                                               dawnflower_anchorite.Description,
                                                               "",
                                                               dawnflower_anchorite.Icon,
                                                               FeatureGroup.None);
            dawnflower_anchorite_progression.Classes = getDawnflowerAcnchoriteArray();

            dawnflower_anchorite_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, channel_energy_domain_progression, solar_invocation),
                                                                      Helpers.LevelEntry(2, spellbook_selection),
                                                                      Helpers.LevelEntry(3, bask_in_radiance),
                                                                      Helpers.LevelEntry(4),
                                                                      Helpers.LevelEntry(5, solar_invocation_move_action),
                                                                      Helpers.LevelEntry(6, credence),
                                                                      Helpers.LevelEntry(7, sunbeam),
                                                                      Helpers.LevelEntry(8, credence),
                                                                      Helpers.LevelEntry(9),
                                                                      Helpers.LevelEntry(10, credence, dawnflower_invocation, solar_invocation_swift_action)
                                                                    };

            dawnflower_anchorite_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { channel_energy_domain_progression };
            dawnflower_anchorite_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(solar_invocation, solar_invocation_move_action, solar_invocation_swift_action),
                                                         Helpers.CreateUIGroup(spellbook_selection, credence, credence, credence),
                                                         Helpers.CreateUIGroup(bask_in_radiance, sunbeam, dawnflower_invocation)
                               
                         };
        }


        static void createSunbeam()
        {
            var sunbeam_resource = Helpers.CreateAbilityResource("DawnflowerAnchoriteSunbeamResource", "", "", "", null);
            sunbeam_resource.SetIncreasedByLevelStartPlusDivStep(1, 10, 1, 100, 0, 0, 0.0f, getDawnflowerAcnchoriteArray());

            var sunbeam_ability = library.CopyAndAdd<BlueprintAbility>("1fca0ba2fdfe2994a8c8bc1f0f2fc5b1", "DawnflowerAnchoriteSunbeam", "");
            sunbeam_ability.Type = AbilityType.SpellLike;
            sunbeam_ability.AddComponent(sunbeam_resource.CreateResourceLogic());

            sunbeam = Common.AbilityToFeature(sunbeam_ability, false);
            sunbeam.AddComponent(sunbeam_resource.CreateAddAbilityResource());
            sunbeam.SetDescription("At 7th level, a Dawnflower anchorite gains the ability to cast sunbeam once per day as a spell-like ability. His caster level is equal to his Hit Dice, and the save DC is Charisma-based. At 10th level, the Dawnflower anchorite can use this ability twice per day.");
        }


        static void createSpellbookSelection()
        {
            spellbook_selection = Helpers.CreateFeatureSelection("DawnflowerAnchoriteSpellbookSelection",
                                                                 "Dawnflower Anchorite Spellbook Selection",
                                                                 "Starting from 2nd level, a dawnflower anchorite gains new spells per day as if he had also gained a level in an an arcane, divine or psychic spellcasting class he belonged to before adding the prestige class. He does not, however, gain any other benefit a character of that class would have gained, except for additional spells per day, spells known (if he is a spontaneous spellcaster), and an increased effective level of spellcasting. If a character had more than one spellcasting class before becoming a dawnflower anchorite, he must decide to which class he adds the new level for purposes of determining spells per day.",
                                                                 "",
                                                                 null,
                                                                 FeatureGroup.EldritchKnightSpellbook);
            spellbook_selection.Obligatory = true;
            Common.addSpellbooksToSpellSelection2("DawnflowerAnchorite", 1, spellbook_selection, alchemist: false);
        }


        static void createCredence()
        {
            dervish_dance = Common.featureToFeature(NewFeats.dervish_dance, false);
            dervish_dance.SetDescription("The Dawnflower anchorite gains Dervish Dance as a bonus feat.");
            dervish_dance.AddComponent(Helpers.PrerequisiteNoFeature(NewFeats.dervish_dance));

            extra_invocations = Helpers.CreateFeature("ExtraInvocationsFeature",
                                                      "Extra Invocations",
                                                      "The Dawnflower anchorite can use solar invocation for twice as many rounds per day as normal.",
                                                      "",
                                                      null,
                                                      FeatureGroup.None,
                                                      Helpers.Create<IncreaseResourceAmountBySharedValue>(i => { i.Resource = solar_invocation_resource; i.Value = Helpers.CreateContextValue(AbilityRankType.Default); }),
                                                      Helpers.Create<IncreaseResourceAmountBySharedValue>(i => { i.Resource = solar_invocation_resource; i.Value = Helpers.CreateContextValue(AbilityRankType.StatBonus); }),
                                                      Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getDawnflowerAcnchoriteArray(), progression: ContextRankProgression.MultiplyByModifier, startLevel: 2),
                                                      Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, stat: StatType.Charisma, type: AbilityRankType.StatBonus),
                                                      Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Charisma)
                                                      );
            extra_invocations.ReapplyOnLevelUp = true;
            credence = Helpers.CreateFeatureSelection("CredenceFeatureSelection",
                                                      "Credence",
                                                      "Each Dawnflower anchorite develops a personal credence that guides his worship of Sarenrae. He gains a credence at 6th level and again every 2 class levels thereafter. Each credence can be chosen only once unless noted otherwise.",
                                                      "",
                                                      null,
                                                      FeatureGroup.None);

            credence.AllFeatures = new BlueprintFeature[] { dervish_dance, divine_light, extra_invocations, solar_defense, solar_defense2, solar_weapons };
        }


        static void createChannelEnergyDomainProgression()
        {
            //add to domains
            var druid = library.Get<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");
            var domain_selection = library.Get<BlueprintFeatureSelection>("48525e5da45c9c243a343fc6545dbdb9");
            var cleric_secondary_domain_selection = library.Get<BlueprintFeatureSelection>("43281c3d7fe18cc4d91928395837cd1e");
            var druid_domain_selection = library.Get<BlueprintFeatureSelection>("5edfe84c93823d04f8c40ca2b4e0f039");
            var blight_druid_domain_selection = library.Get<BlueprintFeatureSelection>("096fc02f6cc817a43991c4b437e12b8e");
            var cleric = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            ClassToProgression.addClassToDomains(dawnflower_anchorite, new BlueprintArchetype[0], ClassToProgression.DomainSpellsType.NoSpells, domain_selection, cleric);
            ClassToProgression.addClassToDomains(dawnflower_anchorite, new BlueprintArchetype[0], ClassToProgression.DomainSpellsType.NoSpells, cleric_secondary_domain_selection, cleric);
            ClassToProgression.addClassToDomains(dawnflower_anchorite, new BlueprintArchetype[0], ClassToProgression.DomainSpellsType.NoSpells, blight_druid_domain_selection, druid);
            ClassToProgression.addClassToDomains(dawnflower_anchorite, new BlueprintArchetype[0], ClassToProgression.DomainSpellsType.NoSpells, druid_domain_selection, druid);
            ChannelEnergyEngine.addClassToChannelEnergyProgression(dawnflower_anchorite);

            foreach (var p in Archetypes.StormDruid.domain_secondary_progressions)
            {
                ClassToProgression.addClassToProgression(dawnflower_anchorite, new BlueprintArchetype[0], ClassToProgression.DomainSpellsType.NoSpells, p, druid);
            }


            foreach (var p in Wildshape.wildshape_progressions)
            {
                ClassToProgression.addClassToProgression(dawnflower_anchorite, new BlueprintArchetype[0], ClassToProgression.DomainSpellsType.NoSpells, p, p.Classes[0]);
            }

            //remove dawnflower anchorite from animal domains
            var domain_animal_companion_progression = library.Get<BlueprintProgression>("125af359f8bc9a145968b5d8fd8159b8");
            domain_animal_companion_progression.Classes = domain_animal_companion_progression.Classes.RemoveFromArray(dawnflower_anchorite);
            var druid_animal_domain_selection = library.Get<BlueprintProgression>("a75ad4936e099c54881cf553e2110703");
            foreach (var a in druid_animal_domain_selection.GetComponents<AddFeatureOnClassLevel>())
            {
                a.AdditionalClasses = a.AdditionalClasses.RemoveFromArray(dawnflower_anchorite);
            }



            channel_energy_domain_progression = Helpers.CreateFeature("DawnflowerAnchoriteChannelDomainProgressionFeature",
                                                                      "Focused",
                                                                      "The character adds his Dawnflower anchorite class levels to his effective class level in the corresponding class for the purpose of determining the effects of the following features: "
                                                                      +"Domains, Channel Energy, Animal Companion and Wildshape.",
                                                                      "",
                                                                      Helpers.GetIcon("a5e23522eda32dc45801e32c05dc9f96"), //good hope
                                                                      FeatureGroup.None
                                                                      );

            var animal_companion_rank = library.Get<BlueprintFeature>("1670990255e4fe948a863bafd5dbda5d");
            for (int i = 1; i <= 10; i++)
            {
                channel_energy_domain_progression.AddComponent(Helpers.CreateAddFeatureOnClassLevelIfHasFact(animal_companion_rank, i, getDawnflowerAcnchoriteArray(), animal_companion_rank));
            }


        }


        static void createSolarInvocation()
        {
            solar_invocation_resource = Helpers.CreateAbilityResource("SolarInvocationResource", "", "", "", null);
            solar_invocation_resource.SetIncreasedByLevel(0, 2, getDawnflowerAcnchoriteArray());
            solar_invocation_resource.SetIncreasedByStat(0, StatType.Charisma);

            bask_in_radiance = Helpers.CreateFeature("BaskInRadianceFeature",
                                         "Bask in Radiance",
                                         "At 3rd level, when a Dawnflower anchorite uses his solar invocation ability, he can designate any number of allies within 30 feet to gain the bonuses granted by solar invocation for as long as he maintains the ability.",
                                         "",
                                         Helpers.GetIcon("0d657aa811b310e4bbd8586e60156a2d"), //cure critical wounds
                                         FeatureGroup.None
                                         );

            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/FontOfSpiritMagic.png");

            var effect_buff = Helpers.CreateBuff("SolarInvocationEffectBuff",
                                                 "Solar Invocation",
                                                 "A Dawnflower anchorite can harness the sun’s life-giving warmth to protect the innocent and smite the wicked. At 1st level, he can invoke the sun as a standard action, granting him a +1 competence bonus on attack rolls and damage rolls against evil creatures and adding 1 to the DC of his spells and spell-like abilities against evil creatures. The bonus on attack rolls and damage rolls increases to +2 at 5th level and +3 at 9th level. If a Dawnflower anchorite has an animal companion, the companion also gains the competence bonus on attack and damage rolls while this ability is active.\n"
                                                 + "A Dawnflower anchorite can invoke the sun for a number of rounds per day equal to twice his Dawnflower anchorite class level + his Charisma modifier. Maintaining this ability on subsequent rounds is a free action. These bonuses apply only when the Dawnflower anchorite is standing in an outdoor area.\n",
                                                 "",
                                                 icon,
                                                 null,
                                                 Helpers.Create<NewMechanics.AttackBonusAgainstAlignment>(a => { a.value = Helpers.CreateContextValue(AbilityRankType.Default); a.descriptor = ModifierDescriptor.Competence; a.alignment = AlignmentComponent.Evil; }),
                                                 Helpers.Create<NewMechanics.SpellsDCBonusAgainstAlignment>(a => { a.value = Helpers.CreateContextValue(AbilityRankType.StatBonus); a.descriptor = ModifierDescriptor.UntypedStackable; a.alignment = AlignmentComponent.Evil; }),
                                                 Helpers.Create<NewMechanics.DamageBonusAgainstAlignment>(a => { a.value = Helpers.CreateContextValue(AbilityRankType.Default); a.descriptor = ModifierDescriptor.Competence; a.alignment = AlignmentComponent.Evil; })
                                                 );

            var defense_effect_buff = Helpers.CreateBuff("SolarDefenseEffectBuff",
                                                 "Solar Defense",
                                                 "While using solar invocation, the Dawnflower anchorite adds his competence bonus on attack rolls to his Armor Class as a dodge bonus and to Reflex saving throws as a sacred bonus. The Dawnflower anchorite can select this credence twice—the second time he does so, the bonus to Armor Class and on Ref lex saves also applies to any companions who gain bonuses from the Dawnflower anchorite’s solar invocation.",
                                                 "",
                                                 Helpers.GetIcon("62888999171921e4dafb46de83f4d67d"), //shield of dawn
                                                 null,
                                                 Helpers.Create<NewMechanics.ArmorClassBonusAgainstAlignment>(a => { a.value = Helpers.CreateContextValue(AbilityRankType.Default); a.descriptor = ModifierDescriptor.Dodge; a.alignment = AlignmentComponent.Evil; }),
                                                 Helpers.Create<NewMechanics.ContextSavingThrowBonusAgainstAlignment>(a => { a.value = Helpers.CreateContextValue(AbilityRankType.Default); a.descriptor = ModifierDescriptor.Sacred; a.alignment = AlignmentComponent.Evil; a.save_type = SavingThrowType.Reflex; })                                               
                                                 );

            solar_defense = Helpers.CreateFeature("SolarDefenseFeature",
                                                  "Solar Defense I",
                                                  defense_effect_buff.Description,
                                                  "",
                                                  defense_effect_buff.Icon,
                                                  FeatureGroup.None
                                                  );

            solar_defense2 = Helpers.CreateFeature("SolarDefense2Feature",
                                                  "Solar Defense II",
                                                  defense_effect_buff.Description,
                                                  "",
                                                  defense_effect_buff.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.PrerequisiteFeature(solar_defense)
                                                  );

            var flaming = library.Get<BlueprintWeaponEnchantment>("30f90becaaac51f41bf56641966c4121");
            var solar_weapons_buff = Helpers.CreateBuff("SolarWeaponsEffectBuff",
                                     "Solar Weapons",
                                     "While using solar invocation, the Dawnflower anchorite can select one weapon (natural or manufactured) wielded by a creature affected by his solar invocation. That weapon gains the flaming weapon special ability for as long as the Dawnflower anchorite’s solar invocation persists.",
                                     "",
                                     LoadIcons.Image2Sprite.Create(@"AbilityIcons/FlameBlade.png"),
                                     null,
                                     Common.createBuffContextEnchantPrimaryHandWeapon(1, false, true, new BlueprintWeaponEnchantment[] {flaming })
                                     );

            solar_weapons = Helpers.CreateFeature("SolarWeaponsFeature",
                                      solar_weapons_buff.Name,
                                      solar_weapons_buff.Description,
                                      "",
                                      solar_weapons_buff.Icon,
                                      FeatureGroup.None
                                      );

            var apply_solar_defense = Common.createContextActionApplyBuff(defense_effect_buff, Helpers.CreateContextDuration(), false, true, true, false);
            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(effect_buff, solar_weapons_buff, solar_weapons);
            Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuffNoRemove(effect_buff,
                                                                                      Helpers.CreateConditional(Common.createContextConditionCasterHasFact(solar_defense),
                                                                                                                Helpers.CreateConditional(Helpers.CreateConditionsCheckerOr(Common.createContextConditionIsCaster(),
                                                                                                                                                                            Common.createContextConditionCasterHasFact(solar_defense2)
                                                                                                                                                                            ),
                                                                                                                                          apply_solar_defense)
                                                                                                               )
                                                                                      );
            var toggle = Common.createToggleAreaEffect(effect_buff, 30.Feet(),
                                                      Helpers.CreateConditionsCheckerOr(Common.createContextConditionIsCaster(), 
                                                                                        Helpers.Create<CompanionMechanics.ContextConditionIsPet>(),
                                                                                        Helpers.Create<NewMechanics.ContextConditionIsAllyAndCasterHasFact>(c => c.fact = bask_in_radiance)
                                                                                        ),
                                                      AbilityActivationType.WithUnitCommand,
                                                      UnitCommand.CommandType.Standard,
                                                      Common.createPrefabLink("dfc59904273f7ee49ab00e5278d86e16"),
                                                      Common.createPrefabLink("9353083f430e5a44a8e9d6e26faec248")
                                                      );
            toggle.AddComponents(solar_invocation_resource.CreateActivatableResourceLogic(ResourceSpendType.NewRound));
            toggle.DeactivateIfCombatEnded = true;
            toggle.DeactivateIfOwnerDisabled = true;
            toggle.Buff.SetBuffFlags(BuffFlags.HiddenInUi);
            toggle.Group = ActivatableAbilityGroupExtension.SolarInvocation.ToActivatableAbilityGroup();
            solar_invocation = Common.ActivatableAbilityToFeature(toggle, false);
            solar_invocation.AddComponents(solar_invocation_resource.CreateAddAbilityResource());

            solar_invocation2 = SaveGameFix.createDummyFeature("SolarInvocation2Feature", "");
            solar_invocation3 = SaveGameFix.createDummyFeature("SolarInvocation3Feature", "");
            var solar_invocation4 = SaveGameFix.createDummyFeature("SolarInvocation4Feature", "");
            solar_invocation2.HideInCharacterSheetAndLevelUp = true;
            solar_invocation2.HideInUI = true;
            solar_invocation3.HideInCharacterSheetAndLevelUp = true;
            solar_invocation3.HideInUI = true;
            solar_invocation4.HideInCharacterSheetAndLevelUp = true;
            solar_invocation4.HideInUI = true;
            solar_invocation.AddComponents(Helpers.CreateAddFeatureOnClassLevel(solar_invocation2, 5, getDawnflowerAcnchoriteArray()),
                                           Helpers.CreateAddFeatureOnClassLevel(solar_invocation3, 9, getDawnflowerAcnchoriteArray()));

            //add scaling to buffs and dawnflower invocation

            var dawnflower_invocation_buff = Helpers.CreateBuff("DawnflowerInvocationBuff",
                                                                "Dawnflower Invocation",
                                                                "At 10th level, a Dawnflower anchorite is able to, once per day, use his solar invocation ability for 1 minute. The Dawnflower anchorite can also use this ability indoors or underground. While using Dawnflower invocation, all bonuses gained from solar invocation increase by 1. Using this ability doesn’t cost rounds per day of solar invocation.",
                                                                "",
                                                                Helpers.GetIcon("e67efd8c84f69d24ab472c9f546fff7e"),
                                                                null,
                                                                Helpers.CreateAddFactContextActions(activated: Common.createContextActionApplyBuff(toggle.Buff, Helpers.CreateContextDuration(), false, true, true, false)),
                                                                Helpers.CreateAddFact(solar_invocation4)
                                                                );

            var dawnflower_invocation_resource = Helpers.CreateAbilityResource("DawnflowerInvocationResource", "", "", "", null);
            dawnflower_invocation_resource.SetFixedResource(1);
            var dawnflower_invocation_ability = Helpers.CreateAbility("DawnflowerInvocationAbility",
                                                                      dawnflower_invocation_buff.Name,
                                                                      dawnflower_invocation_buff.Description,
                                                                      "",
                                                                      dawnflower_invocation_buff.Icon,
                                                                      AbilityType.Supernatural,
                                                                      CommandType.Swift,
                                                                      AbilityRange.Personal,
                                                                      Helpers.oneMinuteDuration,
                                                                      "",
                                                                      Helpers.CreateRunActions(Common.createContextActionApplyBuff(dawnflower_invocation_buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false)),
                                                                      Common.createAbilitySpawnFx("52d413df527f9fa4a8cf5391fd593edd", anchor: AbilitySpawnFxAnchor.SelectedTarget, position_anchor: AbilitySpawnFxAnchor.None, orientation_anchor: AbilitySpawnFxAnchor.None),
                                                                      dawnflower_invocation_resource.CreateResourceLogic()
                                                                      );
            dawnflower_invocation_ability.setMiscAbilityParametersSelfOnly();
            dawnflower_invocation = Common.AbilityToFeature(dawnflower_invocation_ability, false);
            dawnflower_invocation.AddComponent(dawnflower_invocation_resource.CreateAddAbilityResource());

            toggle.AddComponent(Common.createActivatableAbilityRestrictionHasFact(dawnflower_invocation_buff, not: true));
            var config = Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureList,
                                                                     featureList: new BlueprintFeature[] { solar_invocation, solar_invocation2, solar_invocation3, solar_invocation4 }
                                                                     );
            var config2 = Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureList, type: AbilityRankType.StatBonus,
                                                         featureList: new BlueprintFeature[] { solar_invocation, solar_invocation4 }
                                                         );

            effect_buff.AddComponents(config, config2);
            defense_effect_buff.AddComponent(config);

            solar_invocation_move_action = Helpers.CreateFeature("SolarInvocationMoveActionFeature",
                                                                 "Solar Invocation: Move Action",
                                                                 "At 5th level, a Dawnflower anchorite can invoke the sun as a move action instead of a standard action. At 10th level, he can invoke the sun as a swift action.",
                                                                 "",
                                                                 Helpers.GetIcon("03a9630394d10164a9410882d31572f0"), //aid
                                                                 FeatureGroup.None,
                                                                 Helpers.Create<ActivatableAbilityActionTypeModierMechanics.ModifyActivatableAbilityGroupActionType>(m =>
                                                                 {
                                                                     m.group = ActivatableAbilityGroupExtension.SolarInvocation.ToActivatableAbilityGroup();
                                                                     m.action = CommandType.Move;
                                                                 }
                                                                 )
                                                                 );
            solar_invocation_swift_action = library.CopyAndAdd(solar_invocation_move_action, "SolarInvocationSwiftActionFeature", "");
            solar_invocation_swift_action.SetName("Solar Invocation: Swift Action");
            solar_invocation_swift_action.ReplaceComponent<ActivatableAbilityActionTypeModierMechanics.ModifyActivatableAbilityGroupActionType>(m => m.action = CommandType.Swift);

            divine_light = Helpers.CreateFeature("DivineLightFeature",
                                                 "Divine Light",
                                                 "The Dawnflower anchorite can use solar invocation indoors or underground. He can use solar invocation 2 additional rounds per day.",
                                                 "",
                                                 Helpers.GetIcon("be2062d6d85f4634ea4f26e9e858c3b8"), //cleanse
                                                 FeatureGroup.None,
                                                 Helpers.Create<IncreaseResourceAmount>(i => { i.Resource = solar_invocation_resource; i.Value = 2; })
                                                 );

            toggle.AddComponent(Helpers.Create<NewMechanics.OutdoorsUnlessHasFact>(o => o.fact = divine_light));
        }
    }
}
